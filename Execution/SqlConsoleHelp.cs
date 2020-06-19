using System;
using SqlUtils.Commands;

namespace SqlUtils
{
    internal class SqlConsoleHelp
    {
        internal static void Show(string commandName)
        {
            if (!string.IsNullOrEmpty(commandName))
            {
                ShowHelpCommand.ExecuteDirect(commandName, true);
            }
            else
            {
                Console.WriteLine("");
                Console.WriteLine("The console supports 2 types of commands: (1) SQL, (2) Extended");
                Console.WriteLine("");
                Console.WriteLine("SQL Commands");
                Console.WriteLine("");
                Console.WriteLine("   SQL commands are sent directly to the server. Type the command(s)");
                Console.WriteLine("   you want to send. Once all commands are entered, type GO [ENTER]");
                Console.WriteLine("   on a line to send them to the server.");
                Console.WriteLine("");
                Console.WriteLine("Extended commands");
                Console.WriteLine("");
                Console.WriteLine("   Extended commands are interpreted by the console first.");
                Console.WriteLine("   All extended commands are preceded with a '!' character");
                Console.WriteLine("");
                Console.WriteLine("   " + Settings.ConsoleCommandPrefix + "attach <dbpath> [<dbname>]");
                Console.WriteLine("      Attaches a database file to the server using the name if specified.");
                Console.WriteLine("");
                Console.WriteLine("   " + Settings.ConsoleCommandPrefix + "create <dbpath>|" + Settings.DatabaseNamePrefix + "<dbname> [<dbname>]");
                Console.WriteLine("     Creates a new database given the database path or name.");
                Console.WriteLine("");
                Console.WriteLine("   " + Settings.ConsoleCommandPrefix + "detach <dbpath[*]>|" + Settings.DatabaseNamePrefix + "<dbname>");
                Console.WriteLine("      Detaches database(s) by path or name.");
                Console.WriteLine("");
                Console.WriteLine("   " + Settings.ConsoleCommandPrefix + "list");
                Console.WriteLine("      Lists all databases on the server.");
                Console.WriteLine("");
                Console.WriteLine("   " + Settings.ConsoleCommandPrefix + "timer");
                Console.WriteLine("      Turns the timer on/off. Will report the execution time of each command.");
                Console.WriteLine("");
                Console.WriteLine("   " + Settings.ConsoleCommandPrefix + "commandTimeout timeout_in_seconds");
                Console.WriteLine("      Sets the command timeout. 0 sets the timeout to unlimited.");
                Console.WriteLine("");
                Console.WriteLine("   " + Settings.ConsoleCommandPrefix + "run commandfile [var1=val1,...]");
                Console.WriteLine("      Run the given command file optionally passing in variable declarations.");
                Console.WriteLine("");
                Console.WriteLine("   " + Settings.ConsoleCommandPrefix + "consolewnd");
                Console.WriteLine("      Shows the interactive console window.");
                Console.WriteLine("");
                Console.WriteLine("   " + Settings.ConsoleCommandPrefix + "history show [count]");
                Console.WriteLine("      Shows all command history or the last 'count' commands.");
                Console.WriteLine("");
                Console.WriteLine("   " + Settings.ConsoleCommandPrefix + "history save filepath");
                Console.WriteLine("      Saves the command history to a file for later playback (see the 'run' command).");
                Console.WriteLine("");
                Console.WriteLine("   " + Settings.ConsoleCommandPrefix + "history clear");
                Console.WriteLine("      Clears the command history.");
                Console.WriteLine("");
                Console.WriteLine("   " + Settings.ConsoleCommandPrefix + "logopen filepath | " + Settings.ConsoleCommandPrefix + "logclose");
                Console.WriteLine("      Opens/closes log file. All input/output is written to the log.");
                Console.WriteLine("");
                Console.WriteLine("");
            }
        }
    }
}

