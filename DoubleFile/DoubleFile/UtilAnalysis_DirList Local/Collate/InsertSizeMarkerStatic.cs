using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DoubleFile;

namespace Local
{
    partial class Collate
    {
        static class InsertSizeMarkerStatic
        {
            internal static void Go(List<LocalLVitem> listLVitems, int nIx, bool bUnique, bool bAdd = false)
            {
                Init();

                LocalLVitem lvItem = (LocalLVitem)lvMarker.Clone();

                lvItem.Text = ((UtilAnalysis_DirList.FormatSize(((NodeDatum)((LocalTreeNode)(bUnique ? listLVitems[nIx].Tag : ((UList<LocalTreeNode>)listLVitems[nIx].Tag)[0])).Tag).nTotalLength, bNoDecimal: true)));

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
                    lvMarker.BackColor = Color.DarkSlateGray;
                    lvMarker.ForeColor = Color.White;
                    lvMarker.Font = new Font(lvMarker.Font, FontStyle.Bold);
                    lvMarker.Tag = null;
                    bInit = true;
                }
            }

            readonly static LocalLVitem lvMarker = new LocalLVitem();
            static bool bInit = false;
        }
    }
}
