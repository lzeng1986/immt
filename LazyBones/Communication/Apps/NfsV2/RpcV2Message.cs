using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace LazyBones.Communication.Apps.NfsV2
{
    [StructLayout(LayoutKind.Sequential)]
    class RpcV2MessageStruct
    {
        public int Xid;
        public RpcV2MsgType MsgType;
        public int RpcVersion;
        public int ProgId;
        public int ProgVersion;
        public int ProcId;
        public static readonly int Size = Marshal.SizeOf(typeof(RpcV2MessageStruct));
    }

    [StructLayout(LayoutKind.Sequential)]
    class RpcV2Call
    {
        public int Xid;
        public RpcV2MsgType MsgType;
        public int RpcVersion;
        public int ProgId;
        public int ProgVersion;
        public int ProcId;

    }

    public enum RpcV2MsgType
    {
        Call = 0,
        Reply = 1
    };

    enum RpcV2ReplyState
    {
        Accepted = 0,
        Denied = 1
    };

    enum RpcV2AcceptState
    {
        Success = 0, /* RPC executed successfully */
        ProgUnavail = 1, /* remote hasn’t exported program */
        ProgMismatch = 2, /* remote can’t support version # */
        ProcUnavail = 3, /* program can’t support procedure */
        Garbage = 4, /* procedure can’t decode params */
        SystemError = 5 /* errors like memory allocation failure */
    };

    enum RpcV2RejectState
    {
        RpcMismatch = 0, /* RPC version number != 2 */
        AuthError = 1 /* remote can’t authenticate caller */
    };

    enum RpcV2AuthState
    {
        OK = 0, /* success */
        /*
        * failed at remote end
        */
        BadCredential = 1, /* bad credential (seal broken) */
        RejectedCredential = 2, /* client must begin new session */
        BadVerifier = 3, /* bad verifier (seal broken) */
        RejectedVerifier = 4, /* verifier expired or replayed */
        TooWeak = 5, /* rejected for security reasons */
        /*
        * failed locally
        */
        InvalidResponse = 6, /* bogus response verifier */
        Failed = 7 /* reason unknown */
    };
}
