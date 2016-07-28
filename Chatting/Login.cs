using System;

namespace Chatting
{
	class Login
	{
		
		public User LogIn()
		{
            User user = new User();
			Console.WriteLine("+----------------------------------------------------------------+");
			Console.WriteLine("|                            Log In                              |");
			Console.WriteLine("+----------------------------------------------------------------+");
			Console.Write("ID   : ");
			user.id = Console.ReadLine();
			Console.Write("PW   : ");
			user.password = Console.ReadLine();
            return user;
		}
	}
}

