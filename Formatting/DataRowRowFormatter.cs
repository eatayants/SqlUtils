using System.Data;
using System.Text;

namespace SqlUtils.Formatting
{
    internal class DataRowRowFormatter : IRowFormatter
    {
        private DataTable _dataTable;
        private int _rowIndex = -1;

        internal DataRowRowFormatter(DataTable dataTable)
        {
            this._dataTable = dataTable;
        }

        string IRowFormatter.GetCellValueAsString(int columnIndex)
        {
            return ReadDataAsString(this._dataTable, this._rowIndex, columnIndex);
        }

        string IRowFormatter.GetColumnName(int columnIndex)
        {
            return this._dataTable.Columns[columnIndex].ColumnName;
        }

        bool IRowFormatter.MoveNext()
        {
            if (this._rowIndex < (this._dataTable.Rows.Count - 1))
            {
                this._rowIndex++;
                return true;
            }
            return false;
        }

        private static string ReadDataAsString(DataTable dataTable, int rowIndex, int fieldIndex)
        {
            object obj2 = dataTable.Rows[rowIndex][fieldIndex];
            if (obj2 is byte[])
            {
                byte[] buffer = (byte[]) obj2;
                if (buffer.Length <= 160)
                {
                    StringBuilder builder = new StringBuilder(buffer.Length * 2);
                    builder.Append("0x");
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        builder.AppendFormat("{0:X2}", buffer[i]);
                    }
                    return builder.ToString();
                }
            }
            return obj2.ToString();
        }

        int IRowFormatter.ColumnCount
        {
            get
            {
                return this._dataTable.Columns.Count;
            }
        }
    }
}

