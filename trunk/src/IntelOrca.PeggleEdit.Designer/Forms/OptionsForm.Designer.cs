namespace IntelOrca.PeggleEdit.Designer
{
	partial class OptionsForm
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
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabDesigner = new System.Windows.Forms.TabPage();
			this.grpMovement = new System.Windows.Forms.GroupBox();
			this.chkAlwaysShowAnchors = new System.Windows.Forms.CheckBox();
			this.grpGridSnapping = new System.Windows.Forms.GroupBox();
			this.nudSnapThreshold = new System.Windows.Forms.NumericUpDown();
			this.lblSnapThreshold = new System.Windows.Forms.Label();
			this.lblGridSize = new System.Windows.Forms.Label();
			this.nudGridSize = new System.Windows.Forms.NumericUpDown();
			this.chkSnapToGrid = new System.Windows.Forms.CheckBox();
			this.chkShowGrid = new System.Windows.Forms.CheckBox();
			this.tabPeggleNights = new System.Windows.Forms.TabPage();
			this.grpTesting = new System.Windows.Forms.GroupBox();
			this.lblPeggleNightsExePath = new System.Windows.Forms.Label();
			this.txtPeggleNightsExePath = new System.Windows.Forms.TextBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.tabApplication = new System.Windows.Forms.TabPage();
			this.btnSetFileAssociation = new System.Windows.Forms.Button();
			this.tabControl.SuspendLayout();
			this.tabDesigner.SuspendLayout();
			this.grpMovement.SuspendLayout();
			this.grpGridSnapping.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudSnapThreshold)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudGridSize)).BeginInit();
			this.tabPeggleNights.SuspendLayout();
			this.grpTesting.SuspendLayout();
			this.tabApplication.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl.Controls.Add(this.tabDesigner);
			this.tabControl.Controls.Add(this.tabPeggleNights);
			this.tabControl.Controls.Add(this.tabApplication);
			this.tabControl.Location = new System.Drawing.Point(12, 12);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(329, 208);
			this.tabControl.TabIndex = 0;
			// 
			// tabDesigner
			// 
			this.tabDesigner.Controls.Add(this.grpMovement);
			this.tabDesigner.Controls.Add(this.grpGridSnapping);
			this.tabDesigner.Location = new System.Drawing.Point(4, 22);
			this.tabDesigner.Name = "tabDesigner";
			this.tabDesigner.Padding = new System.Windows.Forms.Padding(3);
			this.tabDesigner.Size = new System.Drawing.Size(321, 182);
			this.tabDesigner.TabIndex = 0;
			this.tabDesigner.Text = "Designer";
			this.tabDesigner.UseVisualStyleBackColor = true;
			// 
			// grpMovement
			// 
			this.grpMovement.Controls.Add(this.chkAlwaysShowAnchors);
			this.grpMovement.Location = new System.Drawing.Point(6, 130);
			this.grpMovement.Name = "grpMovement";
			this.grpMovement.Size = new System.Drawing.Size(309, 45);
			this.grpMovement.TabIndex = 1;
			this.grpMovement.TabStop = false;
			this.grpMovement.Text = "Movement";
			// 
			// chkAlwaysShowAnchors
			// 
			this.chkAlwaysShowAnchors.AutoSize = true;
			this.chkAlwaysShowAnchors.Location = new System.Drawing.Point(6, 19);
			this.chkAlwaysShowAnchors.Name = "chkAlwaysShowAnchors";
			this.chkAlwaysShowAnchors.Size = new System.Drawing.Size(128, 17);
			this.chkAlwaysShowAnchors.TabIndex = 0;
			this.chkAlwaysShowAnchors.Text = "Always show anchors";
			this.chkAlwaysShowAnchors.UseVisualStyleBackColor = true;
			// 
			// grpGridSnapping
			// 
			this.grpGridSnapping.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.grpGridSnapping.Controls.Add(this.nudSnapThreshold);
			this.grpGridSnapping.Controls.Add(this.lblSnapThreshold);
			this.grpGridSnapping.Controls.Add(this.lblGridSize);
			this.grpGridSnapping.Controls.Add(this.nudGridSize);
			this.grpGridSnapping.Controls.Add(this.chkSnapToGrid);
			this.grpGridSnapping.Controls.Add(this.chkShowGrid);
			this.grpGridSnapping.Location = new System.Drawing.Point(6, 6);
			this.grpGridSnapping.Name = "grpGridSnapping";
			this.grpGridSnapping.Size = new System.Drawing.Size(309, 118);
			this.grpGridSnapping.TabIndex = 0;
			this.grpGridSnapping.TabStop = false;
			this.grpGridSnapping.Text = "Grid / snapping";
			// 
			// nudSnapThreshold
			// 
			this.nudSnapThreshold.Location = new System.Drawing.Point(95, 93);
			this.nudSnapThreshold.Name = "nudSnapThreshold";
			this.nudSnapThreshold.Size = new System.Drawing.Size(83, 20);
			this.nudSnapThreshold.TabIndex = 5;
			// 
			// lblSnapThreshold
			// 
			this.lblSnapThreshold.AutoSize = true;
			this.lblSnapThreshold.Location = new System.Drawing.Point(92, 77);
			this.lblSnapThreshold.Name = "lblSnapThreshold";
			this.lblSnapThreshold.Size = new System.Drawing.Size(81, 13);
			this.lblSnapThreshold.TabIndex = 4;
			this.lblSnapThreshold.Text = "Snap threshold:";
			// 
			// lblGridSize
			// 
			this.lblGridSize.AutoSize = true;
			this.lblGridSize.Location = new System.Drawing.Point(3, 77);
			this.lblGridSize.Name = "lblGridSize";
			this.lblGridSize.Size = new System.Drawing.Size(50, 13);
			this.lblGridSize.TabIndex = 3;
			this.lblGridSize.Text = "Grid size:";
			// 
			// nudGridSize
			// 
			this.nudGridSize.Location = new System.Drawing.Point(3, 93);
			this.nudGridSize.Name = "nudGridSize";
			this.nudGridSize.Size = new System.Drawing.Size(83, 20);
			this.nudGridSize.TabIndex = 2;
			this.nudGridSize.ValueChanged += new System.EventHandler(this.nudGridSize_ValueChanged);
			// 
			// chkSnapToGrid
			// 
			this.chkSnapToGrid.AutoSize = true;
			this.chkSnapToGrid.Location = new System.Drawing.Point(6, 42);
			this.chkSnapToGrid.Name = "chkSnapToGrid";
			this.chkSnapToGrid.Size = new System.Drawing.Size(83, 17);
			this.chkSnapToGrid.TabIndex = 1;
			this.chkSnapToGrid.Text = "Snap to grid";
			this.chkSnapToGrid.UseVisualStyleBackColor = true;
			// 
			// chkShowGrid
			// 
			this.chkShowGrid.AutoSize = true;
			this.chkShowGrid.Location = new System.Drawing.Point(6, 19);
			this.chkShowGrid.Name = "chkShowGrid";
			this.chkShowGrid.Size = new System.Drawing.Size(73, 17);
			this.chkShowGrid.TabIndex = 0;
			this.chkShowGrid.Text = "Show grid";
			this.chkShowGrid.UseVisualStyleBackColor = true;
			// 
			// tabPeggleNights
			// 
			this.tabPeggleNights.Controls.Add(this.grpTesting);
			this.tabPeggleNights.Location = new System.Drawing.Point(4, 22);
			this.tabPeggleNights.Name = "tabPeggleNights";
			this.tabPeggleNights.Size = new System.Drawing.Size(321, 182);
			this.tabPeggleNights.TabIndex = 1;
			this.tabPeggleNights.Text = "Peggle Nights";
			this.tabPeggleNights.UseVisualStyleBackColor = true;
			// 
			// grpTesting
			// 
			this.grpTesting.Controls.Add(this.lblPeggleNightsExePath);
			this.grpTesting.Controls.Add(this.txtPeggleNightsExePath);
			this.grpTesting.Location = new System.Drawing.Point(6, 6);
			this.grpTesting.Name = "grpTesting";
			this.grpTesting.Size = new System.Drawing.Size(309, 68);
			this.grpTesting.TabIndex = 1;
			this.grpTesting.TabStop = false;
			this.grpTesting.Text = "Testing";
			// 
			// lblPeggleNightsExePath
			// 
			this.lblPeggleNightsExePath.AutoSize = true;
			this.lblPeggleNightsExePath.Location = new System.Drawing.Point(6, 22);
			this.lblPeggleNightsExePath.Name = "lblPeggleNightsExePath";
			this.lblPeggleNightsExePath.Size = new System.Drawing.Size(155, 13);
			this.lblPeggleNightsExePath.TabIndex = 1;
			this.lblPeggleNightsExePath.Text = "Peggle Nights executable path:";
			// 
			// txtPeggleNightsExePath
			// 
			this.txtPeggleNightsExePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtPeggleNightsExePath.Location = new System.Drawing.Point(9, 38);
			this.txtPeggleNightsExePath.Name = "txtPeggleNightsExePath";
			this.txtPeggleNightsExePath.Size = new System.Drawing.Size(294, 20);
			this.txtPeggleNightsExePath.TabIndex = 0;
			this.txtPeggleNightsExePath.TextChanged += new System.EventHandler(this.txtPeggleNightsExePath_TextChanged);
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.Location = new System.Drawing.Point(262, 226);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 1;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(181, 226);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// tabApplication
			// 
			this.tabApplication.Controls.Add(this.btnSetFileAssociation);
			this.tabApplication.Location = new System.Drawing.Point(4, 22);
			this.tabApplication.Name = "tabApplication";
			this.tabApplication.Size = new System.Drawing.Size(321, 182);
			this.tabApplication.TabIndex = 2;
			this.tabApplication.Text = "Application";
			this.tabApplication.UseVisualStyleBackColor = true;
			// 
			// btnSetFileAssociation
			// 
			this.btnSetFileAssociation.Location = new System.Drawing.Point(6, 6);
			this.btnSetFileAssociation.Name = "btnSetFileAssociation";
			this.btnSetFileAssociation.Size = new System.Drawing.Size(139, 23);
			this.btnSetFileAssociation.TabIndex = 0;
			this.btnSetFileAssociation.Text = "Set File Association";
			this.btnSetFileAssociation.UseVisualStyleBackColor = true;
			this.btnSetFileAssociation.Click += new System.EventHandler(this.btnSetFileAssociation_Click);
			// 
			// OptionsForm
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(353, 261);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.tabControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OptionsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Options";
			this.tabControl.ResumeLayout(false);
			this.tabDesigner.ResumeLayout(false);
			this.grpMovement.ResumeLayout(false);
			this.grpMovement.PerformLayout();
			this.grpGridSnapping.ResumeLayout(false);
			this.grpGridSnapping.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudSnapThreshold)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudGridSize)).EndInit();
			this.tabPeggleNights.ResumeLayout(false);
			this.grpTesting.ResumeLayout(false);
			this.grpTesting.PerformLayout();
			this.tabApplication.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabDesigner;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.GroupBox grpGridSnapping;
		private System.Windows.Forms.CheckBox chkSnapToGrid;
		private System.Windows.Forms.CheckBox chkShowGrid;
		private System.Windows.Forms.Label lblSnapThreshold;
		private System.Windows.Forms.Label lblGridSize;
		private System.Windows.Forms.NumericUpDown nudGridSize;
		private System.Windows.Forms.NumericUpDown nudSnapThreshold;
		private System.Windows.Forms.GroupBox grpMovement;
		private System.Windows.Forms.CheckBox chkAlwaysShowAnchors;
		private System.Windows.Forms.TabPage tabPeggleNights;
		private System.Windows.Forms.GroupBox grpTesting;
		private System.Windows.Forms.Label lblPeggleNightsExePath;
		private System.Windows.Forms.TextBox txtPeggleNightsExePath;
		private System.Windows.Forms.TabPage tabApplication;
		private System.Windows.Forms.Button btnSetFileAssociation;
	}
}