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
        public PivotAnalysisResult FindDemarkPivots(List<Candle> candleList)
        {

            // Find pivots
            var result = FindPivots(candleList);
            
            // Print results
            PrintPivotResults(result);

            return result;
            
            // Additional analysis
           // AnalyzeResults(result);
        }
        

         
        /// <summary>
        /// Performs additional analysis on the pivot results
        /// </summary>
        private static void AnalyzeResults(PivotAnalysisResult result)
        {
            Console.WriteLine("\n=== Additional Analysis ===");
            
            if (result.HasValidPattern)
            {
                Console.WriteLine("✅ Valid DeMark pattern detected!");
                Console.WriteLine($"   - Descending highs: {result.PriorPivotHigh.Value} > {result.CurrentPivotHigh.Value}");
                Console.WriteLine($"   - Ascending lows: {result.PriorPivotLow.Value} < {result.CurrentPivotLow.Value}");
            }
            else
            {
                Console.WriteLine("❌ No valid DeMark pattern found");
                
                if (result.CurrentPivotHigh == null)
                    Console.WriteLine("   - Missing current pivot high");
                if (result.PriorPivotHigh == null)
                    Console.WriteLine("   - Missing prior pivot high");
                if (result.CurrentPivotLow == null)
                    Console.WriteLine("   - Missing current pivot low");
                if (result.PriorPivotLow == null)
                    Console.WriteLine("   - Missing prior pivot low");
            }
            
            // Show all found pivots
            Console.WriteLine("\nAll Pivot Highs:");
            foreach (var pivot in result.AllPivotHighs)
            {
                Console.WriteLine($"   - Value: {pivot.Value}, Index: {pivot.Index}");
            }
            
            Console.WriteLine("\nAll Pivot Lows:");
            foreach (var pivot in result.AllPivotLows)
            {
                Console.WriteLine($"   - Value: {pivot.Value}, Index: {pivot.Index}");
            }
        }
    }
}
