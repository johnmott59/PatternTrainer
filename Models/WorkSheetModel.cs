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
        public List<string> Headers { get; set; } = new List<string> { "Ticker", "3Bar Match", "3Bar Confidence", "2Bar Match", "2Bar Confidence" };
      
        public List<WorkSheetRowData> Rows { get; set; } = new List<WorkSheetRowData>();
    
        public void AddTicker(string ticker, MLResult ml3BarResults, MLResult ml2BarResults)
        {
            Rows.Add( new WorkSheetRowData { Ticker = ticker, ml3BarResults = ml3BarResults, ml2BarResults = ml2BarResults } );
        }   
    }
    public class WorkSheetRowData
    {
        public string Ticker { get; set; }
        public MLResult ml3BarResults { get; set; }
        public MLResult ml2BarResults { get; set; }

    }
}
