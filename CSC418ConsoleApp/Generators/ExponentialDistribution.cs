using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSC418ConsoleApp.Generators
{
    internal class ExponentialDistribution(double mean, Random? stream = null) : RandGen<double>
    {
        private readonly RandGen<double> _rand = RandGen.CreateUniform(0, 1, stream);
        private readonly double _m = mean;

        public override double Next()
        {
            return -_m * Math.Log(_rand.Next());
        }
        public static double SeedNext(double mean, Random stream)
        {
            ExponentialDistribution self = new(mean, stream);
            return self.Next();
        }
    }
}
