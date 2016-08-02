using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    class SocketManager
    {
        MessageConvert mc = new MessageConvert();
        Socket socket;
        

        public SocketManager(Socket socket)
        {
            this.socket = socket;
        }
        
        public void Send(MessageType type, byte[] bytes)
        {
            try
            {

                byte[] bodyBytes = bytes;

                Header head = new Header(type, MessageState.REQUEST, bodyBytes.Length);
                byte[] headBytes = mc.StructureToByte(head);
                int byteSent = socket.Send(headBytes);

                byteSent = socket.Send(bodyBytes);
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
            if (socket.Connected)
            {
                byte[] buffer = new byte[8];
                int readBytes = socket.Receive(buffer);

                Header header = (Header)mc.ByteToStructure(buffer, typeof(Header));

                MessageType type = header.type;
                MessageState state = header.state;
                int bodyLen = header.length;


                if (bodyLen > 0)
                {

                    buffer = new byte[bodyLen];
                    readBytes = socket.Receive(buffer);
                    body = buffer;

                }

                return header;
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
