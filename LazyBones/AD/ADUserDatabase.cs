using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.Collections;
using System.Linq.Expressions;

namespace LazyBones.AD
{
    public class ADUserDatabase : ADDatabase<ADUser>
    {
        public ADUserDatabase()
            : base()
        {
        }
        public ADUserDatabase(DirectoryEntry searchRoot)
            : base(searchRoot)
        {
        }
        public ADUserDatabase(DirectoryEntry searchRoot, SearchScope searchScope)
            : base(searchRoot, searchScope)
        {
        }
    }
}
