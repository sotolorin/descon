using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows;
using Descon.Data;
using Descon.UI.DataAccess;
using Descon.WebAccess;
using MahApps.Metro;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Timers.Timer;

namespace Descon.Forms
{
	/* Metro theme is free third party control MahApps: http://mahapps.com/MahApps.Metro/
	 * DLL's have been added to "packages" folder in solution through the NuGet Manager.
	 * 
	 * This form consists a few main parts:
	 * 
	 * TOP - Custom buttons made to look like a Ribbon
	 * LEFT - Detail Data ControlDetailData.xaml
	 * MIDDLE - Drawing and report toggle panel. Drawing panel is Descon.Unity.View.exe
	 * BOTTOM - File name and download status
	 * RIGHT - Optional collapsable gauge panel
	 * 
	 * All data on this form is bound to CommonData and it's sub classes. The graphics portion is Unity application brought in
	 * as a hosted exe file and loaded in a panel
	 */

	public partial class FormDesconMain
	{
		private CommonData _data;
		private readonly string _filePath = string.Empty;
		private bool _dontApplyChanges;		// Turns off the ApplyChanges functionality until we are done creating the window
		private readonly MiscFormMethods miscFormMethods = new MiscFormMethods();
		private readonly SaveDataToXML _saveDataToXml = new SaveDataToXML();
		private Timer _reactivationTimer;
		private const double REACTIVATION_TIMER_MINUTES = 30;
		private const double REACTIVATION_TIMER_MILLISECONDS = REACTIVATION_TIMER_MINUTES * 60 * 1000;
		private bool _triedOnceToActivate;

		#region Stuff for bringing the Unity form as a hosted application

		/// <summary>
		/// Process which will contain the Unity exe
		/// </summary>
		//private Process _unityProcess;
		/// <summary>
		/// This is used to set the parent of the Unity exe
		/// </summary>
		[DllImport("user32.dll", SetLastError = true)]
		private static extern long SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
		/// <summary>
		/// Sets the window position within the hosted panel
		/// </summary>
		[DllImport("user32.dll")]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

		#endregion

		public FormDesconMain(string filePath, CommonData data)
		{
			_dontApplyChanges = true;

			InitializeComponent();

			Application.Current.MainWindow = this;

			_data = data;
			DataContext = _data;

			ThemeManagerHelper.AddThemesToThemeManager();

			ThemeManager.IsThemeChanged += ThemeManager_IsThemeChanged;

			if (File.Exists(filePath))
				_filePath = filePath;

			if(!data.LicenseMinimumDeveloper)
				cbxLicenseType.Visibility = Visibility.Collapsed;
		}

		/// <summary>
		/// Opens a file when the application is already open. The first arg [0] is the application path and the 
		/// second [1] is the file path that was double clicked
		/// </summary>
		public void OpenFileFromArgs(IList<string> args)
		{
			if (File.Exists(args[1]))
			{
				miscFormMethods.OpenFile(args[1], _data);
				ApplyChangesToDrawing(null, new RoutedEventArgs());
			}
		}

		// We want this to trigger after the window is completely down initializing everything
		private void DesconMainWindow_ContentRendered(object sender, EventArgs e)
		{
			_dontApplyChanges = false;
			if (_filePath != string.Empty)
			{
				miscFormMethods.OpenFile(_filePath, _data);
				ApplyChangesToDrawing(null, new RoutedEventArgs());
			}

			ResizeGraphicsPanel();

			_reactivationTimer = new Timer(REACTIVATION_TIMER_MILLISECONDS);
			_reactivationTimer.Elapsed += OnTimedEvent;
			_reactivationTimer.AutoReset = true;
			_reactivationTimer.Enabled = true;
			_reactivationTimer.SynchronizingObject = controlReport.webBrowser; // Forces the thread to connect to the main window
		}

		private void OnTimedEvent(Object source, ElapsedEventArgs e)
		{
			var licenseData = new LoadDataFromXML().LoadLicenseFile();
			var webServerInteraction = new WebServerInteraction();
			var sqlServerInteraction = new SQLServerIntraction();

			if (webServerInteraction.CheckServer())
			{
				string result = sqlServerInteraction.ReactivateLicense(licenseData);
				if (result != "0")
				{
					new FormConnectionError("Someone has logged in to this account from another computer. Your work will be saved and Descon will exit.");

					NoLicenseConnectionExit();
				}
				else
					_triedOnceToActivate = false;
			}
			else if (!_triedOnceToActivate)
			{
				_triedOnceToActivate = true;
				new FormConnectionError("Connection with the internet or license server was lost. One more attempt will be made in " + REACTIVATION_TIMER_MINUTES + " minutes.");
			}
			else if (_triedOnceToActivate)
			{
				if (CommonDataStatic.CurrentFilePath == ConstString.FILE_DEFAULT_NAME)
					CommonDataStatic.CurrentFilePath = ConstString.FOLDER_MYDOCUMENTS_DESCON + ConstString.FILE_DEFAULT_NAME;

				new FormConnectionError("Connection with the internet or license server was lost. Descon will exit and your worked will be saved to:\r\r" + CommonDataStatic.CurrentFilePath);

				NoLicenseConnectionExit();
			}
		}

