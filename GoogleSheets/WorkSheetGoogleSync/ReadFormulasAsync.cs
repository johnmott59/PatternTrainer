using Google.Apis.Sheets.v4;
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
        public async Task<WorkSheet> ReadFormulasAsync(string sheetName)
        {
            var request = _service.Spreadsheets.Values.Get(
                _spreadsheetId,
                sheetName
            );

            request.ValueRenderOption =
                SpreadsheetsResource.ValuesResource.GetRequest
                    .ValueRenderOptionEnum.FORMULA;

            var response = await request.ExecuteAsync();

            // return ConvertToWorkSheet(response, sheetName);

            return new WorkSheet(sheetName);
        }
    }
}
