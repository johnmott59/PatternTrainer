using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;


namespace CandlePatternML
{

    public partial class Program
    {
        APIWrapper oAPIWrapper = new APIWrapper();
        public void Run()
        {

            string authKey = GetAuthKey();

            GetCandleModel model = oAPIWrapper.GetCandles(authKey, "AAPL", DateTime.Now.AddDays(-180), DateTime.Now, APIWrapper.eCandleTime.Daily);

           // decimal min = model.candles.Min(m => m.low);
           // decimal max = model.candles.Max(m => m.high);

            DoThreeBarTraining();

            DoThreeBarRun(model);


        }
        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();

            //  p.SearchForCompression();

        }
    }



 

   
}
