using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using PatternTrainer;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace CandlePatternML
{
    public class MLResult
    {
        public MLResult(string ticker,bool success, float confidence)
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
        static List<string> Tickers = new List<string> {"AAPL","AMD","AMZN","ARM","AVGO",
            "COIN","CRWD","GOOGL","ISRG","HOOD","LLY","META","MSFT","MSTR","NFLX","NVDA",
            "PLTR","TSLA","QQQ","SPY" };


        APIWrapper oAPIWrapper = new APIWrapper();
        public async Task Run()
        {

            string authKey = GetAuthKey();

            MLEngineWrapper mlEngine2bar = new MLEngineWrapper(MLEngineWrapper.eMLEngineType.TwoBarPattern, "c:\\work\\TwoBarModel.zip");
            MLEngineWrapper mlEngine3bar = new MLEngineWrapper(MLEngineWrapper.eMLEngineType.ThreeBarPattern,"c:\\work\\ThreeBarModel.zip");
            MLEngineWrapper mlEngine4bar = new MLEngineWrapper(MLEngineWrapper.eMLEngineType.FourBarPattern, "c:\\work\\FourBarModel.zip");
            MLEngineWrapper mlEngine5bar = new MLEngineWrapper(MLEngineWrapper.eMLEngineType.FiveBarPattern, "c:\\work\\FiveBarModel.zip");

            List<MLResult> resultlist5bar = new List<MLResult>();
            List<MLResult> resultlist4bar = new List<MLResult>();
            List<MLResult> resultlist3bar = new List<MLResult>();
            List<MLResult> resultlist2bar = new List<MLResult>();

            WorkSheetModel results = new WorkSheetModel();

            foreach (var v in Tickers)
            {
                GetCandleModel model;
                try
                {
                    model = oAPIWrapper.GetCandles(authKey, v, DateTime.Today.AddDays(-10), DateTime.Today, APIWrapper.eCandleTime.Daily);
                } catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving data for {v}: {ex.Message}");
                    continue;
                }

                MLResult result5 = DoFiveBarLive(mlEngine5bar, model);
                resultlist5bar.Add(result5);

                MLResult result4 = DoFourBarLive(mlEngine4bar, model);
                resultlist4bar.Add(result4);

                MLResult result3 = DoThreeBarLive(mlEngine3bar, model);
                resultlist3bar.Add(result3);

                MLResult result2 = DoTwoBarLive(mlEngine2bar, model);
                resultlist2bar.Add(result2);

                // add to results worksheet
                results.AddTicker(v,result2,result3,result4,result5);

            }

            Console.WriteLine("Five Bar Pattern Results:");
            foreach (var r in resultlist5bar.Where(m => m.Success))
            {
                Console.WriteLine($"{r.Ticker} {r.Success} Confidence: {r.Confidence:P1}");
            }


            Console.WriteLine("Four Bar Pattern Results:");
            foreach (var r in resultlist4bar.Where(m => m.Success))
            {
                Console.WriteLine($"{r.Ticker} {r.Success} Confidence: {r.Confidence:P1}");
            }

            Console.WriteLine("Three Bar Pattern Results:");
            foreach (var r in resultlist3bar.Where(m=>m.Success))
            {
                Console.WriteLine($"{r.Ticker} {r.Success} Confidence: {r.Confidence:P1}");
            }

            Console.WriteLine("Two Bar Pattern Results:");
            foreach (var r in resultlist2bar.Where(m => m.Success))
            {
                Console.WriteLine($"{r.Ticker}  {r.Success}Confidence: {r.Confidence:P1}");
            }

            WriteWorkSheet2(results);

           // WriteWorkSheet(resultlist3bar);
            // write this to the google sheet
        }
        static void Main(string[] args)
        {
            Program p = new Program();

            //p.DoTwoBarTraining();
            // p.DoFourBarTraining();
            //p.DoFiveBarTraining();

            p.Run().Wait();


        }
    }



 

   
}
