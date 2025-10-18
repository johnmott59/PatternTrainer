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
        public void SimpleWriteWorksheetExample(List<MLResult> resultList)
        {
            var googleSync = new WorkSheetGoogleSync(
         spreadsheetId: ConfigurationManager.AppSettings["spreadsheetid"],
         credentialsPath: "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json"
     );

            // Create and populate your worksheet
            var worksheet = new WorkSheet("Sheet1");
            worksheet.CreateColumn("Symbol");
            worksheet.CreateColumn("3Bar");
            worksheet.CreateColumn("Confidence");

            worksheet.SetValue(0, "A", DateTime.Today.ToShortDateString());

            worksheet.SetValue(1, "A", "Ticker");
            worksheet.SetValue(1, "B", "3 bar match");
            worksheet.SetValue(1, "C", "Confidence");

            for (int i=0; i < resultList.Count; i++)
            {
                worksheet.SetValue(i+2, "A", resultList[i].Ticker);
                worksheet.SetValue(i+2, "B", resultList[i].Success ? "Yes" : "No");
                worksheet.SetValue(i+2, "C", resultList[i].Confidence);
            }

            // Synchronize to Google Sheets
            bool success = googleSync.Synchronize(worksheet);
        }
    }
}
