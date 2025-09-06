using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace CandlePatternML
{
    public class ThreeBarResult
    {
        public ThreeBarResult(string ticker,bool success, float confidence)
        {
            Ticker = ticker;
            Success = success;
            Confidence = confidence;
        }

        public string Ticker { get; set; }
        public bool Success { get; set; }
        public float Confidence { get; set; }
    }

    public partial class Program
    {
        static List<string> Tickers = new List<string> {"CYTK","AAPL","AMD","AMZN","ARM","AVGO",
            "COIN","CRWD","GOOGL","ISRG","HOOD","LLY","META","MSFT","MSTR","NFLX","NVDA",
            "PLTR","TSLA","QQQ","SPY" };


        APIWrapper oAPIWrapper = new APIWrapper();
        public async Task Run()
        {

            string authKey = GetAuthKey();

            MLEngineWrapper mlEngine = new MLEngineWrapper();
            List<ThreeBarResult> resultlist = new List<ThreeBarResult>();

            foreach (var  v in Tickers)
            {
                GetCandleModel model = oAPIWrapper.GetCandles(authKey, v, DateTime.Today.AddDays(-10), DateTime.Today, APIWrapper.eCandleTime.Daily);
                ThreeBarResult result = DoThreeBarRun(mlEngine, model);

                if (result.Success) resultlist.Add(result);
            }

            Console.WriteLine("Three Bar Pattern Results:");
            foreach (var r in resultlist.Where(m=>m.Success))
            {
                Console.WriteLine($"Confidence: {r.Confidence:P1}");
            }

        }
        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();

            //  p.SearchForCompression();

        }
    }



 

   
}
