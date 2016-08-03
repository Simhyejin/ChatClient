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
                try
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
                }catch(SocketException se)
                {
                    Console.Clear();
                    int port = 0;
                    IPAddress ip = mc.GetServerIP(out port);
                    Connection con = new Connection(ip, port);
                    socket = con.startConnection();
                }
            }

            return null;
        }


        public void RECV()
        {
            if (socket.Connected)
            {
                try
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
                    }
                }
                catch (SocketException se)
                {
                    Console.Clear();
                    int port = 0;
                    IPAddress ip = mc.GetServerIP(out port);
                    Connection con = new Connection(ip, port);
                    socket = con.startConnection();
                }
            }
        }

        public void SocketClose()
        {
            socket.Close();
        }

        

    }

}
