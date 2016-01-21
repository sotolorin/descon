using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class FlangePlatesToWeb
	{
		public static void DesignFlangePlatesToWeb(EMemberType memberType)
		{
			string OneorTwo = "";
			int OneorTwoN = 0;
			double WeldLf = 0;
			double capacity = 0;
			double BetaN = 0;
			double TplateMax = 0;
			double TplateMin = 0;
			double sqr12 = 0;
			double AnShear = 0;
			double AgShear = 0;
			double AnTension = 0;
			double AgTension = 0;
			double Tn = 0;
			double bn = 0;
			double Tg = 0;
			double Cap = 0;
			double BeamTrEdge = 0;
			double ehmin = 0;
			double evmin = 0;
			double spPlate = 0;
			double spBeam = 0;
			double RLoadL = 0;
			double RLoadR = 0;
			double lweb = 0;
			double FiRn = 0;
			double Fcr = 0;
			double kL_r = 0;
			double k = 0;
			double WidthforBuckling = 0;
			double widthEnd = 0;
			double widthC = 0;
			double BearingCap = 0;
			double Fbs = 0;
			double si = 0;
			double Fbe = 0;
			double ratio = 0;
			double B = 0;
			double WebWeld = 0;
			double wm = 0;
			double FforShear = 0;
			double Lflange = 0;
			double colshear = 0;
			double wminW = 0;
			double wminf = 0;
			double U = 0;
			double Lmax = 0;
			double Lcheck = 0;
			double weldlength2 = 0;
			double L2 = 0;
			double weldlength1 = 0;
			double WeldAreaReq = 0;
			double RpL = 0;
			double weldlength = 0;
			double tForNetArea = 0;
			double maxwidth = 0;
			double tToDevelopSupWeld = 0;
			double tbReq = 0;
			double ef = 0;
			double tReq_BlockShear = 0;
			double LnShear = 0;
			double LgShear = 0;
			double LnTension = 0;
			double LnTensionO = 0;
			double LgTensionO = 0;
			double LnTensionI = 0;
			double LgTensionI = 0;
			double tnReq = 0;
			double HoleSize = 0;
			double tgReq = 0;
			double PlateWidthAtTaper = 0;
			double t2 = 0;
			double t1 = 0;
			double Anreq = 0;
			double AgReq = 0;
			double re = 0;
			double Ff = 0;
			double Rload = 0;
			double w = 0;
			double w1 = 0;
			double MinWidth = 0;
			double welds = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			var momentDirectWeld = beam.WinConnect.MomentDirectWeld;

			Rload = Math.Abs(beam.P / 2) + Math.Abs(beam.Moment / beam.Shape.d);

			Ff = Rload;
			switch (momentDirectWeld.Type)
			{
				case ENumbers.Zero:
				case ENumbers.One:
				case ENumbers.Two:			
					momentDirectWeld.Bolt.NumberOfRows = (int)Math.Ceiling(Rload / momentDirectWeld.Bolt.BoltStrength / momentDirectWeld.Bolt.NumberOfLines);
					momentDirectWeld.Bolt.NumberOfBolts = momentDirectWeld.Bolt.NumberOfRows * momentDirectWeld.Bolt.NumberOfLines;
					re = Math.Max(momentDirectWeld.Top.et, momentDirectWeld.Bolt.MinEdgeSheared);
					BeamGage(memberType);
					
					if (momentDirectWeld.Bolt.NumberOfLines == 4)
						momentDirectWeld.Top.b = beam.Shape.g1 + 2 * (momentDirectWeld.Top.g1 + re);
					else
						momentDirectWeld.Top.b = beam.Shape.g1 + 2 * re;
					
					momentDirectWeld.Bottom.b = momentDirectWeld.Top.b;
					AgReq = Rload / (ConstNum.FIOMEGA0_9N * momentDirectWeld.Material.Fy);
					Anreq = Rload / (ConstNum.FIOMEGA0_75N * momentDirectWeld.Material.Fu);
					t1 = momentDirectWeld.Top.b;
					t2 = column.Shape.d - 2 * column.Shape.tf;
					momentDirectWeld.Bottom.Length = (momentDirectWeld.Bolt.NumberOfRows - 1) * momentDirectWeld.Bolt.SpacingLongDir + momentDirectWeld.Bolt.EdgeDistLongDir + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + beam.EndSetback;
					momentDirectWeld.Top.Length = momentDirectWeld.Bottom.Length;
					PlateWidthAtTaper = t2 + (t1 - t2) / momentDirectWeld.Bottom.Length * (beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + beam.EndSetback); // 11.075
					tgReq = AgReq / PlateWidthAtTaper;
					HoleSize = momentDirectWeld.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH;
					tnReq = Anreq / (PlateWidthAtTaper - Math.Max(0.15 * PlateWidthAtTaper, momentDirectWeld.Bolt.NumberOfLines * HoleSize));
					
					if (momentDirectWeld.Bolt.NumberOfLines == 4)
					{
						LgTensionI = beam.Shape.g1 + 2 * momentDirectWeld.Top.g1; // inside block
						LnTensionI = LgTensionI - 3 * (momentDirectWeld.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
						LgTensionO = PlateWidthAtTaper - beam.Shape.g1;
						LnTensionO = LgTensionO - 3 * (momentDirectWeld.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
					}
					else
					{
						LgTensionI = beam.Shape.g1; // inside block
						LnTensionI = LgTensionI - (momentDirectWeld.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
						LgTensionO = PlateWidthAtTaper - beam.Shape.g1;
						LnTensionO = LgTensionO - (momentDirectWeld.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
					}
					
					LnTension = Math.Min(LnTensionI, LnTensionO);
					LgShear = 2 * ((momentDirectWeld.Bolt.NumberOfRows - 1) * momentDirectWeld.Bolt.SpacingLongDir + momentDirectWeld.Bolt.EdgeDistLongDir);
					LnShear = LgShear - 2 * (momentDirectWeld.Bolt.NumberOfRows - 0.5) * (momentDirectWeld.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
					tReq_BlockShear = Rload / MiscCalculationsWithReporting.BlockShearNew(momentDirectWeld.Material.Fu, LnShear, 1, LnTension, LgShear, momentDirectWeld.Material.Fy, 1, 0, false);
					BearingThickness(memberType, ref Fbe, ref si, ref Fbs, ref BearingCap, Rload, ref tbReq);
					ef = MiscCalculationsWithReporting.MinimumEdgeDist(momentDirectWeld.Bolt.BoltSize, Rload / momentDirectWeld.Bolt.NumberOfBolts, beam.Material.Fu * beam.Shape.tf, momentDirectWeld.Bolt.MinEdgeSheared, momentDirectWeld.Bolt.HoleLength, momentDirectWeld.Bolt.HoleType);
					
					if (!beam.WinConnect.Beam.BoltEdgeDistanceOnFlange_User)
						beam.WinConnect.Beam.BoltEdgeDistanceOnFlange = Math.Max(ef, beam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
					momentDirectWeld.Bolt.EdgeDistBrace = beam.WinConnect.Beam.BoltEdgeDistanceOnFlange;
					BucklingThickness(memberType, ref tgReq, ref widthC, ref widthEnd, ref WidthforBuckling, false, ref k, ref kL_r, ref Fcr, ref FiRn, Rload);
					momentDirectWeld.Top.FlangeThickness = CommonCalculations.PlateThickness(Math.Max(Math.Max(tgReq, tnReq), Math.Max(tbReq, tReq_BlockShear)));
					break;
				default:
					AgReq = Rload / (ConstNum.FIOMEGA0_9N * momentDirectWeld.Material.Fy);
					momentDirectWeld.Top.b = ((int) Math.Floor(8 * (beam.Shape.bf - 3 * beam.Shape.tf))) / 8.0;
					momentDirectWeld.Bottom.b = momentDirectWeld.Top.b;
					maxwidth = beam.Shape.bf - 2 * beam.Shape.tf;
					tgReq = AgReq / momentDirectWeld.Top.b;
					tForNetArea = Rload / (ConstNum.FIOMEGA0_75N * momentDirectWeld.Material.Fu * 0.85F * momentDirectWeld.Top.b);
					tgReq = Math.Max(tgReq, tForNetArea);
					momentDirectWeld.Top.Length = momentDirectWeld.Top.b + momentDirectWeld.Top.a;
					momentDirectWeld.Top.Length = momentDirectWeld.Top.b;
					BucklingThickness(memberType, ref tgReq, ref widthC, ref widthEnd, ref WidthforBuckling, true, ref k, ref kL_r, ref Fcr, ref FiRn, Rload);
					momentDirectWeld.Top.FlangeThickness = CommonCalculations.PlateThickness(Math.Max(momentDirectWeld.Top.FlangeThickness, tgReq));
					AgReq = Rload / (ConstNum.FIOMEGA0_9N * momentDirectWeld.Material.Fy);
					momentDirectWeld.Bottom.b = ((int) Math.Floor(8 * (beam.Shape.bf + 2 * beam.Shape.tf))) / 8.0;
					tgReq = AgReq / momentDirectWeld.Bottom.b;
					tForNetArea = Rload / (ConstNum.FIOMEGA0_75N * momentDirectWeld.Material.Fu * 0.85F * momentDirectWeld.Bottom.b);
					tgReq = Math.Max(tgReq, tForNetArea);
					momentDirectWeld.Bottom.Length = momentDirectWeld.Bottom.b + momentDirectWeld.Bottom.a;
					momentDirectWeld.Bottom.Length = momentDirectWeld.Bottom.b;
					BucklingThickness(memberType, ref tgReq, ref widthC, ref widthEnd, ref WidthforBuckling, false, ref k, ref kL_r, ref Fcr, ref FiRn, Rload);
					momentDirectWeld.Bottom.FlangeThickness = tgReq;
					SupportWeld(memberType, ref wminf, ref wminW, ref Lflange, ref welds, Rload, ref wm, ref lweb, ref FforShear, ref WebWeld);
					momentDirectWeld.Bottom.Length = beam.Shape.bf;
					MinWidth = beam.Shape.bf + 2 * beam.Shape.tf;
					momentDirectWeld.Bottom.b = NumberFun.Round(MinWidth, 8);
					momentDirectWeld.Top.FlangeThickness = CommonCalculations.PlateThickness(Math.Max(momentDirectWeld.Top.FlangeThickness, tgReq));
					do
					{
						momentDirectWeld.Top.b = (Math.Floor(8 * maxwidth)) / 8;
						tgReq = AgReq / momentDirectWeld.Top.b;
						momentDirectWeld.Top.FlangeThickness = CommonCalculations.PlateThickness(Math.Max(momentDirectWeld.Top.FlangeThickness, tgReq));
						w = CommonCalculations.MinimumWeld(momentDirectWeld.Top.FlangeThickness, beam.Shape.tf);
						if (momentDirectWeld.Top.FlangeThickness <= ConstNum.QUARTER_INCH)
							w1 = momentDirectWeld.Top.FlangeThickness;
						else
							w1 = momentDirectWeld.Top.FlangeThickness - ConstNum.SIXTEENTH_INCH;

						if (w1 < w && w1 < momentDirectWeld.Top.FlangeThickness)
							w1 = momentDirectWeld.Top.FlangeThickness;
						momentDirectWeld.Top.FilletWeldW3 = Math.Ceiling(Math.Min(w, w1));
						maxwidth = beam.Shape.bf - 2 * (momentDirectWeld.Top.FilletWeldW3 + ConstNum.QUARTER_INCH);
					} while (maxwidth < momentDirectWeld.Top.b);
					weldlength = Math.Max(Rload / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentDirectWeld.Top.FilletWeldW3), 3 * momentDirectWeld.Top.b);
					if (weldlength < 2 * momentDirectWeld.Top.b)
						weldlength = 2 * momentDirectWeld.Top.b;
					do
					{
						RpL = Rload / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentDirectWeld.Top.FilletWeldW3);
						WeldAreaReq = (Rload / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx) - momentDirectWeld.Top.FilletWeldW3 * momentDirectWeld.Top.b) / 2; // one line on the side of the plate
						if (WeldAreaReq < momentDirectWeld.Top.FilletWeldW3 * momentDirectWeld.Top.b)
							WeldAreaReq = momentDirectWeld.Top.FilletWeldW3 * momentDirectWeld.Top.b;

						beam.WinConnect.Fema.L1 = ((RpL - momentDirectWeld.Top.b) / 2);
						if (beam.WinConnect.Fema.L1 <= 0)
							beam.WinConnect.Fema.L1 = (RpL / 2);

						weldlength1 = (2 * beam.WinConnect.Fema.L1 + momentDirectWeld.Top.b);
						if (beam.WinConnect.Fema.L1 <= (5 / 3 * momentDirectWeld.Top.b))
							weldlength1 = (2 * beam.WinConnect.Fema.L1 + momentDirectWeld.Top.b);
						else
						{
							L2 = (RpL - 1.5 * momentDirectWeld.Top.b) / 1.7;
							if (L2 <= 0)
								L2 = RpL / 1.7;
							weldlength2 = 2 * L2 + momentDirectWeld.Top.b;
						}

						weldlength = Math.Max(weldlength2, weldlength1);
						Lcheck = Math.Max(beam.WinConnect.Fema.L1, L2);
						Lmax = 10 * Math.Pow(WeldAreaReq, 0.5);
						if (Lmax < Lcheck)
							momentDirectWeld.Top.FilletWeldW3 = momentDirectWeld.Top.FilletWeldW3 + ConstNum.SIXTEENTH_INCH;
					} while (!(Lcheck <= Lmax));

					momentDirectWeld.Top.Length = NumberFun.Round((weldlength - momentDirectWeld.Top.b) / 2 + momentDirectWeld.Top.a + momentDirectWeld.Top.FilletWeldW3, 8);
					momentDirectWeld.Top.Length = momentDirectWeld.Top.Length - momentDirectWeld.Top.a;
					AgReq = Rload / (ConstNum.FIOMEGA0_9N * momentDirectWeld.Material.Fy);
					tToDevelopSupWeld = Rload / (ConstNum.FIOMEGA1_0N * 0.6 * momentDirectWeld.Material.Fy * 2 * (column.Shape.bf / 2 - column.Shape.k1));
					tgReq = Math.Max(AgReq / momentDirectWeld.Bottom.b, tToDevelopSupWeld);
					MinWidth = beam.Shape.bf + 2 * beam.Shape.tf;
					
					do
					{
						momentDirectWeld.Bottom.b = NumberFun.Round(MinWidth, 8);
						BucklingThickness(memberType, ref tgReq, ref widthC, ref widthEnd, ref WidthforBuckling, false, ref k, ref kL_r, ref Fcr, ref FiRn, Rload);
						momentDirectWeld.Bottom.FlangeThickness = Math.Max(momentDirectWeld.Bottom.FlangeThickness, tgReq);

						w = CommonCalculations.MinimumWeld(momentDirectWeld.Bottom.FlangeThickness, beam.Shape.tf);
						if (beam.Shape.tf <= ConstNum.QUARTER_INCH)
							w1 = Math.Floor(beam.Shape.tf);
						else
							w1 = beam.Shape.tf - ConstNum.SIXTEENTH_INCH;

						if (w1 < w && w1 < beam.Shape.tf)
							w1 = beam.Shape.tf;

						momentDirectWeld.Bottom.FilletWeldW3 = Math.Min(w, w1);
						weldlength = Rload / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentDirectWeld.Bottom.FilletWeldW3);

						do
						{
							weldlength = Rload / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentDirectWeld.Bottom.FilletWeldW3);
							WeldAreaReq = Rload / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx) / 2; // One line of weld
							Lcheck = weldlength / 2;
							Lmax = 10 * Math.Pow(WeldAreaReq, 0.5);
							if (Lmax < Lcheck)
								momentDirectWeld.Bottom.FilletWeldW3 = momentDirectWeld.Bottom.FilletWeldW3 + ConstNum.SIXTEENTH_INCH;
						} while (!(Lcheck <= Lmax));

						if (weldlength < 2 * beam.Shape.bf)
							weldlength = 2 * beam.Shape.bf;

						if (weldlength >= 4 * beam.Shape.bf)
							U = 0.85;
						else if (weldlength >= 3 * beam.Shape.bf)
							U = 0.85;
						else if (weldlength >= 2 * beam.Shape.bf)
							U = 0.75;

						tForNetArea = Rload / (ConstNum.FIOMEGA0_75N * U * beam.Material.Fu * momentDirectWeld.Bottom.b);
						if (momentDirectWeld.Bottom.FlangeThickness < tForNetArea)
							momentDirectWeld.Bottom.FlangeThickness = Math.Max(momentDirectWeld.Bottom.FlangeThickness, tForNetArea);
						MinWidth = beam.Shape.bf + 2 * (momentDirectWeld.Bottom.FilletWeldW3 + ConstNum.QUARTER_INCH);
					} while (MinWidth > momentDirectWeld.Bottom.b);

					momentDirectWeld.Bottom.Length = NumberFun.Round(weldlength / 2 + momentDirectWeld.Bottom.a + momentDirectWeld.Bottom.FilletWeldW3, 8); // Int(-8 * (weldlength / 2 + BMWAtt.a + BMWAtt.BSideW)) / 8
					momentDirectWeld.Bottom.Length = momentDirectWeld.Bottom.Length - momentDirectWeld.Bottom.a;
					break;
			}

			SupportWeld(memberType, ref wminf, ref wminW, ref Lflange, ref welds, Rload, ref wm, ref lweb, ref FforShear, ref WebWeld);
			momentDirectWeld.Top.FlangeThickness = CommonCalculations.PlateThickness(momentDirectWeld.Top.FlangeThickness);
			momentDirectWeld.Bottom.FlangeThickness = CommonCalculations.PlateThickness(momentDirectWeld.Bottom.FlangeThickness);
			wminf = CommonCalculations.MinimumWeld(momentDirectWeld.Top.FlangeThickness, column.Shape.tf);
			wminW = CommonCalculations.MinimumWeld(momentDirectWeld.Top.FlangeThickness, column.Shape.tw);
			
			if (momentDirectWeld.WeldFlangeType == EWeldType.CJP)
			{
				colshear = 2 * column.Shape.tf * Lflange * ConstNum.FIOMEGA1_0N * 0.6 * column.Material.Fy;
				if (colshear < FforShear)
					Reporting.AddLine("Column flange thickness cannot develop the force delivered by plate.");
			}
			else
			{
				wm = column.Shape.tf * column.Material.Fu / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				if (welds > wm)
					Reporting.AddLine("Column flange thickness cannot develop weld Strength.");
				welds = Math.Max(welds, wminf);
			}

			momentDirectWeld.Top.FilletWeldW1 = NumberFun.Round(welds, 16);
			if ((!rightBeam.IsActive || !leftBeam.IsActive) && (momentDirectWeld.Type == ENumbers.Four || momentDirectWeld.Type == ENumbers.One))
				momentDirectWeld.Top.FilletWeldW2 = NumberFun.Round(Math.Max(WebWeld, wminW), 16);
			else if (momentDirectWeld.Type != ENumbers.Two && momentDirectWeld.Type != ENumbers.Five)
				momentDirectWeld.Top.FilletWeldW2 = wminW;
			else
				momentDirectWeld.Top.FilletWeldW2 = 0;

			momentDirectWeld.Bottom.FilletWeldW2 = momentDirectWeld.Top.FilletWeldW2;
			beam.WinConnect.Beam.BotAttachThick = momentDirectWeld.Bottom.FlangeThickness;
			beam.WinConnect.Beam.TopAttachThick = momentDirectWeld.Top.FlangeThickness;
			if (momentDirectWeld.Type == ENumbers.Four || momentDirectWeld.Type == ENumbers.One)
			{
				B = (column.Shape.bf - column.Shape.tw) / 2;
				ratio = 0.56 * Math.Pow(ConstNum.ELASTICITY / momentDirectWeld.Material.Fy, 0.5);
				momentDirectWeld.Top.StiffenerThickness = Math.Max(momentDirectWeld.Top.FlangeThickness, NumberFun.Round(B / ratio, 16));
				momentDirectWeld.Bottom.StiffenerThickness = momentDirectWeld.Top.StiffenerThickness;
			}

			if (momentDirectWeld.Type == ENumbers.Zero || momentDirectWeld.Type == ENumbers.One ||
			    momentDirectWeld.Type == ENumbers.Two)
				momentDirectWeld.Bottom = momentDirectWeld.Top;

			if (CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn && CommonDataStatic.ColumnStiffener.TopOfBeamToColumn < Math.Max(momentDirectWeld.Top.FlangeThickness, momentDirectWeld.Top.ExtensionThickness))
				CommonDataStatic.ColumnStiffener.TopOfBeamToColumn = Math.Max(momentDirectWeld.Top.FlangeThickness, momentDirectWeld.Top.ExtensionThickness);

			Reporting.AddHeader(beam.ComponentName + " - " + beam.ShapeName + " - " + beam.Material.Name);
			Reporting.AddHeader("Moment Connection Using Flange Plate:");
			Reporting.AddLine("Flange Force (Ff):");
			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb || CommonDataStatic.BeamToColumnType == EJointConfiguration.ColumnSplice ||
			    CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
			{
				Ff = Math.Abs(beam.P / 2) + Math.Abs(beam.Moment / beam.Shape.d);
				Rload = Ff;
				if (CommonDataStatic.Units == EUnit.US)
					Reporting.AddLine("= P / 2 + M / d = " + Math.Abs(beam.P) + " / 2 + " + Math.Abs(beam.Moment) + " / " + beam.Shape.d);
				else
					Reporting.AddLine("= P / 2 + M / d = " + Math.Abs(beam.P) + " / 2 + " + Math.Abs(beam.Moment) + " / " + beam.Shape.d);

				Reporting.AddLine("= " + Ff + ConstUnit.Force);
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
				return;
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
			{
				RLoadR = Math.Abs(rightBeam.P / 2) + Math.Abs(rightBeam.Moment / rightBeam.Shape.d);
				RLoadL = Math.Abs(leftBeam.P / 2) + Math.Abs(leftBeam.Moment / leftBeam.Shape.d);
				Ff = Math.Max(RLoadR, RLoadL);
				Rload = Ff;
				if (CommonDataStatic.Units == EUnit.US)
				{
					Reporting.AddLine("= P / 2 + M / d");
					Reporting.AddLine("= " + Math.Abs(rightBeam.P) + " / 2 + " + Math.Abs(rightBeam.Moment) + " / " + rightBeam.Shape.d + " = " + RLoadR + ConstUnit.Force + " (Right Side)");
					Reporting.AddLine("= " + Math.Abs(leftBeam.P) + " / 2 + " + Math.Abs(leftBeam.Moment) + " / " + leftBeam.Shape.d + " = " + RLoadL + ConstUnit.Force + " (Left Side)");
				}
				else
				{
					Reporting.AddLine("= P / 2 + M / d");
					Reporting.AddLine("= " + Math.Abs(rightBeam.P) + " / 2 +  " + Math.Abs(rightBeam.Moment) + " / " + rightBeam.Shape.d + " = " + RLoadR + ConstUnit.Force + " (Right Side)");
					Reporting.AddLine("= " + Math.Abs(leftBeam.P) + " / 2 + " + Math.Abs(leftBeam.Moment) + " / " + leftBeam.Shape.d + " = " + RLoadL + ConstUnit.Force + " (Left Side)");
				}
				Reporting.AddLine("Use Ff = " + Ff + ConstUnit.Force);
			}

			Reporting.AddLine("Top Plate: " + momentDirectWeld.Top.b + ConstUnit.Length + " to " + (column.Shape.d - 2 * column.Shape.tf) + ConstUnit.Length + " X " + (momentDirectWeld.Top.Length + 0.5 * (column.Shape.bf - column.Shape.tw)) + ConstUnit.Length + " X " + momentDirectWeld.Top.FlangeThickness + ConstUnit.Length);
			Reporting.AddLine("Bottom Plate: " + momentDirectWeld.Bottom.b + ConstUnit.Length + " to " + (column.Shape.d - 2 * column.Shape.tf) + ConstUnit.Length + " X " + (momentDirectWeld.Bottom.Length + 0.5 * (column.Shape.bf - column.Shape.tw)) + ConstUnit.Length + " X " + momentDirectWeld.Bottom.FlangeThickness + ConstUnit.Length);
			Reporting.AddLine("Plate Material: " + momentDirectWeld.Material.Name);
			if (momentDirectWeld.Type == ENumbers.Zero || momentDirectWeld.Type == ENumbers.One ||
			    momentDirectWeld.Type == ENumbers.Two)
			{
				momentDirectWeld.Bolt.NumberOfBolts = momentDirectWeld.Bolt.NumberOfRows * momentDirectWeld.Bolt.NumberOfLines;
				Reporting.AddLine("Bolts on Flange: " + momentDirectWeld.Bolt.NumberOfBolts + " Bolts - " + momentDirectWeld.Bolt.BoltName + " in " + momentDirectWeld.Bolt.NumberOfLines + " Lines");
				Reporting.AddLine("Bolt Holes on Plate: " + momentDirectWeld.Bolt.HoleWidth + ConstUnit.Length + " Lateral X " + momentDirectWeld.Bolt.HoleLength + ConstUnit.Length + "  Longitudinal");
				Reporting.AddLine("Bolt Holes on Flange: " + momentDirectWeld.Bolt.HoleWidth + ConstUnit.Length + " Lateral X " + momentDirectWeld.Bolt.HoleLength + ConstUnit.Length + "  Longitudinal");

				MiscCalculationsWithReporting.BeamCheck(beam.MemberType);
				Reporting.AddHeader("Bolt Spacing and Edge Distance:");
				spBeam = MiscCalculationsWithReporting.MinimumSpacing((momentDirectWeld.Bolt.BoltSize), Rload / momentDirectWeld.Bolt.NumberOfBolts, beam.Material.Fu * beam.Shape.tf, momentDirectWeld.Bolt.HoleLength, momentDirectWeld.Bolt.HoleType);
				spPlate = MiscCalculationsWithReporting.MinimumSpacing((momentDirectWeld.Bolt.BoltSize), Rload / momentDirectWeld.Bolt.NumberOfBolts, momentDirectWeld.Material.Fu * momentDirectWeld.Top.FlangeThickness, momentDirectWeld.Bolt.HoleLength, momentDirectWeld.Bolt.HoleType);
				momentDirectWeld.Bolt.MinSpacing = Math.Max(spBeam, spPlate);

				evmin = MiscCalculationsWithReporting.MinimumEdgeDist(momentDirectWeld.Bolt.BoltSize, Rload / momentDirectWeld.Bolt.NumberOfBolts, momentDirectWeld.Material.Fu * momentDirectWeld.Top.FlangeThickness, momentDirectWeld.Bolt.MinEdgeSheared, (int) momentDirectWeld.Bolt.HoleLength, momentDirectWeld.Bolt.HoleType);
				Reporting.AddHeader("Edge Distance on Plate Parallel to Beam Axis (el):");
				if (momentDirectWeld.Bolt.EdgeDistLongDir >= evmin)
					Reporting.AddLine("= " + momentDirectWeld.Bolt.EdgeDistLongDir + " >= " + evmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + momentDirectWeld.Bolt.EdgeDistLongDir + " << " + evmin + ConstUnit.Length + " (NG)");

				ehmin = momentDirectWeld.Bolt.MinEdgeSheared;
				Reporting.AddHeader("Edge Distance on Plate Transverse to Beam (et):");
				if (momentDirectWeld.Bolt.EdgeDistTransvDir >= ehmin)
					Reporting.AddLine("= " + momentDirectWeld.Bolt.EdgeDistTransvDir + " >= " + ehmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + momentDirectWeld.Bolt.EdgeDistTransvDir + " << " + ehmin + ConstUnit.Length + " (NG)");

				evmin = MiscCalculationsWithReporting.MinimumEdgeDist(momentDirectWeld.Bolt.BoltSize, Rload / momentDirectWeld.Bolt.NumberOfBolts, beam.Material.Fu * beam.Shape.tf, momentDirectWeld.Bolt.MinEdgeSheared, (int) momentDirectWeld.Bolt.HoleLength, momentDirectWeld.Bolt.HoleType);
				Reporting.AddHeader("Edge Distance on Beam Parallel to Beam Axis (el):");
				if (momentDirectWeld.Bolt.EdgeDistBrace >= evmin)
					Reporting.AddLine("= " + momentDirectWeld.Bolt.EdgeDistBrace + " >= " + evmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + momentDirectWeld.Bolt.EdgeDistBrace + " << " + evmin + ConstUnit.Length + " (NG)");

				ehmin = momentDirectWeld.Bolt.MinEdgeRolled;
				Reporting.AddHeader("Edge Distance on Beam Transverse to Beam (et):");
				if (momentDirectWeld.Bolt.NumberOfLines == 4)
					BeamTrEdge = (beam.Shape.bf - beam.Shape.g1) / 2;
				else
					BeamTrEdge = (beam.Shape.bf - (beam.Shape.g1 + 2 * momentDirectWeld.Top.g1)) / 2;

				if (BeamTrEdge >= ehmin)
					Reporting.AddLine("= " + BeamTrEdge + " >= " + ehmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + BeamTrEdge + " << " + ehmin + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Bolt Shear and Bearing:");
				Cap = momentDirectWeld.Bolt.NumberOfBolts * momentDirectWeld.Bolt.BoltStrength;
				if (Cap >= Ff)
				{
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Shear Strength of Bolts = n * Fv");
					Reporting.AddCapacityLine("= " + momentDirectWeld.Bolt.NumberOfBolts + " *  " + momentDirectWeld.Bolt.BoltStrength + " =  " + Cap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Cap, "Bolt Shear and Bearing", memberType);
				}
				else
				{
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Shear Strength of Bolts = n * Fv");
					Reporting.AddCapacityLine("= " + momentDirectWeld.Bolt.NumberOfBolts + " *  " + momentDirectWeld.Bolt.BoltStrength + " =  " + Cap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Cap, "Bolt Shear and Bearing", memberType);
				}

				Reporting.AddHeader("Bolt Bearing on Plate:");
				Fbe = CommonCalculations.EdgeBearing(momentDirectWeld.Bolt.EdgeDistLongDir, momentDirectWeld.Bolt.HoleLength, momentDirectWeld.Bolt.BoltSize, momentDirectWeld.Material.Fu, momentDirectWeld.Bolt.HoleType, true);
				si = momentDirectWeld.Bolt.SpacingLongDir;
				Fbs = CommonCalculations.SpacingBearing(momentDirectWeld.Bolt.SpacingLongDir, momentDirectWeld.Bolt.HoleLength, momentDirectWeld.Bolt.BoltSize, momentDirectWeld.Bolt.HoleType, momentDirectWeld.Material.Fu, true);
				BearingCap = momentDirectWeld.Bolt.NumberOfLines * (Fbe + Fbs * (momentDirectWeld.Bolt.NumberOfRows - 1)) * momentDirectWeld.Top.FlangeThickness;
				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = Nt * (Fbe + Fbs * (Nl - 1)) * t");
				if (BearingCap >= Ff)
				{
					Reporting.AddLine("= " + momentDirectWeld.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + momentDirectWeld.Bolt.NumberOfRows + " - 1)) * " + momentDirectWeld.Top.FlangeThickness);
					Reporting.AddCapacityLine("= " + BearingCap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / BearingCap, "Bearing Strength", memberType);
				}
				else
				{
					Reporting.AddLine("= " + momentDirectWeld.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + momentDirectWeld.Bolt.NumberOfRows + " - 1)) * " + momentDirectWeld.Top.FlangeThickness);
					Reporting.AddCapacityLine("= " + BearingCap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / BearingCap, "Bearing Strength", memberType);
				}

				Reporting.AddHeader("Bolt Bearing on Flange:");
				Fbe = CommonCalculations.EdgeBearing(beam.WinConnect.Beam.BoltEdgeDistanceOnFlange, momentDirectWeld.Bolt.HoleLength, momentDirectWeld.Bolt.BoltSize, beam.Material.Fu, momentDirectWeld.Bolt.HoleType, true);
				si = momentDirectWeld.Bolt.SpacingLongDir;
				Fbs = CommonCalculations.SpacingBearing(momentDirectWeld.Bolt.SpacingLongDir, momentDirectWeld.Bolt.HoleLength, momentDirectWeld.Bolt.BoltSize, momentDirectWeld.Bolt.HoleType, beam.Material.Fu, true);
				BearingCap = momentDirectWeld.Bolt.NumberOfLines * (Fbe + Fbs * (momentDirectWeld.Bolt.NumberOfRows - 1)) * beam.Shape.tf;
				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = Nt * (Fbe + Fbs * (Nl - 1)) * t");
				if (BearingCap >= Ff)
				{
					Reporting.AddLine("= " + momentDirectWeld.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + momentDirectWeld.Bolt.NumberOfRows + " - 1)) * " + beam.Shape.tf);
					Reporting.AddCapacityLine("= " + BearingCap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / BearingCap, "Bolt Bearing on Flange", memberType);
				}
				else
				{
					Reporting.AddLine("= " + momentDirectWeld.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + momentDirectWeld.Bolt.NumberOfRows + " - 1)) * " + beam.Shape.tf);
					Reporting.AddCapacityLine("= " + BearingCap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / BearingCap, "Bolt Bearing on Flange", memberType);
				}

				Reporting.AddHeader("Plate Tension");
				t1 = momentDirectWeld.Top.b;
				t2 = column.Shape.d - 2 * column.Shape.tf;
				PlateWidthAtTaper = t2 + (t1 - t2) / momentDirectWeld.Bottom.Length * (beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + beam.EndSetback);

				Reporting.AddHeader("Plate Tension " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
				Tg = PlateWidthAtTaper * momentDirectWeld.Top.FlangeThickness * ConstNum.FIOMEGA0_9N * momentDirectWeld.Material.Fy;
				Reporting.AddHeader("Tension Yielding:");
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * b * t");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + momentDirectWeld.Material.Fy + " * " + PlateWidthAtTaper + " * " + momentDirectWeld.Top.FlangeThickness);
				if (Tg >= Ff)
					Reporting.AddCapacityLine("= " + Tg + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tg, "Tension Yielding", memberType);
				else
					Reporting.AddCapacityLine("= " + Tg + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tg, "Tension Yielding", memberType);

				bn = PlateWidthAtTaper - Math.Max(0.15F * PlateWidthAtTaper, momentDirectWeld.Bolt.NumberOfLines * (momentDirectWeld.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH));
				Tn = bn * momentDirectWeld.Top.FlangeThickness * ConstNum.FIOMEGA0_75N * momentDirectWeld.Material.Fu;
				Reporting.AddHeader("Tension Rupture:");
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Fu * (b - Max(0.15 * b); Nt * (dh + " + ConstNum.SIXTEENTH_INCH + "))) * t");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + momentDirectWeld.Material.Fu + " * " + "(" + PlateWidthAtTaper + " - Max(0.15 * " + PlateWidthAtTaper + ", " + momentDirectWeld.Bolt.NumberOfLines + " * (" + momentDirectWeld.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + "))) * " + momentDirectWeld.Top.FlangeThickness);
				if (Tn >= Ff)
					Reporting.AddCapacityLine("= " + Tn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tn, "Tension Rupture", memberType);
				else
					Reporting.AddCapacityLine("= " + Tn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tn, "Tension Rupture", memberType);

				Reporting.AddHeader("Block shear rupture of the Plate:");
				if (momentDirectWeld.Bolt.NumberOfLines == 4)
				{
					AgTension = Math.Min(beam.Shape.g1 + 2 * momentDirectWeld.Top.g1, PlateWidthAtTaper - beam.Shape.g1) * momentDirectWeld.Top.FlangeThickness;
					Reporting.AddLine("Agt = Min((g + 2 * g1), (b - g)) * t");
					Reporting.AddLine("= Min((" + beam.Shape.g1 + " + 2 * " + momentDirectWeld.Top.g1 + "), (" + PlateWidthAtTaper + " - " + beam.Shape.g1 + ")]*" + momentDirectWeld.Top.FlangeThickness);
					Reporting.AddLine("= " + AgTension + ConstUnit.Area);
					Reporting.AddLine("");

					AnTension = AgTension - 3 * (momentDirectWeld.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * momentDirectWeld.Top.FlangeThickness;
					Reporting.AddLine("Ant = Agt - (n - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("= " + AgTension + " - 3 * (" + (momentDirectWeld.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + momentDirectWeld.Top.FlangeThickness);
					Reporting.AddLine("= " + AnTension + ConstUnit.Area);

				}
				else
				{
					AgTension = Math.Min(beam.Shape.g1, PlateWidthAtTaper - beam.Shape.g1) * momentDirectWeld.Top.FlangeThickness;
					Reporting.AddLine("Agt = Min(g, 2 * b - g) * t");
					Reporting.AddLine("= Min(" + beam.Shape.g1 + ", (" + PlateWidthAtTaper + " - " + beam.Shape.g1 + ")) * " + momentDirectWeld.Top.FlangeThickness);
					Reporting.AddLine("= " + AgTension + ConstUnit.Area);

					AnTension = AgTension - (momentDirectWeld.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * momentDirectWeld.Top.FlangeThickness;
					Reporting.AddLine("Ant = Agt - (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("= " + AgTension + " - (" + (momentDirectWeld.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + momentDirectWeld.Top.FlangeThickness);
					Reporting.AddLine("= " + AnTension + ConstUnit.Area);
				}
				AgShear = 2 * ((momentDirectWeld.Bolt.NumberOfRows - 1) * momentDirectWeld.Bolt.SpacingLongDir + momentDirectWeld.Bolt.EdgeDistLongDir) * momentDirectWeld.Top.FlangeThickness;
				Reporting.AddLine("Agv = 2 * ((nl - 1) * s + Le) * t");
				Reporting.AddLine("= 2 * ((" + momentDirectWeld.Bolt.NumberOfRows + " - 1) * " + momentDirectWeld.Bolt.SpacingLongDir + " + " + momentDirectWeld.Bolt.EdgeDistLongDir + ") * " + momentDirectWeld.Top.FlangeThickness);
				Reporting.AddLine("= " + AgShear + ConstUnit.Area);

				AnShear = AgShear - 2 * (momentDirectWeld.Bolt.NumberOfRows - 0.5) * (momentDirectWeld.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * momentDirectWeld.Top.FlangeThickness;
				Reporting.AddLine("Anv = Agv - 2 * (nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("=" + AgShear + " - 2 * (" + momentDirectWeld.Bolt.NumberOfRows + " - 0.5) * (" + (momentDirectWeld.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH + ") * " + momentDirectWeld.Top.FlangeThickness));
				Reporting.AddLine("= " + AnShear + ConstUnit.Area);

				FiRn = MiscCalculationsWithReporting.BlockShearNew(momentDirectWeld.Material.Fu, AnShear, 1, AnTension, AgShear, momentDirectWeld.Material.Fy, 0, Ff, true);
				Reporting.AddHeader("Block shear rupture of the Beam Flange:");

				AgTension = (beam.Shape.bf - beam.Shape.g1) * beam.Shape.tf;
				Reporting.AddLine("Agt = (bf - g) * t = (" + beam.Shape.bf + " - " + beam.Shape.g1 + ") * " + beam.Shape.tf);
				Reporting.AddLine("= " + AgTension + ConstUnit.Area);

				AnTension = AgTension - (momentDirectWeld.Bolt.NumberOfLines - 1) * (momentDirectWeld.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * beam.Shape.tf;
				Reporting.AddLine("Ant = Agt - (nt - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("= " + AgTension + " - (" + momentDirectWeld.Bolt.NumberOfLines + " - 1) * (" + (momentDirectWeld.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + beam.Shape.tf);
				Reporting.AddLine("= " + AnTension + ConstUnit.Area);

				AgShear = 2 * ((momentDirectWeld.Bolt.NumberOfRows - 1) * momentDirectWeld.Bolt.SpacingLongDir + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange) * beam.Shape.tf;
				Reporting.AddLine("Agv = 2 * ((nl - 1) * s + ef) * t");
				Reporting.AddLine("= 2 * ((" + momentDirectWeld.Bolt.NumberOfRows + " - 1) * " + momentDirectWeld.Bolt.SpacingLongDir + " + " + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + ") * " + beam.Shape.tf);
				Reporting.AddLine("= " + AgShear + ConstUnit.Area);

				AnShear = AgShear - 2 * (momentDirectWeld.Bolt.NumberOfRows - 0.5) * (momentDirectWeld.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * beam.Shape.tf;
				Reporting.AddLine("Anv = Agv - 2 * (nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("=" + AgShear + " - 2 * (" + momentDirectWeld.Bolt.NumberOfRows + " - 0.5) * (" + (momentDirectWeld.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) + ") * " + beam.Shape.tf);
				Reporting.AddLine("= " + AnShear + ConstUnit.Area);

				FiRn = MiscCalculationsWithReporting.BlockShearNew(beam.Material.Fu, AnShear, 1, AnTension, AgShear, beam.Material.Fy, 0, Ff, true);
				Reporting.AddLine("Plate " + ConstString.DES_OR_ALLOWABLE + " Compressive Strength:");
				sqr12 = Math.Sqrt(12);
				k = 1.2;
				beam.WinConnect.Fema.L = (beam.EndSetback + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
				widthC = column.Shape.d - 2 * column.Shape.tf;
				widthEnd = momentDirectWeld.Bottom.b;
				WidthforBuckling = widthEnd + (widthC - widthEnd) / momentDirectWeld.Bottom.Length * (momentDirectWeld.Bottom.Length - beam.WinConnect.Fema.L);

				Reporting.AddLine("Unbraced Length (L) = c + ef = " + beam.EndSetback + " + " + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
				Reporting.AddLine("Effective Length Factor (K) = 1.2");
				kL_r = (beam.WinConnect.Fema.L * k * sqr12 / momentDirectWeld.Bottom.FlangeThickness);

				Reporting.AddLine("KL / r = k * L / (t / 3.464) = " + k + " * " + beam.WinConnect.Fema.L + " / (" + momentDirectWeld.Bottom.FlangeThickness + " / 3.464) = " + kL_r);

				Fcr = CommonCalculations.BucklingStress(kL_r, momentDirectWeld.Material.Fy, true);

				FiRn = ConstNum.FIOMEGA0_9N * Fcr * WidthforBuckling * momentDirectWeld.Bottom.FlangeThickness;
				if (FiRn >= Ff)
					Reporting.AddCapacityLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + WidthforBuckling + " * " + momentDirectWeld.Bottom.FlangeThickness + " = " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / FiRn, "Compressive Strength", memberType);
				else
					Reporting.AddCapacityLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + WidthforBuckling + " * " + momentDirectWeld.Bottom.FlangeThickness + " = " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / FiRn, "Compressive Strength", memberType);
			}
			else
			{
				TplateMin = Math.Min(momentDirectWeld.Top.FlangeThickness, momentDirectWeld.Bottom.FlangeThickness);
				TplateMax = Math.Max(momentDirectWeld.Top.FlangeThickness, momentDirectWeld.Bottom.FlangeThickness);

				Reporting.AddHeader("Top Plate Tension Strength:");
				Tg = ConstNum.FIOMEGA0_9N * momentDirectWeld.Top.b * momentDirectWeld.Top.FlangeThickness * momentDirectWeld.Material.Fy;
				Reporting.AddHeader("Tension Yielding:");
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * b * t");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + momentDirectWeld.Material.Fy + " * " + momentDirectWeld.Top.b + " * " + momentDirectWeld.Top.FlangeThickness);
				if (Tg >= Ff)
					Reporting.AddCapacityLine("= " + Tg + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tg, "Tension Yielding", memberType);
				else
					Reporting.AddCapacityLine("= " + Tg + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tg, "Tension Yielding", memberType);

				Tn = ConstNum.FIOMEGA0_75N * 0.85F * momentDirectWeld.Top.b * momentDirectWeld.Top.FlangeThickness * momentDirectWeld.Material.Fu;
				Reporting.AddHeader("Tension Rupture:");
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.85 * b * t * Fu");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.85 * " + momentDirectWeld.Top.b + " * " + momentDirectWeld.Top.FlangeThickness + " * " + momentDirectWeld.Material.Fu);
				if (Tn >= Ff)
					Reporting.AddCapacityLine("= " + Tn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tn, "Tension Rupture", memberType);
				else
					Reporting.AddCapacityLine("= " + Tn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tn, "Tension Rupture", memberType);

				Reporting.AddHeader("Bottom Plate " + ConstString.DES_OR_ALLOWABLE + " Compressive Strength:");
				sqr12 = Math.Sqrt(12);
				k = 1.2;
				widthC = column.Shape.d - 2 * column.Shape.tf;
				widthEnd = momentDirectWeld.Bottom.b;
				WidthforBuckling = widthEnd + (widthC - widthEnd) / momentDirectWeld.Bottom.Length * momentDirectWeld.Bottom.Length;

				Reporting.AddLine("Unbraced Length (L) = a = " + momentDirectWeld.Bottom.a + ConstUnit.Length);
				Reporting.AddLine("Effective Length Factor, K = 1.2");
				kL_r = (momentDirectWeld.Bottom.a * k * sqr12 / momentDirectWeld.Bottom.FlangeThickness);

				Reporting.AddLine("KL / r = k * L / (t / 3.464) = " + k + " * " + momentDirectWeld.Bottom.a + " / (" + momentDirectWeld.Bottom.FlangeThickness + "/3.464) = " + kL_r);

				Fcr = CommonCalculations.BucklingStress(kL_r, momentDirectWeld.Material.Fy, true);

				FiRn = ConstNum.FIOMEGA0_9N * Fcr * WidthforBuckling * momentDirectWeld.Bottom.FlangeThickness;
				if (FiRn >= Ff)
					Reporting.AddCapacityLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + WidthforBuckling + " * " + momentDirectWeld.Bottom.FlangeThickness + " = " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / FiRn, "Compressive Strength", memberType);
				else
					Reporting.AddCapacityLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + WidthforBuckling + " * " + momentDirectWeld.Bottom.FlangeThickness + " = " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / FiRn, "Compressive Strength", memberType);

				Reporting.AddHeader("Bottom Plate " + ConstString.DES_OR_ALLOWABLE + " Tension Strength:");
				Tg = momentDirectWeld.Bottom.b * momentDirectWeld.Bottom.FlangeThickness * ConstNum.FIOMEGA0_9N * momentDirectWeld.Material.Fy;
				Reporting.AddLine("On Gross Area = b * t * " + ConstString.FIOMEGA0_9 + " * Fy");
				Reporting.AddLine("= " + momentDirectWeld.Bottom.b + " * " + momentDirectWeld.Bottom.FlangeThickness + " * " + ConstString.FIOMEGA0_9 + "   * " + momentDirectWeld.Material.Fy);
				if (Tg >= Ff)
					Reporting.AddCapacityLine("= " + Tg + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tg, "Tension Strength", memberType);
				else
					Reporting.AddCapacityLine("= " + Tg + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tg, "Tension Strength", memberType);
				if (momentDirectWeld.Bottom.Length >= 2 * beam.Shape.bf)
					U = 0.85;
				else if (momentDirectWeld.Bottom.Length >= 1.5 * beam.Shape.bf)
					U = 0.85;
				else if (momentDirectWeld.Bottom.Length >= beam.Shape.bf)
					U = 0.75;
				else
					U = 0;

				Tn = ConstNum.FIOMEGA0_75N * U * momentDirectWeld.Bottom.b * momentDirectWeld.Bottom.FlangeThickness * momentDirectWeld.Material.Fu;
				Reporting.AddHeader("Tension Rupture:");
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * u * b * t * Fu");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + U + " * " + momentDirectWeld.Bottom.b + " * " + momentDirectWeld.Bottom.FlangeThickness + " * " + momentDirectWeld.Material.Fu);
				if (Tn >= Ff)
					Reporting.AddCapacityLine("= " + Tn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tn, "Tension Rupture", memberType);
				else
				{
					Reporting.AddCapacityLine("= " + Tn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tn, "Tension Rupture", memberType);
					if (U == 0)
						Reporting.AddLine("(U = 0 when welded length of plate is << plate width.)");
				}

				Reporting.AddHeader("Plate to Beam Flange Weld");
				w = CommonCalculations.MinimumWeld(momentDirectWeld.Top.FlangeThickness, beam.Shape.tf);
				if (momentDirectWeld.Top.FlangeThickness <= ConstNum.QUARTER_INCH)
					w1 = momentDirectWeld.Top.FlangeThickness;
				else
					w1 = momentDirectWeld.Top.FlangeThickness - ConstNum.SIXTEENTH_INCH;
				if (w1 < w && w1 < momentDirectWeld.Top.FlangeThickness)
					w1 = momentDirectWeld.Top.FlangeThickness;

				Reporting.AddLine("Top Plate-to-Beam Flange Weld:");
				Reporting.AddLine("Plate Thickness = " + momentDirectWeld.Top.FlangeThickness + ConstUnit.Length + " Beam Flange Thickness = " + beam.Shape.tf + ConstUnit.Length);
				Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(w) + ConstUnit.Length + " Maximum Weld Size = " + CommonCalculations.WeldSize(w1) + ConstUnit.Length);
				if (momentDirectWeld.Top.FilletWeldW3 >= w && momentDirectWeld.Top.FilletWeldW3 <= w1)
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentDirectWeld.Top.FilletWeldW3) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentDirectWeld.Top.FilletWeldW3) + ConstUnit.Length + " (NG)");

				CommonDataStatic.WeldBetaL = momentDirectWeld.Top.Length - momentDirectWeld.Top.a;
				BetaN = CommonCalculations.WeldBetaFactor(CommonDataStatic.WeldBetaL, momentDirectWeld.Top.FilletWeldW3);
				if (BetaN < 1)
				{
					Reporting.AddLine(ConstNum.BETAS + " = " + BetaN);
					CommonDataStatic.WeldBetaL = BetaN * (momentDirectWeld.Top.Length - momentDirectWeld.Top.a);
					capacity = ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentDirectWeld.Top.FilletWeldW3 * Math.Max(2 * BetaN * CommonDataStatic.WeldBetaL + momentDirectWeld.Top.b, 1.7F * BetaN * CommonDataStatic.WeldBetaL + 1.5 * beam.WinConnect.MomentFlangePlate.TopWidth);
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx * w * Math.Max(2 * " + ConstNum.BETAS + " * Lw + b); 1.7 * " + ConstNum.BETAS + " * Lw + 1.5*b)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + momentDirectWeld.Top.FilletWeldW3 + " * Math.Max((2 * " + (BetaN * CommonDataStatic.WeldBetaL) + " + " + momentDirectWeld.Top.b + "); (1.7 * " + (BetaN * CommonDataStatic.WeldBetaL) + " + 1.5*" + beam.WinConnect.MomentFlangePlate.TopWidth + "))");
				}
				else
				{
					CommonDataStatic.WeldBetaL = momentDirectWeld.Top.Length - momentDirectWeld.Top.a;
					capacity = ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentDirectWeld.Top.FilletWeldW3 * Math.Max(2 * CommonDataStatic.WeldBetaL + momentDirectWeld.Top.b, 1.7F * CommonDataStatic.WeldBetaL + 1.5 * beam.WinConnect.MomentFlangePlate.TopWidth);
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx * w * Max(2 * Lw + b), 1.7 * Lw + 1.5 * b)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + momentDirectWeld.Top.FilletWeldW3 + " * Math.Max((2 * " + CommonDataStatic.WeldBetaL + " + " + momentDirectWeld.Top.b + "); (1.7 * " + CommonDataStatic.WeldBetaL + " + 1.5*" + beam.WinConnect.MomentFlangePlate.TopWidth + "))");
				}
				if (capacity >= Ff)
					Reporting.AddCapacityLine("= " + capacity + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / capacity, "Weld", memberType);
				else
					Reporting.AddCapacityLine("= " + capacity + " << " + Ff + ConstUnit.Force + " (NG)", Ff / capacity, "Weld", memberType);

				w = CommonCalculations.MinimumWeld(momentDirectWeld.Bottom.FlangeThickness, beam.Shape.tf);
				if (beam.Shape.tf <= ConstNum.QUARTER_INCH)
					w1 = beam.Shape.tf;
				else
					w1 = beam.Shape.tf - ConstNum.SIXTEENTH_INCH;
				if (w1 < w && w1 < beam.Shape.tf)
					w1 = beam.Shape.tf;

				Reporting.AddHeader("Bottom Plate-to-Beam Flange Weld:");
				Reporting.AddLine("Plate Thickness = " + momentDirectWeld.Bottom.FlangeThickness + ConstUnit.Length + " Beam Flange Thickness = " + beam.Shape.tf + ConstUnit.Length);
				Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(w) + ConstUnit.Length + " Maximum Weld Size = " + CommonCalculations.WeldSize(w1) + ConstUnit.Length);
				if (momentDirectWeld.Bottom.FilletWeldW3 >= w && momentDirectWeld.Bottom.FilletWeldW3 <= w1)
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentDirectWeld.Bottom.FilletWeldW3) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentDirectWeld.Bottom.FilletWeldW3) + ConstUnit.Length + " (NG)");

				BetaN = CommonCalculations.WeldBetaFactor(momentDirectWeld.Bottom.Length - momentDirectWeld.Bottom.a, momentDirectWeld.Bottom.FilletWeldW3);
				capacity = ConstNum.FIOMEGA0_75N * BetaN * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentDirectWeld.Bottom.FilletWeldW3 * 2 * (momentDirectWeld.Bottom.Length - momentDirectWeld.Bottom.a);
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * " + ConstNum.BETAS + " * 0.4242" + " * Fexx * w * 2 * L ");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + BetaN + " * 0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + momentDirectWeld.Bottom.FilletWeldW3 + " * 2 * " + (momentDirectWeld.Bottom.Length - momentDirectWeld.Bottom.a) + ")");
				if (capacity >= Ff)
					Reporting.AddCapacityLine("= " + capacity + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / capacity, "Weld", memberType);
				else
					Reporting.AddCapacityLine("= " + capacity + " << " + Ff + ConstUnit.Force + " (NG)", Ff / capacity, "Weld", memberType);

			}

			TplateMin = Math.Min(momentDirectWeld.Top.FlangeThickness, momentDirectWeld.Bottom.FlangeThickness);
			TplateMax = Math.Max(momentDirectWeld.Top.FlangeThickness, momentDirectWeld.Bottom.FlangeThickness);

			Reporting.AddHeader("Plate Shear Strength at Column Flange Weld: ");
			Reporting.AddHeader("Force at each half-flange:");
			if ((!rightBeam.IsActive || !leftBeam.IsActive) && (momentDirectWeld.Type == ENumbers.Four || momentDirectWeld.Type == ENumbers.One))
			{
				Lflange = column.Shape.bf / 2 - column.Shape.k1;
				lweb = column.Shape.t;
				FforShear = (Rload + CommonDataStatic.F_Shear) / 4;
				Reporting.AddLine("Fs = (Ff + F_Shear) / 4 = (" + Rload + " + " + CommonDataStatic.F_Shear + ") / 4 = " + FforShear + ConstUnit.Force);
			}
			else
			{
				Lflange = column.Shape.bf / 2 - column.Shape.k1;
				lweb = 0;
				FforShear = (Rload + CommonDataStatic.F_Shear) / 2;
				Reporting.AddLine("Fs = (Ff + F_Shear) / 2 = (" + Rload + " + " + CommonDataStatic.F_Shear + ") / 2 = " + FforShear + ConstUnit.Force);
			}

			Reporting.AddLine("F_Shear = " + CommonDataStatic.F_Shear + ConstUnit.Force + " is from shear plate.");
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.FIOMEGA1_0N + " * 0.6 * Fy * tp * Ls");
			if (momentDirectWeld.WeldFlangeType == EWeldType.CJP)
			{
				Cap = ConstNum.FIOMEGA1_0N * 0.6 * momentDirectWeld.Material.Fy * TplateMin * Lflange;
				Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + momentDirectWeld.Material.Fy + " * " + TplateMin + " * " + Lflange);
			}
			else
			{
				Cap = ConstNum.FIOMEGA1_0N * 0.6 * momentDirectWeld.Material.Fy * TplateMin * (Lflange - 2 * momentDirectWeld.Top.FilletWeldW1);
				Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + momentDirectWeld.Material.Fy + " * " + TplateMin + " * (" + Lflange + " - 2 * " + momentDirectWeld.Top.FilletWeldW1 + ")");
			}
			if (Cap >= FforShear)
				Reporting.AddCapacityLine("= " + Cap + " >= " + FforShear + ConstUnit.Force + " (OK)", FforShear / Cap, "F_Shear", memberType);
			else
				Reporting.AddCapacityLine("= " + Cap + " << " + FforShear + ConstUnit.Force + " (NG)", FforShear / Cap, "F_Shear", memberType);

			TplateMin = Math.Min(momentDirectWeld.Top.FlangeThickness, momentDirectWeld.Bottom.FlangeThickness);
			TplateMax = Math.Max(momentDirectWeld.Top.FlangeThickness, momentDirectWeld.Bottom.FlangeThickness);
			Reporting.AddHeader("Plate-to-Support Weld:");
			if ((!rightBeam.IsActive || !leftBeam.IsActive) && (momentDirectWeld.Type == ENumbers.Four || momentDirectWeld.Type == ENumbers.One))
			{
				Lflange = column.Shape.bf / 2 - column.Shape.k1;
				lweb = column.Shape.t;
				FforShear = (Rload + CommonDataStatic.F_Shear) / 4;
			}
			else
			{
				Lflange = column.Shape.bf / 2 - column.Shape.k1;
				lweb = 0;
				FforShear = (Rload + CommonDataStatic.F_Shear) / 2;
			}
			if (momentDirectWeld.WeldFlangeType == EWeldType.CJP)
				WeldLf = Lflange;
			else
				WeldLf = Lflange - 2 * momentDirectWeld.Top.FilletWeldW1;

			Reporting.AddHeader("Weld to column flange:");
			Reporting.AddLine("See above for the weld force.");
			if (momentDirectWeld.WeldFlangeType == EWeldType.CJP)
				Reporting.AddHeader("Plate to flange weld is CJP");
			else
			{
				Reporting.AddHeader("Minimum fillet weld size:");
				wminf = CommonCalculations.MinimumWeld(TplateMax, column.Shape.tf);

				if (wminf <= momentDirectWeld.Top.FilletWeldW1)
					Reporting.AddLine("wmin = " + wminf + " <= " + momentDirectWeld.Top.FilletWeldW1 + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("wmin = " + wminf + " >> " + momentDirectWeld.Top.FilletWeldW1 + ConstUnit.Length + " (NG)");
			}

			Reporting.AddHeader("Weld Strength at Each Half-Flange:");
			if (momentDirectWeld.WeldFlangeType == EWeldType.CJP)
			{
				capacity = ConstNum.FIOMEGA0_75N * 0.6 * TplateMin * WeldLf * momentDirectWeld.Material.Fu;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.6 * tp * L * Fu");
				Reporting.AddLine(ConstString.PHI + " Rn =" + ConstString.FIOMEGA0_75 + " * 0.6 * " + TplateMin + " * " + WeldLf + " * " + momentDirectWeld.Material.Fu);
			}
			else
			{
				capacity = ConstNum.FIOMEGA0_75N * 0.8484 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentDirectWeld.Top.FilletWeldW1 * WeldLf;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.8484 * Fexx * w * Lw = " + ConstString.FIOMEGA0_75 + " * 0.8484 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + momentDirectWeld.Top.FilletWeldW1 + " * " + WeldLf);
			}
			if (capacity >= FforShear)
				Reporting.AddCapacityLine("= " + capacity + " >= " + FforShear + ConstUnit.Force + " (OK)", FforShear / capacity, "Weld Strength at Each Half-Flange", memberType);
			else
				Reporting.AddCapacityLine("= " + capacity + " << " + FforShear + ConstUnit.Force + " (NG)", FforShear / capacity, "Weld Strength at Each Half-Flange", memberType);

			Reporting.AddHeader("Column Flange Shear at Welds:");
			Reporting.AddHeader("Yielding:");
			if (CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn)
			{
				OneorTwoN = 1;
				OneorTwo = string.Empty;
			}
			else
			{
				OneorTwoN = 2;
				OneorTwo = "2 * ";
			}
			colshear = OneorTwoN * column.Shape.tf * Lflange * ConstNum.FIOMEGA1_0N * 0.6 * column.Material.Fy;
			Reporting.AddLine(ConstString.PHI + " Rn = " + OneorTwo + " tf * L * " + ConstNum.FIOMEGA1_0N + " * 0.6 * Fy = " + OneorTwo + column.Shape.tf + " * " + Lflange + " * " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + column.Material.Fy);
			if (colshear >= FforShear)
				Reporting.AddCapacityLine("= " + colshear + " >= " + FforShear + ConstUnit.Force + " (OK)", FforShear / colshear, "Column Flange Shear at Welds - Yielding", memberType);
			else
				Reporting.AddCapacityLine("= " + colshear + " << " + FforShear + ConstUnit.Force + " (NG)", FforShear / colshear, "Column Flange Shear at Welds - Yielding", memberType);

			Reporting.AddHeader("Rupture:");
			colshear = OneorTwoN * column.Shape.tf * Lflange * ConstNum.FIOMEGA0_75N * 0.6 * column.Material.Fu;
			Reporting.AddLine(ConstString.PHI + " Rn = " + OneorTwo + "tf*L*" + ConstString.FIOMEGA0_75 + " * 0.6 * Fu = " + OneorTwo + column.Shape.tf + " * " + Lflange + " * " + ConstString.FIOMEGA0_75 + " *0.6 * " + column.Material.Fu);
			if (colshear >= FforShear)
				Reporting.AddCapacityLine("= " + colshear + " >= " + FforShear + ConstUnit.Force + " (OK)", FforShear / colshear, "Column Flange Shear at Welds - Rupture", memberType);
			else
				Reporting.AddCapacityLine("= " + colshear + " << " + FforShear + ConstUnit.Force + " (NG)", FforShear / colshear, "Column Flange Shear at Welds - Rupture", memberType);

			switch (momentDirectWeld.Type)
			{
				case ENumbers.Zero:
				case ENumbers.One:
				case ENumbers.Three:
				case ENumbers.Four:
					Reporting.AddHeader("Weld to Column Web:");
					if (momentDirectWeld.WeldFlangeType == EWeldType.CJP)
						Reporting.AddHeader("Plate to web weld is CJP");

					wminW = CommonCalculations.MinimumWeld(TplateMax, column.Shape.tw);
					if (wminW <= momentDirectWeld.Top.FilletWeldW2)
						Reporting.AddLine("wmin = " + wminW + " <= " + momentDirectWeld.Top.FilletWeldW2 + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("wmin = " + wminW + " >> " + momentDirectWeld.Top.FilletWeldW2 + ConstUnit.Length + " (NG)");

					if (lweb > 0)
					{
						if (momentDirectWeld.WeldFlangeType == EWeldType.CJP)
						{
							capacity = ConstNum.FIOMEGA0_9N * Math.Min(column.Material.Fy, momentDirectWeld.Material.Fy) * TplateMin * lweb;
							Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " *Fy*tp*Lw = " + ConstString.FIOMEGA0_9 + "   * " + Math.Min(column.Material.Fy, momentDirectWeld.Material.Fy) + " * " + TplateMin + " * " + lweb);
						}
						else
						{
							capacity = ConstNum.FIOMEGA0_75N * 0.8484 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentDirectWeld.Top.FilletWeldW2 * lweb;
							Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.8484 * Fexx * w * Lw = " + ConstString.FIOMEGA0_75 + " * 0.8484 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + momentDirectWeld.Top.FilletWeldW2 + " * " + lweb);
						}
						if (capacity >= 2 * FforShear)
							Reporting.AddCapacityLine("= " + capacity + " >= " + 2 * FforShear + ConstUnit.Force + " (OK)", 2 * FforShear / capacity, "Weld to Column Web", memberType);
						else
							Reporting.AddCapacityLine("= " + capacity + " << " + 2 * FforShear + ConstUnit.Force + " (NG)", 2 * FforShear / capacity, "Weld to Column Web", memberType);
					}
					break;
			}
		}

		private static void BeamGage(EMemberType memberType)
		{
			double Gage1 = 0;
			double Gage2 = 0;
			double gagemin = 0;
			double gagemax = 0;
			double outgage = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var momentDirectWeld = beam.WinConnect.MomentDirectWeld;

			Gage1 = 2 * (beam.Shape.k1 + momentDirectWeld.Bolt.BoltSize);
			Gage2 = beam.Shape.tw + 2 * (momentDirectWeld.Bolt.BoltSize + ConstNum.HALF_INCH);
			gagemin = Math.Max(Gage2, Gage1);
			gagemax = beam.Shape.bf - 2 * momentDirectWeld.Bolt.MinEdgeRolled;
			if (gagemax < gagemin)
				Reporting.AddLine("Beam flange is not wide enough for " + momentDirectWeld.Bolt.BoltSize + ConstUnit.Length + "  bolts.");
			if (beam.Shape.g1 > gagemax)
				beam.Shape.g1 = gagemax;
			if (beam.Shape.g1 < gagemin)
				beam.Shape.g1 = gagemin;

			outgage = (Math.Floor(4 * (gagemax - beam.Shape.g1) / 2)) / 4.0;
			if (outgage >= 2.67 * momentDirectWeld.Bolt.BoltSize)
			{
				if (momentDirectWeld.Top.g1 > outgage)
					momentDirectWeld.Top.g1 = outgage;
				if (momentDirectWeld.Top.g1 <= 2.67 * momentDirectWeld.Bolt.BoltSize)
					momentDirectWeld.Top.g1 = Math.Min(outgage, NumberFun.Round(2.67F * momentDirectWeld.Bolt.BoltSize, 4));

				if (momentDirectWeld.Bolt.NumberOfLines == 4)
					momentDirectWeld.Top.g1 = outgage;
				else
					momentDirectWeld.Top.g1 = 0;
			}
			else
				momentDirectWeld.Top.g1 = 0;
		}

		private static void BearingThickness(EMemberType memberType, ref double Fbe, ref double si, ref double Fbs, ref double BearingCap, double Rload, ref double tbReq)
		{
			var momentDirectWeld = CommonDataStatic.DetailDataDict[memberType].WinConnect.MomentDirectWeld;

			Fbe = CommonCalculations.EdgeBearing(momentDirectWeld.Bolt.EdgeDistLongDir, momentDirectWeld.Bolt.HoleLength, momentDirectWeld.Bolt.BoltSize, momentDirectWeld.Material.Fu, momentDirectWeld.Bolt.HoleType, false);
			si = momentDirectWeld.Bolt.SpacingLongDir;
			Fbs = CommonCalculations.SpacingBearing(momentDirectWeld.Bolt.SpacingLongDir, momentDirectWeld.Bolt.HoleLength, momentDirectWeld.Bolt.BoltSize, momentDirectWeld.Bolt.HoleType, momentDirectWeld.Material.Fu, false);
			BearingCap = momentDirectWeld.Bolt.NumberOfLines * (Fbe + Fbs * (momentDirectWeld.Bolt.NumberOfRows - 1));
			tbReq = Rload / BearingCap;
		}

		private static void BucklingThickness(EMemberType memberType, ref double tgReq, ref double widthC, ref double widthEnd, ref double WidthforBuckling, bool TopPlate, ref double k, ref double kL_r, ref double Fcr, ref double FiRn, double Rload)
		{
			double t = 0;
			double rg = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var momentDirectWeld = beam.WinConnect.MomentDirectWeld;

			t = tgReq - ConstNum.SIXTEENTH_INCH;
			if (momentDirectWeld.Type == ENumbers.Zero || momentDirectWeld.Type == ENumbers.One ||
			    momentDirectWeld.Type == ENumbers.Two)
			{
				beam.WinConnect.Fema.L = (beam.EndSetback + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
				widthC = column.Shape.d - 2 * column.Shape.tf;
				widthEnd = momentDirectWeld.Bottom.b;
				WidthforBuckling = widthEnd + (widthC - widthEnd) / momentDirectWeld.Bottom.Length * (momentDirectWeld.Bottom.Length - beam.WinConnect.Fema.L);
			}
			else
			{
				beam.WinConnect.Fema.L = momentDirectWeld.Bottom.a;
				if (TopPlate)
					WidthforBuckling = momentDirectWeld.Top.b;
				else
				{
					widthC = column.Shape.d - 2 * column.Shape.tf;
					widthEnd = momentDirectWeld.Bottom.b;
					WidthforBuckling = widthEnd + (widthC - widthEnd) / momentDirectWeld.Bottom.Length * momentDirectWeld.Bottom.Length;
				}
			}
			do
			{
				t = t + ConstNum.SIXTEENTH_INCH;
				k = 1.2;

				rg = t / Math.Sqrt(12);
				kL_r = (k * beam.WinConnect.Fema.L / rg);

				Fcr = CommonCalculations.BucklingStress(kL_r, momentDirectWeld.Material.Fy, false);

				FiRn = ConstNum.FIOMEGA0_9N * Fcr * WidthforBuckling * t;
			} while (FiRn < Rload);
			tgReq = t;
		}

		private static void SupportWeld(EMemberType memberType, ref double wminf, ref double wminW, ref double Lflange, ref double welds, double Rload, ref double wm, ref double lweb, ref double FforShear, ref double WebWeld)
		{
			double weldsP = 0;
			double weldl = 0;
			double discr = 0;
			double welds0 = 0;
			double wlimit1 = 0;
			double wlimit2 = 0;
			double tPlReqfTest = 0;
			double tPlReqf = 0;
			double tPlReqw = 0;
			double tPlReq = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var momentDirectWeld = beam.WinConnect.MomentDirectWeld;

			wminf = CommonCalculations.MinimumWeld(momentDirectWeld.Top.FlangeThickness, column.Shape.tf);
			wminW = CommonCalculations.MinimumWeld(momentDirectWeld.Top.FlangeThickness, column.Shape.tw);

			Lflange = column.Shape.bf / 2 - column.Shape.k1;
			welds = wminf;
			do
			{
				weldsP = welds;
				weldl = Lflange - 2 * welds;

				welds = (Rload + CommonDataStatic.F_Shear) / (4 * weldl * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				weldsP = NumberFun.Round(weldsP, 16);
				welds = NumberFun.Round(welds, 16);
			} while (weldsP != welds && welds <= 0.75);

			wm = column.Shape.tf * column.Material.Fu / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
			if ((!rightBeam.IsActive || !leftBeam.IsActive) && (momentDirectWeld.Type == ENumbers.Four || momentDirectWeld.Type == ENumbers.One))
			{
				Lflange = column.Shape.bf / 2 - column.Shape.k1;
				lweb = column.Shape.t;
				FforShear = (Rload + CommonDataStatic.F_Shear) / 4;
			}
			else
			{
				lweb = 0;
				FforShear = (Rload + CommonDataStatic.F_Shear) / 2;
			}
			discr = Math.Pow(Lflange, 2) / 16 - FforShear / (1.2726 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
			if (discr >= 0)
				welds0 = Lflange / 4 - Math.Sqrt(discr);
			else
				welds0 = 0;

			wlimit1 = Lflange / 6;
			wlimit2 = Lflange / 2 - FforShear / (1.08 * momentDirectWeld.Top.FlangeThickness * momentDirectWeld.Material.Fy);
			tPlReqfTest = FforShear / (ConstNum.FIOMEGA1_0N * 0.6 * momentDirectWeld.Material.Fy * (Lflange - 2 * welds));
			welds0 = welds;

			if ((welds0 == 0 || (welds0 > 0.75 * ConstNum.ONE_INCH) || welds0 > wlimit1 || welds > wlimit2) && tPlReqfTest > 0)
			{
				momentDirectWeld.WeldFlangeType = EWeldType.CJP;
				welds = 0;
				tPlReqf = FforShear / Math.Min(0.48 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Lflange, ConstNum.FIOMEGA1_0N * 0.6 * momentDirectWeld.Material.Fy * Lflange);
			}
			else
			{
				momentDirectWeld.WeldFlangeType = EWeldType.Fillet;
				welds = NumberFun.Round(Math.Max(welds0, wminf), 16);
				tPlReqf = FforShear / (ConstNum.FIOMEGA1_0N * 0.6 * momentDirectWeld.Material.Fy * (Lflange - 2 * welds));
			}

			if (lweb > 0)
			{
				WebWeld = Math.Max(2 * FforShear / (ConstNum.FIOMEGA0_75N * 0.8484 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * lweb), wminW);
				wlimit1 = lweb / 4;

				if (momentDirectWeld.WeldFlangeType == EWeldType.CJP)
					WebWeld = 0;

				tPlReqw = 2 * FforShear / (ConstNum.FIOMEGA0_9N * momentDirectWeld.Material.Fy * lweb);
			}
			else
			{
				momentDirectWeld.WeldFlangeType = EWeldType.CJP;
				tPlReqw = 0;
				WebWeld = 0;
			}
			tPlReq = Math.Max(tPlReqf, tPlReqw);
			if (momentDirectWeld.Top.FlangeThickness < tPlReq)
				momentDirectWeld.Top.FlangeThickness = CommonCalculations.PlateThickness(NumberFun.Round(tPlReq, ERoundingPrecision.Sixteenth, ERoundingStyle.Nearest));
			if (momentDirectWeld.Bottom.FlangeThickness < tPlReq)
				momentDirectWeld.Bottom.FlangeThickness = CommonCalculations.PlateThickness(NumberFun.Round(tPlReq, ERoundingPrecision.Sixteenth, ERoundingStyle.Nearest));
		}
	}
}