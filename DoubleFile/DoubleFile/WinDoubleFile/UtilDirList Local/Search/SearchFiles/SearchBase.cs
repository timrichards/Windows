
namespace DoubleFile
{
    class SearchBase
    {
        protected string 
            _strSearch = null;
        protected bool 
            _bCaseSensitive = true;
        protected bool
            _bSearchFilesOnly = false;
        protected string
            _strCurrentNode = null;
        protected static SearchStatusDelegate
            _statusCallback = null;

        internal enum FolderSpecialHandling { None, Outermost, Innermost };                     // not used
        protected FolderSpecialHandling m_folderHandling = FolderSpecialHandling.Outermost;     // not used

        protected SearchBase(SearchStatusDelegate statusCallback)
        {
            _statusCallback = statusCallback;
        }

        protected SearchBase(SearchBase searchBase)
        {
            _strSearch = searchBase._strSearch;
            _bCaseSensitive = searchBase._bCaseSensitive;
            _bSearchFilesOnly = searchBase._bSearchFilesOnly;
            _strCurrentNode = searchBase._strCurrentNode;
            m_folderHandling = searchBase.m_folderHandling;
            MBoxStatic.Assert(1307.8315, _statusCallback != null);
        }
    }
}
