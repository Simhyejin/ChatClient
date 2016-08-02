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
        public User(string id, string pw)
        {
            this.id = id;
            this.password = pw;
        }
    }

    class Chat
    {
        Client client;
        User user;
        Message m = new Message();
        List<int> RoomList;

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
                        SignUp();
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


        public void SignUp()
        {
            string id;
            string pw;

            Console.Clear();
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                            Sign up                             |");
            Console.WriteLine("+----------------------------------------------------------------+");

            while (true)
            {
               
                Console.Write("ID       : ");
                id = Console.ReadLine();

                if (id.Length > 10 || id.Length < 3)
                {
                    Console.WriteLine("[!]ID lenght should be between 3 and 12");
                    continue;
                }
                
                LoginRequestBody dupReqest = new LoginRequestBody(id.ToCharArray(), "-".ToCharArray());
                byte[] dupBody = m.StructureToByte(dupReqest);
                Console.WriteLine("dupBody.Length: " +dupBody.Length);
                client.Send(MessageType.Id_Dup, dupBody);

                Header header = (Header)client.Recieve(out dupBody);

                if (header.state == MessageState.SUCCESS)
                    break;
                else
                    Console.WriteLine("[!]Duplicated ID");
                
            }

            while (true) { 
                Console.Write("Password     : ");
                pw = ReadPassword();
                if (pw.Length > 10 || pw.Length < 4)
                {
                    Console.WriteLine("[!]Password lenght should be between 4 and 16");
                    continue;
                }

                Console.Write("Passeord again   : ");
                string pw2 = ReadPassword();

                if (pw != pw2)
                {
                    Console.WriteLine("[!] Passwords must match");
                    continue;
                }
                break;
            }

            SignupRequestBody signupReqest = new SignupRequestBody(id.ToCharArray(), pw.ToCharArray(), false);
            byte[] body = m.StructureToByte(signupReqest);
            client.Send(MessageType.Signup, body);

            Header h = (Header)client.Recieve(out body);

            if (h.state == MessageState.SUCCESS)
            {
                LogIn();
            }
            else
            {
                Console.WriteLine("[!]Fail to Sign up");
            }
        }

        public void LogIn()
        {
            string id;
            string password;

            while (true) {
                Console.Clear();
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine("|                            Log In                              |");
                Console.WriteLine("+----------------------------------------------------------------+");

                Console.Write("ID       : ");
                id = Console.ReadLine();

                Console.Write("Password     : ");
                password = ReadPassword();

                LoginRequestBody logInReqest = new LoginRequestBody(id.ToCharArray(), password.ToCharArray());
                byte[] body = client.BodyStructToBytes(logInReqest);
                client.Send(MessageType.Signin, body);

                Header h = (Header)client.Recieve(out body);

                if (h.state == MessageState.SUCCESS)
                    break;

                else
                    Console.WriteLine("[!]Fail to Log in");
               

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
                        password = password.Substring(0, password.Length - 1);
                        int pos = Console.CursorLeft;
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        Console.Write(" ");
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }
            Console.WriteLine();
            return password;
        }
    
        public void Lobby()
        {
            Console.Clear();
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                             Lobby                              |");
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("| 1. RoomList            2. Create Room          3. Enter Room   |");
            Console.WriteLine("+----------------------------------------------------------------+");

            LobbyHelp();
            Console.WriteLine();
            Console.Write("> ");

            string menu = Console.ReadLine();
            menu = menu.ToLower().Replace(" ", "");

            try
            {
                switch (menu)
                {
                    //RoomList
                    case "1": case "1roomlist":
                    case "roomlist": case "room": case "list":
                    case "rl": case "r": case "l":
                        ListRoom();
                        break;
                    case "2":
                        CreateRoom();
                        break;
                    case "3":
                        EnterRoom();
                        break;

                    case "exit":
                        client.SocketClose();
                        break;

                    default:
                        Lobby();
                        break;
                }
            }
            catch (FormatException fe)
            {
                MainHelp();
                Main();
            }
        }

        public void LobbyHelp()
        {
            Console.WriteLine("usage :");
            Console.WriteLine("       [num]");
            Console.WriteLine("       [num] [command]");
            Console.WriteLine("       [command]");
            Console.WriteLine("       [initial of command]");
            Console.WriteLine("(Commands are not case-sensitive.)");
            Console.WriteLine("[EX] 1, roomlist, room , rl, r, ...");
        }

        public void ListRoom()
        {
            RoomRequestBody roomReqest = new RoomRequestBody(user.id.ToCharArray(), 0);
            byte[] body = client.BodyStructToBytes(roomReqest);
            client.Send(MessageType.Room_List, body);
            Header h = (Header)client.Recieve(out body);

            if (h.length == 0)
                Console.WriteLine("방이 없습니다.");
            else if (h.state == MessageState.SUCCESS)
            {


                int[] arr = Array.ConvertAll(body,i=>(int)i);
                RoomList = arr.OfType<int>().ToList();
                foreach (int room in RoomList)
                {
                    Console.WriteLine("room #" + room);
                }
            }
            else
            {
                Console.WriteLine("[!]Fail to get Room List");
                Lobby();
            }
        }

        public void CreateRoom()
        {
            RoomRequestBody createRoom = new RoomRequestBody(user.id.ToCharArray(), 0);
            byte[] body = client.BodyStructToBytes(createRoom);
            client.Send(MessageType.Room_Create, body);
            Header h = (Header)client.Recieve(out body);

            if (h.state == MessageState.SUCCESS)
            {
                int no = BitConverter.ToInt32(body,0);
                Console.WriteLine(no);
                //JoinRoom(no);
                Lobby();
            }
            else
            {
                Console.WriteLine("[!]Fail to Create Room");
            }
        }

        public void EnterRoom()
        {
            while (true)
            {
                Console.WriteLine("Enter Room# : ");
                string no = Console.ReadLine();
                int room;
                if(int.TryParse(no,out room))
                {
                    if (RoomList.Contains(room))
                    {
                        JoinRoom(room);
                        break;
                    }
                }
                Console.WriteLine("Wrong input");
                
            }
        }

        public void JoinRoom(int roomno)
        {
            RoomRequestBody EnterRoom = new RoomRequestBody(user.id.ToCharArray(), roomno);
            byte[] body = client.BodyStructToBytes(EnterRoom);
            client.Send(MessageType.Room_Join, body);

            Header h = (Header)client.Recieve(out body);

            if (h.state == MessageState.SUCCESS)
            {
                Console.WriteLine("["+roomno+"] 번 방에 입장하셨습니다.");
                Lobby();
            }
            else
            {
                Console.WriteLine("[!]Fail to get Room List");
            }

        }
    }
}
