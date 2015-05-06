using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    abstract class ListViewVM_Base<T> : ListViewVM_Base //, IEquatable<ListViewVM_GenericBase<T>>
        where T : ListViewItemVM_Base
    {
        internal delegate IEnumerable<T> EnumerableQuery();
        internal EnumerableQuery Selected = () => { DesignModeOK(); return null; };

        internal IEnumerable<T> ItemsCast { get { return Items.Cast<T>(); } }

        internal virtual T this[string s_in]
        {
            get
            {
                if (null == s_in)
                    return null;
            
                var s = s_in.ToLower();

                return ItemsCast.FirstOrDefault(o => o.SearchValue == s);
            }
        }

        public virtual bool LocalEquals(ListViewVM_Base<T> other)
        {
            if (null == other)
            {
                return false;
            }

            MBoxStatic.Assert(99992, (false == ReferenceEquals(this, other)));

            if (Items.Count != other.Items.Count)
            {
                return false;
            }

            foreach (var item in ItemsCast)
            {
                var otherItem = other[item.SearchValue];

                MBoxStatic.Assert(99991, (false == ReferenceEquals(item, otherItem)));

                if (false == item.LocalEquals(otherItem))
                    return false;
            }

            return true;
        }
    }
}
