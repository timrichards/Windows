using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace DoubleFile
{
    partial class SearchListings : SearchBase
    {
        static internal Func<string, string, bool>
            GetContainsFunction(bool bRegex)
        {
            if (bRegex)
                return (a, b) => Regex.IsMatch(a, b);
            else
                return (a, b) => a.Contains(b);
        }

        class SearchListing : SearchBase
        {
            internal SearchListing(SearchBase searchBase, LVitemProject_Explorer lvItemProjectVM, Action onAbort)
                : base(searchBase)
            {
                _lvItemProjectVM = lvItemProjectVM;
                AbortAll = onAbort;
            }

            internal SearchListing DoThreadFactory()
            {
                _thread = Util.ThreadMake(() =>
                {
                    try
                    {
                        Go();
                    }
                    catch (ArgumentException e)
                    {
                        AbortAll?.Invoke();

                        Util.Assert(99863, false, "ArgumentException in SearchListing\n" +
                            e.GetBaseException().Message);
                    }
                    catch (OutOfMemoryException)
                    {
                        AbortAll?.Invoke();
                        Util.Assert(99921, false, "OutOfMemoryException in SearchListing");
                    }
                });

                return this;
            }

            internal void Join() => _thread.Join();

            internal void Abort()
            {
                if (_bThreadAbort)
                    return;

                _bThreadAbort = true;
                _thread.Abort();
            }

            void Go()
            {
                if (false == _lvItemProjectVM.CanLoad)
                    return;

                using (var sr = new StreamReader(_lvItemProjectVM.ListingFile.OpenFile(FileMode.Open)))
                {
                    var strSearch = _strSearch;
                    var strCurrentNode = "" + _strCurrentNode;

                    if (false == _bCaseSensitive)
                    {
                        strSearch = strSearch.ToLower();
                        strCurrentNode = strCurrentNode.ToLower();
                    }

                    SearchResultsDir searchResultDir = null;
                    var listResults = new SortedDictionary<SearchResultsDir, bool>();
                    var bFirst = false;
                    string strLine = null;
                    var contains = GetContainsFunction(_bRegex);

                    while (null !=
                        (strLine = sr.ReadLine()))
                    {
                        if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                            _bThreadAbort)
                        {
                            return;
                        }

                        var bDir = strLine.StartsWith(FileParse.ksLineType_Directory);
                        var bFile = strLine.StartsWith(FileParse.ksLineType_File);

                        if ((false == bDir) && (false == bFile))
                            continue;

                        var arrLine = strLine.Split('\t');
                        string strMatchDir = null;
                        string strMatchFile = null;

                        if (bDir) strMatchDir = arrLine[2].TrimEnd('\\');
                        if (bFile) strMatchFile = arrLine[3];

                        if (false == _bCaseSensitive)
                        {
                            if (bDir) strMatchDir = strMatchDir.ToLower();
                            if (bFile) strMatchFile = strMatchFile.ToLower();
                        }

                        // strMatchDir gets set to just the folder name after this, but first check the full path
                        if (bDir &&
                            (strMatchDir == strCurrentNode))
                        {
                            if (0 < listResults.Count)
                            {
                                StatusCallback(new SearchResults(_strSearch, _lvItemProjectVM, listResults.Keys), bLast: true);
                                listResults = new SortedDictionary<SearchResultsDir, bool>();
                            }

                            bFirst = true;
                        }

                        // "redoing" this logic prevents bugs during code maintenance from leaking into the result strings

                        string strDir = null;

                        if (bDir) strDir = arrLine[2].TrimEnd('\\');

                        if (bDir &&
                            (null != searchResultDir))
                        {
                            // a. SearchResults.StrDir has a \ at the end for folder & file search where folder matches, because the key would dupe for file matches.
                            // The reason the directory for a folder with files gets an extra backslash is that
                            // it naturally sorts after the item containing the directory alone.
                            searchResultDir.PathBuilder = PathBuilder.FactoryCreateOrFind(strDir + '\\', Cancel: AbortAll);
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
                            contains(strMatchDir, strSearch))
                        {
                            if (null == searchResultDir)
                                searchResultDir = new SearchResultsDir();

                            // b. SearchResults.StrDir has a \ at the end for folder & file search where folder matches, because the key would dupe for file matches.
                            // Not here. The other case a above.
                            searchResultDir.PathBuilder = PathBuilder.FactoryCreateOrFind(strDir, Cancel: AbortAll);
                            listResults.Add(searchResultDir, false);
                            searchResultDir = null;
                        }
                        else if (bFile &&
                            contains(strMatchFile, strSearch))
                        {
                            var strFile = arrLine[3];

                            if (null == searchResultDir)
                                searchResultDir = new SearchResultsDir();

                            if (false == TabledString<TabledStringType_Files>.IsAlive)
                            {
                                AbortAll?.Invoke();
                                return;
                            }

                            try
                            {
                                searchResultDir.ListFiles.Add((TabledString<TabledStringType_Files>)strFile, false);
                            }
                            catch (Exception e) when ((e is ArgumentException) || (e is NullReferenceException))
                            {
                                Util.Assert(99659, (false == TabledString<TabledStringType_Files>.IsAlive));
                                AbortAll?.Invoke();
                                return;
                            }
                        }
                    }

                    if (null != searchResultDir)
                        Util.Assert(1307.8301m, null == searchResultDir.PathBuilder);
                    else
                        Util.Assert(1307.8302m, null == searchResultDir);

                    if (0 < listResults.Count)
                        StatusCallback(new SearchResults(_strSearch, _lvItemProjectVM, listResults.Keys), bFirst: bFirst);
                }
            }

            void StatusCallback(SearchResults searchResults, bool bFirst = false, bool bLast = false)
            {
                var searchStatus = _callbackWR?.Get(w => w);

                if (null == searchStatus)
                {
                    Util.Assert(99861, false);
                    return;
                }

                Util.ThreadMake(() => searchStatus.Status(searchResults, bFirst, bLast));
            }

            LVitemProject_Explorer
                _lvItemProjectVM = null;
            Thread
                _thread = new Thread(() => { });
            Action
                AbortAll = null;
            bool
                _bThreadAbort = false;
        }
    }
}