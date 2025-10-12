using System;
using System.Configuration;
using System.Threading.Tasks;

namespace CandlePatternML
{
    /// <summary>
    /// Example usage of reading data from Google Sheets
    /// </summary>
    public class ReadGoogleSheet
    {
        public static async Task RunReadExampleAsync()
        {
            // Configuration - replace with your actual values
            string spreadsheetId = ConfigurationManager.AppSettings["spreadsheetid"];
            string credentialsPath = "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json";
            
            // Create the Google Sheets synchronizer
            var googleSync = new WorkSheetGoogleSync(spreadsheetId, credentialsPath);
            
            try
            {
                // Example 1: Read entire sheet
                Console.WriteLine("=== Reading entire sheet ===");
                var entireSheet = await googleSync.ReadAsync("Sheet1");
                if (entireSheet != null)
                {
                    Console.WriteLine($"Read sheet '{entireSheet.SheetName}' with {entireSheet.RowCount} rows and {entireSheet.ColumnCount} columns");
                    PrintWorkSheetData(entireSheet);
                }
                
                // Example 2: Read specific range
                Console.WriteLine("\n=== Reading specific range (A1:C10) ===");
                var rangeData = await googleSync.ReadAsync("Sheet1", "A1:C10");
                if (rangeData != null)
                {
                    Console.WriteLine($"Read range with {rangeData.RowCount} rows and {rangeData.ColumnCount} columns");
                    PrintWorkSheetData(rangeData);
                }
                
                // Example 3: Read entire columns
                Console.WriteLine("\n=== Reading entire columns (A:Z) ===");
                var columnData = await googleSync.ReadAsync("Sheet1", "A:Z");
                if (columnData != null)
                {
                    Console.WriteLine($"Read columns with {columnData.RowCount} rows and {columnData.ColumnCount} columns");
                    PrintWorkSheetData(columnData);
                }
                
                // Example 4: Synchronous read
                Console.WriteLine("\n=== Synchronous read ===");
                var syncData = googleSync.Read("Sheet1");
                if (syncData != null)
                {
                    Console.WriteLine($"Synchronous read completed: {syncData.RowCount} rows, {syncData.ColumnCount} columns");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during read operations: {ex.Message}");
            }
        }
        
        public static void RunReadExample()
        {
            // Synchronous version
            RunReadExampleAsync().GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Helper method to print WorkSheet data in a readable format
        /// </summary>
        private static void PrintWorkSheetData(WorkSheet worksheet)
        {
            if (worksheet == null)
            {
                Console.WriteLine("No data to display");
                return;
            }
            
            Console.WriteLine($"\nData from '{worksheet.SheetName}':");
            Console.WriteLine("Headers:");
            for (int col = 0; col < worksheet.ColumnCount; col++)
            {
                var column = worksheet.GetColumn(col);
                Console.Write($"{column?.Header ?? $"Column{col}"}\t");
            }
            Console.WriteLine();
            
            Console.WriteLine("Data:");
            for (int row = 0; row < Math.Min(worksheet.RowCount, 10); row++) // Limit to first 10 rows for display
            {
                var dataRow = worksheet.GetRow(row);
                if (dataRow != null)
                {
                    for (int col = 0; col < worksheet.ColumnCount; col++)
                    {
                        var value = dataRow.GetValue(col);
                        Console.Write($"{value ?? ""}\t");
                    }
                    Console.WriteLine();
                }
            }
            
            if (worksheet.RowCount > 10)
            {
                Console.WriteLine($"... and {worksheet.RowCount - 10} more rows");
            }
        }
        
        /// <summary>
        /// Example of reading data and processing it
        /// </summary>
        public static async Task ProcessReadDataAsync()
        {
            string spreadsheetId = ConfigurationManager.AppSettings["spreadsheetid"];
            string credentialsPath = "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json";
            
            var googleSync = new WorkSheetGoogleSync(spreadsheetId, credentialsPath);
            
            try
            {
                // Read data from a specific sheet
                var worksheet = await googleSync.ReadAsync("StockData");
                if (worksheet == null)
                {
                    Console.WriteLine("Failed to read data from Google Sheets");
                    return;
                }
                
                Console.WriteLine($"Processing data from '{worksheet.SheetName}':");
                Console.WriteLine($"Total rows: {worksheet.RowCount}");
                Console.WriteLine($"Total columns: {worksheet.ColumnCount}");
                
                // Process each row
                for (int row = 0; row < worksheet.RowCount; row++)
                {
                    var dataRow = worksheet.GetRow(row);
                    if (dataRow != null)
                    {
                        // Example: Process stock data
                        var symbol = dataRow.GetValue("A")?.ToString();
                        var price = dataRow.GetValue("B");
                        var date = dataRow.GetValue("C")?.ToString();
                        
                        if (!string.IsNullOrEmpty(symbol))
                        {
                            Console.WriteLine($"Row {row + 1}: {symbol} - {price} - {date}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing data: {ex.Message}");
            }
        }
    }
}
