using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dummy
{
     struct ChatResponseBody
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] id;
        public DateTime date;
        public int msgLen; //lenght of next body 

        public ChatResponseBody(char[] id, DateTime date, int len)
        {
            this.id = new char[12];
            Array.Copy(id, this.id, id.Length);
            this.date = date;
            msgLen = len;
        }
    }
    
}
