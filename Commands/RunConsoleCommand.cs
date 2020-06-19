namespace SqlUtils.Commands
{
    internal class RunConsoleCommand : IEngineCommand, ICommand
    {
        void IEngineCommand.Execute(EngineCommandContext ctx)
        {
            this.ExecuteDirect(ctx);
        }

        internal void ExecuteDirect(EngineCommandContext ctx)
        {
            new SqlConsole(ctx.Engine).Run();
        }
    }
}

