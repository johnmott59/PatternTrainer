using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CandlePatternML
{
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

                sb.AppendLine(RenderPanelSvg(p, start, output, panelWidth, panelHeight, colorByLabel));
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

        private static string RectStroke(double x, double y, double w, double h, string stroke)
        {
            // Ensure positive height (if data reversed)
            if (h < 0) { y += h; h = -h; }
            return $"  <rect x=\"{x:F2}\" y=\"{y:F2}\" width=\"{w:F2}\" height=\"{h:F2}\" " +
                   $"fill=\"none\" stroke=\"{stroke}\" stroke-width=\"1.5\"/>";
        }
    }


}