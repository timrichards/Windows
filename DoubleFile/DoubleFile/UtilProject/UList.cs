using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class UList<T> : 
#if (true)
 Dictionary<T, object>, IReadOnlyList<T>       // Dictionary<T> guarantees uniqueness; faster random seek; removes items fast
    {
        public void Add(T t) { base.Add(t, null); }
        public T this[int i] { get { return base.Keys.ElementAt(i); } }
        public new IEnumerator<T> GetEnumerator() { return base.Keys.GetEnumerator(); }
        public T[] ToArray() { return base.Keys.ToArray(); }
        public List<T> ToList() { return base.Keys.ToList(); }
        public bool Contains(T t) { return base.ContainsKey(t); }
    }
#else
#error Locks up removing items.
        List<T> { }                         // List<T> uses less memory; faster iterator; locks up removing items.
#endif

}
