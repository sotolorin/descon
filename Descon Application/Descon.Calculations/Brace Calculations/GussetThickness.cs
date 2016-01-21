using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public class GussetThickness
	{
		public static double CalcGussetThickness(EMemberType memberType)
		{
			double gussetThickness = 0;

			double FiRnC = 0;
			string OKWhitYieldComp = "";
			string OKWhitYieldTen = "";
			string OKBearComp = "";
			string OKBearTen = "";
			bool CompositSectionOK = false;
			bool WhitmoreBuckOK = false;
			bool WhitmoreYielOKComp = false;
			bool WhitmoreYielOKTen = false;
			bool TearOutWOK = false;
			bool TearOutCOK = false;
			bool BearingOKTen = false;
			bool BearingOKComp = false;
			double BucklingCapComp = 0;
			double TearOutWCap = 0;
			double BearingWCapComp = 0;
			double BearingWCapTen = 0;
			double BearingCCapComp = 0;
			double BearingCCapTen = 0;
			double FiRnPerThC = 0;
			double LntC = 0;
			double LnvC = 0;
			double LgvC = 0;
			double anglegage = 0;
			double fsz = 0;
			double FpsWeb = 0;
			double FpeWeb = 0;
			double FpsClaw = 0;
			double FpeClaw = 0;
			double HoleSizeCT = 0;
			double HoleSizeCL = 0;
			double WhitmSectStressComp = 0;
			double WhitmSectStressTen = 0;
			double WhitmoreLw2 = 0;
			double FiRn = 0;
			double Ant = 0;
			double Agt = 0;
			double Anv = 0;
			double Agv = 0;
			double Gussetdevelopes = 0;
			double Gth = 0;
			double H_Comp = 0;
			double H_Ten = 0;
			double R = 0;
			double H = 0;
			double V = 0;
			double gthickness = 0;
			double tmin = 0;
			bool WhitmoreBucklingTest = false;
			double WhitmoreSectStressForBuckling = 0;
			double UtilizationFactor = 0;
			double Pn = 0;
			double r_comb = 0;
			double Mu = 0;
			double ex = 0;
			double Pu = 0;
			double Ag = 0;
			double Mn = 0;
			double bt3 = 0;
			double bt2 = 0;
			double bt = 0;
			double WhitmSectStressForBuckling;
			double WhitmSectStress = 0;
			double BucklingCap = 0;
			double KLoR = 0;
			double WhitmoreYieldCap = 0;
			double TearOutCCap = 0;
			double Lcr = 0;
			double WhitmoreLw = 0;
			double WhitmoreC = 0;
			double WhitmoreB = 0;
			double WhitmoreOut = 0;
			double L3 = 0;
			double L2 = 0;
			double L1 = 0;
			double slpm = 0;
			double N = 0;
			double g2 = 0;
			double g1 = 0;
			double Lw2 = 0;
			double Lw1 = 0;
			double WhitmoreLw1 = 0;
			double wmaxlimit = 0;
			double w1 = 0;
			double fraverage = 0;
			double fr = 0;
			double BeamWMom = 0;
			double Fb = 0;
			double BeamVfrc = 0;
			double fvv = 0;
			double BeamHfrc = 0;
			double fh = 0;
			double weldlength = 0;
			double Gap = 0;
			bool beamweldok = false;
			bool printout = false;
			double FiRnPerThW = 0;
			double LntW = 0;
			double LnvW = 0;
			double LgtW = 0;
			double LgvW = 0;
			double HoleSizeWT = 0;
			double HoleSizeWL = 0;
			double t1 = 0;
			double t0 = 0;
			double At0 = 0;
			double Av0 = 0;
			double dd = 0;
			double t = 0;
			double slpm1 = 0;
			double SignOfTangent = 0;
			double slpc = 0;
			double slpb = 0;

			//Iw = (m - 3) * 6 + 7;                         Indicates SplicePlates.Bolts
			//ic = Iw + 2;                                  Indicates ClawAngles.Bolts

			t = 0.375;

			DetailData beam;
			DetailData brace;
			DetailData otherSide;
			DetailData sameSide;
			String beamStr = string.Empty;

			switch (memberType)
			{
				case EMemberType.UpperLeft:
					memberType = EMemberType.UpperLeft;
					brace = CommonDataStatic.DetailDataDict[memberType];
					otherSide = CommonDataStatic.DetailDataDict[EMemberType.UpperRight];
					sameSide = CommonDataStatic.DetailDataDict[EMemberType.LowerLeft];
					break;
				case EMemberType.LowerLeft:
					memberType = EMemberType.LowerLeft;
					brace = CommonDataStatic.DetailDataDict[memberType];
					otherSide = CommonDataStatic.DetailDataDict[EMemberType.LowerRight];
					sameSide = CommonDataStatic.DetailDataDict[EMemberType.UpperLeft];
					break;
				case EMemberType.UpperRight:
					memberType = EMemberType.UpperRight;
					brace = CommonDataStatic.DetailDataDict[memberType];
					otherSide = CommonDataStatic.DetailDataDict[EMemberType.UpperLeft];
					sameSide = CommonDataStatic.DetailDataDict[EMemberType.LowerRight];
					break;
				default:
					memberType = EMemberType.LowerRight;
					brace = CommonDataStatic.DetailDataDict[memberType];
					otherSide = CommonDataStatic.DetailDataDict[EMemberType.LowerLeft];
					sameSide = CommonDataStatic.DetailDataDict[EMemberType.UpperRight];
					break;
			}

			if (memberType == EMemberType.LowerLeft || memberType == EMemberType.UpperLeft)
				beam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			else
				beam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BraceToColumn:
					beamStr = !(brace.KBrace | brace.KneeBrace) ? "Beam" : "";
					break;
				case EJointConfiguration.BraceVToBeam:
					beamStr = "Beam";
					break;
				case EJointConfiguration.BraceToColumnBase:
					beamStr = "Base Plate";
					break;
			}

			// Don't do any gusset calcs if the user set the value
			if (brace.BraceConnect.Gusset.Thickness_User)
				return brace.BraceConnect.Gusset.Thickness;

			gussetThickness = brace.BraceConnect.Gusset.Thickness;

			if (brace.BraceConnect.Gusset.ColumnSideSetback < 0 || brace.ShapeType == EShapeType.HollowSteelSection && brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
			{
				if (brace.BraceConnect.Gusset.Thickness == 0)
				{
					switch (brace.BraceToGussetWeldedOrBolted)
					{
						case EConnectionStyle.Welded:
							if (brace.ShapeType == EShapeType.WTSection)
								dd = brace.Shape.bf;
							else
								dd = brace.Shape.d;
							switch (brace.ShapeType)
							{
								case EShapeType.WTSection:
								case EShapeType.SingleAngle:
								case EShapeType.SingleChannel:
									t = 1.414 * brace.BraceConnect.Brace.BraceWeld.Weld1sz * CommonDataStatic.Preferences.DefaultElectrode.Fexx / brace.BraceConnect.Gusset.Material.Fu;
									break;
								case EShapeType.DoubleAngle:
								case EShapeType.HollowSteelSection:
								case EShapeType.DoubleChannel:
									t = 0.707 * brace.BraceConnect.Brace.BraceWeld.Weld1sz * CommonDataStatic.Preferences.DefaultElectrode.Fexx / brace.BraceConnect.Gusset.Material.Fu;
									break;
							}
							if (brace.ShapeType == EShapeType.SingleAngle || brace.ShapeType == EShapeType.DoubleAngle)
							{
								Av0 = 2 * brace.BraceConnect.Brace.BraceWeld.Weld1L;
								At0 = dd;
							}
							else
							{
								Av0 = 2 * brace.BraceConnect.Brace.BraceWeld.Weld1L;
								At0 = dd;
							}
							t0 = brace.AxialTension / (ConstNum.FIOMEGA0_75N * (0.6 * brace.BraceConnect.Gusset.Material.Fy * Av0 + brace.BraceConnect.Gusset.Material.Fu * At0));
							if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
							{
								if (memberType == EMemberType.LowerLeft || memberType == EMemberType.UpperLeft)
									t = Math.Max(t, otherSide.BraceConnect.Gusset.Thickness);
							}
							else
								t = Math.Max(ConstNum.THREE_EIGHTS_INCH, Math.Max(t, t0));
							if (brace.KBrace)
							{
								if (memberType == EMemberType.UpperLeft || memberType == EMemberType.UpperRight)
									t1 = Math.Max(brace.BraceConnect.Gusset.Thickness, sameSide.BraceConnect.Gusset.Thickness);
								else
									t1 = Math.Max(brace.BraceConnect.Gusset.Thickness, sameSide.BraceConnect.Gusset.Thickness);
								t0 = Math.Max(t1, t0);
							}
							t = Math.Max(ConstNum.THREE_EIGHTS_INCH, Math.Max(t, t0));
							break;
						case EConnectionStyle.Bolted:
							if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
								t = Math.Max(brace.BraceConnect.Gusset.Thickness, otherSide.BraceConnect.Gusset.Thickness);
							else
								t = ConstNum.THREE_EIGHTS_INCH;
							if (brace.KBrace)
								t = Math.Max(brace.BraceConnect.Gusset.Thickness, sameSide.BraceConnect.Gusset.Thickness);
							else
								t = ConstNum.THREE_EIGHTS_INCH;
							if (brace.ShapeType == EShapeType.HollowSteelSection && brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
							{
								HoleSizeWL = brace.BraceConnect.Gusset.HoleLongP;
								HoleSizeWT = brace.BraceConnect.Gusset.HoleTransP;
								LgvW = 2 * (brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir * (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) + brace.BraceConnect.SplicePlates.Bolt.EdgeDistGusset);
								LgtW = (brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir * (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1));
								LnvW = 2 * (brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir * (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) + brace.BraceConnect.SplicePlates.Bolt.EdgeDistGusset - (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) * (HoleSizeWL + ConstNum.SIXTEENTH_INCH));
								LntW = ((brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir - (HoleSizeWT + ConstNum.SIXTEENTH_INCH)) * (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1));
								FiRnPerThW = ConstNum.FIOMEGA0_75N * (0.6 * Math.Min(brace.BraceConnect.Gusset.Material.Fy * LgvW, brace.BraceConnect.Gusset.Material.Fu * LnvW) + 1 * brace.BraceConnect.Gusset.Material.Fu * LntW);
								t0 = brace.AxialTension / FiRnPerThW;
							}
							else
								t0 = SmallMethodsDesign.NBRequiredForGussetBlockShear(memberType, 2, brace.BoltBrace.NumberOfBolts);
							break;
					}
					if (t < t0)
						t = t0;
					SmallMethodsDesign.GussetShear(memberType, ref t);
				}
				else
					t = brace.BraceConnect.Gusset.Thickness;

				do
				{
					// Mb = ((int)Math.Floor(m / 4.5)) + 1;                     // This is beam
					brace.BraceConnect.Gusset.Thickness = t;
					if (beam.IsActive)
					{
						SmallMethodsDesign.GussetThickness_ForceEnvelop(memberType, ref BeamHfrc, ref BeamVfrc, ref BeamWMom);
						if (brace.GussetToBeamConnection == EBraceConnectionTypes.DirectlyWelded)
						{
							Gap = Math.Abs(beam.EndSetback + beam.WinConnect.Beam.TCopeL -
							               brace.BraceConnect.Gusset.ColumnSideSetback);
							weldlength = brace.BraceConnect.Gusset.Hb - Gap;

							fh = Math.Abs(BeamHfrc / weldlength);
							fvv = Math.Abs(BeamVfrc / weldlength);
							Fb = Math.Abs(6 * BeamWMom / Math.Pow(weldlength, 2));


							fr = Math.Sqrt(Math.Pow(fh, 2) + Math.Pow(fvv + Fb, 2));
							fraverage = Math.Sqrt(Math.Pow(fh, 2) + Math.Pow(fvv, 2));
							w1 = (Math.Max(1.25 * fraverage, fr) /
							      (ConstNum.FIOMEGA0_75N * 0.6 * Math.Sqrt(2) * CommonDataStatic.Preferences.DefaultElectrode.Fexx));

							if ((memberType == EMemberType.LowerLeft || memberType == EMemberType.UpperLeft) || CommonDataStatic.BeamToColumnType != EJointConfiguration.BraceVToBeam)
							{
								SmallMethodsDesign.GussetWelds(memberType, ref t, BeamHfrc, BeamVfrc, ref wmaxlimit, ref w1);
								if (!brace.BraceConnect.Gusset.BeamWeldSize_User)
									brace.BraceConnect.Gusset.BeamWeldSize = w1;
								if (w1 > wmaxlimit)
									t = t * w1 / wmaxlimit;
							}
							if ((memberType == EMemberType.UpperRight || memberType == EMemberType.LowerRight) &&
							    CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
								wmaxlimit = w1;
							beamweldok = brace.BraceConnect.Gusset.BeamWeldSize >= w1 && w1 <= wmaxlimit || t > ConstNum.THREE_INCHES;
						}
						else
							beamweldok = true;
					}
					else
						beamweldok = true;

					if (!beamweldok)
						t += (CommonDataStatic.Units == EUnit.US ? ConstNum.SIXTEENTH_INCH : 1);

					if (brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Welded)
					{
						if (brace.ShapeType == EShapeType.WTSection)
							WhitmoreLw1 = brace.BraceConnect.Brace.BraceWeld.Weld1L * 1.1547F + brace.Shape.bf;
						else if (brace.ShapeType == EShapeType.SingleAngle || brace.ShapeType == EShapeType.DoubleAngle)
						{
							Lw1 = 1.1547 * brace.BraceConnect.Brace.BraceWeld.Weld1L;
							Lw2 = brace.BraceConnect.Brace.BraceWeld.Weld1L * 0.57735F +
							      brace.BraceConnect.Brace.BraceWeld.Weld2L * 0.57735F + brace.Shape.d;
							if (Lw1 >= Lw2)
								WhitmoreLw1 = Lw1;
							else
								WhitmoreLw1 = Lw2;
						}
						else
							WhitmoreLw1 = brace.BraceConnect.Brace.BraceWeld.Weld1L * 1.1547F + brace.Shape.d;
					}
					else // Brace is bolted
					{
						if (brace.ShapeType == EShapeType.WTSection)
						{
							g1 = brace.GageOnFlange;
							g2 = Math.Min(ConstNum.THREE_INCHES,
								NumberFun.Round(
									(brace.Shape.bf - g1) / 2 - (brace.BoltBrace.MinEdgeRolled + brace.BoltBrace.Eincr),
									ERoundingPrecision.Fourth, ERoundingStyle.RoundDown));
							if (brace.BoltBrace.NumberOfRows == 2)
							{
								N =
									-((int)
										Math.Floor((double)-brace.BoltBrace.NumberOfBolts /
										           brace.BoltBrace.NumberOfRows));
								if (brace.BraceConnect.Brace.BoltsAreStaggered && brace.BoltBrace.NumberOfBolts % 2 == 0)
								{
									WhitmoreLw1 = 2 * g2 + g1 + (brace.BoltBrace.NumberOfBolts - N - 1) * brace.BoltBrace.SpacingLongDir * 1.1547;
									Lw2 = (N - 0.5) * brace.BoltBrace.SpacingLongDir * 1.1547 + g1;
								}
								else if (brace.BraceConnect.Brace.BoltsAreStaggered)
								{
									WhitmoreLw1 = 2 * g2 + g1 + (brace.BoltBrace.NumberOfBolts - N - 0.5) * brace.BoltBrace.SpacingLongDir * 1.1547;
									Lw2 = (N - 1) * brace.BoltBrace.SpacingLongDir * 1.1547 + g1;
								}
								else if (brace.BoltBrace.NumberOfBolts % 2 == 0)
								{
									WhitmoreLw1 = (N - 1) * brace.BoltBrace.SpacingLongDir * 1.1547 + 2 * g2 + g1;
									Lw2 = 0;
								}
								else
								{
									WhitmoreLw1 = 2 * g2 + g1 +
									              (brace.BoltBrace.NumberOfBolts - N - 1) * brace.BoltBrace.SpacingLongDir * 1.1547F;
									Lw2 = (N - 1) * brace.BoltBrace.SpacingLongDir * 1.1547 + g1;
								}
								if (WhitmoreLw1 < Lw2)
									WhitmoreLw1 = Lw2;
							}
							else
								WhitmoreLw1 = (brace.BoltBrace.NumberOfBolts - 1) * brace.BoltBrace.SpacingLongDir * 1.1547F + brace.GageOnFlange;

						}
						else if (brace.ShapeType == EShapeType.SingleAngle || brace.ShapeType == EShapeType.DoubleAngle)
						{
							g2 = CommonCalculations.AngleStandardGage(2, brace.Shape.d);
							N = -(Math.Floor((double)-brace.BoltBrace.NumberOfBolts / brace.BoltBrace.NumberOfRows));
							if (brace.BoltBrace.NumberOfRows == 2)
							{
								if (brace.BraceConnect.Brace.BoltsAreStaggered && brace.BoltBrace.NumberOfBolts % 2 == 0)
								{
									WhitmoreLw1 = (N - 0.5) * brace.BoltBrace.SpacingLongDir * 0.57735 + g2 +
									              (brace.BoltBrace.NumberOfBolts - N - 1) * brace.BoltBrace.SpacingLongDir * 0.57735;
									Lw2 = (N - 0.5) * brace.BoltBrace.SpacingLongDir * 1.1547;
								}
								else if (brace.BraceConnect.Brace.BoltsAreStaggered)
								{
									WhitmoreLw1 = (N - 1) * brace.BoltBrace.SpacingLongDir * 0.57735 + g2 +
									              (brace.BoltBrace.NumberOfBolts - N - 0.5) * brace.BoltBrace.SpacingLongDir * 0.57735;
									Lw2 = (N - 1) * brace.BoltBrace.SpacingLongDir * 1.1547;
								}
								else if (brace.BoltBrace.NumberOfBolts % 2 == 0)
								{
									WhitmoreLw1 = (N - 1) * brace.BoltBrace.SpacingLongDir * 0.57735 + g2 +
									              (brace.BoltBrace.NumberOfBolts - N - 1) * brace.BoltBrace.SpacingLongDir * 0.57735;
									Lw2 = 0;
								}
								else
								{
									WhitmoreLw1 = (N - 1) * brace.BoltBrace.SpacingLongDir * 0.57735 + g2 +
									              (brace.BoltBrace.NumberOfBolts - N - 1) * brace.BoltBrace.SpacingLongDir * 0.57735;
									Lw2 = (N - 1) * brace.BoltBrace.SpacingLongDir * 1.1547;
								}
								if (WhitmoreLw1 < Lw2)
									WhitmoreLw1 = Lw2;
							}
							else
								WhitmoreLw1 = (brace.BoltBrace.NumberOfBolts - 1) * brace.BoltBrace.SpacingLongDir * 1.1547;
						}
						else if (brace.ShapeType == EShapeType.SingleChannel || brace.ShapeType == EShapeType.DoubleChannel)
						{
							WhitmoreLw1 = (brace.BoltBrace.NumberOfBolts / brace.BoltBrace.NumberOfRows - 1) *
							              brace.BoltBrace.SpacingLongDir * 1.1547 +
							              (brace.BoltBrace.NumberOfRows - 1) * brace.BoltBrace.SpacingTransvDir;
						}
						else if (brace.ShapeType == EShapeType.HollowSteelSection &&
						         brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
						{
							// ijk = 7 + (m - 3)*6;         Indicates SplicePlates.Bolts
							WhitmoreLw1 = (brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts /
							               brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) *
							              brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir * 1.1547 +
							              (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) *
							              brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir;
						}
					}

					//i1 = 25 + (m - 3) * 5;
					//if (brace.ShapeType == EShapeType.SingleAngle || brace.ShapeType == EShapeType.DoubleAngle)
					//{
					//	// L
					//	if (brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
					//	{
					//		if (brace.OSLOnBeamSide)
					//		{
					//			i2 = 452 + 6 * (m - 3);
					//			i3 = 450 + 6 * (m - 3);
					//		}
					//		else
					//		{
					//			i2 = 450 + 6 * (m - 3);
					//			i3 = 452 + 6 * (m - 3);
					//		}
					//		if (WhitmoreLinesStartTogether)
					//		{
					//			i2 = 450 + 6 * (m - 3);
					//			i3 = i2;
					//		}
					//		if (brace.BraceBolt.NumberOfBolts % 2 == 0)
					//		{
					//			i4 = 455 + 6 * (m - 3);
					//		}
					//		else
					//		{
					//			i4 = 454 + 6 * (m - 3);
					//		}
					//	}
					//	else
					//	{
					//		if (LongWeldControls)
					//		{
					//			i2 = 302 + 8 * (m - 3);
					//			i3 = 302 + 8 * (m - 3);
					//		}
					//		else
					//		{
					//			if (brace.OSLOnBeamSide)
					//			{
					//				i2 = 304 + 8 * (m - 3);
					//				i3 = 302 + 8 * (m - 3);
					//			}
					//			else
					//			{
					//				i2 = 302 + 8 * (m - 3);
					//				i3 = 304 + 8 * (m - 3);
					//			}
					//		}
					//		i4 = 301 + 8 * (m - 3);
					//	}
					//}
					//else if (brace.ShapeType == EShapeType.WTSection && brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
					//{
					//	if (WhitmoreLinesStartTogether)
					//	{
					//		i2 = 453 + 6 * (m - 3);
					//		i3 = 451 + 6 * (m - 3);
					//	}
					//	else
					//	{
					//		i2 = 475 + 4 * (m - 3);
					//		i3 = 477 + 4 * (m - 3);
					//	}
					//	i4 = 455 + 6 * (m - 3);

					//}
					//else if ((brace.ShapeType == EShapeType.SingleChannel || brace.ShapeType == EShapeType.DoubleChannel) &&
					//		 brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
					//{
					//	i2 = 431 + 4 * (m - 3);
					//	i3 = 433 + 4 * (m - 3);
					//	i4 = 455 + 6 * (m - 3);
					//}
					//else if (brace.ShapeType == EShapeType.HollowSteelSection &&
					//		 brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
					//{
					//	i2 = 431 + 4 * (m - 3);
					//	i3 = 433 + 4 * (m - 3);
					//	i4 = 455 + 6 * (m - 3);
					//}
					//else
					//{
					//	i2 = 171 + (m - 3) * 18;
					//	i3 = 162 + (m - 3) * 18;
					//	i4 = 301 + 8 * (m - 3);
					//}

					//slpm = Math.Tan(brace.Angle * ConstNum.RADIAN);
					//slpm1 = -1 / Math.Tan(brace.Angle * ConstNum.RADIAN);
					//SignOfTangent = Math.Sign(Math.Tan(brace.Angle * ConstNum.RADIAN));
					//slpc = Math.Tan((brace.Angle - SignOfTangent * 30) * ConstNum.RADIAN);
					////  slop of 30o line on column side (Line C)
					//slpb = Math.Tan((brace.Angle + SignOfTangent * 30) * ConstNum.RADIAN); //  slop of 30o line on beam side (Line B)
					//if (BRACE1.x[i1 + 2] == BRACE1.x[i1 + 1])
					//	slpVFree = 100;
					//else
					//	slpVFree = (BRACE1.y[i1 + 2] - BRACE1.y[i1 + 1]) / (BRACE1.x[i1 + 2] - BRACE1.x[i1 + 1]);
					//if (BRACE1.y[i1 + 4] == BRACE1.y[i1 + 3])
					//	slpHFree = 0;
					//else
					//	slpHFree = (BRACE1.y[i1 + 4] - BRACE1.y[i1 + 3]) / (BRACE1.x[i1 + 4] - BRACE1.x[i1 + 3]);
					//SmallMethodsBrace.Intersect(slpm1, BRACE1.x[i4], BRACE1.y[i4], slpHFree, BRACE1.x[i1 + 3], BRACE1.y[i1 + 3],
					//	ref xHFree, ref yHFree);
					//SmallMethodsBrace.Intersect(slpm1, BRACE1.x[i4], BRACE1.y[i4], slpVFree, BRACE1.x[i1 + 2], BRACE1.y[i1 + 2],
					//	ref xVFree, ref yVFree);
					//SmallMethodsBrace.Intersect(slpm1, BRACE1.x[i4], BRACE1.y[i4], 100, BRACE1.x[i1], BRACE1.y[i1], ref xColumn,
					//	ref yColumn); //  Intersection of Whitmore Line and Column Face

					//L1 = SmallMethodsBrace.PointToLineDistance(((int)slpm1), BRACE1.x[i4], BRACE1.y[i4], BRACE1.x[i1], BRACE1.y[i1]);

					//if ((brace.KneeBrace | brace.KBrace) ||
					//	brace.WinConnect.ShearClipAngle.position == EPosition.NoConnection && BRACE1.JointType != 1)
					//{
					//	SmallMethodsBrace.Intersect(100, BRACE1.x[i1], BRACE1.y[i1], slpm, brace.WorkPointX, brace.WorkPointY,
					//		ref xc, ref Yc); //  Intersection of Brace CL and Column Face
					//	L1 = SmallMethodsBrace.PointToLineDistance(((int)slpm1), BRACE1.x[i4], BRACE1.y[i4], xc, Yc);
					//	//  Dist. from Edge of guset to line W along Brace CL
					//	if (brace.WinConnect.ShearClipAngle.position == EPosition.NoConnection)
					//	{
					//		SmallMethodsBrace.Intersect(0, BRACE1.x[i1], BRACE1.y[i1], slpm, brace.WorkPointX, brace.WorkPointY,
					//			ref xe, ref ye); //  Intersection of Brace CL and gusset horiz. edge
					//		L11 = SmallMethodsBrace.PointToLineDistance(((int)slpm1), BRACE1.x[i4], BRACE1.y[i4], xe, ye);
					//		//  Dist. from Edge of guset to line W along Brace CL
					//		L1 = Math.Min(L1, L11);
					//	}
					//}
					//else if (CommonDataStatic.JointConfigurationVisual == EJointConfiguration.BraceVToBeam)
					//{
					//	SmallMethodsBrace.Intersect(0, BRACE1.x[i1], BRACE1.y[i1], slpm, brace.WorkPointX, brace.WorkPointY,
					//		ref xc, ref Yc); //  Intersection of Brace CL and Beam Face
					//	L1 = SmallMethodsBrace.PointToLineDistance(((int)slpm1), BRACE1.x[i4], BRACE1.y[i4], xc, Yc);
					//	//  Dist. from Edge of guset to line W along Brace CL

					//}
					//else
					//	L1 = SmallMethodsBrace.PointToLineDistance(((int)slpm1), BRACE1.x[i4], BRACE1.y[i4], BRACE1.x[i1], BRACE1.y[i1]); //  Dist. from corner of guset to line W


					//if (!brace.BraceConnect.ClawAngles.DoNotConnectFlanges)
					//{
					//}
					//else
					//{
					//	SmallMethodsBrace.Intersect(slpm1, BRACE1.x[i4], BRACE1.y[i4], slpc, BRACE1.x[i2], BRACE1.y[i2], ref x0,
					//		ref y0);
					//	BRACE1.x[431 + (m - 3) * 4] = BRACE1.x[i2];
					//	BRACE1.y[431 + (m - 3) * 4] = BRACE1.y[i2];
					//	BRACE1.x[432 + (m - 3) * 4] = x0;
					//	BRACE1.y[432 + (m - 3) * 4] = y0;

					//	if (Math.Abs(yHFree) < Math.Abs(y0))
					//	{
					//		y0 = yHFree;
					//		x0 = xHFree;
					//	}
					//	SmallMethodsBrace.Intersect(100, BRACE1.x[i1], BRACE1.y[i1], slpm, x0, y0, ref x1, ref y1);
					//	L2 = Math.Sqrt(Math.Pow(x0 - x1, 2) + Math.Pow(y0 - y1, 2));
					//	SmallMethodsBrace.Intersect(slpm1, BRACE1.x[i4], BRACE1.y[i4], slpb, BRACE1.x[i3], BRACE1.y[i3], ref x2,
					//		ref y2);
					//	BRACE1.x[433 + (m - 3) * 4] = BRACE1.x[i3];
					//	BRACE1.y[433 + (m - 3) * 4] = BRACE1.y[i3];
					//	BRACE1.x[434 + (m - 3) * 4] = x2;
					//	BRACE1.y[434 + (m - 3) * 4] = y2;

					//	if (Math.Abs(xVFree) < Math.Abs(x2))
					//	{
					//		y2 = yVFree;
					//		x2 = xVFree;
					//	}
					//	if (brace.KBrace | brace.KneeBrace)
					//	{
					//		SmallMethodsBrace.Intersect(100, BRACE1.x[i1], BRACE1.y[i1], slpm, x2, y2, ref x3, ref y3);
					//	}
					//	else if (CommonDataStatic.JointConfigurationVisual == EJointConfiguration.BraceVToBeam)
					//	{
					//		SmallMethodsBrace.Intersect(0, BRACE1.x[i1], BRACE1.y[i1], slpm, x2, y2, ref x3, ref y3);
					//	}
					//	else
					//	{
					//		SmallMethodsBrace.Intersect(0, BRACE1.x[i1], BRACE1.y[i1], slpm, x2, y2, ref x3, ref y3);
					//	}
					//	L3 = Math.Sqrt(Math.Pow(x2 - x3, 2) + Math.Pow(y2 - y3, 2));
					//}
					//NewLw1 = Math.Sqrt(Math.Pow(x2 - x0, 2) + Math.Pow(y2 - y0, 2));
					//WhitmoreOut = -NewLw1 + WhitmoreLw1;
					//BRACE1.x[400 + 6 * (m - 3)] = x0;
					//BRACE1.y[400 + 6 * (m - 3)] = y0;
					//BRACE1.x[401 + 6 * (m - 3)] = x1;
					//BRACE1.y[401 + 6 * (m - 3)] = y1;
					//BRACE1.x[402 + 6 * (m - 3)] = x2;
					//BRACE1.y[402 + 6 * (m - 3)] = y2;
					//BRACE1.x[403 + 6 * (m - 3)] = x3;
					//BRACE1.y[403 + 6 * (m - 3)] = y3;
					//BRACE1.x[404 + 6 * (m - 3)] = xHFree;
					//BRACE1.y[404 + 6 * (m - 3)] = yHFree;
					//BRACE1.x[405 + 6 * (m - 3)] = xVFree;
					//BRACE1.y[405 + 6 * (m - 3)] = yVFree;
					//switch (memberType)
					//{
					//	case EMemberType.UpperRight:
					//		L2 = L2 * Math.Sign((x0 - BRACE1.x[i1]));
					//		if (!(brace.KBrace | brace.KneeBrace))
					//		{
					//			L3 = L3 * Math.Sign((y2 - BRACE1.y[i1]));
					//		}
					//		break;
					//	case EMemberType.LowerRight:
					//		L2 = L2 * Math.Sign((x0 - BRACE1.x[i1]));
					//		if (!(brace.KBrace | brace.KneeBrace))
					//		{
					//			L3 = L3 * Math.Sign((-y2 + BRACE1.y[i1]));
					//		}
					//		break;
					//	case EMemberType.UpperLeft:
					//		L2 = L2 * Math.Sign((-x0 + BRACE1.x[i1]));
					//		if (!(brace.KBrace | brace.KneeBrace))
					//		{
					//			L3 = L3 * Math.Sign((y2 - BRACE1.y[i1]));
					//		}
					//		break;
					//	case EMemberType.LowerLeft:
					//		L2 = L2 * Math.Sign((-x0 + BRACE1.x[i1]));
					//		if (!(brace.KBrace | brace.KneeBrace))
					//		{
					//			L3 = L3 * Math.Sign((-y2 + BRACE1.y[i1]));
					//		}
					//		break;
					//}

					if (brace.KBrace || brace.KneeBrace)
						WhitmoreB = 0;
					else
						WhitmoreB = -L3 * Math.Abs(slpm);
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
						WhitmoreC = 0;
					else if (L2 != 0)
						WhitmoreC = -L2 / Math.Abs(slpm);
					if (WhitmoreC < 0)
						WhitmoreC = 0;
					if (WhitmoreB < 0)
						WhitmoreB = 0;
					if (WhitmoreOut < 0)
						WhitmoreOut = 0;

					WhitmoreLw = (Math.Max(0, WhitmoreLw1 - WhitmoreB - WhitmoreC - WhitmoreOut));
					if (L2 < 0)
						L2 = 0;
					if (L3 < 0)
						L3 = 0;
					if ((brace.KBrace | brace.KneeBrace) ||
					    brace.WinConnect.ShearClipAngle.Position == EPosition.NoConnection ||
					    CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam ||
					    brace.BraceConnect.Gusset.DontConnectBeam)
						Lcr = 1.2 * L1;
					else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
						Lcr = 1.2 * L1;
					else if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic)
						Lcr = 1.2 * (L1 + L2 + L3) / 3;
					else
						Lcr = 0.5 * (L1 + L2 + L3) / 3;
					if (Lcr < 0)
						Lcr = 0;

					while (t <= ConstNum.FOUR_INCHES)
					{
						if (brace.BraceConnect.ClawAngles.DoNotConnectFlanges)
							TearOutCCap = 0;
						if (column.WebOrientation == EWebOrientation.InPlane)
						{
							WhitmoreYieldCap = ConstNum.FIOMEGA0_9N *
							                   (WhitmoreLw * t * brace.BraceConnect.Gusset.Material.Fy + WhitmoreB * beam.Shape.tw * beam.Material.Fy +
							                    WhitmoreC * column.Shape.tw * column.Material.Fy);
						}
						else
							WhitmoreYieldCap = ConstNum.FIOMEGA0_9N * (WhitmoreLw * t * brace.BraceConnect.Gusset.Material.Fy + WhitmoreB * beam.Material.Fy * beam.Shape.tw);
						KLoR = 3.464 * Lcr / t;

						BucklingCap = ConstNum.FIOMEGA0_9N * CommonCalculations.BucklingStress(KLoR, brace.BraceConnect.Gusset.Material.Fy, false);

						if (column.WebOrientation == EWebOrientation.InPlane)
						{
							WhitmSectStress = brace.BraceConnect.Brace.MaxForce /
							                  (WhitmoreLw * t + WhitmoreB * beam.Shape.tw + WhitmoreC * column.Shape.tw);
							WhitmSectStressForBuckling = brace.AxialCompression /
							                             (WhitmoreLw * t + WhitmoreB * beam.Shape.tw + WhitmoreC * column.Shape.tw);
						}
						else
						{
							WhitmSectStress = brace.BraceConnect.Brace.MaxForce / (WhitmoreLw * t + WhitmoreB * beam.Shape.tw);
							WhitmSectStressForBuckling = brace.AxialCompression /
							                             (WhitmoreLw * t + WhitmoreB * beam.Shape.tw);
						}

						if (brace.ShapeType == EShapeType.HollowSteelSection && brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
						{
							if (column.WebOrientation == EWebOrientation.InPlane)
							{
								bt = WhitmoreLw * t + WhitmoreB * beam.Shape.tw + WhitmoreC * column.Shape.tw;
								bt2 = WhitmoreLw * Math.Pow(t, 2) + WhitmoreB * Math.Pow(beam.Shape.tw, 2) +
								      WhitmoreC * Math.Pow(column.Shape.tw, 2);
								bt3 = WhitmoreLw * Math.Pow(t, 3) + WhitmoreB * Math.Pow(beam.Shape.tw, 3) +
								      WhitmoreC * Math.Pow(column.Shape.tw, 3);
								Mn = (brace.BraceConnect.Gusset.Material.Fy * WhitmoreLw * Math.Pow(t, 2) +
								      beam.Material.Fy * WhitmoreB * Math.Pow(beam.Shape.tw, 2) +
								      column.Material.Fy * WhitmoreC * Math.Pow(column.Shape.tw, 2)) / 4;
							}
							else
							{
								bt = WhitmoreLw * t + WhitmoreB * beam.Shape.tw;
								bt2 = WhitmoreLw * Math.Pow(t, 2) + WhitmoreB * Math.Pow(beam.Shape.tw, 2);
								bt3 = WhitmoreLw * Math.Pow(t, 3) + WhitmoreB * Math.Pow(beam.Shape.tw, 3);
								Mn = (brace.BraceConnect.Gusset.Material.Fy * WhitmoreLw * Math.Pow(t, 2) +
								      beam.Material.Fy * WhitmoreB * Math.Pow(beam.Shape.tw, 2)) / 4;
							}

							Ag = bt;
							Pu = brace.AxialCompression;
							ex = (t + brace.BraceConnect.SplicePlates.Thickness) / 2;
							Mu = Pu * ex / 2;

							r_comb = Math.Sqrt(bt3 / (12 * bt));
							KLoR = Lcr / r_comb;

							Pn = Ag * CommonCalculations.BucklingStress(KLoR, brace.BraceConnect.Gusset.Material.Fy, false);
							if (Pu / (ConstNum.FIOMEGA0_9N * Pn) >= 0.2)
								UtilizationFactor = Pu / (ConstNum.FIOMEGA0_9N * Pn) + 8 / 9.0 * Mu / (ConstNum.FIOMEGA0_9N * Mn);
							else
								UtilizationFactor = Pu / (2 * ConstNum.FIOMEGA0_9N * Pn) + Mu / (ConstNum.FIOMEGA0_9N * Mn);
						}
						else
							UtilizationFactor = 0;

						if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic && WhitmoreSectStressForBuckling > 0)
							WhitmoreBucklingTest = WhitmoreSectStressForBuckling > BucklingCap;
						else
						{
							WhitmoreBucklingTest = WhitmoreSectStressForBuckling > BucklingCap;
							// And (tensionandcompression Or member(m).Fx < 0))
						}

						if (WhitmoreBucklingTest || brace.BraceConnect.Brace.MaxForce > WhitmoreYieldCap || UtilizationFactor > 1)
							t += (CommonDataStatic.Units == EUnit.US ? ConstNum.SIXTEENTH_INCH : 1);
						else
							break;
					}

					if (!beamweldok || t < ConstNum.FOUR_INCHES)
						t += (CommonDataStatic.Units == EUnit.US ? ConstNum.SIXTEENTH_INCH : 1);

				} while (!beamweldok || t < ConstNum.FOUR_INCHES);

				if (brace.KBrace && (memberType == EMemberType.LowerLeft || memberType == EMemberType.LowerRight))
					KBraceAdditionalChecks.CalcKBraceAdditionalChecks(memberType, ref t, printout);
				else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam && (memberType == EMemberType.LowerLeft || memberType == EMemberType.UpperLeft))
					KBraceAdditionalChecks.CalcKBraceAdditionalChecks(memberType, ref t, printout);
				if (CommonDataStatic.Preferences.UseContinuousClipAngles && !((brace.KBrace | brace.KneeBrace) || CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam) && brace.BraceConnect.Gusset.Thickness == 0.0)
					tmin = Math.Max(sameSide.BraceConnect.Gusset.Thickness, beam.Shape.tw);
				else
					tmin = ConstNum.THREE_EIGHTS_INCH;
				t = Math.Max(t, tmin);
				SmallMethodsDesign.BraceBoltBearingOnGusset(memberType, ref gthickness);
				t = Math.Max(t, gthickness);
				switch (brace.GussetToColumnConnection)
				{
					case EBraceConnectionTypes.ClipAngle:
						if (!brace.WinConnect.ShearClipAngle.AnglesBoltedToGusset)
						{
							ClipAnglesBrace.ClipAngleForces(memberType);
							V = brace.WinConnect.ShearClipAngle.ForceY;
							H = brace.WinConnect.ShearClipAngle.ForceX;
							R = Math.Pow(Math.Pow(V, 2) + Math.Pow(H, 2), 0.5);
							SmallMethodsDesign.GussetandBeamCheckwithWeldedClipAngles(memberType, V, H_Ten, H_Comp, R);
							t = Math.Max(t, brace.BraceConnect.Gusset.Thickness);
						}
						break;
				}
				SmallMethodsDesign.GussetWelds(memberType, ref t, BeamHfrc, BeamVfrc, ref wmaxlimit, ref w1);
				if (w1 > wmaxlimit)
					t = t * w1 / wmaxlimit;

				if (t <= ConstNum.ONE_INCH)
					Gth = NumberFun.Round(t, 16);
				else if (t <= ConstNum.THREE_INCHES)
					Gth = NumberFun.Round(t, 8);
				else
					Gth = NumberFun.Round(t, 4);
				if (Gth > ConstNum.FOUR_INCHES)
					Gth = ConstNum.THREE_INCHES;
				if (CommonDataStatic.Preferences.Units == EUnit.Metric)
					Gth = Math.Ceiling(Gth);

				brace.BraceConnect.Gusset.Thickness = CommonCalculations.PlateThickness(Gth);
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
				{
					brace.BraceConnect.Gusset.Thickness = Math.Max(brace.BraceConnect.Gusset.Thickness, otherSide.BraceConnect.Gusset.Thickness);
					otherSide.BraceConnect.Gusset.Thickness = brace.BraceConnect.Gusset.Thickness;
				}
				else if (brace.KBrace)
				{
					brace.BraceConnect.Gusset.Thickness = Math.Max(brace.BraceConnect.Gusset.Thickness, sameSide.BraceConnect.Gusset.Thickness);
					sameSide.BraceConnect.Gusset.Thickness = brace.BraceConnect.Gusset.Thickness;
				}
				gussetThickness = brace.BraceConnect.Gusset.Thickness;

				Reporting.AddHeader(CommonDataStatic.CommonLists.CompleteMemberList[memberType] + " Gusset Thickness");
				Reporting.AddLine("Try t = " + brace.BraceConnect.Gusset.Thickness);
				if (brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Welded)
				{
					switch (brace.ShapeType)
					{
						case EShapeType.WTSection:
						case EShapeType.HollowSteelSection:
						case EShapeType.SingleChannel:
						case EShapeType.DoubleChannel:
							Gussetdevelopes = 2 * ConstNum.FIOMEGA0_75N * 0.6 * brace.BraceConnect.Gusset.Material.Fu * brace.BraceConnect.Gusset.Thickness * brace.BraceConnect.Brace.BraceWeld.Weld1L;
							Reporting.AddHeader("Maximum Brace Weld Force Gusset Can Develop:");
							Reporting.AddLine("= 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu * t * L");
							Reporting.AddLine("= 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + brace.BraceConnect.Gusset.Material.Fu + " * " + brace.BraceConnect.Gusset.Thickness + " * " + brace.BraceConnect.Gusset.Thickness);
							break;
						case EShapeType.SingleAngle:
						case EShapeType.DoubleAngle:
							Gussetdevelopes = ConstNum.FIOMEGA0_75N * 0.6 * brace.BraceConnect.Gusset.Material.Fu * brace.BraceConnect.Gusset.Thickness * (brace.BraceConnect.Brace.BraceWeld.Weld1L + brace.BraceConnect.Brace.BraceWeld.Weld2L);
							Reporting.AddHeader("Maximum Brace Weld Force Gusset Can Develop:");
							Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu * t * (L1 + L2)");
							Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.6*" + brace.BraceConnect.Gusset.Material.Fu + " * " + brace.BraceConnect.Gusset.Thickness + " * (" + brace.BraceConnect.Gusset.Thickness + " + " + brace.BraceConnect.Brace.BraceWeld.Weld2L + ")");
							break;
					}
					if (Gussetdevelopes >= brace.BraceConnect.Brace.MaxForce)
						Reporting.AddLine("= " + Gussetdevelopes + " >= " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Gussetdevelopes + " << " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

					int argTemp1 = 0;
					argTemp1 = SmallMethodsDesign.NBRequiredForGussetBlockShear(memberType, 1, argTemp1);
				}
				else
				{
					if (brace.ShapeType == EShapeType.HollowSteelSection && brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
					{
						Reporting.AddHeader("Gusset Block Shear:");
						HoleSizeWL = brace.BraceConnect.Gusset.HoleLongP;
						HoleSizeWT = brace.BraceConnect.Gusset.HoleTransP;
						Agv = 2 * (brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir * (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) + brace.BraceConnect.SplicePlates.Bolt.EdgeDistGusset) * brace.BraceConnect.Gusset.Thickness;
						Reporting.AddLine("Agv = 2 * (sl * (nl - 1) + eg) * t");
						Reporting.AddLine("= 2 * (" + brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir + " * (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 1) + " + brace.BraceConnect.SplicePlates.Bolt.EdgeDistGusset + ") * " + brace.BraceConnect.Gusset.Thickness);
						Reporting.AddLine("= " + Agv + ConstUnit.Area);

						Anv = 2 * (brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir * (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 0.5) + brace.BraceConnect.SplicePlates.Bolt.EdgeDistGusset - (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) * (HoleSizeWL + ConstNum.SIXTEENTH_INCH)) * brace.BraceConnect.Gusset.Thickness;
						Reporting.AddLine("Anv = 2 * (sl * (nl - 1) + eg - (nl - 0.5) * (dl + " + ConstNum.SIXTEENTH_INCH + ")) * t");
						Reporting.AddLine("= 2 * (" + brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir + " * (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 1) + " + brace.BraceConnect.SplicePlates.Bolt.EdgeDistGusset + " - (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 0.5) * (" + HoleSizeWL + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + brace.BraceConnect.Gusset.Thickness);
						Reporting.AddLine("= " + Anv + ConstUnit.Area);

						Agt = brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir * (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * brace.BraceConnect.Gusset.Thickness;
						Reporting.AddLine("Agt = st * (nt - 1) * t");
						Reporting.AddLine("= " + brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir + " * (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * " + brace.BraceConnect.Gusset.Thickness);
						Reporting.AddLine("= " + Agt + ConstUnit.Area);

						Ant = (brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir - (HoleSizeWT + ConstNum.SIXTEENTH_INCH)) * (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * brace.BraceConnect.Gusset.Thickness;
						Reporting.AddLine("Ant = (st - (dt + " + ConstNum.SIXTEENTH_INCH + ")) * (nt - 1) * t");
						Reporting.AddLine("= (" + brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir + " - (" + HoleSizeWT + " + " + ConstNum.SIXTEENTH_INCH + ")) * (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * " + brace.BraceConnect.Gusset.Thickness);
						Reporting.AddLine("= " + Ant + ConstUnit.Area);

						FiRn = ConstNum.FIOMEGA0_75N * (0.6 * Math.Min(brace.BraceConnect.Gusset.Material.Fy * Agv, brace.BraceConnect.Gusset.Material.Fu * Anv) + 1 * brace.BraceConnect.Gusset.Material.Fu * Ant);
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * (0.6 * Min(Fy * Agv, Fu * Anv) + Fu * Ant)");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * (0.6 * Min(" + brace.BraceConnect.Gusset.Material.Fy + "  * " + Agv + " , " + brace.BraceConnect.Gusset.Material.Fu + "  * " + Anv + " ) + " + 1 + " * " + brace.BraceConnect.Gusset.Material.Fu + "  * " + Ant + " )");

						if (FiRn >= brace.AxialTension)
							Reporting.AddLine("= " + FiRn + " >= " + brace.AxialTension + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + FiRn + " << " + brace.AxialTension + ConstUnit.Force + " (NG)");
					}
					else
						brace.BoltBrace.NumberOfBolts = SmallMethodsDesign.NBRequiredForGussetBlockShear(memberType, 1, brace.BoltBrace.NumberOfBolts);
					Reporting.AddHeader("Bolt Bearing:");
					SmallMethodsDesign.BraceBoltBearingOnGusset(memberType, ref gthickness);
				}
				printout = true;
				SmallMethodsDesign.GussetShear(memberType, ref t);

				Reporting.AddHeader("Check Whitmore Section:");
				if (brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Welded)
				{
					if (brace.ShapeType == EShapeType.SingleAngle || brace.ShapeType == EShapeType.DoubleAngle)
					{
						// L
						WhitmoreLw1 = brace.BraceConnect.Brace.BraceWeld.Weld1L * 1.1547F;
						WhitmoreLw2 = (brace.BraceConnect.Brace.BraceWeld.Weld1L + brace.BraceConnect.Brace.BraceWeld.Weld2L) * 0.57735 + brace.Shape.d;
						Reporting.AddLine("Width1 = 1.1547 * Lweld = 1.1547 * " + brace.BraceConnect.Brace.BraceWeld.Weld1L + " = " + WhitmoreLw1 + ConstUnit.Length);
						Reporting.AddLine("Width2 = 0.57735 * (" + brace.BraceConnect.Brace.BraceWeld.Weld1L + " + " + brace.BraceConnect.Brace.BraceWeld.Weld2L + ") + " + brace.Shape.d + " = " + WhitmoreLw2 + ConstUnit.Length);
						Reporting.AddLine("Width (Lw) = Max(Width1, Width2) = " + Math.Max(WhitmoreLw1, WhitmoreLw2) + ConstUnit.Length);
					}
					else if (brace.ShapeType == EShapeType.WTSection)
					{
						WhitmoreLw1 = 1.1547 * brace.BraceConnect.Brace.BraceWeld.Weld1L + brace.Shape.bf;
						Reporting.AddLine("Width (Lw) = 1.1547 * Lweld + d");
						Reporting.AddLine("= 1.1547 * " + brace.BraceConnect.Gusset.Thickness + " + " + brace.Shape.bf + " = " + WhitmoreLw1 + ConstUnit.Length);
					}
					else
					{
						WhitmoreLw1 = 1.1547 * brace.BraceConnect.Brace.BraceWeld.Weld1L + brace.Shape.d;
						Reporting.AddLine("Width (Lw) = 1.1547 * Lweld + d");
						Reporting.AddLine("= 1.1547 * " + brace.BraceConnect.Gusset.Thickness + " + " + brace.Shape.d + " = " + WhitmoreLw1 + ConstUnit.Length);
					}
				}
				else
				{
					if (brace.ShapeType == EShapeType.WTSection)
					{
						// WT
						g1 = brace.GageOnFlange;
						g2 = Math.Min(3, ((int)Math.Floor(4 * ((brace.Shape.bf - g1) / 2 - (brace.BoltBrace.MinEdgeRolled + brace.BoltBrace.Eincr)))) / 4);
						if (brace.BoltBrace.NumberOfRows == 2)
						{
							N = -((int)Math.Floor((double)-brace.BoltBrace.NumberOfBolts / brace.BoltBrace.NumberOfRows));
							if (brace.BraceConnect.Brace.BoltsAreStaggered && brace.BoltBrace.NumberOfBolts % 2 == 0)
							{
								// 2
								WhitmoreLw1 = 2 * g2 + g1 + (brace.BoltBrace.NumberOfBolts - N - 1) * brace.BoltBrace.SpacingLongDir * 1.1547F;
								Lw2 = (N - 0.5) * brace.BoltBrace.SpacingLongDir * 1.1547 + g1;
								Reporting.AddHeader("Width1 = 2 * g2 + g1 + (n1 - 1) * s * 1.1547");
								Reporting.AddLine("= 2 * " + g2 + " + " + g1 + " + (" + (brace.BoltBrace.NumberOfBolts - N) + "-1) * " + brace.BoltBrace.SpacingLongDir + " * 1.1547");
								Reporting.AddLine("=  " + WhitmoreLw1 + ConstUnit.Length);

								Reporting.AddHeader("Width2 = ((n - 0.5) * s) * 1.1547 + g1");
								Reporting.AddLine("= ((" + N + " - 0.5) * " + brace.BoltBrace.SpacingLongDir + ") * 1.1547 + " + g1);
								Reporting.AddLine("=  " + Lw2 + ConstUnit.Length);

								Reporting.AddLine("Width (Lw) = Max(Width1, Width2) = " + Math.Max(WhitmoreLw1, Lw2) + ConstUnit.Length);

							}
							else if (brace.BraceConnect.Brace.BoltsAreStaggered)
							{
								WhitmoreLw1 = 2 * g2 + g1 + (brace.BoltBrace.NumberOfBolts - N - 0.5) * brace.BoltBrace.SpacingLongDir * 1.1547F;
								Lw2 = (N - 1) * brace.BoltBrace.SpacingLongDir * 1.1547 + g1;
								Reporting.AddLine("Width1 = 2 * g2 + g1 + (n1 - 0.5) * s * 1.1547");
								Reporting.AddLine("= 2 * " + g2 + " + " + g1 + " + (" + (brace.BoltBrace.NumberOfBolts - N) + "-0.5) * " + brace.BoltBrace.SpacingLongDir + " * 1.1547");
								Reporting.AddLine("=  " + WhitmoreLw1 + ConstUnit.Length);

								Reporting.AddLine("Width2 = ((n - 1) * s) * 1.1547 + g1");
								Reporting.AddLine("= ((" + N + " - 1) * " + brace.BoltBrace.SpacingLongDir + ") * 1.1547 + " + g1);
								Reporting.AddLine("=  " + Lw2 + ConstUnit.Length);
								Reporting.AddLine("Width (Lw) = Max(Width1, Width2) = " + Math.Max(WhitmoreLw1, Lw2) + ConstUnit.Length);
							}
							else if (brace.BoltBrace.NumberOfBolts % 2 == 0)
							{
								WhitmoreLw1 = (N - 1) * brace.BoltBrace.SpacingLongDir * 1.1547 + 2 * g2 + g1;
								Lw2 = 0;
								Reporting.AddLine("Width (Lw) = (n - 1) * s * 1.1547 + 2 * g2 + g1");
								Reporting.AddLine("= (" + N + " - 1) * " + brace.BoltBrace.SpacingLongDir + " * 1.1547 + 2 * " + g2 + " + " + g1);
								Reporting.AddLine("= " + WhitmoreLw1 + ConstUnit.Length);
							}
							else
							{
								WhitmoreLw1 = 2 * g2 + g1 + (brace.BoltBrace.NumberOfBolts - N - 1) * brace.BoltBrace.SpacingLongDir * 1.1547F;
								Lw2 = (N - 1) * brace.BoltBrace.SpacingLongDir * 1.1547 + g1;
								Reporting.AddHeader("Width1 = (n - 1) * s * 1.1547 + 2 * g2 + g1");
								Reporting.AddLine("= (" + (brace.BoltBrace.NumberOfBolts - N) + " - 1) * " + brace.BoltBrace.SpacingLongDir + " * 1.1547 + 2 0 *" + g2 + " + " + g1);
								Reporting.AddLine("= " + WhitmoreLw1 + ConstUnit.Length);

								Reporting.AddHeader("Width2 = ((n - 1) * s) * 1.1547 + g1");
								Reporting.AddLine("= ((" + N + " - 1) * " + brace.BoltBrace.SpacingLongDir + ") * 1.1547 + " + g1);
								Reporting.AddLine("=  " + Lw2 + ConstUnit.Length);
								Reporting.AddLine("Width (Lw) = Max(Width1, Width2) = " + Math.Max(WhitmoreLw1, Lw2) + ConstUnit.Length);
							}
						}
						else
						{
							WhitmoreLw1 = (brace.BoltBrace.NumberOfBolts - 1) * brace.BoltBrace.SpacingLongDir * 1.1547F + brace.GageOnFlange;
							Reporting.AddHeader("Width (Lw) = (n - 1) * s * 1.1547 + g1");
							Reporting.AddLine("= (" + brace.BoltBrace.NumberOfBolts + " - 1) * " + brace.BoltBrace.SpacingLongDir + " * 1.1547 + " + g1);
							Reporting.AddLine("= " + WhitmoreLw1 + ConstUnit.Length);
						}
					}
					else if (brace.ShapeType == EShapeType.SingleAngle || brace.ShapeType == EShapeType.DoubleAngle)
					{
						g2 = CommonCalculations.AngleStandardGage(2, brace.Shape.d);
						N = Math.Ceiling((double)brace.BoltBrace.NumberOfBolts / brace.BoltBrace.NumberOfRows);
						if (brace.BoltBrace.NumberOfRows == 2)
						{
							if (brace.BraceConnect.Brace.BoltsAreStaggered && brace.BoltBrace.NumberOfBolts % 2 == 0)
							{
								WhitmoreLw1 = (N - 0.5) * brace.BoltBrace.SpacingLongDir * 0.57735 + g2 + (brace.BoltBrace.NumberOfBolts - N - 1) * brace.BoltBrace.SpacingLongDir * 0.57735;
								Lw2 = (N - 0.5) * brace.BoltBrace.SpacingLongDir * 1.1547;

								Reporting.AddHeader("Width1 = (n - 0.5) * s * 0.57735 + g2 + (n1 - 1) * sl * 0.57735");
								Reporting.AddLine("= (" + N + " - 0.5) * " + brace.BoltBrace.SpacingLongDir + " * 0.57735 + " + g2 + " + (" + (brace.BoltBrace.NumberOfBolts - N) + " - 1) * " + brace.BoltBrace.SpacingLongDir + " * 0.57735");
								Reporting.AddLine("=  " + WhitmoreLw1 + ConstUnit.Length);

								Reporting.AddLine("Width2 = (n - 0.5) * s * 1.1547");
								Reporting.AddLine("= (" + N + " - 0.5) * " + brace.BoltBrace.SpacingLongDir + " * 1.1547");
								Reporting.AddLine("=  " + Lw2 + ConstUnit.Length);
								Reporting.AddLine("Width (Lw) = Max(Width1, Width2) = " + Math.Max(WhitmoreLw1, Lw2) + ConstUnit.Length);

							}
							else if (brace.BraceConnect.Brace.BoltsAreStaggered)
							{
								WhitmoreLw1 = (N - 1) * brace.BoltBrace.SpacingLongDir * 0.57735 + g2 + (brace.BoltBrace.NumberOfBolts - N - 0.5) * brace.BoltBrace.SpacingLongDir * 0.57735;
								Lw2 = (N - 1) * brace.BoltBrace.SpacingLongDir * 1.1547;

								Reporting.AddHeader("Width1 = (n - 1)*s*0.57735 + g2 + (n1 - 0.5) * sl * 0.57735");
								Reporting.AddLine("= (" + N + " - 1) * " + brace.BoltBrace.SpacingLongDir + " * 0.57735 + " + g2 + " + (" + (brace.BoltBrace.NumberOfBolts - N) + " - 0.5) * " + brace.BoltBrace.SpacingLongDir + " * 0.57735");
								Reporting.AddLine("=  " + WhitmoreLw1 + ConstUnit.Length);

								Reporting.AddHeader("Width2 = (n - 1) * s * 1.1547");
								Reporting.AddLine("= (" + N + " - 1) * " + brace.BoltBrace.SpacingLongDir + " * 1.1547");
								Reporting.AddLine("= " + Lw2 + ConstUnit.Length);
								Reporting.AddLine("Width (Lw) = Max(Width1, Width2) = " + Math.Max(WhitmoreLw1, Lw2) + ConstUnit.Length);

							}
							else if (brace.BoltBrace.NumberOfBolts % 2 == 0)
							{
								WhitmoreLw1 = (N - 1) * brace.BoltBrace.SpacingLongDir * 0.57735 + g2 + (brace.BoltBrace.NumberOfBolts - N - 1) * brace.BoltBrace.SpacingLongDir * 0.57735;
								Lw2 = 0;
								Reporting.AddLine("Width (Lw) = (n - 1) * s * 0.57735 + g2 + (n1 - 1) * sl * 0.57735");
								Reporting.AddLine("= (" + N + " - 1) * " + brace.BoltBrace.SpacingLongDir + " * 0.57735 + " + g2 + " + (" + (brace.BoltBrace.NumberOfBolts - N) + " - 1) * " + brace.BoltBrace.SpacingLongDir + " * 0.57735");
								Reporting.AddLine("=" + WhitmoreLw1 + ConstUnit.Length);
							}
							else
							{
								WhitmoreLw1 = (N - 1) * brace.BoltBrace.SpacingLongDir * 0.57735 + g2 + (brace.BoltBrace.NumberOfBolts - N - 1) * brace.BoltBrace.SpacingLongDir * 0.57735;
								Lw2 = (N - 1) * brace.BoltBrace.SpacingLongDir * 1.1547;

								Reporting.AddHeader("Width1 = (n - 1) * s * 0.57735 + g2 + (n1 - 1) * sl * 0.57735");
								Reporting.AddLine("= (" + N + " - 1) * " + brace.BoltBrace.SpacingLongDir + " * 0.57735 + " + g2 + " + (" + (brace.BoltBrace.NumberOfBolts - N) + " - 1) * " + brace.BoltBrace.SpacingLongDir + " * 0.57735");
								Reporting.AddLine("=  " + WhitmoreLw1 + ConstUnit.Length);

								Reporting.AddHeader("Width2 = (n - 1) * s * 1.1547");
								Reporting.AddLine("= (" + N + " - 1) * " + brace.BoltBrace.SpacingLongDir + " * 1.1547");
								Reporting.AddLine("= " + Lw2 + ConstUnit.Length);
								Reporting.AddLine("Width (Lw) = Max(Width1, Width2) = " + Math.Max(WhitmoreLw1, Lw2) + ConstUnit.Length);
							}
						}
						else
						{
							WhitmoreLw1 = (brace.BoltBrace.NumberOfBolts - 1) * brace.BoltBrace.SpacingLongDir * 1.1547F;
							Reporting.AddHeader("Width (Lw) = (n - 1) * s * 1.1547");
							Reporting.AddLine("= (" + brace.BoltBrace.NumberOfBolts + " - 1) * " + brace.BoltBrace.SpacingLongDir + " * 1.1547");
							Reporting.AddLine("=  " + WhitmoreLw1 + ConstUnit.Length);
						}
					}
					else if (brace.ShapeType == EShapeType.SingleChannel || brace.ShapeType == EShapeType.DoubleChannel)
					{
						WhitmoreLw1 = (brace.BoltBrace.NumberOfBolts / brace.BoltBrace.NumberOfRows - 1) * brace.BoltBrace.SpacingLongDir * 1.1547F + (brace.BoltBrace.NumberOfRows - 1) * brace.BoltBrace.SpacingTransvDir;
						Reporting.AddHeader("Width (Lw) = (nl - 1) * s * 1.1547 + (nt - 1) * st");
						Reporting.AddLine("= (" + (brace.BoltBrace.NumberOfBolts / brace.BoltBrace.NumberOfRows) + " - 1) * " + brace.BoltBrace.SpacingLongDir + " * 1.1547 + (" + brace.BoltBrace.NumberOfRows + " - 1) * " + brace.BoltBrace.SpacingTransvDir);
						Reporting.AddLine("=  " + WhitmoreLw1 + ConstUnit.Length);
					}
				}
				if (WhitmoreOut > 0)
					Reporting.AddLine("Lwo = " + WhitmoreOut + ConstUnit.Length + " of Lw is outside the gusset free edge.");
				if (WhitmoreC > 0)
					Reporting.AddLine("Lwc = " + WhitmoreC + ConstUnit.Length + " of Lw is in the column.");
				if (WhitmoreB > 0)
					Reporting.AddLine("Lwb = " + WhitmoreB + ConstUnit.Length + " of Lw is in the " + beamStr + ".");

				Reporting.AddHeader("Width of Whitmore Section inside gusset boundaries, Lwg = " + WhitmoreLw + ConstUnit.Length);
				Reporting.AddHeader("Whitmore Section Stress:");
				Reporting.AddHeader("Tension: ");
				if (column.WebOrientation == EWebOrientation.InPlane)
				{
					WhitmSectStressTen = brace.AxialTension / (WhitmoreLw * brace.BraceConnect.Gusset.Thickness + WhitmoreB * beam.Shape.tw + WhitmoreC * column.Shape.tw);
					Reporting.AddLine("fa = Fx / (Lwg * t + Lwb * twb + Lwc * twc) ");
					Reporting.AddLine("= " + brace.AxialTension + "/(" + WhitmoreLw + " * " + brace.BraceConnect.Gusset.Thickness + " + " + WhitmoreB + " * " + beam.Shape.tw + " + " + WhitmoreC + " * " + column.Shape.tw + ") ");
					Reporting.AddLine("= " + WhitmSectStressTen + ConstUnit.Stress);
				}
				else
				{
					WhitmSectStressTen = brace.AxialTension / (WhitmoreLw * brace.BraceConnect.Gusset.Thickness + WhitmoreB * beam.Shape.tw);
					Reporting.AddLine("fa = Fx / (Lwg * t + Lwb * twb) ");
					Reporting.AddLine("= " + brace.AxialTension + "/(" + WhitmoreLw + " * " + brace.BraceConnect.Gusset.Thickness + " + " + WhitmoreB + " * " + beam.Shape.tw + ") ");
					Reporting.AddLine("= " + WhitmSectStressTen + ConstUnit.Stress);
				}
				Reporting.AddHeader("Compression: ");
				if (column.WebOrientation == EWebOrientation.InPlane)
				{
					WhitmSectStressComp = brace.AxialCompression / (WhitmoreLw * brace.BraceConnect.Gusset.Thickness + WhitmoreB * beam.Shape.tw + WhitmoreC * column.Shape.tw);
					Reporting.AddLine("fa = Fx / (Lwg * t + Lwb * twb + Lwc * twc) ");
					Reporting.AddLine("= " + brace.AxialCompression + "/(" + WhitmoreLw + " * " + brace.BraceConnect.Gusset.Thickness + " + " + WhitmoreB + " * " + beam.Shape.tw + " + " + WhitmoreC + " * " + column.Shape.tw + ") ");
					Reporting.AddLine("= " + WhitmSectStressComp + ConstUnit.Stress);
				}
				else
				{
					WhitmSectStressComp = brace.AxialCompression / (WhitmoreLw * brace.BraceConnect.Gusset.Thickness + WhitmoreB * beam.Shape.tw);
					Reporting.AddLine("fa = Fx / (Lwg * t + Lwb * twb) ");
					Reporting.AddLine("= " + brace.AxialCompression + " / (" + WhitmoreLw + " * " + brace.BraceConnect.Gusset.Thickness + " + " + WhitmoreB + " * " + beam.Shape.tw + ") ");
					Reporting.AddLine("= " + WhitmSectStressComp + ConstUnit.Stress);
				}

				Reporting.AddHeader("Whitmore Section Yielding:");
				if (column.WebOrientation == EWebOrientation.InPlane)
				{
					WhitmoreYieldCap = ConstNum.FIOMEGA0_9N * (WhitmoreLw * brace.BraceConnect.Gusset.Thickness * brace.BraceConnect.Gusset.Material.Fy + WhitmoreB * beam.Shape.tw * beam.Material.Fy + WhitmoreC * column.Shape.tw * column.Material.Fy);
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + ConstString.FIOMEGA0_9 + " * (Lwg * t * Fyg + Lwb * twb * Fyb + Lwc * twc * Fyc) ");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * (" + WhitmoreLw + " * " + brace.BraceConnect.Gusset.Thickness + " * " + brace.BraceConnect.Gusset.Material.Fy + " + " + WhitmoreB + " * " + beam.Shape.tw + " * " + beam.Material.Fy + " + " + WhitmoreC + " * " + column.Shape.tw + " * " + column.Material.Fy + ") ");
				}
				else
				{
					WhitmoreYieldCap = ConstNum.FIOMEGA0_9N * (WhitmoreLw * brace.BraceConnect.Gusset.Thickness * brace.BraceConnect.Gusset.Material.Fy + WhitmoreB * beam.Material.Fy * beam.Shape.tw);
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * (Lwg * t * Fyg + Lwb * twb * Fyb) ");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * (" + WhitmoreLw + " * " + brace.BraceConnect.Gusset.Thickness + " * " + brace.BraceConnect.Gusset.Material.Fy + " + " + WhitmoreB + " * " + beam.Shape.tw + " * " + beam.Material.Fy + ") ");
				}

				if (brace.BraceConnect.Brace.MaxForce <= WhitmoreYieldCap)
					Reporting.AddLine("= " + WhitmoreYieldCap + " >= " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + WhitmoreYieldCap + " << " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

				if (brace.AxialCompression > 0)
				{
					Reporting.AddHeader("Buckling Check:");
					KLoR = 3.464 * Lcr / brace.BraceConnect.Gusset.Thickness;

					if ((brace.KBrace | brace.KneeBrace) || brace.WinConnect.ShearClipAngle.Position == EPosition.NoConnection || CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam || brace.BraceConnect.Gusset.DontConnectBeam)
						Reporting.AddLine("Effective Length of Whitmore Section (K = 1.2), Lcr = " + Lcr + ConstUnit.Length);
					else if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic)
						Reporting.AddHeader("Effective Length of Whitmore Section (K = 1.2), Lcr = " + Lcr + ConstUnit.Length);
					else
						Reporting.AddHeader("Effective Length of Whitmore Section (K = 0.5), Lcr = " + Lcr + ConstUnit.Length);

					Reporting.AddLine("Kl / r = Lcr / (t / 12^0.5) = " + Lcr + "/(" + brace.BraceConnect.Gusset.Thickness + " / 3.464)");
					Reporting.AddLine("= " + KLoR);
					BucklingCap = ConstNum.FIOMEGA0_9N * CommonCalculations.BucklingStress(KLoR, brace.BraceConnect.Gusset.Material.Fy, true);

					if (BucklingCap >= WhitmSectStressComp)
						Reporting.AddLine("Buckling Strength = " + ConstString.FIOMEGA0_9 + " * Fcr = " + BucklingCap + " >= " + WhitmSectStressComp + " ksi (OK)");
					else
						Reporting.AddLine("Buckling Strength = " + ConstString.FIOMEGA0_9 + "* Fcr = " + BucklingCap + " << " + WhitmSectStressComp + " ksi (NG)");
				}

				if (brace.ShapeType == EShapeType.HollowSteelSection && brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
					SmallMethodsDesign.WhitmoreCheckWithMoment(memberType, Lcr, WhitmoreLw, WhitmoreB, WhitmoreC);

				if (brace.KBrace && (memberType == EMemberType.LowerLeft || memberType == EMemberType.LowerRight))
				{
					t = brace.BraceConnect.Gusset.Thickness;
					printout = true;
					KBraceAdditionalChecks.CalcKBraceAdditionalChecks(memberType, ref t, printout);
				}
				else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam && (memberType == EMemberType.LowerLeft || memberType == EMemberType.UpperLeft))
				{
					t = brace.BraceConnect.Gusset.Thickness;
					printout = true;
					KBraceAdditionalChecks.CalcKBraceAdditionalChecks(memberType, ref t, printout);
				}

				Reporting.AddLine("Gusset Plate: Thickness = " + brace.BraceConnect.Gusset.Thickness);
				Reporting.AddLine("Whitmore Section Yield Capacity:");
				if (brace.FxP <= WhitmoreYieldCap)
					Reporting.AddLine("= " + WhitmoreYieldCap + " >= " + brace.FxP + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + WhitmoreYieldCap + " << " + brace.FxP + ConstUnit.Force + " (NG)");
				if (brace.AxialTension < 0)
				{
					Reporting.AddLine("  Whitmore Section Buckling Capacity:");
					if (WhitmSectStress <= BucklingCap)
						Reporting.AddLine("= " + BucklingCap + " >= " + WhitmSectStress + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + BucklingCap + " << " + WhitmSectStress + ConstUnit.Force + " (NG)");
				}

				if (!brace.BraceConnect.ClawAngles.DoNotConnectFlanges)
				{
					HoleSizeCL = brace.BraceConnect.Gusset.HoleLongC; // Bolts(ic).d +0.0625
					HoleSizeCT = brace.BraceConnect.Gusset.HoleTransC; // Bolts(ic).d +0.0625
				}
				else
				{
					HoleSizeCL = 0;
					HoleSizeCT = 0;
				}
				if (!brace.BraceConnect.SplicePlates.DoNotConnectWeb)
				{
					HoleSizeWL = brace.BraceConnect.Gusset.HoleLongP; // Bolts(iw).d +0.0625
					HoleSizeWT = brace.BraceConnect.Gusset.HoleTransP; // Bolts(iw).d +0.0625
				}
				else
				{
					HoleSizeWL = 0;
					HoleSizeWT = 0;
				}

				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
					t = Math.Max(brace.BraceConnect.Gusset.Thickness, otherSide.BraceConnect.Gusset.Thickness);
				else if (brace.KBrace)
					t = Math.Max(brace.BraceConnect.Gusset.Thickness, sameSide.BraceConnect.Gusset.Thickness);
				else
					t = ConstNum.THREE_EIGHTS_INCH;

				Reporting.AddHeader("Compute " + CommonDataStatic.CommonLists.CompleteMemberList[memberType] + " Gusset Thickness");

				if (!brace.BraceConnect.ClawAngles.DoNotConnectFlanges)
					Reporting.AddLine("Claw Angle Bolts:");

				if (!brace.BraceConnect.ClawAngles.DoNotConnectFlanges)
				{
					FpeClaw = CommonCalculations.EdgeBearing(brace.BoltGusset.EdgeDistGusset, brace.BraceConnect.Gusset.HoleLongC, brace.BoltGusset.BoltSize, brace.BraceConnect.Gusset.Material.Fu, brace.BoltGusset.HoleType, false);
					FpsClaw = CommonCalculations.SpacingBearing(brace.BoltGusset.SpacingLongDir, brace.BraceConnect.Gusset.HoleLongC, brace.BoltGusset.BoltSize, brace.BoltGusset.HoleType, brace.BraceConnect.Gusset.Material.Fu, false);
				}
				if (!brace.BraceConnect.SplicePlates.DoNotConnectWeb)
					Reporting.AddLine("Splice Plate Bolts:");
				if (!brace.BraceConnect.SplicePlates.DoNotConnectWeb)
				{
					FpeWeb = CommonCalculations.EdgeBearing(brace.BraceConnect.SplicePlates.Bolt.EdgeDistGusset, brace.BraceConnect.Gusset.HoleLongP, brace.BraceConnect.SplicePlates.Bolt.BoltSize, brace.BraceConnect.Gusset.Material.Fu, brace.BraceConnect.SplicePlates.Bolt.HoleType, false);
					FpsWeb = CommonCalculations.SpacingBearing(brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir, brace.BraceConnect.Gusset.HoleLongP, brace.BraceConnect.SplicePlates.Bolt.BoltSize, brace.BraceConnect.SplicePlates.Bolt.HoleType, brace.BraceConnect.Gusset.Material.Fu, false);
				}
				if (!brace.BraceConnect.ClawAngles.DoNotConnectFlanges)
				{
					fsz = brace.BraceConnect.ClawAngles.Fillet;
					if (fsz < ConstNum.HALF_INCH)
					{
						fsz = ConstNum.HALF_INCH;
					}
					anglegage = CommonCalculations.AngleStandardGage(0, (brace.BraceConnect.ClawAngles.LongLeg + brace.BraceConnect.ClawAngles.ShortLeg - brace.BraceConnect.ClawAngles.LengthOfOSL)); // Bolts(Ic).d + fsz '+ ClawAngle(m).Thickness
					brace.BoltGusset.EdgeDistTransvDir = brace.BraceConnect.ClawAngles.ShortLeg + brace.BraceConnect.ClawAngles.LongLeg - brace.BraceConnect.ClawAngles.LengthOfOSL - anglegage;
					brace.BraceConnect.SplicePlates.Bolt.EdgeDistTransvDir = brace.BoltGusset.EdgeDistTransvDir;
					LgvC = 2 * (brace.BoltGusset.SpacingLongDir * (brace.BoltGusset.NumberOfBolts - 1) + brace.BoltGusset.EdgeDistGusset);
					LnvC = LgvC - 2 * (brace.BoltGusset.NumberOfBolts - 0.5) * (HoleSizeCL + ConstNum.SIXTEENTH_INCH);
					LntC = brace.Shape.d + 2 * anglegage - (HoleSizeCT + ConstNum.SIXTEENTH_INCH);

					FiRnPerThC = ConstNum.FIOMEGA0_75N * (0.6 * Math.Min(brace.BraceConnect.Gusset.Material.Fy * LgvC, brace.BraceConnect.Gusset.Material.Fu * LnvC) + 1 * brace.BraceConnect.Gusset.Material.Fu * LntC);

				}
				if (!brace.BraceConnect.SplicePlates.DoNotConnectWeb)
				{
					LgvW = 2 * (brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir * (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) + brace.BraceConnect.SplicePlates.Bolt.EdgeDistGusset);
					LgtW = (brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir * (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1));
					LnvW = 2 * (brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir * (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) + brace.BraceConnect.SplicePlates.Bolt.EdgeDistGusset - (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) * (HoleSizeWL + ConstNum.SIXTEENTH_INCH));
					LntW = ((brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir - (HoleSizeWT + ConstNum.SIXTEENTH_INCH)) * (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1));
					FiRnPerThW = ConstNum.FIOMEGA0_75N * (0.6 * Math.Min(brace.BraceConnect.Gusset.Material.Fy * LgvW, brace.BraceConnect.Gusset.Material.Fu * LnvW) + 1 * brace.BraceConnect.Gusset.Material.Fu * LntW);
				}

				if (!brace.BraceConnect.ClawAngles.DoNotConnectFlanges)
					WhitmoreLw1 = (brace.BoltGusset.NumberOfBolts - 1) * brace.BoltGusset.SpacingLongDir * 1.1547F + brace.Shape.d + 2 * anglegage;
				else if (!(brace.ShapeType == EShapeType.HollowSteelSection && brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted))
					WhitmoreLw1 = (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) * brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir * 1.1547F + brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir * brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1;

				//TODO: Do we need this?

				//i1 = 25 + (m - 3) * 5;
				//slpm = Math.Tan(brace.Angle * ConstNum.RADIAN); // slop of member
				//slpm1 = -1 / Math.Tan(brace.Angle * ConstNum.RADIAN); //  slope of whitmore section line (Line W)
				//SignOfTangent = Math.Sign(Math.Tan(brace.Angle * ConstNum.RADIAN));
				//slpc = Math.Tan((brace.Angle - SignOfTangent * 30) * ConstNum.RADIAN); //  slop of 30o line on column side (Line C)
				//slpb = Math.Tan((brace.Angle + SignOfTangent * 30) * ConstNum.RADIAN); //  slop of 30o line on beam side (Line B)

				//if (BRACE1.x[i1 + 2] == BRACE1.x[i1 + 1])
				//	slpVFree = 100;
				//else
				//	slpVFree = (BRACE1.y[i1 + 2] - BRACE1.y[i1 + 1]) / (BRACE1.x[i1 + 2] - BRACE1.x[i1 + 1]);
				//if (BRACE1.y[i1 + 4] == BRACE1.y[i1 + 3])
				//	slpHFree = 0;
				//else if (BRACE1.x[i1 + 4] == BRACE1.x[i1 + 3])
				//	slpHFree = 100;
				//else
				//	slpHFree = (BRACE1.y[i1 + 4] - BRACE1.y[i1 + 3]) / (BRACE1.x[i1 + 4] - BRACE1.x[i1 + 3]);

				//if (!brace.BraceConnect.ClawAngles.DoNotConnectFlanges)
				//{
				//	 i2 = 9 + (m - 3) * 6;               Indicates ClawAngles.Bolts
				//	SmallMethodsBrace.Intersect(slpm1, BRACE1.BoltX[i2, 1], BRACE1.BoltY[i2, 1], slpHFree, BRACE1.x[i1 + 3], BRACE1.y[i1 + 3], ref xHFree, ref yHFree); //  Intersection of Line W and Horizontal free edge (Point HW)
				//	SmallMethodsBrace.Intersect(slpm1, BRACE1.BoltX[i2, 1], BRACE1.BoltY[i2, 1], slpVFree, BRACE1.x[i1 + 2], BRACE1.y[i1 + 2], ref xVFree, ref yVFree); //  Intersection of Line W and Vertical free edge (Point VW)
				//	SmallMethodsBrace.Intersect(slpm1, BRACE1.BoltX[i2, 1], BRACE1.BoltY[i2, 1], 100, BRACE1.x[i1], BRACE1.y[i1], ref xColumn, ref yColumn); //  Intersection of Whitmore Line and Column Face
				//	j11 = 1;
				//}
				//else if (!(brace.ShapeType == EShapeType.HollowSteelSection && brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted))
				//{
				//	 i2 = 7 + (m - 3) * 6;                Indicates SplicePlates.Bolts
				//	SmallMethodsBrace.Intersect(slpm1, BRACE1.BoltX[i2, brace.BraceConnect.SplicePlates.Bolts.NumberOfBolts], BRACE1.BoltY[i2, brace.BraceConnect.SplicePlates.Bolts.NumberOfBolts], slpHFree, BRACE1.x[i1 + 3], BRACE1.y[i1 + 3], ref xHFree, ref yHFree); //  Intersection of Line W and Horizontal free edge (Point HW)
				//	SmallMethodsBrace.Intersect(slpm1, BRACE1.BoltX[i2, brace.BraceConnect.SplicePlates.Bolts.NumberOfBolts], BRACE1.BoltY[i2, brace.BraceConnect.SplicePlates.Bolts.NumberOfBolts], slpVFree, BRACE1.x[i1 + 2], BRACE1.y[i1 + 2], ref xVFree, ref yVFree); //  Intersection of Line W and Vertical free edge (Point VW)
				//	SmallMethodsBrace.Intersect(slpm1, BRACE1.BoltX[i2, brace.BraceConnect.SplicePlates.Bolts.NumberOfBolts], BRACE1.BoltY[i2, brace.BraceConnect.SplicePlates.Bolts.NumberOfBolts], 100, BRACE1.x[i1], BRACE1.y[i1], ref xColumn, ref yColumn); //  Intersection of Whitmore Line and Column Face
				//	j11 = brace.BraceConnect.SplicePlates.Bolts.NumberOfBolts;
				//}

				//if ((brace.KneeBrace | brace.KBrace) || brace.WinConnect.ShearClipAngle.position == EPosition.NoConnection && BRACE1.JointType != 1)
				//{
				//	SmallMethodsBrace.Intersect(100, BRACE1.x[i1], BRACE1.y[i1], slpm, brace.WorkPointX, brace.WorkPointY, ref xc, ref Yc); //  Intersection of Brace CL and Column Face
				//	L1 = SmallMethodsBrace.PointToLineDistance(((int) slpm1), BRACE1.BoltX[i2, j11], BRACE1.BoltY[i2, j11], xc, Yc); //  Dist. from Edge of guset to line W along Brace CL
				//	if (brace.WinConnect.ShearClipAngle.position == EPosition.NoConnection)
				//	{
				//		SmallMethodsBrace.Intersect(0, BRACE1.x[i1], BRACE1.y[i1], slpm, brace.WorkPointX, brace.WorkPointY, ref xe, ref ye); //  Intersection of Brace CL and gusset horiz. edge
				//		L11 = SmallMethodsBrace.PointToLineDistance(((int) slpm1), BRACE1.BoltX[i2, j11], BRACE1.BoltY[i2, j11], xe, ye); //  Dist. from Edge of guset to line W along Brace CL
				//		L1 = Math.Min(L1, L11);
				//	}
				//}
				//else if (CommonDataStatic.JointConfigurationVisual == EJointConfiguration.BraceVToBeam)
				//{
				//	SmallMethodsBrace.Intersect(0, BRACE1.x[i1], BRACE1.y[i1], slpm, brace.WorkPointX, brace.WorkPointY, ref xc, ref Yc); //  Intersection of Brace CL and Beam Face
				//	L1 = SmallMethodsBrace.PointToLineDistance(((int) slpm1), BRACE1.BoltX[i2, j11], BRACE1.BoltY[i2, j11], xc, Yc); //  Dist. from Edge of guset to line W along Brace CL
				//}
				//else
				//	L1 = SmallMethodsBrace.PointToLineDistance(((int) slpm1), BRACE1.BoltX[i2, j11], BRACE1.BoltY[i2, j11], BRACE1.x[i1], BRACE1.y[i1]); //  Dist. from corner of guset to line W
				//if (!brace.BraceConnect.ClawAngles.DoNotConnectFlanges)
				//{
				//	 If m = 6 Then Stop
				//	SmallMethodsBrace.Intersect(slpm1, BRACE1.BoltX[i2, 1], BRACE1.BoltY[i2, 1], slpc, BRACE1.BoltX[i2 + 2, BRACE1.Bolts[i2 + 2].N], BRACE1.BoltY[i2 + 2, BRACE1.Bolts[i2 + 2].N], ref x0, ref y0); // Intersection of Lines W and C (Point 0)
				//	BRACE1.x[433 + 4 * (m - 3)] = BRACE1.BoltX[i2 + 2, BRACE1.Bolts[i2 + 2].N];
				//	BRACE1.y[433 + 4 * (m - 3)] = BRACE1.BoltY[i2 + 2, BRACE1.Bolts[i2 + 2].N];
				//	if (Math.Abs(yHFree) < Math.Abs(y0))
				//	{
				//		y0 = yHFree;
				//		x0 = xHFree;
				//	}
				//	SmallMethodsBrace.Intersect(100, BRACE1.x[i1], BRACE1.y[i1], slpm, x0, y0, ref x1, ref y1); // Intersection of Column face and Line // to member through Point 0
				//	L2 = Math.Sqrt(Math.Pow(x0 - x1, 2) + Math.Pow(y0 - y1, 2));

				//	TODO: there's no way to know what i2 is supposed to be here. Could be (m - 3)*6 + 7 or (m - 3)*6 + 9. -RM
				//	SmallMethodsBrace.Intersect(slpm1, BRACE1.BoltX[i2, 1], BRACE1.BoltY[i2, 1], slpb, BRACE1.BoltX[i2, BRACE1.Bolts[i2].N], BRACE1.BoltY[i2, BRACE1.Bolts[i2].N], ref x2, ref y2); // Intersection of Lines W and B (Point 2)
				//	BRACE1.x[431 + 4 * (m - 3)] = BRACE1.BoltX[i2, BRACE1.Bolts[i2].N];
				//	BRACE1.y[431 + 4 * (m - 3)] = BRACE1.BoltY[i2, BRACE1.Bolts[i2].N];
				//	if (Math.Abs(xVFree) < Math.Abs(x2))
				//	{
				//		y2 = yVFree;
				//		x2 = xVFree;
				//	}
				//	if (brace.KBrace | brace.KneeBrace)
				//	{
				//		SmallMethodsBrace.Intersect(100, BRACE1.x[i1], BRACE1.y[i1], slpm, x2, y2, ref x3, ref y3); // Intersection of Column Face and Line // to member through Point 2 (point 3)
				//	}
				//	else if (CommonDataStatic.JointConfigurationVisual == EJointConfiguration.BraceVToBeam)
				//	{
				//		SmallMethodsBrace.Intersect(0, BRACE1.x[i1], BRACE1.y[i1], slpm, x2, y2, ref x3, ref y3); // Intersection of Column Face and Line // to member through Point 2 (point 3)
				//	}
				//	else
				//	{
				//		SmallMethodsBrace.Intersect(0, BRACE1.x[i1], BRACE1.y[i1], slpm, x2, y2, ref x3, ref y3); // Intersection of Beam Flange and Line // to member through Point 2 (point 3)
				//	}
				//	L3 = Math.Sqrt(Math.Pow(x2 - x3, 2) + Math.Pow(y2 - y3, 2));
				//}
				//else
				//{
				//	SmallMethodsBrace.Intersect(slpm1, BRACE1.BoltX[i2, BRACE1.Bolts[i2].N], BRACE1.BoltY[i2, BRACE1.Bolts[i2].N], slpc, BRACE1.BoltX[i2, BRACE1.Bolts[i2].nw], BRACE1.BoltY[i2, BRACE1.Bolts[i2].nw], ref x0, ref y0); // Intersection of Lines W and C (Point 0)
				//	BRACE1.x[433 + 4 * (m - 3)] = BRACE1.BoltX[i2, BRACE1.Bolts[i2].nw];
				//	BRACE1.y[433 + 4 * (m - 3)] = BRACE1.BoltY[i2, BRACE1.Bolts[i2].nw];
				//	if (Math.Abs(yHFree) < Math.Abs(y0))
				//	{
				//		y0 = yHFree;
				//		x0 = xHFree;
				//	}
				//	SmallMethodsBrace.Intersect(100, BRACE1.x[i1], BRACE1.y[i1], slpm, x0, y0, ref x1, ref y1); // Intersection of Column face and Line // to member through Point 0
				//	L2 = Math.Sqrt(Math.Pow(x0 - x1, 2) + Math.Pow(y0 - y1, 2));
				//	SmallMethodsBrace.Intersect(slpm1, BRACE1.BoltX[i2, BRACE1.Bolts[i2].N], BRACE1.BoltY[i2, BRACE1.Bolts[i2].N], slpb, BRACE1.BoltX[i2, 1], BRACE1.BoltY[i2, 1], ref x2, ref y2); // Intersection of Lines W and B (Point 2)
				//	BRACE1.x[431 + 4 * (m - 3)] = BRACE1.BoltX[i2, 1];
				//	BRACE1.y[431 + 4 * (m - 3)] = BRACE1.BoltY[i2, 1];
				//	if (Math.Abs(xVFree) < Math.Abs(x2))
				//	{
				//		y2 = yVFree;
				//		x2 = xVFree;
				//	}
				//	if (brace.KBrace | brace.KneeBrace)
				//	{
				//		SmallMethodsBrace.Intersect(100, BRACE1.x[i1], BRACE1.y[i1], slpm, x2, y2, ref x3, ref y3); // Intersection of Column Face and Line // to member through Point 2
				//	}
				//	else if (CommonDataStatic.JointConfigurationVisual == EJointConfiguration.BraceVToBeam)
				//	{
				//		SmallMethodsBrace.Intersect(0, BRACE1.x[i1], BRACE1.y[i1], slpm, x2, y2, ref x3, ref y3); // Intersection of Column Face and Line // to member through Point 2
				//	}
				//	else
				//	{
				//		SmallMethodsBrace.Intersect(0, BRACE1.x[i1], BRACE1.y[i1], slpm, x2, y2, ref x3, ref y3); // Intersection of Beam Flange and Line // to member through Point 2
				//	}
				//	L3 = Math.Sqrt(Math.Pow(x2 - x3, 2) + Math.Pow(y2 - y3, 2));
				//}
				// If m = 6 Then Stop
				//NewLw1 = Math.Sqrt(Math.Pow(x2 - x0, 2) + Math.Pow(y2 - y0, 2));
				//if (NewLw1 == 0.0)
				//{
				//	NewLw1 = WhitmoreLw1;
				//}
				//WhitmoreOut = -NewLw1 + WhitmoreLw1;
				//BRACE1.x[400 + 6 * (m - 3)] = x0;
				//BRACE1.y[400 + 6 * (m - 3)] = y0;
				//BRACE1.x[401 + 6 * (m - 3)] = x1;
				//BRACE1.y[401 + 6 * (m - 3)] = y1;
				//BRACE1.x[402 + 6 * (m - 3)] = x2;
				//BRACE1.y[402 + 6 * (m - 3)] = y2;
				//BRACE1.x[403 + 6 * (m - 3)] = x3;
				//BRACE1.y[403 + 6 * (m - 3)] = y3;
				//BRACE1.x[404 + 6 * (m - 3)] = xHFree;
				//BRACE1.y[404 + 6 * (m - 3)] = yHFree;
				//BRACE1.x[405 + 6 * (m - 3)] = xVFree;
				//BRACE1.y[405 + 6 * (m - 3)] = yVFree;
				//BRACE1.x[432 + 4 * (m - 3)] = x2;
				//BRACE1.y[432 + 4 * (m - 3)] = y2;
				//BRACE1.x[434 + 4 * (m - 3)] = x0;
				//BRACE1.y[434 + 4 * (m - 3)] = y0;

				//switch (memberType)
				//{
				//	case EMemberType.UpperRight:
				//		L2 = L2 * Math.Sign((x0 - BRACE1.x[i1]));
				//		if (!(brace.KBrace | brace.KneeBrace))
				//		{
				//			L3 = L3 * Math.Sign((y2 - BRACE1.y[i1]));
				//		}
				//		break;
				//	case EMemberType.LowerRight:
				//		L2 = L2 * Math.Sign((x0 - BRACE1.x[i1]));
				//		if (!(brace.KBrace | brace.KneeBrace))
				//		{
				//			L3 = L3 * Math.Sign((-y2 + BRACE1.y[i1]));
				//		}
				//		break;
				//	case EMemberType.UpperLeft:
				//		L2 = L2 * Math.Sign((-x0 + BRACE1.x[i1]));
				//		if (!(brace.KBrace | brace.KneeBrace))
				//		{
				//			L3 = L3 * Math.Sign((y2 - BRACE1.y[i1]));
				//		}
				//		break;
				//	case EMemberType.LowerLeft:
				//		L2 = L2 * Math.Sign((-x0 + BRACE1.x[i1]));
				//		if (!(brace.KBrace | brace.KneeBrace))
				//		{
				//			L3 = L3 * Math.Sign((-y2 + BRACE1.y[i1]));
				//		}
				//		break;
				//}
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase)
					L3 = 0;
				if (column.ShapeType == EShapeType.WideFlange && column.WebOrientation == EWebOrientation.OutOfPlane)
					L2 = 0;
				if (brace.KBrace | brace.KneeBrace)
					WhitmoreB = 0;
				else
					WhitmoreB = -L3 * Math.Abs(slpm);
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
					WhitmoreC = 0;
				else if (L2 != 0)
					WhitmoreC = -L2 / Math.Abs(slpm);

				if (WhitmoreC < 0)
					WhitmoreC = 0;
				if (WhitmoreB < 0)
					WhitmoreB = 0;
				if (WhitmoreOut < 0)
					WhitmoreOut = 0;

				// If m = 6 Then Stop
				WhitmoreLw = (Math.Max(0, WhitmoreLw1 - WhitmoreB - WhitmoreC - WhitmoreOut));
				if (L2 < 0)
					L2 = 0;
				if (L3 < 0)
					L3 = 0;
				if ((brace.KBrace | brace.KneeBrace) || brace.WinConnect.ShearClipAngle.Position == EPosition.NoConnection || CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam || brace.BraceConnect.Gusset.DontConnectBeam)
					Lcr = 1.2 * L1;
				else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
					Lcr = 1.2 * L1;
				else if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic)
					Lcr = 1.2 * (L1 + L2 + L3) / 3;
				else
					Lcr = 0.5 * (L1 + L2 + L3) / 3;

				if (Lcr < 0)
					Lcr = 0;

				if (brace.BraceConnect.Gusset.Thickness == 0.0)
				{
					t0 = NumberFun.Round(Math.Max(1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * brace.WinConnect.ShearClipAngle.WeldSize / brace.BraceConnect.Gusset.Material.Fu, ConstNum.QUARTER_INCH), 16);
					if (t < t0)
						t = t0;
					printout = false;
					SmallMethodsDesign.GussetShear(memberType, ref t);
				}

				//Mb = ((int)Math.Floor(m / 4.5)) + 1;                                  this is also beam
				if (beam.IsActive)
				{
					SmallMethodsDesign.GussetThickness_ForceEnvelop(memberType, ref BeamHfrc, ref BeamVfrc, ref BeamWMom);

					if (brace.GussetToBeamConnection == EBraceConnectionTypes.DirectlyWelded)
					{
						Gap = Math.Abs(beam.EndSetback + beam.WinConnect.Beam.TCopeL - brace.BraceConnect.Gusset.ColumnSideSetback);
						weldlength = brace.BraceConnect.Gusset.Hb - Gap;

						fh = Math.Abs(BeamHfrc / weldlength);
						fvv = Math.Abs(BeamVfrc / weldlength);
						Fb = Math.Abs(6 * BeamWMom / Math.Pow(weldlength, 2));

						fr = Math.Sqrt(Math.Pow(fh, 2) + Math.Pow(fvv + Fb, 2));
						w1 = 1.25 * fr / (ConstNum.FIOMEGA0_75N * 0.6 * Math.Sqrt(2) * CommonDataStatic.Preferences.DefaultElectrode.Fexx);

						SmallMethodsDesign.GussetWelds(memberType, ref t, BeamHfrc, BeamVfrc, ref wmaxlimit, ref w1);
						if (w1 > wmaxlimit)
							t = t * w1 / wmaxlimit;
						beamweldok = brace.BraceConnect.Gusset.BeamWeldSize >= w1;
					}
					else
						beamweldok = true;
				}
				else
					beamweldok = true;

				t -= ConstNum.SIXTEENTH_INCH;

				while (t <= ConstNum.FOUR_INCHES)
				{
					if (brace.BraceConnect.Gusset.Thickness == 0.0)
						t += ConstNum.SIXTEENTH_INCH;
					else
						t = brace.BraceConnect.Gusset.Thickness;

					if (!brace.BraceConnect.ClawAngles.DoNotConnectFlanges)
					{
						BearingCCapTen = 2 * (FpeClaw + FpsClaw * (brace.BoltGusset.NumberOfBolts - 1)) * t;
						BearingCCapComp = 2 * (FpsClaw * brace.BoltGusset.NumberOfBolts) * t;
						TearOutCCap = FiRnPerThC * t;
					}
					else
						BearingCCapTen = 0;

					if (!brace.BraceConnect.SplicePlates.DoNotConnectWeb)
					{
						BearingWCapTen = brace.BraceConnect.SplicePlates.Bolt.NumberOfLines * FpeWeb * t + (brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts - brace.BraceConnect.SplicePlates.Bolt.NumberOfLines) * FpsWeb * t;
						BearingWCapComp = brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts * FpsWeb * t;
						TearOutWCap = FiRnPerThW * t;
					}

					if (column.WebOrientation == EWebOrientation.InPlane)
						WhitmoreYieldCap = ConstNum.FIOMEGA0_9N * (WhitmoreLw * t * brace.BraceConnect.Gusset.Material.Fy + WhitmoreB * beam.Shape.tw * beam.Material.Fy + WhitmoreC * column.Shape.tw * column.Material.Fy);
					else
						WhitmoreYieldCap = ConstNum.FIOMEGA0_9N * (WhitmoreLw * t * brace.BraceConnect.Gusset.Material.Fy + WhitmoreB * beam.Material.Fy * beam.Shape.tw);

					KLoR = 3.464 * Lcr / t;

					BucklingCap = ConstNum.FIOMEGA0_9N * CommonCalculations.BucklingStress(KLoR, brace.BraceConnect.Gusset.Material.Fy, false);

					// begin buckling check of web plate / gusset combination when there is no flange connection
					if (brace.BraceConnect.ClawAngles.DoNotConnectFlanges)
						BucklingCapComp = ConstNum.FIOMEGA0_9N * CommonCalculations.BucklingStress(KLoR, brace.BraceConnect.Gusset.Material.Fy, false);

					if (column.WebOrientation == EWebOrientation.InPlane)
					{
						WhitmSectStressTen = brace.AxialTension / (WhitmoreLw * t + WhitmoreB * beam.Shape.tw + WhitmoreC * column.Shape.tw);
						WhitmSectStressComp = brace.AxialCompression / (WhitmoreLw * t + WhitmoreB * beam.Shape.tw + WhitmoreC * column.Shape.tw);
					}
					else
					{
						WhitmSectStressTen = brace.AxialTension / (WhitmoreLw * t + WhitmoreB * beam.Shape.tw);
						WhitmSectStressComp = brace.AxialCompression / (WhitmoreLw * t + WhitmoreB * beam.Shape.tw);
					}

					if (double.IsNaN(WhitmSectStressTen))
						WhitmSectStressTen = 0;
					if (double.IsNaN(WhitmSectStressComp))
						WhitmSectStressComp = 0;

					if (!brace.BraceConnect.SplicePlates.DoNotConnectWeb)
					{
						var bolts = brace.BraceConnect.SplicePlates.Bolt;
						brace.BraceConnect.SplicePlates.Bolt = bolts;
					}

					BearingOKComp = BearingCCapComp + BearingWCapComp >= brace.AxialCompression;
					BearingOKTen = BearingCCapTen + BearingWCapTen >= brace.AxialTension;

					if (!brace.BraceConnect.ClawAngles.DoNotConnectFlanges && brace.BraceConnect.Brace.FlangeForceTension > 0)
						TearOutCOK = TearOutCCap >= brace.BraceConnect.Brace.FlangeForceTension;
					else
						TearOutCOK = true;

					if (!brace.BraceConnect.SplicePlates.DoNotConnectWeb && brace.BraceConnect.Brace.WebForceTension > 0)
						TearOutWOK = TearOutWCap >= brace.BraceConnect.Brace.WebForceTension;
					else
						TearOutWOK = true;
					WhitmoreYielOKTen = WhitmoreYieldCap >= brace.AxialTension;
					WhitmoreYielOKComp = WhitmoreYieldCap >= brace.AxialCompression;

					if (brace.AxialCompression > 0)
					{
						WhitmoreBuckOK = BucklingCap >= WhitmSectStressComp;
						CompositSectionOK = BucklingCapComp >= 0;
					}
					else
					{
						WhitmoreBuckOK = true;
						CompositSectionOK = true;
					}
					if (BearingOKComp && BearingOKTen && TearOutCOK && TearOutWOK && WhitmoreYielOKTen && WhitmoreYielOKComp && WhitmoreBuckOK && CompositSectionOK) // && beamweldok ))
						break;
				}
				if (brace.KBrace && (memberType == EMemberType.LowerLeft || memberType == EMemberType.LowerRight))
					KBraceAdditionalChecks.CalcKBraceAdditionalChecks(memberType, ref t, printout);
				else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam && (memberType == EMemberType.LowerLeft || memberType == EMemberType.UpperLeft))
					KBraceAdditionalChecks.CalcKBraceAdditionalChecks(memberType, ref t, printout);

				if (CommonDataStatic.Preferences.UseContinuousClipAngles && !((brace.KBrace | brace.KneeBrace) || CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam))
					tmin = Math.Max(sameSide.BraceConnect.Gusset.Thickness, beam.Shape.tw);
				else
					tmin = ConstNum.THREE_EIGHTS_INCH;
				t = Math.Max(t, tmin);

				if (t <= ConstNum.ONE_INCH)
					Gth = NumberFun.Round(t, 16);
				else if (t <= ConstNum.THREE_INCHES)
					Gth = NumberFun.Round(t, 8);
				else
					Gth = NumberFun.Round(t, 4);
				if (Gth > ConstNum.FOUR_INCHES)
					Gth = ConstNum.THREE_INCHES;
				if (CommonDataStatic.Units == EUnit.Metric)
					Gth = Math.Ceiling(Gth);

				brace.BraceConnect.Gusset.Thickness = CommonCalculations.PlateThickness(Gth);
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
				{
					brace.BraceConnect.Gusset.Thickness = Math.Max(brace.BraceConnect.Gusset.Thickness, otherSide.BraceConnect.Gusset.Thickness);
					otherSide.BraceConnect.Gusset.Thickness = brace.BraceConnect.Gusset.Thickness;
				}
				else if (brace.KBrace)
				{
					brace.BraceConnect.Gusset.Thickness = Math.Max(brace.BraceConnect.Gusset.Thickness, sameSide.BraceConnect.Gusset.Thickness);
					sameSide.BraceConnect.Gusset.Thickness = brace.BraceConnect.Gusset.Thickness;
				}

				gussetThickness = brace.BraceConnect.Gusset.Thickness;

				if (BearingOKTen)
					OKBearTen = " >= " + brace.AxialTension + ConstUnit.Force + " (OK)";
				else
					OKBearTen = " << " + brace.AxialTension + ConstUnit.Force + " (NG)";

				if (BearingOKComp)
					OKBearComp = " >= " + brace.AxialCompression + ConstUnit.Force + " (OK)";
				else
					OKBearComp = " << " + brace.AxialCompression + ConstUnit.Force + " (NG)";

				if (WhitmoreYielOKTen)
					OKWhitYieldTen = " >= " + brace.AxialTension + ConstUnit.Force + " (OK)";
				else
					OKWhitYieldTen = " << " + brace.AxialTension + ConstUnit.Force + " (NG)";

				if (WhitmoreYielOKComp)
					OKWhitYieldComp = " >= " + brace.AxialCompression + ConstUnit.Force + " (OK)";
				else
					OKWhitYieldComp = " << " + brace.AxialCompression + ConstUnit.Force + " (NG)";

				Reporting.AddHeader("Try Gusset Thickness (t) = " + brace.BraceConnect.Gusset.Thickness);
				if (!brace.BraceConnect.ClawAngles.DoNotConnectFlanges)
				{
					Reporting.AddHeader("Bolt Bearing on Gusset at Claw Angles:");
					Reporting.AddHeader("With Tensile Force:");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Fbe + Fbs * (n - 1)) * t");
					Reporting.AddLine("= 2 * (" + FpeClaw + " + " + FpsClaw + " * (" + brace.BoltGusset.NumberOfBolts + " - 1)) * " + brace.BraceConnect.Gusset.Thickness);
					Reporting.AddLine("= " + BearingCCapTen + ConstUnit.Force);
					Reporting.AddHeader("With Compressive Force:");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * Fbs * n * t");
					Reporting.AddLine("= 2 * " + FpsClaw + " * " + brace.BoltGusset.NumberOfBolts + " * " + brace.BraceConnect.Gusset.Thickness);
					Reporting.AddLine("= " + BearingCCapComp + ConstUnit.Force);
				}

				if (!brace.BraceConnect.SplicePlates.DoNotConnectWeb)
				{
					Reporting.AddHeader("Bolt Bearing on Gusset at Splice Plates:");
					Reporting.AddHeader("With Tensile Force:");
					Reporting.AddLine(ConstString.PHI + " Rn = (Nw * Fbe + (n - Nw) * Fbs) * t");
					Reporting.AddLine("= (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfLines + " * " + FpeWeb + " + (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts + " - " + brace.BraceConnect.SplicePlates.Bolt.NumberOfLines + ") * " + FpsWeb + ") * " + brace.BraceConnect.Gusset.Thickness);
					Reporting.AddLine("= " + BearingWCapTen + ConstUnit.Force);

					Reporting.AddHeader("With Compressive Force:");
					Reporting.AddLine(ConstString.PHI + " Rn = n * Fbs) * t");
					Reporting.AddLine("= (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts + " * " + FpsWeb + " * " + brace.BraceConnect.Gusset.Thickness);
					Reporting.AddLine("= " + BearingWCapComp + ConstUnit.Force);
				}
				if (!brace.BraceConnect.ClawAngles.DoNotConnectFlanges & !brace.BraceConnect.SplicePlates.DoNotConnectWeb)
				{
					Reporting.AddHeader("Total " + ConstString.DES_OR_ALLOWABLE + " Bearing Strength with Tensile Force:");
					Reporting.AddLine("= " + BearingCCapTen + " + " + BearingWCapTen);
					Reporting.AddLine("= " + BearingCCapTen + BearingWCapTen + ConstUnit.Force + OKBearTen);

					Reporting.AddHeader("Total " + ConstString.DES_OR_ALLOWABLE + " Bearing Strength with Compressive Force:");
					Reporting.AddLine("= " + BearingCCapComp + " + " + BearingWCapComp);
					Reporting.AddLine("= " + BearingCCapComp + BearingWCapComp + ConstUnit.Force + OKBearComp);
				}
				else if (!brace.BraceConnect.SplicePlates.DoNotConnectWeb)
				{
					if (BearingWCapTen >= brace.AxialTension)
						Reporting.AddLine(BearingWCapTen + " >= " + brace.AxialTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(BearingWCapTen + " << " + brace.AxialTension + ConstUnit.Force + " (NG)");
					if (BearingWCapComp >= brace.AxialCompression)
						Reporting.AddLine(BearingWCapComp + " >= " + brace.AxialCompression + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(BearingWCapComp + " << " + brace.AxialCompression + ConstUnit.Force + " (NG)");
				}
				else
				{
					Reporting.AddLine(BearingCCapTen + ConstUnit.Force + OKBearTen);
					Reporting.AddLine(BearingCCapComp + ConstUnit.Force + OKBearComp);
				}
				if (brace.AxialTension > 0)
				{
					if (!brace.BraceConnect.ClawAngles.DoNotConnectFlanges)
					{
						Reporting.AddHeader("Gusset Tear-out Through Bolts at Claw Angles:");
						Reporting.AddLine("Claw Angle Gage (g) = " + anglegage + ConstUnit.Length);
						Agv = 2 * (brace.BoltGusset.SpacingLongDir * (brace.BoltGusset.NumberOfBolts - 1) + brace.BoltGusset.EdgeDistGusset) * brace.BraceConnect.Gusset.Thickness;
						Reporting.AddLine("Agv = 2 * (s * (n - 1) + e) * t");
						Reporting.AddLine("= 2 * (" + brace.BoltGusset.SpacingLongDir + " * (" + brace.BoltGusset.NumberOfBolts + " - 1) + " + brace.BoltGusset.EdgeDistGusset + ") * " + brace.BraceConnect.Gusset.Thickness);
						Reporting.AddLine("= " + Agv + ConstUnit.Area);

						Anv = Agv - 2 * (brace.BoltGusset.NumberOfBolts - 0.5) * (HoleSizeCL + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
						Reporting.AddLine("Anv = Agv - 2*(n - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
						Reporting.AddLine("= " + Agv + " - 2 *(" + brace.BoltGusset.NumberOfBolts + " - 0.5) * (" + HoleSizeCL + " + " + ConstNum.SIXTEENTH_INCH + ")*" + brace.BraceConnect.Gusset.Thickness);
						Reporting.AddLine("= " + Anv + ConstUnit.Area);

						Agt = (brace.Shape.d + 2 * anglegage) * brace.BraceConnect.Gusset.Thickness;
						Reporting.AddLine("Agt = (d_brace + 2 * g) * t");
						Reporting.AddLine("= (" + brace.Shape.d + " + 2 * " + anglegage + ") * " + brace.BraceConnect.Gusset.Thickness);
						Reporting.AddLine("= " + Agt + ConstUnit.Area);

						Ant = Agt - (HoleSizeCT + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
						Reporting.AddLine("Ant = Agt - (dh+" + ConstNum.SIXTEENTH_INCH + ") * t");
						Reporting.AddLine("= " + Agt + " - (" + HoleSizeCT + " + " + ConstNum.SIXTEENTH_INCH + ")*" + brace.BraceConnect.Gusset.Thickness);
						Reporting.AddLine("= " + Ant + ConstUnit.Area);

						FiRnC = SmallMethodsDesign.BlockShearPrint(Agv, Anv, Agt, Ant, brace.BraceConnect.Gusset.Material.Fy, brace.BraceConnect.Gusset.Material.Fu, true);
						if (FiRnC >= brace.AxialTension)
							Reporting.AddLine("= " + FiRnC + " >= " + brace.AxialTension + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + FiRnC + " << " + brace.AxialTension + ConstUnit.Force + " (NG)");
					}
					if (!brace.BraceConnect.SplicePlates.DoNotConnectWeb)
					{
						Reporting.AddHeader("Gusset Block Shear Through Bolts at Splice Plate:");
						Agv = 2 * (brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir * (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) + brace.BraceConnect.SplicePlates.Bolt.EdgeDistGusset) * brace.BraceConnect.Gusset.Thickness;
						Reporting.AddLine("Agv = 2 * (sl * (nl - 1) + eg) * t");
						Reporting.AddLine("= 2 * (" + brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir + "*(" + brace.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 1) + " + brace.BraceConnect.SplicePlates.Bolt.EdgeDistGusset + ")*" + brace.BraceConnect.Gusset.Thickness);
						Reporting.AddLine("= " + Agv + ConstUnit.Area);

						Anv = Agv - 2 * (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 0.5) * (HoleSizeWL + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
						Reporting.AddLine("Anv = Agv - 2*(Nl - 0.5)*(dh + " + ConstNum.SIXTEENTH_INCH + ")*t");
						Reporting.AddLine("= " + Agv + " - 2*(" + brace.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 1) * (" + HoleSizeWL + " + " + ConstNum.SIXTEENTH_INCH + ")*" + brace.BraceConnect.Gusset.Thickness);
						Reporting.AddLine("= " + Anv + ConstUnit.Area);

						Agt = brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir * (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * brace.BraceConnect.Gusset.Thickness;
						Reporting.AddLine("Agt = st * (Nt - 1) * t");
						Reporting.AddLine("= " + brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir + " * (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * " + brace.BraceConnect.Gusset.Thickness);
						Reporting.AddLine("= " + Agt + ConstUnit.Area);

						Ant = Agt - (HoleSizeWT + ConstNum.SIXTEENTH_INCH) * (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * brace.BraceConnect.Gusset.Thickness;
						Reporting.AddLine("Ant = Agt - (dh + " + ConstNum.SIXTEENTH_INCH + ") * (Nt - 1) * t");
						Reporting.AddLine("= " + Agt + " - (" + HoleSizeWT + " + " + ConstNum.SIXTEENTH_INCH + ") * (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * " + brace.BraceConnect.Gusset.Thickness);
						Reporting.AddLine("= " + Ant + ConstUnit.Area);

						FiRn = SmallMethodsDesign.BlockShearPrint(Agv, Anv, Agt, Ant, brace.BraceConnect.Gusset.Material.Fy, brace.BraceConnect.Gusset.Material.Fu, true);
						if (FiRn >= brace.BraceConnect.Brace.WebForceTension)
							Reporting.AddLine("= " + FiRn + " >= " + brace.BraceConnect.Brace.WebForceTension + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + FiRn + " << " + brace.BraceConnect.Brace.WebForceTension + ConstUnit.Force + " (NG)");
					}
				}
				printout = true;
				SmallMethodsDesign.GussetShear(memberType, ref t);
				Reporting.AddHeader("Check Whitmore Section:");
				if (!brace.BraceConnect.ClawAngles.DoNotConnectFlanges)
				{
					Reporting.AddLine("Width (Lw) = (n - 1) * s * 1.1547 + d_brace + 2 * g ");
					Reporting.AddLine("= (" + brace.BoltGusset.NumberOfBolts + " - 1) * " + brace.BoltGusset.SpacingLongDir + " * 1.1547 + " + brace.Shape.d + " + 2 * " + anglegage);
				}
				else
				{
					Reporting.AddLine("Width (Lw) = (nl - 1) * sl * 1.1547 + st * (nw - 1)");
					Reporting.AddLine("= (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 1) * " + brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir + " * 1.1547 + " + brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir + " * (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 1)");
				}
				Reporting.AddLine("= " + WhitmoreLw1 + ConstUnit.Length);
				if (WhitmoreOut > 0)
					Reporting.AddLine("Lwo = " + WhitmoreOut + ConstUnit.Length + " of Lw is outside the gusset free edge.");
				if (WhitmoreC > 0)
					Reporting.AddLine("Lwc = " + WhitmoreC + ConstUnit.Length + " of Lw is in the column.");
				if (WhitmoreB > 0)
					Reporting.AddLine("Lwb = " + WhitmoreB + ConstUnit.Length + " of Lw is in the " + beamStr + ".");

				Reporting.AddHeader("Width of Whitmore Section inside gusset boundaries (Lwg) = " + WhitmoreLw + ConstUnit.Length);
				Reporting.AddHeader("Whitmore Section Yielding:");
				if (column.WebOrientation == EWebOrientation.InPlane)
				{
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * (Lwg * t * Fyg + Lwb * twb * Fyb + Lwc * twc * Fyc) ");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * (" + WhitmoreLw + " * " + brace.BraceConnect.Gusset.Thickness + " * " + brace.BraceConnect.Gusset.Material.Fy + " + " + WhitmoreB + " * " + beam.Shape.tw + " * " + beam.Material.Fy + " + " + WhitmoreC + " * " + column.Shape.tw + " * " + column.Material.Fy + ") ");
					Reporting.AddLine("= " + WhitmoreYieldCap + ConstUnit.Force + OKWhitYieldTen + " (Tension)");
					Reporting.AddLine("= " + WhitmoreYieldCap + ConstUnit.Force + OKWhitYieldComp + " (Compression)");
				}
				else
				{
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + "*(Lwg * t * Fyg + Lwb * twb * Fyb) ");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + "*(" + WhitmoreLw + " * " + brace.BraceConnect.Gusset.Thickness + " * " + brace.BraceConnect.Gusset.Material.Fy + " + " + WhitmoreB + " * " + beam.Shape.tw + " * " + beam.Material.Fy + ") ");
					Reporting.AddLine("= " + WhitmoreYieldCap + ConstUnit.Force + OKWhitYieldTen + " (Tension)");
					Reporting.AddLine("= " + WhitmoreYieldCap + ConstUnit.Force + OKWhitYieldComp + " (Compression)");
				}

				if (brace.AxialCompression > 0)
				{
					// (tensionandcompression Or member(m).Fx < 0) Then
					Reporting.AddHeader("Buckling Check:");
					Reporting.AddHeader("Whitmore Section Stress:");
					if (column.WebOrientation == EWebOrientation.InPlane)
					{
						Reporting.AddLine("fa = Fx/(Lwg*t + Lwb*twb + Lwc*twc) ");
						Reporting.AddLine("= " + brace.AxialCompression + " / (" + WhitmoreLw + " * " + brace.BraceConnect.Gusset.Thickness + " + " + WhitmoreB + " * " + beam.Shape.tw + " + " + WhitmoreC + " * " + column.Shape.tw + ") ");
						Reporting.AddLine("= " + WhitmSectStressComp + ConstUnit.Stress);
					}
					else
					{
						Reporting.AddLine("fa = Fx / (Lwg * t + Lwb * twb) ");
						Reporting.AddLine("= " + brace.AxialCompression + "/(" + WhitmoreLw + " * " + brace.BraceConnect.Gusset.Thickness + " + " + WhitmoreB + " * " + beam.Shape.tw + ") ");
						Reporting.AddLine("= " + WhitmSectStressComp + ConstUnit.Stress);
					}

					KLoR = 3.464 * Lcr / brace.BraceConnect.Gusset.Thickness;

					if ((brace.KBrace | brace.KneeBrace) || brace.WinConnect.ShearClipAngle.Position == EPosition.NoConnection || CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam || brace.BraceConnect.Gusset.DontConnectBeam)
						Reporting.AddHeader("Effective Length of Whitmore Section (K = 1.2), Lcr = " + Lcr + ConstUnit.Length);
					else
						Reporting.AddHeader("Effective Length of Whitmore Section (K = 0.5), Lcr = " + Lcr + ConstUnit.Length);

					Reporting.AddLine("Kl / r = Lcr / (t / 12^0.5) = " + Lcr + " / (" + brace.BraceConnect.Gusset.Thickness + " / 3.464)");
					Reporting.AddLine("= " + KLoR);

					BucklingCap = ConstNum.FIOMEGA0_9N * CommonCalculations.BucklingStress(KLoR, brace.BraceConnect.Gusset.Material.Fy, true);

					if (BucklingCap >= WhitmSectStressComp)
						Reporting.AddLine("Buckling Strength = " + ConstString.FIOMEGA0_9 + "*Fcr = " + BucklingCap + " >= " + WhitmSectStressComp + " ksi (OK)");
					else
						Reporting.AddLine("Buckling Strength = " + ConstString.FIOMEGA0_9 + "*Fcr = " + BucklingCap + " << " + WhitmSectStressComp + " ksi (NG)");
				}
				printout = true;
				SmallMethodsDesign.GussetShear(memberType, ref t);

				if (brace.KBrace && (memberType == EMemberType.LowerLeft || memberType == EMemberType.LowerRight))
				{
					t = brace.BraceConnect.Gusset.Thickness;
					printout = true;
					KBraceAdditionalChecks.CalcKBraceAdditionalChecks(memberType, ref t, printout);
				}
				else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam && (memberType == EMemberType.LowerLeft || memberType == EMemberType.UpperLeft))
				{
					t = brace.BraceConnect.Gusset.Thickness;
					printout = true;
					KBraceAdditionalChecks.CalcKBraceAdditionalChecks(memberType, ref t, printout);
				}
			}
			return gussetThickness;
		}
	}
}