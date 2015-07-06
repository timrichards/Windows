using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DoubleFile
{
    partial class SearchListings : SearchBase
    {
        class SearchListing : SearchBase
        {
            internal SearchListing(SearchBase searchBase, LVitem_ProjectVM volStrings)
                : base(searchBase)
            {
                _volStrings = volStrings;
            }

            internal SearchListing DoThreadFactory()
            {
                _thread = Util.ThreadMake(Go);
                return this;
            }

            internal void Join()
            {
                _thread.Join();
            }

            internal void Abort()
            {
                _bThreadAbort = true;
                _thread.Abort();
            }

            void Go()
            {
                if (false == _volStrings.CanLoad)
                    return;

                using (var sr = new StreamReader(_volStrings.ListingFile))
                {
                    var strSearch = _strSearch;
                    var strCurrentNode = _strCurrentNode ?? string.Empty;

                    if (false == _bCaseSensitive)
                    {
                        strSearch = strSearch.ToLower();
                        strCurrentNode = strCurrentNode.ToLower();
                    }

                    SearchResultsDir searchResultDir = null;
                    var listResults = new SortedDictionary<SearchResultsDir, bool>();
                    var bFirst = false;
                    string strLine = null;

                    while (null != (strLine = sr.ReadLine()))
                    {
                        if (App.LocalExit || _bThreadAbort)
                            return;

                        var bDir = strLine.StartsWith(FileParse.ksLineType_Directory);
                        var bFile = strLine.StartsWith(FileParse.ksLineType_File);

                        if ((false == bDir) && (false == bFile))
                            continue;

                        var arrLine = strLine.Split('\t');
                        string strMatchDir = null;
                        string strMatchFile = null;

                        if (bDir) { strMatchDir = arrLine[2].TrimEnd('\\'); }
                        if (bFile) { strMatchFile = arrLine[3]; }

                        if (false == _bCaseSensitive)
                        {
                            if (bDir) { strMatchDir = strMatchDir.ToLower(); }
                            if (bFile) { strMatchFile = strMatchFile.ToLower(); }
                        }

                        // strMatchDir gets set to just the folder name after this, but first check the full path
                        if (bDir &&
                            (strMatchDir == strCurrentNode))
                        {
                            if (listResults.LocalAny())
                            {
                                StatusCallback(new SearchResults(_strSearch, _volStrings, listResults.Keys), bLast: true);
                                listResults = new SortedDictionary<SearchResultsDir, bool>();
                            }

                            bFirst = true;
                        }

                        // "redoing" this logic prevents bugs during code maintenance from leaking into the result strings

                        string strDir = null;

                        if (bDir) { strDir = arrLine[2].TrimEnd('\\'); }

                        if (bDir &&
                            (null != searchResultDir))
                        {
                            // SearchResults.StrDir has a \ at the end for folder & file search where folder matches, because the key would dupe for file matches.
                            // Not here. The other case below.
                            searchResultDir.StrDir = PathBuilder.FactoryCreateOrFind(strDir, Cancel: Abort);
                            listResults.Add(searchResultDir, false);
                            searchResultDir = null;
                        }

                        // ...now just the last folder name for strMatchDir...      // "outermost"
                        if (bDir &&
                            strMatchDir.Contains('\\'))
                        {
                            if (false == strSearch.Contains('\\'))
                                strMatchDir = strMatchDir.Substring(strMatchDir.LastIndexOf('\\') + 1);
                        }

                        if ((false == _bSearchFilesOnly) &&
                            bDir &&
                            strMatchDir.Contains(strSearch))
                        {
                            if (null == searchResultDir)
                                searchResultDir = new SearchResultsDir();

                            // SearchResults.StrDir has a \ at the end for folder & file search where folder matches, because the key would dupe for file matches.
                            searchResultDir.StrDir = PathBuilder.FactoryCreateOrFind(strDir + '\\', Cancel: Abort);
                            listResults.Add(searchResultDir, false);
                            searchResultDir = null;
                        }
                        else if (bFile &&
                            strMatchFile.Contains(strSearch))
                        {
                            var strFile = arrLine[3];

                            if (null == searchResultDir)
                                searchResultDir = new SearchResultsDir();

                            searchResultDir.ListFiles.Add(strFile, false);
                        }
                    }

                    if (null != searchResultDir)
                        MBoxStatic.Assert(1307.8301m, null == searchResultDir.StrDir);
                    else
                        MBoxStatic.Assert(1307.8302m, null == searchResultDir);

                    if (listResults.LocalAny())
                        StatusCallback(new SearchResults(_strSearch, _volStrings, listResults.Keys), bFirst: bFirst);
                }
            }

            void StatusCallback(SearchResults searchResults, bool bFirst = false, bool bLast = false)
            {
                if (null == _callbackWR)
                {
                    MBoxStatic.Assert(99862, false);
                    return;
                }

                ISearchStatus searchStatus = null;

                _callbackWR.TryGetTarget(out searchStatus);

                if (null == searchStatus)
                {
                    MBoxStatic.Assert(99861, false);
                    return;
                }

                searchStatus.Status(searchResults, bFirst, bLast);
            }

            LVitem_ProjectVM
                _volStrings = null;
            Thread
                _thread = null;
            bool
                _bThreadAbort = false;
        }
    }
}