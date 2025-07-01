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
    internal class Server
    {
        private HttpListener? listener;
        private Task? listenerThread;
        private bool listenConnections = true;
        private readonly Dictionary<string, (string, DateTime)> cookies = []; // key - cookie, data - (login, data of creation of cookie)
        private readonly Dictionary<string, string> cookiesByUsers = []; // key - login, data - cookie
        private readonly Dictionary<string, string[]> users = []; // key - login, data - [login, username, password, role]
        private readonly string jsonResponseSuccess;
        private readonly string jsonResponseFailure;
        private readonly string usersFile = Directory.GetCurrentDirectory() + "\\Configs\\TestUsers.txt";

        private readonly Dictionary<string, string> apiAddresses = [];

        private string[] config = [];

        // --- Article Management ---
        private List<Dictionary<string, string>> articles = new List<Dictionary<string, string>>
        {
            new Dictionary<string, string> {
                {"ID", "1"}, {"category", "Art"}, {"type", "Painting"}, {"name", "Starry Night"},
                {"born", "None"}, {"died", "None"}, {"nationality", "Dutch"}, {"knownFor", "Post-Impressionism"},
                {"notableWork", "Starry Night"}, {"about", "A famous painting by Vincent van Gogh."},
                {"year", "1889"}, {"medium", "Oil on canvas"}, {"dimensions", "73.7 cm × 92.1 cm"},
                {"location", "Museum of Modern Art, New York"}, {"designedByDeveloper", "None"}
            },
            new Dictionary<string, string> {
                {"ID", "2"}, {"category", "Mathematics"}, {"type", "Biography"}, {"name", "Ada Lovelace"},
                {"born", "1815"}, {"died", "1852"}, {"nationality", "British"}, {"knownFor", "First computer programmer"},
                {"notableWork", "Notes on the Analytical Engine"}, {"about", "Mathematician and writer, known for her work on Charles Babbage's Analytical Engine."},
                {"year", "None"}, {"medium", "None"}, {"dimensions", "None"},
                {"location", "None"}, {"designedByDeveloper", "None"}
            },
            new Dictionary<string, string> {
                {"ID", "3"}, {"category", "Technology"}, {"type", "Invention"}, {"name", "Internet"},
                {"born", "None"}, {"died", "None"}, {"nationality", "Global"}, {"knownFor", "Networking"},
                {"notableWork", "World Wide Web"}, {"about", "A global system of interconnected computer networks."},
                {"year", "1960s"}, {"medium", "None"}, {"dimensions", "None"},
                {"location", "Global"}, {"designedByDeveloper", "Many"}
            }
        };

        public Server()
        {
            jsonResponseSuccess = JsonString("result", "success");
            jsonResponseFailure = JsonString("result", "failure");
        }

        internal void Init()
        {
            Console.WriteLine("Loading");
            UpdateUsers();
            UpdateConfig();
        }

        internal void UpdateConfig()
        {
            config = Terminal.GetWebConfig();
            Console.WriteLine(config.Length);
            string[] splitS;
            foreach (string line in config)
            {
                splitS = line.Split("=");
                Console.WriteLine(splitS[0]);
                if (splitS[0] == "AllowedPrefixes")
                {
                    UpdatePrefixes(splitS[1].Split("^"));
                }
                else
                {
                    Console.WriteLine("Command not found");
                }
            }
            Console.WriteLine("Config updated");
        }

        internal void UpdateUsers()
        {
            if (!Path.Exists(usersFile))
            {
                Console.WriteLine("File \"TestUsers.txt\" doesn't exists!");
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
                    users.Add(user[0], [user[0], user[1], user[2], user[3]]);
                }
            }
            Console.WriteLine("Users updated");
        }

        // Same as "allowed origins" in django - what ports it's allowed to listen
        internal void UpdatePrefixes(string[] prefixes)
        {
            if (listener != null) // Execute if server already running
            {
                Console.WriteLine("Warning: The HttpServer already launched \n");
                Console.WriteLine("Updating new prefixes...");


                lock (listener.Prefixes) // Blocks async access to listener.Prefixes until process is done
                {
                    listener.Prefixes.Clear();
                    apiAddresses.Clear();
                    foreach (string s in prefixes)
                    {
                        listener.Prefixes.Add(s);
                        listener.Prefixes.Add(s + @"api/");
                        apiAddresses.Add(s + @"api/", "");
                    }
                }

                Console.WriteLine("Success: Prefixes updated \n");
                return;
            }

            // Execute on first startup

            listener = new HttpListener();
            foreach (string s in prefixes)
            {
                Console.WriteLine(s);
                listener.Prefixes.Add(s);
                Console.WriteLine(s + @"api/");
                listener.Prefixes.Add(s + @"api/");

                apiAddresses.Add(s + @"api/", "");
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
                    Terminal.PrintException(e);
                }
            }
        }

        private string GenerateCookie(string userLogin)
        {
            string s;
            DateTime currentTime;

            if (cookiesByUsers.ContainsKey(userLogin)) // If there is already cookie of same user, delete old one
            {
                cookies.Remove(cookiesByUsers.GetValueOrDefault(userLogin)!);
                cookiesByUsers.Remove(userLogin);
            }
            s = BitConverter.ToString(RandomNumberGenerator.GetBytes(256)); // cryptographically strong random string

            currentTime = DateTime.Now;
            cookies.Add(s, (userLogin, currentTime));
            cookiesByUsers.Add(userLogin, s);

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
                Console.WriteLine(request.UserHostAddress);
                try
                {
                    string debugText = streamReader.ReadToEnd();
                    Encoding encoding = request.ContentEncoding;

                    if (!string.IsNullOrEmpty(debugText))
                    {
                        JsonDocument clientData = JsonDocument.Parse(debugText);
                        Console.WriteLine(debugText);
                        //Dictionary<string, string> sd = clientData.RootElement.Deserialize<Dictionary<string, string>>()!;
                        // buffer = Encoding.UTF8.GetBytes(ParseData(sd));
                        buffer = Encoding.UTF8.GetBytes(ParseData(clientData.RootElement.Deserialize<Dictionary<string, string>>()!));
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
                    Terminal.PrintException(e);
                }

                buffer = SVTerminal.WebPageBytes;
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = buffer.Length;
                output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
                Console.WriteLine("No data has been recieved");
            }
            catch (Exception e)
            {
                Terminal.PrintException(e);
            }
        }

        private string ParseData(Dictionary<string, string> parsedData) // Parse and respond
        {
            string s = "Oh hi";
            try
            {
                if (parsedData.TryGetValue("requestType", out string? requestType))
                {
                    if (requestType == "test")
                    {
                        s = jsonResponseSuccess;
                    }
                    else if (requestType == "cookieTest")
                    {
                        if (CookieTest(parsedData))
                        { s = jsonResponseSuccess; }
                        else { s = jsonResponseFailure; }
                    }
                    else if (requestType == "login")
                    {
                        s = Login(parsedData);
                    }
                    else if (requestType == "getUser")
                    {
                        s = GetUser(parsedData);
                    }
                    else if (requestType == "editUser")
                    {
                        s = EditUser(parsedData);
                    }
                    else if (requestType == "addUser")
                    {
                        s = AddUser(parsedData);
                    }
                    else if (requestType == "deleteUser")
                    {
                        s = DeleteUser(parsedData);
                    }
                    else if (requestType == "getRoles")
                    {
                        s = GetRoles(parsedData);
                    }
                    else if (requestType == "getCategories")
                    {
                        s = GetCategories(parsedData);
                    }
                    else if (requestType == "searchUsers")
                    {
                        s = SearchUsers(parsedData);
                    }
                    else if (requestType == "searchArticles")
                    {
                        s = SearchArticles(parsedData);
                    }
                    else if (requestType == "getArticle")
                    {
                        s = GetArticle(parsedData);
                    }
                    else if (requestType == "addArticle")
                    {
                        s = AddArticle(parsedData);
                    }
                    else if (requestType == "editArticle")
                    {
                        s = EditArticle(parsedData);
                    }
                    else if (requestType == "deleteArticle")
                    {
                        s = DeleteArticle(parsedData);
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
                Terminal.PrintException(e);
                s = jsonResponseFailure;
            }
            Console.WriteLine(s);
            return s;
        }

        private bool GetUserRole(string cookie, out string role)
        {
            role = "None";

            if (cookiesByUsers.TryGetValue(cookie, out string? login))
            {
                if (users.TryGetValue(login, out string[]? user))
                {
                    role = user[3];
                    return true;
                }
            }

            return false;
        }

        private string Login(Dictionary<string, string> parsedData)
        {
            if (parsedData.ContainsKey("login") && parsedData.ContainsKey("password")
                && users.TryGetValue(parsedData.GetValueOrDefault("login")!, out string[]? data))
            {
                if (data[2] == parsedData.GetValueOrDefault("password")) // If the password is right
                {
                    return JsonString([("result", "success"), ("cookie", GenerateCookie(data[0]))]);
                    //return jsonResponseSuccess;
                }
            }
            return jsonResponseFailure;
        }

        private string GetUser(Dictionary<string, string> parsedData)
        {
            if (CookieTest(parsedData)
    && users.TryGetValue(parsedData.GetValueOrDefault("cookie")!, out string[]? data))
            {
                if (data[3] == "Admin" || data[3] == "Superadmin") // If the permissions allow it
                {
                    return JsonString([("result", "success"), ("login", data[0]), ("username", data[1]), ("role", data[3])]);
                    //return jsonResponseSuccess;
                }
            }
            return jsonResponseFailure;
        }

        private string EditUser(Dictionary<string, string> parsedData)
        {
            Console.WriteLine("EditUser called");
            Console.WriteLine("Bypassing CookieTest and GetUserRole checks for debugging");
            string editorRole = "Superadmin";
            if (parsedData.TryGetValue("userLogin", out string? userLogin)
                && users.TryGetValue(userLogin, out string[]? editiedUser)
                && parsedData.TryGetValue("data", out string? data))
            {
                try
                {
                    Dictionary<string, string> newEditiedUser = JsonDocument.Parse(data).RootElement.Deserialize<Dictionary<string, string>>()!;
                    if (editiedUser[3] == "Admin" && editorRole == "Superadmin" 
                        || editiedUser[3] == "Superadmin" && editorRole == "Superadmin"
                        || (editiedUser[3] == "Tutor" || editiedUser[3] == "Student") && (editorRole == "Superadmin" || editorRole == "Superadmin")) // If the permissions allow it
                    {
                        if (newEditiedUser.TryGetValue("role", out string? newRole))
                        {
                            if ((newRole == "Superadmin" || newRole == "Admin"
                                || editiedUser[3] == "Superadmin" && newRole == "Admin")
                                && editorRole == "Admin")
                            {
                                return jsonResponseFailure;
                            }
                        }
                        else
                        {
                            newRole = editiedUser[3];
                        }
                        if (!newEditiedUser.TryGetValue("username", out string? username)) 
                        {
                            username = editiedUser[1];
                        }
                        if (!newEditiedUser.TryGetValue("password", out string? password))
                        {
                            password = editiedUser[2];
                        }
                        users[userLogin] = [editiedUser[0], username, password, newRole];
                        return jsonResponseSuccess;
                    }
                }
                catch (Exception e)
                {
                    Terminal.PrintException(e);
                }
            }
            return jsonResponseFailure;
        }

        private string AddUser(Dictionary<string, string> parsedData)
        {
            Console.WriteLine("AddUser called");
            // TEMP: Disable CookieTest and GetUserRole for debugging
            Console.WriteLine("Bypassing CookieTest and GetUserRole checks for debugging");
            string editorRole = "Superadmin";
            if (parsedData.TryGetValue("data", out string? data))
            {
                try
                {
                    Console.WriteLine($"editorRole: {editorRole}");
                    Console.WriteLine($"data: {data}");
                    Dictionary<string, string> newEditiedUser = JsonDocument.Parse(data).RootElement.Deserialize<Dictionary<string, string>>()!;
                    foreach (var kv in newEditiedUser) Console.WriteLine($"newUser {kv.Key}: {kv.Value}");
                    if (editorRole == "Superadmin" || editorRole == "Superadmin") // If the permissions allow it
                    {
                        if (newEditiedUser.TryGetValue("role", out string? newRole))
                        {
                            if (newRole == "Superadmin" || newRole == "Admin"
                                && editorRole == "Admin")
                            {
                                Console.WriteLine("Failure: Admin cannot add Superadmin/Admin");
                                return jsonResponseFailure;
                            }
                        }
                        else { Console.WriteLine("Failure: No role in new user"); return jsonResponseFailure; }
                        if (!newEditiedUser.TryGetValue("login", out string? login) || !newEditiedUser.TryGetValue("username", out string? username) || !newEditiedUser.TryGetValue("password", out string? password))
                        {
                            Console.WriteLine("Failure: Missing login/username/password");
                            return jsonResponseFailure;
                        }
                        Console.WriteLine($"Trying to add user: {login}, {username}, {password}, {newEditiedUser["role"]}");
                        if (!users.TryAdd(login, [login, username, password, newEditiedUser["role"]]))
                        {
                            Console.WriteLine($"Failure: users.TryAdd failed for {login}");
                            return jsonResponseFailure;
                        }
                        Console.WriteLine("Success: User added");
                        return jsonResponseSuccess;
                    }
                    else
                    {
                        Console.WriteLine($"Failure: editorRole is not Superadmin: {editorRole}");
                    }
                }
                catch (Exception e)
                {
                    Terminal.PrintException(e);
                }
            }
            else
            {
                Console.WriteLine("Failure: parsedData.TryGetValue('data') failed");
            }
            return jsonResponseFailure;
        }

        private string DeleteUser(Dictionary<string, string> parsedData)
        {
            Console.WriteLine("DeleteUser called");
            Console.WriteLine("Bypassing CookieTest and GetUserRole checks for debugging");
            string editorRole = "Superadmin";
            if (parsedData.TryGetValue("userLogin", out string? userLogin)
                && users.TryGetValue(userLogin, out string[]? editiedUser))
            {
                try
                {
                    if (editiedUser[3] == "Admin" && editorRole == "Superadmin"
                        || editiedUser[3] == "Superadmin" && editorRole == "Superadmin"
                        || (editiedUser[3] == "Tutor" || editiedUser[3] == "Student") && (editorRole == "Superadmin" || editorRole == "Superadmin")) // If the permissions allow it
                    {
                        users.Remove(editiedUser[0]);
                        return jsonResponseSuccess;
                    }
                }
                catch (Exception e)
                {
                    Terminal.PrintException(e);
                }
            }
            return jsonResponseFailure;
        }

        private string GetRoles(Dictionary<string, string> parsedData)
        {
            if (CookieTest(parsedData))
            {
                return JsonString([("result", "success"), ("data", "[\"Student\", \"Tutor\", \"Admin\", \"Superadmin\"]")]);
            }
            return jsonResponseFailure;
        }

        private string GetCategories(Dictionary<string, string> parsedData)
        {
            if (CookieTest(parsedData))
            {
                return JsonString([("result", "success"), ("data", "[\"Art\", \"Mathematics\", \"Technology\"]")]);
            }
            return jsonResponseFailure;
        }

        private bool CookieTest(Dictionary<string, string> parsedData) // Simple check if cookie is valid
        {
            if (parsedData.ContainsKey("cookie")
                && cookies.TryGetValue(parsedData.GetValueOrDefault("cookie")!, out (string, DateTime) data))
            {
                if (DateTime.Now.CompareTo(data.Item2.AddHours(2)) < 0) // If less than 2 hours passed
                {
                    return true;
                }
            }
            return false;
        }

        private string SearchUsers(Dictionary<string, string> parsedData)
        {
            if (!CookieTest(parsedData))
                return jsonResponseFailure;
            string searchString = "";
            string[] roles = null;
            if (parsedData.TryGetValue("searchString", out string? ss))
                searchString = ss.ToLower();
            if (parsedData.TryGetValue("roles", out string? rolesStr))
            {
                try { roles = System.Text.Json.JsonSerializer.Deserialize<string[]>(rolesStr); } catch { roles = null; }
            }
            var result = new List<Dictionary<string, string>>();
            lock (users)
            {
                foreach (var kv in users)
                {
                    var login = kv.Value[0];
                    var username = kv.Value[1];
                    var role = kv.Value[3];
                    if (!string.IsNullOrEmpty(searchString) && !login.ToLower().Contains(searchString) && !username.ToLower().Contains(searchString))
                        continue;
                    if (roles != null && roles.Length > 0 && Array.IndexOf(roles, role) == -1)
                        continue;
                    result.Add(new Dictionary<string, string> {
                        { "login", login },
                        { "username", username },
                        { "role", role }
                    });
                }
            }
            return JsonString(new (string, string)[] {
                ("result", "success"),
                ("operationResultMessage", ""),
                ("data", System.Text.Json.JsonSerializer.Serialize(result))
            });
        }

        private string SearchArticles(Dictionary<string, string> parsedData)
        {
            string searchString = "";
            string[] categories = null;
            if (parsedData.TryGetValue("searchString", out string? ss))
                searchString = ss.ToLower();
            if (parsedData.TryGetValue("categories", out string? catStr))
            {
                try { categories = System.Text.Json.JsonSerializer.Deserialize<string[]>(catStr); } catch { categories = null; }
            }
            var result = new List<Dictionary<string, string>>();
            foreach (var art in articles)
            {
                if (!string.IsNullOrEmpty(searchString) && !art["name"].ToLower().Contains(searchString) && !art["category"].ToLower().Contains(searchString))
                    continue;
                if (categories != null && categories.Length > 0 && Array.IndexOf(categories, art["category"]) == -1)
                    continue;
                result.Add(art);
            }
            return JsonString(new (string, string)[] {
                ("result", "success"),
                ("operationResultMessage", ""),
                ("data", System.Text.Json.JsonSerializer.Serialize(result))
            });
        }

        private string GetArticle(Dictionary<string, string> parsedData)
        {
            if (!parsedData.TryGetValue("articleID", out string? id))
                return jsonResponseFailure;
            var art = articles.FirstOrDefault(a => a["ID"] == id);
            if (art == null)
                return jsonResponseFailure;
            return JsonString(new (string, string)[] {
                ("result", "success"),
                ("operationResultMessage", ""),
                ("data", System.Text.Json.JsonSerializer.Serialize(art))
            });
        }

        private string AddArticle(Dictionary<string, string> parsedData)
        {
            // Only allow admin/superadmin (bypass cookie for now)
            if (!parsedData.TryGetValue("data", out string? data))
                return jsonResponseFailure;
            try
            {
                var newArt = JsonDocument.Parse(data).RootElement.Deserialize<Dictionary<string, string>>()!;
                // Assign a new unique ID
                newArt["ID"] = (articles.Count > 0 ? (articles.Max(a => int.Parse(a["ID"])) + 1).ToString() : "1");
                articles.Add(newArt);
                return jsonResponseSuccess;
            }
            catch { return jsonResponseFailure; }
        }

        private string EditArticle(Dictionary<string, string> parsedData)
        {
            // Only allow admin/superadmin (bypass cookie for now)
            if (!parsedData.TryGetValue("articleID", out string? id) || !parsedData.TryGetValue("data", out string? data))
                return jsonResponseFailure;
            var art = articles.FirstOrDefault(a => a["ID"] == id);
            if (art == null) return jsonResponseFailure;
            try
            {
                var newData = JsonDocument.Parse(data).RootElement.Deserialize<Dictionary<string, string>>()!;
                foreach (var kv in newData)
                    art[kv.Key] = kv.Value;
                return jsonResponseSuccess;
            }
            catch { return jsonResponseFailure; }
        }

        private string DeleteArticle(Dictionary<string, string> parsedData)
        {
            // Only allow admin/superadmin (bypass cookie for now)
            if (!parsedData.TryGetValue("article", out string? id))
                return jsonResponseFailure;
            var art = articles.FirstOrDefault(a => a["ID"] == id);
            if (art == null) return jsonResponseFailure;
            articles.Remove(art);
            return jsonResponseSuccess;
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
    class DownloadList(string result, string[] fileNames)
    {
        public string result = result;
        public string[] fileNames = fileNames;
    }
}
