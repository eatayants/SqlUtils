using System;
using System.IO;

namespace SqlUtils.Scripting
{
    internal class ScriptDocument
    {
        private string _filePath;
        private string _rawText;
        private Block _scriptBlock;

        internal void Load(string filePath)
        {
            filePath = PathUtil.EnsureFullPath(filePath);
            if (!File.Exists(filePath))
            {
                throw new Exception("The file '" + Path.GetFileName(filePath) + "' could not be found.");
            }
            this.LoadFromText(filePath);
            this._filePath = filePath;
        }

        private void LoadFromText(string filePath)
        {
            StreamReader reader = new StreamReader(filePath);
            try
            {
                this._rawText = reader.ReadToEnd();
            }
            finally
            {
                reader.Close();
            }
            int num = 0;
            while ((num < this._rawText.Length) && char.IsWhiteSpace(this._rawText[num]))
            {
                num++;
            }
            if (num < this._rawText.Length)
            {
                if (this._rawText[num] == '<')
                {
                    throw new Exception("This script document does not appear to have a valid structure.");
                }
                this._scriptBlock = this.ReadCommandBlock(this._rawText);
            }
        }

        private CommandBlock ReadCommandBlock(string commandBlockText)
        {
            StringReader reader = new StringReader(this._rawText);
            string item = null;
            CommandBlock block = new CommandBlock();
            while ((item = reader.ReadLine()) != null)
            {
                block.Commands.Add(item);
            }
            return block;
        }

        internal string FilePath
        {
            get
            {
                return this._filePath;
            }
        }

        internal bool HasCode
        {
            get
            {
                return false;
            }
        }

        internal Block ScriptBlock
        {
            get
            {
                return this._scriptBlock;
            }
        }
    }
}

