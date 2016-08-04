using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;

namespace Dummy
{
    struct UserInfo
    {
        public string id;
        public string password;

    }
    class Chat
    {
        private Socket socket;

        private SocketManager sm;
        private StateManager st;
        private MessageConvert mc = new MessageConvert();

        private UserInfo user;
        private Dummy dummy;
        private List<int> roomList;

        private State state;
        private int roomNo;
        private int heartBeat;
        private bool isHeartBeat;

        private bool toBeContinue;
        private bool isLock;

        public Chat(Socket socket, Dummy dummy)
        {
            this.socket = socket;
            this.dummy = dummy;
            sm = new SocketManager(this.socket);
            st = new StateManager(this.socket);
            
            state = State.Home;
            heartBeat = 0;

            isLock = false;
            toBeContinue = true;

            Thread runState = new Thread(DoState);
            Thread runRecv = new Thread(Receiving);
            runState.Start();
            runRecv.Start();

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 33 * 1000;
            timer.Elapsed += new ElapsedEventHandler(TimerHealthCheck);
            timer.Start();
            heartBeat = 0;
            isHeartBeat = false;
        }
        
        private void TimerHealthCheck(object sender, ElapsedEventArgs e)
        {
            if (isHeartBeat == false && ++heartBeat > 3)
            {
                state = State.Lobby;
                int port = 0;
                MessageConvert mc = new MessageConvert();
                IPAddress ip = mc.GetServerIP(out port);
                ConnectPassing(ip, port);
            }
               
                
        }
        private void DoState()
        {
            
            while (toBeContinue)
            {
                if (!isLock)
                {
                    switch (state)
                    {
                        case State.Home:
                            state = st.Home();
                            break;
                        case State.DupId:
                            state = st.DupID(out isLock);
                            break;
                        case State.SignUp:
                            st.SignUp(out isLock, dummy);
                            break;
                        case State.LogIn:
                            user = st.LogIn(out isLock, dummy);
                            break;
                        case State.Lobby:
                            PrintLobby();
                            state = st.Lobby(out isLock, ref roomList);
                            break;
                        case State.Join_Direct:
                            RoomRequestBody EnterRoom = new RoomRequestBody(roomNo);
                            byte[] body = mc.StructureToByte(EnterRoom);
                            sm.Send(MessageType.Room_Join, MessageState.REQUEST, body);
                            isLock = true;
                            break;
                        case State.Join:
                            Console.WriteLine("Enter Room# : ");
                            Random r = new Random();
                            int max = roomList.Count-1;
                            int rand = r.Next(0, max);
                            roomNo = roomList[rand];
                            state = State.Join_Direct;
                            break;

                        case State.Room:
                            state = st.Room(roomNo, out isLock);
                            break;
                        case State.Leave:
                            st.LeaveRoom(roomNo, out isLock);
                            roomNo = 0;
                            break;
                        case State.Chat:
                            state = st.Chatting();
                            break;
                        case State.Exit:

                            st.Exit();
                            toBeContinue = false;
                            break;
                        default:
                            Console.WriteLine("UnKnown State");
                            break;

                    }
                }
            }
            
        }
        private void Receiving()
        {
            while (true)
            {
                if (state == State.Exit)
                    break;
                byte[] body = null;
                try
                {
                    Header header = (Header)sm.Recieve(out body);

                    switch (header.type)
                    {
                        case MessageType.Health_Check:
                            HealthCheck(header, body);
                            break;

                        case MessageType.Signup:
                            Signup(header, body);
                            break;
                        case MessageType.Id_Dup:
                            Id_Dup(header, body);
                            break;

                        case MessageType.LogIn:
                            LogIn(header, body);
                            break;
                        case MessageType.LogOut:
                            LogOut(header, body);
                            break;

                        case MessageType.Room_Create:
                            CreateRoom(header, body);
                            break;
                        case MessageType.Room_Join:
                            JoinRoom(header, body);
                            break;
                        case MessageType.Room_Leave:
                            LeaveRoom(header, body);
                            break;
                        case MessageType.Room_List:
                            ListRoom(header, body);
                            break;

                        case MessageType.Chat_MSG_Broadcast:
                            ReceiveMessage(header, body);
                            break;
                        case MessageType.Chat_MSG_From_Client:
                            SendMessage(header, body);
                            break;

                        default:
                            break;
                    }
                }
                catch (SocketException)
                {
                    Console.Clear();
                    int port = 0;
                    IPAddress ip = mc.GetServerIP(out port);
                    Connection con = new Connection(ip, port);
                    socket = con.startConnection();
                }
            }
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

        }

        //case MessageType.Health_Check:
        private void HealthCheck(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.REQUEST)
            {
                sm.Send(MessageType.Health_Check, MessageState.SUCCESS, null);
                isHeartBeat = true;
                heartBeat = 0;
            }
        }

