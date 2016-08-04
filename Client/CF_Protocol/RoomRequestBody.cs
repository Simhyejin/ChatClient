using System;
using System.Runtime.InteropServices;

namespace Client
{
    struct RoomRequestBody
    {
        int roomNo;
        public RoomRequestBody( int roomNo)
        {
            this.roomNo = roomNo;
        }
    }
}
