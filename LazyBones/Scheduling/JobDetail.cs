using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LazyBones.Scheduling.Core;

namespace LazyBones.Scheduling
{
    /// <summary>
    /// 描述一个<see cref="IJob"/>实例的详细信息
    /// </summary>
    [Serializable]
    public class JobDetail
    {
        Key jobKey;
        Type jobType;
        LazyBones.Data.DataMap dataMap;
        bool shouldRecover;
        private JobDetail()
        {
        }
        public static JobDetail Create<TJob>(Key jobKey)
            where TJob : IJob
        {
            var attr = (JobBehaviorAttribute[])typeof(TJob).GetCustomAttributes(typeof(JobBehaviorAttribute), false);
            if (attr == null || attr.Length == 0)
                throw new ArgumentException(typeof(TJob) + "未添加JobBehaviorAttribute特定");
            var behavior = attr[0];
            var jobDetail = new JobDetail();
            jobDetail.Description = behavior.Description;
            jobDetail.SyncExecution = behavior.SyncExecution;
            jobDetail.StoreDataMap = behavior.StoreDataMap;
            jobDetail.Durable = behavior.Durable;
            jobDetail.jobKey = jobKey;
            return jobDetail;
        }
        public string JobName
        {
            get { return jobKey == null ? null : jobKey.Name; }
        }

        public string JobGroup
        {
            get { return jobKey == null ? null : jobKey.Group; }
        }
        public string FullName
        {
            get { return JobGroup + '.' + JobName; }
        }
        /// <summary>
        /// 对于该<see cref="IJob"/>实例的描述
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// 该<see cref="IJob"/>实例是否同步执行
        /// </summary>
        public bool SyncExecution { get; private set; }
        /// <summary>
        /// 该<see cref="IJob"/>实例是否在没有关联触发器时继续保存在调度器中
        /// </summary>
        public bool Durable { get; private set; }
        /// <summary>
        /// 在该<see cref="IJob"/>实例执行后，调度器是否存储对应<see cref="JobExecutionContext.DataMap"/>中已变化的值
        /// </summary>
        public bool StoreDataMap { get; private set; }
    }
}
