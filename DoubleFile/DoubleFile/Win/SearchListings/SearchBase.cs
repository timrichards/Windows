using System;

namespace DoubleFile
{
    class SearchBase
    {
        protected readonly string 
            _strSearch = null;
        protected readonly bool 
            _bCaseSensitive = true;
        protected readonly bool
            _bSearchFilesOnly = false;
        protected readonly string
            _strCurrentNode = null;
        protected readonly bool
            _bRegex = false;
        protected readonly WeakReference<ISearchStatus>
            _callbackWR = null;

        internal enum FolderSpecialHandling { None, Outermost, Innermost };                     // not used
        protected FolderSpecialHandling _folderHandling = FolderSpecialHandling.Outermost;      // not used

        protected SearchBase(
            string strSearch, 
            bool bCaseSensitive,
            SearchBase.FolderSpecialHandling folderHandling,
            bool bSearchFilesOnly,
            string strCurrentNode,
            bool bRegex,
            WeakReference<ISearchStatus> callbackWR)
        {
            _strSearch = strSearch;
            _bCaseSensitive = bCaseSensitive || _bRegex;
            _folderHandling = folderHandling;          // not used
            _bSearchFilesOnly = bSearchFilesOnly;
            _bRegex = bRegex;
            _strCurrentNode = strCurrentNode;
            _callbackWR = callbackWR;
        }

        protected SearchBase(SearchBase searchBase)
        {
            _strSearch = searchBase._strSearch;
            _bCaseSensitive = searchBase._bCaseSensitive;
            _bSearchFilesOnly = searchBase._bSearchFilesOnly;
            _strCurrentNode = searchBase._strCurrentNode;
            _bRegex = searchBase._bRegex;
            _folderHandling = searchBase._folderHandling;
            _callbackWR = searchBase._callbackWR;
        }
    }
}
