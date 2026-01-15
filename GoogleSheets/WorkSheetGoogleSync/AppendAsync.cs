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
        /// Appends data to the end of an existing sheet
        /// </summary>
        public async Task<bool> AppendAsync(WorkSheet worksheet, CancellationToken cancellationToken = default)
        {
            try
            {
                var service = await GetSheetsServiceAsync(cancellationToken);

                // Ensure the sheet exists before trying to append to it
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
                    Console.WriteLine("No data to append");
                    return true;
                }

                // Convert to Google Sheets format
                var values = ConvertToGoogleSheetsFormat(dataArray);

                // Append the data
                var range = $"{worksheet.SheetName}!A1";
                var valueRange = new ValueRange
                {
                    Values = values
                };

                var appendRequest = service.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

                var result = await appendRequest.ExecuteAsync(cancellationToken);

                Console.WriteLine($"Successfully appended worksheet '{worksheet.SheetName}' to Google Sheets");
                Console.WriteLine($"Appended to range: {result.TableRange}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error appending worksheet: {ex.Message}");
                return false;
            }
        }
    }
}
