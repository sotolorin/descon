using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Descon.Data;
using Descon.UI.DataAccess;
using Cursors = System.Windows.Input.Cursors;
using DataFormats = System.Windows.DataFormats;
using Fema = Descon.Calculations.Fema;
using MessageBox = System.Windows.MessageBox;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Descon.Forms
{
	/// <summary>
	/// These are commonly used methods that won't work in Descon.Data becuase of the limited .Net functionality for that DLL or
	/// methods that need CommonData.
	/// </summary>
	public class MiscFormMethods
	{
		/// <summary>
		/// Used to make sure we don't run into a loop when text fields are updated.
		/// </summary>
		private bool applying;

		/// <summary>
		/// Applys changes, runs calculations, generates report, and sends the data to Unity
		/// </summary>
		internal void ApplyChangesToDrawing(CommonData data)
		{
			bool needToZoom = false;

			try
			{
				if (applying)
					return;

				// New drawing, so Zoom to Fit after drawing
				if (!data.DetailDataDict[EMemberType.PrimaryMember].IsActive)
					needToZoom = true;

				applying = true;
				if (data.SelectedMember == null)
					return;

				Mouse.OverrideCursor = Cursors.Wait;

				data.NumberOfNoGoods = 0;

				// Calculations need to run twice because certain values are set with the first pass and used in the second pass
				data.GaugeData.Clear();
				Calculations.Calculations.RunCalculations();

				data.GaugeData.Clear();
				Calculations.Calculations.RunCalculations();

				// Sends the update command to Unity and then waits for it to finish
				UnityInteraction.SendDataToUnity(ConstString.UNITY_UPDATE);
				CommonDataStatic.UnityDoneUpdating = false;
				int waitCounter = 0;
				while (!CommonDataStatic.UnityDoneUpdating)
				{
					if (waitCounter++ == 300) // Give up after 3 seconds
						break;
				}

				foreach (var line in CommonDataStatic.DetailReportLineList)
				{
					if (line.LineString.Contains(ConstString.REPORT_NOGOOD))
						data.NumberOfNoGoods++;
				}

				if (CommonDataStatic.Preferences.ReportSettings.ShowDrawing && !CommonDataStatic.Preferences.ReportSettings.NoViewsSelected)
					UnityInteraction.RequestDrawing();

				data.ReportText = new ReportBuild().BuildReport();
				data.OnPropertyChanged("ReportGoToList");

				// This is used for the side by side page view
				new ReportSave().SaveReport(data.ReportText, ConstString.FILE_REPORT_RTF, null, false);
				using (FileStream fileStream = File.Open(ConstString.FILE_REPORT_RTF, FileMode.Open, FileAccess.Read))
				{
					data.ReportDocument = new FlowDocument();
					TextRange textRange = new TextRange(data.ReportDocument.ContentStart, data.ReportDocument.ContentEnd);
					textRange.Load(fileStream, DataFormats.Rtf);
				}
			}
			finally
			{
				applying = false;
				if(needToZoom)
					UnityInteraction.SendDataToUnity(ConstString.UNITY_ZOOM_TO_FIT);
				if(CommonDataStatic.UnityNewMemberAdded)
					UnityInteraction.SendDataToUnity(ConstString.UNITY_NEW_MEMBER_ADDED);

				Mouse.OverrideCursor = Cursors.Arrow;
			}
		}

		#region File Stuff

		/// <summary>
		/// Creates a new file and sets default data in the UI
		/// </summary>
		internal void NewFile(CommonData data)
		{
			data.CurrentFilePath = ConstString.FILE_DEFAULT_NAME;
			data.SetDefaultUIData();
			UnityInteraction.SendDataToUnity(ConstString.UNITY_NEW_DRAWING);
			UnityInteraction.SendDataToUnity(ConstString.UNITY_ZOOM_TO_FIT);
		}

		/// <summary>
		/// Returns true if the save went through and was successful. forceSave is used for Save As since we don't want to check if
		/// a save is needed
		/// </summary>
		internal bool SaveFile(CommonData data, bool forceSave = false)
		{
			bool fileSaved;

			if (!forceSave && !IsSaveNeeded(data))
				return true;

			var saveDialog = new SaveFileDialog
			{
				AddExtension = true,
				DefaultExt = ConstString.FILE_EXTENSION_DRAWING,
				Filter = ConstString.FILE_DESCRIPTION_DRAWING + "|*" + ConstString.FILE_EXTENSION_DRAWING,
				FileName = data.CurrentFileName,
				InitialDirectory = data.CurrentFilePathOnly,
				OverwritePrompt = true,
				Title = "Save Drawing"
			};
			if (saveDialog.ShowDialog() == DialogResult.Cancel)
				fileSaved = false;
			else
			{
				new SaveDataToXML().SaveDesconDrawing(saveDialog.FileName, true);
				data.CurrentFilePath = saveDialog.FileName;
				fileSaved = true;
			}

			return fileSaved;
		}

		/// <summary>
		/// Opens a Descon file by bringing up the Open File dialog
		/// </summary>
		/// <returns>False if user cancelled open dialog</returns>
		internal bool OpenFile(string path, CommonData data)
		{
			bool canOpenFile = false;

			if (path != string.Empty && !File.Exists(path))
			{
				MessageBox.Show("File not found.", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
				return false;
			}

			var loadData = new LoadDataFromXML();

			MiscMethods.ResetReportLists();

			if (CommonDataStatic.DetailDataDict.Any(component => component.Value.IsActive) && IsSaveNeeded(data))
			{
				var result = MessageBox.Show("Save current drawing?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				if (result == MessageBoxResult.Yes)
				{
					if (SaveFile(data))
						canOpenFile = true;
				}
				else if (result == MessageBoxResult.No)
					canOpenFile = true;
			}
			else
				canOpenFile = true;

			if (!canOpenFile)
				return false;

			if (path == String.Empty)
			{
				var openDialog = new OpenFileDialog
				{
					AddExtension = true,
					DefaultExt = ConstString.FILE_EXTENSION_DRAWING,
					Filter = ConstString.FILE_DESCRIPTION_DRAWING + "|*" + ConstString.FILE_EXTENSION_DRAWING,
					FileName = data.CurrentFileName,
					InitialDirectory = data.CurrentFilePathOnly,
					Title = "Open Drawing"
				};

				if (openDialog.ShowDialog() != DialogResult.Cancel)
					path = openDialog.FileName;
				else
					return false;
			}

			string message;
			bool higherLevelLicenseRequired = false;
			var licenseType = loadData.LoadDesconDrawing(path, true).LicenseType;
			if (CommonDataStatic.LicenseType != ELicenseType.Developer_0 && CommonDataStatic.LicenseType != licenseType)
			{
				switch (CommonDataStatic.LicenseType)
				{
					case ELicenseType.Open_2:
						if (licenseType == ELicenseType.Basic_3 ||
						    licenseType == ELicenseType.Standard_4 ||
						    licenseType == ELicenseType.Next_5)
							higherLevelLicenseRequired = true;
						break;
					case ELicenseType.Basic_3:
						if (licenseType == ELicenseType.Standard_4 ||
						    licenseType == ELicenseType.Next_5)
							higherLevelLicenseRequired = true;
						break;
					case ELicenseType.Standard_4:
						if (licenseType == ELicenseType.Next_5)
							higherLevelLicenseRequired = true;
						break;
				}

				if (higherLevelLicenseRequired)
				{
					message = data.CommonLists.LicenseTypes[licenseType] + " license required to open this file.";
					MessageBox.Show(message, "LICENSE ERROR", MessageBoxButton.OK, MessageBoxImage.Warning);
					return false;
				}
			}

			if (CommonDataStatic.NeedToSaveMaterialsOrWelds)
			{
				var save = new SaveDataToXML();

				var result = MessageBox.Show("User-Specified materials or welds are used in this file that are not present on this installation of Descon. Do you wish to import these to this installation? If you select No, the new materials will be temporary.", "WARNING",
					MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
				if (result == MessageBoxResult.Yes)
				{
					save.SaveMaterials();
					save.SaveWelds();
				}
				else if (result == MessageBoxResult.Cancel)
					return false;

				CommonDataStatic.NeedToSaveMaterialsOrWelds = false;
			}

			data.CurrentFilePath = path;
			var file = loadData.LoadDesconDrawing(path, false);
			if (file == null)
				MessageBox.Show("Invalid File.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
			else
				data.MemberType = (EMemberType)(new CommonLists().MemberList.GetKey(0));

			data.FormControlShellPositionLeft = data.FormControlShellPositionTop = 0;

			data.ContextMenuRecentFiles = MiscMethods.GetRecentlyOpenedFileList();

			return true;
		}

		internal void RunBatchReports()
		{
			var folderBrowser = new FolderBrowserDialog();
			if (folderBrowser.ShowDialog() == DialogResult.OK)
			{
				Mouse.OverrideCursor = Cursors.Wait;

				var files = Directory.GetFiles(folderBrowser.SelectedPath);
				var newThread = new Thread(DoWork);
				newThread.SetApartmentState(ApartmentState.STA);
				newThread.Start(files);

				newThread.Join();	// Halts the calling thread since we want to block the user anyway

				Mouse.OverrideCursor = Cursors.Arrow;

				MessageBox.Show("All files have been processed and saved.", "Complete", MessageBoxButton.OK,
					MessageBoxImage.Information);
			}
		}

		private void DoWork(object files)
		{
			string html;
			var loadData = new LoadDataFromXML();
			var reportSave = new ReportSave();

			foreach (var path in (string[])files)
			{
				if (path.EndsWith(ConstString.FILE_EXTENSION_DRAWING))
				{
					var file = loadData.LoadDesconDrawing(path, false);
					if (file != null)
					{
						CommonDataStatic.LoadingFileInProgress = true;
						html = Calculations.Calculations.RunCalculationsSilently(file.DetailDataList);
						reportSave.SaveReport(html, path.TrimEnd(ConstString.FILE_EXTENSION_DRAWING.ToCharArray()) + ".txt", null, false);
						CommonDataStatic.LoadingFileInProgress = false;
					}
					new SaveDataToXML().SaveDesconDrawing(path, false);
				}
			}
		}

		/// <summary>
		/// This checks to see if there have been any new changes. What it does is get the hash of the current save file and compares it
		/// to a new temp save file. Returns true if the files match which is equivalent to a successful save.
		/// </summary>
		/// <returns>True if no need to save</returns>
		internal bool IsSaveNeeded(CommonData data)
		{
			string oldFileHash;
			string newFileHash;

			new SaveDataToXML().SaveDesconDrawing(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + data.CurrentFileName, true);

			if (File.Exists(data.CurrentFilePath) && File.Exists(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + data.CurrentFileName))
			{
				using (var md5 = MD5.Create())
				{
					using (var stream = File.OpenRead(data.CurrentFilePath))
					{
						oldFileHash = Encoding.UTF7.GetString(md5.ComputeHash(stream), 0, md5.ComputeHash(stream).Length);
					}
					using (var stream = File.OpenRead(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + data.CurrentFileName))
					{
						newFileHash = Encoding.UTF7.GetString(md5.ComputeHash(stream), 0, md5.ComputeHash(stream).Length);
					}
				}
				if (oldFileHash == newFileHash)
					return false;
			}
			return true;
		}

		#endregion

		internal void OpenConnectionForm(EMemberSubType selectedTab, CommonData data)
		{
			Window form;
			UIElement controlForTab0 = null;
			UIElement controlForTab1 = null;
			string titleForTab0 = String.Empty;
			string titleForTab1 = String.Empty;

			try
			{
				if (CommonDataStatic.IsFema)
				{
					controlForTab0 = new ControlFema(data);
					titleForTab0 = "FEMA - 350 Connections";
					Fema.SetFemaVariables(EMemberType.PrimaryMember);
				}
				else
				{
					switch (data.SelectedMember.ShearConnection)
					{
						case EShearCarriedBy.ClipAngle:
							controlForTab0 = new ControlWCShearClipAngle(ref data);
							titleForTab0 = "Web Connection - Clip Angle";
							break;
						case EShearCarriedBy.Tee:
							controlForTab0 = new ControlWCShearTee(ref data);
							titleForTab0 = "Web Connection - Tee";
							break;
						case EShearCarriedBy.SinglePlate:
							controlForTab0 = new ControlWCShearWebPlate(ref data);
							titleForTab0 = "Single Plate - Shear Only";
							break;
						case EShearCarriedBy.EndPlate:	// End Plate Moment has no corresponding shear form
							if (data.SelectedMember.MomentConnection == EMomentCarriedBy.EndPlate)
							{
								controlForTab0 = new ControlWCMomentEndPlate(ref data);
								titleForTab0 = "Moment & Shear Connection - End Plate";
							}
							else
							{
								controlForTab0 = new ControlWCShearEndPlate(ref data);
								titleForTab0 = "End Plate - Shear Only";
							}
							break;
						case EShearCarriedBy.Seat:
							controlForTab0 = new ControlWCShearSeat(ref data);
							titleForTab0 = "Shear Connection - Seated";
							break;
					}
				}

				switch (data.SelectedMember.MomentConnection)
				{
					case EMomentCarriedBy.FlangePlate:
						if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
						{
							controlForTab1 = new ControlWCMomentDirectlyWelded(ref data);
							titleForTab1 = "Moment Connection to Column Web";
						}
						else
						{
							controlForTab1 = new ControlWCMomentFlangePlate(ref data);
							titleForTab1 = "Flange Connection - Plates";
						}
						break;
					case EMomentCarriedBy.Tee:
						controlForTab1 = new ControlWCMomentTee(ref data);
						titleForTab1 = "Flange Connection - Tee's";
						break;
					case EMomentCarriedBy.Angles:
						controlForTab1 = new ControlWCMomentFlangeAngle(ref data);
						titleForTab1 = "Flange Connection - Angles";
						break;
					case EMomentCarriedBy.DirectlyWelded:
						if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
						{
							controlForTab1 = new ControlWCMomentDirectlyWelded(ref data);
							titleForTab1 = "Moment Connection to Column Web";
						}
						break;
				}

				form = new FormControlShell(data, controlForTab0, controlForTab1, titleForTab0, titleForTab1, selectedTab);

				form.ShowDialog();
			}
			catch
			{
				// Do Nothing
			}
		}

		internal void OpenMoreData(CommonData data)
		{
			FormControlShell form = null;

			switch (data.SelectedMember.BraceMoreDataSelection)
			{
				case EBraceConnectionTypes.SinglePlate:
					form = new FormControlShell(data, new ControlWCShearWebPlate(ref data), "Single Plate");
					break;
				case EBraceConnectionTypes.EndPlate:
					form = new FormControlShell(data, new ControlWCShearEndPlate(ref data), "End Plate");
					break;
				case EBraceConnectionTypes.FabricatedTee:
					form = new FormControlShell(data, new ControlBCFabricatedTee(data), "Fabricated Tee");
					break;
				case EBraceConnectionTypes.ClipAngle:
					form = new FormControlShell(data, new ControlBCClipAngle(ref data), "Clip Angle");
					break;
				case EBraceConnectionTypes.ClawAngle:
					form = new FormControlShell(data, new ControlBCClawAngles(ref data), "Claw Angles");
					break;
				case EBraceConnectionTypes.GussetPlate:
					form = new FormControlShell(data, new ControlBCGussetPlate(ref data), "Gusset Plate");
					break;
				case EBraceConnectionTypes.SplicePlate:
					form = new FormControlShell(data, new ControlBCSplicePlates(ref data), "Splice Plate");
					break;
				case EBraceConnectionTypes.Brace:
					form = new FormControlShell(data, new ControlBCBrace(ref data), "Brace");
					break;
				case EBraceConnectionTypes.BasePlate:
					form = new FormControlShell(data, new ControlBCBasePlate(), "Base Plate");
					break;
			}

			if (form != null)
				form.ShowDialog();
		}

		/// <summary>
		/// Takes a screenshot and saves it to disk. Coordinates and size are passed in to only capture the form.
		/// </summary>
		/// <param name="width">Width of the form</param>
		/// <param name="height">Height of the form</param>
		/// <param name="left">Left side position</param>
		/// <param name="top">Top position</param>
		/// <param name="forBugReport">If this is for the bug report, a message will not be shown and the screenshot will be saved to the Bug Report folder</param>
		public static void TakeScreenshot(double width, double height, double left, double top, bool forBugReport)
		{
			string screenshotName;

			using (var bitmap = new Bitmap((int)width, (int)height))
			{
				using (Graphics g = Graphics.FromImage(bitmap))
				{
					g.CopyFromScreen(new Point((int)left, (int)top), new Point(), new Size((int)width, (int)height));
				}

				if (forBugReport)
					screenshotName = ConstString.FOLDER_DESCONDATA_BUG_REPORT + ConstString.FILE_SCREENSHOT;
				else
					screenshotName = ConstString.FOLDER_MYDOCUMENTS_SCREENSHOTS + "(" + DateTime.Now.ToString("MM-dd-yy_HH-mm-ss") + ") " + ConstString.FILE_SCREENSHOT;

				bitmap.Save(screenshotName, ImageFormat.Png);

				if (!forBugReport)
					MessageBox.Show("Screenshot saved to:\r\r" + screenshotName, "Screenshot Saved", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}
	}
}