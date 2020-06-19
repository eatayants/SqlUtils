using System;
using System.Collections.Generic;

namespace SqlUtils.Commands
{
    internal class RunCommandFileCommand : IEngineCommand, ICommand
    {
        private string _scriptPath;
        private Dictionary<string, string> _variables;

        internal RunCommandFileCommand(string scriptPath, Dictionary<string, string> variables)
        {
            this._scriptPath = scriptPath;
            this._variables = variables;
        }

        void IEngineCommand.Execute(EngineCommandContext ctx)
        {
            this.ExecuteDirect(ctx, this._scriptPath, this._variables);
        }

        internal void ExecuteDirect(EngineCommandContext ctx, string scriptPath, Dictionary<string, string> variables)
        {
            new SqlConsole(ctx.Engine).Run(scriptPath, variables);
        }

        internal static bool TryParseVariableDeclaration(string rawVariableDeclaration, out Dictionary<string, string> variables)
        {
            variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(rawVariableDeclaration))
            {
                string[] strArray = rawVariableDeclaration.Split(new char[] { ',' });
                if ((strArray == null) || (strArray.Length == 0))
                {
                    return false;
                }
                foreach (string str in strArray)
                {
                    string[] strArray2 = str.Split(new char[] { '=' });
                    if ((strArray2 == null) || (strArray2.Length != 2))
                    {
                        return false;
                    }
                    variables.Add(strArray2[0], strArray2[1]);
                }
            }
            return true;
        }
    }
}

