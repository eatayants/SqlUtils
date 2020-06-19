using System;
using System.Text;

namespace SqlUtils.Commands
{
    internal class ShowHelpCommand : ISimpleCommand, ICommand
    {
        private string _commandName;
        private bool _isConsole;

        internal ShowHelpCommand()
        {
            this._commandName = null;
        }

        internal ShowHelpCommand(string commandName, bool isConsole)
        {
            this._isConsole = isConsole;
            this._commandName = commandName;
        }

        void ISimpleCommand.Execute()
        {
            ExecuteDirect(this._commandName, this._isConsole);
        }

        internal static void ExecuteDirect()
        {
            ExecuteDirect(null, false);
        }

        internal static void ExecuteDirect(string commandName, bool isConsole)
        {
            if (string.IsNullOrEmpty(commandName))
            {
                Help.Show();
            }
            else
            {
                ShowHelpContext ctx = new ShowHelpContext();
                if (isConsole)
                {
                    ctx.CommandPrefix = Settings.ConsoleCommandPrefix;
                    ctx.UseProgramName = false;
                }
                else
                {
                    ctx.CommandPrefix = "-";
                    ctx.UseProgramName = true;
                }
                commandName = commandName.ToLowerInvariant();
                Console.WriteLine("");
                Console.WriteLine("");
                switch (commandName)
                {
                    case "attach":
                        ShowHelpForAttach(ctx);
                        break;

                    case "create":
                        ShowHelpForCreate(ctx);
                        break;

                    case "detach":
                        ShowHelpForDetach(ctx);
                        break;

                    case "shrink":
                        ShowHelpForShrink(ctx);
                        break;

                    case "trace":
                        ShowHelpForTrace(ctx);
                        break;

                    case "childlist":
                        ShowHelpForChildList(ctx);
                        break;

                    case "list":
                        ShowHelpForList(ctx);
                        break;

                    case "listsrv":
                        ShowHelpForListSrv(ctx);
                        break;

                    case "run":
                        ShowHelpForRun(ctx);
                        break;

                    case "console":
                        ShowHelpForConsole(ctx);
                        break;

                    case "consolewnd":
                        ShowHelpForConsoleWnd(ctx);
                        break;

                    case "version":
                        ShowHelpForVersion(ctx);
                        break;

                    case "upgrade":
                        ShowHelpForUpgrade(ctx);
                        break;

                    default:
                        Console.WriteLine("No extended help available for that command.");
                        break;
                }
                Console.WriteLine("");
            }
        }

        private static string GetCommandSyntax(ShowHelpContext ctx, string commandName, string commandArgs)
        {
            StringBuilder builder = new StringBuilder();
            if (ctx.UseProgramName)
            {
                builder.Append(ProgramInfo.ExecutableName + " ");
            }
            builder.Append(ctx.CommandPrefix);
            builder.Append(commandName);
            builder.Append(" ");
            builder.Append(commandArgs);
            return builder.ToString();
        }

        private static void ShowHelpForAttach(ShowHelpContext ctx)
        {
            Console.WriteLine("Attach command");
            Console.WriteLine("--------------");
            Console.WriteLine("");
            Console.WriteLine("Used to attach a database file to the database server.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Syntax:");
            Console.WriteLine("");
            Console.WriteLine("    attach <dbpath> [<dbname>]");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Description:");
            Console.WriteLine("");
            Console.WriteLine("    Attach takes the path of the database file to attach (usually a .mdf file).");
            Console.WriteLine("    If a database name is specified, the database will be attached under that");
            Console.WriteLine("    name and it can be used when executing SQL queries. Otherwise, the tool");
            Console.WriteLine("    will rely on the server to generate a name (this is only suppoted by some");
            Console.WriteLine("    versions of SQL Server like SQL Express 2005.");
            Console.WriteLine("");
            Console.WriteLine("    Note that when executing this command against a remote server, the data");
            Console.WriteLine("    file path must refer to a location accessible by the remote machine and");
            Console.WriteLine("    considered valid by the server.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Examples:");
            Console.WriteLine("");
            Console.WriteLine("    " + GetCommandSyntax(ctx, "attach", @"c:\data\northwind.mdf"));
            Console.WriteLine("    " + GetCommandSyntax(ctx, "attach", @"c:\data\northwind.mdf northwind"));
        }

