using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Run
    {
        private static bool isclosing = false;

        public static void Main(string[] args)
        {
            Random r = new Random();
            int rand = r.Next(1, 4);
            string randomIP = ConfigurationManager.AppSettings[rand];

            Connection conn = new Connection(IPAddress.Parse(randomIP), 11000);

            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);
            SetConsoleCtrlHandler(ConsoleCtrlCheck, true);


        }
        #region unmanaged
        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate.

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        #endregion

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            // Put your own handler here
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                    isclosing = true;
                    Environment.Exit(0);
                    break;

                case CtrlTypes.CTRL_BREAK_EVENT:
                    isclosing = true;
                    Console.WriteLine("CTRL+BREAK received!");
                    break;

                case CtrlTypes.CTRL_CLOSE_EVENT:
                    isclosing = true;
                    Console.WriteLine("Program being closed!");
                    break;

                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    isclosing = true;
                    Console.WriteLine("User is logging off!");
                    break;

            }
            return true;
        }

        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                Environment.Exit(0);
            }
            return false;
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
                                               // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

    }

   
}
