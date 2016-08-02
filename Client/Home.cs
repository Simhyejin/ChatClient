using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Home
    {

        Socket socket;

        MessageConvert mc = new MessageConvert();
        SocketManager sm;

        public Home(Socket socket)
        {
            this.socket = socket;
            sm = new SocketManager(this.socket);
            StartHome();
        }

        public void StartHome()
        {
            Main();
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                  Closed Program. Good Bye                      |");
            Console.WriteLine("+----------------------------------------------------------------+");
            sm.SocketClose();
        }
        public void Main()
        {
            Console.Clear();
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                      Welcome to 4:33 Chat                      |");
            Console.WriteLine("|                Type \"exit\", if you want to exit                |");
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("| 1. Log In                                                      |");
            Console.WriteLine("| 2. Sign Up                                                     |");
            Console.WriteLine("+----------------------------------------------------------------+");

           
            Console.Write("> ");
            ConsoleKeyInfo key = Console.ReadKey();

            //string menu = Console.ReadLine();
            //menu = menu.ToLower().Replace(" ", "");

           
            switch (key.KeyChar)
            {
                //Login
                case '1': case 'l':
                    Login log = new Login(socket);
                    break;

                case '2': case 's':
                    Signup signup = new Signup(socket);
                    break;
                case '\b':
                    break;
                default:
                    Main();
                    break;
                    
            }
            
           

        }

        
    }
}
