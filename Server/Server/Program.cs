using FileWork;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            if(!BeforeStart())
            {
                DoStartActions();

                AfterStart();
            }
           
            while (true)
            {
                Server.Receive(new object());
            }

            Console.ReadKey();
        }

        public static bool BeforeStart()
        {
            return Server.CheckExistenceCoordinator();
        }

        public static void DoStartActions()
        {
            bool wasWrite = false;
            while(!wasWrite)
            {
                wasWrite = Server.WriteIPInFile();
            }
            
            Server.WaitWhenAllServersWriteThemselves();
        }

        public static void AfterStart()
        {
            string lessIP = null;
            if ((lessIP = Server.IsLess()) != null)
            {
                Server.StartVoting(lessIP);
            }
            else
            {
                while(true)
                {
                    Server.Receive(new object());
                }
            }
        }
    }
}

