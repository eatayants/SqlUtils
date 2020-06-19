namespace SqlUtils.Formatting
{
    internal interface IRowFormatter
    {
        string GetCellValueAsString(int columnIndex);
        string GetColumnName(int columnIndex);
        bool MoveNext();
        int ColumnCount { get; }
    }
}

