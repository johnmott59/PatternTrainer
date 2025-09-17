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
        public void DoFiveBarTraining(decimal minPrice=30,decimal maxPrice=300)
        {
            //// gapBarLow = RandomDouble(30, 300);
            // 1. Create MLContext
            var mlContext = new MLContext(seed: 1);

            var trainingData = new List<FiveBarPatternModel>();

            bool valid = true;
            for (int i = 0; i < 20000; i++,valid = !valid) { 

                GenerateGapSetupTrainingData p = new GenerateGapSetupTrainingData(5, valid,minPrice,maxPrice);

                trainingData.Add(p.GetFiveBarPattern());

            }

            var trainingDataView = mlContext.Data.LoadFromEnumerable(trainingData);

            // 3. Estimate good hyperparameters
            var options = FastTreeConfigHelper.EstimateOptions(trainingData.Count, featureCount: 18);

            // 4. Define training pipeline
            var pipeline = mlContext.Transforms.Concatenate("Features", 
                        nameof(FiveBarPatternModel.GapLow), 
                        nameof(FiveBarPatternModel.GapHigh),
                        nameof(FiveBarPatternModel.Hold1Low), 
                        nameof(FiveBarPatternModel.Hold1High),
                        nameof(FiveBarPatternModel.Hold2Low), 
                        nameof(FiveBarPatternModel.Hold2High),
                        nameof(FiveBarPatternModel.Hold3Low),
                        nameof(FiveBarPatternModel.Hold3High),
                        nameof(FiveBarPatternModel.Hold4Low),
                        nameof(FiveBarPatternModel.Hold4High),

                        nameof(FiveBarPatternModel.Hold1HighMoreThanGapLow),
                        nameof(FiveBarPatternModel.Hold1LowLessThanGapHigh),
                        
                        nameof(FiveBarPatternModel.Hold2HighMoreThanGapLow),
                        nameof(FiveBarPatternModel.Hold2LowLessThanGapHigh),
                        
                        nameof(FiveBarPatternModel.Hold3HighMoreThanGapLow),
                        nameof(FiveBarPatternModel.Hold3LowLessThanGapHigh),
                        
                        nameof(FiveBarPatternModel.Hold4HighMoreThanGapLow),
                        nameof(FiveBarPatternModel.Hold4LowLessThanGapHigh)
                        )
                .Append(mlContext.BinaryClassification.Trainers.FastTree(options));

            // 5. Train the model
            var model = pipeline.Fit(trainingDataView);

            Console.WriteLine("✅ Model training complete.");

            // save the model to a file
            mlContext.Model.Save(model, trainingDataView.Schema, "c:\\work\\FiveBarModel.zip");

         
        }
    }
}

