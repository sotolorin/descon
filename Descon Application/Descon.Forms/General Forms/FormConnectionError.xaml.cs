namespace Descon.Forms
{
	/// <summary>
	/// Interaction logic for FormConnectionError.xaml
	/// </summary>
	public partial class FormConnectionError
	{
		public FormConnectionError(string message)
		{
			InitializeComponent();

			tbxMessage.Text = message;

			ShowDialog();
		}

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Close();
		}
	}
}
