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
            foreach (var s in lsItems)
            {
                Add(new LocalLVitem(s, m_listView));
            }
        }

        internal void AddRange(IReadOnlyList<LocalLVitem> lsItems)
        {
            foreach (var lvItem in lsItems)
            {
                lvItem.ListView = m_listView;
                Add(lvItem);
            }
        }

        internal bool ContainsKey(string s)
        {
            if (s != m_strPrevQuery)
            {
                m_strPrevQuery = s;
                m_lvItemPrevQuery = this[s];
            }

            return (m_lvItemPrevQuery != null);
        }

        internal new LocalLVitem this[int i] { get { return (i < Count) ? base[i] : NullValue; } }

        internal LocalLVitem this[string s]
        {
            get
            {
                if (s == m_strPrevQuery)
                {
                    return m_lvItemPrevQuery;
                }
                else
                {
                    m_strPrevQuery = s;
                    m_lvItemPrevQuery = NullValue;
                    Keys
                        .Where(t => t.Text == s)
                        .FirstOnlyAssert(lvItem => m_lvItemPrevQuery = lvItem);
                    return m_lvItemPrevQuery;                   // TODO: Trim? ignore case? Probably neither.
                }
            }
        }

        static readonly LocalLVitem NullValue = new LocalLVitem();

        readonly LocalLV m_listView = null;
        string m_strPrevQuery = null;
        LocalLVitem m_lvItemPrevQuery = null;
    }
}
