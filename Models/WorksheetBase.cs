using Google.Apis.Sheets.v4;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;



namespace CandlePatternML
{

    public partial class WorksheetBase
    {


        // Configuration - replace with your actual values
        string spreadsheetId = ConfigurationManager.AppSettings["spreadsheetid"];

        static string credentialsPath = "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json";


        public static async Task<WorkSheet> ReadWorkSheet(string sheetName)
        {
            // Configuration - replace with your actual values
            string spreadsheetId = ConfigurationManager.AppSettings["spreadsheetid"];
          

            // Create the Google Sheets synchronizer
            var googleSync = new WorkSheetGoogleSync(spreadsheetId, credentialsPath);

            try
            {
                // Example 1: Read entire sheet
                Console.WriteLine("=== Reading entire sheet ===");
                var entireSheet = await googleSync.ReadAsync(sheetName);
                if (entireSheet != null)
                {
                    Console.WriteLine($"Read sheet '{entireSheet.SheetName}' with {entireSheet.RowCount} rows and {entireSheet.ColumnCount} columns");

                    return entireSheet;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during read operations: {ex.Message}");
            }

            return null;
        }

        public void UpdateWorkSheet(WorkSheet worksheet)
        {
            var googleSync = new WorkSheetGoogleSync(
                spreadsheetId: ConfigurationManager.AppSettings["spreadsheetid"],
                credentialsPath: "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json"
                );
            // Synchronize to Google Sheets
            bool success = googleSync.Synchronize(worksheet);
        }

        public static async Task<WorkSheet> ReadWorkSheetFormulas(string sheetName)
        {
            string spreadsheetId = ConfigurationManager.AppSettings["spreadsheetid"];
            string credentialsPath =
                "C:\\Users\\johnm\\source\\repos\\MLPatterns\\PatternTrainer\\credentials.json";

            var googleSync = new WorkSheetGoogleSync(spreadsheetId, credentialsPath);

            try
            {
                Console.WriteLine("=== Reading formulas only ===");

                var formulaSheet = await googleSync.ReadFormulasAsync(sheetName);
                if (formulaSheet != null)
                {
                    Console.WriteLine(
                        $"Read formulas for sheet '{formulaSheet.SheetName}' " +
                        $"({formulaSheet.RowCount} x {formulaSheet.ColumnCount})"
                    );

                    return formulaSheet;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during formula read: {ex.Message}");
            }

            return null;
        }





    }
}
