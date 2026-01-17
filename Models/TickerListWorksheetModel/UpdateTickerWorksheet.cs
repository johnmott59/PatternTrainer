using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;


namespace CandlePatternML
{

    /// <summary>
    /// this is a model for the worksheet that contains a list of tickers and associated data
    /// </summary>
    public partial class TickerListWorksheetModel 
    {
  
        /// <summary>
        /// transfer the data from the model to the worksheet and update google sheets
        /// </summary>
        public void UpdateTickerWorksheet()
        {
            // for each ticker row locate that row in the worksheet and update the values

            foreach (var v in RowDataList)
            {
    

                if (v.oCandleModel == null) continue;
                if (v.oCandleModel.candles == null) continue;
                if (v.oCandleModel.candles.Count() < 15) continue;

                /*
                 * Get the volume ratio for the last 13 days
                 */
                double volumeRatio = 0.0;
                BigInteger avgVolume = 0;
                BigInteger mostrecentvolume = 0;
                // get the volume from the last 13 days. inver the list, skip the most recent and take 13
                var last13 = v.oCandleModel.candles.Reverse().Skip(1).Take(13).ToList();

                try
                {
                    if (last13.Count == 13)
                    {
                        BigInteger totalvolume = 0;
                        for (int i = 0; i < last13.Count; i++)
                        {
                            totalvolume += last13[i].volume;
                        }
                        avgVolume = totalvolume / last13.Count;
                        // get the most recent volume and compare it to the average of the last 13 days
                        mostrecentvolume = v.oCandleModel.candles[^1].volume;

                        volumeRatio = (double)mostrecentvolume / (double)avgVolume;
                    }
                } catch(Exception ex)
                {
                    Console.WriteLine($"Error calculating volume ratio for {v.Ticker}: {ex.Message}");
                }
              
          
               

                foreach (var row in oWorksheet.Rows)
                {
                    // catch null demark pivot models
                    if (v.oDemarkPivotModel == null)
                    {
                        continue;
                    }
                    if (row.GetValue("A").ToString() == v.Ticker)
                    {
                        // update the last close in column B
                        row.SetValue("B", v.LastClose);

                        // we want the most recent pivot to be lower than the one previous

                        row.SetValue("C", "");
                        row.SetValue("D", "");
                        row.SetValue("E", "");

                        if (v.oDemarkPivotModel.LatestPivotHigh != null && v.oDemarkPivotModel.NextToLastPivotHigh != null)
                        {
                            if (v.oDemarkPivotModel.LatestPivotHigh.Value < v.oDemarkPivotModel.NextToLastPivotHigh.Value)
                            {
                                row.SetValue("C", $"({v.oDemarkPivotModel.LatestPivotHigh?.dtCandle.ToString("MM/dd/yyyy")}) {v.oDemarkPivotModel.LatestPivotHigh?.Value}");
                                row.SetValue("D", $"({v.oDemarkPivotModel.NextToLastPivotHigh?.dtCandle.ToString("MM/dd/yyyy")}) {v.oDemarkPivotModel.NextToLastPivotHigh?.Value}");
                            }
                            // is there a pivot high trend break?
                            if (v.oDemarkPivotModel.PivotHighTrendBreakDate != null)
                            {
                                row.SetValue("E", $"({v.oDemarkPivotModel.PivotHighTrendBreakDate?.ToString("MM/dd/yyyy")})");
                            }
                        }

                        row.SetValue("F", "");
                        row.SetValue("G", "");
                        row.SetValue("H", "");
                        // we want the most recent pivot low to be higher than the one previous
                        if (v.oDemarkPivotModel.LatestPivotLow != null && v.oDemarkPivotModel.NextToLastPivotLow != null)
                        {
                            if (v.oDemarkPivotModel.LatestPivotLow.Value > v.oDemarkPivotModel.NextToLastPivotLow.Value)
                            {
                                row.SetValue("F", $"({v.oDemarkPivotModel.LatestPivotLow?.dtCandle.ToString("MM/dd/yyyy")}) {v.oDemarkPivotModel.LatestPivotLow?.Value}");
                                row.SetValue("G", $"({v.oDemarkPivotModel.NextToLastPivotLow?.dtCandle.ToString("MM/dd/yyyy")}) {v.oDemarkPivotModel.NextToLastPivotLow?.Value}");
                            }
                            // is there a pivot low trend break?
                            if (v.oDemarkPivotModel.PivotLowTrendBreakDate != null)
                            {
                                row.SetValue("H", $"({v.oDemarkPivotModel.PivotLowTrendBreakDate?.ToString("MM/dd/yyyy")})");
                            }
                        }

                        // volume ratio in column I
                        row.SetValue("K", volumeRatio.ToString("F2"));
#if false
                        for (int i=12; i <= 20; i++)
                        {
                            string val = v.ColumnData[i - 12];
                            row.SetValue(i, val);
                        }
#endif
                        // this is how you would set a formula in google sheets
                        //  string formula = $"=INDEX(GOOGLEFINANCE(\"{v.Ticker}\",\"close\",TODAY()-28,TODAY()-28,\"DAILY\"),2,2)";
                        //  row.SetValue("L", formula);
                    }
                }
            }

            // write this to google
            UpdateWorkSheet(oWorksheet);
        }


    }
}
