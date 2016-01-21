using System;
using System.Linq;
using System.Xml.Serialization;

namespace Descon.Data
{
	/// <summary>
	/// All Brace related data classes for each of the UI forms. BOOL values that end in _User are used to determine
	/// if the values should change when run through calculations.
	/// </summary>
	[Serializable]
	public class ConnectionOptionsBrace
	{
		public BCBasePlate BasePlate { get; set; }
		public BCClawAngles ClawAngles { get; set; }
		public BCSplicePlates SplicePlates { get; set; }
		public BCBrace Brace { get; set; }
		public BCGussetPlate Gusset { get; set; }
		public BCFabricatedTee FabricatedTee { get; set; }

		// Object initializers cannot be used for most of these classes because the properties have to be set in a specific order
		public ConnectionOptionsBrace()
		{
			BasePlate = new BCBasePlate();
			SplicePlates = new BCSplicePlates();
			Brace = new BCBrace();
			Gusset = new BCGussetPlate();
			ClawAngles = new BCClawAngles();
			FabricatedTee = new BCFabricatedTee();
		}
	}

	/// <summary>
	/// ControlAPBasePlate
	/// </summary>
	[Serializable]
	public class BCBasePlate : IConnectDescon
	{
		public BCBasePlate()
		{
			Thickness = ConstNum.TWO_INCHES;
			CornerClip = ConstNum.HALF_INCH;
			Extension = ConstNum.TWO_INCHES;
			Material = CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate;
		}

		public double CornerClip { get; set; } // BRACE1.BasePlate.clip - End Setback in UI
		public double Extension { get; set; }
		public double Thickness { get; set; }
		public double LeftEdgeToColumn { get; set; }
		public double RightEdgeToColumn { get; set; }
	}

	/// <summary>
	/// ControlAPClawAngles
	/// </summary>
	[Serializable]
	public class BCClawAngles : IConnectDescon
	{
		private bool _doNotConnectFlanges;

		public BCClawAngles()
		{
			Size = CommonDataStatic.AllShapes.FirstOrDefault(s => s.Value.TypeEnum == EShapeType.SingleAngle).Value;
			OSL = EOSL.ShortLeg;
			Length = 18 * ConstNum.ONE_INCH;

			Material = CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate;
		}

		public Shape Size;
		public bool Size_User { get; set; }

		/// <summary>
		/// Set this instead of the size itself and the Size will be set automatically
		/// </summary>
		public string SizeName
		{
			get { return Size.Name; }
			set { Size = CommonDataStatic.AllShapes[value]; }
		}

		public EOSL OSL { get; set; }

		public bool DoNotConnectFlanges
		{
			get { return _doNotConnectFlanges; }
			set
			{
				_doNotConnectFlanges = value;
				OnPropertyChanged("DoNotConnectFlanges");
			}
		}

		public double Length;

		public double LengthB
		{
			get { return (CommonDataStatic.SelectedMember.BoltGusset.NumberOfBolts - 1) * CommonDataStatic.SelectedMember.BoltGusset.SpacingLongDir + CommonDataStatic.SelectedMember.BoltGusset.EdgeDistLongDir + CommonDataStatic.SelectedMember.BoltGusset.EdgeDistBrace; }
		}

		public double LengthG
		{
			get
			{
				return ((CommonDataStatic.SelectedMember.BoltGusset.NumberOfBolts - 1) * CommonDataStatic.SelectedMember.BoltGusset.SpacingLongDir + CommonDataStatic.SelectedMember.BoltGusset.EdgeDistLongDir +
				        CommonDataStatic.DetailDataDict[CommonDataStatic.SelectedMember.MemberType].BraceConnect.Gusset.EdgeDistance);
			}
		}

		[XmlIgnore]
		public double HoleLongB;
		[XmlIgnore]
		public double HoleTransB;
		[XmlIgnore]
		public double HoleLongG;
		[XmlIgnore]
		public double HoleTransG;

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

		public double Thickness
		{
			get { return Size.t; }
		}

		public double Fillet
		{
			get { return Size.kdes; }
		}
	}

	/// <summary>
	/// ControlAPAplicePlates. The artist formally known as WebPlate
	/// </summary>
	[Serializable]
	public class BCSplicePlates : IConnectDescon
	{
		private bool _doNotConnectWeb;
		private Bolt _bolt;
		private double _lengthB;

