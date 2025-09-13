using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Transactions;
using System.Windows.Input.Manipulations;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace CandlePatternML
{
    public partial class GenerateTrainingData
    {
        public ThreeBarPatternModel GetThreeBarPattern()
        {
            ThreeBarPatternModel pattern = new ThreeBarPatternModel()
            {
                GapBarLowHigh = barList[0],
                Hold1BarLowHigh = barList[1],
                Hold2BarLowHigh = barList[2],
                Label = pass // true if this is a valid pattern
            };

            pattern.SetExtraFeatures(); // set the extra features

            return pattern;
            
        }
    }
}
