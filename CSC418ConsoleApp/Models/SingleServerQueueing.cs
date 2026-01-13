using CSC418ConsoleApp.Generators;
using CSC418ConsoleApp.Utils;
using CsvHelper;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                stats.Push(q_t / time, totalDelay / numberDelayed, b_t / time, serverStatus ? numberDelayed - 1 : numberDelayed);
            
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
        public readonly List<SingleServerReportEntry> ssre = [];
        internal void Push(double avgQ, double avgD, double b, int n)
        {
            var newEntry = new SingleServerReportEntry(avgD, avgQ, b, n);
            ssre.Add(newEntry);
        }
        internal void Reset()
        {
            ssre.Clear();
        }
        internal void Report() {
            Console.WriteLine($"Delay: {ssre.Select(r => r.AvgDelay).Average()}, Queue Length: {ssre.Select(r => r.AvgQueueLength).Average()}, Server Utilization: {ssre.Select(r => r.ServerUtilization).Average()}, Number of Customers Served: {ssre.Select(r => r.NumberServed).Average()}");
            //Utils.Plot.HistoPDF([.. ssre.Select(r => r.AvgDelay)], "Average delay in queue");

            
            Multiplot multiplot = new();

            // configure the multiplot to have 4 subplots
            multiplot.AddPlots(4);

            // add sample data to each subplot
            ScottPlot.Plot plot = multiplot.GetPlot(0);
            Utils.Plot.HistoPDF(plot, [.. ssre.Select(r => r.AvgDelay)], "Average delay in queue");

            ScottPlot.Plot plot2 = multiplot.GetPlot(1);
            Utils.Plot.HistoPDF(plot2, [.. ssre.Select(r => r.AvgQueueLength)], "Average queue length", clr: Colors.C1);

            ScottPlot.Plot plot3 = multiplot.GetPlot(2);
            Utils.Plot.HistoPDF(plot3, [.. ssre.Select(r => r.ServerUtilization)], "Average server utilization", clr: Colors.C2);

            ScottPlot.Plot plot4 = multiplot.GetPlot(3);
            Utils.Plot.HistoPDF(plot4, [.. ssre.Select(r => r.NumberServed)], "Average number of customers served", clr: Colors.C3);

            // configure the multiplot to use a grid layout
            multiplot.Layout = new ScottPlot.MultiplotLayouts.Grid(rows: 2, columns: 2);

            multiplot.SavePng(Path.Combine(Globals._targetFolder, "demo.png"), 900, 675);
            multiplot.SavePng(Path.Combine(Globals._targetFolder, "demo.svg"), 900, 675);

            SaveCSV();

        }
        internal void SaveCSV()
        {
            using (var writer = new StreamWriter(Path.Combine(Globals._targetFolder, "output_ssq.csv")))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(ssre);
            }
        }
    }
    public class SingleServerReportEntry(double avgD, double avgQ, double su, int ns) {
        public double ServerUtilization { get; set; } = su;
        public double AvgDelay { get; set; } = avgD;
        public double AvgQueueLength { get; set; } = avgQ;
        public int NumberServed { get; set; } = ns;

    }

}
