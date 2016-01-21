using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class DesignSplicePlate
	{
		public static void CalcDesignSplicePlate(EMemberType memberType)
		{
			double FiRnG = 0;
			double AntG = 0;
			double AgtG = 0;
			double AntGOut = 0;
			double AgtGOut = 0;
			double AntGIn = 0;
			double AgtGIn = 0;
			double AnvG = 0;
			double AgvG = 0;
			double FiRnB = 0;
			double AntB = 0;
			double AgtB = 0;
			double AntbOut = 0;
			double AgtbOut = 0;
			double AntbIn = 0;
			double AgtbIn = 0;
			double AnvB = 0;
			double AgvB = 0;
			double Ae = 0;
			double Ag = 0;
			double FiRnComp = 0;
			double FiRn = 0;
			double T_dist = 0;
			double Wp = 0;
			double L = 0;
			double An = 0;
			double tmin = 0;
			double tReqBlockShear = 0;
			double tReqG = 0;
			double Lntg = 0;
			double LntgOut = 0;
			double LgtgOut = 0;
			double LntgIn = 0;
			double LgtgIn = 0;
			double Lnvg = 0;
			double Lgvg = 0;
			double tReqB = 0;
			double Rn = 0;
			int Unb = 0;
			double Lntb = 0;
			double Lgtb = 0;
			double LntbOut = 0;
			double LgtbOut = 0;
			double LntbIn = 0;
			double LgtbIn = 0;
			double Lnvb = 0;
			double Lgvb = 0;
			double tn = 0;
			double HoleSize = 0;
			double AeRequired = 0;
			double tg = 0;
			double AgRequired = 0;
			double tmin1 = 0;
			double aC = 0;
			int aT = 0;
			double Fbrs = 0;
			double Fbre = 0;
			double hol = 0;
			double boltnwmax = 0;
			double plwidth = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];

			if (component.BraceConnect.SplicePlates.DoNotConnectWeb)
				return;

			Reporting.AddLine("Design " + CommonDataStatic.CommonLists.CompleteMemberList[MiscMethods.ComponentMinusOne(memberType)] + " Splice Plates");

			plwidth = component.Shape.t;
			boltnwmax = ((int) Math.Floor((plwidth - 2 * component.BraceConnect.SplicePlates.Bolt.EdgeDistTransvDir) / component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir)) + 1;
			if (boltnwmax < 2)
			{
				boltnwmax = 1;
				component.BraceConnect.SplicePlates.Bolt.NumberOfLines = 1;
			}

			component.BraceConnect.SplicePlates.Bolt.NumberOfBolts = component.BraceConnect.SplicePlates.Bolt.NumberOfLines * component.BraceConnect.SplicePlates.Bolt.NumberOfRows;
			component.BraceConnect.SplicePlates.Length = component.BraceConnect.SplicePlates.LengthG + component.EndSetback + component.BraceConnect.SplicePlates.LengthB;

			hol = component.BraceConnect.SplicePlates.HoleLongB;

			Fbre = CommonCalculations.EdgeBearing(component.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir, (hol), component.BraceConnect.SplicePlates.Bolt.BoltSize, component.BraceConnect.SplicePlates.Material.Fu, component.BraceConnect.SplicePlates.Bolt.HoleType, false);

			Fbrs = CommonCalculations.SpacingBearing(component.BraceConnect.SplicePlates.Bolt.SpacingLongDir, hol, component.BraceConnect.SplicePlates.Bolt.BoltSize, component.BraceConnect.SplicePlates.Bolt.HoleType, component.BraceConnect.SplicePlates.Material.Fu, false);
			aT = ((int) (Fbre * component.BraceConnect.SplicePlates.Bolt.NumberOfLines + Fbrs * (component.BraceConnect.SplicePlates.Bolt.NumberOfBolts - component.BraceConnect.SplicePlates.Bolt.NumberOfLines)));
			aC = Fbrs * component.BraceConnect.SplicePlates.Bolt.NumberOfBolts;
			tmin1 = Math.Max(Math.Max(component.BraceConnect.Brace.WebForceTension / (2 * aT), component.AxialCompression / (2 * aC)), ConstNum.QUARTER_INCH);
			// Tension Yielding
			AgRequired = component.PWeb / (2 * ConstNum.FIOMEGA0_9N * component.BraceConnect.SplicePlates.Material.Fy);
			tg = AgRequired / component.BraceConnect.SplicePlates.Width;
			// Tension Rupture
			AeRequired = component.BraceConnect.Brace.WebForceTension / (2 * ConstNum.FIOMEGA0_75N * component.BraceConnect.SplicePlates.Material.Fu);
			HoleSize = component.BraceConnect.SplicePlates.HoleTransG;
			if (HoleSize < component.BraceConnect.SplicePlates.HoleTransB)
				HoleSize = component.BraceConnect.SplicePlates.HoleTransB;
			tn = AeRequired / Math.Min(component.BraceConnect.SplicePlates.Width - component.BraceConnect.SplicePlates.Bolt.NumberOfLines * (HoleSize + ConstNum.SIXTEENTH_INCH), 0.85 * component.BraceConnect.SplicePlates.Width);

			Lgvb = 2 * ((component.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) * component.BraceConnect.SplicePlates.Bolt.SpacingLongDir + component.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir);
			Lnvb = Lgvb - 2 * (component.BraceConnect.SplicePlates.Bolt.NumberOfRows - 0.5) * (component.BraceConnect.SplicePlates.HoleLongB + ConstNum.SIXTEENTH_INCH);
			LgtbIn = component.BraceConnect.SplicePlates.Width - (component.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir;
			LntbIn = LgtbIn - (component.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * (component.BraceConnect.SplicePlates.HoleTransB + ConstNum.SIXTEENTH_INCH);
			if (component.BraceConnect.SplicePlates.Bolt.NumberOfLines % 2 == 0)
			{
				LgtbOut = component.BraceConnect.SplicePlates.Width - component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir;
				LntbOut = LgtbOut - (component.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * (component.BraceConnect.SplicePlates.HoleTransB + ConstNum.SIXTEENTH_INCH);
				Lgtb = Math.Min(LgtbIn, LgtbOut);
				Lntb = Math.Min(LntbIn, LntbOut);
			}
			else
			{
				Lgtb = LgtbIn;
				Lntb = LntbIn;
			}

			if (Lntb >= 0.6 * Lnvb)
			{
				Unb = 1;
				Rn = 0.6 * Math.Min(component.BraceConnect.SplicePlates.Material.Fy * Lgvb, component.BraceConnect.SplicePlates.Material.Fu * Lnvb) + Unb * component.BraceConnect.SplicePlates.Material.Fu * Lntb;
			}
			else
				Rn = 0.6 * component.BraceConnect.SplicePlates.Material.Fu * Lnvb + Math.Min(component.BraceConnect.SplicePlates.Material.Fy * Lgtb, component.BraceConnect.SplicePlates.Material.Fu * Lntb);

			tReqB = component.BraceConnect.Brace.WebForceTension / (2 * ConstNum.FIOMEGA0_75N * Rn);
			Lgvg = 2 * ((component.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) * component.BraceConnect.SplicePlates.Bolt.SpacingLongDir + component.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir);
			Lnvg = Lgvg - 2 * (component.BraceConnect.SplicePlates.Bolt.NumberOfRows - 0.5) * (component.BraceConnect.SplicePlates.HoleLongG + ConstNum.SIXTEENTH_INCH);
			LgtgIn = component.BraceConnect.SplicePlates.Width - (component.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir;
			LntgIn = LgtgIn - (component.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * (component.BraceConnect.SplicePlates.HoleTransG + ConstNum.SIXTEENTH_INCH);
			if (component.BraceConnect.SplicePlates.Bolt.NumberOfLines % 2 == 0)
			{
				LgtgOut = component.BraceConnect.SplicePlates.Width - component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir;
				LntgOut = LgtgOut - (component.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * (component.BraceConnect.SplicePlates.HoleTransG + ConstNum.SIXTEENTH_INCH);
				Lntg = Math.Min(LntgIn, LntgOut);
			}
			else
				Lntg = LntgIn;
			
			Rn = 0.6 * Math.Min(component.BraceConnect.SplicePlates.Material.Fu * Lnvg, component.BraceConnect.SplicePlates.Material.Fy * Lgvg) + 1 * component.BraceConnect.SplicePlates.Material.Fu * Lntg; // per unit thickness
			tReqG = component.BraceConnect.Brace.WebForceTension / (2 * ConstNum.FIOMEGA0_75N * Rn);
			tReqBlockShear = Math.Max(tReqG, tReqB);

			tmin = Math.Max(Math.Max(tmin1, tReqBlockShear), Math.Max(tg, tn));

			if (tmin < tg)
				tmin = tg;
			if (tmin < tn)
				tmin = tn;

			if (!component.BraceConnect.SplicePlates.Thickness_User)
				component.BraceConnect.SplicePlates.Thickness = NumberFun.ConvertFromFraction(tmin);
			An = component.BraceConnect.SplicePlates.Thickness * (component.BraceConnect.SplicePlates.Width - component.BraceConnect.SplicePlates.Bolt.NumberOfLines * (HoleSize + ConstNum.SIXTEENTH_INCH));

			Reporting.AddHeader("Splice plates: 2PL " + component.BraceConnect.SplicePlates.Thickness + " X " + component.BraceConnect.SplicePlates.Width + " X " + component.BraceConnect.SplicePlates.Length);
			L = 2 * ((component.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) * component.BraceConnect.SplicePlates.Bolt.SpacingLongDir + component.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir) + component.BraceConnect.SplicePlates.Bolt.EdgeDistGusset + component.BraceConnect.SplicePlates.Bolt.EdgeDistBrace + component.EndSetback;
			Reporting.AddHeader("Check Plate Length:");
			Reporting.AddLine("L = 2 * ((Nbwl - 1) * sl + ep) + eg + ebr + g");
			Reporting.AddLine("= 2 * ((" + component.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 1) * " + component.BraceConnect.SplicePlates.Bolt.SpacingLongDir + " + " + component.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir + ") + " + component.BraceConnect.SplicePlates.Bolt.EdgeDistGusset + " + " + component.BraceConnect.SplicePlates.Bolt.EdgeDistBrace + " + " + component.EndSetback);
			if (L <= component.BraceConnect.SplicePlates.Length)
				Reporting.AddLine("= " + L + " <= " + component.BraceConnect.SplicePlates.Length + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("= " + L + " >> " + component.BraceConnect.SplicePlates.Length + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Check Plate Width:");
			Wp = (component.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir + 2 * component.BraceConnect.SplicePlates.Bolt.EdgeDistTransvDir;
			Reporting.AddLine("Wp = (Nw - 1) * s + 2 * e = (" + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * " + component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir + " + 2 * " + component.BraceConnect.SplicePlates.Bolt.EdgeDistTransvDir);
			if (Wp <= component.BraceConnect.SplicePlates.Width)
				Reporting.AddLine("= " + Wp + " <= " + component.BraceConnect.SplicePlates.Width + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("= " + Wp + " >> " + component.BraceConnect.SplicePlates.Width + ConstUnit.Length + " (NG)");

			T_dist = component.Shape.t;
			if (component.BraceConnect.SplicePlates.Width <= T_dist)
				Reporting.AddLine("Plate Width = " + component.BraceConnect.SplicePlates.Width + " <= Brace T-dist. = " + T_dist + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Plate Width = " + component.BraceConnect.SplicePlates.Width + " >> Brace T-dist. = " + T_dist + ConstUnit.Length + " (NG)");

			Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Bearing Strength of Plates:");
			hol = component.BraceConnect.SplicePlates.HoleLongB;

			Fbre = CommonCalculations.EdgeBearing(component.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir, (hol), component.BraceConnect.SplicePlates.Bolt.BoltSize, component.BraceConnect.SplicePlates.Material.Fu, component.BraceConnect.SplicePlates.Bolt.HoleType, false);
			Fbrs = CommonCalculations.SpacingBearing(component.BraceConnect.SplicePlates.Bolt.SpacingLongDir, hol, component.BraceConnect.SplicePlates.Bolt.BoltSize, component.BraceConnect.SplicePlates.Bolt.HoleType, component.BraceConnect.SplicePlates.Material.Fu, false);
			FiRn = 2 * (Fbre * component.BraceConnect.SplicePlates.Bolt.NumberOfLines + Fbrs * (component.BraceConnect.SplicePlates.Bolt.NumberOfBolts - component.BraceConnect.SplicePlates.Bolt.NumberOfLines)) * component.BraceConnect.SplicePlates.Thickness;
			FiRnComp = 2 * Fbrs * component.BraceConnect.SplicePlates.Bolt.NumberOfBolts * component.BraceConnect.SplicePlates.Thickness;

			Reporting.AddHeader("With Tensile Force:");
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Fbre * Nw + Fbrs * (n - Nw)) * tp");
			Reporting.AddLine("= 2 * (" + Fbre + " * " + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " + " + Fbrs + " * (" + component.BraceConnect.SplicePlates.Bolt.NumberOfBolts + " - " + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + ")) * " + component.BraceConnect.SplicePlates.Thickness);
			if (FiRn >= component.BraceConnect.Brace.WebForceTension)
				Reporting.AddLine("= " + FiRn + " >= " + component.BraceConnect.Brace.WebForceTension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + component.BraceConnect.Brace.WebForceTension + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("With Compressive Force:");
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * Fbrs * n * tp");
			Reporting.AddLine("= 2*" + Fbrs + " * " + component.BraceConnect.SplicePlates.Bolt.NumberOfBolts + " * " + component.BraceConnect.SplicePlates.Thickness);
			if (FiRn >= component.BraceConnect.Brace.WebForceCompression)
				Reporting.AddLine("= " + FiRnComp + " >= " + component.BraceConnect.Brace.WebForceCompression + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRnComp + " << " + component.BraceConnect.Brace.WebForceCompression + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Tension Yielding of Plates:");
			Ag = component.BraceConnect.SplicePlates.Width * component.BraceConnect.SplicePlates.Thickness;
			Reporting.AddLine("Ag = Wp * t = " + component.BraceConnect.SplicePlates.Width + " * " + component.BraceConnect.SplicePlates.Thickness + " = " + Ag + ConstUnit.Area);
			FiRn = ConstNum.FIOMEGA0_9N * 2 * component.BraceConnect.SplicePlates.Material.Fy * Ag;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * 2 * Fy * Ag");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 2 *  " + component.BraceConnect.SplicePlates.Material.Fy + " * " + Ag);
			if (FiRn >= component.BraceConnect.Brace.WebForceTension)
				Reporting.AddLine("= " + FiRn + " >= " + component.BraceConnect.Brace.WebForceTension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + component.BraceConnect.Brace.WebForceTension + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Tension Rupture of Plates:");
			HoleSize = Math.Max(component.BraceConnect.SplicePlates.HoleTransB, component.BraceConnect.SplicePlates.HoleTransG);
			An = Ag - component.BraceConnect.SplicePlates.Bolt.NumberOfLines * (HoleSize + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.SplicePlates.Thickness;
			Reporting.AddLine("An = Ag - Nw * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tp");
			Reporting.AddLine("= " + Ag + " - " + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " * (" + HoleSize + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.BraceConnect.SplicePlates.Thickness);
			Reporting.AddLine("= " + An + ConstUnit.Area);
			Ae = Math.Min(0.85 * Ag, An);

			Reporting.AddLine("Ae = Min(0.85 * Ag, An) = " + Ae + ConstUnit.Area);
			FiRn = ConstNum.FIOMEGA0_75N * 2 * component.BraceConnect.SplicePlates.Material.Fu * Ae;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 2 * Fu * Ae");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 2 * " + component.BraceConnect.SplicePlates.Material.Fu + " * " + Ae);
			if (FiRn >= component.BraceConnect.Brace.WebForceTension)
				Reporting.AddLine("= " + FiRn + " >= " + component.BraceConnect.Brace.WebForceTension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + component.BraceConnect.Brace.WebForceTension + ConstUnit.Force + " (NG)");

			if (component.BraceConnect.SplicePlates.Bolt.NumberOfLines > 1)
			{
				Reporting.AddHeader("Block Shear Rupture of Plates:");
				AgvB = 2 * ((component.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) * component.BraceConnect.SplicePlates.Bolt.SpacingLongDir + component.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir) * component.BraceConnect.SplicePlates.Thickness;
				AnvB = AgvB - 2 * (component.BraceConnect.SplicePlates.Bolt.NumberOfRows - 0.5) * (component.BraceConnect.SplicePlates.HoleLongB + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.SplicePlates.Thickness;
				AgtbIn = (component.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir * component.BraceConnect.SplicePlates.Thickness;
				AntbIn = AgtbIn - (component.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * (component.BraceConnect.SplicePlates.HoleTransB + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.SplicePlates.Thickness;
				if (component.BraceConnect.SplicePlates.Bolt.NumberOfLines % 2 == 0)
				{
					AgtbOut = (component.BraceConnect.SplicePlates.Width - component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir) * component.BraceConnect.SplicePlates.Thickness;
					AntbOut = AgtbOut - (component.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * (component.BraceConnect.SplicePlates.HoleTransB + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.SplicePlates.Thickness;
					AgtB = Math.Min(AgtbIn, AgtbOut);
					AntB = Math.Min(AntbIn, AntbOut);
				}
				else
				{
					AgtB = AgtbIn;
					AntB = AntbIn;
				}
				if (AntB >= 0.6 * AnvB)
					FiRnB = 2 * ConstNum.FIOMEGA0_75N * (0.6 * Math.Min(component.BraceConnect.SplicePlates.Material.Fy * AgvB, component.BraceConnect.SplicePlates.Material.Fu * AnvB) + 1 * component.BraceConnect.SplicePlates.Material.Fu * AntB);
				else
					FiRnB = 2 * ConstNum.FIOMEGA0_75N * (0.6 * component.BraceConnect.SplicePlates.Material.Fu * AnvB + component.BraceConnect.SplicePlates.Material.Fy * AgtB) * component.BraceConnect.SplicePlates.Material.Fu * AntB;

				AgvG = 2 * ((component.BraceConnect.SplicePlates.Bolt.NumberOfRows - 1) * component.BraceConnect.SplicePlates.Bolt.SpacingLongDir + component.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir) * component.BraceConnect.SplicePlates.Thickness;
				AnvG = AgvG - 2 * (component.BraceConnect.SplicePlates.Bolt.NumberOfRows - 0.5) * (component.BraceConnect.SplicePlates.HoleLongG + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.SplicePlates.Thickness;
				AgtGIn = (component.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir * component.BraceConnect.SplicePlates.Thickness;
				AntGIn = AgtGIn - (component.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * (component.BraceConnect.SplicePlates.HoleTransG + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.SplicePlates.Thickness;
				if (component.BraceConnect.SplicePlates.Bolt.NumberOfLines % 2 == 0)
				{
					AgtGOut = (component.BraceConnect.SplicePlates.Width - component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir) * component.BraceConnect.SplicePlates.Thickness;
					AntGOut = AgtGOut - (component.BraceConnect.SplicePlates.Bolt.NumberOfLines - 1) * (component.BraceConnect.SplicePlates.HoleTransG + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.SplicePlates.Thickness;
					AgtG = Math.Min(AgtGIn, AgtGOut);
					AntG = Math.Min(AntGIn, AntGOut);
				}
				else
				{
					AgtG = AgtGIn;
					AntG = AntGIn;
				}
				if (AntG >= 0.6 * AnvG)
					FiRnG = 2 * ConstNum.FIOMEGA0_75N * (0.6 * Math.Min(component.BraceConnect.SplicePlates.Material.Fy * AgvG, component.BraceConnect.SplicePlates.Material.Fu * AnvG) + 1 * component.BraceConnect.SplicePlates.Material.Fu * AntG);
				else
					FiRnG = 2 * ConstNum.FIOMEGA0_75N * (0.6 * component.BraceConnect.SplicePlates.Material.Fu * AnvG + component.BraceConnect.SplicePlates.Material.Fy * AgtG * component.BraceConnect.SplicePlates.Material.Fu * AntG);

				if (FiRnB <= FiRnG)
				{
					Reporting.AddLine("Agv = 2 * ((Nl - 1) * s + el) * tp");
					Reporting.AddLine("= 2 * ((" + component.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 1) * " + component.BraceConnect.SplicePlates.Bolt.SpacingLongDir + " + " + component.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir + ") * " + component.BraceConnect.SplicePlates.Thickness);
					Reporting.AddLine("= " + AgvB + ConstUnit.Area);

					Reporting.AddLine("Anv = Agv - 2 * (Nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tp");
					Reporting.AddLine("= " + AgvB + " - 2 * (" + component.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 0.5) * (" + component.BraceConnect.SplicePlates.HoleLongB + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.BraceConnect.SplicePlates.Thickness);
					Reporting.AddLine("= " + AnvB + ConstUnit.Area);

					Reporting.AddLine("Agt = (Nw - 1) * st * tp (Inside Block)");
					Reporting.AddLine("= (" + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * " + component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir + " * " + component.BraceConnect.SplicePlates.Thickness);
					Reporting.AddLine("= " + AgtbIn + ConstUnit.Area);

					Reporting.AddLine("Ant = Agt - (Nw - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tp (Inside Block)");
					Reporting.AddLine("= " + AgtbIn + " - (" + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * (" + component.BraceConnect.SplicePlates.HoleTransB + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.BraceConnect.SplicePlates.Thickness);
					Reporting.AddLine("= " + AntbIn + ConstUnit.Area);
					if (component.BraceConnect.SplicePlates.Bolt.NumberOfLines % 2 == 0)
					{
						Reporting.AddLine("Agto = (W - st) * tp (Outside Blocks)");
						Reporting.AddLine("= (" + component.BraceConnect.SplicePlates.Width + " - " + component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir + ") * " + component.BraceConnect.SplicePlates.Thickness);
						Reporting.AddLine("= " + AgtbOut + ConstUnit.Area);

						Reporting.AddLine("Anto = Agto - (Nw - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tp (Outside Blocks)");
						Reporting.AddLine("= " + AgtbOut + " - (" + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * (" + component.BraceConnect.SplicePlates.HoleTransB + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.BraceConnect.SplicePlates.Thickness);
						Reporting.AddLine("= " + AntbOut + ConstUnit.Area);

						AgtB = Math.Min(AgtbIn, AgtbOut);
						AntB = Math.Min(AntbIn, AntbOut);
						Reporting.AddLine("Agt = Min(Agt, Agto)= " + AgtB + ConstUnit.Area);
						Reporting.AddLine("Ant = Min(Ant, Anto)= " + AntB + ConstUnit.Area);
					}
					else
					{
						AgtB = AgtbIn;
						AntB = AntbIn;
					}
					FiRn = SmallMethodsDesign.BlockShearPrint(AgvB, AnvB, AgtB, AntB, component.BraceConnect.SplicePlates.Material.Fy, component.BraceConnect.SplicePlates.Material.Fu, true);
				}
				else
				{
					Reporting.AddLine("Agv = 2 * ((Nl - 1) * s + el) * tp");
					Reporting.AddLine("= 2 * ((" + component.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 1) * " + component.BraceConnect.SplicePlates.Bolt.SpacingLongDir + " + " + component.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir + ")*" + component.BraceConnect.SplicePlates.Thickness);
					Reporting.AddLine("= " + AgvG + ConstUnit.Area);

					Reporting.AddLine("Anv = Agv - 2 * (Nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tp");
					Reporting.AddLine("= " + AgvG + " - 2 * (" + component.BraceConnect.SplicePlates.Bolt.NumberOfRows + " - 0.5) * (" + component.BraceConnect.SplicePlates.HoleLongG + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.BraceConnect.SplicePlates.Thickness);
					Reporting.AddLine("= " + AnvG + ConstUnit.Area);

					Reporting.AddLine("Agt = (Nw - 1) * st * tp (Inside Block)");
					Reporting.AddLine("= (" + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * " + component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir + " * " + component.BraceConnect.SplicePlates.Thickness);
					Reporting.AddLine("= " + AgtGIn + ConstUnit.Area);

					Reporting.AddLine("Ant = Agt - (Nw - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tp (Inside Block)");
					Reporting.AddLine("= " + AgtGIn + " - (" + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * (" + component.BraceConnect.SplicePlates.HoleTransG + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.BraceConnect.SplicePlates.Thickness);
					Reporting.AddLine("= " + AntGIn + ConstUnit.Area);
					if (component.BraceConnect.SplicePlates.Bolt.NumberOfLines % 2 == 0)
					{
						Reporting.AddLine("Agto = (W - st) * tp (Outside Blocks)");
						Reporting.AddLine("= (" + component.BraceConnect.SplicePlates.Width + " - " + component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir + ") * " + component.BraceConnect.SplicePlates.Thickness);
						Reporting.AddLine("= " + AgtGOut + ConstUnit.Area);

						Reporting.AddLine("Anto = Agto - (Nw - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tp (Outside Blocks)");
						Reporting.AddLine("= " + AgtGOut + " - (" + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " - 1) * (" + component.BraceConnect.SplicePlates.HoleTransG + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.BraceConnect.SplicePlates.Thickness);
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
					FiRn = SmallMethodsDesign.BlockShearPrint(AgvG, AnvG, AgtG, AntG, component.BraceConnect.SplicePlates.Material.Fy, component.BraceConnect.SplicePlates.Material.Fu, true);
				}

				if (FiRn >= component.BraceConnect.Brace.WebForceTension / 2)
					Reporting.AddLine("= " + FiRn + " >= " + (component.BraceConnect.Brace.WebForceTension / 2) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + FiRn + " << " + (component.BraceConnect.Brace.WebForceTension / 2) + ConstUnit.Force + " (NG)");
			}
		}
	}
}