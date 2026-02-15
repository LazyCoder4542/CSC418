using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSC418ConsoleApp.SimLib
{
    public abstract class EventHandler
    {
        readonly int eventId;
        public EventHandler(int eventId)
        {
            this.eventId = eventId;
        }

        abstract public void HandleEvent(double[] param,
                                         double clockTime,
                                         Action<double, int> sampst,
                                         Action<double, int> timest,
                                         Action<int, double> scheduleEvent,
                                         Func<int, RecordList> list,
                                         Func<double, int, double> expon,
                                         Func<double, double, int, double> uniform,
                                         Func<List<Tuple<double, double>>, int, double> discrete,
                                         Action stopSim);
    }
}
