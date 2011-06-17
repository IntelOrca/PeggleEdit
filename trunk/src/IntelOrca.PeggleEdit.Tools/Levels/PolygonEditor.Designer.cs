namespace IntelOrca.PeggleEdit.Tools.Levels
{
	partial class PolygonEditor
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
			if (disposing && (components != null)) {
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
			this.lstPoints = new System.Windows.Forms.ListBox();
			this.txtX = new System.Windows.Forms.TextBox();
			this.txtY = new System.Windows.Forms.TextBox();
			this.pnlPointEditor = new System.Windows.Forms.Panel();
			this.btnFinish = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnReset = new System.Windows.Forms.Button();
			this.lblYouMust = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lstPoints
			// 
			this.lstPoints.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.lstPoints.FormattingEnabled = true;
			this.lstPoints.IntegralHeight = false;
			this.lstPoints.Location = new System.Drawing.Point(256, 55);
			this.lstPoints.Name = "lstPoints";
			this.lstPoints.Size = new System.Drawing.Size(118, 269);
			this.lstPoints.TabIndex = 0;
			this.lstPoints.SelectedIndexChanged += new System.EventHandler(this.lstPoints_SelectedIndexChanged);
			// 
			// txtX
			// 
			this.txtX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txtX.Location = new System.Drawing.Point(256, 29);
			this.txtX.Name = "txtX";
			this.txtX.Size = new System.Drawing.Size(47, 20);
			this.txtX.TabIndex = 1;
			this.txtX.TextChanged += new System.EventHandler(this.txtX_TextChanged);
			// 
			// txtY
			// 
			this.txtY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txtY.Location = new System.Drawing.Point(326, 29);
			this.txtY.Name = "txtY";
			this.txtY.Size = new System.Drawing.Size(48, 20);
			this.txtY.TabIndex = 2;
			this.txtY.TextChanged += new System.EventHandler(this.txtY_TextChanged);
			// 
			// pnlPointEditor
			// 
			this.pnlPointEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.pnlPointEditor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlPointEditor.Location = new System.Drawing.Point(12, 29);
			this.pnlPointEditor.Name = "pnlPointEditor";
			this.pnlPointEditor.Size = new System.Drawing.Size(238, 295);
			this.pnlPointEditor.TabIndex = 3;
			this.pnlPointEditor.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlPointEditor_Paint);
			this.pnlPointEditor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlPointEditor_MouseDown);
			this.pnlPointEditor.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnlPointEditor_MouseUp);
			// 
			// btnFinish
			// 
			this.btnFinish.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnFinish.Location = new System.Drawing.Point(299, 330);
			this.btnFinish.Name = "btnFinish";
			this.btnFinish.Size = new System.Drawing.Size(75, 23);
			this.btnFinish.TabIndex = 4;
			this.btnFinish.Text = "Finish";
			this.btnFinish.UseVisualStyleBackColor = true;
			this.btnFinish.Click += new System.EventHandler(this.btnFinish_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.Location = new System.Drawing.Point(218, 330);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 5;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnReset
			// 
			this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnReset.Location = new System.Drawing.Point(12, 330);
			this.btnReset.Name = "btnReset";
			this.btnReset.Size = new System.Drawing.Size(75, 23);
			this.btnReset.TabIndex = 6;
			this.btnReset.Text = "Reset";
			this.btnReset.UseVisualStyleBackColor = true;
			this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
			// 
			// lblYouMust
			// 
			this.lblYouMust.AutoSize = true;
			this.lblYouMust.Location = new System.Drawing.Point(12, 9);
			this.lblYouMust.Name = "lblYouMust";
			this.lblYouMust.Size = new System.Drawing.Size(277, 13);
			this.lblYouMust.TabIndex = 8;
			this.lblYouMust.Text = "You must draw the polygon in an anti-clockwise direction!";
			// 
			// PolygonEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(386, 365);
			this.Controls.Add(this.lblYouMust);
			this.Controls.Add(this.btnReset);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnFinish);
			this.Controls.Add(this.pnlPointEditor);
			this.Controls.Add(this.txtY);
			this.Controls.Add(this.txtX);
			this.Controls.Add(this.lstPoints);
			this.DoubleBuffered = true;
			this.Name = "PolygonEditor";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Polygon Editor";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox lstPoints;
		private System.Windows.Forms.TextBox txtX;
		private System.Windows.Forms.TextBox txtY;
		private System.Windows.Forms.Panel pnlPointEditor;
		private System.Windows.Forms.Button btnFinish;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnReset;
		private System.Windows.Forms.Label lblYouMust;
	}
}