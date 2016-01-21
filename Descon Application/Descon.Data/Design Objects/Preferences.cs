using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Descon.Data
{
	/// <summary>
	/// Main structure for the Preferences file. More preferences are in the subclass DetailedPrefrences.
	/// If major changes are made, increment ConstString.FILE_PREFERENCES_VERSION. See note below
	/// </summary>
	[Serializable]
	public sealed class Preferences : INotifyPropertyChangedDescon
	{
		private ColorSettings _colorSettings;
		private EBracingType _bracingType;
		private ECalcMode _calcMode;
		private ESteelCode _steelCode;
		private ReportSettings _reportSettings;
		private EUnit _units;
		private List<BoltUserASTM> _nonAstmValues;
		private ESeismic _seismic;
		private int _transferForce;
		private bool _useStiffenedGussetPlate;
		private bool _endOfBracePulledBack;
		private Weld _defaultElectrode;
		private double _minThicknessSinglePlate;
		private double _minThicknessGussetPlate;
		private double _minThicknessAngle;
		private Bolt _defaultBoltUs;
		private Bolt _defaultBoltMetric;

		public Preferences()
		{
			var defaultPrefs = new DefaultPreferences();

			Seismic = ESeismic.NonSeismic;
			Units = EUnit.US;

			NonASTMValues = new List<BoltUserASTM>();
			ColorSettings = new ColorSettings();
			ColorSettings = defaultPrefs.SetColorDefaults();
			ApplicationThemeName = "Theme 01";
			ViewSettings = defaultPrefs.SetViewDefaults();
			ReportSettings = defaultPrefs.SetReportDefaults();
			DefaultElectrode = CommonDataStatic.WeldDict["E70XX"];
			DefaultMaterials = new DefaultMaterials();
			DefaultConnectionTypes = new DefaultConnectionTypes();
			DefaultMinimumEdgeDistances = new DefaultMinimumEdgeDistances();
			InputForceType = EPrefsInputForce.TransferForce;
			EnableGauges = true;
			NumberOfRecentlyOpenFiles = 5;
		}

		public string ApplicationThemeName { get; set; }

		/// <summary>
		/// Current unit system
		/// </summary>
		public EUnit Units
		{
			get { return _units; }
			set
			{
				new LoadDataFromXML().LoadShapes();
				_units = value;
				ConvertUnits.ReloadAngleShapeLists();
			}
		}

		/// <summary>
		/// Bracing Type selected in Interface: SCBF, OCBF, or EBF
		/// </summary>
		public EBracingType BracingType
		{
			get { return _bracingType; }
			set
			{
				_bracingType = value;
				OnPropertyChanged("BracingType");
			}
		}

		/// <summary>
		/// Current Seismic setting
		/// </summary>
		public ESeismic Seismic
		{
			get { return _seismic; }
			set
			{
				_seismic = value;
				OnPropertyChanged("Seismic");
			}
		}

		/// <summary>
		/// Current Calc Mode: ASD or LRFD
		/// </summary>
		public ECalcMode CalcMode
		{
			get { return _calcMode; }
			set
			{
				_calcMode = value;
				OnPropertyChanged("CalcMode");
			}
		}

		/// <summary>
		/// Current Steel code: AISC13 or AISC14
		/// </summary>
		public ESteelCode SteelCode
		{
			get { return _steelCode; }
			set
			{
				_steelCode = value;
				OnPropertyChanged("SteelCode");
			}
		}	

		public List<BoltUserASTM> NonASTMValues
		{
			get { return _nonAstmValues; }
			set
			{
				_nonAstmValues = value;
				OnPropertyChanged("NonASTMValues");
			}
		}

		public int NumberOfRecentlyOpenFiles { get; set; }

		public bool AutomaticallyCheckForUpdates { get; set; }
		public bool EnableGauges { get; set; }

		public bool IsWindowMaximized { get; set; }

		public bool UseContinuousClipAngles { get; set; }

		public bool EndOfBracePulledBack
		{
			get { return _endOfBracePulledBack; }
			set
			{
				_endOfBracePulledBack = value;
				OnPropertyChanged("EndOfBracePulledBack");
			}
		}

		public bool UseStiffenedGussetPlate
		{
			get { return CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC13 && _useStiffenedGussetPlate; }
			set { _useStiffenedGussetPlate = value; }
		}

		public EPrefsInputForce InputForceType { get; set; }
		public EPrefsTransferForceSelection TransferForceSelection { get; set; }

		/// <summary>
		/// Enum value used for the drop down control. Use TransferForce for int value
		/// </summary>
		public EPrefsDistanceToBraceAxisSelection TransferForceWithT { get; set; }

		/// <summary>
		/// Returns just the int value. InputForceType to see if the " * t " portion is also needed
		/// </summary>
		public int TransferForce
		{
			get
			{
				if (TransferForceSelection == EPrefsTransferForceSelection.Enter)
					return _transferForce;
				switch (TransferForceWithT)
				{
					case EPrefsDistanceToBraceAxisSelection.t2:
						return 2;
					case EPrefsDistanceToBraceAxisSelection.t3:
						return 3;
					default: // EPrefsDistanceToBraceAxisSelection.t4
						return 4;
				}
			}
			set { _transferForce = value; }
		}

		/// <summary>
		/// Used within the program to automatically retrieve the proper bolt.
		/// </summary>
		[XmlIgnore]
		public Bolt DefaultBolt
		{
			get { return CommonDataStatic.Units == EUnit.US ? DefaultBoltUS.ShallowCopy() : DefaultBoltMetric.ShallowCopy(); }
		}

		public Bolt DefaultBoltUS
		{
			get { return _defaultBoltUs; }
			set
			{
				_defaultBoltUs = value;
				OnPropertyChanged("DefaultBolt");
			}
		}

		public Bolt DefaultBoltMetric
		{
			get { return _defaultBoltMetric; }
			set
			{
				_defaultBoltMetric = value;
				OnPropertyChanged("DefaultBolt");
			}
		}

		public Weld DefaultElectrode
		{
			get { return _defaultElectrode.ShallowCopy(); }
			set { _defaultElectrode = value; }
		}

		[XmlIgnore]
		public string DefaultElectrodeName
		{
			get { return DefaultElectrode.Name; }
			set
			{
				if (value != null) 
					_defaultElectrode = CommonDataStatic.WeldDict[value];
				OnPropertyChanged("DefaultElectrodeName");
				MiscMethods.SetDefaultWelds(); // Only should be triggered when set by the user.
			}
		}

		public DefaultMaterials DefaultMaterials { get; set; }
		public DefaultConnectionTypes DefaultConnectionTypes { get; set; }
		public DefaultMinimumEdgeDistances DefaultMinimumEdgeDistances { get; set; }

		public double MinThicknessSinglePlate
		{
			get { return _minThicknessSinglePlate; }
			set
			{
				_minThicknessSinglePlate = value;
				OnPropertyChanged("MinThicknessSinglePlate");
			}
		}

		public double MinThicknessGussetPlate
		{
			get { return _minThicknessGussetPlate; }
			set
			{
				_minThicknessGussetPlate = value;
				OnPropertyChanged("MinThicknessGussetPlate");
			}
		}

		public double MinThicknessAngle
		{
			get { return _minThicknessAngle; }
			set
			{
				_minThicknessAngle = value;
				OnPropertyChanged("MinThicknessAngle");
			}
		}

		public ViewSettings ViewSettings { get; set; }

		public ReportSettings ReportSettings
		{
			get { return _reportSettings; }
			set
			{
				_reportSettings = value;
				OnPropertyChanged("ReportSettings");
			}
		}

		public ColorSettings ColorSettings
		{
			get { return _colorSettings; }
			set
			{
				_colorSettings = value;
				OnPropertyChanged("ColorSettings");
			}
		}
	}
}