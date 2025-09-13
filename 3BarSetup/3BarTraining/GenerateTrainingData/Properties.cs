using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Transactions;
using System.Windows.Input.Manipulations;


namespace CandlePatternML
{
    public partial class GenerateTrainingData
    {
        public const double DefaultSlippagePercent = .01;

        private static readonly Random rand = new();

        protected double gapBarLow { get; set; }
        protected double gapBarSize { get; set; }
        protected double gapBarHigh { get; set; }

        // generated bars
        protected List<LowHighModel> barList = new List<LowHighModel>();

        // determine the min and max bar size. allow for slippage
        protected double minBarSize;

        // determine valid upper and lower bounds
        protected double validLowerBound;
        protected double validUpperBound;

        protected bool pass; // true if this is a valid pattern
    }
}
