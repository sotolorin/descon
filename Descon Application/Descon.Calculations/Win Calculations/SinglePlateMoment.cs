using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class SinglePlateMoment
	{
		internal static void DesignSinglePlateMoment(EMemberType memberType, double Taxial, ref double V, ref double MaxL, ref double MinL)
		{
			double Cap = 0;
			double w_useful;
			double w_usefulP;
			double w_usefulS;
			double Ru;
			double ecn;
			double ecg;
			double FiRn;
			double Anv;
			double Agv;
			double Ant;
			double Agt;
			double VnCap;
			double An;
			double FiVn;
			double Aw;
			double BeamBlockShearCap;
			double ehmin;
			double evmin;
			double minsp;
			double spPlate;
			double spBeam;
			double R;
			double BearingCap;
			double MinimumThickness;
			double BsCap = 0;
			double BearingCapacity;
			double VnTemp;
			double wp;
			int IterCount = 0;
			double minweld;
			double PlThickness;
			double EccTotal;
			double C1;
			double ew;
			double weldl;
			double suptw = 0;
			double Dumyth1;
			double Maxt;
			double tMax;
			double Mmax;
			double Ab;
			double Fvn = 0;
			double C_Prime = 0;
			bool Shart4;
			bool Shart3;
			bool shart2;
			bool shart1;
			double FiMn = 0;
			double fshear;
			double eccforbending = 0;
			double M_Req = 0;
			double tReqBearingCap;
			double EquivBoltFactor;
			double Fbs;
			double Fbe;
			double FiFcr;
			double Fcr;
			double Q;
			double Lambda;
			double spt6;
			double h0;
			double cp = 0;
			double tminimum;
			double spt4;
			double spt3;
			double EccPlateN = 0;
			double EccPlateG = 0;
			double Znet;
			double Zgross;
			double Sgross;
			double SnetforBuckling;
			double SnetDeduction;
			double n1;
			double nn;
			double s;
			double n;
			double d;
			double sptBlockShear;
			double Ubs;
			double Lnv;
			double Lgv;
			double Lnt;
			double Lgt;
			double spt2;
			double spt1;
			double SPln;
			double c = 0;
			double eccentricity = 0;
			double BeamTearOutCap = 0;
			int boltNumberOfRows;
			int boltNumberOfLines;
			double ShearCap;
			int Nmn;
			double boltNumberOfRowsMax;
			double SupFu = 0;
			double SupThickness1 = 0;
			double eat;
			double SupThickness = 0;
			double LRweldstress = 0;
			double t_web;
			double AccessHoleHight;
			double Mfws;
			double spt;
			double w;
			double w_req;
			double Mn = 0;
			double Mc = 0;
			double Vc = 0;
			double Mr = 0;
			double Vn = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var primaryMember = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			DetailData oppositeBeam;
			if (memberType == EMemberType.RightBeam)
				oppositeBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			else
				oppositeBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			var webPlate = beam.WinConnect.ShearWebPlate;

			if (CommonDataStatic.IsFema)
			{
				switch (beam.WinConnect.Fema.Connection)
				{
					case EFemaConnectionType.WUFW:
					case EFemaConnectionType.FF:
					case EFemaConnectionType.WFP:
						return;
					default:
						V = beam.WinConnect.Fema.Vf;
						break;
				}
			}

			if (!webPlate.Bolt.EdgeDistLongDir_User)
				webPlate.Bolt.EdgeDistLongDir = webPlate.Bolt.MinEdgeSheared;

			if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic && CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.RBS && CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.IMF)
			{
				AccessHoleHight = 0.375 + Math.Max(0.75, 0.75 * beam.Shape.tf);
				MaxL = beam.Shape.d - 2 * (beam.Shape.tf + AccessHoleHight);
				MinL = MaxL;
				if (!beam.WinConnect.DistanceToFirstBolt_User)
					beam.WinConnect.Beam.DistanceToFirstBolt = NumberFun.Round((beam.Shape.d - MaxL) / 2 + webPlate.Bolt.EdgeDistLongDir, 4);
				MaxL = beam.Shape.d - 2 * beam.WinConnect.Beam.DistanceToFirstBolt;
			}
			else if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic && CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.RBS && CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.IMF)
				return;

			if (beam.Moment != 0 && CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
			{
				webPlate.ExtendedConfiguration = true;
				webPlate.WebPlateStiffener = EWebPlateStiffener.With;
			}
			if (beam.Moment != 0 && CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
			{
				webPlate.ExtendedConfiguration = false;
				webPlate.WebPlateStiffener = EWebPlateStiffener.With;
				if (!beam.EndSetback_User && beam.EndSetback > 0.5)
					beam.EndSetback = 0.5;
			}
			
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder && webPlate.ExtendedConfiguration)
			{
				if (!beam.EndSetback_User)
					beam.EndSetback += beam.WinConnect.Beam.TCopeL;
			}

			if (beam.Moment != 0 && (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn || CommonDataStatic.IsFema))
			{
				webPlate.ExtendedConfiguration = true;
				webPlate.WebPlateStiffener = EWebPlateStiffener.Without;
			}

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
			{
				t_web = primaryMember.Shape.tw;

				if (beam.ShearForce > 0 && oppositeBeam.ShearForce > 0)
				{
					if (webPlate.Height > 0 && oppositeBeam.WinConnect.ShearWebPlate.Height > 0)
					{
						LRweldstress = oppositeBeam.ShearForce / oppositeBeam.WinConnect.ShearWebPlate.Height + beam.ShearForce / webPlate.Height;
						SupThickness = t_web / LRweldstress * beam.ShearForce / webPlate.Height;
					}
					else
						SupThickness = t_web;
				}
				else if (beam.ShearForce > 0 || oppositeBeam.ShearForce > 0)
					SupThickness = t_web;
			}

			eat = MiscCalculationsWithReporting.MinimumEdgeDist(webPlate.Bolt.BoltSize, Taxial, webPlate.Material.Fu * webPlate.Thickness, webPlate.Bolt.MinEdgeSheared, (int) webPlate.Bolt.HoleLength, webPlate.Bolt.HoleType);
			if (eat > webPlate.Bolt.EdgeDistTransvDir)
				webPlate.Bolt.EdgeDistTransvDir = MiscCalculationsWithReporting.MinimumEdgeDist(webPlate.Bolt.BoltSize, Taxial, webPlate.Material.Fu * webPlate.Thickness, webPlate.Bolt.MinEdgeSheared, (int) webPlate.Bolt.HoleLength, webPlate.Bolt.HoleType);

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder && webPlate.ExtendedConfiguration && webPlate.WebPlateStiffener == EWebPlateStiffener.With)
			{
                if (!webPlate.TopOffset_User)
                    webPlate.TopOffset = primaryMember.WinConnect.Beam.TopElValue - beam.WinConnect.Beam.TopElValue - primaryMember.Shape.tf;
                if (!webPlate.Height_User) 
                    webPlate.Height = primaryMember.Shape.d - 2 * primaryMember.Shape.tf;
			}

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn || CommonDataStatic.IsFema)
			{
				SupThickness = primaryMember.Shape.tf;
				SupThickness1 = primaryMember.Shape.tf;
				SupFu = primaryMember.Material.Fu;
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
			{
				if (webPlate.h2 == 0)
					webPlate.h2 = 1;
				if (oppositeBeam.WinConnect.ShearWebPlate.h2 == 0)
					oppositeBeam.WinConnect.ShearWebPlate.h2 = 1;
				if (oppositeBeam.WinConnect.ShearWebPlate.Length == 0)
					oppositeBeam.WinConnect.ShearWebPlate.Length = 1;
				if (!oppositeBeam.WinConnect.ShearWebPlate.ExtendedConfiguration)
					oppositeBeam.WinConnect.ShearWebPlate.h2 = oppositeBeam.WinConnect.ShearWebPlate.Length;

				SupThickness1 = primaryMember.Shape.tw;
				webPlate.h1 = webPlate.Length;

				if (beam.MomentConnection == EMomentCarriedBy.FlangePlate)
					webPlate.h2 = beam.Shape.d;
				else
					webPlate.h2 = beam.Shape.d - beam.Shape.tf - (beam.WinConnect.MomentDirectWeld.Top.ExtensionThickness + beam.Shape.tf) / 2;

				SupThickness = primaryMember.Shape.tw * Math.Abs(beam.ShearForce / webPlate.h2) / (Math.Abs(beam.ShearForce / webPlate.h2) + Math.Abs(oppositeBeam.ShearForce / oppositeBeam.WinConnect.ShearWebPlate.h2));
				SupFu = primaryMember.Material.Fu;
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
			{
				if (beam.MemberType == EMemberType.LeftBeam)
				{
					SupThickness = primaryMember.Shape.tw * Math.Abs(beam.ShearForce) / (Math.Abs(beam.ShearForce) + Math.Abs(oppositeBeam.ShearForce));
					SupThickness1 = primaryMember.Shape.tw;
				}
				else
				{
					SupThickness = primaryMember.Shape.tw * Math.Abs(beam.ShearForce) / (Math.Abs(oppositeBeam.ShearForce) + Math.Abs(beam.ShearForce));
					SupThickness1 = primaryMember.Shape.tw;
				}
				SupFu = primaryMember.Material.Fu;
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && beam.Moment == 0)
			{
				if (webPlate.ExtendedConfiguration && webPlate.WebPlateStiffener == EWebPlateStiffener.With && webPlate.Height < 5)
                    if (!webPlate.Height_User) 
                        webPlate.Height = beam.Shape.d;

				if (oppositeBeam.WinConnect.ShearWebPlate.Length == 0)
					oppositeBeam.WinConnect.ShearWebPlate.Length = 1;
				if (!oppositeBeam.WinConnect.ShearWebPlate.ExtendedConfiguration)
					oppositeBeam.WinConnect.ShearWebPlate.h2 = oppositeBeam.WinConnect.ShearWebPlate.Length;
				
				SupThickness1 = primaryMember.Shape.tw;
				webPlate.h1 = webPlate.Length;
				webPlate.h2 = webPlate.Height;

				SupFu = primaryMember.Material.Fu;
			}
			else
				SupThickness = 0;

			if(CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
			{
				webPlate.Clip = 1;
				if (beam.Moment == 0 && webPlate.WebPlateStiffener == EWebPlateStiffener.Without)
					webPlate.Clip = 0;
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
			{
				if (webPlate.WebPlateStiffener == EWebPlateStiffener.With)
					webPlate.Clip = 2 * (primaryMember.Shape.kdes - primaryMember.Shape.tf);
				else
					webPlate.Clip = 0;
			}
			else
				webPlate.Clip = 0;

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
			{
				if (beam.Moment > 0)
				{
					if (beam.MomentConnection == EMomentCarriedBy.FlangePlate)
					{
						webPlate.h1 = beam.Shape.t;
						webPlate.h2 = beam.Shape.d;
					}
					else
					{
						webPlate.h1 = beam.Shape.t;
						webPlate.h2 = beam.Shape.d - beam.Shape.tf - (beam.WinConnect.MomentDirectWeld.Bottom.ExtensionThickness + beam.Shape.tf) / 2;
					}

					webPlate.Length = webPlate.h1;
				}

				boltNumberOfRowsMax = ((int)Math.Floor((beam.WinConnect.Beam.WebHeight - 2 * webPlate.Bolt.EdgeDistLongDir) / webPlate.Bolt.SpacingLongDir)) + 1;
				Nmn = ((int)Math.Floor(beam.WinConnect.Beam.WebHeight / 2 / webPlate.Bolt.SpacingLongDir)) + 1;
				ShearCap = 0;
				boltNumberOfLines = 0;
				boltNumberOfRows = Nmn;

				while (ShearCap < V || beam.Moment == 0 && BeamTearOutCap < Taxial || boltNumberOfLines == 0)
				{
					if (boltNumberOfRows == boltNumberOfRowsMax || boltNumberOfLines == 0)
					{
						boltNumberOfRows = (Nmn - 1);
						boltNumberOfLines++;
					}
					if (!webPlate.BoltNumberOfLines_User)
						webPlate.Bolt.NumberOfLines = boltNumberOfLines;
					
					boltNumberOfRows++;

					if (!webPlate.Bolt.NumberOfRows_User)
						webPlate.Bolt.NumberOfRows = boltNumberOfRows;

					webPlate.Width = (primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh + boltNumberOfLines * webPlate.Bolt.SpacingTransvDir + webPlate.Bolt.EdgeDistTransvDir;

					CalculateEccentricity(memberType, ref boltNumberOfLines, ref eccentricity, ref eccforbending);

					MiscCalculationsWithReporting.EccentricBolt(eccentricity, webPlate.Bolt.SpacingLongDir, webPlate.Bolt.SpacingTransvDir, boltNumberOfLines, boltNumberOfRows, ref c);
					ShearCap = c * webPlate.Bolt.BoltStrength;
					if (beam.Moment == 0 && (beam.WinConnect.Beam.TopCope & beam.WinConnect.Beam.BottomCope) && beam.P != 0)
						BeamTearOutCap = MiscCalculationsWithReporting.BeamWebTearout(beam.MemberType, webPlate.Bolt.NumberOfRows, boltNumberOfLines, webPlate.Bolt.SpacingLongDir, webPlate.Bolt.SpacingTransvDir, EBeamOrAttachment.Beam, ETearOutBlockShear.TearOut, true);
				}

				webPlate.h1 = 2 * webPlate.Bolt.EdgeDistLongDir + (webPlate.Bolt.NumberOfRows - 1) * webPlate.Bolt.SpacingLongDir;
				webPlate.Length = webPlate.h1;
				SPln = webPlate.Length - boltNumberOfRows * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
				spt1 = V / (ConstNum.FIOMEGA0_75N * 0.6 * webPlate.Material.Fu * SPln);
				spt2 = V / (ConstNum.FIOMEGA1_0N * 0.6 * webPlate.Material.Fy * webPlate.Length);

				Lgt = webPlate.Bolt.EdgeDistTransvDir + (webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir;
				Lnt = Lgt - (webPlate.Bolt.NumberOfLines - 0.5) * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
				Lgv = webPlate.Length - webPlate.Bolt.EdgeDistLongDir;
				Lnv = Lgv - (webPlate.Bolt.NumberOfRows - 0.5) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
				Ubs = webPlate.Bolt.NumberOfLines > 1 ? 0.5 : 1;
				sptBlockShear = V / MiscCalculationsWithReporting.BlockShearNew(webPlate.Material.Fu, Lnv, Ubs, Lnt, Lgv, webPlate.Material.Fy, 1, 0, false);

				beam.WinConnect.Fema.L = webPlate.Length;
				d = webPlate.Bolt.HoleWidth;
				n = boltNumberOfRows;
				s = webPlate.Bolt.SpacingLongDir;
				nn = n / 2;
				n1 = (n + 1);
				if ((Math.Floor(n / 2.0)) * 2.0 == n)
					SnetDeduction = (4 * Math.Pow(nn, 3) / 3 + 2 * nn / 3 - nn) * Math.Pow(s, 2) * (d + ConstNum.SIXTEENTH_INCH) * webPlate.Thickness / beam.WinConnect.Fema.L;
				else
					SnetDeduction = (Math.Pow(n1, 3) / 2 - 3 * Math.Pow(n1, 2) / 2 + n1) * Math.Pow(s, 2) * (d + ConstNum.SIXTEENTH_INCH) * webPlate.Thickness / (3 * beam.WinConnect.Fema.L);
				
				SnetforBuckling = webPlate.Thickness * Math.Pow(beam.WinConnect.Fema.L, 2) / 6 - SnetDeduction;
				Sgross = Math.Pow(beam.WinConnect.Fema.L, 2) / 6;
				Zgross = Math.Pow(beam.WinConnect.Fema.L, 2) / 4;
				if (n == (Math.Floor(n / 2.0)) * 2.0)
					Znet = Zgross - 2 * (Math.Pow(n, 2) / 8) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * webPlate.Bolt.SpacingLongDir;
				else
					Znet = Zgross - 2 * ((Math.Pow(n, 2) - 1) / 8) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * webPlate.Bolt.SpacingLongDir;

				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
				{
					if (webPlate.ExtendedConfiguration && beam.Moment == 0)
					{
						EccPlateG = (primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh;
						EccPlateN = (primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh;
					}
					else
					{
						EccPlateG = beam.EndSetback + beam.WinConnect.Beam.Lh;
						EccPlateN = beam.EndSetback + beam.WinConnect.Beam.Lh;
					}
				}
				spt3 = V * EccPlateN / (webPlate.NumberOfPlates * ConstNum.FIOMEGA0_75N * webPlate.Material.Fu * Znet);
				spt4 = V * EccPlateG / (webPlate.NumberOfPlates * ConstNum.FIOMEGA0_9N * webPlate.Material.Fy * Sgross);
				tminimum = webPlate.Length * Math.Sqrt(webPlate.Material.Fy) / (2.45 * Math.Pow(ConstNum.ELASTICITY, 0.5));
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && beam.Moment == 0)
					tminimum = Math.Max(ConstNum.QUARTER_INCH, webPlate.Length * Math.Pow(webPlate.Material.Fy, 0.5) / (234 * MiscCalculationsWithReporting.K_Factor((int)((primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh), webPlate.Length)));
				spt = Math.Max(Math.Max(Math.Max(spt1, spt2), Math.Max(sptBlockShear, tminimum)), Math.Max(spt3, spt4));

				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && beam.Moment == 0)
					cp = webPlate.Width - webPlate.Bolt.EdgeDistTransvDir - webPlate.Bolt.SpacingTransvDir * (webPlate.Bolt.NumberOfLines - 1);

				else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && beam.Moment > 0)
					cp = webPlate.Width - (primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 - webPlate.Bolt.EdgeDistTransvDir - webPlate.Bolt.SpacingTransvDir * (webPlate.Bolt.NumberOfLines - 1);

				h0 = webPlate.Length;
				spt6 = spt - ConstNum.SIXTEENTH_INCH;
				do
				{
					spt6 += ConstNum.SIXTEENTH_INCH;
					Lambda = h0 * Math.Pow(beam.Material.Fy, 0.5) / (10 * spt6 * Math.Pow(475 + 280 * Math.Pow(h0 / cp, 2), 0.5));
					if (Lambda <= 0.7)
						Q = 1;
					else if (Lambda <= 1.41)
						Q = 1.34 - 0.486 * Lambda;
					else
						Q = 1.3 / Math.Pow(Lambda, 2);

					Fcr = webPlate.Material.Fy * Q;
					FiFcr = ConstNum.FIOMEGA0_9N * Fcr;
					beam.WinConnect.Fema.L = webPlate.Length;
					d = webPlate.Bolt.HoleWidth;
					n = boltNumberOfRows;
					s = webPlate.Bolt.SpacingLongDir;
					nn = n / 2;
					n1 = (n + 1);
					if (((int) Math.Floor(boltNumberOfRows / 2.0)) * 2 == boltNumberOfRows)
						SnetDeduction = (4 * Math.Pow(nn, 3) / 3 + 2 * nn / 3 - nn) * Math.Pow(s, 2) * (d + ConstNum.SIXTEENTH_INCH) * spt6 / beam.WinConnect.Fema.L;
					else
						SnetDeduction = (Math.Pow(n1, 3) / 2 - 3 * Math.Pow(n1, 2) / 2 + n1) * Math.Pow(s, 2) * (d + ConstNum.SIXTEENTH_INCH) * spt6 / (3 * beam.WinConnect.Fema.L);
					SnetforBuckling = spt6 * Math.Pow(beam.WinConnect.Fema.L, 2) / 6 - SnetDeduction;
				} while (V > FiFcr * SnetforBuckling / Math.Abs(cp));
				spt = Math.Max(spt, spt6);
				if (!webPlate.Thickness_User)
					webPlate.Thickness = CommonCalculations.PlateThickness(Math.Max(spt, spt3));

                if (!webPlate.BoltNumberOfLines_User) 
                    webPlate.Bolt.NumberOfLines = boltNumberOfLines;
                if (!webPlate.Bolt.NumberOfRows_User) 
                    webPlate.Bolt.NumberOfRows = boltNumberOfRows;

				Fbe = CommonCalculations.EdgeBearing(webPlate.Bolt.EdgeDistLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Material.Fu, webPlate.Bolt.HoleType, false);
				Fbs = CommonCalculations.SpacingBearing(webPlate.Bolt.SpacingLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Bolt.HoleType, webPlate.Material.Fu, false);
				EquivBoltFactor = c / (webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows);
				if (EquivBoltFactor > 1)
					EquivBoltFactor = 1;
				tReqBearingCap = V / (EquivBoltFactor * webPlate.Bolt.NumberOfLines * (Fbe + Fbs * (webPlate.Bolt.NumberOfRows - 1)));
				if (spt < tReqBearingCap)
					spt = tReqBearingCap;
				if (!webPlate.Thickness_User)
					webPlate.Thickness = CommonCalculations.PlateThickness(NumberFun.Round(spt, ERoundingPrecision.Sixteenth, ERoundingStyle.RoundUp));

				if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC13)
				{
					M_Req = V * eccforbending;
					fshear = V / (webPlate.Thickness * webPlate.Length);
					Fcr = Math.Pow(Math.Pow(webPlate.Material.Fy, 2) - 3 * Math.Pow(fshear, 2), 0.5);
					FiMn = Fcr * Zgross * webPlate.Thickness;
					if (FiMn < M_Req)
					{
						spt = spt * M_Req / FiMn;
						if (!webPlate.Thickness_User)
							webPlate.Thickness = CommonCalculations.PlateThickness(NumberFun.Round(spt, 16));
					}
				}
				else
				{
					Mn = webPlate.Material.Fy * webPlate.Thickness * (Math.Pow(webPlate.Length, 2) / 4);
					Mc = ConstNum.FIOMEGA0_9N * Mn;
					Vn = 0.6 * webPlate.Material.Fy * webPlate.Thickness * webPlate.Length;
					Vc = ConstNum.FIOMEGA1_0N * Vn;
					Mr = V * eccentricity;
				}

				if (beam.Moment == 0)
				{
					shart1 = webPlate.Thickness <= webPlate.Bolt.BoltSize / 2 + ConstNum.SIXTEENTH_INCH;
					shart2 = beam.Shape.tw <= webPlate.Bolt.BoltSize / 2 + ConstNum.SIXTEENTH_INCH;
					Shart3 = webPlate.Bolt.EdgeDistTransvDir >= 2 * webPlate.Bolt.BoltSize;
					Shart4 = beam.WinConnect.Beam.Lh >= 2 * webPlate.Bolt.BoltSize;

					MiscCalculationsWithReporting.EccentricBolt(1000, webPlate.Bolt.SpacingLongDir, webPlate.Bolt.SpacingTransvDir, boltNumberOfLines, boltNumberOfRows, ref C_Prime);
					if (C_Prime < 10)
						C_Prime = 100 * C_Prime / 100;
					else if (C_Prime < 98)
						C_Prime = 10 * C_Prime / 10;
					else
						C_Prime = 1 * C_Prime;

					switch (CommonDataStatic.Units)
					{
						case EUnit.US:
							switch (webPlate.Bolt.ASTMType)
							{
								case EBoltASTM.A325:
									Fvn = 48;
									if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
										Fvn = 54;
									break;
								case EBoltASTM.A490:
									Fvn = 60;
									if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
										Fvn = 68;
									break;
							}
							break;
						case EUnit.Metric:
							switch (webPlate.Bolt.ASTMType)
							{
								case EBoltASTM.A325:
									Fvn = 330;
									if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
										Fvn = 372;
									break;
								case EBoltASTM.A490:
									Fvn = 414;
									if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
										Fvn = 457;
									break;
								default:
									Fvn = 0.4 * webPlate.Material.Fu;
									if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
										Fvn = 0.45 * webPlate.Material.Fu;
									break;
							}
							break;
					}

					Ab = Math.PI * Math.Pow(webPlate.Bolt.BoltSize, 2) / 4;

					Mmax = 1.25 * Fvn * Ab * C_Prime;
					tMax = 6 * Mmax / (webPlate.Material.Fy * Math.Pow(webPlate.Length, 2));

					Mmax = 1.25 * webPlate.Bolt.BoltStrength / ConstNum.FIOMEGA0_75N * C_Prime;
					tMax = 6 * Mmax / (webPlate.Material.Fy * Math.Pow(webPlate.Length, 2));

					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
					{
						Maxt = primaryMember.Material.Fu * primaryMember.Shape.tf / webPlate.Material.Fy;
						tMax = Math.Min(Maxt, tMax);
					}

					if (webPlate.Thickness > tMax)
					{
						Dumyth1 = 0;
						do
						{
							Dumyth1 = Dumyth1 + ConstNum.SIXTEENTH_INCH;
						} while (!(CommonCalculations.PlateThickness(Dumyth1) >= tMax));
                        if (!webPlate.Thickness_User) 
                            webPlate.Thickness = CommonCalculations.PlateThickness(NumberFun.Round(Dumyth1 - ConstNum.SIXTEENTH_INCH, 16));
					}
				}

				if ((CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder) && beam.Moment == 0)
				{
					if (beam.ShearForce > 0 && oppositeBeam.ShearForce > 0)
					{
						suptw = primaryMember.Shape.tw;

						if (webPlate.Length > 0 && oppositeBeam.WinConnect.ShearWebPlate.Length > 0)
						{
							SupThickness = suptw / LRweldstress * beam.ShearForce / webPlate.Length;
							weldl = webPlate.Length;
						}
						else if (webPlate.Length > 0)
						{
							weldl = webPlate.Length;
							SupThickness = suptw;
						}
					}
				}
				else if ((CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder) && beam.Moment > 0)
				{
					if (beam.ShearForce > 0 && oppositeBeam.ShearForce > 0)
					{
						suptw = primaryMember.Shape.tw;

						if (webPlate.h2 > 0 && oppositeBeam.WinConnect.ShearWebPlate.h2 > 0)
						{
							LRweldstress = beam.ShearForce / webPlate.h2 + oppositeBeam.ShearForce / oppositeBeam.WinConnect.ShearWebPlate.h2;
							if (beam.MemberType == EMemberType.RightBeam)
								SupThickness = suptw / LRweldstress * beam.ShearForce / oppositeBeam.WinConnect.ShearWebPlate.h2;
							else
								SupThickness = suptw / LRweldstress * beam.ShearForce / oppositeBeam.WinConnect.ShearWebPlate.h2;
						}
						else
							SupThickness = suptw;
					}
				}

				weldl = webPlate.h2 - webPlate.Clip;

				ew = 0;
				c = EccentricWeld.GetEccentricWeld(0, 0, 0, true);
				//EccentricWeld.GetEccentricWeld(0, 0, ref c, 0, true);
				C1 = CommonCalculations.WeldTypeRatio();
				c = 2 * c * C1;
				w = V / (c * weldl) / 16;

                if (!webPlate.SupportWeldSize_User) 
                    webPlate.SupportWeldSize = w;
				Mfws = CommonCalculations.MinimumWeld(SupThickness1, webPlate.Thickness);
                if (!webPlate.SupportWeldSize_User) 
                    webPlate.SupportWeldSize = Math.Max(webPlate.SupportWeldSize, Mfws);

				w_req = webPlate.Material.Fu * webPlate.Thickness / (2 * 0.707 * webPlate.Weld.Fexx);
                if (!webPlate.SupportWeldSize_User) 
                    webPlate.SupportWeldSize = Math.Max(webPlate.SupportWeldSize, w_req);

				// EccTotal = 11.54
				if (webPlate.Eccentricity == EWebEccentricity.AISC)
					EccTotal = beam.EndSetback + beam.WinConnect.Beam.Lh + (webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir / 2;
				else
					EccTotal = (primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh + (webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir / 2;

				if (beam.Moment > 0)
					CommonDataStatic.F_Shear = V * EccTotal / webPlate.h2;
				if (beam.MomentConnection == EMomentCarriedBy.DirectlyWelded)
					PlThickness = Math.Max(beam.WinConnect.MomentDirectWeld.Top.ExtensionThickness, beam.WinConnect.MomentDirectWeld.Bottom.ExtensionThickness);
				else
					PlThickness = Math.Max(beam.WinConnect.MomentDirectWeld.Top.FlangeThickness, beam.WinConnect.MomentDirectWeld.Bottom.FlangeThickness);
				
				minweld = CommonCalculations.MinimumWeld(webPlate.Thickness, PlThickness);
				w = minweld;
				webPlate.BeamWeldSize = w;
				IterCount = 0;
				do
				{
					IterCount++;
					wp = webPlate.BeamWeldSize;
					webPlate.BeamWeldSize = NumberFun.Round(CommonDataStatic.F_Shear / (2 * ConstNum.FIOMEGA0_75N * 0.4242 * webPlate.Weld.Fexx * ((primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 - 2 * w)), 16);
					if (webPlate.BeamWeldSize > w)
						w = webPlate.BeamWeldSize;
				} while (wp != webPlate.BeamWeldSize && IterCount < 10);
				
				if (webPlate.BeamWeldSize < minweld)
					webPlate.BeamWeldSize = minweld;
				if(beam.Moment != 0 && CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToColumnFlange)
				{
					if (beam.MomentConnection == EMomentCarriedBy.FlangePlate)
						webPlate.TopOffset = 0;
					else
						webPlate.TopOffset = -beam.Shape.tf;
				}
                if (!webPlate.Height_User) 
                    webPlate.Height = webPlate.h2;
			}
			else
			{
				boltNumberOfRowsMax = (int)Math.Floor((beam.WinConnect.Beam.WebHeight - 2 * webPlate.Bolt.EdgeDistLongDir) / webPlate.Bolt.SpacingLongDir) + 1;
				Nmn = (int)Math.Ceiling((beam.WinConnect.Beam.WebHeight / 2) / webPlate.Bolt.SpacingLongDir);
				if (Nmn < 2)
					Nmn = 2;
				if (boltNumberOfRowsMax < 2)
				{
					Reporting.AddLine("Two bolts won't fit in web. (NG)");
					boltNumberOfRowsMax = 2;
				}
				VnTemp = (beam.Shape.d - beam.WinConnect.Beam.TCopeD - beam.WinConnect.Beam.BCopeD - Nmn * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * beam.Shape.tw * ConstNum.FIOMEGA0_75N * 0.6 * beam.Material.Fu;
				beam.WinConnect.Fema.Vg = (beam.Shape.d - beam.WinConnect.Beam.TCopeD - beam.WinConnect.Beam.BCopeD) * beam.Shape.tw * ConstNum.FIOMEGA1_0N * 0.6 * beam.Material.Fy;
				if (VnTemp < V || beam.WinConnect.Fema.Vg < V)
				{
					Reporting.AddLine("Beam shear is too high. (NG)");
					return;
				}
				ShearCap = 0;
				boltNumberOfLines = 0;
				boltNumberOfRows = Nmn - 1;
				BearingCapacity = 0;
				while (ShearCap < V || BeamTearOutCap != 0 && BeamTearOutCap < Taxial || BearingCapacity < V)
				{
					if (boltNumberOfRows == boltNumberOfRowsMax)
					{
						boltNumberOfRows = (Nmn - 1);
						boltNumberOfLines++;
					}

                    if (!webPlate.BoltNumberOfLines_User) 
                        webPlate.Bolt.NumberOfLines = boltNumberOfLines;
                    else
	                    boltNumberOfLines = webPlate.Bolt.NumberOfLines;
					
					boltNumberOfRows++;
					if (!webPlate.Bolt.NumberOfRows_User)
					{
						webPlate.Bolt.NumberOfRows = boltNumberOfRows;
						if (!webPlate.BoltNumberOfLines_User && boltNumberOfLines == 0)
							webPlate.Bolt.NumberOfLines = boltNumberOfLines = 1;
					}
					else
					{
						boltNumberOfRows = webPlate.Bolt.NumberOfRows;
						boltNumberOfLines++;
						if (!webPlate.BoltNumberOfLines_User)
							webPlate.Bolt.NumberOfLines = boltNumberOfLines;
						else
							boltNumberOfLines = webPlate.Bolt.NumberOfLines;
					}

					webPlate.Width = beam.EndSetback + beam.WinConnect.Beam.Lh + (boltNumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir + webPlate.Bolt.EdgeDistTransvDir;

					CalculateEccentricity(memberType, ref boltNumberOfLines, ref eccentricity, ref eccforbending);
					MiscCalculationsWithReporting.EccentricBolt(eccentricity, webPlate.Bolt.SpacingLongDir, webPlate.Bolt.SpacingTransvDir, boltNumberOfLines, boltNumberOfRows, ref c);
					ShearCap = webPlate.NumberOfPlates * c * webPlate.Bolt.BoltStrength;
					MiscCalculationsWithReporting.BlockShear(memberType, EShearOpt.SpliceWithMoment, ref BsCap, false);
					ShearCap = Math.Min(ShearCap, BsCap);
					if (beam.Moment == 0 && (beam.WinConnect.Beam.TopCope & beam.WinConnect.Beam.BottomCope) && beam.P != 0)
						BeamTearOutCap = MiscCalculationsWithReporting.BeamWebTearout(beam.MemberType, webPlate.Bolt.NumberOfRows, boltNumberOfLines, webPlate.Bolt.SpacingLongDir, webPlate.Bolt.SpacingTransvDir, EBeamOrAttachment.Beam, ETearOutBlockShear.TearOut, true);

					Fbs = CommonCalculations.SpacingBearing(webPlate.Bolt.SpacingLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Bolt.HoleType, beam.Material.Fu, false);
					if (beam.WinConnect.Beam.TCopeD > 0)
					{
						Fbe = CommonCalculations.EdgeBearing(beam.WinConnect.Beam.DistanceToFirstBolt, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, beam.Material.Fu, webPlate.Bolt.HoleType, false);
						EquivBoltFactor = c / (webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows);
						if (EquivBoltFactor > 1)
							EquivBoltFactor = 1;
						BearingCapacity = EquivBoltFactor * webPlate.Bolt.NumberOfLines * (Fbe + Fbs * (webPlate.Bolt.NumberOfRows - 1)) * beam.Shape.tw;
					}
					else
					{
						Fbe = Fbs;
						EquivBoltFactor = c / (webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows);
						if (EquivBoltFactor > 1)
							EquivBoltFactor = 1;
						BearingCapacity = EquivBoltFactor * webPlate.Bolt.NumberOfLines * Fbs * webPlate.Bolt.NumberOfRows * beam.Shape.tw;
					}
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange && CommonDataStatic.Preferences.Seismic == ESeismic.Seismic && CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.IMF)
					{
						if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic && CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.IMF)
						{
							boltNumberOfRows = (int)((MaxL - 2 * webPlate.Bolt.EdgeDistLongDir) / webPlate.Bolt.SpacingLongDir + 1);
                            if (!webPlate.Bolt.NumberOfRows_User) 
                                webPlate.Bolt.NumberOfRows = boltNumberOfRows;
						}
					}

					if (webPlate.BoltNumberOfLines_User && webPlate.Bolt.NumberOfRows == boltNumberOfRowsMax)
						break;
					//else if (webPlate.Bolt.NumberOfRows_User)
					//	break;

					Mn = webPlate.Material.Fy * webPlate.Thickness * (Math.Pow(webPlate.Length, 2) / 4);
					Mc = ConstNum.FIOMEGA0_9N * Mn;
					Vn = 0.6 * webPlate.Material.Fy * webPlate.Thickness * webPlate.Length;
					Vc = ConstNum.FIOMEGA1_0N * Vn;
					Mr = V * eccentricity;
				}

				webPlate.Length = webPlate.Bolt.SpacingLongDir * (boltNumberOfRows - 1) + 2 * webPlate.Bolt.EdgeDistLongDir;
				SPln = webPlate.Length - boltNumberOfRows * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
				spt1 = V / (ConstNum.FIOMEGA0_75N * 0.6 * webPlate.Material.Fu * SPln * webPlate.NumberOfPlates);
				spt2 = V / (ConstNum.FIOMEGA1_0N * 0.6 * webPlate.Material.Fy * webPlate.Length * webPlate.NumberOfPlates);
				Lgt = webPlate.Bolt.EdgeDistTransvDir + (webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir;
				Lnt = Lgt - (webPlate.Bolt.NumberOfLines - 0.5) * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
				Lgv = webPlate.Length - webPlate.Bolt.EdgeDistLongDir;
				Lnv = Lgv - (webPlate.Bolt.NumberOfRows - 0.5) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);

				Ubs = webPlate.Bolt.NumberOfLines > 1 ? 0.5 : 1;

				beam.WinConnect.Fema.L = webPlate.Length;
				d = webPlate.Bolt.HoleWidth;
				n = webPlate.Bolt.NumberOfRows;
				s = webPlate.Bolt.SpacingLongDir;
				nn = n / 2;
				n1 = (n + 1);

				Sgross = Math.Pow(beam.WinConnect.Fema.L, 2) / 6;
				Zgross = Math.Pow(beam.WinConnect.Fema.L, 2) / 4;

				if (n == ((int) Math.Floor(n / 2.0)) * 2)
					Znet = Zgross - 2 * (Math.Pow(n, 2) / 8) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * webPlate.Bolt.SpacingLongDir;
				else
					Znet = Zgross - 2 * ((Math.Pow(n, 2) - 1) / 8) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * webPlate.Bolt.SpacingLongDir;

				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
				{
					spt3 = V * (beam.EndSetback + beam.WinConnect.Beam.Lh) / (ConstNum.FIOMEGA0_75N * webPlate.Material.Fu * Znet * webPlate.NumberOfPlates);
					spt4 = V * (beam.EndSetback + beam.WinConnect.Beam.Lh) / (ConstNum.FIOMEGA0_9N * webPlate.Material.Fy * Zgross * webPlate.NumberOfPlates);
				}
				else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
				{
					if (webPlate.Eccentricity == EWebEccentricity.AISC)
					{
						spt3 = 0;
						spt4 = 0;
					}
					else
					{
						spt3 = V * (beam.EndSetback + beam.WinConnect.Beam.Lh) / (2 * ConstNum.FIOMEGA0_75N * webPlate.Material.Fu * Znet * webPlate.NumberOfPlates);
						spt4 = V * (beam.EndSetback + beam.WinConnect.Beam.Lh) / (2 * ConstNum.FIOMEGA0_9N * webPlate.Material.Fy * Sgross * webPlate.NumberOfPlates);
					}
				}
				else
				{
					if (webPlate.Eccentricity == EWebEccentricity.AISC)
					{
						spt3 = 0;
						spt4 = 0;
					}
					else
					{
						if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder && webPlate.ExtendedConfiguration)
						{
							spt3 = V * ((primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh) / (ConstNum.FIOMEGA0_75N * webPlate.Material.Fu * Znet * webPlate.NumberOfPlates);
							spt4 = V * ((primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh) / (ConstNum.FIOMEGA0_9N * webPlate.Material.Fy * Sgross * webPlate.NumberOfPlates);
						}
						else
						{
							spt3 = V * (beam.EndSetback + beam.WinConnect.Beam.Lh) / (ConstNum.FIOMEGA0_75N * webPlate.Material.Fu * Znet * webPlate.NumberOfPlates);
							spt4 = V * (beam.EndSetback + beam.WinConnect.Beam.Lh) / (ConstNum.FIOMEGA0_9N * webPlate.Material.Fy * Sgross * webPlate.NumberOfPlates);
						}
					}
				}

				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
				{
					MinimumThickness = Math.Min(beam.Shape.tw, oppositeBeam.Shape.tw);
					if (webPlate.NumberOfPlates == 1)
						spt1 = Math.Max(spt1, MinimumThickness);
					else
						spt1 = Math.Max(spt1, MinimumThickness / 2);
				}

				spt3 = Math.Max(spt3, spt4);
				spt = Math.Max(spt1, spt2);
				tminimum = Math.Max(ConstNum.QUARTER_INCH, webPlate.Length * Math.Sqrt(webPlate.Material.Fy) / (2.45 * Math.Pow(ConstNum.ELASTICITY, 0.5)));
				spt = Math.Max(Math.Max(spt, tminimum), spt3);

				if (primaryMember.Moment == 0)
					cp = webPlate.Width - webPlate.Bolt.EdgeDistTransvDir - webPlate.Bolt.SpacingTransvDir * (webPlate.Bolt.NumberOfLines - 1);     // eccforbending
				else if (primaryMember.Moment > 0)
					cp = webPlate.Width - (primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 - webPlate.Bolt.EdgeDistTransvDir - webPlate.Bolt.SpacingTransvDir * (webPlate.Bolt.NumberOfLines - 1);     // eccforbending

				h0 = webPlate.Length;
				spt6 = spt - ConstNum.SIXTEENTH_INCH;
				do
				{
					spt6 = spt6 + ConstNum.SIXTEENTH_INCH;
					Lambda = h0 * Math.Pow(beam.Material.Fy, 0.5) / (10 * spt6 * Math.Pow(475 + 280 * Math.Pow(h0 / cp, 2), 0.5));
					if (Lambda <= 0.7)
						Q = 1;
					else if (Lambda <= 1.41)
						Q = 1.34 - 0.486 * Lambda;
					else
						Q = 1.3 / Math.Pow(Lambda, 2);
					Fcr = webPlate.Material.Fy * Q;
					FiFcr = ConstNum.FIOMEGA0_9N * Fcr;
					beam.WinConnect.Fema.L = webPlate.Length;
					d = webPlate.Bolt.HoleWidth;
					n = boltNumberOfRows;
					s = webPlate.Bolt.SpacingLongDir;
					nn = n / 2;
					n1 = (n + 1);
					if (((int) Math.Floor(boltNumberOfRows / 2.0)) * 2 == boltNumberOfRows)
						SnetDeduction = (4 * Math.Pow(nn, 3) / 3 + 2 * nn / 3 - nn) * Math.Pow(s, 2) * (d + ConstNum.SIXTEENTH_INCH) * spt6 / beam.WinConnect.Fema.L;
					else
						SnetDeduction = (Math.Pow(n1, 3) / 2 - 3 * Math.Pow(n1, 2) / 2 + n1) * Math.Pow(s, 2) * (d + ConstNum.SIXTEENTH_INCH) * spt6 / (3 * beam.WinConnect.Fema.L);
					SnetforBuckling = spt6 * Math.Pow(beam.WinConnect.Fema.L, 2) / 6 - SnetDeduction;
				} while (V > FiFcr * SnetforBuckling / Math.Abs(cp));
				spt = Math.Max(spt, spt6);

                if (!webPlate.Thickness_User) 
                    webPlate.Thickness = CommonCalculations.PlateThickness(NumberFun.Round(Math.Max(spt, spt3), 16));
                if (!webPlate.BoltNumberOfLines_User) 
                    webPlate.Bolt.NumberOfLines = boltNumberOfLines; // 44444
                if (!webPlate.Bolt.NumberOfRows_User) 
                    webPlate.Bolt.NumberOfRows = boltNumberOfRows;

				Fbe = CommonCalculations.EdgeBearing(webPlate.Bolt.EdgeDistLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Material.Fu, webPlate.Bolt.HoleType, false);
				Fbs = CommonCalculations.SpacingBearing(webPlate.Bolt.SpacingLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Bolt.HoleType, webPlate.Material.Fu, false);
				EquivBoltFactor = c / (webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows);
				if (EquivBoltFactor > 1)
					EquivBoltFactor = 1;
				BearingCap = (EquivBoltFactor * webPlate.Bolt.NumberOfLines * (Fbe + Fbs * (webPlate.Bolt.NumberOfRows - 1)) * webPlate.Thickness * webPlate.NumberOfPlates);
				if (BearingCap < V)
                    if (!webPlate.Thickness_User) 
                        webPlate.Thickness = CommonCalculations.PlateThickness(NumberFun.Round(webPlate.Thickness * V / BearingCap, 16));
				if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice)
				{
					weldl = beam.WinConnect.Fema.L;
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
					{
						weldl = webPlate.h2 - webPlate.Clip;
						ew = 0;
					}
					else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
					{
						if (webPlate.Eccentricity == EWebEccentricity.AISC)
							ew = 0;
						else
							ew = (beam.EndSetback + beam.WinConnect.Beam.Lh) / (2 * weldl);
					}
					else
					{
						ew = 0;
						if (webPlate.WebPlateStiffener == EWebPlateStiffener.With)
							beam.WinConnect.Fema.L = (webPlate.Height - webPlate.Clip);
						else
							beam.WinConnect.Fema.L = (webPlate.Length - webPlate.Clip);
					}
					c = EccentricWeld.GetEccentricWeld(0, ew, 0, false);
					//EccentricWeld.GetEccentricWeld(0, 0, ref c, 0, false);
					C1 = CommonCalculations.WeldTypeRatio();
					c = 2 * c * C1;
					w = V / (c * beam.WinConnect.Fema.L) / 10; // "/ 10" added for the calc to be correct (MT 02/10/15)

                    if (!webPlate.SupportWeldSize_User) 
                        webPlate.SupportWeldSize = w;
					Mfws = CommonCalculations.MinimumWeld(SupThickness1, webPlate.Thickness);
                    if (!webPlate.SupportWeldSize_User) 
                        webPlate.SupportWeldSize = Math.Max(webPlate.SupportWeldSize, Mfws);
					w_req = webPlate.Material.Fu * webPlate.Thickness / (2 * 0.707 * webPlate.Weld.Fexx);
                    if (!webPlate.SupportWeldSize_User) 
                        webPlate.SupportWeldSize = Math.Max(webPlate.SupportWeldSize, w_req);

					if (CommonDataStatic.IsFema)
					{
						switch (beam.WinConnect.Fema.Connection)
						{
							case EFemaConnectionType.RBS:
                                if (!webPlate.SupportWeldSize_User) 
                                    webPlate.SupportWeldSize = Math.Max(ConstNum.FIOMEGA0_75N * webPlate.Thickness, Mfws);
								break;
							case EFemaConnectionType.WUFB:
								w_req = Math.Max(webPlate.Material.Fu * webPlate.Thickness / (2 * 0.707 * webPlate.Weld.Fexx), Mfws);
                                if (!webPlate.SupportWeldSize_User) 
                                    webPlate.SupportWeldSize = w_req;
								break;
						}
					}
				}
			}

			webPlate.Bolt.NumberOfBolts = webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows;
			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && beam.Moment > 0)
			{
				if (webPlate.Eccentricity == EWebEccentricity.AISC)
					EccTotal = beam.EndSetback + beam.WinConnect.Beam.Lh + (webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir / 2;
				else
					EccTotal = (primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh + (webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir / 2;
				CommonDataStatic.F_Shear = V * EccTotal / webPlate.h2;
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb &&
				beam.Moment == 0 && webPlate.WebPlateStiffener == EWebPlateStiffener.With)
			{
				double width = (primaryMember.Shape.bf - primaryMember.Shape.tw) / 2;
				webPlate.StiffenerThickness = CommonCalculations.PlateThickness(width * Math.Pow(webPlate.Material.Fy / ConstNum.ELASTICITY, 0.5) / 0.56);
			}

			Reporting.AddHeader(beam.ComponentName + " - " + beam.ShapeName + " Shear Connection");
			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
				beam.WinConnect.Fema.PlWidth = webPlate.Width + oppositeBeam.WinConnect.ShearWebPlate.Width;
			else
				beam.WinConnect.Fema.PlWidth = webPlate.Width;

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
				weldl = webPlate.h2 - webPlate.Clip;
			else if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice)
				weldl = webPlate.Length;

			boltNumberOfLines = webPlate.Bolt.NumberOfLines;

			CalculateEccentricity(memberType, ref boltNumberOfLines, ref eccentricity, ref eccforbending);

			Reporting.AddHeader(beam.ComponentName + " - " + beam.ShapeName);
			if (webPlate.NumberOfPlates == 2)
			{
				Reporting.AddMainHeader("Shear Connection Using Two Plates:");
				Reporting.AddLine("Plates: 2 PL " + webPlate.Length + ConstUnit.Length + " X " + beam.WinConnect.Fema.PlWidth + ConstUnit.Length + " X " + webPlate.Thickness + ConstUnit.Length);
			}
			else
			{
				Reporting.AddMainHeader("Shear Connection Using One Plate:");
				Reporting.AddLine("Plate (W x L x T): " + webPlate.Length + ConstUnit.Length + " X " + beam.WinConnect.Fema.PlWidth + ConstUnit.Length + " X " + webPlate.Thickness + ConstUnit.Length);
			}
			Reporting.AddLine("Plate Material: " + webPlate.Material.Name);

			webPlate.Bolt.NumberOfBolts = webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows;
			boltNumberOfRows = webPlate.Bolt.NumberOfRows;
			Reporting.AddLine("Beam Setback: " + beam.EndSetback + ConstUnit.Length);
			Reporting.AddLine("Bolts: (" + webPlate.Bolt.NumberOfBolts + ") " + webPlate.Bolt.BoltName);
			Reporting.AddLine("Bolt Holes on Beam Web: " + webPlate.Bolt.HoleWidthSupport + ConstUnit.Length + " Vert. X " + webPlate.Bolt.HoleLengthSupport + ConstUnit.Length + " Horiz.");
			Reporting.AddLine("Bolt Holes on Plate: " + webPlate.Bolt.HoleWidth + ConstUnit.Length + " Vert. X " + webPlate.Bolt.HoleLength + ConstUnit.Length + " Horiz.");
			if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice)
				Reporting.AddLine("Weld: " + CommonCalculations.WeldSize(webPlate.SupportWeldSize) + " " + webPlate.WeldName + " - " + "Fillet Welds");

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
			{
				Maxt = primaryMember.Material.Fu * primaryMember.Shape.tf / webPlate.Material.Fy;
				Reporting.AddHeader("Shear Yielding of HSS face:");
				if (webPlate.Thickness <= Maxt)
				{
					Reporting.AddLine("tp * Fyp <= Fu * t:");
					Reporting.AddLine((webPlate.Thickness * webPlate.Material.Fy) + " <=  " + (primaryMember.Material.Fu * primaryMember.Shape.tf) + " (OK)");
				}
				else
				{
					Reporting.AddLine("tp * Fyp <= Fu * t :");
					Reporting.AddLine((webPlate.Thickness * webPlate.Material.Fy) + " >> " + (primaryMember.Material.Fu * primaryMember.Shape.tf) + " (NG)");
				}
			}

			if (beam.Moment == 0)
			{
				shart1 = webPlate.Thickness <= webPlate.Bolt.BoltSize / 2 + ConstNum.SIXTEENTH_INCH;
				shart2 = beam.Shape.tw <= webPlate.Bolt.BoltSize / 2 + ConstNum.SIXTEENTH_INCH;
				Shart3 = webPlate.Bolt.EdgeDistTransvDir >= 2 * webPlate.Bolt.BoltSize;
				Shart4 = beam.WinConnect.Beam.Lh >= 2 * webPlate.Bolt.BoltSize;
				if ((webPlate.Bolt.NumberOfLines == 1 && (shart1 || shart2) && Shart3 && Shart4) ||
				    (webPlate.Bolt.NumberOfLines == 2 && (shart1 && shart2) && Shart3 && Shart4))
					Reporting.AddLine("Equation [tmax = 6 * Mmax / (Fy * d²)] need not be checked");
				else
				{
					MiscCalculationsWithReporting.EccentricBolt(1000, webPlate.Bolt.SpacingLongDir, webPlate.Bolt.SpacingTransvDir, boltNumberOfLines, boltNumberOfRows, ref C_Prime);
					if (C_Prime < 10)
						C_Prime = 100 * C_Prime / 100;
					else if (C_Prime < 98)
						C_Prime = 10 * C_Prime / 10;
					else
						C_Prime = 1 * C_Prime;

					switch (CommonDataStatic.Units)
					{
						case EUnit.US:
							switch (webPlate.Bolt.ASTMType)
							{
								case EBoltASTM.A325:
									Fvn = 48;
									if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
										Fvn = 54;
									break;
								case EBoltASTM.A490:
									Fvn = 60;
									if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
										Fvn = 68;
									break;
							}
							break;
						case EUnit.Metric:
							switch (webPlate.Bolt.ASTMType)
							{
								case EBoltASTM.A325:
									Fvn = 330;
									if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
										Fvn = 372;
									break;
								case EBoltASTM.A490:
									Fvn = 414;
									if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
										Fvn = 457;
									break;
								default:
									Fvn = 0.4 * webPlate.Material.Fu;
									if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
										Fvn = 0.45 * webPlate.Material.Fu;
									break;
							}
							break;
					}

					Ab = Math.PI * Math.Pow(webPlate.Bolt.BoltSize, 2) / 4;

					Mmax = 1.25 * Fvn * Ab * C_Prime;
					tMax = 6 * Mmax / (webPlate.Material.Fy * Math.Pow(webPlate.Length, 2));
					Reporting.AddLine("Mmax = 1.25 * FvAb * C' = 1.25 * " + Fvn * Ab + " * " + C_Prime + " = " + Mmax + ConstUnit.MomentUnitInch);
					Reporting.AddLine("t_Max = 6 * Mmax / (Fy * L²) = 6 * " + Mmax + " / (" + webPlate.Material.Fy + " * " + webPlate.Length + "²)");
					if (tMax >= webPlate.Thickness)
						Reporting.AddLine("= " + tMax + " >= " + webPlate.Thickness + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + tMax + " << " + webPlate.Thickness + ConstUnit.Length + " (NG)");
				}
			}

			R = Math.Sqrt(V * V);	//R = Math.Sqrt(h * h + V * V); h was always 0
			Reporting.AddHeader("Loading:");
			Reporting.AddLine("Vertical Shear (V) = " + V + ConstUnit.Force);
			Reporting.AddLine("Axial Load (H) = " + 0 + ConstUnit.Force);
			Reporting.AddLine("Resultant (R) = (V² + H²)^0.5 ");
			Reporting.AddLine("= ((" + V + " )² + " + 0 + "²)^0.5 ");
			Reporting.AddLine("= " + R + ConstUnit.Force);

			Reporting.AddHeader("Check Bolt Spacing and Edge Distance:");
			spBeam = MiscCalculationsWithReporting.MinimumSpacing((webPlate.Bolt.BoltSize), (V / webPlate.Bolt.NumberOfBolts), beam.Material.Fu * beam.Shape.tw, webPlate.Bolt.HoleWidth, webPlate.Bolt.HoleType);
			spPlate = MiscCalculationsWithReporting.MinimumSpacing((webPlate.Bolt.BoltSize), (V / webPlate.Bolt.NumberOfBolts), webPlate.Material.Fu * webPlate.Thickness * webPlate.NumberOfPlates, webPlate.Bolt.HoleWidth, webPlate.Bolt.HoleType);
			minsp = Math.Max(spBeam, spPlate);
			if (webPlate.Bolt.SpacingLongDir >= minsp)
				Reporting.AddLine("Spacing (s) = " + webPlate.Bolt.SpacingLongDir + " >= Minimum Spacing = " + minsp + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Spacing (s) = " + webPlate.Bolt.SpacingLongDir + " << Minimum Spacing = " + minsp + ConstUnit.Length + " (NG)");

			evmin = MiscCalculationsWithReporting.MinimumEdgeDist(webPlate.Bolt.BoltSize, V / webPlate.Bolt.NumberOfBolts, webPlate.Material.Fu * webPlate.Thickness * webPlate.NumberOfPlates, webPlate.Bolt.MinEdgeSheared, webPlate.Bolt.HoleWidth, webPlate.Bolt.HoleType);
			Reporting.AddLine(string.Empty);
			Reporting.AddLine("Distance to Horiz. Edge of PL (ev):");
			if (webPlate.Bolt.EdgeDistLongDir >= evmin)
				Reporting.AddLine("= " + webPlate.Bolt.EdgeDistLongDir + " >= " + evmin + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("= " + webPlate.Bolt.EdgeDistLongDir + " << " + evmin + ConstUnit.Length + " (NG)");

			ehmin = MiscCalculationsWithReporting.MinimumEdgeDist(webPlate.Bolt.BoltSize, 0.0, webPlate.Material.Fu * webPlate.Thickness * webPlate.NumberOfPlates, webPlate.Bolt.MinEdgeSheared, webPlate.Bolt.HoleWidth, webPlate.Bolt.HoleType);
			Reporting.AddLine(string.Empty);
			Reporting.AddLine("Distance to Vert. Edge of PL (eh):");
			if (webPlate.Bolt.EdgeDistTransvDir >= ehmin)
				Reporting.AddLine("= " + webPlate.Bolt.EdgeDistTransvDir + " >= " + ehmin + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("= " + webPlate.Bolt.EdgeDistTransvDir + " << " + ehmin + ConstUnit.Length + " (NG)");

			if (webPlate.Bolt.NumberOfLines > 1)
			{
				minsp = NumberFun.Round(2.6666666 * webPlate.Bolt.BoltSize, 16);
				if (webPlate.Bolt.SpacingTransvDir >= minsp)
					Reporting.AddLine("Horiz. Spacing = " + webPlate.Bolt.SpacingTransvDir + " >= Minimum Spacing = " + minsp + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Horiz. Spacing = " + webPlate.Bolt.SpacingTransvDir + " << Minimum Spacing = " + minsp + ConstUnit.Length + " (NG)");
			}

			Reporting.AddHeader("Bolt Strength:");
			CalculateEccentricity(memberType, ref boltNumberOfLines, ref eccentricity, ref eccforbending);

			MiscCalculationsWithReporting.EccentricBolt(eccentricity, webPlate.Bolt.SpacingLongDir, webPlate.Bolt.SpacingTransvDir, webPlate.Bolt.NumberOfLines, webPlate.Bolt.NumberOfRows, ref c);
			ShearCap = webPlate.NumberOfPlates * c * webPlate.Bolt.BoltStrength;
			Reporting.AddGoToHeader(ConstString.DES_OR_ALLOWABLE + " Shear Strength of Bolts:");
			Reporting.AddLine("Number of Vertical Bolt Lines = " + webPlate.Bolt.NumberOfLines);
			Reporting.AddLine("Number of Rows of Bolts = " + webPlate.Bolt.NumberOfRows);
			Reporting.AddLine("Horizontal Spacing = " + webPlate.Bolt.SpacingTransvDir + ConstUnit.Length);
			Reporting.AddLine("Vertical Spacing = " + webPlate.Bolt.SpacingLongDir + ConstUnit.Length);
			Reporting.AddLine("Eccentricity = " + eccentricity + ConstUnit.Length);
			Reporting.AddLine("C = " + c);

			if (c < webPlate.Bolt.NumberOfBolts)
				webPlate.Bolt.disableLSLNWithBoltGroupEccentricity = true;
			else
				webPlate.Bolt.disableLSLNWithBoltGroupEccentricity = false;

			Reporting.AddLine(string.Empty);
			Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = Npl * C * Fv");
			Reporting.AddLine("= " + webPlate.NumberOfPlates + " * " + c + " * " + webPlate.Bolt.BoltStrength);
			if (ShearCap >= V)
				Reporting.AddCapacityLine("= " + ShearCap + " >= " + V + ConstUnit.Force + " (OK)", V / ShearCap, "Shear Capacity", memberType);
			else
				Reporting.AddCapacityLine("= " + ShearCap + " << " + V + ConstUnit.Force + " (NG)", V / ShearCap, "Shear Capacity", memberType);

			MiscCalculationsWithReporting.BlockShear(memberType, EShearOpt.SpliceWithMoment, ref BsCap, true);
			if (Taxial != 0 && beam.Moment == 0)
			{
				//  BeamTearOutCap = BeamWebTearout(WebPlate.Numrows, Nvl, WebPlateBolts.sl, WebPlateBolts.st, "Beam", "TearOut")
				BeamBlockShearCap = MiscCalculationsWithReporting.BeamWebTearout(beam.MemberType, webPlate.Bolt.NumberOfRows, boltNumberOfLines, webPlate.Bolt.SpacingLongDir, webPlate.Bolt.SpacingTransvDir, EBeamOrAttachment.Beam, ETearOutBlockShear.BlockShear, true);

				if (BeamBlockShearCap >= Math.Abs(Taxial))
					Reporting.AddCapacityLine("= " + BeamBlockShearCap + " >= " + Math.Abs(Taxial) + ConstUnit.Force + " (OK)", Math.Abs(Taxial) / BeamBlockShearCap, "Bolt Strength", memberType);
				else
					Reporting.AddCapacityLine("= " + BeamBlockShearCap + " << " + Math.Abs(Taxial) + ConstUnit.Force + " (NG)", Math.Abs(Taxial) / BeamBlockShearCap, "Bolt Strength", memberType);
			}

			if (beam.WinConnect.Beam.TopCope || beam.WinConnect.Beam.BottomCope)
				Cope.CopedBeam(beam.MemberType, true);

			beam.WinConnect.Fema.L = webPlate.Length;
			d = webPlate.Bolt.HoleWidth;
			s = webPlate.Bolt.SpacingLongDir;

			string capacityText = ConstString.DES_OR_ALLOWABLE + " Shear Strength of the Plate";
			Reporting.AddHeader(capacityText + ":");
			Aw = webPlate.Length * webPlate.Thickness;
			FiVn = (MiscCalculationsWithReporting.GrossShearStrength(webPlate.Material.Fy, Aw, memberType, true) * webPlate.NumberOfPlates);
			if (webPlate.NumberOfPlates == 2)
				Reporting.AddLine("For two plates:");
			if (FiVn >= V)
				Reporting.AddCapacityLine(ConstString.PHI + "Vn = " + FiVn + " >= " + V + ConstUnit.Force + " (OK)", V / FiVn, capacityText, memberType);
			else
				Reporting.AddCapacityLine(ConstString.PHI + "Vn = " + FiVn + " << " + V + ConstUnit.Force + " (NG)", V / FiVn, capacityText, memberType);

			Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Shear Rupture Strength:");
			An = (webPlate.Length - webPlate.Bolt.NumberOfRows * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * webPlate.Thickness;
			Reporting.AddLine("Net Area (An) = (L - nL * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t ");
			Reporting.AddLine("= (" + webPlate.Length + " - " + webPlate.Bolt.NumberOfRows + " * " + (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + webPlate.Thickness + " = " + An + ConstUnit.Area);
			VnCap = webPlate.NumberOfPlates * An * ConstNum.FIOMEGA0_75N * 0.6 * webPlate.Material.Fu;

			Reporting.AddLine("Shear Rupture Strength = Npl * An * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu = " + webPlate.NumberOfPlates + " * " + An + " * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + webPlate.Material.Fu);
			if (VnCap >= V)
				Reporting.AddCapacityLine("= " + VnCap + " >= " + V + ConstUnit.Force + " (OK)", V / VnCap, "Shear Rupture Strength", memberType);
			else
				Reporting.AddCapacityLine("= " + VnCap + " << " + V + ConstUnit.Force + " (NG)", V / VnCap, "Shear Rupture Strength", memberType);

			Reporting.AddHeader("Block Shear Strength of the Plate:");
			Agt = (webPlate.Bolt.EdgeDistTransvDir - NumberFun.ConvertFromFraction(4) + (webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir) * webPlate.Thickness * webPlate.NumberOfPlates;
			Reporting.AddLine("Gross Area with Tension Resistance (Agt)");
			Reporting.AddLine("= (et + (Nh - 1) * sh) * t");
			Reporting.AddLine("= (" + (webPlate.Bolt.EdgeDistTransvDir - NumberFun.ConvertFromFraction(4)) + " + (" + webPlate.Bolt.NumberOfLines + " - 1) * " + webPlate.Bolt.SpacingTransvDir + ") * " + (webPlate.Thickness * webPlate.NumberOfPlates));
			Reporting.AddLine("= " + Agt + ConstUnit.Area);
			Ant = (Agt - (webPlate.Bolt.NumberOfLines - 0.5) * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * webPlate.Thickness * webPlate.NumberOfPlates);
			Reporting.AddLine("Net Area with Tension Resistance (Ant)");
			Reporting.AddLine("= Agt - (Nh - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
			Reporting.AddLine("= " + Agt + " - (" + webPlate.Bolt.NumberOfLines + " - 0.5) * (" + webPlate.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ") * " + (webPlate.Thickness * webPlate.NumberOfPlates));
			Reporting.AddLine("= " + Ant + ConstUnit.Area);

			Agv = (webPlate.Length - webPlate.Bolt.EdgeDistLongDir) * webPlate.Thickness * webPlate.NumberOfPlates;
			Reporting.AddHeader("Gross Area with Shear Resistance (Agv)");
			Reporting.AddLine("= (L - el) * t = (" + webPlate.Length + " - " + webPlate.Bolt.EdgeDistLongDir + ") * " + (webPlate.Thickness * webPlate.NumberOfPlates) + " = " + Agv + ConstUnit.Area);

			Anv = (Agv - (webPlate.Bolt.NumberOfRows - 0.5) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * webPlate.Thickness * webPlate.NumberOfPlates);
			Reporting.AddHeader("Net Area with Shear Resistance (Anv)");
			Reporting.AddLine("= Agv - (Nv - 0.5) * (dv + " + ConstNum.SIXTEENTH_INCH + ") * t");
			Reporting.AddLine("= " + Agv + " - (" + webPlate.Bolt.NumberOfRows + " - 0.5) * (" + webPlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")* " + (webPlate.Thickness * webPlate.NumberOfPlates));
			Reporting.AddLine("= " + Anv + ConstUnit.Area);
			Ubs = webPlate.Bolt.NumberOfLines > 1 ? 0.5 : 1;
			FiRn = MiscCalculationsWithReporting.BlockShearNew(webPlate.Material.Fu, Anv, Ubs, Ant, Agv, webPlate.Material.Fy, 0, V, true);
			M_Req = V * eccforbending;

			if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC13)
			{
				Reporting.AddHeader("Check Flexure with Shear:");
				Reporting.AddLine("Required Strength (M_Req)");
				Reporting.AddLine("= V * e = " + V + " * " + eccforbending + " = " + M_Req + ConstUnit.MomentUnitInch);
				fshear = V / (webPlate.Thickness * webPlate.Length);
				Reporting.AddLine("fv = V / (tp * L)= " + V + " / (" + webPlate.Thickness + " * " + webPlate.Length + ")= " + fshear + ConstUnit.Stress);
				
				Reporting.AddLine(string.Empty);
				if (Math.Pow(webPlate.Material.Fy, 2) - 3 * Math.Pow(fshear, 2) > 0)
				{
					Fcr = Math.Pow(Math.Pow(webPlate.Material.Fy, 2) - 3 * Math.Pow(fshear, 2), 0.5);

					Reporting.AddLine("Fcr = (Fy² - 3 * fv²)^0.5 = (" + webPlate.Material.Fy + "² - 3 * " + fshear + "²)^0.5 = " + Fcr + ConstUnit.Stress);
					Zgross = Math.Pow(beam.WinConnect.Fema.L, 2) / 4 * webPlate.Thickness * webPlate.NumberOfPlates;
					FiMn = ConstNum.FIOMEGA0_9N * Fcr * Zgross;
					Reporting.AddLine(ConstString.PHI + "Mn = " + ConstString.FIOMEGA0_9 + " * Fcr * Z = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + Zgross);
				}
				else
				{
					Reporting.AddLine("Fy² - 3 * fv² = " + webPlate.Material.Fy + "² - 3 * " + fshear + "² = " + (Math.Pow(webPlate.Material.Fy, 2) - 3 * Math.Pow(fshear, 2)) + ConstUnit.Stress);
					Reporting.AddLine("Fcr = 0 " + ConstUnit.Stress);
				}
				if (FiMn >= M_Req)
					Reporting.AddCapacityLine("= " + FiMn + " >= " + M_Req + ConstUnit.MomentUnitInch + " (OK)", M_Req / FiMn, "Required Strength (M_Req)", memberType);
				else
					Reporting.AddCapacityLine("= " + FiMn + " << " + M_Req + ConstUnit.MomentUnitInch + " (NG)", M_Req / FiMn, "Required Strength (M_Req)", memberType);
			}
			else
			{
				Reporting.AddHeader("Check Shear Yielding, Buckling and Yielding due to Flexure");
				Reporting.AddLine("Mn = Fy * Z = " + webPlate.Material.Fy + " * " + (webPlate.Thickness * (Math.Pow(webPlate.Length, 2) / 4) + " = " + Mn) + ConstUnit.MomentUnitInch);
				Reporting.AddLine("Mc = " + ConstString.FIOMEGA0_9 + " * " + Mn + " = " + Mc + ConstUnit.MomentUnitInch);
				Reporting.AddLine(string.Empty);
				Reporting.AddLine("Vn = 0.6 * Fy * Ag = 0.6 * " + webPlate.Material.Fy + " * " + (webPlate.Thickness * webPlate.Length) + " = " + Vn + ConstUnit.Force);
				Reporting.AddLine("Vc = " + ConstString.FIOMEGA1_0 + " * " + Vn + " = " + Vc + ConstUnit.Force);
				Reporting.AddLine(string.Empty);
				Reporting.AddLine("Vr = " + V + ConstUnit.Force);
				Reporting.AddLine("Mr = Vr * e = " + V + " * " + eccentricity + " = " + Mr + ConstUnit.MomentUnitInch);

				Reporting.AddLine(string.Empty);
				double result = Math.Pow(V / Vc, 2) + Math.Pow(Mr / Mc, 2);

				if (result <= 1.0)
					Reporting.AddCapacityLine("(Vr/Vc)² + (Mr/Mc)² = (" + V + " / " + Vc + ")² + (" + Mr + " / " + Mc + ")² = " + result + " <= 1.0 (OK)", result, "Check Shear Yielding, Buckling and Yielding due to Flexure", memberType);
				else
					Reporting.AddCapacityLine("(Vr/Vc)² + (Mr/Mc)² = (" + V + " / " + Vc + ")² + (" + Mr + " / " + Mc + ")² = " + result + " >> 1.0 (NG)", result, "Check Shear Yielding, Buckling and Yielding due to Flexure", memberType);
			}
			Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Shear Strength Based on Bending of the Plate:");

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
				Reporting.AddLine("Assume inflection point at midpoint between beams");
			else if ((CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn) && webPlate.Eccentricity == EWebEccentricity.Descon)
				Reporting.AddLine("Assume inflection point at midpoint between bolt line and weld line.");

			n = webPlate.Bolt.NumberOfRows;
			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
			{
				ecg = beam.EndSetback + beam.WinConnect.Beam.Lh;
				ecn = beam.EndSetback + beam.WinConnect.Beam.Lh;
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
			{
				if (webPlate.ExtendedConfiguration && beam.Moment == 0)
				{
					ecg = (primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh;
					ecn = (primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh;
				}
				else
				{
					ecg = beam.EndSetback + beam.WinConnect.Beam.Lh;
					ecn = beam.EndSetback + beam.WinConnect.Beam.Lh;
				}

			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
			{
				if (webPlate.Eccentricity == EWebEccentricity.AISC)
				{
					ecg = 0;
					ecn = 0;
				}
				else
				{
					ecg = (beam.EndSetback + beam.WinConnect.Beam.Lh) / 2;
					ecn = (beam.EndSetback + beam.WinConnect.Beam.Lh) / 2;
				}
			}
			else
			{
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder && webPlate.ExtendedConfiguration)
				{
					ecg = (primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh;
					ecn = (primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh;
				}
				else
				{
					ecg = beam.EndSetback + beam.WinConnect.Beam.Lh;
					ecn = beam.EndSetback + beam.WinConnect.Beam.Lh;
				}
			}

			ecg = eccforbending;
			ecn = eccforbending;
			beam.WinConnect.Fema.L = webPlate.Length;
			d = webPlate.Bolt.HoleWidth;
			n = webPlate.Bolt.NumberOfRows;
			s = webPlate.Bolt.SpacingLongDir;

			Zgross = Math.Pow(beam.WinConnect.Fema.L, 2) / 4 * webPlate.Thickness * webPlate.NumberOfPlates;
			if (n == ((int) Math.Floor(n / 2)) * 2)
				Znet = (Zgross - 2 * (Math.Pow(n, 2) / 8 * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * webPlate.Bolt.SpacingLongDir) * webPlate.Thickness * webPlate.NumberOfPlates);
			else
				Znet = (Zgross - 2 * ((Math.Pow(n, 2) - 1) / 8 * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * webPlate.Bolt.SpacingLongDir * webPlate.Thickness) * webPlate.NumberOfPlates);

			if (ecg == 0 && ecn == 0)
				Reporting.AddLine("Plate bending neglected.");

			beam.WinConnect.Fema.L = webPlate.Length;
			d = webPlate.Bolt.HoleWidth;
			n = boltNumberOfRows;
			s = webPlate.Bolt.SpacingLongDir;
			nn = n / 2;
			n1 = n + 1;
			if (((int) Math.Floor(n / 2)) * 2 == n)
				SnetDeduction = (4 * Math.Pow(nn, 3) / 3 + 2 * nn / 3 - nn) * Math.Pow(s, 2) * (d + ConstNum.SIXTEENTH_INCH) * webPlate.Thickness / beam.WinConnect.Fema.L;
			else
				SnetDeduction = (Math.Pow(n1, 3) / 2 - 3 * Math.Pow(n1, 2) / 2 + n1) * Math.Pow(s, 2) * (d + ConstNum.SIXTEENTH_INCH) * webPlate.Thickness / (3 * beam.WinConnect.Fema.L);
			SnetforBuckling = webPlate.Thickness * Math.Pow(beam.WinConnect.Fema.L, 2) / 6 - SnetDeduction;

			if (ecn > 0)
			{
				Reporting.AddHeader("Flexural Rupture:");
				Reporting.AddLine("Net Section Modulus (Znet) = " + Znet + ConstUnit.SecMod + ",  Eccentricity (e) = " + ecn + ConstUnit.Length);
				FiRn = ConstNum.FIOMEGA0_75N * Znet * webPlate.Material.Fu / ecn;

				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Shear Strength = " + ConstString.PHI + " * Znet * Fu / e = " + ConstString.FIOMEGA0_75 + " * " + Znet + " * " + webPlate.Material.Fu + " / " + ecn);
				if (FiRn >= V)
					Reporting.AddCapacityLine("= " + FiRn + " >= " + V + ConstUnit.Force + " (OK)", V / FiRn, "Flexural Rupture", memberType);
				else
					Reporting.AddCapacityLine("= " + FiRn + " << " + V + ConstUnit.Force + " (NG)", V / FiRn, "Flexural Rupture", memberType);
			}
			
			Reporting.AddHeader("Check Plate Buckling:");
			cp = webPlate.Width - webPlate.Bolt.EdgeDistTransvDir - webPlate.Bolt.SpacingTransvDir * (webPlate.Bolt.NumberOfLines - 1); // eccforbending
			h0 = webPlate.Length;

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && beam.Moment == 0)
				cp = webPlate.Width - webPlate.Bolt.EdgeDistTransvDir - webPlate.Bolt.SpacingTransvDir * (webPlate.Bolt.NumberOfLines - 1); // eccforbending
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && beam.Moment > 0)
				cp = webPlate.Width - (primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 - webPlate.Bolt.EdgeDistTransvDir - webPlate.Bolt.SpacingTransvDir * (webPlate.Bolt.NumberOfLines - 1); // eccforbending

			Reporting.AddLine("c = " + cp + ConstUnit.Length);
			Reporting.AddLine("h0 = " + h0 + ConstUnit.Length);

			Lambda = h0 * Math.Pow(webPlate.Material.Fy, 0.5) / (10 * webPlate.Thickness * Math.Pow(475 + 280 * Math.Pow(h0 / cp, 2), 0.5));
			Reporting.AddLine("Lambda = h0 * Fy^0.5 / (10 * t * (475 + 280 * (h0 / cp)²)^0.5)");
			Reporting.AddLine("= " + h0 + " * " + webPlate.Material.Fy + "^0.5 / (10 * " + webPlate.Thickness + " * (475 + 280 * (" + h0 + " / " + cp + ")²)^0.5)");
			Reporting.AddLine("= " + Lambda);
			if (Lambda <= 0.7)
			{
				Q = 1;
				Reporting.AddLine("Q = 1");
			}
			else if (Lambda <= 1.41)
			{
				Q = 1.34 - 0.486 * Lambda;
				Reporting.AddLine("Q = 1.34 - 0.486 * Lambda = 1.34 - 0.486 * " + Lambda + " = " + Q);
			}
			else
			{
				Q = 1.3 / Math.Pow(Lambda, 2);
				Reporting.AddLine("Q = 1.3 / (Lambda²) = 1.3 / (" + Lambda + "²) = " + Q);
			}
			Fcr = webPlate.Material.Fy * Q;
			FiFcr = ConstNum.FIOMEGA0_9N * Fcr;
			Reporting.AddLine(ConstString.PHI + "Fcr = " + ConstString.FIOMEGA0_9 + " * Fy * Q = " + ConstString.FIOMEGA0_9 + " * " + webPlate.Material.Fy + " * " + Q + " = " + FiFcr + ConstUnit.Stress);

			Reporting.AddHeader("Buckling Strength:");
			beam.WinConnect.Fema.L = webPlate.Length;
			d = webPlate.Bolt.HoleWidth;
			n = boltNumberOfRows;
			s = webPlate.Bolt.SpacingLongDir;
			nn = n / 2;
			n1 = n + 1;
			if (((int) Math.Floor(n / 2)) * 2 == n)
				SnetDeduction = (4 * Math.Pow(nn, 3) / 3 + 2 * nn / 3 - nn) * Math.Pow(s, 2) * (d + ConstNum.SIXTEENTH_INCH) * webPlate.Thickness / beam.WinConnect.Fema.L;
			else
				SnetDeduction = (Math.Pow(n1, 3) / 2 - 3 * Math.Pow(n1, 2) / 2 + n1) * Math.Pow(s, 2) * (d + ConstNum.SIXTEENTH_INCH) * webPlate.Thickness / (3 * beam.WinConnect.Fema.L);
			SnetforBuckling = webPlate.Thickness * Math.Pow(beam.WinConnect.Fema.L, 2) / 6 - SnetDeduction;

			Ru = FiFcr * SnetforBuckling / cp;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.PHI + "Fcr * Snet / c = " + FiFcr + " * " + SnetforBuckling + " / " + cp);
			if (Ru >= V / webPlate.NumberOfPlates)
				Reporting.AddCapacityLine("= " + Ru + " >= " + (V / webPlate.NumberOfPlates) + ConstUnit.Force + " (OK)", V / Ru, "Buckling Strength", memberType);
			else
				Reporting.AddCapacityLine("= " + Ru + " << " + (V / webPlate.NumberOfPlates) + ConstUnit.Force + " (NG)", V / Ru, "Buckling Strength", memberType);

			Reporting.AddHeader("Bolt Bearing on Plate:");
			Fbe = CommonCalculations.EdgeBearing(webPlate.Bolt.EdgeDistLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Material.Fu, webPlate.Bolt.HoleType, true);
			Fbs = CommonCalculations.SpacingBearing(webPlate.Bolt.SpacingLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Bolt.HoleType, webPlate.Material.Fu, true);
			EquivBoltFactor = c / (webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows);
			if (EquivBoltFactor > 1)
				EquivBoltFactor = 1;
			
			BearingCap = (EquivBoltFactor * webPlate.Bolt.NumberOfLines * (Fbe + Fbs * (webPlate.Bolt.NumberOfRows - 1)) * webPlate.Thickness * webPlate.NumberOfPlates);
			Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = nL * (Fbe + Fbs * (nR - 1)) * t * Npl * ef");
			Reporting.AddLine("= " + webPlate.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + webPlate.Bolt.NumberOfRows + " - 1)) * " + webPlate.Thickness + " * " + webPlate.NumberOfPlates + " * " + EquivBoltFactor);

			if (BearingCap >= V)
				Reporting.AddCapacityLine("= " + BearingCap + " >= " + V + ConstUnit.Force + " (OK)", V / BearingCap, "Bolt Bearing on Plate", memberType);
			else
				Reporting.AddCapacityLine("= " + BearingCap + " << " + V + ConstUnit.Force + " (NG)", V / BearingCap, "Bolt Bearing on Plate", memberType);

			Reporting.AddHeader("Bolt Bearing on Beam Web:");
			Fbs = CommonCalculations.SpacingBearing(webPlate.Bolt.SpacingLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Bolt.HoleType, beam.Material.Fu, true);
			if (beam.WinConnect.Beam.TCopeD > 0)
			{
				Fbe = CommonCalculations.EdgeBearing(beam.WinConnect.Beam.DistanceToFirstBolt, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, beam.Material.Fu, webPlate.Bolt.HoleType, true);
				EquivBoltFactor = c / (webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows);
				if (EquivBoltFactor > 1)
					EquivBoltFactor = 1;
				BearingCap = EquivBoltFactor * webPlate.Bolt.NumberOfLines * (Fbe + Fbs * (webPlate.Bolt.NumberOfRows - 1)) * beam.Shape.tw;
				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = nL * (Fbe + Fbs * (nR - 1)) * t * ef");
				Reporting.AddLine("= " + webPlate.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + webPlate.Bolt.NumberOfRows + " - 1)) * " + beam.Shape.tw + " * " + EquivBoltFactor);
			}
			else
			{
				Fbe = Fbs;
				EquivBoltFactor = c / (webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows);
				if (EquivBoltFactor > 1)
					EquivBoltFactor = 1;
				BearingCap = EquivBoltFactor * webPlate.Bolt.NumberOfLines * Fbs * webPlate.Bolt.NumberOfRows * beam.Shape.tw;
				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = nL * Fbs * nR * t * ef");
				Reporting.AddLine("= " + webPlate.Bolt.NumberOfLines + " * " + Fbs + " * " + webPlate.Bolt.NumberOfRows + " * " + beam.Shape.tw + " * " + EquivBoltFactor);
			}

			if (BearingCap >= V)
				Reporting.AddCapacityLine("= " + BearingCap + " >= " + V + ConstUnit.Force + " (OK)", V / BearingCap, "Bolt Bearing on Plate", memberType);
			else
				Reporting.AddCapacityLine("= " + BearingCap + " << " + V + ConstUnit.Force + " (NG)", V / BearingCap, "Bolt Bearing on Plate", memberType);

			if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice)
			{
				Reporting.AddHeader("Weld Strength:");
				Mfws = CommonCalculations.MinimumWeld(SupThickness1, webPlate.Thickness);
				if (webPlate.SupportWeldSize >= Mfws)
					Reporting.AddLine("Weld Size (w) = " + CommonCalculations.WeldSize(webPlate.SupportWeldSize) + " >= Minimum Weld, " + CommonCalculations.WeldSize(Mfws) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size (w) = " + CommonCalculations.WeldSize(webPlate.SupportWeldSize) + " << Minimum Weld, " + CommonCalculations.WeldSize(Mfws) + ConstUnit.Length + " (NG)");

				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && beam.Moment > 0)
				{
					weldl = webPlate.h2 - webPlate.Clip;
					ew = 0;
				}
				else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange ||
				         CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn ||
				         CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
				{
					weldl = webPlate.Length;
					if (webPlate.Eccentricity == EWebEccentricity.AISC)
						ew = 0;
					else
						ew = (beam.EndSetback + beam.WinConnect.Beam.Lh) / (2 * weldl);
				}
				else 
				{
					// ConTypeIndex = 3
					ew = 0;
					if (webPlate.WebPlateStiffener == EWebPlateStiffener.With)
						weldl = webPlate.Height - webPlate.Clip;
					else
						weldl = webPlate.Length - webPlate.Clip;
				}

				c = EccentricWeld.GetEccentricWeld(0, ew, 0, true);
				//EccentricWeld.GetEccentricWeld(0, ew, ref c, 0, true);
				C1 = CommonCalculations.WeldTypeRatio();
				w_usefulS = SupFu * SupThickness / (0.707 * webPlate.Weld.Fexx);
				Reporting.AddLine("Maximum useful weld size for support thickness:");
				Reporting.AddLine("= Fu * t_eff / (0.707 * Fexx) ");
				Reporting.AddLine("= " + SupFu + " * " + SupThickness + " / (0.707 * " + webPlate.Weld.Fexx + ")");
				Reporting.AddLine("= " + w_usefulS + ConstUnit.Length);

				w_usefulP = webPlate.Material.Fu * webPlate.Thickness / (2 * 0.707 * webPlate.Weld.Fexx);
				Reporting.AddHeader("Maximum useful weld size for plate thickness:");
				Reporting.AddLine("= Fu * tp / (2 * 0.707 * Fexx) ");
				Reporting.AddLine("= " + webPlate.Material.Fu + " * " + webPlate.Thickness + " / (2 * 0.707 * " + webPlate.Weld.Fexx + ")");
				Reporting.AddLine("= " + w_usefulP + ConstUnit.Length);

				w_useful = Math.Min(w_usefulP, w_usefulS);
				if (w_useful < webPlate.SupportWeldSize)
				{
					Reporting.AddLine(w_useful + " << " + webPlate.SupportWeldSize + ConstUnit.Length);
					Reporting.AddLine("Use " + w_useful + ConstUnit.Length + " for strength calculation.");
					Reporting.AddLine(string.Empty);
				}
				else
					Reporting.AddLine(" " + w_useful + " >= " + webPlate.SupportWeldSize + ConstUnit.Length + " (OK)");

				if (!CommonDataStatic.IsFema && beam.MomentConnection == EMomentCarriedBy.NoMoment)
				{
					w_req = webPlate.Material.Fu * webPlate.Thickness / (2 * 0.707 * webPlate.Weld.Fexx);
					Reporting.AddHeader("Weld size req. to develop " + ConstString.DES_OR_ALLOWABLE + " shear strength of plate:");
					Reporting.AddLine("w_Req = Fu * t / (2 * 0.707 * Fexx) ");
					Reporting.AddLine("= " + webPlate.Material.Fu + " * " + webPlate.Thickness + " / (2 * 0.707 * " + webPlate.Weld.Fexx + ")");
					if (w_req <= webPlate.SupportWeldSize)
						Reporting.AddLine("= " + w_req + " <= " + webPlate.SupportWeldSize + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + w_req + " >> " + webPlate.SupportWeldSize + ConstUnit.Length + " (NG)");
				}

				FiRn = 2 * c * C1 * Math.Min(w_useful, webPlate.SupportWeldSize) * 16 * weldl;
				Reporting.AddLine(ConstString.PHI + " Rn = 2 * C * C1 * D * L = 2 * " + c + " * " + C1 + " * " + ((Math.Min(w_useful, webPlate.SupportWeldSize)) * 16) + " * " + weldl);
				if (FiRn >= V)
					Reporting.AddCapacityLine("= " + FiRn + " >= " + V + ConstUnit.Force + " (OK)", V / FiRn, "Weld size req. to develop ", memberType);
				else
					Reporting.AddCapacityLine("= " + FiRn + " << " + V + ConstUnit.Force + " (NG)", V / FiRn, "Weld size req. to develop ", memberType);

				if (CommonDataStatic.IsFema && (beam.WinConnect.Fema.Connection == EFemaConnectionType.RBS || beam.WinConnect.Fema.Connection == EFemaConnectionType.WUFB))
				{
					Reporting.AddHeader("FEMA-350 Requirement:");
					switch (beam.WinConnect.Fema.Connection)
					{
						case EFemaConnectionType.RBS:
							if (webPlate.SupportWeldSize >= 0.75 * webPlate.Thickness)
								Reporting.AddLine("Weld Size = " + webPlate.SupportWeldSize + " >=  0.75 * tpl = " + 0.75 * webPlate.Thickness + ConstUnit.Length + " (OK)");
							else
								Reporting.AddLine("Weld Size = " + webPlate.SupportWeldSize + " <<  0.75 * tpl = " + 0.75 * webPlate.Thickness + ConstUnit.Length + " (OK)");
							break;
						case EFemaConnectionType.WUFB:
							Reporting.AddHeader("Weld to develop full shear strength of plate:");
							w_req = webPlate.Material.Fu * webPlate.Thickness / (2 * 0.707 * webPlate.Weld.Fexx);
							Reporting.AddLine("w_req = Fu * t / (2 * 0.707 * Fexx)");
							Reporting.AddLine(" = " + webPlate.Material.Fu + " * " + webPlate.Thickness + " / (2 * 0.707 * " + webPlate.Weld.Fexx + ")");
							if (w_req <= webPlate.SupportWeldSize)
								Reporting.AddLine(" = " + w_req + " <= " + webPlate.SupportWeldSize + ConstUnit.Length + " (OK)");
							else
								Reporting.AddLine(" = " + w_req + " >> " + webPlate.SupportWeldSize + ConstUnit.Length + " (OK)");
							break;
					}
				}
			}
			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && beam.Moment > 0)
			{
				Reporting.AddHeader("Shear PL to Mom. Conn. Plate Weld:");
				if (webPlate.Eccentricity == EWebEccentricity.AISC)
					EccTotal = beam.EndSetback + beam.WinConnect.Beam.Lh + (webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir / 2;
				else
					EccTotal = (primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh + (webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir / 2;

				Reporting.AddLine("Vertical forces at support weld and bolt group are assumed");
				Reporting.AddLine("concentric. The couple generated by these forces will");
				Reporting.AddLine("be resisted by horizontal forces at top and bottom welds.");

				Reporting.AddLine("Eccentricity (ec) = " + EccTotal + ConstUnit.Length);
				Reporting.AddLine("Fh = V * ec / H = " + V + " * " + EccTotal + " / " + webPlate.h2 + " = " + CommonDataStatic.F_Shear + ConstUnit.Force);

				if (beam.MomentConnection == EMomentCarriedBy.DirectlyWelded)
					PlThickness = Math.Max(beam.WinConnect.MomentDirectWeld.Top.ExtensionThickness, beam.WinConnect.MomentDirectWeld.Bottom.ExtensionThickness);
				else
					PlThickness = Math.Max(beam.WinConnect.MomentDirectWeld.Top.FlangeThickness, beam.WinConnect.MomentDirectWeld.Bottom.FlangeThickness);
				
				minweld = CommonCalculations.MinimumWeld(webPlate.Thickness, PlThickness);
				if (webPlate.BeamWeldSize >= minweld)
					Reporting.AddLine("Weld Size = " + webPlate.BeamWeldSize + " >= " + minweld + ConstUnit.Length + " minimum (OK)");
				else
					Reporting.AddLine("Weld Size = " + webPlate.BeamWeldSize + " << " + minweld + ConstUnit.Length + " minimum (NG)");
				beam.WinConnect.Fema.L = ((primaryMember.Shape.bf - primaryMember.Shape.tw) / 2 - 2 * webPlate.BeamWeldSize);
				Cap = (2 * ConstNum.FIOMEGA0_75N * 0.4242 * webPlate.Weld.Fexx * webPlate.BeamWeldSize * beam.WinConnect.Fema.L);
				Reporting.AddLine("Weld Capacity = 2 * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx * w * L");
				Reporting.AddLine("= 2 * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + webPlate.Weld.Fexx + " * " + webPlate.BeamWeldSize + " * " + beam.WinConnect.Fema.L);
				if (Cap >= CommonDataStatic.F_Shear)
					Reporting.AddCapacityLine("= " + Cap + " >= " + CommonDataStatic.F_Shear + ConstUnit.Force + " (OK)", CommonDataStatic.F_Shear / Cap, "Weld Capacity", memberType);
				else
					Reporting.AddCapacityLine("= " + Cap + " << " + CommonDataStatic.F_Shear + ConstUnit.Force + " (NG)", CommonDataStatic.F_Shear / Cap, "Weld Capacity", memberType);
			}
		}

		private static void CalculateEccentricity(EMemberType memberType, ref int Nvl, ref double eccent, ref double eccforbending)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			var shearWebPlate = beam.WinConnect.ShearWebPlate;

			// This block was stolen from DesignSinglePlateShear() below, but is needed for all methods that call this
			// method.
			double Minedgedist, MinED;
			if (shearWebPlate.Bolt.HoleLength > shearWebPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH)
				Minedgedist = shearWebPlate.Bolt.MinEdgeSheared + shearWebPlate.Bolt.Eincr;
			else
				Minedgedist = shearWebPlate.Bolt.MinEdgeSheared;
			MinED = Math.Max(Minedgedist, 2 * shearWebPlate.Bolt.BoltSize);

			if (shearWebPlate.Bolt.EdgeDistTransvDir < MinED)
				shearWebPlate.Bolt.EdgeDistTransvDir = MinED;
			if (shearWebPlate.Bolt.HoleLength > shearWebPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH)
				Minedgedist = shearWebPlate.Bolt.MinEdgeSheared + shearWebPlate.Bolt.Eincr;
			else
				Minedgedist = shearWebPlate.Bolt.MinEdgeSheared;
			MinED = Math.Max(Minedgedist, 2 * shearWebPlate.Bolt.BoltSize);

			if (!beam.WinConnect.Beam.Lh_User && beam.WinConnect.Beam.Lh < MinED)
				beam.WinConnect.Beam.Lh = MinED;
			// End block

			Nvl = shearWebPlate.Bolt.NumberOfLines;
			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BeamToColumnWeb:
					shearWebPlate.Width = (column.Shape.bf - column.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh + (Nvl - 1) * shearWebPlate.Bolt.SpacingTransvDir + shearWebPlate.Bolt.EdgeDistTransvDir;

					if (beam.Moment == 0 && shearWebPlate.WebPlateStiffener == EWebPlateStiffener.Without)
					{
						eccent = (column.Shape.bf - column.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh + (shearWebPlate.Bolt.NumberOfLines - 1) * shearWebPlate.Bolt.SpacingTransvDir / 2;
						eccforbending = (column.Shape.bf - column.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh;
					}
					else if (beam.Moment == 0 && shearWebPlate.WebPlateStiffener == EWebPlateStiffener.With)
					{
						eccforbending = beam.EndSetback + beam.WinConnect.Beam.Lh + (column.Shape.bf - column.Shape.tw) / 2;
						eccent = (column.Shape.bf - column.Shape.tw) / 2 + (beam.EndSetback + beam.WinConnect.Beam.Lh) + (shearWebPlate.Bolt.NumberOfLines - 1) * shearWebPlate.Bolt.SpacingTransvDir / 2;
					}
					else if (beam.Moment != 0 && shearWebPlate.WebPlateStiffener == EWebPlateStiffener.With)
					{
						eccforbending = beam.EndSetback + beam.WinConnect.Beam.Lh;
						eccent = 0;
						if (eccent <= ConstNum.THREE_INCHES)
							eccent = 0;
					}
					break;
				case EJointConfiguration.BeamToColumnFlange:
				case EJointConfiguration.BeamToHSSColumn:
					// Commented out by Mike (12/12/14) becuase it is interferring with necessary calcs
					//if (shearWebPlate.Eccentricity == EWebEccentricity.AISC && beam.M != 0)
					//{
					//	eccent = 0;
					//	eccforbending = 0;
					//}
					//else
					//{				
					shearWebPlate.Width = beam.EndSetback + beam.WinConnect.Beam.Lh + (Nvl - 1) * shearWebPlate.Bolt.SpacingTransvDir + shearWebPlate.Bolt.EdgeDistTransvDir;
					if (beam.Moment == 0 && shearWebPlate.WebPlateStiffener == EWebPlateStiffener.Without)
					{
						// without stiffener No Moment
						eccent = beam.EndSetback + beam.WinConnect.Beam.Lh + (shearWebPlate.Bolt.NumberOfLines - 1) * shearWebPlate.Bolt.SpacingTransvDir / 2;
						eccforbending = beam.EndSetback + beam.WinConnect.Beam.Lh; // / 2
					}
					else if (beam.Moment != 0 && shearWebPlate.WebPlateStiffener == EWebPlateStiffener.Without)
					{
						// without stiff With Moment
						eccent = (beam.EndSetback + beam.WinConnect.Beam.Lh + (shearWebPlate.Bolt.NumberOfLines - 1) * shearWebPlate.Bolt.SpacingTransvDir / 2) / 2;
						eccforbending = (beam.EndSetback + beam.WinConnect.Beam.Lh) / 2;
					}
					break;
				case EJointConfiguration.BeamToGirder:
					if (beam.Moment != 0)
						shearWebPlate.Width = beam.EndSetback + beam.WinConnect.Beam.Lh + (Nvl - 1) * shearWebPlate.Bolt.SpacingTransvDir + shearWebPlate.Bolt.EdgeDistTransvDir;

					if (beam.Moment == 0 && shearWebPlate.WebPlateStiffener == EWebPlateStiffener.Without)
					{
						eccent = column.Shape.tw / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh + (shearWebPlate.Bolt.NumberOfLines - 1) * shearWebPlate.Bolt.SpacingTransvDir / 2;
						eccforbending = column.Shape.tw / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh;
					}
					else if (beam.Moment == 0 && shearWebPlate.WebPlateStiffener == EWebPlateStiffener.With)
					{
						eccent = column.Shape.tw / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh + (shearWebPlate.Bolt.NumberOfLines - 1) * shearWebPlate.Bolt.SpacingTransvDir / 2;
						eccforbending = column.Shape.tw / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh;
					}
					else if (beam.Moment != 0 && shearWebPlate.WebPlateStiffener == EWebPlateStiffener.With)
					{
						eccent = (column.Shape.bf - column.Shape.tw) / 2 + beam.EndSetback + beam.WinConnect.Beam.Lh + (shearWebPlate.Bolt.NumberOfLines - 1) * shearWebPlate.Bolt.SpacingTransvDir / 2;
						//eccent = eccforbending; // eccforbending is always 0 here
						if (eccent <= ConstNum.THREE_INCHES)
							eccent = 0;
					}
					else if (beam.Moment != 0 && shearWebPlate.WebPlateStiffener == EWebPlateStiffener.Without)
					{
						eccent = (column.Shape.bf - column.Shape.tw) / 2 + (beam.EndSetback + beam.WinConnect.Beam.Lh) + (shearWebPlate.Bolt.NumberOfLines - 1) * shearWebPlate.Bolt.SpacingTransvDir / 2;
						eccforbending = beam.EndSetback + beam.WinConnect.Beam.Lh;
					}
					break;
				case EJointConfiguration.BeamSplice:
					eccent = beam.EndSetback + beam.WinConnect.Beam.Lh + ((shearWebPlate.Bolt.NumberOfLines - 1) / 2.0) * shearWebPlate.Bolt.SpacingTransvDir;
					eccforbending = beam.EndSetback + beam.WinConnect.Beam.Lh;
					break;
			}
		}
	}
}