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

        private static readonly Dictionary<string, string> Commands = new()
        {
            ["login"]  = "Log in to Bluesky",
            ["logout"] = "Log out of Bluesky",
            ["help"]   = "Show this help message",
            ["post"]   = "Send post",
            ["cls"]    = "Clear screen",
            ["exit"]   = "Exit the CLI"
        };

        private const string NOT_LOGGED_IN_PROMPT = "Not logged in";

        /// <summary>
        /// コンストラクタは、CLIの初期状態を設定します。ユーザーがログインしていない状態では、プロンプトは "Not logged in" に設定されます。
        /// </summary>
        internal Cli()
        {
            this.Prompt = Cli.NOT_LOGGED_IN_PROMPT;
        }

        /// <summary>
        /// Runメソッドは、ユーザーからのコマンド入力を待ち受け、適切なアクションを実行するためのメインループを提供します。
        /// ユーザーが "exit" コマンドを入力するまで、このループは継続します。
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// WritePromptメソッドは、現在のプロンプトをコンソールに表示します。
        /// ユーザーがログインしている場合は、プロンプトが緑色で表示されます。
        /// </summary>
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
        /// Loginメソッドは、ユーザーにユーザー名またはメールアドレスとパスワードの入力を促し、Blueskyへのログインを試みます。
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
        /// Logoutメソッドは、現在のセッションを終了し、ユーザーをBlueskyからログアウトさせます。
        /// </summary>
        private void Logout()
        {
            this.Client!.Logout().Wait();
            this.Client = null;
            this.Prompt = Cli.NOT_LOGGED_IN_PROMPT;
            ConsoleCommon.ConsoleSuccess("Logged out successfully.");
        }

        /// <summary>
        /// Helpメソッドは、利用可能なコマンドのリストとその説明をコンソールに表示します。
        /// </summary>
        private void Help()
        {
            Console.WriteLine("Available commands:");
            foreach (var kv in Commands)
                Console.WriteLine($"  {kv.Key,-6} - {kv.Value}");
        }

        /// <summary>
        /// Postメソッドは、ユーザーに投稿内容の入力を促し、確認後にBlueskyに投稿を送信します。
        /// </summary>
        private void Post()
        {
            string content = ConsoleCommon.ConsoleTextEdit(string.Empty);
            
            if (string.IsNullOrWhiteSpace(content))
            {
                ConsoleCommon.ConsoleWarning("No content entered. The post has been canceled.");
                return;
            }
            
            ShowPostConfirmation(content);
            
            if (!ConsoleCommon.Confirm())
            {
                ConsoleCommon.ConsoleWarning("Post cancelled.");
                return;
            }

            var response = this.Client!.Post(content, DateTime.Now).Result;
            Console.WriteLine($"Post successful! HTTP Status Code: {response?.StatusCode}");
        }

        /// <summary>
        /// ShowPostConfirmationメソッドは、ユーザーが投稿内容を確認できるようにするためのメソッドです。
        /// </summary>
        /// <param name="content"></param>
        private void ShowPostConfirmation(string content)
        {
            int width = Console.WindowWidth;

            ConsoleCommon.PrintLine('=',width);

            string message = "Are you sure you want to post this?";
            ConsoleCommon.ConsoleInfo(ConsoleCommon.CenterText(message, width));

            ConsoleCommon.PrintLine('=', width);
            ConsoleCommon.ConsoleInfo(content);
            ConsoleCommon.PrintLine('-', width);
        }
    }
}
