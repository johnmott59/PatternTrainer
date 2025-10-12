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
       public partial class Program
    {

        static List<string> Tickers = new List<string> {
"RKT","ALLY","AAPL","AMD","AMZN","ARM","AVGO","COIN","GOOGL",
"ISRG","LLY","META","MSFT","MSTR","NFLX","NVDA","PLTR","TSLA",
"OKLO","SMR","NNE","IONQ","RGTI","QUBT","QBTS","SOFI","ROKU",
"JPM","GS","ARRY","FSLR","BIDU","BABA","PDD","LRCX","MRNA",
"CRCL","ORCL","IBM","XBI","CRSP","IBIT","RBLX","RKLB","ASTS",
"UNH","ADI","ASML","V","MA","AXP","TMUS","WFC","C",
"BA","SPOT","GE","ANET","COST","CRWD","ZS","PANW","FTNT",
"SNOW","DDOG","CVNA","LULU","BBAI","BMNR","WULF","NBIS","IREN",
"WDC","STX","SNDK","SPY","QQQ","DIA"

};


        APIWrapper oAPIWrapper = new APIWrapper();
        public async Task Run()
        {
            /*
             * Read the worksheet containing the tickers
             */
            var tickers = await ReadWorkSheet("Tickers");

            List<string>tickerlist = new List<string>();
            foreach (var v in tickers.Rows)
            {
                tickerlist.Add(v.GetValue(0).ToString());
            }


            // delete all the files in the charts directory

            foreach (var file in System.IO.Directory.GetFiles("c:\\work\\charts"))
            {
                System.IO.File.Delete(file);
            }

            string authKey = GetAuthKey();
            List<string> tickersinplay = new List<string>();

            MLEngineWrapper mlEngine2bar = new MLEngineWrapper(MLEngineWrapper.eMLEngineType.TwoBarPattern, "c:\\work\\TwoBarModel.zip");
            MLEngineWrapper mlEngine3bar = new MLEngineWrapper(MLEngineWrapper.eMLEngineType.ThreeBarPattern,"c:\\work\\ThreeBarModel.zip");
            MLEngineWrapper mlEngine4bar = new MLEngineWrapper(MLEngineWrapper.eMLEngineType.FourBarPattern, "c:\\work\\FourBarModel.zip");
            MLEngineWrapper mlEngine5bar = new MLEngineWrapper(MLEngineWrapper.eMLEngineType.FiveBarPattern, "c:\\work\\FiveBarModel.zip");

            List<MLResult> resultlist5bar = new List<MLResult>();
            List<MLResult> resultlist4bar = new List<MLResult>();
            List<MLResult> resultlist3bar = new List<MLResult>();
            List<MLResult> resultlist2bar = new List<MLResult>();
            List<MLResult> resultRSI4bar = new List<MLResult>();

            WorkSheetModel results = new WorkSheetModel();

            foreach (var v in tickerlist) //.Where(m=>m == "CRSP"))
            {
                Console.WriteLine($"processing ticker {v}");
                GetCandleModel model;
                try
                {
                    model = oAPIWrapper.GetCandles(authKey, v, DateTime.Today.AddDays(-400), DateTime.Today, APIWrapper.eCandleTime.Daily);
                } catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving data for {v}: {ex.Message}");
                    continue;
                }

                // generate a png

                MLResult result2 = DoTwoBarLive(mlEngine2bar, model);
                resultlist2bar.Add(result2);

                MLResult result3 = DoThreeBarLive(mlEngine3bar, model);
                resultlist3bar.Add(result3);

                MLResult result4 = DoFourBarLive(mlEngine4bar, model);
                resultlist4bar.Add(result4);

                MLResult result5 = DoFiveBarLive(mlEngine5bar, model);
                resultlist5bar.Add(result5);

                // get the RSI4bar result
                MLResult result6 = DoRSI4Live(model);
                resultRSI4bar.Add(result6);

                // add to results worksheet
                results.AddTicker(v,result2,result3,result4,result5,result6);

            }

            /*
             * Generate a graphic for the succesul patterns. we start with shorter
             * patterns and allow the longer patterns to overwrite the shorter ones
             */

            Console.WriteLine("Two Bar Pattern Results:");
            foreach (var r in resultlist2bar.Where(m => m.Success))
            {
                if (!tickersinplay.Contains(r.Ticker)) tickersinplay.Add(r.Ticker);

                SchwabLib.Charting.GeneratePNG(r.CandleModel, 30, $"c:\\work\\charts\\{r.Ticker}.png", -2);
                Console.WriteLine($"{r.Ticker}  {r.Success}Confidence: {r.Confidence:P1}");
            }

            Console.WriteLine("Three Bar Pattern Results:");
            foreach (var r in resultlist3bar.Where(m => m.Success))
            {
                if (!tickersinplay.Contains(r.Ticker)) tickersinplay.Add(r.Ticker);
                SchwabLib.Charting.GeneratePNG(r.CandleModel, 30, $"c:\\work\\charts\\{r.Ticker}.png", -3);
                Console.WriteLine($"{r.Ticker} {r.Success} Confidence: {r.Confidence:P1}");
            }

            Console.WriteLine("Four Bar Pattern Results:");
            foreach (var r in resultlist4bar.Where(m => m.Success))
            {
                if (!tickersinplay.Contains(r.Ticker)) tickersinplay.Add(r.Ticker);
                SchwabLib.Charting.GeneratePNG(r.CandleModel, 30, $"c:\\work\\charts\\{r.Ticker}.png", -4);
                Console.WriteLine($"{r.Ticker} {r.Success} Confidence: {r.Confidence:P1}");
            }


            Console.WriteLine("Five Bar Pattern Results:");
            foreach (var r in resultlist5bar.Where(m => m.Success))
            {
                if (!tickersinplay.Contains(r.Ticker)) tickersinplay.Add(r.Ticker);
                SchwabLib.Charting.GeneratePNG(r.CandleModel, 30, $"c:\\work\\charts\\{r.Ticker}.png", -5);
                Console.WriteLine($"{r.Ticker} {r.Success} Confidence: {r.Confidence:P1}");
            }


            Console.WriteLine("RSI4 Pattern Results:");
            foreach (var r in resultRSI4bar.Where(m => m.Success))
            {
                if (!tickersinplay.Contains(r.Ticker)) tickersinplay.Add(r.Ticker);
                SchwabLib.Charting.GeneratePNG(r.CandleModel, 30, $"c:\\work\\charts\\{r.Ticker}.png", -1);
                Console.WriteLine($"{r.Ticker} {r.Success} Confidence: {r.Confidence:P1}");
            }

            // write the tickers to  a text file to load into TOS
            System.IO.File.WriteAllLines("c:\\work\\tickersinplay.txt", tickersinplay);





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
