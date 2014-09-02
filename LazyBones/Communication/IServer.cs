using System.Collections.Generic;
using System.Net;

namespace LazyBones.Communication
{
    public interface IServer : ICommunicator
    {
        Dictionary<string, IOperation> Clients { get; }
        EndPoint ListenEndPoint { get; }

    }
}
