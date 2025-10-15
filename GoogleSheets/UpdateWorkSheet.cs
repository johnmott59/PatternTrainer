using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace CandlePatternML
{

    public partial class Program
    {
        public void UpdateWorkSheet(WorkSheet worksheet)
        {
            var googleSync = new WorkSheetGoogleSync(
                spreadsheetId: ConfigurationManager.AppSettings["spreadsheetid"],
                credentialsPath: "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json"
                );
            // Synchronize to Google Sheets
            bool success = googleSync.Synchronize(worksheet);
        }
    }
}
