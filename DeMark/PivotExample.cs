using Google.Apis.Sheets.v4.Data;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;

namespace CandlePatternML
{
    /// <summary>
    /// Example usage of the DeMark pivot finding functionality
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// Demonstrates how to use the pivot finding functionality
        /// </summary>
        public DemarkPivotModel FindDemarkPivots(List<Candle> candleList)
        {

            // Find pivots
            var result = FindPivots(candleList);


            return result;
            
        }
        


    }
}

