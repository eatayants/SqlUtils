namespace SqlUtils
{
    internal class ShowHistoryCommand : ConsoleCommand
    {
        private int _commandCount;

        internal ShowHistoryCommand(int commandCount)
        {
            this._commandCount = commandCount;
        }

        internal int CommandCount
        {
            get
            {
                return this._commandCount;
            }
        }
    }
}

