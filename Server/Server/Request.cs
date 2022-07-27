using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Request
    {
        public string IPWhoSendRequest;
        public string requestText;
        public Request(string IPWhoSendRequest, string requestText)
        {
            this.IPWhoSendRequest = IPWhoSendRequest;
            this.requestText = requestText;
        }
    }
}
