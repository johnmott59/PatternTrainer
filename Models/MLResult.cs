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
        public MLResult(GetCandleModel model, bool success, float confidence,string notice="")
        {
            CandleModel = model;
            Success = success;
            Confidence = confidence;
            Notice = notice;
        }

        public GetCandleModel CandleModel { get; set; }
        public string Ticker { get { return CandleModel.symbol; } }
        public bool Success { get; set; }
        public float Confidence { get; set; }
        public string Notice { get; set; } = "";
    }
}
