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

        /// <summary>
        /// Constructor for FourBarModel
        /// </summary>
        /// <param name="setupInstance">The SetupInstancesModel instance</param>
        /// <param name="confidence">The confidence value</param>
        /// <param name="note">The note value</param>
        public FourBarModel(SetupInstancesModel setupInstance, decimal confidence, string note)
        {
            SetupInstanceID = setupInstance.ID;
            Confidence = confidence;
            Note = note;
        }
    }
}

