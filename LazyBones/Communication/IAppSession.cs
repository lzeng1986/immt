using System;
using LazyBones.Communication.Messages;

namespace LazyBones.Communication
{
    public interface IAppSession : IDisposable
    {
        void Initialize();
        void ProcessMessage(ILBMessage message);
        void MessageSent(ILBMessage sentMessage);
    }
}
