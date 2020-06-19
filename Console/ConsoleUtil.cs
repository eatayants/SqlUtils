using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SqlUtils
{
    internal class ConsoleUtil
    {
        [DllImport("shell32.dll")]
        private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);
        [DllImport("kernel32")]
        internal static extern bool FreeConsole();
        [DllImport("kernel32")]
        internal static extern uint GetConsoleCP();
        [DllImport("kernel32")]
        internal static extern IntPtr GetConsoleWindow();
        internal static string[] ParseCommandArgs(string args)
        {
            int pNumArgs;
            var ptr = CommandLineToArgvW(args, out pNumArgs);
            var strArray = new string[pNumArgs];
            for (var i = 0; i < pNumArgs; i++)
            {
                strArray[i] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(ptr, i * IntPtr.Size));
            }
            return strArray;
        }

        internal static string PromptForPassword(string promptText)
        {
            return PromptUser(promptText, '*');
        }

        internal static string PromptUser(string promptText)
        {
            return PromptUser(promptText, null);
        }

        internal static string PromptUser(string promptText, char? displayChar)
        {
            Console.Write(promptText + " ");
            StringBuilder builder = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey(true);
                if (info.Key == ConsoleKey.Enter)
                {
                    return builder.ToString();
                }
                builder.Append(info.KeyChar);
                if (displayChar.HasValue)
                {
                    Console.Write(displayChar);
                }
                else
                {
                    Console.Write(info.KeyChar);
                }
            }
        }

        internal static void ResizeConsoleInputBuffer(int newSize)
        {
            Stream stream = Console.OpenStandardInput(newSize);
            if (stream == Stream.Null)
            {
                throw new Exception("Unable to resize console.");
            }
            Encoding encoding = Encoding.GetEncoding((int) GetConsoleCP());
            Console.SetIn(TextReader.Synchronized(new StreamReader(stream, encoding, false, newSize)));
        }
    }
}

