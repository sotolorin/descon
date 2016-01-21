using System;
using System.Linq;
using System.Xml.Serialization;

namespace Descon.Data
{
	/// <summary>
	/// Represents a bolt and sets initial default values
	/// </summary>
	[Serializable]
	public sealed class Bolt : INotifyPropertyChangedDescon
	{
		private double _boltSize;
		private EBoltMinSpacing _minSpacingType;
		private EBoltHoleType _holeType;
		private EBoltASTM _astmType;
		private EBoltType _boltType;
		private bool _spacingLongDirUser;
		private bool _spacingTransvDirUser;
		private int _numberOfBolts;
		private int _numberOfLines;
		private int _numberOfRows;
		private double _minSpacing;
		private bool _edgeDistLongDirUser;
		private bool _edgeDistTransvDirUser;
		private EBoltLimitState _limitState;
		private bool _overrideLimitState;
		private string _slot3Name;
		private bool _slot0;
		private bool _slot1;
		private bool _slot3;
		private bool _slot2;
		private double _edgeDistLongDir;
		private double _edgeDistTransvDir;

		/// <summary>
		/// Creates a copy that can be edited without altering the original data
		/// </summary>
		public Bolt ShallowCopy()
		{
			return (Bolt)MemberwiseClone();
		}

		public Bolt()
		{
			NumberOfBolts = 2;
			NumberOfRows = 2;
			NumberOfLines = 1;
			MinSpacingType = EBoltMinSpacing.Three;
			HoleType = EBoltHoleType.STD;
			SurfaceClass = EBoltSurfaceClass.A;
			HoleDir = EBoltHoleDir.B;
			BoltType = EBoltType.N;
			ASTMType = EBoltASTM.A325;
			FillerFactorEnum = EBoltFillerFactor.One;
			isLSLNEnabled = true;
			isLSLPEnabled = true;
			isSSLNEnabled = true;
			isSSLPEnabled = true;
			isOVSEnabled = true;

			SetBaseValues();
		}

		/// <summary>
		/// Converts the Bolt data when the Unit system changes
		/// </summary>
		public void SetBaseValues()
		{
			BoltSize = CommonDataStatic.Units == EUnit.US ? 0.75 : 20;
			MinSpacing = ConstNum.THREE_INCHES;
			SpacingLongDir = ConstNum.THREE_INCHES;
			SpacingTransvDir = ConstNum.THREE_INCHES;
			EdgeDistBrace = ConstNum.ONEANDHALF_INCHES;
			EdgeDistGusset = ConstNum.ONEANDHALF_INCHES;
			EdgeDistLongDir = ConstNum.ONEANDHALF_INCHES;
			EdgeDistTransvDir = ConstNum.ONEANDHALF_INCHES;
			Pretension = CommonDataStatic.Units == EUnit.US ? 19 : 91000;
			HoleDiameterSTD = CommonDataStatic.Units == EUnit.US ? 0.6875 : 18;
		}

		#region General Properties

		/// <summary>
		/// Returns the bolt name based on particular properties
		/// </summary>
		public string BoltName
		{
			get
			{
				if (ASTM != null && ASTMType == EBoltASTM.NonASTM)
					return "(" + BoltSize + " - " + ASTM.Name + " - " + BoltType + " - " + HoleType + ")";
				else
					return "(" + BoltSize + " - " + ASTMType + " - " + BoltType + " - " + HoleType + ")";
			}
		}

		/// <summary>
		/// When set changes the HoleDir, SpacingAlongLength, HoleLength, HoleWidth, and Eincr values
		/// </summary>
		public EBoltHoleType HoleType // ht
		{
			get { return _holeType; }
			set
			{
				_holeType = value;
				SetHoleValues();
				BoltMethods.SetHoleType(this, _holeType);
				BoltMethods.SetBoltSlotNames();
				OnPropertyChanged("HoleType");
			}
		}

