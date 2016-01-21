namespace Descon.Forms
{
	/// <summary>
	/// Interaction logic for FormConnectionError.xaml
	/// </summary>
	public partial class FormMissingMaterialOrWeld
	{
		public FormMissingMaterialOrWeld(string message)
		{
			InitializeComponent();

			tbxTextBlock.Text = "User-Specified materials or welds are used in this file that are not present on this installation of Descon." +
								" Do you wish to import these to this installation?" + System.Environment.NewLine + System.Environment.NewLine +
			                    " If you choose No, the file's user-specified materials/welds will be assigned the current default types specified in Settings.";

			ShowDialog();
		}

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Close();
		}
	}
}
