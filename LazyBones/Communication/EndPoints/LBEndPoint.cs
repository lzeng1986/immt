using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LazyBones.Communication.Server;

namespace LazyBones.Communication.EndPoints
{
    public abstract class LBEndPoint
    {
        public static LBEndPoint Create(Uri uri)
        {
            LBEndPoint endPoint;
            switch (uri.Scheme)
            {
                //case "tcp":
                //    endPoint = new TcpEndPoint();
                //    break;
                //case "udp":
                //    endPoint = new UdpEndPoint();
                //    break;
                default:
                    throw new ApplicationException("Unsupported protocol " + uri.Scheme + " in end point " + uri);
            }
            endPoint.Uri = uri;
            return endPoint;
        }

        public Uri Uri { get; private set; }

        //public abstract IServer CreateServer();
        //public abstract IClient CreateClient();
    }

    //class TcpEndPoint : LBEndPoint
    //{
    //    public override IServer CreateServer()
    //    {
    //        return new LBTcpServer();
    //    }

    //    public override IClient CreateClient()
    //    {
    //        return null;
    //    }
    //}

    //class UdpEndPoint : LBEndPoint
    //{
    //    public override Server.IServer CreateServer()
    //    {
    //        return new LBUdpServer();
    //    }

    //    public override Server.IClient CreateClient()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
