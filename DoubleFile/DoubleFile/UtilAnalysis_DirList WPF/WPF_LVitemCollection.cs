using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoubleFile;

namespace WPF
{
    class WPF_LVitemCollection : UList<WPF_LVitem>
    {
        internal WPF_LVitemCollection(WPF_ListView listView)
        {
            m_listView = listView;
        }

        internal void AddRange(string[] arrItems)
        {
            foreach (string s in arrItems)
            {
                Add(new WPF_LVitem(s, m_listView));
            }
        }

        internal void AddRange(WPF_LVitem[] arrItems)
        {
            foreach (WPF_LVitem lvItem in arrItems)
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

        internal new WPF_LVitem this[int i] { get { if (i < Count) return base[i]; return NullValue; } }

        internal WPF_LVitem this[string s]
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
                    lvItemPrevQuery = (WPF_LVitem)Keys.Where(t => t.Text == s) ?? NullValue;
                    return lvItemPrevQuery;                   // TODO: Trim? ignore case? Probably neither.
                }
            }
        }

        static WPF_LVitem NullValue = new WPF_LVitem();
        readonly WPF_ListView m_listView = null;
        string strPrevQuery = null;
        WPF_LVitem lvItemPrevQuery = null;
    }
}
