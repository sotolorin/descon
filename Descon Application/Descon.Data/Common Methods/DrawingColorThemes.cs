namespace Descon.Data
{
	public static class DrawingColorThemes
	{
		public static ColorSettings GetColorTheme(EDrawingTheme drawingTheme)
		{
			var colorSettings = new ColorSettings();

			switch (drawingTheme)
			{
				case EDrawingTheme.Default:
					colorSettings.Columns = "#FFED5424";
					colorSettings.BeamsBraces = "#FF204DE6";
					colorSettings.Bolts = "#FF9261F8";
					colorSettings.WeldSymbols = "#FFFBB160";
					colorSettings.Text = "#FF32CEFC";
					colorSettings.DimensionLinesLeaders = "#FF969696";
					colorSettings.ConnectionElements = "#FF0C987A";
					colorSettings.Background = "#FF000000";
					colorSettings.Highlight = "#FF32CEFC";
					break;
				case EDrawingTheme.Muted:
					colorSettings.Columns = "#FF3E77B6";
					colorSettings.BeamsBraces = "#FFB76B52";
					colorSettings.Bolts = "#FFCC5473";
					colorSettings.WeldSymbols = "#FF4FBC8B";
					colorSettings.Text = "#FF3AC2D2";
					colorSettings.DimensionLinesLeaders = "#FF969696";
					colorSettings.ConnectionElements = "#FFE8C637";
					colorSettings.Background = "#FF000000";
					colorSettings.Highlight = "#FF3AC2D2";
					break;
				case EDrawingTheme.Bright:
					colorSettings.Columns = "#FF63C9D5";
					colorSettings.BeamsBraces = "#FF9A2588";
					colorSettings.Bolts = "#FFFBE046";
					colorSettings.WeldSymbols = "#FF4378BC";
					colorSettings.Text = "#FFEC5525";
					colorSettings.DimensionLinesLeaders = "#FF969696";
					colorSettings.ConnectionElements = "#FF5FBD6E";
					colorSettings.Background = "#FF000000";
					colorSettings.Highlight = "#FFEC5525";
					break;
			}

			colorSettings.DrawingTheme = drawingTheme;

			return colorSettings;
		}
	}
}