using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Transactions;
using System.Windows.Input.Manipulations;


namespace CandlePatternML
{
    public partial class GenerateTraining
    {
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
            if (minBarSize < .05) minBarSize = .10; // don't take less than a dime

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
                    tmpBarList.Add(GenerateValidBar());
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
                GenerateMixedValidAndInvalidBars(length - 1);
            }

        }
    }
}
