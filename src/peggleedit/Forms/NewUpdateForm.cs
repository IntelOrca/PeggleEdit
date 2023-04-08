using System.Diagnostics;
using System.Windows.Forms;

namespace IntelOrca.PeggleEdit.Designer.Forms
{
    public partial class NewUpdateForm : Form
    {
        public static void Show(Form parent)
        {
            var form = new NewUpdateForm();
            form.ShowDialog(parent);
        }

        public NewUpdateForm()
        {
            InitializeComponent();
        }

        private void btnDownload_Click(object sender, System.EventArgs e)
        {
            Process.Start("https://github.com/IntelOrca/PeggleEdit/releases");
        }

        private void btnClose_Click(object sender, System.EventArgs e)
        {
            if (chkDoNotShowAgain.Checked)
            {
                Settings.Default.HideVersionNotification = true;
                Settings.Save();
            }
            Close();
        }
    }
}
