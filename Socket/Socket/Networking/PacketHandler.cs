using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server.Networking
{
    public static class PacketHandler
    {
        public static void Handle(byte[] packet, Socket clientSocket)
        {
            short packetLength = BitConverter.ToInt16(packet, 0);
            short packetType = BitConverter.ToInt16(packet, 2);

            Console.WriteLine("Received packet! Length: {0} | Type: {1}", packetLength, packetType);
        }
    }
}
