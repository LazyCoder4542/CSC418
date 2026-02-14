using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Swift;
using System.Text;
using System.Threading.Tasks;

namespace CSC418ConsoleApp.Generators
{
    internal class UniformDistribution : RandGen<double>
    {
        private readonly Random _random;
        private readonly double _min;
        private readonly double _max;
        public UniformDistribution(double min, double max, Random? stream = null)
        {
            _random = stream is null ? new Random() : stream;
            _min = min;
            _max = max;
        }
        override public double Next()
        {
            return _min + (_random.NextDouble() * (_max - _min));
        }
        public static double SeedNext(double a, double b, Random stream)
        {
            UniformDistribution self = new(a, b, stream);
            return self.Next();
        }
    }
}
