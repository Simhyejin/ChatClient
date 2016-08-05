using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;

namespace Admin
{
    class SocketManager
    {
        private MessageConvert mc;

        public SocketManager()
        {
            mc = new MessageConvert();
        }

        public void Send(MessageType type, MessageState state, byte[] bytes, ref Socket socket)
        {
            try
            {
                if (bytes != null)
                {
                    byte[] bodyBytes = bytes;

                    Header head = new Header(type, state, bodyBytes.Length);
                    byte[] headBytes = mc.StructureToByte(head);

                    int byteSent = socket.Send(headBytes);
                    byteSent = socket.Send(bodyBytes);
                }
                else
                {
                    Header head = new Header(type, state, 0);
                    byte[] headBytes = mc.StructureToByte(head);
                    int byteSent = socket.Send(headBytes);
                }
            }
            catch (SocketException)
            {
                ReConnection(out socket);
            }
            catch (Exception)
            {
                ReConnection(out socket);
                //Console.WriteLine(e.ToString());
            }
        }

        public object Recieve(out byte[] body, ref Socket socket)
        {
            body = null;
            if (socket != null)
            {
                try
                {
                    byte[] buffer = new byte[8];
                    int readBytes = socket.Receive(buffer);

                    if (0 == readBytes)
                    {
                        throw new SocketException();
                    }

                    Header header = (Header)mc.ByteToStructure(buffer, typeof(Header));

                    MessageType type = header.type;
                    MessageState state = header.state;
                    int bodyLen = header.length;

                    if (bodyLen > 0)
                    {
                        buffer = new byte[bodyLen];
                        readBytes = socket.Receive(buffer);
                        if (0 == readBytes)
                        {
                            throw new SocketException();
                        }
                        body = buffer;
                    }

                    return header;
                }
                catch (SocketException se)
                {
                    
                    throw;
                }
                catch (Exception e)
                {
                    throw;
                }
            }

            return null;
        }

        public void ReConnection(out Socket socket)
        {
            Console.Clear();
            int port = 0;
            IPAddress ip = mc.GetServerIP(out port);
            Connection con = new Connection(ip, port);
            socket = con.Connect();
        }
    }

}
