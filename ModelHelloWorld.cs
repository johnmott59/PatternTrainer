using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using MIConvexHull;
using SchwabLib;
using SchwabLib.Models;

namespace CandlePatternML
{
    public class CandlePatternInput
    {
        public float A { get; set; }
        public float B { get; set; }
        public float C { get; set; }
        public float D { get; set; }
        public float E { get; set; }
        public float F { get; set; }
        public bool Label { get; set; }
    }
    public partial class Program
    {
        public void ModelHelloWorld()
        {
            // 1. Create MLContext
            var mlContext = new MLContext(seed: 1);

            // 2. Create sample training data
            var trainingData = new List<CandlePatternInput>
            {
                new CandlePatternInput { A = 1.0f, B = 1.2f, C = 0.8f, D = 1.1f, E = 1.15f, F = 0.95f, Label = true },
                new CandlePatternInput { A = 0.9f, B = 1.0f, C = 0.85f, D = 0.95f, E = 1.05f, F = 0.9f, Label = false },
                new CandlePatternInput { A = 1.1f, B = 1.3f, C = 1.0f, D = 1.2f, E = 1.25f, F = 1.05f, Label = true },
                new CandlePatternInput { A = 0.95f, B = 1.05f, C = 0.9f, D = 1.0f, E = 1.1f, F = 0.95f, Label = false },
                new CandlePatternInput { A = 1.2f, B = 1.35f, C = 1.1f, D = 1.3f, E = 1.4f, F = 1.15f, Label = true }
            };

            var trainingDataView = mlContext.Data.LoadFromEnumerable(trainingData);

            // 3. Estimate good hyperparameters
            var options = FastTreeConfigHelper.EstimateOptions(trainingData.Count, featureCount: 6);

            // 4. Define training pipeline
            var pipeline = mlContext.Transforms.Concatenate("Features", nameof(CandlePatternInput.A), nameof(CandlePatternInput.B),
                                                             nameof(CandlePatternInput.C), nameof(CandlePatternInput.D),
                                                             nameof(CandlePatternInput.E), nameof(CandlePatternInput.F))
                                               .Append(mlContext.BinaryClassification.Trainers.FastTree(options));

            // 5. Train the model
            var model = pipeline.Fit(trainingDataView);

            Console.WriteLine("✅ Model training complete.");

            // save the model to a file
            mlContext.Model.Save(model, trainingDataView.Schema, "c:\\work\\CandlePatternModel.zip");

            // load the model from a file
            ITransformer loadedModel;
            DataViewSchema modelSchema;

            using (var fileStream = new FileStream("c:\\work\\CandlePatternModel.zip", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = mlContext.Model.Load(fileStream, out modelSchema);
            }

            // create a prediction engine
            var predictionEngine = mlContext.Model.CreatePredictionEngine<CandlePatternInput, CandlePatternOutput>(loadedModel);

            //run a prediction
            var input = new CandlePatternInput
            {
                A = 1.0f,
                B = 1.2f,
                C = 0.9f,
                D = 1.1f,
                E = 1.15f,
                F = 0.95f
            };

            var result = predictionEngine.Predict(input);

            Console.WriteLine($"Prediction: {(result.IsMatch ? "MATCH" : "NO MATCH")}, Probability: {result.Probability:P1}");
        }
    }
}

