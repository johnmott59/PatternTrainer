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
        /// Synchronizes the entire worksheet to Google Sheets (synchronous version)
        /// </summary>
        public bool Synchronize(WorkSheet worksheet)
        {
            return SynchronizeAsync(worksheet).GetAwaiter().GetResult();
        }

    }
}
