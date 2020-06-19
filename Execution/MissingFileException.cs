using System;

namespace SqlUtils
{
    internal class MissingFileException : Exception
    {
        internal MissingFileException(string message) : base(message)
        {
        }

        internal static void Throw(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new MissingFileException("File could not be found.");
            }
            throw new MissingFileException(string.Format("File '{0}' could not be found.", fileName));
        }
    }
}

