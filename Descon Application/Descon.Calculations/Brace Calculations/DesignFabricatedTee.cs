using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class DesignFabricatedTee
	{
		internal static void CalcDesignFabricatedTee(EMemberType memberType, double H_Tens, double H_Comp, double V, int Mom)
		{
			double usefulweldsize = 0;
			double f_peak = 0;
			double Stress = 0;
			double Moment = 0;
			string Momeffect = "";
			double edgeActual = 0;
			double edgemin = 0;
			double edgeminComparison = 0;
			double Smin = 0;
			double SminGusset = 0;
			double SminPlate = 0;
			double CapacityforShearYielding = 0;
			double CapacityforFlexuralYielding = 0;
			double WeldCapacity = 0;
			double capacity = 0;
			double f_Combined = 0;
			double fvv = 0;
			double fh = 0;
			double Mfws = 0;
			double w = 0;
			double fr = 0;
			double Fb = 0;
			double tminimum = 0;
			double spt = 0;
			double spt3 = 0;
			double Moment2 = 0;
			double SecMod = 0;
			double MomInert = 0;
			double E = 0;
			double S = 0;
			double N = 0;
			double d = 0;
			double L = 0;
			double BearingCapacityH = 0;
			double BearingCapacityV = 0;
			double EquivBoltFactor = 0;
			double Fbe = 0;
			double Fbs = 0;
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
			double Thickness = 0;
			double wmax = 0;
			double minweld = 0;
			double RequiredWeldsize = 0;
			double ShearCap = 0;
			double c = 0;
			double Theta = 0;
			double ex = 0;
			int Nvl = 0;
			int Nbw = 0;
			double ercl = 0;
			double Th = 0;
			double Nmn = 0;
			double ming = 0;
			double tRequiredforShearYielding = 0;
			double tRequiredforYielding = 0;
			double MinLengthofFabTee = 0;
			double WeldAreaRequired = 0;
			double Rm_Prev = 0;
			double Rm = 0;
			double H = 0;
			double R = 0;
			double BF = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];

			if (component.BraceConnect.FabricatedTee.Length == 0)
				component.BraceConnect.FabricatedTee.Length = component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir + 2 * component.BraceConnect.FabricatedTee.Ev1;

			H = component.BraceConnect.Gusset.Hc;
			BF = component.WebOrientation == EWebOrientation.InPlane ? component.Shape.bf : component.Shape.d;
			R = Math.Sqrt(H * H + V * V);
			Rm = Math.Sqrt(Math.Pow(H + 3 * Mom / component.BraceConnect.FabricatedTee.Length, 2) + V * V);
			// FabTee Flange:
			if ((component.WebOrientation == EWebOrientation.InPlane) && !component.BraceConnect.FabricatedTee.bf_User)
				component.BraceConnect.FabricatedTee.bf = (Math.Floor(4 * component.Shape.bf)) / 4;
			else
                if (!component.BraceConnect.FabricatedTee.bf_User) component.BraceConnect.FabricatedTee.bf = (Math.Floor(4 * component.Shape.d)) / 4.0;

			component.BraceConnect.FabricatedTee.FlangeWeld = EWeldType.CJP;
            if (!component.BraceConnect.FabricatedTee.FlangeWeldSize_User) 
                component.BraceConnect.FabricatedTee.FlangeWeldSize = NumberFun.ConvertFromFraction((5 / 8.0) * component.Shape.tf);
			Rm = R;

			do
			{
				Rm_Prev = Rm;
				WeldAreaRequired = Rm / (ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				MinLengthofFabTee = WeldAreaRequired / (component.BraceConnect.FabricatedTee.FlangeWeldSize * 2);
				if (double.IsNaN(MinLengthofFabTee))
					MinLengthofFabTee = 0;
				if (component.BraceConnect.FabricatedTee.Length < MinLengthofFabTee)
					component.BraceConnect.FabricatedTee.Length = MinLengthofFabTee;
				Rm = Math.Sqrt(Math.Pow(H + 3 * Mom / component.BraceConnect.FabricatedTee.Length, 2) + V * V);

			} while (Rm != Rm_Prev);
			// Flexural yielding of Tee Flange
			if (component.WebOrientation == EWebOrientation.InPlane)
				tRequiredforYielding = Math.Sqrt((H + 3 * Mom / component.BraceConnect.FabricatedTee.Length) * Math.Min(component.Shape.bf, component.BraceConnect.FabricatedTee.bf) / (ConstNum.FIOMEGA0_95N * component.BraceConnect.FabricatedTee.Material.Fy * component.BraceConnect.FabricatedTee.Length));
			else
				tRequiredforYielding = Math.Sqrt((H + 3 * Mom / component.BraceConnect.FabricatedTee.Length) * Math.Min(component.Shape.d, component.BraceConnect.FabricatedTee.bf) / (ConstNum.FIOMEGA0_95N * component.BraceConnect.FabricatedTee.Material.Fy * component.BraceConnect.FabricatedTee.Length));
			// Shear Yielding of Tee Flange
			tRequiredforShearYielding = Rm / (2 * ConstNum.FIOMEGA1_0N * 0.6 * component.BraceConnect.FabricatedTee.Material.Fy * component.BraceConnect.FabricatedTee.Length);
            if (!component.BraceConnect.FabricatedTee.Tf_User) component.BraceConnect.FabricatedTee.Tf = CommonCalculations.PlateThickness(Math.Max(tRequiredforYielding, Math.Max(ConstNum.QUARTER_INCH, tRequiredforShearYielding)));
			ming = component.BraceConnect.FabricatedTee.Tf - (Math.Ceiling(component.BraceConnect.FabricatedTee.StemWeldSize * 4)) / 4.0;
			if (!MiscMethods.IsBeamOrColumn(memberType))
			{
				if (component.BraceConnect.Gusset.ColumnSideSetback < ming && !component.BraceConnect.Gusset.ColumnSideSetback_User)
					component.BraceConnect.Gusset.ColumnSideSetback = ming;
			}
			else
			{
				if (!component.EndSetback_User && component.EndSetback < ming)
					component.EndSetback = ming;
			}
			// Stem design: similar to single plate
			component.GussetWeldedToColumn = false;
			// Bolts
			if (MiscMethods.IsBeamOrColumn(memberType))
			{
				if (component.BraceConnect.FabricatedTee.Bolt.NumberOfRows == 0)
                    if (!component.BraceConnect.FabricatedTee.FirstBoltDistance_User) 
                        component.BraceConnect.FabricatedTee.FirstBoltDistance = Math.Floor(component.Shape.kdet + component.BraceConnect.FabricatedTee.Ev1);
				else
                    if (!component.BraceConnect.FabricatedTee.FirstBoltDistance_User) 
                        component.BraceConnect.FabricatedTee.FirstBoltDistance = component.Shape.d / 2 - (component.BraceConnect.FabricatedTee.Bolt.NumberOfRows - 1) * component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir / 2;
				Nmn = Math.Ceiling((component.Shape.t / 2) / component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir);
				Th = component.Shape.tw;
				ercl = component.EndSetback;
			}
			else
			{
				Nmn = (int)Math.Ceiling((component.BraceConnect.Gusset.ColumnSide / 2) / component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir);
				Th = component.BraceConnect.Gusset.Thickness;
				// Gusset(m).ColumnSideSetback = Max(0.5, -Int(-4 * (fabtee(m).WeldSize + 0.125)) / 4)
				ercl = component.BraceConnect.Gusset.ColumnSideSetback;
			}

			if (Nmn < 2)
				Nmn = 2;

			Nbw = component.BraceConnect.FabricatedTee.Bolt.NumberOfRows;
			Nvl = component.BraceConnect.FabricatedTee.Bolt.NumberOfLines;
			ex = (ercl + component.BraceConnect.FabricatedTee.Eh2 + (Nvl - 1) * component.BraceConnect.FabricatedTee.Bolt.SpacingTransvDir / 2);
			if (Mom > 0 && V > 0)
			{
				ex = ex + Mom / V;
				Theta = Math.Atan(Math.Abs(H / V));
			}

			c = SmallMethodsDesign.EccIncLoadOnBG(Nvl, Nbw, ex, Theta, ((int) component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir), ((int) component.BraceConnect.FabricatedTee.Bolt.SpacingTransvDir));
			ShearCap = c * component.BraceConnect.FabricatedTee.Bolt.BoltStrength;

			component.BraceConnect.FabricatedTee.Bolt.NumberOfBolts = component.BraceConnect.FabricatedTee.Bolt.NumberOfLines * component.BraceConnect.FabricatedTee.Bolt.NumberOfRows;
			component.BraceConnect.FabricatedTee.Bolt.NumberOfRows = component.BraceConnect.FabricatedTee.Bolt.NumberOfRows;
			component.BraceConnect.FabricatedTee.D = ercl + component.BraceConnect.FabricatedTee.Eh2 + (Nvl - 1) * component.BraceConnect.FabricatedTee.Bolt.SpacingTransvDir + component.BraceConnect.FabricatedTee.Eh1;
			component.BraceConnect.FabricatedTee.Length = Math.Max(MinLengthofFabTee, component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir * (Nbw - 1) + 2 * component.BraceConnect.FabricatedTee.Ev1);

			WeldAreaRequired = Rm / (ConstNum.FIOMEGA0_75N * 0.6 * 0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
			RequiredWeldsize = NumberFun.ConvertFromFraction(Rm / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * component.BraceConnect.FabricatedTee.Length));
			minweld = CommonCalculations.MinimumWeld(component.BraceConnect.FabricatedTee.Tf, component.Shape.tf);
			if (component.BraceConnect.FabricatedTee.Tf < ConstNum.QUARTER_INCH)
				wmax = component.BraceConnect.FabricatedTee.Tf;
			else
				wmax = component.BraceConnect.FabricatedTee.Tf - ConstNum.SIXTEENTH_INCH;
            if (!component.BraceConnect.FabricatedTee.StemWeldSize_User) 
                component.BraceConnect.FabricatedTee.FlangeWeldSize = Math.Max(RequiredWeldsize, minweld);
			if (wmax < component.BraceConnect.FabricatedTee.FlangeWeldSize && !component.BraceConnect.FabricatedTee.Tf_User)
				component.BraceConnect.FabricatedTee.Tf = component.BraceConnect.FabricatedTee.FlangeWeldSize + ConstNum.SIXTEENTH_INCH;

			if (component.BraceConnect.FabricatedTee.Tw == 0)
				Thickness = ConstNum.QUARTER_INCH;
			else
				Thickness = component.BraceConnect.FabricatedTee.Tw;

			// Combined Tension and Shear Rupture
			An = (component.BraceConnect.FabricatedTee.Length - Nbw * (component.BraceConnect.FabricatedTee.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * Thickness;
			if (R != 0)
			{
				a = H / R;
				b = V / R;
			}

			if (b > 0 && a > 0)
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.18 * (a / b) * (-1 + Math.Pow(1 + Math.Pow(b / a, 2) / 0.09, 0.5)) * An * component.BraceConnect.FabricatedTee.Material.Fu / b;
			else if (b == 0)
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * An * component.BraceConnect.FabricatedTee.Material.Fu;
			else if (a == 0)
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.6 * An * component.BraceConnect.FabricatedTee.Material.Fu;

			// Combined Tension and Shear Yielding
			Ag = component.BraceConnect.FabricatedTee.Length * Thickness;
			if (b > 0 && a > 0)
				GrossTensionandShearCapacity = Math.Pow(ConstNum.FIOMEGA1_0N, 2) * 0.18 * Math.Pow(a / b, 2) * (-1 / ConstNum.FIOMEGA0_95N + Math.Pow(1 / Math.Pow(ConstNum.FIOMEGA0_95N, 2) + Math.Pow(b / a, 2) / (0.09 * Math.Pow(ConstNum.FIOMEGA1_0N, 2)), 0.5)) * Ag * component.BraceConnect.FabricatedTee.Material.Fy / a;
			else if (b == 0)
				GrossTensionandShearCapacity = ConstNum.FIOMEGA0_95N * Ag * component.BraceConnect.FabricatedTee.Material.Fy;
			else if (a == 0)
				GrossTensionandShearCapacity = ConstNum.FIOMEGA1_0N * 0.6 * Ag * component.BraceConnect.FabricatedTee.Material.Fy;
			TensionandShearCapacity = Math.Min(NetTensionandShearCapacity, GrossTensionandShearCapacity);
			if (TensionandShearCapacity < R)
				Thickness = Thickness * R / TensionandShearCapacity;

			// Block Shear 1
			An1 = (component.BraceConnect.FabricatedTee.Length - component.BraceConnect.FabricatedTee.Ev1 - (Nbw - 0.5) * (component.BraceConnect.FabricatedTee.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * Thickness;
			An2 = (component.BraceConnect.FabricatedTee.D - ercl - component.BraceConnect.FabricatedTee.Eh2 - (Nvl - 0.5) * (component.BraceConnect.FabricatedTee. Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * Thickness;
			BlockShearCapacity1 = SmallMethodsDesign.ShearTensionInteraction(1, R, a, b, component.BraceConnect.FabricatedTee.Material.Fu, An1, An2);
			// Block Shear 2
			An1 = (component.BraceConnect.FabricatedTee.Length - 2 * component.BraceConnect.FabricatedTee.Ev1 - (Nbw - 0.5) * (component.BraceConnect.FabricatedTee.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * Thickness;
			An2 = 2 * (component.BraceConnect.FabricatedTee.D - ercl - component.BraceConnect.FabricatedTee.Eh2 - (Nvl - 0.5) * (component.BraceConnect.FabricatedTee. Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * Thickness;
			BlockShearCapacity2 = SmallMethodsDesign.ShearTensionInteraction(2, R, a, b, component.BraceConnect.FabricatedTee.Material.Fu, An1, An2);

			BlockShearCapacity = Math.Min(BlockShearCapacity1, BlockShearCapacity2);
			if (BlockShearCapacity < R)
				Thickness = Thickness * R / BlockShearCapacity;

			// Bolt Bearing under Vertical Load
			Fbs = CommonCalculations.SpacingBearing(component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir, component.BraceConnect.FabricatedTee.Bolt.HoleWidth, component.BraceConnect.FabricatedTee.D, component.BraceConnect.FabricatedTee.Bolt.HoleType, component.BraceConnect.FabricatedTee.Material.Fu, false);
			Fbe = CommonCalculations.EdgeBearing(component.BraceConnect.FabricatedTee.Ev1, (component.BraceConnect.FabricatedTee.Bolt.HoleWidth), component.BraceConnect.FabricatedTee.D, component.BraceConnect.FabricatedTee.Material.Fu, component.BraceConnect.FabricatedTee.Bolt.HoleType, false);
			EquivBoltFactor = c / (Nvl * Nbw);
			if (EquivBoltFactor > 1)
				EquivBoltFactor = 1;
			BearingCapacityV = EquivBoltFactor * Nvl * (Fbe + Fbs * (Nbw - 1)) * component.BraceConnect.FabricatedTee.D * Thickness;
			if (BearingCapacityV < V)
				Thickness = Thickness * R / BearingCapacityV;

			// Bolt Bearing under Horizontal Load
			Fbs = CommonCalculations.SpacingBearing(component.BraceConnect.FabricatedTee.Bolt.SpacingTransvDir, component.BraceConnect.FabricatedTee. Bolt.HoleLength, component.BraceConnect.FabricatedTee.D, component.BraceConnect.FabricatedTee.Bolt.HoleType, component.BraceConnect.FabricatedTee.Material.Fu, false);
			Fbe = CommonCalculations.EdgeBearing(component.BraceConnect.FabricatedTee.Eh1, (component.BraceConnect.FabricatedTee. Bolt.HoleLength), component.BraceConnect.FabricatedTee.D, component.BraceConnect.FabricatedTee.Material.Fu, component.BraceConnect.FabricatedTee.Bolt.HoleType, false);

			EquivBoltFactor = c / (Nvl * Nbw);
			if (EquivBoltFactor > 1)
				EquivBoltFactor = 1;
			BearingCapacityH = EquivBoltFactor * Nbw * (Fbe + Fbs * (Nvl - 1)) * component.BraceConnect.FabricatedTee.D * Thickness;
			if (BearingCapacityH < H)
				Thickness = Thickness * R / BearingCapacityV;
			if (MiscMethods.IsBeamOrColumn(memberType))
                if (!component.BraceConnect.FabricatedTee.FirstBoltDistance_User) 
                    component.BraceConnect.FabricatedTee.FirstBoltDistance = 2 - component.Shape.d / 2 - (Nbw - 1) * component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir / 2;

			component.BraceConnect.FabricatedTee.Bolt.NumberOfBolts = component.BraceConnect.FabricatedTee.Bolt.NumberOfRows;
			L = component.BraceConnect.FabricatedTee.Length;
			d = component.BraceConnect.FabricatedTee.Bolt.HoleWidth;
			N = component.BraceConnect.FabricatedTee.Bolt.NumberOfRows;
			S = component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir;
			E = component.BraceConnect.FabricatedTee.Ev1;
			MomInert = (Math.Pow(L, 3) - N * Math.Pow(d, 3)) / 12 - d * N * (Math.Pow(L / 2 - E, 2) - (L / 2 - E) * S * (N - 1) + Math.Pow(S, 2) * (N - 1) * (2 * N - 1) / 6);
			SecMod = 2 * MomInert / L; // for unit thickness

			if (MiscMethods.IsBeamOrColumn(memberType))
				ex = (component.BraceConnect.FabricatedTee.Tw + component.Shape.tw) / 2;
			else
				ex = (component.BraceConnect.FabricatedTee.Tw + component.BraceConnect.Gusset.Thickness) / 2;

			Moment2 = H * ex / 2;

			if (Mom > 0 && V > 0)
				spt3 = (Mom + V * (ercl + component.BraceConnect.FabricatedTee.Eh2)) / (ConstNum.FIOMEGA0_95N * component.BraceConnect.FabricatedTee.Material.Fy * SecMod);
			else
				spt3 = V * (ercl + component.BraceConnect.FabricatedTee.Eh2) / (ConstNum.FIOMEGA0_95N * component.BraceConnect.FabricatedTee.Material.Fy * SecMod);
			spt = Math.Max(Thickness, spt3);
			tminimum = component.BraceConnect.FabricatedTee.Length * Math.Sqrt(component.BraceConnect.FabricatedTee.Material.Fy) / 418;
			spt = Math.Max(spt, tminimum);
			SmallMethodsDesign.SinglePlateBuckling(memberType, H, spt, Mom, V);

            if (!component.BraceConnect.FabricatedTee.Tw_User) 
                component.BraceConnect.FabricatedTee.Tw = CommonCalculations.PlateThickness(spt);

			Fb = 6 * Mom / Math.Pow(L, 2);
			fr = Math.Sqrt(Math.Pow(H / L + Fb, 2) + Math.Pow(V / L, 2));
			// fraverage = Sqr(((H / L) + Fb / 2) ^ 2 + (V / L) ^ 2)
			// w = Max(fr, 1.25 * fraverage) / (0.4242 * Fexx)
			w = fr / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);

            if (!component.BraceConnect.FabricatedTee.StemWeldSize_User) 
                component.BraceConnect.FabricatedTee.StemWeldSize = NumberFun.ConvertFromFraction(w);
			Mfws = CommonCalculations.MinimumWeld(Th, component.BraceConnect.FabricatedTee.Tw);
            if (!component.BraceConnect.FabricatedTee.StemWeldSize_User) 
                component.BraceConnect.FabricatedTee.StemWeldSize = Math.Max(component.BraceConnect.FabricatedTee.StemWeldSize, Mfws);

			Reporting.AddHeader("Design Fabricated Tee");
			// FabTee Flange:
			Reporting.AddHeader("Flange Plate:");
			Reporting.AddLine("Length = " + component.BraceConnect.FabricatedTee.Length + ConstUnit.Length);
			Reporting.AddLine("Width = " + component.BraceConnect.FabricatedTee.bf + ConstUnit.Length);
			Reporting.AddLine("Thickness = " + component.BraceConnect.FabricatedTee.Tf + ConstUnit.Length);

			// Weld Stress
			Fb = 6 * Mom / Math.Pow(component.BraceConnect.FabricatedTee.Length, 2);
			fh = H / component.BraceConnect.FabricatedTee.Length;
			fvv = V / component.BraceConnect.FabricatedTee.Length;
			f_Combined = Math.Pow(Math.Pow(fh + Fb, 2) + Math.Pow(fvv, 2), 0.5);

			Reporting.AddHeader("Flange to Column Weld:");

			if (component.BraceConnect.FabricatedTee.FlangeWeld == EWeldType.Fillet)
			{
				Reporting.AddLine("Fillet Weld Size = " + CommonCalculations.WeldSize(((int) component.BraceConnect.FabricatedTee.FlangeWeldSize)) + ConstUnit.Length);
				if (minweld <= component.BraceConnect.FabricatedTee.FlangeWeldSize)
					Reporting.AddLine("Min Weld Size = " + CommonCalculations.WeldSize(((int) minweld)) + " <= " + CommonCalculations.WeldSize(((int) component.BraceConnect.FabricatedTee.FlangeWeldSize)) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Min Weld Size = " + CommonCalculations.WeldSize(((int) minweld)) + " >> " + CommonCalculations.WeldSize(((int) component.BraceConnect.FabricatedTee.FlangeWeldSize)) + ConstUnit.Length + " (NG)");
				if (wmax >= component.BraceConnect.FabricatedTee.FlangeWeldSize)
					Reporting.AddLine("Max Weld Size = " + CommonCalculations.WeldSize(((int) wmax)) + " >= " + CommonCalculations.WeldSize(((int) component.BraceConnect.FabricatedTee.FlangeWeldSize)) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Max Weld Size = " + CommonCalculations.WeldSize(((int) wmax)) + " << " + CommonCalculations.WeldSize(((int) component.BraceConnect.FabricatedTee.FlangeWeldSize)) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Max. Weld Force, fmax,");
				Reporting.AddLine("= ((H / L + 6 * M / L²)² + (V / L)²)^0.5");
				Reporting.AddLine("= ((" + H + " / " + component.BraceConnect.FabricatedTee.Length + " + 6 * " + Mom + " / " + component.BraceConnect.FabricatedTee.Length + " ^ 2) ^ 2 + (" + V + " / " + component.BraceConnect.FabricatedTee.Length + ") ^ 2) ^ 0.5");
				Reporting.AddLine("= " + f_Combined + ConstUnit.ForcePerUnitLength);
				capacity = 2 * ConstNum.FIOMEGA0_75N * 0.6 * 0.707 * component.BraceConnect.FabricatedTee.FlangeWeldSize * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
				Reporting.AddLine(ConstString.PHI + " Rn = 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * 0.707 * w * Fexx");
				Reporting.AddLine("= 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * 0.707 * " + component.BraceConnect.FabricatedTee.FlangeWeldSize + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				if (capacity >= f_Combined)
					Reporting.AddLine("= " + capacity + " >= " + f_Combined + ConstUnit.ForcePerUnitLength + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + f_Combined + ConstUnit.ForcePerUnitLength + " (NG)");
			}
			else
			{
				WeldCapacity = ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 2 * component.BraceConnect.FabricatedTee.FlangeWeldSize;
				Reporting.AddHeader("Flange to Column Weld: (Effective throat = " + CommonCalculations.WeldSize((component.BraceConnect.FabricatedTee.FlangeWeldSize)) + "\") Flare-Bevel");
				Reporting.AddLine("Max. Weld Force = ((6 * M / L² + H / L)² + (V / L)²)^0.5");
				Reporting.AddLine("= ((6 * " + Mom + " / " + component.BraceConnect.FabricatedTee.Length + "² + " + H + " / " + component.BraceConnect.FabricatedTee.Length + ")² + (" + V + " / " + component.BraceConnect.FabricatedTee.Length + ")²)^0.5");
				Reporting.AddLine("= " + f_Combined + ConstUnit.ForcePerUnitLength);
				Reporting.AddLine("Weld Strength = (" + ConstString.FIOMEGA0_75 + " * 0.6 * Fexx) * (2 * Ef_Throat)");
				Reporting.AddLine("= (" + ConstString.FIOMEGA0_75 + " * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ") * (2 * " + component.BraceConnect.FabricatedTee.FlangeWeldSize + ")");

				if (WeldCapacity >= f_Combined)
					Reporting.AddLine("= " + WeldCapacity + " >= " + f_Combined + ConstUnit.ForcePerUnitLength + " (OK)");
				else
					Reporting.AddLine("= " + WeldCapacity + " << " + f_Combined + ConstUnit.ForcePerUnitLength + " (NG)");
			}

			// Yielding of Tee Flange
			if (component.WebOrientation == EWebOrientation.InPlane)
				BF = component.Shape.bf;
			else
				BF = component.Shape.d;
			BF = Math.Min(BF, component.BraceConnect.FabricatedTee.bf);
			Reporting.AddHeader("Bending of Flange Plate:");
			CapacityforFlexuralYielding = ConstNum.FIOMEGA0_95N * Math.Pow(component.BraceConnect.FabricatedTee.Tf, 2) * component.BraceConnect.FabricatedTee.Material.Fy * component.BraceConnect.FabricatedTee.Length / BF - 3 * Mom / component.BraceConnect.FabricatedTee.Length;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * tf² * Fy * L / B - 3 * M / L");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + component.BraceConnect.FabricatedTee.Tf + "² * " + component.BraceConnect.FabricatedTee.Material.Fy + " * " + component.BraceConnect.FabricatedTee.Length + " / " + BF + " - 3 * " + Mom + " / " + component.BraceConnect.FabricatedTee.Length);
			if (CapacityforFlexuralYielding >= H)
				Reporting.AddLine("= " + CapacityforFlexuralYielding + " >= " + H + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityforFlexuralYielding + " << " + H + ConstUnit.Force + " (NG)");

			// Shear Yielding of Tee Flange
			Reporting.AddHeader("Shearing of Flange Plate:");
			Reporting.AddHeader("Resultant Force (Rm)");
			Rm = Math.Pow(Math.Pow(H + 3 * Mom / component.BraceConnect.FabricatedTee.Length, 2) + V * V, 0.5);

			Reporting.AddLine("= (H + 3 * M / L)² + V * V)^0.5");
			Reporting.AddLine("= (" + H + " + 3 * " + Mom + " / " + component.BraceConnect.FabricatedTee.Length + ") ^ 2 + " + V + " * " + V + ") ^ 0.5");
			Reporting.AddLine("= " + Rm + ConstUnit.Force);
			CapacityforShearYielding = 2 * ConstNum.FIOMEGA1_0N * 0.6 * component.BraceConnect.FabricatedTee.Tf * component.BraceConnect.FabricatedTee.Material.Fy * component.BraceConnect.FabricatedTee.Length;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * tf * Fy * L");
			Reporting.AddLine("= 2 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + component.BraceConnect.FabricatedTee.Tf + " * " + component.BraceConnect.FabricatedTee.Material.Fy + " * " + component.BraceConnect.FabricatedTee.Length);
			if (CapacityforShearYielding >= Rm)
				Reporting.AddLine("= " + CapacityforShearYielding + " >= " + Rm + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityforShearYielding + " << " + Rm + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Stem Plate:");
			Reporting.AddLine("Length = " + component.BraceConnect.FabricatedTee.Length + ConstUnit.Length);
			Reporting.AddLine("Width = " + (component.BraceConnect.FabricatedTee.D - component.BraceConnect.FabricatedTee.Tf) + ConstUnit.Length);
			Reporting.AddLine("Thickness = " + component.BraceConnect.FabricatedTee.Tw + ConstUnit.Length);
			Reporting.AddLine("Bolts: (" + component.BraceConnect.FabricatedTee.Bolt.NumberOfBolts + ") " + component.BraceConnect.FabricatedTee.Bolt.BoltName);
			Reporting.AddLine("Bolt Holes on Stem Plate: " + component.BraceConnect.FabricatedTee.Bolt.HoleLength + "\" Horiz. X " + component.BraceConnect.FabricatedTee.Bolt.HoleWidth + "\" Vert.");
			Reporting.AddLine("Bolt Holes on Gusset: " + component.BraceConnect.FabricatedTee.Bolt.HoleLength + "\" Horiz. X " + component.BraceConnect.FabricatedTee.Bolt.HoleWidth + "\" Vert.");

			if (component.BraceConnect.FabricatedTee.Bolt.BoltType != EBoltType.SC && component.BraceConnect.FabricatedTee.Bolt.HoleType != EBoltHoleType.STD)
			{
				Reporting.AddHeader("For this type of bolted construction, slotted holes");
				Reporting.AddLine("are permitted  normal to the direction of the load.  Here,");
				Reporting.AddLine("the load direction may be neither horizontal nor vertical");
				Reporting.AddLine("and it is likely to change during the loading histoy.");
				Reporting.AddLine("You need to make a judgment regarding this issue,");
				Reporting.AddLine("and consider using standard holes or slip critical bolts.");
			}

			Reporting.AddHeader("Bolts");

			if (component.BraceConnect.FabricatedTee.Bolt.Slot0 && component.BraceConnect.FabricatedTee.Bolt.Slot1)
			{
				SminPlate = SmallMethodsDesign.MinBoltSpacing(component.BraceConnect.FabricatedTee.Bolt, component.BraceConnect.FabricatedTee.Material.Fu, component.BraceConnect.FabricatedTee.Tw, V / component.BraceConnect.FabricatedTee.Bolt.NumberOfBolts, component.BraceConnect.FabricatedTee.Bolt.HoleWidth);
				SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BraceConnect.FabricatedTee.Bolt, component.BraceConnect.Gusset.Material.Fu, Th, V / component.BraceConnect.FabricatedTee.Bolt.NumberOfBolts, component.BraceConnect.FabricatedTee.Bolt.HoleWidth);
			}
			else if (component.BraceConnect.FabricatedTee.Bolt.Slot0)
			{
				SminPlate = SmallMethodsDesign.MinBoltSpacing(component.BraceConnect.FabricatedTee.Bolt, component.BraceConnect.FabricatedTee.Material.Fu, component.BraceConnect.FabricatedTee.Tw, V / component.BraceConnect.FabricatedTee.Bolt.NumberOfBolts, component.BraceConnect.FabricatedTee.D + ConstNum.SIXTEENTH_INCH);
				SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BraceConnect.FabricatedTee.Bolt, component.BraceConnect.Gusset.Material.Fu, Th, V / component.BraceConnect.FabricatedTee.Bolt.NumberOfBolts, component.BraceConnect.FabricatedTee.Bolt.HoleWidth);
			}
			else if (component.BraceConnect.FabricatedTee.Bolt.Slot1)
			{
				SminPlate = SmallMethodsDesign.MinBoltSpacing(component.BraceConnect.FabricatedTee.Bolt, component.BraceConnect.FabricatedTee.Material.Fu, component.BraceConnect.FabricatedTee.Tw, V / component.BraceConnect.FabricatedTee.Bolt.NumberOfBolts, component.BraceConnect.FabricatedTee.Bolt.HoleWidth);
				SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BraceConnect.FabricatedTee.Bolt, component.BraceConnect.Gusset.Material.Fu, Th, V / component.BraceConnect.FabricatedTee.Bolt.NumberOfBolts, component.BraceConnect.FabricatedTee.D + ConstNum.SIXTEENTH_INCH);
			}
			else
			{
				SminPlate = SmallMethodsDesign.MinBoltSpacing(component.BraceConnect.FabricatedTee.Bolt, component.BraceConnect.FabricatedTee.Material.Fu, component.BraceConnect.FabricatedTee.Tw, V / component.BraceConnect.FabricatedTee.Bolt.NumberOfBolts, component.BraceConnect.FabricatedTee.D + ConstNum.SIXTEENTH_INCH);
				SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BraceConnect.FabricatedTee.Bolt, component.BraceConnect.Gusset.Material.Fu, Th, V / component.BraceConnect.FabricatedTee.Bolt.NumberOfBolts, component.BraceConnect.FabricatedTee.D + ConstNum.SIXTEENTH_INCH);
			}
			Smin = Math.Min(SminPlate, SminGusset);
			if (component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir >= Smin)
				Reporting.AddLine("Bolt Vertical Spacing = " + component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir + " >= Min. Spacing = " + Smin + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Bolt Vertical Spacing = " + component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir + " << Min. Spacing = " + Smin + ConstUnit.Length + " (NG)");

			if (V > 0)
				edgeminComparison = 2 * V / (component.BraceConnect.FabricatedTee.Bolt.NumberOfBolts * component.BraceConnect.FabricatedTee.Material.Fu * component.BraceConnect.FabricatedTee.Tw);
			edgemin = Math.Max(component.BraceConnect.FabricatedTee.Bolt.MinEdgeSheared, edgeminComparison) + component.BraceConnect.FabricatedTee.Bolt.HoleWidth - component.BraceConnect.FabricatedTee.Bolt.HoleDiameterSTD;
			if (component.BraceConnect.FabricatedTee.Bolt.EdgeDistLongDir >= edgemin)
				Reporting.AddLine("Vert. Edge Dist. on S. Plate = " + component.BraceConnect.FabricatedTee.Bolt.EdgeDistLongDir + " >= Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Vert. Edge Dist. on S. Plate = " + component.BraceConnect.FabricatedTee.Bolt.EdgeDistLongDir + " << Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (NG)");

			if (!MiscMethods.IsBeamOrColumn(memberType))
			{
				edgemin = Math.Max(component.BraceConnect.FabricatedTee.Bolt.MinEdgeSheared, 2 * V / (component.BraceConnect.FabricatedTee.Bolt.NumberOfBolts * component.BraceConnect.Gusset.Material.Fu * Th)) + component.BraceConnect.FabricatedTee.Bolt.HoleWidth - component.BraceConnect.FabricatedTee.Bolt.HoleDiameterSTD;
				edgeActual = component.BraceConnect.Gusset.ColumnSide - component.BraceConnect.FabricatedTee.FirstBoltDistance - (component.BraceConnect.FabricatedTee.Bolt.NumberOfRows - 1) * component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir;
				if (edgeActual >= edgemin)
					Reporting.AddLine("Vert. Edge Dist. on Gusset = " + edgeActual + " >= Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Vert. Edge Dist. on Gusset = " + edgeActual + " << Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (NG)");
			}

			Reporting.AddHeader("Bolt Shear Capacity: ");

			ex = (ercl + component.BraceConnect.FabricatedTee.Eh2 + (component.BraceConnect.FabricatedTee.Bolt.NumberOfLines - 1) * component.BraceConnect.FabricatedTee.Bolt.SpacingTransvDir / 2);
			Momeffect = "";
			if (Mom > 0 && V > 0)
			{
				ex = ex + Mom / V;
				Momeffect = " (Includes the effect of Gusset Edge Moment.)";
				Theta = Math.Atan(Math.Abs(H / V));
			}

			c = SmallMethodsDesign.EccIncLoadOnBG(component.BraceConnect.FabricatedTee.Bolt.NumberOfLines, component.BraceConnect.FabricatedTee.Bolt.NumberOfRows, ex, Theta, ((int) component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir), ((int) component.BraceConnect.FabricatedTee.Bolt.SpacingTransvDir));
			ShearCap = c * component.BraceConnect.FabricatedTee.Bolt.BoltStrength;
			Reporting.AddLine("Eccentricity, ex = " + ex + ConstUnit.Length + Momeffect);
			Reporting.AddLine("Vertically: " + component.BraceConnect.FabricatedTee.Bolt.NumberOfRows + " Bolts with " + component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir + ConstUnit.Length + " Spacing");
			Reporting.AddLine("Horizontally: " + component.BraceConnect.FabricatedTee.Bolt.NumberOfLines + " Bolts with " + component.BraceConnect.FabricatedTee.Bolt.SpacingTransvDir + ConstUnit.Length + " Spacing");
			Reporting.AddLine("Resultant Load (" + R + ConstUnit.Force + ") Inclined " + Theta / ConstNum.RADIAN + " Degrees from Vertical");
			Reporting.AddLine("Inclined Eccentic Load Coefficient, C = " + c);
			if (ShearCap >= R)
				Reporting.AddLine(ConstString.PHI + " Rn = C * Fv = " + c + " * " + component.BraceConnect.FabricatedTee.Bolt.BoltStrength + " = " + ShearCap + " >= " + R + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine(ConstString.PHI + " Rn = C * Fv = " + c + " * " + component.BraceConnect.FabricatedTee.Bolt.BoltStrength + " = " + ShearCap + " << " + R + ConstUnit.Force + " (NG)");

			// Bolt Bearing under Vertical Load
			Reporting.AddHeader("Bolt Bearing");
			Reporting.AddHeader("Vertical Load:");
			Fbs = CommonCalculations.SpacingBearing(component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir, component.BraceConnect.FabricatedTee.Bolt.HoleWidth, component.BraceConnect.FabricatedTee.D, component.BraceConnect.FabricatedTee.Bolt.HoleType, component.BraceConnect.FabricatedTee.Material.Fu, false);
			Fbe = CommonCalculations.EdgeBearing(component.BraceConnect.FabricatedTee.Ev1, (component.BraceConnect.FabricatedTee.Bolt.HoleWidth), component.BraceConnect.FabricatedTee.D, component.BraceConnect.FabricatedTee.Material.Fu, component.BraceConnect.FabricatedTee.Bolt.HoleType, false);

			EquivBoltFactor = c / (component.BraceConnect.FabricatedTee.Bolt.NumberOfLines * component.BraceConnect.FabricatedTee.Bolt.NumberOfRows);
			if (EquivBoltFactor > 1)
				EquivBoltFactor = 1;
			Reporting.AddLine("Equiv. Bolt Factor, ef = C/Nb <= 1 = " + c + " / " + (component.BraceConnect.FabricatedTee.Bolt.NumberOfLines * component.BraceConnect.FabricatedTee.Bolt.NumberOfRows) + " = " + EquivBoltFactor);
			BearingCapacityV = EquivBoltFactor * component.BraceConnect.FabricatedTee.Bolt.NumberOfLines * (Fbe + Fbs * (component.BraceConnect.FabricatedTee.Bolt.NumberOfRows - 1)) * component.BraceConnect.FabricatedTee.D * component.BraceConnect.FabricatedTee.Tw;
			Reporting.AddLine(ConstString.PHI + " Rn  = ef * Nh*(Fbe + Fbs * (Nl - 1)) * db * t");
			Reporting.AddLine("= " + EquivBoltFactor + " * " + component.BraceConnect.FabricatedTee.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + component.BraceConnect.FabricatedTee.Bolt.NumberOfRows + " - 1)) * " + component.BraceConnect.FabricatedTee.D + " * " + component.BraceConnect.FabricatedTee.Tw);
			if (BearingCapacityV >= V)
				Reporting.AddLine("= " + BearingCapacityV + " >= " + V + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + BearingCapacityV + " << " + V + ConstUnit.Force + " (NG)");

			// Bolt Bearing under Horizontal Load
			Reporting.AddHeader("Horizontal Load:");
			Fbs = CommonCalculations.SpacingBearing(component.BraceConnect.FabricatedTee.Bolt.SpacingTransvDir, component.BraceConnect.FabricatedTee.Bolt.HoleWidth, component.BraceConnect.FabricatedTee.D, component.BraceConnect.FabricatedTee.Bolt.HoleType, component.BraceConnect.FabricatedTee.Material.Fu, false);
			Fbe = CommonCalculations.EdgeBearing(component.BraceConnect.FabricatedTee.Eh1, (component.BraceConnect.FabricatedTee. Bolt.HoleLength), component.BraceConnect.FabricatedTee.D, component.BraceConnect.FabricatedTee.Material.Fu, component.BraceConnect.FabricatedTee.Bolt.HoleType, false);

			EquivBoltFactor = c / (component.BraceConnect.FabricatedTee.Bolt.NumberOfLines * component.BraceConnect.FabricatedTee.Bolt.NumberOfRows);
			if (EquivBoltFactor > 1)
				EquivBoltFactor = 1;
			
			BearingCapacityH = EquivBoltFactor * component.BraceConnect.FabricatedTee.Bolt.NumberOfRows * (Fbe + Fbs * (component.BraceConnect.FabricatedTee.Bolt.NumberOfLines - 1)) * component.BraceConnect.FabricatedTee.D * component.BraceConnect.FabricatedTee.Tw;
			Reporting.AddLine(ConstString.PHI + " Rn  = ef * Nl * (Fbe + Fbs * (Nh - 1)) * db * t");
			Reporting.AddLine("= " + EquivBoltFactor + " * " + component.BraceConnect.FabricatedTee.Bolt.NumberOfRows + " * (" + Fbe + " + " + Fbs + " * (" + component.BraceConnect.FabricatedTee.Bolt.NumberOfLines + " - 1)) * " + component.BraceConnect.FabricatedTee.D + " * " + component.BraceConnect.FabricatedTee.Tw);
			if (BearingCapacityH >= H)
				Reporting.AddLine("= " + BearingCapacityH + " >= " + H + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + BearingCapacityH + " << " + H + ConstUnit.Force + " (NG)");

			SmallMethodsDesign.GussetandBeamCheckwithFabTee(memberType, V, H, R, ((int) EquivBoltFactor), EBearingTearOut.Bearing);
			Reporting.AddHeader("Plate Shear and Normal Stresses:");
			// Combined Tension and Shear Rupture
			Reporting.AddHeader("Combined Tension and Shear:");
			Reporting.AddFormulaLine();
			if (R != 0)
			{
				a = H / R;
				b = V / R;
			}

			if (V == 0)
				Reporting.AddLine("Load Angle, " + 216 + " = Atn(H/V) = 90 degrees");
			else
				Reporting.AddLine("Load Angle, " + 216 + " = Atn(H/V) = " + Math.Atan(Math.Abs(H / V)) / ConstNum.RADIAN + " degrees");

			Reporting.AddLine("A = Sin(" + 216 + ") = " + a);
			Reporting.AddLine("B = Cos(" + 216 + ") = " + b);
			Reporting.AddHeader("Rupture:");
			An = (component.BraceConnect.FabricatedTee.Length - component.BraceConnect.FabricatedTee.Bolt.NumberOfRows * (component.BraceConnect.FabricatedTee.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * component.BraceConnect.FabricatedTee.Tw;
			Reporting.AddLine("Net Area (An) = (L - Nl * (dv + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + component.BraceConnect.FabricatedTee.Length + " - " + component.BraceConnect.FabricatedTee.Bolt.NumberOfRows + " * (" + component.BraceConnect.FabricatedTee.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + component.BraceConnect.FabricatedTee.Tw);
			Reporting.AddLine("= " + An + ConstUnit.Area);
			if (b > 0 && a > 0)
			{
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.18 * (a / b) * (-1 + Math.Pow(1 + Math.Pow(b / a, 2) / 0.09, 0.5)) * An * component.BraceConnect.FabricatedTee.Material.Fu / b;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.18 * (A/ B) * (-1 + (1 + (B / A)² / 0.09)^0.5) * An * Fu / B");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.18 * (" + a + " / " + b + ") * (-1 + (1 + (" + b + " / " + a + ")² / 0.09)^0.5) * " + An + " * " + component.BraceConnect.FabricatedTee.Material.Fu + " / " + b);
			}
			else if (b == 0)
			{
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * An * component.BraceConnect.FabricatedTee.Material.Fu;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * An*Fu = " + ConstString.FIOMEGA0_75 + " * " + An + " * " + component.BraceConnect.FabricatedTee.Material.Fu);
			}
			else if (a == 0)
			{
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.6 * An * component.BraceConnect.FabricatedTee.Material.Fu;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.6*An*Fu = " + ConstString.FIOMEGA0_75 + " * 0.6 * " + An + " * " + component.BraceConnect.FabricatedTee.Material.Fu);
			}
			if (NetTensionandShearCapacity >= R)
				Reporting.AddLine("= " + NetTensionandShearCapacity + " >= " + R + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + NetTensionandShearCapacity + " << " + R + ConstUnit.Force + " (NG)");

			// Combined Tension and Shear Yielding
			Reporting.AddHeader("Yielding:");
			Ag = component.BraceConnect.FabricatedTee.Length * component.BraceConnect.FabricatedTee.Tw;
			Reporting.AddLine("Ag = L * t = " + component.BraceConnect.FabricatedTee.Length + " * " + component.BraceConnect.FabricatedTee.Tw + " = " + Ag + ConstUnit.Area);
			if (b > 0 && a > 0)
			{
				GrossTensionandShearCapacity = Math.Pow(ConstNum.FIOMEGA1_0N, 2) * 0.18 * Math.Pow(a / b, 2) * (-1 / ConstNum.FIOMEGA0_95N + Math.Pow(1 / Math.Pow(ConstNum.FIOMEGA0_95N, 2) + Math.Pow(b / a, 2) / (0.09 * Math.Pow(ConstNum.FIOMEGA1_0N, 2)), 0.5)) * Ag * component.Material.Fy / a;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.FIOMEGA1_0N + "² * 0.18 * (A / B)² * (-1 / " + ConstString.FIOMEGA0_9 + " + (1 / (" + ConstString.FIOMEGA0_9 + "²) + (B / A)² / (0.09 * " + ConstNum.FIOMEGA1_0N + "²))^0.5) * Ag * Fy / A");
				Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + "² * 0.18 * (" + a + " / " + b + ")² * (-1 / " + ConstString.FIOMEGA0_9 + " + (1 / (" + ConstString.FIOMEGA0_9 + "²) + (" + b + " / " + a + ")² / (0.09 * " + ConstNum.FIOMEGA1_0N + "²))^0.5) * " + Ag + " * " + component.Material.Fy + " / " + a);
			}
			else if (b == 0)
			{
				GrossTensionandShearCapacity = ConstNum.FIOMEGA0_95N * Ag * component.BraceConnect.FabricatedTee.Material.Fy;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Ag * Fy = " + ConstString.FIOMEGA0_9 + " * " + Ag + " * " + component.BraceConnect.FabricatedTee.Material.Fy);
			}
			else if (a == 0)
			{
				GrossTensionandShearCapacity = ConstNum.FIOMEGA0_95N * 0.6 * Ag * component.BraceConnect.FabricatedTee.Material.Fy;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * 0.6 * Ag * Fy = " + ConstString.FIOMEGA0_9 + " * 0.6 * " + Ag + " * " + component.BraceConnect.FabricatedTee.Material.Fy);
			}
			if (GrossTensionandShearCapacity >= R)
				Reporting.AddLine("= " + GrossTensionandShearCapacity + " >= " + R + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + GrossTensionandShearCapacity + " << " + R + ConstUnit.Force + " (NG)");

			// Block Shear 1
			Reporting.AddHeader("Block Shear:");
			Reporting.AddLine("Vertical (An1, Ft1) and Horizontal (An2, Ft2) Sections:");
			Reporting.AddHeader("Pattern 1:");
			An1 = (component.BraceConnect.FabricatedTee.Length - component.BraceConnect.FabricatedTee.Ev1 - (component.BraceConnect.FabricatedTee.Bolt.NumberOfRows - 0.5) * (component.BraceConnect.FabricatedTee.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * component.BraceConnect.FabricatedTee.Tw;
			Reporting.AddLine("An1 = (L - Lv - (Nl - 0.5) * (dv + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + component.BraceConnect.FabricatedTee.Length + " - " + component.BraceConnect.FabricatedTee.Ev1 + " - (" + component.BraceConnect.FabricatedTee.Bolt.NumberOfRows + " - 0.5) * (" + component.BraceConnect.FabricatedTee.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + component.BraceConnect.FabricatedTee.Tw);
			Reporting.AddLine("= " + An1 + ConstUnit.Area);

			An2 = (component.BraceConnect.FabricatedTee.D - ercl - component.BraceConnect.FabricatedTee.Eh2 - (component.BraceConnect.FabricatedTee.Bolt.NumberOfLines - 0.5) * (component.BraceConnect.FabricatedTee. Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * component.BraceConnect.FabricatedTee.Tw;
			Reporting.AddLine("An2 = (W - c - Lh - (Nh - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + component.BraceConnect.FabricatedTee.D + " - " + ercl + " - " + component.BraceConnect.FabricatedTee.Eh2 + " - (" + component.BraceConnect.FabricatedTee.Bolt.NumberOfLines + " - 0.5) * (" + component.BraceConnect.FabricatedTee. Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + component.BraceConnect.FabricatedTee.Tw);
			Reporting.AddLine("= " + An2 + ConstUnit.Area);
			BlockShearCapacity1 = SmallMethodsDesign.ShearTensionInteraction(1, R, a, b, component.BraceConnect.FabricatedTee.Material.Fu, An1, An2);

			Reporting.AddHeader("Pattern 2:");
			An1 = (component.BraceConnect.FabricatedTee.Length - 2 * component.BraceConnect.FabricatedTee.Ev1 - (component.BraceConnect.FabricatedTee.Bolt.NumberOfRows - 0.5) * (component.BraceConnect.FabricatedTee.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * component.BraceConnect.FabricatedTee.Tw;
			Reporting.AddLine("An1 = (L - 2*Lv - (Nl - 0.5) * (dv + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + component.BraceConnect.FabricatedTee.Length + " - 2*" + component.BraceConnect.FabricatedTee.Ev1 + " - (" + component.BraceConnect.FabricatedTee.Bolt.NumberOfRows + " - 0.5) * (" + component.BraceConnect.FabricatedTee.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + component.BraceConnect.FabricatedTee.Tw);
			Reporting.AddLine("= " + An1 + ConstUnit.Area);

			An2 = 2 * (component.BraceConnect.FabricatedTee.D - ercl - component.BraceConnect.FabricatedTee.Eh2 - (component.BraceConnect.FabricatedTee.Bolt.NumberOfLines - 0.5) * (component.BraceConnect.FabricatedTee. Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * component.BraceConnect.FabricatedTee.Tw;
			Reporting.AddLine("An2 = 2 * (W - c - Lh - (Nh - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= 2 * ( * " + component.BraceConnect.FabricatedTee.D + " - " + ercl + " - " + component.BraceConnect.FabricatedTee.Eh2 + " - (" + component.BraceConnect.FabricatedTee.Bolt.NumberOfLines + " - 0.5) * (" + component.BraceConnect.FabricatedTee. Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + component.BraceConnect.FabricatedTee.Tw);
			Reporting.AddLine("= " + An2 + ConstUnit.Area);
			BlockShearCapacity2 = SmallMethodsDesign.ShearTensionInteraction(2, R, a, b, component.BraceConnect.FabricatedTee.Material.Fu, An1, An2);

			SmallMethodsDesign.GussetandBeamCheckwithFabTee(memberType, V, H, R, ((int) EquivBoltFactor), EBearingTearOut.TearOut);

			Reporting.AddHeader("Stem Plate Bending:");
			L = component.BraceConnect.FabricatedTee.Length;
			d = component.BraceConnect.FabricatedTee.Bolt.HoleWidth;
			N = component.BraceConnect.FabricatedTee.Bolt.NumberOfRows;
			S = component.BraceConnect.FabricatedTee.Bolt.SpacingLongDir;
			E = component.BraceConnect.FabricatedTee.Ev1;
			MomInert = ((Math.Pow(L, 3) - N * Math.Pow(d, 3)) / 12 - d * N * (Math.Pow(L / 2 - E, 2) - (L / 2 - E) * S * (N - 1) + Math.Pow(S, 2) * (N - 1) * (2 * N - 1) / 6)) * component.BraceConnect.FabricatedTee.Tw;
			SecMod = 2 * MomInert / L;
			An = (component.BraceConnect.FabricatedTee.Length - component.BraceConnect.FabricatedTee.Bolt.NumberOfRows * (component.BraceConnect.FabricatedTee.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * component.BraceConnect.FabricatedTee.Tw;
			Reporting.AddLine("Net Area (An) = " + An + ConstUnit.Area);
			Reporting.AddLine("Net Section Modulus (Sn) = " + SecMod + ConstUnit.SecMod);
			Moment = Mom + V * (ercl + component.BraceConnect.FabricatedTee.Eh2);
			if (MiscMethods.IsBeamOrColumn(memberType))
				ex = (component.BraceConnect.FabricatedTee.Tw + component.Shape.tw) / 2;
			else
				ex = (component.BraceConnect.FabricatedTee.Tw + component.BraceConnect.Gusset.Thickness) / 2;

			Moment2 = H * ex / 2;
			Stress = H / An + Moment / SecMod + 6 * Moment2 / (component.BraceConnect.FabricatedTee.Tw * An);
			if (double.IsNaN(Stress))
				Stress = 0;
			Reporting.AddHeader("Stress = H / An + (Mo + V * (c + Lh)) / Sn + 6 * M1 / (t * An)");
			Reporting.AddLine("= " + H + " / " + An + " + (" + Mom + " + " + V + " * (" + ercl + " + " + component.BraceConnect.FabricatedTee.Eh2 + ")) / " + SecMod + "+ 6 * " + Moment2 + " / (" + component.BraceConnect.FabricatedTee.Tw + " * " + An + ")");
			if (Stress <= ConstNum.FIOMEGA0_95N * component.BraceConnect.FabricatedTee.Material.Fy)
				Reporting.AddLine("= " + Stress + " <= " + ConstString.FIOMEGA0_9 + " * Fy = " + (ConstNum.FIOMEGA0_95N * component.BraceConnect.FabricatedTee.Material.Fy + " ksi (OK)"));
			else
				Reporting.AddLine("= " + Stress + " >> " + ConstString.FIOMEGA0_9 + " * Fy = " + (ConstNum.FIOMEGA0_95N * component.BraceConnect.FabricatedTee.Material.Fy + " ksi (NG)"));

			Reporting.AddHeader("Tee Stem Buckling:");
			SmallMethodsDesign.SinglePlateBuckling(memberType, H, component.BraceConnect.FabricatedTee.Tw, Mom, V);

			Reporting.AddHeader("Stem to Flange Weld:");
			Fb = 6 * Mom / Math.Pow(L, 2);
			f_peak = Math.Sqrt(Math.Pow(H / L + Fb, 2) + Math.Pow(V / L, 2));
			w = f_peak / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
			Mfws = CommonCalculations.MinimumWeld(Th, component.BraceConnect.FabricatedTee.Tw);
			if (component.BraceConnect.FabricatedTee.StemWeldSize >= Mfws)
				Reporting.AddLine("Weld Size = " + component.BraceConnect.FabricatedTee.StemWeldSize + " >= Min. Weld Size = " + Mfws + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Weld Size = " + component.BraceConnect.FabricatedTee.StemWeldSize + " << Min. Weld Size = " + Mfws + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Weld Stresses:");

			Reporting.AddLine("f_max = [((H / L) + 6 * Mo / L²)² + (V / L)²]^0.5");
			Reporting.AddLine("= [((" + H + " / " + L + ") + 6 * " + Mom + " / " + L + "²)² + (" + V + " / " + L + ")²]^0.5");
			Reporting.AddLine("= " + f_peak + ConstUnit.ForcePerUnitLength);
			Reporting.AddLine("Required Weld Size = f_max / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * Fexx)");
			Reporting.AddLine("= " + f_peak + "/ (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");

			if (w <= component.BraceConnect.FabricatedTee.StemWeldSize)
				Reporting.AddLine("= " + w + " <= " + CommonCalculations.WeldSize(((int) component.BraceConnect.FabricatedTee.StemWeldSize)) + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("= " + w + " >> " + CommonCalculations.WeldSize(((int) component.BraceConnect.FabricatedTee.StemWeldSize)) + ConstUnit.Length + " (NG)");

			usefulweldsize = Math.Min(ConstNum.FIOMEGA0_75N * 0.6 * component.BraceConnect.FabricatedTee.Tw * component.BraceConnect.FabricatedTee.Material.Fu, 2 * ConstNum.FIOMEGA0_75N * 0.6 * component.BraceConnect.FabricatedTee.Tf * component.Material.Fu) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
			if (w <= usefulweldsize)
				Reporting.AddLine("Stem and Flange Plate develop the weld capacity. (OK)");
			else
				Reporting.AddLine("Stem or Flange Plate does not develop the weld capacity. (NG)");
		}
	}
}