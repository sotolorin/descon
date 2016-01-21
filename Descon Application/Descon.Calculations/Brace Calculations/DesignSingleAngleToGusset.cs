using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class DesignSingleAngleToGusset
	{
		internal static void CalcDesignSingleAngletoGusset(EMemberType memberType)
		{
			double tmpCaseArg = 0;
			double Bracedevelopes = 0;
			double Gussetdevelopes = 0;
			double WeldCapacity = 0;
			double Edgeweldcapacity = 0;
			double BetaN2 = 0;
			double Backweldcapacity = 0;
			double BetaN1 = 0;
			double Ant2 = 0;
			double Ant1 = 0;
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
			double SminGusset = 0;
			double SminBrace = 0;
			double Smin = 0;
			double excessforce = 0;
			double AgStiffener = 0;
			double AgAdditionalforRupture = 0;
			double An_effective = 0;
			double excessForceR = 0;
			double An2 = 0;
			double An1 = 0;
			double An = 0;
			double U = 0;
			double L = 0;
			double x_ = 0;
			double L2 = 0;
			double L1 = 0;
			double excessForceY = 0;
			double AgAdditionalforYielding = 0;
			double AgrossYielding = 0;
			double FiRn = 0;
			double weldlength = 0;
			double maxweldsize = 0;
			double minweldsize = 0;
			double BackWeldArea = 0;
			double EdgeWeldArea = 0;
			double pback = 0;
			double Pedge = 0;
			int N_RequiredForBlockShear = 0;
			double N_RequiredBComp = 0;
			double N_RequiredB = 0;
			double Fbrs = 0;
			double Fbre = 0;
			double np = 0;
			double N_RequiredS = 0;
			double g2 = 0;
			double g1 = 0;
			double g0 = 0;
			double AngB = 0;
			double AngX = 0;
			double thick = 0;
			double LongLeg = 0;
			double shortleg = 0;
			double cx = 0;
			double ar = 0;
			double t = 0;
			double b = 0;
			double d = 0;
			string r_y_String = string.Empty;
			string r_t_String = string.Empty;
			string Yildiz = string.Empty;
			double r_y_Strength = 0;
			double r_t_Strength = 0;
			double r_y = 0;
			double r_t = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];

			if (!component.IsActive)
				return;

			SmallMethodsDesign.ExpectedStrengthCoefficient(component.MemberType, ref r_t, ref r_y, ref r_t_Strength, ref r_y_Strength, ref Yildiz, ref r_t_String, ref r_y_String);

			component.BraceConnect.ClawAngles.DoNotConnectFlanges = true;
			component.BraceConnect.SplicePlates.DoNotConnectWeb = true;
			d = component.Shape.d;
			b = component.Shape.bf;
			t = component.Shape.tf;
			ar = (b + d - t) * t;
			component.Shape.a = ar;
			cx = (Math.Pow(d, 2) * t / 2 + (b - t) * Math.Pow(t, 2) / 2) / ar;

			shortleg = Math.Min(component.Shape.d, component.Shape.bf);
			LongLeg = Math.Max(component.Shape.d, component.Shape.bf);
			thick = component.Shape.tf;

			if (component.BraceToGussetWeldedOrBolted == EConnectionStyle.Welded)
			{
				if (component.WebOrientation == EWebOrientation.OutOfPlane)
				{
					AngX = (Math.Pow(shortleg, 2) / 2 * thick + (LongLeg - thick) * Math.Pow(thick, 2) / 2) / ((LongLeg + shortleg - thick) * thick);
					AngB = shortleg - AngX;
					component.AngleX = AngX;
				}
				else
				{
					AngX = (Math.Pow(LongLeg, 2) / 2 * thick + (shortleg - thick) * Math.Pow(thick, 2) / 2) / ((shortleg + LongLeg - thick) * thick);
					AngB = LongLeg - AngX;
					component.AngleY = AngX;
				}
			}
			else
			{
				g0 = CommonCalculations.AngleStandardGage(0, d);
				g1 = CommonCalculations.AngleStandardGage(1, d);
				g2 = CommonCalculations.AngleStandardGage(2, d);

				component.BraceConnect.Brace.BraceWeld.Weld1sz = 0;
				if (component.WebOrientation == EWebOrientation.OutOfPlane)
				{
					AngX = Math.Min(ConstNum.THREE_INCHES, CommonCalculations.AngleStandardGage(component.BoltBrace.NumberOfRows - 1, shortleg));
					AngB = shortleg - AngX;
					component.AngleX = AngX;
				}
				else
				{
					AngX = Math.Min(ConstNum.THREE_INCHES, CommonCalculations.AngleStandardGage(component.BoltBrace.NumberOfRows - 1, LongLeg));
					AngB = LongLeg - AngX;
					component.AngleY = AngX;
				}
			}
			if (component.OSLOnBeamSide)
			{
				component.AngleBeamSide = AngX;
				component.AngleColumnSide = AngB;
			}
			else
			{
				component.AngleBeamSide = AngB;
				component.AngleColumnSide = AngX;
			}

			g0 = CommonCalculations.AngleStandardGage(0, d);
			g1 = AngX;
			if (component.BoltBrace.NumberOfRows == 1)
				g2 = 0;
			else
				g2 = CommonCalculations.AngleStandardGage(2, d);

			if (component.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
			{
				N_RequiredS = (component.BraceConnect.Brace.MaxForce / component.BoltBrace.BoltStrength); // for bolt shear
				if (component.AxialTension > 0)
					np = SmallMethodsDesign.NBRequiredForGussetBlockShear(memberType, 0, N_RequiredS);
				else
					np = 0;
				if (N_RequiredS < np)
					N_RequiredS = ((int)np);

				if (N_RequiredS > 3 && d > ConstNum.FOUR_INCHES && (component.BoltBrace.NumberOfRows == 2))
				{
					component.BoltBrace.EdgeDistTransvDir = d - g1 - g2;
					Fbre = CommonCalculations.EdgeBearing(component.BoltBrace.EdgeDistLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.Material.Fu, component.BoltBrace.HoleType, false);
					Fbrs = CommonCalculations.SpacingBearing(component.BoltBrace.SpacingLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.BoltBrace.HoleType, component.Material.Fu, false);
					N_RequiredB = (component.AxialTension / component.Shape.tf - 2 * Fbre) / Fbrs + 2; // for bearing on angle
					N_RequiredBComp = component.AxialCompression / component.Shape.tf / Fbrs; // for bearing on angle
					N_RequiredB = Math.Max(N_RequiredB, N_RequiredBComp);
					if (component.AxialTension > 0)
						N_RequiredForBlockShear = SmallMethodsDesign.NBRequiredforBraceBlockShear(memberType, t, EShapeType.SingleAngle);
					else
						N_RequiredForBlockShear = 0;
					component.BoltBrace.NumberOfBolts = (int)Math.Ceiling(Math.Max(N_RequiredS, Math.Max(N_RequiredB, N_RequiredForBlockShear)));
					if (!component.EndSetback_User)
					{
						if (component.BoltBrace.NumberOfBolts % 2 == 0 && !component.BraceConnect.Brace.BoltsAreStaggered)
							component.EndSetback = -(component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir * (component.BoltBrace.NumberOfBolts / 2 - 1) + component.BraceConnect.Gusset.EdgeDistance);
						else
							component.EndSetback = -(component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir * ((component.BoltBrace.NumberOfBolts + 1) / 2 - 1) + component.BraceConnect.Gusset.EdgeDistance);
					}
				}
				else
				{
					component.BraceConnect.Brace.BoltsAreStaggered = false;
					component.BoltBrace.EdgeDistTransvDir = d - g1;
					Fbre = CommonCalculations.EdgeBearing(component.BoltBrace.EdgeDistLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.Material.Fu, component.BoltBrace.HoleType, false);
					Fbrs = CommonCalculations.SpacingBearing(component.BoltBrace.SpacingLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.BoltBrace.HoleType, component.Material.Fu, false);
					N_RequiredB = (component.AxialTension / component.Shape.tf - Fbre) / Fbrs + 1; // for bearing on angle
					N_RequiredBComp = component.AxialCompression / component.Shape.tf / Fbrs; // for bearing on angle
					N_RequiredB = Math.Max(N_RequiredB, N_RequiredBComp);
					if (component.AxialTension > 0)
						N_RequiredForBlockShear = SmallMethodsDesign.NBRequiredforBraceBlockShear(memberType, t, EShapeType.SingleAngle);
					else
						N_RequiredForBlockShear = 0;
					component.BoltBrace.NumberOfBolts = (int)Math.Ceiling(Math.Max(N_RequiredS, Math.Max(N_RequiredB, N_RequiredForBlockShear)));
					if (!component.EndSetback_User)
						component.EndSetback = -(component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir * (component.BoltBrace.NumberOfBolts - 1) + component.BraceConnect.Gusset.EdgeDistance);
				}
				component.BraceConnect.Brace.BraceWeld.Weld1L = -component.EndSetback;
				component.BraceConnect.Brace.BraceWeld.Weld1sz = 0;
			}
			else
			{
				// welded
				Pedge = cx / d * component.BraceConnect.Brace.MaxForce;
				pback = (d - cx) / d * component.BraceConnect.Brace.MaxForce;
				EdgeWeldArea = Pedge / (0.707 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				BackWeldArea = pback / (0.707 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				minweldsize = CommonCalculations.MinimumWeld(component.Shape.tf, component.BraceConnect.Gusset.Thickness);
				maxweldsize = Math.Max(component.Shape.tf * ConstNum.FIOMEGA1_0N * 0.6 * component.Material.Fy / (ConstNum.FIOMEGA0_75N * 0.6 * 0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx), component.BraceConnect.Gusset.Thickness * ConstNum.FIOMEGA1_0N * 0.6 * component.BraceConnect.Gusset.Material.Fy / (ConstNum.FIOMEGA0_75N * 0.6 * 0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx));
				tmpCaseArg = component.Shape.tf;
				if (tmpCaseArg < ConstNum.QUARTER_INCH)
				{
					if (maxweldsize > component.Shape.tf)
						maxweldsize = component.Shape.tf;
				}
				else
				{
					if (maxweldsize > component.Shape.tf - ConstNum.SIXTEENTH_INCH)
						maxweldsize = component.Shape.tf - ConstNum.SIXTEENTH_INCH;
				}

				if (minweldsize > maxweldsize)
					minweldsize = maxweldsize;

				weldlength = BackWeldArea / component.BraceConnect.Brace.BraceWeld.Weld1sz;
				if (Math.Max(component.Shape.d, component.Shape.bf) > weldlength)
					weldlength = Math.Max(component.Shape.d, component.Shape.bf);

				if (-component.EndSetback > weldlength)
					weldlength = -component.EndSetback;

				if (!component.EndSetback_User)
					component.EndSetback = NumberFun.Round(weldlength, 8);
				component.BraceConnect.Brace.BraceWeld.Weld1L = -component.EndSetback;

				component.BraceConnect.Brace.BraceWeld.Weld2sz = component.BraceConnect.Brace.BraceWeld.Weld1sz;
				component.BraceConnect.Brace.BraceWeld.Weld2L = component.BraceConnect.Brace.BraceWeld.Weld1L * EdgeWeldArea / BackWeldArea;
			}

			// Tension Yielding - Required Gross Area:
			FiRn = ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy * component.Shape.a;
			AgrossYielding = component.AxialTension / (ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy);
			AgAdditionalforYielding = Math.Max(0, AgrossYielding - component.Shape.a) * (r_y * component.Material.Fy / component.BraceConnect.Gusset.Material.Fy);
			excessForceY = component.AxialTension - FiRn;

			// Tension Rupture
			if (component.WebOrientation == EWebOrientation.OutOfPlane)
			{
				L1 = shortleg;
				L2 = LongLeg;
			}
			else
			{
				L2 = shortleg;
				L1 = LongLeg;
			}
			x_ = t / 2 * (t * L1 + Math.Pow(L2, 2) - Math.Pow(t, 2)) / component.Shape.a;
			switch (component.BraceToGussetWeldedOrBolted)
			{
				case EConnectionStyle.Welded:
					L = -component.EndSetback;
					U = 1 - x_ / L;
					An = component.Shape.a;
					FiRn = ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu * U * component.Shape.a;
					break;
				case EConnectionStyle.Bolted:
					if (component.BoltBrace.NumberOfRows == 1)
						An = component.Shape.a - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
					else if (component.BraceConnect.Brace.BoltsAreStaggered)
					{
						An1 = component.Shape.a - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
						An2 = component.Shape.a + (-2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) + Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2)) * t;
						An = Math.Min(An1, An2);
					}
					else
					{
						if (component.BoltBrace.NumberOfBolts % 2 == 0)
							An = component.Shape.a - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
						else
						{
							An1 = component.Shape.a - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
							An2 = component.Shape.a + (Math.Pow(component.BoltBrace.SpacingLongDir, 2) / (4 * g2) - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH)) * t;
							An = Math.Min(An1, An2);
						}
					}
					L = (-((int)Math.Floor((double)-component.BoltBrace.NumberOfBolts / component.BoltBrace.NumberOfRows)) - 1) * component.BoltBrace.SpacingLongDir;
					U = 1 - x_ / L;
					FiRn = ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu * U * An;
					break;
			}
			excessForceR = component.AxialTension - FiRn;
			An_effective = component.AxialTension / (ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu);
			AgAdditionalforRupture = Math.Max(0, (An_effective / U + component.Shape.a - An - component.Shape.a) * (r_t * component.Material.Fu / component.BraceConnect.Gusset.Material.Fu));

			AgStiffener = Math.Max(AgAdditionalforYielding, AgAdditionalforRupture);
			if (AgStiffener > 0)
			{
				component.BraceStiffener.Thickness = CommonCalculations.PlateThickness((int)(AgStiffener / (component.Shape.d - component.Shape.kdet - ConstNum.ONEANDHALF_INCHES)));
				component.BraceStiffener.WeldSize = -((int)Math.Floor(-16 * (Math.Min(component.Material.Fu * component.Shape.tf / 2, component.BraceConnect.Gusset.Material.Fu * component.BraceStiffener.Thickness) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx)))) / 16.0;
			}
			excessforce = Math.Max(0, Math.Max(excessForceY, excessForceR));
			component.BraceStiffener.Width = component.Shape.d - component.Shape.kdet - ConstNum.ONEANDHALF_INCHES;
			component.BraceStiffener.Length = -component.EndSetback + weldlength;
			component.BraceStiffener.Area = AgStiffener;
			component.BraceStiffener.Force = excessforce;

			switch (component.BraceToGussetWeldedOrBolted)
			{
				case EConnectionStyle.Bolted:
					Reporting.AddHeader(CommonDataStatic.CommonLists.CompleteMemberList[component.MemberType] + " to Gusset Connection");
					Reporting.AddLine("Brace Member: " + component.Shape.Name + " - " + component.Material.Name);
					Reporting.AddLine("Brace Force (Tension): " + component.AxialTension + ConstUnit.Force);
					Reporting.AddLine("Brace Force (Compression): " + component.AxialCompression + ConstUnit.Force);
					Reporting.AddLine("Bolts: (" + component.BoltBrace.NumberOfBolts + ") " + component.BoltBrace.BoltName);
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

					edgemin = Math.Max(component.BoltBrace.MinEdgeSheared + component.BoltBrace.EdgeDistLongDir, SmallMethodsDesign.MinClearDistForBearing((component.AxialTension / component.BoltBrace.NumberOfBolts), component.Shape.tf, component.Material.Fu, component.BoltBrace.HoleType) + component.BraceConnect.Brace.WebLong / 2);
					if (component.BoltBrace.EdgeDistLongDir >= edgemin)
						Reporting.AddLine("Longitudinal Edge Distance on Brace = " + component.BoltBrace.EdgeDistLongDir + " >= Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Longitudinal Edge Distance on Brace = " + component.BoltBrace.EdgeDistLongDir + " << Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (NG)");

					edgemin = Math.Max(component.BoltBrace.MinEdgeSheared + component.BraceConnect.Gusset.EincrP, SmallMethodsDesign.MinClearDistForBearing((component.AxialTension / component.BoltBrace.NumberOfBolts), component.BraceConnect.Gusset.Thickness, component.BraceConnect.Gusset.Material.Fu, component.BoltBrace.HoleType) + component.BraceConnect.Gusset.HoleLongP / 2);
					if (component.BoltBrace.EdgeDistGusset >= edgemin)
						Reporting.AddLine("Longitudinal Edge Distance on Gusset = " + component.BoltBrace.EdgeDistGusset + " >= Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Longitudinal Edge Distance on Gusset = " + component.BoltBrace.EdgeDistGusset + " << Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (NG)");

					if (component.BoltBrace.NumberOfRows == 2)
					{
						Smin = ConstNum.EIGHT_THIRDS * component.BoltBrace.BoltSize;
						spacing = CommonCalculations.AngleStandardGage(2, d);
						if (component.BraceConnect.Brace.BoltsAreStaggered)
						{
							if (component.BoltBrace.SpacingLongDir / 2 - component.BraceConnect.Brace.WebLong < component.BoltBrace.BoltSize)
							{
								sminB = component.BraceConnect.Brace.WebTrans + Math.Sqrt(Math.Pow(component.BoltBrace.BoltSize, 2) - Math.Pow(component.BoltBrace.SpacingLongDir / 2 - component.BraceConnect.Brace.WebLong, 2));
								sminB = Math.Sqrt(Math.Pow(sminB, 2) + Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2));
							}
							else
							{
								sminB = component.BraceConnect.Brace.WebTrans;
								sminB = Math.Sqrt(Math.Pow(sminB, 2) + Math.Pow(component.BoltBrace.BoltSize + component.BraceConnect.Brace.WebLong, 2));
							}
							if (component.BoltBrace.SpacingLongDir / 2 - component.BraceConnect.Gusset.HoleLongP < component.BoltBrace.BoltSize)
							{
								sminG = component.BraceConnect.Gusset.HoleTransP + Math.Sqrt(Math.Pow(component.BoltBrace.BoltSize, 2) - Math.Pow(component.BoltBrace.SpacingLongDir / 2 - component.BraceConnect.Gusset.HoleLongP, 2));
								sminG = Math.Sqrt(Math.Pow(sminG, 2) + Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2));
							}
							else
							{
								sminG = component.BraceConnect.Gusset.HoleTransP;
								sminG = Math.Sqrt(Math.Pow(sminG, 2) + Math.Pow(component.BoltBrace.BoltSize + component.BraceConnect.Gusset.HoleLongP, 2));
							}
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

					edgemin = component.BoltBrace.MinEdgeRolled + component.BoltBrace.EdgeDistTransvDir;
					if (component.BoltBrace.EdgeDistTransvDir >= edgemin)
						Reporting.AddLine("Transverse Edge Distance = " + component.BoltBrace.EdgeDistTransvDir + " >= Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Transverse Edge Distance = " + component.BoltBrace.EdgeDistTransvDir + " << Min. Edge Dist. = " + edgemin + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Bolt " + ConstString.DES_OR_ALLOWABLE + " Shear Strength:");
					capacity = component.BoltBrace.NumberOfBolts * component.BoltBrace.BoltStrength;
					if (capacity >= component.BraceConnect.Brace.MaxForce)
						Reporting.AddLine(ConstString.PHI + " Rn = N * Fv = " + component.BoltBrace.NumberOfBolts + " * " + component.BoltBrace.BoltStrength + " = " + capacity + " >= " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(ConstString.PHI + " Rn = N * Fv = " + component.BoltBrace.NumberOfBolts + " * " + component.BoltBrace.BoltStrength + " = " + capacity + " << " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Brace Check " + CommonDataStatic.CommonLists.CompleteMemberList[component.MemberType]);

					Reporting.AddHeader("Bolt Bearing on " + CommonDataStatic.CommonLists.CompleteMemberList[MiscMethods.ComponentMinusOne(component.MemberType)]);
					Fbre = CommonCalculations.EdgeBearing(component.BoltBrace.EdgeDistLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.Material.Fu, component.BoltBrace.HoleType, false);
					edgdist = component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir / 2;
					if (component.BoltBrace.NumberOfRows == 2 && component.BraceConnect.Brace.BoltsAreStaggered)
					{
						Reporting.AddHeader("For the second bolt line:");
						FbreStag = CommonCalculations.EdgeBearing(edgdist, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.Material.Fu, component.BoltBrace.HoleType, false);
					}
					Fbrs = CommonCalculations.SpacingBearing(component.BoltBrace.SpacingLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.BoltBrace.HoleType, component.Material.Fu, false);
					if (component.BoltBrace.NumberOfRows == 2 && component.BraceConnect.Brace.BoltsAreStaggered)
					{
						edgebearingCap = (Fbre + FbreStag) * component.Shape.tf;
						Reporting.AddHeader("Bearing Strength of end bolts = (Fbre + Fbre1) * t");
						Reporting.AddLine("= (" + Fbre + " + " + FbreStag + ") * " + component.BoltBrace.BoltSize + " * " + component.Shape.tf);
						Reporting.AddLine("= " + edgebearingCap + ConstUnit.Force);
					}
					else
					{
						edgebearingCap = component.BoltBrace.NumberOfRows * Fbre * component.Shape.tf;
						if (component.BoltBrace.NumberOfRows == 2)
						{
							Reporting.AddHeader("Bearing Strength of end bolts = 2 * Fbre * t");
							Reporting.AddLine("= 2 * " + Fbre + " * " + component.Shape.tf + " = " + edgebearingCap + ConstUnit.Force);
						}
						else
						{
							Reporting.AddHeader("Bearing Strength of end bolt = Fbre * t");
							Reporting.AddLine("= " + Fbre + " * " + component.Shape.tf + " = " + edgebearingCap + ConstUnit.Force);
						}
					}
					SpacingbearingCap = (component.BoltBrace.NumberOfBolts - component.BoltBrace.NumberOfRows) * Fbrs * component.Shape.tf;
					SpacingbearingCapComp = component.BoltBrace.NumberOfBolts * Fbrs * component.Shape.tf;
					Reporting.AddHeader("With Tensile Force:");
					Reporting.AddLine("Bearing Strength of remaining bolts = (N - Ne) * Fbrs * t");
					Reporting.AddLine("= (" + component.BoltBrace.NumberOfBolts + " - " + component.BoltBrace.NumberOfRows + ") * " + Fbrs + " * " + component.Shape.tf);
					Reporting.AddLine("= " + SpacingbearingCap + ConstUnit.Force);
					Reporting.AddLine("Total " + ConstString.DES_OR_ALLOWABLE + " Bearing Strength:");
					capacity = edgebearingCap + SpacingbearingCap;
					if (capacity >= component.AxialTension)
						Reporting.AddLine(ConstString.PHI + " Rn = " + edgebearingCap + " + " + SpacingbearingCap + " =  " + capacity + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(ConstString.PHI + " Rn = " + edgebearingCap + " + " + SpacingbearingCap + " =  " + capacity + " << " + component.AxialTension + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("With Compressive Force: ");
					Reporting.AddLine("Total " + ConstString.DES_OR_ALLOWABLE + " Bearing Strength = N * Fbrs * t");
					Reporting.AddLine(ConstString.PHI + " Rn = (" + component.BoltBrace.NumberOfBolts + " * " + Fbrs + " * " + component.Shape.tf);
					if (SpacingbearingCapComp >= component.AxialCompression)
						Reporting.AddLine("= " + SpacingbearingCapComp + " >= " + component.AxialCompression + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + SpacingbearingCapComp + " << " + component.AxialCompression + ConstUnit.Force + " (NG)");

					if (component.AxialTension > 0)
					{
						Reporting.AddHeader("Block Shear Rupture of Brace:");
						g0 = CommonCalculations.AngleStandardGage(0, d);
						g1 = CommonCalculations.AngleStandardGage(1, d);
						g2 = CommonCalculations.AngleStandardGage(2, d);
						if (component.BoltBrace.NumberOfRows == 1)
						{
							Reporting.AddHeader("Bolt Gage on angle (g) = " + g0 + ConstUnit.Length);
							Agt = component.BoltBrace.EdgeDistTransvDir * t;
							Agv = (component.BoltBrace.EdgeDistLongDir + (component.BoltBrace.NumberOfBolts - 1) * component.BoltBrace.SpacingLongDir) * t;
							Ant = (component.BoltBrace.EdgeDistTransvDir - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) / 2) * t;
							Anv = (component.BoltBrace.EdgeDistLongDir + (component.BoltBrace.NumberOfBolts - 1) * component.BoltBrace.SpacingLongDir - (component.BoltBrace.NumberOfBolts - 0.5) * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH)) * t;
							Reporting.AddLine("Agt = et * t = " + component.BoltBrace.EdgeDistTransvDir + " * " + t + " = " + Agt + ConstUnit.Length);
							Reporting.AddLine("Ant = Agt - 0.5 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
							Reporting.AddLine("= " + Agt + " - 0.5 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") *  " + t);
							Reporting.AddLine("= " + Ant + ConstUnit.Area);
							Reporting.AddLine("Agv = (el + (Nl - 1) * sl) * t");
							Reporting.AddLine("= (" + component.BoltBrace.EdgeDistLongDir + " + (" + component.BoltBrace.NumberOfBolts + " - 1) * " + component.BoltBrace.SpacingLongDir + ") * " + t);
							Reporting.AddLine("= " + Agv + ConstUnit.Area);
							Reporting.AddLine("Anv = Agv - (Nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
							Reporting.AddLine("= " + Agv + " - (" + component.BoltBrace.NumberOfBolts + " - 0.5) * (" + component.BraceConnect.Brace.WebLong + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t);
							Reporting.AddLine("= " + Anv + ConstUnit.Area);
						}
						else if (component.BraceConnect.Brace.BoltsAreStaggered)
						{
							Reporting.AddHeader("Bolt Gage on angle (g1) = " + g1 + ConstUnit.Length + ", g2 = " + g2 + ConstUnit.Length);
							Agt = (component.BoltBrace.EdgeDistTransvDir + g2) * t;
							Ant1 = Agt - 0.5 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
							Ant2 = Agt - 0.5 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t + (Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2) - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH)) * t;
							Ant = Math.Min(Ant1, Ant2);
							Reporting.AddLine("Agt =(eat + g2) * t");
							Reporting.AddLine("= (" + component.BoltBrace.EdgeDistTransvDir + " + " + g2 + ") * " + t);
							Reporting.AddLine("= " + Agt + ConstUnit.Area);

							Reporting.AddLine("Ant1 = Agt - 0.5 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
							Reporting.AddLine("= " + Agt + " - 0.5 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") *  " + t);
							Reporting.AddLine("= " + Ant1 + ConstUnit.Area);

							Reporting.AddLine("Ant2 = Agt - 0.5 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t + ((sl / 2)²  / (4 * g2) - (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
							Reporting.AddLine("= " + Agt + " - 0.5 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") *  " + t + " +((" + component.BoltBrace.SpacingLongDir + " / 2)² / (4 * " + g2 + ") - (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + t);
							Reporting.AddLine("= " + Ant2 + ConstUnit.Area);
							Reporting.AddLine("Ant = Min(Ant1, Ant2) = " + Ant + ConstUnit.Area);
							if (component.BoltBrace.NumberOfBolts % 2 == 0)
							{
								Agv = (component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir / 2 + (component.BoltBrace.NumberOfBolts / 2 - 1) * component.BoltBrace.SpacingLongDir) * t;
								Anv = Agv - (component.BoltBrace.NumberOfBolts / 2.0 - 0.5) * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH) * t;
								Reporting.AddLine("Agv = (el + s / 2 + (Nl - 1) * s) * t");
								Reporting.AddLine("= (" + component.BoltBrace.EdgeDistLongDir + " + " + component.BoltBrace.SpacingLongDir + "/2 + (" + component.BoltBrace.NumberOfBolts / 2 + " - 1) * " + component.BoltBrace.SpacingLongDir + ") * " + t);
								Reporting.AddLine("= " + Agv + ConstUnit.Area);
								Reporting.AddLine("Anv = Agv - (N1 - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
								Reporting.AddLine("= " + Agv + " - (" + component.BoltBrace.NumberOfBolts / 2 + " - 0.5) * (" + component.BraceConnect.Brace.WebLong + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t);
								Reporting.AddLine("= " + Anv + ConstUnit.Area);
							}
							else
							{
								Agv = (component.BoltBrace.EdgeDistLongDir + ((component.BoltBrace.NumberOfBolts + 1) / 2 - 1) * component.BoltBrace.SpacingLongDir) * t;
								Anv = Agv - ((component.BoltBrace.NumberOfBolts + 1) / 2.0 - 0.5) * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH) * t;
								Reporting.AddLine("Agv = (el + (N1 - 1) * sl) * t");
								Reporting.AddLine("= (" + component.BoltBrace.EdgeDistLongDir + " + (" + (component.BoltBrace.NumberOfBolts + 1) / 2 + " - 1) * " + component.BoltBrace.SpacingLongDir + " * " + t);
								Reporting.AddLine("= " + Agv + ConstUnit.Area);
								Reporting.AddLine("Anv = Agv - (Nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
								Reporting.AddLine("= " + Agv + " - (" + (component.BoltBrace.NumberOfBolts + 1) / 2 + " - 0.5) * (" + component.BraceConnect.Brace.WebLong + " + " + ConstNum.SIXTEENTH_INCH + ") *  " + t);
								Reporting.AddLine("= " + Anv + ConstUnit.Area);
							}
						}
						else
						{
							Reporting.AddHeader("Bolt Gage on angle (g1)= " + g1 + ConstUnit.Length + ", " + "g2 = " + g2 + ConstUnit.Length);
							if (component.BoltBrace.NumberOfBolts % 2 == 0)
							{
								Agt = (component.BoltBrace.EdgeDistTransvDir + g2) * t;
								Ant = Agt - 1.5 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
								Reporting.AddLine("Agt = (et + g2) * t");
								Reporting.AddLine("= (" + component.BoltBrace.EdgeDistTransvDir + " + " + g2 + ") * " + t);
								Reporting.AddLine("= " + Agt + ConstUnit.Area);
								Reporting.AddLine("Ant = Agt - 1.5 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
								Reporting.AddLine("= " + Agt + " - 1.5 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t);
								Reporting.AddLine("= " + Ant + ConstUnit.Area);
								Agv = (component.BoltBrace.EdgeDistLongDir + (component.BoltBrace.NumberOfBolts / 2 - 1) * component.BoltBrace.SpacingLongDir) * t;
								Anv = Agv - (component.BoltBrace.NumberOfBolts / 2.0 - 0.5) * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH) * t;
								Reporting.AddLine("Agv = (el + (Nl - 1) * sl) * t");
								Reporting.AddLine("= (" + component.BoltBrace.EdgeDistLongDir + " + (" + component.BoltBrace.NumberOfBolts / 2 + " - 1) * " + component.BoltBrace.SpacingLongDir + ") * " + t);
								Reporting.AddLine("= " + Agv + ConstUnit.Area);
								Reporting.AddLine("Anv = Agv - (N1 - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
								Reporting.AddLine("Anv = Agv - (" + component.BoltBrace.NumberOfBolts / 2 + " - 0.5) * (" + component.BraceConnect.Brace.WebLong + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t);
								Reporting.AddLine("= " + Anv + ConstUnit.Area);
							}
							else
							{
								Agt = (component.BoltBrace.EdgeDistTransvDir + g2) * t;
								Ant1 = Agt - 0.5 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
								Ant2 = Agt + (Math.Pow(component.BoltBrace.SpacingLongDir, 2) / (4 * g2) - 1.5 * component.BraceConnect.Brace.WebTrans) * t;
								Ant = Math.Min(Ant1, Ant2);
								Reporting.AddLine("Agt = (et + g2) * t");
								Reporting.AddLine("= (" + component.BoltBrace.EdgeDistTransvDir + " + " + g2 + ") * " + t);
								Reporting.AddLine("= " + Agt + ConstUnit.Area);

								Reporting.AddLine("Ant1 = Agt - 0.5 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
								Reporting.AddLine("= " + Agt + " - 0.5 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t);
								Reporting.AddLine("= " + Ant1 + ConstUnit.Area);

								Reporting.AddLine("Ant2 = Agt + (sl² / (4 * g2) - 1.5 * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
								Reporting.AddLine("= " + Agt + " + (" + component.BoltBrace.SpacingLongDir + "²  / (4*" + g2 + ") - 1.5 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + t);
								Reporting.AddLine("= " + Ant2 + ConstUnit.Area);

								Reporting.AddLine("Ant = Min(Ant1, Ant2) = " + Ant + ConstUnit.Area);
								Agv = (component.BoltBrace.EdgeDistLongDir + ((component.BoltBrace.NumberOfBolts + 1) / 2 - 1) * component.BoltBrace.SpacingLongDir) * t;
								Anv = Agv - ((component.BoltBrace.NumberOfBolts + 1) / 2.0 - 0.5) * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH) * t;
								Reporting.AddLine("Agv = (el + (Nl - 1) * sl) * t");
								Reporting.AddLine("= (" + component.BoltBrace.EdgeDistLongDir + " + (" + (component.BoltBrace.NumberOfBolts + 1) / 2 + " - 1) * " + component.BoltBrace.SpacingLongDir + ") * " + t);
								Reporting.AddLine("= " + Agv + ConstUnit.Area);

								Reporting.AddLine("Anv = Agv - (Nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
								Reporting.AddLine("= " + Agv + " - (" + (component.BoltBrace.NumberOfBolts + 1) / 2 + " - 0.5) * (" + component.BraceConnect.Brace.WebLong + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t);
								Reporting.AddLine("= " + Anv + ConstUnit.Area);
							}
						}

						FiRn = SmallMethodsDesign.BlockShearPrint(Agv, Anv, Agt, Ant, component.Material.Fy, component.Material.Fu, true);
						if (FiRn >= component.AxialTension)
							Reporting.AddLine("= " + ConstString.PHI + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + ConstString.PHI + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
					}
					Reporting.AddHeader("Brace Check:");
					FiRn = ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy * component.Shape.a;
					Reporting.AddHeader("Tension Yielding:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * " + r_y_String + " " + Yildiz + " Fy * Ag = " + ConstString.FIOMEGA0_9 + " * " + r_y_Strength + Yildiz + component.Material.Fy + " * " + component.Shape.a);
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
						An = component.Shape.a - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
						Reporting.AddLine("An = Ag - (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
						Reporting.AddLine("= " + component.Shape.a + " - (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t + " = " + An + ConstUnit.Area);
					}
					else if (component.BraceConnect.Brace.BoltsAreStaggered)
					{
						Reporting.AddHeader("Bolt Gage on angle (g1) = " + g1 + ConstUnit.Length + ", " + "g2 = " + g2 + ConstUnit.Length);
						An1 = component.Shape.a - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
						An2 = component.Shape.a + (-2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) + Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2)) * t;
						An = Math.Min(An1, An2);
						Reporting.AddLine("An1 = Ag - (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
						Reporting.AddLine("= " + component.Shape.a + " - (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t);
						Reporting.AddLine("= " + An1 + ConstUnit.Area);

						Reporting.AddLine("An2 = Ag + (-2* (dh + " + ConstNum.SIXTEENTH_INCH + ") + (s / 2)² / (4 * g2)) * t");
						Reporting.AddLine("= " + component.Shape.a + " + (-2 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") + (" + component.BoltBrace.SpacingLongDir + " / 2)²  / (4 * " + g2 + ")) * " + t);
						Reporting.AddLine("= " + An2 + ConstUnit.Area);
						Reporting.AddLine("An = Min(An1, An2) = " + An + ConstUnit.Area);
					}
					else
					{
						if (component.BoltBrace.NumberOfBolts % 2 == 0)
						{
							An = component.Shape.a - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
							Reporting.AddLine("An = Ag - 2* (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
							Reporting.AddLine("= " + component.Shape.a + " - 2* (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t + " = " + An + ConstUnit.Area);
						}
						else
						{
							An1 = component.Shape.a - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
							An2 = component.Shape.a + (Math.Pow(component.BoltBrace.SpacingLongDir, 2) / (4 * g2) - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH)) * t;
							An = Math.Min(An1, An2);
							Reporting.AddHeader("Bolt Gage on angle (g1) = " + g1 + ConstUnit.Length + ", " + "g2 = " + g2 + ConstUnit.Length);
							Reporting.AddLine("An1 = Ag - (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
							Reporting.AddLine("= " + component.Shape.a + " - (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + t + " = " + An1 + ConstUnit.Area);
							Reporting.AddLine("An2 = Ag + (-2 * (dh + " + ConstNum.SIXTEENTH_INCH + ") + s² / (4 * g2)) * t");
							Reporting.AddLine("= " + component.Shape.a + " + (-2 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") + " + component.BoltBrace.SpacingLongDir + "²  / (4 *" + g2 + ")) * " + t);
							Reporting.AddLine("= " + An2 + ConstUnit.Area);
							Reporting.AddLine("An = Min(An1, An2) = " + An + ConstUnit.Area);
						}
					}
					FiRn = ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu * U * An;
					Reporting.AddHeader("Shear Lag Factor (U) = 1 - x / L = 1 - " + x_ + " / " + L + " = " + U);
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
					Reporting.AddHeader("Brace to Gusset Connection");
					Reporting.AddLine(CommonDataStatic.CommonLists.CompleteMemberList[component.MemberType] + " to Gusset Connection");
					Reporting.AddLine("Brace Force = " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force);
					Reporting.AddLine("Brace to Gusset Weld Size = " + CommonCalculations.WeldSize(component.BraceConnect.Brace.BraceWeld.Weld1sz) + ConstUnit.Length);
					Reporting.AddLine("Brace to Gusset Weld Length Along Heel of Angle = " + component.BraceConnect.Brace.BraceWeld.Weld1L + ConstUnit.Length);
					Reporting.AddLine("Brace to Gusset Weld Length Along Toe of Angle = " + component.BraceConnect.Brace.BraceWeld.Weld2L + ConstUnit.Length);
					minweldsize = CommonCalculations.MinimumWeld(component.Shape.tf, component.BraceConnect.Gusset.Thickness);
					tmpCaseArg = component.Shape.tf;
					if (tmpCaseArg < ConstNum.QUARTER_INCH)
						maxweldsize = component.Shape.tf;
					else
						maxweldsize = component.Shape.tf - ConstNum.SIXTEENTH_INCH;
					if (minweldsize > maxweldsize)
						minweldsize = maxweldsize;
					if (component.BraceConnect.Brace.BraceWeld.Weld1sz >= minweldsize)
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(component.BraceConnect.Brace.BraceWeld.Weld1sz) + " >= Minimum Weld Size = " + CommonCalculations.WeldSize(minweldsize) + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(component.BraceConnect.Brace.BraceWeld.Weld1sz) + " << Minimum Weld Size = " + CommonCalculations.WeldSize(minweldsize) + ConstUnit.Length + " (NG)");
					if (component.BraceConnect.Brace.BraceWeld.Weld1sz <= maxweldsize)
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(component.BraceConnect.Brace.BraceWeld.Weld1sz) + " <= Maximum Weld Size = " + CommonCalculations.WeldSize(maxweldsize) + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(component.BraceConnect.Brace.BraceWeld.Weld1sz) + " >> Maximum Weld Size = " + CommonCalculations.WeldSize(maxweldsize) + ConstUnit.Length + " (NG)");

					BetaN1 = CommonCalculations.WeldBetaFactor(component.BraceConnect.Brace.BraceWeld.Weld1L, component.BraceConnect.Brace.BraceWeld.Weld1sz);
					Backweldcapacity = BetaN1 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * component.BraceConnect.Brace.BraceWeld.Weld1sz * component.BraceConnect.Brace.BraceWeld.Weld1L;
					BetaN2 = CommonCalculations.WeldBetaFactor(component.BraceConnect.Brace.BraceWeld.Weld2L, component.BraceConnect.Brace.BraceWeld.Weld1sz);
					Edgeweldcapacity = BetaN2 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * component.BraceConnect.Brace.BraceWeld.Weld2sz * component.BraceConnect.Brace.BraceWeld.Weld2L;

					Reporting.AddHeader("Heel Weld:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.BETAS + " * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fexx * 0.707 * w * L");
					Reporting.AddLine("= " + BetaN1 + " * " + ConstString.FIOMEGA0_75 + "*0.6*" + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * 0.707 * " + component.BraceConnect.Brace.BraceWeld.Weld1sz + " * " + component.BraceConnect.Brace.BraceWeld.Weld1L);
					Reporting.AddLine("= " + Backweldcapacity + ConstUnit.Force);

					Reporting.AddHeader("Toe Weld:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.BETAS + " * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fexx * 0.707 * w * L");
					Reporting.AddLine("= " + BetaN2 + " * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * 0.707 * " + component.BraceConnect.Brace.BraceWeld.Weld2sz + " * " + component.BraceConnect.Brace.BraceWeld.Weld2L);
					Reporting.AddLine("= " + Edgeweldcapacity + ConstUnit.Force);
					WeldCapacity = Backweldcapacity + Edgeweldcapacity;
					Reporting.AddHeader("Total Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
					if (WeldCapacity >= component.BraceConnect.Brace.MaxForce)
						Reporting.AddLine(ConstString.PHI + " Rn = " + WeldCapacity + " >= " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(ConstString.PHI + " Rn = " + WeldCapacity + " << " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

					Gussetdevelopes = ConstNum.FIOMEGA1_0N * 0.6 * component.BraceConnect.Gusset.Material.Fy * component.BraceConnect.Gusset.Thickness * (component.BraceConnect.Brace.BraceWeld.Weld1L + component.BraceConnect.Brace.BraceWeld.Weld2L);
					Reporting.AddHeader("Maximum Weld Force Gusset Can Develop:");
					Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + " * 0.6 * Fy * t * L");
					Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + component.BraceConnect.Gusset.Material.Fy + " * " + component.BraceConnect.Gusset.Thickness + " * (" + component.BraceConnect.Brace.BraceWeld.Weld1L + " + " + component.BraceConnect.Brace.BraceWeld.Weld2L + ")");
					if (Gussetdevelopes >= component.BraceConnect.Brace.MaxForce)
						Reporting.AddLine("= " + Gussetdevelopes + " >= " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Gussetdevelopes + " << " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

					Bracedevelopes = ConstNum.FIOMEGA1_0N * 0.6 * component.Material.Fy * component.Shape.tf * (component.BraceConnect.Brace.BraceWeld.Weld1L + component.BraceConnect.Brace.BraceWeld.Weld2L);
					Reporting.AddHeader("Maximum Weld Force Brace Can Develop:");
					Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + " * 0.6 * Fy * t * L");
					Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + component.Material.Fy + " * " + component.Shape.tf + " * (" + component.BraceConnect.Brace.BraceWeld.Weld1L + " + " + component.BraceConnect.Brace.BraceWeld.Weld2L + ")");
					if (Bracedevelopes >= component.BraceConnect.Brace.MaxForce)
						Reporting.AddLine("= " + Bracedevelopes + " >= " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Bracedevelopes + " << " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Brace Check " + CommonDataStatic.CommonLists.CompleteMemberList[MiscMethods.ComponentMinusOne(component.MemberType)]);
					FiRn = ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy * component.Shape.a;
					Reporting.AddHeader("Tension Yielding:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * " + r_y_String + Yildiz + "Fy * Ag = " + ConstString.FIOMEGA0_9 + " * " + r_y_Strength + Yildiz + component.Material.Fy + " * " + component.Shape.a);
					if (FiRn >= component.AxialTension)
						Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
					{
						Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
						Reporting.AddLine("Brace Reinforcement Required");
					}

					Reporting.AddHeader("Tension Rupture:");
					Reporting.AddHeader("Shear Lag Factor (U) = 1 - x / L = 1 - " + x_ + " / " + L + " = " + U);
					FiRn = ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu * U * component.Shape.a;
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * " + r_t_String + " " + Yildiz + " Fu * U * Ag");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + r_t_Strength + " " + Yildiz + " " + component.Material.Fu + " * " + U + " * " + component.Shape.a);
					if (FiRn >= component.AxialTension)
						Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
					{
						Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
						Reporting.AddLine("Brace Reinforcement Required");
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
				Reporting.AddLine("Use cover plates to provide the required area.");
				Reporting.AddLine("The length of plates must be sufficient to develop the excess force for yielding and rupture");
				Reporting.AddLine("with welds or bolts considering gross and effective net areas.");
			}
		}
	}
}