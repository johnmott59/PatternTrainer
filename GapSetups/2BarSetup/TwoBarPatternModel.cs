using Microsoft.ML.Data;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;


namespace CandlePatternML
{
   
    /// <summary>
    /// This model will be fed into the ML. It has properties that will be
    /// referenced and properties that won't be referenced. it has to be flat,
    /// not containing other objects. The properties that are ignored by ML are
    /// marked by [NoColumn] and let us use this model to hold this data and 
    /// do other work
    /// </summary>
    public class TwoBarPatternModel
    {
        [NoColumn]
        public Candle GapCandle { get; set; } // the gap candle

        [NoColumn]
        public LowHighModel GapBarLowHigh { get; set; }
      
        public float GapLow {   get {   return GapBarLowHigh.Low;  }}
        public float GapHigh{   get {   return GapBarLowHigh.High; }}

        [NoColumn]
        public Candle Hold1Candle { get; set; } // holding bar 1

        [NoColumn]
        public LowHighModel Hold1BarLowHigh { get; set; } // holding bar 1 
        public float Hold1Low { get { return Hold1BarLowHigh.Low; } }
        public float Hold1High { get { return Hold1BarLowHigh.High; } }

        [NoColumn]
        public Candle Hold2Candle { get; set; } // holding bar 2

        [NoColumn]
        public LowHighModel Hold2BarLowHigh { get; set; } // holding bar 2
        public float Hold2Low { get { return Hold2BarLowHigh.Low; } }
        public float Hold2High { get { return Hold2BarLowHigh.High; } }

        public float Hold1LowLessThanGapHigh { get; set; }
        public float Hold1HighMoreThanGapLow { get; set; }

        public float Hold2LowLessThanGapHigh { get; set; }
        public float Hold2HighMoreThanGapLow { get; set; }


        /// <summary>
        /// Set extra features to emphasize some aspects of the pattern.
        /// The more of these we can do the better the model will be able to learn
        /// and its a way to flag edge conditions that could be inferred from the 
        /// data but gives a way to be explicit about them.
        /// </summary>

        public void SetExtraFeatures()
        {
            Hold1LowLessThanGapHigh = Hold1BarLowHigh.Low < GapBarLowHigh.High ? 1.0f : 0.0f;
            Hold1HighMoreThanGapLow = Hold1BarLowHigh.High > GapBarLowHigh.Low ? 1.0f : 0.0f;
            Hold2LowLessThanGapHigh = Hold2Low < GapBarLowHigh.High ? 1.0f : 0.0f;
            Hold2HighMoreThanGapLow = Hold2High > GapBarLowHigh.Low ? 1.0f : 0.0f;
        }

#if false
        // perform a quick validation. this doesn't replace the ML but is part of 
        // training and leargning
        public bool Validate(double slippagePercent, out string reason)
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
#endif
        public bool Label { get; set; }
#if false
        public static ThreeBarPatternModel GetPattern(List<LowHighModel> bars, bool valid)
        {
            ThreeBarPatternModel pattern = new ThreeBarPatternModel()
            {
                GapBarLowHigh = bars[0],
             //   GapLow = (float)bars[0].Low,
             //   GapHigh = (float)bars[0].High,
             Hold1BarLowHigh = bars[1],
              //  Hold1Low = (float)bars[1].Low,
              //  Hold1High = (float)bars[1].High,
              Hold2BarLowHigh = bars[2],


                Label = valid // true if this is a valid pattern

            };

            pattern.SetExtraFeatures(); // set the extra features

            // find the overlap
            // pattern.Overlap = (float) FindOverlap(bars);

            return pattern;
        }


        /// <summary>
        /// Calculates the overlap ratio of multiple ranges.
        /// Returns the area of the intersection divided by the sum of all individual areas.
        /// </summary>
        /// <param name="ranges">List of (low, high) tuples representing ranges</param>
        /// <returns>Overlap ratio as a double, or 0 if no overlap</returns>
        public static double FindOverlap(List<(double, double)> ranges)
        {
            if (ranges == null || ranges.Count == 0)
                return 0.0;

            // Find the intersection of all ranges
            double maxLow = double.MinValue;
            double minHigh = double.MaxValue;

            foreach (var (low, high) in ranges)
            {
                if (low > high)
                    return 0.0; // Invalid range

                maxLow = Math.Max(maxLow, low);
                minHigh = Math.Min(minHigh, high);
            }

            // Check if there's any overlap
            if (maxLow >= minHigh)
                return 0.0; // No overlap

            // Calculate overlap area
            double overlapArea = minHigh - maxLow;

            // Calculate sum of all individual areas
            double totalArea = 0.0;
            foreach (var (low, high) in ranges)
            {
                totalArea += (high - low);
            }

            // Return overlap ratio
            return totalArea > 0 ? overlapArea / totalArea : 0.0;
        }

#endif
    }
}
