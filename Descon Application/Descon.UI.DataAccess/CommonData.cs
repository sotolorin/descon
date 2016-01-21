using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Descon.Data;
using MahApps.Metro;

namespace Descon.UI.DataAccess
{
	/// <summary>
	/// This class contains all of the current pertinant data used in most forms. You either pass this class around
	/// or just parts of it. The main Descon UI is bound to much of the data in this class, so use caution when removing
	/// or renaming properties. Refactor is your friend.
	/// 
	/// Descon.Data/CommonDataStatic.cs contains a few properties that are used throughout the application. I have made them
	/// static for easy accessibility anywhere. They can still be accessed through this class, but all it will do is reference
	/// the ones in CommonDataStatic.
	/// </summary>
	public partial class CommonData : INotifyPropertyChangedDescon
	{
		private EMemberType _memberType;
		private SolidColorBrush _themeAccent;
		private int _numberOfNoGoods;
		private WindowState _windowState;
		private string _updateDownloadStatus;
		private string _reportText;

		// Because MemberType resets everything including the JointConfig, we do not want to execute the JointConfig setter
		// when it changes the MemberType. These two properties prevent this recurrsion.
		private bool _changingJointConfig;
		private bool _changingMemberType;
		private FlowDocument _reportDocument;

		public CommonData()
		{
			var load = new LoadDataFromXML();
			
			CurrentFilePath = ConstString.FILE_DEFAULT_NAME;
			ConstUnit = new ConstUnit();
			MaterialDict = load.LoadMaterials();
			WeldDict = load.LoadWelds();
			GaugeData = new List<GaugeData>();

			Preferences = load.LoadPreferences();
			CommonDataStatic.Preferences = Preferences;
			if (CommonDataStatic.Preferences.IsWindowMaximized)
				_windowState = WindowState.Maximized;

			CommonDataStatic.AllShapes = new Dictionary<string, Shape> {{ConstString.NONE, new Shape()}};
			ShapesUser = new ObservableCollection<Shape>();

			CommonDataStatic.EccentricWeldCoefficients = load.LoadEccentricWeldCoefficients();

			load.LoadShapes();

			CommonLists = new CommonLists();

			SetDefaultUIData();

			CommonDataStatic.JointConfig = JointConfig;

			ContextMenuRecentFiles = MiscMethods.GetRecentlyOpenedFileList();

			if (CommonDataStatic.LicenseMinimumStandard)
			{
				try
				{
					ShapesUser = LoadUserShapes();
					foreach (var shape in ShapesUser)
						CommonDataStatic.AllShapes.Add(shape.Name, shape);
				}
				catch
				{
					MessageBox.Show("ERROR", "Error reading User Shapes.", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		// Change the title depending on if this is an internal or public release
		public string ApplicationTitle
		{
			get { return CommonDataStatic.LicenseTypeDisplay + " - " + CurrentFileName; }
			//get { return "****WORK IN PROGRESS BETA VERSION**** " + CommonDataStatic.LicenseTypeDisplay + " - " + CurrentFileName; }
		}

		#region General Settings and Data

		private bool licenseTypeChanging;
		public ELicenseType LicenseType
		{
			get { return CommonDataStatic.LicenseType; }
			set
			{
				if (!licenseTypeChanging)
				{
					licenseTypeChanging = true;
					CommonDataStatic.LicenseType = value;
					OnPropertyChanged(null);
					licenseTypeChanging = false;
				}
			}
		}

		/// <summary>
		/// Contains all unit string constants that are determined by the current unit type
		/// </summary>
		public ConstUnit ConstUnit { get; set; }

		/// <summary>
		/// Currently selected component, such as Column. This is used to bind data in the UI.
		/// </summary>
		public DetailData SelectedMember
		{
			get { return CommonDataStatic.SelectedMember; }
			set
			{
				CommonDataStatic.SelectedMember = value;
				BoltMethods.SetBoltSlotNames();
				OnPropertyChanged("MaterialDictMember");
				OnPropertyChanged("MaterialName");
			}
		}

		/// <summary>
		/// Contains all of the data for every component
		/// </summary>
		public Dictionary<EMemberType, DetailData> DetailDataDict
		{
			get { return CommonDataStatic.DetailDataDict; }
			set { CommonDataStatic.DetailDataDict = value; }
		}

		/// <summary>
		/// Current Unit system - either Metric or US
		/// </summary>
		public EUnit Units
		{
			get { return Preferences.Units; }
			set
			{
				if (Preferences.Units != value)
				{
					Preferences.Units = value;
					ConvertUnits.UnitsChanged();
					OnPropertyChanged(null);
					new SaveDataToXML().SavePreferences();
				}
			}
		}

		/// <summary>
		/// We have this added Shape layer in the UI becuase the KBrace and KneeBrace properties must update according
		/// to the complete set of selected shapes.
		/// </summary>
		public string ShapeName
		{
			get { return SelectedMember.ShapeName; }
			set
			{
				if (value != null)
				{
					// Refresh views that haven't been zoomed when adding a new member
					CommonDataStatic.UnityNewMemberAdded = SelectedMember.ShapeName == ConstString.NONE && value != ConstString.NONE;

					SelectedMember.Shape = CommonDataStatic.AllShapes[value];
					OnPropertyChanged("ShapeName");
					OnPropertyChanged("MaterialDictMember");

					MiscCalculationDataMethods.SetKBraceValues();
				}
			}
		}

		/// <summary>
		/// Needed to trigger OnPropertyChanged at this level so menu items can change when the shape type is changed
		/// </summary>
		public EShapeType ShapeType
		{
			get { return SelectedMember.ShapeType; }
			set
			{
				// This logic is necessary for the combo box to update properly and have a default value selected
				SelectedMember.ShapeType = value;
				OnPropertyChanged("ShapeType");
				SelectedMember.Shape = new Shape();
				OnPropertyChanged("ShapesFiltered");
				OnPropertyChanged("ShapeName");
				OnPropertyChanged("OrientationLabel");
				OnPropertyChanged("SelectedMember");
				OnPropertyChanged("IsGageOnFlangeVisible");
				OnPropertyChanged("IsMomentTextBoxEnabled");
				OnPropertyChanged("MaterialDictMember");
				OnPropertyChanged("MaterialName");
			}
		}

		/// <summary>
		/// Complete list of shapes filtered by the selected shape type
		/// </summary>
		public Dictionary<string, Shape> ShapesFiltered
		{
			get
			{
				Dictionary<string, Shape> shapes;

				if (SelectedMember.MaterialName == ConstString.HSS_MATERIAL)
				{
					shapes = CommonDataStatic.AllShapes.Where(s => s.Value.Name == ConstString.NONE ||
					                                               (s.Value.UnitSystem == CommonDataStatic.Units &&
					                                                s.Value.a_A != 0 &&
					                                                s.Value.TypeEnum == ShapeType)).ToDictionary(s => s.Key, s => s.Value);
				}
				else
				{
					shapes = CommonDataStatic.AllShapes.Where(s => s.Value.Name == ConstString.NONE ||
																   (s.Value.UnitSystem == CommonDataStatic.Units &&
																	s.Value.TypeEnum == ShapeType)).ToDictionary(s => s.Key, s => s.Value);
				}

				return shapes;
			}
		}

		/// <summary>
		/// Complete list of User Shapes
		/// </summary>
		public ObservableCollection<Shape> ShapesUser { get; set; }

		/// <summary>
		/// Complete list of Single Angle Shapes set in LoadDataFromXML.LoadShapes();
		/// </summary>
		public Dictionary<string, Shape> ShapesSingleAngle
		{
			get { return CommonDataStatic.ShapesSingleAngle; }
		}

		/// <summary>
		/// Complete list of Single Angle without the "None" default shape
		/// </summary>
		public Dictionary<string, Shape> ShapesSingleAngleNoNone
		{
			get { return CommonDataStatic.ShapesSingleAngleNoNone; }
		}

		/// <summary>
		/// Complete list of Double Angle Shapes
		/// </summary>
		public Dictionary<string, Shape> ShapesDoubleAngle
		{
			get { return CommonDataStatic.ShapesDoubleAngle; }
		}

		/// <summary>
		/// Complete list of Single Channel Shapes
		/// </summary>
		public Dictionary<string, Shape> ShapesSingleChannel
		{
			get { return CommonDataStatic.ShapesSingleChannel; }
		}

		/// <summary>
		/// Complete list of Tee Shapes set in LoadDataFromXML.LoadShapes();
		/// </summary>
		public Dictionary<string, Shape> ShapesTee
		{
			get { return CommonDataStatic.ShapesTee; }
		}

		/// <summary>
		/// All of the Preferences. An alias to CommonDataStatic.Preferences for binding purposes.
		/// </summary>
		public Preferences Preferences
		{
			get { return CommonDataStatic.Preferences; }
			set
			{
				CommonDataStatic.Preferences = value;
				OnPropertyChanged("Preferences");
				OnPropertyChanged("IsAxialForceEnabled");
				OnPropertyChanged("IsGaugeVisible");
			}
		}

		public SeismicSettings SeismicSettings
		{
			get { return CommonDataStatic.SeismicSettings; }
			set { CommonDataStatic.SeismicSettings = value; }
		}

		/// <summary>
		/// Color of the UI. Automatically changes many of the components, but a few must be changed manually or bound to this property.
		/// </summary>
		public SolidColorBrush ThemeAccent
		{
			get { return _themeAccent; }
			set
			{
				_themeAccent = value;

				OnPropertyChanged("ThemeAccent");
				OnPropertyChanged("ThemeAccentColor");
			}
		}

		public Color ThemeAccentColor
		{
			get { return _themeAccent != null ? _themeAccent.Color : Colors.White; }
		}

		public List<SolidColorBrush> ThemeList
		{
			get
			{
				var list = new List<SolidColorBrush>();
				foreach (var theme in CommonLists.ThemeDict)
					list.Add(new SolidColorBrush(Color.FromRgb(theme.Value[0], theme.Value[1], theme.Value[2])));

				return list;
			}
		}

		/// <summary>
		/// Full list of AccentColors for the menu.
		/// </summary>
		public List<AccentColorMenuData> AccentColors
		{
			get { return ThemeManager.Accents.Select(a => new AccentColorMenuData {Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as SolidColorBrush}).ToList(); }
		}

		public double FormControlShellPositionLeft { get; set; }
		public double FormControlShellPositionTop { get; set; }

		#endregion

		#region Lists and Selected Items in Lists

		/// <summary>
		/// Lists of items used in combo boxes throughout the program
		/// </summary>
		public CommonLists CommonLists { get; set; }

		/// <summary>
		/// NOT TO BE USED BY CALCULATIONS. Use "CommonDataStatic.BeamToColumnType" instead. 
		/// Selected Joint Configuration which determines what mode we are in and updates many parts of the UI. Only used
		/// for display purposes.
		/// </summary>
		public EJointConfiguration JointConfig
		{
			get { return CommonDataStatic.JointConfig; }
			set
			{
				if (!_changingJointConfig)
				{
					CommonDataStatic.JointConfig = value;

					MemberType = (EMemberType)CommonLists.MemberList.GetKey(0);

					switch (value)
					{
						case EJointConfiguration.BraceToColumn:
							SelectedMember.MomentConnection = EMomentCarriedBy.DirectlyWelded;
							break;
						case EJointConfiguration.ColumnSplice:
							SelectedMember.MomentConnection = EMomentCarriedBy.NoMoment;
							MiscMethods.DeactivateBraces();
							break;
						case EJointConfiguration.BeamToGirder:
							SelectedMember.MomentConnection = EMomentCarriedBy.FlangePlate;
							MiscMethods.DeactivateBraces();
							break;
						case EJointConfiguration.BeamSplice:
							SelectedMember.MomentConnection = EMomentCarriedBy.NoMoment;
							MiscMethods.DeactivateBraces();
							break;
						default:
							SelectedMember.MomentConnection = EMomentCarriedBy.NoMoment;
							break;
					}

					UnityInteraction.SendDataToUnity(ConstString.UNITY_JOINT_CONFIG_CHANGE);

					OnPropertyChanged("SelectedMember");
					OnPropertyChanged("IsWebOrientationEnabled");
					OnPropertyChanged("IsForceVisible");
				}
			}
		}

		/// <summary>
		/// Currently selected component type which determines what data will be used
		/// </summary>
		public EMemberType MemberType
		{
			get { return _memberType; }
			set
			{
				if (!_changingMemberType)
				{
					_memberType = value;

					SelectedMember = CommonDataStatic.DetailDataDict[_memberType];
					// This tells the class every property potentially changed and needs to be re-evaulated.
					_changingMemberType = true;
					OnPropertyChanged(null);
					if (CommonDataStatic.CommonLists.BraceMoreDataComponentList.Count > 0)
						SelectedMember.BraceMoreDataSelection = CommonDataStatic.CommonLists.BraceMoreDataComponentList.Keys.First();
				}
				_changingJointConfig = false;
				_changingMemberType = false;
			}
		}

		public string MaterialName
		{
			get { return SelectedMember.MaterialName; }
			set
			{
				SelectedMember.MaterialName = value;
				OnPropertyChanged("ShapesFiltered");
				OnPropertyChanged("MaterialName");
			}
		}

		/// <summary>
		/// This material list is used everywhere except for the main panel.
		/// </summary>
		public Dictionary<string, Material> MaterialDict
		{
			get { return CommonDataStatic.MaterialDict.Where(m => m.Value.Name != ConstString.HSS_MATERIAL).ToDictionary(m => m.Key, m => m.Value); }
			set
			{
				CommonDataStatic.MaterialDict = value;
				OnPropertyChanged("MaterialDict");
				OnPropertyChanged("MaterialDictMember");
			}
		}

		/// <summary>
		/// This material list is only used on the main panel. HSS has a special material available.
		/// </summary>
		public Dictionary<string, Material> MaterialDictMember
		{
			get
			{
				if (ShapeType == EShapeType.HollowSteelSection && (SelectedMember.Shape.a_A != 0 || !SelectedMember.IsActive))
					return CommonDataStatic.MaterialDict;
				else
					return CommonDataStatic.MaterialDict.Where(m => m.Value.Name != ConstString.HSS_MATERIAL).ToDictionary(m => m.Key, m => m.Value);
			}
			set
			{
				CommonDataStatic.MaterialDict = value;
				OnPropertyChanged("MaterialDict");
				OnPropertyChanged("MaterialDictMember");
			}
		}

		/// <summary>
		/// All Welds
		/// </summary>
		public Dictionary<string, Weld> WeldDict
		{
			get { return CommonDataStatic.WeldDict.Where(w => w.Value.Metric == (Units == EUnit.Metric)).ToDictionary(w => w.Key, w => w.Value); }
			set
			{
				CommonDataStatic.WeldDict = value;
				OnPropertyChanged("WeldDict");
			}
		}

		/// <summary>
		/// Used to set the Bolt in the bolt selection form
		/// </summary>
		public Bolt CurrentBolt { get; set; }

		/// <summary>
		/// Non ASTM Value set by the user and available in the Bolt dialogue box
		/// </summary>
		public List<BoltUserASTM> NonASTMValues
		{
			get { return Preferences.NonASTMValues; }
			set
			{
				Preferences.NonASTMValues = value;
				OnPropertyChanged("NonASTMValues");
			}
		}

		#endregion

		#region File Path Information

		/// <summary>
		/// Complete file path including the file name
		/// </summary>
		public string CurrentFilePath
		{
			get { return CommonDataStatic.CurrentFilePath; }
			set
			{
				CommonDataStatic.CurrentFilePath = value;
				CommonDataStatic.CurrentFileName = value.Split('\\').Last();
				OnPropertyChanged("CurrentFilePath");
				OnPropertyChanged("CurrentFileName");
				OnPropertyChanged("CurrentFilePathOnly");
				OnPropertyChanged("ApplicationTitle");
			}
		}

		/// <summary>
		/// Name of the current file
		/// </summary>
		public string CurrentFileName
		{
			get { return CommonDataStatic.CurrentFileName; }
		}

		/// <summary>
		/// Current file path without file name
		/// </summary>
		public string CurrentFilePathOnly
		{
			get { return CurrentFilePath.TrimEnd(CurrentFileName.ToCharArray()); }
		}

		#endregion

		#region Connection lists, Splice and Stiffener Data

		public Dictionary<EBraceConnectionTypes, string> GussetToBeamConnectionList
		{
			get { return CommonLists.GussetToBeamConnectionList; }
		}

		public Dictionary<EBraceConnectionTypes, string> GussetToColumnConnectionList
		{
			get { return CommonLists.GussetToColumnConnectionList; }
		}

		public ColumnSplice ColumnSplice
		{
			get { return CommonDataStatic.ColumnSplice; }
			set { CommonDataStatic.ColumnSplice = value; }
		}

		public ColumnStiffener ColumnStiffener
		{
			get { return CommonDataStatic.ColumnStiffener; }
			set { CommonDataStatic.ColumnStiffener = value; }
		}

		/// <summary>
		/// List of ways the Shear connection can be carried
		/// </summary>
		public Dictionary<EShearCarriedBy, string> ShearCarriedByList
		{
			get { return CommonLists.ShearCarriedByList; }
		}

		public EShearCarriedBy ShearConnection
		{
			get { return SelectedMember.ShearConnection; }
			set
			{
				SelectedMember.ShearConnection = value;
				OnPropertyChanged("MomentCarriedByList");
				OnPropertyChanged("IsMomentEnabled");
				OnPropertyChanged("MomentConnection");
			}
		}

		public EMomentCarriedBy MomentConnection
		{
			get { return SelectedMember.MomentConnection; }
			set
			{
				SelectedMember.MomentConnection = value;
				OnPropertyChanged("IsMomentEnabled");
				OnPropertyChanged("IsAxialForceEnabled");
				OnPropertyChanged("IsTransferForceEnabled");
				OnPropertyChanged("MomentConnection");
			}
		}

		public double Moment
		{
			get { return SelectedMember.Moment; }
			set
			{
				SelectedMember.Moment = value;
				OnPropertyChanged("MomentConnection");
			}
		}

		/// <summary>
		/// List of ways the Moment connection can be carried
		/// </summary>
		public Dictionary<EMomentCarriedBy, string> MomentCarriedByList
		{
			get { return CommonLists.MomentCarriedByList(); }
		}

		#endregion

		#region Report Data

		/// <summary>
		/// The actual HTML text that makes up the report
		/// </summary>
		public string ReportText
		{
			get { return _reportText; }
			set
			{
				_reportText = value;
				OnPropertyChanged("ReportText");
				OnPropertyChanged("GaugeData");
				OnPropertyChanged("GaugeMainCapacity");
			}
		}

		public FlowDocument ReportDocument
		{
			get { return _reportDocument; }
			set
			{
				_reportDocument = value;
				OnPropertyChanged("ReportDocument");
			}
		}

		/// <summary>
		/// Total number of NG's in the report used to display in the UI
		/// </summary>
		public int NumberOfNoGoods
		{
			get { return _numberOfNoGoods; }
			set
			{
				_numberOfNoGoods = value;
				OnPropertyChanged("NumberOfNoGoods");
				OnPropertyChanged("NumberOfNoGoodsColor");
			}
		}

		/// <summary>
		/// Color of the No Good display that changes depending on the number of NG's
		/// </summary>
		public Brush NumberOfNoGoodsColor
		{
			get
			{
				if (NumberOfNoGoods == 0)
					return new SolidColorBrush(Color.FromRgb(12, 152, 122));
				else
					return new SolidColorBrush(Color.FromRgb(196, 71, 71));
			}
		}

		/// <summary>
		/// List of headings in the report used for a combobox
		/// </summary>
		public Dictionary<string, string> ReportGoToList
		{
			get { return CommonDataStatic.ReportGoToList; }
			set { CommonDataStatic.ReportGoToList = value; }
		}

		#endregion

		#region Gauge Data

		/// <summary>
		/// Main gauge capacity value. The text for this one is always the same.
		/// </summary>
		public double GaugeMainCapacity
		{
			get
			{
				if (GaugeData.Count < 1) 
					return 0.1;
				else if (NumberOfNoGoods > 0)
					return double.MaxValue;
				else
					return GaugeData.Max(g => g.CapacityValue);
			}
		}

		/// <summary>
		/// The list of capacity values and strings to display for the gauges on the side panel.
		/// </summary>
		public List<GaugeData> GaugeData
		{
			get { return CommonDataStatic.GaugeData; }
			set { CommonDataStatic.GaugeData = value; }
		}

		#endregion
	}
}