		private void NoLicenseConnectionExit()
		{
			if (CommonDataStatic.CurrentFilePath == ConstString.FILE_DEFAULT_NAME)
				CommonDataStatic.CurrentFilePath = ConstString.FOLDER_MYDOCUMENTS_DESCON + ConstString.FILE_DEFAULT_NAME;

			new SaveDataToXML().SaveDesconDrawing(CommonDataStatic.CurrentFilePath, true);
			Environment.Exit(Environment.ExitCode);
		}


		private void DesconMainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				//Create the named pipe server to send messages to Unity
				CommonDataStatic.Server = new NamedPipeServerStream(ConstString.UNITY_PIPE_NAME_SEND, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
				CommonDataStatic.Server.BeginWaitForConnection(UnityInteraction.WaitForConnectionSend, CommonDataStatic.Server);

				var receivePipe = new NamedPipeServerStream(ConstString.UNITY_PIPE_NAME_RECEIVE, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
				receivePipe.BeginWaitForConnection(WaitForConnectionReceive, receivePipe);
			}
			catch
			{
				MessageBox.Show("Error initializing graphics pipe connection. Application will now close.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
				Application.Current.Shutdown();
			}

			var theme = ThemeManager.DetectAppStyle(Application.Current);
			var accent = ThemeManager.Accents.First(x => x.Name == _data.Preferences.ApplicationThemeName);
			ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);

			var fontsCollection = new InstalledFontCollection();
			_data.CommonLists.ReportFontList = fontsCollection.Families.Select(f => f.Name).ToList();

			// Disable/enable certain features depending on the current license			
			if (!CommonDataStatic.LicenseMinimumNext)
				CommonDataStatic.Preferences.Seismic = ESeismic.NonSeismic;
			if (!CommonDataStatic.LicenseMinimumStandard)
				CommonDataStatic.Preferences.Units = EUnit.US;

			tabDesignHeader.IsSelected = true;
			stackPanelReportButtons.Visibility = Visibility.Collapsed;
			wrapPanelGauges.Visibility = Visibility.Collapsed;

			// This loads the Unity exe into the application
			try
			{
				// Finds any previously running Unity processes and kills them. If the debugger is stopped or the app crashes, the
				// process may still be running.
				var processName = Process.GetProcessesByName("Descon.Unity.View");
				if (processName.Length != 0)
				{
					var processlist = Process.GetProcesses();
					foreach (var process in processlist.Where(process => process.ProcessName == "Descon.Unity.View"))
						process.Kill();
				}

				// Unity application and arguments. -popupwindow removes the title bar and border, -nolog prevents a log file from being created
				string directoryName = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
				var procInfo = new ProcessStartInfo(directoryName + "\\Unity View\\Descon.Unity.View.exe", "-popupwindow -nolog");
				CommonDataStatic.UnityProcess = Process.Start(procInfo);
				if (CommonDataStatic.UnityProcess != null)
				{
					// This waits for the Descon window to initialize before adding the Unity exe to the panel
					while (CommonDataStatic.UnityProcess.MainWindowHandle == IntPtr.Zero)
						Thread.Yield();

					SetParent(CommonDataStatic.UnityProcess.MainWindowHandle, windowsFormHostUnity.Handle);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error initializing graphics module: " + ex.Message + "\r\rApplication will now close.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				Application.Current.Shutdown();
			}

			if(CommonDataStatic.Preferences.AutomaticallyCheckForUpdates)
				SoftwareUpdate.CheckForUpdate(_data, false);
		}

		/// <summary>
		/// Saves the preferences, log file and kills the Unity process when the application closes
		/// </summary>
		private void DesconMainWindow_Closing(object sender, CancelEventArgs e)
		{
			if (new WebServerInteraction().CheckServer())
			{
				var licenseData = new LoadDataFromXML().LoadLicenseFile();
				new SQLServerIntraction().DeactivateLicense(licenseData);
			}

			// If there are unsaved changes prompt the user to save, otherwise just continue exiting.
			if (CommonDataStatic.DetailDataDict.Any(component => component.Value.IsActive) && miscFormMethods.IsSaveNeeded(_data))
			{
				var result = MessageBox.Show("There are unsaved changes. Would you like to save?", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
				switch (result)
				{
					case MessageBoxResult.Yes:	// User wants to save
						if (!miscFormMethods.SaveFile(_data))
						{
							e.Cancel = true;	// User cancelled Save Dialog so we don't want to exit Descon
							return;
						}
						break;
					case MessageBoxResult.Cancel:	// User cancelled so we don't want to exit Descon
						e.Cancel = true;
						return;
				}
			}
			// We made it here which means data was saved or there was nothing to save. Time to run for the hills!
			_saveDataToXml.SavePreferences();

			if (CommonDataStatic.UnityProcess != null && !CommonDataStatic.UnityProcess.HasExited)
				CommonDataStatic.UnityProcess.Kill();

			if (CommonDataStatic.Server != null)
				CommonDataStatic.Server.Close();
		}
	}
}