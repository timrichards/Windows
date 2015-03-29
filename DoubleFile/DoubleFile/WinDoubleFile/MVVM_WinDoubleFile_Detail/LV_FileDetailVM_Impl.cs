using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class LV_FileDetailVM : IDisposable
    {
        internal LV_FileDetailVM()
        {
            WinDoubleFile_DuplicatesVM.UpdateFileDetail += Update;
        }

        public void Dispose()
        {
            WinDoubleFile_DuplicatesVM.UpdateFileDetail -= Update;
        }

        internal void Update(IEnumerable<string> ieFileLine = null)
        {
            UtilProject.UIthread(() =>
            {
                Title = null;
                Items.Clear();
            });

            if (null == ieFileLine)
                return;

            var kasHeader = new[] { "Filename", "Created", "Modified", "Attributes", "Length", "Error 1", "Error 2" };

            var asFileLine =
                ieFileLine
                .ToArray();

            asFileLine[3] = UtilDirList.DecodeAttributes(asFileLine[3]);

            if ((asFileLine.Length > FileParse.knColLengthLV) &&
                (false == string.IsNullOrWhiteSpace(asFileLine[FileParse.knColLengthLV])))
            {
                asFileLine[FileParse.knColLengthLV] =
                    UtilDirList.FormatSize(asFileLine[FileParse.knColLengthLV], bBytes: true);
            }

            UtilProject.UIthread(() =>
            {
                for (var i = 1; i < Math.Min(asFileLine.Length, kasHeader.Length); ++i)
                {
                    if (string.IsNullOrWhiteSpace(asFileLine[i]))
                        continue;

                    Add(new LVitem_FileDetailVM(new[] { kasHeader[i], asFileLine[i] }), bQuiet: true);
                }

                Title = asFileLine[0];
                RaiseItems();
            });
        }
    }
}
