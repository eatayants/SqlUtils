namespace SqlUtils
{
    internal class CommandParserInvalidCommandException : CommandParserException
    {
        internal CommandParserInvalidCommandException(string commandName) : base(string.Format("Invalid command '{0}'.", commandName))
        {
        }
    }
}

