using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Networking;

namespace Client
{
    class Program
    {
        private static SocketClient socketClient = new SocketClient();


        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify a port number");
            }

            String port = args[0];

            socketClient.Connect(port);

            while (true)
            {
               string msg_string = Console.ReadLine();
               socketClient.Send(msg_string);
            }
               
        }

        
    }
}
