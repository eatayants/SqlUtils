using System;
using System.Data;
using System.IO;

namespace SqlUtils.Commands
{
    internal class AttachDatabaseCommand : IEngineCommand, ICommand
    {
        private string _dbName;
        private string _filePath;

        internal AttachDatabaseCommand(string filePath, string dbName)
        {
            this._dbName = dbName;
            this._filePath = filePath;
        }

        void IEngineCommand.Execute(EngineCommandContext ctx)
        {
            ExecuteDirect(ctx, this._filePath, this._dbName);
        }

        internal static void ExecuteDirect(EngineCommandContext ctx, string filePath)
        {
            ExecuteDirect(ctx, filePath, null);
        }

        internal static void ExecuteDirect(EngineCommandContext ctx, string filePath, string dbName)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception("Attach requires the path of the data file to attach.");
            }
            try
            {
                filePath = Path.GetFullPath(filePath);
            }
            catch
            {
                throw new InvalidPathException();
            }
            if (!File.Exists(filePath))
            {
                MissingFileException.Throw(Path.GetFileName(filePath));
            }
            if (!string.IsNullOrEmpty(dbName))
            {
                DatabaseIdentifier identifier = DatabaseIdentifier.Parse(dbName);
                if (identifier.IsDatabaseName)
                {
                    dbName = identifier.Value;
                }
            }
            IDbConnection connection = ctx.ConnectionManager.BuildAttachConnection(filePath, dbName);
            connection.Open();
            connection.Close();
            Console.WriteLine("Command completed successfully.");
        }
    }
}

