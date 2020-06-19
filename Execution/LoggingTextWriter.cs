using System;
using System.Collections.Generic;
using System.IO;

namespace SqlUtils
{
    internal class LoggingTextWriter : TextWriter
    {
        private List<WriterInfo> _writerList = new List<WriterInfo>();
        private Dictionary<string, WriterInfo> _writerMap = new Dictionary<string, WriterInfo>(StringComparer.OrdinalIgnoreCase);

        internal LoggingTextWriter()
        {
        }

        internal void AddTextWriter(string name, TextWriter writer, bool enabled)
        {
            if ((this._writerList == null) || (this._writerMap == null))
            {
                throw new InternalException("writerList and writerMap should be initialized.");
            }
            if (this._writerMap.ContainsKey(name))
            {
                throw new Exception("Writer '" + name + "' has already been added.");
            }
            WriterInfo item = new WriterInfo(name, writer, enabled);
            this._writerList.Add(item);
            this._writerMap[name] = item;
        }

        public override void Close()
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    info.Writer.Close();
                }
            }
        }

        internal void EnableWriter(string name, bool enable)
        {
            if ((this._writerList == null) || (this._writerMap == null))
            {
                throw new InternalException("writerList and writerMap should be initialized.");
            }
            WriterInfo info = null;
            if (!this._writerMap.TryGetValue(name, out info) || (info == null))
            {
                throw new Exception("Writer '" + name + "' does not exist.");
            }
            info.Enabled = enable;
        }

        public override void Flush()
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    info.Writer.Flush();
                }
            }
        }

        internal void RemoveTextWriter(string name)
        {
            if ((this._writerList == null) || (this._writerMap == null))
            {
                throw new InternalException("writerList and writerMap should be initialized.");
            }
            WriterInfo info = null;
            if (!this._writerMap.TryGetValue(name, out info) || (info == null))
            {
                throw new Exception("Writer '" + name + "' does not exist.");
            }
            this._writerList.Remove(info);
            this._writerMap.Remove(name);
        }

        public override void Write(bool value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(value);
                    }
                }
            }
        }

        public override void Write(char value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(value);
                    }
                }
            }
        }

        public override void Write(string value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(value);
                    }
                }
            }
        }

        public override void Write(char[] buffer)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(buffer);
                    }
                }
            }
        }

        public override void Write(decimal value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(value);
                    }
                }
            }
        }

        public override void Write(double value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(value);
                    }
                }
            }
        }

        public override void Write(int value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(value);
                    }
                }
            }
        }

        public override void Write(long value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(value);
                    }
                }
            }
        }

        public override void Write(object value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(value);
                    }
                }
            }
        }

        public override void Write(float value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(value);
                    }
                }
            }
        }

        public override void Write(uint value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(value);
                    }
                }
            }
        }

        public override void Write(ulong value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(value);
                    }
                }
            }
        }

        public override void Write(string format, object arg0)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(format, arg0);
                    }
                }
            }
        }

        public override void Write(string format, params object[] arg)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(format, arg);
                    }
                }
            }
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(buffer, index, count);
                    }
                }
            }
        }

        public override void Write(string format, object arg0, object arg1)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(format, arg0, arg1);
                    }
                }
            }
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.Write(format, arg0, arg1, arg2);
                    }
                }
            }
        }

        public override void WriteLine()
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine();
                    }
                }
            }
        }

        public override void WriteLine(bool value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(value);
                    }
                }
            }
        }

        public override void WriteLine(char value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(value);
                    }
                }
            }
        }

        public override void WriteLine(decimal value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(value);
                    }
                }
            }
        }

        public override void WriteLine(char[] buffer)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(buffer);
                    }
                }
            }
        }

        public override void WriteLine(double value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(value);
                    }
                }
            }
        }

        public override void WriteLine(int value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(value);
                    }
                }
            }
        }

        public override void WriteLine(long value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(value);
                    }
                }
            }
        }

        public override void WriteLine(object value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(value);
                    }
                }
            }
        }

        public override void WriteLine(float value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(value);
                    }
                }
            }
        }

        public override void WriteLine(string value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(value);
                    }
                }
            }
        }

        public override void WriteLine(uint value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(value);
                    }
                }
            }
        }

        public override void WriteLine(ulong value)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(value);
                    }
                }
            }
        }

        public override void WriteLine(string format, object arg0)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(format, arg0);
                    }
                }
            }
        }

        public override void WriteLine(string format, params object[] arg)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(format, arg);
                    }
                }
            }
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(format, arg0, arg1);
                    }
                }
            }
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(buffer, index, count);
                    }
                }
            }
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            if (((this._writerList != null) && (this._writerMap != null)) && (this._writerList.Count > 0))
            {
                foreach (WriterInfo info in this._writerList)
                {
                    if (info.Enabled)
                    {
                        info.Writer.WriteLine(format, arg0, arg1, arg2);
                    }
                }
            }
        }

        public override System.Text.Encoding Encoding
        {
            get
            {
                return this._writerList[0].Writer.Encoding;
            }
        }

        public override IFormatProvider FormatProvider
        {
            get
            {
                return this._writerList[0].Writer.FormatProvider;
            }
        }

        public override string NewLine
        {
            get
            {
                return this._writerList[0].Writer.NewLine;
            }
            set
            {
                foreach (WriterInfo info in this._writerList)
                {
                    info.Writer.NewLine = value;
                }
            }
        }
    }
}

