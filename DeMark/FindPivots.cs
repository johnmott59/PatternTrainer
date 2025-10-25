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
    /// Contains the results of pivot analysis
    /// </summary>
    public class PivotAnalysisResult
    {
        public PivotPointModel LatestPivotHigh { get; set; }
        public PivotPointModel NextToLastPivotHigh { get; set; }
        public DateTime? TrendHighBreakDate { get; set; }
        public decimal? ProjectedTrendHighBreakValue { get; set; }

        public PivotPointModel LatestPivotLow { get; set; }
        public PivotPointModel NextToLastPivotLow { get; set; }
        public DateTime? TrendLowBreakDate { get; set; }
        public decimal? ProjectedTrendLowBreakValue { get; set; }

        public bool HasValidPattern => 
            LatestPivotHigh != null && NextToLastPivotHigh != null && 
            LatestPivotLow != null && NextToLastPivotLow != null &&
            NextToLastPivotHigh.Value > LatestPivotHigh.Value && // Descending highs
            NextToLastPivotLow.Value < LatestPivotLow.Value;     // Ascending lows
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
                    result.LatestPivotHigh = new PivotPointModel(
                        candleList[currentIndex + 2].high, 
                        candleList[currentIndex + 2], 
                        currentIndex + 2
                    );
                    noPivotHighCurrent = false;
                }
                else
                {
                    currentIndex++;
                }
            }
            
            // Find prior pivot high (must be higher than current)
            if (result.LatestPivotHigh != null)
            {
                currentIndex = result.LatestPivotHigh.Index + 1; // Start after current pivot
                while (noPivotHighPrior && currentIndex + 4 < candleList.Count)
                {
                    if (IsPivotHigh(candleList, currentIndex))
                    {
                        var priorPivot = new PivotPointModel(
                            candleList[currentIndex + 2].high, 
                            candleList[currentIndex + 2], 
                            currentIndex + 2
                        );
                        
                        // Check if this prior pivot is higher than current (descending pattern)
                        if (priorPivot.Value > result.LatestPivotHigh.Value)
                        {
                            result.NextToLastPivotHigh = priorPivot;
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
                    result.LatestPivotLow = new PivotPointModel(
                        candleList[currentIndex + 2].low, 
                        candleList[currentIndex + 2], 
                        currentIndex + 2
                    );
                    noPivotLowCurrent = false;
                }
                else
                {
                    currentIndex++;
                }
            }
            
            // Find prior pivot low (must be lower than current)
            if (result.LatestPivotLow != null)
            {
                currentIndex = result.LatestPivotLow.Index + 1; // Start after current pivot
                while (noPivotLowPrior && currentIndex + 4 < candleList.Count)
                {
                    if (IsPivotLow(candleList, currentIndex))
                    {
                        var priorPivot = new PivotPointModel(
                            candleList[currentIndex + 2].low, 
                            candleList[currentIndex + 2], 
                            currentIndex + 2
                        );
                        
                        // Check if this prior pivot is lower than current (ascending pattern)
                        if (priorPivot.Value < result.LatestPivotLow.Value)
                        {
                            result.NextToLastPivotLow = priorPivot;
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
                   t2.high > t4.high;
                  
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
                   t2.low < t4.low ;
        }


    }
}
