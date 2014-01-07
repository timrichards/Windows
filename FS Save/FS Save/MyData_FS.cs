using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Drawing;                   // Icon
using System.Collections;               // ArrayList
using System.Windows.Forms;             // TreeNodeCollection
using System.Security.Cryptography;     // MD5CryptoServiceProvider
using System.Xml;

namespace NS_MyData
{
    class MyKey
    {
        static string FileSize(float size)
        {
            if (size <= 0) return string.Empty;
            if (size < 1024) return size.ToString("#,###.##") + " Bytes";
            size /= 1024; if (size < 1024) return size.ToString("#,###.##") + " KB";
            size /= 1024; if (size < 1024) return size.ToString("#,###.##") + " MB";
            size /= 1024; return size.ToString("#,###.##") + " GB";
        }
        static string MD5sum(byte[] b) { return BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(b)).Replace("-", "").ToLower(); }
        static public string NoExt(string sName) { return sName.Contains(".") ? sName.Substring(0, sName.LastIndexOf('.')) : sName; }

        public string sExt { get { return sName.Contains(".") ? sName.Substring(sName.LastIndexOf('.') + 1) : ""; } }
        public string sName = string.Empty;
        public bool bTypeSpecial
        {
            get
            {
                switch (sType)
                {
                    case "File Folder":
                    case "Root":
                    case "Restricted Folder":
                    case "Problem Folder": return true;
                    default: return false;
                }
            }
        }
        public string sNameOnly { get { return bTypeSpecial ? sName : NoExt(sName); } }
        public float mfSize = 0;
        public string sSize { get { return FileSize(mfSize); } }
        public DateTime dtModified = DateTime.MinValue;
        public string sType = string.Empty;
        public string s_type { get { return sType.ToLower(); } }
        public DateTime dtCreated = DateTime.MinValue;
        public string sMD5 = string.Empty;

        ListViewItem mLVI = null;
        public ListViewItem lvi
        {
            get
            {
                if (mLVI is ListViewItem)
                    return mLVI;

                mLVI = new ListViewItem(
                    new string[] {
                        sName,
                        sSize,
                        " " + dtModified.ToString(),
                        sType,
                        " " + dtCreated.ToString()
                    }, s_type
                    );

                // Sort on Tag, not on visible
                ListViewItem.ListViewSubItemCollection lvsic = mLVI.SubItems;
                for (ushort i = 0; i < lvsic.Count; ++i)
                {
                    lvsic[i].Tag = lvsic[i].Text;
                    if (lvsic[i].Tag != (object)lvsic[i].Text) MyData.NeverHit();       // Means that the string's not duplicated
                }

                // Sort folders separately from other items
                if (bTypeSpecial)
                {
                    mLVI.SubItems[0].Tag = " " + mLVI.SubItems[0].Text;   // Name
                    mLVI.SubItems[3].Tag = " " + mLVI.SubItems[3].Text;   // Type
                    if (mLVI.SubItems[0].Tag == (object)mLVI.Text) MyData.NeverHit();
                    if (mLVI.SubItems[3].Tag == (object)mLVI.Text) MyData.NeverHit();
                }

                mLVI.SubItems[1].Tag = mfSize;                                          // Size in bytes as int v. display size

                return mLVI;
            }
        }

        static public string sRoot = string.Empty;
        static public bool bExportRoot = false;

        static public ImageList imglIcons = new ImageList();
        private void AddIcon(string sFullName)
        {
            if (imglIcons.Images.ContainsKey(s_type))
                return;

            imglIcons.Images.Add(s_type, Win32.GetFileIcon(sFullName, Win32.SHGFI_SMALLICON));
        }

        private void init(string s, FileSystemInfo fsi)
        {
            if (sRoot.Length == 0) sRoot = fsi.FullName.Substring(0, fsi.FullName.LastIndexOf('\\'));

            sName = fsi.Name;
            dtModified = fsi.LastWriteTime;
            dtCreated = fsi.CreationTime;

            sType = s.Length > 0 ? s : sExt.Length > 0 ? sExt : "Document";
            AddIcon(fsi.FullName);
        }

        public MyKey() { }
        public MyKey(DirectoryInfo di)
        {
            if (di.FullName == di.Root.FullName)
            {
                sType = "Root";
                AddIcon(@"C:\");
                init("Root", di);
            }
            else init("File Folder", di);

            if (di.Name != sName) MyData.NeverHit();
            mfSize = 0;
        }

        public MyKey(FileInfo fi)
        {
            init(string.Empty, fi);

            if (fi.Name != sName || fi.Extension.Replace(".", "") != sExt) MyData.NeverHit();
            mfSize = fi.Length;
        }

        static public string FormatXML(string s) { return s.Replace("&amp;", "|?:").Replace("&", "&amp;").Replace("|?:", "&amp;"); }

        public override string ToString()
        {
            bool b_export_root = bExportRoot ? !(bExportRoot = false) : false;

            return
                (b_export_root ? " Root=\"" + FormatXML(sRoot) + "\"" : "")
                + (sType.Length > 0 ? " Type=\"" + sType + "\"" : "")
                + (sMD5.Length > 0 ? " MD5=\"" + sMD5 + "\"" : "")
                + " Modified=\"" + dtModified
                + "\" Created=\"" + dtCreated
                + "\"" + (sType != "File Folder" ? " Size=\"" + mfSize.ToString() + "\"" : "")
                + " Name=\"" + FormatXML(sNameOnly) + "\"";
        }
    }

