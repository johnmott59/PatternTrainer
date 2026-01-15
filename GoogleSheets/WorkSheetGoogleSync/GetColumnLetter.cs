using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;


namespace CandlePatternML
{

    public partial class WorkSheetGoogleSync
    {

        /// <summary>
        /// Converts a column index to a column letter (A, B, C, etc.)
        /// </summary>
        private static string GetColumnLetter(int columnIndex)
        {
            if (columnIndex < 0) throw new ArgumentException("Column index must be non-negative");

            string result = "";
            while (columnIndex >= 0)
            {
                result = (char)('A' + (columnIndex % 26)) + result;
                columnIndex = (columnIndex / 26) - 1;
            }
            return result;
        }
    }
}
