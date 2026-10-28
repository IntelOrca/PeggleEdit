namespace IntelOrca.PeggleEdit.Designer.Forms
{
    partial class NewUpdateForm
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
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblText = new System.Windows.Forms.Label();
            this.chkSkipVersion = new System.Windows.Forms.CheckBox();
            this.lnkReleasePage = new System.Windows.Forms.LinkLabel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // btnUpdate
            // 
            this.btnUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdate.Location = new System.Drawing.Point(220, 288);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(174, 50);
            this.btnUpdate.TabIndex = 0;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(400, 288);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(189, 50);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Not now";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblText
            // 
            this.lblText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblText.Location = new System.Drawing.Point(32, 24);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(557, 175);
            this.lblText.TabIndex = 2;
            this.lblText.Text = "A new update to PeggleEdit is available.";
            // 
            // chkSkipVersion
            // 
            this.chkSkipVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkSkipVersion.AutoSize = true;
            this.chkSkipVersion.Location = new System.Drawing.Point(37, 249);
            this.chkSkipVersion.Name = "chkSkipVersion";
            this.chkSkipVersion.Size = new System.Drawing.Size(197, 29);
            this.chkSkipVersion.TabIndex = 3;
            this.chkSkipVersion.Text = "Skip this version";
            this.chkSkipVersion.UseVisualStyleBackColor = true;
            // 
            // lnkReleasePage
            // 
            this.lnkReleasePage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkReleasePage.AutoEllipsis = true;
            this.lnkReleasePage.Location = new System.Drawing.Point(32, 207);
            this.lnkReleasePage.Name = "lnkReleasePage";
            this.lnkReleasePage.Size = new System.Drawing.Size(557, 30);
            this.lnkReleasePage.TabIndex = 4;
            this.lnkReleasePage.TabStop = true;
            this.lnkReleasePage.Text = "https://github.com/IntelOrca/PeggleEdit/releases";
            this.lnkReleasePage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkReleasePage_LinkClicked);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(37, 207);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(552, 30);
            this.progressBar.TabIndex = 5;
            this.progressBar.Visible = false;
            // 
            // NewUpdateForm
            // 
            this.AcceptButton = this.btnUpdate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(601, 350);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lnkReleasePage);
            this.Controls.Add(this.chkSkipVersion);
            this.Controls.Add(this.lblText);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnUpdate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewUpdateForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Update Available";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.CheckBox chkSkipVersion;
        private System.Windows.Forms.LinkLabel lnkReleasePage;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}
