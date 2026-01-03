using System;

namespace CandlePatternML
{
    /// <summary>
    /// Model class for the SetupInstances table
    /// </summary>
    public class SetupInstancesModel
    {
        public int ID { get; set; }
        public string Ticker { get; set; }
        public DateTime DateCreated { get; set; }
    }
}

