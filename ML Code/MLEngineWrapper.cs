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
        public MLContext mlContext { get; set; }

        // load the model from a file
        ITransformer loadedModel;
        DataViewSchema modelSchema;
        public PredictionEngine<ThreeBarPatternModel, CandlePatternOutput> predictionEngine { get; set; }

        public MLEngineWrapper(string ModelFile = "c:\\work\\ThreeBarModel.zip")
        {
            // 1. Create MLContext
            mlContext = new MLContext(seed: 1);

            using (var fileStream = new FileStream(ModelFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = mlContext.Model.Load(fileStream, out modelSchema);
            }

            // create a prediction engine
            predictionEngine = mlContext.Model
                .CreatePredictionEngine<ThreeBarPatternModel, CandlePatternOutput>(loadedModel);

        }
    }
}
