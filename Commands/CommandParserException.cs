using System;

namespace SqlUtils
{
    internal class CommandParserException : Exception
    {
        internal CommandParserException(string message) : base(message)
        {
        }
    }
}

