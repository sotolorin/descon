using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class DesignHSSBraceBoltedToGusset
	{
		internal static void CalcDesignHSSBraceBoltedtoGusset(EMemberType memberType)
		{
			double UF = 0;
			double t = 0;
			double tmin = 0;
			double tReqBlockShear = 0;
			double tReqG = 0;
			double Rn = 0;
			double Lntg = 0;
			double LntgOut = 0;
			double LgtgOut = 0;
			double LntgIn = 0;
			double LgtgIn = 0;
			double Lnvg = 0;
			double Lgvg = 0;
			double tn = 0;
			double HoleSize = 0;
			double AeRequired = 0;
			double tg = 0;
			double AgRequired = 0;
			double tmin1C = 0;
			double tmin1 = 0;
			double a_Comp = 0;
			double a = 0;
			double Fbrs = 0;
			double Fbre = 0;
			double hol = 0;
			int N_boltlines = 0;
			int N_req = 0;
			double excessforce = 0;
			double Weldsz2 = 0;
			double Weldsz1 = 0;
			double AgStiffener = 0;
			double AgAdditionalforRupture = 0;
			double An_effective = 0;
			double excessForceR = 0;
			double Ae = 0;
			double U = 0;
			double Xb = 0;
			double An = 0;
			double excessForceY = 0;
			double AgAdditionalforYielding = 0;
			double AgrossYielding = 0;
			bool loopAgain = false;
			double FiRn = 0;
			double weldforcapacity = 0;
			double weldL = 0;
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
			double Ag = 0;
			double AgtG = 0;
			double AgvG = 0;
			double AgtGIn = 0;
			double AgtGOut = 0;
			double AntG = 0;
			double AntGIn = 0;
			double AntGOut = 0;
			double AnvG = 0;
			double UtilizationFactor = 0;
			double Bracedevelopes = 0;
			double betaN = 0;
			double weldSize;

			DetailData brace = CommonDataStatic.DetailDataDict[memberType];

			SmallMethodsDesign.ExpectedStrengthCoefficient(memberType, ref r_t, ref r_y, ref r_t_Strength, ref r_y_Strength, ref Yildiz, ref r_t_String, ref r_y_String);

			brace.BraceConnect.ClawAngles.DoNotConnectFlanges = true;
			brace.BraceConnect.SplicePlates.DoNotConnectWeb = false;

			brace.BraceConnect.Brace.MaxForce = Math.Max(brace.AxialTension, brace.AxialCompression);
			brace.PFlange = brace.BraceConnect.Brace.MaxForce / 2;
			WeldArea = brace.PFlange / (2 * 0.7072 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
			minweldsize = CommonCalculations.MinimumWeld(brace.Shape.tf, brace.BraceConnect.SplicePlates.Thickness);
			usefulweldsizeforbrace = 1.414 * brace.Shape.tf * brace.Material.Fu / CommonDataStatic.Preferences.DefaultElectrode.Fexx;
			estimatedweldsize = (Math.Max(minweldsize, usefulweldsizeforbrace / 2));
			estimatedweldsize = (Math.Max(estimatedweldsize, Math.Pow(WeldArea, 0.5) / 10));
			if (brace.BraceConnect.Brace.WeldSize == 0.0)
			{
				if (brace.BraceConnect.Brace.BraceWeld.Weld1sz < estimatedweldsize)
					brace.BraceConnect.Brace.BraceWeld.Weld1sz = estimatedweldsize;
			}
			weldlength = WeldArea / (2 * Math.Min(brace.BraceConnect.Brace.BraceWeld.Weld1sz - ConstNum.SIXTEENTH_INCH, estimatedweldsize));
			if ((Math.Max(brace.Shape.d, brace.Shape.bf) > weldlength))
				weldlength = (Math.Max(brace.Shape.d, brace.Shape.bf));
			if (brace.BraceConnect.Brace.BraceWeld.Weld1L == 0.0)
			{
				weldL = weldlength;
				brace.BraceConnect.Brace.BraceWeld.Weld1L = weldL;
			}

			if (brace.BraceConnect.Brace.BraceWeld.Weld1L != 0)
				weldSize = Math.Min(WeldArea / (2 * brace.BraceConnect.Brace.BraceWeld.Weld1L) + ConstNum.SIXTEENTH_INCH, usefulweldsizeforbrace);
			else
				weldSize = usefulweldsizeforbrace;

			if (!brace.BraceConnect.Brace.WeldSize_User)
				brace.BraceConnect.Brace.BraceWeld.Weld1sz = NumberFun.Round(Math.Max(minweldsize, weldSize), 16);

			weldforcapacity = Math.Min(brace.BraceConnect.Brace.BraceWeld.Weld1sz - ConstNum.SIXTEENTH_INCH, usefulweldsizeforbrace);
			brace.BraceConnect.Brace.BraceWeld.Weld2sz = 0;
			brace.BraceConnect.SplicePlates.LengthB = brace.BraceConnect.Brace.BraceWeld.Weld1L;

			do
			{
				betaN = SmallMethodsDesign.WeldBetaFactor(brace.BraceConnect.Brace.BraceWeld.Weld1L, brace.BraceConnect.Brace.BraceWeld.Weld1sz);
				FiRn = betaN * 4 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * weldforcapacity * brace.BraceConnect.Brace.BraceWeld.Weld1L;
				if (FiRn >= brace.FxP)
					break;
				else
				{
					if (brace.BraceConnect.Brace.BraceWeld.Weld1sz <= weldforcapacity - ConstNum.SIXTEENTH_INCH && brace.BraceConnect.Brace.WeldSize == 0.0)
					{
						brace.BraceConnect.Brace.BraceWeld.Weld1sz = brace.BraceConnect.Brace.BraceWeld.Weld1sz + ConstNum.SIXTEENTH_INCH;
						loopAgain = false;
					}
					else
						brace.BraceConnect.Brace.BraceWeld.Weld1L = brace.BraceConnect.Brace.BraceWeld.Weld1L + ConstNum.QUARTER_INCH;
					loopAgain = true;
				}
			} while (loopAgain);

			brace.BraceConnect.SplicePlates.LengthB = brace.BraceConnect.Brace.BraceWeld.Weld1L;

			// Required Gross Area:
			AgrossYielding = brace.AxialTension / (ConstNum.FIOMEGA0_9N * r_y * brace.Material.Fy);
			AgAdditionalforYielding = Math.Max(0, AgrossYielding - brace.Shape.a) * (r_y * brace.Material.Fy / brace.BraceConnect.SplicePlates.Material.Fy);
			excessForceY = brace.AxialTension - ConstNum.FIOMEGA0_9N * r_y * brace.Material.Fy * brace.Shape.a;

			// Required Net Area:
			An = brace.Shape.a - 2 * (brace.BraceConnect.SplicePlates.Thickness + ConstNum.SIXTEENTH_INCH) * brace.Shape.tf;
			if (brace.Shape.d == 0 || brace.Shape.bf == 0)
				Xb = brace.Shape.bf / Math.PI;
			else
				Xb = (Math.Pow(brace.Shape.d, 2) + 2 * brace.Shape.d * brace.Shape.bf) / (4 * (brace.Shape.d + brace.Shape.bf));

			U = 1 - Xb / brace.BraceConnect.Brace.BraceWeld.Weld1L;
			Ae = U * An;
			FiRn = ConstNum.FIOMEGA0_75N * r_t * brace.Material.Fu * Ae;
			excessForceR = brace.AxialTension - FiRn;
			An_effective = brace.AxialTension / (ConstNum.FIOMEGA0_75N * r_t * brace.Material.Fu);
			AgAdditionalforRupture = Math.Max(0, An_effective / U + brace.Shape.a - An - brace.Shape.a) * (r_t * brace.Material.Fy / brace.BraceConnect.SplicePlates.Material.Fy);

			AgStiffener = Math.Max(AgAdditionalforYielding, AgAdditionalforRupture) / 2;
			if (AgStiffener > 0)
			{
				brace.BraceConnect.Brace.BcBraceStiffener.Thickness = CommonCalculations.PlateThickness(AgStiffener / (brace.Shape.bf + ConstNum.ONE_INCH));
				Weldsz1 = Math.Min(brace.BraceConnect.SplicePlates.Material.Fu * brace.BraceConnect.Brace.BcBraceStiffener.Thickness, brace.Material.Fu * brace.Shape.tf) / CommonDataStatic.Preferences.DefaultElectrode.Fexx; // Flare-Bevel Groov effective size
				Weldsz2 = 5 * brace.Shape.tf / 8;
				brace.BraceConnect.Brace.BcBraceStiffener.WeldSize = Math.Min(Weldsz1, Weldsz2);
				excessforce = Math.Max(0, Math.Max(excessForceY, excessForceR));
				weldlength = Math.Floor(4 * excessforce / (4 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * brace.BraceConnect.Brace.BcBraceStiffener.WeldSize)) / 4;
				weldlength = Math.Max(weldlength, brace.BraceConnect.Brace.BcBraceStiffener.Width);
				brace.BraceConnect.Brace.BcBraceStiffener.Width = brace.Shape.bf + ConstNum.ONE_INCH;
				brace.BraceConnect.Brace.BcBraceStiffener.Length = 2 * weldlength;
				brace.BraceConnect.Brace.BcBraceStiffener.Area = AgStiffener;
				brace.BraceConnect.Brace.BcBraceStiffener.Force = excessforce;
			}

			Reporting.AddHeader("Design " + CommonDataStatic.CommonLists.CompleteMemberList[MiscMethods.ComponentMinusOne(memberType)] + " Splice Plates");

			if (!brace.BraceConnect.SplicePlates.Width_User && (brace.BraceConnect.SplicePlates.Width == 0.0 || brace.BraceConnect.SplicePlates.Width < brace.Shape.d + ConstNum.TWO_INCHES))
				brace.BraceConnect.SplicePlates.Width = brace.Shape.d + ConstNum.TWO_INCHES;

			N_req = (int)Math.Floor(brace.BraceConnect.Brace.MaxForce / brace.BraceConnect.SplicePlates.Bolt.BoltStrength);

			if (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows == 0)
			{
				N_boltlines = brace.BraceConnect.SplicePlates.Bolt.NumberOfLines;
				brace.BraceConnect.SplicePlates.Bolt.EdgeDistTransvDir = (brace.BraceConnect.SplicePlates.Width - (N_boltlines - 1) * brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir) / 2;
				if (brace.BraceConnect.SplicePlates.Bolt.EdgeDistTransvDir < brace.BraceConnect.SplicePlates.Bolt.MinEdgeSheared)
				{
					brace.BraceConnect.SplicePlates.Bolt.EdgeDistTransvDir = brace.BraceConnect.SplicePlates.Bolt.MinEdgeSheared;
					if (!brace.BraceConnect.SplicePlates.Width_User)
						brace.BraceConnect.SplicePlates.Width = 2 * brace.BraceConnect.SplicePlates.Bolt.EdgeDistTransvDir + (N_boltlines - 1) * brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir;
				}
			}
			else
			{
				brace.BraceConnect.SplicePlates.Bolt.EdgeDistTransvDir = brace.BraceConnect.SplicePlates.Bolt.MinEdgeSheared;
				N_boltlines = ((int) Math.Floor((brace.BraceConnect.SplicePlates.Width - 2 * brace.BraceConnect.SplicePlates.Bolt.EdgeDistTransvDir) / brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir)) + 1;
				brace.BraceConnect.SplicePlates.Bolt.EdgeDistTransvDir = (brace.BraceConnect.SplicePlates.Width - (N_boltlines - 1) * brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir) / 2;
			}

			brace.BraceConnect.SplicePlates.Bolt.NumberOfLines = N_boltlines;
			if (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows == 0)
				brace.BraceConnect.SplicePlates.Bolt.NumberOfRows = brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts / N_boltlines;
			else if(N_req > 0)
				brace.BraceConnect.SplicePlates.Bolt.NumberOfRows = N_req / N_boltlines;
			
			brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts = brace.BraceConnect.SplicePlates.Bolt.NumberOfRows * brace.BraceConnect.SplicePlates.Bolt.NumberOfLines;

			hol = brace.BraceConnect.SplicePlates.HoleLongG;

			Fbre = CommonCalculations.EdgeBearing(brace.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir, hol, brace.BraceConnect.SplicePlates.Bolt.BoltSize, brace.BraceConnect.SplicePlates.Material.Fu, brace.BraceConnect.SplicePlates.Bolt.HoleType, false);
			Fbrs = CommonCalculations.SpacingBearing(brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir, hol, brace.BraceConnect.SplicePlates.Bolt.BoltSize, brace.BraceConnect.SplicePlates.Bolt.HoleType, brace.BraceConnect.SplicePlates.Material.Fu, false);
			
			a = Fbre * brace.BraceConnect.SplicePlates.Bolt.NumberOfLines + Fbrs * (brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts - brace.BraceConnect.SplicePlates.Bolt.NumberOfLines);
			a_Comp = Fbrs * brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts;
			tmin1 = Math.Max(brace.AxialTension / a, ConstNum.QUARTER_INCH);
			tmin1C = Math.Max(brace.AxialCompression / a_Comp, ConstNum.QUARTER_INCH);
			tmin1 = Math.Max(tmin1, tmin1C);

			// Tension Yielding
			AgRequired = brace.AxialTension / (ConstNum.FIOMEGA0_9N * brace.BraceConnect.SplicePlates.Material.Fy);
			tg = AgRequired / brace.BraceConnect.SplicePlates.Width;

			// Tension Rupture
			AeRequired = brace.AxialTension / (ConstNum.FIOMEGA0_75N * brace.BraceConnect.SplicePlates.Material.Fu);
			HoleSize = brace.BraceConnect.SplicePlates.HoleTransG;
			tn = AeRequired / Math.Min(brace.BraceConnect.SplicePlates.Width - brace.BraceConnect.SplicePlates.Bolt.NumberOfLines * (HoleSize + ConstNum.SIXTEENTH_INCH), 0.85 * brace.BraceConnect.SplicePlates.Width);

			// Block Shear Rupture
			Lgvg = 2 * ((brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) * brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir + brace.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir);
			Lnvg = Lgvg - 2 * (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 0.5) * (brace.BraceConnect.SplicePlates.HoleLongG + ConstNum.SIXTEENTH_INCH);
			LgtgIn = brace.BraceConnect.SplicePlates.Width - (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir;
			LntgIn = LgtgIn - (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * (brace.BraceConnect.SplicePlates.HoleTransG + ConstNum.SIXTEENTH_INCH);
			if (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines % 2 == 0)
			{
				LgtgOut = brace.BraceConnect.SplicePlates.Width - brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir;
				LntgOut = LgtgOut - (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * (brace.BraceConnect.SplicePlates.HoleTransG + ConstNum.SIXTEENTH_INCH);
				Lntg = Math.Min(LntgIn, LntgOut);
			}
			else
				Lntg = LntgIn;

			Rn = 0.6 * Math.Min(brace.BraceConnect.SplicePlates.Material.Fy * Lgvg, brace.BraceConnect.SplicePlates.Material.Fu * Lnvg) + 1 * brace.BraceConnect.SplicePlates.Material.Fu * Lntg; // per unit thickness
			tReqG = brace.AxialTension / (ConstNum.FIOMEGA0_75N * Rn);
			tReqBlockShear = tReqG;
			tmin = Math.Max(Math.Max(tmin1, tReqBlockShear), Math.Max(tg, tn));

			if (!brace.BraceConnect.SplicePlates.Thickness_User && brace.BraceConnect.SplicePlates.Thickness == 0.0)
				brace.BraceConnect.SplicePlates.Thickness = CommonCalculations.PlateThickness(tmin);

			t = brace.BraceConnect.SplicePlates.Thickness;
			loopAgain = false;
			do
			{
				SmallMethodsDesign.SplicePlateBuckling(memberType, ref UF, t);
				if (UF > 1)
				{
					t = t + ConstNum.SIXTEENTH_INCH;
					loopAgain = true;
				}
				else
					loopAgain = false;
			} while (loopAgain);

			if (!brace.BraceConnect.SplicePlates.Thickness_User && brace.BraceConnect.SplicePlates.Thickness == 0.0)
				brace.BraceConnect.SplicePlates.Thickness = CommonCalculations.PlateThickness(t);

			brace.BoltBrace = brace.BraceConnect.SplicePlates.Bolt;
			Reporting.AddHeader(CommonDataStatic.CommonLists.CompleteMemberList[memberType] + " to Gusset Connection");

			Reporting.AddLine("Brace Force (Tension)= " + brace.AxialTension + " " + ConstUnit.Force);
			Reporting.AddLine("Brace Force (Compression)= " + brace.AxialCompression + " " + ConstUnit.Force);
			Reporting.AddLine("Brace to Splice PL. Weld Size = " + CommonCalculations.WeldSize(brace.BraceConnect.Brace.BraceWeld.Weld1sz) + ConstUnit.Length);
			if (weldforcapacity < brace.BraceConnect.Brace.BraceWeld.Weld1sz)
				Reporting.AddLine(" (Use " + weldforcapacity + ConstUnit.Length + " for capacity calculation)");
			Reporting.AddLine("Brace to Splice PL. Weld Length = 4 X " + brace.BraceConnect.Brace.BraceWeld.Weld1L + ConstUnit.Length);
			Reporting.AddLine(" ");
			minweldsize = CommonCalculations.MinimumWeld(brace.Shape.tf, brace.BraceConnect.SplicePlates.Thickness);

			if (brace.BraceConnect.Brace.BraceWeld.Weld1sz >= minweldsize)
				Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(((int) brace.BraceConnect.Brace.BraceWeld.Weld1sz)) + " >= Minimum Weld Size = " + CommonCalculations.WeldSize(((int) minweldsize)) + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(((int) brace.BraceConnect.Brace.BraceWeld.Weld1sz)) + " << Minimum Weld Size = " + CommonCalculations.WeldSize(((int) minweldsize)) + ConstUnit.Length + " (NG)");

			betaN = SmallMethodsDesign.WeldBetaFactor(brace.BraceConnect.Brace.BraceWeld.Weld1L, brace.BraceConnect.Brace.BraceWeld.Weld1sz);
			FiRn = betaN * 4 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * weldforcapacity * brace.BraceConnect.Brace.BraceWeld.Weld1L;
			FiRn = betaN * 4 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * weldforcapacity * brace.BraceConnect.Brace.BraceWeld.Weld1L;
			Reporting.AddHeader("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
			Reporting.AddLine(ConstString.PHI + " Rn = " + betaN + "*4*" + ConstString.FIOMEGA0_75 + " * 0.6 * Fexx * 0.707 * w * L");
			Reporting.AddLine("= " + betaN + " * 4 * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + "*0.707*" + weldforcapacity + " * " + brace.BraceConnect.Brace.BraceWeld.Weld1L);
			if (FiRn >= brace.BraceConnect.Brace.MaxForce)
				Reporting.AddLine("= " + FiRn + " >= " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

			Bracedevelopes = 4 * ConstNum.FIOMEGA1_0N * 0.6 * brace.Material.Fy * brace.Shape.tf * brace.BraceConnect.Brace.BraceWeld.Weld1L;
			Reporting.AddHeader("Maximum Weld Force Brace Can Develop:");
			Reporting.AddLine(ConstString.PHI + " Rn = 4 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * Fy * t * L");
			Reporting.AddLine("= 4*" + ConstNum.FIOMEGA1_0N + " * 0.6 * " + brace.Material.Fy + " * " + brace.Shape.tf + " * " + brace.BraceConnect.Brace.BraceWeld.Weld1L);
			if (Bracedevelopes >= brace.BraceConnect.Brace.MaxForce)
				Reporting.AddLine("= " + Bracedevelopes + " >= " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + Bracedevelopes + " << " + brace.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Brace Check");
			Reporting.AddLine("Check " + CommonDataStatic.CommonLists.CompleteMemberList[MiscMethods.ComponentMinusOne(memberType)]);
			Reporting.AddLine("Tension Yielding of the Brace:");
			FiRn = ConstNum.FIOMEGA0_9N * r_y * brace.Material.Fy * brace.Shape.a;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * " + r_y_String + Yildiz + "Fy * Ag");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + r_y_Strength + " * " + brace.Material.Fy + " * " + brace.Shape.a);
			if (FiRn >= brace.AxialTension)
				Reporting.AddLine("= " + FiRn + " >= " + brace.AxialTension + ConstUnit.Force + " (OK)");
			else
			{
				Reporting.AddLine("= " + FiRn + " << " + brace.AxialTension + ConstUnit.Force + " (NG)");
				Reporting.AddLine("Brace Reinforcement Required.");
			}

			Reporting.AddHeader("Tension Rupture of the Brace:");
			An = brace.Shape.a - 2 * (brace.BraceConnect.SplicePlates.Thickness + ConstNum.SIXTEENTH_INCH) * brace.Shape.tf;
			Reporting.AddLine("An = Ag - 2 * (Tg + " + ConstNum.SIXTEENTH_INCH + ") * Tb");
			Reporting.AddLine("= " + brace.Shape.a + " - 2 * (" + brace.BraceConnect.SplicePlates.Thickness + " + " + ConstNum.SIXTEENTH_INCH + " )* " + brace.Shape.tf);
			Reporting.AddLine("= " + An + ConstUnit.Area);
			if (brace.Shape.d == 0 || brace.Shape.bf == 0)
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

			if (brace.BraceConnect.Brace.BraceWeld.Weld1L != 0)
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
				Reporting.AddLine("Brace Reinforcement Required.");
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
				Reporting.AddHeader("Plate length centered at end of slot ");
				Reporting.AddLine("Weld: Flare-Bevel Groove, 2 places Each Plate full length");
				Reporting.AddLine("Effective throat = " + brace.BraceConnect.Brace.BcBraceStiffener.WeldSize + ConstUnit.Length);
			}

			Reporting.AddHeader("Splice Plate");
			Reporting.AddLine("Splice plate: PL" + brace.BraceConnect.SplicePlates.Thickness + " X " + brace.BraceConnect.SplicePlates.Width + " X " + brace.BraceConnect.SplicePlates.Length);
			Reporting.AddLine("Bolts: (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts + ") " + brace.BraceConnect.SplicePlates.Bolt);
			N_req = (int)Math.Floor(brace.BraceConnect.Brace.MaxForce / brace.BraceConnect.SplicePlates.Bolt.BoltStrength);
			Reporting.AddLine("Number of Bolts Required:");
			if (N_req <= brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts)
				Reporting.AddLine("N_req = Fx/Fv = " + brace.BraceConnect.Brace.MaxForce + "/ " + brace.BraceConnect.SplicePlates.Bolt.BoltStrength + " = " + N_req + " <= " + brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts + " (OK)");
			else
				Reporting.AddLine("N_req = Fx/Fv = " + brace.BraceConnect.Brace.MaxForce + "/ " + brace.BraceConnect.SplicePlates.Bolt.BoltStrength + " = " + N_req + " >> " + brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts + " (NG)");

			Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength of Plate:");
			hol = brace.BraceConnect.SplicePlates.HoleLongG;
			Fbre = CommonCalculations.EdgeBearing(brace.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir, hol, brace.BraceConnect.SplicePlates.Bolt.BoltSize, brace.BraceConnect.SplicePlates.Material.Fu, brace.BraceConnect.SplicePlates.Bolt.HoleType, false);
			Fbrs = CommonCalculations.SpacingBearing(brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir, hol, brace.BraceConnect.SplicePlates.Bolt.BoltSize, brace.BraceConnect.SplicePlates.Bolt.HoleType, brace.BraceConnect.SplicePlates.Material.Fu, false);
			FiRn = (Fbre * brace.BraceConnect.SplicePlates.Bolt.NumberOfLines + Fbrs * (brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts - brace.BraceConnect.SplicePlates.Bolt.NumberOfLines)) * brace.BraceConnect.SplicePlates.Thickness;

			Reporting.AddHeader("With Tensile Force:");
			Reporting.AddLine(ConstString.PHI + " Rn = (Fbre * Nw + Fbrs * (n - Nw)) * tp");
			Reporting.AddLine("= (" + Fbre + " * " + brace.BraceConnect.SplicePlates.Bolt.NumberOfLines + " + " + Fbrs + " * (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts + " - " + brace.BraceConnect.SplicePlates.Bolt.NumberOfLines + ")) * " + brace.BraceConnect.SplicePlates.Thickness);
			if (FiRn >= brace.AxialTension)
				Reporting.AddLine("= " + FiRn + " >= " + brace.AxialTension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + brace.AxialTension + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("With Compressive Force:");
			Reporting.AddLine(ConstString.PHI + " Rn = Fbrs * N * tp");
			Reporting.AddLine("= " + Fbrs + " * " + brace.BraceConnect.SplicePlates.Bolt.NumberOfBolts + " * " + brace.BraceConnect.SplicePlates.Thickness);
			if (FiRn >= brace.AxialCompression)
				Reporting.AddLine("= " + FiRn + " >= " + brace.AxialCompression + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + brace.AxialCompression + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Tension Yielding of Plate:");
			Ag = brace.BraceConnect.SplicePlates.Width * brace.BraceConnect.SplicePlates.Thickness;
			Reporting.AddLine("Ag = Wp * t = " + brace.BraceConnect.SplicePlates.Width + " * " + brace.BraceConnect.SplicePlates.Thickness + " = " + Ag + ConstUnit.Area);
			Reporting.AddLine(" ");
			FiRn = ConstNum.FIOMEGA0_9N * brace.BraceConnect.SplicePlates.Material.Fy * Ag;
			//FiRn = BRACE1.TxtoNum(FiRn);
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * Ag");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + "* " + brace.BraceConnect.SplicePlates.Material.Fy + " * " + Ag);
			if (FiRn >= brace.AxialTension)
				Reporting.AddLine("= " + FiRn + " >= " + brace.AxialTension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + brace.AxialTension + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Tension Rupture of Plate:");
			HoleSize = Math.Max(brace.BraceConnect.SplicePlates.HoleTransB, brace.BraceConnect.SplicePlates.HoleTransG);
			An = Ag - brace.BraceConnect.SplicePlates.Bolt.NumberOfLines * (HoleSize + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.SplicePlates.Thickness;
			Reporting.AddLine("An = Ag - Nw * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tp");
			Reporting.AddLine("= " + Ag + " - " + brace.BraceConnect.SplicePlates.Bolt.NumberOfLines + " * (" + HoleSize + " + " + ConstNum.SIXTEENTH_INCH + ") * " + brace.BraceConnect.SplicePlates.Thickness);
			Reporting.AddLine("= " + An + ConstUnit.Area);
			Ae = Math.Min(0.85 * Ag, An);

			Reporting.AddLine("Ae = Min(0.85 * Ag, An) = " + Ae + ConstUnit.Area);
			FiRn = ConstNum.FIOMEGA0_75N * brace.BraceConnect.SplicePlates.Material.Fu * Ae;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Fu * Ae");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + brace.BraceConnect.SplicePlates.Material.Fu + " * " + Ae);
			if (FiRn >= brace.AxialTension)
				Reporting.AddLine("= " + FiRn + " >= " + brace.AxialTension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + brace.AxialTension + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Block Shear Rupture of Plate:");
			AgvG = 2 * ((brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) * brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir + brace.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir) * brace.BraceConnect.SplicePlates.Thickness;
			AnvG = AgvG - 2 * (brace.BraceConnect.SplicePlates.Bolt.NumberOfRows - 0.5) * (brace.BraceConnect.SplicePlates.HoleLongG + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.SplicePlates.Thickness;
			AgtGIn = (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir * brace.BraceConnect.SplicePlates.Thickness;
			AntGIn = AgtGIn - (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * (brace.BraceConnect.SplicePlates.HoleTransG + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.SplicePlates.Thickness;
			if (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines % 2 == 0)
			{
				AgtGOut = (brace.BraceConnect.SplicePlates.Width - brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir) * brace.BraceConnect.SplicePlates.Thickness;
				AntGOut = AgtGOut - (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * (brace.BraceConnect.SplicePlates.HoleTransG + ConstNum.SIXTEENTH_INCH) * brace.BraceConnect.SplicePlates.Thickness;
				AgtG = Math.Min(AgtGIn, AgtGOut);
				AntG = Math.Min(AntGIn, AntGOut);
			}
			else
			{
				AgtG = AgtGIn;
				AntG = AntGIn;
			}

			Reporting.AddLine("Agv = 2 * ((Nl - 1) * s + el) * tp");
			Reporting.AddLine("= 2 * ((" + brace.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 1) * " + brace.BraceConnect.SplicePlates.Bolt.SpacingLongDir + " + " + brace.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir + ")*" + brace.BraceConnect.SplicePlates.Thickness);
			Reporting.AddLine("= " + AgvG + ConstUnit.Area);

			Reporting.AddLine("Anv = Agv - 2 * (Nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tp");
			Reporting.AddLine("= " + AgvG + " - 2 * (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 0.5) * (" + brace.BraceConnect.SplicePlates.HoleLongG + " + " + ConstNum.SIXTEENTH_INCH + ")*" + brace.BraceConnect.SplicePlates.Thickness);
			Reporting.AddLine("= " + AnvG + ConstUnit.Area);

			Reporting.AddLine("Agt = (Nw - 1) * st * tp (Inside Block)");
			Reporting.AddLine("= (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * " + brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir + " * " + brace.BraceConnect.SplicePlates.Thickness);
			Reporting.AddLine("= " + AgtGIn + ConstUnit.Area);

			Reporting.AddLine("Ant = Agt - (Nw - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tp (Inside Block)");
			Reporting.AddLine("= " + AgtGIn + " - (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * (" + brace.BraceConnect.SplicePlates.HoleTransG + " + " + ConstNum.SIXTEENTH_INCH + ") * " + brace.BraceConnect.SplicePlates.Thickness);
			Reporting.AddLine("= " + AntGIn + ConstUnit.Area);

			if (brace.BraceConnect.SplicePlates.Bolt.NumberOfLines % 2 == 0)
			{
				Reporting.AddLine("Agto = (W - st) * tp (Outside Blocks)");
				Reporting.AddLine("= (" + brace.BraceConnect.SplicePlates.Width + " - " + brace.BraceConnect.SplicePlates.Bolt.SpacingTransvDir + ") * " + brace.BraceConnect.SplicePlates.Thickness);
				Reporting.AddLine("= " + AgtGOut + ConstUnit.Area);

				Reporting.AddLine("Anto = Agto - (Nw - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tp (Outside Blocks)");
				Reporting.AddLine("= " + AgtGOut + " - (" + brace.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * (" + brace.BraceConnect.SplicePlates.HoleTransG + " + " + ConstNum.SIXTEENTH_INCH + ") * " + brace.BraceConnect.SplicePlates.Thickness);
				Reporting.AddLine("= " + AntGOut + ConstUnit.Area);

				AgtG = Math.Min(AgtGIn, AgtGOut);
				AntG = Math.Min(AntGIn, AntGOut);
				Reporting.AddLine("Agt = Min(Agt, Agto)= " + AgtG + ConstUnit.Area);
				Reporting.AddLine("Ant = Min(Ant, Anto)= " + AntG + ConstUnit.Area);
			}
			else
			{
				AgtG = AgtGIn;
				AntG = AntGIn;
			}

			FiRn = SmallMethodsDesign.BlockShearPrint(AgvG, AnvG, AgtG, AntG, brace.BraceConnect.SplicePlates.Material.Fy, brace.BraceConnect.SplicePlates.Material.Fu, true);

			if (FiRn >= brace.AxialTension)
				Reporting.AddLine("= " + FiRn + " >= " + brace.AxialTension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + brace.AxialTension + ConstUnit.Force + " (NG)");

			t = brace.BraceConnect.SplicePlates.Thickness;
			SmallMethodsDesign.SplicePlateBuckling(memberType, ref UtilizationFactor, t);
		}
	}
}