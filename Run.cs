using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using PatternTrainer;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;


namespace CandlePatternML
{
       public partial class Program
    {
        public async Task Run()
        {
            string authKey = GetAuthKey();

            //   var emodel = oAPIWrapper.GetCandles(authKey, "SPY", DateTime.Today.AddDays(-40), DateTime.Today, APIWrapper.eCandleTime.Daily);

            //  FindDemarkPivots(emodel.candles.ToList());

        /*
         * Read the worksheet containing the tickers. this contains a list of models to handle each ticker 
         * and allow it to be filled in with model data
         */

        TickerListWorksheetModel oTickerListWorksheetModel = new TickerListWorksheetModel();

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

            foreach (var tdm in oTickerListWorksheetModel.RowDataList)//.Where(m=>m.Ticker == "NFLX"))
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

                if (model.candles.Length < 10) continue; // not enough data

                DemarkPivotModel DemarkPivots = new DemarkPivotModel();
                DemarkPivots.FindPivots(model.candles.ToList(), GetAuthKey());

                // save the most recent close
                tdm.LastClose = model.candles[^1].close;
                // save all candle data so we can draw a chart
                tdm.oCandleModel = model;
                // save pivot information
                tdm.oDemarkPivotModel = DemarkPivots;



                // get the results of the different tests

                MLResult resultTwoBar = DoTwoBarLive(mlEngine2bar, model);
                resultlist2bar.Add(resultTwoBar);

                MLResult resultThreeBar = DoThreeBarLive(mlEngine3bar, model);
                resultlist3bar.Add(resultThreeBar);

                MLResult resultFourBar = DoFourBarLive(mlEngine4bar, model);
                resultlist4bar.Add(resultFourBar);

                MLResult resultFiveBar = DoFiveBarLive(mlEngine5bar, model);
                resultlist5bar.Add(resultFiveBar);

                // get the RSI4bar result
                MLResult resultRSI4 = DoRSI4Live(model);
                resultRSI4bar.Add(resultRSI4);

                // Save these to models to the database
                SaveModelsToDatabase(ticker,DateTime.Now, DemarkPivots, resultTwoBar, resultThreeBar, resultFourBar, resultFiveBar, resultRSI4);

                // add to results worksheet
                oSetupWorkSheetModel.AddTicker(ticker,DemarkPivots,resultTwoBar,resultThreeBar,resultFourBar,resultFiveBar,resultRSI4);

            }

            /*
             * Generate PNG files for the matching patterns
             */

            GeneratePNG(resultlist2bar, resultlist3bar, resultlist4bar, resultlist5bar,resultRSI4bar , oTickerListWorksheetModel.RowDataList, tickersinplay);

            // write the tickers to  a text file to load into TOS
            System.IO.File.WriteAllLines("c:\\work\\tickersinplay.txt", tickersinplay);

            /*
             * Write out the ticker worksheet with the pivot data
             */

            oTickerListWorksheetModel.UpdateTickerWorksheet();

            /*
             * write out the worksheet with the ML results. this goes to the sheet called 'sheet1' and this sheet only has setups
             * that have triggered a buy signal
             */

            WriteSetupsToWorksheet(oSetupWorkSheetModel);

           // WriteWorkSheet(resultlist3bar);
            // write this to the google sheet
        }


    }



 

   
}
