using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;


namespace Chatting
{
    struct User
    {
        public string id;
        public string password;
    }
    class Chat
    {
        Client Client;
        Login Log = new Login();
        SignIn Sign = new SignIn();

        public Chat(int port)
        {
            Client = new Client(port);
            Main();
           
        }
        public void Main()
        {
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                      Welcome to 4:33 Chat                      |");
            Console.WriteLine("|                Type \"exit\", if you want to exit                |");
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("| 1. Log In                                                      |");
            Console.WriteLine("| 2. Sign In                                                     |");
            Console.WriteLine("+----------------------------------------------------------------+");

            MainHelp();

            Console.Write("> ");
            string menu = Console.ReadLine();
            menu = menu.ToLower().Replace(" ",""); 
            
            try
            {
                switch (menu)
                {
                    case "1":
                    case "1login":
                    case "login":
                    case "log":
                    case "l":
                        User user = Log.LogIn();
                        Client.Send(MessageType.REQUEST_LOGIN, user.id + "," + user.password);

                        break;

                    case "2":
                    case "2signin":
                    case "signin":
                    case "sign":
                    case "s":
                        while (true) {
                            if (Sign.Sign(Client))
                            {
                                Log.LogIn();
                                break;
                            }
                        }
                        break;

                    case "exit":
                        Client.SocketClose();
                        break;

                    default:
                        Main();
                        break;
                }
            }
            catch (FormatException fe)
            {
                MainHelp();
                Main();
            }

            

        }
		public void MainHelp()
		{
			Console.WriteLine("usage :");
			Console.WriteLine("       [num]");
			Console.WriteLine("       [num] [command]");
			Console.WriteLine("       [command]");
            Console.WriteLine("       [initial of command]");
			Console.WriteLine("(Commands are not case-sensitive.)");
		}
    }
}
