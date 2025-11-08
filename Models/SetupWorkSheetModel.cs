using CandlePatternML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternTrainer
{
    public class SetupWorkSheetModel
    {
       

        public List<string> Headers { get; set; } = 
            new List<string> { 
                "Ticker",
                "Trend Break Date",
                "Projected Break Value",
                "2Bar Match", 
                "Note",
                "3Bar Confidence",
                 "Note",
                "4Bar Confidence",
                 "Note",
                "5Bar Confidence",
                 "Note",
                "RSI4 Match"
            };
      
        public List<WorkSheetRowData> Rows { get; set; } = new List<WorkSheetRowData>();
    
        public void AddTicker(string ticker,
            DemarkPivotModel DemarkPivots,
            MLResult ml2BarResults,
            MLResult ml3BarResults, 
            MLResult ml4BarResults,
            MLResult ml5BarResults,
            MLResult RSI4Results
            )
        {
            Rows.Add( new WorkSheetRowData { 
                Ticker = ticker, 
                DemarkPivots= DemarkPivots,
                ml2BarResults = ml2BarResults, 
                ml3BarResults = ml3BarResults,
                ml4BarResults = ml4BarResults,
                ml5BarResults = ml5BarResults,
                RSI4Results = RSI4Results
            } );
        }   
    }
    public class WorkSheetRowData
    {
        public string Ticker { get; set; }
        public DemarkPivotModel DemarkPivots { get; set; }
        public MLResult ml3BarResults { get; set; }
        public MLResult ml2BarResults { get; set; }
        public MLResult ml4BarResults { get; set; }
        public MLResult ml5BarResults { get; set; }
        public MLResult RSI4Results { get; set; }

    }
}
