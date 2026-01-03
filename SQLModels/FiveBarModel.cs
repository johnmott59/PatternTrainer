using System;

namespace CandlePatternML
{
    /// <summary>
    /// Model class for the FiveBar table
    /// </summary>
    public class FiveBarModel
    {
        public int ID { get; set; }
        public int SetupInstanceID { get; set; }
        public decimal? Confidence { get; set; }
        public string Note { get; set; }
    }
}

