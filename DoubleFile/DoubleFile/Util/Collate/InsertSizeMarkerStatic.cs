using System.Collections.Generic;
using System.Windows;

namespace DoubleFile
{
    partial class Collate
    {
        static class InsertSizeMarkerStatic
        {
            static internal void Go(IList<LocalLVitemVM> listLVitems, int nIx, bool bUnique, bool bAdd = false)
            {
                var lvItem = (LocalLVitemVM)_lvMarker.Clone();

                lvItem.SubItems[0] = ((Util.FormatSize(
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

            static readonly LocalLVitemVM
                _lvMarker = new LocalLVitemVM(new[] { "" })
            {
                BackColor = UtilColor.DarkSlateGray,
                ForeColor = UtilColor.White,
                FontWeight = FontWeights.Bold
            };
        }
    }
}
