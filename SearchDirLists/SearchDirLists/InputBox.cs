using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SearchDirLists
{
    public partial class InputBox : Form
    {
        public String Prompt { set { form_lbl_Prompt.Text = value; } }
        public String Entry { get { return form_cb_Entry.Text; } set { form_cb_Entry.Text = value; } }

        public InputBox()
        {
            InitializeComponent();
        }

        public void AddSelector(String str)
        {
            form_cb_Entry.Items.Add(str);
            form_cb_Entry.SelectedIndex = 0;
        }

        private void InputBox_Load(object sender, EventArgs e)
        {
            //if ((form_cb_Entry.Text.Length == 0) && (form_cb_Entry.Items.Count > 0))
            //{
            //    form_cb_Entry.SelectedIndex = 0;
            //}
        }
    }
}
