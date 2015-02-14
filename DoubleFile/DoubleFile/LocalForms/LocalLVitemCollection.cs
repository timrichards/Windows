using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleFile
{
    class LocalLVitemCollection : UList<LocalLVitem>
    {
        internal LocalLVitemCollection(LocalLV listView)
        {
            m_listView = listView;
        }

        internal void AddRange(IReadOnlyList<string> lsItems)
        {
            foreach (string s in lsItems)
            {
                Add(new LocalLVitem(s, m_listView));
            }
        }

        internal void AddRange(IReadOnlyList<LocalLVitem> lsItems)
        {
            foreach (LocalLVitem lvItem in lsItems)
            {
                lvItem.ListView = m_listView;
                Add(lvItem);
            }
        }

        internal bool ContainsKey(string s)
        {
            if (s != strPrevQuery)
            {
                strPrevQuery = s;
                lvItemPrevQuery = this[s];
            }

            return (lvItemPrevQuery != null);
        }

        internal new LocalLVitem this[int i] { get { return (i < Count) ? base[i] : NullValue; } }

        internal LocalLVitem this[string s]
        {
            get
            {
                if (s == strPrevQuery)
                {
                    return lvItemPrevQuery;
                }
                else
                {
                    strPrevQuery = s;
                    lvItemPrevQuery = (LocalLVitem)Keys.Where(t => t.Text == s) ?? NullValue;
                    return lvItemPrevQuery;                   // TODO: Trim? ignore case? Probably neither.
                }
            }
        }

        static readonly LocalLVitem NullValue = new LocalLVitem();
        readonly LocalLV m_listView = null;
        string strPrevQuery = null;
        LocalLVitem lvItemPrevQuery = null;
    }
}
