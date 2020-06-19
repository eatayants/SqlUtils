using System;
using SqlUtils.Commands;

namespace SqlUtils
{
    internal class Program
    {
        internal const int ConsoleInputBufferSize = 0x6400;

        private static void Main(string[] args)
        {
            ConsoleHandler.Initialize();
            ConsoleUtil.ResizeConsoleInputBuffer(0x6400);
            try
            {
                try
                {
                    CommandLineParser.ParseCommandArgs(args);
                }
                catch (CommandParserException exception)
                {
                    if (!string.IsNullOrEmpty(exception.Message))
                    {
                        Console.WriteLine(exception.Message);
                    }
                    return;
                }
                catch (Exception ex)
                {
					if (!string.IsNullOrEmpty(ex.Message))
					{
						Console.WriteLine(ex.Message);
					}
					return;
                }
                if (CommandLineParser.Command == null)
                {
                    ShowHelpCommand.ExecuteDirect();
                }
                else
                {
	                var command = CommandLineParser.Command as ShowHelpCommand;
	                if (command != null)
	                {
		                ((ISimpleCommand) CommandLineParser.Command).Execute();
	                }
	                else
	                {
		                if (!string.IsNullOrEmpty(CommandLineParser.LogFilePath))
		                {
			                CommandLineParser.CleanupArguments(args);
			                ConsoleHandler.StartLog(CommandLineParser.LogFilePath, ProgramInfo.ExecutableName, new string[0]);
		                }
		                try
		                {
			                var engineCommand = CommandLineParser.Command as IEngineCommand;
			                if (engineCommand != null)
			                {
				                new Engine(CommandLineParser.ConnectionOptions).Execute(engineCommand);
			                }
			                else
			                {
				                var simpleCommand = CommandLineParser.Command as ISimpleCommand;
				                if (simpleCommand != null)
				                {
					                simpleCommand.Execute();
				                }
			                }
		                }
		                catch (Exception exception2)
		                {
			                Console.WriteLine(exception2.Message);
		                }
	                }
                }
            }
            finally
            {
                ConsoleHandler.Cleanup();
            }
        }
    }
}

