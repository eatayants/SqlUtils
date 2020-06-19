using System;
using System.Data;

namespace SqlUtils.Commands
{
    internal class EnableTracingCommand : IEngineCommand, ICommand
    {
        private bool _enable;
        private int _traceToEnable = -1;

        internal EnableTracingCommand(bool enable, int traceToEnable)
        {
            this._enable = enable;
            this._traceToEnable = traceToEnable;
        }

        void IEngineCommand.Execute(EngineCommandContext ctx)
        {
            this.ExecuteDirect(ctx, this._enable, this._traceToEnable);
        }

        internal void ExecuteDirect(EngineCommandContext ctx, bool enable, int traceToEnable)
        {
            string str;
            string str2;
            if (traceToEnable == -1)
            {
                str = "4054,-1";
            }
            else
            {
                str = traceToEnable.ToString() + ",-1";
            }
            if (enable)
            {
                str2 = "TRACEON";
            }
            else
            {
                str2 = "TRACEOFF";
            }
            IDbConnection connection = ctx.ConnectionManager.BuildMasterConnection();
            IDbCommand command = DataUtil.CreateCommand(ctx.ConnectionManager, connection);
            command.CommandText = "DBCC " + str2 + " (" + str + ")";
            connection.Open();
            try
            {
                command.ExecuteNonQuery();
                Console.Write("Trace flags {" + str + "} were set to " + enable.ToString());
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to enable trace flags -- " + exception.ToString());
            }
            finally
            {
                connection.Close();
            }
            Console.WriteLine("");
        }
    }
}

