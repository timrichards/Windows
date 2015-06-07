﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    abstract class ListViewVM_Base<T> : ListViewVM_Base //, IEquatable<ListViewVM_GenericBase<T>>
        where T : ListViewItemVM_Base
    {
        internal Func<IEnumerable<T>> Selected = () => { DesignModeOK(); return new T[] { }; };
        internal IEnumerable<T> ItemsCast { get { return Items.Cast<T>(); } }

        internal virtual IEnumerable<T> this[string s_in]
        {
            get
            {
                if (null == s_in)
                    return new T[] { };
            
                var s = s_in.ToLower();

                return ItemsCast.Where(o => o.SearchValue == s);
            }
        }

        public virtual bool LocalEquals(ListViewVM_Base<T> other)
        {
            if (null == other)
                return false;

            MBoxStatic.Assert(99992, (false == ReferenceEquals(this, other)));

            if (Items.Count != other.Items.Count)
                return false;

            var lsMatched = new List<ListViewItemVM_Base>();

            foreach (var item in ItemsCast)
            {
                var bMatch = false;

                foreach (var otherItem in other[item.SearchValue])
                {
                    MBoxStatic.Assert(99991, (false == ReferenceEquals(item, otherItem)));

                    if ((false == lsMatched.Contains(otherItem)) &&
                        item.LocalEquals(otherItem))
                    {
                        lsMatched.Add(otherItem);
                        bMatch = true;
                        break;
                    }
                }

                if (false == bMatch)
                    return false;       // assumes other logic checks for duplicates
            }

            return true;
        }
    }
}
