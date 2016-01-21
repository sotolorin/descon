using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class DesignSinglePlate
	{
		internal static void CalcDesignSinglePlate(EMemberType memberType, double H_Tens, double H_Comp, double V, double Mom)
		{
			double wother = 0;
			double VO = 0;
			double ho = 0;
			double LO = 0;
			double MomO = 0;
			bool shrt3 = false;
			bool shrt2 = false;
			double usefulweldsize = 0;
			double Plth = 0;
			string Momeffect = "";
			double edgeActual = 0;
			double edgemin = 0;
			double Smin = 0;
			double SminGusset = 0;
			double SminPlate = 0;
			double Mfws = 0;
			double w = 0;
			double fraverage = 0;
			double fr = 0;
			double Fb = 0;
			double VertForce = 0;
			double tminimum = 0;
			double spt = 0;
			double spt3 = 0;
			double Stress = 0;
			double Ln = 0;
			double Moment = 0;
			double Moment2 = 0;
			double SecMod = 0;
			double MomInert = 0;
			double E = 0;
			double S = 0;
			double N = 0;
			double d = 0;
			double L = 0;
			double BearingCapacityH_Comp = 0;
			double BearingCapacityH_Tens = 0;
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
			bool exitwhile = false;
			double ShearCap = 0;
			double c = 0;
			double theta = 0;
			double ex = 0;
			int Nvl = 0;
			int Nbw = 0;
			double ercl = 0;
			double Th = 0;
			int Nmn = 0;
			double Nmx = 0;
			double TranForce = 0;
			double AxialForceEccentricity = 0;
			double SupportThickness = 0;
			double H_Max = 0;
			double R_max = 0;
			double R_Tens = 0;
			double R_Comp = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var webPlate = component.WinConnect.ShearWebPlate;

			component.GussetWeldedToColumn = false;

			R_Comp = Math.Sqrt(H_Comp * H_Comp + V * V);
			R_Tens = Math.Sqrt(H_Tens * H_Tens + V * V);
			R_max = Math.Max(R_Comp, R_Tens);
			H_Max = Math.Max(H_Tens, H_Comp);

			// Bolts
			if (column.WebOrientation == EWebOrientation.InPlane)
				SupportThickness = column.Shape.tf;
			else
				SupportThickness = column.Shape.tw;
			switch (memberType)
			{
				case EMemberType.RightBeam:
				case EMemberType.LeftBeam:
					if (!webPlate.BraceDistanceToFirstBolt_User)
						webPlate.BraceDistanceToFirstBolt = (component.Shape.d - (webPlate.Bolt.NumberOfRows - 1) * webPlate.Bolt.SpacingLongDir) / 2;
					else
						webPlate.BraceDistanceToFirstBolt = Math.Max(NumberFun.Round(component.Shape.kdet + webPlate.Bolt.EdgeDistLongDir, 4), webPlate.BraceDistanceToFirstBolt);
					
						if (webPlate.Bolt.NumberOfRows != 0)
					{
						AxialForceEccentricity = component.Shape.d / 2 - webPlate.BraceDistanceToFirstBolt - (webPlate.Bolt.NumberOfRows - 1) * webPlate.Bolt.SpacingLongDir / 2;

						TranForce = Math.Max(component.AxialTension, component.AxialCompression);
						Mom = Math.Abs(TranForce * AxialForceEccentricity);
					}

					Nmx = (Math.Floor((component.Shape.t - 2 * webPlate.Bolt.EdgeDistLongDir) / webPlate.Bolt.SpacingLongDir)) + 1;
					Nmn = (int)Math.Ceiling((component.Shape.t / 2) / webPlate.Bolt.SpacingLongDir);
					Th = component.Shape.tw;
					ercl = component.EndSetback;
					break;
				case EMemberType.UpperRight:
				case EMemberType.LowerRight:
				case EMemberType.UpperLeft:
				case EMemberType.LowerLeft:
					Nmx = Math.Floor(component.BraceConnect.Gusset.VerticalForceColumn - webPlate.BraceDistanceToFirstBolt - webPlate.Bolt.EdgeDistLongDir) / webPlate.Bolt.SpacingLongDir + 1;
					Nmn = (int)Math.Ceiling((component.BraceConnect.Gusset.VerticalForceColumn / 2) / webPlate.Bolt.SpacingLongDir);
					Th = component.BraceConnect.Gusset.Thickness;
					ercl = component.BraceConnect.Gusset.ColumnSideSetback;
					break;
			}

			if (Nmn < 2)
				Nmn = 2;

			if (webPlate.Bolt.NumberOfRows != 0 && webPlate.Bolt.NumberOfLines != 0)
			{
				Nbw = webPlate.Bolt.NumberOfRows;
				Nvl = webPlate.Bolt.NumberOfLines;
				ex = (ercl + webPlate.BraceBoltToVertEdgeDistGussetBeam + (Nvl - 1) * webPlate.Bolt.SpacingTransvDir / 2);
				if (Mom > 0 && V > 0)
				{
					ex = ex + Mom / V;
					theta = Math.Atan(Math.Abs(H_Max / V));
				}
				c = SmallMethodsDesign.EccIncLoadOnBG(Nvl, Nbw, ex, theta, webPlate.Bolt.SpacingLongDir, webPlate.Bolt.SpacingTransvDir);
				ShearCap = c * webPlate.Bolt.BoltStrength;
			}
			else
			{
				ShearCap = 0;
				Nvl = 0;
				Nbw = Nmn;
				while (ShearCap <= R_max && !(Nvl == 4 && Nbw == Nmx) && !exitwhile)
				{
					if (Nvl == webPlate.Bolt.NumberOfLines && webPlate.Bolt.NumberOfLines != 0 && Nbw >= Nmx)
						Nmx = (Nmx + 1);
					if (Nbw >= Nmx || Nvl == 0 || webPlate.Bolt.NumberOfRows != 0)
					{
						if (webPlate.Bolt.NumberOfRows == 0)
							Nbw = (Nmn - 1);
						if (webPlate.Bolt.NumberOfLines != 0)
							Nvl = webPlate.Bolt.NumberOfLines;
						else
							Nvl++;
					}
					if (webPlate.Bolt.NumberOfRows != 0)
						Nbw = webPlate.Bolt.NumberOfRows;
					else
						Nbw++;

					if (!webPlate.BraceDistanceToFirstBolt_User)
						webPlate.BraceDistanceToFirstBolt = (component.Shape.d - (Nbw - 1) * webPlate.Bolt.SpacingLongDir) / 2;
					AxialForceEccentricity = component.Shape.d / 2 - webPlate.BraceDistanceToFirstBolt - (Nbw - 1) * webPlate.Bolt.SpacingLongDir / 2;
					TranForce = Math.Max(component.AxialTension, component.AxialCompression);
					Mom = Math.Abs(TranForce * AxialForceEccentricity);

					ex = (ercl + webPlate.BraceBoltToVertEdgeDistGussetBeam + (Nvl - 1) * webPlate.Bolt.SpacingTransvDir / 2);
					if (Mom > 0 && V > 0)
					{
						ex = ex + Mom / V;
						theta = Math.Atan(Math.Abs(H_Max / V));
					}
					c = SmallMethodsDesign.EccIncLoadOnBG(Nvl, Nbw, ex, theta, webPlate.Bolt.SpacingLongDir, webPlate.Bolt.SpacingTransvDir);
					ShearCap = c * webPlate.Bolt.BoltStrength;
				}
				if (!webPlate.Bolt.NumberOfLines_User)
					webPlate.Bolt.NumberOfLines = Nvl;
				if (!webPlate.Bolt.NumberOfRows_User)
					webPlate.Bolt.NumberOfRows = Nbw;
			}
			webPlate.Bolt.NumberOfBolts = webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows;
			webPlate.Bolt.NumberOfRows = webPlate.Bolt.NumberOfRows;
			webPlate.Width = ercl + webPlate.BraceBoltToVertEdgeDistGussetBeam + (Nvl - 1) * webPlate.Bolt.SpacingTransvDir + webPlate.Bolt.EdgeDistTransvDir;
			webPlate.Length = webPlate.Bolt.SpacingLongDir * (Nbw - 1) + 2 * webPlate.Bolt.EdgeDistLongDir;

			if (webPlate.Thickness == 0)
				Thickness = ConstNum.QUARTER_INCH;
			else
				Thickness = webPlate.Thickness;

			// Combined Tension and Shear Rupture
			An = (webPlate.Length - Nbw * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * Thickness; //  (An=An1)
			if (R_Tens != 0)
			{
				a = H_Tens / R_Tens;
				b = V / R_Tens;
			}

			if (b > 0 && a > 0)
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.18 * (a / b) * (-1 + Math.Pow(1 + Math.Pow(b / a, 2) / 0.09, 0.5)) * An * webPlate.Material.Fu / b;
			else if (b == 0)
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * An * webPlate.Material.Fu;
			else if (a == 0)
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.6 * An * webPlate.Material.Fu;

			// Combined Tension and Shear Yielding
			Ag = webPlate.Length * Thickness;
			if (b > 0 && a > 0)
				GrossTensionandShearCapacity = Math.Pow(ConstNum.FIOMEGA1_0N, 2) * 0.18 * Math.Pow(a / b, 2) * (-1 / ConstNum.FIOMEGA0_9N + Math.Pow(1 / Math.Pow(ConstNum.FIOMEGA0_9N, 2) + Math.Pow(b / a, 2) / (0.09 * Math.Pow(ConstNum.FIOMEGA1_0N, 2)), 0.5)) * Ag * webPlate.Material.Fy / a;
			else if (b == 0)
				GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * Ag * webPlate.Material.Fy;
			else if (a == 0)
				GrossTensionandShearCapacity = ConstNum.FIOMEGA1_0N * 0.6 * Ag * webPlate.Material.Fy;
			TensionandShearCapacity = Math.Min(NetTensionandShearCapacity, GrossTensionandShearCapacity);
			if (TensionandShearCapacity < R_Tens)
				Thickness = Thickness * R_Tens / TensionandShearCapacity;

			// Block Shear 1
			An1 = (webPlate.Length - webPlate.Bolt.EdgeDistLongDir - (Nbw - 0.5) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * Thickness;
			An2 = (webPlate.Width - ercl - webPlate.BraceBoltToVertEdgeDistGussetBeam - (Nvl - 0.5) * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * Thickness;

			BlockShearCapacity1 = SmallMethodsDesign.ShearTensionInteraction(1, R_Tens, a, b, webPlate.Material.Fu, An1, An2);

			// Block Shear 2
			An1 = (webPlate.Length - 2 * webPlate.Bolt.EdgeDistLongDir - (Nbw - 1) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * Thickness;
			An2 = 2 * (webPlate.Width - ercl - webPlate.BraceBoltToVertEdgeDistGussetBeam - (Nvl - 0.5) * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * Thickness;

			BlockShearCapacity2 = SmallMethodsDesign.ShearTensionInteraction(2, R_Tens, a, b, webPlate.Material.Fu, An1, An2);

			BlockShearCapacity = Math.Min(BlockShearCapacity1, BlockShearCapacity2);
			if (BlockShearCapacity < R_Tens)
				Thickness = Thickness * R_Tens / BlockShearCapacity;

			// Bolt Bearing under Vertical Load
			Fbs = CommonCalculations.SpacingBearing(webPlate.Bolt.SpacingLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Bolt.HoleType, webPlate.Material.Fu, false);
			Fbe = CommonCalculations.EdgeBearing(webPlate.Bolt.EdgeDistLongDir, (webPlate.Bolt.HoleWidth), webPlate.Bolt.BoltSize, webPlate.Material.Fu, webPlate.Bolt.HoleType, false);

			EquivBoltFactor = c / (Nvl * Nbw);
			if (EquivBoltFactor > 1)
				EquivBoltFactor = 1;
			BearingCapacityV = EquivBoltFactor * Nvl * (Fbe + Fbs * (Nbw - 1)) * Thickness;
			if (BearingCapacityV < V)
				Thickness = Thickness * V / BearingCapacityV;

			// Bolt Bearing under Horizontal Load (Tension)

			Fbs = CommonCalculations.SpacingBearing(webPlate.Bolt.SpacingTransvDir, webPlate.Bolt.HoleLength, webPlate.Bolt.BoltSize, webPlate.Bolt.HoleType, webPlate.Material.Fu, false);
			Fbe = CommonCalculations.EdgeBearing(webPlate.Bolt.EdgeDistTransvDir, (webPlate.Bolt.HoleLength), webPlate.Bolt.BoltSize, webPlate.Material.Fu, webPlate.Bolt.HoleType, false);

			EquivBoltFactor = c / (Nvl * Nbw);
			if (EquivBoltFactor > 1)
				EquivBoltFactor = 1;
			BearingCapacityH_Tens = EquivBoltFactor * Nbw * (Fbe + Fbs * (Nvl - 1)) * Thickness;
			if (BearingCapacityH_Tens < H_Tens)
				Thickness = Thickness * H_Tens / BearingCapacityH_Tens;
			BearingCapacityH_Comp = EquivBoltFactor * Nbw * Fbs * Nvl * Thickness;
			if (BearingCapacityH_Comp < H_Comp)
				Thickness = Thickness * H_Comp / BearingCapacityH_Comp;

			L = webPlate.Length;
			d = webPlate.Bolt.HoleWidth;
			N = webPlate.Bolt.NumberOfRows;
			S = webPlate.Bolt.SpacingLongDir;
			E = webPlate.Bolt.EdgeDistLongDir;

			MomInert = (Math.Pow(L, 3) - N * Math.Pow(d, 3)) / 12 - d * N * (Math.Pow(L / 2 - E, 2) - (L / 2 - E) * S * (N - 1) + Math.Pow(S, 2) * (N - 1) * (2 * N - 1) / 6);
			SecMod = 2 * MomInert / L; // for unit thickness

			ex = (webPlate.Thickness + component.Shape.tw) / 2;
			Moment2 = H_Max * ex / 2;
			Moment = Mom + V * (ercl + webPlate.BraceBoltToVertEdgeDistGussetBeam);
			// Stress = H / An + Moment / SecMod + 6 * Moment2 / (SinglePlate(m).Thickness * An)
			Ln = webPlate.Length - webPlate.Bolt.NumberOfRows * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
			Stress = H_Max / Ln + Moment / SecMod + 6 * Moment2 / Ln;
			spt3 = Stress / (ConstNum.FIOMEGA0_9N * webPlate.Material.Fy);

			spt = Math.Max(Thickness, spt3);
			tminimum = webPlate.Length * Math.Sqrt(webPlate.Material.Fy) / 418;
			spt = Math.Max(spt, tminimum);

			switch (memberType)
			{
				case EMemberType.LeftBeam:
				case EMemberType.RightBeam:
					VertForce = V;
					break;
				default:
					VertForce = component.BraceConnect.Gusset.GussetEFCompression.Vc; //BRACE1.GussetEFCompression[m].Vc;
					break;
			}

			SmallMethodsDesign.SinglePlateBuckling(memberType, H_Comp, spt, Mom, VertForce); // GussetEFCompression(m).Vc

			if (!webPlate.Thickness_User)
				webPlate.Thickness = CommonCalculations.PlateThickness(spt);

			if (!webPlate.SupportWeldSize_User)
			{
				Fb = 6 * Mom / Math.Pow(L, 2);
				fr = Math.Sqrt(Math.Pow(H_Max / L + Fb, 2) + Math.Pow(V / L, 2));
				fraverage = Math.Sqrt(Math.Pow(H_Max / L + Fb / 2, 2) + Math.Pow(V / L, 2));
				w = Math.Max(fr, 1.25 * fraverage) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				webPlate.SupportWeldSize = NumberFun.Round(w, 16);
				Mfws = CommonCalculations.MinimumWeld(SupportThickness, webPlate.Thickness);
				webPlate.SupportWeldSize = Math.Max(webPlate.SupportWeldSize, Mfws);
				switch (memberType)
				{
					case EMemberType.RightBeam:
					case EMemberType.LeftBeam:
						if (!component.EndSetback_User)
							component.EndSetback = Math.Max(NumberFun.Round(webPlate.SupportWeldSize, 4), component.EndSetback);
						break;
					default:
						if (!component.BraceConnect.Gusset.ColumnSideSetback_User)
							component.BraceConnect.Gusset.ColumnSideSetback = Math.Max(NumberFun.Round(webPlate.SupportWeldSize, 4), component.BraceConnect.Gusset.ColumnSideSetback);
						break;
				}
			}

			Reporting.AddHeader("Design Single Plate");
			Reporting.AddLine("Plate Length = " + webPlate.Length + ConstUnit.Length);
			Reporting.AddLine("Plate Width = " + webPlate.Width + ConstUnit.Length);
			Reporting.AddLine("Plate Thickness = " + webPlate.Thickness + ConstUnit.Length);
			Reporting.AddLine("Bolts: (" + webPlate.Bolt.NumberOfBolts + ") " + webPlate.Bolt.BoltName);
			Reporting.AddLine("Bolt Holes on S. Plate: " + webPlate.Bolt.HoleLength + "\" Horiz. X " + webPlate.Bolt.HoleWidth + "\" Vert.");
			Reporting.AddLine("Bolt Holes on Gusset: " + webPlate.Bolt.HoleLength + "\" Horiz. X " + webPlate.Bolt.HoleWidth + "\" Vert.");

			if (webPlate.Bolt.BoltType != EBoltType.SC && webPlate.Bolt.HoleType != EBoltHoleType.STD)
			{
				Reporting.AddLine("WARNING");
				Reporting.AddLine("For this type of bolted construction, slotted holes");
				Reporting.AddLine("are permitted  normal to the direction of the load.  Here,");
				Reporting.AddLine("the load direction may be neither horizontal nor vertical");
				Reporting.AddLine("and it is likely to change during the loading histoy.");
				Reporting.AddLine("You need to make a judgment regarding this issue,");
				Reporting.AddLine("and consider using standard holes or slip critical bolts.");
			}
			// Bolts
			Reporting.AddHeader("Bolts");

			if (webPlate.Bolt.Slot0 && webPlate.Bolt.Slot1)
			{
				SminPlate = SmallMethodsDesign.MinBoltSpacing(webPlate.Bolt, webPlate.Material.Fu, webPlate.Thickness, V / webPlate.Bolt.NumberOfBolts, webPlate.Bolt.HoleWidth);
				SminGusset = SmallMethodsDesign.MinBoltSpacing(webPlate.Bolt, component.BraceConnect.Gusset.Material.Fu, Th, V / webPlate.Bolt.NumberOfBolts, webPlate.Bolt.HoleWidth);
			}
			else if (webPlate.Bolt.Slot0)
			{
				SminPlate = SmallMethodsDesign.MinBoltSpacing(webPlate.Bolt, webPlate.Material.Fu, webPlate.Thickness, V / webPlate.Bolt.NumberOfBolts, webPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH);
				SminGusset = SmallMethodsDesign.MinBoltSpacing(webPlate.Bolt, component.BraceConnect.Gusset.Material.Fu, Th, V / webPlate.Bolt.NumberOfBolts, webPlate.Bolt.HoleWidth);
			}
			else if (webPlate.Bolt.Slot1)
			{
				SminPlate = SmallMethodsDesign.MinBoltSpacing(webPlate.Bolt, webPlate.Material.Fu, webPlate.Thickness, V / webPlate.Bolt.NumberOfBolts, webPlate.Bolt.HoleWidth);
				SminGusset = SmallMethodsDesign.MinBoltSpacing(webPlate.Bolt, component.BraceConnect.Gusset.Material.Fu, Th, V / webPlate.Bolt.NumberOfBolts, webPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH);
			}
			else
			{
				SminPlate = SmallMethodsDesign.MinBoltSpacing(webPlate.Bolt, webPlate.Material.Fu, webPlate.Thickness, V / webPlate.Bolt.NumberOfBolts, webPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH);
				SminGusset = SmallMethodsDesign.MinBoltSpacing(webPlate.Bolt, component.BraceConnect.Gusset.Material.Fu, Th, V / webPlate.Bolt.NumberOfBolts, webPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH);
			}
			Smin = Math.Min(SminPlate, SminGusset);
			if (webPlate.Bolt.SpacingLongDir >= Smin)
				Reporting.AddLine("Bolt Vertical Spacing = " + webPlate.Bolt.SpacingLongDir + " >= Min. Spacing = " + Smin + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Bolt Vertical Spacing = " + webPlate.Bolt.SpacingLongDir + " << Min. Spacing = " + Smin + ConstUnit.Length + " (NG)");

			edgemin = Math.Max(webPlate.Bolt.MinEdgeSheared + (webPlate.Bolt.HoleWidth - webPlate.Bolt.HoleDiameterSTD) / 2, SmallMethodsDesign.MinClearDistForBearing(V / webPlate.Bolt.NumberOfBolts, webPlate.Thickness, webPlate.Material.Fu, webPlate.Bolt.HoleType) + webPlate.Bolt.HoleWidth / 2);
			if (webPlate.Bolt.EdgeDistLongDir >= edgemin)
				Reporting.AddLine("Vert. Edge Dist. on S. Plate = " + webPlate.Bolt.EdgeDistLongDir + " >= Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Vert. Edge Dist. on S. Plate = " + webPlate.Bolt.EdgeDistLongDir + " << Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (NG)");

			edgemin = Math.Max(webPlate.Bolt.MinEdgeSheared + (webPlate.Bolt.HoleWidth - webPlate.Bolt.HoleDiameterSTD) / 2,
				SmallMethodsDesign.MinClearDistForBearing(V / webPlate.Bolt.NumberOfBolts, Th, component.BraceConnect.Gusset.Material.Fu, webPlate.Bolt.HoleType) + webPlate.Bolt.HoleWidth / 2);
			edgeActual = component.BraceConnect.Gusset.VerticalForceColumn - webPlate.BraceDistanceToFirstBolt - (webPlate.Bolt.NumberOfRows - 1) * webPlate.Bolt.SpacingLongDir;
			if (edgeActual >= edgemin)
				Reporting.AddLine("Vert. Edge Dist. on Gusset = " + edgeActual + " >= Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Vert. Edge Dist. on Gusset = " + edgeActual + " << Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (NG)");
			Reporting.AddHeader("Bolt Shear Strength: ");

			ex = (ercl + webPlate.BraceBoltToVertEdgeDistGussetBeam + (webPlate.Bolt.NumberOfLines - 1) * webPlate.Bolt.SpacingTransvDir / 2);
			Momeffect = "";
			if (Mom > 0 && V > 0)
			{
				ex = ex + Mom / V;
				switch (memberType)
				{
					case EMemberType.RightBeam:
					case EMemberType.LeftBeam:
						Momeffect = " Includes the effect of Transfer Force Ecc.";
						break;
					default:
						Momeffect = " Includes the effect of Gusset Edge Moment.";
						break;
				}

				theta = Math.Atan(Math.Abs(H_Max / V));
			}

			c = SmallMethodsDesign.EccIncLoadOnBG(webPlate.Bolt.NumberOfLines, webPlate.Bolt.NumberOfRows, ex, theta, (webPlate.Bolt.SpacingLongDir), (webPlate.Bolt.SpacingTransvDir));
			ShearCap = c * webPlate.Bolt.BoltStrength;
			Reporting.AddLine("Eccentricity, ex = " + ex + ConstUnit.Length + Momeffect);
			Reporting.AddLine("Vertically: " + webPlate.Bolt.NumberOfRows + " Bolts with " + webPlate.Bolt.SpacingLongDir + ConstUnit.Length + " Spacing");
			Reporting.AddLine("Horizontally: " + webPlate.Bolt.NumberOfLines + " Bolts with " + webPlate.Bolt.SpacingTransvDir + ConstUnit.Length + " Spacing");
			Reporting.AddLine("Resultant Load (" + R_max + ConstUnit.Force + ") Inclined " + theta / ConstNum.RADIAN + " Degrees from Vertical");
			Reporting.AddLine("Inclined Eccentic Load Coefficient, C = " + c);
			if (ShearCap >= R_max)
				Reporting.AddLine("FiRn = C * Fv = " + c + " * " + webPlate.Bolt.BoltStrength + " = " + ShearCap + " >= " + R_max + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("FiRn = C * Fv = " + c + " * " + webPlate.Bolt.BoltStrength + " = " + ShearCap + " << " + R_max + ConstUnit.Force + " (NG)");

			// Bolt Bearing under Vertical Load
			Reporting.AddHeader("Bolt Bearing - Vertical Load:");
			Fbs = CommonCalculations.SpacingBearing(webPlate.Bolt.SpacingLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Bolt.HoleType, webPlate.Material.Fu, false);
			Fbe = CommonCalculations.EdgeBearing(webPlate.Bolt.EdgeDistLongDir, webPlate.Bolt.HoleWidth, webPlate.Bolt.BoltSize, webPlate.Material.Fu, webPlate.Bolt.HoleType, false);

			EquivBoltFactor = c / (webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows);
			if (EquivBoltFactor > 1)
				EquivBoltFactor = 1;

			Reporting.AddHeader("Equiv. Bolt Factor (ef) = C / Nb <= 1 = " + c + " / " + (webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows) + " = " + EquivBoltFactor);
			BearingCapacityV = EquivBoltFactor * webPlate.Bolt.NumberOfLines * (Fbe + Fbs * (webPlate.Bolt.NumberOfRows - 1)) * webPlate.Thickness;
			Reporting.AddLine("FiRn = ef * Nh * (Fbe + Fbs * (Nl - 1)) * t");
			Reporting.AddLine("= " + EquivBoltFactor + " * " + webPlate.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + webPlate.Bolt.NumberOfRows + "-1)) * " + webPlate.Thickness);
			if (BearingCapacityV >= V)
				Reporting.AddLine("= " + BearingCapacityV + " >= " + V + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + BearingCapacityV + " << " + V + ConstUnit.Force + " (NG)");

			// Bolt Bearing under Horizontal Load
			Reporting.AddHeader("Horizontal Load:");
			Fbs = CommonCalculations.SpacingBearing(webPlate.Bolt.SpacingTransvDir, webPlate.Bolt.HoleLength, webPlate.Bolt.BoltSize, webPlate.Bolt.HoleType, webPlate.Material.Fu, false);
			Fbe = CommonCalculations.EdgeBearing(webPlate.Bolt.EdgeDistTransvDir, webPlate.Bolt.HoleLength, webPlate.Bolt.BoltSize, webPlate.Material.Fu, webPlate.Bolt.HoleType, false);

			EquivBoltFactor = c / (webPlate.Bolt.NumberOfLines * webPlate.Bolt.NumberOfRows);
			if (EquivBoltFactor > 1)
				EquivBoltFactor = 1;

			Reporting.AddHeader("With Compressive Force:");
			BearingCapacityH_Comp = EquivBoltFactor * webPlate.Bolt.NumberOfRows * Fbs * webPlate.Bolt.NumberOfLines * webPlate.Thickness;
			Reporting.AddLine("FiRn = ef * Nl * Fbs * Nh  * t");
			Reporting.AddLine("= " + EquivBoltFactor + " * " + webPlate.Bolt.NumberOfRows + " * " + Fbs + " * " + webPlate.Bolt.NumberOfLines + " * " + webPlate.Thickness);
			if (BearingCapacityH_Comp >= H_Comp)
				Reporting.AddLine("= " + BearingCapacityH_Comp + " >= " + H_Comp + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + BearingCapacityH_Comp + " << " + H_Comp + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("With Tensile Force:");
			BearingCapacityH_Tens = EquivBoltFactor * webPlate.Bolt.NumberOfRows * (Fbe + Fbs * (webPlate.Bolt.NumberOfLines - 1)) * webPlate.Thickness;
			Reporting.AddLine("FiRn = ef * Nl * (Fbe + Fbs * (Nh - 1)) * t");
			Reporting.AddLine("= " + EquivBoltFactor + " * " + webPlate.Bolt.NumberOfRows + " * (" + Fbe + " + " + Fbs + " * (" + webPlate.Bolt.NumberOfLines + " - 1)) * " + webPlate.Thickness);
			if (BearingCapacityH_Tens >= H_Tens)
				Reporting.AddLine("= " + BearingCapacityH_Tens + " >= " + H_Tens + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + BearingCapacityH_Tens + " << " + H_Tens + ConstUnit.Force + " (NG)");

			SmallMethodsDesign.GussetandBeamCheckwithSinglePlate(memberType, V, H_Comp, H_Tens, R_Tens, EquivBoltFactor, EBearingTearOut.Bearing);

			Reporting.AddHeader("Plate Shear and Normal Stresses");
			// Combined Tension and Shear Rupture
			Reporting.AddLine("Single Plate Combined Tension and Shear:");
			Reporting.AddFormulaLine();

			if (R_Tens != 0)
			{
				a = H_Tens / R_Tens;
				b = V / R_Tens;
			}

			if (V == 0)
				Reporting.AddLine("Load Angle, " + 216 + " = Atn(H / V) = 90 Degees");
			else
				Reporting.AddLine("Load Angle, " + 216 + " = Atn(H / V) = " + (Math.Atan(Math.Abs(H_Tens / V)) / ConstNum.RADIAN) + " Degees");

			Reporting.AddLine("A = Sin(" + 216 + ") = " + a);
			Reporting.AddLine("B = Cos(" + 216 + ") = " + b);

			Reporting.AddHeader("Rupture:");
			An = (webPlate.Length - webPlate.Bolt.NumberOfRows * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * webPlate.Thickness;
			Reporting.AddLine("Net Area, An = (L - Nl * (dv + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + webPlate.Length + " - " + webPlate.Bolt.NumberOfRows + " * (" + webPlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + webPlate.Thickness);
			Reporting.AddLine("= " + An + ConstUnit.Area);
			if (b > 0 && a > 0)
			{
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.18 * (a / b) * (-1 + Math.Pow(1 + Math.Pow(b / a, 2) / 0.09, 0.5)) * An * webPlate.Material.Fu / b;
				Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * 0.18 * (A / B) * (-1 + (1 + (B / A)² / 0.09)^0.5) * An * Fu / B");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.18 * (" + a + " / " + b + ") * (-1 + (1 + (" + b + " / " + a + ")² / 0.09)^0.5) * " + An + " * " + webPlate.Material.Fu + " / " + b);
			}
			else if (b == 0)
			{
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * An * webPlate.Material.Fu;
				Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * An * Fu = " + ConstString.FIOMEGA0_75 + " * " + An + " * " + webPlate.Material.Fu);
			}
			else if (a == 0)
			{
				NetTensionandShearCapacity = ConstNum.FIOMEGA0_75N * 0.6 * An * webPlate.Material.Fu;
				Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * 0.6 * An * Fu = " + ConstString.FIOMEGA0_75 + " * 0.6 * " + An + " * " + webPlate.Material.Fu);
			}
			if (NetTensionandShearCapacity >= R_Tens)
				Reporting.AddLine("= " + NetTensionandShearCapacity + " >= " + R_Tens + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + NetTensionandShearCapacity + " << " + R_Tens + ConstUnit.Force + " (NG)");

			// Combined Tension and Shear Yielding
			Reporting.AddHeader("Yielding:");
			Ag = webPlate.Length * webPlate.Thickness;
			Reporting.AddLine("Ag= L * t = " + webPlate.Length + " * " + webPlate.Thickness + " = " + Ag + ConstUnit.Area);
			if (b > 0 && a > 0)
			{
				GrossTensionandShearCapacity = Math.Pow(ConstNum.FIOMEGA1_0N, 2) * 0.18 * Math.Pow(a / b, 2) * (-1 / ConstNum.FIOMEGA0_9N + Math.Pow(1 / Math.Pow(ConstNum.FIOMEGA0_9N, 2) + Math.Pow(b / a, 2) / (0.09 * Math.Pow(ConstNum.FIOMEGA1_0N, 2)), 0.5)) * Ag * component.Material.Fy / a;
				Reporting.AddLine("FiRn = " + ConstNum.FIOMEGA1_0N + "² * 0.18 * (A / B)² * (-1 / " + ConstString.FIOMEGA0_9 + " + (1 / (" + ConstString.FIOMEGA0_9 + "²) + (B / A)² / (0.09 * " + ConstNum.FIOMEGA1_0N + "²))^0.5) * Ag * Fy / A");
				Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + "² * 0.18 * (" + a + " / " + b + ")² * (-1 / " + ConstString.FIOMEGA0_9 + " + (1 / (" + ConstString.FIOMEGA0_9 + "²) + (" + b + " / " + a + ")² / (0.09 * " + ConstNum.FIOMEGA1_0N + "²))^0.5) * " + Ag + " * " + component.Material.Fy + " / " + a);
			}
			else if (b == 0)
			{
				GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * Ag * webPlate.Material.Fy;
				Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_9 + " * Ag * Fy = " + ConstString.FIOMEGA0_9 + " * " + Ag + " * " + webPlate.Material.Fy);
			}
			else if (a == 0)
			{
				GrossTensionandShearCapacity = ConstNum.FIOMEGA0_9N * 0.6 * Ag * webPlate.Material.Fy;
				Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_9 + " * 0.6 * Ag * Fy = " + ConstString.FIOMEGA0_9 + " * 0.6 * " + Ag + " * " + webPlate.Material.Fy);
			}
			if (GrossTensionandShearCapacity >= R_Tens)
				Reporting.AddLine("= " + GrossTensionandShearCapacity + " >= " + R_Tens + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + GrossTensionandShearCapacity + " << " + R_Tens + ConstUnit.Force + " (NG)");

			// Block Shear 1
			Reporting.AddHeader("Block Shear:");
			Reporting.AddLine("Vertical (An1, Ft1) and Horizontal (An2, Ft2) Sections:");
			Reporting.AddHeader("Pattern 1:");

			An1 = (webPlate.Length - webPlate.Bolt.EdgeDistLongDir - (webPlate.Bolt.NumberOfRows - 0.5) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * webPlate.Thickness;
			Reporting.AddLine("An1 = (L - Lv - (Nl - 0.5) * (dv + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + webPlate.Length + " - " + webPlate.Bolt.EdgeDistLongDir + " - (" + webPlate.Bolt.NumberOfRows + " - 0.5) * (" + webPlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + webPlate.Thickness);
			Reporting.AddLine("= " + An1 + ConstUnit.Area);

			An2 = (webPlate.Width - ercl - webPlate.BraceBoltToVertEdgeDistGussetBeam - (webPlate.Bolt.NumberOfLines - 0.5) * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * webPlate.Thickness;
			Reporting.AddLine("An2 = (W - c - Lh - (Nh - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + webPlate.Width + " - " + ercl + " - " + webPlate.BraceBoltToVertEdgeDistGussetBeam + " - (" + webPlate.Bolt.NumberOfLines + " - 0.5) * (" + webPlate.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + webPlate.Thickness);
			Reporting.AddLine("= " + An2 + ConstUnit.Area);

			Reporting.AddHeader("Pattern 2:");
			An1 = (webPlate.Length - 2 * webPlate.Bolt.EdgeDistLongDir - (webPlate.Bolt.NumberOfRows - 0.5) * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * webPlate.Thickness;
			Reporting.AddLine("An1 = (L - 2*Lv - (Nl - 0.5) * (dv + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + webPlate.Length + " - 2 * " + webPlate.Bolt.EdgeDistLongDir + " - (" + webPlate.Bolt.NumberOfRows + " - 0.5) * (" + webPlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + webPlate.Thickness);
			Reporting.AddLine("= " + An1 + ConstUnit.Area);

			An2 = 2 * (webPlate.Width - ercl - webPlate.BraceBoltToVertEdgeDistGussetBeam - (webPlate.Bolt.NumberOfLines - 0.5) * (webPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * webPlate.Thickness;
			Reporting.AddLine("An2 = 2 * (W - c - Lh - (Nh - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= 2 * (" + webPlate.Width + " - " + ercl + " - " + webPlate.BraceBoltToVertEdgeDistGussetBeam + " - (" + webPlate.Bolt.NumberOfLines + " - 0.5) * (" + webPlate.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + webPlate.Thickness);
			Reporting.AddLine("= " + An2 + ConstUnit.Area);

			SmallMethodsDesign.GussetandBeamCheckwithSinglePlate(memberType, V, H_Comp, H_Tens, R_Tens, ((int)EquivBoltFactor), EBearingTearOut.TearOut);

			Reporting.AddHeader("Plate Bending:");
			L = webPlate.Length;
			d = webPlate.Bolt.HoleWidth;
			N = webPlate.Bolt.NumberOfRows;
			S = webPlate.Bolt.SpacingLongDir;
			E = webPlate.Bolt.EdgeDistLongDir;
			MomInert = ((Math.Pow(L, 3) - N * Math.Pow(d, 3)) / 12 - d * N * (Math.Pow(L / 2 - E, 2) - (L / 2 - E) * S * (N - 1) + Math.Pow(S, 2) * (N - 1) * (2 * N - 1) / 6)) * webPlate.Thickness;
			SecMod = 2 * MomInert / L;
			An = (webPlate.Length - webPlate.Bolt.NumberOfRows * (webPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * webPlate.Thickness;
			Reporting.AddLine("Net Area (An) = " + An + ConstUnit.Area);
			Reporting.AddLine("Net Section Modulus (Sn) = " + SecMod + ConstUnit.SecMod);
			Moment = Mom + V * (ercl + webPlate.BraceBoltToVertEdgeDistGussetBeam);

			ex = (webPlate.Thickness + component.BraceConnect.Gusset.Thickness) / 2;
			Reporting.AddLine("e = (tp + tg) / 2 = (" + webPlate.Thickness + " + " + component.BraceConnect.Gusset.Thickness + ") / 2 = " + ex + ConstUnit.Area);

			Moment2 = H_Max * ex / 2;
			Stress = H_Max / An + Moment / SecMod + 6 * Moment2 / (webPlate.Thickness * An);
			Reporting.AddLine("Stress = H / An + (Mo + V * (c + Lh)) / Sn + 6 * (H * e / 2) / (t * An)");
			Reporting.AddLine("= " + H_Max + " / " + An + " + (" + Mom + " + " + V + " * (" + ercl + " + " + webPlate.BraceBoltToVertEdgeDistGussetBeam + ")) / " + SecMod + "+ 6 * (" + H_Max + " * " + ex + " / 2) / (" + webPlate.Thickness + " * " + An + ")");
			if (Stress <= ConstNum.FIOMEGA0_9N * webPlate.Material.Fy)
				Reporting.AddLine("= " + Stress + " <= " + ConstString.FIOMEGA0_9 + " * Fy = " + ConstNum.FIOMEGA0_9N * webPlate.Material.Fy + " ksi (OK)");
			else
				Reporting.AddLine("= " + Stress + " >> " + ConstString.FIOMEGA0_9 + " * Fy = " + ConstNum.FIOMEGA0_9N * webPlate.Material.Fy + " ksi (NG)");

			Reporting.AddHeader("Plate Buckling:");
			Plth = webPlate.Thickness;
			switch (memberType)
			{
				case EMemberType.RightBeam:
				case EMemberType.LeftBeam:
					VertForce = V;
					break;
				default:
					VertForce = component.BraceConnect.Gusset.GussetEFCompression.Vc;
					break;
			}

			SmallMethodsDesign.SinglePlateBuckling(memberType, H_Comp, Plth, Mom, VertForce); // GussetEFCompression(m).Vc

			Reporting.AddHeader("Plate to Column Weld:");
			Fb = 6 * Mom / Math.Pow(L, 2);
			fr = Math.Sqrt(Math.Pow(H_Max / L + Fb, 2) + Math.Pow(V / L, 2));
			fraverage = Math.Sqrt(Math.Pow(H_Max / L + Fb / 2, 2) + Math.Pow(V / L, 2));
			w = Math.Max(fr, 1.25 * fraverage) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
			Mfws = CommonCalculations.MinimumWeld(SupportThickness, webPlate.Thickness);
			if (webPlate.SupportWeldSize >= Mfws)
				Reporting.AddLine("Weld Size = " + webPlate.SupportWeldSize + " >= Min. Weld Size = " + Mfws + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Weld Size = " + webPlate.SupportWeldSize + " << Min. Weld Size = " + Mfws + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Weld Stresses:");
			Reporting.AddLine("fr = [((H / L) + 6 * Mo / L²)² + (V / L)²]^0.5");
			Reporting.AddLine("= [((" + H_Max + " / " + L + ") + 6 * " + Mom + " / " + L + "²)² + (" + V + " / " + L + ")²]^0.5");
			Reporting.AddLine("= " + fr + ConstUnit.ForcePerUnitLength);
			Reporting.AddLine("fraverage = [((H / L) + 3 * Mo / L²)² + (V / L)²]^0.5");
			Reporting.AddLine("= [((" + H_Max + " / " + L + ") + 3 * " + Mom + " / " + L + "²)² + (" + V + " / " + L + ")²]^0.5");
			Reporting.AddLine("= " + fraverage + ConstUnit.ForcePerUnitLength);
			Reporting.AddLine("Required Weld Size = Max(fr, 1.25 * fraverage) / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * Fexx)");
			Reporting.AddLine("= Max(" + fr + ", 1.25 * " + fraverage + ") / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
			if (w <= webPlate.SupportWeldSize)
				Reporting.AddLine("= " + w + " <= " + CommonCalculations.WeldSize(webPlate.SupportWeldSize) + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("= " + w + " >> " + CommonCalculations.WeldSize(webPlate.SupportWeldSize) + ConstUnit.Length + " (NG)");

			usefulweldsize = Math.Min(ConstNum.FIOMEGA0_75N * 0.6 * webPlate.Thickness * webPlate.Material.Fu, 2 * ConstNum.FIOMEGA0_75N * 0.6 * SupportThickness * CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Material.Fu) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
			Reporting.AddHeader("Useful weld size:");
			Reporting.AddLine("= Min(" + ConstString.FIOMEGA0_75 + " * 0.6 * tp * Fup, 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * tc * Fuc) / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * Fexx)");
			Reporting.AddLine("= Min(" + ConstString.FIOMEGA0_75 + " * 0.6 * " + webPlate.Thickness + " * " + webPlate.Material.Fu + ", 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + SupportThickness + " * " + CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Material.Fu + ") / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
			if (usefulweldsize >= w)
				Reporting.AddLine("= " + usefulweldsize + " >= " + w + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("= " + usefulweldsize + " << " + w + ConstUnit.Length + " (NG)");

			if (column.WebOrientation == EWebOrientation.OutOfPlane && column.ShapeType == EShapeType.WideFlange)
			{
				var otherSide = new DetailData();
				var otherType = new EMemberType();
				var otherUpperBrace = new DetailData();
				var otherLowerBrace = new DetailData();

				switch (memberType)
				{
					case EMemberType.RightBeam:
						otherType = EMemberType.LeftBeam;
						otherSide = CommonDataStatic.DetailDataDict[otherType];
						break;
					case EMemberType.LeftBeam:
						otherType = EMemberType.RightBeam;
						otherSide = CommonDataStatic.DetailDataDict[otherType];
						break;
					case EMemberType.UpperRight:
						otherType = EMemberType.UpperLeft;
						otherSide = CommonDataStatic.DetailDataDict[otherType];
						break;
					case EMemberType.LowerRight:
						otherType = EMemberType.LowerLeft;
						otherSide = CommonDataStatic.DetailDataDict[otherType];
						break;
					case EMemberType.UpperLeft:
						otherType = EMemberType.UpperRight;
						otherSide = CommonDataStatic.DetailDataDict[otherType];
						break;
					case EMemberType.LowerLeft:
						otherType = EMemberType.LowerRight;
						otherSide = CommonDataStatic.DetailDataDict[otherType];
						break;
				}

				switch (memberType)
				{
					case EMemberType.RightBeam:
					case EMemberType.UpperRight:
					case EMemberType.LowerRight:
						otherUpperBrace = CommonDataStatic.DetailDataDict[EMemberType.UpperLeft];
						otherLowerBrace = CommonDataStatic.DetailDataDict[EMemberType.LowerLeft];
						break;
					case EMemberType.LeftBeam:
					case EMemberType.UpperLeft:
					case EMemberType.LowerLeft:
						otherUpperBrace = CommonDataStatic.DetailDataDict[EMemberType.UpperRight];
						otherLowerBrace = CommonDataStatic.DetailDataDict[EMemberType.LowerRight];
						break;
				}

				shrt2 = otherUpperBrace.IsActive && otherUpperBrace.GussetToColumnConnection == EBraceConnectionTypes.DirectlyWelded;
				shrt3 = otherLowerBrace.IsActive && otherLowerBrace.GussetToColumnConnection == EBraceConnectionTypes.DirectlyWelded;

				if (shrt2 || shrt3)
				{
					Reporting.AddHeader("Check the effect of the member connected to the other side of the column web, if there is one.");
				}
				if (otherSide.IsActive && otherSide.GussetToColumnConnection == EBraceConnectionTypes.SinglePlate)
				{
					switch (otherType)
					{
						case EMemberType.RightBeam:
						case EMemberType.LeftBeam:
							MomO = 0;
							LO = otherSide.WinConnect.ShearWebPlate.Length;
							ho = Math.Abs(otherSide.WinConnect.ShearClipAngle.ForceX);
							VO = Math.Abs(otherSide.WinConnect.ShearClipAngle.ForceY);
							break;
						default:
							MomO = otherSide.BraceConnect.Gusset.Mc;
							LO = otherSide.WinConnect.ShearWebPlate.Length;
							ho = Math.Abs(otherSide.BraceConnect.Gusset.Hc);
							VO = Math.Abs(otherSide.BraceConnect.Gusset.VerticalForceColumn);
							break;
					}
					Fb = 6 * MomO / Math.Pow(L, 2);
					fr = Math.Sqrt(Math.Pow(ho / LO + Fb, 2) + Math.Pow(VO / LO, 2));
					fraverage = Math.Sqrt(Math.Pow(ho / LO + Fb / 2, 2) + Math.Pow(VO / LO, 2));
					wother = Math.Max(fr, 1.25 * fraverage) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);

					Reporting.AddHeader("Weld size required on other side of web = " + wother + ConstUnit.Length);
					usefulweldsize = 2 * ConstNum.FIOMEGA0_75N * 0.6 * SupportThickness * CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Material.Fu / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddHeader("Max. weld column web can develop:");
					Reporting.AddLine("= 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * tw * Fu / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * Fexx)");
					Reporting.AddLine("= 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + SupportThickness + " * " + CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Material.Fu + " / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
					if (usefulweldsize >= w + wother)
						Reporting.AddLine("= " + usefulweldsize + " >= (w + w_Other Side) = " + w + wother + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + usefulweldsize + " << (w + w_Other Side) = " + w + wother + ConstUnit.Length + " (NG)");
				}
			}
		}
	}
}