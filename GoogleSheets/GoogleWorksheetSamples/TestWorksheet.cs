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
        public void TestWorkSheet()
        {
            var googleSync = new WorkSheetGoogleSync(
         spreadsheetId: ConfigurationManager.AppSettings["spreadsheetid"],
         credentialsPath: "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json"
     );

            // Create and populate your worksheet
            var worksheet = new WorkSheet("Sheet1");
            worksheet.CreateColumn("Symbol");
            worksheet.CreateColumn("Price");
            worksheet.SetValue(0, "A", "AMZN");
            worksheet.SetValue(0, "B", 189.55);

            // Synchronize to Google Sheets
            bool success = googleSync.Synchronize(worksheet);
        }
    }
}
