using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    abstract class ListViewVM_GenericBase<T> : ListViewVM_Base, IEquatable<ListViewVM_GenericBase<T>>
        where T : ListViewItemVM_Base
    {
        internal delegate IEnumerable<T> EnumerableQuery();
        internal EnumerableQuery Selected = () => { DesignModeOK(); return null; };

        internal IEnumerable<T> ItemsCast { get { return _items.Cast<T>(); } }

        internal T this[string s_in]
        {
            get
            {
                if (null == s_in)
                    return null;
            
                var s = s_in.ToLower();

                foreach (var o in ItemsCast)
                {
                    if (o.SearchValue == s)
                        return o;
                }

                return null;
            }
        }

        public virtual bool Equals(ListViewVM_GenericBase<T> other)
        {
            if (null == other)
            {
                return false;
            }

            MBoxStatic.Assert(99992, (false == object.ReferenceEquals(this, other)));

            if (Items.Count != other.Items.Count)
            {
                return false;
            }

            foreach (var item in ItemsCast)
            {
                var otherItem = other[item.SearchValue];

                MBoxStatic.Assert(99991, (false == object.ReferenceEquals(item, otherItem)));

                if (false == item.Equals(otherItem))
                    return false;
            }

            return true;
        }
    }
}
