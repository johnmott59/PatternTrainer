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
        /// Synchronizes the entire worksheet to Google Sheets
        /// </summary>
        public async Task<bool> SynchronizeAsync(WorkSheet worksheet, CancellationToken cancellationToken = default)
        {
            try
            {
                var service = await GetSheetsServiceAsync(cancellationToken);

                // Ensure the sheet exists before trying to write to it
                bool sheetExists = await EnsureSheetExistsAsync(worksheet.SheetName, cancellationToken);
                if (!sheetExists)
                {
                    Console.WriteLine($"Error: Could not create or verify sheet '{worksheet.SheetName}'");
                    return false;
                }

                // Get the data as a 2D array
                var dataArray = worksheet.GetDataArray();

                if (dataArray.Length == 0)
                {
                    Console.WriteLine("No data to synchronize");
                    return true;
                }

                // Convert to Google Sheets format
                var values = ConvertToGoogleSheetsFormat(dataArray);

                // Clear existing content first (optional - you can remove this if you want to append)
                await ClearSheetAsync(service, worksheet.SheetName, cancellationToken);

                // Write the data
                var range = $"{worksheet.SheetName}!A1";
                var valueRange = new ValueRange
                {
                    Values = values
                };

                var updateRequest = service.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

                var result = await updateRequest.ExecuteAsync(cancellationToken);

                Console.WriteLine($"Successfully synchronized worksheet '{worksheet.SheetName}' to Google Sheets");
                Console.WriteLine($"Updated range: {result.UpdatedRange}");
                Console.WriteLine($"Updated rows: {result.UpdatedRows}");
                Console.WriteLine($"Updated columns: {result.UpdatedColumns}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error synchronizing worksheet: {ex.Message}");
                return false;
            }
        }
    }
}
