using System;
using System.Runtime.InteropServices;

namespace Dummy
{
    struct RoomRequestBody
    {
       
        int roomNo;
        public RoomRequestBody(int roomNo)
        {
            
            this.roomNo = roomNo;
        }
    }
}
