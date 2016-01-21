using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class FlangeSplicePlate
	{
		internal static void DesignFlangeSplicePlate(EMemberType memberType)
		{
			double Rload = 0;
			double RLoadL = 0;
			double RLoadR = 0;
			double Girdertw = 0;
			double SupFy = 0;
			double a;
			double minweld;
			double Fbs = 0;
			double Fbe = 0;
			double BearingCap = 0;
			double FiRn = 0;
			double Fcr = 0;
			double kL_r = 0;
			double w = 0;
			double w1 = 0;
			double weldlength;
			double U = 0;
			double weldlength_1;
			double tForNetArea;
			double Lmax;
			double Lcheck;
			double L2 = 0;
			double WeldAreaReq;
			double RpL;
			double maxwidth;
			double rg;
			double k = 0;
			double t;
			double ef;
			double tbReq = 0;
			double tReq_BlockShear;
			double re;
			double tmin;
			double MinWidth;
			double tnReq;
			double HoleSize;
			double tgReq;
			double Anreq;
			double AgReq;
			double LnShear;
			double LgShear;
			double LnTension;
			double LgTension;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			var flangePlate = beam.WinConnect.MomentFlangePlate;

			if (rightBeam.MomentConnection != EMomentCarriedBy.FlangePlate || leftBeam.MomentConnection != EMomentCarriedBy.FlangePlate)
				return;

			// Resets these values if the user changed them then unchecked the box. They are not set anywhere else.
			if (!flangePlate.Bolt.EdgeDistLongDir_User)
				flangePlate.Bolt.EdgeDistLongDir = flangePlate.Bolt.MinEdgeSheared;
			if (!flangePlate.Bolt.EdgeDistTransvDir_User)
				flangePlate.Bolt.EdgeDistTransvDir = flangePlate.Bolt.MinEdgeSheared;

			if (!flangePlate.Bolt.SpacingLongDir_User)
				flangePlate.Bolt.SpacingLongDir = CommonDataStatic.Units == EUnit.US ? 3 : 75;
			if (!flangePlate.Bolt.SpacingTransvDir_User)
				flangePlate.Bolt.SpacingTransvDir = CommonDataStatic.Units == EUnit.US ? 3 : 75;

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BraceToColumn:
				case EJointConfiguration.ColumnSplice:
					return;
				case EJointConfiguration.BeamToGirder:
					if (rightBeam.WinConnect.Beam.BottomCope && leftBeam.WinConnect.Beam.BottomCope)
						Reporting.AddLine(ConstString.BEAM_SPLICE_WARNING);

					SupFy = column.Material.Fy;
					Girdertw = Math.Max(column.Shape.tw, column.Shape.tf);
					Girdertw = Math.Max(Girdertw, leftBeam.Shape.tf);
					RLoadR = Math.Abs(rightBeam.P / 2) + Math.Abs(rightBeam.Moment / rightBeam.Shape.d);
					RLoadL = Math.Abs(leftBeam.P / 2) + Math.Abs(leftBeam.Moment / leftBeam.Shape.d);
					Rload = Math.Max(RLoadR, RLoadL);
					break;
				case EJointConfiguration.BeamSplice:
					if (rightBeam.Shape.d != leftBeam.Shape.d)
						Reporting.AddLine(ConstString.BEAM_SPLICE_WARNING);

					Girdertw = Math.Max(column.Shape.tf, leftBeam.Shape.tf);
					SupFy = rightBeam.Material.Fy;
					RLoadR = Math.Abs(rightBeam.P / 2) + Math.Abs(rightBeam.Moment / rightBeam.Shape.d);
					RLoadL = Math.Abs(leftBeam.P / 2) + Math.Abs(leftBeam.Moment / leftBeam.Shape.d);
					Rload = Math.Max(RLoadR, RLoadL);
					break;
			}

			if (flangePlate.Connection == EConnectionStyle.Bolted)
			{
				if (!flangePlate.Bolt.NumberOfRows_User)
					flangePlate.Bolt.NumberOfRows = (int) Math.Ceiling(Rload / flangePlate.Bolt.BoltStrength / flangePlate.Bolt.NumberOfLines);
				flangePlate.Bolt.NumberOfBolts = flangePlate.Bolt.NumberOfRows * flangePlate.Bolt.NumberOfLines;
				re = flangePlate.Bolt.MinEdgeSheared;
				GetBeamGageSplice(memberType);
				if (!flangePlate.TopWidth_User || flangePlate.TopWidth == 0)
				{
					if (flangePlate.Bolt.NumberOfLines == 4)
						flangePlate.TopWidth = beam.GageOnFlange + 2 * (flangePlate.TransvBoltSpacingOut + re);
					else
						flangePlate.TopWidth = beam.GageOnFlange + 2 * re;
				}
				AgReq = Rload / (ConstNum.FIOMEGA0_9N * flangePlate.Material.Fy);
				Anreq = Rload / (ConstNum.FIOMEGA0_75N * flangePlate.Material.Fu);
				tgReq = AgReq / flangePlate.TopWidth;
				HoleSize = flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH;
				tnReq = Anreq / (flangePlate.TopWidth - Math.Max(0.15 * flangePlate.TopWidth, flangePlate.Bolt.NumberOfLines * HoleSize));

				if (flangePlate.Bolt.NumberOfLines == 4)
				{
					LgTension = Math.Min(beam.GageOnFlange, 2 * flangePlate.Bolt.EdgeDistTransvDir) + 2 * flangePlate.TransvBoltSpacingOut;
					LnTension = LgTension - 3 * (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
				}
				else
				{
					LgTension = Math.Min(beam.GageOnFlange, 2 * flangePlate.Bolt.EdgeDistTransvDir);
					LnTension = LgTension - (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
				}
				LgShear = 2 * ((flangePlate.Bolt.NumberOfRows - 1) * flangePlate.Bolt.SpacingLongDir + flangePlate.Bolt.EdgeDistLongDir);
				LnShear = LgShear - 2 * (flangePlate.Bolt.NumberOfRows - 0.5) * (flangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);

				tReq_BlockShear = Rload / MiscCalculationsWithReporting.BlockShearNew(flangePlate.Material.Fu, LnShear, 1, LnTension, LgShear, flangePlate.Material.Fy, 1, 0, false);

				GetRequiredThforBearingSplice(memberType, ref Fbe, ref Fbs, ref BearingCap, Rload, ref tbReq);
				tmin = Math.Max(Math.Max(tgReq, tnReq), Math.Max(tbReq, tReq_BlockShear));
				if (!flangePlate.TopThickness_User || flangePlate.TopThickness == 0)
					flangePlate.TopThickness = NumberFun.Round(tmin, 16);
				ef = MiscCalculationsWithReporting.MinimumEdgeDist(flangePlate.Bolt.BoltSize, Rload / flangePlate.Bolt.NumberOfBolts, beam.Material.Fu * beam.Shape.tf, flangePlate.Bolt.MinEdgeSheared, flangePlate.Bolt.HoleLength, flangePlate.Bolt.HoleType);
				if (!beam.WinConnect.Beam.BoltEdgeDistanceOnFlange_User)
					beam.WinConnect.Beam.BoltEdgeDistanceOnFlange = Math.Max(ef, beam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
				leftBeam.WinConnect.MomentFlangePlate.Bolt.EdgeDistBrace = beam.WinConnect.Beam.BoltEdgeDistanceOnFlange;
				if (!flangePlate.TopLength_User || flangePlate.TopLength == 0)
					flangePlate.TopLength = flangePlate.Bolt.SpacingLongDir * (flangePlate.Bolt.NumberOfRows - 1) + flangePlate.Bolt.EdgeDistLongDir + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + beam.EndSetback;

				// Here we should set BottomPlate = TopPlate (MT 7/13/15)
				flangePlate.BottomLength = flangePlate.TopLength;
				flangePlate.BottomPlateToBeamWeldSize = flangePlate.TopPlateToBeamWeldSize;
				flangePlate.BottomPlateToSupportWeldSize = flangePlate.TopPlateToSupportWeldSize;
				flangePlate.BottomThickness = flangePlate.TopThickness;
				flangePlate.BottomWidth = flangePlate.TopWidth;
				flangePlate.BottomHSSSideWallWeldLength = flangePlate.TopHSSSideWallWeldLength;

				t = flangePlate.BottomThickness - ConstNum.SIXTEENTH_INCH;
				
                while (FiRn < Rload)
				{
					t += ConstNum.SIXTEENTH_INCH;
					k = 0.65;
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
					{
						if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted && rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
						else if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
						else if (rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback);
						else
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback);
					}
					else
					{
						if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted && rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + column.Shape.bf);
						else if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + column.Shape.bf);
						else if (rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + column.Shape.bf);
						else
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + column.Shape.bf);

					}
					rg = t / Math.Sqrt(12);
					kL_r = (k * beam.WinConnect.Fema.L / rg);
					Fcr = CommonCalculations.BucklingStress(kL_r, flangePlate.Material.Fy, false);
					FiRn = ConstNum.FIOMEGA0_9N * Fcr * flangePlate.BottomWidth * t;
				}
				if (!flangePlate.BottomThickness_User || flangePlate.BottomThickness == 0)
					flangePlate.BottomThickness = CommonCalculations.PlateThickness(t);
			    if (!flangePlate.TopThickness_User)
			        flangePlate.TopThickness = flangePlate.BottomThickness;
			}
			else
			{
				AgReq = Math.Max(Rload / (ConstNum.FIOMEGA0_9N * flangePlate.Material.Fy), Rload / (ConstNum.FIOMEGA0_75N * 0.85F * flangePlate.Material.Fu));

				maxwidth = beam.Shape.bf - 2 * (flangePlate.TopPlateToBeamWeldSize + ConstNum.QUARTER_INCH);
				
				while (maxwidth < flangePlate.TopWidth)
				{
					flangePlate.TopWidth = NumberFun.Round(maxwidth, ERoundingPrecision.Eighth, ERoundingStyle.RoundDown);
					tgReq = AgReq / flangePlate.TopWidth;
					t = tgReq - ConstNum.SIXTEENTH_INCH;
					FiRn = ConstNum.FIOMEGA0_9N * Fcr * flangePlate.TopWidth * t;
					while (FiRn < Rload)
					{
						t += ConstNum.SIXTEENTH_INCH;
						k = 0.65;
						if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
						{
							if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted && rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
							else if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
							else if (rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback);
							else
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback);
						}
						else
						{
							if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted && rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + column.Shape.bf);
							else if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + column.Shape.bf);
							else if (rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + column.Shape.bf);
							else
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + column.Shape.bf);
						}
						rg = t / Math.Sqrt(12);
						kL_r = (k * beam.WinConnect.Fema.L / rg);
						Fcr = CommonCalculations.BucklingStress(kL_r, flangePlate.Material.Fy, false);
						FiRn = ConstNum.FIOMEGA0_9N * Fcr * flangePlate.TopWidth * t;
					}
					if (!flangePlate.TopThickness_User || flangePlate.TopThickness == 0)
						flangePlate.TopThickness = CommonCalculations.PlateThickness(t);

					w = CommonCalculations.MinimumWeld(flangePlate.TopThickness, beam.Shape.tf);
					if (flangePlate.TopThickness <= ConstNum.QUARTER_INCH)
						w1 = flangePlate.TopThickness;
					else
						w1 = flangePlate.TopThickness - ConstNum.SIXTEENTH_INCH;
					if (!flangePlate.TopPlateToBeamWeldSize_User)
						flangePlate.TopPlateToBeamWeldSize = Math.Min(w, w1);
					maxwidth = beam.Shape.bf - 2 * (flangePlate.TopPlateToBeamWeldSize + ConstNum.QUARTER_INCH);				
				}

				do
				{
					RpL = Rload /
					      (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx *
					       flangePlate.TopPlateToBeamWeldSize);
					WeldAreaReq = (Rload / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx) -
					               flangePlate.TopPlateToBeamWeldSize * flangePlate.TopWidth) / 2; // one line on the side of the plate
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
					if (Lmax < Lcheck)
						flangePlate.TopPlateToBeamWeldSize = flangePlate.TopPlateToBeamWeldSize + ConstNum.SIXTEENTH_INCH;
				} while (!(Lcheck <= Lmax) && !double.IsNaN(Lmax));

				if (!flangePlate.TopLength_User || flangePlate.TopLength == 0)
					flangePlate.TopLength = NumberFun.Round((weldlength_1 - flangePlate.TopWidth) / 2 + beam.EndSetback + flangePlate.TopPlateToBeamWeldSize, 8);
				AgReq = Rload / (ConstNum.FIOMEGA0_9N * flangePlate.Material.Fy);
				MinWidth = beam.Shape.bf + 2 * beam.Shape.tf;
				
				// Not sure where this line came from: (Mike (12/18/14)
				//MinWidth = beam.Shape.bf + 2 * (flangePlate.BottomPlateToBeamWeldSize + ConstNum.QUARTER_INCH);
				do
				{
					flangePlate.BottomWidth = NumberFun.Round(MinWidth, 8);
					tgReq = AgReq / flangePlate.BottomWidth;
					t = tgReq - ConstNum.SIXTEENTH_INCH;

					FiRn = ConstNum.FIOMEGA0_9N * Fcr * flangePlate.BottomWidth * t;
					while (FiRn < Rload)
					{
						t += ConstNum.SIXTEENTH_INCH;
						k = 0.65;
						if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
						{
							if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted && rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
							else if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
							else if (rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback);
							else
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback);
						}
						else
						{
							if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted && rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + column.Shape.bf);
							else if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + column.Shape.bf);
							else if (rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + column.Shape.bf);
							else
								beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + column.Shape.bf);
						}
						rg = t / Math.Sqrt(12);
						kL_r = (k * beam.WinConnect.Fema.L / rg);

						Fcr = CommonCalculations.BucklingStress(kL_r, flangePlate.Material.Fy, false);

						FiRn = ConstNum.FIOMEGA0_9N * Fcr * flangePlate.BottomWidth * t;
					}

					if (flangePlate.BottomThickness_User || flangePlate.BottomThickness == 0)
						flangePlate.BottomThickness = CommonCalculations.PlateThickness(Math.Max(tgReq, t));

					w = CommonCalculations.MinimumWeld(flangePlate.BottomThickness, beam.Shape.tf);
					if (flangePlate.BottomThickness <= ConstNum.QUARTER_INCH)
						w1 = NumberFun.Round(beam.Shape.tf, 16);
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
					else
						weldlength = weldlength_1;
					if (weldlength >= 4 * beam.Shape.bf)
						U = 1;
					else if (weldlength >= 3 * beam.Shape.bf)
						U = 0.87;
					else if (weldlength >= 2 * beam.Shape.bf)
						U = 0.75;
					if (U > 0.85)
						U = 0.85;
					tForNetArea = Rload / (ConstNum.FIOMEGA0_75N * U * flangePlate.Material.Fu * flangePlate.BottomWidth);
					if (flangePlate.BottomThickness < tForNetArea)
					{
						if (flangePlate.BottomThickness_User || flangePlate.BottomThickness == 0)
							flangePlate.BottomThickness = CommonCalculations.PlateThickness(tForNetArea);
					}
					MinWidth = beam.Shape.bf + 2 * (flangePlate.BottomPlateToBeamWeldSize + ConstNum.QUARTER_INCH);
				} while (MinWidth > flangePlate.BottomWidth);
				
				if (!flangePlate.BottomLength_User)
					flangePlate.BottomLength = NumberFun.Round(weldlength / 2 + beam.EndSetback + flangePlate.BottomPlateToBeamWeldSize, 8);
			}

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
			{
				if (!flangePlate.BottomWidth_User || flangePlate.BottomWidth == 0)
					flangePlate.BottomWidth = beam.Shape.bf;
				if (!flangePlate.BottomThickness_User || flangePlate.BottomThickness == 0)
					flangePlate.BottomThickness = beam.Shape.tf;

				if (!flangePlate.BottomPlateToSupportWeldSize_User || flangePlate.BottomPlateToSupportWeldSize == 0)
					flangePlate.BottomPlateToSupportWeldSize = NumberFun.Round(Rload / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.BottomWidth * 2), 16);
				if (flangePlate.BottomPlateToSupportWeldSize <= 0.3126)
					flangePlate.PlateToSupportWeldType = EWeldType.Fillet;
				else if (flangePlate.BottomPlateToSupportWeldSize <= flangePlate.BottomThickness / 2)
				{
					flangePlate.PlateToSupportWeldType = EWeldType.CJP;
					flangePlate.BottomPlateToSupportWeldSize = 0;
				}
				else
					flangePlate.PlateToSupportWeldType = EWeldType.PJP;
			}
			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder && !beam.WinConnect.Beam.TopCope)
			{
				flangePlate.TopWidth = 0;
				flangePlate.TopLength = 0;
				flangePlate.TopThickness = 0;
			}
			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder && !beam.WinConnect.Beam.BottomCope)
			{
				flangePlate.BottomWidth = 0;
				flangePlate.BottomLength = 0;
				flangePlate.BottomThickness = 0;
			}

			a = rightBeam.WinConnect.Beam.TopElValue - rightBeam.Shape.d - (leftBeam.WinConnect.Beam.TopElValue - leftBeam.Shape.d);
			if (a > leftBeam.Shape.kdes)
			{
				flangePlate.FExtensionCase = 1;
				flangePlate.FExtensionT = column.Shape.tf;
				flangePlate.FExtensionWidth = rightBeam.Shape.bf / 2;
				flangePlate.FExtensionFy = rightBeam.Material.Fy;
				flangePlate.FExtensionLength = Rload / 2 / (ConstNum.FIOMEGA1_0N * 0.6 * Math.Min(leftBeam.Material.Fy * leftBeam.Shape.tw / 2, flangePlate.FExtensionFy * flangePlate.FExtensionT));
				flangePlate.FExtensionWeld = Rload / (4 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.FExtensionLength);
				minweld = CommonCalculations.MinimumWeld(flangePlate.FExtensionT, rightBeam.Shape.tw);
				flangePlate.FExtensionWeld = NumberFun.Round(Math.Max(flangePlate.FExtensionWeld, minweld), 16);
			}
			else if (a >= Girdertw)
			{
				flangePlate.FExtensionCase = 2;
				flangePlate.FExtensionT = flangePlate.TopThickness;
				flangePlate.FExtensionWidth = flangePlate.TopWidth;
				flangePlate.FExtensionFy = flangePlate.Material.Fy;
				flangePlate.FExtensionLength = flangePlate.TopLength;
				flangePlate.FExtensionWeld = Rload / (4 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.FExtensionLength);
				minweld = CommonCalculations.MinimumWeld(flangePlate.FExtensionT, rightBeam.Shape.tw);
				flangePlate.FExtensionWeld = NumberFun.Round(Math.Max(flangePlate.FExtensionWeld, minweld), 16);
			}
			else if (a < -rightBeam.Shape.kdet)
			{
				flangePlate.FExtensionCase = 3;
				flangePlate.FExtensionT = leftBeam.Shape.tf;
				flangePlate.FExtensionWidth = leftBeam.Shape.bf / 2;
				flangePlate.FExtensionFy = leftBeam.Material.Fy;
				flangePlate.FExtensionLength = Rload / 2 / (ConstNum.FIOMEGA1_0N * 0.6 * Math.Min(rightBeam.Material.Fy * rightBeam.Shape.tw / 2, flangePlate.FExtensionFy * flangePlate.FExtensionT));
				flangePlate.FExtensionWeld = Rload / (4 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.FExtensionLength);
				minweld = CommonCalculations.MinimumWeld(flangePlate.FExtensionT, leftBeam.Shape.tw);
				flangePlate.FExtensionWeld = NumberFun.Round(Math.Max(flangePlate.FExtensionWeld, minweld), 16);
			}
			else if (a <= -Girdertw)
			{
				flangePlate.FExtensionCase = 4;
				flangePlate.FExtensionT = flangePlate.TopThickness;
				flangePlate.FExtensionWidth = flangePlate.TopWidth;
				flangePlate.FExtensionFy = flangePlate.Material.Fy;
				flangePlate.FExtensionLength = flangePlate.TopLength;
				flangePlate.FExtensionWeld = Rload / (4 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.FExtensionLength);
				minweld = CommonCalculations.MinimumWeld(flangePlate.FExtensionT, leftBeam.Shape.tw);
				flangePlate.FExtensionWeld = NumberFun.Round(Math.Max(flangePlate.FExtensionWeld, minweld), 16);
			}
			else
			{
				flangePlate.FExtensionCase = 0;
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
				{
					flangePlate.FExtensionT = flangePlate.TopThickness;
					flangePlate.FExtensionWidth = flangePlate.TopWidth;
					flangePlate.FExtensionFy = flangePlate.Material.Fy;
					flangePlate.FExtensionLength = flangePlate.TopLength;
					flangePlate.FExtensionWeld = Rload / (4 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.FExtensionLength);
					minweld = CommonCalculations.MinimumWeld(flangePlate.FExtensionT, leftBeam.Shape.tw);
					flangePlate.FExtensionWeld = NumberFun.Round(Math.Max(flangePlate.FExtensionWeld, minweld), 16);
				}
			}

			FlangeSplicePlateCapacity(memberType, ref SupFy, ref Girdertw, ref RLoadR, ref RLoadL, ref Fbe, ref Fbs, ref BearingCap, ref FiRn, ref k, ref kL_r, ref Fcr, ref w, ref w1, ref a, ref U);
		}

		private static void GetBeamGageSplice(EMemberType memberType)
		{
			double gagemax;
			double outgage;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var flangePlate = beam.WinConnect.MomentFlangePlate;

			gagemax = beam.Shape.bf - 2 * flangePlate.Bolt.MinEdgeRolled;

			outgage = NumberFun.Round((gagemax - beam.GageOnFlange) / 2, 4);
			if (!flangePlate.TransvBoltSpacing_User || flangePlate.TransvBoltSpacingOut == 0)
			{
				if (outgage >= 2.67 * flangePlate.Bolt.BoltSize)
				{
					if (flangePlate.TransvBoltSpacingOut > outgage)
						flangePlate.TransvBoltSpacingOut = outgage;
					if (flangePlate.TransvBoltSpacingOut <= 2.67 * flangePlate.Bolt.BoltSize)
						flangePlate.TransvBoltSpacingOut = Math.Min(NumberFun.Round(2.67 * flangePlate.Bolt.BoltSize, 4), outgage);
				}
				else
				{
					flangePlate.TransvBoltSpacingOut = 0;
					flangePlate.Bolt.NumberOfLines = 2;
				}
			}
		}

		private static void GetRequiredThforBearingSplice(EMemberType memberType, ref double Fbe, ref double Fbs, ref double BearingCap, double Rload, ref double tbReq)
		{
			var flangePlate = CommonDataStatic.DetailDataDict[memberType].WinConnect.MomentFlangePlate;

			Fbe = CommonCalculations.EdgeBearing(flangePlate.Bolt.EdgeDistLongDir, flangePlate.Bolt.HoleLength, flangePlate.Bolt.BoltSize, flangePlate.Material.Fu, flangePlate.Bolt.HoleType, false);
			Fbs = CommonCalculations.SpacingBearing(flangePlate.Bolt.SpacingLongDir, flangePlate.Bolt.HoleLength, flangePlate.Bolt.BoltSize, flangePlate.Bolt.HoleType, flangePlate.Material.Fu, false);
			BearingCap = flangePlate.Bolt.NumberOfLines * (Fbe + Fbs * (flangePlate.Bolt.NumberOfRows - 1));
			tbReq = Rload / BearingCap;
		}

		private static void FlangeSplicePlateCapacity(EMemberType memberType, ref double SupFy, ref double Girdertw, ref double RLoadR, ref double RLoadL, ref double Fbe, ref double Fbs, ref double BearingCap, ref double FiRn, ref double k, ref double kL_r, ref double Fcr, ref double w, ref double w1, ref double a, ref double U)
		{
			double Ff = 0;
			double PLLength;
			double spBeam;
			double spPlate;
			double evmin;
			double ehmin;
			double BeamTrEdge;
			double Cap;
			double Tg;
			double bn;
			double Tn;
			double AgTension;
			double AnTension;
			double AgShear;
			double AnShear;
			double sqr12;
			double BetaN;
			double RequiredWeldSize;
			double FullPenCap;
			double RequiredreinfSize;
			double PLLengthBottom;
			double wLength;
			double tmpCaseArg;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			var flangePlate = beam.WinConnect.MomentFlangePlate;

			Reporting.AddMainHeader(beam.ComponentName + " - " + beam.ShapeName + " Moment Connection");
			Reporting.AddHeader("Moment Connection Using Flange Plate: ");

			if (CommonDataStatic.IsFema)
				return;

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BraceToColumn:
				case EJointConfiguration.ColumnSplice:
					return;
				case EJointConfiguration.BeamToGirder:
					SupFy = column.Material.Fy;
					if (rightBeam.WinConnect.Beam.BottomCope && leftBeam.WinConnect.Beam.BottomCope)
					{
						Reporting.AddLine("Descon does not handle the moment connection when both beams are coped at bottom. If you encounter this condition often, fax us the detail you prefer and we might incorporate it in a future version of Descon. (NG)");
						return;
					}
					Girdertw = Math.Max(column.Shape.tw, column.Shape.tf);
					Girdertw = Math.Max(Girdertw, leftBeam.Shape.tf);
					RLoadR = Math.Abs(rightBeam.P / 2) + Math.Abs(rightBeam.Moment / rightBeam.Shape.d);
					RLoadL = Math.Abs(leftBeam.P / 2) + Math.Abs(leftBeam.Moment / leftBeam.Shape.d);
					Ff = Math.Max(RLoadR, RLoadL);

					Reporting.AddHeader("Flange Force (Ff):");
					Reporting.AddHeader("Right Side:");
					if (CommonDataStatic.Units == EUnit.US)
					{
						Reporting.AddLine("RFf = P / 2 + M / d");
						Reporting.AddLine("= " + Math.Abs(rightBeam.P) + " / 2 + " + Math.Abs(rightBeam.Moment) + " / " + rightBeam.Shape.d);
					}
					else
					{
						Reporting.AddLine("RFf = P / 2 + M / d");
						Reporting.AddLine("= " + Math.Abs(rightBeam.P) + " / 2 + " + Math.Abs(rightBeam.Moment) + " / " + rightBeam.Shape.d);
					}

					Reporting.AddLine("= " + RLoadR + ConstUnit.Force);
					Reporting.AddHeader("Left Side:");
					if (CommonDataStatic.Units == EUnit.US)
					{
						Reporting.AddLine("LFf = P / 2 + M / d");
						Reporting.AddLine("= " + Math.Abs(leftBeam.P) + " / 2 + " + Math.Abs(leftBeam.Moment) + " / " + leftBeam.Shape.d);
					}
					else
					{
						Reporting.AddLine("LFf = P / 2 + M / d");
						Reporting.AddLine("= " + Math.Abs(leftBeam.P) + " / 2 + " + Math.Abs(leftBeam.Moment) + " / " + leftBeam.Shape.d);
					}
					Reporting.AddLine("= " + RLoadL + ConstUnit.Force);
					Reporting.AddLine(string.Empty);
					Reporting.AddLine("Ff = Max(RFf, LFf) = " + Ff + ConstUnit.Force);
					break;
				case EJointConfiguration.BeamSplice:
					if (rightBeam.Shape.d != leftBeam.Shape.d)
					{
						Reporting.AddLine(ConstString.BEAM_SPLICE_WARNING);
						return;
					}
					Girdertw = Math.Max(column.Shape.tf, leftBeam.Shape.tf);
					SupFy = rightBeam.Material.Fy;
					RLoadR = Math.Abs(rightBeam.P / 2) + Math.Abs(rightBeam.Moment / rightBeam.Shape.d);
					RLoadL = Math.Abs(leftBeam.P / 2) + Math.Abs(leftBeam.Moment / leftBeam.Shape.d);
					Ff = Math.Max(RLoadR, RLoadL);

					Reporting.AddHeader("Flange Force (Ff):");
					Reporting.AddHeader("Right Side:");
					if (CommonDataStatic.Units == EUnit.US)
					{
						Reporting.AddLine("RFf = P / 2 + M / d");
						Reporting.AddLine("= " + Math.Abs(rightBeam.P) + " / 2 + " + Math.Abs(rightBeam.Moment) + " / " + rightBeam.Shape.d);
					}
					else
					{
						Reporting.AddLine("RFf = P / 2 + M / d");
						Reporting.AddLine("= " + Math.Abs(rightBeam.P) + " / 2 + " + Math.Abs(rightBeam.Moment) + " / " + rightBeam.Shape.d);
					}
					Reporting.AddLine("= " + RLoadR + ConstUnit.Force);
					Reporting.AddHeader("Left Side:");
					if (CommonDataStatic.Units == EUnit.US)
					{
						Reporting.AddLine("LFf = P / 2 + M / d");
						Reporting.AddLine("= " + Math.Abs(leftBeam.P) + " / 2 + " + Math.Abs(leftBeam.Moment) + " / " + leftBeam.Shape.d);
					}
					else
					{
						Reporting.AddLine("LFf = P / 2 + M / d");
						Reporting.AddLine("= " + Math.Abs(leftBeam.P) + " / 2 + " + Math.Abs(leftBeam.Moment) + " / " + leftBeam.Shape.d);
					}
					Reporting.AddLine("= " + RLoadL + ConstUnit.Force);
					Reporting.AddLine(string.Empty);
					Reporting.AddLine("Ff = Max(RFf, LFf) = " + Ff + ConstUnit.Force);
					break;
			}

			if ((rightBeam.WinConnect.Beam.TopCope & leftBeam.WinConnect.Beam.TopCope) || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
			{
				PLLength = flangePlate.TopLength + flangePlate.TopLength;
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
					PLLength = PLLength + column.Shape.bf;

				Reporting.AddHeader("Top Flange Connection:");
				Reporting.AddLine("Top Plate: " + PLLength + ConstUnit.Length + " X " + flangePlate.TopWidth + ConstUnit.Length + " X " + flangePlate.TopThickness + ConstUnit.Length);
				Reporting.AddLine("Plate Material: " + flangePlate.Material.Name);
				if (flangePlate.Connection == EConnectionStyle.Bolted)
				{
					Reporting.AddLine(string.Empty);
					flangePlate.Bolt.NumberOfBolts = flangePlate.Bolt.NumberOfRows * flangePlate.Bolt.NumberOfLines;
					Reporting.AddLine("Bolts on Flange: " + flangePlate.Bolt.NumberOfBolts + " Bolts - " + flangePlate.Bolt.BoltName + " in " + flangePlate.Bolt.NumberOfLines + " Lines");
					Reporting.AddLine("Bolt Holes on Plate: " + flangePlate.Bolt.HoleWidth + ConstUnit.Length + "  Lateral X " + flangePlate.Bolt.HoleLength + ConstUnit.Length + "  Longitudinal");
					Reporting.AddLine("Bolt Holes on Flange: " + flangePlate.Bolt.HoleWidth + ConstUnit.Length + "  Lateral X " + flangePlate.Bolt.HoleLength + ConstUnit.Length + "  Longitudinal");

					MiscCalculationsWithReporting.BeamCheck(beam.MemberType);

					Reporting.AddHeader("Bolts");
					spBeam = MiscCalculationsWithReporting.MinimumSpacing((flangePlate.Bolt.BoltSize), (Ff / flangePlate.Bolt.NumberOfBolts), beam.Material.Fu * beam.Shape.tf, flangePlate.Bolt.HoleLength, flangePlate.Bolt.HoleType);
					spPlate = MiscCalculationsWithReporting.MinimumSpacing((flangePlate.Bolt.BoltSize), (Ff / flangePlate.Bolt.NumberOfBolts), flangePlate.Material.Fu * flangePlate.TopThickness, flangePlate.Bolt.HoleLength, flangePlate.Bolt.HoleType);
					flangePlate.Bolt.MinSpacing = Math.Max(spBeam, spPlate);

					if (flangePlate.Bolt.SpacingLongDir >= flangePlate.Bolt.MinSpacing)
						Reporting.AddLine("Spacing (s) = " + flangePlate.Bolt.SpacingLongDir + " >= Minimum Spacing = " + flangePlate.Bolt.MinSpacing + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Spacing (s) = " + flangePlate.Bolt.SpacingLongDir + " << Minimum Spacing = " + flangePlate.Bolt.MinSpacing + ConstUnit.Length + " (NG)");

					evmin = MiscCalculationsWithReporting.MinimumEdgeDist(flangePlate.Bolt.BoltSize, Ff / flangePlate.Bolt.NumberOfBolts, flangePlate.Material.Fu * flangePlate.TopThickness, flangePlate.Bolt.MinEdgeSheared, (int) flangePlate.Bolt.HoleLength, flangePlate.Bolt.HoleType);
					Reporting.AddHeader("Edge Distance on Plate:");
					Reporting.AddLine("Parallel to Beam Axis (el):");
					if (flangePlate.Bolt.EdgeDistLongDir >= evmin)
						Reporting.AddLine("= " + flangePlate.Bolt.EdgeDistLongDir + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + flangePlate.Bolt.EdgeDistLongDir + " << " + evmin + ConstUnit.Length + " (NG)");

					ehmin = flangePlate.Bolt.MinEdgeSheared;
					Reporting.AddHeader("Transverse to Beam (et):");
					if (flangePlate.Bolt.EdgeDistTransvDir >= ehmin)
						Reporting.AddLine("= " + ((flangePlate.TopWidth - beam.GageOnFlange) / 2) + " >= " + ehmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + ((flangePlate.TopWidth - beam.GageOnFlange) / 2) + " << " + ehmin + ConstUnit.Length + " (NG)");

					evmin = MiscCalculationsWithReporting.MinimumEdgeDist(flangePlate.Bolt.BoltSize, Ff / flangePlate.Bolt.NumberOfBolts, beam.Material.Fu * beam.Shape.tf, flangePlate.Bolt.MinEdgeSheared, (int) flangePlate.Bolt.HoleLength, flangePlate.Bolt.HoleType);
					Reporting.AddHeader("Edge Distance on Beam Flange:");
					Reporting.AddLine("Parallel to Beam Axis (el):");
					if (beam.WinConnect.Beam.BoltEdgeDistanceOnFlange >= evmin)
						Reporting.AddLine("= " + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " << " + evmin + ConstUnit.Length + " (NG)");
					ehmin = flangePlate.Bolt.MinEdgeRolled;

					Reporting.AddHeader("Transverse to Beam (et):");
					BeamTrEdge = (beam.Shape.bf - (beam.GageOnFlange + 2 * flangePlate.TransvBoltSpacingOut)) / 2;
					if (BeamTrEdge >= ehmin)
						Reporting.AddLine("= " + BeamTrEdge + " >= " + ehmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + BeamTrEdge + " << " + ehmin + ConstUnit.Length + " (NG)");

					Cap = flangePlate.Bolt.NumberOfBolts * flangePlate.Bolt.BoltStrength;
					Reporting.AddHeader("Bolt Shear:");
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = n * Fv = " + flangePlate.Bolt.NumberOfBolts + " *  " + flangePlate.Bolt.BoltStrength);
					if (Cap >= Ff)
						Reporting.AddLine("= " + Cap + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Cap + " << " + Ff + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Bolt Bearing:");
					Reporting.AddHeader("Bolt Bearing on Plate:");
					Fbe = CommonCalculations.EdgeBearing(flangePlate.Bolt.EdgeDistLongDir, flangePlate.Bolt.HoleLength, flangePlate.Bolt.BoltSize, flangePlate.Material.Fu, flangePlate.Bolt.HoleType, true);
					Fbs = CommonCalculations.SpacingBearing(flangePlate.Bolt.SpacingLongDir, flangePlate.Bolt.HoleLength, flangePlate.Bolt.BoltSize, flangePlate.Bolt.HoleType, flangePlate.Material.Fu, true);
					BearingCap = flangePlate.Bolt.NumberOfLines * (Fbe + Fbs * (flangePlate.Bolt.NumberOfRows - 1)) * flangePlate.TopThickness;
					Reporting.AddLine("");
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = Nt * (Fbe + Fbs * (Nl - 1)) * t");
					Reporting.AddLine("= " + flangePlate.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + flangePlate.Bolt.NumberOfRows + " - 1)) * " + flangePlate.TopThickness);
					if (BearingCap >= Ff)
						Reporting.AddLine("= " + BearingCap + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + BearingCap + " << " + Ff + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Bolt Bearing on Flange:");
					Fbe = CommonCalculations.EdgeBearing(beam.WinConnect.Beam.BoltEdgeDistanceOnFlange, flangePlate.Bolt.HoleLength, flangePlate.Bolt.BoltSize, beam.Material.Fu, flangePlate.Bolt.HoleType, true);
					Fbs = CommonCalculations.SpacingBearing(flangePlate.Bolt.SpacingLongDir, flangePlate.Bolt.HoleLength, flangePlate.Bolt.BoltSize, flangePlate.Bolt.HoleType, beam.Material.Fu, true);
					BearingCap = flangePlate.Bolt.NumberOfLines * (Fbe + Fbs * (flangePlate.Bolt.NumberOfRows - 1)) * beam.Shape.tf;

					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = Nt * (Fbe + Fbs * (Nl - 1)) * t");
					Reporting.AddLine("= " + flangePlate.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + flangePlate.Bolt.NumberOfRows + " - 1)) * " + beam.Shape.tf);
					if (BearingCap >= Ff)
						Reporting.AddLine("= " + BearingCap + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + BearingCap + " << " + Ff + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Plate Tension " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
					Tg = flangePlate.TopWidth * flangePlate.TopThickness * ConstNum.FIOMEGA0_9N * flangePlate.Material.Fy;
					Reporting.AddHeader("Tension Yielding:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * b * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + flangePlate.Material.Fy + " * " + flangePlate.TopWidth + " * " + flangePlate.TopThickness);
					if (Tg >= Ff)
						Reporting.AddLine("= " + Tg + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Tg + " << " + Ff + ConstUnit.Force + " (NG)");

					bn = flangePlate.TopWidth - Math.Max(0.15F * flangePlate.TopWidth, flangePlate.Bolt.NumberOfLines * (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH));
					Tn = bn * flangePlate.TopThickness * ConstNum.FIOMEGA0_75N * flangePlate.Material.Fu;

					Reporting.AddHeader("Tension Rupture:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Fu * (b - Max(0.15 * b; Nt * (dh + " + ConstNum.SIXTEENTH_INCH + "))) * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + flangePlate.Material.Fu + " * " + "(" + flangePlate.TopWidth + " - Max(0.15 * " + flangePlate.TopWidth + " ; " + flangePlate.Bolt.NumberOfLines + " * (" + flangePlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + "))) * " + flangePlate.TopThickness);
					if (Tn >= Ff)
						Reporting.AddLine("= " + Tn + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Tn + " << " + Ff + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Block shear rupture of the Plate:");
					if (flangePlate.Bolt.NumberOfLines == 4)
					{
						AgTension = (Math.Min(beam.GageOnFlange, 2 * flangePlate.Bolt.EdgeDistTransvDir) + 2 * flangePlate.TransvBoltSpacingOut) * flangePlate.TopThickness;
						Reporting.AddLine(string.Empty);
						Reporting.AddLine("Agt = (Min(g1, 2 * e) + 2 * g2) * t");
						Reporting.AddLine("= (" + Math.Min(beam.GageOnFlange, 2 * flangePlate.Bolt.EdgeDistTransvDir) + " + 2 * " + flangePlate.TransvBoltSpacingOut + ") * " + flangePlate.TopThickness);
						Reporting.AddLine("= " + AgTension + ConstUnit.Area);

						Reporting.AddLine(string.Empty);
						AnTension = AgTension - 3 * (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * flangePlate.TopThickness;
						Reporting.AddLine("Ant = Agt - (n - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
						Reporting.AddLine("= " + AgTension + " - 3 * (" + (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + flangePlate.TopThickness);
						Reporting.AddLine("= " + AnTension + ConstUnit.Area);
					}
					else
					{
						AgTension = Math.Min(beam.GageOnFlange, flangePlate.TopWidth - beam.GageOnFlange) * flangePlate.TopThickness;
						Reporting.AddLine("Agt = Min(g, 2 * e) * t = " + Math.Min(beam.GageOnFlange, flangePlate.TopWidth - beam.GageOnFlange) + " * " + flangePlate.TopThickness);
						Reporting.AddLine("= " + AgTension + ConstUnit.Area);

						Reporting.AddLine(string.Empty);
						AnTension = AgTension - (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * flangePlate.TopThickness;
						Reporting.AddLine("Ant = Agt - (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
						Reporting.AddLine("= " + AgTension + " - (" + (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + flangePlate.TopThickness);
						Reporting.AddLine("= " + AnTension + ConstUnit.Area);
					}
					AgShear = 2 * ((flangePlate.Bolt.NumberOfRows - 1) * flangePlate.Bolt.SpacingLongDir + flangePlate.Bolt.EdgeDistLongDir) * flangePlate.TopThickness;

					Reporting.AddLine(string.Empty);
					Reporting.AddLine("Agv = 2 * ((nl - 1) * s + Le) * t");
					Reporting.AddLine("= 2 * ((" + flangePlate.Bolt.NumberOfRows + " - 1) * " + flangePlate.Bolt.SpacingLongDir + " + " + flangePlate.Bolt.EdgeDistLongDir + ") * " + flangePlate.TopThickness);
					Reporting.AddLine("= " + AgShear + ConstUnit.Area);

					Reporting.AddLine(string.Empty);
					AnShear = AgShear - 2 * (flangePlate.Bolt.NumberOfRows - 0.5) * (flangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * flangePlate.TopThickness;
					Reporting.AddLine("Anv = Agv - 2 * (nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("=" + AgShear + " - 2 * (" + flangePlate.Bolt.NumberOfRows + " - 0.5) * (" + (flangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) + ") *" + flangePlate.TopThickness);
					Reporting.AddLine("= " + AnShear + ConstUnit.Area);

					FiRn = MiscCalculationsWithReporting.BlockShearNew(flangePlate.Material.Fu, AnShear, 1, AnTension, AgShear, flangePlate.Material.Fy, 0, Ff, true);

					Reporting.AddHeader("Top Plate " + ConstString.DES_OR_ALLOWABLE + " Compressive Strength:");
					sqr12 = Math.Sqrt(12);
					k = 0.65;
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
					{
						if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted && rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
							Reporting.AddLine("Unbraced Length (L) = gap + efR + efL = " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
							Reporting.AddLine("Unbraced Length (L) = gap + efL = " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback);
							Reporting.AddLine("Unbraced Length (L) = gap + efR = " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback);
							Reporting.AddLine("Unbraced Length (L) = gap = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
					}
					else
					{
						if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted && rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + efR + efL + bfG");
							Reporting.AddLine("= " + rightBeam.EndSetback + " + " + leftBeam.EndSetback + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + efL + bfG");
							Reporting.AddLine("= " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + efL + bfG");
							Reporting.AddLine("= " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + bfG");
							Reporting.AddLine("= " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
					}
					Reporting.AddHeader("Effective Length Factor (K) = 0.65");
					kL_r = (beam.WinConnect.Fema.L * k * sqr12 / flangePlate.TopThickness);

					Reporting.AddLine("KL / r = k * L / (t / 3.464) = " + k + " * " + beam.WinConnect.Fema.L + " / (" + flangePlate.TopThickness + " / 3.464) = " + kL_r);
					Fcr = CommonCalculations.BucklingStress(kL_r, flangePlate.Material.Fy, true);

					Reporting.AddLine(string.Empty);
					FiRn = ConstNum.FIOMEGA0_9N * Fcr * flangePlate.TopWidth * flangePlate.TopThickness;
					if (FiRn >= Ff)
						Reporting.AddLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + flangePlate.TopWidth + " * " + flangePlate.TopThickness + " = " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + flangePlate.TopWidth + " * " + flangePlate.TopThickness + " = " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Block shear rupture of the Beam Top Flange:");

					AgTension = (beam.Shape.bf - beam.GageOnFlange) * beam.Shape.tf;
					Reporting.AddLine("Agt = (bf - g) * t = (" + beam.Shape.bf + " - " + beam.GageOnFlange + ") * " + beam.Shape.tf);
					Reporting.AddLine("= " + AgTension + ConstUnit.Area);

					Reporting.AddLine(string.Empty);
					AnTension = AgTension - (flangePlate.Bolt.NumberOfLines - 1) * (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * beam.Shape.tf;
					Reporting.AddLine("Ant = Agt - (nt - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("= " + AgTension + " - (" + flangePlate.Bolt.NumberOfLines + " - 1) * (" + (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + beam.Shape.tf);
					Reporting.AddLine("= " + AnTension + ConstUnit.Area);

					Reporting.AddLine(string.Empty);
					AgShear = 2 * ((flangePlate.Bolt.NumberOfRows - 1) * flangePlate.Bolt.SpacingTransvDir + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange) * beam.Shape.tf;
					Reporting.AddLine("Agv = 2 * ((nl - 1) * s + ef) * t");
					Reporting.AddLine("= 2 * ((" + flangePlate.Bolt.NumberOfRows + " - 1) * " + flangePlate.Bolt.SpacingTransvDir + " + " + flangePlate.Bolt.EdgeDistTransvDir + ") * " + beam.Shape.tf);
					Reporting.AddLine("= " + AgShear + ConstUnit.Area);

					Reporting.AddLine(string.Empty);
					AnShear = AgShear - 2 * (flangePlate.Bolt.NumberOfRows - 0.5) * (flangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * beam.Shape.tf;
					Reporting.AddLine("Anv = Agv - 2 * (nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("=" + AgShear + " - 2*(" + flangePlate.Bolt.NumberOfRows + " - 0.5)*(" + (flangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) + ") *" + beam.Shape.tf);
					Reporting.AddLine("= " + AnShear + ConstUnit.Area);

					FiRn = MiscCalculationsWithReporting.BlockShearNew(beam.Material.Fu, AnShear, 1, AnTension, AgShear, beam.Material.Fy, 0, Ff, true);
				}
				else
				{
					Reporting.AddHeader("Top Plate Tension Strength:");
					Tg = ConstNum.FIOMEGA0_9N * flangePlate.TopWidth * flangePlate.TopThickness * flangePlate.Material.Fy;
					Reporting.AddHeader("Tension Yielding:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " *Fy*b*t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + flangePlate.Material.Fy + " * " + flangePlate.TopWidth + " * " + flangePlate.TopThickness);
					if (Tg >= Ff)
						Reporting.AddLine("= " + Tg + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Tg + " << " + Ff + ConstUnit.Force + " (NG)");

					Tn = ConstNum.FIOMEGA0_75N * 0.85F * flangePlate.TopWidth * flangePlate.TopThickness * flangePlate.Material.Fu;
					Reporting.AddHeader("Tension Rupture:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.85 * Fu * b * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.85" + flangePlate.Material.Fu + " * " + flangePlate.TopWidth + " * " + flangePlate.TopThickness);
					if (Tn >= Ff)
						Reporting.AddLine("= " + Tn + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Tn + " << " + Ff + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Top Plate " + ConstString.DES_OR_ALLOWABLE + " Compressive Strength:");
					sqr12 = Math.Sqrt(12);
					k = 0.65;
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
					{
						if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted && rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
							Reporting.AddLine("Unbraced Length, L = gap + efR + efL = " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
							Reporting.AddLine("Unbraced Length, L = gap + efL = " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback);
							Reporting.AddLine("Unbraced Length, L = gap + efR = " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback);
							Reporting.AddLine("Unbraced Length, L = gap = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
					}
					else
					{
						if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted && rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + efR + efL + bfG");
							Reporting.AddLine("= " + rightBeam.EndSetback + " + " + leftBeam.EndSetback + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + efL + bfG");
							Reporting.AddLine("= " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + efL + bfG");
							Reporting.AddLine("= " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + bfG");
							Reporting.AddLine("= " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}

					}
					Reporting.AddHeader("Effective Length Factor (K) = 0.65");
					kL_r = (beam.WinConnect.Fema.L * k * sqr12 / flangePlate.TopThickness);

					Reporting.AddLine("KL / r = k * L / (t / 3.464) = " + k + " * " + beam.WinConnect.Fema.L + " / (" + flangePlate.TopThickness + " / 3.464) = " + kL_r);

					Fcr = CommonCalculations.BucklingStress(kL_r, flangePlate.Material.Fy, true);

					FiRn = ConstNum.FIOMEGA0_9N * Fcr * flangePlate.TopWidth * flangePlate.TopThickness;
					if (FiRn >= Ff)
						Reporting.AddLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + flangePlate.TopWidth + " * " + flangePlate.TopThickness + " = " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + flangePlate.TopWidth + " * " + flangePlate.TopThickness + " = " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)");

					w = CommonCalculations.MinimumWeld(flangePlate.TopThickness, beam.Shape.tf);
					if (flangePlate.TopThickness <= ConstNum.QUARTER_INCH)
						w1 = flangePlate.TopThickness;
					else
						w1 = flangePlate.TopThickness - ConstNum.SIXTEENTH_INCH;
					if (w > w1)
						w = w1;

					Reporting.AddHeader("Top Plate to Beam Weld:");
					Reporting.AddLine("Plate Thickness = " + flangePlate.TopThickness + ConstUnit.Length + "     Beam Flange Thickness = " + beam.Shape.tf + ConstUnit.Length);
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(w) + ConstUnit.Length + "    Maximum Weld Size = " + CommonCalculations.WeldSize(w1) + ConstUnit.Length);
					if (flangePlate.TopPlateToBeamWeldSize >= w && flangePlate.TopPlateToBeamWeldSize <= w1)
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(flangePlate.TopPlateToBeamWeldSize) + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(flangePlate.TopPlateToBeamWeldSize) + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Weld " + ConstString.DES_OR_ALLOWABLE + " Strength:");
					CommonDataStatic.WeldBetaL = flangePlate.TopLength - beam.EndSetback - flangePlate.TopPlateToBeamWeldSize;
					Reporting.AddLine("Welded length of PL (Lw) = " + CommonDataStatic.WeldBetaL + ConstUnit.Length);
					BetaN = CommonCalculations.WeldBetaFactor(CommonDataStatic.WeldBetaL, flangePlate.TopPlateToBeamWeldSize);
					if (BetaN < 1)
					{
						FiRn = ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.TopPlateToBeamWeldSize * Math.Max(2 * CommonDataStatic.WeldBetaL * BetaN + flangePlate.TopWidth, 1.7F * CommonDataStatic.WeldBetaL * BetaN + 1.5 * flangePlate.TopWidth);
						Reporting.AddLine(ConstNum.BETAS + " = " + BetaN);
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.4242 * Fexx * w * Max((2 * " + ConstNum.BETAS + " Lw + b); (1.7 * " + ConstNum.BETAS + " Lw + 1.5*b))");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4242 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + flangePlate.TopPlateToBeamWeldSize + " * Math.Max(2 * " + (BetaN * CommonDataStatic.WeldBetaL) + " + " + flangePlate.TopWidth + "; 1.7 * " + (BetaN * CommonDataStatic.WeldBetaL) + " + 1.5 * " + flangePlate.TopWidth + ")");
					}
					else
					{
						FiRn = ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.TopPlateToBeamWeldSize * Math.Max(2 * CommonDataStatic.WeldBetaL + flangePlate.TopWidth, 1.7F * CommonDataStatic.WeldBetaL + 1.5 * flangePlate.TopWidth);
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.4242 * Fexx * w * Max((2 * Lw + b); (1.7 * Lw + 1.5 * b))");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4242 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + flangePlate.TopPlateToBeamWeldSize + " * Math.Max(2 * " + CommonDataStatic.WeldBetaL + " + " + flangePlate.TopWidth + "; 1.7 * " + CommonDataStatic.WeldBetaL + " + 1.5 * " + flangePlate.TopWidth + ")");
					}
					if (FiRn >= Ff)
						Reporting.AddLine("= " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)");
				}
			}
			else
			{
				Reporting.AddHeader("Top Flange to Girder Weld:");
				if (!flangePlate.BottomPlateToSupportWeldSize_User || flangePlate.BottomPlateToSupportWeldSize == 0)
					flangePlate.BottomPlateToSupportWeldSize = NumberFun.Round(Ff / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * beam.Shape.bf * 2), 16);
				RequiredWeldSize = Ff / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * beam.Shape.bf * 2);
				Reporting.AddHeader("Required Fillet Weld Size ");
				Reporting.AddLine("= Ff/(" + ConstString.FIOMEGA0_75 + " * 0.4242 * Fexx * b * 2)");
				Reporting.AddLine("= " + Ff + " / (" + ConstString.FIOMEGA0_75 + " * 0.4242 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + beam.Shape.bf + " * 2) ");
				Reporting.AddLine("= " + RequiredWeldSize + ConstUnit.Length);
				FullPenCap = 0.6 * beam.Material.Fy * beam.Shape.bf * beam.Shape.tf;
				if (RequiredWeldSize <= NumberFun.ConvertFromFraction(5))
				{
					Reporting.AddHeader("Use " + CommonCalculations.WeldSize(flangePlate.BottomPlateToSupportWeldSize) + ConstUnit.Length + "  Fillet Weld ");
					Reporting.AddHeader("on Both Sides of Flange.");
				}
				else if (Ff <= FullPenCap)
					Reporting.AddHeader("Use Full Penetration Weld.");
				else
				{
					RequiredreinfSize = (Ff - FullPenCap) / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * beam.Shape.bf * 2);
					if (!flangePlate.BottomPlateToSupportWeldSize_User || flangePlate.BottomPlateToSupportWeldSize == 0)
						flangePlate.BottomPlateToSupportWeldSize = NumberFun.Round(Math.Max(RequiredreinfSize, beam.Shape.tf / 2), 16);
					Reporting.AddHeader("Use Full Penetration Weld with " + CommonCalculations.WeldSize(flangePlate.BottomPlateToSupportWeldSize) + ConstUnit.Length);
					Reporting.AddHeader("Reinforcement on Both Sides of Flange");
				}
			}
			if ((rightBeam.WinConnect.Beam.BottomCope & leftBeam.WinConnect.Beam.BottomCope) || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
			{
				PLLengthBottom = rightBeam.WinConnect.MomentFlangePlate.BottomLength + leftBeam.WinConnect.MomentFlangePlate.BottomLength + column.Shape.bf;
				Reporting.AddHeader("Bottom Flange Connection:");
				Reporting.AddHeader("Bottom Plate: " + PLLengthBottom + ConstUnit.Length + " X " + flangePlate.BottomWidth + ConstUnit.Length + " X " + flangePlate.BottomThickness + ConstUnit.Length);
				if (flangePlate.Connection == EConnectionStyle.Bolted)
				{
					flangePlate.Bolt.NumberOfBolts = flangePlate.Bolt.NumberOfRows * flangePlate.Bolt.NumberOfLines;
					Reporting.AddLine("Bolts on Flange: " + flangePlate.Bolt.NumberOfBolts + " Bolts - " + flangePlate.Bolt.BoltName + " in " + flangePlate.Bolt.NumberOfLines + " Lines");
					Reporting.AddLine("Bolt Holes on Plate: " + flangePlate.Bolt.HoleWidth + ConstUnit.Length + "  Lateral X " + flangePlate.Bolt.HoleLength + ConstUnit.Length + "  Longitudinal");
					Reporting.AddLine("Bolt Holes on Flange: " + flangePlate.Bolt.HoleWidth + ConstUnit.Length + "  Lateral X " + flangePlate.Bolt.HoleLength + ConstUnit.Length + "  Longitudinal");

					Reporting.AddHeader("Bolts");
					spBeam = MiscCalculationsWithReporting.MinimumSpacing((flangePlate.Bolt.BoltSize), (Ff / flangePlate.Bolt.NumberOfBolts), beam.Material.Fu * beam.Shape.tf, flangePlate.Bolt.HoleLength, flangePlate.Bolt.HoleType);
					spPlate = MiscCalculationsWithReporting.MinimumSpacing((flangePlate.Bolt.BoltSize), (Ff / flangePlate.Bolt.NumberOfBolts), flangePlate.Material.Fu * flangePlate.BottomThickness, flangePlate.Bolt.HoleLength, flangePlate.Bolt.HoleType);
					flangePlate.Bolt.MinSpacing = Math.Max(spBeam, spPlate);

					if (flangePlate.Bolt.SpacingLongDir >= flangePlate.Bolt.MinSpacing)
						Reporting.AddLine("Spacing (s) = " + flangePlate.Bolt.SpacingLongDir + " >= Minimum Spacing = " + flangePlate.Bolt.MinSpacing + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Spacing (s) = " + flangePlate.Bolt.SpacingLongDir + " << Minimum Spacing = " + flangePlate.Bolt.MinSpacing + ConstUnit.Length + " (NG)");

					evmin = MiscCalculationsWithReporting.MinimumEdgeDist(flangePlate.Bolt.BoltSize, Ff / flangePlate.Bolt.NumberOfBolts, flangePlate.Material.Fu * flangePlate.BottomThickness, flangePlate.Bolt.MinEdgeSheared, (int) flangePlate.Bolt.HoleLength, flangePlate.Bolt.HoleType);
					Reporting.AddHeader("Edge Distance on Plate:");
					Reporting.AddLine("Parallel to Beam Axis (el):");
					if (flangePlate.Bolt.EdgeDistLongDir >= evmin)
						Reporting.AddLine("= " + flangePlate.Bolt.EdgeDistLongDir + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + flangePlate.Bolt.EdgeDistLongDir + " << " + evmin + ConstUnit.Length + " (NG)");

					ehmin = flangePlate.Bolt.MinEdgeSheared;
					Reporting.AddHeader("Transverse to Beam (et):");
					if (flangePlate.Bolt.EdgeDistTransvDir >= ehmin)
						Reporting.AddLine("= " + flangePlate.Bolt.EdgeDistTransvDir + " >= " + ehmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + flangePlate.Bolt.EdgeDistTransvDir + " << " + ehmin + ConstUnit.Length + " (NG)");

					evmin = MiscCalculationsWithReporting.MinimumEdgeDist(flangePlate.Bolt.BoltSize, Ff / flangePlate.Bolt.NumberOfBolts, beam.Material.Fu * beam.Shape.tf, flangePlate.Bolt.MinEdgeSheared, (int) flangePlate.Bolt.HoleLength, flangePlate.Bolt.HoleType);
					Reporting.AddHeader("Edge Distance on Beam Flange:");
					Reporting.AddLine("Parallel to Beam Axis (el):");
					if (beam.WinConnect.Beam.BoltEdgeDistanceOnFlange >= evmin)
						Reporting.AddLine("= " + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " >= " + evmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " << " + evmin + ConstUnit.Length + " (NG)");

					ehmin = flangePlate.Bolt.MinEdgeRolled;
					Reporting.AddLine("Transverse to Beam (et):");
					BeamTrEdge = (beam.Shape.bf - (beam.GageOnFlange + 2 * flangePlate.TransvBoltSpacingOut)) / 2;
					if (BeamTrEdge >= ehmin)
						Reporting.AddLine("= " + BeamTrEdge + " >= " + ehmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + BeamTrEdge + " << " + ehmin + ConstUnit.Length + " (NG)");

					Cap = flangePlate.Bolt.NumberOfBolts * flangePlate.Bolt.BoltStrength;
					Reporting.AddHeader("Bolt Shear:");
					Reporting.AddLine("Capacity = n * Fv = " + flangePlate.Bolt.NumberOfBolts + " *  " + flangePlate.Bolt.BoltStrength);
					if (Cap >= Ff)
						Reporting.AddLine("= " + Cap + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Cap + " << " + Ff + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Bolt Bearing:");
					Reporting.AddHeader("Bolt Bearing on Plate:");
					Fbe = CommonCalculations.EdgeBearing(flangePlate.Bolt.EdgeDistLongDir, flangePlate.Bolt.HoleLength, flangePlate.Bolt.BoltSize, flangePlate.Material.Fu, flangePlate.Bolt.HoleType, true);
					Fbs = CommonCalculations.SpacingBearing(flangePlate.Bolt.SpacingLongDir, flangePlate.Bolt.HoleLength, flangePlate.Bolt.BoltSize, flangePlate.Bolt.HoleType, flangePlate.Material.Fu, true);
					BearingCap = flangePlate.Bolt.NumberOfLines * (Fbe + Fbs * (flangePlate.Bolt.NumberOfRows - 1)) * flangePlate.BottomThickness;
					Reporting.AddHeader("Capacity = Nt * (Fbe + Fbs * (Nl - 1)) * t");
					Reporting.AddLine("= " + flangePlate.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + flangePlate.Bolt.NumberOfRows + " - 1)) * " + flangePlate.BottomThickness);
					if (BearingCap >= Ff)
						Reporting.AddLine("= " + BearingCap + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + BearingCap + " << " + Ff + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Bolt Bearing on Flange:");
					Fbe = CommonCalculations.EdgeBearing(beam.WinConnect.Beam.BoltEdgeDistanceOnFlange, flangePlate.Bolt.HoleLength, flangePlate.Bolt.BoltSize, beam.Material.Fu, flangePlate.Bolt.HoleType, true);
					Fbs = CommonCalculations.SpacingBearing(flangePlate.Bolt.SpacingLongDir, flangePlate.Bolt.HoleLength, flangePlate.Bolt.BoltSize, flangePlate.Bolt.HoleType, beam.Material.Fu, true);
					BearingCap = flangePlate.Bolt.NumberOfLines * (Fbe + Fbs * (flangePlate.Bolt.NumberOfRows - 1)) * beam.Shape.tf;
					Reporting.AddHeader("Capacity = Nt * (Fbe + Fbs * (Nl - 1)) * t");
					Reporting.AddLine("= " + flangePlate.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + flangePlate.Bolt.NumberOfRows + " - 1)) * " + beam.Shape.tf);
					if (BearingCap >= Ff)
						Reporting.AddLine("= " + BearingCap + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + BearingCap + " << " + Ff + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Plate Tension " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
					Tg = flangePlate.BottomWidth * flangePlate.BottomThickness * ConstNum.FIOMEGA0_9N * flangePlate.Material.Fy;
					Reporting.AddHeader("Tension Yielding:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * b * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + flangePlate.Material.Fy + " * " + flangePlate.BottomWidth + " * " + flangePlate.BottomThickness);
					if (Tg >= Ff)
						Reporting.AddLine("= " + Tg + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Tg + " << " + Ff + ConstUnit.Force + " (NG)");

					bn = flangePlate.BottomWidth - Math.Max(0.15F * flangePlate.BottomWidth, flangePlate.Bolt.NumberOfLines * (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH));
					Tn = bn * flangePlate.BottomThickness * ConstNum.FIOMEGA0_75N * flangePlate.Material.Fu;
					Reporting.AddHeader("Tension Rupture:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Fu * (b - Max(0.15 * b; Nt * (dh + " + ConstNum.SIXTEENTH_INCH + "))) * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + flangePlate.Material.Fu + " * " + "(" + flangePlate.BottomWidth + " - Max(0.15 * " + flangePlate.BottomWidth + " ; " + flangePlate.Bolt.NumberOfLines + " * (" + flangePlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + "))) * " + flangePlate.BottomThickness);
					if (Tn >= Ff)
						Reporting.AddLine("= " + Tn + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Tn + " << " + Ff + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Block shear rupture of the Plate:");
					if (flangePlate.Bolt.NumberOfLines == 4)
					{
						AgTension = (Math.Min(beam.GageOnFlange, 2 * flangePlate.Bolt.EdgeDistTransvDir) + 2 * flangePlate.TransvBoltSpacingOut) * flangePlate.BottomThickness;
						Reporting.AddLine("Agt = (Min(g1, 2 * e) + 2 * g2) * t");
						Reporting.AddLine("= (" + Math.Min(beam.GageOnFlange, 2 * flangePlate.Bolt.EdgeDistTransvDir) + " + 2 * " + flangePlate.TransvBoltSpacingOut + ") * " + flangePlate.BottomThickness);
						Reporting.AddLine("= " + AgTension + ConstUnit.Area);

						Reporting.AddLine(string.Empty);
						AnTension = AgTension - 3 * (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * flangePlate.BottomThickness;
						Reporting.AddLine("Ant = Agt - (n - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
						Reporting.AddLine("= " + AgTension + " - 3 * (" + (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + flangePlate.BottomThickness);
						Reporting.AddLine("= " + AnTension + ConstUnit.Area);
					}
					else
					{
						AgTension = Math.Min(beam.GageOnFlange, flangePlate.BottomWidth - beam.GageOnFlange) * flangePlate.BottomThickness;
						Reporting.AddLine("Agt = Min(g, 2 * e) * t = " + Math.Min(beam.GageOnFlange, flangePlate.BottomWidth - beam.GageOnFlange) + " * " + flangePlate.BottomThickness);
						Reporting.AddLine("= " + AgTension + ConstUnit.Area);

						Reporting.AddLine(string.Empty);
						AnTension = AgTension - (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * flangePlate.BottomThickness;
						Reporting.AddLine("Ant = Agt - (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
						Reporting.AddLine("= " + AgTension + " - (" + (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + flangePlate.BottomThickness);
						Reporting.AddLine("= " + AnTension + ConstUnit.Area);
					}
					Reporting.AddLine(string.Empty);
					AgShear = 2 * ((flangePlate.Bolt.NumberOfRows - 1) * flangePlate.Bolt.SpacingLongDir + flangePlate.Bolt.EdgeDistLongDir) * flangePlate.BottomThickness;
					Reporting.AddLine("Agv = 2 * ((nl - 1) * s + Le) * t");
					Reporting.AddLine("= 2 * ((" + flangePlate.Bolt.NumberOfRows + " - 1) * " + flangePlate.Bolt.SpacingLongDir + " + " + flangePlate.Bolt.EdgeDistLongDir + ") * " + flangePlate.BottomThickness);
					Reporting.AddLine("= " + AgShear + ConstUnit.Area);

					Reporting.AddLine(string.Empty);
					AnShear = AgShear - 2 * (flangePlate.Bolt.NumberOfRows - 0.5) * (flangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * flangePlate.BottomThickness;
					Reporting.AddLine("Anv = Agv - 2 * (nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("=" + AgShear + " - 2*(" + flangePlate.Bolt.NumberOfRows + " - 0.5) * (" + (flangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) + ") *" + flangePlate.BottomThickness);
					Reporting.AddLine("= " + AnShear + ConstUnit.Area);
					FiRn = MiscCalculationsWithReporting.BlockShearNew(flangePlate.Material.Fu, AnShear, 1, AnTension, AgShear, flangePlate.Material.Fy, 0, Ff, true);
					Reporting.AddHeader("Block shear rupture of the Beam Bottom Flange:");

					Reporting.AddLine(string.Empty);
					AgTension = (beam.Shape.bf - beam.GageOnFlange) * beam.Shape.tf;
					Reporting.AddLine("Agt = (bf - g) * t = (" + beam.Shape.bf + " - " + beam.GageOnFlange + ") * " + beam.Shape.tf);
					Reporting.AddLine("= " + AgTension + ConstUnit.Area);

					Reporting.AddLine(string.Empty);
					AnTension = AgTension - (flangePlate.Bolt.NumberOfLines - 1) * (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * beam.Shape.tf;
					Reporting.AddLine("Ant = Agt - (nt - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("= " + AgTension + " - (" + flangePlate.Bolt.NumberOfLines + " - 1) * (" + (flangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + beam.Shape.tf);
					Reporting.AddLine("= " + AnTension + ConstUnit.Area);

					Reporting.AddLine(string.Empty);
					AgShear = 2 * ((flangePlate.Bolt.NumberOfRows - 1) * flangePlate.Bolt.SpacingTransvDir + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange) * beam.Shape.tf;
					Reporting.AddLine("Agv = 2 * ((nl - 1) * s + ef) * t");
					Reporting.AddLine("= 2 * ((" + flangePlate.Bolt.NumberOfRows + " - 1) * " + flangePlate.Bolt.SpacingTransvDir + " + " + flangePlate.Bolt.EdgeDistTransvDir + ") * " + beam.Shape.tf);
					Reporting.AddLine("= " + AgShear + ConstUnit.Area);

					Reporting.AddLine(string.Empty);
					AnShear = AgShear - 2 * (flangePlate.Bolt.NumberOfRows - 0.5) * (flangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * beam.Shape.tf;
					Reporting.AddLine("Anv = Agv - 2 * (nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("=" + AgShear + " - 2 * (" + flangePlate.Bolt.NumberOfRows + " - 0.5) * (" + (flangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) + ") *" + beam.Shape.tf);
					Reporting.AddLine("= " + AnShear + ConstUnit.Area);

					FiRn = MiscCalculationsWithReporting.BlockShearNew(beam.Material.Fu, AnShear, 1, AnTension, AgShear, beam.Material.Fy, 0, Ff, true);

					Reporting.AddHeader("Bottom Plate " + ConstString.DES_OR_ALLOWABLE + " Compressive Strength:");
					sqr12 = Math.Sqrt(12);
					k = 0.65;
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
					{
						if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted && rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
							Reporting.AddLine("Unbraced Length, L = gap + efR + efL = " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
							Reporting.AddLine("Unbraced Length, L = gap + efL = " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback);
							Reporting.AddLine("Unbraced Length, L = gap + efR = " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback);
							Reporting.AddLine("Unbraced Length, L = gap = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
					}
					else
					{
						if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted && rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + efR + efL + bfG");
							Reporting.AddLine("= " + rightBeam.EndSetback + " + " + leftBeam.EndSetback + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + efL + bfG");
							Reporting.AddLine("= " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + efL + bfG");
							Reporting.AddLine("= " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + bfG");
							Reporting.AddLine("= " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
					}

					Reporting.AddHeader("Effective Length Factor, K = 0.65");
					kL_r = (beam.WinConnect.Fema.L * k * sqr12 / flangePlate.BottomThickness);
					Reporting.AddLine("KL / r = k * L / (t / 3.464) = " + k + " * " + beam.WinConnect.Fema.L + " / (" + flangePlate.BottomThickness + " / 3.464) = " + kL_r);
					Fcr = CommonCalculations.BucklingStress(kL_r, flangePlate.Material.Fy, true);

					FiRn = ConstNum.FIOMEGA0_9N * Fcr * flangePlate.BottomWidth * flangePlate.BottomThickness;
					if (FiRn >= Ff)
						Reporting.AddLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + flangePlate.BottomWidth + " * " + flangePlate.BottomThickness + " = " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + flangePlate.BottomWidth + " * " + flangePlate.BottomThickness + " = " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)");
				}
				else
				{
					Reporting.AddHeader("Bottom Plate to Beam Weld");
					w = CommonCalculations.MinimumWeld(flangePlate.BottomThickness, beam.Shape.tf);
					if (beam.Shape.tf <= ConstNum.QUARTER_INCH)
						w1 = beam.Shape.tf;
					else
						w1 = beam.Shape.tf - ConstNum.SIXTEENTH_INCH;
					Reporting.AddHeader("Bottom Plate-to-Flange Weld:");
					Reporting.AddLine("Plate Thickness = " + flangePlate.BottomThickness + ConstUnit.Length + " Beam Flange Thickness = " + beam.Shape.tf + ConstUnit.Length);
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(w) + ConstUnit.Length);
					Reporting.AddLine("Maximum Weld Size = " + CommonCalculations.WeldSize(w1) + ConstUnit.Length);
					if (flangePlate.BottomPlateToBeamWeldSize >= w && flangePlate.BottomPlateToBeamWeldSize <= w1)
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(flangePlate.BottomPlateToBeamWeldSize) + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(flangePlate.BottomPlateToBeamWeldSize) + ConstUnit.Length + " (NG)");

					CommonDataStatic.WeldBetaL = flangePlate.BottomLength - beam.EndSetback - flangePlate.BottomPlateToBeamWeldSize;
					Reporting.AddLine("Welded length of PL, Lw = Lp - c - w");
					Reporting.AddLine("= " + flangePlate.BottomLength + " - " + beam.EndSetback + " - " + flangePlate.BottomPlateToBeamWeldSize + " = " + CommonDataStatic.WeldBetaL + ConstUnit.Length);
					BetaN = CommonCalculations.WeldBetaFactor(CommonDataStatic.WeldBetaL, flangePlate.BottomPlateToBeamWeldSize);
					if (BetaN < 1)
					{
						FiRn = ConstNum.FIOMEGA0_75N * BetaN * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.BottomPlateToBeamWeldSize * 2 * CommonDataStatic.WeldBetaL;
						Reporting.AddLine(ConstNum.BETAS + " = " + BetaN);
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * " + ConstNum.BETAS + " * 0.4242 * Fexx * w * 2* Lw ");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + BetaN + " * 0.4242 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + flangePlate.BottomPlateToBeamWeldSize + " * 2 * " + CommonDataStatic.WeldBetaL);
					}
					else
					{
						FiRn = ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * flangePlate.BottomPlateToBeamWeldSize * 2 * CommonDataStatic.WeldBetaL;
						Reporting.AddLine(ConstNum.BETAS + " = " + BetaN);
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.4242 * Fexx * w * 2 * Lw ");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4242 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + flangePlate.BottomPlateToBeamWeldSize + " * 2 * " + CommonDataStatic.WeldBetaL);
					}
					if (FiRn >= Ff)
						Reporting.AddLine("= " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Plate Width on " + beam.ComponentName);
					a = beam.Shape.bf + 2 * flangePlate.BottomPlateToBeamWeldSize;
					if (flangePlate.BottomWidth >= a)
						Reporting.AddLine("= " + flangePlate.BottomWidth + " >= bf + 2 * w = " + beam.Shape.bf + " + 2 * " + flangePlate.BottomPlateToBeamWeldSize + " = " + a + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + flangePlate.BottomWidth + " << bf + 2 * w = " + beam.Shape.bf + " + 2 * " + flangePlate.BottomPlateToBeamWeldSize + " = " + a + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Bottom Plate Tension Strength:");
					Tg = ConstNum.FIOMEGA0_9N * flangePlate.BottomWidth * flangePlate.BottomThickness * flangePlate.Material.Fy;
					Reporting.AddHeader("Tension Yielding:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * b * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + flangePlate.Material.Fy + " * " + flangePlate.BottomWidth + " * " + flangePlate.BottomThickness);
					if (Tg >= Ff)
						Reporting.AddLine("= " + Tg + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Tg + " << " + Ff + ConstUnit.Force + " (NG)");

					Tn = ConstNum.FIOMEGA0_75N * 0.85 * flangePlate.BottomWidth * flangePlate.BottomThickness * flangePlate.Material.Fu;
					Reporting.AddHeader("Tension Rupture:");
					wLength = flangePlate.BottomLength - beam.EndSetback;
					tmpCaseArg = wLength / beam.Shape.bf;
					if (tmpCaseArg >= 1.5)
						U = 0.85;
					else if (tmpCaseArg >= 1)
						U = 0.75;
					else
						U = 0;
					if (U > 0)
					{
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * U * Fu * b * t");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + U + " * " + flangePlate.Material.Fu + " * " + flangePlate.BottomWidth + " * " + flangePlate.BottomThickness);
						if (Tn >= Ff)
							Reporting.AddLine("= " + Tn + " >= " + Ff + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + Tn + " << " + Ff + ConstUnit.Force + " (NG)");
					}
					else
						Reporting.AddLine("Welded length of the plate << Beam flange width (NG)");

					Reporting.AddHeader("Bottom Plate " + ConstString.DES_OR_ALLOWABLE + " Compressive Strength:");
					sqr12 = Math.Sqrt(12);
					k = 0.65;
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
					{
						if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted && rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
							Reporting.AddLine("Unbraced Length (L) = gap + efR + efL = " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
							Reporting.AddLine("Unbraced Length (L) = gap + efL = " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback);
							Reporting.AddLine("Unbraced Length (L) = gap + efR = " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback);
							Reporting.AddLine("Unbraced Length (L) = gap = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
					}
					else
					{
						if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted && rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + efR + efL + bfG");
							Reporting.AddLine("= " + rightBeam.EndSetback + " + " + leftBeam.EndSetback + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (leftBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + efL + bfG");
							Reporting.AddLine("= " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + leftBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else if (rightBeam.WinConnect.MomentFlangePlate.Connection == EConnectionStyle.Bolted)
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + leftBeam.EndSetback + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + efL + bfG");
							Reporting.AddLine("= " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + rightBeam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}
						else
						{
							beam.WinConnect.Fema.L = (rightBeam.EndSetback + leftBeam.EndSetback + column.Shape.bf);
							Reporting.AddLine("Unbraced Length, L = cR + cL + bfG");
							Reporting.AddLine("= " + (rightBeam.EndSetback + leftBeam.EndSetback) + " + " + column.Shape.bf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
						}

					}
					Reporting.AddHeader("Effective Length Factor (K) = 0.65");
					kL_r = (beam.WinConnect.Fema.L * k * sqr12 / flangePlate.BottomThickness);

					Reporting.AddLine("KL / r = k * L / (t / 3.464) = " + k + " * " + beam.WinConnect.Fema.L + " / (" + flangePlate.BottomThickness + " / 3.464) = " + kL_r);

					Fcr = CommonCalculations.BucklingStress(kL_r, flangePlate.Material.Fy, true);

					FiRn = ConstNum.FIOMEGA0_9N * Fcr * flangePlate.BottomWidth * flangePlate.BottomThickness;
					if (FiRn >= Ff)
						Reporting.AddLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + flangePlate.BottomWidth + " * " + flangePlate.BottomThickness + " = " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + flangePlate.BottomWidth + " * " + flangePlate.BottomThickness + " = " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)");
				}

			}
			else
			{
				Reporting.AddHeader("Bottom Flange to Girder Weld:");
				if (!flangePlate.BottomPlateToSupportWeldSize_User || flangePlate.BottomPlateToSupportWeldSize == 0)
					flangePlate.BottomPlateToSupportWeldSize = NumberFun.Round(Ff / (0.2121 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * beam.Shape.bf * 2), 16);
				RequiredWeldSize = Ff / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * beam.Shape.bf * 2);
				Reporting.AddHeader("Required Fillet Weld Size ");
				Reporting.AddLine("= Ff / (" + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx * b * 2)");
				Reporting.AddLine("= " + Ff + " / (" + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + beam.Shape.bf + " * 2) ");
				Reporting.AddLine("= " + RequiredWeldSize + ConstUnit.Length);
				FullPenCap = 0.6 * beam.Material.Fy * beam.Shape.bf * beam.Shape.tf;
				if (RequiredWeldSize <= NumberFun.ConvertFromFraction(5))
				{
					Reporting.AddHeader("Use " + CommonCalculations.WeldSize(flangePlate.BottomPlateToSupportWeldSize) + ConstUnit.Length + "  Fillet Weld ");
					Reporting.AddHeader("on Both Sides of Flange.");
				}
				else if (Ff <= FullPenCap)
					Reporting.AddHeader("Use Full Penetration Weld.");
				else
				{
					RequiredreinfSize = (Ff - FullPenCap) / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * beam.Shape.bf * 2);
					if (!flangePlate.BottomPlateToSupportWeldSize_User || flangePlate.BottomPlateToSupportWeldSize == 0)
						flangePlate.BottomPlateToSupportWeldSize = NumberFun.Round(Math.Max(RequiredreinfSize, beam.Shape.tf / 2), 16);
					Reporting.AddHeader("Use Full Penetration Weld with " + CommonCalculations.WeldSize(flangePlate.BottomPlateToSupportWeldSize) + ConstUnit.Length);
					Reporting.AddHeader("Reinforcement on Both Sides of Flange");
				}
			}
		}
	}
}