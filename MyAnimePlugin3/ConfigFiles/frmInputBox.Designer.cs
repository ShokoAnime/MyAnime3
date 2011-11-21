namespace MyAnimePlugin3.ConfigFiles
{
	partial class frmInputBox
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
			this.txtOutput = new System.Windows.Forms.TextBox();
			this.txtPrompt = new System.Windows.Forms.Label();
			this.btnOK = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// txtOutput
			// 
			this.txtOutput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtOutput.Location = new System.Drawing.Point(15, 34);
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.Size = new System.Drawing.Size(317, 20);
			this.txtOutput.TabIndex = 42;
			// 
			// txtPrompt
			// 
			this.txtPrompt.AutoSize = true;
			this.txtPrompt.Location = new System.Drawing.Point(12, 9);
			this.txtPrompt.Name = "txtPrompt";
			this.txtPrompt.Size = new System.Drawing.Size(67, 13);
			this.txtPrompt.TabIndex = 41;
			this.txtPrompt.Text = "Input Prompt";
			// 
			// btnOK
			// 
			this.btnOK.Location = new System.Drawing.Point(257, 60);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 43;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			// 
			// frmInputBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(353, 100);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.txtOutput);
			this.Controls.Add(this.txtPrompt);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "frmInputBox";
			this.Text = "frmInputBox";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtOutput;
		private System.Windows.Forms.Label txtPrompt;
		private System.Windows.Forms.Button btnOK;
	}
}