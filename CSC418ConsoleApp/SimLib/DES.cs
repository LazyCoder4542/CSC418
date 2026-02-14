using CSC418ConsoleApp.Generators;
using CSC418ConsoleApp.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSC418ConsoleApp.SimLib
{
    /// <summary>
    /// Discrete Event Simulation (DES) engine that manages simulation clock, events, statistics, and random number generation.
    /// </summary>
    public class DES
    {
        private readonly SimClock clock;
        private readonly List<RecordList> lists;
        private readonly List<EventHandler> eventHandlers;
        private readonly List<SampVariable> sampVariables;
        private readonly List<TimeVariable> timeVariables;
        private readonly List<Random> randomStreams;
        private readonly int _eventListId;

        /// <summary>
        /// Initializes a new DES instance with specified configuration.
        /// </summary>
        /// <param name="sV">Number of sample-based statistical variables</param>
        /// <param name="tV">Number of time-weighted statistical variables</param>
        /// <param name="listConfig">Array of list configurations as (id, capacity) tuples</param>
        /// <param name="handlers">Array of event handlers for processing events</param>
        /// <param name="eventList">ID of the list to use as the event queue</param>
        /// <param name="nStream">Number of independent random number streams</param>
        public DES(int sV,
                   int tV,
                   ValueTuple<int, int>[] listConfig,
                   EventHandler[] handlers,
                   int eventList,
                   int nStream
                   )
        {
            // event list
            _eventListId = eventList;
            //clock
            clock = new SimClock();
            // initialize lists
            lists = [.. listConfig.Select(x =>
            {
                if (x.Item2 >= 0)
                    return new RecordList(x.Item1, x.Item2);
                else return new RecordList(x.Item1);
            })];
            // event handlers
            eventHandlers = [..handlers];
            // variables
            sampVariables = [];
            timeVariables = [];
            for (int i = 0; i < sV; i++) {
                sampVariables.Add(new SampVariable(i));
            }
            for (int i = 0; i < tV; i++)
            {
                timeVariables.Add(new TimeVariable(i));
            }
            // streams
            randomStreams = [];
            for (int i = 0; i < nStream; i++) {
                randomStreams.Add(new Random());
            }
            // Initialize DES components here
        }

        /// <summary>
        /// Resets all simulation components to initial state.
        /// </summary>
        public void Init()
        {
            sampVariables.ForEach(x => x.Reset());
            timeVariables.ForEach(x => x.Reset());
            lists.ForEach(x => x.Clear());
        }

        /// <summary>
        /// Begins the discrete event simulation execution.
        /// </summary>
        public void StartSim()
        {
            var eventList = lists[_eventListId];
            var queue = lists[0];
            var server = lists[1];
            while (eventList.Count > 0)
            {
                //Console.WriteLine("\nHanding an event...");
                //Console.WriteLine($"Event list: {eventList}");
                //Console.WriteLine($"Queue: {queue}");
                //Console.WriteLine($"Server: {server}");
                TimingRoutine();
            }
        }

        /// <summary>
        /// Stops the simulation by clearing the event queue.
        /// </summary>
        public void StopSim()
        {
            UpdateAllTimeStat();
            var eventList = lists[_eventListId];
            eventList.Clear();
        }

        /// <summary>
        /// Retrieves and processes the next scheduled event, advancing the simulation clock.
        /// </summary>
        public void TimingRoutine() // process next event and advance clock
        {
            var eventList = lists[_eventListId];
            if (eventList.Count > 0)
            {
                var nextEvent = eventList.RemoveFirst();
                int nextEventTime = (int) nextEvent[0];
                int nextEventType = (int) nextEvent[1];

                clock.Advance(nextEvent[0]);

                CallHandleEvent(nextEventType);

            }
        }

        /// <summary>
        /// Invokes the appropriate event handler for the specified event type.
        /// </summary>
        /// <param name="eventId">ID of the event to handle</param>
        public void CallHandleEvent(int eventId)
        {
            var handler = eventHandlers[eventId];
            handler.HandleEvent(clock.CurrentTime, SampSt, TimeSt, ScheduleEvent, GetList, Expon, Uniform, Discrete<double>, StopSim);
        }

        /// <summary>
        /// Schedules a future event at the specified time.
        /// </summary>
        /// <param name="eventId">ID of the event to schedule</param>
        /// <param name="time">Simulation time when the event should occur</param>
        public void ScheduleEvent(int eventId, double time)
        {
            var eventList = lists[_eventListId];
            eventList.Add([time, eventId]);
        }

        /// <summary>
        /// Generates an exponentially distributed random variable.
        /// </summary>
        /// <param name="mean">Mean of the exponential distribution</param>
        /// <param name="streamId">ID of the random stream to use</param>
        /// <returns>Exponentially distributed random value</returns>
        public double Expon(double mean, int streamId)
        {
            var stream = randomStreams[streamId];

            return ExponentialDistribution.SeedNext(mean, stream);
        }

        /// <summary>
        /// Generates a discrete random variable from a probability distribution.
        /// </summary>
        /// <typeparam name="T">Type of the discrete values</typeparam>
        /// <param name="dist">List of (value, probability) tuples defining the distribution</param>
        /// <param name="streamId">ID of the random stream to use</param>
        /// <returns>Randomly selected value from the distribution</returns>
        public T Discrete<T>(List<Tuple<T, double>> dist, int streamId)
        {
            var stream = randomStreams[streamId];
            return DiscreteDistribution<T>.StreamNext(dist, stream);
        }

        /// <summary>
        /// Generates a uniformly distributed random variable.
        /// </summary>
        /// <param name="a">Lower bound of the uniform distribution</param>
        /// <param name="b">Upper bound of the uniform distribution</param>
        /// <param name="streamId">ID of the random stream to use</param>
        /// <returns>Uniformly distributed random value in [a, b]</returns>
        public double Uniform(double a, double b, int streamId)
        {
            var stream = randomStreams[streamId];

            return UniformDistribution.SeedNext(a, b, stream);
        }

        /// <summary>
        /// Retrieves a record list by its ID.
        /// </summary>
        /// <param name="id">ID of the list to retrieve</param>
        /// <returns>The requested RecordList</returns>
        public RecordList GetList(int id)
        {
            return lists[id];
        }

        /// <summary>
        /// Updates a sample-based statistical variable.
        /// </summary>
        /// <param name="value">Value to add to the statistic</param>
        /// <param name="id">ID of the sample variable</param>
        public void SampSt(double value, int id)
        {
            var samp_var = sampVariables[id];
            samp_var.AddValue(value);
        }

        /// <summary>
        /// Updates a time-weighted statistical variable.
        /// </summary>
        /// <param name="value">Value to record for the statistic</param>
        /// <param name="id">ID of the time variable</param>
        public void TimeSt(double value, int id)
        {
            var time_var = timeVariables[id];
            time_var.UpdateValue(value, clock.CurrentTime);
        }

        /// <summary>
        /// Retrieves the current value of a sample-based statistic.
        /// </summary>
        /// <param name="id">ID of the sample variable</param>
        /// <returns>Current accumulated value</returns>
        public double getSampSt(int id)
        {
            var samp_var = sampVariables[id];
            return samp_var.GetValue();
        }

        /// <summary>
        /// Retrieves the current value of a time-weighted statistic.
        /// </summary>
        /// <param name="id">ID of the time variable</param>
        /// <returns>Current time-weighted accumulated value</returns>
        public double getTimeSt(int id)
        {
            var time_var = timeVariables[id];
            return time_var.GetValue();
        }

        /// <summary>
        /// Updates all time-weighted statistics to reflect the final simulation state.
        /// </summary>
        private void UpdateAllTimeStat()
        {
            timeVariables.ForEach(x => x.UpdateValue(0, clock.CurrentTime));
        }

        /// <summary>
        /// Represents a sample-based statistical accumulator for discrete observations.
        /// </summary>
        private class SampVariable
        {
            private readonly int _id;
            private double value = 0;
            public SampVariable(int _id)
            {
                this._id = _id;
            }
            public void AddValue(double v)
            {
                value += v;
            }
            public double GetValue()
            {
                return value;
            }
            public void Reset()
            {
                value = 0;
            }
        }

        /// <summary>
        /// Represents a time-weighted statistical accumulator for continuous-time observations.
        /// </summary>
        private class TimeVariable
        {
            private readonly int _id;
            private double value = 0;
            private double currValue = 0;
            private double lastTime = 0;

            public TimeVariable(int _id)
            {
                this._id = _id;
            }
            public void AddValue(double v, double currTime) {
                value += currValue * (currTime - lastTime);
                currValue += v;
                lastTime = currTime;
            }
            public void UpdateValue(double v, double currTime)
            {
                v = v - currValue;
                AddValue(v, currTime);
            }
            public void IncValue(double currTime)
            {
                AddValue(1, currTime);
            }

            public void DecValue(double currTime)
            {
                AddValue(-1, currTime);
            }
            public double GetValue()
            {
                return value;
            }
            public void Reset() {
                value = 0;
                currValue = 0;
                lastTime = 0;
            }
        }
    }

}