		public bool isOVSEnabled { get; set; }
		public bool isSSLNEnabled { get; set; }
		public bool isSSLPEnabled { get; set; }
		public bool isLSLNEnabled { get; set; }
		public bool isLSLPEnabled { get; set; }
		// Special case in Single Plate that requires LSLN to be disabled
		public bool disableLSLNWithBoltGroupEccentricity;

		public EBoltLimitState LimitState
		{
			get
			{
				if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
					_limitState = EBoltLimitState.Slip;
				else if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC13 && _limitState == EBoltLimitState.Slip)
					BoltMethods.SetHoleType(this, _holeType);
				return _limitState;
			}
			set { _limitState = value; }
		}

		public bool OverrideLimitState
		{
			get { return _overrideLimitState; }
			set
			{
				_overrideLimitState = value;
				OnPropertyChanged("OverrideLimitState");
				BoltMethods.SetHoleType(this, _holeType);
			}
		}

		public EBoltSurfaceClass SurfaceClass { get; set; }

		public EBoltType BoltType //ct
		{
			get { return _boltType; }
			set
			{
				_boltType = value;

				if (CommonDataStatic.DetailDataDict != null)
				{
					MiscMethods.SetHoleTypesEnabledOrDisabled(CommonDataStatic.DetailDataDict[EMemberType.RightBeam]);
					MiscMethods.SetHoleTypesEnabledOrDisabled(CommonDataStatic.DetailDataDict[EMemberType.LeftBeam]);
				}

				if ((HoleType == EBoltHoleType.SSLP && !isSSLPEnabled) ||
				    (HoleType == EBoltHoleType.SSLN && !isSSLNEnabled) ||
				    (HoleType == EBoltHoleType.LSLP && !isLSLPEnabled) ||
				    (HoleType == EBoltHoleType.LSLN && !isLSLNEnabled) ||
				    (HoleType == EBoltHoleType.OVS && !isOVSEnabled))
					HoleType = EBoltHoleType.STD;

				OnPropertyChanged("IsFillerFactorEnabled");
				OnPropertyChanged("BoltType");
				OnPropertyChanged("BoltName");
				OnPropertyChanged("HoleType");
				OnPropertyChanged("isOVSEnabled");
				OnPropertyChanged("isSSLNEnabled");
				OnPropertyChanged("isSSLPEnabled");
				OnPropertyChanged("isLSLNEnabled");
				OnPropertyChanged("isLSLPEnabled");
			}
		}

		public EBoltASTM ASTMType
		{
			get { return _astmType; }
			set
			{
				_astmType = value;
				if (_astmType == EBoltASTM.NonASTM && CommonDataStatic.Preferences.NonASTMValues.Any())
					ASTM = CommonDataStatic.Preferences.NonASTMValues.First();
				OnPropertyChanged("ASTM");
				OnPropertyChanged("BoltName");
			}
		}

		public BoltUserASTM ASTM { get; set; }

		public double SpacingLongDir { get; set; } // sl

		public bool SpacingLongDir_User
		{
			get { return _spacingLongDirUser; }
			set
			{
				_spacingLongDirUser = value;
				if (!_spacingLongDirUser)
				{
					CalculateMinSpacing();
					OnPropertyChanged("SpacingLongDir");
					OnPropertyChanged("SpacingTransvDir");
				}
			}
		}

		public double SpacingTransvDir { get; set; } // st

		public bool SpacingTransvDir_User
		{
			get { return _spacingTransvDirUser; }
			set
			{
				_spacingTransvDirUser = value;
				if (!_spacingTransvDirUser)
				{
					CalculateMinSpacing();
					OnPropertyChanged("SpacingLongDir");
					OnPropertyChanged("SpacingTransvDir");
				}
			}
		}

		public double Eincr { get; set; } // sometimes EdgeInc()

		[XmlIgnore]
		public double MinEdgeSheared
		{
			get { return BoltMethods.ShearedEdge(_boltSize); }
		}

		[XmlIgnore]
		public double MinEdgeRolled
		{
			get { return BoltMethods.RolledEdge(_boltSize); }
		}

