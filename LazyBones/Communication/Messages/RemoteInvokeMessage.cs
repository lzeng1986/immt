using System;

namespace LazyBones.Communication.Messages
{
    [Serializable]
    class RemoteInvokeMessage : LBMessage
    {
        public string ClassName { get; set; }
        
        public string MethodName { get; set; }

        public object[] Parameters { get; set; }
    }
}
