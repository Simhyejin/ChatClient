using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Connection
    {
        IPEndPoint serverEP;
        Socket socket;

        IPAddress ip;
        int port;

        public Connection(IPAddress ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public Socket startConnection()
        {
            do
            {
                Console.WriteLine("Connecting {0} ...",ip);

            } while (!connect());
            Console.Clear();
            return socket;
        }

        public bool connect()
        {
            serverEP = new IPEndPoint(ip, port);

            if (socket == null || !socket.Connected)
            {
                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(serverEP);
                    return true;
                }
                catch (SocketException se)
                {
                    //Console.WriteLine(se.ToString());
                    return false;
                }
            }
            else
                return false;
        }

    }
}
