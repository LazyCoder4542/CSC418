using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSC418ConsoleApp.Utils
{
    internal static class Plot
    {
        public static void HistoPDF(double[] data, string xLabel, string yLabel = "Proportion", string path = "histogram.png")
        {
            ScottPlot.Plot myPlot = new();

            // Create a histogram from a collection of values
            var hist = ScottPlot.Statistics.Histogram.WithBinCount(100, data);

            // Display the histogram as a bar plot
            var barPlot = myPlot.Add.Bars(hist.Bins, hist.GetProbability());

            // Customize the style of each bar
            foreach (var bar in barPlot.Bars)
            {
                bar.Size = hist.FirstBinSize;
                bar.LineWidth = 0;
                bar.FillStyle.AntiAlias = false;
                bar.FillColor = Colors.C0.Lighten(.3);
            }

            // Plot the probability curve on top the histogram
            ScottPlot.Statistics.ProbabilityDensity pd = new(data);
            double[] xs = Generate.Range(data.Min(), data.Max(), 0.005);
            double sumBins = hist.Bins.Select(x => pd.GetY(x)).Sum();
            double[] ys = pd.GetYs(xs, 1.0 / sumBins);

            var curve = myPlot.Add.ScatterLine(xs, ys);
            curve.LineWidth = 2;
            curve.LineColor = Colors.Black;
            curve.LinePattern = LinePattern.DenselyDashed;

            // Customize plot style
            myPlot.Axes.Margins(bottom: 0);
            myPlot.YLabel(yLabel);
            myPlot.XLabel(xLabel);

            myPlot.SavePng(Path.Combine(Globals._targetFolder, path), 600, 450);
        }
    }
}
