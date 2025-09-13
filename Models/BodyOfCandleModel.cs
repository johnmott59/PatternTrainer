using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using SchwabLib.Models;
using System;
using System.Collections.Generic;


namespace CandlePatternML
{

    public sealed class BodyOfCandleModel
    {
        public Candle Candle { get; }

        public BodyOfCandleModel(Candle candle)
        {
            Candle = candle ?? throw new ArgumentNullException(nameof(candle));
        }

        private decimal BodyMin => Math.Min(Candle.open, Candle.close);
        private decimal BodyMax => Math.Max(Candle.open, Candle.close);

        public static bool operator <(BodyOfCandleModel left, BodyOfCandleModel right)
        {
            if (left is null) throw new ArgumentNullException(nameof(left));
            if (right is null) throw new ArgumentNullException(nameof(right));
            return left.BodyMax < right.BodyMin;
        }

        public static bool operator >(BodyOfCandleModel left, BodyOfCandleModel right)
        {
            if (left is null) throw new ArgumentNullException(nameof(left));
            if (right is null) throw new ArgumentNullException(nameof(right));
            return left.BodyMin > right.BodyMax;
        }

#if false
    public static bool operator <=(BodyOfCandle left, BodyOfCandle right)
    {
        // TODO: implement later
        throw new NotImplementedException();
    }

    public static bool operator >=(BodyOfCandle left, BodyOfCandle right)
    {
        // TODO: implement later
        throw new NotImplementedException();
    }

    public static bool operator ==(BodyOfCandle left, BodyOfCandle right)
    {
        // TODO: implement later
        throw new NotImplementedException();
    }

    public static bool operator !=(BodyOfCandle left, BodyOfCandle right)
    {
        // TODO: implement later
        throw new NotImplementedException();
    }
#endif
    }


}
