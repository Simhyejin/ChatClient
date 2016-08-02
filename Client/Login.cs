using System;
using System.Collections.Generic;
using System.Linq;
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

        public Login(Socket socket)
        {
            this.socket = socket;
            sm = new SocketManager(this.socket);
            LogIn();
        }
        

        public void LogIn()
        {
            string id;
            string password;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine("|                            Log In                              |");
                Console.WriteLine("+----------------------------------------------------------------+");

                Console.Write("ID       : ");
                id = Console.ReadLine();

                Console.Write("Password     : ");
                password = mc.ReadPassword();

                LoginRequestBody logInReqest = new LoginRequestBody(id.ToCharArray(), password.ToCharArray());
                byte[] body = mc.StructureToByte(logInReqest);
                sm.Send(MessageType.Signin, body);

                Header h = (Header)sm.Recieve(out body);

                if (h.state == MessageState.SUCCESS)
                    break;

                else
                    Console.WriteLine("[!]Fail to Log in");
            }
            user = new User(id, password);
            Lobby lobby = new Lobby(socket,user);
        }
    }
}
