using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Lobby
    {
        Socket socket;

        MessageConvert mc = new MessageConvert();
        SocketManager sm;

        User user;
        List<int> RoomList;

        public Lobby(Socket socket, User user, out Socket sock)
        {
            this.socket = socket;
            this.user = user;
            sm = new SocketManager(this.socket);
            StartLobby();
            sock = this.socket;
            
        }

        public void StartLobby()
        {
            bool flag = true;

            while (flag) {
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine("|                             Lobby                              |");
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine("| 1. RoomList            2. Create Room          3. Join Room    |");
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.Write("> ");

                String menu = null;
                KeyType result = mc.TryReadLine(out menu);
                Console.WriteLine(result);
                if (KeyType.Success == result)
                {
                    switch (menu)
                    {
                        case "1":
                        case "l":
                            ListRoom();
                            break;

                        case "2":
                        case "c":
                            CreateRoom();
                            break;

                        case "3":
                        case "j":
                            JoinRoom();
                            break;

                        default:
                            Console.WriteLine("[!]잘못된 입력입니다.");
                            break;

                    }
                }
                else if (result == KeyType.Exit)
                {
                      
                }
                else if (result == KeyType.GoBack)
                {
                    flag = false;
                }
                else if (result == KeyType.LogOut)
                {
                    flag = false;
                    LoginRequestBody requset = new LoginRequestBody(user.id.ToCharArray(), "-".ToCharArray());
                    byte[] body = mc.StructureToByte(user);
                    sm.Send(MessageType.LogOut, body);
                }
                Console.Clear();
            }
        }

        public void ListRoom()
        {
            RoomRequestBody roomReqest = new RoomRequestBody(user.id.ToCharArray(), 0);
            byte[] body = mc.StructureToByte(roomReqest);
            sm.Send(MessageType.Room_List, body);
            Header h = (Header)sm.Recieve(out body);

            if (h.length == 0)
                Console.WriteLine("방이 없습니다.");
            else if (h.state == MessageState.SUCCESS)
            {

                RoomList = mc.BytesToList(body);
                foreach (int room in RoomList)
                {
                    Console.WriteLine("room #" + room);
                }
            }
            else
            {
                Console.WriteLine("[!]Fail to get Room List");
            }
        }

        

        public void CreateRoom()
        {
            RoomRequestBody createRoom = new RoomRequestBody(user.id.ToCharArray(), 0);
            byte[] body = mc.StructureToByte(createRoom);
            sm.Send(MessageType.Room_Create, body);
            Header h = (Header)sm.Recieve(out body);

            if (h.state == MessageState.SUCCESS)
            {
                int no = BitConverter.ToInt32(body, 0);
                Join(no);
            }
            else
            {
                Console.WriteLine("[!]Fail to Create Room");
            }
        }

        public void JoinRoom()
        {
            while (true)
            {
                Console.WriteLine("Enter Room# : ");
                string no = Console.ReadLine();
                int room;
                if (int.TryParse(no, out room))
                {
                    if (RoomList.Contains(room))
                    {
                        Join(room);
                        break;
                    }
                }
                Console.WriteLine("Wrong input");

            }
        }

        public void Join(int roomno)
        {
            RoomRequestBody EnterRoom = new RoomRequestBody(user.id.ToCharArray(), roomno);
            byte[] body = mc.StructureToByte(EnterRoom);
            sm.Send(MessageType.Room_Join, body);

            Header h = (Header)sm.Recieve(out body);

            if (h.state == MessageState.SUCCESS)
            {
                Room room = new Room(socket, user, roomno);
            }
            else
            {
                if (0 == h.length) 
                    Console.WriteLine("[!]Fail to get Room List");
                else
                {
                    JoinFailBody joinFail = (JoinFailBody)mc.ByteToStructure(body,typeof(JoinFailBody));
                    int port = 0;
                    IPAddress ip = mc.GetServerIP(out port);
                    Connection con = new Connection(ip, port);
                    socket = con.startConnection();
                    Room room = new Room(socket, user, roomno);

                }
            }

        }

    }
}
