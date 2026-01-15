using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;


namespace CandlePatternML
{

    public partial class Program
    {

        /// <summary>
        /// Saves the ML results and trend break data to the database
        /// </summary>
        /// <param name="ticker">The ticker symbol</param>
        /// <param name="demarkPivots">The DeMark pivot analysis results</param>
        /// <param name="resultTwoBar">Two bar pattern ML result</param>
        /// <param name="resultThreeBar">Three bar pattern ML result</param>
        /// <param name="resultFourBar">Four bar pattern ML result</param>
        /// <param name="resultFiveBar">Five bar pattern ML result</param>
        /// <param name="resultRSI4">RSI4 pattern ML result</param>
        private void SaveModelsToDatabase(string ticker,DateTime StartDate, DemarkPivotModel demarkPivots,
            MLResult resultTwoBar, MLResult resultThreeBar, MLResult resultFourBar,
            MLResult resultFiveBar, MLResult resultRSI4)
        {
            // Create and save the SetupInstancesModel first
            var setupInstance = new SetupInstancesModel(ticker, StartDate);
            setupInstance.Save();

            if (resultRSI4.Success)
            {
                var rsi4Model = new RSI4LongModel(setupInstance, (decimal)resultRSI4.Confidence);
                rsi4Model.Save();
            }

            // Save TwoBarModel if successful
            if (resultTwoBar.Success)
            {
                var twoBarModel = new TwoBarModel(setupInstance, (decimal)resultTwoBar.Confidence, resultTwoBar.Notice ?? "");
                twoBarModel.Save();
            }

            // Save ThreeBarModel if successful
            if (resultThreeBar.Success)
            {
                var threeBarModel = new ThreeBarModel(setupInstance, (decimal)resultThreeBar.Confidence, resultThreeBar.Notice ?? "");
                threeBarModel.Save();
            }

            // Save FourBarModel if successful
            if (resultFourBar.Success)
            {
                var fourBarModel = new FourBarModel(setupInstance, (decimal)resultFourBar.Confidence, resultFourBar.Notice ?? "");
                fourBarModel.Save();
            }

            // Save FiveBarModel if successful
            if (resultFiveBar.Success)
            {
                var fiveBarModel = new FiveBarModel(setupInstance, (decimal)resultFiveBar.Confidence, resultFiveBar.Notice ?? "");
                fiveBarModel.Save();
            }

            // Save TrendBreaksModel if there's a trend break date or forecasted value
            if (demarkPivots.PivotHighTrendBreakDate.HasValue || demarkPivots.ForecastedPivotHighTrendBreak.HasValue)
            {
                // Use trend break date if available, otherwise use current date for forecasted breaks
                var trendBreakDate = demarkPivots.PivotHighTrendBreakDate ?? DateTime.Now;
                // Use forecasted value if available, otherwise use 0
                var projectedValue = demarkPivots.ForecastedPivotHighTrendBreak ?? 0m;
                var trendBreaksModel = new TrendBreaksModel(setupInstance, trendBreakDate, projectedValue);
                trendBreaksModel.Save();
            }
        }
    }
}
