using System.Runtime.InteropServices;

namespace Client
{
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
}
