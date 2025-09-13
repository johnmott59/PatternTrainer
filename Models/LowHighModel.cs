using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;


namespace CandlePatternML
{

    public class LowHighModel
    {
        public float Low { get; set; }
        public float High { get; set; }

        public LowHighModel(decimal low, decimal high)
        {
            Low = (float)low;
            High = (float)high;
        }
        public LowHighModel(double Low, double High)
        {
            this.Low = (float)Low;
            this.High = (float)High;
        }

        public LowHighModel(float Low, float High)
        {
            this.Low = Low;
            this.High = High;
        }
        // basic sanity check
        public bool Valid { get { return Low > 0 && High > 0 && Low < High; } }
    }
}
