using SqlUtils.Commands;
using SqlUtils.Scripting;

namespace ExecSqlCmd.Commands
{
    using System;
    using System.Data.SqlClient;
    using System.IO;
    internal class CreateScripCommand : IEngineCommand, ICommand
    {
        private readonly string dbName;
        private readonly string filePath;
		private readonly string prefix;
		internal CreateScripCommand(string filePath, string dbName, string prefix)
        {
            this.dbName = dbName;
            this.filePath = filePath;
			this.prefix = prefix;
        }
        void IEngineCommand.Execute(EngineCommandContext ctx)
        {
			ExecuteDirect(ctx, filePath, dbName, prefix);
        }
        internal static void ExecuteDirect(EngineCommandContext ctx, string filePath, string dbName,string prefix)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new Exception("Required to specify database name.");
            }
            if (string.IsNullOrEmpty(filePath))
            {
				throw new Exception("Required to specify filePath.");
			}
            try
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Error: " + ex.Message);
            }
            var server = ctx.ConnectionManager.ConnectionOptions.ServerName;
            if (string.IsNullOrEmpty(server))
            {
                server = "(local)";
            }
            try
            {
                var strbuild = new SqlConnectionStringBuilder();
                strbuild["Server"] = server;
                strbuild["Initial Catalog"] = dbName;
                strbuild.IntegratedSecurity = true;
                var connection = new SqlConnection(strbuild.ConnectionString);
                connection.Open();
				var Co = new ScriptObject(connection, filePath, prefix);
				Console.WriteLine(@"Scripting tables");
                Co.CreateTableScript();
				Console.WriteLine(@"Scripting constraints");
				Co.CreateConstreints();
				Console.WriteLine(@"Scripting procedures and functions");
				Co.CreateFuncAndProc();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Error: " + ex.Message);
            }
        }
    }
}
