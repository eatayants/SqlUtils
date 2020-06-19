using System.Data;
using System.Text;

namespace SqlUtils.Formatting
{
    internal class DataRecordRowFormatter : IRowFormatter
    {
        private readonly IDataReader _reader;

        internal DataRecordRowFormatter(IDataReader reader)
        {
            _reader = reader;
        }

        string IRowFormatter.GetCellValueAsString(int columnIndex)
        {
            return ReadDataAsString(_reader, columnIndex);
        }

        string IRowFormatter.GetColumnName(int columnIndex)
        {
            return _reader.GetName(columnIndex);
        }

        bool IRowFormatter.MoveNext()
        {
            return _reader.Read();
        }

        private static string ReadDataAsString(IDataRecord dataRecord, int fieldIndex)
        {
            var obj2 = dataRecord.GetValue(fieldIndex);
            if (!(obj2 is byte[]))
            {
                return obj2.ToString();
            }
            var buffer = (byte[]) obj2;
            if (buffer.Length > 160)
            {
                return obj2.ToString();
            }
            var builder = new StringBuilder(buffer.Length * 2);
            builder.Append("0x");
            foreach (var item in buffer)
            {
                builder.AppendFormat("{0:X2}", item);
            }
            return builder.ToString();
        }

        int IRowFormatter.ColumnCount
        {
            get
            {
                return _reader.FieldCount;
            }
        }
    }
}