		[XmlIgnore]
		public double HoleLength; // HoleHoriz
		[XmlIgnore]
		public double HoleWidth; // HoleVert
		[XmlIgnore]
		public double HoleLengthSupport;
		[XmlIgnore]
		public double HoleWidthSupport;
		[XmlIgnore]
		public double HoleDiameterSTD; // HoleDiamSTD

		[XmlIgnore]
		public double FillerFactor
		{
			get { return FillerFactorEnum == EBoltFillerFactor.One ? 1 : 0.85; }
		}

		public EBoltFillerFactor FillerFactorEnum { get; set; }

		[XmlIgnore]
		public bool IsFillerFactorEnabled
		{
			get { return CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14 && _boltType == EBoltType.SC; }
		}

		// The following block are properties taken from BRACE1.BoltData
		public int NumberOfBolts
		{
			get
			{
				if (_numberOfBolts == 0)
					_numberOfBolts = _numberOfLines * _numberOfRows;
				return _numberOfBolts;
			}
			set { _numberOfBolts = value; }
		}

		public bool NumberOfBolts_User { get; set; }

		public int NumberOfLines // nw - Number in each row
		{
			get { return _numberOfLines; }
			set
			{
				if (value == 0)
					value = 1;
				_numberOfLines = value;
				if (_numberOfLines != 0 && _numberOfRows != 0)
					_numberOfBolts = _numberOfLines * _numberOfRows;
				OnPropertyChanged("NumberOfBolts");
			}
		}

		public bool NumberOfLines_User { get; set; }

		public int NumberOfRows // nl - Number of Rows
		{
			get { return _numberOfRows; }
			set
			{
				if (value == 0)
					value = 1;
				_numberOfRows = value;
				if (_numberOfLines != 0 && _numberOfRows != 0)
					_numberOfBolts = _numberOfLines * _numberOfRows;
				OnPropertyChanged("NumberOfBolts");
			}
		}

		public bool NumberOfRows_User { get; set; }

		// eal in Descon 7
		public double EdgeDistLongDir
		{
			get { return _edgeDistLongDir; }
			set
			{
				int index = new CommonLists().BoltSizes.IndexOf(BoltSize);
				if (index >= 0 && BoltSize > 0 && CommonDataStatic.Preferences != null &&
					value < CommonDataStatic.Preferences.DefaultMinimumEdgeDistances.MinimumEdgeDistanceArray[index])
					_edgeDistLongDir = CommonDataStatic.Preferences.DefaultMinimumEdgeDistances.MinimumEdgeDistanceArray[index];
				else
					_edgeDistLongDir = value;
			}
		}

		public bool EdgeDistLongDir_User
		{
			get { return _edgeDistLongDirUser; }
			set
			{
				if (_edgeDistLongDirUser && !value)
					EdgeDistLongDir = CommonDataStatic.Units == EUnit.US ? 1.5 : 38;
				_edgeDistLongDirUser = value;
				OnPropertyChanged("EdgeDistLongDir");
			}
		}

		// eat in Descon 7
		public double EdgeDistTransvDir
		{
			get { return _edgeDistTransvDir; }
			set
			{
				int index = new CommonLists().BoltSizes.IndexOf(BoltSize);
				if (BoltSize > 0 && CommonDataStatic.Preferences != null && 
					value < CommonDataStatic.Preferences.DefaultMinimumEdgeDistances.MinimumEdgeDistanceArray[index])
					_edgeDistTransvDir = CommonDataStatic.Preferences.DefaultMinimumEdgeDistances.MinimumEdgeDistanceArray[index];
				else
					_edgeDistTransvDir = value;
			}
		}

		public bool EdgeDistTransvDir_User
		{
			get { return _edgeDistTransvDirUser; }
			set
			{
				if (_edgeDistTransvDirUser && !value)
					EdgeDistTransvDir = CommonDataStatic.Units == EUnit.US ? 1.5 : 38;
				_edgeDistTransvDirUser = value;
				OnPropertyChanged("EdgeDistTransvDir");
			}
		}

