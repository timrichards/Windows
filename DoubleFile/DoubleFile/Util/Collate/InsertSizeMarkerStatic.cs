using System.Collections.Generic;
using System.Windows;

namespace DoubleFile
{
    partial class Collate
    {
        static class InsertSizeMarkerStatic
        {
            static internal void Go(IList<LVitem_ClonesVM> listLVitems, int nIx, bool bSolitary, bool bAdd = false)
            {
                var lvItem = new LVitem_ClonesVM(new[] {
                    listLVitems[nIx].WithLocalTreeNode(localTreeNode => ((Util.FormatSize(
                    localTreeNode
                    .NodeDatum
                    .TotalLength,
                    bNoDecimal: true)))) });

                if (bAdd)
                    listLVitems.Add(lvItem);
                else
                    listLVitems.Insert(nIx, lvItem);
            }
        }
    }
}
