using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSC418ConsoleApp.Generators
{
    internal class UniformDistribution : RandGen
    {
        private readonly Random _random;
        private readonly double _min;
        private readonly double _max;
        public UniformDistribution(double min, double max)
        {
            _random = new Random();
            _min = min;
            _max = max;
        }
        override public double Next()
        {
            return _min + (_random.NextDouble() * (_max - _min));
        }
    }
}
