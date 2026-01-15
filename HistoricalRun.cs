using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using PatternTrainer;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;


namespace CandlePatternML
{
       public partial class Program
    {

        MLEngineWrapper mlEngine2bar = new MLEngineWrapper(MLEngineWrapper.eMLEngineType.TwoBarPattern, "c:\\work\\TwoBarModel.zip");
        MLEngineWrapper mlEngine3bar = new MLEngineWrapper(MLEngineWrapper.eMLEngineType.ThreeBarPattern, "c:\\work\\ThreeBarModel.zip");
        MLEngineWrapper mlEngine4bar = new MLEngineWrapper(MLEngineWrapper.eMLEngineType.FourBarPattern, "c:\\work\\FourBarModel.zip");
        MLEngineWrapper mlEngine5bar = new MLEngineWrapper(MLEngineWrapper.eMLEngineType.FiveBarPattern, "c:\\work\\FiveBarModel.zip");


        public async Task RunHistory()
        {
            /*
             * get ticker data
             */

            var tickerWorksheet = await ReadWorkSheet("Tickers");

            TickerListWorksheetModel oTickerListWorksheetModel = new TickerListWorksheetModel();

            List<TickerListRowDataModel> TickerDataModelList = oTickerListWorksheetModel.RowDataList; //  new List<TickerListRowDataModel>();

            for (int i=-200; i <= -180; i++)
            {
                DateTime startDate = DateTime.Today.AddDays(i);
                Console.WriteLine($"Starting historical run for start date {startDate:d}");
                await HistoricalRun(TickerDataModelList,startDate);
            }

        }


        public async Task HistoricalRun(List<TickerListRowDataModel> TickerDataModelList, DateTime StartDate)
        {
            string authKey = GetAuthKey();

            List<string> tickersinplay = new List<string>();

            List<MLResult> resultlist5bar = new List<MLResult>();
            List<MLResult> resultlist4bar = new List<MLResult>();
            List<MLResult> resultlist3bar = new List<MLResult>();
            List<MLResult> resultlist2bar = new List<MLResult>();
            List<MLResult> resultRSI4bar = new List<MLResult>();

            SetupWorkSheetModel oSetupWorkSheetModel = new SetupWorkSheetModel();

            foreach (var tdm in TickerDataModelList.Take(10))//.Where(m=>m.Ticker == "NFLX"))
            {
                string ticker = tdm.Ticker;

                Console.WriteLine($"processing ticker {ticker}");
                GetCandleModel candleDataModel;
                try
                {
                    candleDataModel = oAPIWrapper.GetCandles(authKey, ticker, StartDate.AddDays(-400), StartDate, APIWrapper.eCandleTime.Daily);
                } catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving data for {ticker}: {ex.Message}");
                    continue;
                }

                if (candleDataModel.candles.Length < 10) continue; // not enough data

                DemarkPivotModel DemarkPivots = new DemarkPivotModel();
                DemarkPivots.FindPivots(candleDataModel.candles.ToList(), authKey);

                // save the most recent close
                tdm.LastClose = candleDataModel.candles[^1].close;
                // save all candle data so we can draw a chart
                tdm.oCandleModel = candleDataModel;
                // save pivot information
                tdm.oDemarkPivotModel = DemarkPivots;



                // get the results of the different tests

                MLResult resultTwoBar = DoTwoBarLive(mlEngine2bar, candleDataModel);
                resultlist2bar.Add(resultTwoBar);

                MLResult resultThreeBar = DoThreeBarLive(mlEngine3bar, candleDataModel);
                resultlist3bar.Add(resultThreeBar);

                MLResult resultFourBar = DoFourBarLive(mlEngine4bar, candleDataModel);
                resultlist4bar.Add(resultFourBar);

                MLResult resultFiveBar = DoFiveBarLive(mlEngine5bar, candleDataModel);
                resultlist5bar.Add(resultFiveBar);

                // get the RSI4bar result
                MLResult resultRSI4 = DoRSI4Live(candleDataModel);
                resultRSI4bar.Add(resultRSI4);

                // Save these to models to the database
                SaveModelsToDatabase(ticker, StartDate, DemarkPivots, resultTwoBar, resultThreeBar, resultFourBar, resultFiveBar, resultRSI4);


            }
 
        }


    }

   
}
