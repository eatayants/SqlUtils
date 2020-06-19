using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using SqlUtils.Formatting;

namespace SqlUtils
{
    internal class DataUtil
    {
        internal const int ConnectionDefaultTimeout = 20;
        private const string CreateMdfSql = "DECLARE @databaseName sysname\nSET @databaseName = CONVERT(sysname, NEWID())\nWHILE EXISTS (SELECT name FROM sys.databases WHERE name = @databaseName)\nBEGIN\n\tSET @databaseName = CONVERT(sysname, NEWID())\nEND\nSET @databaseName = '[' + @databaseName + ']'\nDECLARE @sqlString nvarchar(MAX)\nSET @sqlString = 'CREATE DATABASE ' + @databaseName + N' ON ( NAME = [{0}], FILENAME = N''{1}'')'\nEXEC sp_executesql @sqlString\nSET @sqlString = 'ALTER DATABASE ' + @databaseName + ' SET AUTO_SHRINK ON'\nEXEC sp_executesql @sqlString\nSET @sqlString = 'ALTER DATABASE ' + @databaseName + ' SET OFFLINE WITH ROLLBACK IMMEDIATE'\nEXEC sp_executesql @sqlString\nSET @sqlString = 'EXEC sp_detach_db ' + @databaseName\nEXEC sp_executesql @sqlString";
        internal const string DefaultServerName = @".\SQLExpress";
        private const string UnlockDatabaseSql = "USE master\nIF EXISTS (SELECT * FROM sysdatabases WHERE name = N'{0}')\nBEGIN\n\tALTER DATABASE [{1}] SET OFFLINE WITH ROLLBACK IMMEDIATE\n\tEXEC sp_detach_db [{1}]\nEND";

        internal static string BuildConnectionString(string serverName, bool useMainInstance, int timeout)
        {
            return BuildConnectionString(serverName, null, null, null, useMainInstance, timeout);
        }

        internal static string BuildConnectionString(string serverName, string filePath, bool useMainInstance, int timeout)
        {
            return BuildConnectionString(serverName, null, null, filePath, useMainInstance, timeout);
        }

        internal static string BuildConnectionString(string serverName, string userName, string password, bool useMainInstance, int timeout)
        {
            return BuildConnectionString(serverName, userName, password, null, useMainInstance, timeout);
        }

        internal static string BuildConnectionString(string serverName, string userName, string password, string filePath, bool useMainInstance, int timeout)
        {
            if (string.IsNullOrWhiteSpace(serverName))
            {
                serverName = @".\SQLExpress";
            }
            string str = "Data Source=" + serverName + ";";
            if (userName == null)
            {
                str = str + "Integrated Security = SSPI;";
            }
            else
            {
                string str2 = str;
                str = str2 + "user=" + userName + ";password=" + password + ";";
            }
            if ((filePath != null) && (filePath.Length > 0))
            {
                if (!Path.IsPathRooted(filePath))
                {
                    filePath = PathUtil.EnsureFullPath(filePath);
                }
                if (!File.Exists(filePath))
                {
                    throw new Exception("File '" + Path.GetFileName(filePath) + "' could not be found.");
                }
                str = str + "AttachDbFileName=\"" + filePath + "\";";
            }
            if (!useMainInstance)
            {
                str = str + "User Instance=true;";
            }
            object obj2 = str;
            return string.Concat(new object[] { obj2, "Timeout=", timeout, ";" });
        }

