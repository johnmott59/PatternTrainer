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
        public void DoThreeBarRun(GetCandleModel model)
        {
            // 1. Create MLContext
            var mlContext = new MLContext(seed: 1);

            // load the model from a file
            ITransformer loadedModel;
            DataViewSchema modelSchema;

            using (var fileStream = new FileStream("c:\\work\\ThreeBarModel.zip", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = mlContext.Model.Load(fileStream, out modelSchema);
            }

            // create a prediction engine
            var predictionEngine = mlContext.Model.CreatePredictionEngine<ThreeBarPatternModel, CandlePatternOutput>(loadedModel);

            List<ThreeBarPatternModel> patternModelList = new List<ThreeBarPatternModel>();
            List<DateTime> dtList = new List<DateTime>();
            List<CandlePatternOutput> outputList = new List<CandlePatternOutput>();
           
            for (int i=1; i < model.candles.Count() - 4; i++)
            {
                // do we have a gap?
                if (model.candles[i].open <= model.candles[i - 1].close) continue;

                // this is a gap
                Console.WriteLine($"found gap at  {model.candles[i].dtDotNet.ToShortDateString()}");
                ThreeBarPatternModel input = new ThreeBarPatternModel
                {
                    GapBarLowHigh = new LowHighModel(model.candles[i].low, model.candles[i].high),
                    GapCandle = model.candles[i],
                   
                    Hold1BarLowHigh = new LowHighModel(model.candles[i + 1].low, model.candles[i + 1].high),
                    Hold1Candle = model.candles[i + 1],

                    Hold2BarLowHigh = new LowHighModel(model.candles[i + 2].low, model.candles[i + 2].high),
                    Hold2Candle = model.candles[i + 2],

                };
                // add in extra features that emphasize some aspects of the pattern
                input.SetExtraFeatures();

                // run the prediction
                var result = predictionEngine.Predict(input);

                List<(double, double)> bars = new List<(double, double)>
                {
                    (input.GapBarLowHigh.Low, input.GapBarLowHigh.High),
                    (input.Hold1BarLowHigh.Low, input.Hold1BarLowHigh.High),
                    (input.Hold2Low, input.Hold2High),
                };

                if (!GenerateTraining.CheckThreeBarPattern(bars, GenerateTraining.DefaultSlippagePercent, out string reason))
                {
                 //   Console.WriteLine($"Skipping index {i} due to invalid pattern: {reason}");
               //     continue;
                } else
                {
                    Console.WriteLine($"Valid pattern at index {i}: {reason}");
                }

                Console.WriteLine($"Prediction for index {i}: {(result.IsMatch ? "MATCH" : "NO MATCH")}, Probability: {result.Probability:P1}");

                if (result.IsMatch)
                {
                    patternModelList.Add(input);
                    dtList.Add(model.candles[i].dtDotNet);
                    outputList.Add(result);
                }
            }

            ThreeBarSvgReporter.WriteSvgHtml("c:\\work\\3bar.html",patternModelList,dtList, outputList);

        }
    }
}

