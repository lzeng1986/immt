using System.Collections.Generic;

namespace LazyBones.Communication.Apps.NfsV2
{
    //rfc1057[1988]-[APPENDIX A: PORT MAPPER PROGRAM PROTOCOL]-[page 22]
    //程序号:100000 ** 版本号:2 ** 使用端口号:111
    //程序号、版本号、使用端口号均来自rfc1057-[Page 23]
    public class PortMapperSession : RpcV2Session
    {
        const int TCP = 6; /* protocol number for TCP/IP */
        const int UDP = 17; /* protocol number for UDP/IP */

        protected override int CheckProgAndVer(int progId, int progVer)
        {
            if (progId.Equals(100000))
            {
                return progVer.Equals(2) ? -1 : 2;
            }
            else
            {
                return -2;
            }
        }
        protected override void Call(int prog, int procedureId, RpcPacket rpcPacket, RpcPacket replyPacket)
        {
            switch (procedureId)
            {
                case 3:	// GetPort
                    GetPort(rpcPacket, replyPacket);
                    break;
                default:
                    throw new BadProcedureException();
            }
        }
        Dictionary<int, PortMapperRegisterItem> registeredItems = new Dictionary<int, PortMapperRegisterItem>();
        public void RegisterPort(int progId, int progVer, int registerPort)
        {
            registeredItems[progId] = new PortMapperRegisterItem { Vers = progVer, Port = registerPort };
        }
        void GetPort(RpcPacket callMsg, RpcPacket reply)
        {
            // rfc1057[1988] [page 24] 中定义 [10/2/2013 zliang]
            int prog = callMsg.GetInt32();// Given program [10/2/2013 zliang]
            int vers = callMsg.GetInt32();// version number of prog [10/2/2013 zliang]
            int protocol = callMsg.GetInt32();// transport protocol number [10/2/2013 zliang]
            int port = callMsg.GetInt32();// ignored [10/2/2013 zliang]

            int registeredPort = 0;

            if (protocol == UDP)
            {
                if (registeredItems.ContainsKey(prog))
                {
                    if (registeredItems[prog].Vers == vers)
                    {
                        registeredPort = registeredItems[prog].Port;
                    }
                }
            }
            reply.Append(registeredPort);
        }
        private struct PortMapperRegisterItem
        {
            public int Vers { get; set; }
            public int Port { get; set; }
        }
    }
}
