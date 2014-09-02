using System;

namespace LazyBones.Communication
{
    public class ErrorEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }

        public ErrorEventArgs(Exception ex)
        {
            Exception = ex;
        }
    }
}
