using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Data;


namespace CandlePatternML
{
    /// <summary>
    /// Represents a pivot point with its value and the candle that contains it
    /// </summary>
    public class PivotPoint
    {
        public decimal Value { get; set; }
        public Candle Candle { get; set; }
        public int Index { get; set; } // Index in the original candle list
        public DateTime dtCandle
        {
            get { return Candle.dtDotNet; }
        }
        
        public PivotPoint(decimal value, Candle candle, int index)
        {
            Value = value;
            Candle = candle;
            Index = index;
        }
    }

    /// <summary>
    /// Contains the results of pivot analysis
    /// </summary>
    public class PivotAnalysisResult
    {
        public PivotPoint CurrentPivotHigh { get; set; }
        public PivotPoint PriorPivotHigh { get; set; }
        public PivotPoint CurrentPivotLow { get; set; }
        public PivotPoint PriorPivotLow { get; set; }
        public List<PivotPoint> AllPivotHighs { get; set; } = new List<PivotPoint>();
        public List<PivotPoint> AllPivotLows { get; set; } = new List<PivotPoint>();
        
        public bool HasValidPattern => 
            CurrentPivotHigh != null && PriorPivotHigh != null && 
            CurrentPivotLow != null && PriorPivotLow != null &&
            PriorPivotHigh.Value > CurrentPivotHigh.Value && // Descending highs
            PriorPivotLow.Value < CurrentPivotLow.Value;     // Ascending lows
    }

    public partial class Program
    {
        /// <summary>
        /// Finds pivot highs and lows in a list of candles using DeMark methodology
        /// </summary>
        /// <param name="candleList">List of candles to analyze (most recent first)</param>
        /// <returns>PivotAnalysisResult containing all found pivots</returns>
        public PivotAnalysisResult FindPivots(List<Candle> candleList)
        {

            // invert the candle list so we find the most recent pivots first

            candleList.Reverse();

            string authKey = GetAuthKey();

            var result = new PivotAnalysisResult();
            
            if (candleList == null || candleList.Count < 5)
            {
                Console.WriteLine("Insufficient data for pivot analysis. Need at least 5 candles.");
                return result;
            }

            // Find current and prior pivot highs
            FindPivotHighs(candleList, result);
            
            // Find current and prior pivot lows
            FindPivotLows(candleList, result);
            
            return result;
        }

        /// <summary>
        /// Finds pivot highs using the DeMark methodology
        /// </summary>
        private void FindPivotHighs(List<Candle> candleList, PivotAnalysisResult result)
        {
            // Start from the most recent candle (index 0) and work backwards
            int currentIndex = 0;
            bool noPivotHighCurrent = true;
            bool noPivotHighPrior = true;
            
            // Find current pivot high
            while (noPivotHighCurrent && currentIndex + 4 < candleList.Count)
            {
                if (IsPivotHigh(candleList, currentIndex))
                {
                    result.CurrentPivotHigh = new PivotPoint(
                        candleList[currentIndex + 2].high, 
                        candleList[currentIndex + 2], 
                        currentIndex + 2
                    );
                    result.AllPivotHighs.Add(result.CurrentPivotHigh);
                    noPivotHighCurrent = false;
                }
                else
                {
                    currentIndex++;
                }
            }
            
            // Find prior pivot high (must be higher than current)
            if (result.CurrentPivotHigh != null)
            {
                currentIndex = result.CurrentPivotHigh.Index + 1; // Start after current pivot
                while (noPivotHighPrior && currentIndex + 4 < candleList.Count)
                {
                    if (IsPivotHigh(candleList, currentIndex))
                    {
                        var priorPivot = new PivotPoint(
                            candleList[currentIndex + 2].high, 
                            candleList[currentIndex + 2], 
                            currentIndex + 2
                        );
                        
                        // Check if this prior pivot is higher than current (descending pattern)
                        if (priorPivot.Value > result.CurrentPivotHigh.Value)
                        {
                            result.PriorPivotHigh = priorPivot;
                            result.AllPivotHighs.Add(priorPivot);
                            noPivotHighPrior = false;
                        }
                        else
                        {
                            currentIndex++;
                        }
                    }
                    else
                    {
                        currentIndex++;
                    }
                }
            }
        }

