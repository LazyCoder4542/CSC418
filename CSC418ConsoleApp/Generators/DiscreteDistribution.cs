using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSC418ConsoleApp.Generators
{
    internal class DiscreteDistribution<T> : RandGen<T>
    {
        private readonly RandGen<double> _random;
        private readonly List<double> p = [];
        private readonly List<T> v = [];
        public DiscreteDistribution(List<Tuple<T, double>> dist)
        {
            _random = RandGen.CreateUniform(0, 1);
            double cumP = 0;
            for (int i = 0; i < dist.Count; i++)
            {
                p.Add(cumP);
                v.Add(dist[i].Item1);
                cumP += dist[i].Item2;
            }
        }
        private int Search(double x)
        {
            int left = 0;
            int right = p.Count;

            while (left < right)
            {
                int mid = (left + right) / 2;
                if (p[mid] <= x)
                {
                    left = mid;
                }
                else right = mid;
            }

            return left;
        }
        override public T Next()
        {
            double x = _random.Next();
            return v[Search(x)];
        }
    }
}
