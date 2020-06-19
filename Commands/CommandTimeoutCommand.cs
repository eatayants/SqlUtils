namespace SqlUtils
{
    internal class CommandTimeoutCommand : ConsoleCommand
    {
        private int _timeout = -1;

        internal CommandTimeoutCommand(int timeout)
        {
            this._timeout = timeout;
        }

        internal int Timeout
        {
            get
            {
                return this._timeout;
            }
        }
    }
}

