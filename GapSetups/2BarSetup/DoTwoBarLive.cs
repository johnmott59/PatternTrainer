using MIConvexHull;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Numerics;

namespace CandlePatternML
{
    public partial class Program
    {
        public MLResult DoTwoBarLive(MLEngineWrapper mlEngine,GetCandleModel model)
        {
            List<string> noticeList = new List<string>();
            // get the last 3 days of candles. We need 2 bars for the pattern
            // and one day prior to see if there is a gap

            int length = model.candles.Length;
            List<Candle> PatternCandles = model.candles.Skip(length - 3).ToList();

            double margin = 0.001;   // tolerance for comparing highs and lows
            Candle bar0 = PatternCandles[0]; // bar before the gap
                                             // is the day before gap a red day?
            if (bar0.close < bar0.open)
            {
                noticeList.Add("Pre Gap Red");
            }
            Candle bar1 = PatternCandles[1]; // gap day
            Candle bar2 = PatternCandles[2]; // bar after gap (candidate inside bar)

            bool GapDayGreen = bar1.close > bar1.open;
            
            double GapSize = Math.Log((double)bar1.open / (double)bar0.close);

            if ((GapSize > .0068 && GapDayGreen == true) || GapSize > .012)
            { 
                if (GapDayGreen == false)
                {
                    noticeList.Add("Gap Day Red");
                }
      
                double upperLimit = (double) bar1.high * (1 + margin);
                double lowerLimit = (double) bar1.low * (1 - margin);
                
                bool BelowUpperLimit = (double)bar2.high <= upperLimit;
                bool AboveLowerLimit = (double)bar2.low >= lowerLimit;

                if (BelowUpperLimit && AboveLowerLimit)
                {
                    // inside bar
                    return new MLResult(model, true, 1, string.Join("\n",noticeList));
                }
            }
            
            return new MLResult(model,false, 0);

        }
    }
}

