using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;


namespace CandlePatternML
{
    public class MLResult
    {
        public MLResult(GetCandleModel model, bool success, float confidence)
        {
            CandleModel = model;
            Success = success;
            Confidence = confidence;
        }

        public GetCandleModel CandleModel { get; set; }
        public string Ticker { get { return CandleModel.symbol; } }
        public bool Success { get; set; }
        public float Confidence { get; set; }
    }
}
