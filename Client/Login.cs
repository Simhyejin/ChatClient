using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    struct User
    {
        public string id;
        public string password;
        public User(string id, string pw)
        {
            this.id = id;
            this.password = pw;
        }
    }

    class Login
    {
        Socket socket;

        User user = new User();
        MessageConvert mc = new MessageConvert();
        SocketManager sm;

        public Login(Socket socket, out Socket sock)
        {
            this.socket = socket;
            sock = socket;
            sm = new SocketManager(this.socket);
            if (socket.Connected)
                LogIn();
            else
            {
                int port = 0;
                IPAddress ip = mc.GetServerIP(out port);
                Connection con = new Connection(ip,port);
                sock = con.startConnection();
                socket = sock;
            }
                
        }
        

        public void LogIn()
        {
            string id;
            string password;

            while (true)
            {
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine("|                            Log In                              |");
                Console.WriteLine("+----------------------------------------------------------------+");

                Console.Write("ID       : ");
                id = Console.ReadLine();

                Console.Write("Password     : ");
                password = mc.ReadPassword();

                LoginRequestBody logInReqest = new LoginRequestBody(id.ToCharArray(), password.ToCharArray());
                byte[] body = mc.StructureToByte(logInReqest);
              
                sm.Send(MessageType.LogIn, body);
                Header h = (Header)sm.Recieve(out body);
                if (!h.Equals(null)&&h.state == MessageState.SUCCESS)
                    break;

                else
                    Console.WriteLine("[!]Fail to Log in");
 
            }
            user = new User(id, password);
            Console.Clear();
            Lobby lobby = new Lobby(socket, user, out socket);
        }
    }
}
