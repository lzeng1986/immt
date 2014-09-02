namespace LazyBones.Communication
{
    using System;
    using LazyBones.Log;

    /// <summary>
    /// 定义通讯基础类，实现了基本的状态转换
    /// </summary>
    public abstract class Communicator : ICommunicator
    {
        static Logger logger = LogManager.Current;
        protected Communicator()
        {
        }
        protected virtual void OnOpening()
        {
            var handler = Opening;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        protected virtual void OnOpened()
        {
            var handler = Opened;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        protected virtual void OnClosing()
        {
            var handler = Closing;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        protected virtual void OnClosed()
        {
            var handler = Closed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        protected virtual void OnError(Exception ex)
        {
            var handler = Error;
            if (handler != null)
                handler(this, new ErrorEventArgs(ex));
            logger.Error(ex, ex.Message);
        }
        protected abstract void OpenCommunicator();
        protected abstract void CloseCommunicator();
        public void Open()
        {
            if (State != CommunicatorState.Created)
            {
                throw new InvalidStateException(State);
            }
            try
            {
                State = CommunicatorState.Opening;
                OpenCommunicator();
                State = CommunicatorState.Opened;
            }
            catch (Exception e)
            {
                State = CommunicatorState.Error;
                OnError(e);
            }
        }
        public void Close()
        {
            if (State != CommunicatorState.Opened)
            {
                throw new InvalidStateException(State);
            }
            try
            {
                State = CommunicatorState.Closing;
                CloseCommunicator();
                State = CommunicatorState.Closed;
            }
            catch (Exception e)
            {
                State = CommunicatorState.Error;
                OnError(e);
            }
        }
        object locker = new object();
        volatile CommunicatorState state = CommunicatorState.Created;
        public CommunicatorState State
        {
            get { return state; }
            private set
            {
                if (CheckStateValid(value))
                {
                    state = value;
                    switch (state)
                    {
                        case CommunicatorState.Opening:
                            OnOpening();
                            break;
                        case CommunicatorState.Opened:
                            OnOpened();
                            break;
                        case CommunicatorState.Closing:
                            OnClosing();
                            break;
                        case CommunicatorState.Closed:
                            OnClosed();
                            break;
                    }
                }
            }
        }
        bool CheckStateValid(CommunicatorState newState)
        {
            if (state == newState)
                return false;
            switch (state)
            {
                case CommunicatorState.Created:
                    return true;
                case CommunicatorState.Opening:
                    if (newState == CommunicatorState.Opened || newState == CommunicatorState.Error)
                        return true;
                    break;
                case CommunicatorState.Opened:
                    if (newState == CommunicatorState.Closing || newState == CommunicatorState.Error)
                        return true;
                    break;
                case CommunicatorState.Closing:
                    if (newState == CommunicatorState.Closed || newState == CommunicatorState.Error)
                        return true;
                    break;
            }
            return false;
        }

        public event EventHandler Opening;
        public event EventHandler Opened;
        public event EventHandler Closing;
        public event EventHandler Closed;
        public event EventHandler<ErrorEventArgs> Error;
    }

    public class StateInvalidChangedException : Exception
    {
        public CommunicatorState State { get; private set; }
        public StateInvalidChangedException(CommunicatorState stateChangeTo)
            : base("无效的状态转换")
        {
            State = stateChangeTo;
        }
    }
    public class InvalidStateException : Exception
    {
        public CommunicatorState State { get; private set; }
        public InvalidStateException(CommunicatorState state)
            : base("当前状态无效")
        {
            State = state;
        }
    }
}
