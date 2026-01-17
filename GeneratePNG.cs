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
        public void GeneratePNG(List<MLResult> resultlist2bar,
            List<MLResult> resultlist3bar,
            List<MLResult> resultlist4bar,
            List<MLResult> resultlist5bar,
            List<MLResult> resultRSI4bar,
            List<TickerListRowDataModel> TickerDataModelList, 
            List<string> tickersinplay)
        {
            /*
   * Generate a graphic for the succesul patterns. we start with shorter
   * patterns and allow the longer patterns to overwrite the shorter ones
   */

            Console.WriteLine("Two Bar Pattern Results:");
            foreach (var r in resultlist2bar.Where(m => m.Success))
            {
                if (!tickersinplay.Contains(r.Ticker)) tickersinplay.Add(r.Ticker);
                TickerListRowDataModel tdm = TickerDataModelList.Find(m => m.Ticker == r.Ticker);

                //v.NextToLastPivotHigh, v.LatestPivotHigh, v.PivotHighTrendBreak);


                SchwabLib.Charting.GeneratePNG(r.CandleModel,
                    30,
                    $"c:\\work\\charts\\{r.Ticker}.png",
                    -2,
                     tdm.oDemarkPivotModel.NextToLastPivotHigh,
                     tdm.oDemarkPivotModel.LatestPivotHigh
                    );
                Console.WriteLine($"{r.Ticker}  {r.Success}Confidence: {r.Confidence:P1}");
            }

            Console.WriteLine("Three Bar Pattern Results:");
            foreach (var r in resultlist3bar.Where(m => m.Success))
            {
                if (!tickersinplay.Contains(r.Ticker)) tickersinplay.Add(r.Ticker);

                TickerListRowDataModel tdm = TickerDataModelList.Find(m => m.Ticker == r.Ticker);

                //v.NextToLastPivotHigh, v.LatestPivotHigh, v.PivotHighTrendBreak);

                SchwabLib.Charting.GeneratePNG(r.CandleModel,
                    30,
                    $"c:\\work\\charts\\{r.Ticker}.png",
                    -3,
                     tdm.oDemarkPivotModel.NextToLastPivotHigh,
                     tdm.oDemarkPivotModel.LatestPivotHigh
                    );
                Console.WriteLine($"{r.Ticker} {r.Success} Confidence: {r.Confidence:P1}");
            }

            Console.WriteLine("Four Bar Pattern Results:");
            foreach (var r in resultlist4bar.Where(m => m.Success))
            {
                if (!tickersinplay.Contains(r.Ticker)) tickersinplay.Add(r.Ticker);

                TickerListRowDataModel tdm = TickerDataModelList.Find(m => m.Ticker == r.Ticker);

                //v.NextToLastPivotHigh, v.LatestPivotHigh, v.PivotHighTrendBreak);

                SchwabLib.Charting.GeneratePNG(r.CandleModel,
                    30,
                    $"c:\\work\\charts\\{r.Ticker}.png",
                    -4,
                    tdm.oDemarkPivotModel.NextToLastPivotHigh,
                    tdm.oDemarkPivotModel.LatestPivotHigh

                    );
                Console.WriteLine($"{r.Ticker} {r.Success} Confidence: {r.Confidence:P1}");
            }


            Console.WriteLine("Five Bar Pattern Results:");
            foreach (var r in resultlist5bar.Where(m => m.Success))
            {
                if (!tickersinplay.Contains(r.Ticker)) tickersinplay.Add(r.Ticker);

                TickerListRowDataModel tdm = TickerDataModelList.Find(m => m.Ticker == r.Ticker);

                //v.NextToLastPivotHigh, v.LatestPivotHigh, v.PivotHighTrendBreak);

                SchwabLib.Charting.GeneratePNG(r.CandleModel,
                    30,
                    $"c:\\work\\charts\\{r.Ticker}.png",
                    -5,
                    tdm.oDemarkPivotModel.NextToLastPivotHigh,
                    tdm.oDemarkPivotModel.LatestPivotHigh);
                Console.WriteLine($"{r.Ticker} {r.Success} Confidence: {r.Confidence:P1}");
            }


            Console.WriteLine("RSI4 Pattern Results:");
            foreach (var r in resultRSI4bar.Where(m => m.Success))
            {
                if (!tickersinplay.Contains(r.Ticker)) tickersinplay.Add(r.Ticker);

                TickerListRowDataModel tdm = TickerDataModelList.Find(m => m.Ticker == r.Ticker);

                SchwabLib.Charting.GeneratePNG(r.CandleModel,
                    30,
                    $"c:\\work\\charts\\{r.Ticker}.png",
                    -1,
                    tdm.oDemarkPivotModel.NextToLastPivotHigh,
                    tdm.oDemarkPivotModel.LatestPivotHigh
                    );
                Console.WriteLine($"{r.Ticker} {r.Success} Confidence: {r.Confidence:P1}");
            }
        }
    }
}
