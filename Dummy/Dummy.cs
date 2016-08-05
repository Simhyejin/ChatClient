using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Dummy
{
    struct Dummy
    {
        public string id;
        public string password;

        public Dummy(string id, string password)
        {
            this.id = id;
            this.password = password;
        }
    }
}
