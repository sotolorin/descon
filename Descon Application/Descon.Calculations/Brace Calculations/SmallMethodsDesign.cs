using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class SmallMethodsDesign
	{
		internal static double TeeExcentricity(double bf, double tf, double d, double tw, double a)
		{
			return (bf * Math.Pow(tf, 2) + tw * (Math.Pow(d, 2) - Math.Pow(tf, 2))) / (2 * a);
		}

		internal static void Intersect(double slp1, double x1, double y1, double slop2, double x2, double y2, ref double x, ref double y)
		{
			// Intersection of
			// y=slp1*(x-x1)+y1
			// y=slp2*(x-x2)+y2

			if (!Equals(slp1, slop2))
			{
				if (Math.Abs(slp1) <= 99 && Math.Abs(slop2) <= 99)
				{
					x = ((slp1 * x1 - y1 - slop2 * x2 + y2) / (slp1 - slop2));
					y = (slp1 * (x - x1) + y1);
				}
				else if (Math.Abs(slp1) > 99 && Math.Abs(slop2) <= 99)
				{
					x = (x1);
					y = (slop2 * (x1 - x2) + y2);
				}
				else if (Math.Abs(slp1) <= 99 && Math.Abs(slop2) > 99)
				{
					x = (x2);
					y = (slp1 * (x2 - x1) + y1);
				}
			}
		}

		internal static void ClipWelds(EMemberType memberType, double Hforce, double Vforce, ref double AlengthWeld, double AlengthMax, ref double weldcap, bool enableReporting)
		{
			bool moreThanMax = false;
			double weldforCap = 0;
			double c1 = 0;
			double R = 0;
			double Cnst = 0;
			double Theta = 0;
			int leg = 0;
			double a = 0;
			double k = 0;
			double hL = 0;
			double minweld = 0;
			double wmax = 0;
			double t = 0;
			double wmaxlimit = 0;
			double ultstr = 0;
			double Gap = 0;
			double thick = 0;

			var component = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			if (memberType == EMemberType.LeftBeam || memberType == EMemberType.RightBeam)
			{
				thick = component.Shape.tw;
				Gap = component.EndSetback;
				ultstr = component.Material.Fu;
			}
			else
			{
				thick = component.BraceConnect.Gusset.Thickness;
				Gap = component.EndSetback;
				ultstr = component.BraceConnect.Gusset.Material.Fu;
			}
			AlengthWeld = component.WinConnect.ShearClipAngle.Length;
			wmaxlimit = 0.7072 * ultstr * thick / CommonDataStatic.Preferences.DefaultElectrode.Fexx;

			//  Clip Angle Welds
			t = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].WinConnect.ShearClipAngle.Thickness;
			if (t < ConstNum.QUARTER_INCH)
				wmax = t;
			else
				wmax = t - ConstNum.SIXTEENTH_INCH;

			minweld = CommonCalculations.MinimumWeld(t, thick);
			if (MiscMethods.IsBeam(memberType))
			{
				if (component.WinConnect.ShearClipAngle.OSL == EOSL.ShortLeg)
					hL = component.WinConnect.ShearClipAngle.LongLeg;
				else
					hL = component.WinConnect.ShearClipAngle.ShortLeg;
			}

			if (CommonDataStatic.Preferences.UseContinuousClipAngles)
			{
				switch (memberType)
				{
					case EMemberType.RightBeam:
						if (CommonDataStatic.DetailDataDict[EMemberType.UpperRight].IsActive && CommonDataStatic.DetailDataDict[EMemberType.LowerRight].IsActive)
						{
							k = 0;
							a = hL / AlengthWeld;
							leg = 0;
						}
						else if (CommonDataStatic.DetailDataDict[EMemberType.UpperRight].IsActive || CommonDataStatic.DetailDataDict[EMemberType.LowerRight].IsActive)
						{
							k = (hL - Gap) / AlengthWeld;
							a = (hL - AlengthWeld * k * k / (2 + k + k)) / AlengthWeld;
							leg = 2;
						}
						else
						{
							k = (hL - Gap) / AlengthWeld;
							a = (hL - AlengthWeld * k * k / (1 + k + k)) / AlengthWeld;
							leg = 1;
						}
						break;
					case EMemberType.LeftBeam:
						if (CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].IsActive && CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].IsActive)
						{
							k = 0;
							a = hL / AlengthWeld;
							leg = 0;
						}
						else if (CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].IsActive || CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].IsActive)
						{
							k = (hL - Gap) / AlengthWeld;
							a = (hL - AlengthWeld * k * k / (2 + k + k)) / AlengthWeld;
							leg = 2;
						}
						else
						{
							k = (hL - Gap) / AlengthWeld;
							a = (hL - AlengthWeld * k * k / (1 + k + k)) / AlengthWeld;
							leg = 1;
						}
						break;
					default:
						k = (hL - Gap) / AlengthWeld;
						a = (hL - AlengthWeld * k * k / (2 + k + k)) / AlengthWeld;
						leg = 2;
						break;
				}
			}
			else
			{
				k = (hL - Gap) / AlengthWeld;
				a = (hL - AlengthWeld * k * k / (1 + k + k)) / AlengthWeld;
				leg = 1;
			}

			if (memberType != EMemberType.LeftBeam && memberType != EMemberType.RightBeam && component.BraceConnect.Gusset.Mc != 0)
			{
				if (component.BraceConnect.Gusset.VerticalForceColumn == 0)
					a = a + Math.Abs(component.BraceConnect.Gusset.Mc / (1 * AlengthWeld));
				else
					a = a + Math.Abs(component.BraceConnect.Gusset.Mc / (component.BraceConnect.Gusset.VerticalForceColumn * AlengthWeld));
			}
			if (Vforce == 0)
				Theta = Math.PI / 2;
			else
				Theta = Math.Atan(Math.Abs(Hforce / Vforce));
			if (k > 2)
				k = 2;
			if (a > 3)
				a = 3;

			if (enableReporting)
			{
				Reporting.AddHeader("Weld Size Required for Inclined Eccentric Load:");
				Reporting.AddLine("Maximum useful weld size = 0.7072 * Fu * t / Fexx");
				Reporting.AddLine("= 0.7072 * " + ultstr + " * " + thick + " / " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " = " + wmaxlimit + ConstUnit.Length);

				if (k == 0)
					Reporting.AddLine("k = 0");
				else
				{
					Reporting.AddLine("k = (wh - gap) / L ");
					Reporting.AddLine("= (" + hL + " - " + Gap + ") / " + AlengthWeld + " = " + k);
				}

				if (memberType != EMemberType.LeftBeam && memberType != EMemberType.RightBeam && component.BraceConnect.Gusset.Mc != 0)
				{
					Reporting.AddLine("a = wh / L - k² / (" + leg + " + 2 * k) + Mc / (Vc * L)");
					Reporting.AddLine("= " + hL + " / " + AlengthWeld + " - " + k + "² / (" + leg + " + 2 * " + k + ") + " + component.BraceConnect.Gusset.Mc + " / (" + component.BraceConnect.Gusset.VerticalForceColumn + " * " + AlengthWeld + ")");
				}
				else
				{
					if (leg == 0)
						Reporting.AddLine("a = wh / L = " + hL + " / " + AlengthWeld);
					else
					{
						Reporting.AddLine("a = wh / L - k² / (" + leg + " + 2 * k)");
						Reporting.AddLine("= " + hL + " / " + AlengthWeld + " - " + k + "² / (" + leg + " + 2 * " + k + ") = " + a);

						Reporting.AddLine(216 + " = Arctan(H / V) ");
						Reporting.AddLine("= Arctan(" + Hforce + " / " + Vforce + ") ");
						Reporting.AddLine("= " + (Theta * 180 / Math.PI) + " Degrees");
					}
				}
			}

			if (CommonDataStatic.Preferences.UseContinuousClipAngles)
			{
				switch (memberType)
				{
					case EMemberType.RightBeam:
						if (CommonDataStatic.DetailDataDict[EMemberType.UpperRight].IsActive || CommonDataStatic.DetailDataDict[EMemberType.LowerRight].IsActive)
							CommonDataStatic.LShapedWeld = true;
						else
							CommonDataStatic.LShapedWeld = false;
						break;
					case EMemberType.LeftBeam:
						if (CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].IsActive || CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].IsActive)
							CommonDataStatic.LShapedWeld = true;
						else
							CommonDataStatic.LShapedWeld = false;
						break;
					case EMemberType.UpperRight:
					case EMemberType.LowerRight:
						if (CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive)
							CommonDataStatic.LShapedWeld = true;
						else
							CommonDataStatic.LShapedWeld = false;
						break;
					case EMemberType.UpperLeft:
					case EMemberType.LowerLeft:
						if (CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive)
							CommonDataStatic.LShapedWeld = true;
						else
							CommonDataStatic.LShapedWeld = false;
						break;
				}
			}
			else
				CommonDataStatic.LShapedWeld = false;

			Cnst = EccentricWeld.GetEccentricWeld(k, a, Theta, enableReporting);
			//EccentricWeld.GetEccentricWeld(k, a, ref Cnst, Theta, enableReporting);
			R = Math.Sqrt(Math.Pow(Hforce, 2) + Math.Pow(Vforce, 2)) / 2;
			c1 = CommonCalculations.WeldTypeRatio();

			if (component.WinConnect.ShearClipAngle.WeldSize < minweld)
				component.WinConnect.ShearClipAngle.WeldSize = minweld;
			t = component.WinConnect.ShearClipAngle.Thickness;
			if (t < ConstNum.QUARTER_INCH)
				wmax = t;
			else
				wmax = t - ConstNum.SIXTEENTH_INCH;
			if (component.WinConnect.ShearClipAngle.WeldSize > wmax)
				component.WinConnect.ShearClipAngle.WeldSize = wmax;

			if (component.WinConnect.ShearClipAngle.WeldSize > wmaxlimit)
			{
				weldforCap = wmaxlimit;
				moreThanMax = true;
			}
			else
				weldforCap = component.WinConnect.ShearClipAngle.WeldSize;

			weldcap = 2 * (Cnst * AlengthWeld * c1 * 16) * weldforCap;

			if (enableReporting)
			{
				Reporting.AddLine("Try w = " + CommonCalculations.WeldSize(component.WinConnect.ShearClipAngle.WeldSize) + ConstUnit.Length + " weld");
				if (wmax >= component.WinConnect.ShearClipAngle.WeldSize)
					Reporting.AddLine("Maximum weld size for angle thickness = " + wmax + " >= " + component.WinConnect.ShearClipAngle.WeldSize + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Maximum weld size for angle thickness = " + wmax + " << " + component.WinConnect.ShearClipAngle.WeldSize + ConstUnit.Length + " (NG)");

				if (minweld <= component.WinConnect.ShearClipAngle.WeldSize)
					Reporting.AddLine("Minimum weld size = " + minweld + " <= " + component.WinConnect.ShearClipAngle.WeldSize + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum weld size = " + minweld + " >> " + component.WinConnect.ShearClipAngle.WeldSize + ConstUnit.Length + " (NG)");

				if (moreThanMax)
					Reporting.AddHeader("Use w = " + weldforCap + ConstUnit.Length + " for capacity calculation");

				Reporting.AddLine("Weld = C * L * c1 * 16 * w ");
				Reporting.AddLine("= " + Cnst + " * " + AlengthWeld + " * " + c1 + " * 16 * " + weldforCap);
				Reporting.AddLine("= " + weldcap + ConstUnit.Force);
				Reporting.AddLine("Resultant Load (R) = 0.5 * (H² + V²)^0.5 ");
				Reporting.AddLine("= 0.5 * ((" + Hforce + ")² + (" + Vforce + ")²)^0.5 ");

				if (R <= weldcap / 2)
					Reporting.AddLine(R + ConstUnit.Force + " <= " + weldcap + ConstUnit.Force + " (OK)");
				else if (R <= ConstNum.TOLERANCE * weldcap / 2)
					Reporting.AddLine(R + ConstUnit.Force + " >> " + weldcap + ConstUnit.Force + " (NG) (Close)");
				else
					Reporting.AddLine(R + ConstUnit.Force + " >> " + weldcap + ConstUnit.Force + " (NG)");
			}
		}

		/// <summary>
		/// Calculates the Minimum Bolt Spacing
		/// </summary>
		internal static double MinBoltSpacing(Bolt bolt, double fu, double t, double P, double holeLength)
		{
			double minBoltSpacing;

			if (bolt.HoleType == EBoltHoleType.LSLN)
				minBoltSpacing = P / (ConstNum.FIOMEGA0_75N * t * fu) + holeLength;
			else
				minBoltSpacing = P / (ConstNum.FIOMEGA0_75N * 1.2 * t * fu) + holeLength;

			if (double.IsNaN(minBoltSpacing))
				minBoltSpacing = 0;

			if (minBoltSpacing > 3 * bolt.BoltSize)
				minBoltSpacing = 3 * bolt.BoltSize;
			if (minBoltSpacing < (8.0 / 3) * bolt.BoltSize)
				minBoltSpacing = (8.0 / 3) * bolt.BoltSize;

			return minBoltSpacing;
		}

		internal static double BlockShearPrint(double agv, double anv, object agt, double ant, double fy, double fu, bool enableReporting)
		{
			double fiRn = ConstNum.FIOMEGA0_75N * (0.6 * Math.Min(fu * anv, fy * agv) + 1 * fu * ant);

			if (enableReporting)
			{
				Reporting.AddHeader("Block Shear Print:");
				Reporting.AddLine(fiRn + "  fiRn = " + ConstString.FIOMEGA0_75 + " * (0.6 * Min(" + fu + " fu * " + anv + " anv, " + fy + " + fy * " + agv + " agv) + " + 1 + " 1 * " + fu + " fu * " + ant + " ant)");
			}

			return fiRn;
		}

		internal static double MinClearDistForBearing(double p, double t, double fu, EBoltHoleType holeType)
		{
			double Lc;
			double c;

			switch (holeType)
			{
				case EBoltHoleType.LSLN:
					c = ConstNum.FIOMEGA0_75N;
					break;
				default:
					c = ConstNum.FIOMEGA0_75N * 1.2;
					break;
			}
			Lc = p / (c * t * fu);
			return double.IsNaN(Lc) ? 0 : Lc;
		}

		internal static void GussetandBeamCheckwithFabTee(EMemberType memberType, double V, double H, double R, int EquivBoltFactor, EBearingTearOut BearingorTearOut)
		{
			double BlockShearCapacity1 = 0;
			double An1 = 0;
			double Ag1 = 0;
			double GrossTensionandShearCapacity = 0;
			double NetTensionandShearCapacity = 0;
			double An2 = 0;
			double Ag2 = 0;
			double An = 0;
			double Ag = 0;
			double b = 0;
			double a = 0;
			double BearingCapacityH = 0;
			double BearingCapacityV = 0;
			double Fbe = 0;
			double Fbs = 0;
			double Lv = 0;
			double Lh = 0;
			double Th = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];

			if (MiscMethods.IsBeam(memberType))
			{
				Th = component.Shape.tw;
				Lh = component.WinConnect.ShearWebTee.Eh2;
			}
			else
			{
				Th = component.BraceConnect.Gusset.Thickness;
				Lh = component.BraceConnect.FabricatedTee.Eh2;
				Lv = component.BraceConnect.Gusset.VerticalForceColumn - (component.BraceConnect.FabricatedTee.FirstBoltDistance + component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir *
				                                                          (component.BraceConnect.FabricatedTee.Bolt.NumberOfRows - 1));
			}
			switch (BearingorTearOut)
			{
				case EBearingTearOut.Bearing:
					if (memberType != EMemberType.LeftBeam && memberType != EMemberType.RightBeam)
						Reporting.AddHeader("Bolt Bearing on Gusset");
					else
						Reporting.AddHeader("Bolt Bearing on Beam Web:");
					Reporting.AddHeader("Vertical Load:");

					Fbs = CommonCalculations.SpacingBearing(component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir, component.BraceConnect.FabricatedTee.Bolt.HoleWidth, component.BraceConnect.FabricatedTee.Bolt.BoltSize, component.BraceConnect.FabricatedTee.Bolt.HoleType, component.Material.Fu, false);

					if (memberType != EMemberType.LeftBeam && memberType != EMemberType.RightBeam)
					{
						Fbe = CommonCalculations.EdgeBearing(Lv, component.BraceConnect.FabricatedTee.Bolt.HoleWidth, component.BraceConnect.FabricatedTee.Bolt.BoltSize, component.Material.Fu, component.BraceConnect.FabricatedTee.Bolt.HoleType, false);
						BearingCapacityV = EquivBoltFactor * component.BraceConnect.FabricatedTee.Bolt.NumberOfLines * (Fbe + Fbs * (component.BraceConnect.FabricatedTee.Bolt.NumberOfRows - 1)) * component.BraceConnect.FabricatedTee.Bolt.BoltSize * Th;
						Reporting.AddLine(ConstString.PHI + " Rn = ef * Nh * (Fbre + Fbrs * (Nl - 1)) * db * t");
						Reporting.AddLine("= " + EquivBoltFactor + " * " + component.BraceConnect.FabricatedTee.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + component.BraceConnect.FabricatedTee.Bolt.BoltSize + " * " + Th);
					}
					else
					{
						BearingCapacityV = EquivBoltFactor * component.BraceConnect.FabricatedTee.Bolt.NumberOfLines * Fbs * component.BraceConnect.FabricatedTee.Bolt.NumberOfRows * component.BraceConnect.FabricatedTee.Bolt.BoltSize * Th;
						Reporting.AddLine(ConstString.PHI + " Rn = ef * Nh * Fbs * Nl * db * t");
						Reporting.AddLine("= " + EquivBoltFactor + " * " + component.BraceConnect.FabricatedTee.Bolt.NumberOfLines + " * " + Fbs + " * " + component.BraceConnect.FabricatedTee.Bolt.NumberOfRows + " * " + component.BraceConnect.FabricatedTee.Bolt.BoltSize + " * " + Th);
					}
					if (BearingCapacityV >= V)
						Reporting.AddLine("= " + BearingCapacityV + " >= " + V + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + BearingCapacityV + " << " + V + ConstUnit.Force + " (NG)");

					// Bolt Bearing under Horizontal Load
					Reporting.AddHeader("Horizontal Load:");
					Fbs = CommonCalculations.SpacingBearing(component.BraceConnect.FabricatedTee.Bolt.SpacingTransvDir, component.BraceConnect.FabricatedTee.Bolt.HoleLength, component.BraceConnect.FabricatedTee.Bolt.BoltSize, component.BraceConnect.FabricatedTee.Bolt.HoleType, component.Material.Fu, false);
					Fbe = CommonCalculations.EdgeBearing(Lh, (component.BraceConnect.FabricatedTee.Bolt.HoleLength), component.BraceConnect.FabricatedTee.Bolt.BoltSize, component.Material.Fu, component.BraceConnect.FabricatedTee.Bolt.HoleType, false);
					BearingCapacityH = EquivBoltFactor * component.BraceConnect.FabricatedTee.Bolt.NumberOfRows * (Fbe + Fbs * (component.BraceConnect.FabricatedTee.Bolt.NumberOfLines - 1)) * component.BraceConnect.FabricatedTee.Bolt.BoltSize * Th;
					Reporting.AddLine(ConstString.PHI + " Rn = ef * Nl * (Fbe + Fbs * (Nh - 1)) * db * t");
					Reporting.AddLine("= " + EquivBoltFactor + " * " + component.BraceConnect.FabricatedTee.Bolt.NumberOfRows + " * (" + Fbe + " + " + Fbs + " * (" + component.BraceConnect.FabricatedTee.Bolt.NumberOfLines + " - 1)) * " + component.BraceConnect.FabricatedTee.Bolt.BoltSize + " * " + Th);
					if (BearingCapacityH >= H)
						Reporting.AddLine("= " + BearingCapacityH + " >= " + H + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + BearingCapacityH + " << " + H + ConstUnit.Force + " (NG)");
					break;

				case EBearingTearOut.TearOut:
					if (memberType != EMemberType.LeftBeam && memberType != EMemberType.RightBeam)
						Reporting.AddHeader("Gusset Tear-out");
					else
						Reporting.AddHeader("Beam Web Tear-out");

					// Combined Tension and Shear Rupture
					Reporting.AddHeader("Combined Tension and Shear");
					if (R != 0)
					{
						a = H / R;
						b = V / R;
					}

					if (V == 0)
						Reporting.AddHeader("Load Angle, " + ConstString.PHI + " = Atn(H / V) = 90 degrees");
					else
						Reporting.AddHeader("Load Angle, " + ConstString.PHI + " = Atn(H / V) = " + (Math.Atan(Math.Abs(H / V)) / ConstNum.RADIAN) + " degrees");
					Reporting.AddLine("A = Sin(" + ConstString.PHI + ") = " + a);
					Reporting.AddLine("B = Cos(" + ConstString.PHI + ") = " + b);

					NetAndGrossArea(memberType, 0, ref Ag, ref An, ref Ag2, ref An2);
					Reporting.AddLine("Rupture:");
					if (b > 0 && a > 0)
					{
						NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.18 * (a / b) * (-1 + Math.Pow(1 + Math.Pow(b / a, 2) / 0.09, 0.5)) * An * component.Material.Fu / b;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.18 * (A/ B) * (-1 + (1 + (B / A)² / 0.09)^0.5) * An * Fu / B");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.18 * (" + a + " / " + b + ") * (-1 + (1 + (" + b + " / " + a + ")² / 0.09)^0.5) * " + An + " * " + component.Material.Fu + " / " + b);
					}
					else if (b == 0)
					{
						NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * An * component.Material.Fu;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * An*Fu = " + ConstString.FIOMEGA0_75 + " * " + An + " * " + component.Material.Fu);
					}
					else if (a == 0)
					{
						NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.6 * An * component.Material.Fu;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.6 * An * Fu = " + ConstString.FIOMEGA0_75 + " * 0.6 * " + An + " * " + component.Material.Fu);
					}
					if (NetTensionandShearCapacity >= R)
						Reporting.AddLine("= " + NetTensionandShearCapacity + " >= " + R + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + NetTensionandShearCapacity + " << " + R + ConstUnit.Force + " (NG)");
					// Combined Tension and Shear Yielding
					Reporting.AddHeader("Yielding:");
					if (b > 0 && a > 0)
					{
						GrossTensionandShearCapacity = Math.Pow(ConstNum.FIOMEGA1_0N, 2) * 0.18 * Math.Pow(a / b, 2) * (-1 / ConstNum.FIOMEGA0_9N + Math.Pow(1 / Math.Pow(ConstNum.FIOMEGA0_9N, 2) + Math.Pow(b / a, 2) / (0.09 * Math.Pow(ConstNum.FIOMEGA1_0N, 2)), 0.5)) * Ag * component.Material.Fy / a;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.FIOMEGA1_0N + "² * 0.18 * (A / B)² * (-1 / " + ConstString.FIOMEGA0_9 + " + (1 / (" + ConstString.FIOMEGA0_9 + "²) + (B / A)² / (0.09 * " + ConstNum.FIOMEGA1_0N + "²))^0.5) * Ag * Fy/A");
						Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + "² * 0.18 * (" + a + " / " + b + ")² * (-1 / " + ConstString.FIOMEGA0_9 + " + (1 / (" + ConstString.FIOMEGA0_9 + "²) + (" + b + " / " + a + ")² / (0.09 * " + ConstNum.FIOMEGA1_0N + "²))^0.5) * " + Ag + " * " + component.Material.Fy + " / " + a);
					}
					else if (b == 0)
					{
						GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * Ag * component.Material.Fy;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Ag * Fy = " + ConstString.FIOMEGA0_9 + " * " + Ag + " * " + component.Material.Fy);
					}
					else if (a == 0)
					{
						GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * 0.6 * Ag * component.Material.Fy;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * 0.6 * Ag * Fy = " + ConstString.FIOMEGA0_9 + " * 0.6 * " + Ag + " * " + component.Material.Fy);
					}
					if (GrossTensionandShearCapacity >= R)
						Reporting.AddLine("= " + GrossTensionandShearCapacity + " >= " + R + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + GrossTensionandShearCapacity + " << " + R + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Block Shear:");
					Reporting.AddLine("Vertical (An1, Ft1) and Horizontal (An2, Ft2) Sections:");
					if (memberType != EMemberType.LeftBeam && memberType != EMemberType.RightBeam)
					{
						Reporting.AddHeader("Pattern 1:");
						NetAndGrossArea(memberType, 1, ref Ag1, ref An1, ref Ag2, ref An2);
						BlockShearCapacity1 = ShearTensionInteraction(1, R, a, b, component.Material.Fu, An1, An2);
					}
					if (BlockShearCapacity1 >= R)
						Reporting.AddLine("= " + BlockShearCapacity1 + " >= " + R + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + BlockShearCapacity1 + " << " + R + ConstUnit.Force + " (NG)");

					// Block Shear 2
					Reporting.AddHeader("Pattern 2:");
					NetAndGrossArea(memberType, 2, ref Ag1, ref An1, ref Ag2, ref An2);
					break;
			}
		}

		internal static void HSSSideWallYielding(EMemberType memberType)
		{
			double FiRn;
			double R;
			double length = 0;
			double mom;
			double V;
			double H;

			var component = CommonDataStatic.DetailDataDict[memberType];

			if (memberType == EMemberType.PrimaryMember || memberType == EMemberType.LeftBeam || memberType == EMemberType.RightBeam)
			{
				H = component.WinConnect.ShearClipAngle.ForceX;
				V = component.WinConnect.ShearClipAngle.ForceY;
				mom = 0;
			}
			else
			{
				H = component.BraceConnect.Gusset.Hc;
				V = component.BraceConnect.Gusset.VerticalForceColumn;
				mom = component.BraceConnect.Gusset.Mc;
			}

			if (component.GussetToColumnConnection == EBraceConnectionTypes.FabricatedTee)
				length = component.BraceConnect.FabricatedTee.Length;
			else if (component.GussetToColumnConnection == EBraceConnectionTypes.EndPlate)
				length = component.WinConnect.ShearEndPlate.Length;

			R = Math.Pow(Math.Pow(H + 3 * mom / length, 2) + Math.Pow(1.73 * V, 2), 0.5);
			FiRn = 2 * 1 * component.Material.Fy * component.Shape.tw * (5 * 1.5 * component.Shape.tw + length);

			Reporting.AddHeader("Yielding of HSS Side Wall Resultant force:");
			Reporting.AddLine("  R = ((H + 3 * M / L)² + (1.73 * V)²)^0.5");
			Reporting.AddLine("= ((" + H + " + 3 * " + mom + " / " + length + ")² + (1.73 * " + V + ")²)^0.5");
			Reporting.AddLine("= " + R + ConstUnit.Force);

			Reporting.AddLine(ConstString.PHI + " Rn = 2 * 1.0 * Fy * t * (5 * 1.5 * t + N)");
			Reporting.AddLine("= 2 * 1.0 * " + component.Material.Fy + " * " + component.Shape.tw + " * (5 * 1.5 * " + component.Shape.tw + " + " + length + ")");
			if (FiRn >= R)
				Reporting.AddLine("= " + FiRn + " >= " + R + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + R + ConstUnit.Force + " (NG)");
		}

		internal static void HSSSideWallCripling(EMemberType memberType)
		{
			double FiRn;
			double R;
			double length = 0;
			double mom;
			double H;

			var component = CommonDataStatic.DetailDataDict[memberType];

			if (memberType == EMemberType.PrimaryMember || memberType == EMemberType.LeftBeam || memberType == EMemberType.RightBeam)
			{
				H = component.WinConnect.ShearClipAngle.ForceX;
				mom = 0;
			}
			else
			{
				H = component.BraceConnect.Gusset.Hc;
				mom = component.BraceConnect.Gusset.Mc;
			}

			if (component.GussetToColumnConnection == EBraceConnectionTypes.FabricatedTee)
				length = component.BraceConnect.FabricatedTee.Length;
			else if (component.GussetToColumnConnection == EBraceConnectionTypes.EndPlate)
				length = component.WinConnect.ShearEndPlate.Length;

			R = H + 3 * mom / length;
			FiRn = ConstNum.FIOMEGA0_75N * 1.6 * Math.Pow(ConstNum.ELASTICITY, 0.5) * Math.Pow(component.Shape.tw, 2) * (1 + 3 * length / component.Shape.d) * Math.Pow(component.Material.Fy, 0.5);

			Reporting.AddHeader("Crippling  of HSS Side Wall Resultant force:");
			Reporting.AddLine("R = H + 3 * M / L");
			Reporting.AddLine("= " + H + " + 3 * " + mom + " / " + length);
			Reporting.AddLine("= " + R + ConstUnit.Force);

			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 1.6 * E^0.5 * t² * (1 + 3 * L / h) * Fy^ 0.5");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + "* 1.6 * " + ConstNum.ELASTICITY + " * " + component.Shape.tw + "² * (1 + 3 * " + length + " / " + component.Shape.d + ") * " + component.Material.Fy + "^0.5");
			if (FiRn >= R)
				Reporting.AddLine("= " + FiRn + " >= " + R + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + R + ConstUnit.Force + " (NG)");
		}

		internal static void HSSwallShear(EMemberType memberType, ref double L)
		{
			double FiRn;
			double R;
			double Mom = 0;
			double eBottom;
			double yBottom = 0;
			double eTop = 0;
			double ortasi = 0;
			double Th = 0;
			double V;
			double H;
			double tens1;
			double comp1;

			var component = CommonDataStatic.DetailDataDict[memberType];
			var webPlate = component.WinConnect.ShearWebPlate;

			Reporting.AddHeader("HSS Wall Shear Capacity:");

			if (memberType == EMemberType.PrimaryMember || memberType == EMemberType.LeftBeam || memberType == EMemberType.RightBeam)
			{
				comp1 = Math.Max(component.WinConnect.ShearClipAngle.ForceX, component.AxialCompression);
				tens1 = Math.Max(component.WinConnect.ShearClipAngle.ForceX, component.AxialTension);
				H = Math.Max(tens1, comp1);
				V = component.WinConnect.ShearClipAngle.ForceY;

				Reporting.AddLine("Horizontal force: H = " + H + ConstUnit.Force);
				Reporting.AddLine("Vertical force: V = " + V + ConstUnit.Force);

				switch (component.GussetToColumnConnection)
				{
					case EBraceConnectionTypes.SinglePlate:
						L = webPlate.Length;
						Th = webPlate.Thickness;
						break;
					case EBraceConnectionTypes.FabricatedTee:
						L = component.BraceConnect.FabricatedTee.Length;
						Th = component.BraceConnect.FabricatedTee.Tw + 2 * component.BraceConnect.FabricatedTee.Tf;
						break;
				}
			}
			else
			{
				if (component.KBrace && (memberType == EMemberType.LowerRight || memberType == EMemberType.LowerLeft))
				{
					// if m is the component type index, this is m - 1
					var secondaryComponent = memberType == EMemberType.LowerRight ? EMemberType.UpperRight : EMemberType.UpperLeft;

					H = Math.Abs(component.BraceConnect.Gusset.Hc + CommonDataStatic.DetailDataDict[secondaryComponent].BraceConnect.Gusset.Hc);
					V = Math.Abs(-component.BraceConnect.Gusset.VerticalForceColumn + CommonDataStatic.DetailDataDict[secondaryComponent].BraceConnect.Gusset.VerticalForceColumn);
					// TODO: Find out what to do with ortasi, eTop, eBottom, etc.
					//ortasi = (BRACE1.y[29 + i1] + BRACE1.y[34 + i1]) / 2;
					//L = BRACE1.y[29 + i1] - BRACE1.y[34 + i1];
					//SmallMethodsBrace.Intersect(100, BRACE1.x[25 + i1], BRACE1.y[25 + i1], Math.Tan(BRACE1.member[m - 1].Angle * ConstNum.RADIAN), BRACE1.member[m - 1].WorkX, BRACE1.member[m - 1].WorkY, ref dumy, ref yTop);
					//eTop = yTop - ortasi;
					//SmallMethodsBrace.Intersect(100, BRACE1.x[25 + i1], BRACE1.y[25 + i1], Math.Tan(BRACE1.member[m].Angle * ConstNum.RADIAN), BRACE1.member[m].WorkX, component.WorkPointY, ref dumy, ref yBottom);
					eBottom = ortasi - yBottom;
					Mom = Math.Abs(eTop * CommonDataStatic.DetailDataDict[secondaryComponent].BraceConnect.Gusset.Hc - eBottom * component.BraceConnect.Gusset.Hc);
				}
				else
				{
					H = component.BraceConnect.Gusset.Hc;
					V = component.BraceConnect.Gusset.VerticalForceColumn;
					Mom = component.BraceConnect.Gusset.Mc;
					switch (component.GussetToColumnConnection)
					{
						case EBraceConnectionTypes.SinglePlate:
							L = webPlate.Length;
							Th = webPlate.Thickness;
							break;
						case EBraceConnectionTypes.DirectlyWelded:
							L = component.BraceConnect.Gusset.VerticalForceColumn;
							Th = component.BraceConnect.Gusset.Thickness;
							break;
						case EBraceConnectionTypes.FabricatedTee:
							L = component.BraceConnect.FabricatedTee.Length;
							Th = component.BraceConnect.FabricatedTee.Tw + 2 * component.BraceConnect.FabricatedTee.Tf;
							break;
					}
				}
				Reporting.AddLine("Horizontal force: H = " + H + ConstUnit.Force);
				Reporting.AddLine("Horizontal force: V = " + V + ConstUnit.Force);
				Reporting.AddLine("Moment: M = " + Mom + ConstUnit.MomentUnitInch);
			}

			Reporting.AddLine("Resultant force");
			R = Math.Pow(Math.Pow(H + 3 * Mom / L, 2) + Math.Pow(V, 2), 0.5);
			if (double.IsNaN(R))
				R = 0;
			Reporting.AddLine("R = ((H + 3 * M / L)² + V²)^0.5");
			Reporting.AddLine("= ((" + H + " + 3 * " + Mom + " / " + L + ")² + " + V + "²)^0.5 = " + R + ConstUnit.Force);
			FiRn = ConstNum.FIOMEGA1_0N * 0.6 * component.Material.Fy * 2 * component.Shape.tf * L;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.FIOMEGA1_0N + " * 0.6 * Fy * 2 * t * L");
			Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + component.Material.Fy + " * 2 * " + component.Shape.tf + " * " + L);
			if (FiRn >= R)
				Reporting.AddLine("= " + FiRn + " >= " + R + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + R + ConstUnit.Force + " (NG)");
		}

		internal static void HSSwallYielding(EMemberType memberType, ref double L)
		{
			double FiRn;
			double b;
			double fa;
			double Qf;
			double Moment;
			double eBottom;
			double yBottom = 0;
			double eTop = 0;
			double ortasi = 0;
			double Mom;
			double H;
			double Th = 0;
			double He = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];
			var webPlate = component.WinConnect.ShearWebPlate;

			Reporting.AddHeader("HSS Wall Flexural Yielding:");

			if (memberType == EMemberType.PrimaryMember || memberType == EMemberType.LeftBeam || memberType == EMemberType.RightBeam)
			{
				He = component.WinConnect.ShearClipAngle.ForceX;
				Reporting.AddLine("Horizontal force: H = " + He + ConstUnit.Force);
				switch (component.GussetToColumnConnection)
				{
					case EBraceConnectionTypes.SinglePlate:
						L = webPlate.Length;
						Th = webPlate.Thickness;
						break;
					case EBraceConnectionTypes.FabricatedTee:
						L = component.BraceConnect.FabricatedTee.Length;
						Th = component.BraceConnect.FabricatedTee.bf;
						break;
				}
			}
			else
			{
				H = component.BraceConnect.Gusset.Hc;
				Mom = component.BraceConnect.Gusset.Mc;

				if (component.KBrace)
				{
					if (memberType == EMemberType.LowerRight || memberType == EMemberType.LowerLeft)
					{
						// if m is the component type index, this is m - 1
						var secondaryComponent = memberType == EMemberType.LowerRight ? EMemberType.UpperRight : EMemberType.UpperLeft;

						switch (component.GussetToColumnConnection)
						{
							case EBraceConnectionTypes.SinglePlate:
								L = webPlate.Length;
								Th = webPlate.Thickness;
								break;
							case EBraceConnectionTypes.DirectlyWelded:
								L = component.BraceConnect.Gusset.VerticalForceColumn;
								Th = component.BraceConnect.Gusset.Thickness;
								break;
							case EBraceConnectionTypes.FabricatedTee:
								L = component.BraceConnect.FabricatedTee.Length;
								Th = component.BraceConnect.FabricatedTee.bf;
								break;
						}
						H = Math.Abs(component.BraceConnect.Gusset.Hc + CommonDataStatic.DetailDataDict[secondaryComponent].BraceConnect.Gusset.Hc);
						// TODO: Find out what to do with ortasi, eTop, eBottom, etc.
						//i1 = 5 * (m - 4);
						//ortasi = (BRACE1.y[29 + i1] + BRACE1.y[34 + i1]) / 2;
						//L = BRACE1.y[29 + i1] - BRACE1.y[34 + i1];
						//SmallMethodsBrace.Intersect(100, BRACE1.x[25 + i1], BRACE1.y[25 + i1], Math.Tan(CommonDataStatic.DetailDataDict[secondaryComponent].Angle * ConstNum.RADIAN), CommonDataStatic.DetailDataDict[secondaryComponent].WorkPointX, CommonDataStatic.DetailDataDict[secondaryComponent].WorkPointY, ref dumy, ref yTop);
						//eTop = yTop - ortasi;
						//SmallMethodsBrace.Intersect(100, BRACE1.x[25 + i1], BRACE1.y[25 + i1], Math.Tan(component.Angle * ConstNum.RADIAN), component.WorkPointX, component.WorkPointY, ref dumy, ref yBottom);
						eBottom = ortasi - yBottom;
						Moment = Math.Abs(eTop * CommonDataStatic.DetailDataDict[secondaryComponent].BraceConnect.Gusset.Hc - eBottom * component.BraceConnect.Gusset.Hc);
						He = Math.Abs(H) + 3 * Math.Abs(Moment) / L;
						Reporting.AddHeader("Horizontal force (He) = H + 3 * M / L");
						Reporting.AddLine("= " + H + " + 3 * " + Moment + " / " + L + " = " + He + ConstUnit.Force);
					}
				}
				else
				{
					switch (component.GussetToColumnConnection)
					{
						case EBraceConnectionTypes.SinglePlate:
							L = webPlate.Length;
							Th = webPlate.Thickness;
							break;
						case EBraceConnectionTypes.DirectlyWelded:
							L = component.BraceConnect.Gusset.VerticalForceColumn;
							Th = component.BraceConnect.Gusset.Thickness;
							break;
						case EBraceConnectionTypes.FabricatedTee:
							L = component.BraceConnect.FabricatedTee.Length;
							Th = component.BraceConnect.FabricatedTee.bf;
							break;
					}
					if (L != 0)
						He = Math.Abs(H) + 3 * Math.Abs(Mom) / L;
					Reporting.AddLine("Horizontal force (He) = H + 3 * M / L");
					Reporting.AddLine("= " + H + " + 3 * " + Mom + " / " + L + " = " + He + ConstUnit.Force);
				}
			}
			// Not sure about this one - if (BRACE1.BraceMoreData[0].Tension >= 0)
			if (CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].AxialTension >= 0)
			{
				Qf = 1;
				Reporting.AddLine("Qf = 1");
			}
			else
			{
				fa = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].AxialCompression / CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.a;
				Qf = 1 - 0.3 * fa / component.Material.Fy * (1 + fa / component.Material.Fy);
				Reporting.AddLine("Qf = 1 - 0.3 * fa / Fy * (1 + fa / Fy)");
				Reporting.AddLine("= 1 - 0.3 * " + fa + " / " + component.Material.Fy + " * (1 + " + fa + " / " + component.Material.Fy + ")");
				Reporting.AddLine("= " + Qf);
			}

			if (CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].WebOrientation == EWebOrientation.InPlane)
				b = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.bf;
			else
				b = component.Shape.d;
			FiRn = ConstNum.FIOMEGA1_0N * component.Material.Fy * Math.Pow(CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.tf, 2) / (1 - Th / b) * (2 * L / b + 4 * Math.Sqrt(1 - Th / b)) * Qf;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.FIOMEGA1_0N + " * Fy * t² / (1- t1 / B) * (2 * N / B + 4 * (1 - t1 / B)^0.5) * Qf");
			Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + " * " + component.Material.Fy + " * " + component.Shape.tf + "² / (1 - " + Th + " / " + b + ") * (2 * " + L + " / " + b + "+ 4 * (1 - " + Th + " / " + b + ")^0.5) * " + Qf);
			if (FiRn >= He)
				Reporting.AddLine("= " + FiRn + " >= " + He + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + He + ConstUnit.Force + " (NG)");
		}

		private static void ColumnWebYieldingAndCripling_Forces(EMemberType memberType, ref double HFrcMax, ref double HFrc_Comp, ref double VFrc, ref double Moment)
		{
			var component = CommonDataStatic.DetailDataDict[memberType];

			switch (memberType)
			{
				case EMemberType.RightBeam:
				case EMemberType.LeftBeam:
					HFrcMax = Math.Max(component.AxialTension, component.AxialCompression);
					HFrc_Comp = component.AxialCompression;
					VFrc = component.WinConnect.ShearClipAngle.ForceY;
					break;
				default:
					HFrcMax = Math.Max(Math.Abs(component.BraceConnect.Gusset.GussetEFTension.Hc), Math.Abs(component.BraceConnect.Gusset.GussetEFCompression.Hc));
					HFrc_Comp = Math.Abs(component.BraceConnect.Gusset.GussetEFCompression.Hc);
					VFrc = Math.Max(Math.Abs(component.BraceConnect.Gusset.GussetEFTension.Vc), Math.Abs(component.BraceConnect.Gusset.GussetEFCompression.Vc));
					Moment = Math.Max(Math.Abs(component.BraceConnect.Gusset.GussetEFTension.Mc), Math.Abs(component.BraceConnect.Gusset.GussetEFCompression.Mc));
					break;
			}
		}

		internal static void ColumnWebYieldingAndCripling(EMemberType memberType)
		{
			double Rcap1 = 0;
			double Rcap = 0;
			int k = 0;
			double thWebYieldingColumn = 0;
			double N = 0;
			double tf = 0;
			double tw = 0;
			double HFrc_Comp = 0;
			double RColumnForCripling = 0;
			double HFrcMax = 0;
			double RColumn = 0;
			double Moment = 0;
			double H = 0;
			double VFrc = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			bool componentIsBrace = MiscMethods.IsBrace(memberType);
			var webPlate = component.WinConnect.ShearWebPlate;

			if (column.WebOrientation == EWebOrientation.OutOfPlane || CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam) // if (BRACE1.member[0].orient == "Out of Plane" || BRACE1.JointType == 1)
				return;

			if (component.GussetWeldedToColumn && componentIsBrace)
			{
				if (!component.KBrace)
				{
					H = component.BraceConnect.Gusset.VerticalForceColumn;
					ColumnWebYieldingAndCripling_Forces(memberType, ref HFrcMax, ref HFrc_Comp, ref VFrc, ref Moment);
				}
			}
			else if (component.GussetToColumnConnection == EBraceConnectionTypes.ClipAngle)
			{
				H = component.WinConnect.ShearClipAngle.Length;
				ColumnWebYieldingAndCripling_Forces(memberType, ref HFrcMax, ref HFrc_Comp, ref VFrc, ref Moment);

			}
			else if (component.GussetToColumnConnection == EBraceConnectionTypes.SinglePlate)
			{
				H = webPlate.Length;
				ColumnWebYieldingAndCripling_Forces(memberType, ref HFrcMax, ref HFrc_Comp, ref VFrc, ref Moment);
			}
			else if (component.GussetToColumnConnection == EBraceConnectionTypes.FabricatedTee)
			{
				H = component.BraceConnect.FabricatedTee.Length;
				ColumnWebYieldingAndCripling_Forces(memberType, ref HFrcMax, ref HFrc_Comp, ref VFrc, ref Moment);
			}
			else if (component.GussetToColumnConnection == EBraceConnectionTypes.DirectlyWelded)
			{
				H = component.BraceConnect.Gusset.VerticalForceColumn;
				ColumnWebYieldingAndCripling_Forces(memberType, ref HFrcMax, ref HFrc_Comp, ref VFrc, ref Moment);
			}
			else if (component.GussetToColumnConnection == EBraceConnectionTypes.EndPlate)
			{
				H = component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts * component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir;
				ColumnWebYieldingAndCripling_Forces(memberType, ref HFrcMax, ref HFrc_Comp, ref VFrc, ref Moment);

			}
			if (!componentIsBrace)
			{
				RColumn = Math.Sqrt(Math.Pow(HFrcMax, 2) + Math.Pow(1.73 * VFrc, 2));
				RColumnForCripling = HFrc_Comp;
			}
			else
			{
				RColumn = Math.Sqrt(Math.Pow(HFrcMax + 3 * Moment / H, 2) + Math.Pow(1.73 * VFrc, 2));
				RColumnForCripling = HFrc_Comp + 3 * Moment / H;
			}

			if (double.IsNaN(RColumn))
				RColumn = 0;
			if (double.IsNaN(RColumnForCripling))
				RColumnForCripling = 0;

			tw = column.Shape.tw;
			tf = column.Shape.tf;
			N = H / column.Shape.d;
			thWebYieldingColumn = RColumn / (ConstNum.FIOMEGA1_0N * column.Material.Fy * (H + 5 * column.Shape.kdes));

			Reporting.AddHeader("Column Web Local Yielding:");
			if (!componentIsBrace)
			{
				Reporting.AddLine("Force from Beam (RColumn) = (H² + (1.73 * V)²)^0.5");
				Reporting.AddLine(String.Format("{0} {1} = ({2}² + (1.73 * {3})²)^0.5)", RColumn, ConstUnit.Force, HFrcMax, VFrc));
			}
			else
			{
				Reporting.AddLine("Force from Gusset (RColumn) = ((H + 3 * M / N)² + (1.73 * V)²)^0.5");
				Reporting.AddLine(String.Format("{0} {1} = (({2} + 3 * {3} / {4})² + (1.73 * {5})²)^0.5",
					RColumn, ConstUnit.Force, HFrcMax, Moment, H, VFrc));
			}
			Reporting.AddLine("Required Web Thickness = RColumn / (" + ConstString.FIOMEGA1_0 + " * Fy * (N + 5 * k))");
			Reporting.AddLine(String.Format("{0} = {1} / {2} * {3} * ({4} + 5 * {5}))",
				thWebYieldingColumn, RColumn, ConstNum.FIOMEGA1_0N, column.Material.Fy, H, column.Shape.kdes));
			if (thWebYieldingColumn <= tw)
				Reporting.AddLine(String.Format("Required Web Thickness {0} <= tw {1} (OK)", thWebYieldingColumn, tw));
			else
				Reporting.AddLine(String.Format("Required Web Thickness {0} <= tw {1} (NG)", thWebYieldingColumn, tw));

			RColumn = RColumnForCripling;

			Reporting.AddHeader("Column Web Crippling:");
			if (memberType == EMemberType.RightBeam || memberType == EMemberType.LeftBeam)
				Reporting.AddLine(String.Format("Force from Beam (RColumn) = {0} {1}", RColumn, ConstUnit.Force));
			else
			{
				Reporting.AddLine("Force from Gusset (RColumn) = H + 3 * M / N");
				Reporting.AddLine(String.Format("{0} = {1} + 3 * {2} / {3}", RColumn, HFrcMax, Moment, H));
			}

			k = 0;
			do
			{
				k++;

				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase)
				{
					if (H / column.Shape.d <= 0.2)
						Rcap = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(ConstNum.ELASTICITY, 0.5) * Math.Pow(tw, 2) * (1 + 3 * N * Math.Pow(tw / tf, 1.5)) * Math.Pow(column.Material.Fy * tf / tw, 0.5);
					else
						Rcap = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(ConstNum.ELASTICITY, 0.5) * Math.Pow(tw, 2) * (1 + 4 * N * Math.Pow(tw / tf, 1.5)) * Math.Pow(column.Material.Fy * tf / tw, 0.5);
				}
				else
					Rcap = ConstNum.FIOMEGA0_75N * 0.8 * Math.Pow(ConstNum.ELASTICITY, 0.5) * Math.Pow(tw, 2) * (1 + 3 * N * Math.Pow(tw / tf, 1.5)) * Math.Pow(column.Material.Fy * tf / tw, 0.5);
				if (k == 1)
					Rcap1 = Rcap;
				if (Rcap < RColumn)
					tw += ConstNum.SIXTEENTH_INCH;
			} while (Rcap < RColumn);

			if (Rcap1 >= RColumn)
				Reporting.AddLine("Rcap " + Rcap1 + " >= RColumn " + RColumn + " " +  ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("Rcap " + Rcap1 + " >= RColumn " + RColumn + " " + ConstUnit.Force + " (NG)");

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase)
			{
				if (H / column.Shape.d <= 0.2)
				{
					Reporting.AddLine("Rcap = " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstString.YOUNGS_MODULUS_E + "^0.5 * tw² * (1 + 3 * N * (tw / tf)^1.5) * (Fy * tf / tw)^0.5");
					Reporting.AddLine(Rcap + " = " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstNum.ELASTICITY + "^0.5 * " + tw + "² * (1 + 3 * " + N + " * (" + tw + " / " + tf + ")^1.5 * (" + column.Material.Fy + " * " + tf + " / " + tw + ")^0.5");
				}
				else
				{
					Reporting.AddLine("Rcap = " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstString.YOUNGS_MODULUS_E + "^0.5 * tw² * (1 + 4 * N * (tw / tf)^1.5) * (Fy * tf / tw)^0.5");
					Reporting.AddLine(Rcap + " = " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstNum.ELASTICITY + "^0.5 * " + tw + "² * (1 + 4 * " + N + " * (" + tw + " / " + tf + ")^1.5 * (" + column.Material.Fy + " * " + tf + " / " + tw + ")^0.5");

				}
			}
			else
			{
				Reporting.AddLine("Rcap = " + ConstString.FIOMEGA0_75 + " * 0.8 * " + ConstString.YOUNGS_MODULUS_E + "^0.5 * tw² * (1 + 3 * N * (tw / tf)^1.5) * (Fy * tf / tw)^0.5");
				Reporting.AddLine(Rcap + " = " + ConstString.FIOMEGA0_75 + " * 0.8 * " + ConstNum.ELASTICITY + "^0.5 * " + tw + "² * (1 + 3 * " + N + " * (" + tw + " / " + tf + ")^1.5 * (" + column.Material.Fy + " * " + tf + " / " + tw + ")^0.5");

			}
			if (tw > column.Shape.tw)
				Reporting.AddLine("Required Thickness = " + tw + ConstUnit.Length);
		}

		internal static void ColumnFlangeBending(EMemberType memberType, Bolt bolt)
		{
			double thShear;
			double NeededBolts;
			double thBending;
			double AlfaP;
			double Beta;
			double delta;
			double P;
			double ro;
			double ap;
			double bp;
			double a = 0;
			double b;
			double HoleSize = 0;
			double TperBolt;
			double Ball;
			double t;
			double N;
			double V;
			double Moment = 0;
			double Th = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			switch (memberType)
			{
				case EMemberType.RightBeam:
				case EMemberType.LeftBeam:
					Th = component.Shape.tw;
					Moment = 0;
					break;
				case EMemberType.UpperRight:
				case EMemberType.LowerRight:
				case EMemberType.UpperLeft:
				case EMemberType.LowerLeft:
					Th = component.BraceConnect.Gusset.Thickness;
					Moment = Math.Abs(component.BraceConnect.Gusset.Mc);
					break;
			}

			V = component.WinConnect.ShearClipAngle.ForceY / 2;
			N = bolt.NumberOfBolts;
			t = (component.AxialTension + 3 * Moment / ((N + 1) * bolt.SpacingLongDir)) / 2;

			if (t == 0)
				return;

			Reporting.AddHeader("Column Flange Bending:");
			t = (component.AxialTension + 3 * Moment / ((N + 1) * bolt.SpacingLongDir)) / 2;
			Ball = BoltsForTension.CalcBoltsForTension(memberType, bolt, t, V, N, true);
			TperBolt = t / N;

			Reporting.AddLine("Force (H') = (H + 3 * M / ((N + 1) * sl) / 2");
			Reporting.AddLine(t + " = (" + component.AxialTension + " + 3 * " + Moment + " / ((" + N + " + 1) * " + bolt.SpacingLongDir + ") / 2");
			Reporting.AddLine("Force per Bolt (T) = H' / N");
			Reporting.AddLine(TperBolt + " = " + t + " / " + N);

			b = (CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].GageOnFlange - column.Shape.tw) / 2;

			switch (component.GussetToColumnConnection)
			{
				case EBraceConnectionTypes.ClipAngle:
					HoleSize = component.WinConnect.ShearClipAngle.BoltOnColumn.HoleDiameterSTD;
					a = Math.Min(1.25 * b, Math.Min((column.Shape.bf - CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].GageOnFlange) / 2, (2 * component.WinConnect.ShearClipAngle.LengthOfOSL + Th - CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].GageOnFlange) / 2));
					break;
				case EBraceConnectionTypes.EndPlate:
					HoleSize = component.WinConnect.ShearEndPlate.Bolt.HoleDiameterSTD;
					a = Math.Min(1.25 * b, Math.Min((column.Shape.bf - CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].GageOnFlange) / 2, (component.WinConnect.ShearEndPlate.Width - CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].GageOnFlange) / 2));
					break;
			}

			bp = (b - bolt.BoltSize / 2);
			ap = (a + bolt.BoltSize / 2);

			ro = bp / ap;
			P = bolt.SpacingLongDir;
			delta = 1 - HoleSize / P;
			Beta = (Ball / TperBolt - 1) / ro;

			if (Beta >= 1)
			{
				AlfaP = 1;
				Reporting.AddLine("Alpha' = 1");
			}
			else
			{
				AlfaP = Math.Min(1, Beta / (delta * (1 - Beta)));
				Reporting.AddHeader(String.Format("Alpha' = Min(1, Beta / (delta * (1 - Beta)) = {0}", AlfaP));
			}

			thBending = Math.Sqrt(4 / ConstNum.FIOMEGA0_9N * TperBolt * bp / (P * column.Material.Fy * (1 + delta * AlfaP)));

			Reporting.AddLine(String.Format("b = {0} {5}, a = {1} {5}, b' = {2} {5}, a' = {3} {5}, ro = {4} {5}",
				b, a, bp, ap, ro, ConstUnit.Length));
			Reporting.AddLine(String.Format("p = {0}, d' = {1}, delta = 1 - d' / p = 1 - {1} / {0}", P, HoleSize));
			Reporting.AddLine(String.Format("Beta = (B / T - 1) / ro = ({0} / {1} - 1) / {2}", Ball, TperBolt, ro));
			Reporting.AddHeader("Required Flange Thickness for Bending (treq'd)");
			Reporting.AddLine("4 / " + ConstString.FIOMEGA0_9 + " * T * b' / (p * Fy * (1 + delta * Alpha')))^0.5");
			Reporting.AddLine(String.Format("{0} = (4 / {1} * {2} * {3} / ({4} * {5} * (1 + {6} * {7})))^0.5",
				thBending, ConstString.FIOMEGA0_9, TperBolt, bp, P, column.Material.Fy, delta, AlfaP));

			if (thBending <= column.Shape.tf)
				Reporting.AddLine(String.Format("treq'd {0} <= tf {1} (OK)", thBending, column.Shape.tf));
			else
			{
				Reporting.AddLine(String.Format("treq'd {0} >> tf {1} (NG)", thBending, column.Shape.tf));
				switch (component.GussetToColumnConnection)
				{
					case EBraceConnectionTypes.ClipAngle:
					case EBraceConnectionTypes.EndPlate:
						NeededBolts = Math.Ceiling(bolt.NumberOfBolts * thBending / column.Shape.tf);
						Reporting.AddLine(String.Format("Increasing the number of bolt rows to {0} or decreasing the bolt gage on column might help.", NeededBolts));
						break;
				}
			}

			thShear = TperBolt / Math.Min(ConstNum.FIOMEGA1_0N * 0.6 * P * column.Material.Fy, ConstNum.FIOMEGA0_75N * 0.6 * (P - (HoleSize + ConstNum.SIXTEENTH_INCH)) * column.Material.Fu);

			Reporting.AddHeader("Column Flange Shear - Required Flange Thickness for Shear");
			Reporting.AddLine(String.Format("thShear = T / Min( " + ConstNum.FIOMEGA1_0N + " * 0.6 * p * Fy, " + ConstString.FIOMEGA0_75 + " * 0.6 * (p = (d' + {0}))) * Fu", ConstNum.SIXTEENTH_INCH));
			Reporting.AddLine(String.Format("{0} = {1} / Min({2} * 0.6 * {3} * {4}, {5} * 0.6 * ({6} = ({7} + {8}))) * {9}",
				thShear, TperBolt, ConstNum.FIOMEGA1_0N, P, column.Material.Fy, ConstString.FIOMEGA0_75, P, HoleSize, ConstNum.SIXTEENTH_INCH, column.Material.Fu));
			if (thShear <= column.Shape.tf)
				Reporting.AddLine(String.Format("thShear {0} <= tf {1} (OK)", thShear, column.Shape.tf));
			else
				Reporting.AddLine(String.Format("thShear {0} >> tf {1} (NG)", thShear, column.Shape.tf));
		}

		internal static void ColumnWebBending(EMemberType memberType)
		{
			CWBending.CalcCWBending(memberType);
			// There was a lot of code here in Descon 7, but is now overwritten by the above method
		}

		internal static void BoltBearingOnColumn(EMemberType memberType)
		{
			double Pcap;
			double tcolumn;
			double Fbs;
			double p1;
			double P;

			DetailData component = CommonDataStatic.DetailDataDict[memberType];
			DetailData otherSide;

			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			if (component.WinConnect.ShearClipAngle.Position == EPosition.NoConnection) // if (component.WinConnect.ShearClipAngle.position == 4)
				return;

			switch (memberType)
			{
				case EMemberType.RightBeam:
					otherSide = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
					break;
				case EMemberType.LeftBeam:
					otherSide = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
					break;
				case EMemberType.UpperRight:
					otherSide = CommonDataStatic.DetailDataDict[EMemberType.UpperLeft];
					break;
				case EMemberType.LowerRight:
					otherSide = CommonDataStatic.DetailDataDict[EMemberType.LowerLeft];
					break;
				case EMemberType.UpperLeft:
					otherSide = CommonDataStatic.DetailDataDict[EMemberType.UpperRight];
					break;
				case EMemberType.LowerLeft:
					otherSide = CommonDataStatic.DetailDataDict[EMemberType.LowerRight];
					break;
				default:
					otherSide = column;
					break;
			}

			if (otherSide.ShapeName == ConstString.NONE)
				otherSide = column;

			Reporting.AddHeader("Bolt Bearing on Column:");

			P = Math.Abs(component.WinConnect.ShearClipAngle.ForceY / (2 * component.BoltBrace.NumberOfBolts));
			p1 = P;

			Fbs = CommonCalculations.SpacingBearing(component.BoltBrace.SpacingLongDir, component.BoltBrace.HoleDiameterSTD, component.BoltBrace.BoltSize, component.BoltBrace.HoleType, component.Material.Fu, true);

			if (column.WebOrientation == EWebOrientation.InPlane)
				tcolumn = column.Shape.tf;
			else
			{
				if (otherSide != column)
					P = p1 + Math.Abs(otherSide.WinConnect.ShearClipAngle.ForceY / (2 * otherSide.BoltBrace.NumberOfBolts)); // P = p1 + Math.Abs(BRACE1.ClipForceY[motherside] / (2 * BRACE1.Bolts[motherside].N));
				tcolumn = column.Shape.tw;
			}
			Pcap = Fbs * tcolumn;
			if (Pcap >= P)
				Reporting.AddLine(" >= " + P + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine(" << " + P + ConstUnit.Force + " (NG)");

			Reporting.AddLine("Vertical force per bolt = " + p1 + ConstUnit.Force);
			Reporting.AddLine("Bearing Strength = Fbs * tc = " + Fbs + " * " + tcolumn);
			Reporting.AddLine("= " + Pcap + ConstUnit.Force);
		}

		internal static void BeamWebYieldingAndCripling(EMemberType memberType)
		{
			string OKbotCrip = string.Empty;
			string OKtopCrip = string.Empty;
			double RcapBot = 0;
			double RcapTop = 0;
			double eLeft = 0;
			double xLeft2 = 0;
			double eRight = 0;
			double xRight2 = 0;
			double ortasi = 0;
			int k = 0;
			double Nbot = 0;
			double Ntop = 0;
			double tf = 0;
			double tw = 0;
			double thWebYieldingBot = 0;
			double Rbot = 0;
			double thWebYieldingTop = 0;
			double Rtop = 0;
			double Mi2Max = 0;
			double Hi2Max = 0;
			double Vi2Comp = 0;
			double Vi2Max = 0;
			double Mi1Max = 0;
			double Hi1Max = 0;
			double Vi1Comp = 0;
			double Vi1Max = 0;
			double GapB = 0;
			double GapT = 0;
			double H = 0;
			double R = 0;
			double L = 0;
			double V = 0;
			double Moment;
			double Rcap;

			Reporting.AddLine("Beam Web Local Yielding");
			DetailData beam;
			DetailData upperBrace;
			DetailData lowerBrace;

			// Set the components to be used as the beam and braces for the rest of the calcs.
			if (memberType == EMemberType.LowerLeft || memberType == EMemberType.UpperLeft)
			{
				beam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
				upperBrace = CommonDataStatic.DetailDataDict[EMemberType.UpperLeft];
				lowerBrace = CommonDataStatic.DetailDataDict[EMemberType.LowerLeft];
			}
			else
			{
				beam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
				upperBrace = CommonDataStatic.DetailDataDict[EMemberType.UpperRight];
				lowerBrace = CommonDataStatic.DetailDataDict[EMemberType.LowerRight];
			}

			// Eq. K1-1 and K1-2 for Yielding and K1-4 or K1-5 for Crippling
			if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BraceVToBeam)
			{
				// Not V-Brace
				if (upperBrace.BraceConnect.Gusset.ColumnSideSetback > beam.EndSetback + beam.WinConnect.Beam.TCopeL)
					GapT = 0;
				else
				{
					GapT = beam.EndSetback + beam.WinConnect.Beam.TCopeL -
					       upperBrace.BraceConnect.Gusset.ColumnSideSetback;
				}
				if (lowerBrace.BraceConnect.Gusset.ColumnSideSetback > beam.EndSetback + beam.WinConnect.Beam.TCopeL)
					GapB = 0;
				else
					GapT = beam.EndSetback + beam.WinConnect.Beam.TCopeL - lowerBrace.BraceConnect.Gusset.ColumnSideSetback;
				Vi1Max = Math.Max(upperBrace.BraceConnect.Gusset.GussetEFTension.Vb, upperBrace.BraceConnect.Gusset.GussetEFCompression.Vb);
				Vi1Comp = upperBrace.BraceConnect.Gusset.GussetEFCompression.Vb;
				Hi1Max = Math.Max(upperBrace.BraceConnect.Gusset.GussetEFTension.Hb, upperBrace.BraceConnect.Gusset.GussetEFCompression.Hb);
				Mi1Max = Math.Max(upperBrace.BraceConnect.Gusset.GussetEFTension.Mb, upperBrace.BraceConnect.Gusset.GussetEFCompression.Mb);

				Vi2Max = Math.Max(lowerBrace.BraceConnect.Gusset.GussetEFTension.Vb, lowerBrace.BraceConnect.Gusset.GussetEFCompression.Vb);
				Vi2Comp = lowerBrace.BraceConnect.Gusset.GussetEFCompression.Vb;
				Hi2Max = Math.Max(lowerBrace.BraceConnect.Gusset.GussetEFTension.Hb, lowerBrace.BraceConnect.Gusset.GussetEFCompression.Hb);
				Mi2Max = Math.Max(lowerBrace.BraceConnect.Gusset.GussetEFTension.Mb, lowerBrace.BraceConnect.Gusset.GussetEFCompression.Mb);

				if (upperBrace.ShapeName != ConstString.NONE)
				{
					Rtop = Math.Sqrt(Math.Pow(1.73 * Hi1Max, 2) + Math.Pow(Math.Abs(Vi1Max) + 3 * Math.Abs(Mi1Max) / (upperBrace.BraceConnect.Gusset.Hb - GapT), 2));
					thWebYieldingTop = Rtop / (ConstNum.FIOMEGA1_0N * beam.Material.Fy * (upperBrace.BraceConnect.Gusset.Hb - GapT + 2.5 * beam.Shape.kdes));
					if (double.IsNaN(Rtop))
						Rtop = 0;
					if (double.IsNaN(thWebYieldingTop))
						thWebYieldingTop = 0;

					Reporting.AddLine("Force from Top, Rtop = ((1.73 * HbTop)² + (VbTop + 3 * MbTop / Ltop)²)^0.5");
					Reporting.AddLine(Rtop + "= ((1.73 * " + Hi1Max + ")² + (" + Vi2Max + " + 3 * " + Mi2Max + " / " + (upperBrace.BraceConnect.Gusset.Hb - GapT) + ")²)^0.5");

					Reporting.AddLine("Required Web Thickness = Rtop / (" + ConstString.FIOMEGA1_0 + " * Fy * (L + 2.5 * k))");
					Reporting.AddLine(thWebYieldingTop + " " + ConstUnit.Length + " = " + Rtop + " / (" + ConstNum.FIOMEGA1_0N + " * " + beam.Material.Fy + " * (" + (upperBrace.BraceConnect.Gusset.Hb - GapT) + " + 2.5 * " + beam.Shape.kdes + "))");
				}
				else
				{
					Rtop = 0;
					thWebYieldingTop = 0;
				}
				if (lowerBrace.ShapeName != ConstString.NONE)
				{
					Rbot = Math.Sqrt(Math.Pow(1.73 * Hi2Max, 2) + Math.Pow(Math.Abs(Vi2Max) + 3 * Math.Abs(Mi2Max) / (lowerBrace.BraceConnect.Gusset.Hb - GapB), 2));
					thWebYieldingBot = Rbot / (ConstNum.FIOMEGA1_0N * beam.Material.Fy * (lowerBrace.BraceConnect.Gusset.Hb - GapB + 2.5 * beam.Shape.kdes));
					if (double.IsNaN(Rbot))
						Rbot = 0;
					if (double.IsNaN(thWebYieldingBot))
						thWebYieldingBot = 0;

					Reporting.AddLine("Force from Bottom, Rtop = ((1.73 * HbTop)² + (VbTop + 3 * MbTop / Ltop)²)^0.5");
					Reporting.AddLine(Rbot + " = ((1.73 * " + Hi1Max + ")² + (" + Vi2Max + " + 3 * " + Mi2Max + " / " + (lowerBrace.BraceConnect.Gusset.Hb - GapB) + ")²)^0.5");
					Reporting.AddLine("Required Web Thickness = Rtop / (Fy * (L + 2.5 * k))");
					Reporting.AddLine(thWebYieldingBot + " " + ConstUnit.Length + " = " + Rbot + " / (" + beam.Material.Fy + " * (" + (lowerBrace.BraceConnect.Gusset.Hb - GapB) + " + 2.5 * " + beam.Shape.kdes + "))");
				}
				else
				{
					Rbot = 0;
					thWebYieldingBot = 0;
				}
				if (thWebYieldingTop <= beam.Shape.tw)
					Reporting.AddLine("Web Yielding Top " + thWebYieldingTop + " <= " + beam.Shape.tw + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Web Yielding Top " + thWebYieldingTop + " >> " + beam.Shape.tw + ConstUnit.Length + " (NG)");
				if (thWebYieldingBot <= beam.Shape.tw)
					Reporting.AddLine("Web Yielding Bottom " + thWebYieldingBot + " <= " + beam.Shape.tw + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Web Yielding Bottom " + thWebYieldingBot + " >> " + beam.Shape.tw + ConstUnit.Length + " (NG)");

				// Web crippling
				if (upperBrace.ShapeName != ConstString.NONE)
					Rtop = Math.Abs(Vi1Comp) + 3 * Math.Abs(Mi1Max / (upperBrace.BraceConnect.Gusset.Hb - GapT));
				else
					Rtop = 0;
				if (lowerBrace.ShapeName != ConstString.NONE)
					Rbot = Math.Abs(Vi2Comp) + 3 * Math.Abs(Mi2Max / (lowerBrace.BraceConnect.Gusset.Hb - GapB));
				else
					Rbot = 0;

				if (double.IsNaN(Rtop))
					Rtop = 0;
				if (double.IsNaN(Rbot))
					Rbot = 0;

				tw = beam.Shape.tw;
				tf = beam.Shape.tf;
				Ntop = (upperBrace.BraceConnect.Gusset.Hb - GapT) / beam.Shape.d;
				Nbot = (lowerBrace.BraceConnect.Gusset.Hb - GapB) / beam.Shape.d;
				k = 0;
				do
				{
					k++;
					if (Ntop <= 0.2)
						RcapTop = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(ConstNum.ELASTICITY, 0.5) * Math.Pow(tw, 2) * (1 + 3 * Ntop * Math.Pow(tw / tf, 1.5)) * Math.Sqrt(beam.Material.Fy * tf / tw);
					else
						RcapTop = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(ConstNum.ELASTICITY, 0.5) * Math.Pow(tw, 2) * (1 + (4 * Ntop - 0.2) * Math.Pow(tw / tf, 1.5)) * Math.Sqrt(beam.Material.Fy * tf / tw);

					if (Nbot <= 0.2)
						RcapBot = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(ConstNum.ELASTICITY, 0.5) * Math.Pow(tw, 2) * (1 + 3 * Nbot * Math.Pow(tw / tf, 1.5)) * Math.Sqrt(beam.Material.Fy * tf / tw);
					else
						RcapBot = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(ConstNum.ELASTICITY, 0.5) * Math.Pow(tw, 2) * (1 + (4 * Nbot - 0.2) * Math.Pow(tw / tf, 1.5)) * Math.Sqrt(beam.Material.Fy * tf / tw);

					if (double.IsNaN(RcapTop))
						RcapTop = 0;
					if (double.IsNaN(RcapBot))
						RcapBot = 0;

					if (k == 1)
					{
						if (RcapTop >= Rtop)
							OKtopCrip = "Rcap Top" + RcapTop + " >= " + Rtop + ConstUnit.Force + " (OK)";
						else
							OKtopCrip = "Rcap Top" + RcapTop + " << " + Rtop + ConstUnit.Force + " (NG)";
						if (RcapBot >= Rbot)
							OKbotCrip = "Rcap Bottom" + RcapBot + " >= " + Rbot + ConstUnit.Force + " (OK)";
						else
							OKbotCrip = "Rcap Bottom" + RcapBot + " << " + Rbot + ConstUnit.Force + " (NG)";

						Reporting.AddHeader("Beam Web Crippling:");
						if (beam.Shape.Name != ConstString.NONE)
						{
							Reporting.AddLine("Force from Top, Rtop = VbTop + 3 * MbTop / Ltop");
							Reporting.AddLine("= " + Math.Abs(Vi1Max) + " + 3 *" + Math.Abs(Mi1Max) + " / " + (upperBrace.BraceConnect.Gusset.Hb - GapT));
							Reporting.AddLine("= " + Rtop + ConstUnit.Force);
							Reporting.AddLine("for Top Loading, FiRn:");
							if (Ntop <= 0.2)
							{
								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstNum.ELASTICITY + "^0.5 * tw² * (1 + 3 * (Ntop / d) * (tw / tf)^1.5) * (Fy * tf / tw)^0.5");
								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstNum.ELASTICITY + " * " + tw + "² * (1 + 3 * (" + (upperBrace.BraceConnect.Gusset.Hb - GapT) + " / " + beam.Shape.d + ")");
								Reporting.AddLine(" * (" + tw + " / " + tf + ")^1.5) * (" + beam.Material.Fy + " * " + tf + " / " + tw + ")^0.5");
							}
							else
							{
								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstNum.ELASTICITY + "^0.5 * tw² * (1 + (4 * (Ntop / d)-0.2) * (tw / tf)^1.5) * (Fy * tf / tw)^0.5");
								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstNum.ELASTICITY + " * " + tw + "² * (1 + (4 * (" + (upperBrace.BraceConnect.Gusset.Hb - GapT) + " / " + beam.Shape.d + ")-0.2)");
								Reporting.AddLine(" * (" + tw + " / " + tf + ")^1.5) * (" + beam.Material.Fy + " * " + tf + " / " + tw + ")^0.5");
							}
							Reporting.AddLine("= " + RcapTop + ConstUnit.Force + OKtopCrip);
						}
						if (lowerBrace.Shape.Name != ConstString.NONE)
						{
							Reporting.AddHeader("Force from Bottom, Rbot = VbBot + 3 * MbBot / LBot");
							Reporting.AddLine("= " + Math.Abs(Vi2Max) + " + 3 * " + Math.Abs(Mi2Max) + " / " + (lowerBrace.BraceConnect.Gusset.Hb - GapB));
							Reporting.AddLine("= " + Rbot + ConstUnit.Force);
							Reporting.AddLine("For Bottom Loading, FiRn:");
							if (Nbot <= 0.2)
							{
								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstNum.ELASTICITY + "^0.5 * tw² * (1 + 3 * (Nbot / d) * (tw / tf)^1.5) * (Fy * tf / tw)^0.5");
								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstNum.ELASTICITY + " * " + tw + "² * (1 + 3 * (" + (lowerBrace.BraceConnect.Gusset.Hb - GapB) + " / " + beam.Shape.d + ")");
								Reporting.AddLine("* (" + tw + " / " + tf + ")^1.5) * (" + beam.Material.Fy + " * " + tf + " / " + tw + ")^0.5");
							}
							else
							{
								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstNum.ELASTICITY + "^0.5 * tw² * (1 + (4 * (Nbot / d) - 0.2) * (tw / tf)^1.5) * (Fy * tf / tw)^0.5");
								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstNum.ELASTICITY + " * " + tw + "² * (1 + (4 * (" + (lowerBrace.BraceConnect.Gusset.Hb - GapB) + " / " + beam.Shape.d + ") - 0.2)");
								Reporting.AddLine(" * (" + tw + " / " + tf + ")^1.5) * (" + beam.Material.Fy + " * " + tf + " / " + tw + ")^0.5");
							}
							Reporting.AddLine("= " + RcapBot + ConstUnit.Force + OKbotCrip);
						}
					}

					if (RcapTop < Rtop || RcapBot < Rbot)
						tw += ConstNum.SIXTEENTH_INCH;
					if (k > 1000) // TODO: Something is really wrong with this. It never exits even if the other conditions are met
						break;
				} while ((RcapTop < Rtop && upperBrace.IsActive) || (RcapBot < Rbot && lowerBrace.IsActive));
			}

				//TODO: I'll get to this later. We need to change the parameter to EMemberType to fit V-Brace config.
			else
			{
				// V-Brace
				H = Math.Abs(-beam.BraceConnect.Gusset.Hb + upperBrace.BraceConnect.Gusset.Hb);
				V = Math.Abs(beam.BraceConnect.Gusset.VerticalForceBeam + upperBrace.BraceConnect.Gusset.VerticalForceBeam);
				Moment = Math.Abs(beam.BraceConnect.Gusset.VerticalForceBeam * upperBrace.BraceConnect.Gusset.VerticalForceBeam);

				//ortasi = (BRACE1.x[26 + i1] + BRACE1.x[36 + i1]) / 2;
				//L = BRACE1.x[26 + i1] - BRACE1.x[36 + i1];
				//SmallMethodsDesign.Intersect(0, BRACE1.x[25 + i1].ToDbl(), BRACE1.y[25 + i1].ToDbl(), Math.Tan(beam.Angle * BRACE1.radian), beam.WorkPointX, beam.WorkPointY, ref xRight2, ref dumy);
				//eRight = xRight2 - ortasi;
				//SmallMethodsDesign.Intersect(0, BRACE1.x[25 + i1].ToDbl(), BRACE1.y[25 + i1].ToDbl(), Math.Tan(component.Angle * BRACE1.radian), component.WorkPointX, component.WorkPointY, ref xLeft2, ref dumy);
				eRight = xRight2 - ortasi;
				eLeft = xLeft2 - ortasi;
				R = Math.Sqrt(Math.Pow(1.73 * H, 2) + Math.Pow(V + 3 * Moment / L, 2));
				thWebYieldingTop = R / (ConstNum.FIOMEGA1_0N * beam.Material.Fy * L);

				Reporting.AddHeader("Beam Local Stresses");
				Reporting.AddLine("Beam Web Local Yielding:");
				Reporting.AddLine("Force from Gusset, R = ((1.73 * H)² + (V + 3 * M / L)²)^0.5");
				Reporting.AddLine("= ((1.73 * " + H + ")² + (" + V + " + 3 * " + Moment + " / " + L + ")²)^0.5");
				Reporting.AddLine("= " + R + ConstUnit.Force);

				Reporting.AddLine("Required Web Thickness = R / (" + ConstNum.FIOMEGA1_0N + " * Fy * L)");
				Reporting.AddLine("= " + R + " / (" + ConstNum.FIOMEGA1_0N + " * " + beam.Material.Fy + " * " + L + ")");
				if (thWebYieldingTop >= beam.Shape.tw)
					Reporting.AddLine("= " + thWebYieldingTop + " >= " + beam.Shape.tw + ConstUnit.Length + " (NG)");
				else
					Reporting.AddLine("= " + thWebYieldingTop + " << " + beam.Shape.tw + ConstUnit.Length + " (OK)");

				R = V + 3 * Moment / L;
				tw = beam.Shape.tw;
				tf = beam.Shape.tf;
				Rcap = ConstNum.FIOMEGA0_75N * 0.8 * Math.Pow(ConstNum.ELASTICITY, 0.5) * Math.Pow(tw, 2) * (1 + 3 * (L / beam.Shape.d) * Math.Pow(tw / tf, 1.5)) * Math.Sqrt(beam.Material.Fy * tf / tw);

				Reporting.AddLine("Beam Web Crippling:");
				Reporting.AddLine("Force from Gusset (R) = V + 3 * M / L");
				Reporting.AddLine("= " + V + " + 3 * " + Moment + " / " + L);
				Reporting.AddLine("= " + R + ConstUnit.Force);
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.8 * " + ConstNum.ELASTICITY + "^0.5 * tw² *(1 + 3 * (L / d) *(tw / tf)^1.5) * (Fy * tf / tw)^0.5");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.8 * " + ConstNum.ELASTICITY + " * " + tw + "² * (1 + 3 * (" + L + " / " + beam.Shape.d + ") * (" + tw + " / " + tf + ")^1.5) * (" + beam.Material.Fy + " * " + tf + " / " + tw + ")^0.5");

				if (Rcap >= R)
					Reporting.AddLine("= " + Rcap + " >= " + R + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + Rcap + " << " + R + ConstUnit.Force + " (NG)");
			}
		}

		internal static void ColumnAndBeamCheck(EMemberType memberType, Bolt bolt)
		{
			double b;

			var component = CommonDataStatic.DetailDataDict[memberType];
			var webPlate = component.WinConnect.ShearWebPlate;
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			Reporting.AddGoToHeader("Column and Beam Check");
			switch (memberType)
			{
				case EMemberType.RightBeam:
				case EMemberType.LeftBeam:
					Reporting.AddLine(String.Format("Beam and Column Local Stresses for {0}", CommonDataStatic.CommonLists.CompleteMemberList[memberType]));
					break;
				default:
					if (component.WinConnect.ShearClipAngle.Position != EPosition.NoConnection)
					{
						if (component.KBrace)
						{
							switch (memberType)
							{
								case EMemberType.UpperRight:
								case EMemberType.LowerRight:
									Reporting.AddLine("Column Local Stresses for right hand side connection");
									break;
								case EMemberType.UpperLeft:
								case EMemberType.LowerLeft:
									Reporting.AddLine("Column Local Stresses for left hand side connection");
									break;
							}
						}
						else
							Reporting.AddLine(String.Format("Column Local Stresses for {0}", CommonDataStatic.CommonLists.CompleteMemberList[memberType]));
						if (component.KBrace && (memberType == EMemberType.UpperRight || memberType == EMemberType.UpperLeft))
						{
							Reporting.AddLine("(See Lower Brace Calculation.)");
							return;
						}
					}
					break;
			}

			if (component.ShapeName != ConstString.NONE)
				BeamWebYieldingAndCripling(memberType);
			switch (column.ShapeType)
			{
				case EShapeType.WideFlange:
					if (!(component.GussetWeldedToColumn || component.GussetToColumnConnection == EBraceConnectionTypes.SinglePlate))
						BoltBearingOnColumn(memberType);
					if (column.WebOrientation == EWebOrientation.InPlane && !(component.GussetWeldedToColumn || component.GussetToColumnConnection == EBraceConnectionTypes.SinglePlate))
						ColumnFlangeBending(memberType, bolt);
					if (column.WebOrientation == EWebOrientation.InPlane)
						ColumnWebYieldingAndCripling(memberType);
					else
						ColumnWebBending(memberType);
					break;
				case EShapeType.HollowSteelSection:
					if (component.GussetWeldedToColumn || component.GussetToColumnConnection == EBraceConnectionTypes.SinglePlate)
					{
						double length = webPlate.Length;
						HSSwallShear(memberType, ref length);
						HSSPunchingShear(memberType, ref length);
						HSSwallYielding(memberType, ref length);
						webPlate.Length = length;
					}
					else if (component.GussetWeldedToColumn || component.GussetToColumnConnection == EBraceConnectionTypes.FabricatedTee)
					{
						HSSSideWallYielding(memberType);
						HSSSideWallCripling(memberType);
						if (column.WebOrientation == EWebOrientation.InPlane)
							b = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.bf;
						else
							b = component.Shape.d;
						if (b > component.BraceConnect.FabricatedTee.bf)
						{
							double length = component.BraceConnect.FabricatedTee.Length;
							HSSwallYielding(memberType, ref length);
							component.BraceConnect.FabricatedTee.Length = length;
						}
					}
					else if (component.GussetToColumnConnection == EBraceConnectionTypes.EndPlate)
					{
						HSSSideWallYielding(memberType);
						HSSSideWallCripling(memberType);
					}

					if (column.ShapeType == EShapeType.WideFlange)
					{
						if (column.WebOrientation == EWebOrientation.InPlane)
							ColumnWebYieldingAndCripling(memberType);
						else
							ColumnWebBending(memberType);
					}
					break;
			}
		}

		internal static void HSSPunchingShear(EMemberType memberType, ref double L)
		{
			double ftp = 0;
			double Mom = 0;
			double eBottom = 0;
			double eTop = 0;
			double Th = 0;
			double H = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];
			var webPlate = component.WinConnect.ShearWebPlate;

			Reporting.AddHeader("HSS Wall Punching Shear:");

			if (memberType == EMemberType.PrimaryMember || memberType == EMemberType.RightBeam || memberType == EMemberType.LeftBeam)
			{
				H = component.WinConnect.ShearClipAngle.ForceX;
				Reporting.AddLine("Horizontal force: H = " + H + ConstUnit.Force);
				switch (component.GussetToColumnConnection)
				{
					case EBraceConnectionTypes.SinglePlate:
						L = webPlate.Length;
						Th = webPlate.Thickness;
						break;
					case EBraceConnectionTypes.FabricatedTee:
						L = component.BraceConnect.FabricatedTee.Length;
						Th = component.BraceConnect.FabricatedTee.Tw + 2 * component.BraceConnect.FabricatedTee.Tf;
						break;
				}
			}
			else
			{
				if (component.KBrace && (memberType == EMemberType.LowerRight || memberType == EMemberType.LowerLeft))
				{
					// if m is the component type index, this is m - 1
					var secondaryComponent = memberType == EMemberType.LowerRight ? EMemberType.UpperRight : EMemberType.UpperLeft;

					H = Math.Abs(component.BraceConnect.Gusset.Hc + CommonDataStatic.DetailDataDict[secondaryComponent].BraceConnect.Gusset.Hc);
					// TODO: Find out what to do with ortasi, eTop, eBottom, etc.
					//i1 = 5 * (m - 4);
					//ortasi = (BRACE1.y[29 + i1] + BRACE1.y[34 + i1]) / 2;
					//L = BRACE1.y[29 + i1] - BRACE1.y[34 + i1];
					//SmallMethodsBrace.Intersect(100, BRACE1.x[25 + i1], BRACE1.y[25 + i1], Math.Tan(CommonDataStatic.DetailDataDict[secondaryComponent].Angle * ConstNum.RADIAN), CommonDataStatic.DetailDataDict[secondaryComponent].WorkPointX, CommonDataStatic.DetailDataDict[secondaryComponent].WorkPointY, ref dumy, ref yTop);
					//eTop = yTop - ortasi;
					//SmallMethodsBrace.Intersect(100, BRACE1.x[25 + i1], BRACE1.y[25 + i1], Math.Tan(component.Angle * ConstNum.RADIAN), component.WorkPointX, component.WorkPointY, ref dumy, ref yBottom);
					//eBottom = ortasi - yBottom;
					Mom = Math.Abs(eTop * CommonDataStatic.DetailDataDict[secondaryComponent].BraceConnect.Gusset.Hc - eBottom * component.BraceConnect.Gusset.Hc);
				}
				else
				{
					H = component.BraceConnect.Gusset.Hc;
					Mom = component.BraceConnect.Gusset.Mc;
					switch (component.GussetToColumnConnection)
					{
						case EBraceConnectionTypes.SinglePlate:
							L = webPlate.Length;
							Th = webPlate.Thickness;
							break;
						case EBraceConnectionTypes.DirectlyWelded:
							L = component.BraceConnect.Gusset.VerticalForceColumn;
							Th = component.BraceConnect.Gusset.Thickness;
							break;
						case EBraceConnectionTypes.FabricatedTee:
							L = component.BraceConnect.FabricatedTee.Length;
							Th = component.BraceConnect.FabricatedTee.Tw + 2 * component.BraceConnect.FabricatedTee.Tf;
							break;
					}
				}
				Reporting.AddLine("Horizontal force: H = " + H + ConstUnit.Force);
				Reporting.AddLine("Moment: M = " + Mom + ConstUnit.MomentUnitInch);
			}
			Reporting.AddHeader("Maximum force/Length:");
			ftp = (H + 6 * Mom / L) / L;
			if (double.IsNaN(ftp))
				ftp = 0;
			Reporting.AddLine("ftp = (H + 6 * M / L) / L");
			Reporting.AddLine("= (" + H + " + 6 * " + Mom + " / " + L + ") / " + L + " = " + ftp + ConstUnit.ForcePerUnitLength);

			//Fut = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Material.Fu * CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.tf;
			//Reporting.AddLine("Fut = Fu * t");
			//Reporting.AddLine("= " + CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Material.Fu + " * " + component.Shape.tf);
			//if (Fut >= ftp)
			//	Reporting.AddLine("= " + Fut + " >= " + ftp + ConstUnit.ForcePerUnitLength + " (OK)");
			//else
			//	Reporting.AddLine("= " + Fut + " << " + ftp + ConstUnit.ForcePerUnitLength + " (NG)");
		}

		private static double LateralPlateBucklingAllowable(double c, double ho, double Fy, double tw, bool enableReporting)
		{
			double tmpCaseArg = 0;
			double FiFcr = 0;
			double Q = 0;
			double Lamda = 0;
			double k = 0;

			tmpCaseArg = 2 * c / ho;
			if (tmpCaseArg <= 0.25)
				k = 16;
			else if (tmpCaseArg <= 0.3)
				k = 13;
			else if (tmpCaseArg <= 0.4)
				k = 10;
			else if (tmpCaseArg <= 0.5)
				k = 6;
			else if (tmpCaseArg <= 0.6)
				k = 4.5;
			else if (tmpCaseArg <= 0.75)
				k = 2.5;
			else if (tmpCaseArg <= 1)
				k = 1.3;
			else if (tmpCaseArg <= 1.5)
				k = 0.8;
			else if (tmpCaseArg <= 2)
				k = 0.6;
			else if (tmpCaseArg <= 3)
				k = 0.5;
			else
				k = 0.425;

			Lamda = Math.Pow(Fy / k, 0.5) * ho / (0.98 * ConstNum.ELASTICITY * 2 * tw);

			if (Lamda <= 0.7)
				Q = 1;
			else if (Lamda <= 1.41)
				Q = 1.34 - 0.486 * Lamda;
			else
				Q = 1.3 / Math.Pow(Lamda, 2);
			FiFcr = ConstNum.FIOMEGA0_9N * Fy * Q;

			if (enableReporting)
			{
				Reporting.AddHeader("Bending Stress for Lateral Buckling:");
				Reporting.AddLine("c = " + c + ConstUnit.Length + ", ho = L = " + ho + ConstUnit.Length + ", 2 * c / ho = " + 2 * c / ho + ",   K = " + k);
				Reporting.AddLine("m = (Fy / K)^0.5 * ho / (0.98 * E^0.5 * 2 * tp)");
				Reporting.AddLine("= (" + Fy + " / " + k + ")^0.5 * " + ho + " / (0.98 * " + ConstNum.ELASTICITY + " * 2 * " + tw + ")");
				Reporting.AddLine("= " + Lamda);
				if (Lamda <= 0.7)
					Reporting.AddLine("Q = 1");
				else if (Lamda <= 1.41)
					Reporting.AddLine("Q =  1.34 - 0.486 * m = 1.34 - 0.486 * " + Lamda + " = " + Q);
				else
					Reporting.AddLine("Q =  1.3 / (m²) = 1.3 / (" + Lamda + ")² = " + Q);
				Reporting.AddLine("FiFcr = " + ConstString.FIOMEGA0_9 + " * Fy * Q  = " + ConstString.FIOMEGA0_9 + " * " + Fy + " * " + Q);
			}
			return FiFcr;
		}

		internal static void SinglePlateBuckling(EMemberType memberType, double H, double t, double Mom, double R)
		{
			double Fcr = 0;
			double Lc = 0;
			double t0 = 0;
			double UtilizationFactor = 0;
			double Pn = 0;
			double KLoR = 0;
			double Lcr = 0;
			double Mn = 0;
			double Mu = 0;
			double Pu = 0;
			double Ag = 0;
			double Lb = 0;
			double MaxStress = 0;
			double f_b2 = 0;
			double f_b1 = 0;
			double fa_Weak = 0;
			double An = 0;
			double SecMod = 0;
			double MomInert = 0;
			double ex = 0;
			double FiRcr = 0;
			double tl = 0;
			double cx = 0;
			double E = 0;
			double S = 0;
			double N = 0;
			double d = 0;
			double L = 0;
			double pFy = 0;
			double Plength = 0;
			double MLh = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];
			var webPlate = component.WinConnect.ShearWebPlate;

			switch (component.GussetToColumnConnection)
			{
				case EBraceConnectionTypes.SinglePlate:
					MLh = webPlate.BraceBoltToVertEdgeDistGussetBeam;
					Plength = webPlate.Length;
					pFy = webPlate.Material.Fy;
					L = Plength;
					d = webPlate.Bolt.HoleWidth;
					N = webPlate.Bolt.NumberOfRows;
					S = webPlate.Bolt.SpacingLongDir;
					E = webPlate.Bolt.EdgeDistLongDir;
					break;
				case EBraceConnectionTypes.FabricatedTee:
					MLh = component.BraceConnect.FabricatedTee.Eh2;
					Plength = component.BraceConnect.FabricatedTee.Length;
					pFy = component.BraceConnect.FabricatedTee.Material.Fy;
					L = Plength;
					d = component.BraceConnect.FabricatedTee.Bolt.HoleWidth;
					N = component.BraceConnect.FabricatedTee.Bolt.NumberOfRows;
					S = component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir;
					E = component.BraceConnect.FabricatedTee.Ev1;
					break;
			}
			if (memberType == EMemberType.PrimaryMember || memberType == EMemberType.RightBeam || memberType == EMemberType.LeftBeam)
				cx = MLh + component.EndSetback;
			else
				cx = MLh + component.BraceConnect.Gusset.ColumnSideSetback;

			do
			{
				tl = t;
				FiRcr = LateralPlateBucklingAllowable(((int)cx), Plength, pFy, tl, false);
				if (memberType == EMemberType.PrimaryMember || memberType == EMemberType.RightBeam || memberType == EMemberType.LeftBeam)
					ex = (t + component.Shape.tw) / 2;
				else
					ex = (t + component.BraceConnect.Gusset.Thickness) / 2;

				MomInert = ((Math.Pow(L, 3) - N * Math.Pow(d, 3)) / 12 - d * N * (Math.Pow(L / 2 - E, 2) - (L / 2 - E) * S * (N - 1) + Math.Pow(S, 2) * (N - 1) * (2 * N - 1) / 6)) * t;
				SecMod = 2 * MomInert / L;
				An = (Plength - N * (d + ConstNum.SIXTEENTH_INCH)) * t;

				fa_Weak = H / An;
				f_b1 = 6 * (H * ex / 2) / (t * An);
				f_b2 = (R * cx + Mom) / SecMod;
				MaxStress = fa_Weak + f_b1 + f_b2;
				if (MaxStress > FiRcr)
					t = t + ConstNum.SIXTEENTH_INCH;
				do
				{
					if (memberType == EMemberType.PrimaryMember || memberType == EMemberType.RightBeam || memberType == EMemberType.LeftBeam)
					{
						Lb = MLh + component.EndSetback;
						ex = (t + component.Shape.tw) / 2;
					}
					else
					{
						Lb = MLh + component.BraceConnect.Gusset.ColumnSideSetback;
						ex = (t + component.BraceConnect.Gusset.Thickness) / 2;
					}

					Ag = Plength * t;
					Pu = H;
					Mu = Pu * ex / 2;
					Mn = pFy * Plength * Math.Pow(t, 2) / 4;

					Lcr = 1.2 * Lb;
					KLoR = 3.464 * Lcr / t;
					Pn = Ag * CommonCalculations.BucklingStress(KLoR, component.BraceConnect.Gusset.Material.Fy, false);
					if (Pu / (ConstNum.FIOMEGA0_9N * Pn) >= 0.2)
						UtilizationFactor = Pu / (ConstNum.FIOMEGA0_9N * Pn) + 8 / 9.0 * Mu / (ConstNum.FIOMEGA0_9N * Mn);
					else
						UtilizationFactor = Pu / (2 * ConstNum.FIOMEGA0_9N * Pn) + Mu / (ConstNum.FIOMEGA0_9N * Mn);

					if (double.IsNaN(UtilizationFactor))
						UtilizationFactor = 0;

					t0 = t;
					if (UtilizationFactor > 1)
						t += ConstNum.SIXTEENTH_INCH;
				} while (t0 < t);
			} while (tl < t);

			if (memberType == EMemberType.PrimaryMember || memberType == EMemberType.RightBeam || memberType == EMemberType.LeftBeam)
				cx = MLh + component.BraceConnect.Gusset.ColumnSideSetback;
			else
				cx = MLh + component.EndSetback;

			if (memberType == EMemberType.PrimaryMember || memberType == EMemberType.RightBeam || memberType == EMemberType.LeftBeam)
				ex = (t + component.Shape.tw) / 2;
			else
				ex = (t + component.BraceConnect.Gusset.Thickness) / 2;

			fa_Weak = H / An;
			f_b1 = 6 * (H * ex / 2) / (t * An);
			f_b2 = (R * cx + Mom) / SecMod;

			Reporting.AddHeader("Single Plate Buckling");
			MaxStress = H / An + 6 * (H * ex / 2) / (t * An) + (R * cx + Mom) / SecMod;
			if (double.IsNaN(MaxStress))
				MaxStress = 0;
			Reporting.AddHeader("Maximum Stress:");
			Reporting.AddLine("= H / An + 6 * (H * ex / 2) / (t * An) + (V * cx + Mom) / S");
			Reporting.AddLine("= " + H + " / " + An + " + 6 * (" + H + " * " + ex + " / 2) / (" + t + " * " + An + ") + (" + R + " * " + cx + " + " + Mom + ") / " + SecMod);
			Reporting.AddLine("= " + MaxStress + ConstUnit.Stress);

			FiRcr = LateralPlateBucklingAllowable(cx, Plength, pFy, t, true);
			if (FiRcr >= MaxStress)
				Reporting.AddLine("= " + FiRcr + " >= " + MaxStress + " ksi (OK)");
			else
				Reporting.AddLine("= " + FiRcr + " << " + MaxStress + " ksi (NG)");

			Reporting.AddHeader("Compression Buckling of Plate:");
			Reporting.AddLine("Using K = 1.2 and L = " + Lb + ConstUnit.Length);
			Reporting.AddLine("r = t / (12^0.5) = " + t + " / 3.464 = " + (t / 3.464) + ConstUnit.Length);
			Reporting.AddLine("KL / r = " + KLoR);

			Lc = KLoR * Math.Pow(webPlate.Material.Fy / ConstNum.ELASTICITY, 0.5) / Math.PI;

			Reporting.AddLine("Lc = KL / r * (Fy / E)^0.5 / PI");
			Reporting.AddLine("= " + KLoR + " * (" + pFy + " / " + ConstNum.ELASTICITY + ")^0.5 / " + Math.PI);
			Reporting.AddLine("= " + Lc);
			if (Lc <= 1.5)
			{
				Fcr = Math.Pow(0.658, Math.Pow(Lc, 2)) * pFy;
				Reporting.AddLine("Fcr = 0.658^(Lc²) * Fy");
				Reporting.AddLine("= 0.658^" + Math.Pow(Lc, 2) + " * " + pFy + " = " + Fcr + ConstUnit.Stress);
			}
			else
			{
				Fcr = 0.877 * pFy / Math.Pow(Lc, 2);
				Reporting.AddLine("Fcr = 0.877 * Fy / (Lc²)");
				Reporting.AddLine("= 0.877 * " + pFy + " / (" + Lc + "²) = " + Fcr + ConstUnit.Stress);
			}

			Reporting.AddLine("Pn = Lp * t * Fcr = " + Plength + " * " + t + " * " + Fcr + " = " + Pn + ConstUnit.Force);
			Reporting.AddLine("Mu = Pu * e / 2 = " + Pu + " * " + ex + " / 2 = " + Mu + ConstUnit.MomentUnitInch);
			Reporting.AddLine("Mn = Fy * Lp * t² / 4 = " + pFy + " * " + Plength + " * " + t + "² / 4 = " + Mn + ConstUnit.MomentUnitInch);
			Reporting.AddHeader("Utilization Factor:");
			if (Pu / (ConstNum.FIOMEGA0_9N * Pn) >= 0.2)
			{
				Reporting.AddLine("Pu / (" + ConstString.FIOMEGA0_9 + " * Pn) >= 0.2");
				Reporting.AddLine("Pu / (" + ConstString.FIOMEGA0_9 + " * Pn) + 8 / 9 * Mu / (" + ConstString.FIOMEGA0_9 + " * Mn)");
				Reporting.AddLine("= " + Pu + " / (" + ConstString.FIOMEGA0_9 + " * " + Pn + ") + 8 / 9 * " + Mu + " / (" + ConstString.FIOMEGA0_9 + " * " + Mn + ")");
			}
			else
			{
				Reporting.AddLine("Pu / (" + ConstString.FIOMEGA0_9 + " * Pn) << 0.2");
				Reporting.AddLine("Pu / (2 * " + ConstString.FIOMEGA0_9 + " * Pn) + Mu / (" + ConstString.FIOMEGA0_9 + " * Mn)");
				Reporting.AddLine("= " + Pu + " / (2 * " + ConstString.FIOMEGA0_9 + " * " + Pn + ") + " + Mu + " / (" + ConstString.FIOMEGA0_9 + " * " + Mn + ")");
			}
			if (UtilizationFactor <= 1)
				Reporting.AddLine("= " + UtilizationFactor + " <= 1.0 (OK)");
			else
				Reporting.AddLine("= " + UtilizationFactor + " >> 1.0 (NG)");
		}

		internal static void GussetandBeamCheckwithSinglePlate(EMemberType memberType, double V, double H_Comp, double H_Tens, double R, double EquivBoltFactor, EBearingTearOut bearingorTearOut)
		{
			double GrossTensionandShearCapacity = 0;
			double NetTensionandShearCapacity = 0;
			double An2 = 0;
			double Ag2 = 0;
			double An = 0;
			double Ag = 0;
			double b = 0;
			double a = 0;
			double bearingCapacityHComp = 0;
			double bearingCapacityHTens = 0;
			double bearingCapacityV = 0;
			double Fbe = 0;
			double Fbs = 0;
			double Lv = 0;
			double Lh = 0;
			double Th = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];
			var webPlate = component.WinConnect.ShearWebPlate;

			if (memberType == EMemberType.PrimaryMember || memberType == EMemberType.RightBeam || memberType == EMemberType.LeftBeam)
			{
				Th = component.Shape.tw;
				Lh = webPlate.BraceBoltToVertEdgeDistGussetBeam;
			}
			else
			{
				Th = component.BraceConnect.Gusset.Thickness;
				Lh = webPlate.BraceBoltToVertEdgeDistGussetBeam;
				Lv = component.BraceConnect.Gusset.VerticalForceColumn - (webPlate.BraceDistanceToFirstBolt + webPlate.Bolt.SpacingLongDir * (webPlate.Bolt.NumberOfRows - 1));
			}
			switch (bearingorTearOut)
			{
				case EBearingTearOut.Bearing:
					if (memberType == EMemberType.PrimaryMember || memberType == EMemberType.RightBeam || memberType == EMemberType.LeftBeam)
						Reporting.AddHeader("Bolt Bearing on Beam Web");
					else
						Reporting.AddHeader("Bolt Bearing on Gusset");

					Reporting.AddHeader("Vertical Load:");
					if (memberType == EMemberType.PrimaryMember || memberType == EMemberType.RightBeam || memberType == EMemberType.LeftBeam)
					{
						Fbs = CommonCalculations.SpacingBearing(webPlate.Bolt.SpacingLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Bolt.HoleType, component.Material.Fu, false);
						bearingCapacityV = EquivBoltFactor * webPlate.Bolt.NumberOfLines * Fbs * webPlate.Bolt.NumberOfRows * Th;
						Reporting.AddLine(ConstString.PHI + " Rn = ef * Nh * Fbs * Nl * t");
						Reporting.AddLine("= " + EquivBoltFactor + " * " + webPlate.Bolt.NumberOfLines + " * " + Fbs + " * " + webPlate.Bolt.NumberOfRows + " * " + Th);
					}
					else
					{
						Fbs = CommonCalculations.SpacingBearing(webPlate.Bolt.SpacingLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Bolt.HoleType, component.Material.Fu, false);
						Fbe = CommonCalculations.EdgeBearing(Lv, (webPlate.Bolt.HoleWidth), webPlate.Bolt.BoltSize, component.Material.Fu, webPlate.Bolt.HoleType, false);
						bearingCapacityV = EquivBoltFactor * webPlate.Bolt.NumberOfLines * (Fbe + Fbs * (webPlate.Bolt.NumberOfRows - 1)) * Th;
						Reporting.AddLine(ConstString.PHI + " Rn = ef * Nh * (Fbe + Fbs * (Nl - 1)) * t");
						Reporting.AddLine("= " + EquivBoltFactor + " * " + webPlate.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + webPlate.Bolt.NumberOfRows + " - 1)) * " + Th);
					}
					if (bearingCapacityV >= V)
						Reporting.AddLine("= " + bearingCapacityV + " >= " + V + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + bearingCapacityV + " << " + V + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Horizontal Load:");
					Fbs = CommonCalculations.SpacingBearing(webPlate.Bolt.SpacingTransvDir, webPlate.Bolt.HoleLength, webPlate.Bolt.BoltSize, webPlate.Bolt.HoleType, component.Material.Fu, false);
					Fbe = CommonCalculations.EdgeBearing(Lh, (webPlate.Bolt.HoleLength), webPlate.Bolt.BoltSize, component.Material.Fu, webPlate.Bolt.HoleType, false);

					Reporting.AddHeader("With Tensile Force");
					bearingCapacityHTens = EquivBoltFactor * webPlate.Bolt.NumberOfRows * (Fbe + Fbs * (webPlate.Bolt.NumberOfLines - 1)) * Th;
					Reporting.AddLine(ConstString.PHI + " Rn = ef * Nl * (Fbe + Fbs * (Nh - 1)) * t");
					Reporting.AddLine("= " + EquivBoltFactor + " * " + webPlate.Bolt.NumberOfRows + " * (" + Fbe + " + " + Fbs + " * (" + webPlate.Bolt.NumberOfLines + " - 1)) * " + Th);
					if (bearingCapacityHTens >= H_Tens)
						Reporting.AddLine("= " + bearingCapacityHTens + " >= " + H_Tens + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + bearingCapacityHTens + " << " + H_Tens + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("With Compressive Force");
					bearingCapacityHComp = EquivBoltFactor * webPlate.Bolt.NumberOfRows * Fbs * Fbe * webPlate.Bolt.NumberOfLines * Th;
					Reporting.AddLine(ConstString.PHI + " Rn = ef * Nl *  Fbs * Nh  * t");
					Reporting.AddLine("= " + EquivBoltFactor + " * " + webPlate.Bolt.NumberOfRows + " * " + Fbe + " * " + Fbs + " * " + webPlate.Bolt.NumberOfLines + " * " + Th);
					if (bearingCapacityHComp >= H_Comp)
						Reporting.AddLine("= " + bearingCapacityHComp + " >= " + H_Comp + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + bearingCapacityHComp + " << " + H_Comp + ConstUnit.Force + " (NG)");
					break;
				case EBearingTearOut.TearOut:
					if (memberType == EMemberType.PrimaryMember || memberType == EMemberType.RightBeam || memberType == EMemberType.LeftBeam)
						Reporting.AddHeader("Beam Web Tear-out:");
					else
						Reporting.AddHeader("Gusset Tear-out:");

					// Combined Tension and Shear Rupture
					Reporting.AddHeader("Combined Tension and Shear:");
					if (R != 0)
					{
						a = H_Tens / R; //a = H / R; <- this was the original code, but H was never set
						b = V / R;
					}

					if (V == 0)
						Reporting.AddHeader("Load Angle, " + ConstString.PHI + " = Atn(H / V) = 90 Degees");
					else
						Reporting.AddHeader("Load Angle, " + ConstString.PHI + " = Atn(H / V) = " + Math.Atan(Math.Abs(H_Tens / V) / ConstNum.RADIAN) + " Degees");
					Reporting.AddLine("A = Sin(" + ConstString.PHI + ") = " + a);
					Reporting.AddLine("B = Cos(" + ConstString.PHI + ") = " + b);

					NetAndGrossArea(memberType, 0, ref Ag, ref An, ref Ag2, ref An2);
					Reporting.AddHeader("Rupture:");
					if (b > 0 && a > 0)
					{
						NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.18 * (a / b) * (-1 + Math.Pow(1 + Math.Pow(b / a, 2) / 0.09, 0.5)) * An * component.Material.Fu / b;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.18 * (A / B) * (-1 + (1 + (B / A)² / 0.09)^0.5) * An * Fu / B");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.18 * (" + a + " / " + b + ") * (-1 + (1 + (" + b + " / " + a + ")² / 0.09)^0.5) * " + An + " * " + component.Material.Fu + " / " + b);
					}
					else if (b == 0)
					{
						NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * An * component.Material.Fu;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * An * Fu = " + ConstString.FIOMEGA0_75 + " * " + An + " * " + component.Material.Fu);
					}
					else if (a == 0)
					{
						NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.6 * An * component.Material.Fu;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.6 * An * Fu = " + ConstString.FIOMEGA0_75 + " * 0.6 * " + An + " * " + component.Material.Fu);
					}
					if (NetTensionandShearCapacity >= R)
						Reporting.AddLine("= " + NetTensionandShearCapacity + " >= " + R + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + NetTensionandShearCapacity + " << " + R + ConstUnit.Force + " (NG)");

					// Combined Tension and Shear Yielding
					Reporting.AddHeader("Yielding:");
					if (b > 0 && a > 0)
					{
						GrossTensionandShearCapacity = Math.Pow(ConstNum.FIOMEGA1_0N, 2) * 0.18 * Math.Pow(a / b, 2) * (-1 / ConstNum.FIOMEGA0_9N + Math.Pow(1 / Math.Pow(ConstNum.FIOMEGA0_9N, 2) + Math.Pow(b / a, 2) / (0.09 * Math.Pow(ConstNum.FIOMEGA1_0N, 2)), 0.5)) * Ag * component.Material.Fy / a;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.FIOMEGA1_0N + "² * 0.18 * (A / B)² * (-1 / " + ConstString.FIOMEGA0_9 + " + (1 / (" + ConstString.FIOMEGA0_9 + "²) + (B / A)² / (0.09 * " + ConstNum.FIOMEGA1_0N + "²))^0.5) * Ag * Fy/A");
						Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + "² * 0.18 * (" + a + " / " + b + ")² * (-1 / " + ConstString.FIOMEGA0_9 + " + (1 / (" + ConstString.FIOMEGA0_9 + "²) + (" + b + " / " + a + ")² / (0.09 * " + ConstNum.FIOMEGA1_0N + "²))^0.5) * " + Ag + " * " + component.Material.Fy + " / " + a);
					}
					else if (b == 0)
					{
						GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * Ag * component.Material.Fy;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Ag * Fy = " + ConstString.FIOMEGA0_9 + " * " + Ag + " * " + component.Material.Fy);
					}
					else if (a == 0)
					{
						GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * 0.6 * Ag * component.Material.Fy;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * 0.6 * Ag * Fy = " + ConstString.FIOMEGA0_9 + " * 0.6 * " + Ag + " * " + component.Material.Fy);
					}
					if (GrossTensionandShearCapacity >= R)
						Reporting.AddLine("= " + GrossTensionandShearCapacity + " >= " + R + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + GrossTensionandShearCapacity + " << " + R + ConstUnit.Force + " (NG)");
					break;
			}
		}

		internal static double EccIncLoadOnBG(int Nvl, int Nbw, double ex, double Theta, double sv, double sh)
		{
			double EccIncLoadOnBG = 0;

			double toler = 0;
			double PreviousMoment = 0;
			double Cb = 0;
			double delta = 0;
			double SumRf = 0;
			double sumfy = 0;
			double sumfx = 0;
			double DeltaMaxRatio = 0;
			double Rmax = 0;
			double y0 = 0;
			double x0 = 0;
			double Rpr = 0;
			double Moment = 0;
			double increment = 0;
			double ro = 0;
			double E = 0;
			double yG = 0;
			double xG = 0;
			int n = 0;
			double S = 0;
			double c = 0;
			double DeltaMax = 0;

			double[] Xb = new double[101];
			double[] Yb = new double[101];
			double[] Fxb = new double[101];
			double[] Fyb = new double[101];
			double[] Fb = new double[101];
			double[] Rb = new double[101];

			DeltaMax = 0.34 * ConstNum.ONE_INCH;
			c = Math.Cos(Theta);
			S = Math.Sin(Theta);

			n = 0;
			for (int i = 1; i <= Nvl; i++)
			{
				for (int j = 1; j <= Nbw; j++)
				{
					n++;
					Xb[n] = (i - 1) * sh;
					Yb[n] = (j - 1) * sv;
				}
			}

			//  Center of Gravity xG, yG
			xG = 0;
			yG = 0;
			for (int k = 1; k <= n; k++)
			{
				xG = xG + Xb[k];
				yG = yG + Yb[k];
			}
			xG = xG / n;
			yG = yG / n;
			for (int k = 1; k <= n; k++)
			{
				Xb[k] = Xb[k] - xG;
				Yb[k] = Yb[k] - yG;
			}
			xG = 0;
			yG = 0;
			E = ex * c;
			ro = -9.9 * ConstNum.ONE_INCH;
			increment = 10 * ConstNum.ONE_INCH;
			if (CommonDataStatic.Units == EUnit.Metric)
				Moment = 113000;
			else
				Moment = 1;
			do
			{
				ro = ro + increment;
				Rpr = ro;
				x0 = -ro * c;
				y0 = ro * S + yG;
				Rmax = 0;
				for (int k = 1; k <= n; k++)
				{
					Rb[k] = Math.Sqrt(Math.Pow(Xb[k] - x0, 2) + Math.Pow(Yb[k] - y0, 2));
					if (Rmax < Rb[k])
						Rmax = Rb[k];
				}
				DeltaMaxRatio = DeltaMax / Rmax;
				sumfx = 0;
				sumfy = 0;
				SumRf = 0;
				for (int k = 1; k <= n; k++)
				{
					delta = DeltaMaxRatio * Rb[k];
					Fb[k] = Math.Pow(1 - Math.Exp(-10 * delta), 0.55);
					Fxb[k] = Fb[k] / Rb[k] * Math.Abs(Yb[k] - y0) * Math.Sign((y0 - Yb[k]));
					Fyb[k] = Fb[k] / Rb[k] * Math.Abs(Xb[k] - x0) * Math.Sign((Xb[k] - x0));
					sumfx = sumfx + Fxb[k];
					sumfy = sumfy + Fyb[k];
					SumRf = SumRf + Rb[k] * Fb[k];
				}
				Cb = Math.Sqrt(Math.Pow(sumfx, 2) + Math.Pow(sumfy, 2));
				PreviousMoment = Moment;
				Moment = SumRf - Cb * (E + ro);
				if (Moment * PreviousMoment < 0)
				{
					ro = Rpr - increment;
					increment = increment / 2;
					Moment = PreviousMoment;
				}
				else if (Moment == 0)
					break;

				if (CommonDataStatic.Units == EUnit.US)
					toler = 0.00001;
				else
					toler = 1;
			} while (Math.Abs(Moment) > toler && increment > 0.00000001 * ConstNum.ONE_INCH);
			EccIncLoadOnBG = Cb;

			return EccIncLoadOnBG;
		}

		internal static double HangerAllowable(EMemberType memberType, double t, ref double a, double b, double ball, bool enableReporting)
		{
			double HangerAllowable = 0;

			double tall;
			double alfaP;
			double ro;
			double delta;
			double Tc;
			double ap;
			double bp;
			double P = 0;
			double Fua = 0;
			double holeSize = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];
			Bolt bolts = component.GussetToColumnConnection == EBraceConnectionTypes.ClipAngle
				? component.WinConnect.ShearClipAngle.BoltOnColumn
				: component.WinConnect.ShearEndPlate.Bolt;

			if (bolts.NumberOfBolts == 0)
				return 0;

			switch (component.GussetToColumnConnection)
			{
				case EBraceConnectionTypes.ClipAngle:
					holeSize = component.WinConnect.ShearClipAngle.BoltOnColumn.HoleLength;
					Fua = component.WinConnect.ShearClipAngle.Material.Fu;
					P = component.WinConnect.ShearClipAngle.Length / bolts.NumberOfBolts;
					break;
				case EBraceConnectionTypes.EndPlate:
					holeSize = component.WinConnect.ShearEndPlate.Bolt.HoleLength;
					Fua = component.WinConnect.ShearEndPlate.Material.Fu;
					P = component.WinConnect.ShearEndPlate.Length / bolts.NumberOfBolts;
					break;
			}

			if (a > 1.25 * b)
				a = 1.25 * b;

			bp = (b - bolts.BoltSize / 2);
			ap = a + bolts.BoltSize / 2;

			Tc = Math.Sqrt(4 / ConstNum.FIOMEGA0_9N * ball * bp / (P * Fua));
			delta = 1 - holeSize / P;
			ro = bp / ap;
			alfaP = (Math.Pow(Tc / t, 2) - 1) / (delta * (1 + ro));
			if (alfaP > 1)
				tall = ball * Math.Pow(t / Tc, 2) * (1 + delta);
			else if (alfaP < 0)
				tall = ball;
			else
				tall = ball * Math.Pow(t / Tc, 2) * (1 + delta * alfaP);

			HangerAllowable = tall;

			if (enableReporting)
			{
				Reporting.AddHeader("Allowable tensile load on " + CommonDataStatic.CommonLists.CompleteMemberList[memberType] + " per tributary area for each bolt:");
				Reporting.AddLine("dh = " + holeSize + ConstUnit.Length + "    b' = " + bp + ConstUnit.Length);
				Reporting.AddLine("a = Min(e, 1.25 * b)= " + a + ConstUnit.Length + " a' = " + ap + ConstUnit.Length + " p = " + P + ConstUnit.Length);
				Reporting.AddLine("tc = (4 / " + ConstString.FIOMEGA0_9 + " * B * b' / (p * Fu))^0.5 ");
				Reporting.AddLine("= (4 / " + ConstString.FIOMEGA0_9 + " * " + ball + " * " + bp + " / (" + P + " * " + Fua + "))^0.5 ");
				Reporting.AddLine("= " + Tc + ConstUnit.Length);
				Reporting.AddLine("delta = 1 - dh / p ");
				Reporting.AddLine("= 1 - " + holeSize + " / " + P + " = " + delta);
				Reporting.AddLine("ro = b' / a' ");
				Reporting.AddLine("= " + bp + " / " + ap + "= " + ro);
				Reporting.AddLine("Alpha' = ((tc / t) ^ 2 - 1) / (delta * (1 + ro))");
				Reporting.AddLine("= ((" + Tc + " / " + t + ")² - 1) / (" + delta + " * (1 + " + ro + ")) ");
				Reporting.AddLine("= " + alfaP);

				if (alfaP > 1)
				{
					Reporting.AddLine("Ta = B * (t / tc)² * (1 + delta)");
					Reporting.AddLine("= " + ball + " * (" + t + " / " + Tc + ")² * (1 + " + delta + ") ");
					Reporting.AddLine("= " + tall + ConstUnit.Force);
				}
				else if (alfaP < 0)
					Reporting.AddLine("Ta = B = " + ball + ConstUnit.Force);
				else
				{
					Reporting.AddLine("Ta = B * (t / tc)² * (1 + delta * Alpha')");
					Reporting.AddLine("= " + ball + " * (" + t + " / " + Tc + ")² * (1 + " + delta + " * " + alfaP + ") ");
					Reporting.AddLine("= " + tall + ConstUnit.Force);
				}
			}
			return HangerAllowable;
		}

		internal static double AngleBeamSideGage(EMemberType memberType, EBoltedLeg boltedLeg, double d, double tw, ref double columnGage)
		{
			double angleBeamSideGage = 0;
			double gc = 0;
			double dd = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];
			dd = d;

			switch (boltedLeg)
			{
				case EBoltedLeg.Both:
					angleBeamSideGage = component.BraceConnect.Gusset.Thickness + 2 * dd + ConstNum.ONE_INCH + ConstNum.QUARTER_INCH;
					gc = tw + Math.Max(2 * (component.BraceConnect.Gusset.Thickness + 2 * dd + ConstNum.ONE_INCH + ConstNum.QUARTER_INCH), 2 * (component.WinConnect.ShearClipAngle.Fillet + dd));
					break;
				case EBoltedLeg.Support:
					angleBeamSideGage = 0;
					gc = tw + 2 * (component.WinConnect.ShearClipAngle.Fillet + dd);
					break;
				case EBoltedLeg.Beam:
					angleBeamSideGage = component.WinConnect.ShearClipAngle.Fillet + dd;
					gc = 0;
					break;
			}

			columnGage = gc;
			return angleBeamSideGage;
		}

		internal static bool ClipAnglesGussetSide(EMemberType memberType, bool enableReporting)
		{
			double RequiredThickness = 0;
			double Lh = 0;
			double FirstGage = 0;
			double ItemWidth = 0;
			double HoleHoriz = 0;
			double HoleVert = 0;
			int NbHoriz = 0;
			int NbVert = 0;
			double edgemin = 0;
			double ShearCap = 0;
			double c = 0;
			int nb = 0;
			double Theta = 0;
			double ex = 0;
			double LvOnGusset = 0;
			double Lv = 0;
			double L = 0;
			double Th = 0;
			double ColumnGage = 0;
			double beamSideGage = 0;
			double tw = 0;
			double eat = 0;
			double R = 0;
			double V = 0;
			double H = 0;
			double otherleg = 0;
			double Fbs;
			double Fbe;
			double EquivBoltFactor = 0;
			double Smin;
			double SminPlate;
			double SminGusset;
			double maximumGage;
			double minimumGage;
			double BearingCapacityV;
			double BearingCapacityH;
			string momEffect = string.Empty;

			var component = CommonDataStatic.DetailDataDict[memberType];
			var webPlate = component.WinConnect.ShearWebPlate;
			var bolt = component.WinConnect.ShearClipAngle.BoltOnGusset;

			if (component.WinConnect.ShearClipAngle.OSL == EOSL.LongLeg)
				otherleg = component.WinConnect.ShearClipAngle.ShortLeg;
			else
				otherleg = component.WinConnect.ShearClipAngle.LongLeg;

			H = component.WinConnect.ShearClipAngle.ForceX;
			V = component.WinConnect.ShearClipAngle.ForceY;
			R = Math.Sqrt(Math.Pow(V, 2) + Math.Pow(H, 2));
			eat = bolt.MinEdgeRolled + bolt.Eincr;

			if (memberType == EMemberType.RightBeam || memberType == EMemberType.LeftBeam)
				tw = component.Shape.tw;
			else
				tw = component.BraceConnect.Gusset.Thickness;

			beamSideGage = AngleBeamSideGage(memberType, EBoltedLeg.Both, bolt.BoltSize, tw, ref ColumnGage);

			if (beamSideGage == 0 || beamSideGage + eat > otherleg)
				return false;

			if (component.WinConnect.ShearClipAngle.GussetSideGage == 0 || component.WinConnect.ShearClipAngle.GussetSideGage + eat > otherleg)
				return false;

			Th = component.WinConnect.ShearClipAngle.Thickness;
			L = component.WinConnect.ShearClipAngle.Length;
			Lv = component.WinConnect.ShearClipAngle.BoltOnColumn.EdgeDistBrace;

			// These formulas are guesses for the switch statement below
			if (memberType == EMemberType.UpperRight || memberType == EMemberType.UpperLeft)
				LvOnGusset = component.BraceConnect.Gusset.EdgeDistance - component.WinConnect.ShearClipAngle.BoltOnColumn.MinEdgeSheared + bolt.EdgeDistLongDir;
			else if (memberType == EMemberType.LowerRight || memberType == EMemberType.LowerLeft)
				LvOnGusset = component.WinConnect.ShearClipAngle.BoltOnColumn.MinEdgeSheared + bolt.EdgeDistLongDir - component.BraceConnect.Gusset.EdgeDistance;

			//switch (m)
			//{
			//	case 3:
			//	case 5:
			//		LvOnGusset = BRACE1.y[29 + (m - 3) * 5] - BRACE1.y[84 + (m - 1) * 8] + bolt.eal;
			//		break;
			//	case 4:
			//	case 6:
			//		LvOnGusset = BRACE1.y[81 + (m - 1) * 8] + bolt.eal - BRACE1.y[29 + (m - 3) * 5];
			//		break;
			//}

			ex = component.WinConnect.ShearClipAngle.GussetSideGage;
			if (component.Moment > 0 && V > 0)
			{
				ex = ex + component.Moment / V;
				Theta = Math.Atan(Math.Abs(H / V));
			}

			nb = bolt.NumberOfBolts;
			if (nb == 0)
				nb = 2;
			while (ShearCap < R)
			{
				c = EccIncLoadOnBG(1, nb, ex, Theta, bolt.SpacingLongDir, bolt.SpacingTransvDir);
				ShearCap = 2 * c * bolt.BoltStrength;
				nb++;
			}

			edgemin = Math.Max(bolt.MinEdgeSheared + (webPlate.Bolt.EdgeDistTransvDir - bolt.HoleDiameterSTD) / 2,
				MinClearDistForBearing(V / bolt.NumberOfBolts, Th, component.WinConnect.ShearClipAngle.Material.Fu, bolt.HoleType) + webPlate.Bolt.EdgeDistTransvDir / 2);
			if (bolt.EdgeDistLongDir < edgemin)
				bolt.EdgeDistLongDir = Math.Ceiling(4 * edgemin) / 4;

			component.WinConnect.ShearClipAngle.Length = (nb - 1) * bolt.SpacingLongDir + 2 * bolt.EdgeDistLongDir;
			NbVert = bolt.NumberOfBolts;
			NbHoriz = 1;
			HoleVert = webPlate.Bolt.EdgeDistTransvDir;
			HoleHoriz = webPlate.Bolt.EdgeDistLongDir;
			ItemWidth = otherleg;
			FirstGage = component.WinConnect.ShearClipAngle.GussetSideGage;
			Lh = otherleg - FirstGage;

			RequiredThickness = BlockShearWithInclinedLoad(H / 2, V / 2, Th, L, Lv, Lh, NbVert, NbHoriz, HoleVert, HoleHoriz, component.Material.Fy, component.Material.Fu, ItemWidth, FirstGage);
			if (RequiredThickness > component.WinConnect.ShearClipAngle.Thickness)
				return false;

			if (enableReporting)
			{
				if (memberType == EMemberType.LeftBeam || memberType == EMemberType.RightBeam)
					Reporting.AddHeader("Angle-to-Beam Bolts");
				else
					Reporting.AddHeader("Angle-to-Gusset Bolts");

				Reporting.AddLine(" (" + bolt.NumberOfBolts + ") " + bolt.BoltName);

				if (bolt.Slot0 && bolt.Slot1)
				{
					SminPlate = MinBoltSpacing(bolt, webPlate.Material.Fu, webPlate.Thickness, V / bolt.NumberOfBolts, webPlate.Bolt.HoleWidth);
					SminGusset = MinBoltSpacing(bolt, component.BraceConnect.Gusset.Material.Fu, Th, V / bolt.NumberOfBolts, webPlate.Bolt.HoleWidth);
				}
				else if (bolt.Slot0)
				{
					SminPlate = MinBoltSpacing(bolt, webPlate.Material.Fu, webPlate.Thickness, V / bolt.NumberOfBolts, bolt.BoltSize + ConstNum.SIXTEENTH_INCH);
					SminGusset = MinBoltSpacing(bolt, component.BraceConnect.Gusset.Material.Fu, Th, V / bolt.NumberOfBolts, webPlate.Bolt.HoleWidth);
				}
				else if (bolt.Slot1)
				{
					SminPlate = MinBoltSpacing(bolt, webPlate.Material.Fu, webPlate.Thickness, V / bolt.NumberOfBolts, webPlate.Bolt.HoleWidth);
					SminGusset = MinBoltSpacing(bolt, component.BraceConnect.Gusset.Material.Fu, Th, V / bolt.NumberOfBolts, bolt.BoltSize + ConstNum.SIXTEENTH_INCH);
				}
				else
				{
					SminPlate = MinBoltSpacing(bolt, webPlate.Material.Fu, webPlate.Thickness, V / bolt.NumberOfBolts, bolt.BoltSize + ConstNum.SIXTEENTH_INCH);
					SminGusset = MinBoltSpacing(bolt, component.BraceConnect.Gusset.Material.Fu, Th, V / bolt.NumberOfBolts, bolt.BoltSize + ConstNum.SIXTEENTH_INCH);
				}
				Smin = Math.Min(SminPlate, SminGusset);

				if (bolt.SpacingLongDir >= Smin)
					Reporting.AddLine("Bolt Vertical Spacing = " + bolt.SpacingLongDir + " >= Min. Spacing = " + Smin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Bolt Vertical Spacing = " + bolt.SpacingLongDir + " << Min. Spacing = " + Smin + ConstUnit.Length + " (NG)");

				edgemin = Math.Max(bolt.MinEdgeSheared + (webPlate.Bolt.SpacingTransvDir - bolt.HoleDiameterSTD) / 2, MinClearDistForBearing(V / bolt.NumberOfBolts, webPlate.Thickness, webPlate.Material.Fu, bolt.HoleType) + webPlate.Bolt.SpacingTransvDir / 2);
				if (bolt.EdgeDistLongDir >= edgemin)
					Reporting.AddLine("   Vert. Edge Dist. on Angle = " + bolt.EdgeDistLongDir + " >= Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("   Vert. Edge Dist. on Angle = " + bolt.EdgeDistLongDir + " << Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (NG)");

				if (memberType != EMemberType.LeftBeam || memberType != EMemberType.RightBeam)
				{
					edgemin = Math.Max(bolt.MinEdgeSheared + (webPlate.Bolt.SpacingTransvDir - bolt.HoleDiameterSTD) / 2,
						MinClearDistForBearing(V / bolt.NumberOfBolts, Th, component.BraceConnect.Gusset.Material.Fu, bolt.HoleType) + webPlate.Bolt.SpacingTransvDir / 2);

					if (LvOnGusset >= edgemin)
						Reporting.AddLine("Vertical Edge Dist. on Gusset = " + LvOnGusset + " >= Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Vertical Edge Dist. on Gusset = " + LvOnGusset + " << Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (NG)");
				}
				eat = bolt.MinEdgeRolled + bolt.Eincr;
				if (memberType == EMemberType.RightBeam || memberType == EMemberType.LeftBeam)
					tw = component.Shape.tw;
				else
					tw = component.BraceConnect.Gusset.Thickness;

				maximumGage = otherleg - eat;
				minimumGage = AngleBeamSideGage(memberType, EBoltedLeg.Both, bolt.BoltSize, tw, ref ColumnGage);
				if (component.WinConnect.ShearClipAngle.GussetSideGage <= maximumGage)
					Reporting.AddLine("Bolt Gage on Angle = " + component.WinConnect.ShearClipAngle.GussetSideGage + " <=  Max. gage = " + maximumGage + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Bolt Gage on Angle = " + component.WinConnect.ShearClipAngle.GussetSideGage + " >> Max. gage = " + maximumGage + ConstUnit.Length + " (NG)");
				if (component.WinConnect.ShearClipAngle.GussetSideGage >= minimumGage)
					Reporting.AddLine("Bolt Gage on Angle = " + component.WinConnect.ShearClipAngle.GussetSideGage + " >=  Min. gage = " + minimumGage + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Bolt Gage on Angle = " + component.WinConnect.ShearClipAngle.GussetSideGage + " << Min. gage = " + minimumGage + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Bolt Shear Strength: ");

				ex = FirstGage;
				if (component.Moment > 0 && V > 0)
				{
					ex = ex + component.Moment / V;
					if (memberType != EMemberType.LeftBeam || memberType != EMemberType.RightBeam)
						momEffect = " (Includes the effect of Transfer Force Ecc.)";
					else
						momEffect = " (Includes the effect of Gusset Edge Moment.)";
					Theta = Math.Atan(Math.Abs(H / V));
				}

				c = EccIncLoadOnBG(1, bolt.NumberOfBolts, ex, Theta, (bolt.SpacingLongDir), (bolt.SpacingTransvDir));
				ShearCap = 2 * c * bolt.BoltStrength;
				Reporting.AddHeader("Eccentricity (ex) = " + ex + ConstUnit.Length + momEffect);
				Reporting.AddLine(bolt.NumberOfBolts + " Bolts with " + bolt.SpacingLongDir + ConstUnit.Length + " Spacing");

				Reporting.AddHeader("Resultant Load (" + R + ConstUnit.Force + ") Inclined " + Theta / ConstNum.RADIAN + " Degrees from Vertical");
				Reporting.AddLine("Inclined Eccentic Load Coefficient (C) = " + c);

				if (ShearCap >= R)
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * C * Fv = 2 * " + c + " * " + bolt.BoltStrength + " = " + ShearCap + " >= " + R + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * C * Fv = 2 * " + c + " * " + bolt.BoltStrength + " = " + ShearCap + " << " + R + ConstUnit.Force + " (NG)");

				// Bolt Bearing under Vertical Load
				Reporting.AddHeader("Bolt Bearing on Angle Leg:");
				Reporting.AddHeader("Vertical Load:");
				Fbs = CommonCalculations.SpacingBearing(bolt.SpacingLongDir, webPlate.Bolt.SpacingTransvDir, bolt.BoltSize, bolt.HoleType, webPlate.Material.Fu, false);
				Fbe = CommonCalculations.EdgeBearing(bolt.EdgeDistLongDir, (webPlate.Bolt.SpacingTransvDir), bolt.BoltSize, webPlate.Material.Fu, bolt.HoleType, false);
				EquivBoltFactor = c / (1 * bolt.NumberOfBolts);
				if (EquivBoltFactor > 1)
					EquivBoltFactor = 1;
				Reporting.AddLine("Equiv. Bolt Factor, ef = C / Nb <= 1 = " + c + " / " + bolt.NumberOfBolts + " = " + EquivBoltFactor);
				BearingCapacityV = EquivBoltFactor * 1 * (Fbe + Fbs * (bolt.NumberOfBolts - 1)) * bolt.BoltSize * component.WinConnect.ShearClipAngle.Thickness;
				Reporting.AddLine(ConstString.PHI + " Rn = ef * Nh * (Fbe + Fbs * (Nl - 1)) * db * t");
				Reporting.AddLine("= " + EquivBoltFactor + " * " + 1 + " * (" + Fbe + " + " + Fbs + " * (" + bolt.NumberOfBolts + " - 1)) * " + bolt.BoltSize + " * " + component.WinConnect.ShearClipAngle.Thickness);
				if (BearingCapacityV >= V / 2)
					Reporting.AddLine(BearingCapacityV + " >= " + V + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine(BearingCapacityV + " << " + V + ConstUnit.Force + " (NG)");

				// Bolt Bearing under Horizontal Load
				Reporting.AddHeader("Horizontal Load:");
				Fbe = CommonCalculations.EdgeBearing(otherleg - FirstGage, (HoleHoriz), bolt.BoltSize, component.WinConnect.ShearClipAngle.Material.Fu, bolt.HoleType, false);
				EquivBoltFactor = c / (1 * bolt.NumberOfBolts);
				if (EquivBoltFactor > 1)
					EquivBoltFactor = 1;
				BearingCapacityH = EquivBoltFactor * bolt.NumberOfBolts * (Fbe + Fbs * (1 - 1)) * bolt.BoltSize * component.WinConnect.ShearClipAngle.Thickness;
				Reporting.AddLine(ConstString.PHI + " Rn = ef * Nl * Fbe * db * t");
				Reporting.AddLine("= " + EquivBoltFactor + " * " + bolt.NumberOfBolts + " * " + Fbe + " * " + bolt.BoltSize + " * " + component.WinConnect.ShearClipAngle.Thickness);
				if (BearingCapacityH >= H / 2)
					Reporting.AddLine(BearingCapacityH + " >= " + (H / 2) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine(BearingCapacityH + " << " + (H / 2) + ConstUnit.Force + " (NG)");

				GussetandBeamCheckwithBoltedClipAngles(memberType, V, H, R, EquivBoltFactor, EBearingTearOut.Bearing);
				RequiredThickness = BlockShearWithInclinedLoad(H / 2, V / 2, Th, L, Lv, Lh, NbVert, NbHoriz, HoleVert, HoleHoriz, component.Material.Fy, component.Material.Fu, ItemWidth, FirstGage);
				GussetandBeamCheckwithBoltedClipAngles(memberType, V, H, R, EquivBoltFactor, EBearingTearOut.TearOut);
			}
			return true;
		}

		private static double BlockShearWithInclinedLoad(double H, double V, double Th, double L, double Lv, double Lh, double NbVert, double NbHoriz, double HoleVert, double HoleHoriz, double Fy, double Fu, double ItemWidth, double FirstGage)
		{
			double BlockShearWithInclinedLoad = 0;

			double BlockShearCapacity = 0;
			double BlockShearCapacity2 = 0;
			double BlockShearCapacity1 = 0;
			double An2 = 0;
			double An1 = 0;
			double TensionandShearCapacity = 0;
			double GrossTensionandShearCapacity = 0;
			double Ag = 0;
			double NetTensionandShearCapacity = 0;
			double b = 0;
			double a = 0;
			double An = 0;
			double R = 0;
			double Th_Req = 0;

			// Combined Tension and Shear Rupture
			Th_Req = Th;
			R = Math.Pow(Math.Pow(H, 2) + Math.Pow(V, 2), 0.5);
			An = (L - NbVert * (HoleVert + ConstNum.SIXTEENTH_INCH)) * Th_Req;
			a = H / R;
			b = V / R;

			if (b > 0 && a > 0)
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.18 * (a / b) * (-1 + Math.Pow(1 + Math.Pow(b / a, 2) / 0.09, 0.5)) * An * Fu / b;
			else if (b == 0)
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * An * Fu;
			else if (a == 0)
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.6 * An * Fu;
			// Combined Tension and Shear Yielding
			Ag = L * Th_Req;
			if (b > 0 && a > 0)
				GrossTensionandShearCapacity = Math.Pow(ConstNum.FIOMEGA1_0N, 2) * 0.18 * Math.Pow(a / b, 2) * (-1 / ConstNum.FIOMEGA0_9N + Math.Pow(1 / Math.Pow(ConstNum.FIOMEGA0_9N, 2) + Math.Pow(b / a, 2) / (0.09 * Math.Pow(ConstNum.FIOMEGA1_0N, 2)), 0.5)) * Ag * Fy / a;
			else if (b == 0)
				GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * Ag * Fy;
			else if (a == 0)
				GrossTensionandShearCapacity = ConstNum.FIOMEGA1_0N * 0.6 * Ag * Fy;

			TensionandShearCapacity = Math.Min(NetTensionandShearCapacity, GrossTensionandShearCapacity);

			if (TensionandShearCapacity < R)
				Th_Req = Th_Req * R / TensionandShearCapacity;

			// Block Shear 1
			An1 = (L - Lv - (NbVert - 0.5) * (HoleVert + ConstNum.SIXTEENTH_INCH)) * Th_Req;
			An2 = (ItemWidth - FirstGage - (NbHoriz - 0.5) * (HoleHoriz + ConstNum.SIXTEENTH_INCH)) * Th_Req;
			BlockShearCapacity1 = ShearTensionInteraction(1, R, a, b, Fu, An1, An2);
			// Block Shear 2
			An1 = (L - 2 * Lv - (NbVert - 1) * (HoleVert + ConstNum.SIXTEENTH_INCH)) * Th_Req;
			An2 = 2 * (ItemWidth - FirstGage - (NbHoriz - 0.5) * (HoleHoriz + ConstNum.SIXTEENTH_INCH)) * Th_Req;
			BlockShearCapacity2 = ShearTensionInteraction(2, R, a, b, Fu, An1, An2);
			BlockShearCapacity = Math.Min(BlockShearCapacity1, BlockShearCapacity2);
			if (BlockShearCapacity < R)
			{
				Th_Req = Th_Req * R / BlockShearCapacity;
			}
			BlockShearWithInclinedLoad = Th_Req;

			Reporting.AddHeader("Block Shear With Inclined Load:");
			Reporting.AddLine("Angle Leg Shear and Normal Stresses");
			Reporting.AddLine("Combined Tension and Shear on Angle Leg");
			Reporting.AddFormulaLine();

			a = H / R;
			b = V / R;
			if (V == 0)
				Reporting.AddHeader("Load Angle, " + ConstString.PHI + " = Atn(H / V) = 90 Degees");
			else
				Reporting.AddHeader("Load Angle, " + ConstString.PHI + " = Atn(H / V) = " + Math.Atan(Math.Abs(H / V) / ConstNum.RADIAN) + " Degees");

			Reporting.AddLine("A = Sin(" + ConstString.PHI + ") = " + a);
			Reporting.AddLine("B = Cos(" + ConstString.PHI + ") = " + b);

			Reporting.AddHeader("Rupture of Angle Leg:");
			An = (L - NbVert * (HoleVert + ConstNum.SIXTEENTH_INCH)) * Th;
			Reporting.AddLine("Net Area, An = (L - Nl * (dv + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + L + " - " + NbVert + " * (" + HoleVert + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + Th);
			Reporting.AddLine("= " + An + ConstUnit.Area);

			if (b > 0 && a > 0)
			{
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.18 * (a / b) * (-1 + Math.Pow(1 + Math.Pow(b / a, 2) / 0.09, 0.5)) * An * Fu / b;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + "* 0.18 * (A/ B) * (-1 + (1 + (B / A)² / 0.09)^0.5) * An * Fu / B");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.18 * (" + a + " / " + b + ") * (-1 + (1 + (" + b + " / " + a + ")² / 0.09)^0.5) * " + An + " * " + Fu + " / " + b);
			}
			else if (b == 0)
			{
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * An * Fu;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * An * Fu = " + ConstString.FIOMEGA0_75 + " * " + An + " * " + Fu);
			}
			else if (a == 0)
			{
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.6 * An * Fu;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.6 * An * Fu = " + ConstString.FIOMEGA0_75 + " * 0.6 * " + An + " * " + Fu);
			}
			if (NetTensionandShearCapacity >= R)
				Reporting.AddLine("= " + NetTensionandShearCapacity + " >= " + R + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + NetTensionandShearCapacity + " << " + R + ConstUnit.Force + " (NG)");

			// Combined Tension and Shear Yielding
			Reporting.AddHeader("Yielding of Angle Leg:");
			Ag = L * Th;
			Reporting.AddLine("Ag= L * t = " + L + " * " + Th + " = " + Ag + ConstUnit.Area);

			if (b > 0 && a > 0)
			{
				GrossTensionandShearCapacity = Math.Pow(ConstNum.FIOMEGA1_0N, 2) * 0.18 * Math.Pow(a / b, 2) * (-1 / ConstNum.FIOMEGA0_9N + Math.Pow(1 / Math.Pow(ConstNum.FIOMEGA0_9N, 2) + Math.Pow(b / a, 2) / (0.09 * Math.Pow(ConstNum.FIOMEGA1_0N, 2)), 0.5)) * Ag * Fy / a;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.FIOMEGA1_0N + "² * 0.18 * (A / B)² * (-1 / " + ConstString.FIOMEGA0_9 + " + (1 / (" + ConstString.FIOMEGA0_9 + "²) + (B / A)² / (0.09 * " + ConstNum.FIOMEGA1_0N + "²))^0.5) * Ag * Fy/A");
				Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + "² * 0.18 * (" + a + " / " + b + ")² * (-1 / " + ConstString.FIOMEGA0_9 + " + (1 / (" + ConstString.FIOMEGA0_9 + "²) + (" + b + " / " + a + ")² / (0.09 * " + ConstNum.FIOMEGA1_0N + "²))^0.5) * " + Ag + " * " + Fy + " / " + a);
			}
			else if (b == 0)
			{
				GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * Ag * Fy;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Ag * Fy = " + ConstString.FIOMEGA0_9 + " * " + Ag + " * " + Fy);
			}
			else if (a == 0)
			{
				GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * 0.6 * Ag * Fy;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * 0.6 * Ag * Fy = " + ConstString.FIOMEGA0_9 + " * 0.6 * " + Ag + " * " + Fy);
			}
			if (GrossTensionandShearCapacity >= R)
				Reporting.AddLine(" = " + GrossTensionandShearCapacity + " >= " + R + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine(" = " + GrossTensionandShearCapacity + " << " + R + ConstUnit.Force + " (NG)");

			// Block Shear 1
			Reporting.AddHeader("Block Shear of Angle Leg:");
			Reporting.AddLine("Vertical (An1, Ft1) and Horizontal (An2, Ft2) Sections:");

			Reporting.AddHeader("Pattern 1:");
			An1 = (L - Lv - (NbVert - 0.5) * (HoleVert + ConstNum.SIXTEENTH_INCH)) * Th;
			Reporting.AddLine("An1 = (L - Lv - (Nl - 0.5) * (dv + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + L + " - " + Lv + " - (" + NbVert + " - 0.5) * (" + HoleVert + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + Th);
			Reporting.AddLine("= " + An + ConstUnit.Area);
			An2 = (ItemWidth - FirstGage - (NbHoriz - 0.5) * (HoleHoriz + ConstNum.SIXTEENTH_INCH)) * Th;
			Reporting.AddLine("An2 = (W-g1 - (Nh - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + ItemWidth + " - " + FirstGage + " - (" + NbHoriz + " - 0.5) * (" + HoleHoriz + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + Th);
			Reporting.AddLine("= " + An2 + ConstUnit.Area);
			BlockShearCapacity1 = ShearTensionInteraction(1, R, a, b, Fu, An1, An2);

			Reporting.AddHeader("Pattern 2:");
			An1 = (L - 2 * Lv - (NbVert - 1) * (HoleVert + ConstNum.SIXTEENTH_INCH)) * Th;
			Reporting.AddLine("An1 = (L - 2*Lv - (Nl - 1) * (dv + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + L + " - 2*" + Lv + " - (" + NbVert + " - 1) * (" + HoleVert + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + Th);
			Reporting.AddLine("= " + An + ConstUnit.Area);

			An2 = 2 * (ItemWidth - FirstGage - (NbHoriz - 0.5) * (HoleHoriz + ConstNum.SIXTEENTH_INCH)) * Th;
			Reporting.AddLine("An2 = 2 * (W - g1- (Nh - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= 2 * (" + ItemWidth + " - " + FirstGage + " - (" + NbHoriz + " - 0.5) * (" + HoleHoriz + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + Th);
			Reporting.AddLine("= " + An2 + ConstUnit.Area);
			BlockShearCapacity2 = ShearTensionInteraction(2, R, a, b, Fu, An1, An2);

			return BlockShearWithInclinedLoad;
		}

		internal static void SplicePlateBuckling(EMemberType memberType, ref double UtilizationFactor, double t)
		{
			double Pn = 0;
			double Fcr = 0;
			double KLoR = 0;
			double Lcr = 0;
			double Lb = 0;
			double Mn = 0;
			double Mu = 0;
			double ex = 0;
			double Pu = 0;
			double Ag = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];

			Ag = component.BraceConnect.SplicePlates.Width * t;
			Pu = component.AxialCompression;
			ex = (t + component.BraceConnect.Gusset.Thickness) / 2;
			Mu = Pu * ex / 2;
			Mn = component.BraceConnect.SplicePlates.Material.Fy * component.BraceConnect.SplicePlates.Width * Math.Pow(t, 2) / 4;
			Lb = component.BraceConnect.Gusset.ColumnSideSetback + component.BraceConnect.Gusset.EdgeDistance;
			Lcr = 1.2 * Lb;
			KLoR = 3.464 * Lcr / t;

			Fcr = CommonCalculations.BucklingStress(KLoR, component.BraceConnect.Gusset.Material.Fy, true);
			Pn = Ag * Fcr;
			if (Pu / (ConstNum.FIOMEGA0_9N * Pn) >= 0.2)
				UtilizationFactor = Pu / (ConstNum.FIOMEGA0_9N * Pn) + 8 / 9.0 * Mu / (ConstNum.FIOMEGA0_9N * Mn);
			else
				UtilizationFactor = Pu / (2 * ConstNum.FIOMEGA0_9N * Pn) + Mu / (ConstNum.FIOMEGA0_9N * Mn);

			Reporting.AddHeader("Splice Plate Buckling");
			Reporting.AddLine("Ag = W * t = " + component.BraceConnect.SplicePlates.Width + " * " + t + " = " + Ag + ConstUnit.Area);
			Reporting.AddLine("ex = (ts + tg)/ 2 = (" + component.BraceConnect.SplicePlates.Thickness + " * " + t + ") / 2  = " + ex + ConstUnit.Length);
			Reporting.AddLine("Mu =Pu * ex / 2 = " + Pu + " * " + ex + " / 2  = " + Mu + ConstUnit.MomentUnitInch);
			Reporting.AddLine("Mn =(Fy * W * t²) / 4 = (" + component.BraceConnect.SplicePlates.Material.Fy + " * " + component.BraceConnect.SplicePlates.Width + " * " + component.BraceConnect.SplicePlates.Thickness + "²) / 4  = " + Mn + ConstUnit.MomentUnitInch);
			Reporting.AddLine("Pn = Ag * Fcr = " + Ag + " * " + Fcr + " = " + Pn + ConstUnit.Force);
			Reporting.AddLine("Lb= g + e = " + component.BraceConnect.Gusset.ColumnSideSetback + " + " + component.BraceConnect.Gusset.EdgeDistance + " = " + Lb + ConstUnit.Length);
			Reporting.AddLine("Lcr = 1.2 * Lb = 1.2 * " + Lb + " = " + Lcr + ConstUnit.Length);
			Reporting.AddLine("KL / r = 3.464 * Lcr / t  = 3.464 * " + Lcr + " / " + component.BraceConnect.SplicePlates.Thickness + " = " + KLoR);

			Pn = Ag * CommonCalculations.BucklingStress(KLoR, component.BraceConnect.Gusset.Material.Fy, true);
			if (Pu / (ConstNum.FIOMEGA0_9N * Pn) >= 0.2)
				UtilizationFactor = Pu / (ConstNum.FIOMEGA0_9N * Pn) + 8 / 9.0 * Mu / (ConstNum.FIOMEGA0_9N * Mn);
			else
				UtilizationFactor = Pu / (2 * ConstNum.FIOMEGA0_9N * Pn) + Mu / (ConstNum.FIOMEGA0_9N * Mn);

			Reporting.AddHeader("Utilization Factor:");
			if (Pu / (ConstNum.FIOMEGA0_9N * Pn) >= 0.2)
			{
				Reporting.AddLine("Pu / (" + ConstString.FIOMEGA0_9 + " * Pn) >= 0.2");
				Reporting.AddLine("Pu / (" + ConstString.FIOMEGA0_9 + "*Pn) + 8 / 9 * Mu / (" + ConstString.FIOMEGA0_9 + " * Mn)");
				Reporting.AddLine("= " + Pu + " / (" + ConstString.FIOMEGA0_9 + " * " + Pn + ") + 8 / 9 * " + Mu + " / (" + ConstString.FIOMEGA0_9 + " * " + Mn + ")");
			}
			else
			{
				Reporting.AddLine("Pu / (" + ConstString.FIOMEGA0_9 + " * Pn) << 0.2");
				Reporting.AddLine("Pu / (2 * " + ConstString.FIOMEGA0_9 + " * Pn) + Mu / (" + ConstString.FIOMEGA0_9 + " * Mn)");
				Reporting.AddLine("= " + Pu + " / (2 * " + ConstString.FIOMEGA0_9 + " * " + Pn + ") + " + Mu + " / (" + ConstString.FIOMEGA0_9 + " * " + Mn + ")");
			}
			if (UtilizationFactor <= 1)
				Reporting.AddLine(UtilizationFactor + " <= 1.0 (OK)");
			else
				Reporting.AddLine(UtilizationFactor + " >> 1.0 (NG)");
		}

		internal static void BraceBoltBearingOnGusset(EMemberType memberType, ref double gthickness)
		{
			double capacityComb = 0;
			double n1 = 0;
			double Fbre1 = 0;
			double capacityComp = 0;
			double capacity = 0;
			double Fbrs = 0;
			double Fbre = 0;
			double N = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];

			if (component.BraceToGussetWeldedOrBolted != EConnectionStyle.Bolted)
				return;

			N = component.BoltBrace.NumberOfBolts;
			Fbre = CommonCalculations.EdgeBearing(component.BoltBrace.EdgeDistGusset, component.BraceConnect.Gusset.HoleLongP, component.BoltBrace.BoltSize, component.BraceConnect.Gusset.Material.Fu, component.BoltBrace.HoleType, false);
			Fbrs = CommonCalculations.SpacingBearing(component.BoltBrace.SpacingLongDir, component.BraceConnect.Gusset.HoleLongP, component.BoltBrace.BoltSize, component.BoltBrace.HoleType, component.BraceConnect.Gusset.Material.Fu, false);
			if (component.ShapeType == EShapeType.SingleAngle || component.ShapeType == EShapeType.DoubleAngle)
			{
				if (component.BoltBrace.NumberOfRows == 1)
				{
					capacity = Fbre + (N - 1) * Fbrs;
					capacityComp = N * Fbrs;
				}
				else if (component.BraceConnect.Brace.BoltsAreStaggered)
				{
					Fbre1 = CommonCalculations.EdgeBearing((component.BoltBrace.EdgeDistGusset + component.BoltBrace.SpacingLongDir / 2), (component.BraceConnect.Gusset.HoleLongP), component.BoltBrace.BoltSize, component.BraceConnect.Gusset.Material.Fu, component.BoltBrace.HoleType, false);
					capacity = Fbre + Fbre1 + (N - 2) * Fbrs;
					capacityComp = N * Fbrs;
				}
				else if (component.BoltBrace.NumberOfBolts % 2 == 0)
				{
					capacity = 2 * Fbre + (N - 2) * Fbrs;
					capacityComp = N * Fbrs;
				}
				else
				{
					Fbre1 = CommonCalculations.EdgeBearing((component.BoltBrace.EdgeDistGusset + component.BoltBrace.SpacingLongDir), (component.BraceConnect.Gusset.HoleLongP), component.BoltBrace.BoltSize, component.BraceConnect.Gusset.Material.Fu, component.BoltBrace.HoleType, false);
					capacity = Fbre + Fbre1 + (N - 2) * Fbrs;
					capacityComp = N * Fbrs;
				}
			}
			else if (component.ShapeType == EShapeType.WTSection)
			{
				if (component.BoltBrace.NumberOfRows == 1)
				{
					capacity = 2 * Fbre + (2 * N - 2) * Fbrs;
					capacityComp = 2 * N * Fbrs;
				}
				else if (component.BraceConnect.Brace.BoltsAreStaggered)
				{
					Fbre1 = CommonCalculations.EdgeBearing((component.BoltBrace.EdgeDistGusset + component.BoltBrace.SpacingLongDir / 2), (component.BraceConnect.Gusset.HoleLongP), component.BoltBrace.BoltSize, component.BraceConnect.Gusset.Material.Fu, component.BoltBrace.HoleType, false);
					capacity = 2 * Fbre + 2 * Fbre1 + (2 * N - 4) * Fbrs;
					capacityComp = 2 * N * Fbrs;
				}
				else if (component.BoltBrace.NumberOfBolts % 2 == 0)
				{
					capacity = 4 * Fbre + (2 * N - 4) * Fbrs;
					capacityComp = 2 * N * Fbrs;
				}
				else
				{
					Fbre1 = CommonCalculations.EdgeBearing((component.BoltBrace.EdgeDistGusset + component.BoltBrace.SpacingLongDir), (component.BraceConnect.Gusset.HoleLongP), component.BoltBrace.BoltSize, component.BraceConnect.Gusset.Material.Fu, component.BoltBrace.HoleType, false);
					capacity = 2 * Fbre + 2 * Fbre1 + (2 * N - 4) * Fbrs;
					capacityComp = 2 * N * Fbrs;
				}
			}
			else if (component.ShapeType == EShapeType.SingleChannel || component.ShapeType == EShapeType.DoubleChannel)
			{
				// C
				n1 = component.BoltBrace.NumberOfRows;
				capacity = n1 * Fbre + (N - n1) * Fbrs;
				capacityComp = N * Fbrs;
			}
			else if (component.ShapeType == EShapeType.HollowSteelSection && component.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted
				)
			{
				n1 = component.BraceConnect.SplicePlates.Bolt.NumberOfLines;
				N = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].BoltBrace.NumberOfLines * n1; //N = BRACE1.Bolts[7 + (m - 3) * 6].nl * n1;
				capacity = n1 * Fbre + (N - n1) * Fbrs;
				capacityComp = N * Fbrs;
			}
			gthickness = Math.Max(component.AxialTension / capacity, component.AxialCompression / capacityComp);

			if (component.ShapeType == EShapeType.SingleAngle || component.ShapeType == EShapeType.DoubleAngle)
			{
				if (component.BoltBrace.NumberOfRows == 1)
				{
					Reporting.AddHeader("With Tensile Force:");
					capacity = (Fbre + (N - 1) * Fbrs) * component.BraceConnect.Gusset.Thickness;
					Reporting.AddLine(ConstString.PHI + " Rn = (Fbre + (n - 1) * Fbrs) * t");
					Reporting.AddLine("= (" + Fbre + " + (" + N + " - 1) * " + Fbrs + ") * " + component.BraceConnect.Gusset.Thickness);
					if (capacity >= component.AxialTension)
						Reporting.AddLine(capacity + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacity + " << " + component.AxialTension + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("With Compressive Force:");
					capacityComp = N * Fbrs * component.BraceConnect.Gusset.Thickness;
					Reporting.AddLine(ConstString.PHI + " Rn = N * Fbrs * t");
					Reporting.AddLine("= " + N + Fbrs + " * " + component.BraceConnect.Gusset.Thickness);
					if (capacityComp >= component.AxialCompression)
						Reporting.AddLine(capacityComp + " >= " + component.AxialCompression + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacityComp + " << " + component.AxialCompression + ConstUnit.Force + " (NG)");
				}
				else if (component.BraceConnect.Brace.BoltsAreStaggered)
				{
					Reporting.AddHeader("For the second bolt line:");
					Fbre1 = CommonCalculations.EdgeBearing((component.BoltBrace.EdgeDistGusset + component.BoltBrace.SpacingLongDir / 2), ((int)component.BraceConnect.Gusset.HoleLongP), component.BoltBrace.BoltSize, component.BraceConnect.Gusset.Material.Fu, component.BoltBrace.HoleType, false);
					capacity = (Fbre + Fbre1 + (N - 2) * Fbrs) * component.BraceConnect.Gusset.Thickness;
					Reporting.AddHeader("With Tensile Force:");
					Reporting.AddLine(ConstString.PHI + " Rn = (Fbre + Fbre1 + (n - 2) * Fbrs) * t");
					Reporting.AddLine("= (" + Fbre + " + " + Fbre1 + " + (" + N + " - 2) * " + Fbrs + ") * " + component.BraceConnect.Gusset.Thickness);
					if (capacity >= component.AxialTension)
						Reporting.AddLine(capacity + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacity + " << " + component.AxialTension + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("With Compressive Force:");
					Reporting.AddLine(ConstString.PHI + " Rn = N * Fbrs) * t");
					Reporting.AddLine("= " + N + " * " + Fbrs + " * " + component.BraceConnect.Gusset.Thickness);
					if (capacityComp >= component.AxialCompression)
						Reporting.AddLine(capacityComp + " >= " + component.AxialCompression + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacityComp + " << " + component.AxialCompression + ConstUnit.Force + " (NG)");

				}
				else if (component.BoltBrace.NumberOfBolts % 2 == 0)
				{
					Reporting.AddHeader("With Tensile Force:");
					capacity = (2 * Fbre + (N - 2) * Fbrs) * component.BraceConnect.Gusset.Thickness;
					Reporting.AddLine(ConstString.PHI + " Rn = (2 * Fbre + (n - 2) * Fbrs) * t");
					Reporting.AddLine("= (2 * " + Fbre + " + (" + N + " - 2) * " + Fbrs + ") * " + component.BraceConnect.Gusset.Thickness);
					if (capacity >= component.AxialTension)
						Reporting.AddLine(capacity + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacity + " << " + component.AxialTension + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("With Compressive Force:");
					capacityComp = N * Fbrs * component.BraceConnect.Gusset.Thickness;
					Reporting.AddLine(ConstString.PHI + " Rn = N * Fbrs * t");
					Reporting.AddLine("= " + N + " - 2) * " + Fbrs + " * " + component.BraceConnect.Gusset.Thickness);
					if (capacityComp >= component.AxialCompression)
						Reporting.AddLine(capacityComp + " >= " + component.AxialCompression + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacityComp + " << " + component.AxialCompression + ConstUnit.Force + " (NG)");
				}
				else
				{
					Reporting.AddHeader("For the second bolt line:");
					Fbre1 = CommonCalculations.EdgeBearing((component.BoltBrace.EdgeDistGusset + component.BoltBrace.SpacingLongDir), ((int)component.BraceConnect.Gusset.HoleLongP), component.BoltBrace.BoltSize, component.BraceConnect.Gusset.Material.Fu, component.BoltBrace.HoleType, false);
					Reporting.AddHeader("With Tensile Force:");
					capacity = (Fbre + Fbre1 + (N - 2) * Fbrs) * component.BraceConnect.Gusset.Thickness;
					Reporting.AddLine(ConstString.PHI + " Rn = (Fbre + Fbre1 + (n - 2) * Fbrs) * t");
					Reporting.AddLine("= (" + Fbre + " + " + Fbre1 + " + (" + N + " - 2) * " + Fbrs + ") * " + component.BraceConnect.Gusset.Thickness);
					if (capacity >= component.AxialTension)
						Reporting.AddLine(capacity + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacity + " << " + component.AxialTension + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("With Compressive Force:");
					capacityComp = N * Fbrs * component.BraceConnect.Gusset.Thickness;
					Reporting.AddLine(ConstString.PHI + " Rn = N * Fbrs * t");
					Reporting.AddLine("= " + N + " * " + Fbrs + " * " + component.BraceConnect.Gusset.Thickness);
					if (capacityComp >= component.AxialCompression)
						Reporting.AddLine(capacityComp + " >= " + component.AxialCompression + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacityComp + " << " + component.AxialCompression + ConstUnit.Force + " (NG)");
				}
			}
			else if (component.ShapeType == EShapeType.WTSection)
			{
				if (component.BoltBrace.NumberOfRows == 1)
				{
					Reporting.AddHeader("With Tensile Force:");
					capacity = (2 * Fbre + (2 * N - 2) * Fbrs) * component.BraceConnect.Gusset.Thickness;
					Reporting.AddLine(ConstString.PHI + " Rn = (2 * Fbre + (n - 2) * Fbrs) * t");
					Reporting.AddLine("= (2 * " + Fbre + " + (" + 2 * N + " - 2) * " + Fbrs + ") * " + component.BraceConnect.Gusset.Thickness);
					if (capacity >= component.AxialTension)
						Reporting.AddLine(capacity + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacity + " << " + component.AxialTension + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("With Compressive Force:");
					capacityComb = 2 * N * Fbrs * component.BraceConnect.Gusset.Thickness;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * N * Fbrs * t");
					Reporting.AddLine("= " + 2 * N + " * " + Fbrs + " * " + component.BraceConnect.Gusset.Thickness);
					if (capacityComb >= component.AxialCompression)
						Reporting.AddLine(capacityComb + " >= " + component.AxialCompression + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacityComb + " << " + component.AxialCompression + ConstUnit.Force + " (NG)");
				}
				else if (component.BraceConnect.Brace.BoltsAreStaggered)
				{
					Reporting.AddHeader("For the second bolt line:");
					Fbre1 = CommonCalculations.EdgeBearing((component.BoltBrace.EdgeDistGusset + component.BoltBrace.SpacingLongDir / 2), ((int)component.BraceConnect.Gusset.HoleLongP), component.BoltBrace.BoltSize, component.BraceConnect.Gusset.Material.Fu, component.BoltBrace.HoleType, true);

					Reporting.AddHeader("With Tensile Force:");
					capacity = (2 * Fbre + 2 * Fbre1 + (2 * N - 4) * Fbrs) * component.BraceConnect.Gusset.Thickness;
					Reporting.AddLine(ConstString.PHI + " Rn = (2 * Fbre + 2 * Fbre1 + (n - 4) * Fbrs) * t");
					Reporting.AddLine("= (2 * " + Fbre + " + 2 * " + Fbre1 + " + (" + 2 * N + " - 4) * " + Fbrs + ") * " + component.BraceConnect.Gusset.Thickness);
					if (capacity >= component.AxialTension)
						Reporting.AddLine(capacity + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacity + " << " + component.AxialTension + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("With Compressive Force:");
					capacityComp = 2 * N * Fbrs * component.BraceConnect.Gusset.Thickness;
					Reporting.AddLine(ConstString.PHI + " Rn = N * Fbrs * t");
					Reporting.AddLine("= " + 2 * N + " * " + Fbrs + " * " + component.BraceConnect.Gusset.Thickness);
					if (capacityComp >= component.AxialCompression)
						Reporting.AddLine(capacityComp + " >= " + component.AxialCompression + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacityComp + " << " + component.AxialCompression + ConstUnit.Force + " (NG)");
				}
				else if (component.BoltBrace.NumberOfBolts % 2 == 0)
				{
					Reporting.AddHeader("With Tensile Force:");
					capacity = (4 * Fbre + (2 * N - 4) * Fbrs) * component.BraceConnect.Gusset.Thickness;
					Reporting.AddLine(ConstString.PHI + " Rn = (4 * Fbre + (n - 4) * Fbrs) * t");
					Reporting.AddLine("= (4 * " + Fbre + " + (" + 2 * N + " - 4) * " + Fbrs + ") * " + component.BraceConnect.Gusset.Thickness);
					if (capacity >= component.AxialTension)
						Reporting.AddLine(capacity + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacity + " << " + component.AxialTension + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("With Compressive Force:");
					capacityComp = 2 * N * Fbrs * component.BraceConnect.Gusset.Thickness;
					Reporting.AddLine(ConstString.PHI + " Rn = N * Fbrs * t");
					Reporting.AddLine("= " + 2 * N + " * " + Fbrs + " ) * " + component.BraceConnect.Gusset.Thickness);
					if (capacityComp >= component.AxialCompression)
						Reporting.AddLine(capacityComp + " >= " + component.AxialCompression + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacityComp + " << " + component.AxialCompression + ConstUnit.Force + " (NG)");
				}
				else
				{
					Reporting.AddHeader("For the second bolt line:");
					Fbre1 = CommonCalculations.EdgeBearing((component.BoltBrace.EdgeDistGusset + component.BoltBrace.SpacingLongDir), ((int)component.BraceConnect.Gusset.HoleLongP), component.BoltBrace.BoltSize, component.BraceConnect.Gusset.Material.Fu, component.BoltBrace.HoleType, true);
					Reporting.AddHeader("With Tensile Force:");
					capacity = (2 * Fbre + 2 * Fbre1 + (2 * N - 4) * Fbrs) * component.BraceConnect.Gusset.Thickness;
					Reporting.AddLine(ConstString.PHI + " Rn = (2 * Fbre + 2 * Fbre1 + (n - 4) * Fbrs) * t");
					Reporting.AddLine("= (2 * " + Fbre + " + 2 * " + Fbre1 + " + (" + 2 * N + " - 4) * " + Fbrs + ") * " + component.BraceConnect.Gusset.Thickness);
					if (capacity >= component.AxialTension)
						Reporting.AddLine(capacity + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacity + " << " + component.AxialTension + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("With Compressive Force:");
					capacityComp = 2 * N * Fbrs * component.BraceConnect.Gusset.Thickness;
					Reporting.AddLine(ConstString.PHI + " Rn = N * Fbrs * t");
					Reporting.AddLine("= " + 2 * N + " * " + Fbrs + " * " + component.BraceConnect.Gusset.Thickness);
					if (capacityComp >= component.AxialCompression)
						Reporting.AddLine(capacityComp + " >= " + component.AxialCompression + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(capacityComp + " << " + component.AxialCompression + ConstUnit.Force + " (NG)");
				}
			}
			else if (component.ShapeType == EShapeType.SingleChannel || component.ShapeType == EShapeType.DoubleChannel)
			{
				n1 = component.BoltBrace.NumberOfRows;
				Reporting.AddHeader("With Tensile Force:");
				capacity = (n1 * Fbre + (N - n1) * Fbrs) * component.BraceConnect.Gusset.Thickness;
				Reporting.AddLine(ConstString.PHI + " Rn = (n1 * Fbre + (n - n1) * Fbrs) * t");
				Reporting.AddLine("= (" + n1 + " * " + Fbre + " + (" + N + "-" + n1 + ") * " + Fbrs + ") * " + component.BraceConnect.Gusset.Thickness);
				if (capacity >= component.AxialTension)
					Reporting.AddLine(capacity + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine(capacity + " << " + component.AxialTension + ConstUnit.Force + " (NG)");

				Reporting.AddHeader("With Compressive Force:");
				capacityComp = N * Fbrs * component.BraceConnect.Gusset.Thickness;
				Reporting.AddLine(ConstString.PHI + " Rn = N * Fbrs * t");
				Reporting.AddLine("= " + N + " * " + Fbrs + " * " + component.BraceConnect.Gusset.Thickness);
				if (capacityComp >= component.AxialCompression)
					Reporting.AddLine(capacityComp + " >= " + component.AxialCompression + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine(capacityComp + " << " + component.AxialCompression + ConstUnit.Force + " (NG)");
			}
			else if (component.ShapeType == EShapeType.HollowSteelSection && component.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
			{
				n1 = component.BraceConnect.SplicePlates.Bolt.NumberOfLines;
				N = component.BraceConnect.SplicePlates.Bolt.NumberOfBolts;
				Reporting.AddHeader("With Tensile Force:");
				capacity = (n1 * Fbre + (N - n1) * Fbrs) * component.BraceConnect.Gusset.Thickness;
				Reporting.AddLine(ConstString.PHI + " Rn = (n1 * Fbre + (n - n1) * Fbrs) * t");
				Reporting.AddLine("= (" + n1 + " * " + Fbre + " + (" + N + "-" + n1 + ") * " + Fbrs + ") * " + component.BraceConnect.Gusset.Thickness);
				if (capacity >= component.AxialTension)
					Reporting.AddLine(capacity + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine(capacity + " << " + component.AxialTension + ConstUnit.Force + " (NG)");

				Reporting.AddHeader("With Compressive Force:");
				capacityComp = N * Fbrs * component.BraceConnect.Gusset.Thickness;
				Reporting.AddLine(ConstString.PHI + " Rn = N * Fbrs * t");
				Reporting.AddLine("= " + N + " * " + Fbrs + " * " + component.BraceConnect.Gusset.Thickness);
				if (capacityComp >= component.AxialCompression)
					Reporting.AddLine(capacityComp + " >= " + component.AxialCompression + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine(capacityComp + " << " + component.AxialCompression + ConstUnit.Force + " (NG)");
			}
		}

		internal static void GussetWelds(EMemberType memberType, ref double t, double Hforce, double Vforce, ref double wmaxlimit, ref double w1)
		{
			double GussetWelds_w = 0;
			double wmin = 0;
			double fvv = 0;
			double M2 = 0;
			double M1 = 0;
			double eLeft = 0;
			double eRight = 0;
			double L = 0;
			double V = 0;
			double V_02;
			double V_01;
			double V4 = 0;
			double V3 = 0;
			double V2 = 0;
			double V1 = 0;
			double H = 0;
			double H2 = 0;
			double H1 = 0;
			int flg = 0;
			double fpa = 0;
			double minweld = 0;
			double RichardFactor = 0;
			double fraverage = 0;
			double fr = 0;
			double Fb = 0;
			double fh = 0;
			double ffv = 0;
			double weldlength = 0;
			double Gap = 0;
			double Th = 0;
			double R = 0;
			double Moment = 0;

			DetailData component = CommonDataStatic.DetailDataDict[memberType];
			DetailData beam;
			if (memberType == EMemberType.UpperLeft || memberType == EMemberType.LowerLeft)
				beam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			else
				beam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BraceToColumn:
				case EJointConfiguration.BraceToColumnBase:
					if (component.BraceConnect.Gusset.DontConnectBeam)
					{
						component.BraceConnect.Gusset.BeamWeldSize = 0;
						return;
					}
					t = component.BraceConnect.Gusset.Thickness;
					Hforce = Math.Abs(component.BraceConnect.Gusset.Hb);
					Vforce = Math.Abs(component.BraceConnect.Gusset.VerticalForceBeam);
					Moment = Math.Abs(component.BraceConnect.Gusset.Mb);
					R = Math.Sqrt(Math.Pow(Hforce, 2) + Math.Pow(Vforce, 2));

					switch (component.ShapeType)
					{
						case EShapeType.WideFlange:
							if (CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].WebOrientation == EWebOrientation.InPlane)
								Th = component.Shape.d / 2;
							else
								Th = component.Shape.tw / 2;
							break;
						case EShapeType.HollowSteelSection:
							if (CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].WebOrientation == EWebOrientation.InPlane)
								Th = component.Shape.d / 2;
							else
								Th = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.bf;
							break;
					}
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase)
					{
						Gap = component.BraceConnect.BasePlate.CornerClip + component.BraceConnect.Gusset.ColumnSideSetback;
						if (beam.MemberType == EMemberType.LeftBeam)
						{
							component.BraceConnect.BasePlate.LeftEdgeToColumn = component.BraceConnect.Gusset.Hb + component.BraceConnect.Gusset.ColumnSideSetback + Th + component.BraceConnect.BasePlate.Extension;
							weldlength = Math.Min(component.BraceConnect.Gusset.Hb + component.BraceConnect.Gusset.ColumnSideSetback, component.BraceConnect.BasePlate.LeftEdgeToColumn - Th) - Gap;
						}
						else
						{
							component.BraceConnect.BasePlate.RightEdgeToColumn = component.BraceConnect.Gusset.Hb + component.BraceConnect.Gusset.ColumnSideSetback + Th + component.BraceConnect.BasePlate.Extension;
							weldlength = Math.Min(component.BraceConnect.Gusset.Hb + component.BraceConnect.Gusset.ColumnSideSetback, component.BraceConnect.BasePlate.RightEdgeToColumn - Th) - Gap;
						}
					}
					else
					{
						if (component.BraceConnect.Gusset.ColumnSideSetback > beam.EndSetback + beam.WinConnect.Beam.TCopeL)
							Gap = 0;
						else
							Gap = beam.EndSetback + beam.WinConnect.Beam.TCopeL - component.BraceConnect.Gusset.ColumnSideSetback;
						weldlength = component.BraceConnect.Gusset.BeamSide - Gap;
					}

					ffv = Vforce / weldlength;
					fh = Hforce / weldlength;
					Fb = 6 * Moment / Math.Pow(weldlength, 2);
					fr = Math.Sqrt(Math.Pow(ffv + Fb, 2) + Math.Pow(fh, 2));
					fraverage = Math.Sqrt(Math.Pow(ffv + Fb / 2, 2) + Math.Pow(fh, 2));

					Reporting.AddHeader(CommonDataStatic.CommonLists.CompleteMemberList[memberType] + " Gusset to Beam Connection");
					Reporting.AddLine("Horizontal Force on Welds (Hb) = " + Hforce + ConstUnit.Force);
					Reporting.AddLine("Vertical Force on Welds (Vb) = " + Vforce + ConstUnit.Force);
					Reporting.AddLine("Moment on Welds (M) = " + component.BraceConnect.Gusset.Mb + ConstUnit.MomentUnitInch);
					Reporting.AddLine("Weld Length on Each Side of Gusset Plate (L) = " + weldlength + ConstUnit.Length);
					Reporting.AddLine("Average Force on Welds per Unit Length = fraverage");
					Reporting.AddLine("= ((V / L + 3 * M / (L²))² + (H / L)²)^0.5");
					Reporting.AddLine("= ((" + Vforce + " / " + weldlength + " + 3 * " + Moment + " / (" + weldlength + " ²))² + (" + Hforce + " / " + weldlength + ")²)^0.5");
					Reporting.AddLine("= " + fraverage + ConstUnit.ForcePerUnitLength);
					if (Moment == 0)
						Reporting.AddLine("fr = fraverage");
					else
					{
						Reporting.AddLine("Max. Force on Welds per Unit Length = fr");
						Reporting.AddLine("= ((V / L + 6 * M / (L²))² + (H / L)²)^0.5");
						Reporting.AddLine("= ((" + Vforce + " / " + weldlength + " + 6 * " + Moment + " / (" + weldlength + " ^ 2))² + (" + Hforce + " / " + weldlength + ")²)^0.5");
						Reporting.AddLine("= " + fr + ConstUnit.ForcePerUnitLength);
					}

					RichardFactor = 1.25;
					wmaxlimit = 0.7072F * component.BraceConnect.Gusset.Material.Fu * t / CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine("Maximum useful weld size = 0.7072 * Fu * t / Fexx");
					Reporting.AddLine("= 0.7072 * " + component.BraceConnect.Gusset.Material.Fu + " * " + t + " / " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddLine("= " + wmaxlimit + ConstUnit.Length);

					minweld = CommonCalculations.MinimumWeld(beam.Shape.tf, t);
					w1 = Math.Max(RichardFactor * fraverage, fr) / (ConstNum.FIOMEGA0_75N * 0.6 * Math.Sqrt(2) * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					if (w1 > wmaxlimit)
						Reporting.AddLine("Required Weld size exceeds maximum useful weld size. (NG)");

					GussetWelds_w = w1 < minweld ? minweld : w1;

					if (!component.BraceConnect.Gusset.BeamWeldSize_User)
						component.BraceConnect.Gusset.BeamWeldSize = GussetWelds_w;

					flg = 0;
					Reporting.AddLine("Use Richard Factor (Rf) = 1.25");
					fpa = (Math.Max(RichardFactor * fraverage, fr));
					Reporting.AddHeader("Required Weld Size (w) = Max(Rf * f_avrg, f_peak) / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.41 * Fexx) ");
					Reporting.AddLine("= " + fpa + " / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.41 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ") ");
					if (w1 <= component.BraceConnect.Gusset.BeamWeldSize)
						Reporting.AddLine("= " + w1 + " <= " + component.BraceConnect.Gusset.BeamWeldSize + ConstUnit.Length + " (OK)");
					else
					{
						Reporting.AddLine("= " + w1 + " >> " + component.BraceConnect.Gusset.BeamWeldSize + ConstUnit.Length + " (NG)");
						flg = 1;
					}
					if (minweld <= component.BraceConnect.Gusset.BeamWeldSize)
						Reporting.AddLine(" Minimum Weld size = " + minweld + " <= " + component.BraceConnect.Gusset.BeamWeldSize + ConstUnit.Length + " (OK)");
					else
					{
						Reporting.AddLine(" Minimum Weld size = " + minweld + " >> " + component.BraceConnect.Gusset.BeamWeldSize + ConstUnit.Length + " (NG)");
						flg = 1;
					}
					if (w1 > wmaxlimit)
					{
						Reporting.AddLine("Required Weld size exceeds maximum useful weld size. (NG)");
						flg = 1;
					}
					if (flg == 1)
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(component.BraceConnect.Gusset.BeamWeldSize) + ConstUnit.Length + " (NG)");
					else
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(component.BraceConnect.Gusset.BeamWeldSize) + ConstUnit.Length + " (OK)");
					Reporting.AddLine("Gusset to Beam Welds: " + CommonCalculations.WeldSize(GussetWelds_w) + ConstUnit.Length + " Weld");
					if (w1 > wmaxlimit)
						Reporting.AddLine("Required Weld size exceeds maximum useful weld size. (NG)");
					break;
				case EJointConfiguration.BraceVToBeam:
					DetailData secondaryComponent;
					if (memberType == EMemberType.LowerLeft)
						secondaryComponent = CommonDataStatic.DetailDataDict[EMemberType.LowerRight];
					else
						secondaryComponent = CommonDataStatic.DetailDataDict[EMemberType.UpperRight];

					wmaxlimit = 0.7072 * component.BraceConnect.Gusset.Material.Fu * t / CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					if (memberType == EMemberType.UpperLeft || memberType == EMemberType.LowerLeft)
					{
						H1 = component.BraceConnect.Gusset.GussetEFCompression.Hb + secondaryComponent.BraceConnect.Gusset.GussetEFCompression.Hb;
						H2 = secondaryComponent.BraceConnect.Gusset.GussetEFCompression.Hb + component.BraceConnect.Gusset.GussetEFTension.Hb;
						H = Math.Max(H1, H2);
						V1 = component.BraceConnect.Gusset.GussetEFCompression.Vb - component.BraceConnect.Gusset.GussetEFTension.Vb;
						V2 = component.BraceConnect.Gusset.GussetEFCompression.Vb - component.BraceConnect.Gusset.GussetEFTension.Vb;
						V3 = component.BraceConnect.Gusset.GussetEFCompression.Vb + component.BraceConnect.Gusset.GussetEFCompression.Vb;
						V4 = component.BraceConnect.Gusset.GussetEFTension.Vb + component.BraceConnect.Gusset.GussetEFTension.Vb;
						V_01 = Math.Max(V1, V2);
						V_02 = Math.Max(V3, V4);
						V = Math.Max(V_01, V_02);

						// TODO: This needs work for Moment to be calculated. BraceX and BraceY have been added to DetailData
						double ortasi;
						double xRight2 = 0;
						double xLeft2 = 0;
						double dumy = 0;

						//ortasi = (BRACE1.x[(26 + i1)] + BRACE1.x[(36 + i1)]) / 2;
						//L = BRACE1.x[(26 + i1)] - BRACE1.x[(36 + i1)];
						//Intersect(0, BRACE1.x[(25 + i1)], BRACE1.y[(25 + i1)], Math.Tan(beam.Angle * ConstNum.RADIAN), beam.WorkPointX, beam.WorkPointY, ref xRight2, ref dumy);
						//eRight = xRight2 - ortasi;
						//Intersect(0, BRACE1.x[(25 + i1)], BRACE1.y[(25 + i1)], Math.Tan(component.Angle * ConstNum.RADIAN), component.WorkPointX, component.WorkPointY, ref xLeft2, ref dumy);
						//eLeft = ortasi - xLeft2;

						M1 = Math.Abs(eLeft * component.BraceConnect.Gusset.GussetEFTension.Vb - eRight * component.BraceConnect.Gusset.GussetEFCompression.Vb);
						M2 = Math.Abs(eLeft * component.BraceConnect.Gusset.GussetEFTension.Vb - eRight * component.BraceConnect.Gusset.GussetEFCompression.Vb);
						Moment = (Math.Max(M1, M2));
						fh = H / L;
						fvv = V / L;
						Fb = 6 * Moment / (L * L);
						fr = Math.Sqrt(Math.Pow(fh, 2) + Math.Pow(fvv + Fb, 2));
						fraverage = Math.Sqrt(Math.Pow(fh, 2) + Math.Pow(fvv + Fb / 2, 2));
						w1 = Math.Max(1.25 * fraverage, fr) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						wmaxlimit = ConstNum.FIOMEGA1_0N * 0.6 * Math.Min(t * component.BraceConnect.Gusset.Material.Fy, 2 * CommonDataStatic.DetailDataDict[EMemberType.RightBeam].Shape.tf * component.Material.Fy) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						wmin = CommonCalculations.MinimumWeld(t, CommonDataStatic.DetailDataDict[EMemberType.RightBeam].Shape.tf);
						if (w1 < wmin)
							w1 = wmin;

						if (!component.BraceConnect.Gusset.BeamWeldSize_User)
							component.BraceConnect.Gusset.BeamWeldSize = w1;
						if (!secondaryComponent.BraceConnect.Gusset.BeamWeldSize_User)
							secondaryComponent.BraceConnect.Gusset.BeamWeldSize = component.BraceConnect.Gusset.BeamWeldSize;

						if (memberType == EMemberType.UpperLeft)
							Reporting.AddHeader("Upper Gusset to Beam Connection");
						else
							Reporting.AddHeader("Lower Gusset to Beam Connection");

						Reporting.AddLine("Horizontal Force on Welds (H) = " + H + " " + ConstUnit.Force);
						Reporting.AddLine("Vertical Force on Welds (V) = " + V + " " + ConstUnit.Force);
						Reporting.AddLine("Moment on Welds (M) = | eLeft * Vleft - eRight * Vright |");
						Reporting.AddLine("= Abs(" + eLeft + " * " + component.BraceConnect.Gusset.VerticalForceBeam + " - " + eRight + " * " + secondaryComponent.BraceConnect.Gusset.VerticalForceBeam + ")");
						Reporting.AddLine("= " + Moment + ConstUnit.MomentUnitInch);
						Reporting.AddLine("Length of Welds, L = " + L + ConstUnit.Length);
						Reporting.AddLine("Max. Force on Welds per Unit Length = f ");
						Reporting.AddLine("= ((V / L + 6 * M / L²)² + (H / L)²)^0.5");
						Reporting.AddLine("= ((" + V + " / " + L + " + 6 * " + Moment + " / " + L + "²)² + (" + H + " / " + L + ")²)^0.5");
						Reporting.AddLine("= " + fr + ConstUnit.ForcePerUnitLength);

						Reporting.AddLine("Average Force on Welds per Unit Length = fraverage");
						Reporting.AddLine("= ((V / L + 3 * M / L²)² + (H / L)²)^0.5");
						Reporting.AddLine("= ((" + V + " / " + L + " + 3 * " + Moment + " / " + L + "²)² + (" + H + " / " + L + ")²)^0.5");
						Reporting.AddLine("= " + fraverage + ConstUnit.ForcePerUnitLength);

						RichardFactor = 1.25;
						wmaxlimit = ConstNum.FIOMEGA0_75N * 0.6 * Math.Min(component.BraceConnect.Gusset.Thickness * component.BraceConnect.Gusset.Material.Fu, 2 * CommonDataStatic.DetailDataDict[EMemberType.RightBeam].Shape.tf * component.Material.Fu) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						Reporting.AddLine("Maximum useful weld size = " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu * t / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * Fexx)");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.6 * Min(" + component.BraceConnect.Gusset.Thickness + " * " + component.BraceConnect.Gusset.Material.Fu + ", 2 * " + CommonDataStatic.DetailDataDict[EMemberType.RightBeam].Shape.tf + " * " + CommonDataStatic.DetailDataDict[EMemberType.RightBeam].Material.Fu + ") / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
						Reporting.AddLine("= " + wmaxlimit + ConstUnit.Length);

						flg = 0;
						Reporting.AddLine("Use Richard Factor (Rf) = 1.25");
						w1 = Math.Max(1.25 * fraverage, fr) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);

						Reporting.AddLine("Required Weld Size (w) = Max(Rf * fav, f) / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * Fexx)");
						Reporting.AddLine("= " + Math.Max(RichardFactor * fraverage, fr) + " / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ") ");
						if (w1 <= component.BraceConnect.Gusset.BeamWeldSize)
							Reporting.AddLine("= " + w1 + " <= " + component.BraceConnect.Gusset.BeamWeldSize + ConstUnit.Length + " (OK)");
						else
						{
							Reporting.AddLine("= " + w1 + " >> " + component.BraceConnect.Gusset.BeamWeldSize + ConstUnit.Length + " (NG)");
							flg = 1;
						}

						if (wmin <= component.BraceConnect.Gusset.BeamWeldSize)
							Reporting.AddLine(" Minimum Weld size = " + wmin + " <= " + component.BraceConnect.Gusset.BeamWeldSize + ConstUnit.Length + " (OK)");
						else
						{
							Reporting.AddLine(" Minimum Weld size = " + wmin + " >> " + component.BraceConnect.Gusset.BeamWeldSize + ConstUnit.Length + " (NG)");
							flg = 1;
						}

						if (w1 > wmaxlimit)
						{
							Reporting.AddLine("Required Weld size exceeds maximum useful weld size. (NG)");
							flg = 1;
						}
						if (flg == 1)
							Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(component.BraceConnect.Gusset.BeamWeldSize) + ConstUnit.Length + " (NG)");
						else
							Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(component.BraceConnect.Gusset.BeamWeldSize) + ConstUnit.Length + " (OK)");

						BeamWebYieldingAndCripling(memberType);
					}
					break;
			}
		}

		internal static void GussetThickness_ForceEnvelop(EMemberType memberType, ref double BeamHfrc, ref double BeamVfrc, ref double BeamWMom)
		{
			double R_tension;
			double R_compression;

			var component = CommonDataStatic.DetailDataDict[memberType];

			R_tension = Math.Pow(Math.Pow(component.BraceConnect.Gusset.GussetEFTension.Hb, 2) + Math.Pow(component.BraceConnect.Gusset.GussetEFTension.Vb, 2), 0.5);
			R_compression = Math.Pow(Math.Pow(component.BraceConnect.Gusset.GussetEFCompression.Hb, 2) + Math.Pow(component.BraceConnect.Gusset.GussetEFCompression.Vb, 2), 0.5);
			if (R_tension >= R_compression)
			{
				BeamHfrc = component.BraceConnect.Gusset.GussetEFTension.Hb;
				BeamVfrc = component.BraceConnect.Gusset.GussetEFTension.Vb;
				BeamWMom = component.BraceConnect.Gusset.GussetEFTension.Mb;
			}
			else
			{
				BeamHfrc = component.BraceConnect.Gusset.GussetEFCompression.Hb;
				BeamVfrc = component.BraceConnect.Gusset.GussetEFCompression.Vb;
				BeamWMom = component.BraceConnect.Gusset.GussetEFCompression.Mb;
			}
		}

		internal static void GussetShear(EMemberType memberType, ref double t)
		{
			double An;
			double FiRn;
			double t0;
			double VgCap = 0;
			double Vn;
			double Aw;
			double BsCapacity = 0;
			double Anv;
			double Ant;
			double Agv = 0;
			double Agt = 0;
			double Lh;
			double Lv = 0;
			double sigmax;
			double Hmax;
			double Moment;
			double a;
			double S;
			double Ag;
			double H;

			if (!CommonDataStatic.Preferences.UseContinuousClipAngles)
				return;

			DetailData beam;
			DetailData component = CommonDataStatic.DetailDataDict[memberType];
			var webPlate = component.WinConnect.ShearWebPlate;

			if (memberType == EMemberType.UpperLeft || memberType == EMemberType.LowerLeft)
				beam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			else
				beam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			t = component.BraceConnect.Gusset.Thickness;
			H = component.BraceConnect.Gusset.VerticalForceColumn;
			Ag = H * t;
			S = H * H * t / 6;
			a = Ag;

			Moment = Math.Abs(component.WinConnect.ShearClipAngle.ForceY * (beam.EndSetback + beam.WinConnect.Beam.TCopeL));
			sigmax = component.WinConnect.ShearClipAngle.ForceX / a + Moment / S;
			Hmax = (Math.Max(component.AxialTension, component.AxialCompression));
			sigmax = Hmax / a + Moment / S;
			Lh = component.WinConnect.ShearClipAngle.LongLeg + component.WinConnect.ShearClipAngle.ShortLeg - component.WinConnect.ShearClipAngle.LengthOfOSL - component.BraceConnect.Gusset.ColumnSideSetback;

			//switch (component.MemberType)
			//{
			//	case EMemberType.UpperRight:
			//		Lv = BRACE1.y[100] - BRACE1.y[25];
			//		break;
			//	case EMemberType.UpperLeft:
			//		Lv = BRACE1.y[30] - BRACE1.y[105];
			//		break;
			//	case EMemberType.LowerRight:
			//		Lv = BRACE1.y[116] - BRACE1.y[35];
			//		break;
			//	case EMemberType.LowerLeft:
			//		Lv = BRACE1.y[40] - BRACE1.y[121];
			//		break;
			//}

			Agt = Lh * t;
			//Agv = Lv * t;

			// Not sure why we do a check on clip angles here, and use a value from its bolts, when we seem to be dealing with a single plate,
			// but it does match V7. -RM 10/14/14
			if (component.WinConnect.ShearClipAngle.AnglesBoltedToGusset)
			{
				Ant = Agt - 0.5 * webPlate.Bolt.HoleWidth * t;
				Anv = Agv - (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfRows - 0.5) * webPlate.Bolt.HoleWidth * t;
			}
			else
			{
				Ant = Agt;
				Anv = Agv;
			}

			BsCapacity = ConstNum.FIOMEGA0_75N * (0.6 * Math.Min(component.BraceConnect.Gusset.Material.Fy * Agv, component.BraceConnect.Gusset.Material.Fu * Anv) + component.BraceConnect.Gusset.Material.Fu * Ant);

			if (sigmax > ConstNum.FIOMEGA0_9N * component.BraceConnect.Gusset.Material.Fy)
				t = t * sigmax / (ConstNum.FIOMEGA0_9N * component.BraceConnect.Gusset.Material.Fy);
			if (BsCapacity < component.WinConnect.ShearClipAngle.ForceY)
				t = t * component.WinConnect.ShearClipAngle.ForceY / BsCapacity;
			t = CommonDataStatic.Units == EUnit.US ? t / 16.0 : t;

			do
			{
				t0 = t;
				if (VgCap < component.WinConnect.ShearClipAngle.ForceY)
					t += ConstNum.SIXTEENTH_INCH;
			} while (t0 < t);

			Reporting.AddHeader("Gusset Stresses at Beam Cope:");
			Reporting.AddLine("Normal Stress:");
			Moment = Math.Abs(component.WinConnect.ShearClipAngle.ForceY * (beam.EndSetback + beam.WinConnect.Beam.TCopeL));
			Hmax = (Math.Max(component.AxialTension, component.AxialCompression));
			sigmax = Hmax / a + Moment / S;
			Reporting.AddLine("Area (A) = h * t = " + H + " * " + t + " = " + Ag + ConstUnit.Area);
			Reporting.AddLine("Section Modulus (S) = (h²) * t / 6 = (" + H + "²) * " + t + " / 6 = " + S + ConstUnit.SecMod);
			Reporting.AddLine("Moment, M = Vc * e = " + Math.Abs(component.WinConnect.ShearClipAngle.ForceY) + " * " + (beam.EndSetback + beam.WinConnect.Beam.TCopeL) + " = " + Moment + ConstUnit.MomentUnitInch);
			Reporting.AddLine("Stress = H / A + M / S = " + Hmax + " / " + a + " + " + Moment + " / " + S);
			if (sigmax <= ConstNum.FIOMEGA0_9N * component.BraceConnect.Gusset.Material.Fy)
				Reporting.AddLine("= " + sigmax + " <= 0.9 Fy = " + (ConstNum.FIOMEGA0_9N * component.BraceConnect.Gusset.Material.Fy) + " ksi (OK)");
			else
				Reporting.AddLine("= " + sigmax + " >> 0.9 Fy = " + (ConstNum.FIOMEGA0_9N * component.BraceConnect.Gusset.Material.Fy) + " ksi (NG)");

			Reporting.AddHeader("Shear Stress:");
			Reporting.AddLine("Agv = " + Agv + ConstUnit.Area + ", Anv = " + Anv + ConstUnit.Area + ", Ant = " + Ant + ConstUnit.Area);

			Reporting.AddHeader("Block Shear:");
			//  If At >= 0.6 * Av Then
			FiRn = ConstNum.FIOMEGA0_75N * (0.6 * Math.Min(component.BraceConnect.Gusset.Material.Fy * Agv, component.BraceConnect.Gusset.Material.Fu * Anv) + component.BraceConnect.Gusset.Material.Fu * Ant);
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * (0.6 * Min(Fy * Agv, Fu * Anv) + Fu * Ant)");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * (0.6 * Min(" + component.BraceConnect.Gusset.Material.Fy + " * " + Agv + ", " + component.BraceConnect.Gusset.Material.Fu + " * " + Anv + ") + " + component.BraceConnect.Gusset.Material.Fu + " * " + Ant + ")");

			if (FiRn >= component.WinConnect.ShearClipAngle.ForceY)
				Reporting.AddLine("= " + FiRn + " >= " + component.WinConnect.ShearClipAngle.ForceY + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + component.WinConnect.ShearClipAngle.ForceY + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Shear Yielding:");
			Aw = H * t;
			Reporting.AddLine("Ag = h * t = " + H + " * " + t + " = " + Aw + ConstUnit.Area);
			Vn = 0.6 * component.BraceConnect.Gusset.Material.Fy * Aw;
			Reporting.AddLine("Vn = 0.6 * Fy * Ag");
			Reporting.AddLine("= 0.6 * " + component.BraceConnect.Gusset.Material.Fy + " * " + Aw + " = " + Vn + ConstUnit.Force);
			VgCap = ConstNum.FIOMEGA1_0N * Vn;
			Reporting.AddLine(" ");
			if (VgCap >= component.WinConnect.ShearClipAngle.ForceY)
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.FIOMEGA1_0N + " * Vn = " + VgCap + " >= " + component.WinConnect.ShearClipAngle.ForceY + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.FIOMEGA1_0N + " * Vn = " + VgCap + " << " + component.WinConnect.ShearClipAngle.ForceY + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Shear Rupture:");
			An = (H - webPlate.Bolt.NumberOfRows * webPlate.Bolt.HoleWidth) * t;
			Reporting.AddLine("An = (H - N * dh) * t");
			Reporting.AddLine("An = (" + H + " - " + webPlate.Bolt.NumberOfRows + " * " + webPlate.Bolt.HoleWidth + ") * " + t);
			Vn = 0.6 * component.BraceConnect.Gusset.Material.Fu * An;
			Reporting.AddLine("Vn = 0.6 * Fu * An");
			Reporting.AddLine("= 0.6 * " + component.BraceConnect.Gusset.Material.Fu + " * " + An + " = " + Vn + ConstUnit.Force);
			VgCap = ConstNum.FIOMEGA0_75N * Vn;
			if (VgCap >= component.WinConnect.ShearClipAngle.ForceY)
				Reporting.AddLine("FiVn = " + ConstString.FIOMEGA0_75 + " * Vn = " + VgCap + " >= " + component.WinConnect.ShearClipAngle.ForceY + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("FiVn = " + ConstString.FIOMEGA0_75 + " * Vn = " + VgCap + " << " + component.WinConnect.ShearClipAngle.ForceY + ConstUnit.Force + " (NG)");
		}

		internal static int NBRequiredforBraceBlockShear(EMemberType memberType, double t, EShapeType shapeType)
		{
			double FiRn = 0;
			double Ant2 = 0;
			double Ant1 = 0;
			double Anv = 0;
			double Ant = 0;
			double Agv = 0;
			bool Stag = false;
			double et = 0;
			int nl = 0;
			double et1 = 0;
			double et2 = 0;
			double g2 = 0;
			double g1 = 0;
			double g0 = 0;
			double d = 0;
			int N = 0;
			double dT = 0;
			double dl = 0;
			double el = 0;
			double sl = 0;

			var brace = CommonDataStatic.DetailDataDict[memberType];

			sl = brace.BoltBrace.SpacingLongDir;
			el = brace.BoltBrace.EdgeDistLongDir;
			dl = brace.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH;
			dT = brace.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH;
			N = 1;

			switch (shapeType)
			{
				case EShapeType.SingleAngle:
				case EShapeType.DoubleAngle:
					d = brace.Shape.d;
					g0 = CommonCalculations.AngleStandardGage(0, d);
					g1 = CommonCalculations.AngleStandardGage(1, d);
					g2 = CommonCalculations.AngleStandardGage(2, d);
					et2 = d - g1 - g2;
					et1 = d - g0;
					break;
				case EShapeType.WTSection:
					d = brace.Shape.bf;
					g1 = brace.GageOnFlange;
					g2 = Math.Min(ConstNum.THREE_INCHES, NumberFun.Round((brace.Shape.bf - g1) / 2 - (brace.BoltBrace.MinEdgeRolled + brace.BoltBrace.Eincr), 4));
					if (g2 < NumberFun.ConvertFromFraction(28))
						g2 = 0;
					et1 = (brace.Shape.bf - g1) / 2;
					et2 = et1 - g2;
					break;
				case EShapeType.SingleChannel:
					break;
			}
			do
			{
				N++;
				if (N > 3 && g2 > 0)
				{
					nl = 2;
					et = et2;
					Stag = g2 < 2.667 * brace.BoltBrace.BoltSize;
				}
				else
				{
					nl = 1;
					et = et1;
				}

				if (nl == 1)
				{
					Agv = (el + (N - 1) * sl) * t;
					Ant = (et - dT / 2) * t;
					Anv = (el + (N - 1) * sl - (N - 0.5) * dl) * t;
				}
				else if (nl == 2 && Stag)
				{
					Ant1 = (et + g2 - dT / 2) * t;
					Ant2 = (et + g2 - dT / 2) * t + (Math.Pow(sl / 2, 2) / (4 * g2) - dT) * t;
					Ant = Math.Min(Ant1, Ant2);
					if (N % 2 == 0)
					{
						Anv = (el + sl / 2 + (N / 2.0 - 1) * sl - (N / 2.0 - 0.5) * dl) * t;
						Agv = (el + sl / 2 + (N / 2.0 - 1) * sl) * t;
					}
					else
					{
						Anv = (el + ((N + 1) / 2.0 - 1) * sl - ((N + 1) / 2.0 - 0.5) * dl) * t;
						Agv = (el + ((N + 1) / 2.0 - 1) * sl) * t;
					}
				}
				else
				{
					if (N % 2 == 0)
					{
						Ant = (et + g2 - 1.5 * dT) * t;
						Anv = (el + (N / 2 - 1) * sl - (N / 2.0 - 0.5) * dl) * t;
						Agv = (el + (N / 2 - 1) * sl) * t;
					}
					else
					{
						Ant1 = (et + g2 - dT / 2) * t;
						Ant2 = (et + g2 + Math.Pow(sl, 2) / (4 * g2) - 1.5 * dT) * t;
						Ant = Math.Min(Ant1, Ant2);
						Anv = (el + ((N + 1) / 2 - 1) * sl - ((N + 1) / 2.0 - 0.5) * dl) * t;
						Anv = (el + ((N + 1) / 2 - 1) * sl) * t;
					}
				}

				FiRn = ConstNum.FIOMEGA0_75N * (0.6 * Math.Min(brace.Material.Fu * Anv, brace.Material.Fy * Agv) + 1 * brace.Material.Fu * Ant);

			} while (FiRn < brace.AxialTension);

			return N;
		}

		internal static int NBRequiredForGussetBlockShear(EMemberType memberType, int flag, double N)
		{
			double NBRequiredForGussetBlockShear = 0;

			double Rn;
			double t0;
			double Ant2;
			double Agt2 = 0;
			double Ant1;
			double Agt1 = 0;
			double g1 = 0;
			double Ant = 0;
			double Agt = 0;
			double Anv = 0;
			double Agv = 0;
			double n1;
			double g2;
			double d;
			double np;

			var brace = CommonDataStatic.DetailDataDict[memberType];

			// flag=0  Returns NB w/o report
			// flag=1  Reports calculation
			// flag=2  returns thickness required for np=n
			if (brace.AxialTension <= 0)
			{
				NBRequiredForGussetBlockShear = 0;
				return (int)Math.Ceiling(NBRequiredForGussetBlockShear);
			}
			if (flag == 0 || flag == 2)
			{
				np = N - 1;
				d = brace.ShapeType == EShapeType.WTSection ? brace.Shape.bf : brace.Shape.d;

				do
				{
					np++;
					if (brace.ShapeType == EShapeType.HollowSteelSection && brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Welded)
					{
						NBRequiredForGussetBlockShear = N;
						return (int)Math.Ceiling(NBRequiredForGussetBlockShear);
					}
					else if ((brace.ShapeType == EShapeType.SingleAngle || brace.ShapeType == EShapeType.DoubleAngle) && brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
					{
						g2 = CommonCalculations.AngleStandardGage(2, d);

						if (np > 3 && d > ConstNum.FOUR_INCHES)
						{
							if (brace.BoltBrace.NumberOfRows == 0)
								brace.BoltBrace.NumberOfRows = 2;
							if (g2 < 2.667 * brace.BoltBrace.BoltSize && brace.BoltBrace.NumberOfRows == 2)
								brace.BraceConnect.Brace.BoltsAreStaggered = true;
							else
								brace.BraceConnect.Brace.BoltsAreStaggered = false;
						}
						else
						{
							if (brace.BoltBrace.NumberOfRows == 0)
								brace.BoltBrace.NumberOfRows = 1;
						}
						if (brace.BoltBrace.NumberOfRows == 2 && flag > 0)
						{
							if (np % 2 == 0 && brace.BraceConnect.Brace.BoltsAreStaggered)
							{
								n1 = np / 2.0;

								Agv = (2 * ((n1 - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset) + brace.BoltBrace.SpacingLongDir / 2);
								Anv = Agv - 2 * (n1 - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH);
								Agt = g2 + Math.Pow(brace.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2);
								Ant = Agt - (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH);
							}
							else if (brace.BraceConnect.Brace.BoltsAreStaggered)
							{
								n1 = Math.Floor(np / 2.0) + 1;
								Agv = ((2 * n1 - 3) * brace.BoltBrace.SpacingLongDir + 2 * brace.BoltBrace.EdgeDistGusset + brace.BoltBrace.SpacingLongDir / 2);
								Anv = Agv - (2 * n1 - 2) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH);
								Agt = g2 + Math.Pow(brace.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2);
								Ant = Agt - (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH);
							}
							else if (np % 2 == 0)
							{
								n1 = np / 2.0;
								Agv = 2 * ((n1 - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset);
								Anv = Agv - 2 * (n1 - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH);
								Agt = g2;
								Ant = Agt - (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH);
							}
							else
							{
								n1 = Math.Floor(np / 2.0) + 1;
								Agv = 2 * (n1 - 1) * brace.BoltBrace.SpacingLongDir + 2 * brace.BoltBrace.EdgeDistGusset;
								Anv = Agv - (2 * n1 - 2) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH);
								Agt = g2;
								Ant = Agt - (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH);
							}
						}
					}
					else if (brace.ShapeType == EShapeType.WTSection)
					{
						// WT

						g1 = brace.GageOnFlange;

						g2 = Math.Min(ConstNum.THREE_INCHES, Math.Floor(4 * ((brace.Shape.bf - g1) / 2 - (brace.BoltBrace.MinEdgeRolled + brace.BoltBrace.Eincr)))) / 4;
						if (brace.BoltBrace.NumberOfRows == 2)
						{
							if (np % 2 == 0 && brace.BraceConnect.Brace.BoltsAreStaggered)
							{
								n1 = np / 2.0;
								Agv = 2 * ((n1 - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset + brace.BoltBrace.SpacingLongDir / 2);
								Anv = Agv - 2 * (n1 - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH);
								Agt1 = g1 + 2 * g2 + 2 * Math.Pow(brace.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2);
								Ant1 = Agt1 - 3 * (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH);
								Agt2 = g1 + 2 * g2;
								Ant2 = Agt2 - (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH);
								Agt = Math.Min(Agt1, Agt2);
								Ant = Math.Min(Ant1, Ant2);
							}
							else if (brace.BraceConnect.Brace.BoltsAreStaggered)
							{
								n1 = Math.Floor(np / 2.0);
								Agv = 2 * ((n1 - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset + brace.BoltBrace.SpacingLongDir / 2);
								Anv = Agv - 2 * (n1 - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH);
								Agt = g1 + 2 * g2 + 2 * Math.Pow(brace.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2);
								Ant = Agt - 3 * (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH);
							}
							else if (np % 2 == 0)
							{
								n1 = np / 2.0;
								Agv = 2 * ((n1 - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset);
								Anv = Agv - 2 * (n1 - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH);
								Agt = g1 + 2 * g2;
								Ant = Agt - 3 * (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH);
							}
							else
							{
								n1 = Math.Floor(np / 2);
								Agv = 2 * ((n1 - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset + brace.BoltBrace.SpacingLongDir);
								Anv = Agv - 2 * (n1 - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH);
								Agt = g1 + 2 * g2;
								Ant = Agt - 3 * (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH);
							}
						}
						else
						{
							Agv = 2 * ((np - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset);
							Anv = Agv - (np - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH);
							Agt = g1;
							Ant = Agt - (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH);
						}
					}
					else if (brace.ShapeType == EShapeType.SingleChannel || brace.ShapeType == EShapeType.DoubleChannel)
					{
						// C
						n1 = (np / brace.BoltBrace.NumberOfRows);
						Agv = 2 * ((n1 - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset);
						Anv = Agv - 2 * (n1 - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH);
						Agt = (brace.BoltBrace.NumberOfRows - 1) * brace.BoltBrace.SpacingTransvDir;
						Ant = Agt - (brace.BoltBrace.NumberOfRows - 1) * (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH);
					}

					if (Agv.Equals(0.0) && Agt.Equals(0.0))
						t0 = 0;
					else
					{
						Rn = 0.6 * Math.Min(brace.BraceConnect.Gusset.Material.Fu * Anv, brace.BraceConnect.Gusset.Material.Fy * Agv) + 1 * brace.BraceConnect.Gusset.Material.Fu * Ant;
						t0 = brace.AxialTension / (ConstNum.FIOMEGA0_75N * Rn);
					}
				} while (t0 > brace.BraceConnect.Gusset.Thickness && brace.BraceConnect.Gusset.Thickness > 0 && flag == 0);

				if (flag == 0)
					NBRequiredForGussetBlockShear = np;
				else
					NBRequiredForGussetBlockShear = t0;
			}
			else if (brace.AxialTension > 0)
			{
				NBRequiredForGussetBlockShear = brace.BoltBrace.NumberOfBolts;
				np = brace.BoltBrace.NumberOfBolts;

				if (brace.ShapeType == EShapeType.HollowSteelSection && brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Welded)
				{
					Agv = 2 * brace.BraceConnect.Brace.BraceWeld.Weld1L * brace.BraceConnect.Gusset.Thickness;
					Anv = Agv;

					d = brace.ShapeType == EShapeType.WTSection ? brace.Shape.bf : brace.Shape.d;

					Agt = d * brace.BraceConnect.Gusset.Thickness;
					Ant = Agt;

					Reporting.AddHeader("Block Shear of Gusset at Brace");

					Reporting.AddLine("Agv = Anv = 2 * L * t");
					Reporting.AddLine(Agv + " = 2 * " + brace.BraceConnect.Brace.BraceWeld.Weld1L + " * " + brace.BraceConnect.Gusset.Thickness);

					Reporting.AddLine("Agt = Ang = d * t");
					Reporting.AddLine(Agt + " = " + d + " * " + brace.BraceConnect.Gusset.Thickness);
				}
				else
				{
					if ((brace.ShapeType == EShapeType.SingleAngle || brace.ShapeType == EShapeType.DoubleAngle) && brace.BoltBrace.NumberOfRows == 1)
						return (int)Math.Floor(NBRequiredForGussetBlockShear);

					CommonDataStatic.DetailReportLineList.Add(new ReportLine {LineType = EReportLineType.Header, LineString = "Block Shear of Gusset at Brace"});

					if (brace.ShapeType == EShapeType.SingleAngle || brace.ShapeType == EShapeType.DoubleAngle)
					{
						g2 = CommonCalculations.AngleStandardGage(2, brace.Shape.d);

						if (brace.BoltBrace.NumberOfRows == 2)
						{
							if (np % 2 == 0 && brace.BraceConnect.Brace.BoltsAreStaggered)
							{
								n1 = (np / 2.0);

								Agv = 2 * ((n1 - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset + brace.BoltBrace.SpacingLongDir / 2) * brace.BraceConnect.Gusset.Thickness;
								Anv = Agv - 2 * (n1 - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
								Agt = (g2 + Math.Pow(brace.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2)) * brace.BraceConnect.Gusset.Thickness;
								Ant = Agt - (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;

								Reporting.AddLine("Agv = 2 * ((n1 - 1) * sl + eg + sl / 2) * t");
								Reporting.AddLine(String.Format("{0} = 2 * (({1} - 1) * {2} + {3} + {4} / 2) * {5}",
									Agv, n1, brace.BoltBrace.SpacingLongDir, brace.BoltBrace.EdgeDistGusset, brace.BoltBrace.SpacingLongDir, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine(String.Format("Anv = Agv - 2 * (n1 - 0.5) * (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} = {1} - 2 * ({2} - 0.5) * ({3} + {4}) * {5}",
									Anv, Agv, n1, brace.BraceConnect.Gusset.HoleLongP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine("Agt = (g2 + (sl / 2)²) / (4 * g2)) * t");
								Reporting.AddLine(String.Format("{0} = ({1} + ({2} / 2)² / 4 * {3})) * {4}",
									Agt, g2, brace.BoltBrace.SpacingLongDir, g2, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine(String.Format("Ant = Agt - (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} = {1} - ({2} + {3} * {4}",
									Ant, Agt, brace.BraceConnect.Gusset.HoleTransP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));
							}
							else if (brace.BraceConnect.Brace.BoltsAreStaggered)
							{
								n1 = Math.Floor(np / 2.0) + 1;

								Agv = ((2 * n1 - 3) * brace.BoltBrace.SpacingLongDir + 2 * brace.BoltBrace.EdgeDistGusset + brace.BoltBrace.SpacingLongDir / 2) * brace.BraceConnect.Gusset.Thickness;
								Ant = Agt - (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
								Anv = Agv - (2 * n1 - 2) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
								Agt = (g2 + Math.Pow(brace.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2)) * brace.BraceConnect.Gusset.Thickness;

								Reporting.AddLine("Agv = ((2 * n1 - 3) * sl + 2 * eg + sl / 2) * t");
								Reporting.AddLine(String.Format("{0} = ((2 * {1} - 3) * {2} + 2 * {3} + {4} / 2) * {5}",
									Agv, n1, brace.BoltBrace.SpacingLongDir, brace.BoltBrace.EdgeDistGusset, brace.BoltBrace.SpacingLongDir, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine(String.Format("Anv = Agv - (2 * n1 - 2) * (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} = {1} - (2 * {2} - 2) * ({3} + {4}) * {5}",
									Anv, Agv, n1, brace.BraceConnect.Gusset.HoleLongP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine("Agt = (g2 + (sl / 2)² /(4 * g2)) * t");
								Reporting.AddLine(String.Format("{0} = ({1} + ({2} / 2)² / (4 * {3})) * {4}",
									Agt, g2, brace.BoltBrace.SpacingLongDir, g2, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine(String.Format("Ant = Agt - (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} = {1} - ({2} + {3}) * {4}",
									Ant, Agt, brace.BraceConnect.Gusset.HoleTransP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));

							}
							else if (np % 2 == 0)
							{
								n1 = np / 2.0;

								Agv = 2 * ((n1 - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset) * brace.BraceConnect.Gusset.Thickness;
								Anv = Agv - 2 * (n1 - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
								Agt = g2 * brace.BraceConnect.Gusset.Thickness;
								Ant = Agt - (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;

								Reporting.AddLine("Agv = 2 * ((n1 - 1) * sl + eg) * t");
								Reporting.AddLine(String.Format("{0} = 2 * (({1} - 1) * {2} + {3}) * {4}",
									Agv, n1, brace.BoltBrace.SpacingLongDir, brace.BoltBrace.EdgeDistGusset, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine(String.Format("Anv = Agv - 2 * (n1 - 0.5) * (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} = {1} - 2 * ({2} - 0.5) * ({3} + {4} * {5}",
									Anv, Agv, n1, brace.BraceConnect.Gusset.HoleLongP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine("Agt = g2 * t");
								Reporting.AddLine(String.Format("{0} = {1} * {2}", Agt, g2, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine("Ant = Agt - (dh + {0}) * t");
								Reporting.AddLine(String.Format("{0} = {1} - ({2} + {3}) * {4}",
									Ant, Agt, brace.BraceConnect.Gusset.HoleTransP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));
							}
							else
							{
								n1 = Math.Floor(np / 2.0) + 1;

								Agv = 2 * ((n1 - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset) * brace.BraceConnect.Gusset.Thickness;
								Anv = Agv - 2 * (n1 - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
								Agt = g2 * brace.BraceConnect.Gusset.Thickness;
								Ant = Agt - (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;

								Reporting.AddLine("Agv = 2 * ((n1 - 1) * sl + eg) * t");
								Reporting.AddLine(String.Format("{0} = 2 * (({1} - 1) * {2} + {3}) * {4}",
									Agv, n1, brace.BoltBrace.SpacingLongDir, brace.BoltBrace.EdgeDistGusset, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine(String.Format("Anv = Agv - 2 * (n1 - 0.5) * (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} = {1} - 2 * ({2} - 0.5) * ({3} + {4} * {5}",
									Anv, Agv, n1, brace.BraceConnect.Gusset.HoleLongP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine("Agt = g2 * t");
								Reporting.AddLine(String.Format("{0} = {1} * {2}", Agt, g2, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine("Ant = Agt - (dh + {0}) * t");
								Reporting.AddLine(String.Format("{0} = {1} - ({2} + {3}) * {4}",
									Ant, Agt, brace.BraceConnect.Gusset.HoleTransP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));
							}
						}
					}
					else if (brace.ShapeType == EShapeType.WTSection) // else if (BRACE1.SectionTypeIndex[m] == 1)
					{
						g1 = brace.GageOnFlange;
						g2 = Math.Min(ConstNum.THREE_INCHES, (Math.Floor(4 * ((brace.Shape.bf - g1) / 2 - (brace.BoltBrace.MinEdgeRolled + brace.BoltBrace.Eincr)))) / 4);

						if (brace.BoltBrace.NumberOfRows == 2) // if (component.BraceBolt.NumRowsOfBolts == 2)
						{
							if (np % 2 == 0 && brace.BraceConnect.Brace.BoltsAreStaggered) // if (np % 2 == 0 && BRACE1.BraceBoltsAreStaggered[m])
							{
								n1 = np / 2.0;

								Agv = 2 * ((n1 - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset + brace.BoltBrace.SpacingLongDir / 2) * brace.BraceConnect.Gusset.Thickness;
								Anv = Agv - 2 * (n1 - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
								Agt1 = (g1 + 2 * g2 + 2 * Math.Pow(brace.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2)) * brace.BraceConnect.Gusset.Thickness;
								Ant1 = Agt1 - 3 * (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
								Agt2 = (g1 + 2 * g2) * brace.BraceConnect.Gusset.Thickness;
								Ant2 = Agt2 - (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
								Agt = Math.Min(Agt1, Agt2);
								Ant = Math.Min(Ant1, Ant2);

								Reporting.AddLine("Agv = 2 * ((n1 - 1) * sl + eg) * t");
								Reporting.AddLine(String.Format("{0} = 2 * (({1} - 1) * {2} + {3}) * {4}",
									Agv, n1, brace.BoltBrace.SpacingLongDir, brace.BoltBrace.EdgeDistGusset, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine(String.Format("Anv = Agv - 2 * (n1 - 0.5) * (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} = {1} - 2 * ({2} - 0.5) * ({3} + {4} * {5}",
									Anv, Agv, n1, brace.BraceConnect.Gusset.HoleLongP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine("Agt1 = (g1 + 2 * g2 + 2 * (sl / 2)² /(4 * g2)) * t");
								Reporting.AddLine(String.Format("{0} = ({1} + 2 * {2} * ({3} / 2)² / (4 * {4})) * {5}",
									Agt1, g1, g2, brace.BoltBrace.SpacingLongDir, g2, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine(String.Format("Ant1 = Agt1 - 3 * (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} = {1} - 3 * ({2} + {3}) * {4}",
									Ant1, Agt1, brace.BraceConnect.Gusset.HoleTransP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine("Agt2 = (g1 + 2 * g2) * t");
								Reporting.AddLine(String.Format("{0} = ({1} + 2 * {2}) * {3}", Agt2, g1, g2, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine(String.Format("Ant2 = Agt2 - (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} = {1} - ({2} + {3}) * {4}",
									Ant2, Agt2, brace.BraceConnect.Gusset.HoleTransP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine("Agt = Min(Agt1, Agt2)");
								Reporting.AddLine(String.Format("{0} = Min({1}, {2})", Agt, Agt1, Agt2));

								Reporting.AddLine("Ant = Min(Ant1, Ant2)");
								Reporting.AddLine(String.Format("{0} = Min({1}, {2})", Ant, Ant1, Ant2));
							}
							else if (brace.BraceConnect.Brace.BoltsAreStaggered) // else if (BRACE1.BraceBoltsAreStaggered[m] != 0)
							{
								n1 = Math.Floor(np / 2.0);

								Agv = 2 * ((n1 - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset + brace.BoltBrace.SpacingLongDir / 2) * brace.BraceConnect.Gusset.Thickness;
								Anv = Agv - 2 * (n1 - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
								Agt = (g1 + 2 * g2 + 2 * Math.Pow(brace.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2)) * brace.BraceConnect.Gusset.Thickness;
								Ant = Agt - 3 * (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;

								Reporting.AddLine("Agv = 2 * ((n1 - 1) * sl + eg + sl / 2) * t");
								Reporting.AddLine(String.Format("{0} = 2 * (({1} - 1) * {2} + {3} + {4} / 2) * {5}",
									Agv, n1, brace.BoltBrace.SpacingLongDir, brace.BoltBrace.EdgeDistGusset, brace.BoltBrace.SpacingLongDir, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine(String.Format("Anv = Agv - 2 * (n1 - 0.5) * (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} = {1} - 2 * ({2} - 0.5) * ({3} + {4} * {5}",
									Anv, Agv, n1, brace.BraceConnect.Gusset.HoleLongP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine("Agt1 = (g1 + 2 * g2 + 2 * (sl / 2)² /(4 * g2)) * t");
								Reporting.AddLine(String.Format("{0} = ({1} + 2 * {2} * ({3} / 2)² / (4 * {4})) * {5}",
									Agt1, g1, g2, brace.BoltBrace.SpacingLongDir, g2, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine(String.Format("Ant = Agt - 3 * (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} = {1} - 3 * ({2} + {3}) * {4}",
									Ant, Agt, brace.BraceConnect.Gusset.HoleTransP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));
							}
							else if (np % 2 == 0)
							{
								n1 = np / 2.0;

								Agv = 2 * ((n1 - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset) * brace.BraceConnect.Gusset.Thickness;
								Anv = Agv - 2 * (n1 - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
								Agt = (g1 + 2 * g2) * brace.BraceConnect.Gusset.Thickness;
								Ant = Agt - 3 * (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;

								Reporting.AddLine("Agv = 2 * ((n1 - 1) * sl + eg) * t");
								Reporting.AddLine(String.Format("{0} = 2 * (({1} - 1) * {2} + {3}) * {4}",
									Agv, n1, brace.BoltBrace.SpacingLongDir, brace.BoltBrace.EdgeDistGusset, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine(String.Format("Anv = Agv - 2 * (n1 - 0.5) * (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} = {1} - 2 * ({2} - 0.5) * ({3} + {4} * {5}",
									Anv, Agv, n1, brace.BraceConnect.Gusset.HoleLongP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine("Agt2 = (g1 + 2 * g2) * t");
								Reporting.AddLine(String.Format("{0} = ({1} + 2 * {2}) * {3}", Agt2, g1, g2, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine(String.Format("Ant = Agt - 3 * (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} = {1} - 3 * ({2} + {3}) * {4}",
									Ant, Agt, brace.BraceConnect.Gusset.HoleTransP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));
							}
							else
							{
								n1 = Math.Floor(np / 2.0);

								Agv = 2 * ((n1 - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset + brace.BoltBrace.SpacingLongDir) * brace.BraceConnect.Gusset.Thickness;
								Anv = Agv - 2 * (n1 - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
								Agt = (g1 + 2 * g2) * brace.BraceConnect.Gusset.Thickness;
								Ant = Agt - 3 * (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;

								Reporting.AddLine("Agv = 2 * ((n1 - 1) * sl + eg) * t");
								Reporting.AddLine(String.Format("{0} = 2 * (({1} - 1) * {2} + {3}) * {4}",
									Agv, n1, brace.BoltBrace.SpacingLongDir, brace.BoltBrace.EdgeDistGusset, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine(String.Format("Anv = Agv - 2 * (n1 - 0.5) * (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} = {1} - 2 * ({2} - 0.5) * ({3} + {4} * {5}",
									Anv, Agv, n1, brace.BraceConnect.Gusset.HoleLongP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine("Agt2 = (g1 + 2 * g2) * t");
								Reporting.AddLine(String.Format("{0} = ({1} + 2 * {2}) * {3}", Agt2, g1, g2, brace.BraceConnect.Gusset.Thickness));

								Reporting.AddLine(String.Format("Ant = Agt - 3 * (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} = {1} - 3 * ({2} + {3}) * {4}",
									Ant, Agt, brace.BraceConnect.Gusset.HoleTransP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));
							}
						}
						else
						{
							Agv = 2 * ((np - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset) * brace.BraceConnect.Gusset.Thickness;
							Anv = Agv - (np - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
							Agt = g1 * brace.BraceConnect.Gusset.Thickness;
							Ant = Agt - (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;

							Reporting.AddLine("Agv = 2 * ((np - 1) * sl + eg) * t");
							Reporting.AddLine(String.Format("{0} = 2 * (({1} - 1) * {2} + {3}) * {4}",
								Agv, np, brace.BoltBrace.SpacingLongDir, brace.BoltBrace.EdgeDistGusset, brace.BraceConnect.Gusset.Thickness));

							Reporting.AddLine(String.Format("Anv = Agv - 2 * (np - 0.5) * (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
							Reporting.AddLine(String.Format("{0} = {1} - 2 * ({2} - 0.5) * ({3} + {4} * {5}",
								Anv, Agv, np, brace.BraceConnect.Gusset.HoleLongP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));

							Reporting.AddLine("Agt = g1 * t");
							Reporting.AddLine(String.Format("{0} = {1} * {2}", Agt, g1, brace.BraceConnect.Gusset.Thickness));

							Reporting.AddLine(String.Format("Ant = Agt - (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
							Reporting.AddLine(String.Format("{0} = {1} - ({2} + {3}) * {4}",
								Ant, Agt, brace.BraceConnect.Gusset.HoleTransP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));
						}
					}
					else if (brace.ShapeType == EShapeType.SingleChannel || brace.ShapeType == EShapeType.DoubleChannel) // else if (BRACE1.SectionTypeIndex[m] == 5 || BRACE1.SectionTypeIndex[m] == 6)
					{
						// C
						n1 = np / brace.BoltBrace.NumberOfRows;

						Agv = 2 * ((n1 - 1) * brace.BoltBrace.SpacingLongDir + brace.BoltBrace.EdgeDistGusset) * brace.BraceConnect.Gusset.Thickness;
						Anv = Agv - 2 * (n1 - 0.5) * (brace.BraceConnect.Gusset.HoleLongP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;

						Reporting.AddLine("Agv = 2 * ((n1 - 1) * sl + eg) * t");
						Reporting.AddLine(String.Format("{0} = 2 * (({1} - 1) * {2} + {3}) * {4}",
							Agv, n1, brace.BoltBrace.SpacingLongDir, brace.BoltBrace.EdgeDistGusset, brace.BraceConnect.Gusset.Thickness));

						Reporting.AddLine(String.Format("Anv = Agv - 2 * (n1 - 0.5) * (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
						Reporting.AddLine(String.Format("{0} = {1} - 2 * ({2} - 0.5) * ({3} + {4} * {5}",
							Anv, Agv, n1, brace.BraceConnect.Gusset.HoleLongP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));

						Agt = (brace.BoltBrace.NumberOfRows - 1) * brace.BoltBrace.SpacingTransvDir * brace.BraceConnect.Gusset.Thickness;
						// Agt = (component.BraceBolt.NumRowsOfBolts - 1) * BRACE1.BraceBolts[m].st * component.BraceConnect.Gusset.Thickness;

						Reporting.AddLine("Agt = (Nt - 1) * st * t");
						Reporting.AddLine(String.Format("{0} = ({1} - 1) * {2} * {3}",
							Agt, brace.BoltBrace.NumberOfRows, brace.BoltBrace.SpacingTransvDir, brace.BraceConnect.Gusset.Thickness));

						Ant = Agt - (brace.BoltBrace.NumberOfRows - 1) * (brace.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.Gusset.Thickness;
						// Ant = Agt - (component.BraceBolt.NumRowsOfBolts - 1) * (component.BraceConnect.Gusset.HoleTransP + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;

						Reporting.AddLine(String.Format("Ant = Agt - (Nt - 1) * (dh + {0}) * t", ConstNum.SIXTEENTH_INCH));
						Reporting.AddLine(String.Format("{0} = {1} - ({2} - 1) * ({3} + {4}) * {5}",
							Ant, Agt, brace.BoltBrace.NumberOfRows, brace.BraceConnect.Gusset.HoleTransP, ConstNum.SIXTEENTH_INCH, brace.BraceConnect.Gusset.Thickness));
					}
				}
			}
			return (int)Math.Floor(NBRequiredForGussetBlockShear);
		}

		internal static void WhitmoreCheckWithMoment(EMemberType memberType, double Lcr, double WhitmoreLw, double WhitmoreB, double WhitmoreC)
		{
			double UtilizationFactor;
			double Fcr = 0;
			double Pn;
			double KLoR = 0;
			double r_comb;
			double Mu;
			double ex;
			double Pu;
			double Ag;
			double Mn;
			double bt3;
			double bt;

			var component = CommonDataStatic.DetailDataDict[memberType];

			Reporting.AddHeader("Buckling Check with Load Eccentricity:");
			if (component.WebOrientation == EWebOrientation.InPlane)
			{
				bt = WhitmoreLw * component.BraceConnect.Gusset.Thickness + WhitmoreB * component.Shape.tw + WhitmoreC * component.Shape.tw;
				bt = WhitmoreLw * component.BraceConnect.Gusset.Thickness + WhitmoreB * component.Shape.tw + WhitmoreC * component.Shape.tw;
				bt3 = WhitmoreLw * Math.Pow(component.BraceConnect.Gusset.Thickness, 3) + WhitmoreB * Math.Pow(component.Shape.tw, 3) + WhitmoreC * Math.Pow(component.Shape.tw, 3);
				Mn = (component.BraceConnect.Gusset.Material.Fy * WhitmoreLw * Math.Pow(component.BraceConnect.Gusset.Thickness, 2) + component.Material.Fy * WhitmoreB * Math.Pow(component.Shape.tw, 2) + component.Material.Fy * WhitmoreC * Math.Pow(component.Shape.tw, 2)) / 4;
			}
			else
			{
				bt = WhitmoreLw * component.BraceConnect.Gusset.Thickness + WhitmoreB * component.Shape.tw;
				bt3 = WhitmoreLw * Math.Pow(component.BraceConnect.Gusset.Thickness, 3) + WhitmoreB * Math.Pow(component.Shape.tw, 3);
				Mn = (component.BraceConnect.Gusset.Material.Fy * WhitmoreLw * Math.Pow(component.BraceConnect.Gusset.Thickness, 2) + component.Material.Fy * WhitmoreB * Math.Pow(component.Shape.tw, 2)) / 4;
			}

			Ag = bt;
			Pu = component.AxialCompression;
			ex = (component.BraceConnect.Gusset.Thickness + component.BraceConnect.SplicePlates.Thickness) / 2;
			Mu = Pu * ex / 2;
			r_comb = Math.Sqrt(bt3 / (12 * bt));
			if (double.IsNaN(r_comb))
				r_comb = 0;
			else
				KLoR = Lcr / r_comb;

			Reporting.AddLine("Ag = " + Ag + ConstUnit.Area);
			Reporting.AddLine("ex = (ts + tg) / 2 = (" + component.BraceConnect.SplicePlates.Thickness + " * " + component.BraceConnect.Gusset.Thickness + ") / 2  = " + ex + ConstUnit.Length);
			Reporting.AddLine("Mu =Pu * ex /2 = " + Pu + " * " + ex + " / 2  = " + Mu + ConstUnit.MomentUnitInch);
			Reporting.AddLine("Mn =(Fy * W * t²) / 4 = (" + component.BraceConnect.SplicePlates.Material.Fy + " * " + component.BraceConnect.SplicePlates.Width + " * " + component.BraceConnect.SplicePlates.Thickness + "²) / 4  = " + Mn + ConstUnit.MomentUnitInch);
			Reporting.AddLine("KL / r = Lcr / r  = " + Lcr + " / " + r_comb + " = " + KLoR);
			Pn = Ag * CommonCalculations.BucklingStress(KLoR, component.BraceConnect.Gusset.Material.Fy, true);
			Reporting.AddLine("Pn = Ag * Fcr = " + Ag + " * " + Fcr + " = " + Pn + ConstUnit.Force);
			if (Pu / (ConstNum.FIOMEGA0_9N * Pn) >= 0.2)
				UtilizationFactor = Pu / (ConstNum.FIOMEGA0_9N * Pn) + 8 / 9.0 * Mu / (ConstNum.FIOMEGA0_9N * Mn);
			else
				UtilizationFactor = Pu / (2 * ConstNum.FIOMEGA0_9N * Pn) + Mu / (ConstNum.FIOMEGA0_9N * Mn);

			if (double.IsNaN(UtilizationFactor))
				UtilizationFactor = 0;

			Reporting.AddHeader("Utilization Factor:");
			if (Pu / (ConstNum.FIOMEGA0_9N * Pn) >= 0.2)
			{
				Reporting.AddLine("Pu / (" + ConstString.FIOMEGA0_9 + " * Pn) >= 0.2");
				Reporting.AddLine("Pu / (" + ConstString.FIOMEGA0_9 + " * Pn) + 8 / 9 * Mu / (" + ConstString.FIOMEGA0_9 + " * Mn)");
				Reporting.AddLine("= " + Pu + " / (" + ConstString.FIOMEGA0_9 + " * " + Pn + ") + 8 / 9 * " + Mu + " / (" + ConstString.FIOMEGA0_9 + " * " + Mn + ")");
			}
			else
			{
				Reporting.AddLine("Pu / (" + ConstString.FIOMEGA0_9 + " * Pn) << 0.2");
				Reporting.AddLine("Pu / (2 * " + ConstString.FIOMEGA0_9 + " * Pn) + Mu / (" + ConstString.FIOMEGA0_9 + " * Mn)");
				Reporting.AddLine("= " + Pu + " / (2*" + ConstString.FIOMEGA0_9 + " * " + Pn + ") + " + Mu + " / (" + ConstString.FIOMEGA0_9 + " * " + Mn + ")");
			}
			if (UtilizationFactor <= 1)
				Reporting.AddLine("= " + UtilizationFactor + " <= 1.0 (OK)");
			else
				Reporting.AddLine("= " + UtilizationFactor + " >> 1.0 (NG)");
		}

		internal static double ShearTensionInteraction(int flg, double R, double a, double b, double Fu, double An1, double An2)
		{
			double ShearTensionInteraction;

			double BlockShearCapacity;
			double Fv2;
			double ft2;
			double fv1;
			double ft1;

			if (b == 0)
			{
				ft1 = Fu;
				fv1 = 0;
				ft2 = 0;
				Fv2 = 0.6 * Fu;
				BlockShearCapacity = Fv2 * An2 + ft1 * An1;
			}
			else if (a == 0)
			{
				ft1 = 0;
				fv1 = 0.6 * Fu;
				ft2 = Fu;
				Fv2 = 0;
				BlockShearCapacity = fv1 * An1 + ft2 * An2;
			}
			else
			{
				ft1 = ConstNum.FIOMEGA0_75N * 0.18 * Math.Pow(a / b, 2) * (-1 + Math.Pow(1 + Math.Pow(b / a, 2) / 0.09, 0.5)) * Fu;
				fv1 = ft1 * b / a;
				ft2 = ConstNum.FIOMEGA0_75N * 0.18 * Math.Pow(b / a, 2) * (-1 + Math.Pow(1 + Math.Pow(a / b, 2) / 0.09, 0.5)) * Fu;
				Fv2 = ft2 * a / b;
				BlockShearCapacity = (fv1 * An1 + ft2 * An2) / b;
			}
			ShearTensionInteraction = BlockShearCapacity;

			if (b == 0)
			{
				BlockShearCapacity = Fv2 * An2 + ft1 * An1;
				Reporting.AddLine(ConstString.PHI + " Rn = (Fv2 * An2 + ft1 * An1)");
				Reporting.AddLine("=  (" + Fv2 + " * " + An2 + " + " + ft1 + " * " + An1 + ")");
			}
			else if (a == 0)
			{
				BlockShearCapacity = ConstNum.FIOMEGA0_75N * (fv1 * An1 + ft2 * An2);
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * (fv1 * An1 + ft2 * An2)");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * (" + fv1 + " * " + An1 + " +" + ft2 + " * " + An2 + ")");
			}
			else
			{
				Reporting.AddHeader("Adjusted " + ConstString.DES_OR_ALLOWABLE + " Stress:");

				switch (flg)
				{
					case 1:
						Reporting.AddLine("ft1 = " + ConstString.FIOMEGA0_75 + " * 0.18 * (A / B)² * (-1 + (1 + (B / A)² / 0.09)^0.5) * Fu");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.18 * (" + a + " / " + b + ")² * (-1 + (1 + (" + b + " / " + a + ")² / 0.09)^0.5) * " + Fu);
						Reporting.AddLine("= " + ft1 + ConstUnit.Stress);
						Reporting.AddLine(" Fv1 = ft1 * B / A = " + ft1 + " * " + b + " / " + a + " = " + fv1 + ConstUnit.Stress);
						Reporting.AddLine("ft2 = " + ConstString.FIOMEGA0_75 + " * 0.18 * (B / A)² * (-1 + (1 + (A / B)² / 0.09)^0.5) * Fu");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.18 * (" + b + " / " + a + ")² * (-1 + (1 + (" + a + " / " + b + ")² / 0.09)^0.5) * " + Fu);
						Reporting.AddLine("= " + ft2 + ConstUnit.Stress);
						Reporting.AddLine(" Fv2 = ft2 * A / B = " + ft2 + " * " + a + " / " + b + " = " + Fv2 + ConstUnit.Stress);
						break;
					case 2:
						Reporting.AddLine("Same as Above");
						Reporting.AddLine(ConstString.PHI + " Rn = (Fv1 * An1 + Ft2 * An2) / B");
						Reporting.AddLine("= (" + fv1 + " * " + An1 + " + " + ft2 + " * " + An2 + ") / " + b);
						BlockShearCapacity = (fv1 * An1 + ft2 * An2) / b;
						break;
				}
			}
			if (BlockShearCapacity >= R)
				Reporting.AddLine("= " + BlockShearCapacity + " >= " + R + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + BlockShearCapacity + " << " + R + ConstUnit.Force + " (NG)");

			return ShearTensionInteraction;
		}

		internal static double WeldBetaFactor(double wLength, double wSize)
		{
			return Math.Min(1, 1.2 - 0.002 * wLength / wSize);
		}

		// TODO: Determine the value for deduct. There are lots of places in this method where deduct uses unknown values
		private static void NetAndGrossArea(EMemberType memberType, int Pattern, ref double Ag1, ref double An1, ref double Ag2, ref double An2)
		{
			double deduct = 0;
			double copeB;
			double copeT;

			var component = CommonDataStatic.DetailDataDict[memberType];
			var webPlate = component.WinConnect.ShearWebPlate;

			// An1,Ag1 are vertical
			// An2, Ag2 are horizontal
			switch (memberType)
			{
				case EMemberType.RightBeam:
				case EMemberType.LeftBeam:
					switch (component.GussetToColumnConnection)
					{
						case EBraceConnectionTypes.DirectlyWelded:
						case EBraceConnectionTypes.EndPlate:
							break;
						case EBraceConnectionTypes.ClipAngle:
							if (CommonDataStatic.Preferences.UseContinuousClipAngles)
							{
								if (memberType == EMemberType.RightBeam)
								{
									if (!CommonDataStatic.DetailDataDict[EMemberType.UpperRight].IsActive && !CommonDataStatic.DetailDataDict[EMemberType.LowerRight].IsActive)
										copeT = copeB = 1;
									else if (!CommonDataStatic.DetailDataDict[EMemberType.UpperRight].IsActive)
									{
										copeT = 1;
										copeB = 0;
									}
									else if (!CommonDataStatic.DetailDataDict[EMemberType.LowerRight].IsActive)
									{
										copeT = 0;
										copeB = 1;
									}
									else
									{
										copeT = 0;
										copeB = 0;
									}
								}
								else
								{
									if (!CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].IsActive && !CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].IsActive)
										copeT = copeB = 1;
									else if (!CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].IsActive)
									{
										copeT = 1;
										copeB = 0;
									}
									else if (!CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].IsActive)
									{
										copeT = 0;
										copeB = 1;
									}
									else
									{
										copeT = 0;
										copeB = 0;
									}
								}
								if (component.WinConnect.ShearClipAngle.AnglesBoltedToGusset)
								{
									switch (Pattern)
									{
										case 0:
											Ag1 = (component.Shape.d - (copeT + copeB) * component.WinConnect.Beam.TCopeD) * component.Shape.tw;
											An1 = Ag1 - component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
											Ag2 = 0;
											An2 = 0;
											break;
										case 1:
											Ag1 = (component.Shape.d - deduct) * component.Shape.tw;
											An1 = Ag1 - (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 0.5) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
											Ag2 = (component.WinConnect.ShearClipAngle.GussetSideGage - component.EndSetback) * component.Shape.tw;
											An2 = Ag2 - (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
											break;
										case 2:
											Ag1 = (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 1) * component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir * component.Shape.tw;
											An1 = Ag1 - (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 1) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
											Ag2 = 2 * (component.WinConnect.ShearClipAngle.GussetSideGage - component.EndSetback) * component.Shape.tw;
											An2 = Ag2 - 2 * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
											break;
									}
								}
								else // welded
								{
									switch (Pattern)
									{
										case 0:
											Ag1 = (component.Shape.d - (copeT + copeB) * component.WinConnect.Beam.TCopeD) * component.Shape.tw;
											An1 = Ag1;
											Ag2 = 0;
											An2 = 0;
											break;
										case 1:
											Ag1 = (component.Shape.d - deduct) * component.Shape.tw;
											An1 = Ag1;
											Ag2 = (component.WinConnect.ShearClipAngle.ShortLeg + component.WinConnect.ShearClipAngle.LongLeg - component.WinConnect.ShearClipAngle.LengthOfOSL - component.EndSetback) * component.Shape.tw;
											An2 = Ag2;
											break;
										case 2:
											Ag1 = component.WinConnect.ShearClipAngle.Length * component.Shape.tw;
											An1 = Ag1;
											Ag2 = 2 * (component.WinConnect.ShearClipAngle.ShortLeg + component.WinConnect.ShearClipAngle.LongLeg - component.WinConnect.ShearClipAngle.LengthOfOSL - component.EndSetback) * component.Shape.tw;
											An2 = Ag2;
											break;
									}
								}
							}
							else // not continuousangles
							{
								if (component.WinConnect.ShearClipAngle.AnglesBoltedToGusset)
								{
									switch (Pattern)
									{
										case 0:
											Ag1 = component.Shape.d * component.Shape.tw;
											An1 = Ag1 - component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
											Ag2 = 0;
											An2 = 0;
											break;
										case 1:
											Ag1 = (component.Shape.d - deduct) * component.Shape.tw;
											An1 = Ag1 - (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 0.5) * (webPlate.Bolt.HoleWidth + (ConstNum.SIXTEENTH_INCH)) * component.Shape.tw;
											Ag2 = (component.WinConnect.ShearClipAngle.GussetSideGage - component.EndSetback) * component.Shape.tw;
											An2 = Ag2 - (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
											break;
										case 2:
											Ag1 = (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 1) * component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir * component.Shape.tw;
											An1 = Ag1 - (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 1) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
											Ag2 = 2 * (component.WinConnect.ShearClipAngle.GussetSideGage - component.EndSetback) * component.Shape.tw;
											An2 = Ag2 - 2 * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
											break;
									}
								}
								else // welded
								{
									switch (Pattern)
									{
										case 0:
											Ag1 = component.Shape.d * component.Shape.tw;
											An1 = Ag1;
											Ag2 = 0;
											An2 = 0;
											break;
										case 1:
											Ag1 = (component.Shape.d - deduct) * component.Shape.tw;
											An1 = Ag1;
											Ag2 = (component.WinConnect.ShearClipAngle.ShortLeg + component.WinConnect.ShearClipAngle.LongLeg - component.WinConnect.ShearClipAngle.LengthOfOSL - component.EndSetback) * component.Shape.tw;
											An2 = Ag2;
											break;
										case 2:
											Ag1 = component.WinConnect.ShearClipAngle.Length * component.Shape.tw;
											An1 = Ag1;
											Ag2 = 2 * (component.WinConnect.ShearClipAngle.ShortLeg + component.WinConnect.ShearClipAngle.LongLeg - component.WinConnect.ShearClipAngle.LengthOfOSL - component.EndSetback) * component.Shape.tw;
											An2 = Ag2;
											break;
									}
								}
							}
							break;
						case EBraceConnectionTypes.SinglePlate:
							switch (Pattern)
							{
								case 0:
									Ag1 = component.Shape.d * component.Shape.tw;
									An1 = Ag1 - webPlate.Bolt.NumberOfRows * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
									Ag2 = 0;
									An2 = 0;
									break;
								case 1:
									//deduct = (Math.Max(BRACE1.y[8 * m + 4] - BRACE1.BoltY[m, component.Bolt.NumRowsOfBolts], BRACE1.BoltY[m, 1] - BRACE1.y[8 * m + 1]));
									Ag1 = (component.Shape.d - deduct) * component.Shape.tw;
									An1 = Ag1 - (webPlate.Bolt.NumberOfRows - 0.5) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
									Ag2 = ((webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir + webPlate.Bolt.EdgeDistTransvDir) * component.Shape.tw;
									An2 = Ag2 - (webPlate.Bolt.NumberOfLines - 0.5) * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
									break;
								case 2:
									Ag1 = (webPlate.Bolt.NumberOfRows - 1) * webPlate.Bolt.SpacingLongDir * component.Shape.tw;
									An1 = Ag1 - (webPlate.Bolt.NumberOfRows - 1) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
									Ag2 = 2 * ((webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir + webPlate.Bolt.EdgeDistTransvDir) * component.Shape.tw;
									An2 = Ag2 - 2 * (webPlate.Bolt.NumberOfLines - 0.5) * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
									break;
							}
							break;
						case EBraceConnectionTypes.FabricatedTee:
							switch (Pattern)
							{
								case 0:
									Ag1 = component.Shape.d * component.Shape.tw;
									An1 = Ag1 - component.BraceConnect.FabricatedTee.Bolt.NumberOfRows * (component.BraceConnect.FabricatedTee.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
									Ag2 = 0;
									An2 = 0;
									break;
								case 1:
									//deduct = (Math.Max(BRACE1.y[8 * m + 4] - BRACE1.BoltY[m, component.Bolt.NumRowsOfBolts], BRACE1.BoltY[m, 1] - BRACE1.y[8 * m + 1]));
									Ag1 = (component.Shape.d - deduct) * component.Shape.tw;
									An1 = Ag1 - (component.BraceConnect.FabricatedTee.Bolt.NumberOfRows - 0.5) * (component.BraceConnect.FabricatedTee.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
									Ag2 = ((component.BraceConnect.FabricatedTee.Bolt.NumberOfLines - 1) * component.BraceConnect.FabricatedTee.Bolt.SpacingTransvDir + component.BraceConnect.FabricatedTee.Eh2) * component.Shape.tw;
									An2 = Ag2 - (component.BraceConnect.FabricatedTee.Bolt.NumberOfLines - 0.5) * (component.BraceConnect.FabricatedTee.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
									break;
								case 2:
									Ag1 = (component.BraceConnect.FabricatedTee.Bolt.NumberOfRows - 1) * component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir * component.Shape.tw;
									An1 = Ag1 - (component.BraceConnect.FabricatedTee.Bolt.NumberOfRows - 1) * (component.BraceConnect.FabricatedTee.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
									Ag2 = 2 * ((component.BraceConnect.FabricatedTee.Bolt.NumberOfLines - 1) * component.BraceConnect.FabricatedTee.Bolt.SpacingTransvDir + component.BraceConnect.FabricatedTee.Eh2) * component.Shape.tw;
									An2 = Ag2 - 2 * (component.BraceConnect.FabricatedTee.Bolt.NumberOfLines - 0.5) * (component.BraceConnect.FabricatedTee.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
									break;
							}
							break;
					}
					break;
				default:
					switch (component.GussetToColumnConnection)
					{
						case EBraceConnectionTypes.EndPlate:
						case EBraceConnectionTypes.DirectlyWelded:
							break;
						case EBraceConnectionTypes.ClipAngle:
							if (CommonDataStatic.Preferences.UseContinuousClipAngles)
							{
								if (component.WinConnect.ShearClipAngle.AnglesBoltedToGusset)
								{
									switch (Pattern)
									{
										case 0:
											Ag1 = component.BraceConnect.Gusset.ColumnSide * component.BraceConnect.Gusset.Thickness;
											An1 = Ag1 - component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
											Ag2 = 0;
											An2 = 0;
											break;
										case 1:
											switch (memberType)
											{
												case EMemberType.UpperRight:
												case EMemberType.UpperLeft:
													//deduct = (Math.Max(BRACE1.BoltY[m + 30, 1] - BRACE1.y[25 + (m - 3) * 5], BRACE1.y[29 + (m - 3) * 5] - BRACE1.BoltY[m + 30, component.Bolt.NumberOfBolts]));
													break;
												case EMemberType.LowerRight:
												case EMemberType.LowerLeft:
													//deduct = (Math.Max(BRACE1.BoltY[m + 30, 1] - BRACE1.y[29 + (m - 3) * 5], BRACE1.y[25 + (m - 3) * 5] - BRACE1.BoltY[m + 30, component.Bolt.NumberOfBolts]));
													break;
											}
											Ag1 = (component.BraceConnect.Gusset.ColumnSide - deduct) * component.BraceConnect.Gusset.Thickness;
											An1 = Ag1 - (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 0.5) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
											Ag2 = (component.WinConnect.ShearClipAngle.GussetSideGage - component.EndSetback) * component.BraceConnect.Gusset.Thickness;
											An2 = Ag2 - (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
											break;
										case 2:
											Ag1 = (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 1) * component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir * component.BraceConnect.Gusset.Thickness;
											An1 = Ag1 - (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 1) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
											Ag2 = 2 * (component.WinConnect.ShearClipAngle.GussetSideGage - component.EndSetback) * component.BraceConnect.Gusset.Thickness;
											An2 = Ag2 - 2 * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
											break;
									}
								}
								else // welded
								{
									switch (Pattern)
									{
										case 0:
											Ag1 = component.BraceConnect.Gusset.ColumnSide * component.BraceConnect.Gusset.Thickness;
											An1 = Ag1;
											Ag2 = 0;
											An2 = 0;
											break;
										case 1:
											switch (memberType)
											{
												case EMemberType.UpperRight:
												case EMemberType.UpperLeft:
													//deduct = (Math.Max(BRACE1.y[81 + (m - 1) * 8] - BRACE1.y[25 + (m - 3) * 5], BRACE1.y[29 + (m - 3) * 5] - BRACE1.y[84 + (m - 1) * 8]));
													break;
												case EMemberType.LowerRight:
												case EMemberType.LowerLeft:
													//deduct = (Math.Max(BRACE1.y[81 + (m - 1) * 8] - BRACE1.y[29 + (m - 3) * 5], BRACE1.y[25 + (m - 3) * 5] - BRACE1.y[84 + (m - 1) * 8]));
													break;
											}
											Ag1 = (component.BraceConnect.Gusset.ColumnSide - deduct) * component.BraceConnect.Gusset.Thickness;
											An1 = Ag1;
											Ag2 = (component.WinConnect.ShearClipAngle.ShortLeg + component.WinConnect.ShearClipAngle.LongLeg - component.WinConnect.ShearClipAngle.LengthOfOSL - component.BraceConnect.Gusset.ColumnSideSetback) * component.BraceConnect.Gusset.Thickness;
											An2 = Ag2;
											break;
										case 2:
											Ag1 = component.WinConnect.ShearClipAngle.Length * component.BraceConnect.Gusset.Thickness;
											An1 = Ag1;
											Ag2 = 2 * (component.WinConnect.ShearClipAngle.ShortLeg + component.WinConnect.ShearClipAngle.LongLeg - component.WinConnect.ShearClipAngle.LengthOfOSL - component.BraceConnect.Gusset.ColumnSideSetback) * component.BraceConnect.Gusset.Thickness;
											An2 = Ag2;
											break;
									}
								}
							}
							else // not continuousangles
							{
								if (component.WinConnect.ShearClipAngle.AnglesBoltedToGusset)
								{
									switch (Pattern)
									{
										case 0:
											Ag1 = component.BraceConnect.Gusset.ColumnSide * component.BraceConnect.Gusset.Thickness;
											An1 = Ag1 - component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
											Ag2 = 0;
											An2 = 0;
											break;
										case 1:
											switch (memberType)
											{
												case EMemberType.UpperRight:
												case EMemberType.UpperLeft:
													//deduct = (Math.Max(BRACE1.y[29 + (m - 3) * 5] - BRACE1.BoltY[m + 30, component.Bolt.NumberOfBolts], BRACE1.y[25 + (m - 3) * 5] - BRACE1.BoltY[m + 30, 1]));
													break;
												case EMemberType.LowerRight:
												case EMemberType.LowerLeft:
													//deduct = (Math.Max(BRACE1.y[25 + (m - 3) * 5] - BRACE1.BoltY[m + 30, component.Bolt.NumberOfBolts], BRACE1.y[29 + (m - 3) * 5] - BRACE1.BoltY[m + 30, 1]));
													break;
											}
											Ag1 = (component.BraceConnect.Gusset.ColumnSide - deduct) * component.BraceConnect.Gusset.Thickness;
											An1 = Ag1 - (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 0.5) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
											Ag2 = (component.WinConnect.ShearClipAngle.GussetSideGage - component.EndSetback) * component.BraceConnect.Gusset.Thickness;
											An2 = Ag2 - (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
											break;
										case 2:
											Ag1 = (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 1) * component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir * component.BraceConnect.Gusset.Thickness;
											An1 = Ag1 - (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 1) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
											Ag2 = 2 * (component.WinConnect.ShearClipAngle.GussetSideGage - component.EndSetback) * component.BraceConnect.Gusset.Thickness;
											An2 = Ag2 - 2 * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
											break;
									}
								}
								else // Welded
								{
									switch (Pattern)
									{
										case 0:
                                            Ag1 = component.BraceConnect.Gusset.ColumnSide * component.BraceConnect.Gusset.Thickness;
											An1 = Ag1;
											Ag2 = 0;
											An2 = 0;
											break;
										case 1:
											switch (memberType)
											{
												case EMemberType.UpperRight:
												case EMemberType.UpperLeft:
													//deduct = (Math.Max(BRACE1.y[29 + (m - 3) * 5] - BRACE1.y[84 + (m - 1) * 8], BRACE1.y[25 + (m - 3) * 5] - BRACE1.y[81 + (m - 1) * 8]));
													break;
												case EMemberType.LowerRight:
												case EMemberType.LowerLeft:
													//deduct = (Math.Max(BRACE1.y[25 + (m - 3) * 5] - BRACE1.y[84 + (m - 1) * 8], BRACE1.y[29 + (m - 3) * 5] - BRACE1.y[81 + (m - 1) * 8]));
													break;
											}
                                            Ag1 = (component.BraceConnect.Gusset.ColumnSide - deduct) * component.BraceConnect.Gusset.Thickness;
											An1 = Ag1;
											Ag2 = (component.WinConnect.ShearClipAngle.ShortLeg + component.WinConnect.ShearClipAngle.LongLeg - component.WinConnect.ShearClipAngle.LengthOfOSL - component.BraceConnect.Gusset.ColumnSideSetback) * component.BraceConnect.Gusset.Thickness;
											An2 = Ag2;
											break;
										case 2:
											Ag1 = component.WinConnect.ShearClipAngle.Length * component.BraceConnect.Gusset.Thickness;
											An1 = Ag1;
											Ag2 = 2 * (component.WinConnect.ShearClipAngle.ShortLeg + component.WinConnect.ShearClipAngle.LongLeg - component.WinConnect.ShearClipAngle.LengthOfOSL - component.BraceConnect.Gusset.ColumnSideSetback) * component.BraceConnect.Gusset.Thickness;
											An2 = Ag2;
											break;
									}
								}
							}
							break;
						case EBraceConnectionTypes.SinglePlate:
							switch (Pattern)
							{
								case 0:
                                    Ag1 = component.BraceConnect.Gusset.ColumnSide * component.BraceConnect.Gusset.Thickness;
									An1 = Ag1 - webPlate.Bolt.NumberOfRows * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
									Ag2 = 0;
									An2 = 0;
									break;
								case 1:
									switch (memberType)
									{
										case EMemberType.UpperRight:
										case EMemberType.UpperLeft:
											//deduct = (Math.Max(BRACE1.y[29 + (m - 3) * 5] - BRACE1.BoltY[m, component.Bolt.NumRowsOfBolts], BRACE1.BoltY[m, 1] - BRACE1.y[25 + (m - 3) * 5]));
											break;
										case EMemberType.LowerRight:
										case EMemberType.LowerLeft:
											//deduct = (Math.Max(BRACE1.y[25 + (m - 3) * 5] - BRACE1.BoltY[m, component.Bolt.NumRowsOfBolts], BRACE1.BoltY[m, 1] - BRACE1.y[29 + (m - 3) * 5]));
											break;
									}
                                    Ag1 = (component.BraceConnect.Gusset.ColumnSide - deduct) * component.BraceConnect.Gusset.Thickness;
									An1 = Ag1 - (webPlate.Bolt.NumberOfRows - 0.5) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
									Ag2 = ((webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir + webPlate.Bolt.EdgeDistTransvDir) * component.BraceConnect.Gusset.Thickness;
									An2 = Ag2 - (webPlate.Bolt.NumberOfLines - 0.5) * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
									break;
								case 2:
									Ag1 = (webPlate.Bolt.NumberOfRows - 1) * webPlate.Bolt.SpacingLongDir * component.BraceConnect.Gusset.Thickness;
									An1 = Ag1 - (webPlate.Bolt.NumberOfRows - 1) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
									Ag2 = 2 * ((webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir + webPlate.Bolt.EdgeDistTransvDir) * component.BraceConnect.Gusset.Thickness;
									An2 = Ag2 - 2 * (webPlate.Bolt.NumberOfLines - 0.5) * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
									break;
							}
							break;
						case EBraceConnectionTypes.FabricatedTee:
							switch (Pattern)
							{
								case 0:
                                    Ag1 = component.BraceConnect.Gusset.ColumnSide * component.BraceConnect.Gusset.Thickness;
									An1 = Ag1 - component.BraceConnect.FabricatedTee.Bolt.NumberOfRows * (component.BraceConnect.FabricatedTee.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
									Ag2 = 0;
									An2 = 0;
									break;
								case 1:
									switch (memberType)
									{
										case EMemberType.UpperRight:
										case EMemberType.UpperLeft:
											//deduct = (Math.Max(BRACE1.y[29 + (m - 3) * 5] - BRACE1.BoltY[m, component.Bolt.NumRowsOfBolts], BRACE1.BoltY[m, 1] - BRACE1.y[25 + (m - 3) * 5]));
											break;
										case EMemberType.LowerRight:
										case EMemberType.LowerLeft:
											//deduct = (Math.Max(BRACE1.y[25 + (m - 3) * 5] - BRACE1.BoltY[m, component.Bolt.NumRowsOfBolts], BRACE1.BoltY[m, 1] - BRACE1.y[29 + (m - 3) * 5]));
											break;
									}
                                    Ag1 = (component.BraceConnect.Gusset.ColumnSide - deduct) * component.BraceConnect.Gusset.Thickness;
									An1 = Ag1 - (component.BraceConnect.FabricatedTee.Bolt.NumberOfRows - 0.5) * (component.BraceConnect.FabricatedTee.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
									Ag2 = ((component.BraceConnect.FabricatedTee.Bolt.NumberOfLines - 1) * component.BraceConnect.FabricatedTee.Bolt.SpacingTransvDir + component.BraceConnect.FabricatedTee.Eh2) * component.BraceConnect.Gusset.Thickness;
									An2 = Ag2 - (component.BraceConnect.FabricatedTee.Bolt.NumberOfLines - 0.5) * (component.BraceConnect.FabricatedTee.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
									break;
								case 2:
									Ag1 = (component.BraceConnect.FabricatedTee.Bolt.NumberOfRows - 1) * component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir * component.BraceConnect.Gusset.Thickness;
									An1 = Ag1 - (component.BraceConnect.FabricatedTee.Bolt.NumberOfRows - 1) * (component.BraceConnect.FabricatedTee.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
									Ag2 = 2 * ((component.BraceConnect.FabricatedTee.Bolt.NumberOfLines - 1) * component.BraceConnect.FabricatedTee.Bolt.SpacingTransvDir + component.BraceConnect.FabricatedTee.Eh2) * component.BraceConnect.Gusset.Thickness;
									An2 = Ag2 - 2 * (component.BraceConnect.FabricatedTee.Bolt.NumberOfLines - 0.5) * (component.BraceConnect.FabricatedTee.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.Gusset.Thickness;
									break;
							}
							break;
					}
					break;
			}
			if (Pattern == 0)
				Reporting.AddLine("Ag = " + Ag1 + ConstUnit.Area + "    " + "An = " + An1 + ConstUnit.Area);
			else
				Reporting.AddLine("Ag1 = " + Ag1 + ConstUnit.Area + "    " + "An1 = " + An1 + ConstUnit.Area + "    " + "Ag2 = " + Ag2 + ConstUnit.Area + "    " + "An2 = " + An2 + ConstUnit.Area);
		}

		internal static void GussetandBeamCheckwithWeldedClipAngles(EMemberType memberType, double V, double H_Ten, double H_Comp, double R)
		{
			double sonuc;
			double BlockShearCapacity2;
			double BlockShearCapacity1 = 0;
			double An1 = 0;
			double Ag1 = 0;
			double GrossTensionandShearCapacity;
			double NetTensionandShearCapacity;
			double An2 = 0;
			double Ag2 = 0;
			double An = 0;
			double Ag = 0;
			double b = 0;
			double a = 0;

			double Fy;
			double Fu;

			var component = CommonDataStatic.DetailDataDict[memberType];
			bool componentIsBrace = (memberType == EMemberType.UpperRight || memberType == EMemberType.UpperLeft ||
			                         memberType == EMemberType.LowerRight || memberType == EMemberType.LowerLeft);

			if (R == 0)
				return;

			if (!componentIsBrace)
			{
				Fu = component.Material.Fu;
				Fy = component.Material.Fy;
			}
			else
			{
				Fu = component.BraceConnect.Gusset.Material.Fu;
				Fy = component.BraceConnect.Gusset.Material.Fy;
			}

			if (componentIsBrace)
				Reporting.AddHeader("Gusset Tear-out");
			else
				Reporting.AddHeader("Beam Web Tear-out:");

			Reporting.AddHeader("Combined Tension and Shear:");
			Reporting.AddFormulaLine();

			if (R != 0)
			{
				a = H_Ten / R;
				b = V / R;
			}

			Reporting.AddLine("Resultant Force, R = (H² + V²)^0.5 = (" + H_Ten + "² +" + V + "²)^0.5 = " + R + ConstUnit.Force);
			if (V == 0)
				Reporting.AddLine("Load Angle, " + ConstString.PHI + " = Atn(H / V) = 90 Degees");
			else
				Reporting.AddLine("Load Angle, " + ConstString.PHI + " = Atn(H / V) = " + Math.Atan(Math.Abs(H_Ten / V)) / ConstNum.RADIAN + " Degees");
			Reporting.AddLine("A = Sin(" + ConstString.PHI + ") = " + a);
			Reporting.AddLine("B = Cos(" + ConstString.PHI + ") = " + b);

			NetAndGrossArea(memberType, 0, ref Ag, ref An, ref Ag2, ref An2);
			Reporting.AddHeader("Rupture:");

			if (b > 0 && a > 0)
			{
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.18 * (a / b) * (-1 + Math.Pow(1 + Math.Pow(b / a, 2) / 0.09, 0.5)) * An * Fu / b;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.18 * (A / B) * (-1 + (1 + (B / A)² / 0.09)^0.5) * An * Fu / B");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.18 * (" + a + " / " + b + ") * (-1 + (1 + (" + b + " / " + a + ")² / 0.09)^0.5) * " + An + " * " + Fu + " / " + b);
			}
			else if (b == 0)
			{
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * An * Fu;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * An * Fu = " + ConstString.FIOMEGA0_75 + " * " + An + " * " + Fu);
			}
			else
			{
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.6 * An * Fu;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.6 * An * Fu = " + ConstString.FIOMEGA0_75 + " * 0.6 * " + An + " * " + Fu);
			}
			if (NetTensionandShearCapacity >= R)
				Reporting.AddLine("= " + NetTensionandShearCapacity + " >= " + R + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + NetTensionandShearCapacity + " << " + R + ConstUnit.Force + " (NG)");

			// Combined Tension and Shear Yielding
			Reporting.AddHeader("Yielding:");

			if (b > 0 && a > 0)
			{
				GrossTensionandShearCapacity = Math.Pow(ConstNum.FIOMEGA1_0N, 2) * 0.18 * Math.Pow(a / b, 2) * (-1 / ConstNum.FIOMEGA0_9N + Math.Pow(1 / Math.Pow(ConstNum.FIOMEGA0_9N, 2) + Math.Pow(b / a, 2) / (0.09 * Math.Pow(ConstNum.FIOMEGA1_0N, 2)), 0.5)) * Ag * Fy / a;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.FIOMEGA1_0N + "² * 0.18 * (A / B)² * (-1 / " + ConstString.FIOMEGA0_9 + " + (1 / (" + ConstString.FIOMEGA0_9 + "²) + (B / A)² / (0.09 * " + ConstNum.FIOMEGA1_0N + "²))^0.5) * Ag * Fy / A");
				Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + "² * 0.18 * (" + a + " / " + b + ")² * (-1 / " + ConstString.FIOMEGA0_9 + " + (1 / (" + ConstString.FIOMEGA0_9 + "²) + (" + b + " / " + a + ")² / (0.09 * " + ConstNum.FIOMEGA1_0N + "²))^0.5) * " + Ag + " * " + Fy + " / " + a);
			}
			else if (b == 0)
			{
				GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * Ag * Fy;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Ag * Fy = " + ConstString.FIOMEGA0_9 + " * " + Ag + " * " + Fy);
			}
			else // if (a == 0)
			{
				GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * 0.6 * Ag * Fy;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * 0.6 * Ag * Fy = " + ConstString.FIOMEGA0_9 + " * 0.6 * " + Ag + " * " + Fy);
			}

			if (GrossTensionandShearCapacity >= R)
				Reporting.AddLine("= " + GrossTensionandShearCapacity + " >= " + R + ConstUnit.Force + " (OK)");
			else
			{
				Reporting.AddLine("= " + GrossTensionandShearCapacity + " << " + R + ConstUnit.Force + " (NG)");
				if (!component.BraceConnect.Gusset.Thickness_User)
					component.BraceConnect.Gusset.Thickness = CommonCalculations.PlateThickness(component.BraceConnect.Gusset.Thickness * R / GrossTensionandShearCapacity);
				if (double.IsNaN(component.BraceConnect.Gusset.Thickness))
					component.BraceConnect.Gusset.Thickness = 0;
			}

			// Block Shear 1
			if (componentIsBrace)
				Reporting.AddHeader("Block Shear of Gusset:");
			else
				Reporting.AddHeader("Block Shear of Beam Web:");
			Reporting.AddHeader("Vertical (An1,Ft1) and Horizontal (An2,Ft2) Sections:");

			if (componentIsBrace || CommonDataStatic.Preferences.UseContinuousClipAngles)
			{
				NetAndGrossArea(memberType, 1, ref Ag1, ref An1, ref Ag2, ref An2);
				Reporting.AddHeader("Pattern 1:");
				BlockShearCapacity1 = ShearTensionInteraction(1, R, a, b, component.Material.Fu, An1, An2);
			}
			// Block Shear 2
			Reporting.AddHeader("Pattern 2:");
			NetAndGrossArea(memberType, 2, ref Ag1, ref An1, ref Ag2, ref An2);

			if (!componentIsBrace && !CommonDataStatic.Preferences.UseContinuousClipAngles)
				BlockShearCapacity2 = ShearTensionInteraction(1, R, a, b, component.Material.Fu, An1, An2);
			else
				BlockShearCapacity2 = ShearTensionInteraction(2, R, a, b, component.Material.Fu, An1, An2);

			if (R != 0)
			{
				a = H_Ten / R;
				b = V / R;
			}

			NetAndGrossArea(memberType, 0, ref Ag, ref An, ref Ag2, ref An2);

			if (b > 0 && a > 0)
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.18 * (a / b) * (-1 + Math.Pow(1 + Math.Pow(b / a, 2) / 0.09, 0.5)) * An * Fu / b;
			else if (b == 0)
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * An * Fu;
			else
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.6 * An * Fu;

			// Combined Tension and Shear Yielding
			if (b > 0 && a > 0)
				GrossTensionandShearCapacity = Math.Pow(ConstNum.FIOMEGA1_0N, 2) * 0.18 * Math.Pow(a / b, 2) * (-1.0 / ConstNum.FIOMEGA0_9N + Math.Pow(1.0 / Math.Pow(ConstNum.FIOMEGA0_9N, 2) + Math.Pow(b / a, 2) / (0.09 * Math.Pow(ConstNum.FIOMEGA1_0N, 2)), 0.5)) * Ag * Fy / a;
			else if (b == 0)
				GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * Ag * Fy;
			else //if (a == 0)
				GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * 0.6 * Ag * Fy;

			// Block Shear 1
			if (componentIsBrace || CommonDataStatic.Preferences.UseContinuousClipAngles)
			{
				NetAndGrossArea(memberType, 1, ref Ag1, ref An1, ref Ag2, ref An2);
				BlockShearCapacity1 = ShearTensionInteraction(1, R, a, b, Fu, An1, An2);
			}
			// Block Shear 2 "Pattern 2:"
			NetAndGrossArea(memberType, 2, ref Ag1, ref An1, ref Ag2, ref An2);

			if (!componentIsBrace && !CommonDataStatic.Preferences.UseContinuousClipAngles)
				BlockShearCapacity2 = ShearTensionInteraction(1, R, a, b, Fu, An1, An2);
			else
				BlockShearCapacity2 = ShearTensionInteraction(2, R, a, b, Fu, An1, An2);
			sonuc = Math.Min(Math.Min(BlockShearCapacity2, BlockShearCapacity1), Math.Min(GrossTensionandShearCapacity, NetTensionandShearCapacity));
			if (sonuc < R && !component.BraceConnect.Gusset.Thickness_User)
				component.BraceConnect.Gusset.Thickness = CommonCalculations.PlateThickness(component.BraceConnect.Gusset.Thickness * R / sonuc);
			if (double.IsNaN(component.BraceConnect.Gusset.Thickness))
				component.BraceConnect.Gusset.Thickness = 0;
		}

		internal static void ExpectedStrengthCoefficient(EMemberType memberType, ref double materialRt, ref double materialRy, ref double rtStrength, ref double ryStrength, ref string Yildiz, ref string rtString, ref string ryString)
		{
			var component = CommonDataStatic.DetailDataDict[memberType];

			if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic)
			{
				materialRt = component.Material.Rt;
				materialRy = component.Material.Ry;

				rtStrength = materialRt;
				ryStrength = materialRy;
				rtString = "Rt";
				ryString = "Ry";
				Yildiz = " * ";
			}
			else
			{
				materialRt = 1;
				materialRy = 1;

				rtStrength = 0;
				ryStrength = 0;
				rtString = string.Empty;
				ryString = string.Empty;
				Yildiz = " ";
			}
		}

		internal static double Encroachment(EMemberType memberType)
		{
			double encroachment;
			var component = CommonDataStatic.DetailDataDict[memberType];

			double tmpCaseArg = component.Shape.kdet - component.Shape.tf;

			if (tmpCaseArg <= 0.3125)
				encroachment = 0.25;
			else if (tmpCaseArg <= 0.5)
				encroachment = 0.375;
			else if (tmpCaseArg <= 0.8125)
				encroachment = 0.5;
			else if (tmpCaseArg <= 1.25)
				encroachment = 0.625;
			else // if (tmpCaseArg >= 1.3125)
				encroachment = 0.75;
			return encroachment;
		}

		private static void GussetandBeamCheckwithBoltedClipAngles(EMemberType memberType, double V, double H, double R, double EquivBoltFactor, EBearingTearOut bearingorTearOut)
		{
			double BlockShearCapacity2 = 0;
			double BlockShearCapacity1 = 0;
			double An1 = 0;
			double Ag1 = 0;
			double GrossTensionandShearCapacity = 0;
			double NetTensionandShearCapacity = 0;
			double An2 = 0;
			double Ag2 = 0;
			double An = 0;
			double Ag = 0;
			double b = 0;
			double a = 0;
			double HoleVert = 0;
			int NbVert = 0;
			double L = 0;
			double BearingCapacityH = 0;
			double BearingCapacityV = 0;
			double Fbe = 0;
			double Fbs = 0;
			double Lv = 0;
			double Lh = 0;
			double Gap = 0;
			double Th = 0;
			double ClipAngleWebLeg = 0;
			double fv1, ft1, Fv2, ft2;

			var component = CommonDataStatic.DetailDataDict[memberType];
			var webPlate = component.WinConnect.ShearWebPlate;
			var bolts = component.WinConnect.ShearClipAngle.BoltOnGusset;

			ClipAngleWebLeg = component.WinConnect.ShearClipAngle.LongLeg + component.WinConnect.ShearClipAngle.ShortLeg - component.WinConnect.ShearClipAngle.LengthOfOSL;
			if (MiscMethods.IsBeamOrColumn(memberType))
			{
				Th = component.Shape.tw;
				Gap = component.EndSetback;
				Lh = component.WinConnect.ShearClipAngle.GussetSideGage - Gap;
				// Lv not used with beam
				//Lv = BRACE1.y[12 + (m - 1) * 8] - component.Shape.kdet - (BRACE1.y[84 + (m - 1) * 8] - BRACE1.Bolts[m + 30].eal);
			}
			else
			{
				Th = component.BraceConnect.Gusset.Thickness;
				Gap = component.EndSetback;
				Lh = component.WinConnect.ShearClipAngle.GussetSideGage - Gap; //  webPlate.MemberLh
				switch (memberType)
				{
					case EMemberType.UpperRight:
					case EMemberType.UpperLeft:
						//Lv = BRACE1.y[29 + (m - 3) * 5] - BRACE1.y[84 + (m - 1) * 8] + BRACE1.Bolts[m + 30].eal;
						break;
					case EMemberType.LowerRight:
					case EMemberType.LowerLeft:
						//Lv = BRACE1.y[81 + (m - 1) * 8] + BRACE1.Bolts[m + 30].eal - BRACE1.y[29 + (m - 3) * 5];
						break;
				}
			}
			switch (bearingorTearOut)
			{
				case EBearingTearOut.Bearing:
					if (MiscMethods.IsBeamOrColumn(memberType))
						Reporting.AddHeader("Bolt Bearing on Beam Web:");
					else
						Reporting.AddHeader("Bolt Bearing on Gusset:");
					Reporting.AddHeader("Vertical Load:");

					if (!MiscMethods.IsBeamOrColumn(memberType) || CommonDataStatic.Preferences.UseContinuousClipAngles)
					{
						Fbs = CommonCalculations.SpacingBearing(bolts.SpacingLongDir, webPlate.Bolt.HoleWidth, bolts.BoltSize, bolts.HoleType, component.Material.Fu, false);
						Fbe = CommonCalculations.EdgeBearing(Lv, (webPlate.Bolt.HoleWidth), bolts.BoltSize, component.Material.Fu, bolts.HoleType, false);
						BearingCapacityV = EquivBoltFactor * (Fbe + Fbs * (bolts.NumberOfRows - 1)) * Th;
						Reporting.AddLine(ConstString.PHI + " Rn = ef * (Fbe + Fbs * (Nl - 1)) * t");
						Reporting.AddLine("= " + EquivBoltFactor + " * (" + Fbe + " + " + EquivBoltFactor + " * (" + bolts.NumberOfRows + " - 1)) * " + Th);
					}
					else
					{
						Fbs = CommonCalculations.SpacingBearing(bolts.SpacingLongDir, webPlate.Bolt.HoleWidth, bolts.BoltSize, bolts.HoleType, component.Material.Fu, false);
						BearingCapacityV = EquivBoltFactor * Fbs * bolts.NumberOfRows * Th;
						Reporting.AddLine(ConstString.PHI + " Rn = ef * Fbs * Nl * t");
						Reporting.AddLine("= " + EquivBoltFactor + " * " + EquivBoltFactor + " * " + bolts.NumberOfRows + " * " + Th);
					}
					if (BearingCapacityV >= V)
						Reporting.AddLine("= " + BearingCapacityV + " >= " + V + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + BearingCapacityV + " << " + V + ConstUnit.Force + " (NG)");

					//Bolt Bearing under Horizontal Load
					Reporting.AddHeader("Horizontal Load:");
					Fbe = CommonCalculations.EdgeBearing(Lh, (webPlate.Bolt.HoleLength), bolts.BoltSize, component.Material.Fu, bolts.HoleType, false);
					BearingCapacityH = EquivBoltFactor * bolts.NumberOfRows * Fbe * Th;
					Reporting.AddLine(ConstString.PHI + " Rn = ef * Nl * Fbe * t");
					Reporting.AddLine("= " + EquivBoltFactor + " * " + bolts.NumberOfRows + " * " + Fbe + " * " + Th);
					if (BearingCapacityH >= H)
						Reporting.AddLine("= " + BearingCapacityH + " >= " + H + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + BearingCapacityH + " << " + H + ConstUnit.Force + " (NG)");
					break;
				case EBearingTearOut.TearOut:
					if (!MiscMethods.IsBeamOrColumn(memberType))
					{
						Reporting.AddHeader("Gusset Tear-out:");
						L = component.BraceConnect.Gusset.VerticalForceColumn;
						NbVert = bolts.NumberOfBolts;
						HoleVert = webPlate.Bolt.HoleWidth;
						Th = component.BraceConnect.Gusset.Thickness;
					}
					else
					{
						Reporting.AddHeader("Beam Web Tear-out:");
						if (CommonDataStatic.Preferences.UseContinuousClipAngles)
							L = component.Shape.d - component.WinConnect.Beam.TCopeD;
						else
							L = component.Shape.d;
						NbVert = bolts.NumberOfRows;
						HoleVert = webPlate.Bolt.HoleWidth;
						Th = component.Shape.tw;
					}

					//Combined Tension and Shear Rupture
					Reporting.AddHeader("Combined Tension and Shear:");
					Reporting.AddFormulaLine();
					if (R != 0)
					{
						a = H / R;
						b = V / R;
					}

					if (V == 0)
						Reporting.AddLine("Load Angle, " + ConstString.PHI + " = Atn(H / V) = 90 Degees");
					else
						Reporting.AddLine("Load Angle, " + ConstString.PHI + " = Atn(H / V) = " + Math.Atan(Math.Abs(H / V) / ConstNum.RADIAN) + " Degees");
					Reporting.AddLine("A = Sin(" + ConstString.PHI + ") = " + a);
					Reporting.AddLine("B = Cos(" + ConstString.PHI + ") = " + b);

					NetAndGrossArea(memberType, 0, ref Ag, ref An, ref Ag2, ref An2);
					Reporting.AddHeader("Rupture:");
					An = (L - NbVert * (HoleVert + ConstNum.SIXTEENTH_INCH)) * Th;
					Reporting.AddLine("Net Area, An = (L - Nl * (dv + " + ConstNum.SIXTEENTH_INCH + ")) * t");
					Reporting.AddLine("= (" + L + " - " + NbVert + " * (" + HoleVert + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + Th);
					Reporting.AddLine("= " + An + ConstUnit.Area);
					if (b > 0 && a > 0)
					{
						NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.18 * (a / b) * (-1 + Math.Pow(1 + Math.Pow(b / a, 2) / 0.09, 0.5)) * An * component.Material.Fu / b; // Fv1*An1/B
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.18 * (A / B) * (-1 + (1 + (B / A)² / 0.09)^0.5) * An * Fu / B");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.18 * (" + a + " / " + b + ") * (-1 + (1 + (" + b + " / " + a + ")² / 0.09)^0.5) * " + An + " * " + component.Material.Fu + " / " + b);
					}
					else if (b == 0)
					{
						NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * An * component.Material.Fu;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * An * Fu = " + ConstString.FIOMEGA0_75 + " * " + An + " * " + component.Material.Fu);
					}
					else if (a == 0)
					{
						NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.6 * An * component.Material.Fu;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.6 * An * Fu = " + ConstString.FIOMEGA0_75 + " * 0.6 * " + An + " * " + component.Material.Fu);
					}
					if (NetTensionandShearCapacity >= R)
						Reporting.AddLine("= " + NetTensionandShearCapacity + " >= " + R + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + NetTensionandShearCapacity + " << " + R + ConstUnit.Force + " (NG)");

					//Combined Tension and Shear Yielding
					Reporting.AddHeader("Yielding:");
					Ag = L * Th;
					Reporting.AddLine("Ag= L * t = " + L + " * " + Th + " = " + Ag + ConstUnit.Area);
					if (b > 0 && a > 0)
					{
						GrossTensionandShearCapacity = Math.Pow(ConstNum.FIOMEGA1_0N, 2) * 0.18 * Math.Pow(a / b, 2) * (-1 / ConstNum.FIOMEGA0_9N + Math.Pow(1 / Math.Pow(ConstNum.FIOMEGA0_9N, 2) + Math.Pow(b / a, 2) / (0.09 * Math.Pow(ConstNum.FIOMEGA1_0N, 2)), 0.5)) * Ag * component.Material.Fy / a;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.FIOMEGA1_0N + "² * 0.18 * (A / B)² * (-1 / " + ConstString.FIOMEGA0_9 + " + (1 / (" + ConstString.FIOMEGA0_9 + "²) + (B / A)² / (0.09 * " + ConstNum.FIOMEGA1_0N + "²))^0.5) * Ag * Fy/A");
						Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + "² * 0.18 * (" + a + " / " + b + ")² * (-1 / " + ConstString.FIOMEGA0_9 + " + (1 / (" + ConstString.FIOMEGA0_9 + "²) + (" + b + " / " + a + ")² / (0.09 * " + ConstNum.FIOMEGA1_0N + "²))^0.5) * " + Ag + " * " + component.Material.Fy + " / " + a);
					}
					else if (b == 0)
					{
						GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * Ag * component.Material.Fy;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Ag * Fy = " + ConstString.FIOMEGA0_9 + " * " + Ag + " * " + component.Material.Fy);
					}
					else if (a == 0)
					{
						GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * 0.6 * Ag * component.Material.Fy;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * 0.6 * Ag * Fy = " + ConstString.FIOMEGA0_9 + " * 0.6 * " + Ag + " * " + component.Material.Fy);
					}
					if (GrossTensionandShearCapacity >= R)
						Reporting.AddLine("= " + GrossTensionandShearCapacity + " >= " + R + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + GrossTensionandShearCapacity + " << " + R + ConstUnit.Force + " (NG)");

					//Block Shear 1
					if (MiscMethods.IsBeamOrColumn(memberType))
						Reporting.AddHeader("Block Shear of Beam Web:");
					else
						Reporting.AddHeader("Block Shear of Gusset:");
					Reporting.AddHeader("Vertical (An1, Ft1) and Horizontal (An2, Ft2) Sections:");

					if (!MiscMethods.IsBeamOrColumn(memberType) || CommonDataStatic.Preferences.UseContinuousClipAngles)
					{
						NetAndGrossArea(memberType, 1, ref Ag1, ref An1, ref Ag2, ref An2);
						Reporting.AddHeader("Pattern 1:");
						An1 = (component.WinConnect.ShearClipAngle.Length - 2 * bolts.EdgeDistLongDir + Lv - (bolts.NumberOfRows - 0.5) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * Th;
						Reporting.AddLine("An1 = (L - 2 * Lvs + Lvg - (Nl - 0.5) * (dv + " + ConstNum.SIXTEENTH_INCH + ")) * t");
						Reporting.AddLine("= (" + component.WinConnect.ShearClipAngle.Length + " - 2 * " + bolts.EdgeDistLongDir + " + " + Lv + " - (" + bolts.NumberOfRows + " - 0.5) * (" + webPlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + Th);
						Reporting.AddLine("= " + An1 + ConstUnit.Area);
						An2 = (ClipAngleWebLeg - Gap - bolts.EdgeDistTransvDir - (1 - 0.5) * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * Th;
						Reporting.AddLine("An2 = (W - c - Lh - (Nh - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
						Reporting.AddLine("= (" + ClipAngleWebLeg + " - " + Gap + " - " + bolts.EdgeDistTransvDir + " - (" + 1 + " - 0.5) * (" + webPlate.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + Th);
						Reporting.AddLine("= " + An2 + ConstUnit.Area);

						BlockShearCapacity1 = ShearTensionInteraction(1, R, a, b, component.Material.Fu, An1, An2);
						if (b == 0)
						{
							ft1 = 0.5 * component.Material.Fu;
							fv1 = 0;
							ft2 = 0;
							Fv2 = 0.3 * component.Material.Fu;
							BlockShearCapacity1 = 1.7 * ConstNum.FIOMEGA0_75N * (Fv2 * An2 + ft1 * An1);
							Reporting.AddLine(ConstString.PHI + " Rn = 1.7 * " + ConstString.FIOMEGA0_75 + " * (0.3 * An2 + 0.5 * An1) * Fu");
							Reporting.AddLine("= 1.7 * " + ConstString.FIOMEGA0_75 + " * (0.3 * " + An2 + " + 0.5 * " + An1 + ") * " + component.Material.Fu);
						}
						else if (a == 0)
						{
							ft1 = 0;
							fv1 = 0.3 * component.Material.Fu;
							ft2 = 0.5 * component.Material.Fu;
							Fv2 = 0;
							BlockShearCapacity1 = 1.7 * ConstNum.FIOMEGA0_75N * (fv1 * An1 + ft2 * An2);
							Reporting.AddLine(ConstString.PHI + " Rn = 1.7 *" + ConstString.FIOMEGA0_75 + " * (0.3 * An1 + 0.5 * An2) * Fu");
							Reporting.AddLine("= 1.7 * " + ConstString.FIOMEGA0_75 + " * (0.3 * " + An1 + " + 0.5 * " + An2 + ") * " + component.Material.Fu);
						}
						else
						{
							Reporting.AddHeader("Adjusted allowable stresses:");
							ft1 = (-0.09 * a + Math.Sqrt(0.0081 * Math.Pow(a, 2) + 0.09 * Math.Pow(b, 2))) * a * component.Material.Fu / Math.Pow(b, 2);
							Reporting.AddLine("Ft1 = (-0.09 * A + (0.0081 * A² + 0.09 * B²)^0.5) * A * Fu / B²");
							Reporting.AddLine("= (-0.09 * " + a + " + (0.0081 * " + a + "² + 0.09 * " + b + "²)^0.5) * " + a + " * " + component.Material.Fu + " / " + b + "²");
							Reporting.AddLine("= " + ft1 + ConstUnit.Stress);
							fv1 = ft1 * b / a;
							Reporting.AddLine(" Fv1 = ft1 * B / A = " + ft1 + " * " + b + " / " + a + " = " + fv1 + ConstUnit.Stress);
							ft2 = (-0.09 * b + Math.Sqrt(0.0081 * Math.Pow(b, 2) + 0.09 * Math.Pow(a, 2))) * b * component.Material.Fu / Math.Pow(a, 2);
							Reporting.AddLine("Ft2 = (-0.09 * B + (0.0081 * B² + 0.09 * A²)^0.5) * B * Fu / A²");
							Reporting.AddLine("= (-0.09 * " + b + " + (0.0081 * " + b + "² + 0.09 * " + a + "²)^0.5) * " + b + " * " + component.Material.Fu + " / " + a + "²");
							Reporting.AddLine("= " + ft2 + ConstUnit.Stress);

							Fv2 = ft2 * a / b;
							Reporting.AddLine(" Fv2 = ft2 * A / B = " + ft2 + " * " + a + " / " + b + " = " + Fv2 + ConstUnit.Stress);
							BlockShearCapacity1 = 1.7 * ConstNum.FIOMEGA0_75N * (fv1 * An1 + ft2 * An2) / b;
							Reporting.AddLine(ConstString.PHI + " Rn = 1.7 * " + ConstString.FIOMEGA0_75 + " * (Fv1 * An1 + Ft2 * An2) / B");
							Reporting.AddLine("= 1.7 * " + ConstString.FIOMEGA0_75 + " * (" + fv1 + " * " + An1 + " + " + ft2 + " * " + An2 + ") / " + b);
						}
						if (BlockShearCapacity1 >= R)
							Reporting.AddLine("= " + BlockShearCapacity1 + " >= " + R + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + BlockShearCapacity1 + " << " + R + ConstUnit.Force + " (NG)");
					}
					// Block Shear 2
					Reporting.AddHeader("Pattern 2:");
					NetAndGrossArea(memberType, 2, ref Ag1, ref An1, ref Ag2, ref An2);
					An1 = (component.WinConnect.ShearClipAngle.Length - 2 * bolts.EdgeDistLongDir - (bolts.NumberOfRows - 1) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * Th;
					Reporting.AddLine("An1 = (L - 2 * Lv - (Nl - 1) * (dv + " + ConstNum.SIXTEENTH_INCH + ")) * t");
					Reporting.AddLine("= (" + component.WinConnect.ShearClipAngle.Length + " - 2 * " + bolts.EdgeDistLongDir + " - (" + bolts.NumberOfRows + " - 1) * (" + webPlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + Th);
					Reporting.AddLine("= " + An1 + ConstUnit.Area);
					An2 = 2 * (ClipAngleWebLeg - Gap - bolts.EdgeDistTransvDir - (1 - 0.5) * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * Th;
					Reporting.AddLine("An2 = 2 * (W - c - Lh - (Nh - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
					Reporting.AddLine("= 2 * (" + ClipAngleWebLeg + " - " + Gap + " - " + bolts.EdgeDistTransvDir + " - (" + 1 + " - 0.5) * (" + webPlate.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + Th);
					Reporting.AddLine("= " + An2 + ConstUnit.Area);
					if (MiscMethods.IsBeamOrColumn(memberType) && CommonDataStatic.Preferences.UseContinuousClipAngles)
						BlockShearCapacity2 = ShearTensionInteraction(1, R, a, b, component.Material.Fu, An1, An2);
					else
						BlockShearCapacity2 = ShearTensionInteraction(2, R, a, b, component.Material.Fu, An1, An2);

					if (b == 0)
					{
						ft1 = 0.5 * component.Material.Fu;
						fv1 = 0;
						ft2 = 0;
						Fv2 = 0.3 * component.Material.Fu;
						BlockShearCapacity2 = 1.7 * ConstNum.FIOMEGA0_75N * (Fv2 * An2 + ft1 * An1);
						Reporting.AddLine(ConstString.PHI + " Rn = 1.7 * " + ConstString.FIOMEGA0_75 + " * (0.3 * An2 + 0.5 * An1) * Fu");
						Reporting.AddLine("= 1.7 * " + ConstString.FIOMEGA0_75 + " * (0.3 * " + An2 + " + 0.5 * " + An1 + ") *" + component.Material.Fu);
					}
					else if (a == 0)
					{
						ft1 = 0;
						fv1 = 0.3 * component.Material.Fu;
						ft2 = 0.5 * component.Material.Fu;
						Fv2 = 0;
						BlockShearCapacity2 = 1.7 * ConstNum.FIOMEGA0_75N * (fv1 * An1 + ft2 * An2);
						Reporting.AddLine(ConstString.PHI + " Rn = 1.7 * " + ConstString.FIOMEGA0_75 + " * (0.3 * An1 + 0.5 * An2) * Fu");
						Reporting.AddLine("= 1.7 * " + ConstString.FIOMEGA0_75 + " * (0.3 * " + An1 + " + 0.5 * " + An2 + ") *" + component.Material.Fu);
					}
					else
					{
						ft1 = (-0.09 * a + Math.Sqrt(0.0081 * Math.Pow(a, 2) + 0.09 * Math.Pow(b, 2))) * a * component.Material.Fu / Math.Pow(b, 2);
						fv1 = ft1 * b / a;
						ft2 = (-0.09 * b + Math.Sqrt(0.0081 * Math.Pow(b, 2) + 0.09 * Math.Pow(a, 2))) * b * component.Material.Fu / Math.Pow(a, 2);
						Fv2 = ft2 * a / b;
						Reporting.AddHeader("Adjusted allowable stresses:");
						if (!MiscMethods.IsBeamOrColumn(memberType))
							Reporting.AddHeader("(Same as pattern 1)");
						else
						{
							Reporting.AddLine("ft1 = (-0.09 * a + Sqrt(0.0081 * a² + 0.09 * b²)) * a * Fu / b²)");
							Reporting.AddLine("Ft1 = (-0.09 * A + (0.0081 * A² + 0.09 * B²)^0.5) * A * Fu / B²");
							Reporting.AddLine("= (-0.09 * " + a + " + (0.0081 * " + a + "² + 0.09 * " + b + "²)^0.5) * " + a + " * " + component.Material.Fu + " / " + b + "²");
							Reporting.AddLine("= " + ft1 + ConstUnit.Stress);
							fv1 = ft1 * b / a;
							Reporting.AddLine(" Fv1 = ft1 * B / A = " + ft1 + " * " + b + " / " + a + " = " + fv1 + ConstUnit.Stress);
							ft2 = (-0.09 * b + Math.Sqrt(0.0081 * Math.Pow(b, 2) + 0.09 * Math.Pow(a, 2))) * b * component.Material.Fu / Math.Pow(a, 2);
							Reporting.AddLine("Ft2 = (-0.09 * B + (0.0081 * B² + 0.09 * A²)^0.5) * B *  Fu / A²");
							Reporting.AddLine("= (-0.09 * " + b + " + (0.0081 * " + b + "² + 0.09 * " + a + "²)^0.5) * " + b + " * " + component.Material.Fu + " / " + a + "²");
							Reporting.AddLine("= " + ft2 + ConstUnit.Stress);

							Fv2 = ft2 * a / b;
							Reporting.AddLine(" Fv2 = ft2 * A / B = " + ft2 + " * " + a + " / " + b + " = " + Fv2 + ConstUnit.Stress);
							BlockShearCapacity1 = 1.7 * ConstNum.FIOMEGA0_75N * (fv1 * An1 + ft2 * An2) / b;
							Reporting.AddLine(ConstString.PHI + " Rn = 1.7 * " + ConstString.FIOMEGA0_75 + " * (Fv1 * An1 + Ft2 * An2) / B");
							Reporting.AddLine("= 1.7 * " + ConstString.FIOMEGA0_75 + " * (" + fv1 + " * " + An1 + " + " + ft2 + " * " + An2 + ") / " + b);
						}
						BlockShearCapacity2 = 1.7 * ConstNum.FIOMEGA0_75N * (fv1 * An1 + ft2 * An2) / b;
						Reporting.AddLine(ConstString.PHI + " Rn = 1.7 * " + ConstString.FIOMEGA0_75 + " * (Fv1 * An1 + Ft2 * An2) / B");
						Reporting.AddLine("= 1.7 * " + ConstString.FIOMEGA0_75 + "*(" + fv1 + " * " + An1 + " + " + ft2 + " * " + An2 + ")/" + b);
					}
					if (BlockShearCapacity2 >= R)
						Reporting.AddLine("= " + BlockShearCapacity2 + " >= " + R + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + BlockShearCapacity2 + " << " + R + ConstUnit.Force + " (NG)");
					break;
			}
		}
	}
}