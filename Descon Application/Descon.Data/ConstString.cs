using System;

namespace Descon.Data
{
	/// <summary>
	/// String constants for things like file names and modes. Some change according to CalcMode and Unit. Some about and help screen
	/// text is also found here.
	/// </summary>
	public static class ConstString
	{
		private static bool IsASD
		{
			get { return CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD; }
		}

		private static bool IsFema350
		{
			get { return CommonDataStatic.IsFema; }
		}

		/// <summary>
		/// Current Preferences Version used to keep track if we need to replace the file. Increment to have the file replaced.
		/// </summary>
		public const string PREFERENCES_VERSION = "8.85";

		#region Unity Message Strings

		public const string UNITY_PIPE_NAME_SEND = "DesconPipe";
		public const string UNITY_PIPE_NAME_RECEIVE = "UnityPipe";
		public const string UNITY_UPDATE = "UPDATE";
		public const string UNITY_UPDATE_DONE = "UPDATE_DONE";
		public const string UNITY_NEW_DRAWING = "NEW_DRAWING";
		public const string UNITY_NEW_MEMBER_ADDED = "NEW_MEMBER_ADDED";
		public const string UNITY_USER_SAVED = "USER_SAVED";
		public const string UNITY_DONE_SAVING = "DONE_SAVING";
		public const string UNITY_PREFERENCES_UPDATE = "PREFERENCES_UPDATES";
		public const string UNITY_ZOOM_TO_FIT = "ZOOM_TO_FIT";
		public const string UNITY_ZOOM_TO_FIT_SELECTED = "ZOOM_TO_FIT_SELECTED";
		public const string UNITY_CREATE_IMAGE = "CREATE_IMAGE";
		public const string UNITY_CREATE_IMAGE_DONE = "CREATE_IMAGE_DONE";
		public const string UNITY_JOINT_CONFIG_CHANGE = "JOINT_CONFIG_CHANGE";

		#endregion

		#region Generic UI Strings

		public const string DESCON_EXE = "Descon.exe";

		public const string METRIC = "Metric";
		public const string US = "US";

		public const string NONE = "None";

		public const string DEFAULT_FONT_NAME = "Segoe UI";
		public const string DEFAULT_FONT_SIZE = "Small";

		public const string HSS_MATERIAL = "A1085";

		#endregion

		#region Connection Types and other drop down strings

		public const string CON_DIRECTLY_WELDED = "Directly Welded";
		public const string CON_CLIP_ANGLE = "Clip Angle";
		public const string CON_CLAW_ANGLE = "Claw Angle";
		public const string CON_GUSSET_PLATE = "Gusset Plate";
		public const string CON_SPLICE_PLATE = "Splice Plate";
		public const string CON_BRACE = "Brace";
		public const string CON_SINGLE_PLATE = "Single Plate";
		public const string CON_SINGLE_PLATE_EXTENDED = "Extended Single Plate";
		public const string CON_END_PLATE = "End Plate";
		public const string CON_FABRICATED_TEE = "Fabricated Tee";
		public const string CON_BASE_PLATE = "Base Plate";
		public const string CON_TEE = "Tee";
		public const string CON_SEAT = "Seat";

		public const string CON_NO_MOMENT = "No Moment";
		public const string CON_FLANGE_PLATE = "Flange Plate";
		public const string CON_ANGLES = "Angles";

		public static string MEMBER_COLUMN_OR_BEAM
		{
			get
			{
				switch (CommonDataStatic.JointConfig)
				{
					case EJointConfiguration.BraceVToBeam:
						return "Beam";
					case EJointConfiguration.BeamToGirder:
						return "Girder";
					default:
						return "Column";
				}
			}
		}

		public static string MEMBER_RIGHT_SIDE_BEAM
		{
			get { return CommonDataStatic.BeamToColumnType == EJointConfiguration.ColumnSplice ? "Top Column" : "Right Side Beam"; }
		}

		public static string MEMBER_LEFT_SIDE_BEAM
		{
			get { return CommonDataStatic.BeamToColumnType == EJointConfiguration.ColumnSplice ? "Bottom Column" : "Left Side Beam"; }
		}

		public const string MEMBER_UPPER_RIGHT_BRACE = "Upper Right Brace";
		public const string MEMBER_LOWER_RIGHT_BRACE = "Lower Right Brace";
		public const string MEMBER_UPPER_LEFT_BRACE = "Upper Left Brace";
		public const string MEMBER_LOWER_LEFT_BRACE = "Lower Left Brace";
		public const string MEMBER_GIRDER = "Girder";

		public static string JOINT_CONFIG_BRACE_TO_COLUMN
		{
			get { return CommonDataStatic.LicenseMinimumStandard ? "Beam and/or Brace to Column" : "Beam to Column"; }
		}

