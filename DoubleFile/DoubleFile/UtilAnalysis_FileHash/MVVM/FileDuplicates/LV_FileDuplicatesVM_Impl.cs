using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DoubleFile
{
    partial class LV_FileDuplicatesVM
    {
        internal LV_FileDuplicatesVM(GlobalData_Base gd)
        {
            _gd = gd;
        }

        internal void TreeFileSelChanged(IReadOnlyList<FileDictionary.DuplicateStruct> lsDuplicates)
        {
            UtilProject.UIthread(Items.Clear);

            if (null == lsDuplicates)
                return;

            var lsLine = new List<string>();
            var lsDirLine = new List<string>();

            lsDuplicates
                .GroupBy(duplicate => duplicate.LVitemProjectVM)
                .AsParallel()
                .ForEach(g =>
            {
                List<int> lsLineNumbers = new List<int>();

                foreach (var duplicate in g)
                {
                    lsLineNumbers.Add(duplicate.LineNumber);
                }

                lsLineNumbers.Sort();

                int nLine = 0;
                string strDirLine = null;

                foreach (var strLine
                    in File.ReadLines(g.Key.ListingFile))
                {
                    ++nLine;

                    if (strLine.StartsWith(FileParse.ksLineType_Directory))
                    {
                        strDirLine = strLine;       // clobber
                    }
                    else if (nLine + 1 == lsLineNumbers[0])
                    {
#if (DEBUG)
                        MBoxStatic.Assert(99905, "" + nLine == strLine.Split('\t')[1]);
#endif
                        lsLineNumbers.RemoveAt(0);

                        lock (lsLine)
                            lsLine.Add(strLine);
                        
                        lock (lsDirLine)
                            lsDirLine.Add(strDirLine);
                        
                        if (0 == lsLineNumbers.Count)
                            break;
                    }
                }
            });

            MBoxStatic.Assert(99908, lsLine.Count == lsDirLine.Count);

            UtilProject.UIthread(() => 
            {
                for (var n = 0; n < lsLine.Count; ++n)
                {
                    Add(new LVitem_FileDuplicatesVM(new[] { lsLine[n].Split('\t')[3], lsDirLine[n].Split('\t')[2] }), bQuiet: true);
                }

                RaiseItems();
            });
        }

        GlobalData_Base _gd = null;
    }
}
