using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Program
    {
        static string ip = "write this ip where agent server";
        static void Main(string[] args)
        {
            GetTimeFromServer();
        }

        public static void GetTimeFromServer()
        {
            int port = 5555;
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            UdpClient udpClient = new UdpClient();
            try
            {
                string request = "GetTime";
                byte[] sentData = Encoding.Default.GetBytes(request);

                int numberOfSentBytes = udpClient.Send(sentData, sentData.Length, ipPoint);

                byte[] receiveData = udpClient.Receive(ref ipPoint);
                string response = Encoding.Default.GetString(receiveData);

                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                udpClient.Close();
            }
        }
    }
}
