using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using PatternTrainer;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace CandlePatternML
{

    public partial class Program
    {
        public void WriteWorkSheet2(WorkSheetModel results)
        {
            var googleSync = new WorkSheetGoogleSync(
         spreadsheetId: ConfigurationManager.AppSettings["spreadsheetid"],
         credentialsPath: "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json"
     );

            // Create and populate your worksheet
            var worksheet = new WorkSheet("Sheet1");
            foreach (var v in results.Headers)
            {
                worksheet.CreateColumn(v);
            }

            worksheet.SetValue(0, "A", DateTime.Today.ToShortDateString());

            // place the column names in the first row
           
            for (int i=0; i < results.Headers.Count; i++)
            {
                worksheet.SetValue(1, i, results.Headers[i]);
            }

            // now place the data in the rows below
            int rowindex = 2; // start at row 2 (row 0 is date, row 1 is column names)
            foreach (var v in results.Rows)
            {
                worksheet.SetValue(rowindex, "A",v.Ticker);
                worksheet.SetValue(rowindex, "B", v.ml3BarResults.Success);
                worksheet.SetValue(rowindex, "C", v.ml3BarResults.Confidence);
                worksheet.SetValue(rowindex, "D", v.ml2BarResults.Success);
                worksheet.SetValue(rowindex, "E", v.ml2BarResults.Confidence);
                rowindex++;
            }
 

            // Synchronize to Google Sheets
            bool success = googleSync.Synchronize(worksheet);
        }
    }
}
