using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class DirectlyWelded
	{
		internal static void DesignDirectlyWelded(EMemberType memberType)
		{
			double TplateMax = 0;
			double TplateMin = 0;
			double wm = 0;
			double colshear = 0;
			double tPlReqw = 0;
			double WebWeld = 0;
			double tPlReqf = 0;
			double welds0 = 0;
			double discr = 0;
			double wminW = 0;
			double wminf = 0;
			double FicPn = 0;
			double Fcr = 0;
			double Lamda_c = 0;
			double klr = 0;
			double Ag = 0;
			double t_slenderness = 0;
			double t2 = 0;
			double FforShear = 0;
			double lweb = 0;
			double Lflange = 0;
			double t3 = 0;
			double ratio = 0;
			double B = 0;
			double t1 = 0;
			double flangeForce = 0;
			double t = 0;
			double tPlReq = 0;
			double welds = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			var directWeld = beam.WinConnect.MomentDirectWeld;

			flangeForce = Math.Abs(beam.P / 2) + Math.Abs(beam.Moment / (beam.Shape.d - beam.Shape.tf));

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BeamToColumnWeb:
					t1 = Math.Max(flangeForce / (ConstNum.FIOMEGA1_0N * directWeld.Material.Fy * beam.Shape.bf), flangeForce / (ConstNum.FIOMEGA0_75N * directWeld.Material.Fu * beam.Shape.bf));
					if (!beam.EndSetback_User)
						beam.EndSetback = directWeld.Top.a;
					// Width Thickness Ratio
					B = (column.Shape.bf - column.Shape.tw) / 2 + directWeld.Top.a;
					ratio = 0.56 * Math.Pow(ConstNum.ELASTICITY / directWeld.Material.Fy, 0.5);
					t3 = B / ratio;
					if ((!rightBeam.IsActive || !leftBeam.IsActive) &&
					    (directWeld.Type == ENumbers.Four || directWeld.Type == ENumbers.One))
					{
						Lflange = column.Shape.bf / 2 - column.Shape.k1;
						lweb = column.Shape.t;
						FforShear = (flangeForce + CommonDataStatic.F_Shear) / 4;
					}
					else
					{
						Lflange = column.Shape.bf / 2 - column.Shape.k1;
						lweb = 0;
						FforShear = (flangeForce + CommonDataStatic.F_Shear) / 2;
					}

					t2 = Math.Max(t1, t3);
					beam.WinConnect.Fema.L = directWeld.Top.a;

					t_slenderness = B / (0.56 * Math.Pow(ConstNum.ELASTICITY / directWeld.Material.Fy, 0.5));
					t3 = t_slenderness - ConstNum.SIXTEENTH_INCH;
					do
					{
						t3 = t3 + ConstNum.SIXTEENTH_INCH;
						Ag = t3 * beam.Shape.bf;
						klr = 4.157 * B / t3;
						Lamda_c = klr / Math.PI * Math.Sqrt(directWeld.Material.Fy / ConstNum.ELASTICITY);
						if (klr <= 25)
							Fcr = directWeld.Material.Fy;
						else if (Lamda_c <= 1.5)
							Fcr = Math.Pow(0.658, Math.Pow(Lamda_c, 2)) * directWeld.Material.Fy;
						else
							Fcr = 0.877 / Math.Pow(Lamda_c, 2) * directWeld.Material.Fy;
						FicPn = ConstNum.FIOMEGA0_9N * Ag * Fcr;
					} while (FicPn < flangeForce);

					t = Math.Max(t2, t3);
					if (t < beam.Shape.tf + ConstNum.EIGHTH_INCH)
					{
						t = beam.Shape.tf + ConstNum.EIGHTH_INCH;
					}
					directWeld.Top.ExtensionThickness = CommonCalculations.PlateThickness(NumberFun.Round(t, 16));
					directWeld.Top.StiffenerThickness = directWeld.Top.ExtensionThickness;
					wminf = CommonCalculations.MinimumWeld(directWeld.Top.ExtensionThickness, column.Shape.tf);
					wminW = CommonCalculations.MinimumWeld(directWeld.Top.ExtensionThickness, column.Shape.tw);
					if ((!rightBeam.IsActive || !leftBeam.IsActive) &&
					    (directWeld.Type == ENumbers.Four || directWeld.Type == ENumbers.One))
					{
						Lflange = column.Shape.bf / 2 - column.Shape.k1;
						lweb = column.Shape.t;
						FforShear = (flangeForce + CommonDataStatic.F_Shear) / 4;
					}
					else
					{
						Lflange = column.Shape.bf / 2 - column.Shape.k1;
						lweb = 0;
						FforShear = (flangeForce + CommonDataStatic.F_Shear) / 2;
					}
					discr = Math.Pow(Lflange, 2) / 16.0 - FforShear / (1.2726 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					if (discr >= 0)
						welds0 = Lflange / 4 - Math.Sqrt(discr);
					else
						welds0 = 0;

					if (directWeld.WeldFlangeType == EWeldType.CJP)
					{
						welds = 0;
						tPlReqf = FforShear / Math.Min(0.48 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Lflange, ConstNum.FIOMEGA1_0N * 0.6 * directWeld.Material.Fy * Lflange);
					}
					else
					{
						welds = NumberFun.Round(Math.Max(welds0, wminf), 16);
						tPlReqf = FforShear / (ConstNum.FIOMEGA1_0N * 0.6 * directWeld.Material.Fy * (Lflange - 2 * welds));
					}

					if (lweb > 0)
					{
						WebWeld = Math.Max(2 * FforShear / (ConstNum.FIOMEGA0_75N * 0.8484 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * lweb), wminW);
						if (directWeld.WeldFlangeType == EWeldType.CJP)
							WebWeld = 0;
						tPlReqw = 2 * FforShear / (ConstNum.FIOMEGA1_0N * directWeld.Material.Fy * lweb);
					}
					else
					{
						tPlReqw = 0;
						WebWeld = 0;
					}
					tPlReq = Math.Max(tPlReqf, tPlReqw);
					if (directWeld.Top.ExtensionThickness < tPlReq)
						directWeld.Top.ExtensionThickness = CommonCalculations.PlateThickness(NumberFun.Round(tPlReq, 16));
					wminf = CommonCalculations.MinimumWeld(directWeld.Top.ExtensionThickness, column.Shape.tf);
					wminW = CommonCalculations.MinimumWeld(directWeld.Top.ExtensionThickness, column.Shape.tw);
					if (directWeld.WeldFlangeType == EWeldType.CJP)
					{
						colshear = 2 * column.Shape.tf * Lflange * ConstNum.FIOMEGA1_0N * 0.6 * column.Material.Fy;
						if (colshear < FforShear)
							Reporting.AddLine("Column flange thickness cannot develop the force delivered by plate.");
					}
					else
					{
						wm = column.Shape.tf * column.Material.Fu / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						if (welds > wm)
							Reporting.AddLine("Column flange thickness cannot develop weld strength.");
						welds = Math.Max(welds, wminf);
					}
					directWeld.Top.FilletWeldW1 = NumberFun.Round(welds, 16);
					if ((!rightBeam.IsActive || !leftBeam.IsActive) &&
					    (directWeld.Type == ENumbers.Four || directWeld.Type == ENumbers.One))
						directWeld.Top.FilletWeldW2 = NumberFun.Round(Math.Max(WebWeld, wminW), 16);
					else if (directWeld.Type != ENumbers.Two && directWeld.Type != ENumbers.Five)
						directWeld.Top.FilletWeldW2 = wminW;
					else
						directWeld.Top.FilletWeldW2 = 0;

					if (directWeld.Bottom.ExtensionThickness < beam.Shape.tf + ConstNum.THREE_EIGHTS_INCH)
					{
						directWeld.Bottom.ExtensionThickness = CommonCalculations.PlateThickness(NumberFun.Round(beam.Shape.tf + ConstNum.THREE_EIGHTS_INCH, 16));
						directWeld.Bottom.StiffenerThickness = directWeld.Bottom.ExtensionThickness;
					}
					if (directWeld.Top.ExtensionThickness < beam.Shape.tf + ConstNum.QUARTER_INCH)
					{
						directWeld.Top.ExtensionThickness = CommonCalculations.PlateThickness(NumberFun.Round(beam.Shape.tf + ConstNum.QUARTER_INCH, 16));
						directWeld.Top.StiffenerThickness = directWeld.Top.ExtensionThickness;
					}
					TplateMin = Math.Min(directWeld.Top.ExtensionThickness, directWeld.Bottom.ExtensionThickness);
					TplateMax = Math.Max(directWeld.Top.ExtensionThickness, directWeld.Bottom.ExtensionThickness);
					wminf = CommonCalculations.MinimumWeld(TplateMax, column.Shape.tf);
					wminW = CommonCalculations.MinimumWeld(TplateMax, column.Shape.tw);
					if (directWeld.WeldFlangeType == EWeldType.CJP)
					{
						colshear = 2 * column.Shape.tf * Lflange * ConstNum.FIOMEGA1_0N * 0.6 * column.Material.Fy;
						if (colshear < FforShear)
							Reporting.AddLine("Column flange thickness cannot develop the force delivered by plate.");
					}
					else
					{
						wm = column.Shape.tf * column.Material.Fu / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						if (welds > wm)
							Reporting.AddLine("Column flange thickness cannot develop weld strength.");
						welds = Math.Max(welds, wminf);
					}
					directWeld.Top.FilletWeldW1 = NumberFun.Round(welds, 16);
					directWeld.Bottom.FilletWeldW1 = NumberFun.Round(welds, 16);
					if ((!rightBeam.IsActive || !leftBeam.IsActive) && (directWeld.Type == ENumbers.Four || directWeld.Type == ENumbers.One))
						directWeld.Top.FilletWeldW2 = NumberFun.Round(Math.Max(WebWeld, wminW), 16);
					else if (directWeld.Type != ENumbers.Two && directWeld.Type != ENumbers.Five)
						directWeld.Top.FilletWeldW2 = wminW;
					else
						directWeld.Top.FilletWeldW2 = 0;
					directWeld.Bottom.FilletWeldW2 = directWeld.Top.FilletWeldW2;
					break;
				default:
					if (!beam.EndSetback_User && beam.EndSetback > ConstNum.THREE_EIGHTS_INCH)
						beam.EndSetback = ConstNum.THREE_EIGHTS_INCH;
					if (!beam.EndSetback_User && beam.EndSetback < ConstNum.THREE_SIXTEENTHS)
						beam.EndSetback = ConstNum.THREE_SIXTEENTHS;
					break;
			}

			DirectlyWeldedCapacity(memberType, ref TplateMin, ref TplateMax, ref flangeForce, ref Lflange, ref lweb, ref FforShear, ref B, ref ratio, ref klr, ref Fcr, ref wminf, ref wminW, ref colshear);
		}

		private static void DirectlyWeldedCapacity(EMemberType memberType, ref double TplateMin, ref double TplateMax, ref double flangeForce, ref double Lflange, ref double lweb, ref double FforShear, ref double B, ref double ratio, ref double kL_r, ref double Fcr, ref double wminf, ref double wminW, ref double colshear)
		{
			double WeldCap = 0;
			double Cap = 0;
			double bovt = 0;
			double WidthforBuckling = 0;
			double FiRn = 0;
			double WeldLf = 0;
			double capacity = 0;
			double Fym = 0;

			if (CommonDataStatic.IsFema)
			{
				FemaReporting.DesignFemaReporting(memberType);
				return;
			}

			if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic)
				return;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

			var directWeld = beam.WinConnect.MomentDirectWeld;

			Reporting.AddHeader(beam.ComponentName + " - " + beam.ShapeName + " Moment Connection");

			Reporting.AddHeader("Moment Connection With Directly Welded Flanges:");
			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BeamToColumnWeb:
					TplateMin = Math.Min(directWeld.Top.ExtensionThickness, directWeld.Bottom.ExtensionThickness);
					TplateMax = Math.Max(directWeld.Top.ExtensionThickness, directWeld.Bottom.ExtensionThickness);
					if (TplateMin == TplateMax)
						Reporting.AddHeader(TplateMin + ConstUnit.Length + " Flange Extension Plate - " + directWeld.Material.Name);
					else
					{
						Reporting.AddLine(directWeld.Top.ExtensionThickness + ConstUnit.Length + " Top Flange Extension Plate - " + directWeld.Material.Name);
						Reporting.AddLine(directWeld.Bottom.ExtensionThickness + ConstUnit.Length + " Bottom Flange Extension Plate - " + directWeld.Material.Name);
					}

					flangeForce = Math.Abs(beam.P / 2) + Math.Abs(beam.Moment / (beam.Shape.d - beam.Shape.tf));
					Reporting.AddHeader("Flange Force (Ff):");
					if (CommonDataStatic.Units == EUnit.US)
					{
						Reporting.AddLine("= P / 2 + M / (d - tf)");
						Reporting.AddLine("= " + Math.Abs(beam.P) + " / 2 + " + Math.Abs(beam.Moment) + "/ (" + beam.Shape.d + " - " + beam.Shape.tf + ")");
					}
					else
					{
						Reporting.AddLine("= P / 2 + M / (d - tf)");
						Reporting.AddLine("= " + Math.Abs(beam.P) + " / 2 + * " + Math.Abs(beam.Moment) + "/ (" + beam.Shape.d + " - " + beam.Shape.tf + ")");
					}
					Reporting.AddLine("= " + flangeForce + ConstUnit.Force);

					Reporting.AddHeader("Check Flange Extension Plate");
					Reporting.AddHeader("Beam Flange-to-Plate Weld:");
					WeldCap = ConstNum.FIOMEGA0_9N * beam.Material.Fy * beam.Shape.bf * beam.Shape.tf;
					Reporting.AddLine("Weld Strength = " + ConstString.FIOMEGA0_9 + " * Fy * bf * tf");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + beam.Material.Fy + " * " + beam.Shape.bf + " * " + beam.Shape.tf);
					if (WeldCap >= flangeForce)
						Reporting.AddCapacityLine("= " + WeldCap + " >= " + flangeForce + ConstUnit.Force + " (OK)", flangeForce / WeldCap, "Beam Flange-to-Plate Weld", memberType);
					else
						Reporting.AddCapacityLine("= " + WeldCap + " << " + flangeForce + ConstUnit.Force + " (NG)", flangeForce / WeldCap, "Beam Flange-to-Plate Weld", memberType);

					Cap = ConstNum.FIOMEGA0_9N * directWeld.Material.Fy * beam.Shape.bf * TplateMin;
					Reporting.AddLine("Extension PL Tension Strength (Yielding) = " + ConstString.FIOMEGA0_9 + " * Fy * bf * tp");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + "  * " + directWeld.Material.Fy + " * " + beam.Shape.bf + " * " + TplateMin);
					if (Cap >= flangeForce)
						Reporting.AddCapacityLine("= " + Cap + " >= " + flangeForce + ConstUnit.Force + " (OK)", flangeForce / Cap, "Extension PL Tension Strength (Yielding)", memberType);
					else
						Reporting.AddCapacityLine("= " + Cap + " << " + flangeForce + ConstUnit.Force + " (NG)", flangeForce / Cap, "Extension PL Tension Strength (Yielding)", memberType);

					Cap = ConstNum.FIOMEGA0_75N * directWeld.Material.Fu * beam.Shape.bf * TplateMin;
					Reporting.AddLine("Extension PL Tension Strength (Fracture) = " + ConstString.FIOMEGA0_75 + " * Fu * bf * tp");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + directWeld.Material.Fu + " * " + beam.Shape.bf + " * " + TplateMin);
					if (Cap >= flangeForce)
						Reporting.AddCapacityLine("= " + Cap + " >= " + flangeForce + ConstUnit.Force + " (OK)", flangeForce / Cap, "Extension PL Tension Strength (Fracture)", memberType);
					else
						Reporting.AddCapacityLine("= " + Cap + " << " + flangeForce + ConstUnit.Force + " (NG)", flangeForce / Cap, "Extension PL Tension Strength (Fracture)", memberType);

					Reporting.AddHeader("Plate Shear Strength at Column Flange Weld: ");
					Reporting.AddHeader("Force at each half-flange (Fs):");
					if ((!rightBeam.IsActive || !leftBeam.IsActive) && (directWeld.Type == ENumbers.Four || directWeld.Type == ENumbers.One))
					{
						Lflange = column.Shape.bf / 2 - column.Shape.k1;
						lweb = column.Shape.t;
						FforShear = (flangeForce + CommonDataStatic.F_Shear) / 4;
						Reporting.AddLine("Fs = (Ff + F_Shear) / 4 = (" + flangeForce + " + " + CommonDataStatic.F_Shear + ") / 4 = " + FforShear + ConstUnit.Force);
					}
					else
					{
						Lflange = column.Shape.bf / 2 - column.Shape.k1;
						lweb = 0;
						FforShear = (flangeForce + CommonDataStatic.F_Shear) / 2;
						Reporting.AddLine("Fs = (Ff + F_Shear) / 2 = (" + flangeForce + " + " + CommonDataStatic.F_Shear + ") / 2 = " + FforShear + ConstUnit.Force);
					}

					Reporting.AddHeader("F_Shear = " + CommonDataStatic.F_Shear + ConstUnit.Force + " is from shear plate.");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.FIOMEGA1_0N + " * 0.6 * Fy * tp * Ls");
					if (directWeld.WeldFlangeType == EWeldType.CJP)
					{
						Cap = ConstNum.FIOMEGA1_0N * 0.6 * directWeld.Material.Fy * TplateMin * Lflange;
						Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + directWeld.Material.Fy + " * " + TplateMin + "  * " + Lflange);
					}
					else
					{
						Cap = ConstNum.FIOMEGA1_0N * 0.6 * directWeld.Material.Fy * TplateMin * (Lflange - 2 * directWeld.Top.FilletWeldW1);
						Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + directWeld.Material.Fy + " * " + TplateMin + "  * (" + Lflange + " - 2 * " + directWeld.Top.FilletWeldW1 + ")");
					}
					if (Cap >= FforShear)
						Reporting.AddCapacityLine("= " + Cap + " >= " + FforShear + ConstUnit.Force + " (OK)", FforShear / Cap, "test", memberType);
					else
						Reporting.AddCapacityLine("= " + Cap + " << " + FforShear + ConstUnit.Force + " (NG)", FforShear / Cap, "test", memberType);

					B = (column.Shape.bf - column.Shape.tw) / 2 + directWeld.Top.a;
					ratio = 0.56 * Math.Pow(ConstNum.ELASTICITY / directWeld.Material.Fy, 0.5);
					bovt = B / TplateMin;

					Reporting.AddLine("Width / Thickness = " + B + " / " + TplateMin);
					if (bovt <= ratio)
						Reporting.AddLine("= " + bovt + " <= 0.56 * (E / Fy)^0.5 = " + ratio + " (OK)");
					else
						Reporting.AddLine("= " + bovt + " >> 0.56 * (E / Fy)^0.5 = " + ratio + " (NG)");

					Reporting.AddHeader("Flange Extension PL Buckling:");
					beam.WinConnect.Fema.L = directWeld.Bottom.a;

					WidthforBuckling = Math.Min(beam.Shape.bf, column.Shape.d - 2 * column.Shape.tf);
					Reporting.AddLine("Unbraced Length (L) = " + beam.WinConnect.Fema.L + ConstUnit.Length + ", Effective Length Factor, K = 1.2");

					kL_r = directWeld.Bottom.a * 1.2 / (TplateMin / Math.Sqrt(12));
					Reporting.AddLine("KL / r = k * L / (t / 3.464) = " + 1.2 + " * " + directWeld.Bottom.a + " / (" + TplateMin + " / 3.464) = " + kL_r);
					Fcr = CommonCalculations.BucklingStress(kL_r, directWeld.Material.Fy, true);

					FiRn = ConstNum.FIOMEGA0_9N * Fcr * WidthforBuckling * TplateMin;
					if (FiRn >= flangeForce)
						Reporting.AddCapacityLine(ConstString.PHI + " cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + WidthforBuckling + " * " + TplateMin + " = " + FiRn + " >= " + flangeForce + ConstUnit.Force + " (OK)", flangeForce / FiRn, "Flange Extension PL Buckling", beam.MemberType);
					else
						Reporting.AddCapacityLine(ConstString.PHI + " cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + WidthforBuckling + " * " + TplateMin + " = " + FiRn + " << " + flangeForce + ConstUnit.Force + " (NG)", flangeForce / FiRn, "Flange Extension PL Buckling", beam.MemberType);

					directWeld.Top.StiffenerThickness = directWeld.Top.ExtensionThickness;
					directWeld.Bottom.StiffenerThickness = directWeld.Bottom.ExtensionThickness;

					wminf = CommonCalculations.MinimumWeld(TplateMax, column.Shape.tf);
					wminW = CommonCalculations.MinimumWeld(TplateMax, column.Shape.tw);

					Reporting.AddHeader("Extension Plate to Support Weld:");
					if ((!rightBeam.IsActive || !leftBeam.IsActive) && (directWeld.Type == ENumbers.Four || directWeld.Type == ENumbers.One))
					{
						Lflange = column.Shape.bf / 2 - column.Shape.k1;
						lweb = column.Shape.t;
						FforShear = (flangeForce + CommonDataStatic.F_Shear) / 4;
					}
					else
					{
						Lflange = column.Shape.bf / 2 - column.Shape.k1;
						lweb = 0;
						FforShear = (flangeForce + CommonDataStatic.F_Shear) / 2;
					}

					if (directWeld.WeldFlangeType == EWeldType.CJP)
						WeldLf = Lflange;
					else
						WeldLf = Lflange - 2 * directWeld.Top.FilletWeldW1;

					Reporting.AddHeader("Weld to column flange:");
					Reporting.AddHeader("See above for the weld force.");
					if (directWeld.WeldFlangeType == EWeldType.CJP)
						Reporting.AddHeader("Plate to flange weld is CJP");
					else
					{
						Reporting.AddHeader("Minimum fillet weld size:");
						wminf = CommonCalculations.MinimumWeld(TplateMax, column.Shape.tf);

						if (wminf <= directWeld.Top.FilletWeldW1)
							Reporting.AddLine("wmin = " + wminf + " <= " + directWeld.Top.FilletWeldW1 + ConstUnit.Length + " (OK)");
						else
							Reporting.AddLine("wmin = " + wminf + " >> " + directWeld.Top.FilletWeldW1 + ConstUnit.Length + " (NG)");
					}

					Reporting.AddHeader("Weld Strength at Each Half-Flange:");
					if (directWeld.WeldFlangeType == EWeldType.CJP)
					{
						capacity = 0.48 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * TplateMin * WeldLf;
						Reporting.AddLine(ConstString.PHI + " Rn = 0.48 * Fexx * tp * Lw = 0.48 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + beam.Shape.a + " * " + WeldLf);
					}
					else
					{
						capacity = ConstNum.FIOMEGA0_75N * 0.8484 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * directWeld.Top.FilletWeldW1 * WeldLf;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.8484 * Fexx * w * Lw = " + ConstString.FIOMEGA0_75 + " * 0.8484 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + directWeld.Top.FilletWeldW1 + " * " + WeldLf);
					}
					if (capacity >= FforShear)
						Reporting.AddCapacityLine("= " + capacity + " >= " + FforShear + ConstUnit.Force + " (OK)", FforShear / capacity, " Weld Strength at Each Half-Flange", memberType);
					else
						Reporting.AddCapacityLine("= " + capacity + " << " + FforShear + ConstUnit.Force + " (NG)", FforShear / capacity, " Weld Strength at Each Half-Flange", memberType);

					Reporting.AddHeader("Column Flange Shear at Welds:");

					int numberForShear = CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn ? 1 : 2;

					colshear = numberForShear * column.Shape.tf * Lflange * ConstNum.FIOMEGA1_0N * 0.6 * column.Material.Fy;
					Reporting.AddLine(ConstString.PHI + " Rn = " + numberForShear + " * tf * L * " + ConstNum.FIOMEGA1_0N + " * 0.6 * Fy = " + numberForShear + " * " + column.Shape.tf + " * " + Lflange + " * " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + column.Material.Fy);
					if (colshear >= FforShear)
						Reporting.AddCapacityLine("= " + colshear + " >= " + FforShear + ConstUnit.Force + " (OK)", FforShear / capacity, " Weld Strength at Each Half-Flange", memberType);
					else
						Reporting.AddCapacityLine("= " + colshear + " << " + FforShear + ConstUnit.Force + " (NG)", FforShear / capacity, " Weld Strength at Each Half-Flange", memberType);

					switch (directWeld.Type)
					{
						case ENumbers.Zero:
						case ENumbers.One:
						case ENumbers.Two:
						case ENumbers.Three:
							Reporting.AddHeader("Weld to Column Web:");
							if (directWeld.WeldWebType == EWeldType.CJP)
								Reporting.AddHeader("Plate to web weld is CJP");
							else
							{
								wminW = CommonCalculations.MinimumWeld(TplateMax, column.Shape.tw);
								if (wminW <= directWeld.Top.FilletWeldW2)
									Reporting.AddLine("wmin = " + wminW + " <= " + directWeld.Top.FilletWeldW2 + ConstUnit.Length + " (OK)");
								else
									Reporting.AddLine("wmin = " + wminW + " >> " + directWeld.Top.FilletWeldW2 + ConstUnit.Length + " (NG)");
							}
							if (lweb > 0)
							{
								if (directWeld.WeldWebType == EWeldType.CJP)
								{
									capacity = ConstNum.FIOMEGA1_0N * Math.Min(column.Material.Fy, directWeld.Material.Fy) * TplateMin * lweb;
									Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.FIOMEGA1_0N + " * Fy * tp * Lw = " + ConstNum.FIOMEGA1_0N + "   * " + Math.Min(column.Material.Fy, directWeld.Material.Fy) + " * " + beam.Shape.a + " * " + lweb);
								}
								else
								{
									capacity = ConstNum.FIOMEGA0_75N * 0.8484 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * directWeld.Top.FilletWeldW2 * lweb;
									Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.8484 * Fexx * w * Lw = " + ConstString.FIOMEGA0_75 + " * 0.8484 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + directWeld.Top.FilletWeldW2 + " * " + lweb);
								}
								if (capacity >= 2 * FforShear)
									Reporting.AddCapacityLine("= " + capacity + " >= " + 2 * FforShear + ConstUnit.Force + " (OK)", 2 * FforShear / capacity, "Weld to Column Web", memberType);
								else
									Reporting.AddCapacityLine("= " + capacity + " << " + 2 * FforShear + ConstUnit.Force + " (NG)", 2 * FforShear / capacity, "Weld to Column Web", memberType);
							}
							break;
					}
					break;
				default:
					Reporting.AddGoToHeader("Weld Strength");
					flangeForce = Math.Abs(beam.P / 2) + Math.Abs(beam.Moment / (beam.Shape.d - beam.Shape.tf));
					Reporting.AddLine("Flange Force (Ff):");
					if (CommonDataStatic.Units == EUnit.US)
					{
						Reporting.AddLine("= P / 2 + M / (d - tf)");
						Reporting.AddLine("= (" + Math.Abs(beam.P) + " / 2) + " + Math.Abs(beam.Moment) + " / (" + beam.Shape.d + " - " + beam.Shape.tf + ")");
					}
					else
					{
						Reporting.AddLine("= P / 2 + M / (d - tf)");
						Reporting.AddLine("= " + Math.Abs(beam.P) + " / 2) + " + Math.Abs(beam.Moment) + " / (" + beam.Shape.d + " - " + beam.Shape.tf + ")");
					}
					Reporting.AddLine("= " + flangeForce + ConstUnit.Force);

					Fym = Math.Min(column.Material.Fy, beam.Material.Fy);
					if (beam.Shape.bf != 0 && column.Shape.B != 0)
						B = Math.Min(beam.Shape.bf, column.Shape.B);
					else
						B = Math.Max(beam.Shape.bf, column.Shape.B);

					//capacity = ConstNum.FIOMEGA0_9N * Fym * B * beam.Shape.tf;
					capacity = ConstNum.FIOMEGA0_9N * Fym * beam.Shape.zx / (beam.Shape.d - beam.Shape.tf);
					Reporting.AddLine(string.Empty);

					string capacityString = "Full Penetration Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH;
					Reporting.AddLine(capacityString + " = " + ConstString.FIOMEGA0_9 + " * Fy * Zx / (d - tf)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + Fym + " * " + beam.Shape.zx + " / (" + beam.Shape.d + " - " + beam.Shape.tf + ")");
					if (capacity >= flangeForce)
						Reporting.AddCapacityLine("= " + capacity + " >= " + flangeForce + ConstUnit.Force + " (OK)", flangeForce / capacity, capacityString, memberType);
					else
						Reporting.AddCapacityLine("= " + capacity + " << " + flangeForce + ConstUnit.Force + " (NG)", flangeForce / capacity, capacityString, memberType);

					// This logic is completely new to Descon 8 (MT 6/8/15)
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
					{
						double beta;

						bool limitPass = true; // Used to skip most of the logic if any of the first few checks are false

						Reporting.AddHeader("Concentrated Forces on HSS");
						Reporting.AddHeader("Check General Limits of Applicability");

						if (column.Material.Fy <= 52.0)
							Reporting.AddLine("Fy = " + column.Material.Fy + ConstUnit.Stress + " <= 52.0" + ConstUnit.Stress + " (Within Limit)");
						else
						{
							Reporting.AddLine("Fy = " + column.Material.Fy + ConstUnit.Stress + " >> 52.0" + ConstUnit.Stress + " (Beyond Limit)");
							limitPass = false;
						}

						if (column.Material.Fy / beam.Material.Fu <= 0.8)
							Reporting.AddLine("Fy / Fu = " + column.Material.Fy + " / " + column.Material.Fu + " = " + (column.Material.Fy / beam.Material.Fu) + " <= 0.8 (Within Limit)");
						else
						{
							Reporting.AddLine("Fy / Fu = " + column.Material.Fy + " / " + column.Material.Fu + " = " + (column.Material.Fy / beam.Material.Fu) + " >> 0.8 (Beyond Limit)");
							limitPass = false;
						}

						beta = beam.Shape.bf / column.Shape.B;
						Reporting.AddLine("Beta = Bp / B = " + beam.Shape.bf + " / " + column.Shape.B + " = " + beta);
						if (0.25 < beta && beta <= 1.0)
							Reporting.AddLine("0.25 < " + beta + " <= 1.0 (Within Limit)");
						else
						{
							Reporting.AddLine("0.25 < " + beta + " >> 1.0 (Beyond Limit)");
							limitPass = false;
						}

						if (column.Shape.B / column.Shape.tf <= 35.0)
							Reporting.AddLine("B / t = " + column.Shape.B + " / " + column.Shape.tw + " = " + (column.Shape.B / column.Shape.tf) + " <= 35.0 (Within Limit)");
						else
						{
							Reporting.AddLine("B / t = " + column.Shape.B + " / " + column.Shape.tw + " = " + (column.Shape.B / column.Shape.tf) + " >> 35.0 (Beyond Limit)");
							limitPass = false;
						}

						if (!limitPass)
							Reporting.AddLine("AISC Limits of Applicability Not Met. (NG)");
						else
						{
							double limitPunching1;
							double limitPunching2;
							double yielding;
							double yieldingMax;

							// Equation K1-2
							yielding = ConstNum.FIOMEGA0_95N * (10 * column.Material.Fy * column.Shape.tf / (column.Shape.B / column.Shape.tf)) * beam.Shape.bf;
							yieldingMax = ConstNum.FIOMEGA0_95N * beam.Material.Fy * beam.Shape.tf * beam.Shape.bf;
							Reporting.AddHeader("Local Yielding Due to Uneven Load Distribution:");
							Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.PHI + " * (10 * Fy * t / (B / t)) * Bp <= " + ConstString.PHI + " * Fyp * tp * Bp");
							Reporting.AddLine("= " + ConstString.FIOMEGA0_95 + " * (10 * " + column.Material.Fy + " * " + column.Shape.tf + " / (" + column.Shape.B + " / " + column.Shape.tf + ")) * " + beam.Shape.bf + " = " + yielding);
							Reporting.AddLine("= " + yielding + " < " + yieldingMax + ConstUnit.Stress + " = " + ConstString.FIOMEGA0_95 + " * " + beam.Material.Fy + " * " + beam.Shape.tf + " * " + beam.Shape.bf);

							if (yieldingMax > flangeForce)
								Reporting.AddLine("= " + yielding + " >> " + flangeForce + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= " + yielding + " <= " + flangeForce + ConstUnit.Force + " (NG)");

							Reporting.AddHeader("Shear Yielding (Punching):");
							Reporting.AddLine("Check Limits of Applicability");

							limitPunching1 = column.Shape.B - 2 * column.Shape.tf;
							Reporting.AddLine("B - 2 * t = " + limitPunching1);
							if (beam.Shape.bf <= limitPunching1)
								Reporting.AddLine("Bp = " + beam.Shape.bf + " <= " + limitPunching1 + " (Within Limit)");
							else
								Reporting.AddLine("Bp = " + beam.Shape.bf + " >> " + limitPunching1 + " (Beyond Limit)");

							limitPunching2 = 0.85 * column.Shape.B;
							Reporting.AddLine("0.85 * B = " + limitPunching2);
							if (beam.Shape.bf > limitPunching2)
								Reporting.AddLine("Bp = " + beam.Shape.bf + " >> " + limitPunching2 + " (Within Limit)");
							else
								Reporting.AddLine("Bp = " + beam.Shape.bf + " <= " + limitPunching2 + " (Beyond Limit)");

							Reporting.AddLine(string.Empty);

							if (beam.Shape.bf > limitPunching1 || beam.Shape.bf <= limitPunching2)
								Reporting.AddLine("(Limit State Does Not Apply)");
							else
							{
								double limitState;
								double bep;

								// Equation K1-3
								bep = 10 * beam.Shape.bf / (column.Shape.B / column.Shape.tf);
								limitState = ConstNum.FIOMEGA0_95N * 0.6 * column.Material.Fy * column.Shape.tf * (2 * beam.Shape.tf + 2 * bep);
								Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.PHI + " * 0.6 * Fy * t * (2 * tp + 2 * Bep)");
								Reporting.AddLine("= " + ConstString.FIOMEGA0_95 + " * 0.6 * " + column.Material.Fy + " * " + column.Shape.tf + " * (2 * " + beam.Shape.tf + " + 2 * " + bep + ") = " + limitState);

								if (limitState > flangeForce)
									Reporting.AddLine("= " + limitState + " >> " + flangeForce + ConstUnit.Force + " (OK)");
								else
									Reporting.AddLine("= " + limitState + " <= " + flangeForce + ConstUnit.Force + " (NG)");
							}

							Reporting.AddLine(string.Empty);
							if (beta < 1.0)
							{
								Reporting.AddLine("Beta = " + beta + " < 1.0");
								Reporting.AddLine("Limit States of Sidewall Local Yielding, Sidewall Local Crippling and Sidewall Local Buckling Do Not Apply.");
							}
							else
							{
								double limit;
								double k;
								double qf;
								double u;
								double fc;

								Reporting.AddLine("Beta = " + beta + " >= 1.0");

								// Equation K1-4
								Reporting.AddHeader("Check Limit State of Sidewall Local Yielding");
								k = 1.5 * column.Shape.tf;
								limit = ConstNum.FIOMEGA1_0N * 2 * column.Material.Fy * column.Shape.tf * (5 * k + beam.Shape.tf);
								Reporting.AddLine(ConstString.FIOMEGA1_0 + " * 2 * Fy * t * (5 * k + N)");
								Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + " * 2 * " + column.Material.Fy + " * " + column.Shape.tf + " * (5 * " + k + " * " + beam.Shape.tf + ") = " + limit);

								if (limit > flangeForce)
									Reporting.AddLine("= " + limit + " >> " + flangeForce + ConstUnit.Force + " (OK)");
								else
									Reporting.AddLine("= " + limit + " <= " + flangeForce + ConstUnit.Force + " (NG)");

								// Equation K1-5
								Reporting.AddHeader("Check Limit State of Sidewall Local Crippling");
								if (column.Moment > 0)
								{
									fc = CommonDataStatic.Preferences.CalcMode == ECalcMode.LRFD ? column.Material.Fy : 0.6 * column.Material.Fy;
									u = Math.Abs(column.P / (column.Shape.a * fc) + column.Moment / (column.Shape.sx * fc));
									qf = 1.3 - 0.4 * u / beta;
								}
								else
									qf = 1;

								limit = ConstNum.FIOMEGA0_75N * 1.6 * Math.Pow(column.Shape.tf, 2) * (1 + 3 * beam.Shape.tf / (column.Shape.Ht - 3 * column.Shape.tf)) * Math.Sqrt(ConstNum.ELASTICITY * column.Material.Fy) * qf;
								Reporting.AddLine(ConstString.FIOMEGA0_75 + " * 1.6 * t² * (1 + 3 * N / (H - 3 * t)) * (E * Fy)^0.5 * Qf");
								Reporting.AddLine("= " + ConstNum.FIOMEGA0_75N + " * 1.6 * " + Math.Pow(column.Shape.tf, 2) + " * (1 + 3 * " + beam.Shape.tf + " / (" + column.Shape.Ht + " - 3 * " + column.Shape.tf + ")) * (E * " + column.Material.Fy + ")^0.5 * " + qf + " = " + limit);

								if (limit > flangeForce)
									Reporting.AddLine("= " + limit + " >> " + flangeForce + ConstUnit.Force + " (OK)");
								else
									Reporting.AddLine("= " + limit + " <= " + flangeForce + ConstUnit.Force + " (NG)");

								if (directWeld.MomentType == EDirectWeldMomentType.Gravity)
								{
									// Equation K1-6
									Reporting.AddHeader("Check Limit State of Sidewall Load Buckling");
									limit = ConstNum.FIOMEGA0_9N * (48 * Math.Pow(column.Shape.tf, 3) / (column.Shape.H - 3 * column.Shape.tf)) * Math.Sqrt(ConstNum.ELASTICITY * column.Material.Fy) * qf;
									Reporting.AddLine(ConstString.FIOMEGA0_9 + "(48 * t³ / (H - 3 * t)) * (E * Fy)^0.5 * Qf");
									Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * (48 * " + Math.Pow(column.Shape.tf, 3) + " / (" + column.Shape.H + " - 3 * " + column.Shape.tf + ")) * (" + ConstNum.ELASTICITY + " * " + column.Material.Fy + ")^0.5 * " + qf + " = " + limit);

									if (limit > flangeForce)
										Reporting.AddLine("= " + limit + " >> " + flangeForce + ConstUnit.Force + " (OK)");
									else
										Reporting.AddLine("= " + limit + " <= " + flangeForce + ConstUnit.Force + " (NG)");
								}
							}
						}
					}
					break;
			}
		}
	}
}