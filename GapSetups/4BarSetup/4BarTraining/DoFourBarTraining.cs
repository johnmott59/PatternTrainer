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
        public void DoFourBarTraining(decimal minPrice=30,decimal maxPrice=300)
        {
            //// gapBarLow = RandomDouble(30, 300);
            // 1. Create MLContext
            var mlContext = new MLContext(seed: 1);

            var trainingData = new List<FourBarPatternModel>();

            bool valid = true;
            for (int i = 0; i < 20000; i++,valid = !valid) { 

                GenerateGapSetupTrainingData p = new GenerateGapSetupTrainingData(4, valid,minPrice,maxPrice);

                trainingData.Add(p.GetFourBarPattern());

            }

            var trainingDataView = mlContext.Data.LoadFromEnumerable(trainingData);

            // 3. Estimate good hyperparameters
            var options = FastTreeConfigHelper.EstimateOptions(trainingData.Count, featureCount: 14);

            // 4. Define training pipeline
            var pipeline = mlContext.Transforms.Concatenate("Features", 
                        nameof(FourBarPatternModel.GapLow), 
                        nameof(FourBarPatternModel.GapHigh),
                        nameof(FourBarPatternModel.Hold1Low), 
                        nameof(FourBarPatternModel.Hold1High),
                        nameof(FourBarPatternModel.Hold2Low), 
                        nameof(FourBarPatternModel.Hold2High),
                        nameof(FourBarPatternModel.Hold3Low),
                        nameof(FourBarPatternModel.Hold3High),
                        nameof(FourBarPatternModel.Hold1HighMoreThanGapLow),
                        nameof(FourBarPatternModel.Hold1LowLessThanGapHigh),
                        nameof(FourBarPatternModel.Hold2HighMoreThanGapLow),
                        nameof(FourBarPatternModel.Hold2LowLessThanGapHigh),
                        nameof(FourBarPatternModel.Hold3HighMoreThanGapLow),
                        nameof(FourBarPatternModel.Hold3LowLessThanGapHigh)
                        )
                .Append(mlContext.BinaryClassification.Trainers.FastTree(options));

            // 5. Train the model
            var model = pipeline.Fit(trainingDataView);

            Console.WriteLine("✅ Model training complete.");

            // save the model to a file
            mlContext.Model.Save(model, trainingDataView.Schema, "c:\\work\\FourBarModel.zip");

         
        }
    }
}

