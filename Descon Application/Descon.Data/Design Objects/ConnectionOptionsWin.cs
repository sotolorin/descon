using System;
using System.Xml.Serialization;

namespace Descon.Data
{
	/// <summary>
	/// All Win related classes for each of the UI forms
	/// </summary>
	[Serializable]
	public class ConnectionOptionsWin
	{
		private bool _distanceToFirstBoltUser;

		public WCBeam Beam { get; set; }
		public Fema Fema { get; set; }

		public WCShearClipAngle ShearClipAngle { get; set; }
		public WCShearWebTee ShearWebTee { get; set; }
		public WCShearWebPlate ShearWebPlate { get; set; }
		public WCShearEndPlate ShearEndPlate { get; set; }
		public WCShearSeat ShearSeat { get; set; }

		public WCMomentEndPlate MomentEndPlate { get; set; }
		public WCMomentFlangePlate MomentFlangePlate { get; set; }
		public WCMomentTee MomentTee { get; set; }
		public WCMomentFlangeAngle MomentFlangeAngle { get; set; }
		public WCMomentDirectlyWelded MomentDirectWeld { get; set; }

		/// <summary>
		/// This is a special case where this value determines other values in many classes at the same level.
		/// </summary>
		public bool DistanceToFirstBolt_User
		{
			get { return _distanceToFirstBoltUser; }
			set
			{
				_distanceToFirstBoltUser = value;
				if (value)
				{
					if (!ShearWebTee.Position_User)
						ShearWebTee.Position = EPosition.Top;
					if (!ShearClipAngle.Position_User)
						ShearClipAngle.Position = EPosition.Top;
					if (!ShearEndPlate.Position_User)
						ShearEndPlate.Position = EPosition.Top;
					if (!ShearWebPlate.Position_User)
						ShearWebPlate.Position = EPosition.Top;
				}
			}
		}

		public ConnectionOptionsWin()
		{
			Beam = new WCBeam();
			Fema = new Fema();

			ShearClipAngle = new WCShearClipAngle();
			ShearWebTee = new WCShearWebTee();
			ShearWebPlate = new WCShearWebPlate();
			ShearEndPlate = new WCShearEndPlate();
			ShearSeat = new WCShearSeat();

			MomentEndPlate = new WCMomentEndPlate();
			MomentFlangePlate = new WCMomentFlangePlate();
			MomentTee = new WCMomentTee();
			MomentFlangeAngle = new WCMomentFlangeAngle();
			MomentDirectWeld = new WCMomentDirectlyWelded();
		}
	}

	#region Miscellaneous Controls - Beam, Stiff

	/// <summary>
	/// ControlCOBeam: Beam.
	/// </summary>
	[Serializable]
	public class WCBeam : IConnectDescon
	{
		private double _topElFeet;
		private double _topElInches;
		private double _topElNumerator;
		private double _topElDenominator;
		private bool _isTopElNegative;
		private bool _topElMainUser;
		private double _tCopeL;
		private double _tCopeD;
		private double _bCopeL;
		private double _bCopeD;
		private bool _lhUser;
		private bool _boltEdgeDistanceOnFlangeUser;
		private double _distanceToFirstBolt;
		private double _distanceToFirstBoltDisplay;
		private bool _tCopeUser;
		private bool _bCopeUser;

		public WCBeam()
		{
			Material = CommonDataStatic.Preferences.DefaultMaterials.WShape.ShallowCopy();
			SkewOrIncline = EBeamSkewOrIncline.PerpendicularToSupport;
			CopeReinforcement = new BeamCopeReinforcement();
			DistanceToFirstBolt = ConstNum.THREE_INCHES;
			Lh = ConstNum.ONEANDHALF_INCHES;
			BoltEdgeDistanceOnFlange = ConstNum.TWO_INCHES;

			TopElDenominator = 16;
		}

		/// <summary>
		/// Use this to get the actual value for TopEl. The other properties are just parts.
		/// </summary>
		[XmlIgnore]
		public double TopElValue
		{
			get
			{
				if (CommonDataStatic.Units == EUnit.US && TopElDenominator != 0)
					return ((TopElFeet * 12) + TopElInches + (TopElNumerator / TopElDenominator)) * (IsTopElNegative ? -1 : 1);
				else
					return TopElFeet;
			}
		}

		/// <summary>
		/// This always displays the Beam Gage no matter what and can override the DetailData.GageOnFlange value
		/// </summary>
		public double GageOnFlange { get; set; }

		public bool TopElMain_User
		{
			get { return _topElMainUser; }
			set
			{
				_topElMainUser = value;
				if (!value)
				{
					TopElFeet = TopElInches = TopElNumerator = 0;
					TopElDenominator = 16;
					IsTopElNegative = false;
					OnPropertyChanged("TopElFeet");
					OnPropertyChanged("TopElInches");
					OnPropertyChanged("TopElNumerator");
					OnPropertyChanged("TopElDenominator");
					OnPropertyChanged("IsTopElNegative");
				}
			}
		}

		public double TopElFeet
		{
			get { return _topElFeet; }
			set
			{
				_topElFeet = value;
				OnPropertyChanged("TopElPreview");
			}
		}

		public double TopElInches
		{
			get { return _topElInches; }
			set
			{
				_topElInches = value;
				OnPropertyChanged("TopElPreview");
			}
		}

		public double TopElNumerator
		{
			get { return _topElNumerator; }
			set
			{
				_topElNumerator = value;
				OnPropertyChanged("TopElPreview");
			}
		}

		public double TopElDenominator
		{
			get { return _topElDenominator; }
			set
			{
				_topElDenominator = value;
				OnPropertyChanged("TopElPreview");
			}
		}

		public bool IsTopElNegative
		{
			get { return _isTopElNegative; }
			set
			{
				_isTopElNegative = value;
				OnPropertyChanged("TopElPreview");
			}
		}

		/// Used for display only in the form
		[XmlIgnore]
		public string TopElPreview
		{
			get
			{
				if (CommonDataStatic.Units == EUnit.US)
					return (IsTopElNegative ? "-" : string.Empty) + TopElFeet + "' " + TopElInches + "\" " + TopElNumerator + "/" + TopElDenominator;
				else
					return TopElFeet + " mm";
			}
		}

		public double BoltEdgeDistanceOnFlange { get; set; } // DESGEN.Beam.ef

		public double TCopeL
		{
			get { return _tCopeL; }
			set
			{
				if (_tCopeL != 0 && value == 0)
					_tCopeD = 0;

				_tCopeL = value;

				OnPropertyChanged("TCopeL");
				OnPropertyChanged("TCopeD");
			}
		}

		public double TCopeD
		{
			get { return _tCopeD; }
			set
			{
				if (_tCopeD != 0 && value == 0)
					_tCopeL = 0;

				_tCopeD = value;

				OnPropertyChanged("TCopeL");
				OnPropertyChanged("TCopeD");
			}
		}

		public double BCopeL
		{
			get { return _bCopeL; }
			set
			{
				if (_bCopeL != 0 && value == 0)
					_bCopeD = 0;

				_bCopeL = value;

				OnPropertyChanged("BCopeL");
				OnPropertyChanged("BCopeD");
			}
		}

