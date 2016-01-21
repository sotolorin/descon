using System;
using System.Linq;
using System.Xml.Serialization;

namespace Descon.Data
{
	/// <summary>
	/// ControlCOSplice: Column Splice
	/// Special Control used for Detail for Column Splice type connection
	/// </summary>
	[Serializable]
	public class ColumnSplice : IConnectDescon
	{
		private Bolt _bolt;
		private ESpliceConnection _connectionOption;

		public ColumnSplice()
		{
			Bolt = CommonDataStatic.Preferences.DefaultBolt.ShallowCopy();
			Weld = CommonDataStatic.WeldDict.FirstOrDefault().Value;
			ConnectionOption = ESpliceConnection.FlangePlate;
			ConnectionUpper = EConnectionStyle.Bolted;
			ConnectionLower = EConnectionStyle.Bolted;
			WebWeldType = EWeldType.CJP;
			FlangeWeldType = EWeldType.CJP;
			DesignWebSpliceFor = EDesignWebSpliceFor.Vs;
			Material = CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate;
			Channel = new Shape();
			NumberOfChannelsEnum = ENumbers.One;
			SetData();
		}

		private void SetData()
		{
			BoltRowsFlangeUpper = BoltRowsFlangeLower = 3;
			BoltRowsWebUpper = BoltRowsWebUpper = 2;
			BoltVertSpacing = _bolt.MinSpacing;
			BoltHorizEdgeDistanceWeb =
			BoltHorizEdgeDistanceFlange = _bolt.EdgeDistLongDir;
			BoltVertEdgeDistanceColumn =
			BoltVertEdgeDistancePlate = _bolt.EdgeDistTransvDir;
		}

		// These are on the main ControlDetailData form and replace all of the other controls for other joint configs
		public double Compression { get; set; }	// C in Descon 7 UI, P in Descon 7 code
		public double Tension { get; set; }
		public double Moment { get; set; }
		public double Shear { get; set; }
		public double Cmin { get; set; }
		public bool UseSeismic { get; set; }
		public EDesignWebSpliceFor DesignWebSpliceFor { get; set; }			// DESGEN.Colsplice.WebshearIsVs
		public bool SMF { get; set; }
		public double ShearWithSMF { get; set; }

		public double WebShear;

		// The rest are in ControlWCSplice
		public int NumberOfChannels
		{
			get { return NumberOfChannelsEnum == ENumbers.One ? 1 : 2; }
		}
		public ENumbers NumberOfChannelsEnum { get; set; } // Used for radio buttons
		public Shape Channel { get; set; }

		public ESpliceConnection ConnectionOption
		{
			get { return _connectionOption; }
			set
			{
				_connectionOption = value;
				OnPropertyChanged("IsButtPlate");
				OnPropertyChanged("IsFlangeDataEnabled");
				OnPropertyChanged("IsWebDataEnabled");
			}
		} // DESGEN.Colsplice.BasicType

		public ESpliceChannelType ChannelType { get; set; }					// DESGEN.Colsplice.ChannelsTempOrPerm
		public EConnectionStyle ConnectionUpper { get; set; }				// DESGEN.Colsplice.PLChBoltedOrWeldedToUCol
		public EConnectionStyle ConnectionLower { get; set; }				// DESGEN.Colsplice.PLChBoltedOrWeldedToLCol

		public Bolt Bolt
		{
			get { return _bolt; }
			set
			{
				_bolt = value;
				SetData();
			}
		}

		// Make sure to check the end of the class for properties that aren't in the form
		public int BoltRowsFlangeUpper { get; set; }					// DESGEN.Colsplice.NumBoltRowsonUCF
		public int BoltRowsFlangeLower { get; set; }					// DESGEN.Colsplice.NumBoltRowsonLCF
		public int BoltRowsWebUpper { get; set; }						// DESGEN.Colsplice.NumBoltRowsonUCW
		public int BoltRowsWebLower { get; set; }						// DESGEN.Colsplice.NumBoltRowsonLCW

		public double BoltGageFlangeUpper { get; set; }					// DESGEN.Colsplice.BoltGageOnUCF
		public double BoltGageFlangeLower { get; set; }					// DESGEN.Colsplice.BoltGageOnLCF
		public double BoltGageWebUpper { get; set; }					// DESGEN.Colsplice.BoltGageOnUCW
		public double BoltGageWebLower { get; set; }					// DESGEN.Colsplice.BoltGageOnLCW
		
		// The properties that have more than one equivalent in Descon 7 just copied data around
		public double BoltVertSpacing { get; set; }						// DESGEN.Colsplice.BoltVertSponLCF, DESGEN.Colsplice.BoltVertSponUCF
		public double BoltVertEdgeDistanceColumn { get; set; }			// DESGEN.Colsplice.BoltVertEdgeonLC, DESGEN.Colsplice.BoltVertEdgeonUC
		public double BoltVertEdgeDistancePlate { get; set; }			// DESGEN.Colsplice.BoltVertEdgeonBofPF, DESGEN.Colsplice.BoltVertEdgeonTofPF, DESGEN.Colsplice.BoltVertEdgeonBofPW, DESGEN.Colsplice.BoltHorizEdgeonWPlU
		public double BoltHorizEdgeDistanceFlange { get; set; }			// DESGEN.Colsplice.BoltHorizEdgeonFPlL, DESGEN.Colsplice.BoltHorizEdgeonFPlU
		public double BoltHorizEdgeDistanceWeb { get; set; }			// DESGEN.Colsplice.BoltHorizEdgeonWPlL

