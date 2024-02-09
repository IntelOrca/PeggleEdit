namespace IntelOrca.PeggleEdit.Designer.Forms
{
    partial class PakToolForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnPakBrowse = new System.Windows.Forms.Button();
            this.txtPakLocation = new System.Windows.Forms.TextBox();
            this.txtExtractLocation = new System.Windows.Forms.TextBox();
            this.btnExtractBrowse = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnPack = new System.Windows.Forms.Button();
            this.btnExtract = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75.7732F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24.2268F));
            this.tableLayoutPanel1.Controls.Add(this.btnPakBrowse, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtPakLocation, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtExtractLocation, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnExtractBrowse, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1150, 258);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // btnPakBrowse
            // 
            this.btnPakBrowse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPakBrowse.Location = new System.Drawing.Point(879, 33);
            this.btnPakBrowse.Margin = new System.Windows.Forms.Padding(8);
            this.btnPakBrowse.Name = "btnPakBrowse";
            this.btnPakBrowse.Size = new System.Drawing.Size(263, 31);
            this.btnPakBrowse.TabIndex = 0;
            this.btnPakBrowse.Text = "Browse...";
            this.btnPakBrowse.UseVisualStyleBackColor = true;
            this.btnPakBrowse.Click += new System.EventHandler(this.btnPakBrowse_Click);
            // 
            // txtPakLocation
            // 
            this.txtPakLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPakLocation.Location = new System.Drawing.Point(8, 33);
            this.txtPakLocation.Margin = new System.Windows.Forms.Padding(8);
            this.txtPakLocation.Name = "txtPakLocation";
            this.txtPakLocation.Size = new System.Drawing.Size(855, 31);
            this.txtPakLocation.TabIndex = 1;
            // 
            // txtExtractLocation
            // 
            this.txtExtractLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtExtractLocation.Location = new System.Drawing.Point(8, 105);
            this.txtExtractLocation.Margin = new System.Windows.Forms.Padding(8);
            this.txtExtractLocation.Name = "txtExtractLocation";
            this.txtExtractLocation.Size = new System.Drawing.Size(855, 31);
            this.txtExtractLocation.TabIndex = 2;
            // 
            // btnExtractBrowse
            // 
            this.btnExtractBrowse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnExtractBrowse.Location = new System.Drawing.Point(879, 105);
            this.btnExtractBrowse.Margin = new System.Windows.Forms.Padding(8);
            this.btnExtractBrowse.Name = "btnExtractBrowse";
            this.btnExtractBrowse.Size = new System.Drawing.Size(263, 31);
            this.btnExtractBrowse.TabIndex = 3;
            this.btnExtractBrowse.Text = "Browse...";
            this.btnExtractBrowse.UseVisualStyleBackColor = true;
            this.btnExtractBrowse.Click += new System.EventHandler(this.btnExtractBrowse_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(147, 25);
            this.label1.TabIndex = 4;
            this.label1.Text = ".pak Location:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(166, 25);
            this.label2.TabIndex = 5;
            this.label2.Text = "Extract location:";
            // 
            // flowLayoutPanel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.btnPack);
            this.flowLayoutPanel1.Controls.Add(this.btnExtract);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 211);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1144, 44);
            this.flowLayoutPanel1.TabIndex = 6;
            // 
            // btnPack
            // 
            this.btnPack.Location = new System.Drawing.Point(984, 0);
            this.btnPack.Margin = new System.Windows.Forms.Padding(0);
            this.btnPack.Name = "btnPack";
            this.btnPack.Size = new System.Drawing.Size(160, 40);
            this.btnPack.TabIndex = 0;
            this.btnPack.Text = "Pack";
            this.btnPack.UseVisualStyleBackColor = true;
            this.btnPack.Click += new System.EventHandler(this.btnPack_Click);
            // 
            // btnExtract
            // 
            this.btnExtract.Location = new System.Drawing.Point(816, 0);
            this.btnExtract.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(160, 40);
            this.btnExtract.TabIndex = 1;
            this.btnExtract.Text = "Extract";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // PakToolForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1150, 373);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PakToolForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = ".pak Extract / Pack";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnPakBrowse;
        private System.Windows.Forms.TextBox txtPakLocation;
        private System.Windows.Forms.TextBox txtExtractLocation;
        private System.Windows.Forms.Button btnExtractBrowse;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnPack;
        private System.Windows.Forms.Button btnExtract;
    }
}