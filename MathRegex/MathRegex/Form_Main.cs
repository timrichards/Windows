using System;                   // EventArgs
using System.Windows.Forms;     // Form
using System.Text.RegularExpressions;
using System.IO;                // StreamReader
using System.Drawing;           // Point

namespace RegexReplace
{
    public partial class Form_Main : Form
    {
        public Rectangle StartLocation = Rectangle.Empty;

        bool mbGo;
        WebBrowser mweb = new WebBrowser();
        public Form_Main()
        {
            InitializeComponent();
            mweb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(web_DocumentCompleted);
        }

        uint miMax = 0;
        void ProgressInit(uint iMax) { miMax = iMax; lblProgMax.Text = "" + (progressBarX1.Maximum = (int)iMax); lblProgCurr.Text = "" + (progressBarX1.Value = 0); panelEx2.Visible = true; }
        void Progress() { lblProgCurr.Text = "" + (progressBarX1.Value = (int)++miFound); }
        void ProgressDispose() { progressBarX1.Value = 0; panelEx2.Visible = false; }

        string msGuid;
        string Replace(string s)
        { return s.Replace("${ix}", miFound + "").Replace("\r", "/*R" + msGuid + "*/").Replace("\n", "/*N" + msGuid + "*/").Replace("'", "/*SQ" + msGuid + "*/").Replace("\"", "/*DQ" + msGuid + "*/"); }
        string Dereplace(string s)
        { return s.Replace("/*R" + msGuid + "*/", "\r").Replace("/*N" + msGuid + "*/", "\n").Replace("/*SQ" + msGuid + "*/", "'").Replace("/*DQ" + msGuid + "*/", "\""); }

        string msMath;
        uint miFound;
        uint miMathReplaced;
        uint miNoMathReplaced;
        uint miNoEvalReplaced;
        uint miNotParsedReplaced;
        bool mb_web_DocumentCompleted;
        string MyMatchEvaluator(Match m)
        {
            if (!mbGo)
                return string.Empty;

            Progress();

            string sReplace = tbReplace.Text;
            if (string.IsNullOrEmpty(tbMath.Text) || (!tbReplace.Text.Contains("${Math}")))
                ++miNoMathReplaced;
            else
            {
                string sTempHTML = Application.StartupPath + @"\temp.html";
                msGuid = new Guid().ToString();
                using (StreamWriter sw = new StreamWriter(sTempHTML, false))
                    sw.Write(
@"<html><head><script type='text/javascript'>window.onload=function(){try{document.getElementById('d').innerHTML=eval('" + Replace(m.Result(msMath)) + @"');}catch(e0){document.getElementById('e').innerHTML='e';document.getElementById('d').innerHTML=eval('" + Replace(m.Value) + @"');}}</script></head><body><div id='d'></div><div id='e'></div></body></html>"
                    );

                mb_web_DocumentCompleted = false;
                mweb.Navigate(sTempHTML);
                while (!mb_web_DocumentCompleted)
                    Application.DoEvents();
                File.Delete(sTempHTML);
                double dOut;
                if (mweb.Document.GetElementById("e").InnerHtml == "e")
                    ++miNoEvalReplaced;
                if (double.TryParse(Dereplace(mweb.Document.GetElementById("d").InnerHtml), out dOut))
                {
                    ++miMathReplaced;
                    sReplace = sReplace.Replace("${Math}", dOut.ToString(tbFormat.Text));
                }
                else
                    ++miNotParsedReplaced;
            }
            return m.Result(sReplace).Replace("${ix}", miFound + "");
        }

        void web_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            mb_web_DocumentCompleted = true;
        }

        RegexOptions GetMyRegexOptions()
        {
            RegexOptions regexOptions = RegexOptions.None;

            if (ckCompiled.Checked) regexOptions |= RegexOptions.Compiled;
            if (ckCultureInvariant.Checked) regexOptions |= RegexOptions.CultureInvariant;
            if (ckECMAScript.Checked) regexOptions |= RegexOptions.ECMAScript;
            if (ckExplicitCapture.Checked) regexOptions |= RegexOptions.ExplicitCapture;
            if (ckIgnoreCase.Checked) regexOptions |= RegexOptions.IgnoreCase;
            if (ckIgnorePatternWhitespace.Checked) regexOptions |= RegexOptions.IgnorePatternWhitespace;
            if (ckMultiline.Checked) regexOptions |= RegexOptions.Multiline;
            if (ckRightToLeft.Checked) regexOptions |= RegexOptions.RightToLeft;
            if (ckSingleLine.Checked) regexOptions |= RegexOptions.Singleline;

            return regexOptions;
        }

        void SetMyRegexOptions(int value)
        {
            ckCompiled.Checked = (value & (uint)RegexOptions.Compiled) != 0;
            ckCultureInvariant.Checked = (value & (uint)RegexOptions.CultureInvariant) != 0;
            ckECMAScript.Checked = (value & (uint)RegexOptions.ECMAScript) != 0;
            ckExplicitCapture.Checked = (value & (uint)RegexOptions.ExplicitCapture) != 0;
            ckIgnoreCase.Checked = (value & (uint)RegexOptions.IgnoreCase) != 0;
            ckIgnorePatternWhitespace.Checked = (value & (uint)RegexOptions.IgnorePatternWhitespace) != 0;
            ckMultiline.Checked = (value & (uint)RegexOptions.Multiline) != 0;
            ckRightToLeft.Checked = (value & (uint)RegexOptions.RightToLeft) != 0;
            ckSingleLine.Checked = (value & (uint)RegexOptions.Singleline) != 0;
        }

