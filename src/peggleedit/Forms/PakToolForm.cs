using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Designer.Forms
{
    public partial class PakToolForm : Form
    {
        public PakToolForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            AutoResize();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AutoResize();
        }

        private void AutoResize()
        {
            ClientSize = new Size(ClientSize.Width, tableLayoutPanel1.Height);
        }

        private void btnPakBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = ".pak Files (*.pak)|*.pak";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtPakLocation.Text = dialog.FileName;
                }
            }
        }

        private void btnExtractBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtExtractLocation.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            var pakLocation = txtPakLocation.Text.Trim();
            var extractLocation = txtExtractLocation.Text.Trim();
            try
            {
                var pak = new PakCollection(pakLocation);
                pak.Export(extractLocation);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void btnPack_Click(object sender, EventArgs e)
        {
            var pakLocation = txtPakLocation.Text.Trim();
            var extractLocation = txtExtractLocation.Text.Trim();
            try
            {
                var pak = new PakCollection();
                Import(pak, extractLocation, "");
                pak.Save(pakLocation);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Import(PakCollection pak, string absolute, string relative)
        {
            foreach (var f in Directory.GetFiles(absolute))
            {
                pak.ImportFile(relative, f);
            }
            foreach (var d in Directory.GetDirectories(absolute))
            {
                var folderName = Path.GetFileName(d);
                Import(pak, d, Path.Combine(relative, folderName));
            }
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
