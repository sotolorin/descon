namespace Descon.Data
{
	/// <summary>
	/// Sets up default preferences when we are creating a new preferences file
	/// </summary>
	public class DefaultPreferences
	{
		public ColorSettings SetColorDefaults()
		{
			var colorSettings = new ColorSettings();
			colorSettings.Columns = "#FFED5424";
			colorSettings.BeamsBraces = "#FF204DE6";
			colorSettings.Bolts = "#FF9261F8";
			colorSettings.WeldSymbols = "#FFFBB160";
			colorSettings.Text = "#FF32CEFC";
			colorSettings.DimensionLinesLeaders = "#FF969696";
			colorSettings.ConnectionElements = "#FF0C987A";
			colorSettings.Background = "#FF000000";
			colorSettings.Highlight = "#FF32CEFC";

			colorSettings.DrawingTheme = EDrawingTheme.Default;

			return colorSettings;
		}

		public ReportSettings SetReportDefaults()
		{
			var reportHeader = new ReportSettings
			{
				FontName = ConstString.DEFAULT_FONT_NAME,
				FontSize = ConstString.DEFAULT_FONT_SIZE,
				NumberOfColumns = 1,
				DefaultSaveFileFormat = EReportFileTypes.PDF,
				ShowCalculations = true,
				ShowDrawingAtTop = true,
				ShowLeftSideView = true,
				ShowFrontView = true,
				CombineDrawingViews = true,
				AutoToggleBookmarks = true,

				Title1 = "PROJECT NAME",
				Title2 = "PAGE NO",
				Title3 = "CALCULATED BY",
				Title4 = "CHECKED BY",
				Title5 = "DESCRIPTION",
				Title6 = "PROJECT NO",
				Title7 = "OF",
				Title8 = "CALC DATE",
				Title9 = "PROJECT DATE",

				UseDateForValue8 = true
			};

			return reportHeader;
		}

		public ViewSettings SetViewDefaults()
		{
			var viewSettings = new ViewSettings
			{
				Callouts = true,
				Dimensions = true,
				Welds = true,

				Show3D = true,
				ShowTop = true,
				ShowFront = true,
				ShowLeft = true,
				ShowRight = true,

				ThinLines = true,
			};

			return viewSettings;
		}
	}
}