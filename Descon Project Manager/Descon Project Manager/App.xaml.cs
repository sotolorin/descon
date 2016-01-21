using System.Windows;
using Descon.Forms;

namespace DesconPM.Main
{
	public partial class App
	{
		private void Application_Startup(object sender, StartupEventArgs args)
		{
			var licensing = new Licensing();

			if (licensing.LoadLicensingData())
				new FormDesconPMMain().Show();
		}
	}
}