		public double EdgeDistGusset { get; set; } // eg
		public bool EdgeDistGusset_User { get; set; }

		public double EdgeDistBrace; // eb

		public double Pretension;

		public double BoltStrength
		{
			get { return BoltMethods.CalculateBoltStrength(this); }
		}

		/// <summary>
		/// Returns the bolt size and if set, changes the HoleDiameterSTD, MinEdgeSheared, and MinEdgeRolled
		/// Also changes HoleLength and HoleWidth, as these properties are completely dependent upon bolt size. -RM 10/13/14
		/// </summary>
		public double BoltSize
		{
			get { return _boltSize; }
			set
			{
				_boltSize = value;

				SetHoleValues();

				OnPropertyChanged("MinEdgeSheared");
				OnPropertyChanged("MinEdgeRolled");
				OnPropertyChanged("HoleWidth");
				OnPropertyChanged("HoleLength");
				OnPropertyChanged("BoltName");
			}
		}

		private void SetHoleValues()
		{
			double length;
			double width;

			if (CommonDataStatic.Units == EUnit.Metric)
			{
				if (_boltSize < 24)
					HoleDiameterSTD = (_boltSize + 2);
				else
					HoleDiameterSTD = (_boltSize + 3);
			}

			switch (HoleType)
			{
				case EBoltHoleType.STD:
					HoleLength = CommonDataStatic.Units == EUnit.US ? _boltSize + ConstNum.SIXTEENTH_INCH : _boltSize + 3;
					HoleWidth = CommonDataStatic.Units == EUnit.US ? _boltSize + ConstNum.SIXTEENTH_INCH : _boltSize + 3;
					HoleLengthSupport = CommonDataStatic.Units == EUnit.US ? _boltSize + ConstNum.SIXTEENTH_INCH : _boltSize + 3;
					HoleWidthSupport = CommonDataStatic.Units == EUnit.US ? _boltSize + ConstNum.SIXTEENTH_INCH : _boltSize + 3;
					break;
				case EBoltHoleType.OVS:
					HoleLength = _boltSize + ConstNum.FIVE_SIXTEENTHS;
					HoleWidth = _boltSize + ConstNum.FIVE_SIXTEENTHS;
					HoleLengthSupport = _boltSize + ConstNum.FIVE_SIXTEENTHS;
					HoleWidthSupport = _boltSize + ConstNum.FIVE_SIXTEENTHS;
					break;
				case EBoltHoleType.SSLN:
				case EBoltHoleType.SSLP:
					if (_boltSize < 0.625)
						length = _boltSize + ConstNum.THREE_SIXTEENTHS;
					else if (_boltSize < 1)
						length = _boltSize + ConstNum.QUARTER_INCH;
					else if (_boltSize < 1.125)
						length = _boltSize + ConstNum.FIVE_SIXTEENTHS;
					else
						length = _boltSize + ConstNum.THREE_EIGHTS_INCH;

					width = CommonDataStatic.Units == EUnit.US ? _boltSize + ConstNum.SIXTEENTH_INCH : _boltSize + 3;

					if ((Slot0 && !Slot1) || (Slot2 && !Slot3))
					{
						HoleLength = length;
						HoleWidth = width;
						HoleLengthSupport = width;
						HoleWidthSupport = width;
					}

					else if ((!Slot0 && Slot1) || (!Slot2 && Slot3))
					{
						HoleLength = width;
						HoleWidth = width;
						HoleLengthSupport = length;
						HoleWidthSupport = width;
					}
					else if ((Slot0 && Slot1) || (Slot2 && Slot3))
					{
						HoleLength = length;
						HoleWidth = width;
						HoleLengthSupport = length;
						HoleWidthSupport = width;
					}
					else
					{
						HoleLength = width;
						HoleWidth = length;
						HoleLengthSupport = width;
						HoleWidthSupport = length;
					}

					break;
				case EBoltHoleType.LSLN:
				case EBoltHoleType.LSLP:
					HoleWidth = CommonDataStatic.Units == EUnit.US ? _boltSize + ConstNum.SIXTEENTH_INCH : _boltSize + 3;
					HoleLength = _boltSize * 2.5;
					HoleWidthSupport = CommonDataStatic.Units == EUnit.US ? _boltSize + ConstNum.SIXTEENTH_INCH : _boltSize + 3;
					HoleLengthSupport = _boltSize * 2.5;
					break;
			}
		}