		public double SpliceLengthUpperFlange { get; set; }				// DESGEN.Colsplice.FlangePLTopLength
		public double SpliceLengthUpperWeb { get; set; }				// DESGEN.Colsplice.WebPLTopLength
		public double SpliceLengthLowerFlange { get; set; }				// DESGEN.Colsplice.FlangePLBotLength
		public double SpliceLengthLowerWeb { get; set; }				// DESGEN.Colsplice.WebPLBotLength
		public double SpliceWidthFlange { get; set; }					// DESGEN.Colsplice.FlangePLWidth
		public double SpliceWidthWeb { get; set; }						// DESGEN.Colsplice.WebPLWidth
		public double SpliceThicknessFlange { get; set; }				// DESGEN.Colsplice.FlangePLThickness
		public double SpliceThicknessWeb { get; set; }					// DESGEN.Colsplice.WebPLThickness

		public double FillerLengthFlangeUpper { get; set; }				// DESGEN.Colsplice.FillerLengthUF
		public double FillerLengthWebUpper { get; set; }				// DESGEN.Colsplice.FillerLengthUW
		public double FillerLengthFlangeLower { get; set; }				// DESGEN.Colsplice.FillerLengthLF
		public double FillerLengthWebLower { get; set; }				// DESGEN.Colsplice.FillerLengthLW

		public double FillerWidthFlangeUpper { get; set; }				// DESGEN.Colsplice.FillerWidthUF
		public double FillerWidthWebUpper { get; set; }					// DESGEN.Colsplice.FillerWidthUW
		public double FillerWidthFlangeLower { get; set; }				// DESGEN.Colsplice.FillerWidthLF
		public double FillerWidthWebLower { get; set; }					// DESGEN.Colsplice.FillerWidthLW

		public double FillerThicknessFlangeUpper { get; set; }			// DESGEN.Colsplice.FillerThicknessUF
		public double FillerThicknessWebUpper { get; set; }				// DESGEN.Colsplice.FillerThicknessUW
		public double FillerThicknessFlangeLower { get; set; }			// DESGEN.Colsplice.FillerThicknessLF
		public double FillerThicknessWebLower { get; set; }				// DESGEN.Colsplice.FillerThicknessLW

		public double FilletWeldSizeFlangeUpper { get; set; }			// DESGEN.Colsplice.FlangeWeldsSize, DESGEN.Colsplice.WeldSizeUCF
		public double FilletWeldSizeWebUpper { get; set; }				// DESGEN.Colsplice.WebWeldsSize, DESGEN.Colsplice.WeldSizeUCW
		public double FilletWeldSizeFlangeLower { get; set; }			// DESGEN.Colsplice.WeldSizeLCF
		public double FilletWeldSizeWebLower { get; set; }				// DESGEN.Colsplice.WeldSizeLCW

		public EWeldType FlangeWeldType { get; set; }					// DESGEN.Colsplice.FlangeWelds
		public EWeldType WebWeldType { get; set; }						// DESGEN.Colsplice.WebWelds
		public EWeldType ButtWeldTypeLower { get; set; }				// DESGEN.Colsplice.LColButtPLweld
		public EWeldType ButtWeldTypeUpper { get; set; }				// DESGEN.Colsplice.UColButtPLweld
		
		public double ButtLength { get; set; }							// DESGEN.Colsplice.ButtPLLength
		public double ButtWidth { get; set; }							// DESGEN.Colsplice.ButtPLWidth
		public double ButtThickness { get; set; }						// DESGEN.Colsplice.ButtPLThickness
		public double ButtWeldSizeUpper { get; set; }					// DESGEN.Colsplice.UColButtPLweldSize
		public double ButtWeldSizeLower { get; set; }					// DESGEN.Colsplice.LColButtPLweldSize

		// These are used for the UI
		[XmlIgnore]
		public bool IsButtPlate
		{
			get { return ConnectionOption == ESpliceConnection.ButtPlate; }
		}

		[XmlIgnore]
		public bool IsFlangeDataEnabled
		{
			get { return ConnectionOption != ESpliceConnection.DirectlyWelded; }
		}

		[XmlIgnore]
		public bool IsWebDataEnabled
		{
			get { return ConnectionOption != ESpliceConnection.FlangePlate; }
		}

		// Properties not used in form
		public int FillerNumBoltRowsLF;
		public int FillerNumBoltRowsUF;
		public int FillerNumBoltRowsLW;
		public int FillerNumBoltRowsUW;
		public double FlangePLLength;
		public double WebPLLength;
		public double WebPLWidth;
		public double FillerWeldLengthUW;
		public double FillerWeldLengthLW;
		public double FillerWeldLengthLF;
		public double FillerWeldLengthUF;
		public double FillerWeldSizeLW;
		public double FillerWeldSizeLF;
		public double FillerWeldSizeUF;
		public double FillerWeldSizeUW;
		public double WeldLengthXLW;
		public double WeldLengthYLW;
		public double WeldLengthXLF;
		public double WeldLengthYLF;
		public double WeldLengthXUF;
		public double WeldLengthYUF;
		public double WeldLengthXUW;
		public double WeldLengthYUW;
		public double HoleHorizontal;
		public double HoleVertical;
	}
}