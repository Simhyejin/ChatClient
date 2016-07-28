using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Chatting
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct Header
    {
        public MessageType type;
        public int length;
        public Header(MessageType type, int lenght)
        {
            this.length = lenght;
            this.type = type;
        }
    }

    public enum MessageType : short
    {
        REQUEST_SIGNUP = 100,
        REQEUST_CHECK_SIGNEDUP = 110,
        REQUEST_LOGIN = 120,

        CHAT_MSG = 200,

        REQUEST_CREATE_ROOM = 310,
        REQUEST_LEAVE_ROOM = 320,
        REQUEST_JOIN_ROOM = 330,
        REQUEST_LIST_ROOM = 340,

        STATUS_SUCCESS = 200,
        STATUS_FAIL = 400,
    };

    class Message 
	{
		public  void ByteArrayToStruct(byte[] buffer, ref Header obj)
		{
			int len = Marshal.SizeOf(typeof(Header));

			IntPtr i = Marshal.AllocHGlobal(len);
			Marshal.Copy(buffer, 0, i, len);
			obj = (Header)Marshal.PtrToStructure(i, obj.GetType());

			Marshal.FreeHGlobal(i);
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
       

    }
}
