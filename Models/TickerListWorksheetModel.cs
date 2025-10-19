using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;


namespace CandlePatternML
{
    /// <summary>
    /// here is the model for each row
    /// </summary>
    public partial class TickerListRowDataModel
    {
        public string Ticker { get; set; }
        public decimal LastClose { get; set; }

        public PivotPoint LatestPivotHigh { get; set; }
        public PivotPoint NextToLastPivotHigh { get; set; }

        public PivotPoint LatestPivotLow { get; set; }
        public PivotPoint NextToLastPivotLow { get; set; }

    }
    /// <summary>
    /// this is a model for the worksheet that contains a list of tickers and associated data
    /// </summary>
    public partial class TickerListWorksheetModel : WorksheetBase
    {
        public WorkSheet oWorksheet { get; set; }
        public List<TickerListRowDataModel> RowDataList = new List<TickerListRowDataModel>();

        // constructor, load the list of tickers worksheet.
        public TickerListWorksheetModel()
        {
            this.oWorksheet = ReadWorkSheet("Tickers").Result;

            // read the list of tickers and create ticker data models. so far they only contain the ticker symbol
            foreach (var v in oWorksheet.Rows)
            {
                RowDataList.Add(new TickerListRowDataModel() { Ticker = v.GetValue(0).ToString() });
            }
        }

        public void SaveChanges()
        {
            UpdateWorkSheet(oWorksheet);
        }

        // constructor, load the list of tickers worksheet.
        public TickerListWorksheetModel(WorkSheet oWorksheet)
        {
            this.oWorksheet = oWorksheet;

            // read the list of tickers and create ticker data models. so far they only contain the ticker symbol
            foreach (var v in oWorksheet.Rows)
            {
                RowDataList.Add(new TickerListRowDataModel() { Ticker = v.GetValue(0).ToString() });
            }
        }
        /// <summary>
        /// transfer the data from the model to the worksheet and update google sheets
        /// </summary>
        public void UpdateTickerWorksheet()
        {
            // for each ticker row locate that row in the worksheet and update the values

            foreach (var v in RowDataList)
            {
                foreach (var row in oWorksheet.Rows)
                {
                    if (row.GetValue("A").ToString() == v.Ticker)
                    {
                        // update the last close in column B
                        row.SetValue("B", v.LastClose);

                        // we want the most recent pivot to be lower than the one previous

                        row.SetValue("C", "");
                        row.SetValue("D", "");

                        if (v.LatestPivotHigh != null && v.NextToLastPivotHigh != null)
                        {
                            if (v.LatestPivotHigh.Value < v.NextToLastPivotHigh.Value)
                            {
                                row.SetValue("C", $"({v.LatestPivotHigh?.dtCandle.ToString("MM/dd/yyyy")}) {v.LatestPivotHigh?.Value}");
                                row.SetValue("D", $"({v.NextToLastPivotHigh?.dtCandle.ToString("MM/dd/yyyy")}) {v.NextToLastPivotHigh?.Value}");
                            }
                        }

                        row.SetValue("E", "");
                        row.SetValue("F", "");
                        // we want the most recent pivot low to be higher than the one previous
                        if (v.LatestPivotLow != null && v.NextToLastPivotLow != null)
                        {
                            if (v.LatestPivotLow.Value > v.NextToLastPivotLow.Value)
                            {
                                row.SetValue("E", $"({v.LatestPivotLow?.dtCandle.ToString("MM/dd/yyyy")}) {v.LatestPivotLow?.Value}");
                                row.SetValue("F", $"({v.NextToLastPivotLow?.dtCandle.ToString("MM/dd/yyyy")}) {v.NextToLastPivotLow?.Value}");
                            }
                        }
                    }
                }
            }

            // write this to google
            UpdateWorkSheet(oWorksheet);
        }


    }
}
