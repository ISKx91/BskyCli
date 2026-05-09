using BskyCli.bsky.cli;
using System.Reflection;

namespace BskyCli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var appVer = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
            Console.WriteLine($"Welcome to BskyCli! (ver {appVer})" + Environment.NewLine);

            try
            {
                var cli = new Cli();
                cli.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
