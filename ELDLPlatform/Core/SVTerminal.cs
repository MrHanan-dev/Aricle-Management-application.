using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELDLPlatform.Core
{
    internal static class SVTerminal
    {
        private static bool firstInit = true;
        private static bool debugInfo = true;

        private static List<string> terminalInput = [];

        internal static void Terminal(List<string> input)
        {
            terminalInput = input;
        }
        internal static void Init()
        {
            if (firstInit)
            {
                firstInit = false;
            }
        }
    }
}
