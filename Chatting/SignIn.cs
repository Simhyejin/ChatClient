using System;
using System.Net.Sockets;
using System.Text;


namespace Chatting
{
	class SignIn
	{
        User NewUser = new User();
        public bool Sign(Client client)
		{

            Console.WriteLine("+----------------------------------------------------------------+");
			Console.WriteLine("|                            Sign In                             |");
			Console.WriteLine("+----------------------------------------------------------------+");

            Console.Write("ID       : ");
            NewUser.id = Console.ReadLine();
            client.Send(MessageType.REQEUST_CHECK_SIGNEDUP, NewUser.id);

			Console.Write("Password     : ");
			string pw = Console.ReadLine();
			Console.Write("Passeord again   : ");
			string pw2 = Console.ReadLine();

			if (pw != pw2)
			{
				Console.WriteLine("[!] Passwords must match");
                return false;
			}
            return true;

		}
    }
}