		public const string JOINT_CONFIG_V_BRACE_TO_BEAM = "V Brace to Beam";
		public const string JOINT_CONFIG_BRACE_TO_COLUMN_BASE = "Brace to Column Base";
		public const string JOINT_CONFIG_BEAM_TO_COLUMN_FLANGE = "Beam to Column Flange";
		public const string JOINT_CONFIG_BEAM_TO_COLUMN_WEB = "Beam to Column Web";
		public const string JOINT_CONFIG_BEAM_TO_HSS_COLUMN = "Beam to HSS Column";
		public const string JOINT_CONFIG_COLUMN_SPLICE = "Column Splice";
		public const string JOINT_CONFIG_BEAM_TO_GIRDER = "Beam to Girder";
		public const string JOINT_CONFIG_BEAM_SPLICE = "Beam Splice";

		public const string UI_BOTTOM_TO_TOP = "Bottom of Beam to Top Bolt Dist.";
		public const string UI_TOP_TO_TOP = "Top of Beam to Top Bolt Dist.";

		#endregion

		#region Calculation Related Strings

		public const string YOUNGS_MODULUS_E = "E";

	    public static string PHI
	    {
			get { return CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD ? "(1 / FS)" : "Φ"; }
	    }

		public static string FIOMEGA0_9
		{
			get { return (IsASD && !IsFema350) ? "(1 / 1.67)" : "0.9"; }
		}

		public static string FIOMEGA0_75
		{
			get { return (IsASD && !IsFema350) ? "(1 / 2.0)" : "0.75"; }
		}

		public static string FIOMEGA1_0
		{
			get { return (IsASD && !IsFema350) ? "(1 / 1.50)" : "1.0"; }
		}

		public static string FIOMEGA0_95
		{
			get { return (IsASD && !IsFema350) ? "(1 / 1.58)" : "0.95"; }
		}

	    public static string DES_OR_ALLOWABLE
	    {
            get { return CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD ? "Allowable" : "Design"; }
	    }

	    public static string DES_OR_ALLOWABLE_STRENGTH
	    {
            get { return CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD ? "Allowable Strength" : "Design Strength"; }
	    }

		#endregion

		#region File and directory names

		// Folders in the user area of Windows
		public static readonly string FOLDER_MYDOCUMENTS_DESCON = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Descon\";
		public static readonly string FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA = FOLDER_MYDOCUMENTS_DESCON + @"ProgramData\";
		public static readonly string FOLDER_MYDOCUMENTS_SCREENSHOTS = FOLDER_MYDOCUMENTS_DESCON + @"Screenshots\";
		public static readonly string FOLDER_DESCONDATA_BUG_REPORT = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + @"Feedback Report\";
		public static readonly string FOLDER_DESCONDATA_THEMES = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + @"Themes\";
		public static readonly string FILE_INSTALLER = FOLDER_MYDOCUMENTS_DESCON + "Descon Setup.exe";

		// Various files used in Descon
        public static string FOLDER_LOCATION = "C:\\Descon\\";
		public const string FILE_EXTENSION_DRAWING = ".dsn";
		public const string FILE_EXTENSION_PROJECT = ".dpj";
		public const string FILE_DEFAULT_NAME = "Drawing" + FILE_EXTENSION_DRAWING;
		public const string FILE_SCREENSHOT = "Screenshot.png";
		public const string FILE_FEEDBACK_REPORT_ZIP = "Feedback.zip";
		public static readonly string FILE_REPORT_HTML = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "Report.html";
		public static readonly string FILE_PDFHEADER_HTML = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "pdfHeader.html";
		public static readonly string FILE_REPORT_RTF = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "Report.rtf";
		public static readonly string FILE_BUG_REPORT_TEXT = FOLDER_DESCONDATA_BUG_REPORT + "BugReport.txt";
		public static readonly string FILE_BUG_REPORT_HTML = FOLDER_DESCONDATA_BUG_REPORT + "Report.html";
		public static readonly string FILE_BUG_REPORT_SAVE = FOLDER_DESCONDATA_BUG_REPORT + FILE_DEFAULT_NAME;
		public static readonly string FILE_BUG_REPORT_PREFERENCES = FOLDER_DESCONDATA_BUG_REPORT + FILE_DEFAULT_NAME;
		public static readonly string FILE_LOGO = FOLDER_MYDOCUMENTS_DESCON + "logo.png";
		public static readonly string FILE_EULA = FOLDER_MYDOCUMENTS_DESCON + "Descon EULA.rtf";
		public static readonly string FILE_CHANGELOG = FOLDER_MYDOCUMENTS_DESCON + "ChangeLog.rtf";
		public static readonly string FILE_BOOKMARK_PNG = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "bookmark.png";
		public static readonly string FILE_EDIT_TEXT_PNG = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "edit_text.png";
		public static readonly string FILE_TEMP_SAVE = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "temp" + FILE_EXTENSION_DRAWING;

		// HTML to PDF converter exe
		public const string FILE_HTML_TO_PDF_UTILITY = "html_to_pdf.exe";

