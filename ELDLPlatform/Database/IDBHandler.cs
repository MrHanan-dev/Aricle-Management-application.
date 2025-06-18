using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELDLPlatform.Database
{
    // Interface for interacting with DBHandler
    internal interface IDBHandler
    {
        internal string GetPassword(int username);
    }
}
