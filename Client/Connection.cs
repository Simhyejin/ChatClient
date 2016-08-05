﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Connection
    {
        private IPEndPoint serverEP;

        private IPAddress ip;
        private int port;
        private MessageConvert mc = new MessageConvert();

        public Connection(IPAddress ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public bool IsConnected(Socket socket)
        {
            if (socket == null || !socket.Connected)
            {
                return false;
            }
            return true;
        }

        public Socket Connect()
        {
            while (true)
            {
                Socket socket = null;
                
                try
                {
                    Console.WriteLine("Connecting {0}:{1} ....",ip, port);
                    serverEP = new IPEndPoint(ip, port);
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(serverEP);

                    string title = "IP : " + ip + " Port : " + port;
                    Console.Title = title;
                    return socket;
                }
                catch (SocketException)
                {
                    //Console.WriteLine(se.ToString());
                    ip = mc.GetServerIP(out port);
                    continue;
                }
                catch (Exception e)
                {
                    ip = mc.GetServerIP(out port);
                    //Console.WriteLine(e.ToString());
                    continue;
                }
                
            }
            
        }

    }
}
