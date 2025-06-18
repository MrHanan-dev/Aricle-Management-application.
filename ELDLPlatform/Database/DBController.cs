using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELDLPlatform.Database
{
    // Main controlling body, responsible for connecting/initialising/switching between databases
    internal class DBController
    {
        internal DBHandler? dBHandler;

        internal void SetDBConfig() { } // reserved
        internal void ConnectToDatabase() { }
        internal void DisconnectFromDatabase() { }
    }
}
