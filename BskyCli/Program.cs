using BskyCli.bsky.cli;
using System.Reflection;
using static BskyCli.bsky.cli.Cli;

namespace BskyCli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var appVer = Assembly.GetExecutingAssembly().GetName().Version!.ToString();

            string title = $@"
 _    _        _                                   _             ______       _            _____  _  _  _
| |  | |      | |                                 | |            | ___ \     | |          /  __ \| |(_)| |
| |  | |  ___ | |  ___   ___   _ __ ___    ___    | |_   ___     | |_/ / ___ | | __ _   _ | /  \/| | _ | |
| |/\| | / _ \| | / __| / _ \ | '_ ` _ \  / _ \   | __| / _ \    | ___ \/ __|| |/ /| | | || |    | || || |
\  /\  /|  __/| || (__ | (_) || | | | | ||  __/   | |_ | (_) |   | |_/ /\__ \|   < | |_| || \__/\| || ||_|
 \/  \/  \___||_| \___| \___/ |_| |_| |_| \___|    \__| \___/    \____/ |___/|_|\_\ \__, | \____/|_||_|(_)
                                                                                     __/ |
    Created By : ISKx91                                                             |___/
    Version    : {appVer}
";
            Cli.ConsoleCommon.ConsoleInfo(title);

            try
            {
                var cli = new Cli();
                cli.Run().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
