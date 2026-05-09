using BskyCli.bsky.client;

namespace BskyCli.bsky.cli
{
    internal class Cli
    {
        private Client? Client { get; set; }

        private string Prompt { get; set; }


        private const string NotLoggedInPrompt = "Not logged in";


        internal Cli()
        {
            this.Prompt = Cli.NotLoggedInPrompt;
        }






        internal async Task<int> Run()
        {
            Console.WriteLine("Type 'help' for a list of commands, or 'exit' to quit.");
            while (true)
            {
                this.WritePrompt();
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) continue;
                var command = input.Trim().ToLower();
                if (command == "exit")
                {
                    if (this.Client != null)
                    {
                        this.Logout();
                    }
                    Console.WriteLine("Goodbye!");
                    break;
                }
                else if (command == "login")
                {
                    if (this.Client != null)
                    {
                        ConsoleError("You are already logged in. Please logout first before logging in again.");
                        continue;
                    }
                    this.Login();
                }
                else if (command == "logout")
                {
                    if (this.Client == null)
                    {
                        ConsoleError("You are not logged in.");
                        continue;
                    }
                    this.Logout();
                }
                else if (command == "help")
                {
                    this.Help();
                }
                else if (command == "post")
                {
                    if (this.Client == null)
                    {
                        ConsoleError("You are not logged in.");
                        continue;
                    }
                    this.Post();
                }
                else
                {
                    Console.WriteLine($"Unknown command: {command}");
                }
            }
            return 0;
        }


        private void WritePrompt()
        {
            if (this.Prompt != Cli.NotLoggedInPrompt)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            Console.Write(this.Prompt);
            Console.ResetColor();
            Console.Write($" > ");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void Login()
        {
            Console.Write("Enter username or e-mail address:");
            var identifier = Console.ReadLine();
            Console.Write("Enter password:");
            var password = ReadPassword();

            var client = Client.Login(identifier, password).Result;

            this.Client = client;
            this.Prompt = client.session.handle;
            ConsoleSuccess("Login successful! You can now use the CLI to interact with Bluesky.");
        }

        /// <summary>
        /// 
        /// </summary>
        private void Logout()
        {
            this.Client?.Logout().Wait();
            this.Client = null;
            this.Prompt = Cli.NotLoggedInPrompt;
            ConsoleSuccess("Logged out successfully.");
        }

        private void Help()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  login  - Log in to Bluesky");
            Console.WriteLine("  logout - Log out of Bluesky");
            Console.WriteLine("  help   - Show this help message");
            Console.WriteLine("  post   - Send post");
            Console.WriteLine("  exit   - Exit the CLI");
        }

        private void Post()
        {
            Console.Write("Enter your post content: ");
            var content = Console.ReadLine();
            var statusCode = this.Client.Post(content, DateTime.Now).Result;
            Console.WriteLine($"Post successful! HTTP Status Code: {statusCode}");
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        password = password.Substring(0, password.Length - 1);
                    }
                }
                info = Console.ReadKey(true);
            }
            Console.WriteLine();
            return password;
        }


        private static void ConsoleInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void ConsoleSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void ConsoleWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void ConsoleError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