		public double BCopeD
		{
			get { return _bCopeD; }
			set
			{
				if (_bCopeD != 0 && value == 0)
					_bCopeL = 0;

				_bCopeD = value;

				OnPropertyChanged("BCopeL");
				OnPropertyChanged("BCopeD");
			}
		}

		public double Lh { get; set; }

		public EBeamSkewOrIncline SkewOrIncline { get; set; }

		public BeamCopeReinforcement CopeReinforcement;

		public double SkewAngle_A { get; set; }
		public double SkewAngle_B { get; set; }
		public double InclinceAngle_A { get; set; }
		public double InclinceAngle_B { get; set; }
		public double WebHeight;

		public double BotAttachThick;
		public double TopAttachThick;
		public double WebAttachTop;
		public double WebAttachBottom;

		public bool TopCope
		{
			get { return TCopeL > 0 && TCopeD > 0; }
		}

		public bool BottomCope
		{
			get { return BCopeL > 0 && BCopeD > 0; }
		}

		public bool Lh_User
		{
			get { return _lhUser; }
			set
			{
				_lhUser = value;
				if (!value)
				{
					Lh = ConstNum.ONEANDHALF_INCHES;
					OnPropertyChanged("Lh");
				}
			}
		}

		public bool BoltEdgeDistanceOnFlange_User
		{
			get { return _boltEdgeDistanceOnFlangeUser; }
			set
			{
				_boltEdgeDistanceOnFlangeUser = value;
				if (!value)
				{
					BoltEdgeDistanceOnFlange = ConstNum.TWO_INCHES;
					OnPropertyChanged("BoltEdgeDistanceOnFlange");
				}
			}
		}

		public bool TCope_User
		{
			get { return _tCopeUser; }
			set
			{
				if (_tCopeUser && !value)
				{
					_tCopeUser = false; // Necessary for method below
					MiscCalculationDataMethods.CalculateCopeStuff(CommonDataStatic.SelectedMember.MemberType);
				}
				_tCopeUser = value;
				OnPropertyChanged("TCope_User");
				OnPropertyChanged("TCopeL");
				OnPropertyChanged("TCopeD");
			}
		}

		public bool BCope_User
		{
			get { return _bCopeUser; }
			set
			{
				if (_bCopeUser && !value)
				{
					_bCopeUser = false; // Necessary for method below
					MiscCalculationDataMethods.CalculateCopeStuff(CommonDataStatic.SelectedMember.MemberType);
				}
				_bCopeUser = value;
				OnPropertyChanged("BCope_User");
				OnPropertyChanged("BCopeL");
				OnPropertyChanged("BCopeD");
			}
		}

		public double SkewAngle_A_User { get; set; }
		public double SkewAngle_B_User { get; set; }
		public double InclinceAngle_A_User { get; set; }
		public double InclinceAngle_B_User { get; set; }

		// Used in calculations
		public double DistanceToFirstBolt
		{
			get { return _distanceToFirstBolt; }
			set
			{
				_distanceToFirstBolt = value;
				OnPropertyChanged("DistanceToFirstBolt");
			}
		}

		// LV for display in UI
		public double DistanceToFirstBoltDisplay
		{
			get { return _distanceToFirstBoltDisplay; }
			set
			{
				_distanceToFirstBoltDisplay = value;
				OnPropertyChanged("DistanceToFirstBolt");
			}
		}
	}

	#endregion

	#region Shear Connection Button

	/// <summary>
	/// ControlCOShearSeat: Shear Connection - Seated
	/// </summary>
	[Serializable]
	public class WCShearSeat : IConnectDescon
	{
		private double _weldSizeSupport;
		private double _weldSizeBeam;
		private ESeatStiffener _stiffener;

		public WCShearSeat()
		{
			Bolt = CommonDataStatic.Preferences.DefaultBolt;
			Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();

			if (CommonDataStatic.Units == EUnit.US)
				Angle = CommonDataStatic.ShapesSingleAngleNoNone["L8X8X1-1/8"];
			else
				Angle = CommonDataStatic.ShapesSingleAngleNoNone["L203X203X28.6"];

			TopAngle = new Shape();
			StiffenerAngle = new Shape();
			StiffenerTee = new Shape();
			Material = CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate.ShallowCopy();
			
			Bolt.NumberOfRows = 1;
		}

		public Shape Angle { get; set; }
		public Shape TopAngle { get; set; }

		public string AngleName
		{
			get { return Angle.Name; }
			set { Angle = CommonDataStatic.AllShapes[value ?? ConstString.NONE]; }
		}

		public string TopAngleName
		{
			get { return TopAngle.Name; }
			set { TopAngle = CommonDataStatic.AllShapes[value ?? ConstString.NONE]; }
		}

		public ESeatConnection Connection { get; set; }

		public ESeatStiffener Stiffener
		{
			get { return _stiffener; }
			set
			{
				_stiffener = value;
				OnPropertyChanged("Stiffener");
			}
		}

		public Bolt Bolt { get; set; }

		public double WeldSizeSupport // DESGEN.Seat.SupportWeld
		{
			get { return NumberFun.Round(_weldSizeSupport, 16); }
			set { _weldSizeSupport = value; }
		}

		public double WeldSizeBeam // DESGEN.Seat.BeamWeld
		{
			get { return NumberFun.Round(_weldSizeBeam, 16); }
			set { _weldSizeBeam = value; }
		}

		public double AngleShort
		{
			get { return 4; }
		} // DESGEN.SeatAngle1.Short - Always seems to be 4
		public double AngleLength { get; set; }
		public double TopAngleLength; // Used in report, but not in the UI
		public double PlateLength { get; set; } // PLLength
		public double PlateWidth { get; set; } // PLWidth
		public double PlateThickness { get; set; } // PLThickness
		public EShortLegOn ShortLegOn { get; set; }
		// Stiffener
		public Shape StiffenerAngle { get; set; }
		public Shape StiffenerTee { get; set; }

		public string StiffenerAngleName
		{
			get { return StiffenerAngle.Name; }
			set { StiffenerAngle = CommonDataStatic.AllShapes[value ?? ConstString.NONE]; }
		}

		public string StiffenerTeeName
		{
			get { return StiffenerTee.Name; }
			set { StiffenerTee = CommonDataStatic.AllShapes[value ?? ConstString.NONE]; }
		}

		public double StiffenerLength { get; set; }
		public double StiffenerWidth { get; set; }
		public double StiffenerThickness { get; set; }

		public double StiffenerFlangeTh;
		public double StiffenerFlangeWidth;

		// The remaining properties are used in the Unity code
		public double TrSpacingOut;
		public double TopAngleBeamLeg
		{
			get { return TopAngle.t < ConstNum.QUARTER_INCH ? ConstNum.FOUR_INCHES : TopAngle.d; }
		}
		public double TopAngleSupLeg
		{
			get { return TopAngle.t < ConstNum.QUARTER_INCH ? ConstNum.FOUR_INCHES : TopAngle.b; }
		}

		// User check box properties
		public bool Angle_User { get; set; }
		public bool TopAngle_User { get; set; }
		public bool WeldSizeSupport_User { get; set; }
		public bool WeldSizeBeam_User { get; set; }
		public bool AngleLength_User { get; set; }
		public bool PlateLength_User { get; set; }
		public bool PlateWidth_User { get; set; }
		public bool PlateThickness_User { get; set; }
		public bool L2Length_User { get; set; }
		public bool TeeLength_User { get; set; }
		public bool StiffenerLength_User { get; set; }
		public bool StiffenerWidth_User { get; set; }
		public bool StiffenerThickness_User { get; set; }
	}