        private void Form_Main_Load(object sender, EventArgs e)
        {
            if (!StartLocation.IsEmpty)
            { Location = StartLocation.Location; Size = StartLocation.Size; ckTopmost.Checked = TopMost; expandFunctions.Expanded = Program.mbFuncsExpanded; }
            tbSearchRegex.Text = Program.msSearch;
            SetMyRegexOptions(Program.miRegexOptions);
            tbMath.Text = Program.msMath;
            tbFormat.Text = Program.msFormat;
            tbReplace.Text = Program.msReplace;
        }

        private void Form_Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (mweb is WebBrowser)
                mweb.Dispose();
            mbGo = false;
            if (mbMaximized) TopMost = mbPrevTopmost;

            Program.msSearch = tbSearchRegex.Text;
            Program.miRegexOptions = (int)GetMyRegexOptions();
            Program.msMath = tbMath.Text;
            Program.msFormat = tbFormat.Text;
            Program.msReplace = tbReplace.Text;
        }

        void buttonX1_Click(object sender, EventArgs e)
        {
            string sContent = null;
            try
            {
                string sReplaced;
                using (StreamReader sr = new StreamReader(msFileName))
                {
                    sContent = sr.ReadToEnd();
                    tbMain.Text = "Please wait...\r\n\r\n" + sContent;
                    tbMain.Refresh();

                    pnlMain.Enabled = false;
                    mbGo = true;
                    Regex myRegex = new Regex(tbSearchRegex.Text, GetMyRegexOptions());
                    msMath = new Regex(@"(?<!\$\{\s*)(?<A>\b([\w][^\d]*)+)").Replace(tbMath.Text, "Math.${A}");
                    miFound = miMathReplaced = miNoMathReplaced = miNotParsedReplaced = miNoEvalReplaced = 0;
                    ProgressInit((uint)myRegex.Matches(sContent).Count);
                    sReplaced = myRegex.Replace(sContent, new MatchEvaluator(MyMatchEvaluator));
                }

                if (!mbGo)
                {
                    tbMain.Text = sContent;
                    return;
                }

                using (StreamWriter sw = new StreamWriter(msFileName, false))
                    sw.Write(sReplaced);

                tbMain.Text = sContent = sReplaced;  // sContent gets used in finally
                tbMain.Refresh();

                if (DialogResult.Yes == MessageBox.Show(this, miFound + " found.\r\n" + miMathReplaced + " replaced math.\r\n" + miNoMathReplaced + " replaced non-math.\r\n" + miNoEvalReplaced + " replaced, couldn't evaluate.\r\n" + miNotParsedReplaced + " replaced, evaluated, couldn't parse result. Quit?", "Completed.", MessageBoxButtons.YesNo))
                    Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                ProgressDispose();
                pnlMain.Enabled = true;
                tbMain.Text = sContent;
            }
        }

        string msFileName;
        private void buttonX2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open a file to perform a Regex replace";
            ofd.ShowDialog();
            if (string.IsNullOrEmpty(ofd.FileName))
                return;

            Text = "Math Regex - " + (msFileName = ofd.FileName);

            using (StreamReader sr = new StreamReader(msFileName))
                tbMain.Text = sr.ReadToEnd();
        }

        private void progressBarX1_Click(object sender, EventArgs e) { mbGo = false; }
        private void ckTopmost_CheckedChanged(object sender, EventArgs e) { TopMost = ckTopmost.Checked; }

        bool mbPrevTopmost = false;
        bool mbMaximized = false;
        private void Form_Main_LocationChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            { mbPrevTopmost = TopMost; TopMost = ckTopmost.Checked = false; mbMaximized = true; }
            else if (this.WindowState == FormWindowState.Normal)
            { Program.mrFormMainBounds = Bounds; if (mbMaximized) { TopMost = ckTopmost.Checked = mbPrevTopmost; mbPrevTopmost = false; mbMaximized = false; } }
        }

        private void Form_Main_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
                Program.mrFormMainBounds = Bounds;
        }

        Rectangle origEditRect = Rectangle.Empty;
        Font origEditFont = new System.Drawing .Font("Microsoft Sans Serif" , 8.25F, FontStyle .Regular, GraphicsUnit.Point, 0);
        private void tbReplace_Leave(object sender, EventArgs e)
        {
            if (origEditRect.IsEmpty)
                return;

            ((DevComponents.DotNetBar.Controls.TextBoxX)sender).Bounds = origEditRect;
            ((DevComponents.DotNetBar.Controls.TextBoxX)sender).Font = origEditFont;
            origEditRect = Rectangle.Empty;
        }

        private void expandablePanel1_ExpandedChanged(object sender, DevComponents.DotNetBar.ExpandedChangeEventArgs e)
        { Program.mbFuncsExpanded = expandFunctions.Expanded; }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (!origEditRect.IsEmpty)
                return;

            DevComponents.DotNetBar.Controls.TextBoxX tb = null;
            switch ((string)((DevComponents.DotNetBar.ButtonX)sender).Tag)
            {
                case "Search": tb = tbSearchRegex; break;
                case "Math": tb = tbMath; break;
                case "Replace": tb = tbReplace; break;
                default:
                    return;
            }
            origEditRect = tb.Bounds;
            origEditFont = tb.Font;
            tb.Focus();
            tb.Bounds = lblTBeditBounds.Bounds;
            tb.Font = new Font("Lucida Console", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tb.BringToFront();
        }
    }
}