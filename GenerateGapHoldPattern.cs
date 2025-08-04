using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using MIConvexHull;
using SchwabLib;
using SchwabLib.Models;

namespace CandlePatternML
{

#if false
    public static class PatternGenerator
    {
        private static readonly Random rand = new();

        public static List<(decimal high, decimal low)> GenerateGapHoldPattern(int length, bool pass, decimal slippagePercent = 0.007m)
        {
            if (length != 3 && length != 4)
                throw new ArgumentException("Length must be 3 or 4");

            while (true) // retry loop for pass validation
            {
                var pattern = new List<(decimal high, decimal low)>();

                // 🔹 1. Create the gap bar
                decimal gapLow = RandomDecimal(30m, 300m);
                decimal gapSize = RandomDecimal(0.5m, 20m);
                decimal gapHigh = gapLow + gapSize;

                pattern.Add((gapHigh, gapLow));

                // 🔹 2. Determine slippage direction
                bool allowUpperSlippage = rand.Next(2) == 0;

                // For a valid pattern
                decimal validLowerBound = allowUpperSlippage ? gapLow : gapLow - gapSize * slippagePercent;
                decimal validUpperBound = allowUpperSlippage ? gapHigh + gapSize * slippagePercent : gapHigh;

                decimal minBarSize = gapSize * 0.01m;

                // For pass enforcement tracking
                bool hasPointInUpperThird = false;

                // 🔹 3. Generate holding bars
                for (int i = 1; i < length; i++)
                {
                    decimal lowerBound = validLowerBound;
                    decimal upperBound = validUpperBound;

                    if (!pass && i == rand.Next(1, length)) // force at least one failure
                    {
                        if (allowUpperSlippage)
                        {
                            upperBound = gapLow - gapSize * 0.05m;
                            lowerBound = upperBound - gapSize;
                        }
                        else
                        {
                            lowerBound = gapHigh + gapSize * 0.05m;
                            upperBound = lowerBound + gapSize;
                        }
                    }

                    decimal barSize = RandomDecimal(minBarSize, gapSize);

                    decimal maxLow = upperBound - barSize;
                    if (maxLow < lowerBound) maxLow = lowerBound;

                    decimal barLow = RandomDecimal(lowerBound, maxLow);
                    decimal barHigh = barLow + barSize;

                    // Check if either high or low is inside the top 2/3 of the gap
                    if (pass)
                    {
                        decimal minRequiredY = gapLow + gapSize / 3m;
                        if ((barLow >= minRequiredY && barLow <= gapHigh) ||
                            (barHigh >= minRequiredY && barHigh <= gapHigh))
                        {
                            hasPointInUpperThird = true;
                        }
                    }

                    pattern.Add((barHigh, barLow));
                }

                if (pass && !hasPointInUpperThird)
                {
                    // Retry until we generate a valid "pass" pattern
                    continue;
                }
                if (pass)
                {
                    for (int j = 1; j < pattern.Count; j++)
                    {
                        var (barHigh, barLow) = pattern[j];

                        if (allowUpperSlippage)
                        {
                            if (barLow < gapLow || barHigh > gapHigh + gapSize * slippagePercent)
                                continue; // retry
                        }
                        else
                        {
                            if (barLow < gapLow - gapSize * slippagePercent || barHigh > gapHigh)
                                continue; // retry
                        }
                    }
                }

                return pattern;
            }
        }

        private static decimal RandomDecimal(decimal min, decimal max)
        {
            return (decimal)rand.NextDouble() * (max - min) + min;
        }
    }
#endif
}




