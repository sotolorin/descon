using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class ColumnSpliceWebPlate
	{
		public static void WebPlateCapacity(EColumnSplice ColumnConnection, double Fw)
		{
			double usefulweldonPlate = 0;
			double FWeldSizeFillerToCol = 0;
			double FWeldSizeChToFiller = 0;
			double FWeldSizeChToCol = 0;
			double dumy = 0;
			double minedge = 0;
			double minsp = 0;
			double FuT = 0;
			double FillerDForce = 0;
			double Fbe = 0;
			double n = 0;
			double Fbs = 0;
			double capacity = 0;
			double C1 = 0;
			double cN = 0;
			double Ca = 0;
			double a = 0;
			double Cmax = 0;
			double Costheta = 0;
			double Sintheta = 0;
			double c = 0;
			double Eccentricity = 0;
			double reduction = 0;
			double BFv = 0;
			double F_Horizontal = 0;
			double F_Vertical = 0;
			double Fdesign = 0;
			string part2 = "";
			string part1 = "";
			double NumPLorCH = 0;
			double tw = 0;
			double d = 0;

			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			if (colSplice.ConnectionOption == ESpliceConnection.DirectlyWelded)
			{
				d = colSplice.Channel.d;
				tw = colSplice.Channel.tw;
				NumPLorCH = colSplice.NumberOfChannels;
				part1 = "Channel";
				part2 = "Channels";
				if (colSplice.ChannelType == ESpliceChannelType.Temporary)
				{
					Fdesign = 0;
					F_Vertical = 0;
					F_Horizontal = 0;
				}
				else
				{
					Fdesign = Math.Sqrt(Math.Pow(Fw, 2) + Math.Pow(colSplice.WebShear, 2));
					F_Vertical = Fw;
					F_Horizontal = colSplice.WebShear;
				}
			}
			else
			{
				d = colSplice.SpliceWidthWeb;
				tw = colSplice.SpliceThicknessWeb;
				NumPLorCH = 2;
				part1 = "Plate";
				part2 = "Plates";
				Fdesign = colSplice.WebShear;
				F_Vertical = 0;
				F_Horizontal = colSplice.WebShear;

				Reporting.AddHeader("Web Splice:");
				Reporting.AddLine("Vertical Force = " + F_Vertical + ConstUnit.Force);
				Reporting.AddLine("Horizontal Force = " + F_Horizontal + ConstUnit.Force);
				Reporting.AddLine("Resultant Force, Fr = " + Fdesign + ConstUnit.Force);
				Reporting.AddLine("");
				if (Fdesign == 0)
				{
					Reporting.AddHeader("Use Nominal Splice.");
					Reporting.AddLine("Web Plates: 2PL " + colSplice.WebPLLength + "X" + colSplice.SpliceWidthWeb + "X" + colSplice.SpliceThicknessWeb + ConstUnit.Length + " - " + colSplice.Material.Name);
					return;
				}
				Reporting.AddHeader("Web Plates: 2PL " + colSplice.WebPLLength + "X" + colSplice.SpliceWidthWeb + "X" + colSplice.SpliceThicknessWeb + ConstUnit.Length + " - " + colSplice.Material.Name);
			}
			if (ColumnConnection == EColumnSplice.LowerColumn || ColumnConnection == EColumnSplice.Both)
			{
				Reporting.AddHeader("Connection to Lower Column Web");

				if (colSplice.ConnectionLower == EConnectionStyle.Bolted)
				{
					colSplice.SpliceWidthWeb = colSplice.BoltGageWebLower + 2 * colSplice.BoltHorizEdgeDistanceWeb;
					if (Fdesign == 0)
					{
						colSplice.SpliceThicknessWeb = ConstNum.THREE_EIGHTS_INCH;
						colSplice.BoltRowsWebLower = 2;
						colSplice.BoltVertEdgeDistanceColumn = 2;
						colSplice.BoltVertSpacing = 6;
						colSplice.BoltVertEdgeDistancePlate = 2;
						colSplice.SpliceLengthLowerWeb = colSplice.BoltVertEdgeDistanceColumn + colSplice.BoltVertSpacing + colSplice.BoltVertEdgeDistancePlate;
						colSplice.FillerNumBoltRowsLW = 0;
						if (colSplice.FillerThicknessWebLower > 0)
						{
							colSplice.FillerLengthWebLower = colSplice.SpliceLengthLowerWeb;
						}
					}
					else
					{
						Reporting.AddHeader("Bolts on Lower Column Web");
						BoltsLowerWeb(tw, ref FuT, ref minsp, F_Horizontal, part2, ref minedge);

						Reporting.AddHeader("Bolt Shear Capacity (Splice):");

						if (colSplice.Bolt.BoltType == EBoltType.SC || colSplice.FillerThicknessWebLower <= ConstNum.QUARTER_INCH)
							BFv = NumPLorCH * colSplice.Bolt.BoltStrength;
						else if (colSplice.FillerThicknessWebLower <= 0.75 * ConstNum.ONE_INCH)
						{
							if (CommonDataStatic.Units == EUnit.US)
								reduction = 0.4 * (colSplice.FillerThicknessWebLower - 0.25);
							else
								reduction = 0.0154 * (colSplice.FillerThicknessWebLower - 6);
							BFv = NumPLorCH * colSplice.Bolt.BoltStrength * (1 - reduction);
							Reporting.AddHeader("Strength Reduction for Bolts:");

							if (CommonDataStatic.Units == EUnit.US)
							{
								Reporting.AddLine("Reduction Factor, r = 1- 0.4 * (t_fil - 0.25 )");
								Reporting.AddLine("= 1 - 0.4 * (" + colSplice.FillerThicknessWebLower + " - 0.25 ) = " + (1 - reduction));
							}
							else
							{
								Reporting.AddLine("Reduction Factor, r = 1 - 0.0154 * (t_fil - 6 )");
								Reporting.AddLine("= 1 - 0.0154 * (" + colSplice.FillerThicknessWebLower + " - 6 ) = " + (1 - reduction));
							}
						}
						else
							BFv = NumPLorCH * colSplice.Bolt.BoltStrength;

						Eccentricity = colSplice.BoltVertEdgeDistanceColumn + (colSplice.BoltRowsWebLower - 1) * colSplice.BoltVertSpacing / 2;
						MiscCalculationsWithReporting.EccentricBolt(Eccentricity, colSplice.BoltGageWebLower, colSplice.BoltVertSpacing, colSplice.BoltRowsWebLower, 2, ref c);
						Reporting.AddLine("Eccentricity = " + Eccentricity + ConstUnit.Length);
						Reporting.AddLine("Bolt Grid: 2 X " + colSplice.BoltRowsWebLower);
						Reporting.AddLine("Spacing: " + colSplice.BoltGageWebLower + " X " + colSplice.BoltVertSpacing + ConstUnit.Length);
						Reporting.AddLine("Bolt Capacity Coefficient, C = " + c);
						if (F_Vertical > 0)
						{
							Reporting.AddHeader("Adjustment for Inclined Load:");
							Sintheta = F_Vertical / Fdesign;
							Costheta = F_Horizontal / Fdesign;
							Cmax = 2 * colSplice.BoltRowsWebLower;
							a = Cmax / c;
							Ca = Math.Max(a * c / (Sintheta + a * Costheta), c);
							cN = Ca / (colSplice.BoltRowsWebLower * 2);
							C1 = Ca;
							capacity = Ca * BFv;
							Reporting.AddLine(ConstString.PHI + " = " + Math.Atan(F_Vertical / F_Horizontal) * 180 / Math.PI + " Degrees");
							Reporting.AddLine("Cmax = " + Cmax + "; A = " + a);
							Reporting.AddLine("Ca = Max(C; [A * C / (Sin(" + ConstString.PHI + ") + A * Cos(" + ConstString.PHI + "))])");
							Reporting.AddLine("= Max(" + c + "; [" + a + " * " + c + " / (" + Sintheta + " + " + a + " * " + Costheta + ")])");
							Reporting.AddLine("= " + Ca);
							Reporting.AddLine(ConstString.PHI + " Rn = Ca * Fv = " + Ca + " * " + BFv);
						}
						else
						{
							cN = c / (colSplice.BoltRowsWebLower * 2);
							C1 = c;
							capacity = c * BFv;
							Reporting.AddLine(ConstString.PHI + " Rn = C * Fv = " + c + " * " + BFv);
						}
						if (capacity >= Fdesign)
							Reporting.AddLine("= " + capacity + " >= " + Fdesign + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + Fdesign + ConstUnit.Force + " (NG)");

						Reporting.AddHeader("Bolt Bearing on Column Web:");
						Fbs = CommonCalculations.SpacingBearing(colSplice.BoltGageWebLower, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, bColumn.Material.Fu, true);
						n = colSplice.BoltRowsWebLower * 2;
						capacity = n * Fbs * bColumn.Shape.tw * cN;
						Reporting.AddLine(ConstString.PHI + " Rn = N * Fbs * tw * (C / N) = " + n + " * " + Fbs + " * " + bColumn.Shape.tw + " * (" + C1 + " / " + n + ")");
						if (capacity >= Fdesign)
							Reporting.AddLine("= " + capacity + " >= " + Fdesign + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + Fdesign + ConstUnit.Force + " (NG)");

						Reporting.AddHeader("Splice " + part2 + ":");
						Reporting.AddHeader("Bolt Bearing on " + part2 + ":");
						Fbs = CommonCalculations.SpacingBearing(colSplice.BoltGageWebLower, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, true);
						Fbe = CommonCalculations.EdgeBearing(colSplice.BoltHorizEdgeDistanceWeb, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Material.Fu, colSplice.Bolt.HoleType, true);
						n = colSplice.BoltRowsWebLower;
						capacity = NumPLorCH * n * (Fbe + Fbs) * tw * cN;
						Reporting.AddLine(ConstString.PHI + " Rn = 2 * Nr * (Fbe +  Fbs) * t * (C / N)");
						Reporting.AddLine("= 2 * " + n + " * (" + Fbe + " + " + Fbs + ") * " + tw + " * (" + C1 + " / " + 2 * n + ")");
						if (capacity >= Fdesign)
							Reporting.AddLine("= " + capacity + " >= " + Fdesign + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + Fdesign + ConstUnit.Force + " (NG)");

						Reporting.AddHeader("Shear On Gross Area:");
						capacity = NumPLorCH * d * tw * ConstNum.FIOMEGA0_9N * 0.6 * colSplice.Material.Fy;
						Reporting.AddLine(ConstString.PHI + " Rn = 2 * d * t * " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy");
						Reporting.AddLine("= 2 * " + d + " * " + tw + " * " + ConstString.FIOMEGA0_9 + " * 0.6 * " + colSplice.Material.Fy);
						if (capacity >= F_Horizontal)
							Reporting.AddLine("= " + capacity + " >= " + F_Horizontal + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + F_Horizontal + ConstUnit.Force + " (NG)");

						Reporting.AddHeader("Shear On Net Area:");
						capacity = NumPLorCH * (d - 2 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * tw * ConstNum.FIOMEGA0_75N * 0.6 * colSplice.Material.Fu;
						Reporting.AddLine(ConstString.PHI + " Rn = 2 * (d - 2 * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t * " + ConstString.FIOMEGA0_75 + "*0.6 * Fu");
						Reporting.AddLine("= 2 * (" + d + " - 2 * (" + colSplice.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + tw + " * " + ConstString.FIOMEGA0_75 + "*0.6 * " + colSplice.Material.Fu);
						if (capacity >= F_Horizontal)
							Reporting.AddLine("= " + capacity + " >= " + F_Horizontal + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + F_Horizontal + ConstUnit.Force + " (NG)");

						if (colSplice.Bolt.BoltType != EBoltType.SC && colSplice.FillerThicknessWebLower > 0.75 * ConstNum.ONE_INCH)
						{
							Reporting.AddHeader("Aditional Bolts on Fillers:");
							FillerDForce = Fdesign * 2 * colSplice.FillerThicknessWebLower / (bColumn.Shape.tw + 2 * colSplice.FillerThicknessWebLower);
							Reporting.AddLine("Develop Fillers for Fr * 2 * t_fill/(tw + 2 * t_fill) = " + FillerDForce + ConstUnit.Force);

							Eccentricity = colSplice.SpliceLengthLowerWeb + colSplice.BoltVertEdgeDistanceColumn + (colSplice.FillerNumBoltRowsLW - 1) * colSplice.BoltVertSpacing / 2;
							MiscCalculationsWithReporting.EccentricBolt(Eccentricity, colSplice.BoltGageWebLower, colSplice.BoltVertSpacing, colSplice.FillerNumBoltRowsLW, 2, ref c);
							Reporting.AddLine("Eccentricity = " + Eccentricity + ConstUnit.Length);
							Reporting.AddLine("Bolt Grid: 2 X " + colSplice.FillerNumBoltRowsLW);
							Reporting.AddLine("Spacing: " + colSplice.BoltGageWebLower + " X " + colSplice.BoltVertSpacing + ConstUnit.Length);
							Reporting.AddLine("Bolt Capacity Coefficient (C) = " + c);
							if (F_Vertical > 0)
							{
								Reporting.AddHeader("Adjustment for Inclined Load:");
								Sintheta = F_Vertical / Fdesign;
								Costheta = F_Horizontal / Fdesign;
								Cmax = 2 * colSplice.FillerNumBoltRowsLW;
								a = Cmax / c;
								Ca = Math.Max(a * c / (Sintheta + a * Costheta), c);
								cN = Ca / (colSplice.FillerNumBoltRowsLW * 2);
								C1 = Ca;
								capacity = Ca * BFv;
								Reporting.AddLine(ConstString.PHI + " = " + Math.Atan(F_Vertical / F_Horizontal) * 180 / Math.PI + " Degrees");
								Reporting.AddLine("Cmax = " + Cmax + ";  A = " + a);
								Reporting.AddLine("Ca = Max(C; [A * C / (Sin(" + ConstString.PHI + ") + A * Cos(" + ConstString.PHI + "))])");
								Reporting.AddLine("= Max(" + c + "; [" + a + " * " + c + " / (" + Sintheta + " + " + a + " * " + Costheta + ")])");
								Reporting.AddLine("= " + Ca);
								Reporting.AddLine(ConstString.PHI + " Rn = Ca * Fv = " + Ca + " * " + BFv);
							}
							else
							{
								cN = c / (colSplice.FillerNumBoltRowsLW * 2);
								C1 = c;
								capacity = c * BFv;
								Reporting.AddLine(ConstString.PHI + " Rn = C * Fv = " + c + " * " + BFv);
							}
							if (capacity >= FillerDForce)
								Reporting.AddLine("= " + capacity + " >= " + FillerDForce + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= " + capacity + " << " + FillerDForce + ConstUnit.Force + " (NG)");

							Reporting.AddHeader("Bolt Bearing on Fillers:");
							Fbs = CommonCalculations.SpacingBearing(colSplice.BoltGageWebLower, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, true);
							Fbe = CommonCalculations.EdgeBearing(colSplice.BoltHorizEdgeDistanceWeb, (colSplice.Bolt.HoleLength), colSplice.Bolt.BoltSize, colSplice.Material.Fu, colSplice.Bolt.HoleType, true);
							n = colSplice.FillerNumBoltRowsLW;
							capacity = NumPLorCH * n * (Fbe + Fbs) * colSplice.FillerThicknessWebLower * cN;
							Reporting.AddLine(ConstString.PHI + " Rn = 2 * Nr * (Fbe + Fbs) * t * (C / N)");
							Reporting.AddLine("= 2 * " + n + " * (" + Fbe + " + " + Fbs + ") * " + colSplice.FillerThicknessWebLower + " * (" + C1 + " / " + 2 * n + ")");
							if (capacity >= FillerDForce)
								Reporting.AddLine("= " + capacity + " >= " + FillerDForce + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= " + capacity + " << " + FillerDForce + ConstUnit.Force + " (NG)");

							Reporting.AddHeader("Bolt Bearing on Column Web:");
							Fbs = CommonCalculations.SpacingBearing(colSplice.BoltGageWebLower, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, bColumn.Material.Fu, true);
							n = colSplice.FillerNumBoltRowsLW * 2;
							capacity = n * Fbs * bColumn.Shape.tw * cN;
							Reporting.AddLine(ConstString.PHI + " Rn = N * Fbs * tw * (C / N) = " + n + " * " + Fbs + " * " + bColumn.Shape.tw + " * (" + C1 + " / " + n + ")");
							if (capacity >= FillerDForce)
								Reporting.AddLine("= " + capacity + " >= " + FillerDForce + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= " + capacity + " << " + FillerDForce + ConstUnit.Force + " (NG)");
						}
					}
				}
				else
				{
					WeldsLowerWeb(d, ref FWeldSizeChToCol, tw, ref FWeldSizeChToFiller, ref FWeldSizeFillerToCol, part1, NumPLorCH, ref usefulweldonPlate, F_Horizontal, F_Vertical, ref dumy);
					Reporting.AddHeader(part1 + " Shear:");
					capacity = 2 * d * tw * ConstNum.FIOMEGA0_9N * 0.6 * colSplice.Material.Fy;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * d * t * " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy");
					Reporting.AddLine("= 2 * " + d + " * " + tw + " * " + ConstString.FIOMEGA0_9 + " * 0.6 * " + colSplice.Material.Fy);
					if (capacity >= F_Horizontal)
						Reporting.AddLine("= " + capacity + " >= " + F_Horizontal + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + capacity + " << " + F_Horizontal + ConstUnit.Force + " (NG)");
				}
			}
			if (ColumnConnection == EColumnSplice.UpperColumn || ColumnConnection == EColumnSplice.Both)
			{
				Reporting.AddHeader("Connection to Upper Column Web");
				if (colSplice.ConnectionUpper == EConnectionStyle.Bolted)
				{
					if (Fdesign == 0)
					{
						colSplice.SpliceThicknessWeb = ConstNum.THREE_EIGHTS_INCH;
						colSplice.BoltRowsWebUpper = 2;
						colSplice.BoltVertEdgeDistanceColumn = 2;
						colSplice.BoltVertSpacing = 6;
						colSplice.BoltVertEdgeDistancePlate = 2;
						colSplice.SpliceLengthUpperWeb = colSplice.BoltVertEdgeDistanceColumn + colSplice.BoltVertSpacing + colSplice.BoltVertEdgeDistancePlate;
						colSplice.FillerNumBoltRowsUW = 0;
						if (colSplice.FillerThicknessWebUpper > 0)
							colSplice.FillerLengthWebUpper = colSplice.SpliceLengthUpperWeb;
					}
					else
					{
						Reporting.AddHeader("Bolts on Upper Column Web");
						BoltsUpperWeb(tw, ref FuT, ref minsp, F_Horizontal, part2, ref minedge);
						Reporting.AddLine("Bolt Shear Capacity (Splice):");

						colSplice.SpliceWidthWeb = colSplice.BoltGageWebUpper + 2 * colSplice.BoltVertEdgeDistancePlate;

						if (colSplice.Bolt.BoltType == EBoltType.SC || colSplice.FillerThicknessWebUpper <= ConstNum.QUARTER_INCH)
							BFv = NumPLorCH * colSplice.Bolt.BoltStrength;
						else if (colSplice.FillerThicknessWebUpper <= 0.75 * ConstNum.ONE_INCH)
						{
							if (CommonDataStatic.Units == EUnit.US)
								reduction = 0.4 * (colSplice.FillerThicknessWebUpper - 0.25);
							else
								reduction = 0.0154 * (colSplice.FillerThicknessWebUpper - 6);
							BFv = NumPLorCH * colSplice.Bolt.BoltStrength * (1 - reduction);
							Reporting.AddHeader("Strength Reduction for Bolts:");
							if (CommonDataStatic.Units == EUnit.US)
							{
								Reporting.AddLine("Reduction Factor (r) = 1 - 0.4 * (t_fil - " + ConstNum.QUARTER_INCH + ")");
								Reporting.AddLine("= 1 - 0.4 * (" + colSplice.FillerThicknessWebUpper + " - " + ConstNum.QUARTER_INCH + ") = " + (1 - reduction));
							}
							else
							{
								Reporting.AddLine("Reduction Factor (r) = 1 - 0.0154 * (t_fil - 6 )");
								Reporting.AddLine("= 1 - 0.0154 * (" + colSplice.FillerThicknessWebUpper + " - 6 ) = " + (1 - reduction));
							}
						}
						else
							BFv = NumPLorCH * colSplice.Bolt.BoltStrength;

						Eccentricity = colSplice.BoltVertEdgeDistanceColumn + (colSplice.BoltRowsWebUpper - 1) * colSplice.BoltVertSpacing / 2;
						MiscCalculationsWithReporting.EccentricBolt(Eccentricity, colSplice.BoltGageWebUpper, colSplice.BoltVertSpacing, colSplice.BoltRowsWebUpper, 2, ref c);
						Reporting.AddLine("Eccentricity = " + Eccentricity + ConstUnit.Length);
						Reporting.AddLine("Bolt Grid: 2 X " + colSplice.BoltRowsWebUpper);
						Reporting.AddLine("Spacing: " + colSplice.BoltGageWebUpper + " X " + colSplice.BoltVertSpacing + ConstUnit.Length);
						Reporting.AddLine("Bolt Capacity Coefficient, C = " + c);
						if (F_Vertical > 0)
						{
							Reporting.AddLine("Adjustment for Inclined Load:");
							Sintheta = F_Vertical / Fdesign;
							Costheta = F_Horizontal / Fdesign;
							Cmax = 2 * colSplice.BoltRowsWebUpper;
							a = Cmax / c;
							Ca = Math.Max(a * c / (Sintheta + a * Costheta), c);
							cN = Ca / (colSplice.BoltRowsWebUpper * 2);
							C1 = Ca;
							capacity = Ca * BFv;
							Reporting.AddLine(ConstString.PHI + " = " + Math.Atan(F_Vertical / F_Horizontal) * 180 / Math.PI + " Degrees");
							Reporting.AddLine("Cmax = " + Cmax + ";  A = " + a);
							Reporting.AddLine("Ca = Max(C; [A * C / (Sin(" + ConstString.PHI + ") + A * Cos(" + ConstString.PHI + "))])");
							Reporting.AddLine("= Max(" + c + "; [" + a + " * " + c + " / (" + Sintheta + " + " + a + " * " + Costheta + ")])");
							Reporting.AddLine("= " + Ca);
							Reporting.AddLine(ConstString.PHI + " Rn = Ca * Fv = " + Ca + " * " + BFv);
						}
						else
						{
							cN = c / (colSplice.BoltRowsWebUpper * 2);
							C1 = c;
							capacity = c * BFv;
							Reporting.AddLine(ConstString.PHI + " Rn = C * Fv = " + c + " * " + BFv);
						}
						if (capacity >= Fdesign)
							Reporting.AddLine("= " + capacity + " >= " + Fdesign + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + Fdesign + ConstUnit.Force + " (NG)");

						Reporting.AddHeader("Bolt Bearing on Column Web:");
						Fbs = CommonCalculations.SpacingBearing(colSplice.BoltGageWebUpper, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, tColumn.Material.Fu, true);
						n = colSplice.BoltRowsWebUpper * 2;
						capacity = n * Fbs * tColumn.Shape.tw * cN;
						Reporting.AddLine(ConstString.PHI + " Rn = N * Fbs * tw * (C / N) = " + n + " * " + Fbs + " * " + tColumn.Shape.tw + " * (" + C1 + " / " + n + ")");
						if (capacity >= Fdesign)
							Reporting.AddLine("= " + capacity + " >= " + Fdesign + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + Fdesign + ConstUnit.Force + " (NG)");

						Reporting.AddHeader("Splice " + part2 + ":");
						Reporting.AddLine("Bolt Bearing on " + part2 + ":");
						Fbs = CommonCalculations.SpacingBearing(colSplice.BoltGageWebUpper, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, true);
						Fbe = CommonCalculations.EdgeBearing(colSplice.BoltVertEdgeDistancePlate, (colSplice.Bolt.HoleLength), colSplice.Bolt.BoltSize, colSplice.Material.Fu, colSplice.Bolt.HoleType, true);
						n = colSplice.BoltRowsWebUpper;
						capacity = NumPLorCH * n * (Fbe + Fbs) * tw * cN;
						Reporting.AddLine(ConstString.PHI + " Rn = 2 * Nr * (Fbe +  Fbs) * t * (C / N)");
						Reporting.AddLine("= 2 * " + n + " * (" + Fbe + " + " + Fbs + ") * " + tw + " * (" + C1 + " / " + 2 * n + ")");
						if (capacity >= Fdesign)
							Reporting.AddLine("= " + capacity + " >= " + Fdesign + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + Fdesign + ConstUnit.Force + " (NG)");

						Reporting.AddHeader("Shear On Gross Area:");
						capacity = 2 * d * tw * ConstNum.FIOMEGA0_9N * 0.6 * colSplice.Material.Fy;
						Reporting.AddLine(ConstString.PHI + " Rn = 2 * d * t * " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy");
						Reporting.AddLine("= 2 * " + d + " * " + tw + " * " + ConstString.FIOMEGA0_9 + " * 0.6 * " + colSplice.Material.Fy);
						if (capacity >= F_Horizontal)
							Reporting.AddLine("= " + capacity + " >= " + F_Horizontal + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + F_Horizontal + ConstUnit.Force + " (NG)");

						Reporting.AddHeader("Shear On Net Area:");
						capacity = 2 * (d - 2 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * tw * ConstNum.FIOMEGA0_75N * 0.6 * colSplice.Material.Fu;
						Reporting.AddLine(ConstString.PHI + " Rn = 2 * (d - 2 * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu");
						Reporting.AddLine("= 2 * (" + d + " - 2 * (" + colSplice.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + tw + " * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + colSplice.Material.Fu);
						if (capacity >= F_Horizontal)
							Reporting.AddLine("= " + capacity + " >= " + F_Horizontal + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + F_Horizontal + ConstUnit.Force + " (NG)");

						if (colSplice.Bolt.BoltType != EBoltType.SC && colSplice.FillerThicknessWebUpper > 0.75 * ConstNum.ONE_INCH)
						{
							Reporting.AddHeader("Aditional Bolts on Fillers:");
							FillerDForce = Fdesign * 2 * colSplice.FillerThicknessWebUpper / (tColumn.Shape.tw + 2 * colSplice.FillerThicknessWebUpper);
							Reporting.AddLine("Develop Fillers for Fr * 2 * t_fill/(tw + 2 * t_fill) = " + FillerDForce + ConstUnit.Force);

							Eccentricity = colSplice.SpliceLengthUpperWeb + colSplice.BoltVertEdgeDistanceColumn + (colSplice.FillerNumBoltRowsUW - 1) * colSplice.BoltVertSpacing / 2;
							MiscCalculationsWithReporting.EccentricBolt(Eccentricity, colSplice.BoltGageWebUpper, colSplice.BoltVertSpacing, colSplice.FillerNumBoltRowsUW, 2, ref c);
							Reporting.AddLine("Eccentricity = " + Eccentricity + ConstUnit.Length);
							Reporting.AddLine("Bolt Grid: 2 X " + colSplice.FillerNumBoltRowsUW);
							Reporting.AddLine("Spacing: " + colSplice.BoltGageWebUpper + " X " + colSplice.BoltVertSpacing + ConstUnit.Length);
							Reporting.AddLine("Bolt Capacity Coefficient, C = " + c);
							if (F_Vertical > 0)
							{
								Reporting.AddLine("Adjustment for Inclined Load:");
								Sintheta = F_Vertical / Fdesign;
								Costheta = F_Horizontal / Fdesign;
								Cmax = 2 * colSplice.FillerNumBoltRowsUW;
								a = Cmax / c;
								Ca = Math.Max(a * c / (Sintheta + a * Costheta), c);
								cN = Ca / (colSplice.FillerNumBoltRowsUW * 2);
								capacity = Ca * BFv;
								Reporting.AddLine(ConstString.PHI + " = " + Math.Atan(F_Vertical / F_Horizontal) * 180 / Math.PI + " Degrees");
								Reporting.AddLine("Cmax = " + Cmax + ";  A = " + a);
								Reporting.AddLine("Ca = Max(C, [A * C / (Sin(" + ConstString.PHI + ") + A * Cos(" + ConstString.PHI + "))])");
								Reporting.AddLine("= Max(" + c + "; [" + a + " * " + c + " / (" + Sintheta + " + " + a + " * " + Costheta + ")])");
								Reporting.AddLine("= " + Ca);
								Reporting.AddLine(ConstString.PHI + " Rn = Ca * Fv = " + Ca + " * " + BFv);
							}
							else
							{
								cN = c / (colSplice.FillerNumBoltRowsUW * 2);
								capacity = c * BFv;
								Reporting.AddLine(ConstString.PHI + " Rn = C * Fv = " + c + " * " + BFv);
							}
							if (capacity >= FillerDForce)
								Reporting.AddLine("= " + capacity + " >= " + FillerDForce + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= " + capacity + " << " + FillerDForce + ConstUnit.Force + " (NG)");
							Fbs = CommonCalculations.SpacingBearing(colSplice.BoltGageWebUpper, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, tColumn.Material.Fu, true);
							Fbe = CommonCalculations.EdgeBearing(colSplice.BoltVertEdgeDistancePlate, (colSplice.Bolt.HoleLength), colSplice.Bolt.BoltSize, colSplice.Material.Fu, colSplice.Bolt.HoleType, true);
							n = colSplice.FillerNumBoltRowsUW;
							capacity = NumPLorCH * n * (Fbe + Fbs) * colSplice.FillerThicknessWebUpper * cN;
							Reporting.AddLine(ConstString.PHI + " Rn = 2 * Nr * (Fbe + Fbs) * t * (C / N)");
							Reporting.AddLine("= 2 * " + n + " *(" + Fbe + " + " + Fbs + ") * " + colSplice.FillerThicknessWebUpper + " * (" + C1 + " / " + 2 * n + ")");
							if (capacity >= FillerDForce)
								Reporting.AddLine("= " + capacity + " >= " + FillerDForce + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= " + capacity + " << " + FillerDForce + ConstUnit.Force + " (NG)");

							Reporting.AddHeader("Bolt Bearing on Column Web:");
							Fbs = CommonCalculations.SpacingBearing(colSplice.BoltGageWebUpper, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, tColumn.Material.Fu, true);
							n = colSplice.FillerNumBoltRowsUW * 2;
							capacity = n * Fbs * tColumn.Shape.tw * cN;
							Reporting.AddLine(ConstString.PHI + " Rn = N * Fbs * tw * (C / N) = " + n + " * " + Fbs + " * " + tColumn.Shape.tw + " * (" + C1 + " / " + n + ")");
							if (capacity >= FillerDForce)
								Reporting.AddLine("= " + capacity + " >= " + FillerDForce + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= " + capacity + " << " + FillerDForce + ConstUnit.Force + " (NG)");
						}
					}
				}
				else
				{
					WeldsUpperWeb(d, ref FWeldSizeChToCol, tw, ref FWeldSizeChToFiller, ref FWeldSizeFillerToCol, part1, NumPLorCH, ref usefulweldonPlate, F_Horizontal, F_Vertical, ref dumy);
					Reporting.AddHeader(part1 + " Shear:");
					capacity = 2 * d * tw * ConstNum.FIOMEGA0_9N * 0.6 * colSplice.Material.Fy;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * d * t * " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy");
					Reporting.AddLine("= 2 * " + d + " * " + tw + " * " + ConstString.FIOMEGA0_9 + " * 0.6 * " + colSplice.Material.Fy);
					if (capacity >= F_Horizontal)
						Reporting.AddLine("= " + capacity + " >= " + F_Horizontal + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + capacity + " << " + F_Horizontal + ConstUnit.Force + " (NG)");
				}
			}
		}

		public static void WebPlateDesign(ref double dw, ref double d, ref double tw, double Fw, ref double reduction, ref double capacity, ref double CSMinX, ref double CSMinY, ref double PLLength, ref double PLType, ref double Gage, ref double usefulweldonPlate)
		{
			double AgReq = 0;
			double NumPLorCH = 0;
			double Fdesign = 0;
			double F_Vertical = 0;
			double F_Horizontal = 0;
			double BFv = 0;
			double Fbsc = 0;
			int Nvl = 0;
			double Eccentricity = 0;
			double c = 0;
			double Sintheta = 0;
			double Costheta = 0;
			double Cmax = 0;
			double a = 0;
			double Ca = 0;
			double Nvlweb = 0;
			double tg_req = 0;
			double Ln = 0;
			double tn_req = 0;
			double ts = 0;
			double Fbs = 0;
			double Fbe = 0;
			double tbr_ReqPl = 0;
			double FillerDForce = 0;
			double Nvlpbr = 0;
			double FWeldSizeChToCol = 0;
			double FWeldSizeChToFiller = 0;
			double FWeldSizeFillerToCol = 0;
			double w_yf = 0;

			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			dw = Math.Min(tColumn.Shape.t, bColumn.Shape.t);
			AgReq = colSplice.WebShear / (ConstNum.FIOMEGA0_9N * 0.6 * colSplice.Material.Fy);

			if (colSplice.ConnectionOption == ESpliceConnection.DirectlyWelded)
			{
				d = colSplice.Channel.d;
				tw = colSplice.Channel.tw;
				NumPLorCH = colSplice.NumberOfChannels;
				if (colSplice.ChannelType == ESpliceChannelType.Temporary)
				{
					Fdesign = 0;
					F_Vertical = 0;
					F_Horizontal = 0;
				}
				else
				{
					Fdesign = Math.Sqrt(Math.Pow(Fw, 2) + Math.Pow(colSplice.WebShear, 2));
					F_Vertical = Fw;
					F_Horizontal = colSplice.WebShear;
				}
			}
			else
			{
				d = colSplice.SpliceWidthWeb;
				tw = colSplice.SpliceThicknessWeb;
				NumPLorCH = 2;
				Fdesign = colSplice.WebShear;
				F_Vertical = 0;
				F_Horizontal = colSplice.WebShear;
			}

			if (colSplice.ConnectionLower == EConnectionStyle.Bolted)
			{
				colSplice.SpliceWidthWeb = colSplice.BoltGageWebLower + 2 * colSplice.BoltHorizEdgeDistanceWeb;
				if (Fdesign == 0)
				{
					colSplice.SpliceThicknessWeb = ConstNum.THREE_EIGHTS_INCH;
					colSplice.BoltRowsWebLower = 2;
					colSplice.BoltVertEdgeDistanceColumn = ConstNum.TWO_INCHES;
					colSplice.BoltVertSpacing = NumberFun.ConvertFromFraction(96);
					colSplice.BoltVertEdgeDistancePlate = ConstNum.TWO_INCHES;
					colSplice.SpliceLengthLowerWeb = colSplice.BoltVertEdgeDistanceColumn + colSplice.BoltVertSpacing + colSplice.BoltVertEdgeDistancePlate;
					colSplice.FillerNumBoltRowsLW = 0;
					if (colSplice.FillerThicknessWebLower > 0)
						colSplice.FillerLengthWebLower = colSplice.SpliceLengthLowerWeb;
				}
				else
				{
					if (colSplice.Bolt.BoltType == EBoltType.SC || colSplice.FillerThicknessWebLower <= ConstNum.QUARTER_INCH)
						BFv = NumPLorCH * colSplice.Bolt.BoltStrength;
					else if (colSplice.FillerThicknessWebLower <= 0.75 * ConstNum.ONE_INCH)
					{
						if (CommonDataStatic.Units == EUnit.US)
							reduction = 0.4 * (colSplice.FillerThicknessWebLower - 0.25);
						else
							reduction = 0.0154 * (colSplice.FillerThicknessWebLower - 6);
						BFv = NumPLorCH * colSplice.Bolt.BoltStrength * (1 - reduction);
					}
					else
						BFv = NumPLorCH * colSplice.Bolt.BoltStrength;

					Fbsc = CommonCalculations.SpacingBearing(colSplice.BoltGageWebLower, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, bColumn.Material.Fu, false);
					Nvl = 1;
					do
					{
						Nvl = Nvl + 1;
						Eccentricity = colSplice.BoltVertEdgeDistanceColumn + (Nvl - 1) * colSplice.BoltVertSpacing / 2;
						MiscCalculationsWithReporting.EccentricBolt(Eccentricity, colSplice.BoltGageWebLower, colSplice.BoltVertSpacing, Nvl, 2, ref c);
						Sintheta = F_Vertical / Fdesign;
						Costheta = F_Horizontal / Fdesign;
						Cmax = 2 * Nvl;
						a = Cmax / c;
						Ca = Math.Max(a * c / (Sintheta + a * Costheta), c);
						capacity = Ca * BFv;
						Nvlweb = Fdesign / (2 * Fbsc * bColumn.Shape.tw * Ca / Cmax);
					} while (Fdesign > capacity || Nvlweb > Nvl);
					colSplice.BoltRowsWebLower = Nvl;
					colSplice.SpliceLengthLowerWeb = colSplice.BoltVertEdgeDistanceColumn + (Nvl - 1) * colSplice.BoltVertSpacing + colSplice.BoltVertEdgeDistancePlate;
					if (colSplice.ConnectionOption != ESpliceConnection.DirectlyWelded)
					{
						tg_req = AgReq / (NumPLorCH * colSplice.SpliceWidthWeb);
						Ln = colSplice.SpliceWidthWeb - 2 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
						tn_req = F_Horizontal / (NumPLorCH * Ln * ConstNum.FIOMEGA0_75N * 0.6 * colSplice.Material.Fu);
						ts = Math.Max(tg_req, tn_req);
						Fbs = CommonCalculations.SpacingBearing(colSplice.BoltGageWebLower, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, false);
						Fbe = CommonCalculations.EdgeBearing(colSplice.BoltHorizEdgeDistanceWeb, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Material.Fu, colSplice.Bolt.HoleType, false);
						tbr_ReqPl = F_Horizontal / (NumPLorCH * colSplice.BoltRowsWebLower * (Fbe + Fbs) * (Ca / Cmax));
						colSplice.SpliceThicknessWeb = Math.Max(ts, Math.Max(tbr_ReqPl, ConstNum.THREE_EIGHTS_INCH));
						colSplice.SpliceThicknessWeb = NumberFun.Round(colSplice.SpliceThicknessWeb, ERoundingPrecision.Eighth, ERoundingStyle.RoundUp);
					}
					if (colSplice.Bolt.BoltType != EBoltType.SC && colSplice.FillerThicknessWebLower > 0.75 * ConstNum.ONE_INCH)
					{
						FillerDForce = Fdesign * 2 * colSplice.FillerThicknessWebLower / (bColumn.Shape.tw + 2 * colSplice.FillerThicknessWebLower);
						Fbsc = CommonCalculations.SpacingBearing(colSplice.BoltGageWebLower, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, bColumn.Material.Fu, false);
						Nvl = 1;
						do
						{
							Nvl++;
							Eccentricity = colSplice.SpliceLengthLowerWeb + colSplice.BoltVertEdgeDistanceColumn + (Nvl - 1) * colSplice.BoltVertSpacing / 2;
							MiscCalculationsWithReporting.EccentricBolt(Eccentricity, colSplice.BoltGageWebLower, colSplice.BoltVertSpacing, Nvl, 2, ref c);
							Sintheta = F_Vertical / Fdesign;
							Costheta = F_Horizontal / Fdesign;
							Cmax = 2 * Nvl;
							a = Cmax / c;
							Ca = Math.Max(a * c / (Sintheta + a * Costheta), c);
							capacity = Ca * BFv;
							Nvlpbr = FillerDForce / (NumPLorCH * (Fbe + Fbs) * colSplice.FillerThicknessWebLower * Ca / Cmax);
							Nvlweb = FillerDForce / (2 * Fbsc * bColumn.Shape.tw * Ca / Cmax);
						} while (FillerDForce > capacity || Nvlweb > Nvl || Nvlpbr > Nvl);

						colSplice.FillerNumBoltRowsLW = Nvl;
						colSplice.FillerLengthWebLower = colSplice.SpliceLengthLowerWeb + 2 * colSplice.BoltVertEdgeDistancePlate + (colSplice.FillerNumBoltRowsLW - 1) * colSplice.BoltVertSpacing;
					}
					else
					{
						colSplice.FillerNumBoltRowsLW = 0;
						if (colSplice.FillerThicknessWebLower > 0)
							colSplice.FillerLengthWebLower = colSplice.SpliceLengthLowerWeb;
					}
				}
			}
			else
				ColumnSplice.WeldsLowerWeb(ref d, ref tw, ref FWeldSizeChToCol, ref FWeldSizeChToFiller, ref FWeldSizeFillerToCol, ref CSMinX, ref CSMinY, ref PLLength, ref PLType, ref Gage, ref NumPLorCH, ref Fdesign, Fw, ref usefulweldonPlate, F_Horizontal, F_Vertical, ref w_yf);

			if (colSplice.ConnectionUpper == EConnectionStyle.Bolted)
			{
				if (Fdesign == 0)
				{
					colSplice.SpliceThicknessWeb = ConstNum.THREE_EIGHTS_INCH;
					colSplice.BoltRowsWebUpper = 2;
					colSplice.BoltVertSpacing = NumberFun.ConvertFromFraction(96);
					colSplice.BoltVertEdgeDistancePlate = ConstNum.TWO_INCHES;
					colSplice.BoltVertEdgeDistanceColumn = ConstNum.TWO_INCHES;
					colSplice.SpliceLengthUpperWeb = colSplice.BoltVertEdgeDistanceColumn + colSplice.BoltVertSpacing + colSplice.BoltVertEdgeDistancePlate;
					colSplice.FillerNumBoltRowsUW = 0;
					if (colSplice.FillerThicknessWebUpper > 0)
						colSplice.FillerLengthWebUpper = colSplice.SpliceLengthUpperWeb;
				}
				else
				{
					colSplice.SpliceWidthWeb = colSplice.BoltGageWebUpper + 2 * colSplice.BoltVertEdgeDistancePlate;

					if (colSplice.Bolt.BoltType == EBoltType.SC || colSplice.FillerThicknessWebUpper <= ConstNum.QUARTER_INCH)
						BFv = NumPLorCH * colSplice.Bolt.BoltStrength;
					else if (colSplice.FillerThicknessWebUpper <= 0.75 * ConstNum.ONE_INCH)
					{
						if (CommonDataStatic.Units == EUnit.US)
							reduction = 0.4 * (colSplice.FillerThicknessWebUpper - 0.25);
						else
							reduction = 0.0154 * (colSplice.FillerThicknessWebUpper - 6);

						BFv = NumPLorCH * colSplice.Bolt.BoltStrength * (1 - reduction);
					}
					else
						BFv = NumPLorCH * colSplice.Bolt.BoltStrength;

					Fbsc = CommonCalculations.SpacingBearing(colSplice.BoltGageWebUpper, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, bColumn.Material.Fu, false);
					Nvl = 1;
					
					do
					{
						Nvl++;
						Eccentricity = colSplice.BoltVertEdgeDistanceColumn + (Nvl - 1) * colSplice.BoltVertSpacing / 2;
						MiscCalculationsWithReporting.EccentricBolt(Eccentricity, colSplice.BoltGageWebUpper, colSplice.BoltVertSpacing, Nvl, 2, ref c);
						Sintheta = F_Vertical / Fdesign;
						Costheta = F_Horizontal / Fdesign;
						Cmax = 2 * Nvl;
						a = Cmax / c;
						Ca = Math.Max(a * c / (Sintheta + a * Costheta), c);
						capacity = Ca * BFv;
						Nvlweb = Fdesign / (2 * Fbsc * tColumn.Shape.tw * Ca / Cmax);
					} while (Fdesign > capacity || Nvlweb > Nvl);

					//do
					//{
					//	Nvl = Nvl + 1;
					//	Eccentricity = DESGEN.Colsplice.BoltVertEdgeonUC + (Nvl - 1) * DESGEN.colSplice.BoltVertSpacing / 2;
					//	DDESIGN.EccentricBolt(Eccentricity.ToDbl(), DESGEN.Colsplice.BoltGageOnUCW.ToDbl(), DESGEN.colSplice.BoltVertSpacing.ToDbl(), Nvl.ToDbl(), 2, ref c);
					//	Sintheta = F_Vertical / Fdesign;
					//	Costheta = F_Horizontal / Fdesign;
					//	Cmax = 2 * Nvl;
					//	a = Cmax / c;
					//	Ca = DDESIGN.max((float)(a * c / (Sintheta + a * Costheta)), c);
					//	capacity = Ca * BFv;
					//	Nvlweb = Fdesign / (2 * Fbsc * DESGEN.TColumn.tw * Ca / Cmax);
					//} while (Fdesign > capacity || Nvlweb > Nvl);

					colSplice.BoltRowsWebUpper = Nvl;
					
					colSplice.SpliceLengthUpperWeb = colSplice.BoltVertEdgeDistanceColumn + (Nvl - 1) * colSplice.BoltVertSpacing + colSplice.BoltVertEdgeDistancePlate;
					if (colSplice.ConnectionOption == ESpliceConnection.DirectlyWelded)
					{
						Fbs = CommonCalculations.SpacingBearing(colSplice.BoltGageWebUpper, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, false);
					}
					else
					{
						// gross and net shear
						tg_req = AgReq / (NumPLorCH * colSplice.SpliceWidthWeb);
						Ln = colSplice.SpliceWidthWeb - 2 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
						tn_req = F_Horizontal / (NumPLorCH * Ln * ConstNum.FIOMEGA0_75N * 0.6 * colSplice.Material.Fu);
						ts = Math.Max(tg_req, tn_req);
						// bolt bearing
						Fbs = CommonCalculations.SpacingBearing(colSplice.BoltGageWebUpper, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, false);
						Fbe = CommonCalculations.EdgeBearing(colSplice.BoltVertEdgeDistancePlate, (colSplice.Bolt.HoleLength), colSplice.Bolt.BoltSize, colSplice.Material.Fu, colSplice.Bolt.HoleType, false);
						tbr_ReqPl = F_Horizontal / (NumPLorCH * colSplice.BoltRowsWebUpper * (Fbe + Fbs) * (Ca / Cmax));
						colSplice.SpliceThicknessWeb = Math.Max(ts, Math.Max(tbr_ReqPl, ConstNum.THREE_EIGHTS_INCH));
						colSplice.SpliceThicknessWeb = NumberFun.Round(colSplice.SpliceThicknessWeb, ERoundingPrecision.Eighth, ERoundingStyle.RoundUp);
					}
					if (colSplice.Bolt.BoltType != EBoltType.SC && colSplice.FillerThicknessWebUpper > 0.75 * ConstNum.ONE_INCH)
					{
						FillerDForce = Fdesign * 2 * colSplice.FillerThicknessWebUpper / (tColumn.Shape.tw + 2 * colSplice.FillerThicknessWebUpper);

						Fbsc = CommonCalculations.SpacingBearing(colSplice.BoltGageWebUpper, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, tColumn.Material.Fu, false);
						Nvl = 1;
						do
						{
							Nvl = Nvl + 1;
							Eccentricity = colSplice.SpliceLengthUpperWeb + colSplice.BoltVertEdgeDistanceColumn + (Nvl - 1) * colSplice.BoltVertSpacing / 2;
							MiscCalculationsWithReporting.EccentricBolt(Eccentricity, colSplice.BoltGageWebUpper, colSplice.BoltVertSpacing, Nvl, 2, ref c);
							Sintheta = F_Vertical / Fdesign;
							Costheta = F_Horizontal / Fdesign;
							Cmax = 2 * Nvl;
							a = Cmax / c;
							Ca = Math.Max(a * c / (Sintheta + a * Costheta), c);
							capacity = Ca * BFv;
							if (colSplice.ConnectionOption == ESpliceConnection.DirectlyWelded)
								Nvlpbr = FillerDForce / (NumPLorCH * 2 * Fbs * colSplice.FillerThicknessWebUpper * Ca / Cmax);
							else
								Nvlpbr = FillerDForce / (NumPLorCH * (Fbe + Fbs) * colSplice.FillerThicknessWebUpper * Ca / Cmax);
							Nvlweb = FillerDForce / (2 * Fbsc * tColumn.Shape.tw * Ca / Cmax);
						} while (Fdesign > capacity || Nvlweb > Nvl || Nvlpbr > Nvl);

						colSplice.FillerNumBoltRowsUW = Nvl;
						colSplice.FillerLengthWebUpper = colSplice.SpliceLengthUpperWeb + 2 * colSplice.BoltVertEdgeDistancePlate + (colSplice.FillerNumBoltRowsUW - 1) * colSplice.BoltVertSpacing;
					}
					else
					{
						colSplice.FillerNumBoltRowsUW = 0;
						if (colSplice.FillerThicknessWebUpper > 0)
							colSplice.FillerLengthWebUpper = colSplice.SpliceLengthUpperWeb;
					}
				}
			}
			else
				ColumnSplice.WeldsUpperWeb(ref d, ref tw, ref FWeldSizeChToCol, ref FWeldSizeChToFiller, ref FWeldSizeFillerToCol, ref CSMinX, ref CSMinY, ref PLLength, ref PLType, ref Gage, ref NumPLorCH, ref Fdesign, Fw, ref usefulweldonPlate, F_Horizontal, F_Vertical, ref w_yf);
		}

		private static void BoltsLowerWeb(double tw, ref double FuT, ref double minsp, double F_Horizontal, string part2, ref double minedge)
		{
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Bolt Vertical Spacing:");
			FuT = Math.Min(2 * tw * colSplice.Material.Fu, bColumn.Shape.tw * bColumn.Material.Fu);
			minsp = MiscCalculationsWithReporting.MinimumSpacing((colSplice.Bolt.BoltSize), 0, FuT, Math.Max(colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleWidth), colSplice.Bolt.HoleType);
			if (colSplice.BoltVertSpacing >= minsp)
				Reporting.AddLine("s = " + colSplice.BoltVertSpacing + " >= " + minsp + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("s = " + colSplice.BoltVertSpacing + " << " + minsp + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Bolt Horizontal  Spacing:");
			FuT = Math.Min(2 * tw * colSplice.Material.Fu, bColumn.Shape.tw * bColumn.Material.Fu);
			minsp = MiscCalculationsWithReporting.MinimumSpacing((colSplice.Bolt.BoltSize), F_Horizontal / (2 * colSplice.BoltRowsWebLower), FuT, Math.Max(colSplice.Bolt.HoleLength, colSplice.Bolt.HoleLength), colSplice.Bolt.HoleType);
			if (colSplice.BoltGageWebLower >= minsp)
				Reporting.AddLine("g = " + colSplice.BoltGageWebLower + " >= " + minsp + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("g = " + colSplice.BoltGageWebLower + " << " + minsp + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Bolt Vertical Edge Dist. on " + part2 + ":");
			FuT = 2 * tw * colSplice.Material.Fu;
			minedge = MiscCalculationsWithReporting.MinimumEdgeDist(colSplice.Bolt.BoltSize, 0, FuT, colSplice.Bolt.MinEdgeSheared, (int)colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleType);
			if (colSplice.BoltVertEdgeDistancePlate >= minedge)
				Reporting.AddLine("Ev = " + colSplice.BoltVertEdgeDistancePlate + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Ev = " + colSplice.BoltVertEdgeDistancePlate + " << " + minedge + ConstUnit.Length + " (NG)");

			if (colSplice.ConnectionOption != ESpliceConnection.DirectlyWelded)
			{
				Reporting.AddHeader("Bolt Horizontal Edge Dist. on " + part2 + ":");
				FuT = 2 * tw * colSplice.Material.Fu;
				minedge = MiscCalculationsWithReporting.MinimumEdgeDist(colSplice.Bolt.BoltSize, F_Horizontal / (2 * colSplice.BoltRowsWebLower), FuT, colSplice.Bolt.MinEdgeSheared, (int)colSplice.Bolt.HoleLength, colSplice.Bolt.HoleType);
				if (colSplice.BoltHorizEdgeDistanceWeb >= minedge)
					Reporting.AddLine("Eh = " + colSplice.BoltHorizEdgeDistanceWeb + " >= " + minedge + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Eh = " + colSplice.BoltHorizEdgeDistanceWeb + " << " + minedge + ConstUnit.Length + " (NG)");
			}
			Reporting.AddHeader("Bolt Vertical Edge Dist. on Column:");
			FuT = bColumn.Shape.tw * colSplice.Material.Fu;
			minedge = MiscCalculationsWithReporting.MinimumEdgeDist(colSplice.Bolt.BoltSize, 0, FuT, colSplice.Bolt.MinEdgeSheared, (int)colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleType);
			if (colSplice.BoltVertEdgeDistanceColumn >= minedge)
				Reporting.AddLine("Evc = " + colSplice.BoltVertEdgeDistanceColumn + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Evc = " + colSplice.BoltVertEdgeDistanceColumn + " << " + minedge + ConstUnit.Length + " (NG)");
		}

		private static void WeldsLowerWeb(double d, ref double FWeldSizeChToCol, double tw, ref double FWeldSizeChToFiller, ref double FWeldSizeFillerToCol, string part1, double NumPLorCH, ref double usefulweldonPlate, double F_Horizontal, double F_Vertical, ref double dumy)
		{
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			FWeldSizeChToCol = CommonCalculations.MinimumWeld(bColumn.Shape.tw, d);
			FWeldSizeChToFiller = CommonCalculations.MinimumWeld(colSplice.FillerThicknessWebLower, tw);
			FWeldSizeFillerToCol = CommonCalculations.MinimumWeld(bColumn.Shape.tw, colSplice.FillerThicknessWebLower);

			Reporting.AddHeader("Welds:");
			if (colSplice.FillerThicknessWebLower == 0)
			{
				Reporting.AddHeader(part1 + "/Web Weld:" + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebLower) + ConstUnit.Length);
				if (FWeldSizeChToCol <= colSplice.FilletWeldSizeWebLower)
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(FWeldSizeChToCol) + " <= " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebLower) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(FWeldSizeChToCol) + " >> " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebLower) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Splice " + part1 + ":");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * colSplice.SpliceThicknessWeb, bColumn.Material.Fu * bColumn.Shape.tw / NumPLorCH) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t; Fuc * tw') / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + tw + "; " + bColumn.Material.Fu + " * " + bColumn.Shape.tw / NumPLorCH + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebLower)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FilletWeldSizeWebLower + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.WeldLengthYLW, d, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FilletWeldSizeWebLower + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.WeldLengthYLW, d, colSplice.FilletWeldSizeWebLower, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				}
			}
			else if (colSplice.FillerThicknessWebLower < ConstNum.QUARTER_INCH)
			{
				Reporting.AddHeader(part1 + "/Web Weld (with fillers):" + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebLower) + ConstUnit.Length);
				Reporting.AddLine("Useful Weld Size on Splice " + part1 + " Filler:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * tw, bColumn.Material.Fu * bColumn.Shape.tw / NumPLorCH) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t; Fuc * tw') / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + tw + "; " + bColumn.Material.Fu + " * " + bColumn.Shape.tw / NumPLorCH + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebLower - colSplice.FillerThicknessWebLower)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w - t_fill = " + (colSplice.FilletWeldSizeWebLower - colSplice.FillerThicknessWebLower) + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.WeldLengthYLW, d, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w - t_fill = " + (colSplice.FilletWeldSizeWebLower - colSplice.FillerThicknessWebLower) + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.WeldLengthYLW, d, (colSplice.FilletWeldSizeWebLower - colSplice.FillerThicknessWebLower), F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				}
			}
			else
			{
				Reporting.AddHeader(part1 + "/Filler Weld:" + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebLower) + ConstUnit.Length);
				if (FWeldSizeChToFiller <= colSplice.FilletWeldSizeWebLower)
					Reporting.AddLine("Min. Weld Size = " + CommonCalculations.WeldSize(FWeldSizeChToFiller) + " <= " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebLower) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Min. Weld Size = " + CommonCalculations.WeldSize(FWeldSizeChToFiller) + " >> " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebLower) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Splice " + part1 + " / Filler:");
				usefulweldonPlate = colSplice.Material.Fu * Math.Min(tw, colSplice.FillerThicknessWebLower) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Fu * Min(t; t_fill) / (0.707 * Fexx)");
				Reporting.AddLine("= " + colSplice.Material.Fu + " * Min(" + tw + "; " + colSplice.FillerThicknessWebLower + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebLower)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FilletWeldSizeWebLower + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.WeldLengthYLW, d, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FilletWeldSizeWebLower + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.WeldLengthYLW, d, colSplice.FilletWeldSizeWebLower, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				}

				Reporting.AddHeader("Filler/Web Weld:" + CommonCalculations.WeldSize(colSplice.FillerWeldSizeLW) + ConstUnit.Length);
				if (FWeldSizeFillerToCol <= colSplice.FillerWeldSizeLW)
					Reporting.AddLine("Min. Weld Size (Filler/Web) = " + CommonCalculations.WeldSize(FWeldSizeFillerToCol) + " <= " + CommonCalculations.WeldSize(colSplice.FillerWeldSizeLW) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size (Filler/Web) = " + CommonCalculations.WeldSize(FWeldSizeFillerToCol) + " >> " + CommonCalculations.WeldSize(colSplice.FillerWeldSizeLW) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Filler/Web:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * colSplice.FillerThicknessWebLower, bColumn.Material.Fu * bColumn.Shape.tw / NumPLorCH) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t_fill; Fuc * tw) / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + colSplice.FillerThicknessWebLower + "; " + bColumn.Material.Fu + " * " + bColumn.Shape.tw / NumPLorCH + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FillerWeldSizeLW)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FillerWeldSizeLW + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.FillerWeldLengthLW, colSplice.FillerWidthWebLower, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "II");
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FillerWeldSizeLW + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.FillerWeldLengthLW, colSplice.FillerWidthWebLower, colSplice.FillerWeldSizeLW, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "II");
				}
			}
		}

		private static void BoltsUpperWeb(double tw, ref double FuT, ref double minsp, double F_Horizontal, string part2, ref double minedge)
		{
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Bolt Vertical Spacing:");
			FuT = Math.Min(2 * tw * colSplice.Material.Fu, tColumn.Shape.tw * tColumn.Material.Fu);
			minsp = MiscCalculationsWithReporting.MinimumSpacing((colSplice.Bolt.BoltSize), 0, FuT, Math.Max(colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleWidth), colSplice.Bolt.HoleType);
			if (colSplice.BoltVertSpacing >= minsp)
				Reporting.AddLine("s = " + colSplice.BoltVertSpacing + " >= " + minsp + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("s = " + colSplice.BoltVertSpacing + " << " + minsp + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Bolt Horizontal  Spacing:");
			FuT = Math.Min(2 * tw * colSplice.Material.Fu, tColumn.Shape.tw * tColumn.Material.Fu);
			minsp = MiscCalculationsWithReporting.MinimumSpacing((colSplice.Bolt.BoltSize), F_Horizontal / (2 * colSplice.BoltRowsWebUpper), FuT, Math.Max(colSplice.Bolt.HoleLength, colSplice.Bolt.HoleLength), colSplice.Bolt.HoleType);
			if (colSplice.BoltGageWebUpper >= minsp)
				Reporting.AddLine("g = " + colSplice.BoltGageWebUpper + " >= " + minsp + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("g = " + colSplice.BoltGageWebUpper + " << " + minsp + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Bolt Vertical Edge Dist. on " + part2 + ":");
			FuT = 2 * tw * colSplice.Material.Fu;
			minedge = MiscCalculationsWithReporting.MinimumEdgeDist(colSplice.Bolt.BoltSize, 0, FuT, colSplice.Bolt.MinEdgeSheared, colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleType);
			if (colSplice.BoltVertEdgeDistancePlate >= minedge)
				Reporting.AddLine("Ev = " + colSplice.BoltVertEdgeDistancePlate + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Ev = " + colSplice.BoltVertEdgeDistancePlate + " << " + minedge + ConstUnit.Length + " (NG)");

			if (colSplice.ConnectionOption != ESpliceConnection.DirectlyWelded)
			{
				Reporting.AddHeader("Bolt Horizontal Edge Dist. on " + part2 + ":");
				FuT = 2 * tw * colSplice.Material.Fu;
				minedge = MiscCalculationsWithReporting.MinimumEdgeDist(colSplice.Bolt.BoltSize, F_Horizontal / (2 * colSplice.BoltRowsWebUpper), FuT, colSplice.Bolt.MinEdgeSheared, (int)colSplice.Bolt.HoleLength, colSplice.Bolt.HoleType);
				if (colSplice.BoltVertEdgeDistancePlate >= minedge)
					Reporting.AddLine("Eh = " + colSplice.BoltVertEdgeDistancePlate + " >= " + minedge + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Eh = " + colSplice.BoltVertEdgeDistancePlate + " << " + minedge + ConstUnit.Length + " (NG)");
			}
			Reporting.AddHeader("Bolt Vertical Edge Dist. on Column:");
			FuT = tColumn.Shape.tw * tColumn.Material.Fu;
			minedge = MiscCalculationsWithReporting.MinimumEdgeDist(colSplice.Bolt.BoltSize, 0, FuT, colSplice.Bolt.MinEdgeSheared, colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleType);
			if (colSplice.BoltVertEdgeDistanceColumn >= minedge)
				Reporting.AddLine("Evc = " + colSplice.BoltVertEdgeDistanceColumn + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Evc = " + colSplice.BoltVertEdgeDistanceColumn + " << " + minedge + ConstUnit.Length + " (NG)");
		}

		private static void WeldsUpperWeb(double d, ref double FWeldSizeChToCol, double tw, ref double FWeldSizeChToFiller, ref double FWeldSizeFillerToCol, string part1, double NumPLorCH, ref double usefulweldonPlate, double F_Horizontal, double F_Vertical, ref double dumy)
		{
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			FWeldSizeChToCol = CommonCalculations.MinimumWeld(tColumn.Shape.tw, d);
			FWeldSizeChToFiller = CommonCalculations.MinimumWeld(colSplice.FillerThicknessWebUpper, tw);
			FWeldSizeFillerToCol = CommonCalculations.MinimumWeld(tColumn.Shape.tw, colSplice.FillerThicknessWebUpper);

			Reporting.AddHeader("Welds:");
			if (colSplice.FillerThicknessWebUpper == 0)
			{
				Reporting.AddHeader(part1 + " / Web Weld:" + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebUpper) + ConstUnit.Length);
				if (FWeldSizeChToCol <= colSplice.FilletWeldSizeWebUpper)
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(FWeldSizeChToCol) + " <= " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebUpper) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(FWeldSizeChToCol) + " >> " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebUpper) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Splice " + part1 + ":");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * tw, tColumn.Material.Fu * tColumn.Shape.tw / NumPLorCH) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t; Fuc * tw') / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + tw + "; " + tColumn.Material.Fu + " * " + tColumn.Shape.tw / NumPLorCH + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebUpper)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FilletWeldSizeWebUpper + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.WeldLengthYUW, d, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FilletWeldSizeWebUpper + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.WeldLengthYUW, d, colSplice.FilletWeldSizeWebUpper, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				}
			}
			else if (colSplice.FillerThicknessWebUpper < ConstNum.QUARTER_INCH)
			{
				Reporting.AddHeader(part1 + "/Web Weld (with fillers):" + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebUpper) + ConstUnit.Length);
				Reporting.AddHeader("Useful Weld Size on Splice Channel & Filler:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * tw, tColumn.Material.Fu * tColumn.Shape.tw / NumPLorCH) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t; Fuc * tw') / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + tw + "; " + tColumn.Material.Fu + " * " + tColumn.Shape.tw / NumPLorCH + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebUpper - colSplice.FillerThicknessWebUpper)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w - t_fill = " + (colSplice.FilletWeldSizeWebUpper - colSplice.FillerThicknessWebUpper) + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.WeldLengthYUW, d, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w - t_fill = " + (colSplice.FilletWeldSizeWebUpper - colSplice.FillerThicknessWebUpper) + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.WeldLengthYUW, d, (colSplice.FilletWeldSizeWebUpper - colSplice.FillerThicknessWebUpper), F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				}
			}
			else
			{
				Reporting.AddHeader(part1 + "/Filler Weld:" + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebUpper) + ConstUnit.Length);
				if (FWeldSizeChToFiller <= colSplice.FilletWeldSizeWebUpper)
					Reporting.AddLine("Min. Weld Size = " + CommonCalculations.WeldSize(FWeldSizeChToFiller) + " <= " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebUpper) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Min. Weld Size = " + CommonCalculations.WeldSize(FWeldSizeChToFiller) + " >> " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebUpper) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size:");
				usefulweldonPlate = colSplice.Material.Fu * Math.Min(tw, colSplice.FillerThicknessWebUpper) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Fu * Min(t; t_fill) / (0.707 * Fexx)");
				Reporting.AddLine("= " + colSplice.Material.Fu + " * Min(" + tw + "; " + colSplice.FillerThicknessWebUpper + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebUpper)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FilletWeldSizeWebUpper + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.WeldLengthYUW, d, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FilletWeldSizeWebUpper + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.WeldLengthYUW, d, colSplice.FilletWeldSizeWebUpper, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				}
				Reporting.AddHeader("Filler/Web Weld:" + CommonCalculations.WeldSize(colSplice.FillerWeldSizeUW) + ConstUnit.Length);
				if (FWeldSizeFillerToCol <= colSplice.FillerWeldSizeUW)
					Reporting.AddLine("Min. Weld Size (Filler/Web) = " + CommonCalculations.WeldSize(FWeldSizeFillerToCol) + " <= " + CommonCalculations.WeldSize(colSplice.FillerWeldSizeUW) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size (Filler/Web) = " + CommonCalculations.WeldSize(FWeldSizeFillerToCol) + " >> " + CommonCalculations.WeldSize(colSplice.FillerWeldSizeUW) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Filler/Web:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * colSplice.FillerThicknessWebUpper, tColumn.Material.Fu * tColumn.Shape.tw / NumPLorCH) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t_fill; Fuc * tw) / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + colSplice.FillerThicknessWebUpper + "; " + tColumn.Material.Fu + " * " + tColumn.Shape.tw / NumPLorCH + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FillerWeldSizeUW)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FillerWeldSizeUW + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.FillerWeldLengthUW, colSplice.FillerWidthWebUpper, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "II");
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FillerWeldSizeUW + ConstUnit.Length);
					dumy = ColumnSpliceMisc.WeldLengthForShear(true, colSplice.FillerWeldLengthUW, colSplice.FillerWidthWebUpper, colSplice.FillerWeldSizeUW, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "II");
				}
			}
		}
	}
}