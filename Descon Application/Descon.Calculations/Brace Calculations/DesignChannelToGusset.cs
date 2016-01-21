using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class DesignChannelToGusset
	{
		internal static void CalcDesignChannelToGusset(EMemberType memberType)
		{
			double Bracedevelopes = 0;
			double Gussetdevelopes = 0;
			double WeldCapacity = 0;
			double weldforcapacity = 0;
			int nl = 0;
			double SpacingbearingCapComp = 0;
			double SpacingbearingCap = 0;
			double edgebearingCap = 0;
			double capacity = 0;
			double smin1 = 0;
			double sminG = 0;
			double sminB = 0;
			double edgemin = 0;
			double Smin = 0;
			double SminGusset = 0;
			double SminBrace = 0;
			double excessforce = 0;
			double AgStiffener = 0;
			double AgAdditionalforRupture = 0;
			double An_effective = 0;
			double excessForceR = 0;
			double U = 0;
			double L = 0;
			double An = 0;
			double x_ = 0;
			double tf = 0;
			double a = 0;
			double b = 0;
			double excessForceY = 0;
			double AgAdditionalforYielding = 0;
			double AgrossYielding = 0;
			double FiRn = 0;
			double weldlength = 0;
			double estimatedweldsize = 0;
			double usefulweldsizeforbrace = 0;
			double minweldsize = 0;
			double WeldArea = 0;
			int N_RequiredForBlockShear = 0;
			double Rn = 0;
			double Anv = 0;
			double Agv = 0;
			int Nrow = 0;
			double Ant = 0;
			double Agt = 0;
			double N_RequiredBComp = 0;
			double N_RequiredB = 0;
			double Fbrs = 0;
			double Fbre = 0;
			double np = 0;
			double N_RequiredS = 0;
			double Numrows = 0;
			double nwmax = 0;
			double d = 0;
			double t = 0;
			string r_y_String = "";
			string r_t_String = "";
			string Yildiz = "";
			double r_y_Strength = 0;
			double r_t_Strength = 0;
			double r_y = 0;
			double r_t = 0;
			int nChannel;
			double betaN;

			var component = CommonDataStatic.DetailDataDict[memberType];

			SmallMethodsDesign.ExpectedStrengthCoefficient(component.MemberType, ref r_t, ref r_y, ref r_t_Strength, ref r_y_Strength, ref Yildiz, ref r_t_String, ref r_y_String);

			nChannel = component.ShapeType == EShapeType.SingleChannel ? 1 : 2;

			// Overlapping HSS
			component.BraceConnect.ClawAngles.DoNotConnectFlanges = true;
			component.BraceConnect.SplicePlates.DoNotConnectWeb = true;

			if (component.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
			{
				t = component.Shape.tw * nChannel;
				d = component.Shape.t - 2 * (component.BoltBrace.BoltSize + 0.5);
				nwmax = Math.Floor(d / component.BoltBrace.SpacingTransvDir) + 1;
				N_RequiredS = ((component.BraceConnect.Brace.MaxForce / (component.BoltBrace.BoltStrength * nChannel))); // for bolt shear
				component.BoltBrace.NumberOfRows = (int) nwmax;
				if (component.AxialTension > 0)
					np = SmallMethodsDesign.NBRequiredForGussetBlockShear(memberType, 0, N_RequiredS);
				else
					np = 0;

				if (N_RequiredS < np)
					N_RequiredS = np;
				nwmax = Math.Floor(d / component.BoltBrace.SpacingTransvDir) + 1;
				component.BoltBrace.NumberOfRows = (int) nwmax;
				Fbre = CommonCalculations.EdgeBearing(component.BoltBrace.EdgeDistLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.Material.Fu, component.BoltBrace.HoleType, false);
				Fbrs = CommonCalculations.SpacingBearing(component.BoltBrace.SpacingLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.BoltBrace.HoleType, component.Material.Fu, false);
				N_RequiredB = (component.AxialTension / (component.Shape.tw * nChannel) - component.BoltBrace.NumberOfRows * Fbre) / Fbrs + component.BoltBrace.NumberOfRows; // for bearing on C web
				N_RequiredBComp = component.AxialCompression / (component.Shape.tw * nChannel) / Fbrs; // for bearing on C web
				N_RequiredB = Math.Max(N_RequiredB, N_RequiredBComp);
				Agt = (component.BoltBrace.NumberOfRows - 1) * component.BoltBrace.SpacingTransvDir * t;
				Ant = Agt - (component.BoltBrace.NumberOfRows - 1) * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
				if (component.AxialTension > 0)
				{
					Nrow = 1;
					do
					{
						Nrow = Nrow + 1;
						Agv = 2 * ((Nrow - 1) * component.BoltBrace.SpacingLongDir + component.BoltBrace.EdgeDistLongDir) * t;
						Anv = Agv - 2 * (Nrow - 0.5) * t * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH);

						Rn = 0.6 * Math.Min(component.Material.Fu * Anv, component.Material.Fy * Agv) + 1 * component.Material.Fu * Ant;
					} while (ConstNum.FIOMEGA0_75N * Rn < component.AxialTension);
					N_RequiredForBlockShear = Nrow * component.BoltBrace.NumberOfRows;
				}
				else
					N_RequiredForBlockShear = 0;

				component.BoltBrace.NumberOfBolts = (int)Math.Ceiling(Math.Max(N_RequiredS, Math.Max(N_RequiredB, N_RequiredForBlockShear)));
				if (component.BoltBrace.NumberOfBolts < 4)
					component.BoltBrace.NumberOfBolts = 4;
				Numrows = (int)Math.Ceiling((double)component.BoltBrace.NumberOfBolts / component.BoltBrace.NumberOfRows);
				if (Numrows < 2)
				{
					Numrows = 2;
					component.BoltBrace.NumberOfBolts = (int)(Numrows * component.BoltBrace.NumberOfRows);
					if (!component.BraceConnect.Gusset.ColumnSideSetback_User)
						component.BraceConnect.Gusset.ColumnSideSetback = -(component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir * (Numrows - 1) + component.BraceConnect.Gusset.EdgeDistance);
					component.BraceConnect.Brace.BraceWeld.Weld1sz = 0;
					component.BraceConnect.Brace.BraceWeld.Weld1L = -component.BraceConnect.Gusset.ColumnSideSetback;
				}
				else // welded
				{
					component.PFlange = component.BraceConnect.Brace.MaxForce / (2 * nChannel);
					WeldArea = component.PFlange / (0.7072 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					minweldsize = CommonCalculations.MinimumWeld(component.Shape.tw, component.BraceConnect.Gusset.Thickness);
					usefulweldsizeforbrace = 1.414 * component.Shape.tw * component.Material.Fu / CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					estimatedweldsize = Math.Max(minweldsize, usefulweldsizeforbrace / 2);

					estimatedweldsize = Math.Min(usefulweldsizeforbrace, Math.Pow(WeldArea, 0.5) / 10);
					if (component.BraceConnect.Brace.BraceWeld.Weld1sz < estimatedweldsize)
						component.BraceConnect.Brace.BraceWeld.Weld1sz = estimatedweldsize;
					weldlength = WeldArea / Math.Min(component.BraceConnect.Brace.BraceWeld.Weld1sz, usefulweldsizeforbrace);
					if (component.Shape.d > weldlength)
						weldlength = component.Shape.d;
					component.BraceConnect.Brace.BraceWeld.Weld1L = weldlength;
					if (!component.BraceConnect.Gusset.ColumnSideSetback_User)
						component.BraceConnect.Gusset.ColumnSideSetback = ((int)Math.Ceiling(component.BraceConnect.Brace.BraceWeld.Weld1L * 8)) / 8.0;
					component.BraceConnect.Brace.BraceWeld.Weld1L = -component.BraceConnect.Gusset.ColumnSideSetback;
					component.BraceConnect.Brace.BraceWeld.Weld1sz = NumberFun.ConvertFromFraction((int)(WeldArea / component.BraceConnect.Brace.BraceWeld.Weld1L));
					if (component.BraceConnect.Brace.BraceWeld.Weld1sz < minweldsize)
						component.BraceConnect.Brace.BraceWeld.Weld1sz = minweldsize;
				}

				//  Tension Yielding - Required Gross Area:
				FiRn = (ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy * component.Shape.a * nChannel);
				AgrossYielding = component.AxialTension / (ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy * nChannel);
				AgAdditionalforYielding = Math.Max(0, AgrossYielding - component.Shape.a) * (r_y * component.Material.Fy / component.BraceConnect.Gusset.Material.Fy);
				excessForceY = component.AxialTension - FiRn;

				// Rupture
				b = component.Shape.bf;
				t = component.Shape.tw;
				a = component.Shape.a;
				tf = component.Shape.tf;
				d = component.Shape.d;
				x_ = (Math.Pow(b, 2) * tf + Math.Pow(t, 2) * (d - 2 * tf) / 2) / a;
				switch (component.BraceToGussetWeldedOrBolted)
				{
					case EConnectionStyle.Bolted:
						An = component.Shape.a * nChannel - component.BoltBrace.NumberOfRows * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
						L = -component.BraceConnect.Gusset.ColumnSideSetback - component.BoltBrace.EdgeDistTransvDir - component.BoltBrace.EdgeDistLongDir;
						U = 1 - x_ / L;
						break;
					case EConnectionStyle.Welded:
						L = -component.BraceConnect.Gusset.ColumnSideSetback;
						U = 1 - x_ / L;
						An = component.Shape.a * nChannel;
						break;
				}

				FiRn = ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu * U * An;
				excessForceR = component.AxialTension - FiRn;
				An_effective = component.AxialTension / (ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu);
				AgAdditionalforRupture = Math.Max(0, An_effective / U + component.Shape.a - An - component.Shape.a) * (r_t * component.Material.Fu / component.BraceConnect.Gusset.Material.Fu);
				AgStiffener = Math.Max(AgAdditionalforYielding, AgAdditionalforRupture);
				excessforce = Math.Max(0, Math.Max(excessForceY, excessForceR));
				component.BraceStiffener.Area = AgStiffener;
				component.BraceStiffener.Force = excessforce;

				component.BraceConnect.Brace.BraceWeld.Weld2sz = 0;
				switch (component.BraceToGussetWeldedOrBolted)
				{
					case EConnectionStyle.Bolted:
						Reporting.AddHeader(CommonDataStatic.CommonLists.CompleteMemberList[component.MemberType] + " to Gusset Connection");
						Reporting.AddLine("Brace Member: " + component.Shape.Name + " - " + component.Material.Name);
						Reporting.AddLine("Brace Force (Tension): " + component.AxialTension + ConstUnit.Force);
						Reporting.AddLine("Brace Force (Compression: " + component.AxialCompression + ConstUnit.Force);
						Reporting.AddLine("Bolts: (" + component.BoltBrace.NumberOfBolts + ")" + component.BoltBrace.BoltName);
						Reporting.AddLine("Bolt Holes on Brace:" + component.BraceConnect.Brace.WebTrans + "\" Transv. X " + component.BraceConnect.Brace.WebLong + "\" Longit.");
						Reporting.AddLine("Bolt Holes on Gusset:" + component.BraceConnect.Gusset.HoleTransP + "\" Transv. X " + component.BraceConnect.Gusset.HoleLongP + "\" Longit.");
						if (component.BoltBrace.Slot0 && component.BoltBrace.Slot1)
						{
							SminBrace = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, component.Shape.tw * nChannel, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BraceConnect.Brace.WebLong);
							SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.BraceConnect.Gusset.Material.Fu, component.BraceConnect.Gusset.Thickness, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BraceConnect.Gusset.HoleLongP);
						}
						else if (component.BoltBrace.Slot0)
						{
							SminBrace = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, component.Shape.tw * nChannel, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BoltBrace.BoltSize + ConstNum.SIXTEENTH_INCH);
							SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.BraceConnect.Gusset.Material.Fu, component.BraceConnect.Gusset.Thickness, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BraceConnect.Gusset.HoleLongP);
						}
						else if (component.BoltBrace.Slot1)
						{
							SminBrace = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, component.Shape.tw * nChannel, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BraceConnect.Brace.WebLong);
							SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.BraceConnect.Gusset.Material.Fu, component.BraceConnect.Gusset.Thickness, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BoltBrace.BoltSize + ConstNum.SIXTEENTH_INCH);
						}
						else
						{
							SminBrace = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, component.Shape.tw * nChannel, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BoltBrace.BoltSize + ConstNum.SIXTEENTH_INCH);
							SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.BraceConnect.Gusset.Material.Fu, component.BraceConnect.Gusset.Thickness, (component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts), component.BoltBrace.BoltSize + ConstNum.SIXTEENTH_INCH);
						}
						Smin = Math.Min(SminBrace, SminGusset);
						if (component.BoltBrace.SpacingLongDir >= Smin)
							Reporting.AddLine("Longitudinal Bolt Spacing = " + component.BoltBrace.SpacingLongDir + " >= Min. Spacing = " + Smin + ConstUnit.Length + " (OK)");
						else
							Reporting.AddLine("Longitudinal Bolt Spacing = " + component.BoltBrace.SpacingLongDir + " << Min. Spacing = " + Smin + ConstUnit.Length + " (NG)");

						edgemin = Math.Max(component.BoltBrace.MinEdgeSheared + component.BoltBrace.Eincr, SmallMethodsDesign.MinClearDistForBearing((component.AxialTension / component.BoltBrace.NumberOfBolts), (component.Shape.tw * nChannel), component.Material.Fu, component.BoltBrace.HoleType) + component.BraceConnect.Brace.WebLong / 2);
						if (component.BoltBrace.EdgeDistLongDir >= edgemin)
							Reporting.AddLine("Longitudinal Edge Distance on Brace = " + component.BoltBrace.EdgeDistLongDir + " >= Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (OK)");
						else
							Reporting.AddLine("Longitudinal Edge Distance on Brace = " + component.BoltBrace.EdgeDistLongDir + " << Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (NG)");

						edgemin = Math.Max(component.BoltBrace.MinEdgeSheared + component.BraceConnect.Gusset.EincrP, SmallMethodsDesign.MinClearDistForBearing((component.AxialTension / component.BoltBrace.NumberOfBolts), component.BraceConnect.Gusset.Thickness, component.BraceConnect.Gusset.Material.Fu, component.BoltBrace.HoleType) + component.BraceConnect.Gusset.HoleLongP / 2);
						if (component.BoltBrace.EdgeDistGusset >= edgemin)
							Reporting.AddLine("Longitudinal Edge Distance on Gusset = " + component.BoltBrace.EdgeDistGusset + " >= Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (OK)");
						else
							Reporting.AddLine("Longitudinal Edge Distance on Gusset = " + component.BoltBrace.EdgeDistGusset + " << Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (NG)");

						Smin = ConstNum.EIGHT_THIRDS * component.BoltBrace.BoltSize;
						sminB = component.BraceConnect.Brace.WebTrans + component.BoltBrace.BoltSize;
						sminG = component.BraceConnect.Gusset.HoleTransP + component.BoltBrace.BoltSize;
						smin1 = Math.Max(sminB, sminG);
						Smin = Math.Max(Smin, smin1);
						if (component.BoltBrace.SpacingTransvDir >= Smin)
							Reporting.AddLine("Transverse Bolt Spacing = " + component.BoltBrace.SpacingTransvDir + " >= Min. Spacing = " + Smin + ConstUnit.Length + " (OK)");
						else
							Reporting.AddLine("Transverse Bolt Spacing = " + component.BoltBrace.SpacingTransvDir + " << Min. Spacing = " + Smin + ConstUnit.Length + " (NG)");

						Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Shear Strength of Bolts:");
						capacity = component.BoltBrace.NumberOfBolts * component.BoltBrace.BoltStrength * nChannel;
						if (capacity >= component.BraceConnect.Brace.MaxForce)
							Reporting.AddCapacityLine(ConstString.PHI + " Rn = N * Fv = " + component.BoltBrace.NumberOfBolts + " * " + component.BoltBrace.BoltStrength * nChannel + " = " + capacity + " >= " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)", component.BraceConnect.Brace.MaxForce / capacity, "Bolts", component.MemberType);
						else
							Reporting.AddCapacityLine(ConstString.PHI + " Rn = N * Fv = " + component.BoltBrace.NumberOfBolts + " * " + component.BoltBrace.BoltStrength * nChannel + " = " + capacity + " << " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)", component.BraceConnect.Brace.MaxForce / capacity, "Bolts", component.MemberType);

						Reporting.AddHeader("Bolt Bearing on Brace:");
						Fbre = CommonCalculations.EdgeBearing(component.BoltBrace.EdgeDistLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.Material.Fu, component.BoltBrace.HoleType, false);
						Fbrs = CommonCalculations.SpacingBearing(component.BoltBrace.SpacingLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.BoltBrace.HoleType, component.Material.Fu, false);
						edgebearingCap = (component.BoltBrace.NumberOfRows * Fbre * component.Shape.tw * nChannel);

						Reporting.AddHeader("With Tensile Force:");
						Reporting.AddLine("Bearing Capacity of end bolts = Ne * Fbre * t");
						Reporting.AddLine("= " + component.BoltBrace.NumberOfRows + " * " + Fbre + " * " + component.Shape.tw * nChannel);
						Reporting.AddLine("= " + edgebearingCap + ConstUnit.Force);
						SpacingbearingCap = ((component.BoltBrace.NumberOfBolts - component.BoltBrace.NumberOfRows) * Fbrs * component.Shape.tw * nChannel);
						SpacingbearingCapComp = (component.BoltBrace.NumberOfBolts * Fbrs * component.Shape.tw * nChannel);

						Reporting.AddHeader("Bearing Capacity of remaining bolts = (N - Ne) * Fbrs * t");
						Reporting.AddLine("= (" + component.BoltBrace.NumberOfBolts + " - " + component.BoltBrace.NumberOfRows + ") * " + Fbrs + " * " + component.Shape.tw * nChannel);
						Reporting.AddLine("= " + SpacingbearingCap + ConstUnit.Force);
						Reporting.AddLine("Total Bearing Capacity:");
						capacity = edgebearingCap + SpacingbearingCap;
						if (capacity >= component.AxialTension)
							Reporting.AddLine("= " + edgebearingCap + " + " + SpacingbearingCap + " =  " + capacity + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + edgebearingCap + " + " + SpacingbearingCap + " =  " + capacity + " << " + component.AxialTension + ConstUnit.Force + " (NG)");

						Reporting.AddHeader("With Compressive Force:");
						Reporting.AddLine("Total Bearing Capacity = N * Fbrs * t");
						Reporting.AddLine("= " + component.BoltBrace.NumberOfBolts + " * " + Fbrs + " * " + component.Shape.tw * nChannel);
						if (SpacingbearingCapComp >= component.AxialCompression)
							Reporting.AddLine("= " + SpacingbearingCapComp + " >= " + component.AxialCompression + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + SpacingbearingCapComp + " << " + component.AxialCompression + ConstUnit.Force + " (NG)");

						nl = component.BoltBrace.NumberOfBolts / component.BoltBrace.NumberOfRows;

						Reporting.AddHeader("Brace Check " + CommonDataStatic.CommonLists.CompleteMemberList[MiscMethods.ComponentMinusOne(component.MemberType)]);
						Reporting.AddHeader("Tension Yielding of the Brace:");
						FiRn = (ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy * component.Shape.a * nChannel);
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * " + r_y_String + Yildiz + "Fy * Ag = " + ConstString.FIOMEGA0_9 + " * " + r_y_Strength + Yildiz + component.Material.Fy + " * " + component.Shape.a * nChannel);
						if (FiRn >= component.AxialTension)
							Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
						else
						{
							Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
							Reporting.AddLine("Brace Reinforcement Required");
						}

						Reporting.AddHeader("Tension Rupture of the Brace:");
						Ant = component.Shape.a * nChannel - component.BoltBrace.NumberOfRows * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
						Reporting.AddLine("Net Area (An) = A - Nt * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
						Reporting.AddLine("= " + component.Shape.a * nChannel + " - " + component.BoltBrace.NumberOfRows + " * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t + " = " + Ant + ConstUnit.Area);
						Reporting.AddLine("Shear Lag Factor,U = 1 - x / L = 1 - " + x_ + " / " + L + " = " + U);
						FiRn = ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu * U * Ant;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * " + r_t_String + Yildiz + "Fu * U * An = " + ConstString.FIOMEGA0_75 + " * " + r_t_Strength + Yildiz + component.Material.Fu + " * " + U + " * " + Ant);
						if (FiRn >= component.AxialTension)
							Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
						else
						{
							Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
							Reporting.AddLine("Brace Reinforcement Required");
						}

						if (excessForceY > 0)
							Reporting.AddLine("Force in excess of Yield Strength = " + excessForceY + ConstUnit.Force);
						if (excessForceR > 0)
							Reporting.AddLine("Force in excess of Rupture Strength = " + excessForceR + ConstUnit.Force);

						AgStiffener = Math.Max(AgAdditionalforYielding, AgAdditionalforRupture);
						excessforce = Math.Max(0, Math.Max(excessForceY, excessForceR));
						component.BraceStiffener.Width = component.Shape.d - component.Shape.kdet - ConstNum.ONEANDHALF_INCHES;
						component.BraceStiffener.Length = -component.BraceConnect.Gusset.ColumnSideSetback + weldlength;
						component.BraceStiffener.Area = AgStiffener;
						component.BraceStiffener.Force = excessforce;
						if (component.BraceStiffener.Area > 0)
						{
							Reporting.AddHeader("Required Reinforcement Area Using " + component.BraceConnect.Gusset.Material.Name + " Steel:");
							Reporting.AddLine("A_gross = " + component.BraceStiffener.Area + ConstUnit.Area);
							Reporting.AddHeader("Use cover plates to provide the required area.");
							Reporting.AddLine("The length of plates must be sufficient to develop the excess force for yielding and rupture");
							Reporting.AddLine("with welds or bolts considering gross and effective net areas.");
							Reporting.AddHeader("Use 2PL Each with:");
							Reporting.AddLine("Thickness = " + component.BraceStiffener.Thickness + ConstUnit.Length);
							Reporting.AddLine("Width = " + component.BraceStiffener.Width + ConstUnit.Length);
							Reporting.AddLine("Length = " + 2 * weldlength + ConstUnit.Length);
							Reporting.AddLine("Plate length centered at end of slot");
						}

						if (component.AxialTension > 0)
						{
							Reporting.AddHeader("Block Shear of Brace:");
							Agt = (component.BoltBrace.NumberOfRows - 1) * component.BoltBrace.SpacingTransvDir * t;

							Reporting.AddHeader("Gross Area with Tension Resistance (Agt):");
							Reporting.AddLine("= (Nt - 1) * st * t");
							Reporting.AddLine("= (" + component.BoltBrace.NumberOfRows + " - 1) * " + component.BoltBrace.SpacingTransvDir + " * " + t);
							Reporting.AddLine("= " + Agt + ConstUnit.Area);
							
							Ant = Agt - (component.BoltBrace.NumberOfRows - 1) * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
							Reporting.AddHeader("Net Area with Tension Resistance (Ant):");
							Reporting.AddLine("= Agt - (Nt - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
							Reporting.AddLine("= " + Agt + " - (" + component.BoltBrace.NumberOfRows + " - 1) * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t);
							Reporting.AddLine("= " + Ant + ConstUnit.Area);
							
							Agv = 2 * ((nl - 1) * component.BoltBrace.SpacingLongDir + component.BoltBrace.EdgeDistLongDir) * t;
							Reporting.AddHeader("Gross Area with Shear Resistance (Agv):");
							Reporting.AddLine("= 2 * ((nl - 1) * sl + el) * t");
							Reporting.AddLine("= 2 * ((" + nl + " - 1) * " + component.BoltBrace.SpacingLongDir + " + " + component.BoltBrace.EdgeDistLongDir + ") * " + t);
							Reporting.AddLine("= " + Agv + ConstUnit.Area);
							
							Anv = Agv - 2 * (nl - 0.5) * t * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH);
							Reporting.AddHeader("Net Area with Shear Resistance (Anv):");
							Reporting.AddLine("= Agv - 2 * (nl - 0.5) * t * (dh + " + ConstNum.SIXTEENTH_INCH + ")");
							Reporting.AddLine("= " + Agv + " - 2 * (" + nl + " - 0.5) * " + t + " * (" + component.BraceConnect.Brace.WebLong + " + " + ConstNum.SIXTEENTH_INCH + ")");
							Reporting.AddLine("= " + Anv + ConstUnit.Area);

							FiRn = SmallMethodsDesign.BlockShearPrint(Agv, Anv, Agt, Ant, component.Material.Fy, component.Material.Fu, true);
							if (Ant >= 0.6 * Anv)
							{
								FiRn = 0.6 * component.Material.Fy * Agv + component.Material.Fu * Ant;
								Reporting.AddLine("Ant >= 0.6 * Anv");
								Reporting.AddLine(ConstString.PHI + " Rn = 0.6 * Fy * Agv + Fu * Ant");
								Reporting.AddLine("= 0.6 * " + component.Material.Fy + " * " + Agv + " + " + component.Material.Fu + " * " + Ant);
							}
							else
							{
								FiRn = 0.6 * component.Material.Fu * Anv + component.Material.Fy * Agt;
								Reporting.AddLine("Ant << 0.6 * Anv");
								Reporting.AddLine(ConstString.PHI + "Rn = 0.6 * Fu * Anv + Fy * Agt");
								Reporting.AddLine("= 0.6 * " + component.Material.Fu + " * " + Anv + " + " + component.Material.Fy + " * " + Agt);
							}
							if (FiRn >= component.AxialTension)
								Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
						}
						break;
					case EConnectionStyle.Welded:
						weldforcapacity = Math.Min(component.BraceConnect.Brace.BraceWeld.Weld1sz, usefulweldsizeforbrace);
						Reporting.AddHeader(CommonDataStatic.CommonLists.CompleteMemberList[component.MemberType] + " to Gusset Connection");
						Reporting.AddLine("Brace Force = " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force);
						Reporting.AddLine("Brace to Gusset Weld Size = " + CommonCalculations.WeldSize(((int) component.BraceConnect.Brace.BraceWeld.Weld1sz)) + ConstUnit.Length);
						if (weldforcapacity < component.BraceConnect.Brace.BraceWeld.Weld1sz)
							Reporting.AddLine("(Use " + weldforcapacity + ConstUnit.Length + " for capacity calculation)");
						Reporting.AddLine("Brace to Gusset Weld Length = 2 X " + component.BraceConnect.Brace.BraceWeld.Weld1L * nChannel + ConstUnit.Length);

						minweldsize = CommonCalculations.MinimumWeld(component.Shape.tw, component.BraceConnect.Gusset.Thickness);
						if (component.BraceConnect.Brace.BraceWeld.Weld1sz >= minweldsize)
							Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize((component.BraceConnect.Brace.BraceWeld.Weld1sz)) + " >= Minimum Weld Size = " + CommonCalculations.WeldSize(((int) minweldsize)) + ConstUnit.Length + " (OK)");
						else
							Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize((component.BraceConnect.Brace.BraceWeld.Weld1sz)) + " << Minimum Weld Size = " + CommonCalculations.WeldSize(((int) minweldsize)) + ConstUnit.Length + " (NG)");

						betaN = CommonCalculations.WeldBetaFactor(component.BraceConnect.Brace.BraceWeld.Weld1L, component.BraceConnect.Brace.BraceWeld.Weld1sz);
						WeldCapacity = (betaN * 2 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * weldforcapacity * component.BraceConnect.Brace.BraceWeld.Weld1L * nChannel);
						Reporting.AddHeader("Weld Capacity = 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fexx * 0.707 * w * L");
						Reporting.AddLine("= 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * 0.707 * " + weldforcapacity + " * " + component.BraceConnect.Brace.BraceWeld.Weld1L * nChannel);
						if (WeldCapacity >= component.BraceConnect.Brace.MaxForce)
							Reporting.AddLine("= " + WeldCapacity + " >= " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + WeldCapacity + " << " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

						Gussetdevelopes = 2 * 0.4 * component.BraceConnect.Gusset.Material.Fy * component.BraceConnect.Gusset.Thickness * component.BraceConnect.Brace.BraceWeld.Weld1L;
						Reporting.AddHeader("Maximum Weld Force Gusset Can Develop:");
						Reporting.AddLine("= 2 * 0.4 * Fy * t * L");
						Reporting.AddLine("= 2 * 0.4 * " + component.BraceConnect.Gusset.Material.Fy + " * " + component.BraceConnect.Gusset.Thickness + " * " + component.BraceConnect.Brace.BraceWeld.Weld1L);
						if (Gussetdevelopes >= component.FxP)
							Reporting.AddLine("= " + Gussetdevelopes + " >= " + component.FxP + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + Gussetdevelopes + " >> " + component.FxP + ConstUnit.Force + " (OK)");

						Bracedevelopes = 2 * ConstNum.FIOMEGA1_0N * 0.6 * component.Material.Fy * component.Shape.tw * nChannel * component.BraceConnect.Brace.BraceWeld.Weld1L;
						Reporting.AddHeader("Maximum Weld Force Brace Can Develop:");
						Reporting.AddLine("= 2 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * Fy * t * L");
						Reporting.AddLine("= 2 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + component.Material.Fy + " * " + component.Shape.tw * nChannel + " * " + component.BraceConnect.Brace.BraceWeld.Weld1L);
						if (Bracedevelopes >= component.BraceConnect.Brace.MaxForce)
							Reporting.AddLine("= " + Bracedevelopes + " >= " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + Bracedevelopes + " << " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

						Reporting.AddHeader("Brace Check " + CommonDataStatic.CommonLists.CompleteMemberList[MiscMethods.ComponentMinusOne(component.MemberType)]);
						Reporting.AddHeader("Tension Yielding of the Brace:");
						FiRn = (ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy * component.Shape.a * nChannel);
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * " + r_y_String + Yildiz + "Fy*Ag = " + ConstString.FIOMEGA0_9 + " * " + r_y_Strength + Yildiz + component.Material.Fy + " * " + component.Shape.a * nChannel);
						if (FiRn >= component.AxialTension)
							Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
						else
						{
							Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
							Reporting.AddLine("Brace Reinforcement Required");
						}

						Reporting.AddHeader("Tension Rupture of the Brace:");
						Reporting.AddHeader("Shear Lag Factor,U = 1-x/L = 1 - " + x_ + " / " + L + " = " + U);
						FiRn = (ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu * U * component.Shape.a * nChannel);
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * " + r_t_String + Yildiz + "Fu * U*A = " + ConstString.FIOMEGA0_75 + " * " + r_t_Strength + Yildiz + component.Material.Fu + " * " + U + " * " + component.Shape.a * nChannel);
						if (FiRn >= component.AxialTension)
							Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
						else
						{
							Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
							Reporting.AddLine("(Brace Reinforcement Required.)");
						}

						if (excessForceY > 0)
							Reporting.AddLine("Force in excess of Yield Strength = " + excessForceY + ConstUnit.Force);
						if (excessForceR > 0)
							Reporting.AddLine("Force in excess of Rupture Strength = " + excessForceR + ConstUnit.Force);

						AgStiffener = Math.Max(AgAdditionalforYielding, AgAdditionalforRupture);
						if (AgStiffener > 0)
						{
							component.BraceStiffener.Thickness = CommonCalculations.PlateThickness(AgStiffener / (component.Shape.d - component.Shape.kdet - ConstNum.ONEANDHALF_INCHES));
							component.BraceStiffener.WeldSize = -(-16 * (Math.Min(component.Material.Fu * component.Shape.tf / 2, component.BraceConnect.Gusset.Material.Fu * component.BraceStiffener.Thickness) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx))) / 16;
						}
						excessforce = Math.Max(0, Math.Max(excessForceY, excessForceR));
						weldlength = -(-4 * excessforce / (ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 4 * 0.707 * component.BraceStiffener.WeldSize)) / 4;
						component.BraceStiffener.Width = component.Shape.d - component.Shape.kdet - ConstNum.ONEANDHALF_INCHES;
						component.BraceStiffener.Length = -component.BraceConnect.Gusset.ColumnSideSetback + weldlength;
						component.BraceStiffener.Area = AgStiffener;
						component.BraceStiffener.Force = excessforce;
						if (component.BraceStiffener.Area > 0)
						{
							Reporting.AddHeader("Required Reinforcement Area Using " + component.BraceConnect.Gusset.Material.Name + " Steel:");
							Reporting.AddLine("A_gross = " + component.BraceStiffener.Area + ConstUnit.Area);
							Reporting.AddLine("Use cover plates to provide the required area.");
							Reporting.AddLine("The length of plates must be sufficient to develop the excess force for yielding and rupture");
							Reporting.AddLine("with welds or bolts considering gross and effective net areas.");

							Reporting.AddHeader("Use 2PL Each with:");
							Reporting.AddLine("Thickness = " + component.BraceStiffener.Thickness + ConstUnit.Length);
							Reporting.AddLine("Width = " + component.BraceStiffener.Width + ConstUnit.Length);
							Reporting.AddLine("Length = " + weldlength + ConstUnit.Length);
							Reporting.AddLine("(Plate length centered at end of slot)");
						}
						break;
				}
			}
		}
	}
}