using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class FormDesconMain
	{
		#region File

		private void menuNew_Click(object sender, RoutedEventArgs e)
		{
			NewFile();
		}

		private bool NewFile()
		{
			if (CommonDataStatic.DetailDataDict.Any(component => component.Value.IsActive) && new MiscFormMethods().IsSaveNeeded(_data))
			{
				var result = MessageBox.Show("Save current drawing?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				if (result == MessageBoxResult.Yes)
				{
					if (miscFormMethods.SaveFile(_data))
						miscFormMethods.NewFile(_data);
				}
				else if (result == MessageBoxResult.No)
					miscFormMethods.NewFile(_data);
				else if (result == MessageBoxResult.Cancel)
					return false;
			}
			else
				miscFormMethods.NewFile(_data);

			ApplyChangesToDrawing(null, new RoutedEventArgs());
			Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => controlDetailData.SelectShapeField()));

			return true;
		}

		private void menuOpen_Click(object sender, RoutedEventArgs e)
		{
			MenuOpen(string.Empty);
		}

		private void menuOpenRightClickSelect_Click(object sender, RoutedEventArgs e)
		{
			string path = _data.ContextMenuRecentFiles[((TextBlock)sender).Text];
			if (path != string.Empty)
				MenuOpen(path);
		}

		private void MenuOpen(string path)
		{
			UnityInteraction.SendDataToUnity(ConstString.UNITY_NEW_DRAWING);

			if (miscFormMethods.OpenFile(path, _data))
				ApplyChangesToDrawing(null, new RoutedEventArgs());

			ApplyChangesToDrawing(null, new RoutedEventArgs());

			controlReport.ScrollToTop();
		}

		private void menuSave_Click(object sender, RoutedEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Wait;

			controlReport.SaveComments();

			if (File.Exists(_data.CurrentFilePath) && _data.CurrentFilePath != ConstString.FILE_DEFAULT_NAME)
				new SaveDataToXML().SaveDesconDrawing(_data.CurrentFilePath, true);
			else
				miscFormMethods.SaveFile(_data);

			UnityInteraction.SendDataToUnity(ConstString.UNITY_USER_SAVED);

			Mouse.OverrideCursor = Cursors.Arrow;
		}

		private void menuSaveAs_Click(object sender, RoutedEventArgs e)
		{
			miscFormMethods.SaveFile(_data, true);
		}

		private void menuBatchReports_Click(object sender, RoutedEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Wait;
			miscFormMethods.RunBatchReports();
			Mouse.OverrideCursor = Cursors.Arrow;
		}

		#endregion

		#region Design tab

		private void menuSeismicSettings_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormSeismicSettings(_data);
			form.ShowDialog();
			miscFormMethods.ApplyChangesToDrawing(_data);
		}

		#endregion

		#region Tools tab

		private void menuMaterialsAndWelds_Click(object sender, RoutedEventArgs e)
		{
			var save = new SaveDataToXML();
			var load = new LoadDataFromXML();

			var form = new FormControlShell(_data, new ControlMaterialWeldEdit(ref _data), "Materials and Welds");
			var result = form.ShowDialog();
			if (result == true)
			{
				save.SaveMaterials();
				save.SaveWelds();
			}

			_data.MaterialDict = load.LoadMaterials();
			_data.WeldDict = load.LoadWelds();

			_data.MaterialName = _data.MaterialDictMember.FirstOrDefault().Value.Name;

			UnityInteraction.SendDataToUnity(ConstString.UNITY_PREFERENCES_UPDATE);
		}

		private void menuUserShapes_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormUserShapes(_data);
			form.ShowDialog();
			_data.OnPropertyChanged("ShapesFiltered");
		}

		private void menuPreferences_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlSettings(ref _data), "Settings");
			if (form.ShowDialog() == true)
			{
				new SaveDataToXML().SavePreferences();
				UnityInteraction.SendDataToUnity(ConstString.UNITY_PREFERENCES_UPDATE);
			}

			_data.Preferences = new LoadDataFromXML().LoadPreferences();

			miscFormMethods.ApplyChangesToDrawing(_data);

			_data.ContextMenuRecentFiles = MiscMethods.GetRecentlyOpenedFileList();
		}

		private void menuReportSettings_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlReportSettings(false), "Report Settings");
			if (form.ShowDialog() == true)
				new SaveDataToXML().SavePreferences();
			else
				_data.Preferences = new LoadDataFromXML().LoadPreferences();

			miscFormMethods.ApplyChangesToDrawing(_data);
		}

		private void menuScreenshot_Click(object sender, RoutedEventArgs e)
		{
			FindBoundsAndTakeScreenshot(false);
		}

		private void menuShortCuts_Click(object sender, RoutedEventArgs e)
		{
			new FormControlShell(_data, new ControlShortCuts(), "Shortcuts", true, true).Show();
		}


		private void menuQuickStart_Click(object sender, RoutedEventArgs e)
		{
			new FormControlShell(_data, new ControlQuickStart(), "Quick Start", true, true).Show();
		}

		private void menuCalculator_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("calc");
		}

		#endregion

		#region Help tab

		private void menuSupport_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("http://support.desconplus.com");
		}

		private void menuReferenceManual_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("http://www.desconplus.com/");
		}

		private void menuSendFeedback_Click(object sender, RoutedEventArgs e)
		{
			// Screenshot has to be here becuase we want to capture the main form
			FindBoundsAndTakeScreenshot(true);
			new FormFeedback(_data).ShowDialog();
		}

		private void menuOpenDesconDocs_Click(object sender, RoutedEventArgs e)
		{
			Process.Start(ConstString.FOLDER_MYDOCUMENTS_DESCON);
		}

		private void menuCheckForUpdate_Click(object sender, RoutedEventArgs e)
		{
			SoftwareUpdate.CheckForUpdate(_data, true);
		}

		private void menuChangeLog_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(ConstString.FILE_CHANGELOG))
				Process.Start(ConstString.FILE_CHANGELOG);
			else
			{
				new FileManipulatation().WriteResourceToFile(ConstString.FILE_CHANGELOG);
				Process.Start(ConstString.FILE_CHANGELOG);
			}
		}

		private void menuLicense_Click(object sender, RoutedEventArgs e)
		{
			new FormLicense("License", 0, true).ShowDialog();
		}

		#endregion

		#region Drawing Section Buttons

		private void btnZoomToFit_Click(object sender, RoutedEventArgs e)
		{
			UnityInteraction.SendDataToUnity(ConstString.UNITY_ZOOM_TO_FIT);
		}

		private void btnZoomToFitSelected_Click(object sender, RoutedEventArgs e)
		{
			UnityInteraction.SendDataToUnity(ConstString.UNITY_ZOOM_TO_FIT_SELECTED);
		}

		private void btnToggleView_Click(object sender, RoutedEventArgs e)
		{
			switch (((Control)sender).Uid)
			{
				case "btnShowLeft":
					_data.Preferences.ViewSettings.ShowLeft = !_data.Preferences.ViewSettings.ShowLeft;
					break;
				case "btnShowRight":
					_data.Preferences.ViewSettings.ShowRight = !_data.Preferences.ViewSettings.ShowRight;
					break;
				case "btnShowTop":
					_data.Preferences.ViewSettings.ShowTop = !_data.Preferences.ViewSettings.ShowTop;
					break;
				case "btnShowFront":
					_data.Preferences.ViewSettings.ShowFront = !_data.Preferences.ViewSettings.ShowFront;
					break;
				case "btnShow3D":
					_data.Preferences.ViewSettings.Show3D = !_data.Preferences.ViewSettings.Show3D;
					break;
				case "btnShowLeftReport":
					_data.Preferences.ReportSettings.ShowLeftSideView = !_data.Preferences.ReportSettings.ShowLeftSideView;
					break;
				case "btnShowRightReport":
					_data.Preferences.ReportSettings.ShowRightSideView = !_data.Preferences.ReportSettings.ShowRightSideView;
					break;
				case "btnShowTopReport":
					_data.Preferences.ReportSettings.ShowTopView = !_data.Preferences.ReportSettings.ShowTopView;
					break;
				case "btnShowFrontReport":
					_data.Preferences.ReportSettings.ShowFrontView = !_data.Preferences.ReportSettings.ShowFrontView;
					break;
				case "btnShow3DReport":
					_data.Preferences.ReportSettings.Show3DView = !_data.Preferences.ReportSettings.Show3DView;
					break;
			}

			SavePreferencesAndTellUnity();
			if(((Control)sender).Uid.Contains("Report"))
				ApplyChangesToDrawing(null, new RoutedEventArgs());
		}

		private void btnToggleView_MouseRightButtonUp(object sender, RoutedEventArgs e)
		{
			if (((Control)sender).Uid.Contains("Report"))
			{
				_data.Preferences.ReportSettings.ShowLeftSideView = false;
				_data.Preferences.ReportSettings.ShowRightSideView = false;
				_data.Preferences.ReportSettings.ShowTopView = false;
				_data.Preferences.ReportSettings.ShowFrontView = false;
				_data.Preferences.ReportSettings.Show3DView = false;
			}
			else
			{
				_data.Preferences.ViewSettings.ShowLeft = false;
				_data.Preferences.ViewSettings.ShowRight = false;
				_data.Preferences.ViewSettings.ShowTop = false;
				_data.Preferences.ViewSettings.ShowFront = false;
				_data.Preferences.ViewSettings.Show3D = false;
			}

			switch (((Control)sender).Uid)
			{
				case "btnShowLeft":
					_data.Preferences.ViewSettings.ShowLeft = true;
					break;
				case "btnShowRight":
					_data.Preferences.ViewSettings.ShowRight = true;
					break;
				case "btnShowTop":
					_data.Preferences.ViewSettings.ShowTop = true;
					break;
				case "btnShowFront":
					_data.Preferences.ViewSettings.ShowFront = true;
					break;
				case "btnShow3D":
					_data.Preferences.ViewSettings.Show3D = true;
					break;
				case "btnShowLeftReport":
					_data.Preferences.ReportSettings.ShowLeftSideView = true;
					break;
				case "btnShowRightReport":
					_data.Preferences.ReportSettings.ShowRightSideView = true;
					break;
				case "btnShowTopReport":
					_data.Preferences.ReportSettings.ShowTopView = true;
					break;
				case "btnShowFrontReport":
					_data.Preferences.ReportSettings.ShowFrontView = true;
					break;
				case "btnShow3DReport":
					_data.Preferences.ReportSettings.Show3DView = true;
					break;
			}

			SavePreferencesAndTellUnity();
			if (((Control)sender).Uid.Contains("Report"))
				ApplyChangesToDrawing(null, new RoutedEventArgs());
		}

		#endregion

		#region Report Section Buttons

		private void btnShowDrawing_Click(object sender, RoutedEventArgs e)
		{
			_data.Preferences.ReportSettings.ShowDrawing = !_data.Preferences.ReportSettings.ShowDrawing;
			SavePreferencesAndTellUnity();
			ApplyChangesToDrawing(null, new RoutedEventArgs());
		}

		private void btnShowCalculations_Click(object sender, RoutedEventArgs e)
		{
			_data.Preferences.ReportSettings.ShowCalculations = !_data.Preferences.ReportSettings.ShowCalculations;
			_data.Preferences.ReportSettings.OnPropertyChanged("ShowCalculations");
			SavePreferencesAndTellUnity();
			ApplyChangesToDrawing(null, new RoutedEventArgs());
		}

		private void btnReportTopOfPage_Click(object sender, RoutedEventArgs e)
		{
			_data.Preferences.ReportSettings.ShowDrawingAtTop = true;
			_data.Preferences.ReportSettings.OnPropertyChanged("ShowDrawingAtTop");
			SavePreferencesAndTellUnity();
			ApplyChangesToDrawing(null, new RoutedEventArgs());
		}

		private void btnReportBottomOfPage_Click(object sender, RoutedEventArgs e)
		{
			_data.Preferences.ReportSettings.ShowDrawingAtTop = false;
			_data.Preferences.ReportSettings.OnPropertyChanged("ShowDrawingAtTop");
			SavePreferencesAndTellUnity();
			ApplyChangesToDrawing(null, new RoutedEventArgs());
		}

		#endregion

		/// <summary>
		/// Sets the borders to look like normal Ribbon style tabs with only the current tab completely outlined
		/// </summary>
		private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (tabDesignHeader.IsSelected)
			{
				tabDesignHeader.BorderThickness = new Thickness(1, 1, 1, 0);
				tabToolsHeader.BorderThickness = new Thickness(0, 0, 0, 1);
				tabSupportHeader.BorderThickness = new Thickness(0, 0, 0, 1);
			}
			else if (tabToolsHeader.IsSelected)
			{
				tabDesignHeader.BorderThickness = new Thickness(0, 0, 0, 1);
				tabToolsHeader.BorderThickness = new Thickness(1, 1, 1, 0);
				tabSupportHeader.BorderThickness = new Thickness(0, 0, 0, 1);
			}
			else if (tabSupportHeader.IsSelected)
			{
				tabDesignHeader.BorderThickness = new Thickness(0, 0, 0, 1);
				tabToolsHeader.BorderThickness = new Thickness(0, 0, 0, 1);
				tabSupportHeader.BorderThickness = new Thickness(1, 1, 1, 0);
			}
		}

		/// <summary>
		/// Finds the proper bounds of the form after determining if it has been maximized
		/// </summary>
		private void FindBoundsAndTakeScreenshot(bool forBugReport)
		{
			double left, top;

			if (WindowState == WindowState.Maximized)
			{
				left = 0;
				top = 0;
			}
			else
			{
				left = Left;
				top = Top;
			}

			MiscFormMethods.TakeScreenshot(ActualWidth, ActualHeight, left, top, forBugReport);
		}

		private void GaugePlotter_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (wrapPanelGauges.Visibility == Visibility.Collapsed)
			{
				wrapPanelGauges.Visibility = Visibility.Visible;
				btnArrowGauges.Source = new BitmapImage(new Uri("pack://application:,,,/Descon.Resources;component/Images/Icons/Descon_UI_Icons_MainTab_ArrowRight.png", UriKind.Absolute));
			}
			else
			{
				wrapPanelGauges.Visibility = Visibility.Collapsed;
				btnArrowGauges.Source = new BitmapImage(new Uri("pack://application:,,,/Descon.Resources;component/Images/Icons/Descon_UI_Icons_MainTab_ArrowLeft.png", UriKind.Absolute));
			}
		}
	}
}