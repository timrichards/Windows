using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DoubleFile
{
    partial class SearchType2 : SearchBase
    {
        internal SearchType2(
            LV_ProjectVM lvProjectVM,
            string strSearch, 
            bool bCaseSensitive,
            SearchBase.FolderSpecialHandling folderHandling,
            bool bSearchFilesOnly,
            string strCurrentNode,
            SearchStatusDelegate statusCallback,
            Action doneCallback)
            : base(statusCallback)
        {
            _lvProjectVM = lvProjectVM;
            _strSearch = strSearch;
            _bCaseSensitive = bCaseSensitive;
            _folderHandling = folderHandling;          // not used
            _bSearchFilesOnly = bSearchFilesOnly;
            _strCurrentNode = strCurrentNode;
            _doneCallback = doneCallback;
        }

        void Go()
        {
            if (null == _lvProjectVM)
                return;

            UtilProject.WriteLine("Searching for '" + _strSearch + "'");

            DateTime dtStart = DateTime.Now;

            foreach (var volStrings in _lvProjectVM.ItemsCast)
            {
                SearchFile searchFile = new SearchFile(this, volStrings);

                _cbagWorkers.Add(searchFile.DoThreadFactory());
            }

            foreach (SearchFile worker in _cbagWorkers)
                worker.Join();

            UtilProject.WriteLine(string.Format("Completed Search for {0} in {1} seconds.", _strSearch, ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));

            if (App.LocalExit || _bThreadAbort)
                return;

            _doneCallback();
        }

        internal void EndThread()
        {
            foreach (SearchFile worker in _cbagWorkers)
            {
                worker.Abort();
            }

            _bThreadAbort = true;
            _thread = null;
        }

        internal SearchType2 DoThreadFactory()
        {
            _thread = new Thread(Go);
            _thread.IsBackground = true;
            _thread.Start();
            return this;
        }

        internal bool
            IsAborted { get { return _bThreadAbort; } }

        readonly Action
            _doneCallback = null;
        readonly LV_ProjectVM
            _lvProjectVM = null;

        ConcurrentBag<SearchFile>
            _cbagWorkers = new ConcurrentBag<SearchFile>();
        Thread
            _thread = null;
        bool
            _bThreadAbort = false;
    }
}
