using System;
using System.Windows.Media.Effects;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class TeeShear
	{
		internal static void DesignTeeShear(EMemberType memberType, ref double MaxL, ref double MinL)
		{
			double Vn;
			double capacity;
			double WWeldCap = 0;
			double TearoutStrength;
			double er;
			double spStem;
			double spBeam;
			double BendingCap;
			double OslMoment;
			double OslWeldCap;
			double BearingCap;
			double FiRn;
			double a2;
			double a1;
			double d2;
			double d1;
			double ehmin;
			double spFlange;
			double spSupport;
			double R;
			double V;
			double db_min;
			double ts_min;
			string num = "";
			double OslBot = 0;
			double OslTop = 0;
			double WBot = 0;
			double WTop = 0;
			double BearingCap1 = 0;
			double Fbs = 0;
			double Fbe = 0;
			double HoleSize = 0;
			double w = 0;
			double BeamTearOutCap = 0;
			double TenCap = 0;
			double BsCap = 0;
			double Cap = 0;
			int MinBolts = 0;
			double b1;
			double rut = 0;
			double ruv = 0;
			double ecc = 0;
			double ta1;
			double Aw;
			double h;
			double BlockShearWithWeld;
			double PhiRn2;
			double PhiRn;
			double Lnv2;
			double Lgv = 0;
			double Lgt = 0;
			double Lnv = 0;
			double Lnt = 0;
			double Beamlv;
			double webd;
			double WeldCap = 0;
			double OslWeldCapacity;
			double RT;
			double C1 = 0;
			double w_min;
			double lfortension;
			double BF = 0;
			double TensionCap;
			double FiRn_perBolt = 0;
			double ta;
			double gmin;
			double g;
			double edg = 0;
			double B;
			double Gage;
			double Thick = 0;
			double AgReq = 0;
			double Anreq = 0;
			double Ag;
			double An = 0;
			double AnOsl = 0;
			double tReqforBlockShearofStem = 0;
			double tRequiredForFlexuralRupture;
			double tRequiredForFlexuralYielding;
			double s;
			double a;
			double Anw = 0;
			double t = 0;
			double ho;
			double c = 0;
			int i = 0;
			double ts_max;
			double E1 = 0;
			double FuT;
			double LengthOslW = 0;
			double tReqforBlockShearofFlange = 0;
			double LengthWW = 0;
			double tOslBReq = 0;
			double tWBreq = 0;
			double tReq = 0;
			int n = 0;
			double LengthOB = 0;
			double LengthWB = 0;
			bool errorcondition;
			double Rload;
			double rew;
			double seosl;
			double reOsl = 0;
			double oldeato = 0;
			double oldeatw = 0;
			double minboltdisttotop;
			double SupportFu = 0;
			double SupFy = 0;
			double SupThickness1 = 0;
			double SupThickness = 0;
			double mnweld;
			double MxWeld;
			bool RigidSupport;
			double sew;
			double evmin = 0;
			double rut_Applied = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

			var webTee = beam.WinConnect.ShearWebTee;

			if (webTee.OSLConnection == EConnectionStyle.Welded)
				RigidSupport = false;
			else
			{
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange)
					RigidSupport = true;
				else
					RigidSupport = false;
			}

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
			{
				SupThickness = column.Shape.tf;
				SupThickness1 = column.Shape.tf;
				SupFy = column.Material.Fy;
				SupportFu = column.Material.Fu;
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
			{
				if (beam.MemberType == EMemberType.LeftBeam)
				{
					MiscCalculationsWithReporting.EffectiveSupThickness(beam.MemberType, ref SupThickness, ref SupThickness1);
					//SupThickness = column.Shape.tw * Math.Abs(leftBeam.ShearForce) / (leftBeam.ShearForce) + Math.Abs(rightBeam.ShearForce);
					//SupThickness1 = column.Shape.tw;
				}
				else
				{
					MiscCalculationsWithReporting.EffectiveSupThickness(beam.MemberType, ref SupThickness, ref SupThickness1);
					//SupThickness = column.Shape.tw * Math.Abs(rightBeam.ShearForce) / (leftBeam.ShearForce) + Math.Abs(rightBeam.ShearForce);
					//SupThickness1 = column.Shape.tw;
				}
				SupFy = column.Material.Fy;
				SupportFu = column.Material.Fu;
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
			{
				if (beam.MemberType == EMemberType.LeftBeam)
				{
					MiscCalculationsWithReporting.EffectiveSupThickness(beam.MemberType, ref SupThickness, ref SupThickness1);
					//SupThickness = column.Shape.tw * Math.Abs(leftBeam.ShearForce) / (Math.Abs(leftBeam.ShearForce) + Math.Abs(rightBeam.ShearForce));
					//SupThickness1 = column.Shape.tw;
				}
				else
				{
					SupThickness = column.Shape.tw * Math.Abs(rightBeam.ShearForce) / (Math.Abs(leftBeam.ShearForce) + Math.Abs(rightBeam.ShearForce));
					SupThickness1 = column.Shape.tw;
				}
				SupFy = column.Material.Fy;
				SupportFu = column.Material.Fu;
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
			{
				SupThickness1 = webTee.Size.tf;
				SupThickness = SupThickness1;
				SupFy = webTee.Material.Fy;
				SupportFu = webTee.Material.Fu;
			}
			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder && webTee.BeamSideConnection == EConnectionStyle.Welded && webTee.OSLConnection == EConnectionStyle.Bolted && beam.WinConnect.Beam.TopCope && webTee.Position == EPosition.Top)
			{
				minboltdisttotop = beam.WinConnect.Beam.TCopeD + webTee.WeldSizeStem + ConstNum.SIXTEENTH_INCH + webTee.FlangeEdgeDistLongDir;
				if (beam.WinConnect.ShearClipAngle.BoltOslOnSupport.EdgeDistGusset < minboltdisttotop && !beam.WinConnect.ShearClipAngle.BoltOslOnSupport.EdgeDistGusset_User)
					beam.WinConnect.ShearClipAngle.BoltOslOnSupport.EdgeDistGusset = minboltdisttotop;
			}
			if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToGirder && webTee.BeamSideConnection == EConnectionStyle.Welded && webTee.OSLConnection == EConnectionStyle.Bolted && !beam.WinConnect.Beam.TopCope && webTee.Position == EPosition.Top)
			{
				minboltdisttotop = beam.Shape.kdet + webTee.WeldSizeStem + ConstNum.SIXTEENTH_INCH + webTee.FlangeEdgeDistLongDir;
				if (beam.WinConnect.ShearClipAngle.BoltOslOnSupport.EdgeDistGusset < minboltdisttotop && !beam.WinConnect.ShearClipAngle.BoltOslOnSupport.EdgeDistGusset_User)
					beam.WinConnect.ShearClipAngle.BoltOslOnSupport.EdgeDistGusset = minboltdisttotop;
			}

			if (webTee.StemSpacingLongDir == 0)
				webTee.StemSpacingLongDir = ConstNum.THREE_INCHES;

			oldeatw = webTee.BoltWebOnStem.EdgeDistTransvDir;
			oldeato = webTee.BoltOslOnFlange.EdgeDistTransvDir;
			MaxL = NumberFun.Round(MaxL, 8);

			if (webTee.OSLConnection == EConnectionStyle.Bolted)
			{
				reOsl = webTee.BoltOslOnFlange.MinEdgeRolled;
				seosl = webTee.BoltOslOnFlange.MinEdgeSheared;
				if (!webTee.BoltOslOnFlange.EdgeDistTransvDir_User)
					webTee.BoltOslOnFlange.EdgeDistTransvDir = reOsl;
				if (!webTee.BoltOslOnFlange.EdgeDistLongDir_User)
					webTee.FlangeEdgeDistLongDir = seosl;
			}

			if (beam.MomentConnection == EMomentCarriedBy.Tee)
				MinL = webTee.FlangeSpacingLongDir + 2 * webTee.FlangeEdgeDistLongDir;
			else
				MinL = NumberFun.Round(MinL, 8);

			if (webTee.BeamSideConnection == EConnectionStyle.Bolted)
			{
				rew = webTee.BoltWebOnStem.MinEdgeRolled;
				sew = webTee.BoltWebOnStem.MinEdgeSheared;
				if (rew > webTee.BoltWebOnStem.EdgeDistTransvDir)
					webTee.BoltWebOnStem.EdgeDistTransvDir = rew;
				if (!beam.WinConnect.Beam.Lh_User && beam.WinConnect.Beam.Lh < sew)
					beam.WinConnect.Beam.Lh = sew;
				if (webTee.StemEdgeDistLongDir < sew)
					webTee.StemEdgeDistLongDir = NumberFun.Round(sew, 4);
			}
			Rload = Math.Sqrt(Math.Pow(webTee.H, 2) + Math.Pow(webTee.V, 2));
			errorcondition = false;

			if (webTee.BeamSideConnection == EConnectionStyle.Bolted && webTee.OSLConnection == EConnectionStyle.Bolted)
			{
				GetGage(memberType);
				if (!webTee.BoltOslOnFlange.NumberOfBolts_User)
				{
					GetNumberofOslBolts(memberType, MinL, ref MinBolts, RigidSupport, ref ecc, ref ruv, ref rut, ref FiRn_perBolt);
					GetLengthforOslBolted(memberType, ref LengthOB, MinL, MaxL, ref errorcondition);
				}
				if (!webTee.BoltWebOnStem.NumberOfBolts_User)
				{
					GetNumberOfWBolts(memberType, MinL, ref MinBolts, MaxL, RigidSupport, ref ecc, Rload, ref n, ref c, ref Cap, ref BsCap, TenCap, ref BeamTearOutCap);
					GetLengthForWebBolted(memberType, ref evmin, ref LengthWB, MinL, MaxL, ref errorcondition);
				}
				if (errorcondition)
				{
					Reporting.AddLine("Could not find web tee. (NG)");
					return;
				}
				else
					webTee.SLength = Math.Max(LengthWB, LengthOB);
				if (webTee.SLength > LengthWB && !webTee.BoltWebOnStem.NumberOfBolts_User)
				{
					if (webTee.BoltStagger == EBoltStagger.None)
					{
						webTee.StemNumberOfRows = ((int)Math.Floor((webTee.SLength - 2 * webTee.StemEdgeDistLongDir) / webTee.StemSpacingLongDir)) + 1;
						webTee.StemEdgeDistLongDir = 0.5 * (webTee.SLength - (webTee.StemNumberOfRows - 1) * webTee.StemSpacingLongDir);
					}
					else
					{
						webTee.StemNumberOfRows = ((int)Math.Floor((webTee.SLength - webTee.StemSpacingLongDir / 2 - 2 * webTee.StemEdgeDistLongDir) / webTee.StemSpacingLongDir)) + 1;
						webTee.StemEdgeDistLongDir = 0.5 * (webTee.SLength - webTee.StemSpacingLongDir / 2 - (webTee.StemNumberOfRows - 1) * webTee.StemSpacingLongDir);
					}
				}
				if (webTee.SLength > LengthOB && !webTee.BoltOslOnFlange.NumberOfBolts_User)
				{
					if (webTee.BoltStagger == EBoltStagger.None)
					{
						n = ((int)Math.Floor((webTee.SLength - 2 * webTee.FlangeEdgeDistLongDir) / webTee.FlangeSpacingLongDir)) + 1;
						if (n > webTee.FlangeNumberOfRows && !webTee.BoltOslOnFlange.NumberOfBolts_User)
						{
							webTee.FlangeNumberOfRows = n;
							webTee.FlangeEdgeDistLongDir = 0.5 * (webTee.SLength - (webTee.FlangeNumberOfRows - 1) * webTee.FlangeSpacingLongDir);
						}
					}
					else
					{
						n = ((int)Math.Floor((webTee.SLength - webTee.FlangeSpacingLongDir / 2 - 2 * webTee.FlangeEdgeDistLongDir) / webTee.FlangeSpacingLongDir)) + 1;
						if (n > webTee.FlangeNumberOfRows && !webTee.BoltOslOnFlange.NumberOfBolts_User)
						{
							webTee.FlangeNumberOfRows = n;
							webTee.FlangeEdgeDistLongDir = 0.5 * (webTee.SLength - webTee.FlangeSpacingLongDir / 2 - (webTee.FlangeNumberOfRows - 1) * webTee.FlangeSpacingLongDir);
						}
					}
				}

				GetRequiredShearArea(memberType, ref AgReq, ref Anreq);
				GetRequiredThforOslBearing(memberType, ref HoleSize, ref Fbe, ref Fbs, ref BearingCap1, ref tOslBReq);
				GetRequiredThforWBearing(memberType, ref HoleSize, ref Fbe, ref Fbs, ref BearingCap1, ref tWBreq);
				tReq = Math.Max(tWBreq, tOslBReq);
			}
			else if (webTee.BeamSideConnection == EConnectionStyle.Welded && webTee.OSLConnection == EConnectionStyle.Bolted)
			{
				GetGage(memberType);
				webTee.StemNumberOfRows = 0;
				if (!webTee.BoltOslOnFlange.NumberOfBolts_User)
				{
					GetNumberofOslBolts(memberType, MinL, ref MinBolts, RigidSupport, ref ecc, ref ruv, ref rut, ref FiRn_perBolt);
					GetLengthforOslBolted(memberType, ref LengthOB, MinL, MaxL, ref errorcondition);
				}
				GetLengthforWebWelded(memberType, ref w, Rload, ref LengthWW, MaxL, ref errorcondition, MinL);
				if (errorcondition)
				{
					Reporting.AddLine("Could not find web tee. (NG)");
					return;
				}
				else
				{
					webTee.SLength = Math.Max(LengthWW, LengthOB);
					if (webTee.SLength > LengthOB)
					{
						n = (int)Math.Ceiling((webTee.SLength - 2 * webTee.FlangeEdgeDistLongDir) / webTee.FlangeSpacingLongDir) + 1;
						beam.WinConnect.Fema.L = (2 * webTee.FlangeEdgeDistLongDir + webTee.FlangeSpacingLongDir * (n - 1));
						if (beam.WinConnect.Fema.L <= MaxL && !webTee.BoltOslOnFlange.NumberOfBolts_User)
						{
							webTee.FlangeNumberOfRows = n;
							webTee.SLength = beam.WinConnect.Fema.L;
						}
						else
							webTee.SLength = LengthOB;
					}
				}
				GetRequiredShearArea(memberType, ref AgReq, ref Anreq);
				GetRequiredThforOslBearing(memberType, ref HoleSize, ref Fbe, ref Fbs, ref BearingCap1, ref tOslBReq);
				GetThicknessForBlockShear(memberType, reOsl, ref Lgt, ref Lnt, ref Lgv, ref Lnv, ref tReqforBlockShearofFlange, ref tReqforBlockShearofStem, webTee.Size.d, beam.EndSetback, true);

				tReq = Math.Max(tOslBReq, tReqforBlockShearofFlange);
			}
			else if (webTee.BeamSideConnection == EConnectionStyle.Bolted && webTee.OSLConnection == EConnectionStyle.Welded)
			{
				webTee.FlangeNumberOfRows = 0;
				if (!webTee.BoltWebOnStem.NumberOfBolts_User)
					GetNumberOfWBolts(memberType, MinL, ref MinBolts, MaxL, RigidSupport, ref ecc, Rload, ref n, ref c, ref Cap, ref BsCap, TenCap, ref BeamTearOutCap);
				
				GetLengthForWebBolted(memberType, ref evmin, ref LengthWB, MinL, MaxL, ref errorcondition);
				GetLengthforOslWelded(memberType, ref BF, SupThickness, SupFy, ref LengthOslW, MaxL, MinL);

				webTee.SLength = Math.Max(LengthWB, LengthOslW);
				if (webTee.SLength > LengthWB)
				{
					n = ((int) Math.Ceiling((webTee.SLength - 2 * webTee.StemEdgeDistLongDir) / webTee.StemSpacingLongDir)) + 1;
					beam.WinConnect.Fema.L = (2 * webTee.StemEdgeDistLongDir + webTee.StemSpacingLongDir * (n - 1));
					if (!webTee.BoltWebOnStem.NumberOfBolts_User && beam.WinConnect.Fema.L <= MaxL)
					{
						webTee.StemNumberOfRows = n;
						webTee.SLength = beam.WinConnect.Fema.L;
					}
					else if (!webTee.BoltWebOnStem.NumberOfBolts_User)
						webTee.SLength = LengthWB;
				}

				GetRequiredShearArea(memberType, ref AgReq, ref Anreq);
				GetRequiredThforWBearing(memberType, ref HoleSize, ref Fbe, ref Fbs, ref BearingCap1, ref tWBreq);
				tReq = tWBreq;
			}
			else if (webTee.BeamSideConnection == EConnectionStyle.Welded && webTee.OSLConnection == EConnectionStyle.Welded)
			{
				tReq = 0;
				webTee.StemNumberOfRows = 0;
				webTee.FlangeNumberOfRows = 0;
				GetLengthforWebWelded(memberType, ref w, Rload, ref LengthWW, MaxL, ref errorcondition, MinL);
				GetLengthforOslWelded(memberType, ref BF, SupThickness, SupFy, ref LengthOslW, MaxL, MinL);
				if (errorcondition)
				{
					Reporting.AddLine("Could not find web tee. (NG)");
					return;
				}
				else
					webTee.SLength = Math.Max(LengthWW, LengthOslW);
				GetRequiredShearArea(memberType, ref AgReq, ref Anreq);
			}
			if (webTee.BeamSideConnection == EConnectionStyle.Bolted)
			{
				rew = webTee.BoltWebOnStem.MinEdgeSheared;
				if (webTee.Size.tw == 0)
					FuT = webTee.Material.Fu * 0.5;
				else
					FuT = webTee.Material.Fu * webTee.Size.tw;
				E1 = MiscCalculationsWithReporting.MinimumEdgeDist(webTee.BoltWebOnStem.BoltSize, webTee.H / webTee.StemNumberOfRows, FuT, rew, (int)webTee.BoltWebOnStem.HoleLength, webTee.BoltWebOnStem.HoleType);

				if (E1 > webTee.BoltWebOnStem.EdgeDistTransvDir)
					webTee.BoltWebOnStem.EdgeDistTransvDir = E1;
				if (!beam.WinConnect.Beam.Lh_User && beam.WinConnect.Beam.Lh < webTee.BoltWebOnStem.MinEdgeSheared)
					beam.WinConnect.Beam.Lh = webTee.BoltWebOnStem.MinEdgeSheared;
				sew = webTee.BoltWebOnStem.MinEdgeSheared;
				if (webTee.StemEdgeDistLongDir < sew)
					webTee.StemEdgeDistLongDir = sew;
				ts_max = webTee.BoltWebOnStem.BoltSize / 2 + ConstNum.SIXTEENTH_INCH;
			}
			else
				ts_max = 100;

			foreach (var tee in CommonDataStatic.ShapesTee)
			{
				double tempEndSetback;
				Shape tempTee;

				if (!webTee.Size_User)
					tempTee = tee.Value.ShallowCopy();
				else
					tempTee = webTee.Size.ShallowCopy();

				tempEndSetback = tempTee.kdet;
				c = tempEndSetback + beam.WinConnect.Beam.Lh;
				ho = webTee.SLength;

				if (!webTee.Size_User && ts_max < tempTee.tw || MiscCalculationsWithReporting.LateralPlateBuckling(c, ho, webTee.Material.Fy, false) > tempTee.tw)
					continue;

				switch (webTee.BeamSideConnection)
				{
					case EConnectionStyle.Welded:
						if (!webTee.Size_User && tempTee.d < 3)
							continue;
						if (tempTee.tw >= ConstNum.QUARTER_INCH)
							MxWeld = tempTee.tw - ConstNum.SIXTEENTH_INCH;
						else
							MxWeld = tempTee.tw;
						mnweld = CommonCalculations.MinimumWeld(tempTee.tw, beam.Shape.tw);
						if (!webTee.Size_User && mnweld > MxWeld)
							continue;
						t = tempTee.tw;
						Anw = webTee.SLength * t;
						break;
					case EConnectionStyle.Bolted:
						if (!webTee.Size_User && tempTee.d < 4 || tempTee.d < tempEndSetback + beam.WinConnect.Beam.Lh + E1 || tempTee.tw > webTee.BoltWebOnStem.BoltSize / 2 + ConstNum.SIXTEENTH_INCH)
							continue;
						a = tempEndSetback + beam.WinConnect.Beam.Lh;
						n = webTee.StemNumberOfRows;
						s = webTee.StemSpacingLongDir;
						beam.WinConnect.Fema.L = webTee.SLength;

						tRequiredForFlexuralYielding = beam.ShearForce * 6 * a / (ConstNum.FIOMEGA0_9N * webTee.Material.Fy * Math.Pow(webTee.SLength, 2));
						if (!webTee.Size_User && tRequiredForFlexuralYielding > tempTee.tw)
							continue;
						tRequiredForFlexuralRupture = (6 * a * beam.ShearForce / (ConstNum.FIOMEGA0_75N * webTee.Material.Fu * (Math.Pow(beam.WinConnect.Fema.L, 2) - Math.Pow(s, 2) * n * (Math.Pow(n, 2) - 1) * (webTee.BoltWebOnStem.HoleWidth + ConstNum.SIXTEENTH_INCH) / beam.WinConnect.Fema.L)));
						if (!webTee.Size_User && tRequiredForFlexuralRupture > tempTee.tw)
							continue;
						t = tempTee.tw;
						Anw = (webTee.SLength - webTee.StemNumberOfRows * (webTee.BoltWebOnStem.HoleWidth + ConstNum.SIXTEENTH_INCH)) * t;
						break;
				}

				GetThicknessForBlockShear(memberType, reOsl, ref Lgt, ref Lnt, ref Lgv, ref Lnv, ref tReqforBlockShearofFlange, ref tReqforBlockShearofStem, tempTee.d, tempEndSetback, false);
				if (!webTee.Size_User && tReqforBlockShearofStem > tempTee.tw)
					continue;
				if (!webTee.Size_User && tReqforBlockShearofFlange > tempTee.tf)
					continue;
				switch (webTee.OSLConnection)
				{
					case EConnectionStyle.Welded:
						mnweld = CommonCalculations.MinimumWeld(tempTee.tf, SupThickness1);
						if (tempTee.tf >= ConstNum.QUARTER_INCH)
							MxWeld = tempTee.tf - ConstNum.SIXTEENTH_INCH;
						else
							MxWeld = tempTee.tf;
						if (!webTee.Size_User && mnweld > MxWeld)
							continue;
						t = tempTee.tf;
						AnOsl = webTee.SLength * t;
						break;
					case EConnectionStyle.Bolted:
						t = tempTee.tf;
						AnOsl = (webTee.SLength - webTee.FlangeNumberOfRows * (webTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH)) * t;
						break;
				}
				An = Math.Min(AnOsl, Anw);
				Ag = webTee.SLength * t;
				if (!webTee.Size_User && tempTee.tw < tReq || An < Anreq || Ag < AgReq)
					continue;

				Thick = tempTee.tw;
				if (webTee.OSLConnection == EConnectionStyle.Bolted)
				{
					Gage = beam.GageOnColumn;
					B = (Gage - tempTee.tw) / 2;
					if (!webTee.Size_User && 2 * (B + reOsl) + tempTee.tw > tempTee.bf)
						continue;

					edg = (tempTee.bf - Gage) / 2;
					webTee.BoltOslOnFlange.EdgeDistTransvDir = edg;

					if (webTee.BeamSideConnection == EConnectionStyle.Bolted)
					{
						g = tempTee.d - E1;
						if (webTee.BoltStagger == EBoltStagger.None)
							gmin = tempTee.kdet + webTee.BoltWebOnStem.BoltSize + ConstNum.HALF_INCH;
						else
							gmin = MiscCalculationDataMethods.StaggeredBoltGage(Math.Max(webTee.BoltWebOnStem.BoltSize, webTee.BoltOslOnFlange.BoltSize), (Math.Max(webTee.StemSpacingLongDir, webTee.FlangeSpacingLongDir) / 2), tempTee.tf);
						if (!beam.WinConnect.Beam.Lh_User && gmin > tempEndSetback + beam.WinConnect.Beam.Lh)
							beam.WinConnect.Beam.Lh = gmin - tempEndSetback;
						if (!webTee.Size_User && g < gmin)
							continue;
					}

					ta = MiscCalculationsWithReporting.HangerStrength(beam.MemberType, webTee.BoltOslOnFlange.HoleWidth, webTee.Material.Fu, tempTee.tf, edg, B, FiRn_perBolt, rut, false);
					if (ta < rut)
						continue;
					TensionCap = webTee.FlangeNumberOfRows * ta;
				}
				else
				{
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
					{
						MiscCalculationDataMethods.WebTeeSupportWeldType(beam.MemberType, ref BF, tempTee);
						if (!webTee.Size_User && 
							!((webTee.FarSideWeldIsFlare || webTee.FarSideWeldIsFillet) &&
						      (webTee.NearSideWeldIsFlare || webTee.NearSideWeldIsFillet)))
							continue;
					}
					else
					{
						webTee.FarSideWeldIsFlare = false;
						webTee.NearSideWeldIsFlare = false;
						webTee.NearSideWeldIsFillet = true;
						webTee.FarSideWeldIsFillet = true;
					}

					TensionCap = 2 * ConstNum.FIOMEGA0_9N * webTee.Material.Fy * webTee.SLength * Math.Pow(tempTee.tf, 2) / (4 * (tempTee.bf / 2 - tempTee.k1));
					if (TensionCap < webTee.H)
					{
						lfortension = NumberFun.Round(webTee.SLength / TensionCap * webTee.H, 8);
						if (lfortension > MaxL)
							webTee.SLength = MaxL;
						if (lfortension < MaxL)
						{
							if (webTee.BeamSideConnection == EConnectionStyle.Welded)
								webTee.SLength = lfortension;
							else
							{
								if (webTee.SLength < lfortension)
									webTee.SLength = lfortension;
							}

							if (webTee.BeamSideConnection == EConnectionStyle.Bolted && !webTee.BoltWebOnStem.NumberOfBolts_User)
							{
								if (lfortension < webTee.SLength + webTee.StemSpacingLongDir)
								{
									webTee.StemEdgeDistLongDir = webTee.StemEdgeDistLongDir + (lfortension - webTee.SLength) / 2;
									webTee.SLength = lfortension;
								}
								else
								{
									webTee.StemNumberOfRows++;
									webTee.SLength = lfortension;
									webTee.StemEdgeDistLongDir = (webTee.SLength - (webTee.StemNumberOfRows - 1) * webTee.StemSpacingLongDir) / 2;
								}
							}
							if (webTee.OSLConnection == EConnectionStyle.Bolted)
							{
								if (lfortension < webTee.SLength + webTee.FlangeSpacingLongDir)
								{
									webTee.FlangeEdgeDistLongDir = webTee.FlangeEdgeDistLongDir + (lfortension - webTee.SLength) / 2;
									webTee.SLength = lfortension;
								}
								else
								{
									webTee.FlangeNumberOfRows++;
									webTee.SLength = lfortension;
									webTee.FlangeEdgeDistLongDir = (webTee.SLength - (webTee.FlangeNumberOfRows - 1) * webTee.FlangeSpacingLongDir) / 2;
								}
							}
							TensionCap = ConstNum.FIOMEGA0_9N * webTee.Material.Fy * webTee.SLength * Math.Pow(tempTee.tf, 2) / (4 * (tempTee.bf / 2 - tempTee.k1));
						}
					}
					mnweld = CommonCalculations.MinimumWeld(tempTee.tf, SupThickness1);
					B = tempTee.bf / 2 - tempTee.k1;
					w_min = Math.Min(0.625 * tempTee.tw, 1.106 / CommonDataStatic.Preferences.DefaultElectrode.Fexx * (webTee.Material.Fy * Math.Pow(tempTee.tf, 2) / B) * (Math.Pow(B, 2) / Math.Pow(webTee.SLength, 2) + 2));
					mnweld = Math.Max(mnweld, w_min);
					C1 = CommonCalculations.WeldTypeRatio();
					if (!webTee.WeldSizeFlange_User)
						webTee.WeldSizeFlange = NumberFun.Round(Math.Max(mnweld, MiscCalculationsWithReporting.OslWeld(beam.MemberType, Rload / 2.0, false)), 16);
					if (webTee.FarSideWeldIsFillet || webTee.NearSideWeldIsFillet)
					{
						if (tempTee.tf >= ConstNum.QUARTER_INCH)
							MxWeld = tempTee.tf - ConstNum.SIXTEENTH_INCH;
						else
							MxWeld = tempTee.tf;
						if (!webTee.Size_User && webTee.WeldSizeFlange > MxWeld)
							continue;
					}

					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
					{
						if (beam.MemberType == EMemberType.LeftBeam)
						{
							if (rightBeam.IsActive && rightBeam.WinConnect.ShearWebTee.WeldSizeFlange == 0 && !rightBeam.WinConnect.ShearWebTee.WeldSizeFlange_User)
								rightBeam.WinConnect.ShearWebTee.WeldSizeFlange = ConstNum.EIGHTH_INCH;
							if (!rightBeam.IsActive && !rightBeam.WinConnect.ShearWebTee.WeldSizeFlange_User)
								rightBeam.WinConnect.ShearWebTee.WeldSizeFlange = 0;
							RT = 1.414 * SupportFu * SupThickness1 / ((webTee.WeldSizeFlange + rightBeam.WinConnect.ShearWebTee.WeldSizeFlange) * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
							if (!leftBeam.WinConnect.ShearWebTee.WeldSizeFlange_User)
								leftBeam.WinConnect.ShearWebTee.WeldSizeFlange = webTee.WeldSizeFlange;
						}
						else
						{
							if (leftBeam.IsActive && leftBeam.WinConnect.ShearWebTee.WeldSizeFlange == 0 && leftBeam.WinConnect.ShearWebTee.WeldSizeFlange_User)
								leftBeam.WinConnect.ShearWebTee.WeldSizeFlange = ConstNum.EIGHTH_INCH;
							if (!leftBeam.IsActive && !leftBeam.WinConnect.ShearWebTee.WeldSizeFlange_User)
								leftBeam.WinConnect.ShearWebTee.WeldSizeFlange = 0;
							RT = 1.414 * SupportFu * SupThickness1 / ((webTee.WeldSizeFlange + leftBeam.WinConnect.ShearWebTee.WeldSizeFlange) * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
							if (!rightBeam.WinConnect.ShearWebTee.WeldSizeFlange_User)
								rightBeam.WinConnect.ShearWebTee.WeldSizeFlange = webTee.WeldSizeFlange;
						}
					}
					else
						RT = 1.414 * SupportFu * SupThickness1 / (webTee.WeldSizeFlange * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					if (RT > 1)
						RT = 1;
					OslWeldCapacity = (RT * MiscCalculationsWithReporting.OslWeld(beam.MemberType, 0, false));

					if (OslWeldCapacity < Rload / 2)
					{
						if (webTee.SLength < MaxL - ConstNum.QUARTER_INCH)
							webTee.SLength = webTee.SLength + ConstNum.QUARTER_INCH;
						else if(!webTee.Size_User)
							continue;
					}
				}
				if (!webTee.Size_User && TensionCap < webTee.H)
					continue;
				if (webTee.BeamSideConnection == EConnectionStyle.Welded)
				{
					MiscCalculationsWithReporting.ClipWelds(beam.MemberType, ref WeldCap, true);

					webd = beam.Shape.d - beam.WinConnect.Beam.TCopeD - beam.WinConnect.Beam.BCopeD;

					if (beam.WinConnect.Beam.TopCope & beam.WinConnect.Beam.BottomCope)
					{
						Beamlv = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (webTee.FrontY + webTee.SLength / 2);
						Lnt = tempTee.d - tempEndSetback;
						Lnv = webTee.SLength + Beamlv;
						Lgt = Lnt;
						Lgv = webTee.SLength + Beamlv;
						Lnv2 = webd;

						PhiRn = MiscCalculationsWithReporting.BlockShearNew(beam.Material.Fu, Lnv, 1, Lnt, Lgv, beam.Material.Fy, beam.Shape.tw, 0, true);
						PhiRn2 = ConstNum.FIOMEGA0_75N * 0.6 * beam.Material.Fu * Lnv2 * beam.Shape.tw;
						BlockShearWithWeld = Math.Min(PhiRn, PhiRn2);
					}
					else if (beam.WinConnect.Beam.TopCope)
					{
						Beamlv = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (webTee.FrontY + webTee.SLength / 2);
						Lnt = tempTee.d - tempEndSetback;
						Lnv = webTee.SLength + Beamlv;
						Lgt = Lnt;
						Lgv = webTee.SLength + Beamlv;
						Lnv2 = webd;

						BlockShearWithWeld = MiscCalculationsWithReporting.BlockShearNew(beam.Material.Fu, Lnv, 1, Lnt, Lgv, beam.Material.Fy, beam.Shape.tw, 0, true);
					}
					else
					{
						h = beam.Shape.d - beam.Shape.tf - Math.Max(beam.WinConnect.Beam.BCopeD, beam.Shape.tf);
						Aw = beam.Shape.tw * webd;
						BlockShearWithWeld = MiscCalculationsWithReporting.GrossShearStrength(beam.Material.Fy, Aw, memberType, false);
					}

					if (WeldCap < Rload || BlockShearWithWeld < Rload)
					{
						if (webTee.SLength <= MaxL - ConstNum.QUARTER_INCH)
							webTee.SLength = webTee.SLength + ConstNum.QUARTER_INCH;
						else if(!webTee.Size_User)
							continue;
					}
				}

				webTee.Size = tempTee.ShallowCopy();

				if (!beam.EndSetback_User)
					beam.EndSetback = tempEndSetback;

				break;
			}

			if (webTee.OSLConnection == EConnectionStyle.Bolted && beam.P > 0)
			{
				ta1 = 0;
				if (i > -1)
				{
					MiscCalculationsWithReporting.OSLBoltsWithoutPlaneEccentricity(memberType, webTee.V, ((int)webTee.H), webTee.FlangeNumberOfRows, ((int)webTee.FlangeSpacingLongDir), ecc, ref ruv, ref rut);
					b1 = (webTee.Size.bf - webTee.Size.tw) / 2 - edg;
					ta1 = MiscCalculationsWithReporting.HangerStrength(beam.MemberType, webTee.BoltOslOnFlange.HoleWidth, webTee.Material.Fu, webTee.Size.tf, edg, b1, FiRn_perBolt, rut, true);
				}
				if (ta1 > 0 && ta1 < rut)
				{
					if (webTee.SLength + webTee.FlangeSpacingLongDir < MaxL && !webTee.BoltOslOnFlange.NumberOfBolts_User)
					{
						webTee.FlangeNumberOfRows++;
						webTee.SLength = webTee.SLength + webTee.FlangeSpacingLongDir;
						leftBeam.WinConnect.ShearWebTee.FLength = webTee.SLength;
					}
				}
			}
			else
			{
				if (webTee.BeamSideConnection == EConnectionStyle.Bolted)
					webTee.BoltWebOnStem.EdgeDistTransvDir = webTee.Size.d - beam.WinConnect.Beam.Lh - beam.EndSetback;
			}

			if (webTee.BeamSideConnection == EConnectionStyle.Bolted)
			{
				webTee.BoltWebOnStem.EdgeDistTransvDir = webTee.Size.d - beam.EndSetback - beam.WinConnect.Beam.Lh;
				leftBeam.WinConnect.ShearWebTee.BoltWebOnStem.EdgeDistBrace = beam.WinConnect.Beam.Lh;
				if (webTee.BoltStagger == EBoltStagger.None)
					webTee.StemEdgeDistLongDir = (webTee.SLength - (webTee.StemNumberOfRows - 1) * webTee.StemSpacingLongDir) / 2;
				else
					webTee.StemEdgeDistLongDir = (webTee.SLength - webTee.StemSpacingLongDir / 2 - (webTee.StemNumberOfRows - 1) * webTee.StemSpacingLongDir) / 2;
			}
			if (webTee.OSLConnection == EConnectionStyle.Bolted)
			{
				if (webTee.BoltStagger == EBoltStagger.None)
					webTee.FlangeEdgeDistLongDir = (webTee.SLength - (webTee.FlangeNumberOfRows - 1) * webTee.FlangeSpacingLongDir) / 2;
				else
					webTee.FlangeEdgeDistLongDir = (webTee.SLength - webTee.FlangeSpacingLongDir / 2 - (webTee.FlangeNumberOfRows - 1) * webTee.FlangeSpacingLongDir) / 2;
			}

			Reporting.AddHeader(new CommonLists().CompleteMemberList[beam.MemberType] + " - " + beam.ShapeName + " Shear Connection Using Tee");
			switch (webTee.BoltStagger)
			{
				case EBoltStagger.None:
					WTop = webTee.StemEdgeDistLongDir;
					WBot = webTee.StemEdgeDistLongDir;
					OslTop = webTee.FlangeEdgeDistLongDir;
					OslBot = webTee.FlangeEdgeDistLongDir;
					break;
				case EBoltStagger.Support:
					WTop = webTee.StemEdgeDistLongDir;
					WBot = webTee.StemEdgeDistLongDir + webTee.StemSpacingLongDir / 2;
					OslTop = webTee.FlangeEdgeDistLongDir + webTee.FlangeSpacingLongDir / 2;
					OslBot = webTee.FlangeEdgeDistLongDir;
					break;
				case EBoltStagger.Beam:
					WTop = webTee.StemEdgeDistLongDir + webTee.StemSpacingLongDir / 2;
					WBot = webTee.StemEdgeDistLongDir;
					OslTop = webTee.FlangeEdgeDistLongDir;
					OslBot = webTee.FlangeEdgeDistLongDir + webTee.FlangeSpacingLongDir / 2;
					break;
				case EBoltStagger.OneLessRow:
					if (webTee.BoltOslOnFlange.NumberOfRows == webTee.BoltWebOnStem.NumberOfRows - 1)
					{
						WTop = webTee.StemEdgeDistLongDir;
						WBot = webTee.StemEdgeDistLongDir;
						OslTop = webTee.FlangeEdgeDistLongDir + webTee.FlangeSpacingLongDir / 2;
						OslBot = webTee.FlangeEdgeDistLongDir + webTee.FlangeSpacingLongDir / 2;
					}
					else
					{
						WTop = webTee.StemEdgeDistLongDir + webTee.StemSpacingLongDir / 2;
						WBot = webTee.StemEdgeDistLongDir + webTee.StemSpacingLongDir / 2;
						OslTop = webTee.FlangeEdgeDistLongDir;
						OslBot = webTee.FlangeEdgeDistLongDir;
					}
					break;
			}

			// Commented out to avoid overwriting the correct Weld length in WebTeeSupportWeldType. These calculations shouldn't need to be
			// run a second time. (MT 5/19/15)
			//if (webTee.OSLConnection == EConnectionStyle.Welded && CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
			//{
			//	double argTemp1 = 0;
			//	MiscCalculationsWithReporting.WebTeeSupportWeldType(beam.MemberType, ref argTemp1, webTee.Size);
			//}

			Reporting.AddMainHeader(num + webTee.Size.Name + " X " + webTee.SLength + ConstUnit.Length);
			Reporting.AddLine("Tee Material: " + webTee.Material.Name);

			if (webTee.BeamSideConnection == EConnectionStyle.Bolted)
			{
				Reporting.AddHeader("Check Tee Stem Thickness:");
				ts_max = webTee.BoltWebOnStem.BoltSize / 2 + ConstNum.SIXTEENTH_INCH;
				if (ts_max >= webTee.Size.tw)
					Reporting.AddLine("ts_max = db / 2 + 1 / 16 = " + webTee.BoltWebOnStem.BoltSize + " / 2 + " + ConstNum.SIXTEENTH_INCH + " = " + ts_max + " >= " + webTee.Size.tw + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("ts_max = db / 2 + 1 / 16 = " + webTee.BoltWebOnStem.BoltSize + " / 2 + " + ConstNum.SIXTEENTH_INCH + " = " + ts_max + " << " + webTee.Size.tw + ConstUnit.Length + " (NG)");
			}

			ts_min = MiscCalculationsWithReporting.LateralPlateBuckling((beam.EndSetback + beam.WinConnect.Beam.Lh), webTee.SLength, webTee.Material.Fy, true);
			if (ts_min <= webTee.Size.tw)
				Reporting.AddLine("= " + ts_min + " <= " + webTee.Size.tw + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("= " + ts_min + " >> " + webTee.Size.tw + ConstUnit.Length + " (NG)");

			if (webTee.OSLConnection == EConnectionStyle.Bolted)
			{
				Reporting.AddHeader("Support Side Connection: " + (2 * webTee.FlangeNumberOfRows) + " Bolts " + webTee.BoltOslOnFlange.BoltName);
				Reporting.AddLine("Check Bolt Diameter:");
				B = beam.GageOnColumn / 2 - webTee.Size.k1;
				beam.WinConnect.Fema.L = webTee.SLength;
				db_min = Math.Min(0.163F * webTee.Size.tf * Math.Pow(webTee.Material.Fy / B * (Math.Pow(B, 2) / Math.Pow(beam.WinConnect.Fema.L, 2) + 2), 0.5), 0.69F * Math.Pow(webTee.Size.tw, 0.5));

				Reporting.AddLine("b = g / 2 - k1 = " + beam.GageOnColumn + " / 2 - " + webTee.Size.k1 + " = " + B + ConstUnit.Length);
				Reporting.AddLine("db_min= Min(0.163 * tf * (Fy / b * (b² / L² + 2))^0.5, 0.69 * ts^0.5)");
				Reporting.AddLine("= Min(0.163 * " + webTee.Size.tf + " * (" + webTee.Material.Fy + " / " + B + " * (" + B + "² / " + webTee.SLength + "² + 2))^0.5, 0.69 * " + webTee.Size.tw + "^0.5)");
				if (db_min <= webTee.BoltOslOnFlange.BoltSize)
					Reporting.AddLine("= " + db_min + " <= " + webTee.BoltOslOnFlange.BoltSize + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + db_min + " >> " + webTee.BoltOslOnFlange.BoltSize + ConstUnit.Length + " (NG)");

				if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice && !CommonDataStatic.IsFema)
				{
					Reporting.AddHeader("Bolt Holes on Support: " + webTee.BoltWebOnStem.HoleWidth + ConstUnit.Length + " Vert. X " + webTee.BoltWebOnStem.HoleLength + ConstUnit.Length + " Horiz.");
					Reporting.AddLine("Effective Thickness of Support Material: " + SupThickness + ConstUnit.Length);
					MiscCalculationsWithReporting.DetermineBoltAlignment(memberType, true);
				}
				Reporting.AddLine("Bolt Holes on Tee Flange: " + webTee.BoltOslOnFlange.HoleWidth + ConstUnit.Length + " Vert. X " + webTee.BoltOslOnFlange.HoleLength + ConstUnit.Length + " Horiz.");
			}
			else
			{
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
				{
					double argTemp2 = 0;
					MiscCalculationDataMethods.WebTeeSupportWeldType(beam.MemberType, ref argTemp2, webTee.Size);
					if (webTee.FarSideWeldIsFlare && webTee.NearSideWeldIsFlare)
					{
						Reporting.AddHeader("Support Side Connection:");
						Reporting.AddLine(CommonCalculations.WeldSize(5 / 8.0 * column.Shape.tf) + " (Effective Throat) " + webTee.WeldName + " Flare Bevel Groove Welds");
						Reporting.AddLine("Equivalent Fillet Weld Size= " + CommonCalculations.WeldSize(5 / 8.0 * column.Shape.tf / 0.707) + ConstUnit.Length);
					}
					else if (webTee.FarSideWeldIsFillet && webTee.NearSideWeldIsFillet)
					{
						Reporting.AddHeader("Support Side Connection: ");
						Reporting.AddLine(CommonCalculations.WeldSize(webTee.WeldSizeFlange) + " " + webTee.WeldName + " Fillet Welds");
					}
					else
					{
						if (webTee.NearSideWeldIsFlare)
						{
							Reporting.AddHeader("Support Side Connection (Near Side):");
							Reporting.AddLine(CommonCalculations.WeldSize(5 / 8.0 * column.Shape.tf) + " (Effective Throat) " + webTee.WeldName + " Flare Bevel Groove Weld");
							Reporting.AddLine("Equivalent Fillet Weld Size= " + CommonCalculations.WeldSize(5 / 8.0 * column.Shape.tf / 0.707) + ConstUnit.Length);
						}
						else if (webTee.NearSideWeldIsFillet)
						{
							Reporting.AddHeader("Support Side Connection (Near Side):");
							Reporting.AddLine(CommonCalculations.WeldSize(webTee.WeldSizeFlange) + " " + webTee.WeldName + " Fillet Weld");
						}
						else
						{
							Reporting.AddLine("Support Side Connection (Near Side):");
							Reporting.AddLine("Not enough space for weld (NG)");
						}
						if (webTee.FarSideWeldIsFlare)
						{
							Reporting.AddHeader("Support Side Connection (Far Side):");
							Reporting.AddLine(CommonCalculations.WeldSize(5 / 8.0 * column.Shape.tf) + " (Effective Throat) " + webTee.WeldName + " Flare Bevel Groove Weld");
							Reporting.AddLine("Equivalent Fillet Weld Size= " + CommonCalculations.WeldSize(5 / 8.0 * column.Shape.tf / 0.707) + ConstUnit.Length);
						}
						else if (webTee.FarSideWeldIsFillet)
						{
							Reporting.AddHeader("Support Side Connection (Far Side):");
							Reporting.AddLine(CommonCalculations.WeldSize(webTee.WeldSizeFlange) + " " + webTee.WeldName + " Fillet Weld");
						}
						else
						{
							Reporting.AddLine("Support Side Connection (Far Side):");
							Reporting.AddLine("Not enough space for weld (NG)");
						}
					}
				}
				else
				{
					Reporting.AddHeader("Support Side Connection:");
					Reporting.AddLine(CommonCalculations.WeldSize(webTee.WeldSizeFlange) + " " + webTee.WeldName + " Fillet Weld");
				}
				Reporting.AddLine("Effective Thickness of Support Material: " + SupThickness + ConstUnit.Length);
				MiscCalculationsWithReporting.DetermineBoltAlignment(memberType, true);
			}
			if (webTee.BeamSideConnection == EConnectionStyle.Bolted)
			{
				Reporting.AddHeader("Beam Side Connection: " + webTee.StemNumberOfRows + " Bolts " + webTee.BoltWebOnStem.BoltName);
				Reporting.AddLine("Bolt Holes on Beam Web: " + webTee.BoltWebOnStem.HoleWidth + ConstUnit.Length + " Vert. X " + webTee.BoltWebOnStem.HoleLength + ConstUnit.Length + " Horiz.");
				Reporting.AddLine("Bolt Holes on Tee Stem: " + webTee.BoltWebOnStem.HoleWidth + ConstUnit.Length + " Vert. X " + webTee.BoltWebOnStem.HoleLength + ConstUnit.Length + " Horiz.");
				Reporting.AddLine("Beam Web Thickness: " + beam.Shape.tw + ConstUnit.Length);
				Reporting.AddLine("Beam Web Height: " + beam.WinConnect.Beam.WebHeight + ConstUnit.Length);
			}
			else
			{
				Reporting.AddHeader("Beam Side Connection: " + CommonCalculations.WeldSize(webTee.WeldSizeStem) + " " + webTee.WeldName + " Fillet Welds");
				Reporting.AddLine("Beam Web Thickness: " + beam.Shape.tw + ConstUnit.Length);
				Reporting.AddLine("Beam Web Height: " + beam.WinConnect.Beam.WebHeight + ConstUnit.Length);
			}
			Reporting.AddLine("Beam Setback: " + beam.EndSetback + ConstUnit.Length);
			V = webTee.V;
			h = webTee.H;
			R = Math.Sqrt(h * h + V * V);

			Reporting.AddHeader("Loading:");
			Reporting.AddLine("Vertical Shear, V = " + V + ConstUnit.Force);
			Reporting.AddLine("Axial Load, H = " + h + ConstUnit.Force);
			Reporting.AddLine("Resultant, R = (V² + H²)^0.5 = ((" + V + " )² + " + h + "²)^0.5 = " + R + ConstUnit.Force);

			Reporting.AddHeader("Check Clearances:");
			if (webTee.BeamSideConnection == EConnectionStyle.Welded)
				w = webTee.WeldSizeStem;
			else
				w = 0;
			if (beam.WinConnect.Beam.WebHeight >= webTee.SLength)
				Reporting.AddLine("Beam Web Clear Height = " + beam.WinConnect.Beam.WebHeight + " >= " + webTee.SLength + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Beam Web Clear Height = " + beam.WinConnect.Beam.WebHeight + " << " + webTee.SLength + ConstUnit.Length + " (NG)");

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder &&
				beam.WinConnect.Beam.DistanceToFirstBoltDisplay < ConstNum.QUARTER_INCH ||
				(beam.WinConnect.Beam.CopeReinforcement.Type == EReinforcementType.None && beam.WinConnect.Beam.DistanceToFirstBoltDisplay < (webTee.WeldSizeStem + ConstNum.SIXTEENTH_INCH)) ||
				(beam.WinConnect.Beam.CopeReinforcement.Type != EReinforcementType.None && beam.WinConnect.Beam.DistanceToFirstBoltDisplay < (webTee.WeldSizeStem + beam.WinConnect.Beam.CopeReinforcement.WeldSize + ConstNum.SIXTEENTH_INCH)))
				Reporting.AddLine("Clip Angle is too high (NG)");

			if (beam.P > 0)
			{
				double T = beam.Shape.d - 2 * beam.Shape.kdet;
				if (MinL <= webTee.SLength)
					Reporting.AddLine("Minimum Length of Tee = T / 2 = " + T + " / 2 = " + MinL + " <= " + webTee.SLength + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Length of Tee = T / 2 = " + T + " / 2 = " + MinL + " >> " + webTee.SLength + ConstUnit.Length + " (NG)");
			}

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BeamToHSSColumn:
				case EJointConfiguration.BeamToColumnFlange:
					if (webTee.OSLConnection == EConnectionStyle.Welded)
					{
						switch (CommonDataStatic.BeamToColumnType)
						{
							case EJointConfiguration.BeamToColumnFlange:
								B = webTee.Size.bf + 2 * webTee.WeldSizeFlange;
								if (column.Shape.bf >= B)
									Reporting.AddLine("Column Flange Width = " + column.Shape.bf + " >= " + B + ConstUnit.Length + " (OK)");
								else
									Reporting.AddLine("Column Flange Width = " + column.Shape.bf + " << " + B + ConstUnit.Length + " (NG)");
								break;
							case EJointConfiguration.BeamToHSSColumn:
								MiscCalculationDataMethods.WebTeeSupportWeldType(beam.MemberType, ref BF, webTee.Size);
								if (!((webTee.FarSideWeldIsFlare || webTee.FarSideWeldIsFillet) && (webTee.NearSideWeldIsFlare || webTee.NearSideWeldIsFillet)))
									Reporting.AddLine("Warning: The Tee Flange is too narrow to allow flare weld and too wide to allow fillet weld");
								break;
						}
					}
					break;
				case EJointConfiguration.BeamToColumnWeb:
					B = webTee.Size.bf + webTee.Size.tw + beam.Shape.tw;
					w = column.Shape.t;
					if (w >= B)
						Reporting.AddLine("Column Web = " + w + " >= " + B + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Column Web = " + w + " << " + B + ConstUnit.Length + " (NG)");
					break;
			}

			if (webTee.OSLConnection == EConnectionStyle.Bolted)
			{
				double minSpacing;

				Reporting.AddGoToHeader("Support Side Bolts:");
				FuT = Math.Min(SupportFu * SupThickness, webTee.Material.Fu * webTee.Size.tf);
				spSupport = MiscCalculationsWithReporting.MinimumSpacing((webTee.BoltOslOnFlange.BoltSize), (webTee.V / webTee.FlangeNumberOfRows), SupportFu * SupThickness, leftBeam.WinConnect.ShearWebTee.BoltWebOnStem.HoleWidth, webTee.BoltOslOnFlange.HoleType);
				spFlange = MiscCalculationsWithReporting.MinimumSpacing((webTee.BoltOslOnFlange.BoltSize), (webTee.V / webTee.FlangeNumberOfRows), webTee.Material.Fu * webTee.Size.tf, webTee.BoltOslOnFlange.HoleWidth, webTee.BoltOslOnFlange.HoleType);

				minSpacing = Math.Max(spSupport, spFlange);

				if (webTee.FlangeSpacingLongDir >= webTee.BoltOslOnFlange.MinSpacing)
					Reporting.AddLine("Spacing (s) = " + webTee.FlangeSpacingLongDir + " >= Minimum Spacing = " + minSpacing + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Spacing (s) = " + webTee.FlangeSpacingLongDir + " << Minimum Spacing =" + minSpacing + ConstUnit.Length + " (NG)");

				evmin = MiscCalculationsWithReporting.MinimumEdgeDist(webTee.BoltOslOnFlange.BoltSize, webTee.V / webTee.FlangeNumberOfRows, webTee.Material.Fu * webTee.Size.tf, webTee.BoltOslOnFlange.MinEdgeSheared, (int)webTee.BoltOslOnFlange.HoleWidth, webTee.BoltOslOnFlange.HoleType);

				if (OslTop != OslBot)
				{
					Reporting.AddHeader("Distance to Horizontal Edge, ev (Top):");
					if (OslTop >= evmin)
						Reporting.AddLine("= " + OslTop + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + OslTop + " << " + evmin + ConstUnit.Length + " (NG)");
					Reporting.AddLine("Distance to Horizontal Edge (ev) (Bottom):");
					if (OslBot >= evmin)
						Reporting.AddLine("= " + OslBot + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + OslBot + " << " + evmin + ConstUnit.Length + " (NG)");
				}
				else
				{
					Reporting.AddLine("Distance to Horizontal Edge (ev):");
					if (OslBot >= evmin)
						Reporting.AddLine("= " + OslBot + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + OslBot + " << " + evmin + ConstUnit.Length + " (NG)");
				}
				ehmin = MiscCalculationsWithReporting.MinimumEdgeDist(webTee.BoltOslOnFlange.BoltSize, 0, webTee.Material.Fu * webTee.Size.tf, webTee.BoltOslOnFlange.MinEdgeRolled, (int)webTee.BoltOslOnFlange.HoleLength, webTee.BoltOslOnFlange.HoleType); // CommonCalculations.RolledEdge(fbolts.d)
				Reporting.AddLine("Distance to Vertical Edge (eh):");
				if (webTee.BoltOslOnFlange.EdgeDistTransvDir >= ehmin)
					Reporting.AddLine("= " + webTee.BoltOslOnFlange.EdgeDistTransvDir + " >= " + ehmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + webTee.BoltOslOnFlange.EdgeDistTransvDir + " << " + ehmin + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Gage on Flange:");
				g = beam.GageOnColumn;
				gmin = webTee.BoltOslOnFlange.BoltSize + ConstNum.HALF_INCH + webTee.Size.kdet + webTee.Size.tw / 2 + beam.Shape.tw + (webTee.BoltOslOnFlange.BoltSize + ConstNum.HALF_INCH);
				if (g >= gmin)
				{
					Reporting.AddLine("Bolt Gage on Tee Flange = " + g + " >= " + gmin + ConstUnit.Length + " (OK)");
				}
				else
				{
					Reporting.AddLine("Bolt Gage on Tee Flange = " + g + " << " + gmin + ConstUnit.Length + " (NG)");
					Reporting.AddLine("Verify with the fabricator. This may be acceptable or you may need to increase the gage on column");
				}
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
				{
					Reporting.AddLine("Column Gage = " + beam.GageOnColumn + ConstUnit.Length);
					d1 = (beam.GageOnColumn - leftBeam.WinConnect.ShearWebTee.BoltWebOnStem.HoleLength) / 2;
					d2 = (beam.GageOnColumn + leftBeam.WinConnect.ShearWebTee.BoltWebOnStem.HoleLength) / 2;
					a1 = (g - webTee.BoltOslOnFlange.HoleLength) / 2;
					a2 = (g + webTee.BoltOslOnFlange.HoleLength) / 2;
					if (Math.Min(d2, a2) - Math.Max(d1, a1) < webTee.BoltOslOnFlange.BoltSize)
						Reporting.AddLine("Check Bolt Hole Line-up (NG)");
				}

				Reporting.AddGoToHeader(ConstString.DES_OR_ALLOWABLE + " Shear Strength of Bolts:");
				if (beam.P == 0 && !RigidSupport)
				{
					Cap = 2 * webTee.FlangeNumberOfRows * webTee.BoltOslOnFlange.BoltStrength;
					if (Cap >= webTee.V)
						Reporting.AddCapacityLine("= 2 * n * (" + ConstString.PHI + " Rn) =  2 * " + webTee.FlangeNumberOfRows + " * " + webTee.BoltOslOnFlange.BoltStrength + " = " + Cap + " >= " + webTee.V + ConstUnit.Force + " (OK)", webTee.V / Cap, "Shear Strength of Bolts", memberType);
					else
						Reporting.AddCapacityLine("= 2 * n * (" + ConstString.PHI + " Rn) =  2 * " + webTee.FlangeNumberOfRows + " * " + webTee.BoltOslOnFlange.BoltStrength + " = " + Cap + " << " + webTee.V + ConstUnit.Force + " (NG)", webTee.V / Cap, "Shear Strength of Bolts", memberType);
					n = webTee.FlangeNumberOfRows;
					c = n;
				}
				else
				{
					if (webTee.BoltOslOnFlange.BoltType == EBoltType.SC)
					{
						switch (CommonDataStatic.Preferences.CalcMode)
						{
							case ECalcMode.ASD:
								Cap = 2 * webTee.FlangeNumberOfRows * webTee.BoltOslOnFlange.BoltStrength * (1 - 1.5 * webTee.H / (1.13F * webTee.BoltOslOnFlange.Pretension * webTee.FlangeNumberOfRows));
								Reporting.AddLine("= 2 * n * (" + ConstString.PHI + " Rn) * (1 - 1.5 * Ta / (1.13 * Tm * n))");
								Reporting.AddLine("= 2 * " + webTee.FlangeNumberOfRows + " * " + webTee.BoltOslOnFlange.BoltStrength + " *  (1 - 1.5 * " + webTee.H + " / (1.13 * " + webTee.BoltOslOnFlange.Pretension + " * " + webTee.FlangeNumberOfRows + "))");
								break;
							case ECalcMode.LRFD:
								Cap = 2 * webTee.FlangeNumberOfRows * webTee.BoltOslOnFlange.BoltStrength * (1 - webTee.H / (1.13F * webTee.BoltOslOnFlange.Pretension * webTee.FlangeNumberOfRows));
								Reporting.AddLine("= 2 * n * (" + ConstString.PHI + " Rn) * (1 - Tu / (1.13 * Tm * n))");
								Reporting.AddLine("= 2 * " + webTee.FlangeNumberOfRows + " * " + webTee.BoltOslOnFlange.BoltStrength + " *  (1 - " + webTee.H + " / (1.13 * " + webTee.BoltOslOnFlange.Pretension + " * " + webTee.FlangeNumberOfRows + "))");
								break;
						}

						if (Cap >= webTee.V)
							Reporting.AddCapacityLine("= " + Cap + " >= " + webTee.V + ConstUnit.Force + " (OK)", webTee.V / Cap, "Shear Strength of Bolts", beam.MemberType);
						else
							Reporting.AddCapacityLine("= " + Cap + " << " + webTee.V + ConstUnit.Force + " (NG)", webTee.V / Cap, "Shear Strength of Bolts", beam.MemberType);
					}
					else
					{
						Cap = 2 * webTee.FlangeNumberOfRows * webTee.BoltOslOnFlange.BoltStrength;
						if (Cap >= webTee.V)
							Reporting.AddCapacityLine("= 2 * n * (" + ConstString.PHI + " Rn) = 2 * " + webTee.FlangeNumberOfRows + " * " + webTee.BoltOslOnFlange.BoltStrength + " = " + Cap + " >= " + webTee.V + ConstUnit.Force + " (OK)", webTee.V / Cap, "Bolts", beam.MemberType);
						else
							Reporting.AddCapacityLine("= 2 * n * (" + ConstString.PHI + " Rn) = 2 * " + webTee.FlangeNumberOfRows + " * " + webTee.BoltOslOnFlange.BoltStrength + " = " + Cap + " << " + webTee.V + ConstUnit.Force + " (NG)", webTee.V / Cap, "Bolts", beam.MemberType);
					}
					if (RigidSupport)
						ecc = beam.EndSetback + beam.WinConnect.Beam.Lh;
					else
						ecc = 0;

					Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Tension Strength of Bolts:");
					MiscCalculationsWithReporting.OSLBoltsWithoutPlaneEccentricity(memberType, webTee.V, ((int)webTee.H), webTee.FlangeNumberOfRows, ((int)webTee.FlangeSpacingLongDir), ecc, ref ruv, ref rut);
					FiRn_perBolt = BoltsForTension.CalcBoltsForTension(memberType, webTee.BoltOslOnFlange, webTee.H, webTee.V, (2 * webTee.FlangeNumberOfRows), true);
					if (FiRn_perBolt >= rut)
						Reporting.AddCapacityLine(ConstString.PHI + " Rn = " + FiRn_perBolt + " >= " + rut + ConstUnit.Force + " / bolt  (OK)", rut / FiRn_perBolt, "Tension Strength of Bolts", beam.MemberType);
					else
						Reporting.AddCapacityLine(ConstString.PHI + " Rn = " + FiRn_perBolt + " << " + rut + ConstUnit.Force + " / bolt  (NG)", rut / FiRn_perBolt, "Tension Strength of Bolts", beam.MemberType);

					Reporting.AddHeader("Tension Strength of Tee Flange");
					edg = webTee.BoltOslOnFlange.EdgeDistTransvDir;
					B = (beam.GageOnColumn - webTee.Size.tw) / 2;
					rut_Applied = rut;
					FiRn = MiscCalculationsWithReporting.HangerStrength(beam.MemberType, webTee.BoltOslOnFlange.HoleWidth, webTee.Material.Fu, webTee.Size.tf, edg, B, FiRn_perBolt, rut, true);
					TensionCap = FiRn;

					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Tension Strength:");
					if (TensionCap >= rut_Applied)
						Reporting.AddCapacityLine(ConstString.PHI + " Rn = " + FiRn + " >= " + rut_Applied + ConstUnit.Force + " / Bolt (OK)", rut_Applied / TensionCap, "Tension Strength", beam.MemberType);
					else
						Reporting.AddCapacityLine(ConstString.PHI + " Rn = " + FiRn + " << " + rut_Applied + ConstUnit.Force + " / Bolt (NG)", rut_Applied / TensionCap, "Tension Strength", beam.MemberType);
				}

				Reporting.AddHeader("Bolt Bearing on Tee Flange:");
				Fbe = CommonCalculations.EdgeBearing(Math.Min(OslTop, OslBot), webTee.BoltOslOnFlange.HoleWidth, webTee.BoltOslOnFlange.BoltSize, webTee.Material.Fu, webTee.BoltOslOnFlange.HoleType, true);
				Fbs = CommonCalculations.SpacingBearing(webTee.FlangeSpacingLongDir, webTee.BoltOslOnFlange.HoleWidth, webTee.BoltOslOnFlange.BoltSize, webTee.BoltOslOnFlange.HoleType, webTee.Material.Fu, true);
				BearingCap = 2 * (Fbe + Fbs * (webTee.FlangeNumberOfRows - 1)) * webTee.Size.tf;

				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = 2 * (Fbe + Fbs * (n - 1)) * t");
				Reporting.AddLine("= 2 * (" + Fbe + " + " + Fbs + " * (" + webTee.FlangeNumberOfRows + " - 1)) * " + webTee.Size.tf);
				if (BearingCap >= V)
					Reporting.AddCapacityLine("= " + BearingCap + " >= " + V + ConstUnit.Force + " (OK)", V / BearingCap, "Bolt Bearing on Tee Flange", beam.MemberType);
				else
					Reporting.AddCapacityLine("= " + BearingCap + " << " + V + ConstUnit.Force + " (NG)", V / BearingCap, "Bolt Bearing on Tee Flange", beam.MemberType);

				if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice && !CommonDataStatic.IsFema)
				{
					Reporting.AddHeader("Bolt Bearing on Support:");
					Fbs = CommonCalculations.SpacingBearing(webTee.FlangeSpacingLongDir, leftBeam.WinConnect.ShearWebTee.BoltWebOnStem.HoleWidth, webTee.BoltOslOnFlange.BoltSize, webTee.BoltOslOnFlange.HoleType, SupportFu, true);
					BearingCap = 2 * Fbs * webTee.FlangeNumberOfRows * SupThickness;

					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = 2 * Fbs * n * t");
					Reporting.AddLine("= 2 * " + Fbs + " * " + webTee.FlangeNumberOfRows + " * " + SupThickness);
					if (BearingCap >= V)
						Reporting.AddCapacityLine("= " + BearingCap + " >= " + V + ConstUnit.Force + " (OK)", V / BearingCap, "Bolt Bearing on Support", beam.MemberType);
					else
						Reporting.AddCapacityLine("= " + BearingCap + " << " + V + ConstUnit.Force + " (NG)", V / BearingCap, "Bolt Bearing on Support", beam.MemberType);
				}
			}
			else
			{
				Reporting.AddGoToHeader("Support Side Weld: " + CommonCalculations.WeldSize(webTee.WeldSizeFlange) + " " + webTee.WeldName);
                Reporting.AddLine(string.Empty);
                Reporting.AddLine("Tee Flange Thickness = " + webTee.Size.tf + ConstUnit.Length);
				Reporting.AddLine("Support Thickness = " + SupThickness + ConstUnit.Length);
                Reporting.AddLine(string.Empty);

				mnweld = CommonCalculations.MinimumWeld(webTee.Size.tf, SupThickness1);
				if (webTee.Size.tf >= ConstNum.QUARTER_INCH)
					MxWeld = webTee.Size.tf - ConstNum.SIXTEENTH_INCH;
				else
					MxWeld = webTee.Size.tf;

				Reporting.AddLine("Minimum Weld = " + CommonCalculations.WeldSize(mnweld) + ConstUnit.Length);
				if (webTee.WeldSizeFlange >= mnweld)
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(webTee.WeldSizeFlange) + " >= " + CommonCalculations.WeldSize(mnweld) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(webTee.WeldSizeFlange) + " << " + CommonCalculations.WeldSize(mnweld) + ConstUnit.Length + " (NG)");

				if (webTee.FarSideWeldIsFillet || webTee.NearSideWeldIsFillet)
				{
					Reporting.AddLine("Maximum Weld = " + CommonCalculations.WeldSize(MxWeld) + ConstUnit.Length);
					if (webTee.WeldSizeFlange <= MxWeld)
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(webTee.WeldSizeFlange) + " <= " + CommonCalculations.WeldSize(MxWeld) + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(webTee.WeldSizeFlange) + " >> " + CommonCalculations.WeldSize(MxWeld) + ConstUnit.Length + " (NG)");
				}

				Reporting.AddHeader("Minimum Weld Size for Ductility:");
                Reporting.AddLine(string.Empty);
				B = webTee.Size.bf / 2 - webTee.Size.k1;
				Reporting.AddLine("b = bf/2 - k1 = ( " + webTee.Size.bf + " / 2 - " + webTee.Size.k1 + " = " + B + ConstUnit.Length);
                Reporting.AddLine(string.Empty);

				w_min = 1.106 / CommonDataStatic.Preferences.DefaultElectrode.Fexx * (webTee.Material.Fy * Math.Pow(webTee.Size.tf, 2) / B) * (Math.Pow(B, 2) / Math.Pow(webTee.SLength, 2) + 2);
				if (w_min > 0.625 * webTee.Size.tw)
					w_min = 0.625 * webTee.Size.tw;

				Reporting.AddLine("w_min = (1.106 / Fexx) * (Fy * tf² / b) * (b² / L² + 2) <= 5 / 8 * ts");
				Reporting.AddLine("= (1.106 / " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ") * (" + webTee.Material.Fy + " * " + webTee.Size.tf + "² / " + B + ") * (" + B + "² / " + webTee.SLength + "² + 2) <= 0.625 * " + webTee.Size.tw);
				Reporting.AddLine("= " + w_min + ConstUnit.Length);
                Reporting.AddLine(string.Empty);
				if (webTee.WeldSizeFlange >= w_min)
					Reporting.AddLine("Weld Size = " + webTee.WeldSizeFlange + " >= " + w_min + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + webTee.WeldSizeFlange + " << " + w_min + ConstUnit.Length + " (NG)");

				OslWeldCap = MiscCalculationsWithReporting.OslWeld(beam.MemberType, 0, true);

				Reporting.AddHeader("Reduction Factor for Support Thickness (Rt):");
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
				{
					if (beam.MemberType == EMemberType.LeftBeam)
					{
						if (rightBeam.IsActive && rightBeam.WinConnect.ShearWebTee.WeldSizeFlange == 0 && !rightBeam.WinConnect.ShearWebTee.WeldSizeFlange_User)
							rightBeam.WinConnect.ShearWebTee.WeldSizeFlange = ConstNum.EIGHTH_INCH;
						if (!rightBeam.IsActive && !rightBeam.WinConnect.ShearWebTee.WeldSizeFlange_User)
							rightBeam.WinConnect.ShearWebTee.WeldSizeFlange = 0;
						RT = 1.414 * SupportFu * SupThickness1 / ((webTee.WeldSizeFlange + rightBeam.WinConnect.ShearWebTee.WeldSizeFlange) * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						if (!leftBeam.WinConnect.ShearWebTee.WeldSizeFlange_User)
							leftBeam.WinConnect.ShearWebTee.WeldSizeFlange = webTee.WeldSizeFlange;
					}
					else
					{
						if (leftBeam.IsActive && leftBeam.WinConnect.ShearWebTee.WeldSizeFlange == 0 && !leftBeam.WinConnect.ShearWebTee.WeldSizeFlange_User)
							leftBeam.WinConnect.ShearWebTee.WeldSizeFlange = ConstNum.EIGHTH_INCH;
						if (!leftBeam.IsActive && !leftBeam.WinConnect.ShearWebTee.WeldSizeFlange_User)
							leftBeam.WinConnect.ShearWebTee.WeldSizeFlange = 0;
						RT = 1.414 * SupportFu * SupThickness1 / ((webTee.WeldSizeFlange + leftBeam.WinConnect.ShearWebTee.WeldSizeFlange) * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						if (!rightBeam.WinConnect.ShearWebTee.WeldSizeFlange_User)
							rightBeam.WinConnect.ShearWebTee.WeldSizeFlange = webTee.WeldSizeFlange;
					}
					Reporting.AddLine("= 1.414 * Fu * ts / ((wL + wR) * Fexx)");
					Reporting.AddLine("= 1.414 * " + SupportFu + " * " + SupThickness1 + " / ((" + leftBeam.WinConnect.ShearWebTee.WeldSizeFlange + " + " + rightBeam.WinConnect.ShearWebTee.WeldSizeFlange + ") * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				}
				else
				{
					RT = 1.414 * SupportFu * SupThickness1 / (webTee.WeldSizeFlange * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddLine("= 1.414 * Fu * ts / (w * Fexx)");
					Reporting.AddLine("= 1.414 * " + SupportFu + " * " + SupThickness1 + " / (" + webTee.WeldSizeFlange + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				}
				if (RT > 1)
				{
					WeldCap = OslWeldCap;
					Reporting.AddLine("= " + RT + " >= 1 No Reduction, " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " (One Line) = " + WeldCap + ConstUnit.Force);
				}
				else
				{
					WeldCap = RT * OslWeldCap;
					Reporting.AddLine("= " + RT + "; " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " (One Line) = " + RT + " * " + OslWeldCap + " = " + WeldCap + ConstUnit.Force);
				}
				WeldCap = 2 * WeldCap;

                Reporting.AddLine(string.Empty);
				if (WeldCap >= R)
					Reporting.AddCapacityLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + WeldCap + " >= " + R + ConstUnit.Force + " (OK)", R / WeldCap, "Weld", beam.MemberType);
				else
					Reporting.AddCapacityLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + WeldCap + " << " + R + ConstUnit.Force + " (NG)", R / WeldCap, "Weld", beam.MemberType);

				if (webTee.H != 0)
				{
					OslMoment = webTee.H * (webTee.Size.bf / 2 - webTee.Size.k1) / 2;
					BendingCap = ConstNum.FIOMEGA0_9N * webTee.Material.Fy * webTee.SLength * Math.Pow(webTee.Size.tf, 2) / 4;

					Reporting.AddHeader("Tee Flange Bending:");
					Reporting.AddLine("Moment = T * (bf / 2 - k1) / 2 = " + webTee.H + " * (" + (webTee.Size.bf / 2) + " - " + webTee.Size.k1 + ") / 2 = " + OslMoment + ConstUnit.Moment + "");
					if (BendingCap >= OslMoment)
						Reporting.AddLine("Bending Strength = " + ConstString.FIOMEGA0_9 + " * Fy * L * t² / 4 = " + ConstString.FIOMEGA0_9 + " * " + webTee.Material.Fy + " * " + webTee.SLength + " * " + webTee.Size.tf + "² / 4 = " + BendingCap + " >= " + OslMoment + ConstUnit.Moment + " (OK)");
					else
						Reporting.AddLine("Bending Strength = " + ConstString.FIOMEGA0_9 + " * Fy * L * t² / 4 = " + ConstString.FIOMEGA0_9 + " * " + webTee.Material.Fy + " * " + webTee.SLength + " * " + webTee.Size.tf + "² / 4 = " + BendingCap + " << " + OslMoment + ConstUnit.Moment + " (NG)");
				}
			}

			if (webTee.BeamSideConnection == EConnectionStyle.Bolted)
			{
				double minSpacing;

				webTee.BoltWebOnStem.EdgeDistTransvDir = webTee.Size.d - beam.EndSetback - beam.WinConnect.Beam.Lh;
				Reporting.AddGoToHeader("Beam Side Bolts: " + webTee.StemNumberOfRows + " Bolts - " + webTee.BoltWebOnStem.BoltName);

				FuT = Math.Min(beam.Material.Fu * beam.Shape.tw, webTee.Material.Fu * webTee.Size.tw);
				spBeam = MiscCalculationsWithReporting.MinimumSpacing((webTee.BoltWebOnStem.BoltSize), (webTee.V / webTee.StemNumberOfRows), beam.Material.Fu * beam.Shape.tw, webTee.BoltWebOnStem.HoleWidth, webTee.BoltWebOnStem.HoleType);
				spStem = MiscCalculationsWithReporting.MinimumSpacing((webTee.BoltWebOnStem.BoltSize), (webTee.V / webTee.StemNumberOfRows), webTee.Material.Fu * webTee.Size.tw, webTee.BoltWebOnStem.HoleWidth, webTee.BoltWebOnStem.HoleType);

				minSpacing = Math.Max(spBeam, spStem);

				if (webTee.StemSpacingLongDir >= webTee.BoltWebOnStem.MinSpacing)
					Reporting.AddLine("Spacing (s) = " + webTee.StemSpacingLongDir + " >= Minimum Spacing = " + minSpacing + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Spacing (s) = " + webTee.StemSpacingLongDir + " << Minimum Spacing = " + minSpacing + ConstUnit.Length + " (NG)");

				evmin = MiscCalculationsWithReporting.MinimumEdgeDist(webTee.BoltWebOnStem.BoltSize, webTee.V / webTee.StemNumberOfRows, webTee.Material.Fu * webTee.Size.tw, webTee.BoltWebOnStem.MinEdgeSheared, (int)webTee.BoltWebOnStem.HoleWidth, webTee.BoltWebOnStem.HoleType);
				Reporting.AddLine("");
				if (WTop != WBot)
				{
					Reporting.AddLine("Distance to Horizontal Edge (ev) (Top):");
					if (WTop >= evmin)
						Reporting.AddLine("= " + WTop + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + WTop + " << " + evmin + ConstUnit.Length + " (NG)");
					Reporting.AddLine("Distance to Horizontal Edge (ev) (Bottom):");
					if (WBot >= evmin)
						Reporting.AddLine("= " + WBot + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + WBot + " << " + evmin + ConstUnit.Length + " (NG)");
				}
				else
				{
					Reporting.AddLine("Distance to Horizontal Edge (ev):");
					if (webTee.StemEdgeDistLongDir >= evmin)
						Reporting.AddLine("= " + WBot + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + WBot + " << " + evmin + ConstUnit.Length + " (NG)");
				}
				er = webTee.BoltWebOnStem.MinEdgeSheared;
				ehmin = MiscCalculationsWithReporting.MinimumEdgeDist(webTee.BoltWebOnStem.BoltSize, webTee.H / webTee.StemNumberOfRows, webTee.Material.Fu * webTee.Size.tw, er, (int)webTee.BoltWebOnStem.HoleLength, webTee.BoltWebOnStem.HoleType);

				Reporting.AddLine("Distance to Vertical Edge (eh):");
				if (webTee.BoltWebOnStem.EdgeDistTransvDir >= ehmin)
					Reporting.AddLine("= " + webTee.BoltWebOnStem.EdgeDistTransvDir + " >= " + ehmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + webTee.BoltWebOnStem.EdgeDistTransvDir + " << " + ehmin + ConstUnit.Length + " (NG)");

				Reporting.AddLine("Bolt Gage on Tee Stem:");
				webTee.BoltGageOnTeeStem = webTee.Size.d - webTee.BoltWebOnStem.EdgeDistTransvDir;

				if (webTee.BoltStagger == EBoltStagger.None)
					gmin = webTee.Size.kdet + webTee.BoltWebOnStem.BoltSize + ConstNum.HALF_INCH;
				else
					gmin = MiscCalculationDataMethods.StaggeredBoltGage(Math.Max(webTee.BoltWebOnStem.BoltSize, webTee.BoltOslOnFlange.BoltSize), (Math.Max(webTee.StemSpacingLongDir, webTee.FlangeSpacingLongDir) / 2), webTee.Size.tf);

				if (webTee.BoltGageOnTeeStem >= gmin)
					Reporting.AddLine("= " + webTee.BoltGageOnTeeStem + " >= " + gmin + ConstUnit.Length + " (OK)");
				else
				{
					Reporting.AddLine("= " + webTee.BoltGageOnTeeStem + " << " + gmin + ConstUnit.Length + " (NG)");
					Reporting.AddLine("Verify with the fabricator. This may be acceptable.");
					Reporting.AddLine("(The gage is the sum of Lh of the beam and the beam setback.");
					Reporting.AddLine("You can change these values in the 'Beam data window')");
				}

				Reporting.AddGoToHeader(ConstString.DES_OR_ALLOWABLE + " Shear Strength of Bolts:");
				if (RigidSupport)
					ecc = 0;
				else
					ecc = beam.EndSetback + beam.WinConnect.Beam.Lh;

				MiscCalculationsWithReporting.EccentricBolt(ecc, webTee.StemSpacingLongDir, 0, 1, webTee.StemNumberOfRows, ref c);
				Reporting.AddLine("Load Eccentricity = " + ecc + " " + ConstUnit.Length);
				Reporting.AddLine("Number of Bolts = " + webTee.StemNumberOfRows);
				Reporting.AddLine("Bolt Spacing = " + webTee.StemSpacingLongDir + " " + ConstUnit.Length);
				Reporting.AddLine("C = " + c);
				Cap = c * webTee.BoltWebOnStem.BoltStrength;
				if (Cap >= R)
					Reporting.AddCapacityLine("= c * (" + ConstString.PHI + " Rn) = " + c + " * " + webTee.BoltWebOnStem.BoltStrength + " = " + Cap + " >= " + R + ConstUnit.Force + " (OK)", R / Cap, "Shear Strength of Bolts", beam.MemberType);
				else
					Reporting.AddCapacityLine("= c * (" + ConstString.PHI + " Rn) = " + c + " * " + webTee.BoltWebOnStem.BoltStrength + " = " + Cap + " << " + R + ConstUnit.Force + " (NG)", R / Cap, "Shear Strength of Bolts", beam.MemberType);

				Reporting.AddHeader("Bolt Bearing on Tee Stem:");
                Reporting.AddLine(string.Empty);
				Fbe = CommonCalculations.EdgeBearing(Math.Min(WTop, WBot), webTee.BoltWebOnStem.HoleWidth, webTee.BoltWebOnStem.BoltSize, webTee.Material.Fu, webTee.BoltWebOnStem.HoleType, true);
				Fbs = CommonCalculations.SpacingBearing(webTee.StemSpacingLongDir, webTee.BoltWebOnStem.HoleWidth, webTee.BoltWebOnStem.BoltSize, webTee.BoltWebOnStem.HoleType, webTee.Material.Fu, true);

				BearingCap = (Fbe + Fbs * (webTee.StemNumberOfRows - 1)) * webTee.Size.tw * c / webTee.StemNumberOfRows;

                Reporting.AddLine(string.Empty);
				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = (Fbe + Fbs * (n - 1)) * t * C / N");
				Reporting.AddLine("= (" + Fbe + " + " + Fbs + " * (" + webTee.StemNumberOfRows + " - 1)) * " + webTee.Size.tw + " * " + c + " / " + webTee.StemNumberOfRows);
				if (BearingCap >= V)
					Reporting.AddCapacityLine("= " + BearingCap + " >= " + V + ConstUnit.Force + " (OK)", V / BearingCap, "Bolt Bearing on Tee Stem", beam.MemberType);
				else
					Reporting.AddCapacityLine("= " + BearingCap + " << " + V + ConstUnit.Force + " (NG)", V / BearingCap, "Bolt Bearing on Tee Stem", beam.MemberType);

				Reporting.AddGoToHeader("Bolt Bearing on Beam Web:");

				Fbs = CommonCalculations.SpacingBearing(webTee.StemSpacingLongDir, webTee.BoltWebOnStem.HoleWidth, webTee.BoltWebOnStem.BoltSize, webTee.BoltWebOnStem.HoleType, beam.Material.Fu, true);
				if (beam.WinConnect.Beam.TopCope)
				{
					Fbe = CommonCalculations.EdgeBearing(beam.WinConnect.Beam.DistanceToFirstBolt, webTee.BoltWebOnStem.HoleWidth, webTee.BoltWebOnStem.BoltSize, beam.Material.Fu, webTee.BoltWebOnStem.HoleType, true);
					BearingCap = (Fbe + Fbs * (webTee.StemNumberOfRows - 1)) * beam.Shape.tw * c / webTee.StemNumberOfRows;
					Reporting.AddLine("Bearing Capacity = (Fbe + Fbs * (n - 1)) * t * C / N");
					Reporting.AddLine("= (" + Fbe + " + " + Fbs + " * (" + webTee.StemNumberOfRows + " - 1)) * " + beam.Shape.tw + " * " + c + " / " + webTee.StemNumberOfRows);
				}
				else
				{
					BearingCap = Fbs * webTee.StemNumberOfRows * beam.Shape.tw * c / webTee.StemNumberOfRows;
                    Reporting.AddLine(string.Empty);
                    Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = Fbs * n * t * C / N");
					Reporting.AddLine("= " + Fbs + " * " + webTee.StemNumberOfRows + " * " + beam.Shape.tw + " * " + c + " / " + webTee.StemNumberOfRows);
				}
				if (BearingCap >= V)
					Reporting.AddCapacityLine("= " + BearingCap + " >= " + V + ConstUnit.Force + " (OK)", V / BearingCap, "Bolt Bearing on Beam Web", beam.MemberType);
				else
					Reporting.AddCapacityLine("= " + BearingCap + " << " + V + ConstUnit.Force + " (NG)", V / BearingCap, "Bolt Bearing on Beam Web", beam.MemberType);
				if (h != 0)
				{
					Reporting.AddHeader("Bolt Bearing Under Beam Axial Load:");
					Reporting.AddHeader("On Tee Stem:");
					Fbe = CommonCalculations.EdgeBearing(webTee.BoltWebOnStem.EdgeDistTransvDir, webTee.BoltWebOnStem.HoleLength, webTee.BoltWebOnStem.BoltSize, webTee.Material.Fu, webTee.BoltWebOnStem.HoleType, true);
					BearingCap = Fbe * webTee.StemNumberOfRows * webTee.BoltWebOnStem.BoltSize * webTee.Size.tw;
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = Fbe * n * t");
					Reporting.AddLine("= " + Fbe + " * " + webTee.StemNumberOfRows + " * " + webTee.Size.tw);
					if (BearingCap >= h)
						Reporting.AddCapacityLine("= " + BearingCap + " >= " + h + ConstUnit.Force + " (OK)", h / BearingCap, "Bolt Bearing Under Beam Axial Load On Tee Stem", beam.MemberType);
					else
						Reporting.AddCapacityLine("= " + BearingCap + " << " + h + ConstUnit.Force + " (NG)", h / BearingCap, "Bolt Bearing Under Beam Axial Load On Tee Stem", beam.MemberType);

					Reporting.AddHeader("On Beam Web:");
					Fbe = CommonCalculations.EdgeBearing(beam.WinConnect.Beam.Lh, webTee.BoltWebOnStem.HoleLength, webTee.BoltWebOnStem.BoltSize, beam.Material.Fu, webTee.BoltWebOnStem.HoleType, true);
					BearingCap = Fbe * webTee.StemNumberOfRows * beam.Shape.tw;
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = Fbe*n*t");
					Reporting.AddLine("= " + Fbe + " * " + webTee.StemNumberOfRows + " * " + beam.Shape.tw);
					if (BearingCap >= h)
						Reporting.AddCapacityLine("= " + BearingCap + " >= " + h + ConstUnit.Force + " (OK)", h / BearingCap, "Bolt Bearing Under Beam Axial Load On Beam Web", beam.MemberType);
					else
						Reporting.AddCapacityLine("= " + BearingCap + " << " + h + ConstUnit.Force + " (NG)", h / BearingCap, "Bolt Bearing Under Beam Axial Load On Beam Web", beam.MemberType);
				}

				MiscCalculationsWithReporting.BlockShear(memberType, EShearOpt.WebTee, ref BsCap, true);
				if (webTee.H != 0 && (beam.WinConnect.Beam.TopCope & beam.WinConnect.Beam.BottomCope))
				{
					BeamTearOutCap = MiscCalculationsWithReporting.BeamWebTearout(beam.MemberType, webTee.StemNumberOfRows, 1, webTee.StemSpacingLongDir, 0, EBeamOrAttachment.Beam, ETearOutBlockShear.TearOut, true);
					if (BeamTearOutCap >= Math.Abs(webTee.H))
						Reporting.AddCapacityLine("= " + BeamTearOutCap + " >= " + Math.Abs(webTee.H) + ConstUnit.Force + " (OK)", Math.Abs(webTee.H) / BeamTearOutCap, "Beam Tear Out", beam.MemberType);
					else
						Reporting.AddCapacityLine("= " + BeamTearOutCap + " << " + Math.Abs(webTee.H) + ConstUnit.Force + " (NG)", Math.Abs(webTee.H) / BeamTearOutCap, "Beam Tear Out", beam.MemberType);
				}

				if (beam.WinConnect.Beam.TopCope || beam.WinConnect.Beam.BottomCope)
					Cope.CopedBeam(beam.MemberType, true);

				if (h != 0)
				{
					Reporting.AddHeader("Tee Stem Tear out Under Beam Axial Load:");
					TearoutStrength = MiscCalculationsWithReporting.BeamWebTearout(beam.MemberType, webTee.StemNumberOfRows, 1, webTee.StemSpacingLongDir, 0, EBeamOrAttachment.WebTee, ETearOutBlockShear.TearOut, true);
					if (TearoutStrength >= h)
						Reporting.AddCapacityLine("= " + TearoutStrength + " >= " + h + ConstUnit.Force + " (OK)", h / TearoutStrength, "Tee Stem Tear out Under Beam Axial Load", beam.MemberType);
					else
						Reporting.AddCapacityLine("= " + TearoutStrength + " << " + h + ConstUnit.Force + " (NG)", h / TearoutStrength, "Tee Stem Tear out Under Beam Axial Load", beam.MemberType);
				}
			}
			else
			{
				Reporting.AddGoToHeader("Beam Side Weld: " + CommonCalculations.WeldSize(webTee.WeldSizeStem) + " " + webTee.WeldName);
				Reporting.AddLine("Tee Stem Thickness = " + webTee.Size.tw + ConstUnit.Length);
				Reporting.AddLine("Beam Web Thickness = " + beam.Shape.tw + ConstUnit.Length);
				mnweld = CommonCalculations.MinimumWeld(webTee.Size.tw, beam.Shape.tw);
				if (webTee.Size.tw >= ConstNum.QUARTER_INCH)
					MxWeld = webTee.Size.tw - ConstNum.SIXTEENTH_INCH;
				else
					MxWeld = webTee.Size.tw;
				Reporting.AddLine("Minimum Weld = " + CommonCalculations.WeldSize(mnweld) + ConstUnit.Length);
				if (webTee.WeldSizeStem >= mnweld)
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(webTee.WeldSizeStem) + " >= " + CommonCalculations.WeldSize(mnweld) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(webTee.WeldSizeStem) + " << " + CommonCalculations.WeldSize(mnweld) + ConstUnit.Length + " (NG)");

				Reporting.AddLine("Maximum Weld = " + CommonCalculations.WeldSize(MxWeld) + ConstUnit.Length);
				if (webTee.WeldSizeStem <= MxWeld)
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(webTee.WeldSizeStem) + " <= " + CommonCalculations.WeldSize(MxWeld) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(webTee.WeldSizeStem) + " >> " + CommonCalculations.WeldSize(MxWeld) + ConstUnit.Length + " (NG)");

				MiscCalculationsWithReporting.ClipWelds(beam.MemberType, ref WWeldCap, true);
				if (WWeldCap >= R)
					Reporting.AddCapacityLine("Weld Capacity = " + WWeldCap + " >= " + R + ConstUnit.Force + " (OK)", R / WWeldCap, "Weld Capacity", beam.MemberType);
				else
					Reporting.AddCapacityLine("Weld Capacity = " + WWeldCap + " << " + R + ConstUnit.Force + " (NG)", R / WWeldCap, "Weld Capacity", beam.MemberType);

				if (webTee.H != 0)
				{
					Reporting.AddHeader("Check Tee Stem for Tension");
					capacity = ConstNum.FIOMEGA0_9N * webTee.Material.Fy * webTee.SLength * webTee.Size.tw;
					if (capacity >= webTee.H)
						Reporting.AddCapacityLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = 0.9 Fy * L * t = " + ConstString.FIOMEGA0_9 + " * " + webTee.Material.Fy + " * " + webTee.SLength + " * " + webTee.Size.tw + " = " + capacity + " >= " + webTee.H + ConstUnit.Force + " (OK)", webTee.H / capacity, "Tee Stem for Tension", beam.MemberType);
					else
						Reporting.AddCapacityLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = 0.9 Fy * L * t = " + ConstString.FIOMEGA0_9 + " * " + webTee.Material.Fy + " * " + webTee.SLength + " * " + webTee.Size.tw + " = " + capacity + " << " + webTee.H + ConstUnit.Force + " (NG)", webTee.H / capacity, "Tee Stem for Tension", beam.MemberType);
				}

				Reporting.AddGoToHeader(ConstString.DES_OR_ALLOWABLE + " Shear Strength of Beam Web");
				MiscCalculationsWithReporting.BlockShearForWelded(beam.MemberType, ref BsCap, EShearCarriedBy.Tee);
				if (BsCap >= beam.ShearForce)
					Reporting.AddCapacityLine("= " + BsCap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / BsCap, "Shear Strength of Beam Web", beam.MemberType);
				else
					Reporting.AddCapacityLine("= " + BsCap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / BsCap, "Shear Strength of Beam Web", beam.MemberType);

				if (beam.WinConnect.Beam.TopCope || beam.WinConnect.Beam.BottomCope)
					Cope.CopedBeam(beam.MemberType, true);
			}

			Reporting.AddGoToHeader(ConstString.DES_OR_ALLOWABLE + " Shear Strength of Tee Stem:");
			Ag = webTee.SLength * webTee.Size.tw;
			Reporting.AddHeader("Shear Yielding " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
			Reporting.AddLine("Gross Area (Ag) = L * t = " + webTee.SLength + " * " + webTee.Size.tw + " = " + Ag + ConstUnit.Area);
			beam.WinConnect.Fema.Vg = ConstNum.FIOMEGA1_0N * 0.6 * Ag * webTee.Material.Fy;
			if (beam.WinConnect.Fema.Vg >= V)
				Reporting.AddCapacityLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA1_0 + " * 0.6 * Ag * Fy = " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + Ag + " * " + webTee.Material.Fy + " = " + beam.WinConnect.Fema.Vg + " >= " + V + ConstUnit.Force + " (OK)", V / beam.WinConnect.Fema.Vg, "Shear Strength of Tee Stem", beam.MemberType);
			else
				Reporting.AddCapacityLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA1_0 + " * 0.6 * Ag * Fy = " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + Ag + " * " + webTee.Material.Fy + " = " + beam.WinConnect.Fema.Vg + " << " + V + ConstUnit.Force + " (NG)", V / beam.WinConnect.Fema.Vg, "Shear Strength of Tee Stem", beam.MemberType);

			if (webTee.BeamSideConnection == EConnectionStyle.Bolted || webTee.OSLConnection == EConnectionStyle.Bolted)
				Reporting.AddHeader("Shear Rupture " + ConstString.DES_OR_ALLOWABLE + " Strength");

			if (webTee.BeamSideConnection == EConnectionStyle.Bolted && webTee.OSLConnection == EConnectionStyle.Bolted)
			{
				AnOsl = (webTee.SLength - webTee.FlangeNumberOfRows * (webTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH)) * webTee.Size.tf;
				Anw = (webTee.SLength - webTee.StemNumberOfRows * (webTee.BoltWebOnStem.HoleWidth + ConstNum.SIXTEENTH_INCH)) * webTee.Size.tw;
				An = Math.Min(AnOsl, Anw);
				Reporting.AddHeader("Net Area on Flange (An1):");
				Reporting.AddLine("= (L - n * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t = (" + webTee.SLength + " - " + webTee.FlangeNumberOfRows + " * (" + webTee.BoltOslOnFlange.HoleWidth + "+" + ConstNum.SIXTEENTH_INCH + "))* " + webTee.Size.tw + " = " + AnOsl + ConstUnit.Area);

				Reporting.AddHeader("Net Area on Beam Side Leg (An2):");
				Reporting.AddLine("= (L - n * dh) * t = (" + webTee.SLength + " - " + webTee.StemNumberOfRows + " * (" + webTee.BoltWebOnStem.HoleWidth + "+" + ConstNum.SIXTEENTH_INCH + "))* " + webTee.Size.tf + " = " + Anw + ConstUnit.Area);
				Reporting.AddLine("An = Min(An1, An2)= " + An + ConstUnit.Area);
			}
			else if (webTee.BeamSideConnection == EConnectionStyle.Bolted && webTee.OSLConnection == EConnectionStyle.Welded)
			{
				An = (webTee.SLength - webTee.StemNumberOfRows * (webTee.BoltWebOnStem.HoleWidth + ConstNum.SIXTEENTH_INCH)) * webTee.Size.tw;
				Reporting.AddLine("Net Area on Beam Side Leg (An):");
				Reporting.AddLine("= (L - n * ( dh + " + ConstNum.SIXTEENTH_INCH + ")) * t = (" + webTee.SLength + " - " + webTee.StemNumberOfRows + " * (" + webTee.BoltWebOnStem.HoleWidth + "+" + ConstNum.SIXTEENTH_INCH + "))* " + webTee.Size.tw + " = " + Anw + ConstUnit.Area);
				Reporting.AddLine("An = " + An + ConstUnit.Area);
			}
			else if (webTee.BeamSideConnection == EConnectionStyle.Welded && webTee.OSLConnection == EConnectionStyle.Bolted)
			{
				An = (webTee.SLength - webTee.FlangeNumberOfRows * (webTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH)) * webTee.Size.tf;
				Reporting.AddHeader("Net Area on Flange (An):");
				Reporting.AddLine("= (L - n * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t = (" + webTee.SLength + " - " + webTee.FlangeNumberOfRows + " (* " + webTee.BoltOslOnFlange.HoleWidth + "+" + ConstNum.SIXTEENTH_INCH + "))* " + webTee.Size.tf + " = " + AnOsl + ConstUnit.Area);
				Reporting.AddLine("An = " + An + ConstUnit.Area);
			}

			if (webTee.BeamSideConnection == EConnectionStyle.Bolted || webTee.OSLConnection == EConnectionStyle.Bolted)
			{
				Vn = ConstNum.FIOMEGA0_75N * 0.6 * An * webTee.Material.Fu;
				if (Vn >= V)
					Reporting.AddCapacityLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.6 * An * Fu = " + ConstString.FIOMEGA0_75 + " * 0.6 * " + An + " * " + webTee.Material.Fu + " = " + Vn + " >= " + V + ConstUnit.Force + " (OK)", V / Vn, "Rn", beam.MemberType);
				else
					Reporting.AddCapacityLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.6 * An * Fu = " + ConstString.FIOMEGA0_75 + " * 0.6 * " + An + " * " + webTee.Material.Fu + " = " + Vn + " << " + V + ConstUnit.Force + " (NG)", V / Vn, "Rn", beam.MemberType);
			}
			if (webTee.BeamSideConnection == EConnectionStyle.Bolted)
			{
				Reporting.AddGoToHeader("Block Shear Strength of Tee Stem:");

				Lgt = webTee.BoltWebOnStem.EdgeDistTransvDir;
				Lnt = webTee.BoltWebOnStem.EdgeDistTransvDir - (webTee.BoltWebOnStem.HoleLength + ConstNum.SIXTEENTH_INCH) / 2;
				Lgv = (webTee.StemNumberOfRows - 1) * webTee.StemSpacingLongDir + webTee.StemEdgeDistLongDir;
				Lnv = Lgv - (webTee.StemNumberOfRows - 0.5) * (webTee.BoltWebOnStem.HoleWidth + ConstNum.SIXTEENTH_INCH);

				Reporting.AddHeader("Gross Length with Tension resistance (Lgt) = Lh = " + Lgt + ConstUnit.Length);
				Reporting.AddLine("Net Length with Tension resistance (Lnt)");
				Reporting.AddLine("= Lgt - (dh + " + ConstNum.SIXTEENTH_INCH + ") / 2 = " + Lgt + " - " + (webTee.BoltOslOnFlange.HoleLength + ConstNum.SIXTEENTH_INCH) + " / 2 = " + Lnt + ConstUnit.Length);

				Reporting.AddHeader("Gross Length with Shear resistance (Lgv)");
				Reporting.AddLine("= (n - 1) * s + Lv");
				Reporting.AddLine("= (" + webTee.StemNumberOfRows + " - 1) * " + webTee.StemSpacingLongDir + " + " + webTee.StemEdgeDistLongDir + " = " + Lgv + ConstUnit.Length);

				Reporting.AddHeader("Net Length with Shear resistance (Lnv)");
				Reporting.AddLine("= Lgv - (n - 0.5) * (dv + " + ConstNum.SIXTEENTH_INCH + ")");
				Reporting.AddLine("= " + Lgv + " - (" + webTee.StemNumberOfRows + " - 0.5) * " + (webTee.BoltWebOnStem.HoleWidth + ConstNum.SIXTEENTH_INCH) + ")");
				Reporting.AddLine("= " + Lnv + ConstUnit.Length);

				PhiRn = MiscCalculationsWithReporting.BlockShearNew(webTee.Material.Fu, Lnv, 1, Lnt, Lgv, webTee.Material.Fy, webTee.Size.tw, webTee.V, true);

				Reporting.AddGoToHeader(ConstString.DES_OR_ALLOWABLE + " Flexural Strength of Tee Stem:");
				a = beam.EndSetback + beam.WinConnect.Beam.Lh;
				n = webTee.StemNumberOfRows;
				s = webTee.StemSpacingLongDir;
				beam.WinConnect.Fema.L = webTee.SLength;
				if (RigidSupport)
					ecc = a - webTee.Size.kdes;
				else
					ecc = a;

				PhiRn = ConstNum.FIOMEGA0_9N * webTee.Size.tw * webTee.Material.Fy * Math.Pow(webTee.SLength, 2) / (4 * ecc);
				Reporting.AddHeader("Flexural Yielding:");
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * tw * Fy * L² / (4 * e)");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + webTee.Size.tw + " * " + webTee.Material.Fy + " * " + webTee.SLength + "² / (4 * " + ecc + ")");
				if (PhiRn >= webTee.V)
					Reporting.AddCapacityLine("= " + PhiRn + " >= " + webTee.V + ConstUnit.Force + " (OK)", webTee.V / PhiRn, "Flexural Yielding", memberType);
				else
					Reporting.AddCapacityLine("= " + PhiRn + " << " + webTee.V + ConstUnit.Force + " (NG)", webTee.V / PhiRn, "Flexural Yielding", memberType);

				Reporting.AddHeader("Flexural Rupture:");
				if (!RigidSupport)
				{
					PhiRn = ConstNum.FIOMEGA0_75N * webTee.Material.Fu * webTee.Size.tw * (Math.Pow(beam.WinConnect.Fema.L, 2) - Math.Pow(s, 2) * n * (Math.Pow(n, 2) - 1) * (webTee.BoltWebOnStem.HoleWidth + ConstNum.SIXTEENTH_INCH) / beam.WinConnect.Fema.L) / (4 * ecc);
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Fu * tw * (L² - (s² * N * (N² - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ")) / L) / (4 * e)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + webTee.Material.Fu + " * " + webTee.Size.tw + "*(" + beam.WinConnect.Fema.L + "² - (" + s + "² * " + n + " * (" + n + "² - 1) * (" + webTee.BoltWebOnStem.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) / " + beam.WinConnect.Fema.L + ") / (4 * " + ecc + ")");
				}
				else
				{
					PhiRn = ConstNum.FIOMEGA0_75N * webTee.Material.Fu * webTee.Size.tw * Math.Pow(beam.WinConnect.Fema.L, 2) / (4 * ecc);
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Fu * tw * (L² ) / (4 * e)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + webTee.Material.Fu + " * " + webTee.Size.tw + "*(" + beam.WinConnect.Fema.L + "² )/(4*" + ecc + ")");
				}
				if (PhiRn >= webTee.V)
					Reporting.AddCapacityLine("= " + PhiRn + " >= " + webTee.V + ConstUnit.Force + " (OK)", webTee.V / PhiRn, "Flexural Rupture", memberType);
				else
					Reporting.AddCapacityLine("= " + PhiRn + " << " + webTee.V + ConstUnit.Force + " (NG)", webTee.V / PhiRn, "Flexural Rupture", memberType);
			}
			if (webTee.OSLConnection == EConnectionStyle.Bolted)
			{
				Reporting.AddHeader("Block Shear Strength of Tee Flange:");
				Lgt = webTee.BoltOslOnFlange.EdgeDistTransvDir;
				Lnt = webTee.BoltOslOnFlange.EdgeDistTransvDir - (webTee.BoltOslOnFlange.HoleLength + ConstNum.SIXTEENTH_INCH) / 2;
				Lgv = (webTee.FlangeNumberOfRows - 1) * webTee.FlangeSpacingLongDir + webTee.FlangeEdgeDistLongDir;
				Lnv = Lgv - (webTee.FlangeNumberOfRows - 0.5) * (webTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH);

				Reporting.AddHeader("Gross Length with Tension resistance, Lgt = Lh = " + Lgt + ConstUnit.Length);
				Reporting.AddLine("Net Length with Tension resistance, Lnt");
				Reporting.AddLine("= Lgt - (dh+" + ConstNum.SIXTEENTH_INCH + ") / 2 = " + Lgt + " - " + (webTee.BoltOslOnFlange.HoleLength + ConstNum.SIXTEENTH_INCH) + " / 2 = " + Lnt + ConstUnit.Length);

				Reporting.AddHeader("Gross Length with Shear resistance, Lgv");
				Reporting.AddLine("= (n - 1) * s + Lv");
				Reporting.AddLine("= (" + webTee.FlangeNumberOfRows + " - 1) * " + webTee.FlangeSpacingLongDir + " + " + webTee.FlangeEdgeDistLongDir + " = " + Lgv + ConstUnit.Length);

				Reporting.AddHeader("Net Length with Shear resistance, Lnv");
				Reporting.AddLine("= Lgv - (n - 0.5) * (dv+" + ConstNum.SIXTEENTH_INCH + ")");
				Reporting.AddLine("= " + Lgv + " - (" + webTee.FlangeNumberOfRows + " - 0.5) * " + (webTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH) + ")");
				Reporting.AddLine("= " + Lnv + ConstUnit.Length);

				PhiRn = MiscCalculationsWithReporting.BlockShearNew(webTee.Material.Fu, Lnv, 1, Lnt, Lgv, webTee.Material.Fy, webTee.Size.tf, webTee.V, true);
			}

			if (webTee.H != 0)
				Stiff.ColumnLocalStress(memberType);
		}

		private static void GetGage(EMemberType memberType)
		{
			double ColumnGageMin = 0;
			double ColumnGageMax = 0;
			int Rnbs = 0;
			int Lnbs = 0;
			double RColumnGageMin = 0;
			double LColumnGageMin = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BeamToHSSColumn:
				case EJointConfiguration.BeamToColumnFlange:
					ColumnGageMin = beam.GageOnColumn;
					ColumnGageMax = beam.GageOnColumn;
					break;
				case EJointConfiguration.BeamToColumnWeb:
				case EJointConfiguration.BeamToGirder:
					if (rightBeam.WinConnect.ShearWebTee.BeamSideConnection == EConnectionStyle.Bolted)
						Rnbs = 2;
					else
						Rnbs = 1;
					if (leftBeam.WinConnect.ShearWebTee.BeamSideConnection == EConnectionStyle.Bolted)
						Lnbs = 2;
					else
						Lnbs = 1;
					RColumnGageMin = rightBeam.Shape.tw + 2 * (ConstNum.HALF_INCH + MiscCalculationsWithReporting.AngleGageMinimum(rightBeam.WinConnect.ShearWebTee.BoltOslOnFlange.BoltSize, Rnbs));
					LColumnGageMin = leftBeam.Shape.tw + 2 * (ConstNum.HALF_INCH + MiscCalculationsWithReporting.AngleGageMinimum(leftBeam.WinConnect.ShearWebTee.BoltOslOnFlange.BoltSize, Lnbs));
					ColumnGageMin = Math.Max(RColumnGageMin, LColumnGageMin);
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
						ColumnGageMax = column.Shape.t - 2 * Math.Max(rightBeam.WinConnect.ShearWebTee.BoltOslOnFlange.EdgeDistTransvDir, leftBeam.WinConnect.ShearWebTee.BoltOslOnFlange.EdgeDistTransvDir);
					else
						ColumnGageMax = 1000;

					ColumnGageMin = beam.GageOnColumn;
					ColumnGageMax = beam.GageOnColumn;
					break;
				default:
					if (beam.GageOnColumn > 0)
					{
						ColumnGageMin = beam.GageOnColumn;
						ColumnGageMax = beam.GageOnColumn;
					}
					else
					{
						ColumnGageMin = ConstNum.THREE_INCHES;
						ColumnGageMax = 8 * ConstNum.ONE_INCH;
					}
					break;
			}
		}

		private static void GetNumberofOslBolts(EMemberType memberType, double MinL, ref int MinBolts, bool RigidSupport, ref double ecc, ref double ruv, ref double rut, ref double FiRn_perBolt)
		{
			bool DoAgain;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var webTee = beam.WinConnect.ShearWebTee;

			MinBolts = Math.Max(2, (int)(Math.Floor((MinL - 2 * webTee.FlangeEdgeDistLongDir) / webTee.FlangeSpacingLongDir) + 1));
			if (RigidSupport || webTee.H > 0 && !webTee.BoltOslOnFlange.NumberOfBolts_User)
			{
				if (RigidSupport)
					ecc = beam.EndSetback + beam.WinConnect.Beam.Lh;
				else
					ecc = 0;
				webTee.FlangeNumberOfRows = (int)Math.Max(MinBolts, BoltsForTension.CalcBoltsForTension(memberType, webTee.BoltOslOnFlange, webTee.H, webTee.V, 0, true));
				do
				{
					MiscCalculationsWithReporting.OSLBoltsWithoutPlaneEccentricity(memberType, webTee.V, ((int)webTee.H), webTee.FlangeNumberOfRows, ((int)webTee.FlangeSpacingLongDir), ecc, ref ruv, ref rut);
					FiRn_perBolt = BoltsForTension.CalcBoltsForTension(memberType, webTee.BoltOslOnFlange, webTee.H, webTee.V, (2 * webTee.FlangeNumberOfRows), true);
					DoAgain = false;
					if (rut > FiRn_perBolt)
					{
						webTee.FlangeNumberOfRows++;
						DoAgain = true;
					}
				} while (DoAgain);
			}
			else if (!webTee.BoltOslOnFlange.NumberOfBolts_User)
				webTee.FlangeNumberOfRows = (int)Math.Max(MinBolts, Math.Ceiling(beam.ShearForce / (2 * webTee.BoltOslOnFlange.BoltStrength)));
		}

		private static void GetNumberOfWBolts(EMemberType memberType, double MinL, ref int MinBolts, double MaxL, bool RigidSupport, ref double ecc, double Rload, ref int n, ref double c, ref double Cap, ref double BsCap, double TenCap, ref double BeamTearOutCap)
		{
			double maxbolts;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var webTee = beam.WinConnect.ShearWebTee;

			MinBolts = Math.Max(2, ((int)Math.Floor((MinL - 2 * webTee.StemEdgeDistLongDir) / webTee.StemSpacingLongDir)) + 1);
			maxbolts = ((int)Math.Floor((MaxL - 2 * webTee.StemEdgeDistLongDir) / webTee.StemSpacingLongDir)) + 1;
			if (RigidSupport)
				ecc = 0;
			else
				ecc = beam.EndSetback + beam.WinConnect.Beam.Lh;
			n = (int)Math.Ceiling(Rload / webTee.BoltWebOnStem.BoltStrength);
			if (!webTee.BoltWebOnStem.NumberOfBolts_User)
			{
				webTee.StemNumberOfRows = (int)(Math.Max(MinBolts, n) - 1);
				do
				{
					webTee.StemNumberOfRows++;
					MiscCalculationsWithReporting.EccentricBolt(ecc, webTee.StemSpacingLongDir, 0, 1, webTee.StemNumberOfRows, ref c);
					Cap = c * webTee.BoltWebOnStem.BoltStrength;
				} while (Cap < Rload);

				do
				{
					MiscCalculationsWithReporting.BlockShear(memberType, EShearOpt.WebTee, ref BsCap, false);
					if (BsCap < beam.ShearForce)
						webTee.StemNumberOfRows++;
				} while (!(webTee.StemNumberOfRows >= maxbolts || BsCap >= beam.ShearForce));
				if (beam.Moment == 0)
				{
					do
					{
						BeamTearOutCap = MiscCalculationsWithReporting.BeamWebTearout(beam.MemberType, webTee.StemNumberOfRows, 1, webTee.StemSpacingLongDir, 0, EBeamOrAttachment.Beam, ETearOutBlockShear.TearOut, false);
						if (BeamTearOutCap < beam.P)
							webTee.StemNumberOfRows++;
					} while (!(webTee.StemNumberOfRows >= maxbolts || BeamTearOutCap >= beam.P));
				}
			}
		}

		private static void GetLengthForWebBolted(EMemberType memberType, ref double evmin, ref double LengthWB, double MinL, double MaxL, ref bool errorcondition)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];
			var webTee = beam.WinConnect.ShearWebTee;

			if (webTee.Size.tw == 0)
				webTee.Size.tw = ConstNum.THREE_SIXTEENTHS;
			evmin = MiscCalculationsWithReporting.MinimumEdgeDist(webTee.BoltWebOnStem.BoltSize, webTee.V / webTee.StemNumberOfRows, webTee.Material.Fu * webTee.Size.tw, webTee.BoltWebOnStem.MinEdgeSheared, (int)webTee.BoltWebOnStem.HoleWidth, webTee.BoltWebOnStem.HoleType);
			evmin = Math.Max(evmin, webTee.BoltWebOnStem.MinEdgeSheared);
			evmin = NumberFun.Round(evmin, 4);
			LengthWB = (webTee.StemNumberOfRows - 1) * webTee.StemSpacingLongDir + 2 * evmin;
			if (webTee.BoltStagger != EBoltStagger.None)
				LengthWB = (LengthWB + webTee.StemSpacingLongDir / 2);
			if (LengthWB < MinL && !webTee.BoltWebOnStem.NumberOfBolts_User)
			{
				webTee.StemNumberOfRows++;
				evmin = MiscCalculationsWithReporting.MinimumEdgeDist(webTee.BoltWebOnStem.BoltSize, webTee.V / webTee.StemNumberOfRows, webTee.Material.Fu * webTee.Size.tw, webTee.BoltWebOnStem.MinEdgeSheared, (int)webTee.BoltWebOnStem.HoleWidth, webTee.BoltWebOnStem.HoleType);
				evmin = NumberFun.Round(evmin, 4);
				LengthWB = (webTee.StemNumberOfRows - 1) * webTee.StemSpacingLongDir + 2 * evmin;
				if (webTee.BoltStagger != EBoltStagger.None)
					LengthWB = (LengthWB + webTee.StemSpacingLongDir / 2);
			}
			//if (LengthWB > MaxL)
			//{
			//	errorcondition = true;
			//	Reporting.AddLine("Too many bolts. (NG)");
			//}
		}

		private static void GetLengthforOslBolted(EMemberType memberType, ref double LengthOB, double MinL, double MaxL, ref bool errorcondition)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];
			var webTee = beam.WinConnect.ShearWebTee;

			LengthOB = (webTee.FlangeNumberOfRows - 1) * webTee.FlangeSpacingLongDir + 2 * webTee.FlangeEdgeDistLongDir; // * ShearedEdgeforAngle(fbolts.d)  'fbolts.eal
			if (webTee.BoltStagger != EBoltStagger.None)
				LengthOB = (LengthOB + webTee.FlangeSpacingLongDir / 2);
			if (LengthOB < MinL && !webTee.BoltOslOnFlange.NumberOfBolts_User)
			{
				webTee.FlangeNumberOfRows++;
				LengthOB = (webTee.FlangeNumberOfRows - 1) * webTee.FlangeSpacingLongDir + 2 * webTee.FlangeEdgeDistLongDir; // * ShearedEdgeforAngle(fbolts.d)  'fbolts.eal
				if (webTee.BoltStagger != EBoltStagger.None)
					LengthOB = (LengthOB + webTee.FlangeSpacingLongDir / 2);
			}
			if (LengthOB > MaxL)
			{
				errorcondition = true;
				Reporting.AddLine("Too many bolts. (NG)");
			}
		}

		private static void GetRequiredShearArea(EMemberType memberType, ref double AgReq, ref double Anreq)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];
			var webTee = beam.WinConnect.ShearWebTee;

			AgReq = webTee.V / (ConstNum.FIOMEGA1_0N * 0.6 * webTee.Material.Fy);
			Anreq = webTee.V / (ConstNum.FIOMEGA0_75N * 0.6 * webTee.Material.Fu);
		}

		private static void GetRequiredThforOslBearing(EMemberType memberType, ref double HoleSize, ref double Fbe, ref double Fbs, ref double BearingCap1, ref double tOslBReq)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];
			var webTee = beam.WinConnect.ShearWebTee;

			HoleSize = webTee.BoltOslOnFlange.HoleLength;
			Fbe = CommonCalculations.EdgeBearing(webTee.FlangeEdgeDistLongDir, webTee.BoltOslOnFlange.HoleWidth, webTee.BoltOslOnFlange.BoltSize, webTee.Material.Fu, webTee.BoltOslOnFlange.HoleType, false);
			Fbs = CommonCalculations.SpacingBearing(webTee.FlangeSpacingLongDir, webTee.BoltOslOnFlange.HoleWidth, webTee.BoltOslOnFlange.BoltSize, webTee.BoltOslOnFlange.HoleType, webTee.Material.Fu, false);
			BearingCap1 = 2 * (Fbe + Fbs * (webTee.FlangeNumberOfRows - 1));
			tOslBReq = webTee.V / BearingCap1;
		}

		private static void GetRequiredThforWBearing(EMemberType memberType, ref double HoleSize, ref double Fbe, ref double Fbs, ref double BearingCap1, ref double tWBreq)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];
			var webTee = beam.WinConnect.ShearWebTee;

			HoleSize = webTee.BoltWebOnStem.HoleLength;
			Fbe = CommonCalculations.EdgeBearing(webTee.StemEdgeDistLongDir, webTee.BoltWebOnStem.HoleWidth, webTee.BoltWebOnStem.BoltSize, webTee.Material.Fu, webTee.BoltWebOnStem.HoleType, false);
			Fbs = CommonCalculations.SpacingBearing(webTee.StemSpacingLongDir, webTee.BoltWebOnStem.HoleWidth, webTee.BoltWebOnStem.BoltSize, webTee.BoltWebOnStem.HoleType, webTee.Material.Fu, false);
			BearingCap1 = Fbe + Fbs * (webTee.StemNumberOfRows - 1);
			tWBreq = webTee.V / BearingCap1;
		}

		private static void GetLengthforWebWelded(EMemberType memberType, ref double w, double Rload, ref double LengthWW, double MaxL, ref bool errorcondition, double MinL)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];

			w = ConstNum.FIOMEGA0_75N * 0.6 * beam.Shape.tw * beam.Material.Fu / (0.707 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
			if (w > NumberFun.ConvertFromFraction(5))
				w = NumberFun.ConvertFromFraction(5);
			if (CommonDataStatic.Units == EUnit.US)
				LengthWW = -(Math.Floor(-8 * (Rload / w - 60) / 26)) / 8.0;
			else
				LengthWW = -(Math.Floor(-(Rload / w - 10507) / 179.25));

			if (LengthWW > MaxL)
			{
				errorcondition = true;
				Reporting.AddLine("Tee Length too long. (NG)");
			}
			if (LengthWW < MinL)
				LengthWW = MinL;
		}

		private static void GetThicknessForBlockShear(EMemberType memberType, double reOsl, ref double Lgt, ref double Lnt, ref double Lgv, ref double Lnv, ref double tReqforBlockShearofFlange, ref double tReqforBlockShearofStem, double shapeDValue, double beamEndSetback, bool enableReporting)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];
			var webTee = beam.WinConnect.ShearWebTee;

			if (webTee.OSLConnection == EConnectionStyle.Bolted)
			{
				Lgt = Math.Max(reOsl, (CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].GageOnFlange - beam.Shape.tw) / 2);
				Lnt = (Lgt - (webTee.BoltOslOnFlange.HoleLength + ConstNum.SIXTEENTH_INCH) / 2);
				Lgv = (webTee.FlangeNumberOfRows - 1) * webTee.FlangeSpacingLongDir + webTee.FlangeEdgeDistLongDir;
				Lnv = Lgv - (webTee.FlangeNumberOfRows - 0.5) * (webTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH);
				tReqforBlockShearofFlange = webTee.V / MiscCalculationsWithReporting.BlockShearNew(webTee.Material.Fu, Lnv, 1, Lnt, Lgv, webTee.Material.Fy, 1, 0, enableReporting);

			}
			else
				tReqforBlockShearofFlange = 0;
			if (webTee.BeamSideConnection == EConnectionStyle.Bolted)
			{
				Lgt = shapeDValue - (beamEndSetback + beam.WinConnect.Beam.Lh);
				Lnt = (Lgt - (webTee.BoltWebOnStem.HoleLength + ConstNum.SIXTEENTH_INCH) / 2);
				Lgv = (webTee.StemNumberOfRows - 1) * webTee.StemSpacingLongDir + webTee.StemEdgeDistLongDir;
				Lnv = Lgv - (webTee.StemNumberOfRows - 0.5) * (webTee.BoltWebOnStem.HoleWidth + ConstNum.SIXTEENTH_INCH);
				tReqforBlockShearofStem = webTee.V / MiscCalculationsWithReporting.BlockShearNew(webTee.Material.Fu, Lnv, 1, Lnt, Lgv, webTee.Material.Fy, 1, 0, enableReporting);
			}
			else
				tReqforBlockShearofStem = 0;
		}

		private static void GetLengthforOslWelded(EMemberType memberType, ref double BF, double SupThickness, double SupFy, ref double LengthOslW, double MaxL, double MinL)
		{
			double t1;
			double e;
			double L2;
			double P1;
			double w1;
			double L3;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var webTee = beam.WinConnect.ShearWebTee;

			if (webTee.Size.tf == 0)
				t1 = NumberFun.ConvertFromFraction(5);
			else
				t1 = webTee.Size.tf;
			if (webTee.Size.bf == 0)
				e = 3;
			else
			{
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
				{
					if (column.WebOrientation == EWebOrientation.OutOfPlane)
						BF = (column.Shape.d - 3 * column.Shape.tf) / 2;
					else
						BF = (column.Shape.bf - 3 * column.Shape.tf) / 2;
					e = Math.Min(webTee.Size.bf / 2 + webTee.Size.tw / 2, BF / 2 - beam.Shape.tw / 2);
				}
				else
					e = webTee.Size.bf / 2 + webTee.Size.tw / 2;
			}

			beam.WinConnect.Fema.L1 = (webTee.V / (ConstNum.FIOMEGA1_0N * 0.6 * SupThickness * SupFy));
			L2 = webTee.V / (ConstNum.FIOMEGA1_0N * 0.6 * t1 * webTee.Material.Fy);
			P1 = webTee.V;
			w1 = Math.Max(ConstNum.QUARTER_INCH, webTee.WeldSizeFlange);
			L3 = Math.Sqrt((Math.Pow(P1, 2) + Math.Sqrt(Math.Pow(P1, 4) + 21 * Math.Pow(w1 * e * CommonDataStatic.Preferences.DefaultElectrode.Fexx * P1, 2))) / (0.81 * Math.Pow(CommonDataStatic.Preferences.DefaultElectrode.Fexx * w1, 2)));
			LengthOslW = NumberFun.Round(Math.Max(Math.Max(beam.WinConnect.Fema.L1, L2), L3), 8);

			if (LengthOslW > MaxL)
				LengthOslW = MaxL;
			if (LengthOslW < MinL)
				LengthOslW = MinL;
		}
	}
}