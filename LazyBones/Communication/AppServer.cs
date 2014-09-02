using System;

namespace LazyBones.Communication
{
    public abstract class AppServer
    {
        public DateTimeOffset CreateTime { get; private set; }
        public DateTimeOffset StartTime { get; private set; }

        public virtual string Name
        {
            get { return this.GetType().Name; }
        }

        protected AppServer()
        {
            CreateTime = DateTimeOffset.Now;
        }

        IServer server;
        internal void Initialize(IServer server)
        {
            this.server = server;
            server.Opened += server_Opened;
            server.Closed += server_Closed;
        }

        void server_Closed(object sender, EventArgs e)
        {
            OnStop();
            server.Opened -= server_Opened;
            server.Closed -= server_Closed;
        }

        void server_Opened(object sender, EventArgs e)
        {
            StartTime = DateTimeOffset.Now;
            OnStart();
        }

        public void Start()
        {
            server.Open();
        }

        public void Stop()
        {
            server.Close();
        }

        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
    }
}
