using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using MIConvexHull;
using SchwabLib;
using SchwabLib.Models;

namespace CandlePatternML
{
    public partial class Program
    {
        public static class FastTreeConfigHelper
        {
            public static FastTreeBinaryTrainer.Options EstimateOptions(int trainingSize, int featureCount)
            {
                int minExamplesPerLeaf = Math.Max(1, Math.Min(5, trainingSize / 5));
                int numberOfLeaves = Math.Max(2, Math.Min(64, trainingSize / (minExamplesPerLeaf * 2)));
                int numberOfTrees = Math.Max(10, Math.Min(500, trainingSize / (numberOfLeaves * 2)));


                return new FastTreeBinaryTrainer.Options
                {
                    NumberOfLeaves = Math.Max(2, numberOfLeaves),
                    NumberOfTrees = Math.Max(1, numberOfTrees),
                    MinimumExampleCountPerLeaf = minExamplesPerLeaf,
                    LearningRate = 0.2f,
                    LabelColumnName = "Label",
                    FeatureColumnName = "Features"
                };
            }
        }
    }
}

