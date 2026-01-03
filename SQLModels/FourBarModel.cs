using System;

namespace CandlePatternML
{
    /// <summary>
    /// Model class for the FourBar table
    /// </summary>
    public class FourBarModel
    {
        public int ID { get; set; }
        public int SetupInstanceID { get; set; }
        public decimal? Confidence { get; set; }
        public string Note { get; set; }
    }
}

