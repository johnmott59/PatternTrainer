using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
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
        public WorkSheetGoogleSync(string spreadsheetId, string credentialsPath, string tokenStorePath = "token-store", string applicationName = "PatternTrainer")
        {
            _spreadsheetId = spreadsheetId;
            _credentialsPath = credentialsPath;
            _tokenStorePath = tokenStorePath;
            _applicationName = applicationName;

            UserCredential credential;

            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { SheetsService.Scope.Spreadsheets },
                    "user",
                    CancellationToken.None,
                    new FileDataStore("PatternTrainer.Tokens", true)
                ).Result;
            }

            _service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "PatternTrainer"
            });

        }

    }
}
