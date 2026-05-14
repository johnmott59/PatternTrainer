using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CandlePatternML
{
    public partial class Program
    {
        /// <summary>
        /// Fetches intraday candles for the symbol (placeholder uses the same daily call as the main loop).
        /// Restricts to the last six session dates in the series and runs
        /// <see cref="SchwabLib.Studies.ComputeFullVolumeProfileForPreviousDays"/>, which skips the newest session day,
        /// yielding up to five volume profiles for the prior completed sessions.
        /// </summary>
        public List<Studies.VolumeProfileResult> ComputeVolumeProfilesLastFiveDays(
            string ticker,
            int binSizeInPennies = 20,
            double percentile = 0.67)
        {
            string authKey = GetAuthKey();

            // TODO: switch to one-minute candles and an appropriate lookback once the API call is ready.
            GetCandleModel minuteModel = oAPIWrapper.GetCandles(
                authKey,
                ticker,
                DateTime.Today.AddDays(-400),
                DateTime.Today,
                APIWrapper.eCandleTime.Daily);

            List<Candle> candles = minuteModel.candles.ToList();
            if (candles.Count == 0)
                return new List<Studies.VolumeProfileResult>();

            List<DateTime> sessionDates = candles
                .Select(c => c.dtDotNet.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            int dayCount = Math.Min(6, sessionDates.Count);
            var lastSessionDates = sessionDates.TakeLast(dayCount).ToHashSet();
            List<Candle> windowed = candles.Where(c => lastSessionDates.Contains(c.dtDotNet.Date)).ToList();

            return Studies.ComputeFullVolumeProfileForPreviousDays(windowed, binSizeInPennies, percentile);
        }
    }
}