        internal static List<DatabaseInfo> BuildDatabaseInfoList(ConnectionManager connectionManager, IDbConnection connection)
        {
            List<DatabaseInfo> list = new List<DatabaseInfo>();
            IDbCommand command = CreateCommand(connectionManager, connection);
            command.CommandText = "SELECT * FROM SYSDATABASES";
            try
            {
                var reader = command.ExecuteReader();
                try
                {
                    string str;
                    if (!((SqlDataReader) reader).HasRows || (reader.FieldCount <= 0))
                    {
                        return list;
                    }
                    int ordinal = reader.GetOrdinal("name");
                    int i = reader.GetOrdinal("filename");
                    if ((ordinal != -1) && (i != -1))
                    {
                        goto Label_0096;
                    }
                    throw new Exception("SYSDATABASE didn't adhere to the expected schema.");
                    Label_0075:
                    str = reader.GetString(ordinal);
                    string path = reader.GetString(i);
                    list.Add(new DatabaseInfo(str, path));
                    Label_0096:
                    if (reader.Read())
                    {
                        goto Label_0075;
                    }
                    return list;
                }
                finally
                {
                    try
                    {
                        reader.Close();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            catch
            {
                connection.Close();
            }
            return list;
        }

        internal static string BuildOleDbConnectionString(string serverName, string userName, string password)
        {
            if ((serverName == null) || (serverName.Length == 0))
            {
                serverName = @".\SQLExpress";
            }
            string str = "Provider=SQLOLEDB;Data Source=" + serverName + ";";
            if (userName == null)
            {
                return (str + "Integrated Security = SSPI;");
            }
            string str2 = str;
            return (str2 + "User ID=" + userName + ";Password=" + password + ";");
        }

        internal static IDbCommand CreateCommand(ConnectionManager connectionManager, IDbConnection connection)
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandTimeout = connectionManager.ConnectionOptions.CommandTimeout;
            return command;
        }

        internal static void CreateDatabaseFile(ConnectionManager connectionManager, string filePath)
        {
            IDbConnection connection = connectionManager.BuildMasterConnection();
            connection.Open();
            try
            {
                string str = Path.GetFileNameWithoutExtension(filePath).Replace("]", "]]").Replace("'", "''");
                filePath = Path.GetFullPath(filePath).Replace("'", "''''");
                try
                {
                    IDbCommand command = CreateCommand(connectionManager, connection);
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, "DECLARE @databaseName sysname\nSET @databaseName = CONVERT(sysname, NEWID())\nWHILE EXISTS (SELECT name FROM sys.databases WHERE name = @databaseName)\nBEGIN\n\tSET @databaseName = CONVERT(sysname, NEWID())\nEND\nSET @databaseName = '[' + @databaseName + ']'\nDECLARE @sqlString nvarchar(MAX)\nSET @sqlString = 'CREATE DATABASE ' + @databaseName + N' ON ( NAME = [{0}], FILENAME = N''{1}'')'\nEXEC sp_executesql @sqlString\nSET @sqlString = 'ALTER DATABASE ' + @databaseName + ' SET AUTO_SHRINK ON'\nEXEC sp_executesql @sqlString\nSET @sqlString = 'ALTER DATABASE ' + @databaseName + ' SET OFFLINE WITH ROLLBACK IMMEDIATE'\nEXEC sp_executesql @sqlString\nSET @sqlString = 'EXEC sp_detach_db ' + @databaseName\nEXEC sp_executesql @sqlString", new object[] { str, filePath });
                    command.ExecuteNonQuery();
                }
                catch (Exception exception)
                {
                    throw new Exception("An error occurred while creating a new DB file: " + Environment.NewLine + exception.ToString());
                }
            }
            finally
            {
                connection.Close();
            }
        }

        internal static void DetachDatabase(ConnectionManager connectionManager, IDbConnection connection, string dbName, bool silent)
        {
            IDbCommand command = CreateCommand(connectionManager, connection);
            command.CommandType = CommandType.Text;
            string str = dbName.Replace("]", "]]").Replace("'", "''");
            command.CommandText = string.Format("USE master\nIF EXISTS (SELECT * FROM sysdatabases WHERE name = N'{0}')\nBEGIN\n\tALTER DATABASE [{1}] SET OFFLINE WITH ROLLBACK IMMEDIATE\n\tEXEC sp_detach_db [{1}]\nEND", dbName, str);
            try
            {
                command.ExecuteNonQuery();
                if (!silent)
                {
                    Console.WriteLine("Detached '" + dbName + "' successfully.");
                }
            }
            catch (SqlException exception)
            {
                if (!exception.Message.StartsWith("Unable to open the physical file", StringComparison.OrdinalIgnoreCase))
                {
                    if (!silent)
                    {
                        Console.WriteLine("Failed to detach '" + dbName + "'");
                    }
                }
                else if (!silent)
                {
                    Console.WriteLine("Detached '" + dbName + "' successfully.");
                }
            }
            catch
            {
            }
        }

        internal static void DisplayException(Exception ex)
        {
            string message = null;
            SqlException exception = ex as SqlException;
            if (exception != null)
            {
                if (exception.Errors.Count == 0)
                {
                    if (exception.Message != null)
                    {
                        message = message + exception.Message;
                    }
                }
                else
                {
                    int num = 1;
                    foreach (SqlError error in exception.Errors)
                    {
                        if (error.Message != null)
                        {
                            object obj2 = message + error.Message + Environment.NewLine;
                            message = string.Concat(new object[] { obj2, "[SqlException Number ", error.Number, ", Class ", error.Class, ", State ", error.State, ", Line ", error.LineNumber, "]" }) + Environment.NewLine;
                            num++;
                        }
                    }
                }
            }
            else
            {
                message = ex.Message;
            }
            if (message == null)
            {
                message = "Exception occurred. No information provided.";
            }
            Console.WriteLine(message);
        }

        internal static int FetchAndDisplayRows(IDataReader reader, SqlConsole.ProcessingOption option, out DataTable table, out object scalar)
        {
            DataTable dataTable = new DataTable();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                dataTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }
            dataTable.BeginLoadData();
            object[] values = new object[reader.FieldCount];
            while (reader.Read())
            {
                reader.GetValues(values);
                dataTable.LoadDataRow(values, true);
            }
            dataTable.EndLoadData();
            ITableFormatter formatter = TableFormatterBuilder.BuildTableFormatter(dataTable);
            Console.WriteLine(formatter.ReadHeader());
            Console.WriteLine("");
            Console.WriteLine(new string('-', formatter.TotalColumnWidth));
            int num2 = 0;
            string str = null;
            while ((str = formatter.ReadNextRow()) != null)
            {
                Console.WriteLine(str);
                Console.WriteLine("");
                num2++;
            }
            scalar = null;
            table = null;
            if (option == SqlConsole.ProcessingOption.Scalar)
            {
                if ((dataTable.Rows.Count > 0) && (dataTable.Columns.Count > 0))
                {
                    scalar = dataTable.Rows[0][0];
                }
                return num2;
            }
            if (option == SqlConsole.ProcessingOption.Table)
            {
                table = dataTable;
            }
            return num2;
        }

