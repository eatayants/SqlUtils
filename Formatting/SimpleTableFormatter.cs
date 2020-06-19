using System;
using System.Text;

namespace SqlUtils.Formatting
{
    internal class SimpleTableFormatter : ITableFormatter
    {
        private int _actualRecordsRead;
        private const int ColumnSpacing = 3;
        private int[] _columnSizes;
        private int _currentRow;
        private IRowFormatter _rowFormatter;
        private const int SampleSize = 0x19;
        private string[,] _sampleData;
        private int _totalColumnWidth;
        private int _totalRowCount;

        internal SimpleTableFormatter(IRowFormatter rowFormatter) : this(rowFormatter, 0x19, 3)
        {
        }

        internal SimpleTableFormatter(IRowFormatter rowFormatter, int sampleSize, int columnSpacing)
        {
            this._currentRow = 1;
            this._rowFormatter = rowFormatter;
            this.Initialize(sampleSize, columnSpacing);
        }

        string ITableFormatter.ReadHeader()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this._sampleData.GetLength(1); i++)
            {
                builder.Append(string.Format("{0,-" + this._columnSizes[i] + "}", this._sampleData[0, i]));
            }
            return builder.ToString();
        }

        string ITableFormatter.ReadNextRow()
        {
            StringBuilder builder = null;
            if ((this._currentRow < this._sampleData.GetLength(0)) && (this._currentRow <= this._actualRecordsRead))
            {
                builder = new StringBuilder();
                for (int i = 0; i < this._sampleData.GetLength(1); i++)
                {
                    builder.Append(string.Format("{0,-" + this._columnSizes[i] + "}", this._sampleData[this._currentRow, i]));
                }
            }
            else
            {
                if (!this._rowFormatter.MoveNext())
                {
                    return null;
                }
                builder = new StringBuilder();
                for (int j = 0; j < this._rowFormatter.ColumnCount; j++)
                {
                    builder.Append(string.Format("{0,-" + this._columnSizes[j] + "}", this._rowFormatter.GetCellValueAsString(j)));
                }
                this._totalRowCount++;
            }
            this._currentRow++;
            return builder.ToString();
        }

        private void Initialize(int sampleSize, int columnSpacing)
        {
            int num;
            int num2;
            this._columnSizes = new int[this._rowFormatter.ColumnCount];
            this._sampleData = new string[sampleSize + 1, this._rowFormatter.ColumnCount];
            for (num = 0; num < this._rowFormatter.ColumnCount; num++)
            {
                this._sampleData[0, num] = this._rowFormatter.GetColumnName(num);
            }
            num = 1;
            while ((num <= sampleSize) && this._rowFormatter.MoveNext())
            {
                num2 = 0;
                while (num2 < this._rowFormatter.ColumnCount)
                {
                    this._sampleData[num, num2] = this._rowFormatter.GetCellValueAsString(num2);
                    num2++;
                }
                num++;
            }
            this._actualRecordsRead = num - 1;
            this._totalRowCount = this._actualRecordsRead;
            this._totalColumnWidth = 0;
            for (num = 0; num < this._sampleData.GetLength(1); num++)
            {
                for (num2 = 0; (num2 < this._sampleData.GetLength(0)) && (num2 <= this._actualRecordsRead); num2++)
                {
                    if (this._sampleData[num2, num] != null)
                    {
                        this._columnSizes[num] = Math.Max(this._columnSizes[num], this._sampleData[num2, num].Length);
                    }
                }
                this._columnSizes[num] = Math.Max(this._columnSizes[num] + columnSpacing, columnSpacing);
                this._totalColumnWidth += this._columnSizes[num];
            }
        }

        int ITableFormatter.TotalColumnWidth
        {
            get
            {
                return this._totalColumnWidth;
            }
        }
    }
}

