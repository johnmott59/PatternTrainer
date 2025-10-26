using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Xml.Linq;


namespace CandlePatternML
{
    public class DemarkPivotModel : IEquatable<DemarkPivotModel>

    {
        public PivotPointModel LatestPivotHigh { get; set; }
        public PivotPointModel NextToLastPivotHigh { get; set; }
        public DateTime? PivotHighTrendBreakDate { get; set; }
        public decimal? ForecastedPivotHighTrendBreak { get; set; }

        public PivotPointModel LatestPivotLow { get; set; }
        public PivotPointModel NextToLastPivotLow { get; set; }
        public DateTime? PivotLowTrendBreakDate { get; set; }
        public decimal? ForecastedPivotLowTrendBreak { get; set; }

        public bool Equals(DemarkPivotModel other)
        {
            bool result = true;

            // handle null conditions
            if (other is null) return false;
            if (LatestPivotHigh == null && other.LatestPivotHigh != null) return false;
            if (NextToLastPivotHigh == null && other.NextToLastPivotHigh != null) return false;
            if (LatestPivotLow == null && other.LatestPivotLow != null) return false;
            if (NextToLastPivotLow == null && other.NextToLastPivotLow != null) return false;
            if (PivotHighTrendBreakDate == null && other.PivotHighTrendBreakDate != null) return false;
            if (ForecastedPivotHighTrendBreak == null && other.ForecastedPivotHighTrendBreak != null) return false;
            if (PivotLowTrendBreakDate == null && other.PivotLowTrendBreakDate != null) return false;
            if (ForecastedPivotLowTrendBreak == null && other.ForecastedPivotLowTrendBreak != null) return false;


            // compare values
            if (LatestPivotHigh != null && !LatestPivotHigh.Equals(other.LatestPivotHigh)) return false;
            if (NextToLastPivotHigh != null && !NextToLastPivotHigh.Equals(other.NextToLastPivotHigh)) return false;
            if (LatestPivotLow != null && !LatestPivotLow.Equals(other.LatestPivotLow)) return false;
            if (NextToLastPivotLow != null && !NextToLastPivotLow.Equals(other.NextToLastPivotLow)) return false;
            if (PivotHighTrendBreakDate != null && !PivotHighTrendBreakDate.Equals(other.PivotHighTrendBreakDate)) return false;
            if (ForecastedPivotHighTrendBreak != null && !ForecastedPivotHighTrendBreak.Equals(other.ForecastedPivotHighTrendBreak)) return false;
            if (PivotLowTrendBreakDate != null && !PivotLowTrendBreakDate.Equals(other.PivotLowTrendBreakDate)) return false;
            if (ForecastedPivotLowTrendBreak != null && !ForecastedPivotLowTrendBreak.Equals(other.ForecastedPivotLowTrendBreak)) return false;

            return true;

        }
        

        /// <summary>
        /// Finds pivot highs and lows in a list of candles using DeMark methodology
        /// </summary>
        /// <param name="candleList">List of candles to analyze (most recent first)</param>
        /// <returns>PivotAnalysisResult containing all found pivots</returns>
        public bool FindPivots(List<Candle> candleList,string authkey)
        {

            // invert the candle list so we find the most recent pivots first

            candleList.Reverse();
         
            if (candleList == null || candleList.Count < 5)
            {
                Console.WriteLine("Insufficient data for pivot analysis. Need at least 5 candles.");
                return false;
            }

            // Find current and prior pivot highs
            FindPivotHighs(candleList);

            // Find current and prior pivot lows
            FindPivotLows(candleList);

            // go back to normal order and find the breaks of the trend lines

            candleList.Reverse();

            // find pivot high break
            FindPivotHighBreak(candleList);

            // find pivot low break
            FindPivotLowBreak(candleList);

            return true;
        }

        /// <summary>
        /// Finds pivot highs using the DeMark methodology
        /// </summary>
        private void FindPivotHighs(List<Candle> candleList)
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
                    LatestPivotHigh = new PivotPointModel(
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
            if (LatestPivotHigh != null)
            {
                currentIndex = LatestPivotHigh.Index + 1; // Start after current pivot
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
                        if (priorPivot.Value > LatestPivotHigh.Value)
                        {
                            NextToLastPivotHigh = priorPivot;
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
        private void FindPivotLows(List<Candle> candleList)
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
                    LatestPivotLow = new PivotPointModel(
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
            if (LatestPivotLow != null)
            {
                currentIndex = LatestPivotLow.Index + 1; // Start after current pivot
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
                        if (priorPivot.Value < LatestPivotLow.Value)
                        {
                            NextToLastPivotLow = priorPivot;
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
                   t2.low < t4.low;
        }

        public void FindPivotHighBreak(List<Candle> candles)
        {
            // locate the candle after the latest pivot high
            if (LatestPivotHigh == null || NextToLastPivotHigh == null)
            {
                return;
            }
            int latestPivotIndex = candles.FindIndex(c => c.dtDotNet == LatestPivotHigh.dtCandle);
            if (latestPivotIndex == -1 || latestPivotIndex + 1 >= candles.Count)
            {
                return;
            }

            int candleCountinRun = NextToLastPivotHigh.Index - LatestPivotHigh.Index;
            double slope = (double)(LatestPivotHigh.Value - NextToLastPivotHigh.Value)
                / (double)candleCountinRun;

            // for each candle after the last pivot high, compute the estimated value along 
            // the slope and see if the candle close breaks that value
            double CurrentEndValue = (double)LatestPivotHigh.Value;
            for (int i = latestPivotIndex + 1; i < candles.Count; i++)
            {
                // see where the downtrend line is at this candle
                CurrentEndValue += slope;
                // see if this candle close breaks that value
                if (candles[i].close > (decimal)CurrentEndValue)
                {
                    // we have a break of the downtrend
                    PivotHighTrendBreakDate = candles[i].dtDotNet;
                    break;
                }
            }

            // if we don't have a trend line that has not been broken yet
            // compute the forecasted value for the next candle
            if (PivotHighTrendBreakDate == null)
            {
                ForecastedPivotHighTrendBreak = (decimal)(CurrentEndValue + slope);
            }
        }

        public void FindPivotLowBreak(List<Candle> candles)
        {
            // locate the candle after the latest pivot low
            if (LatestPivotLow == null || NextToLastPivotLow == null)
            {
                return;
            }
            int latestPivotIndex = candles.FindIndex(c => c.dtDotNet == LatestPivotLow.dtCandle);
            if (latestPivotIndex == -1 || latestPivotIndex + 1 >= candles.Count)
            {
                return;
            }

            int candleCountinRun = NextToLastPivotLow.Index - LatestPivotLow.Index;
            double slope = (double)(LatestPivotLow.Value - NextToLastPivotLow.Value)
                / (double)candleCountinRun;

            // for each candle after the last pivot low, compute the estimated value along 
            // the slope and see if the candle close breaks that value
            double CurrentEndValue = (double)LatestPivotLow.Value;
            for (int i = latestPivotIndex + 1; i < candles.Count; i++)
            {
                // see where the downtrend line is at this candle
                CurrentEndValue += slope;
                // see if this candle close breaks that value
                if (candles[i].close < (decimal)CurrentEndValue)
                {
                    // we have a break of the downtrend
                    PivotLowTrendBreakDate = candles[i].dtDotNet;
                    break;
                }
            }

            // compute the forecasted value for the next candle
            if (PivotLowTrendBreakDate == null)
            {
                ForecastedPivotLowTrendBreak = (decimal)(CurrentEndValue + slope);
            }
        }

    }

}
