using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.DirectoryServices;
using System.Globalization;

namespace LazyBones.AD
{   
    class ADQuery<T> : IQueryable<T>
    {
        IADDatabase database;
        string queryString = string.Empty;
        readonly Expression expression;
        static HashSet<string> properties = new HashSet<string>();
        Type originalType;
        public ADQuery(Expression ex)
        {
            this.expression = ex;
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return Query();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public Expression Expression
        {
            get { return expression; }
        }

        public IQueryProvider Provider
        {
            get { return new ADQueryProvider(); }
        }

        IEnumerator<T> Query()
        {
            Parse();
            return GetResult();
        }

        void Parse()
        {
            Parse(this.expression);
        }
        void Parse(Expression ex)
        {
            if (ex is ConstantExpression)
            {
                var express = ex as ConstantExpression;
                database = express.Value as IADDatabase;
            }
            else if (ex is MethodCallExpression)
            {
                var express = ex as MethodCallExpression;
                if (express.Method.DeclaringType != typeof(Queryable))
                    throw new NotSupportedException("Detected invalid top-level method-call.");

                Parse(express.Arguments[0]);

                switch (express.Method.Name)
                {
                    case "Where":
                        queryString = ParsePredicate(express.Arguments[1]);
                        break;
                    case "Select":
                        BuildSelect(((UnaryExpression)express.Arguments[1]).Operand as LambdaExpression);
                        break;
                    case "First":
                    case "FirstOrDefault":
                        BuildFirst(((UnaryExpression)express.Arguments[1]).Operand as LambdaExpression);
                        break;
                    default:
                        throw new NotSupportedException("不支持的LINQ操作 : " + express.Method.Name);
                }
            }
            else
                throw new NotSupportedException("检测到不支持的表达式类型");
        }
        string ParsePredicate(Expression e)
        {
            var sb = new StringBuilder();
            sb.Append("(");            
            if (e is BinaryExpression)    //解析二元表达式
            {
                var format = GetPredicateStringFormat(e.NodeType);
                var binEx = e as BinaryExpression;
                if (e.NodeType == ExpressionType.AndAlso || e.NodeType == ExpressionType.OrElse)   //逻辑运算符
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, format, ParsePredicate(binEx.Left), ParsePredicate(binEx.Right));
                }
                else  //关系运算符
                {
                    if (binEx.Left is MemberExpression && ((MemberExpression)binEx.Left).Member.DeclaringType == originalType)
                    {
                        var attrib = GetQueryFieldName(((MemberExpression)binEx.Left).Member);
                        var val = Expression.Lambda(binEx.Right).Compile().DynamicInvoke().ToString();
                        val = val.Replace("(", "0x28").Replace(")", "0x29").Replace(@"\", "0x5c"); //将LDAP中不能使用的字符转换成转译字符
                        sb.AppendFormat(CultureInfo.InvariantCulture, format, attrib,val);
                    }
                    else
                        throw new NotSupportedException("必须针对实体类型进行查询，且实体类型需放在关系运算符左边");
                }
            }
            else if (e is UnaryExpression) //解析一元表达式
            {
                var format = GetPredicateStringFormat(e.NodeType);
                var unaryEx = e as UnaryExpression;
                sb.AppendFormat(CultureInfo.InvariantCulture, format, ParsePredicate(unaryEx.Operand));
            }
            else if (e is MethodCallExpression) //解析调用表达式
            {
                var callEx = e as MethodCallExpression;
                if (callEx.Method.DeclaringType != typeof(string))
                    throw new NotSupportedException("不能将该类型转换成AD查询语句 " + callEx.Method.DeclaringType.FullName);

                var member = (callEx.Object as MemberExpression).Member;
                var val = Expression.Lambda(callEx.Arguments[0]).Compile().DynamicInvoke().ToString();
                val = val.Replace("(", "0x28").Replace(")", "0x29").Replace(@"\", "0x5c"); //将LDAP中不能使用的字符转换成转译字符
                switch (callEx.Method.Name)
                {
                    case "Contains":
                        sb.AppendFormat(CultureInfo.InvariantCulture, "{0}=*{1}*", GetQueryFieldName(member), val);
                        break;
                    case "StartsWith":
                        sb.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}*", GetQueryFieldName(member), val);
                        break;
                    case "EndsWith":
                        sb.AppendFormat(CultureInfo.InvariantCulture, "{0}=*{1}", GetQueryFieldName(member), val);
                        break;
                    default:
                        throw new NotSupportedException("检测到不支持的字符串查询方法，目前只支持Contains,StartsWith,EndsWith");
                }
            }
            else
                throw new NotSupportedException("Unsupported query expression detected. Cannot translate to LDAP equivalent.");
            sb.Append(")");
            return sb.ToString();
        }
        //根据ExpressionType返回比较字符串模式
        string GetPredicateStringFormat(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Not:
                    return "!{0}";
                case ExpressionType.AndAlso:
                    return "&{0}{1}";
                case ExpressionType.OrElse:
                    return "!{0}{1}";
                case ExpressionType.Equal:
                    return "{0}={1}";
                case ExpressionType.NotEqual:
                    return "!({0}={1})";
                case ExpressionType.GreaterThanOrEqual:
                    return "{0}>={1}";
                case ExpressionType.GreaterThan:
                    return "&({0}>={1})(!({0}={1}))";
                case ExpressionType.LessThanOrEqual:
                    return "{0}<={1}";
                case ExpressionType.LessThan:
                    return "&({0}<={1})(!({0}={1}))";
                default:
                    throw new NotSupportedException("检测到不支持的运算符" + type);
            }
        }
        string GetQueryFieldName(System.Reflection.MemberInfo member)
        {
            var da = member.GetCustomAttributes(typeof(ADPropertyAttribute), false) as ADPropertyAttribute[];
            if (da != null && da.Length != 0 && da[0] != null)
            {
                if (da[0].PropertyType == ADPropertyType.ActiveDs)
                    throw new InvalidOperationException("Can't execute query filters for ADSI properties.");
                else
                    return da[0].Name;
            }
            else
                return member.Name;
        }
        void BuildSelect(LambdaExpression ex)
        {
            ////
            //// Store projection information including the compiled lambda for subsequent execution
            //// and a minimal set of properties to be retrieved (improves efficiency of queries).
            ////
            //project = ex.Compile();

            ////
            //// Original type is kept for reflection during querying.
            ////
            //originalType = p.Parameters[0].Type;

            ////
            //// Support for (anonymous) type initialization based on "member init expressions".
            ////
            //MemberInitExpression mi = p.Body as MemberInitExpression;
            //if (mi != null)
            //    foreach (MemberAssignment b in mi.Bindings)
            //        FindProperties(b.Expression);
            ////
            //// Support for identity projections (e.g. user => user), getting all properties back.
            ////
            //else
            //    foreach (PropertyInfo i in originalType.GetProperties())
            //        properties.Add(i.Name);
        }
        void BuildFirst(LambdaExpression ex)
        {

        }
        IEnumerator<T> GetResult()
        {
            var attr = (ADSchemaAttribute[])database.OriginalType.GetCustomAttributes(typeof(ADSchemaAttribute), false);
            if (attr == null || attr.Length == 0)
                throw new InvalidOperationException("查询类需要添加ADSchemaAttribute特性");

            string q = String.Format("(&(objectClass={0}){1})", attr[0].Schema, queryString);
            DirectorySearcher s = new DirectorySearcher(database.SearchRoot, q, properties.ToArray(), database.SearchScope);

            //Type helper = attr[0].ActiveDsHelperType;
            return null;
            foreach (SearchResult sr in s.FindAll())
            {
                DirectoryEntry e = sr.GetDirectoryEntry();

                //object result = Activator.CreateInstance(project == null ? typeof(T) : originalType);

                ///// *** UPDATE ***
                //DirectoryEntity entity = result as DirectoryEntity;
                //if (entity != null)
                //    entity.DirectoryEntry = e;

                //if (project == null)
                //{
                //    foreach (PropertyInfo p in typeof(T).GetProperties())
                //        AssignResultProperty(helper, e, result, p.Name);

                //    yield return (T)result;
                //}
                //else
                //{
                //    foreach (string prop in properties)
                //        AssignResultProperty(helper, e, result, prop);

                //    yield return (T)project.DynamicInvoke(result);
                //}
            }

            //DirectoryEntry root = _source.Root;
            //string q = String.Format("(&(objectClass={0}){1})", attr[0].Schema, query);
            //DirectorySearcher s = new DirectorySearcher(root, q, properties.ToArray(), _source.Scope);

            //if (_source.Log != null)
            //    _source.Log.WriteLine(q);

            //Type helper = attr[0].ActiveDsHelperType;

            //foreach (SearchResult sr in s.FindAll())
            //{
            //    DirectoryEntry e = sr.GetDirectoryEntry();

            //    object result = Activator.CreateInstance(project == null ? typeof(T) : originalType);

            //    /// *** UPDATE ***
            //    DirectoryEntity entity = result as DirectoryEntity;
            //    if (entity != null)
            //        entity.DirectoryEntry = e;

            //    if (project == null)
            //    {
            //        foreach (PropertyInfo p in typeof(T).GetProperties())
            //            AssignResultProperty(helper, e, result, p.Name);

            //        yield return (T)result;
            //    }
            //    else
            //    {
            //        foreach (string prop in properties)
            //            AssignResultProperty(helper, e, result, prop);

            //        yield return (T)project.DynamicInvoke(result);
            //    }
            //}
        }
    }
}
