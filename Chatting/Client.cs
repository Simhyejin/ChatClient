using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Configuration;

namespace Chatting
{
    public class Client
    {
        IPEndPoint Server;
        Socket Socket;
        IPAddress IP;
        int Port;                                                                    
        public Client(int port)
        {
            Port = port;
            IP = GetServerIP(); 
            ServerEP(IP, Port);
            Connection();
        }
        
        public void ServerEP(IPAddress ip,int port)
        {
            try
            {
                Server = new IPEndPoint(ip, port);
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected Exception : {0}", e.ToString());
            }

        }

        public void Connection()
        {
            try
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket.Connect(Server);
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Socket.Close();
                //Console.WriteLine("SocketException : {0}", se.ToString());
                Console.Write("Connecting Server  ");
                
                IPAddress ip = GetServerIP();
                Console.WriteLine("{0} ...",ip);
                ServerEP(ip, Port);
                Connection();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected Exception : {0}", e.ToString());
            }
        }

        public void Send(MessageType type, string data)
        {
            try
            {
                Message m = new Message();
                byte[] arr = Encoding.UTF8.GetBytes(data);

                Header head = new Header(type, arr.Length);

                byte[] headBytes = m.StructureToByte(head);
                int byteSent = Socket.Send(headBytes);
                Console.WriteLine(byteSent);

                byte[] bodyBytes = Encoding.UTF8.GetBytes(data);
                byteSent = Socket.Send(bodyBytes);
                Console.WriteLine(byteSent);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public string Recieve(MessageType type)
        {
            try
            {
                Message m = new Message();

                byte[] HeadBytes = new byte[6];
                int byteRecv = Socket.Receive(HeadBytes);

                Header RecvHeader = new Header();
                m.ByteArrayToStruct(HeadBytes, ref RecvHeader);
                if (type != RecvHeader.type)
                {
                    return null;
                }

                byte[] BodyBytes = new byte[RecvHeader.length];
                byteRecv = Socket.Receive(BodyBytes);

                string s = Encoding.UTF8.GetString(BodyBytes);
                return s;
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return null;
        }
        public void SocketClose()
        {
            Socket.Close();
        }

        //Retrun Random IP in Server List 
        public IPAddress GetServerIP()
        {
            Random r = new Random();
            int rand = r.Next(1, 4);
            string randomIP = ConfigurationManager.AppSettings[rand];

            return IPAddress.Parse(randomIP);
        }




    }
}
