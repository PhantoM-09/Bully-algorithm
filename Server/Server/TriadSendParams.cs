using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class TriadSendParams
    {
        public string IP;
        public int port;
        public string message;
        public TriadSendParams(string IP, int port, string message)
        {
            this.IP = IP;
            this.port = port;
            this.message = message;
        }
    }
}
