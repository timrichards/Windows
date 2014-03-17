using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace SearchDirLists
{
    public partial class Form1 : Form
    {
        private String m_strVolumeName = "";
        private String m_strPath = "";
        private String m_strSaveAs = "";
        private String m_strSearch = "";

        private List<String> m_list_Errors = new List<string>();

        private bool m_bBrowseLoaded = false;

        Hashtable m_hashCache = new Hashtable();

        public Form1()
        {
            InitializeComponent();
            Console.WindowWidth = Console.LargestWindowWidth;
        }

#region Selected Index Changed

        private bool FormatPath(Control ctl, ref String strPath, bool bFailOnDirectory = true)
        {
            if (Directory.Exists(Path.GetFullPath(strPath)))
            {
                String strCapDrive = strPath.Substring(0, strPath.IndexOf(":" + Path.DirectorySeparatorChar) + 2);

                strPath = Path.GetFullPath(strPath).Replace(strCapDrive, strCapDrive.ToUpper());

                if (strPath != strCapDrive.ToUpper())
                {
                    strPath = strPath.TrimEnd(Path.DirectorySeparatorChar);
                }

                ctl.Text = strPath;
            }
            else if (bFailOnDirectory)
            {
                form_tabControl.TabIndex = 0;
                FormError(ctl, "Path does not exist.                    ", "Save Fields");
                return false;
            }

            return true;
        }

        private bool SaveFields(bool bFailOnDirectory = true)
        {
            m_strVolumeName = form_cb_VolumeName.Text.Trim();
            m_strPath = form_cb_Path.Text.Trim();
            m_strSearch = form_cb_Search.Text;

            if (m_strPath.Length > 0)
            {
                m_strPath += Path.DirectorySeparatorChar;

                if (FormatPath(form_cb_Path, ref m_strPath, bFailOnDirectory) == false)
                {
                    return false;
                }
            }

            if (form_cb_SaveAs.Text.Length > 0)
            {
                form_cb_SaveAs.Text = m_strSaveAs = Path.GetFullPath(form_cb_SaveAs.Text.Trim());

                if (FormatPath(form_cb_SaveAs, ref m_strSaveAs, bFailOnDirectory) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private void ComboBoxItemsInsert(ComboBox comboBox, String strText = "")
        {
            if (strText.Length <= 0)
            {
                strText = comboBox.Text;
            }

            strText = strText.Trim();

            if (strText.Length <= 0)
            {
                return;
            }

            if (comboBox.Items.Contains(strText))
            {
                return;
            }

            comboBox.Items.Insert(0, strText);
        }

        private void form_cb_VolumeName_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cb_VolumeName, m_strPath);
            m_strPath = form_cb_VolumeName.Text;
        }

        private void cb_Path_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cb_Path, m_strPath);
            m_strPath = form_cb_Path.Text;
        }

        private void cb_SaveAs_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cb_SaveAs, m_strSaveAs);
            m_strSaveAs = form_cb_SaveAs.Text;
        }

        private void cb_Search_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItemsInsert(form_cb_Search, m_strSearch);
            m_strSearch = form_cb_Search.Text;
        }

