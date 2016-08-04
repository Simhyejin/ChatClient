using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Admin
{
    // struct for Client ~ BE Header
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct Header
    {
        public MessageType type; // message type
        public MessageState state; // message status
        public int length; // body length 

        public Header(MessageType type, MessageState state, int lenght)
        {
            this.type = type;
            this.state = state;
            this.length = lenght;
        }
    }
    // 
    public enum MessageType : short
    {

        Total_Room_Count = 110, // type for request total room number in application
        User_Status = 210, // type for request FE' user number
        Chat_Ranking = 310, // type for request chatting ranking 

        LogIn = 410,
        LogOut = 420,

        Health_Check = 510

    };

    public enum MessageState : short
    {
        REQUEST = 100, // state that this message is 'Request'
        SUCCESS = 200, // state that this message is Response messgae with 'Success'
        FAIL = 400 // state that this message is Response message with 'Fail'
    }
}
