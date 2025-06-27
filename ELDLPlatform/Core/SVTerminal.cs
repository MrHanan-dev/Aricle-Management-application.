using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ELDLPlatform.Core
{
    internal static class SVTerminal
    {
        private static bool firstInit = false;
        private static bool debugInfo = true;
        private static string webPageText = "";
        private static string webPagePath = "";
        internal static byte[] WebPageBytes { private set; get; } = [];

        private static List<string> terminalInput = [];
        private static readonly Server server = new();

        internal static void TerminalConsole(List<string> input)
        {
            terminalInput = input;
            if (terminalInput[0] == "Init")
            {
                Init();
            }
            if (terminalInput[0] == "UpdatePage")
            {
                UpdatePage();
            }
            else
            {
                Console.WriteLine("Command not found");
            }
        }
        internal static void Init()
        {
            Console.WriteLine("Init");
            if (firstInit)
            {
                Console.WriteLine("Server is already running, use \"SV Update\" instead");
                return;
            }
            firstInit = false;
            UpdatePage();
            server.Init();
        }

        internal static void UpdatePage()
        {
            webPagePath = Terminal.GetCurrentDirectory() + @"\WebSide\index.html";
            Console.WriteLine("Loading page: " + webPagePath);
            if (File.Exists(webPagePath))
            {
                try
                {
                    webPageText = File.ReadAllText(webPagePath);
                    WebPageBytes = Encoding.UTF8.GetBytes(webPageText);
                    Console.WriteLine("Web page us updated");
                }
                catch (Exception e)
                {
                    Terminal.PrintException(e);
                }
            }
            else
            {
                Console.WriteLine("WARNING: Page doesn't exist!");
            }
        }

        internal static void SetDebugInfo(bool debug) { debugInfo = debug; }
    }
}
