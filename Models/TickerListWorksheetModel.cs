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
        public GetCandleModel oCandleModel { get; set; }

        public PivotPointModel LatestPivotHigh { get; set; }
        public PivotPointModel NextToLastPivotHigh { get; set; }
        public Candle? PivotHighTrendBreak { get; set; }

        // if we don't already have a trend line break we want to know at what price the trend line would be broken
        // this field will be used to estimate what price would need to break in the next
        // day's candle in order to break the trend line, and can be used to set an alert
        public decimal? ForecastedPivotHighTrendBreak { get; set; }

        public PivotPointModel LatestPivotLow { get; set; }
        public PivotPointModel NextToLastPivotLow { get; set; }
        public Candle? PivotLowTrendBreak { get; set; }

        // if we don't already have a trend line break we want to know at what price the trend line would be broken
        // this field will be used to estimate what price would need to break in the next
        // day's candle in order to break the trend line, and can be used to set an alert
        public decimal? ForecastedPivotLowTrendBreak { get; set; }

        // given a list of candle data, find the first candle that breaks the pivot high downtrend

        public void FindPivotHighBreak(List<Candle> candles)
        {
            // locate the candle after the latest pivot high
            if (LatestPivotHigh == null || NextToLastPivotHigh == null)
            {
                return;
            }
            int latestPivotIndex = candles.FindIndex(c => c.dtDotNet == LatestPivotHigh.dtCandle);
            if (latestPivotIndex == -1 || latestPivotIndex + 1 >= candles.Count)
            {
                return;
            }

            int candleCountinRun = NextToLastPivotHigh.Index - LatestPivotHigh.Index;
            double slope = (double)(LatestPivotHigh.Value - NextToLastPivotHigh.Value)
                / (double)candleCountinRun;

            // for each candle after the last pivot high, compute the estimated value along 
            // the slope and see if the candle close breaks that value
            double CurrentEndValue = (double)LatestPivotHigh.Value;
            for (int i= latestPivotIndex + 1; i < candles.Count; i++)
            {
                // see where the downtrend line is at this candle
                CurrentEndValue += slope;
                // see if this candle close breaks that value
                if (candles[i].close > (decimal)CurrentEndValue)
                {
                    // we have a break of the downtrend
                    PivotHighTrendBreak = candles[i];
                    break;
                }
            }

            // if we don't have a trend line that has not been broken yet
            // compute the forecasted value for the next candle
            if (PivotHighTrendBreak == null)
            {
                ForecastedPivotHighTrendBreak = (decimal)(CurrentEndValue + slope);
            }
        }

        public void FindPivotLowBreak(List<Candle> candles)
        {
            // locate the candle after the latest pivot low
            if (LatestPivotLow == null || NextToLastPivotLow == null)
            {
                return;
            }
            int latestPivotIndex = candles.FindIndex(c => c.dtDotNet == LatestPivotLow.dtCandle);
            if (latestPivotIndex == -1 || latestPivotIndex + 1 >= candles.Count)
            {
                return;
            }

            int candleCountinRun = NextToLastPivotLow.Index - LatestPivotLow.Index;
            double slope = (double)(LatestPivotLow.Value - NextToLastPivotLow.Value)
                / (double)candleCountinRun;

            // for each candle after the last pivot low, compute the estimated value along 
            // the slope and see if the candle close breaks that value
            double CurrentEndValue = (double)LatestPivotLow.Value;
            for (int i = latestPivotIndex + 1; i < candles.Count; i++)
            {
                // see where the downtrend line is at this candle
                CurrentEndValue += slope;
                // see if this candle close breaks that value
                if (candles[i].close < (decimal)CurrentEndValue)
                {
                    // we have a break of the downtrend
                    PivotLowTrendBreak = candles[i];
                    break;
                }
            }

            // compute the forecasted value for the next candle
            if (PivotLowTrendBreak == null)
            {
                ForecastedPivotLowTrendBreak = (decimal)(CurrentEndValue + slope);
            }
        }

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

                        if (v.LatestPivotHigh != null && v.NextToLastPivotHigh != null)
                        {
                            if (v.LatestPivotHigh.Value < v.NextToLastPivotHigh.Value)
                            {
                                row.SetValue("C", $"({v.LatestPivotHigh?.dtCandle.ToString("MM/dd/yyyy")}) {v.LatestPivotHigh?.Value}");
                                row.SetValue("D", $"({v.NextToLastPivotHigh?.dtCandle.ToString("MM/dd/yyyy")}) {v.NextToLastPivotHigh?.Value}");
                            }
                            // is there a pivot high trend break?
                            if (v.PivotHighTrendBreak != null)
                            {
                                row.SetValue("E", $"({v.PivotHighTrendBreak?.dtDotNet.ToString("MM/dd/yyyy")}) {v.PivotHighTrendBreak?.close}");
                            }
                        }

                        row.SetValue("F", "");
                        row.SetValue("G", "");
                        row.SetValue("H", "");
                        // we want the most recent pivot low to be higher than the one previous
                        if (v.LatestPivotLow != null && v.NextToLastPivotLow != null)
                        {
                            if (v.LatestPivotLow.Value > v.NextToLastPivotLow.Value)
                            {
                                row.SetValue("F", $"({v.LatestPivotLow?.dtCandle.ToString("MM/dd/yyyy")}) {v.LatestPivotLow?.Value}");
                                row.SetValue("G", $"({v.NextToLastPivotLow?.dtCandle.ToString("MM/dd/yyyy")}) {v.NextToLastPivotLow?.Value}");
                            }
                            // is there a pivot low trend break?
                            if (v.PivotLowTrendBreak != null)
                            {
                                row.SetValue("H", $"({v.PivotLowTrendBreak?.dtDotNet.ToString("MM/dd/yyyy")}) {v.PivotLowTrendBreak?.close}");
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
