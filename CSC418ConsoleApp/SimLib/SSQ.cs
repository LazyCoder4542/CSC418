using CSC418ConsoleApp.Generators;
using CSC418ConsoleApp.Utils;
using ScottPlot.Statistics;
using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace CSC418ConsoleApp.SimLib
{
    enum ListType {
        QUEUE,
        SERVER,
        EVENTLIST
    }
    enum EventType
    {
        ARRIVAL,
        DEPARTURE,
        ENDSIMULATION
    }
    enum SampstType
    {
        DELAY_IN_QUEUE,
        NUM_DELAYED
    }
    enum TimestType
    {
        NUM_IN_QUEUE,
        SERVER_UTILIZATION,
    }
    enum StreamType
    {
        INTER_ARRIVAL,
        SERVICE
    }

    public class SSQ
    {
        // number of statistics
        int sV = Enum.GetValues<SampstType>().Length;
        int tV = Enum.GetValues<TimestType>().Length;
        int nS = Enum.GetValues<StreamType>().Length;
        // List configs
        // (number of attributes, rankAttribute(-1 for no rank))
        readonly (int, int) queueConf = (1, -1);         // arrival time
        readonly (int, int) serverConf = (1, -1);       // dummmy record for server
        readonly (int, int) eventListConf = (2, 0);     // time, eventType

        private readonly DES des;

        // vars
        readonly double IAT;
        readonly double ST;
        readonly double endTime;

        public SSQ(double interArrivalTime, double serviceTime, double endTime = 1000)
        {
            this.IAT = interArrivalTime;
            this.ST = serviceTime;

            ValueTuple<int, int>[] listConfig = [queueConf, serverConf, eventListConf];
            EventHandler[] handlers =
            {
                new ArrivalEventHandler(IAT, ST),
                new DepartureEventHandler(ST),
                new EndSimEventHandler()
            };
            this.des = new(sV, tV, listConfig, handlers, (int) ListType.EVENTLIST, nS);

            this.endTime = endTime;

        }
        public void StartSim(int n = 100)
        {
            des.Init();
            // schdule first arrival
            double arrivalTime = des.Expon(IAT, (int)StreamType.INTER_ARRIVAL);
            Console.WriteLine(arrivalTime);
            des.ScheduleEvent((int)EventType.ARRIVAL, arrivalTime);
            // schedule end sim
            des.ScheduleEvent((int)EventType.ENDSIMULATION, endTime);

            Console.WriteLine("\nStarting simulation");
            des.StartSim();
            // do smth


            // end of simulation
            Console.WriteLine("\nSimulation Result");
            double avgDelay = des.getSampSt((int)SampstType.DELAY_IN_QUEUE) / des.getSampSt((int)SampstType.NUM_DELAYED);
            double avgNumInQueue = des.getTimeSt((int)TimestType.NUM_IN_QUEUE) / endTime;
            double serverUtil = des.getTimeSt((int)TimestType.SERVER_UTILIZATION) / endTime;
            Console.WriteLine($"Average Delay in Queue: {avgDelay}");
            Console.WriteLine($"Average Number in Queue: {avgNumInQueue}");
            Console.WriteLine($"Server utilization: {serverUtil}");
        }
    }
    internal class ArrivalEventHandler: EventHandler
    {
        private readonly double IAT;
        private readonly double ST;
        public ArrivalEventHandler(double IAT, double ST): base((int)EventType.ARRIVAL) {
            this.IAT = IAT;
            this.ST = ST;
        }

        public override void HandleEvent(double clockTime,
                                         Action<double, int> sampst,
                                         Action<double, int> timest,
                                         Action<int, double> scheduleEvent,
                                         Func<int, RecordList> list,
                                         Func<double, int, double> expon,
                                         Func<double, double, int, double> uniform,
                                         Func<List<Tuple<double, double>>, int, double> discrete,
                                         Action stopSim)
        {
            // schedule next arrival event
            scheduleEvent((int)EventType.ARRIVAL, clockTime + expon(IAT, (int)StreamType.INTER_ARRIVAL));
            // check server
            var server = list((int)ListType.SERVER);
            if (server.Count > 0)
            {
                // server busy (add to queue)
                var queue = list((int) ListType.QUEUE);
                queue.Add([clockTime]);
                timest(queue.Count, (int)TimestType.NUM_IN_QUEUE);
            }
            else
            {
                // server is idle
                // add to server and schedule departure event
                // inc number_delayed
                server.Add([clockTime]);
                double serviceTime = expon(ST, (int)StreamType.SERVICE);
                scheduleEvent((int)EventType.DEPARTURE, clockTime + serviceTime);
                sampst(1, (int)SampstType.NUM_DELAYED);
                timest(1, (int)TimestType.SERVER_UTILIZATION);
            }
        }
    }
    internal class DepartureEventHandler : EventHandler
    {
        private readonly double ST;
        public DepartureEventHandler(double ST) : base((int)EventType.DEPARTURE) {
            this.ST = ST;
        }

        public override void HandleEvent(double clockTime,
                                         Action<double, int> sampst,
                                         Action<double, int> timest,
                                         Action<int, double> scheduleEvent,
                                         Func<int, RecordList> list,
                                         Func<double, int, double> expon,
                                         Func<double, double, int, double> uniform,
                                         Func<List<Tuple<double, double>>, int, double> discrete,
                                         Action stopSim)
        {
            var queue = list((int)ListType.QUEUE);
            var server = list((int)ListType.SERVER);

            server.RemoveFirst();

            if (queue.Count > 0) {
                double arrivalTime = queue.RemoveFirst()[0];
                double delay = clockTime - arrivalTime;
                sampst(delay,(int) SampstType.DELAY_IN_QUEUE);
                server.Add([clockTime]);
                double serviceTime = expon(ST, (int)StreamType.SERVICE);
                scheduleEvent((int)EventType.DEPARTURE, clockTime + serviceTime);
                sampst(1, (int)SampstType.NUM_DELAYED);
                timest(queue.Count, (int)TimestType.NUM_IN_QUEUE);
            }

            timest(server.Count > 0 ? 1 : 0, (int)TimestType.SERVER_UTILIZATION);
        }
    }
    internal class EndSimEventHandler : EventHandler
    {
        private readonly double ST;
        public EndSimEventHandler() : base((int)EventType.ENDSIMULATION) {}

        public override void HandleEvent(double clockTime,
                                         Action<double, int> sampst,
                                         Action<double, int> timest,
                                         Action<int, double> scheduleEvent,
                                         Func<int, RecordList> list,
                                         Func<double, int, double> expon,
                                         Func<double, double, int, double> uniform,
                                         Func<List<Tuple<double, double>>, int, double> discrete,
                                         Action stopSim)
        {
            stopSim();
        }
    }

}
