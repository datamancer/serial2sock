using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace MySocket
{
    public enum Status
    {
        Running,
        Idle,
        Stopping
    }

    class Program
    {
        static SocketPermission permission;
        static Socket s;
        static IPEndPoint ipEndPoint;
        static Socket handler;
        static private Status _status = Status.Idle;

        static Boolean stateToStop = false;

        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("Please specify a port number");
            }

            String ipEndPoint_s = args[0];
            int ipEndPoint_i;
            bool parsed = Int32.TryParse(ipEndPoint_s, out ipEndPoint_i);

            if( !parsed )
                Console.WriteLine("Int32.TryParse could not parse '{0}' to an int.\n", ipEndPoint_s);

            try
            {
                s = null;

                // permissions object for access restrictions
                permission = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", SocketPermission.AllPorts);

                // ensures the code to have permission to access socket
                permission.Demand();

                // resolves host name to IPHostEntry instance
                IPHostEntry ipHost = Dns.GetHostEntry(""); 

                // grab localhost ip
                IPAddress ipAddr = ipHost.AddressList[0];

                // create a network endpoint
                ipEndPoint = new IPEndPoint(ipAddr, ipEndPoint_i);

                // create a socket object to listen to incoming connection
                s = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // associate a socket with local endpoint
                s.Bind(ipEndPoint);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error occurred when creating the socket.");
                Console.WriteLine(e.ToString());
                
            }

            Console.WriteLine("Successfully created socket on {0} port: {1}", ipEndPoint.Address, ipEndPoint.Port);
            //Console.ReadKey();

            try
            {
                s.Listen(10);
                AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
                s.BeginAccept(aCallback, s);
            }

            catch (System.Exception e)
            {
                Console.WriteLine("Error occurred when listening to the socket.");
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("Successfully reading socket");

            _status = Status.Running;

            while (_status == Status.Running)
            {
                if ( stateToStop == true)
                {
                    _status = Status.Stopping;
                }
            }

            if (_status == Status.Stopping)
            {
                s.Shutdown(SocketShutdown.Receive); ;
                s.Close();
            }
        }

        private static void AcceptCallback( IAsyncResult ar)
        {
            Socket listener = null;

            // A new Socket to handle remote host communication  
            Socket handler = null;
            try
            {
                // Receiving byte array  
                byte[] buffer = new byte[1024];
                // Get Listening Socket object  
                listener = (Socket)ar.AsyncState;
                // Create a new socket  
                handler = listener.EndAccept(ar);

                // Using the Nagle algorithm  
                handler.NoDelay = false;

                // Creates one object array for passing data  
                object[] obj = new object[2];
                obj[0] = buffer;
                obj[1] = handler;

                // Begins to asynchronously receive data  
                handler.BeginReceive(
                    buffer,        // An array of type Byt for received data  
                    0,             // The zero-based position in the buffer   
                    buffer.Length, // The number of bytes to receive  
                    SocketFlags.None,// Specifies send and receive behaviors  
                    new AsyncCallback(ReceiveCallback),//An AsyncCallback delegate  
                    obj            // Specifies infomation for receive operation  
                    );

                // Begins an asynchronous operation to accept an attempt  
                AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
                listener.BeginAccept(aCallback, listener);
            }

            catch (System.Exception e)
            {
                Console.WriteLine("The following error has occurred:");
                Console.WriteLine(e.ToString());
            }
           
        }

        static void ReceiveCallback( IAsyncResult ar )
        {
            try
            {


                // Fetch a user-defined object that contains information  
                object[] obj = new object[2];
                obj = (object[])ar.AsyncState;

                // Received byte array  
                byte[] buffer = (byte[])obj[0];

                // A Socket to handle remote host communication.  
                handler = (Socket)obj[1];

                // Received message  
                string content = string.Empty;


                // The number of bytes received.  
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    content += Encoding.Unicode.GetString(buffer, 0,
                        bytesRead);

                    // If message contains "<Client Quit>", finish receiving 
                    if (content.IndexOf("<Client Quit>") > -1)
                    {
                        // Convert byte array to string 
                        string str = content.Substring(0, content.LastIndexOf("<Client Quit>"));
                        Console.WriteLine("Read " + str.Length * 2 + "bytes from client.]n Data: " + str);

                        // go ahead and stop after the first message
                        stateToStop = true;
                    }

                    else
                    {
                        // Continues to asynchronously receive data 
                        byte[] buffernew = new byte[1024];
                        obj[0] = buffernew;
                        obj[1] = handler;
                        handler.BeginReceive(buffernew, 0, buffernew.Length,
                            SocketFlags.None,
                            new AsyncCallback(ReceiveCallback), obj);
                    }
                }
            }

            catch( System.Exception e)
            {
                Console.WriteLine("The following error has occurred:");
                Console.WriteLine(e.ToString());
            }
            
        }
    }
}