        private static void ShowHelpForChildList(ShowHelpContext ctx)
        {
            Console.WriteLine("ChildList command");
            Console.WriteLine("--------------");
            Console.WriteLine("");
            Console.WriteLine("Used to list the child instances of SQL Express.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Syntax");
            Console.WriteLine("");
            Console.WriteLine("   childlist");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Details");
            Console.WriteLine("");
            Console.WriteLine("   SQL Server Express allows the creation of user instances (or child");
            Console.WriteLine("   instances) where different users are isolated from each other. This");
            Console.WriteLine("   command will list the child instances along with their pipe name");
            Console.WriteLine("   which can be used to connect to them in tools that do not support ");
            Console.WriteLine("   instance redirection.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Examples");
            Console.WriteLine("");
            Console.WriteLine("   " + GetCommandSyntax(ctx, "childlist", ""));
            Console.WriteLine("");
        }

        private static void ShowHelpForConsole(ShowHelpContext ctx)
        {
            Console.WriteLine("Console command");
            Console.WriteLine("--------------");
            Console.WriteLine("");
            Console.WriteLine("Launches the console mode.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Syntax");
            Console.WriteLine("");
            Console.WriteLine("   console");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Details");
            Console.WriteLine("");
            Console.WriteLine("   The console allows the user to enter SQL commands and extended commands");
            Console.WriteLine("   to run against the database server. For more information, type 'help'");
            Console.WriteLine("   in the console.");
            Console.WriteLine("");
            Console.WriteLine("");
        }

        private static void ShowHelpForConsoleWnd(ShowHelpContext ctx)
        {
            Console.WriteLine("ConsoleWnd command");
            Console.WriteLine("--------------");
            Console.WriteLine("");
            Console.WriteLine("Launches the console window.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Syntax");
            Console.WriteLine("");
            Console.WriteLine("   consolewnd");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Details");
            Console.WriteLine("");
            Console.WriteLine("   The console window allows the user to interact with the SQL console.");
            Console.WriteLine("   The same commands as in the console can be entered. It can also");
            Console.WriteLine("   load/save command files.");
            Console.WriteLine("");
            Console.WriteLine("");
        }

        private static void ShowHelpForCreate(ShowHelpContext ctx)
        {
            Console.WriteLine("Create command");
            Console.WriteLine("--------------");
            Console.WriteLine("");
            Console.WriteLine("Used to create a new database.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Syntax");
            Console.WriteLine("");
            Console.WriteLine("   create <dbpath>|" + Settings.DatabaseNamePrefix + "<dbname> [<dbname>]");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Details");
            Console.WriteLine("");
            Console.WriteLine("   Create can take a path to a database file to create and/or database name.");
            Console.WriteLine("");
            Console.WriteLine("   (1) When a path is specified, a database name can also be specified.");
            Console.WriteLine("       If the name is specified, the database will be attached under that");
            Console.WriteLine("       name. If no name is specified, the data file will not be attached.");
            Console.WriteLine("");
            Console.WriteLine("   (2) When a database name is specified in brackets instead of a file path,");
            Console.WriteLine("       a database with that name will be created on the server. The location");
            Console.WriteLine("       of the data file will be determined by the server.");
            Console.WriteLine("");
            Console.WriteLine("Examples");
            Console.WriteLine("");
            Console.WriteLine("    " + GetCommandSyntax(ctx, "create", Settings.DatabaseNamePrefix + "northwind"));
            Console.WriteLine("      Creates a new database called 'northwind' on the server.");
            Console.WriteLine("");
            Console.WriteLine("    " + GetCommandSyntax(ctx, "create", @"c:\data\northwind.mdf"));
            Console.WriteLine("      Creates a new database file. Database is not attached to the server.");
            Console.WriteLine("");
            Console.WriteLine("    " + GetCommandSyntax(ctx, "create", @"c:\data\northwind.mdf northwind"));
            Console.WriteLine("      Creates a new database called 'northwind' at the location specified.");
        }

