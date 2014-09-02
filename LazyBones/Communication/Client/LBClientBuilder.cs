using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LazyBones.Communication.Messages;

namespace LazyBones.Communication.Client
{
    public class LBClientBuilder<TMessage>
        where TMessage : class,ILBMessage
    {
        internal LBClientBuilder(SocketMode mode)
        {
            
        }
    }
}
