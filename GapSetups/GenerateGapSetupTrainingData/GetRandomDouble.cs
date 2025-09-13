using SchwabLib;
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
        private double GetRandomDouble(double min, double max)
        {
            return rand.NextDouble() * (max - min) + min;
        }
    }
}
