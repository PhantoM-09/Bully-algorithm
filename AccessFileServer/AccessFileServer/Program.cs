using System;

namespace AccessFileServer
{
    class Program
    {
        static void Main(string[] args)
        {
            while(true)
            {
                AcessControl.Receive();
            }
        }
    }
}
