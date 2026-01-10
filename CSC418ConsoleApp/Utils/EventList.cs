using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSC418ConsoleApp.Utils
{
    internal class EventList
    {
        private readonly List<EventNode> _nodes = [];
        private readonly Dictionary<string, int> _names = [];
        private readonly PriorityQueue<(int, double), double> _pq = new();

        public int Count { get { return _pq.Count; } }

        public EventList(List<string> events)
        {
            for (int i = 0; i < events.Count; i++) {
                _nodes.Add(new EventNode(i, events[i]));
                if (!_names.ContainsKey(events[i]))
                    _names[events[i]] = i;
                else
                    throw new Exception($"Duplicate event name: {events[i]}");
            }
        }
        public EventList(int n)
        {
            for (int i = 0; i < n; i++)
            {
                _nodes.Add(new EventNode(i, $"Event{i}"));
                _names[$"Event{i}"] = i;
            }
        }
        public void Add(int i, double time)
        {
            if (i >= _nodes.Count) throw new Exception($"No Event: id({i})");
            _pq.Enqueue((i, time), time);
        }
        public void Add(string name, double time)
        {
            if (!_names.TryGetValue(name, out int value)) return;
            Add(value, time);
        }
        public (EventNode, double)? Next()
        {
            if (_pq.Count > 0) {
                (int i, double time) = _pq.Dequeue();
                return (_nodes[i], time);
            }
            return null;
        }
        public (EventNode, double)? PeekNext()
        {
            if (_pq.Count > 0)
            {
                (int i, double time) = _pq.Peek();
                return (_nodes[i], time);
            }
            return null;
        }
        public void Reset()
        {
            _pq.Clear();
        }

    }
    internal class EventNode
    {
        public readonly int _id;
        public readonly string name;
        public EventNode(int id, string name)
        {
            _id = id;
            this.name = name;
        }
    }
}
