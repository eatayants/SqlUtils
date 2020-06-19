using System;
using System.IO;

namespace SqlUtils
{
    internal class ConsoleHandler
    {
        private static TextWriter _logWriter;
        private static LoggingTextReader _newReader;
        private static LoggingTextWriter _newWriter;
        private static TextReader _oldConsoleIn;
        private static TextWriter _oldConsoleOut;

        internal static void Cleanup()
        {
            if (_logWriter != null)
            {
                try
                {
                    _logWriter.Flush();
                    _logWriter.Close();
                }
                catch (Exception)
                {
                }
                _logWriter = null;
            }
            if (_newReader != null)
            {
                Console.SetIn(_oldConsoleIn);
                _oldConsoleIn = null;
                _newReader = null;
            }
            if (_newWriter != null)
            {
                Console.SetOut(_oldConsoleOut);
                _oldConsoleOut = null;
                _newWriter = null;
            }
        }

        private static void EnsureRedirectOutputStream()
        {
            if (_oldConsoleOut == null)
            {
                _oldConsoleOut = Console.Out;
                _newWriter = new LoggingTextWriter();
                _newWriter.AddTextWriter("STDOUT", Console.Out, true);
                Console.SetOut(_newWriter);
            }
        }

        internal static void FlushLog()
        {
            if (_logWriter != null)
            {
                _logWriter.Flush();
            }
        }

        internal static void Initialize()
        {
            EnsureRedirectOutputStream();
        }

        private static void StartLog(TextWriter logWriter, string command, string[] initialArgs)
        {
            ConsoleHandler._logWriter = logWriter;
            _newWriter.AddTextWriter("LOG", logWriter, true);
            _oldConsoleIn = Console.In;
            _newReader = new LoggingTextReader(Console.In, logWriter);
            Console.SetIn(_newReader);
            if (!string.IsNullOrEmpty(command))
            {
                logWriter.Write(command);
                if ((initialArgs != null) && (initialArgs.Length > 0))
                {
                    foreach (string str in initialArgs)
                    {
                        logWriter.Write(" " + str);
                    }
                }
                logWriter.WriteLine("");
            }
        }

        internal static void StartLog(string logFilePath, string command, string[] initialArgs)
        {
            StreamWriter logWriter = new StreamWriter(logFilePath);
            StartLog(logWriter, command, initialArgs);
        }

        internal static void StopLog()
        {
            if (_logWriter != null)
            {
                _logWriter.Flush();
                _logWriter.Close();
                _logWriter = null;
                try
                {
                    _newWriter.RemoveTextWriter("LOG");
                    Console.SetIn(_oldConsoleIn);
                }
                catch (Exception)
                {
                }
            }
        }

        internal static LoggingTextReader Input
        {
            get
            {
                return _newReader;
            }
        }

        internal static bool IsLogOpen
        {
            get
            {
                return (_logWriter != null);
            }
        }

        internal static LoggingTextWriter Output
        {
            get
            {
                return _newWriter;
            }
        }
    }
}

