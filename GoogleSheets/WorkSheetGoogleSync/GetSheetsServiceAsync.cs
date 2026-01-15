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
        /// <summary>
        /// Gets the Google Sheets service
        /// </summary>
        private async Task<SheetsService> GetSheetsServiceAsync(CancellationToken cancellationToken)
        {
            string _tokenStorePath = Path.Combine(
     Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
     "GoogleTokenStore"
 );

            Console.WriteLine($"TokenStorePath = '{_tokenStorePath}'");
            Console.WriteLine($"Absolute = '{Path.GetFullPath(_tokenStorePath)}'");

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
    }
}
