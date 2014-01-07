using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Collections;               // Hashtable
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;

namespace NS_MyData
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            listViewEx1.ListViewItemSorter = new ListViewColumnSorter();
            MyKey.imglIcons.ColorDepth = ColorDepth.Depth32Bit;
        }

        void Export(string sFullFilename)
        {
            string sNoExt = MyKey.NoExt(sFullFilename);

            TextWriter tw = new StreamWriter(new FileStream(sNoExt + ".FSS", FileMode.Create));
            MyKey.bExportRoot = true;
            MyData.dOuterNode.Export(tw, 0);
            tw.Close();

            FileStream fs = new FileStream(sNoExt + ".FSD", FileMode.Create, FileAccess.Write);
            new BinaryFormatter().Serialize(fs, MyKey.imglIcons.ImageStream);
            new BinaryFormatter().Serialize(fs, MyKey.imglIcons.Images.Keys);
            fs.Close();
        }

        private void comboBoxEx1_Validating(object sender, CancelEventArgs e)
        {
            DirectoryInfo di = null;
            try { di = new DirectoryInfo(comboBoxEx1.Text); }
            catch { return; }

            if (!di.Exists)
                return;

            MyData.GetSubdirInfo(di);

            listViewEx1.Items.Clear();
            treeView1.Nodes.Clear();
            MyData.dOuterNode.PopulateTree(treeView1.Nodes);

            Export(Application.LocalUserAppDataPath + @"\Backup.FSS");
        }

        ListViewItem[] arrLVI = null;
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            mixSelected = -1;
            listViewEx1.Items.Clear();

            arrLVI = new ListViewItem[listViewEx1.VirtualListSize = ((MyData)e.Node.Tag).marr.Count];
            ushort i = 0;
            foreach (MyData d in ((MyData)e.Node.Tag).marr)
                arrLVI[i++] = ((MyKey)d.moKey).lvi;

            listViewEx1.Items.AddRange(arrLVI);
            listViewEx1_ColumnClick(null, new ColumnClickEventArgs(1));
        }

        private void listViewEx1_ColumnClick(object sender, ColumnClickEventArgs e)     //http://support.microsoft.com/kb/319401
        {
            ListViewColumnSorter lvwColumnSorter = (ListViewColumnSorter)listViewEx1.ListViewItemSorter;
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                if (lvwColumnSorter.Order == SortOrder.Ascending) lvwColumnSorter.Order = SortOrder.Descending;
                else lvwColumnSorter.Order = SortOrder.Ascending;
            }
            else { lvwColumnSorter.SortColumn = e.Column; lvwColumnSorter.Order = SortOrder.Ascending; }
            listViewEx1.Sort();
        }

        void Import(string sFullFilename)
        {
            string sNoExt = MyKey.NoExt(sFullFilename);

            //{
            //    ushort i = 0;
            //    foreach (string s in MyKey.imglIcons.Images.Keys)
            //    {
            //        if (s.Length == 0)
            //            MyKey.imglIcons.Images.RemoveAt(i);
            //        ++i;
            //    }
            //}

            //FileStream fs = new FileStream(sNoExt + ".FSD", FileMode.Open, FileAccess.Read);
            //MyKey.imglIcons.ImageStream = (ImageListStreamer)new BinaryFormatter().Deserialize(fs);
            //MyKey.imglIcons.Images.Keys = (System.Collections.Specialized.StringCollection)new BinaryFormatter().Deserialize(fs);
            //bool[] arr2 = new bool[MyKey.imglIcons.Images.Keys.Count];
            //{
            //    for (ushort i = 0; i < MyKey.imglIcons.Images.Keys.Count; ++i)
            //        arr2[i] = s == string.Empty;
            //    for (ushort i = 0; i < arr2.Length; ++i)
            //        if (arr2[i]) = s == string.Empty;

            //}
            //MyKey.imglIcons.Images.Keys.AddRange(arr2);
            //fs.Close();

            MyData.Import(sNoExt + ".FSS");
            listViewEx1.Items.Clear();
            treeView1.Nodes.Clear();
            MyData.dOuterNode.PopulateTree(treeView1.Nodes);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Import(Application.LocalUserAppDataPath + @"\Backup.FSS");
            treeView1.ImageList = MyKey.imglIcons;
            listViewEx1.SmallImageList = MyKey.imglIcons;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            openFileDialog1.SupportMultiDottedExtensions = false;
            openFileDialog1.Filter = "FS Save documents|*.FSS";
            openFileDialog1.DefaultExt = ".fss";

            if (DialogResult.OK != openFileDialog1.ShowDialog())
                return;

            Import(openFileDialog1.FileName);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.SupportMultiDottedExtensions = false;
            saveFileDialog1.Filter = "FS Save documents|*.FSS";
            saveFileDialog1.DefaultExt = ".fss";

            if (DialogResult.OK != saveFileDialog1.ShowDialog())
                return;

            if (!saveFileDialog1.FileName.EndsWith(".fss", StringComparison.CurrentCultureIgnoreCase))
                saveFileDialog1.FileName += ".fss";

            Export(saveFileDialog1.FileName);
        }

        private void btnMD5_Click(object sender, EventArgs e)
        {
            if (listViewEx1.SelectedItems.Count == 0)
            {
                if (DialogResult.OK != MessageBox.Show("A directory selection could take awhile.", "File Browser", MessageBoxButtons.OKCancel))
                    return;


                return;
            }

        }

        short mixSelected = -1;
        private void listViewEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mixSelected >= 0)
            {
                foreach (ListViewItem.ListViewSubItem lvs in listViewEx1.Items[mixSelected].SubItems)
                { lvs.BackColor = Color.White; lvs.ForeColor = Color.Black; }
                mixSelected = -1;
            }
            if (listViewEx1.SelectedItems.Count == 0)
                return;

            mixSelected = (short)listViewEx1.SelectedItems[0].Index;
            foreach (ListViewItem.ListViewSubItem lvs in listViewEx1.Items[mixSelected].SubItems)
            { lvs.BackColor = Color.FromArgb(167, 205, 240); lvs.ForeColor = Color.White; }
        }

        private void treeView1_Click(object sender, EventArgs e)
        {
            listViewEx1.SelectedItems.Clear();
        }

        private void listViewEx1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (arrLVI == null || arrLVI.Length == 0)
                return;

            e.Item = arrLVI[e.ItemIndex];
        }
    }
}