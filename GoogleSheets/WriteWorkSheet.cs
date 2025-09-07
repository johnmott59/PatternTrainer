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
        public void WriteWorkSheet(List<ThreeBarResult> resultList)
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

            worksheet.SetValue(0, "A", "Ticker");
            worksheet.SetValue(0, "B", "3 bar match");
            worksheet.SetValue(0, "C", "Confidence");

            for (int i=0; i < resultList.Count; i++)
            {
                worksheet.SetValue(i+1, "A", resultList[i].Ticker);
                worksheet.SetValue(i+1, "B", resultList[i].Success ? "Yes" : "No");
                worksheet.SetValue(i+1, "C", resultList[i].Confidence);
            }

            // Synchronize to Google Sheets
            bool success = googleSync.Synchronize(worksheet);
        }
    }
}
