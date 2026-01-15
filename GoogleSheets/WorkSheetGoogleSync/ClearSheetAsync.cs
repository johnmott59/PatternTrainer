using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;


namespace CandlePatternML
{

    public partial class WorkSheetGoogleSync
    {
        /// <summary>
        /// Clears the entire sheet content
        /// </summary>
        private async Task ClearSheetAsync(SheetsService service, string sheetName, CancellationToken cancellationToken)
        {
            try
            {
                // Only try to clear if the sheet exists and has content
                var clearRequest = service.Spreadsheets.Values.Clear(new ClearValuesRequest(), _spreadsheetId, $"{sheetName}!A:Z");
                await clearRequest.ExecuteAsync(cancellationToken);
                Console.WriteLine($"Cleared existing content from sheet '{sheetName}'");
            }
            catch (Exception ex)
            {
                // If clearing fails (e.g., sheet doesn't exist or is empty), that's okay
                // We'll just proceed with writing the new data
                Console.WriteLine($"Note: Sheet '{sheetName}' appears to be empty or new (no content to clear)");
            }
        }
    }
}
