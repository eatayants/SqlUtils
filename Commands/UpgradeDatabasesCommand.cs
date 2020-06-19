using System;
using System.Data;
using System.IO;
using System.Threading;

namespace SqlUtils.Commands
{
    internal class UpgradeDatabasesCommand : IEngineCommand, ICommand
    {
        private string _upgradePath;

        internal UpgradeDatabasesCommand(string upgradePath)
        {
            this._upgradePath = upgradePath;
        }

        void IEngineCommand.Execute(EngineCommandContext ctx)
        {
            this.ExecuteDirect(ctx, this._upgradePath);
        }

        internal void ExecuteDirect(EngineCommandContext ctx, string upgradePath)
        {
            if (string.IsNullOrEmpty(upgradePath))
            {
                throw new Exception("Upgrade requires the path of the data file to upgrade or a folder.");
            }
            string[] files = null;
            if (Directory.Exists(upgradePath))
            {
                files = Directory.GetFiles(upgradePath, "*.mdf");
            }
            else if (File.Exists(upgradePath) && (string.Compare(Path.GetExtension(upgradePath), ".mdf", StringComparison.OrdinalIgnoreCase) == 0))
            {
                files = new string[] { upgradePath };
            }
            if ((files == null) || (files.Length == 0))
            {
                throw new Exception("Could not data file(s) at the location specified.");
            }
            IDbConnection connection = ctx.ConnectionManager.BuildMasterConnection();
            connection.Open();
            int num = this.TryGetDatabaseVersion(connection, "master");
            connection.Close();
            foreach (string str in files)
            {
                connection.Open();
                try
                {
                    if (DataUtil.IsFileAttached(ctx.ConnectionManager, connection, str) != null)
                    {
                        Console.WriteLine("File '" + Path.GetFileName(str) + "' is already attached to the server. Skipping file.");
                        goto Label_019C;
                    }
                }
                finally
                {
                    connection.Close();
                }
                Console.Write("Processing '" + Path.GetFileName(str) + "'...");
                IDbConnection connection2 = ctx.ConnectionManager.BuildAttachConnection(str, 0);
                string databaseName = null;
                connection2.Open();
                try
                {
                    int num2;
                    databaseName = connection2.Database;
                    DateTime now = DateTime.Now;
                    if (num == -1)
                    {
                        goto Label_0170;
                    }
                Label_0123:
                    num2 = this.TryGetDatabaseVersion(connection2, databaseName);
                    if ((num2 != -1) && (num2 != num))
                    {
                        Thread.Sleep(500);
                        if ((DateTime.Now - now) < TimeSpan.FromMinutes(2.0))
                        {
                            goto Label_0123;
                        }
                    }
                }
                finally
                {
                    connection2.Close();
                }
            Label_0170:
                if (databaseName != null)
                {
                    connection.Open();
                    try
                    {
                        DataUtil.DetachDatabase(ctx.ConnectionManager, connection, databaseName, true);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
                Console.WriteLine(" Done");
            Label_019C:;
            }
        }

        private int TryGetDatabaseVersion(IDbConnection connection, string databaseName)
        {
            int num = -2147483648;
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT DatabaseProperty('" + databaseName + "', 'version')";
            try
            {
                object obj2 = command.ExecuteScalar();
                if (obj2 is DBNull)
                {
                    return -1;
                }
                num = (int) obj2;
            }
            catch
            {
            }
            return num;
        }
    }
}

