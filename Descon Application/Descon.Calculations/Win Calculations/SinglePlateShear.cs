using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class SinglePlateShear
	{
		internal static void DesignSinglePlateShear(EMemberType memberType, double V, double MaxL)
		{
			double WeldCapacity;
			double w_useful;
			double minweld;
			double Agv;
			double Agt;
			double Anv;
			double Ant;
			double FiRn;
			double NetShear;
			double evminR;
			double evminS;
			double minsp;
			double spPlate;
			double spBeam;
			double w1;
			double C1;
			double ew;
			double clip;
			double R;
			double wmin;
			double BearingCap;
			double EquivBoltFactor;
			double Fbs;
			double Fbe;
			double t1234;
			double Maxt;
			double sptSlenderness;
			double spt34;
			double spt12;
			double spt4;
			double FiFbcl;
			double FiFbc;
			double fd;
			double dc;
			double hh;
			double spt3Extension;
			double spt3;
			double ec;
			double ecForExtension;
			double SecModOfExtension;
			double HeightAtTransition;
			double SecMod;
			double MomInert;
			double e;
			double s;
			double n = 0;
			double d;
			double Tblockshear;
			double Lgv;
			double Lnv;
			double Lnt;
			double spt2;
			double spt1;
			double SPln;
			double MaxThickness;
			double ho;
			double cc;
			double multiplier = 0;
			double eb = 0;
			bool shart2;
			bool shart1;
			double BsCap = 0;
			double c = 0;
			double eccent;
			int Nbw;
			int Nvl;
			double ShearCap;
			double a = 0;
			int Nmn;
			double Nmx;
			double MinED;
			double Minedgedist;
			double Rlength;
			double Maximum;
			double bovt;
			double supThickness1 = 0;
			double supThickness;
			double w;
			double trequired;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			var webPlate = beam.WinConnect.ShearWebPlate;

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
			{
				supThickness = column.Shape.tf;
				supThickness1 = column.Shape.tf;
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
				{
					bovt = (column.Shape.bf - 3 * column.Shape.tf) / column.Shape.tf;
					Maximum = 1.4 * Math.Pow(ConstNum.ELASTICITY / column.Material.Fy, 0.5);
					if (bovt > Maximum)
					{
						Reporting.AddLine("HSS b/t exceeds 1.4 * (E / Fy)^0.5.  Single Plate connection is not suitable. (NG)");
						return;
					}
				}
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
			{
				if (webPlate.ExtendedConfiguration)
					Rlength = Math.Max(1, webPlate.Height);
				else
					Rlength = Math.Max(1, rightBeam.WinConnect.ShearWebPlate.Length);
				if (leftBeam.WinConnect.ShearWebPlate.Length == 0)
					leftBeam.WinConnect.ShearWebPlate.Length = 1;

				supThickness = Math.Abs(leftBeam.ShearForce / leftBeam.WinConnect.ShearWebPlate.Length) / (Math.Abs(leftBeam.ShearForce / leftBeam.WinConnect.ShearWebPlate.Length) + Math.Abs(rightBeam.ShearForce / Rlength));
				supThickness1 = column.Shape.tw;
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
			{
				if (webPlate.ExtendedConfiguration)
					Rlength = Math.Max(1, webPlate.Height);
				else
					Rlength = Math.Max(1, rightBeam.WinConnect.ShearWebPlate.Length);
				if (leftBeam.WinConnect.ShearWebPlate.Length == 0)
					leftBeam.WinConnect.ShearWebPlate.Length = 1;

				supThickness = column.Shape.tw * Math.Abs(leftBeam.ShearForce / leftBeam.WinConnect.ShearWebPlate.Length) / (Math.Abs(leftBeam.ShearForce / leftBeam.WinConnect.ShearWebPlate.Length) + Math.Abs(rightBeam.ShearForce / Rlength));
				supThickness1 = column.Shape.tw;
			}
			else
				supThickness = 0;

			if (supThickness == 0)
				supThickness = supThickness1;

			if (webPlate.Bolt.HoleLength > webPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH)
				Minedgedist = webPlate.Bolt.MinEdgeSheared + webPlate.Bolt.Eincr;
			else
				Minedgedist = webPlate.Bolt.MinEdgeSheared;
			MinED = Math.Max(Minedgedist, 2 * webPlate.Bolt.BoltSize);

			if (webPlate.Bolt.EdgeDistTransvDir < MinED)
				webPlate.Bolt.EdgeDistTransvDir = MinED;
			if (webPlate.Bolt.HoleLength > webPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH)
				Minedgedist = webPlate.Bolt.MinEdgeSheared + webPlate.Bolt.Eincr;
			else
				Minedgedist = webPlate.Bolt.MinEdgeSheared;
			MinED = Math.Max(Minedgedist, 2 * webPlate.Bolt.BoltSize);

			if (!beam.WinConnect.Beam.Lh_User && beam.WinConnect.Beam.Lh < MinED)
				beam.WinConnect.Beam.Lh = MinED;

			Nmx = Math.Min((double)(((int)Math.Floor((beam.WinConnect.Beam.WebHeight - 2 * webPlate.Bolt.EdgeDistLongDir) / webPlate.Bolt.SpacingLongDir)) + 1), 12);
			Nmn = ((int)Math.Floor(beam.WinConnect.Beam.WebHeight / 2 / webPlate.Bolt.SpacingLongDir)) + 1;
			if (!webPlate.BeamIsLaterallySupported && Nmn <= Nmx - 1)
				Nmn = (Nmn + 1);

			if (Nmn < 2)
				Nmn = 2;
			if (!webPlate.ExtendedConfiguration && CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToColumnWeb)
			{
				a = beam.EndSetback + beam.WinConnect.Beam.Lh;
				if (a > NumberFun.ConvertFromFraction(56))
				{
					if (beam.WinConnect.Beam.SkewOrIncline != EBeamSkewOrIncline.Skewed)
					{
						if (!beam.EndSetback_User)
							beam.EndSetback = ConstNum.HALF_INCH;
						if (!beam.WinConnect.Beam.Lh_User)
							beam.WinConnect.Beam.Lh = ConstNum.THREE_INCHES;
						a = NumberFun.ConvertFromFraction(56);
					}
				}

				webPlate.Width = beam.EndSetback + beam.WinConnect.Beam.Lh + webPlate.Bolt.EdgeDistTransvDir;
			}

			Nvl = 1;
			Nbw = (Nmn - 1);
			if (!webPlate.BoltNumberOfLines_User)
				webPlate.Bolt.NumberOfLines = 1;

			for (int i = 1; i <= 2; i++)
			{
				do
				{
					if (webPlate.ExtendedConfiguration && (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder))
					{
						Nbw++;
						if (!webPlate.Bolt.NumberOfRows_User)
							webPlate.Bolt.NumberOfRows = Nbw;
						if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
							eccent = (column.Shape.bf - column.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh + (webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir / 2;
						else
							eccent = (column.Shape.bf - column.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh + (webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir / 2;
						MiscCalculationsWithReporting.EccentricBolt(eccent, webPlate.Bolt.SpacingLongDir, webPlate.Bolt.SpacingTransvDir, Nvl, Nbw, ref c);

						ShearCap = c * webPlate.Bolt.BoltStrength;
						MiscCalculationsWithReporting.BlockShear(memberType, EShearOpt.SpliceWithoutMoment, ref BsCap, false);
						ShearCap = Math.Min(ShearCap, BsCap);
						if (ShearCap < V && Nbw == Nmx && Nvl < 4)
						{
							Nvl++;
							Nbw = (Nmn - 1);
						}
						if (!webPlate.BoltNumberOfLines_User)
							webPlate.Bolt.NumberOfLines = Nvl;

						if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
							webPlate.Width = (column.Shape.bf - column.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh + (Nvl - 1) * webPlate.Bolt.SpacingTransvDir + webPlate.Bolt.EdgeDistTransvDir;
					}
					else
					{
						Nbw++;
						shart1 = CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder && (beam.MemberType == EMemberType.RightBeam && !leftBeam.IsActive || beam.MemberType == EMemberType.LeftBeam && !rightBeam.IsActive);
						shart2 = CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && (beam.MemberType == EMemberType.RightBeam && !leftBeam.IsActive || beam.MemberType == EMemberType.LeftBeam && !rightBeam.IsActive);
						if (shart1 || shart2)
						{
							if (webPlate.Bolt.HoleType == EBoltHoleType.STD)
								eb = Math.Max(Math.Abs(ConstNum.ONE_INCH * (Nbw - 1) - a), a);
							else if (webPlate.Bolt.HoleType == EBoltHoleType.SSLN || webPlate.Bolt.HoleType == EBoltHoleType.SSLP)
								eb = Math.Max(Math.Abs(ConstNum.ONE_INCH * 2 * Nbw / 3 - a), a);
						}
						else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
							eb = beam.EndSetback + beam.WinConnect.Beam.Lh;
						else
						{
							if (webPlate.Bolt.HoleType == EBoltHoleType.STD)
								eb = Math.Abs(ConstNum.ONE_INCH * (Nbw - 1) - a);
							else if (webPlate.Bolt.HoleType == EBoltHoleType.SSLN || webPlate.Bolt.HoleType == EBoltHoleType.SSLP)
								eb = Math.Abs(ConstNum.ONE_INCH * 2 * Nbw / 3 - a);
						}
						if (!webPlate.Bolt.NumberOfRows_User)
							webPlate.Bolt.NumberOfRows = Nbw;
						if (webPlate.Bolt.NumberOfRows <= 9 || webPlate.Bolt.HoleType == EBoltHoleType.SSLN)
						{
							eb = 0;
							multiplier = 1;
						}
						else
						{
							eb = (webPlate.Bolt.NumberOfRows - 4) * ConstNum.ONE_INCH;
							multiplier = 1.25;
						}
						eccent = eb;

						MiscCalculationsWithReporting.EccentricBolt(eccent, webPlate.Bolt.SpacingLongDir, webPlate.Bolt.SpacingTransvDir, Nvl, Nbw, ref c);
						c *= multiplier;
						ShearCap = c * webPlate.Bolt.BoltStrength;
						MiscCalculationsWithReporting.BlockShear(memberType, EShearOpt.SpliceWithoutMoment, ref BsCap, false);
						ShearCap = Math.Min(ShearCap, BsCap);
					}
				} while (ShearCap < V && BsCap > 0 && Nbw < Nmx);

				//if (Nbw >= Nmx && ShearCap < V)
				//	Reporting.AddLine("Required number of bolts is more than maximum that can fit in beam web. (NG)");

				webPlate.Length = webPlate.Bolt.SpacingLongDir * (Nbw - 1) + 2 * webPlate.Bolt.EdgeDistLongDir;
				if (webPlate.Length > MaxL)
					webPlate.Length = MaxL;
				if (webPlate.ExtendedConfiguration)
				{
					if (webPlate.WebPlateStiffener == EWebPlateStiffener.Without)
						if (!webPlate.Height_User)
							webPlate.Height = webPlate.Length;
				}
				else if (!webPlate.Height_User)
					webPlate.Height = webPlate.Length;
				cc = beam.WinConnect.Beam.Lh + beam.EndSetback;
				ho = webPlate.Length;
				if (webPlate.ExtendedConfiguration)
					CommonDataStatic.MinThickness = MiscCalculationsWithReporting.LateralPlateBuckling((cc), (ho), webPlate.Material.Fy, true);
				else
					CommonDataStatic.MinThickness = 0;
				MaxThickness = Math.Max(webPlate.Bolt.BoltSize / 2 + ConstNum.SIXTEENTH_INCH, CommonDataStatic.MinThickness);

				SPln = webPlate.Length - Nbw * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
				spt1 = V / (ConstNum.FIOMEGA0_75N * 0.6 * webPlate.Material.Fu * SPln);
				spt2 = V / (ConstNum.FIOMEGA1_0N * 0.6 * webPlate.Material.Fy * webPlate.Length);
				Lnt = webPlate.Bolt.EdgeDistTransvDir - (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) / 2;
				Lnv = webPlate.Length - webPlate.Bolt.EdgeDistLongDir - (Nbw - 0.5) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
				Lgv = webPlate.Length - webPlate.Bolt.EdgeDistLongDir;

				Tblockshear = V / MiscCalculationsWithReporting.BlockShearNew(webPlate.Material.Fu, Lnv, 1, Lnt, Lgv, webPlate.Material.Fy, 1, 0, false);

				beam.WinConnect.Fema.L = webPlate.Length;
				d = webPlate.Bolt.HoleWidth;
				n = webPlate.Bolt.NumberOfRows;
				s = webPlate.Bolt.SpacingLongDir;
				e = webPlate.Bolt.EdgeDistLongDir;
				MomInert = (Math.Pow(beam.WinConnect.Fema.L, 3) - n * Math.Pow(d, 3)) / 12 - d * n * (Math.Pow(beam.WinConnect.Fema.L / 2 - e, 2) - (beam.WinConnect.Fema.L / 2 - e) * s * (n - 1) + Math.Pow(s, 2) * (n - 1) * (2 * n - 1) / 6);
				SecMod = 2 * MomInert / beam.WinConnect.Fema.L;
				if (webPlate.ExtendedConfiguration && (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder))
				{
					if (webPlate.WebPlateStiffener == EWebPlateStiffener.With)
					{
						ho = webPlate.Height - webPlate.TopOffset - +e;
						HeightAtTransition = Math.Min(webPlate.Height, Math.Min(beam.WinConnect.Fema.L, ho));
					}
					else
					{
						ho = beam.WinConnect.Fema.L;
						HeightAtTransition = beam.WinConnect.Fema.L;
					}
					spt2 = V / (ConstNum.FIOMEGA1_0N * 0.6 * webPlate.Material.Fy * HeightAtTransition);
					SecModOfExtension = Math.Pow(HeightAtTransition, 2) / 6;
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
					{
						ecForExtension = (column.Shape.bf - column.Shape.tw) / 2;
						ec = (column.Shape.bf - column.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh;
					}
					else
					{
						ecForExtension = (column.Shape.bf - column.Shape.tw) / 2;
						ec = (column.Shape.bf - column.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh;
					}
					spt3 = V * ec / (ConstNum.FIOMEGA0_9N * webPlate.Material.Fy * SecMod);
					spt3Extension = V * ecForExtension / (ConstNum.FIOMEGA0_9N * webPlate.Material.Fy * SecModOfExtension);
					spt3 = Math.Max(spt3, spt3Extension);
					if (spt3 > 2)
					{
						Reporting.AddLine("Plate thickness is unreasonable because plate depth at transition is too small. (NG)");
						if (!webPlate.Thickness_User)
							webPlate.Thickness = spt3;
						return;
					}
					if (webPlate.WebPlateStiffener == EWebPlateStiffener.Without)
					{
						hh = (webPlate.Bolt.NumberOfRows - 1) * webPlate.Bolt.SpacingLongDir;
						dc = Math.Max(beam.WinConnect.Beam.DistanceToFirstBolt, beam.Shape.d - beam.WinConnect.Beam.DistanceToFirstBolt - hh);
						if (!beam.WinConnect.DistanceToFirstBolt_User && dc > 0.2 * beam.Shape.d)
						{
							beam.WinConnect.Beam.DistanceToFirstBolt = ((int)Math.Floor(4 * (beam.Shape.d - hh) / 2)) / 4.0;
							dc = Math.Max(beam.WinConnect.Beam.DistanceToFirstBolt, beam.Shape.d - beam.WinConnect.Beam.DistanceToFirstBolt - hh);
						}
						fd = 3.5 - 7.5 * dc / beam.Shape.d;
						FiFbc = ConstNum.FIOMEGA0_9N * 0.62 * Math.PI * ConstNum.ELASTICITY * fd * Math.Pow(spt3, 2) / (ec * webPlate.Length);
						FiFbcl = ConstNum.FIOMEGA0_9N * webPlate.Material.Fy;
						if (FiFbc < FiFbcl)
							spt4 = spt3 * Math.Sqrt(FiFbcl / FiFbc);
						else
							spt4 = spt3;
					}
					else
						spt4 = spt3;
				}
				else
				{
					spt3 = V * eb / (ConstNum.FIOMEGA0_9N * webPlate.Material.Fy * SecMod);
					MomInert = Math.Pow(beam.WinConnect.Fema.L, 3) / 12;
					SecMod = 2 * MomInert / beam.WinConnect.Fema.L;
					spt4 = V * (a + eb) / (ConstNum.FIOMEGA0_9N * webPlate.Material.Fy * SecMod);
				}
				spt12 = Math.Max(spt1, spt2);
				spt34 = Math.Max(spt3, spt4);

				cc = beam.WinConnect.Beam.Lh + beam.EndSetback;
				ho = webPlate.Length;
				sptSlenderness = MiscCalculationsWithReporting.LateralPlateBuckling(((int)cc), ((int)ho), webPlate.Material.Fy, false);
				MaxThickness = Math.Max(webPlate.Bolt.BoltSize / 2 + ConstNum.SIXTEENTH_INCH, CommonDataStatic.MinThickness);
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
				{
					Maxt = column.Material.Fu * column.Shape.tf / webPlate.Material.Fy;
					MaxThickness = Math.Min(MaxThickness, Maxt);
				}

				t1234 = NumberFun.Round(Math.Max(sptSlenderness, Math.Max(Math.Max(spt12, spt34), Tblockshear)), 16);
				if (!webPlate.Thickness_User)
				{
					webPlate.Thickness = Math.Max(CommonDataStatic.MinThickness, t1234);
					if (webPlate.Thickness > MaxThickness)
						webPlate.Thickness = MaxThickness;
				}
				if (!webPlate.Thickness_User)
					webPlate.Thickness = Math.Max(CommonDataStatic.MinThickness, t1234);
				if (webPlate.Thickness > MaxThickness && !webPlate.ExtendedConfiguration)
					if (!webPlate.Thickness_User)
						webPlate.Thickness = MaxThickness;
				if (!webPlate.BoltNumberOfLines_User)
					webPlate.Bolt.NumberOfLines = Nvl;
				if (!webPlate.Bolt.NumberOfRows_User)
					webPlate.Bolt.NumberOfRows = Nbw;
				Fbe = CommonCalculations.EdgeBearing(webPlate.Bolt.EdgeDistLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Material.Fu, webPlate.Bolt.HoleType, false);
				Fbs = CommonCalculations.SpacingBearing(webPlate.Bolt.SpacingLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Bolt.HoleType, webPlate.Material.Fu, false);

				EquivBoltFactor = c / (webPlate.Bolt.NumberOfRows * webPlate.Bolt.NumberOfLines);
				if (EquivBoltFactor > 1)
					EquivBoltFactor = 1;
				BearingCap = EquivBoltFactor * (Fbe + Fbs * (webPlate.Bolt.NumberOfRows - 1)) * webPlate.Thickness;
				if (BearingCap < V)
					if (!webPlate.Thickness_User)
						webPlate.Thickness = NumberFun.Round(webPlate.Thickness * V / BearingCap, 16);
				if (webPlate.Thickness > MaxThickness && !webPlate.ExtendedConfiguration)
					if (!webPlate.Thickness_User)
						webPlate.Thickness = MaxThickness;
				wmin = CommonCalculations.MinimumWeld(webPlate.Thickness, supThickness1);
				if (webPlate.ExtendedConfiguration && (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder))
				{
					R = V;
					if (webPlate.WebPlateStiffener == EWebPlateStiffener.With)
						clip = 2 * 0.5;
					else
						clip = 0;
					w = R / (ConstNum.FIOMEGA0_75N * 0.8484 * webPlate.Weld.Fexx * (webPlate.Height - clip));
					if (!webPlate.SupportWeldSize_User)
						webPlate.SupportWeldSize = w;
					trequired = ConstNum.FIOMEGA0_75N * 0.8484 * w * webPlate.Weld.Fexx / webPlate.Material.Fu;
					if (webPlate.Thickness < trequired)
						if (!webPlate.Thickness_User)
							webPlate.Thickness = NumberFun.Round(trequired, 16);
				}
				else
				{
					ew = 0;
					c = EccentricWeld.GetEccentricWeld(0, ew, 0, false);
					//EccentricWeld.GetEccentricWeld(0, ew, ref c, 0, false);
					C1 = CommonCalculations.WeldTypeRatio();
					c = 2 * c * C1;
					w = (V / (c * webPlate.Length)) / 16;
					w1 = Math.Max(0.625 * webPlate.Thickness, w);
					if (!webPlate.SupportWeldSize_User)
						webPlate.SupportWeldSize = w1;
				}
				if (!webPlate.SupportWeldSize_User)
					webPlate.SupportWeldSize = Math.Max(webPlate.SupportWeldSize, wmin);
				webPlate.Bolt.NumberOfBolts = webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows;
				if (webPlate.Bolt.NumberOfRows + 1 > Nmx || webPlate.Thickness <= MaxThickness)
					break;
			}
			if (webPlate.ExtendedConfiguration && (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder))
			{
				webPlate.Width = (column.Shape.bf - column.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh + (webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir + webPlate.Bolt.EdgeDistTransvDir;
			}
			else
				webPlate.Width = beam.EndSetback + beam.WinConnect.Beam.Lh + webPlate.Bolt.EdgeDistTransvDir;

			if (beam.WinConnect.Beam.TopCope || beam.WinConnect.Beam.BottomCope)
				Cope.CopedBeam(beam.MemberType, false);

			Reporting.AddMainHeader(beam.ComponentName + " - " + beam.ShapeName + " Shear Connection");
			webPlate.Length = webPlate.Bolt.SpacingLongDir * (webPlate.Bolt.NumberOfRows - 1) + 2 * webPlate.Bolt.EdgeDistLongDir;
			if (webPlate.ExtendedConfiguration)
			{
				if (webPlate.WebPlateStiffener == EWebPlateStiffener.Without)
					if (!webPlate.Height_User)
						webPlate.Height = webPlate.Length;
			}
			else if (!webPlate.Height_User)
				webPlate.Height = webPlate.Length;

			Reporting.AddHeader("Shear Connection Using One Plate:");
			a = beam.EndSetback + beam.WinConnect.Beam.Lh;
			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
			{
				bovt = (column.Shape.bf - 3 * column.Shape.tf) / column.Shape.tf;
				Maximum = 1.4 * Math.Pow(ConstNum.ELASTICITY / column.Material.Fy, 0.5);
				if (bovt > Maximum)
				{
					Reporting.AddLine("HSS b / t = " + bovt + " >> 1.4 * (E / Fy)^0.5 = " + Maximum + ". Single Plate connection is not suitable. (NG)");
					return;
				}
				else
					Reporting.AddLine("HSS b / t = " + bovt + " <= 1.4 * (E / Fy)^0.5 = " + Maximum + " (OK)");

				Maxt = column.Material.Fu * column.Shape.tf / webPlate.Material.Fy;
				Reporting.AddHeader("Shear Yielding of HSS face:");
				if (webPlate.Thickness <= Maxt)
				{
					Reporting.AddLine("tp * Fyp <= Fu * t:");
					Reporting.AddLine((webPlate.Thickness * webPlate.Material.Fy) + " <=  " + (column.Material.Fu * column.Shape.tf) + " (OK)");
				}
				else
				{
					Reporting.AddLine("tp * Fyp <= Fu * t :");
					Reporting.AddLine((webPlate.Thickness * webPlate.Material.Fy) + " >> " + (column.Material.Fu * column.Shape.tf) + " (NG)");
				}
			}

			if (webPlate.ExtendedConfiguration)
			{
				if (webPlate.Length != webPlate.Height)
					Reporting.AddLine("Plate (W x L x T): " + webPlate.Length + " ~ " + webPlate.Height + ConstUnit.Length + " X " + webPlate.Width + ConstUnit.Length + " X " + webPlate.Thickness + ConstUnit.Length);
				else
					Reporting.AddLine("Plate (W x L x T): " + webPlate.Length + ConstUnit.Length + " X " + webPlate.Width + ConstUnit.Length + " X " + webPlate.Thickness + ConstUnit.Length);
				if (webPlate.WebPlateStiffener == EWebPlateStiffener.With)
					cc = beam.WinConnect.Beam.Lh + beam.EndSetback;
				else
				{
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
						cc = beam.WinConnect.Beam.Lh + beam.EndSetback + (column.Shape.bf - column.Shape.tw) / 2;
					else
						cc = beam.WinConnect.Beam.Lh + beam.EndSetback + (column.Shape.bf - column.Shape.tw) / 2;
				}
				ho = webPlate.Height;
			}
			else
			{
				Reporting.AddLine("Plate (W x L x T): " + webPlate.Length + ConstUnit.Length + " X " + webPlate.Width + ConstUnit.Length + " X " + webPlate.Thickness + ConstUnit.Length);
				cc = beam.WinConnect.Beam.Lh + beam.EndSetback;
				ho = webPlate.Length;
			}
			if (webPlate.ExtendedConfiguration)
			{
				CommonDataStatic.MinThickness = MiscCalculationsWithReporting.LateralPlateBuckling((cc), (ho), webPlate.Material.Fy, true);
				if (CommonDataStatic.MinThickness <= webPlate.Thickness)
					Reporting.AddLine("= " + CommonDataStatic.MinThickness + " <= " + webPlate.Thickness + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + CommonDataStatic.MinThickness + " >> " + webPlate.Thickness + ConstUnit.Length + " (NG)");
			}
			if (!webPlate.ExtendedConfiguration)
			{
				MaxThickness = webPlate.Bolt.BoltSize / 2 + ConstNum.SIXTEENTH_INCH;
				Reporting.AddLine("Max. Thickness = d / 2 + " + ConstNum.SIXTEENTH_INCH);
				if (MaxThickness >= Math.Min(webPlate.Thickness, beam.Shape.tw))
					Reporting.AddLine("= " + MaxThickness + " >= Min(" + webPlate.Thickness + ", " + beam.Shape.tw + ")" + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + MaxThickness + " << Min(" + webPlate.Thickness + ", " + beam.Shape.tw + ")" + " (NG)");
			}

			Reporting.AddLine("Plate Material: " + webPlate.Material.Name);

			if (webPlate.Length > beam.WinConnect.Beam.WebHeight)
				Reporting.AddLine("(Plate Length is greater than beam web clear space. (NG))");

			webPlate.Bolt.NumberOfBolts = webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows;
			Nbw = webPlate.Bolt.NumberOfRows;
			Nvl = webPlate.Bolt.NumberOfLines;
			if (webPlate.ExtendedConfiguration && CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
				Reporting.AddLine("Beam Setback from End of Col. Flange: " + beam.EndSetback + ConstUnit.Length);
			else
				Reporting.AddLine("Beam Setback: " + beam.EndSetback + ConstUnit.Length);

			Reporting.AddHeader("Bolts: (" + webPlate.Bolt.NumberOfBolts + ") " + webPlate.Bolt.BoltName);
			Reporting.AddLine("Bolt Holes on Beam Web: " + webPlate.Bolt.HoleWidthSupport + ConstUnit.Length + " Vert. X " + webPlate.Bolt.HoleLengthSupport + ConstUnit.Length + " Horiz.");
			Reporting.AddLine("Bolt Holes on Plate: " + webPlate.Bolt.HoleWidth + ConstUnit.Length + " Vert. X " + webPlate.Bolt.HoleLength + ConstUnit.Length + " Horiz.");
			Reporting.AddLine("Weld: " + CommonCalculations.WeldSize(webPlate.SupportWeldSize) + " " + ConstUnit.Length + " " + webPlate.WeldName + " Fillet Weld, both sides of PL.");

			R = Math.Sqrt(webPlate.h1 * webPlate.h1 + V * V);
			Reporting.AddHeader("Loading:");
			Reporting.AddLine("Vertical Shear (V) = " + V + ConstUnit.Force);
			Reporting.AddLine("Axial Load (H) = " + webPlate.h1 + ConstUnit.Force);
			Reporting.AddLine("Resultant (Ru) = (V² + H²)^0.5 = ((" + V + " )² + " + webPlate.h1 + "²)^0.5 = " + R + ConstUnit.Force);

			Reporting.AddHeader("Check Bolt Spacing and Edge Distance:");
			spBeam = MiscCalculationsWithReporting.MinimumSpacing((webPlate.Bolt.BoltSize), V / webPlate.Bolt.NumberOfBolts, beam.Material.Fu * beam.Shape.tw, webPlate.Bolt.HoleWidth, webPlate.Bolt.HoleType);
			spPlate = MiscCalculationsWithReporting.MinimumSpacing((webPlate.Bolt.BoltSize), V / webPlate.Bolt.NumberOfBolts, webPlate.Material.Fu * webPlate.Thickness, webPlate.Bolt.HoleWidth, webPlate.Bolt.HoleType);
			minsp = Math.Max(spBeam, spPlate);
			if (webPlate.Bolt.SpacingLongDir >= minsp)
				Reporting.AddLine("Spacing (s) = " + webPlate.Bolt.SpacingLongDir + " >= Minimum Spacing = " + minsp + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Spacing (s) = " + webPlate.Bolt.SpacingLongDir + " << Minimum Spacing = " + minsp + ConstUnit.Length + " (NG)");

			evminS = MiscCalculationsWithReporting.MinimumEdgeDist(webPlate.Bolt.BoltSize, V / webPlate.Bolt.NumberOfBolts, webPlate.Material.Fu * webPlate.Thickness, webPlate.Bolt.MinEdgeSheared, (int)webPlate.Bolt.HoleWidth, webPlate.Bolt.HoleType);
			evminR = MiscCalculationsWithReporting.MinimumEdgeDist(webPlate.Bolt.BoltSize, V / webPlate.Bolt.NumberOfBolts, webPlate.Material.Fu * webPlate.Thickness, webPlate.Bolt.MinEdgeRolled, (int)webPlate.Bolt.HoleWidth, webPlate.Bolt.HoleType);

			Reporting.AddLine(string.Empty);
			Reporting.AddHeader("Distance to Horiz. Edge of PL (ev):");
			if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC13)
			{
				Reporting.AddLine("(If Sheared Edge)");
				if (webPlate.Bolt.EdgeDistLongDir >= evminS)
					Reporting.AddLine("= " + webPlate.Bolt.EdgeDistLongDir + " >= " + evminS + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + webPlate.Bolt.EdgeDistLongDir + " << " + evminS + ConstUnit.Length + " (NG)");
			}

			if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC13)
				Reporting.AddLine("(If Rolled or Gas Cut Edge)");
			if (webPlate.Bolt.EdgeDistLongDir >= evminR)
				Reporting.AddLine("= " + webPlate.Bolt.EdgeDistLongDir + " >= " + evminR + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("= " + webPlate.Bolt.EdgeDistLongDir + " << " + evminR + ConstUnit.Length + " (NG)");

			if (webPlate.Bolt.HoleLength > webPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH)
				Minedgedist = webPlate.Bolt.MinEdgeSheared + webPlate.Bolt.Eincr;
			else
				Minedgedist = webPlate.Bolt.MinEdgeSheared;

			MinED = Math.Max(Minedgedist, 2 * webPlate.Bolt.BoltSize);
			Reporting.AddHeader("Minimum Distance to Vert. Edge of PL:");
			Reporting.AddLine("= Max(2 * db, " + Minedgedist + ")= " + MinED + ConstUnit.Length);
			Reporting.AddLine("Distance  to Vert. Edge of PL (eh):");
			if (webPlate.Bolt.EdgeDistTransvDir >= MinED)
				Reporting.AddLine("= " + webPlate.Bolt.EdgeDistTransvDir + " >= " + MinED + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("= " + webPlate.Bolt.EdgeDistTransvDir + " << " + MinED + ConstUnit.Length + " (NG)");

			if (webPlate.Bolt.HoleLength > webPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH)
				Minedgedist = webPlate.Bolt.MinEdgeSheared + webPlate.Bolt.Eincr;
			else
				Minedgedist = webPlate.Bolt.MinEdgeSheared;

			MinED = Math.Max(Minedgedist, 2 * webPlate.Bolt.BoltSize);
			Reporting.AddHeader("Minimum Distance to End of Beam:");
			Reporting.AddLine("= Max(2 * db, " + Minedgedist + ")= " + MinED + ConstUnit.Length);
			Reporting.AddLine("Distance to End of Beam (Lh):");
			if (beam.WinConnect.Beam.Lh >= MinED)
				Reporting.AddLine("= " + beam.WinConnect.Beam.Lh + " >= " + MinED + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("= " + beam.WinConnect.Beam.Lh + " << " + MinED + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Bolt Strength:");
			if (webPlate.ExtendedConfiguration && CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
			{
				eb = (column.Shape.bf - column.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh + (webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir / 2;
				Reporting.AddLine("Load Eccentricity for Bolts (eb) = " + eb + ConstUnit.Length);
			}
			else
			{
				if (a <= NumberFun.ConvertFromFraction(56))
					Reporting.AddLine("Distance from Weld Line to Bolt Line, a = " + a + " <= " + NumberFun.ConvertFromFraction(56) + ConstUnit.Length + " (OK)");
				else
				{
					Reporting.AddLine("Distance from Weld Line to Bolt Line, a = " + a + " >> " + NumberFun.ConvertFromFraction(56) + ConstUnit.Length + " (NG)");
					Reporting.AddLine("(Design procedure used is not valid for a > " + NumberFun.ConvertFromFraction(56) + ConstUnit.Length + " )");
				}
				Reporting.AddLine("Load Eccentricity for Bolts (eb):");

				if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC13)
				{
					if (webPlate.Bolt.NumberOfRows <= 9)
					{
						eb = 0;
						multiplier = 1;
						Reporting.AddLine("Bolt Rows = " + webPlate.Bolt.NumberOfRows + " <= 9 ... eb = 0");

					}
					else if (webPlate.Bolt.HoleType == EBoltHoleType.SSLN)
					{
						eb = 0;
						multiplier = 1;
						Reporting.AddLine("Bolt Hole = " + webPlate.Bolt.HoleType + " ... eb = 0");
					}
					else
					{
						eb = (webPlate.Bolt.NumberOfRows - 4) * ConstNum.ONE_INCH;
						multiplier = 1.25;
						Reporting.AddLine("eb = (N - 4) * " + ConstNum.ONE_INCH + " = " + (n - 4) * ConstNum.ONE_INCH + ConstUnit.Length + " (Multiply C by 1.25)");
					}
				}
				else // New in Descon 8
				{
					if ((webPlate.Bolt.NumberOfRows >= 2 && webPlate.Bolt.NumberOfRows <= 5) ||
					    (webPlate.Bolt.NumberOfRows >= 6 && webPlate.Bolt.NumberOfRows <= 12 && webPlate.Bolt.HoleType == EBoltHoleType.SSLN))
					{
						eb = a / 2;
						Reporting.AddLine("eb = a / 2 = " + a + " / 2 = " + eb);
					}
					else if (webPlate.Bolt.NumberOfRows >= 6 && webPlate.Bolt.NumberOfRows <= 12 && webPlate.Bolt.HoleType == EBoltHoleType.STD)
					{
						eb = a;
						Reporting.AddLine("eb = a = " + eb);
					}
				}
			}
			eccent = eb;
			MiscCalculationsWithReporting.EccentricBolt(eccent, webPlate.Bolt.SpacingLongDir, webPlate.Bolt.SpacingTransvDir, Nvl, Nbw, ref c);

			Reporting.AddHeader("Bolt Group Strength Coefficient for Eccentric Load:");
			if (Nvl == 1)
				Reporting.AddLine("Number of Bolts = " + Nbw + ", Spacing = " + webPlate.Bolt.SpacingLongDir + ConstUnit.Length + ",  C = " + c);
			else
			{
				Reporting.AddLine("Number of Bolt Rows = " + Nbw + ", Spacing = " + webPlate.Bolt.SpacingLongDir + ConstUnit.Length);
				Reporting.AddLine("Number of Vert. Bolt Lines = " + Nvl + ", Spacing = " + webPlate.Bolt.SpacingLongDir + ConstUnit.Length);
				Reporting.AddLine("C = " + c);
			}

			if (c < Nbw)
				webPlate.Bolt.disableLSLNWithBoltGroupEccentricity = true;
			else
				webPlate.Bolt.disableLSLNWithBoltGroupEccentricity = false;

			ShearCap = multiplier * c * webPlate.Bolt.BoltStrength;
			if (multiplier == 1.25)
			{
				if (ShearCap >= V)
					Reporting.AddCapacityLine("Bolt Group " + ConstString.DES_OR_ALLOWABLE_STRENGTH + "= 1.25 * C * Fv = 1.25 * " + c + " * " + webPlate.Bolt.BoltStrength + " = " + ShearCap + " >= " + V + ConstUnit.Force + " (OK)", V / ShearCap, "Shear Capacity", memberType);
				else
					Reporting.AddCapacityLine("Bolt Group " + ConstString.DES_OR_ALLOWABLE_STRENGTH + "= 1.25 * C * Fv = 1.25 * " + c + " * " + webPlate.Bolt.BoltStrength + " = " + ShearCap + " << " + V + ConstUnit.Force + " (NG)", V / ShearCap, "Shear Capacity", memberType);
			}
			else
			{
				if (ShearCap >= V)
					Reporting.AddCapacityLine("Bolt Group " + ConstString.DES_OR_ALLOWABLE_STRENGTH + "= C * Fv = " + c + " * " + webPlate.Bolt.BoltStrength + " = " + ShearCap + " >= " + V + ConstUnit.Force + " (OK)", V / ShearCap, "Shear Capacity", memberType);
				else
					Reporting.AddCapacityLine("Bolt Group " + ConstString.DES_OR_ALLOWABLE_STRENGTH + "= C * Fv = " + c + " * " + webPlate.Bolt.BoltStrength + " = " + ShearCap + " << " + V + ConstUnit.Force + " (NG)", V / ShearCap, "Shear Capacity", memberType);
			}

			MiscCalculationsWithReporting.BlockShear(memberType, EShearOpt.SpliceWithoutMoment, ref BsCap, true);
			if (beam.WinConnect.Beam.TopCope || beam.WinConnect.Beam.BottomCope)
				Cope.CopedBeam(beam.MemberType, true);

			beam.WinConnect.Fema.L = webPlate.Length;
			d = webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH;
			s = webPlate.Bolt.SpacingLongDir;

			Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Shear Strength of the Plate:");

			Reporting.AddHeader("Shear Rupture " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
			NetShear = ConstNum.FIOMEGA0_75N * 0.6 * webPlate.Material.Fu * (webPlate.Length - webPlate.Bolt.NumberOfRows * d) * webPlate.Thickness;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu * (L - n * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.6 * " + webPlate.Material.Fu + " * (" + webPlate.Length + " - " + webPlate.Bolt.NumberOfRows + " * " + d + ") * " + webPlate.Thickness);
			if (NetShear >= V)
				Reporting.AddCapacityLine("= " + NetShear + " >= " + V + ConstUnit.Force + " (OK)", V / NetShear, "Shear Rupture", memberType);
			else
				Reporting.AddCapacityLine("= " + NetShear + " << " + V + ConstUnit.Force + " (NG)", V / NetShear, "Shear Rupture", memberType);

			Reporting.AddHeader("Shear Yielding " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
			FiRn = ConstNum.FIOMEGA1_0N * 0.6 * webPlate.Material.Fy * webPlate.Length * webPlate.Thickness;

			Reporting.AddLine(ConstString.PHI + " Rn = (" + ConstString.FIOMEGA1_0 + " * 0.6 * Fy * L) * t");
			Reporting.AddLine("= (" + ConstString.FIOMEGA1_0 + " * 0.6 * " + webPlate.Material.Fy + " * " + webPlate.Length + ") * " + webPlate.Thickness);
			if (FiRn >= V)
				Reporting.AddCapacityLine("= " + FiRn + " >= " + V + ConstUnit.Force + " (OK)", V / FiRn, "Shear Yielding", memberType);
			else
				Reporting.AddCapacityLine("= " + FiRn + " << " + V + ConstUnit.Force + " (NG)", V / FiRn, "Shear Yielding", memberType);

			Reporting.AddHeader("Block Shear Rupture " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
			Ant = (webPlate.Bolt.EdgeDistTransvDir - (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) / 2) * webPlate.Thickness;
			Anv = (webPlate.Length - webPlate.Bolt.EdgeDistLongDir - (Nbw - 0.5) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * webPlate.Thickness;
			Agt = webPlate.Bolt.EdgeDistTransvDir * webPlate.Thickness;
			Agv = (webPlate.Length - webPlate.Bolt.EdgeDistLongDir) * webPlate.Thickness;
			Reporting.AddLine("Net Area with Tension Resistance (Ant)");
			Reporting.AddLine("= (Lh - (dh + " + ConstNum.SIXTEENTH_INCH + ") / 2) * t");
			Reporting.AddLine("= (" + webPlate.Bolt.EdgeDistTransvDir + " - (" + webPlate.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ") / 2) * " + webPlate.Thickness);
			Reporting.AddLine("= " + Ant + ConstUnit.Area);

			Reporting.AddLine(string.Empty);
			Reporting.AddLine("Net Area with Shear Resistance (Anv)");
			Reporting.AddLine("= (L - Lv - (N - 0.5) * (dv + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + webPlate.Length + " - " + webPlate.Bolt.EdgeDistLongDir + " - (" + webPlate.Bolt.NumberOfRows + " - 0.5) * (" + webPlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + webPlate.Thickness);
			Reporting.AddLine("= " + Anv + ConstUnit.Area);

			Reporting.AddLine(string.Empty);
			Reporting.AddLine("Gross Area with Tension Resistance (Agt)");
			Reporting.AddLine("= Lh * t = " + webPlate.Bolt.EdgeDistTransvDir + " * " + webPlate.Thickness + " = " + Agt + ConstUnit.Area);

			Reporting.AddLine(string.Empty);
			Reporting.AddLine("Gross Area with Shear Resistance (Agv)");
			Reporting.AddLine("= (L - Lv) * t = (" + webPlate.Length + " - " + webPlate.Bolt.EdgeDistLongDir + ") * " + webPlate.Thickness + " = " + Agv + ConstUnit.Area);

			FiRn = MiscCalculationsWithReporting.BlockShearNew(webPlate.Material.Fu, Anv, 1, Ant, Agv, webPlate.Material.Fy, 0, V, true);

			Reporting.AddHeader("Bolt Bearing on Plate:");
			Fbe = CommonCalculations.EdgeBearing(webPlate.Bolt.EdgeDistLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Material.Fu, webPlate.Bolt.HoleType, true);
			Fbs = CommonCalculations.SpacingBearing(webPlate.Bolt.SpacingLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Bolt.HoleType, webPlate.Material.Fu, true);

			EquivBoltFactor = c / (webPlate.Bolt.NumberOfRows * webPlate.Bolt.NumberOfLines);
			if (EquivBoltFactor > 1)
				EquivBoltFactor = 1;

			BearingCap = EquivBoltFactor * webPlate.Bolt.NumberOfLines * (Fbe + Fbs * (webPlate.Bolt.NumberOfRows - 1)) * webPlate.Thickness;
			if (Nvl == 1)
			{
				Reporting.AddHeader("Bearing " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
				Reporting.AddLine(" = (Fbe + Fbs * (n - 1)) * t * ef");
				Reporting.AddLine("= (" + Fbe + " + " + Fbs + " * (" + webPlate.Bolt.NumberOfRows + " - 1)) * " + webPlate.Thickness + " * " + EquivBoltFactor);
			}
			else
			{
				Reporting.AddHeader("Bearing " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
				Reporting.AddLine(" = (Fbe + Fbs * (n - 1)) * t * ef");
				Reporting.AddLine("= (" + Fbe + " + " + Fbs + " * (" + webPlate.Bolt.NumberOfRows + " - 1)) * " + Nvl + " * " + webPlate.Thickness + " * " + EquivBoltFactor);
			}

			if (BearingCap >= V)
				Reporting.AddCapacityLine("= " + BearingCap + " >= " + V + ConstUnit.Force + " (OK)", V / BearingCap, "Bearing Capacity", memberType);
			else
				Reporting.AddCapacityLine("= " + BearingCap + " << " + V + ConstUnit.Force + " (NG)", V / BearingCap, "Bearing Capacity", memberType);

			Reporting.AddHeader("Bolt Bearing on Beam Web:");
			Fbs = CommonCalculations.SpacingBearing(webPlate.Bolt.SpacingLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Bolt.HoleType, beam.Material.Fu, true);
			if (beam.WinConnect.Beam.TopCope)
			{
				Fbe = CommonCalculations.EdgeBearing(beam.WinConnect.Beam.DistanceToFirstBolt, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, beam.Material.Fu, webPlate.Bolt.HoleType, true);

				EquivBoltFactor = c / webPlate.Bolt.NumberOfRows;
				if (EquivBoltFactor > 1)
					EquivBoltFactor = 1;

				BearingCap = EquivBoltFactor * (Fbe + Fbs * (webPlate.Bolt.NumberOfRows - 1)) * beam.Shape.tw;
				Reporting.AddHeader("Bearing " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
				Reporting.AddLine(" = (Fbe + Fbs * (n - 1)) * t * ef");
				Reporting.AddLine("= (" + Fbe + " + " + Fbs + " * (" + webPlate.Bolt.NumberOfBolts + " - 1)) * " + beam.Shape.tw + " * " + EquivBoltFactor);
			}
			else
			{
				EquivBoltFactor = c / (webPlate.Bolt.NumberOfRows * webPlate.Bolt.NumberOfLines);
				if (EquivBoltFactor > 1)
					EquivBoltFactor = 1;

				BearingCap = EquivBoltFactor * Fbs * webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows * beam.Shape.tw;
				if (Nvl == 1)
				{
					Reporting.AddHeader("Bearing " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
					Reporting.AddLine(" = Fbs * n * t * ef");
					Reporting.AddLine("= " + Fbs + " * " + webPlate.Bolt.NumberOfRows + " * " + beam.Shape.tw + " * " + EquivBoltFactor);
				}
				else
				{
					Reporting.AddHeader("Bearing " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
					Reporting.AddLine(" = Fbs * n1 * n * t * ef");
					Reporting.AddLine("= " + Fbs + " * " + Nvl + " * " + webPlate.Bolt.NumberOfRows + " * " + beam.Shape.tw + " * " + EquivBoltFactor);
				}
			}
			if (BearingCap >= V)
				Reporting.AddCapacityLine("= " + BearingCap + " >= " + V + ConstUnit.Force + " (OK)", V / BearingCap, "Bearing Capacity", memberType);
			else
				Reporting.AddCapacityLine("= " + BearingCap + " << " + V + ConstUnit.Force + " (NG)", V / BearingCap, "Bearing Capacity", memberType);

			Reporting.AddHeader("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
			minweld = CommonCalculations.MinimumWeld(supThickness1, webPlate.Thickness);
			if (minweld <= webPlate.SupportWeldSize)
				Reporting.AddLine("Minimum Weld Size = " + minweld + " <= " + webPlate.SupportWeldSize + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Minimum Weld Size = " + minweld + " >> " + webPlate.SupportWeldSize + ConstUnit.Length + " (NG)");

			if (webPlate.ExtendedConfiguration && CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
			{
				w_useful = column.Material.Fu * supThickness1 / (0.707 * webPlate.Weld.Fexx);
				Reporting.AddHeader("Maximum useful weld size for support thickness:");
				Reporting.AddLine("= Fu * t_eff / (0.707" + " * Fexx) ");
				Reporting.AddLine("= " + column.Material.Fu + " * " + supThickness1 + " / (0.707" + " * " + webPlate.Weld.Fexx + ")");
				if (w_useful < webPlate.SupportWeldSize)
				{
					Reporting.AddLine("= " + w_useful + " << " + webPlate.SupportWeldSize + ConstUnit.Length);
					Reporting.AddLine("(use " + w_useful + ConstUnit.Length + " for capacity calculation.)");
				}
				else
					Reporting.AddLine("= " + w_useful + " >= " + webPlate.SupportWeldSize + ConstUnit.Length + " (OK)");

				WeldCapacity = ConstNum.FIOMEGA0_75N * 0.8484 * webPlate.Weld.Fexx * Math.Min(w_useful, webPlate.SupportWeldSize) * (webPlate.Height - ConstNum.ONE_INCH);
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.8484 * Fexx * L * w");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.8484 * " + webPlate.Weld.Fexx + " * " + (webPlate.Height - ConstNum.ONE_INCH) + " * " + webPlate.SupportWeldSize);
				if (WeldCapacity >= V)
					Reporting.AddCapacityLine("= " + WeldCapacity + " >= " + V + ConstUnit.Force + " (OK)", V / WeldCapacity, "Weld Capacity", memberType);
				else
					Reporting.AddCapacityLine("= " + WeldCapacity + " << " + V + ConstUnit.Force + " (NG)", V / WeldCapacity, "Weld Capacity", memberType);
			}
			else
			{
				w_useful = column.Material.Fu * supThickness1 / (0.707 * webPlate.Weld.Fexx);
				WeldCapacity = c * webPlate.Length * Math.Min(w_useful, webPlate.SupportWeldSize);
				Reporting.AddHeader("Maximum useful weld size for support thickness:");
				Reporting.AddLine("= Fu * t_eff / (0.707 * Fexx)");
				Reporting.AddLine("= " + column.Material.Fu + " * " + supThickness1 + " / (0.707" + " * " + webPlate.Weld.Fexx + ")");
				if (w_useful < webPlate.SupportWeldSize)
				{
					Reporting.AddLine("= " + w_useful + " << " + webPlate.SupportWeldSize + ConstUnit.Length);
					Reporting.AddLine("(use " + w_useful + ConstUnit.Length + " for capacity calculation.)");
				}
				else
					Reporting.AddLine("= " + w_useful + " >= " + webPlate.SupportWeldSize + ConstUnit.Length + " (OK)");

				WeldCapacity = 2 * ConstNum.FIOMEGA0_75N * 0.707 * 0.6 * webPlate.Weld.Fexx * beam.WinConnect.Fema.L * Math.Min(w_useful, webPlate.SupportWeldSize);
				Reporting.AddLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " = 2 * " + ConstString.FIOMEGA0_75 + " * 0.707 * 0.6 * Fexx * L * w");
				Reporting.AddLine("= 2 * " + ConstString.FIOMEGA0_75 + " * 0.707 * 0.6 * " + webPlate.Weld.Fexx + " * " + beam.WinConnect.Fema.L + " * " + Math.Min(w_useful, webPlate.SupportWeldSize));

				if (WeldCapacity >= V)
					Reporting.AddCapacityLine("= " + WeldCapacity + " >= " + V + ConstUnit.Force + " (OK)", V / WeldCapacity, "Weld Capacity", memberType);
				else
					Reporting.AddCapacityLine("= " + WeldCapacity + " << " + V + ConstUnit.Force + " (NG)", V / WeldCapacity, "Weld Capacity", memberType);
			}
		}
	}
}
