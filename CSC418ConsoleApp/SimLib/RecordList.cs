using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableToImageExport.Utilities;

namespace CSC418ConsoleApp.SimLib
{
    public class RecordList
    {
        private readonly List<double[]> _data = new();
        private readonly int _attributeCount;
        private readonly int? _rankBy; // null means no sorting
        readonly string? alias;

        public RecordList(int n, string? name = null)
        {
            if (n <= 0)
                throw new ArgumentException("Attribute count must be positive.");

            _attributeCount = n;
            _rankBy = null;
            alias = name;
        }

        public RecordList(int n, int rankBy, string? name = null)
        {
            if (n <= 0)
                throw new ArgumentException("Attribute count must be positive.");

            if (rankBy < 0 || rankBy >= n)
                throw new ArgumentException("rankBy must be a valid attribute index.");

            _attributeCount = n;
            _rankBy = rankBy;
            alias = name;

        }

        public void Add(double[] record)
        {
            if (record == null || record.Length != _attributeCount)
                throw new ArgumentException($"Record must have exactly {_attributeCount} attributes.");

            if (_rankBy == null)
            {
                _data.Add(record);
            }
            else
            {
                // insert in sorted position
                int index = _data.BinarySearch(
                    record,
                    Comparer<double[]>.Create((a, b) => a[_rankBy.Value].CompareTo(b[_rankBy.Value]))
                );

                if (index < 0)
                    index = ~index;

                _data.Insert(index, record);
            }
        }

        // removeFirst()
        public double[] RemoveFirst()
        {
            if (_data.Count == 0)
                throw new InvalidOperationException("List is empty.");

            var item = _data[0];
            _data.RemoveAt(0);
            return item;
        }

        // removeLast()
        public double[] RemoveLast()
        {
            if (_data.Count == 0)
                throw new InvalidOperationException("List is empty.");

            int lastIndex = _data.Count - 1;
            var item = _data[lastIndex];
            _data.RemoveAt(lastIndex);
            return item;
        }

        public void Clear()
        {
            _data.Clear();
        }

        public override string? ToString()
        {
            string output = "";
            _data.ForEach(x =>
            {
                output += $"[{String.Join(", ", x.Select(y => y.ToString()))}]";
            });
            return output;
        }

        // Optional helper
        public int Count => _data.Count;
    }
}
