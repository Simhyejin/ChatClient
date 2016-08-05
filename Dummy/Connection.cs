using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dummy
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
                    continue;
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.ToString());
                    continue;
                }
            }

        }

    }
}
