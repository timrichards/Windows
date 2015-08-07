using System;
using System.Windows;

namespace DoubleFile
{
    // can't be struct because of null
    class TabledString<T> : IComparable<TabledString<T>>, IComparable
        where T : TypedArrayBase, new()
    {
        static TabledStringStatics
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
                TypedArrayBase.tA[new T().Type] = 
                new T_Generating { RefCount = nRefCount };
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
                TypedArrayBase.tA[new T().Type] =
                null;
        }

        static internal void GenerationStarting() =>
            _t = TypedArrayBase.tA[new T().Type] = new T_Generating(_t);

        static internal void GenerationEnded()
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                return;

            _t =
                TypedArrayBase.tA[new T().Type] =
                new T_Generated(_t);
        }

        int nIndex = -1;
    }
}
