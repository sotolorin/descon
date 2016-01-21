using System;
using System.Xml.Serialization;

namespace Descon.Data
{
	[Serializable]
	public class ColumnStiffener : IConnectDescon
	{
		public ColumnStiffener()
		{
			Weld = CommonDataStatic.WeldDict[CommonDataStatic.Preferences.DefaultElectrodeName];
			Material = CommonDataStatic.Preferences.DefaultMaterials.StiffenerPlate.ShallowCopy();
			UseExtendedDoublerPlate = true;
			StiffenerType = EStiffenerType.Inclined;
		}

		// These are used by Unity
		[XmlIgnore]
		public bool RightStiffenerRequired
		{
			get { return RCompStifForce != 0 || RTenstifForce != 0; }
		}

		[XmlIgnore]
		public bool LeftStiffenerRequired
		{
			get { return LCompStifForce != 0 || LTenstifForce != 0; }
		}

		/// <summary>
		/// If only one stiffener is required we give the user the option to chose Inclined or Horizontal
		/// </summary>
		[XmlIgnore]
		public bool IsHorizOptionEnabled
		{
			get
			{
				if (RightStiffenerRequired != LeftStiffenerRequired)
				{
					return true;
				}
				else
				{
					if (StiffenerType == EStiffenerType.Horizontal)
					{
						StiffenerType = EStiffenerType.Inclined;
						OnPropertyChanged("StiffenerType");
					}
					return false;
				}
			}
		}

		public bool IsPartialLengthOfWebEnabled
		{
			get
			{
				if (CommonDataStatic.DetailDataDict[EMemberType.RightBeam].MomentConnection != EMomentCarriedBy.NoMoment &&
				    CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].MomentConnection != EMomentCarriedBy.NoMoment)
					return false;
				else
					return true;
			}
		}

		/// <summary>
		/// If both stiffeners are required we give the user the option to chose Two Horizontal instead of inclined
		/// </summary>
		[XmlIgnore]
		public bool IsTwoHorizOptionEnabled
		{
			get
			{
				if (RightStiffenerRequired && LeftStiffenerRequired)
				{
					return true;
				}
				else
				{
					if (StiffenerType == EStiffenerType.HoritzotalTwo)
					{
						StiffenerType = EStiffenerType.Inclined;
						OnPropertyChanged("StiffenerType");
					}
					return false;
				}
			}
		}

		public EStiffenerType StiffenerType { get; set; }

		public bool BeamNearTopOfColumn
		{
			get { return _beamNearTopOfColumn; }
			set
			{
				_beamNearTopOfColumn = value;
				OnPropertyChanged("BeamNearTopOfColumn");
			}
		}

		public double TopOfBeamToColumn { get; set; }

		public double ColumnWebShearOverride { get; set; }
		public bool UseExtendedDoublerPlate { get; set; }

		// Doubler Plate
		public int DNumberOfPlates { get; set; } // DESGEN.DoublerPlate.cwsr
		public double DHorizontal { get; set; } // DESGEN.DoublerPlate.h
		public double DVertical { get; set; } // DESGEN.DoublerPlate.LvR
		public double DThickness { get; set; } // DESGEN.DoublerPlate.t1
		public double DThickness2; // DESGEN.DoublerPlate.t2
		public double DFlangeWeldSize { get; set; } // DESGEN.DoublerPlate.dbWeldF
		public double DWebWeldSize { get; set; } // DESGEN.DoublerPlate.dbWeldW

		// Stiffeners
		public EStiffenerLength StiffenerLength { get; set; } // DESGEN.ColStiffener.LOpt == "H" - Partial, "F" - Full
		public double SLength { get; set; }
		public double SWidth { get; set; }
		public double SThickness { get; set; } // DESGEN.ColStiffener.t
		public double SFlangeWeldSize { get; set; } // DESGEN.ColStiffener.Cffs
		public double SWebWeldSize { get; set; } // DESGEN.ColStiffener.Cwfs

		public double RCompStifForce;
		public double LCompStifForce;
		public double RTenstifForce;
		public double LTenstifForce;

		public double DColShr;
		public bool CompStiff;
		public bool TenStiff;
		public double LDoublerPlateBottom;
		public double RDoublerPlateBottom;
		public double LvL;
		public double Dpt;

		public double LPuf;
		public double RPuf;
		public double LRy;
		public double RRy;
		public double LDepth;
		public double RDepth;

		public double RDblextension;
		public double LDblextension;
		private bool _beamNearTopOfColumn;

		public bool ColumnWebShear_User { get; set; }
		public bool SWidth_User { get; set; }
		public bool SThickness_User { get; set; }
		public bool SFlangeWeldSize_User { get; set; }
		public bool SWebWeldSize_User { get; set; }
		public bool DNumberOfPlates_User { get; set; }
		public bool DHorizontal_User { get; set; }
		public bool DVertical_User { get; set; }
		public bool DThickness_User { get; set; }
		public bool DFlangeWeldSize_User { get; set; }
		public bool DWebWeldSize_User { get; set; }
	}
}