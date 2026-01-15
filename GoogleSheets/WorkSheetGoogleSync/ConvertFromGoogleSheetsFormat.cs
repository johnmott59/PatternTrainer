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
        /// Converts Google Sheets data format to WorkSheet model
        /// </summary>
        private WorkSheet ConvertFromGoogleSheetsFormat(IList<IList<object>> values, string sheetName, bool hasHeaders = true)
        {
            var worksheet = new WorkSheet(sheetName);

            if (values == null || values.Count == 0)
                return worksheet;

            // Determine the number of columns (use the maximum row length)
            int maxColumns = values.Max(row => row?.Count ?? 0);

            // Create columns first
            for (int col = 0; col < maxColumns; col++)
            {
                string header = $"Column{GetColumnLetter(col)}";
                worksheet.CreateColumn(header);
            }

            if (hasHeaders && values.Count > 0 && values[0] != null)
            {
                // Set column headers from first row
                for (int col = 0; col < Math.Min(values[0].Count, maxColumns); col++)
                {
                    string headerValue = values[0][col]?.ToString();
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        worksheet.Columns[col].Header = headerValue;
                    }
                    else
                    {
                        worksheet.Columns[col].Header = $"Column{GetColumnLetter(col)}";
                    }
                }

                // Start data from row 1 (skip header row)
                for (int row = 1; row < values.Count; row++)
                {
                    var dataRow = worksheet.CreateRow();
                    if (values[row] != null)
                    {
                        for (int col = 0; col < Math.Min(values[row].Count, maxColumns); col++)
                        {
                            dataRow.SetValue(col, values[row][col]);
                        }
                    }
                }
            }
            else
            {
                // No headers, treat all rows as data
                for (int row = 0; row < values.Count; row++)
                {
                    var dataRow = worksheet.CreateRow();
                    if (values[row] != null)
                    {
                        for (int col = 0; col < Math.Min(values[row].Count, maxColumns); col++)
                        {
                            dataRow.SetValue(col, values[row][col]);
                        }
                    }
                }
            }

            return worksheet;
        }
    }
}
