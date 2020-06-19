using System;
using System.Data;
using System.Data.SqlClient;
using SqlUtils.Formatting;

namespace SqlUtils.Commands
{
    internal class ListChildInstancesCommand : IEngineCommand, ICommand
    {
        void IEngineCommand.Execute(EngineCommandContext ctx)
        {
            this.ExecuteDirect(ctx);
        }

        internal void ExecuteDirect(EngineCommandContext ctx)
        {
            IDbConnection connection = ctx.ConnectionManager.BuildMasterConnection(true);
            string str = "SELECT owning_principal_name AS [User], instance_pipe_name AS [Pipe], OS_process_id AS [ProcessId], heart_beat AS [Status] FROM sys.dm_os_child_instances";
            IDbCommand command = DataUtil.CreateCommand(ctx.ConnectionManager, connection);
            command.CommandText = str;
            connection.Open();
            try
            {
                SqlDataReader dataReader = (SqlDataReader) command.ExecuteReader();
                if (dataReader != null)
                {
                    try
                    {
                        if (dataReader.HasRows)
                        {
                            ITableFormatter formatter = TableFormatterBuilder.BuildTableFormatter(dataReader);
                            Console.WriteLine("");
                            Console.WriteLine(formatter.ReadHeader());
                            Console.WriteLine(new string('-', formatter.TotalColumnWidth));
                            string str2 = null;
                            while ((str2 = formatter.ReadNextRow()) != null)
                            {
                                Console.WriteLine(str2);
                            }
                        }
                        else
                        {
                            Console.WriteLine("No child instance found.");
                        }
                    }
                    finally
                    {
                        dataReader.Close();
                    }
                }
            }
            finally
            {
                connection.Close();
            }
            Console.WriteLine("");
        }
    }
}

