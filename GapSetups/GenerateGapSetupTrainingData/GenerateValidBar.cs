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
        private LowHighModel GenerateValidBar()
        {

            // get a lower value
            double lowerValue = GetRandomDouble(validLowerBound, validUpperBound);

            // the lower value needs to be less than the upper bound minus the minimum bar size
            if (lowerValue >= validUpperBound - minBarSize)
            {
                lowerValue = validUpperBound - minBarSize;
            }
            // now get a higher value   
            double highervalue = GetRandomDouble(lowerValue, validUpperBound);

            return new LowHighModel(lowerValue, highervalue);
        }
    }
}
