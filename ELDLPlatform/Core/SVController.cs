using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Nodes;
using System.Threading;

namespace ELDLPlatform.Core
{
    internal class SVController
    {
        private HttpListener? listener;
        private Task? listenerThread;
        private bool listenConnections = false;
        private readonly Dictionary<string, string[]> users = []; // key - username, data - [username, password]
        private readonly Dictionary<string, (string, DateTime)> cookies = []; // key - cookie, data - (username, data of creation of cookie)
        private readonly Dictionary<string, string> cookiesByUsers = []; // key - username, data - cookie
        private readonly string jsonResponseSuccess;
        private readonly string jsonResponseFailure;
        private readonly string currentDirectory = Directory.GetCurrentDirectory() + "\\userFiles";
        private readonly string prefixesFile = Directory.GetCurrentDirectory() + "\\prefixes.txt";
        private readonly string usersFile = Directory.GetCurrentDirectory() + "\\users.txt";

        public SVController()
        {
            jsonResponseSuccess = JsonString("result", "success");
            jsonResponseFailure = JsonString("result", "failure");
        }

        internal void LaunchServer()
        {
            UpdateUsers();
            UpdatePrefixes();
            Console.WriteLine(Directory.GetCurrentDirectory());
        }
        internal void UpdateUsers()
        {
            if (!Path.Exists(usersFile))
            {
                Console.WriteLine("File \"users.txt\" doesn't exists!");
                Console.WriteLine("Expected path: " + usersFile);
                return;
            }
            var s = File.ReadLines(usersFile);
            string[] user;
            lock (users)
            {
                users.Clear();
                cookies.Clear();
                cookiesByUsers.Clear();
                foreach (string c in s)
                {
                    user = c.Split('~');
                    users.Add(user[0], [user[0], user[1]]);
                }
            }
            Console.WriteLine("Users updated");
        }

        // Same as "allowed origins" in django - what ports it's allowed to listen
        internal void UpdatePrefixes()
        {
            if (!Path.Exists(prefixesFile))
            {
                Console.WriteLine("File \"prefixes.txt\" doesn't exists!");
                Console.WriteLine("Expected path: " + prefixesFile);
                return;
            }

            var prefixes = File.ReadLines(prefixesFile);

            if (listener != null) // Execute if server already running
            {
                Console.WriteLine("Warning: The HttpServer already launched \n");
                Console.WriteLine("Updating new prefixes...");


                lock (listener.Prefixes) // Blocks async access to listener.Prefixes until process is done
                {
                    listener.Prefixes.Clear();
                    foreach (string s in prefixes)
                    {

                        listener.Prefixes.Add(s);
                    }
                }

                Console.WriteLine("Success: Prefixes updated \n");
                return;
            }

            // Execute on first startup

            listener = new HttpListener();
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }
            listenConnections = true;
            Console.WriteLine("Success: Prefixes loaded \n");
            listenerThread = Task.Run(() => Listening());
        }

        private void Listening() // Process of listening and responding to connections
        {

            if (listener == null) { return; }

            listener.Start();
            //List<Task> listOfRequests = [];
            HttpListenerContext context;
            while (listenConnections)
            {
                try
                {
                    context = listener.GetContext(); // Waiting for request. BLOCKING thread
                    Task.Run(() => Respond(context)); // Responding to request. NOT blocking thread
                    // Is NOT equivalent of // Task.Run(() => listener.GetContext());
                }
                catch (Exception e)
                {
                    PrintException(e);
                }
            }
        }

        private string GenerateCookie(string userName)
        {
            string s;
            DateTime currentTime;

            if (cookiesByUsers.ContainsKey(userName)) // If there is already cookie of same user, delete old one
            {
                cookies.Remove(cookiesByUsers.GetValueOrDefault(userName)!);
                cookiesByUsers.Remove(userName);
            }
            s = BitConverter.ToString(RandomNumberGenerator.GetBytes(256)); // cryptographically strong random string

            currentTime = DateTime.Now;
            cookies.Add(s, (userName, currentTime));
            cookiesByUsers.Add(userName, s);

            return s;
        }