	/// <summary>
	/// ControlCOShearClipAngle: Web Connection - Clip Angle
	/// </summary>
	[Serializable]
	public class WCShearClipAngle : IConnectDescon
	{
		private double _weldSizeSupport;
		private double _weldSizeBeam;
		private EConnectionStyle _supportSideConnection;
		private EPosition _position;
		private double _weldSize;
		private EBoltStagger _boltStagger;
		private bool _positionUser;

		public WCShearClipAngle()
		{
			// WinConnect Defaults
			BoltWebOnBeam = CommonDataStatic.Preferences.DefaultBolt;
			BoltOslOnSupport = CommonDataStatic.Preferences.DefaultBolt;
			Size = new Shape();
			Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
			Position = EPosition.Top;
			SizeType = EWebConnectionSize.L2;
			BoltStagger = EBoltStagger.None;
			SupportSideConnection = EConnectionStyle.Bolted;
			BeamSideConnection = EConnectionStyle.Welded;
			Material = CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate.ShallowCopy();
			// BraceConnect Defaults
			BoltOnColumn = CommonDataStatic.Preferences.DefaultBolt;
			BoltOnGusset = CommonDataStatic.Preferences.DefaultBolt;
			Length = 18*ConstNum.ONE_INCH;
			OSL = EOSL.LongLeg;
			TopOfBeamToBolt = ConstNum.THREE_INCHES;
		}

		/// <summary>
		/// Creates a copy that can be edited without altering the original data. Used to mirror left and right beams.
		/// </summary>
		public WCShearClipAngle ShallowCopy()
		{
			return (WCShearClipAngle)MemberwiseClone();
		}

		public bool IsWeldedEnabled
		{
			get { return CommonDataStatic.JointConfig != EJointConfiguration.BeamSplice; }
		}

		#region Formally WinConnect data

		public EWebConnectionSize SizeType { get; set; }
		public EOSL OSL { get; set; }

		public EBoltStagger BoltStagger
		{
			get { return _boltStagger; }
			set
			{
				_boltStagger = value;
				OnPropertyChanged("BoltStagger");
			}
		}

		public Bolt BoltOslOnSupport { get; set; }
		public Bolt BoltWebOnBeam { get; set; }

		// These are used for the UI. If we have no stagger, then the values much match
		[XmlIgnore]
		public double BoltOslOnSupportEdgeDistanceLong
		{
			get { return BoltOslOnSupport.EdgeDistLongDir; }
			set
			{
				BoltOslOnSupport.EdgeDistLongDir = value;
				if (BoltStagger == EBoltStagger.None)
				{
					BoltWebOnBeam.EdgeDistLongDir_User = true;
					BoltWebOnBeam.EdgeDistLongDir = value;
					OnPropertyChanged("BoltWebOnBeamEdgeDistanceLong");
					OnPropertyChanged("BoltWebOnBeam.EdgeDistLongDir_User");
				}
			}
		}

		[XmlIgnore]
		public double BoltWebOnBeamEdgeDistanceLong
		{
			get { return BoltWebOnBeam.EdgeDistLongDir; }
			set
			{
				BoltWebOnBeam.EdgeDistLongDir = value;
				if (BoltStagger == EBoltStagger.None)
				{
					BoltOslOnSupport.EdgeDistLongDir_User = true;
					BoltOslOnSupport.EdgeDistLongDir = value;
					OnPropertyChanged("BoltOslOnSupportEdgeDistanceLong");
					OnPropertyChanged("BoltOslOnSupport.EdgeDistLongDir_User");
				}
			}
		}

		public EConnectionStyle SupportSideConnection // OSL Side
		{
			get
			{
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
					_supportSideConnection = EConnectionStyle.Welded;
				return _supportSideConnection;
			}
			set { _supportSideConnection = value; }
		}

		public EConnectionStyle BeamSideConnection { get; set; }

		public Shape Size { get; set; }

		public string SizeName
		{
			get { return Size.Name; }
			set
			{
				if (value != null)
					Size = CommonDataStatic.AllShapes[value];
				else
					Size = CommonDataStatic.AllShapes[ConstString.NONE];
			}
		}

		public double WeldSizeSupport //  DESGEN.ClipAngle.OslWeld
		{
			get { return NumberFun.Round(_weldSizeSupport, 16); }
			set { _weldSizeSupport = value; }
		}

		public double WeldSizeBeam // DESGEN.ClipAngle.WWeld
		{
			get { return NumberFun.Round(_weldSizeBeam, 16); }
			set { _weldSizeBeam = value; }
		}

		public double Length { get; set; }

		public double ShortLeg
		{
			get { return Math.Min(Size.d, Size.b); }
		}

		public double LongLeg
		{
			get { return Math.Max(Size.d, Size.b); }
		}

		public double LengthOfOSL
		{
			get { return OSL == EOSL.ShortLeg ? ShortLeg : LongLeg; }
		}

		public double OppositeOfOSL // wl - LengthOfOSL reversed
		{
			get { return OSL == EOSL.ShortLeg ? LongLeg : ShortLeg; }
		}

		public double Thickness
		{
			get { return Size.t; }
		}

		public double OSLTop;
		public double OSLBot;
		public double WTop;
		public double WBot;

		public double V;
		public double h;

		public int Number // Number of Clip Angles in use
		{
			get
			{
				switch (SizeType)
				{
					case EWebConnectionSize.L:
						return 1;
					case EWebConnectionSize.L2:
						return 2;
					default:
						return 0;
				}
			}
		}

		#endregion

		#region Formally BraceConnect data

		/// <summary>
		/// Sets the lable in the UI to be beam or gusset depending on the current Member type
		/// </summary>
		public string ClipAngleLabel
		{
			get { return MiscMethods.IsBeam(CommonDataStatic.SelectedMember.MemberType) ? "Beam" : "Gusset"; }
		}

		public string TopToBoltLabel
		{
			get
			{
				if (MiscMethods.IsBrace(CommonDataStatic.SelectedMember.MemberType))
				{
					switch (Position)
					{
						case EPosition.Top:
							return "Top of Gusset to Bolt";
						case EPosition.Bottom:
							return "Bottom of Gusset to Bolt";
						default:
							return "Gusset to Bolt";
					}
				}
				else
				{
					switch (Position)
					{
						case EPosition.Top:
							return "Top of Beam to Bolt";
						case EPosition.Bottom:
							return "Bottom of Beam to Bolt";
						default:
							return "Gusset to Bolt";
					}
				}
			}
		}

		public Bolt BoltOnColumn { get; set; }
		public Bolt BoltOnGusset { get; set; }

		public double WeldSize
		{
			get { return NumberFun.Round(_weldSize, 16); }
			set { _weldSize = value; }
		}

		public double GussetSideGage { get; set; }
		public double TopOfBeamToBolt { get; set; }

		public bool AnglesBoltedToGusset { get; set; }

		public EPosition Position
		{
			get { return _position; }
			set
			{
				_position = value;
				OnPropertyChanged("IsTopOfBeamToBoltEnabled");
			}
		}

