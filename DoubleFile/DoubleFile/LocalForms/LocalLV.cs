using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleFile
{
    class LocalLV
    {
        internal LocalLVitem
            TopItem { get; set; }
        internal LocalLVitemCollection
            Items { get; set; }
        
        internal LocalLV() { Items = new LocalLVitemCollection(this); }
        internal void Invalidate() { }
    }
}
