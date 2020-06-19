using System;

namespace SqlUtils
{
    internal class SqlDataSourceInformation
    {
        private string _instanceName;
        private int _portNumber = -1;
        private string _serverName;

        internal static SqlDataSourceInformation Parse(string dataSource)
        {
            if (string.IsNullOrEmpty(dataSource))
            {
                throw new Exception("Invalid empty datasource name.");
            }
            SqlDataSourceInformation information = new SqlDataSourceInformation();
            try
            {
                int index = dataSource.IndexOf(',');
                if (index != -1)
                {
                    information._portNumber = int.Parse(dataSource.Substring(index + 1));
                    dataSource = dataSource.Substring(0, index);
                }
                int length = dataSource.IndexOf('\\');
                if (length > -1)
                {
                    information._instanceName = dataSource.Substring(length + 1);
                    dataSource = dataSource.Substring(0, length);
                }
            }
            catch
            {
                throw new Exception("The datasource name was not in the format expected.");
            }
            information._serverName = dataSource;
            return information;
        }

        internal bool VerifyIsLocal()
        {
            if (((!string.IsNullOrEmpty(this._serverName) && !this._serverName.Equals(".", StringComparison.OrdinalIgnoreCase)) && (!this._serverName.Equals("(local)", StringComparison.OrdinalIgnoreCase) && !this._serverName.Equals("localhost", StringComparison.OrdinalIgnoreCase))) && (!this._serverName.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase) && !this._serverName.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            return true;
        }

        internal string InstanceName
        {
            get
            {
                return this._instanceName;
            }
        }

        internal int PortNumber
        {
            get
            {
                return this._portNumber;
            }
        }

        internal string ServerName
        {
            get
            {
                return this._serverName;
            }
        }
    }
}

