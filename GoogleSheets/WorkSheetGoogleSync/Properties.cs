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
        private readonly string _spreadsheetId;
        private readonly string _credentialsPath;
        private readonly string _tokenStorePath;
        private readonly string _applicationName;
        private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private readonly SheetsService _service;

    }
}
