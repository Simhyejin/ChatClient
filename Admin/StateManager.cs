using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Admin
{

    public enum State  
    {
        Exit,
        Home,
        Monitor
    };

    class StateManager
    {
        Socket socket;
        UserInfo user;

        MessageConvert mc;
        SocketManager sm;

        //Home
        public StateManager(Socket socket)
        {
            user = new UserInfo();
            this.socket = socket;
            mc = new MessageConvert();
            sm = new SocketManager(this.socket);
        }

        //Exit Program.
        public void ExitState()
        {
            Console.WriteLine();
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                  Closed Program. Good Bye                      |");
            Console.WriteLine("+----------------------------------------------------------------+");
           
        }

        //Home
        public UserInfo HomeState(out bool isLock)
        {
            Console.Clear();
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                    4:33 Chat Monitor system                    |");
            Console.WriteLine("|                Type \"exit\", if you want to exit                |");
            Console.WriteLine("+----------------------------------------------------------------+");

            Console.Write("ID       : ");
            user.id = Console.ReadLine();

            Console.Write("Password     : ");
            user.password = mc.ReadPassword();

            LoginRequestBody logInReqest = new LoginRequestBody(user.id.ToCharArray(), user.password.ToCharArray());
            byte[] body = mc.StructureToByte(logInReqest);

            sm.Send(MessageType.LogIn, MessageState.REQUEST, body);
            isLock = true;
            return user;
            
        }

        public State MonitorState(out bool isLock)
        {
            Console.Clear();
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                             MONITOR                            |");
            Console.WriteLine("+----------------------------------------------------------------+");
            String input = null;
            KeyType result = mc.TryReadLine(out input);
            isLock = false;

            if (result == KeyType.Exit)
            {
                return State.Exit;
            }
            else if (result == KeyType.LogOut || result == KeyType.GoBack)
            {
                LoginRequestBody requset = new LoginRequestBody(user.id.ToCharArray(), "-".ToCharArray());
                byte[] body = mc.StructureToByte(requset);
                sm.Send(MessageType.LogOut, MessageState.REQUEST, body);
                isLock = true;
                return State.Home;
            }
            else
                return State.Monitor;
        }
    

    }
    
}
