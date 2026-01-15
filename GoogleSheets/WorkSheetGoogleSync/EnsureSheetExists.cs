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
        /// Creates a new sheet in the spreadsheet if it doesn't exist (synchronous version)
        /// </summary>
        public bool EnsureSheetExists(string sheetName)
        {
            return EnsureSheetExistsAsync(sheetName).GetAwaiter().GetResult();
        }
    }
}
