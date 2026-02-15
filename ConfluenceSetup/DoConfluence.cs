using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using MIConvexHull;
using SchwabLib;
using SchwabLib.Models;
using System.Diagnostics.Eventing.Reader;
using System.DirectoryServices.ActiveDirectory;
using System.Windows.Input;
using System.Data;

namespace CandlePatternML
{
    public partial class Program
    {
        public MLResult DoConfluence(GetCandleModel model)
        {
            double ProximityATRMultiplier = 1.5; // This can be adjusted based on how close you want the price to be to the indicators

            // get the candles for the last 6 months.
            var candles = model.candles.Where(c => c.dtDotNet > DateTime.Now.AddMonths(-6)).ToList();
            
            // get studies for this time

            List<double?> emaSlowSeries = SchwabLib.Studies.CalculateEMA(candles, 200);
            List<double?> emaFastSeries = SchwabLib.Studies.CalculateEMA(candles, 34);
            List<double?> atrSeries = SchwabLib.Studies.CalculateATR(candles, 14);

            int conflenceCount = 0;
            int lastindex = candles.Count - 1;

            if (atrSeries[lastindex] == null) return new MLResult(model, false, 0) ;

            double currentPrice = (double) candles[lastindex].close; 
            
            double currentATR = atrSeries[lastindex].Value;
            double proximityThreshold = ProximityATRMultiplier * currentATR;

            bool nearEMASlow = Math.Abs((double) (currentPrice - emaSlowSeries[lastindex])) <= proximityThreshold;
            bool nearEMAFast = Math.Abs((double) (currentPrice - emaFastSeries[lastindex])) <= proximityThreshold;

            int confluenceCount =  (nearEMASlow ? 1 : 0) +
                                    (nearEMAFast ? 1 : 0) ;

            if (confluenceCount >= 1) return new MLResult(model, true, confluenceCount);

            return new MLResult(model, false, 0);

        }
    }
}

