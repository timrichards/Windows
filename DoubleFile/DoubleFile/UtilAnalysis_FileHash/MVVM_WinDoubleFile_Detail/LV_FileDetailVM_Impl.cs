using System;
using System.Linq;

namespace DoubleFile
{
    partial class LV_FileDetailVM
    {
        internal LV_FileDetailVM(GlobalData_Base gd)
        {
            _gd = gd;
        }

        internal void Update(string strFileLine = null)
        {
            UtilProject.UIthread(() =>
            {
                Title = null;
                Items.Clear();
            });

            if (string.IsNullOrWhiteSpace(strFileLine))
                return;

            string[] kasHeader = new[] { "Filename", "Created", "Modified", "Attributes", "Length", "Error 1", "Error 2" };
            var asFile = strFileLine.Split('\t')
                .Skip(3)                            // makes this an LV line: knColLengthLV
                .ToArray();

            asFile[3] = UtilAnalysis_DirList.DecodeAttributes(asFile[3]);

            if ((asFile.Length > FileParse.knColLengthLV) &&
                (false == string.IsNullOrWhiteSpace(asFile[FileParse.knColLengthLV])))
            {
                asFile[FileParse.knColLengthLV] =
                    UtilAnalysis_DirList.FormatSize(asFile[FileParse.knColLengthLV], bBytes: true);
            }

            UtilProject.UIthread(() =>
            {
                for (int i = 1; i < Math.Min(asFile.Length, kasHeader.Length); ++i)
                {
                    if (string.IsNullOrWhiteSpace(asFile[i]))
                        continue;

                    Add(new LVitem_FileDetailVM(new[] { kasHeader[i], asFile[i] }), bQuiet: true);
                }

                Title = asFile[0];
                RaiseItems();
            });
        }

        GlobalData_Base _gd = null;
    }
}
