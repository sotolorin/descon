using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class ClipAngle
	{
		internal static void DesignClipAngles(EMemberType memberType, ref double MaxL, ref double MinL)
		{
			string capacityText;
			double WWeldCap = 0;
			double BendingCap;
			double OslWeldCap;
			double BearingCap;
			double FiRn;
			double ehmin ;
			double evmin;
			double R;
			double V;
			double Znet = 0;
			double ZnetDeduction = 0;
			double d = 0;
			double n1 = 0;
			int nn = 0;
			double Zx = 0;
			double Mu = 0;
			double BearingCap1 = 0;
			double Fbs = 0;
			double si = 0;
			double Fbe = 0;
			double HoleSize = 0;
			double e = 0;
			double BeamBlockShearCap = 0;
			double BeamTearOutCap = 0;
			double BsCap = 0;
			double ecc = 0;
			double Cap = 0;
			double c = 0;
			double agage = 0;
			int MinBolts = 0;
			double h;
			double PhiRn2;
			double PhiRn;
			double blockShearWithWeld = 0;
			double Lgv = 0;
			double Lgt = 0;
			double Lnv = 0;
			double Lnt = 0;
			double webd;
			double WeldCap = 0;
			double OslWeldCapacity;
			double RT;
			double w = 0;
			double Cnst;
			double theta;
			double a;
			double k;
			double C1;
			double TensionCap;
			double FiRn_perBolt = 0;
			double tReqforBending = 0;
			double gmin;
			double g;
			double edg;
			double B;
			double columnGageMax = 0;
			double columnGageMin = 0;
			double osl;
			double AgReq = 0;
			double Anreq = 0;
			double Ag;
			double An = 0;
			double AnOsl = 0;
			double tReqforBlockShearofWebLeg = 0;
			double tReqforBlockShear;
			double WLeg = 0;
			double OslLeg = 0;
			double Anw = 0;
			double t = 0;
			double boltd;
			double OSLRange2 = 0;
			double OSLRange1 = 0;
			double BF;
			double E1 = 0;
			double FuT = 0;
			double tReqforBlockShearofOSL = 0;
			double tOslBReq = 0;
			double tWBreq = 0;
			double tReq = 0;
			int n = 0;
			double LengthOB = 0;
			double LengthWB = 0;
			double Rload;
			double sew;
			double rew = 0;
			double reOsl = 0;
			double minboltdisttotop;
			double SupportFu = 0;
			double SupFy = 0;
			double SupThickness1 = 0;
			double SupThickness = 0;
			double mnweld = 0;
			double MxWeld;
			double lfortension;
			double LengthWW = 0;
			double LengthOslW = 0;

			DetailData beam, oppositeBeam;
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

			if (memberType == EMemberType.LeftBeam)
			{
				beam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
				oppositeBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			}
			else
			{
				beam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
				oppositeBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			}

			if (!beam.EndSetback_User)
				beam.EndSetback = oppositeBeam.EndSetback = ConstNum.HALF_INCH;

			if (!beam.WinConnect.Beam.Lh_User)
				beam.WinConnect.Beam.Lh = ConstNum.ONEANDHALF_INCHES;
			if (!oppositeBeam.WinConnect.Beam.Lh_User)
				oppositeBeam.WinConnect.Beam.Lh = ConstNum.ONEANDHALF_INCHES;

			var shearClipAngle = beam.WinConnect.ShearClipAngle;

			// Reset to avoid this value increasing over and over
			if (!shearClipAngle.BoltOslOnSupport.EdgeDistLongDir_User)
				shearClipAngle.BoltOslOnSupportEdgeDistanceLong = shearClipAngle.BoltOslOnSupport.MinEdgeSheared;
			if (!shearClipAngle.BoltWebOnBeam.EdgeDistLongDir_User)
				shearClipAngle.BoltWebOnBeam.EdgeDistLongDir = shearClipAngle.BoltWebOnBeam.MinEdgeSheared;
			if (!shearClipAngle.BoltOnColumn.EdgeDistLongDir_User)
				shearClipAngle.BoltOnColumn.EdgeDistLongDir = shearClipAngle.BoltOnColumn.MinEdgeSheared;
			if (!shearClipAngle.BoltOnGusset.EdgeDistLongDir_User)
				shearClipAngle.BoltOnGusset.EdgeDistLongDir = shearClipAngle.BoltOnGusset.MinEdgeSheared;

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BeamToHSSColumn:
				case EJointConfiguration.BeamToColumnFlange:
					SupThickness = column.Shape.tf;
					SupThickness1 = column.Shape.tf;
					SupFy = column.Material.Fy;
					SupportFu = column.Material.Fu;
					break;
				case EJointConfiguration.BeamToGirder:
				case EJointConfiguration.BeamToColumnWeb:
					//if (rightBeam.WinConnect.ShearClipAngle.BoltOslOnSupport.NumberOfBolts == leftBeam.WinConnect.ShearClipAngle.BoltOslOnSupport.NumberOfBolts)
					//{
					//	if (memberType == EMemberType.LeftBeam)
					//		SupThickness = column.Shape.tw * Math.Abs(leftBeam.ShearForce) / (Math.Abs(leftBeam.ShearForce) + Math.Abs(rightBeam.ShearForce));
					//	else
					//		SupThickness = column.Shape.tw * Math.Abs(rightBeam.ShearForce) / (Math.Abs(leftBeam.ShearForce) + Math.Abs(rightBeam.ShearForce));
					//}
					//else
						MiscCalculationsWithReporting.EffectiveSupThickness(memberType, ref SupThickness, ref SupThickness1);

					SupFy = beam.Material.Fy;
					SupportFu = beam.Material.Fu;
					break;
				case EJointConfiguration.BeamSplice:
					SupThickness = shearClipAngle.Size.t;
					SupThickness1 = shearClipAngle.Size.t;
					if (SupThickness <= 0)
						SupThickness = ConstNum.QUARTER_INCH;
					SupFy = shearClipAngle.Material.Fy;
					SupportFu = shearClipAngle.Material.Fu;
					break;
			}

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder && shearClipAngle.BeamSideConnection == EConnectionStyle.Welded &&
			    shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted && beam.WinConnect.Beam.TopCope && shearClipAngle.Position == EPosition.Top)
			{
				minboltdisttotop = beam.WinConnect.Beam.TCopeD + shearClipAngle.WeldSizeBeam + ConstNum.SIXTEENTH_INCH + shearClipAngle.BoltOslOnSupportEdgeDistanceLong;
				if (shearClipAngle.BoltOslOnSupport.EdgeDistGusset < minboltdisttotop && !shearClipAngle.BoltOslOnSupport.EdgeDistGusset_User)
					shearClipAngle.BoltOslOnSupport.EdgeDistGusset = minboltdisttotop;
			}
			if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToGirder && shearClipAngle.BeamSideConnection == EConnectionStyle.Welded &&
			    shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted && beam.WinConnect.Beam.TopCope && shearClipAngle.Position == EPosition.Top)
			{
				minboltdisttotop = column.Shape.kdet + shearClipAngle.WeldSizeBeam + ConstNum.SIXTEENTH_INCH + shearClipAngle.BoltOslOnSupportEdgeDistanceLong;
				if (shearClipAngle.BoltOslOnSupport.EdgeDistGusset < minboltdisttotop && !shearClipAngle.BoltOslOnSupport.EdgeDistGusset_User)
					shearClipAngle.BoltOslOnSupport.EdgeDistGusset = minboltdisttotop;
			}

			MaxL = NumberFun.Round(MaxL, 8);

			if (beam.MomentConnection == EMomentCarriedBy.Tee)
				MinL = shearClipAngle.BoltOslOnSupport.SpacingLongDir + 2 * shearClipAngle.BoltOslOnSupportEdgeDistanceLong;
			else
				MinL = NumberFun.Round(MinL, 8);

			// Commented out < comparisons below becuase the calculations rely on those values being set (MT - 2/6/15)
			if (shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
			{
				double seOsl;

				reOsl = shearClipAngle.BoltOslOnSupport.MinEdgeRolled;
				seOsl = MiscCalculationsWithReporting.ShearedEdgeforAngle(shearClipAngle.BoltOslOnSupport.BoltSize);
				shearClipAngle.BoltOslOnSupport.EdgeDistTransvDir = seOsl;

				if (shearClipAngle.BoltOslOnSupportEdgeDistanceLong < seOsl && !shearClipAngle.BoltOslOnSupport.EdgeDistLongDir_User)
					shearClipAngle.BoltOslOnSupportEdgeDistanceLong = seOsl;
			}
			if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted)
			{
				rew = shearClipAngle.BoltOslOnSupport.MinEdgeRolled;
				sew = shearClipAngle.BoltWebOnBeam.MinEdgeSheared;
				//if (rew > shearClipAngle.BoltWebOnBeam.EdgeDistTransvDir)
				shearClipAngle.BoltWebOnBeam.EdgeDistTransvDir = rew;
				//if (beam.WinConnect.Beam.Lh < sew)
				if (!beam.WinConnect.Beam.Lh_User && beam.WinConnect.Beam.Lh < sew)
					beam.WinConnect.Beam.Lh = sew;
				sew = MiscCalculationsWithReporting.ShearedEdgeforAngle(shearClipAngle.BoltWebOnBeam.BoltSize);
				if (!shearClipAngle.BoltWebOnBeam.EdgeDistLongDir_User)
					shearClipAngle.BoltWebOnBeam.EdgeDistLongDir = sew;
			}

			Rload = Math.Sqrt(Math.Pow(shearClipAngle.h, 2) + Math.Pow(shearClipAngle.V, 2));

			if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted && shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
			{
				GetGage(beam.MemberType, ref columnGageMin, ref columnGageMax);
				GetNumberOfOSLBolts(beam.MemberType, MinL, ref MinBolts, ref FiRn_perBolt, ref n, ref agage, ref c, ref Cap);
				GetNumberOfWebBolts(beam.MemberType, MinL, ref MinBolts, MaxL, Rload, ref n, ref ecc, ref c, ref Cap, ref BsCap, ref BeamTearOutCap, ref BeamBlockShearCap);
				GetLengthForWebBolted(beam.MemberType, ref LengthWB, MinL);
				GetLengthForOSLBolted(beam.MemberType, ref LengthOB, MinL);
				shearClipAngle.Length = Math.Max(LengthWB, LengthOB);

				if (shearClipAngle.Length > LengthWB && !shearClipAngle.BoltWebOnBeam.NumberOfRows_User && !shearClipAngle.BoltWebOnBeam.EdgeDistLongDir_User)
				{
					if (shearClipAngle.BoltStagger == EBoltStagger.None)
					{
						shearClipAngle.BoltWebOnBeam.NumberOfRows = ((int)Math.Floor((shearClipAngle.Length - 2 * shearClipAngle.BoltWebOnBeam.EdgeDistLongDir) / shearClipAngle.BoltWebOnBeam.SpacingLongDir)) + 1;
						shearClipAngle.BoltWebOnBeam.EdgeDistLongDir = 0.5 * (shearClipAngle.Length - (shearClipAngle.BoltWebOnBeam.NumberOfRows - 1) * shearClipAngle.BoltWebOnBeam.SpacingLongDir);
					}
					else
					{
						shearClipAngle.BoltWebOnBeam.NumberOfRows = ((int)Math.Floor((shearClipAngle.Length - shearClipAngle.BoltWebOnBeam.SpacingLongDir / 2 - 2 * shearClipAngle.BoltWebOnBeam.EdgeDistLongDir) / shearClipAngle.BoltWebOnBeam.SpacingLongDir)) + 1;
						shearClipAngle.BoltWebOnBeam.EdgeDistLongDir = 0.5 * (shearClipAngle.Length - shearClipAngle.BoltWebOnBeam.SpacingLongDir / 2 - (shearClipAngle.BoltWebOnBeam.NumberOfRows - 1) * shearClipAngle.BoltWebOnBeam.SpacingLongDir);
					}
				}
				if (shearClipAngle.Length > LengthOB && !shearClipAngle.BoltOslOnSupport.NumberOfRows_User && !shearClipAngle.BoltOslOnSupport.EdgeDistLongDir_User)
				{
					if (shearClipAngle.BoltStagger == EBoltStagger.None)
					{
						n = ((int)Math.Floor((shearClipAngle.Length - 2 * shearClipAngle.BoltOslOnSupportEdgeDistanceLong) / shearClipAngle.BoltOslOnSupport.SpacingLongDir)) + 1;
						if (n > shearClipAngle.BoltOslOnSupport.NumberOfRows && !shearClipAngle.BoltOslOnSupport.EdgeDistLongDir_User)
						{
							shearClipAngle.BoltOslOnSupport.NumberOfRows = n;
							shearClipAngle.BoltOslOnSupportEdgeDistanceLong = 0.5 * (shearClipAngle.Length - (shearClipAngle.BoltOslOnSupport.NumberOfRows - 1) * shearClipAngle.BoltOslOnSupport.SpacingLongDir);
						}
					}
					else
					{
						n = (int)Math.Floor((shearClipAngle.Length - shearClipAngle.BoltOslOnSupport.SpacingLongDir / 2 - 2 * shearClipAngle.BoltOslOnSupportEdgeDistanceLong) / shearClipAngle.BoltOslOnSupport.SpacingLongDir) + 1;
						if (n > shearClipAngle.BoltOslOnSupport.NumberOfRows && !shearClipAngle.BoltOslOnSupport.EdgeDistLongDir_User)
						{
							shearClipAngle.BoltOslOnSupport.NumberOfRows = n;
							shearClipAngle.BoltOslOnSupportEdgeDistanceLong = 0.5 * (shearClipAngle.Length - shearClipAngle.BoltOslOnSupport.SpacingLongDir / 2 - (shearClipAngle.BoltOslOnSupport.NumberOfRows - 1) * shearClipAngle.BoltOslOnSupport.SpacingLongDir);
						}
					}
				}

				GetRequiredShearArea(beam.MemberType, ref AgReq, ref Anreq);
				GetRequiredThForOSLBearing(beam.MemberType, ref HoleSize, ref Fbe, ref si, ref Fbs, ref BearingCap1, ref tOslBReq);
				GetRequiredThForWebBearing(beam.MemberType, ref HoleSize, ref Fbe, ref si, ref Fbs, ref BearingCap1, ref tWBreq);
				tReq = Math.Max(tWBreq, tOslBReq);
			}
			else if (shearClipAngle.BeamSideConnection == EConnectionStyle.Welded && shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
			{
				GetGage(beam.MemberType, ref columnGageMin, ref columnGageMax);
				shearClipAngle.BoltWebOnBeam.NumberOfRows = 0;
				GetNumberOfOSLBolts(beam.MemberType, MinL, ref MinBolts, ref FiRn_perBolt, ref n, ref agage, ref c, ref Cap);
				GetLengthForWebWelded(beam.MemberType, ref w, Rload, ref LengthWW, MaxL, MinL);
				GetLengthForOSLBolted(beam.MemberType, ref LengthOB, MinL);
				shearClipAngle.Length = Math.Max(LengthWW, LengthOB);

				if (shearClipAngle.Length > LengthOB)
				{
					n = ((int)Math.Ceiling((shearClipAngle.Length - 2 * shearClipAngle.BoltOslOnSupportEdgeDistanceLong) / shearClipAngle.BoltOslOnSupport.SpacingLongDir)) + 1;
					beam.WinConnect.Fema.L = (2 * shearClipAngle.BoltOslOnSupportEdgeDistanceLong + shearClipAngle.BoltOslOnSupport.SpacingLongDir * (n - 1));
					if (beam.WinConnect.Fema.L <= MaxL)
					{
						if (!shearClipAngle.BoltOslOnSupport.NumberOfRows_User)
							shearClipAngle.BoltOslOnSupport.NumberOfRows = n;
						shearClipAngle.Length = beam.WinConnect.Fema.L;
					}
					shearClipAngle.Length = LengthOB;
				}
				GetRequiredShearArea(beam.MemberType, ref AgReq, ref Anreq);
				GetRequiredThForOSLBearing(beam.MemberType, ref HoleSize, ref Fbe, ref si, ref Fbs, ref BearingCap1, ref tOslBReq);
				GetThicknessForBlockShear(beam.MemberType, reOsl, OslLeg, ref Lgt, ref Lnt, ref Lgv, ref Lnv, ref tReqforBlockShearofOSL, WLeg, ref tReqforBlockShearofWebLeg, false);

				tReq = Math.Max(tOslBReq, tReqforBlockShearofOSL);
			}
			else if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted && shearClipAngle.SupportSideConnection == EConnectionStyle.Welded)
			{
				shearClipAngle.BoltOslOnSupport.NumberOfRows = 0;
				GetNumberOfWebBolts(beam.MemberType, MinL, ref MinBolts, MaxL, Rload, ref n, ref ecc, ref c, ref Cap, ref BsCap, ref BeamTearOutCap, ref BeamBlockShearCap);
				GetLengthForWebBolted(beam.MemberType, ref LengthWB, MinL);
				shearClipAngle.Length = Math.Max(LengthWB, LengthOslW);
				if (shearClipAngle.Length > LengthWB)
				{
					n = ((int)Math.Ceiling((shearClipAngle.Length - 2 * shearClipAngle.BoltWebOnBeam.EdgeDistLongDir) / shearClipAngle.BoltWebOnBeam.SpacingLongDir)) + 1;
					beam.WinConnect.Fema.L = (2 * shearClipAngle.BoltWebOnBeam.EdgeDistLongDir + shearClipAngle.BoltWebOnBeam.SpacingLongDir * (n - 1));
					if (beam.WinConnect.Fema.L <= MaxL && !shearClipAngle.BoltWebOnBeam.NumberOfRows_User)
					{
						shearClipAngle.BoltWebOnBeam.NumberOfRows = n;
						shearClipAngle.Length = beam.WinConnect.Fema.L;
					}
					shearClipAngle.Length = LengthWB;
				}
				GetRequiredShearArea(beam.MemberType, ref AgReq, ref Anreq);
				GetRequiredThForWebBearing(beam.MemberType, ref HoleSize, ref Fbe, ref si, ref Fbs, ref BearingCap1, ref tWBreq);
				tReq = tWBreq;
			}
			else if (shearClipAngle.BeamSideConnection == EConnectionStyle.Welded && shearClipAngle.SupportSideConnection == EConnectionStyle.Welded)
			{
				tReq = 0;
				shearClipAngle.BoltWebOnBeam.NumberOfRows = 0;
				shearClipAngle.BoltOslOnSupport.NumberOfRows = 0;
				GetLengthForWebWelded(beam.MemberType, ref w, Rload, ref LengthWW, MaxL, MinL);
				GetLengthForOSLWelded(beam.MemberType, t, ref e, SupThickness, SupFy, ref LengthOslW, MaxL, MinL);
				shearClipAngle.Length = Math.Max(LengthWW, LengthOslW);
				GetRequiredShearArea(beam.MemberType, ref AgReq, ref Anreq);
			}
			if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted)
			{
				rew = shearClipAngle.BoltWebOnBeam.MinEdgeRolled;
				if (shearClipAngle.Size.t == 0)
					FuT = shearClipAngle.Material.Fu * 0.5;
				else
					FuT = shearClipAngle.Material.Fu * shearClipAngle.Size.t;

				E1 = MiscCalculationsWithReporting.MinimumEdgeDist(shearClipAngle.BoltWebOnBeam.BoltSize, shearClipAngle.h / shearClipAngle.BoltWebOnBeam.NumberOfRows, FuT, rew, shearClipAngle.BoltWebOnBeam.HoleLength, shearClipAngle.BoltWebOnBeam.HoleType);

				if (E1 > shearClipAngle.BoltWebOnBeam.EdgeDistTransvDir)
					shearClipAngle.BoltWebOnBeam.EdgeDistTransvDir = E1;
				if (!beam.WinConnect.Beam.Lh_User && beam.WinConnect.Beam.Lh < shearClipAngle.BoltWebOnBeam.MinEdgeSheared)
					beam.WinConnect.Beam.Lh = shearClipAngle.BoltWebOnBeam.MinEdgeSheared;
				sew = MiscCalculationsWithReporting.ShearedEdgeforAngle(shearClipAngle.BoltWebOnBeam.BoltSize);
				if (shearClipAngle.BoltWebOnBeam.EdgeDistLongDir < sew && !shearClipAngle.BoltWebOnBeam.EdgeDistLongDir_User)
					shearClipAngle.BoltWebOnBeam.EdgeDistLongDir = sew;
			}

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
			{
				BF = column.Shape.B;

				// May need to change the term in parenthases to 'column.Shape.tnom * 2.25' based on p.1-5 of AISC
				OSLRange1 = BF / 2 - (column.Shape.tf * 1.5) - shearClipAngle.WeldSizeSupport - beam.Shape.tw / 2;
				OSLRange2 = BF / 2 - beam.Shape.tw / 2;
			}

			Shape tempAngle;

			foreach (var angle in CommonDataStatic.ShapesSingleAngle)
			{
				if (angle.Value.Name == ConstString.NONE)
					continue;

				if (shearClipAngle.Size_User)
					tempAngle = shearClipAngle.Size.ShallowCopy();
				else
					tempAngle = angle.Value.ShallowCopy();

				var shortLeg = tempAngle.b < tempAngle.d ? tempAngle.b : tempAngle.d;
				var longLeg = tempAngle.b > tempAngle.d ? tempAngle.b : tempAngle.d;

				// Not sure where the following few lines came from. SupThickness is based on column value, and is set at the beginning of DesignClipAngles().  -RM 10/29/2014
				//SupThickness = tempAngle.t;
				//if (SupThickness <= 0)
				//    SupThickness = ConstNum.QUARTER_INCH;
				//SupThickness1 = SupThickness;

				//shearClipAngle.Size = tempAngle;

				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
				{
					if (shearClipAngle.OSL == EOSL.ShortLeg)
					{
						if (shortLeg > OSLRange1 && shortLeg < OSLRange2)
							continue;
					}
					else
					{
						if (longLeg > OSLRange1 && longLeg < OSLRange2)
							continue;
					}
				}

				boltd = 0;
				if (shearClipAngle.Number == 1)
				{
					if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted && shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
						boltd = Math.Max(shearClipAngle.BoltWebOnBeam.BoltSize, shearClipAngle.BoltOslOnSupport.BoltSize);
					else if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted)
						boltd = shearClipAngle.BoltWebOnBeam.BoltSize;
					else if (shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
						boltd = shearClipAngle.BoltOslOnSupport.BoltSize;
				}
				if (boltd == 0)
					beam.MinThickness = 0;
				else if (boltd <= NumberFun.ConvertFromFraction(14))
					beam.MinThickness = ConstNum.THREE_EIGHTS_INCH;
				else
					beam.MinThickness = ConstNum.HALF_INCH;

				if (shearClipAngle.Number == 1 && tempAngle.t < beam.MinThickness)
					continue;
				if (shearClipAngle.OSL == EOSL.ShortLeg)
				{
					if (shearClipAngle.Number == 1 && shearClipAngle.SupportSideConnection == EConnectionStyle.Welded && shortLeg != ConstNum.THREE_INCHES)
						continue;
				}
				else
				{
					if (shearClipAngle.Number == 1 && shearClipAngle.SupportSideConnection == EConnectionStyle.Welded && longLeg != ConstNum.THREE_INCHES)
						continue;
				}
				if (shearClipAngle.Number == 2)
				{
					if (shearClipAngle.OSL == EOSL.ShortLeg)
					{
						if (shortLeg < ConstNum.FOUR_INCHES && shearClipAngle.Length >= 4.5 * ConstNum.FOUR_INCHES)
							continue;
					}
					else
					{
						if (longLeg < ConstNum.FOUR_INCHES && shearClipAngle.Length >= 4.5 * ConstNum.FOUR_INCHES)
							continue;
					}
				}
				switch (shearClipAngle.BeamSideConnection)
				{
					case EConnectionStyle.Welded:
						if (shearClipAngle.SupportSideConnection == EConnectionStyle.Welded)
						{
							if (shearClipAngle.OSL == EOSL.ShortLeg)
							{
								if (longLeg < ConstNum.THREE_INCHES)
									continue;
							}
							else
							{
								if (shortLeg < ConstNum.THREE_INCHES)
									continue;
							}
						}
						else
						{
							if (shearClipAngle.OSL == EOSL.ShortLeg)
							{
								if (longLeg < ConstNum.THREEANDHALF_INCHES)
									continue;
							}
							else
							{
								if (shortLeg < ConstNum.THREEANDHALF_INCHES)
									continue;
							}
						}

						mnweld = CommonCalculations.MinimumWeld(tempAngle.t, beam.Shape.tw);
						if (tempAngle.t >= ConstNum.QUARTER_INCH)
							MxWeld = tempAngle.t - ConstNum.SIXTEENTH_INCH;
						else
							MxWeld = tempAngle.t;
						if (mnweld > MxWeld)
							continue;
						t = tempAngle.t;
						Anw = shearClipAngle.Length * t;
						break;
					case EConnectionStyle.Bolted:
						if (shearClipAngle.OSL == EOSL.ShortLeg)
						{
							if (longLeg < beam.EndSetback + beam.WinConnect.Beam.Lh + E1 || longLeg < ConstNum.THREEANDHALF_INCHES)
								continue;
						}
						else
						{
							if (shortLeg < beam.EndSetback + beam.WinConnect.Beam.Lh + E1 || shortLeg < ConstNum.THREEANDHALF_INCHES)
								continue;
						}
						t = tempAngle.t;
						Anw = (shearClipAngle.Length - shearClipAngle.BoltWebOnBeam.NumberOfRows *
						       (shearClipAngle.BoltWebOnBeam.HoleDiameterSTD + ConstNum.SIXTEENTH_INCH)) * t;
						break;
				}
				if (shearClipAngle.OSL == EOSL.ShortLeg)
				{
					OslLeg = shortLeg;
					WLeg = longLeg;
				}
				else
				{
					OslLeg = longLeg;
					WLeg = shortLeg;
				}

				GetThicknessForBlockShear(beam.MemberType, reOsl, OslLeg, ref Lgt, ref Lnt, ref Lgv, ref Lnv, ref tReqforBlockShearofOSL, WLeg, ref tReqforBlockShearofWebLeg, false);
				tReqforBlockShear = Math.Max(tReqforBlockShearofOSL, tReqforBlockShearofWebLeg);
				if (tReqforBlockShear > tempAngle.t)
					continue;
				switch (shearClipAngle.SupportSideConnection)
				{
					case EConnectionStyle.Welded:
						mnweld = CommonCalculations.MinimumWeld(tempAngle.t, SupThickness1);
						if (tempAngle.t >= ConstNum.QUARTER_INCH)
							MxWeld = tempAngle.t - ConstNum.SIXTEENTH_INCH;
						else
							MxWeld = tempAngle.t;
						if (mnweld > MxWeld)
							continue;
						t = tempAngle.t;
						AnOsl = shearClipAngle.Length * t;
						GetLengthForOSLWelded(beam.MemberType, t, ref e, SupThickness, SupFy, ref LengthOslW, MaxL, MinL);
						break;
					case EConnectionStyle.Bolted:
						t = tempAngle.t;
						AnOsl = (shearClipAngle.Length - shearClipAngle.BoltOslOnSupport.NumberOfRows * (shearClipAngle.BoltOslOnSupport.HoleWidth + ConstNum.SIXTEENTH_INCH)) * t;
						break;
				}

				An = Math.Min(AnOsl, Anw);
				Ag = shearClipAngle.Length * t;
				if (tempAngle.t < tReq || An < Anreq || Ag < AgReq)
					continue;

				osl = shearClipAngle.OSL == EOSL.LongLeg ? longLeg : shortLeg;

				if (shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
				{
					double ta;

					//if (!beam.GageOnColumn_User)
					//{
					//	if (columnGageMin == columnGageMax)
					//		beam.GageOnColumn = columnGageMin;
					//	else
					//		beam.GageOnColumn = beam.Shape.tw + 2 * (t + MiscCalculationsWithReporting.AngleGageMinimum(shearClipAngle.BoltOslOnSupport.BoltSize, nbs));
						
					//	beam.GageOnColumn = NumberFun.Round(beam.GageOnColumn, 2);
					//}

					if (2 * (osl - reOsl) + beam.Shape.tw < beam.GageOnColumn)
						continue;

					B = (beam.GageOnColumn - beam.Shape.tw) / 2 - t / 2;
					edg = osl - t / 2 - B;
					shearClipAngle.BoltOslOnSupport.EdgeDistTransvDir = edg;
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange)
						edg = Math.Min(edg, (column.Shape.bf - beam.GageOnColumn) / 2);

					if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted)
					{
						g = shortLeg - E1;
						if (shearClipAngle.BoltStagger == EBoltStagger.None)
							gmin = tempAngle.kdet + shearClipAngle.BoltWebOnBeam.BoltSize;
						else
							gmin = MiscCalculationDataMethods.StaggeredBoltGage(Math.Max(shearClipAngle.BoltWebOnBeam.BoltSize, shearClipAngle.BoltOslOnSupport.BoltSize), (Math.Max(shearClipAngle.BoltWebOnBeam.SpacingLongDir, shearClipAngle.BoltOslOnSupport.SpacingLongDir) / 2), tempAngle.t);
						if (!beam.WinConnect.Beam.Lh_User && gmin > beam.EndSetback + beam.WinConnect.Beam.Lh)
							beam.WinConnect.Beam.Lh = gmin - beam.EndSetback;
						if (g < gmin)
							continue;
					}
					if (shearClipAngle.Number == 1)
					{
						GetThicknessForBending(beam.MemberType, beam.GageOnColumn, ref e, ref Mu, ref Zx, ref nn, ref n1, ref d, n, ref ZnetDeduction, ref Znet, ref tReqforBending);
						if (tReqforBending > tempAngle.t)
							continue;
					}

					ta = MiscCalculationsWithReporting.HangerStrength(beam.MemberType, shearClipAngle.BoltOslOnSupport.HoleWidth, shearClipAngle.Material.Fu, t, edg, B, FiRn_perBolt, 0, false);
					TensionCap = shearClipAngle.BoltOslOnSupport.NumberOfRows * ta;

					Lnt = shearClipAngle.BoltOslOnSupport.EdgeDistTransvDir - (shearClipAngle.BoltOslOnSupport.HoleLength + ConstNum.SIXTEENTH_INCH) / 2;
					Lgv = (shearClipAngle.BoltOslOnSupport.NumberOfRows - 1) * shearClipAngle.BoltOslOnSupport.SpacingLongDir + shearClipAngle.BoltOslOnSupportEdgeDistanceLong;
					Lnv = Lgv - (shearClipAngle.BoltOslOnSupport.NumberOfRows - 0.5) * (shearClipAngle.BoltOslOnSupport.HoleWidth + ConstNum.SIXTEENTH_INCH);

					PhiRn = ConstNum.FIOMEGA0_75N * (0.6 * Math.Min(shearClipAngle.Material.Fu * Lnv, shearClipAngle.Material.Fy * Lgv) + 1 * shearClipAngle.Material.Fu * Lnt) * tempAngle.t;
					if (PhiRn < shearClipAngle.V)
						continue;
				}
				else // Support side = Welded
				{
					TensionCap = ConstNum.FIOMEGA0_9N * shearClipAngle.Material.Fy * shearClipAngle.Length * Math.Pow(tempAngle.t, 2) / (4 * (shearClipAngle.LengthOfOSL - tempAngle.kdet));
					if (TensionCap < shearClipAngle.h)
					{
						lfortension = -((int)Math.Floor(-shearClipAngle.Length / TensionCap * shearClipAngle.h));
						lfortension = NumberFun.Round(lfortension, 8);
						if (lfortension > MaxL)
							shearClipAngle.Length = MaxL;
						if (lfortension < MaxL)
						{
							if (shearClipAngle.BeamSideConnection == EConnectionStyle.Welded)
								shearClipAngle.Length = lfortension;
							else
							{
								if (shearClipAngle.Length < lfortension)
									shearClipAngle.Length = lfortension;
							}

							if (!shearClipAngle.BoltWebOnBeam.NumberOfRows_User && !shearClipAngle.BoltWebOnBeam.EdgeDistLongDir_User && shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted)
							{
								if (lfortension < shearClipAngle.Length + shearClipAngle.BoltWebOnBeam.SpacingLongDir)
								{
									shearClipAngle.BoltWebOnBeam.EdgeDistLongDir = shearClipAngle.BoltWebOnBeam.EdgeDistLongDir + (lfortension - shearClipAngle.Length) / 2;
									shearClipAngle.Length = lfortension;
								}
								else
								{
									shearClipAngle.BoltWebOnBeam.NumberOfRows++;
									shearClipAngle.Length = lfortension;
									shearClipAngle.BoltWebOnBeam.EdgeDistLongDir = (shearClipAngle.Length - (shearClipAngle.BoltWebOnBeam.NumberOfRows - 1) * shearClipAngle.BoltWebOnBeam.SpacingLongDir) / 2;
								}
							}
							if (!shearClipAngle.BoltOslOnSupport.NumberOfRows_User && !shearClipAngle.BoltWebOnBeam.EdgeDistLongDir_User && shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
							{
								if (lfortension < shearClipAngle.Length + shearClipAngle.BoltOslOnSupport.SpacingLongDir && !shearClipAngle.BoltOslOnSupport.EdgeDistLongDir_User)
								{
									shearClipAngle.BoltOslOnSupportEdgeDistanceLong = shearClipAngle.BoltOslOnSupportEdgeDistanceLong + (lfortension - shearClipAngle.Length) / 2;
									shearClipAngle.Length = lfortension;
								}
								else
								{
									shearClipAngle.BoltOslOnSupport.NumberOfRows = shearClipAngle.BoltOslOnSupport.NumberOfRows + 1;
									shearClipAngle.Length = lfortension;
									if (!shearClipAngle.BoltOslOnSupport.EdgeDistLongDir_User)
										shearClipAngle.BoltOslOnSupportEdgeDistanceLong = (shearClipAngle.Length - (shearClipAngle.BoltOslOnSupport.NumberOfRows - 1) * shearClipAngle.BoltOslOnSupport.SpacingLongDir) / 2;
								}
							}
							TensionCap = ConstNum.FIOMEGA0_9N * shearClipAngle.Material.Fy * shearClipAngle.Length * Math.Pow(tempAngle.t, 2) / (4 * (shearClipAngle.LengthOfOSL - tempAngle.kdet));
						}
					}
					mnweld = CommonCalculations.MinimumWeld(tempAngle.t, SupThickness1);
					C1 = CommonCalculations.WeldTypeRatio();
					if (shearClipAngle.Number == 1)
					{
						k = shearClipAngle.LengthOfOSL / shearClipAngle.Length;
						a = beam.Shape.tw / (2 * shearClipAngle.Length) + k - Math.Pow(k, 2) / (2 * (1 + k));
						theta = shearClipAngle.h / shearClipAngle.V;
						Cnst = EccentricWeld.GetEccentricWeld(k, a, theta, false);
						//EccentricWeld.GetEccentricWeld(k, a, ref Cnst, theta, false);
						w = (int)Math.Floor(Rload / (Cnst * C1 * shearClipAngle.Length)) / 16.0;
						if (!shearClipAngle.WeldSizeSupport_User)
							shearClipAngle.WeldSizeSupport = Math.Max(mnweld, w);
					}
					else if (!shearClipAngle.WeldSizeSupport_User)
						shearClipAngle.WeldSizeSupport = Math.Max(mnweld, MiscCalculationsWithReporting.OslWeld(beam.MemberType, Rload, false));
					if (tempAngle.t >= ConstNum.QUARTER_INCH)
						MxWeld = tempAngle.t - ConstNum.SIXTEENTH_INCH;
					else
						MxWeld = tempAngle.t;
					if (shearClipAngle.WeldSizeSupport > MxWeld)
						continue;

					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
					{
						if (beam.IsActive && (shearClipAngle.WeldSizeSupport == 0 || !shearClipAngle.WeldSizeSupport_User))
							shearClipAngle.WeldSizeSupport = ConstNum.EIGHTH_INCH;
						if (!beam.IsActive)
							shearClipAngle.WeldSizeSupport = 0;
						RT = 1.414 * SupportFu * SupThickness1 / ((shearClipAngle.WeldSizeSupport + shearClipAngle.WeldSizeSupport) * shearClipAngle.Weld.Fexx);
						if (!oppositeBeam.WinConnect.ShearClipAngle.WeldSizeSupport_User)
							oppositeBeam.WinConnect.ShearClipAngle.WeldSizeSupport = shearClipAngle.WeldSizeSupport;
					}
					else
						RT = 1.414 * SupportFu * SupThickness1 / (shearClipAngle.WeldSizeSupport * shearClipAngle.Weld.Fexx);
					if (RT > 1)
						RT = 1;
					if (shearClipAngle.Number == 1)
					{
						k = shearClipAngle.LengthOfOSL / shearClipAngle.Length;
						a = beam.Shape.tw / (2 * shearClipAngle.Length) + k - Math.Pow(k, 2) / (2 * (1 + k));
						theta = shearClipAngle.h / shearClipAngle.V;
						Cnst = EccentricWeld.GetEccentricWeld(k, a, theta, false);
						//EccentricWeld.GetEccentricWeld(k, a, ref Cnst, theta, false);
						OslWeldCapacity = RT * 16 * shearClipAngle.WeldSizeSupport * Cnst * C1 * shearClipAngle.Length;
					}
					else
						OslWeldCapacity = RT * MiscCalculationsWithReporting.OslWeld(beam.MemberType, 0, false);

					if (OslWeldCapacity < Rload)
					{
						if (shearClipAngle.Length < (MaxL - ConstNum.QUARTER_INCH))
							shearClipAngle.Length = shearClipAngle.Length + ConstNum.QUARTER_INCH;
						else
							continue;
					}
				}

				if (TensionCap < shearClipAngle.h)
					continue;
				if (shearClipAngle.BeamSideConnection == EConnectionStyle.Welded)
				{
					bool needToTryWeld = true;
					double Lnv2;
					double Aw;
					double beamLv;

					while (needToTryWeld)
					{
						MiscCalculationsWithReporting.ClipWelds(beam.MemberType, ref WeldCap, false);
						WeldCap = NumberFun.Round(WeldCap, 8);

						webd = beam.Shape.d - beam.WinConnect.Beam.TCopeD - beam.WinConnect.Beam.BCopeD;

						if (beam.WinConnect.Beam.TopCope && beam.WinConnect.Beam.BottomCope)
						{
							beamLv = beam.WinConnect.Beam.FrontY + column.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (shearClipAngle.FrontY + shearClipAngle.Length / 2);
							Lnt = longLeg + shortLeg - shearClipAngle.LengthOfOSL - beam.EndSetback;
							Lnv = shearClipAngle.Length + beamLv;
							Lgt = Lnt;
							Lgv = shearClipAngle.Length + beamLv;
							Lnv2 = webd;
							PhiRn = MiscCalculationsWithReporting.BlockShearNew(beam.Material.Fu, Lnv, 1, Lnt, Lgv, beam.Material.Fy, beam.Shape.tw, 0, false);
							PhiRn2 = ConstNum.FIOMEGA0_75N * 0.6 * beam.Material.Fu * Lnv2 * beam.Shape.tw;

							blockShearWithWeld = Math.Min(PhiRn, PhiRn2);
						}
						else if (beam.WinConnect.Beam.TopCope)
						{
							beamLv = beam.WinConnect.Beam.FrontY + column.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (shearClipAngle.FrontY + shearClipAngle.Length / 2);
							Lnt = longLeg + shortLeg - shearClipAngle.LengthOfOSL - beam.EndSetback;
							Lnv = shearClipAngle.Length + beamLv;
							Lgt = Lnt;
							Lgv = shearClipAngle.Length + beamLv;
							Lnv2 = webd;
							PhiRn = MiscCalculationsWithReporting.BlockShearNew(beam.Material.Fu, Lnv, 1, Lnt, Lgv, beam.Material.Fy, beam.Shape.tw, 0, false);

							blockShearWithWeld = PhiRn;
						}
						else
						{
							h = column.Shape.d - column.Shape.tf - Math.Max(beam.WinConnect.Beam.BCopeD, column.Shape.tf);
							Aw = beam.Shape.tw * webd;
							blockShearWithWeld = MiscCalculationsWithReporting.GrossShearStrength(beam.Material.Fy, Aw, memberType, false);
						}

						if (WeldCap / shearClipAngle.Number < Rload || blockShearWithWeld < shearClipAngle.Number * Rload)
						{
							if (shearClipAngle.Length <= (MaxL - ConstNum.QUARTER_INCH))
							{
								shearClipAngle.Length = shearClipAngle.Length + ConstNum.QUARTER_INCH;
								needToTryWeld = true;
							}
							else
								needToTryWeld = false;
						}
						else
							needToTryWeld = false;
					}
					if (WeldCap / shearClipAngle.Number < Rload && blockShearWithWeld < shearClipAngle.Number * Rload)
						continue;
				}

				if (tempAngle.t < CommonDataStatic.Preferences.MinThicknessAngle)
					continue;

				shearClipAngle.Size = tempAngle.ShallowCopy();

				break;
			}

			if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted && !shearClipAngle.BoltOslOnSupport.EdgeDistLongDir_User &&
				!shearClipAngle.BoltOslOnSupport.EdgeDistTransvDir_User)
			{
				shearClipAngle.BoltWebOnBeam.EdgeDistTransvDir = MiscCalculationsWithReporting.MinimumEdgeDist(shearClipAngle.BoltWebOnBeam.BoltSize, shearClipAngle.h / shearClipAngle.BoltWebOnBeam.NumberOfRows, FuT, rew, shearClipAngle.BoltWebOnBeam.HoleLength, shearClipAngle.BoltWebOnBeam.HoleType);
				shearClipAngle.BoltWebOnBeam.EdgeDistTransvDir = shearClipAngle.OppositeOfOSL - beam.EndSetback - beam.WinConnect.Beam.Lh;
				shearClipAngle.BoltWebOnBeam.EdgeDistBrace = beam.WinConnect.Beam.Lh;
				if (shearClipAngle.BoltStagger == EBoltStagger.None)
					shearClipAngle.BoltWebOnBeam.EdgeDistLongDir = (shearClipAngle.Length - (shearClipAngle.BoltWebOnBeam.NumberOfRows - 1) * shearClipAngle.BoltWebOnBeam.SpacingLongDir) / 2;
				else
					shearClipAngle.BoltWebOnBeam.EdgeDistLongDir = (shearClipAngle.Length - shearClipAngle.BoltWebOnBeam.SpacingLongDir / 2 - (shearClipAngle.BoltWebOnBeam.NumberOfRows - 1) * shearClipAngle.BoltWebOnBeam.SpacingLongDir) / 2;
			}

			if (shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted && !shearClipAngle.BoltOslOnSupport.EdgeDistLongDir_User)
			{
				if (shearClipAngle.BoltStagger == EBoltStagger.None)
					shearClipAngle.BoltOslOnSupportEdgeDistanceLong = (shearClipAngle.Length - (shearClipAngle.BoltOslOnSupport.NumberOfRows - 1) * shearClipAngle.BoltOslOnSupport.SpacingLongDir) / 2;
				else
					shearClipAngle.BoltOslOnSupportEdgeDistanceLong = (shearClipAngle.Length - shearClipAngle.BoltOslOnSupport.SpacingLongDir / 2 - (shearClipAngle.BoltOslOnSupport.NumberOfRows - 1) * shearClipAngle.BoltOslOnSupport.SpacingLongDir) / 2;
			}

			if (beam.WinConnect.Beam.TopCope || beam.WinConnect.Beam.BottomCope)
				Cope.CopedBeam(beam.MemberType, false);

			switch (shearClipAngle.Position)
			{
				case EPosition.Top:
					if (shearClipAngle.BeamSideConnection == EConnectionStyle.Welded)
					{
						if (shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
							shearClipAngle.FrontY = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.DistanceToFirstBolt - beam.WinConnect.Beam.TCopeD + shearClipAngle.BoltOslOnSupportEdgeDistanceLong - shearClipAngle.Length / 2;
						else
							shearClipAngle.FrontY = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.Shape.kdet - shearClipAngle.Length / 2;
					}
					else
						shearClipAngle.FrontY = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.DistanceToFirstBolt - beam.WinConnect.Beam.TCopeD + shearClipAngle.BoltWebOnBeam.EdgeDistLongDir - shearClipAngle.Length / 2;
					break;
				case EPosition.Center:
					shearClipAngle.FrontY = beam.WinConnect.Beam.FrontY;
					break;
				case EPosition.Bottom:
					shearClipAngle.FrontY = beam.WinConnect.Beam.FrontY - beam.Shape.d / 2 + beam.Shape.kdet + shearClipAngle.Length / 2;
					break;
			}

			Reporting.AddMainHeader(beam.ComponentName);
			Reporting.AddHeader("Shear Connection Using Clip Angle(s):");

			switch (shearClipAngle.BoltStagger)
			{
				case EBoltStagger.None:
					shearClipAngle.WTop = shearClipAngle.BoltWebOnBeam.EdgeDistLongDir;
					shearClipAngle.WBot = shearClipAngle.BoltWebOnBeam.EdgeDistLongDir;
					shearClipAngle.OSLTop = shearClipAngle.BoltOslOnSupportEdgeDistanceLong;
					shearClipAngle.OSLBot = shearClipAngle.BoltOslOnSupportEdgeDistanceLong;
					break;
				case EBoltStagger.Support:
					shearClipAngle.WTop = shearClipAngle.BoltWebOnBeam.EdgeDistLongDir;
					shearClipAngle.WBot = shearClipAngle.BoltWebOnBeam.EdgeDistLongDir + shearClipAngle.BoltWebOnBeam.SpacingLongDir / 2;
					shearClipAngle.OSLTop = shearClipAngle.BoltOslOnSupportEdgeDistanceLong + shearClipAngle.BoltOslOnSupport.SpacingLongDir / 2;
					shearClipAngle.OSLBot = shearClipAngle.BoltOslOnSupportEdgeDistanceLong;
					break;
				case EBoltStagger.Beam:
					shearClipAngle.WTop = shearClipAngle.BoltWebOnBeam.EdgeDistLongDir + shearClipAngle.BoltWebOnBeam.SpacingLongDir / 2;
					shearClipAngle.WBot = shearClipAngle.BoltWebOnBeam.EdgeDistLongDir;
					shearClipAngle.OSLTop = shearClipAngle.BoltOslOnSupportEdgeDistanceLong;
					shearClipAngle.OSLBot = shearClipAngle.BoltOslOnSupportEdgeDistanceLong + shearClipAngle.BoltOslOnSupport.SpacingLongDir / 2;
					break;
				case EBoltStagger.OneLessRow:
					shearClipAngle.WTop = shearClipAngle.BoltWebOnBeam.EdgeDistLongDir;
					shearClipAngle.WBot = shearClipAngle.BoltWebOnBeam.EdgeDistLongDir;
					shearClipAngle.OSLTop = shearClipAngle.BoltOslOnSupportEdgeDistanceLong + shearClipAngle.BoltOslOnSupport.SpacingLongDir / 2;
					shearClipAngle.OSLBot = shearClipAngle.BoltOslOnSupportEdgeDistanceLong + shearClipAngle.BoltOslOnSupport.SpacingLongDir / 2;
					break;
				default:
					shearClipAngle.WTop = shearClipAngle.BoltWebOnBeam.EdgeDistLongDir;
					shearClipAngle.WBot = shearClipAngle.BoltWebOnBeam.EdgeDistLongDir;
					shearClipAngle.OSLTop = shearClipAngle.BoltOslOnSupportEdgeDistanceLong;
					shearClipAngle.OSLBot = shearClipAngle.BoltOslOnSupportEdgeDistanceLong;
					break;
			}

			string clipAngle = shearClipAngle.Number == 1 ? "Clip Angle: " : "Clip Angles: 2 ";
			Reporting.AddHeader(clipAngle + " " + shearClipAngle.Size.Name + " X " + shearClipAngle.Length + ConstUnit.Length);
			Reporting.AddLine("Angle Material: " + shearClipAngle.Material.Name);
			Reporting.AddLine("OSL: " + (shearClipAngle.OSL == EOSL.ShortLeg ? "Short Leg" : "Long Leg"));

			if (shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
			{
				Reporting.AddHeader("Support Side Connection: " + (shearClipAngle.Number * shearClipAngle.BoltOslOnSupport.NumberOfRows) + " Bolts " + shearClipAngle.BoltOslOnSupport.BoltName);
				if (!CommonDataStatic.IsFema && CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice)
				{
					Reporting.AddLine("Bolt Holes on Support: " + shearClipAngle.BoltOslOnSupport.HoleWidthSupport + ConstUnit.Length + " Vert. X " + shearClipAngle.BoltOslOnSupport.HoleLengthSupport + ConstUnit.Length + " Horiz.");
					Reporting.AddLine("Effective Thickness of Support Material: " + SupThickness + ConstUnit.Length);
					MiscCalculationsWithReporting.DetermineBoltAlignment(memberType, true);
				}
				Reporting.AddLine("Bolt Holes on Angles: " + shearClipAngle.BoltOslOnSupport.HoleWidth + ConstUnit.Length + " Vert. X " + shearClipAngle.BoltOslOnSupport.HoleLength + ConstUnit.Length + " Horiz.");
			}
			else
			{
				Reporting.AddHeader("Support Side Connection: " + CommonCalculations.WeldSize(shearClipAngle.WeldSizeSupport) + " " + shearClipAngle.WeldName + " Fillet Welds");
				Reporting.AddLine("Effective Thickness of Support Material: " + SupThickness + ConstUnit.Length);
				MiscCalculationsWithReporting.DetermineBoltAlignment(memberType, true);
			}
			if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted)
			{
				Reporting.AddHeader("Beam Side Connection: " + shearClipAngle.BoltWebOnBeam.NumberOfRows + " Bolts " + shearClipAngle.BoltWebOnBeam.BoltName);
				Reporting.AddLine("Bolt Holes on Beam Web: " + shearClipAngle.BoltWebOnBeam.HoleWidthSupport + ConstUnit.Length + " Vert. X " + shearClipAngle.BoltWebOnBeam.HoleLengthSupport + ConstUnit.Length + " Horiz.");
				Reporting.AddLine("Bolt Holes on Angles: " + shearClipAngle.BoltWebOnBeam.HoleWidth + ConstUnit.Length + " Vert. X " + shearClipAngle.BoltWebOnBeam.HoleLength + ConstUnit.Length + " Horiz.");
				Reporting.AddLine("Beam Web Thickness: " + beam.Shape.tw + ConstUnit.Length);
				Reporting.AddLine("Beam Web Height: " + beam.WinConnect.Beam.WebHeight + ConstUnit.Length);

			}
			else
			{
				Reporting.AddHeader("Beam Side Connection: " + CommonCalculations.WeldSize(shearClipAngle.WeldSizeBeam) + " " + shearClipAngle.WeldName + " Fillet Welds");
				Reporting.AddLine("Beam Web Thickness: " + beam.Shape.tw + ConstUnit.Length);
				Reporting.AddLine("Beam Web Height: " + beam.WinConnect.Beam.WebHeight + ConstUnit.Length);
			}
			Reporting.AddLine("Beam Setback: " + beam.EndSetback + ConstUnit.Length);

			V = shearClipAngle.Number * shearClipAngle.V;
			h = shearClipAngle.Number * shearClipAngle.h;
			R = Math.Sqrt(h * h + V * V);

			Reporting.AddHeader("Loading:");
			Reporting.AddLine("Vertical Shear (V) = " + V + ConstUnit.Force);
			Reporting.AddLine("Axial Load (H) = " + h + ConstUnit.Force);
			Reporting.AddLine("Resultant (R) = (V² + H²)^0.5 = ((" + V + " )² + (" + h + " )²)^0.5 = " + R + ConstUnit.Force);

			Reporting.AddHeader("Check Clearances:");
			if (beam.WinConnect.Beam.WebHeight >= shearClipAngle.Length)
				Reporting.AddLine("Beam Web Clear Height = " + beam.WinConnect.Beam.WebHeight + " >= " + shearClipAngle.Length + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Beam Web Clear Height = " + beam.WinConnect.Beam.WebHeight + " << " + shearClipAngle.Length + ConstUnit.Length + " (NG)");

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder &&
			    beam.WinConnect.Beam.DistanceToFirstBoltDisplay < ConstNum.QUARTER_INCH ||
			    (beam.WinConnect.Beam.CopeReinforcement.Type == EReinforcementType.None && beam.WinConnect.Beam.DistanceToFirstBoltDisplay < (shearClipAngle.WeldSizeBeam + ConstNum.SIXTEENTH_INCH)) ||
			    (beam.WinConnect.Beam.CopeReinforcement.Type != EReinforcementType.None && beam.WinConnect.Beam.DistanceToFirstBoltDisplay < (shearClipAngle.WeldSizeBeam + beam.WinConnect.Beam.CopeReinforcement.WeldSize + ConstNum.SIXTEENTH_INCH)))
				Reporting.AddLine("Clip Angle is too high (NG)");

			if (beam.P > 0)
			{
				double T = beam.Shape.d - 2 * beam.Shape.kdet;
				if (MinL <= shearClipAngle.Length)
					Reporting.AddLine("Minimum Length of Clip Angle = T / 2 = " + T + " / 2 = " + MinL + " <= " + shearClipAngle.Length + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Length of Clip Angle = T / 2 = " + T + " / 2 = " + MinL + " >> " + shearClipAngle.Length + ConstUnit.Length + " (NG)");
			}

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BeamToHSSColumn:
				case EJointConfiguration.BeamToColumnFlange:
					if (shearClipAngle.SupportSideConnection == EConnectionStyle.Welded)
					{
						switch (CommonDataStatic.BeamToColumnType)
						{
							case EJointConfiguration.BeamToColumnFlange:
								B = 2 * (shearClipAngle.LengthOfOSL + shearClipAngle.WeldSizeSupport) + column.Shape.tw;
								if (column.Shape.bf >= B)
									Reporting.AddLine("Column Flange Width = " + column.Shape.bf + " >= " + B + ConstUnit.Length + " (OK)");
								else
									Reporting.AddLine("Column Flange Width = " + column.Shape.bf + " << " + B + ConstUnit.Length + " (NG)");
								break;
							case EJointConfiguration.BeamToHSSColumn:
								BF = CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb ? column.Shape.d : column.Shape.bf;
								if (((shearClipAngle.LengthOfOSL + beam.Shape.tw / 2) < (BF / 2)) &&
								    ((shearClipAngle.LengthOfOSL + beam.Shape.tw / 2) > (BF / 2 - column.Shape.kdet - shearClipAngle.WeldSizeSupport)))
									Reporting.AddLine("The Angle OSL is too narrow to allow flare weld and too wide to allow fillet weld");
								break;
						}
					}
					break;
				case EJointConfiguration.BeamToColumnWeb:
					B = 2 * shearClipAngle.LengthOfOSL + column.Shape.tw;
					w = column.Shape.t; // d - 2 * beam.k
					if (w >= B)
						Reporting.AddLine("Column Web = " + w + " >= " + B + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Column Web = " + w + " << " + B + ConstUnit.Length + " (NG)");
					break;
			}

			if (shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
			{
				double spAngle;
				double spSupport;
				//double minSpacing;

				Reporting.AddGoToHeader("Support Side Bolts:");
				FuT = Math.Min(SupportFu * SupThickness, shearClipAngle.Material.Fu * shearClipAngle.Size.t);
				spSupport = MiscCalculationsWithReporting.MinimumSpacing(shearClipAngle.BoltOslOnSupport.BoltSize, shearClipAngle.V / shearClipAngle.BoltOslOnSupport.NumberOfRows, SupportFu * SupThickness, shearClipAngle.BoltWebOnBeam.HoleWidth, shearClipAngle.BoltOslOnSupport.HoleType);
				spAngle = MiscCalculationsWithReporting.MinimumSpacing(shearClipAngle.BoltOslOnSupport.BoltSize, shearClipAngle.V / shearClipAngle.BoltOslOnSupport.NumberOfRows, shearClipAngle.Material.Fu * shearClipAngle.Size.t, shearClipAngle.BoltOslOnSupport.HoleWidth, shearClipAngle.BoltOslOnSupport.HoleType);

				shearClipAngle.BoltOslOnSupport.MinSpacing = Math.Max(spSupport, spAngle);

				if (shearClipAngle.BoltOslOnSupport.SpacingLongDir >= shearClipAngle.BoltOslOnSupport.MinSpacing)
					Reporting.AddLine("Spacing (s) = " + shearClipAngle.BoltOslOnSupport.SpacingLongDir + " >= Minimum Spacing = " + shearClipAngle.BoltOslOnSupport.MinSpacing + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Spacing (s) = " + shearClipAngle.BoltOslOnSupport.SpacingLongDir + " << Minimum Spacing =" + shearClipAngle.BoltOslOnSupport.MinSpacing + ConstUnit.Length + " (NG)");

				evmin = MiscCalculationsWithReporting.MinimumEdgeDist(shearClipAngle.BoltOslOnSupport.BoltSize, shearClipAngle.V / shearClipAngle.BoltOslOnSupport.NumberOfRows, shearClipAngle.Material.Fu * shearClipAngle.Size.t, MiscCalculationsWithReporting.ShearedEdgeforAngle(shearClipAngle.BoltOslOnSupport.BoltSize), shearClipAngle.BoltOslOnSupport.HoleWidth, shearClipAngle.BoltOslOnSupport.HoleType);

				if (shearClipAngle.OSLTop != shearClipAngle.OSLBot)
				{
					Reporting.AddHeader("Distance to Horizontal Edge (ev) (Top):"); // Not sure how to replace OSLTop and OSLBot here...should reflect EdgeDistLongDir value
					if (shearClipAngle.OSLTop >= evmin)
						Reporting.AddLine("= " + shearClipAngle.OSLTop + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + shearClipAngle.OSLTop + " << " + evmin + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Distance to Horizontal Edge (ev) (Bottom):");
					if (shearClipAngle.OSLBot >= evmin)
						Reporting.AddLine("= " + shearClipAngle.OSLBot + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + shearClipAngle.OSLBot + " << " + evmin + ConstUnit.Length + " (NG)");
				}
				else
				{
					Reporting.AddLine("Distance to Horizontal Edge (ev):");
					if (shearClipAngle.OSLBot >= evmin)
						Reporting.AddLine("= " + shearClipAngle.OSLBot + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + shearClipAngle.OSLBot + " << " + evmin + ConstUnit.Length + " (NG)");
				}

				ehmin = MiscCalculationsWithReporting.MinimumEdgeDist(shearClipAngle.BoltOslOnSupport.BoltSize, 0, shearClipAngle.Material.Fu * shearClipAngle.Size.t, shearClipAngle.BoltOslOnSupport.MinEdgeRolled, shearClipAngle.BoltOslOnSupport.HoleWidth, shearClipAngle.BoltOslOnSupport.HoleType);
				Reporting.AddLine("Distance to Vertical Edge (eh):");
				if (shearClipAngle.BoltOslOnSupport.EdgeDistTransvDir >= ehmin)
					Reporting.AddLine("= " + shearClipAngle.BoltOslOnSupport.EdgeDistTransvDir + " >= " + ehmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + shearClipAngle.BoltOslOnSupport.EdgeDistTransvDir + " << " + ehmin + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Gage on OSL:");
				g = shearClipAngle.LengthOfOSL - shearClipAngle.BoltOslOnSupport.EdgeDistTransvDir;
				gmin = shearClipAngle.BoltOslOnSupport.BoltSize + shearClipAngle.Size.t + ConstNum.HALF_INCH;
				if (g >= gmin)
					Reporting.AddLine("Angle Gage = " + g + " >= " + gmin + ConstUnit.Length + " (OK)");
				else
				{
					Reporting.AddLine("Angle " + beam.GageOnColumn + "= " + g + " << " + gmin + ConstUnit.Length + " (NG)");
					Reporting.AddLine("Verify with the fabricator. This may be acceptable");
					Reporting.AddLine("or you may need to increase the Gage on column");
				}
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
				{
					double d1, d2, a1, a2;

					Reporting.AddLine("Column Gage = " + beam.GageOnColumn + ConstUnit.Length);
					d1 = (beam.GageOnColumn - shearClipAngle.BoltOslOnSupport.HoleLength) / 2;
					d2 = (beam.GageOnColumn + shearClipAngle.BoltOslOnSupport.HoleLength) / 2;
					a1 = g + beam.Shape.tw / 2 - shearClipAngle.BoltOslOnSupport.HoleLength / 2;
					a2 = g + beam.Shape.tw / 2 + shearClipAngle.BoltOslOnSupport.HoleLength / 2;
					if (Math.Min(d2, a2) - Math.Max(d1, a1) < shearClipAngle.BoltOslOnSupport.BoltSize)
						Reporting.AddLine("Check Bolt Hole Line-up (NG)");
				}

				capacityText = ConstString.DES_OR_ALLOWABLE + " Shear Strength of Bolts";
				Reporting.AddGoToHeader(capacityText);
				
				if (beam.P == 0)
				{
					if (shearClipAngle.Number == 2)
					{
						Cap = shearClipAngle.Number * shearClipAngle.BoltOslOnSupport.NumberOfRows * shearClipAngle.BoltOslOnSupport.BoltStrength;
						if (Cap >= shearClipAngle.Number * shearClipAngle.V)
							Reporting.AddCapacityLine("= 2 * n * (" + ConstString.PHI + " Rn) = 2 * " + shearClipAngle.BoltOslOnSupport.NumberOfRows + " * " + shearClipAngle.BoltOslOnSupport.BoltStrength + " = " + Cap + " >= " + (shearClipAngle.Number * shearClipAngle.V) + ConstUnit.Force + " (OK)", (shearClipAngle.Number * shearClipAngle.V) / Cap, capacityText, beam.MemberType);
						else
							Reporting.AddCapacityLine("= 2 * n * (" + ConstString.PHI + " Rn) = 2 * " + shearClipAngle.BoltOslOnSupport.NumberOfRows + " * " + shearClipAngle.BoltOslOnSupport.BoltStrength + " = " + Cap + " << " + (shearClipAngle.Number * shearClipAngle.V) + ConstUnit.Force + " (NG)", (shearClipAngle.Number * shearClipAngle.V) / Cap, capacityText, beam.MemberType);

						n = shearClipAngle.BoltOslOnSupport.NumberOfRows;
						c = n;
					}
					else
					{
						n = shearClipAngle.BoltOslOnSupport.NumberOfRows;
						agage = beam.GageOnColumn / 2;
						MiscCalculationsWithReporting.EccentricBolt(agage, shearClipAngle.BoltOslOnSupport.SpacingLongDir, 0, 1, n, ref c);
						Cap = c * shearClipAngle.BoltOslOnSupport.BoltStrength;
						if (Cap >= shearClipAngle.Number * shearClipAngle.V)
							Reporting.AddCapacityLine("= C * (" + ConstString.PHI + " Rn) = " + c + " * " + shearClipAngle.BoltOslOnSupport.BoltStrength + " = " + Cap + " >= " + (shearClipAngle.Number * shearClipAngle.V) + ConstUnit.Force + " (OK)", (shearClipAngle.Number * shearClipAngle.V) / Cap, capacityText, beam.MemberType);
						else
							Reporting.AddCapacityLine("= C * (" + ConstString.PHI + " Rn) = " + c + " * " + shearClipAngle.BoltOslOnSupport.BoltStrength + " = " + Cap + " << " + (shearClipAngle.Number * shearClipAngle.V) + ConstUnit.Force + " (NG)", (shearClipAngle.Number * shearClipAngle.V) / Cap, capacityText, beam.MemberType);
					}
				}
				else
				{
					// with axial load
					if (shearClipAngle.Number == 2)
					{
						if (shearClipAngle.BoltOslOnSupport.BoltType == EBoltType.SC)
						{
							switch (CommonDataStatic.Preferences.CalcMode)
							{
								case ECalcMode.ASD:
									Cap = shearClipAngle.Number * shearClipAngle.BoltOslOnSupport.NumberOfRows * shearClipAngle.BoltOslOnSupport.BoltStrength * (1 - 1.5 * shearClipAngle.h / (1.13 * shearClipAngle.BoltOslOnSupport.Pretension * shearClipAngle.BoltOslOnSupport.NumberOfRows));
									Reporting.AddLine("= 2 * n * (" + ConstString.PHI + " Rn) * (1 - 1.5 * Ta / (1.13 * Tm * n))");
									Reporting.AddLine("= 2 * " + shearClipAngle.BoltOslOnSupport.NumberOfRows + " * " + shearClipAngle.BoltOslOnSupport.BoltStrength + " *  (1 - 1.5 * " + shearClipAngle.h + " / (1.13 * " + shearClipAngle.BoltOslOnSupport.Pretension + " * " + shearClipAngle.BoltOslOnSupport.NumberOfRows + "))");
									break;
								case ECalcMode.LRFD:
									Cap = shearClipAngle.Number * shearClipAngle.BoltOslOnSupport.NumberOfRows * shearClipAngle.BoltOslOnSupport.BoltStrength * (1 - shearClipAngle.h / (1.13F * shearClipAngle.BoltOslOnSupport.Pretension * shearClipAngle.BoltOslOnSupport.NumberOfRows));
									Reporting.AddLine("= 2 * n * (" + ConstString.PHI + " Rn) * (1 - Tu / (1.13 * Tm * n))");
									Reporting.AddLine("= 2 * " + shearClipAngle.BoltOslOnSupport.NumberOfRows + " * " + shearClipAngle.BoltOslOnSupport.BoltStrength + " *  (1 - " + shearClipAngle.h + " / (1.13 * " + shearClipAngle.BoltOslOnSupport.Pretension + " * " + shearClipAngle.BoltOslOnSupport.NumberOfRows + "))");
									break;
							}
							if (Cap >= shearClipAngle.Number * shearClipAngle.V)
								Reporting.AddLine("= " + Cap + " >= " + (shearClipAngle.Number * shearClipAngle.V) + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= " + Cap + " << " + (shearClipAngle.Number * shearClipAngle.V) + ConstUnit.Force + " (NG)");
						}
						else
						{
							Cap = shearClipAngle.Number * shearClipAngle.BoltOslOnSupport.NumberOfRows * shearClipAngle.BoltOslOnSupport.BoltStrength;
							if (Cap >= shearClipAngle.Number * shearClipAngle.V)
								Reporting.AddLine("= 2 * n * (" + ConstString.PHI + " Rn) = 2 * " + shearClipAngle.BoltOslOnSupport.NumberOfRows + " * " + shearClipAngle.BoltOslOnSupport.BoltStrength + " = " + Cap + " >= " + (shearClipAngle.Number * shearClipAngle.V) + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= 2 * n * (" + ConstString.PHI + " Rn) = 2 * " + shearClipAngle.BoltOslOnSupport.NumberOfRows + " * " + shearClipAngle.BoltOslOnSupport.BoltStrength + " = " + Cap + " << " + (shearClipAngle.Number * shearClipAngle.V) + ConstUnit.Force + " (NG)");
						}
						n = shearClipAngle.BoltOslOnSupport.NumberOfRows;
						c = n;
					}
					else
					{
						n = shearClipAngle.BoltOslOnSupport.NumberOfRows;
						agage = beam.GageOnFlange / 2;
						MiscCalculationsWithReporting.EccentricBolt(agage, shearClipAngle.BoltOslOnSupport.SpacingLongDir, 0, 1, n, ref c);
						if (shearClipAngle.BoltOslOnSupport.BoltType == EBoltType.SC)
						{
							switch (CommonDataStatic.Preferences.CalcMode)
							{
								case ECalcMode.ASD:
									Cap = c * shearClipAngle.BoltOslOnSupport.BoltStrength * (1 - 1.5 * shearClipAngle.h / (1.13F * shearClipAngle.BoltOslOnSupport.Pretension * shearClipAngle.BoltOslOnSupport.NumberOfRows));
									if (Cap >= shearClipAngle.Number * shearClipAngle.V)
										Reporting.AddLine("= C * (" + ConstString.PHI + " Rn) * (1 - 1.5 * Ta / (1.13 * Tm * n)) = " + c + " * " + shearClipAngle.BoltOslOnSupport.BoltStrength + " *  (1 - " + shearClipAngle.h + " / (1.13 * " + shearClipAngle.BoltOslOnSupport.Pretension + " * " + shearClipAngle.BoltOslOnSupport.NumberOfRows + ")) = " + Cap + " >= " + (shearClipAngle.Number * shearClipAngle.V) + ConstUnit.Force + " (OK)");
									else
										Reporting.AddLine("= C * (" + ConstString.PHI + " Rn) * (1 - 1.5 * Tu / (1.13 * Tm * n)) = " + c + " * " + shearClipAngle.BoltOslOnSupport.BoltStrength + " *  (1 - " + shearClipAngle.h + " / (1.13 * " + shearClipAngle.BoltOslOnSupport.Pretension + " * " + shearClipAngle.BoltOslOnSupport.NumberOfRows + ")) = " + Cap + " << " + (shearClipAngle.Number * shearClipAngle.V) + ConstUnit.Force + " (NG)");
									break;
								case ECalcMode.LRFD:
									Cap = c * shearClipAngle.BoltOslOnSupport.BoltStrength * (1 - shearClipAngle.h / (1.13F * shearClipAngle.BoltOslOnSupport.Pretension * shearClipAngle.BoltOslOnSupport.NumberOfRows));
									if (Cap >= shearClipAngle.Number * shearClipAngle.V)
										Reporting.AddLine("= C * (" + ConstString.PHI + " Rn) * (1 - Tu / (1.13 * Tm * n)) = " + c + " * " + shearClipAngle.BoltOslOnSupport.BoltStrength + " *  (1 - " + shearClipAngle.h + " / (1.13 * " + shearClipAngle.BoltOslOnSupport.Pretension + " * " + shearClipAngle.BoltOslOnSupport.NumberOfRows + ")) = " + Cap + " >= " + (shearClipAngle.Number * shearClipAngle.V) + ConstUnit.Force + " (OK)");
									else
										Reporting.AddLine("= C * (" + ConstString.PHI + " Rn) * (1 - Tu / (1.13 * Tm * n)) = " + c + " * " + shearClipAngle.BoltOslOnSupport.BoltStrength + " *  (1 - " + shearClipAngle.h + " / (1.13 * " + shearClipAngle.BoltOslOnSupport.Pretension + " * " + shearClipAngle.BoltOslOnSupport.NumberOfRows + ")) = " + Cap + " << " + (shearClipAngle.Number * shearClipAngle.V) + ConstUnit.Force + " (NG)");
									break;
							}
						}
						else
						{
							Cap = c * shearClipAngle.BoltOslOnSupport.BoltStrength;
							if (Cap >= shearClipAngle.Number * shearClipAngle.V)
								Reporting.AddLine("= C * (" + ConstString.PHI + " Rn) = " + c + " * " + shearClipAngle.BoltOslOnSupport.BoltStrength + " = " + Cap + " >= " + (shearClipAngle.Number * shearClipAngle.V) + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= C * (" + ConstString.PHI + " Rn) = " + c + " * " + shearClipAngle.BoltOslOnSupport.BoltStrength + " = " + Cap + " << " + (shearClipAngle.Number * shearClipAngle.V) + ConstUnit.Force + " (NG)");
						}
					}

					Reporting.AddHeader("Tension Strength of Clip Angle(s)");
					FiRn_perBolt = BoltsForTension.CalcBoltsForTension(beam.MemberType, shearClipAngle.BoltOslOnSupport, shearClipAngle.h, n / c * shearClipAngle.V, shearClipAngle.BoltOslOnSupport.NumberOfRows, true);
					edg = shearClipAngle.BoltOslOnSupport.EdgeDistTransvDir;
					B = (shearClipAngle.LengthOfOSL - edg - shearClipAngle.Size.t / 2);
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange)
						edg = Math.Min(edg, (column.Shape.bf - beam.GageOnColumn) / 2);
					FiRn = MiscCalculationsWithReporting.HangerStrength(beam.MemberType, shearClipAngle.LengthOfOSL, shearClipAngle.Material.Fu, shearClipAngle.Size.t, edg, B, FiRn_perBolt, 0, true);
					TensionCap = shearClipAngle.Number * shearClipAngle.BoltOslOnSupport.NumberOfRows * FiRn;

					Reporting.AddHeader("Design Tension Strength:");
					if (shearClipAngle.Number == 2)
					{
						if (TensionCap >= h)
							Reporting.AddLine("= 2 * n * (" + ConstString.PHI + " Rn) = 2 * " + shearClipAngle.BoltOslOnSupport.NumberOfRows + " * " + FiRn + " = " + TensionCap + " >= " + h + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= 2 * n * (" + ConstString.PHI + " Rn) = 2 * " + shearClipAngle.BoltOslOnSupport.NumberOfRows + " * " + FiRn + " = " + TensionCap + " << " + h + ConstUnit.Force + " (NG)");
					}
					else
					{
						if (TensionCap >= h)
							Reporting.AddLine("= n * (" + ConstString.PHI + " Rn) = " + shearClipAngle.BoltOslOnSupport.NumberOfRows + " * " + FiRn + " = " + TensionCap + " >= " + h + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= n * (" + ConstString.PHI + " Rn) = " + shearClipAngle.BoltOslOnSupport.NumberOfRows + " * " + FiRn + " = " + TensionCap + " << " + h + ConstUnit.Force + " (NG)");
					}
				}

				Reporting.AddGoToHeader("Bolt Bearing Check");
				Reporting.AddHeader(beam.ComponentName + " Bolt Bearing on Angle(s):");
				Fbe = CommonCalculations.EdgeBearing(Math.Min(shearClipAngle.OSLTop, shearClipAngle.OSLBot), (shearClipAngle.BoltOslOnSupport.HoleLength), shearClipAngle.BoltOslOnSupport.BoltSize, shearClipAngle.Material.Fu, shearClipAngle.BoltOslOnSupport.HoleType, true);
				Fbs = CommonCalculations.SpacingBearing(shearClipAngle.BoltOslOnSupport.SpacingLongDir, shearClipAngle.BoltOslOnSupport.HoleLength, shearClipAngle.BoltOslOnSupport.BoltSize, shearClipAngle.BoltOslOnSupport.HoleType, shearClipAngle.Material.Fu, true);
				
				if (shearClipAngle.Number == 2)
				{
                    BearingCap = shearClipAngle.Number * (Fbe + Fbs * (shearClipAngle.BoltOslOnSupport.NumberOfRows - 1)) * shearClipAngle.Size.t;
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = 2 * (Fbe + Fbs * (n - 1)) * t");
					Reporting.AddLine("= 2 * (" + Fbe + " + " + Fbs + " * (" + shearClipAngle.BoltOslOnSupport.NumberOfRows + " - 1)) * " + shearClipAngle.Size.t);
				}
				else
				{
                    // Bug 68: new calc for BearingCap when there's a single angle.     -RM
                    BearingCap = (c / shearClipAngle.BoltOslOnSupport.NumberOfRows) * (Fbe + Fbs * (shearClipAngle.BoltOslOnSupport.NumberOfRows - 1)) * shearClipAngle.Size.t;
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = C/n * (Fbe + Fbs * (n - 1)) * t");
					Reporting.AddLine("= (" + c + "/" + shearClipAngle.BoltOslOnSupport.NumberOfRows + ") * (" + Fbe + " + " + Fbs + " * (" + shearClipAngle.BoltOslOnSupport.NumberOfRows + " - 1)) * " + shearClipAngle.Size.t);
				}

				capacityText = "Bolt Bearing on Angle(s)";
				if (BearingCap >= V)
					Reporting.AddCapacityLine("= " + BearingCap + " >= " + V + ConstUnit.Force + " (OK)", V / BearingCap, capacityText, beam.MemberType);
				else
					Reporting.AddCapacityLine("= " + BearingCap + " << " + V + ConstUnit.Force + " (NG)", V / BearingCap, capacityText, beam.MemberType);

				if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice && !CommonDataStatic.IsFema)
				{
					Reporting.AddHeader("Bolt Bearing on Support:");
					Fbs = CommonCalculations.SpacingBearing(shearClipAngle.BoltOslOnSupport.SpacingLongDir, shearClipAngle.BoltOslOnSupport.HoleWidth, shearClipAngle.BoltOslOnSupport.BoltSize, shearClipAngle.BoltOslOnSupport.HoleType, SupportFu, true);
					

					if (shearClipAngle.Number == 2)
					{
                        BearingCap = shearClipAngle.Number * Fbs * shearClipAngle.BoltOslOnSupport.NumberOfRows * SupThickness;
						Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = 2 * Fbs * n * t");
						Reporting.AddLine("= 2 * " + Fbs + " * " + shearClipAngle.BoltOslOnSupport.NumberOfRows + " * " + SupThickness);
					}
					else
					{
                        // Bug 68: new calc for BearingCap when there's a single angle.     -RM
                        BearingCap = shearClipAngle.Number * Fbs * c * SupThickness;
                        Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = Fbs * C * t");
						Reporting.AddLine("= " + Fbs + " * " + c + " * " + SupThickness);
					}

					capacityText = "Bolt Bearing on Support";
					if (BearingCap >= V)
						Reporting.AddCapacityLine("= " + BearingCap + " >= " + V + ConstUnit.Force + " (OK)", V / BearingCap, capacityText, beam.MemberType);
					else
						Reporting.AddCapacityLine("= " + BearingCap + " << " + V + ConstUnit.Force + " (NG)", V / BearingCap, capacityText, beam.MemberType);
				}
			}
			else
			{
				Reporting.AddGoToHeader("Support Side Weld: " + CommonCalculations.WeldSize(shearClipAngle.WeldSizeSupport) + " " + shearClipAngle.WeldName);
				Reporting.AddLine("Angle Thickness = " + shearClipAngle.Size.t + ConstUnit.Length);
				Reporting.AddLine("Support Thickness = " + SupThickness + ConstUnit.Length);
				mnweld = CommonCalculations.MinimumWeld(shearClipAngle.Size.t, SupThickness1);
				if (shearClipAngle.Size.t >= ConstNum.QUARTER_INCH)
					MxWeld = shearClipAngle.Size.t - ConstNum.SIXTEENTH_INCH;
				else
					MxWeld = shearClipAngle.Size.t;

				Reporting.AddLine(string.Empty);
				Reporting.AddLine("Minimum Weld = " + CommonCalculations.WeldSize(mnweld) + ConstUnit.Length);
				if (shearClipAngle.WeldSizeSupport >= mnweld)
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(shearClipAngle.WeldSizeSupport) + " >= " + CommonCalculations.WeldSize(mnweld) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(shearClipAngle.WeldSizeSupport) + " << " + CommonCalculations.WeldSize(mnweld) + ConstUnit.Length + " (NG)");
				Reporting.AddLine("Maximum Weld = " + CommonCalculations.WeldSize(MxWeld) + ConstUnit.Length);
				if (shearClipAngle.WeldSizeSupport <= MxWeld)
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(shearClipAngle.WeldSizeSupport) + " <= " + CommonCalculations.WeldSize(MxWeld) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(shearClipAngle.WeldSizeSupport) + " >> " + CommonCalculations.WeldSize(MxWeld) + ConstUnit.Length + " (NG)");

				if (shearClipAngle.Number == 1)
				{
					C1 = CommonCalculations.WeldTypeRatio();
					k = shearClipAngle.LengthOfOSL / shearClipAngle.Length;
					a = column.Shape.tw / (2 * shearClipAngle.Length) + k - Math.Pow(k, 2) / (2 * (1 + k));
					theta = shearClipAngle.h / shearClipAngle.V;
					Cnst = EccentricWeld.GetEccentricWeld(k, a, theta, true);
					//EccentricWeld.GetEccentricWeld(k, a, ref Cnst, theta, true);
					OslWeldCap = 16 * shearClipAngle.WeldSizeSupport * Cnst * C1 * shearClipAngle.Length;

					Reporting.AddHeader("Weld Strength = D * C * C1 * L");
					Reporting.AddLine("= " + (16 * shearClipAngle.WeldSizeSupport) + " * " + Cnst + " * " + C1 + " * " + shearClipAngle.Length);
					Reporting.AddLine("= " + OslWeldCap + ConstUnit.Force + " (Before thickness check)");
				}
				else
					OslWeldCap = MiscCalculationsWithReporting.OslWeld(beam.MemberType, 0, true);

				Reporting.AddHeader("Reduction Factor for Support Thickness (Rt):");
				// Rt = 1.8859 * SupFy * SupThickness / (ClipAngle.OslWeld * Fexx)
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
				{
					if (beam.IsActive && (shearClipAngle.WeldSizeSupport == 0 || !shearClipAngle.WeldSizeSupport_User))
						shearClipAngle.WeldSizeSupport = ConstNum.EIGHTH_INCH;
					else if (beam.IsActive && !shearClipAngle.WeldSizeSupport_User)
						shearClipAngle.WeldSizeSupport = 0;
					RT = 1.414 * SupportFu * SupThickness1 / ((shearClipAngle.WeldSizeSupport + shearClipAngle.WeldSizeSupport) * shearClipAngle.Weld.Fexx);
					if (!oppositeBeam.WinConnect.ShearClipAngle.WeldSizeSupport_User)
						oppositeBeam.WinConnect.ShearClipAngle.WeldSizeSupport = shearClipAngle.WeldSizeSupport;
					Reporting.AddLine("= 1.414 * Fu * ts / ((wL + wR) * Fexx)");
					Reporting.AddLine("= 1.414 * " + SupportFu + " * " + SupThickness1 + " / ((" + oppositeBeam.WinConnect.ShearClipAngle.WeldSizeSupport + " + " + shearClipAngle.WeldSizeSupport + ") * " + shearClipAngle.Weld.Fexx + ")");
				}
				else
				{
					RT = 1.414 * SupportFu * SupThickness1 / (shearClipAngle.WeldSizeSupport * shearClipAngle.Weld.Fexx);
					Reporting.AddLine("= 1.414 * Fu * ts / (w * Fexx)");
					Reporting.AddLine("= 1.414 * " + SupportFu + " * " + SupThickness1 + " / (" + shearClipAngle.WeldSizeSupport + " * " + shearClipAngle.Weld.Fexx + ")");
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
				WeldCap = shearClipAngle.Number * WeldCap;

				if (WeldCap >= R)
					Reporting.AddLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + WeldCap + " >= " + R + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + WeldCap + " << " + R + ConstUnit.Force + " (NG)");

				if (shearClipAngle.h != 0)
				{
					double OslMoment = shearClipAngle.h * (shearClipAngle.LengthOfOSL - shearClipAngle.Size.kdet);
					BendingCap = ConstNum.FIOMEGA0_9N * shearClipAngle.Material.Fy * shearClipAngle.Length * Math.Pow(shearClipAngle.Size.t, 2) / 4;
					Reporting.AddHeader("OSL Bending:");
					Reporting.AddLine("Moment = T * (OSL - f) = " + shearClipAngle.h + " * (" + shearClipAngle.LengthOfOSL + " - " + shearClipAngle.Size.kdet + ") = " + OslMoment + ConstUnit.MomentUnitInch);
					if (BendingCap >= OslMoment)
						Reporting.AddLine("Bending Strength = " + ConstString.FIOMEGA0_9 + " * Fy * L * t² / 4 = " + ConstString.FIOMEGA0_9 + " * " + shearClipAngle.Material.Fy + " * " + shearClipAngle.Length + " * " + shearClipAngle.Size.t + "² / 4 = " + BendingCap + " >= " + OslMoment + ConstUnit.MomentUnitInch + " (OK)");
					else
						Reporting.AddLine("Bending Strength = " + ConstString.FIOMEGA0_9 + " * Fy * L * t² / 4 = " + ConstString.FIOMEGA0_9 + " * " + shearClipAngle.Material.Fy + " * " + shearClipAngle.Length + " * " + shearClipAngle.Size.t + "² / 4 = " + BendingCap + " << " + OslMoment + ConstUnit.MomentUnitInch + " (NG)");
				}
			}

			if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted)
			{
				double rolledEdge;
				double spBeam;
				double spAngle;
				//double minSpacing;

				Reporting.AddGoToHeader("Beam Side Bolts: " + shearClipAngle.BoltWebOnBeam.NumberOfRows + " Bolts - " + shearClipAngle.BoltWebOnBeam.BoltName);
				FuT = Math.Min(beam.Material.Fu * column.Shape.tw / shearClipAngle.Number, shearClipAngle.Material.Fu * shearClipAngle.Size.t);
				spBeam = MiscCalculationsWithReporting.MinimumSpacing(shearClipAngle.BoltWebOnBeam.BoltSize, (shearClipAngle.V / shearClipAngle.BoltWebOnBeam.NumberOfRows), beam.Material.Fu * beam.Shape.tw / shearClipAngle.Number, shearClipAngle.BoltWebOnBeam.HoleWidth, shearClipAngle.BoltWebOnBeam.HoleType);
				spAngle = MiscCalculationsWithReporting.MinimumSpacing(shearClipAngle.BoltWebOnBeam.BoltSize, (shearClipAngle.V / shearClipAngle.BoltWebOnBeam.NumberOfRows), shearClipAngle.Material.Fu * shearClipAngle.Size.t, shearClipAngle.BoltWebOnBeam.HoleWidth, shearClipAngle.BoltWebOnBeam.HoleType);

				shearClipAngle.BoltWebOnBeam.MinSpacing = Math.Max(spBeam, spAngle);

				if (shearClipAngle.BoltWebOnBeam.SpacingLongDir >= shearClipAngle.BoltWebOnBeam.MinSpacing)
					Reporting.AddLine("Spacing (s) = " + shearClipAngle.BoltWebOnBeam.SpacingLongDir + " >= Minimum Spacing = " + shearClipAngle.BoltWebOnBeam.MinSpacing + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Spacing (s) = " + shearClipAngle.BoltWebOnBeam.SpacingLongDir + " << Minimum Spacing = " + shearClipAngle.BoltWebOnBeam.MinSpacing + ConstUnit.Length + " (NG)");
				// check longitudinal edge distance
				evmin = MiscCalculationsWithReporting.MinimumEdgeDist(shearClipAngle.BoltWebOnBeam.BoltSize, shearClipAngle.V / shearClipAngle.BoltWebOnBeam.NumberOfRows, shearClipAngle.Material.Fu * shearClipAngle.Size.t, MiscCalculationsWithReporting.ShearedEdgeforAngle(shearClipAngle.BoltWebOnBeam.BoltSize), (int)shearClipAngle.BoltWebOnBeam.HoleWidth, shearClipAngle.BoltWebOnBeam.HoleType);

				if (shearClipAngle.WTop != shearClipAngle.WBot)
				{
					Reporting.AddHeader("Distance to Horizontal Edge (ev) (Top):");
					if (shearClipAngle.WTop >= evmin)
						Reporting.AddLine("= " + shearClipAngle.WTop + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + shearClipAngle.WTop + " << " + evmin + ConstUnit.Length + " (NG)");
					Reporting.AddLine("Distance to Horizontal Edge (ev) (Bottom):");
					if (shearClipAngle.WBot >= evmin)
						Reporting.AddLine("= " + shearClipAngle.WBot + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + shearClipAngle.WBot + " << " + evmin + ConstUnit.Length + " (NG)");
				}
				else
				{
					Reporting.AddLine("Distance to Horizontal Edge (ev):");
					if (shearClipAngle.BoltWebOnBeam.EdgeDistLongDir >= evmin)
						Reporting.AddLine("= " + shearClipAngle.WBot + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + shearClipAngle.WBot + " << " + evmin + ConstUnit.Length + " (NG)");
				}

				rolledEdge = shearClipAngle.BoltWebOnBeam.MinEdgeRolled;
				ehmin = MiscCalculationsWithReporting.MinimumEdgeDist(shearClipAngle.BoltWebOnBeam.BoltSize, shearClipAngle.h / shearClipAngle.BoltWebOnBeam.NumberOfRows, shearClipAngle.Material.Fu * shearClipAngle.Size.t, rolledEdge, shearClipAngle.BoltWebOnBeam.HoleLength, shearClipAngle.BoltWebOnBeam.HoleType);
				Reporting.AddLine("Distance to Vertical Edge (eh):");
				if (shearClipAngle.BoltWebOnBeam.EdgeDistTransvDir >= ehmin)
					Reporting.AddLine("= " + shearClipAngle.BoltWebOnBeam.EdgeDistTransvDir + " >= " + ehmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + shearClipAngle.BoltWebOnBeam.EdgeDistTransvDir + " << " + ehmin + ConstUnit.Length + " (NG)");

				Reporting.AddLine("Gage on Angle Leg in Beam Web:");
				g = shearClipAngle.OppositeOfOSL - shearClipAngle.BoltWebOnBeam.EdgeDistTransvDir;

				if (shearClipAngle.BoltStagger == EBoltStagger.None)
					gmin = shearClipAngle.Size.kdet + shearClipAngle.BoltWebOnBeam.BoltSize;
				else
					gmin = MiscCalculationDataMethods.StaggeredBoltGage(Math.Max(shearClipAngle.BoltWebOnBeam.BoltSize, shearClipAngle.BoltOslOnSupport.BoltSize), (Math.Max(shearClipAngle.BoltWebOnBeam.SpacingLongDir, shearClipAngle.BoltOslOnSupport.SpacingLongDir) / 2), shearClipAngle.Size.t);

				if (g >= gmin)
					Reporting.AddLine("= " + g + " >= " + gmin + ConstUnit.Length + " (OK)");
				else
				{
					Reporting.AddLine("= " + g + " << " + gmin + ConstUnit.Length + " (NG)");
					Reporting.AddLine("Verify with the fabricator. This may be acceptable.");
					Reporting.AddLine("The Gage is the sum of Lh of the beam and the beam setback.");
					Reporting.AddLine("You can change these values in the 'Beam data window'.");
				}

				Reporting.AddGoToHeader(ConstString.DES_OR_ALLOWABLE + " - Shear Strength of Bolts:");
				capacityText = ConstString.DES_OR_ALLOWABLE + " Load Eccentricity";
				ecc = beam.EndSetback + beam.WinConnect.Beam.Lh;
				if (ecc >= ConstNum.THREE_INCHES)
				{
					MiscCalculationsWithReporting.EccentricBolt(ecc, shearClipAngle.BoltWebOnBeam.SpacingLongDir, 0, 1, shearClipAngle.BoltWebOnBeam.NumberOfRows, ref c);
					Reporting.AddHeader("Load Eccentricity = " + (beam.EndSetback + beam.WinConnect.Beam.Lh) + ConstUnit.Length);
					Reporting.AddLine("Number of Bolts = " + shearClipAngle.BoltWebOnBeam.NumberOfRows);
					Reporting.AddLine("Bolt Spacing = " + shearClipAngle.BoltWebOnBeam.SpacingLongDir + ConstUnit.Length);
					Reporting.AddLine("C = " + c);
					Cap = shearClipAngle.Number * c * shearClipAngle.BoltWebOnBeam.BoltStrength;
					if (shearClipAngle.Number == 2)
					{
						if (Cap >= R)
							Reporting.AddCapacityLine("= 2 * c * (" + ConstString.PHI + " Rn) = 2 * " + c + " * " + shearClipAngle.BoltWebOnBeam.BoltStrength + " = " + Cap + " >= " + R + ConstUnit.Force + " (OK)", R / Cap, capacityText, beam.MemberType);
						else
							Reporting.AddCapacityLine("= 2 * c * (" + ConstString.PHI + " Rn) = 2 * " + c + " * " + shearClipAngle.BoltWebOnBeam.BoltStrength + " = " + Cap + " << " + R + ConstUnit.Force + " (NG)", R / Cap, capacityText, beam.MemberType);
					}
					else
					{
						if (Cap >= R)
							Reporting.AddCapacityLine("= c * (" + ConstString.PHI + " Rn) = " + c + " * " + shearClipAngle.BoltWebOnBeam.BoltStrength + " = " + Cap + " >= " + R + ConstUnit.Force + " (OK)", R / Cap, capacityText, beam.MemberType);
						else
							Reporting.AddCapacityLine("= c * (" + ConstString.PHI + " Rn) = " + c + " * " + shearClipAngle.BoltWebOnBeam.BoltStrength + " = " + Cap + " << " + R + ConstUnit.Force + " (NG)", R / Cap, capacityText, beam.MemberType);
					}
				}
				else
				{
					Cap = shearClipAngle.Number * shearClipAngle.BoltWebOnBeam.NumberOfRows * shearClipAngle.BoltWebOnBeam.BoltStrength;
					if (shearClipAngle.Number == 2)
					{
						if (Cap >= R)
							Reporting.AddCapacityLine("= 2 * N * (" + ConstString.PHI + " Rn) = 2 * " + shearClipAngle.BoltWebOnBeam.NumberOfRows + " * " + shearClipAngle.BoltWebOnBeam.BoltStrength + " = " + Cap + " >= " + R + ConstUnit.Force + " (OK)", R / Cap, capacityText, beam.MemberType);
						else
							Reporting.AddCapacityLine("= 2 * N * (" + ConstString.PHI + " Rn) = 2 * " + shearClipAngle.BoltWebOnBeam.NumberOfRows + " * " + shearClipAngle.BoltWebOnBeam.BoltStrength + " = " + Cap + " << " + R + ConstUnit.Force + " (NG)", R / Cap, capacityText, beam.MemberType);
					}
					else
					{
						if (Cap >= R)
							Reporting.AddCapacityLine("= N * (" + ConstString.PHI + " Rn) = " + shearClipAngle.BoltWebOnBeam.NumberOfRows + " * " + shearClipAngle.BoltWebOnBeam.BoltStrength + " = " + Cap + " >= " + R + ConstUnit.Force + " (OK)", R / Cap, capacityText, beam.MemberType);
						else
							Reporting.AddCapacityLine("= N * (" + ConstString.PHI + " Rn) = " + shearClipAngle.BoltWebOnBeam.NumberOfRows + " * " + shearClipAngle.BoltWebOnBeam.BoltStrength + " = " + Cap + " << " + R + ConstUnit.Force + " (NG)", R / Cap, capacityText, beam.MemberType);
					}
				}

				Reporting.AddHeader("Bolt Bearing on Angles:");
				Fbe = CommonCalculations.EdgeBearing(Math.Min(shearClipAngle.WTop, shearClipAngle.WBot), (shearClipAngle.BoltWebOnBeam.HoleWidth), shearClipAngle.BoltWebOnBeam.BoltSize, shearClipAngle.Material.Fu, shearClipAngle.BoltWebOnBeam.HoleType, true);
				Fbs = CommonCalculations.SpacingBearing(shearClipAngle.BoltWebOnBeam.SpacingLongDir, shearClipAngle.BoltWebOnBeam.HoleWidth, shearClipAngle.BoltWebOnBeam.BoltSize, shearClipAngle.BoltWebOnBeam.HoleType, shearClipAngle.Material.Fu, true);

				BearingCap = shearClipAngle.Number * (Fbe + Fbs * (shearClipAngle.BoltWebOnBeam.NumberOfRows - 1)) * shearClipAngle.Size.t;

				if (shearClipAngle.Number == 2)
				{
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = 2 * (Fbe + Fbs * (n - 1)) * t");
					Reporting.AddLine("= 2 * (" + Fbe + " + " + Fbs + " * (" + shearClipAngle.BoltWebOnBeam.NumberOfRows + " - 1)) * " + shearClipAngle.Size.t);
				}
				else
				{
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = (Fbe + Fbs * (n - 1)) * t");
					Reporting.AddLine("= (" + Fbe + " + " + Fbs + " * (" + shearClipAngle.BoltWebOnBeam.NumberOfRows + " - 1)) * " + shearClipAngle.Size.t);
				}
				if (BearingCap >= V)
					Reporting.AddLine("= " + BearingCap + " >= " + V + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + BearingCap + " << " + V + ConstUnit.Force + " (NG)");

				Reporting.AddHeader("Bolt Bearing on Beam Web:");
				Fbs = CommonCalculations.SpacingBearing(shearClipAngle.BoltWebOnBeam.SpacingLongDir, shearClipAngle.BoltWebOnBeam.HoleWidth, shearClipAngle.BoltWebOnBeam.BoltSize, shearClipAngle.BoltWebOnBeam.HoleType, beam.Material.Fu, true);
				if (beam.WinConnect.Beam.TopCope)
				{
					Fbe = CommonCalculations.EdgeBearing(beam.WinConnect.Beam.DistanceToFirstBolt, shearClipAngle.BoltWebOnBeam.HoleWidth, shearClipAngle.BoltWebOnBeam.BoltSize, beam.Material.Fu, shearClipAngle.BoltWebOnBeam.HoleType, true);
					BearingCap = (Fbe + Fbs * (shearClipAngle.BoltWebOnBeam.NumberOfRows - 1)) * beam.Shape.tw;
					Reporting.AddLine("Bearing Capacity = (Fbe + Fbs * (n - 1)) * t");
					Reporting.AddLine("= (" + Fbe + " + " + Fbs + " * (" + shearClipAngle.BoltWebOnBeam.NumberOfRows + " - 1)) * " + beam.Shape.tw);
				}
				else
				{
					BearingCap = Fbs * shearClipAngle.BoltWebOnBeam.NumberOfRows * beam.Shape.tw;
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = Fbs * n * t");
					Reporting.AddLine("= " + Fbs + " * " + shearClipAngle.BoltWebOnBeam.NumberOfRows + " * " + beam.Shape.tw);
				}
				if (BearingCap >= V)
					Reporting.AddLine("= " + BearingCap + " >= " + V + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + BearingCap + " << " + V + ConstUnit.Force + " (NG)");
				if (h != 0)
				{
					Reporting.AddHeader("Bolt Bearing Under Beam Axial Load:");
					Reporting.AddHeader("On Angles:");
					Fbe = CommonCalculations.EdgeBearing(shearClipAngle.BoltWebOnBeam.EdgeDistTransvDir, shearClipAngle.BoltWebOnBeam.HoleLength, shearClipAngle.BoltWebOnBeam.BoltSize, shearClipAngle.Material.Fu, shearClipAngle.BoltWebOnBeam.HoleType, true);
					BearingCap = shearClipAngle.Number * Fbe * shearClipAngle.BoltWebOnBeam.NumberOfRows * shearClipAngle.Size.t;
					if (shearClipAngle.Number == 2)
					{
						Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = 2 * Fbe * n * t");
						Reporting.AddLine("= 2*" + Fbe + " * " + shearClipAngle.BoltWebOnBeam.NumberOfRows + " * " + shearClipAngle.Size.t);
					}
					else
					{
						Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = Fbe * n * t");
						Reporting.AddLine("= " + Fbe + " * " + shearClipAngle.BoltWebOnBeam.NumberOfRows + " * " + shearClipAngle.Size.t);
					}
					if (BearingCap >= h)
						Reporting.AddLine("= " + BearingCap + " >= " + h + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + BearingCap + " << " + h + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("On Beam Web:");
					Fbe = CommonCalculations.EdgeBearing(beam.WinConnect.Beam.Lh, shearClipAngle.BoltWebOnBeam.HoleLength, shearClipAngle.BoltWebOnBeam.BoltSize, beam.Material.Fu, shearClipAngle.BoltWebOnBeam.HoleType, true);
					BearingCap = Fbe * shearClipAngle.BoltWebOnBeam.NumberOfRows * column.Shape.tw;
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = Fbe * n * t");
					Reporting.AddLine("= " + Fbe + " * " + shearClipAngle.BoltWebOnBeam.NumberOfRows + " * " + column.Shape.tw);
					if (BearingCap >= h)
						Reporting.AddLine("= " + BearingCap + " >= " + h + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + BearingCap + " << " + h + ConstUnit.Force + " (NG)");
				}

				MiscCalculationsWithReporting.BlockShear(beam.MemberType, EShearOpt.ClipAngle, ref BsCap, true);
				if (shearClipAngle.h * shearClipAngle.Number != 0)
				{
					Reporting.AddHeader("Beam Web Tear out Under Axial Load:");
					BeamTearOutCap = MiscCalculationsWithReporting.BeamWebTearout(beam.MemberType, shearClipAngle.BoltWebOnBeam.NumberOfRows, 1, shearClipAngle.BoltWebOnBeam.SpacingLongDir, 0, EBeamOrAttachment.Beam, ETearOutBlockShear.TearOut, true);
					BeamBlockShearCap = MiscCalculationsWithReporting.BeamWebTearout(beam.MemberType, shearClipAngle.BoltWebOnBeam.NumberOfRows, 1, shearClipAngle.BoltWebOnBeam.SpacingLongDir, 0, EBeamOrAttachment.Beam, ETearOutBlockShear.BlockShear, true);
				}

				if (beam.WinConnect.Beam.TopCope || beam.WinConnect.Beam.BottomCope)
					Cope.CopedBeam(beam.MemberType, true);

				if (h != 0)
				{
					double blockShearStrength;
					double tearoutStrength;

					Reporting.AddHeader("Angle Tear out Under Beam Axial Load:");

					tearoutStrength = MiscCalculationsWithReporting.BeamWebTearout(beam.MemberType, shearClipAngle.BoltWebOnBeam.NumberOfRows, 1, shearClipAngle.BoltWebOnBeam.SpacingLongDir, 0, EBeamOrAttachment.Angle, ETearOutBlockShear.TearOut, true);
					if (shearClipAngle.Number == 2)
					{
						Reporting.AddLine("For two angles, " + ConstString.PHI + " Rn = 2 * " + tearoutStrength);
						tearoutStrength = 2 * tearoutStrength;
					}
					if (tearoutStrength >= h)
						Reporting.AddLine("= " + tearoutStrength + " >= " + h + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + tearoutStrength + " << " + h + ConstUnit.Force + " (NG)");

					blockShearStrength = MiscCalculationsWithReporting.BeamWebTearout(beam.MemberType, shearClipAngle.BoltWebOnBeam.NumberOfRows, 1, shearClipAngle.BoltWebOnBeam.SpacingLongDir, 0, EBeamOrAttachment.Angle, ETearOutBlockShear.BlockShear, true);
					if (shearClipAngle.Number == 2)
					{
						Reporting.AddLine("For two angles, " + ConstString.PHI + " Rn = 2 * " + blockShearStrength);
						blockShearStrength = 2 * blockShearStrength;
					}
					if (blockShearStrength >= h)
						Reporting.AddLine("= " + blockShearStrength + " >= " + h + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + blockShearStrength + " << " + h + ConstUnit.Force + " (NG)");
				}
			}
			else
			{
				Reporting.AddGoToHeader("Beam Side Weld: " + CommonCalculations.WeldSize(shearClipAngle.WeldSizeBeam) + " " + shearClipAngle.WeldName);
				Reporting.AddLine("Angle Thickness = " + shearClipAngle.Size.t + ConstUnit.Length);
				Reporting.AddLine("Beam Web Thickness = " + beam.Shape.tw + ConstUnit.Length);

				if (shearClipAngle.Size.t >= ConstNum.QUARTER_INCH)
					MxWeld = shearClipAngle.Size.t - ConstNum.SIXTEENTH_INCH;
				else
					MxWeld = shearClipAngle.Size.t;
				Reporting.AddLine("Minimum Weld = " + CommonCalculations.WeldSize(mnweld) + ConstUnit.Length);
				if (shearClipAngle.WeldSizeBeam >= mnweld)
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(shearClipAngle.WeldSizeBeam) + " >= " + CommonCalculations.WeldSize(mnweld) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(shearClipAngle.WeldSizeBeam) + " << " + CommonCalculations.WeldSize(mnweld) + ConstUnit.Length + " (NG)");

				Reporting.AddLine("Maximum Weld = " + CommonCalculations.WeldSize(MxWeld) + ConstUnit.Length);
				if (shearClipAngle.WeldSizeBeam <= MxWeld)
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(shearClipAngle.WeldSizeBeam) + " <= " + CommonCalculations.WeldSize(MxWeld) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(shearClipAngle.WeldSizeBeam) + " >> " + CommonCalculations.WeldSize(MxWeld) + ConstUnit.Length + " (NG)");

				MiscCalculationsWithReporting.ClipWelds(beam.MemberType, ref WWeldCap, true);

				capacityText = "Weld Capacity";
				if (WWeldCap >= R)
					Reporting.AddCapacityLine("Weld Capacity = " + WWeldCap + " >= " + R + ConstUnit.Force + " (OK)", R / WWeldCap, capacityText, beam.MemberType);
				else
					Reporting.AddCapacityLine("Weld Capacity = " + WWeldCap + " << " + R + ConstUnit.Force + " (NG)", R / WWeldCap, capacityText, beam.MemberType);

				if (shearClipAngle.h != 0)
				{
					Reporting.AddHeader("Check Angle Leg for Tension");
					double capacity = ConstNum.FIOMEGA0_9N * shearClipAngle.Material.Fy * shearClipAngle.Length * shearClipAngle.Size.t;
					if (capacity >= shearClipAngle.h)
						Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + ConstString.FIOMEGA0_9 + "  Fy * L * t = " + ConstString.FIOMEGA0_9 + "   * " + shearClipAngle.Material.Fy + " * " + shearClipAngle.Length + " * " + shearClipAngle.Size.t + " = " + capacity + " >= " + shearClipAngle.h + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + ConstString.FIOMEGA0_9 + "  Fy * L * t = " + ConstString.FIOMEGA0_9 + "   * " + shearClipAngle.Material.Fy + " * " + shearClipAngle.Length + " * " + shearClipAngle.Size.t + " = " + capacity + " << " + shearClipAngle.h + ConstUnit.Force + " (NG)");
				}

				capacityText = ConstString.DES_OR_ALLOWABLE + " Shear Strength of Beam Web";
				Reporting.AddGoToHeader(capacityText + ":");
				MiscCalculationsWithReporting.BlockShearForWelded(beam.MemberType, ref BsCap, EShearCarriedBy.ClipAngle);
				if (beam.WinConnect.Beam.TopCope || beam.WinConnect.Beam.BottomCope)
					Cope.CopedBeam(beam.MemberType, true);
			}

			capacityText = "Shear Strength of Angle(s)";
			Reporting.AddGoToHeader(ConstString.DES_OR_ALLOWABLE + " " + capacityText);
			Ag = shearClipAngle.Length * shearClipAngle.Size.t;
			Reporting.AddHeader("Shear Yielding " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
			Reporting.AddLine("Gross Area (Ag) = L * t = " + shearClipAngle.Length + " * " + shearClipAngle.Size.t + " = " + Ag + ConstUnit.Area);
			beam.WinConnect.Fema.Vg = shearClipAngle.Number * ConstNum.FIOMEGA1_0N * 0.6 * Ag * shearClipAngle.Material.Fy;

			
			if (shearClipAngle.Number == 2)
			{
				if (beam.WinConnect.Fema.Vg >= V)
				{
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * " + ConstString.FIOMEGA1_0 + " * 0.6 * Ag * Fy = 2 * " + ConstString.FIOMEGA1_0 + " * 0.6 * " + Ag + " * " + shearClipAngle.Material.Fy);
					Reporting.AddCapacityLine(" = " + beam.WinConnect.Fema.Vg + " >= " + V + ConstUnit.Force + " (OK)", V / beam.WinConnect.Fema.Vg, capacityText, beam.MemberType);
				}
				else
					Reporting.AddCapacityLine(ConstString.PHI + " Rn = 2 * " + ConstString.FIOMEGA1_0 + " * 0.6 * Ag * Fy = 2 * " + ConstString.FIOMEGA1_0 + " * 0.6 * " + Ag + " * " + shearClipAngle.Material.Fy + " = " + beam.WinConnect.Fema.Vg + " << " + V + ConstUnit.Force + " (NG)", V / beam.WinConnect.Fema.Vg, capacityText, beam.MemberType);
			}
			else
			{
				if (beam.WinConnect.Fema.Vg >= V)
					Reporting.AddCapacityLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA1_0 + " * 0.6 * Ag * Fy = " + ConstString.FIOMEGA1_0 + " * 0.6 * " + Ag + " * " + shearClipAngle.Material.Fy + " = " + beam.WinConnect.Fema.Vg + " >= " + V + ConstUnit.Force + " (OK)", V / beam.WinConnect.Fema.Vg, capacityText, beam.MemberType);
				else
					Reporting.AddCapacityLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA1_0 + " * 0.6 * Ag * Fy = " + ConstString.FIOMEGA1_0 + " * 0.6 * " + Ag + " * " + shearClipAngle.Material.Fy + " = " + beam.WinConnect.Fema.Vg + " << " + V + ConstUnit.Force + " (NG)", V / beam.WinConnect.Fema.Vg, capacityText, beam.MemberType);
			}

			if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted || shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
				Reporting.AddHeader("Shear Rupture " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
			if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted && shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
			{
				AnOsl = (shearClipAngle.Length - shearClipAngle.BoltOslOnSupport.NumberOfRows * (shearClipAngle.BoltOslOnSupport.HoleWidth + ConstNum.SIXTEENTH_INCH)) * shearClipAngle.Size.t;
				Anw = (shearClipAngle.Length - shearClipAngle.BoltWebOnBeam.NumberOfRows * (shearClipAngle.BoltWebOnBeam.HoleWidth + ConstNum.SIXTEENTH_INCH)) * shearClipAngle.Size.t;
				An = Math.Min(AnOsl, Anw);

				Reporting.AddHeader("Net Area on Osl (An1):");
				Reporting.AddLine("= (L - n * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t = (" + shearClipAngle.Length + " - " + shearClipAngle.BoltOslOnSupport.NumberOfRows + " * (" + shearClipAngle.BoltOslOnSupport.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + shearClipAngle.Size.t + " = " + AnOsl + ConstUnit.Area);

				Reporting.AddHeader("Net Area on Beam Side Leg (An2):");
				Reporting.AddLine("= (L - n * dh + " + ConstNum.SIXTEENTH_INCH + ") * t = (" + shearClipAngle.Length + " - " + shearClipAngle.BoltWebOnBeam.NumberOfRows + " * (" + shearClipAngle.BoltWebOnBeam.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + shearClipAngle.Size.t + " = " + Anw + ConstUnit.Area);
				Reporting.AddLine("An = Min(An1, An2)= " + An + ConstUnit.Area);

			}
			else if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted && shearClipAngle.SupportSideConnection == EConnectionStyle.Welded)
			{
				An = (shearClipAngle.Length - shearClipAngle.BoltWebOnBeam.NumberOfRows * (shearClipAngle.BoltWebOnBeam.HoleWidth + ConstNum.SIXTEENTH_INCH)) * shearClipAngle.Size.t;
				Reporting.AddHeader("Net Area on Beam Side Leg (An):");
				Reporting.AddLine("= (L - n * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t = (" + shearClipAngle.Length + " - " + shearClipAngle.BoltWebOnBeam.NumberOfRows + " * (" + shearClipAngle.BoltWebOnBeam.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + shearClipAngle.Size.t + " = " + An + ConstUnit.Area);
				Reporting.AddLine("An = " + An + ConstUnit.Area);
			}
			else if (shearClipAngle.BeamSideConnection == EConnectionStyle.Welded && shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
			{
				An = (shearClipAngle.Length - shearClipAngle.BoltOslOnSupport.NumberOfRows * (shearClipAngle.BoltOslOnSupport.HoleWidth + ConstNum.SIXTEENTH_INCH)) * shearClipAngle.Size.t;
				Reporting.AddLine("Net Area on Osl (An):");
				Reporting.AddLine("= (L - n * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
				Reporting.AddLine("= (" + shearClipAngle.Length + " - " + shearClipAngle.BoltOslOnSupport.NumberOfRows + " * ( " + shearClipAngle.BoltOslOnSupport.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + shearClipAngle.Size.t + " = " + AnOsl + ConstUnit.Area);
				Reporting.AddLine("An = " + An + ConstUnit.Area);
			}

			capacityText = "Net Area on Osl (An)";
			if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted || shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
			{
				double Vn = shearClipAngle.Number * ConstNum.FIOMEGA0_75N * 0.6 * An * shearClipAngle.Material.Fu;
				if (shearClipAngle.Number == 2)
				{
					if (Vn >= V)
						Reporting.AddCapacityLine(ConstString.PHI + " Rn = 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * An * Fu = 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + An + " * " + shearClipAngle.Material.Fu + " = " + Vn + " >= " + V + ConstUnit.Force + " (OK)", V / Vn, capacityText, beam.MemberType);
					else
						Reporting.AddCapacityLine(ConstString.PHI + " Rn = 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * An * Fu = 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + An + " * " + shearClipAngle.Material.Fu + " = " + Vn + " << " + V + ConstUnit.Force + " (NG)", V / Vn, capacityText, beam.MemberType);
				}
				else
				{
					if (Vn >= V)
						Reporting.AddCapacityLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.6 * An * Fu = " + ConstString.FIOMEGA0_75 + " * 0.6 * " + An + " * " + shearClipAngle.Material.Fu + " = " + Vn + " >= " + V + ConstUnit.Force + " (OK)", V / Vn, capacityText, beam.MemberType);
					else
						Reporting.AddCapacityLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.6 * An * Fu = " + ConstString.FIOMEGA0_75 + " * 0.6 * " + An + " * " + shearClipAngle.Material.Fu + " = " + Vn + " << " + V + ConstUnit.Force + " (NG)", V / Vn, capacityText, beam.MemberType);
				}
			}

			if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted)
			{
				Reporting.AddHeader("Block Shear Strength of Beamside Leg of One Angle:");

				Lgt = shearClipAngle.BoltWebOnBeam.EdgeDistTransvDir;
				Lnt = shearClipAngle.BoltWebOnBeam.EdgeDistTransvDir - (shearClipAngle.BoltWebOnBeam.HoleLength + ConstNum.SIXTEENTH_INCH) / 2;
				Lgv = (shearClipAngle.BoltWebOnBeam.NumberOfRows - 1) * shearClipAngle.BoltWebOnBeam.SpacingLongDir + shearClipAngle.BoltWebOnBeam.EdgeDistLongDir;
				Lnv = Lgv - (shearClipAngle.BoltWebOnBeam.NumberOfRows - 0.5) * (shearClipAngle.BoltWebOnBeam.HoleWidth + ConstNum.SIXTEENTH_INCH);

                Reporting.AddLine(String.Empty);
                Reporting.AddLine("Gross Length with Tension resistance (Lgt) = Lh = " + Lgt + ConstUnit.Length);

                Reporting.AddLine(String.Empty);
				Reporting.AddLine("Net Length with Tension resistance (Lnt)");
				Reporting.AddLine("= Lgt - (dh + " + ConstNum.SIXTEENTH_INCH + ") / 2 = " + Lgt + " - " + (shearClipAngle.BoltWebOnBeam.HoleLength + ConstNum.SIXTEENTH_INCH) + " / 2 = " + Lnt + ConstUnit.Length);

                Reporting.AddLine(String.Empty);
				Reporting.AddLine("Gross Length with Shear resistance (Lgv)");
				Reporting.AddLine("= (n - 1) * s + Lv");
				Reporting.AddLine("= (" + shearClipAngle.BoltWebOnBeam.NumberOfRows + " - 1) * " + shearClipAngle.BoltWebOnBeam.SpacingLongDir + " + " + shearClipAngle.BoltWebOnBeam.EdgeDistLongDir + " = " + Lgv + ConstUnit.Length);

                Reporting.AddLine(String.Empty);
				Reporting.AddLine("Net Length with Shear resistance (Lnv)");
				Reporting.AddLine("= Lgv - (n - 0.5) * (dv + " + ConstNum.SIXTEENTH_INCH + ")");
				Reporting.AddLine("= " + Lgv + " - (" + shearClipAngle.BoltWebOnBeam.NumberOfRows + " - 0.5) * (" + (shearClipAngle.BoltWebOnBeam.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH) + ")");
				Reporting.AddLine("= " + Lnv + ConstUnit.Length);

				PhiRn = MiscCalculationsWithReporting.BlockShearNew(shearClipAngle.Material.Fu, Lnv, 1, Lnt, Lgv, shearClipAngle.Material.Fy, shearClipAngle.Size.t, shearClipAngle.V, true);
			}
			if (shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
			{
				Reporting.AddHeader("Block Shear Strength of Supportside Leg of One Angle:");
				Lgt = shearClipAngle.BoltOslOnSupport.EdgeDistTransvDir;
				Lnt = shearClipAngle.BoltOslOnSupport.EdgeDistTransvDir - (shearClipAngle.BoltOslOnSupport.HoleLength + ConstNum.SIXTEENTH_INCH) / 2;
				Lgv = (shearClipAngle.BoltOslOnSupport.NumberOfRows - 1) * shearClipAngle.BoltOslOnSupport.SpacingLongDir + shearClipAngle.BoltOslOnSupportEdgeDistanceLong;
				Lnv = Lgv - (shearClipAngle.BoltOslOnSupport.NumberOfRows - 0.5) * (shearClipAngle.BoltOslOnSupport.HoleWidth + ConstNum.SIXTEENTH_INCH);

                Reporting.AddLine(String.Empty);
				Reporting.AddLine("Gross Length with Tension resistance (Lgt) = Lh = " + Lgt + ConstUnit.Length);

                Reporting.AddLine(String.Empty);
                Reporting.AddLine("Net Length with Tension resistance (Lnt)");
				Reporting.AddLine("= Lgt - (dh + " + ConstNum.SIXTEENTH_INCH + ") / 2 = " + Lgt + " - " + (shearClipAngle.BoltOslOnSupport.HoleLength + ConstNum.SIXTEENTH_INCH) + " / 2 = " + Lnt + ConstUnit.Length);

                Reporting.AddLine(String.Empty);
				Reporting.AddLine("Gross Length with Shear resistance (Lgv)");
				Reporting.AddLine("= (n - 1) * s + Lv");
				Reporting.AddLine("= (" + shearClipAngle.BoltOslOnSupport.NumberOfRows + " - 1) * " + shearClipAngle.BoltOslOnSupport.SpacingLongDir + " + " + shearClipAngle.BoltOslOnSupportEdgeDistanceLong + " = " + Lgv + ConstUnit.Length);

                Reporting.AddLine(String.Empty);
				Reporting.AddLine("Net Length with Shear resistance (Lnv)");
				Reporting.AddLine("= Lgv - (n - 0.5) * (dv + " + ConstNum.SIXTEENTH_INCH + ")");
				Reporting.AddLine("= " + Lgv + " - (" + shearClipAngle.BoltOslOnSupport.NumberOfRows + " - 0.5) * (" + (shearClipAngle.BoltOslOnSupport.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH) + ")");
				Reporting.AddLine("= " + Lnv + ConstUnit.Length);

				PhiRn = ConstNum.FIOMEGA0_75N * (0.6 * Math.Min(shearClipAngle.Material.Fu * Lnv, shearClipAngle.Material.Fy * Lgv) + 1 * shearClipAngle.Material.Fu * Lnt) * shearClipAngle.Size.t;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * (0.6 * Min(Fu * Lnv, Fy * Lgv) + Fu * Lnt) * t");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * (0.6 * Min(" + shearClipAngle.Material.Fu + " * " + Lnv + ", " + shearClipAngle.Material.Fy + " * " + Lgv + ") + " + 1 + " * " + shearClipAngle.Material.Fu + " * " + Lnt + ") * " + shearClipAngle.Size.t);
				if (PhiRn >= shearClipAngle.V)
					Reporting.AddCapacityLine("= " + PhiRn + " >= " + shearClipAngle.V + ConstUnit.Force + " (OK)", shearClipAngle.V / PhiRn, "Net Length with Shear resistance (Lnv)", beam.MemberType);
				else
					Reporting.AddCapacityLine("= " + PhiRn + " << " + shearClipAngle.V + ConstUnit.Force + " (NG)", shearClipAngle.V / PhiRn, "Net Length with Shear resistance (Lnv)", beam.MemberType);

				if (shearClipAngle.Number == 1 && shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
				{
					string deduct;
					string decuct1;
					double FiMn;

					Reporting.AddHeader("Bending of Angle Supportside Leg:");

					e = beam.GageOnFlange / 2;
					Mu = shearClipAngle.V * e;
					Zx = shearClipAngle.Size.t * Math.Pow(shearClipAngle.Length, 2) / 4;
					nn = shearClipAngle.BoltOslOnSupport.NumberOfRows / 2;
					n1 = Math.Pow(shearClipAngle.BoltOslOnSupport.NumberOfRows, 2) - 1;
					beam.WinConnect.Fema.L = shearClipAngle.Length;
					d = shearClipAngle.BoltOslOnSupport.HoleLength;
                    if ((int)NumberFun.Round(n, ERoundingPrecision.Half, ERoundingStyle.RoundDown) == n)
					{
						ZnetDeduction = Math.Pow(nn, 2) * shearClipAngle.BoltOslOnSupport.SpacingLongDir * (d + ConstNum.SIXTEENTH_INCH) * shearClipAngle.Size.t;
						decuct1 = "(Nb / 2)^ 2 * s * (dh +" + ConstNum.SIXTEENTH_INCH + ") * t";
						deduct = nn + " ^ 2 * " + shearClipAngle.BoltOslOnSupport.SpacingLongDir + " * (" + d + " + " + ConstNum.SIXTEENTH_INCH + ") * " + shearClipAngle.Size.t;
					}
					else
					{
						ZnetDeduction = n1 / 4 * shearClipAngle.BoltOslOnSupport.SpacingLongDir * (d + ConstNum.SIXTEENTH_INCH) * shearClipAngle.Size.t;
						decuct1 = "(Nb² - 1) * s * (dh +" + ConstNum.SIXTEENTH_INCH + ") * t";
						deduct = n1 + " / 4 * " + shearClipAngle.BoltOslOnSupport.SpacingLongDir + " * (" + d + " + " + ConstNum.SIXTEENTH_INCH + ") * " + shearClipAngle.Size.t;
					}
					Znet = shearClipAngle.Size.t * Math.Pow(beam.WinConnect.Fema.L, 2) / 4 - ZnetDeduction;

					Reporting.AddLine("Bending Moment (Mu) = Ru * e = " + shearClipAngle.V + " * " + e + " = " + Mu + ConstUnit.MomentUnitInch);
					Reporting.AddLine("Gross Section Modulus (Zx) = (t * L²) / 4");
					Reporting.AddLine("= " + shearClipAngle.Size.t + " * " + shearClipAngle.Length + "² / 4 = " + Zx + ConstUnit.SecMod);

					Reporting.AddLine("Net Section Modulus (Znet)= Zx - " + decuct1);
					Reporting.AddLine("= " + Zx + " - " + deduct);
					Reporting.AddLine("= " + Znet + ConstUnit.SecMod);

					Reporting.AddLine("Flexural Yielding Strength:");
					FiMn = ConstNum.FIOMEGA0_9N * shearClipAngle.Material.Fy * Zx;
					Reporting.AddLine(ConstString.PHI + "Mn = " + ConstString.FIOMEGA0_9 + " * Fy * Zx = " + ConstString.FIOMEGA0_9 + " * " + shearClipAngle.Material.Fy + " * " + Zx);
					if (FiMn >= Mu)
						Reporting.AddLine("= " + FiMn + " >= " + Mu + ConstUnit.MomentUnitInch + " (OK)");
					else
						Reporting.AddLine("= " + FiMn + " << " + Mu + ConstUnit.MomentUnitInch + " (NG)");

					Reporting.AddLine("Flexural Rupture Strength:");
					FiMn = ConstNum.FIOMEGA0_75N * shearClipAngle.Material.Fu * Znet;
					Reporting.AddLine(ConstString.PHI + "Mn = " + ConstString.FIOMEGA0_75 + " * Fu * Znet = " + ConstString.FIOMEGA0_75 + " * " + shearClipAngle.Material.Fu + " * " + Znet);
					if (FiMn >= Mu)
						Reporting.AddLine("= " + FiMn + " >= " + Mu + ConstUnit.MomentUnitInch + " (OK)");
					else
						Reporting.AddLine("= " + FiMn + " << " + Mu + ConstUnit.MomentUnitInch + " (NG)");
				}
			}

			if (beam.AxialCompression > 0 && beam.AxialTension > 0)
				SmallMethodsDesign.ColumnAndBeamCheck(beam.MemberType, beam.WinConnect.ShearClipAngle.BoltOnColumn);

			if (shearClipAngle.h != 0)
				Stiff.ColumnLocalStress(beam.MemberType);
		}

		private static void GetGage(EMemberType memberType, ref double ColumnGageMin, ref double ColumnGageMax)
		{
			int Rnbs = 0;
			int Lnbs = 0;
			double RColumnGageMin = 0;
			double LColumnGageMin = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];

			DetailData leftBeam, rightBeam;

			rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BeamToHSSColumn:
				case EJointConfiguration.BeamToColumnFlange:
					ColumnGageMin = beam.GageOnFlange;
					ColumnGageMax = beam.GageOnFlange;
					break;
				case EJointConfiguration.BeamToColumnWeb:
				case EJointConfiguration.BeamToGirder:
					if (rightBeam.WinConnect.ShearClipAngle.BeamSideConnection == EConnectionStyle.Bolted)
						Rnbs = 2;
					else
						Rnbs = 1;
					if (leftBeam.WinConnect.ShearClipAngle.BeamSideConnection == EConnectionStyle.Bolted)
						Lnbs = 2;
					else
						Lnbs = 1;
					RColumnGageMin = rightBeam.Shape.tw + 2 * (ConstNum.HALF_INCH + MiscCalculationsWithReporting.AngleGageMinimum(rightBeam.WinConnect.ShearClipAngle.BoltOslOnSupport.BoltSize, Rnbs));
					LColumnGageMin = leftBeam.Shape.tw + 2 * (ConstNum.HALF_INCH + MiscCalculationsWithReporting.AngleGageMinimum(leftBeam.WinConnect.ShearClipAngle.BoltOslOnSupport.BoltSize, Lnbs));
					ColumnGageMin = Math.Max(RColumnGageMin, LColumnGageMin);
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
						ColumnGageMax = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.t - 2 * Math.Max(rightBeam.WinConnect.ShearClipAngle.BoltOslOnSupport.EdgeDistTransvDir, leftBeam.WinConnect.ShearClipAngle.BoltOslOnSupport.EdgeDistTransvDir);
					else
						ColumnGageMax = 100;
					break;
				default:
					ColumnGageMin = ConstNum.FIVEANDHALF_INCHES;
					ColumnGageMax = ConstNum.FIVEANDHALF_INCHES;
					break;
			}
		}

		private static void GetNumberOfOSLBolts(EMemberType memberType, double MinL, ref int MinBolts, ref double FiRn_perBolt, ref int n, ref double agage, ref double c, ref double Cap)
		{
			var shearClipAngle = CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearClipAngle;

			if (!shearClipAngle.BoltOslOnSupport.NumberOfRows_User)
			{
				if (shearClipAngle.Number == 2)
				{
					MinBolts = (int)Math.Max(2, (Math.Floor((MinL - 2 * shearClipAngle.BoltOslOnSupportEdgeDistanceLong) / shearClipAngle.BoltOslOnSupport.SpacingLongDir) + 1));
					shearClipAngle.BoltOslOnSupport.NumberOfRows = (int)Math.Max(MinBolts, BoltsForTension.CalcBoltsForTension(memberType, shearClipAngle.BoltOslOnSupport, shearClipAngle.h, shearClipAngle.V, 0, false));
					FiRn_perBolt = BoltsForTension.CalcBoltsForTension(memberType, shearClipAngle.BoltOslOnSupport, shearClipAngle.h, shearClipAngle.V, shearClipAngle.BoltOslOnSupport.NumberOfRows, false);
				}
				else
				{
					MinBolts = (int)Math.Max(2, ((Math.Floor(MinL - 2 * shearClipAngle.BoltOslOnSupportEdgeDistanceLong) / shearClipAngle.BoltOslOnSupport.SpacingLongDir) + 1));
					shearClipAngle.BoltOslOnSupport.NumberOfRows = (int)Math.Max(MinBolts, BoltsForTension.CalcBoltsForTension(memberType, shearClipAngle.BoltOslOnSupport, shearClipAngle.h, shearClipAngle.V, 0, false));
					n = shearClipAngle.BoltOslOnSupport.NumberOfRows;
					agage = CommonDataStatic.DetailDataDict[memberType].GageOnFlange / 2;
					n--;
					do
					{
						n++;
						MiscCalculationsWithReporting.EccentricBolt(agage, shearClipAngle.BoltOslOnSupport.SpacingLongDir, 0, 1, n, ref c);
						Cap = c * shearClipAngle.BoltOslOnSupport.BoltStrength;
					} while (Cap < Math.Sqrt(Math.Pow(shearClipAngle.V, 2) + Math.Pow(shearClipAngle.h, 2)));
					shearClipAngle.BoltOslOnSupport.NumberOfRows = n;
					FiRn_perBolt = BoltsForTension.CalcBoltsForTension(memberType, shearClipAngle.BoltOslOnSupport, shearClipAngle.h, n / c * shearClipAngle.V, shearClipAngle.BoltOslOnSupport.NumberOfRows, false);
				}
			}
		}

		private static void GetNumberOfWebBolts(EMemberType memberType, double MinL, ref int MinBolts, double MaxL, double Rload, ref int n, ref double ecc, ref double c, ref double Cap, ref double BsCap, ref double BeamTearOutCap, ref double BeamBlockShearCap)
		{
			int maxbolts = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var shearClipAngle = CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearClipAngle;

			MinBolts = (int)Math.Max(2, (Math.Floor((MinL - 2 * shearClipAngle.BoltWebOnBeam.EdgeDistLongDir) / shearClipAngle.BoltWebOnBeam.SpacingLongDir)) + 1);
			maxbolts = (int)Math.Floor((MaxL - 2 * shearClipAngle.BoltWebOnBeam.EdgeDistLongDir) / shearClipAngle.BoltWebOnBeam.SpacingLongDir) + 1;
			n = (int)Math.Ceiling(Rload / shearClipAngle.BoltWebOnBeam.BoltStrength);
			ecc = beam.EndSetback + beam.WinConnect.Beam.Lh;

			if (!shearClipAngle.BoltWebOnBeam.NumberOfRows_User)
			{
				if (ecc >= ConstNum.THREE_INCHES)
				{
					shearClipAngle.BoltWebOnBeam.NumberOfRows = Math.Max(MinBolts, n) - 1;
					do
					{
						shearClipAngle.BoltWebOnBeam.NumberOfRows++;
						MiscCalculationsWithReporting.EccentricBolt(ecc, shearClipAngle.BoltWebOnBeam.SpacingLongDir, 0, 1, shearClipAngle.BoltWebOnBeam.NumberOfRows, ref c);
						Cap = c * shearClipAngle.BoltWebOnBeam.BoltStrength;
					} while (Cap < Rload);
				}
				else
					shearClipAngle.BoltWebOnBeam.NumberOfRows = Math.Max(MinBolts, n);

				do
				{
					MiscCalculationsWithReporting.BlockShear(memberType, EShearOpt.ClipAngle, ref BsCap, false);
					if (BsCap < beam.ShearForce)
						shearClipAngle.BoltWebOnBeam.NumberOfRows++;
				} while (!(shearClipAngle.BoltWebOnBeam.NumberOfRows >= maxbolts || BsCap >= beam.ShearForce));
				do
				{
					BeamTearOutCap = MiscCalculationsWithReporting.BeamWebTearout(beam.MemberType, shearClipAngle.BoltWebOnBeam.NumberOfRows, 1, shearClipAngle.BoltWebOnBeam.SpacingLongDir, 0, EBeamOrAttachment.Beam, ETearOutBlockShear.TearOut, false);
					BeamBlockShearCap = MiscCalculationsWithReporting.BeamWebTearout(beam.MemberType, shearClipAngle.BoltWebOnBeam.NumberOfRows, 1, shearClipAngle.BoltWebOnBeam.SpacingLongDir, 0, EBeamOrAttachment.Beam, ETearOutBlockShear.BlockShear, false);
					if (BeamTearOutCap > 0 && BeamTearOutCap < beam.P || BeamBlockShearCap > 0 && BeamBlockShearCap < beam.P)
						shearClipAngle.BoltWebOnBeam.NumberOfRows++;
				} while (!(shearClipAngle.BoltWebOnBeam.NumberOfRows >= maxbolts || BeamTearOutCap == 0 || BeamBlockShearCap == 0 || BeamTearOutCap >= beam.P && BeamBlockShearCap >= beam.P));
			}
		}

		private static void GetLengthForWebBolted(EMemberType memberType, ref double LengthWB, double MinL)
		{
			var shearClipAngle = CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearClipAngle;

			LengthWB = (shearClipAngle.BoltWebOnBeam.NumberOfRows - 1) * shearClipAngle.BoltWebOnBeam.SpacingLongDir + 2 * MiscCalculationsWithReporting.ShearedEdgeforAngle(shearClipAngle.BoltWebOnBeam.BoltSize); // WBolts.eal
			if (shearClipAngle.BoltStagger != EBoltStagger.None)
				LengthWB = (LengthWB + shearClipAngle.BoltWebOnBeam.SpacingLongDir / 2);
			if (LengthWB < MinL && !shearClipAngle.BoltWebOnBeam.NumberOfRows_User)
			{
				shearClipAngle.BoltWebOnBeam.NumberOfRows++;
				LengthWB = (shearClipAngle.BoltWebOnBeam.NumberOfRows - 1) * shearClipAngle.BoltWebOnBeam.SpacingLongDir + 2 * MiscCalculationsWithReporting.ShearedEdgeforAngle(shearClipAngle.BoltWebOnBeam.BoltSize); // WBolts.eal
				if (shearClipAngle.BoltStagger != EBoltStagger.None)
					LengthWB = (LengthWB + shearClipAngle.BoltWebOnBeam.SpacingLongDir / 2);
			}
		}

		private static void GetLengthForOSLBolted(EMemberType memberType, ref double LengthOB, double MinL)
		{
			var shearClipAngle = CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearClipAngle;

			LengthOB = (shearClipAngle.BoltOslOnSupport.NumberOfRows - 1) * shearClipAngle.BoltOslOnSupport.SpacingLongDir + 2 * shearClipAngle.BoltOslOnSupportEdgeDistanceLong;
			if (shearClipAngle.BoltStagger != EBoltStagger.None)
				LengthOB = (LengthOB + shearClipAngle.BoltOslOnSupport.SpacingLongDir / 2);
			if (LengthOB < MinL)
			{
				if (!shearClipAngle.BoltOslOnSupport.NumberOfRows_User)
					shearClipAngle.BoltOslOnSupport.NumberOfRows++;
				LengthOB = (shearClipAngle.BoltOslOnSupport.NumberOfRows - 1) * shearClipAngle.BoltOslOnSupport.SpacingLongDir + 2 * shearClipAngle.BoltOslOnSupportEdgeDistanceLong;
				if (shearClipAngle.BoltStagger != EBoltStagger.None)
					LengthOB = (LengthOB + shearClipAngle.BoltOslOnSupport.SpacingLongDir / 2);
			}
		}

		private static void GetRequiredShearArea(EMemberType memberType, ref double AgReq, ref double Anreq)
		{
			var shearClipAngle = CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearClipAngle;

			AgReq = shearClipAngle.V / (ConstNum.FIOMEGA1_0N * 0.6 * shearClipAngle.Material.Fy);
			Anreq = shearClipAngle.V / (ConstNum.FIOMEGA0_75N * 0.6 * shearClipAngle.Material.Fu);
		}

		private static void GetRequiredThForOSLBearing(EMemberType memberType, ref double HoleSize, ref double Fbe, ref double si, ref double Fbs, ref double BearingCap1, ref double tOslBReq)
		{
			var shearClipAngle = CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearClipAngle;

			HoleSize = shearClipAngle.LengthOfOSL;
			Fbe = CommonCalculations.EdgeBearing(shearClipAngle.BoltOslOnSupportEdgeDistanceLong, shearClipAngle.LengthOfOSL, shearClipAngle.BoltOslOnSupport.BoltSize, shearClipAngle.Material.Fu, shearClipAngle.BoltOslOnSupport.HoleType, false);
			si = shearClipAngle.BoltOslOnSupport.SpacingLongDir;
			Fbs = CommonCalculations.SpacingBearing(shearClipAngle.BoltOslOnSupport.SpacingLongDir, shearClipAngle.LengthOfOSL, shearClipAngle.BoltOslOnSupport.BoltSize, shearClipAngle.BoltOslOnSupport.HoleType, shearClipAngle.Material.Fu, false);
			BearingCap1 = Fbe + Fbs * (shearClipAngle.BoltOslOnSupport.NumberOfRows - 1);
			tOslBReq = shearClipAngle.V / BearingCap1;
		}

		private static void GetRequiredThForWebBearing(EMemberType memberType, ref double HoleSize, ref double Fbe, ref double si, ref double Fbs, ref double BearingCap1, ref double tWBreq)
		{
			var shearClipAngle = CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearClipAngle;

			HoleSize = shearClipAngle.BoltWebOnBeam.HoleLength;
			Fbe = CommonCalculations.EdgeBearing(shearClipAngle.BoltWebOnBeam.EdgeDistLongDir, shearClipAngle.BoltWebOnBeam.HoleWidth, shearClipAngle.BoltWebOnBeam.BoltSize, shearClipAngle.Material.Fu, shearClipAngle.BoltWebOnBeam.HoleType, false);
			si = shearClipAngle.BoltWebOnBeam.SpacingLongDir;
			Fbs = CommonCalculations.SpacingBearing(shearClipAngle.BoltWebOnBeam.SpacingLongDir, shearClipAngle.BoltWebOnBeam.HoleWidth, shearClipAngle.BoltWebOnBeam.BoltSize, shearClipAngle.BoltWebOnBeam.HoleType, shearClipAngle.Material.Fu, false);
			BearingCap1 = Fbe + Fbs * (shearClipAngle.BoltWebOnBeam.NumberOfRows - 1);
			tWBreq = shearClipAngle.V / BearingCap1;
		}

		private static void GetLengthForWebWelded(EMemberType memberType, ref double w, double Rload, ref double LengthWW, double MaxL, double MinL)
		{
			w = ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.DetailDataDict[memberType].Shape.tw * CommonDataStatic.DetailDataDict[memberType].Material.Fu / (CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearClipAngle.Number * 0.707 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearClipAngle.Weld.Fexx);
			if (w > NumberFun.ConvertFromFraction(5))
				w = NumberFun.ConvertFromFraction(5);
			if (w > 0)
			{
				if (CommonDataStatic.Units == EUnit.US)
					LengthWW = (Rload / w - 60) / 26;
				else
					LengthWW = (Rload / w - 10507) / 179.25;
			}

			LengthWW = NumberFun.Round(LengthWW, 8);
			if (LengthWW > MaxL)
				Reporting.AddLine("Angle too long.");
			if (LengthWW < MinL)
				LengthWW = MinL;
		}

		private static void GetThicknessForBlockShear(EMemberType memberType, double reOsl, double OslLeg, ref double Lgt, ref double Lnt, ref double Lgv, ref double Lnv, ref double tReqforBlockShearofOSL, double WLeg, ref double tReqforBlockShearofWebLeg, bool enableReporting)
		{
			var shearClipAngle = CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearClipAngle;

			if (shearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
			{
				Lgt = Math.Max(reOsl, OslLeg - (CommonDataStatic.DetailDataDict[memberType].GageOnFlange - CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.tw) / 2);
				Lnt = Lgt - (shearClipAngle.LengthOfOSL + ConstNum.SIXTEENTH_INCH) / 2;
				Lgv = (shearClipAngle.BoltOslOnSupport.NumberOfRows - 1) * shearClipAngle.BoltOslOnSupport.SpacingLongDir + shearClipAngle.BoltOslOnSupportEdgeDistanceLong;
				Lnv = Lgv - (shearClipAngle.BoltOslOnSupport.NumberOfRows - 0.5) * (shearClipAngle.LengthOfOSL + ConstNum.SIXTEENTH_INCH);
				tReqforBlockShearofOSL = shearClipAngle.V / MiscCalculationsWithReporting.BlockShearNew(shearClipAngle.Material.Fu, Lnv, 1, Lnt, Lgv, shearClipAngle.Material.Fy, 1, 0, enableReporting);
			}
			else
				tReqforBlockShearofOSL = 0;
			if (shearClipAngle.BeamSideConnection == EConnectionStyle.Bolted)
			{
				Lgt = WLeg - (CommonDataStatic.DetailDataDict[memberType].EndSetback + CommonDataStatic.DetailDataDict[memberType].WinConnect.Beam.Lh);
				Lnt = (Lgt - (shearClipAngle.BoltWebOnBeam.HoleLength + ConstNum.SIXTEENTH_INCH) / 2);
				Lgv = (shearClipAngle.BoltWebOnBeam.NumberOfRows - 1) * shearClipAngle.BoltWebOnBeam.SpacingLongDir + shearClipAngle.BoltWebOnBeam.EdgeDistLongDir;
				Lnv = Lgv - (shearClipAngle.BoltWebOnBeam.NumberOfRows - 0.5) * (shearClipAngle.BoltWebOnBeam.HoleWidth + ConstNum.SIXTEENTH_INCH);
				if (Lnt >= 0.6 * Lnv)
					tReqforBlockShearofWebLeg = shearClipAngle.V / (ConstNum.FIOMEGA0_75N * (0.6 * shearClipAngle.Material.Fy * Lgv + shearClipAngle.Material.Fu * Lnt));
				else
					tReqforBlockShearofWebLeg = shearClipAngle.V / (ConstNum.FIOMEGA0_75N * (0.6 * shearClipAngle.Material.Fu * Lnv + shearClipAngle.Material.Fy * Lgt));
				tReqforBlockShearofWebLeg = shearClipAngle.V / MiscCalculationsWithReporting.BlockShearNew(shearClipAngle.Material.Fu, Lnv, 1, Lnt, Lgv, shearClipAngle.Material.Fy, 1, 0, enableReporting);
			}
			else
				tReqforBlockShearofWebLeg = 0;
		}

		private static void GetLengthForOSLWelded(EMemberType memberType, double t, ref double e, double SupThickness, double SupFy, ref double LengthOslW, double MaxL, double MinL)
		{
			double t1 = 0;
			double L2 = 0;
			double P1 = 0;
			double w1 = 0;
			double L3 = 0;

			var shearClipAngle = CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearClipAngle;

			t1 = t == 0 ? NumberFun.ConvertFromFraction(5) : t;

			if (shearClipAngle.LengthOfOSL == 0)
				e = ConstNum.THREE_INCHES;
			else
				e = shearClipAngle.LengthOfOSL;

			CommonDataStatic.DetailDataDict[memberType].WinConnect.Fema.L1 = (shearClipAngle.V / (ConstNum.FIOMEGA1_0N * 0.6 * SupThickness * SupFy));
			L2 = shearClipAngle.V / (ConstNum.FIOMEGA1_0N * 0.6 * t1 * shearClipAngle.Material.Fy);
			P1 = shearClipAngle.V;
			w1 = t1 < NumberFun.ConvertFromFraction(4) ? t1 : t1 - ConstNum.SIXTEENTH_INCH;
			w1 = Math.Min(w1, Math.Max(ConstNum.QUARTER_INCH, shearClipAngle.WeldSizeSupport));

			L3 = Math.Sqrt((Math.Pow(P1, 2) + Math.Sqrt(Math.Pow(P1, 4) + 5.2488 * Math.Pow(w1 * e * shearClipAngle.Weld.Fexx * P1, 2))) / (0.2025 * Math.Pow(shearClipAngle.Weld.Fexx * w1, 2)));

			LengthOslW = Math.Max(Math.Max(CommonDataStatic.DetailDataDict[memberType].WinConnect.Fema.L1, L2), L3);

			if (LengthOslW > MaxL)
				LengthOslW = MaxL;
			if (LengthOslW < MinL)
				LengthOslW = MinL;

			LengthOslW = NumberFun.Round(LengthOslW, 8);
		}

		private static void GetThicknessForBending(EMemberType memberType, double Gage, ref double e, ref double Mu, ref double Zx, ref int nn, ref double n1, ref double d, double n, ref double ZnetDeduction, ref double Znet, ref double tReqforBending)
		{
			var shearClipAngle = CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearClipAngle;

			e = Gage / 2;
			Mu = CommonDataStatic.DetailDataDict[memberType].ShearForce * e;
			Zx = Math.Pow(shearClipAngle.Length, 2) / 4;
			nn = shearClipAngle.BoltOslOnSupport.NumberOfRows / 2;
			n1 = Math.Pow(shearClipAngle.BoltOslOnSupport.NumberOfRows, 2) - 1;
			CommonDataStatic.DetailDataDict[memberType].WinConnect.Fema.L = shearClipAngle.Length;
			d = shearClipAngle.LengthOfOSL;
			if (((int)Math.Floor(n / 2)) * 2 == n)
				ZnetDeduction = Math.Pow(nn, 2) * shearClipAngle.BoltOslOnSupport.SpacingLongDir * (d + ConstNum.SIXTEENTH_INCH);
			else
				ZnetDeduction = n1 / 4 * shearClipAngle.BoltOslOnSupport.SpacingLongDir * (d + ConstNum.SIXTEENTH_INCH);
			Znet = Math.Pow(CommonDataStatic.DetailDataDict[memberType].WinConnect.Fema.L, 2) / 4 - ZnetDeduction;

			tReqforBending = Math.Max(Mu / (ConstNum.FIOMEGA0_9N * shearClipAngle.Material.Fy * Zx), Mu / (ConstNum.FIOMEGA0_75N * shearClipAngle.Material.Fu * Znet));
		}
	}
}