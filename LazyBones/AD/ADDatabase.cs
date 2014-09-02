using System;
using System.Collections.Generic;
using System.Linq;
using System.DirectoryServices;
using System.Linq.Expressions;
using System.Collections;

namespace LazyBones.AD
{   
    /// <summary>
    /// 代表LDAP的数据源，允许通过LINQ查询
    /// </summary>
    public class ADDatabase<T> : IQueryable<T>,IADDatabase
    {
        internal readonly DirectoryEntry searchRoot;
        internal readonly SearchScope searchScope;
        static HashSet<string> properties = new HashSet<string>();
        internal protected ADDatabase()
            : this(ADQueryProvider.DefaultSearchRoot, SearchScope.Subtree)
        {

        }
        internal protected ADDatabase(DirectoryEntry searchRoot)
            : this(searchRoot, SearchScope.Subtree)
        {

        }
        internal protected ADDatabase(DirectoryEntry searchRoot, SearchScope searchScope)
        {
            this.searchRoot = searchRoot;
            this.searchScope = searchScope;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ADQuery<T>(this.Expression).GetEnumerator();
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
            get { return Expression.Constant(this); }
        }

        public IQueryProvider Provider
        {
            get { return new ADQueryProvider(); }
        }

        public DirectoryEntry SearchRoot
        {
            get { return searchRoot; }
        }

        public SearchScope SearchScope
        {
            get { return searchScope; }
        }

        public Type OriginalType { get { return typeof(T); } }
    }
    /// <summary>
    /// ADDatabase提供的接口
    /// </summary>
    interface IADDatabase
    {
        DirectoryEntry SearchRoot { get; }
        SearchScope SearchScope { get; }
        Type OriginalType { get; }
    }
}
