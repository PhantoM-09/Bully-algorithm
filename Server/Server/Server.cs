using FileWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class Server
    {
        static UdpClient fileUdpClient = new UdpClient(ServerSettings.ServerPortForFile);    //Надо будет закрыть
        static UdpClient globalUdpClient = new UdpClient(ServerSettings.Port);    //Надо будет закрыть
        static int globalUdpClientReceiveTimeOut = globalUdpClient.Client.ReceiveTimeout;
        static int okResponsesCount = 0;
        static int waitServersCount = 0;
        static int flagOnceStartVoting = 0;

        static int countSendToCoordinator = 0;

        public static void GetGrantToAccessFile()
        {
            while(true)
            {
                UdpClient udpClient = new UdpClient();
                fileUdpClient.Client.ReceiveTimeout = ServerSettings.TimeWaitClientAccessFile;
                try
                {
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ServerSettings.AccessFileServerIP), ServerSettings.AccessFileServerPort);
                    
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
                fileUdpClient.Client.ReceiveTimeout = ServerSettings.TimeWaitClientAccessFile;
                try
                {
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ServerSettings.AccessFileServerIP), ServerSettings.AccessFileServerPort);

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

        public static bool CheckExistenceCoordinator()
        {
            GetGrantToAccessFile();
            ConfigurationFile configurationFile = FileManager.ReadConfigurationFile();
            RevokeGrantToAccessFile();

            if(configurationFile.CoordinatorIp.Contains(ServerSettings.PatternToFind))
            {
                int currentOctet = GetFourthOctet(GetIP(ServerSettings.PatternToFind));
                int coordinatorOctet = GetFourthOctet(configurationFile.CoordinatorIp);

                if(currentOctet >= coordinatorOctet)
                {
                    EndVoting(new object());
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(CheckCoordinator);
                }

                return true;
            }

            return false;
        }
        //Пишет в общий файл свой IP
        public static bool WriteIPInFile()
        {
            try
            {
                string serverIP = GetIP(ServerSettings.PatternToFind);
                if (serverIP is null)
                    throw new Exception("Have no IP of this pattern");

                GetGrantToAccessFile();
                ConfigurationFile configurationFile = FileManager.ReadConfigurationFile();
                RevokeGrantToAccessFile();

                Console.WriteLine("Write IP in configuration file");

                string wasWrite = configurationFile.IPAllServers.Find(i => i.Equals(serverIP));
                if(wasWrite is null)
                {
                    configurationFile.IPAllServers.Add(serverIP);

                    GetGrantToAccessFile();
                    FileManager.WriteConfigurationFile(configurationFile);
                    RevokeGrantToAccessFile();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Write IP in file section: " + ex.Message);
                return false;
            }

        }

        //Читать файл и смотреть не записались ли все серверы
        public static bool WaitWhenAllServersWriteThemselves()
        {
            Console.WriteLine("Wait all servers");
            while (true)
            {
                try
                {
                    GetGrantToAccessFile();
                    ConfigurationFile configurationFile = FileManager.ReadConfigurationFile();
                    RevokeGrantToAccessFile();

                    if (configurationFile.IPAllServers.Count == ServerSettings.CountOfServers)
                    {
                        Console.WriteLine("Continue...");
                        return true;
                    }
                    Thread.Sleep(2500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" Wait all servers section: " + ex.Message);
                }
            }
        }

        //Является ли данный сервер обладателем меньшего идентификатора
        public static string IsLess()
        {
            Console.WriteLine("I'm less?");
            while (true)
            {
                try
                {
                    GetGrantToAccessFile();
                    ConfigurationFile configurationFile = FileManager.ReadConfigurationFile();
                    RevokeGrantToAccessFile();

                    string myIP = GetIP(ServerSettings.PatternToFind);
                    int myFourthOctet = GetFourthOctet(myIP);
                    int min = myFourthOctet;

                    foreach (var item in configurationFile.IPAllServers)
                    {
                        if (!item.Equals(myIP))
                        {
                            int otherForthOctet = GetFourthOctet(item);
                            if (otherForthOctet < min)
                                min = otherForthOctet;
                        }
                    }

                    if (myFourthOctet == min)
                    {
                        Console.WriteLine("I'm less");
                        return myIP;
                    }

                    Console.WriteLine("I'm not less");
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("IsLess section: " + ex.Message);
                }
            }
        }

        public static void StartVoting(string lessIP)
        {
            
            try
            {
                if (flagOnceStartVoting == 0)
                {
                    Console.WriteLine("Voting is start");
                    flagOnceStartVoting = 1;

                    GetGrantToAccessFile();
                    ConfigurationFile configurationFile = FileManager.ReadConfigurationFile();
                    RevokeGrantToAccessFile();

                    int myFourthOctet = GetFourthOctet(lessIP);
                    bool flagWasSend = false;
                    int countDublicate = 0;
                    foreach (var item in configurationFile.IPAllServers)
                    {
                        if (!item.Equals(lessIP))
                        {
                            int otherForthOctet = GetFourthOctet(item);
                            if (otherForthOctet > myFourthOctet)
                            {
                                ThreadPool.QueueUserWorkItem(Send, new TriadSendParams(item, ServerSettings.Port, "Start voting"));
                                ThreadPool.QueueUserWorkItem(Receive, "WaitOK");
                                Interlocked.Increment(ref waitServersCount);
                                flagWasSend = true;
                            }
                        }
                        else
                        {
                            countDublicate++;
                        }
                    }

                    if (flagWasSend)
                    {
                        ThreadPool.QueueUserWorkItem(WaitResponseFromServers);
                    }
                    else
                    {
                        if(countDublicate == configurationFile.IPAllServers.Count)
                            EndVoting(new object());
                    }
                }
                else
                {
                    flagOnceStartVoting = 0;
                    EndVoting(new object());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("StartVoting section: " + ex.Message);
            }
        }

        public static void Send(object state)
        {
            UdpClient udpClient = new UdpClient();
            TriadSendParams pairParams = state as TriadSendParams;
            if (pairParams is null)
                return;

            try
            {
                Console.WriteLine("Send: " + pairParams.message + " for: " + pairParams.IP);

                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(pairParams.IP), pairParams.port);

                string request = pairParams.message;
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

        public static void Receive(object state)
        {
            IPEndPoint ipPoint = null;
            try
            {
                if (state.ToString().Equals("WaitOK"))
                {
                    globalUdpClient.Client.ReceiveTimeout = ServerSettings.TimeWaitClientOK;
                }

                byte[] receivedData = globalUdpClient.Receive(ref ipPoint);
                globalUdpClient.Client.ReceiveTimeout = globalUdpClientReceiveTimeOut;

                ThreadPool.QueueUserWorkItem(HandlingRequest, new Request(ipPoint.Address.ToString(), Encoding.Default.GetString(receivedData)));

               
            }
            catch (Exception ex)
            {
                Console.WriteLine("Receive section: " + ex.Message);
            }
        }

        public static void HandlingRequest(object state)
        {
            int flagOneChange = 0;
            Request request = null;
            try
            {
                request = state as Request;
                if (request is null)
                    throw new Exception("Request is null");

                string requestText = request.requestText;

                Console.WriteLine("Sending message: " + requestText + "; From: " + request.IPWhoSendRequest);
                if (requestText.Equals("Start voting"))
                {
                    ThreadPool.QueueUserWorkItem(Send, new TriadSendParams(request.IPWhoSendRequest, ServerSettings.Port, "OK"));
                    Thread.Sleep(1500);

                    StartVoting(GetIP(ServerSettings.PatternToFind));
                    return;
                }
                else if(requestText.Equals("OK"))
                {
                    Interlocked.Increment(ref okResponsesCount);
                    Interlocked.Decrement(ref waitServersCount);
                    flagOneChange = 1;
                    return;
                }
                else if (requestText.Equals("End voting"))
                {
                    Console.WriteLine("Coordinator is: " + request.IPWhoSendRequest);
                    ThreadPool.QueueUserWorkItem(CheckCoordinator);
                }
                else if (requestText.Equals("Coordinator live"))
                {
                    ThreadPool.QueueUserWorkItem(Send, new TriadSendParams(request.IPWhoSendRequest, ServerSettings.Port, "Yes"));
                }
                else if (requestText.Equals("Yes"))
                {
                    countSendToCoordinator = 0;
                }
                else if (requestText.Equals("GetTime"))
                {
                    DateTime now = DateTime.Now;
                    string time = now.ToString("ddMMyyyy:mm:ss");
                    ThreadPool.QueueUserWorkItem(Send, new TriadSendParams(request.IPWhoSendRequest, ServerSettings.Port, time));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Handling request: " + ex.Message);
                if(request.requestText.Equals("OK") && flagOneChange == 0)
                {
                    Interlocked.Decrement(ref waitServersCount);
                }
            }
        }

        public static void WaitResponseFromServers(object state)
        {
            while(waitServersCount != 0)
            {

            }

            if (okResponsesCount > 0)
                ThreadPool.QueueUserWorkItem(WaitCoordinator);
            else
                ThreadPool.QueueUserWorkItem(EndVoting);
        }

        public static void WaitCoordinator(object state)
        {
            Console.WriteLine("Wait coordinator");
            Receive("Wait coordinator");


        }

        public static void EndVoting(object state)
        {
            Thread.Sleep(5000);
            Console.WriteLine("End voting");

            GetGrantToAccessFile();
            ConfigurationFile configurationFile = FileManager.ReadConfigurationFile();
            RevokeGrantToAccessFile();

            string coordinatorIP = GetIP(ServerSettings.PatternToFind);
            foreach (var item in configurationFile.IPAllServers)
            {
                if (!item.Equals(coordinatorIP))
                {
                    ThreadPool.QueueUserWorkItem(Send, new TriadSendParams(item, ServerSettings.Port, "End voting"));
                }
            }

            configurationFile.CoordinatorIp = coordinatorIP;

            GetGrantToAccessFile();
            FileManager.WriteConfigurationFile(configurationFile);
            RevokeGrantToAccessFile();

            Console.WriteLine("I'm coordinator");
        }

        public static void CheckCoordinator(object state)
        {
            Thread.Sleep(2000);
            int flagVoting = 0;
            while(flagVoting != 1)
            {

                GetGrantToAccessFile();
                ConfigurationFile configurationFile = FileManager.ReadConfigurationFile();
                RevokeGrantToAccessFile();

                Send(new TriadSendParams(configurationFile.CoordinatorIp, ServerSettings.Port, "Coordinator live"));
                countSendToCoordinator++;

                Thread.Sleep(5000);

                if(countSendToCoordinator == 3)
                {
                    flagOnceStartVoting = 0;
                    StartVoting(GetIP(ServerSettings.PatternToFind));
                    flagVoting = 1;
                }
            }
        }
        //Находит IP по заданному шаблону
        public static string GetIP(string patternToFind)
        {
            string hostName = Dns.GetHostName();
            IPHostEntry iPHostEntry = Dns.GetHostByName(hostName);
            IPAddress[] addresses = iPHostEntry.AddressList;

            foreach (var item in addresses)
            {
                if (item.ToString().Contains(patternToFind))
                    return item.ToString();
            }

            return null;
        }

        //Взятие четвертого октета из IP
        public static int GetFourthOctet(string IP)
        {
            return int.Parse(IP.Substring(IP.LastIndexOf('.') + 1));
        }
    }
}
