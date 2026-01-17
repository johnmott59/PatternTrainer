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
        public DemarkPivotModel oDemarkPivotModel { get; set; }
  

    }
}
