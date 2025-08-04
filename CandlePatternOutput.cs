using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using MIConvexHull;
using SchwabLib;
using SchwabLib.Models;

namespace CandlePatternML
{

        public class CandlePatternOutput
        {
            [ColumnName("PredictedLabel")]
            public bool IsMatch { get; set; }

            public float Score { get; set; }
            public float Probability { get; set; }
        }
    }


