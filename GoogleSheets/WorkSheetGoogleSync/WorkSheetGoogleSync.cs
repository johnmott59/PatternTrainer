using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Added for FirstOrDefault
using System.Threading;
using System.Threading.Tasks;

namespace CandlePatternML
{
    /// <summary>
    /// Provides synchronization functionality to save WorkSheet data to Google Sheets
    /// </summary>
    public partial class WorkSheetGoogleSync
    {  



 


        /// <summary>
        /// Reads data from a Google Sheet and returns it as a WorkSheet model (synchronous version)
        /// </summary>
        /// <param name="sheetName">The name of the sheet to read from</param>
        /// <param name="range">Optional range to read (e.g., "A1:Z100" or "A:Z"). If null, reads the entire sheet</param>
        /// <returns>A WorkSheet object containing the read data, or null if reading failed</returns>
        public WorkSheet Read(string sheetName, string range = null)
        {
            return ReadAsync(sheetName, range).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Reads data from a Google Sheet with explicit header control
        /// </summary>
        /// <param name="sheetName">The name of the sheet to read from</param>
        /// <param name="range">Optional range to read (e.g., "A1:Z100" or "A:Z"). If null, reads the entire sheet</param>
        /// <param name="hasHeaders">Whether the first row contains headers</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A WorkSheet object containing the read data, or null if reading failed</returns>
        public async Task<WorkSheet> ReadWithHeadersAsync(string sheetName, string range = null, bool hasHeaders = true, CancellationToken cancellationToken = default)
        {
            try
            {
                var service = await GetSheetsServiceAsync(cancellationToken);
                
                // Construct the range string
                string rangeString = string.IsNullOrEmpty(range) ? sheetName : $"{sheetName}!{range}";
                
                // Read the data from Google Sheets
                var request = service.Spreadsheets.Values.Get(_spreadsheetId, rangeString);
                var response = await request.ExecuteAsync(cancellationToken);
                
                if (response.Values == null || response.Values.Count == 0)
                {
                    Console.WriteLine($"No data found in sheet '{sheetName}'");
                    return new WorkSheet(sheetName);
                }
                
                // Convert the Google Sheets data to WorkSheet model
                var worksheet = ConvertFromGoogleSheetsFormat(response.Values, sheetName, hasHeaders);
                
                Console.WriteLine($"Successfully read {worksheet.RowCount} rows and {worksheet.ColumnCount} columns from sheet '{sheetName}'");
                return worksheet;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from sheet '{sheetName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Reads data from a Google Sheet with explicit header control (synchronous version)
        /// </summary>
        /// <param name="sheetName">The name of the sheet to read from</param>
        /// <param name="range">Optional range to read (e.g., "A1:Z100" or "A:Z"). If null, reads the entire sheet</param>
        /// <param name="hasHeaders">Whether the first row contains headers</param>
        /// <returns>A WorkSheet object containing the read data, or null if reading failed</returns>
        public WorkSheet ReadWithHeaders(string sheetName, string range = null, bool hasHeaders = true)
        {
            return ReadWithHeadersAsync(sheetName, range, hasHeaders).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Converts Google Sheets data format to WorkSheet model
        /// </summary>
        private WorkSheet ConvertFromGoogleSheetsFormat(IList<IList<object>> values, string sheetName, bool hasHeaders = true)
        {
            var worksheet = new WorkSheet(sheetName);
            
            if (values == null || values.Count == 0)
                return worksheet;
            
            // Determine the number of columns (use the maximum row length)
            int maxColumns = values.Max(row => row?.Count ?? 0);
            
            // Create columns first
            for (int col = 0; col < maxColumns; col++)
            {
                string header = $"Column{GetColumnLetter(col)}";
                worksheet.CreateColumn(header);
            }
            
            if (hasHeaders && values.Count > 0 && values[0] != null)
            {
                // Set column headers from first row
                for (int col = 0; col < Math.Min(values[0].Count, maxColumns); col++)
                {
                    string headerValue = values[0][col]?.ToString();
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        worksheet.Columns[col].Header = headerValue;
                    }
                    else
                    {
                        worksheet.Columns[col].Header = $"Column{GetColumnLetter(col)}";
                    }
                }
                
                // Start data from row 1 (skip header row)
                for (int row = 1; row < values.Count; row++)
                {
                    var dataRow = worksheet.CreateRow();
                    if (values[row] != null)
                    {
                        for (int col = 0; col < Math.Min(values[row].Count, maxColumns); col++)
                        {
                            dataRow.SetValue(col, values[row][col]);
                        }
                    }
                }
            }
            else
            {
                // No headers, treat all rows as data
                for (int row = 0; row < values.Count; row++)
                {
                    var dataRow = worksheet.CreateRow();
                    if (values[row] != null)
                    {
                        for (int col = 0; col < Math.Min(values[row].Count, maxColumns); col++)
                        {
                            dataRow.SetValue(col, values[row][col]);
                        }
                    }
                }
            }
            
            return worksheet;
        }

        /// <summary>
        /// Converts a column index to a column letter (A, B, C, etc.)
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
        /// Debug method to print WorkSheet structure for troubleshooting
        /// </summary>
        public void PrintWorkSheetDebugInfo(WorkSheet worksheet, string title = "WorkSheet Debug Info")
        {
            Console.WriteLine($"\n=== {title} ===");
            Console.WriteLine($"Sheet Name: {worksheet.SheetName}");
            Console.WriteLine($"Column Count: {worksheet.ColumnCount}");
            Console.WriteLine($"Row Count: {worksheet.RowCount}");
            
            Console.WriteLine("\nHeaders:");
            for (int col = 0; col < worksheet.ColumnCount; col++)
            {
                var column = worksheet.GetColumn(col);
                Console.WriteLine($"  Column {col}: '{column?.Header}'");
            }
            
            Console.WriteLine("\nData Rows:");
            for (int row = 0; row < worksheet.RowCount; row++)
            {
                var dataRow = worksheet.GetRow(row);
                Console.Write($"  Row {row}: ");
                if (dataRow != null)
                {
                    for (int col = 0; col < worksheet.ColumnCount; col++)
                    {
                        var value = dataRow.GetValue(col);
                        Console.Write($"{value ?? "null"} ");
                    }
                }
                else
                {
                    Console.Write("(null row)");
                }
                Console.WriteLine();
            }
            
            // Show what GetDataArray() would produce
            var dataArray = worksheet.GetDataArray();
            Console.WriteLine($"\nGetDataArray() produces {dataArray.GetLength(0)} rows x {dataArray.GetLength(1)} columns:");
            for (int row = 0; row < dataArray.GetLength(0); row++)
            {
                Console.Write($"  Array Row {row}: ");
                for (int col = 0; col < dataArray.GetLength(1); col++)
                {
                    Console.Write($"{dataArray[row, col] ?? "null"} ");
                }
                Console.WriteLine();
            }
        }
    }
}
