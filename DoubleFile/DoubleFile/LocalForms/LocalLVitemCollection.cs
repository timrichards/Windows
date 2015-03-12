using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class LocalLVitemCollection : KeyList<LocalLVitem>
    {
        internal LocalLVitemCollection(LocalLV listView)
        {
            _listView = listView;
        }

        internal void AddRange(IReadOnlyList<string> lsItems)
        {
            foreach (var s in lsItems)
            {
                Add(new LocalLVitem(s, _listView));
            }
        }

        internal void AddRange(IReadOnlyList<LocalLVitem> lsItems)
        {
            foreach (var lvItem in lsItems)
            {
                lvItem.ListView = _listView;
                Add(lvItem);
            }
        }

        internal bool ContainsKeyA(string s)
        {
            if (s != _strPrevQuery)
            {
                _strPrevQuery = s;
                _lvItemPrevQuery = this[s];
            }

            return (_lvItemPrevQuery != null);
        }

        internal new LocalLVitem this[int i] { get { return (i < Count) ? base[i] : null; } }

        internal LocalLVitem this[string s]
        {
            get
            {
                if (s == _strPrevQuery)
                {
                    return _lvItemPrevQuery;
                }
                else
                {
                    _strPrevQuery = s;
                    _lvItemPrevQuery = null;
                    Keys
                        .Where(t => t.Text == s)
                        .FirstOnlyAssert(lvItem => _lvItemPrevQuery = lvItem);
                    return _lvItemPrevQuery;                   // TODO: Trim? ignore case? Probably neither.
                }
            }
        }

        readonly LocalLV
            _listView = null;
        string
            _strPrevQuery = null;
        LocalLVitem
            _lvItemPrevQuery = null;
    }
}
