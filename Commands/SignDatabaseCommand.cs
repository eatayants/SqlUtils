using System;
using System.Data;
using System.IO;

namespace SqlUtils.Commands
{
    internal class SignDatabaseCommand : IEngineCommand, ICommand
    {
        private string _database;
        private string _keyPath;

        internal SignDatabaseCommand(string database, string keyPath)
        {
            this._keyPath = keyPath;
            this._database = database;
        }

        void IEngineCommand.Execute(EngineCommandContext ctx)
        {
            ExecuteDirect(ctx, this._database, this._keyPath);
        }

        internal static void ExecuteDirect(EngineCommandContext ctx, string database, string keyPath)
        {
            IDbConnection connection;
            if (string.IsNullOrEmpty(database))
            {
                throw new Exception("Sign requires a file path or existing database name in brackets.");
            }
            if (string.IsNullOrEmpty(keyPath))
            {
                throw new Exception("Sign requires the path to the key file (.snk) that contains the encryption key.");
            }
            keyPath = PathUtil.EnsureFullPath(keyPath);
            if (!File.Exists(keyPath))
            {
                MissingFileException.Throw(Path.GetFileName(keyPath));
            }
            string dbName = null;
            bool flag = false;
            DatabaseIdentifier identifier = DatabaseIdentifier.Parse(database);
            database = identifier.Value;
            if (identifier.IsDatabaseName)
            {
                if (string.IsNullOrEmpty(database))
                {
                    throw new Exception("Invalid empty database name specified.");
                }
                connection = ctx.ConnectionManager.BuildMasterConnection();
                connection.Open();
                try
                {
                    IDbCommand command = DataUtil.CreateCommand(ctx.ConnectionManager, connection);
                    command.CommandText = "use [" + database + "]";
                    command.ExecuteNonQuery();
                }
                catch
                {
                    connection.Close();
                    throw;
                }
                dbName = database;
            }
            else
            {
                if (!File.Exists(database))
                {
                    MissingFileException.Throw(Path.GetFileName(database));
                }
                connection = ctx.ConnectionManager.BuildMasterConnection();
                connection.Open();
                try
                {
                    dbName = DataUtil.IsFileAttached(ctx.ConnectionManager, connection, database);
                }
                finally
                {
                    connection.Close();
                }
                if (dbName == null)
                {
                    flag = true;
                }
                connection = ctx.ConnectionManager.BuildAttachConnection(database, dbName);
                connection.Open();
                dbName = connection.Database;
            }
            try
            {
                IDbCommand command2 = DataUtil.CreateCommand(ctx.ConnectionManager, connection);
                command2.CommandText = "sp_resign_database 'ASYMMETRIC KEY', '" + keyPath + "'";
                command2.ExecuteNonQuery();
                string str2 = null;
                try
                {
                    byte[] publicKey = StrongName.ExtractPublicKeyFromKeyFile(keyPath);
                    if ((publicKey == null) || (publicKey.Length == 0))
                    {
                        throw new Exception("Invalid empty public key.");
                    }
                    byte[] bytes = StrongName.CreatePublicKeyToken(publicKey);
                    if ((bytes == null) || (bytes.Length == 0))
                    {
                        throw new Exception("Invalid empty public key token.");
                    }
                    str2 = Util.FormatBytesAsHex(bytes);
                }
                catch (Exception)
                {
                }
                if (flag)
                {
                    command2.CommandText = "use master";
                    command2.ExecuteNonQuery();
                    DataUtil.DetachDatabase(ctx.ConnectionManager, connection, dbName, true);
                }
                if (str2 != null)
                {
                    Console.WriteLine("Database signed successfully with key token '" + str2 + "'.");
                }
                else
                {
                    Console.WriteLine("Database signed successfully.");
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }
}

