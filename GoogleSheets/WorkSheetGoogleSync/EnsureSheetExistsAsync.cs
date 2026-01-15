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
        /// Creates a new sheet in the spreadsheet if it doesn't exist
        /// </summary>
        public async Task<bool> EnsureSheetExistsAsync(string sheetName, CancellationToken cancellationToken = default)
        {
            try
            {
                var service = await GetSheetsServiceAsync(cancellationToken);

                // Get the spreadsheet to check existing sheets
                var spreadsheet = await service.Spreadsheets.Get(_spreadsheetId).ExecuteAsync(cancellationToken);

                // Check if the sheet already exists
                var existingSheet = spreadsheet.Sheets?.FirstOrDefault(s => s.Properties.Title == sheetName);
                if (existingSheet != null)
                {
                    Console.WriteLine($"Sheet '{sheetName}' already exists");
                    return true;
                }

                // Create a new sheet
                var addSheetRequest = new Request
                {
                    AddSheet = new AddSheetRequest
                    {
                        Properties = new SheetProperties
                        {
                            Title = sheetName
                        }
                    }
                };

                var batchUpdateRequest = new BatchUpdateSpreadsheetRequest
                {
                    Requests = new List<Request> { addSheetRequest }
                };

                var result = await service.Spreadsheets.BatchUpdate(batchUpdateRequest, _spreadsheetId).ExecuteAsync(cancellationToken);

                Console.WriteLine($"Successfully created new sheet '{sheetName}'");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating sheet '{sheetName}': {ex.Message}");
                Console.WriteLine("This might be due to insufficient permissions or invalid spreadsheet ID");
                return false;
            }
        }
    }
}
