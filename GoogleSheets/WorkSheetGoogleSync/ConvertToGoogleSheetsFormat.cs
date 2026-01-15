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
        /// Converts the 2D data array to Google Sheets format
        /// </summary>
        private IList<IList<object>> ConvertToGoogleSheetsFormat(object[,] dataArray)
        {
            var rows = dataArray.GetLength(0);
            var cols = dataArray.GetLength(1);

            var result = new List<IList<object>>();

            for (int row = 0; row < rows; row++)
            {
                var rowData = new List<object>();
                for (int col = 0; col < cols; col++)
                {
                    var value = dataArray[row, col];
                    rowData.Add(value ?? "");
                }
                result.Add(rowData);
            }

            return result;
        }
    }
}
