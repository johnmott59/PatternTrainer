using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;


namespace CandlePatternML
{

    public partial class TickerListWorksheetModel
    {
        public WorkSheet oWorksheet { get; set; }
        public List<TickerListRowDataModel> RowDataList = new List<TickerListRowDataModel>();
    }
}
