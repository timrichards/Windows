using System;

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
        protected WeakReference<ISearchStatus>
            _callbackWR = null;

        internal enum FolderSpecialHandling { None, Outermost, Innermost };                     // not used
        protected FolderSpecialHandling _folderHandling = FolderSpecialHandling.Outermost;      // not used

        protected SearchBase(WeakReference<ISearchStatus> callbackWR)
        {
            _callbackWR = callbackWR;
        }

        protected SearchBase(SearchBase searchBase)
        {
            _strSearch = searchBase._strSearch;
            _bCaseSensitive = searchBase._bCaseSensitive;
            _bSearchFilesOnly = searchBase._bSearchFilesOnly;
            _strCurrentNode = searchBase._strCurrentNode;
            _folderHandling = searchBase._folderHandling;
            _callbackWR = searchBase._callbackWR;
        }
    }
}
