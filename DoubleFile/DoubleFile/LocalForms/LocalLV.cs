using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleFile
{
    class LocalLV
    {
        internal LocalLV() { _Items = new LocalLVitemCollection(this); }
        internal void Invalidate() { }

        internal LocalLVitem
            _TopItem = null;
        internal LocalLVitemCollection
            _Items = null;
    }
}
