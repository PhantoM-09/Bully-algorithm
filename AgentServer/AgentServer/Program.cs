using FileWork;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AgentServer
{
    class Program
    {
        static void Main(string[] args)
        {
            while(true)
            {
                TakeTimeToClient();
            }
            
        }

        public static void TakeTimeToClient()
        {
            int port = 5555;
            IPEndPoint ipPoint = null;
            UdpClient udpClient = new UdpClient(port);
            try
            {
                
                byte[] receiveData = udpClient.Receive(ref ipPoint);
                string request = Encoding.Default.GetString(receiveData);

                if(request.Equals("GetTime"))
                {
                    byte[] sentData = Encoding.Default.GetBytes(request);

                    GetGrantToAccessFile();
                    ConfigurationFile configurationFile = FileManager.ReadConfigurationFile();
                    RevokeGrantToAccessFile();

                    int numberOfSentBytes = udpClient.Send(sentData, sentData.Length, configurationFile.CoordinatorIp, port); ;

                    IPEndPoint ipP = null;
                    byte[] resp = udpClient.Receive(ref ipP);

                    udpClient.Send(resp, resp.Length, ipPoint);
                }
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

        static UdpClient fileUdpClient = new UdpClient(3333);

        public static void GetGrantToAccessFile()
        {
            while (true)
            {
                UdpClient udpClient = new UdpClient();
                fileUdpClient.Client.ReceiveTimeout = 1500;
                try
                {
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("write this ip where access file server"), 4444);

                    string request = "Get grant to access a file";
                    byte[] sentData = Encoding.Default.GetBytes(request);

                    //Console.WriteLine("Send request for get grant to access a file");
                    udpClient.Send(sentData, sentData.Length, ipPoint);

                    //Console.WriteLine("Wait response about get grant to access a file");
                    byte[] receiveData = fileUdpClient.Receive(ref ipPoint);
                    string response = Encoding.Default.GetString(receiveData);

                    //Console.WriteLine("Server response: " + response + "\n");

                    if (response.Equals("Granting access to file"))
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Check access of file: " + ex.Message);
                }
                finally
                {
                    udpClient.Close();
                }
            }

        }

        public static void RevokeGrantToAccessFile()
        {
            while (true)
            {
                UdpClient udpClient = new UdpClient();
                fileUdpClient.Client.ReceiveTimeout = 1500;
                try
                {
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("write this ip where access file server"), 4444);

                    string request = "Revoke grant to access a file";
                    byte[] sentData = Encoding.Default.GetBytes(request);

                    //Console.WriteLine("Send request for revoke grant to access a file");
                    udpClient.Send(sentData, sentData.Length, ipPoint);

                    //Console.WriteLine("Wait response about revoke grant to access a file");
                    byte[] receiveData = fileUdpClient.Receive(ref ipPoint);
                    string response = Encoding.Default.GetString(receiveData);

                    //Console.WriteLine("Server response: " + response);
                    if (response.Equals("Revoking access to file"))
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Check access of file: " + ex.Message);
                }
                finally
                {
                    udpClient.Close();
                }
            }

        }

        public static void StartActins()
        {
            FileManager.CreateConfigurationFile();
        }
    }
}
