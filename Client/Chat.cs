﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;

namespace Client
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
        private List<int> RoomList;

        private State state; 
        private int roomNo;

        System.Timers.Timer timer;
        private int heartBeat; //HeartBeat Count
        private bool isHeartBeat;

        private bool toBeContinue; //State
        private bool isLockState; // thread lock

        public Chat(Socket socket)
        {
            this.socket = socket;
            sm = new SocketManager();
            st = new StateManager(this.socket);

            state = State.Home;
            heartBeat = 0;

            isLockState = false;
            toBeContinue = true;

            Thread runState = new Thread(DoState);
            Thread runRecv = new Thread(Receiving);
            runState.Start();
            runRecv.Start();

            timer = new System.Timers.Timer();
            timer.Interval = 10 * 1000;
            timer.Elapsed += new ElapsedEventHandler(TimerHealthCheck);
            timer.Start();
            heartBeat = 0;
            isHeartBeat = false;
        }
        
        private void TimerHealthCheck(object sender, ElapsedEventArgs e)
        {
            if (!isHeartBeat && ++heartBeat > 3)
            {
                heartBeat = 0;
                int port = 0;
                MessageConvert mc = new MessageConvert();
                IPAddress ip = mc.GetServerIP(out port);
                ReConnect(ip, port);
                state = State.Home;
                isLockState = false;
                
            }
            isHeartBeat = false;
        }

        private void DoState()
        {
            while (toBeContinue)
            {
                if (!isLockState)
                {
                    switch (state)
                    {
                        case State.Home:
                            state = st.Home();
                            break;
                        case State.DupId:
                            state = st.DupID(out isLockState);
                            break;
                        case State.SignUp:
                            st.SignUp(out isLockState);
                            break;
                        case State.LogIn:
                            user = st.LogIn(out isLockState, out state);
                            break;
                        case State.Lobby:
                            PrintLobby();
                            state = st.Lobby(out isLockState, out RoomList, RoomList);
                            break;
                        case State.Join_Direct:
                            RoomRequestBody EnterRoom = new RoomRequestBody(roomNo);
                            byte[] body = mc.StructureToByte(EnterRoom);
                            sm.Send(MessageType.Room_Join, MessageState.REQUEST, body, ref socket);
                            isLockState = true;
                            break;
                        case State.Join:
                            while (true)
                            {
                                Console.WriteLine();
                                Console.Write("Enter Room# : ");
                                string no = Console.ReadLine();
                                
                                if (int.TryParse(no, out roomNo))
                                {
                                        state = State.Join_Direct;
                                        break;
                                }
                                Console.WriteLine("[!]Wrong input");

                            }
                            break;

                        case State.Room:
                            state = st.Room(roomNo, out isLockState);
                            break;
                        case State.Leave:
                            state = st.LeaveRoom(roomNo, out isLockState);
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
                    Header header = (Header)sm.Receive(out body, ref socket);

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
                    ReConnect(ip, port);
                    state = State.Home;
                    isLockState = false;
                }
                catch (Exception)
                {
                    Console.Clear();
                    int port = 0;
                    IPAddress ip = mc.GetServerIP(out port);
                    ReConnect(ip, port);
                    state = State.Home;
                    isLockState = false;
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
                timer.Interval = 33 * 1000;
                //sm.Send(MessageType.Health_Check, MessageState.SUCCESS, null, ref socket);
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
                isLockState = false;
            }
            else
            {
                Console.WriteLine("[!]Fail to Sign up");
                Console.WriteLine("Press any key to retry....");
                Console.ReadKey();
            }

        }
        //case MessageType.Id_Dup:
        private void Id_Dup(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.SUCCESS)
            {
                state = State.SignUp;
                isLockState = false;
            }
            else
            {
                Console.WriteLine("\n[!]Duplicated ID");
                Console.WriteLine("Press any key to retry....");
                Console.ReadKey();
                isLockState = false;
            }

        }

        //case MessageType.LogIn:
        private void LogIn(Header header, byte[] body)
        {
            Header h = header;

            if (!h.Equals(null) && h.state == MessageState.SUCCESS)
            {
                state = State.Lobby;
                isLockState = false;
            }
            else
            {
                Console.WriteLine("[!]Fail to Log in");
                Console.WriteLine("Press any key to retry....");
                Console.ReadKey();
                isLockState = false;
            }
        }

        //case MessageType.LogOut:
        private void LogOut(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.SUCCESS)
            {
                state = State.Home;
                isLockState = false;
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
                isLockState = false;
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
                isLockState = false;
                Console.Clear();
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine("|                           Room {0}                               |", roomNo);
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine("|        ESC : Exit      F1: Back                                |");
                Console.WriteLine("+----------------------------------------------------------------+");
            }
            else if (h.state == MessageState.FAIL)
            {
                if (0 == h.length)
                {
                    Console.WriteLine("[!]Fail to Join Room");
                    Console.WriteLine("Press any key to retry....");
                    Console.ReadKey();
                }
                else
                {
                    JoinFailBody joinFail = (JoinFailBody)mc.ByteToStructure(body, typeof(JoinFailBody));
                    string id = new string(joinFail.ip);
                    id = id.Split('\0')[0];
                    ConnectPassing(IPAddress.Parse(id), joinFail.port);
                    state = State.Join_Direct;
                }
                isLockState = false;
            }
            else if(h.state == MessageState.REQUEST)
            {
                ChatResponseBody chatbody = (ChatResponseBody)mc.ByteToStructure(body, typeof(ChatResponseBody));
                string id = new string(chatbody.id);
                DateTime date = chatbody.date;
                int bodyLen = chatbody.msgLen;

                if(bodyLen > 0)
                {
                    byte[] buffer = new byte[bodyLen];
                    int readBytes = socket.Receive(buffer);

                    string msg = mc.ByteToString(buffer);
                }
                
                id = id.Split('\0')[0];

                
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new String(' ', Console.BufferWidth));
                Console.WriteLine("*****{0,30} {1,12} enter the room ****** ", date, id);


            }

        }

        //case MessageType.Room_Leave:
        private void LeaveRoom(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.SUCCESS)
            {
                if(state != State.Exit)
                    state = State.Lobby;
                st.ListRoom();
            }
            else if (h.state == MessageState.FAIL)
            {   
                Console.WriteLine("[!]Fail to leave Room");
            }
            else if (h.state == MessageState.REQUEST)
            {
                ChatResponseBody chatbody = (ChatResponseBody)mc.ByteToStructure(body, typeof(ChatResponseBody));
                string id = new string(chatbody.id);
                DateTime date = chatbody.date;
                int bodyLen = chatbody.msgLen;

                if (bodyLen > 0)
                {
                    byte[] buffer = new byte[bodyLen];
                    int readBytes = socket.Receive(buffer);

                    string msg = mc.ByteToString(buffer);
                }

                id = id.Split('\0')[0];


                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new String(' ', Console.BufferWidth));
                Console.WriteLine("*****{0,30} {1,12} left the room ******* ", date, id);

            }

        }

        //case MessageType.Room_List:
        private void ListRoom(Header header, byte[] body)
        {
            RoomList = null;
            Header h = header;
            isLockState = true;
            if (h.length == 0)
            {
                ;
            }

            else if (h.state == MessageState.SUCCESS)
            {

                RoomList = mc.BytesToList(body);
            }

            isLockState = false;
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

            if (h.state != MessageState.SUCCESS)
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
            if(RoomList != null)
            {
                int i = 1;
                foreach (int room in RoomList)
                {
                    Console.WriteLine("[{0,3}]room {1}",i, room);
                    i++;
                }
            }
        }

        private void ReConnect(IPAddress ip, int port)
        {
            Connection con = new Connection(ip, port);
            Socket temp = con.Connect();
            //socket.Close();
            socket = temp;

            sm = new SocketManager();
            st = new StateManager(socket);
        }
        private void ConnectPassing(IPAddress ip, int port)
        {
            ReConnect(ip, port);
            LoginRequestBody logInReqest = new LoginRequestBody(user.id.ToCharArray(), user.password.ToCharArray());
            byte[] body = mc.StructureToByte(logInReqest);

            sm.Send(MessageType.LogIn, MessageState.REQUEST, body, ref socket);
            Header header = (Header)sm.Receive(out body, ref socket);
        }


    }
}
