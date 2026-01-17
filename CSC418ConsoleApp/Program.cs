using CSC418ConsoleApp.Models;
using ScottPlot;

namespace CSC418ConsoleApp;

public static class Program
{
    static void Main(string[] args)
    {
        // SSQ Driver code
        SingleServerQueueing ssq = new(1, 0.5);
        ssq.startSim(10000, 1000);

        // Inventory system Driver code
        //List<Tuple<int, double>> demand_dist = [Tuple.Create(1, 1.0 / 6), Tuple.Create(2, 1.0 / 3)];
        //double meanInterDemandTime = 0.1;
        //Tuple<double, double> lagTime = Tuple.Create(0.5, 1.0);

        //InvertorySystem ivsys = new(demand_dist, meanInterDemandTime, lagTime, 1);

        //double simEnd = 120;
        //List<(int S, int s)> policies = [(20, 40), (20, 60), (20, 80), (20, 100), (40, 60), (40, 80), (40, 100), (60, 80), (60, 100)];

    }
}

