using System.Collections.Generic;

namespace SqlUtils.Scripting
{
    internal class CommandBlock : Block
    {
        private List<string> _commands = new List<string>();

        internal List<string> Commands
        {
            get
            {
                return this._commands;
            }
        }
    }
}

