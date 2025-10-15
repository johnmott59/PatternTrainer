using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq; // Added for FirstOrDefault

namespace CandlePatternML
{
    /// <summary>
    /// Provides synchronization functionality to save WorkSheet data to Google Sheets
    /// </summary>
    public class WorkSheetGoogleSync
    {
        private readonly string _spreadsheetId;
        private readonly string _credentialsPath;
        private readonly string _tokenStorePath;
        private readonly string _applicationName;
        private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };

        public WorkSheetGoogleSync(string spreadsheetId, string credentialsPath, string tokenStorePath = "token-store", string applicationName = "PatternTrainer")
        {
            _spreadsheetId = spreadsheetId;
            _credentialsPath = credentialsPath;
            _tokenStorePath = tokenStorePath;
            _applicationName = applicationName;
        }

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

        /// <summary>
        /// Synchronizes the entire worksheet to Google Sheets (synchronous version)
        /// </summary>
        public bool Synchronize(WorkSheet worksheet)
        {
            return SynchronizeAsync(worksheet).GetAwaiter().GetResult();
        }

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

        /// <summary>
        /// Appends data to the end of an existing sheet (synchronous version)
        /// </summary>
        public bool Append(WorkSheet worksheet)
        {
            return AppendAsync(worksheet).GetAwaiter().GetResult();
        }

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

        /// <summary>
        /// Converts the 2D data array to Google Sheets format
        /// </summary>
        private IList<IList<object>> ConvertToGoogleSheetsFormat(object[,] dataArray)
        {
            var rows = dataArray.GetLength(0);
            var cols = dataArray.GetLength(1);
            
            var result = new List<IList<object>>();
            
            for (int row = 0; row < rows; row++)
            {
                var rowData = new List<object>();
                for (int col = 0; col < cols; col++)
                {
                    var value = dataArray[row, col];
                    rowData.Add(value ?? "");
                }
                result.Add(rowData);
            }
            
            return result;
        }

