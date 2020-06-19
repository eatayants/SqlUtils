using System.IO;

namespace SqlUtils
{
    internal class WriterInfo
    {
        internal bool Enabled = true;
        internal string Name;
        internal TextWriter Writer;

        internal WriterInfo(string name, TextWriter writer, bool enabled)
        {
            this.Name = name;
            this.Writer = writer;
            this.Enabled = enabled;
        }
    }
}

