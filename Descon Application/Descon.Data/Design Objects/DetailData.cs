using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Descon.Data
{
	/// <summary>
	/// This class represents each component (Left Beam, Right Beam, Column, etc.)
	/// </summary>
	[Serializable]
	public sealed class DetailData : IConnectDescon
	{
		private EShapeType _shapeType;
		private EBraceConnectionTypes _gussetToColumnConnection;
		private EBraceConnectionTypes _braceMoreDataSelection;
		private EConnectionStyle _braceToGussetWeldedOrBolted;
		private EMomentCarriedBy _momentConnection;
		private EShearCarriedBy _shearConnection;
		private Shape _shape;
		private double _endSetback;
		private double _transferTension;
		private double _transferCompression;
		private double _axialTension;
		private double _axialCompression;
		private double _moment;
		private bool _endSetbackUser;
		private double _shearForce;
		private EWebOrientation _webOrientation;

		private readonly Preferences prefs = CommonDataStatic.Preferences;

		public DetailData()
		{
			BoltBrace = prefs.DefaultBolt.ShallowCopy();
			BoltGusset = prefs.DefaultBolt.ShallowCopy();

			WinConnect = new ConnectionOptionsWin();
			BraceConnect = new ConnectionOptionsBrace();
			BraceStiffener = new BraceStiffener();

			SlopeRise = 1;
			SlopeRun = 1;
			EndSetback = ConstNum.HALF_INCH;

			SetDefaultShearAndMomentConnections();
		}

		private void SetDefaultShearAndMomentConnections()
		{
			if (!CommonDataStatic.LoadingFileInProgress && CommonDataStatic.LicenseMinimumStandard)
			{
				switch (CommonDataStatic.BeamToColumnType)
				{
					case EJointConfiguration.BeamToColumnFlange:
						ShearConnection = prefs.DefaultConnectionTypes.BeamToColumnFlangeShear;
						MomentConnection = prefs.DefaultConnectionTypes.BeamToColumnFlangeMoment;
						break;
					case EJointConfiguration.BeamToColumnWeb:
						if (prefs.DefaultConnectionTypes.BeamToColumnWebShear == EShearCarriedBy.SinglePlateExtended)
						{
							ShearConnection = EShearCarriedBy.SinglePlate;
							WinConnect.ShearWebPlate.ExtendedConfiguration = true;
						}
						else
							ShearConnection = prefs.DefaultConnectionTypes.BeamToColumnWebShear;
						MomentConnection = prefs.DefaultConnectionTypes.BeamToColumnWebMoment;
						break;
					case EJointConfiguration.BeamToHSSColumn:
						ShearConnection = prefs.DefaultConnectionTypes.BeamToHSSShear;
						MomentConnection = prefs.DefaultConnectionTypes.BeamToHSSMoment;
						break;
					case EJointConfiguration.BeamToGirder:
						if (prefs.DefaultConnectionTypes.BeamToGirderShear == EShearCarriedBy.SinglePlateExtended)
						{
							ShearConnection = EShearCarriedBy.SinglePlate;
							WinConnect.ShearWebPlate.ExtendedConfiguration = true;
						}
						else
							ShearConnection = prefs.DefaultConnectionTypes.BeamToGirderShear;
						MomentConnection = prefs.DefaultConnectionTypes.BeamToGirderMoment;
						break;
					case EJointConfiguration.BeamSplice:
						ShearConnection = prefs.DefaultConnectionTypes.BeamSpliceShear;
						break;
				}
			}
		}

		public ConnectionOptionsWin WinConnect { get; set; } // Angles and Plates forms in old DesconWin
		public ConnectionOptionsBrace BraceConnect { get; set; } // Connection options forms in old DesconBrace

		/// <summary>
		/// Component type for this piece of data
		/// </summary>
		public EMemberType MemberType;

		/// <summary>
		/// Returns the string name of the component. Example: "Left Beam"
		/// </summary>
		public string ComponentName
		{
			get { return CommonDataStatic.CommonLists.CompleteMemberList[MemberType]; }
		}

		/// <summary>
		/// Returns true if a Shape has been selected, therefore making this component active
		/// </summary>
		public bool IsActive
		{
			get { return _shape != null && _shape.Name != ConstString.NONE; }
		}

		// These need to be overwritten because of the extra functionality to the UseHSSMaterialValues value.
		public new Material Material { get; set; }

		public new string MaterialName
		{
			get { return Material != null ? Material.Name : string.Empty; }
			set
			{
				if (!String.IsNullOrEmpty(value))
				{
					if (CommonDataStatic.MaterialDict.ContainsKey(value))
						Material = CommonDataStatic.MaterialDict[value];
					else
					{
						Material.UserDefined = true;
						CommonDataStatic.MaterialDict.Add(Material.Name, Material);
						CommonDataStatic.NeedToSaveMaterialsOrWelds = true;
					}
				}

				if (Shape != null)
				{
					if (value == ConstString.HSS_MATERIAL && ShapeType == EShapeType.HollowSteelSection)
						Shape.UseHSSMaterialValues = true;
					else
						Shape.UseHSSMaterialValues = false;
				}
			}
		}

		/// <summary>
		/// The default materials that depend on each Shape Type are set here along with some other important data
		/// </summary>
		public EShapeType ShapeType
		{
			get { return _shapeType; }
			set
			{
				_shapeType = value;
				if (!CommonDataStatic.LoadingFileInProgress)
				{
					switch (value)
					{
						case EShapeType.WideFlange:
							Material = prefs.DefaultMaterials.WShape.ShallowCopy();
							CommonDataStatic.ColumnSplice.Material = prefs.DefaultMaterials.WShape.ShallowCopy();
							break;
						case EShapeType.WTSection:
							Material = prefs.DefaultMaterials.WTShape.ShallowCopy();
							CommonDataStatic.ColumnSplice.Material = prefs.DefaultMaterials.WTShape.ShallowCopy();
							break;
						case EShapeType.SingleAngle:
						case EShapeType.DoubleAngle:
							Material = prefs.DefaultMaterials.Angle.ShallowCopy();
							CommonDataStatic.ColumnSplice.Material = prefs.DefaultMaterials.Angle.ShallowCopy();
							break;
						case EShapeType.SingleChannel:
						case EShapeType.DoubleChannel:
							Material = prefs.DefaultMaterials.Channel.ShallowCopy();
							CommonDataStatic.ColumnSplice.Material = prefs.DefaultMaterials.Channel.ShallowCopy();
							break;
						case EShapeType.HollowSteelSection:
							Material = prefs.DefaultMaterials.HSSShape.ShallowCopy();
							CommonDataStatic.ColumnSplice.Material = prefs.DefaultMaterials.HSSShape.ShallowCopy();
							break;
					}
				}

				if (CommonDataStatic.CommonLists.BraceMoreDataComponentList.Count > 0)
					BraceMoreDataSelection = CommonDataStatic.CommonLists.BraceMoreDataComponentList.Keys.First();
			}
		}

		public Shape Shape
		{
			get { return _shape; }
			set
			{
				_shape = value;
				OnPropertyChanged("BraceMoreDataComponentList");
				OnPropertyChanged("GageOnFlange");
				OnPropertyChanged("TransferTension");
				OnPropertyChanged("TransferCompression");
				OnPropertyChanged("AxialTension");
				OnPropertyChanged("AxialCompression");

				if (CommonDataStatic.CommonLists.BraceMoreDataComponentList.Count > 0)
					BraceMoreDataSelection = CommonDataStatic.CommonLists.BraceMoreDataComponentList.Keys.First();
			}
		}

		public string ShapeName
		{
			get { return _shape != null ? _shape.Name : ConstString.NONE; }
		}

		public EBraceConnectionTypes GussetToColumnConnection
		{
			get { return _gussetToColumnConnection; }
			set
			{
				_gussetToColumnConnection = value;
				OnPropertyChanged("BraceMoreDataComponentList");
				if (CommonDataStatic.CommonLists.BraceMoreDataComponentList.Count > 0)
					BraceMoreDataSelection = CommonDataStatic.CommonLists.BraceMoreDataComponentList.Keys.First();
			}
		}

		public EBraceConnectionTypes GussetToBeamConnection { get; set; }

		public EConnectionStyle BraceToGussetWeldedOrBolted
		{
			get { return _braceToGussetWeldedOrBolted; }
			set
			{
				_braceToGussetWeldedOrBolted = value;
				OnPropertyChanged("BraceMoreDataComponentList");
				if (CommonDataStatic.CommonLists.BraceMoreDataComponentList.Count > 0)
					BraceMoreDataSelection = CommonDataStatic.CommonLists.BraceMoreDataComponentList.Keys.First();
			}
		}

		/// <summary>
		/// Bolt uses for braces. This is not some kind of generic bolt for the member.
		/// </summary>
		public Bolt BoltBrace { get; set; }
		public Bolt BoltGusset { get; set; }

		// These are set in InitializeData.SetGageValues
		public double GageOnColumn { get; set; }		// Always the Column Gage value (g1)
		public bool GageOnColumn_User { get; set; }
		public double GageOnFlange { get; set; }		// If no moment than uses Column Gage (g1), otherwise is set to Beam Gage (g1). Logic in InitializeData.cs
		public bool GageOnFlange_User { get; set; }

		public EWebOrientation WebOrientation
		{
			get { return _webOrientation; }
			set
			{
				_webOrientation = value;
				// Resets the default shear and moments for other members if the web orientation is changed
				if (MemberType == EMemberType.PrimaryMember && CommonDataStatic.DetailDataDict != null)
				{
					foreach (var detailData in CommonDataStatic.DetailDataDict)
					{
						if (detailData.Value.MemberType != EMemberType.PrimaryMember && !detailData.Value.IsActive)
							detailData.Value.SetDefaultShearAndMomentConnections();
					}
				}
			}
		}

		public bool GussetWeldedToColumn { get; set; }
		public bool OSLOnBeamSide { get; set; }

		public EShearCarriedBy ShearConnection
		{
			get {return _shearConnection; }
			set
			{
				bool shearValueChanged = _shearConnection != value;

				if (!CommonDataStatic.LoadingFileInProgress && !CommonDataStatic.CommonLists.ShearCarriedByList.ContainsKey(value))
					_shearConnection = CommonDataStatic.CommonLists.ShearCarriedByList.FirstOrDefault().Key;
				else
					_shearConnection = value;

				if (CommonDataStatic.LoadingFileInProgress)
					return;

				// If Moment is 0, set to No Moment. If the new Moment List does not contain the chosen moment, reset it
				if (Moment == 0)
					MomentConnection = EMomentCarriedBy.NoMoment;
				else if (shearValueChanged && !CommonDataStatic.CommonLists.MomentCarriedByList().ContainsKey(MomentConnection))
					MomentConnection = ResetMoment();
			}
		}

		/// <summary>
		/// Diaphragm is only used for UI
		/// </summary>
		public EMomentCarriedBy MomentConnection
		{
			get { return _momentConnection; }
			set
			{
				_momentConnection = value;

				if (!CommonDataStatic.LoadingFileInProgress && _momentConnection == EMomentCarriedBy.NoMoment)
					Moment = 0;
				OnPropertyChanged("MomentConnection");
			}
		}

		public double Moment
		{
			get { return _moment; }
			set
			{
				if (!CommonDataStatic.LoadingFileInProgress)
				{
					// Only set the Moment Connection when moment is changed from 0 to something else and the current moment is not No Moment
					if (_moment == 0 && value != 0 && _momentConnection == EMomentCarriedBy.NoMoment)
						MomentConnection = ResetMoment();
					else if (_momentConnection != EMomentCarriedBy.NoMoment && value == 0)
						MomentConnection = EMomentCarriedBy.NoMoment;
				}

				_moment = value;
				OnPropertyChanged("Moment");
			}
		}

		private EMomentCarriedBy ResetMoment()
		{
			if (CommonDataStatic.LicenseMinimumStandard)
			{
				switch (CommonDataStatic.BeamToColumnType)
				{
					case EJointConfiguration.BeamToColumnFlange:
						if (CommonDataStatic.CommonLists.MomentCarriedByList().ContainsKey(prefs.DefaultConnectionTypes.BeamToColumnFlangeMoment))
							return prefs.DefaultConnectionTypes.BeamToColumnFlangeMoment;
						else
							return EMomentCarriedBy.DirectlyWelded;
					case EJointConfiguration.BeamToColumnWeb:
						if (CommonDataStatic.CommonLists.MomentCarriedByList().ContainsKey(prefs.DefaultConnectionTypes.BeamToColumnWebMoment))
							return prefs.DefaultConnectionTypes.BeamToColumnWebMoment;
						else
							return EMomentCarriedBy.DirectlyWelded;
					case EJointConfiguration.BeamToHSSColumn:
						if (CommonDataStatic.CommonLists.MomentCarriedByList().ContainsKey(prefs.DefaultConnectionTypes.BeamToHSSMoment))
							return prefs.DefaultConnectionTypes.BeamToHSSMoment;
						else
							return EMomentCarriedBy.DirectlyWelded;
					case EJointConfiguration.BeamToGirder:
						if (CommonDataStatic.CommonLists.MomentCarriedByList().ContainsKey(prefs.DefaultConnectionTypes.BeamToGirderMoment))
							return prefs.DefaultConnectionTypes.BeamToGirderMoment;
						else
							return EMomentCarriedBy.DirectlyWelded;
					default:
						return EMomentCarriedBy.NoMoment;
				}
			}
			else
			{
				switch (_shearConnection)
				{
					case EShearCarriedBy.ClipAngle:
					case EShearCarriedBy.SinglePlate:
					case EShearCarriedBy.Tee:
						if (CommonDataStatic.CommonLists.MomentCarriedByList().ContainsKey(EMomentCarriedBy.FlangePlate))
							return EMomentCarriedBy.FlangePlate;
						else
							return EMomentCarriedBy.DirectlyWelded;
					case EShearCarriedBy.EndPlate:
						if (CommonDataStatic.CommonLists.MomentCarriedByList().ContainsKey(EMomentCarriedBy.EndPlate))
							return EMomentCarriedBy.EndPlate;
						else
							return EMomentCarriedBy.DirectlyWelded;
					default:
						return EMomentCarriedBy.NoMoment;
				}
			}
		}

		public BraceStiffener BraceStiffener;
		public double MinThickness;

		// ControlBrace elements
		public double WorkPointX { get; set; }
		public double WorkPointY { get; set; }

		/// <summary>
		/// This represents the P value from Descon Win. It returns the max of all the tensions and compressions.
		/// </summary>
		public double P
		{
			get
			{
				return Math.Max(Math.Max(TransferCompression, TransferTension),
					Math.Max(AxialCompression, AxialTension));
			}
		}

		public double AxialTension
		{
			get { return _axialTension; }
			set
			{
				_axialTension = value;
				OnPropertyChanged("AxialTension");
			}
		}

		public double AxialCompression
		{
			get { return _axialCompression; }
			set
			{
				_axialCompression = value;
				OnPropertyChanged("AxialCompression");
			}
		}

		public double TransferTension
		{
			get { return _transferTension; }
			set
			{
				_transferTension = value;
				OnPropertyChanged("TransferTension");
			}
		}

		public double TransferCompression
		{
			get { return _transferCompression; }
			set
			{
				_transferCompression = value;
				OnPropertyChanged("TransferCompression");
			}
		}

		public bool AxialTension_User { get; set; }
		public bool AxialCompression_User { get; set; }

		public double SlopeRun { get; set; }
		public double SlopeRise { get; set; }

		/// <summary>
		/// ercl in Descon 7
		/// </summary>
		public double EndSetback
		{
			get { return _endSetback; }
			set
			{
				_endSetback = value;
				OnPropertyChanged("EndSetback");
			}
		}

		public bool EndSetback_User
		{
			get { return _endSetbackUser; }
			set
			{	// Switching from set by user to set by calcs
				if (_endSetbackUser && !value && !CommonDataStatic.LoadingFileInProgress)
					EndSetback = ConstNum.HALF_INCH;
				_endSetbackUser = value;
				OnPropertyChanged("EndSetback_User");
			}
		}

		public double ShearForce
		{
			get { return _shearForce; }
			set
			{
				_shearForce = value;
				OnPropertyChanged("ShearForce");
			}
		}

		// Following properties taken from BRACE1.MemberProperties (I may need more from here later)
		public double Angle
		{
			get { return NumberFun.AngleFromRiseAndRun(MemberType); }
		}

		public double FxP;

		public double PFlange;
		public double PWeb;
		public double Length { get; set; }

		// These value are determined by which components are currently active
		public bool KBrace;
		public bool KneeBrace;

		// Global properties in BRACE1
		public double AngleColumnSide;
		public double AngleBeamSide;

		// Seismic Related Settings
		public bool Slender;
		public double Wno;

		#region Properties that don't need savin'

		[XmlIgnore]
		public Dictionary<EBraceConnectionTypes, string> BraceMoreDataComponentList
		{
			get { return CommonDataStatic.CommonLists.BraceMoreDataComponentList; }
		}

		[XmlIgnore]
		public EBraceConnectionTypes BraceMoreDataSelection
		{
			get { return _braceMoreDataSelection; }
			set
			{
				_braceMoreDataSelection = value;
				OnPropertyChanged("BraceMoreDataSelection");
			}
		}

		#endregion

		#region Properties used only for the Unity Graphics

		public double AngleX { get; set; }
		public double AngleY { get; set; }

		public double BraceX { get; set; }
		public double BraceY { get; set; }

		#endregion
	}
}