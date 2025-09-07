using System;
using System.Configuration;
using System.Threading.Tasks;

namespace CandlePatternML
{
    /// <summary>
    /// Example usage of the WorkSheet model with Google Sheets synchronization
    /// </summary>
    public class WorkSheetExample
    {
        public static async Task RunExampleAsync()
        {
            // Configuration - replace with your actual values
            string spreadsheetId = ConfigurationManager.AppSettings["spreadsheetid"];
            string credentialsPath = "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json";
            
            // Create the Google Sheets synchronizer
            var googleSync = new WorkSheetGoogleSync(spreadsheetId, credentialsPath);
            
            // Create a new worksheet
            var worksheet = new WorkSheet("StockData");
            
            // Create columns with custom headers
            var symbolCol = worksheet.CreateColumn("Symbol");
            var nameCol = worksheet.CreateColumn("Company Name");
            var priceCol = worksheet.CreateColumn("Price");
            var dateCol = worksheet.CreateColumn("Date");
            var notesCol = worksheet.CreateColumn("Notes");
            
            // Add data using different methods
            
            // Method 1: Using CreateRow and SetValue
            var row1 = worksheet.CreateRow();
            row1.SetValue("A", "AAPL");
            row1.SetValue("B", "Apple Inc.");
            row1.SetValue("C", 189.55);
            row1.SetValue("D", DateTime.Now.ToString("yyyy-MM-dd"));
            row1.SetValue("E", "Technology stock");
            
            // Method 2: Using worksheet.SetValue directly
            worksheet.SetValue(1, "A", "MSFT");
            worksheet.SetValue(1, "B", "Microsoft Corp.");
            worksheet.SetValue(1, "C", 420.10);
            worksheet.SetValue(1, "D", DateTime.Now.ToString("yyyy-MM-dd"));
            worksheet.SetValue(1, "E", "Software giant");
            
            // Method 3: Using column letters
            worksheet.SetValue(2, "A", "GOOGL");
            worksheet.SetValue(2, "B", "Alphabet Inc.");
            worksheet.SetValue(2, "C", 2800.00);
            worksheet.SetValue(2, "D", DateTime.Now.ToString("yyyy-MM-dd"));
            worksheet.SetValue(2, "E", "Search engine leader");
            
            // Method 4: Using column indices
            worksheet.SetValue(3, 0, "TSLA");
            worksheet.SetValue(3, 1, "Tesla Inc.");
            worksheet.SetValue(3, 2, 250.75);
            worksheet.SetValue(3, 3, DateTime.Now.ToString("yyyy-MM-dd"));
            worksheet.SetValue(3, 4, "Electric vehicles");
            
            Console.WriteLine($"Worksheet '{worksheet.SheetName}' created with:");
            Console.WriteLine($"  - {worksheet.ColumnCount} columns");
            Console.WriteLine($"  - {worksheet.RowCount} rows");
            Console.WriteLine($"  - Data ready for synchronization");
            
            // Ensure the sheet exists in Google Sheets (optional)
            bool sheetExists = await googleSync.EnsureSheetExistsAsync("StockData");
            if (!sheetExists)
            {
                Console.WriteLine("Warning: Could not create sheet in Google Sheets");
                return;
            }
            
            // Synchronize the data to Google Sheets
            Console.WriteLine("Synchronizing to Google Sheets...");
            bool success = await googleSync.SynchronizeAsync(worksheet);
            
            if (success)
            {
                Console.WriteLine("✅ Successfully synchronized to Google Sheets!");
            }
            else
            {
                Console.WriteLine("❌ Failed to synchronize to Google Sheets");
            }
        }
        
        public static void RunExample()
        {
            // Synchronous version
            RunExampleAsync().GetAwaiter().GetResult();
        }
        
        public static async Task RunAppendExampleAsync()
        {
            // Configuration
            string spreadsheetId = "1gnkMIbxJN6EVEO18Yj9-4KPrX7MV2qXl7X5KGi4P6o8";
            string credentialsPath = "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json";
            
            var googleSync = new WorkSheetGoogleSync(spreadsheetId, credentialsPath);
            
            // Create a worksheet for new data to append
            var newData = new WorkSheet("StockData");
            
            // Create columns
            newData.CreateColumn("Symbol");
            newData.CreateColumn("Company Name");
            newData.CreateColumn("Price");
            newData.CreateColumn("Date");
            newData.CreateColumn("Notes");
            
            // Add new data
            newData.SetValue(0, "A", "AMZN");
            newData.SetValue(0, "B", "Amazon.com Inc.");
            newData.SetValue(0, "C", 175.25);
            newData.SetValue(0, "D", DateTime.Now.ToString("yyyy-MM-dd"));
            newData.SetValue(0, "E", "E-commerce leader");
            
            newData.SetValue(1, "A", "META");
            newData.SetValue(1, "B", "Meta Platforms Inc.");
            newData.SetValue(1, "C", 485.90);
            newData.SetValue(1, "D", DateTime.Now.ToString("yyyy-MM-dd"));
            newData.SetValue(1, "E", "Social media");
            
            // Append the new data (don't clear existing)
            Console.WriteLine("Appending new data to Google Sheets...");
            bool success = await googleSync.AppendAsync(newData);
            
            if (success)
            {
                Console.WriteLine("✅ Successfully appended to Google Sheets!");
            }
            else
            {
                Console.WriteLine("❌ Failed to append to Google Sheets");
            }
        }
    }
}
