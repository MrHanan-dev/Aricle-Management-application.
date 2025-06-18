using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ELDLPlatform.Database
{
    internal static class DBTerminal
    {
        private static bool firstInit = true;
        private static readonly Dictionary<string, Action> actions = [];

        private static List<string> terminalInput = [];

        private static readonly List <DBController> terminalControllers = [];

        internal static void Terminal(List<string> input)
        {
            terminalInput = input;
            if (actions.TryGetValue(terminalInput[0], out Action? value)) { value(); }
        }

        internal static void Init()
        {
            if (firstInit)
            {
                firstInit = false;
                Console.WriteLine("Loading actions...");

                actions.Add("ShutDown", ShutDown);
                actions.Add("Help", Help);

                Console.WriteLine("Action loading is completed...");
            }
        }

        internal static void ShutDown()
        {
            Console.WriteLine(terminalInput.Count);
        }

        private static void Help()
        {
            Console.WriteLine("\n\n");
            Console.WriteLine("__________________________________________________________________________");
            Console.WriteLine("DB Terminal");
            Console.WriteLine("__________________________________________________________________________");
            Console.WriteLine();
            Console.WriteLine("List of commands:");
            Console.WriteLine("Help - list of commands");
            Console.WriteLine("Launch - launch server");
            Console.WriteLine("Disconnect - update users");
            Console.WriteLine("ShutDown - shut down database");
            Console.WriteLine("\n\n");
        }

        internal static DBController GetController()
        {
            return terminalControllers[0];
        }
    }
}
