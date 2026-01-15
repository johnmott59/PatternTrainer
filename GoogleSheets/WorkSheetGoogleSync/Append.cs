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
        /// Appends data to the end of an existing sheet (synchronous version)
        /// </summary>
        public bool Append(WorkSheet worksheet)
        {
            return AppendAsync(worksheet).GetAwaiter().GetResult();
        }
    }
}
