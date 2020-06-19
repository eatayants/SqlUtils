using System;
using System.Collections.Generic;
using SqlUtils.Commands;
using ExecSqlCmd.Commands;

namespace SqlUtils
{
    internal class CommandLineParser
    {
        private static ICommand _command;
        private static ConnectionOptions _connectionOptions;
        private static string _logFilePath;

        internal static string[] CleanupArguments(string[] arguments)
        {
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i].Equals("-pwd", StringComparison.OrdinalIgnoreCase) || arguments[i].Equals("/pwd", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (arguments.Length > i)
                    {
                        arguments[i] = new string('*', 8);
                    }
                }
            }
            return arguments;
        }

        private static bool IsCommand(string arg)
        {
            if (!arg.StartsWith("-") && !arg.StartsWith("/"))
            {
                return false;
            }
            return (arg.Length > 1);
        }

        private static void ParseCommand(string[] args, ref int i)
        {
            if (args.Length == 0)
            {
                SetCommand(new ShowHelpCommand());
            }
            else if (args[0].Equals("help", StringComparison.OrdinalIgnoreCase) || args[0].Equals("?", StringComparison.OrdinalIgnoreCase))
            {
                SetCommand(new ShowHelpCommand(TryParseString(args, ref i), false));
            }
            else
            {
                if (!IsCommand(args[i]))
                {
                    throw new CommandParserException("Invalid argument '" + args[i] + "'.");
                }
                var a = args[i].Substring(1).ToLowerInvariant();
                if (string.Equals(a, "a", StringComparison.Ordinal) || string.Equals(a, "attach", StringComparison.Ordinal))
                {
                    string filePath = ParseString(args, ref i, a);
                    string dbName = TryParseString(args, ref i);
                    SetCommand(new AttachDatabaseCommand(filePath, dbName));
                }
                else if (string.Equals(a, "create", StringComparison.Ordinal))
                {
                    string str4 = ParseString(args, ref i, a);
                    string databaseName = TryParseString(args, ref i);
                    SetCommand(new CreateDatabaseCommand(str4, databaseName));
                }
                else if (string.Equals(a, "l", StringComparison.Ordinal) || string.Equals(a, "list", StringComparison.Ordinal))
                {
                    SetCommand(new ListDatabasesCommand());
                }
                else if (string.Equals(a, "childlist", StringComparison.Ordinal))
                {
                    SetCommand(new ListChildInstancesCommand());
                }
                else if (string.Equals(a, "d", StringComparison.Ordinal) || string.Equals(a, "detach", StringComparison.Ordinal))
                {
                    SetCommand(new DetachDatabasesCommand(ParseString(args, ref i, a)));
                }
                else if (string.Equals(a, "u", StringComparison.Ordinal) || string.Equals(a, "upgrade", StringComparison.Ordinal))
                {
                    SetCommand(new UpgradeDatabasesCommand(ParseString(args, ref i, a)));
                }
                else if (string.Equals(a, "сс", StringComparison.Ordinal) || string.Equals(a, "concat", StringComparison.Ordinal))
                {
					var _concatConfig = ParseString(args, ref i, a);
					SetCommand(new ConcatCommand(_concatConfig));
                }
				else if (string.Equals(a, "generatescript", StringComparison.Ordinal) || string.Equals(a, "g", StringComparison.Ordinal))
				{
					string dbName = ParseString(args, ref i, a);
					string prefix = ParseString(args, ref i, a);
					string filePath = TryParseString(args, ref i);
					SetCommand(new CreateScripCommand(filePath, dbName, prefix));
				}
                else if (string.Equals(a, "t", StringComparison.Ordinal) || string.Equals(a, "trace", StringComparison.Ordinal))
                {
                    bool flag;
                    ParseString(args, ref i, a);
                    if (args[i][0] == '+')
                    {
                        flag = true;
                    }
                    else
                    {
                        if (args[i][0] != '-')
                        {
                            throw new CommandParserInvalidArgumentsException(a);
                        }
                        flag = false;
                    }
                    int result = -1;
                    if ((args[i].Length > 1) && !int.TryParse(args[i].Substring(1), out result))
                    {
                        throw new CommandParserInvalidArgumentsException(a);
                    }
                    SetCommand(new EnableTracingCommand(flag, result));
                }
                else if (string.Equals(a, "c", StringComparison.Ordinal) || string.Equals(a, "console", StringComparison.Ordinal))
                {
                    SetCommand(new RunConsoleCommand());
                }
                else if (string.Equals(a, "run", StringComparison.Ordinal))
                {
                    string scriptPath = ParseString(args, ref i, a);
                    string rawVariableDeclaration = TryParseString(args, ref i);
                    if (rawVariableDeclaration != null)
                    {
                        Dictionary<string, string> variables = null;
                        if (!RunCommandFileCommand.TryParseVariableDeclaration(rawVariableDeclaration, out variables))
                        {
                            throw new CommandParserInvalidArgumentsException(a);
                        }
                        SetCommand(new RunCommandFileCommand(scriptPath, variables));
                    }
                    else
                    {
                        SetCommand(new RunCommandFileCommand(scriptPath, null));
                    }
                }
                else if (string.Equals(a, "version", StringComparison.Ordinal))
                {
                    SetCommand(new ShowVersionCommand());
                }
                else if (string.Equals(a, "listsrv", StringComparison.Ordinal))
                {
                    int num2;
                    bool listRemote = false;
                    i = num2 = i + 1;
                    if (args.Length > num2)
                    {
                        if (string.Compare(args[i], "remote", StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            throw new CommandParserInvalidArgumentsException(a);
                        }
                        listRemote = true;
                    }
                    SetCommand(new ListServersCommand(listRemote));
                }
                else if (string.Equals(a, "shrink", StringComparison.Ordinal))
                {
                    SetCommand(new ShrinkDatabaseCommand(ParseString(args, ref i, a)));
                }
                else if (string.Equals(a, "s", StringComparison.Ordinal) || string.Equals(a, "server", StringComparison.Ordinal))
                {
                    _connectionOptions.ServerName = ParseString(args, ref i, a);
                }
                else if (string.Equals(a, "user", StringComparison.Ordinal))
                {
                    _connectionOptions.UserName = ParseString(args, ref i, a);
                }
                else if (string.Equals(a, "pwd", StringComparison.Ordinal))
                {
                    _connectionOptions.Password = ParseString(args, ref i, a);
                }
                else if (string.Equals(a, "pwd?", StringComparison.Ordinal))
                {
                    _connectionOptions.PromptForPassword = true;
                }
                else if (string.Equals(a, "timeout", StringComparison.Ordinal))
                {
                    int num3;
                    i = num3 = i + 1;
                    if ((args.Length <= num3) || !int.TryParse(args[i], out _connectionOptions.ConnectionTimeout))
                    {
                        throw new CommandParserInvalidArgumentsException(a);
                    }
                }
                else if (string.Equals(a, "cmdtimeout", StringComparison.Ordinal))
                {
                    int num4;
                    i = num4 = i + 1;
                    if ((args.Length <= num4) || !int.TryParse(args[i], out _connectionOptions.CommandTimeout))
                    {
                        throw new CommandParserInvalidArgumentsException(a);
                    }
                }
                else if (string.Equals(a, "m", StringComparison.Ordinal) || string.Equals(a, "main", StringComparison.Ordinal))
                {
                    _connectionOptions.UseMainInstance = true;
                }
                else if (string.Equals(a, "child", StringComparison.Ordinal))
                {
                    int num5;
                    _connectionOptions.UseMainInstance = false;
                    i = num5 = i + 1;
                    if (args.Length > num5)
                    {
                        if (!args[i].StartsWith("-") && !args[i].StartsWith("/"))
                        {
                            _connectionOptions.RunningAs = args[i];
                        }
                        else
                        {
                            i--;
                        }
                    }
                }
                else if (string.Equals(a, "log", StringComparison.Ordinal))
                {
                    _logFilePath = ParseString(args, ref i, a);
                }
                else if (string.Equals(a, "nocode", StringComparison.Ordinal))
                {
                    Global.NoCode = true;
                }
                else if (string.Equals(a, "silentmode", StringComparison.Ordinal))
                {
                    Global.SilentMode = true;
                }
                else
                {
                    if ((!string.Equals(a, "?", StringComparison.Ordinal) && !string.Equals(a, "help", StringComparison.Ordinal)) && !string.Equals(a, "-help", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new CommandParserInvalidCommandException(a);
                    }
                    SetCommand(new ShowHelpCommand(TryParseString(args, ref i), false));
                }
            }
        }

        internal static void ParseCommandArgs(string[] args)
        {
            _command = null;
            _connectionOptions = null;
            if (args.Length == 0)
            {
                _command = new ShowHelpCommand();
            }
            else
            {
                _connectionOptions = new ConnectionOptions();
                for (int i = 0; i < args.Length; i++)
                {
                    ParseCommand(args, ref i);
                }
                if (_command == null)
                {
                    _command = new ShowHelpCommand();
                }
                VerifyArguments();
            }
        }

        private static string ParseString(string[] args, ref int position, string commandName)
        {
            int num;
            position = num = position + 1;
            if (args.Length <= num)
            {
                throw new CommandParserInvalidArgumentsException(commandName);
            }
            return args[position];
        }

        protected static void SetCommand(ICommand newCommand)
        {
            if (Command != null)
            {
                throw new CommandParserException("Can only specify one command at a time.");
            }
            _command = newCommand;
        }

        private static string TryParseString(string[] args, ref int position)
        {
            int num;
            position = num = position + 1;
            if (args.Length > num)
            {
                if (!IsCommand(args[position]))
                {
                    return args[position];
                }
                position--;
            }
            return null;
        }

        private static void VerifyArguments()
        {
            if (_command != null)
            {
                if (_connectionOptions.UserName != null)
                {
                    if (((_connectionOptions.Password != null) || !_connectionOptions.PromptForPassword) && ((_connectionOptions.Password == null) || _connectionOptions.PromptForPassword))
                    {
                        throw new CommandParserException("Password must be specified when using the 'user' switch.");
                    }
                }
                else if ((_connectionOptions.Password != null) || _connectionOptions.PromptForPassword)
                {
                    throw new CommandParserException("User must be specified when using the 'pwd' switch.");
                }
            }
            if (!string.IsNullOrEmpty(_connectionOptions.RunningAs) && _connectionOptions.UseMainInstance)
            {
                throw new CommandParserException("The 'child' option can only be specified to connect to user instances, not a main instance.");
            }
        }

        internal static ICommand Command
        {
            get
            {
                return _command;
            }
        }

        internal static ConnectionOptions ConnectionOptions
        {
            get
            {
                return _connectionOptions;
            }
        }

        internal static string LogFilePath
        {
            get
            {
                return _logFilePath;
            }
        }
    }
}

