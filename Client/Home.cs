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
        bool flag = true;

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
            while (flag) {
                
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine("|                      Welcome to 4:33 Chat                      |");
                Console.WriteLine("|                Type \"exit\", if you want to exit                |");
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine("| 1. Log In                                                      |");
                Console.WriteLine("| 2. Sign Up                                                     |");
                Console.WriteLine("+----------------------------------------------------------------+");


                Console.Write("> ");
                String menu = null;
                KeyType result = mc.TryReadLine(out menu);

                if(KeyType.Success == result)
                {

                    Console.Clear();
                    switch (menu)
                    {
                        //Login
                        case "1":
                        case "l":
                            Login log = new Login(socket, out socket);
                            break;

                        case "2":
                        case "s":
                            Signup signup = new Signup(socket);
                            break;
                        default:
                            Console.WriteLine("[!]잘못된 입력입니다.");
                            break;
                    }
                }
                else if(result == KeyType.Exit)
                { 
                    flag = false;
                }
            }
            
           

        }

        
    }
}
