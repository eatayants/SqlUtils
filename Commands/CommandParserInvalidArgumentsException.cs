namespace SqlUtils
{
    internal class CommandParserInvalidArgumentsException : CommandParserException
    {
        internal CommandParserInvalidArgumentsException(string commandName) : base(string.Format("Invalid arguments for command '{0}'.", commandName))
        {
        }
    }
}

