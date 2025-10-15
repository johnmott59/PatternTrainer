using System;
using System.Collections.Generic;
using System.Linq;

namespace CandlePatternML
{
    /// <summary>
    /// Represents a column in a worksheet with a header and data
    /// </summary>
    public class WSColumn
    {
        public string Header { get; set; }
        public string ColumnLetter { get; set; }
        public int ColumnIndex { get; set; }
        public List<object> Data { get; set; } = new List<object>();

        public WSColumn(string header, int columnIndex)
        {
            Header = header;
            ColumnIndex = columnIndex;
            ColumnLetter = GetColumnLetter(columnIndex);
        }

        /// <summary>
        /// Converts a column index (0-based) to a column letter (A, B, C, etc.)
        /// </summary>
        private static string GetColumnLetter(int columnIndex)
        {
            if (columnIndex < 0) throw new ArgumentException("Column index must be non-negative");
            
            string result = "";
            while (columnIndex >= 0)
            {
                result = (char)('A' + (columnIndex % 26)) + result;
                columnIndex = (columnIndex / 26) - 1;
            }
            return result;
        }

        /// <summary>
        /// Adds a value to this column
        /// </summary>
        public void AddValue(object value)
        {
            Data.Add(value);
        }

        /// <summary>
        /// Gets the value at a specific row index
        /// </summary>
        public object GetValue(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= Data.Count)
                return null;
            return Data[rowIndex];
        }

