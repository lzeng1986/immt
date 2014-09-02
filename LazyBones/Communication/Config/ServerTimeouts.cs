using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.Communication.Config
{
    class ServerTimeouts
    {
        public int SendTimeOut { get; set; }

        public int ReveiveTimeOut { get; set; }

        public int ConnectionTimeOut { get; set; }

        public int IdleSessionTimeOut { get; set; }
    }
}