        // Generating JSONs to send them to user

        public static string JsonString(string key, string value) // { "key": "value" }
        {
            return "{\"" + key + "\": \"" + value + "\"}";
        }

        public static string JsonString((string, string)[] data) // { "key1": "value1", "key2": "value2", <...>}
        {
            Dictionary<string, string> d = [];
            foreach (var item in data)
            {
                d.Add(item.Item1, item.Item2);
            }
            return JsonSerializer.Serialize(d);
        }

        private void Respond(HttpListenerContext context) // Respond to request
        {
            try
            {
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                StreamReader streamReader = new(request.InputStream, request.ContentEncoding);
                Stream output;
                byte[] buffer;

                try
                {
                    string debugText = streamReader.ReadToEnd();
                    Encoding encoding = request.ContentEncoding;

                    if (!string.IsNullOrEmpty(debugText))
                    {
                        JsonDocument clientData = JsonDocument.Parse(debugText);
                        Console.WriteLine(debugText);
                        Dictionary<string, string> sd = clientData.RootElement.Deserialize<Dictionary<string, string>>()!;
                        buffer = Encoding.UTF8.GetBytes(ParseData(sd));
                        response.ContentEncoding = request.ContentEncoding;
                        response.ContentLength64 = buffer.Length;
                        output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close(); // "Close()" sends data to a client
                        return;
                    }
                }
                catch (Exception e)
                {
                    PrintException(e);
                }

                buffer = Encoding.UTF8.GetBytes("Oh hi");
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = buffer.Length;
                output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
                Console.WriteLine("No data has been recieved");
            }
            catch (Exception e)
            {
                PrintException(e);
            }
        }

        private string ParseData(Dictionary<string, string> parsedData) // Parse and respond
        {
            string s = "Oh hi";
            try
            {
                if (parsedData.TryGetValue("requestType", out string? requestType))
                {
                    if (requestType == "Test")
                    {
                        s = jsonResponseSuccess;
                    }
                    else if (requestType == "Login")
                    {
                        s = Login(parsedData);
                    }
                    else if (requestType == "CookieLogin")
                    {
                        if (CookieLogin(parsedData))
                        { s = jsonResponseSuccess; }
                        else { s = jsonResponseFailure; }
                    }
                    else if (requestType == "UploadFile")
                    {
                        if (CookieLogin(parsedData))
                        { s = DownloadFile(parsedData); }
                        else { s = jsonResponseFailure; }
                    }
                    else if (requestType == "GetListOfFiles")
                    {
                        if (CookieLogin(parsedData))
                        { s = GetListOfFiles(parsedData); }
                        else { s = jsonResponseFailure; }
                    }
                    else if (requestType == "DownloadFile")
                    {
                        if (CookieLogin(parsedData))
                        { s = UploadFile(parsedData); }
                        else { s = jsonResponseFailure; }
                    }
                    else if (requestType == "DeleteFile")
                    {
                        if (CookieLogin(parsedData))
                        { s = DeleteFile(parsedData); }
                        else { s = jsonResponseFailure; }
                    }
                    Console.WriteLine(s);
                    Console.WriteLine(DateTime.Now.ToString());
                    return s;
                }
                foreach (string value in parsedData!.Values)
                {
                    Console.WriteLine(value);
                }
            }
            catch (Exception e)
            {
                PrintException(e);
                s = jsonResponseFailure;
            }
            Console.WriteLine(s);
            return s;
        }

        private string Login(Dictionary<string, string> parsedData)
        {
            if (parsedData.ContainsKey("userNameData") && parsedData.ContainsKey("userPasswordData")
                && users.TryGetValue(parsedData.GetValueOrDefault("userNameData")!, out string[]? data))
            {
                if (data[1] == parsedData.GetValueOrDefault("userPasswordData")) // If the password is right
                {
                    return JsonString([("result", "success"), ("cookie", GenerateCookie(data[0]))]);
                    //return jsonResponseSuccess;
                }
            }
            return jsonResponseFailure;
        }

