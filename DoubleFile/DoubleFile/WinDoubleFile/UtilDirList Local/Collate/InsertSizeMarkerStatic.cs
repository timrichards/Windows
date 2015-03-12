using System.Collections.Generic;
using DoubleFile;

namespace Local
{
    partial class Collate
    {
        static class InsertSizeMarkerStatic
        {
            internal static void Go(IList<LocalLVitem> listLVitems, int nIx, bool bUnique, bool bAdd = false)
            {
                Init();

                var lvItem = (LocalLVitem)lvMarker.Clone();

                lvItem.Text = ((UtilDirList.FormatSize(
                    (bUnique
                        ? listLVitems[nIx].LocalTreeNode
                        : ((LocalTreeNode)(listLVitems[nIx].TreeNodes)[0]))
                    .NodeDatum
                    .TotalLength,
                    bNoDecimal: true)));

                if (bAdd)
                {
                    listLVitems.Add(lvItem);
                }
                else
                {
                    listLVitems.Insert(nIx, lvItem);
                }
            }

            static void Init()
            {
                if (bInit == false)
                {
                    lvMarker.BackColor = UtilColor.DarkSlateGray;
                    lvMarker.ForeColor = UtilColor.White;
                    lvMarker.FontWeight = System.Windows.FontWeights.Bold;
                    bInit = true;
                }
            }

            readonly static LocalLVitem lvMarker = new LocalLVitem();
            static bool bInit = false;
        }
    }
}
