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


#if false
// this isn't working, its an attempt to develop stuff to read formualas. the json wasn't correct somehow
            GoogleCredential credential;
            try
            {
                using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential
                        .FromStream(stream)
                        .CreateScoped(SheetsService.Scope.Spreadsheets);
                }

                _service = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "PatternTrainer"
                });

            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

#endif
        }

    }
}
