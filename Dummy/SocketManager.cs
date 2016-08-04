﻿using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;

namespace Dummy
{
    class SocketManager
    {
        private MessageConvert mc;
        private Socket socket;
        

        public SocketManager(Socket socket)
        {
            this.socket = socket;
            mc = new MessageConvert();
        }
        
        public void Send(MessageType type, MessageState state, byte[] bytes)
        {
            try
            {
                if (bytes!=null)
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
                Console.Clear();
                int port = 0;
                IPAddress ip = mc.GetServerIP(out port);
                Connection con = new Connection(ip, port);
                socket = con.startConnection();
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
                }catch(SocketException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            return null;
        }
    }

}
