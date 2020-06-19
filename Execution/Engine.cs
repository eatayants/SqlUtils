using System;
using SqlUtils.Commands;

namespace SqlUtils
{
    internal class Engine
    {
        private ConnectionManager _connectionManager = new ConnectionManager();

        internal Engine(ConnectionOptions connectionOptions)
        {
            this._connectionManager = new ConnectionManager();
            if (!this._connectionManager.Initialize(connectionOptions))
            {
                throw new Exception("Failed to initialize connection.");
            }
        }

        internal void Execute(IEngineCommand command)
        {
            EngineCommandContext ctx = new EngineCommandContext {
                Engine = this,
                ConnectionManager = this._connectionManager
            };
            command.Execute(ctx);
        }

        internal ConnectionManager ConnectionManager
        {
            get
            {
                return this._connectionManager;
            }
        }
    }
}

