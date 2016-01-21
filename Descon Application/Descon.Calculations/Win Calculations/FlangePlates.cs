using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class FlangePlates
	{
		internal static void DesignFlangePlates(EMemberType memberType)
		{
			double tmpCaseArg;
			double weldlength_1;
			double Reinforcementsize;
			double RequiredWeldSize;
			double capacity;
			double a;
			double WeakestWidth;
			double UnbracedLength;
			double wLength;
			double BetaN;
			double sqr12;
			double AnShear;
			double AgShear;
			double AnTension;
			double AgTension;
			double Tn;
			double bn;
			double bn2;
			double bn1;
			double Tg;
			double bgrosswidth = 0;
			double tgrosswidth = 0;
			double Cap;
			double BeamTrEdge;
			double ehmin;
			double evmin;
			double spPlate;
			double spBeam;
			double Max_w_t_Ratio;
			double WidthTicknessRatioB;
			double WidthTicknessRatioT;
			double BearingCap = 0;
			double Fbs = 0;
			double si = 0;
			double Fbe = 0;
			double fullpenweldcap;
			double XDistancefromColumnFace;
			double LengthOfSideWeld;
			double wmin;
			double tForNetArea;
			double U = 0;
			double Lmax;
			double Lcheck;
			double L2 = 0;
			double weldlength = 0;
			double WeldAreaReq;
			double RpL;
			double maxwidth;
			double minw;
			double FiRn;
			double Fcr;
			double kL_r;
			double rg;
			double k;
			double t;
			double ef;
			double tbReq = 0;
			double tReq_BlockShear;
			double LnShear;
			double LgShear;
			double LnTension;
			double LgTension;
			double edgedist;
			double tnReq;
			double tgReq;
			double bAtBoltHole;
			double HoleSize;
			double Anreq;
			double AgReq;
			double Tt_cutOut = 0;
			double Bt_cutOut = 0;
			double Bt_todevelopsidewelds;
			double Tt_todevelopsidewelds = 0;
			int numberOfActiveMoments;
			double dc;
			double BF;
			double t_for_slenderness;
			double t_forCompression;
			double Ws;
			double WsTop;
			double WsBot;
			double EldiffThreshold;
			double RLoadL;
			double RLoadR;
			double SupThickness;
			double Ff;
			double Rload;
			double w;
			double w1;
			double MinWidth;

			string capacityText;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			if (rightBeam.MomentConnection != EMomentCarriedBy.NoMoment && leftBeam.MomentConnection != EMomentCarriedBy.NoMoment)
				numberOfActiveMoments = 2;
			else
				numberOfActiveMoments = 1;

			var flangePlate = beam.WinConnect.MomentFlangePlate;

			if (!flangePlate.Bolt.EdgeDistLongDir_User) // Added to property set EdgeDistLongDir according to Descon 7 code
				flangePlate.Bolt.EdgeDistLongDir = flangePlate.Bolt.MinEdgeSheared;

			if (!flangePlate.Bolt.EdgeDistTransvDir_User)
			{
				double edgeDistance = (beam.Shape.bf - beam.GageOnFlange) / 2;
				edgeDistance = NumberFun.Round(edgeDistance, ERoundingPrecision.Eighth, ERoundingStyle.RoundUp);
				flangePlate.Bolt.EdgeDistTransvDir = Math.Max(edgeDistance, flangePlate.Bolt.MinEdgeSheared);
			}

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BraceToColumn:
					return;
				case EJointConfiguration.BeamToGirder:
					SupThickness = column.Shape.tw;
					RLoadR = Math.Abs(rightBeam.P / 2) + Math.Abs(rightBeam.Moment / rightBeam.Shape.d);
					RLoadL = Math.Abs(leftBeam.P / 2) + Math.Abs(leftBeam.Moment / leftBeam.Shape.d);
					Rload = Math.Max(RLoadR, RLoadL);
					Ff = Rload;
					break;
				default:
					if (CommonDataStatic.IsFema)
						Rload = Math.Abs(beam.Moment / beam.Shape.d);
					else
						Rload = Math.Abs(beam.P / 2) + Math.Abs(beam.Moment / beam.Shape.d);
					Ff = Rload;

					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
						SupThickness = column.Shape.tf;
					else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
						SupThickness = column.Shape.tw;
					else
						SupThickness = 0;
					break;
			}
			if (!CommonDataStatic.IsFema)
			{
				if (flangePlate.Connection == EConnectionStyle.Bolted)
				{
					if (!flangePlate.Bolt.NumberOfRows_User)
						flangePlate.Bolt.NumberOfRows = (int)Math.Ceiling(Rload / flangePlate.Bolt.BoltStrength / flangePlate.Bolt.NumberOfLines);
					flangePlate.Bolt.NumberOfBolts = flangePlate.Bolt.NumberOfRows * flangePlate.Bolt.NumberOfLines;
					GetBeamGage(memberType);
					switch (CommonDataStatic.BeamToColumnType)
					{
						case EJointConfiguration.BeamToHSSColumn:
							if (!flangePlate.TopWidth_User || flangePlate.TopWidth == 0)
								flangePlate.TopWidth = NumberFun.Round(beam.Shape.bf, 4);

							EldiffThreshold = 0.75 * ConstNum.ONE_INCH;
							if (CommonDataStatic.ColumnStiffener.TopOfBeamToColumn <= EldiffThreshold)
							{
								CommonDataStatic.ColumnStiffener.TopOfBeamToColumn = EldiffThreshold;
								WsBot = (int)Math.Ceiling(2 * flangePlate.TopWidth) / 4.0;
								WsTop = WsBot;
							}
							else
							{
								Ws = (int)Math.Ceiling(2 * flangePlate.TopWidth) / 4.0;
								WsTop = Ws;
								WsBot = Ws;
							}

							t_forCompression = Rload / (2 * ConstNum.FIOMEGA0_9N * flangePlate.Material.Fy * WsBot);
							t_for_slenderness = WsBot / (0.56 * Math.Pow(ConstNum.ELASTICITY / flangePlate.Material.Fy, 0.5));

							if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToColumnWeb)
							{
								BF = column.Shape.d;
								dc = column.Shape.bf;
							}
							else
							{
								BF = column.Shape.bf;
								dc = column.Shape.d;
							}
							if (!flangePlate.TopWrapAroundWidth_User)
								flangePlate.TopWrapAroundWidth = WsTop;

							flangePlate.BottomWrapAroundWidth = WsBot;
							flangePlate.TopWidthAtColumn = BF + 2 * flangePlate.TopWrapAroundWidth;
							flangePlate.BottomWidthAtColumn = BF + 2 * flangePlate.BottomWrapAroundWidth;
							if (rightBeam.IsActive && rightBeam.MomentConnection == EMomentCarriedBy.FlangePlate && leftBeam.IsActive && leftBeam.MomentConnection == EMomentCarriedBy.FlangePlate)
								numberOfActiveMoments = 2;
							else
								numberOfActiveMoments = 1;

							flangePlate.BottomHSSSideWallWeldLength = (dc - 3 * column.Shape.tf) / numberOfActiveMoments;
							if (flangePlate.TopWrapAroundWidth <= 0.75 * ConstNum.ONE_INCH)
								flangePlate.TopHSSSideWallWeldLength = column.Shape.d / numberOfActiveMoments;
							else
								flangePlate.TopHSSSideWallWeldLength = (dc - 3 * column.Shape.tf) / numberOfActiveMoments;

							if (CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn && CommonDataStatic.ColumnStiffener.TopOfBeamToColumn <= 0.75 * ConstNum.ONE_INCH)
								Tt_todevelopsidewelds = Rload / (2 * ConstNum.FIOMEGA1_0N * 0.6 * flangePlate.TopHSSSideWallWeldLength * flangePlate.Material.Fy);
							Bt_todevelopsidewelds = Rload / (2 * ConstNum.FIOMEGA1_0N * 0.6 * flangePlate.BottomHSSSideWallWeldLength * flangePlate.Material.Fy);
							Bt_cutOut = Math.Max(Math.Max(t_forCompression, Bt_todevelopsidewelds), Math.Max(Bt_todevelopsidewelds, t_for_slenderness));
							Tt_cutOut = Math.Max(Bt_cutOut, Tt_todevelopsidewelds);
							AgReq = Rload / (ConstNum.FIOMEGA0_9N * flangePlate.Material.Fy);
							Anreq = Rload / (ConstNum.FIOMEGA0_75N * flangePlate.Material.Fu);
							HoleSize = flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH;
							if (flangePlate.TopLength == flangePlate.TopWrapAroundWidth)
								bAtBoltHole = flangePlate.TopWidthAtColumn;
							else
								bAtBoltHole = Math.Min(flangePlate.TopWidthAtColumn, flangePlate.TopWidth + (flangePlate.TopWidthAtColumn - flangePlate.TopWidth) / (flangePlate.TopLength - flangePlate.TopWrapAroundWidth) * (flangePlate.TopLength - beam.EndSetback - beam.WinConnect.Beam.BoltEdgeDistanceOnFlange));

							if (CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn && CommonDataStatic.ColumnStiffener.TopOfBeamToColumn <= 0.75 * ConstNum.ONE_INCH)
							{
								tgReq = AgReq / bAtBoltHole;
								tnReq = Math.Max(Anreq / (bAtBoltHole - Math.Max(0.15 * bAtBoltHole, flangePlate.Bolt.NumberOfLines * HoleSize)), Anreq / (0.85 * flangePlate.TopWidthAtColumn));
							}
							else
							{
								tgReq = AgReq / Math.Min(bAtBoltHole, 2 * flangePlate.TopWrapAroundWidth);
								tnReq = Math.Max(Anreq / (bAtBoltHole - Math.Max(0.15 * bAtBoltHole, flangePlate.Bolt.NumberOfLines * HoleSize)), Anreq / (0.85 * 2 * flangePlate.TopWrapAroundWidth));
							}
							break;
						default:
							//momentFlange.Bolt.EdgeDistTransvDir = (beam.Shape.bf - beam.GageOnFlange) / 2;
							if (!flangePlate.TopWidth_User || flangePlate.TopWidth == 0)
							{
								if (flangePlate.Bolt.NumberOfLines == 4)
									flangePlate.TopWidth = NumberFun.Round(beam.GageOnFlange + 2 * (flangePlate.TransvBoltSpacingOut + Math.Max(flangePlate.Bolt.MinEdgeSheared, flangePlate.Bolt.EdgeDistTransvDir)), 8);
								else
									flangePlate.TopWidth = NumberFun.Round(beam.GageOnFlange + 2 * Math.Max(flangePlate.Bolt.MinEdgeSheared, flangePlate.Bolt.EdgeDistTransvDir), 8);
							}

							bAtBoltHole = flangePlate.TopWidth;
							AgReq = Rload / (ConstNum.FIOMEGA0_9N * flangePlate.Material.Fy);
							Anreq = Rload / (ConstNum.FIOMEGA0_75N * flangePlate.Material.Fu);
							tgReq = AgReq / flangePlate.TopWidth;
							HoleSize = flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH;
							tnReq = Anreq / (flangePlate.TopWidth - Math.Max(0.15 * flangePlate.TopWidth, flangePlate.Bolt.NumberOfLines * HoleSize));
							break;
					}

					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
					{
						if (flangePlate.Bolt.NumberOfLines == 4)
							edgedist = (flangePlate.TopWidthAtColumn - (beam.GageOnFlange + 2 * flangePlate.TransvBoltSpacingOut)) / 2;
						else
							edgedist = (flangePlate.TopWidthAtColumn - beam.GageOnFlange) / 2;
					}
					else
						edgedist = flangePlate.Bolt.EdgeDistTransvDir;
					if (flangePlate.Bolt.NumberOfLines == 4)
					{
						LgTension = Math.Min(beam.GageOnFlange, 2 * edgedist) + 2 * flangePlate.TransvBoltSpacingOut;
						LnTension = LgTension - 3 * (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
					}
					else
					{
						LgTension = Math.Min(beam.GageOnFlange, 2 * edgedist);
						LnTension = LgTension - (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
					}
					LgShear = 2 * ((flangePlate.Bolt.NumberOfRows - 1) * flangePlate.Bolt.SpacingLongDir + flangePlate.Bolt.EdgeDistLongDir);
					LnShear = LgShear - 2 * (flangePlate.Bolt.NumberOfRows - 0.5) * (flangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);

					tReq_BlockShear = Rload / MiscCalculationsWithReporting.BlockShearNew(flangePlate.Material.Fu, LnShear, 1, LnTension, LgShear, flangePlate.Material.Fy, 1, 0, false);

					GetRequiredThicknessForBearing(memberType, ref Fbe, ref si, ref Fbs, ref BearingCap, Rload, ref tbReq);
					if (!flangePlate.TopThickness_User)
						flangePlate.TopThickness = CommonCalculations.PlateThickness(Math.Max(Math.Max(tgReq, tnReq), Math.Max(tReq_BlockShear, Math.Max(tbReq, Tt_cutOut))));
					if (!flangePlate.BottomThickness_User)
						flangePlate.BottomThickness = CommonCalculations.PlateThickness(Math.Max(Math.Max(tgReq, tnReq), Math.Max(tReq_BlockShear, Math.Max(tbReq, Bt_cutOut))));

					ef = MiscCalculationsWithReporting.MinimumEdgeDist(flangePlate.Bolt.BoltSize, Rload / flangePlate.Bolt.NumberOfBolts, beam.Material.Fu * beam.Shape.tf, flangePlate.Bolt.MinEdgeSheared, (int)flangePlate.Bolt.HoleLength, flangePlate.Bolt.HoleType);
					if (!beam.WinConnect.Beam.BoltEdgeDistanceOnFlange_User)
						beam.WinConnect.Beam.BoltEdgeDistanceOnFlange = Math.Max(ef, beam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
					if (!flangePlate.TopLength_User)
						flangePlate.TopLength = flangePlate.Bolt.SpacingLongDir * (flangePlate.Bolt.NumberOfRows - 1) + flangePlate.Bolt.EdgeDistLongDir + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + beam.EndSetback;

					// Here we should set BottomPlate = TopPlate    -RM 01/29/2015
					flangePlate.BottomLength = flangePlate.TopLength;
					flangePlate.BottomPlateToBeamWeldSize = flangePlate.TopPlateToBeamWeldSize;
					flangePlate.BottomPlateToSupportWeldSize = flangePlate.TopPlateToSupportWeldSize;
					flangePlate.BottomThickness = flangePlate.TopThickness;
					flangePlate.BottomWidth = flangePlate.TopWidth;
					//flangePlate.BottomHSSSideWallWeldLength = flangePlate.TopHSSSideWallWeldLength;

					t = flangePlate.TopThickness;

					do
					{
						k = 0.65;
						beam.WinConnect.Fema.L = (beam.EndSetback + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
						rg = t / Math.Sqrt(12);
						kL_r = (k * beam.WinConnect.Fema.L / rg);

						Fcr = CommonCalculations.BucklingStress(kL_r, flangePlate.Material.Fy, false);

						if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
							FiRn = ConstNum.FIOMEGA0_9N * Fcr * 2 * flangePlate.BottomWrapAroundWidth * t;
						else
							FiRn = ConstNum.FIOMEGA0_9N * Fcr * flangePlate.BottomWidth * t;

						if (FiRn < Rload && FiRn != 0)
							t += ConstNum.SIXTEENTH_INCH;
					} while (FiRn < Rload && FiRn != 0);

					if (!flangePlate.TopThickness_User || flangePlate.TopThickness == 0)
						flangePlate.TopThickness = t;

					minw = CommonCalculations.MinimumWeld(flangePlate.TopThickness, SupThickness);
					if (!flangePlate.TopPlateToSupportWeldSize_User || flangePlate.TopPlateToSupportWeldSize == 0)
					{
						flangePlate.TopPlateToSupportWeldSize = NumberFun.Round(Rload / (ConstNum.FIOMEGA0_75N * 1.5 * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min(flangePlate.TopWidth, column.Shape.bf) * 2), 16);
						flangePlate.TopPlateToSupportWeldSize = Math.Max(minw, flangePlate.TopPlateToSupportWeldSize);
					}
					minw = CommonCalculations.MinimumWeld(flangePlate.BottomThickness, SupThickness);
					if (!flangePlate.BottomPlateToSupportWeldSize_User || flangePlate.BottomPlateToSupportWeldSize == 0)
					{
						if (flangePlate.BottomWidth != 0)
							flangePlate.BottomPlateToSupportWeldSize = NumberFun.Round(Rload / (ConstNum.FIOMEGA0_75N * 1.5 * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min(flangePlate.BottomWidth, column.Shape.bf) * 2), 16);
						else
							flangePlate.BottomPlateToSupportWeldSize = NumberFun.Round(Rload / (ConstNum.FIOMEGA0_75N * 1.5 * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * column.Shape.bf * 2), 16);
						flangePlate.BottomPlateToSupportWeldSize = Math.Max(minw, flangePlate.BottomPlateToSupportWeldSize);
					}
				}
				else
				{
					if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToHSSColumn && CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToColumnFlange)
					{
						AgReq = Math.Max(Rload / (ConstNum.FIOMEGA0_9N * flangePlate.Material.Fy), Rload / (ConstNum.FIOMEGA0_75N * flangePlate.Material.Fu));
						maxwidth = beam.Shape.bf - 2 * beam.Shape.tf;
						if (!flangePlate.TopWidth_User || flangePlate.TopWidth == 0)
						{
							do
							{
								flangePlate.TopWidth = maxwidth;
								tgReq = AgReq / flangePlate.TopWidth;
								if (!flangePlate.TopThickness_User || flangePlate.TopThickness == 0)
									flangePlate.TopThickness = CommonCalculations.PlateThickness(tgReq);

								w = CommonCalculations.MinimumWeld(flangePlate.TopThickness, beam.Shape.tf);
								if (flangePlate.TopThickness <= ConstNum.QUARTER_INCH)
									w1 = flangePlate.TopThickness;
								else
									w1 = flangePlate.TopThickness - ConstNum.SIXTEENTH_INCH;
								if (!flangePlate.TopPlateToBeamWeldSize_User || flangePlate.TopPlateToBeamWeldSize == 0)
									flangePlate.TopPlateToBeamWeldSize = Math.Min(w, w1);
								maxwidth = beam.Shape.bf - 2 * (flangePlate.TopPlateToBeamWeldSize + ConstNum.QUARTER_INCH);
							} while (maxwidth < flangePlate.TopWidth);
						}

						do
						{
							RpL = Rload / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.TopPlateToBeamWeldSize);
							WeldAreaReq = (Rload / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx) - flangePlate.TopPlateToBeamWeldSize * flangePlate.TopWidth) / 2; // one line on the side of the plate
							beam.WinConnect.Fema.L1 = ((RpL - flangePlate.TopWidth) / 2);
							if (beam.WinConnect.Fema.L1 >= (5 / 3 * flangePlate.TopWidth))
								weldlength_1 = (2 * beam.WinConnect.Fema.L1 + flangePlate.TopWidth);
							else
							{
								L2 = (RpL - 1.5 * flangePlate.TopWidth) / 1.7;
								weldlength_1 = 2 * L2 + flangePlate.TopWidth;
							}
							Lcheck = Math.Max(beam.WinConnect.Fema.L1, L2);
							Lmax = 10 * Math.Pow(WeldAreaReq, 0.5);
							if (Lmax < Lcheck && (!flangePlate.TopPlateToBeamWeldSize_User || flangePlate.TopPlateToBeamWeldSize == 0))
								flangePlate.TopPlateToBeamWeldSize = flangePlate.TopPlateToBeamWeldSize + ConstNum.SIXTEENTH_INCH;
						} while (!(Lcheck <= Lmax));

						if (!flangePlate.TopLength_User || flangePlate.TopLength == 0)
						{
							if (flangePlate.ConnectionType == EFlangeConnectionType.FullyRestrained)
								flangePlate.TopLength = NumberFun.Round((weldlength_1 - flangePlate.TopWidth) / 2 + beam.EndSetback + flangePlate.TopPlateToBeamWeldSize, 8); // Int(-8 * ((weldlength - TFlangePlate.Width) / 2 + Beam.ercl + TFlangePlate.BSideW)) / 8
							else
								flangePlate.TopLength = NumberFun.Round(flangePlate.TopWidth + weldlength_1 / 2, 8);
						}
						AgReq = Rload / (ConstNum.FIOMEGA0_9N * flangePlate.Material.Fy);
						MinWidth = beam.Shape.bf + 2 * beam.Shape.tf;
						if (!flangePlate.BottomWidth_User || flangePlate.BottomWidth == 0)
						{
							do
							{
								flangePlate.BottomWidth = NumberFun.Round(MinWidth, 8);
								tgReq = AgReq / flangePlate.BottomWidth;
								t = tgReq - ConstNum.SIXTEENTH_INCH;
								do
								{
									t += ConstNum.SIXTEENTH_INCH;
									k = 0.65;
									beam.WinConnect.Fema.L = beam.EndSetback;
									rg = t / Math.Sqrt(12);
									kL_r = (k * beam.WinConnect.Fema.L / rg);
									Fcr = CommonCalculations.BucklingStress(kL_r, flangePlate.Material.Fy, false);
									FiRn = ConstNum.FIOMEGA0_9N * Fcr * flangePlate.BottomWidth * t;
								} while (FiRn < Rload);

								if (!flangePlate.TopThickness_User || flangePlate.TopThickness == 0)
									flangePlate.TopThickness = CommonCalculations.PlateThickness(Math.Max(tgReq, t));

								w = CommonCalculations.MinimumWeld(flangePlate.BottomThickness, beam.Shape.tf);
								if (flangePlate.BottomThickness <= ConstNum.QUARTER_INCH)
									w1 = ((int)Math.Floor(16 * beam.Shape.tf)) / 16.0;
								else
									w1 = beam.Shape.tf - ConstNum.SIXTEENTH_INCH;
								if (!flangePlate.BottomPlateToBeamWeldSize_User || flangePlate.BottomPlateToBeamWeldSize == 0)
									flangePlate.BottomPlateToBeamWeldSize = Math.Min(w, w1);
								do
								{
									weldlength_1 = Rload / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.BottomPlateToBeamWeldSize);
									WeldAreaReq = Rload / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx) / 2;
									Lcheck = weldlength_1 / 2;
									Lmax = 10 * Math.Pow(WeldAreaReq, 0.5);
									if (Lmax < Lcheck)
									{
										if (!flangePlate.BottomPlateToBeamWeldSize_User || flangePlate.BottomPlateToBeamWeldSize == 0)
											flangePlate.BottomPlateToBeamWeldSize = flangePlate.BottomPlateToBeamWeldSize + ConstNum.SIXTEENTH_INCH;
									}
								} while (!(Lcheck <= Lmax));
								if (weldlength_1 < 2 * beam.Shape.bf)
									weldlength = 2 * beam.Shape.bf;
								if (weldlength >= 4 * beam.Shape.bf)
									U = 1;
								else if (weldlength >= 3 * beam.Shape.bf)
									U = 0.87;
								else if (weldlength >= 2 * beam.Shape.bf)
									U = 0.75;
								tForNetArea = Rload / (ConstNum.FIOMEGA0_75N * U * beam.Material.Fu * flangePlate.BottomWidth);
								if (flangePlate.BottomThickness < tForNetArea)
									flangePlate.TopThickness = CommonCalculations.PlateThickness(tForNetArea);
								MinWidth = beam.Shape.bf + 2 * (flangePlate.BottomPlateToBeamWeldSize + ConstNum.QUARTER_INCH);
							} while (MinWidth > flangePlate.BottomWidth);
						}

						if (!flangePlate.BottomLength_User || flangePlate.BottomLength == 0)
							flangePlate.BottomLength = NumberFun.Round(weldlength / 2 + beam.EndSetback + flangePlate.BottomPlateToBeamWeldSize, 8);

						minw = CommonCalculations.MinimumWeld(flangePlate.BottomThickness, SupThickness);
						if (!flangePlate.BottomPlateToSupportWeldSize_User || flangePlate.BottomPlateToSupportWeldSize == 0)
						{
							flangePlate.BottomPlateToSupportWeldSize = NumberFun.Round(Rload / (ConstNum.FIOMEGA0_75N * 1.5 * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min(flangePlate.BottomWidth, column.Shape.bf) * 2), 16);
							flangePlate.BottomPlateToSupportWeldSize = Math.Max(minw, flangePlate.BottomPlateToSupportWeldSize);
						}

						minw = CommonCalculations.MinimumWeld(flangePlate.TopThickness, SupThickness);

						if (!flangePlate.TopPlateToSupportWeldSize_User || flangePlate.TopPlateToSupportWeldSize == 0)
						{
							flangePlate.TopPlateToSupportWeldSize = NumberFun.Round(Rload / (ConstNum.FIOMEGA0_75N * 1.5 * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min(flangePlate.TopWidth, column.Shape.bf) * 2), 16);
							flangePlate.TopPlateToSupportWeldSize = Math.Max(minw, flangePlate.TopPlateToSupportWeldSize);
						}
					}
					else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
					{
						AgReq = Math.Max(Rload / (ConstNum.FIOMEGA0_9N * flangePlate.Material.Fy), Rload / (ConstNum.FIOMEGA0_75N * flangePlate.Material.Fu));
						if (!flangePlate.TopWidth_User || flangePlate.TopWidth == 0)
							flangePlate.TopWidth = beam.Shape.bf - ConstNum.TWO_INCHES;
						if (!flangePlate.BottomWidth_User || flangePlate.BottomWidth == 0)
							flangePlate.BottomWidth = beam.Shape.bf + ConstNum.TWO_INCHES;
						if (!flangePlate.TopWrapAroundWidth_User || flangePlate.TopWrapAroundWidth == 0)
							flangePlate.TopWrapAroundWidth = flangePlate.TopWidth / 2;
						flangePlate.BottomWrapAroundWidth = flangePlate.BottomWidth / 2;

						if (!flangePlate.TopThickness_User || flangePlate.TopThickness == 0)
							flangePlate.TopThickness = CommonCalculations.PlateThickness(AgReq / flangePlate.TopWidth);
						if (!flangePlate.BottomThickness_User || flangePlate.BottomThickness == 0)
							flangePlate.BottomThickness = CommonCalculations.PlateThickness(AgReq / flangePlate.BottomWidth);
						wmin = CommonCalculations.MinimumWeld(flangePlate.TopThickness, beam.Shape.tf);
						if (flangePlate.TopThickness <= ConstNum.QUARTER_INCH)
							w1 = flangePlate.TopThickness;
						else
							w1 = flangePlate.TopThickness - ConstNum.SIXTEENTH_INCH;

						if (!flangePlate.TopPlateToBeamWeldSize_User || flangePlate.TopPlateToBeamWeldSize == 0)
							flangePlate.TopPlateToBeamWeldSize = wmin;
						if (!flangePlate.BottomPlateToBeamWeldSize_User || flangePlate.BottomPlateToBeamWeldSize == 0)
							flangePlate.BottomPlateToBeamWeldSize = wmin;

						maxwidth = beam.Shape.bf - 2 * (flangePlate.TopPlateToBeamWeldSize + ConstNum.QUARTER_INCH);
						LengthOfSideWeld = (Rload / (ConstNum.FIOMEGA0_75N * 0.4242 * flangePlate.TopPlateToBeamWeldSize * CommonDataStatic.Preferences.DefaultElectrode.Fexx) - 1.5 * flangePlate.TopWidth) / 2;

						XDistancefromColumnFace = (column.Shape.bf + 2 * flangePlate.TopWrapAroundWidth - flangePlate.TopWidth) / 2 / 0.57735 + flangePlate.TopWrapAroundWidth;
						if (!flangePlate.TopLength_User || flangePlate.TopLength == 0)
							flangePlate.TopLength = LengthOfSideWeld + XDistancefromColumnFace;
						LengthOfSideWeld = Rload / (ConstNum.FIOMEGA0_75N * 0.4242 * flangePlate.TopPlateToBeamWeldSize * CommonDataStatic.Preferences.DefaultElectrode.Fexx) / 2;
						XDistancefromColumnFace = (column.Shape.bf + 2 * flangePlate.BottomWrapAroundWidth - flangePlate.BottomWidth) / 2 / 0.57735 + flangePlate.BottomWrapAroundWidth;
						if (!flangePlate.BottomLength_User || flangePlate.BottomLength == 0)
						{
							flangePlate.BottomLength = LengthOfSideWeld + XDistancefromColumnFace;
							flangePlate.BottomLength = LengthOfSideWeld;
						}
						flangePlate.ConnectionType = EFlangeConnectionType.FullyRestrained;
						minw = CommonCalculations.MinimumWeld(flangePlate.TopThickness, SupThickness);
						if (rightBeam.IsActive && rightBeam.MomentConnection == EMomentCarriedBy.FlangePlate && leftBeam.IsActive && leftBeam.MomentConnection == EMomentCarriedBy.FlangePlate)
							numberOfActiveMoments = 2;
						else
							numberOfActiveMoments = 1;

						if (flangePlate.TopWrapAroundWidth <= 0.75 * ConstNum.ONE_INCH)
							flangePlate.TopHSSSideWallWeldLength = column.Shape.d / numberOfActiveMoments;
						else
						{
							if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
								flangePlate.TopHSSSideWallWeldLength = (column.Shape.bf - 3 * column.Shape.tf) / numberOfActiveMoments;
							else
								flangePlate.TopHSSSideWallWeldLength = (column.Shape.d - 3 * column.Shape.tf) / numberOfActiveMoments;
						}
						if (!flangePlate.BottomPlateToSupportWeldSize_User || flangePlate.BottomPlateToSupportWeldSize == 0)
						{
							flangePlate.BottomPlateToSupportWeldSize = Rload / (2 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 2 * flangePlate.BottomHSSSideWallWeldLength);
							flangePlate.BottomPlateToSupportWeldSize = NumberFun.Round(Math.Max(minw, flangePlate.BottomPlateToSupportWeldSize), 16);
						}

						fullpenweldcap = 2 * Math.Min(ConstNum.FIOMEGA1_0N * 0.6 * Math.Min(flangePlate.BottomThickness * flangePlate.Material.Fy, 2 * column.Shape.tf * column.Material.Fy), 0.8 * 0.6 * flangePlate.TopThickness * CommonDataStatic.Preferences.DefaultElectrode.Fexx) * flangePlate.BottomHSSSideWallWeldLength;

						if (flangePlate.TopPlateToSupportWeldSize <= 0.3751 * ConstNum.ONE_INCH)
							flangePlate.ConnectionType = EFlangeConnectionType.FullyRestrained;
						else
						{
							flangePlate.ConnectionType = EFlangeConnectionType.PartiallyRestrained;
							flangePlate.TopPlateToSupportWeldSize = 0;
						}

						if (CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn)
						{
							if (!flangePlate.TopPlateToSupportWeldSize_User || flangePlate.TopPlateToSupportWeldSize == 0)
							{
								flangePlate.TopPlateToSupportWeldSize = Rload / (2 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 2 * flangePlate.TopHSSSideWallWeldLength);
								flangePlate.TopPlateToSupportWeldSize = NumberFun.Round(Math.Max(minw, flangePlate.TopPlateToSupportWeldSize), 16);
							}
							fullpenweldcap = 2 * Math.Min(ConstNum.FIOMEGA1_0N * 0.6 * Math.Min(flangePlate.TopThickness * flangePlate.Material.Fy, 2 * column.Shape.tf * column.Material.Fy), 0.8 * 0.6 * flangePlate.TopThickness * CommonDataStatic.Preferences.DefaultElectrode.Fexx) * flangePlate.TopHSSSideWallWeldLength;
							if (flangePlate.TopPlateToSupportWeldSize <= 0.3751 * ConstNum.ONE_INCH)
								flangePlate.ConnectionType = EFlangeConnectionType.FullyRestrained;
							else
							{
								flangePlate.ConnectionType = EFlangeConnectionType.PartiallyRestrained;
								flangePlate.TopPlateToSupportWeldSize = 0;
							}
						}
						else
						{
							if (!flangePlate.TopPlateToSupportWeldSize_User || flangePlate.TopPlateToSupportWeldSize == 0)
								flangePlate.TopPlateToSupportWeldSize = flangePlate.BottomPlateToSupportWeldSize;
							flangePlate.ConnectionType = flangePlate.ConnectionType;
						}
					}
					else
					{
						minw = CommonCalculations.MinimumWeld(flangePlate.BottomThickness, SupThickness);
						if (!flangePlate.BottomPlateToSupportWeldSize_User || flangePlate.BottomPlateToSupportWeldSize == 0)
						{
							flangePlate.BottomPlateToSupportWeldSize = NumberFun.Round(Rload / (ConstNum.FIOMEGA0_75N * 1.5 * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min(flangePlate.BottomWidth, column.Shape.bf) * 2), 16);
							flangePlate.BottomPlateToSupportWeldSize = Math.Max(minw, flangePlate.BottomPlateToSupportWeldSize);
						}

						minw = CommonCalculations.MinimumWeld(flangePlate.TopThickness, SupThickness);
						if (!flangePlate.TopPlateToSupportWeldSize_User || flangePlate.TopPlateToSupportWeldSize == 0)
						{
							flangePlate.TopPlateToSupportWeldSize = NumberFun.Round(Rload / (ConstNum.FIOMEGA0_75N * 1.5 * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min(flangePlate.TopWidth, column.Shape.bf) * 2), 16);
							flangePlate.TopPlateToSupportWeldSize = Math.Max(minw, flangePlate.TopPlateToSupportWeldSize);
						}
						fullpenweldcap = flangePlate.TopThickness * ConstNum.FIOMEGA0_9N * Math.Min(flangePlate.Material.Fy, column.Material.Fy) * Math.Min(flangePlate.BottomWidth, column.Shape.bf);

						if (flangePlate.BottomPlateToSupportWeldSize <= 0.3751 * ConstNum.ONE_INCH)
							flangePlate.ConnectionType = EFlangeConnectionType.FullyRestrained;
						else
						{
							flangePlate.ConnectionType = EFlangeConnectionType.PartiallyRestrained;
							flangePlate.BottomPlateToSupportWeldSize = 0;
						}

						fullpenweldcap = flangePlate.TopThickness * ConstNum.FIOMEGA0_9N * Math.Min(flangePlate.Material.Fy, column.Material.Fy) * Math.Min(flangePlate.TopWidth, column.Shape.bf);
						if (flangePlate.TopPlateToSupportWeldSize <= 0.3751 * ConstNum.ONE_INCH)
							flangePlate.ConnectionType = EFlangeConnectionType.FullyRestrained;
						else
						{
							flangePlate.ConnectionType = EFlangeConnectionType.PartiallyRestrained;
							flangePlate.TopPlateToSupportWeldSize = 0;
						}
					}
				}

				if (CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn && (CommonDataStatic.ColumnStiffener.TopOfBeamToColumn == 0 || CommonDataStatic.ColumnStiffener.TopOfBeamToColumn == 0))
					flangePlate.TopThickness = Math.Max(Math.Max(flangePlate.TopThickness, CommonDataStatic.ColumnStiffener.SThickness), flangePlate.TopThickness);

				beam.WinConnect.Beam.BotAttachThick = flangePlate.BottomThickness;
				beam.WinConnect.Beam.TopAttachThick = flangePlate.TopThickness;
			}

			Reporting.AddMainHeader(beam.ComponentName + " - " + beam.ShapeName + " Moment Connection");
			if (CommonDataStatic.IsFema)
			{
				FemaReporting.DesignFemaReporting(memberType);
				if (flangePlate.Connection == EConnectionStyle.Welded)
					return;
			}
			else
			{
				Reporting.AddHeader("Moment Connection Using Flange Plate: ");
				Reporting.AddHeader("Flange Force (Ff):");
			}

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BeamToHSSColumn:
				case EJointConfiguration.BeamToColumnFlange:
				case EJointConfiguration.ColumnSplice:
				case EJointConfiguration.BeamSplice:
					Ff = Math.Abs(beam.P / 2) + Math.Abs(beam.Moment / beam.Shape.d);
					Rload = Ff;
					if (CommonDataStatic.Units == EUnit.US)
					{
						Reporting.AddLine("= P / 2 + M / d");
						Reporting.AddLine("= " + Math.Abs(beam.P) + " / 2 + " + Math.Abs(beam.Moment) + " / " + beam.Shape.d);
					}
					else
					{
						Reporting.AddLine("= P / 2 + M / d");
						Reporting.AddLine("= " + Math.Abs(beam.P) + " / 2 + " + Math.Abs(beam.Moment) + " / " + beam.Shape.d);
					}
					Reporting.AddLine("= " + Ff + ConstUnit.Force);
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
						SupThickness = column.Shape.tf;
					else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
						SupThickness = column.Shape.tw;
					else
						SupThickness = 0;
					break;
				case EJointConfiguration.BeamToGirder:
					SupThickness = column.Shape.tw;
					RLoadR = Math.Abs(rightBeam.P / 2) + Math.Abs(rightBeam.Moment / rightBeam.Shape.d);
					RLoadL = Math.Abs(leftBeam.P / 2) + Math.Abs(leftBeam.Moment / leftBeam.Shape.d);
					Ff = Math.Max(RLoadR, RLoadL);
					Rload = Ff;
					if (CommonDataStatic.Units == EUnit.US)
					{
						Reporting.AddLine("= P / 2 + M / d");
						Reporting.AddLine("= " + Math.Abs(rightBeam.P) + " / 2 + " + Math.Abs(rightBeam.Moment) + " / " + rightBeam.Shape.d + "= " + RLoadR + ConstUnit.Force + " (Right Side)");
						Reporting.AddLine("= " + Math.Abs(leftBeam.P) + " / 2 + " + Math.Abs(leftBeam.Moment) + " / " + leftBeam.Shape.d + "= " + RLoadL + ConstUnit.Force + " (Left Side)");
					}
					else
					{
						Reporting.AddLine("= P / 2 + M / d");
						Reporting.AddLine("= " + Math.Abs(rightBeam.P) + " / 2 +  " + Math.Abs(rightBeam.Moment) + " / " + rightBeam.Shape.d + "= " + RLoadR + ConstUnit.Force + " (Right Side)");
						Reporting.AddLine("= " + Math.Abs(leftBeam.P) + " / 2 + " + Math.Abs(leftBeam.Moment) + " / " + leftBeam.Shape.d + "= " + RLoadL + ConstUnit.Force + " (Left Side)");
					}
					Reporting.AddLine("Use Ff = " + Ff + ConstUnit.Force);
					break;
			}

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
			{
				if (column.WebOrientation == EWebOrientation.OutOfPlane)
				{
					BF = column.Shape.d;
					dc = column.Shape.bf;
				}
				else
				{
					BF = column.Shape.bf;
					dc = column.Shape.d;
				}

				Ws = NumberFun.Round(flangePlate.TopWidth / 2, 2);
				flangePlate.TopWidthAtColumn = BF + 2 * flangePlate.TopWrapAroundWidth;
				flangePlate.BottomWidthAtColumn = BF + 2 * flangePlate.BottomWrapAroundWidth;

				if (flangePlate.BottomWrapAroundWidth <= 0.75 * ConstNum.ONE_INCH)
					flangePlate.HSSSideWallWeldLength = column.Shape.d / numberOfActiveMoments;
				else
					flangePlate.HSSSideWallWeldLength = (column.Shape.Ht - 3 * column.Shape.tf) / numberOfActiveMoments;
				
				flangePlate.DiaphragmA = flangePlate.HSSSideWallWeldLength + 1.5 * column.Shape.tf;
				flangePlate.DiaphragmC = (flangePlate.Bolt.NumberOfRows - 1) * flangePlate.Bolt.SpacingLongDir + flangePlate.Bolt.EdgeDistLongDir;

				flangePlate.TopLength = flangePlate.BottomLength = flangePlate.DiaphragmA + flangePlate.TopWrapAroundWidth + flangePlate.DiaphragmC;

				Reporting.AddLine("Top Plate: " + flangePlate.TopLength + ConstUnit.Length + " X (" + flangePlate.TopWidth + " ~ " + flangePlate.TopWidthAtColumn + ") " + ConstUnit.Length + " X " + flangePlate.TopThickness + ConstUnit.Length);
				Reporting.AddLine("Bottom Plate: " + flangePlate.BottomLength + ConstUnit.Length + " X (" + flangePlate.BottomWidth + " ~ " + flangePlate.BottomWidthAtColumn + ") " + ConstUnit.Length + " X " + flangePlate.TopThickness + ConstUnit.Length);
				Reporting.AddLine("Plate Material: " + flangePlate.Material.Name);

				WidthTicknessRatioT = flangePlate.TopWrapAroundWidth / flangePlate.TopThickness;
				WidthTicknessRatioB = flangePlate.BottomWrapAroundWidth / flangePlate.TopThickness;
				Max_w_t_Ratio = 0.56 * Math.Pow(ConstNum.ELASTICITY / flangePlate.Material.Fy, 0.5);

				if (WidthTicknessRatioT > WidthTicknessRatioB)
				{
					if (WidthTicknessRatioT <= Max_w_t_Ratio)
						Reporting.AddLine("Width / Thick. = " + flangePlate.TopWrapAroundWidth + " / " + flangePlate.TopThickness + " = " + WidthTicknessRatioT + " <= 0.56 * (E / Fy)^0.5 = 0.56 * (" + ConstNum.ELASTICITY + " / " + flangePlate.Material.Fy + ")^0.5 = " + Max_w_t_Ratio + " (OK)");
					else
						Reporting.AddLine("Width / Thick. = " + flangePlate.TopWrapAroundWidth + " / " + flangePlate.TopThickness + " = " + WidthTicknessRatioT + " >> 0.56*(E/Fy)^0.5 = 0.56 * (" + ConstNum.ELASTICITY + " / " + flangePlate.Material.Fy + ")^0.5 = " + Max_w_t_Ratio + " (NG)");
				}
				else
				{
					if (WidthTicknessRatioB <= Max_w_t_Ratio)
						Reporting.AddLine("Width / Thick. = " + flangePlate.BottomWrapAroundWidth + " / " + flangePlate.TopThickness + " = " + WidthTicknessRatioB + " <= 0.56 * (E / Fy)^0.5 = 0.56 * (" + ConstNum.ELASTICITY + " / " + flangePlate.Material.Fy + ")^0.5 = " + Max_w_t_Ratio + " (OK)");
					else
						Reporting.AddLine("Width / Thick. = " + flangePlate.BottomWrapAroundWidth + " / " + flangePlate.TopThickness + " = " + WidthTicknessRatioB + " >> 0.56 * (E / Fy)^0.5 = 0.56 * (" + ConstNum.ELASTICITY + " / " + flangePlate.Material.Fy + ")^0.5 = " + Max_w_t_Ratio + " (NG)");
				}
			}
			else if (!CommonDataStatic.IsFema)
			{
				Reporting.AddLine("Top Plate: " + flangePlate.TopLength + ConstUnit.Length + " X " + flangePlate.TopWidth + ConstUnit.Length + " X " + flangePlate.TopThickness + ConstUnit.Length);
				Reporting.AddLine("Bottom Plate: " + flangePlate.BottomLength + ConstUnit.Length + " X " + flangePlate.BottomWidth + ConstUnit.Length + " X " + flangePlate.TopThickness + ConstUnit.Length);
				Reporting.AddLine("Plate Material: " + flangePlate.Material.Name);
			}
			if (flangePlate.Connection == EConnectionStyle.Bolted)
			{
				flangePlate.Bolt.NumberOfBolts = flangePlate.Bolt.NumberOfRows * flangePlate.Bolt.NumberOfLines;
				Reporting.AddLine("Bolts on Flange: " + flangePlate.Bolt.NumberOfBolts + " Bolts - " + flangePlate.Bolt.BoltName + " in " + flangePlate.Bolt.NumberOfLines + " Lines");
				Reporting.AddLine("Bolt Holes on Plate: " + flangePlate.Bolt.HoleWidth + ConstUnit.Length + "  Lateral X " + flangePlate.Bolt.HoleLength + ConstUnit.Length + "  Longitudinal");
				Reporting.AddLine("Bolt Holes on Flange: " + flangePlate.Bolt.HoleWidth + ConstUnit.Length + "  Lateral X " + flangePlate.Bolt.HoleLength + ConstUnit.Length + "  Longitudinal");

				MiscCalculationsWithReporting.BeamCheck(beam.MemberType);

				Reporting.AddGoToHeader("Check Bolts:");
				//spBeam = MinimumSpacing(((int)DESGEN.TFlangePlateBolts.d), Rload / DESGEN.TFlangePlateBolts.n, DESGEN.Beam.Fu * DESGEN.Beam.tf, DESGEN.Beam.FlangeHoleHoriz.ToDbl(), DESGEN.TFlangePlateBolts.ht);
				//spPlate = MinimumSpacing(((int)DESGEN.TFlangePlateBolts.d), Rload / DESGEN.TFlangePlateBolts.n, DESGEN.TFlangePlate.Fu * DESGEN.TFlangePlate.t, DESGEN.TFlangePlate.HoleHoriz.ToDbl(), DESGEN.TFlangePlateBolts.ht);

				spBeam = MiscCalculationsWithReporting.MinimumSpacing(flangePlate.Bolt.BoltSize, Rload / flangePlate.Bolt.NumberOfBolts, beam.Material.Fu * beam.Shape.tf, flangePlate.Bolt.HoleLength, flangePlate.Bolt.HoleType);
				spPlate = MiscCalculationsWithReporting.MinimumSpacing(flangePlate.Bolt.BoltSize, Rload / flangePlate.Bolt.NumberOfBolts, flangePlate.Material.Fu * flangePlate.TopThickness, flangePlate.Bolt.HoleLength, flangePlate.Bolt.HoleType);
				flangePlate.TopMinSpacing = Math.Max(spBeam, spPlate);

				if (flangePlate.Bolt.SpacingLongDir >= flangePlate.TopMinSpacing)
					Reporting.AddLine("Spacing (s) = " + flangePlate.Bolt.SpacingLongDir + " >= Minimum Spacing = " + flangePlate.TopMinSpacing + " " + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Spacing (s) = " + flangePlate.Bolt.SpacingLongDir + " << Minimum Spacing = " + flangePlate.TopMinSpacing + " " + ConstUnit.Length + " (NG)");

				evmin = MiscCalculationsWithReporting.MinimumEdgeDist(flangePlate.Bolt.BoltSize, Rload / flangePlate.Bolt.NumberOfBolts, flangePlate.Material.Fu * flangePlate.TopThickness, flangePlate.Bolt.MinEdgeSheared, (int)flangePlate.Bolt.HoleLength, flangePlate.Bolt.HoleType);
				Reporting.AddLine("Edge Distance on Plate Parallel to Beam Axis (el):");
				if (flangePlate.Bolt.EdgeDistLongDir >= evmin)
					Reporting.AddLine("= " + flangePlate.Bolt.EdgeDistLongDir + " >= " + evmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + flangePlate.Bolt.EdgeDistLongDir + " << " + evmin + ConstUnit.Length + " (NG)");

				ehmin = flangePlate.Bolt.MinEdgeSheared;

				flangePlate.Bolt.EdgeDistTransvDir = (flangePlate.TopWidth - beam.GageOnFlange) / 2.0; // Line added as a result of Bug 167
				Reporting.AddLine("Edge Distance on Plate Transverse to Beam (et):");
				if (flangePlate.Bolt.EdgeDistTransvDir >= ehmin)
					Reporting.AddLine("= " + flangePlate.Bolt.EdgeDistTransvDir + " >= " + ehmin + ConstUnit.Length + " (OK)"); // ((momentFlange.TopWidth - beam.GageOnFlange) / 2)
				else
					Reporting.AddLine("= " + flangePlate.Bolt.EdgeDistTransvDir + " << " + ehmin + ConstUnit.Length + " (NG)"); // ((momentFlange.TopWidth - beam.GageOnFlange) / 2)

				// Check edge distance on Beam
				evmin = MiscCalculationsWithReporting.MinimumEdgeDist(flangePlate.Bolt.BoltSize, Rload / flangePlate.Bolt.NumberOfBolts, beam.Material.Fu * beam.Shape.tf, flangePlate.Bolt.MinEdgeSheared, (int)flangePlate.Bolt.HoleLength, flangePlate.Bolt.HoleType);
				Reporting.AddLine("Edge Distance on Beam Parallel to Beam Axis (el):");
				if (beam.WinConnect.Beam.BoltEdgeDistanceOnFlange >= evmin)
					Reporting.AddLine("= " + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " >= " + evmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " << " + evmin + ConstUnit.Length + " (NG)");

				ehmin = flangePlate.Bolt.MinEdgeRolled;
				Reporting.AddLine("Edge Distance Transverse to Beam (et):");
				BeamTrEdge = (beam.Shape.bf - (beam.GageOnFlange + 2 * flangePlate.TransvBoltSpacingOut)) / 2;
				if (BeamTrEdge >= ehmin)
					Reporting.AddLine("= " + BeamTrEdge + " >= " + ehmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + BeamTrEdge + " << " + ehmin + ConstUnit.Length + " (NG)");

				Cap = flangePlate.Bolt.NumberOfBolts * flangePlate.Bolt.BoltStrength;

				Reporting.AddLine(string.Empty);
				capacityText = ConstString.DES_OR_ALLOWABLE + " Shear Strength of Bolts";

				if (Cap >= Ff)
					Reporting.AddCapacityLine(capacityText + " = " + ConstString.PHI + " n * Fv = " + flangePlate.Bolt.NumberOfBolts + " * " + flangePlate.Bolt.BoltStrength + " =  " + Cap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Cap, capacityText, memberType);
				else
					Reporting.AddCapacityLine(capacityText + " = " + ConstString.PHI + " n * Fv = " + flangePlate.Bolt.NumberOfBolts + " * " + flangePlate.Bolt.BoltStrength + " =  " + Cap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Cap, capacityText, memberType);

				Reporting.AddGoToHeader("Bolt Bearing on Plate:");
				Fbe = CommonCalculations.EdgeBearing(flangePlate.Bolt.EdgeDistLongDir, (flangePlate.Bolt.HoleLength), flangePlate.Bolt.BoltSize, flangePlate.Material.Fu, flangePlate.Bolt.HoleType, true);
				si = flangePlate.Bolt.SpacingLongDir;
				Fbs = CommonCalculations.SpacingBearing(flangePlate.Bolt.SpacingLongDir, flangePlate.Bolt.HoleLength, flangePlate.Bolt.BoltSize, flangePlate.Bolt.HoleType, flangePlate.Material.Fu, true);
				BearingCap = flangePlate.Bolt.NumberOfLines * (Fbe + Fbs * (flangePlate.Bolt.NumberOfRows - 1)) * flangePlate.TopThickness;
				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = Nt * (Fbe + Fbs * (Nl - 1)) * t");
				Reporting.AddLine("= " + flangePlate.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + flangePlate.Bolt.NumberOfRows + " - 1)) * " + flangePlate.TopThickness);

				capacityText = "Bolt Bearing on Plate";

				if (BearingCap >= Ff)
					Reporting.AddCapacityLine("= " + BearingCap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / BearingCap, capacityText, memberType);
				else
					Reporting.AddCapacityLine("= " + BearingCap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / BearingCap, capacityText, memberType);

				Reporting.AddHeader("Bolt Bearing on Flange:");
				Fbe = CommonCalculations.EdgeBearing(beam.WinConnect.Beam.BoltEdgeDistanceOnFlange, (flangePlate.Bolt.HoleLength), flangePlate.Bolt.BoltSize, beam.Material.Fu, flangePlate.Bolt.HoleType, true);
				si = flangePlate.Bolt.SpacingLongDir;
				Fbs = CommonCalculations.SpacingBearing(flangePlate.Bolt.SpacingLongDir, flangePlate.Bolt.HoleLength, flangePlate.Bolt.BoltSize, flangePlate.Bolt.HoleType, beam.Material.Fu, true);
				BearingCap = flangePlate.Bolt.NumberOfLines * (Fbe + Fbs * (flangePlate.Bolt.NumberOfRows - 1)) * beam.Shape.tf;
				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = Nt * (Fbe + Fbs * (Nl - 1)) * t");
				Reporting.AddLine("=" + flangePlate.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + flangePlate.Bolt.NumberOfRows + " - 1)) * " + beam.Shape.tf);

				capacityText = "Bolt Bearing on Flange";

				if (BearingCap >= Ff)
					Reporting.AddCapacityLine("= " + BearingCap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / BearingCap, capacityText, memberType);
				else
					Reporting.AddCapacityLine("= " + BearingCap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / BearingCap, capacityText, memberType);

				if (CommonDataStatic.IsFema)
					tgrosswidth = flangePlate.TopWidth;

				if (!CommonDataStatic.IsFema)
				{
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
					{
						if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
							BF = column.Shape.d;
						else
							BF = column.Shape.bf;
						Ws = NumberFun.Round(flangePlate.TopWidth / 2, 2);

						bAtBoltHole = Math.Min(flangePlate.TopWidthAtColumn, flangePlate.TopWidth + (flangePlate.TopWidthAtColumn - flangePlate.TopWidth) / (flangePlate.TopLength - flangePlate.TopWrapAroundWidth) * (flangePlate.TopLength - beam.EndSetback - beam.WinConnect.Beam.BoltEdgeDistanceOnFlange));
						if (CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn && CommonDataStatic.ColumnStiffener.TopOfBeamToColumn <= 0.75 * ConstNum.ONE_INCH)
						{
							tgrosswidth = bAtBoltHole;
							flangePlate.TopWidthAtColumn = BF + 2 * flangePlate.TopWrapAroundWidth;
						}
						else
							tgrosswidth = Math.Min(2 * flangePlate.TopWrapAroundWidth, bAtBoltHole);

						bgrosswidth = 2 * flangePlate.BottomWrapAroundWidth;
					}
					else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
					{
						flangePlate.TopWidthAtColumn = flangePlate.TopWidth;
						tgrosswidth = flangePlate.TopWidth;
						Ws = flangePlate.TopWidth;
						if (!flangePlate.TopWrapAroundWidth_User || flangePlate.TopWrapAroundWidth == 0)
							flangePlate.TopWrapAroundWidth = Ws;
						bgrosswidth = flangePlate.BottomWidth;
						bAtBoltHole = flangePlate.TopWidth;
					}
					else
					{
						flangePlate.TopWidthAtColumn = flangePlate.TopWidth;
						tgrosswidth = flangePlate.TopWidth;
						Ws = flangePlate.TopWidth / 2;
						if (!flangePlate.TopWrapAroundWidth_User || flangePlate.TopWrapAroundWidth == 0)
							flangePlate.TopWrapAroundWidth = Ws;
						bgrosswidth = flangePlate.BottomWidth;
						bAtBoltHole = flangePlate.TopWidth;
					}

					Reporting.AddGoToHeader("Plate Tension " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
					Reporting.AddHeader("Tension Yielding:");
					Tg = tgrosswidth * flangePlate.TopThickness * ConstNum.FIOMEGA0_9N * flangePlate.Material.Fy;
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * b * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + flangePlate.Material.Fy + " * " + tgrosswidth + " * " + flangePlate.TopThickness);

					capacityText = "Plate Tension Yielding";

					if (Tg >= Ff)
						Reporting.AddCapacityLine("= " + Tg + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tg, capacityText, memberType);
					else
						Reporting.AddCapacityLine("= " + Tg + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tg, capacityText, memberType);

					bn1 = flangePlate.TopWidthAtColumn - Math.Max(0.15F * flangePlate.TopWidthAtColumn, flangePlate.Bolt.NumberOfLines * (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH));
					if (!(CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn && CommonDataStatic.ColumnStiffener.TopOfBeamToColumn <= 0.75 * ConstNum.ONE_INCH))
					{
						bn2 = 2 * 0.85 * flangePlate.TopWidthAtColumn;
						bn = Math.Min(bn1, bn2);
					}
					else
						bn = bn1;

					Reporting.AddHeader("Tension Rupture:");
					Reporting.AddHeader("Effective Net Width:");

					if (!(CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn && CommonDataStatic.ColumnStiffener.TopOfBeamToColumn <= 0.75 * ConstNum.ONE_INCH))
						Reporting.AddLine("bn1 = b - Max(0.15 * b; nT * (dh + " + ConstNum.SIXTEENTH_INCH + "))");
					else
						Reporting.AddLine("bn = b - Max(0.15 * b; nT * (dh + " + ConstNum.SIXTEENTH_INCH + "))");

					Reporting.AddLine("= " + bAtBoltHole + " - Max(0.15 * " + bAtBoltHole + "; " + flangePlate.Bolt.NumberOfLines + " * (" + flangePlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) = " + bn1 + ConstUnit.Length);
					if (!(CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn && CommonDataStatic.ColumnStiffener.TopOfBeamToColumn <= 0.75 * ConstNum.ONE_INCH))
					{
						bn2 = 0.85 * 2 * flangePlate.TopWrapAroundWidth;
						bn = Math.Min(bn1, bn2);
						Reporting.AddLine("bn2 = 2 * 0.85 * Ws = 2 * 0.85 * " + flangePlate.TopWrapAroundWidth + " = " + bn2 + ConstUnit.Length);
						Reporting.AddLine("bn = Min(bn1, bn2) =  Min(" + bn1 + ", " + bn2 + ") = " + bn + ConstUnit.Length);
					}
					Tn = bn * flangePlate.TopThickness * ConstNum.FIOMEGA0_75N * flangePlate.Material.Fu;
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Fu * bn * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + flangePlate.Material.Fu + " * " + bn + " * " + flangePlate.TopThickness);

					capacityText = "Tension Rupture Effective Net Width";

					if (Tn >= Ff)
						Reporting.AddCapacityLine("= " + Tn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tn, capacityText, memberType);
					else
						Reporting.AddCapacityLine("= " + Tn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tn, capacityText, memberType);
				}

				Reporting.AddHeader("Block shear rupture of the Plate:");
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
				{
					if (flangePlate.Bolt.NumberOfLines == 4)
						edgedist = (flangePlate.TopWidthAtColumn - (beam.GageOnFlange + 2 * flangePlate.TransvBoltSpacingOut)) / 2;
					else
						edgedist = (flangePlate.TopWidthAtColumn - beam.GageOnFlange) / 2;
				}
				else
					edgedist = (tgrosswidth - beam.GageOnFlange) / 2;

				if (flangePlate.Bolt.NumberOfLines == 4)
				{
					AgTension = (Math.Min(beam.GageOnFlange, 2 * edgedist) + 2 * flangePlate.TransvBoltSpacingOut) * flangePlate.TopThickness;
					Reporting.AddLine("Agt = (Min(g1, 2 * e) + 2 * g2) * t");
					Reporting.AddLine("= (" + Math.Min(beam.GageOnFlange, 2 * edgedist) + " + 2 * " + flangePlate.TransvBoltSpacingOut + ") * " + flangePlate.TopThickness);
					Reporting.AddLine("= " + AgTension + ConstUnit.Area);

					AnTension = AgTension - 3 * (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * flangePlate.TopThickness;
					Reporting.AddLine("Ant = Agt - (n - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("= " + AgTension + " - 3 * (" + (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + flangePlate.TopThickness);
					Reporting.AddLine("= " + AnTension + ConstUnit.Area);
				}
				else
				{
					AgTension = Math.Min(beam.GageOnFlange, 2 * edgedist) * flangePlate.TopThickness;
					Reporting.AddLine("Agt = Min(g, 2 * e) * t = " + Math.Min(beam.GageOnFlange, 2 * edgedist) + " * " + flangePlate.TopThickness);
					Reporting.AddLine("= " + AgTension + ConstUnit.Area);

					AnTension = AgTension - (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * flangePlate.TopThickness;
					Reporting.AddLine("Ant = Agt - (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("= " + AgTension + " - (" + (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + flangePlate.TopThickness);
					Reporting.AddLine("= " + AnTension + ConstUnit.Area);
				}
				AgShear = 2 * ((flangePlate.Bolt.NumberOfRows - 1) * flangePlate.Bolt.SpacingLongDir + flangePlate.Bolt.EdgeDistLongDir) * flangePlate.TopThickness;
				Reporting.AddLine("Agv = 2 * ((nl - 1) * s + Le) * t");
				Reporting.AddLine("= 2 * ((" + flangePlate.Bolt.NumberOfRows + " - 1) * " + flangePlate.Bolt.SpacingLongDir + " + " + flangePlate.Bolt.EdgeDistLongDir + ") * " + flangePlate.TopThickness);
				Reporting.AddLine("= " + AgShear + ConstUnit.Area);

				AnShear = AgShear - 2 * (flangePlate.Bolt.NumberOfRows - 0.5) * (flangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * flangePlate.TopThickness;
				Reporting.AddLine("Anv = Agv - 2 * (nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("=" + AgShear + " - 2 * (" + flangePlate.Bolt.NumberOfRows + " - 0.5) * (" + (flangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) + ")*" + flangePlate.TopThickness);
				Reporting.AddLine("= " + AnShear + ConstUnit.Area);

				FiRn = MiscCalculationsWithReporting.BlockShearNew(flangePlate.Material.Fu, AnShear, 1, AnTension, AgShear, flangePlate.Material.Fy, 0, Ff, true);

				Reporting.AddHeader("Block shear rupture of the Beam Flange:");

				AgTension = (beam.Shape.bf - beam.GageOnFlange) * beam.Shape.tf;
				Reporting.AddLine("Agt = (bf - g) * t = (" + beam.Shape.bf + " - " + beam.GageOnFlange + ")* " + beam.Shape.tf + " ");
				Reporting.AddLine("= " + AgTension + ConstUnit.Area);

				AnTension = AgTension - (flangePlate.Bolt.NumberOfLines - 1) * (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * beam.Shape.tf;
				Reporting.AddLine("Ant = Agt - (nt - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("= " + AgTension + " - (" + flangePlate.Bolt.NumberOfLines + " - 1) * (" + (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + beam.Shape.tf);
				Reporting.AddLine("= " + AnTension + ConstUnit.Area);

				AgShear = 2 * ((flangePlate.Bolt.NumberOfRows - 1) * flangePlate.Bolt.SpacingLongDir + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange) * beam.Shape.tf;
				Reporting.AddLine("Agv = 2 * ((nl - 1) * s + ef) * t");
				Reporting.AddLine("= 2 * ((" + flangePlate.Bolt.NumberOfRows + " - 1) * " + flangePlate.Bolt.SpacingLongDir + " + " + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + ") * " + beam.Shape.tf);
				Reporting.AddLine("= " + AgShear + ConstUnit.Area);

				AnShear = AgShear - 2 * (flangePlate.Bolt.NumberOfRows - 0.5) * (flangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * beam.Shape.tf;
				Reporting.AddLine("Anv = Agv - 2 * (nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("=" + AgShear + " - 2 * (" + flangePlate.Bolt.NumberOfRows + " - 0.5) * (" + (flangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) + ") * " + beam.Shape.tf);
				Reporting.AddLine("= " + AnShear + ConstUnit.Area);

				FiRn = MiscCalculationsWithReporting.BlockShearNew(beam.Material.Fu, AnShear, 1, AnTension, AgShear, beam.Material.Fy, 0, Ff, true);

				if (CommonDataStatic.IsFema)
					return;

				Reporting.AddHeader("Bottom Plate " + ConstString.DES_OR_ALLOWABLE + " Compressive Strength:");
				sqr12 = Math.Sqrt(12);
				k = 0.65;
				beam.WinConnect.Fema.L = (beam.EndSetback + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
				Reporting.AddLine("Unbraced Length (L) = c + ef = " + beam.EndSetback + " + " + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
				Reporting.AddLine("Effective Length Factor, K = 0.65");
				kL_r = (beam.WinConnect.Fema.L * k * sqr12 / flangePlate.TopThickness);

				Reporting.AddLine("KL / r = k * L / (t / 3.464) = " + k + " * " + beam.WinConnect.Fema.L + " / (" + flangePlate.TopThickness + " / 3.464) = " + kL_r);

				Fcr = CommonCalculations.BucklingStress(kL_r, flangePlate.Material.Fy, true);

				FiRn = ConstNum.FIOMEGA0_9N * Fcr * bgrosswidth * flangePlate.TopThickness;

				capacityText = "Bottom Plate " + ConstString.DES_OR_ALLOWABLE + " Compressive Strength";

				if (FiRn >= Ff)
					Reporting.AddCapacityLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + bgrosswidth + " * " + flangePlate.TopThickness + " = " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / FiRn, capacityText, memberType);
				else
					Reporting.AddCapacityLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + bgrosswidth + " * " + flangePlate.TopThickness + " = " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / FiRn, capacityText, memberType);
			}
			else
			{
				Reporting.AddHeader("Top Plate Tension Strength");
				if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToColumnFlange)
				{
					Reporting.AddHeader("Top Plate Tension Strength:");
					Tg = ConstNum.FIOMEGA0_9N * flangePlate.TopWidth * flangePlate.TopThickness * flangePlate.Material.Fy;
					Reporting.AddHeader("Tension Yielding:");

					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * b * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + flangePlate.Material.Fy + " * " + flangePlate.TopWidth + " * " + flangePlate.TopThickness);
					if (Tg >= Ff)
						Reporting.AddLine("= " + Tg + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Tg + " << " + Ff + ConstUnit.Force + " (NG)");

					Tn = ConstNum.FIOMEGA0_75N * 0.85F * flangePlate.TopWidth * flangePlate.TopThickness * flangePlate.Material.Fu;
					Reporting.AddHeader("Tension Rupture:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.85 * Fu * b * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.85 * " + flangePlate.Material.Fu + " * " + flangePlate.TopWidth + " * " + flangePlate.TopThickness);

					capacityText = "Top Plate " + ConstString.DES_OR_ALLOWABLE + " Compressive Strength";

					if (Tn >= Ff)
						Reporting.AddCapacityLine("= " + Tn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tn, capacityText, memberType);
					else
						Reporting.AddCapacityLine("= " + Tn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tn, capacityText, memberType);

					w = CommonCalculations.MinimumWeld(flangePlate.TopThickness, beam.Shape.tf);
					if (flangePlate.TopThickness <= ConstNum.QUARTER_INCH)
						w1 = flangePlate.TopThickness;
					else
						w1 = flangePlate.TopThickness - ConstNum.SIXTEENTH_INCH;
					if (w > w1)
						w = w1;

					Reporting.AddHeader("Top Plate to Beam Weld:");
					Reporting.AddLine("Plate Thickness = " + flangePlate.TopThickness + ConstUnit.Length + " Beam Flange Thickness = " + beam.Shape.tf + ConstUnit.Length);
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(w) + ConstUnit.Length + " Maximum Weld Size = " + CommonCalculations.WeldSize(w1) + ConstUnit.Length);
					if (flangePlate.TopPlateToBeamWeldSize >= w && flangePlate.TopPlateToBeamWeldSize <= w1)
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(flangePlate.TopPlateToBeamWeldSize) + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(flangePlate.TopPlateToBeamWeldSize) + ConstUnit.Length + " (NG)");

					Reporting.AddLine("Weld " + ConstString.DES_OR_ALLOWABLE + " Strength:");
					if (flangePlate.ConnectionType == EFlangeConnectionType.FullyRestrained)
					{
						CommonDataStatic.WeldBetaL = flangePlate.TopLength - beam.EndSetback - flangePlate.TopPlateToBeamWeldSize;
						Reporting.AddLine("Welded length of PL, Lw = " + CommonDataStatic.WeldBetaL + ConstUnit.Length);
					}
					else
					{
						CommonDataStatic.WeldBetaL = flangePlate.TopLength - 1.5 * flangePlate.TopWidth;
						Reporting.AddLine("Welded length of PL, Lw = " + CommonDataStatic.WeldBetaL + ConstUnit.Length);
					}
					BetaN = CommonCalculations.WeldBetaFactor(CommonDataStatic.WeldBetaL, flangePlate.TopPlateToBeamWeldSize);
					if (BetaN < 1)
					{
						FiRn = ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.TopPlateToBeamWeldSize * Math.Max(2 * CommonDataStatic.WeldBetaL * BetaN + flangePlate.TopWidth, 1.7F * CommonDataStatic.WeldBetaL * BetaN + 1.5 * flangePlate.TopWidth);
						Reporting.AddLine(ConstNum.BETAS + " = " + BetaN);
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.4242 * Fexx * w * Max((2 * " + ConstNum.BETAS + " Lw + b), (1.7*" + ConstNum.BETAS + " Lw + 1.5*b))");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4242 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + flangePlate.TopPlateToBeamWeldSize + " * Math.Max(2 * " + (BetaN * CommonDataStatic.WeldBetaL) + " + " + flangePlate.TopWidth + "; 1.7 * " + (BetaN * CommonDataStatic.WeldBetaL) + " + 1.5 * " + flangePlate.TopWidth + ")");
					}
					else
					{
						FiRn = ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.TopPlateToBeamWeldSize * Math.Max(2 * CommonDataStatic.WeldBetaL + flangePlate.TopWidth, 1.7F * CommonDataStatic.WeldBetaL + 1.5 * flangePlate.TopWidth);
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + "* 0.4242 * Fexx * w * Max((2 * Lw + b); (1.7 * Lw + 1.5 * b))");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4242 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + flangePlate.TopPlateToBeamWeldSize + " * Math.Max(2 * " + CommonDataStatic.WeldBetaL + " + " + flangePlate.TopWidth + "; 1.7 * " + CommonDataStatic.WeldBetaL + " + 1.5 * " + flangePlate.TopWidth + ")");
					}
					if (FiRn >= Ff)
						Reporting.AddLine("= " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)");
				}
				else
				{
					Reporting.AddHeader("Top Plate to Beam Flange Weld:");
					FiRn = ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.TopPlateToBeamWeldSize * Math.Max(2 * flangePlate.TopLength + flangePlate.TopWidth, 1.7F * flangePlate.TopLength + 1.5 * flangePlate.TopWidth);
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.4242 * Fexx * w * Max((2 * Lw + b), (1.7 * Lw + 1.5 * b))");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4242 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + flangePlate.TopPlateToBeamWeldSize + " * Math.Max(2 * " + flangePlate.TopLength + " + " + flangePlate.TopWidth + "; 1.7 * " + flangePlate.TopLength + " + 1.5 * " + flangePlate.TopWidth + ")");
				}
				if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToColumnFlange)
				{
					Reporting.AddHeader("Bottom Plate Tension Strength:");
					Tg = ConstNum.FIOMEGA0_9N * flangePlate.BottomWidth * flangePlate.BottomThickness * flangePlate.Material.Fy;
					Reporting.AddHeader("Tension Yielding:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * b * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + flangePlate.Material.Fy + " * " + flangePlate.BottomWidth + " * " + flangePlate.TopThickness);
					if (Tg >= Ff)
						Reporting.AddLine("= " + Tg + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Tg + " << " + Ff + ConstUnit.Force + " (NG)");

					Tn = ConstNum.FIOMEGA0_75N * 0.85F * flangePlate.BottomWidth * flangePlate.BottomThickness * flangePlate.Material.Fu;
					Reporting.AddHeader("Tension Rupture:");
					if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToColumnFlange)
					{
						wLength = flangePlate.BottomLength - beam.EndSetback;
						tmpCaseArg = wLength / beam.Shape.bf;
						if (tmpCaseArg >= 1.5)
							U = 0.85;
						else if (tmpCaseArg >= 1)
							U = 0.75;
						else
							U = 0;
					}
					else
						U = 1;

					if (U > 0)
					{
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * U * Fu * b * t");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + U + " * " + flangePlate.Material.Fu + " * " + flangePlate.BottomWidth + " * " + flangePlate.TopThickness);

						capacityText = "Bottom Plate " + ConstString.DES_OR_ALLOWABLE + " Compressive Strength";

						if (Tn >= Ff)
							Reporting.AddCapacityLine("= " + Tn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tn, capacityText, memberType);
						else
							Reporting.AddCapacityLine("= " + Tn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tn, capacityText, memberType);
					}
					else
						Reporting.AddLine("Welded length of the plate << Beam flange width (NG)");

					Reporting.AddHeader("Bottom Plate " + ConstString.DES_OR_ALLOWABLE + " Compressive Strength:");
					sqr12 = Math.Sqrt(12);
					k = 0.65;
					if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToColumnFlange)
					{
						Reporting.AddLine("Unbraced Lengt (L) = c = " + beam.EndSetback + ConstUnit.Length);
						UnbracedLength = beam.EndSetback;
					}
					else
					{
						UnbracedLength = flangePlate.BottomLength - flangePlate.BottomLength;
						Reporting.AddLine("Unbraced Length (L) = " + UnbracedLength + ConstUnit.Length);
					}
					Reporting.AddLine("Effective Length Factor (K) = 0.65");
					kL_r = (beam.EndSetback * k * sqr12 / flangePlate.TopThickness);

					Reporting.AddLine("KL / r = k * L / (t / 3.464) = " + k + " * " + beam.EndSetback + " / (" + flangePlate.TopThickness + "/3.464) = " + kL_r);

					Fcr = CommonCalculations.BucklingStress(kL_r, flangePlate.Material.Fy, true);

					FiRn = ConstNum.FIOMEGA0_9N * Fcr * flangePlate.BottomWidth * flangePlate.BottomLength;
					FiRn = ((int)Math.Floor(100 * FiRn)) / 100.0;
					Ff = ((int)Math.Floor(100 * Ff)) / 100.0;
					if (FiRn >= Ff)
						Reporting.AddLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + flangePlate.BottomWidth + " * " + flangePlate.TopThickness + " = " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + flangePlate.BottomWidth + " * " + flangePlate.TopThickness + " = " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)");

					w = CommonCalculations.MinimumWeld(flangePlate.TopThickness, beam.Shape.tf);
					if (flangePlate.TopThickness <= ConstNum.QUARTER_INCH)
						w1 = ((int)Math.Floor(16 * beam.Shape.tf)) / 16.0;
					else
						w1 = beam.Shape.tf - ConstNum.SIXTEENTH_INCH;
					if (w > w1)
						w = w1;

					Reporting.AddHeader("Bottom Plate to Beam Weld:");
					Reporting.AddLine("Plate Thickness = " + flangePlate.TopThickness + ConstUnit.Length + " Beam Flange Thickness = " + beam.Shape.tf + ConstUnit.Length);
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(w) + ConstUnit.Length + " Maximum Weld Size = " + CommonCalculations.WeldSize(w1) + ConstUnit.Length);
					if (flangePlate.BottomPlateToBeamWeldSize >= w && flangePlate.BottomPlateToBeamWeldSize <= w1)
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(flangePlate.BottomPlateToBeamWeldSize) + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(flangePlate.BottomPlateToBeamWeldSize) + ConstUnit.Length + " (NG)");
				}

				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
				{
					Reporting.AddHeader("Bottom Plate to Beam Flange Weld");
					FiRn = ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.BottomPlateToBeamWeldSize * 2 * flangePlate.BottomLength;
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.4242 * Fexx * w * 2 * Lw");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4242 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + flangePlate.BottomPlateToBeamWeldSize + " * 2 * " + flangePlate.BottomLength);
					if (FiRn >= Ff)
						Reporting.AddLine("= " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Top Plate Tension Strength:");
					WeakestWidth = Math.Min(flangePlate.TopWidth, 2 * flangePlate.TopWrapAroundWidth);
					Tg = ConstNum.FIOMEGA0_9N * WeakestWidth * flangePlate.TopThickness * flangePlate.Material.Fy;
					Reporting.AddHeader("Tension Yielding:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * b * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + flangePlate.Material.Fy + " * " + WeakestWidth + " * " + flangePlate.TopThickness);

					capacityText = "Top Plate Tension Strength";

					if (Tg >= Ff)
						Reporting.AddCapacityLine("= " + Tg + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tg, capacityText, memberType);
					else
						Reporting.AddCapacityLine("= " + Tg + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tg, capacityText, memberType);

					Tn = ConstNum.FIOMEGA0_75N * WeakestWidth * flangePlate.TopThickness * flangePlate.Material.Fu;
					Reporting.AddHeader("Tension Rupture:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Fu * b * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + flangePlate.Material.Fu + " * " + WeakestWidth + " * " + flangePlate.TopThickness);

					if (Tn >= Ff)
						Reporting.AddCapacityLine("= " + Tn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tn, capacityText, memberType);
					else
						Reporting.AddCapacityLine("= " + Tn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tn, capacityText, memberType);

					FiRn = ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.BottomPlateToBeamWeldSize * 2 * flangePlate.BottomLength;
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.4242 * Fexx * w * 2 * Lw");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4242 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + flangePlate.BottomPlateToBeamWeldSize + " * 2 * " + flangePlate.BottomLength);

					Reporting.AddLine("Bottom Plate " + ConstString.DES_OR_ALLOWABLE + " Buckling Strength:");
					sqr12 = Math.Sqrt(12);
					k = 0.65;

					UnbracedLength = flangePlate.BottomLength - flangePlate.BottomLength;
					Reporting.AddLine("Unbraced Length, L = " + UnbracedLength + ConstUnit.Length);

					Reporting.AddLine("Effective Length Factor, K = 0.65");
					kL_r = (beam.EndSetback * k * sqr12 / flangePlate.TopThickness);
					Reporting.AddLine("KL / r = k * L / (t / 3.464) = " + k + " * " + UnbracedLength + " / (" + flangePlate.TopThickness + " / 3.464) = " + kL_r);

					Fcr = CommonCalculations.BucklingStress(kL_r, flangePlate.Material.Fy, true);

					FiRn = ConstNum.FIOMEGA0_9N * Fcr * flangePlate.BottomWidth * flangePlate.BottomThickness;
					FiRn = ((int)Math.Floor(100 * FiRn)) / 100.0;
					Ff = ((int)Math.Floor(100 * Ff)) / 100.0;

					if (FiRn >= Ff)
						Reporting.AddCapacityLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + flangePlate.BottomWidth + " * " + flangePlate.TopThickness + " = " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / FiRn, capacityText, memberType);
					else
						Reporting.AddCapacityLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + flangePlate.BottomWidth + " * " + flangePlate.TopThickness + " = " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / FiRn, capacityText, memberType);
				}
				else
				{
					if (flangePlate.BottomPlateToBeamWeldSize > 0)
						weldlength = Math.Max(Ff / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.BottomPlateToBeamWeldSize), 2 * beam.Shape.bf);
					else
						weldlength = 2 * beam.Shape.bf;
					Reporting.AddLine("Required Weld Length, Lw = Max(Ff / (" + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx * w), 2 * Bf)");
					Reporting.AddLine("= Max(" + Ff + " / (" + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + flangePlate.BottomPlateToBeamWeldSize + "), 2 * " + beam.Shape.bf + "");
					Reporting.AddLine("= " + weldlength + ConstUnit.Length);

					a = weldlength / 2 + beam.EndSetback + flangePlate.BottomPlateToBeamWeldSize;
					if (flangePlate.BottomLength >= a)
						Reporting.AddLine("Plate Length = " + flangePlate.BottomLength + " >= Lw / 2 + c + w = " + weldlength + " / 2 + " + beam.EndSetback + " + " + flangePlate.BottomPlateToBeamWeldSize + " = " + a + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Plate Length = " + flangePlate.BottomLength + " << Lw / 2 + c + w = " + weldlength + " / 2 + " + beam.EndSetback + " + " + flangePlate.BottomPlateToBeamWeldSize + " = " + a + ConstUnit.Length + " (NG)");

					CommonDataStatic.WeldBetaL = weldlength / 2;
					BetaN = CommonCalculations.WeldBetaFactor(CommonDataStatic.WeldBetaL, flangePlate.BottomPlateToBeamWeldSize);
					if (BetaN < 1)
					{
						FiRn = ConstNum.FIOMEGA0_75N * BetaN * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.BottomPlateToBeamWeldSize * 2 * CommonDataStatic.WeldBetaL;
						Reporting.AddLine(ConstNum.BETAS + " = " + BetaN);
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + ConstNum.BETAS + " * 0.4242 * Fexx * w * 2 * Lw ");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + BetaN + " * 0.4242 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + flangePlate.BottomPlateToBeamWeldSize + " * 2 * " + CommonDataStatic.WeldBetaL);
					}
					else
					{
						FiRn = ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.BottomPlateToBeamWeldSize * 2 * CommonDataStatic.WeldBetaL;
						Reporting.AddLine(ConstNum.BETAS + " = " + BetaN);
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.4242 * Fexx * w * 2 * Lw ");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4242 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + flangePlate.BottomPlateToBeamWeldSize + " * 2 * " + CommonDataStatic.WeldBetaL);
					}

					capacityText = "Top Plate Tension Strength";

					if (FiRn >= Ff)
						Reporting.AddCapacityLine("= " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / FiRn, capacityText, memberType);
					else
						Reporting.AddCapacityLine("= " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / FiRn, capacityText, memberType);
				}
			}

			switch (CommonDataStatic.BeamToColumnType)
			{
					// Was BeamToColumnFlange. Changed to match v7.     -RM 01/29/2015
				case EJointConfiguration.BeamToHSSColumn:
					minw = CommonCalculations.MinimumWeld(flangePlate.TopThickness, SupThickness);
					if (rightBeam.IsActive && rightBeam.MomentConnection == EMomentCarriedBy.FlangePlate && leftBeam.IsActive && leftBeam.MomentConnection == EMomentCarriedBy.FlangePlate)
						numberOfActiveMoments = 2;
					else
						numberOfActiveMoments = 1;

					Reporting.AddHeader("Plate-to-Support Weld:");
					Reporting.AddHeader("HSS Side Wall Shear:");
					if (CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn && CommonDataStatic.ColumnStiffener.TopOfBeamToColumn <= 0.75 * ConstNum.ONE_INCH)
					{
						Reporting.AddHeader("At top plate weld:");
						capacity = 2 * ConstNum.FIOMEGA1_0N * 0.6 * column.Material.Fy * flangePlate.TopHSSSideWallWeldLength * column.Shape.tw;
						Reporting.AddLine("Capacity = 2 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * Fy * Lw * t");
						Reporting.AddLine("= 2 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + column.Material.Fy + " * " + flangePlate.TopHSSSideWallWeldLength + " * " + column.Shape.tw);

						capacityText = "Plate to Support Weld for HSS Side Wall Shear";

						if (capacity >= Rload)
							Reporting.AddCapacityLine("= " + capacity + " >= " + Rload + ConstUnit.Force + " (OK)", Rload / capacity, capacityText, memberType);
						else
							Reporting.AddCapacityLine("= " + capacity + " << " + Rload + ConstUnit.Force + " (NG)", Rload / capacity, capacityText, memberType);

						Reporting.AddHeader("Plate Shear at Weld:");
						capacity = 2 * ConstNum.FIOMEGA1_0N * 0.6 * flangePlate.Material.Fy * flangePlate.TopHSSSideWallWeldLength * flangePlate.TopThickness;
						Reporting.AddLine("Capacity = 2 * " + ConstNum.FIOMEGA1_0N + " *0.6 * Fy * Lw* tp");
						Reporting.AddLine("= 2 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + flangePlate.Material.Fy + " * " + flangePlate.TopHSSSideWallWeldLength + " * " + flangePlate.TopThickness);
						if (capacity >= Rload)
							Reporting.AddCapacityLine("= " + capacity + " >= " + Rload + ConstUnit.Force + " (OK)", Rload / capacity, capacityText, memberType);
						else
							Reporting.AddCapacityLine("= " + capacity + " << " + Rload + ConstUnit.Force + " (NG)", Rload / capacity, capacityText, memberType);
					}
					Reporting.AddHeader("At bottom plate weld:");
					capacity = 4 * ConstNum.FIOMEGA1_0N * 0.6 * column.Material.Fy * flangePlate.BottomHSSSideWallWeldLength * column.Shape.tw;
					Reporting.AddLine("Capacity = 4 * " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy * Lw * t");
					Reporting.AddLine("= 4 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + column.Material.Fy + " * " + flangePlate.BottomHSSSideWallWeldLength + " * " + column.Shape.tw);

					capacityText = "Plate to Support Weld for Bottom Plate Weld";

					if (capacity >= Rload)
						Reporting.AddCapacityLine("= " + capacity + " >= " + Rload + ConstUnit.Force + " (OK)", Rload / capacity, capacityText, memberType);
					else
						Reporting.AddCapacityLine("= " + capacity + " << " + Rload + ConstUnit.Force + " (NG)", Rload / capacity, capacityText, memberType);

					Reporting.AddHeader("Plate Shear at Weld:");
					capacity = 2 * ConstNum.FIOMEGA1_0N * 0.6 * flangePlate.Material.Fy * flangePlate.BottomHSSSideWallWeldLength * flangePlate.TopThickness;
					Reporting.AddLine("Capacity = 2 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * Fy * Lw * tp");
					Reporting.AddLine("= 2 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + flangePlate.Material.Fy + " * " + flangePlate.BottomHSSSideWallWeldLength + " * " + flangePlate.TopThickness);

					capacityText = "Plate to Support Weld for Plate Shear at Weld";

					if (capacity >= Rload)
						Reporting.AddCapacityLine("= " + capacity + " >= " + Rload + ConstUnit.Force + " (OK)", Rload / capacity, capacityText, memberType);
					else
						Reporting.AddCapacityLine("= " + capacity + " << " + Rload + ConstUnit.Force + " (NG)", Rload / capacity, capacityText, memberType);

					if (CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn && CommonDataStatic.ColumnStiffener.TopOfBeamToColumn <= 0.75 * ConstNum.ONE_INCH)
					{
						Reporting.AddHeader("Top plate weld:");
						RequiredWeldSize = Rload / (2 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.TopHSSSideWallWeldLength);
						Reporting.AddLine("Required Fillet Weld Size = Ff / (2 * " + ConstString.FIOMEGA0_75 + " * 0.4242 * E * Lw)");
						Reporting.AddLine("= " + Ff + " / (2 * " + ConstString.FIOMEGA0_75 + " * 0.4242 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + flangePlate.TopHSSSideWallWeldLength + ")");
						fullpenweldcap = 2 * Math.Min(ConstNum.FIOMEGA1_0N * 0.6 * Math.Min(flangePlate.TopThickness * flangePlate.Material.Fy, column.Shape.tf * column.Material.Fy), 0.8 * 0.6 * column.Shape.tf * CommonDataStatic.Preferences.DefaultElectrode.Fexx) * flangePlate.TopHSSSideWallWeldLength;

						if (RequiredWeldSize <= 0.3751 * ConstNum.ONE_INCH)
						{
							if (RequiredWeldSize <= flangePlate.TopPlateToSupportWeldSize)
								Reporting.AddLine("= " + RequiredWeldSize + " <= " + flangePlate.TopPlateToSupportWeldSize + ConstUnit.Length + " (OK)");
							else
								Reporting.AddLine("= " + RequiredWeldSize + " >> " + flangePlate.TopPlateToSupportWeldSize + ConstUnit.Length + " (NG)");
						}
						else
						{
							Reporting.AddHeader("= " + RequiredWeldSize + ConstUnit.Length);
							Reporting.AddHeader("Use Full Penetration Weld.");

							FiRn = 2 * Math.Min(ConstNum.FIOMEGA1_0N * 0.6 * Math.Min(flangePlate.TopThickness * flangePlate.Material.Fy, column.Shape.tf * column.Material.Fy), 0.8 * 0.6 * column.Shape.tf * CommonDataStatic.Preferences.DefaultElectrode.Fexx) * flangePlate.TopHSSSideWallWeldLength;
							Reporting.AddLine(ConstString.PHI + " Rn = 2 * Min[" + ConstNum.FIOMEGA1_0N + " * 0.6 * Min(tp * Fyp, tc * Fyc), 0.8 * 0.6 * tc * E] * Lw");
							Reporting.AddLine("= 2 *  Math.Min(" + ConstNum.FIOMEGA1_0N + " * 0.6 * Math.Min(" + flangePlate.TopThickness + " * " + flangePlate.Material.Fy + ", " + column.Shape.tf + " * " + column.Material.Fy + "), 0.8 * 0.6 * " + column.Shape.tf + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ") * " + flangePlate.TopHSSSideWallWeldLength);

							capacityText = "Plate to Support Weld for Top Plate Weld";

							if (FiRn >= Rload)
								Reporting.AddCapacityLine("= " + FiRn + " >= " + Rload + ConstUnit.Force + " (OK)", Rload / FiRn, capacityText, memberType);
							else
								Reporting.AddCapacityLine("= " + FiRn + " << " + Rload + ConstUnit.Force + " (NG)", Rload / FiRn, capacityText, memberType);
						}
						Reporting.AddHeader("Bottom plate weld:");
					}

					RequiredWeldSize = Rload / (2 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 2 * flangePlate.BottomHSSSideWallWeldLength);
					Reporting.AddHeader("Required Fillet Weld Size = Ff / (2 * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx * 2 * b)");
					Reporting.AddLine("= " + Ff + " / (2 * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * 2* " + flangePlate.BottomHSSSideWallWeldLength + ")");
					fullpenweldcap = 2 * Math.Min(ConstNum.FIOMEGA1_0N * 0.6 * Math.Min(flangePlate.TopThickness * flangePlate.Material.Fy, 2 * column.Shape.tf * column.Material.Fy), 0.8 * 0.6 * flangePlate.TopThickness * CommonDataStatic.Preferences.DefaultElectrode.Fexx) * flangePlate.BottomHSSSideWallWeldLength;
					Reinforcementsize = (Rload - fullpenweldcap) / (2 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.BottomHSSSideWallWeldLength * 2);
					if (RequiredWeldSize <= 0.3751 * ConstNum.ONE_INCH)
					{
						if (RequiredWeldSize <= flangePlate.TopPlateToSupportWeldSize)
							Reporting.AddLine("= " + RequiredWeldSize + " <= " + flangePlate.TopPlateToSupportWeldSize + ConstUnit.Length + "   (OK)");
						else
							Reporting.AddLine("= " + RequiredWeldSize + " >> " + flangePlate.TopPlateToSupportWeldSize + ConstUnit.Length + "   (NG)");
					}
					else
					{
						Reporting.AddHeader("= " + RequiredWeldSize + ConstUnit.Length);
						Reporting.AddHeader("Use Full Penetration Weld.");

						FiRn = 2 * Math.Min(ConstNum.FIOMEGA1_0N * 0.6 * Math.Min(flangePlate.TopThickness * flangePlate.Material.Fy, 2 * column.Shape.tf * column.Material.Fy), 0.8 * 0.6 * flangePlate.TopThickness * CommonDataStatic.Preferences.DefaultElectrode.Fexx) * flangePlate.BottomHSSSideWallWeldLength;
						Reporting.AddLine(ConstString.PHI + " Rn = 2 * Min(" + ConstNum.FIOMEGA1_0N + " * 0.6 * Min(tp * Fyp, 2 * tc * Fyc), 0.8 * 0.6 * tp * Fexx) * Lw");
						Reporting.AddLine("= 2 *  Min(" + ConstNum.FIOMEGA1_0N + " * 0.6 * Math.Min(" + flangePlate.TopThickness + " * " + flangePlate.Material.Fy + "; 2*" + column.Shape.tf + " * " + column.Material.Fy + "); 0.8 * 0.6 * " + flangePlate.TopThickness + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ") * " + flangePlate.BottomHSSSideWallWeldLength);

						capacityText = "Plate to Support Weld for Bottom Plate Weld";

						if (FiRn >= Rload)
							Reporting.AddCapacityLine("= " + FiRn + " >= " + Rload + ConstUnit.Force + " (OK)", Rload / FiRn, capacityText, memberType);
						else
							Reporting.AddCapacityLine("= " + FiRn + " << " + Rload + ConstUnit.Force + " (NG)", Rload / FiRn, capacityText, memberType);
					}
					break;
				default:
					Reporting.AddHeader("Top Plate-to-Support Weld:");

					RequiredWeldSize = Ff / (ConstNum.FIOMEGA0_75N * 1.5 * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min(flangePlate.TopWidth, column.Shape.bf) * 2);
					Reporting.AddLine("Required Fillet Weld Size = Ff / (" + ConstString.FIOMEGA0_75 + " * 1.5 * 0.4242 * Fexx * b * 2)");
					Reporting.AddLine("= " + Ff + " / (" + ConstString.FIOMEGA0_75 + " * 1.5 * 0.4242  * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + Math.Min(flangePlate.TopWidth, column.Shape.bf) + " * 2) ");

					if (RequiredWeldSize <= flangePlate.TopPlateToSupportWeldSize)
						Reporting.AddLine("= " + RequiredWeldSize + ConstUnit.Length + " <= " + flangePlate.TopPlateToSupportWeldSize + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + RequiredWeldSize + ConstUnit.Length + " >> " + flangePlate.TopPlateToSupportWeldSize + ConstUnit.Length + " (NG)");

					fullpenweldcap = flangePlate.TopThickness * ConstNum.FIOMEGA0_75N * flangePlate.Material.Fu * Math.Min(flangePlate.TopWidth, column.Shape.bf);
					Reinforcementsize = (Ff - fullpenweldcap) / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min(flangePlate.TopWidth, column.Shape.bf) * 2);

					if (Reinforcementsize <= 0)
					{
						Reporting.AddHeader("If using Full Penetration Weld: ");
						fullpenweldcap = flangePlate.TopThickness * ConstNum.FIOMEGA0_75N * flangePlate.Material.Fu * Math.Min(flangePlate.TopWidth, column.Shape.bf);
						Reporting.AddLine("Capacity = tp * " + ConstString.FIOMEGA0_75 + " * Fu * Min(PL_Width, BF)");
						Reporting.AddLine("= " + flangePlate.TopThickness + " * " + ConstString.FIOMEGA0_75 + " * " + flangePlate.Material.Fu + " * Min(" + flangePlate.TopWidth + " , " + column.Shape.bf + ")");

						capacityText = "Full Penetration Weld";

						if (fullpenweldcap >= Ff)
							Reporting.AddCapacityLine("= " + fullpenweldcap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / fullpenweldcap, capacityText, memberType);
						else
							Reporting.AddCapacityLine("= " + fullpenweldcap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / fullpenweldcap, capacityText, memberType);
					}

					Reporting.AddHeader("Bottom Plate-to-Support Weld:");
					RequiredWeldSize = Ff / (ConstNum.FIOMEGA0_75N * 1.5 * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min(flangePlate.BottomWidth, column.Shape.bf) * 2);
					Reporting.AddLine("Required Fillet Weld Size = Ff / (" + ConstString.FIOMEGA0_75 + " * 1.5 * 0.4242" + " * Fexx * b * 2)");
					Reporting.AddLine("= " + Ff + " / (" + ConstString.FIOMEGA0_75 + " * 1.5 * 0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + Math.Min(flangePlate.BottomWidth, column.Shape.bf) + " * 2) ");

					if (RequiredWeldSize <= flangePlate.BottomPlateToSupportWeldSize)
						Reporting.AddLine("= " + RequiredWeldSize + ConstUnit.Length + " <= " + flangePlate.BottomPlateToSupportWeldSize + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + RequiredWeldSize + ConstUnit.Length + " >> " + flangePlate.BottomPlateToSupportWeldSize + ConstUnit.Length + " (NG)");
					fullpenweldcap = flangePlate.TopThickness * ConstNum.FIOMEGA0_9N * Math.Min(flangePlate.Material.Fy, column.Material.Fy) * Math.Min(flangePlate.BottomWidth, column.Shape.bf);
					Reinforcementsize = (Ff - fullpenweldcap) / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min(flangePlate.BottomWidth, column.Shape.bf) * 2);

					if (Reinforcementsize <= 0)
					{
						Reporting.AddHeader("If Using Full Penetration Weld:");
						fullpenweldcap = flangePlate.TopThickness * ConstNum.FIOMEGA0_75N * flangePlate.Material.Fu * Math.Min(flangePlate.BottomWidth, column.Shape.bf);
						Reporting.AddLine("Capacity = tp * " + ConstString.FIOMEGA0_75 + " * Fu * Min(PL_Width, BF)");
						Reporting.AddLine("= " + flangePlate.TopThickness + " * " + ConstString.FIOMEGA0_75 + " * " + flangePlate.Material.Fu + " * Min(" + flangePlate.BottomWidth + " , " + column.Shape.bf + ")");

						capacityText = "Full Penetration Weld";

						if (fullpenweldcap >= Ff)
							Reporting.AddCapacityLine("= " + fullpenweldcap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / fullpenweldcap, capacityText, memberType);
						else
							Reporting.AddCapacityLine("= " + fullpenweldcap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / fullpenweldcap, capacityText, memberType);
					}
					if (flangePlate.ConnectionType != EFlangeConnectionType.PartiallyRestrained)
					{
						Reporting.AddLine("Note: Descon does not check the moment versus rotation behavior of the connection.");
						Reporting.AddLine("If your particular application requires this check, you must do it outside the program.");
					}
					break;
			}
		}

		private static void GetBeamGage(EMemberType memberType)
		{
			double Gage1;
			double Gage2;
			double gagemin;
			double gagemax;
			double outgage;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var momentFlange = beam.WinConnect.MomentFlangePlate;

			Gage1 = 2 * (beam.Shape.k1 + momentFlange.Bolt.BoltSize);
			Gage2 = beam.Shape.tw + 2 * (momentFlange.Bolt.BoltSize + ConstNum.HALF_INCH);
			gagemin = Math.Max(Gage2, Gage1);
			gagemax = beam.Shape.bf - 2 * momentFlange.Bolt.MinEdgeRolled;

			if (gagemax < gagemin)
				Reporting.AddLine("Beam flange is not wide enough for " + momentFlange.Bolt.BoltSize + ConstUnit.Length + "  bolts.");

			outgage = NumberFun.Round(((gagemax - beam.GageOnFlange) / 2), 4);
			if (!momentFlange.TransvBoltSpacing_User || momentFlange.TransvBoltSpacingOut == 0)
			{
				if (outgage >= 2.67 * momentFlange.Bolt.BoltSize)
				{
					if (momentFlange.TransvBoltSpacingOut > outgage)
						momentFlange.TransvBoltSpacingOut = outgage;
					if (momentFlange.TransvBoltSpacingOut <= 2.67 * momentFlange.Bolt.BoltSize)
						momentFlange.TransvBoltSpacingOut = Math.Min(outgage, NumberFun.Round(2.67 * momentFlange.Bolt.BoltSize, 4));
				}
				else
				{
					momentFlange.TransvBoltSpacingOut = 0;
					momentFlange.Bolt.NumberOfLines = 2;
				}
			}
		}

		private static void GetRequiredThicknessForBearing(EMemberType memberType, ref double Fbe, ref double si, ref double Fbs, ref double BearingCap, double Rload, ref double tbReq)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];
			var momentFlange = beam.WinConnect.MomentFlangePlate;

			Fbe = CommonCalculations.EdgeBearing(momentFlange.Bolt.EdgeDistLongDir, momentFlange.Bolt.HoleLength, momentFlange.Bolt.BoltSize, momentFlange.Material.Fu, momentFlange.Bolt.HoleType, false);
			si = momentFlange.Bolt.SpacingLongDir;
			Fbs = CommonCalculations.SpacingBearing(momentFlange.Bolt.SpacingLongDir, momentFlange.Bolt.HoleLength, momentFlange.Bolt.BoltSize, momentFlange.Bolt.HoleType, momentFlange.Material.Fu, false);
			BearingCap = momentFlange.Bolt.NumberOfLines * (Fbe + Fbs * (momentFlange.Bolt.NumberOfRows - 1));
			tbReq = Rload / BearingCap;
		}
	}
}