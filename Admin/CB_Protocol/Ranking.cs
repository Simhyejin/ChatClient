
using System.Runtime.InteropServices;

namespace Admin
{
    // containing single rank information
    // will be used as struct array to provide Ranking information
    public struct Ranking
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] id; // user id
        public int rank; // user's rank 
    }
}