        private static void ShowHelpForDetach(ShowHelpContext ctx)
        {
            Console.WriteLine("Detach command");
            Console.WriteLine("--------------");
            Console.WriteLine("");
            Console.WriteLine("Used to detach a database from the server.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Syntax");
            Console.WriteLine("");
            Console.WriteLine("   detach <dbpath[*]>|dbname");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Details");
            Console.WriteLine("");
            Console.WriteLine("   Detach can take the path of the data file to detach or a database name.");
            Console.WriteLine("");
            Console.WriteLine("   (1) When a database path is specified, any database currently attached to");
            Console.WriteLine("       the server under that path will be detached. The path can end with an");
            Console.WriteLine("       asterisk '*' in which case all database paths starting with the value");
            Console.WriteLine("       specified will be matched.");
            Console.WriteLine("");
            Console.WriteLine("   (2) When a database name is specified in brackets, any database with a name");
            Console.WriteLine("       that matches the value specified will be detached.");
            Console.WriteLine("");
            Console.WriteLine("   Note that the master, tempdb, model and msdb databases will not be detached");
            Console.WriteLine("   by this command. If you need to do this, use standard SQL commands in the");
            Console.WriteLine("   console mode instead.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Examples");
            Console.WriteLine("");
            Console.WriteLine("    " + GetCommandSyntax(ctx, "detach", Settings.DatabaseNamePrefix + "northwind"));
            Console.WriteLine("");
            Console.WriteLine("    " + GetCommandSyntax(ctx, "detach", @"c:\data\northwind.mdf"));
            Console.WriteLine("");
            Console.WriteLine("    " + GetCommandSyntax(ctx, "detach", @"c:\data\*"));
            Console.WriteLine(@"      Will detach all file under c:\data\.");
        }

        private static void ShowHelpForList(ShowHelpContext ctx)
        {
            Console.WriteLine("List command");
            Console.WriteLine("--------------");
            Console.WriteLine("");
            Console.WriteLine("List the databases on the current server instance.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Syntax");
            Console.WriteLine("");
            Console.WriteLine("   list");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Details");
            Console.WriteLine("");
            Console.WriteLine("   This command lists the databases attached to a given server instance.");
            Console.WriteLine("   Note that SQL Server Express supports child instances and therefore");
            Console.WriteLine("   databases attached to the child instances are separate from the ones");
            Console.WriteLine("   attached to the main instance. To see the databases on the main instance");
            Console.WriteLine("   use the -main switch. To see the databases on the child instance, use");
            Console.WriteLine("   the -child switch (this is the default for ExecSqlCmd).");
            Console.WriteLine("");
            Console.WriteLine("Examples");
            Console.WriteLine("");
            Console.WriteLine("   " + GetCommandSyntax(ctx, "list", ""));
        }

        private static void ShowHelpForListSrv(ShowHelpContext ctx)
        {
            Console.WriteLine("ListSrv");
            Console.WriteLine("--------------");
            Console.WriteLine("");
            Console.WriteLine("Lists the SQL Server instances installed or running");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Syntax");
            Console.WriteLine("");
            Console.WriteLine("   ListSrv [remote]");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Details");
            Console.WriteLine("");
            Console.WriteLine("   If the 'remote' keyword is omitted, ListSrv will list the instances");
            Console.WriteLine("   of SQL Server installed on the local machine whether or not they are");
            Console.WriteLine("   running. With the 'remote' keyword, ListSrv will try to discover the");
            Console.WriteLine("   instances of SQL Server that are currently running on the network.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Examples");
            Console.WriteLine("");
            Console.WriteLine("   " + GetCommandSyntax(ctx, "listsrv", ""));
            Console.WriteLine("");
            Console.WriteLine("   " + GetCommandSyntax(ctx, "listsrv", "remote"));
        }

        private static void ShowHelpForRun(ShowHelpContext ctx)
        {
            Console.WriteLine("Run command");
            Console.WriteLine("--------------");
            Console.WriteLine("");
            Console.WriteLine("Used to run a command file or script file.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Syntax");
            Console.WriteLine("");
            Console.WriteLine("   run <filepath> [<var1>=<val1>[,...]]");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Details");
            Console.WriteLine("");
            Console.WriteLine("   This command will run a given command file against the database server.");
            Console.WriteLine("   The file can contain SQL commands and/or extended commands (see console).");
            Console.WriteLine("");
            Console.WriteLine("   Command files can also contain variables. The variables will be replaced");
            Console.WriteLine("   with the provided value(s) before the query is sent to the database server. Variables");
            Console.WriteLine("   must be declared as follows in the file: " + Settings.ExpansionVariablePrefix + "VARNAME " + Settings.ExpansionVariableSuffix + ". For example, if a");
            Console.WriteLine("   file contains 'SELECT * FROM " + Settings.ExpansionVariablePrefix + "TABLENAME" + Settings.ExpansionVariableSuffix + "', you can give it a value on");
            Console.WriteLine("   the command like like this: 'tablename=Customers'. The SQL query sent will");
            Console.WriteLine("   then be: 'SELECT * FROM Customers'.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Examples");
            Console.WriteLine("");
            Console.WriteLine("   " + GetCommandSyntax(ctx, "run", "createdb.scr"));
            Console.WriteLine("");
            Console.WriteLine("   " + GetCommandSyntax(ctx, "run", @"c:\myscripts\powerscript.txt database=Pubs,table=Authors"));
        }

