using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Client.Networking
{
    class SocketClient
    {
        private Socket _socket;
        private byte[] _buffer;
        private IPEndPoint _ip;

        /**
         * Name: SocketClient
         * Purpose: Ctor for a client socket.
         * Parameters: N/A
         */
        public SocketClient()
        {
            _socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        }

        /**
         * Name: Connect
         * Purpose: Connects the client socket to the server socket
         * Parameters: string port_s -- the port number
         * Returns: void
         */
        public void Connect(string port_s)
        {
            int port = 0;
            bool parsed = Int32.TryParse(port_s, out port);

            if (!parsed)
                Console.WriteLine("Could not convert '{0)' to an integer.", port_s);

            IPAddress ipAddress = Dns.GetHostEntry("").AddressList[0]; // returns localhost
            _ip = new IPEndPoint(ipAddress, port);

            try
            {
                _socket.Connect(_ip);
                Console.WriteLine("Successfully connected to port " + port_s);
            }

            catch( SocketException se)
            {
                Console.WriteLine("Could not connect for the following reason: " + se.ToString());
            }
 
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

            if (!_socket.Connected)
            {
                Console.WriteLine("Not currently connected to the server.");
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
         * Name: ConnectCallback
         * Purpose: Initiates receipt state when server acknowledges connection
         * Parameters: IAsyncResult result
         * Returns: void
         */
        private void ConnectCallback(IAsyncResult result)
        {
            if (_socket == null)
            {
                Console.WriteLine("Client socket null.");
                return;
            }

            if (!_socket.Connected)
            {
                Console.WriteLine("Could not connect to the server.");
                return;
            }

            try
            {
                _buffer = new byte[1024];
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, null);
                Console.WriteLine("Connected to the server!");
            }

            catch (SocketException se)
            {
                Console.WriteLine("Could not connect for the following reason: " + se.ToString());
            }
        }


        /**
         * Name: ReceiveCallBack
         * Purpose: Handles case when the server sends a response back, enabling two-way communication with a socket and a COM port.
         * Parameters: IAsyncResult result
         * Returns: void
         */
        private void ReceiveCallback(IAsyncResult result)
        {
            if (_socket == null)
            {
                Console.WriteLine("this socket null.");
                return;
            }

            Socket serverSocket = result.AsyncState as Socket;

            if (serverSocket == null)
            {
                Console.WriteLine("Invalid server socket.");
                return;
            }

            int bufLength = _socket.EndReceive(result);
            byte[] packet = new byte[bufLength];
            Array.Copy(_buffer, packet, packet.Length);

            try
            {
                serverSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, null);
                string str = System.Text.Encoding.Default.GetString(_buffer);
                Console.WriteLine("Client Socket receives this message from Server Socket: " + str);

                // clear buffer
                Array.Clear(_buffer, 0, _buffer.Length);

                /*
                _buffer = new byte[1024];
                serverSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, null);

                string str = System.Text.Encoding.Default.GetString(_buffer);
                Console.WriteLine("Client Socket: " + str);
                */
            }

            catch (SocketException se)
            {
                Console.WriteLine("Could not receive for the following reason: " + se.ToString());
            }
        }

        /**
         * Name: Send
         * Purpose: Forwards console input out to server socket
         * Parameters: N/A
         * Returns: void
         */
        public void Send(string msg_string)
        {
            if (_socket == null)
            {
                Console.WriteLine("Client socket null.");
                return;
            }

            if (!_socket.Connected)
            {
                Console.WriteLine("Not currently connected to the server socket.");
                return;
            }

            try
            {
                byte[] msg = Encoding.Unicode.GetBytes(msg_string); 

                _socket.SendTo(msg, _ip);
            }

            catch (Exception exc)
            {
                Console.WriteLine("Socket send failed with the following error: ");
                Console.WriteLine(exc.ToString());
                Disconnect();
            }
        }
    }
}
