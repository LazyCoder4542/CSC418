using CSC418ConsoleApp.Generators;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSC418ConsoleApp.Models
{
    internal class InventorySystem
    {
        // G Variables
        private readonly double eD; // expected inter-demand time
        private readonly Tuple<double, double> eL; // expected lag time
        private readonly double rI; // restock interval
        private readonly Costs costs; // Product costs
        
        // Generator;
        private readonly RandGen<int> sizeRand;
        private readonly RandGen<double> dRand;
        private readonly RandGen<double> lRand;

        // Others
        private double I; // Inventory stock level
        private double s;
        private double S;

        // Stat Counters

        internal InventorySystem(List<Tuple<int, double>> demandSizeDist, double interDemandTime, Tuple<double, double> lagTime, double restockInterval, Costs costs)
        {
            eD = interDemandTime;
            eL = lagTime;
            rI = restockInterval;
            this.costs = costs;

            sizeRand = RandGen.CreateDiscreteDist(demandSizeDist);
            dRand = RandGen.CreateExponential(eD);
            lRand = RandGen.CreateUniform(eL.Item1, eL.Item2);
        }
    }
    /// <summary>
    /// Represents the costs for a single product.
    /// </summary>
    /// <param name="K">Setup cost.</param>
    /// <param name="I">Order cost.</param>
    /// <param name="H">Holding cost.</param>
    /// <param name="Pi">Backlog cost.</param>
    internal record Costs(double K, double I, double H, double Pi);
}
