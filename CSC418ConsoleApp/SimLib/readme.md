# SimLib - Discrete Event Simulation Library

A lightweight C# library for building discrete event simulations with built-in support for statistical collection, event scheduling, and random variate generation.

## Overview

SimLib provides a flexible framework for modeling discrete event systems. The core `DES` class manages simulation clock, event scheduling, statistical variables, and random number streams, while allowing you to define custom event handlers for your specific simulation logic.

## Features

- **Event-driven architecture** with customizable event handlers
- **Statistical collection** for both sample-based and time-weighted metrics
- **Multiple random number streams** with built-in distributions (Exponential, Uniform, Discrete)
- **Flexible list management** for queues, servers, and event scheduling
- **Type-safe configuration** using enums

## Installation

Add the SimLib classes to your C# project.

## Quick Start

Here's a complete example implementing a Single Server Queue (M/M/1):

### 1. Define Your Simulation Components
```csharp
// Define list types used in your simulation
enum ListType {
    QUEUE,
    SERVER,
    EVENTLIST
}

// Define event types
enum EventType {
    ARRIVAL,
    DEPARTURE,
    ENDSIMULATION
}

// Define sample-based statistics
enum SampstType {
    DELAY_IN_QUEUE,
    NUM_DELAYED
}

// Define time-weighted statistics
enum TimestType {
    NUM_IN_QUEUE,
    SERVER_UTILIZATION
}

// Define random number streams
enum StreamType {
    INTER_ARRIVAL,
    SERVICE
}
```

### 2. Create Event Handlers
```csharp
internal class ArrivalEventHandler : EventHandler
{
    private readonly double IAT;
    private readonly double ST;
    
    public ArrivalEventHandler(double IAT, double ST) : base((int)EventType.ARRIVAL) {
        this.IAT = IAT;
        this.ST = ST;
    }

    public override void HandleEvent(
        double clockTime,
        Action<double, int> sampst,
        Action<double, int> timest,
        Action<int, double> scheduleEvent,
        Func<int, RecordList> list,
        Func<double, int, double> expon,
        Func<double, double, int, double> uniform,
        Func<List<Tuple<double, double>>, int, double> discrete,
        Action stopSim)
    {
        // Schedule next arrival
        scheduleEvent((int)EventType.ARRIVAL, 
            clockTime + expon(IAT, (int)StreamType.INTER_ARRIVAL));
        
        var server = list((int)ListType.SERVER);
        
        if (server.Count > 0) {
            // Server busy - add to queue
            var queue = list((int)ListType.QUEUE);
            queue.Add([clockTime]);
            timest(queue.Count, (int)TimestType.NUM_IN_QUEUE);
        } else {
            // Server idle - begin service
            server.Add([clockTime]);
            double serviceTime = expon(ST, (int)StreamType.SERVICE);
            scheduleEvent((int)EventType.DEPARTURE, clockTime + serviceTime);
            sampst(1, (int)SampstType.NUM_DELAYED);
            timest(1, (int)TimestType.SERVER_UTILIZATION);
        }
    }
}
```

### 3. Set Up and Run Your Simulation
```csharp
public class SSQ
{
    private readonly DES des;
    private readonly double endTime;

    public SSQ(double interArrivalTime, double serviceTime, double endTime = 1000)
    {
        int sV = Enum.GetValues<SampstType>().Length;
        int tV = Enum.GetValues<TimestType>().Length;
        int nS = Enum.GetValues<StreamType>().Length;
        
        // Configure lists: (numAttributes, rankAttribute)
        ValueTuple<int, int>[] listConfig = [
            (1, -1),  // Queue: stores arrival time
            (1, -1),  // Server: dummy record
            (2, 0)    // Event list: ranked by time
        ];
        
        EventHandler[] handlers = [
            new ArrivalEventHandler(interArrivalTime, serviceTime),
            new DepartureEventHandler(serviceTime),
            new EndSimEventHandler()
        ];
        
        this.des = new DES(sV, tV, listConfig, handlers, 
            (int)ListType.EVENTLIST, nS);
        this.endTime = endTime;
    }

    public void StartSim()
    {
        des.Init();
        
        // Schedule initial events
        double firstArrival = des.Expon(IAT, (int)StreamType.INTER_ARRIVAL);
        des.ScheduleEvent((int)EventType.ARRIVAL, firstArrival);
        des.ScheduleEvent((int)EventType.ENDSIMULATION, endTime);
        
        // Run simulation
        des.StartSim();
        
        // Collect and display results
        double avgDelay = des.getSampSt((int)SampstType.DELAY_IN_QUEUE) 
            / des.getSampSt((int)SampstType.NUM_DELAYED);
        double avgNumInQueue = des.getTimeSt((int)TimestType.NUM_IN_QUEUE) 
            / endTime;
        double serverUtil = des.getTimeSt((int)TimestType.SERVER_UTILIZATION) 
            / endTime;
        
        Console.WriteLine($"Average Delay in Queue: {avgDelay}");
        Console.WriteLine($"Average Number in Queue: {avgNumInQueue}");
        Console.WriteLine($"Server Utilization: {serverUtil}");
    }
}

// Usage
var simulation = new SSQ(interArrivalTime: 1.0, serviceTime: 0.8, endTime: 10000);
simulation.StartSim();
```

## Core Concepts

### DES Constructor Parameters
```csharp
public DES(
    int sV,                          // Number of sample-based statistics
    int tV,                          // Number of time-weighted statistics
    ValueTuple<int, int>[] listConfig, // List configurations
    EventHandler[] handlers,         // Event handlers
    int eventList,                   // Index of event list
    int nStream                      // Number of random streams
)
```

### List Configuration

Each list is configured as a tuple `(numAttributes, rankAttribute)`:
- `numAttributes`: Number of values stored per record
- `rankAttribute`: Index of attribute to rank by (-1 for FIFO)

### Statistical Variables

**Sample-based (`SampSt`)**: For discrete observations
- Example: Total delay, number of customers served
- Retrieved as raw sum via `getSampSt(id)`

**Time-weighted (`TimeSt`)**: For continuous-time metrics
- Example: Queue length over time, server utilization
- Retrieved as time-integrated value via `getTimeSt(id)`
- Divide by simulation time for time-averaged statistics

### Random Variates
```csharp
// Exponential distribution
double value = des.Expon(mean, streamId);

// Uniform distribution
double value = des.Uniform(a, b, streamId);

// Discrete distribution
var dist = new List<Tuple<int, double>> {
    Tuple.Create(1, 0.3),
    Tuple.Create(2, 0.5),
    Tuple.Create(3, 0.2)
};
int value = des.Discrete(dist, streamId);
```

### Event Handling

Event handlers receive these parameters:
- `clockTime`: Current simulation time
- `sampst`: Update sample-based statistic
- `timest`: Update time-weighted statistic
- `scheduleEvent`: Schedule future event
- `list`: Access simulation lists
- `expon`, `uniform`, `discrete`: Random variate generators
- `stopSim`: Stop the simulation

## Best Practices

1. **Use enums** for type-safe indexing of events, lists, statistics, and streams
2. **Initialize properly**: Call `des.Init()` before each simulation run
3. **Schedule termination**: Always schedule an end simulation event
4. **Separate concerns**: Keep event logic in dedicated handler classes
5. **Calculate averages**: Divide accumulated statistics by appropriate denominators

## License

MIT License - see the [LICENSE](LICENSE) file for full details.

Copyright (c) 2026 [Your Name]