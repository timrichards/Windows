using System.Collections.Generic;

namespace DoubleFile
{
    partial class Collate
    {
        static class InsertSizeMarkerStatic
        {
            static internal void Go(IList<LocalLVitem> listLVitems, int nIx, bool bUnique, bool bAdd = false)
            {
                Init();

                var lvItem = (LocalLVitem)lvMarker.Clone();

                lvItem.Text = ((Util.FormatSize(
                    (bUnique
                        ? listLVitems[nIx].LocalTreeNode
                        : (listLVitems[nIx].TreeNodes)[0])
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
