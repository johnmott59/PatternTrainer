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

        static void Main(string[] args)
        {
            Program p = new Program();

            //   p.DoTwoBarTraining();
            //  p.DoThreeBarTraining();
            //   p.DoFourBarTraining();
            //   p.DoFiveBarTraining();

          //  p.RunHistory().Wait();

           p.Run().Wait();


        }
    }
}
