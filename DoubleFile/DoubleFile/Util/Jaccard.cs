using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    public class Jaccard
    {
        internal static double
            Calc(HashSet<int> hs1, HashSet<int> hs2) => (hs1.Intersect(hs2).Count() / (double)hs1.Union(hs2).Count());
        internal static double
            Calc(List<int> ls1, List<int> ls2) => Calc(new HashSet<int>(ls1), new HashSet<int>(ls2));
    }
}
