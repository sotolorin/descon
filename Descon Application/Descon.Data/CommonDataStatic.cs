using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;

namespace Descon.Data
{
	// To avoid making CommonData as a whole static, I have moved information that is needed throughout the software to these
	// static properties. They can still be accessed through CommonData, but those properties will actually call these. 
	// 
	// IMPORTANT: If these properties are not updated through CommonData their data won't be up to date!
	/// <summary>
	/// Static data that can be used as a shortcut if CommonData isn't available
	/// </summary>
	public static class CommonDataStatic
	{
		public static Process UnityProcess;

		/// <summary>
		/// Server connection to Unity
		/// </summary>
		public static NamedPipeServerStream Server;

		/// <summary>
		/// Determines if the Unity process is suspended
		/// </summary>
		public static bool IsProcessSuspended;

		/// <summary>
		/// Tells the entire application we are opening a file to avoid executing certain code paths.
		/// </summary>
		public static bool LoadingFileInProgress;

		/// <summary>
		/// Determines when Unity is done saving
		/// </summary>
		public static bool UnityDoneUpdating;

		/// <summary>
		/// Determines when Unity is done saving
		/// </summary>
		public static bool UnityDoneSaving;

		/// <summary>
		/// Used to determine if we need to tell Unity a new member has been added so it can zoom to fit the views
		/// </summary>
		public static bool UnityNewMemberAdded;

		/// <summary>
		/// Determines when Unity is done creating the images
		/// </summary>
		public static bool UnityDoneCreatingImage;

		/// <summary>
		/// If a save file contains materials not in the program we will ask to save them as user materials
		/// </summary>
		public static bool NeedToSaveMaterialsOrWelds;

		public static Dictionary<EMemberType, DetailData> DetailDataDict;

		public static string CurrentFileName;
		public static string CurrentFilePath;

		public static ColumnSplice ColumnSplice;
		public static ColumnStiffener ColumnStiffener;
		public static SeismicSettings SeismicSettings;

		public static List<GaugeData> GaugeData;

		public static DetailData SelectedMember;

		public static Preferences Preferences;

		public static List<List<double>> EccentricWeldCoefficients;

		public static string CompanyName;
		public static ELicenseType LicenseType;
		public static string LicenseTypeDisplay;

		public static bool IsReportOpen; // Used so we don't request the Unity drawing png when not needed

		/// <summary>
		/// ONLY should be used for the menu setting, not the internal setting
		/// </summary>
		public static EJointConfiguration JointConfig { get; set; }

		/// <summary>
		/// This is the Joint Config value that should be used for calculations since it changes depending on certain settings.
		/// </summary>
		public static EJointConfiguration BeamToColumnType
		{
			get
			{
				if (DetailDataDict == null)
					return JointConfig;
				else if (JointConfig == EJointConfiguration.BraceToColumn && !MiscMethods.IsAnyBraceActive())
				{
					if (DetailDataDict[EMemberType.PrimaryMember].ShapeType == EShapeType.HollowSteelSection)
						return EJointConfiguration.BeamToHSSColumn;
					else if (DetailDataDict[EMemberType.PrimaryMember].WebOrientation == EWebOrientation.InPlane)
						return EJointConfiguration.BeamToColumnFlange;
					else if (DetailDataDict[EMemberType.PrimaryMember].WebOrientation == EWebOrientation.OutOfPlane)
						return EJointConfiguration.BeamToColumnWeb;
					else
						return JointConfig;
				}
				else
					return JointConfig;
			}
		}

		// The following are properties in Preferences that need to be used often so we are creating static shortcuts
		/// <summary>
		/// Alias to Preferences setting for Metric or US unit system. If we haven't set preferences yet, just return US
		/// </summary>
		public static EUnit Units
		{
			get { return Preferences != null ? Preferences.Units : EUnit.US; }
		}

