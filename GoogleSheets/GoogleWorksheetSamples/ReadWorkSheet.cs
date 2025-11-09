using System;
using System.Configuration;
using System.Threading.Tasks;

namespace CandlePatternML
{
    /// <summary>
    /// Example usage of reading data from Google Sheets
    /// </summary>
    public partial class  Program
    {
        public static async Task<WorkSheet> ReadWorkSheet(string sheetName)
        {
            // Configuration - replace with your actual values
            string spreadsheetId = ConfigurationManager.AppSettings["spreadsheetid"];
            string credentialsPath = "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json";
            
            // Create the Google Sheets synchronizer
            var googleSync = new WorkSheetGoogleSync(spreadsheetId, credentialsPath);

            //
            try
            {
                // Example 1: Read entire sheet
                Console.WriteLine("=== Reading entire sheet ===");
                var entireSheet = await googleSync.ReadAsync(sheetName);
                if (entireSheet != null)
                {
                    Console.WriteLine($"Read sheet '{entireSheet.SheetName}' with {entireSheet.RowCount} rows and {entireSheet.ColumnCount} columns");

                    return entireSheet;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during read operations: {ex.Message}");
            }

            return null;
        }
        
        
    }
}
