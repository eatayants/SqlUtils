namespace SqlUtils
{
    internal class SaveHistoryCommand : ConsoleCommand
    {
        private string _filePath;

        internal SaveHistoryCommand(string filePath)
        {
            this._filePath = filePath;
        }

        internal string FilePath
        {
            get
            {
                return this._filePath;
            }
        }
    }
}

