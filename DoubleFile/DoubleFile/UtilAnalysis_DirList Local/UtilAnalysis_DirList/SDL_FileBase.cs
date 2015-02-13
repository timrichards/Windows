using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace DoubleFile
{
    // SearchDirLists listing file|*.sdl_list|SearchDirLists volume list file|*.sdl_vol|SearchDirLists copy scratchpad file|*.sdl_copy|SearchDirLists ignore list file|*.sdl_ignore
    abstract class SDL_FileBase : FileParse
    {
        internal const string BaseFilter = "Text files|*.txt|All files|*.*";
        internal const string FileAndDirListFileFilter = "SearchDirLists listing file|*." + ksFileExt_Listing;

        internal readonly string Header = null;

        readonly string m_strDescription = null;
        internal string Description { get { return m_strDescription + " list file"; } }

        readonly string m_strExt = null;
        internal string Filter { get { return "SearchDirLists " + Description + "|*." + m_strExt; } }

        internal static SaveFileDialog SFD = null;        // TODO: remove frankenSFD

        protected string m_strPrevFile = null;
        protected string m_strFileNotDialog = null;

        static bool bInited = false;

        internal static void Init()
        {
            if (bInited)
            {
                return;
            }

            SFD = new SaveFileDialog {OverwritePrompt = false};
            bInited = true;
        }

        protected SDL_FileBase(string strHeader, string strExt, string strDescription)
        {
            Init();
            MBoxStatic.Assert(1303.4306, SFD != null);
            Header = strHeader;
            m_strExt = strExt;
            m_strDescription = strDescription;
        }

        bool ShowDialog(FileDialog fd)
        {
            fd.Filter = Filter + "|" + BaseFilter;
            fd.FilterIndex = 0;
            fd.FileName = Path.GetFileNameWithoutExtension(m_strPrevFile);
            fd.InitialDirectory = Path.GetDirectoryName(m_strPrevFile);

            if (fd.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            m_strFileNotDialog = m_strPrevFile = fd.FileName;
            return true;
        }

        protected virtual void ReadListItem(ListView lv,
            IEnumerable<string> strArray)
        {
            lv.Items.Add(new ListViewItem(strArray.ToArray()));
        }

        internal bool ReadList(ListView lv_in)
        {
            var nCols = lv_in.Columns.Count;
            var lv = new ListView();       // fake

            if ((null == m_strFileNotDialog) &&
                (false == ShowDialog(new OpenFileDialog())))
            {
                return false;
            }

            if (null == m_strFileNotDialog)
            {
                return false;
            }

            if (false == Keyboard.IsKeyDown(Key.LeftShift))
            {
                lv_in
                    .Items
                    .Clear();
            }

            using (var sr = File.OpenText(m_strFileNotDialog))
            {
                var strLine = sr.ReadLine();

                if (strLine == Header)
                {
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        ReadListItem(lv,
                            strLine
                            .TrimEnd(new char[] { '\t' })
                            .Split('\t')
                            .Take(nCols));
                    }
                }
            }

            if (false == lv.Items.IsEmpty())
            {
                var lvItems = lv
                    .Items
                    .Cast<ListViewItem>();

                lv.Items.Clear();
                lv_in.Items.AddRange(lvItems.ToArray());
                lv_in.Invalidate();
                return true;
            }
            else
            {
                MBoxStatic.ShowDialog("Not a valid " + Description + ".", "Load " + Description);
                return false;
            }
        }

        protected virtual string WriteListItem(int nIndex, string str) { return str; }

        internal bool WriteList(ListView.ListViewItemCollection lvItems)
        {
            if (ShowDialog(SFD) == false)
            {
                return false;
            }

            if ((File.Exists(m_strPrevFile))
                && (MBoxStatic.ShowDialog(m_strPrevFile + " already exists. Overwrite?",
                    Description,
                    MessageBoxButton.YesNo)
                != MessageBoxResult.Yes))
            {
                return false;
            }

            using (var sw = File.CreateText(m_strPrevFile))
            {
                sw.WriteLine(Header);

                foreach (ListViewItem lvItem in lvItems)
                {
                    sw.Write(WriteListItem(0, lvItem.SubItems[0].Text));

                    var nIx = 1;

                    foreach (var lvSubitem in lvItem.SubItems.Cast<ListViewItem.ListViewSubItem>()
                        .Skip(1))
                    {
                        sw.Write('\t' + WriteListItem(nIx, lvSubitem.Text));
                        ++nIx;
                    }

                    sw.WriteLine();
                }
            }

            return true;
        }
    }
}
