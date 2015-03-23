using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DoubleFile
{
    partial class SearchType2 : SearchBase
    {
        class SearchFile : SearchBase
        {
            internal SearchFile(SearchBase searchBase, LVitem_ProjectVM volStrings)
                : base(searchBase)
            {
                _volStrings = volStrings;
            }

            internal SearchFile DoThreadFactory()
            {
                _thread = new Thread(Go);
                _thread.IsBackground = true;
                _thread.Start();
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
                    var listResults = new KeyListSorted<SearchResultsDir>();
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
                            if (false == listResults.IsEmpty())
                            {
                                _statusCallback(new SearchResults(_strSearch, _volStrings, listResults), bLast: true);
                                listResults = new KeyListSorted<SearchResultsDir>();
                            }

                            bFirst = true;
                        }

                        // "redoing" this logic prevents bugs during code maintenance from leaking into the result strings

                        string strDir = null;

                        if (bDir) { strDir = arrLine[2].TrimEnd('\\'); }

                        if (bDir &&
                            (null != searchResultDir))
                        {
                            searchResultDir.StrDir = strDir;

                            listResults.Add(searchResultDir);

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

                            searchResultDir.StrDir = strDir;

                            listResults.Add(searchResultDir);

                            searchResultDir = null;
                        }
                        else if (bFile &&
                            strMatchFile.Contains(strSearch))
                        {
                            string strFile = arrLine[3];

                            if (null == searchResultDir)
                                searchResultDir = new SearchResultsDir();

                            searchResultDir.ListFiles.Add(strFile);
                        }
                    }

                    if (null != searchResultDir)
                        MBoxStatic.Assert(1307.8301, null == searchResultDir.StrDir);
                    else
                        MBoxStatic.Assert(1307.8302, null == searchResultDir);

                    if (false == listResults.IsEmpty())
                        _statusCallback(new SearchResults(_strSearch, _volStrings, listResults), bFirst: bFirst);
                }
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