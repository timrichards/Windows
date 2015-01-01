using System;

namespace DoubleFile
{
    internal class Utilities
    {
        static internal void WriteLine(string str = null)
        {
#if (DEBUG)
            Console.WriteLine(str);
#endif
        }
    }
}
