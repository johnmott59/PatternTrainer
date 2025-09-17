using CandlePatternML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternTrainer
{
    public class WorkSheetModel
    {
        public List<string> Headers { get; set; } = 
            new List<string> { 
                "Ticker",
                "2Bar Match", "2Bar Confidence",
                "3Bar Match", "3Bar Confidence",
                "4Bar Match", "4Bar Confidence",
                "5Bar Match", "5Bar Confidence"
            };
      
        public List<WorkSheetRowData> Rows { get; set; } = new List<WorkSheetRowData>();
    
        public void AddTicker(string ticker,
            MLResult ml2BarResults,
            MLResult ml3BarResults, 
            MLResult ml4BarResults,
            MLResult ml5BarResults
            )
        {
            Rows.Add( new WorkSheetRowData { 
                Ticker = ticker, 
                ml2BarResults = ml2BarResults, 
                ml3BarResults = ml3BarResults,
                ml4BarResults = ml4BarResults,
                ml5BarResults = ml5BarResults
            } );
        }   
    }
    public class WorkSheetRowData
    {
        public string Ticker { get; set; }
        public MLResult ml3BarResults { get; set; }
        public MLResult ml2BarResults { get; set; }
        public MLResult ml4BarResults { get; set; }
        public MLResult ml5BarResults { get; set; }

    }
}
