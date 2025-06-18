using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using ELDLPlatform.Database;

namespace ELDLPlatform.Core
{
    internal static class Terminal
    {
        private static bool _consoleMainThread_ = true;
        private static readonly Dictionary<string, Action> actions = [];

        private static readonly string currentDirectoryPath = Directory.GetCurrentDirectory();

        // Config files
        private static readonly string configDirectoryPath = Directory.GetCurrentDirectory() + "\\Configs";
        private static readonly string webConfigPath = Directory.GetCurrentDirectory() + "\\WebConfig.txt";
        private static readonly string dbConfigPath = Directory.GetCurrentDirectory() + "\\DBConfig.txt";
        private static string webConfigFile = "";
        private static string dbConfigFile = "";

        // Main thread, can exist only one at a time
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
        internal static void TerminalConsole()
        {
            Init();
            SVTerminal.Init();
            DBTerminal.Init();
            string s;
            while (_consoleMainThread_) 
            {
                Console.WriteLine("\n\n");
                s = Console.ReadLine() + "";
                Console.WriteLine("\n\n");

                ParseInput(s);
            }

            Console.WriteLine("Enter any symbol to close console");
            Console.ReadLine();
        }

        private static void ParseInput(string input)
        {
            List<string> i = input.Split(' ').ToList<string>();

            if (i.Count < 2)
            {
                Console.WriteLine("Command must contain at least one prefix");
                return;
            }
            string prefix = i[0];
            i.RemoveAt(0);
            if (prefix == "CT")
            {
                if (actions.TryGetValue(input, out Action? value)) { value(); }
            }
            else if (prefix == "SV") { SVTerminal.Terminal(i); }
            else if (prefix == "DB") { DBTerminal.Terminal(i); }
        }

        private static void Init()
        {
            LoadConfigs();

            Console.WriteLine("Loading actions...");

            actions.Add("ShutDown", ShutDown);
            actions.Add("PrintCurrentDirectory", PrintCurrentDirectory);
            actions.Add("PrintConfigDirectory", PrintConfigDirectory);
            actions.Add("Help", Help);

            Console.WriteLine("Action loading is completed...");
        }

        private static void ShutDown()
        {
            _consoleMainThread_ = false;
        }

        private static void Help()
        {
            Console.WriteLine("\n\n");
            Console.WriteLine("__________________________________________________________________________");
            Console.WriteLine("Core Terminal");
            Console.WriteLine("__________________________________________________________________________");
            Console.WriteLine();
            Console.WriteLine("List of commands:");
            Console.WriteLine("Help - list of commands");
            Console.WriteLine("ShutDown - shut down server");
            Console.WriteLine("\n\n");
        }

        private static void LoadConfigs()
        {
            Console.WriteLine("Loading configs...");
            webConfigFile = LoadFile(webConfigPath);
            dbConfigFile = LoadFile(dbConfigPath);
            Console.WriteLine("Configs are loaded");
        }

        private static string LoadFile(string path)
        {
            try
            {
                if (!Path.Exists(path))
                {
                    Console.WriteLine("\n\n");
                    Console.WriteLine("Requested File doesn't exists!");
                    Console.WriteLine("Expected path: " + path);
                    Console.WriteLine("\n\n");
                    return "";
                }
                var s = File.ReadLines(path) + "";
                Console.WriteLine("Users updated");
                return s;
            }
            catch (Exception e) { PrintException(e); return ""; }
        }

        internal static string GetWebConfig() { return webConfigFile; }
        internal static string GetDBConfig() { return dbConfigFile; }

        internal static void PrintCurrentDirectory()
        {
            Console.WriteLine("\n\n");
            Console.WriteLine("__________________________________________________________________________");
            Console.WriteLine("Current directory:");
            Console.WriteLine(currentDirectoryPath);
            Console.WriteLine("__________________________________________________________________________");
            Console.WriteLine("\n\n");
        }

        internal static void PrintConfigDirectory()
        {
            Console.WriteLine("\n\n");
            Console.WriteLine("__________________________________________________________________________");
            Console.WriteLine("Config directory:");
            Console.WriteLine(configDirectoryPath);
            Console.WriteLine("__________________________________________________________________________");
            Console.WriteLine("\n\n");
        }

        public static void PrintException(Exception e) // Method to conveniently print exceptions
        {
            Console.WriteLine("\n________________________________________________");
            Console.WriteLine(e.HResult + ": " + e.Message);
            Console.WriteLine(e.StackTrace);
            Console.WriteLine("\n________________________________________________\n");
        }
    }
}
