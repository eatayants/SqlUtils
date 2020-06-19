using System;
using System.Data;
using System.IO;

namespace SqlUtils.Commands
{
    internal class CreateDatabaseCommand : IEngineCommand, ICommand
    {
        private string _databaseName;
        private string _filePath;

        internal CreateDatabaseCommand(string filePath, string databaseName)
        {
            this._filePath = filePath;
            this._databaseName = databaseName;
        }

        void IEngineCommand.Execute(EngineCommandContext ctx)
        {
            ExecuteDirect(ctx, this._filePath, this._databaseName);
        }

        internal static void ExecuteDirect(EngineCommandContext ctx, string filePath, string databaseName)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception("Create requires a file path or existing database name in brackets.");
            }
            bool flag = false;
            DatabaseIdentifier identifier = DatabaseIdentifier.Parse(filePath);
            if (identifier.IsDatabaseName)
            {
                if (!string.IsNullOrEmpty(databaseName))
                {
                    throw new Exception("Database name was already specified within brackets. Should not specify database name again.");
                }
                databaseName = identifier.Value;
                if (string.IsNullOrEmpty(databaseName))
                {
                    throw new Exception("Invalid empty database name specified.");
                }
            }
            else
            {
                filePath = identifier.Value;
                try
                {
                    filePath = Path.GetFullPath(filePath);
                }
                catch
                {
                    throw new InvalidPathException();
                }
                if (string.IsNullOrEmpty(databaseName))
                {
                    flag = true;
                    databaseName = Path.GetFileNameWithoutExtension(filePath);
                    if (string.IsNullOrEmpty(databaseName))
                    {
                        throw new Exception("Invalid database path specified.");
                    }
                }
            }
            IDbConnection connection = ctx.ConnectionManager.BuildMasterConnection();
            connection.Open();
            try
            {
                IDbCommand command = DataUtil.CreateCommand(ctx.ConnectionManager, connection);
                string str = DataUtil.QuoteDbObjectName(databaseName);
                if (identifier.IsDatabaseName)
                {
                    command.CommandText = "CREATE DATABASE " + str;
                }
                else
                {
                    command.CommandText = "CREATE DATABASE " + str + " ON (NAME='" + str + "', FILENAME='" + filePath + "')";
                }
                command.ExecuteNonQuery();
                if (flag)
                {
                    DataUtil.DetachDatabase(ctx.ConnectionManager, connection, databaseName, true);
                }
            }
            finally
            {
                connection.Close();
            }
            Console.WriteLine("Command completed successfully.");
        }
    }
}

