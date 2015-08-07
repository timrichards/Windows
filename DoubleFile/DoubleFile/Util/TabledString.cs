using System;
using System.Windows;

namespace DoubleFile
{
    // can't be struct because of null
    class TabledString<T> : IComparable<TabledString<T>>, IComparable
        where T : TabledStringTypesBase, new()
    {
        static TabledStringBase
            _t = null;

        static public explicit operator
            TabledString<T>(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : new TabledString<T> { nIndex = _t.Set(value) };

        static public explicit operator
            string(TabledString<T> value) => (null == value) ? null : _t?.Get(value.nIndex);

        public int CompareTo(object that) => CompareTo((TabledString<T>)that);
        public int CompareTo(TabledString<T> that) => _t?.CompareTo(nIndex, that.nIndex) ?? 0;

        static internal bool IsAlive => null != _t;

        static internal void Reinitialize()
        {
            var nRefCount = _t?.RefCount ?? 0;

            _t =
                TabledStringTypesBase.Types[new T().Type] = 
                new TabledStringGenerating { RefCount = nRefCount };
        }

        static internal void AddRef()
        {
            if (null == _t)
                Reinitialize();

            ++_t.RefCount;
        }

        static internal void DropRef()
        {
            Util.Assert(99934, 0 < _t.RefCount);
            --_t.RefCount;

            if (0 < _t.RefCount)
                return;

            _t =
                TabledStringTypesBase.Types[new T().Type] =
                null;
        }

        static internal void GenerationStarting() =>
            _t = TabledStringTypesBase.Types[new T().Type] = new TabledStringGenerating(_t);

        static internal void GenerationEnded()
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                return;

            _t =
                TabledStringTypesBase.Types[new T().Type] =
                new TabledStringGenerated(_t);
        }

        int nIndex = -1;
    }
}
