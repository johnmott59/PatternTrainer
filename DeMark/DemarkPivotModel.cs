using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;


namespace CandlePatternML
{
    public class DemarkPivotModel
    {
        public PivotPointModel LatestPivotHigh { get; set; }
        public PivotPointModel NextToLastPivotHigh { get; set; }
        public DateTime? TrendHighBreakDate { get; set; }
        public decimal? ProjectedTrendHighBreakValue { get; set; }

        public PivotPointModel LatestPivotLow { get; set; }
        public PivotPointModel NextToLastPivotLow { get; set; }
        public DateTime? TrendLowBreakDate { get; set; }
        public decimal? ProjectedTrendLowBreakValue { get; set; }

    }

}