        /// <summary>
        /// Gets the Google Sheets service
        /// </summary>
        private async Task<SheetsService> GetSheetsServiceAsync(CancellationToken cancellationToken)
        {
            // Load OAuth client and authorize
            using var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read);
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                cancellationToken,
                new FileDataStore(_tokenStorePath, true)
            );

            var service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName,
            });

            return service;
        }

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

        /// <summary>
        /// Creates a new sheet in the spreadsheet if it doesn't exist (synchronous version)
        /// </summary>
        public bool EnsureSheetExists(string sheetName)
        {
            return EnsureSheetExistsAsync(sheetName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Reads data from a Google Sheet and returns it as a WorkSheet model
        /// </summary>
        /// <param name="sheetName">The name of the sheet to read from</param>
        /// <param name="range">Optional range to read (e.g., "A1:Z100" or "A:Z"). If null, reads the entire sheet</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A WorkSheet object containing the read data, or null if reading failed</returns>
        public async Task<WorkSheet> ReadAsync(string sheetName, string range = null, CancellationToken cancellationToken = default)
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
                var worksheet = ConvertFromGoogleSheetsFormat(response.Values, sheetName, true);
                
                Console.WriteLine($"Successfully read {worksheet.RowCount} rows and {worksheet.ColumnCount} columns from sheet '{sheetName}'");
                return worksheet;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from sheet '{sheetName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Reads data from a Google Sheet and returns it as a WorkSheet model (synchronous version)
        /// </summary>
        /// <param name="sheetName">The name of the sheet to read from</param>
        /// <param name="range">Optional range to read (e.g., "A1:Z100" or "A:Z"). If null, reads the entire sheet</param>
        /// <returns>A WorkSheet object containing the read data, or null if reading failed</returns>
        public WorkSheet Read(string sheetName, string range = null)
        {
            return ReadAsync(sheetName, range).GetAwaiter().GetResult();
        }

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

        /// <summary>
        /// Reads data from a Google Sheet with explicit header control (synchronous version)
        /// </summary>
        /// <param name="sheetName">The name of the sheet to read from</param>
        /// <param name="range">Optional range to read (e.g., "A1:Z100" or "A:Z"). If null, reads the entire sheet</param>
        /// <param name="hasHeaders">Whether the first row contains headers</param>
        /// <returns>A WorkSheet object containing the read data, or null if reading failed</returns>
        public WorkSheet ReadWithHeaders(string sheetName, string range = null, bool hasHeaders = true)
        {
            return ReadWithHeadersAsync(sheetName, range, hasHeaders).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Converts Google Sheets data format to WorkSheet model
        /// </summary>
        private WorkSheet ConvertFromGoogleSheetsFormat(IList<IList<object>> values, string sheetName, bool hasHeaders = true)
        {
            var worksheet = new WorkSheet(sheetName);
            
            if (values == null || values.Count == 0)
                return worksheet;
            
            // Determine the number of columns (use the maximum row length)
            int maxColumns = values.Max(row => row?.Count ?? 0);
            
            // Create columns first
            for (int col = 0; col < maxColumns; col++)
            {
                string header = $"Column{GetColumnLetter(col)}";
                worksheet.CreateColumn(header);
            }
            
            if (hasHeaders && values.Count > 0 && values[0] != null)
            {
                // Set column headers from first row
                for (int col = 0; col < Math.Min(values[0].Count, maxColumns); col++)
                {
                    string headerValue = values[0][col]?.ToString();
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        worksheet.Columns[col].Header = headerValue;
                    }
                    else
                    {
                        worksheet.Columns[col].Header = $"Column{GetColumnLetter(col)}";
                    }
                }
                
                // Start data from row 1 (skip header row)
                for (int row = 1; row < values.Count; row++)
                {
                    var dataRow = worksheet.CreateRow();
                    if (values[row] != null)
                    {
                        for (int col = 0; col < Math.Min(values[row].Count, maxColumns); col++)
                        {
                            dataRow.SetValue(col, values[row][col]);
                        }
                    }
                }
            }
            else
            {
                // No headers, treat all rows as data
                for (int row = 0; row < values.Count; row++)
                {
                    var dataRow = worksheet.CreateRow();
                    if (values[row] != null)
                    {
                        for (int col = 0; col < Math.Min(values[row].Count, maxColumns); col++)
                        {
                            dataRow.SetValue(col, values[row][col]);
                        }
                    }
                }
            }
            
            return worksheet;
        }

        /// <summary>
        /// Converts a column index to a column letter (A, B, C, etc.)
        /// </summary>
        private static string GetColumnLetter(int columnIndex)
        {
            if (columnIndex < 0) throw new ArgumentException("Column index must be non-negative");
            
            string result = "";
            while (columnIndex >= 0)
            {
                result = (char)('A' + (columnIndex % 26)) + result;
                columnIndex = (columnIndex / 26) - 1;
            }
            return result;
        }

        /// <summary>
        /// Debug method to print WorkSheet structure for troubleshooting
        /// </summary>
        public void PrintWorkSheetDebugInfo(WorkSheet worksheet, string title = "WorkSheet Debug Info")
        {
            Console.WriteLine($"\n=== {title} ===");
            Console.WriteLine($"Sheet Name: {worksheet.SheetName}");
            Console.WriteLine($"Column Count: {worksheet.ColumnCount}");
            Console.WriteLine($"Row Count: {worksheet.RowCount}");
            
            Console.WriteLine("\nHeaders:");
            for (int col = 0; col < worksheet.ColumnCount; col++)
            {
                var column = worksheet.GetColumn(col);
                Console.WriteLine($"  Column {col}: '{column?.Header}'");
            }
            
            Console.WriteLine("\nData Rows:");
            for (int row = 0; row < worksheet.RowCount; row++)
            {
                var dataRow = worksheet.GetRow(row);
                Console.Write($"  Row {row}: ");
                if (dataRow != null)
                {
                    for (int col = 0; col < worksheet.ColumnCount; col++)
                    {
                        var value = dataRow.GetValue(col);
                        Console.Write($"{value ?? "null"} ");
                    }
                }
                else
                {
                    Console.Write("(null row)");
                }
                Console.WriteLine();
            }
            
            // Show what GetDataArray() would produce
            var dataArray = worksheet.GetDataArray();
            Console.WriteLine($"\nGetDataArray() produces {dataArray.GetLength(0)} rows x {dataArray.GetLength(1)} columns:");
            for (int row = 0; row < dataArray.GetLength(0); row++)
            {
                Console.Write($"  Array Row {row}: ");
                for (int col = 0; col < dataArray.GetLength(1); col++)
                {
                    Console.Write($"{dataArray[row, col] ?? "null"} ");
                }
                Console.WriteLine();
            }
        }
    }
}
