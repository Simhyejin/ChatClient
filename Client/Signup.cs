using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Signup
    {
        Socket socket;

        MessageConvert mc = new MessageConvert();
        SocketManager sm;

        public Signup(Socket socket)
        {
            this.socket = socket;
            sm = new SocketManager(this.socket);
            StartSignUp();
        }

        public void StartSignUp()
        {
            ConsoleKeyInfo key = Console.ReadKey();
            while (!key.Equals(ConsoleKey.Escape))
            {
                SignUp();
            }
            Home home = new Home(socket);
        }

        public void SignUp()
        {
            string id;
            string pw;
            bool flag = true;

            while (flag) {
                Console.Clear();
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine("|                            Sign up                             |");
                Console.WriteLine("+----------------------------------------------------------------+");

                while (true)
                {

                    Console.Write("ID       : ");
                    id = Console.ReadLine();
                    
                    if (id.Length > 10 || id.Length < 3)
                    {
                        Console.WriteLine("[!]ID lenght should be between 3 and 12");
                        continue;
                    }

                    LoginRequestBody dupReqest = new LoginRequestBody(id.ToCharArray(), "-".ToCharArray());
                    byte[] dupBody = mc.StructureToByte(dupReqest);
                    Console.WriteLine("dupBody.Length: " + dupBody.Length);
                    sm.Send(MessageType.Id_Dup, dupBody);

                    Header header = (Header)sm.Recieve(out dupBody);

                    if (header.state == MessageState.SUCCESS)
                        break;
                    else
                        Console.WriteLine("[!]Duplicated ID");

                }

                while (true)
                {
                    Console.Write("Password     : ");
                    pw = mc.ReadPassword();
                    if (pw.Length > 10 || pw.Length < 4)
                    {
                        Console.WriteLine("[!]Password lenght should be between 4 and 16");
                        continue;
                    }

                    Console.Write("Passeord again   : ");
                    string pw2 = mc.ReadPassword();

                    if (pw != pw2)
                    {
                        Console.WriteLine("[!] Passwords must match");
                        continue;
                    }
                    break;
                }

                SignupRequestBody signupReqest = new SignupRequestBody(id.ToCharArray(), pw.ToCharArray(), false);
                byte[] body = mc.StructureToByte(signupReqest);
                sm.Send(MessageType.Signup, body);

                Header h = (Header)sm.Recieve(out body);

                if (h.state == MessageState.SUCCESS)
                {
                    flag = false;
                }
                else
                {
                    Console.WriteLine("[!]Fail to Sign up");
                }
                Console.Clear();
            }
        }
    }
}
