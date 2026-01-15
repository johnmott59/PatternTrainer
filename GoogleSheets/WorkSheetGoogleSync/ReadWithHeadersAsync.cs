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


    }
}
