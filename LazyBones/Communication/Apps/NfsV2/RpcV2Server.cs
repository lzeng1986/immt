using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LazyBones.Communication.Server;

namespace LazyBones.Communication.Apps.NfsV2
{
    public class RpcV2Server : IAppServer
    {
        public LBBinding Binding
        {
            get { throw new NotImplementedException(); }
        }

        public string GetNewSessionId(ILBSessionContext contextSession)
        {
            throw new NotImplementedException();
        }

        public IAppSession CreateAppSession(ILBSessionContext contextSession)
        {
            throw new NotImplementedException();
        }

        #region IAppServer Members


        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public void OnStart()
        {
            throw new NotImplementedException();
        }

        public void OnStop()
        {
            throw new NotImplementedException();
        }

        #endregion


        public string GetNewClientId(System.Net.IPEndPoint remoteIPEP)
        {
            throw new NotImplementedException();
        }
    }
}
