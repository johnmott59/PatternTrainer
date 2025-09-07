using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Transactions;
using System.Windows.Input.Manipulations;


namespace CandlePatternML
{
    public partial class GenerateTrainingData
    {
        /// <summary>
        /// This an optional debugging routine to validate that a 3-bar pattern meets the criteria.
        /// bars[0] = gap bar (high, low). bars[1], bars[2] = holding bars.
        /// Returns true if both holding bars stay within [gapLow - pct*range, gapHigh + pct*range].
        /// </summary>
        public static bool CheckThreeBarPattern(
            IReadOnlyList<(double low, double high)> bars,
            double slippagePercent,
            out string reason)
        {
            reason = "OK";

            // Validate highs/lows ordering
            for (int i = 0; i < bars.Count; i++)
            {
                var (l, h) = bars[i];
                if (double.IsNaN(h) || double.IsNaN(l) || double.IsInfinity(h) || double.IsInfinity(l))
                {
                    reason = $"Bar {i} has NaN/Infinity.";
                    return false;
                }
                if (h <= l)
                {
                    reason = $"Bar {i} has high <= low.";
                    return false;
                }
            }

            // 2) Gap bar defines the base range
            var (gapLow, gapHigh) = bars[0];
            double gapRange = gapHigh - gapLow;
            if (gapRange <= 0)
            {
                reason = "Gap bar range must be > 0.";
                return false;
            }

            if (slippagePercent < 0)
            {
                reason = "slippagePercent must be >= 0.";
                return false;
            }

            // 3) Compute allowed bounds with symmetric slippage. the slippage is 
            // based on the size
            double allowedLower = gapLow - gapLow * slippagePercent;
            double allowedUpper = gapHigh + gapLow * slippagePercent;


            // 4) Validate holding bars
            for (int i = 1; i <= 2; i++)
            {
                var (l, h) = bars[i];
                if (l < allowedLower || h > allowedUpper)
                {
                    reason = $"Bar {i} violates bounds. Allowed [{allowedLower}, {allowedUpper}], got low={l}, high={h}.";
                    return false;
                }
            }

            return true;
        }
    }
}
