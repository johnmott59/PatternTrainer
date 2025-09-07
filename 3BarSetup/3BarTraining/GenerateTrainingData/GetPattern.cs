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
        public ThreeBarPatternModel GetPattern()
        {
            return ThreeBarPatternModel.GetPattern(barList, this.pass);
        }
    }
}
