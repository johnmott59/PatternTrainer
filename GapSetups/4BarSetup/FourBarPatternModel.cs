using Microsoft.ML.Data;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;


namespace CandlePatternML
{
   
    /// <summary>
    /// This model will be fed into the ML. It has properties that will be
    /// referenced and properties that won't be referenced. it has to be flat,
    /// not containing other objects. The properties that are ignored by ML are
    /// marked by [NoColumn] and let us use this model to hold this data and 
    /// do other work
    /// </summary>
    public class FourBarPatternModel
    {
        [NoColumn]
        public Candle GapCandle { get; set; } // the gap candle

        [NoColumn]
        public LowHighModel GapBarLowHigh { get; set; }
      
        public float GapLow {   get {   return GapBarLowHigh.Low;  }}
        public float GapHigh{   get {   return GapBarLowHigh.High; }}

        [NoColumn]
        public Candle Hold1Candle { get; set; } // holding bar 1

        [NoColumn]
        public LowHighModel Hold1BarLowHigh { get; set; } // holding bar 1 
        public float Hold1Low { get { return Hold1BarLowHigh.Low; } }
        public float Hold1High { get { return Hold1BarLowHigh.High; } }

        [NoColumn]
        public Candle Hold2Candle { get; set; } // holding bar 2

        [NoColumn]
        public LowHighModel Hold2BarLowHigh { get; set; } // holding bar 2
        public float Hold2Low { get { return Hold2BarLowHigh.Low; } }
        public float Hold2High { get { return Hold2BarLowHigh.High; } }

        [NoColumn]
        public Candle Hold3Candle { get; set; } // holding bar 3

        [NoColumn]
        public LowHighModel Hold3BarLowHigh { get; set; } // holding bar 3
        public float Hold3Low { get { return Hold3BarLowHigh.Low; } }
        public float Hold3High { get { return Hold3BarLowHigh.High; } }

        public float Hold1LowLessThanGapHigh { get; set; }
        public float Hold1HighMoreThanGapLow { get; set; }

        public float Hold2LowLessThanGapHigh { get; set; }
        public float Hold2HighMoreThanGapLow { get; set; }

        public float Hold3LowLessThanGapHigh { get; set; }
        public float Hold3HighMoreThanGapLow { get; set; }


        /// <summary>
        /// Set extra features to emphasize some aspects of the pattern.
        /// The more of these we can do the better the model will be able to learn
        /// and its a way to flag edge conditions that could be inferred from the 
        /// data but gives a way to be explicit about them.
        /// </summary>

        public void SetExtraFeatures()
        {
            Hold1LowLessThanGapHigh = Hold1BarLowHigh.Low < GapBarLowHigh.High ? 1.0f : 0.0f;
            Hold1HighMoreThanGapLow = Hold1BarLowHigh.High > GapBarLowHigh.Low ? 1.0f : 0.0f;
         
            Hold2LowLessThanGapHigh = Hold2Low < GapBarLowHigh.High ? 1.0f : 0.0f;
            Hold2HighMoreThanGapLow = Hold2High > GapBarLowHigh.Low ? 1.0f : 0.0f;
         
            Hold3LowLessThanGapHigh = Hold3Low < GapBarLowHigh.High ? 1.0f : 0.0f;
            Hold3HighMoreThanGapLow = Hold3High > GapBarLowHigh.Low ? 1.0f : 0.0f;
        }


        public bool Label { get; set; }

    }
}