#endregion //Selected Index Changed

        private void form_btn_Path_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            ComboBoxItemsInsert(form_cb_Path);
            m_strPath = form_cb_Path.Text = folderBrowserDialog1.SelectedPath;
        }

        private void form_btn_SaveAs_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            ComboBoxItemsInsert(form_cb_SaveAs);
            m_strSaveAs = form_cb_SaveAs.Text = saveFileDialog1.FileName;

            if (File.Exists(m_strSaveAs))
            {
                form_cb_VolumeName.Text = "";
                form_cb_Path.Text = "";
            }

            m_bBrowseLoaded = false;
        }

        private bool ReadHeader()
        {
            using (StreamReader file = new StreamReader(m_strSaveAs))
            {
                do
                {
                    String line = file.ReadLine();

                    if (line != Utilities.m_str_HEADER) break;

                    line = file.ReadLine();

                    if (line == null) break;

                    form_cb_VolumeName.Text = line;
                    line = file.ReadLine();

                    if (line == null) break;

                    form_cb_Path.Text = line;
                    return SaveFields(false);
                }
                while (false);
            }

            return false;
        }

        void FormError(Control control, String strError, String strTitle)
        {
            control.BackColor = Color.Red;
            timer_killRed.Enabled = true;
            MessageBox.Show(strError, strTitle);
        }

        private void form_btn_AddVolume_Click(object sender, EventArgs e)
        {
            if (SaveFields(false) == false)
            {
                return;
            }

            form_cb_VolumeName.BackColor = Color.Empty;
            form_cb_Path.BackColor = Color.Empty;
            form_cb_SaveAs.BackColor = Color.Empty;

            if (m_strSaveAs.Length <= 0)
            {
                FormError(form_cb_SaveAs, "Must have a file to load or save directory listing to.", "Volume Save As");
                return;
            }

            if (form_lv_Volumes.FindItemWithText(m_strSaveAs) != null)
            {
                FormError(form_cb_SaveAs, "File already in use in list of volumes.            ", "Volume Save As");
                return;
            }

            if (File.Exists(m_strSaveAs) && (m_strPath.Length > 0))
            {
                if (MessageBox.Show(m_strSaveAs + " already exists. Overwrite?                 ", "Volume Save As", MessageBoxButtons.YesNo)
                    != System.Windows.Forms.DialogResult.Yes)
                {
                    form_cb_SaveAs.BackColor = Color.Red;
                    timer_killRed.Enabled = true;
                    return;
                }
            }

            if ((File.Exists(m_strSaveAs) == false) && form_lv_Volumes.Items.ContainsKey(m_strPath))
            {
                FormError(form_cb_Path, "Path already added.                                   ", "Volume Source Path");
                return;
            }

            if ((m_strVolumeName.Length > 0) && form_lv_Volumes.FindItemWithText(m_strVolumeName) != null)
            {
                form_cb_VolumeName.BackColor = Color.Red;

                if (MessageBox.Show("Nickname already in use. Use it for more than one volume?", "Volume Save As", MessageBoxButtons.YesNo)
                    != DialogResult.Yes)
                {
                    return;
                }
                else
                {
                    form_cb_VolumeName.BackColor = Color.Empty;
                }
            }

            if ((File.Exists(m_strSaveAs) == false) && (m_strPath.Length <= 0))
            {
                form_cb_Path.BackColor = Color.Red;
                MessageBox.Show("Must have a path or existing directory listing file.  ", "Volume Source Path");
                return;
            }

            if ((m_strPath.Length > 0) && (Directory.Exists(m_strPath) == false))
            {
                form_cb_Path.BackColor = Color.Red;
                MessageBox.Show("Path does not exist.                                  ", "Volume Source Path");
                return;
            }

            String strStatus = "Not Saved";

            if (File.Exists(m_strSaveAs))
            {
                if (m_strPath.Length <= 0)
                {
                    bool bFileOK = ReadHeader();

                    if (bFileOK)
                    {
                        strStatus = Utilities.m_str_USING_FILE;
                    }
                    else
                    {
                        if (m_strPath.Length > 0)
                        {
                            strStatus = "File is bad. Will overwrite.";
                        }
                        else
                        {
                            form_cb_Path.BackColor = Color.Red;
                            MessageBox.Show("File is bad and path does not exist.           ", "Volume Source Path");
                            return;
                        }
                    }
                }
                else
                {
                    strStatus = "Will overwrite.";
                }
            }

            if (m_strVolumeName.Length == 0)
            {
                form_cb_VolumeName.BackColor = Color.Red;

                if (MessageBox.Show("Would you like to enter a nickname for this volume?", "Volume Save As", MessageBoxButtons.YesNo)
                    != DialogResult.No)
                {
                    return;
                }
                else
                {
                    form_cb_VolumeName.BackColor = Color.Empty;
                }
            }

            ListViewItem lvItem = new ListViewItem(new string[] { m_strVolumeName, m_strPath, m_strSaveAs, strStatus, "Yes" });

            lvItem.Name = m_strPath;
            form_lv_Volumes.Items.Add(lvItem);
            form_btn_SavePathInfo.Enabled = true;
            m_bBrowseLoaded = false;
        }

        private void form_btn_RemoveVolume_Click(object sender, EventArgs e)
        {
            ListView.SelectedIndexCollection lvSelect = form_lv_Volumes.SelectedIndices;

            if (lvSelect.Count <= 0)
            {
                return;
            }

            form_lv_Volumes.Items[lvSelect[0]].Remove();
            UpdateLV_VolumesSelection();
            form_btn_SavePathInfo.Enabled = (form_lv_Volumes.Items.Count > 0);
            m_bBrowseLoaded = false;
        }

        private void SetLV_VolumesItemInclude(ListViewItem lvItem, bool bInclude)
        {
            lvItem.SubItems[4].Text = (bInclude) ? "Yes" : "No";
        }

        private void form_btn_ToggleInclude_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection lvSelect = form_lv_Volumes.SelectedItems;

            if (lvSelect.Count <= 0)
            {
                return;
            }

            SetLV_VolumesItemInclude(lvSelect[0], LV_VolumesItemInclude(lvSelect[0]) == false);
            m_bBrowseLoaded = false;
        }

        private bool LV_VolumesItemInclude(ListViewItem lvItem)
        {
            return (lvItem.SubItems[4].Text == "Yes");
        }

        private void form_btn_SavePathInfo_Click(object sender, EventArgs e)
        {
            DoSavePathInfo();
        }

        private void form_btn_SaveVolumeList_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (File.Exists(saveFileDialog1.FileName))
            {
                if (MessageBox.Show(saveFileDialog1.FileName + " already exists. Overwrite?         ", "Volume List Save As", MessageBoxButtons.YesNo)
                    != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }
            }

            using (TextWriter fs = File.CreateText(saveFileDialog1.FileName))
            {
                fs.WriteLine(Utilities.m_str_VOLUME_LIST_HEADER);

                foreach (ListViewItem lvItem in form_lv_Volumes.Items)
                {
                    foreach (ListViewItem.ListViewSubItem lvSubitem in lvItem.SubItems)
                    {
                        fs.Write(lvSubitem.Text + "\t");
                    }

                    fs.WriteLine();
                }
            }
        }

        private void form_btn_LoadVolumeList_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
            openFileDialog1.FileName = saveFileDialog1.FileName;

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (TextReader fs = File.OpenText(openFileDialog1.FileName))
            {
                String strLine = fs.ReadLine();

                if (strLine != Utilities.m_str_VOLUME_LIST_HEADER)
                {
                    MessageBox.Show(openFileDialog1.FileName + " is not a valid volume list file.", "Load Volume List");
                    return;
                }

                form_lv_Volumes.Items.Clear();

                while ((strLine = fs.ReadLine()) != null)
                {
                    String[] strArray = strLine.Split('\t');

                    strArray[3] = "Using file.";

                    if (File.Exists(strArray[2]) == false)
                    {
                        strArray[2] = Path.GetDirectoryName(openFileDialog1.FileName) + Path.DirectorySeparatorChar + Path.GetFileName(strArray[2]);

                        if (File.Exists(strArray[2]) == false)
                        {
                            strArray[3] = "No file. Will create.";
                        }
                    }

                    form_lv_Volumes.Items.Add(new ListViewItem(strArray));
                }
            }

            if (form_lv_Volumes.Items.Count > 0)
            {
                form_btn_SavePathInfo.Enabled = true;
            }

            m_bBrowseLoaded = false;
        }

        private void UpdateLV_VolumesSelection()
        {
            bool bHasSelection = (form_lv_Volumes.SelectedIndices.Count > 0);

            form_btn_RemoveVolume.Enabled = bHasSelection;
            form_btn_ToggleInclude.Enabled = bHasSelection;
        }

        private void form_lv_Volumes_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            UpdateLV_VolumesSelection();
        }

        class NodeDatum
        {
            long m_nPrevLineNo = 0;
            long m_nLineNo = 0;

            public long PrevlineNo { get { return m_nPrevLineNo; } }
            public long LineNo { get { return m_nLineNo; } }
            public NodeDatum(long nPrevLineNo, long nLineNo) { m_nPrevLineNo = nPrevLineNo; m_nLineNo = nLineNo; }
        }

        class Node
        {
            static SortedDictionary<String, Node> nodes = null;
            SortedDictionary<String, Node> subNodes = new SortedDictionary<string, Node>();
            String m_strPath = "";
            static long m_nStaticLineNo = 0;
            long m_nPrevLineNo = 0;
            long m_nLineNo = 0;
            bool bUseShortPath = true;

            public Node(String in_str, long nLineNo)
            {
                if (in_str.EndsWith(":" + Path.DirectorySeparatorChar) == false)
                {
                    Debug.Assert(in_str.Trim().EndsWith(Path.DirectorySeparatorChar.ToString()) == false);
                }

                m_strPath = in_str;
                m_nPrevLineNo = m_nStaticLineNo;
                m_nStaticLineNo = m_nLineNo = nLineNo;

                // Path.GetDirectoryName() does not preserve filesystem root

                String strParent = m_strPath;
                int nIndex = strParent.LastIndexOf(Path.DirectorySeparatorChar);

                if (nIndex < 0)
                {
                    return;
                }

                strParent = strParent.Remove(nIndex);

                if (nodes.ContainsKey(strParent) == false)
                {
                    nodes.Add(strParent, new Node(strParent, 0));
                }

                if (nodes[strParent].subNodes.ContainsKey(m_strPath) == false)
                {
                    nodes[strParent].subNodes.Add(m_strPath, this);
                }
            }

            public static void SetRootNode(SortedDictionary<String, Node> node)
            {
                nodes = node;
            }

            public TreeNode AddToTree(String strVolumeName = null)
            {
                int nIndex = m_strPath.LastIndexOf(Path.DirectorySeparatorChar);
                String strShortPath = bUseShortPath ? m_strPath.Substring(nIndex + 1) : m_strPath;

                if (strVolumeName != null)
                {
                    bool bNotRedundant = (strVolumeName.EndsWith(strShortPath) == false);

                    if (bNotRedundant)
                    {
                        strShortPath = strVolumeName + " (" + strShortPath + ")";
                    }
                    else
                    {
                        strShortPath = strVolumeName;
                    }
                }

                TreeNode treeNode = null;

                if (subNodes.Count == 1)
                {
                    Node subNode = subNodes.Values.First();

                    if (this == nodes.Values.First())
                    {
                        // cull all root node single-chains.
                        SetRootNode(subNodes);
                        subNode.m_strPath.Insert(0, m_strPath + Path.DirectorySeparatorChar);
                        subNode.bUseShortPath = false;
                        treeNode = subNode.AddToTree(strVolumeName);
                    }
                    else
                    {
                        treeNode = new TreeNode(strShortPath, new TreeNode[] { subNode.AddToTree() });
                    }
                }
                else if (subNodes.Count > 1)
                {
                    List<TreeNode> treeList = new List<TreeNode>();

                    foreach (Node node in subNodes.Values)
                    {
                        treeList.Add(node.AddToTree());
                    }

                    treeNode = new TreeNode(strShortPath, treeList.ToArray());
                }
                else
                {
                    treeNode = new TreeNode(strShortPath);
                }

                treeNode.Tag = new NodeDatum(m_nPrevLineNo, m_nLineNo);
                return treeNode;
            }
        }

        class DirData
        {
            TreeView form_treeView_Browse = null;
            SortedDictionary<String, Node> nodes = new SortedDictionary<string, Node>();

            public DirData(TreeView ctl)
            {
                form_treeView_Browse = ctl;
                Node.SetRootNode(nodes);
            }

            public void AddToTree(String in_str, long nLineNo)
            {
                if (nodes.ContainsKey(in_str))
                {
                    Node node = nodes[in_str];
                    Debug.Assert(false);
                }

                while (in_str.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    in_str = in_str.Remove(in_str.LastIndexOf(Path.DirectorySeparatorChar));
                }

                nodes.Add(in_str, new Node(in_str, nLineNo));
            }

            public TreeNode AddToTree(String strVolumeName)
            {
                TreeNode rootNode = nodes.Values.First().AddToTree(strVolumeName);

                form_treeView_Browse.Nodes.Add(rootNode);
                return rootNode;
            }
        }
        
        private void form_tabPage_Browse_Paint(object sender, PaintEventArgs e)
        {
            if (m_bBrowseLoaded)
            {
                return;
            }

        //    new Thread(new ThreadStart(CreateBrowsingTree)).Start();
        //}

        //private void CreateBrowsingTree()
        //{
            form_treeView_Browse.Nodes.Clear();
            form_LV_Files.Items.Clear();
            form_LV_Detail.Items.Clear();
            m_hashCache.Clear();

            Console.WriteLine();
            Console.WriteLine("Creating browsing tree.");

            DateTime dtStart = DateTime.Now;

            foreach (ListViewItem lvItem in form_lv_Volumes.Items)
            {
                if (LV_VolumesItemInclude(lvItem) == false)
                {
                    continue;
                }

                form_cb_VolumeName.Text = lvItem.SubItems[0].Text;
                form_cb_Path.Text = lvItem.SubItems[1].Text;
                form_cb_SaveAs.Text = lvItem.SubItems[2].Text;

                if (SaveFields(false) == false)
                {
                    return;
                }

                using (StreamReader file = new StreamReader(m_strSaveAs))
                {
                    String line = "";
                    DirData dirData = new DirData(form_treeView_Browse);
                    long nLineNo = -1;

                    while ((line = file.ReadLine()) != null)
                    {
                        ++nLineNo;

                        StringBuilder strDriveInfo = new StringBuilder();

                        if (line == Utilities.m_str_DRIVE)
                        {
                            for (int i = 0; i < 8; ++i)
                            {
                                strDriveInfo.AppendLine(file.ReadLine());
                                ++nLineNo;
                            }

                            m_hashCache.Add("driveInfo" + m_strSaveAs, strDriveInfo.ToString().Trim());
                            continue;
                        }

                        if (line.Contains('\t') == false)
                        {
                            continue;
                        }

                        if (line.Contains(":" + Path.DirectorySeparatorChar) == false)
                        {
                            continue;
                        }

                        String[] strArray = line.Split('\t');
                        String strNew = strArray[0];

                        if (strNew.Length <= 0)
                        {
                            continue;
                        }

                        // directory
                        dirData.AddToTree(strNew, nLineNo);
                    }

                    dirData.AddToTree(m_strVolumeName).Tag = m_strSaveAs;
                }
            }

            Console.WriteLine(String.Format("Completed browsing tree in {0} seconds.", ((int) (DateTime.Now - dtStart).TotalMilliseconds/10)/100.0));
            m_bBrowseLoaded = true;
        }

        private void timer_killRed_Tick(object sender, EventArgs e)
        {
            form_cb_VolumeName.BackColor = Color.Empty;
            form_cb_Path.BackColor = Color.Empty;
            form_cb_SaveAs.BackColor = Color.Empty;
            timer_killRed.Enabled = false;
        }

        private void form_treeView_Browse_AfterSelect(object sender, TreeViewEventArgs e)
        {
            form_LV_Detail.Items.Clear();
            form_LV_Files.Items.Clear();

            TreeNode nodeParent = e.Node;

            while (nodeParent.Parent != null)
            {
                nodeParent = nodeParent.Parent;
            }

            Debug.Assert(nodeParent.Tag is String);

            String strFile = (String)nodeParent.Tag;

            if (File.Exists(strFile) == false)
            {
                Debug.Assert(false);
                return;
            }

            if ((e.Node.Tag is NodeDatum) == false)
            {
                return;
            }

            NodeDatum nodeDatum = (NodeDatum) e.Node.Tag;

            if (nodeDatum.LineNo <= 0)
            {
                return;
            }

            long nPrevDir = nodeDatum.PrevlineNo;
            long nLineNo = nodeDatum.LineNo;
            String strLine = File.ReadLines(strFile).Skip((int)nLineNo).Take(1).ToArray()[0];
            String[] strArray = strLine.Split('\t');
            long nIx = 0;
            DateTime dt;


            // Directory detail

            nIx = 2; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) { form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Created\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() })); }
            nIx = 3; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Modified\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() }));
            nIx = 4; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Attributes\t", strArray[nIx] }));
            nIx = 5;

            long nLengthDebug_A = 0;

            if ((strArray.Length > nIx) && (strArray[nIx].Length > 0))
            {
                Debug.Assert(long.TryParse(strArray[nIx], out nLengthDebug_A));
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Size\t", Utilities.FormatSize(strArray[nIx], true) }));
            }

            nIx = 6; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Error 1\t", strArray[nIx] }));
            nIx = 7; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Error 2\t", strArray[nIx] }));

            Console.WriteLine(strLine);

            
            // Volume detail

            if (m_hashCache.ContainsKey("driveInfo" + strFile))
            {
                String strDriveInfo = (String)m_hashCache["driveInfo" + strFile];
                String[] arrDriveInfo = strDriveInfo.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.None);

                Debug.Assert(new int[] { 7, 8 }.Contains(arrDriveInfo.Length));
                form_LV_Detail.Items.Add(new ListViewItem());

                ListViewItem lvItem = new ListViewItem("Volume detail");

                lvItem.BackColor = Color.DarkGray;
                lvItem.ForeColor = Color.White;
                form_LV_Detail.Items.Add(lvItem);
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Available Free Space", Utilities.FormatSize(arrDriveInfo[0], true) }));
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Drive Format", arrDriveInfo[1] }));
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Drive Type", arrDriveInfo[2] }));
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Name", arrDriveInfo[3] }));
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Root Directory", arrDriveInfo[4] }));
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Total Free Space", Utilities.FormatSize(arrDriveInfo[5], true) }));
                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Total Size", Utilities.FormatSize(arrDriveInfo[6], true) }));

                if (arrDriveInfo.Length == 8)
                {
                    form_LV_Detail.Items.Add(new ListViewItem(new String[] { "Volume Label", arrDriveInfo[7] }));
                }
            }


            // file list

            if (nPrevDir <= 0)
            {
                return;
            }

            if ((nLineNo - nPrevDir) <= 1)  // dir has no files
            {
                return;
            }

            long nLengthDebug = 0;

            if (m_hashCache.ContainsKey(strLine) == false)
            {
                DateTime dtStart = DateTime.Now;
                List<String> listLines = File.ReadLines(strFile)
                    .Skip((int)nPrevDir+1)
                    .Take((int)(nLineNo - nPrevDir - 1))
                    .ToList();

                listLines.Sort();

                for (int i = 0; i < listLines.Count; ++i)
                {
                    strArray = listLines[i].Split('\t');

                    if ((strArray.Length > 5) && (strArray[5].Length > 0))
                    {
                        nLengthDebug += long.Parse(strArray[5]);
                        strArray[5] = Utilities.FormatSize(strArray[5]);
                    }

                    form_LV_Files.Items.Add(new ListViewItem(strArray));
                }

                TimeSpan timeSpan = (DateTime.Now - dtStart);
                String strTimeSpan = (((int)timeSpan.TotalMilliseconds / 100) / 10.0).ToString();

                Console.WriteLine("File list took " + strTimeSpan + " seconds.");

                if (timeSpan.Seconds > 1)
                {
                    ListViewItem[] itemArray = new ListViewItem[form_LV_Files.Items.Count];

                    form_LV_Files.Items.CopyTo(itemArray, 0);
                    m_hashCache.Add(strLine, itemArray);
                    m_hashCache.Add("nLengthDebug" + strLine, nLengthDebug);
                    m_hashCache.Add("timeSpan" + strLine, strTimeSpan);
                    Console.WriteLine("Cached.");
                }

                form_LV_Detail.Items.Add(new ListViewItem(new String[] { "# Files", (nLineNo - nPrevDir + 1).ToString() }));
            }
            else    // file list is cached
            {
                DateTime dtStart = DateTime.Now;
                ListViewItem[] itemArray = (ListViewItem[])m_hashCache[strLine];

                form_LV_Files.Items.AddRange(itemArray);

                if (itemArray.Length > 0)
                {
                    form_LV_Detail.Items.Add(new ListViewItem(new String[] { "# Files", itemArray.Length.ToString() }));
                }

                TimeSpan timeSpan = (DateTime.Now - dtStart);

                nLengthDebug = (long)m_hashCache["nLengthDebug" + strLine];
                Console.WriteLine("File list used to take " + (String)m_hashCache["timeSpan" + strLine] + " seconds before caching.");
                Console.WriteLine("Cache read took " + (int)timeSpan.TotalMilliseconds + " milliseconds.");
            }

            Debug.Assert(nLengthDebug == nLengthDebug_A);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            DoSearch();
        }

        private void form_cb_Search_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (new Keys[] {Keys.Enter, Keys.Return}.Contains((Keys) e.KeyChar))
            {
                btnSearch_Click(sender, e);
            }
        }
    }
}
