using System;
using System.Runtime.InteropServices;

namespace Client
{
    struct RoomRequestBody
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        char[] id;
        int roomNo;
        public RoomRequestBody(char[] id, int roomNo)
        {
            this.id = new char[12];
            Array.Copy(id, this.id, id.Length);
            this.roomNo = roomNo;
        }
    }
}
