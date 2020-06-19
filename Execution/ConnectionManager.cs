using System;
using System.Data;
using System.Data.SqlClient;

namespace SqlUtils
{
    internal class ConnectionManager
    {
        private ConnectionOptions _connectionOptions;
        private string _originalServerName;

        internal IDbConnection BuildAttachConnection(string filePath)
        {
            return this.BuildAttachConnection(filePath, (string) null);
        }

        internal IDbConnection BuildAttachConnection(string filePath, int timeout)
        {
            return new SqlConnectionWrapper(new SqlConnection(DataUtil.BuildConnectionString(this._connectionOptions.ServerName, this._connectionOptions.UserName, this._connectionOptions.Password, filePath, this._connectionOptions.UseMainInstance, timeout)));
        }

        internal IDbConnection BuildAttachConnection(string filePath, string dbName)
        {
            string connectionString = DataUtil.BuildConnectionString(this._connectionOptions.ServerName, this._connectionOptions.UserName, this._connectionOptions.Password, filePath, this._connectionOptions.UseMainInstance, this._connectionOptions.ConnectionTimeout);
            if (!string.IsNullOrEmpty(dbName))
            {
                connectionString = "Database=" + dbName + ";" + connectionString;
            }
            return new SqlConnectionWrapper(new SqlConnection(connectionString));
        }

        internal IDbConnection BuildMasterConnection()
        {
            return this.BuildMasterConnection(this._connectionOptions.UseMainInstance);
        }

        internal IDbConnection BuildMasterConnection(bool useMainInstance)
        {
            return new SqlConnectionWrapper(new SqlConnection(DataUtil.BuildConnectionString(this._connectionOptions.ServerName, this._connectionOptions.UserName, this._connectionOptions.Password, useMainInstance, this._connectionOptions.ConnectionTimeout)));
        }

        internal bool Initialize(ConnectionOptions connectionOptions)
        {
            this._connectionOptions = connectionOptions;
            if (connectionOptions.ServerName == null)
            {
                if (ProgramInfo.InSqlMode)
                {
                    string[] localInstances = null;
                    try
                    {
                        localInstances = SqlServerEnumerator.GetLocalInstances();
                    }
                    catch (Exception)
                    {
                    }
                    if ((localInstances != null) && (localInstances.Length > 0))
                    {
                        this._connectionOptions.ServerName = localInstances[0];
                    }
                    else
                    {
                        this._connectionOptions.ServerName = ".";
                    }
                }
                else
                {
                    this._connectionOptions.ServerName = @".\SQLEXPRESS";
                }
            }
            if (connectionOptions.PromptForPassword)
            {
                try
                {
                    this._connectionOptions.Password = ConsoleUtil.PromptForPassword("Enter password for user '" + connectionOptions.UserName + "': ");
                    Console.WriteLine("");
                }
                catch (UserCanceledException)
                {
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(connectionOptions.RunningAs) && !connectionOptions.UseMainInstance)
            {
                IDbConnection connection = this.BuildMasterConnection(true);
                string str = "SELECT owning_principal_name, instance_pipe_name FROM sys.dm_os_child_instances";
                IDbCommand command = DataUtil.CreateCommand(this, connection);
                command.CommandText = str;
                connection.Open();
                try
                {
                    SqlDataReader reader = (SqlDataReader) command.ExecuteReader();
                    if (reader != null)
                    {
                        try
                        {
                            if (reader.HasRows)
                            {
                                int ordinal = reader.GetOrdinal("owning_principal_name");
                                int num2 = reader.GetOrdinal("instance_pipe_name");
                                while (reader.Read())
                                {
                                    if (string.Equals(reader.GetString(ordinal), connectionOptions.RunningAs, StringComparison.OrdinalIgnoreCase))
                                    {
                                        string str2 = reader.GetString(num2);
                                        if (this._originalServerName == null)
                                        {
                                            Console.WriteLine("Using instance '" + str2 + "'.");
                                            Console.WriteLine("");
                                            this._originalServerName = connectionOptions.ServerName;
                                            this._connectionOptions.ServerName = str2;
                                            this._connectionOptions.UseMainInstance = true;
                                        }
                                        else
                                        {
                                            Console.WriteLine("Multiple child instances were found to run under the principal name you specified. The first one will be used.");
                                        }
                                    }
                                }
                            }
                            if (this._originalServerName == null)
                            {
                                throw new Exception("No child instance is running under the principal name you specified.");
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            return true;
        }

        internal ConnectionOptions ConnectionOptions
        {
            get
            {
                return this._connectionOptions;
            }
        }
    }
}

