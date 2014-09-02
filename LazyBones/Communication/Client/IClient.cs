using System;

namespace LazyBones.Communication.Client
{
    public interface IClient : IDisposable
    {
        event EventHandler Connected;

        event EventHandler Disconnected;

        int ConnectTimeout { get; set; }

        bool IsConnected { get; }

        void Connect();

        void Disconnect();
    }
}
