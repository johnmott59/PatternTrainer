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

        /// <summary>
        /// Constructor for TrendBreaksModel
        /// </summary>
        /// <param name="setupInstance">The SetupInstancesModel instance</param>
        /// <param name="trendBreakDate">The trend break date</param>
        /// <param name="projectedBreakValue">The projected break value</param>
        public TrendBreaksModel(SetupInstancesModel setupInstance, DateTime trendBreakDate, decimal projectedBreakValue)
        {
            SetupInstanceID = setupInstance.ID;
            TrendBreakDate = trendBreakDate;
            ProjectedBreakValue = projectedBreakValue;
        }
    }
}

