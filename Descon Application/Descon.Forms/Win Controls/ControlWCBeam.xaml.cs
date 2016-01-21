using System.Windows;
using Descon.Data;

namespace Descon.Forms
{
	public partial class ControlWCBeam
	{
		public ControlWCBeam()
		{
			InitializeComponent();

			if (CommonDataStatic.Units == EUnit.Metric)
				stackPanelTopEl.Visibility = Visibility.Collapsed;
		}
	}
}