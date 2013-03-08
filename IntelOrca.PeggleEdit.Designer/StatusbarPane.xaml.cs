using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IntelOrca.PeggleEdit.Designer
{
	/// <summary>
	/// Interaction logic for StatusbarPane.xaml
	/// </summary>
	internal partial class StatusbarPane : UserControl
	{
		public StatusbarPane()
		{
			InitializeComponent();
		}

		public void SetStatus(string status)
		{
			mStatusTextBlock.Text = status;
		}
	}
}
