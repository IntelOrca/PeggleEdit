using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IntelOrca.PeggleEdit.Designer.Forms
{
    public partial class NewUpdateForm : Form
    {
        private readonly UpdateInfo _update;
        private CancellationTokenSource _cts;

        /// <summary>
        /// Asks the user whether to update. Returns true if the update has been downloaded and is
        /// ready to install, in which case the caller should close PeggleEdit.
        /// </summary>
        public static bool Show(Form parent, UpdateInfo update, UpdateRing ring)
        {
            using (var form = new NewUpdateForm(update, ring))
            {
                return form.ShowDialog(parent) == DialogResult.OK;
            }
        }

        public NewUpdateForm(UpdateInfo update, UpdateRing ring)
        {
            InitializeComponent();
            _update = update;

            var ringName = ring == UpdateRing.Nightly ? "nightly" : "release";
            Text = "New Update Available";
            lblText.Text =
                $"A new {ringName} build of {Program.AppTitle} is available: {update.DisplayVersion}\n" +
                $"You have {Program.AppVersionLabel}.\n\n" +
                $"Do you want to update now? {Program.AppTitle} will close, update and restart. " +
                "Any unsaved changes will be lost.";
            lnkReleasePage.Text = update.PageUrl;
            chkSkipVersion.Text = update.IsNightly ? "Skip this nightly build" : "Skip this version";
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            btnUpdate.Enabled = false;
            chkSkipVersion.Enabled = false;
            lnkReleasePage.Visible = false;
            progressBar.Visible = true;
            btnClose.Text = "Cancel";

            _cts = new CancellationTokenSource();
            var progress = new Progress<(long done, long total)>(p =>
            {
                if (IsDisposed)
                    return;
                if (p.total > 0)
                {
                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Value = (int)Math.Min(100, p.done * 100 / p.total);
                }
                else
                {
                    progressBar.Style = ProgressBarStyle.Marquee;
                }
            });

            try
            {
                Updater.EnsureAppDirectoryWritable();
                var zipPath = await Updater.DownloadAsync(_update, progress, _cts.Token);
                var staging = await Task.Run(() => Updater.Stage(zipPath));
                // The staged files are removed on the next start if the user cancelled while extracting.
                _cts.Token.ThrowIfCancellationRequested();
                Updater.PendingInstall = staging;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (OperationCanceledException) when (_cts.IsCancellationRequested)
            {
                // Cancelled by the user
            }
            catch (Exception ex)
            {
                if (IsDisposed)
                    return;

                var message = ex is OperationCanceledException ? "The download timed out." : ex.Message;
                var result = MessageBox.Show(
                    this,
                    $"{message}\n\nDo you want to open the download page instead?",
                    "Failed to update",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                    Process.Start(_update.PageUrl);

                btnUpdate.Enabled = true;
                chkSkipVersion.Enabled = true;
                lnkReleasePage.Visible = true;
                progressBar.Visible = false;
                progressBar.Value = 0;
                btnClose.Text = "Not now";
            }
        }

        private void lnkReleasePage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(_update.PageUrl);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cts?.Cancel();
            if (DialogResult != DialogResult.OK && chkSkipVersion.Checked)
            {
                Settings.Default.SkippedUpdate = _update.Key;
                Settings.Save();
            }
            base.OnFormClosing(e);
        }
    }
}
