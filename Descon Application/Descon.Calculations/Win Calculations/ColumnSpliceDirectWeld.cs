using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class ColumnSpliceDirectWeld
	{
		public static void DesignColumnSpliceDirectWeld( ref double Acontact )
		{
			double usefulweldonPlate = 0;
			double FWeldSizeFillerToCol = 0;
			double FWeldSizeChToFiller = 0;
			double FWeldSizeChToCol = 0;
			double minedge = 0;
			double minsp = 0;
			double FuT = 0;
			double Fsplice = 0;
			double a = 0;
			double ce = 0;
			double s = 0;
			double e = 0;
			double CapacityN = 0;
			double Ae = 0;
			double U = 0;
			double A_ch = 0;
			double CapacityBS = 0;
			double Lnv = 0;
			double Lgv = 0;
			double Lnt = 0;
			double Lgt = 0;
			double CapacityBr = 0;
			double Fbrs = 0;
			double Fbre = 0;
			double Ftemp = 0;
			double capacity = 0;
			double maxweldsize = 0;
			double minweldsize = 0;
			double Fdesign = 0;
			string seism = "";
			double FfSeismic = 0;
			double Fw = 0;
			double Fw_Comp = 0;
			double Fw_Ten = 0;
			double Ff_Comp = 0;
			double Ff_Ten = 0;
			double Ff_T = 0;
			double Ff_C = 0;
			int Fw_Ten4 = 0;
			double Ff_Ten4 = 0;
			double Fw_Comp4 = 0;
			double Ff_Comp4 = 0;
			double Fw_Ten3 = 0;
			double Ff_Ten3 = 0;
			int Fw_Comp3 = 0;
			double Ff_Comp3 = 0;
			double Fw_Ten2 = 0;
			double Ff_Ten2 = 0;
			double Fw_Comp1 = 0;
			double Ff_Comp1 = 0;
			double tw = 0;
			double d = 0;
			double BF = 0;
			double tf = 0;
			double weldForce = 0;

			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Directly Welded Columns:");
			Acontact = Math.Min(tColumn.Shape.a, bColumn.Shape.a);

			weldForce = 0;
			tf = Math.Min(tColumn.Shape.tf, bColumn.Shape.tf);
			BF = Math.Min(tColumn.Shape.bf, bColumn.Shape.bf);
			d = Math.Min(tColumn.Shape.d, bColumn.Shape.d);
			tw = Math.Min(tColumn.Shape.tw, bColumn.Shape.tw);

			Reporting.AddHeader("Forces Through Flange and Web splices:");
			if (colSplice.Moment == 0 && colSplice.Compression > 0)
			{
				// case 1
				Ff_Comp1 = colSplice.Compression / Acontact * tf * BF;
				Fw_Comp1 = colSplice.Compression - 2 * Ff_Comp1;
				Reporting.AddHeader("Axial Compression:");
				Reporting.AddHeader("Flange (each):");
				Reporting.AddLine("Ff_Comp = C / A * tf * bf =  " + colSplice.Compression + " /  " + Acontact + " *  " + tf + " *  " + BF + " =  " + Ff_Comp1 + ConstUnit.Force);
				Reporting.AddHeader("Web:");
				Reporting.AddLine("Fw_Comp = C - 2 * Ff_Comp =  " + colSplice.Compression + " - 2 * " + Ff_Comp1 + " =  " + Fw_Comp1 + ConstUnit.Force);
			}

			if (colSplice.Moment == 0 && colSplice.Tension > 0)
			{
				Ff_Ten2 = colSplice.Tension / Acontact * tf * BF;
				Fw_Ten2 = colSplice.Tension - 2 * Ff_Ten2;
				Reporting.AddHeader("Axial Tension:");
				Reporting.AddHeader("Flange (each):");
				Reporting.AddLine("Ff_Ten = T / A * tf * bf =  " + colSplice.Tension + " /  " + Acontact + " *  " + tf + " *  " + BF + " =  " + Ff_Ten2 + ConstUnit.Force);
				Reporting.AddHeader("Web:");
				Reporting.AddLine("Fw_Ten = T - 2 * Ff_Ten =  " + colSplice.Tension + " - 2 * " + Ff_Ten2 + " =  " + Fw_Ten2 + ConstUnit.Force);
			}
			if (colSplice.Moment > 0 && colSplice.Tension >= 0)
			{
				if (colSplice.Tension > 0)
				{
					weldForce = Math.Max(weldForce, (colSplice.Tension / 2 + colSplice.Moment / Math.Min(tColumn.Shape.d, bColumn.Shape.d)) / Math.Min(tColumn.Shape.bf, bColumn.Shape.bf));
					Ff_Ten3 = colSplice.Tension / Acontact * tf * BF + colSplice.Moment / d;
					Fw_Ten3 = colSplice.Tension - 2 * Ff_Ten3;
					Ff_Comp3 = Math.Max(-colSplice.Tension / Acontact * tf * BF + colSplice.Moment / d, 0);
					Fw_Comp3 = 0;
					Reporting.AddHeader("Combined Moment & Tension");
					Reporting.AddLine("Ff_Ten = T/A*tf*BF + M/d");
					Reporting.AddLine("= " + colSplice.Tension + " / " + Acontact + " * " + tf + " * " + BF + " + " + colSplice.Moment + " / " + d);
					Reporting.AddLine("= " + Ff_Ten3 + ConstUnit.Force);
					
					Reporting.AddHeader("Fw_Ten = T - 2*Ff_Ten");
					Reporting.AddLine("= " + colSplice.Tension + " - 2*" + Ff_Ten3 + " = " + Fw_Ten3 + ConstUnit.Force);
					
					Reporting.AddHeader("Ff_Comp = Max[-T/A*tf*BF + M/d; 0]");
					Reporting.AddLine("= Max[-" + colSplice.Tension + " / " + Acontact + " * " + tf + " * " + BF + " + " + colSplice.Moment + " / " + d + ";0]");
					Reporting.AddLine("= " + Ff_Comp3 + ConstUnit.Force);
					Reporting.AddLine("Fw_Comp = 0");
				}
				else
				{
					weldForce = Math.Max(weldForce, (-colSplice.Cmin / 2 + colSplice.Moment / Math.Min(tColumn.Shape.d, bColumn.Shape.d)) / Math.Min(tColumn.Shape.bf, bColumn.Shape.bf));
					Ff_Ten3 = Math.Max(0, -colSplice.Cmin / Acontact * tf * BF + colSplice.Moment / d);
					Fw_Ten3 = Math.Max(0, -colSplice.Cmin - 2 * Ff_Ten3);
					Ff_Comp3 = Math.Max(colSplice.Cmin / Acontact * tf * BF + colSplice.Moment / d, 0);
					Fw_Comp3 = 0;

					Reporting.AddHeader("Combined Moment & Tension");
					Reporting.AddLine("Ff_Ten = -Cmin / A * tf * BF + M / d");
					Reporting.AddLine("= " + (-colSplice.Cmin) + " / " + Acontact + " * " + tf + " * " + BF + " + " + colSplice.Moment + " / " + d);
					Reporting.AddLine("= " + Ff_Ten3 + ConstUnit.Force);

					Reporting.AddHeader("Fw_Ten = -Cmin - 2 * Ff_Ten >= 0");
					Reporting.AddLine("= " + (-colSplice.Cmin) + " - 2*" + Ff_Ten3 + " = " + Fw_Ten3 + ConstUnit.Force);

					Reporting.AddHeader("Ff_Comp = Max[CminA * tf * BF + M / d; 0]");
					Reporting.AddLine("= Max[-" + colSplice.Cmin + " / " + Acontact + " * " + tf + " * " + BF + " + " + colSplice.Moment + " / " + d + ";0]");
					Reporting.AddLine("= " + Ff_Comp3 + ConstUnit.Force);
					Reporting.AddLine("Fw_Comp = 0");
				}
			}

			if (colSplice.Moment > 0 && colSplice.Compression >= 0)
			{
				Reporting.AddHeader("Combined Moment & Compression");
				Ff_C = colSplice.Compression / Acontact * (tColumn.Shape.tf - ConstNum.QUARTER_INCH) * Math.Min(tColumn.Shape.bf, bColumn.Shape.bf) + colSplice.Moment / Math.Min(bColumn.Shape.d, tColumn.Shape.d);
				weldForce = Math.Max(weldForce, Ff_C / Math.Min(tColumn.Shape.bf, bColumn.Shape.bf));
				Ff_Comp4 = colSplice.Compression / Acontact * tf * BF + colSplice.Moment / d;
				Fw_Comp4 = colSplice.Compression / Acontact * tw * (d - 2 * tf);

				Ff_T = colSplice.Compression / Acontact * (tColumn.Shape.tf - ConstNum.QUARTER_INCH) * Math.Min(tColumn.Shape.bf, bColumn.Shape.bf) - colSplice.Moment / Math.Min(bColumn.Shape.d, tColumn.Shape.d);
				Ff_T = Math.Abs(Math.Min(Ff_T, 0));
				weldForce = Math.Max(weldForce, Ff_T / Math.Min(tColumn.Shape.bf, bColumn.Shape.bf));
				Ff_Ten4 = Math.Max(-colSplice.Compression / Acontact * tf * BF + colSplice.Moment / d, 0);
				Fw_Ten4 = 0;

				Reporting.AddHeader("Ff_Comp = Column.p / Acontact * tf * bf + M / d");
				Reporting.AddLine("= " + colSplice.Compression + " / " + Acontact + " * " + tf + " * " + BF + " + " + colSplice.Moment + " / " + d);
				Reporting.AddLine("= " + Ff_Comp4 + ConstUnit.Force);

				Reporting.AddHeader("Fw_Comp = Column.p / Acontact * tw * (d - 2 * tf)");
				Reporting.AddLine("= " + colSplice.Compression + " / " + Acontact + " * " + tw + " * (" + d + " - 2 * " + tf);
				Reporting.AddLine("= " + Fw_Comp4 + ConstUnit.Force);

				Reporting.AddHeader("Ff_Ten = Max[-C / A * tf * bf + M / d; 0]");
				Reporting.AddLine("= Max[-" + colSplice.Compression + " / " + Acontact + " * " + tf + " * " + BF + " + " + colSplice.Moment + " / " + d + ";0]");
				Reporting.AddLine("= " + Ff_Ten4 + ConstUnit.Force);
				Reporting.AddLine("Fw_Ten = 0");

			}
			Ff_Ten = Math.Max(Ff_Ten2, Math.Max(Ff_Ten3, Ff_Ten4));
			Ff_Comp = Math.Max(Ff_Comp1, Math.Max(Ff_Comp3, Ff_Comp4));
			Fw_Ten = Math.Max(Fw_Ten2, Math.Max(Fw_Ten3, Fw_Ten4));
			Fw_Comp = Math.Max(Fw_Comp1, Math.Max(Fw_Comp3, Fw_Comp4));
			Fw = Math.Max(Fw_Ten, Fw_Comp);
			if (colSplice.UseSeismic)
			{
				if (colSplice.SMF)
					Ff_Ten = Math.Max(Ff_Ten, ColumnSplice.FfToDevMoment);
				FfSeismic = Math.Max(Ff_Ten, ColumnSplice.FlangeTensionforSeismic);
				if (colSplice.FlangeWeldType == EWeldType.PJP)
					FfSeismic = Math.Max(2 * Ff_Ten, FfSeismic);
				seism = "(Increased per seismic provisions)";
			}
			else
			{
				FfSeismic = Ff_Ten;
				seism = string.Empty;
			}

			Reporting.AddHeader("Maximum Forces:");
			Reporting.AddLine("Flange Force (Compression), Ffc = " + Ff_Comp + ConstUnit.Force);
			Reporting.AddLine("Flange Force (Tension), Fft = " + Ff_Ten + ConstUnit.Force + seism);
			Reporting.AddLine("Web Force (Compression), Fwc = " + Fw_Comp + ConstUnit.Force);
			Reporting.AddLine("Web Force (Tension), Fwt = " + Fw_Ten + ConstUnit.Force);

			Fdesign = Math.Sqrt(Math.Pow(Fw, 2) + Math.Pow(colSplice.WebShear, 2));
			Reporting.AddLine("Web Vertical Force = " + Fw + ConstUnit.Force);
			Reporting.AddLine("Web Horizontal Force = " + colSplice.WebShear + ConstUnit.Force);
			Reporting.AddLine("Web Resultant Force (Fr) = " + Fdesign + ConstUnit.Force);

			if (colSplice.FlangeWeldType == EWeldType.PJP)
			{
				Reporting.AddLine("Flange Welds (PJP): " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeFlangeUpper) + ConstUnit.Length);
				minweldsize = ColumnSpliceMisc.MinimumPJPSize(tColumn.Shape.tf, bColumn.Shape.tf);
				maxweldsize = ((int)Math.Floor(16 * (tf - ConstNum.QUARTER_INCH))) / 16.0;
				if (minweldsize > maxweldsize)
				{
					Reporting.AddLine("Minimum Weld Size = " + minweldsize + " >> Maximum Weld Size = " + maxweldsize + ConstUnit.Length + " (NG)");
					Reporting.AddLine("Use CJP weld or Splice Plates.");
				}
				if (minweldsize <= colSplice.FilletWeldSizeFlangeUpper)
					Reporting.AddLine("Minimum Weld Size = " + minweldsize + " <= " + colSplice.FilletWeldSizeFlangeUpper + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size = " + minweldsize + " >> " + colSplice.FilletWeldSizeFlangeUpper + ConstUnit.Length + " (NG)");

				if (maxweldsize >= colSplice.FilletWeldSizeFlangeUpper)
					Reporting.AddLine("Maximum Weld Size = " + maxweldsize + " >= " + colSplice.FilletWeldSizeFlangeUpper + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Maximum Weld Size = " + maxweldsize + " << " + colSplice.FilletWeldSizeFlangeUpper + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Check Flange Welds for Tension:");
				capacity = colSplice.FilletWeldSizeFlangeUpper * BF * Math.Min(0.8 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx, ConstNum.FIOMEGA0_9N * Math.Min(tColumn.Material.Fy, bColumn.Material.Fy));
				Reporting.AddLine("Weld Capacity = w * bf * Min[(0.8 * 0.6 *  Fexx), " + ConstString.FIOMEGA0_9 + " * Min(Fy_uc; Fy_lc)]");
				Reporting.AddLine("= " + colSplice.FilletWeldSizeFlangeUpper + " * " + BF + " * Min[(0.8 * 0.6 *  " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + "); " + ConstString.FIOMEGA0_9 + " * Min(" + tColumn.Material.Fy + ", " + bColumn.Material.Fy + ")]");
				if (colSplice.UseSeismic)
				{
					Ftemp = 2 * Ff_Ten;
					seism = " (Force doubled per seismic provisions) ";
				}
				else
				{
					Ftemp = Ff_Ten;
					seism = string.Empty;
				}
				if (capacity >= Ftemp)
					Reporting.AddLine("= " + capacity + " >= " + Ftemp + ConstUnit.Force + " (OK)" + seism);
				else
					Reporting.AddLine("= " + capacity + " << " + Ftemp + ConstUnit.Force + " (NG)" + seism);

				Reporting.AddHeader("Check Flange Welds for Compression:");
				capacity = colSplice.FilletWeldSizeFlangeUpper * BF * ConstNum.FIOMEGA0_9N * Math.Min(tColumn.Material.Fy, bColumn.Material.Fy);
				Reporting.AddLine("Weld Capacity = w * bf * " + ConstString.FIOMEGA0_9 + " *Min(Fy_uc; Fy_lc)");
				Reporting.AddLine("= " + colSplice.FilletWeldSizeFlangeUpper + " * " + BF + "* " + ConstString.FIOMEGA0_9 + " *Min(" + tColumn.Material.Fy + "; " + bColumn.Material.Fy + ")");
				if (capacity >= Ff_Comp)
					Reporting.AddLine("= " + capacity + " >= " + Ff_Comp + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + Ff_Comp + ConstUnit.Force + " (NG)");
			}

			if (colSplice.FlangeWeldType == EWeldType.CJP)
			{
				Reporting.AddHeader("Flange Welds: CJP");
				Reporting.AddHeader("Check Flange Welds for Tension:");
				capacity = tf * BF * ConstNum.FIOMEGA0_9N * Math.Min(tColumn.Material.Fy, bColumn.Material.Fy);
				Reporting.AddLine("Weld Capacity = tf * bf * " + ConstString.FIOMEGA0_9 + " * Min(Fy_uc, Fy_lc)");
				Reporting.AddLine("= " + tf + " * " + BF + " * " + ConstString.FIOMEGA0_9 + " * Min(" + tColumn.Material.Fy + ", " + bColumn.Material.Fy + ")");
				if (capacity >= Ff_Ten)
					Reporting.AddLine("= " + capacity + " >= " + Ff_Ten + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + Ff_Ten + ConstUnit.Force + " (NG)");

				Reporting.AddHeader("Check Flange Welds for Compression:");
				capacity = tf * BF * ConstNum.FIOMEGA0_9N * Math.Min(tColumn.Material.Fy, bColumn.Material.Fy);
				Reporting.AddLine("Weld Capacity = tf * bf * " + ConstString.FIOMEGA0_9 + " * Min(Fy_uc, Fy_lc)");
				Reporting.AddLine("= " + tf + " * " + BF + " * " + ConstString.FIOMEGA0_9 + " * Min(" + tColumn.Material.Fy + ", " + bColumn.Material.Fy + ")");
				if (capacity >= Ff_Comp)
					Reporting.AddLine("= " + capacity + " >= " + Ff_Comp + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + Ff_Comp + ConstUnit.Force + " (NG)");
			}

			if (colSplice.ChannelType == ESpliceChannelType.Temporary)
			{
				if (colSplice.NumberOfChannels == 2)
					Reporting.AddHeader("Splice Channels: 2 " + colSplice.Channel.Name + " - " + colSplice.Material.Name);
				else
					Reporting.AddHeader("Splice Channel: 1 " + colSplice.Channel.Name + " - " + colSplice.Material.Name);
				
				Reporting.AddLine("(Remove after welding flanges to weld column web.)");

				if (colSplice.WebWeldType == EWeldType.PJP)
				{
					Reporting.AddHeader("Web Welds (PJP): " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebUpper) + ConstUnit.Length);
					minweldsize = ColumnSpliceMisc.MinimumPJPSize(tColumn.Shape.tw, bColumn.Shape.tw);
					maxweldsize = ((int)Math.Floor(16 * (tw - ConstNum.QUARTER_INCH))) / 16.0;
					if (minweldsize > maxweldsize)
					{
						Reporting.AddLine("Minimum Weld Size = " + minweldsize + " >> Maximum Weld Size = " + maxweldsize + ConstUnit.Length + " (NG)");
						Reporting.AddLine("(Use CJP weld or Splice Plates/Channels.)");
					}

					if (minweldsize <= colSplice.FilletWeldSizeWebUpper)
						Reporting.AddLine("Minimum Weld Size = " + minweldsize + " <= " + colSplice.FilletWeldSizeWebUpper + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Minimum Weld Size = " + minweldsize + " >> " + colSplice.FilletWeldSizeWebUpper + ConstUnit.Length + " (NG)");

					if (maxweldsize >= colSplice.FilletWeldSizeWebUpper)
						Reporting.AddLine("Maximum Weld Size = " + maxweldsize + " >= " + colSplice.FilletWeldSizeWebUpper + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Maximum Weld Size = " + maxweldsize + " << " + colSplice.FilletWeldSizeWebUpper + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Check Web Welds for Tension:");
					capacity = colSplice.FilletWeldSizeWebUpper * (d - 2 * tf) * Math.Min(0.8 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx, ConstNum.FIOMEGA0_9N * Math.Min(tColumn.Material.Fy, bColumn.Material.Fy));
					Reporting.AddLine("Weld Capacity = w * (d - 2 * tf) * Min[(0.8 * 0.6 *  Fexx); " + ConstString.FIOMEGA0_9 + " * Min(Fy_uc, Fy_lc)]");
					Reporting.AddLine("= " + colSplice.FilletWeldSizeWebUpper + " * (" + d + " - 2 * " + tf + ") * Min[(0.8 * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + "); " + ConstString.FIOMEGA0_9 + " * Min(" + tColumn.Material.Fy + "; " + bColumn.Material.Fy + ")]");

					if (colSplice.UseSeismic)
					{
						Ftemp = 2 * Fw_Ten;
						seism = " (Force doubled per seismic provisions) ";
					}
					else
					{
						Ftemp = Fw_Ten;
						seism = "";
					}

					if (capacity >= Fw_Ten)
						Reporting.AddLine("= " + capacity + " >= " + Ftemp + ConstUnit.Force + " (OK)" + seism);
					else
						Reporting.AddLine("= " + capacity + " << " + Ftemp + ConstUnit.Force + " (NG)" + seism);

					Reporting.AddHeader("Check Web Welds for Compression:");
					capacity = colSplice.FilletWeldSizeWebUpper * BF * ConstNum.FIOMEGA0_9N * Math.Min(tColumn.Material.Fy, bColumn.Material.Fy);
					Reporting.AddLine("Weld Capacity = w *  (d - 2 * tf) * " + ConstString.FIOMEGA0_9 + " * Min(Fy_uc, Fy_lc)");
					Reporting.AddLine("= " + colSplice.FilletWeldSizeWebUpper + " * (" + d + " - 2 * " + tf + ") * " + ConstString.FIOMEGA0_9 + " * Min(" + tColumn.Material.Fy + "; " + bColumn.Material.Fy + ")");
					if (capacity >= Fw_Comp)
						Reporting.AddLine("= " + capacity + " >= " + Fw_Comp + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + capacity + " << " + Fw_Comp + ConstUnit.Force + " (NG)");
				}

				if (colSplice.WebWeldType == EWeldType.CJP)
				{
					Reporting.AddHeader("Web Welds: CJP");
					Reporting.AddLine("Check Web Welds for Tension:");
					capacity = tw * (d - 2 * tf) * ConstNum.FIOMEGA0_9N * Math.Min(tColumn.Material.Fy, bColumn.Material.Fy);
					Reporting.AddLine("Weld Capacity = tw * (d - 2 * tf) * " + ConstString.FIOMEGA0_9 + " * Min(Fy_uc, Fy_lc)");
					Reporting.AddLine("= " + tw + " * (" + d + " - 2 * " + tf + ") * " + ConstString.FIOMEGA0_9 + " * Min(" + tColumn.Material.Fy + ", " + bColumn.Material.Fy + ")");
					if (capacity >= Fw_Ten)
						Reporting.AddLine("= " + capacity + " >= " + Fw_Ten + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + capacity + " << " + Fw_Ten + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Check Web Welds for Compression:");
					capacity = tw * (d - 2 * tf) * ConstNum.FIOMEGA0_9N * Math.Min(tColumn.Material.Fy, bColumn.Material.Fy);
					Reporting.AddLine("Weld Capacity = tw *  (d - 2 * tf) * " + ConstString.FIOMEGA0_9 + " * Min(Fy_uc, Fy_lc)");
					Reporting.AddLine("= " + tw + " * " + (d - 2 * tf) + " * " + ConstString.FIOMEGA0_9 + " * Min(" + tColumn.Material.Fy + "; " + bColumn.Material.Fy + ")");
					if (capacity >= Fw_Comp)
						Reporting.AddLine("= " + capacity + " >= " + Fw_Comp + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + capacity + " << " + Fw_Comp + ConstUnit.Force + " (NG)");
				}
			}
			else
			{
				Reporting.AddHeader("Web Splice:");
				if (colSplice.NumberOfChannels == 2)
					Reporting.AddLine("Splice Channels: 2 " + colSplice.Channel.Name + " - " + colSplice.Material.Name);
				else
					Reporting.AddLine("Splice Channel: 1 " + colSplice.Channel.Name + " - " + colSplice.Material.Name);
				Reporting.AddLine("(Left in place to participate in load transfer)");
				if (colSplice.ConnectionUpper == EConnectionStyle.Bolted)
				{
					if (colSplice.WebShear > 0)
						ColumnSpliceWebPlate.WebPlateCapacity(EColumnSplice.UpperColumn, Fw);
					else
						BoltsUpperWeb(ref e, ref s, ref ce, ref tw, ref tf, ref d, ref a, Fw_Ten, Fw_Comp, ref Fsplice, ref FuT, ref minsp, 0, ref minedge, ref capacity);
					BoltBearingUpperColumnTension(ref Fbre, ref Fbrs, ref CapacityBr, Fw_Ten);
					BoltBearingUpperColumnCompression(ref Fbrs, ref CapacityBr, Fw_Comp, Fw_Ten);
					BlockShearUpperColumn(ref Lgt, ref Lnt, ref Lgv, ref Lnv, ref CapacityBS, Fw_Ten, Fw_Comp);
					PlateTensionGross(ref A_ch, Fw_Ten);
				}
				else
				{
					if (colSplice.WebShear > 0)
						ColumnSpliceWebPlate.WebPlateCapacity(EColumnSplice.UpperColumn, Fw);
					else
						WeldsUpperWeb(ref d, ref tw, ref FWeldSizeChToCol, ref FWeldSizeChToFiller, ref FWeldSizeFillerToCol, ref usefulweldonPlate, ref capacity, Fw);
					PlateTensionGross(ref A_ch, Fw_Ten);
					PlateTensionWeldedNetUpperColumn(ref d, ref A_ch, ref U, ref CapacityN, Fw_Ten);
				}

				if (colSplice.ConnectionLower == EConnectionStyle.Bolted)
				{
					if (colSplice.WebShear > 0)
						ColumnSpliceWebPlate.WebPlateCapacity(EColumnSplice.LowerColumn, Fw);
					else
						BoltsLowerWeb(ref e, ref s, ref ce, ref tw, ref tf, ref d, ref a, Fw_Ten, Fw_Comp, ref Fsplice, ref FuT, ref minsp, 0, ref minedge, ref capacity);
					BoltBearingLowerColumnTension(ref Fbre, ref Fbrs, ref CapacityBr, Fw_Ten);
					BoltBearingLowerColumnCompression(ref Fbrs, ref CapacityBr, Fw_Comp);
					BlockShearLowerColumn(ref Lgt, ref Lnt, ref Lgv, ref Lnv, ref CapacityBS, Fw_Ten);
					PlateTensionGross(ref A_ch, Fw_Ten);
				}
				else
				{
					if (colSplice.WebShear > 0)
						ColumnSpliceWebPlate.WebPlateCapacity(EColumnSplice.LowerColumn, Fw);
					else
						WeldsLowerWeb(ref d, ref tw, ref FWeldSizeChToCol, ref FWeldSizeChToFiller, ref FWeldSizeFillerToCol, ref usefulweldonPlate, ref capacity, Fw);
					PlateTensionGross(ref A_ch, Fw_Ten);
					PlateTensionWeldedNetLowerColumn(ref d, ref A_ch, ref U, ref CapacityN, Fw_Ten);
				}
			} 

			Reporting.AddHeader("On Net Area:");
			A_ch = colSplice.Channel.a * colSplice.NumberOfChannels;
			if (colSplice.BoltRowsWebUpper >= 3)
				U = 0.85;
			else
				U = 0.75;
			
			Ae = U * (A_ch - 2 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * colSplice.Channel.tw;
			CapacityN = ConstNum.FIOMEGA0_75N * colSplice.Material.Fu * Ae;
			Reporting.AddLine("Ae = U * (Ag - 2 * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= " + Ae + ConstUnit.Area);
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + "* Fu * Ae = " + ConstString.FIOMEGA0_75 + "* " + colSplice.Material.Fu + " * " + Ae);
			if (CapacityN >= Fw_Ten)
				Reporting.AddLine("= " + CapacityN + " >= " + Fw_Ten + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityN + " << " + Fw_Ten + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("On Net Area:");
			A_ch = colSplice.Channel.a * colSplice.NumberOfChannels;
			if (colSplice.BoltRowsWebLower >= 3)
				U = 0.85;
			else
				U = 0.75;
			Ae = U * (A_ch - 2 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * colSplice.Channel.tw;
			CapacityN = ConstNum.FIOMEGA0_75N * colSplice.Material.Fu * Ae;
			Reporting.AddLine("Ae = U * (Ag - 2 * Nc * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= " + Ae + ConstUnit.Area);

			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Fu * Ae = " + ConstString.FIOMEGA0_75 + "* " + colSplice.Material.Fu + " * " + Ae);
			if (CapacityN >= Fw_Ten)
				Reporting.AddLine("= " + CapacityN + " >= " + Fw_Ten + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityN + " << " + Fw_Ten + ConstUnit.Force + " (NG)");
		}

		private static void BoltsUpperWeb( ref double e, ref double s, ref double ce, ref double tw, ref double tf, ref double d, ref double a, double Fw_Ten, double Fw_Comp, ref double Fsplice, ref double FuT, ref double minsp, double dumy, ref double minedge, ref double capacity )
		{
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			e = colSplice.BoltVertEdgeDistancePlate;
			s = colSplice.BoltVertSpacing;
			ce = colSplice.BoltVertEdgeDistanceColumn;
			tw = colSplice.NumberOfChannels * colSplice.Channel.tw;
			tf = colSplice.Channel.tf;
			d = colSplice.Channel.d;
			a = colSplice.Channel.a;
			Fsplice = Math.Max(Fw_Ten, Fw_Comp);
			
			Reporting.AddHeader("Bolt Spacing:");
			FuT = Math.Min(tw * colSplice.Material.Fu, tColumn.Shape.tw * tColumn.Material.Fu);
			minsp = MiscCalculationsWithReporting.MinimumSpacing((colSplice.Bolt.BoltSize), (Fsplice / (2 * colSplice.BoltRowsWebUpper)), FuT, Math.Max(colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleWidth), colSplice.Bolt.HoleType);
			if (s >= minsp)
				Reporting.AddLine("s = " + s + " >= " + minsp + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("s = " + s + " << " + minsp + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Bolt Vertical Edge Dist. on Channel:");
			FuT = tw * colSplice.Material.Fu;
			minedge = MiscCalculationsWithReporting.MinimumEdgeDist(colSplice.Bolt.BoltSize, Fsplice / (2 * colSplice.BoltRowsWebUpper), FuT, colSplice.Bolt.MinEdgeSheared, (int)colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleType);
			if (e >= minedge)
				Reporting.AddLine("E = " + e + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("E = " + e + " << " + minedge + ConstUnit.Length + " (NG)");

			Reporting.AddLine("Bolt Vertical Edge Dist. on Column:");
			FuT = tColumn.Shape.tw * tColumn.Material.Fu;
			minedge = MiscCalculationsWithReporting.MinimumEdgeDist(colSplice.Bolt.BoltSize, Fsplice / (2 * colSplice.BoltRowsWebUpper), FuT, colSplice.Bolt.MinEdgeSheared, (int)colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleType);
			if (ce >= minedge)
				Reporting.AddLine("ce = " + ce + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("ce = " + ce + " << " + minedge + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Bolt Shear:");
			if (colSplice.Bolt.BoltType == EBoltType.SC || colSplice.FillerThicknessWebUpper <= ConstNum.QUARTER_INCH)
			{
				capacity = 2 * (colSplice.BoltRowsWebUpper + colSplice.FillerNumBoltRowsUW) * colSplice.Bolt.BoltStrength * colSplice.NumberOfChannels;
				Reporting.AddLine(ConstString.PHI + " Rn = 2 * N*Fv = 2 * " + (colSplice.BoltRowsWebUpper + colSplice.FillerNumBoltRowsUW) + " * " + (colSplice.Bolt.BoltStrength * colSplice.NumberOfChannels));
				if (capacity >= Fsplice)
					Reporting.AddLine("= " + capacity + " >= " + Fsplice + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + Fsplice + ConstUnit.Force + " (NG)");
			}
			else if (colSplice.FillerThicknessWebUpper <= 0.75 * ConstNum.ONE_INCH)
			{
				capacity = 2 * (colSplice.BoltRowsWebUpper + colSplice.FillerNumBoltRowsUW) * colSplice.Bolt.BoltStrength * colSplice.NumberOfChannels * (1 - 0.4 * (colSplice.FillerThicknessWebUpper - ConstNum.QUARTER_INCH));
				Reporting.AddLine(ConstString.PHI + " Rn = 2 * N * Fv * (1 - 0.4 * (t - " + ConstNum.QUARTER_INCH + "))");
				Reporting.AddLine("= 2 * " + (colSplice.BoltRowsWebUpper + colSplice.FillerNumBoltRowsUW) + " * " + (colSplice.Bolt.BoltStrength * colSplice.NumberOfChannels) + "*(1 - 0.4*(" + colSplice.FillerThicknessWebUpper + " - " + ConstNum.QUARTER_INCH + "))");
				if (capacity >= Fsplice)
					Reporting.AddLine("= " + capacity + " >= " + Fsplice + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + Fsplice + ConstUnit.Force + " (NG)");
			}
			else
			{
				capacity = 2 * (colSplice.BoltRowsWebUpper + colSplice.FillerNumBoltRowsUW) * colSplice.Bolt.BoltStrength * colSplice.NumberOfChannels;
				Reporting.AddLine(ConstString.PHI + " Rn = 2*(Ns + Nf)*Fv = 2*(" + colSplice.BoltRowsWebUpper + " + " + colSplice.FillerNumBoltRowsUW + ") * " + (colSplice.Bolt.BoltStrength * colSplice.NumberOfChannels));
				if (capacity >= Fsplice)
					Reporting.AddLine("= " + capacity + " >= " + Fsplice + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + Fsplice + ConstUnit.Force + " (NG)");
			}
		}

		private static void BoltBearingUpperColumnTension( ref double Fbre, ref double Fbrs, ref double CapacityBr, double Fw_Ten )
		{
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Bolt Bearing Under Tensile Force:");
			Reporting.AddHeader("Column Web:");
			Fbre = CommonCalculations.EdgeBearing(colSplice.BoltVertEdgeDistanceColumn, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, tColumn.Material.Fu, colSplice.Bolt.HoleType, true);
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, tColumn.Material.Fu, true);
			CapacityBr = 2 * (Fbre + (colSplice.BoltRowsWebUpper - 1) * Fbrs) * tColumn.Shape.tw;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Fbre + (N - 1) * Fbrs) * tw");
			Reporting.AddLine("= 2 * (" + Fbre + " + (" + colSplice.BoltRowsWebUpper + " - 1) * " + Fbrs + ") * " + tColumn.Shape.tw);
			if (CapacityBr >= Fw_Ten)
				Reporting.AddLine("= " + CapacityBr + " >= " + Fw_Ten + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + Fw_Ten + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Splice Channel:");
			Fbre = CommonCalculations.EdgeBearing(colSplice.BoltVertEdgeDistancePlate, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Material.Fu, colSplice.Bolt.HoleType, true);
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, true);
			CapacityBr = 2 * (Fbre + (colSplice.BoltRowsWebUpper - 1) * Fbrs) * colSplice.NumberOfChannels * colSplice.Channel.tw;
			Reporting.AddLine(ConstString.PHI + " Rn =  2 * (Fbre + (N - 1) * Fbrs) * Nc * t");
			Reporting.AddLine("= 2 * (" + Fbre + " + (" + colSplice.BoltRowsWebUpper + " - 1) * " + Fbrs + ") * " + colSplice.NumberOfChannels + " * " + colSplice.Channel.tw);
			if (CapacityBr >= Fw_Ten)
				Reporting.AddLine("= " + CapacityBr + " >= " + Fw_Ten + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + Fw_Ten + ConstUnit.Force + " (NG)");
		}

		private static void BoltBearingUpperColumnCompression( ref double Fbrs, ref double CapacityBr, double Fw_Comp, double Fw_Ten )
		{
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Bolt Bearing Under Compressive Force:");
			Reporting.AddHeader("Column Web:");
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, tColumn.Material.Fu, true);
			CapacityBr = 2 * colSplice.BoltRowsWebUpper * Fbrs * tColumn.Shape.tw;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * N* Fbrs * tw");
			Reporting.AddLine("= 2 * " + colSplice.BoltRowsWebUpper + " * " + Fbrs + " * " + tColumn.Shape.tw);
			if (CapacityBr >= Fw_Comp)
				Reporting.AddLine("= " + CapacityBr + " >= " + Fw_Comp + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + Fw_Comp + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Splice Channel:");
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, true);
			CapacityBr = 2 * colSplice.BoltRowsWebUpper * Fbrs * colSplice.NumberOfChannels * colSplice.Channel.tw;
			Reporting.AddLine(ConstString.PHI + " Rn =  2 * N * Fbrs * Nc * t");
			Reporting.AddLine("= 2 * " + colSplice.BoltRowsWebUpper + " * " + Fbrs + " * " + colSplice.NumberOfChannels + " * " + colSplice.Channel.tw);
			if (CapacityBr >= Fw_Ten)
				Reporting.AddLine("= " + CapacityBr + " >= " + Fw_Comp + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + Fw_Comp + ConstUnit.Force + " (NG)");
		}

		private static void BlockShearUpperColumn( ref double Lgt, ref double Lnt, ref double Lgv, ref double Lnv, ref double CapacityBS, double Fw_Ten, double Fw_Comp )
		{
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Block Shear:");
			Reporting.AddHeader("Column Web:");

			Lgt = colSplice.BoltGageWebUpper;
			Lnt = Lgt - (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
			Lgv = 2 * (colSplice.BoltVertEdgeDistanceColumn + (colSplice.BoltRowsWebUpper - 1) * colSplice.BoltVertSpacing);
			Lnv = Lgv - 2 * (colSplice.BoltRowsWebUpper - 0.5) * (colSplice.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
			Reporting.AddLine("Lgt = g = " + colSplice.BoltGageWebUpper + ConstUnit.Length);
			Reporting.AddLine("Lnt = Lgt - (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgt + " - (" + colSplice.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnt + ConstUnit.Length);
			Reporting.AddLine("Lgv = 2 * (e + (N - 1) * s) = 2 * (" + colSplice.BoltVertEdgeDistanceColumn + " + (" + colSplice.BoltRowsWebUpper + " - 1) * " + colSplice.BoltVertSpacing + ") = " + Lgv + ConstUnit.Length);
			Reporting.AddLine("Lnv = Lgv - 2 * (N - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgv + " - 2 * (" + colSplice.BoltRowsWebUpper + " - 0.5) * (" + colSplice.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnv + ConstUnit.Length);
			CapacityBS = MiscCalculationsWithReporting.BlockShearNew(tColumn.Material.Fu, Lnv, 1, Lnt, Lgv, tColumn.Material.Fy, tColumn.Shape.tw, Fw_Ten, true);
			
			if (CapacityBS >= Fw_Comp)
				Reporting.AddLine("= " + CapacityBS + " >= " + Fw_Ten + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBS + " << " + Fw_Ten + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Splice Channel:");
			Lgt = colSplice.BoltGageWebUpper;
			Lnt = Lgt - (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
			Lgv = 2 * (colSplice.BoltVertEdgeDistancePlate + (colSplice.BoltRowsWebUpper - 1) * colSplice.BoltVertSpacing);
			Lnv = Lgv - 2 * (colSplice.BoltRowsWebUpper - 0.5) * (colSplice.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
			Reporting.AddLine("Lgt = g = " + colSplice.BoltGageWebUpper + ConstUnit.Length);
			Reporting.AddLine("Lnt = Lgt - (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgt + " - (" + colSplice.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnt + ConstUnit.Length);
			Reporting.AddLine("Lgv = 2*(e + (N - 1)*s) = 2*(" + colSplice.BoltVertEdgeDistancePlate + " + (" + colSplice.BoltRowsWebUpper + " - 1) * " + colSplice.BoltVertSpacing + ") = " + Lgv + ConstUnit.Length);
			Reporting.AddLine("Lnv = Lgv - 2 * (N - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgv + " - 2 * (" + colSplice.BoltRowsWebUpper + " - 0.5) * (" + colSplice.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnv + ConstUnit.Length);
			CapacityBS = MiscCalculationsWithReporting.BlockShearNew(colSplice.Material.Fu, Lnv, 1, Lnt, Lgv, colSplice.Material.Fy, colSplice.Channel.tw, Fw_Ten, true);
			
			Reporting.AddLine("= " + CapacityBS + ConstUnit.Force);
			Reporting.AddLine("(" + colSplice.NumberOfChannels + "Channel(s)):");
			if (colSplice.NumberOfChannels * CapacityBS >= Fw_Ten)
				Reporting.AddLine("= " + (colSplice.NumberOfChannels * CapacityBS) + " >= " + Fw_Ten + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + (colSplice.NumberOfChannels * CapacityBS) + " << " + Fw_Ten + ConstUnit.Force + " (NG)");
		}

		private static void PlateTensionGross( ref double A_ch, double Fw_Ten )
		{
			double CapacityG = 0;

			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Splice Channel Tension On Gross Area:");
			A_ch = colSplice.Channel.a * colSplice.NumberOfChannels;
			CapacityG = ConstNum.FIOMEGA0_9N * colSplice.Material.Fy * A_ch;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + "  * Fy * Ag = " + ConstString.FIOMEGA0_9 + "  * " + colSplice.Material.Fy + " * " + A_ch);
			if (CapacityG >= Fw_Ten)
				Reporting.AddLine("= " + CapacityG + " >= " + Fw_Ten + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityG + " << " + Fw_Ten + ConstUnit.Force + " (NG)");
		}

		private static void WeldsUpperWeb( ref double d, ref double tw, ref double FWeldSizeChToCol, ref double FWeldSizeChToFiller, ref double FWeldSizeFillerToCol, ref double usefulweldonPlate, ref double capacity, double Fw )
		{
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			d = colSplice.Channel.d;
			tw = colSplice.Channel.tw;
			FWeldSizeChToCol = CommonCalculations.MinimumWeld(tColumn.Shape.tw, d);
			FWeldSizeChToFiller = CommonCalculations.MinimumWeld(colSplice.FillerThicknessWebUpper, tw);
			FWeldSizeFillerToCol = CommonCalculations.MinimumWeld(tColumn.Shape.tw, colSplice.FillerThicknessWebUpper);

			Reporting.AddHeader("Welds:");
			if (colSplice.FillerThicknessWebUpper == 0)
			{
				if (FWeldSizeChToCol <= colSplice.FilletWeldSizeWebUpper)
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(FWeldSizeChToCol) + " <= " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebUpper) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(FWeldSizeChToCol) + " >> " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebUpper) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Splice Channel:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * tw, tColumn.Material.Fu * tColumn.Shape.tw / colSplice.NumberOfChannels) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t; Fuc * tw') / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + tw + "; " + tColumn.Material.Fu + " * " + (tColumn.Shape.tw / colSplice.NumberOfChannels) + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebUpper)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FilletWeldSizeWebUpper + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYUW + colSplice.WeldLengthXUW) * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * wu * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYUW + " + " + colSplice.WeldLengthXUW + ") * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FilletWeldSizeWebUpper + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYUW + colSplice.WeldLengthXUW) * colSplice.FilletWeldSizeWebUpper * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * w * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYUW + " + " + colSplice.WeldLengthXUW + ") * " + colSplice.FilletWeldSizeWebUpper + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= Fw / colSplice.NumberOfChannels)
					Reporting.AddLine("= " + capacity + " >= Fw = " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << Fw = " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (NG)");
			}
			else if (colSplice.FillerThicknessWebUpper < ConstNum.QUARTER_INCH)
			{
				Reporting.AddHeader("Useful Weld Size on Splice Channel & Filler:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * tw, tColumn.Material.Fu * tColumn.Shape.tw / colSplice.NumberOfChannels) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t; Fuc * tw') / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + tw + "; " + tColumn.Material.Fu + " * " + (tColumn.Shape.tw / colSplice.NumberOfChannels) + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebUpper - colSplice.FillerThicknessWebUpper)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w - t_fill = " + (colSplice.FilletWeldSizeWebUpper - colSplice.FillerThicknessWebUpper) + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYUW + colSplice.WeldLengthXUW) * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * wu * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYUW + " + " + colSplice.WeldLengthXUW + ") * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w - t_fill = " + (colSplice.FilletWeldSizeWebUpper - colSplice.FillerThicknessWebUpper) + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYUW + colSplice.WeldLengthXUW) * (colSplice.FilletWeldSizeWebUpper - colSplice.FillerThicknessWebUpper) * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * (w-t_fill)) * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYUW + " + " + colSplice.WeldLengthXUW + ") * " + (colSplice.FilletWeldSizeWebUpper - colSplice.FillerThicknessWebUpper) + ") * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= Fw / colSplice.NumberOfChannels)
					Reporting.AddLine("= " + capacity + " >= Fw = " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << Fw = " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (NG)");
			}
			else
			{
				if (FWeldSizeChToFiller <= colSplice.FilletWeldSizeWebUpper)
					Reporting.AddLine("Min. Weld Size (Channel/Filler) = " + CommonCalculations.WeldSize(FWeldSizeChToFiller) + " <= " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebUpper) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size(Channel/Filler) = " + CommonCalculations.WeldSize(FWeldSizeChToFiller) + " >> " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebUpper) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Splice Channel/Filler:");
				usefulweldonPlate = colSplice.Material.Fu * Math.Min(tw, colSplice.FillerThicknessWebUpper) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Fu * Min(t; t_fill) / (0.707* Fexx)");
				Reporting.AddLine("= " + colSplice.Material.Fu + " * Min(" + tw + "; " + colSplice.FillerThicknessWebUpper + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebUpper)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FilletWeldSizeWebUpper + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYUW + colSplice.WeldLengthXUW) * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * wu * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYUW + " + " + colSplice.WeldLengthXUW + ") * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FilletWeldSizeWebUpper + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYUW + colSplice.WeldLengthXUW) * colSplice.FilletWeldSizeWebUpper * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * w * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYUW + " + " + colSplice.WeldLengthXUW + ") * " + colSplice.FilletWeldSizeWebUpper + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= Fw / colSplice.NumberOfChannels)
					Reporting.AddLine("= " + capacity + " >= " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (NG)");

				if (FWeldSizeFillerToCol <= colSplice.FillerWeldLengthUW)
					Reporting.AddLine("Min. Weld Size (Filler/Web) = " + CommonCalculations.WeldSize(FWeldSizeFillerToCol) + " <= " + CommonCalculations.WeldSize(colSplice.FillerWeldLengthUW) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size (Filler/Web) = " + CommonCalculations.WeldSize(FWeldSizeFillerToCol) + " >> " + CommonCalculations.WeldSize(colSplice.FillerWeldLengthUW) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Filler/Web:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * colSplice.FillerThicknessWebUpper, tColumn.Material.Fu * tColumn.Shape.tw / colSplice.NumberOfChannels) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t_fill; Fuc * tw) / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + colSplice.FillerThicknessWebUpper + "; " + tColumn.Material.Fu + " * " + (tColumn.Shape.tw / colSplice.NumberOfChannels) + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FillerWeldLengthUW)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FillerWeldLengthUW + ConstUnit.Length);
					capacity = 2 * colSplice.FillerWeldLengthUW * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * Lb * wu * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * " + colSplice.FillerWeldLengthUW + " * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FillerWeldLengthUW + ConstUnit.Length);
					capacity = 2 * colSplice.FillerWeldLengthUW * colSplice.FillerWeldLengthUW * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * Lb * w * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * " + colSplice.FillerWeldLengthUW + " * " + colSplice.FillerWeldLengthUW + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= Fw / colSplice.NumberOfChannels)
					Reporting.AddLine("= " + capacity + " >= Fw = " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << Fw = " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (NG)");
			}
		}

		private static void PlateTensionWeldedNetUpperColumn( ref double d, ref double A_ch, ref double U, ref double CapacityN, double Fw_Ten )
		{
			var colSplice = CommonDataStatic.ColumnSplice;

			d = colSplice.Channel.d;
			A_ch = colSplice.Channel.a * colSplice.NumberOfChannels;
			U = 0.85;
			Reporting.AddLine("");
			Reporting.AddLine("On Effective Net Area:");
			Reporting.AddLine("");
			CapacityN = ConstNum.FIOMEGA0_75N * colSplice.Material.Fu * U * A_ch;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + "* Fu * U * Ag");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + "* " + colSplice.Material.Fu + " * " + U + " * " + A_ch);
			if (CapacityN >= Fw_Ten)
				Reporting.AddLine("= " + CapacityN + " >= " + Fw_Ten + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityN + " << " + Fw_Ten + ConstUnit.Force + " (NG)");
		}

		private static void BoltsLowerWeb( ref double e, ref double s, ref double ce, ref double tw, ref double tf, ref double d, ref double a, double Fw_Ten, double Fw_Comp, ref double Fsplice, ref double FuT, ref double minsp, double dumy, ref double minedge, ref double capacity )
		{
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			e = colSplice.BoltVertEdgeDistancePlate;
			s = colSplice.BoltVertSpacing;
			ce = colSplice.BoltVertEdgeDistanceColumn;
			tw = colSplice.Channel.tw * colSplice.NumberOfChannels;
			tf = colSplice.Channel.tf;
			d = colSplice.Channel.d;
			a = colSplice.Channel.a * colSplice.NumberOfChannels;
			Fsplice = Math.Max(Fw_Ten, Fw_Comp);

			Reporting.AddHeader("Bolt Spacing:");
			FuT = Math.Min(tw * colSplice.Material.Fu, bColumn.Shape.tw * bColumn.Material.Fu);
			minsp = MiscCalculationsWithReporting.MinimumSpacing((colSplice.Bolt.BoltSize), (Fsplice / (2 * colSplice.Bolt.BoltStrength)), FuT, Math.Max(colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleLength), colSplice.Bolt.HoleType);
			if (s >= minsp)
				Reporting.AddLine("s = " + s + " >= " + minsp + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("s = " + s + " << " + minsp + ConstUnit.Length + " (NG)");

			Reporting.AddLine("Bolt Vertical Edge Dist. on Channel:");
			FuT = tw * colSplice.Material.Fu;
			minedge = MiscCalculationsWithReporting.MinimumEdgeDist(colSplice.Bolt.BoltSize, Fsplice / (2 * colSplice.BoltRowsWebLower), FuT, colSplice.Bolt.MinEdgeSheared, (int)colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleType);
			if (e >= minedge)
				Reporting.AddLine("E = " + e + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("E = " + e + " << " + minedge + ConstUnit.Length + " (NG)");

			Reporting.AddLine("Bolt Vertical Edge Dist. on Column:");
			FuT = bColumn.Shape.tw * bColumn.Material.Fu;
			minedge = MiscCalculationsWithReporting.MinimumEdgeDist(colSplice.Bolt.BoltSize, Fsplice / (2 * colSplice.BoltRowsWebLower), FuT, colSplice.Bolt.MinEdgeSheared, (int)colSplice.Bolt.HoleLength, colSplice.Bolt.HoleType);
			if (ce >= minedge)
				Reporting.AddLine("ce = " + ce + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("ce = " + ce + " << " + minedge + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Bolt Shear:");
			if (colSplice.Bolt.BoltType == EBoltType.SC || colSplice.FillerThicknessWebLower <= ConstNum.QUARTER_INCH)
			{
				capacity = 2 * (colSplice.BoltRowsWebLower + colSplice.FillerNumBoltRowsLW) * colSplice.Bolt.BoltStrength * colSplice.NumberOfChannels;
				Reporting.AddLine(ConstString.PHI + " Rn = 2 * N*Fv = 2 * " + (colSplice.BoltRowsWebLower + colSplice.FillerNumBoltRowsLW) + " * " + (colSplice.Bolt.BoltStrength * colSplice.NumberOfChannels));
				if (capacity >= Fsplice)
					Reporting.AddLine("= " + capacity + " >= " + Fsplice + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + Fsplice + ConstUnit.Force + " (NG)");
			}
			else if (colSplice.FillerThicknessWebLower <= 0.75 * ConstNum.ONE_INCH)
			{
				capacity = 2 * (colSplice.BoltRowsWebLower + colSplice.FillerNumBoltRowsLW) * colSplice.Bolt.BoltStrength * colSplice.NumberOfChannels * (1 - 0.4 * (colSplice.FillerThicknessWebLower - ConstNum.QUARTER_INCH));
				Reporting.AddLine(ConstString.PHI + " Rn = 2 * N * Fv * (1 - 0.4 * (t - " + ConstNum.QUARTER_INCH + "))");
				Reporting.AddLine("= 2 * " + (colSplice.BoltRowsWebLower + colSplice.FillerNumBoltRowsLW) + " * " + (colSplice.Bolt.BoltStrength * colSplice.NumberOfChannels) + "*(1 - 0.4*(" + colSplice.FillerThicknessWebLower + " - " + ConstNum.QUARTER_INCH + "))");
				if (capacity >= Fsplice)
					Reporting.AddLine("= " + capacity + " >= " + Fsplice + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + Fsplice + ConstUnit.Force + " (NG)");
			}
			else
			{
				capacity = 2 * (colSplice.BoltRowsWebLower + colSplice.FillerNumBoltRowsLW) * colSplice.Bolt.BoltStrength * colSplice.NumberOfChannels;
				Reporting.AddLine(ConstString.PHI + " Rn = 2*(Ns + Nf)*Fv = 2*(" + colSplice.BoltRowsWebLower + " + " + colSplice.FillerNumBoltRowsLW + ") * " + (colSplice.Bolt.BoltStrength * colSplice.NumberOfChannels));
				if (capacity >= Fsplice)
					Reporting.AddLine("= " + capacity + " >= " + Fsplice + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + Fsplice + ConstUnit.Force + " (NG)");
			}
		}

		private static void BoltBearingLowerColumnTension( ref double Fbre, ref double Fbrs, ref double CapacityBr, double Fw_Ten )
		{
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Bolt Bearing Under Tensile Force:");
			Reporting.AddHeader("Column Web:");
			Fbre = CommonCalculations.EdgeBearing(colSplice.BoltVertEdgeDistanceColumn, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, bColumn.Material.Fu, colSplice.Bolt.HoleType, true);
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, bColumn.Material.Fu, true);
			CapacityBr = 2 * (Fbre + (colSplice.BoltRowsWebLower - 1) * Fbrs) * bColumn.Shape.tw;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Fbre + (N - 1) * Fbrs) * tw");
			Reporting.AddLine("= 2 * (" + Fbre + " + (" + colSplice.BoltRowsWebLower + " - 1) * " + Fbrs + ") * " + bColumn.Shape.tw);
			if (CapacityBr >= Fw_Ten)
				Reporting.AddLine("= " + CapacityBr + " >= " + Fw_Ten + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + Fw_Ten + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Splice Channel:");
			Fbre = CommonCalculations.EdgeBearing(colSplice.BoltVertEdgeDistancePlate, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Material.Fu, colSplice.Bolt.HoleType, true);
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, true);
			CapacityBr = 2 * (Fbre + (colSplice.BoltRowsWebLower - 1) * Fbrs) * colSplice.NumberOfChannels * colSplice.Channel.tw;
			Reporting.AddLine(ConstString.PHI + " Rn =  2 * (Fbre + (N - 1) * Fbrs) * t");
			Reporting.AddLine("= 2 * (" + Fbre + " + (" + colSplice.BoltRowsWebLower + " - 1) * " + Fbrs + ") * " + colSplice.NumberOfChannels + " * " + colSplice.Channel.tw);
			if (CapacityBr >= Fw_Ten)
				Reporting.AddLine("= " + CapacityBr + " >= " + Fw_Ten + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + Fw_Ten + ConstUnit.Force + " (NG)");
		}

		private static void BoltBearingLowerColumnCompression( ref double Fbrs, ref double CapacityBr, double Fw_Comp )
		{
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Bolt Bearing Under Compressive Force:");
			Reporting.AddHeader("Column Web:");
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleLength, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, bColumn.Material.Fu, true);
			CapacityBr = 2 * colSplice.BoltRowsWebLower * Fbrs * bColumn.Shape.tw;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * N* Fbrs * tw");
			Reporting.AddLine("= 2 * " + colSplice.BoltRowsWebLower + " * " + Fbrs + " * " + bColumn.Shape.tw);
			if (CapacityBr >= Fw_Comp)
				Reporting.AddLine("= " + CapacityBr + " >= " + Fw_Comp + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + Fw_Comp + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Splice Channel:");
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, true);
			CapacityBr = 2 * colSplice.BoltRowsWebLower * Fbrs * colSplice.NumberOfChannels * colSplice.Channel.tw;
			Reporting.AddLine(ConstString.PHI + " Rn =  2 * N * Fbrs * Nc * t");
			Reporting.AddLine("= 2 * " + colSplice.BoltRowsWebLower + " * " + Fbrs + " * " + colSplice.NumberOfChannels + " * " + colSplice.Channel.tw);
			if (CapacityBr >= Fw_Comp)
				Reporting.AddLine("= " + CapacityBr + " >= " + Fw_Comp + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + Fw_Comp + ConstUnit.Force + " (NG)");
		}

		private static void BlockShearLowerColumn( ref double Lgt, ref double Lnt, ref double Lgv, ref double Lnv, ref double CapacityBS, double Fw_Ten )
		{
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Block  Shear:");
			Reporting.AddHeader("Column Web:");
			Lgt = colSplice.BoltGageWebLower;
			Lnt = Lgt - (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
			Lgv = 2 * (colSplice.BoltVertEdgeDistanceColumn + (colSplice.BoltRowsWebLower - 1) * colSplice.BoltVertSpacing);
			Lnv = Lgv - 2 * (colSplice.BoltRowsWebLower - 0.5) * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
			Reporting.AddLine("Lgt = g = " + colSplice.BoltGageWebLower + ConstUnit.Length);
			Reporting.AddLine("Lnt = Lgt - (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgt + " - (" + colSplice.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnt + ConstUnit.Length);
			Reporting.AddLine("Lgv = 2 * (e + (N - 1) * s) = 2 * (" + colSplice.BoltVertEdgeDistanceColumn + " + (" + colSplice.BoltRowsWebLower + " - 1) * " + colSplice.BoltVertSpacing + ") = " + Lgv + ConstUnit.Length);
			Reporting.AddLine("Lnv = Lgv - 2 * (N - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgv + " - 2 * (" + colSplice.BoltRowsWebLower + " - 0.5) * (" + colSplice.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnv + ConstUnit.Length);
			CapacityBS = MiscCalculationsWithReporting.BlockShearNew(bColumn.Material.Fu, Lnv, 1, Lnt, Lgv, bColumn.Material.Fy, bColumn.Shape.tw, Fw_Ten, true);
			
			if (CapacityBS >= Fw_Ten)
				Reporting.AddLine("= " + CapacityBS + " >= " + Fw_Ten + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBS + " << " + Fw_Ten + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Splice Channel:");
			Lgt = colSplice.BoltGageWebLower;
			Lnt = Lgt - (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
			Lgv = 2 * (colSplice.BoltVertEdgeDistancePlate + (colSplice.BoltRowsWebLower - 1) * colSplice.BoltVertSpacing);
			Lnv = Lgv - 2 * (colSplice.BoltRowsWebLower - 0.5) * (colSplice.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
			Reporting.AddLine("Lgt = g = " + colSplice.BoltGageWebLower + ConstUnit.Length);
			Reporting.AddLine("Lnt = Lgt - (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgt + " - (" + colSplice.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnt + ConstUnit.Length);
			Reporting.AddLine("Lgv = 2*(e + (N - 1) * s) = 2*(" + colSplice.BoltVertEdgeDistancePlate + " + (" + colSplice.BoltRowsWebLower + " - 1) * " + colSplice.BoltVertSpacing + ") = " + Lgv + ConstUnit.Length);
			Reporting.AddLine("Lnv = Lgv - 2 * (N - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgv + " - 2 * (" + colSplice.BoltRowsWebLower + " - 0.5) * (" + colSplice.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnv + ConstUnit.Length);
			CapacityBS = MiscCalculationsWithReporting.BlockShearNew(colSplice.Material.Fu, Lnv, 1, Lnt, Lgv, colSplice.Material.Fy, colSplice.Channel.tw, Fw_Ten, true);
			
			Reporting.AddLine("= " + CapacityBS + ConstUnit.Force);
			Reporting.AddLine("(" + colSplice.NumberOfChannels + "Channel(s)):");
			if (colSplice.NumberOfChannels * CapacityBS >= Fw_Ten)
				Reporting.AddLine("= " + (colSplice.NumberOfChannels * CapacityBS) + " >= " + Fw_Ten + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + (colSplice.NumberOfChannels * CapacityBS) + " << " + Fw_Ten + ConstUnit.Force + " (NG)");
		}

		private static void WeldsLowerWeb( ref double d, ref double tw, ref double FWeldSizeChToCol, ref double FWeldSizeChToFiller, ref double FWeldSizeFillerToCol, ref double usefulweldonPlate, ref double capacity, double Fw )
		{
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			d = colSplice.Channel.d;
			tw = colSplice.Channel.tw;
			FWeldSizeChToCol = CommonCalculations.MinimumWeld(bColumn.Shape.tw, d);
			FWeldSizeChToFiller = CommonCalculations.MinimumWeld(colSplice.FillerThicknessWebLower, tw);
			FWeldSizeFillerToCol = CommonCalculations.MinimumWeld(bColumn.Shape.tw, colSplice.FillerThicknessWebLower);

			Reporting.AddHeader("Welds:");
			if (colSplice.FillerThicknessWebLower == 0)
			{
				if (FWeldSizeChToCol <= colSplice.FilletWeldSizeWebLower)
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(FWeldSizeChToCol) + " <= " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebLower) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(FWeldSizeChToCol) + " >> " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebLower) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Splice Channel:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * tw, bColumn.Material.Fu * bColumn.Shape.tw / colSplice.NumberOfChannels) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t, Fuc * tw') / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + tw + "; " + bColumn.Material.Fu + " * " + (bColumn.Shape.tw / colSplice.NumberOfChannels) + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebLower)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FilletWeldSizeWebLower + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYLW + colSplice.WeldLengthXLW) * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine("");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * wu * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYLW + " + " + colSplice.WeldLengthXLW + ") * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FilletWeldSizeWebLower + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYLW + colSplice.WeldLengthXLW) * colSplice.FilletWeldSizeWebLower * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine("");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * w * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYLW + " + " + colSplice.WeldLengthXLW + ") * " + colSplice.FilletWeldSizeWebLower + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= Fw / colSplice.NumberOfChannels)
					Reporting.AddLine("= " + capacity + " >= Fw = " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << Fw = " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (NG)");
			}
			else if (colSplice.FillerThicknessWebLower < ConstNum.QUARTER_INCH)
			{
				Reporting.AddHeader("Useful Weld Size on Splice Channel & Filler:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * tw, bColumn.Material.Fu * bColumn.Shape.tw / colSplice.NumberOfChannels) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t, Fuc * tw') / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + tw + "; " + bColumn.Material.Fu + " * " + (bColumn.Shape.tw / colSplice.NumberOfChannels) + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebLower - colSplice.FillerThicknessWebLower)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w - t_fill = " + (colSplice.FilletWeldSizeWebLower - colSplice.FillerThicknessWebLower) + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYLW + colSplice.WeldLengthXLW) * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * wu * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYLW + " + " + colSplice.WeldLengthXLW + ") * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w - t_fill = " + (colSplice.FilletWeldSizeWebLower - colSplice.FillerThicknessWebLower) + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYLW + colSplice.WeldLengthXLW) * (colSplice.FilletWeldSizeWebLower - colSplice.FillerThicknessWebLower) * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * (w-t_fill)) * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYLW + " + " + colSplice.WeldLengthXLW + ") * " + (colSplice.FilletWeldSizeWebLower - colSplice.FillerThicknessWebLower) + ") * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= Fw / colSplice.NumberOfChannels)
					Reporting.AddLine("= " + capacity + " >= Fw = " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << Fw = " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (NG)");
			}
			else
			{
				if (FWeldSizeChToFiller <= colSplice.FilletWeldSizeWebLower)
					Reporting.AddLine("Min. Weld Size (Channel/Filler) = " + CommonCalculations.WeldSize(FWeldSizeChToFiller) + " <= " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebLower) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size(Channel/Filler) = " + CommonCalculations.WeldSize(FWeldSizeChToFiller) + " >> " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeWebLower) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Splice Channel/Filler:");
				usefulweldonPlate = colSplice.Material.Fu * Math.Min(tw, colSplice.FillerThicknessWebLower) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Fu * Min(t, t_fill) / (0.707 * Fexx)");
				Reporting.AddLine("= " + colSplice.Material.Fu + " * Min(" + tw + "; " + colSplice.FillerThicknessWebLower + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeWebLower)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FilletWeldSizeWebLower + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYLW + colSplice.WeldLengthXLW) * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * wu * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYLW + " + " + colSplice.WeldLengthXLW + ") * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FilletWeldSizeWebLower + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYLW + colSplice.WeldLengthXLW) * colSplice.FilletWeldSizeWebLower * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine("");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * w * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYLW + " + " + colSplice.WeldLengthXLW + ") * " + colSplice.FilletWeldSizeWebLower + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= Fw / colSplice.NumberOfChannels)
					Reporting.AddLine("= " + capacity + " >= " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (NG)");

				if (FWeldSizeFillerToCol <= colSplice.FillerWeldSizeLW)
					Reporting.AddLine("Min. Weld Size (Filler/Web) = " + CommonCalculations.WeldSize(FWeldSizeFillerToCol) + " <= " + CommonCalculations.WeldSize(colSplice.FillerWeldSizeLW) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size (Filler/Web) = " + CommonCalculations.WeldSize(FWeldSizeFillerToCol) + " >> " + CommonCalculations.WeldSize(colSplice.FillerWeldSizeLW) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Filler/Web:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * colSplice.FillerThicknessWebLower, bColumn.Material.Fu * bColumn.Shape.tw / colSplice.NumberOfChannels) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t_fill; Fuc * tw) / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + colSplice.FillerThicknessWebLower + "; " + bColumn.Material.Fu + " * " + (bColumn.Shape.tw / colSplice.NumberOfChannels) + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FillerWeldSizeLW)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FillerWeldSizeLW + ConstUnit.Length);
					capacity = 2 * colSplice.FillerWeldLengthLW * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * Lb * wu * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * " + colSplice.FillerWeldLengthLW + " * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FillerWeldSizeLW + ConstUnit.Length);
					capacity = 2 * colSplice.FillerWeldLengthLW * colSplice.FillerWeldSizeLW * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * Lb * w * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * " + colSplice.FillerWeldLengthLW + " * " + colSplice.FillerWeldSizeLW + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= Fw / colSplice.NumberOfChannels)
					Reporting.AddLine("= " + capacity + " >= Fw = " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << Fw = " + (Fw / colSplice.NumberOfChannels) + ConstUnit.Force + " (NG)");
			}
		}

		private static void PlateTensionWeldedNetLowerColumn( ref double d, ref double A_ch, ref double U, ref double CapacityN, double Fw_Ten )
		{
			var colSplice = CommonDataStatic.ColumnSplice;

			d = colSplice.Channel.d;
			A_ch = colSplice.Channel.a * colSplice.NumberOfChannels;
			U = 0.85;
			Reporting.AddHeader("On Effective Net Area:");
			CapacityN = ConstNum.FIOMEGA0_75N * colSplice.Material.Fu * U * A_ch;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + "* Fu * U * Ag");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + "* " + colSplice.Material.Fu + " * " + U + " * " + A_ch);
			if (CapacityN >= Fw_Ten)
				Reporting.AddLine("= " + CapacityN + " >= " + Fw_Ten + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityN + " << " + Fw_Ten + ConstUnit.Force + " (NG)");
		}
	}
}