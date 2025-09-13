using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Transactions;
using System.Windows.Input.Manipulations;


namespace CandlePatternML
{
    public partial class GenerateGapSetupTrainingData
    {
        protected void GenerateMixedValidAndInvalidBars(int totalBars)
        {
            // generate at least two bars, because we need at least one valid and one invalid
            if (totalBars < 2)
                throw new ArgumentException("n must be >= 2");

            int numInvalid = rand.Next(1, totalBars); // at least one invalid

            int numValid = totalBars - numInvalid;

            var bars = new List<LowHighModel>();

            // Insert invalid bars first
            for (int i = 0; i < numInvalid; i++)
                bars.Add(GenerateInvalidBar());

            // Then valid bars
            for (int i = 0; i < numValid; i++)
                bars.Add(GenerateValidBar());

            // Shuffle so valid/invalid are mixed
            ShuffleBars(bars);

            barList.AddRange(bars);
        }
    }
}
