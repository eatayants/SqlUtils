using System;

namespace SqlUtils
{
    internal class DatabaseIdentifier
    {
        private bool _isDatabaseName;
        private string _value;

        private DatabaseIdentifier()
        {
        }

        internal static DatabaseIdentifier Parse(string databaseString)
        {
            if (string.IsNullOrEmpty(databaseString))
            {
                throw new Exception("Database should not be null.");
            }
            DatabaseIdentifier identifier = new DatabaseIdentifier();
            if (databaseString.StartsWith(Settings.DatabaseNamePrefix))
            {
                identifier._value = databaseString.Substring(Settings.DatabaseNamePrefix.Length);
                identifier._isDatabaseName = true;
                return identifier;
            }
            identifier._value = databaseString;
            identifier._isDatabaseName = false;
            return identifier;
        }

        internal bool IsDatabaseName
        {
            get
            {
                return this._isDatabaseName;
            }
        }

        internal string Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

