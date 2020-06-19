using System;
using System.Data;
using System.IO;

namespace SqlUtils.Commands
{
    internal class IsSignedDatabaseCommand : IEngineCommand, ICommand
    {
        private string _database;
        private string _publicKeyFile;

        internal IsSignedDatabaseCommand(string database, string publicKeyFile)
        {
            this._database = database;
            this._publicKeyFile = publicKeyFile;
        }

        void IEngineCommand.Execute(EngineCommandContext ctx)
        {
            ExecuteDirect(ctx, this._database, this._publicKeyFile);
        }

        internal static void ExecuteDirect(EngineCommandContext ctx, string database, string publicKeyFile)
        {
            IDbConnection connection;
            if (string.IsNullOrEmpty(database))
            {
                throw new Exception("IsSigned requires a file path or existing database name in brackets.");
            }
            if (string.IsNullOrEmpty(publicKeyFile))
            {
                throw new Exception("IsSigned requires the path to the key file (.snk) that contains the public key.");
            }
            DatabaseIdentifier identifier = DatabaseIdentifier.Parse(database);
            database = identifier.Value;
            if (identifier.IsDatabaseName)
            {
                if (string.IsNullOrEmpty(database))
                {
                    throw new Exception("Invalid empty database name specified.");
                }
            }
            else if (!File.Exists(database))
            {
                MissingFileException.Throw(Path.GetFileName(database));
            }
            if (!Path.GetExtension(publicKeyFile).Equals(".snk", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Key file should have the .snk extension. Only asymmetric key files are supported.");
            }
            if (!File.Exists(publicKeyFile))
            {
                MissingFileException.Throw(Path.GetFileName(publicKeyFile));
            }
            publicKeyFile = PathUtil.EnsureFullPath(publicKeyFile);
            byte[] fileBytes = GetFileBytes(publicKeyFile);
            string str = "0x" + Util.FormatBytesAsHex(fileBytes);
            string str2 = "0x" + Util.FormatBytesAsHex(StrongName.CreatePublicKeyToken(fileBytes));
            Console.WriteLine("Verifying signature using the specified key file...");
            bool flag = true;
            if (identifier.IsDatabaseName)
            {
                connection = ctx.ConnectionManager.BuildMasterConnection();
            }
            else
            {
                IDbConnection connection2 = ctx.ConnectionManager.BuildMasterConnection();
                connection2.Open();
                string dbName = null;
                try
                {
                    dbName = DataUtil.IsFileAttached(ctx.ConnectionManager, connection2, database);
                }
                finally
                {
                    connection2.Close();
                }
                if (dbName == null)
                {
                    flag = true;
                }
                connection = ctx.ConnectionManager.BuildAttachConnection(database, dbName);
            }
            connection.Open();
            try
            {
                string str4 = null;
                if (flag)
                {
                    str4 = connection.Database;
                }
                IDbCommand command = DataUtil.CreateCommand(ctx.ConnectionManager, connection);
                if (identifier.IsDatabaseName)
                {
                    command.CommandText = "use [" + database + "]";
                    command.ExecuteNonQuery();
                }
                bool flag2 = false;
                command.CommandText = "SELECT count(*) FROM sys.asymmetric_keys where thumbprint=" + str2 + " AND public_key=" + str;
                object obj2 = command.ExecuteScalar();
                if ((obj2 is int) && (((int) obj2) == 1))
                {
                    command.CommandText = "SELECT count (*) FROM sys.fn_check_object_signatures('ASYMMETRIC KEY', " + str2 + ") WHERE is_signed <> 1 OR is_signature_valid <> 1";
                    obj2 = command.ExecuteScalar();
                    if ((obj2 is int) && (((int) obj2) == 0))
                    {
                        flag2 = true;
                    }
                }
                if (flag2)
                {
                    Console.WriteLine("Succeeded. The signature for all database objects matched.");
                }
                else
                {
                    Console.WriteLine("Failed. One or more object(s) failed signature validation.");
                }
                if (flag)
                {
                    try
                    {
                        DataUtil.DetachDatabase(ctx.ConnectionManager, connection, str4, true);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            finally
            {
                connection.Close();
            }
        }

        private static byte[] GetFileBytes(string filePath)
        {
            byte[] buffer = null;
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                buffer = new byte[(int) stream.Length];
                stream.Read(buffer, 0, (int) stream.Length);
            }
            return buffer;
        }
    }
}

