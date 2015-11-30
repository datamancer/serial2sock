using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MainPower.Com0com.Redirector
{
    public enum CommsMode
    {
        RFC2217,
    }


    public enum CommsStatus
    {
        Running, 
        Idle,
    }

    public class Com0comPortPair : INotifyPropertyChanged
    {
        #region Fields
        private string _portConfigStringA = "";
        private string _portConfigStringB = "";
        private string _baudRate = "";
        private Process _p;
        private CommsStatus _commsStatus = CommsStatus.Idle;
        private CommsMode _commsMode = CommsMode.RFC2217;
        private string _outputData = "";
        private string _remoteIP = "";
        private string _localPort = "";

        // Receiving byte array   
        byte[] bytes = new byte[1024];
        byte[] input = new byte[1024];
        Socket senderSock;

        public SerialPort comA;
        public SerialPort comB;

        //public ComponentModel components;

        #endregion

        #region Properties

        public int PairNumber { get; private set; }
        public string PortNameA { get; private set; }
        public string PortNameB { get; private set; }
        public string BaudRate { get; private set; }

        public string RemoteIP
        {
            get
            {
                return _remoteIP;
            }
            set
            {
                _remoteIP = value;
                OnPropertyChanged("RemoteIP");
            }
        }

        public string LocalPort
        {
            get
            {
                return _localPort;
            }
            set
            {
                _localPort = value;
                OnPropertyChanged("LocalPort");
            }
        }
        public string OutputData
        {
            get { return _outputData; }
            private set 
            {
                _outputData = value;
                OnPropertyChanged("OutputData");
            }
        }
        public CommsMode CommsMode
        {
            get { return _commsMode; }
            set
            {
                _commsMode = value;
                OnPropertyChanged("CommsMode");
            }
        }
        public CommsStatus CommsStatus
        {
            get { return _commsStatus; }
            set
            {
                _commsStatus = value;
                OnPropertyChanged("CommsStatus");
            }
        }

        public string PortConfigStringA
        {
            get { return _portConfigStringA; }
            set
            {
                Regex regex = new Regex(@"(?<=PortName=)\w+(?=,)");
                _portConfigStringA = value;
                PortNameA = regex.Match(value).Value;
                
                OnPropertyChanged("PortNameA");
                OnPropertyChanged("PortConfigStringA");
                
            }
        }

        
        public string PortConfigStringB 
        {
            get { return _portConfigStringB; }
            set
            {
                Regex regex = new Regex(@"(?<=PortName=)\w+(?=,)");
                _portConfigStringB = value;
                PortNameB = regex.Match(value).Value;
                
                OnPropertyChanged("PortNameB");
                OnPropertyChanged("PortConfigStringB");
                
            }
        }

        public string baudRate
        {
            get { return _baudRate; }
            set
            {
                _baudRate = value;

                OnPropertyChanged("BaudRate");
            }
        }

        #endregion

        public Com0comPortPair(int number)
        {
            LocalPort = "8883";
            PairNumber = number;
        }

        #region Static Functions
      
        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        private static void KillProcessAndChildren(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch
            {
                //we might get exceptions here, as parent might auto exit once their children are terminated
            }
        }

        #endregion

        public void StartComms()
        {
            if (CommsStatus == CommsStatus.Running)
                return;
            string program = "";
            string arguments = "";

            switch (CommsMode)
            { 
                case CommsMode.RFC2217:
                    program = "Socket.exe";
                    arguments = string.Format("{0}", LocalPort);
                    break;
            }

            _p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files (x86)\com0com\" + program,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            _p.EnableRaisingEvents = true;
            _p.Exited += _p_Exited;
            _p.OutputDataReceived += _p_OutputDataReceived;
            _p.ErrorDataReceived += _p_ErrorDataReceived;

            OutputData = "";
            _p.Start();
            _p.BeginOutputReadLine();
            _p.BeginErrorReadLine();

            SetupComPort();
            SetupSocket();

            // write inputs to socket
            while (CommsStatus == CommsStatus.Running) // may need to check this elsewhere
            {
                // read contents from comB into buffer
                input = Encoding.Unicode.GetBytes(comB.ReadByte() + "<Client Quit>"); // play with this TODO
                senderSock.Send(input);
            }
        }
        private void SetupComPort()
        {
            // buffer in anything from the com port
            //byte[] buffer = new byte[1024]; // used later
            string baudRate = "9600"; // will pull from ComboBox later TODO

            // connect to second com port .. consider refactoring this TODO
            comB = new System.IO.Ports.SerialPort();
            comB.PortName = PortNameB;
            comB.BaudRate = Convert.ToInt32(baudRate);
            comB.Open();
        }

        private void SetupSocket()
        {
            try
            {
                // Create one SocketPermission for socket access restrictions  
                SocketPermission permission = new SocketPermission(
                    NetworkAccess.Connect,    // Connection permission  
                    TransportType.Tcp,        // Defines transport types  
                    "",                       // Gets the IP addresses  
                    SocketPermission.AllPorts // All ports  
                    );

                // Ensures the code to have permission to access a Socket  
                permission.Demand();

                // Resolves a host name to an IPHostEntry instance             
                IPHostEntry ipHost = Dns.GetHostEntry("");

                // Gets first IP address associated with a localhost  

                IPAddress ipAddr = ipHost.AddressList[0];

                // Creates a network endpoint  
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 8883);

                // Create one Socket object to setup Tcp connection  
                senderSock = new Socket(
                    ipAddr.AddressFamily,// Specifies the addressing scheme  
                    SocketType.Stream,   // The type of socket   
                    ProtocolType.Tcp     // Specifies the protocols   
                    );

                senderSock.NoDelay = false;   // Using the Nagle algorithm  

                // Establishes a connection to a remote host  
                senderSock.Connect(ipEndPoint);
                //Console.WriteLine("Socket connected to " + senderSock.RemoteEndPoint.ToString());
            }

            catch (SocketException e)
            {
                //Console.WriteLine("Socket exception: " + e.ToString());
            }

            catch (Exception exc)
            {
                //Console.WriteLine("Client received the following error: ");
                //Console.WriteLine(exc.ToString());
            }



            // ensure comm status is running
            CommsStatus = CommsStatus.Running;

        }

        private void ReceiveDataFromServer()
        {
            try
            {
                // Receives data from a bound Socket.  
                int bytesRec = senderSock.Receive(bytes);

                // Converts byte array to string  
                String theMessageToReceive = Encoding.Unicode.GetString(bytes, 0, bytesRec);

                // Continues to read the data till data isn't available  
                while (senderSock.Available > 0)
                {
                    bytesRec = senderSock.Receive(bytes);
                    theMessageToReceive += Encoding.Unicode.GetString(bytes, 0, bytesRec);
                }

                //Console.WriteLine("Client: " + "The server reply: " + theMessageToReceive);
            }

            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }


        public void StopComms()
        {
            if (_p == null)
            {
                CommsStatus = CommsStatus.Idle;
                return;
            }
            if (_p.HasExited)
                return;
            KillProcessAndChildren(_p.Id);
        }

        private void _p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputData += e.Data + Environment.NewLine;
        }

        private void _p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputData += e.Data + Environment.NewLine;
        }

        private void _p_Exited(object sender, EventArgs e)
        {
            CommsStatus = CommsStatus.Idle;
        }

        #region INotifyPropertyChangedMembers
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        public void VerifyPropertyName(string propertyName)
        {
            //Verify that the property name matches a real,
            //public instance property on this object
            //an empty property name is ok, used to refresh all properties
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                Debug.Fail( "Invalid property name: " + propertyName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        
    }
}
