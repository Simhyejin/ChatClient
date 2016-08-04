using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Admin
{
    class Connection
    {
        IPEndPoint serverEP;
        Socket socket;

        IPAddress ip;
        int port;

        MessageConvert mc = new MessageConvert();

        public Connection(IPAddress ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public Socket startConnection()
        {
            Console.WriteLine("Connecting {0} {1}...", ip, port);
            while (true)
            { 
                for (int i = 0; i < 10; i++)
                {
                    if (!connect())
                        Console.WriteLine("Connecting {0} {1}...", ip, port);
                    else
                    {
                        Console.Clear();
                        return socket;
                    }
                }
                Console.Clear();
                //ip = mc.GetServerIP(out port);
            }

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
                    
                    string title = "IP : " + ip + " Port : " + port;
                    Console.Title = title;
                    return true;
                }
                catch (SocketException)
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
