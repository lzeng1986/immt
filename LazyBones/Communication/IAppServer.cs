
using System.Net;
namespace LazyBones.Communication
{
    public interface IAppServer
    {
        LBBinding Binding { get; }
        string Name { get; }
        string GetNewClientId(IPEndPoint remoteIPEP);
    }
}