		public bool IsMatchOtherSidesBoltsEnabled
		{
			get
			{
				if (CommonDataStatic.SelectedMember.MemberType == EMemberType.RightBeam && Position_User)
					return CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].WinConnect.ShearClipAngle.Position != EPosition.MatchOtherSideBolts;
				else if (CommonDataStatic.SelectedMember.MemberType == EMemberType.LeftBeam && Position_User)
					return CommonDataStatic.DetailDataDict[EMemberType.RightBeam].WinConnect.ShearClipAngle.Position != EPosition.MatchOtherSideBolts;
				else
					return false;
			}
		}

		public double Fillet
		{
			get { return Size.kdes; }
		}

		[XmlIgnore]
		public double ForceY; // BRACE1.ClipForceY[m]
		[XmlIgnore]
		public double ForceX; // BRACE1.ClipForceX[m]
		[XmlIgnore]
		public double MinBolts; // BRACE1.clipangleminbolts

		#endregion

		// All bools for DSCCheckBoxValue bindings
		public bool SizeName_User { get; set; }
		public bool WeldSize_User { get; set; }
		public bool GussetSideGage_User { get; set; }
		public bool TopOfBeamToBolt_User { get; set; }
		public bool Size_User { get; set; }
		public bool WeldSizeSupport_User { get; set; }
		public bool WeldSizeBeam_User { get; set; }

