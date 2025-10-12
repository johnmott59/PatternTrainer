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
        public MLResult DoThreeBarLive(MLEngineWrapper mlEngine,GetCandleModel model)
        {
            // get the last 4 days of candles. We need 3 bars for the pattern
            // and one day prior to see if there is a gap

            int length = model.candles.Length;
            List<Candle> PatternCandles = model.candles.Skip(length - 4).ToList();

            // do we have a gap?
            if (PatternCandles[1].open <= PatternCandles[0].close) return new MLResult(model,false, 0);
            // did the close of today fill the gap?
            if (PatternCandles[1].close <= PatternCandles[0].close) return new MLResult(model,false, 0);

            // do the hold bars close below the low of the gap bar?
            // only the low can dip below the gap bar low
            if (PatternCandles[2].close <= PatternCandles[0].low) return new MLResult(model, false, 0);
            if (PatternCandles[3].close <= PatternCandles[0].low) return new MLResult(model,  false, 0);
#if true
            if (PatternCandles[2].open <= PatternCandles[0].low) return new MLResult(model, false, 0);
            if (PatternCandles[3].open <= PatternCandles[0].low) return new MLResult(model, false, 0);

            if (PatternCandles[2].high <= PatternCandles[0].low) return new MLResult(model, false, 0);
            if (PatternCandles[3].high <= PatternCandles[0].low) return new MLResult(model, false, 0);
#endif

            // check to see if the body of candle 3 is below the body of candles 1 and 2
            BodyOfCandleModel b1 = new BodyOfCandleModel(PatternCandles[1]);
            BodyOfCandleModel b2 = new BodyOfCandleModel(PatternCandles[2]);
            BodyOfCandleModel b3 = new BodyOfCandleModel(PatternCandles[3]);

            if(b3 < b2 && b3 < b1) return new MLResult(model, false, 0);
#if false
// use for graphing the patterns
            List<ThreeBarPatternModel> patternModelList = new List<ThreeBarPatternModel>();
            List<DateTime> dtList = new List<DateTime>();
            List<CandlePatternOutput> outputList = new List<CandlePatternOutput>();
#endif

           // Console.WriteLine($"found gap at  {PatternCandles[1].dtDotNet.ToShortDateString()}");
            ThreeBarPatternModel input = new ThreeBarPatternModel
            {
                GapBarLowHigh = new LowHighModel(PatternCandles[1].low, PatternCandles[1].high),
                GapCandle = PatternCandles[1],

                Hold1BarLowHigh = new LowHighModel(PatternCandles[2].low, PatternCandles[2].high),
                Hold1Candle = PatternCandles[2],

                Hold2BarLowHigh = new LowHighModel(PatternCandles[3].low, PatternCandles[3].high),
                Hold2Candle = PatternCandles[3],

            };
            // add in extra features that emphasize some aspects of the pattern
            input.SetExtraFeatures();
            // run the prediction
            var result = mlEngine.predictionEngine3Bar.Predict(input);

       //     Console.WriteLine($"Prediction for {model.symbol} : {(result.IsMatch ? "MATCH" : "NO MATCH")}, Probability: {result.Probability:P1}");

#if false
// graph the successful patterns
            if (result.IsMatch)
            {
                patternModelList.Add(input);
                dtList.Add(PatternCandles[1].dtDotNet);
                outputList.Add(result);

                ThreeBarSvgReporter.WriteSvgHtml($"c:\\work\\3bar_{model.symbol}.html", patternModelList, dtList, outputList);
            }
#endif

            return new MLResult(model,result.IsMatch, result.Probability);
        }
    }
}

