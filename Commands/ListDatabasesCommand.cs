using System;
using System.Collections.Generic;
using System.Data;

namespace SqlUtils.Commands
{
    internal class ListDatabasesCommand : IEngineCommand, ICommand
    {
        void IEngineCommand.Execute(EngineCommandContext ctx)
        {
            ExecuteDirect(ctx);
        }

        internal static void ExecuteDirect(EngineCommandContext ctx)
        {
            IDbConnection connection = ctx.ConnectionManager.BuildMasterConnection();
            connection.Open();
            try
            {
                List<DatabaseInfo> list = DataUtil.BuildDatabaseInfoList(ctx.ConnectionManager, connection);
                if (list != null)
                {
                    int num = 1;
                    foreach (DatabaseInfo info in list)
                    {
                        Console.WriteLine(num + ". " + info.Name);
                        num++;
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