		public BCSplicePlates()
		{
			Bolt = CommonDataStatic.Preferences.DefaultBolt;
			Width = CommonDataStatic.Units == EUnit.US ? 6 : 150;
			Length = CommonDataStatic.Units == EUnit.US ? 12 : 300;
			Thickness = CommonDataStatic.Units == EUnit.US ? .5 : 13;
			Material = CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate;

			SetData();
		}

		private void SetData()
		{
			HoleLongB = _bolt.BoltSize + ConstNum.SIXTEENTH_INCH;
			HoleLongG = _bolt.BoltSize + ConstNum.SIXTEENTH_INCH;
			HoleTransB = _bolt.BoltSize + ConstNum.SIXTEENTH_INCH;
			HoleTransG = _bolt.BoltSize + ConstNum.SIXTEENTH_INCH;
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

		public double Width { get; set; }
		public double Length { get; set; }
		public double Thickness { get; set; }

		public bool Width_User { get; set; }
		public bool Thickness_User { get; set; }

		public bool DoNotConnectWeb
		{
			get { return _doNotConnectWeb; }
			set
			{
				_doNotConnectWeb = value;
				OnPropertyChanged("DoNotConnectWeb");
			}
		}

		[XmlIgnore]
		public double LengthG
		{
			get { return (_bolt.NumberOfRows - 1) * _bolt.SpacingLongDir + _bolt.EdgeDistLongDir + _bolt.EdgeDistGusset; }
		}

		public double LengthB
		{
			get
			{
				if (_lengthB == 0)
					_lengthB = (_bolt.NumberOfRows - 1) * _bolt.SpacingLongDir + _bolt.EdgeDistLongDir + _bolt.MinEdgeSheared;
				return _lengthB;
			}
			set { _lengthB = value; }
		}

		public double HoleLongB;
		public double HoleTransB;
		public double HoleLongG;
		public double HoleTransG;
	}

	/// <summary>
	/// ControlAPBrace
	/// </summary>
	[Serializable]
	public class BCBrace : IConnectDescon
	{
		private double _weldSize;
		private double _weldLength;

		public BCBrace()
		{
			BraceWeld = new BraceWelds();
			BcBraceStiffener = new BraceStiffener();
		}

		public double DistanceFromWP { get; set; } // BRACE1.BraceSetBack

		public double WeldSize
		{
			get { return NumberFun.Round(_weldSize, 16); }
			set { _weldSize = value; }
		}

		public double Length
		{
			get { return NumberFun.Round(_weldLength, 16); }
			set { _weldLength = value; }
		}

		// From BRACE1.BRMoreData
		public double MaxForce;
		public double FlangeForceTension;
		public double FlangeForceCompression;
		public double WebForceTension;
		public double WebForceCompression;

		public bool BoltsAreStaggered; // Added to match BRACE1.BraceBoltsAreStaggered[m]
		public BraceWelds BraceWeld; // Added to match BRACE1.BraceWeld[m]
		public BraceStiffener BcBraceStiffener;

		// The follow were in BoltHoles[] in Descon 7
		public double FlangeLong;
		public double FlangeTrans;
		public double WebLong;
		public double WebTrans;

		public bool EndSetback_User { get; set; }
		public bool DistanceFromWP_User { get; set; }
		public bool WeldSize_User { get; set; }
		public bool Length_User { get; set; }
	}

	/// <summary>
	/// ControlAPGussetPlate
	/// </summary>
	[Serializable]
	public class BCGussetPlate : IConnectDescon
	{
		private bool _dontConnectColumn;
		private bool _verticalForceUser;

		public BCGussetPlate()
		{
			GussetEFCompression = new EdgeForce();
			GussetEFTension = new EdgeForce();
			Material = CommonDataStatic.Preferences.DefaultMaterials.GussetPlate;
		}

		public double BeamSideFreeEdgeX { get; set; } // HFreeX
		public double BeamSideFreeEdgeY{ get; set; } // HFreeY
		public double ColumnSideFreeEdgeY{ get; set; } // VFreeY
		public double ColumnSideFreeEdgeX{ get; set; } // VFreeX
		public double ColumnSide{ get; set; } // BRACE1.Gusset[].VColumn
		public double BeamSide { get; set; } // BRACE1.Gusset[].HBeam
		public double Thickness{ get; set; }
		public double ColumnSideSetback{ get; set; } // BRACE1.Gusset[].Gap
		public double EdgeDistance{ get; set; } // BRACE1.Gusset[m].Edge
		public double BeamWeldSize{ get; set; }
		public double ColumnWeldSize{ get; set; }

