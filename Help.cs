using System;
using System.Reflection;

namespace SqlUtils
{
    internal class Help
    {
        internal static void Show()
        {
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            string str = ProgramInfo.InSqlMode ? "SQL Server Utility" : "SQL Server Express Utility";
            Console.WriteLine("Usage: " + str + " <command> [<args>] [<options>]");
            Console.WriteLine("");
            Console.WriteLine("Commands");
            Console.WriteLine("");
            Console.WriteLine("  -a[ttach] <dbpath> [<dbname>]");
            Console.WriteLine("     Attach a database file to the server using the name if specified.");
            Console.WriteLine("");
            Console.WriteLine("  -create <dbpath>|" + Settings.DatabaseNamePrefix + "<dbname> [<dbname>]");
            Console.WriteLine("     Create a new database given the database path or name.");
            Console.WriteLine("");
            Console.WriteLine("  -l[ist]");
            Console.WriteLine("     Lists all databases on the server.");
            Console.WriteLine("");
            Console.WriteLine("  -d[etach] <dbpath[*]>|" + Settings.DatabaseNamePrefix + "<dbname>");
            Console.WriteLine("     Detaches database(s) by path or name.");
            Console.WriteLine("");
            Console.WriteLine("  -u[pgrade] <dbpath>|<directory>");
            Console.WriteLine("     Upgrades an individual database file or all database files in a folder.");
            Console.WriteLine("");
            Console.WriteLine("  -t[race] +|-[<number>]");
            Console.WriteLine("     Enables or disables the specified trace number for all client connections.");
            Console.WriteLine("     Exclude number to trace SQL commands. Output written to the server log.");
            Console.WriteLine("");
            Console.WriteLine("  -childlist");
            Console.WriteLine("     Lists the child (user) instances of SQL Server.");
            Console.WriteLine("");
            Console.WriteLine("  -c[onsole]");
            Console.WriteLine("     Console mode. Allows user to type SQL statements to run on the server.");
            Console.WriteLine("");
            Console.WriteLine("  -consolewnd");
            Console.WriteLine("      Launches the interactive console window.");
            Console.WriteLine("");
            Console.WriteLine("  -run <filepath> [<var1>=<val1>[,...]]");
            Console.WriteLine("     Runs a command file (SQL or extended commands) with the variables provided.");
            Console.WriteLine("");
            Console.WriteLine("  -version");
            Console.WriteLine("     Displays the version reported by SQL Server.");
            Console.WriteLine("");
            Console.WriteLine("  -listsrv [remote]");
            Console.WriteLine("     Lists the local or remote instances of SQL Server.");
            Console.WriteLine("");
            Console.WriteLine("  -shrink <dbpath>|" + Settings.DatabaseNamePrefix + "<dbname>");
            Console.WriteLine("     Shrinks the given database and runs checkpoint.");
            Console.WriteLine("");
            Console.WriteLine("Options");
            Console.WriteLine("");
            if (!ProgramInfo.InSqlMode)
            {
                Console.WriteLine("  -m[ain]");
                Console.WriteLine("     Use the main instance. (Default is to use the child instance.)");
                Console.WriteLine("");
            }
            Console.WriteLine("  -child [<username>]");
            Console.WriteLine("     Connect to child instance for the current or specified user.");
            Console.WriteLine("");
            Console.WriteLine(@"  -s[erver] <server>[\<instance>][,<port>]");
            Console.WriteLine("     Specify the server, instance name and/or port to connect to.");
            Console.WriteLine(@"     e.g. -s .\SQLEXPRESS will connect to SSE on the local machine.");
            Console.WriteLine("");
            Console.WriteLine("  -user <username>");
            Console.WriteLine("     Specify the user name to connect with.");
            Console.WriteLine("");
            Console.WriteLine("  -pwd <password> | -pwd?");
            Console.WriteLine("     Specify the password to connect with, or prompt for it.");
            Console.WriteLine("     e.g. -s mysrv -user sa -pwd? will prompt for a password.");
            Console.WriteLine("");
            Console.WriteLine("  -timeout <seconds>");
            Console.WriteLine("     Specify the connection timeout in seconds. 0 for infinite.");
            Console.WriteLine("");
            Console.WriteLine("  -cmdtimeout <seconds>");
            Console.WriteLine("     Specify the command timeout in seconds. 0 for infinite.");
            Console.WriteLine("");
            Console.WriteLine("  -log <logfile>");
            Console.WriteLine("     Write all program input/output to the specified log file.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("For more information about a command, type '" + ProgramInfo.ExecutableName + " help <commandname>'");
            Console.WriteLine("");
            Console.WriteLine("");
        }
    }
}

