using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;


namespace CandlePatternML
{

    public partial class MLEngineWrapper
    {
        public enum eMLEngineType
        {
            TwoBarPattern,
            ThreeBarPattern
        }
        public MLContext mlContext { get; set; }

        // load the model from a file
        ITransformer loadedModel;
        DataViewSchema modelSchema;
        public PredictionEngine<ThreeBarPatternModel, CandlePatternOutput> predictionEngine3Bar { get; set; }
        public PredictionEngine<TwoBarPatternModel, CandlePatternOutput> predictionEngine2Bar { get; set; }


        public MLEngineWrapper(eMLEngineType eEngine, string ModelFile)
        {
            // 1. Create MLContext
            mlContext = new MLContext(seed: 1);

            using (var fileStream = new FileStream(ModelFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = mlContext.Model.Load(fileStream, out modelSchema);
            }

            switch (eEngine)
            {
                case eMLEngineType.TwoBarPattern:
                    predictionEngine2Bar = mlContext
                        .Model
                        .CreatePredictionEngine<TwoBarPatternModel, CandlePatternOutput>(loadedModel);
                    break;

                case eMLEngineType.ThreeBarPattern:
                    // create a prediction engine
                    predictionEngine3Bar = mlContext
                        .Model
                        .CreatePredictionEngine<ThreeBarPatternModel, CandlePatternOutput>(loadedModel);

                    break;
                default:
                    throw new Exception($"Unknown engine type {eEngine}");
            }   

         
        }
    }
}
