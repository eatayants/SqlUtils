using System.Data;

namespace SqlUtils.Formatting
{
    internal class TableFormatterBuilder
    {
        internal static ITableFormatter BuildTableFormatter(DataTable dataTable)
        {
            return new SimpleTableFormatter(new DataRowRowFormatter(dataTable));
        }

        internal static ITableFormatter BuildTableFormatter(IDataReader dataReader)
        {
            return new SimpleTableFormatter(new DataRecordRowFormatter(dataReader));
        }
    }
}

