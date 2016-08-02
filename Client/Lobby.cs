using System;
using System.Collections.Generic;
using System.Linq;
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

        public Lobby(Socket socket, User user)
        {
            this.socket = socket;
            this.user = user;
            sm = new SocketManager(this.socket);
            StartLobby();
        }

        public void StartLobby()
        {
            Console.Clear();
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                             Lobby                              |");
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("| 1. RoomList            2. Create Room          3. Join Room    |");
            Console.WriteLine("+----------------------------------------------------------------+");

            Console.WriteLine();
            Console.Write("> ");

//            string menu = Console.ReadLine();
//            menu = menu.ToLower().Replace(" ", "");

            ConsoleKeyInfo key = Console.ReadKey();



            switch (key.KeyChar)
            {
                //Login
                case '1':
                case 'l':
                    ListRoom();
                    break;

                case '2':
                case 'c':

                    CreateRoom();
                    break;

                case '3':
                case 'j':
                    JoinRoom();
                    break;

                case '\b':
                    break;

                default:
                    StartLobby();
                    break;

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

                RoomList = BytesToList(body);
                foreach (int room in RoomList)
                {
                    Console.WriteLine("room #" + room);
                }
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("[!]Fail to get Room List");
                StartLobby();
            }
        }

        public List<int> BytesToList(byte[] body)
        {
            List<int> list = new List<int>();
            for (int idx = 0; idx < (body.Length % 4); idx++)
            {
                byte[] tmpArr = new byte[4];
                Array.Copy(body, idx * 4, tmpArr, 0, 4); // tmpArr에 byte4개 들어가있음.
                
                int tmp = BitConverter.ToInt32(tmpArr,0);
                list.Add(tmp);
            }
            return list;
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
                Console.WriteLine(no);
                //JoinRoom(no);
                StartLobby();
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
                Console.WriteLine("[" + roomno + "] 번 방에 입장하셨습니다.");
                StartLobby();
            }
            else
            {
                Console.WriteLine("[!]Fail to get Room List");
            }

        }

    }
}
