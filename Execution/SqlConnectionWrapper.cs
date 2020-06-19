using System;
using System.Data;
using System.Data.SqlClient;

namespace SqlUtils
{
    internal class SqlConnectionWrapper : IDbConnection, IDisposable
    {
        private System.Data.SqlClient.SqlConnection _connection;

        internal SqlConnectionWrapper(System.Data.SqlClient.SqlConnection connection)
        {
            this._connection = connection;
            this._connection.InfoMessage += new SqlInfoMessageEventHandler(this.сonnection_InfoMessage);
        }

        private void OpenConnectionInternal(bool isNested)
        {
            try
            {
                this._connection.Open();
            }
            catch (Exception exception)
            {
                if ((isNested || (this._connection == null)) || (!(exception is SqlException) || !SqlConnectionFailureDiagnostics.ProcessException(this._connection, exception as SqlException)))
                {
                    throw;
                }
                this.OpenConnectionInternal(true);
            }
        }

        IDbTransaction IDbConnection.BeginTransaction()
        {
            return this._connection.BeginTransaction();
        }

        IDbTransaction IDbConnection.BeginTransaction(IsolationLevel il)
        {
            return this._connection.BeginTransaction(il);
        }

        void IDbConnection.ChangeDatabase(string databaseName)
        {
            this._connection.ChangeDatabase(databaseName);
        }

        void IDbConnection.Close()
        {
            this._connection.Close();
        }

        IDbCommand IDbConnection.CreateCommand()
        {
            return this._connection.CreateCommand();
        }

        void IDbConnection.Open()
        {
            this.OpenConnectionInternal(false);
        }

        void IDisposable.Dispose()
        {
            this._connection.InfoMessage -= new SqlInfoMessageEventHandler(this.сonnection_InfoMessage);
            this._connection.Dispose();
        }

        private void сonnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            foreach (SqlError error in e.Errors)
            {
                Console.WriteLine(error.Message);
            }
        }

        internal System.Data.SqlClient.SqlConnection SqlConnection
        {
            get
            {
                return this._connection;
            }
        }

        string IDbConnection.ConnectionString
        {
            get
            {
                return this._connection.ConnectionString;
            }
            set
            {
                this._connection.ConnectionString = value;
            }
        }

        int IDbConnection.ConnectionTimeout
        {
            get
            {
                return this._connection.ConnectionTimeout;
            }
        }

        string IDbConnection.Database
        {
            get
            {
                return this._connection.Database;
            }
        }

        ConnectionState IDbConnection.State
        {
            get
            {
                return this._connection.State;
            }
        }
    }
}