		/// <summary>
		/// When set changes the SpacingLongDir value
		/// </summary>
		public EBoltMinSpacing MinSpacingType
		{
			get { return _minSpacingType; }
			set
			{
				_minSpacingType = value;
				CalculateMinSpacing();

				OnPropertyChanged("SpacingLongDir");
				OnPropertyChanged("SpacingTransvDir");
				OnPropertyChanged("MinSpacing");
				OnPropertyChanged("MinSpacingCustom");
			}
		}

		private void CalculateMinSpacing()
		{
			if (MinSpacing == 0 || (!CommonDataStatic.LoadingFileInProgress))
			{
				if (CommonDataStatic.Units == EUnit.US)
				{
					switch (_minSpacingType)
					{
						case EBoltMinSpacing.DiameterX267:
							if (!SpacingLongDir_User)
								SpacingLongDir = 2.67 * BoltSize;
							if (!SpacingTransvDir_User)
								SpacingTransvDir = 2.67 * BoltSize;
							MinSpacing = 2.67 * BoltSize;
							break;
						case EBoltMinSpacing.DiameterX3:
							if (!SpacingLongDir_User)
								SpacingLongDir = 3 * BoltSize;
							if (!SpacingTransvDir_User)
								SpacingTransvDir = 3 * BoltSize;
							MinSpacing = 3 * BoltSize;
							break;
						case EBoltMinSpacing.Three:
							if (!SpacingLongDir_User)
								SpacingLongDir = 3;
							if (!SpacingTransvDir_User)
								SpacingTransvDir = 3;
							MinSpacing = 3;
							break;
						case EBoltMinSpacing.Custom:
							if (!SpacingLongDir_User)
								SpacingLongDir = MinSpacing;
							if (!SpacingTransvDir_User)
								SpacingTransvDir = MinSpacing;
							break;
					}
				}
				else
				{
					switch (_minSpacingType)
					{
						case EBoltMinSpacing.DiameterX267:
							if (!SpacingLongDir_User)
								SpacingLongDir = 67 * BoltSize;
							if (!SpacingTransvDir_User)
								SpacingTransvDir = 67 * BoltSize;
							MinSpacing = 67 * BoltSize;
							break;
						case EBoltMinSpacing.DiameterX3:
							if (!SpacingLongDir_User)
								SpacingLongDir = 75 * BoltSize;
							if (!SpacingTransvDir_User)
								SpacingTransvDir = 75 * BoltSize;
							MinSpacing = 75 * BoltSize;
							break;
						case EBoltMinSpacing.Three:
							if (!SpacingLongDir_User)
								SpacingLongDir = 75;
							if (!SpacingTransvDir_User)
								SpacingTransvDir = 75;
							MinSpacing = 75;
							break;
						case EBoltMinSpacing.Custom:
							if (!SpacingLongDir_User)
								SpacingLongDir = MinSpacing;
							if (!SpacingTransvDir_User)
								SpacingTransvDir = MinSpacing;
							break;
					}
				}
			}
		}

		/// <summary>
		/// Enables the MinSpacing field on the Bolt form if Custom Min Spacing type is selected
		/// </summary>
		[XmlIgnore]
		public bool MinSpacingCustom
		{
			get { return _minSpacingType == EBoltMinSpacing.Custom; }
		}

