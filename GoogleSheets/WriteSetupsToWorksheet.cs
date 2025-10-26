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
        public void WriteSetupsToWorksheet(SetupWorkSheetModel results)
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

            worksheet.SetValue(0, "A", DateTime.Today.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());

            // place the column names in the first row


            // now place the data in the rows below
            int rowindex = 1; // start at row 2 (row 0 is date, row 1 is column names)
            foreach (var v in results.Rows)
            {
                // if there are no success tests skip this
                if (!v.ml2BarResults.Success 
                    && !v.ml3BarResults.Success 
                    && !v.ml4BarResults.Success
                    && !v.ml5BarResults.Success
                    && !v.RSI4Results.Success
                    ) continue;
                
                worksheet.SetValue(rowindex, "A",v.Ticker);

                /*
                 * If there is a high trend line see if we have either break of the line
                 * or a projection of when it will be broken
                 */
                if (v.DemarkPivots.TrendHighBreakDate != null)
                {
                    worksheet.SetValue(rowindex, "B", v.DemarkPivots.TrendHighBreakDate?.ToShortDateString());
                } else if (v.DemarkPivots.ProjectedTrendHighBreakValue != null)
                {
                    worksheet.SetValue(rowindex, "C", v.DemarkPivots.ProjectedTrendHighBreakValue?.ToString("F2"));
                }

                if (v.ml2BarResults.Success)
                {
                    worksheet.SetValue(rowindex, "D", v.ml2BarResults.Success);
                    worksheet.SetValue(rowindex, "E", v.ml2BarResults.Notice);
                }

                if (v.ml3BarResults.Success)
                {
                    worksheet.SetValue(rowindex, "F", v.ml3BarResults.Confidence.ToString("F2"));
                }

                if (v.ml4BarResults.Success)
                {
                    worksheet.SetValue(rowindex, "G", v.ml4BarResults.Confidence.ToString("F2"));
                }

                if (v.ml5BarResults.Success)
                {
                    worksheet.SetValue(rowindex, "H", v.ml5BarResults.Confidence.ToString("F2"));
                }

                if (v.RSI4Results.Success)
                {
                    worksheet.SetValue(rowindex, "I", v.RSI4Results.Confidence.ToString("F2"));
                }
                rowindex++;
            }
 

            // Synchronize to Google Sheets
            bool success = googleSync.Synchronize(worksheet);
        }
    }
}
