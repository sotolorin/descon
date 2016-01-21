using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class ColumnSpliceMisc
	{
		public static double MinimumPJPSize(double t1, double t2)
		{
			double minimumPJPSize;
			double tmpCaseArg;

			if (CommonDataStatic.Units == EUnit.US)
			{
				tmpCaseArg = Math.Min(t1, t2);
				if (tmpCaseArg <= 0.25)
					minimumPJPSize = 0.125;
				else if (tmpCaseArg <= 0.5)
					minimumPJPSize = 0.1875;
				else if (tmpCaseArg <= 0.75)
					minimumPJPSize = 0.25;
				else if (tmpCaseArg <= 1.5)
					minimumPJPSize = 0.3125;
				else if (tmpCaseArg <= 2.25)
					minimumPJPSize = 0.375;
				else if (tmpCaseArg <= 6)
					minimumPJPSize = 0.5;
				else
					minimumPJPSize = 0.625;

				if (minimumPJPSize > t1)
					minimumPJPSize = t1;
				if (minimumPJPSize > t2)
					minimumPJPSize = t2;
			}
			else
			{
				tmpCaseArg = Math.Min(t1, t2);
				if (tmpCaseArg <= 6)
					minimumPJPSize = 3;
				else if (tmpCaseArg <= 13)
					minimumPJPSize = 5;
				else if (tmpCaseArg <= 19)
					minimumPJPSize = 6;
				else if (tmpCaseArg <= 38)
					minimumPJPSize = 8;
				else if (tmpCaseArg <= 57)
					minimumPJPSize = 10;
				else if (tmpCaseArg <= 150)
					minimumPJPSize = 13;
				else
					minimumPJPSize = 16;
				if (minimumPJPSize > t1)
					minimumPJPSize = t1;
				if (minimumPJPSize > t2)
					minimumPJPSize = t2;
			}

			return minimumPJPSize;
		}

		public static double WeldLengthForShear(bool enableReporting, double k_L, double L, double w, double V, double P, double e, string wt)
		{
			double weldLengthForShear = 0;
			double fr;
			double f4;
			double f3;
			double f2;
			double F1;
			double M;
			double Ip;
			double k;
			double kL;

			if (V == 0 && P == 0)
			{
				weldLengthForShear = 0;
				return weldLengthForShear;
			}

			if (!enableReporting)
			{
				kL = L - ConstNum.HALF_INCH;
				do
				{
					kL = kL + ConstNum.HALF_INCH;
					k = kL / L;
					if (wt == "C")
					{
						Ip = Math.Pow(L, 3) * (Math.Pow(1 + 2 * k, 3) / 12 - Math.Pow(k, 2) * Math.Pow(1 + k, 2) / (1 + 2 * k));
						M = V * (k * (1 + k) * L / (1 + 2 * k) + e);
						F1 = V / ((1 + 2 * k) * L);
						f2 = M * k * (1 + k) * L / (1 + 2 * k) / Ip;
						f3 = M * L / (2 * Ip);
						f4 = P / ((1 + 2 * k) * L);
					}
					else
					{
						Ip = k * Math.Pow(L, 3) / 2 * (1 + Math.Pow(k, 2) / 3);
						M = V * (k * L / 2 + e);
						F1 = V / (2 * k * L);
						f2 = M * k * L / (2 * Ip);
						f3 = M * L / (2 * Ip);
						f4 = P / (2 * k * L);
					}
					fr = Math.Sqrt(Math.Pow(F1 + f2, 2) + Math.Pow(f3 + f4, 2)) / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * w);
				} while (fr > 1);
				weldLengthForShear = kL;
			}
			else
			{
				k = k_L / L;

				Reporting.AddHeader("Weld Capacity:");
				Reporting.AddLine("L = " + L + ConstUnit.Length + " " + "k = " + k);
				Reporting.AddLine("Vertical Force (P) = " + P + ConstUnit.Force);
				Reporting.AddLine("Horizontal Force (V) = " + V + ConstUnit.Force);
				Reporting.AddLine("Weld Size (w) = " + w + ConstUnit.Length);

				if (wt == "C")
				{
					Ip = Math.Pow(L, 3) * (Math.Pow(1 + 2 * k, 3) / 12 - Math.Pow(k, 2) * Math.Pow(1 + k, 2) / (1 + 2 * k));
					M = V * (k * (1 + k) * L / (1 + 2 * k) + e);
					F1 = V / ((1 + 2 * k) * L);
					f2 = M * k * (1 + k) * L / (1 + 2 * k) / Ip;
					f3 = M * L / (2 * Ip);
					f4 = P / ((1 + 2 * k) * L);
				}
				else
				{
					Ip = k * Math.Pow(L, 3) / 2 * (1 + Math.Pow(k, 2) / 3);
					M = V * (k * L / 2 + e);
					F1 = V / (2 * k * L);
					f2 = M * k * L / (2 * Ip);
					f3 = M * L / (2 * Ip);
					f4 = P / (2 * k * L);
				}

				fr = Math.Sqrt(Math.Pow(F1 + f2, 2) + Math.Pow(f3 + f4, 2)) / (0.707 * w);
				Reporting.AddLine("Ip = " + Ip + ConstUnit.SecMod);
				Reporting.AddLine("M = " + M + ConstUnit.Moment);

				Reporting.AddHeader("Max. Weld Forces per Unit Length:");
				Reporting.AddLine("From V, f1 = " + F1 + ConstUnit.ForcePerUnitLength);
				Reporting.AddLine("From M, f2 = " + f2 + " (Hoizontal) " + ConstUnit.ForcePerUnitLength);
				Reporting.AddLine("From M, f3 = " + f3 + " (Vertical) " + ConstUnit.ForcePerUnitLength);
				Reporting.AddLine("From P, f4 = " + f4 + ConstUnit.ForcePerUnitLength);

				Reporting.AddHeader("Resultant Stress:");
				Reporting.AddLine("fr = [(f1 + f2) ^ 2 + (f3 + f4) ^ 2]^0.5  / (0.707 * w)");
				Reporting.AddLine("= [(" + F1 + " + " + f2 + ")² + (" + f3 + " + " + f4 + ")²]^0.5 / (0.707 * " + w + ")");
				if (fr <= 0.45 * CommonDataStatic.Preferences.DefaultElectrode.Fexx)
					Reporting.AddLine("= " + fr + " <= 0.45 * Fexx = " + 0.45 * CommonDataStatic.Preferences.DefaultElectrode.Fexx + ConstUnit.Stress + " (OK)");
				else
					Reporting.AddLine("= " + fr + " <= 0.45 * Fexx = " + 0.45 * CommonDataStatic.Preferences.DefaultElectrode.Fexx + ConstUnit.Stress + " (NG)");
			}

			return weldLengthForShear;
		}

		public static void ColumnStressCheck()
		{
			double Zef = 0;
			double Sef = 0;
			double Afe = 0;
			double Afn = 0;
			double Afg = 0;
			double CapacityN = 0;
			double An = 0;
			double U = 0;
			double capacity = 0;

			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			if (colSplice.Tension == 0 && colSplice.Moment == 0)
				return;

			Reporting.AddHeader("Upper Column Tension and Moment Check");
			Reporting.AddHeader("Tension:");
			Reporting.AddHeader("On Gross Area:");
			capacity = ConstNum.FIOMEGA0_9N * tColumn.Material.Fy * tColumn.Shape.a;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * A = " + ConstString.FIOMEGA0_9 + " * " + tColumn.Material.Fy + " * " + tColumn.Shape.a);
			if (capacity >= colSplice.Tension)
				Reporting.AddLine("= " + capacity + " >= " + colSplice.Tension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + capacity + " << " + colSplice.Tension + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("On Effective Net Area:");

			switch (colSplice.ConnectionOption)
			{
				case ESpliceConnection.FlangeAndWebPlate:
				case ESpliceConnection.FlangePlate:
					if (colSplice.ConnectionUpper == EConnectionStyle.Bolted)
					{
						if (tColumn.Shape.bf >= 2 / 3 * tColumn.Shape.d && colSplice.BoltRowsFlangeUpper >= 3)
							U = 0.9;
						else if (colSplice.BoltRowsFlangeUpper >= 3)
							U = 0.85;
						else
							U = 0.75;
						An = tColumn.Shape.a - 4 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * tColumn.Shape.tf;
						if (colSplice.ConnectionOption == ESpliceConnection.FlangeAndWebPlate)
							An = An - 2 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * tColumn.Shape.tw;
					}
					else
					{
						if (tColumn.Shape.bf >= 2 / 3 * tColumn.Shape.d)
							U = 0.9;
						else
							U = 0.85;
						An = tColumn.Shape.a;
					}
					break;
				case ESpliceConnection.ButtPlate:
					U = 1;
					An = 2 * tColumn.Shape.bf * tColumn.Shape.tf;
					break;
				case ESpliceConnection.DirectlyWelded:
					U = 1;
					if (colSplice.ConnectionUpper == EConnectionStyle.Bolted)
						An = tColumn.Shape.a - 2 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * tColumn.Shape.tw;
					else
						An = tColumn.Shape.a;
					break;
			}

			CapacityN = ConstNum.FIOMEGA0_75N * tColumn.Material.Fu * U * An;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + "* Fu * U * An = " + ConstString.FIOMEGA0_75 + " * " + tColumn.Material.Fu + " * " + U + " * " + An);
			if (CapacityN >= colSplice.Tension)
				Reporting.AddLine("= " + CapacityN + " >= " + colSplice.Tension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityN + " << " + colSplice.Tension + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Moment:");
			Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " of Gross Section:");

			capacity = ConstNum.FIOMEGA0_9N * Math.Min(tColumn.Material.Fy * tColumn.Shape.zx, 1.5 * tColumn.Material.Fy * tColumn.Shape.sx) / ConstNum.COEFFICIENT_ONE_THOUSAND;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " *Min(Fy*Z; 1.5*Fy*S)/" + ConstNum.COEFFICIENT_ONE_THOUSAND + "  = " + ConstString.FIOMEGA0_9 + " *Min(" + tColumn.Material.Fy + " * " + tColumn.Shape.zx + "; 1.5* " + tColumn.Material.Fy + " * " + tColumn.Shape.sx + ")/" + ConstNum.COEFFICIENT_ONE_THOUSAND);
			if (capacity >= colSplice.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND)
				Reporting.AddLine("= " + capacity + " >= " + (colSplice.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (OK)");
			else
				Reporting.AddLine("= " + capacity + " << " + (colSplice.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (NG)");
			if (colSplice.ConnectionOption == ESpliceConnection.FlangeAndWebPlate || colSplice.ConnectionOption == ESpliceConnection.FlangePlate)
			{
				if (colSplice.ConnectionUpper == EConnectionStyle.Bolted)
				{
					Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE_STRENGTH + " of Net Section:");
					Afg = tColumn.Shape.bf * tColumn.Shape.tf;
					Afn = Afg - 2 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * tColumn.Shape.tf;
					Reporting.AddLine("Afg = bf * tf = " + Afg + ConstUnit.Area);
					Reporting.AddLine("Afn = Afg - 2 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tf = " + Afn + ConstUnit.Area);
					Reporting.AddLine(string.Empty);
					if (ConstNum.FIOMEGA0_75N * tColumn.Material.Fu * Afn >= ConstNum.FIOMEGA0_9N * tColumn.Material.Fy * Afg)
					{
						Reporting.AddLine(ConstString.FIOMEGA0_75 + "* Fu * Afn = " + (ConstNum.FIOMEGA0_75N * colSplice.Material.Fu * Afn) + "  >= " + ConstString.FIOMEGA0_9 + "  * Fy * Afg = " + ConstString.FIOMEGA0_9 + " * " + (tColumn.Material.Fy * Afg));
						Reporting.AddLine("No reduction of " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " is required.");
					}
					else
					{
						Afe = 5 / 6.0 * tColumn.Material.Fu / tColumn.Material.Fy * Afn;
						Sef = tColumn.Shape.sx - 2 * (Afg - Afe) * ((tColumn.Shape.d - tColumn.Shape.tf) / 2);
						Zef = tColumn.Shape.zx - 2 * (Afg - Afe) * ((tColumn.Shape.d - tColumn.Shape.tf) / 2);
						capacity = ConstNum.FIOMEGA0_9N * Math.Min(tColumn.Material.Fy * Zef, 1.5 * tColumn.Material.Fy * Sef) / ConstNum.COEFFICIENT_ONE_THOUSAND;

						Reporting.AddLine(ConstString.FIOMEGA0_75 + " * Fu * Afn = " + (ConstNum.FIOMEGA0_75N * colSplice.Material.Fu * Afn) + "  << " + ConstString.FIOMEGA0_9 + "  * Fy * Afg = " + (ConstNum.FIOMEGA0_9N * tColumn.Material.Fy * Afg));
						Reporting.AddLine(string.Empty);
						Reporting.AddLine("Afe = (5 / 6) * (Fu / Fy) * Afn = (5 / 6) * (" + tColumn.Material.Fu + " / " + tColumn.Material.Fy + ") * " + Afn + " = " + Afe + ConstUnit.Area);
						Reporting.AddLine("Se = S - 2 * (Afg - Afe) * ((d - tf) / 2)");
						Reporting.AddLine("= " + tColumn.Shape.sx + " - 2 * (" + Afg + " - " + Afe + ") * ((" + tColumn.Shape.d + " - " + tColumn.Shape.tf + ") / 2) = " + Sef + ConstUnit.SecMod);
						Reporting.AddLine("Ze = Z - 2 * (Afg - Afe) * ((d - tf) / 2)");
						Reporting.AddLine("= " + tColumn.Shape.zx + " - 2 * (" + Afg + " - " + Afe + ") * ((" + tColumn.Shape.d + " - " + tColumn.Shape.tf + ") / 2) = " + Zef + ConstUnit.SecMod);
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Min(Fy * Ze, 1.5 * Fy * Se) / " + ConstNum.COEFFICIENT_ONE_THOUSAND + " = " + ConstString.FIOMEGA0_9 + " *Min(" + tColumn.Material.Fy + " * " + Zef + "; 1.5* " + tColumn.Material.Fy + " * " + Sef + ")/" + ConstNum.COEFFICIENT_ONE_THOUSAND);
						if (capacity >= colSplice.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND)
							Reporting.AddLine("= " + capacity + " >= " + (colSplice.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + (colSplice.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (NG)");
					}
				}
			}

			Reporting.AddHeader("Lower Column Tension and Moment Check");
			Reporting.AddHeader("Tension:");
			Reporting.AddHeader("On Gross Area:");
			capacity = ConstNum.FIOMEGA0_9N * bColumn.Material.Fy * bColumn.Shape.a;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * A = " + ConstString.FIOMEGA0_9 + " * " + bColumn.Material.Fy + " * " + bColumn.Shape.a);
			if (capacity >= colSplice.Tension)
				Reporting.AddLine("= " + capacity + " >= " + colSplice.Tension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + capacity + " << " + colSplice.Tension + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("On Effective Net Area:");
			switch (colSplice.ConnectionOption)
			{
				case ESpliceConnection.FlangeAndWebPlate:
				case ESpliceConnection.FlangePlate:
					if (colSplice.ConnectionLower == EConnectionStyle.Bolted)
					{
						if (bColumn.Shape.bf >= 2 / 3 * bColumn.Shape.d && colSplice.BoltRowsFlangeLower >= 3)
							U = 0.9;
						else if (colSplice.BoltRowsFlangeLower >= 3)
							U = 0.85;
						else
							U = 0.75;
						An = bColumn.Shape.a - 4 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * bColumn.Shape.tf;
						if (colSplice.ConnectionOption == ESpliceConnection.FlangeAndWebPlate)
							An = An - 2 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * bColumn.Shape.tw;
					}
					else
					{
						if (bColumn.Shape.bf >= 2 / 3 * bColumn.Shape.d)
							U = 0.9;
						else
							U = 0.85;
						An = bColumn.Shape.a;
					}
					break;
				case ESpliceConnection.ButtPlate:
					U = 1;
					An = 2 * bColumn.Shape.bf * bColumn.Shape.tf;
					break;
				case ESpliceConnection.DirectlyWelded:
					U = 1;
					if (colSplice.ConnectionLower == EConnectionStyle.Bolted)
						An = bColumn.Shape.a - 2 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * bColumn.Shape.tw;
					else
						An = bColumn.Shape.a;
					break;
			}
			CapacityN = ConstNum.FIOMEGA0_75N * bColumn.Material.Fu * U * An;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Fu * U * An = " + ConstString.FIOMEGA0_75 + " * " + bColumn.Material.Fu + " * " + U + " * " + An);
			if (CapacityN >= colSplice.Tension)
				Reporting.AddLine("= " + CapacityN + " >= " + colSplice.Tension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityN + " << " + colSplice.Tension + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Moment:");
			capacity = ConstNum.FIOMEGA0_9N * Math.Min(bColumn.Material.Fy * bColumn.Shape.zx, 1.5 * bColumn.Material.Fy * bColumn.Shape.sx) / ConstNum.COEFFICIENT_ONE_THOUSAND;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " *Min(Fy * Z; 1.5 * Fy * S)/" + ConstNum.COEFFICIENT_ONE_THOUSAND + " = " + ConstString.FIOMEGA0_9 + " *Min(" + bColumn.Material.Fy + " * " + bColumn.Shape.zx + "; 1.5* " + bColumn.Material.Fy + " * " + bColumn.Shape.sx + ")/" + ConstNum.COEFFICIENT_ONE_THOUSAND);
			if (capacity >= colSplice.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND)
				Reporting.AddLine("= " + capacity + " >= " + (colSplice.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (OK)");
			else
				Reporting.AddLine("= " + capacity + " << " + (colSplice.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (NG)");

			if (colSplice.ConnectionOption == ESpliceConnection.FlangeAndWebPlate || colSplice.ConnectionOption == ESpliceConnection.FlangePlate)
			{
				if (colSplice.ConnectionLower == EConnectionStyle.Bolted)
				{
					Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE_STRENGTH + " of Net Section:");
					Afg = bColumn.Shape.bf * bColumn.Shape.tf;
					Afn = Afg - 2 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * bColumn.Shape.tf;
					Reporting.AddLine("Afg = bf * tf = " + Afg + ConstUnit.Area);
					Reporting.AddLine("Afn = Afg - 2 * (dh + " + ConstNum.SIXTEENTH_INCH + " ) * tf = " + Afn + ConstUnit.Area);
					Reporting.AddLine("");
					if (ConstNum.FIOMEGA0_75N * bColumn.Material.Fu * Afn >= ConstNum.FIOMEGA0_9N * bColumn.Material.Fy * Afg)
					{
						Reporting.AddLine(ConstString.FIOMEGA0_75 + "* Fu * Afn = " + (ConstNum.FIOMEGA0_75N * bColumn.Material.Fu * Afn) + "  >= " + ConstString.FIOMEGA0_9 + "  * Fy * Afg = " + (ConstNum.FIOMEGA0_9N * bColumn.Material.Fy * Afg));
						Reporting.AddLine("No reduction of " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " is required.");
					}
					else
					{
						Afe = 5 / 6.0 * bColumn.Material.Fu / bColumn.Material.Fy * Afn;
						Sef = bColumn.Shape.sx - 2 * (Afg - Afe) * ((bColumn.Shape.d - bColumn.Shape.tf) / 2);
						Zef = bColumn.Shape.zx - 2 * (Afg - Afe) * ((bColumn.Shape.d - bColumn.Shape.tf) / 2);
						capacity = ConstNum.FIOMEGA0_9N * Math.Min(bColumn.Material.Fy * Zef, 1.5 * bColumn.Material.Fy * Sef) / ConstNum.COEFFICIENT_ONE_THOUSAND;
						Reporting.AddLine(ConstString.FIOMEGA0_75 + " * Fu * Afn = " + (ConstNum.FIOMEGA0_75N * colSplice.Material.Fu * Afn) + "  << " + ConstString.FIOMEGA0_9 + "  * Fy * Afg = " + (ConstNum.FIOMEGA0_9N * bColumn.Material.Fy * Afg));

						Reporting.AddLine("Afe = (5 / 6) * (Fu / Fy) * Afn = (5 / 6) * (" + bColumn.Material.Fu + " / " + bColumn.Material.Fy + ") * " + Afn + " = " + Afe + ConstUnit.Area);
						Reporting.AddLine("Se = S - 2 * (Afg - Afe) * ((d - tf) / 2)");
						Reporting.AddLine("= " + bColumn.Shape.sx + " - 2 * (" + Afg + " - " + Afe + ") * ((" + bColumn.Shape.d + " - " + bColumn.Shape.tf + ") / 2) = " + Sef + ConstUnit.SecMod);
						Reporting.AddLine("Ze = Z - 2 * (Afg - Afe) * ((d - tf) / 2)");
						Reporting.AddLine("= " + bColumn.Shape.zx + " - 2 * (" + Afg + " - " + Afe + ") * ((" + bColumn.Shape.d + " - " + bColumn.Shape.tf + ") / 2) = " + Zef + ConstUnit.SecMod);
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Min(Fy * Ze, 1.5 * Fy * Se)/" + ConstNum.COEFFICIENT_ONE_THOUSAND + " = " + ConstString.FIOMEGA0_9 + " *Min(" + bColumn.Material.Fy + " * " + Zef + "; 1.5* " + bColumn.Material.Fy + " * " + Sef + ")/" + ConstNum.COEFFICIENT_ONE_THOUSAND);
						if (capacity >= colSplice.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND)
							Reporting.AddLine("= " + capacity + " >= " + (colSplice.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + (colSplice.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (NG)");
					}
				}
			}
		}

		public static void SpliceMomentOfInertia()
		{
			double Ich = 0;
			double tw = 0;
			double tf = 0;
			double dw = 0;
			double MomInL = 0;
			double MomInU = 0;
			double t = 0;
			double MomIn = 0;
			double B = 0;
			double d = 0;

			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			d = Math.Min(tColumn.Shape.d, bColumn.Shape.d);
			B = Math.Min(tColumn.Shape.bf, bColumn.Shape.bf);
			switch (colSplice.ConnectionOption)
			{
				case ESpliceConnection.FlangePlate:
					d = Math.Max(tColumn.Shape.d, bColumn.Shape.d);
					MomIn = colSplice.SpliceWidthFlange * (Math.Pow(d + 2 * colSplice.SpliceThicknessFlange, 3) - Math.Pow(d, 3)) / 12;
					break;
				case ESpliceConnection.FlangeAndWebPlate:
					d = Math.Max(tColumn.Shape.d, bColumn.Shape.d);
					MomIn = colSplice.SpliceWidthFlange * (Math.Pow(d + 2 * colSplice.SpliceThicknessFlange, 3) - Math.Pow(d, 3)) / 12 + 2 * colSplice.SpliceThicknessWeb * Math.Pow(colSplice.SpliceWidthWeb, 3) / 12;
					break;
				case ESpliceConnection.ButtPlate:
					switch (colSplice.ButtWeldTypeUpper)
					{
						case EWeldType.Fillet:
							t = colSplice.ButtWeldSizeUpper;
							d = tColumn.Shape.d;
							break;
						case EWeldType.PJP:
							t = colSplice.ButtWeldSizeUpper;
							d = tColumn.Shape.d - 2 * colSplice.ButtWeldSizeUpper;
							break;
						case EWeldType.CJP:
							t = tColumn.Shape.tf;
							d = tColumn.Shape.d - 2 * tColumn.Shape.tf;
							break;
					}
					MomInU = tColumn.Shape.bf * (Math.Pow(d + 2 * t, 3) - Math.Pow(d, 3)) / 12;
					switch (colSplice.ButtWeldTypeLower)
					{
						case EWeldType.Fillet:
							t = colSplice.ButtWeldSizeLower;
							d = bColumn.Shape.d;
							break;
						case EWeldType.PJP:
							t = colSplice.ButtWeldSizeLower;
							d = bColumn.Shape.d - 2 * colSplice.ButtWeldSizeLower;
							break;
						case EWeldType.CJP:
							t = bColumn.Shape.tf;
							d = bColumn.Shape.d - 2 * bColumn.Shape.tf;
							break;
					}
					MomInL = bColumn.Shape.bf * (Math.Pow(d + 2 * t, 3) - Math.Pow(d, 3)) / 12;
					MomIn = Math.Min(MomInU, MomInL);
					break;
				case ESpliceConnection.DirectlyWelded:
					switch (colSplice.FlangeWeldType)
					{
						case EWeldType.PJP:
							t = colSplice.FilletWeldSizeFlangeUpper;
							d = Math.Min(tColumn.Shape.d, bColumn.Shape.d) - 2 * colSplice.FilletWeldSizeFlangeUpper;
							break;
						case EWeldType.CJP:
							t = Math.Min(tColumn.Shape.tf, bColumn.Shape.tf);
							d = Math.Min(tColumn.Shape.d - 2 * tColumn.Shape.tf, bColumn.Shape.d - 2 * bColumn.Shape.tf);
							break;
					}
					MomIn = Math.Min(tColumn.Shape.bf, bColumn.Shape.bf) * (Math.Pow(d + 2 * t, 3) - Math.Pow(d, 3)) / 12;
					if (colSplice.ChannelType == ESpliceChannelType.Temporary)
					{
						switch (colSplice.WebWeldType)
						{
							case EWeldType.PJP:
								t = colSplice.FilletWeldSizeWebUpper;
								dw = Math.Min(tColumn.Shape.d - 2 * tColumn.Shape.tf, bColumn.Shape.d - 2 * bColumn.Shape.tf);
								break;
							case EWeldType.CJP:
								t = Math.Min(tColumn.Shape.tw, bColumn.Shape.tw);
								dw = Math.Min(tColumn.Shape.d - 2 * tColumn.Shape.tf, bColumn.Shape.d - 2 * bColumn.Shape.tf);
								break;
						}
						MomIn = (MomIn + t * Math.Pow(dw, 3) / 12);
					}
					else
					{
						d = colSplice.Channel.d;
						tf = colSplice.Channel.tf;
						tw = colSplice.Channel.tw;
						B = colSplice.Channel.bf;
						Ich = (B * Math.Pow(d, 3) - (B - tw) * Math.Pow(d - 2 * tf, 3)) / 12;
						MomIn = MomIn + colSplice.NumberOfChannels * Ich;
					}
					break;
			}
			Reporting.AddHeader("Moment of Inertia:");
			Reporting.AddLine("(Provided for stiffness evaluation if required.)");
			Reporting.AddLine("Top Column = " + (tColumn.Shape.sx * tColumn.Shape.d / 2) + ConstUnit.MomentInertia);
			Reporting.AddLine("Bottom Column = " + (bColumn.Shape.sx * bColumn.Shape.d / 2) + ConstUnit.MomentInertia);
			Reporting.AddLine("Splice = " + MomIn + ConstUnit.MomentInertia);
		}

		public static void ColSpliceMinPLThickness(ref double CSMinX, ref double CSMinY, ref double PlWidth, ref double PLLength, ref double PLType, ref double Gage, ref double Minth)
		{
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			if (colSplice.ConnectionLower == EConnectionStyle.Bolted && colSplice.ConnectionUpper == EConnectionStyle.Bolted)
			{
				if (tColumn.Shape.d >= 19 * ConstNum.ONE_INCH && tColumn.Shape.a >= 134 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 19 * ConstNum.ONE_INCH && bColumn.Shape.a >= 134 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.75 * ConstNum.ONE_INCH; // >=W14X455
					Gage = 13.5 * ConstNum.ONE_INCH;
					PLType = 1 * ConstNum.ONE_INCH;
					PlWidth = 16 * ConstNum.ONE_INCH;
					PLLength = 18.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 16.4 * ConstNum.ONE_INCH && tColumn.Shape.a >= 75.6 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 16.4 * ConstNum.ONE_INCH && bColumn.Shape.a >= 75.6 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.625 * ConstNum.ONE_INCH; // >=W14X257
					Gage = 11.5 * ConstNum.ONE_INCH;
					PLType = 1 * ConstNum.ONE_INCH;
					PlWidth = 16 * ConstNum.ONE_INCH;
					PLLength = 18.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 14.8 * ConstNum.ONE_INCH && tColumn.Shape.a >= 42.7 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 14.8 * ConstNum.ONE_INCH && bColumn.Shape.a >= 42.7 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.5 * ConstNum.ONE_INCH; // >=W14X145
					Gage = 13.5 * ConstNum.ONE_INCH;
					PLType = 1 * ConstNum.ONE_INCH;
					PlWidth = 16 * ConstNum.ONE_INCH;
					PLLength = 18.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 14 * ConstNum.ONE_INCH && tColumn.Shape.a >= 26.5 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 14 * ConstNum.ONE_INCH && bColumn.Shape.a >= 26.5 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W14X90
					Gage = 13.5 * ConstNum.ONE_INCH;
					PLType = 1 * ConstNum.ONE_INCH;
					PlWidth = 16 * ConstNum.ONE_INCH;
					PLLength = 18.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 13.7 * ConstNum.ONE_INCH && tColumn.Shape.a >= 12.6 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.7 * ConstNum.ONE_INCH && bColumn.Shape.a >= 12.6 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W14X43
					Gage = 13.5 * ConstNum.ONE_INCH;
					PLType = 1 * ConstNum.ONE_INCH;
					PlWidth = 16 * ConstNum.ONE_INCH;
					PLLength = 18.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 13.1 * ConstNum.ONE_INCH && tColumn.Shape.a >= 35.3 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.1 * ConstNum.ONE_INCH && bColumn.Shape.a >= 35.3 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.625 * ConstNum.ONE_INCH; // >=W12X120
					Gage = 13.5 * ConstNum.ONE_INCH;
					PLType = 1 * ConstNum.ONE_INCH;
					PlWidth = 16 * ConstNum.ONE_INCH;
					PLLength = 18.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 11.9 * ConstNum.ONE_INCH && tColumn.Shape.a >= 11.7 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 11.9 * ConstNum.ONE_INCH && bColumn.Shape.a >= 11.7 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W12X40
					Gage = 13.5 * ConstNum.ONE_INCH;
					PLType = 1 * ConstNum.ONE_INCH;
					PlWidth = 16 * ConstNum.ONE_INCH;
					PLLength = 18.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 9.73 * ConstNum.ONE_INCH && tColumn.Shape.a >= 9.71 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 9.73 * ConstNum.ONE_INCH && bColumn.Shape.a >= 9.71 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W10X33
					Gage = 13.5 * ConstNum.ONE_INCH;
					PLType = 1 * ConstNum.ONE_INCH;
					PlWidth = 16 * ConstNum.ONE_INCH;
					PLLength = 18.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 8 * ConstNum.ONE_INCH && tColumn.Shape.a >= 9.12 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 8 * ConstNum.ONE_INCH && bColumn.Shape.a >= 9.12 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W8X31
					Gage = 13.5 * ConstNum.ONE_INCH;
					PLType = 1 * ConstNum.ONE_INCH;
					PlWidth = 16 * ConstNum.ONE_INCH;
					PLLength = 18.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 7.93 * ConstNum.ONE_INCH && tColumn.Shape.a >= 7.08 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 7.93 * ConstNum.ONE_INCH && bColumn.Shape.a >= 7.08 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W8X24
					Gage = 13.5 * ConstNum.ONE_INCH;
					PLType = 1 * ConstNum.ONE_INCH;
					PlWidth = 16 * ConstNum.ONE_INCH;
					PLLength = 18.5 * ConstNum.ONE_INCH;
				}
				else
				{
					Minth = 0.375 * ConstNum.ONE_INCH; //  All Others
					Gage = 13.5 * ConstNum.ONE_INCH;
					PLType = 1 * ConstNum.ONE_INCH;
					PlWidth = 16 * ConstNum.ONE_INCH;
					PLLength = 18.5 * ConstNum.ONE_INCH;
				}
			}
			if (colSplice.ConnectionLower == EConnectionStyle.Welded && colSplice.ConnectionUpper == EConnectionStyle.Welded)
			{
				if (tColumn.Shape.d >= 19 * ConstNum.ONE_INCH && tColumn.Shape.a >= 134 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 19 * ConstNum.ONE_INCH && bColumn.Shape.a >= 134 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.625 * ConstNum.ONE_INCH; // >=W14X455
					CSMinX = 5 * ConstNum.ONE_INCH;
					CSMinY = 7 * ConstNum.ONE_INCH;
					PlWidth = 14 * ConstNum.ONE_INCH;
					PLLength = 18 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 17.1 * ConstNum.ONE_INCH && tColumn.Shape.a >= 91.4 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 17.1 * ConstNum.ONE_INCH && bColumn.Shape.a >= 91.4 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.625 * ConstNum.ONE_INCH; // >=W14X311
					CSMinX = 4 * ConstNum.ONE_INCH;
					CSMinY = 6 * ConstNum.ONE_INCH;
					PlWidth = 12 * ConstNum.ONE_INCH;
					PLLength = 16 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 15.7 * ConstNum.ONE_INCH && tColumn.Shape.a >= 62 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 15.7 * ConstNum.ONE_INCH && bColumn.Shape.a >= 62 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.5 * ConstNum.ONE_INCH; // >=W14X211
					CSMinX = 4 * ConstNum.ONE_INCH;
					CSMinY = 6 * ConstNum.ONE_INCH;
					PlWidth = 12 * ConstNum.ONE_INCH;
					PLLength = 16 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 14 * ConstNum.ONE_INCH && tColumn.Shape.a >= 26.5 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 14 * ConstNum.ONE_INCH && bColumn.Shape.a >= 26.5 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W14X90
					CSMinX = 4 * ConstNum.ONE_INCH;
					CSMinY = 6 * ConstNum.ONE_INCH;
					PlWidth = 12 * ConstNum.ONE_INCH;
					PLLength = 16 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 13.9 * ConstNum.ONE_INCH && tColumn.Shape.a >= 17.9 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.9 * ConstNum.ONE_INCH && bColumn.Shape.a >= 17.9 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W14X61
					CSMinX = 3 * ConstNum.ONE_INCH;
					CSMinY = 6 * ConstNum.ONE_INCH;
					PlWidth = 8 * ConstNum.ONE_INCH;
					PLLength = 16 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 13.7 * ConstNum.ONE_INCH && tColumn.Shape.a >= 12.6 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.7 * ConstNum.ONE_INCH && bColumn.Shape.a >= 12.6 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.3125 * ConstNum.ONE_INCH; // >=W14X43
					CSMinX = 2 * ConstNum.ONE_INCH;
					CSMinY = 5 * ConstNum.ONE_INCH;
					PlWidth = 6 * ConstNum.ONE_INCH;
					PLLength = 14 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 13.1 * ConstNum.ONE_INCH && tColumn.Shape.a >= 35.3 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.1 * ConstNum.ONE_INCH && bColumn.Shape.a >= 35.3 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.5 * ConstNum.ONE_INCH; // >=W12X120
					CSMinX = 3 * ConstNum.ONE_INCH;
					CSMinY = 6 * ConstNum.ONE_INCH;
					PlWidth = 8 * ConstNum.ONE_INCH;
					PLLength = 16 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 12.1 * ConstNum.ONE_INCH && tColumn.Shape.a >= 15.6 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 12.1 * ConstNum.ONE_INCH && bColumn.Shape.a >= 15.6 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W12X53
					CSMinX = 3 * ConstNum.ONE_INCH;
					CSMinY = 6 * ConstNum.ONE_INCH;
					PlWidth = 8 * ConstNum.ONE_INCH;
					PLLength = 16 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 11.9 * ConstNum.ONE_INCH && tColumn.Shape.a >= 11.7 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 11.9 * ConstNum.ONE_INCH && bColumn.Shape.a >= 11.7 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.3125 * ConstNum.ONE_INCH; // >=W12X40
					CSMinX = 2 * ConstNum.ONE_INCH;
					CSMinY = 5 * ConstNum.ONE_INCH;
					PlWidth = 6 * ConstNum.ONE_INCH;
					PLLength = 14 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 9.98 * ConstNum.ONE_INCH && tColumn.Shape.a >= 14.4 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 9.98 * ConstNum.ONE_INCH && bColumn.Shape.a >= 14.4 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W10X49
					CSMinX = 3 * ConstNum.ONE_INCH;
					CSMinY = 6 * ConstNum.ONE_INCH;
					PlWidth = 8 * ConstNum.ONE_INCH;
					PLLength = 16 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 9.73 * ConstNum.ONE_INCH && tColumn.Shape.a >= 9.71 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 9.73 * ConstNum.ONE_INCH && bColumn.Shape.a >= 9.71 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.3125 * ConstNum.ONE_INCH; // >=W10X33
					CSMinX = 2 * ConstNum.ONE_INCH;
					CSMinY = 5 * ConstNum.ONE_INCH;
					PlWidth = 6 * ConstNum.ONE_INCH;
					PLLength = 14 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 8 * ConstNum.ONE_INCH && tColumn.Shape.a >= 9.12 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 8 * ConstNum.ONE_INCH && bColumn.Shape.a >= 9.12 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W8X31
					CSMinX = 2 * ConstNum.ONE_INCH;
					CSMinY = 5 * ConstNum.ONE_INCH;
					PlWidth = 6 * ConstNum.ONE_INCH;
					PLLength = 14 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 7.93 * ConstNum.ONE_INCH && tColumn.Shape.a >= 7.08 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 7.93 * ConstNum.ONE_INCH && bColumn.Shape.a >= 7.08 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.3125 * ConstNum.ONE_INCH; // >=W8X24
					CSMinX = 2 * ConstNum.ONE_INCH;
					CSMinY = 4 * ConstNum.ONE_INCH;
					PlWidth = 5 * ConstNum.ONE_INCH;
					PLLength = 12 * ConstNum.ONE_INCH;
				}
				else
				{
					Minth = 0.3125 * ConstNum.ONE_INCH; //  All Others
					CSMinX = 2 * ConstNum.ONE_INCH;
					CSMinY = 4 * ConstNum.ONE_INCH;
					PlWidth = 5 * ConstNum.ONE_INCH;
					PLLength = 12 * ConstNum.ONE_INCH;
				}
			}
			if (colSplice.ConnectionLower == EConnectionStyle.Bolted || colSplice.ConnectionUpper == EConnectionStyle.Bolted)
			{
				if (tColumn.Shape.d >= 19 * ConstNum.ONE_INCH && tColumn.Shape.a >= 134 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 19 * ConstNum.ONE_INCH && bColumn.Shape.a >= 134 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.625 * ConstNum.ONE_INCH; // >=W14X455
					CSMinX = 5 * ConstNum.ONE_INCH;
					CSMinY = 7 * ConstNum.ONE_INCH;
					PlWidth = 14 * ConstNum.ONE_INCH;
					PLLength = 18 * ConstNum.ONE_INCH;
					Gage = 11.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 17.1 * ConstNum.ONE_INCH && tColumn.Shape.a >= 91.4 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 17.1 * ConstNum.ONE_INCH && bColumn.Shape.a >= 91.4 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.625 * ConstNum.ONE_INCH; // >=W14X311
					CSMinX = 4 * ConstNum.ONE_INCH;
					CSMinY = 6 * ConstNum.ONE_INCH;
					PlWidth = 12 * ConstNum.ONE_INCH;
					PLLength = 17 * ConstNum.ONE_INCH;
					Gage = 9.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 15.7 * ConstNum.ONE_INCH && tColumn.Shape.a >= 62 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 15.7 * ConstNum.ONE_INCH && bColumn.Shape.a >= 62 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.5 * ConstNum.ONE_INCH; // >=W14X211
					CSMinX = 4 * ConstNum.ONE_INCH;
					CSMinY = 6 * ConstNum.ONE_INCH;
					PlWidth = 12 * ConstNum.ONE_INCH;
					PLLength = 17 * ConstNum.ONE_INCH;
					Gage = 9.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 14 * ConstNum.ONE_INCH && tColumn.Shape.a >= 26.5 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 14 * ConstNum.ONE_INCH && bColumn.Shape.a >= 26.5 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W14X90
					CSMinX = 4 * ConstNum.ONE_INCH;
					CSMinY = 6 * ConstNum.ONE_INCH;
					PlWidth = 12 * ConstNum.ONE_INCH;
					PLLength = 14 * ConstNum.ONE_INCH;
					Gage = 9.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 13.9 * ConstNum.ONE_INCH && tColumn.Shape.a >= 17.9 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.9 * ConstNum.ONE_INCH && bColumn.Shape.a >= 17.9 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W14X61
					CSMinX = 3 * ConstNum.ONE_INCH;
					CSMinY = 6 * ConstNum.ONE_INCH;
					PlWidth = 8 * ConstNum.ONE_INCH;
					PLLength = 14 * ConstNum.ONE_INCH;
					Gage = 5.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 13.7 * ConstNum.ONE_INCH && tColumn.Shape.a >= 12.6 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.7 * ConstNum.ONE_INCH && bColumn.Shape.a >= 12.6 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.3125 * ConstNum.ONE_INCH; // >=W14X43
					CSMinX = 2 * ConstNum.ONE_INCH;
					CSMinY = 5 * ConstNum.ONE_INCH;
					PlWidth = 6 * ConstNum.ONE_INCH;
					PLLength = 13 * ConstNum.ONE_INCH;
					Gage = 3.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 13.1 * ConstNum.ONE_INCH && tColumn.Shape.a >= 35.3 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.1 * ConstNum.ONE_INCH && bColumn.Shape.a >= 35.3 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.5 * ConstNum.ONE_INCH; // >=W12X120
					CSMinX = 3 * ConstNum.ONE_INCH;
					CSMinY = 6 * ConstNum.ONE_INCH;
					PlWidth = 8 * ConstNum.ONE_INCH;
					PLLength = 14 * ConstNum.ONE_INCH;
					Gage = 5.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 12.1 * ConstNum.ONE_INCH && tColumn.Shape.a >= 15.6 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 12.1 * ConstNum.ONE_INCH && bColumn.Shape.a >= 15.6 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W12X53
					CSMinX = 3 * ConstNum.ONE_INCH;
					CSMinY = 6 * ConstNum.ONE_INCH;
					PlWidth = 8 * ConstNum.ONE_INCH;
					PLLength = 14 * ConstNum.ONE_INCH;
					Gage = 5.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 11.9 * ConstNum.ONE_INCH && tColumn.Shape.a >= 11.7 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 11.9 * ConstNum.ONE_INCH && bColumn.Shape.a >= 11.7 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.3125 * ConstNum.ONE_INCH; // >=W12X40
					CSMinX = 2 * ConstNum.ONE_INCH;
					CSMinY = 5 * ConstNum.ONE_INCH;
					PlWidth = 6 * ConstNum.ONE_INCH;
					PLLength = 13 * ConstNum.ONE_INCH;
					Gage = 3.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 9.98 * ConstNum.ONE_INCH && tColumn.Shape.a >= 14.4 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 9.98 * ConstNum.ONE_INCH && bColumn.Shape.a >= 14.4 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W10X49
					CSMinX = 3 * ConstNum.ONE_INCH;
					CSMinY = 6 * ConstNum.ONE_INCH;
					PlWidth = 8 * ConstNum.ONE_INCH;
					PLLength = 14 * ConstNum.ONE_INCH;
					Gage = 5.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 9.73 * ConstNum.ONE_INCH && tColumn.Shape.a >= 9.71 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 9.73 * ConstNum.ONE_INCH && bColumn.Shape.a >= 9.71 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.3125 * ConstNum.ONE_INCH; // >=W10X33
					CSMinX = 2 * ConstNum.ONE_INCH;
					CSMinY = 5 * ConstNum.ONE_INCH;
					PlWidth = 6 * ConstNum.ONE_INCH;
					PLLength = 13 * ConstNum.ONE_INCH;
					Gage = 3.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 8 * ConstNum.ONE_INCH && tColumn.Shape.a >= 9.12 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 8 * ConstNum.ONE_INCH && bColumn.Shape.a >= 9.12 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.375 * ConstNum.ONE_INCH; // >=W8X31
					CSMinX = 2 * ConstNum.ONE_INCH;
					CSMinY = 5 * ConstNum.ONE_INCH;
					PlWidth = 6 * ConstNum.ONE_INCH;
					PLLength = 13 * ConstNum.ONE_INCH;
					Gage = 3.5 * ConstNum.ONE_INCH;
				}
				else if (tColumn.Shape.d >= 7.93 * ConstNum.ONE_INCH && tColumn.Shape.a >= 7.08 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 7.93 * ConstNum.ONE_INCH && bColumn.Shape.a >= 7.08 * Math.Pow(ConstNum.ONE_INCH, 2))
				{
					Minth = 0.3125 * ConstNum.ONE_INCH; // >=W8X24
					CSMinX = 2 * ConstNum.ONE_INCH;
					CSMinY = 4 * ConstNum.ONE_INCH;
					PlWidth = 5 * ConstNum.ONE_INCH;
					PLLength = 12 * ConstNum.ONE_INCH;
					Gage = 3.5 * ConstNum.ONE_INCH;
				}
				else
				{
					Minth = 0.3125 * ConstNum.ONE_INCH; //  All Others
					CSMinX = 2 * ConstNum.ONE_INCH;
					CSMinY = 4 * ConstNum.ONE_INCH;
					PlWidth = 5 * ConstNum.ONE_INCH;
					PLLength = 12 * ConstNum.ONE_INCH;
					Gage = 3.5 * ConstNum.ONE_INCH;
				}
			}

			Minth = CommonCalculations.PlateThickness(Minth);
		}

		public static void ButtPlateCapacity()
		{
			double maxweldsize = 0;
			double minweldsize = 0;
			double weldForceU = 0;
			double weldForceL = 0;
			double weldForceLc = 0;
			double weldForceUc = 0;
			string seism = "";
			double weldForceLt = 0;
			double weldForceUt = 0;
			double CompflangeforceL = 0;
			double CompflangeforceU = 0;
			double FlangeforceL = 0;
			double FlangeforceU = 0;
			double Ff_tL = 0;
			double Ff_tU = 0;
			double Mcapacity = 0;
			double M = 0;
			double Ff_cL = 0;
			double Ff_cU = 0;
			double capacity = 0;
			double V = 0;
			double nonContactArea = 0;
			double FbrBot = 0;
			double FbrTop = 0;
			double d = 0;
			double BF = 0;

			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Butt Plate:");
			if (tColumn.Shape.d >= 7.89 * ConstNum.ONE_INCH && tColumn.Shape.a >= 2.96 * Math.Pow(ConstNum.ONE_INCH, 2) && (tColumn.Shape.d <= 9 * ConstNum.ONE_INCH && tColumn.Shape.a <= 19.7 * Math.Pow(ConstNum.ONE_INCH, 2)) && (bColumn.Shape.d >= 9.87 * ConstNum.ONE_INCH && bColumn.Shape.a >= 3.54 * Math.Pow(ConstNum.ONE_INCH, 2)) && (bColumn.Shape.d <= 11.4 * ConstNum.ONE_INCH && bColumn.Shape.a <= 32.9 * Math.Pow(ConstNum.ONE_INCH, 2)) || bColumn.Shape.d >= 7.89 * ConstNum.ONE_INCH && bColumn.Shape.a >= 2.96 * Math.Pow(ConstNum.ONE_INCH, 2) && (bColumn.Shape.d <= 9 * ConstNum.ONE_INCH && bColumn.Shape.a <= 19.7 * Math.Pow(ConstNum.ONE_INCH, 2)) && (tColumn.Shape.d >= 9.87 * ConstNum.ONE_INCH && tColumn.Shape.a >= 3.54 * Math.Pow(ConstNum.ONE_INCH, 2)) && (tColumn.Shape.d <= 11.4 * ConstNum.ONE_INCH && tColumn.Shape.a <= 32.9 * Math.Pow(ConstNum.ONE_INCH, 2)))
			{
				if (colSplice.ButtThickness >= ConstNum.ONEANDHALF_INCHES)
					Reporting.AddLine("Thickness = " + colSplice.ButtThickness + " >= " + ConstNum.ONEANDHALF_INCHES + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Thickness = " + colSplice.ButtThickness + " <<  " + ConstNum.ONEANDHALF_INCHES + ConstUnit.Length + " (NG)");
			}
			else
			{
				if (colSplice.ButtThickness >= ConstNum.TWO_INCHES)
					Reporting.AddLine("Thickness = " + colSplice.ButtThickness + " >= " + ConstNum.TWO_INCHES + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Thickness = " + colSplice.ButtThickness + " << " + ConstNum.TWO_INCHES + ConstUnit.Length + " (NG)");
			}
			BF = Math.Max(bColumn.Shape.bf, tColumn.Shape.bf);
			d = Math.Max(bColumn.Shape.d, tColumn.Shape.d) + ConstNum.THREE_INCHES;
			if (colSplice.ButtWidth >= BF)
				Reporting.AddLine("Width = " + colSplice.ButtWidth + " >= Max(bf_upper, bf_lower) = " + BF + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Width = " + colSplice.ButtWidth + " << Max(bf_upper, bf_lower) = " + BF + ConstUnit.Length + " (NG)");

			if (colSplice.ButtLength >= d)
				Reporting.AddLine("Length = " + colSplice.ButtLength + " >= Max(d_upper, d_lower) + 3 = " + d + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Length = " + colSplice.ButtLength + " << Max(d_upper, d_lower) + 3 = " + d + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Upper Column Bearing, FbrTop:");
			FbrTop = ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(tColumn.Material.Fy, colSplice.Material.Fy) * tColumn.Shape.a;
			Reporting.AddLine("FbrTop = " + ConstString.FIOMEGA0_75 + " * 1.8 * Min(Fy_uc; Fy_pl) * A_uc");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 1.8 * Min(" + tColumn.Material.Fy + "; " + colSplice.Material.Fy + ") * " + tColumn.Shape.a);
			Reporting.AddLine("= " + FbrTop + ConstUnit.Force);

			Reporting.AddHeader("Lower Column Bearing, FbrBot:");
			FbrBot = ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(bColumn.Material.Fy, colSplice.Material.Fy) * bColumn.Shape.a;
			Reporting.AddLine("FbrBot = " + ConstString.FIOMEGA0_75 + " * 1.8 * Min(Fy_lc; Fy_pl) * A_lc");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 1.8 * Min(" + bColumn.Material.Fy + "; " + colSplice.Material.Fy + ") * " + bColumn.Shape.a);
			Reporting.AddLine("= " + FbrBot + ConstUnit.Force);

			Reporting.AddHeader("Flange Forces:");

			if (colSplice.Moment == 0 && colSplice.Compression > 0)
			{
				Reporting.AddHeader("Axial Compression:");

				if (colSplice.Compression <= Math.Min(FbrTop, FbrBot))
					Reporting.AddLine("C = " + colSplice.Compression + " <= Min(FbrTop, FbrBot) = " + Math.Min(FbrTop, FbrBot) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("C = " + colSplice.Compression + " >> Min(FbrTop, FbrBot) = " + Math.Min(FbrTop, FbrBot) + ConstUnit.Force + " (NG)");

				nonContactArea = Math.Min(tColumn.Shape.a, bColumn.Shape.a);
				V = colSplice.Compression / Math.Min(tColumn.Shape.a, bColumn.Shape.a) * nonContactArea;
				capacity = (colSplice.ButtThickness * (ConstNum.FIOMEGA0_9N * 0.6 * colSplice.Material.Fy * (colSplice.ButtWidth - Math.Max(tColumn.Shape.tw, bColumn.Shape.tw)) * 2));
				if (nonContactArea > 0)
				{
					Reporting.AddHeader("Force Transfered Through Plate Shear:");
					Reporting.AddLine("(Assume this is proportional to the partial");
					Reporting.AddLine("area of the smaller column not directly");
					Reporting.AddLine("resting opposite the bigger column.)");

					Reporting.AddHeader("V = C / Min(A_uc, A_lc) * (Min(A_uc, A_lc) - Ac)");
					Reporting.AddLine("= " + colSplice.Compression + " /  " + Math.Min(tColumn.Shape.a, bColumn.Shape.a) + " *  (" + Math.Min(tColumn.Shape.a, bColumn.Shape.a) + " - " + 0 + ")");
					Reporting.AddLine("= " + V + ConstUnit.Force);

					Reporting.AddHeader("Plate Shear Capacity = 2 * tpl * (" + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * (Wpl - Max(tw_uc, tw_lc) ))");
					Reporting.AddLine("= 2 * " + colSplice.ButtThickness + " * (" + ConstString.FIOMEGA0_9 + " * 0.6 * " + colSplice.Material.Fy + " * (" + colSplice.ButtWidth + " - " + Math.Max(tColumn.Shape.tw, bColumn.Shape.tw) + "))");
					if (capacity >= V)
						Reporting.AddLine("= " + capacity + " >= " + V + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + capacity + " << " + V + ConstUnit.Force + " (NG)");
				}

				Ff_cU = colSplice.Compression / tColumn.Shape.a * (tColumn.Shape.tf * tColumn.Shape.bf);
				Ff_cL = colSplice.Compression / bColumn.Shape.a * (bColumn.Shape.tf * bColumn.Shape.bf);
				Reporting.AddHeader("Flange Compressive Forces:");
				Reporting.AddLine("Upper Column, Ff_cu = (P / A) * tf * bf = " + Ff_cU + ConstUnit.Force);
				Reporting.AddLine("Lower Column, Ff_cl = (P / A) * tf * bf = " + Ff_cL + ConstUnit.Force);

			}
			if (colSplice.Moment == 0 && colSplice.Tension > 0)
			{
				Reporting.AddHeader("Axial Tension:");
				Reporting.AddHeader("Plate Shear:");
				V = colSplice.Tension / 2;
				Reporting.AddLine("V = T/2 = " + V + ConstUnit.Force);
				capacity = colSplice.ButtThickness * ConstNum.FIOMEGA0_9N * 0.6 * colSplice.Material.Fy * colSplice.ButtWidth;
				Reporting.AddLine(ConstString.PHI + " Rn = tpl * " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * Wpl");
				Reporting.AddLine("= " + colSplice.ButtThickness + " * " + ConstString.FIOMEGA0_9 + " * 0.6 * " + colSplice.Material.Fy + " * " + colSplice.ButtWidth);
				if (capacity >= V)
					Reporting.AddLine("= " + capacity + " >= " + V + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + V + ConstUnit.Force + " (NG)");

				Reporting.AddHeader("Plate Bending:");
				M = V * Math.Abs(bColumn.Shape.d - tColumn.Shape.d) / 2;
				Mcapacity = ConstNum.FIOMEGA0_75N * colSplice.Material.Fy * Math.Pow(colSplice.ButtThickness, 2) * colSplice.ButtWidth / 6;

				Reporting.AddHeader("Moment = V * |d_uc - d_lc| / 2 = " + V + " * " + Math.Abs(bColumn.Shape.d - tColumn.Shape.d) + " / 2= " + M + ConstUnit.Moment);
				Reporting.AddLine("Mom. Capacity = " + ConstString.FIOMEGA0_75 + "* Fy * t² * Wpl / 6");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + colSplice.Material.Fy + " * " + colSplice.ButtThickness + "² * " + colSplice.ButtWidth + " / 6");
				if (Mcapacity >= M)
					Reporting.AddLine("= " + Mcapacity + " >= " + M + ConstUnit.Moment + " (OK)");
				else
					Reporting.AddLine("= " + Mcapacity + " << " + M + ConstUnit.Moment + " (NG)");

				Ff_tU = colSplice.Tension / 2;
				Ff_tL = colSplice.Tension / 2;
				Reporting.AddHeader("Flange Tensile Forces:");
				Reporting.AddLine("Upper Column, Ff_tu = T/2 = " + Ff_tU + ConstUnit.Force);
				Reporting.AddLine("Lower Column, Ff_tl = T/2 = " + Ff_tL + ConstUnit.Force);

			}

			if (colSplice.Moment > 0 && colSplice.Tension >= 0)
			{
				Reporting.AddHeader("Combined Moment & Tension");
				if (colSplice.Tension > 0)
				{
					FlangeforceU = colSplice.Tension / 2 + colSplice.Moment / tColumn.Shape.d;
					FlangeforceL = colSplice.Tension / 2 + colSplice.Moment / bColumn.Shape.d;
					Ff_tU = Math.Max(Ff_tU, FlangeforceU);
					Ff_tL = Math.Max(Ff_tL, FlangeforceL);
					Reporting.AddHeader("Upper Col. Flange Force, Ff_tu = T / 2 +  Ms / d_uc");
					Reporting.AddLine("= " + colSplice.Tension + " / 2 + " + colSplice.Moment + " / " + tColumn.Shape.d + " = " + FlangeforceU + ConstUnit.Force);
					Reporting.AddHeader("Lower Col. Flange Force, Ff_tl = T / 2 + Ms / d_lc");
					Reporting.AddLine("= " + colSplice.Tension + " / 2 + " + colSplice.Moment + " / " + bColumn.Shape.d + " = " + FlangeforceL + ConstUnit.Force);
				}
				else
				{
					FlangeforceU = -colSplice.Cmin / 2 + colSplice.Moment / tColumn.Shape.d;
					FlangeforceL = -colSplice.Cmin / 2 + colSplice.Moment / bColumn.Shape.d;
					Ff_tU = Math.Max(Ff_tU, FlangeforceU);
					Ff_tL = Math.Max(Ff_tL, FlangeforceL);
					Reporting.AddHeader("Upper Col. Flange Force, Ff_tu = -Cmin / 2 +  Ms / d_uc");
					Reporting.AddLine("= " + (-colSplice.Cmin) + " / 2 + " + colSplice.Moment + " / " + tColumn.Shape.d + " = " + FlangeforceU + ConstUnit.Force);
					Reporting.AddHeader("Lower Col. Flange Force, Ff_tl = -Cmin / 2 + Ms / d_lc");
					Reporting.AddLine("= " + (-colSplice.Cmin) + " / 2 + " + colSplice.Moment + " / " + bColumn.Shape.d + " = " + FlangeforceL + ConstUnit.Force);
				}
				Reporting.AddHeader("Plate Shear:");
				V = Math.Min(FlangeforceU, FlangeforceL);
				Reporting.AddLine("V = Min(Ffu, Ffl) = " + V + ConstUnit.Force);
				capacity = colSplice.ButtThickness * ConstNum.FIOMEGA0_9N * 0.6 * colSplice.Material.Fy * colSplice.ButtWidth;
				Reporting.AddLine(ConstString.PHI + " Rn = tpl * " + ConstString.FIOMEGA0_9 + " *0.6 * Fy * Wpl");
				Reporting.AddLine("= " + colSplice.ButtThickness + " * " + ConstString.FIOMEGA0_9 + " *0.6 * " + colSplice.Material.Fy + " * " + colSplice.ButtWidth);
				if (capacity >= V)
					Reporting.AddLine("= " + capacity + " >= " + V + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + V + ConstUnit.Force + " (NG)");

				Reporting.AddHeader("Plate Bending:");
				M = V * Math.Abs(bColumn.Shape.d - tColumn.Shape.d) / 2;
				Mcapacity = ConstNum.FIOMEGA0_9N * colSplice.Material.Fy * Math.Pow(colSplice.ButtThickness, 2) * colSplice.ButtWidth / 4;

				Reporting.AddHeader("Moment = V * |d_uc - d_lc| / 2 = " + V + " * " + Math.Abs(bColumn.Shape.d - tColumn.Shape.d) + " / 2= " + M + ConstUnit.Moment);
				Reporting.AddLine("Mom. Capacity = " + ConstString.FIOMEGA0_9 + "* Fy * t² * Wpl / 4");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + colSplice.Material.Fy + " * " + colSplice.ButtThickness + "² * " + colSplice.ButtWidth + " / 4");
				if (Mcapacity >= M)
					Reporting.AddLine("= " + Mcapacity + " >= " + M + ConstUnit.Moment + " (OK)");
				else
					Reporting.AddLine("= " + Mcapacity + " << " + M + ConstUnit.Moment + " (NG)");

				CompflangeforceU = Math.Max(colSplice.Moment / tColumn.Shape.d - colSplice.Tension / 2, 0);
				CompflangeforceL = Math.Max(colSplice.Moment / bColumn.Shape.d - colSplice.Tension / 2, 0);
				Ff_cU = Math.Max(Ff_cU, CompflangeforceU);
				Ff_cL = Math.Max(Ff_cL, CompflangeforceL);
				Reporting.AddHeader("Compression Side:");
				Reporting.AddLine("Upper Col. Flange Force, Ff_cu = Max[( Ms / d_uc - T / 2); 0]");
				Reporting.AddLine("= Max[(" + colSplice.Moment + " / " + tColumn.Shape.d + " - " + colSplice.Tension + " / 2); 0] = " + CompflangeforceU + ConstUnit.Force);
				Reporting.AddLine("Lower Col. Flange Force, Ff_cl = Max[( Ms / d_lc - T / 2); 0]");
				Reporting.AddLine("= Max[(" + colSplice.Moment + " / " + bColumn.Shape.d + " - " + colSplice.Tension + " / 2); 0] = " + CompflangeforceL + ConstUnit.Force);

				if (CompflangeforceU > 0)
				{
					Reporting.AddHeader("Upper Col. Flange End Bearing:");
					capacity = ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(tColumn.Material.Fy, colSplice.Material.Fy) * (tColumn.Shape.tf * tColumn.Shape.bf);
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + "*1.8 * Min(Fy_uc; Fy_pl) * (tf_uc * bf_uc)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + "*1.8 * Min(" + tColumn.Material.Fy + "; " + colSplice.Material.Fy + ") * (" + tColumn.Shape.tf + " * " + tColumn.Shape.bf + ")");
					if (capacity >= CompflangeforceU)
						Reporting.AddLine("= " + capacity + " >= " + CompflangeforceU + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + capacity + " << " + CompflangeforceU + ConstUnit.Force + " (NG)");
				}
				if (CompflangeforceL > 0)
				{
					Reporting.AddHeader("Lower Col. Flange End Bearing:");
					capacity = ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(bColumn.Material.Fy, colSplice.Material.Fy) * (bColumn.Shape.tf * bColumn.Shape.bf);
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 1.8 * Min(Fy_lc; Fy_pl) * (tf_lc * bf_lc)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 1.8 * Min(" + bColumn.Material.Fy + "; " + colSplice.Material.Fy + ") * (" + bColumn.Shape.tf + " * " + bColumn.Shape.bf + ")");
					if (capacity >= CompflangeforceL)
						Reporting.AddLine("= " + capacity + " >= " + CompflangeforceL + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + capacity + " << " + CompflangeforceL + ConstUnit.Force + " (NG)");
				}
			}

			if (colSplice.Moment > 0 && colSplice.Compression >= 0)
			{
				FlangeforceU = colSplice.Compression / tColumn.Shape.a * (tColumn.Shape.tf * tColumn.Shape.bf) + colSplice.Moment / tColumn.Shape.d;
				FlangeforceL = colSplice.Compression / bColumn.Shape.a * (bColumn.Shape.tf * bColumn.Shape.bf) + colSplice.Moment / bColumn.Shape.d;
				Ff_cU = Math.Max(Ff_cU, FlangeforceU);
				Ff_cL = Math.Max(Ff_cL, FlangeforceL);

				Reporting.AddHeader("Combined Moment & Compression");
				Reporting.AddHeader("Upper Col. Flange Comp. Force, Ff_cu = C / A_uc * (tf_uc * bf_uc) +  Ms / d_uc");
				Reporting.AddLine("= " + colSplice.Compression + " / " + tColumn.Shape.a + " * (" + tColumn.Shape.tf + " * " + tColumn.Shape.bf + ") + " + colSplice.Moment + " / " + tColumn.Shape.d);
				Reporting.AddLine("= " + FlangeforceU + ConstUnit.Force);

				Reporting.AddHeader("Lower Col. Flange Comp. Force, Ff_cl = C / A_lc * (tf_lc * bf_lc) + Ms / d_lc");
				Reporting.AddLine("= " + colSplice.Compression + " / " + bColumn.Shape.a + " * (" + bColumn.Shape.tf + " * " + bColumn.Shape.bf + ") + " + colSplice.Moment + " / " + bColumn.Shape.d);
				Reporting.AddLine("= " + FlangeforceL + ConstUnit.Force);

				Reporting.AddHeader("Plate Shear:");
				V = Math.Min(FlangeforceU, FlangeforceL);
				Reporting.AddLine("V = Min(Ffu, Ffl) = " + V + ConstUnit.Force);
				capacity = colSplice.ButtThickness * ConstNum.FIOMEGA0_9N * 0.6 * colSplice.Material.Fy * colSplice.ButtWidth;
				Reporting.AddLine(ConstString.PHI + " Rn = tpl * " + ConstString.FIOMEGA0_9 + " *0.6 * Fy * Wpl");
				Reporting.AddLine("= " + colSplice.ButtThickness + " * " + ConstString.FIOMEGA0_9 + " *0.6 * " + colSplice.Material.Fy + " * " + colSplice.ButtWidth);
				if (capacity >= V)
					Reporting.AddLine("= " + capacity + " >= " + V + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + V + ConstUnit.Force + " (NG)");

				Reporting.AddHeader("Plate Bending:");
				M = V * Math.Abs(bColumn.Shape.d - tColumn.Shape.d) / 2;
				Mcapacity = ConstNum.FIOMEGA0_9N * colSplice.Material.Fy * Math.Pow(colSplice.ButtThickness, 2) * colSplice.ButtWidth / 4;

				Reporting.AddHeader("Moment = V * |d_uc - d_lc| / 2 = " + V + " * " + Math.Abs(bColumn.Shape.d - tColumn.Shape.d) + " / 2= " + M + ConstUnit.Moment);
				Reporting.AddLine("");
				Reporting.AddHeader("Mom. Capacity = " + ConstString.FIOMEGA0_9 + "* Fy * t² * Wpl / 4");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + colSplice.Material.Fy + " * " + colSplice.ButtThickness + "² * " + colSplice.ButtWidth + " / 4");
				if (Mcapacity >= M)
					Reporting.AddLine("= " + Mcapacity + " >= " + M + ConstUnit.Moment + " (OK)");
				else
					Reporting.AddLine("= " + Mcapacity + " << " + M + ConstUnit.Moment + " (NG)");

				Reporting.AddHeader("Upper Col. Flange End Bearing:");
				capacity = ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(tColumn.Material.Fy, colSplice.Material.Fy) * (tColumn.Shape.tf * tColumn.Shape.bf);
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + "*1.8 * Min(Fy_uc; Fy_pl) * (tf_uc * bf_uc)");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + "*1.8 * Min(" + tColumn.Material.Fy + "; " + colSplice.Material.Fy + ") * (" + tColumn.Shape.tf + " * " + tColumn.Shape.bf + ")");
				if (capacity >= FlangeforceU)
					Reporting.AddLine("= " + capacity + " >= " + FlangeforceU + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + FlangeforceU + ConstUnit.Force + " (NG)");

				Reporting.AddHeader("Lower Col. Flange End Bearing:");
				capacity = ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(bColumn.Material.Fy, colSplice.Material.Fy) * (bColumn.Shape.tf * bColumn.Shape.bf);
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + "*1.8 * Min(Fy_lc; Fy_pl) * (tf_lc * bf_lc)");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + "*1.8 * Min(" + bColumn.Material.Fy + "; " + colSplice.Material.Fy + ") * (" + bColumn.Shape.tf + " * " + bColumn.Shape.bf + ")");
				if (capacity >= FlangeforceL)
					Reporting.AddLine("= " + capacity + " >= " + FlangeforceL + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + FlangeforceL + ConstUnit.Force + " (NG)");

				FlangeforceU = Math.Abs(Math.Min(colSplice.Compression / tColumn.Shape.a * (tColumn.Shape.tf * tColumn.Shape.bf) - colSplice.Moment / (tColumn.Shape.d - tColumn.Shape.tf), 0));
				FlangeforceL = Math.Abs(Math.Min(colSplice.Compression / bColumn.Shape.a * (bColumn.Shape.tf * bColumn.Shape.bf) - colSplice.Moment / (bColumn.Shape.d - bColumn.Shape.tf), 0));
				Ff_tU = Math.Max(Ff_tU, FlangeforceU);
				Ff_tL = Math.Max(Ff_tL, FlangeforceL);
				Reporting.AddHeader("Upper Col. Flange Ten. Force, Ff_tu = -C / A_uc * (tf_uc * bf_uc) + Ms / d_uc >= 0");
				Reporting.AddLine("= -" + colSplice.Compression + " / " + tColumn.Shape.a + " * (" + tColumn.Shape.tf + " * " + tColumn.Shape.bf + ") + " + colSplice.Moment + " / " + tColumn.Shape.d);
				Reporting.AddLine("= " + FlangeforceU + ConstUnit.Force);

				Reporting.AddHeader("Lower Col. Flange Ten. Force, Ff_tl = -C / A_lc * (tf_lc * bf_lc) + Ms / d_lc >= 0");
				Reporting.AddLine("= -" + colSplice.Compression + " / " + bColumn.Shape.a + " * (" + bColumn.Shape.tf + " * " + bColumn.Shape.bf + ") + " + colSplice.Moment + " / " + bColumn.Shape.d);
				Reporting.AddLine("= " + FlangeforceL + ConstUnit.Force);
			}
			Reporting.AddHeader("Maximum Weld Forces (as applicable):");
			Reporting.AddHeader("For PJP Groove Welds,");
			if (colSplice.UseSeismic)
			{
				if (colSplice.SMF)
				{
					Ff_tU = Math.Max(Ff_tU, ColumnSplice.FfToDevMoment);
					Ff_tL = Math.Max(Ff_tL, ColumnSplice.FfToDevMoment);
				}
				if (colSplice.ButtWeldTypeUpper == EWeldType.PJP)
					weldForceUt = Math.Max(2 * Ff_tU, ColumnSplice.FlangeTensionforSeismic) / tColumn.Shape.bf;
				else
					weldForceUt = Math.Max(Ff_tU, ColumnSplice.FlangeTensionforSeismic) / tColumn.Shape.bf;
				if (colSplice.ButtWeldTypeLower == EWeldType.PJP)
					weldForceLt = Math.Max(2 * Ff_tL, ColumnSplice.FlangeTensionforSeismic) / bColumn.Shape.bf;
				else
					weldForceLt = Math.Max(Ff_tL, ColumnSplice.FlangeTensionforSeismic) / bColumn.Shape.bf;
				seism = ", increased per seismic provisions.";
			}
			else
			{
				weldForceUt = Ff_tU / tColumn.Shape.bf;
				weldForceLt = Ff_tL / bColumn.Shape.bf;
				seism = ")";
			}

			weldForceUc = Ff_cU / tColumn.Shape.bf;
			weldForceLc = Ff_cL / bColumn.Shape.bf;
			weldForceL = Math.Max(weldForceLt, weldForceLc);
			weldForceU = Math.Max(weldForceUt, weldForceUc);

			Reporting.AddHeader("Fw_cu = Ff_cu/bf = " + weldForceUc + ConstUnit.ForcePerUnitLength + " (Up. Col.-Compression)");
			Reporting.AddLine("Fw_tu = Ff_tu/bf = " + weldForceUt + ConstUnit.ForcePerUnitLength + " (Up. Col.-Tension" + seism);
			Reporting.AddLine("Fw_cl = Ff_cl/bf = " + weldForceLc + ConstUnit.ForcePerUnitLength + " (Lower Col.-Compression)");
			Reporting.AddLine("Fw_tl = Ff_tl/bf = " + weldForceLt + ConstUnit.ForcePerUnitLength + " (Lower Col.-Tension" + seism);

			Reporting.AddHeader("For CJP Groove and Fillet Welds,");
			Reporting.AddLine("Fw_u = " + weldForceU + ConstUnit.ForcePerUnitLength + " (Upper Column)");
			Reporting.AddLine("Fw_l = " + weldForceL + ConstUnit.ForcePerUnitLength + " (Lower Column)");

			Reporting.AddHeader("Welds:");

			switch (colSplice.ButtWeldTypeUpper)
			{
				case EWeldType.Fillet:
					Reporting.AddLine("Upper Column/Butt Plate Fillet Weld: " + CommonCalculations.WeldSize(colSplice.ButtWeldSizeUpper) + ConstUnit.Length);
					Reporting.AddLine("");
					minweldsize = CommonCalculations.MinimumWeld(colSplice.ButtThickness, tColumn.Shape.tf);
					if (minweldsize <= colSplice.ButtWeldSizeUpper)
						Reporting.AddLine("Minimum Weld Size = " + minweldsize + " <= " + colSplice.ButtWeldSizeUpper + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Minimum Weld Size = " + minweldsize + " >> " + colSplice.ButtWeldSizeUpper + ConstUnit.Length + " (NG)");

					capacity = ConstNum.FIOMEGA0_75N * 0.4242 * colSplice.ButtWeldSizeUpper * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine("Weld Capacity = " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * w * Fexx = " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + colSplice.ButtWeldSizeUpper + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					if (capacity >= weldForceU)
						Reporting.AddLine("= " + capacity + " >= " + weldForceU + ConstUnit.ForcePerUnitLength + " (OK)");
					else
						Reporting.AddLine("= " + capacity + " << " + weldForceU + ConstUnit.ForcePerUnitLength + " (NG)");
					break;
				case EWeldType.PJP:
					Reporting.AddLine("Upper Column/Butt Plate PJP Weld: " + CommonCalculations.WeldSize(colSplice.ButtWeldSizeUpper) + ConstUnit.Length);

					minweldsize = MinimumPJPSize(colSplice.ButtThickness, tColumn.Shape.tf);
					if (minweldsize <= colSplice.ButtWeldSizeUpper)
						Reporting.AddLine("Minimum Weld Size = " + minweldsize + " <= " + colSplice.ButtWeldSizeUpper + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Minimum Weld Size = " + minweldsize + " >> " + colSplice.ButtWeldSizeUpper + ConstUnit.Length + " (NG)");

					maxweldsize = tColumn.Shape.tf - ConstNum.EIGHTH_INCH;
					if (maxweldsize >= colSplice.ButtWeldSizeUpper)
						Reporting.AddLine("Maximum Weld Size = " + maxweldsize + " >= " + colSplice.ButtWeldSizeUpper + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Maximum Weld Size = " + maxweldsize + " << " + colSplice.ButtWeldSizeUpper + ConstUnit.Length + " (NG)");

					if (weldForceUt > 0)
					{
						capacity = colSplice.ButtWeldSizeUpper * Math.Min(0.8 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx, ConstNum.FIOMEGA0_9N * Math.Min(colSplice.Material.Fy, tColumn.Material.Fy));
						Reporting.AddHeader("Weld Capacity = w * Min[(0.8 * 0.6 * Fexx); " + ConstString.FIOMEGA0_9 + " * Min(Fy_pl, Fy_uc)]");
						Reporting.AddLine("= " + colSplice.ButtWeldSizeUpper + " * Min[(0.8 * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + "), " + ConstString.FIOMEGA0_9 + " *Min(" + colSplice.Material.Fy + "; " + tColumn.Material.Fy + ")]");
						if (capacity >= weldForceUt)
							Reporting.AddLine("= " + capacity + " >= " + weldForceUt + ConstUnit.ForcePerUnitLength + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + weldForceUt + ConstUnit.ForcePerUnitLength + " (NG)");
					}
					if (weldForceUc > 0)
					{
						capacity = colSplice.ButtWeldSizeUpper * ConstNum.FIOMEGA0_9N * Math.Min(colSplice.Material.Fy, tColumn.Material.Fy);
						Reporting.AddLine("Weld Capacity = w * " + ConstString.FIOMEGA0_9 + " * Min(Fy_pl, Fy_uc)");
						Reporting.AddLine("= " + colSplice.ButtWeldSizeUpper + " * " + ConstString.FIOMEGA0_9 + " * Min(" + colSplice.Material.Fy + ", " + tColumn.Material.Fy + ")");
						if (capacity >= weldForceUc)
							Reporting.AddLine("= " + capacity + " >= " + weldForceUc + ConstUnit.ForcePerUnitLength + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + weldForceUc + ConstUnit.ForcePerUnitLength + " (NG)");
					}
					break;
				default:
					Reporting.AddHeader("Upper Column/Butt Plate Weld: CJP");
					capacity = tColumn.Shape.tf * ConstNum.FIOMEGA0_9N * Math.Min(colSplice.Material.Fy, tColumn.Material.Fy);
					Reporting.AddLine(ConstString.PHI + " Rn = tf * " + ConstString.FIOMEGA0_9 + "  * Min(Fy_pl, Fy_uc)");
					Reporting.AddLine("= " + tColumn.Shape.tf + " * " + ConstString.FIOMEGA0_9 + "  * Min(" + colSplice.Material.Fy + "; " + tColumn.Material.Fy + ")");
					if (capacity >= weldForceU)
						Reporting.AddLine("= " + capacity + " >= " + weldForceU + ConstUnit.ForcePerUnitLength + " (OK)");
					else
						Reporting.AddLine("= " + capacity + " << " + weldForceU + ConstUnit.ForcePerUnitLength + " (NG)");
					break;
			}

			switch (colSplice.ButtWeldTypeLower)
			{
				case EWeldType.Fillet:
					Reporting.AddHeader("Lower Column/Butt Plate Fillet Weld: " + CommonCalculations.WeldSize(colSplice.ButtWeldSizeLower) + ConstUnit.Length);
					minweldsize = CommonCalculations.MinimumWeld(colSplice.ButtThickness, bColumn.Shape.tf);
					if (minweldsize <= colSplice.ButtWeldSizeLower)
						Reporting.AddLine("Minimum Weld Size = " + minweldsize + " <= " + colSplice.ButtWeldSizeLower + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Minimum Weld Size = " + minweldsize + " >> " + colSplice.ButtWeldSizeLower + ConstUnit.Length + " (NG)");

					capacity = ConstNum.FIOMEGA0_75N * 0.4242 * colSplice.ButtWeldSizeLower * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddHeader("Weld Capacity = " + ConstString.FIOMEGA0_75 + " *0.4242" + "* w * Fexx = " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + colSplice.ButtWeldSizeLower + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx)
					;
					if (capacity >= weldForceL)
						Reporting.AddLine("= " + capacity + " >= " + weldForceL + ConstUnit.ForcePerUnitLength + " (OK)");
					else
						Reporting.AddLine("= " + capacity + " << " + weldForceL + ConstUnit.ForcePerUnitLength + " (NG)");
					break;
				case EWeldType.PJP:
					Reporting.AddHeader("Lower Column/Butt Plate PJP Weld: " + CommonCalculations.WeldSize(colSplice.ButtWeldSizeLower) + ConstUnit.Length);
					minweldsize = MinimumPJPSize(colSplice.ButtThickness, bColumn.Shape.tf);
					if (minweldsize <= colSplice.ButtWeldSizeLower)
						Reporting.AddLine("Minimum Weld Size = " + minweldsize + " <= " + colSplice.ButtWeldSizeLower + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Minimum Weld Size = " + minweldsize + " >> " + colSplice.ButtWeldSizeLower + ConstUnit.Length + " (NG)");

					maxweldsize = bColumn.Shape.tf - ConstNum.EIGHTH_INCH;
					if (maxweldsize >= colSplice.ButtWeldSizeLower)
						Reporting.AddLine("Maximum Weld Size = " + maxweldsize + " >= " + colSplice.ButtWeldSizeLower + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Maximum Weld Size = " + maxweldsize + " << " + colSplice.ButtWeldSizeLower + ConstUnit.Length + " (NG)");

					if (weldForceLt > 0)
					{
						capacity = colSplice.ButtWeldSizeLower * Math.Min(0.8 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx, ConstNum.FIOMEGA0_9N * Math.Min(colSplice.Material.Fy, bColumn.Material.Fy));
						Reporting.AddHeader("Weld Capacity = w * Min[(0.8 * 0.6 *  Fexx), " + ConstString.FIOMEGA0_9 + " * Min(Fy_pl, Fy_lc)]");
						Reporting.AddLine("= " + colSplice.ButtWeldSizeLower + " * Min[(0.8 * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + "); " + ConstString.FIOMEGA0_9 + " * Min(" + colSplice.Material.Fy + ", " + bColumn.Material.Fy + ")]");
						if (capacity >= weldForceLt)
							Reporting.AddLine("= " + capacity + " >= " + weldForceLt + ConstUnit.ForcePerUnitLength + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + weldForceLt + ConstUnit.ForcePerUnitLength + " (NG)");
					}
					if (weldForceLc > 0)
					{
						capacity = colSplice.ButtWeldSizeLower * ConstNum.FIOMEGA0_9N * Math.Min(colSplice.Material.Fy, bColumn.Material.Fy);
						Reporting.AddHeader("Weld Capacity = w * " + ConstString.FIOMEGA0_9 + " * Min(Fy_pl, Fy_uc)");
						Reporting.AddLine("= " + colSplice.ButtWeldSizeLower + " * " + ConstString.FIOMEGA0_9 + " * Min(" + colSplice.Material.Fy + "; " + bColumn.Material.Fy + ")");
						if (capacity >= weldForceLc)
							Reporting.AddLine("= " + capacity + " >= " + weldForceLc + ConstUnit.ForcePerUnitLength + " (OK)");
						else
							Reporting.AddLine("= " + capacity + " << " + weldForceLc + ConstUnit.ForcePerUnitLength + " (NG)");
					}
					break;
				default:
					Reporting.AddHeader("Lower Column/Butt Plate Weld: CJP");
					capacity = bColumn.Shape.tf * ConstNum.FIOMEGA0_9N * Math.Min(colSplice.Material.Fy, bColumn.Material.Fy);
					Reporting.AddLine(ConstString.PHI + " Rn = tf * " + ConstString.FIOMEGA0_9 + "  * Min(Fy_pl, Fy_lc)");
					Reporting.AddLine("= " + bColumn.Shape.tf + " * " + ConstString.FIOMEGA0_9 + "  * Min(" + colSplice.Material.Fy + "; " + bColumn.Material.Fy + ")");
					if (capacity >= weldForceL)
						Reporting.AddLine("= " + capacity + " >= " + weldForceL + ConstUnit.ForcePerUnitLength + " (OK)");
					else
						Reporting.AddLine("= " + capacity + " << " + weldForceL + ConstUnit.ForcePerUnitLength + " (NG)");
					break;
			}
		}
	}
}