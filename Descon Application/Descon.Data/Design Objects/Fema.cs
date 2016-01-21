using System;
using System.Linq;

namespace Descon.Data
{
	// This class is used in WinConnect and there is one for each beam
	[Serializable]
	public class Fema : IConnectDescon
	{
		public Fema()
		{
			FlangeBolt = CommonDataStatic.Preferences.DefaultBolt.ShallowCopy();
			WebBolt = CommonDataStatic.Preferences.DefaultBolt.ShallowCopy();
			Material = CommonDataStatic.Preferences.DefaultMaterials.WShape.ShallowCopy();
			Weld = CommonDataStatic.WeldDict.FirstOrDefault().Value;
			SetData();
		}

		private void SetData()
		{
			BoltHoleSize();
		}

		public EFemaConnectionType Connection { get; set; }

		public Bolt FlangeBolt { get; set; }
		public Bolt WebBolt { get; set; }
		public double L { get; set; } // C-C Length of beam
		public double H { get; set; } // Average of story heights above and below the joint
		public double Wg { get; set; } // Beam gravity Load
		public double Pg { get; set; } // DESGEN.FEMAVariables.Vg
		public double Lp { get; set; } // Distance to Pg from Column Face
		public double Shf { get; set; } // Distance to hinge from far end column CL.)
		public bool UseNearEndDistance { get; set; }
		public double EndOffset { get; set; }
		public double BoltEdgeDistanceOnWeb { get; set; }
		public double BoltEdgeDistanceOnFlange { get; set; }
		public double DistanceFromTOB { get; set; }

		public double PlWidth;

		public bool WeldAccessHole;

		// Use to be globals in FEMA350.cs
		public double sh;
		public double Mf;
		public double Myf;
		public double Mc;
		public double Vf;
		public double shf;
		public double Cpr;
		public double Ry;
		public double Ryc;
		public double Mpr;
		public double Cy;
		public double Sh;
		public double c;
		public double pf;
		public double Vp;
		public double pg;
		public double L1;
		public double Lp_h;
		public double Se;
		public double Ze;
		public double Lpl;
		public double Vg;

		public string MaximumBeamDepth;
		public double MaximumBeamFlangeThickness;
		public double MinimumSpanToDepthRatio;

		public string BeamMaterialSpec;
		public string ColumnMaterialSpec;
		public string ColumnDepth;

		private void BoltHoleSize()
		{
			double wSTD = 0.0;
			double boltd = 0.0;
			double w = 0.0;

			switch (CommonDataStatic.Units)
			{
				case EUnit.US:
					switch (FlangeBolt.HoleType)
					{
						case EBoltHoleType.STD:
							w = FlangeBolt.BoltSize + 0.0625;
							L = (FlangeBolt.BoltSize + 0.0625);
							break;
						case EBoltHoleType.OVS:
							if (FlangeBolt.BoltSize < 0.625)
							{
								w = FlangeBolt.BoltSize + 0.125;
								L = w;
							}
							else if (FlangeBolt.BoltSize < 1)
							{
								w = FlangeBolt.BoltSize + 0.1875;
								L = w;
							}
							else if (FlangeBolt.BoltSize < 1.125)
							{
								w = FlangeBolt.BoltSize + 0.25;
								L = w;
							}
							else
							{
								w = FlangeBolt.BoltSize + 0.3125;
								L = w;
							}
							break;
						case EBoltHoleType.SSLN:
						case EBoltHoleType.SSLP:
							w = FlangeBolt.BoltSize + 0.0625;
							if (FlangeBolt.BoltSize < 0.625)
								L = (FlangeBolt.BoltSize + 0.1875);
							else if (FlangeBolt.BoltSize < 1)
								L = (FlangeBolt.BoltSize + 0.25);
							else if (FlangeBolt.BoltSize < 1.125)
								L = (FlangeBolt.BoltSize + 0.3125);
							else
								L = (FlangeBolt.BoltSize + 0.375);
							break;
						case EBoltHoleType.LSLN:
						case EBoltHoleType.LSLP:
							w = FlangeBolt.BoltSize + 0.0625;
							L = (2.5 * FlangeBolt.BoltSize);
							break;
					}
					FlangeBolt.HoleWidth = w;
					FlangeBolt.HoleLength = L;
					FlangeBolt.HoleDiameterSTD = FlangeBolt.BoltSize + 0.0625;
					break;
				case EUnit.Metric:
					boltd = FlangeBolt.BoltSize;
					if (boltd < 24)
						wSTD = boltd + 2;
					else
						wSTD = boltd + 3;
					switch (FlangeBolt.HoleType)
					{
						case EBoltHoleType.STD:
							w = wSTD;
							L = w;
							break;
						case EBoltHoleType.OVS:
							if (boltd < 22)
							{
								w = boltd + 4;
								L = w;
							}
							else if (boltd < 27)
							{
								w = boltd + 6;
								L = w;
							}
							else
							{
								w = boltd + 8;
								L = w;
							}
							break;
						case EBoltHoleType.SSLN:
						case EBoltHoleType.SSLP:
							w = wSTD;
							if (boltd < 22)
								L = (boltd + 6);
							else if (boltd < 27)
								L = (boltd + 8);
							else
								L = (boltd + 10);
							break;
						case EBoltHoleType.LSLN:
						case EBoltHoleType.LSLP:
							w = wSTD;
							L = ((int)Math.Floor(2.5 * boltd));
							break;
					}
					FlangeBolt.HoleWidth = w;
					FlangeBolt.HoleLength = L;
					FlangeBolt.HoleDiameterSTD = wSTD;
					break;
			}
		}
	}
}