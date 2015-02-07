using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DoubleFile;

namespace WPF
{
    partial class Collate
    {
        static class InsertSizeMarkerStatic
        {
            internal static void Go(List<ListViewItem> listLVitems, int nIx, bool bUnique, bool bAdd = false)
            {
                Init();

                ListViewItem lvItem = (ListViewItem)lvMarker.Clone();

                lvItem.Text = ((UtilAnalysis_DirList.FormatSize(((NodeDatum)((SDL_TreeNode)(bUnique ? listLVitems[nIx].Tag : ((UList<SDL_TreeNode>)listLVitems[nIx].Tag)[0])).Tag).nTotalLength, bNoDecimal: true)));

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

            readonly static ListViewItem lvMarker = new ListViewItem();
            static bool bInit = false;
        }
    }
}
