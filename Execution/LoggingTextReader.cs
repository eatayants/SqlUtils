using System.IO;

namespace SqlUtils
{
    internal class LoggingTextReader : TextReader
    {
        private TextWriter _logWriter;
        private TextReader _textReader;

        internal LoggingTextReader(TextReader textReader, TextWriter logWriter)
        {
            this._textReader = textReader;
            this._logWriter = logWriter;
        }

        public override void Close()
        {
            this._textReader.Close();
            this._logWriter.Close();
        }

        public override int Peek()
        {
            return this._textReader.Peek();
        }

        public override int Read()
        {
            return this._textReader.Read();
        }

        public override int Read(char[] buffer, int index, int count)
        {
            return this._textReader.Read(buffer, index, count);
        }

        public override int ReadBlock(char[] buffer, int index, int count)
        {
            return this._textReader.ReadBlock(buffer, index, count);
        }

        public override string ReadLine()
        {
            string str = this._textReader.ReadLine();
            this._logWriter.WriteLine(str);
            return str;
        }

        public override string ReadToEnd()
        {
            string str = this._textReader.ReadToEnd();
            this._logWriter.Write(str);
            return str;
        }
    }
}

