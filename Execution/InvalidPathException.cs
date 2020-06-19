using System;

namespace SqlUtils
{
    internal class InvalidPathException : Exception
    {
        internal InvalidPathException() : base(string.Format("Invalid path specified.", new object[0]))
        {
        }
    }
}

