using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSC418ConsoleApp.Generators
{
    internal abstract class RandGen<T>
    {
        abstract public T Next();
    }

    internal static class RandGen
    {
        static public RandGen<double> CreateUniform(double a, double b)
        {
            return new UniformDistribution(a, b);
        }
        static public RandGen<double> CreateExponential(double mean)
        {
            return new ExponentialDistribution(mean);
        }
        static public RandGen<T> CreateDiscreteDist<T>(List<Tuple<T, double>> dist)
        {
            return new DiscreteDistribution<T>(dist);
        }
    }
}
