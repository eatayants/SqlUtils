using System;

namespace SqlUtils.Commands
{
    internal class ListServersCommand : IEngineCommand, ICommand
    {
        private readonly bool _listRemote;

        internal ListServersCommand(bool listRemote)
        {
            _listRemote = listRemote;
        }

        void IEngineCommand.Execute(EngineCommandContext ctx)
        {
            ExecuteDirect(ctx, this._listRemote);
        }

        internal static void ExecuteDirect(EngineCommandContext ctx, bool listRemote)
        {
            string[] remoteInstances;
            if (listRemote)
            {
                Console.WriteLine("Gathering list of remote servers...");
                try
                {
                    remoteInstances = SqlServerEnumerator.GetRemoteInstances();
                }
                catch
                {
                    Console.WriteLine("Failed to retrieve the remote servers.");
                    return;
                }
            }
            else
            {
                try
                {
                    remoteInstances = SqlServerEnumerator.GetLocalInstances();
                }
                catch
                {
                    Console.WriteLine("Failed to retrieve local instances.");
                    return;
                }
            }
            Console.WriteLine("");
            if ((remoteInstances != null) && (remoteInstances.Length > 0))
            {
                foreach (string str in remoteInstances)
                {
                    Console.WriteLine(str);
                }
            }
            else
            {
                Console.WriteLine("No SQL Server instance found.");
            }
        }
    }
}