        /// <summary>
        /// Finds pivot lows using the DeMark methodology
        /// </summary>
        private void FindPivotLows(List<Candle> candleList, PivotAnalysisResult result)
        {
            // Start from the most recent candle (index 0) and work backwards
            int currentIndex = 0;
            bool noPivotLowCurrent = true;
            bool noPivotLowPrior = true;
            
            // Find current pivot low
            while (noPivotLowCurrent && currentIndex + 4 < candleList.Count)
            {
                Console.WriteLine($"current date {candleList[currentIndex].dtDotNet.ToShortDateString()}");
                if (IsPivotLow(candleList, currentIndex))
                {
                    result.CurrentPivotLow = new PivotPoint(
                        candleList[currentIndex + 2].low, 
                        candleList[currentIndex + 2], 
                        currentIndex + 2
                    );
                    result.AllPivotLows.Add(result.CurrentPivotLow);
                    noPivotLowCurrent = false;
                }
                else
                {
                    currentIndex++;
                }
            }
            
            // Find prior pivot low (must be lower than current)
            if (result.CurrentPivotLow != null)
            {
                currentIndex = result.CurrentPivotLow.Index + 1; // Start after current pivot
                while (noPivotLowPrior && currentIndex + 4 < candleList.Count)
                {
                    if (IsPivotLow(candleList, currentIndex))
                    {
                        var priorPivot = new PivotPoint(
                            candleList[currentIndex + 2].low, 
                            candleList[currentIndex + 2], 
                            currentIndex + 2
                        );
                        
                        // Check if this prior pivot is lower than current (ascending pattern)
                        if (priorPivot.Value < result.CurrentPivotLow.Value)
                        {
                            result.PriorPivotLow = priorPivot;
                            result.AllPivotLows.Add(priorPivot);
                            noPivotLowPrior = false;
                        }
                        else
                        {
                            currentIndex++;
                        }
                    }
                    else
                    {
                        currentIndex++;
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a candle at the given index is a pivot high
        /// Based on DeMark criteria: t-2.high > t0.high AND t-2.high > t-1.high AND t-2.high > t-3.high AND t-2.high > t-4.high AND t-1.high > t0.high AND t-3.high > t-4.high
        /// </summary>
        private bool IsPivotHigh(List<Candle> candleList, int t0Index)
        {
            if (t0Index + 4 >= candleList.Count) return false;
            
            // t-2 is the candidate pivot (2 candles before t0)
            int t2Index = t0Index + 2;
            int t1Index = t0Index + 1;
            int t3Index = t0Index + 3;
            int t4Index = t0Index + 4;
            
            var t0 = candleList[t0Index];
            var t1 = candleList[t1Index];
            var t2 = candleList[t2Index];
            var t3 = candleList[t3Index];
            var t4 = candleList[t4Index];
            
            return t2.high > t0.high &&
                   t2.high > t1.high &&
                   t2.high > t3.high &&
                   t2.high > t4.high &&
                   t1.high > t0.high &&
                   t3.high > t4.high;
        }

        /// <summary>
        /// Determines if a candle at the given index is a pivot low
        /// Based on DeMark criteria: t-2.low < t0.low AND t-2.low < t-1.low AND t-2.low < t-3.low AND t-2.low < t-4.low AND t-1.low < t0.low AND t-3.low < t-4.low
        /// </summary>
        private bool IsPivotLow(List<Candle> candleList, int t0Index)
        {
            if (t0Index + 4 >= candleList.Count) return false;
            
            // t-2 is the candidate pivot (2 candles before t0)
            int t2Index = t0Index + 2;
            int t1Index = t0Index + 1;
            int t3Index = t0Index + 3;
            int t4Index = t0Index + 4;
            
            var t0 = candleList[t0Index];
            var t1 = candleList[t1Index];
            var t2 = candleList[t2Index];
            var t3 = candleList[t3Index];
            var t4 = candleList[t4Index];
            
            return t2.low < t0.low &&
                   t2.low < t1.low &&
                   t2.low < t3.low &&
                   t2.low < t4.low &&
                   t1.low < t0.low &&
                   t3.low < t4.low;
        }

        /// <summary>
        /// Prints the results of pivot analysis to console
        /// </summary>
        public void PrintPivotResults(PivotAnalysisResult result)
        {
            Console.WriteLine("=== Pivot Analysis Results ===");
            
            if (result.CurrentPivotHigh != null)
            {
                Console.WriteLine($"Current Pivot High: {result.CurrentPivotHigh.Value} on date {result.CurrentPivotHigh.Candle.dtDotNet.ToShortDateString()}");
            }
            else
            {
                Console.WriteLine("No current pivot high found");
            }
            
            if (result.PriorPivotHigh != null)
            {
                Console.WriteLine($"Prior Pivot High: {result.PriorPivotHigh.Value} on date {result.PriorPivotHigh.Candle.dtDotNet.ToShortDateString()}");
            }
            else
            {
                Console.WriteLine("No prior pivot high found");
            }
            
            if (result.CurrentPivotLow != null)
            {
                Console.WriteLine($"Current Pivot Low: {result.CurrentPivotLow.Value} at date {result.CurrentPivotLow.Candle.dtDotNet.ToShortDateString()}");
            }
            else
            {
                Console.WriteLine("No current pivot low found");
            }
            
            if (result.PriorPivotLow != null)
            {
                Console.WriteLine($"Prior Pivot Low: {result.PriorPivotLow.Value} at index {result.PriorPivotLow.Candle.dtDotNet.ToShortDateString()}");
            }
            else
            {
                Console.WriteLine("No prior pivot low found");
            }
            
            Console.WriteLine($"Valid DeMark Pattern: {result.HasValidPattern}");
            Console.WriteLine($"Total Pivot Highs Found: {result.AllPivotHighs.Count}");
            Console.WriteLine($"Total Pivot Lows Found: {result.AllPivotLows.Count}");
        }
    }
}