		public double MinSpacing
		{
			get { return _minSpacing; }
			set
			{
				_minSpacing = value;
				if (!SpacingLongDir_User && SpacingLongDir < _minSpacing)
					SpacingLongDir = _minSpacing;
				if (!SpacingTransvDir_User && SpacingTransvDir < _minSpacing)
					SpacingTransvDir = _minSpacing;
				OnPropertyChanged("MinSpacing");
				OnPropertyChanged("SpacingLongDir");
				OnPropertyChanged("SpacingTransvDir");
			}
		}

		public bool MinSpacing_User { get; set; }

		public EBoltHoleDir HoleDir { get; set; }

		#endregion

		#region Slot Stuff

		public bool Slot0
		{
			get { return _slot0; }
			set
			{
				_slot0 = value;
				SetHoleValues();
			}
		}

		public bool Slot1
		{
			get { return _slot1; }
			set
			{
				_slot1 = value;
				SetHoleValues();
			}
		}

		public bool Slot2
		{
			get { return _slot2; }
			set
			{
				_slot2 = value;
				SetHoleValues();
			}
		}

		public bool Slot3
		{
			get { return _slot3; }
			set
			{
				_slot3 = value;
				SetHoleValues();
			}
		}

		private void SetDefaultSlotAndUpdateValues()
		{
			if (!Slot0Visibility)
				Slot0 = false;
			if (!Slot1Visibility)
				Slot1 = false;
			if (!Slot2Visibility)
				Slot2 = false;
			if (!Slot3Visibility)
				Slot3 = false;

			OnPropertyChanged("Slot0");
			OnPropertyChanged("Slot1");
			OnPropertyChanged("Slot2");
			OnPropertyChanged("Slot3");
			OnPropertyChanged("Slot0Name");
			OnPropertyChanged("Slot1Name");
			OnPropertyChanged("Slot2Name");
			OnPropertyChanged("Slot3Name");
			OnPropertyChanged("Slot0Visibility");
			OnPropertyChanged("Slot1Visibility");
			OnPropertyChanged("Slot2Visibility");
			OnPropertyChanged("Slot3Visibility");
		}

		[XmlIgnore]
		public string Slot0Name { get; set; }

		[XmlIgnore]
		public string Slot1Name { get; set; }

		[XmlIgnore]
		public string Slot2Name { get; set; }

		[XmlIgnore]
		public string Slot3Name
		{
			get { return _slot3Name; }
			set
			{
				_slot3Name = value;
				SetDefaultSlotAndUpdateValues();
			}
		}

		[XmlIgnore]
		public bool Slot0Visibility
		{
			get
			{
				if (Slot0Name == string.Empty || HoleType == EBoltHoleType.STD ||
				    (CommonDataStatic.BeamToColumnType == EJointConfiguration.ColumnSplice && HoleType == EBoltHoleType.OVS))
					return false;
				else
					return true;
			}
		}

		[XmlIgnore]
		public bool Slot1Visibility
		{
			get
			{
				if (Slot1Name == string.Empty || HoleType == EBoltHoleType.STD ||
				    (CommonDataStatic.BeamToColumnType == EJointConfiguration.ColumnSplice && HoleType == EBoltHoleType.OVS))
					return false;
				else
					return true;
			}
		}

		[XmlIgnore]
		public bool Slot2Visibility
		{
			get
			{
				if (Slot2Name == string.Empty || HoleType == EBoltHoleType.STD ||
				    (CommonDataStatic.BeamToColumnType == EJointConfiguration.ColumnSplice && HoleType == EBoltHoleType.OVS))
					return false;
				else
					return true;
			}
		}

		[XmlIgnore]
		public bool Slot3Visibility
		{
			get
			{
				if (Slot3Name == string.Empty || _holeType == EBoltHoleType.STD ||
				    (CommonDataStatic.BeamToColumnType == EJointConfiguration.ColumnSplice && _holeType == EBoltHoleType.OVS))
					return false;
				else
					return true;
			}
		}

		#endregion
	}
}