		public EdgeForce GussetEFTension;
		public EdgeForce GussetEFCompression;
		public double HoleLongP;
		public double HoleTransP;
		public double HoleLongC;
		public double HoleTransC;
		public double EincrP;
		public double Length;
		public double BraceSide;

		public double Hb { get; set; }
		public double Mb { get; set; }
		public double Hc { get; set; }
		public double Mc { get; set; }

		public EMomentForEquilibrium Moment { get; set; }
		public double VerticalForceColumn { get; set; } // BRACE1.Gusset[].Vc
		public double VerticalForceBeam { get; set; } // BRACE1.Gusset[].Vbm
		public bool DontConnectBeam { get; set; }

		public bool DontConnectColumn
		{
			get { return _dontConnectColumn; }
			set
			{
				_dontConnectColumn = value;
				if (value)
				{
					Moment = EMomentForEquilibrium.GussetToBeam;
					OnPropertyChanged("Moment");
				}
			}
		}

		public bool VerticalForce_User
		{
			get { return _verticalForceUser; }
			set
			{
				_verticalForceUser = value;
				OnPropertyChanged("VerticalForce_User");
			}
		}

		public bool ColumnSideSetBack_User { get; set; }
		public bool BeamSideFreeEdgeX_User { get; set; }
		public bool BeamSideFreeEdgeY_User { get; set; }
		public bool ColumnSideFreeEdgeY_User { get; set; }
		public bool ColumnSideFreeEdgeX_User { get; set; }
		public bool ColumnSide_User { get; set; }
		public bool BeamSide_User { get; set; }
		public bool Thickness_User { get; set; }
		public bool ColumnSideSetback_User { get; set; }
		public bool EdgeDistance_User { get; set; }
		public bool BeamWeldSize_User { get; set; }
		public bool ColumnWeldSize_User { get; set; }
	}

	[Serializable]
	public class BCFabricatedTee : IConnectDescon
	{
		private double _stemWeldSize;
		private double _flangeWeldSize;
		private double _bf;
		private double _tf;
		private double _tw;

		public BCFabricatedTee()
		{
			Bolt = CommonDataStatic.Preferences.DefaultBolt;
			FirstBoltDistance = ConstNum.THREE_INCHES;
			Eh1 = ConstNum.ONEANDHALF_INCHES;
			Eh2 = ConstNum.ONEANDHALF_INCHES;
			Ev1 = ConstNum.ONEANDHALF_INCHES;
			Material = CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate;
		}

		public Bolt Bolt { get; set; }

		public double Length { get; set; } // L

		[XmlIgnore]
		public double Height
		{
			get { return D - Tf; }
		}

		public double bf
		{
			get { return _bf == 0 ? CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.bf : _bf; }
			set { _bf = value; }
		}

		public double Tf
		{
			get { return _tf == 0 ? CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.tf : _tf; }
			set { _tf = value; }
		}

		public double Tw
		{
			get { return _tw == 0 ? CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.tw : _tw; }
			set { _tw = value; }
		}

		public double Ev1 { get; set; } // BRACE1.FabTee[].Lv
		public double FirstBoltDistance { get; set; } // ev2

		public EWeldType FlangeWeld { get; set; } // BRACE1.FlangeWeldIsFillet[m]
		public double Eh1 { get; set; } // BRACE1.FabTee[].Lh
		public double Eh2 { get; set; } // BRACE1.FabTee[].MemberLh

		public double StemWeldSize // W
		{
			get { return NumberFun.Round(_stemWeldSize, 16); }
			set { _stemWeldSize = value; }
		}

		public double FlangeWeldSize // Ef
		{
			get { return NumberFun.Round(_flangeWeldSize, 16); }
			set { _flangeWeldSize = value; }
		}

		public double D; // BRACE1.FabTee[].d

		// All bools for DSCCheckBoxValue bindings
		public bool bf_User { get; set; }
		public bool Tf_User { get; set; }
		public bool Tw_User { get; set; }
		public bool Ev1_User { get; set; }
		public bool FirstBoltDistance_User { get; set; }
		public bool Eh1_User { get; set; }
		public bool Eh2_User { get; set; }
		public bool StemWeldSize_User { get; set; }
		public bool FlangeWeldSize_User { get; set; }
	}
}