using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using SqlUtils.Formatting;

namespace SqlUtils.Commands
{
    internal class ShrinkDatabaseCommand : IEngineCommand, ICommand
    {
        private string _database;

        internal ShrinkDatabaseCommand(string database)
        {
            this._database = database;
        }

        void IEngineCommand.Execute(EngineCommandContext ctx)
        {
            ExecuteDirect(ctx, this._database);
        }

        internal static void ExecuteDirect(EngineCommandContext ctx, string database)
        {
            IDbConnection connection;
            if (string.IsNullOrEmpty(database))
            {
                throw new Exception("Shrink requires a file path or existing database name in brackets.");
            }
            bool flag = false;
            string dbName = null;
            DatabaseIdentifier identifier = DatabaseIdentifier.Parse(database);
            database = identifier.Value;
            if (identifier.IsDatabaseName)
            {
                database = identifier.Value;
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
                database = PathUtil.EnsureFullPath(database);
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
            string[] initialSizes = null;
            List<DataUtil.DatabaseFileInfo> databaseFiles = null;
            try
            {
                databaseFiles = DataUtil.GetDatabaseFiles(ctx.ConnectionManager, connection);
                if ((databaseFiles == null) || (databaseFiles.Count == 0))
                {
                    throw new Exception("Could not get database file information from the server.");
                }
                initialSizes = new string[databaseFiles.Count];
                for (int j = 0; j < databaseFiles.Count; j++)
                {
                    initialSizes[j] = databaseFiles[j].Size;
                }
                IDbCommand command2 = DataUtil.CreateCommand(ctx.ConnectionManager, connection);
                command2.CommandText = "CHECKPOINT";
                command2.ExecuteNonQuery();
                command2.CommandText = "CHECKPOINT";
                command2.ExecuteNonQuery();
                command2.CommandText = "DBCC SHRINKDATABASE ([" + dbName + "], 10)";
                command2.ExecuteNonQuery();
            }
            finally
            {
                connection.Close();
            }
            connection = ctx.ConnectionManager.BuildMasterConnection();
            connection.Open();
            try
            {
                IDbCommand command3 = DataUtil.CreateCommand(ctx.ConnectionManager, connection);
                command3.CommandText = "use [" + dbName + "]";
                command3.ExecuteNonQuery();
                databaseFiles = DataUtil.GetDatabaseFiles(ctx.ConnectionManager, connection);
                if (flag)
                {
                    command3.CommandText = "use master";
                    command3.ExecuteNonQuery();
                    DataUtil.DetachDatabase(ctx.ConnectionManager, connection, dbName, true);
                }
            }
            finally
            {
                connection.Close();
            }
            if ((databaseFiles == null) || (databaseFiles.Count == 0))
            {
                throw new Exception("Could not get database file information from the server.");
            }
            string[] finalSizes = new string[databaseFiles.Count];
            for (int i = 0; i < databaseFiles.Count; i++)
            {
                finalSizes[i] = databaseFiles[i].Size;
            }
            PrintSizeReport(databaseFiles, initialSizes, finalSizes);
            Console.WriteLine("Command completed successfully.");
        }

        private static void PrintSizeReport(List<DataUtil.DatabaseFileInfo> databaseFiles, string[] initialSizes, string[] finalSizes)
        {
            if ((((databaseFiles != null) && (initialSizes != null)) && ((finalSizes != null) && (databaseFiles.Count == initialSizes.Length))) && (initialSizes.Length == finalSizes.Length))
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("File");
                dataTable.Columns.Add("Initial size");
                dataTable.Columns.Add("Final size");
                for (int i = 0; i < databaseFiles.Count; i++)
                {
                    string fileName = null;
                    try
                    {
                        fileName = Path.GetFileName(databaseFiles[i].FilePath);
                    }
                    catch
                    {
                    }
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = databaseFiles[i].Name;
                    }
                    dataTable.Rows.Add(new object[] { fileName, initialSizes[i], finalSizes[i] });
                }
                ITableFormatter formatter = TableFormatterBuilder.BuildTableFormatter(dataTable);
                Console.WriteLine(formatter.ReadHeader());
                Console.WriteLine("");
                Console.WriteLine(new string('-', formatter.TotalColumnWidth));
                int num2 = 0;
                string str2 = null;
                while ((str2 = formatter.ReadNextRow()) != null)
                {
                    Console.WriteLine(str2);
                    Console.WriteLine("");
                    num2++;
                }
            }
        }
    }
}

