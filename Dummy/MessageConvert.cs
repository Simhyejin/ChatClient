using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Dummy
{
    class MessageConvert
    {
        public object ByteToStructure(byte[] data, Type type)
        {
            IntPtr buff = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, buff, data.Length);
            object obj = Marshal.PtrToStructure(buff, type);
            Marshal.FreeHGlobal(buff);

            if (Marshal.SizeOf(obj) != data.Length)
            {
                return null;
            }

            return obj;
        }


        public byte[] StructureToByte(object obj)
        {
            int datasize = Marshal.SizeOf(obj);
            IntPtr buff = Marshal.AllocHGlobal(datasize);
            Marshal.StructureToPtr(obj, buff, false);
            byte[] data = new byte[datasize];
            Marshal.Copy(buff, data, 0, datasize);
            Marshal.FreeHGlobal(buff);
            return data;
        }

        public List<int> BytesToList(byte[] body)
        {
            List<int> list = new List<int>();
            for (int idx = 0; idx <(body.Length / 4); idx++)
            {
                byte[] tmpArr = new byte[4];
                Array.Copy(body, idx * 4, tmpArr, 0, 4); // tmpArr에 byte4개 들어가있음.

                int tmp = BitConverter.ToInt32(tmpArr, 0);
                list.Add(tmp);
            }
            return list;
        }

        public string ByteToString(byte[] buff)
        {
            return Encoding.UTF8.GetString(buff);
        }

        public byte[] StringToByte(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        //Retrun Random IP in Server List 
        public IPAddress GetServerIP(out int port)
        {
            Random r = new Random();
            int max = ConfigurationManager.AppSettings.Keys.Count;
            int rand = r.Next(0, max);

            string[] randomIP = ConfigurationManager.AppSettings.Keys[rand].Split(',');
            port = int.Parse(randomIP[1]);
            return IPAddress.Parse(randomIP[0]);
        }

        public string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        password = password.Substring(0, password.Length - 1);
                        int pos = Console.CursorLeft;
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        Console.Write(" ");
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }
            Console.WriteLine();
            return password;
        }

        public KeyType TryReadLine(out string result)
        {
            var buf = new StringBuilder();
            for (;;)
            {
                //exit
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                {
                    result = "";
                    return KeyType.Exit;
                }
                //go to back
                else if(key.Key == ConsoleKey.F1)
                {
                    result = "";
                    return KeyType.GoBack;
                }
                else if (key.Key == ConsoleKey.F2)
                {
                    result = "";
                    return KeyType.LogOut;
                }
                else if (key.Key == ConsoleKey.F3)
                {
                    result = "";
                    return KeyType.Delete;
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    result = buf.ToString();
                    return KeyType.Success;
                }
                else if (key.Key == ConsoleKey.Backspace && buf.Length > 0)
                {
                    buf.Remove(buf.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (key.KeyChar != 0)
                {
                    buf.Append(key.KeyChar);
                    Console.Write(key.KeyChar);
                }
            }
        }  
    }
    public enum KeyType
    {
        Success,
        GoBack,
        LogOut,
        Exit,
        Delete

    };
}
