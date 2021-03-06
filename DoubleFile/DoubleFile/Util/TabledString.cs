﻿using System;
using System.Windows;

namespace DoubleFile
{
    // can't be struct because of null
    class TabledString<T> : IComparable<TabledString<T>>, IComparable
        where T : TabledStringTypesBase, new()
    {
        public override string
            ToString() => _t.Get(nIndex);

        static public explicit operator
            TabledString<T>(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : new TabledString<T> { nIndex = _t.Set(value) };

        public int CompareTo(object that) => CompareTo((TabledString<T>)that);
        public int CompareTo(TabledString<T> that) => _t?.CompareTo(nIndex, that.nIndex) ?? 0;

        static internal bool
            IsAlive => null != _t;
        static internal bool
            IsGenerating => _t is TabledStringGenerating;

        static internal void
            AddRef()
        {
            if (null == _t)
                _t = TabledStringTypesBase.Types[new T().Type] = new TabledStringGenerating();

            ++_nRefCount;
        }

        static internal void
            DropRef()
        {
            Util.Assert(99934, 0 < _nRefCount);

            if (0 < --_nRefCount)
                return;

            _t =
                TabledStringTypesBase.Types[new T().Type] =
                null;
        }

        static internal void
            GenerationStarting() => _t = TabledStringTypesBase.Types[new T().Type] = new TabledStringGenerating(_t);

        static internal void
            GenerationEnded()
        {
            if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                return;

            var t = _t.As<TabledStringGenerating>();

            if (null == t)
            {
                Util.Assert(99915, false);
                return;
            }

            if (t.IndexGenerator != t.DictSortedStrings.Count)
            {
                Util.Assert(99922, false);
                _t = TabledStringTypesBase.Types[new T().Type] = new TabledStringGenerating();
                return;
            }

            _t =
                TabledStringTypesBase.Types[new T().Type] =
                new TabledStringGenerated(t);
        }

        static TabledStringBase
            _t = null;
        static int
            _nRefCount = 0;

        int nIndex = -1;
    }
}
