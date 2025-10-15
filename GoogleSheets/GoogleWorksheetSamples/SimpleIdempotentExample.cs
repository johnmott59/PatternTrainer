using System;
using System.Configuration;
using System.Threading.Tasks;

namespace CandlePatternML
{
    /// <summary>
    /// Simple example showing how to use the fixed idempotent Google Sheets operations
    /// </summary>
    public class SimpleIdempotentExample
    {
        public static async Task RunExampleAsync()
        {
            string spreadsheetId = ConfigurationManager.AppSettings["spreadsheetid"];
            string credentialsPath = "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json";
            
            var googleSync = new WorkSheetGoogleSync(spreadsheetId, credentialsPath);
            
            try
            {
                Console.WriteLine("=== Simple Idempotent Example ===");
                
                // Step 1: Read existing data
                Console.WriteLine("1. Reading existing sheet...");
                var worksheet = await googleSync.ReadAsync("MyData");
                
                if (worksheet == null)
                {
                    Console.WriteLine("Sheet doesn't exist, creating new one...");
                    worksheet = new WorkSheet("MyData");
                    worksheet.CreateColumn("Product");
                    worksheet.CreateColumn("Price");
                    worksheet.CreateColumn("Quantity");
                    
                    // Add some sample data
                    worksheet.SetValue(0, "A", "Widget A");
                    worksheet.SetValue(0, "B", 19.99);
                    worksheet.SetValue(0, "C", 100);
                    
                    worksheet.SetValue(1, "A", "Widget B");
                    worksheet.SetValue(1, "B", 29.99);
                    worksheet.SetValue(1, "C", 50);
                }
                
                Console.WriteLine("Current data:");
                googleSync.PrintWorkSheetDebugInfo(worksheet, "Current Data");
                
                // Step 2: Write the data (this should preserve headers)
                Console.WriteLine("\n2. Writing data to Google Sheets...");
                bool success = await googleSync.SynchronizeAsync(worksheet);
                Console.WriteLine($"Write success: {success}");
                
                // Step 3: Read it back to verify idempotency
                Console.WriteLine("\n3. Reading back to verify...");
                var readBack = await googleSync.ReadAsync("MyData");
                
                if (readBack != null)
                {
                    Console.WriteLine("Read back data:");
                    googleSync.PrintWorkSheetDebugInfo(readBack, "Read Back Data");
                    
                    // The data should be identical
                    bool isIdentical = AreWorkSheetsIdentical(worksheet, readBack);
                    Console.WriteLine($"\nData is identical: {(isIdentical ? "✅ YES" : "❌ NO")}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
        private static bool AreWorkSheetsIdentical(WorkSheet ws1, WorkSheet ws2)
        {
            if (ws1 == null || ws2 == null) return ws1 == ws2;
            if (ws1.RowCount != ws2.RowCount || ws1.ColumnCount != ws2.ColumnCount) return false;
            
            // Check headers
            for (int col = 0; col < ws1.ColumnCount; col++)
            {
                if (ws1.GetColumn(col)?.Header != ws2.GetColumn(col)?.Header)
                    return false;
            }
            
            // Check data
            for (int row = 0; row < ws1.RowCount; row++)
            {
                for (int col = 0; col < ws1.ColumnCount; col++)
                {
                    if (!Equals(ws1.GetRow(row)?.GetValue(col), ws2.GetRow(row)?.GetValue(col)))
                        return false;
                }
            }
            
            return true;
        }
    }
}
