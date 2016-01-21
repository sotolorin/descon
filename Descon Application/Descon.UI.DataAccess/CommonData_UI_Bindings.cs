using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Descon.Data;

namespace Descon.UI.DataAccess
{
	public partial class CommonData
	{
		private Dictionary<string, string> _contextMenuRecentFiles;

		public bool IsMomentEnabled
		{
			get
			{
				if (CommonDataStatic.LicenseType == ELicenseType.Open_2 ||
					MomentCarriedByList.Count < 2 ||
					CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice ||
					(CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].IsActive ||
				    CommonDataStatic.DetailDataDict[EMemberType.LowerRight].IsActive ||
				    CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].IsActive ||
				    CommonDataStatic.DetailDataDict[EMemberType.UpperRight].IsActive))
					return false;
				else
					return true;
			}
		}

		public bool IsMomentEnabledForColumn
		{
			get
			{
				if (CommonDataStatic.SelectedMember.MemberType == EMemberType.PrimaryMember &&
					CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn &&
					(CommonDataStatic.DetailDataDict[EMemberType.RightBeam].MomentConnection != EMomentCarriedBy.NoMoment ||
					CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].MomentConnection != EMomentCarriedBy.NoMoment))
					return true;
				else
					return false;
			}
		}

		public bool IsCurrentMemberEnabled
		{
			get
			{
				switch (CommonDataStatic.JointConfig)
				{
					case EJointConfiguration.BraceToColumn:
						// If the Primary Member is not active and the member is not the primary member
						if (!CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].IsActive && MemberType != EMemberType.PrimaryMember)
							return false;
						// Right Beam is not active and member is a right side member
						if (!CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive &&
						    (MemberType == EMemberType.LowerLeft || MemberType == EMemberType.UpperLeft))
							return false;
						// Left Beam is not active and member is a left side member
						if (!CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive &&
						    (MemberType == EMemberType.LowerRight || MemberType == EMemberType.UpperRight))
							return false;
						// If the member is a brace, but we have a moment connection
						if (MiscMethods.IsBrace(MemberType) &&
						    (CommonDataStatic.DetailDataDict[EMemberType.RightBeam].MomentConnection != EMomentCarriedBy.NoMoment ||
						     CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].MomentConnection != EMomentCarriedBy.NoMoment))
							return false;
						return true;
					case EJointConfiguration.BraceVToBeam:
					case EJointConfiguration.BraceToColumnBase:
					case EJointConfiguration.BeamToGirder:
					case EJointConfiguration.ColumnSplice:
					case EJointConfiguration.BeamSplice:
						return true;
					default:
						return true;
				}
			}
		}

		public Dictionary<string, string> ContextMenuRecentFiles
		{
			get { return _contextMenuRecentFiles; }
			set
			{
				_contextMenuRecentFiles = value;
				OnPropertyChanged("ContextMenuRecentFiles");
			}
		}

		#region UI Font and Color Data

		public SolidColorBrush ColorDrawingColumns
		{
			get { return new BrushConverter().ConvertFromString(Preferences.ColorSettings.Columns) as SolidColorBrush; }
			set
			{
				Preferences.ColorSettings.Columns = value.Color.ToString();
				OnPropertyChanged("ColorDrawingColumns");
			}
		}

		public SolidColorBrush ColorDrawingBeamsBraces
		{
			get { return new BrushConverter().ConvertFromString(Preferences.ColorSettings.BeamsBraces) as SolidColorBrush; }
			set
			{
				Preferences.ColorSettings.BeamsBraces = value.Color.ToString();
				OnPropertyChanged("ColorDrawingBeamsBraces");
			}
		}

		public SolidColorBrush ColorDrawingDimensionLinesLeaders
		{
			get { return new BrushConverter().ConvertFromString(Preferences.ColorSettings.DimensionLinesLeaders) as SolidColorBrush; }
			set
			{
				Preferences.ColorSettings.DimensionLinesLeaders = value.Color.ToString();
				OnPropertyChanged("ColorDrawingDimensionLinesLeaders");
			}
		}

		public SolidColorBrush ColorDrawingBolts
		{
			get { return new BrushConverter().ConvertFromString(Preferences.ColorSettings.Bolts) as SolidColorBrush; }
			set
			{
				Preferences.ColorSettings.Bolts = value.Color.ToString();
				OnPropertyChanged("ColorDrawingBolts");
			}
		}

		public SolidColorBrush ColorDrawingWeldSymbols
		{
			get { return new BrushConverter().ConvertFromString(Preferences.ColorSettings.WeldSymbols) as SolidColorBrush; }
			set
			{
				Preferences.ColorSettings.WeldSymbols = value.Color.ToString();
				OnPropertyChanged("ColorDrawingWeldSymbols");
			}
		}

		public SolidColorBrush ColorDrawingText
		{
			get { return new BrushConverter().ConvertFromString(Preferences.ColorSettings.Text) as SolidColorBrush; }
			set
			{
				Preferences.ColorSettings.Text = value.Color.ToString();
				OnPropertyChanged("ColorDrawingText");
			}
		}

		public SolidColorBrush ColorDrawingConnectionElements
		{
			get { return new BrushConverter().ConvertFromString(Preferences.ColorSettings.ConnectionElements) as SolidColorBrush; }
			set
			{
				Preferences.ColorSettings.ConnectionElements = value.Color.ToString();
				OnPropertyChanged("ColorDrawingConnectionElements");
			}
		}

		public SolidColorBrush ColorDrawingHighlight
		{
			get { return new BrushConverter().ConvertFromString(Preferences.ColorSettings.Highlight) as SolidColorBrush; }
			set
			{
				Preferences.ColorSettings.Highlight = value.Color.ToString();
				OnPropertyChanged("ColorDrawingHighlightd");
			}
		}

		public SolidColorBrush ColorDrawingBackground
		{
			get { return new BrushConverter().ConvertFromString(Preferences.ColorSettings.Background) as SolidColorBrush; }
			set
			{
				Preferences.ColorSettings.Background = value.Color.ToString();
				OnPropertyChanged("ColorDrawingBackground");
			}
		}

		#endregion

		#region UI Element Bindings - Changes the look of the UI and some of the text depending on certain conditions

		public string UpdateDownloadStatus
		{
			get { return _updateDownloadStatus; }
			set
			{
				_updateDownloadStatus = value;
				OnPropertyChanged("UpdateDownloadStatus");
			}
		}

		public WindowState WindowState
		{
			get { return _windowState; }
			set
			{
				_windowState = value;
				CommonDataStatic.Preferences.IsWindowMaximized = value == WindowState.Maximized;
			}
		}

		public string OrientationLabel
		{
			get
			{
				switch (ShapeType)
				{
					case EShapeType.HollowSteelSection:
						return "Long Side";
					case EShapeType.SingleAngle:
					case EShapeType.DoubleAngle:
						return "Long Leg";
					default:
						return "Web Orientation";
				}
			}
		}

		/// <summary>
		/// Returns true if we are using the Beam Splice config
		/// </summary>
		public bool IsBeamSpliceConfig
		{
			get { return CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice; }
		}

		public bool IsStiffenerButtonEnabled
		{
			get
			{
				return CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice &&
				       CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToGirder;
			}
		}

		/// <summary>
		/// Returns true if we are using the Beam to Girder config
		/// </summary>
		public bool IsWebOrientationEnabled
		{
			get
			{
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder || IsBeam)
					return false;
				else
					return true;
			}
		}

		/// <summary>
		/// Determines whether the splice panel on the main form is enabled
		/// </summary>
		public Visibility IsColumnSpliceConfig
		{
			get { return CommonDataStatic.BeamToColumnType == EJointConfiguration.ColumnSplice ? Visibility.Visible : Visibility.Collapsed; }
		}

		/// <summary>
		/// Will return Visible if the current member is not a column
		/// </summary>
		public bool IsShearVisible
		{
			get { return MemberType == EMemberType.PrimaryMember && CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToGirder; }
		}

		/// <summary>
		/// Passes back the visible state when the selected member is a beam
		/// </summary>
		public bool IsBeam
		{
			get { return MiscMethods.IsBeam(MemberType) && CommonDataStatic.BeamToColumnType != EJointConfiguration.ColumnSplice; }
		}

		/// <summary>
		/// Passes back the visible state when the selected member is a beam
		/// </summary>
		public Visibility IsShearFieldVisible
		{
			get
			{
				if ((JointConfig == EJointConfiguration.BeamToGirder && IsBeam) ||
					(JointConfig != EJointConfiguration.BeamToGirder && (IsBeam || MemberType == EMemberType.PrimaryMember)))
					return Visibility.Visible;
				else
					return Visibility.Collapsed;
			}
		}

		/// <summary>
		/// Returns true if the Left Beam is active
		/// </summary>
		public bool IsLeftBeamActive
		{
			get { return CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive; }
		}

		/// <summary>
		/// Returns true if the Right Beam is active
		/// </summary>
		public bool IsRightBeamActive
		{
			get { return CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive; }
		}

		/// <summary>
		/// Passes back the visible state when the selected member is a brace
		/// </summary>
		public Visibility IsBrace
		{
			get { return MiscMethods.IsBrace(MemberType) ? Visibility.Visible : Visibility.Collapsed; }
		}

		public bool IsAxialForceEnabled
		{
			get
			{
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
					return false;
				else if (CommonDataStatic.LicenseMinimumStandard)
					return true;
				else if (CommonDataStatic.DetailDataDict[EMemberType.RightBeam].MomentConnection != EMomentCarriedBy.NoMoment &&
				         CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].MomentConnection != EMomentCarriedBy.NoMoment)
					return true;
				else
					return false;
			}
		}

		public bool IsTransferForceEnabled
		{
			get
			{
				if (MiscMethods.IsBrace(MemberType) &&
				    CommonDataStatic.Preferences.InputForceType == EPrefsInputForce.TransferForce &&
				    (CommonDataStatic.DetailDataDict[EMemberType.RightBeam].MomentConnection != EMomentCarriedBy.NoMoment ||
				     CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].MomentConnection != EMomentCarriedBy.NoMoment))
					return true;
				else
					return false;
			}
		}

		#endregion

		#region License Related Bindings

		public bool IsNotDemo
		{
			get { return CommonDataStatic.LicenseType != ELicenseType.Demo_1; }
		}

		/// <summary>
		/// Determines if the License Type is at minimum the Basic license
		/// </summary>
		public bool LicenseMinimumBasic
		{
			get { return CommonDataStatic.LicenseMinimumBasic; }
		}

		/// <summary>
		/// Determines if the License Type is at minimum the Standar license
		/// </summary>
		public bool LicenseMinimumStandard
		{
			get { return CommonDataStatic.LicenseMinimumStandard; }
		}

		/// <summary>
		/// Determines if the License Type is at minimum the Next license
		/// </summary>
		public bool LicenseMinimumNext
		{
			get { return CommonDataStatic.LicenseMinimumNext; }
		}

		/// <summary>
		/// Determines if the License Type is at minimum the Next license
		/// </summary>
		public bool LicenseMinimumDeveloper
		{
			get { return CommonDataStatic.LicenseMinimumDeveloper; }
		}

		#endregion
	}
}