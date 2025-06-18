using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELDLPlatform.Database
{
    internal class User(string login, string username, string password, int role) : DBData
    {
        internal string login = login;
        internal string username = username;
        internal string password = password;
        internal int role = role;

        internal override string SerializeAsJSON()
        {
            string s = "";
            return s;
        }
    }
}
