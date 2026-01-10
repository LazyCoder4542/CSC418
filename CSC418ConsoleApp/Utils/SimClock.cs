using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSC418ConsoleApp.Utils
{
    internal class SimClock
    {
        internal double CurrentTime { get; private set; }
        internal SimClock()
        {
            CurrentTime = 0;
        }
        internal void Advance(double time)
        {
            CurrentTime = time;
        }
        internal void Reset()
        {
            CurrentTime = 0;
        }
    }
}
