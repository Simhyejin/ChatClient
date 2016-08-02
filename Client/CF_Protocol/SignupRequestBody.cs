using System;
using System.Runtime.InteropServices;

namespace Client
{
    struct SignupRequestBody
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        char[] id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        char[] password;
        bool isDummy;

        public SignupRequestBody(char[] id, char[] password, bool dummy)
        {
            this.id = new char[12];
            this.password = new char[16];
            Array.Copy(id, this.id, id.Length);
            Array.Copy(password, this.password, password.Length);
            isDummy = false;
        }
    }
}
