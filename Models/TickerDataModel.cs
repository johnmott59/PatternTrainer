using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;


namespace CandlePatternML
{

    public partial class TickerDataModel
    {
        public string Ticker { get; set; }
        public decimal RecentClose { get; set; }

        public PivotPoint LatestPivotHigh { get; set; }
        public PivotPoint NextToLastPivotHigh { get; set; }

        public PivotPoint LatestPivotLow { get; set; }
        public PivotPoint NextToLastPivotLow{ get; set; }

    }
}
