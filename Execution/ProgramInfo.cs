using System;
using System.IO;
using System.Reflection;

namespace SqlUtils
{
    internal class ProgramInfo
    {
        private static string _executableName = null;
        private static string _executablePath = null;
        private static bool? _inSqlMode = null;

        internal static string ExecutableName
        {
            get
            {
                if (_executableName == null)
                {
                    _executableName = Path.GetFileNameWithoutExtension(ExecutablePath);
                }
                return _executableName;
            }
        }

        internal static string ExecutablePath
        {
            get
            {
                if (_executablePath == null)
                {
                    _executablePath = Assembly.GetEntryAssembly().Location;
                }
                return _executablePath;
            }
        }

        internal static bool InSqlMode
        {
            get
            {
                if (!_inSqlMode.HasValue)
                {
                    _inSqlMode = new bool?(string.Equals(ExecutableName, "SQLUtil", StringComparison.OrdinalIgnoreCase));
                }
                return _inSqlMode.Value;
            }
        }
    }
}

