using ScottPlot;

Random random = new();
int n = 1000;
double b = 5.0;
double step = 0.5;
double[] x = new double[n];
double[] rv = new double[n];
Func<double, double> PDF = (x) => (1.0/b) * System.Math.Exp(-System.Math.Max(0,x)/b);
Func<double, double> CDF = (x) => 1 - System.Math.Exp(-System.Math.Max(0,x)/b);
Func<double, double, double> PCDF = (a, b) => CDF(b) - CDF(a);

for (int i = 0; i < n; i++)
{
    x[i] = random.NextDouble();
    rv[i] = -b * System.Math.Log(x[i]);
}

double mx = rv.Max();

int np = (int) (System.Math.Ceiling(mx) / step + 1);
double[] dataX = [.. Enumerable.Range(0, np - 1).Select(i => i * step)];
int[] dataY = new int[np];

foreach (var item in rv)
{
    int p = (int) System.Math.Round(item / step);
    dataY[p] += 1;
}

for (int i = 1; i < np; i++)
    dataY[i] += dataY[i-1];

Console.WriteLine($"Sum: {dataY.Sum()}");
foreach (var data in dataY)
{
    Console.WriteLine(data);
}

ScottPlot.Plot myPlot = new();
var obs = myPlot.Add.Scatter(dataX, [..dataY.Select(d => (double) d/n)], Colors.Blue.WithAlpha(.8));
obs.LegendText = "Observed";

var actual = myPlot.Add.Function(CDF);
actual.LineColor = Colors.Orange;
actual.LegendText = "Actual";

myPlot.Axes.Title.Label.Text = $"Cummulative Distribution of {n} Exponential Random Variable with mean {b:f2}";
myPlot.Legend.Alignment = Alignment.UpperRight;
// myPlot.Axes.Title.Label.ForeColor = Colors.RebeccaPurple;
// myPlot.Axes.Title.Label.FontSize = 32;

myPlot.Axes.SetLimits(0, mx, 0, 1);

myPlot.ShowLegend();

// myPlot.SaveSvg("quickstart.svg", 400, 300);
myPlot.SaveSvg("temp/exp_cdf.svg", 800, 450);
myPlot.SavePng("temp/exp_cdf.png", 800, 450);