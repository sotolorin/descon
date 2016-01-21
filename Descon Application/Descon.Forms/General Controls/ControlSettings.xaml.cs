using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;
using Descon.Data;
using Descon.UI.DataAccess;
using MahApps.Metro;
using Application = System.Windows.Application;

namespace Descon.Forms
{
    public partial class ControlSettings
    {
	    private CommonData _data;

		/// <summary>
		/// Preferences setup form
		/// </summary>
        public ControlSettings(ref CommonData data)
        {
            InitializeComponent();

			_data = data;

			imageTransferForce.ToolTip = ConstString.HELP_BRACE_PREFERENCES;
        }

		private void btnBolt_Click(object sender, RoutedEventArgs e)
		{
			FormControlShell form;

			if (CommonDataStatic.Units == EUnit.US)
			{
				form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.Preferences.DefaultBoltUS), "Bolt Selection");
				form.ShowDialog();
				_data.Preferences.DefaultBoltUS = _data.CurrentBolt.ShallowCopy();
			}
			else
			{
				form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.Preferences.DefaultBoltMetric), "Bolt Selection");
				form.ShowDialog();
				_data.Preferences.DefaultBoltMetric = _data.CurrentBolt.ShallowCopy();
			}

			foreach (var detailData in _data.DetailDataDict)
				detailData.Value.BoltBrace = _data.CurrentBolt.ShallowCopy();
		}

		private void cbxDrawingTheme_Changed(object sender, EventArgs e)
		{
			_data.Preferences.ColorSettings = DrawingColorThemes.GetColorTheme(_data.Preferences.ColorSettings.DrawingTheme);

			_data.ColorDrawingColumns = new BrushConverter().ConvertFromString(_data.Preferences.ColorSettings.Columns) as SolidColorBrush;
			_data.ColorDrawingBeamsBraces = new BrushConverter().ConvertFromString(_data.Preferences.ColorSettings.BeamsBraces) as SolidColorBrush;
			_data.ColorDrawingDimensionLinesLeaders = new BrushConverter().ConvertFromString(_data.Preferences.ColorSettings.DimensionLinesLeaders) as SolidColorBrush;
			_data.ColorDrawingText = new BrushConverter().ConvertFromString(_data.Preferences.ColorSettings.Text) as SolidColorBrush;
			_data.ColorDrawingBolts = new BrushConverter().ConvertFromString(_data.Preferences.ColorSettings.Bolts) as SolidColorBrush;
			_data.ColorDrawingWeldSymbols = new BrushConverter().ConvertFromString(_data.Preferences.ColorSettings.WeldSymbols) as SolidColorBrush;
			_data.ColorDrawingConnectionElements = new BrushConverter().ConvertFromString(_data.Preferences.ColorSettings.ConnectionElements) as SolidColorBrush;
			_data.ColorDrawingBackground = new BrushConverter().ConvertFromString(_data.Preferences.ColorSettings.Background) as SolidColorBrush;
		}

		/// <summary>
		/// Click event for the color icons. Calls a color selection form so the user can choose a new color
		/// </summary>
		private void colorIcon_Click(object sender, RoutedEventArgs e)
		{
			var solidColorBrush = new SolidColorBrush();

			var colorDialog = new ColorDialog();
			if (colorDialog.ShowDialog() == DialogResult.OK)
			{
				solidColorBrush.Color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
				((Ellipse)sender).Fill = solidColorBrush;
			}
		}

		private void btnResetDefaults_Click(object sender, RoutedEventArgs e)
		{
			_data.Preferences.DefaultMaterials = new DefaultMaterials();
			_data.Preferences.DefaultConnectionTypes = new DefaultConnectionTypes();
			_data.Preferences.DefaultMinimumEdgeDistances = new DefaultMinimumEdgeDistances();
			_data.Preferences.DefaultElectrode = CommonDataStatic.WeldDict["E70XX"];
			_data.Preferences.DefaultBoltUS = new Bolt();
			_data.Preferences.DefaultBoltMetric = new Bolt();
			_data.Preferences.MinThicknessAngle = 0;
			_data.Preferences.MinThicknessGussetPlate = 0;
			_data.Preferences.MinThicknessSinglePlate = 0;
		}

		private void ellipseThemeSelect_Click(object sender, RoutedEventArgs e)
		{
			var theme = ThemeManager.DetectAppStyle(Application.Current);
			var accent = ThemeManager.Accents.First(x => x.Name == ((Ellipse)sender).Uid);
			ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
		}

	    private void btnSetWhiteBackground_Click(object sender, RoutedEventArgs e)
	    {
			_data.Preferences.ColorSettings.Background = "#FFFFFFFF";
			_data.ColorDrawingBackground = new BrushConverter().ConvertFromString(_data.Preferences.ColorSettings.Background) as SolidColorBrush;
	    }

		private void btnSetBlackBackground_Click(object sender, RoutedEventArgs e)
		{
			_data.Preferences.ColorSettings.Background = "#FF000000";
			_data.ColorDrawingBackground = new BrushConverter().ConvertFromString(_data.Preferences.ColorSettings.Background) as SolidColorBrush;
		}
    }
}