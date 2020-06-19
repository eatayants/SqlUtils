using System;
using System.Data;

namespace SqlUtils.Commands
{
    internal class ShowVersionCommand : IEngineCommand, ICommand
    {
        void IEngineCommand.Execute(EngineCommandContext ctx)
        {
            this.ExecuteDirect(ctx);
        }

        internal void ExecuteDirect(EngineCommandContext ctx)
        {
            IDbConnection connection = ctx.ConnectionManager.BuildMasterConnection();
            connection.Open();
            try
            {
                IDbCommand command = DataUtil.CreateCommand(ctx.ConnectionManager, connection);
                command.CommandText = "SELECT @@VERSION";
                Console.WriteLine(command.ExecuteScalar().ToString());
            }
            finally
            {
                connection.Close();
            }
        }
    }
}

