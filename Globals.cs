using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Globals
    {
        public static bool serverIsRunning = false;
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    }
}
