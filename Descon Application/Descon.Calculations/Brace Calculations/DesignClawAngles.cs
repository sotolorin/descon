using System;
using System.Linq;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class DesignClawAngles
	{
		public static void CalcDesignClawAngles(EMemberType memberType)
		{
			bool checkGage = false;

			double RnG = 0;
			double AnvG = 0;
			double AgvG = 0;
			double AntG = 0;
			double AgtG = 0;
			double RnB = 0;
			double AnvB = 0;
			double AgvB = 0;
			double AntB = 0;
			double AgtB = 0;
			double FiRn = 0;
			double tmin = 0;
			double tReqG = 0;
			double Lnvg = 0;
			double Lgvg = 0;
			double Lntg = 0;
			double Lgtg = 0;
			double Ga = 0;
			double tReqB = 0;
			double Rn = 0;
			double Lnvb = 0;
			double Lgvb = 0;
			double Lntb = 0;
			double Lgtb = 0;
			double AngleGageMax = 0;
			double AngleGageMin = 0;
			double AnglGage2 = 0;
			double AnglGage1 = 0;
			double osl = 0;
			double thick = 0;
			double Ae = 0;
			double U = 0;
			double Xb = 0;
			double YBarForAngle = 0;
			double XBarForAngle = 0;
			double Ag = 0;
			double L = 0;
			double S = 0;
			double t = 0;
			double re = 0;
			double HoleSize = 0;
			double AeRequired = 0;
			double AgRequired = 0;
			double tmin0 = 0;
			double aC = 0;
			double aT = 0;
			double Fbrs = 0;
			double Fbre = 0;
			double hol = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];

			if (component.BraceConnect.ClawAngles.DoNotConnectFlanges)
				return;

			Reporting.AddHeader("Design " + CommonDataStatic.CommonLists.CompleteMemberList[MiscMethods.ComponentMinusOne(memberType)] + " Claw Angles");

			component.BraceConnect.ClawAngles.Length = component.BraceConnect.ClawAngles.LengthG + component.BraceConnect.ClawAngles.LengthB + component.EndSetback;
			hol = component.BraceConnect.ClawAngles.HoleLongB;
			if (hol < component.BraceConnect.ClawAngles.HoleLongG)
				hol = component.BraceConnect.ClawAngles.HoleLongG;

			Fbre = CommonCalculations.EdgeBearing(component.BoltGusset.EdgeDistLongDir, (hol), component.BoltGusset.BoltSize, component.BraceConnect.ClawAngles.Material.Fu, component.BoltGusset.HoleType, false);
			Fbrs = CommonCalculations.SpacingBearing(component.BoltGusset.SpacingLongDir, hol, component.BoltGusset.BoltSize, component.BoltGusset.HoleType, component.BraceConnect.ClawAngles.Material.Fu, false);

			aT = Fbre + Fbrs * (component.BoltGusset.NumberOfBolts - 1);
			aC = Fbrs * component.BoltGusset.NumberOfBolts;
			tmin0 = Math.Max(component.BraceConnect.Brace.FlangeForceTension / (2 * aT), component.AxialCompression / (2 * aC));
			AgRequired = component.PFlange / (2 * ConstNum.FIOMEGA0_9N * component.BraceConnect.ClawAngles.Material.Fy);
			AeRequired = component.PFlange / (2 * ConstNum.FIOMEGA0_75N * component.BraceConnect.ClawAngles.Material.Fu);
			HoleSize = component.BraceConnect.ClawAngles.HoleTransB; // Bolts(ic).d +0.0625
			if (HoleSize < component.BraceConnect.ClawAngles.HoleTransG)
				HoleSize = component.BraceConnect.ClawAngles.HoleTransG;

			re = component.BoltGusset.MinEdgeRolled;

			foreach (var shape in CommonDataStatic.ShapesSingleAngle)
			{
				// This logic is used so if the user chose the Size then we check once and drop out of the loop
				var currentShape = shape.Value;
				if (component.BraceConnect.ClawAngles.Size_User)
					currentShape = CommonDataStatic.AllShapes.First(s => s.Value.Name == component.BraceConnect.ClawAngles.SizeName).Value;

				t = currentShape.t;
				S = Math.Min(currentShape.d, currentShape.b);
				L = Math.Max(currentShape.d, currentShape.b);
				Ag = (L + S - t) * t;
				XBarForAngle = (L * t + (Math.Pow(S, 2) - Math.Pow(t, 2))) / (2 * (S + L - t));
				YBarForAngle = (S * t + (Math.Pow(L, 2) - Math.Pow(t, 2))) / (2 * (L + S - t));
				Xb = Math.Max(XBarForAngle, YBarForAngle);
				U = Math.Min(1 - Xb / ((component.BoltGusset.NumberOfBolts - 1) * component.BoltGusset.SpacingLongDir), 0.9);
				Ae = Math.Min(0.85 * Ag, U * (Ag - (HoleSize + ConstNum.SIXTEENTH_INCH) * t));

				if (component.BraceConnect.Gusset.Thickness >= 4)
					thick = component.Shape.tw;
				else
				{
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
					{
						if (memberType == EMemberType.UpperLeft)
							thick = CommonDataStatic.DetailDataDict[EMemberType.UpperRight].BraceConnect.Gusset.Thickness;
						else if (memberType == EMemberType.LowerLeft)
							thick = CommonDataStatic.DetailDataDict[EMemberType.LowerRight].BraceConnect.Gusset.Thickness;
						else if (memberType == EMemberType.UpperRight)
							thick = CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].BraceConnect.Gusset.Thickness;
						else if (memberType == EMemberType.LowerRight)
							thick = CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].BraceConnect.Gusset.Thickness;
						else
							thick = component.BraceConnect.Gusset.Thickness;
					}
				}

				if (component.BraceConnect.ClawAngles.OSL == EOSL.ShortLeg)
					osl = component.BraceConnect.ClawAngles.ShortLeg;
				else
					osl = component.BraceConnect.ClawAngles.LongLeg;

				AnglGage1 = thick + 2 * (t + component.BoltGusset.BoltSize + 0.5);
				AnglGage2 = thick + 2 * (component.BoltGusset.BoltSize + 0);	// 0 was called fsz in Descon 7
				AngleGageMin = AnglGage1 < AnglGage2 ? AnglGage2 : AnglGage1;

				AngleGageMax = thick + 2 * (osl - re);
				if (component.GageOnFlange < AngleGageMin || component.GageOnFlange > AngleGageMax)
					checkGage = true;

				// AngleBlockShear:
				Lgtb = (S + L - osl - (component.GageOnFlange - component.BraceConnect.Gusset.Thickness) / 2);
				Lntb = Lgtb - 0.5 * (component.BraceConnect.ClawAngles.HoleTransB + ConstNum.SIXTEENTH_INCH);
				Lgvb = (component.BoltGusset.NumberOfBolts - 1) * component.BoltGusset.SpacingLongDir + component.BoltGusset.EdgeDistLongDir;
				Lnvb = Lgvb - (component.BoltGusset.NumberOfBolts - 0.5) * (component.BraceConnect.ClawAngles.HoleLongB + ConstNum.SIXTEENTH_INCH);

				Rn = 0.6 * Math.Min(component.BraceConnect.ClawAngles.Material.Fu * Lnvb, component.BraceConnect.ClawAngles.Material.Fy * Lgvb) + 1 * component.BraceConnect.ClawAngles.Material.Fu * Lntb; // per unit thickness
				tReqB = component.PFlange / (2 * ConstNum.FIOMEGA0_75N * Rn);
				Ga = CommonCalculations.AngleStandardGage(0, osl);
				Lgtg = osl - Ga;
				Lntg = Lgtg - 0.5 * (component.BraceConnect.ClawAngles.HoleTransG + ConstNum.SIXTEENTH_INCH);
				Lgvg = component.BoltGusset.NumberOfBolts * component.BoltGusset.SpacingLongDir + component.BoltGusset.EdgeDistLongDir;
				Lnvg = Lgvg - (component.BoltGusset.NumberOfBolts - 0.5) * (component.BraceConnect.ClawAngles.HoleLongG + ConstNum.SIXTEENTH_INCH);

				Rn = 0.6 * Math.Min(component.BraceConnect.ClawAngles.Material.Fu * Lnvg, component.BraceConnect.ClawAngles.Material.Fy * Lgvg) + 1 * component.BraceConnect.ClawAngles.Material.Fu * Lntg; // per unit thickness
				tReqG = component.PFlange / (2 * ConstNum.FIOMEGA0_75N * Rn);
				tmin = Math.Max(tmin0, Math.Max(tReqB, tReqG));

				if(checkGage && t >= tmin && Ag >= AgRequired && Ae >= AeRequired || component.BraceConnect.ClawAngles.Size_User)
				{
					component.BraceConnect.ClawAngles.SizeName = currentShape.Name;
					break;
				}
			}

			Reporting.AddHeader("Selected Angle = " + component.BraceConnect.ClawAngles.SizeName);
			Reporting.AddLine("Gage on Brace Flange = " + component.GageOnFlange + ConstUnit.Length);
			Reporting.AddLine("Angle Gage on Brace Flange = " + ((component.GageOnFlange - component.BraceConnect.Gusset.Thickness) / 2) + ConstUnit.Length);
			Reporting.AddLine("Angle Gage on Gusset (ga) = " + Ga + ConstUnit.Length);
			Reporting.AddLine("Gage on Gusset = ga + d + ga = " + 2 * Ga + component.Shape.d + ConstUnit.Length);

			Reporting.AddHeader("Claw Angle Length = Lclaw");
			Reporting.AddLine("= 2 * ((Nbf - 1) * s + ea) + eg + ebr + g");
			Reporting.AddLine("= 2 * ((" + component.BoltGusset.NumberOfBolts + " - 1) * " + component.BoltGusset.SpacingLongDir + " + " + component.BoltGusset.EdgeDistLongDir + ") + " + component.BoltGusset.EdgeDistGusset + " + " + component.BoltGusset.EdgeDistBrace + " + " + component.EndSetback);
			Reporting.AddLine("= " + component.BraceConnect.ClawAngles.Length + ConstUnit.Length);

			Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Bearing Strength:");
			hol = component.BraceConnect.ClawAngles.HoleLongB;

			Fbre = CommonCalculations.EdgeBearing(component.BoltGusset.EdgeDistLongDir, hol, component.BoltGusset.BoltSize, component.BraceConnect.ClawAngles.Material.Fu, component.BoltGusset.HoleType, false);
			Fbrs = CommonCalculations.SpacingBearing(component.BoltGusset.SpacingLongDir, hol, component.BoltGusset.BoltSize, component.BoltGusset.HoleType, component.BraceConnect.ClawAngles.Material.Fu, false);

			Reporting.AddHeader("With Tensile Force:");
			FiRn = 2 * (Fbre + Fbrs * (component.BoltGusset.NumberOfBolts - 1)) * component.BraceConnect.ClawAngles.Thickness;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Fbre + Fbrs * (Nbf - 1)) * t");
			Reporting.AddLine("= 2 * (" + Fbre + " + " + Fbrs + " * (" + component.BoltGusset.NumberOfBolts + " - 1)) * " + component.BraceConnect.ClawAngles.Thickness);
			if (FiRn >= component.BraceConnect.Brace.FlangeForceTension)
				Reporting.AddLine("= " + FiRn + " >= " + component.BraceConnect.Brace.FlangeForceTension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + component.BraceConnect.Brace.FlangeForceTension + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("With Compressive Force:");
			FiRn = 2 * Fbrs * component.BoltGusset.NumberOfBolts * component.BraceConnect.ClawAngles.Thickness;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * Fbrs * Nbf * t");
			Reporting.AddLine("= 2 * " + Fbrs + " * " + component.BoltGusset.NumberOfBolts + " * " + component.BraceConnect.ClawAngles.Thickness);
			if (FiRn >= component.BraceConnect.Brace.FlangeForceCompression)
				Reporting.AddLine("= " + FiRn + " >= " + component.BraceConnect.Brace.FlangeForceCompression + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + component.BraceConnect.Brace.FlangeForceCompression + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Tension Yielding of Claw Angles:");
			t = component.BraceConnect.ClawAngles.Thickness;
			S = component.BraceConnect.ClawAngles.ShortLeg;
			L = component.BraceConnect.ClawAngles.LongLeg;
			Ag = (L + S - t) * t;
			FiRn = 2 * ConstNum.FIOMEGA0_9N * component.BraceConnect.ClawAngles.Material.Fy * Ag;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * " + ConstString.FIOMEGA0_9 + " * Fy * Ag");
			Reporting.AddLine("= 2 * " + ConstString.FIOMEGA0_9 + " * " + component.BraceConnect.ClawAngles.Material.Fy + " * " + Ag);
			if (FiRn >= component.BraceConnect.Brace.FlangeForceTension)
				Reporting.AddLine("= " + FiRn + " >= " + component.BraceConnect.Brace.FlangeForceTension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + component.BraceConnect.Brace.FlangeForceTension + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Tension Rupture of Claw Angles:");
			XBarForAngle = (L * t + (Math.Pow(S, 2) - Math.Pow(t, 2))) / (2 * (S + L - t));
			YBarForAngle = (S * t + (Math.Pow(L, 2) - Math.Pow(t, 2))) / (2 * (L + S - t));
			Xb = Math.Max(XBarForAngle, YBarForAngle);
			U = Math.Min(1 - Xb / ((component.BoltGusset.NumberOfBolts - 1) * component.BoltGusset.SpacingLongDir), 0.9);
			Reporting.AddLine("U = Min((1 - x / L), 0.9)");
			Reporting.AddLine("= Min(1 - " + Xb + " / " + (component.BoltGusset.NumberOfBolts - 1) * component.BoltGusset.SpacingLongDir + ", 0.9) = " + U);
			HoleSize = Math.Max(component.BraceConnect.ClawAngles.HoleTransG, component.BraceConnect.ClawAngles.HoleTransB);
			Ae = Math.Min(0.85 * Ag, U * (Ag - (HoleSize + ConstNum.SIXTEENTH_INCH) * t));

			Reporting.AddLine("Ae = Min((0.85 * Ag), U * (Ag - (dh + " + ConstNum.SIXTEENTH_INCH + ") * t))");
			Reporting.AddLine("= Min(" + (0.85 * Ag) + ", " + U + " * (" + Ag + " - " + (HoleSize + ConstNum.SIXTEENTH_INCH) + " * " + t + "))");
			Reporting.AddLine("= " + Ae + ConstUnit.Area);

			FiRn = 2 * ConstNum.FIOMEGA0_75N * component.BraceConnect.ClawAngles.Material.Fu * Ae;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * " + ConstString.FIOMEGA0_75 + " * Fu * Ae = 2 * " + ConstString.FIOMEGA0_75 + " * " + component.BraceConnect.ClawAngles.Material.Fu + " * " + Ae);
			if (FiRn >= component.BraceConnect.Brace.FlangeForceTension)
				Reporting.AddLine("= " + FiRn + " >= " + component.BraceConnect.Brace.FlangeForceTension + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + component.BraceConnect.Brace.FlangeForceTension + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Block Shear Rupture of Claw Angles:");
			AgtB = (S + L - osl - (component.GageOnFlange - component.BraceConnect.Gusset.Thickness) / 2) * component.BraceConnect.ClawAngles.Thickness;
			AntB = AgtB - 0.5 * (component.BraceConnect.ClawAngles.HoleTransB + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.ClawAngles.Thickness;
			AgvB = ((component.BoltGusset.NumberOfBolts - 1) * component.BoltGusset.SpacingLongDir + component.BoltGusset.EdgeDistLongDir) * component.BraceConnect.ClawAngles.Thickness;
			AnvB = AgvB - (component.BoltGusset.NumberOfBolts - 0.5) * (component.BraceConnect.ClawAngles.HoleLongB + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.ClawAngles.Thickness;

			RnB = 0.6 * Math.Min(component.BraceConnect.ClawAngles.Material.Fy * AgvB, component.BraceConnect.ClawAngles.Material.Fu * AnvB) + 1 * component.BraceConnect.ClawAngles.Material.Fu * AntB;

			Ga = CommonCalculations.AngleStandardGage(0, osl);
			AgtG = (osl - Ga) * component.BraceConnect.ClawAngles.Thickness;
			AntG = AgtG - 0.5 * (component.BraceConnect.ClawAngles.HoleTransG + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.ClawAngles.Thickness;
			AgvG = ((component.BoltGusset.NumberOfBolts) * component.BoltGusset.SpacingLongDir + component.BoltGusset.EdgeDistLongDir) * component.BraceConnect.ClawAngles.Thickness;
			AnvG = AgvG - (component.BoltGusset.NumberOfBolts - 0.5) * (component.BraceConnect.ClawAngles.HoleLongG + ConstNum.SIXTEENTH_INCH) * component.BraceConnect.ClawAngles.Thickness;
			RnG = 0.6 * Math.Min(component.BraceConnect.ClawAngles.Material.Fy * AgvG, component.BraceConnect.ClawAngles.Material.Fu * AnvG) + component.BraceConnect.ClawAngles.Material.Fu * AntG;

			if (RnG <= RnB)
			{
				Reporting.AddLine("Agt = et * t = " + (osl - Ga) + " * " + component.BraceConnect.ClawAngles.Thickness + " = " + AgtG + ConstUnit.Area);
				Reporting.AddLine("Ant = Agt - 0.5 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("= " + AgtG + " - 0.5 * (" + component.BraceConnect.ClawAngles.HoleTransG + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.BraceConnect.ClawAngles.Thickness);
				Reporting.AddLine("= " + AntG + ConstUnit.Area);
				
				Reporting.AddLine("Agv = ((N - 1) * s + el) * t");
				Reporting.AddLine("= ((" + component.BoltGusset.NumberOfBolts + " - 1) * " + component.BoltGusset.SpacingLongDir + " + " + component.BoltGusset.EdgeDistLongDir + ") * " + component.BraceConnect.ClawAngles.Thickness);
				Reporting.AddLine("= " + AgvG + ConstUnit.Area);

				Reporting.AddLine("Anv = Agv - (N - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("= " + AgvG + " - (" + component.BoltGusset.NumberOfBolts + " - 0.5) * (" + component.BraceConnect.ClawAngles.HoleLongG + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.BraceConnect.ClawAngles.Thickness);
				Reporting.AddLine("= " + AnvG + ConstUnit.Area);
				FiRn = SmallMethodsDesign.BlockShearPrint(AgvG, AnvG, AgtG, AntG, component.BraceConnect.ClawAngles.Material.Fy, component.BraceConnect.ClawAngles.Material.Fu, true);
			}
			else
			{
				Reporting.AddLine("Agt = et * t = " + (S + L - osl - (component.GageOnFlange - component.BraceConnect.Gusset.Thickness) / 2) + " * " + component.BraceConnect.ClawAngles.Thickness + " = " + AgtB + ConstUnit.Area);
				Reporting.AddLine("Ant = Agt - 0.5 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("= " + AgtB + " - 0.5 * (" + component.BraceConnect.ClawAngles.HoleTransB + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.BraceConnect.ClawAngles.Thickness);
				Reporting.AddLine("= " + AntB + ConstUnit.Area);

				Reporting.AddLine("Agv = ((N - 1) * s + el) * t");
				Reporting.AddLine("= ((" + component.BoltGusset.NumberOfBolts + " - 1) * " + component.BoltGusset.SpacingLongDir + " + " + component.BoltGusset.EdgeDistLongDir + ") * " + component.BraceConnect.ClawAngles.Thickness);
				Reporting.AddLine("= " + AgvB + ConstUnit.Area);

				Reporting.AddLine("Anv = Agv - (N - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("= " + AgvB + " - (" + component.BoltGusset.NumberOfBolts + " - 0.5) * (" + component.BraceConnect.ClawAngles.HoleLongB + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.BraceConnect.ClawAngles.Thickness);
				Reporting.AddLine("= " + AnvB + ConstUnit.Area);
				FiRn = SmallMethodsDesign.BlockShearPrint(AgvB, AnvB, AgtB, AntB, component.BraceConnect.ClawAngles.Material.Fy, component.BraceConnect.ClawAngles.Material.Fu, true);
			}
			if (FiRn >= component.BraceConnect.Brace.FlangeForceTension / 2)
				Reporting.AddLine("= " + FiRn + " >= " + (component.BraceConnect.Brace.FlangeForceTension / 2) + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + FiRn + " << " + (component.BraceConnect.Brace.FlangeForceTension / 2) + ConstUnit.Force + " (NG)");
		}
	}
}