using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ServerSettings
    {
        public static readonly string PatternToFind = "192.168.43";
        public static readonly int CountOfServers = 1;
        public static readonly int Port = 5555;
        public static readonly string AccessFileServerIP = "write this ip where access file server";
        public static readonly int AccessFileServerPort = 4444;
        public static readonly int ServerPortForFile = 3333;
        public static readonly int TimeWaitClientOK = 5000;
        public static readonly int TimeWaitClientAccessFile = 1500;
    }
}
