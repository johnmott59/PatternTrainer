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


        APIWrapper oAPIWrapper = new APIWrapper();
        public async Task Run()
        {
            string authKey = GetAuthKey();

            //   var emodel = oAPIWrapper.GetCandles(authKey, "SPY", DateTime.Today.AddDays(-40), DateTime.Today, APIWrapper.eCandleTime.Daily);

            //  FindDemarkPivots(emodel.candles.ToList());

        /*
         * Read the worksheet containing the tickers
         */
        var tickerWorksheet = await ReadWorkSheet("Tickers");

#if false
// this is how we will add fields and update the worksheet
// this logic will go in the update routine to add in close and pivot values
            // insert a row for the headers
            tickerWorksheet.Rows.Insert(0, new WSRow(0, tickerWorksheet)) ;

            tickerWorksheet.SetValue(0, "A", "Ticker");
            tickerWorksheet.SetValue(0, "B", "Last Close");
            tickerWorksheet.SetValue(0, "C", "Pivot High");

            int ndx = 0;
            foreach (var v in tickerWorksheet.Rows.Skip(1))
            {
                v.SetValue(1, ndx++);
            }
            
            UpdateWorkSheet(tickerWorksheet);
#endif

            List<TickerDataModel> TickerDataModelList = new List<TickerDataModel>();

            // read the list of tickers and create ticker data models. so far they only contain the ticker symbol
            foreach (var v in tickerWorksheet.Rows)
            {
                TickerDataModelList.Add(new TickerDataModel() { Ticker = v.GetValue(0).ToString() }); 
            }

            // delete all the files in the charts directory

            foreach (var file in System.IO.Directory.GetFiles("c:\\work\\charts"))
            {
                System.IO.File.Delete(file);
            }

            //string authKey = GetAuthKey();
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

            SetupWorkSheetModel oSetupWorkSheetModel = new SetupWorkSheetModel();

            foreach (var tdm in TickerDataModelList) //.Where(m=>m == "CRSP"))
            {
                string ticker = tdm.Ticker;

                Console.WriteLine($"processing ticker {ticker}");
                GetCandleModel model;
                try
                {
                    model = oAPIWrapper.GetCandles(authKey, ticker, DateTime.Today.AddDays(-400), DateTime.Today, APIWrapper.eCandleTime.Daily);
                } catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving data for {ticker}: {ex.Message}");
                    continue;
                }

                // find recent pivots
                var DemarkPivots =  FindDemarkPivots(model.candles.ToList());

                // update this ticker data model with the pivots
                tdm.LatestPivotHigh = DemarkPivots.LatestPivotHigh;
                tdm.LatestPivotLow = DemarkPivots.LatestPivotLow;
                tdm.NextToLastPivotLow = DemarkPivots.NextToLastPivotLow;
                tdm.NextToLastPivotHigh = DemarkPivots.NextToLastPivotHigh;

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
                oSetupWorkSheetModel.AddTicker(ticker,DemarkPivots,result2,result3,result4,result5,result6);

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

            /*
             * Write out the ticker worksheet with the pivot data
             */
            UpdateTickerWorksheet(tickerWorksheet,TickerDataModelList);

            /*
             * write out the worksheet with the ML results
             */

            WriteSetupsToWorksheet(oSetupWorkSheetModel);

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
