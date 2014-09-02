using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.AD
{
    public class ADUserNotFoundException : Exception
    {
        public ADUserNotFoundException()
            : base("指定用户不存在")
        {

        }
    }
}
