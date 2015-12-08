using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;


namespace MainPower.Com0com.Redirector
{
    class SocketServer
    {

        private Socket _socket;
        private byte[] _buffer = new byte[1024];
        private IPEndPoint ipEndPoint;

        /**
         * Name: SocketServer
         * Purpose: Constructor for a SocketServer socket to manage packets to and from client sockets
         * Parameters: N/A
         */
        public SocketServer()
        {
            _socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        }

        /**
         * Name: Bind
         * Purpose: Binds to input port number on localhost
         * Parameters: int port
         * Returns: void
         */
        public void Bind(string port)
        {
            if (port == null)
            {
                Console.WriteLine("Invalid port number");
                return;
            }

            int portNum = 0;
            if (!Int32.TryParse(port, out portNum))
            {
                Console.WriteLine("Error parsing port number: " + port);
                return;
            }


            // grab localhost ip... 127.0.0.1
            IPAddress ipAddr = Dns.GetHostEntry("").AddressList[0];

            // create a network endpoint
            ipEndPoint = new IPEndPoint(ipAddr, portNum);

            try
            {
                _socket.Bind(ipEndPoint);
            }

            catch (SocketException se)
            {
                Console.WriteLine("Error binding port number: ");
                Console.WriteLine(se.ToString());
            }

        }

        /**
         * Name: Listen
         * Purpose: Listens for incoming client connections
         * Parameters: int backlog -- differs for OS
         * Returns: void
         */

        public void Listen(int backlog)
        {
            if (_socket == null)
            {
                Console.WriteLine("No socket created");
                return;
            }

            _socket.Listen(backlog);
        }

        /**
         * Name: Disconnect
         * Purpose: Disconnects the client from the server
         * Parameters: N/A
         * Returns: void
         */
        public void Disconnect()
        {
            if (_socket == null)
            {
                Console.WriteLine("No socket created");
                return;
            }

            try
            {
                _socket.Shutdown(SocketShutdown.Receive); ;
                _socket.Close();
            }

            catch (SocketException se)
            {
                Console.WriteLine("Disconnect Error: " + se.ToString());
            }

        }

        /**
         * Name: Accept
         * Purpose: Accepts incoming connection from client sockets by setting up the callback function.
         * Parameters: N/A
         * Returns: void
         */
        public void Accept()
        {
            if (_socket == null)
            {
                Console.WriteLine("Server socket null");
                return;
            }

            try
            {
                _socket.BeginAccept(AcceptedCallback, null);
            }

            catch (SocketException se)
            {
                Console.WriteLine("Error accepting client: ");
                Console.WriteLine(se.ToString());
            }
        }

        /**
         * Name: AcceptedCallback
         * Purpose: Accepts incoming connection from client sockets.
         * Parameters: IAsyncResult result
         * Returns: void
         */
        private void AcceptedCallback(IAsyncResult result)
        {
            Socket clientSocket = _socket.EndAccept(result);

            if (clientSocket == null)
            {
                Console.WriteLine("Invalid client socket.");
                return;
            }

            Accept();
            byte[] buffer = new byte[1024];
            clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, clientSocket);
        }

        /**
         * Name: ReceiveCallback
         * Purpose: Handles packet receipt from client socket.
         * Parameters: IAsyncResult result
         * Returns: void
         */
        private void ReceiveCallback(IAsyncResult result)
        {

            if (_socket == null)
            {
                Console.WriteLine("Server socket null.");
                return;
            }

            Socket clientSocket = result.AsyncState as Socket;

            if (clientSocket == null)
            {
                Console.WriteLine("Invalid client socket.");
                return;
            }

            int bufferSize = clientSocket.EndReceive(result);
            byte[] packet = new byte[bufferSize];
            Array.Copy(_buffer, packet, packet.Length);

            try
            {
                clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, clientSocket);
                string str = System.Text.Encoding.Default.GetString(_buffer);
                Console.WriteLine("Client Socket: " + str);

                // clear buffer
                Array.Clear(_buffer, 0, _buffer.Length);
            }

            catch (SocketException se)
            {
                Console.WriteLine("Error receiving content: ");
                Console.WriteLine(se.ToString());
            }
        }

    }
}
