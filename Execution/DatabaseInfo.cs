namespace SqlUtils
{
    internal class DatabaseInfo
    {
        private string _name;
        private string _path;

        internal DatabaseInfo(string name, string path)
        {
            this._name = name;
            this._path = path;
        }

        internal string Name
        {
            get
            {
                return this._name;
            }
        }

        internal string Path
        {
            get
            {
                return this._path;
            }
        }
    }
}

