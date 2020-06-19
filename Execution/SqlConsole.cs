using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using SqlUtils.Commands;
using SqlUtils.Scripting;

namespace SqlUtils
{
    internal class SqlConsole : ISqlConsole
    {
        private bool _addToHistory = true;
        internal List<string> CommandBuffer;
        private IDbConnection _connection;
        private StringBuilder _currentCommandEntry = new StringBuilder();
        private long _currentLineNumber = 1L;
        private readonly Engine _engine;
        private readonly History _history = new History();
        internal object LastScalar;
        internal DataTable LastTable;
        internal bool QuitConsole;
        private bool _timerEnabled;

        internal SqlConsole(Engine engine)
        {
            this._engine = engine;
        }

        private void EnsureInitialized()
        {
            if (this._connection == null)
            {
                this._connection = this._engine.ConnectionManager.BuildMasterConnection();
                this._connection.Open();
                this.CommandBuffer = new List<string>();
            }
        }

        void ISqlConsole.Execute(string command)
        {
            string line = null;
            StringReader reader = new StringReader(command);
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(this._currentLineNumber + "> " + line);
                if (!this.ProcessConsoleEntry(line))
                {
                    this.QuitConsole = true;
                    return;
                }
                if (ConsoleHandler.IsLogOpen)
                {
                    try
                    {
                        ConsoleHandler.FlushLog();
                        continue;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            this._currentLineNumber = 1L;
            this.CommandBuffer.Clear();
        }

        private void ExecuteExtendedCommand(ConsoleCommand command)
        {
            try
            {
                if (command is TimerCommand)
                {
                    this._timerEnabled = !this._timerEnabled;
                    if (this._timerEnabled)
                    {
                        Console.WriteLine("Timer enabled.");
                    }
                    else
                    {
                        Console.WriteLine("Timer disabled.");
                    }
                    return;
                }
                if (command is CommandTimeoutCommand)
                {
                    int timeout = ((CommandTimeoutCommand) command).Timeout;
                    if (timeout < 0)
                    {
                        Console.WriteLine("Invalid command timeout specified. Must be a positive integer.");
                    }
                    else
                    {
                        this._engine.ConnectionManager.ConnectionOptions.CommandTimeout = timeout;
                        if (timeout == 0)
                        {
                            Console.WriteLine("Command timeout set to 0 (unlimited).");
                        }
                        else if (timeout > 0)
                        {
                            Console.WriteLine("Command timeout set to " + timeout + " second(s).");
                        }
                    }
                    return;
                }
                if (command is ShowHistoryCommand)
                {
                    this.ShowHistory(((ShowHistoryCommand) command).CommandCount);
                    return;
                }
                if (command is SaveHistoryCommand)
                {
                    this.SaveHistory(((SaveHistoryCommand) command).FilePath);
                    return;
                }
                if (command is ClearHistoryCommand)
                {
                    this._addToHistory = false;
                    this._history.Clear();
                    Console.WriteLine("History cleared.");
                    return;
                }
                if (command is OpenLogCommand)
                {
                    if (ConsoleHandler.IsLogOpen)
                    {
                        throw new Exception("Log is already opened. To create a new log file, first close the current log using 'logclose'.");
                    }
                    ConsoleHandler.StartLog(((OpenLogCommand) command).FilePath, "> " + Settings.ConsoleCommandPrefix + "logopen", new string[] { ((OpenLogCommand) command).FilePath });
                    Console.WriteLine("Log opened.");
                    return;
                }
                if (command is CloseLogCommand)
                {
                    if (!ConsoleHandler.IsLogOpen)
                    {
                        throw new Exception("Log is already closed.");
                    }
                    ConsoleHandler.StopLog();
                    Console.WriteLine("Log closed.");
                    return;
                }
                Console.WriteLine("Extended command not recognized.");
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error processing extended command.");
                if (!string.IsNullOrEmpty(exception.Message))
                {
                    Console.WriteLine(exception.Message);
                }
            }
            Console.WriteLine("");
        }

        private void ExecuteSqlCommands(List<string> commandsToExecute, ProcessingOption option)
        {
            if (commandsToExecute.Count > 0)
            {
                string str = "";
                for (int i = 0; i < commandsToExecute.Count; i++)
                {
                    str = str + commandsToExecute[i];
                    if (i < (commandsToExecute.Count - 1))
                    {
                        str = str + ";";
                    }
                }
                commandsToExecute.Clear();
                try
                {
                    SqlCommand command = (SqlCommand) DataUtil.CreateCommand(this._engine.ConnectionManager, this._connection);
                    if (this._engine.ConnectionManager.ConnectionOptions.CommandTimeout >= 0)
                    {
                        command.CommandTimeout = this._engine.ConnectionManager.ConnectionOptions.CommandTimeout;
                    }
                    command.CommandText = str;
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader != null)
                    {
                        try
                        {
                            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                            stopwatch.Stop();
                            int num3 = 1;
                            do
                            {
                                if (num3 > 1)
                                {
                                    Console.WriteLine("");
                                    Console.WriteLine("Results #" + num3);
                                    Console.WriteLine("==========================");
                                }
                                if (reader.FieldCount > 0)
                                {
                                    Console.WriteLine("");
                                    Console.WriteLine(DataUtil.FetchAndDisplayRows(reader, option, out this.LastTable, out this.LastScalar) + " row(s) affected.");
                                }
                                else if (reader.RecordsAffected >= 0)
                                {
                                    Console.WriteLine("");
                                    Console.WriteLine(reader.RecordsAffected + " row(s) affected.");
                                }
                                else if (!Global.SilentMode)
                                {
                                    Console.WriteLine("Command completed successfully.");
                                }
                                if (this._timerEnabled)
                                {
                                    Console.WriteLine("");
                                    Console.WriteLine(elapsedMilliseconds + " milliseconds.");
                                }
                                num3++;
                            }
                            while (reader.NextResult());
                        }
                        finally
                        {
                            try
                            {
                                reader.Close();
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error executing commands:" + Environment.NewLine);
                    DataUtil.DisplayException(exception);
                    commandsToExecute.Clear();
                }
            }
        }

        private void PlayCommandFile(string filePath, IDbConnection connection, Dictionary<string, string> variables)
        {
            ScriptEngine.ExecuteScript(this, filePath, connection, variables);
        }

        internal bool ProcessConsoleEntry(string line)
        {
            return this.ProcessConsoleEntry(line, ProcessingOption.None);
        }

        internal bool ProcessConsoleEntry(string line, ProcessingOption option)
        {
            string[] strArray;
            string str = line;
            line = line.ToLowerInvariant().Trim();
            if (string.IsNullOrEmpty(line))
            {
                strArray = new string[0];
            }
            else
            {
                int index = line.IndexOf(" ");
                if (index == -1)
                {
                    line.Substring(1).ToLowerInvariant();
                    strArray = new string[0];
                }
                else
                {
                    line.Substring(index).ToLowerInvariant();
                    strArray = ConsoleUtil.ParseCommandArgs(line.Substring(index + 1));
                }
            }
            this.LastTable = null;
            this.LastScalar = null;
            try
            {
                if (line.StartsWith(Settings.ConsoleCommandPrefix))
                {
                    this.ProcessExtendedCommand(line);
                }
                else if (string.Compare(line, "go", StringComparison.Ordinal) == 0)
                {
                    if (this.CommandBuffer == null)
                    {
                        return true;
                    }
                    if (this._currentCommandEntry.Length > 0)
                    {
                        this.CommandBuffer.Add(this._currentCommandEntry.ToString());
                        this._currentCommandEntry = new StringBuilder();
                    }
                    this._currentLineNumber = 1L;
                    this.ExecuteSqlCommands(this.CommandBuffer, option);
                }
                else
                {
                    if ((string.Compare(line, "quit", StringComparison.Ordinal) == 0) || (string.Compare(line, "exit", StringComparison.Ordinal) == 0))
                    {
                        return false;
                    }
                    if (string.Compare(line, "help", StringComparison.Ordinal) == 0)
                    {
                        if (strArray.Length > 1)
                        {
                            throw new CommandParserInvalidArgumentsException("help");
                        }
                        if (strArray.Length == 1)
                        {
                            SqlConsoleHelp.Show(strArray[0]);
                        }
                        else
                        {
                            SqlConsoleHelp.Show(null);
                        }
                        return true;
                    }
                    if (option == ProcessingOption.None)
                    {
                        this._currentCommandEntry.Append(str + Environment.NewLine);
                        this._currentLineNumber += 1L;
                    }
                    else
                    {
                        this.ExecuteSqlCommands(new List<string>(new string[] { line }), option);
                    }
                }
                if (this._addToHistory && !ScriptEngine.IsExecutingScript)
                {
                    this._history.Add(line);
                }
            }
            finally
            {
                this._addToHistory = true;
            }
            return true;
        }

        private void ProcessExtendedCommand(string commandLine)
        {
            try
            {
                ICommand command = ConsoleCommandParser.Parse(commandLine);
                if (command == null)
                {
                    throw new Exception("Error processing command.");
                }
                if (command is ConsoleCommand)
                {
                    this.ExecuteExtendedCommand((ConsoleCommand) command);
                }
                else if (command is ISimpleCommand)
                {
                    ((ISimpleCommand) command).Execute();
                }
                else if (command is IEngineCommand)
                {
                    this._engine.Execute((IEngineCommand) command);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error processing extended command.");
                if (!string.IsNullOrEmpty(exception.Message))
                {
                    Console.WriteLine(exception.Message);
                }
            }
            Console.WriteLine("");
        }

        internal void Run()
        {
            this.Run(null, null);
        }

        internal void Run(string commandFile, Dictionary<string, string> variables)
        {
            this.EnsureInitialized();
            try
            {
                if (string.IsNullOrEmpty(commandFile))
                {
                    string str;
                    Console.WriteLine("");
                    Console.WriteLine("Console mode. Type 'help' for more information.");
                    this.QuitConsole = false;
                    do
                    {
                        Console.Write(this._currentLineNumber + "> ");
                        str = Console.ReadLine();
                        if ((str == null) || !this.ProcessConsoleEntry(str))
                        {
                            return;
                        }
                        if (ConsoleHandler.IsLogOpen)
                        {
                            try
                            {
                                ConsoleHandler.FlushLog();
                            }
                            catch
                            {
                            }
                        }
                    }
                    while ((str != null) && !this.QuitConsole);
                }
                else
                {
                    this.PlayCommandFile(commandFile, this._connection, variables);
                }
            }
            finally
            {
                try
                {
                    this._connection.Close();
                }
                catch
                {
                }
            }
        }

        internal void RunCommand(ConsoleCommand command)
        {
            this.EnsureInitialized();
            this.ExecuteExtendedCommand(command);
        }

        private void SaveHistory(string path)
        {
            if ((this._history == null) || (this._history.Count == 0))
            {
                Console.WriteLine("History is empty.");
            }
            else
            {
                StreamWriter writer = new StreamWriter(path);
                foreach (string str in (IEnumerable<string>) this._history)
                {
                    writer.WriteLine(str);
                }
                writer.Flush();
                writer.Close();
                Console.WriteLine("History saved successfully.");
            }
        }

        private void ShowHistory(int commandCount)
        {
            if ((this._history == null) || (this._history.Count == 0))
            {
                Console.WriteLine("History is empty.");
            }
            else
            {
                int num;
                if (commandCount == 0)
                {
                    num = 0;
                }
                else if (commandCount > 0)
                {
                    num = this._history.Count - commandCount;
                }
                else
                {
                    return;
                }
                Console.WriteLine("");
                int num2 = Util.NumberOfDigits(this._history.Count) + 2;
                for (int i = num; i < this._history.Count; i++)
                {
                    Console.WriteLine(string.Format("{0,-" + num2 + "}{1}", (i + 1) + ".", this._history[i]));
                }
                Console.WriteLine("");
            }
        }

        internal IDbConnection Connection
        {
            get
            {
                return this._connection;
            }
        }

        internal enum ProcessingOption
        {
            None,
            Scalar,
            Table,
            Immediate
        }
    }
}

