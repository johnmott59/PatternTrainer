﻿using Microsoft.ML;
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
        public GetCandleModel oCandleModel { get; set; }
        public DemarkPivotModel oDemarkPivotModel { get; set; }

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
                    }
                }
            }

            // write this to google
            UpdateWorkSheet(oWorksheet);
        }


    }
}
