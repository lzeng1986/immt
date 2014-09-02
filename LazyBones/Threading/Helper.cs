using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.Threading
{
    static class Helper
    {
        public static bool CheckJobStatusChange(TaskStatus from,TaskStatus to)
        {
            if (from == to) // 不能转换到相同状态 [10/30/2013 zliang]
                return false;
            switch (from)
            {
                case TaskStatus.Working:
                    return (to == TaskStatus.Canceled) || (to == TaskStatus.Completed);
                case TaskStatus.Waiting:
                    return true; ;
                case TaskStatus.Canceled:
                case TaskStatus.Completed:
                    return false;
                default:
                    throw new ApplicationException("Job status change check failed!");
            }
        }
    }
}
