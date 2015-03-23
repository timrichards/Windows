using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class KeyList<T> : 
#if (true)
        Dictionary<T, object>, IReadOnlyList<T>     // Dictionary<T> guarantees uniqueness; faster random seek; removes items fast
    {
        internal void Add(T key) { base.Add(key, null); }
        public T this[int i] { get { return base.Keys.ElementAt(i); } }
        public new IEnumerator<T> GetEnumerator() { return base.Keys.GetEnumerator(); }
        internal T[] ToArray() { return base.Keys.ToArray(); }
        internal List<T> ToList() { return base.Keys.ToList(); }
        internal bool Contains(T key)
        {
            object outValue;

            return TryGetValue(key, out outValue);
        }
    }
#else
#error Locks up removing items.
        List<T> { }                                 // List<T> uses less memory; faster iterator; locks up removing items.
#endif

}