		// The following are for Unity
		public static readonly string FILE_UNITY_DRAWING = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "unity_drawing" + FILE_EXTENSION_DRAWING; // Descon drawing file passed to Unity
		public static readonly string FILE_UNITY_DIMENSIONS = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "dimensions.xml";
		// Screenshots of the drawing from Unity for report
		public static readonly string FILE_UNITY_IMAGE_ALL = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "drawing_all.png";
		public static readonly string FILE_UNITY_IMAGE_LEFT = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "drawing_left.png";
		public static readonly string FILE_UNITY_IMAGE_RIGHT = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "drawing_right.png";
		public static readonly string FILE_UNITY_IMAGE_TOP = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "drawing_top.png";
		public static readonly string FILE_UNITY_IMAGE_FRONT = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "drawing_front.png";
		public static readonly string FILE_UNITY_IMAGE_3D = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "drawing_3d.png";

		public const string FILE_DESCRIPTION_DRAWING = "Descon Drawing";
		public const string FILE_DESCRIPTION_PROJECT = "Descon Project";

		// These are used in the Project Manager solution
		public const string FILE_PROJECT_EXTENSION = ".dsj";
		public const string FILE_PROJECT_DEFAULT_NAME = "DesconProject" + FILE_PROJECT_EXTENSION;

		// The following files are necessary for the application to run and are saved to the hard drive
		public static readonly string FILE_PREFERENCES = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "Preferences.xml";
		public static readonly string FILE_PREFERENCES_MANAGER = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "Preferences_Manager.xml";
		public static readonly string FILE_DATA_FILE_VERSION = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "DataFileVersion.dat"; // Determines if the Shape, Material, and Weld files need to be replaced
		public static readonly string FILE_PREFERENCES_VERSION = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "PreferencesVersion.dat"; // Determines if the Preferences need to be reset and replaced
		public static readonly string FILE_USER_SHAPES = FOLDER_MYDOCUMENTS_DESCON + "User_Shapes.xml";
		public static readonly string FILE_USER_MATERIALS = FOLDER_MYDOCUMENTS_DESCON + "User_Materials.xml";
		public static readonly string FILE_USER_WELDS = FOLDER_MYDOCUMENTS_DESCON + "User_Welds.xml";
		public static readonly string FILE_LICENSE = FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "License.dat";

		public const string FILE_DETAIL_REPORT = "Detail Calculation Report";

		#endregion

		#region Report Strings

		public const string REPORT_NOGOOD = "(NG)";
		public const string REPORT_OK = "(OK)";
		public const string REPORT_NORMAL_LINE = "_N_";
		// These are used when we want (OK) or (NG) on a line to not automtically trigger the green and red fonts
		public const string REPORT_NOGOOD_SPECIAL = "(N_G)";
		public const string REPORT_OK_SPECIAL = "(O_K)";
		// Used for the Project Manager when displaying multiple reports in one document
		public const string REPORT_INDEX = "_INDEX_";

		#endregion

		public const string BEAM_SPLICE_WARNING = "Descon does not handle the moment splice of different depth beams.";

		public const string HELP_BRACE_PREFERENCES =
@"Here you can specify the distance parallel to the brace axis
from the end of the brace to the end of the Gusset/Beam weld.

You can either type in a number (inches) or select from the list
2t, 3t, or 4t, where t is the thickness of the gusset plate.

To pull back the brace end from the hard point on the column side
change the Brace to W.P. distance.";

		public const string HELP_BOLT_STAGGER =
@"Support Bolts Lower and Beam Bolts Lower
only work with equal number of rows on support and beam sides.

'One Less Row' is automatically selected if the number of rows 
on either side is changed to one less than the other side.";

		public const string HELP_DESIGN_WEB_SPLICE =
@"Cmin = Minimum compressive column force at the splice.

Cmin should not be greater than 75% of column compressive
force due to DEAD LOAD only.

Cmin is used to calculate the frictional resistance to shear at the
bearing surface. Coefficient of friction is assumed to be 0.33.

Cmin is also used to calculate the tensile force on the tension side
for combined axial compression and moment.

If T is greater than zero, Vs will be used regardless of what you
select or what Cmin value is.

When using Seismic Provisions and Framing System is SMF,
V-Req entered above is used.";

		public const string HELP_FEMA_CONNECTION_DETAIL =
@"L = Length of beam from center line to center line of columns

H = Average of the story heights above and below the joint

Wg = Gravity load uniformly distributed along the beam

Pg = Sum of concentrated gravity loads on the beam

Lp = Distance from the column face to the resultant of the 
     total concentrated gravity load on the beam

Shf = Distance to the far end hinge from the far end column CL";

		public const string HELP_FABRICATED_TEE =
@"For beams and upper gussets, 'ev2' is the distance to the first bolt from TOB.
For lower gussets, 'ev2' is the distance to the first bolt from BOB.

If you increase tf and/or W1, the program may increase the gusset (or beam) offset
from the column face.

If you decrease these dimensions, you can change the gusset (or beam)
offset in the Gusset (or Beam) control.

Ef is the effective throat thickness.";

		public const string HELP_SEISMIC =
			@"RBS = Reduced Beam Section
4E = 4 Bolt Unstiffened End Plate
4ES = 4 Bolt Stiffened End Plate
8ES = 8 Bolt Stiffened End Plate

Gravity Load: Input the gravity loads from the load combination
1.2 * D + f1 * L + 0.2 * S
D = Dead Load
L = Live Load
f1 = Load Factor for Live Load not less than 0.5
S = Snow Load";
	}
}