using System;
using System.Windows;
using System.Windows.Media;
using Descon.Data;
using MahApps.Metro;

namespace Descon.UI.DataAccess
{
	public static class ThemeManagerHelper
	{
		/// <summary>
		/// Creates theme files and adds them to the manager
		/// </summary>
		public static void AddThemesToThemeManager()
		{
			var commonLists = new CommonLists();

			foreach (var theme in commonLists.ThemeDict)
				CreateAppStyleBy(theme.Key, theme.Value[0], theme.Value[1], theme.Value[2]);
		}

		private static void CreateAppStyleBy(string name, byte r, byte g, byte b)
		{
			var resourceDictionary = new ResourceDictionary();

			resourceDictionary.Add("HighlightColor", Color.FromArgb(255, r, g, b));
			resourceDictionary.Add("AccentColor", Color.FromArgb(204, r, g, b));
			resourceDictionary.Add("AccentColor2", Color.FromArgb(153, r, g, b));
			resourceDictionary.Add("AccentColor3", Color.FromArgb(102, r, g, b));
			resourceDictionary.Add("AccentColor4", Color.FromArgb(51, r, g, b));

			resourceDictionary.Add("HighlightBrush", new SolidColorBrush((Color)resourceDictionary["HighlightColor"]));
			resourceDictionary.Add("AccentColorBrush", new SolidColorBrush((Color)resourceDictionary["AccentColor"]));
			resourceDictionary.Add("AccentColorBrush2", new SolidColorBrush((Color)resourceDictionary["AccentColor2"]));
			resourceDictionary.Add("AccentColorBrush3", new SolidColorBrush((Color)resourceDictionary["AccentColor3"]));
			resourceDictionary.Add("AccentColorBrush4", new SolidColorBrush((Color)resourceDictionary["AccentColor4"]));
			resourceDictionary.Add("WindowTitleColorBrush", new SolidColorBrush((Color)resourceDictionary["AccentColor"]));

			resourceDictionary.Add("ProgressBrush", new LinearGradientBrush(
				new GradientStopCollection(new[]
                {
                    new GradientStop((Color)resourceDictionary["HighlightColor"], 0),
                    new GradientStop((Color)resourceDictionary["AccentColor3"], 1)
                }),
				new Point(0.001, 0.5), new Point(1.002, 0.5)));

			resourceDictionary.Add("CheckmarkFill", new SolidColorBrush((Color)resourceDictionary["AccentColor"]));
			resourceDictionary.Add("RightArrowFill", new SolidColorBrush((Color)resourceDictionary["AccentColor"]));

			resourceDictionary.Add("IdealForegroundColor", Colors.White);
			resourceDictionary.Add("IdealForegroundColorBrush", new SolidColorBrush((Color)resourceDictionary["IdealForegroundColor"]));
			resourceDictionary.Add("AccentSelectedColorBrush", new SolidColorBrush((Color)resourceDictionary["IdealForegroundColor"]));

			var fileName = ConstString.FOLDER_DESCONDATA_THEMES + name + ".xaml";
			using (var writer = System.Xml.XmlWriter.Create(fileName, new System.Xml.XmlWriterSettings { Indent = true }))
			{
				System.Windows.Markup.XamlWriter.Save(resourceDictionary, writer);
				writer.Close();
			}

			resourceDictionary = new ResourceDictionary {Source = new Uri(fileName, UriKind.Absolute)};

			var newAccent = new Accent { Name = name, Resources = resourceDictionary };
			ThemeManager.AddAccent(newAccent.Name, newAccent.Resources.Source);
		}
	}
}