using System.Collections.Generic;

namespace DoubleFile
{
    class SearchResultsDir
    {
        readonly List<string> m_listFiles = new List<string>();

        internal string StrDir { get; set; }
        internal List<string> ListFiles { get { return m_listFiles; } }

        void AddFile(string strFile)
        {
            m_listFiles.Add(strFile);
        }
    }
}
