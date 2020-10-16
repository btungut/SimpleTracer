using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTracer.Internal
{
    internal class MergedLookup<TKey, TValue> : ILookup<TKey, TValue>
    {
        private ILookup<TKey, TValue>[] _lookups;

        public MergedLookup(ILookup<TKey, TValue>[] lookups)
        {
            _lookups = lookups;
        }

        //TODO : performance??
        public IEnumerable<TValue> this[TKey key]
        {
            get
            {
                IEnumerable<TValue> result = _lookups[0][key];

                for (int i = 1; i < _lookups.Length; i++)
                    result = result.Concat(_lookups[i][key]);

                return result.Distinct();
            }
        }

        public int Count => _lookups.Sum(t => t.Count);

        public bool Contains(TKey key)
        {
            return _lookups.Any(t => t.Contains(key));
        }

        public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