        /// <summary>
        /// Sets the value at a specific row index
        /// </summary>
        public void SetValue(int rowIndex, object value)
        {
            while (Data.Count <= rowIndex)
            {
                Data.Add(null);
            }
            Data[rowIndex] = value;
        }
    }

    /// <summary>
    /// Represents a row in a worksheet
    /// </summary>
    public class WSRow
    {
        public int RowIndex { get; set; }
        public Dictionary<int, object> Values { get; set; } = new Dictionary<int, object>();
        public WorkSheet WorkSheet { get; set; }

        public WSRow(int rowIndex, WorkSheet workSheet)
        {
            RowIndex = rowIndex;
            WorkSheet = workSheet;
        }

        /// <summary>
        /// Sets a value in a specific column by index
        /// </summary>
        public void SetValue(int columnIndex, object value)
        {
            Values[columnIndex] = value;
        }

        /// <summary>
        /// Sets a value in a specific column by letter (A, B, C, etc.)
        /// </summary>
        public void SetValue(string columnLetter, object value)
        {
            int columnIndex = GetColumnIndex(columnLetter);
            SetValue(columnIndex, value);
        }

        /// <summary>
        /// Gets a value from a specific column by index
        /// </summary>
        public object GetValue(int columnIndex)
        {
            return Values.TryGetValue(columnIndex, out var value) ? value : null;
        }

        /// <summary>
        /// Gets a value from a specific column by letter (A, B, C, etc.)
        /// </summary>
        public object GetValue(string columnLetter)
        {
            int columnIndex = GetColumnIndex(columnLetter);
            return GetValue(columnIndex);
        }

        /// <summary>
        /// Converts a column letter to a column index
        /// </summary>
        private static int GetColumnIndex(string columnLetter)
        {
            if (string.IsNullOrEmpty(columnLetter))
                throw new ArgumentException("Column letter cannot be null or empty");

            int result = 0;
            foreach (char c in columnLetter.ToUpper())
            {
                if (c < 'A' || c > 'Z')
                    throw new ArgumentException($"Invalid column letter: {c}");
                result = result * 26 + (c - 'A' + 1);
            }
            return result - 1; // Convert to 0-based index
        }
    }

    /// <summary>
    /// Represents a worksheet that can hold rows and columns of data
    /// </summary>
    public class WorkSheet
    {
        public string SheetName { get; set; }
        public List<WSColumn> Columns { get; set; } = new List<WSColumn>();
        public List<WSRow> Rows { get; set; } = new List<WSRow>();
        public int NextColumnIndex { get; set; } = 0;
        public int NextRowIndex { get; set; } = 0;

        public WorkSheet(string sheetName)
        {
            SheetName = sheetName;
        }

        /// <summary>
        /// Creates a new column with the specified header
        /// </summary>
        public WSColumn CreateColumn(string header)
        {
            var column = new WSColumn(header, NextColumnIndex);
            Columns.Add(column);
            NextColumnIndex++;
            return column;
        }

        /// <summary>
        /// Creates a new column with an auto-generated header (A, B, C, etc.)
        /// </summary>
        public WSColumn CreateColumn()
        {
            string header = GetColumnLetter(NextColumnIndex);
            return CreateColumn(header);
        }

        /// <summary>
        /// Creates a new row
        /// </summary>
        public WSRow CreateRow()
        {
            var row = new WSRow(NextRowIndex, this);
            Rows.Add(row);
            NextRowIndex++;
            return row;
        }

        /// <summary>
        /// Gets a column by index
        /// </summary>
        public WSColumn GetColumn(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= Columns.Count)
                return null;
            return Columns[columnIndex];
        }

        /// <summary>
        /// Gets a column by letter (A, B, C, etc.)
        /// </summary>
        public WSColumn GetColumn(string columnLetter)
        {
            int columnIndex = GetColumnIndex(columnLetter);
            return GetColumn(columnIndex);
        }

        /// <summary>
        /// Gets a row by index
        /// </summary>
        public WSRow GetRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= Rows.Count)
                return null;
            return Rows[rowIndex];
        }

        /// <summary>
        /// Sets a value at a specific row and column
        /// </summary>
        public void SetValue(int rowIndex, int columnIndex, object value)
        {
            // Ensure the row exists
            while (Rows.Count <= rowIndex)
            {
                CreateRow();
            }

            // Ensure the column exists
            while (Columns.Count <= columnIndex)
            {
                CreateColumn();
            }

            // Set the value
            Rows[rowIndex].SetValue(columnIndex, value);
        }

        /// <summary>
        /// Sets a value at a specific row and column using letters
        /// </summary>
        public void SetValue(int rowIndex, string columnLetter, object value)
        {
            int columnIndex = GetColumnIndex(columnLetter);
            SetValue(rowIndex, columnIndex, value);
        }

        /// <summary>
        /// Gets a value at a specific row and column
        /// </summary>
        public object GetValue(int rowIndex, int columnIndex)
        {
            if (rowIndex < 0 || rowIndex >= Rows.Count || columnIndex < 0 || columnIndex >= Columns.Count)
                return null;
            return Rows[rowIndex].GetValue(columnIndex);
        }

        /// <summary>
        /// Gets a value at a specific row and column using letters
        /// </summary>
        public object GetValue(int rowIndex, string columnLetter)
        {
            int columnIndex = GetColumnIndex(columnLetter);
            return GetValue(rowIndex, columnIndex);
        }

        /// <summary>
        /// Gets the total number of rows (including empty ones)
        /// </summary>
        public int RowCount => Math.Max(Rows.Count, NextRowIndex);

        /// <summary>
        /// Gets the total number of columns (including empty ones)
        /// </summary>
        public int ColumnCount => Math.Max(Columns.Count, NextColumnIndex);

        /// <summary>
        /// Converts a column letter to a column index
        /// </summary>
        private static int GetColumnIndex(string columnLetter)
        {
            if (string.IsNullOrEmpty(columnLetter))
                throw new ArgumentException("Column letter cannot be null or empty");

            int result = 0;
            foreach (char c in columnLetter.ToUpper())
            {
                if (c < 'A' || c > 'Z')
                    throw new ArgumentException($"Invalid column letter: {c}");
                result = result * 26 + (c - 'A' + 1);
            }
            return result - 1; // Convert to 0-based index
        }

        /// <summary>
        /// Converts a column index to a column letter
        /// </summary>
        private static string GetColumnLetter(int columnIndex)
        {
            if (columnIndex < 0) throw new ArgumentException("Column index must be non-negative");
            
            string result = "";
            while (columnIndex >= 0)
            {
                result = (char)('A' + (columnIndex % 26)) + result;
                columnIndex = (columnIndex / 26) - 1;
            }
            return result;
        }

        /// <summary>
        /// Clears all data from the worksheet
        /// </summary>
        public void Clear()
        {
            Rows.Clear();
            Columns.Clear();
            NextRowIndex = 0;
            NextColumnIndex = 0;
        }

        /// <summary>
        /// Gets all data as a 2D array for easy export
        /// </summary>
        public object[,] GetDataArray()
        {
            int dataRows = RowCount;
            int cols = ColumnCount;
            
            if (dataRows == 0 || cols == 0)
                return new object[0, 0];

            // Include headers in the total row count
            int totalRows = dataRows + 1; // +1 for header row
            var data = new object[totalRows, cols];

            // Fill headers in row 0
            for (int col = 0; col < cols; col++)
            {
                if (col < Columns.Count)
                    data[0, col] = Columns[col].Header;
                else
                    data[0, col] = GetColumnLetter(col);
            }

            // Fill data starting from row 1
            for (int row = 0; row < dataRows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (row < Rows.Count)
                        data[row + 1, col] = Rows[row].GetValue(col); // +1 to account for header row
                    else
                        data[row + 1, col] = null;
                }
            }

            return data;
        }
    }
}
