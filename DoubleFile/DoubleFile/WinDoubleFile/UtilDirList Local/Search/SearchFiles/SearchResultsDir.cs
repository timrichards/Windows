using System.Collections.Generic;

namespace DoubleFile
{
    class SearchResultsDir
    {
        internal string
            StrDir { get; set; }

        internal List<string>
            ListFiles { get { return _listFiles; } }
        readonly List<string> _listFiles = new List<string>();

        void AddFile(string strFile)
        {
            _listFiles.Add(strFile);
        }
    }
}
