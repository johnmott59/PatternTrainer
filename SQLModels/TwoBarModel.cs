using System;

namespace CandlePatternML
{
    /// <summary>
    /// Model class for the TwoBar table
    /// </summary>
    public class TwoBarModel
    {
        public int ID { get; set; }
        public int SetupInstanceID { get; set; }
        public decimal? Confidence { get; set; }
        public string Note { get; set; }
    }
}