		public bool Position_User
		{
			get { return _positionUser; }
			set
			{
				_positionUser = value;
				OnPropertyChanged("Position_User");
				OnPropertyChanged("IsMatchOtherSidesBoltsEnabled");
			}
		}
	}

	/// <summary>
	/// ControlCOShearTee: Web Connection - Tee
	/// </summary>
	[Serializable]
	public class WCShearWebTee : IConnectDescon
	{
		private double _weldSizeFlange;
		private double _weldSizeStem;
		private EBoltStagger _boltStagger;
		private double _sLength;
		private double _fLength;
		private bool _positionUser;

		public WCShearWebTee()
		{
			Size = new Shape();
			BoltWebOnStem = CommonDataStatic.Preferences.DefaultBolt;
			BoltOslOnFlange = CommonDataStatic.Preferences.DefaultBolt;
			Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
			Material = CommonDataStatic.Preferences.DefaultMaterials.WTShape.ShallowCopy();
			Eh2 = ConstNum.ONEANDHALF_INCHES;
			OSLConnection = EConnectionStyle.Bolted;
			BeamSideConnection = EConnectionStyle.Bolted;
		}

		/// <summary>
		/// Creates a copy that can be edited without altering the original data. Used to mirror left and right beams.
		/// </summary>
		public WCShearWebTee ShallowCopy()
		{
			return (WCShearWebTee)MemberwiseClone();
		}

		public EPosition Position { get; set; }

		public bool IsMatchOtherSidesBoltsEnabled
		{
			get
			{
				if (CommonDataStatic.SelectedMember.MemberType == EMemberType.RightBeam && Position_User)
					return CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].WinConnect.ShearWebTee.Position != EPosition.MatchOtherSideBolts;
				else if (CommonDataStatic.SelectedMember.MemberType == EMemberType.LeftBeam && Position_User)
					return CommonDataStatic.DetailDataDict[EMemberType.RightBeam].WinConnect.ShearWebTee.Position != EPosition.MatchOtherSideBolts;
				else
					return false;
			}
		}

		public EBoltStagger BoltStagger
		{
			get { return _boltStagger; }
			set
			{
				_boltStagger = value;
				OnPropertyChanged("BoltStagger");
			}
		}

		public Bolt BoltOslOnFlange { get; set; } // DESGEN.FBolts - Flange
		public Bolt BoltWebOnStem { get; set; } // DESGEN.SBolts - Stem

		public EConnectionStyle OSLConnection { get; set; }
		public EConnectionStyle BeamSideConnection { get; set; }
		public Shape Size { get; set; }

		public string SizeName
		{
			get { return Size.Name; }
			set
			{
				if (value != null)
					Size = CommonDataStatic.ShapesTee[value];
			}
		}

		public double SLength
		{
			get { return _sLength; }
			set
			{
				_sLength = value;
				OnPropertyChanged("SLength");
			}
		}

		// Used in Unity only
		public double FLength
		{
			get
			{
				if (_fLength == 0)
					_fLength = Size.bf;
				return _fLength;
			}
			set { _fLength = value; }
		}

		public double WeldSizeFlange // DESGEN.WebTee.CWeld
		{
			get { return NumberFun.Round(_weldSizeFlange, 16); }
			set { _weldSizeFlange = value; }
		}

		public double WeldSizeStem // DESGEN.WebTee.BWeld
		{
			get { return NumberFun.Round(_weldSizeStem, 16); }
			set { _weldSizeStem = value; }
		}

		public bool Size_User { get; set; }
		public bool WeldSizeFlange_User { get; set; }
		public bool WeldSizeStem_User { get; set; }

		// These are used as aliases because whenever they change, other things must as well. Bind the UI to these.
		public double StemSpacingLongDir
		{
			get { return BoltWebOnStem.SpacingLongDir; }
			set
			{
				BoltWebOnStem.SpacingLongDir = value;
				MiscCalculationDataMethods.SetTeeLengthData(this);
			}
		}

		public double StemEdgeDistLongDir
		{
			get { return BoltWebOnStem.EdgeDistLongDir; }
			set
			{
				BoltWebOnStem.EdgeDistLongDir = value;
				MiscCalculationDataMethods.SetTeeLengthData(this);
			}
		}

		public int StemNumberOfRows
		{
			get { return BoltWebOnStem.NumberOfRows; }
			set
			{
				BoltWebOnStem.NumberOfRows = value;
				MiscCalculationDataMethods.SetTeeLengthData(this);
			}
		}

		public double FlangeSpacingLongDir
		{
			get { return BoltOslOnFlange.SpacingLongDir; }
			set
			{
				BoltOslOnFlange.SpacingLongDir = value;
				MiscCalculationDataMethods.SetTeeLengthData(this);
			}
		}

		public double FlangeEdgeDistLongDir
		{
			get { return BoltOslOnFlange.EdgeDistLongDir; }
			set
			{
				BoltOslOnFlange.EdgeDistLongDir = value;
				MiscCalculationDataMethods.SetTeeLengthData(this);
			}
		}

		public int FlangeNumberOfRows
		{
			get { return BoltOslOnFlange.NumberOfRows; }
			set
			{
				BoltOslOnFlange.NumberOfRows = value;
				MiscCalculationDataMethods.SetTeeLengthData(this);
			}
		}

		public bool FarSideWeldIsFlare;
		public bool NearSideWeldIsFlare;
		public bool NearSideWeldIsFillet;
		public bool FarSideWeldIsFillet;

		public double V;
		public double H;
		public double Eh2;
		public double BoltGageOnTeeStem; // Special value used in report and on drawing

		public bool Position_User
		{
			get { return _positionUser; }
			set
			{
				_positionUser = value;
				OnPropertyChanged("Position_User");
				OnPropertyChanged("IsMatchOtherSidesBoltsEnabled");
			}
		}
	}

	/// <summary>
	/// ControlCOShearSinglePlate: Web Connection - Single Plate
	/// </summary>
	[Serializable]
	public class WCShearWebPlate : IConnectDescon
	{
		private double _supportWeldSize;
		private double _beamWeldSize;
		private EPosition _position;
		private bool _extendedConfiguration;
		private bool _positionUser;

		public WCShearWebPlate()
		{
			Bolt = CommonDataStatic.Preferences.DefaultBolt;
			Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
			WebPlateStiffener = EWebPlateStiffener.Without;
			Eccentricity = EWebEccentricity.AISC;
			NumberOfPlatesEnum = ENumbers.One;
			Material = CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate.ShallowCopy();
			BeamIsLaterallySupported = true;
		}

		/// <summary>
		/// Creates a copy that can be edited without altering the original data. Used to mirror left and right beams.
		/// </summary>
		public WCShearWebPlate ShallowCopy()
		{
			return (WCShearWebPlate)MemberwiseClone();
		}

		public string OSLBoltDistanceLabel
		{
			get { return Position == EPosition.Bottom ? ConstString.UI_BOTTOM_TO_TOP : ConstString.UI_TOP_TO_TOP; }
		}

		public EPosition Position
		{
			get { return _position; }
			set
			{
				_position = value;
				OnPropertyChanged("OSLBoltDistanceLabel");
			}
		}

		public bool IsMatchOtherSidesBoltsEnabled
		{
			get
			{
				if (CommonDataStatic.SelectedMember.MemberType == EMemberType.RightBeam && Position_User)
					return CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].WinConnect.ShearWebPlate.Position != EPosition.MatchOtherSideBolts;
				else if (CommonDataStatic.SelectedMember.MemberType == EMemberType.LeftBeam && Position_User)
					return CommonDataStatic.DetailDataDict[EMemberType.RightBeam].WinConnect.ShearWebPlate.Position != EPosition.MatchOtherSideBolts;
				else
					return false;
			}
		}

		public Bolt Bolt { get; set; }

		/// <summary>
		/// This is an alias to the internal bolt value used to the ExtendedConfiguration value. This value should only be used for
		/// binding in the UI.
		/// </summary>
		public int BoltNumberOfLines
		{
			get { return Bolt.NumberOfLines; }
			set
			{
				Bolt.NumberOfLines = value;
				if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice)
				{
					if (BoltNumberOfLines_User && Bolt.NumberOfLines > 1)
					{
						ExtendedConfiguration = true;
						OnPropertyChanged("ExtendedConfiguration");
					}
				}
			}
		}

		public bool BoltNumberOfLines_User
		{
			get { return Bolt.NumberOfBolts_User; }
			set { Bolt.NumberOfBolts_User = value; }
		}

		public bool ExtendedConfiguration
		{
			get { return _extendedConfiguration; }
			set
			{
				_extendedConfiguration = value;
				if (!value)
				{
					BoltNumberOfLines_User = false;
					WebPlateStiffener = EWebPlateStiffener.Without;
					OnPropertyChanged("BoltNumberOfLines_User");
					OnPropertyChanged("WebPlateStiffener");
				}
			}
		}


		public double SupportWeldSize // DESGEN.WebPlate.SupSideW
		{
			get { return NumberFun.Round(_supportWeldSize, 16); }
			set { _supportWeldSize = value; }
		}

		public double BeamWeldSize // DESGEN.ExtWebPLTopandBottomWeld, DESGEN.WebPlate.BSideW
		{
			get { return NumberFun.Round(_beamWeldSize, 16); }
			set { _beamWeldSize = value; }
		}

		public double Width { get; set; }
		public double Length { get; set; }

		public double Thickness // DESGEN.WebPlate.t, can be overwritten if Thickness is greater than the max	
		{
			get { return _thickness; }
			set
			{
				double minThickness = CommonDataStatic.Preferences.MinThicknessSinglePlate;
				if (!Thickness_User && value < minThickness)
					_thickness = minThickness;
				else
					_thickness = value;
			}
		}

		// Single Plate Only
		public bool BeamIsLaterallySupported { get; set; }

		/// <summary>
		/// Used for radio button in UI
		/// </summary>
		public ENumbers NumberOfPlatesEnum
		{
			get { return _numberOfPlatesEnum; }
			set
			{
				_numberOfPlatesEnum = value;
				NumberOfPlates = value == ENumbers.One ? 1 : 2;
			}
		}

		public int NumberOfPlates;

		public double TopOffset { get; set; } // DESGEN.ExtendedWebPlate.TopOffSet / DESGEN.WebPlate.t (Top of Beam to Stiffener (h))
		public double Height { get; set; } // DESGEN.ExtendedWebPlate.Height (Clear Spacing of Stiffener (H))
		public double StiffenerThickness { get; set; }
		public EWebPlateStiffener WebPlateStiffener { get; set; }
		public EWebEccentricity Eccentricity { get; set; }
		public EWeldType SupportWeldType { get; set; }

		// The following are used in the Unity code
		public double h1;
		public double h2;
		private ENumbers _numberOfPlatesEnum;
		private double _thickness;

		// All bools for DSCCheckBoxValue bindings
		public bool SupportWeldSize_User { get; set; }
		public bool Thickness_User { get; set; }
		public bool TopOffset_User { get; set; }
		public bool Height_User { get; set; }
		public bool StiffenerThickness_User { get; set; }

		public bool Position_User
		{
			get { return _positionUser; }
			set
			{
				_positionUser = value;
				OnPropertyChanged("Position_User");
				OnPropertyChanged("IsMatchOtherSidesBoltsEnabled");
			}
		}

		// Calculated in the Single Plate With Moment calcs and used in Unity
		public double Clip { get; set; }

		// The following are for Braces only
		public double BraceDistanceToFirstBolt;
		public double BraceBoltToVertEdgeDistGussetBeam;

		public bool BraceDistanceToFirstBolt_User;
	}

	/// <summary>
	/// ControlCOShearEndPlate: Web Connection - End Plate
	/// </summary>
	[Serializable]
	public class WCShearEndPlate : IConnectDescon
	{
		private Bolt _bolt;
		private EPosition _position;
		private bool _positionUser;

		public WCShearEndPlate()
		{
			Bolt = CommonDataStatic.Preferences.DefaultBolt;
			Bolt.NumberOfLines = 2;
			Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
			Position = EPosition.Top;
			Material = CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate.ShallowCopy();
			BoltDistanceToTopBeam = ConstNum.THREE_INCHES;
		}

		/// <summary>
		/// Creates a copy that can be edited without altering the original data. Used to mirror left and right beams.
		/// </summary>
		public WCShearEndPlate ShallowCopy()
		{
			return (WCShearEndPlate)MemberwiseClone();
		}

		private void SetData()
		{
			TOBtoFirstBolt = ConstNum.THREE_INCHES;
			BoltToHorizEdgeDist = ConstNum.ONEANDHALF_INCHES;
			Length = (_bolt.NumberOfRows - 1) * _bolt.SpacingTransvDir + 2 * BoltToHorizEdgeDist;
		}

		public Bolt Bolt
		{
			get { return _bolt; }
			set
			{
				_bolt = value;
				SetData();
			}
		}

		public double WeldSize { get; set; } // DESGEN.ShearEndPlate.BSideW
		public double Width { get; set; }
		public double Length { get; set; }
		public double Thickness { get; set; }
		public double TrSpacingOut { get; set; }
		public double TrSpacing3 { get; set; }

		// End Plate Only
		public double BoltDistanceToTopBeam { get; set; }

		public string OSLBoltDistanceLabel
		{
			get { return Position == EPosition.Bottom ? ConstString.UI_BOTTOM_TO_TOP : ConstString.UI_TOP_TO_TOP; }
		}

		public EPosition Position
		{
			get { return _position; }
			set
			{
				_position = value;
				OnPropertyChanged("OSLBoltDistanceLabel");
			}
		}

		public bool IsMatchOtherSidesBoltsEnabled
		{
			get
			{
				if (CommonDataStatic.SelectedMember.MemberType == EMemberType.RightBeam && Position_User)
					return CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].WinConnect.ShearEndPlate.Position != EPosition.MatchOtherSideBolts;
				else if (CommonDataStatic.SelectedMember.MemberType == EMemberType.LeftBeam && Position_User)
					return CommonDataStatic.DetailDataDict[EMemberType.RightBeam].WinConnect.ShearEndPlate.Position != EPosition.MatchOtherSideBolts;
				else
					return false;
			}
		}

		public double BoltToHorizEdgeDist { get; set; } // Lv
		public double TOBtoFirstBolt { get; set; }

		public double ConnectionPlateThickness { get; set; }

		// All bools for DSCCheckBoxValue bindings
		public bool WeldSize_User { get; set; }
		public bool Thickness_User { get; set; }
		public bool BoltDistanceToTopBeam_User { get; set; }
		public bool TOBtoFirstBolt_User { get; set; }
		public bool ConnectionPlateThickness_User { get; set; }

		public bool Position_User
		{
			get { return _positionUser; }
			set
			{
				_positionUser = value;
				OnPropertyChanged("Position_User");
				OnPropertyChanged("IsMatchOtherSidesBoltsEnabled");
			}
		}
	}

	#endregion

	#region Moment Connection Button

	/// <summary>
	/// ControlCOMomentEndPlate: End Plate - Shear and Moment
	/// </summary>
	[Serializable]
	public class WCMomentEndPlate : IConnectDescon
	{
		private double _flangeWeldSize;
		private double _webWeldNearSize;
		private double _webWeldRestSize;
		private double _boltTransSpacing3;

		public WCMomentEndPlate()
		{
			Bolt = CommonDataStatic.Preferences.DefaultBolt;
			Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
			BraceStiffener = new BraceStiffener();
			FlangeWeldType = EWeldType.CJP;
			Material = CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate.ShallowCopy();
		}

		public Bolt Bolt { get; set; }
		public EWeldType FlangeWeldType { get; set; } // DESGEN.MomentEndPlate.FlangeWeldType
		public double Width { get; set; }
		public double Length { get; set; }
		public double Thickness { get; set; } // DESGEN.MomentEndPlate.t

		public double FlangeWeldSize // DESGEN.MomentEndPlate.FlangeWeld
		{
			get { return NumberFun.Round(_flangeWeldSize, 16); }
			set { _flangeWeldSize = value; }
		}

		public double WebWeldNearSize // DESGEN.MomentEndPlate.WebWeldNearFlangeS
		{
			get { return NumberFun.Round(_webWeldNearSize, 16); }
			set { _webWeldNearSize = value; }
		}

		public double WebWeldRestSize // DESGEN.MomentEndPlate.WebWeld
		{
			get { return NumberFun.Round(_webWeldRestSize, 16); }
			set { _webWeldRestSize = value; }
		}

		public double WebWeldNearLength { get; set; } // DESGEN.MomentEndPlate.WebWeldNearFlangeL
		public int AdditionalBoltsForShear { get; set; } // DESGEN.MomentEndPlate.Nshear
		public double DistanceToFirstBolt { get; set; }
		public double BoltTransSpacing1 { get; set; } // DESGEN.MomentEndPlate.TrSpacingOut
		public double BoltTransSpacing3 // Bolt Diameter * 2.67 or Bolt SpacingTransvDir (Seismic code)
		{
			get
			{
				if (_boltTransSpacing3 == 0)
					_boltTransSpacing3 = Bolt.BoltSize*2.67;
				return _boltTransSpacing3;
			}
			set { _boltTransSpacing3 = value; }
		}

		public double ConnectionPlateThickness { get; set; }

		public BraceStiffener BraceStiffener;

		// All bools for DSCCheckBoxValue bindings
		public bool Width_User { get; set; }
		public bool Length_User { get; set; }
		public bool Thickness_User { get; set; }
		public bool AdditionalBoltsForShear_User { get; set; }
		public bool DistanceToFirstBolt_User { get; set; }
		public bool FlangeWeldSize_User { get; set; }
		public bool WebWeldNearSize_User { get; set; }
		public bool WebWeldNearLength_User { get; set; }
		public bool WebWeldRestSize_User { get; set; }
	}

	/// <summary>
	/// ControlCOMomentFlangePlate: Flange Connection - Plates
	/// </summary>
	[Serializable]
	public class WCMomentFlangePlate : IConnectDescon
	{
		private EConnectionStyle _connection;
		private EMomentFlangePlateType _plateType;

		public WCMomentFlangePlate()
		{
			Bolt = CommonDataStatic.Preferences.DefaultBolt;
			Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
			Material = CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate.ShallowCopy();
			Bolt.NumberOfLines = 2;
			Connection = EConnectionStyle.Bolted;
			PlateType = EMomentFlangePlateType.FlangePlate;
		}

		/// <summary>
		/// Creates a copy that can be edited without altering the original data. Used to mirror left and right beams.
		/// </summary>
		public WCMomentFlangePlate ShallowCopy()
		{
			return (WCMomentFlangePlate)MemberwiseClone();
		}

		public Bolt Bolt { get; set; }

		public EConnectionStyle Connection
		{
			get { return _connection; }
			set
			{
				_connection = value;
				OnPropertyChanged("Connection");
			}
		} // Bolted or Welded

		public EWeldType PlateToSupportWeldType { get; set; } // DESGEN.TFlangePlate.SupSideWType, DESGEN.BFlangePlate.SupSideWType
		public EFlangeConnectionType ConnectionType { get; set; } // FullyRestrained or PartiallyRestrained

		public EMomentFlangePlateType PlateType
		{
			get { return _plateType; }
			set
			{
				_plateType = value;
				OnPropertyChanged("PlateType");
			}
		}

		// Top Section
		public double TopWidth { get; set; } // DESGEN.TFlangePlate.Width / DiaphragmD
		public double TopWidthAtColumn;

		[XmlIgnore]
		public double TopWidthUI
		{
			get { return Math.Max(TopWidth, TopWidthAtColumn); }
			set { }	// Dummy for binding
		}

		public double TopLength { get; set; }
		public double TopThickness { get; set; } // DESGEN.TFlangeSplicePlate.t
		public double TopPlateToBeamWeldSize { get; set; } // DESGEN.TFlangePlate.BSideW
		public double TopPlateToSupportWeldSize { get; set; } // DESGEN.TFlangePlate.SupSideW
		public double TopHSSSideWallWeldLength;
		public double TopMinSpacing;

		// Bottom Section
		public double BottomWidth;
		public double BottomWidthAtColumn;

		[XmlIgnore]
		public double BottomWidthUI
		{
			get { return Math.Max(BottomWidth, BottomWidthAtColumn); }
			set { }	// Dummy for binding
		}

		public double BottomLength { get; set; }
		public double BottomThickness { get; set; } // DESGEN.BFlangeSplicePlate.t
		public double BottomPlateToBeamWeldSize { get; set; } // DESGEN.BFlangePlate.BSideW
		public double BottomPlateToSupportWeldSize { get; set; } // DESGEN.BFlangePlate.SupSideW
		public double BottomWrapAroundWidth;
		public double BottomHSSSideWallWeldLength;

		public double TransvBoltSpacingOut { get; set; } // DESGEN.TFlangePlate.TrSpacingOut
		public double TransvBoltSpacing3 { get; set; }

		public double DiaphragmA { get; set; } // CutOutPLProp[0]
		public double TopWrapAroundWidth { get; set; } // DESGEN.TWRapAroundWidth / DiaphragmB
		public double DiaphragmC { get; set; } // CutOutPLProp[2]

		public bool TransvBoltSpacing_User { get; set; }

		public bool TopWidth_User { get; set; }
		public bool TopLength_User { get; set; }
		public bool TopThickness_User { get; set; }
		public bool TopPlateToBeamWeldSize_User { get; set; }
		public bool TopPlateToSupportWeldSize_User { get; set; }

		public bool BottomWidth_User { get; set; }
		public bool BottomLength_User { get; set; }
		public bool BottomThickness_User { get; set; }
		public bool BottomPlateToBeamWeldSize_User { get; set; }
		public bool BottomPlateToSupportWeldSize_User { get; set; }

		public bool DiaphragmA_User { get; set; }
		public bool TopWrapAroundWidth_User { get; set; }

		public double HSSSideWallWeldLength;

		// These are used for drawing and are set in DesignFlangeSplicePlate()
		public int FExtensionCase;
		public double FExtensionWidth;
		public double FExtensionT;
		public double FExtensionLength;
		public double FExtensionWeld;
		public double FExtensionFy;
	}

	/// <summary>
	/// ControlCOMomentTee: Flange Connection - Tee's
	/// </summary>
	[Serializable]
	public class WCMomentTee : IConnectDescon
	{
		private bool _edgeDistLongDirUser;

		public WCMomentTee()
		{
			TopTeeShape = new Shape();
			BottomTeeShape = new Shape();

			TopWeld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
			BottomWeld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();

			BoltColumnFlange = CommonDataStatic.Preferences.DefaultBolt;
			BoltBeamStem = CommonDataStatic.Preferences.DefaultBolt;
			BoltColumnFlange.EdgeDistLongDir = CommonDataStatic.Units == EUnit.US ? 1.25 : 32;
			BoltColumnFlange.NumberOfBolts = 4;
			BoltBeamStem.NumberOfLines = 2;

			TopMaterial = CommonDataStatic.Preferences.DefaultMaterials.WTShape.ShallowCopy();
			BottomMaterial = CommonDataStatic.Preferences.DefaultMaterials.WTShape.ShallowCopy();

			TeeConnectionStyle = EConnectionStyle.Bolted;

			Column_s = ConstNum.THREE_INCHES;
			Column_e = ConstNum.ONEANDHALF_INCHES;
			BottomWeldSize = ConstNum.THREE_SIXTEENTHS;
			TopWeldSize = ConstNum.THREE_SIXTEENTHS;
		}

		public string BoltColumnImageSource
		{
			get
			{
				switch (BoltColumnFlange.NumberOfBolts)
				{
					case 4:
						return "pack://application:,,,/Descon.Resources;component/Images/Drawing_Examples/Attachments.Tee.4.png";
					case 8:
						return "pack://application:,,,/Descon.Resources;component/Images/Drawing_Examples/Attachments.Tee.8.png";
					default: // 16
						return "pack://application:,,,/Descon.Resources;component/Images/Drawing_Examples/Attachments.Tee.16.png";
				}
			}
		}

		public string BoltBeamImageSource
		{
			get
			{
				switch (BoltBeamStem.NumberOfLines)
				{
					case 2:
						return "pack://application:,,,/Descon.Resources;component/Images/Drawing_Examples/Attachments.Tee.Side.2.png";
					default: // 4
						return "pack://application:,,,/Descon.Resources;component/Images/Drawing_Examples/Attachments.Tee.Side.4.png";
				}
			}
		}

		public EConnectionStyle TeeConnectionStyle { get; set; }

		public Shape TopTeeShape { get; set; }

		public string TopTeeShapeName
		{
			get { return TopTeeShape.Name; }
			set
			{
				TopTeeShape = CommonDataStatic.AllShapes[value ?? ConstString.NONE];
				if (TeeConnectionStyle == EConnectionStyle.Bolted)
				{
					BottomTeeShape = TopTeeShape.ShallowCopy();
					OnPropertyChanged("BottomTeeShape");
					OnPropertyChanged("BottomTeeShapeName");
				}
			}
		}

		public Shape BottomTeeShape { get; set; }

		public string BottomTeeShapeName
		{
			get { return BottomTeeShape.Name; }
			set
			{
				BottomTeeShape = CommonDataStatic.AllShapes[value ?? ConstString.NONE];
			}
		}

		// Top Tee Data
		public Material TopMaterial { get; set; }
		public string TopMaterialName
		{
			get { return TopMaterial.Name; }
			set { TopMaterial = CommonDataStatic.MaterialDict[value]; }
		}

		public double TopLengthAtFlange { get; set; }	// TTee.Flength
		public double TopLengthAtStem { get; set; }		// TTee.Slength

		public double TopWeldSize { get; set; }
		public Weld TopWeld { get; set; }
		public string TopWeldName
		{
			get { return TopWeld.Name; }
			set { TopWeld = CommonDataStatic.WeldDict[value]; }
		}

		// Bottom Tee Data
		public Material BottomMaterial { get; set; }
		public string BottomMaterialName
		{
			get { return BottomMaterial.Name; }
			set { BottomMaterial = CommonDataStatic.MaterialDict[value]; }
		}

		public double BottomLengthAtFlange { get; set; }	// BTee.Flength
		public double BottomLengthAtStem { get; set; }		// BTee.Slength

		public double BottomWeldSize { get; set; }
		public Weld BottomWeld { get; set; }
		public string BottomWeldName
		{
			get { return BottomWeld.Name; }
			set { BottomWeld = CommonDataStatic.WeldDict[value]; }
		}

		// Column side bolt layout (DESGEN.TeeBoltsOnFlange)
		public Bolt BoltColumnFlange { get; set; }
		public int BoltColumnFlangeNumberOfBolts
		{
			get { return BoltColumnFlange.NumberOfBolts; }
			set
			{
				BoltColumnFlange.NumberOfBolts = value;
				OnPropertyChanged("BoltColumnImageSource");
			}
		}

		public double Beam_g { get; set; }		// BTrSpacingIn
		public double Beam_g1 { get; set; }		// BTrSpacingOut

		// Beam side bolt layout (DESGEN.TeeBoltsOnStem)
		public Bolt BoltBeamStem { get; set; }
		public int BoltBeamStemNumberOfBolts
		{
			get { return BoltBeamStem.NumberOfLines; }
			set
			{
				BoltBeamStem.NumberOfLines = value;
				OnPropertyChanged("BoltBeamImageSource");
			}
		}

		public double Column_a { get; set; }	// At
		public double Column_e { get; set; }	// ec
		public double Column_s { get; set; }	// sc
		public double Column_g { get; set; }	// CTrSpacingIn
		public double Column_g1 { get; set; }	// CTrSpacingOut

		// These remaining doubles are used in the calcs, but don't seem to be used in the UI
		public double CWeld;
		public double BWeld;

		// Override for Bolt data because the default value is different
		public bool EdgeDistLongDir_User
		{
			get { return _edgeDistLongDirUser; }
			set
			{
				if (_edgeDistLongDirUser && !value)
					BoltColumnFlange.EdgeDistLongDir = CommonDataStatic.Units == EUnit.US ? 1.25 : 32;
				_edgeDistLongDirUser = value;
				OnPropertyChanged("BoltColumnFlange.EdgeDistLongDir");
			}
		}

		public bool TopTeeShape_User { get; set; }
		public bool BottomTeeShape_User { get; set; }

		public bool TopLengthAtFlange_User { get; set; }
		public bool TopLengthAtStem_User { get; set; }
		public bool BottomLengthAtFlange_User { get; set; }
		public bool BottomLengthAtStem_User { get; set; }

		public bool TopWeldSize_User { get; set; }
		public bool BottomWeldSize_User { get; set; }

		public bool Beam_g_User { get; set; }

		public bool Column_g_User { get; set; }
		public bool Column_a_User { get; set; }
		public bool Column_e_User { get; set; }
		public bool Column_s_User { get; set; }
	}

	/// <summary>
	/// ControlCOMomentAngle: Flange Connection - Angles
	/// </summary>
	[Serializable]
	public class WCMomentFlangeAngle : IConnectDescon
	{
		private double _beamWeldSize;
		private double _columnWeldSize;
		private bool _angleUser;

		public WCMomentFlangeAngle()
		{
			Angle = new Shape();
			BeamBolt = CommonDataStatic.Preferences.DefaultBolt;
			BeamBolt.NumberOfLines = 2;	// Can only be 2 or 4, but the default is 1 for normal bolts
			ColumnBolt = CommonDataStatic.Preferences.DefaultBolt;
			Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
			BeamConnection = EConnectionStyle.Bolted;
			ColumnConnection = EConnectionStyle.Bolted;
			Material = CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate.ShallowCopy();
		}

		public Shape Angle { get; set; } // "Size" in UI	

        /// <summary>
        /// Gets the value for the shorter of the angle's 2 legs.
        /// Example: If the angle is L8x4x1/2, this will return 4.
        /// </summary>
	    public double ShortLeg
	    {
            get { return Math.Min(Angle.b, Angle.d); }
	    }

        /// <summary>
        /// Gets the value for the longer of the angle's 2 legs.
        /// Example: If the angle is L8x4x1/2, this will return 8.
        /// </summary>
	    public double LongLeg
	    {
            get { return Math.Max(Angle.b, Angle.d); }
	    }

		public string AngleName
		{
			get { return Angle.Name; }
			set { Angle = CommonDataStatic.AllShapes[value ?? ConstString.NONE]; }
		}

		public bool Angle_User
		{
			get { return _angleUser; }
			set
			{
				_angleUser = value;
				OnPropertyChanged("Angle_User");
			}
		}

		public double Length { get; set; }

		public double BeamWeldSize // DESGEN.FlangeAngle.BeamWeld
		{
			get { return NumberFun.Round(_beamWeldSize, 16); }
			set { _beamWeldSize = value; }
		}

		public double ColumnWeldSize // DESGEN.FlangeAngle.SupportWeld
		{
			get { return NumberFun.Round(_columnWeldSize, 16); }
			set { _columnWeldSize = value; }
		}

		// Support Side (Column Side)
		public EConnectionStyle ColumnConnection { get; set; }
		public Bolt ColumnBolt { get; set; } // FlangeAngleOslBolts
		public double ColumnBoltSpacingOut { get; set; } // DESGEN.FlangeAngle.ColSpacingOut
		public double ColumnBoltSpacing3 { get; set; } // DESGEN -> gc2

		// Beam Side
		public EConnectionStyle BeamConnection { get; set; }
		public Bolt BeamBolt { get; set; } // FlangeAngleFBolts
		public double BeamBoltSpacing3 { get; set; } // DESGEN -> g2

		public double AngleLong;

		// All bools for DSCCheckBoxValue bindings
		public bool BeamWeldSize_User { get; set; }
		public bool ColumnWeldSize_User { get; set; }
		public bool ColumnBoltSpacing_User { get; set; }
	}

	/// <summary>
	/// ControlCOMomentDirectlyWelded: Moment Connection to Column Web
	/// This is used for Flange Plates when the Joint configuration is set to Beam to Column Web
	/// </summary>
	[Serializable]
	public class WCMomentDirectlyWelded : IConnectDescon
	{
		public WCMomentDirectlyWelded()
		{
			Top = new WCMomentDirectlyWeldedDetail();
			Bottom = new WCMomentDirectlyWeldedDetail();
			Bolt = CommonDataStatic.Preferences.DefaultBolt;
			Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
			Type = ENumbers.Zero;
			WeldFlangeType = EWeldType.CJP;
			WeldWebType = EWeldType.CJP;
			Material = CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate.ShallowCopy();
			MomentType = EDirectWeldMomentType.Lateral;

			Bolt.NumberOfLines = 2;

			Top.a = ConstNum.THREE_INCHES;
			Bottom.a = ConstNum.THREE_INCHES;
			Top.s = ConstNum.THREE_INCHES;
			Bottom.s = ConstNum.THREE_INCHES;
			Top.el = ConstNum.ONEANDHALF_INCHES;
			Bottom.el = ConstNum.ONEANDHALF_INCHES;
			Top.et = ConstNum.ONEANDHALF_INCHES;
			Bottom.et = ConstNum.ONEANDHALF_INCHES;
		}

		public Bolt Bolt { get; set; }
		public EWeldType WeldFlangeType { get; set; }
		public EWeldType WeldWebType { get; set; }

		// This is ugly, but for now it is the same as the old Descon 7 code.
		// Flange Plate mode (Design Flange Plates to Web): 0, 1, 2, 3, 4, 5
		// Directly Welded mode: 6, 7, 8, 9, 10
		public ENumbers Type { get; set; }

		public WCMomentDirectlyWeldedDetail Top { get; set; } // TMWAtt data
		public WCMomentDirectlyWeldedDetail Bottom { get; set; } // BMWAtt data

		public EDirectWeldMomentType MomentType { get; set; }
	}

	/// <summary>
	/// This is only used for WCMomentDirectlyWelded
	/// </summary>
	[Serializable]
	public sealed class WCMomentDirectlyWeldedDetail
	{
		public double StiffenerThickness { get; set; } // MWAtt.StiffenerT
		public double ExtensionThickness { get; set; } // MWAtt.ExtPlateT
		public double FlangeThickness { get; set; } // MWAtt.FlangePlateT
		public double s { get; set; }
		public double el { get; set; }
		public double et { get; set; }
		public double a { get; set; }
		public double Length { get; set; }
		public double b { get; set; }
		public double FilletWeldW1 { get; set; } // MWAtt.SupWeldF
		public double FilletWeldW2 { get; set; } // MWAtt.SupWeldW
		public double FilletWeldW3 { get; set; } // MWAtt.BSideW

		public double g1;
	}

	#endregion
}