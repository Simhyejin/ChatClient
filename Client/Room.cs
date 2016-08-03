using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Room
    {
        Socket socket;
        User user;
        int roomNo;

        MessageConvert mc = new MessageConvert();
        SocketManager sm;

        public Room(Socket socket, User user, int roomNo)
        {
            this.socket = socket;
            this.user = user;
            this.roomNo = roomNo;

            sm = new SocketManager(this.socket);
            StartRoom();
        }

        public void StartRoom()
        {
            
            Console.WriteLine("[{0} 번방에 입장하였습니다.]",roomNo);
            Console.ReadLine();
            Thread send = new Thread(() => { SendMSG(); });
            Thread read = new Thread(() => { GetMSG(); });
            //read.ba

        }

        public void SendMSG()
        {
            bool flag = true; 
            while (flag)
            {
                string chat = null;
                KeyType result = mc.TryReadLine(out chat);

                if (KeyType.Success == result)
                {
                    byte[] body = mc.StringToByte(chat);
                    sm.Send(MessageType.Chat_MSG_From_Client, body);
                }
                else
                {
                    flag = false;
                }
            }

        }

        public void GetMSG()
        {
            byte[] body = null;
            Header h = (Header)sm.Recieve(out body);
            switch (h.type)
            {
                case MessageType.Health_Check:
                    break;
                case MessageType.Room_Leave:
                    break;
                case MessageType.Chat_MSG_Broadcast:
                    break;
            }
        }
    }
}

