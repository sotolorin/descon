using System.Windows;

namespace Descon.Forms
{
	public partial class FormHotKeys
	{
		public FormHotKeys()
		{
			InitializeComponent();

			Owner = Application.Current.MainWindow;
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}