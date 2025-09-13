using SchwabLib.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CandlePatternML
{
    /// <summary>
    /// utility to write out a set of 3-bar patterns as SVG images in an HTML file.
    /// </summary>
    public static class ThreeBarSvgReporter
    {
        public static void WriteSvgHtml(
            string filePath,
            List<ThreeBarPatternModel> patterns,
            List<DateTime> starts,
            List<CandlePatternOutput> outputs,
            int panelWidth = 300,          // 2:3 ratio -> 300x450 by default
            int panelHeight = 450,
            bool colorByLabel = true)
        {
            if (patterns == null) throw new ArgumentNullException(nameof(patterns));
            if (starts == null) throw new ArgumentNullException(nameof(starts));
            if (patterns.Count != starts.Count)
                throw new ArgumentException("patterns and starts must have the same length.");

            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset=\"utf-8\"><title>3-Bar Patterns</title></head><body style=\"font-family: sans-serif;\">");

            // Wrap each panel in its own SVG
            for (int i = 0; i < patterns.Count; i++)
            {
                var p = patterns[i];
                var start = starts[i];
                var output = outputs[i];

               // sb.AppendLine(RenderPanelSvg(p, start, output, panelWidth, panelHeight, colorByLabel));
                sb.AppendLine(RenderPanelCandleSvg(p, start, output, panelWidth, panelHeight, colorByLabel));
            }

            sb.AppendLine("</body></html>");

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, sb.ToString());
        }

        private static string RenderPanelSvg(
            ThreeBarPatternModel p, 
            DateTime start, 
            CandlePatternOutput output,
            int width, int height, bool colorByLabel)
        {
            // Margins
            const int left = 20;
            const int right = 70;   // room for price labels
            const int top = 10;
            const int bottom = 35;  // room for date

            int drawWidth = width - left - right;
            int drawHeight = height - top - bottom;

            // Extract highs/lows
            var lows = new[] { p.GapBarLowHigh.Low, p.Hold1BarLowHigh.Low, p.Hold2Low };
            var highs = new[] { p.GapBarLowHigh.High, p.Hold1BarLowHigh.High, p.Hold2High };

            float min = lows.Min();
            float max = highs.Max();

            // avoid zero height range
            if (Math.Abs(max - min) < 1e-9)
            {
                max += 1;
                min -= 1;
            }

            // y scale (price -> SVG y)
            double ScaleY(double price) => top + (max - price) / (max - min) * drawHeight;

            // x positions for the 3 bars
            // space them evenly with gaps
            int nBars = 3;
            double gapFrac = 0.15; // fraction of each slot for spacing
            double slotWidth = (double)drawWidth / nBars;
            double boxWidth = slotWidth * (1.0 - gapFrac);

            double XForBar(int idx) => left + slotWidth * idx + (slotWidth - boxWidth) / 2.0;

            // ticks (5 evenly spaced)
            var ticks = Enumerable.Range(0, 5).Select(t => min + (float)t * (max - min) / 4f).ToArray();

            string stroke = colorByLabel
                ? (p.Label ? "#1a7f37" : "#b91c1c") // green / red
                : "#000";

            var sb = new StringBuilder();
            sb.AppendLine($"<svg width=\"{width}\" height=\"{height}\" viewBox=\"0 0 {width} {height}\" " +
                          "xmlns=\"http://www.w3.org/2000/svg\" style=\"border:1px solid #ddd; margin:6px;\">");

            // Background
            sb.AppendLine($"  <rect x=\"0\" y=\"0\" width=\"{width}\" height=\"{height}\" fill=\"white\"/>");

            // Axis line
            double axisX = width - right + 20;
            sb.AppendLine($"  <line x1=\"{axisX}\" y1=\"{top}\" x2=\"{axisX}\" y2=\"{top + drawHeight}\" stroke=\"#aaa\" stroke-width=\"1\"/>");

            // Ticks + labels
            for (int t = 0; t < ticks.Length; t++)
            {
                double y = ScaleY(ticks[t]);
                sb.AppendLine($"  <line x1=\"{axisX - 5}\" y1=\"{y}\" x2=\"{axisX}\" y2=\"{y}\" stroke=\"#aaa\" stroke-width=\"1\"/>");
                sb.AppendLine($"  <text x=\"{axisX + 2}\" y=\"{y + 4}\" font-size=\"10\" fill=\"#333\">{ticks[t].ToString("0.#####")}</text>");
            }

            // Candle boxes (only outlines)
            // Bar 1
            sb.AppendLine(RectStroke(XForBar(0), ScaleY(p.GapBarLowHigh.High), boxWidth, ScaleY(p.GapBarLowHigh.Low) - ScaleY(p.GapBarLowHigh.High), stroke));
            // Bar 2
            sb.AppendLine(RectStroke(XForBar(1), ScaleY(p.Hold1High), boxWidth, ScaleY(p.Hold1BarLowHigh.Low) - ScaleY(p.Hold1High), stroke));
            // Bar 3
            sb.AppendLine(RectStroke(XForBar(2), ScaleY(p.Hold2High), boxWidth, ScaleY(p.Hold2Low) - ScaleY(p.Hold2High), stroke));

            // Date
            sb.AppendLine($"  <text x=\"{width / 2.0}\" y=\"{height - 10}\" font-size=\"12\" text-anchor=\"middle\" fill=\"#333\"><tspan font-weight=\"bold\">{output.Probability*100:F2}%</tspan> {start:yyyy-MM-dd}</text>");

            // Optional: label flag
            if (colorByLabel)
            {
                string lbl = ""; // output.Probability.ToString("P1", CultureInfo.InvariantCulture);
                sb.AppendLine($"  <text x=\"{left}\" y=\"{top + 12}\" font-size=\"12\" fill=\"{stroke}\">{lbl}</text>");
            }

            sb.AppendLine("</svg>");
            return sb.ToString();
        }

        private static string RenderPanelCandleSvg(
            ThreeBarPatternModel p, 
            DateTime start, 
            CandlePatternOutput output,
            int width, int height, bool colorByLabel)
        {
            // Margins
            const int left = 20;
            const int right = 70;   // room for price labels
            const int top = 10;
            const int bottom = 35;  // room for date

            int drawWidth = width - left - right;
            int drawHeight = height - top - bottom;

            // Extract highs/lows from candles for scaling
            var lows = new[] { p.GapCandle.low, p.Hold1Candle.low, p.Hold2Candle.low };
            var highs = new[] { p.GapCandle.high, p.Hold1Candle.high, p.Hold2Candle.high };

            float min = (float) lows.Min();
            float max = (float) highs.Max();

            // avoid zero height range
            if (Math.Abs(max - min) < 1e-9)
            {
                max += 1;
                min -= 1;
            }

            // y scale (price -> SVG y)
            double ScaleY(double price) => top + (max - price) / (max - min) * drawHeight;

            // x positions for the 3 bars
            // space them evenly with gaps
            int nBars = 3;
            double gapFrac = 0.15; // fraction of each slot for spacing
            double slotWidth = (double)drawWidth / nBars;
            double boxWidth = slotWidth * (1.0 - gapFrac);

            double XForBar(int idx) => left + slotWidth * idx + (slotWidth - boxWidth) / 2.0;

            // ticks (5 evenly spaced)
            var ticks = Enumerable.Range(0, 5).Select(t => min + (float)t * (max - min) / 4f).ToArray();

            string stroke = colorByLabel
                ? (p.Label ? "#1a7f37" : "#b91c1c") // green / red
                : "#000";

            var sb = new StringBuilder();
            sb.AppendLine($"<svg width=\"{width}\" height=\"{height}\" viewBox=\"0 0 {width} {height}\" " +
                          "xmlns=\"http://www.w3.org/2000/svg\" style=\"border:1px solid #ddd; margin:6px;\">");

            // Background
            sb.AppendLine($"  <rect x=\"0\" y=\"0\" width=\"{width}\" height=\"{height}\" fill=\"white\"/>");

            // Axis line
            double axisX = width - right + 20;
            sb.AppendLine($"  <line x1=\"{axisX}\" y1=\"{top}\" x2=\"{axisX}\" y2=\"{top + drawHeight}\" stroke=\"#aaa\" stroke-width=\"1\"/>");

            // Ticks + labels
            for (int t = 0; t < ticks.Length; t++)
            {
                double y = ScaleY(ticks[t]);
                sb.AppendLine($"  <line x1=\"{axisX - 5}\" y1=\"{y}\" x2=\"{axisX}\" y2=\"{y}\" stroke=\"#aaa\" stroke-width=\"1\"/>");
                sb.AppendLine($"  <text x=\"{axisX + 2}\" y=\"{y + 4}\" font-size=\"10\" fill=\"#333\">{ticks[t].ToString("0.#####")}</text>");
            }

            // Render candlesticks
            // Bar 1 (Gap)
            RenderCandlestick(sb, XForBar(0), p.GapCandle, boxWidth, stroke, ScaleY);
            // Bar 2 (Hold1)
            RenderCandlestick(sb, XForBar(1), p.Hold1Candle, boxWidth, stroke, ScaleY);
            // Bar 3 (Hold2)
            RenderCandlestick(sb, XForBar(2), p.Hold2Candle, boxWidth, stroke, ScaleY);

            // Date
            sb.AppendLine($"  <text x=\"{width / 2.0}\" y=\"{height - 10}\" font-size=\"12\" text-anchor=\"middle\" fill=\"#333\"><tspan font-weight=\"bold\">{output.Probability*100:F2}%</tspan> {start:yyyy-MM-dd}</text>");

            // Optional: label flag
            if (colorByLabel)
            {
                string lbl = ""; // output.Probability.ToString("P1", CultureInfo.InvariantCulture);
                sb.AppendLine($"  <text x=\"{left}\" y=\"{top + 12}\" font-size=\"12\" fill=\"{stroke}\">{lbl}</text>");
            }

            sb.AppendLine("</svg>");
            return sb.ToString();
        }

        private static void RenderCandlestick(StringBuilder sb, double x, Candle candle, double boxWidth, string stroke, Func<double, double> scaleY)
        {
            // Determine if bullish (green) or bearish (red)
            bool isBullish = candle.close > candle.open;
            string fillColor = isBullish ? "#1a7f37" : "#b91c1c";
            string strokeColor = stroke;

            // Calculate body positions
            double bodyTop = Math.Min((double)candle.open,(double) candle.close);
            double bodyBottom = Math.Max((double) candle.open, (double) candle.close);
            double bodyHeight = bodyBottom - bodyTop;

            // Render wick (high to low)
            double wickX = x + boxWidth / 2.0;
            sb.AppendLine($"  <line x1=\"{wickX:F2}\" y1=\"{scaleY((double)candle.high):F2}\" x2=\"{wickX:F2}\" y2=\"{scaleY((double)candle.low):F2}\" stroke=\"{strokeColor}\" stroke-width=\"1.5\"/>");

            // Render body
            if (bodyHeight > 0)
            {
                double bodyY = scaleY(bodyBottom); // SVG y is top-down, so use bodyBottom for y position
                double bodyHeightScaled = scaleY(bodyTop) - scaleY(bodyBottom); // Calculate scaled height
                sb.AppendLine($"  <rect x=\"{x:F2}\" y=\"{bodyY:F2}\" width=\"{boxWidth:F2}\" height=\"{bodyHeightScaled:F2}\" " +
                             $"fill=\"{fillColor}\" stroke=\"{strokeColor}\" stroke-width=\"1.5\"/>");
            }
            else
            {
                // Doji - just a line for the body
                sb.AppendLine($"  <line x1=\"{x:F2}\" y1=\"{scaleY((double)candle.open):F2}\" x2=\"{x + boxWidth:F2}\" y2=\"{scaleY((double)candle.open):F2}\" stroke=\"{strokeColor}\" stroke-width=\"1.5\"/>");
            }
        }

        private static string RectStroke(double x, double y, double w, double h, string stroke)
        {
            // Ensure positive height (if data reversed)
            if (h < 0) { y += h; h = -h; }
            return $"  <rect x=\"{x:F2}\" y=\"{y:F2}\" width=\"{w:F2}\" height=\"{h:F2}\" " +
                   $"fill=\"none\" stroke=\"{stroke}\" stroke-width=\"1.5\"/>";
        }
    }


}