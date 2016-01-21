using System.Windows;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class ControlWCSplice
	{
		private CommonData _data;

		public ControlWCSplice(ref CommonData data)
		{
			InitializeComponent();

			_data = data;
		}

		private void btnBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.ColumnSplice.Bolt), "Bolt Selection");
			form.ShowDialog();
		}
	}
}