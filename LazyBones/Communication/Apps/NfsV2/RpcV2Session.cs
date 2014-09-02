using System;

namespace LazyBones.Communication.Apps.NfsV2
{
    public abstract class RpcV2Session : IAppSession
    {
        public virtual void Initialize()
        {
        }

        public void Process(RpcPacket message)
        {
            // 读取rpc消息头 [10/30/2013 zliang]
            var xid = message.GetInt32();
            var msgType = message.GetInt32();
            var rpcVersion = message.GetInt32();
            var programId = message.GetInt32();
            var programVersion = message.GetInt32();
            var procedureId = message.GetInt32();

            var replyPacket = new RpcPacket(new byte[0]);
            replyPacket.Append(xid);    // rpc_msg.XID
            replyPacket.Append(1);		// rpc_msg.REPLY

            if (rpcVersion != 2)
            {
                replyPacket.Append(1);		// rpc_msg.reply_body.MSG_DENIED
                replyPacket.Append(0);		// rpc_msg.reply_body.rejected_reply.RPC_MISMATCH
                replyPacket.Append(2);		// rpc_msg.reply_body.rejected_reply.mismatch_info.low
                replyPacket.Append(2);		// rpc_msg.reply_body.rejected_reply.mismatch_info.low
            }
            else
            {
                replyPacket.Append(0);		// rpc_msg.reply_body.MSG_ACCEPTED
                //AddNullAuthentication
                replyPacket.Append(0);		// rpc_msg.reply_body.accepted_reply.opaque_auth.NULL
                replyPacket.Append(0);		// rpc_msg.reply_body.accepted_reply.opaque_auth.<datsize>
                if (msgType != 0)
                {
                    replyPacket.Append(4); //rpc_msg.reply_body.accept_stat.GARBAGE_ARGS = 4
                }
                else
                {
                    var result = CheckProgAndVer(programId, programVersion);
                    if (result == -2)
                    {
                        //rpc_msg.reply_body.accept_stat.PROG_UNAVAIL = 1
                        //* remote hasn’t exported program
                        replyPacket.Append(1);
                    }
                    else if (result >= 0)
                    {
                        //rpc_msg.reply_body.accept_stat.PROG_MISMATCH = 2
                        //* remote can’t support version #
                        replyPacket.Append(2);
                        replyPacket.Append(result);	// rpc_msg.reply_body.accepted_reply.mismatch_info.low
                        replyPacket.Append(result);	// rpc_msg.reply_body.accepted_reply.mismatch_info.high
                    }
                    else
                    {
                        try
                        {
                            replyPacket.Append(0); //rpc_msg.reply_body.accept_stat.SUCCESS = 0
                            CrackCredentials(message);
                            CrackVerifier(message);
                            Call(programId, procedureId, message, replyPacket);
                        }
                        catch (BadProcedureException)
                        {
                            replyPacket.Append(3); //rpc_msg.reply_body.accept_stat.PROC_UNAVAIL = 3
                        }
                    }
                }
            }
            //operation.SendMessage(replyPacket);
        }

        public void MessageSent(RpcPacket sentMessage)
        {
        }

        void CrackCredentials(RpcPacket callPacket)
        {
            int flavor = callPacket.GetInt32();
            int length = callPacket.GetInt32();
            callPacket.Jump(length);
        }
        void CrackVerifier(RpcPacket callPacket)
        {
            int flavor = callPacket.GetInt32();
            int length = callPacket.GetInt32();
            callPacket.Jump(length);
        }
        protected abstract void Call(int progId, int procedureId, RpcPacket rpcPacket, RpcPacket replyPacket);
        /// <summary>
        /// 检查程序号及其版本号是否正确
        /// </summary>
        /// <returns>正确返回-1，程序号错误返回-2，版本号错误返回正确的版本号</returns>
        protected abstract int CheckProgAndVer(int progId, int progVer);

        #region IAppSession Members


        public void ProcessMessage(Messages.ILBMessage message)
        {
            throw new NotImplementedException();
        }

        public void MessageSent(Messages.ILBMessage sentMessage)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
