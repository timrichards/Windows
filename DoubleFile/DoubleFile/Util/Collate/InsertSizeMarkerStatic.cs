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
                var lvItem = new LocalLVitemVM(new[] { "" })
                {
                    BackColor = UtilColor.DarkSlateGray,
                    ForeColor = UtilColor.White,
                    FontWeight = FontWeights.Bold
                };

                lvItem.Folder = ((Util.FormatSize(
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
        }
    }
}
