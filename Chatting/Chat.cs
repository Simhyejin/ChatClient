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

    class Chat
    {
        Client client;
        User user;

        public Chat(int port)
        {
            client = new Client(port);
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
                    //Login
                    case "1": case "1login": case "login": case "log" :case "l":
                        LogIn();
                        break;

                    case "2": case "2signin": case "signin": case "sign": case "s":
                        SignIn();
                        break;

                    case "exit":
                        client.SocketClose();
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


        public void SignIn()
        {
            string id;
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                            Sign In                             |");
            Console.WriteLine("+----------------------------------------------------------------+");

            while (true)
            {
                Message m = new Message();
                Console.Write("ID       : ");
                id = Console.ReadLine();

                if (id.Length > 10 || id.Length < 3)
                {
                    Console.WriteLine("[!]ID lenght should be between 3 and 12");
                    continue;
                }
                SignRequestBody signInReqest = new SignRequestBody(id.ToCharArray(), "-".ToCharArray(),false);
                byte[] body = m.StructureToByte(signInReqest);
                client.Send(MessageType.Id_Dup, body);
                client.Recieve(MessageType.Id_Dup);
                break;
            }

            while (true) { 
                Console.Write("Password     : ");
                string pw = Console.ReadLine();
                if (pw.Length > 10 || pw.Length < 4)
                {
                    Console.WriteLine("[!]Password lenght should be between 4 and 16");
                    continue;
                }

                Console.Write("Passeord again   : ");
                string pw2 = Console.ReadLine();

                if (pw != pw2)
                {
                    Console.WriteLine("[!] Passwords must match");
                    continue;
                }
                break;
            }
            LogIn();

        }

        public void LogIn()
        {
            string id;
            string password;
            while (true) { 
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine("|                            Log In                              |");
                Console.WriteLine("+----------------------------------------------------------------+");

                Console.Write("ID       : ");
                id = Console.ReadLine();

                Console.Write("Password     : ");
                password = ReadPassword();

                SignRequestBody logInReqest = new SignRequestBody(id.ToCharArray(), password.ToCharArray(),false);
                byte[] body = client.BodyStructToBytes(logInReqest);
                client.Send(MessageType.Signin, body);
                client.Recieve(MessageType.Signin);
            }
            user = new User(id, password);
            Lobby();
        }

        public string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        // remove one character from the list of password characters
                        password = password.Substring(0, password.Length - 1);
                        // get the location of the cursor
                        int pos = Console.CursorLeft;
                        // move the cursor to the left by one character
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        // replace it with space
                        Console.Write(" ");
                        // move the cursor to the left by one character again
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }
            // add a new line because user pressed enter at the end of their password
            Console.WriteLine();
            return password;
        }
    
        public void Lobby()
        {

        }
    }
}
