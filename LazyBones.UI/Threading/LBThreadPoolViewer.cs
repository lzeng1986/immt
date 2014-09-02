using System;
using System.Windows.Forms;

namespace LazyBones.Threading
{
    public partial class LBThreadPoolViewer : Form
    {
        LBThreadPool threadPool;
        public LBThreadPool ThreadPool
        {
            get
            {
                return threadPool;
            }
            set
            {
                if (threadPool != null)
                {
                    threadPool.Idle -= threadPool_Idle;

                    //threadPool.ThreadPoolInfoChanged -= threadPool_ThreadPoolInfoChanged;
                }
                threadPool = value;
                if (threadPool != null)
                {
                    threadPool.Idle += threadPool_Idle;
                    //threadPool.ThreadPoolInfoChanged += threadPool_ThreadPoolInfoChanged;
                }
                numericMaxThreads.Enabled = numericMinThreads.Enabled = threadPool != null;
            }
        }

        void threadPool_ThreadPoolInfoChanged(object sender, EventArgs e)
        {
            this.Invoke(new EventHandler(timerPerformance_Tick), null, null);
        }
        void threadPool_Idle(object sender, EventArgs e)
        {
            this.Invoke(new EventHandler(timerPerformance_Tick), null, null);
        }
        public LBThreadPoolViewer()
        {
            InitializeComponent();
            ThreadPool = null;
        }

        private void numericMaxThreads_ValueChanged(object sender, EventArgs e)
        {
            if (threadPool.Concurrency != (int)numericMaxThreads.Value)
                threadPool.Concurrency = (int)numericMaxThreads.Value;
            numericMaxThreads.Value = threadPool.Concurrency;
        }

        private void numericMinThreads_ValueChanged(object sender, EventArgs e)
        {
            if (threadPool.MinThreads != (int)numericMinThreads.Value)
                threadPool.MinThreads = (int)numericMinThreads.Value;
            numericMinThreads.Value = threadPool.MinThreads;
        }

        private void timerPerformance_Tick(object sender, EventArgs e)
        {
            //var info = threadPool.PoolInfo;
            //labelJobInQueue.Text = info.JobInQueueCount.ToString();
            //labelJobInPool.Text = info.JobInPoolCount.ToString();
            //labelThreadsInPool.Text = info.ThreadsInPoolCount.ToString();
            //labelWorkThreads.Text = info.WorkingThreadsCount.ToString();
            //numericMaxThreads.Value = threadPool.Concurrency;
            //numericMinThreads.Value = threadPool.MinThreads;

            //threadPoolInfoViewThread.AddValue(info.ThreadsInPoolCount, info.WorkingThreadsCount);
            //threadPoolInfoViewJob.AddValue(info.JobInPoolCount, info.JobInQueueCount);

            labelStatus.Text = threadPool.IsIdle ? "空闲" : "运行";
            labelTime.Text = threadPool.IsIdle ? "空闲时长 " + threadPool.IdleTime : "运行时长 " + threadPool.ExeTime;
        }
        private void FormImmtThreadPoolManage_Load(object sender, EventArgs e)
        {
            timerPerformance.Start();
        }
    }
}
