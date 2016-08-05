using System;
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

        private UserInfo user;              // Current User Info(id, password)
        private State state;                // Current State ( Exit, Home, LogIn, DupId, SignUp, Lobby, Join, Join_Direct, Room, Leave, Chat)

        private int roomNo;                 // Room Number
        private List<int> RoomList;         // Room List
        
        private System.Timers.Timer timer;  // Health Cheak Timer
        private int heartBeat;              // HeartBeat Count
        private bool isHeartBeat;           // If Receive Health Check Message, True 
                                            // Else, False


        private bool toBeContinue;          // To keep going While Changing State thread
        private bool isLockState;           // To lock Changing State thread

        public Chat(Socket socket)
        {
            this.socket = socket;
            sm = new SocketManager();
            st = new StateManager(this.socket);

            state = State.Home;             // Init State to Home
            heartBeat = 0;                  

            isLockState = false;    
            toBeContinue = true;    

            Thread runState = new Thread(ChangeState);  // Create thread for changing State
            Thread runRecv = new Thread(Receiving);     // Create thread for Receiving 
            runState.Start();
            runRecv.Start();

            timer = new System.Timers.Timer();          // health Check timer
            timer.Interval = 30 * 1000;                 // set health Check  Interval time
            timer.Elapsed += new ElapsedEventHandler(TimerHealthCheck);
            timer.Start();
            heartBeat = 0;
            isHeartBeat = false;
        }

        /// <summary>
        /// Timer event to count Health Check Message.
        /// If missed 3 Health Check Message, Reconnect to ont of servers.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Working function of Changing State Thread 
        /// </summary>
        private void ChangeState()
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

                        case State.DupId:   // Check id is dupplicated
                            state = st.DupID(out isLockState);
                            break;

                        case State.SignUp: // Request Signup
                            st.SignUp(out isLockState);
                            break;

                        case State.LogIn:  // Request Login
                            user = st.LogIn(out isLockState, out state);
                            break;

                        case State.Lobby: // RoomList, Create Room, Join Room
                            state = st.Lobby(out isLockState, out RoomList, RoomList);
                            break;
                            
                        case State.Join_Direct:  // Request Join Room 
                            RoomRequestBody EnterRoom = new RoomRequestBody(roomNo);
                            byte[] body = mc.StructureToByte(EnterRoom);
                            sm.Send(MessageType.Room_Join, MessageState.REQUEST, body, ref socket);
                            isLockState = true;
                            break;

                        case State.Join:    // Get Input room number
                            while (true)
                            {
                                Console.WriteLine();
                                Console.Write("Enter Room# : ");           
                                string no = Console.ReadLine();              // Input

                                if (int.TryParse(no, out roomNo))           // Check input is Number
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

                        case State.Leave: // Request Leave room
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

        /// <summary>
        /// Working function of Receiving Thread.
        /// Receive Message and process the message.
        /// </summary>
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
                catch (SocketException)     // Catch Socket Exception and Reconnection
                {
                    Console.Clear();
                    int port = 0;
                    IPAddress ip = mc.GetServerIP(out port);
                    ReConnect(ip, port);
                    state = State.Home;
                    isLockState = false;
                        
                    Thread.Sleep(1000);
                }
                catch (Exception)           // Catch Unhandled Exception and Reconnection
                {
                    lock (socket)
                    {
                        Console.Clear();
                        int port = 0;
                        IPAddress ip = mc.GetServerIP(out port);
                        ReConnect(ip, port);
                        state = State.Home;
                        isLockState = false;
                        Thread.Sleep(1000);
                    }
                }
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            
        }

        #region
        //case MessageType.Health_Check:
        /// <summary>
        /// Send Response Message of Health Check.
        /// isHeartBeat varable set true
        /// hearBeat Count set zero
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        private void HealthCheck(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.REQUEST)
            {
                timer.Interval = 33 * 1000;
                sm.Send(MessageType.Health_Check, MessageState.SUCCESS, null, ref socket);
                isHeartBeat = true;
                heartBeat = 0;
            }
        }

        
        //case MessageType.Signup:
        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
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
        #endregion


        private void ReConnect(IPAddress ip, int port)
        {
            Connection con = new Connection(ip, port);
            Socket temp = con.Connect();
            socket.Close();
            socket = temp;
            
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
