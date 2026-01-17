using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;


namespace CandlePatternML
{

    public partial class TickerListWorksheetModel : WorksheetBase
    {

        // constructor, load the list of tickers worksheet.
        public TickerListWorksheetModel()
        {
            this.oWorksheet = ReadWorkSheet("Tickers").Result;

            // read the list of tickers and create ticker data models. so far they only contain the ticker symbol
            foreach (var v in oWorksheet.Rows)
            {
                var newrow = new TickerListRowDataModel();
                newrow.Ticker = v.GetValue(0).ToString();

                RowDataList.Add(newrow);
            }
        }
    }
}
