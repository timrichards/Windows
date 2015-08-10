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
        protected readonly bool
            _bRootText = false;
        protected readonly WeakReference<ISearchStatus>
            _callbackWR = null;

        internal enum FolderSpecialHandling { None, Outermost, Innermost };     // not used
        protected FolderSpecialHandling
            _folderHandling = FolderSpecialHandling.Outermost;                  // not used

        internal SearchBase(
            string strSearch, 
            bool bCaseSensitive,
            FolderSpecialHandling folderHandling,
            bool bSearchFilesOnly,
            string strCurrentNode,
            bool bRegex,
            bool bRootText,
            WeakReference<ISearchStatus> callbackWR)
        {
            _strSearch = strSearch;
            _bRegex = bRegex;
            _bCaseSensitive = bCaseSensitive || bRegex;     // bug prone if before bRegex assignment
            _folderHandling = folderHandling;               // not used
            _bSearchFilesOnly = bSearchFilesOnly;
            _strCurrentNode = strCurrentNode;
            _bRootText = bRootText;
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
            _bRootText = searchBase._bRootText;
            _callbackWR = searchBase._callbackWR;
        }
    }
}
