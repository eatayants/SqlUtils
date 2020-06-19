namespace SqlUtils.Commands
{
    internal interface IEngineCommand : ICommand
    {
        void Execute(EngineCommandContext ctx);
    }
}

