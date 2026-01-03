using System;

namespace CandlePatternML
{
    /// <summary>
    /// Model class for the TrendBreaks table
    /// </summary>
    public class TrendBreaksModel
    {
        public int ID { get; set; }
        public int SetupInstanceID { get; set; }
        public DateTime? TrendBreakDate { get; set; }
        public decimal? ProjectedBreakValue { get; set; }
    }
}

