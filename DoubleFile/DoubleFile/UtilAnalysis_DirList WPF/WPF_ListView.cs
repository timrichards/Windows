using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF
{
    class WPF_ListView
    {
        internal WPF_ListView() { Items = new WPF_LVitemCollection(this); }
        internal void Invalidate() { }

        internal WPF_LVitem TopItem = null;
        internal WPF_LVitemCollection Items = null;
    }

}
