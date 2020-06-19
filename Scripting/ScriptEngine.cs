using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using SqlUtils.Properties;

namespace SqlUtils.Scripting
{
    internal class ScriptEngine
    {
        private static Dictionary<string, ScriptDocument> _executionMap;

        internal static void ExecuteScript(SqlConsole console, string filePath, IDbConnection connection, Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception("Invalid empty script file path");
            }
            filePath = PathUtil.EnsureFullPath(filePath);
            if (_executionMap == null)
            {
                _executionMap = new Dictionary<string, ScriptDocument>(StringComparer.OrdinalIgnoreCase);
            }
            ScriptDocument document = new ScriptDocument();
            document.Load(filePath);
            _executionMap[document.FilePath] = document;
            try
            {
                Console.WriteLine("");
                if (document.ScriptBlock != null)
                {
                    var block = document.ScriptBlock as CommandBlock;
                    if (block != null)
                    {
                        var scriptBlock = block;
                        foreach (string str in scriptBlock.Commands)
                        {
                            var command = str;
                            if ((variables != null) && (variables.Count > 0))
                            {
                                command = ExpandVariables(command, variables);
                            }
                            if (!Global.NoCode)
                            {
                                Console.WriteLine(@"> " + command);
                            }
                            console.ProcessConsoleEntry(command, SqlConsole.ProcessingOption.None);
                        }
                    }
                    Console.WriteLine(Resources.ScriptEngine_PlaybackCompleted, Path.GetFileName(document.FilePath));
                }
                else
                {
                    Console.WriteLine(Resources.ScriptEngine_ExecuteScript_CommandFileIsEmpty);
                }
            }
            finally
            {
                _executionMap.Remove(document.FilePath);
            }
        }

        private static string ExpandVariables(string command, Dictionary<string, string> variables)
        {
            if ((variables == null) || (variables.Count == 0))
            {
                return command;
            }
            int startIndex = 0;
            int index = command.IndexOf(Settings.ExpansionVariablePrefix, StringComparison.Ordinal);
            if (index < 0)
            {
                return command;
            }
            var builder = new StringBuilder();
            do
            {
                builder.Append(command.Substring(startIndex, index - startIndex));
                var num3 = command.IndexOf(Settings.ExpansionVariableSuffix, index + 2, StringComparison.Ordinal);
                if (num3 > -1)
                {
                    var key = command.Substring(index + 2, (num3 - index) - 2);
                    if (variables.ContainsKey(key))
                    {
                        builder.Append(variables[key]);
                        startIndex = num3 + 1;
                    }
                    else
                    {
                        builder.Append(command.Substring(index, num3 - index));
                        startIndex = num3 + 1;
                    }
                }
                else
                {
                    builder.Append(command.Substring(index));
                    break;
                }
                index = command.IndexOf(Settings.ExpansionVariablePrefix, startIndex, StringComparison.Ordinal);
            }
            while (index > -1);
            builder.Append(command.Substring(startIndex));
            return builder.ToString();
        }

        internal static bool IsExecutingScript
        {
            get
            {
                return ((_executionMap != null) && (_executionMap.Count > 0));
            }
        }
    }
}

