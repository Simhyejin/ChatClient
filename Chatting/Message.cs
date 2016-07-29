using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Chatting
{ 
    struct User
    {
        string id;
        string password;
        public User(string id, string pw)
        {
            this.id = id;
            this.password = pw;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct Header
    {
        public MessageType type;
        public MessageState state;
        public int length;
       
        public Header(MessageType type, MessageState state, int lenght)
        {
            this.type = type;
            this.state = state;
            this.length = lenght;
        }

    }

    public enum MessageType : short
    {
        Id_Dup = 110,
        Signup = 120,

        Signin = 210,

        Room_Create = 310,
        Room_Leave = 320,
        Room_Join = 330,
        Room_List = 340,

        Chat_MSG_From_Client = 410,
        Chat_MSG_Broadcast = 420,
    };

    public enum MessageState : short
    {
        REQUEST = 100,
        SUCCESS = 200,
        FAIL = 400
    }

    struct SignRequestBody
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        char[] id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        char[] password;
        bool isDummy;

        public SignRequestBody(char[] id, char[] password, bool dummy)
        {
            this.id = new char[12];
            this.password = new char[16];
            Array.Copy(id, this.id, id.Length);
            Array.Copy(password, this.password, password.Length);
            isDummy = false;
        }
    }
 

    struct RoomRequestBody
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        char[] id;
        int roomNo;
        public RoomRequestBody(char[] id, int roomNo)
        {
            this.id = new char[12];
            Array.Copy(id, this.id, id.Length);
            this.id = id;
            this.roomNo = roomNo;
        }
    }

    class Message 
	{
		
        public object ByteToStructure(byte[] data, Type type)
        {
            IntPtr buff = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, buff, data.Length);
            object obj = Marshal.PtrToStructure(buff, type);
            Marshal.FreeHGlobal(buff);

            if (Marshal.SizeOf(obj) != data.Length)
            {
                return null;
            }

            return obj;
        }


        public  byte[] StructureToByte(object obj)
        {
            int datasize = Marshal.SizeOf(obj);
            IntPtr buff = Marshal.AllocHGlobal(datasize); 
            Marshal.StructureToPtr(obj, buff, false); 
            byte[] data = new byte[datasize]; 
            Marshal.Copy(buff, data, 0, datasize); 
            Marshal.FreeHGlobal(buff); 
            return data; 
        }
       
        public string ByteToString(byte[] buff)
        {
            return Encoding.UTF8.GetString(buff);
        }

        public byte[] StringToByte(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

    }
}
