using System.Windows;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	/// <summary>
	/// Allows the user to toggle each view panel in the graphics interface on or off
	/// </summary>
	public partial class FormViews
	{
		public FormViews(CommonData data, Point point)
		{
			InitializeComponent();

			//Sets the window location on the screen. 50 is arbitrary just to move it down and over a bit.
			Left = point.X + 50;
			Top = point.Y + 50;

			DataContext = data;

			AllowsTransparency = true; // Doesn't work if set in the Properties window
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void CheckBox_Changed(object sender, RoutedEventArgs e)
		{
			UnityInteraction.SendDataToUnity(ConstString.UNITY_PREFERENCES_UPDATE);
		}
	}
}