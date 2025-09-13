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
        public void DoThreeBarTraining(decimal minPrice=30,decimal maxPrice=300)
        {
            //// gapBarLow = RandomDouble(30, 300);
            // 1. Create MLContext
            var mlContext = new MLContext(seed: 1);

            var trainingData = new List<ThreeBarPatternModel>();

            bool valid = true;
            for (int i = 0; i < 20000; i++,valid = !valid) { 

                GenerateGapSetupTrainingData p = new GenerateGapSetupTrainingData(3, valid,minPrice,maxPrice);

                trainingData.Add(p.GetThreeBarPattern());

            }

            var trainingDataView = mlContext.Data.LoadFromEnumerable(trainingData);

            // 3. Estimate good hyperparameters
            var options = FastTreeConfigHelper.EstimateOptions(trainingData.Count, featureCount: 6);

            // 4. Define training pipeline
            var pipeline = mlContext.Transforms.Concatenate("Features", 
                        nameof(ThreeBarPatternModel.GapLow), 
                        nameof(ThreeBarPatternModel.GapHigh),
                        nameof(ThreeBarPatternModel.Hold1Low), 
                        nameof(ThreeBarPatternModel.Hold1High),
                        nameof(ThreeBarPatternModel.Hold2Low), 
                        nameof(ThreeBarPatternModel.Hold2High),
                        nameof(ThreeBarPatternModel.Hold1HighMoreThanGapLow),
                        nameof(ThreeBarPatternModel.Hold1LowLessThanGapHigh),
                        nameof(ThreeBarPatternModel.Hold2HighMoreThanGapLow),
                        nameof(ThreeBarPatternModel.Hold2LowLessThanGapHigh)
                        )
                .Append(mlContext.BinaryClassification.Trainers.FastTree(options));

            // 5. Train the model
            var model = pipeline.Fit(trainingDataView);

            Console.WriteLine("✅ Model training complete.");

            // save the model to a file
            mlContext.Model.Save(model, trainingDataView.Schema, "c:\\work\\ThreeBarModel.zip");

         
        }
    }
}

