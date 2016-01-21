using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class DesignHSSbracetoGusset
	{
		public static void CalcDesignHSSBraceToGusset(EMemberType memberType)
		{
			double Bracedevelopes = 0;
			double WeldCapacity = 0;
			double excessforce = 0;
			double Weldsz2 = 0;
			double Weldsz1 = 0;
			double AgStiffener = 0;
			double AgAdditionalforRupture;
			double An_effective = 0;
			double excessForceR = 0;
			double FiRn = 0;
			double Ae = 0;
			double U = 0;
			double Xb = 0;
			double An = 0;
			double excessForceY = 0;
			double AgAdditionalforYielding;
			double AgrossYielding = 0;
			double weldforcapacity = 0;
			double weldlength = 0;
			double estimatedweldsize = 0;
			double usefulweldsizeforbrace = 0;
			double minweldsize = 0;
			double WeldArea = 0;
			string r_y_String = "";
			string r_t_String = "";
			string Yildiz = "";
			double r_y_Strength = 0;
			double r_t_Strength = 0;
			double r_y = 0;
			double r_t = 0;

			var brace = CommonDataStatic.DetailDataDict[memberType];

			SmallMethodsDesign.ExpectedStrengthCoefficient(memberType, ref r_t, ref r_y, ref r_t_Strength, ref r_y_Strength, ref Yildiz, ref r_t_String, ref r_y_String);
			brace.BraceConnect.Brace.MaxForce = Math.Max(brace.AxialTension, brace.AxialCompression);

			if (!brace.IsActive)
				return;

			if (brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
			{
				DesignHSSBraceBoltedToGusset.CalcDesignHSSBraceBoltedtoGusset(memberType);
				return;
			}

			brace.BraceConnect.ClawAngles.DoNotConnectFlanges = true;
			brace.BraceConnect.SplicePlates.DoNotConnectWeb = true;
			brace.PFlange = Math.Max(brace.AxialTension / 2, brace.AxialCompression / 2);

			WeldArea = brace.PFlange / (0.707 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
			minweldsize = CommonCalculations.MinimumWeld(brace.Shape.tf, brace.BraceConnect.Gusset.Thickness);
			usefulweldsizeforbrace = 1.414 * brace.Shape.tf * brace.Material.Fu / CommonDataStatic.Preferences.DefaultElectrode.Fexx;
			estimatedweldsize = Math.Max(minweldsize, usefulweldsizeforbrace / 2);
			estimatedweldsize = Math.Max(estimatedweldsize, Math.Pow(WeldArea, 0.5) / 10);
			if (brace.BraceConnect.Brace.WeldSize == 0.0)
			{
				if (brace.BraceConnect.Brace.BraceWeld.Weld1sz < estimatedweldsize)
					brace.BraceConnect.Brace.BraceWeld.Weld1sz = estimatedweldsize;
			}
			if (-brace.EndSetback > weldlength)
			{
				weldlength = -brace.EndSetback;
				brace.BraceConnect.Brace.BraceWeld.Weld1sz = WeldArea / (2 * weldlength) + ConstNum.SIXTEENTH_INCH;
			}
			else
				weldlength = WeldArea / (2 * Math.Min(brace.BraceConnect.Brace.BraceWeld.Weld1sz - ConstNum.SIXTEENTH_INCH, estimatedweldsize));
			if (Math.Max(brace.Shape.d, brace.Shape.bf) > weldlength)
				weldlength = Math.Max(brace.Shape.d, brace.Shape.bf);
			if (-brace.EndSetback > weldlength)
				brace.BraceConnect.Brace.BraceWeld.Weld1L = -brace.EndSetback;
			else
				brace.BraceConnect.Brace.BraceWeld.Weld1L = weldlength;
			if (!brace.EndSetback_User)
				brace.EndSetback = Math.Floor(-brace.BraceConnect.Brace.BraceWeld.Weld1L * 4) / 4;
			brace.BraceConnect.Brace.BraceWeld.Weld1L = -brace.EndSetback;
			if (brace.BraceConnect.Brace.WeldSize == 0.0)
				brace.BraceConnect.Brace.BraceWeld.Weld1sz = NumberFun.Round(Math.Max(minweldsize, Math.Min(WeldArea / (2 * brace.BraceConnect.Brace.BraceWeld.Weld1L) + ConstNum.SIXTEENTH_INCH, usefulweldsizeforbrace)), 16);
			else
				brace.BraceConnect.Brace.BraceWeld.Weld1sz = NumberFun.Round(brace.BraceConnect.Brace.BraceWeld.Weld1sz, 16);
			brace.BraceConnect.Brace.BraceWeld.Weld2sz = 0;

			weldforcapacity = Math.Min(brace.BraceConnect.Brace.BraceWeld.Weld1sz - ConstNum.SIXTEENTH_INCH, usefulweldsizeforbrace);

			// Required Gross Area:
			AgrossYielding = brace.AxialTension / (ConstNum.FIOMEGA0_9N * r_y * brace.Material.Fy);
			AgAdditionalforYielding = Math.Max(0, AgrossYielding - brace.Shape.a) * (r_y * brace.Material.Fy / brace.BraceConnect.Gusset.Material.Fy);
			excessForceY = brace.AxialTension - ConstNum.FIOMEGA0_9N * r_y * brace.Material.Fy * brace.Shape.a;

			// Required Net Area:
			An = brace.Shape.a - 2 * (brace.BraceConnect.Gusset.Thickness + ConstNum.SIXTEENTH_INCH) * brace.Shape.tf;
			if (brace.Shape.d == 0 || brace.Shape.bf == 0)
				Xb = brace.Shape.bf / Math.PI;
			else
				Xb = (Math.Pow(brace.Shape.d, 2) + 2 * brace.Shape.d * brace.Shape.bf) / (4 * (brace.Shape.d + brace.Shape.bf));
			U = 1 - Xb / brace.BraceConnect.Brace.BraceWeld.Weld1L;
			if (U > 0.9)
				U = 0.9;
			Ae = U * An;

			FiRn = ConstNum.FIOMEGA0_75N * r_t * brace.Material.Fu * Ae;
			excessForceR = brace.AxialTension - FiRn;
			An_effective = brace.AxialTension / (ConstNum.FIOMEGA0_75N * r_t * brace.Material.Fu);
			AgAdditionalforRupture = Math.Max(0, An_effective / U + brace.Shape.a - An - brace.Shape.a) * (r_t * brace.Material.Fu / brace.BraceConnect.Gusset.Material.Fu);

			AgStiffener = Math.Max(AgAdditionalforYielding, AgAdditionalforRupture) / 2;
			if (AgStiffener > 0)
			{
				brace.BraceConnect.Brace.BcBraceStiffener.Thickness = CommonCalculations.PlateThickness((AgStiffener / (brace.Shape.bf + ConstNum.ONE_INCH)));
				Weldsz1 = Math.Min(brace.BraceConnect.SplicePlates.Material.Fu * brace.BraceConnect.Brace.BcBraceStiffener.Thickness, brace.Material.Fu * brace.Shape.tf) / CommonDataStatic.Preferences.DefaultElectrode.Fexx; // Flare-Bevel Groov effective size
				Weldsz2 = 5 * brace.Shape.tf / 8;
				brace.BraceConnect.Brace.BcBraceStiffener.WeldSize = Math.Min(Weldsz1, Weldsz2);
				excessforce = Math.Max(0, Math.Max(excessForceY, excessForceR));
				weldlength = -(Math.Floor(-4 * excessforce / (4 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * brace.BraceConnect.Brace.BcBraceStiffener.WeldSize))) / 4;
				brace.BraceConnect.Brace.BcBraceStiffener.Width = brace.Shape.bf + ConstNum.ONE_INCH;
				weldlength = Math.Max(weldlength, brace.BraceConnect.Brace.BcBraceStiffener.Width);
				brace.BraceConnect.Brace.BcBraceStiffener.Length = 2 * weldlength;
				brace.BraceConnect.Brace.BcBraceStiffener.Area = AgStiffener;
				brace.BraceConnect.Brace.BcBraceStiffener.Force = excessforce;
			}

			Reporting.AddHeader(CommonDataStatic.CommonLists.CompleteMemberList[memberType] + " to Gusset Connection");
			Reporting.AddLine("Brace Force (Tension) = " + brace.AxialTension + " " + ConstUnit.Force);
			Reporting.AddLine("Brace Force (Compression)= " + brace.AxialCompression + " " + ConstUnit.Force);
			Reporting.AddLine("Brace to Gusset Weld Size = " + CommonCalculations.WeldSize(brace.BraceConnect.Brace.BraceWeld.Weld1sz) + ConstUnit.Length);
			if (weldforcapacity < brace.BraceConnect.Brace.BraceWeld.Weld1sz)
				Reporting.AddLine("Use " + weldforcapacity + ConstUnit.Length + " for strength calculation");
			Reporting.AddLine("Brace to Gusset Weld Length = 4 X " + brace.BraceConnect.Brace.BraceWeld.Weld1L + ConstUnit.Length);
			WeldCapacity = 4 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * weldforcapacity * brace.BraceConnect.Brace.BraceWeld.Weld1L;
			if (WeldCapacity >= brace.BraceConnect.Brace.MaxForce)
				Reporting.AddLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + WeldCapacity + " >= " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + WeldCapacity + " << " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Maximum Weld Force Brace Can Develop:");
			Bracedevelopes = 4 * ConstNum.FIOMEGA0_75N * 0.6 * brace.Material.Fu * brace.Shape.tf * brace.BraceConnect.Brace.BraceWeld.Weld1L;
			if (Bracedevelopes >= brace.BraceConnect.Brace.MaxForce)
				Reporting.AddLine(ConstString.PHI + " Rn = " + Bracedevelopes + " >= " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine(ConstString.PHI + " Rn = " + Bracedevelopes + " << " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Tension Yielding of the Brace:");
			FiRn = ConstNum.FIOMEGA0_9N * r_y * brace.Material.Fy * brace.Shape.a;
			if (FiRn >= brace.AxialTension)
				Reporting.AddLine(ConstString.PHI + " Rn = " + FiRn + " >= " + brace.AxialTension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine(ConstString.PHI + " Rn = " + FiRn + " << " + brace.AxialTension + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Tension Rupture of the Brace:");
			An = brace.Shape.a - 2 * (brace.BraceConnect.Gusset.Thickness + ConstNum.SIXTEENTH_INCH) * brace.Shape.tf;
			if (brace.Shape.d == 0 || brace.Shape.bf == 0)
				Xb = brace.Shape.bf / Math.PI;
			else
				Xb = (Math.Pow(brace.Shape.d, 2) + 2 * brace.Shape.d * brace.Shape.bf) / (4 * (brace.Shape.d + brace.Shape.bf));
			U = 1 - Xb / brace.BraceConnect.Brace.BraceWeld.Weld1L;
			if (U > 0.9)
				U = 0.9;
			Ae = U * An;
			FiRn = ConstNum.FIOMEGA0_75N * r_t * brace.Material.Fu * Ae;
			if (FiRn >= brace.AxialTension)
				Reporting.AddLine(ConstString.PHI + " Rn = " + FiRn + " >= " + brace.AxialTension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine(ConstString.PHI + " Rn = " + FiRn + " << " + brace.AxialTension + ConstUnit.Force + " (NG)");

			Reporting.AddHeader(CommonDataStatic.CommonLists.CompleteMemberList[memberType] + " to Gusset Connection");
			Reporting.AddLine("Brace Force (Tension) = " + brace.AxialTension + " " + ConstUnit.Force);
			Reporting.AddLine("Brace Force (Compression)= " + brace.AxialCompression + " " + ConstUnit.Force);
			Reporting.AddLine("Brace to Gusset Weld Size = " + CommonCalculations.WeldSize(brace.BraceConnect.Brace.BraceWeld.Weld1sz) + ConstUnit.Length);
			if (weldforcapacity < brace.BraceConnect.Brace.BraceWeld.Weld1sz)
				Reporting.AddLine(" (Use " + weldforcapacity + ConstUnit.Length + " for capacity calculation)");
			Reporting.AddLine("Brace to Gusset Weld Length = 4 X " + brace.BraceConnect.Brace.BraceWeld.Weld1L + ConstUnit.Length);

			minweldsize = CommonCalculations.MinimumWeld(brace.Shape.tf, brace.BraceConnect.Gusset.Thickness);
			if (brace.BraceConnect.Brace.BraceWeld.Weld1sz >= minweldsize)
				Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(brace.BraceConnect.Brace.BraceWeld.Weld1sz) + " >= Minimum Weld Size = " + CommonCalculations.WeldSize(minweldsize) + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(brace.BraceConnect.Brace.BraceWeld.Weld1sz) + " << Minimum Weld Size = " + CommonCalculations.WeldSize(minweldsize) + ConstUnit.Length + " (NG)");

			double betaN = SmallMethodsDesign.WeldBetaFactor(brace.BraceConnect.Brace.BraceWeld.Weld1L, brace.BraceConnect.Brace.BraceWeld.Weld1sz);
			FiRn = betaN * 4 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * weldforcapacity * brace.BraceConnect.Brace.BraceWeld.Weld1L;

			Reporting.AddHeader("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
			Reporting.AddLine(ConstString.PHI + " Rn = " + betaN + "*4*" + ConstString.FIOMEGA0_75 + " * 0.6 * Fexx * 0.707 * w * L");
			Reporting.AddLine("= " + betaN + " * 4 * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * 0.707 * " + weldforcapacity + " * " + brace.BraceConnect.Brace.BraceWeld.Weld1L);
			if (FiRn >= brace.BraceConnect.Brace.MaxForce)
				Reporting.AddLine("= " + FiRn + " >= " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

			Bracedevelopes = 4 * ConstNum.FIOMEGA0_75N * 0.6 * brace.Material.Fu * brace.Shape.tf * brace.BraceConnect.Brace.BraceWeld.Weld1L;
			Reporting.AddHeader("Maximum Weld Force Brace Can Develop:");
			Reporting.AddLine(ConstString.PHI + " Rn = 4 * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu * t * L");
			Reporting.AddLine("= 4*" + ConstString.FIOMEGA0_75 + " * 0.6*" + brace.Material.Fu + " * " + brace.Shape.tf + " * " + brace.BraceConnect.Brace.BraceWeld.Weld1L);
			if (Bracedevelopes >= brace.BraceConnect.Brace.MaxForce)
				Reporting.AddLine("= " + Bracedevelopes + " >= " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + Bracedevelopes + " << " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Check " + CommonDataStatic.CommonLists.CompleteMemberList[memberType]);
			Reporting.AddHeader("Tension Yielding of the Brace:");
			FiRn = ConstNum.FIOMEGA0_9N * r_y * brace.Material.Fy * brace.Shape.a;

			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * " + r_y_String + Yildiz + "Fy * Ag");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + r_y_Strength + Yildiz + brace.Material.Fy + " * " + brace.Shape.a);

			if (FiRn >= brace.AxialTension)
			{
				Reporting.AddLine("= " + FiRn + " >= " + brace.AxialTension + ConstUnit.Force + " (OK)");
			}
			else
			{
				Reporting.AddLine("= " + FiRn + " << " + brace.AxialTension + ConstUnit.Force + " (NG)");
				Reporting.AddLine("(Brace Reinforcement Required.)");
			}

			Reporting.AddHeader("Tension Rupture of the Brace:");
			An = brace.Shape.a - 2 * (brace.BraceConnect.Gusset.Thickness + ConstNum.SIXTEENTH_INCH) * brace.Shape.tf;
			Reporting.AddLine("An = Ag - 2 * (Tg + " + ConstNum.SIXTEENTH_INCH + " ) * Tb");
			Reporting.AddLine("= " + brace.Shape.a + " - 2 * (" + brace.BraceConnect.Gusset.Thickness + " + " + ConstNum.SIXTEENTH_INCH + " ) * " + brace.Shape.tf);
			Reporting.AddLine("= " + An + ConstUnit.Area);
			if (brace.Shape.d == 0.0 || brace.Shape.bf == 0.0)
			{
				Xb = (brace.Shape.d + brace.Shape.bf) / Math.PI;
				Reporting.AddLine("x = D / Pi = " + (brace.Shape.d + brace.Shape.bf) + " / " + Math.PI + " = " + Xb + ConstUnit.Length);
			}
			else
			{
				Xb = (Math.Pow(brace.Shape.d, 2) + 2 * brace.Shape.d * brace.Shape.bf) / (4 * (brace.Shape.d + brace.Shape.bf));
				Reporting.AddLine("x = ((B or H)² + 2 * B * H) / (4 * (B + H))");
				Reporting.AddLine("= (" + brace.Shape.d + "² + 2 * " + brace.Shape.d + " * " + brace.Shape.bf + ") / (4 * (" + brace.Shape.d + " + " + brace.Shape.bf + "))");
				Reporting.AddLine("= " + Xb + ConstUnit.Length);
			}

			U = 1 - Xb / brace.BraceConnect.Brace.BraceWeld.Weld1L;
			Reporting.AddLine("U = 1 - (x / L)");
			Reporting.AddLine("= 1 - (" + Xb + " / " + brace.BraceConnect.Brace.BraceWeld.Weld1L + ")");
			Reporting.AddLine("= " + U);

			Ae = U * An;
			Reporting.AddLine("Ae = U * An = " + U + " * " + An + " = " + Ae + ConstUnit.Area);
			FiRn = ConstNum.FIOMEGA0_75N * r_t * brace.Material.Fu * Ae;
			//FiRn = BRACE1.TxtoNum(FiRn);
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * " + r_t_String + Yildiz + "Fu * Ae");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + r_t_Strength + Yildiz + brace.Material.Fu + " * " + Ae);
			if (FiRn >= brace.AxialTension)
				Reporting.AddLine("= " + FiRn + " >= " + brace.AxialTension + ConstUnit.Force + " (OK)");
			else
			{
				Reporting.AddLine("= " + FiRn + " << " + brace.AxialTension + ConstUnit.Force + " (NG)");
				Reporting.AddLine("(Brace Reinforcement Required.)");
			}

			if (excessForceY > 0)
				Reporting.AddLine("Force in excess of Yield Strength = " + excessForceY + ConstUnit.Force);
			if (excessForceR > 0)
				Reporting.AddLine("Force in excess of Rupture Strength = " + excessForceR + ConstUnit.Force);
			if (brace.BraceConnect.Brace.BcBraceStiffener.Area > 0)
			{
				Reporting.AddHeader("Required Reinforcement Area Using " + brace.BraceConnect.SplicePlates.Material.Name + " Steel");
				Reporting.AddLine("= " + brace.BraceConnect.Brace.BcBraceStiffener.Area + ConstUnit.Area);
				Reporting.AddHeader("Use 2PL Each with:");
				Reporting.AddLine("Thickness = " + brace.BraceConnect.Brace.BcBraceStiffener.Thickness + ConstUnit.Length);
				Reporting.AddLine("Width = " + brace.BraceConnect.Brace.BcBraceStiffener.Width + ConstUnit.Length);
				Reporting.AddLine("Length = " + 2 * weldlength + ConstUnit.Length);
				Reporting.AddHeader("(Plate length centered at end of slot)");
				Reporting.AddLine("Weld:Flare-Bevel Groove, 2 places Each Plate full length");
				Reporting.AddLine("Effective throat = " + brace.BraceConnect.Brace.BcBraceStiffener.WeldSize + ConstUnit.Length);
			}
		}
	}
}