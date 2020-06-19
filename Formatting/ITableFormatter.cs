namespace SqlUtils.Formatting
{
    internal interface ITableFormatter
    {
        string ReadHeader();
        string ReadNextRow();
        int TotalColumnWidth { get; }
    }
}

