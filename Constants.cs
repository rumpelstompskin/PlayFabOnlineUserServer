using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public static class Constants
    {
        // Server Tick Configuration
        public const int MAX_USERS = 1000;
        public const int TICKS_PER_SEC = 1;
        public const float MS_PER_TICK = 1000 / TICKS_PER_SEC;

        // Server Requirements
        public const string CERT_FILE = "server.pfx";
        public const string CERT_PWD = "1234";
        public const string AUTH_PWD = "Authorization Test Key";
    }
}
