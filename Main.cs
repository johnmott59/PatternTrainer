using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SchwabLib;
using SchwabLib.Models;
using SchwabLib.OptionModel;
using System;
using System.Collections.Generic;
using static SchwabLib.TheilSenEstimator;

namespace CandlePatternML
{

    public partial class Program
    {

        static void Main(string[] args)
        {
            Program p = new Program();

            //   p.DoTwoBarTraining();
            //  p.DoThreeBarTraining();
            //   p.DoFourBarTraining();
            //   p.DoFiveBarTraining();

            //  p.RunHistory().Wait();

            p.Run().Wait();

           // p.GetDividend();


        }


            List<string> tickers = new List<string>
        {

"ABR",
"ACP",
"ADAMI",
"ADX",
"ARCC",
"ASGI",
"AVK",
"BITO",
"BKLN",
"BTCI",
"CAIE",
"CEFS",
"CLOZ",
"CSWC",
"EIC",
"FSCO",
"GSBD",
"HTD",
"HTGC",
"IYRI",
"JBBB",
"JEPI",
"JLS",
"NCV",
"NIE",
"NML",
"PAXS",
"PBDC",
"PCN",
"PDI",
"PFFA",
"PTY",
"STWD",
"SVOL",
"THQ",

        };
        public void GetDividend()
        {
            string authKey = GetAuthKey();

            List<(string, decimal)> results = new List<(string, decimal)>();

            foreach (var t in tickers)
            {
                string quoteData = oAPIWrapper.GetFullQuotedata(authKey, t);

                JObject root = JObject.Parse(quoteData);

                foreach (var symbolProperty in root.Properties())
                {
                    string symbol = symbolProperty.Name;
                    JObject data = (JObject)symbolProperty.Value;

                    decimal? divAmount = data["fundamental"]?["divAmount"]?.Value<decimal>();
                    int? divFreq = data["fundamental"]?["divFreq"]?.Value<int>();
                    decimal? divPayAmount = data["fundamental"]?["divPayAmount"]?.Value<decimal>();
                    decimal? lastPrice = data["quote"]?["lastPrice"]?.Value<decimal>();

                    decimal AnnulizedDividend = divPayAmount.Value * divFreq.Value;
                    decimal DividendYield = (AnnulizedDividend / lastPrice.GetValueOrDefault()) * 100;

                    results.Add((symbol, DividendYield));
                }

                results = results.OrderByDescending(r => r.Item2).ToList();
      
            }

            foreach (var r in results.OrderByDescending(r => r.Item2))
            {
                Console.WriteLine($"{r.Item1}: {r.Item2:F2}%");
            }


        }
    }
}
