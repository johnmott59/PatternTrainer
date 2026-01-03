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

        /// <summary>
        /// Constructor for TwoBarModel
        /// </summary>
        /// <param name="setupInstance">The SetupInstancesModel instance</param>
        /// <param name="confidence">The confidence value</param>
        /// <param name="note">The note value</param>
        public TwoBarModel(SetupInstancesModel setupInstance, decimal confidence, string note)
        {
            SetupInstanceID = setupInstance.ID;
            Confidence = confidence;
            Note = note;
        }
    }
}

