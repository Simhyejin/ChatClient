using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Dummy
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
            sm = new SocketManager(this.socket);
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
            Console.WriteLine("|                Type \"exit\", if you want to exit                |");
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("| 1. Log In                                                      |");
            Console.WriteLine("| 2. Sign Up                                                     |");
            Console.WriteLine("+----------------------------------------------------------------+");

            
            return State.LogIn;
            
        }

        //Sign up
        public State DupID(out bool isLock)
        {
            string id;
            Console.Clear();
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                            Sign up                             |");
            Console.WriteLine("+----------------------------------------------------------------+");

            Console.Write("ID           : ");

            isLock =false;

            KeyType result = mc.TryReadLine(out id);
            if (KeyType.Success == result)
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
                    sm.Send(MessageType.Id_Dup, MessageState.REQUEST, dupBody);
                    isLock = true;
                    user.id = id;
                }
                return State.DupId;

            }
            else if (result == KeyType.GoBack)
            {
                return State.Home;
            }
            else if (result == KeyType.Exit)
            {
                return State.Exit;
            }
            else
            {
                return State.DupId;
            }
        }
        public void SignUp(out bool isLock, Dummy dummy)
        {
            string pw;
            isLock = false;
            Console.WriteLine();
            Console.Write("Password     : ");
            pw = dummy.password;
                

            Console.Write("Passeord again   : ");
            string pw2 = dummy.password;

            
            SignupRequestBody signupReqest = new SignupRequestBody(user.id.ToCharArray(), pw.ToCharArray(), false);
            byte[] body = mc.StructureToByte(signupReqest);
            sm.Send(MessageType.Signup, MessageState.REQUEST, body);
            isLock = true;
        }

        //Log in
        public UserInfo LogIn(out bool isLock, Dummy dummy)
        {
            Console.Clear();
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                            Log In                              |");
            Console.WriteLine("+----------------------------------------------------------------+");

            Console.Write("ID       : ");
            user.id = dummy.id;

            Console.Write("Password     : ");
            user.password = dummy.password;

            LoginRequestBody logInReqest = new LoginRequestBody(user.id.ToCharArray(), user.password.ToCharArray());
            byte[] body = mc.StructureToByte(logInReqest);

            sm.Send(MessageType.LogIn, MessageState.REQUEST, body);
            isLock = true;
            return user;
        }

        //Lobby
        public State Lobby(out bool isLock,  ref List<int> room)
        {
            Console.Write("> ");
            isLock = false;
            
            if (room == null) {
                ListRoom();
                isLock = true;
            }
            else
            {
                if (room.Count == 0)
                {
                    CreateRoom();
                    isLock = true;
                }
                else       
                    return State.Join;
            }
            
            return State.Lobby;
            
        }
        public void ListRoom()
        {
            RoomRequestBody roomReqest = new RoomRequestBody(0);
            byte[] body = mc.StructureToByte(roomReqest);
            sm.Send(MessageType.Room_List, MessageState.REQUEST, body);
        }

        public void CreateRoom()
        {
            RoomRequestBody createRoom = new RoomRequestBody(0);
            byte[] body = mc.StructureToByte(createRoom);
            sm.Send(MessageType.Room_Create, MessageState.REQUEST, body);
           
        }



        public void LeaveRoom(int roomno, out bool isLock)
        {
            RoomRequestBody LeaveRoom = new RoomRequestBody(roomno);
            byte[] body = mc.StructureToByte(LeaveRoom);
            sm.Send(MessageType.Room_Leave, MessageState.REQUEST, body);
            isLock = true;

        }


        //Room
        public State Room(int room, out bool isLock)
        {
            String chat = null;
            KeyType result = mc.TryReadLine(out chat);
            isLock = false;

            if (result == KeyType.Success)
            {
                byte[] body = mc.StringToByte(chat);
                sm.Send(MessageType.Chat_MSG_From_Client, MessageState.REQUEST, body);
                isLock = true;
                return State.Chat;
            }
            else if (result == KeyType.Exit)
            {
                sm.Send(MessageType.Room_Leave, MessageState.REQUEST, null);
                isLock = true;
                return State.Exit;
            }
            else if (result == KeyType.GoBack)
            {
                sm.Send(MessageType.Room_Leave, MessageState.REQUEST, null);
                isLock = true;
                return State.Lobby;
            }
            return State.Room;
        }

        public State Chatting()
        {
            Random r = new Random();
            int min = 5;
            int max = 10;

            int rand = r.Next(min, max);
            for (int i = 0; i < rand; i++)
            {
                DateTime date = new DateTime();
                String chat = date.ToString();
                byte[] body = mc.StringToByte(chat);
                sm.Send(MessageType.Chat_MSG_From_Client, MessageState.REQUEST, body);
            }
            Thread.Sleep(rand * 1000);

           return State.Leave;
           
        }
        public bool checkStringFormat(string s)
        {
            bool idChecker = Regex.IsMatch(s, @"[0-9a-zA-Z]$");
            return idChecker;

        }
    }
    
}
