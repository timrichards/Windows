using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DoubleFile
{
    class SearchFile : SearchBase
    {
        internal SearchFile(SearchBase searchBase, LVitem_ProjectVM volStrings)
            : base(searchBase)
        {
            m_volStrings = volStrings;
        }

        internal SearchFile DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
            return this;
        }

        internal void Join()
        {
            m_thread.Join();
        }

        internal void Abort()
        {
            m_bThreadAbort = true;
            m_thread.Abort();
        }

        void Go()
        {
            if (m_volStrings.CanLoad == false)
            {
                return;
            }

            using (StreamReader file = new StreamReader(m_volStrings.ListingFile))
            {
                string strLine = null;
                SearchResultsDir searchResultDir = null;
                string strSearch = m_strSearch;
                List<SearchResultsDir> listResults = new List<SearchResultsDir>();
                bool bFirst = false;
                string strCurrentNode = m_strCurrentNode ?? string.Empty;

                if (m_bCaseSensitive == false)
                {
                    strSearch = strSearch.ToLower();
                    strCurrentNode = strCurrentNode.ToLower();
                }

                while ((strLine = file.ReadLine()) != null)
                {
                    if (m_bThreadAbort || GlobalData.Instance.FormAnalysis_DirList_Closing)
                    {
                        return;
                    }

                    bool bDir = strLine.StartsWith(FileParse.ksLineType_Directory);
                    bool bFile = strLine.StartsWith(FileParse.ksLineType_File);

                    if ((bDir == false) && (bFile == false))
                    {
                        continue;
                    }

                    string[] arrLine = strLine.Split('\t');
                    string strMatchDir = null;
                    string strMatchFile = null;

                    if (bDir) { strMatchDir = arrLine[2].TrimEnd('\\'); }
                    if (bFile) { strMatchFile = arrLine[3]; }

                    if (m_bCaseSensitive == false)
                    {
                        if (bDir) { strMatchDir = strMatchDir.ToLower(); }
                        if (bFile) { strMatchFile = strMatchFile.ToLower(); }
                    }

                    // strMatchDir gets set to just the folder name after this, but first check the full path
                    if (bDir && (strMatchDir == strCurrentNode))
                    {
                        if (listResults.Count > 0)
                        {
                            listResults.Sort((x, y) => x.StrDir.CompareTo(y.StrDir));
                            m_statusCallback(new SearchResults(m_strSearch, m_volStrings, listResults), bLast: true);
                            listResults = new List<SearchResultsDir>();
                        }

                        bFirst = true;
                    }

                    // "redoing" this logic prevents bugs during code maintenance from leaking into the result strings

                    string strDir = null;

                    if (bDir) { strDir = arrLine[2].TrimEnd('\\'); }

                    if (bDir && (searchResultDir != null))
                    {
                        searchResultDir.StrDir = strDir;
                        searchResultDir.ListFiles.Sort();
                        listResults.Add(searchResultDir);
                        searchResultDir = null;
                    }

                    // ...now just the last folder name for strMatchDir...      // "outermost"
                    if (bDir && strMatchDir.Contains('\\'))
                    {
                        if (strSearch.Contains('\\') == false)
                        {
                            strMatchDir = strMatchDir.Substring(strMatchDir.LastIndexOf('\\') + 1);
                        }
                    }

                    if ((m_bSearchFilesOnly == false) && bDir && strMatchDir.Contains(strSearch))
                    {
                        if (searchResultDir == null)
                        {
                            searchResultDir = new SearchResultsDir();
                        }

                        searchResultDir.StrDir = strDir;
                        listResults.Add(searchResultDir);
                        searchResultDir = null;
                    }
                    else if (bFile && strMatchFile.Contains(strSearch))
                    {
                        string strFile = arrLine[3];

                        if (searchResultDir == null)
                        {
                            searchResultDir = new SearchResultsDir();
                        }

                        searchResultDir.ListFiles.Add(strFile);
                    }
                }

                if (searchResultDir != null)
                {
                    MBox.Assert(1307.8301, searchResultDir.StrDir == null);
                }
                else
                {
                    MBox.Assert(1307.8302, searchResultDir == null);
                }

                if (listResults.Count > 0)
                {
                    listResults.Sort((x, y) => x.StrDir.CompareTo(y.StrDir));
                    m_statusCallback(new SearchResults(m_strSearch, m_volStrings, listResults), bFirst: bFirst);
                }
            }
        }

        Thread m_thread = null;
        bool m_bThreadAbort = false;
        LVitem_ProjectVM m_volStrings = null;
    }
}
