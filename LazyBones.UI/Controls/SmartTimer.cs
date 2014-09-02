using System;
using System.ComponentModel;
using System.Threading;
using LazyBones.Extensions;

namespace LazyBones.UI.Controls
{
    [DefaultEvent("Elapsed")]
    public class SmartTimer : Component
    {
        AsyncOperation asyncOperation;
        Timer timer;
        public SmartTimer()
        {
            InitializeComponent();
        }

        public SmartTimer(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
        void InitializeComponent()
        {
            timer = new Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);
            asyncOperation = AsyncOperationManager.CreateOperation(null);
        }
        System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components.Dispose();
                if (timer != null)
                    timer.Dispose();
                timer = null;
            }
            base.Dispose(disposing);
        }

        void Callback(object state)
        {
            asyncOperation.Post(OnElapsed, null);
        }
        void OnElapsed(object state)
        {
            Elapsed.SafeCall(this);
        }
        public event EventHandler Elapsed;
        void CheckDispose()
        {
            if (timer == null)
                throw new ObjectDisposedException(GetType().Name);
        }
        public void Once(int dueTime)
        {
            CheckDispose();
            timer.Change(dueTime, Timeout.Infinite);
        }
        public void Start(int dueTime, int period)
        {
            CheckDispose();
            timer.Change(dueTime, period);
        }

        public void Stop()
        {
            CheckDispose();
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
