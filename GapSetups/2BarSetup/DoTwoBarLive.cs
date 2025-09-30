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
        public MLResult DoTwoBarLive(MLEngineWrapper mlEngine,GetCandleModel model)
        {
            // get the last 3 days of candles. We need 2 bars for the pattern
            // and one day prior to see if there is a gap

            int length = model.candles.Length;
            List<Candle> PatternCandles = model.candles.Skip(length - 3).ToList();

            // do we have a gap?
            if (PatternCandles[1].open <= PatternCandles[0].close) return new MLResult(model,false, 0);
            // did the close of today fill the gap?
            if (PatternCandles[1].close <= PatternCandles[0].close) return new MLResult(model,false, 0);

            // does the hold bar hold above the gap bar? 
            // only the low can dip below the gap bar low
            if (PatternCandles[2].close <= PatternCandles[0].low) return new MLResult(model,false, 0);
            if (PatternCandles[2].high <= PatternCandles[0].low) return new MLResult(model, false, 0);
            if (PatternCandles[2].open <= PatternCandles[0].low) return new MLResult(model, false, 0);

#if false
// use for graphing the patterns
            List<ThreeBarPatternModel> patternModelList = new List<ThreeBarPatternModel>();
            List<DateTime> dtList = new List<DateTime>();
            List<CandlePatternOutput> outputList = new List<CandlePatternOutput>();
#endif

            Console.WriteLine($"found gap at  {PatternCandles[1].dtDotNet.ToShortDateString()}");
            TwoBarPatternModel input = new TwoBarPatternModel
            {
                GapBarLowHigh = new LowHighModel(PatternCandles[1].low, PatternCandles[1].high),
                GapCandle = PatternCandles[1],

                Hold1BarLowHigh = new LowHighModel(PatternCandles[2].low, PatternCandles[2].high),
                Hold1Candle = PatternCandles[2],

            };
            // add in extra features that emphasize some aspects of the pattern
            input.SetExtraFeatures();
            // run the prediction
            var result = mlEngine.predictionEngine2Bar.Predict(input);

            Console.WriteLine($"Prediction for {model.symbol} : {(result.IsMatch ? "MATCH" : "NO MATCH")}, Probability: {result.Probability:P1}");


            return new MLResult(model,result.IsMatch, result.Probability);
        }
    }
}

