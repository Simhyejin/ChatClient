using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Client
{

    public enum State  
    {
        Exit,
        Home,
        LogIn,
        
        DupId,
        SignUp,

        Lobby,
        Join,
        Join_Direct,
        
        Room,
        Leave,
        Chat
            
    };

    class StateManager
    {
        private Socket socket;
        private UserInfo user;

        private MessageConvert mc;
        private SocketManager sm;
        

        //Home
        public StateManager(Socket socket)
        {
            user = new UserInfo();
            this.socket = socket;

            mc = new MessageConvert();
            sm = new SocketManager();
        }

        //Exit Program.
        public void Exit()
        {
            Console.WriteLine();
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                  Closed Program. Good Bye                      |");
            Console.WriteLine("+----------------------------------------------------------------+");
           
        }

        //Home
        public State Home()
        {
            Console.Clear();
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                      Welcome to 4:33 Chat                      |");
            Console.WriteLine("|             Press \"exit\" key, if you want to exit              |");
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("| 1. Log In                                                      |");
            Console.WriteLine("| 2. Sign Up                                                     |");
            Console.WriteLine("+----------------------------------------------------------------+");

            Console.Write("> ");
            String menu = null;
            MessageConvert.KeyType result = mc.TryReadLine(out menu);

            if (MessageConvert.KeyType.Success == result)
            {

                Console.Clear();
                switch (menu)
                {
                    //Login
                    case "1":
                    case "l":
                        return State.LogIn;

                    case "2":
                    case "s":
                        return State.DupId;
                    default:
                        Console.WriteLine("[!]wrong input.");
                        Console.WriteLine("Press any key to retry....");
                        Console.ReadKey();
                        return State.Home;
                }
            }
            else if (result == MessageConvert.KeyType.Exit)
            {
                return State.Exit;
            }
            else
                return State.Home;
        }

        //Sign up
        public State DupID(out bool isLock)
        {
            string id;
            Console.Clear();
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                            Sign up                             |");
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|        ESC : Exit      F1: Back                                |");
            Console.WriteLine("+----------------------------------------------------------------+");

            Console.Write("ID           : ");

            isLock =false;

            MessageConvert.KeyType result = mc.TryReadLine(out id);
            if (MessageConvert.KeyType.Success == result)
            {
                if (!checkStringFormat(id))
                {
                    Console.WriteLine("\n[!]ID Must contain English or Numbers");
                    Console.WriteLine("Press any key to retry....");
                    Console.ReadKey();
                }
                else if (id.Length > 10 || id.Length < 3)
                {
                    Console.WriteLine("\n[!]ID lenght should be between 3 and 12");
                    Console.WriteLine("Press any key to retry....");
                    Console.ReadKey();
                }
                else
                {
                    LoginRequestBody dupReqest = new LoginRequestBody(id.ToCharArray(), "-".ToCharArray());
                    byte[] dupBody = mc.StructureToByte(dupReqest);
                    sm.Send(MessageType.Id_Dup, MessageState.REQUEST, dupBody, ref socket);
                    isLock = true;
                    user.id = id;
                }
                return State.DupId;

            }
            else if (result == MessageConvert.KeyType.GoBack)
            {
                return State.Home;
            }
            else if (result == MessageConvert.KeyType.Exit)
            {
                return State.Exit;
            }
            else
            {
                return State.DupId;
            }
        }
        public void SignUp(out bool isLock)
        {
            string pw;
            isLock = false;
            Console.WriteLine();
            Console.Write("Password     : ");
            pw = mc.ReadPassword();
       
            if (pw.Length > 10 || pw.Length < 4) {
                Console.WriteLine("\n[!]Password lenght should be between 4 and 16");
                Console.WriteLine("Press any key to retry....");
                Console.ReadKey();
            }
                

            Console.Write("Passeord again   : ");
            string pw2 = mc.ReadPassword();

            if (pw != pw2)
            {
                Console.WriteLine("\n[!] Passwords must match");
                Console.WriteLine("Press any key to retry....");
                Console.ReadKey();
            }

            else
            {
                SignupRequestBody signupReqest = new SignupRequestBody(user.id.ToCharArray(), pw.ToCharArray(), false);
                byte[] body = mc.StructureToByte(signupReqest);
                sm.Send(MessageType.Signup, MessageState.REQUEST, body, ref socket);
                isLock = true;
            }
        }

        //Log in
        public UserInfo LogIn(out bool isLock, out State state)
        {
            Console.Clear();
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                            Log In                              |");
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|        ESC : Exit      F1: Back                                |");
            Console.WriteLine("+----------------------------------------------------------------+");

            user.id = null;
            state = State.LogIn;
            isLock = false;

            Console.Write("ID           : ");
            MessageConvert.KeyType result = mc.TryReadLine(out user.id);

            if (MessageConvert.KeyType.Success == result)
            {
                
                
            }
            else if (result == MessageConvert.KeyType.GoBack)
            {
                state = State.Home;
            }
            else if (result == MessageConvert.KeyType.Exit)
            {
                state =  State.Exit;
            }
            Console.WriteLine();
            Console.Write("Password     : ");
            user.password = mc.ReadPassword();

            LoginRequestBody logInReqest = new LoginRequestBody(user.id.ToCharArray(), user.password.ToCharArray());
            byte[] body = mc.StructureToByte(logInReqest);

            sm.Send(MessageType.LogIn, MessageState.REQUEST, body, ref socket);
            isLock = true;
            return user;

        }

        //Lobby
        public State Lobby(out bool isLock, out List<int> rl, List<int> room)
        {
           
            Console.Write("> ");
            isLock = false;
            String menu = null;
            MessageConvert.KeyType result = mc.TryReadLine(out menu);
            rl = room;

            if (MessageConvert.KeyType.Success == result)
            {
                switch (menu)
                {
                    case "1":
                    case "l":
                        ListRoom();
                        isLock = true;
                        break;

                    case "2":
                    case "c":
                        CreateRoom();
                        isLock = true;
                        break;

                    case "3":
                    case "j":
                        return State.Join;

                    default:
                        Console.WriteLine("\n[!]Wrong Input");
                        Console.WriteLine("Press any key to retry....");
                        Console.ReadKey();
                        break;

                }
            }
            else if (result == MessageConvert.KeyType.Exit)
            {
                return State.Exit;
            }
            else if (result == MessageConvert.KeyType.GoBack || result == MessageConvert.KeyType.LogOut)
            {

                LoginRequestBody requset = new LoginRequestBody(user.id.ToCharArray(), "-".ToCharArray());
                byte[] body = mc.StructureToByte(requset);
                sm.Send(MessageType.LogOut, MessageState.REQUEST, body, ref socket);
                isLock = true;
                return State.Home;

            }
            else if (result == MessageConvert.KeyType.Delete)
            {
                rl = null;
                return State.Lobby;
            }
            return State.Lobby;
            
        }
        public void ListRoom()
        {
            RoomRequestBody roomReqest = new RoomRequestBody(0);
            byte[] body = mc.StructureToByte(roomReqest);
            sm.Send(MessageType.Room_List, MessageState.REQUEST, body, ref socket);
        }

        public void CreateRoom()
        {
            RoomRequestBody createRoom = new RoomRequestBody(0);
            byte[] body = mc.StructureToByte(createRoom);
            sm.Send(MessageType.Room_Create, MessageState.REQUEST, body, ref socket);
           
        }



        public State LeaveRoom(int roomno, out bool isLock)
        {
            RoomRequestBody LeaveRoom = new RoomRequestBody(roomno);
            byte[] body = mc.StructureToByte(LeaveRoom);
            sm.Send(MessageType.Room_Leave, MessageState.REQUEST, body, ref socket);
            isLock = true;
            return State.Lobby;

        }


        //Room
        public State Room(int room, out bool isLock)
        {
            String chat = null;
            MessageConvert.KeyType result = mc.TryReadLine(out chat);
            isLock = false;

            if (result == MessageConvert.KeyType.Success)
            {
                byte[] body = mc.StringToByte(chat);
                sm.Send(MessageType.Chat_MSG_From_Client, MessageState.REQUEST, body, ref socket);
                isLock = true;
                return State.Chat;
            }
            else if (result == MessageConvert.KeyType.Exit)
            {
                sm.Send(MessageType.Room_Leave, MessageState.REQUEST, null, ref socket);
                isLock = true;
                return State.Exit;
            }
            else if (result == MessageConvert.KeyType.GoBack)
            {
                sm.Send(MessageType.Room_Leave, MessageState.REQUEST, null, ref socket);
                isLock = true;
                return State.Lobby;
            }
            return State.Room;
        }

        public State Chatting()
        {
            String chat = null;
            MessageConvert.KeyType result = mc.TryReadLine(out chat);
            if (MessageConvert.KeyType.Success == result)
            {
                byte[] body = mc.StringToByte(chat);
                sm.Send(MessageType.Chat_MSG_From_Client, MessageState.REQUEST, body, ref socket);
            }
            else if (result == MessageConvert.KeyType.Exit)
            {
                return State.Exit;
            }
            else if (result == MessageConvert.KeyType.GoBack)
            {
                return State.Leave;
            }
            return State.Chat;
        }
        public bool checkStringFormat(string s)
        {
            bool idChecker = Regex.IsMatch(s, @"[0-9a-zA-Z]$");
            return idChecker;

        }
    }
    
}
