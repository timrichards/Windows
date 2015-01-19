using System;

namespace DoubleFile
{
    internal partial class Utilities
    {
        static internal void WriteLine(string str = null)
        {
#if (DEBUG)
            Console.WriteLine(str);
#endif
        }
    }
}
