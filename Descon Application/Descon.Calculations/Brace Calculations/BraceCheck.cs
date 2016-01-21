using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class BraceCheck
	{
		internal static bool CalcBraceCheck(EMemberType memberType, ref bool BearingonFlangeOK, ref  bool BearingonWebOK, ref bool FlangeTearOutOk, ref bool WebTearOutOk, ref bool NetTensionOk, bool enableReporting)
		{
			double excessforce = 0;
			double AgStiffener = 0;
			double AgAdditionalforRupture = 0;
			double An_effective = 0;
			double excessForceR = 0;
			double excessForceY = 0;
			double AgAdditionalforYielding = 0;
			double AgrossYielding = 0;
			double FiPn = 0;
			double L = 0;
			double x_ = 0;
			double d = 0;
			double tf = 0;
			double a = 0;
			double t = 0;
			double b = 0;
			double Anet = 0;
			double NetTensionCap = 0;
			double NetTensionCap3 = 0;
			double NetTensionCap2 = 0;
			double NetTensionCap1 = 0;
			double Tca = 0;
			double Anet3 = 0;
			double Anet2 = 0;
			double Anet1 = 0;
			double g = 0;
			double S = 0;
			double U = 0;
			double AtnWeb = 0;
			double AtgWeb = 0;
			double AvnWeb = 0;
			double AvgWeb = 0;
			double FiRn = 0;
			int Ubn = 0;
			double AtnFlange = 0;
			double AtgFlange = 0;
			double AvnFlange = 0;
			double AvgFlange = 0;
			double HolesizePlus = 0;
			double anglegage = 0;
			double fsz = 0;
			bool BearingonWebOKComp = false;
			double FpsWebComp = 0;
			double FpsWeb = 0;
			double SpBearing = 0;
			double FpeWeb = 0;
			bool BearingonFlangeOKComp = false;
			double FpsFlangeComp = 0;
			double FpsFlange = 0;
			double SpBear = 0;
			double FpeFlange = 0;
			double EdgeBear = 0;
			string ryString = "";
			string rtString = "";
			string Yildiz = "";
			double ryStrength = 0;
			double rtStrength = 0;
			double materialRy = 0;
			double materialRt = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];

			if (component.EndSetback < 0)
				return true;

			SmallMethodsDesign.ExpectedStrengthCoefficient(memberType, ref materialRt, ref materialRy, ref rtStrength, ref ryStrength, ref Yildiz, ref rtString, ref ryString);

			if (enableReporting)
				Reporting.AddHeader("Brace Check " + CommonDataStatic.CommonLists.CompleteMemberList[MiscMethods.ComponentMinusOne(memberType)]);

			if (!component.BraceConnect.ClawAngles.DoNotConnectFlanges)
			{
				EdgeBear = CommonCalculations.EdgeBearing(component.BoltGusset.EdgeDistBrace, component.BraceConnect.Brace.FlangeLong, component.BoltGusset.BoltSize, component.Material.Fu, component.BoltGusset.HoleType, enableReporting);
				FpeFlange = 4 * component.Shape.tf * EdgeBear;

				if (enableReporting)
				{
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength of Flange Using Bolt Edge Distance, Fbe");
					Reporting.AddLine("= 4 * tf * Fbre");
					Reporting.AddLine("= 4 * " + component.Shape.tf + " * " + EdgeBear + " = " + FpeFlange + ConstUnit.Force);
				}
				SpBear = CommonCalculations.SpacingBearing(component.BoltGusset.SpacingLongDir, component.BraceConnect.Brace.FlangeLong, component.BoltGusset.BoltSize, component.BoltGusset.HoleType, component.Material.Fu, enableReporting);
				FpsFlange = 4 * (component.BoltGusset.NumberOfBolts - 1) * component.Shape.tf * SpBear;
				FpsFlangeComp = 4 * component.BoltGusset.NumberOfBolts * component.Shape.tf * SpBear;
				BearingonFlangeOK = FpeFlange + FpsFlange >= 2 * component.BraceConnect.Brace.FlangeForceTension;
				BearingonFlangeOKComp = FpsFlangeComp >= 2 * component.BraceConnect.Brace.FlangeForceCompression;
				if (enableReporting)
				{
					Reporting.AddHeader("With Tensile Force:");
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength of Flange Using Bolt Spacing (Fbs):");
					Reporting.AddLine("= 4 * (n - 1) * tf * Fbrs");
					Reporting.AddLine("= 4 * (" + component.BoltGusset.NumberOfBolts + " - 1) * " + component.Shape.tf + " * " + SpBear);
					Reporting.AddLine("= " + FpsFlange + ConstUnit.Force);

					Reporting.AddLine("Total " + ConstString.DES_OR_ALLOWABLE + " Bearing Strength of Flange (Fbr)");
					Reporting.AddLine("= " + FpeFlange + " + " + FpsFlange);
					if (BearingonFlangeOK)
						Reporting.AddLine("= " + (FpeFlange + FpsFlange) + ConstUnit.Force + " >= " + 2 * component.BraceConnect.Brace.FlangeForceTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + (FpeFlange + FpsFlange) + ConstUnit.Force + " << " + 2 * component.BraceConnect.Brace.FlangeForceTension + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("With Compressive Force:");
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength of Flange Using Bolt Spacing (Fbs):");
					Reporting.AddLine("= 4 * n * tf * Fbrs");
					Reporting.AddLine("= 4 * " + component.BoltGusset.NumberOfBolts + " * " + component.Shape.tf + " * " + SpBear);
					Reporting.AddLine("= " + FpsFlangeComp + ConstUnit.Force);

					Reporting.AddLine("Total " + ConstString.DES_OR_ALLOWABLE + " Bearing Strength of Flange (Fbe)");
					if (BearingonFlangeOKComp)
						Reporting.AddLine("= " + FpsFlangeComp + ConstUnit.Force + " >= " + 2 * component.BraceConnect.Brace.FlangeForceCompression + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + FpsFlangeComp + ConstUnit.Force + " << " + 2 * component.BraceConnect.Brace.FlangeForceCompression + ConstUnit.Force + " (NG)");
				}
			}

			if (!component.BraceConnect.SplicePlates.DoNotConnectWeb && component.BraceToGussetWeldedOrBolted != EConnectionStyle.Bolted)
			{
				EdgeBear = CommonCalculations.EdgeBearing(component.BraceConnect.SplicePlates.Bolt.EdgeDistBrace, (component.BraceConnect.Brace.WebLong), component.BraceConnect.SplicePlates.Bolt.BoltSize, component.Material.Fu, component.BraceConnect.SplicePlates.Bolt.HoleType, enableReporting);
				FpeWeb = component.BraceConnect.SplicePlates.Bolt.NumberOfLines * component.Shape.tw * EdgeBear;

				if (enableReporting)
				{
					Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Bearing Strength of Web Using Bolt Edge Distance (Wbe)");
					Reporting.AddLine("= Nw * tw * Wbre");
					Reporting.AddLine("= " + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " * " + component.Shape.tw + " * " + EdgeBear + " = " + FpeWeb + ConstUnit.Force);
				}

				SpBearing = CommonCalculations.SpacingBearing(component.BraceConnect.SplicePlates.Bolt.SpacingLongDir, component.BraceConnect.Brace.WebLong, component.BraceConnect.SplicePlates.Bolt.BoltSize, component.BraceConnect.SplicePlates.Bolt.HoleType, component.Material.Fu, enableReporting);
				FpsWeb = (component.BraceConnect.SplicePlates.Bolt.NumberOfBolts - component.BraceConnect.SplicePlates.Bolt.NumberOfLines) * component.Shape.tw * SpBearing;
				FpsWebComp = component.BraceConnect.SplicePlates.Bolt.NumberOfBolts * component.Shape.tw * SpBearing;
				BearingonWebOK = FpeWeb + FpsWeb >= component.BraceConnect.Brace.WebForceTension;
				BearingonWebOKComp = FpsWebComp >= component.BraceConnect.Brace.WebForceCompression;

				if (enableReporting)
				{
					Reporting.AddHeader("With Tensile Force:");
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength of Web Using Bolt Spacing (Wbs)");
					Reporting.AddLine("= (n - Nw) * tw * Wbrs");
					Reporting.AddLine("= (" + component.BraceConnect.SplicePlates.Bolt.NumberOfBolts + " - " + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + ") * " + component.Shape.tw + " * " + SpBearing);
					Reporting.AddLine("= " + FpsWeb + ConstUnit.Force);

					Reporting.AddLine("Total " + ConstString.DES_OR_ALLOWABLE + " Bearing Strength of Web (Wbr)");
					Reporting.AddLine("= " + FpeWeb + " + " + FpsWeb);
					if (BearingonWebOK)
						Reporting.AddLine("= " + (FpeWeb + FpsWeb) + ConstUnit.Force + " >= " + component.BraceConnect.Brace.WebForceTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + (FpeWeb + FpsWeb) + ConstUnit.Force + " << " + component.BraceConnect.Brace.WebForceTension + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("With Compressive Force:");
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength of Web Using Bolt Spacing (Wbs)");
					Reporting.AddLine("= n * tw * Wbrs");
					Reporting.AddLine("= (" + component.BraceConnect.SplicePlates.Bolt.NumberOfBolts + " * " + component.Shape.tw + " * " + SpBearing);
					Reporting.AddLine("= " + FpsWebComp + ConstUnit.Force);
					Reporting.AddLine("Total " + ConstString.DES_OR_ALLOWABLE + " Bearing Strength of Web (Wbr)");
					if (BearingonWebOKComp)
						Reporting.AddLine("= " + FpsWebComp + ConstUnit.Force + " >= " + component.BraceConnect.Brace.WebForceCompression + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + FpsWebComp + ConstUnit.Force + " << " + component.BraceConnect.Brace.WebForceCompression + ConstUnit.Force + " (NG)");
				}
			}
			else
				BearingonWebOK = true;

			if (!component.BraceConnect.ClawAngles.DoNotConnectFlanges)
			{
				fsz = component.BraceConnect.ClawAngles.Fillet;
				if (fsz < 0.5)
					fsz = 0.5;

				anglegage = (component.GageOnFlange - component.BraceConnect.Gusset.Thickness) / 2;
				HolesizePlus = component.BraceConnect.Brace.FlangeLong + ConstNum.SIXTEENTH_INCH;
				AvgFlange = 4 * (component.BoltGusset.SpacingLongDir * (component.BoltGusset.NumberOfBolts - 1) + component.BoltGusset.EdgeDistBrace) * component.Shape.tf;
				AvnFlange = 4 * (component.BoltGusset.SpacingLongDir * (component.BoltGusset.NumberOfBolts - 1) + component.BoltGusset.EdgeDistBrace - (component.BoltGusset.NumberOfBolts - 0.5) * HolesizePlus) * component.Shape.tf;
				HolesizePlus = component.BraceConnect.Brace.FlangeTrans + ConstNum.SIXTEENTH_INCH;
				AtgFlange = 2 * (component.Shape.bf - (component.BraceConnect.Gusset.Thickness + 2 * anglegage)) * component.Shape.tf;
				AtnFlange = 2 * (component.Shape.bf - (component.BraceConnect.Gusset.Thickness + 2 * anglegage) - HolesizePlus) * component.Shape.tf;

				Ubn = 1;
				FiRn = ConstNum.FIOMEGA0_75N * (0.6 * Math.Min(materialRy * component.Material.Fy * AvgFlange, materialRt * component.Material.Fu * AvnFlange) + Ubn * materialRt * component.Material.Fu * AtnFlange);

				FlangeTearOutOk = FiRn >= 2 * component.BraceConnect.Brace.FlangeForceTension;
			}

			if (!component.BraceConnect.SplicePlates.DoNotConnectWeb)
			{
				HolesizePlus = component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH;
				AvgWeb = 2 * (component.BraceConnect.SplicePlates.Bolt.SpacingLongDir * (component.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) + component.BraceConnect.SplicePlates.Bolt.EdgeDistBrace) * component.Shape.tw;
				AvnWeb = 2 * (component.BraceConnect.SplicePlates.Bolt.SpacingLongDir * (component.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) + component.BraceConnect.SplicePlates.Bolt.EdgeDistBrace - (component.BraceConnect.SplicePlates.Bolt.NumberOfRows - 0.5) * HolesizePlus) * component.Shape.tw;

				HolesizePlus = component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH;
				AtgWeb = component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir * (component.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * component.Shape.tw;
				AtnWeb = (component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir - HolesizePlus) * (component.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * component.Shape.tw;

				Ubn = 1;
				FiRn = ConstNum.FIOMEGA0_75N * (0.6 * Math.Min(materialRy * component.Material.Fy * AvgWeb, materialRt * component.Material.Fu * AvnWeb) + Ubn * materialRt * component.Material.Fu * AtnWeb);
				WebTearOutOk = FiRn >= component.BraceConnect.Brace.WebForceTension;
			}
			else
				WebTearOutOk = true;
			// Tension Rupture:
			if (!component.BraceConnect.ClawAngles.DoNotConnectFlanges && !component.BraceConnect.SplicePlates.DoNotConnectWeb)
			{
				U = 1;
				S = component.BraceConnect.ClawAngles.LengthB - component.BoltGusset.EdgeDistLongDir - component.BraceConnect.SplicePlates.LengthB + component.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir;
				g = (component.Shape.d - component.BraceConnect.SplicePlates.Width) / 2 + component.BraceConnect.SplicePlates.Bolt.EdgeDistTransvDir + anglegage;
				if (S > 0)
				{
					Anet1 = component.Shape.a - 4 * (component.BraceConnect.Brace.FlangeTrans + ConstNum.SIXTEENTH_INCH) * component.Shape.tf;
					Anet2 = Anet1 - component.BraceConnect.SplicePlates.Bolt.NumberOfLines * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
					Anet3 = Anet2 + S * S / 2 / g;
					Tca = 2 * component.BraceConnect.Brace.FlangeForceTension / component.BoltGusset.NumberOfBolts * (int) Math.Ceiling(Math.Abs(S) / component.BoltGusset.SpacingLongDir);
				}
				else
				{
					Anet1 = component.Shape.a - component.BraceConnect.SplicePlates.Bolt.NumberOfLines * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
					Anet2 = Anet1 - 4 * (component.BraceConnect.Brace.FlangeTrans + ConstNum.SIXTEENTH_INCH) * component.Shape.tf;
					Anet3 = Anet2 + S * S / 2 / g;
					Tca = component.BraceConnect.Brace.WebForceTension / component.BraceConnect.SplicePlates.Bolt.NumberOfRows * (int)Math.Ceiling(Math.Abs(S) / component.BraceConnect.SplicePlates.Bolt.SpacingLongDir);
				}
				NetTensionCap1 = Anet1 * ConstNum.FIOMEGA0_75N * materialRt * component.Material.Fu;
				NetTensionCap2 = Anet2 * ConstNum.FIOMEGA0_75N * materialRt * component.Material.Fu + Tca;
				NetTensionCap3 = Anet3 * ConstNum.FIOMEGA0_75N * materialRt * component.Material.Fu;

				if (NetTensionCap1 < NetTensionCap2)
				{
					NetTensionCap = NetTensionCap1;
					Anet = Anet1;
				}
				else
				{
					NetTensionCap = NetTensionCap2;
					Anet = Anet2;
				}
				if (NetTensionCap > NetTensionCap3)
				{
					NetTensionCap = NetTensionCap3;
					Anet = Anet3;
				}
			}
			else if (!component.BraceConnect.SplicePlates.DoNotConnectWeb)
			{
				b = component.Shape.bf / 2;
				t = component.Shape.tw / 2;
				a = component.Shape.a / 2;
				tf = component.Shape.tf;
				d = component.Shape.d;
				x_ = (Math.Pow(b, 2) * tf + Math.Pow(t, 2) * (d - 2 * tf) / 2) / a;
				L = component.BraceConnect.SplicePlates.LengthB - component.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir - component.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir;
				U = 1 - x_ / L;

				Anet = component.Shape.a - component.BraceConnect.SplicePlates.Bolt.NumberOfLines * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * component.Shape.tw;
				NetTensionCap = U * Anet * ConstNum.FIOMEGA0_75N * materialRt * component.Material.Fu;
			}
			else
			{
				x_ = SmallMethodsDesign.TeeExcentricity(component.Shape.bf, component.Shape.tf, component.Shape.d / 2, component.Shape.tw, component.Shape.a / 2);
				L = component.BraceConnect.ClawAngles.LengthB - component.BoltGusset.EdgeDistLongDir - component.BoltGusset.EdgeDistLongDir;
				U = 1 - x_ / L;

				Anet = component.Shape.a - 4 * (component.BraceConnect.Brace.FlangeTrans + ConstNum.SIXTEENTH_INCH) * component.Shape.tf;
				NetTensionCap = U * Anet * ConstNum.FIOMEGA0_75N * materialRt * component.Material.Fu;
			}
			NetTensionOk = NetTensionCap >= component.AxialTension;

			// Required Gross Area:
			FiPn = ConstNum.FIOMEGA0_95N * materialRy * component.Material.Fy * component.Shape.a;
			AgrossYielding = component.AxialTension / (ConstNum.FIOMEGA0_95N * materialRy * component.Material.Fy);
			AgAdditionalforYielding = Math.Max(0, AgrossYielding - component.Shape.a) * (materialRy * component.Material.Fy / component.BraceConnect.Gusset.Material.Fy);
			excessForceY = component.AxialTension - FiPn;
			// Check Stiffeners for Tension Rupture
			FiRn = NetTensionCap;
			excessForceR = component.AxialTension - FiRn;
			An_effective = component.AxialTension / (ConstNum.FIOMEGA0_75N * materialRt * component.Material.Fu);
			AgAdditionalforRupture = Math.Max(0, An_effective / U + component.Shape.a - Anet - component.Shape.a) * (materialRt * component.Material.Fu / component.BraceConnect.Gusset.Material.Fu);

			AgStiffener = (Math.Max(AgAdditionalforYielding, AgAdditionalforRupture));
			if (AgStiffener > 0)
			{
				component.BraceStiffener.Thickness = CommonCalculations.PlateThickness((int)(AgStiffener / (component.Shape.d - component.Shape.kdet - ConstNum.ONEANDHALF_INCHES)));
				component.BraceStiffener.WeldSize = (int)Math.Ceiling(16.0 * (Math.Min(component.Material.Fu * component.Shape.tf / 2.0, component.BraceConnect.Gusset.Material.Fu * component.BraceStiffener.Thickness) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx))) / 16.0;
			}
			excessforce = Math.Max(0, (Math.Max(excessForceY, excessForceR)));
			component.BraceStiffener.Width = component.Shape.d - component.Shape.kdet - ConstNum.ONEANDHALF_INCHES;
			component.BraceStiffener.Length = -component.EndSetback + component.BraceStiffener.WeldSize;
			component.BraceStiffener.Area = AgStiffener;
			component.BraceStiffener.Force = excessforce;

			if (enableReporting)
			{
				if (!component.BraceConnect.ClawAngles.DoNotConnectFlanges)
				{
					Reporting.AddHeader("Block Shear Strength of the Flange");
					Reporting.AddLine("Bolt Gage on Angle = " + anglegage + ConstUnit.Length);
					Reporting.AddLine("Bolt Hole Size (Longitudinal) = " + component.BraceConnect.Brace.FlangeLong + ConstUnit.Length);
					Reporting.AddLine("Bolt Hole Size (Transverse) = " + component.BraceConnect.Brace.FlangeTrans + ConstUnit.Length);

					Reporting.AddHeader("Gross Area with Shear Resistance:");
					Reporting.AddLine("Agv = 4 * (s * (n - 1) + e) * tf");
					Reporting.AddLine("= 4 * (" + component.BoltGusset.SpacingLongDir + " * (" + component.BoltGusset.NumberOfBolts + " - 1) + " + component.BoltGusset.EdgeDistBrace + ") * " + component.Shape.tf);
					Reporting.AddLine("= " + AvgFlange + ConstUnit.Area);

					Reporting.AddHeader("Net Area with Shear Resistance:");
					Reporting.AddLine("Anv = Agv - 4 * (n - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tf");
					Reporting.AddLine("= " + AvgFlange + " - 4 * (" + component.BoltGusset.NumberOfBolts + " - 0.5) * (" + (component.BraceConnect.Brace.FlangeLong + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.Shape.tf));
					Reporting.AddLine("= " + AvnFlange + ConstUnit.Area);

					Reporting.AddHeader("Gross Area with Tension Resistance:");
					Reporting.AddLine("Agt = 2 * (bf - (tg + 2 * ga)) * tf");
					Reporting.AddLine("= 2 * (" + component.Shape.bf + " - (" + component.BraceConnect.Gusset.Thickness + " + 2 * " + anglegage + ")) * " + component.Shape.tf);
					Reporting.AddLine("= " + AtgFlange + ConstUnit.Area);

					Reporting.AddHeader("Net Area with Tension Resistance:");
					Reporting.AddLine("Ant = Agt - 2 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tf");
					Reporting.AddLine("= " + AtgFlange + " - 2 * (" + component.BraceConnect.Brace.FlangeTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.Shape.tf);
					Reporting.AddLine("= " + AtnFlange + ConstUnit.Area);

					FiRn = SmallMethodsDesign.BlockShearPrint(AvgFlange, AvnFlange, AtgFlange, AtnFlange, component.Material.Fy, component.Material.Fu, true);

					if (FiRn >= 2 * component.BraceConnect.Brace.FlangeForceTension)
						Reporting.AddLine("= " + FiRn + " >= 2 * Pf = " + 2 * component.BraceConnect.Brace.FlangeForceTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + FiRn + " << 2 * Pf = " + 2 * component.BraceConnect.Brace.FlangeForceTension + ConstUnit.Force + " (NG)");
					if (AtnFlange <= 0)
						Reporting.AddLine("Bolts are outside brace flange (NG)");
				}
				if (!component.BraceConnect.SplicePlates.DoNotConnectWeb && component.BraceConnect.SplicePlates.Bolt.NumberOfLines > 1)
				{
					Reporting.AddHeader("Block Shear Strength of the Web");
					Reporting.AddLine("Bolt Hole Size (Longitudinal) = " + component.BraceConnect.Brace.WebLong + ConstUnit.Length);
					Reporting.AddLine("Bolt Hole Size (Transverse) = " + component.BraceConnect.Brace.WebTrans + ConstUnit.Length);

					Reporting.AddHeader("Gross Area with Shear Resistance:");
					Reporting.AddLine("Agv = 2 * (sl * (nl - 1) + e) * tw");
					Reporting.AddLine("= 2 * (" + component.BraceConnect.SplicePlates.Bolt.SpacingLongDir + " * (" + component.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 1) + " + component.BraceConnect.SplicePlates.Bolt.EdgeDistBrace + ") * " + component.Shape.tw);
					Reporting.AddLine("= " + AvgWeb + ConstUnit.Area);

					Reporting.AddHeader("Net Area with Shear Resistance:");
					Reporting.AddLine("Anv = Agv - 2 * (nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tw");
					Reporting.AddLine("= " + AvgWeb + " - 2 * (" + component.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 0.5) * (" + component.BraceConnect.Brace.WebLong + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.Shape.tw);
					Reporting.AddLine("= " + AvnWeb + ConstUnit.Area);

					Reporting.AddHeader("Gross Area with Tension Resistance:");
					Reporting.AddLine("Agt = st * (Nw - 1) * tw");
					Reporting.AddLine("= " + component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir + " * (" + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * " + component.Shape.tw);
					Reporting.AddLine("= " + AtgWeb + ConstUnit.Area);

					Reporting.AddHeader("Net Area with Tension Resistance:");
					Reporting.AddLine("Ant = Agt - (dh + " + ConstNum.SIXTEENTH_INCH + ") * (Nw - 1) * tw");
					Reporting.AddLine("= " + AtgWeb + " - (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * (" + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * " + component.Shape.tw);
					Reporting.AddLine("= " + AtnWeb + ConstUnit.Area);

					FiRn = SmallMethodsDesign.BlockShearPrint(AvgWeb, AvnWeb, AtgWeb, AtnWeb, component.Material.Fy, component.Material.Fu, true);

					if (FiRn >= component.BraceConnect.Brace.WebForceTension)
						Reporting.AddLine("= " + FiRn + " >= Pw = " + component.BraceConnect.Brace.WebForceTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + FiRn + " << Pw = " + component.BraceConnect.Brace.WebForceTension + ConstUnit.Force + " (NG)");
				}

				Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Tension Strength");
				Reporting.AddHeader("Tension Yielding:");
				FiPn = ConstNum.FIOMEGA0_9N * materialRy * component.Material.Fy * component.Shape.a;
				Reporting.AddLine(ConstString.PHI + " Pn = " + ConstString.FIOMEGA0_9 + " * " + ryString + Yildiz + " Fy * Ag = " + ConstString.FIOMEGA0_9 + " * " + ryStrength + Yildiz + component.Material.Fy + " * " + component.Shape.a);
				if (FiPn >= component.AxialTension)
					Reporting.AddLine("= " + FiPn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
				else
				{
					Reporting.AddLine("= " + FiPn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
					Reporting.AddLine("Brace Reinforcement Required");
				}

				Reporting.AddHeader("Tension Rupture");
				if (!component.BraceConnect.ClawAngles.DoNotConnectFlanges && !component.BraceConnect.SplicePlates.DoNotConnectWeb)
				{
					if (S > 0)
					{
						Reporting.AddLine("Net Area = An1 = A - 4 * (dh + " + ConstNum.SIXTEENTH_INCH + " ) * tf");
						Reporting.AddLine("= " + component.Shape.a + " - 4 * " + (component.BraceConnect.Brace.FlangeTrans + ConstNum.SIXTEENTH_INCH) + " * " + component.Shape.tf);
						Reporting.AddLine("= " + Anet1 + ConstUnit.Area);

						Reporting.AddLine("Net Area = An2 = An1 - Nw * (dh + " + ConstNum.SIXTEENTH_INCH + " ) * tw");
						Reporting.AddLine("= " + Anet1 + " - " + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " * (" + (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) + ") * " + component.Shape.tw);
						Reporting.AddLine("= " + Anet2 + ConstUnit.Area);

						Reporting.AddLine("An3 = An2 + s * s / 2 / g");
						Reporting.AddLine("= " + Anet2 + " + " + S + "² / 2 / " + g);
						Reporting.AddLine("= " + Anet3 + ConstUnit.Area);
					}
					else
					{
						Reporting.AddLine("Net Area = An1 = A - Nw * (dh + " + ConstNum.SIXTEENTH_INCH + " ) * tw");
						Reporting.AddLine("= " + component.Shape.a + " - " + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " * " + (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) + " * " + component.Shape.tw);
						Reporting.AddLine("= " + Anet1 + ConstUnit.Area);

						Reporting.AddLine("Net Area = An2 = An1 - 4 * (dh + " + ConstNum.SIXTEENTH_INCH + " ) * tf");
						Reporting.AddLine("= " + Anet1 + " - 4 * (" + (component.BraceConnect.Brace.FlangeTrans + ConstNum.SIXTEENTH_INCH) + ") * " + component.Shape.tf);
						Reporting.AddLine("= " + Anet2 + ConstUnit.Area);

						Reporting.AddLine("An3 = An2 + s * s / 2 / g");
						Reporting.AddLine("= " + Anet2 + " + " + Math.Abs(S) + "² / 2 / " + g);
						Reporting.AddLine("= " + Anet3 + ConstUnit.Area);
					}

					if (rtStrength == 0)
						NetTensionCap1 = NetTensionCap2 = NetTensionCap3 = 0;
					Reporting.AddLine("TCap1 = An1 * " + ConstString.FIOMEGA0_75 + " * " + rtString + Yildiz + "Fu = " + Anet1 + " * " + ConstString.FIOMEGA0_75 + " * " + rtStrength + Yildiz + " * " + component.Material.Fu + " = " + NetTensionCap1);
					Reporting.AddLine("TCap2 = An2 * " + ConstString.FIOMEGA0_75 + " * " + rtString + Yildiz + "Fu + Tca = " + Anet2 + " * " + ConstString.FIOMEGA0_75 + " * " + rtStrength + Yildiz + " * " + component.Material.Fu + " + " + Tca + " = " + NetTensionCap2);
					Reporting.AddLine("TCap3 = An3 * " + ConstString.FIOMEGA0_75 + " * " + rtString + Yildiz + "Fu = " + Anet3 + " * " + ConstString.FIOMEGA0_75 + " * " + rtStrength + Yildiz + " * " + component.Material.Fu + " = " + NetTensionCap3);

					if (NetTensionOk)
						Reporting.AddLine(ConstString.PHI + " Pn = " + NetTensionCap + ConstUnit.Force + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(ConstString.PHI + " Pn = " + NetTensionCap + ConstUnit.Force + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
				}
				else if (!component.BraceConnect.SplicePlates.DoNotConnectWeb)
				{
					Reporting.AddHeader("Shear Lag Factor (U) = 1 - x / L = 1 - " + x_ + " / " + L + " = " + U);
					Reporting.AddLine("Anet = A - nw * (dh +  " + ConstNum.SIXTEENTH_INCH + ") * tw");
					Reporting.AddLine("= " + component.Shape.a + " - " + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.Shape.tw);
					Reporting.AddLine("= " + Anet + ConstUnit.Area);
					Reporting.AddLine(ConstString.PHI + "Pn = U * Anet * " + ConstString.FIOMEGA0_75 + " * " + rtString + Yildiz + "Fu");
					Reporting.AddLine("= " + U + " * " + Anet + " * " + ConstString.FIOMEGA0_75 + " * " + rtStrength + Yildiz + component.Material.Fu);
					Reporting.AddLine("= " + NetTensionCap + ConstUnit.Force);
					if (NetTensionOk)
						Reporting.AddLine(" >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(" << " + component.AxialTension + ConstUnit.Force + " (NG)");
				}
				else
				{
					Reporting.AddHeader("Shear Lag Factor (U) = 1 - x / L = 1 - " + x_ + " / " + L + " = " + U);
					Anet = component.Shape.a - 4 * (component.BraceConnect.Brace.FlangeTrans + ConstNum.SIXTEENTH_INCH) * component.Shape.tf;
					NetTensionCap = U * Anet * ConstNum.FIOMEGA0_75N * materialRt * component.Material.Fu;

					Reporting.AddLine("Net Area = An = A - 4 * (dh + " + ConstNum.SIXTEENTH_INCH + " ) * tf");
					Reporting.AddLine("= " + component.Shape.a + " - 4 * " + (component.BraceConnect.Brace.FlangeTrans + ConstNum.SIXTEENTH_INCH) + " * " + component.Shape.tf);
					Reporting.AddLine("= " + Anet + ConstUnit.Area);
					Reporting.AddLine(ConstString.PHI + "Pn = U * An * " + ConstString.FIOMEGA0_75 + " * " + rtString + Yildiz + "Fu");
					Reporting.AddLine("= " + U + " * " + Anet + " * " + ConstString.FIOMEGA0_75 + " * " + rtStrength + Yildiz + component.Material.Fu);
					Reporting.AddLine("= " + NetTensionCap + ConstUnit.Force);
					if (NetTensionOk)
						Reporting.AddLine(" >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine(" << " + component.AxialTension + ConstUnit.Force + " (NG)");
				}
				if (NetTensionCap < component.AxialTension)
					Reporting.AddLine("(Brace Reinforcement Required.)");
			}

			// Required Gross Area:
			FiPn = ConstNum.FIOMEGA0_95N * materialRy * component.Material.Fy * component.Shape.a;
			AgrossYielding = component.AxialTension / (ConstNum.FIOMEGA0_95N * materialRy * component.Material.Fy);
			AgAdditionalforYielding = Math.Max(0, AgrossYielding - component.Shape.a) * (materialRy * component.Material.Fy / component.BraceConnect.Gusset.Material.Fy);
			excessForceY = component.AxialTension - FiPn;
			// Check Stiffeners for Tension Rupture
			FiRn = NetTensionCap;
			excessForceR = component.AxialTension - FiRn;
			An_effective = component.AxialTension / (ConstNum.FIOMEGA0_75N * materialRt * component.Material.Fu);

			AgAdditionalforRupture = Math.Max(0, (An_effective / U + component.Shape.a - Anet - component.Shape.a) * materialRt * component.Material.Fu / component.BraceConnect.Gusset.Material.Fu);
			if (enableReporting)
			{
				if (excessForceY > 0)
					Reporting.AddLine("Force in excess of Yield Strength = " + excessForceY + ConstUnit.Force);
				if (excessForceR > 0)
					Reporting.AddLine("Force in excess of Rupture Strength = " + excessForceR + ConstUnit.Force);
			}
			AgStiffener = (Math.Max(AgAdditionalforYielding, AgAdditionalforRupture));
			excessforce = (Math.Max(0, (Math.Max(excessForceY, excessForceR))));
			component.BraceStiffener.Width = component.Shape.d - component.Shape.kdet - ConstNum.ONEANDHALF_INCHES;
			component.BraceStiffener.Length = -component.EndSetback + component.BraceStiffener.WeldSize;
			component.BraceStiffener.Area = AgStiffener;
			component.BraceStiffener.Force = excessforce;
			if (component.BraceStiffener.Area > 0 && enableReporting)
			{
				Reporting.AddHeader("Required Reinforcement Area Using " + component.BraceConnect.Gusset.Material.Name + " Steel:");
				Reporting.AddLine("A_gross = " + component.BraceStiffener.Area + ConstUnit.Area);
				Reporting.AddLine("Use cover plates on web and/or flanges to provide the required area.");
				Reporting.AddLine("The length of plates must be sufficient to develop the excess force for yielding and rupture");
				Reporting.AddLine("with welds or bolts considering gross and effective net areas.");
			}

			return (BearingonFlangeOK && BearingonWebOK && FlangeTearOutOk && WebTearOutOk);
		}
	}
}