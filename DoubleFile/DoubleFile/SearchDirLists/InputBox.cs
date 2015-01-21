using System;
using System.Windows.Forms;

namespace SearchDirLists
{
    public partial class InputBox : Form
    {
        public string Prompt { set { form_lbl_Prompt.Text = value; } }
        public string Entry { get { return form_cb_Entry.Text; } set { form_cb_Entry.Text = value; } }

        public InputBox()
        {
            InitializeComponent();
        }

        public void AddSelector(string str)
        {
            form_cb_Entry.Items.Add(str);
            form_cb_Entry.SelectedIndex = 0;
        }

        private void InputBox_Load(object sender, EventArgs e)
        {
            //if ((form_cb_Entry.Text.Length <= 0) && (form_cb_Entry.Items.Count > 0))
            //{
            //    form_cb_Entry.SelectedIndex = 0;
            //}
        }

        public void SetNextButtons()
        {
            form_btn_OK.Text = "Next";
            form_btn_Cancel.Text = "Skip";
        }
    }
}
