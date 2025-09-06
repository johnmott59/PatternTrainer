using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace CandlePatternML
{
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using Google.Apis.Sheets.v4.Data;
    using Google.Apis.Util.Store;

    public partial class Program
    {
        private const string SpreadsheetId = "1gnkMIbxJN6EVEO18Yj9-4KPrX7MV2qXl7X5KGi4P6o8";
        private const string SheetName = "Sheet1";
        private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };

        public static void UpdateSheet()
        {
            // Load OAuth client and block until authorized (one-time browser prompt)
            using var stream = new FileStream("C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json", FileMode.Open, FileAccess.Read);
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("token-store", true)
            ).Result; // block here for simplicity in debugging

            var service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "StockSignals",
            });

            // --- Append a row (sync) ---
            var newRow = new ValueRange
            {
                Values = new List<IList<object>> {
                new List<object> { "AAPL", 189.55, DateTime.UtcNow.ToString("u"), "sync append" }
            }
            };
            var append = service.Spreadsheets.Values.Append(newRow, SpreadsheetId, $"{SheetName}!A1:D1");
            append.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResult = append.Execute();
            Console.WriteLine($"Appended to: {appendResult.TableRange}");

            // --- Overwrite a specific range (sync) ---
            var overwrite = new ValueRange
            {
                Values = new List<IList<object>> {
                new List<object> { "MSFT", 420.10, DateTime.UtcNow.ToString("u"), "sync update" }
            }
            };
            var update = service.Spreadsheets.Values.Update(overwrite, SpreadsheetId, $"{SheetName}!A2:D2");
            update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var updateResult = update.Execute();
            Console.WriteLine($"Updated range: {updateResult.UpdatedRange}");
        }
    }


}