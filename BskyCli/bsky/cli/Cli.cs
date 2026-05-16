using BskyCli.bsky.client;

namespace BskyCli.bsky.cli
{
    /// <summary>
    /// CLI (Command Line Interface) クラスは、ユーザーがコマンドを入力してBlueskyと対話できるようにするためのクラスです。
    /// </summary>
    internal partial class Cli
    {
        private Client? Client { get; set; }

        private string Prompt { get; set; }


        private const string NOT_LOGGED_IN_PROMPT = "Not logged in";

        /// <summary>
        /// コンストラクタは、CLIの初期状態を設定します。ユーザーがログインしていない状態では、プロンプトは "Not logged in" に設定されます。
        /// </summary>
        internal Cli()
        {
            this.Prompt = Cli.NOT_LOGGED_IN_PROMPT;
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
                        ConsoleCommon.ConsoleError("You are already logged in. Please logout first before logging in again.");
                        continue;
                    }
                    this.Login();
                }
                else if (command == "logout")
                {
                    if (this.Client == null)
                    {
                        ConsoleCommon.ConsoleError("You are not logged in.");
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
                        ConsoleCommon.ConsoleError("You are not logged in.");
                        continue;
                    }
                    this.Post();
                }
                else if (command == "cls")
                {
                    ConsoleCommon.ClearScreen();
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
            if (this.Prompt != Cli.NOT_LOGGED_IN_PROMPT)
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
            var password = ConsoleCommon.ReadPassword();

            var client = Client.Login(identifier!, password).Result;

            this.Client = client;
            this.Prompt = client.session.handle;
            ConsoleCommon.ConsoleSuccess("Login successful! You can now use the CLI to interact with Bluesky.");
        }

        /// <summary>
        /// 
        /// </summary>
        private void Logout()
        {
            this.Client!.Logout().Wait();
            this.Client = null;
            this.Prompt = Cli.NOT_LOGGED_IN_PROMPT;
            ConsoleCommon.ConsoleSuccess("Logged out successfully.");
        }

        private void Help()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  login  - Log in to Bluesky");
            Console.WriteLine("  logout - Log out of Bluesky");
            Console.WriteLine("  help   - Show this help message");
            Console.WriteLine("  post   - Send post");
            Console.WriteLine("  cls    - Clear Screen");
            Console.WriteLine("  exit   - Exit the CLI");
        }

        private void Post()
        {
            string content = ConsoleCommon.ConsoleTextEdit(string.Empty);
            if (string.IsNullOrWhiteSpace(content))
            {
                ConsoleCommon.ConsoleWarning("No content entered. The post has been canceled.");
                return;
            }
            ConsoleCommon.ConsoleInfo("============================================================");
            ConsoleCommon.ConsoleInfo("         Is it okay to post the following content ?         ");
            ConsoleCommon.ConsoleInfo("============================================================");
            ConsoleCommon.ConsoleInfo(content);
            ConsoleCommon.ConsoleInfo("------------------------------------------------------------");
            if (!ConsoleCommon.Confirm())
            {
                ConsoleCommon.ConsoleWarning("Post cancelled.");
                return;
            }
            var response = this.Client!.Post(content, DateTime.Now).Result;
            Console.WriteLine($"Post successful! HTTP Status Code: {response?.StatusCode}");
        }
    }
}