        internal static List<DatabaseFileInfo> GetDatabaseFiles(ConnectionManager connectionManager, IDbConnection connection)
        {
            IDbCommand command = CreateCommand(connectionManager, connection);
            command.CommandText = "sp_helpfile";
            List<DatabaseFileInfo> list = new List<DatabaseFileInfo>();
            SqlDataReader reader = (SqlDataReader) command.ExecuteReader();
            if (reader != null)
            {
                try
                {
                    string str;
                    int ordinal = reader.GetOrdinal("name");
                    int num2 = reader.GetOrdinal("filename");
                    int num3 = reader.GetOrdinal("size");
                    if (reader.HasRows && (num2 >= 0))
                    {
                        goto Label_0094;
                    }
                    throw new Exception("Could not get file information from the server for the given database.");
                Label_0067:
                    str = reader.GetString(ordinal);
                    string filePath = reader.GetString(num2);
                    string size = reader.GetString(num3);
                    list.Add(new DatabaseFileInfo(str, filePath, size));
                Label_0094:
                    if (reader.Read())
                    {
                        goto Label_0067;
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return list;
        }

        internal static string GetLogFilePath(string dataFilePath)
        {
            return Path.Combine(Path.GetDirectoryName(dataFilePath), Path.GetFileNameWithoutExtension(dataFilePath) + "_log.ldf");
        }

        internal static string IsFileAttached(ConnectionManager connectionManager, IDbConnection connection, string filePath)
        {
            filePath = PathUtil.EnsureFullPath(filePath);
            List<DatabaseInfo> list = BuildDatabaseInfoList(connectionManager, connection);
            if ((list != null) && (list.Count > 0))
            {
                foreach (DatabaseInfo info in list)
                {
                    string b = PathUtil.EnsureFullPath(info.Path);
                    if (string.Equals(filePath, b, StringComparison.OrdinalIgnoreCase))
                    {
                        return info.Name;
                    }
                }
            }
            return null;
        }

        internal static string QuoteDbObjectName(string dbObjectName)
        {
            if (string.IsNullOrEmpty(dbObjectName))
            {
                return dbObjectName;
            }
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            int startIndex = 0;
            int index = -1;
            do
            {
                index = dbObjectName.IndexOf(']');
                if (index != -1)
                {
                    builder.Append(dbObjectName.Substring(startIndex, index - startIndex));
                    builder.Append("]]");
                    startIndex = index + 1;
                }
            }
            while ((startIndex < dbObjectName.Length) && (index != -1));
            if (startIndex < dbObjectName.Length)
            {
                builder.Append(dbObjectName.Substring(startIndex, dbObjectName.Length - startIndex));
            }
            builder.Append("]");
            return builder.ToString();
        }

        internal class DatabaseFileInfo
        {
            internal string FilePath;
            internal string Name;
            internal string Size;

            internal DatabaseFileInfo(string name, string filePath, string size)
            {
                this.Name = name;
                this.FilePath = filePath;
                this.Size = size;
            }
        }
    }
}

