namespace RegexReplace
{
    partial class Form_Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Main));
            this.lblSearch = new DevComponents.DotNetBar.LabelX();
            this.lblMath = new DevComponents.DotNetBar.LabelX();
            this.tbSearchRegex = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.tbMath = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.lblExplanation = new DevComponents.DotNetBar.LabelX();
            this.lblReplace = new DevComponents.DotNetBar.LabelX();
            this.tbReplace = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.btnReplace = new DevComponents.DotNetBar.ButtonX();
            this.ckCompiled = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.ckCultureInvariant = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.ckECMAScript = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.ckExplicitCapture = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.ckIgnoreCase = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.ckIgnorePatternWhitespace = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.ckMultiline = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.ckRightToLeft = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.ckSingleLine = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.btnBrowse = new DevComponents.DotNetBar.ButtonX();
            this.lblFormat = new DevComponents.DotNetBar.LabelX();
            this.tbFormat = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.lblFormat1 = new DevComponents.DotNetBar.LabelX();
            this.pnlMain = new DevComponents.DotNetBar.PanelEx();
            this.labelX3 = new DevComponents.DotNetBar.LabelX();
            this.labelX2 = new DevComponents.DotNetBar.LabelX();
            this.labelX1 = new DevComponents.DotNetBar.LabelX();
            this.lblTBeditBounds = new System.Windows.Forms.Label();
            this.btnEditReplace = new DevComponents.DotNetBar.ButtonX();
            this.btnEditMath = new DevComponents.DotNetBar.ButtonX();
            this.btnEditSearch = new DevComponents.DotNetBar.ButtonX();
            this.tbMain = new System.Windows.Forms.TextBox();
            this.lblProgCurr = new DevComponents.DotNetBar.LabelX();
            this.lblProgMax = new DevComponents.DotNetBar.LabelX();
            this.expandFunctions = new DevComponents.DotNetBar.ExpandablePanel();
            this.tbFunctions = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.groupPanel2 = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.tbHandy = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.groupPanel1 = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.tbExtra = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.progressBarX1 = new DevComponents.DotNetBar.Controls.ProgressBarX();
            this.btnCancel = new DevComponents.DotNetBar.ButtonX();
            this.panelEx2 = new DevComponents.DotNetBar.PanelEx();
            this.lblC = new DevComponents.DotNetBar.LabelX();
            this.ckTopmost = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.pnlMain.SuspendLayout();
            this.expandFunctions.SuspendLayout();
            this.groupPanel2.SuspendLayout();
            this.groupPanel1.SuspendLayout();
            this.panelEx2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(6, 9);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(72, 13);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search Regex";
            // 
            // lblMath
            // 
            this.lblMath.AutoSize = true;
            this.lblMath.Location = new System.Drawing.Point(365, 9);
            this.lblMath.Name = "lblMath";
            this.lblMath.Size = new System.Drawing.Size(40, 13);
            this.lblMath.TabIndex = 0;
            this.lblMath.Text = "${Math}";
            // 
            // tbSearchRegex
            // 
            // 
            // 
            // 
            this.tbSearchRegex.Border.Class = "TextBoxBorder";
            this.tbSearchRegex.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbSearchRegex.Location = new System.Drawing.Point(84, 7);
            this.tbSearchRegex.Multiline = true;
            this.tbSearchRegex.Name = "tbSearchRegex";
            this.tbSearchRegex.Size = new System.Drawing.Size(244, 20);
            this.tbSearchRegex.TabIndex = 1;
            this.tbSearchRegex.Text = "(?<A>0\\.\\d+)";
            this.tbSearchRegex.Leave += new System.EventHandler(this.tbReplace_Leave);
            // 
            // tbMath
            // 
            // 
            // 
            // 
            this.tbMath.Border.Class = "TextBoxBorder";
            this.tbMath.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbMath.Location = new System.Drawing.Point(464, 7);
            this.tbMath.Multiline = true;
            this.tbMath.Name = "tbMath";
            this.tbMath.Size = new System.Drawing.Size(245, 20);
            this.tbMath.TabIndex = 3;
            this.tbMath.Text = "sqrt( ${A} * .98 )";
            this.tbMath.Leave += new System.EventHandler(this.tbReplace_Leave);
            // 
            // lblExplanation
            // 
            this.lblExplanation.AutoSize = true;
            this.lblExplanation.Location = new System.Drawing.Point(365, 85);
            this.lblExplanation.Name = "lblExplanation";
            this.lblExplanation.Size = new System.Drawing.Size(303, 38);
            this.lblExplanation.TabIndex = 2;
            this.lblExplanation.Text = "Apply math to numbers found using regex. Group the number,\r\ne.g. (?<A>0\\.\\d+), an" +
                "d then specify the math,\r\ne.g. ${A} * .98. The result will be stuffed into ${Mat" +
                "h}.";
            this.lblExplanation.WordWrap = true;
            // 
            // lblReplace
            // 
            this.lblReplace.AutoSize = true;
            this.lblReplace.Location = new System.Drawing.Point(365, 61);
            this.lblReplace.Name = "lblReplace";
            this.lblReplace.Size = new System.Drawing.Size(78, 13);
            this.lblReplace.TabIndex = 0;
            this.lblReplace.Text = "Replace Regex";
            // 
            // tbReplace
            // 
            // 
            // 
            // 
            this.tbReplace.Border.Class = "TextBoxBorder";
            this.tbReplace.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbReplace.Location = new System.Drawing.Point(464, 59);
            this.tbReplace.Multiline = true;
            this.tbReplace.Name = "tbReplace";
            this.tbReplace.Size = new System.Drawing.Size(245, 20);
            this.tbReplace.TabIndex = 7;
            this.tbReplace.Text = "${A} is the original. ${Math} is the result.";
            this.tbReplace.Leave += new System.EventHandler(this.tbReplace_Leave);
            // 
            // btnReplace
            // 
            this.btnReplace.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnReplace.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnReplace.Location = new System.Drawing.Point(267, 100);
            this.btnReplace.Name = "btnReplace";
            this.btnReplace.Size = new System.Drawing.Size(75, 23);
            this.btnReplace.TabIndex = 10;
            this.btnReplace.Text = "Replace";
            this.btnReplace.Click += new System.EventHandler(this.buttonX1_Click);
            // 
            // ckCompiled
            // 
            this.ckCompiled.AutoSize = true;
            this.ckCompiled.Location = new System.Drawing.Point(180, 33);
            this.ckCompiled.Name = "ckCompiled";
            this.ckCompiled.Size = new System.Drawing.Size(68, 15);
            this.ckCompiled.TabIndex = 13;
            this.ckCompiled.Text = "Compiled";
            this.ckCompiled.TextColor = System.Drawing.Color.Blue;
            // 
            // ckCultureInvariant
            // 
            this.ckCultureInvariant.AutoSize = true;
            this.ckCultureInvariant.Location = new System.Drawing.Point(6, 54);
            this.ckCultureInvariant.Name = "ckCultureInvariant";
            this.ckCultureInvariant.Size = new System.Drawing.Size(102, 15);
            this.ckCultureInvariant.TabIndex = 15;
            this.ckCultureInvariant.Text = "Culture Invariant";
            // 
            // ckECMAScript
            // 
            this.ckECMAScript.AutoSize = true;
            this.ckECMAScript.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ckECMAScript.Location = new System.Drawing.Point(6, 33);
            this.ckECMAScript.Name = "ckECMAScript";
            this.ckECMAScript.Size = new System.Drawing.Size(85, 15);
            this.ckECMAScript.TabIndex = 11;
            this.ckECMAScript.Text = "ECMAScript:";
            this.ckECMAScript.TextColor = System.Drawing.Color.Blue;
            // 
            // ckExplicitCapture
            // 
            this.ckExplicitCapture.AutoSize = true;
            this.ckExplicitCapture.Location = new System.Drawing.Point(180, 75);
            this.ckExplicitCapture.Name = "ckExplicitCapture";
            this.ckExplicitCapture.Size = new System.Drawing.Size(99, 15);
            this.ckExplicitCapture.TabIndex = 19;
            this.ckExplicitCapture.Text = "Explicit Capture";
            // 
            // ckIgnoreCase
            // 
            this.ckIgnoreCase.AutoSize = true;
            this.ckIgnoreCase.Location = new System.Drawing.Point(93, 33);
            this.ckIgnoreCase.Name = "ckIgnoreCase";
            this.ckIgnoreCase.Size = new System.Drawing.Size(81, 15);
            this.ckIgnoreCase.TabIndex = 12;
            this.ckIgnoreCase.Text = "Ignore Case";
            this.ckIgnoreCase.TextColor = System.Drawing.Color.Blue;
            // 
            // ckIgnorePatternWhitespace
            // 
            this.ckIgnorePatternWhitespace.AutoSize = true;
            this.ckIgnorePatternWhitespace.Location = new System.Drawing.Point(6, 75);
            this.ckIgnorePatternWhitespace.Name = "ckIgnorePatternWhitespace";
            this.ckIgnorePatternWhitespace.Size = new System.Drawing.Size(152, 15);
            this.ckIgnorePatternWhitespace.TabIndex = 18;
            this.ckIgnorePatternWhitespace.Text = "Ignore Pattern Whitespace";
            // 
            // ckMultiline
            // 
            this.ckMultiline.AutoSize = true;
            this.ckMultiline.Location = new System.Drawing.Point(264, 33);
            this.ckMultiline.Name = "ckMultiline";
            this.ckMultiline.Size = new System.Drawing.Size(62, 15);
            this.ckMultiline.TabIndex = 14;
            this.ckMultiline.Text = "Multiline";
            this.ckMultiline.TextColor = System.Drawing.Color.Blue;
            // 
            // ckRightToLeft
            // 
            this.ckRightToLeft.AutoSize = true;
            this.ckRightToLeft.Location = new System.Drawing.Point(264, 54);
            this.ckRightToLeft.Name = "ckRightToLeft";
            this.ckRightToLeft.Size = new System.Drawing.Size(78, 15);
            this.ckRightToLeft.TabIndex = 17;
            this.ckRightToLeft.Text = "RightToLeft";
            // 
            // ckSingleLine
            // 
            this.ckSingleLine.AutoSize = true;
            this.ckSingleLine.Location = new System.Drawing.Point(180, 54);
            this.ckSingleLine.Name = "ckSingleLine";
            this.ckSingleLine.Size = new System.Drawing.Size(76, 15);
            this.ckSingleLine.TabIndex = 16;
            this.ckSingleLine.Text = "Single Line";
            // 
            // btnBrowse
            // 
            this.btnBrowse.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnBrowse.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnBrowse.Location = new System.Drawing.Point(6, 100);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 9;
            this.btnBrowse.Text = "Choose File";
            this.btnBrowse.Click += new System.EventHandler(this.buttonX2_Click);
            // 
            // lblFormat
            // 
            this.lblFormat.AutoSize = true;
            this.lblFormat.Location = new System.Drawing.Point(365, 35);
            this.lblFormat.Name = "lblFormat";
            this.lblFormat.Size = new System.Drawing.Size(100, 13);
            this.lblFormat.TabIndex = 0;
            this.lblFormat.Text = "(${Math}).ToString(\"";
            // 
            // tbFormat
            // 
            // 
            // 
            // 
            this.tbFormat.Border.Class = "TextBoxBorder";
            this.tbFormat.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbFormat.Location = new System.Drawing.Point(464, 33);
            this.tbFormat.Name = "tbFormat";
            this.tbFormat.Size = new System.Drawing.Size(112, 20);
            this.tbFormat.TabIndex = 5;
            this.tbFormat.Text = "0.00000";
            // 
            // lblFormat1
            // 
            this.lblFormat1.AutoSize = true;
            this.lblFormat1.Location = new System.Drawing.Point(577, 35);
            this.lblFormat1.Name = "lblFormat1";
            this.lblFormat1.Size = new System.Drawing.Size(10, 13);
            this.lblFormat1.TabIndex = 6;
            this.lblFormat1.Text = "\")";
            // 
            // pnlMain
            // 
            this.pnlMain.CanvasColor = System.Drawing.SystemColors.Control;
            this.pnlMain.Controls.Add(this.tbSearchRegex);
            this.pnlMain.Controls.Add(this.tbMath);
            this.pnlMain.Controls.Add(this.tbReplace);
            this.pnlMain.Controls.Add(this.labelX3);
            this.pnlMain.Controls.Add(this.labelX2);
            this.pnlMain.Controls.Add(this.labelX1);
            this.pnlMain.Controls.Add(this.lblTBeditBounds);
            this.pnlMain.Controls.Add(this.btnEditReplace);
            this.pnlMain.Controls.Add(this.btnEditMath);
            this.pnlMain.Controls.Add(this.btnEditSearch);
            this.pnlMain.Controls.Add(this.tbMain);
            this.pnlMain.Controls.Add(this.lblSearch);
            this.pnlMain.Controls.Add(this.lblMath);
            this.pnlMain.Controls.Add(this.ckSingleLine);
            this.pnlMain.Controls.Add(this.lblReplace);
            this.pnlMain.Controls.Add(this.ckRightToLeft);
            this.pnlMain.Controls.Add(this.ckMultiline);
            this.pnlMain.Controls.Add(this.lblFormat);
            this.pnlMain.Controls.Add(this.ckIgnorePatternWhitespace);
            this.pnlMain.Controls.Add(this.lblFormat1);
            this.pnlMain.Controls.Add(this.ckIgnoreCase);
            this.pnlMain.Controls.Add(this.ckExplicitCapture);
            this.pnlMain.Controls.Add(this.tbFormat);
            this.pnlMain.Controls.Add(this.ckECMAScript);
            this.pnlMain.Controls.Add(this.lblExplanation);
            this.pnlMain.Controls.Add(this.ckCultureInvariant);
            this.pnlMain.Controls.Add(this.btnReplace);
            this.pnlMain.Controls.Add(this.ckCompiled);
            this.pnlMain.Controls.Add(this.btnBrowse);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(740, 276);
            this.pnlMain.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.pnlMain.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.pnlMain.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.pnlMain.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.pnlMain.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.pnlMain.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.pnlMain.Style.GradientAngle = 90;
            this.pnlMain.TabIndex = 0;
            // 
            // labelX3
            // 
            this.labelX3.AutoSize = true;
            this.labelX3.ForeColor = System.Drawing.Color.Maroon;
            this.labelX3.Location = new System.Drawing.Point(464, 61);
            this.labelX3.Name = "labelX3";
            this.labelX3.Size = new System.Drawing.Size(158, 13);
            this.labelX3.TabIndex = 8;
            this.labelX3.Text = "Editing below. Tab to finish edit.";
            // 
            // labelX2
            // 
            this.labelX2.AutoSize = true;
            this.labelX2.ForeColor = System.Drawing.Color.Maroon;
            this.labelX2.Location = new System.Drawing.Point(464, 9);
            this.labelX2.Name = "labelX2";
            this.labelX2.Size = new System.Drawing.Size(158, 13);
            this.labelX2.TabIndex = 8;
            this.labelX2.Text = "Editing below. Tab to finish edit.";
            // 
            // labelX1
            // 
            this.labelX1.AutoSize = true;
            this.labelX1.ForeColor = System.Drawing.Color.Maroon;
            this.labelX1.Location = new System.Drawing.Point(84, 9);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(158, 13);
            this.labelX1.TabIndex = 8;
            this.labelX1.Text = "Editing below. Tab to finish edit.";
            // 
            // lblTBeditBounds
            // 
            this.lblTBeditBounds.Location = new System.Drawing.Point(12, 139);
            this.lblTBeditBounds.Name = "lblTBeditBounds";
            this.lblTBeditBounds.Size = new System.Drawing.Size(700, 98);
            this.lblTBeditBounds.TabIndex = 7;
            this.lblTBeditBounds.Visible = false;
            // 
            // btnEditReplace
            // 
            this.btnEditReplace.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnEditReplace.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnEditReplace.Location = new System.Drawing.Point(707, 59);
            this.btnEditReplace.Name = "btnEditReplace";
            this.btnEditReplace.Size = new System.Drawing.Size(15, 20);
            this.btnEditReplace.TabIndex = 8;
            this.btnEditReplace.Tag = "Replace";
            this.btnEditReplace.Text = "/";
            this.btnEditReplace.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnEditMath
            // 
            this.btnEditMath.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnEditMath.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnEditMath.Location = new System.Drawing.Point(707, 7);
            this.btnEditMath.Name = "btnEditMath";
            this.btnEditMath.Size = new System.Drawing.Size(15, 20);
            this.btnEditMath.TabIndex = 4;
            this.btnEditMath.Tag = "Math";
            this.btnEditMath.Text = "/";
            this.btnEditMath.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnEditSearch
            // 
            this.btnEditSearch.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnEditSearch.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnEditSearch.Location = new System.Drawing.Point(326, 7);
            this.btnEditSearch.Name = "btnEditSearch";
            this.btnEditSearch.Size = new System.Drawing.Size(15, 20);
            this.btnEditSearch.TabIndex = 2;
            this.btnEditSearch.Tag = "Search";
            this.btnEditSearch.Text = "/";
            this.btnEditSearch.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // tbMain
            // 
            this.tbMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbMain.BackColor = System.Drawing.Color.White;
            this.tbMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbMain.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbMain.Location = new System.Drawing.Point(3, 129);
            this.tbMain.Multiline = true;
            this.tbMain.Name = "tbMain";
            this.tbMain.ReadOnly = true;
            this.tbMain.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbMain.Size = new System.Drawing.Size(734, 133);
            this.tbMain.TabIndex = 99;
            this.tbMain.WordWrap = false;
            // 
            // lblProgCurr
            // 
            this.lblProgCurr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblProgCurr.BackColor = System.Drawing.Color.Transparent;
            this.lblProgCurr.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.lblProgCurr.Location = new System.Drawing.Point(0, 2);
            this.lblProgCurr.Name = "lblProgCurr";
            this.lblProgCurr.Size = new System.Drawing.Size(54, 10);
            this.lblProgCurr.TabIndex = 6;
            this.lblProgCurr.Text = "0";
            this.lblProgCurr.TextAlignment = System.Drawing.StringAlignment.Far;
            // 
            // lblProgMax
            // 
            this.lblProgMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgMax.BackColor = System.Drawing.Color.Transparent;
            this.lblProgMax.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.lblProgMax.Location = new System.Drawing.Point(686, 3);
            this.lblProgMax.Name = "lblProgMax";
            this.lblProgMax.Size = new System.Drawing.Size(54, 10);
            this.lblProgMax.TabIndex = 6;
            this.lblProgMax.Text = "0";
            // 
            // expandFunctions
            // 
            this.expandFunctions.CanvasColor = System.Drawing.SystemColors.Control;
            this.expandFunctions.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.expandFunctions.Controls.Add(this.tbFunctions);
            this.expandFunctions.Expanded = false;
            this.expandFunctions.ExpandedBounds = new System.Drawing.Rectangle(748, 100, 434, 279);
            this.expandFunctions.ExpandOnTitleClick = true;
            this.expandFunctions.Location = new System.Drawing.Point(748, 100);
            this.expandFunctions.Name = "expandFunctions";
            this.expandFunctions.Size = new System.Drawing.Size(434, 26);
            this.expandFunctions.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.expandFunctions.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.expandFunctions.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.expandFunctions.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.expandFunctions.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarDockedBorder;
            this.expandFunctions.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
            this.expandFunctions.Style.GradientAngle = 90;
            this.expandFunctions.TabIndex = 3;
            this.expandFunctions.TitleStyle.Alignment = System.Drawing.StringAlignment.Center;
            this.expandFunctions.TitleStyle.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.expandFunctions.TitleStyle.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.expandFunctions.TitleStyle.Border = DevComponents.DotNetBar.eBorderType.RaisedInner;
            this.expandFunctions.TitleStyle.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.expandFunctions.TitleStyle.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.expandFunctions.TitleStyle.GradientAngle = 90;
            this.expandFunctions.TitleText = "Math Functions";
            this.expandFunctions.ExpandedChanged += new DevComponents.DotNetBar.ExpandChangeEventHandler(this.expandablePanel1_ExpandedChanged);
            // 
            // tbFunctions
            // 
            this.tbFunctions.BackColor = System.Drawing.Color.Black;
            // 
            // 
            // 
            this.tbFunctions.Border.Class = "TextBoxBorder";
            this.tbFunctions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbFunctions.ForeColor = System.Drawing.Color.White;
            this.tbFunctions.Location = new System.Drawing.Point(0, 26);
            this.tbFunctions.Multiline = true;
            this.tbFunctions.Name = "tbFunctions";
            this.tbFunctions.ReadOnly = true;
            this.tbFunctions.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbFunctions.Size = new System.Drawing.Size(434, 0);
            this.tbFunctions.TabIndex = 9;
            this.tbFunctions.Text = resources.GetString("tbFunctions.Text");
            this.tbFunctions.WordWrap = false;
            // 
            // groupPanel2
            // 
            this.groupPanel2.CanvasColor = System.Drawing.SystemColors.Control;
            this.groupPanel2.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.groupPanel2.Controls.Add(this.tbHandy);
            this.groupPanel2.Location = new System.Drawing.Point(897, 7);
            this.groupPanel2.Name = "groupPanel2";
            this.groupPanel2.Size = new System.Drawing.Size(288, 87);
            // 
            // 
            // 
            this.groupPanel2.Style.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.groupPanel2.Style.BackColorGradientAngle = 90;
            this.groupPanel2.Style.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.groupPanel2.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel2.Style.BorderBottomWidth = 1;
            this.groupPanel2.Style.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.groupPanel2.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel2.Style.BorderLeftWidth = 1;
            this.groupPanel2.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel2.Style.BorderRightWidth = 1;
            this.groupPanel2.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel2.Style.BorderTopWidth = 1;
            this.groupPanel2.Style.CornerDiameter = 4;
            this.groupPanel2.Style.CornerType = DevComponents.DotNetBar.eCornerType.Rounded;
            this.groupPanel2.Style.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.groupPanel2.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.groupPanel2.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            this.groupPanel2.TabIndex = 2;
            this.groupPanel2.Text = "Handy";
            // 
            // tbHandy
            // 
            this.tbHandy.BackColor = System.Drawing.Color.Black;
            // 
            // 
            // 
            this.tbHandy.Border.Class = "TextBoxBorder";
            this.tbHandy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbHandy.ForeColor = System.Drawing.Color.White;
            this.tbHandy.Location = new System.Drawing.Point(0, 0);
            this.tbHandy.Multiline = true;
            this.tbHandy.Name = "tbHandy";
            this.tbHandy.ReadOnly = true;
            this.tbHandy.Size = new System.Drawing.Size(282, 66);
            this.tbHandy.TabIndex = 8;
            this.tbHandy.Text = "http://msdn2.microsoft.com/en-us/library/hs600312.aspx\r\nhttp://msdn2.microsoft.co" +
                "m/en-us/library/kfsatb94.aspx";
            // 
            // groupPanel1
            // 
            this.groupPanel1.CanvasColor = System.Drawing.SystemColors.Control;
            this.groupPanel1.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.groupPanel1.Controls.Add(this.tbExtra);
            this.groupPanel1.Location = new System.Drawing.Point(745, 7);
            this.groupPanel1.Name = "groupPanel1";
            this.groupPanel1.Size = new System.Drawing.Size(146, 87);
            // 
            // 
            // 
            this.groupPanel1.Style.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.groupPanel1.Style.BackColorGradientAngle = 90;
            this.groupPanel1.Style.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.groupPanel1.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel1.Style.BorderBottomWidth = 1;
            this.groupPanel1.Style.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.groupPanel1.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel1.Style.BorderLeftWidth = 1;
            this.groupPanel1.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel1.Style.BorderRightWidth = 1;
            this.groupPanel1.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel1.Style.BorderTopWidth = 1;
            this.groupPanel1.Style.CornerDiameter = 4;
            this.groupPanel1.Style.CornerType = DevComponents.DotNetBar.eCornerType.Rounded;
            this.groupPanel1.Style.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.groupPanel1.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.groupPanel1.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            this.groupPanel1.TabIndex = 1;
            this.groupPanel1.Text = "Extra";
            // 
            // tbExtra
            // 
            this.tbExtra.BackColor = System.Drawing.Color.Black;
            // 
            // 
            // 
            this.tbExtra.Border.Class = "TextBoxBorder";
            this.tbExtra.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbExtra.ForeColor = System.Drawing.Color.White;
            this.tbExtra.Location = new System.Drawing.Point(0, 0);
            this.tbExtra.Multiline = true;
            this.tbExtra.Name = "tbExtra";
            this.tbExtra.ReadOnly = true;
            this.tbExtra.Size = new System.Drawing.Size(140, 66);
            this.tbExtra.TabIndex = 8;
            this.tbExtra.Text = "${ix}   -   Index of Found";
            // 
            // progressBarX1
            // 
            this.progressBarX1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarX1.ChunkColor = System.Drawing.Color.Black;
            this.progressBarX1.ChunkColor2 = System.Drawing.Color.Black;
            this.progressBarX1.ColorTable = DevComponents.DotNetBar.eProgressBarItemColor.Paused;
            this.progressBarX1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.progressBarX1.Location = new System.Drawing.Point(60, 1);
            this.progressBarX1.Name = "progressBarX1";
            this.progressBarX1.Size = new System.Drawing.Size(620, 13);
            this.progressBarX1.Style = DevComponents.DotNetBar.eDotNetBarStyle.Office2003;
            this.progressBarX1.TabIndex = 14;
            // 
            // btnCancel
            // 
            this.btnCancel.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnCancel.Location = new System.Drawing.Point(348, 1);
            this.btnCancel.MaximumSize = new System.Drawing.Size(44, 13);
            this.btnCancel.MinimumSize = new System.Drawing.Size(44, 13);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(44, 13);
            this.btnCancel.Style = DevComponents.DotNetBar.eDotNetBarStyle.Office2003;
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.progressBarX1_Click);
            // 
            // panelEx2
            // 
            this.panelEx2.CanvasColor = System.Drawing.SystemColors.Control;
            this.panelEx2.Controls.Add(this.lblProgCurr);
            this.panelEx2.Controls.Add(this.btnCancel);
            this.panelEx2.Controls.Add(this.progressBarX1);
            this.panelEx2.Controls.Add(this.lblProgMax);
            this.panelEx2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelEx2.Location = new System.Drawing.Point(0, 261);
            this.panelEx2.Name = "panelEx2";
            this.panelEx2.Size = new System.Drawing.Size(740, 15);
            this.panelEx2.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.panelEx2.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.panelEx2.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.panelEx2.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.panelEx2.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.panelEx2.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.panelEx2.Style.GradientAngle = 90;
            this.panelEx2.TabIndex = 16;
            this.panelEx2.Visible = false;
            // 
            // lblC
            // 
            this.lblC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblC.BackColor = System.Drawing.Color.Transparent;
            this.lblC.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblC.Location = new System.Drawing.Point(-9329, 262);
            this.lblC.Name = "lblC";
            this.lblC.Size = new System.Drawing.Size(10069, 15);
            this.lblC.TabIndex = 17;
            this.lblC.Text = "MathRegex 1.7 © 2008 Tim Richards";
            this.lblC.TextAlignment = System.Drawing.StringAlignment.Far;
            // 
            // ckTopmost
            // 
            this.ckTopmost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ckTopmost.AutoSize = true;
            this.ckTopmost.Location = new System.Drawing.Point(0, 262);
            this.ckTopmost.Name = "ckTopmost";
            this.ckTopmost.Size = new System.Drawing.Size(64, 15);
            this.ckTopmost.TabIndex = 6;
            this.ckTopmost.Text = "Topmost";
            this.ckTopmost.CheckedChanged += new System.EventHandler(this.ckTopmost_CheckedChanged);
            // 
            // Form_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(740, 276);
            this.Controls.Add(this.panelEx2);
            this.Controls.Add(this.groupPanel1);
            this.Controls.Add(this.groupPanel2);
            this.Controls.Add(this.expandFunctions);
            this.Controls.Add(this.ckTopmost);
            this.Controls.Add(this.lblC);
            this.Controls.Add(this.pnlMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(748, 300);
            this.Name = "Form_Main";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Math Regex";
            this.Load += new System.EventHandler(this.Form_Main_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_Main_FormClosed);
            this.Resize += new System.EventHandler(this.Form_Main_Resize);
            this.LocationChanged += new System.EventHandler(this.Form_Main_LocationChanged);
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            this.expandFunctions.ResumeLayout(false);
            this.groupPanel2.ResumeLayout(false);
            this.groupPanel1.ResumeLayout(false);
            this.panelEx2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevComponents.DotNetBar.LabelX lblSearch;
        private DevComponents.DotNetBar.LabelX lblMath;
        private DevComponents.DotNetBar.Controls.TextBoxX tbSearchRegex;
        private DevComponents.DotNetBar.Controls.TextBoxX tbMath;
        private DevComponents.DotNetBar.LabelX lblExplanation;
        private DevComponents.DotNetBar.LabelX lblReplace;
        private DevComponents.DotNetBar.Controls.TextBoxX tbReplace;
        private DevComponents.DotNetBar.ButtonX btnReplace;
        private DevComponents.DotNetBar.Controls.CheckBoxX ckCompiled;
        private DevComponents.DotNetBar.Controls.CheckBoxX ckCultureInvariant;
        private DevComponents.DotNetBar.Controls.CheckBoxX ckECMAScript;
        private DevComponents.DotNetBar.Controls.CheckBoxX ckExplicitCapture;
        private DevComponents.DotNetBar.Controls.CheckBoxX ckIgnoreCase;
        private DevComponents.DotNetBar.Controls.CheckBoxX ckIgnorePatternWhitespace;
        private DevComponents.DotNetBar.Controls.CheckBoxX ckMultiline;
        private DevComponents.DotNetBar.Controls.CheckBoxX ckRightToLeft;
        private DevComponents.DotNetBar.Controls.CheckBoxX ckSingleLine;
        private DevComponents.DotNetBar.ButtonX btnBrowse;
        private DevComponents.DotNetBar.LabelX lblFormat;
        private DevComponents.DotNetBar.Controls.TextBoxX tbFormat;
        private DevComponents.DotNetBar.LabelX lblFormat1;
        private DevComponents.DotNetBar.PanelEx pnlMain;
        private DevComponents.DotNetBar.LabelX lblProgMax;
        private DevComponents.DotNetBar.LabelX lblProgCurr;
        private DevComponents.DotNetBar.ExpandablePanel expandFunctions;
        private DevComponents.DotNetBar.Controls.TextBoxX tbFunctions;
        private DevComponents.DotNetBar.Controls.GroupPanel groupPanel2;
        private DevComponents.DotNetBar.Controls.TextBoxX tbHandy;
        private DevComponents.DotNetBar.Controls.GroupPanel groupPanel1;
        private DevComponents.DotNetBar.Controls.TextBoxX tbExtra;
        private DevComponents.DotNetBar.Controls.ProgressBarX progressBarX1;
        private DevComponents.DotNetBar.ButtonX btnCancel;
        private DevComponents.DotNetBar.PanelEx panelEx2;
        private DevComponents.DotNetBar.LabelX lblC;
        private DevComponents.DotNetBar.Controls.CheckBoxX ckTopmost;
        private System.Windows.Forms.TextBox tbMain;
        private DevComponents.DotNetBar.ButtonX btnEditSearch;
        private DevComponents.DotNetBar.ButtonX btnEditReplace;
        private DevComponents.DotNetBar.ButtonX btnEditMath;
        private System.Windows.Forms.Label lblTBeditBounds;
        private DevComponents.DotNetBar.LabelX labelX1;
        private DevComponents.DotNetBar.LabelX labelX3;
        private DevComponents.DotNetBar.LabelX labelX2;
    }
}