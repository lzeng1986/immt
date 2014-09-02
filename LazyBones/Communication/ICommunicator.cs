using System;

namespace LazyBones.Communication
{
    /// <summary>
    /// 定义基础通讯接口
    /// </summary>
    public interface ICommunicator  //Open,Close两个方法应实现为阻塞方式
    {
        void Open();
        void Close();
        CommunicatorState State { get; }
        event EventHandler Opening;
        event EventHandler Opened;
        event EventHandler Closing;
        event EventHandler Closed;
        event EventHandler<ErrorEventArgs> Error;
    }
}
