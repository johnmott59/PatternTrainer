using System;
using System.Configuration;
using System.Threading.Tasks;

namespace CandlePatternML
{
    /// <summary>
    /// Example demonstrating idempotent read/write operations with Google Sheets
    /// </summary>
    public class IdempotentReadWrite
    {
        public static async Task RunIdempotentExampleAsync()
        {
            // Configuration - replace with your actual values
            string spreadsheetId = ConfigurationManager.AppSettings["spreadsheetid"];
            string credentialsPath = "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json";
            
            // Create the Google Sheets synchronizer
            var googleSync = new WorkSheetGoogleSync(spreadsheetId, credentialsPath);
            
            try
            {
                Console.WriteLine("=== Idempotent Read/Write Example ===");
                
                // Step 1: Read the existing sheet
                Console.WriteLine("1. Reading existing sheet...");
                var originalWorksheet = await googleSync.ReadAsync("TestSheet");
                
                if (originalWorksheet == null)
                {
                    Console.WriteLine("Failed to read the sheet. Creating a new one...");
                    
                    // Create a new worksheet with sample data
                    originalWorksheet = new WorkSheet("TestSheet");
                    originalWorksheet.CreateColumn("Name");
                    originalWorksheet.CreateColumn("Age");
                    originalWorksheet.CreateColumn("City");
                    
                    // Add sample data
                    originalWorksheet.SetValue(0, "A", "John");
                    originalWorksheet.SetValue(0, "B", 25);
                    originalWorksheet.SetValue(0, "C", "New York");
                    
                    originalWorksheet.SetValue(1, "A", "Jane");
                    originalWorksheet.SetValue(1, "B", 30);
                    originalWorksheet.SetValue(1, "C", "Los Angeles");
                    
                    // Write the initial data
                    bool success = await googleSync.SynchronizeAsync(originalWorksheet);
                    if (!success)
                    {
                        Console.WriteLine("Failed to create initial sheet");
                        return;
                    }
                }
                
                Console.WriteLine($"Original sheet has {originalWorksheet.RowCount} rows and {originalWorksheet.ColumnCount} columns");
                PrintWorkSheetData(originalWorksheet, "Original Data");
                
                // Step 2: Write the same data back (should be idempotent)
                Console.WriteLine("\n2. Writing the same data back...");
                bool writeSuccess = await googleSync.SynchronizeAsync(originalWorksheet);
                
                if (writeSuccess)
                {
                    Console.WriteLine("✅ Write operation completed successfully");
                }
                else
                {
                    Console.WriteLine("❌ Write operation failed");
                    return;
                }
                
                // Step 3: Read the sheet again to verify it's unchanged
                Console.WriteLine("\n3. Reading the sheet again to verify no changes...");
                var verifyWorksheet = await googleSync.ReadAsync("TestSheet");
                
                if (verifyWorksheet != null)
                {
                    Console.WriteLine($"Verification sheet has {verifyWorksheet.RowCount} rows and {verifyWorksheet.ColumnCount} columns");
                    PrintWorkSheetData(verifyWorksheet, "Verification Data");
                    
                    // Compare the data
                    bool isIdentical = CompareWorkSheets(originalWorksheet, verifyWorksheet);
                    if (isIdentical)
                    {
                        Console.WriteLine("✅ Idempotent operation successful - data is identical!");
                    }
                    else
                    {
                        Console.WriteLine("❌ Data has changed - operation is not idempotent");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Failed to read sheet for verification");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during idempotent operation: {ex.Message}");
            }
        }
        
        public static void RunIdempotentExample()
        {
            // Synchronous version
            RunIdempotentExampleAsync().GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Demonstrates reading with explicit header control
        /// </summary>
        public static async Task RunHeaderControlExampleAsync()
        {
            string spreadsheetId = ConfigurationManager.AppSettings["spreadsheetid"];
            string credentialsPath = "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json";
            
            var googleSync = new WorkSheetGoogleSync(spreadsheetId, credentialsPath);
            
            try
            {
                Console.WriteLine("\n=== Header Control Example ===");
                
                // Read with headers (default behavior)
                Console.WriteLine("1. Reading with headers (default)...");
                var withHeaders = await googleSync.ReadWithHeadersAsync("TestSheet", hasHeaders: true);
                if (withHeaders != null)
                {
                    Console.WriteLine($"With headers: {withHeaders.RowCount} data rows, {withHeaders.ColumnCount} columns");
                    PrintWorkSheetData(withHeaders, "With Headers");
                }
                
                // Read without headers
                Console.WriteLine("\n2. Reading without headers...");
                var withoutHeaders = await googleSync.ReadWithHeadersAsync("TestSheet", hasHeaders: false);
                if (withoutHeaders != null)
                {
                    Console.WriteLine($"Without headers: {withoutHeaders.RowCount} data rows, {withoutHeaders.ColumnCount} columns");
                    PrintWorkSheetData(withoutHeaders, "Without Headers");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during header control example: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Helper method to print WorkSheet data
        /// </summary>
        private static void PrintWorkSheetData(WorkSheet worksheet, string title)
        {
            Console.WriteLine($"\n--- {title} ---");
            
            if (worksheet == null)
            {
                Console.WriteLine("No data to display");
                return;
            }
            
            // Print headers
            Console.WriteLine("Headers:");
            for (int col = 0; col < worksheet.ColumnCount; col++)
            {
                var column = worksheet.GetColumn(col);
                Console.Write($"{column?.Header ?? $"Column{col}"}\t");
            }
            Console.WriteLine();
            
            // Print data (limit to first 5 rows for display)
            Console.WriteLine("Data:");
            for (int row = 0; row < Math.Min(worksheet.RowCount, 5); row++)
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
            
            if (worksheet.RowCount > 5)
            {
                Console.WriteLine($"... and {worksheet.RowCount - 5} more rows");
            }
        }
        
        /// <summary>
        /// Compares two WorkSheet objects to see if they contain identical data
        /// </summary>
        private static bool CompareWorkSheets(WorkSheet worksheet1, WorkSheet worksheet2)
        {
            if (worksheet1 == null || worksheet2 == null)
                return worksheet1 == worksheet2;
            
            // Compare dimensions
            if (worksheet1.RowCount != worksheet2.RowCount || worksheet1.ColumnCount != worksheet2.ColumnCount)
                return false;
            
            // Compare headers
            for (int col = 0; col < worksheet1.ColumnCount; col++)
            {
                var col1 = worksheet1.GetColumn(col);
                var col2 = worksheet2.GetColumn(col);
                if (col1?.Header != col2?.Header)
                    return false;
            }
            
            // Compare data
            for (int row = 0; row < worksheet1.RowCount; row++)
            {
                var row1 = worksheet1.GetRow(row);
                var row2 = worksheet2.GetRow(row);
                
                if (row1 == null || row2 == null)
                {
                    if (row1 != row2)
                        return false;
                    continue;
                }
                
                for (int col = 0; col < worksheet1.ColumnCount; col++)
                {
                    var value1 = row1.GetValue(col);
                    var value2 = row2.GetValue(col);
                    
                    if (!Equals(value1, value2))
                        return false;
                }
            }
            
            return true;
        }
    }
}

