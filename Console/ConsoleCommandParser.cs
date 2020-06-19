using System;
using System.Collections.Generic;
using SqlUtils.Commands;

namespace SqlUtils
{
    internal class ConsoleCommandParser
    {
        internal static ICommand Parse(string commandLine)
        {
            string str;
            string[] strArray;
            if (commandLine == null)
            {
                throw new CommandParserException("Invalid null command line.");
            }
            if (!commandLine.StartsWith(Settings.ConsoleCommandPrefix))
            {
                throw new CommandParserException("Commands should start with a '!' character.");
            }
            int index = commandLine.IndexOf(" ");
            if (index == -1)
            {
                str = commandLine.Substring(1).ToLowerInvariant();
                strArray = new string[0];
            }
            else
            {
                int startIndex = 1;
                str = commandLine.Substring(startIndex, index - startIndex).ToLowerInvariant();
                strArray = ConsoleUtil.ParseCommandArgs(commandLine.Substring(index + 1));
            }
            if (string.Compare(str, "attach", StringComparison.Ordinal) == 0)
            {
                string filePath = (strArray.Length > 0) ? strArray[0] : null;
                string dbName = (strArray.Length > 1) ? strArray[1] : null;
                if (filePath == null)
                {
                    throw new CommandParserInvalidArgumentsException(str);
                }
                return new AttachDatabaseCommand(filePath, dbName);
            }
            if (string.Compare(str, "detach", StringComparison.Ordinal) == 0)
            {
                string database = (strArray.Length > 0) ? strArray[0] : null;
                if (database == null)
                {
                    throw new CommandParserInvalidArgumentsException(str);
                }
                return new DetachDatabasesCommand(database);
            }
            if (string.Compare(str, "create", StringComparison.Ordinal) == 0)
            {
                string str5 = (strArray.Length > 0) ? strArray[0] : null;
                string databaseName = (strArray.Length > 1) ? strArray[1] : null;
                if (str5 == null)
                {
                    throw new CommandParserInvalidArgumentsException(str);
                }
                return new CreateDatabaseCommand(str5, databaseName);
            }
            if (string.Compare(str, "trace", StringComparison.Ordinal) == 0)
            {
                bool flag;
                if (strArray.Length != 1)
                {
                    throw new CommandParserInvalidArgumentsException(str);
                }
                if (strArray[0][0] == '+')
                {
                    flag = true;
                }
                else
                {
                    if (strArray[0][0] != '-')
                    {
                        throw new CommandParserInvalidArgumentsException(str);
                    }
                    flag = false;
                }
                int result = -1;
                if ((strArray[0].Length > 1) && !int.TryParse(strArray[0].Substring(1), out result))
                {
                    throw new CommandParserInvalidArgumentsException(str);
                }
                return new EnableTracingCommand(flag, result);
            }
            if (string.Compare(str, "commandtimeout", StringComparison.Ordinal) == 0)
            {
                int num4;
                string str7 = (strArray.Length > 0) ? strArray[0] : null;
                if (str7 == null)
                {
                    throw new CommandParserInvalidArgumentsException(str);
                }
                if (!int.TryParse(str7, out num4))
                {
                    throw new CommandParserInvalidArgumentsException(str);
                }
                return new CommandTimeoutCommand(num4);
            }
            if (string.Compare(str, "list", StringComparison.Ordinal) == 0)
            {
                return new ListDatabasesCommand();
            }
            if (string.Compare(str, "run", StringComparison.Ordinal) == 0)
            {
                string scriptPath = (strArray.Length > 0) ? strArray[0] : null;
                string str9 = (strArray.Length > 1) ? strArray[1] : null;
                if (scriptPath == null)
                {
                    throw new CommandParserInvalidArgumentsException(str);
                }
                Dictionary<string, string> variables = null;
                if (!string.IsNullOrEmpty(str9) && !RunCommandFileCommand.TryParseVariableDeclaration(str9, out variables))
                {
                    throw new Exception("Invalid variable declaration. Must be in the form name=value,name2=value2,...");
                }
                return new RunCommandFileCommand(scriptPath, variables);
            }
            if (string.Compare(str, "timer", StringComparison.Ordinal) == 0)
            {
                return new TimerCommand();
            }
            if (string.Compare(str, "history", StringComparison.Ordinal) != 0)
            {
                if (string.Compare(str, "logopen", StringComparison.Ordinal) == 0)
                {
                    string str10 = (strArray.Length > 0) ? strArray[0] : null;
                    if (str10 == null)
                    {
                        throw new CommandParserInvalidArgumentsException(str);
                    }
                    if (string.IsNullOrEmpty(str10))
                    {
                        throw new CommandParserException("Expected log file path as argument.");
                    }
                    return new OpenLogCommand(str10);
                }
                if (string.Compare(str, "logclose", StringComparison.Ordinal) == 0)
                {
                    return new CloseLogCommand();
                }
                if (string.Compare(str, "consolewnd", StringComparison.Ordinal) != 0)
                {
                    throw new CommandParserInvalidCommandException(str);
                }
                return new ConsoleWindowCommand();
            }
            string str11 = (strArray.Length > 0) ? strArray[0].ToLowerInvariant() : null;
            string s = (strArray.Length > 1) ? strArray[1] : null;
            string str13 = str11;
            if (str13 == null)
            {
                throw new CommandParserInvalidArgumentsException(str);
            }
            if (!(str13 == "show"))
            {
                if (str13 != "save")
                {
                    if (str13 != "clear")
                    {
                        throw new CommandParserInvalidArgumentsException(str);
                    }
                    return new ClearHistoryCommand();
                }
            }
            else
            {
                int num5 = 0;
                if ((s != null) && (!int.TryParse(s, out num5) || (num5 < 0)))
                {
                    throw new CommandParserInvalidArgumentsException(str);
                }
                return new ShowHistoryCommand(num5);
            }
            if (s == null)
            {
                throw new CommandParserInvalidArgumentsException(str);
            }
            return new SaveHistoryCommand(s);
        }
    }
}

