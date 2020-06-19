namespace SqlUtils
{
    internal class ConnectionOptions
    {
        internal int CloseTimeout = 0x3e8;
        internal int CommandTimeout = 20;
        internal int ConnectionTimeout = 20;
        internal string Password;
        internal bool PromptForPassword;
        internal string RunningAs;
        internal string ServerName;
        internal bool UseMainInstance = ProgramInfo.InSqlMode;
        internal string UserName;
    }
}

