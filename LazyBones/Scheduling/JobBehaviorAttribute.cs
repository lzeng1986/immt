using System;

namespace LazyBones.Scheduling
{
    /// <summary>
    /// 定义<see cref="IJob"/>实例的行为
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class JobBehaviorAttribute : Attribute
    {
        /// <summary>
        /// 创建一个默认的<see cref="JobBehaviorAttribute"/>实例
        /// </summary>
        public JobBehaviorAttribute()
        {
            SyncExecution = false;
            Durable = false;
        }
        /// <summary>
        /// 该<see cref="IJob"/>实例是否同步执行，默认为<see langword="false"/>
        /// </summary>
        public bool SyncExecution { get; set; }
        /// <summary>
        /// 该<see cref="IJob"/>实例是否在没有关联触发器时继续保存在调度器中，默认为<see langword="false"/>
        /// </summary>
        public bool Durable { get; set; }
        /// <summary>
        /// 在该<see cref="IJob"/>实例执行后，调度器是否存储对应<see cref="JobExecutionContext.DataMap"/>中已变化的值
        /// </summary>
        public bool StoreDataMap { get; set; }
        /// <summary>
        /// 对于该<see cref="IJob"/>实例的描述
        /// </summary>
        public string Description { get; set; }
    }
}
