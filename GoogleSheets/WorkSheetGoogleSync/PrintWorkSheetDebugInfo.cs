using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Added for FirstOrDefault
using System.Threading;
using System.Threading.Tasks;

namespace CandlePatternML
{
    /// <summary>
    /// Provides synchronization functionality to save WorkSheet data to Google Sheets
    /// </summary>
    public partial class WorkSheetGoogleSync
    {  

        /// <summary>
        /// Debug method to print WorkSheet structure for troubleshooting
        /// </summary>
        public void PrintWorkSheetDebugInfo(WorkSheet worksheet, string title = "WorkSheet Debug Info")
        {
            Console.WriteLine($"\n=== {title} ===");
            Console.WriteLine($"Sheet Name: {worksheet.SheetName}");
            Console.WriteLine($"Column Count: {worksheet.ColumnCount}");
            Console.WriteLine($"Row Count: {worksheet.RowCount}");
            
            Console.WriteLine("\nHeaders:");
            for (int col = 0; col < worksheet.ColumnCount; col++)
            {
                var column = worksheet.GetColumn(col);
                Console.WriteLine($"  Column {col}: '{column?.Header}'");
            }
            
            Console.WriteLine("\nData Rows:");
            for (int row = 0; row < worksheet.RowCount; row++)
            {
                var dataRow = worksheet.GetRow(row);
                Console.Write($"  Row {row}: ");
                if (dataRow != null)
                {
                    for (int col = 0; col < worksheet.ColumnCount; col++)
                    {
                        var value = dataRow.GetValue(col);
                        Console.Write($"{value ?? "null"} ");
                    }
                }
                else
                {
                    Console.Write("(null row)");
                }
                Console.WriteLine();
            }
            
            // Show what GetDataArray() would produce
            var dataArray = worksheet.GetDataArray();
            Console.WriteLine($"\nGetDataArray() produces {dataArray.GetLength(0)} rows x {dataArray.GetLength(1)} columns:");
            for (int row = 0; row < dataArray.GetLength(0); row++)
            {
                Console.Write($"  Array Row {row}: ");
                for (int col = 0; col < dataArray.GetLength(1); col++)
                {
                    Console.Write($"{dataArray[row, col] ?? "null"} ");
                }
                Console.WriteLine();
            }
        }
    }
}