    public partial class MyData
    {
        private void _GetSubdirInfo(DirectoryInfo di)
        {
            if (!(di is DirectoryInfo)) return;

            MyKey mkd = new MyKey(di);
            NewElement(mkd);
            ElementData(di.Name);

            bool bArrDIok = true;
            DirectoryInfo[] arrDI = null;
            try { arrDI = di.GetDirectories(); }
            catch (ArgumentException e) { bArrDIok = false; mkd.sType = "Problem Folder"; }
            catch { bArrDIok = false; mkd.sType = "Restricted Folder"; }

            bool bArrFIok = true;
            FileInfo[] arrFI = null;
            try { arrFI = di.GetFiles(); }
            catch (ArgumentException e) { bArrFIok = false; mkd.sType = "Problem Folder"; }
            catch { bArrFIok = false; mkd.sType = "Restricted Folder"; }

            if (bArrDIok)
                for (ushort i = 0; i < arrDI.Length; ++i) mSubNode._GetSubdirInfo(arrDI[i]);

            if (bArrFIok)
            {
                for (ushort i = 0; i < arrFI.Length; ++i)
                {
                    MyKey mkf = new MyKey(arrFI[i]);
                    NewElement(mkf);
                    ElementData(arrFI[i].Name);
                    EndElement(mkf);
                }
            }

            EndElement(mkd);
        }
        public static void GetSubdirInfo(DirectoryInfo di)
        {
            dOuterNode = new MyData();
            MyKey.sRoot = string.Empty;

            dOuterNode._GetSubdirInfo(di);

            if (dOuterNode.moKey != null || dOuterNode.moValue != null) NeverHit();

            dOuterNode.RemoveNulls();
            dOuterNode = dOuterNode.mSubNode;
        }

        public void PopulateTree(TreeNodeCollection tNodes)
        {
            if (!marrIs) return;     // Don't put non-containers in tree.
            string s = string.Empty;
            if (moValue != null) { s = moValue.ToString(); if (s.Length > 0) { tNodes.Add(s, s, 0); tNodes[s].Tag = this; } }
            for (ushort i = 0; i < marr.Count; ++i) ((MyData)marr[i]).PopulateTree(s.Length > 0 ? tNodes[s].Nodes : tNodes);
        }

        public void Export(TextWriter tw, ushort level)
        {
            if (moKey == null && moValue == null) NeverHit();

            tw.Write(indent(level) + "<Path" + level);

            if (moKey != null)
                tw.Write(((MyKey)moKey).ToString());

            if (marrIs)
            {
                tw.WriteLine(">");
                for (ushort i = 0; i < marr.Count; ++i) if (marr[i] is MyData) ((MyData)marr[i]).Export(tw, (ushort)(level + 1));
                tw.WriteLine(indent(level) + "</Path" + level + ">");
            }
            else tw.WriteLine("/>");
        }

        private void Import(MyData myD)
        {
            if (!myD.marrIs)
                return;

            MyKey mk = new MyKey();
            moKey = mk;

            for (ushort i = 0; i < myD.marr.Count; ++i)
            {
                MyData d = ((MyData)myD.marr[i]);

                switch (d.moKey.ToString())
                {
                    case "Root":
                        if (MyKey.sRoot.Length > 0) NeverHit();
                        MyKey.sRoot = d.moValue.ToString(); break;
                    case "Type": mk.sType = d.moValue.ToString(); break;
                    case "MD5": mk.sMD5 = d.moValue.ToString(); break;
                    case "Modified": mk.dtModified = Convert.ToDateTime(d.moValue); break;
                    case "Created": mk.dtCreated = Convert.ToDateTime(d.moValue); break;
                    case "Size": mk.mfSize = (float)Convert.ToDouble(d.moValue); break;
                    case "Name":
                        mk.sName = d.moValue.ToString().Replace("&amp;", "&");
                        moValue = mk.sName;
                        break;
                    default:
                        if (d.moKey.ToString().Substring(0, 4) == "Path")
                            (mSubNode = new MyData()).Import(d);
                        else
                            NeverHit();
                        break;
                }
            }
        }

        public static void Import(string sFullFileName)
        {
            XmlTextReader xtr = new XmlTextReader(new StreamReader(sFullFileName).BaseStream);
            xtr.WhitespaceHandling = WhitespaceHandling.None;

            MyData.dOuterNode = new MyData();
            MyData.ReadXML(xtr);

            MyData myD = MyData.dOuterNode;
            MyData.dOuterNode = new MyData();

            MyKey.sRoot = string.Empty;
            dOuterNode.Import(myD);
        }
    }
}
