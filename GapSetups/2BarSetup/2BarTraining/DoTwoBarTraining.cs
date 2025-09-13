using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using MIConvexHull;
using SchwabLib;
using SchwabLib.Models;
using System.Diagnostics.Eventing.Reader;

namespace CandlePatternML
{

    public partial class Program
    {
        public void DoTwoBarTraining(decimal minPrice=30,decimal maxPrice=300)
        {
            //// gapBarLow = RandomDouble(30, 300);
            // 1. Create MLContext
            var mlContext = new MLContext(seed: 1);

            var trainingData = new List<TwoBarPatternModel>();

            bool valid = true;
            for (int i = 0; i < 20000; i++,valid = !valid) { 

                GenerateGapSetupTrainingData p = new GenerateGapSetupTrainingData(2, valid,minPrice,maxPrice);

                trainingData.Add(p.GetTwoBarPattern());

            }

            var trainingDataView = mlContext.Data.LoadFromEnumerable(trainingData);

            // 3. Estimate good hyperparameters
            var options = FastTreeConfigHelper.EstimateOptions(trainingData.Count, featureCount: 6);

            // 4. Define training pipeline
            var pipeline = mlContext.Transforms.Concatenate("Features", 
                        nameof(TwoBarPatternModel.GapLow), 
                        nameof(TwoBarPatternModel.GapHigh),
                        nameof(TwoBarPatternModel.Hold1Low), 
                        nameof(TwoBarPatternModel.Hold1High),
                        nameof(TwoBarPatternModel.Hold1HighMoreThanGapLow),
                        nameof(TwoBarPatternModel.Hold1LowLessThanGapHigh)
                        )
                .Append(mlContext.BinaryClassification.Trainers.FastTree(options));

            // 5. Train the model
            var model = pipeline.Fit(trainingDataView);

            Console.WriteLine("✅ Model training complete.");

            // save the model to a file
            mlContext.Model.Save(model, trainingDataView.Schema, "c:\\work\\TwoBarModel.zip");

         
        }
    }
}

