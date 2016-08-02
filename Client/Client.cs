using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chat_Cleint
{
    class Client
    {
        Message m = new Message();

        private Socket clientSocket;
        private IPEndPoint serverEP;

        private BackgroundWorker bwRecevier;
        
        public bool Connected()
        {
            if (this.clientSocket != null)
                return this.clientSocket.Connected;
            else
                return false;
        }

        public IPAddress ServerIP()
        {
            if (Connected())
                return serverEP.Address;
            else
                return IPAddress.None;
        }

        public int ServerPort()
        {
            if (Connected())
                return serverEP.Port;
            else
                return -1;
        }


        public IPAddress IP()
        {
            if (Connected())
                return ((IPEndPoint)clientSocket.LocalEndPoint).Address;
            else
                return IPAddress.None;
        }

        public int Port()
        {
            if (Connected())
                return ((IPEndPoint)clientSocket.LocalEndPoint).Port;
            else
                return -1;
        }

        public Client(IPAddress ip, int port)
        {
            
            serverEP = new IPEndPoint(ip, port);
            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkChangeHandle);
        }

        public void ConnectToServer()
        {
            BackgroundWorker bwConnector = new BackgroundWorker();
            bwConnector.DoWork += new DoWorkEventHandler(bw_DoWork);
            bwConnector.RunWorkerCompleted += new RunWorkerCompletedEventHandler();
            bwConnector.RunWorkerAsync();
        }

        private void NetworkChangeHandle(object sender, NetworkAvailabilityEventArgs e)
        {
            if (!e.IsAvailable)
            {
                Console.WriteLine("Disconnected");
            }
            else
                Console.WriteLine("Connected");

        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(serverEP);
                e.Result = true;
                bwRecevier = new BackgroundWorker();
                bwRecevier.WorkerSupportsCancellation = true;
                bwRecevier.DoWork += new DoWorkEventHandler(Recieve);
                bwRecevier.RunWorkerAsync();
            }
            catch ()
            {

            }
        }

        Semaphore semaphor = new Semaphore(1,1);

        private bool SendToServer(MessageType type, byte[] body)
        {
            try
            {
                semaphor.WaitOne();
                byte[] bodyBytes = body;

                Header head = new Header(type, MessageState.REQUEST, bodyBytes.Length);
                byte[] headBytes = m.StructureToByte(head);

                int byteSent = clientSocket.Send(headBytes);
                byteSent = clientSocket.Send(bodyBytes);
                semaphor.Release();
                return true;
            }
            catch
            {
                semaphor.Release();
                return false;
            }
        }

        private void Recieve(object sender, DoWorkEventArgs e)
        {
            while (clientSocket.Connected)
            {
                byte[] buffer = new byte[8];
                int readBytes = clientSocket.Receive(buffer);
                if (0 == readBytes)
                    break;

                Header header = (Header)m.ByteToStructure(buffer, typeof(Header));

                MessageType type = header.type;
                MessageState state = header.state;
                int bodyLen = header.length;

                Console.WriteLine(type);
                Console.WriteLine(state);
                Console.WriteLine(bodyLen);

                if (bodyLen > 0)
                {
                    
                    buffer = new byte[bodyLen];
                    readBytes = clientSocket.Receive(buffer);
                    if (0 == readBytes)
                        break;
                    ProcessBody(type, state, buffer);
                }

            }
        }

        private void ProcessBody(MessageType type, MessageState state, byte[] body)
        {
            switch (type)
            {
                case MessageType.Chat_MSG_Broadcast:
                    break;
                case MessageType.Chat_MSG_From_Client:
                    break;
                case MessageType.Id_Dup:
                    if (state == MessageState.SUCCESS)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("[!]Duplicated ID");
                    }
                    break;
                case MessageType.Room_Create:
                    break;
                case MessageType.Room_Join:
                    break;
                case MessageType.Room_Leave:
                    break;
                case MessageType.Room_List:
                    break;
                case MessageType.Signin:
                    break;
                case MessageType.Signup:
                    break;
                default:
                    break;
            }
        }


    }
}
