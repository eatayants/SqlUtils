using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using Microsoft.Win32;

namespace SqlUtils
{
    internal class SqlServerEnumerator
    {
        internal static string[] GetLocalInstances()
        {
            RegistryKey key = null;
            string[] strArray;
            try
            {
                key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Microsoft SQL Server", false);
            }
            catch
            {
            }
            if (key == null)
            {
                throw new Exception(@"Could not open key registry key 'Software\Microsoft\Microsoft SQL Server'");
            }
            try
            {
                strArray = (string[]) key.GetValue("InstalledInstances");
            }
            catch
            {
                throw new Exception("Failed to get the values under InstalledInstances.");
            }
            if ((strArray != null) && (strArray.Length > 0))
            {
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (string.Compare(strArray[i], "MSSQLSERVER", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        strArray[i] = Environment.MachineName;
                    }
                    else
                    {
                        strArray[i] = Environment.MachineName + @"\" + strArray[i];
                    }
                }
            }
            return strArray;
        }

        internal static string[] GetRemoteInstances()
        {
            DataTable dataSources = SqlDataSourceEnumerator.Instance.GetDataSources();
            if ((dataSources == null) || (dataSources.Rows.Count == 0))
            {
                return null;
            }
            if ((dataSources.Columns["ServerName"] == null) || (dataSources.Columns["InstanceName"] == null))
            {
                throw new Exception("The underlying SQL Server enumerator did not return the information.");
            }
            List<string> list = new List<string>();
            foreach (DataRow row in dataSources.Rows)
            {
                string item = row["ServerName"] as string;
                if (item != null)
                {
                    string str2 = row["InstanceName"] as string;
                    if (!string.IsNullOrEmpty(str2))
                    {
                        item = item + @"\" + str2;
                    }
                    list.Add(item);
                }
            }
            list.Sort();
            return list.ToArray();
        }
    }
}

