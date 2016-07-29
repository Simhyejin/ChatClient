using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Configuration;

namespace Chatting
{
    public class Client
    {
        Message m = new Message();
        IPEndPoint server;
        Socket socket;
        IPAddress ip;
        int port;                                                                    
        public Client(int port)
        {
            this.port = port;
            ip = GetServerIP();
            ServerEP(ip, this.port);
            Connection();
        }
        
        public void ServerEP(IPAddress ip,int port)
        {
            try
            {
                server = new IPEndPoint(ip, port);
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
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(server);
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                socket.Close();
                //Console.WriteLine("SocketException : {0}", se.ToString());
                Console.Write("Connecting Server  ");
                
                IPAddress ip = GetServerIP();
                Console.WriteLine("{0} ...",ip);
                ServerEP(ip, port);
                Connection();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected Exception : {0}", e.ToString());
            }
        }

        public byte[] BodyStructToBytes(object obj)
        {
            return m.StructureToByte(obj);
        }

        public void Send(MessageType type, byte[] bytes)
        {
            try
            {

                byte[] bodyBytes = bytes;

                Header head = new Header(type,MessageState.REQUEST,bodyBytes.Length);
                byte[] headBytes = m.StructureToByte(head);

                int byteSent = socket.Send(headBytes);
               // Console.WriteLine(byteSent);
                
                byteSent = socket.Send(bodyBytes);
                //Console.WriteLine(byteSent);
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
                byte[] HeadBytes = new byte[8];
                int byteRecv = socket.Receive(HeadBytes);

                Header RecvHeader = new Header();
                RecvHeader = (Header)m.ByteToStructure(HeadBytes, typeof(Header));

                byte[] BodyBytes = new byte[RecvHeader.length];
                byteRecv = socket.Receive(BodyBytes);

                string s = m.ByteToString(BodyBytes);
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
            socket.Close();
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
