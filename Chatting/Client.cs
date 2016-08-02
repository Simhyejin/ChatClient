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
        Socket clientSocket;
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
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(server);
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
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
                int byteSent = clientSocket.Send(headBytes);
                
                byteSent = clientSocket.Send(bodyBytes);
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

        public object Recieve(out byte[] body)
        {
            body = null;
            if (clientSocket.Connected)
            {
                byte[] buffer = new byte[8];
                int readBytes = clientSocket.Receive(buffer);
               
                Header header = (Header)m.ByteToStructure(buffer, typeof(Header));

                MessageType type = header.type;
                MessageState state = header.state;
                int bodyLen = header.length;
              

                if (bodyLen > 0)
                {

                    buffer = new byte[bodyLen];
                    readBytes = clientSocket.Receive(buffer);
                    body = buffer;
                   
                }

                return header;
            }

            return null;
        }
        
       
        public void SocketClose()
        {
            clientSocket.Close();
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