		/// <summary>
		/// Determines if the current Joint Configuration is FEMA
		/// </summary>
		public static bool IsFema
		{
			get
			{
				if (Preferences.Seismic == ESeismic.Seismic && SelectedMember.WinConnect.Fema.Connection != EFemaConnectionType.None)
					return true;
				else
					return false;
			}
		}

		#region Licensing

		/// <summary>
		/// Determines if the License Type is at minimum the Basic license
		/// </summary>
		public static bool LicenseMinimumBasic
		{
			get
			{
				switch (LicenseType)
				{
					case ELicenseType.Developer_0:
					case ELicenseType.Demo_1:
					case ELicenseType.Basic_3:
					case ELicenseType.Standard_4:
					case ELicenseType.Next_5:
						return true;
					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Determines if the License Type is at minimum the Standar license
		/// </summary>
		public static bool LicenseMinimumStandard
		{
			get
			{
				switch (LicenseType)
				{
					case ELicenseType.Developer_0:
					case ELicenseType.Standard_4:
					case ELicenseType.Next_5:
						return true;
					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Determines if the License Type is at minimum the Next license
		/// </summary>
		public static bool LicenseMinimumNext
		{
			get
			{
				switch (LicenseType)
				{
					case ELicenseType.Developer_0:
					case ELicenseType.Next_5:
						return true;
					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Determines if the License Type is Developer
		/// </summary>
		public static bool LicenseMinimumDeveloper
		{
			get { return LicenseType == ELicenseType.Developer_0; }
		}

		#endregion

		#region Shape and Object Lists

		/// <summary>
		/// Every shape in the system
		/// </summary>
		public static Dictionary<string, Shape> AllShapes { get; set; }
		
		/// <summary>
		/// All Single Angle shapes sorted by size from smallest to largest set in MiscMethods.ReloadAngleShapeLists();
		/// </summary>
		public static Dictionary<string, Shape> ShapesSingleAngle { get; set; }

		/// <summary>
		/// All Single Angle shapes sorted by size from smallest to largest
		/// </summary>
		public static Dictionary<string, Shape> ShapesSingleAngleNoNone { get; set; }

		/// <summary>
		/// All Double Angle shapes sorted by size from smallest to largest
		/// </summary>
		public static Dictionary<string, Shape> ShapesDoubleAngle { get; set; }

		/// <summary>
		/// All WT (Tee) shapes sorted by size from smallest to largest
		/// </summary>
		public static Dictionary<string, Shape> ShapesTee { get; set; }

		/// <summary>
		/// All Channel shapes sorted by size from smallest to largest
		/// </summary>
		public static Dictionary<string, Shape> ShapesSingleChannel { get; set; }

		/// <summary>
		/// List of all materials
		/// </summary>
		public static Dictionary<string, Material> MaterialDict { get; set; }

		/// <summary>
		/// List of all Welds/Electrodes
		/// </summary>
		public static Dictionary<string, Weld> WeldDict { get; set; }

		#endregion

		#region Report Data

		/// <summary>
		/// Complete list of report lines created by each calculation for the Detail Report. See Descon.DataAccess/BuildAndSaveReport.cs for examples
		/// </summary>
		public static readonly List<ReportLine> DetailReportLineList = new List<ReportLine>();

		public static List<string> ReportBookmarkList;
		public static List<string> ReportHighlightList;
		public static Dictionary<string, string> ReportCommentList;
		public static List<string> ReportNoGoodList;
		public static List<string> ReportCapacityList;
		public static Dictionary<string, string> ReportGoToList;
		public static Dictionary<string, double> ReportJavascriptVarList;

		public static CommonLists CommonLists = new CommonLists();

		#endregion

		#region Global Properties in the old Descon 7 code

		public static double F_Shear;
		public static double WeldBetaL;
		public static double MinThickness;
		public static double Minth;
	    public static double PryingForce;
		public static bool LShapedWeld;

		#endregion
	}
}
