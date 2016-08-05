using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;

namespace Admin
{
    struct UserInfo
    {
        public string id;
        public string password;

    }
    class Monitor
    {
        private Socket socket;

        private SocketManager sm;
        private StateManager st;
        private MessageConvert mc = new MessageConvert();

        private UserInfo user;
        private State state;
        private System.Timers.Timer timer;

        //private int heartBeat;
        //private bool isHeartBeat;

        private bool toBeContinue;
        private bool isLock;

        public Monitor(Socket socket)
        {
            this.socket = socket;
            sm = new SocketManager();
            st = new StateManager(this.socket);

            state = State.Home;
            //heartBeat = 0;

            isLock = false;
            toBeContinue = true;

            Thread runState = new Thread(DoState);
            Thread runRecv = new Thread(Receiving);
            runState.Start();
            runRecv.Start();

            timer = new System.Timers.Timer();
            timer.Interval = 6 * 1000;
            timer.Elapsed += new ElapsedEventHandler(RequestTimer);
            //heartBeat = 0;
            //isHeartBeat = false;
        }

        public void RequestTimer(object sender, ElapsedEventArgs e)
        {
            Console.Clear();
            sm.Send(MessageType.Total_Room_Count,MessageState.REQUEST, null, ref socket);
            sm.Send(MessageType.User_Status, MessageState.REQUEST, null, ref socket);
            sm.Send(MessageType.Chat_Ranking, MessageState.REQUEST, null, ref socket);
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                    4:33 Chat Monitor system                    |");
            Console.WriteLine("|                Type \"exit\", if you want to exit                |");
            Console.WriteLine("+----------------------------------------------------------------+");
        }
        public void DoState()
        {
            while (toBeContinue)
            {
                if (!isLock)
                {
                    switch (state)
                    {
                        case State.Home:
                            timer.Stop();
                            user = st.HomeState(out isLock);
                            break;

                        case State.Monitor:
                            sm.Send(MessageType.Total_Room_Count, MessageState.REQUEST, null, ref socket);
                            sm.Send(MessageType.User_Status, MessageState.REQUEST, null, ref socket);
                            sm.Send(MessageType.Chat_Ranking, MessageState.REQUEST, null, ref socket);
                            timer.Start();
                            state = st.MonitorState(out isLock);
                            break;

                        case State.Exit:
                            timer.Stop();
                            st.ExitState();
                            toBeContinue = false;
                            break;
                        default:
                            Console.WriteLine("[!]UnKnown State");
                            break;

                    }
                }
            }

        }

        public void Receiving()
        {
            while (true)
            {
                if (state == State.Exit)
                    break;

                byte[] body = null;
                try {
                    Header header = (Header)sm.Recieve(out body, ref socket);
                

                    switch (header.type)
                    {

                        case MessageType.LogIn:
                            LogIn(header, body);
                            break;
                        case MessageType.LogOut:
                            LogOut(header, body);
                            break;

                        //FE당 유저 수
                        case MessageType.User_Status:
                            UserStatus(header, body);
                            break;
                        case MessageType.Chat_Ranking:

                            ChatRanking(header, body);
                            break;
                        case MessageType.Total_Room_Count:
                            TotalRoom(header, body);
                            break;


                        default:
                            Console.WriteLine("[!]UnKnown MessageType");
                            break;
                    }
                }
                catch (Exception)
                {
                    IPAddress ip = IPAddress.Parse("10.100.58.3");
                    int port = 20852;
                    Connection con = new Connection(ip, port);
                    socket = con.Connect();
                }
            }
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

        }

        //case MessageType.Health_Check:
        public void HealthCheck(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.REQUEST)
            {
                sm.Send(MessageType.Health_Check, MessageState.SUCCESS, null, ref socket);
                //isHeartBeat = true;
                //heartBeat = 0;
            }
        }

        //case MessageType.LogIn:
        public void LogIn(Header header, byte[] body)
        {
            Header h = header;

            if (!h.Equals(null) && h.state == MessageState.SUCCESS)
            {
                state = State.Monitor;
                isLock = false;
            }
            else
            {
                Console.WriteLine("[!]Fail to Log in");
                Console.WriteLine("Press any key to retry....");
                Console.ReadKey();
                isLock = false;
            }
        }

        //case MessageType.LogOut:
        public void LogOut(Header header, byte[] body)
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
        // case MessageType.Total_Room_Count:
        public void TotalRoom(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.SUCCESS)
            {
                PrintTotalRoom(body);
            }
            else
            {
                Console.WriteLine("[!]Fail to get TotalRoom");
                Console.WriteLine("Press any key to retry....");
                Console.ReadKey();
            }
        }
        public void PrintTotalRoom(byte[] body)
        {
            int num = BitConverter.ToInt32(body, 0);
            Console.WriteLine("==================================================================");
            Console.WriteLine("|  TOTAL ROOM : {0}                                               |",num);
            Console.WriteLine("==================================================================");


        }

        // case MessageType.User_Status:
        public void UserStatus(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.SUCCESS)
            {
                PrintUserStatus(body, h.length);
            }
            else
            {
                Console.WriteLine("[!]Fail to get UserStatus");
                Console.WriteLine("Press any key to retry....");
                Console.ReadKey();
            }
        }

        public void PrintUserStatus(byte[] body, int len)
        {
            Console.WriteLine();
            Console.WriteLine("==================================================================");
            Console.WriteLine("|  USER STATUS                                                   |");
            Console.WriteLine("==================================================================");
            Console.WriteLine("{0,-10} {1,-20} {2,-10} {3,-10}","[No]","[IP]","[Port]","[# of Users]");
            if (len > 0)
            {
                UserStatus[] userStatus = (UserStatus[]) mc.ByteToUserStatus(body, typeof(UserStatus));
            
                int i = 0;
                foreach (UserStatus us in userStatus)
                {
                    i++;
                    string ip = new string(us.ip);
                    ip = ip.Split('\0')[0];
                    Console.WriteLine("{0,-10} {1,-20} {2,-10} {3,-10}", i, ip, us.port, us.num);
                }
            }

        }
        // case MessageType.Chat_Ranking:
        public void ChatRanking(Header header, byte[] body)
        {
            Header h = header;

            if (h.state == MessageState.SUCCESS)
            {
                PrintChatRanking(body, h.length);
                isLock = false;
            }
            else
            {
                Console.WriteLine("[!]Fail to get Ranking");
                Console.WriteLine("Press any key to retry....");
                Console.ReadKey();
            }
        }

        public void PrintChatRanking(byte[] body, int len)
        {
            Console.WriteLine();
            Console.WriteLine("==================================================================");
            Console.WriteLine("| CHAT RANKING                                                    |");
            Console.WriteLine("==================================================================");
            Console.WriteLine("{0,-5} {1,-20} {2,-10}","[No]", "[Id]", "[Count]");
            if (len > 0)
            {
                Ranking[] ranking = (Ranking[])mc.ByteToRanking(body, typeof(Ranking));

                int i = 0;
                foreach (Ranking rank in ranking)
                {
                    i++;
                    string id = new string(rank.id);
                    id = id.Split('\0')[0];
                    Console.WriteLine("{0,-5} {1,-20} {2,-10}", rank.rank, id, rank.score);
                }
            }
            
        }

        


    }

}