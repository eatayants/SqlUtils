using System;

namespace SqlUtils
{
    internal class InternalException : Exception
    {
        private string _internalMessage;

        internal InternalException(string message) : base("Internal exception")
        {
            this._internalMessage = message;
        }

        internal string InternalMessage
        {
            get
            {
                return this._internalMessage;
            }
        }
    }
}

