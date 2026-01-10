using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSC418ConsoleApp.Generators
{
    internal abstract class RandGen
    {
        abstract public double Next();

        static public RandGen CreateUniform(double a, double b)
        {
            return new UniformDistribution(a, b);
        }
        static public RandGen CreateExponential(double mean)
        {
            return new ExponentialDistribution(mean);
        }
    }
}
