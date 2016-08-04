using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Run
{
    class Program
    {
        static void Main(string[] args)
        {
            String no;
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("===========================================");
                Console.WriteLine("|                DUMMY CLIENT              |");
                Console.WriteLine("===========================================");
                Console.WriteLine("How Many?");
                no = Console.ReadLine();
                if (checkStringFormat(no))
                    break;
            }
            int num = int.Parse(no);

            for(int i = 0; i < num; i++)
            {
                Process.Start("C:\\Users\\Yungyung\\Documents\\Visual Studio 2015\\Projects\\Chatting\\Dummy\\bin\\Debug\\Dummy.exe","dummy"+i);
            }
        }
        public static bool checkStringFormat(string s)
        {
            bool idChecker = Regex.IsMatch(s, @"[0-9]$");
            return idChecker;

        }
    }
}
