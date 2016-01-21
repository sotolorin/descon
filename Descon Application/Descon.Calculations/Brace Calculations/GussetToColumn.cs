using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class GussetToColumn
	{
		public static void CalcGussetToColumn(EMemberType memberType)
		{
			double usefulweldsize = 0;
			double SupThickness = 0;
			double w1 = 0;
			double minweld = 0;
			double wmaxlimit = 0;
			double RichardFactor = 0;
			double M2 = 0;
			double M1 = 0;
			double eTop = 0;
			double yTop = 0;
			double ortasi = 0;
			double V2 = 0;
			double V1 = 0;
			double H2 = 0;
			double H1 = 0;
			double fs = 0;
			double ecc = 0;
			double Lmin = 0;
			double R = 0;
			double w = 0;
			double fraverage = 0;
			double fr = 0;
			double Fb = 0;
			double ec = 0;
			double Yc = 0;
			double xc = 0;
			double Moment = 0;
			double fvv = 0;
			double fh = 0;
			double L = 0;
			double H = 0;
			double wmin = 0;
			double b = 0;
			int NforTV = 0;
			double cap = 0;
			int N = 0;
			double V = 0;
			double t_Comp = 0;
			double t_Ten = 0;
			double d2 = 0;
			double SupportThickness = 0;
			double H_Comp = 0;
			double H_Tens = 0;

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
				return;

			var brace = CommonDataStatic.DetailDataDict[memberType];
			var gusset = brace.BraceConnect.Gusset; // Alias to shrink the code

			DetailData beam;
			if (memberType == EMemberType.LowerLeft || memberType == EMemberType.UpperLeft)
				beam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			else
				beam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			H_Tens = gusset.GussetEFTension.Hc;
			H_Comp = gusset.GussetEFCompression.Hc;

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumn && (brace.KBrace | brace.KneeBrace))
			{
				brace.GussetToColumnConnection = EBraceConnectionTypes.DirectlyWelded;
				brace.GussetWeldedToColumn = true;
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumn && brace.KBrace)
			{
				CommonDataStatic.DetailDataDict[memberType].GussetToColumnConnection = EBraceConnectionTypes.DirectlyWelded;
				CommonDataStatic.DetailDataDict[memberType].GussetWeldedToColumn = true;
			}
			else
				CommonDataStatic.DetailDataDict[memberType].GussetWeldedToColumn = false;

			Reporting.AddHeader("Gusset to Column Connection");
			if (brace.KBrace && memberType == EMemberType.LowerRight)
				Reporting.AddHeader("Right Hand Side Gusset to Column Connection");
			else if (brace.KBrace && memberType == EMemberType.LowerLeft)
				Reporting.AddHeader("Left Hand Side Gusset to Column Connection");
			else
				Reporting.AddHeader(MiscMethods.GetComponentName(memberType) + " Gusset to Column Connection");

			if (column.WebOrientation == EWebOrientation.InPlane)
				SupportThickness = column.Shape.tf;
			else
			{
				if (column.ShapeType == EShapeType.WideFlange)
					SupportThickness = column.Shape.tw;
				else
					SupportThickness = column.Shape.tf;
			}

			switch (brace.GussetToColumnConnection)
			{
				case EBraceConnectionTypes.ClipAngle:
					if (beam.IsActive && !brace.GussetWeldedToColumn)
					{
                        //if (!brace.BraceConnect.Gusset.ColumnSideSetback_User)
                        //    brace.BraceConnect.Gusset.ColumnSideSetback = ConstNum.HALF_INCH;

						ClipAnglesBrace.ClipAngleForces(memberType);

						t_Ten = brace.AxialTension / 2;
						t_Comp = brace.AxialCompression / 2;
						V = Math.Abs(brace.WinConnect.ShearClipAngle.ForceY / 2);

						Reporting.AddLine("Vertical Force on Each Clip Angle, V = " + V + ConstUnit.Force);
						Reporting.AddLine("Horizontal Tension on Each Clip Angle, Ft = " + t_Ten + ConstUnit.Force);
						Reporting.AddLine("Horizontal Compression on Each Clip Angle, Fc = " + t_Comp + ConstUnit.Force);
					}

					N = -brace.BoltBrace.NumberOfBolts;
					Reporting.AddHeader("Angle-to-Column Bolts:");
					Reporting.AddLine(" (" + brace.BoltBrace.NumberOfBolts + ") " + brace.WinConnect.ShearClipAngle.BoltOnColumn.BoltName + " Bolts");
					Reporting.AddHeader("Shear Strength of Bolts:");

					if (brace.BoltBrace.BoltType == EBoltType.SC &&
					    !(CommonDataStatic.Preferences.Seismic == ESeismic.Seismic &&
					      CommonDataStatic.SeismicSettings.Response == EResponse.RGreaterThan3 &&
					      brace.BoltBrace.HoleType == EBoltHoleType.STD))
					{
						cap = 2 * brace.BoltBrace.NumberOfBolts * brace.BoltBrace.BoltStrength * (1 - t_Ten / (1.13F * brace.BoltBrace.Pretension * brace.BoltBrace.NumberOfBolts));

						Reporting.AddLine("= 2 * n * (" + "FiRn) * (1 - Tu / (1.13 * Tm * n))");
						Reporting.AddLine("= 2 * " + brace.BoltBrace.NumberOfBolts + " * " + brace.BoltBrace.BoltStrength + " *  (1 - " + t_Ten + " / (1.13 * " + brace.BoltBrace.Pretension + " * " + brace.BoltBrace.NumberOfBolts + "))");

						if (cap >= 2 * V)
							Reporting.AddLine("= " + cap + " >= " + 2 * V + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + cap + " << " + 2 * V + ConstUnit.Force + " (NG)");
					}
					else
					{
						cap = 2 * brace.BoltBrace.NumberOfBolts * brace.BoltBrace.BoltStrength;

						if (cap >= 2 * V)
							Reporting.AddLine("= 2 * n * (" + "FiRn) = 2 * " + brace.BoltBrace.NumberOfBolts + " * " + brace.BoltBrace.BoltStrength + " = " + cap + " >= " + 2 * V + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= 2 * n * (" + "FiRn) = 2 * " + brace.BoltBrace.NumberOfBolts + " * " + brace.BoltBrace.BoltStrength + " = " + cap + " << " + 2 * V + ConstUnit.Force + " (NG)");
					}
					NforTV = (int) BoltsForTension.CalcBoltsForTension(memberType, brace.BoltBrace, t_Ten, V, N, true);
					brace.BoltBrace.NumberOfBolts = NforTV;
					b = BoltsForTension.CalcBoltsForTension(memberType, brace.BoltBrace, t_Ten, V, NforTV, true);
					ClipAnglesBrace.DesignClipAngles(memberType, NforTV, b);
					break;
				case EBraceConnectionTypes.DirectlyWelded:
					gusset.Length = 0;
					gusset.Thickness = 0;
					gusset.ColumnSideSetback = 0;
					brace.GussetWeldedToColumn = true;
					gusset.ColumnSideSetback = 0;
					wmin = Math.Max(0.125, CommonCalculations.MinimumWeld(gusset.Thickness, SupportThickness));
					H = gusset.Hc;
					V = gusset.VerticalForceColumn;

					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase)
					{
						if (!gusset.DontConnectBeam)
							L = gusset.VerticalForceColumn - CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].BraceConnect.BasePlate.CornerClip; // L = BRACE1.Gusset[m].VColumn - BRACE1.BasePlate.clip;
						else
							L = gusset.VerticalForceColumn;
					}
					else
						L = gusset.VerticalForceColumn - CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].BraceConnect.BasePlate.CornerClip; // L = BRACE1.Gusset[m].VColumn - BRACE1.BasePlate.clip;
					fh = Math.Abs(H / L);
					fvv = Math.Abs(V / L);

					if (!gusset.DontConnectBeam)
						Moment = gusset.Mc;
					else
					{
						SmallMethodsDesign.Intersect(Math.Tan(brace.Angle * ConstNum.RADIAN), brace.WorkPointX, brace.WorkPointY, 100, d2, 0, ref xc, ref Yc);

						//TODO: There's not currently a replacement for the massive array BRACE1.y[]
						//ec = Math.Abs(BRACE1.y[25 + 5 * (m - 3)] + Math.Sign(25 + 5 * (m - 3)) * (brace.BraceConnect.BasePlate.CornerClip + L / 2) - Yc);
						Moment = H * ec;

						//SmallMethodsBrace.Intersect(Math.Tan(BRACE1.member[m].Angle * BRACE1.radian), BRACE1.member[m].WorkX, BRACE1.member[m].WorkY, 100, d2, 0, ref xc, ref Yc);
						//ec = Math.Abs(BRACE1.y[25 + 5 * (m - 3)] + Math.Sign(25 + 5 * (m - 3)) * (BRACE1.BasePlate.clip + L / 2) - Yc);
						//Moment = H * ec;
					}
					Fb = Math.Abs(6 * Moment / (L * L));
					fr = Math.Sqrt(Math.Pow(fvv, 2) + Math.Pow(fh + Fb, 2));
					fraverage = Math.Sqrt(Math.Pow(fvv, 2) + Math.Pow(fh + Fb / 2, 2));
					w = Math.Max(1.25 * fraverage, fr) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx); // w = Math.Max(1.25 * fraverage, fr) / (ConstString.FIOMEGA0_75N * 0.6 * 1.414 * BRACE1.Fexx);
					if (w < wmin)
						w = wmin;

					if (brace.KneeBrace)
					{
						V = gusset.VerticalForceColumn;
						H = gusset.Hc;

						R = Math.Sqrt(Math.Pow(V, 2) + Math.Pow(H, 2));
						if (gusset.ColumnWeldSize > 0.0)
						{
							Lmin = Math.Min(R / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * gusset.ColumnWeldSize),
								(brace.Shape.d - 2 * gusset.ColumnSideSetback * 0.57735) /
								Math.Cos(brace.Angle * ConstNum.RADIAN));
						}
						else
						{
							Lmin = Math.Min(R / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * wmin),
								(brace.Shape.d - 2 * gusset.ColumnSideSetback * 0.57735) / Math.Cos(brace.Angle * ConstNum.RADIAN));
						}
						L = Math.Max(Lmin, gusset.VerticalForceColumn);

						//TODO: Not currently a replacement for the array BRACE1.x[] of 701 floats. Yes, 701.
						//i1 = 5 * (m - 3);
						//SmallMethodsBrace.Intersect(100, BRACE1.x[25 + i1], BRACE1.y[25 + i1], Math.Tan(brace.Angle * ConstNum.RADIAN), brace.WorkPointX, brace.WorkPointY, ref x0, ref y0);
						//ecc = Math.Abs((BRACE1.y[25 + i1] + BRACE1.y[29 + i1]) / 2 - y0);

						gusset.Mc = gusset.Hc;// * ecc;
						if (L != 0)
						{
							fs = gusset.VerticalForceColumn / L;
							fh = gusset.Hc / L;
						}

						if (L > 0)
							Fb = 6 * gusset.Mc / Math.Pow(L, 2);

						fr = Math.Sqrt(Math.Pow(fs, 2) + Math.Pow(fh + Fb, 2));
						fraverage = Math.Sqrt(Math.Pow(fs, 2) + Math.Pow(fh + Fb / 2, 2));
						w = Math.Max(fr, 1.25 * fraverage) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						if (w < wmin)
							w = wmin;
						if (!gusset.ColumnSide_User)
							gusset.ColumnWeldSize = NumberFun.Round(w, 16);
					}
					if (brace.KBrace && memberType == EMemberType.LowerLeft || memberType == EMemberType.LowerRight)
					{
						DetailData upperBrace = CommonDataStatic.DetailDataDict[memberType];

						H1 = gusset.GussetEFCompression.Hc + upperBrace.BraceConnect.Gusset.GussetEFTension.Hc;
						H2 = upperBrace.BraceConnect.Gusset.GussetEFCompression.Hc + gusset.GussetEFTension.Hc;
						H = Math.Max(H1, H2);
						V1 = gusset.GussetEFCompression.Vc + upperBrace.BraceConnect.Gusset.GussetEFTension.Vc;
						V2 = upperBrace.BraceConnect.Gusset.GussetEFCompression.Vc + gusset.GussetEFTension.Vc;
						V = Math.Max(V1, V2);

						//i1 = 5 * (m - 4);
						//ortasi = (BRACE1.y[29 + i1] + BRACE1.y[34 + i1]) / 2;
						//L = BRACE1.y[29 + i1] - BRACE1.y[34 + i1];
						//SmallMethodsBrace.Intersect(100, BRACE1.x[25 + i1].ToDbl(), BRACE1.y[25 + i1].ToDbl(), Math.Tan(BRACE1.member[m - 1].Angle * BRACE1.radian), BRACE1.member[m - 1].WorkX.ToDbl(), BRACE1.member[m - 1].WorkY.ToDbl(), ref dumy, ref yTop);
						//eTop = yTop - ortasi;
						//SmallMethodsBrace.Intersect(100, BRACE1.x[25 + i1].ToDbl(), BRACE1.y[25 + i1].ToDbl(), Math.Tan(BRACE1.member[m].Angle * BRACE1.radian), BRACE1.member[m].WorkX.ToDbl(), BRACE1.member[m].WorkY.ToDbl(), ref dumy, ref yBottom);
						//eBottom = ortasi - yBottom;

						M1 = Math.Abs(/*eTop * */ upperBrace.BraceConnect.Gusset.GussetEFTension.Hc * gusset.GussetEFCompression.Hc);
						M2 = Math.Abs(/*eTop * */ upperBrace.BraceConnect.Gusset.GussetEFCompression.Hc * gusset.GussetEFTension.Hc);
						Moment = Math.Max(M1, M2);

						fh = H / L;
						fvv = V / L;
						Fb = 6 * Moment / (L * L);
						fr = Math.Sqrt(Math.Pow(fvv, 2) + Math.Pow(fh + Fb, 2));
						fraverage = Math.Sqrt(Math.Pow(fvv, 2) + Math.Pow(fh + Fb / 2, 2));
						w = Math.Max(fr, 1.25 * fraverage) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						if (w < wmin)
							w = wmin;

						if (!gusset.ColumnSide_User)
							gusset.ColumnWeldSize = NumberFun.Round(w, 16);
						if (!upperBrace.BraceConnect.Gusset.ColumnSide_User)
							upperBrace.BraceConnect.Gusset.ColumnWeldSize = gusset.ColumnWeldSize;
					}
					if (brace.KneeBrace)
					{
						H = gusset.Hc;
						V = gusset.VerticalForceColumn;
						Moment = H;// * ecc;
						R = Math.Sqrt(Math.Pow(V, 2) + Math.Pow(H, 2));
						L = gusset.VerticalForceColumn;
						Fb = 6 * Moment / (L * L);
						fr = Math.Sqrt(Math.Pow(H / L + Fb, 2) + Math.Pow(V / L, 2));
						fraverage = Math.Sqrt(Math.Pow(H / L + Fb / 2, 2) + Math.Pow(V / L, 2));
						w = Math.Max(fr, 1.25 * fraverage) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						if (w < wmin)
							w = wmin;

						if (!gusset.ColumnSide_User)
							gusset.ColumnWeldSize = NumberFun.Round(w, 16);
					}
					else if (brace.KneeBrace)
					{
						if (memberType == EMemberType.UpperLeft || memberType == EMemberType.UpperRight)
							Reporting.AddLine("See lower brace calculation.");
						else
						{
							DetailData upperBrace = CommonDataStatic.DetailDataDict[memberType];

							H1 = gusset.GussetEFCompression.Hc + upperBrace.BraceConnect.Gusset.GussetEFTension.Hc;
							H2 = upperBrace.BraceConnect.Gusset.GussetEFCompression.Hc + gusset.GussetEFTension.Hc;
							H = Math.Max(H1, H2);
							V1 = gusset.GussetEFCompression.Vc + upperBrace.BraceConnect.Gusset.GussetEFTension.Vc;
							V2 = upperBrace.BraceConnect.Gusset.GussetEFCompression.Vc + gusset.GussetEFTension.Vc;
							V = Math.Max(V1, V2);

							M1 = Math.Abs(/*eTop * */ upperBrace.BraceConnect.Gusset.GussetEFTension.Hc * gusset.GussetEFCompression.Hc);
							M2 = Math.Abs(/*eTop * */ upperBrace.BraceConnect.Gusset.GussetEFCompression.Hc * gusset.GussetEFTension.Hc);
							Moment = Math.Max(M1, M2);

							fh = H / L;
							fvv = V / L;
							Fb = 6 * Moment / (L * L);
							fr = Math.Sqrt(Math.Pow(fvv, 2) + Math.Pow(fh + Fb, 2));
							fraverage = Math.Sqrt(Math.Pow(fvv, 2) + Math.Pow(fh + Fb / 2, 2));
							w = Math.Max(fr, 1.25 * fraverage) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
							if (w < wmin)
								w = wmin;

							if (!gusset.ColumnSide_User)
								gusset.ColumnWeldSize = NumberFun.Round(w, 16);
						}
					}

					Reporting.AddHeader("Weld Size = " + gusset.ColumnWeldSize + ConstUnit.Length);
					Reporting.AddLine("Weld Length on Each Side of Gusset Plate, L = " + L + ConstUnit.Length);
					Reporting.AddLine("Horizontal Force on Welds, H = " + H + ConstUnit.Force);
					Reporting.AddLine("Vertical Force on Welds, V = " + V + ConstUnit.Force);

					if ((brace.KBrace | brace.KneeBrace) == false)
					{
						if (!gusset.DontConnectBeam)
						{
							Moment = gusset.Mc;
							Reporting.AddLine("Moment on Welds (M) = " + Moment + ConstUnit.Moment);
						}
						else
						{
							//SmallMethodsBrace.Intersect(Math.Tan(brace.Angle * ConstNum.RADIAN), brace.WorkPointX, brace.WorkPointY, 100, d2, 0, ref xc, ref Yc);
							//ec = Math.Abs(BRACE1.y[25 + 5 * (m - 3)] + Math.Sign(25 + 5 * (m - 3)) * (brace.BraceConnect.BasePlate.CornerClip + L / 2) - Yc);
							Moment = H * ec;
							Reporting.AddLine("Moment on Welds, M = H * e = " + H + " * " + ec + " = " + Moment + ConstUnit.Moment);
						}

						Reporting.AddHeader("Max. Force on Welds per Unit Length = f ");
						Reporting.AddLine("= ((H / L + 6 * M / L²)² + (V / L)²)^0.5");
						Reporting.AddLine("= ((" + H + " / " + L + " + 6 *" + Moment + " / " + L + "²)² + (" + V + " / " + L + ")²)^0.5");
						Reporting.AddLine("= " + fr + ConstUnit.ForceUniform);

						Reporting.AddHeader("Average Force on Welds per Unit Length = fraverage");
						Reporting.AddLine("= ((H / L + 3 * M / L²)² + (V / L)²)^0.5");
						Reporting.AddLine("= ((" + H + " / " + L + " + 3 * " + Moment + " / " + L + "²)² + (" + V + " / " + L + ")²)^0.5");
						Reporting.AddLine("= " + fraverage + ConstUnit.ForceUniform);
					}
					else if (brace.KBrace)
					{
						Reporting.AddHeader("Moment on Welds (M) =| eTop * Htop - eBot * Hbot |");
						Reporting.AddLine("= |" + eTop + " * " + gusset.Hc + " * " + gusset.Hc + "|");
						Reporting.AddLine("= " + Moment + ConstUnit.Moment);

						Reporting.AddHeader("Max. Force on Welds per Unit Length = f ");
						Reporting.AddLine("= ((H / L + 6 * M / L²)² + (V / L)²)^0.5");
						Reporting.AddLine("= ((" + H + " / " + L + " + 6 *" + Moment + " / " + L + "²)² + (" + V + " / " + L + ")²)^0.5");
						Reporting.AddLine("= " + fr + ConstUnit.ForceUniform);

						Reporting.AddHeader("Average Force on Welds per Unit Length = fraverage");
						Reporting.AddLine("= ((H / L + 3 * M / L²)² + (V / L)²)^0.5");
						Reporting.AddLine("= ((" + H + " / " + L + " + 3 * " + Moment + " / " + L + "²)² + (" + V + " / " + L + ")²)^0.5");
						Reporting.AddLine("= " + fraverage + ConstUnit.ForceUniform);
					}
					else if (brace.KneeBrace)
					{
						//ecc = Math.Abs((BRACE1.y[25 + i1] + BRACE1.y[29 + i1]) / 2 - y0);
						H = gusset.Hc;
						V = gusset.VerticalForceColumn;
						Moment = H;// * ecc;
						R = Math.Sqrt(Math.Pow(V, 2) + Math.Pow(H, 2));
						L = gusset.VerticalForceColumn;
						Fb = 6 * Moment / (L * L);
						fr = Math.Sqrt(Math.Pow(H / L + Fb, 2) + Math.Pow(V / L, 2));
						fraverage = Math.Sqrt(Math.Pow(H / L + Fb / 2, 2) + Math.Pow(V / L, 2));
						if (double.IsNaN(fr))
							fr = 0;
						if (double.IsNaN(fraverage))
							fraverage = 0;

						Reporting.AddHeader("Moment on Welds (M) = h * ec = " + H + " * " + ecc + " = " + Moment + ConstUnit.Moment);
						Reporting.AddLine("Max. Force on Welds per Unit Length = f ");
						Reporting.AddLine("= ((H / L + 6 * M / L²)² + (V / L)²)^0.5");
						Reporting.AddLine("= ((" + H + " / " + L + " + 6 * " + Moment + " / " + L + "²)² + (" + V + " / " + L + ")²)^0.5");
						Reporting.AddLine("= " + fr + ConstUnit.ForceUniform);

						Reporting.AddHeader("Average Force on Welds per Unit Length = fraverage");
						Reporting.AddLine("= ((H / L + 3 * M / L²)² + (V / L)²)^0.5");
						Reporting.AddLine("= ((" + H + " / " + L + " + 3 * " + Moment + " / " + L + "²)² + (" + V + " / " + L + ")²)^0.5");
						Reporting.AddLine("= " + fraverage + ConstUnit.ForceUniform);
					}

					RichardFactor = 1.25;
					if (column.WebOrientation == EWebOrientation.InPlane)
					{
						wmaxlimit = 0.707 * Math.Min(gusset.Material.Fu * gusset.Thickness,
							2 * column.Material.Fu * column.Shape.tf) / CommonDataStatic.Preferences.DefaultElectrode.Fexx;

						Reporting.AddHeader("Maximum useful weld size = 0.707 * Min(Fug * tg, 2 * Fuc * tf) / Fexx");
						Reporting.AddLine("= 0.707 * Min(" + gusset.Material.Fu + " * " + gusset.Thickness + ", 2 * " + brace.Material.Fu + " * " + brace.Shape.tf + ") / " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						Reporting.AddLine("= " + wmaxlimit + ConstUnit.Length);
					}
					else
					{
						wmaxlimit = 0.707 * Math.Min(gusset.Material.Fu * gusset.Thickness, 2 * gusset.Material.Fu * brace.Shape.tw) / CommonDataStatic.Preferences.DefaultElectrode.Fexx;
						Reporting.AddHeader("Maximum useful weld size = 0.707 * Min(Fug * tg, 2 * Fuc * tw) / Fexx");
						Reporting.AddLine("= 0.707 * Min(" + gusset.Material.Fu + " * " + gusset.Thickness + ", 2 *" + brace.Material.Fu + " * " + brace.Shape.tw + ") / " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						Reporting.AddLine("= " + wmaxlimit + ConstUnit.Length);
						Reporting.AddHeader("Check the effect of the member connected to the other side of the column web, if there is one.");
					}
					minweld = CommonCalculations.MinimumWeld(SupportThickness, gusset.Thickness);
					w1 = Math.Max(fr, RichardFactor * fraverage) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					w = w1 < minweld ? minweld : w1;
					
					if (gusset.ColumnWeldSize == 0)
					{
						if (!gusset.ColumnSide_User)
							gusset.ColumnWeldSize = w;
						Reporting.AddHeader("Use Richard Factor (Rf)= 1.25");
						Reporting.AddLine("Required Weld Size (w) = Max(fr, Rf * fraverage) / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * Fexx) ");
						Reporting.AddLine("= Max(" + fr + ", 1.25 * " + fraverage + ") / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
						Reporting.AddLine("= " + w1 + ConstUnit.Length);

						if (w1 > wmaxlimit)
							Reporting.AddLine("Required Weld size exceeds maximum useful weld size. (NG)");
						else
						{
							Reporting.AddHeader("Minimum Weld Size = " + CommonCalculations.WeldSize(minweld) + ConstUnit.Length);
							Reporting.AddLine("Use " + CommonCalculations.WeldSize(w) + ConstUnit.Length + " Weld");
						}
					}
					else
					{
						Reporting.AddHeader("Use Richard Factor (Rf) = 1.25");
						Reporting.AddLine("Required Weld Size (w) = Max(fr, Rf * fraverage) / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.41 * Fexx) ");
						Reporting.AddLine("= Max(" + fr + ", 1.25 * " + fraverage + ") / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.41 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ") ");
						if (w1 <= gusset.ColumnWeldSize)
							Reporting.AddLine("= " + w1 + " <= " + gusset.ColumnWeldSize + ConstUnit.Length + " (OK)");
						else
							Reporting.AddLine("= " + w1 + " >> " + gusset.ColumnWeldSize + ConstUnit.Length + " (NG)");
					}

					Reporting.AddHeader("Try " + CommonCalculations.WeldSize(gusset.ColumnWeldSize) + ConstUnit.Length + " weld");

					minweld = CommonCalculations.MinimumWeld(SupportThickness, gusset.Thickness);
					if (minweld <= gusset.ColumnWeldSize)
						Reporting.AddLine(" Minimum Weld size = " + minweld + " <= " + gusset.ColumnWeldSize + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine(" Minimum Weld size = " + minweld + " >> " + gusset.ColumnWeldSize + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Effective sup. thick.:");
					if (column.ShapeType == EShapeType.WideFlange && column.WebOrientation == EWebOrientation.OutOfPlane)
					{
						DetailData otherSide = null;

						switch (memberType)
						{
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
						}
						if (otherSide != null && otherSide.ShapeName != ConstString.NONE)
						{
							if (otherSide.GussetToColumnConnection == EBraceConnectionTypes.DirectlyWelded && (gusset.ColumnWeldSize + otherSide.BraceConnect.Gusset.ColumnWeldSize) > 0)
							{
								SupThickness = column.Shape.tw * gusset.ColumnWeldSize / (gusset.ColumnWeldSize + otherSide.BraceConnect.Gusset.ColumnWeldSize);
								Reporting.AddLine("tse = tw * w / (w + w_otherside)");
								Reporting.AddLine("= " + brace.Shape.tw + " * " + gusset.ColumnWeldSize + " / (" + gusset.ColumnWeldSize + "  + " + otherSide.BraceConnect.Gusset.ColumnWeldSize + " )");
								Reporting.AddLine("= " + SupThickness + ConstUnit.Length);
							}
							else if (otherSide.GussetToColumnConnection == EBraceConnectionTypes.SinglePlate && gusset.ColumnWeldSize + otherSide.WinConnect.ShearWebPlate.SupportWeldSize > 0)
							{
								SupThickness = column.Shape.tw * gusset.ColumnWeldSize / (gusset.ColumnWeldSize + otherSide.WinConnect.ShearWebPlate.SupportWeldSize);
								Reporting.AddLine("tse = tw * w / (w + w_otherside)");
								Reporting.AddLine("= " + brace.Shape.tw + " * " + gusset.ColumnWeldSize + " / (" + gusset.ColumnWeldSize + "  + " + otherSide.WinConnect.ShearWebPlate.SupportWeldSize + " )");
								Reporting.AddLine("= " + SupThickness + ConstUnit.Length);
							}
							else
							{
								SupThickness = column.Shape.tw;
								Reporting.AddLine("tse = tw = " + brace.Shape.tw + ConstUnit.Length);
							}
						}
						else
						{
							SupThickness = column.Shape.tw;
							Reporting.AddLine("tse = tw = " + brace.Shape.tw + ConstUnit.Length);
						}
						usefulweldsize = 0.4 * brace.Material.Fy * SupThickness / (0.3 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707);
					}
					else
					{
						SupThickness = column.Shape.tf;
						Reporting.AddLine("tse = tf = " + brace.Shape.tf + ConstUnit.Length);
						usefulweldsize = 0.4 * brace.Material.Fy * brace.Shape.tf / (0.3 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707);
					}

					Reporting.AddHeader("Useful weld size:");
					usefulweldsize = Math.Min(ConstNum.FIOMEGA0_75N * 0.6 * gusset.Thickness * gusset.Material.Fu, 2 * ConstNum.FIOMEGA0_75N * 0.6 * SupThickness * column.Material.Fu) / (2 * 0.707 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddLine("wu = Min(" + ConstString.FIOMEGA0_75 + " * 0.6 * tg * Fup, 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * tse * Fuc) / (2 * 0.707 * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fexx)");
					Reporting.AddLine("= Min(" + ConstString.FIOMEGA0_75 + " * 0.6 * " + gusset.Thickness + " * " + gusset.Material.Fu + ", 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + SupThickness + " * " + brace.Material.Fu + ") / (2 * 0.707 * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
					if (usefulweldsize >= w1)
						Reporting.AddLine("= " + usefulweldsize + " >=  w_required = " + w1 + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + usefulweldsize + " <<  w_required = " + w1 + ConstUnit.Length + " (NG)");
					if (w1 <= usefulweldsize)
					{
						Reporting.AddLine("Plate and column develop the required weld capacity. (OK)");
						if (column.WebOrientation == EWebOrientation.OutOfPlane && column.ShapeType == EShapeType.WideFlange)
							Reporting.AddHeader("Check the effect of the member connected to the other side of the column web, if there is one.");
					}
					else
						Reporting.AddLine("Plate or column does not develop the requird weld capacity. (NG)");
					break;
				case EBraceConnectionTypes.SinglePlate:
					Reporting.AddHeader("With Tensile Brace Force:");
					Reporting.AddLine("Vertical Force on Connection Plate (V) = " + gusset.GussetEFTension.Vc + ConstUnit.Force);
					Reporting.AddLine("Horizontal Force on Connection Plate (H) = " + gusset.GussetEFTension.Hc + ConstUnit.Force);
					Reporting.AddLine("Resultant Force on Connection Plate (R) = " + Math.Pow(Math.Pow(gusset.GussetEFTension.Vc, 2) + Math.Pow(gusset.GussetEFTension.Hc, 2), 0.5) + ConstUnit.Force);
					Reporting.AddLine("Moment on Connection Plate (M) = " + gusset.GussetEFTension.Mc + ConstUnit.Moment);
					Reporting.AddLine("With Compressive Brace Force:");
					Reporting.AddLine("Vertical Force on Connection Plate (V) = " + gusset.GussetEFCompression.Vc + ConstUnit.Force);
					Reporting.AddLine("Horizontal Force on Connection Plate (H) = " + gusset.GussetEFCompression.Hc + ConstUnit.Force);
					Reporting.AddLine("Resultant Force on Connection Plate (R) = " + Math.Pow(Math.Pow(gusset.GussetEFCompression.Vc, 2) + Math.Pow(gusset.GussetEFCompression.Hc, 2), 0.5) + ConstUnit.Force);
					Reporting.AddLine("Moment on Connection Plate (M) = " + gusset.GussetEFCompression.Mc + ConstUnit.Moment);

					DesignSinglePlate.CalcDesignSinglePlate(memberType, Math.Abs(gusset.GussetEFTension.Hc),
						Math.Abs(gusset.GussetEFCompression.Hc),
						Math.Abs(Math.Max(gusset.GussetEFCompression.Vc,
							gusset.GussetEFTension.Vc)),
						Math.Abs(Math.Max(gusset.GussetEFCompression.Mc,
							gusset.GussetEFTension.Mc)));
					break;
				case EBraceConnectionTypes.FabricatedTee:
					Reporting.AddLine("Vertical Force on Tee (V) = " + gusset.VerticalForceColumn + ConstUnit.Force);
					Reporting.AddLine("Horizontal Force on Tee (T) = " + gusset.Hc + ConstUnit.Force);
					Reporting.AddLine("Moment on Tee (M) = " + gusset.Mc + ConstUnit.Moment);
					DesignFabricatedTee.CalcDesignFabricatedTee(memberType, Math.Abs(H_Tens), Math.Abs(H_Comp), Math.Abs(gusset.VerticalForceColumn), ((int)Math.Abs(gusset.Mc)));
					break;
				case EBraceConnectionTypes.EndPlate:
					ClipAnglesBrace.ClipAngleForces(memberType);
					DesignEndPlate.CalcDesignEndPlate(memberType, Math.Abs(H_Tens), Math.Abs(H_Comp), ((int)Math.Abs(gusset.VerticalForceColumn)), ((int)Math.Abs(gusset.Mc)));
					break;
			}

			SmallMethodsDesign.ColumnAndBeamCheck(memberType, brace.BoltBrace);
		}
	}
}