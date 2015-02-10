using System.Collections.Generic;
using System.Windows.Forms;
using DoubleFile;
using System.Windows.Media;

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

                lvItem.Text = ((UtilAnalysis_DirList.FormatSize(
                    ((NodeDatum)((LocalTreeNode)(bUnique ? listLVitems[nIx].Tag :
                    ((UList<LocalTreeNode>)listLVitems[nIx].Tag)[0])).Tag).nTotalLength,
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
                    lvMarker.Font = new System.Drawing.Font(lvMarker.Font, System.Drawing.FontStyle.Bold);
                    lvMarker.Tag = null;
                    bInit = true;
                }
            }

            readonly static LocalLVitem lvMarker = new LocalLVitem();
            static bool bInit = false;
        }
    }
}
