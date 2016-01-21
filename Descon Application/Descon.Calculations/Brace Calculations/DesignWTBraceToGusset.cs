using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class DesignWTBraceToGusset
	{
		internal static void CalcDesignWTBraceToGusset(EMemberType memberType)
		{
			double Bracedevelopes = 0;
			double WeldCapacity = 0;
			double weldforcapacity = 0;
			double Ant2 = 0;
			double Ant1 = 0;
			double AntW = 0;
			double AgtW = 0;
			double Anv = 0;
			double Ant = 0;
			double Agv = 0;
			double Agt = 0;
			double SpacingbearingCapComp = 0;
			double SpacingbearingCap = 0;
			double edgebearingCap = 0;
			double FbreStag = 0;
			double edgdist = 0;
			double capacity = 0;
			double smin1 = 0;
			double sminG = 0;
			double sminB = 0;
			double spacing = 0;
			double edgemin = 0;
			double Smin = 0;
			double SminGusset = 0;
			double SminBrace = 0;
			int Ntotal = 0;
			double excessforce = 0;
			double AgStiffener = 0;
			double AgAdditionalforRupture = 0;
			double An_effective = 0;
			double excessForceR = 0;
			double FiRn = 0;
			double U = 0;
			double L = 0;
			double An2 = 0;
			double An1 = 0;
			double An = 0;
			double excessForceY = 0;
			double AgAdditionalforYielding = 0;
			double AgrossYielding = 0;
			double checkwidth = 0;
			double weldlength = 0;
			double estimatedweldsize = 0;
			double usefulweldsizeforbrace = 0;
			double maxweldsize = 0;
			double minweldsize = 0;
			double WeldArea = 0;
			int N_RequiredForBlockShear = 0;
			double N_RequiredBComp = 0;
			double N_RequiredB = 0;
			double Fbrs = 0;
			double Fbre = 0;
			double np = 0;
			int N_RequiredS = 0;
			double g2 = 0;
			double g1 = 0;
			double g0 = 0;
			double t = 0;
			double PflangeMx = 0;
			double PflangeC = 0;
			double PflangeT = 0;
			double x_ = 0;
			string r_y_String = "";
			string r_t_String = "";
			string Yildiz = "";
			double r_y_Strength = 0;
			double r_t_Strength = 0;
			double r_y = 0;
			double r_t = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];
			double braceweldsize;
			double betaN;

			SmallMethodsDesign.ExpectedStrengthCoefficient(component.MemberType, ref r_t, ref r_y, ref r_t_Strength, ref r_y_Strength, ref Yildiz, ref r_t_String, ref r_y_String);

			x_ = SmallMethodsDesign.TeeExcentricity(component.Shape.bf, component.Shape.tf, component.Shape.d, component.Shape.tw, component.Shape.a);
			component.BraceConnect.ClawAngles.DoNotConnectFlanges = true;
			component.BraceConnect.SplicePlates.DoNotConnectWeb = true;
			PflangeT = component.AxialTension / 2.0;
			PflangeC = component.AxialCompression / 2.0;
			PflangeMx = component.BraceConnect.Brace.MaxForce / 2.0;

			t = component.Shape.tf;
			g0 = component.GageOnFlange; // ElProp(m).g1
			g1 = g0;
            g2 = Math.Min(ConstNum.THREE_INCHES, ((int)Math.Floor(4 * ((component.Shape.bf - g1) / 2 - (component.BoltBrace.MinEdgeRolled + component.BoltBrace.Eincr)))) / 4.0);

			if (component.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
			{
                N_RequiredS = ((int)(PflangeMx / component.BoltBrace.BoltStrength)); // for bolt shear
				if (PflangeT > 0)
				{
					// (tensionandcompression Or member(m).Fx > 0) Then
					np = SmallMethodsDesign.NBRequiredForGussetBlockShear(memberType, 0, N_RequiredS);
				}
				else
					np = 0;
				if (N_RequiredS < np)
					N_RequiredS = ((int) np);
				if (g2 < NumberFun.ConvertFromFraction(28))
					g2 = 0;
				if (g2 == 0 && component.BoltBrace.NumberOfRows == 2)
					component.BoltBrace.NumberOfRows = 1;
				if (N_RequiredS > 3 && g2 > 0 && component.BoltBrace.NumberOfRows == 2)
				{
                    component.BoltBrace.EdgeDistTransvDir = (component.Shape.bf - g1 - 2 * g2) / 2;
					Fbre = CommonCalculations.EdgeBearing(component.BoltBrace.EdgeDistLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.Material.Fu, component.BoltBrace.HoleType, false);
					Fbrs = CommonCalculations.SpacingBearing(component.BoltBrace.SpacingLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.BoltBrace.HoleType, component.Material.Fu, false);
					N_RequiredB = (PflangeT / component.Shape.tf - 2 * Fbre) / Fbrs + 2; // for bearing on WT Flange
					N_RequiredBComp = PflangeC / component.Shape.tf / Fbrs; // for bearing on WT Flange
					N_RequiredB = Math.Max(N_RequiredB, N_RequiredBComp);

					if (g2 < 2.667 * component.BoltBrace.BoltSize)
						component.BraceConnect.Brace.BoltsAreStaggered = true;
					else
						component.BraceConnect.Brace.BoltsAreStaggered = false;
					if (component.AxialTension > 0)
						N_RequiredForBlockShear = SmallMethodsDesign.NBRequiredforBraceBlockShear(memberType, t, EShapeType.WTSection);
					else
						N_RequiredForBlockShear = 0;

					component.BoltBrace.NumberOfBolts = (-((int) Math.Floor(-Math.Max(N_RequiredS, Math.Max(N_RequiredB, N_RequiredForBlockShear)))));
					if (!component.EndSetback_User && component.BoltBrace.NumberOfBolts % 2 == 0 && !component.BraceConnect.Brace.BoltsAreStaggered)
						component.EndSetback = -(component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir * (component.BoltBrace.NumberOfBolts / 2 - 1) + component.BraceConnect.Gusset.EdgeDistance);
					else if(!component.EndSetback_User)
						component.EndSetback = -(component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir * ((component.BoltBrace.NumberOfBolts + 1) / 2 - 1) + component.BraceConnect.Gusset.EdgeDistance);
				}
				else
				{
					component.BraceConnect.Brace.BoltsAreStaggered = false;
					component.BoltBrace.EdgeDistTransvDir = (component.Shape.bf - g0) / 2;
					Fbre = CommonCalculations.EdgeBearing(component.BoltBrace.EdgeDistLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.Material.Fu, component.BoltBrace.HoleType, false);
					Fbrs = CommonCalculations.SpacingBearing(component.BoltBrace.SpacingLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.BoltBrace.HoleType, component.Material.Fu, false);
					N_RequiredB = (PflangeT / component.Shape.tf - Fbre) / Fbrs + 1; // for bearing on WT flange
					N_RequiredBComp = PflangeC / component.Shape.tf / Fbrs; // for bearing on WT flange
					N_RequiredB = Math.Max(N_RequiredB, N_RequiredBComp);
					component.BoltBrace.NumberOfBolts = (-((int) Math.Floor(-Math.Max(N_RequiredS, Math.Max(N_RequiredB, N_RequiredForBlockShear)))));
					component.BoltBrace.NumberOfBolts = (-((int) Math.Floor(-Math.Max(N_RequiredS, Math.Max(N_RequiredB, N_RequiredForBlockShear)))));

					if (component.BoltBrace.NumberOfBolts < 2)
						component.BoltBrace.NumberOfBolts = 2;
					if (!component.EndSetback_User)
						component.EndSetback = -(component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir * (component.BoltBrace.NumberOfBolts - 1) + component.BraceConnect.Gusset.EdgeDistance);
				}
				component.BraceConnect.Brace.BraceWeld.Weld1L = -component.EndSetback;
			}
			else
			{
				WeldArea = PflangeMx / (0.707 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				minweldsize = CommonCalculations.MinimumWeld(component.Shape.tf, component.BraceConnect.Gusset.Thickness);
				if (component.Shape.tf < ConstNum.QUARTER_INCH)
					maxweldsize = component.Shape.tf;
				else
					maxweldsize = component.Shape.tf - ConstNum.SIXTEENTH_INCH;
				if (minweldsize > maxweldsize)
					minweldsize = maxweldsize;
				usefulweldsizeforbrace = 1.414 * component.Shape.tf * component.Material.Fu / CommonDataStatic.Preferences.DefaultElectrode.Fexx;
				estimatedweldsize = Math.Max(minweldsize, usefulweldsizeforbrace / 2);
				if (estimatedweldsize > maxweldsize)
					estimatedweldsize = maxweldsize;
				estimatedweldsize = Math.Min(estimatedweldsize, Math.Pow(WeldArea, 0.5) / 10);
				if (component.BraceConnect.Brace.BraceWeld.Weld1sz < estimatedweldsize)
					component.BraceConnect.Brace.BraceWeld.Weld1sz = estimatedweldsize;

				weldlength = WeldArea / Math.Min(component.BraceConnect.Brace.BraceWeld.Weld1sz, usefulweldsizeforbrace);
				checkwidth = Math.Max(component.Shape.bf, component.Shape.d);
				if (weldlength < checkwidth)
					weldlength = checkwidth;
				if (-component.EndSetback > weldlength)
				{
					component.BraceConnect.Brace.BraceWeld.Weld1L = -component.EndSetback;
					braceweldsize = WeldArea / (2 * component.BraceConnect.Brace.BraceWeld.Weld1L);
					component.BraceConnect.Brace.BraceWeld.Weld1sz = -(-16 * Math.Min(braceweldsize, usefulweldsizeforbrace)) / 16;
					if (component.BraceConnect.Brace.BraceWeld.Weld1sz < braceweldsize)
						component.BraceConnect.Brace.BraceWeld.Weld1L = WeldArea / (2 * Math.Min(component.BraceConnect.Brace.BraceWeld.Weld1sz, usefulweldsizeforbrace));
				}
				else
					component.BraceConnect.Brace.BraceWeld.Weld1L = weldlength;

				if (component.BraceConnect.Brace.BraceWeld.Weld1sz > usefulweldsizeforbrace)
					component.BraceConnect.Brace.BraceWeld.Weld1L = component.BraceConnect.Brace.BraceWeld.Weld1L * component.BraceConnect.Brace.BraceWeld.Weld1sz / usefulweldsizeforbrace;

				if (!component.EndSetback_User)
					component.EndSetback = (((int)Math.Floor(-component.BraceConnect.Brace.BraceWeld.Weld1L * 4)) / 4.0);
				component.BraceConnect.Brace.BraceWeld.Weld1L = -component.EndSetback;
				component.BraceConnect.Brace.BraceWeld.Weld1sz = NumberFun.ConvertFromFraction((int) (WeldArea / component.BraceConnect.Brace.BraceWeld.Weld1L));
				if (component.BraceConnect.Brace.BraceWeld.Weld1sz < minweldsize)
					component.BraceConnect.Brace.BraceWeld.Weld1sz = minweldsize;
			}
			component.BraceConnect.Brace.BraceWeld.Weld2sz = 0;
			// Required Gross Area:
			AgrossYielding = component.AxialTension / (ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy);
			AgAdditionalforYielding = Math.Max(0, AgrossYielding - component.Shape.a) * (r_y * component.Material.Fy / component.BraceConnect.Gusset.Material.Fy);
			excessForceY = component.AxialTension - ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy * component.Shape.a;
			// Required Net Area:
			if (component.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
			{
				if (component.BoltBrace.NumberOfRows == 1)
					An = component.Shape.a - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
				else if (component.BraceConnect.Brace.BoltsAreStaggered)
				{
					An1 = component.Shape.a - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
					An2 = component.Shape.a + 2 * (-2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) + Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2)) * t;
					An = Math.Min(An1, An2);
				}
				else
				{
					if (component.BoltBrace.NumberOfBolts % 2 == 0)
						An = component.Shape.a - 4 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
					else
					{
						An1 = component.Shape.a - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
						An2 = component.Shape.a + 2 * (Math.Pow(component.BoltBrace.SpacingLongDir, 2) / (4 * g2) - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH)) * t;
						An = Math.Min(An1, An2);
					}
				}
				L = -component.EndSetback - component.BoltBrace.EdgeDistLongDir - component.BraceConnect.Gusset.EdgeDistance;
				U = 1 - x_ / L;
			}
			else
			{
				L = -component.EndSetback;
				U = 1 - x_ / L;
				An = component.Shape.a;
			}

			FiRn = ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu * U * An;
			excessForceR = component.AxialTension - FiRn;
			An_effective = component.AxialTension / (ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu);
			AgAdditionalforRupture = Math.Max(0, An_effective / U + component.Shape.a - An - component.Shape.a) * (r_t * component.Material.Fu / component.BraceConnect.Gusset.Material.Fu);

			AgStiffener = Math.Max(AgAdditionalforYielding, AgAdditionalforRupture);
			if (AgStiffener > 0)
			{
				component.BraceStiffener.Thickness = CommonCalculations.PlateThickness(((int) (AgStiffener / (component.Shape.d - component.Shape.kdet - ConstNum.ONEANDHALF_INCHES))));
				component.BraceStiffener.WeldSize = -((int) Math.Floor(-16 * (Math.Min(component.Material.Fu * component.Shape.tf, component.BraceConnect.Gusset.Material.Fu * component.BraceStiffener.Thickness) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx)))) / 16.0;
			}
			excessforce = Math.Max(0, Math.Max(excessForceY, excessForceR));
			component.BraceStiffener.Width = component.Shape.d - component.Shape.kdet - ConstNum.ONEANDHALF_INCHES;
			component.BraceStiffener.Length = -component.EndSetback + weldlength;
			component.BraceStiffener.Area = AgStiffener;
			component.BraceStiffener.Force = excessforce;

			switch (component.BraceToGussetWeldedOrBolted)
			{
				case EConnectionStyle.Bolted:
					Ntotal = 2 * component.BoltBrace.NumberOfBolts;
					Reporting.AddHeader(CommonDataStatic.CommonLists.CompleteMemberList[component.MemberType] + " to Gusset Connection");
					Reporting.AddLine("Brace Member: " + component.Shape.Name + " - " + component.Material.Name);
					Reporting.AddLine("Brace Force (Tension): " + component.AxialTension + ConstUnit.Force);
					Reporting.AddLine("Brace Force (Compression): " + component.AxialCompression + ConstUnit.Force);
					Reporting.AddLine("Bolts: (" + Ntotal + ")" + component.BoltBrace.BoltName);
					Reporting.AddLine("Bolt Holes on Brace:" + component.BraceConnect.Brace.WebTrans + "\" Transv. X " + component.BraceConnect.Brace.WebLong + "\" Longit.");
					Reporting.AddLine("Bolt Holes on Gusset:" + component.BraceConnect.Gusset.HoleTransP + "\" Transv. X " + component.BraceConnect.Gusset.HoleLongP + "\" Longit.");

					if (component.BoltBrace.Slot0 && component.BoltBrace.Slot1)
					{
						SminBrace = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, component.Shape.tf, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BraceConnect.Brace.WebLong);
						SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.BraceConnect.Gusset.Material.Fu, component.BraceConnect.Gusset.Thickness, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BraceConnect.Gusset.HoleLongP);
					}
					else if (component.BoltBrace.Slot0)
					{
						SminBrace = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, component.Shape.tf, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BoltBrace.BoltSize + ConstNum.SIXTEENTH_INCH);
						SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.BraceConnect.Gusset.Material.Fu, component.BraceConnect.Gusset.Thickness, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BraceConnect.Gusset.HoleLongP);
					}
					else if (component.BoltBrace.Slot1)
					{
						SminBrace = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, component.Shape.tf, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BraceConnect.Brace.WebLong);
						SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.BraceConnect.Gusset.Material.Fu, component.BraceConnect.Gusset.Thickness, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BoltBrace.BoltSize + ConstNum.SIXTEENTH_INCH);
					}
					else
					{
						SminBrace = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, component.Shape.tf, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BoltBrace.BoltSize + ConstNum.SIXTEENTH_INCH);
						SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.BraceConnect.Gusset.Material.Fu, component.BraceConnect.Gusset.Thickness, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BoltBrace.BoltSize + ConstNum.SIXTEENTH_INCH);
					}
					Smin = Math.Min(SminBrace, SminGusset);
					if (component.BoltBrace.SpacingLongDir >= Smin)
						Reporting.AddLine("Longitudinal Bolt Spacing = " + component.BoltBrace.SpacingLongDir + " >= Min. Spacing = " + Smin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Longitudinal Bolt Spacing = " + component.BoltBrace.SpacingLongDir + " << Min. Spacing = " + Smin + ConstUnit.Length + " (NG)");

					// edgemin = Max(BraceBolts(m).MinEdgeSheared, 2 * member(m).Pflange / (BraceBolts(m).n * member(m).Fu * ElProp(m).tf)) + BraceBolts(m).eali
					edgemin = Math.Max(component.BoltBrace.MinEdgeSheared + component.BoltBrace.Eincr, SmallMethodsDesign.MinClearDistForBearing((component.AxialTension / 2 / component.BoltBrace.NumberOfBolts), component.Shape.tf, component.Material.Fu, component.BoltBrace.HoleType) + component.BraceConnect.Brace.WebLong / 2);
					if (component.BoltBrace.EdgeDistLongDir >= edgemin)
						Reporting.AddLine("Longitudinal Edge Distance on Brace = " + component.BoltBrace.EdgeDistLongDir + " >= Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Longitudinal Edge Distance on Brace = " + component.BoltBrace.EdgeDistLongDir + " << Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (NG)");

					// edgemin = Max(BraceBolts(m).MinEdgeSheared, 2 * member(m).Pflange / (BraceBolts(m).n * Gusset(m).Fu * Gusset(m).th)) + Gusset(m).EincrP
					edgemin = Math.Max(component.BoltBrace.MinEdgeSheared + component.BraceConnect.Gusset.EincrP, SmallMethodsDesign.MinClearDistForBearing((component.AxialTension / 2 / component.BoltBrace.NumberOfBolts), component.BraceConnect.Gusset.Thickness, component.BraceConnect.Gusset.Material.Fu, component.BoltBrace.HoleType) + component.BraceConnect.Gusset.HoleLongP / 2);
					if (component.BoltBrace.EdgeDistGusset >= edgemin)
						Reporting.AddLine("Longitudinal Edge Distance on Gusset = " + component.BoltBrace.EdgeDistGusset + " >= Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Longitudinal Edge Distance on Gusset = " + component.BoltBrace.EdgeDistGusset + " << Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (NG)");

					if (component.BoltBrace.NumberOfRows == 2)
					{
						g1 = component.GageOnFlange; // ElProp(m).g1
						g2 = Math.Min(3, ((int)Math.Floor(4 * ((component.Shape.bf - g1) / 2 - (component.BoltBrace.MinEdgeRolled + component.BoltBrace.Eincr)))) / 4);
						Smin = ConstNum.EIGHT_THIRDS * component.BoltBrace.BoltSize;
						spacing = g2;
						if (component.BraceConnect.Brace.BoltsAreStaggered)
						{
							if (component.BoltBrace.SpacingLongDir / 2 - component.BraceConnect.Brace.WebLong < component.BoltBrace.BoltSize)
							{
								sminB = component.BraceConnect.Brace.WebTrans + Math.Sqrt(Math.Pow(component.BoltBrace.BoltSize, 2) - Math.Pow(component.BoltBrace.SpacingLongDir / 2 - component.BraceConnect.Brace.WebLong, 2));
								sminB = Math.Sqrt(Math.Pow(sminB, 2) + Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2));
							}
							else
								sminB = Math.Sqrt(Math.Pow(component.BraceConnect.Brace.WebTrans, 2) + Math.Pow(component.BoltBrace.BoltSize + component.BraceConnect.Brace.WebLong, 2));
							if (component.BoltBrace.SpacingLongDir / 2 - component.BraceConnect.Gusset.HoleLongP < component.BoltBrace.BoltSize)
							{
								sminG = component.BraceConnect.Gusset.HoleTransP + Math.Sqrt(Math.Pow(component.BoltBrace.BoltSize, 2) - Math.Pow(component.BoltBrace.SpacingLongDir / 2 - component.BraceConnect.Gusset.HoleLongP, 2));
								sminG = Math.Sqrt(Math.Pow(sminG, 2) + Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2));
							}
							else
								sminG = Math.Sqrt(Math.Pow(sminG = component.BraceConnect.Gusset.HoleTransP, 2) + Math.Pow(component.BoltBrace.BoltSize + component.BraceConnect.Gusset.HoleLongP, 2));
							
							smin1 = Math.Max(Smin, Math.Max(sminB, sminG));
							if (Math.Sqrt(Math.Pow(Math.Max(component.BraceConnect.Gusset.HoleTransP, component.BraceConnect.Brace.WebTrans), 2) + Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2)) > smin1)
								Smin = Math.Max(component.BraceConnect.Gusset.HoleTransP, component.BraceConnect.Brace.WebTrans);
							else
								Smin = Math.Sqrt(Math.Pow(smin1, 2) - Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2));
						}
						else
						{
							sminB = component.BraceConnect.Brace.WebTrans + component.BoltBrace.BoltSize;
							sminG = component.BraceConnect.Gusset.HoleTransP + component.BoltBrace.BoltSize;
							smin1 = Math.Max(sminB, sminG);
							Smin = Math.Max(Smin, smin1);
						}
						if (spacing >= Smin)
							Reporting.AddLine("Transverse Bolt Spacing = " + spacing + " >= Min. Spacing = " + Smin + ConstUnit.Length + " (OK)");
						else
							Reporting.AddLine("Transverse Bolt Spacing = " + spacing + " << Min. Spacing = " + Smin + ConstUnit.Length + " (NG)");
					}
					Reporting.AddLine(" ");
					edgemin = component.BoltBrace.MinEdgeRolled + component.BoltBrace.Eincr;
					if (component.BoltBrace.EdgeDistTransvDir >= edgemin)
						Reporting.AddLine("Transverse Edge Distance on Brace = " + component.BoltBrace.EdgeDistTransvDir + " >= Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Transverse Edge Distance on Brace = " + component.BoltBrace.EdgeDistTransvDir + " << Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Bolt Shear Strength:");
					capacity = Ntotal * component.BoltBrace.BoltStrength;
					if (capacity >= component.BraceConnect.Brace.MaxForce)
						Reporting.AddLine(ConstString.PHI + " Rn = N * Fv = " + Ntotal + " * " + component.BoltBrace.BoltStrength + " = " + capacity + " >= " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(ConstString.PHI + " Rn = N * Fv = " + Ntotal + " * " + component.BoltBrace.BoltStrength + " = " + capacity + " << " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Brace Check " + CommonDataStatic.CommonLists.CompleteMemberList[MiscMethods.ComponentMinusOne(component.MemberType)]);
					Reporting.AddHeader("Bolt Bearing on Brace:");
					Fbre = CommonCalculations.EdgeBearing(component.BoltBrace.EdgeDistLongDir, (component.BraceConnect.Brace.WebLong), component.BoltBrace.BoltSize, component.Material.Fu, component.BoltBrace.HoleType, false);
					if (component.BoltBrace.NumberOfRows == 2 && component.BraceConnect.Brace.BoltsAreStaggered)
					{
						edgdist = component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir / 2;
						Reporting.AddHeader("For the second bolt line:");
						FbreStag = CommonCalculations.EdgeBearing(edgdist, (component.BraceConnect.Brace.WebLong), component.BoltBrace.BoltSize, component.Material.Fu, component.BoltBrace.HoleType, false);
					}
					Fbrs = CommonCalculations.SpacingBearing(component.BoltBrace.SpacingLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.BoltBrace.HoleType, component.Material.Fu, false);
					if (component.BoltBrace.NumberOfRows == 2 && component.BraceConnect.Brace.BoltsAreStaggered)
					{
						edgebearingCap = 2 * (Fbre + FbreStag) * component.Shape.tf;
						Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength of end bolts = (Fbre + Fbre1) * t");
						Reporting.AddLine("= 2 * (" + Fbre + " + " + FbreStag + ") * " + component.Shape.tf + "= " + edgebearingCap + ConstUnit.Force);
					}
					else
					{
						edgebearingCap = 2 * component.BoltBrace.NumberOfRows * Fbre * component.Shape.tf;
						if (component.BoltBrace.NumberOfRows == 2)
						{
							Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength of end bolts = 4 * Fbre * t");
							Reporting.AddLine("= 4 * " + Fbre + " * " + component.Shape.tf + "= " + edgebearingCap + ConstUnit.Force);
						}
						else
						{
							Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength of end bolts = 2 * Fbre * t");
							Reporting.AddLine("= 2 * " + Fbre + " * " + component.Shape.tf + " = " + edgebearingCap + ConstUnit.Force);
						}
					}
					SpacingbearingCap = (Ntotal - 2 * component.BoltBrace.NumberOfRows) * Fbrs * component.Shape.tf;
					SpacingbearingCapComp = Ntotal * Fbrs * component.Shape.tf;
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength of remaining bolts = (N - Ne) * Fbrs * t");
					Reporting.AddLine("= (" + Ntotal + " - " + 2 * component.BoltBrace.NumberOfRows + ") * " + Fbrs + " * " + component.Shape.tf);
					Reporting.AddLine("= " + SpacingbearingCap + ConstUnit.Force);
					Reporting.AddLine("Total " + ConstString.DES_OR_ALLOWABLE + " Bearing Strength:");
					Reporting.AddLine("With Tensile Force:");
					capacity = edgebearingCap + SpacingbearingCap;
					if (capacity >= component.AxialTension)
						Reporting.AddLine(ConstString.PHI + " Rn = " + edgebearingCap + " + " + SpacingbearingCap + " =  " + capacity + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(ConstString.PHI + " Rn = " + edgebearingCap + " + " + SpacingbearingCap + " =  " + capacity + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
					
						Reporting.AddHeader("With Compressive Force:");
					Reporting.AddLine(ConstString.PHI + " Rn = N * Fbrs * tf = " + Ntotal + " * " + Fbrs + " * " + component.Shape.tf);
					if (SpacingbearingCapComp >= component.AxialTension)
						Reporting.AddLine("= " + SpacingbearingCapComp + " >= " + component.AxialCompression + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + SpacingbearingCapComp + " << " + component.AxialCompression + ConstUnit.Force + " (NG)");

					if (component.AxialTension > 0)
					{
						Reporting.AddHeader("Block Shear of Brace:");
						g0 = component.GageOnFlange; // ElProp(m).g1
						g1 = g0;
						g2 = Math.Min(3, ((int)Math.Floor(4 * ((component.Shape.bf - g1) / 2 - (component.BoltBrace.MinEdgeRolled + component.BoltBrace.Eincr)))) / 4);
						if (component.BoltBrace.NumberOfRows == 1)
						{
							Reporting.AddHeader("Bolt Gage on Brace (g0 = " + g0 + ConstUnit.Length);
							Agt = 2 * component.BoltBrace.EdgeDistTransvDir * t;
							Agv = 2 * (component.BoltBrace.EdgeDistLongDir + (component.BoltBrace.NumberOfBolts - 1) * component.BoltBrace.SpacingLongDir) * t;
							Ant = 2 * (component.BoltBrace.EdgeDistTransvDir - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) / 2) * t;
							Anv = 2 * (component.BoltBrace.EdgeDistLongDir + (component.BoltBrace.NumberOfBolts - 1) * component.BoltBrace.SpacingLongDir - (component.BoltBrace.NumberOfBolts - 0.5) * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH)) * t;
							AgtW = component.Shape.a - 2 * t * component.BoltBrace.EdgeDistTransvDir;
							AntW = AgtW - t * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH);
							if (Ant <= AntW)
							{
								Reporting.AddLine("Agt = 2 * et * t = 2 * " + component.BoltBrace.EdgeDistTransvDir + " * " + t + " = " + Agt + ConstUnit.Area);
								Reporting.AddLine("Ant = Agt - (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
								Reporting.AddLine("= " + Agt + " - (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t + "= " + Ant + ConstUnit.Area);
							}
							else
							{
								Reporting.AddHeader("Rupture through stem controls");
								Reporting.AddLine("Agt = A - 2 * et * t = " + component.Shape.a + " - 2 * " + component.BoltBrace.EdgeDistTransvDir + " * " + t + " = " + AgtW + ConstUnit.Area);
								Reporting.AddLine("Ant = Agt - (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
								Reporting.AddLine("= " + AgtW + " - (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ")* " + t + "= " + AntW + ConstUnit.Area);
							}
							Reporting.AddLine("Agv = 2 * (el + (Nl - 1) * sl) * t");
							Reporting.AddLine("= 2 * (" + component.BoltBrace.EdgeDistLongDir + " + (" + component.BoltBrace.NumberOfBolts + " - 1) * " + component.BoltBrace.SpacingLongDir + ") * " + t);
							Reporting.AddLine("= " + Agv + ConstUnit.Area);
							Reporting.AddLine("Anv = Agv - 2 * (Nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
							Reporting.AddLine("= " + Agv + " - 2*(" + component.BoltBrace.NumberOfBolts + " - 0.5) * (" + component.BraceConnect.Brace.WebLong + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t + "= " + Anv + ConstUnit.Area);
						}
						else if (component.BraceConnect.Brace.BoltsAreStaggered)
						{
							Reporting.AddHeader("Bolt Gage on Brace, (g2 - g1 - g2) = " + g2 + " - " + g1 + " - " + g2 + ConstUnit.Length);
							Agt = 2 * (component.BoltBrace.EdgeDistTransvDir + g2) * t;
							Ant1 = Agt - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
							Ant2 = Agt - 3 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t + 2 * (Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2)) * t;
							Ant = Math.Min(Ant1, Ant2);
							Reporting.AddLine("Agt = 2 * (eat + g2) * t");
							Reporting.AddLine("= 2 * (" + component.BoltBrace.EdgeDistTransvDir + " + " + g2 + ") * " + t + "= " + Agt + ConstUnit.Area);

							Reporting.AddLine("Ant1 = Agt - (dh + " + ConstNum.SIXTEENTH_INCH + ")*t");
							Reporting.AddLine("= " + Agt + " - (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t + "= " + Ant1 + ConstUnit.Area);

							Reporting.AddLine("Ant2 = Agt - 3 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t + 2 * ((sl / 2)² / (4 * g2)) * t");
							Reporting.AddLine("= " + Agt + " - 3 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t + " + 2 * ((" + component.BoltBrace.SpacingLongDir + " / 2)² / (4 * " + g2 + "))" + t + "= " + Ant2 + ConstUnit.Area);

							Reporting.AddLine("Ant = Min(Ant1, Ant2) = " + Ant + ConstUnit.Area);
							if (Ntotal / 2 % 2 == 0)
							{
								Agv = 2 * (component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir / 2 + (component.BoltBrace.NumberOfBolts / 2 - 1) * component.BoltBrace.SpacingLongDir) * t;
								Anv = Agv - 2 * (component.BoltBrace.NumberOfBolts / 2.0 - 0.5) * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH) * t;
								Reporting.AddLine("Agv = 2 * (el + s / 2 + (Nl - 1) * s) * t");
								Reporting.AddLine("= 2 * (" + component.BoltBrace.EdgeDistLongDir + " + " + component.BoltBrace.SpacingLongDir + " / 2 + (" + component.BoltBrace.NumberOfBolts / 2 + " - 1) * " + component.BoltBrace.SpacingLongDir + ") * " + t);
								Reporting.AddLine("= " + Agv + ConstUnit.Area);
								Reporting.AddLine("Anv = Agv - 2 * (N1 - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
								Reporting.AddLine("= " + Agv + " - 2*(" + component.BoltBrace.NumberOfBolts / 2 + " - 0.5) * (" + component.BraceConnect.Brace.WebLong + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t + "= " + Anv + ConstUnit.Area);
							}
							else
							{
								Agv = 2 * (component.BoltBrace.EdgeDistLongDir + ((component.BoltBrace.NumberOfBolts + 1) / 2 - 1) * component.BoltBrace.SpacingLongDir) * t;
								Anv = Agv - 2 * ((component.BoltBrace.NumberOfBolts + 1) / 2.0 - 0.5) * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH) * t;
								Reporting.AddLine("Agv = 2 * (el + (N1 - 1) * sl) * t");
								Reporting.AddLine("= 2 * (" + component.BoltBrace.EdgeDistLongDir + " + (" + (component.BoltBrace.NumberOfBolts + 1) / 2 + " - 1) * " + component.BoltBrace.SpacingLongDir + " * " + t);
								Reporting.AddLine("= " + Agv + ConstUnit.Area);
								Reporting.AddLine("Anv = Agv - 2 * (Nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
								Reporting.AddLine("= " + Agv + " - 2 * (" + ((component.BoltBrace.NumberOfBolts + 1) / 2 + " - 0.5) * (" + component.BraceConnect.Brace.WebLong + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t) + "= " + Anv + ConstUnit.Area);
							}
						}
						else
						{
							Reporting.AddHeader("Bolt Gage on Brace, (g2 - g1 - g2) = " + g2 + " - " + g1 + " - " + g2 + ConstUnit.Length);
							if (Ntotal / 2 % 2 == 0)
							{
								Agt = 2 * (component.BoltBrace.EdgeDistTransvDir + g2) * t;
								Ant = Agt - 3 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
								Reporting.AddLine("Agt = 2 * (et + g2) * t");
								Reporting.AddLine("= 2 * (" + component.BoltBrace.EdgeDistTransvDir + " + " + g2 + ") * " + t);
								Reporting.AddLine("= " + Agt + ConstUnit.Area);
								Reporting.AddLine("Ant = Agt - 3 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
								Reporting.AddLine("= " + Agt + " - 3 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ")*" + t + "= " + Ant + ConstUnit.Area);
								
								Agv = 2 * (component.BoltBrace.EdgeDistLongDir + (component.BoltBrace.NumberOfBolts / 2 - 1) * component.BoltBrace.SpacingLongDir) * t;
								Anv = Agv - 2 * (component.BoltBrace.NumberOfBolts / 2.0 - 0.5) * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH) * t;
								Reporting.AddLine("Agv = 2 * (el + (Nl - 1) * sl) * t");
								Reporting.AddLine("= 2*(" + component.BoltBrace.EdgeDistLongDir + " + (" + ((component.BoltBrace.NumberOfBolts / 2) + " - 1) * " + component.BoltBrace.SpacingLongDir + ") * " + t));
								Reporting.AddLine("= " + Agv + ConstUnit.Area);
								Reporting.AddLine("Anv = Agv - 2 * (N1 - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
								Reporting.AddLine("Anv = Agv - 2 * (" + (component.BoltBrace.NumberOfBolts / 2) + " - 0.5) * (" + component.BraceConnect.Brace.WebLong + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t + "= " + Anv + ConstUnit.Area);
							}
							else
							{
								Agt = 2 * (component.BoltBrace.EdgeDistTransvDir + g2) * t;
								Ant1 = Agt - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
								Ant2 = Agt + 2 * (Math.Pow(component.BoltBrace.SpacingLongDir, 2) / (4 * g2) - 1.5 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH)) * t;
								Ant = Math.Min(Ant1, Ant2);
								Reporting.AddLine("Agt = 2 * (et + g2) * t");
								Reporting.AddLine("= 2*(" + component.BoltBrace.EdgeDistTransvDir + " + " + g2 + ") * " + t + "= " + Agt + ConstUnit.Area);

								Reporting.AddLine("Ant1 = Agt - (dh + " + ConstNum.SIXTEENTH_INCH + ")*t");
								Reporting.AddLine("= " + Agt + " - (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ")*" + t + "= " + Ant1 + ConstUnit.Area);

								Reporting.AddLine("Ant2 = Agt + 2 * (sl² / (4 * g2) - 1.5 * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
								Reporting.AddLine("= " + Agt + " + 2 * (" + component.BoltBrace.SpacingLongDir + "² /(4 * " + g2 + ") - 1.5 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + t + "= " + Ant2 + ConstUnit.Area);

								Reporting.AddLine("Ant = Min(Ant1, Ant2) = " + Ant + ConstUnit.Area);
								Agv = 2 * (component.BoltBrace.EdgeDistLongDir + ((component.BoltBrace.NumberOfBolts + 1) / 2 - 1) * component.BoltBrace.SpacingLongDir) * t;
								Anv = Agv - 2 * ((component.BoltBrace.NumberOfBolts + 1) / 2.0 - 0.5) * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH) * t;
								Reporting.AddLine("Agv = 2 * (el + (Nl - 1) * sl) * t");
								Reporting.AddLine("= 2 * (" + component.BoltBrace.EdgeDistLongDir + " + (" + ((component.BoltBrace.NumberOfBolts + 1) / 2) + " - 1) * " + component.BoltBrace.SpacingLongDir + ") * " + t);
								Reporting.AddLine("= " + Agv + ConstUnit.Area);
								Reporting.AddLine("Anv = Agv - 2 * (Nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
								Reporting.AddLine("= " + Agv + " - 2 * (" + ((component.BoltBrace.NumberOfBolts + 1) / 2) + " - 0.5) * (" + component.BraceConnect.Brace.WebLong + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t + "= " + Anv + ConstUnit.Area);
							}
						}

						FiRn = SmallMethodsDesign.BlockShearPrint(Agv, Anv, Agt, Ant, component.Material.Fy, component.Material.Fu, true);
						if (FiRn >= component.AxialTension)
							Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
					}
					Reporting.AddHeader("Brace Check " + CommonDataStatic.CommonLists.CompleteMemberList[MiscMethods.ComponentMinusOne(component.MemberType)]);
					FiRn = ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy * component.Shape.a;
					Reporting.AddHeader("Tension Yielding:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_95 + " * " + r_y_String + Yildiz + "Fy * Ag = " + ConstString.FIOMEGA0_95 + " * " + r_y_Strength + Yildiz + component.Material.Fy + " * " + component.Shape.a);
					if (FiRn >= component.AxialTension)
						Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
					{
						Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
						Reporting.AddLine("Brace Reinforcement Required");
					}

					Reporting.AddHeader("Tension Rupture:");
					if (component.BoltBrace.NumberOfRows == 1)
					{
						An = component.Shape.a - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
						Reporting.AddLine("An = Ag - 2 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
						Reporting.AddLine("= " + component.Shape.a + " - 2 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t + " = " + An + ConstUnit.Area);
					}
					else if (component.BraceConnect.Brace.BoltsAreStaggered)
					{
						Reporting.AddLine("Bolt Gage on Brace, (g2 - g1 - g2) = " + g2 + " - " + g1 + " - " + g2 + ConstUnit.Length);
						An1 = component.Shape.a - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
						An2 = component.Shape.a + 2 * (-2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) + Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2)) * t;
						An = Math.Min(An1, An2);
						Reporting.AddLine("An1 = Ag - 2 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
						Reporting.AddLine("= " + component.Shape.a + " - 2*(" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t + "= " + An1 + ConstUnit.Area);

						Reporting.AddLine("An2 = Ag + 2 * (-2 * (dh + " + ConstNum.SIXTEENTH_INCH + ") + (s / 2)² / (4 * g2)) * t");
						Reporting.AddLine("= " + component.Shape.a + " + 2 * (-2 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") + (" + component.BoltBrace.SpacingLongDir + " / 2)² / (4 * " + g2 + ")) * " + t + "= " + An2 + ConstUnit.Area);
						Reporting.AddLine("An = Min(An1, An2) = " + An + ConstUnit.Area);
					}
					else
					{
						if (component.BoltBrace.NumberOfBolts % 2 == 0)
						{
							An = component.Shape.a - 4 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
							Reporting.AddLine("An = Ag - 4 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
							Reporting.AddLine("= " + component.Shape.a + " - 4*(" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t + " = " + An + ConstUnit.Area);
						}
						else
						{
							An1 = component.Shape.a - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
							An2 = component.Shape.a + 2 * (Math.Pow(component.BoltBrace.SpacingLongDir, 2) / (4 * g2) - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH)) * t;
							An = Math.Min(An1, An2);
							Reporting.AddHeader("Bolt Gage on Brace, (g2 - g1 - g2) = " + g2 + " - " + g1 + " - " + g2 + ConstUnit.Length);
							Reporting.AddLine("An1 = Ag - 2 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
							Reporting.AddLine("= " + component.Shape.a + " - 2*(" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t + " = " + An1 + ConstUnit.Area);

							Reporting.AddLine("An2 = Ag + 2 * (-2 * (dh + " + ConstNum.SIXTEENTH_INCH + ") + s² / (4 * g2)) * t");
							Reporting.AddLine("= " + component.Shape.a + " + 2*(-2 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") + " + component.BoltBrace.SpacingLongDir + "² /(4 * " + g2 + ")) * " + t + "= " + An2 + ConstUnit.Area);

							Reporting.AddLine("An = Min(An1, An2) = " + An + ConstUnit.Area);
						}
					}
					L = -component.EndSetback - component.BoltBrace.EdgeDistLongDir - component.BraceConnect.Gusset.EdgeDistance;
					U = 1 - x_ / L;
					Reporting.AddHeader("Shear Lag Factor (U) = 1 - x / L = 1 - " + x_ + " / " + L + " = " + U);
					FiRn = ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu * U * An;
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * " + r_t_String + Yildiz + "Fu * U * An");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + r_t_Strength + Yildiz + component.Material.Fu + " * " + U + " * " + An);
					if (FiRn >= component.AxialTension)
						Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
					{
						Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
						Reporting.AddLine("Brace Reinforcement Required");
					}
					break;
				case EConnectionStyle.Welded:
					weldforcapacity = Math.Min(component.BraceConnect.Brace.BraceWeld.Weld1sz, usefulweldsizeforbrace);
					Reporting.AddHeader(CommonDataStatic.CommonLists.CompleteMemberList[component.MemberType] + " to Gusset Connection");
					Reporting.AddLine("Brace Force (Tension) = " + component.AxialTension + ConstUnit.Force);
					Reporting.AddLine("Brace Force (Compression) = " + component.AxialCompression + ConstUnit.Force);
					Reporting.AddLine("Brace to Gusset Weld Size = " + CommonCalculations.WeldSize(component.BraceConnect.Brace.BraceWeld.Weld1sz) + ConstUnit.Length);
					if (weldforcapacity < component.BraceConnect.Brace.BraceWeld.Weld1sz)
						Reporting.AddHeader(" (Use " + weldforcapacity + ConstUnit.Length + " for capacity calculation)");

					Reporting.AddHeader("Brace to Gusset Weld Length = 2 X " + component.BraceConnect.Brace.BraceWeld.Weld1L + ConstUnit.Length);
					minweldsize = CommonCalculations.MinimumWeld(component.Shape.tf, component.BraceConnect.Gusset.Thickness);
					if (component.BraceConnect.Brace.BraceWeld.Weld1sz >= minweldsize)
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(component.BraceConnect.Brace.BraceWeld.Weld1sz) + " >= Minimum Weld Size = " + CommonCalculations.WeldSize(((int) minweldsize)) + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(component.BraceConnect.Brace.BraceWeld.Weld1sz) + " << Minimum Weld Size = " + CommonCalculations.WeldSize(((int) minweldsize)) + ConstUnit.Length + " (NG)");

					betaN = SmallMethodsDesign.WeldBetaFactor(component.BraceConnect.Brace.BraceWeld.Weld1L, component.BraceConnect.Brace.BraceWeld.Weld1sz);
					WeldCapacity = betaN * 2 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * weldforcapacity * component.BraceConnect.Brace.BraceWeld.Weld1L;
					Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Weld Strength:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.BETAS + " * 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fexx * 0.707 * w * L");
					Reporting.AddLine("= " + betaN + " * 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * 0.707 * " + weldforcapacity + " * " + component.BraceConnect.Brace.BraceWeld.Weld1L);
					if (WeldCapacity >= component.BraceConnect.Brace.MaxForce)
						Reporting.AddLine("= " + WeldCapacity + " >= " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + WeldCapacity + " << " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

					Bracedevelopes = 2 * ConstNum.FIOMEGA1_0N * 0.6 * component.Material.Fy * component.Shape.tf * component.BraceConnect.Brace.BraceWeld.Weld1L;
					Reporting.AddHeader("Maximum Weld Force Brace Can Develop:");
					Reporting.AddLine("= 2 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * Fy * t * L");
					Reporting.AddLine("= 2 * " + ConstNum.FIOMEGA1_0N + " * 0.6*" + component.Material.Fy + " * " + component.Shape.tf + " * " + component.BraceConnect.Brace.BraceWeld.Weld1L);
					if (Bracedevelopes >= component.BraceConnect.Brace.MaxForce)
						Reporting.AddLine("= " + Bracedevelopes + " >= " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Bracedevelopes + " << " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Brace Check " + CommonDataStatic.CommonLists.CompleteMemberList[MiscMethods.ComponentMinusOne(component.MemberType)]);
					FiRn = ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy * component.Shape.a;
					Reporting.AddHeader("Tension Yielding:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_95 + " * " + r_y_String + Yildiz + "Fy*Ag = " + ConstString.FIOMEGA0_95 + " * " + r_y_Strength + Yildiz + component.Material.Fy + " * " + component.Shape.a);
					if (FiRn >= component.AxialTension)
						Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
					{
						Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
						Reporting.AddLine("Brace Reinforcement Required");
					}

					L = -component.EndSetback;
					U = 1 - x_ / L;
					Reporting.AddHeader("Shear Lag Factor (U) = 1 - x / L = 1 - " + x_ + " / " + L + " = " + U);
					Reporting.AddHeader("Tension Rupture:");
					FiRn = ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu * U * component.Shape.a;
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * " + r_t_String + Yildiz + "Fu * U * Ag");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + r_t_Strength + Yildiz + component.Material.Fu + " * " + U + " * " + component.Shape.a);
					if (FiRn >= component.AxialTension)
						Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
					{
						Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
						Reporting.AddLine("(Brace Reinforcement Required.)");
					}
					break;
			}
			if (excessForceY > 0)
				Reporting.AddLine("Force in excess of Yield Strength = " + excessForceY + ConstUnit.Force);
			if (excessForceR > 0)
				Reporting.AddLine("Force in excess of Rupture Strength = " + excessForceR + ConstUnit.Force);

			AgStiffener = Math.Max(AgAdditionalforYielding, AgAdditionalforRupture);
			excessforce = Math.Max(0, Math.Max(excessForceY, excessForceR));
			component.BraceStiffener.Width = component.Shape.d - component.Shape.kdet - ConstNum.ONEANDHALF_INCHES;
			component.BraceStiffener.Length = -component.EndSetback + weldlength;
			component.BraceStiffener.Area = AgStiffener;
			component.BraceStiffener.Force = excessforce;
			if (component.BraceStiffener.Area > 0)
			{
				Reporting.AddHeader("Required Reinforcement Area Using " + component.BraceConnect.Gusset.Material.Name + " Steel:");
				Reporting.AddLine("A_gross = " + component.BraceStiffener.Area + ConstUnit.Area);
				Reporting.AddLine("Use cover plates on web and/or flange to provide the required area.");
				Reporting.AddLine("The length of plates must be sufficient to develop the excess force for");
				Reporting.AddLine("yielding and rupture with welds or bolts considering gross and effective net areas.");
			}
		}
	}
}