        private bool CookieLogin(Dictionary<string, string> parsedData) // Simple check if cookie is valid
        {
            if (parsedData.ContainsKey("clientCookie")
                && cookies.TryGetValue(parsedData.GetValueOrDefault("clientCookie")!, out (string, DateTime) data))
            {
                if (DateTime.Now.CompareTo(data.Item2.AddHours(2)) < 0) // If less than 2 hours passed
                {
                    return true;
                }
            }
            return false;
        }

        private string DownloadFile(Dictionary<string, string> parsedData)
        {
            if (parsedData.TryGetValue("clientCookie", out string? cookie) &&
                parsedData.TryGetValue("fileName", out string? fileName) &&
                parsedData.TryGetValue("fileData", out string? fileData))
            {
                string localPath = currentDirectory + '\\' + cookies[cookie].Item1;
                if (!Directory.Exists(localPath))
                {
                    Directory.CreateDirectory(localPath);
                }
                localPath = localPath + '\\' + fileName;
                File.WriteAllBytes(localPath, Convert.FromBase64String(fileData));
                Console.WriteLine(localPath);
                return jsonResponseSuccess;
            }
            return jsonResponseFailure;
        }

        private string GetListOfFiles(Dictionary<string, string> parsedData)
        {
            if (parsedData.TryGetValue("clientCookie", out string? cookie))
            {
                string localPath = currentDirectory + '\\' + cookies[cookie].Item1;
                if (!Directory.Exists(localPath) || !Directory.EnumerateFiles(localPath).Any())
                {
                    return jsonResponseFailure;
                }
                List<string> s = [];
                foreach (string fileName in Directory.EnumerateFiles(localPath))
                {
                    //s = s + '"' + Path.GetFileName(fileName) + "\",";
                    s.Add(Path.GetFileName(fileName));
                }
                Dictionary<string, string[]> d = [];
                d.Add("success", s.ToArray());
                return JsonSerializer.Serialize(d);
            }
            return jsonResponseFailure;
        }

        private string UploadFile(Dictionary<string, string> parsedData)
        {
            if (parsedData.TryGetValue("clientCookie", out string? cookie) &&
                parsedData.TryGetValue("fileName", out string? fileName))
            {
                string localPath = currentDirectory + '\\' + cookies[cookie].Item1 + '\\' + fileName;

                if (!Path.Exists(localPath)) { return jsonResponseFailure; }

                Console.WriteLine(localPath);
                return JsonString([("result", "success"), ("fileData", Convert.ToBase64String(File.ReadAllBytes(localPath)))]);
            }
            return jsonResponseFailure;
        }

        private string DeleteFile(Dictionary<string, string> parsedData)
        {
            if (parsedData.TryGetValue("clientCookie", out string? cookie) &&
                parsedData.TryGetValue("fileName", out string? fileName))
            {
                string localPath = currentDirectory + '\\' + cookies[cookie].Item1 + '\\' + fileName;

                if (!Path.Exists(localPath)) { return jsonResponseFailure; }
                File.Delete(localPath);
                Console.WriteLine(localPath);
                return jsonResponseSuccess;
            }
            return jsonResponseFailure;
        }

        public static void PrintException(Exception e) // Method to conveniently print exceptions
        {
            Console.WriteLine("\n________________________________________________");
            Console.WriteLine(e.HResult + ": " + e.Message);
            Console.WriteLine(e.StackTrace);
            Console.WriteLine("\n________________________________________________\n");
        }

        internal void AllowListenConnections() { listenConnections = true; }
        internal void DisableListenConnections()
        {
            listenConnections = false;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            listenerThread.WaitAsync(new CancellationToken(true));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            Console.WriteLine("Server shutted down");
        }
    }
}
