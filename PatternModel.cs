using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CandlePatternML
{

    public partial class Program
    {
        public void run()
        {
            float[] inputValues = new float[1]; //  GetYourSlidingWindow(); // Example: length 6 (3 bars * 2 values)
            int barCount = 3;

            var model = PatternModelFactory.GetModel(barCount);
            bool match = model.Predict(inputValues, out float prob);

            Console.WriteLine($"Match: {match}, Probability: {prob:P1}");
        }

        /// <summary>
        /// normalize the values in the range of 0 to 1
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public float[] Normalize(float[] values)
        {
            float min = values.Min();
            float max = values.Max();
            float range = Math.Max(1e-5f, max - min); // Avoid divide by zero
            float[] normalized = values.Select(v => (v - min) / range).ToArray();
            return normalized;
        }


    }
    public interface IPatternModel
    {
        bool Predict(float[] inputValues, out float probability);
    }


    public class PatternModel<TInput> : IPatternModel where TInput : class, new()
    {
        private readonly PredictionEngine<TInput, CandlePatternOutput> engine;
        private readonly Func<float[], TInput> mapper;

        public PatternModel(MLContext mlContext, string modelPath, Func<float[], TInput> inputMapper)
        {
            var model = mlContext.Model.Load(modelPath, out _);
            this.engine = mlContext.Model.CreatePredictionEngine<TInput, CandlePatternOutput>(model);
            this.mapper = inputMapper;
        }

        public bool Predict(float[] inputValues, out float probability)
        {
            var input = mapper(inputValues);
            var result = engine.Predict(input);
            probability = result.Probability;
            return result.IsMatch;
        }
    }
    public class CandleInput3
    {
        public float A { get; set; }
        public float B { get; set; }
        public float C { get; set; }
        public float D { get; set; }
        public float E { get; set; }
        public float F { get; set; }

        [ColumnName("Label")]
        public bool Label { get; set; }
    }

    public class CandleInput4
    {
        public float A { get; set; }
        public float B { get; set; }
        public float C { get; set; }
        public float D { get; set; }
        public float E { get; set; }
        public float F { get; set; }
        public float G { get; set; }
        public float H { get; set; }

        [ColumnName("Label")]
        public bool Label { get; set; }
    }

    public static class PatternModelFactory
    {
        private static readonly MLContext mlContext = new();

        private static readonly Dictionary<int, IPatternModel> modelCache = new();

        public static IPatternModel GetModel(int barCount)
        {
            if (modelCache.TryGetValue(barCount, out var model))
                return model;

            model = barCount switch
            {
                3 => new PatternModel<CandleInput3>(
                        mlContext,
                        "model_3bars.zip",
                        values => new CandleInput3
                        {
                            A = values[0],
                            B = values[1],
                            C = values[2],
                            D = values[3],
                            E = values[4],
                            F = values[5],
                            Label = false // Optional during inference
                        }),

                4 => new PatternModel<CandleInput4>(
                        mlContext,
                        "model_4bars.zip",
                        values => new CandleInput4
                        {
                            A = values[0],
                            B = values[1],
                            C = values[2],
                            D = values[3],
                            E = values[4],
                            F = values[5],
                            G = values[6],
                            H = values[7],
                            Label = false
                        }),

                _ => throw new NotSupportedException($"Unsupported bar count: {barCount}")
            };

            modelCache[barCount] = model;
            return model;
        }
    }



}

