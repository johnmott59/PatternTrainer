using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using MIConvexHull;
using SchwabLib;
using SchwabLib.Models;
using System.Diagnostics.Eventing.Reader;
using System.DirectoryServices.ActiveDirectory;

namespace CandlePatternML
{
    public partial class Program
    {
        public MLResult DoRSI4Live(GetCandleModel model)
        {
            // get the 200 day SMA. 

            List<double?> sma200 = SchwabLib.Studies.CalculateSMA(model.candles.ToList(), 200);

            // get the 4 period RSI

            List<double?> rsi4 = SchwabLib.Studies.CalculateRSI(model.candles.ToList(), 4);

            // are we above the 200 day and is the RSI < 25?

            if ((double)model.candles[^1].close > sma200[^1] && rsi4[^1] <= 25)
            {
                return new MLResult(model, true, 1);
            }

            return new MLResult(model, false, 0);
        }
    }
}

