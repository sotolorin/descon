using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Descon.Data
{
	/// <summary>
	/// Default Materials for each of the component types
	/// </summary>
	public sealed class DefaultMinimumEdgeDistances : INotifyPropertyChangedDescon
	{
		private double _selectedBoltSize;
		private readonly CommonLists _commonLists = new CommonLists();
		private const int NUMBER_OF_BOLT_SIZES = 8; // Imperial has 8, Metric has 7

		public DefaultMinimumEdgeDistances()
		{
			MinimumEdgeDistanceArray = new double[NUMBER_OF_BOLT_SIZES];
			SelectedBoltSize = _commonLists.BoltSizes.FirstOrDefault();
		}

		public double[] MinimumEdgeDistanceArray { get; set; }

		[XmlIgnore]
		public double SelectedBoltSize
		{
			get { return _selectedBoltSize; }
			set
			{
				_selectedBoltSize = value;
				OnPropertyChanged("MinimumEdgeDistance");
			}
		}

		[XmlIgnore]
		public double MinimumEdgeDistance
		{
			get
			{
				int index = _commonLists.BoltSizes.IndexOf(_selectedBoltSize);
				return MinimumEdgeDistanceArray[index];
			}
			set
			{
				int index = _commonLists.BoltSizes.IndexOf(_selectedBoltSize);
				MinimumEdgeDistanceArray[index] = value;
			}
		}
	}

	/// <summary>
	/// Default Materials for each of the component types
	/// </summary>
	public sealed class DefaultMaterials : INotifyPropertyChangedDescon
	{
		private EPrefsMaterialDefaults _selectedElementType;

		public DefaultMaterials()
		{
			WShape = CommonDataStatic.MaterialDict["A992"];
			WTShape = CommonDataStatic.MaterialDict["A992"];
			HSSShape = CommonDataStatic.MaterialDict["A500-B-46"];
			Angle = CommonDataStatic.MaterialDict["A36"];
			Channel = CommonDataStatic.MaterialDict["A36"];
			ConnectionPlate = CommonDataStatic.MaterialDict["A36"];
			GussetPlate = CommonDataStatic.MaterialDict["A36"];
			StiffenerPlate = CommonDataStatic.MaterialDict["A36"];

			SelectedElementType = EPrefsMaterialDefaults.WShapes;
		}

		public Material WShape { get; set; }
		public Material WTShape { get; set; }
		public Material HSSShape { get; set; }
		public Material Angle { get; set; }
		public Material Channel { get; set; }
		public Material ConnectionPlate { get; set; }
		public Material GussetPlate { get; set; }
		public Material StiffenerPlate { get; set; }

		[XmlIgnore]
		public EPrefsMaterialDefaults SelectedElementType
		{
			get { return _selectedElementType; }
			set
			{
				_selectedElementType = value;
				OnPropertyChanged("MaterialDict");
				OnPropertyChanged("SelectedMaterial");
				OnPropertyChanged("SelectedElementType");
			}
		}

		[XmlIgnore]
		public Dictionary<string, Material> MaterialDict
		{
			get
			{
				if (SelectedElementType == EPrefsMaterialDefaults.HSSShapes)
					return CommonDataStatic.MaterialDict;
				else
					return CommonDataStatic.MaterialDict.Where(m => m.Value.Name != ConstString.HSS_MATERIAL).ToDictionary(m => m.Key, m => m.Value);
			}
			set { CommonDataStatic.MaterialDict = value; }
		}

		[XmlIgnore]
		public string SelectedMaterial
		{
			get
			{
				switch (SelectedElementType)
				{
					case EPrefsMaterialDefaults.WShapes:
						return CommonDataStatic.Preferences.DefaultMaterials.WShape.Name;
					case EPrefsMaterialDefaults.WTShapes:
						return CommonDataStatic.Preferences.DefaultMaterials.WTShape.Name;
					case EPrefsMaterialDefaults.HSSShapes:
						return CommonDataStatic.Preferences.DefaultMaterials.HSSShape.Name;
					case EPrefsMaterialDefaults.Angles:
						return CommonDataStatic.Preferences.DefaultMaterials.Angle.Name;
					case EPrefsMaterialDefaults.Channels:
						return CommonDataStatic.Preferences.DefaultMaterials.Channel.Name;
					case EPrefsMaterialDefaults.ConnectionPlate:
						return CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate.Name;
					case EPrefsMaterialDefaults.GussetPlate:
						return CommonDataStatic.Preferences.DefaultMaterials.GussetPlate.Name;
					case EPrefsMaterialDefaults.StiffenerPlate:
						return CommonDataStatic.Preferences.DefaultMaterials.StiffenerPlate.Name;
					default:
						return CommonDataStatic.Preferences.DefaultMaterials.WShape.Name;
				}
			}
			set
			{
				if (value == null)
					value = CommonDataStatic.MaterialDict.FirstOrDefault().Value.Name;

				switch (SelectedElementType)
				{
					case EPrefsMaterialDefaults.WShapes:
						CommonDataStatic.Preferences.DefaultMaterials.WShape = CommonDataStatic.MaterialDict[value];
						break;
					case EPrefsMaterialDefaults.WTShapes:
						CommonDataStatic.Preferences.DefaultMaterials.WTShape = CommonDataStatic.MaterialDict[value];
						break;
					case EPrefsMaterialDefaults.HSSShapes:
						CommonDataStatic.Preferences.DefaultMaterials.HSSShape = CommonDataStatic.MaterialDict[value];
						break;
					case EPrefsMaterialDefaults.Angles:
						CommonDataStatic.Preferences.DefaultMaterials.Angle = CommonDataStatic.MaterialDict[value];
						break;
					case EPrefsMaterialDefaults.Channels:
						CommonDataStatic.Preferences.DefaultMaterials.Channel = CommonDataStatic.MaterialDict[value];
						break;
					case EPrefsMaterialDefaults.ConnectionPlate:
						CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate = CommonDataStatic.MaterialDict[value];
						break;
					case EPrefsMaterialDefaults.GussetPlate:
						CommonDataStatic.Preferences.DefaultMaterials.GussetPlate = CommonDataStatic.MaterialDict[value];
						break;
					case EPrefsMaterialDefaults.StiffenerPlate:
						CommonDataStatic.Preferences.DefaultMaterials.StiffenerPlate = CommonDataStatic.MaterialDict[value];
						break;
				}
			}
		}
	}

	public sealed class DefaultConnectionTypes : INotifyPropertyChangedDescon
	{
		private EJointConfiguration _selectedConfiguration;

		public DefaultConnectionTypes()
		{
			BeamToColumnWebShear = EShearCarriedBy.ClipAngle;
			BeamToColumnFlangeShear = EShearCarriedBy.ClipAngle;
			BeamToHSSShear = EShearCarriedBy.ClipAngle;
			BeamToGirderShear = EShearCarriedBy.ClipAngle;
			BeamSpliceShear = EShearCarriedBy.ClipAngle;

			BeamToColumnWebMoment = EMomentCarriedBy.NoMoment;
			BeamToColumnFlangeMoment = EMomentCarriedBy.NoMoment;
			BeamToHSSMoment = EMomentCarriedBy.NoMoment;
			BeamToGirderMoment = EMomentCarriedBy.NoMoment;

			SelectedConfiguration = EJointConfiguration.BeamToColumnFlange;
		}

		public EShearCarriedBy BeamToColumnWebShear { get; set; }
		public EShearCarriedBy BeamToColumnFlangeShear { get; set; }
		public EShearCarriedBy BeamToHSSShear { get; set; }
		public EShearCarriedBy BeamToGirderShear { get; set; }
		public EShearCarriedBy BeamSpliceShear { get; set; }

		public EMomentCarriedBy BeamToColumnWebMoment { get; set; }
		public EMomentCarriedBy BeamToColumnFlangeMoment { get; set; }
		public EMomentCarriedBy BeamToHSSMoment { get; set; }
		public EMomentCarriedBy BeamToGirderMoment { get; set; }

		// The following are used in the UI so the user can choose a Joint Config and then which defaults are active
		[XmlIgnore]
		public EJointConfiguration SelectedConfiguration
		{
			get { return _selectedConfiguration; }
			set
			{
				_selectedConfiguration = value;
				OnPropertyChanged("ShearDict");
				OnPropertyChanged("SelectedConfiguration");
				OnPropertyChanged("SelectedConnectionShear");
				OnPropertyChanged("SelectedConnectionMoment");
				OnPropertyChanged("IsMomentEnabled");
			}
		}

		[XmlIgnore]
		public Dictionary<EJointConfiguration, string> JointConfigDict
		{
			get
			{
				var jointConfigList = new Dictionary<EJointConfiguration, string>
				{
					{EJointConfiguration.BeamToColumnFlange, "Beam to Column Flange"},
					{EJointConfiguration.BeamToColumnWeb, "Beam to Column Web"},
					{EJointConfiguration.BeamToHSSColumn, ConstString.JOINT_CONFIG_BEAM_TO_HSS_COLUMN},
					{EJointConfiguration.BeamToGirder, ConstString.JOINT_CONFIG_BEAM_TO_GIRDER},
					{EJointConfiguration.BeamSplice, ConstString.JOINT_CONFIG_BEAM_SPLICE}
				};

				return jointConfigList;
			}
		}

		[XmlIgnore]
		public Dictionary<EShearCarriedBy, string> ShearDict
		{
			get
			{
				var shearList = new Dictionary<EShearCarriedBy, string>();
				shearList.Add(EShearCarriedBy.SinglePlate, ConstString.CON_SINGLE_PLATE);
				if (SelectedConfiguration == EJointConfiguration.BeamToGirder || SelectedConfiguration == EJointConfiguration.BeamToColumnWeb)
					shearList.Add(EShearCarriedBy.SinglePlateExtended, ConstString.CON_SINGLE_PLATE_EXTENDED);
				shearList.Add(EShearCarriedBy.ClipAngle, ConstString.CON_CLIP_ANGLE);
				shearList.Add(EShearCarriedBy.EndPlate, ConstString.CON_END_PLATE);
				shearList.Add(EShearCarriedBy.Tee, ConstString.CON_TEE);
				if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToGirder && CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice)
					shearList.Add(EShearCarriedBy.Seat, ConstString.CON_SEAT);
				return shearList;
			}
		}

		[XmlIgnore]
		public Dictionary<EMomentCarriedBy, string> MomentDict
		{
			get
			{
				var momentList = new Dictionary<EMomentCarriedBy, string>
				{
					{EMomentCarriedBy.NoMoment, ConstString.CON_NO_MOMENT},
					{EMomentCarriedBy.FlangePlate, ConstString.CON_FLANGE_PLATE},
					{EMomentCarriedBy.Tee, ConstString.CON_TEE},
					{EMomentCarriedBy.Angles, ConstString.CON_ANGLES},
					{EMomentCarriedBy.EndPlate, ConstString.CON_END_PLATE},
					{EMomentCarriedBy.DirectlyWelded, ConstString.CON_DIRECTLY_WELDED}
				};

				return momentList;
			}
		}

		[XmlIgnore]
		public EShearCarriedBy SelectedConnectionShear
		{
			get
			{
				switch (SelectedConfiguration)
				{
					case EJointConfiguration.BeamToColumnFlange:
						return CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToColumnFlangeShear;
					case EJointConfiguration.BeamToColumnWeb:
						return CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToColumnWebShear;
					case EJointConfiguration.BeamToHSSColumn:
						return CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToHSSShear;
					case EJointConfiguration.BeamToGirder:
						return CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToGirderShear;
					case EJointConfiguration.BeamSplice:
						return CommonDataStatic.Preferences.DefaultConnectionTypes.BeamSpliceShear;
					default:
						return CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToColumnFlangeShear;
				}
			}
			set
			{
				switch (SelectedConfiguration)
				{
					case EJointConfiguration.BeamToColumnFlange:
						CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToColumnFlangeShear = value;
						break;
					case EJointConfiguration.BeamToColumnWeb:
						CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToColumnWebShear = value;
						break;
					case EJointConfiguration.BeamToHSSColumn:
						CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToHSSShear = value;
						break;
					case EJointConfiguration.BeamToGirder:
						CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToGirderShear = value;
						break;
					case EJointConfiguration.BeamSplice:
						CommonDataStatic.Preferences.DefaultConnectionTypes.BeamSpliceShear = value;
						break;
				}

				OnPropertyChanged("SelectedConnectionShear");
			}
		}

		[XmlIgnore]
		public EMomentCarriedBy SelectedConnectionMoment
		{
			get
			{
				switch (SelectedConfiguration)
				{
					case EJointConfiguration.BeamToColumnFlange:
						return CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToColumnFlangeMoment;
					case EJointConfiguration.BeamToColumnWeb:
						return CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToColumnWebMoment;
					case EJointConfiguration.BeamToHSSColumn:
						return CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToHSSMoment;
					case EJointConfiguration.BeamToGirder:
						return CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToGirderMoment;
					default:
						return CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToColumnFlangeMoment;
				}
			}
			set
			{
				switch (SelectedConfiguration)
				{
					case EJointConfiguration.BeamToColumnFlange:
						CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToColumnFlangeMoment = value;
						break;
					case EJointConfiguration.BeamToColumnWeb:
						CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToColumnWebMoment = value;
						break;
					case EJointConfiguration.BeamToHSSColumn:
						CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToHSSMoment = value;
						break;
					case EJointConfiguration.BeamToGirder:
						CommonDataStatic.Preferences.DefaultConnectionTypes.BeamToGirderMoment = value;
						break;
				}
				OnPropertyChanged("SelectedConnectionMoment");
			}
		}
	}

	/// <summary>
	/// View settings that determine which parts of the drawing are visible
	/// </summary>
	public sealed class ViewSettings : INotifyPropertyChangedDescon
	{
		private bool _show3D;
		private bool _showTop;
		private bool _showFront;
		private bool _showLeft;
		private bool _showRight;
		public bool Dimensions { get; set; }
		public bool Callouts { get; set; }
		public bool Welds { get; set; }

		public bool Show3D
		{
			get { return _show3D; }
			set
			{
				_show3D = value;
				OnPropertyChanged("Show3D");
			}
		}

		public bool ShowTop
		{
			get { return _showTop; }
			set
			{
				_showTop = value;
				OnPropertyChanged("ShowTop");
			}
		}

		public bool ShowFront
		{
			get { return _showFront; }
			set
			{
				_showFront = value;
				OnPropertyChanged("ShowFront");
			}
		}

		public bool ShowLeft
		{
			get { return _showLeft; }
			set
			{
				_showLeft = value;
				OnPropertyChanged("ShowLeft");
			}
		}

		public bool ShowRight
		{
			get { return _showRight; }
			set
			{
				_showRight = value;
				OnPropertyChanged("ShowRight");
			}
		}

		public bool Show3DWireframe { get; set; }

		public bool ThinLines { get; set; }
	}

	/// <summary>
	/// Colors settings for the drawing components and the UI theme
	/// </summary>
	public sealed class ColorSettings
	{
		public string Columns { get; set; }
		public string BeamsBraces { get; set; }
		public string Bolts { get; set; }
		public string DimensionLinesLeaders { get; set; }
		public string WeldSymbols { get; set; }
		public string Text { get; set; }
		public string ConnectionElements { get; set; }
		public string Highlight { get; set; }
		public string Background { get; set; }

		public EDrawingTheme DrawingTheme { get; set; }
	}

	/// <summary>
	/// Report header text fields and custom image path. Values are used for a 5 row, 4 column table
	/// </summary>
	public sealed class ReportSettings : INotifyPropertyChangedDescon
	{
		private string _value8;
		private string _value9;
		private bool _showDrawing;
		private bool _useDateForValue8;
		private bool _useDateForValue9;
		private bool _showLeftSideView;
		private bool _showRightSideView;
		private bool _showTopView;
		private bool _showFrontView;
		private bool _show3DView;

		public string FontName { get; set; }
		public string FontSize { get; set; }
		public int NumberOfColumns { get; set; }
		public EReportFileTypes DefaultSaveFileFormat { get; set; }

		public bool AddLineNumbersToReport { get; set; }

		public bool AutoToggleBookmarks { get; set; }

		public bool PDFShowHeaderEveryPage { get; set; }
		public bool ShowFullFilePathInHeader { get; set; }

		public bool ShowCalculations { get; set; }

		public bool ShowDrawing
		{
			get { return _showDrawing; }
			set
			{
				// If none of the views are selected and the user chooses to show the drawing, we enable two of them.
				if (!_showDrawing && value)
				{
					if (NoViewsSelected)
						ShowLeftSideView = ShowFrontView = true;
				}

				_showDrawing = value;
				OnPropertyChanged("ShowDrawing");
				OnPropertyChanged("ShowLeftSideView");
				OnPropertyChanged("ShowFrontView");
			}
		}

		public bool ShowDrawingInColor { get; set; }
		public bool ShowDrawingAtTop { get; set; }

		/// <summary>
		/// This is used only in the UI for the button
		/// </summary>
		[XmlIgnore]
		public bool ShowDrawingAtBottom
		{
			get { return !ShowDrawingAtTop; }
		}

		public bool CombineDrawingViews { get; set; }

		public bool ShowLeftSideView
		{
			get { return _showLeftSideView; }
			set
			{
				_showLeftSideView = value;
				OnPropertyChanged("ShowLeftSideView");
			}
		}

		public bool ShowRightSideView
		{
			get { return _showRightSideView; }
			set
			{
				_showRightSideView = value;
				OnPropertyChanged("ShowRightSideView");
			}
		}

		public bool ShowTopView
		{
			get { return _showTopView; }
			set
			{
				_showTopView = value;
				OnPropertyChanged("ShowTopView");
			}
		}

		public bool ShowFrontView
		{
			get { return _showFrontView; }
			set
			{
				_showFrontView = value;
				OnPropertyChanged("ShowFrontView");
			}
		}

		public bool Show3DView
		{
			get { return _show3DView; }
			set
			{
				_show3DView = value;
				OnPropertyChanged("Show3DView");
			}
		}

		/// <summary>
		/// Returns true if all of the views are disabled.
		/// </summary>
		[XmlIgnore]
		public bool NoViewsSelected
		{
			get { return !ShowLeftSideView && !ShowRightSideView && !ShowTopView && !ShowFrontView && !Show3DView; }
		}

		public bool SmartPageNumbering { get; set; }

		// Column 1 - Rows 1 through 5
		public string Title1 { get; set; }
		public string Title2 { get; set; }
		public string Title3 { get; set; }
		public string Title4 { get; set; }
		public string Title5 { get; set; }
		// Column 2 - Rows 1 through 5
		public string Value1 { get; set; }
		public string Value2 { get; set; }
		public string Value3 { get; set; }
		public string Value4 { get; set; }
		public string Value5 { get; set; }
		// Column 3 - Rows 1 through 4
		public string Title6 { get; set; }
		public string Title7 { get; set; }
		public string Title8 { get; set; }
		public string Title9 { get; set; }
		// Column 4 - Rows 1 through 4
		public string Value6 { get; set; }
		public string Value7 { get; set; }

		public string Value8
		{
			get { return UseDateForValue8 ? DateTime.Today.ToShortDateString() : _value8; }
			set { _value8 = value; }
		}

		public string Value9
		{
			get { return UseDateForValue9 ? DateTime.Today.ToShortDateString() : _value9; }
			set { _value9 = value; }
		}

		// The following two values have the option to always be displayed as dates
		public bool UseDateForValue8
		{
			get { return _useDateForValue8; }
			set
			{
				_useDateForValue8 = value;
				OnPropertyChanged("UseDateForValue8");
				OnPropertyChanged("Value8");
			}
		}

		public bool UseDateForValue9
		{
			get { return _useDateForValue9; }
			set
			{
				_useDateForValue9 = value;
				OnPropertyChanged("UseDateForValue9");
				OnPropertyChanged("Value9");
			}
		}
	}
}