using System;
using System.Windows;
using Descon.Forms;
using Descon.UI.DataAccess;

namespace Descon.Main
{
	/// <summary>
	/// Interaction logic for FormSetDesonMode.xaml
	/// </summary>
	public partial class FormStartup
	{
		public FormStartup(string filePath)
		{
			InitializeComponent();

			Show();

			LaunchTheMainForm(filePath);

			Close();
		}

		[STAThread]
		private void LaunchTheMainForm(string filePath)
		{
			var licensing = new Licensing();
			if (!licensing.LoadLicensingData())
			{
				Close();
				return;
			}

			new FileManipulatation().ReplaceMissingFiles();
			var commonData = new CommonData();
			Window form = new FormDesconMain(filePath, commonData);
			form.Show();
		}
	}
}