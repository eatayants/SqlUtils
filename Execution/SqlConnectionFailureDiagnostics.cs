using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using SqlUtils.Scripting;

namespace SqlUtils
{
    internal class SqlConnectionFailureDiagnostics
    {
        private static ServiceStatusError CheckServiceError(string serviceName)
        {
            try
            {
                ServiceController controller = new ServiceController(serviceName);
                if (controller.Status != ServiceControllerStatus.Running)
                {
                    return ServiceStatusError.Notstarted;
                }
                return ServiceStatusError.None;
            }
            catch (InvalidOperationException exception)
            {
                Win32Exception innerException = exception.InnerException as Win32Exception;
                if (innerException.NativeErrorCode == 0x424)
                {
                    return ServiceStatusError.Notinstalled;
                }
                return ServiceStatusError.Other;
            }
            catch
            {
                Debugger.Break();
                return ServiceStatusError.Other;
            }
        }

        internal static bool ProcessException(SqlConnection connection, SqlException ex)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connection.ConnectionString);
            SqlDataSourceInformation information = SqlDataSourceInformation.Parse(builder.DataSource);
            if ((!string.IsNullOrEmpty(information.ServerName) && information.VerifyIsLocal()) && !string.IsNullOrEmpty(information.InstanceName))
            {
                string serviceName = "MSSQL$" + information.InstanceName;
                ServiceStatusError error = CheckServiceError(serviceName);
                string str2 = null;
                switch (error)
                {
                    case ServiceStatusError.Notinstalled:
                        str2 = "The service '" + serviceName + "' is not installed.";
                        break;

                    case ServiceStatusError.Notstarted:
                    {
                        str2 = "The service '" + serviceName + "' is not started.";
                        string str3 = "";
                        Console.WriteLine(str2);
                        if (!ScriptEngine.IsExecutingScript)
                        {
                            while (!str3.Equals("y", StringComparison.OrdinalIgnoreCase) && !str3.Equals("n", StringComparison.OrdinalIgnoreCase))
                            {
                                str3 = ConsoleUtil.PromptUser("Would you like to start the service (y/n)?");
                                Console.WriteLine("");
                                if (str3.Equals("y", StringComparison.OrdinalIgnoreCase))
                                {
                                    ServiceController controller = new ServiceController(serviceName);
                                    Console.Write("Starting service...");
                                    try
                                    {
                                        controller.Start();
                                        controller.WaitForStatus(ServiceControllerStatus.Running);
                                        Thread.Sleep(0x1388);
                                    }
                                    catch
                                    {
                                        Console.WriteLine("");
                                    }
                                    if (controller.Status == ServiceControllerStatus.Running)
                                    {
                                        Console.WriteLine(" Done.");
                                        return true;
                                    }
                                    Console.WriteLine(" Failed.");
                                    return false;
                                }
                            }
                        }
                        break;
                    }
                }
                if (str2 != null)
                {
                    throw new Exception(ex.Message + Environment.NewLine + Environment.NewLine + "Additional information:" + Environment.NewLine + str2, ex);
                }
            }
            throw ex;
        }

        private enum ServiceStatusError
        {
            None,
            Notinstalled,
            Notstarted,
            Other
        }
    }
}

