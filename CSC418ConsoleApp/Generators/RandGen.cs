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
        static public RandGen<double> CreateUniform(double a, double b, Random? stream = null)
        {
            return new UniformDistribution(a, b, stream);
        }
        static public RandGen<double> CreateExponential(double mean, Random? stream = null)
        {
            return new ExponentialDistribution(mean, stream);
        }
        static public RandGen<T> CreateDiscreteDist<T>(List<Tuple<T, double>> dist, Random? stream = null)
        {
            return new DiscreteDistribution<T>(dist, stream);
        }
    }
}
