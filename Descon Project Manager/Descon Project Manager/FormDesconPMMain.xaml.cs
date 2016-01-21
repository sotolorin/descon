using System;
using System.Windows;
using Descon.Data;
using Descon.UI.DataAccess;
using DesconPM.DataAccess;

namespace DesconPM.Main
{
	public partial class FormDesconPMMain
	{
		private CommonDataProjectManager _projectData;

		public FormDesconPMMain()
		{
			InitializeComponent();

			ThemeManagerHelper.AddThemesToThemeManager();

			_projectData = new CommonDataProjectManager
			{
				ProjectStructure =
				{
					DateModified = DateTime.Now,
					DateCreated = DateTime.Now
				}
			};

			DataContext = _projectData;
			_projectData.Preferences = new LoadDataFromXML().LoadPreferences();

			ctrlReport.ProjectManagerReportInit();
		}

		private void SettingsUpdated(object sender, RoutedEventArgs e)
		{
			_projectData.Preferences = new LoadDataFromXML().LoadPreferences();
		}

		#region Buttons

		private void btnOpenProject_Click(object sender, RoutedEventArgs e)
		{
			OpenProject();
		}

		private void btnNewProject_Click(object sender, RoutedEventArgs e)
		{
			NewProject();
		}

		private void btnSaveProject_Click(object sender, RoutedEventArgs e)
		{
			SaveProject();
		}

		private void btnSaveAsProject_Click(object sender, RoutedEventArgs e)
		{
			SaveAsProject();
		}

		private void btnAddDrawing_Click(object sender, RoutedEventArgs e)
		{
			AddDrawing();
			_projectData.ReportText = string.Empty;
		}

		private void btnRemoveDrawing_Click(object sender, RoutedEventArgs e)
		{
			RemoveDrawing();
			_projectData.ReportText = string.Empty;
		}

		private void btnMoveDrawingUp_Click(object sender, RoutedEventArgs e)
		{
			MoveDrawing(true);
			_projectData.ReportText = string.Empty;
		}

		private void btnMoveDrawingDown_Click(object sender, RoutedEventArgs e)
		{
			MoveDrawing(false);
			_projectData.ReportText = string.Empty;
		}

		private void btnRefreshReport_Click(object sender, RoutedEventArgs e)
		{
			RefreshReport();
		}

		#endregion

		#region Other Events

		private void dataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (((DrawingItem)dataGrid.SelectedValue).Checked)
				ctrlReport.FindBlockInReport(ConstString.REPORT_INDEX + dataGrid.SelectedIndex);
		}

		#endregion
	}
}