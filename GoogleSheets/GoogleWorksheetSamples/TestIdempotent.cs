using System;
using System.Configuration;
using System.Threading.Tasks;

namespace CandlePatternML
{
    /// <summary>
    /// Simple test to verify idempotent operations work correctly
    /// </summary>
    public class TestIdempotent
    {
        public static async Task RunTestAsync()
        {
            string spreadsheetId = ConfigurationManager.AppSettings["spreadsheetid"];
            string credentialsPath = "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json";
            
            var googleSync = new WorkSheetGoogleSync(spreadsheetId, credentialsPath);
            
            try
            {
                Console.WriteLine("=== Testing Idempotent Operations ===");
                
                // Create a test worksheet with headers and data
                var testWorksheet = new WorkSheet("IdempotentTest");
                testWorksheet.CreateColumn("Name");
                testWorksheet.CreateColumn("Age");
                testWorksheet.CreateColumn("City");
                
                // Add data (this will go into Rows, not overwrite headers)
                testWorksheet.SetValue(0, "A", "John");
                testWorksheet.SetValue(0, "B", 25);
                testWorksheet.SetValue(0, "C", "New York");
                
                testWorksheet.SetValue(1, "A", "Jane");
                testWorksheet.SetValue(1, "B", 30);
                testWorksheet.SetValue(1, "C", "Los Angeles");
                
                Console.WriteLine("Original worksheet structure:");
                googleSync.PrintWorkSheetDebugInfo(testWorksheet, "Original Worksheet");
                
                // Write to Google Sheets
                Console.WriteLine("\n1. Writing to Google Sheets...");
                bool writeSuccess = await googleSync.SynchronizeAsync(testWorksheet);
                Console.WriteLine($"Write success: {writeSuccess}");
                
                // Read back from Google Sheets
                Console.WriteLine("\n2. Reading back from Google Sheets...");
                var readBack = await googleSync.ReadAsync("IdempotentTest");
                if (readBack != null)
                {
                    Console.WriteLine("Read back worksheet structure:");
                    googleSync.PrintWorkSheetDebugInfo(readBack, "Read Back Worksheet");
                }
                else
                {
                    Console.WriteLine("Failed to read back worksheet");
                    return;
                }
                
                // Write the read-back data again
                Console.WriteLine("\n3. Writing read-back data to Google Sheets...");
                bool writeBackSuccess = await googleSync.SynchronizeAsync(readBack);
                Console.WriteLine($"Write back success: {writeBackSuccess}");
                
                // Read again to verify
                Console.WriteLine("\n4. Reading again to verify...");
                var finalRead = await googleSync.ReadAsync("IdempotentTest");
                if (finalRead != null)
                {
                    Console.WriteLine("Final worksheet structure:");
                    googleSync.PrintWorkSheetDebugInfo(finalRead, "Final Worksheet");
                    
                    // Compare with original
                    bool isIdentical = CompareWorkSheets(testWorksheet, finalRead);
                    Console.WriteLine($"\nIdempotent test result: {(isIdentical ? "✅ PASSED" : "❌ FAILED")}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during test: {ex.Message}");
            }
        }
        
        
        private static bool CompareWorkSheets(WorkSheet worksheet1, WorkSheet worksheet2)
        {
            if (worksheet1 == null || worksheet2 == null)
                return worksheet1 == worksheet2;
            
            // Compare dimensions
            if (worksheet1.RowCount != worksheet2.RowCount || worksheet1.ColumnCount != worksheet2.ColumnCount)
            {
                Console.WriteLine($"  Dimension mismatch: {worksheet1.RowCount}x{worksheet1.ColumnCount} vs {worksheet2.RowCount}x{worksheet2.ColumnCount}");
                return false;
            }
            
            // Compare headers
            for (int col = 0; col < worksheet1.ColumnCount; col++)
            {
                var col1 = worksheet1.GetColumn(col);
                var col2 = worksheet2.GetColumn(col);
                if (col1?.Header != col2?.Header)
                {
                    Console.WriteLine($"  Header mismatch at column {col}: '{col1?.Header}' vs '{col2?.Header}'");
                    return false;
                }
            }
            
            // Compare data
            for (int row = 0; row < worksheet1.RowCount; row++)
            {
                var row1 = worksheet1.GetRow(row);
                var row2 = worksheet2.GetRow(row);
                
                if (row1 == null || row2 == null)
                {
                    if (row1 != row2)
                    {
                        Console.WriteLine($"  Row {row} null mismatch");
                        return false;
                    }
                    continue;
                }
                
                for (int col = 0; col < worksheet1.ColumnCount; col++)
                {
                    var value1 = row1.GetValue(col);
                    var value2 = row2.GetValue(col);
                    
                    if (!Equals(value1, value2))
                    {
                        Console.WriteLine($"  Data mismatch at row {row}, col {col}: '{value1}' vs '{value2}'");
                        return false;
                    }
                }
            }
            
            return true;
        }
    }
}
