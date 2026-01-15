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
        /// Reads data from a Google Sheet and returns it as a WorkSheet model (synchronous version)
        /// </summary>
        /// <param name="sheetName">The name of the sheet to read from</param>
        /// <param name="range">Optional range to read (e.g., "A1:Z100" or "A:Z"). If null, reads the entire sheet</param>
        /// <returns>A WorkSheet object containing the read data, or null if reading failed</returns>
        public WorkSheet Read(string sheetName, string range = null)
        {
            return ReadAsync(sheetName, range).GetAwaiter().GetResult();
        }
    }
}
