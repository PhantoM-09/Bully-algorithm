using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AccessFileServer
{
    public class AcessControl
    {
        private static bool isBusy = false;
        private static int port = 4444;
        private static UdpClient globalUdpClient = new UdpClient(port);
        private static int portOther = 3333;
        public static void Receive()
        {
            IPEndPoint ipPoint = null;
            try
            {
                Console.WriteLine("Wait request...");
                byte[] receiveData = globalUdpClient.Receive(ref ipPoint);

                string request = Encoding.Default.GetString(receiveData);
                if (request.Equals("Get grant to access a file"))
                {
                    if (isBusy)
                    {
                        Send(ipPoint.Address.ToString(), "File is busy");
                        Console.WriteLine("\nRequest: ");
                        Console.WriteLine("  From: " + ipPoint.Address.ToString());
                        Console.WriteLine("  Message: " + request);
                        Console.WriteLine("Response: File is busy\n");
                    }
                    else
                    {
                        isBusy = true;
                        Send(ipPoint.Address.ToString(), "Granting access to file");
                        Console.WriteLine("\nRequest: ");
                        Console.WriteLine("  From: " + ipPoint.Address.ToString());
                        Console.WriteLine("  Message: " + request);
                        Console.WriteLine("Response: Granting access to file\n");
                    }
                    return;
                }
                else if(request.Equals("Revoke grant to access a file"))
                {
                    isBusy = false;
                    Send(ipPoint.Address.ToString(), "Revoking access to file");
                    Console.WriteLine("\nRequest: ");
                    Console.WriteLine("  From: " + ipPoint.Address.ToString());
                    Console.WriteLine("  Message: " + request);
                    Console.WriteLine("Response: Revoking access to file\n");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Receive section: " + ex.Message);
            }
        }

        public static void Send(string IP, string message)
        {
            UdpClient udpClient = new UdpClient();
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(IP), portOther);

                string request = message;
                byte[] sentData = Encoding.Default.GetBytes(request);

                udpClient.Send(sentData, sentData.Length, ipPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send section: " + ex.Message);
            }
            finally
            {
                udpClient.Close();
            }
        }
    }
}
