namespace SqlUtils
{
    internal class OpenLogCommand : ConsoleCommand
    {
        private string _filePath;

        internal OpenLogCommand(string filePath)
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

