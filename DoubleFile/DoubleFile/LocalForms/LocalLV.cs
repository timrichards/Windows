using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleFile
{
    class LocalLV
    {
        internal LocalLV() { Items = new LocalLVitemCollection(this); }
        internal void Invalidate() { }

        internal LocalLVitem TopItem = null;
        internal LocalLVitemCollection Items = null;
    }

}
