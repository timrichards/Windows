using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Collections;               // ArrayList
using System.Xml;

namespace NS_MyData
{
    public partial class MyData
    {
        static public void NeverHit() { }
        static StringBuilder sBadNodes = new StringBuilder();   // caused by duplicates of moKey

        public object moKey = null;
        public object moValue = null;

        public ArrayList marr = new ArrayList();

        bool marrIs { get { return marr is ArrayList && marr.Count > 0; } }
        MyData marrLast { get { return marrIs ? marr[marr.Count - 1] as MyData : null; } set { marr[marr.Count - 1] = value; } }
        MyData mSubNode
        {
            get { return marrLast; }
            set { if (marrIs && !(marr[marr.Count - 1] is MyData)) marrLast = value; else marr.Add(value); }
        }
        MyData InnerAlmost { get { return mSubNode is MyData && mSubNode.mSubNode is MyData ? mSubNode.InnerAlmost : this; } }
        MyData InnerMost { get { return mSubNode is MyData ? mSubNode.InnerMost : this; } }

        public MyData() { }
        private MyData(object sKey) { moKey = sKey; }
        public void NewElement(object sKey) { if (mSubNode is MyData) InnerMost.NewElement(sKey); else mSubNode = new MyData(sKey); }
        public void ElementData(object sValue) { if (mSubNode is MyData) InnerMost.ElementData(sValue); else moValue = sValue; }
        public void EndElement(object sKey)
        {
            if (InnerAlmost != this) { InnerAlmost.EndElement(sKey); return; }

            if (moKey == sKey)
                return;

            if (!(mSubNode is MyData))          // caused by duplicates of moKey
            {
                NeverHit();
                sBadNodes.Append("moKey:").Append(moKey.ToString()).Append("     sKey:").AppendLine(sKey.ToString());
                return;
            }

            mSubNode.EndElement(sKey);
            mSubNode = null;                    // kick off a new one
        }

        public void RemoveNulls()
        {
            for (ushort i = 0; i < marr.Count; ++i) if (marr[i] is MyData) ((MyData)marr[i]).RemoveNulls();
            if (marr.Count == 0) { marr = null; return; }
            if (marr[marr.Count - 1] == null) marr.RemoveAt(marr.Count - 1);
        }

        public static MyData dOuterNode = new MyData();
        private static bool _ReadXML(XmlTextReader xtr) { try { return xtr.Read(); } catch { return false; } }   // Unexpected eof
        public static void ReadXML(XmlTextReader xtr)
        {
            while (_ReadXML(xtr))
            {
                switch (xtr.NodeType)
                {
                    case XmlNodeType.XmlDeclaration: break;

                    case XmlNodeType.Element:
                        if (xtr.HasValue) NeverHit();

                        if (!xtr.HasAttributes && xtr.IsEmptyElement)
                            break;

                        object s = xtr.Name;
                        dOuterNode.NewElement(s);

                        bool bEndElement = xtr.IsEmptyElement;

                        while (xtr.MoveToNextAttribute())
                        {
                            // Each of these iterative xtr.Names is different from `s' above...
                            dOuterNode.NewElement(xtr.Name);
                            dOuterNode.ElementData(xtr.Value);
                            dOuterNode.EndElement(xtr.Name);
                        }

                        if (bEndElement)
                            dOuterNode.EndElement(s);

                        break;

                    case XmlNodeType.Text:
                        if (xtr.Name.Length > 0) NeverHit();

                        dOuterNode.ElementData(xtr.Value);
                        break;

                    case XmlNodeType.CDATA:
                        if (xtr.Name.Length > 0) NeverHit();

                        dOuterNode.ElementData(xtr.Value);
                        break;

                    case XmlNodeType.EndElement:
                        if (xtr.IsEmptyElement) NeverHit();
                        if (xtr.HasValue) NeverHit();
                        if (xtr.HasAttributes) NeverHit();

                        dOuterNode.EndElement(xtr.Name);
                        break;

                    case XmlNodeType.Attribute: NeverHit(); break;
                    case XmlNodeType.Comment: NeverHit(); break;
                    case XmlNodeType.Document: NeverHit(); break;
                    case XmlNodeType.DocumentFragment: NeverHit(); break;
                    case XmlNodeType.DocumentType: NeverHit(); break;
                    case XmlNodeType.EndEntity: NeverHit(); break;
                    case XmlNodeType.Entity: NeverHit(); break;
                    case XmlNodeType.EntityReference: NeverHit(); break;
                    case XmlNodeType.None: NeverHit(); break;
                    case XmlNodeType.Notation: NeverHit(); break;
                    case XmlNodeType.ProcessingInstruction: NeverHit(); break;
                    case XmlNodeType.SignificantWhitespace: NeverHit(); break;
                    case XmlNodeType.Whitespace: NeverHit(); break;
                    default: NeverHit(); break;
                }
            }

            dOuterNode.RemoveNulls();
            dOuterNode = dOuterNode.mSubNode;
        }

        public static string indent(ushort level)
        {
            StringBuilder s = new StringBuilder();

            for (ushort i = 0; i < level; ++i)
                s.Append(" ");
            return s.ToString();
        }

        public string Schema(ushort level)
        {
            if (!marrIs)
                return null;

            StringBuilder s = new StringBuilder();

            string sKey = null;
            for (ushort i = 0; i < marr.Count; ++i)
            {
                MyData d = ((MyData)marr[i]);

                if (sKey != null && d.moKey.ToString() == sKey)
                    continue;

                sKey = d.moKey.ToString();

                s
                    .Append(indent(level))
                    .AppendLine(d.moKey.ToString())
                    .Append(d.Schema((ushort)(level + 1)));
            }
            return s.ToString();
        }

        //public static ArrayList Schema()
        //{
        //    ArrayList arr = new ArrayList();

        //    string sKey = null;
        //    for (ushort i = 0; i < myD.marr.Count; ++i)
        //    {
        //        MyData d = ((MyData)marr[i]);

        //        if (sKey != null && d.msKey == sKey)
        //            continue;

        //        sKey = d.msKey;

        //        arr.AddRange(indent(level) + d.msKey);
        //        s
        //            .Append(d.Schema((ushort)(level + 1)));
        //    }
        //    return s.ToString();
        //}
    }
}
