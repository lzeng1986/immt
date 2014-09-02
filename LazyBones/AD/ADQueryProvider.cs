using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

namespace LazyBones.AD
{
    /// <summary>
    /// 提供LINQ查询
    /// </summary>
    public class ADQueryProvider : IQueryProvider
    {
        public readonly static DirectoryEntry DefaultSearchRoot = new DirectoryEntry("LDAP://" + Domain.GetCurrentDomain().Name);

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            return new ADQuery<TElement>(expression);
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            throw new NotImplementedException();
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}
