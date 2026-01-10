using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSC418ConsoleApp.Generators
{
    internal class ExponentialDistribution(double mean) : RandGen
    {
        private readonly RandGen _rand = RandGen.CreateUniform(0, 1);
        private readonly double _m = mean;

        public override double Next()
        {
            return -_m * Math.Log(_rand.Next());
        }
    }
}