        //----------Sign Up--------------
        //case MessageType.Signup:
        private void Signup(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.SUCCESS)
            {
                state = State.LogIn;
                isLock = false;
            }
            else
            {
                Console.WriteLine("[!]Fail to Sign up");
                Console.WriteLine("Press any key to retry....");
                Console.ReadKey();
                isLock = false;
            }

        }
        //case MessageType.Id_Dup:
        private void Id_Dup(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.SUCCESS)
            {
                state = State.SignUp;
                isLock = false;
            }
            else
            {
                Console.WriteLine("\n[!]Duplicated ID");
                Console.WriteLine("Press any key to retry....");
                Console.ReadKey();
                isLock = false;
            }

        }

        //case MessageType.LogIn:
        private void LogIn(Header header, byte[] body)
        {
            Header h = header;

            if (!h.Equals(null) && h.state == MessageState.SUCCESS)
            {
                state = State.Lobby;
                isLock = false;
            }
            else
            {
                state = State.SignUp;
                isLock = false;
            }
        }
        //case MessageType.LogOut:
        private void LogOut(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.SUCCESS)
            {
                state = State.Home;
                isLock = false;
            }
            else
            {
                Console.WriteLine("[!]Fail to Log Out");
                Console.WriteLine("Press any key to retry....");
                Console.ReadKey();
            }
        }

        //case MessageType.Room_Create:
        private void CreateRoom(Header header, byte[] body)
        {
            Header h = header;
            
            if (h.state == MessageState.SUCCESS)
            {
                roomNo = BitConverter.ToInt32(body, 0);
                state = State.Join_Direct;
                isLock = false;
            }
            else
            {
                Console.WriteLine("[!]Fail to Create Room");
                Console.WriteLine("Press any key to retry....");
                Console.ReadKey();
            }
        }
        //case MessageType.Room_Join:
        private void JoinRoom(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.SUCCESS)
            {
                state = State.Chat;
                isLock = false;
                Console.Clear();
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine("|                           Room {0}                               |", roomNo);
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine("|        ESC : Exit      F1: Back                                |");
                Console.WriteLine("+----------------------------------------------------------------+");
            }
            else
            {
                if (0 == h.length)
                {
                    roomList = null;
                }
                else
                {
                    JoinFailBody joinFail = (JoinFailBody)mc.ByteToStructure(body, typeof(JoinFailBody));
                    string id = new string(joinFail.ip);
                    id = id.Split('\0')[0];
                    ConnectPassing(IPAddress.Parse(id), joinFail.port);
                    state = State.Join_Direct;
                }
                isLock = false;
            }

        }

        //case MessageType.Room_Leave:
        private void LeaveRoom(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.SUCCESS)
            {
                if (state != State.Exit)
                {
                    LoginRequestBody requset = new LoginRequestBody(user.id.ToCharArray(), "-".ToCharArray());
                    byte[] bytes = mc.StructureToByte(requset);
                    sm.Send(MessageType.LogOut, MessageState.REQUEST, bytes);
                    isLock = true;
                    state = State.Exit;
                }
                isLock = false;
            }
            else
            {   
                Console.WriteLine("[!]Fail to leave Room");
                Console.WriteLine("Press any key to retry....");
                Console.ReadKey();
            }

        }
        //case MessageType.Room_List:
        private void ListRoom(Header header, byte[] body)
        {
            Header h = header;
            isLock = true;
            if (h.length == 0)
            {
                roomList = new List<int>();
            }

            else if (h.state == MessageState.SUCCESS)
            {

                roomList = mc.BytesToList(body);
            }

            isLock = false;
        }

        //case MessageType.Chat_MSG_Broadcast:
        private void ReceiveMessage(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.REQUEST)
            {
                
                ChatResponseBody chatbody = (ChatResponseBody)mc.ByteToStructure(body, typeof(ChatResponseBody));
                string id = new string(chatbody.id);
                DateTime date = chatbody.date;
                int bodyLen = chatbody.msgLen;

                byte[] buffer = new byte[bodyLen];
                int readBytes = socket.Receive(buffer);

                string msg = mc.ByteToString(buffer);

                id = id.Split('\0')[0];
                if ( id == user.id)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new String(' ', Console.BufferWidth));
                    
                    Console.WriteLine("[{0}]", date);
                    Console.WriteLine("[{0}]{1}",id,msg);
                   
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("[{0}]", date);
                    Console.WriteLine("[{0}]{1}", id, msg);
                }

            }
            else
            {
                Console.WriteLine("[!]Fail to Receive Message");
            }

        }

        //case MessageType.Chat_MSG_From_Client:
        private void SendMessage(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.SUCCESS)
            {
                Console.WriteLine();
                
            }
            else
            {
                Console.WriteLine("[!]Fail to Send Message");
            }

        }

        private void PrintLobby()
        {
            Console.Clear();
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                             Lobby                              |");
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("| 1. RoomList            2. Create Room          3. Join Room    |");
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("| ESC : Exit     F1: Back      F2: LogOut      F3: Delete List   |");
            Console.WriteLine("+----------------------------------------------------------------+");
            PrintRoomList();
        }

        private void PrintRoomList()
        {
            if(roomList!=null)
                foreach (int room in roomList)
                {
                    Console.WriteLine("room #" + room);
                }
        }

        private void ConnectPassing(IPAddress ip, int port)
        {
            Connection con = new Connection(ip, port);
            Socket temp = con.startConnection();
            socket.Close();
            socket = temp;
            LoginRequestBody logInReqest = new LoginRequestBody(user.id.ToCharArray(), user.password.ToCharArray());
            byte[] body = mc.StructureToByte(logInReqest);

            sm = new SocketManager(socket);
            st = new StateManager(socket);

            sm.Send(MessageType.LogIn, MessageState.REQUEST, body);
            Header header = (Header)sm.Recieve(out body);
        }


    }
}
