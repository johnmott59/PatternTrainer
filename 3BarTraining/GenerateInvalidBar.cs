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

            return new LowHighModel(low, high);
        }
    }
}
