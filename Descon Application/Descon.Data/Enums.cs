namespace Descon.Data
{
	#region General

	public enum EUnit
	{
		Metric,
		US
	}

	public enum EBracingType
	{
		OCBF,
		EBF,
		SCBF
	}

	public enum ESteelCode
	{
		AISC13,
		AISC14
	}

	public enum ECalcMode
	{
		ASD,
		LRFD
	}

	public enum ERoundingPrecision
	{
		WholeNumber,
		Half,
		Fourth,
		Eighth,
		Tenth,
		Sixteenth,
		Hundredth
	}

	public enum ERoundingStyle
	{
		Nearest,
		RoundUp,
		RoundDown,
		TowardsZero,
		TowardsInfinity
	}

	public enum EFormSelectionDataType
	{
		Welds,
		Materials,
		Shapes
	}

	public enum EMemberType
	{
		/// <summary>
		/// Can be Column, Beam, or Girder depending on the Joint Configuration
		/// </summary>
		PrimaryMember,
		/// <summary>
		/// Doubles as TColumn for Column Splice connection
		/// </summary>
		RightBeam,
		/// <summary>
		/// Doubles as BColumn for Column Splice connection
		/// </summary>
		LeftBeam,
		UpperRight,
		LowerRight,
		UpperLeft,
		LowerLeft
	}

	public enum EMemberSubType
	{
		Main,
		Beam,
		Shear,
		Moment,
		BoltShearBeam,
		BoltShearSupport,
		BoltMomentBeam,
		BoltMomentSupport,
		Stiffener
	}

	public enum EClickType
	{
		Single,
		Double,
		Right
	}

	public enum EShapeType
	{
		None,
		WideFlange,
		WTSection,
		SingleAngle,
		DoubleAngle,
		HollowSteelSection,
		SingleChannel,
		DoubleChannel,
	}

	public enum EFemaConnectionType
	{
		None,
		WUFB,
		WUFW,
		FF,
		RBS,
		WFP,
		BUEP,
		BFP,
		BSEP,
		DST
	}

	public enum EPrefsInputForce
	{
		AxialForce,
		TransferForce
	}

	public enum EPrefsTransferForceSelection
	{
		Select,
		Enter
	}

	public enum EPrefsDistanceToBraceAxisSelection
	{
		t2,
		t3,
		t4
	}

	public enum EPosition
	{
		Top,
		Center,
		Bottom,
		MatchOtherSideBolts,
		NoConnection
	}

	public enum EDesconEditBoxTypes
	{
		Integer,
		Decimal_Two_Digits,
		Decimal_Three_Digits,
		Decimal_Four_Digits,
		Fraction_Over_16,
		Feet_To_Inches,
		Text
	}

	//W Shapes: A992 – Fy=50, Fu=65
	//WT Shapes: A992 – Fy=50, Fu=65
	//HSS Shapes: A500-B-46 – Fy=46, Fu=58
	//Pipe: A53-B – Fy=35, Fu=60
	//Channels: A36 – Fy=36, Fu=58
	//Angles: A36 – Fy=36, Fu=58
	//Connection Plate: A36 – Fy=36, Fu=58
	//Gusset Plates: A36 – Fy=36, Fu=58
	//Stiffener Plates: A572-50 – Fy=50, Fu=65
	public enum EPrefsMaterialDefaults
	{
		WShapes,
		WTShapes,
		HSSShapes,
		Pipe,
		Channels,
		Angles,
		ConnectionPlate,
		GussetPlate,
		StiffenerPlate
	}

	public enum EDrawingTheme
	{
		Default,
		Muted,
		Bright
	}

	#endregion

	#region Bolt Enums

	public enum EBoltMinSpacing
	{
		DiameterX267,
		DiameterX3,
		Three,
		Custom
	}

	public enum EBoltHoleType
	{
		STD,
		OVS,
		SSLN,
		SSLP,
		LSLN,
		LSLP
	}

	public enum EBoltSurfaceClass
	{
		A,
		B,
		C
	}

	public enum EBoltType
	{
		SC,
		N,
		X
	}

	public enum EBoltASTM
	{
		A325,
		A490,
		NonASTM
	}

	public enum EBoltHoleDir
	{
		N,
		B,
		T,
		L
	}

	public enum EBoltLimitState
	{
		Serviceability,
		Strength,
		Slip
	}

	public enum EBoltFillerFactor
	{
		One,
		PointEightFive
	}

	#endregion

	#region Seismic

	public enum EResponse
	{
		RLessThan3,
		RGreaterThan3
	}

	public enum ESeismic
	{
		Seismic,
		NonSeismic
	}

	public enum ESeismicDesignType
	{
		AISC341,
		AISC358,
		FEMA
	}

	public enum EFramingSystem
	{
		IMF,
		OMF,
		SMF
	}

	public enum ESeismicType
	{
		Low,
		High
	}

	public enum EPanelZoneDeformation
	{
		IsConsidered,
		IsNotConsidered
	}

	public enum ESeismicDistance
	{
		GreaterThanColumnDepth,
		LessThanColumnDepth
	}

	public enum EPrequalifiedLimitType
	{
		PlateThickness,					// tp
		EndPlateWidth,					// bp
		BoltGage,						// g
		BoltDistanceFromBeamFlange,		// Pf
		BoltVerticalSpacing,			// Pb
		BeamDepth,						// d
		BeamFlangeThickness,			// tbf
		BeamFlangeWidth					// bbf
	}

	public enum ESeismicConnectionType
	{
		RBS,
		/// <summary>
		/// 4E
		/// </summary>
		E4,
		/// <summary>
		/// 4ES
		/// </summary>
		ES4,
		/// <summary>
		/// 8ES
		/// </summary>
		ES8,
	}

	#endregion

	#region Main Control placed on FormMain

	public enum EConnectionStyle
	{
		Welded,
		Bolted
	}

	public enum EWebOrientation
	{
		InPlane,
		OutOfPlane
	}

	public enum EDesignWebSpliceFor
	{
		Vs,
		VsTimesCmin
	}

	/// <summary>
	/// All Joint Configurations, some of which are only internally used for calculations
	/// </summary>
	public enum EJointConfiguration
	{
		// Brace and Beam to Column (Descon Brace and Win)
		BraceToColumn,
		// Descon Brace
		BraceVToBeam,
		BraceToColumnBase,
		// Descon Win
		ColumnSplice,
		BeamToGirder,
		BeamSplice,
		BeamToHSSColumn,		// Internal config only, not in menu
		BeamToColumnFlange,		// Internal config only, not in menu
		BeamToColumnWeb,		// Internal config only, not in menu
	}

	public enum EShearCarriedBy
	{
		ClipAngle,
		SinglePlate,
		EndPlate,
		Seat,
		Tee,
		// This is only used for the default shear type in Settings and automatically sets the Extentended Configurtion to true
		SinglePlateExtended
	}

	public enum EMomentCarriedBy
	{
		NoMoment,
		DirectlyWelded,
		EndPlate,
		FlangePlate,
		Tee,
		Angles
	}

	#endregion

	#region Connection Options for Win Mode

	public enum EBeamSkewOrIncline
	{
		Skewed,
		Inclined,
		PerpendicularToSupport
	}

	public enum EBeamOrAttachment
	{
		Beam,
		Angle,
		SinglePlate,
		WebTee
	}

	public enum EReinforcementType
	{
		None,
		HorizontalSiffener,
		HorizontalAndVerticalStiffener,
		DoublerPlate
	}

	public enum EShearOpt
	{
		ClipAngle,
		WebTee,
		SpliceWithMoment,
		SpliceWithoutMoment
	}

	public enum ETearOutBlockShear
	{
		TearOut,
		BlockShear
	}

	public enum ESpliceConnection
	{
		FlangePlate,
		FlangeAndWebPlate,
		ButtPlate,
		DirectlyWelded
	}

	public enum ESpliceChannelType
	{
		Temporary,
		Permanent
	}

	public enum EWeldType
	{
		/// <summary>
		/// FF (Fillet)
		/// </summary>
		Fillet,
		/// <summary>
		/// FP (Full Penetration)
		/// </summary>
		CJP,
		/// <summary>
		/// FFR
		/// </summary>
		PJP
	}

	public enum EFlangeConnectionType
	{
		FullyRestrained,
		PartiallyRestrained
	}

	public enum EShortLegOn
	{
		BeamSide,
		SupportSide
	}

	public enum ESeatConnection
	{
		Bolted,
		BoltedStiffenedPlate,
		Welded,
		WeldedStiffened
	}

	public enum ESeatStiffener
	{
		L2,
		Tee,
		Plate,
		None
	}

	public enum EOSL
	{
		LongLeg,
		ShortLeg
	}

	public enum EWebConnectionSize
	{
		L,
		L2
	}

	public enum EStiffenerLength
	{
		FullLengthOfWeb,
		PartialLengthOfWeb
	}

	public enum EStiffenerType
	{
		Inclined,
		Horizontal,
		HoritzotalTwo
	}

	public enum EBoltStagger
	{
		None,
		Support,
		Beam,
		OneLessRow
	}

	public enum EWebPlateStiffener
	{
		With,
		Without
	}

	public enum EWebEccentricity
	{
		Descon,
		AISC
	}

	// Used for Directly Welded selection mode
	public enum ENumbers
	{
		// Flange Plate mode
		Zero,	// 6 in Directly Welded
		One,	// 7 in Directly Welded
		Two,	// 8 in Directly Welded
		Three,	// 9 in Directly Welded
		Four,	// 10 in Directly Welded
		Five,
		// The rest of these are here so save files don't explode
		Six,
		Seven,
		Eight,
		Nine,
		Ten
	}

	public enum EDirectWeldMomentType
	{
		Lateral,
		Gravity
	}

	public enum EMomentFlangePlateType
	{
		FlangePlate,
		DiaphragmPlate
	}

	#endregion

	#region Connection Options for Brace Mode

	public enum EColumnSplice
	{
		UpperColumn,
		LowerColumn,
		Both
	}

	public enum EBoltedLeg
	{
		Both,
		Support,
		Beam
	}

	public enum EMomentForEquilibrium
	{
		GussetToBeam,
		GussetToColumn,
		Both
	}

	public enum EBraceConnectionTypes
	{
		DirectlyWelded,
		ClipAngle,
		ClawAngle,
		GussetPlate,
		SplicePlate,
		Brace,
		BasePlate,
		SinglePlate,
		EndPlate,
		FabricatedTee, 
        Seat,
        FlangePlate,
        Stiffener
	}

    public enum EBearingTearOut
	{
		Bearing,
		TearOut
	}

	#endregion

	#region Report and Printing Enums

	public enum EReportLineType
	{
		MainHeader,
		Header,
		GoToHeader,
		NormalLine,
		CapacityLine,
		DebugLineHeader
	}

	/// <summary>
	/// File types assigned to int values. Index for filter in save dialog starts at 1 instead of 0
	/// </summary>
	public enum EReportFileTypes
	{
		PDF = 1,
		RTF = 2,
		HTML = 3,
		TXT = 4
	}

	#endregion

	#region Licensing Enums

	/// <summary>
	/// The 6 license types. Number is the level which goes from 0 to 5
	/// </summary>
	public enum ELicenseType
	{
		Developer_0,
		Demo_1,
		Open_2,
		Basic_3,
		Standard_4,
		Next_5,
		NoMatch,
		NoSeats
	}

	#endregion

	#region Unity Enums

	public enum EOffsetType
	{
		Top,
		Right,
		Left,
		Bottom
	}

	#endregion
}