using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyAnimePlugin3.ConfigFiles
{
	public partial class frmInputBox : Form
	{
		public string Output { get; set; }

		public frmInputBox()
		{
			InitializeComponent();

			btnOK.Click += new EventHandler(btnOK_Click);
		}

		void btnOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
			Output = this.txtOutput.Text.Trim();
		}

		public void Init(string title, string prompt, string defaultText)
		{
			this.Text = title;
			this.txtPrompt.Text = prompt;
			this.txtOutput.Text = defaultText;
		}
	}
}
