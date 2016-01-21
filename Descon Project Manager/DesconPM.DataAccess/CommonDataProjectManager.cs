using System.Linq;
using System.Windows;
using Descon.UI.DataAccess;
using MahApps.Metro;

namespace DesconPM.DataAccess
{
	public class CommonDataProjectManager : CommonData
	{
		private ProjectFileStructure _projectStructure;

		/// <summary>
		/// Class based on CommonData from the main Descon application with the addition of Project data
		/// </summary>
		public CommonDataProjectManager()
		{
			ProjectStructure = new ProjectFileStructure();

			// Initializes the theme color for the application
			var theme = ThemeManager.DetectAppStyle(Application.Current);
			var accent = ThemeManager.Accents.First(x => x.Name == Preferences.ApplicationThemeName);
			ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
			ThemeAccent = AccentColors.First(a => a.Name == Preferences.ApplicationThemeName).ColorBrush;
		}

		/// <summary>
		/// Specific project data that isn't in CommonData
		/// </summary>
		public ProjectFileStructure ProjectStructure
		{
			get { return _projectStructure; }
			set
			{
				_projectStructure = value;
				OnPropertyChanged("ProjectStructure");
			}
		}
	}
}