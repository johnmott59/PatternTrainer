using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Transactions;
using System.Windows.Input.Manipulations;


namespace CandlePatternML
{
 
   


    public class GenerateTraining
    {
        public const double DefaultSlippagePercent = .01;

        private static readonly Random rand = new();

        protected double gapBarLow { get; set; }
        protected double gapBarSize { get; set; }
        protected double gapBarHigh { get; set; }
        
        // generated bars
        protected List<LowHighModel> barList = new List<LowHighModel> ();

        // determine the min and max bar size. allow for slippage
        protected double minBarSize;

        // determine valid upper and lower bounds
        protected double validLowerBound ;
        protected double validUpperBound ;

        protected double Overlap { get; set; } // amount of overlap

        protected bool pass; // true if this is a valid pattern

        private double RandomDouble(double min, double max)
        {
            return rand.NextDouble() * (max - min) + min;
        }

        public ThreeBarPatternModel GetPattern()
        {
            return ThreeBarPatternModel.GetPattern(barList,this.pass);
        }
        public GenerateTraining(int length, 
            bool pass, 
            decimal minPrice, 
            decimal maxPrice,
            double overlap,
            double slippagePercent = DefaultSlippagePercent)
        {

            this.pass = pass; // true if this is a valid pattern
            this.Overlap = overlap; // amount of overlap

            if (length != 3 && length != 4)
                throw new ArgumentException("Length must be 3 or 4");
            /*
             * Create the gap bar
             */
            // 🔹 1. Create the gap bar
            // gapBarLow = RandomDouble(30, 300);
            // get a price within the range of minPrice and maxPrice
            gapBarLow = RandomDouble((double)minPrice, (double)maxPrice);
            gapBarSize = RandomDouble(0.5, 20);
            gapBarHigh = gapBarLow + gapBarSize;

            // add this bar
            barList.Add(new LowHighModel(gapBarLow, gapBarHigh));

            // determine the min and max bar size. allow for slippage

            minBarSize = gapBarSize * 0.01;
            if (minBarSize< .05) minBarSize = .10; // don't take less than a dime

            // determine valid upper and lower bounds allowing for slippage
            // slippage is based on the stock price, not the bar size

            validLowerBound = gapBarLow - gapBarLow * slippagePercent;
            validUpperBound = gapBarHigh + gapBarLow * slippagePercent;

            double overage = 0;
            if (pass)
            {
                List<LowHighModel> tmpBarList = new List<LowHighModel>();
                int attempts = 0; // to avoid infinite loops    
               // do
               // {
                    tmpBarList.Clear(); // clear the temporary list
                    for (int i = 0; i < length - 1; i++)
                    {
                        tmpBarList.Add(GetValidBar());
                    }

                    //  overage = ThreeBarPattern.FindOverlap(tmpBarList); // update the overlap value

                    attempts++;
                //} while (true);// overage < Overlap);
           
                //if (attempts > 100)
                //{
                //    Console.WriteLine("more than 100 attempts to generate valid bars, using last attempt");
                //}
                barList.AddRange(tmpBarList); // add the valid bars to the list
            }
            else
            {
                GenerateMixedBars(length-1);
            }
 
        }

        protected void AddValidBar()
        {

     
            barList.Add(GetValidBar());


        }
        

        private LowHighModel GetValidBar()
        {

            // get a lower value
            double lowerValue = RandomDouble(validLowerBound, validUpperBound);

            // the lower value needs to be less than the upper bound minus the minimum bar size
            if (lowerValue >= validUpperBound - minBarSize)
            {
                lowerValue = validUpperBound - minBarSize;
            }
            // now get a higher value   
            double highervalue = RandomDouble(lowerValue, validUpperBound); 
         
            return new LowHighModel(lowerValue, highervalue);
        }

        private LowHighModel GenerateInvalidBar()
        {
            if (validLowerBound >= validUpperBound)
                throw new ArgumentException("Invalid bounds");

            double range = validUpperBound - validLowerBound;
            double margin = range * 0.01; // slight buffer to keep things safely outside

            int type = rand.Next(5); // randomly pick 1 of the 5 failure types

            double low, high;

            switch (type)
            {
                case 0: // fully above
                    low = validUpperBound + margin;
                    high = low + RandomDouble(range * 0.5, range); // taller bar
                    break;

                case 1: // fully below
                    high = validLowerBound - margin;
                    low = high - RandomDouble(range * 0.5, range);
                    break;

                case 2: // straddle lower bound
                    low = validLowerBound - RandomDouble(range * 0.5, range);
                    high = validLowerBound + RandomDouble(margin, range * 0.5);
                    break;

                case 3: // straddle upper bound
                    low = validUpperBound - RandomDouble(range * 0.5, margin);
                    high = validUpperBound + RandomDouble(margin, range * 0.5);
                    break;

                case 4: // fully spans both
                    low = validLowerBound - RandomDouble(range * 0.5, range);
                    high = validUpperBound + RandomDouble(range * 0.5, range);
                    break;

                default:
                    throw new InvalidOperationException("Invalid type index");
            }

            if (high < low)
            {
                throw new Exception("high < low");
            }

            return new LowHighModel(low,high);
        }

        protected void GenerateMixedBars(int totalBars)
        {
            if (totalBars <= 0)
                throw new ArgumentException("n must be >= 1");

            int numInvalid = rand.Next(1, totalBars); // at least one invalid

            int numValid = totalBars - numInvalid;

            var bars = new List<LowHighModel>();

            // Insert invalid bars first
            for (int i = 0; i < numInvalid; i++)
                bars.Add(GenerateInvalidBar());

            // Then valid bars
            for (int i = 0; i < numValid; i++)
                bars.Add(GetValidBar());

            // Shuffle so valid/invalid are mixed
            Shuffle(bars);

            barList.AddRange(bars);
        }
        private void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

 
        /// <summary>
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
                var (l,h) = bars[i];
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
            var (gapLow,gapHigh) = bars[0];
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
                var (l,h) = bars[i];
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
