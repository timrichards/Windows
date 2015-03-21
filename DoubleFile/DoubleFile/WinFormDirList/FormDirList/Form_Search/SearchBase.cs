
namespace DoubleFile
{
    class SearchBase
    {
        internal enum FolderSpecialHandling { None, Outermost, Innermost };                     // not used
        protected FolderSpecialHandling m_folderHandling = FolderSpecialHandling.Outermost;     // not used

        protected SearchBase(SearchStatusDelegate statusCallback)
        {
            m_statusCallback = statusCallback;
        }

        protected SearchBase(SearchBase searchBase)
        {
            m_strSearch = searchBase.m_strSearch;
            m_bCaseSensitive = searchBase.m_bCaseSensitive;
            m_bSearchFilesOnly = searchBase.m_bSearchFilesOnly;
            m_strCurrentNode = searchBase.m_strCurrentNode;
            m_folderHandling = searchBase.m_folderHandling;
            MBoxStatic.Assert(1307.8315, m_statusCallback != null);
        }

        protected string m_strSearch = null;
        protected bool m_bCaseSensitive = true;
        protected bool m_bSearchFilesOnly = false;
        protected string m_strCurrentNode = null;
        protected static SearchStatusDelegate m_statusCallback = null;
    }
}
