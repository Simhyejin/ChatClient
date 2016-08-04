using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Admin
{
    // struct for monitoring FE 
    // struct contains information for 1 FE 
    // FE Monitoring information will be sent to Client as struct CBFEUserStatus array 
    public struct UserStatus
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public char[] ip;
        public int port;

        public int num;

        public UserStatus(char[] ip, int port, int num)
        {
            this.ip = new char[15];
            Array.Copy(ip, this.ip, ip.Length);

            this.port = port;
            this.num = num;
        }

    }
}