        private static void ShowHelpForShrink(ShowHelpContext ctx)
        {
            Console.WriteLine("Shrink command");
            Console.WriteLine("--------------");
            Console.WriteLine("");
            Console.WriteLine("Used to checkpoint/shrink a database.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Syntax");
            Console.WriteLine("");
            Console.WriteLine("   shrink <dbpath>|" + Settings.DatabaseNamePrefix + "<dbname>");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Details");
            Console.WriteLine("");
            Console.WriteLine("   Shrink can take the path to the database file or a database name.");
            Console.WriteLine("   It will ask the database server to shrink the database by removing");
            Console.WriteLine("   extra space. This command also performs a checkpoint on the database");
            Console.WriteLine("   before executing the shrink operation.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Examples");
            Console.WriteLine("");
            Console.WriteLine("   " + GetCommandSyntax(ctx, "shrink", Settings.DatabaseNamePrefix + "northwind"));
            Console.WriteLine("");
            Console.WriteLine("   " + GetCommandSyntax(ctx, "shrink", @"c:\data\northwind.mdf"));
        }

        private static void ShowHelpForTrace(ShowHelpContext ctx)
        {
            Console.WriteLine("Trace command");
            Console.WriteLine("--------------");
            Console.WriteLine("");
            Console.WriteLine("Used to enable/disable trace flags on the server.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Syntax");
            Console.WriteLine("");
            Console.WriteLine("   trace +|-[<tracenumber>]");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Details");
            Console.WriteLine("");
            Console.WriteLine("   Trace flags can be useful for a number of things including tracing what SQL");
            Console.WriteLine("   commands are being executed by the server. Trace can enable/disable a");
            Console.WriteLine("   specific trace number or if omitted, enable/disable tracing of SQL commands.");
            Console.WriteLine("   sent by any client connection. Trace output is usually written to the server");
            Console.WriteLine("   log file.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Examples");
            Console.WriteLine("");
            Console.WriteLine("    " + GetCommandSyntax(ctx, "trace", "+"));
            Console.WriteLine("");
            Console.WriteLine("    " + GetCommandSyntax(ctx, "trace", "-4054"));
        }

        private static void ShowHelpForUpgrade(ShowHelpContext ctx)
        {
            Console.WriteLine("Upgrade command");
            Console.WriteLine("--------------");
            Console.WriteLine("");
            Console.WriteLine("Used to ensure that the database file(s) are upgraded to match the server version.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Syntax");
            Console.WriteLine("");
            Console.WriteLine("   upgrade <dbpath>|<directory>");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Details");
            Console.WriteLine("");
            Console.WriteLine("   When working with databases created with previous versions of SQL Server,");
            Console.WriteLine("   the databases must be upgraded by the server before they can be used. This");
            Console.WriteLine("   function makes it easy to take one or more database files and have the");
            Console.WriteLine("   server upgrade them. All the upgrade work is done by the server itself.");
            Console.WriteLine("");
            Console.WriteLine("   Upgrade can take a database file path or directory as argument. If a");
            Console.WriteLine("   directory is specified, all data files (.mdf) in that folder will be");
            Console.WriteLine("   upgraded.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Examples");
            Console.WriteLine("");
            Console.WriteLine("    " + GetCommandSyntax(ctx, "upgrade", @"c:\data\northwind.mdf"));
            Console.WriteLine("");
            Console.WriteLine("    " + GetCommandSyntax(ctx, "upgrade", @"c:\data\"));
        }

        private static void ShowHelpForVersion(ShowHelpContext ctx)
        {
            Console.WriteLine("Version command");
            Console.WriteLine("--------------");
            Console.WriteLine("");
            Console.WriteLine("Used to display the version of the database server.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Syntax");
            Console.WriteLine("");
            Console.WriteLine("   version");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Details");
            Console.WriteLine("");
            Console.WriteLine("   This command retrieves and displays the version information");
            Console.WriteLine("   as reported by SQL Server.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Examples");
            Console.WriteLine("");
            Console.WriteLine("   " + GetCommandSyntax(ctx, "version", ""));
            Console.WriteLine("");
            Console.WriteLine("   " + GetCommandSyntax(ctx, "version", "-s machine1 -m -version"));
            Console.WriteLine("       Retrieves the version of SQL running on machine1, main instance.");
        }
    }
}

