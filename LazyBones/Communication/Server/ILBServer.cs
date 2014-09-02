using System;
using System.Collections.Generic;
using System.Net;

namespace LazyBones.Communication.Server
{
    public interface ILBServer
    {
        Dictionary<string, ILBSessionContext> Clients { get; }
        void Start();
        void Stop();
        IAppServer AppServer { get; set; }
        //IPEndPoint 
    }
}
