using System.Collections.Generic;

namespace LazyBones.Threading
{
    /// <summary>
    /// 提供一个先入先出的ThreadJob处理队列，后一个ThreadJob只有在前一个ThreadJob完成之后才可开始
    /// </summary>
    public class JobFIFOGroup
    {
        volatile bool isBusy = false;
        LinkedList<Task> queue = new LinkedList<Task>();
        readonly LBThreadPool threadPool;
        /// <summary>
        /// 创建一个JobFIFOGroup，使用LazyBonesThreadPool.Default执行
        /// </summary>
        public JobFIFOGroup()
        {
            //threadPool = LBThreadPool.Default;
        }
        /// <summary>
        /// 创建一个JobFIFOGroup，使用指定线程池执行
        /// </summary>
        /// <param name="usedThreadPool">执行操作的线程池</param>
        public JobFIFOGroup(LBThreadPool usedThreadPool)
        {
            threadPool = usedThreadPool;
        }
        /// <summary>
        /// 获取处理队列是否处于执行状态
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
        }
        /// <summary>
        /// 将一个ThreadJob添加进处理队列
        /// </summary>
        /// <param name="job">添加的作业</param>
        public void Append(Task job)
        {
            lock (queue)
            {
                queue.AddLast(job);
            }
            ProcessNextJob();
        }
        /// <summary>
        /// 清空处理队列
        /// </summary>
        public void Clear()
        {
            lock (queue)
            {
                queue.Clear();
            }
        }
        /// <summary>
        /// 处理队列长度
        /// </summary>
        public int Count
        {
            get
            {
                return queue.Count;
            }
        }
        void ProcessNextJob()
        {
            if (isBusy)
                return;
            lock (queue)
            {
                if (queue.Count <= 0)
                    return;
                var job = queue.First.Value;
                queue.RemoveFirst();
                //job.Completed += JobCompleted;
                //job.Canceled += JobCompleted;
                //job.ErrorOccured += JobCompleted;
                //threadPool.RunAsync(job);
                isBusy = true;
            }
        }
        void JobCompleted(Task job)
        {
            isBusy = false;
            //job.Completed -= JobCompleted;
            //job.Canceled -= JobCompleted;
            //job.ErrorOccured -= JobCompleted;
            ProcessNextJob();
        }
    }
}
