using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC418ConsoleApp.Generators;
using CSC418ConsoleApp.Utils;

namespace CSC418ConsoleApp.Models
{
    internal class SingleServerQueueing
    {
        private readonly double eS; // expected service time
        private readonly double eA; // expected inter-arrival time
        private readonly RandGen sRand; // service time generator
        private readonly RandGen aRand; // inter arrival time generator
        private readonly SimClock clk;
        private readonly EventList eventList;

        // Stats counters;
        private readonly Queue<double> _queue; // contains the arrival time of customers on the queue
        private int numberDelayed;
        private double totalDelay;
        private double q_t;
        private double b_t;
        private double lastEvent;
        private bool serverStatus;

        // others;
        //private double numberServed;

        // Stats Report
        internal readonly SingleServerQueueingStats stats;

        internal SingleServerQueueing(double interArrivalTime, double serviceTime)
        {
            eS = serviceTime;
            eA = interArrivalTime;
            sRand = RandGen.CreateExponential(serviceTime);
            aRand = RandGen.CreateExponential(interArrivalTime);
            eventList = new(["ARRIVAL", "DEPARTURE", "END_SIMULATION"]);
            clk = new();

            _queue = new();
            stats = new();
        }
        private void Init()
        {
            eventList.Reset();
            clk.Reset();
            _queue.Clear();
            numberDelayed = 0;
            totalDelay = 0;
            q_t = 0;
            b_t = 0;
            lastEvent = 0;
            serverStatus = false;
        }
        internal void startSim(double time, int rep = 1) // [Stopping condition] time = endtime
        {
            for (int i = 0; i < rep; i++)
            {

                Init();

                // Schedule first arrival
                ScheduleEvent("ARRIVAL", aRand);

                // Schedule End Simulation
                ScheduleEvent("END_SIMULATION", time);

                // Begin Event Cycle
                TimingRoutine();

                stats.Push(q_t / time, totalDelay / numberDelayed, b_t / time);
            
            }
            stats.Report();
        }
        internal void Reset()
        {
            stats.Reset();
        }
        private void ScheduleEvent(string name, RandGen gen)
        {
            ScheduleEvent(name, gen.Next() + clk.CurrentTime);
        }
        private void ScheduleEvent(string name, double time)
        {
            eventList.Add(name, time);
        }
        private void TimingRoutine()
        {
            while (true)
            {
                var next = eventList.Next();
                if (next == null) break;
                (var ev, double time) = next.Value;

                clk.Advance(time);

                // update q_t and b_t;
                double t_diff = time - lastEvent;
                q_t += _queue.Count * t_diff;
                if (serverStatus) b_t += t_diff;

                if (ev.name == "END_SIMULATION") break;

                switch (ev.name)
                {
                    case "ARRIVAL":
                        ArrivalRoutine();
                        break;
                    case "DEPARTURE":
                        DepartureRoutine();
                        break;

                }

                lastEvent = time;
            }
        }
        // Event Routines : Schedules new events, update states and statistical counters
        private void ArrivalRoutine()
        {
            ScheduleEvent("ARRIVAL", aRand);

            if (serverStatus)
            {
                _queue.Enqueue(clk.CurrentTime);
            }
            else
            {
                numberDelayed++;
                serverStatus = true;
                ScheduleEvent("DEPARTURE", sRand);
            }
        }
        private void DepartureRoutine()
        {
            if (_queue.Count > 0)
            {
                double delay = clk.CurrentTime - _queue.Dequeue();
                totalDelay += delay;
                numberDelayed++;

                ScheduleEvent("DEPARTURE", sRand);
            }
            else
            {
                serverStatus = false;
            }
        }

    }
    internal class SingleServerQueueingStats
    {
        readonly List<double> serverUtilization = [];
        readonly List<double> avgDelay = [];
        readonly List<double> avgQueueLength = [];
        internal void Push(double avgQ, double avgD, double b)
        {
            serverUtilization.Add(b);
            avgDelay.Add(avgD);
            avgQueueLength.Add(avgQ);
        }
        internal void Reset()
        {
            serverUtilization.Clear();
            avgDelay.Clear();
            avgQueueLength.Clear();
        }
        internal void Report() {
            Console.WriteLine($"Delay: {avgDelay.Average()}, Queue Length: {avgQueueLength.Average()}, Server Utilization: {serverUtilization.Average()}");
            Plot.HistoPDF(avgDelay.ToArray(), "Average delay in queue");
        }
    }
    public class SingleServerReportEntry {
        public int Id { get; set; }
        public double ServerUtilization { get; set; }
        public double AvgDelay { get; set; }
        public double AvgQueueLength { get; set; }
        public int NumberServed { get; set; }

    }

}
