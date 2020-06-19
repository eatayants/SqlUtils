using System;
using System.Collections.Generic;
using System.Data;

namespace SqlUtils.Commands
{
    internal class DetachDatabasesCommand : IEngineCommand, ICommand
    {
        private string _database;

        internal DetachDatabasesCommand(string database)
        {
            this._database = database;
        }

        void IEngineCommand.Execute(EngineCommandContext ctx)
        {
            ExecuteDirect(ctx, this._database);
        }

        internal static void ExecuteDirect(EngineCommandContext ctx, string database)
        {
            if (string.IsNullOrEmpty(database))
            {
                throw new Exception("Detach requires a file path or database name (within brackets).");
            }
            bool flag = false;
            DatabaseIdentifier identifier = DatabaseIdentifier.Parse(database);
            database = identifier.Value;
            if (identifier.IsDatabaseName)
            {
                if (string.IsNullOrEmpty(database))
                {
                    throw new Exception("Invalid database name specified.");
                }
            }
            else
            {
                if (database.EndsWith("*"))
                {
                    database = database.Substring(0, database.Length - 1);
                    flag = true;
                }
                database = PathUtil.EnsureFullPath(database);
            }
            IDbConnection connection = ctx.ConnectionManager.BuildMasterConnection();
            connection.Open();
            try
            {
                List<DatabaseInfo> list = DataUtil.BuildDatabaseInfoList(ctx.ConnectionManager, connection);
                if (list != null)
                {
                    bool flag2 = false;
                    foreach (DatabaseInfo info in list)
                    {
                        bool flag3 = false;
                        try
                        {
                            if (identifier.IsDatabaseName)
                            {
                                if (string.Equals(database, info.Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    flag3 = true;
                                }
                            }
                            else
                            {
                                string b = PathUtil.EnsureFullPath(info.Path);
                                if (flag)
                                {
                                    if (b.StartsWith(database, StringComparison.OrdinalIgnoreCase))
                                    {
                                        flag3 = true;
                                    }
                                }
                                else if (string.Equals(database, b, StringComparison.OrdinalIgnoreCase))
                                {
                                    flag3 = true;
                                }
                            }
                            if (flag3 && (((string.Compare(info.Name, "master", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(info.Name, "tempdb", StringComparison.OrdinalIgnoreCase) == 0)) || ((string.Compare(info.Name, "model", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(info.Name, "msdb", StringComparison.OrdinalIgnoreCase) == 0))))
                            {
                                Console.WriteLine("Warning: Not detaching system database '{0}'. Use SQL commands in the console to do it.", info.Name);
                                flag3 = false;
                            }
                        }
                        catch
                        {
                        }
                        if (flag3)
                        {
                            DataUtil.DetachDatabase(ctx.ConnectionManager, connection, info.Name, false);
                            flag2 = true;
                        }
                    }
                    if (!flag2)
                    {
                        if (identifier.IsDatabaseName)
                        {
                            Console.WriteLine("No valid database name matches the value specified.");
                        }
                        else
                        {
                            Console.WriteLine("No valid database path matches the value specified.");
                        }
                    }
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }
}

