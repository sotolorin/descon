using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class ColumnSplice
	{
		internal static double FlangeTensionforSeismic;
		internal static double FfToDevMoment;
		private static double w_y;

		public static void DesignColumnSplice()
		 {
			double tw = 0;
			double capacity = 0;
			double FfillerUF = 0;
			double FfillerC = 0;
			double FspliceC = 0;
			double usefulweldonPlate = 0;
			double FspliceT = 0;
			double reduction = 0;
			double FfillerLF = 0;
			double Fsplice = 0;
			double Gage = 0;
			double PLType = 0;
			double PLLength = 0;
			double CSMinY = 0;
			double CSMinX = 0;
			bool FullContact = false;
			double tf = 0;
			double Fbr_F = 0;
			double d = 0;
			double Fbr_C = 0;
			double Fbr_Filler = 0;
			double Fbr_Column = 0;
			double S_ContactwithFiller = 0;
			double AcontactwithFiller = 0;
			double S_Contact = 0;
			double dmax = 0;
			double Fw_Ten = 0;
			double dw = 0;
			double Fw_Comp = 0;
			double maxweldsize;
			double Ff_Ten = 0;
			double BF = 0;
			double Ff_Comp = 0;
			double Acontact = 0;
			bool shart2;
			bool shart1;
			double weldForceLt = 0;
			double weldForceLc = 0;
			double weldForceL = 0;
			double weldForceUt = 0;
			double weldForceUc = 0;
			double weldForceU = 0;
			double FWeldsize = 0;
			double MnTop;
			double MnBot;
			double Fw = 0;
			double minweldsize;
			double fyc;
			double FWeldSizec;
			double FWeldSizet;

			bool Type1 = false;

			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			colSplice.SpliceThicknessFlange = 0;
			colSplice.SpliceThicknessWeb = 0;

			if (colSplice.UseSeismic)
			{
				FlangeTensionforSeismic = 0.5 * Math.Min(MiscCalculationDataMethods.ExpYieldStr(bColumn.Material.Fy) * bColumn.Material.Fy * bColumn.Shape.tf * bColumn.Shape.bf, MiscCalculationDataMethods.ExpYieldStr(tColumn.Material.Fy) * tColumn.Material.Fy * tColumn.Shape.tf * tColumn.Shape.bf);
				if (colSplice.SMF)
				{
					MnTop = Math.Min(tColumn.Material.Fy * tColumn.Shape.zx, ConstNum.ONEANDHALF_INCHES * tColumn.Material.Fy * tColumn.Shape.sx);
					MnBot = Math.Min(bColumn.Material.Fy * bColumn.Shape.zx, ConstNum.ONEANDHALF_INCHES * bColumn.Material.Fy * bColumn.Shape.sx);
					FfToDevMoment = Math.Min(MnTop / (tColumn.Shape.d - tColumn.Shape.tf), MnBot / (bColumn.Shape.d - bColumn.Shape.tf));
				}
				else
					FfToDevMoment = 0;
			}

			if (colSplice.DesignWebSpliceFor == EDesignWebSpliceFor.Vs)
				colSplice.WebShear = colSplice.Shear;
			else
				colSplice.WebShear = Math.Max(0, colSplice.Shear - 0.33 * colSplice.Cmin);

			if (colSplice.UseSeismic && colSplice.SMF)
				colSplice.WebShear = colSplice.ShearWithSMF;

			FillerThickness();

			ContactSurface(ref BF, ref d, ref dmax, ref tf, ref Acontact, ref S_Contact, ref AcontactwithFiller, ref S_ContactwithFiller, ref FullContact);
			FlangeSpliceForces(Acontact, ref Fbr_Column, AcontactwithFiller, ref Fbr_Filler, ref Fbr_C, ref Fbr_F, ref FfillerC, ref FspliceC, ref FspliceT, dmax, S_Contact, S_ContactwithFiller, ref Fsplice, ref FfillerLF, ref FfillerUF);

			switch (colSplice.ConnectionOption)
			{
				case ESpliceConnection.FlangePlate:
					FlangePlateDesign(ref Type1, ref CSMinX, ref CSMinY, ref PLLength, ref PLType, ref Gage, FullContact, AcontactwithFiller, Acontact, Fsplice, FfillerLF, ref reduction, FspliceT, ref FWeldsize, ref usefulweldonPlate, FspliceC, FfillerC, FfillerUF, ref capacity);
					break;
				case ESpliceConnection.FlangeAndWebPlate:
					FlangePlateDesign(ref Type1, ref CSMinX, ref CSMinY, ref PLLength, ref PLType, ref Gage, FullContact, AcontactwithFiller, Acontact, Fsplice, FfillerLF, ref reduction, FspliceT, ref FWeldsize, ref usefulweldonPlate, FspliceC, FfillerC, FfillerUF, ref capacity);
					ColumnSpliceWebPlate.WebPlateDesign(ref dw, ref d, ref tw, Fw, ref reduction, ref capacity, ref CSMinX, ref CSMinY, ref PLLength, ref PLType, ref Gage, ref usefulweldonPlate);
					colSplice.WebPLLength = colSplice.SpliceLengthUpperWeb + colSplice.SpliceLengthLowerWeb;
					break;
				case ESpliceConnection.ButtPlate:
					if (tColumn.Shape.d <= 9 * ConstNum.ONE_INCH && bColumn.Shape.d <= 11.4 * ConstNum.ONE_INCH || bColumn.Shape.d <= 9 * ConstNum.ONE_INCH && tColumn.Shape.d <= 11.4 * ConstNum.ONE_INCH)
						colSplice.ButtThickness = ConstNum.ONEANDHALF_INCHES;
					else
						colSplice.ButtThickness = ConstNum.TWO_INCHES;
					
					colSplice.ButtWidth = NumberFun.ConvertFromFraction(Math.Max(bColumn.Shape.bf, tColumn.Shape.bf), 8);
					colSplice.ButtLength = NumberFun.ConvertFromFraction(Math.Max(bColumn.Shape.d, tColumn.Shape.d) + ConstNum.THREE_INCHES, 8);
					ButtPlateForces(ref weldForceL, ref weldForceU, Acontact, ref capacity, ref weldForceUt, ref weldForceLt, ref weldForceUc, ref weldForceLc);
					
					colSplice.SpliceWidthFlange =
					colSplice.FlangePLLength =
					colSplice.SpliceLengthLowerFlange =
					colSplice.SpliceLengthUpperFlange =
					colSplice.SpliceThicknessFlange =
					colSplice.FillerThicknessFlangeLower =
					colSplice.FillerThicknessFlangeUpper =
					colSplice.FillerWidthFlangeLower =
					colSplice.FillerWidthFlangeUpper =
					colSplice.WebPLLength =
					colSplice.SpliceWidthWeb =
					colSplice.SpliceLengthLowerWeb =
					colSplice.SpliceLengthUpperWeb =
					colSplice.SpliceThicknessWeb =
					colSplice.FillerThicknessWebLower =
					colSplice.FillerThicknessWebUpper =
					colSplice.FillerWidthWebLower =
					colSplice.FillerWidthWebUpper = 0;

					switch (colSplice.ButtWeldTypeUpper)
					{
						case EWeldType.Fillet:
							minweldsize = CommonCalculations.MinimumWeld(colSplice.ButtThickness, tColumn.Shape.tf);
							FWeldsize = NumberFun.ConvertFromFraction(weldForceU / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx));
							colSplice.ButtWeldSizeUpper = Math.Max(FWeldsize, minweldsize);
							break;
						case EWeldType.PJP:
							fyc = Math.Min(colSplice.Material.Fy, tColumn.Material.Fy);
							minweldsize = ColumnSpliceMisc.MinimumPJPSize(colSplice.ButtThickness, tColumn.Shape.tf);
							FWeldSizec = NumberFun.ConvertFromFraction(weldForceUc / (ConstNum.FIOMEGA0_9N * fyc));
							FWeldSizet = NumberFun.ConvertFromFraction(weldForceUt / Math.Min(0.8 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx, ConstNum.FIOMEGA0_9N * fyc));
							colSplice.ButtWeldSizeUpper = Math.Max(FWeldsize, minweldsize);
							FWeldsize = Math.Max(FWeldSizec, FWeldSizet);
							if (colSplice.ButtWeldSizeUpper > tColumn.Shape.tf - ConstNum.EIGHTH_INCH)
							{
								Reporting.AddLine("Required PJP weld size exceeds maximum.");
								colSplice.ButtWeldSizeUpper = tColumn.Shape.tf - ConstNum.EIGHTH_INCH;
							}
							break;
						default:
							colSplice.ButtWeldSizeUpper = 0;
							break;
					}
					switch (colSplice.ButtWeldTypeLower)
					{
						case EWeldType.Fillet:
							minweldsize = CommonCalculations.MinimumWeld(colSplice.ButtThickness, bColumn.Shape.tf);
							FWeldsize = NumberFun.ConvertFromFraction(weldForceL / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx));
							colSplice.ButtWeldSizeLower = Math.Max(FWeldsize, minweldsize);
							break;
						case EWeldType.PJP:
							fyc = Math.Min(colSplice.Material.Fy, bColumn.Material.Fy);
							minweldsize = ColumnSpliceMisc.MinimumPJPSize(colSplice.ButtThickness, bColumn.Shape.tf);
							FWeldSizec = NumberFun.ConvertFromFraction(weldForceLc / (ConstNum.FIOMEGA0_9N * fyc));
							FWeldSizet = NumberFun.ConvertFromFraction(weldForceLt / Math.Min(0.8 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx, ConstNum.FIOMEGA0_9N * fyc));
							FWeldsize = Math.Max(FWeldSizec, FWeldSizet);
							colSplice.ButtWeldSizeLower = Math.Max(FWeldsize, minweldsize);
							if (colSplice.ButtWeldSizeLower > bColumn.Shape.tf - ConstNum.EIGHTH_INCH)
							{
								Reporting.AddLine("Required PJP weld size exceeds maximum.");
								colSplice.ButtWeldSizeLower = bColumn.Shape.tf - ConstNum.EIGHTH_INCH;
							}
							break;
						default:
							colSplice.ButtWeldSizeLower = 0;
							break;
					}
					break;
				case ESpliceConnection.DirectlyWelded:
					colSplice.SpliceWidthFlange = 0;
					colSplice.FlangePLLength = 0;
					colSplice.SpliceLengthLowerFlange = 0;
					colSplice.SpliceLengthUpperFlange = 0;
					colSplice.SpliceThicknessFlange = 0;
					colSplice.FillerThicknessFlangeLower = 0;
					colSplice.FillerThicknessFlangeUpper = 0;
					colSplice.FillerWidthFlangeLower = 0;
					colSplice.FillerWidthFlangeUpper = 0;
					shart1 = tColumn.Shape.d <= bColumn.Shape.d && tColumn.Shape.d - 2 * tColumn.Shape.tf >= bColumn.Shape.d - 2 * bColumn.Shape.tf && tColumn.Shape.bf <= bColumn.Shape.bf && tColumn.Shape.tw <= bColumn.Shape.tw;
					shart2 = bColumn.Shape.d <= tColumn.Shape.d && bColumn.Shape.d - 2 * bColumn.Shape.tf >= tColumn.Shape.d - 2 * tColumn.Shape.tf && bColumn.Shape.bf <= tColumn.Shape.bf && bColumn.Shape.tw <= tColumn.Shape.tw;
					if (!(shart1 || shart2))
					{
						Reporting.AddLine("\"Directly Welded\" splice option is available only when the smaller" + "\r" + "column has full bearing on the larger column." + "\r" + "\r" + "Try " + "\"Butt Plated" + "\" or " + "\"Flange Plated" + "\" splice.");
						colSplice.ConnectionOption = ESpliceConnection.ButtPlate;
						return;
					}
					Acontact = Math.Min(tColumn.Shape.a, bColumn.Shape.a);

					DirectlyWeldedForces(ref tf, ref BF, ref d, ref tw, ref dw, Acontact, ref Ff_Ten, ref Ff_Comp, ref Fw_Ten, ref Fw_Comp, ref Fw);
					ColumnSpliceWebPlate.WebPlateDesign(ref dw, ref d, ref tw, Fw, ref reduction, ref capacity, ref CSMinX, ref CSMinY, ref PLLength, ref PLType, ref Gage, ref usefulweldonPlate);
					colSplice.WebPLLength = colSplice.SpliceLengthUpperWeb + colSplice.SpliceLengthLowerWeb;
					if (colSplice.FlangeWeldType == EWeldType.PJP)
					{
						minweldsize = ColumnSpliceMisc.MinimumPJPSize(tColumn.Shape.tf, bColumn.Shape.tf);
						fyc = Math.Min(tColumn.Material.Fy, bColumn.Material.Fy);
						FWeldSizec = NumberFun.ConvertFromFraction(Ff_Comp / (ConstNum.FIOMEGA0_9N * fyc * BF), 16);
						FWeldSizet = NumberFun.ConvertFromFraction(Ff_Ten / (Math.Min(0.8 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx, ConstNum.FIOMEGA0_9N * fyc) * BF));
						FWeldsize = Math.Max(FWeldSizet, FWeldSizec);
						colSplice.FilletWeldSizeFlangeUpper = Math.Max(FWeldsize, minweldsize);
						maxweldsize = ((int)Math.Floor(16 * Math.Max(Math.Min(tColumn.Shape.tf, bColumn.Shape.tf) - ConstNum.QUARTER_INCH, 0))) / 16.0;
						if (colSplice.FilletWeldSizeFlangeUpper > maxweldsize)
							colSplice.FilletWeldSizeFlangeUpper = maxweldsize;
					}
					if (colSplice.WebWeldType == EWeldType.PJP)
					{
						minweldsize = ColumnSpliceMisc.MinimumPJPSize(tColumn.Shape.tw, bColumn.Shape.tw);
						fyc = Math.Min(tColumn.Material.Fy, bColumn.Material.Fy);
						FWeldSizec = NumberFun.ConvertFromFraction(Fw_Comp / (ConstNum.FIOMEGA0_9N * fyc * dw));
						FWeldSizet = NumberFun.ConvertFromFraction(Fw_Ten / (Math.Min(0.8 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx, ConstNum.FIOMEGA0_9N * colSplice.Material.Fy) * dw));
						FWeldsize = Math.Max(FWeldSizet, FWeldSizec);
						colSplice.FilletWeldSizeWebUpper = Math.Max(FWeldsize, minweldsize);
						maxweldsize = ((int)Math.Floor(16 * Math.Max(Math.Min(tColumn.Shape.tw, bColumn.Shape.tw) - ConstNum.QUARTER_INCH, 0))) / 16.0;
						if (colSplice.FilletWeldSizeWebUpper > maxweldsize)
							colSplice.FilletWeldSizeWebUpper = maxweldsize;
					}
					break;
			}

			ColumnSpliceCapacity.DesignColumnSpliceCapacity(dmax, Acontact, S_Contact, AcontactwithFiller, S_ContactwithFiller, Fbr_Column, Fbr_Filler, Fbr_C, d, Fbr_F, false);
		}

		private static void ContactSurface( ref double BF, ref double d, ref double dmax, ref double tf, ref double Acontact, ref double S_Contact, ref double AcontactwithFiller, ref double S_ContactwithFiller, ref bool FullContact )
		{
			double t_Min;
			double t_Max;
			double Twmin;
			double dwithFiller;
			double tfFiller;
			double AUc;
			double ALc;
			double im;
			double FilWidth;
			double ImwithFiller;
			double TdwithFiller;
			double BdwithFiller;

			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			BF = Math.Min(tColumn.Shape.bf, bColumn.Shape.bf);
			t_Min = Math.Min(tColumn.Shape.d - 2 * tColumn.Shape.tf, bColumn.Shape.d - 2 * bColumn.Shape.tf);
			t_Max = Math.Max(tColumn.Shape.d - 2 * tColumn.Shape.tf, bColumn.Shape.d - 2 * bColumn.Shape.tf);
			Twmin = Math.Min(tColumn.Shape.tw, bColumn.Shape.tw);
			d = Math.Min(tColumn.Shape.d, bColumn.Shape.d);
			dmax = Math.Max(tColumn.Shape.d, bColumn.Shape.d);
			tf = Math.Max((d - t_Max) / 2, 0);
			dwithFiller = Math.Min(tColumn.Shape.d + 2 * colSplice.FillerThicknessFlangeUpper, bColumn.Shape.d + 2 * colSplice.FillerThicknessFlangeLower);
			tfFiller = Math.Max((dwithFiller - t_Max) / 2 - tf, 0);
			AUc = 2 * tColumn.Shape.tf * tColumn.Shape.bf + tColumn.Shape.tw * (tColumn.Shape.d - 2 * tColumn.Shape.tf);
			ALc = 2 * bColumn.Shape.tf * bColumn.Shape.bf + bColumn.Shape.tw * (bColumn.Shape.d - 2 * bColumn.Shape.tf);
			Acontact = 2 * tf * BF + Twmin * t_Min;
			im = Twmin * Math.Pow(t_Min, 3) / 12 + 2 * BF * Math.Pow(tf, 3) / 12 + 2 * tf * BF * Math.Pow((d + t_Max) / 4, 2);
			if (t_Min == tColumn.Shape.d - 2 * tColumn.Shape.tf)
			{
				if (tColumn.Shape.d <= t_Max)
				{
					Acontact = Acontact + bColumn.Shape.tw * (tColumn.Shape.d - t_Min);
					im = im + 2 / 12.0 * bColumn.Shape.tw * Math.Pow((tColumn.Shape.d - t_Min) / 2, 3) + 2 * bColumn.Shape.tw * (tColumn.Shape.d - t_Min) * Math.Pow((t_Min + tColumn.Shape.d) / 4, 2);
				}
				else
				{
					Acontact = Acontact + bColumn.Shape.tw * (t_Max - t_Min);
					im = im + 2 / 12.0 * bColumn.Shape.tw * Math.Pow((t_Max - t_Min) / 2, 3) + bColumn.Shape.tw * (t_Max - t_Min) * Math.Pow((t_Max + t_Min) / 4, 2);
				}
			}
			else
			{
				if (bColumn.Shape.d <= t_Max)
				{
					Acontact = Acontact + tColumn.Shape.tw * (bColumn.Shape.d - t_Min);
					im = im + 2 / 12.0 * tColumn.Shape.tw * Math.Pow((bColumn.Shape.d - t_Min) / 2, 3) + 2 * tColumn.Shape.tw * (bColumn.Shape.d - t_Min) * Math.Pow((t_Min + bColumn.Shape.d) / 4, 2);
				}
				else
				{
					Acontact = Acontact + tColumn.Shape.tw * (t_Max - t_Min);
					im = im + 2 / 12.0 * tColumn.Shape.tw * Math.Pow((t_Max - t_Min) / 2, 3) + tColumn.Shape.tw * (t_Max - t_Min) * Math.Pow((t_Max + t_Min) / 4, 2);
				}
			}
			S_Contact = im / (d / 2);

			FilWidth = Math.Max(colSplice.FillerWidthFlangeUpper, colSplice.FillerWidthFlangeLower);
			AcontactwithFiller = 2 * tf * BF + Twmin * t_Min + 2 * tfFiller * FilWidth;
			ImwithFiller = Twmin * Math.Pow(t_Min, 3) / 12 + 2 * BF * Math.Pow(tf, 3) / 12 + 2 * tf * BF * Math.Pow((d + t_Max) / 4, 2) + 2 * FilWidth * Math.Pow(tfFiller, 3) / 12 + 2 * FilWidth * tfFiller * Math.Pow((dwithFiller + t_Max) / 4 - tf, 2);
			if (t_Min == tColumn.Shape.d - 2 * tColumn.Shape.tf)
			{
				TdwithFiller = tColumn.Shape.d + 2 * colSplice.FillerThicknessFlangeUpper;
				if (TdwithFiller <= t_Max)
				{
					AcontactwithFiller = AcontactwithFiller + bColumn.Shape.tw * (TdwithFiller - t_Min);
					ImwithFiller = ImwithFiller + 2 / 12.0 * bColumn.Shape.tw * Math.Pow((TdwithFiller - t_Min) / 2, 3) + 2 * bColumn.Shape.tw * (TdwithFiller - t_Min) * Math.Pow((t_Min + TdwithFiller) / 4, 2);
				}
				else
				{
					AcontactwithFiller = AcontactwithFiller + bColumn.Shape.tw * (t_Max - t_Min);
					ImwithFiller = ImwithFiller + 2 / 12.0 * bColumn.Shape.tw * Math.Pow((t_Max - t_Min) / 2, 3) + bColumn.Shape.tw * (t_Max - t_Min) * Math.Pow((t_Max + t_Min) / 4, 2);
				}
			}
			else
			{
				BdwithFiller = bColumn.Shape.d + 2 * colSplice.FillerThicknessFlangeLower;
				if (BdwithFiller <= t_Max)
				{
					Acontact = Acontact + tColumn.Shape.tw * (BdwithFiller - t_Min);
					ImwithFiller = ImwithFiller + 2 / 12.0 * tColumn.Shape.tw * Math.Pow((BdwithFiller - t_Min) / 2, 3) + 2 * tColumn.Shape.tw * (BdwithFiller - t_Min) * Math.Pow((t_Min + BdwithFiller) / 4, 2);
				}
				else
				{
					AcontactwithFiller = AcontactwithFiller + tColumn.Shape.tw * (t_Max - t_Min);
					ImwithFiller = ImwithFiller + 2 / 12.0 * tColumn.Shape.tw * Math.Pow((t_Max - t_Min) / 2, 3) + tColumn.Shape.tw * (t_Max - t_Min) * Math.Pow((t_Max + t_Min) / 4, 2);
				}
			}
			S_ContactwithFiller = ImwithFiller / (dwithFiller / 2);
			if (Math.Floor(100 * Acontact) / 100.0 < (Math.Floor(100 * Math.Min(AUc, ALc))) / 100.0)
				FullContact = false;
			else
				FullContact = true;
		}

		private static void FlangeSpliceForces( double Acontact, ref double Fbr_Column, double AcontactwithFiller, ref double Fbr_Filler, ref double Fbr_C, ref double Fbr_F, ref double FfillerC, ref double FspliceC, ref double FspliceT, double dmax, double S_Contact, double S_ContactwithFiller, ref double Fsplice, ref double FfillerLF, ref double FfillerUF )
		{
			double Ffiller0c = 0;
			double Fsplice0c = 0;
			double Ffiller0T = 0;
			double Fsplice0t = 0;
			double FfillerT;
			double Fa;
			double Fb;
			double Pexcess;
			double Ffiller;

			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Fbr_Column = ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(tColumn.Material.Fy, bColumn.Material.Fy) * Acontact;
			Fbr_Filler = Fbr_Column + ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(colSplice.Material.Fy, bColumn.Material.Fy) * (AcontactwithFiller - Acontact);

			Fbr_C = ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(tColumn.Material.Fy, bColumn.Material.Fy);
			Fbr_F = ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(colSplice.Material.Fy, bColumn.Material.Fy);

			FfillerC = 0;
			FspliceC = 0;
			FfillerT = 0;
			FspliceT = 0;

			if (colSplice.Moment == 0 && colSplice.Compression > 0)
			{
				if (colSplice.Compression <= Fbr_Column)
				{
					FfillerC = 0;
					FspliceC = 0;
				}
				else if (colSplice.Compression <= Fbr_Filler)
				{
					FfillerC = colSplice.Compression / AcontactwithFiller * (AcontactwithFiller - Acontact) / 2;
					FspliceC = 0;
				}
				else
				{
					FfillerC = (Fbr_Filler - Fbr_Column) / 2;
					FspliceC = (colSplice.Compression - Fbr_Filler) / 2;
				}
			}
			Ffiller0c = Math.Max(Ffiller0c, FfillerC);
			Fsplice0c = Math.Max(Fsplice0c, FspliceC);

			if (colSplice.Moment == 0 && colSplice.Tension > 0)
			{
				FfillerT = 0;
				FspliceT = colSplice.Tension / 2;
			}
			Ffiller0T = Math.Max(Ffiller0T, FfillerT);
			Fsplice0t = Math.Max(Fsplice0t, FspliceT);

			if (colSplice.Moment > 0 && colSplice.Tension >= 0)
			{
				FfillerT = 0;
				FspliceT = colSplice.Tension / 2 + colSplice.Moment / dmax;
			}
			else if (colSplice.Moment > 0 && colSplice.Tension == 0)
			{
				FfillerT = 0;
				FspliceT = -colSplice.Cmin / 2 + colSplice.Moment / dmax;
			}
			Ffiller0T = Math.Max(Ffiller0T, FfillerT);
			Fsplice0t = Math.Max(Fsplice0t, FspliceT);

			if (colSplice.Moment > 0 && colSplice.Compression >= 0)
			{
				Fa = colSplice.Compression / Acontact;
				Fb = colSplice.Moment / S_Contact;
				if (Fa <= Fbr_C)
				{
					if (Fa + Fb <= Fbr_C)
					{
						FfillerC = 0;
						FspliceC = 0;
					}
					else
					{
						Pexcess = (Fa + Fb - Fbr_C) * S_Contact / dmax;
						if (AcontactwithFiller - Acontact > 0)
						{
							Fa = Pexcess / ((AcontactwithFiller - Acontact) / 2);
							if (Fa <= Fbr_F)
							{
								FfillerC = Pexcess;
								FspliceC = 0;
							}
							else
							{
								FfillerC = Fbr_F * (AcontactwithFiller - Acontact) / 2;
								FspliceC = Pexcess - FfillerC;
							}
						}
						else
						{
							FfillerC = 0;
							FspliceC = Pexcess;
						}
					}
				}
				else
				{
					Fa = colSplice.Compression / AcontactwithFiller;
					Fb = colSplice.Moment / S_ContactwithFiller;
					if (Fa + Fb <= Fbr_F)
					{
						FfillerC = (Fa + Fb) * (AcontactwithFiller - Acontact) / 2;
						FspliceC = 0;
					}
					else
					{
						Pexcess = (Fa + Fb - Fbr_F) * S_ContactwithFiller / dmax;
						FfillerC = Fbr_F * (AcontactwithFiller - Acontact) / 2;
						FspliceC = Pexcess;
					}
				}
				Ffiller0c = Math.Max(Ffiller0c, FfillerC);
				Fsplice0c = Math.Max(Fsplice0c, FspliceC);

				Fa = 0.75 * colSplice.Compression / Acontact - colSplice.Moment / S_Contact;
				if (Fa >= 0)
				{
					FfillerT = 0;
					FspliceT = 0;
				}
				else
				{
					FfillerT = 0;
					FspliceT = (colSplice.Moment - 0.75 * colSplice.Compression / Acontact * S_Contact) / dmax;
				}
				Ffiller0T = Math.Max(Ffiller0T, FfillerT);
				Fsplice0t = Math.Max(Fsplice0t, FspliceT);

				Fa = 0.75 * colSplice.Compression / AcontactwithFiller - colSplice.Moment / S_ContactwithFiller;
				if (Fa >= 0)
				{
					FfillerT = 0;
					FspliceT = 0;
				}
				else
				{
					FfillerT = 0;
					FspliceT = (colSplice.Moment - 0.75 * colSplice.Compression / AcontactwithFiller * S_ContactwithFiller) / dmax;
				}
				Ffiller0T = Math.Max(Ffiller0T, FfillerT);
				Fsplice0t = Math.Max(Fsplice0t, FspliceT);
			}

			Ffiller0c = Math.Max(Ffiller0c, FfillerC);
			Fsplice0c = Math.Max(Fsplice0c, FspliceC);
			FfillerC = Ffiller0c;
			FspliceC = Fsplice0c;

			Ffiller0T = Math.Max(Ffiller0T, FfillerT);
			Fsplice0t = Math.Max(Fsplice0t, FspliceT);
			FfillerT = Ffiller0T;
			FspliceT = Fsplice0t;
			if (colSplice.UseSeismic && colSplice.SMF)
				FspliceT = Math.Max(FspliceT, FfToDevMoment);
			Ffiller = Math.Max(FfillerC, FfillerT);
			Fsplice = Math.Max(FspliceC, FspliceT);

			if (colSplice.FillerThicknessFlangeLower == 0)
				FfillerLF = 0;
			else
				FfillerLF = Ffiller;

			if (colSplice.FillerThicknessFlangeUpper == 0)
				FfillerUF = 0;
			else
				FfillerUF = Ffiller;
		}

		private static void ButtPlateForces( ref double weldForceL, ref double weldForceU, double Acontact, ref double capacity, ref double weldForceUt, ref double weldForceLt, ref double weldForceUc, ref double weldForceLc )
		{
			double FbrTop;
			double FbrBot;
			double nonContactArea;
			double tForShear;
			double Ff_cU = 0;
			double Ff_cL = 0;
			double FForce = 0;
			double PlateShearCapacity;
			double M;
			double Mcapacity;
			double Ff_tU = 0;
			double Ff_tL = 0;
			double FlangeforceU;
			double FlangeforceL;
			double flangeforceUc;
			double flangeforceLc;
			double V;

			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			if (colSplice.Moment == 0 && colSplice.Compression > 0)
			{
				// case 1
				FbrTop = ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(tColumn.Material.Fy, colSplice.Material.Fy) * tColumn.Shape.a;
				FbrBot = ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(bColumn.Material.Fy, colSplice.Material.Fy) * bColumn.Shape.a;
				if (colSplice.Compression > Math.Min(FbrTop, FbrBot))
					Reporting.AddLine("Bearing strength is insufficient.");

				nonContactArea = Math.Min(tColumn.Shape.a, bColumn.Shape.a) - Acontact;
				tForShear = colSplice.Compression / Math.Min(tColumn.Shape.a, bColumn.Shape.a) * nonContactArea / (ConstNum.FIOMEGA0_9N * 0.6 * colSplice.Material.Fy * (colSplice.ButtWidth - Math.Max(tColumn.Shape.tw, bColumn.Shape.tw)) * 2);
				colSplice.ButtThickness = Math.Max(tForShear, colSplice.ButtThickness);
				Ff_cU = colSplice.Compression / tColumn.Shape.a * (tColumn.Shape.tf * tColumn.Shape.bf);
				Ff_cL = colSplice.Compression / bColumn.Shape.a * (bColumn.Shape.tf * bColumn.Shape.bf);
			}
			if (colSplice.Moment == 0 && colSplice.Tension > 0)
			{
				FForce = Math.Max(0, colSplice.Tension - colSplice.Cmin);
				PlateShearCapacity = colSplice.ButtThickness * ConstNum.FIOMEGA0_9N * 0.6 * colSplice.Material.Fy * colSplice.ButtWidth;
				if (FForce / 2 > PlateShearCapacity)
					colSplice.ButtThickness = NumberFun.ConvertFromFraction(colSplice.ButtThickness * (FForce / 2) / PlateShearCapacity, 8);

				M = FForce / 2 * Math.Abs(bColumn.Shape.d - tColumn.Shape.d) / 2;
				Mcapacity = ConstNum.FIOMEGA0_9N * colSplice.Material.Fy * Math.Pow(colSplice.ButtThickness, 2) * colSplice.ButtWidth / 4;
				if (M > Mcapacity)
					colSplice.ButtThickness = NumberFun.ConvertFromFraction(colSplice.ButtThickness * Math.Sqrt(M / Mcapacity), 8);
				
				Ff_tU = FForce / 2;
				Ff_tL = FForce / 2;
			}

			if (colSplice.Moment > 0 && colSplice.Tension >= 0)
			{
				if (colSplice.Tension > 0)
				{
					FlangeforceU = colSplice.Tension / 2 + colSplice.Moment / tColumn.Shape.d;
					FlangeforceL = colSplice.Tension / 2 + colSplice.Moment / bColumn.Shape.d;
					capacity = colSplice.ButtThickness * ConstNum.FIOMEGA0_9N * 0.6 * colSplice.Material.Fy * colSplice.ButtWidth;
					Ff_tU = Math.Max(Ff_tU, FlangeforceU);
					Ff_tL = Math.Max(Ff_tL, FlangeforceL);
				}
				else
				{
					FlangeforceU = -colSplice.Cmin / 2 + colSplice.Moment / tColumn.Shape.d;
					FlangeforceL = -colSplice.Cmin / 2 + colSplice.Moment / bColumn.Shape.d;
					capacity = colSplice.ButtThickness * ConstNum.FIOMEGA0_9N * 0.6 * colSplice.Material.Fy * colSplice.ButtWidth;
					Ff_tU = Math.Max(Ff_tU, FlangeforceU);
					Ff_tL = Math.Max(Ff_tL, FlangeforceL);
				}

				flangeforceUc = Math.Max(-FForce / 2 + colSplice.Moment / (tColumn.Shape.d - tColumn.Shape.tf), 0);
				flangeforceLc = Math.Max(-FForce / 2 + colSplice.Moment / (bColumn.Shape.d - bColumn.Shape.tf), 0);
				Ff_cU = Math.Max(Ff_cU, flangeforceUc);
				Ff_cL = Math.Max(Ff_cL, flangeforceLc);

				V = Math.Min(FlangeforceU, FlangeforceL);
				if (V > capacity)
					colSplice.ButtThickness = NumberFun.ConvertFromFraction(colSplice.ButtThickness * V / capacity, 8);
				M = V * Math.Abs(bColumn.Shape.d - tColumn.Shape.d) / 2;
				Mcapacity = ConstNum.FIOMEGA0_9N * colSplice.Material.Fy * Math.Pow(colSplice.ButtThickness, 2) * colSplice.ButtWidth / 4;
				if (M > Mcapacity)
					colSplice.ButtThickness = NumberFun.ConvertFromFraction(colSplice.ButtThickness * Math.Sqrt(M / Mcapacity), 8);
			}

			if (colSplice.Moment > 0 && colSplice.Compression >= 0)
			{
				FlangeforceU = colSplice.Compression / tColumn.Shape.a * (tColumn.Shape.tf * tColumn.Shape.bf) + colSplice.Moment / (tColumn.Shape.d - tColumn.Shape.tf);
				if (FlangeforceU > ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(tColumn.Material.Fy, colSplice.Material.Fy) * (tColumn.Shape.tf * tColumn.Shape.bf))
					Reporting.AddLine("Bearing strength is insufficient.");

				FlangeforceL = colSplice.Compression / bColumn.Shape.a * (bColumn.Shape.tf * bColumn.Shape.bf) + colSplice.Moment / (bColumn.Shape.d - bColumn.Shape.tf);
				if (FlangeforceL > ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(bColumn.Material.Fy, colSplice.Material.Fy) * (bColumn.Shape.tf * bColumn.Shape.bf))
					Reporting.AddLine("Bearing strength is insufficient.");
				
				Ff_cU = Math.Max(Ff_cU, FlangeforceU);
				Ff_cL = Math.Max(Ff_cL, FlangeforceL);

				FlangeforceU = Math.Abs(Math.Min(colSplice.Compression / tColumn.Shape.a * (tColumn.Shape.tf * tColumn.Shape.bf) - colSplice.Moment / (tColumn.Shape.d - tColumn.Shape.tf), 0));
				FlangeforceL = Math.Abs(Math.Min(colSplice.Compression / bColumn.Shape.a * (bColumn.Shape.tf * bColumn.Shape.bf) - colSplice.Moment / (bColumn.Shape.d - bColumn.Shape.tf), 0));
				Ff_tU = Math.Max(Ff_tU, FlangeforceU);
				Ff_tL = Math.Max(Ff_tL, FlangeforceL);
			}

			if (colSplice.UseSeismic)
			{
				if (colSplice.SMF)
				{
					Ff_tU = Math.Max(Ff_tU, FfToDevMoment);
					Ff_tL = Math.Max(Ff_tL, FfToDevMoment);
				}
				if (colSplice.ButtWeldTypeUpper == EWeldType.PJP)
					weldForceUt = Math.Max(2 * Ff_tU, FlangeTensionforSeismic) / tColumn.Shape.bf;
				else
					weldForceUt = Math.Max(Ff_tU, FlangeTensionforSeismic) / tColumn.Shape.bf;
				
				if (colSplice.ButtWeldTypeLower == EWeldType.PJP)
					weldForceLt = Math.Max(2 * Ff_tL, FlangeTensionforSeismic) / bColumn.Shape.bf;
				else
					weldForceLt = Math.Max(Ff_tL, FlangeTensionforSeismic) / bColumn.Shape.bf;
			}
			else
			{
				weldForceUt = Ff_tU / tColumn.Shape.bf;
				weldForceLt = Ff_tL / bColumn.Shape.bf;
			}
			weldForceUc = Ff_cU / tColumn.Shape.bf;
			weldForceLc = Ff_cL / bColumn.Shape.bf;
			weldForceL = Math.Max(weldForceLt, weldForceLc);
			weldForceU = Math.Max(weldForceUt, weldForceUc);
		}

		private static void DirectlyWeldedForces( ref double tf, ref double BF, ref double d, ref double tw, ref double dw, double Acontact, ref double Ff_Ten, ref double Ff_Comp, ref double Fw_Ten, ref double Fw_Comp, ref double Fw )
		{
			double Ff_Comp1 = 0;
			double Fw_Comp1 = 0;
			double Ff_Ten2 = 0;
			double Fw_Ten2 = 0;
			double Ff_Comp3 = 0;
			int Fw_Comp3 = 0;
			double Ff_Ten3 = 0;
			double Fw_Ten3 = 0;
			double Ff_Comp4 = 0;
			double Fw_Comp4 = 0;
			double Ff_Ten4 = 0;
			int Fw_Ten4 = 0;
			double Ff_T = 0;
			double FfSeismic = 0;

			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			tf = Math.Min(tColumn.Shape.tf, bColumn.Shape.tf);
			BF = Math.Min(tColumn.Shape.bf, bColumn.Shape.bf);
			d = Math.Min(tColumn.Shape.d, bColumn.Shape.d);
			tw = Math.Min(tColumn.Shape.tw, bColumn.Shape.tw);

			dw = Math.Min(tColumn.Shape.d - tColumn.Shape.tf, bColumn.Shape.d - bColumn.Shape.tf);
			if (colSplice.Moment == 0 && colSplice.Compression > 0)
			{
				Ff_Comp1 = colSplice.Compression / Acontact * tf * BF;
				Fw_Comp1 = colSplice.Compression - 2 * Ff_Comp1;
			}

			if (colSplice.Moment == 0 && colSplice.Tension > 0)
			{
				Ff_Ten2 = colSplice.Tension / Acontact * tf * BF;
				Fw_Ten2 = colSplice.Tension - 2 * Ff_Ten2;
			}
			if (colSplice.Moment > 0 && colSplice.Tension >= 0)
			{
				if (colSplice.Tension > 0)
				{
					Ff_Ten3 = colSplice.Tension / Acontact * tf * BF + colSplice.Moment / d;
					Fw_Ten3 = colSplice.Tension - 2 * Ff_Ten3;
					Ff_Comp3 = Math.Max(-colSplice.Tension / Acontact * tf * BF + colSplice.Moment / d, 0);
					Fw_Comp3 = 0;
				}
				else
				{
					Ff_Ten3 = Math.Max(0, -colSplice.Cmin / Acontact * tf * BF + colSplice.Moment / d);
					Fw_Ten3 = Math.Max(0, -colSplice.Cmin - 2 * Ff_Ten3);
					Ff_Comp3 = Math.Max(-colSplice.Tension / Acontact * tf * BF + colSplice.Moment / d, 0);
					Fw_Comp3 = 0;
				}
			}

			if (colSplice.Moment > 0 && colSplice.Compression >= 0)
			{
				Ff_Comp4 = colSplice.Compression / Acontact * tf * BF + colSplice.Moment / d;
				Fw_Comp4 = colSplice.Compression / Acontact * tw * (d - 2 * tf);
				
				Ff_T = colSplice.Compression / Acontact * (tColumn.Shape.tf - ConstNum.QUARTER_INCH) * BF - colSplice.Moment / d;
				Ff_T = Math.Abs(Math.Min(Ff_T, 0));
				Ff_Ten4 = Math.Max(-colSplice.Compression / Acontact * tf * BF + colSplice.Moment / d, 0);
				Fw_Ten4 = 0;
			}

			Ff_Ten = Math.Max(Ff_Ten2, Math.Max(Ff_Ten3, Ff_Ten4));
			Ff_Comp = Math.Max(Ff_Comp1, Math.Max(Ff_Comp3, Ff_Comp4));
			Fw_Ten = Math.Max(Fw_Ten2, Math.Max(Fw_Ten3, Fw_Ten4));
			Fw_Comp = Math.Max(Fw_Comp1, Math.Max(Fw_Comp3, Fw_Comp4));
			Fw = Math.Max(Fw_Ten, Fw_Comp);

			if (colSplice.UseSeismic)
			{
				if (colSplice.SMF)
					Ff_Ten = Math.Max(Ff_Ten, FfToDevMoment);
				
				FfSeismic = Math.Max(Ff_Ten, FlangeTensionforSeismic);			
				if (colSplice.FlangeWeldType == EWeldType.PJP)
					FfSeismic = Math.Max(2 * Ff_Ten, FfSeismic);
			}
			else
				FfSeismic = Ff_Ten;
		}

		private static void Fillers( double dl, double du, ref double tl, ref double tu )
		{
			var colSplice = CommonDataStatic.ColumnSplice;
			
			if (colSplice.ConnectionUpper == EConnectionStyle.Bolted && colSplice.ConnectionLower == EConnectionStyle.Bolted)
			{
				if (dl >= du + ConstNum.QUARTER_INCH && dl <= du + ConstNum.FIVE_EIGHTS_INCH)
				{
					tl = 0;
					tu = 0;
				}
				else if (dl >= du - ConstNum.QUARTER_INCH && dl <= du + ConstNum.EIGHTH_INCH)
				{
					if (dl >= du && dl <= du + ConstNum.EIGHTH_INCH)
						tu = ConstNum.EIGHTH_INCH;
					else if (dl >= du - ConstNum.QUARTER_INCH && dl <= du - ConstNum.EIGHTH_INCH)
						tu = ConstNum.QUARTER_INCH;
				}
				else if (dl >= du + NumberFun.ConvertFromFraction(12))
					tu = (dl - du) / 2 - ConstNum.EIGHTH_INCH;
				
				tl = 0;
			}
			else if (colSplice.ConnectionUpper == EConnectionStyle.Bolted && colSplice.ConnectionLower == EConnectionStyle.Welded)
			{
				if (dl >= du + ConstNum.QUARTER_INCH && dl <= du + ConstNum.FIVE_EIGHTS_INCH)
				{
					tu = 0;
					tl = 0;
				}
				else if (dl >= du - ConstNum.QUARTER_INCH && dl <= du + ConstNum.EIGHTH_INCH)
				{
					if (dl >= du && dl <= du + ConstNum.EIGHTH_INCH)
						tl = ConstNum.EIGHTH_INCH;
					else
						tl = ConstNum.THREE_SIXTEENTHS;
					tu = 0;
				}
				else if (dl >= du + NumberFun.ConvertFromFraction(12))
				{
					tu = (dl - du) / 2 - ConstNum.EIGHTH_INCH;
					tl = 0;
				}
				else
				{
					tu = (dl - du) / 2 - ConstNum.EIGHTH_INCH;
					tl = 0;
				}
			}
			else if (colSplice.ConnectionUpper == EConnectionStyle.Welded && colSplice.ConnectionLower == EConnectionStyle.Bolted)
			{
				if ((du - dl) < -ConstNum.QUARTER_INCH)
				{
					if (8 * (dl - du) / 2 - ((int)Math.Floor(8 * (dl - du) / 2)) == ConstNum.HALF_INCH)
						tu = (8 * (dl - du) / 2 - 0.01) / 8 - ConstNum.EIGHTH_INCH;
					else
						tu = ((8 * (dl - du) / 2)) / 8 - ConstNum.EIGHTH_INCH;
					tl = 0;
				}
				else if (du - dl < 0)
				{
					tu = ConstNum.QUARTER_INCH;
					tl = 0;
				}
				else if ((du - dl) <= ConstNum.EIGHTH_INCH)
				{
					tu = ConstNum.EIGHTH_INCH;
					tl = 0;
				}
				else if ((du - dl) <= ConstNum.FIVE_EIGHTS_INCH)
				{
					tu = 0;
					tl = 0;
				}
				else
				{
					tu = 0;
					if (8 * (du - dl) / 2 - ((int)Math.Floor(8 * (du - dl) / 2)) == ConstNum.HALF_INCH)
						tl = (8 * (du - dl) / 2 - 0.01) / 8 - ConstNum.EIGHTH_INCH;
					else
						tl = ((8 * (du - dl) / 2)) / 8 - ConstNum.EIGHTH_INCH;
				}
			}
			else
			{
				if (dl >= du - ConstNum.QUARTER_INCH && dl <= du)
				{
					tu = 0;
					tl = NumberFun.ConvertFromFraction((dl - du) / 2 + ConstNum.SIXTEENTH_INCH);
				}
				else if (dl > du && dl <= du + ConstNum.EIGHTH_INCH)
				{
					tu = 0;
					tl = 0;
				}
				else if (dl > du + ConstNum.EIGHTH_INCH && dl <= du + ConstNum.HALF_INCH)
				{
					tu = NumberFun.ConvertFromFraction((dl - du) / 2 - ConstNum.SIXTEENTH_INCH);
					tl = 0;
				}
				else if (dl > du + ConstNum.HALF_INCH)
				{
					tu = NumberFun.ConvertFromFraction((dl - du) / 2 - ConstNum.SIXTEENTH_INCH);
					tl = 0;
				}
			}

			tu = NumberFun.Round(tu, ERoundingPrecision.Eighth, ERoundingStyle.RoundUp);
			tl = NumberFun.Round(tl, ERoundingPrecision.Eighth, ERoundingStyle.RoundUp);
		}

		internal static void WeldsLowerWeb( ref double d, ref double tw, ref double FWeldSizeChToCol, ref double FWeldSizeChToFiller, ref double FWeldSizeFillerToCol, ref double CSMinX, ref double CSMinY, ref double PLLength, ref double PLType, ref double Gage, ref double NumPLorCH, ref double Fdesign, double Fw, ref double usefulweldonPlate, double F_Horizontal, double F_Vertical, ref double w_yf )
		{
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			if (colSplice.ConnectionOption == ESpliceConnection.DirectlyWelded)
			{
				d = colSplice.Channel.d;
				tw = colSplice.Channel.tw;
				FWeldSizeChToCol = CommonCalculations.MinimumWeld(bColumn.Shape.tw, d);
				FWeldSizeChToFiller = CommonCalculations.MinimumWeld(colSplice.FillerThicknessWebLower, tw);
				FWeldSizeFillerToCol = CommonCalculations.MinimumWeld(bColumn.Shape.tw, colSplice.FillerThicknessWebLower);
				ColumnSpliceMisc.ColSpliceMinPLThickness(ref CSMinX, ref CSMinY, ref colSplice.WebPLWidth, ref PLLength, ref PLType, ref Gage, ref CommonDataStatic.Minth);
				colSplice.WeldLengthXLW = Math.Max(((int)Math.Ceiling(d)) / 4.0, CSMinX);
				colSplice.WeldLengthYLW = Math.Max(CSMinY, d);
				NumPLorCH = colSplice.NumberOfChannels;
				if (colSplice.ChannelType == ESpliceChannelType.Temporary)
					Fdesign = 0;
				else
					Fdesign = Fw;
				if (colSplice.WebShear > 0)
					colSplice.WeldLengthXLW = d / 2;
			}
			else
			{
				d = colSplice.SpliceWidthWeb;
				tw = colSplice.SpliceThicknessWeb;
				FWeldSizeChToCol = CommonCalculations.MinimumWeld(bColumn.Shape.tw, d) + colSplice.FillerThicknessWebLower;
				FWeldSizeChToFiller = CommonCalculations.MinimumWeld(colSplice.FillerThicknessWebLower, tw);
				FWeldSizeFillerToCol = CommonCalculations.MinimumWeld(bColumn.Shape.tw, colSplice.FillerThicknessWebLower);

				ColumnSpliceMisc.ColSpliceMinPLThickness(ref CSMinX, ref CSMinY, ref colSplice.WebPLWidth, ref PLLength, ref PLType, ref Gage, ref CommonDataStatic.Minth);
				tw = CommonDataStatic.Minth;
				if (colSplice.SpliceThicknessWeb < tw)
					colSplice.SpliceThicknessWeb = tw;
				colSplice.WeldLengthXLW = d / 2;
				colSplice.WeldLengthYLW = Math.Max(CSMinY, d);
				NumPLorCH = 2;
				Fdesign = colSplice.WebShear;
			}
			if (colSplice.FillerThicknessWebLower == 0)
			{
				if (FWeldSizeChToCol > colSplice.FilletWeldSizeWebLower)
					colSplice.FilletWeldSizeWebLower = FWeldSizeChToCol;
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * tw, bColumn.Material.Fu * bColumn.Shape.tw / NumPLorCH) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebLower)
					w_y = ColumnSpliceMisc.WeldLengthForShear(false, 0, d, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				else
					w_y = ColumnSpliceMisc.WeldLengthForShear(false, 0, d, colSplice.FilletWeldSizeWebLower, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				if (w_y > colSplice.WeldLengthYLW)
					colSplice.WeldLengthYLW = NumberFun.ConvertFromFraction(w_y, 2);
				
				colSplice.WeldLengthYLW = NumberFun.ConvertFromFraction(colSplice.WeldLengthYLW, 2);
				colSplice.SpliceLengthLowerWeb = colSplice.WeldLengthYLW + ConstNum.TWO_INCHES;
				colSplice.FillerLengthWebLower = 0;
				colSplice.FillerWeldLengthLW = 0;
			}
			else if (colSplice.FillerThicknessWebLower < ConstNum.QUARTER_INCH)
			{
				if (FWeldSizeChToCol > colSplice.FilletWeldSizeWebLower)
					colSplice.FilletWeldSizeWebLower = FWeldSizeChToCol;
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * tw, bColumn.Material.Fu * bColumn.Shape.tw / NumPLorCH) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebLower - colSplice.FillerThicknessWebLower)
					w_y = ColumnSpliceMisc.WeldLengthForShear(false, 0, d, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				else
					w_y = ColumnSpliceMisc.WeldLengthForShear(false, 0, d, (colSplice.FilletWeldSizeWebLower - colSplice.FillerThicknessWebLower), F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				if (w_y > colSplice.WeldLengthYLW)
					colSplice.WeldLengthYLW = w_y;
				
				colSplice.WeldLengthYLW = NumberFun.ConvertFromFraction(colSplice.WeldLengthYLW, 2);
				colSplice.SpliceLengthLowerWeb = colSplice.WeldLengthYLW + ConstNum.TWO_INCHES;
				colSplice.FillerLengthWebLower = colSplice.SpliceLengthLowerWeb;
				colSplice.FillerWeldLengthLW = 0;
			}
			else
			{
				if (FWeldSizeChToFiller > colSplice.FilletWeldSizeWebLower)
					colSplice.FilletWeldSizeWebLower = FWeldSizeChToFiller;
				usefulweldonPlate = colSplice.Material.Fu * Math.Min(tw, colSplice.FillerThicknessWebLower) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebLower)
					w_y = ColumnSpliceMisc.WeldLengthForShear(false, 0, d, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				else
					w_y = ColumnSpliceMisc.WeldLengthForShear(false, 0, d, colSplice.FilletWeldSizeWebLower, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				
				if (w_y > colSplice.WeldLengthYLW)
					colSplice.WeldLengthYLW = w_y;
				colSplice.WeldLengthYLW = NumberFun.ConvertFromFraction(colSplice.WeldLengthYLW, 2);
				colSplice.SpliceLengthLowerWeb = colSplice.WeldLengthYLW + ConstNum.TWO_INCHES;
				if (FWeldSizeFillerToCol > colSplice.FillerWeldSizeLW)
					colSplice.FillerWeldSizeLW = FWeldSizeFillerToCol;
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * colSplice.FillerThicknessWebLower, bColumn.Material.Fu * bColumn.Shape.tw / NumPLorCH) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				if (usefulweldonPlate < colSplice.FillerWeldSizeLW)
					w_yf = ColumnSpliceMisc.WeldLengthForShear(false, 0, colSplice.FillerWidthWebLower, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "II");
				else
					w_yf = ColumnSpliceMisc.WeldLengthForShear(false, 0, d, colSplice.FilletWeldSizeWebLower, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "II");
				
				if (w_yf > colSplice.WeldLengthYLW)
					colSplice.FillerWeldLengthLW = w_yf;
			}
			colSplice.FillerWeldLengthLW = NumberFun.ConvertFromFraction(colSplice.FillerWeldLengthLW, 2);
			colSplice.FillerWeldLengthLW = Math.Max(colSplice.FillerWeldLengthLW, colSplice.WeldLengthYLW + ConstNum.ONE_INCH);
			colSplice.FillerLengthWebLower = colSplice.FillerWeldLengthLW + ConstNum.TWO_INCHES;
		}

		private static void FillerThickness()
		{
			double dl;
			double du;
			double tl = 0;
			double tu = 0;

			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;
			
			if (colSplice.ConnectionOption == ESpliceConnection.FlangePlate || colSplice.ConnectionOption == ESpliceConnection.FlangeAndWebPlate)
			{
				dl = NumberFun.Round(bColumn.Shape.d, 8);
				du = NumberFun.Round(tColumn.Shape.d, 8);
				Fillers(dl, du, ref tl, ref tu);
				colSplice.FillerThicknessFlangeLower = tl;
				colSplice.FillerThicknessFlangeUpper = tu;
				colSplice.FillerThicknessWebLower = 0;
				colSplice.FillerThicknessWebUpper = 0;
			}
			if (colSplice.ConnectionOption == ESpliceConnection.FlangeAndWebPlate || colSplice.ConnectionOption == ESpliceConnection.DirectlyWelded)
			{
				if (colSplice.ConnectionOption == ESpliceConnection.DirectlyWelded)
				{
					colSplice.FillerThicknessFlangeLower = 0;
					colSplice.FillerThicknessFlangeUpper = 0;
				}

				dl = NumberFun.Round(bColumn.Shape.tw, 8);
				du = NumberFun.Round(tColumn.Shape.tw, 8);
				Fillers(dl, du, ref tl, ref tu);
				colSplice.FillerThicknessWebLower = tl;
				colSplice.FillerThicknessWebUpper = tu;
				if (colSplice.SpliceLengthLowerWeb == 0)
					colSplice.SpliceLengthLowerWeb = ConstNum.FOUR_INCHES;
				if (colSplice.SpliceLengthUpperWeb == 0)
					colSplice.SpliceLengthUpperWeb = ConstNum.FOUR_INCHES;
				if (colSplice.ConnectionLower == EConnectionStyle.Bolted)
				{
					colSplice.FillerLengthWebLower = colSplice.SpliceLengthLowerWeb;
					colSplice.FillerWidthWebLower = colSplice.SpliceWidthWeb;
				}
				else
				{
					if (colSplice.FillerThicknessWebLower >= ConstNum.QUARTER_INCH)
					{
						colSplice.FillerLengthWebLower = colSplice.SpliceLengthLowerWeb + ConstNum.TWO_INCHES;
						colSplice.FillerWidthWebLower = colSplice.SpliceWidthWeb + ConstNum.TWO_INCHES;
						colSplice.FillerWeldLengthLW = colSplice.FillerLengthWebLower - ConstNum.TWO_INCHES;
					}
					else
					{
						colSplice.FillerLengthWebLower = colSplice.SpliceLengthLowerWeb;
						colSplice.FillerWidthWebLower = colSplice.SpliceWidthWeb;
						colSplice.FillerWeldLengthLW = colSplice.FillerLengthWebLower - ConstNum.TWO_INCHES;
					}
				}
				if (colSplice.ConnectionUpper == EConnectionStyle.Bolted)
				{
					colSplice.FillerLengthWebUpper = colSplice.SpliceLengthUpperWeb;
					colSplice.FillerWidthWebUpper = colSplice.SpliceWidthWeb;
				}
				else
				{
					if (colSplice.FillerThicknessWebUpper >= ConstNum.QUARTER_INCH)
					{
						colSplice.FillerLengthWebUpper = colSplice.SpliceLengthUpperWeb + ConstNum.TWO_INCHES;
						colSplice.FillerWidthWebUpper = colSplice.SpliceWidthWeb + ConstNum.TWO_INCHES;
						colSplice.FillerWeldLengthUW = colSplice.FillerLengthWebUpper - ConstNum.TWO_INCHES;
					}
					else
					{
						colSplice.FillerLengthWebUpper = colSplice.SpliceLengthUpperWeb;
						colSplice.FillerWidthWebUpper = colSplice.SpliceWidthWeb;
						colSplice.FillerWeldLengthUW = colSplice.FillerLengthWebUpper - ConstNum.TWO_INCHES;
					}
				}
			}
			if (colSplice.ConnectionOption == ESpliceConnection.ButtPlate)
			{
				colSplice.FillerThicknessFlangeLower = 0;
				colSplice.FillerThicknessFlangeUpper = 0;
				colSplice.FillerThicknessWebLower = 0;
				colSplice.FillerThicknessWebUpper = 0;
			}
		}

		public static void WeldsUpperWeb( ref double d, ref double tw, ref double FWeldSizeChToCol, ref double FWeldSizeChToFiller, ref double FWeldSizeFillerToCol, ref double CSMinX, ref double CSMinY, ref double PLLength, ref double PLType, ref double Gage, ref double NumPLorCH, ref double Fdesign, double Fw, ref double usefulweldonPlate, double F_Horizontal, double F_Vertical, ref double w_yf )
		{
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			if (colSplice.ConnectionOption == ESpliceConnection.DirectlyWelded)
			{
				d = colSplice.Channel.d;
				tw = colSplice.Channel.tw;
				FWeldSizeChToCol = CommonCalculations.MinimumWeld(tColumn.Shape.tw, d);
				FWeldSizeChToFiller = CommonCalculations.MinimumWeld(colSplice.FillerThicknessWebUpper, tw);
				FWeldSizeFillerToCol = CommonCalculations.MinimumWeld(tColumn.Shape.tw, colSplice.FillerThicknessWebUpper);

				ColumnSpliceMisc.ColSpliceMinPLThickness(ref CSMinX, ref CSMinY, ref colSplice.WebPLWidth, ref PLLength, ref PLType, ref Gage, ref CommonDataStatic.Minth);
				tw = CommonDataStatic.Minth;
				if (colSplice.SpliceThicknessWeb < tw)
					colSplice.SpliceThicknessWeb = tw;
				colSplice.WeldLengthXUW = Math.Max(((int)Math.Ceiling(d)) / 4.0, CSMinX);
				colSplice.WeldLengthYUW = Math.Max(CSMinY, d);
				NumPLorCH = colSplice.NumberOfChannels;
				if (colSplice.ChannelType == ESpliceChannelType.Temporary)
					Fdesign = 0;
				else
					Fdesign = Math.Sqrt(Math.Pow(Fw, 2) + Math.Pow(colSplice.WebShear, 2));
				if (colSplice.WebShear > 0)
					colSplice.WeldLengthXUW = d / 2;
			}
			else
			{
				d = colSplice.SpliceWidthWeb;
				tw = colSplice.SpliceThicknessWeb;
				FWeldSizeChToCol = CommonCalculations.MinimumWeld(tColumn.Shape.tw, d);
				FWeldSizeChToFiller = CommonCalculations.MinimumWeld(colSplice.FillerThicknessWebUpper, tw);
				FWeldSizeFillerToCol = CommonCalculations.MinimumWeld(tColumn.Shape.tw, colSplice.FillerThicknessWebUpper);

				ColumnSpliceMisc.ColSpliceMinPLThickness(ref CSMinX, ref CSMinY, ref colSplice.WebPLWidth, ref PLLength, ref PLType, ref Gage, ref CommonDataStatic.Minth);
				tw = CommonDataStatic.Minth;
				if (colSplice.SpliceThicknessWeb < tw)
					colSplice.SpliceThicknessWeb = tw;
				colSplice.WeldLengthXUW = d / 2;
				colSplice.WeldLengthYUW = Math.Max(CSMinY, d);
				NumPLorCH = 2;
				Fdesign = colSplice.WebShear;
			}
			if (colSplice.FillerThicknessWebUpper == 0)
			{
				if (FWeldSizeChToCol > colSplice.FilletWeldSizeWebUpper)
					colSplice.FilletWeldSizeWebUpper = FWeldSizeChToCol;
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * tw, tColumn.Material.Fu * tColumn.Shape.tw / NumPLorCH) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebUpper)
					w_y = ColumnSpliceMisc.WeldLengthForShear(false, 0, d, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				else
					w_y = ColumnSpliceMisc.WeldLengthForShear(false, 0, d, colSplice.FilletWeldSizeWebUpper, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				if (w_y > colSplice.WeldLengthYUW)
					colSplice.WeldLengthYUW = NumberFun.ConvertFromFraction(w_y, 2);
				colSplice.WeldLengthYUW = NumberFun.ConvertFromFraction(colSplice.WeldLengthYUW, 2);
				colSplice.SpliceLengthUpperWeb = colSplice.WeldLengthYUW + ConstNum.TWO_INCHES;
				colSplice.FillerLengthWebUpper = 0;
				colSplice.FillerWeldLengthUW = 0;
			}
			else if (colSplice.FillerThicknessWebUpper < ConstNum.QUARTER_INCH)
			{
				if (FWeldSizeChToCol > colSplice.FilletWeldSizeWebUpper)
					colSplice.FilletWeldSizeWebUpper = FWeldSizeChToCol;
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * tw, tColumn.Material.Fu * tColumn.Shape.tw / NumPLorCH) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebUpper - colSplice.FillerThicknessWebUpper)
					w_y = ColumnSpliceMisc.WeldLengthForShear(false, 0, d, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				else
					w_y = ColumnSpliceMisc.WeldLengthForShear(false, 0, d, (colSplice.FilletWeldSizeWebUpper - colSplice.FillerThicknessWebUpper), F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				if (w_y > colSplice.WeldLengthYUW)
					colSplice.WeldLengthYUW = w_y;
				
				colSplice.WeldLengthYUW = NumberFun.ConvertFromFraction(colSplice.WeldLengthYUW, 2);
				colSplice.SpliceLengthUpperWeb = colSplice.WeldLengthYUW + ConstNum.TWO_INCHES;
				colSplice.FillerLengthWebUpper = colSplice.SpliceLengthUpperWeb;
				colSplice.FillerWeldLengthUW = 0;
			}
			else
			{
				if (FWeldSizeChToFiller > colSplice.FilletWeldSizeWebUpper)
					colSplice.FilletWeldSizeWebUpper = FWeldSizeChToFiller;
				usefulweldonPlate = colSplice.Material.Fu * Math.Min(tw, colSplice.FillerThicknessWebUpper) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebUpper)
					w_y = ColumnSpliceMisc.WeldLengthForShear(false, 0, d, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");
				else
					w_y = ColumnSpliceMisc.WeldLengthForShear(false, 0, d, colSplice.FilletWeldSizeWebUpper, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "C");

				if (w_y > colSplice.WeldLengthYUW)
					colSplice.WeldLengthYUW = w_y;
				colSplice.WeldLengthYUW = NumberFun.ConvertFromFraction(colSplice.WeldLengthYUW, 2);
				colSplice.SpliceLengthUpperWeb = colSplice.WeldLengthYUW + ConstNum.TWO_INCHES;
				if (FWeldSizeFillerToCol > colSplice.FillerWeldSizeUW)
					colSplice.FillerWeldSizeUW = FWeldSizeFillerToCol;
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * colSplice.FillerThicknessWebUpper, tColumn.Material.Fu * tColumn.Shape.tw / NumPLorCH) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				if (usefulweldonPlate < colSplice.FillerWeldSizeUW)
					w_yf = ColumnSpliceMisc.WeldLengthForShear(false, 0, colSplice.FillerWidthWebUpper, usefulweldonPlate, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "II");
				else
					w_yf = ColumnSpliceMisc.WeldLengthForShear(false, 0, d, colSplice.FilletWeldSizeWebUpper, F_Horizontal / NumPLorCH, F_Vertical / NumPLorCH, 2, "II");

				if (w_yf > colSplice.FillerWeldLengthUW)
					colSplice.FillerWeldLengthUW = w_yf;
				colSplice.FillerWeldLengthUW = NumberFun.ConvertFromFraction(colSplice.FillerWeldLengthUW, 2);
				colSplice.FillerWeldLengthUW = Math.Max(colSplice.FillerWeldLengthUW, colSplice.WeldLengthYUW + ConstNum.ONE_INCH);
				colSplice.FillerLengthWebUpper = colSplice.FillerWeldLengthUW + ConstNum.TWO_INCHES;
			}
		}

		private static void FlangePlateDesign(ref bool Type1, ref double CSMinX, ref double CSMinY, ref double PLLength, ref double PLType, ref double Gage, bool FullContact, double AcontactwithFiller, double Acontact, double Fsplice, double FfillerLF, ref double reduction, double FspliceT, ref double FWeldsize, ref double usefulweldonPlate, double FspliceC, double FfillerC, double FfillerUF, ref double capacity)
		{
			bool Type2 = false;
			double e = 0.0;
			double s = 0.0;
			double ce = 0.0;
			double FtoDevFillBearing = 0.0;
			double NbFillDevelopBr = 0;
			double FtoDevFillSplice = 0.0;
			double NbFillDevelopSpl = 0;
			double NB = 0;
			double NbFiller = 0;
			double NbFillerD = 0.0;
			int nl = 0;
			double Glf = 0.0;
			double ehf = 0.0;
			double wfl = 0.0;
			double FspliceTseismic = 0.0;
			double Fillerweldsize = 0.0;
			double DeltaFillerWidthLF = 0.0;
			double maxwidthFL = 0.0;
			double MaxWidthFu = 0.0;
			double maxwidth = 0.0;
			double weldlength = 0.0;
			double usefulweldonFiller = 0.0;
			double fillerweldlength = 0.0;
			double LengthX = 0.0;
			double LengthY = 0.0;
			double Beta = 0.0;
			double A_req = 0.0;
			double Lreq = 0.0;
			int Nu = 0;
			double Guf = 0.0;
			double wfu = 0.0;
			double DeltaFillerWidthUF = 0.0;
			double Lgt = 0.0;
			double Lnt = 0.0;
			int np = 0;
			int n = 0;
			double Lgv = 0.0;
			double Lnv = 0.0;
			double CapacityBS = 0.0;
			double Fbre = 0.0;
			double Fbrs = 0.0;
			double CapacityBr = 0.0;
			double CapacityG = 0.0;
			double An = 0.0;
			double Ae = 0.0;
			double CapacityN = 0.0;
			double U = 0.0;
			double BuckLength = 0.0;
			double tbuckling = 0.0;
			double k = 0.0;
			double rg = 0.0;
			double kL_r = 0.0;
			double Fcr = 0.0;
			double FiRn = 0.0;
			Type2 = true;
			Type1 = false;

			double dumyweldsize;
			double dumylength;
			double plateWidth = 0;
			double minth = 0;

			var colSplice = CommonDataStatic.ColumnSplice;
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

			if (bColumn.Shape.d > 14.7 * ConstNum.ONE_INCH && bColumn.Shape.a > 38.8 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d > 14.7 * ConstNum.ONE_INCH && bColumn.Shape.a > 38.8 * Math.Pow(ConstNum.ONE_INCH, 2))
			{
				Type2 = false;
				Type1 = true;
			}

			MinimumPlateTickness(ref CSMinX, ref CSMinY, ref PLLength, ref PLType, ref Gage);
			//   MinimumWeldLength CSMinX, CSMinY
			if (colSplice.ConnectionLower == EConnectionStyle.Bolted && colSplice.ConnectionUpper == EConnectionStyle.Bolted)
				ColSpliceMinPLThickness("AllBolted", ref CSMinX, ref CSMinY, ref plateWidth, ref PLLength, ref PLType, ref Gage, ref minth);
			if (colSplice.ConnectionLower == EConnectionStyle.Bolted)
			{
				e = colSplice.BoltVertEdgeDistancePlate;
				s = colSplice.BoltVertSpacing;
				ce = colSplice.BoltVertEdgeDistanceColumn;
				
				if (e == 0)
					e = Math.Max(colSplice.Bolt.MinEdgeSheared, ConstNum.ONEANDHALF_INCHES);
				if (ce == 0)
					ce = Math.Max(colSplice.Bolt.MinEdgeSheared, ConstNum.ONEANDHALF_INCHES);
				if (s == 0)
					s = Math.Max(2.66 * colSplice.Bolt.BoltSize, ConstNum.THREE_INCHES);
				
				colSplice.BoltVertEdgeDistancePlate = e;
				colSplice.BoltVertSpacing = s;
				colSplice.BoltVertEdgeDistanceColumn = ce;
				// Develop Fillers for Bearing If Necessary
				if (!FullContact && colSplice.FillerThicknessFlangeLower > 0)
				{
					FtoDevFillBearing = ConstNum.FIOMEGA0_75N * 1.8F * colSplice.Material.Fy * (AcontactwithFiller - Acontact) / 2;
					NbFillDevelopBr = FtoDevFillBearing / colSplice.Bolt.BoltStrength;
				}
				else
					NbFillDevelopBr = 0;
				FtoDevFillSplice = Fsplice * colSplice.FillerThicknessFlangeLower / (bColumn.Shape.tf + colSplice.FillerThicknessFlangeLower);
				if (colSplice.FillerThicknessFlangeLower > 0.75 * ConstNum.ONE_INCH && Fsplice > 0)
					NbFillDevelopSpl = FtoDevFillSplice / colSplice.Bolt.BoltStrength;
				else
					NbFillDevelopSpl = 0;

				if (colSplice.Bolt.BoltType == EBoltType.SC || colSplice.FillerThicknessFlangeLower <= ConstNum.QUARTER_INCH)
				{
					NB = Math.Max(((int)Math.Ceiling(0.5 * Fsplice / colSplice.Bolt.BoltStrength) * 2), 4);
					if (colSplice.FillerThicknessFlangeLower > 0)
						NbFiller = Math.Max(((int)Math.Ceiling(0.5 * (FfillerLF + Fsplice) / colSplice.Bolt.BoltStrength) * 2 - NB), 0);
					else
						NbFiller = 0;
				}
				else if (colSplice.FillerThicknessFlangeLower <= 0.75 * ConstNum.ONE_INCH)
				{
					if (CommonDataStatic.Units == EUnit.US)
						reduction = 0.4 * (colSplice.FillerThicknessFlangeLower - 0.25);
					else
						reduction = 0.0154 * (colSplice.FillerThicknessFlangeLower - 6);

					NB = Math.Max((Math.Ceiling(0.5 * Fsplice / (colSplice.Bolt.BoltStrength * (1 - reduction))) * 2), 4);
					NbFiller = Math.Max((Math.Ceiling(0.5 * (FfillerLF + Fsplice) / (colSplice.Bolt.BoltStrength * (1 - reduction))) * 2 - NB), 0);
				}
				else
				{
					NB = Math.Max(((int)Math.Ceiling(0.5 * Fsplice / colSplice.Bolt.BoltStrength) * 2), 4);
					NbFiller = Math.Max(((int)Math.Ceiling(0.5 * (FfillerLF + Fsplice) / colSplice.Bolt.BoltStrength) * 2 - NB), 0);
				}

				if (Type1)
					colSplice.BoltRowsFlangeLower = (int)Math.Max((NB / 2), 3);
				else
					colSplice.BoltRowsFlangeLower = (int)Math.Max((NB / 2), 2);
				
				NbFillDevelopBr = Math.Max((NbFillDevelopBr - colSplice.BoltRowsFlangeUpper * 2), 0);
				NbFillerD = ((int)Math.Ceiling(0.5 * Math.Max((NbFillDevelopBr), (NbFillDevelopSpl)))) * 2;
				NbFiller = Math.Max((NbFillerD), NbFiller);

				colSplice.FillerNumBoltRowsLF = (int)(NbFiller / 2);
				nl = colSplice.BoltRowsFlangeLower;
				colSplice.SpliceLengthLowerFlange = (e + (nl - 1) * s + ce);
				colSplice.FillerLengthFlangeLower = (e + (nl - 1) * s + ce + Math.Sign(NbFiller) * (e + (NbFiller / 2 - 1) * s + e));

				Glf = colSplice.BoltGageFlangeLower;
				ehf = colSplice.BoltHorizEdgeDistanceFlange;
				if (Glf == 0)
				{
					Glf = Math.Max(bColumn.Shape.g1, (Math.Floor(2 * 0.4 * bColumn.Shape.bf)) / 2);
					colSplice.BoltGageFlangeLower = (Glf);
				}
				if (ehf == 0)
				{
					ehf = Math.Max((colSplice.Bolt.MinEdgeSheared), ConstNum.ONEANDHALF_INCHES);
					colSplice.BoltHorizEdgeDistanceFlange = ehf;
				}
				wfl = Glf + 2 * ehf;
			}
			else
			{
				if (colSplice.UseSeismic && FspliceT > 0)
				{
					FspliceTseismic = Math.Max((FspliceT), FlangeTensionforSeismic);
					if (colSplice.SMF)
						FspliceTseismic = Math.Max((FspliceTseismic), FfToDevMoment);
				}
				else
					FspliceTseismic = FspliceT;
				
				FWeldsize = CommonCalculations.MinimumWeld(bColumn.Shape.tf, colSplice.SpliceThicknessFlange);
				if (colSplice.FillerThicknessFlangeLower < ConstNum.QUARTER_INCH)
				{
					colSplice.FilletWeldSizeFlangeLower = (FWeldsize + colSplice.FillerThicknessFlangeLower);
					Fillerweldsize = 0;
					DeltaFillerWidthLF = 0;
				}
				else
				{
					colSplice.FilletWeldSizeFlangeLower = CommonCalculations.MinimumWeld(colSplice.FillerThicknessFlangeLower, colSplice.SpliceThicknessFlange);
					Fillerweldsize = CommonCalculations.MinimumWeld(bColumn.Shape.tf, colSplice.FillerThicknessFlangeLower);
					DeltaFillerWidthLF = colSplice.FilletWeldSizeFlangeLower + ConstNum.SIXTEENTH_INCH;
				}
				colSplice.FillerWeldSizeLF = Fillerweldsize;
				maxwidthFL = ((int)Math.Floor(bColumn.Shape.bf - colSplice.FilletWeldSizeFlangeLower - Fillerweldsize - ConstNum.EIGHTH_INCH));
				MaxWidthFu = ((int)Math.Floor(tColumn.Shape.bf - colSplice.FilletWeldSizeFlangeUpper - Fillerweldsize - ConstNum.EIGHTH_INCH));
				maxwidth = Math.Min((maxwidthFL), (MaxWidthFu)) - ConstNum.ONE_INCH;
				// weld length
				do
				{
					if (colSplice.FillerThicknessFlangeLower < ConstNum.QUARTER_INCH)
					{
						usefulweldonPlate = (Math.Min(colSplice.Material.Fu * colSplice.SpliceThicknessFlange, bColumn.Material.Fu * bColumn.Shape.tf) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx));
						weldlength = Math.Max((FspliceC + FfillerC), (FspliceTseismic)) / (ConstNum.FIOMEGA0_75N * 0.4242F * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min((FWeldsize), (usefulweldonPlate)));
					}
					else
					{
						usefulweldonPlate = colSplice.Material.Fu * Math.Min(colSplice.FillerThicknessFlangeLower, colSplice.SpliceThicknessFlange) / (0.707F * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						usefulweldonFiller = (Math.Min(colSplice.Material.Fu * colSplice.FillerThicknessFlangeLower, bColumn.Material.Fu * bColumn.Shape.tf) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx));
						weldlength = Fsplice / (ConstNum.FIOMEGA0_75N * 0.4242F * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min((FWeldsize), (usefulweldonPlate)));
						fillerweldlength = Math.Max((FspliceC + FfillerC), (FspliceTseismic)) / (ConstNum.FIOMEGA0_75N * 0.4242F * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min(colSplice.FillerWeldSizeLF, (usefulweldonFiller)));
					}
					LengthX = NumberFun.Round(colSplice.SpliceWidthFlange / 3, 1) + ConstNum.ONE_INCH;
					LengthY = NumberFun.Round(Math.Max((weldlength / 2 - LengthX), maxwidth), 1);
					LengthY = Math.Max((LengthY), CSMinY);
					colSplice.SpliceLengthLowerFlange = (LengthY + ConstNum.TWO_INCHES);
					dumyweldsize = (FWeldsize);
					dumylength = (LengthY);
					Beta = CommonCalculations.WeldBetaFactor(dumylength, dumyweldsize);
					if (Beta < 1)
						FWeldsize = FWeldsize + ConstNum.SIXTEENTH_INCH;
				} while (Beta < 1);
				
				if (colSplice.FillerThicknessFlangeLower < ConstNum.QUARTER_INCH)
					colSplice.FilletWeldSizeFlangeLower = (FWeldsize + colSplice.FillerThicknessFlangeLower);
				else
					colSplice.FilletWeldSizeFlangeLower = (FWeldsize);
				
				A_req = fillerweldlength * colSplice.FillerWeldSizeLF / 2;
				Lreq = (1.2 * colSplice.FillerWeldSizeLF - Math.Pow(1.44 * Math.Pow(colSplice.FillerWeldSizeLF, 2) - 4 * A_req * 0.002, 0.5)) / 0.004;
				fillerweldlength = 2 * Lreq;

				if (colSplice.FillerThicknessFlangeLower < ConstNum.QUARTER_INCH)
					colSplice.FillerLengthFlangeLower = (((int)Math.Ceiling(LengthY)));
				else
					colSplice.FillerLengthFlangeLower = (((int)Math.Ceiling(Math.Max((fillerweldlength / 2 + ConstNum.TWO_INCHES), (LengthY + ConstNum.THREE_INCHES)))));
				
				colSplice.WeldLengthXLF = LengthX;
				colSplice.WeldLengthYLF = LengthY;
				colSplice.FillerWeldLengthLF = colSplice.FillerLengthFlangeLower - ConstNum.TWO_INCHES;
			}

			if (colSplice.ConnectionUpper == EConnectionStyle.Bolted)
			{
				e = colSplice.BoltVertEdgeDistancePlate;
				s = colSplice.BoltVertSpacing;
				ce = colSplice.BoltVertEdgeDistanceColumn;
				if (e == 0)
					e = Math.Max((colSplice.Bolt.MinEdgeSheared), ConstNum.ONEANDHALF_INCHES);
				if (ce == 0)
					ce = Math.Max((colSplice.Bolt.MinEdgeSheared), ConstNum.ONEANDHALF_INCHES);
				if (s == 0)
					s = Math.Max(2.66F * colSplice.Bolt.BoltSize, ConstNum.THREE_INCHES);

				colSplice.BoltVertEdgeDistancePlate = (e);
				colSplice.BoltVertSpacing = s;
				colSplice.BoltVertEdgeDistanceColumn = (ce);

				if (!FullContact && colSplice.FillerThicknessFlangeUpper > 0)
				{
					FtoDevFillBearing = ConstNum.FIOMEGA0_75N * 1.8F * colSplice.Material.Fy * (AcontactwithFiller - Acontact) / 2;
					NbFillDevelopBr = FtoDevFillBearing / colSplice.Bolt.BoltStrength;
				}
				else
					NbFillDevelopBr = 0;
				
				FtoDevFillSplice = Fsplice * colSplice.FillerThicknessFlangeUpper / (tColumn.Shape.tf + colSplice.FillerThicknessFlangeUpper);
				if (colSplice.FillerThicknessFlangeUpper > 0.75 * ConstNum.ONE_INCH && Fsplice > 0)
					NbFillDevelopSpl = FtoDevFillSplice / colSplice.Bolt.BoltStrength;
				else
					NbFillDevelopSpl = 0;

				if (colSplice.Bolt.BoltType == EBoltType.SC || colSplice.FillerThicknessFlangeUpper <= ConstNum.QUARTER_INCH)
				{
					NB = Math.Max((Math.Ceiling(0.5 * Fsplice / colSplice.Bolt.BoltStrength) * 2), 4);
					if (colSplice.FillerThicknessFlangeUpper > 0)
						NbFiller = Math.Max((((int)Math.Ceiling(0.5 * (FfillerUF + Fsplice) / colSplice.Bolt.BoltStrength)) * 2 - NB), 0);
					else
						NbFiller = 0;
				}
				else if (colSplice.FillerThicknessFlangeUpper <= 0.75 * ConstNum.ONE_INCH)
				{
					if (CommonDataStatic.Units == EUnit.US)
						reduction = 0.4 * (colSplice.FillerThicknessFlangeUpper - 0.25);
					else
						reduction = 0.0154 * (colSplice.FillerThicknessFlangeUpper - 6);
					
					NB = Math.Max((Math.Ceiling(0.5 * Fsplice / (colSplice.Bolt.BoltStrength * (1 - reduction))) * 2), 4);
					NbFiller = Math.Max((Math.Ceiling(0.5 * (FfillerUF + Fsplice) / (colSplice.Bolt.BoltStrength * (1 - reduction))) * 2 - NB), 0);
				}
				else
				{
					NB = Math.Max((Math.Ceiling(0.5 * Fsplice / colSplice.Bolt.BoltStrength) * 2), 4);
					NbFiller = Math.Max((Math.Ceiling(0.5 * (FfillerUF + Fsplice) / colSplice.Bolt.BoltStrength) * 2 - NB), 0);
				}
				
				if (Type1)
					colSplice.BoltRowsFlangeUpper = (int)Math.Max((NB / 2), 3);
				else
					colSplice.BoltRowsFlangeUpper = (int)Math.Max((NB / 2), 2);

				NbFillDevelopBr = Math.Max((NbFillDevelopBr - colSplice.BoltRowsFlangeUpper * 2), 0);
				NbFillerD = Math.Ceiling(0.5 * Math.Max(NbFillDevelopBr, NbFillDevelopSpl)) * 2;
				NbFiller = Math.Max((NbFillerD), NbFiller);

				colSplice.FillerNumBoltRowsUF = (int)(NbFiller / 2);

				Nu = colSplice.BoltRowsFlangeUpper;
				colSplice.SpliceLengthUpperFlange = (e + (Nu - 1) * s + ce);
				colSplice.FillerLengthFlangeUpper = (e + (Nu - 1) * s + ce + Math.Sign(NbFiller) * (e + (NbFiller / 2 - 1) * s + e));
				// width
				Guf = colSplice.BoltGageFlangeUpper;
				ehf = colSplice.BoltHorizEdgeDistanceFlange;
				if (Guf == 0)
				{
					Guf = Math.Max(tColumn.Shape.g1, Math.Floor(2 * 0.4 * tColumn.Shape.bf) / 2);
					colSplice.BoltGageFlangeUpper = Guf;
				}
				if (ehf == 0)
				{
					ehf = Math.Max((colSplice.Bolt.MinEdgeSheared), ConstNum.ONEANDHALF_INCHES);
					colSplice.BoltHorizEdgeDistanceFlange = ehf;
				}
				wfu = Guf + 2 * ehf;
			}
			else
			{
				// UC welded
				if (colSplice.UseSeismic && FspliceT > 0)
				{
					FspliceTseismic = Math.Max((FspliceT), FlangeTensionforSeismic);
					if (colSplice.SMF)
						FspliceTseismic = Math.Max((FspliceTseismic), FfToDevMoment);
				}
				else
					FspliceTseismic = FspliceT;

				FWeldsize = CommonCalculations.MinimumWeld(tColumn.Shape.tf, colSplice.SpliceThicknessFlange);
				if (colSplice.FillerThicknessFlangeUpper < ConstNum.QUARTER_INCH)
				{
					colSplice.FilletWeldSizeFlangeUpper = (FWeldsize + colSplice.FillerThicknessFlangeUpper);
					Fillerweldsize = 0;
					DeltaFillerWidthUF = 0;
				}
				else
				{
					colSplice.FilletWeldSizeFlangeUpper = CommonCalculations.MinimumWeld(colSplice.FillerThicknessFlangeUpper, colSplice.SpliceThicknessFlange);
					Fillerweldsize = CommonCalculations.MinimumWeld(tColumn.Shape.tf, colSplice.FillerThicknessFlangeUpper);
					DeltaFillerWidthUF = colSplice.FilletWeldSizeFlangeUpper + ConstNum.SIXTEENTH_INCH;
				}
				colSplice.FillerWeldSizeUF = Fillerweldsize;
				maxwidthFL = ((int)Math.Floor(bColumn.Shape.bf - colSplice.FilletWeldSizeFlangeLower - Fillerweldsize - ConstNum.EIGHTH_INCH));
				MaxWidthFu = ((int)Math.Floor(tColumn.Shape.b - colSplice.FilletWeldSizeFlangeUpper - Fillerweldsize - ConstNum.EIGHTH_INCH));
				maxwidth = Math.Min((maxwidthFL), (MaxWidthFu)) - ConstNum.ONE_INCH;
				// weld length
				do
				{
					if (colSplice.FillerThicknessFlangeUpper < ConstNum.QUARTER_INCH)
					{
						usefulweldonPlate = (Math.Min(colSplice.Material.Fu * colSplice.SpliceThicknessFlange, bColumn.Material.Fu * bColumn.Shape.tf) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx));
						weldlength = Math.Max((FspliceC + FfillerC), (FspliceTseismic)) / (ConstNum.FIOMEGA0_75N * 0.4242F * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min((FWeldsize), (usefulweldonPlate)));
					}
					else
					{
						usefulweldonPlate = colSplice.Material.Fu * Math.Min(colSplice.FillerThicknessFlangeUpper, colSplice.SpliceThicknessFlange) / (0.707F * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						usefulweldonFiller = (Math.Min(colSplice.Material.Fu * colSplice.FillerThicknessFlangeUpper, tColumn.Material.Fu * tColumn.Shape.tf) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx));
						weldlength = Fsplice / (ConstNum.FIOMEGA0_75N * 0.4242F * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min((FWeldsize), (usefulweldonPlate)));
						fillerweldlength = Math.Max((FspliceC + FfillerC), (FspliceTseismic)) / (ConstNum.FIOMEGA0_75N * 0.4242F * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Math.Min(colSplice.FillerWeldSizeUF, (usefulweldonFiller)));
					}
					LengthX = NumberFun.Round(colSplice.SpliceWidthFlange / 3 + ConstNum.ONE_INCH, 1);
					LengthY = NumberFun.Round(Math.Max((weldlength / 2 - LengthX), maxwidth), 1);
					LengthY = Math.Max((LengthY), CSMinY);
					colSplice.SpliceLengthUpperFlange = (LengthY + ConstNum.TWO_INCHES);
					dumyweldsize = (FWeldsize);
					dumylength = (LengthY);
					Beta = CommonCalculations.WeldBetaFactor(dumylength, dumyweldsize);
					if (Beta < 1)
					{
						FWeldsize = FWeldsize + ConstNum.SIXTEENTH_INCH;
					}
				} while (!(Beta >= 1));

				A_req = fillerweldlength * colSplice.FillerWeldSizeUF / 2;
				Lreq = (1.2 * colSplice.FillerWeldSizeUF - Math.Pow(1.44 * Math.Pow(colSplice.FillerWeldSizeUF, 2) - 4 * A_req * 0.002, 0.5)) / 0.004;
				fillerweldlength = 2 * Lreq;
				colSplice.FilletWeldSizeFlangeUpper = (FWeldsize);

				if (colSplice.FillerThicknessFlangeUpper < ConstNum.QUARTER_INCH)
					colSplice.FillerLengthFlangeUpper = Math.Floor(LengthY);
				else
					colSplice.FillerLengthFlangeUpper = Math.Floor(Math.Max((fillerweldlength / 2 + ConstNum.TWO_INCHES), (LengthY + ConstNum.THREE_INCHES)));
				colSplice.WeldLengthXUF = (LengthX);
				colSplice.WeldLengthYUF = (LengthY);
				colSplice.FillerWeldLengthUF = colSplice.FillerLengthFlangeUpper - ConstNum.TWO_INCHES;
			}

			colSplice.FlangePLLength = colSplice.SpliceLengthLowerFlange + colSplice.SpliceLengthUpperFlange;
			if (colSplice.ConnectionUpper == EConnectionStyle.Bolted && colSplice.ConnectionLower == EConnectionStyle.Bolted)
			{
				colSplice.SpliceWidthFlange = Math.Max(wfu, (wfl));
				colSplice.FillerWidthFlangeLower = colSplice.SpliceWidthFlange;
				colSplice.FillerWidthFlangeUpper = colSplice.SpliceWidthFlange;
			}
			else if (colSplice.ConnectionUpper == EConnectionStyle.Bolted)
			{
				colSplice.SpliceWidthFlange = Math.Min(wfu, (maxwidthFL));
				colSplice.FillerWidthFlangeUpper = colSplice.SpliceWidthFlange;
				colSplice.FillerWidthFlangeLower = colSplice.SpliceWidthFlange + 2 * DeltaFillerWidthLF;
				if (colSplice.SpliceWidthFlange < wfu)
					colSplice.BoltGageFlangeUpper = (Math.Floor(4 * (colSplice.SpliceWidthFlange - 2 * colSplice.BoltHorizEdgeDistanceFlange)) / 4);
			}
			else if (colSplice.ConnectionLower == EConnectionStyle.Bolted)
			{
				colSplice.SpliceWidthFlange = Math.Min((wfl), (MaxWidthFu));
				colSplice.FillerWidthFlangeLower = colSplice.SpliceWidthFlange;
				colSplice.FillerWidthFlangeUpper = colSplice.SpliceWidthFlange + 2 * DeltaFillerWidthUF;
				if (colSplice.SpliceWidthFlange < wfl)
					colSplice.BoltGageFlangeLower = (Math.Floor(4 * (colSplice.SpliceWidthFlange - 2 * colSplice.BoltHorizEdgeDistanceFlange)) / 4);
			}
			else
			{
				colSplice.SpliceWidthFlange = Math.Min((maxwidthFL), (MaxWidthFu)) - ConstNum.ONE_INCH;
				colSplice.FillerWidthFlangeLower = colSplice.SpliceWidthFlange + 2 * DeltaFillerWidthLF;
				colSplice.FillerWidthFlangeUpper = colSplice.SpliceWidthFlange + 2 * DeltaFillerWidthUF;
			}
			// Tension Capacity:
			if (FspliceT > 0)
			{

				if (colSplice.ConnectionLower == EConnectionStyle.Bolted)
				{
					// Column Flange Block Shear
					Lgt = bColumn.Shape.bf - colSplice.BoltGageFlangeLower;
					Lnt = Lgt - (colSplice.HoleHorizontal + ConstNum.SIXTEENTH_INCH);
					np = colSplice.BoltRowsFlangeLower;
					n = colSplice.BoltRowsFlangeLower - 1;
					do
					{
						n = n + 1;
						Lgv = 2 * (colSplice.BoltVertEdgeDistanceColumn + (n - 1) * colSplice.BoltVertSpacing);
						Lnv = Lgv - 2 * (n - 0.5) * (colSplice.HoleVertical + ConstNum.SIXTEENTH_INCH);
						ColumnSpliceBlockShear(Lgv, Lnv, Lnt, bColumn.Shape.tf, bColumn.Material.Fy, bColumn.Material.Fu, ref CapacityBS, 0);
					} while (CapacityBS < FspliceT);
					colSplice.BoltRowsFlangeLower = n;
					// Bolt Bearing on Column Flange
					Fbre = CommonCalculations.EdgeBearing(colSplice.BoltVertEdgeDistanceColumn, colSplice.HoleVertical, colSplice.Bolt.BoltSize, bColumn.Material.Fu, colSplice.Bolt.HoleType, false);
					Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.HoleVertical, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, bColumn.Material.Fu, false);
					n = colSplice.BoltRowsFlangeLower - 1;
					do
					{
						n = n + 1;
						CapacityBr = 2 * (Fbre + (n - 1) * Fbrs) * bColumn.Shape.tf;
					} while (CapacityBr < FspliceT);
					colSplice.BoltRowsFlangeLower = n;
					if (colSplice.BoltRowsFlangeLower > np)
					{
						colSplice.SpliceLengthLowerFlange = colSplice.SpliceLengthLowerFlange + (colSplice.BoltRowsFlangeLower - np) * colSplice.BoltVertSpacing;
						colSplice.FillerLengthFlangeLower = colSplice.FillerLengthFlangeLower + (colSplice.BoltRowsFlangeLower - np) * colSplice.BoltVertSpacing;
						colSplice.FlangePLLength = colSplice.SpliceLengthUpperFlange + colSplice.SpliceLengthLowerFlange;
					}
					// Bearing Capacity on Plate
					Fbre = CommonCalculations.EdgeBearing(colSplice.BoltVertEdgeDistancePlate, colSplice.HoleVertical, colSplice.Bolt.BoltSize, colSplice.Material.Fu, colSplice.Bolt.HoleType, false);
					Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.HoleVertical, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, false);
					CapacityBr = 2 * (Fbre + (colSplice.BoltRowsFlangeLower - 1) * Fbrs) * colSplice.SpliceThicknessFlange;

					// Gross Tension Cap of Plate
					CapacityG = ConstNum.FIOMEGA0_9N * colSplice.Material.Fy * colSplice.SpliceWidthFlange * colSplice.SpliceThicknessFlange;
					// Net Tension
					An = (colSplice.SpliceWidthFlange - 2 * (colSplice.HoleHorizontal + ConstNum.SIXTEENTH_INCH)) * colSplice.SpliceThicknessFlange;
					Ae = Math.Min(An, 0.85F * colSplice.SpliceWidthFlange * colSplice.SpliceThicknessFlange);
					CapacityN = ConstNum.FIOMEGA0_75N * colSplice.Material.Fu * Ae;
					// Block Shear of PLate
					Lgt = Math.Min(colSplice.BoltGageFlangeLower, colSplice.SpliceWidthFlange - colSplice.BoltGageFlangeLower);
					Lnt = Lgt - (colSplice.HoleHorizontal + ConstNum.SIXTEENTH_INCH);
					Lgv = 2 * (colSplice.BoltVertEdgeDistancePlate + (colSplice.BoltRowsFlangeLower - 1) * colSplice.BoltVertSpacing);
					Lnv = Lgv - 2 * (colSplice.BoltRowsFlangeLower - 0.5) * (colSplice.HoleVertical + ConstNum.SIXTEENTH_INCH);
					ColumnSpliceBlockShear(Lgv, Lnv, Lnt, colSplice.SpliceThicknessFlange, colSplice.Material.Fy, colSplice.Material.Fu, ref CapacityBS, 0);

					capacity = Math.Min(Math.Min((CapacityBr), CapacityG), Math.Min((CapacityN), CapacityBS));
					if (capacity < FspliceT)
						colSplice.SpliceThicknessFlange = NumberFun.Round(colSplice.SpliceThicknessFlange * FspliceT / capacity, 8);
				}
				else
				{
					// welded
					// Gross Tension Cap of Plate
					CapacityG = ConstNum.FIOMEGA0_9N * colSplice.Material.Fy * colSplice.SpliceWidthFlange * colSplice.SpliceThicknessFlange;
					// Net Tension
					if (colSplice.WeldLengthYLF >= 2 * colSplice.SpliceWidthFlange)
						U = 1;
					else if (colSplice.WeldLengthYLF >= 1.5F * colSplice.SpliceWidthFlange)
						U = 0.87;
					else if (colSplice.WeldLengthYLF >= colSplice.SpliceWidthFlange)
						U = 0.75;
					else
					{
						U = 0.75;
						colSplice.WeldLengthYLF = colSplice.SpliceWidthFlange;
						colSplice.SpliceLengthLowerFlange = colSplice.WeldLengthYLF + ConstNum.TWO_INCHES;
						colSplice.FlangePLLength = colSplice.SpliceLengthLowerFlange + colSplice.SpliceLengthUpperFlange;
					}

					CapacityN = ConstNum.FIOMEGA0_75N * colSplice.Material.Fu * U * colSplice.SpliceWidthFlange * colSplice.SpliceThicknessFlange;
					capacity = Math.Min((CapacityN), CapacityG);
					if (capacity < FspliceTseismic)
						colSplice.SpliceThicknessFlange = NumberFun.Round((colSplice.SpliceThicknessFlange * FspliceTseismic / capacity), 8);
				}
				// Upper Column
				if (colSplice.ConnectionUpper == EConnectionStyle.Bolted)
				{
					// Column Flange Block Shear
					Lgt = tColumn.Shape.bf - colSplice.BoltGageFlangeUpper;
					Lnt = Lgt - (colSplice.HoleHorizontal + ConstNum.SIXTEENTH_INCH);
					np = colSplice.BoltRowsFlangeUpper;
					
					do
					{
						Lgv = 2 * (colSplice.BoltVertEdgeDistanceColumn + (n - 1) * colSplice.BoltVertSpacing);
						Lnv = Lgv - 2 * (n - 0.5) * (colSplice.HoleVertical + ConstNum.SIXTEENTH_INCH);
						ColumnSpliceBlockShear(Lgv, Lnv, Lnt, tColumn.Shape.tf, tColumn.Material.Fy, tColumn.Material.Fu, ref CapacityBS, 0);
					} while (CapacityBS < FspliceT);
					
					colSplice.BoltRowsFlangeUpper = n;
					// Bolt Bearing on Column Flange
					Fbre = CommonCalculations.EdgeBearing(colSplice.BoltVertEdgeDistanceColumn, ((int)colSplice.HoleVertical), colSplice.Bolt.BoltSize, tColumn.Material.Fu, colSplice.Bolt.HoleType, false);
					Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.HoleVertical, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, tColumn.Material.Fu, false);
					n = colSplice.BoltRowsFlangeUpper - 1;
					
					do
					{
						n++;
						CapacityBr = 2 * (Fbre + (n - 1) * Fbrs) * tColumn.Shape.tf;
					} while (CapacityBr < FspliceT);
					
					colSplice.BoltRowsFlangeUpper = n;
					if (colSplice.BoltRowsFlangeUpper > np)
					{
						colSplice.SpliceLengthUpperFlange = colSplice.SpliceLengthUpperFlange + (colSplice.BoltRowsFlangeUpper - np) * colSplice.BoltVertSpacing;
						colSplice.FillerLengthFlangeUpper = colSplice.FillerLengthFlangeUpper + (colSplice.BoltRowsFlangeUpper - np) * colSplice.BoltVertSpacing;
						colSplice.FlangePLLength = colSplice.SpliceLengthUpperFlange + colSplice.SpliceLengthLowerFlange;
					}

					// Bearing Capacity on Plate
					Fbre = CommonCalculations.EdgeBearing(colSplice.BoltVertEdgeDistancePlate, colSplice.HoleVertical, colSplice.Bolt.BoltSize, colSplice.Material.Fu, colSplice.Bolt.HoleType, false);
					Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.HoleVertical, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, false);
					CapacityBr = 2 * (Fbre + (colSplice.BoltRowsFlangeUpper - 1) * Fbrs) * colSplice.SpliceThicknessFlange;

					// Gross Tension Cap of Plate
					CapacityG = ConstNum.FIOMEGA0_9N * colSplice.Material.Fy * colSplice.SpliceWidthFlange * colSplice.SpliceThicknessFlange;
					// Net Tension
					An = (colSplice.SpliceWidthFlange - 2 * (colSplice.HoleHorizontal + ConstNum.SIXTEENTH_INCH)) * colSplice.SpliceThicknessFlange;
					Ae = Math.Min(An, 0.85F * colSplice.SpliceWidthFlange * colSplice.SpliceThicknessFlange);
					CapacityN = ConstNum.FIOMEGA0_75N * colSplice.Material.Fu * Ae;
					// Block Shear of PLate
					Lgt = Math.Min(colSplice.BoltGageFlangeUpper, colSplice.SpliceWidthFlange - colSplice.BoltGageFlangeUpper);
					Lnt = Lgt - (colSplice.HoleHorizontal + ConstNum.SIXTEENTH_INCH);
					Lgv = 2 * (colSplice.BoltVertEdgeDistancePlate + (colSplice.BoltRowsFlangeUpper - 1) * colSplice.BoltVertSpacing);
					Lnv = Lgv - 2 * (colSplice.BoltRowsFlangeUpper - 0.5) * (colSplice.HoleVertical + ConstNum.SIXTEENTH_INCH);
					ColumnSpliceBlockShear(Lgv, Lnv, Lnt, colSplice.SpliceThicknessFlange, colSplice.Material.Fy, colSplice.Material.Fu, ref CapacityBS, 0);

					capacity = Math.Min(Math.Min((CapacityBr), CapacityG), Math.Min((CapacityN), CapacityBS));
					if (capacity < FspliceT)
						colSplice.SpliceThicknessFlange = NumberFun.Round((colSplice.SpliceThicknessFlange * FspliceT / capacity), 8);
				}
				else
				{
					// welded
					CapacityG = ConstNum.FIOMEGA0_9N * colSplice.Material.Fy * colSplice.SpliceWidthFlange * colSplice.SpliceThicknessFlange;
					// Net Tension
					if (colSplice.WeldLengthYUF >= 2 * colSplice.SpliceWidthFlange)
						U = 1;
					else if (colSplice.WeldLengthYUF >= 1.5 * colSplice.SpliceWidthFlange)
						U = 0.87;
					else if (colSplice.WeldLengthYUF >= colSplice.SpliceWidthFlange)
						U = 0.75;
					else
					{
						U = 0.75;
						colSplice.WeldLengthYUF = colSplice.SpliceWidthFlange;
						colSplice.SpliceLengthUpperFlange = colSplice.WeldLengthYUF + ConstNum.TWO_INCHES;
						colSplice.FlangePLLength = colSplice.SpliceLengthUpperFlange + colSplice.SpliceLengthLowerFlange;
					}

					CapacityN = ConstNum.FIOMEGA0_75N * colSplice.Material.Fu * U * colSplice.SpliceWidthFlange * colSplice.SpliceThicknessFlange;
					capacity = Math.Min((CapacityN), CapacityG);
					if (capacity < FspliceTseismic)
						colSplice.SpliceThicknessFlange = NumberFun.Round((colSplice.SpliceThicknessFlange * FspliceT / capacity), 8);
				}
			}
			if (FspliceC > 0)
			{
				if (colSplice.ConnectionUpper == EConnectionStyle.Bolted)
				{

					// Bolt Bearing on Column Flange
					Fbrs = CommonCalculations.SpacingBearing(0, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, tColumn.Material.Fu, false);
					np = colSplice.BoltRowsFlangeUpper;
					n = colSplice.BoltRowsFlangeUpper - 1;
					do
					{
						n++;
						CapacityBr = 2 * n * Fbrs * tColumn.Shape.tf;
					} while (CapacityBr < FspliceT);
					colSplice.BoltRowsFlangeUpper = n;
					if (colSplice.BoltRowsFlangeUpper > np)
					{
						colSplice.SpliceLengthUpperFlange = colSplice.SpliceLengthUpperFlange + (colSplice.BoltRowsFlangeUpper - np) * colSplice.BoltVertSpacing;
						colSplice.FillerLengthFlangeUpper = colSplice.FillerLengthFlangeUpper + (colSplice.BoltRowsFlangeUpper - np) * colSplice.BoltVertSpacing;
						colSplice.FlangePLLength = colSplice.SpliceLengthUpperFlange + colSplice.SpliceLengthLowerFlange;
					}

					// Bearing Capacity on Plate
					Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.HoleVertical, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, false);
					CapacityBr = 2 * colSplice.BoltRowsFlangeUpper * Fbrs * colSplice.SpliceThicknessFlange;
					if (CapacityBr < FspliceC)
						colSplice.SpliceThicknessFlange = NumberFun.Round((colSplice.SpliceThicknessFlange * FspliceC / CapacityBr), 8);
					BuckLength = colSplice.BoltVertEdgeDistanceColumn;
				}
				else
					BuckLength = colSplice.SpliceLengthUpperFlange - colSplice.WeldLengthYUF;
				if (colSplice.ConnectionLower == EConnectionStyle.Bolted)
				{
					// Bolt Bearing on Column Flange
					Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.HoleVertical, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, bColumn.Material.Fu, false);
					np = colSplice.BoltRowsFlangeLower;
					n = colSplice.BoltRowsFlangeLower - 1;
					do
					{
						n++;
						CapacityBr = 2 * n * Fbrs * bColumn.Shape.tf;
					} while (CapacityBr < FspliceT);
					
					colSplice.BoltRowsFlangeLower = n;
					if (colSplice.BoltRowsFlangeLower > np)
					{
						colSplice.SpliceLengthLowerFlange = colSplice.SpliceLengthLowerFlange + (colSplice.BoltRowsFlangeLower - np) * colSplice.BoltVertSpacing;
						colSplice.FillerLengthFlangeLower = colSplice.FillerLengthFlangeLower + (colSplice.BoltRowsFlangeLower - np) * colSplice.BoltVertSpacing;
						colSplice.FlangePLLength = colSplice.SpliceLengthUpperFlange + colSplice.SpliceLengthLowerFlange;
					}
					// Bearing Capacity on Plate
					Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.HoleVertical, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, false);
					CapacityBr = 2 * colSplice.BoltRowsFlangeLower * Fbrs * colSplice.SpliceThicknessFlange;
					if (CapacityBr < FspliceC)
						colSplice.SpliceThicknessFlange = NumberFun.Round((colSplice.SpliceThicknessFlange * FspliceC / CapacityBr), 8);
					BuckLength = BuckLength + colSplice.BoltVertEdgeDistanceColumn;
				}
				else
					BuckLength = BuckLength + colSplice.SpliceLengthLowerFlange - colSplice.WeldLengthYLF;

				// PLate Buckling
				tbuckling = colSplice.SpliceThicknessFlange - ConstNum.SIXTEENTH_INCH;
				k = 0.65;
				do
				{
					tbuckling = tbuckling + ConstNum.SIXTEENTH_INCH;
					rg = tbuckling / Math.Sqrt(12);
					kL_r = k * BuckLength / rg;

					Fcr = CommonCalculations.BucklingStress(kL_r, colSplice.Material.Fy, false);

					FiRn = ConstNum.FIOMEGA0_9N * Fcr * colSplice.SpliceWidthFlange * tbuckling;
				} while (FiRn < FspliceC);
				
				if (colSplice.SpliceThicknessFlange < tbuckling)
					colSplice.SpliceThicknessFlange = tbuckling;
				colSplice.SpliceThicknessFlange = NumberFun.Round(colSplice.SpliceThicknessFlange, 8);
			}
		}

		private static void ColumnSpliceBlockShear(double Lgv, double Lnv, double Lnt, double t, double Fy, double Fu, ref double CapacityBS, double V)
		{
			CapacityBS = MiscCalculationsWithReporting.BlockShearNew(Fu, Lnv, 1, Lnt, Lgv, Fy, t, V, false);
		}

		private static void MinimumPlateTickness(ref double CSMinX, ref double CSMinY, ref double PLLength, ref double PLType, ref double Gage)
		{
			var colSplice = CommonDataStatic.ColumnSplice;
			double minth = 0;
			double plateWidth = 0;

			// Thickness
			if (colSplice.ConnectionLower == EConnectionStyle.Bolted && colSplice.ConnectionUpper == EConnectionStyle.Bolted)
			{
				ColSpliceMinPLThickness("AllBolted", ref CSMinX, ref CSMinY, ref plateWidth, ref PLLength, ref PLType, ref Gage, ref minth);
				if (colSplice.SpliceThicknessFlange < minth)
					colSplice.SpliceThicknessFlange = minth;
			}
			else if (colSplice.ConnectionLower == EConnectionStyle.Welded && colSplice.ConnectionUpper == EConnectionStyle.Welded)
			{
				ColSpliceMinPLThickness("AllWelded", ref CSMinX, ref CSMinY, ref plateWidth, ref PLLength, ref PLType, ref Gage, ref minth);
				if (colSplice.SpliceThicknessFlange < minth)
					colSplice.SpliceThicknessFlange = minth;
			}
			else if (colSplice.ConnectionLower == EConnectionStyle.Welded)
			{
				ColSpliceMinPLThickness("LCWeldedUCBolted", ref CSMinX, ref CSMinY, ref plateWidth, ref PLLength, ref PLType, ref Gage, ref minth);
				if (colSplice.SpliceThicknessFlange < minth)
					colSplice.SpliceThicknessFlange = minth;
			}
			else if (colSplice.ConnectionUpper == EConnectionStyle.Welded)
			{
				ColSpliceMinPLThickness("UCWeldedLCBolted", ref CSMinX, ref CSMinY, ref plateWidth, ref PLLength, ref PLType, ref Gage, ref minth);
				if (colSplice.SpliceThicknessFlange < minth)
					colSplice.SpliceThicknessFlange = minth;
			}
		}

		private static void ColSpliceMinPLThickness(string BoltedOrWelded, ref double CSMinX, ref double CSMinY, ref double PlWidth, ref double PLLength, ref double PLType, ref double Gage, ref double Minth)
		{
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

			switch (BoltedOrWelded)
			{
				case "AllBolted":
					if (tColumn.Shape.d >= 19 * ConstNum.ONE_INCH && tColumn.Shape.a >= 134 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 19 * ConstNum.ONE_INCH && bColumn.Shape.a >= 134 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.75 * ConstNum.ONE_INCH;     // >=W14X455
						Gage = 13.5 * ConstNum.ONE_INCH;
						PLType = 1 * ConstNum.ONE_INCH;
						PlWidth = 16 * ConstNum.ONE_INCH;
						PLLength = 18.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 16.4 * ConstNum.ONE_INCH && tColumn.Shape.a >= 75.6 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 16.4 * ConstNum.ONE_INCH && bColumn.Shape.a >= 75.6 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.625 * ConstNum.ONE_INCH;     // >=W14X257
						Gage = 11.5 * ConstNum.ONE_INCH;
						PLType = 1 * ConstNum.ONE_INCH;
						PlWidth = 16 * ConstNum.ONE_INCH;
						PLLength = 18.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 14.8 * ConstNum.ONE_INCH && tColumn.Shape.a >= 42.7 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 14.8 * ConstNum.ONE_INCH && bColumn.Shape.a >= 42.7 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.5 * ConstNum.ONE_INCH;     // >=W14X145
						Gage = 13.5 * ConstNum.ONE_INCH;
						PLType = 1 * ConstNum.ONE_INCH;
						PlWidth = 16 * ConstNum.ONE_INCH;
						PLLength = 18.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 14 * ConstNum.ONE_INCH && tColumn.Shape.a >= 26.5 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 14 * ConstNum.ONE_INCH && bColumn.Shape.a >= 26.5 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W14X90
						Gage = 13.5 * ConstNum.ONE_INCH;
						PLType = 1 * ConstNum.ONE_INCH;
						PlWidth = 16 * ConstNum.ONE_INCH;
						PLLength = 18.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 13.7 * ConstNum.ONE_INCH && tColumn.Shape.a >= 12.6 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.7 * ConstNum.ONE_INCH && bColumn.Shape.a >= 12.6 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W14X43
						Gage = 13.5 * ConstNum.ONE_INCH;
						PLType = 1 * ConstNum.ONE_INCH;
						PlWidth = 16 * ConstNum.ONE_INCH;
						PLLength = 18.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 13.1 * ConstNum.ONE_INCH && tColumn.Shape.a >= 35.3 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.1 * ConstNum.ONE_INCH && bColumn.Shape.a >= 35.3 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.625 * ConstNum.ONE_INCH;     // >=W12X120
						Gage = 13.5 * ConstNum.ONE_INCH;
						PLType = 1 * ConstNum.ONE_INCH;
						PlWidth = 16 * ConstNum.ONE_INCH;
						PLLength = 18.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 11.9 * ConstNum.ONE_INCH && tColumn.Shape.a >= 11.7 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 11.9 * ConstNum.ONE_INCH && bColumn.Shape.a >= 11.7 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W12X40
						Gage = 13.5 * ConstNum.ONE_INCH;
						PLType = 1 * ConstNum.ONE_INCH;
						PlWidth = 16 * ConstNum.ONE_INCH;
						PLLength = 18.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 9.73 * ConstNum.ONE_INCH && tColumn.Shape.a >= 9.71 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 9.73 * ConstNum.ONE_INCH && bColumn.Shape.a >= 9.71 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W10X33
						Gage = 13.5 * ConstNum.ONE_INCH;
						PLType = 1 * ConstNum.ONE_INCH;
						PlWidth = 16 * ConstNum.ONE_INCH;
						PLLength = 18.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 8 * ConstNum.ONE_INCH && tColumn.Shape.a >= 9.12 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 8 * ConstNum.ONE_INCH && bColumn.Shape.a >= 9.12 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W8X31
						Gage = 13.5 * ConstNum.ONE_INCH;
						PLType = 1 * ConstNum.ONE_INCH;
						PlWidth = 16 * ConstNum.ONE_INCH;
						PLLength = 18.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 7.93 * ConstNum.ONE_INCH && tColumn.Shape.a >= 7.08 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 7.93 * ConstNum.ONE_INCH && bColumn.Shape.a >= 7.08 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W8X24
						Gage = 13.5 * ConstNum.ONE_INCH;
						PLType = 1 * ConstNum.ONE_INCH;
						PlWidth = 16 * ConstNum.ONE_INCH;
						PLLength = 18.5 * ConstNum.ONE_INCH;
					}
					else
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     //  All Others
						Gage = 13.5 * ConstNum.ONE_INCH;
						PLType = 1 * ConstNum.ONE_INCH;
						PlWidth = 16 * ConstNum.ONE_INCH;
						PLLength = 18.5 * ConstNum.ONE_INCH;
					}
					break;
				case "AllWelded":
					if (tColumn.Shape.d >= 19 * ConstNum.ONE_INCH && tColumn.Shape.a >= 134 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 19 * ConstNum.ONE_INCH && bColumn.Shape.a >= 134 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.625 * ConstNum.ONE_INCH;     // >=W14X455
						CSMinX = 5 * ConstNum.ONE_INCH;
						CSMinY = 7 * ConstNum.ONE_INCH;
						PlWidth = 14 * ConstNum.ONE_INCH;
						PLLength = 18 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 17.1 * ConstNum.ONE_INCH && tColumn.Shape.a >= 91.4 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 17.1 * ConstNum.ONE_INCH && bColumn.Shape.a >= 91.4 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.625 * ConstNum.ONE_INCH;     // >=W14X311
						CSMinX = 4 * ConstNum.ONE_INCH;
						CSMinY = 6 * ConstNum.ONE_INCH;
						PlWidth = 12 * ConstNum.ONE_INCH;
						PLLength = 16 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 15.7 * ConstNum.ONE_INCH && tColumn.Shape.a >= 62 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 15.7 * ConstNum.ONE_INCH && bColumn.Shape.a >= 62 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.5F * ConstNum.ONE_INCH;     // >=W14X211
						CSMinX = 4 * ConstNum.ONE_INCH;
						CSMinY = 6 * ConstNum.ONE_INCH;
						PlWidth = 12 * ConstNum.ONE_INCH;
						PLLength = 16 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 14 * ConstNum.ONE_INCH && tColumn.Shape.a >= 26.5 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 14 * ConstNum.ONE_INCH && bColumn.Shape.a >= 26.5 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W14X90
						CSMinX = 4 * ConstNum.ONE_INCH;
						CSMinY = 6 * ConstNum.ONE_INCH;
						PlWidth = 12 * ConstNum.ONE_INCH;
						PLLength = 16 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 13.9 * ConstNum.ONE_INCH && tColumn.Shape.a >= 17.9 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.9 * ConstNum.ONE_INCH && bColumn.Shape.a >= 17.9 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W14X61
						CSMinX = 3 * ConstNum.ONE_INCH;
						CSMinY = 6 * ConstNum.ONE_INCH;
						PlWidth = 8 * ConstNum.ONE_INCH;
						PLLength = 16 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 13.7 * ConstNum.ONE_INCH && tColumn.Shape.a >= 12.6 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.7 * ConstNum.ONE_INCH && bColumn.Shape.a >= 12.6 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.3125F * ConstNum.ONE_INCH;     // >=W14X43
						CSMinX = 2 * ConstNum.ONE_INCH;
						CSMinY = 5 * ConstNum.ONE_INCH;
						PlWidth = 6 * ConstNum.ONE_INCH;
						PLLength = 14 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 13.1 * ConstNum.ONE_INCH && tColumn.Shape.a >= 35.3 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.1 * ConstNum.ONE_INCH && bColumn.Shape.a >= 35.3 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.5F * ConstNum.ONE_INCH;     // >=W12X120
						CSMinX = 3 * ConstNum.ONE_INCH;
						CSMinY = 6 * ConstNum.ONE_INCH;
						PlWidth = 8 * ConstNum.ONE_INCH;
						PLLength = 16 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 12.1 * ConstNum.ONE_INCH && tColumn.Shape.a >= 15.6 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 12.1 * ConstNum.ONE_INCH && bColumn.Shape.a >= 15.6 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W12X53
						CSMinX = 3 * ConstNum.ONE_INCH;
						CSMinY = 6 * ConstNum.ONE_INCH;
						PlWidth = 8 * ConstNum.ONE_INCH;
						PLLength = 16 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 11.9 * ConstNum.ONE_INCH && tColumn.Shape.a >= 11.7 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 11.9 * ConstNum.ONE_INCH && bColumn.Shape.a >= 11.7 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.3125F * ConstNum.ONE_INCH;     // >=W12X40
						CSMinX = 2 * ConstNum.ONE_INCH;
						CSMinY = 5 * ConstNum.ONE_INCH;
						PlWidth = 6 * ConstNum.ONE_INCH;
						PLLength = 14 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 9.98 * ConstNum.ONE_INCH && tColumn.Shape.a >= 14.4 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 9.98 * ConstNum.ONE_INCH && bColumn.Shape.a >= 14.4 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W10X49
						CSMinX = 3 * ConstNum.ONE_INCH;
						CSMinY = 6 * ConstNum.ONE_INCH;
						PlWidth = 8 * ConstNum.ONE_INCH;
						PLLength = 16 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 9.73 * ConstNum.ONE_INCH && tColumn.Shape.a >= 9.71 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 9.73 * ConstNum.ONE_INCH && bColumn.Shape.a >= 9.71 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.3125F * ConstNum.ONE_INCH;     // >=W10X33
						CSMinX = 2 * ConstNum.ONE_INCH;
						CSMinY = 5 * ConstNum.ONE_INCH;
						PlWidth = 6 * ConstNum.ONE_INCH;
						PLLength = 14 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 8 * ConstNum.ONE_INCH && tColumn.Shape.a >= 9.12 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 8 * ConstNum.ONE_INCH && bColumn.Shape.a >= 9.12 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W8X31
						CSMinX = 2 * ConstNum.ONE_INCH;
						CSMinY = 5 * ConstNum.ONE_INCH;
						PlWidth = 6 * ConstNum.ONE_INCH;
						PLLength = 14 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 7.93 * ConstNum.ONE_INCH && tColumn.Shape.a >= 7.08 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 7.93 * ConstNum.ONE_INCH && bColumn.Shape.a >= 7.08 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.3125F * ConstNum.ONE_INCH;     // >=W8X24
						CSMinX = 2 * ConstNum.ONE_INCH;
						CSMinY = 4 * ConstNum.ONE_INCH;
						PlWidth = 5 * ConstNum.ONE_INCH;
						PLLength = 12 * ConstNum.ONE_INCH;
					}
					else
					{
						Minth = 0.3125F * ConstNum.ONE_INCH;     //  All Others
						CSMinX = 2 * ConstNum.ONE_INCH;
						CSMinY = 4 * ConstNum.ONE_INCH;
						PlWidth = 5 * ConstNum.ONE_INCH;
						PLLength = 12 * ConstNum.ONE_INCH;
					}
					break;
				case "LCWeldedUCBolted":
				case "UCWeldedLCBolted":
					if (tColumn.Shape.d >= 19 * ConstNum.ONE_INCH && tColumn.Shape.a >= 134 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 19 * ConstNum.ONE_INCH && bColumn.Shape.a >= 134 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.625 * ConstNum.ONE_INCH;     // >=W14X455
						CSMinX = 5 * ConstNum.ONE_INCH;
						CSMinY = 7 * ConstNum.ONE_INCH;
						PlWidth = 14 * ConstNum.ONE_INCH;
						PLLength = 18 * ConstNum.ONE_INCH;
						Gage = 11.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 17.1 * ConstNum.ONE_INCH && tColumn.Shape.a >= 91.4 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 17.1 * ConstNum.ONE_INCH && bColumn.Shape.a >= 91.4 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.625 * ConstNum.ONE_INCH;     // >=W14X311
						CSMinX = 4 * ConstNum.ONE_INCH;
						CSMinY = 6 * ConstNum.ONE_INCH;
						PlWidth = 12 * ConstNum.ONE_INCH;
						PLLength = 17 * ConstNum.ONE_INCH;
						Gage = 9.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 15.7 * ConstNum.ONE_INCH && tColumn.Shape.a >= 62 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 15.7 * ConstNum.ONE_INCH && bColumn.Shape.a >= 62 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.5F * ConstNum.ONE_INCH;     // >=W14X211
						CSMinX = 4 * ConstNum.ONE_INCH;
						CSMinY = 6 * ConstNum.ONE_INCH;
						PlWidth = 12 * ConstNum.ONE_INCH;
						PLLength = 17 * ConstNum.ONE_INCH;
						Gage = 9.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 14 * ConstNum.ONE_INCH && tColumn.Shape.a >= 26.5 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 14 * ConstNum.ONE_INCH && bColumn.Shape.a >= 26.5 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W14X90
						CSMinX = 4 * ConstNum.ONE_INCH;
						CSMinY = 6 * ConstNum.ONE_INCH;
						PlWidth = 12 * ConstNum.ONE_INCH;
						PLLength = 14 * ConstNum.ONE_INCH;
						Gage = 9.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 13.9 * ConstNum.ONE_INCH && tColumn.Shape.a >= 17.9 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.9 * ConstNum.ONE_INCH && bColumn.Shape.a >= 17.9 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W14X61
						CSMinX = 3 * ConstNum.ONE_INCH;
						CSMinY = 6 * ConstNum.ONE_INCH;
						PlWidth = 8 * ConstNum.ONE_INCH;
						PLLength = 14 * ConstNum.ONE_INCH;
						Gage = 5.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 13.7 * ConstNum.ONE_INCH && tColumn.Shape.a >= 12.6 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.7 * ConstNum.ONE_INCH && bColumn.Shape.a >= 12.6 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.3125F * ConstNum.ONE_INCH;     // >=W14X43
						CSMinX = 2 * ConstNum.ONE_INCH;
						CSMinY = 5 * ConstNum.ONE_INCH;
						PlWidth = 6 * ConstNum.ONE_INCH;
						PLLength = 13 * ConstNum.ONE_INCH;
						Gage = 3.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 13.1 * ConstNum.ONE_INCH && tColumn.Shape.a >= 35.3 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 13.1 * ConstNum.ONE_INCH && bColumn.Shape.a >= 35.3 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.5F * ConstNum.ONE_INCH;     // >=W12X120
						CSMinX = 3 * ConstNum.ONE_INCH;
						CSMinY = 6 * ConstNum.ONE_INCH;
						PlWidth = 8 * ConstNum.ONE_INCH;
						PLLength = 14 * ConstNum.ONE_INCH;
						Gage = 5.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 12.1 * ConstNum.ONE_INCH && tColumn.Shape.a >= 15.6 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 12.1 * ConstNum.ONE_INCH && bColumn.Shape.a >= 15.6 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W12X53
						CSMinX = 3 * ConstNum.ONE_INCH;
						CSMinY = 6 * ConstNum.ONE_INCH;
						PlWidth = 8 * ConstNum.ONE_INCH;
						PLLength = 14 * ConstNum.ONE_INCH;
						Gage = 5.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 11.9 * ConstNum.ONE_INCH && tColumn.Shape.a >= 11.7 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 11.9 * ConstNum.ONE_INCH && bColumn.Shape.a >= 11.7 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.3125F * ConstNum.ONE_INCH;     // >=W12X40
						CSMinX = 2 * ConstNum.ONE_INCH;
						CSMinY = 5 * ConstNum.ONE_INCH;
						PlWidth = 6 * ConstNum.ONE_INCH;
						PLLength = 13 * ConstNum.ONE_INCH;
						Gage = 3.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 9.98 * ConstNum.ONE_INCH && tColumn.Shape.a >= 14.4 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 9.98 * ConstNum.ONE_INCH && bColumn.Shape.a >= 14.4 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W10X49
						CSMinX = 3 * ConstNum.ONE_INCH;
						CSMinY = 6 * ConstNum.ONE_INCH;
						PlWidth = 8 * ConstNum.ONE_INCH;
						PLLength = 14 * ConstNum.ONE_INCH;
						Gage = 5.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 9.73 * ConstNum.ONE_INCH && tColumn.Shape.a >= 9.71 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 9.73 * ConstNum.ONE_INCH && bColumn.Shape.a >= 9.71 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.3125F * ConstNum.ONE_INCH;     // >=W10X33
						CSMinX = 2 * ConstNum.ONE_INCH;
						CSMinY = 5 * ConstNum.ONE_INCH;
						PlWidth = 6 * ConstNum.ONE_INCH;
						PLLength = 13 * ConstNum.ONE_INCH;
						Gage = 3.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 8 * ConstNum.ONE_INCH && tColumn.Shape.a >= 9.12 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 8 * ConstNum.ONE_INCH && bColumn.Shape.a >= 9.12 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.375 * ConstNum.ONE_INCH;     // >=W8X31
						CSMinX = 2 * ConstNum.ONE_INCH;
						CSMinY = 5 * ConstNum.ONE_INCH;
						PlWidth = 6 * ConstNum.ONE_INCH;
						PLLength = 13 * ConstNum.ONE_INCH;
						Gage = 3.5 * ConstNum.ONE_INCH;
					}
					else if (tColumn.Shape.d >= 7.93 * ConstNum.ONE_INCH && tColumn.Shape.a >= 7.08 * Math.Pow(ConstNum.ONE_INCH, 2) || bColumn.Shape.d >= 7.93 * ConstNum.ONE_INCH && bColumn.Shape.a >= 7.08 * Math.Pow(ConstNum.ONE_INCH, 2))
					{
						Minth = 0.3125F * ConstNum.ONE_INCH;     // >=W8X24
						CSMinX = 2 * ConstNum.ONE_INCH;
						CSMinY = 4 * ConstNum.ONE_INCH;
						PlWidth = 5 * ConstNum.ONE_INCH;
						PLLength = 12 * ConstNum.ONE_INCH;
						Gage = 3.5 * ConstNum.ONE_INCH;
					}
					else
					{
						Minth = 0.3125F * ConstNum.ONE_INCH;     //  All Others
						CSMinX = 2 * ConstNum.ONE_INCH;
						CSMinY = 4 * ConstNum.ONE_INCH;
						PlWidth = 5 * ConstNum.ONE_INCH;
						PLLength = 12 * ConstNum.ONE_INCH;
						Gage = 3.5 * ConstNum.ONE_INCH;
					}
					break;
			}
			Minth = (CommonCalculations.PlateThickness(Minth));
		}
	}
}