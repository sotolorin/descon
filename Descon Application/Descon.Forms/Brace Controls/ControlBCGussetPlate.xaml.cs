using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class ControlBCGussetPlate
	{
		public ControlBCGussetPlate(ref CommonData data)
		{
			InitializeComponent();

			if (data.DetailDataDict[EMemberType.PrimaryMember].Shape.Name == ConstString.NONE &&
				data.SelectedMember.KBrace || data.SelectedMember.KBrace)
			{
				ctrlBeamWeld.IsEnabled = false;
				ctrlColumnWeld.IsEnabled = true;
			}
		}
	}
}