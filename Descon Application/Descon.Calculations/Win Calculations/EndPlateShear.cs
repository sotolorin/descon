using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class EndPlateShear
	{
		internal static void DesignEndPlateShear(EMemberType memberType, double P, double V)
		{
			int n = 0;
			int maxbolts = 0;
			int np = 0;
			double reductionfactor;
			double Agv;
			double Agt;
			double Anv;
			double Ant;
			double VnCap;
			double An;
			double VgCap;
			double Ag;
			double Vcap;
			double minsp;
			double spPlate;
			double spSupport;
			double VperBolt;
			double minedge;
			double esh;
			double TensionCap = 0;
			double ta = 0;
			double B = 0;
			double a = 0;
			double Ball = 0;
			double wCap;
			double usefulweldsizeforPL;
			double usefulweldsize;
			double mweld;
			double tt = 0;
			double Tblockshear;
			double Lgv;
			double Lnv;
			double Lnt;
			double Tn;
			double Tg;
			double BearingCap1 = 0;
			double Fbs = 0;
			double Fbe;
			double Anreq;
			double AgReq;
			double Rload;
			double SupThickness1 = 0;
			double SupThickness = 0;
			double t;
			double Plthforweld;
			double columnGageMin;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			var endPlate = beam.WinConnect.ShearEndPlate;

			string com = MiscMethods.GetShortComponentName(memberType) + "_";

			if (!endPlate.BoltDistanceToTopBeam_User)
				endPlate.BoltDistanceToTopBeam = beam.WinConnect.Beam.DistanceToFirstBolt + beam.WinConnect.Beam.TCopeD;

			if (!endPlate.Bolt.EdgeDistLongDir_User)
				endPlate.Bolt.EdgeDistLongDir = endPlate.Bolt.MinEdgeSheared;
			if (!endPlate.Bolt.EdgeDistTransvDir_User)
				endPlate.Bolt.EdgeDistTransvDir = endPlate.Bolt.MinEdgeSheared;

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
			{
				if (beam.MemberType == EMemberType.LeftBeam)
					SupThickness = rightBeam.WinConnect.ShearEndPlate.Thickness;
				else
					SupThickness = leftBeam.WinConnect.ShearEndPlate.Thickness;
			}
			else
				MiscCalculationsWithReporting.EffectiveSupThickness(memberType, ref SupThickness, ref SupThickness1);

            if (!endPlate.Thickness_User) 
                endPlate.Thickness = 0;

			Rload = Math.Sqrt(Math.Pow(P, 2) + Math.Pow(V, 2));

			columnGageMin = GetGageSh(memberType);
			if (endPlate.Bolt.NumberOfLines == 4)
				endPlate.Width = 2 * endPlate.Bolt.EdgeDistTransvDir + beam.GageOnColumn + 2 * endPlate.TrSpacingOut;
			else
				endPlate.Width = 2 * endPlate.Bolt.EdgeDistTransvDir + beam.GageOnColumn;
			
			GetNumberOfBolts(memberType, ref maxbolts, P, V, column.Material.Fu, ref Fbs, SupThickness, ref BearingCap1, ref n, ref np, ref Ball, ref a, ref B, ref tt, ref ta, ref TensionCap);

			AgReq = V / (2 * (ConstNum.FIOMEGA1_0N * 0.6 * endPlate.Material.Fy));
			Anreq = V / (2 * (ConstNum.FIOMEGA0_75N * 0.6 * endPlate.Material.Fu));

			Fbe = CommonCalculations.EdgeBearing(endPlate.Bolt.EdgeDistLongDir, endPlate.Bolt.HoleWidth, endPlate.Bolt.BoltSize, endPlate.Material.Fu, endPlate.Bolt.HoleType, false);
			Fbs = CommonCalculations.SpacingBearing(endPlate.Bolt.SpacingLongDir, endPlate.Bolt.HoleWidth, endPlate.Bolt.BoltSize, endPlate.Bolt.HoleType, endPlate.Material.Fu, false);

			if (CommonDataStatic.JointConfig == EJointConfiguration.BeamSplice)
			{
				if (rightBeam.WinConnect.ShearEndPlate.Bolt.NumberOfBolts > leftBeam.WinConnect.ShearEndPlate.Bolt.NumberOfBolts)
				{
					leftBeam.WinConnect.ShearEndPlate.Bolt.NumberOfRows = rightBeam.WinConnect.ShearEndPlate.Bolt.NumberOfRows;
					leftBeam.WinConnect.ShearEndPlate.Bolt.NumberOfLines = rightBeam.WinConnect.ShearEndPlate.Bolt.NumberOfLines;
				}
				else
				{
					rightBeam.WinConnect.ShearEndPlate.Bolt.NumberOfRows = leftBeam.WinConnect.ShearEndPlate.Bolt.NumberOfRows;
					rightBeam.WinConnect.ShearEndPlate.Bolt.NumberOfLines = leftBeam.WinConnect.ShearEndPlate.Bolt.NumberOfLines;
				}
			}

			do
			{ 
				BearingCap1 = endPlate.Bolt.NumberOfLines * (Fbe + Fbs * (endPlate.Bolt.NumberOfRows - 1));

				endPlate.Length = (endPlate.Bolt.NumberOfRows - 1) * endPlate.Bolt.SpacingLongDir + 2 * endPlate.Bolt.EdgeDistLongDir;
				Tg = AgReq / endPlate.Length;
				Tn = Anreq / (endPlate.Length - endPlate.Bolt.NumberOfRows * (endPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH));
				Lnt = endPlate.Bolt.EdgeDistTransvDir - (endPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) / 2;
				Lnv = endPlate.Length - endPlate.Bolt.EdgeDistLongDir - (endPlate.Bolt.NumberOfRows - 0.5) * (endPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
				Lgv = endPlate.Length - endPlate.Bolt.EdgeDistLongDir;

				Tblockshear = V / (2 * MiscCalculationsWithReporting.BlockShearNew(endPlate.Material.Fu, Lnv, 1, Lnt, Lgv, endPlate.Material.Fy, 1, 0, false));

				t = Math.Max(tt, Math.Max(Math.Max(Math.Max(Tg, Tn), Math.Max(Tblockshear, V / BearingCap1)), ConstNum.QUARTER_INCH));

				np = endPlate.Bolt.NumberOfRows * endPlate.Bolt.NumberOfLines;
				if (t > ConstNum.THREE_EIGHTS_INCH && np < maxbolts)
					if (!endPlate.Bolt.NumberOfRows_User)
						endPlate.Bolt.NumberOfRows++;
				n = endPlate.Bolt.NumberOfRows * endPlate.Bolt.NumberOfLines;

				if (t > ConstNum.FIVE_EIGHTS_INCH)
					Reporting.AddLine("Plate thickness exceeds " + ConstNum.FIVE_EIGHTS_INCH + ConstUnit.Length + ". (NG)");
                if (!endPlate.Thickness_User) 
                    endPlate.Thickness = NumberFun.Round(t, 16);

				if (!beam.EndSetback_User)
					beam.EndSetback = endPlate.Thickness;

				mweld = CommonCalculations.MinimumWeld(endPlate.Thickness, beam.Shape.tw);
				endPlate.Length = Math.Min(endPlate.Length, beam.WinConnect.Beam.WebHeight);
				usefulweldsize = beam.Material.Fu * beam.Shape.tw / (1.414 * endPlate.Weld.Fexx);
				usefulweldsizeforPL = endPlate.Material.Fu * endPlate.Thickness / (0.707 * endPlate.Weld.Fexx);
				usefulweldsize = Math.Min(usefulweldsize, usefulweldsizeforPL);

				double weldSize = ConstNum.SIXTEENTH_INCH;

				if (endPlate.WeldSize_User)
				{
					weldSize = endPlate.WeldSize;
					endPlate.Length = Math.Min(endPlate.Length, beam.WinConnect.Beam.WebHeight);
					wCap = 2 * (endPlate.Length - 2 * weldSize) * weldSize * (ConstNum.FIOMEGA0_75N * 0.6 * 0.707) * endPlate.Weld.Fexx;
					reductionfactor = usefulweldsize / weldSize;
					if (reductionfactor < 1)
						wCap *= reductionfactor;
					else
						reductionfactor = 1;
				}
				else
				{
					do
					{
						if (usefulweldsize <= weldSize)
						{
							if (!endPlate.Bolt.NumberOfRows_User)
								endPlate.Bolt.NumberOfRows++;
							n = endPlate.Bolt.NumberOfRows * endPlate.Bolt.NumberOfLines;
							endPlate.Length = (endPlate.Bolt.NumberOfRows - 1) * endPlate.Bolt.SpacingLongDir + 2 * endPlate.Bolt.EdgeDistLongDir;
						}
						else
							weldSize += ConstNum.SIXTEENTH_INCH;

						endPlate.Length = Math.Min(endPlate.Length, beam.WinConnect.Beam.WebHeight);
						wCap = 2 * (endPlate.Length - 2 * weldSize) * weldSize * (ConstNum.FIOMEGA0_75N * 0.6 * 0.707) * endPlate.Weld.Fexx;

						reductionfactor = usefulweldsize / weldSize;

						if (reductionfactor < 1)
							wCap *= reductionfactor;
						else
							reductionfactor = 1;

					} while (wCap < Rload && !endPlate.Bolt.NumberOfRows_User);
				}
				Plthforweld = endPlate.Weld.Fexx * weldSize / (1.414 * endPlate.Material.Fu);

				if (endPlate.Thickness < Plthforweld)
				{
                    if (!endPlate.Thickness_User) 
                        endPlate.Thickness = NumberFun.Round(Plthforweld, 16);
					if (!beam.EndSetback_User)
						beam.EndSetback = endPlate.Thickness;
				}
                if (!endPlate.WeldSize_User) 
                    endPlate.WeldSize = weldSize;
				if (mweld > endPlate.WeldSize && !endPlate.WeldSize_User)
					endPlate.WeldSize = mweld;
			} while (n > np);

			//columnGageMin = GetGageSh(memberType);

			Reporting.AddMainHeader(beam.ComponentName + " - " + beam.ShapeName + " Shear Connection");
			Reporting.AddHeader("Shear Connection Using End Plate: ");
			Reporting.AddLine("Plate (W x L x T): " + com + "endPlate_Width|e|" + ConstUnit.Length + " X " + endPlate.Length + ConstUnit.Length + " X " + endPlate.Thickness + ConstUnit.Length);
			Reporting.AddLine("Plate Material: " + endPlate.Material.Name);
			Reporting.AddLine("Bolts: (" + com + "endPlate_Bolt_NumberOfBolts|e|" + ") " + endPlate.Bolt.BoltName);
			Reporting.AddLine("Bolt Holes on Support: " + endPlate.Bolt.HoleWidthSupport + ConstUnit.Length + " Vert. X " + endPlate.Bolt.HoleLengthSupport + ConstUnit.Length + "  Horiz.");
			Reporting.AddLine("Bolt Holes on Plate: " + endPlate.Bolt.HoleWidth + ConstUnit.Length + " Vert. X " + endPlate.Bolt.HoleLength + ConstUnit.Length + "  Horiz.");
			Reporting.AddLine("Weld: " + CommonCalculations.WeldSize(endPlate.WeldSize) + " " + endPlate.WeldName + " Fillet Welds");

			if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice)
			{
				Reporting.AddLine("Effective Thickness of Support Material: " + SupThickness + ConstUnit.Length);
				MiscCalculationsWithReporting.DetermineBoltAlignment(memberType, true);
			}

			if (endPlate.Thickness > ConstNum.THREE_EIGHTS_INCH)
				Reporting.AddLine("WARNING: Plate thickness exceeds " + ConstNum.THREE_EIGHTS_INCH + ConstUnit.Length);

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && column.Shape.t < endPlate.Width)
				Reporting.AddLine("Plate width >> Column T-dist. = " + column.Shape.t + ConstUnit.Length + " (NG)");
			//columnGageMin = GetGageSh(memberType);

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
			{
				if (beam.GageOnColumn < columnGageMin)
					Reporting.AddLine("Bolt Gage = " + beam.GageOnColumn + " << Minimum Gage = " + columnGageMin + ConstUnit.Length + " (NG)");
			}
			else
			{
				if (beam.GageOnFlange < columnGageMin)
					Reporting.AddLine("Bolt Gage = " + beam.GageOnFlange + " << Minimum Gage = " + columnGageMin + ConstUnit.Length + " (NG)");
			}

			esh = endPlate.Bolt.MinEdgeSheared;
			minedge = MiscCalculationsWithReporting.MinimumEdgeDist(endPlate.Bolt.BoltSize, 0, endPlate.Material.Fu * endPlate.Thickness, esh, (int)endPlate.Bolt.HoleLength, endPlate.Bolt.HoleType);

			if (endPlate.Bolt.EdgeDistTransvDir < minedge)
				Reporting.AddLine("Bolt Edge Dist. (Horiz) = " + endPlate.Bolt.EdgeDistTransvDir + " << Minimum = " + minedge + ConstUnit.Length + " (NG)");

			esh = endPlate.Bolt.MinEdgeSheared;
			VperBolt = beam.ShearForce / endPlate.Bolt.NumberOfBolts;
			minedge = MiscCalculationsWithReporting.MinimumEdgeDist(endPlate.Bolt.BoltSize, VperBolt, endPlate.Material.Fu * endPlate.Thickness, esh, (int) endPlate.Bolt.HoleWidth, endPlate.Bolt.HoleType);
			if (endPlate.Bolt.EdgeDistLongDir < minedge)
				Reporting.AddLine("Bolt Edge Dist. (Vert) = " + endPlate.Bolt.EdgeDistLongDir + " << Minimum = " + minedge + ConstUnit.Length + " (NG)");

			spSupport = MiscCalculationsWithReporting.MinimumSpacing((endPlate.Bolt.BoltSize), VperBolt, column.Material.Fu * SupThickness, endPlate.Bolt.HoleWidth, endPlate.Bolt.HoleType);
			spPlate = MiscCalculationsWithReporting.MinimumSpacing((endPlate.Bolt.BoltSize), VperBolt, endPlate.Material.Fu * endPlate.Thickness, endPlate.Bolt.HoleWidth, endPlate.Bolt.HoleType);
			minsp = Math.Max(spSupport, spPlate);
			if (endPlate.Bolt.SpacingLongDir < minsp)
				Reporting.AddLine("Bolt Spacing = " + endPlate.Bolt.SpacingLongDir + " << Minimum = " + minsp + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Loading:");
			Reporting.AddLine("Vertical Shear (V) = " + V + ConstUnit.Force);
			Reporting.AddLine("Axial Load (H) = " + P + ConstUnit.Force);
			Reporting.AddLine("Resultant (R) = (V² + H²)^0.5 = ((" + V + " )² + " + P + "²)^0.5 = " + Rload + ConstUnit.Force);

			Reporting.AddGoToHeader("Bolt Capacity:");
			Reporting.AddGoToHeader(ConstString.DES_OR_ALLOWABLE + " Shear Strength of Bolts:");
			Vcap = endPlate.Bolt.NumberOfBolts * endPlate.Bolt.BoltStrength;
			if (Vcap >= V)
				Reporting.AddCapacityLine(ConstString.PHI + " Rn = n * Fv = " + com + "endPlate_Bolt_NumberOfBolts|e|" + " * " + endPlate.Bolt.BoltStrength + " = " + Vcap + " >= " + V + ConstUnit.Force + " (OK)", V / Vcap, "Shear Strength of Bolts", memberType);
			else
				Reporting.AddCapacityLine(ConstString.PHI + " Rn = n * Fv = " + com + "endPlate_Bolt_NumberOfBolts|e|" + " * " + endPlate.Bolt.BoltStrength + " = " + Vcap + " << " + V + ConstUnit.Force + " (NG)", V / Vcap, "Shear Strength of Bolts", memberType);

			if (P != 0)
			{
				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Tension Strength:");
				Ball = BoltsForTension.CalcBoltsForTension(memberType, endPlate.Bolt, P, V, endPlate.Bolt.NumberOfBolts, true);
				a = endPlate.Bolt.EdgeDistTransvDir;
				B = (endPlate.Width - beam.Shape.tw) / 2 - a;
				ta = MiscCalculationsWithReporting.HangerStrength(beam.MemberType, endPlate.Bolt.HoleWidth, endPlate.Material.Fu, endPlate.Thickness, a, B, Ball, 0, true);
				TensionCap = endPlate.Bolt.NumberOfBolts * ta;
				if (TensionCap >= P)
					Reporting.AddCapacityLine(ConstString.PHI + " Rn = n * Ta = " + com + "endPlate.Bolt.NumberOfBolts|e|" + " * " + ta + " = " + TensionCap + " >= " + P + ConstUnit.Force + " (OK)", P / TensionCap, "Tension Strength", memberType);
				else
					Reporting.AddCapacityLine(ConstString.PHI + " Rn = n * Ta = " + com + "endPlate.Bolt.NumberOfBolts|e|" + " * " + ta + " = " + TensionCap + " << " + P + ConstUnit.Force + " (NG)", P / TensionCap, "Tension Strength", memberType);
			}

			Reporting.AddGoToHeader("End Plate " + ConstString.DES_OR_ALLOWABLE + " Shear Strength");
			Reporting.AddHeader("End Plate Shear Capacity:");
			Ag = endPlate.Length * endPlate.Thickness;
			VgCap = 2 * Ag * ConstNum.FIOMEGA1_0N * 0.6 * endPlate.Material.Fy;
			Reporting.AddLine("Gross Area (Ag) = L * t = " + endPlate.Length + " * " + endPlate.Thickness + " = " + Ag + ConstUnit.Area);
			Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = 2 * Ag * " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy ");
			Reporting.AddLine("= 2 * " + Ag + " * " + ConstString.FIOMEGA1_0 + " * 0.6 * " + endPlate.Material.Fy);
			if (VgCap >= V)
				Reporting.AddCapacityLine("= " + VgCap + " >= " + V + ConstUnit.Force + " (OK)", V / VgCap, "End Plate Shear Capacity", memberType);
			else
				Reporting.AddCapacityLine("= " + VgCap + " << " + V + ConstUnit.Force + " (NG)", V / VgCap, "End Plate Shear Capacity", memberType);

			An = (endPlate.Length - endPlate.Bolt.NumberOfRows * (endPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * endPlate.Thickness;
			VnCap = 2 * An * ConstNum.FIOMEGA0_75N * 0.6 * endPlate.Material.Fu;
			Reporting.AddLine(string.Empty);
			Reporting.AddLine("Net Area (An) = (L - n * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t = (" + endPlate.Length + " - " + endPlate.Bolt.NumberOfRows + " * " + endPlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ") * " + endPlate.Thickness + " = " + An + ConstUnit.Area);
			Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = 2 * An * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu ");
			Reporting.AddLine("= 2 * " + An + " * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + endPlate.Material.Fu);
			if (VnCap >= V)
				Reporting.AddCapacityLine("= " + VnCap + " >= " + V + ConstUnit.Force + " (OK)", V / VnCap, "End Plate Net Area Capacity", memberType);
			else
				Reporting.AddCapacityLine("= " + VnCap + " << " + V + ConstUnit.Force + " (NG)", V / VnCap, "End Plate Net Area Capacity", memberType);

			Reporting.AddHeader("Block Shear " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
			Ant = (endPlate.Bolt.EdgeDistTransvDir - (endPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) / 2) * endPlate.Thickness;
			Anv = (endPlate.Length - endPlate.Bolt.EdgeDistLongDir - (endPlate.Bolt.NumberOfRows - 0.5) * (endPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * endPlate.Thickness;
			Agt = endPlate.Bolt.EdgeDistTransvDir * endPlate.Thickness;
			Agv = (endPlate.Length - endPlate.Bolt.EdgeDistLongDir) * endPlate.Thickness;

			Reporting.AddLine(string.Empty);
			Reporting.AddLine("Net Area with Tension Resistance (Ant)");
			Reporting.AddLine("= (Lh - (dh + " + ConstNum.SIXTEENTH_INCH + ") / 2) * t");
			Reporting.AddLine("= (" + endPlate.Bolt.EdgeDistTransvDir + " - (" + endPlate.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ") / 2) * " + endPlate.Thickness);
			Reporting.AddLine("= " + Ant + ConstUnit.Area);

			Reporting.AddLine(string.Empty);
			Reporting.AddLine("Net Area with Shear Resistance (Anv)");
			Reporting.AddLine("= (L - Lv - (N - 0.5) * (dv + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + endPlate.Length + " - " + endPlate.Bolt.EdgeDistLongDir + " - (" + endPlate.Bolt.NumberOfRows + " - 0.5) * (" + endPlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + endPlate.Thickness);
			Reporting.AddLine("= " + Anv + ConstUnit.Area);

			Reporting.AddLine(string.Empty);
			Reporting.AddLine("Gross Area with Tension Resistance (Agt)");
			Reporting.AddLine("= Lh * t = " + endPlate.Bolt.EdgeDistTransvDir + " * " + endPlate.Thickness + " = " + Agt + ConstUnit.Area);

			Reporting.AddLine(string.Empty);
			Reporting.AddLine("Gross Area with Shear Resistance (Agv)");
			Reporting.AddLine("= (L - Lv) * t = (" + endPlate.Length + " - " + endPlate.Bolt.EdgeDistLongDir + ") * " + endPlate.Thickness + " = " + Agv + ConstUnit.Area);

			Reporting.AddLine(string.Empty);
			MiscCalculationsWithReporting.BlockShearNew(endPlate.Material.Fu, Anv, 1, Ant, Agv, endPlate.Material.Fy, 0, V / 2, true);

			Reporting.AddGoToHeader("Bolt Bearing on End Plate:");
			Fbe = CommonCalculations.EdgeBearing(endPlate.Bolt.EdgeDistLongDir, endPlate.Bolt.HoleWidth, endPlate.Bolt.BoltSize, endPlate.Material.Fu, endPlate.Bolt.HoleType, true);
			Fbs = CommonCalculations.SpacingBearing(endPlate.Bolt.SpacingLongDir, endPlate.Bolt.HoleWidth, endPlate.Bolt.BoltSize, endPlate.Bolt.HoleType, endPlate.Material.Fu, true);
			BearingCap1 = endPlate.Bolt.NumberOfLines * (Fbe + Fbs * (endPlate.Bolt.NumberOfRows - 1)) * endPlate.Thickness;
			Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = nT * (Fbe + Fbs * (n - 1)) * t");
			Reporting.AddLine("= " + endPlate.Bolt.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + endPlate.Bolt.NumberOfRows + " - 1)) * " + endPlate.Thickness);
			if (BearingCap1 >= V)
				Reporting.AddCapacityLine("= " + BearingCap1 + " >= " + V + ConstUnit.Force + " (OK)", V / BearingCap1, "Bolt Bearing on End Plate", memberType);
			else
				Reporting.AddCapacityLine("= " + BearingCap1 + " << " + V + ConstUnit.Force + " (NG)", V / BearingCap1, "Bolt Bearing on End Plate", memberType);
			if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice)
			{
				Reporting.AddGoToHeader("Bolt Bearing on Support:");
				Fbs = CommonCalculations.SpacingBearing(endPlate.Bolt.SpacingLongDir, endPlate.Bolt.HoleWidth, endPlate.Bolt.BoltSize, endPlate.Bolt.HoleType, column.Material.Fu, true);
				BearingCap1 = endPlate.Bolt.NumberOfLines * (Fbs * endPlate.Bolt.NumberOfRows) * SupThickness;
				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = nT * (Fbs * n) * t");
				Reporting.AddLine("= " + endPlate.Bolt.NumberOfLines + " * (" + Fbs + " * " + endPlate.Bolt.NumberOfRows + ") * " + SupThickness);
				if (BearingCap1 >= V)
					Reporting.AddCapacityLine("= " + BearingCap1 + " >= " + V + ConstUnit.Force + " (OK)", V / BearingCap1, "Bolt Bearing on Support", memberType);
				else
					Reporting.AddCapacityLine("= " + BearingCap1 + " << " + V + ConstUnit.Force + " (NG)", V / BearingCap1, "Bolt Bearing on Support", memberType);
			}

			if (!beam.EndSetback_User)
				beam.EndSetback = endPlate.Thickness;

			Reporting.AddGoToHeader("Beam Web to Plate Weld:");
			double mnweld = CommonCalculations.MinimumWeld(beam.Shape.tw, endPlate.Thickness);
			if (endPlate.WeldSize >= mnweld)
				Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(endPlate.WeldSize) + " >= Minimum Weld Size = " + CommonCalculations.WeldSize(mnweld) + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(endPlate.WeldSize) + " << Minimum Weld Size = " + CommonCalculations.WeldSize(mnweld) + ConstUnit.Length + " (NG)");

			Reporting.AddLine("Weld Length (L) = " + endPlate.Length + ConstUnit.Length);
			Reporting.AddLine("Useful Weld Size = Fu * tw / (1.414 * Fexx)");
			Reporting.AddLine("= " + beam.Material.Fu + " * " + beam.Shape.tw + " / (1.414 * " + endPlate.Weld.Fexx + ")");
			Reporting.AddLine("= " + usefulweldsize + ConstUnit.Length);

			if (reductionfactor >= 1)
			{
				Reporting.AddLine("No weld strength reduction required");
				Reporting.AddLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " = 2 * (L - 2 * w) * w * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx ");
				Reporting.AddLine("= 2 * (" + endPlate.Length + " - 2 * " + endPlate.WeldSize + ") * " + endPlate.WeldSize + " * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + endPlate.Weld.Fexx);
			}
			else
			{
				Reporting.AddLine("Reduce weld Strength by " + reductionfactor + " for beam tw");
				Reporting.AddLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " = 2 * (L - 2 * w) * w * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx * rf");
				Reporting.AddLine("= 2 * (" + endPlate.Length + " - 2 * " + endPlate.WeldSize + ") * " + endPlate.WeldSize + " * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + endPlate.Weld.Fexx + " * " + reductionfactor);
			}

			if (wCap >= Rload)
				Reporting.AddCapacityLine("= " + wCap + " >= " + Rload + ConstUnit.Force + " (OK)", Rload / wCap, "Weld", memberType);
			else
				Reporting.AddCapacityLine("= " + wCap + " << " + Rload + ConstUnit.Force + " (NG)", Rload / wCap, "Weld", memberType);

			if (beam.WinConnect.Beam.TopCope || beam.WinConnect.Beam.BottomCope)
				Cope.CopedBeam(beam.MemberType, true);

			Stiff.ColumnLocalStress(memberType);

			com = com.Replace("|b|", string.Empty);
			CommonDataStatic.ReportJavascriptVarList.Add(com + "endPlate_Width", endPlate.Width);
			CommonDataStatic.ReportJavascriptVarList.Add(com + "endPlate_Bolt_NumberOfBolts", endPlate.Bolt.NumberOfBolts);
		}

		private static double GetGageSh(EMemberType memberType)
		{
			double gageCol1;
			double gageCol2;
			double columnGageMax;
			double columnGageMin;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			var endPlate = beam.WinConnect.ShearEndPlate;

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange)
			{
				gageCol1 = 2 * (column.Shape.k1 + endPlate.Bolt.BoltSize);
				gageCol2 = column.Shape.tw + 2 * (endPlate.Bolt.BoltSize + ConstNum.HALF_INCH);
				columnGageMin = Math.Min(gageCol2, gageCol1);
				columnGageMax = column.Shape.bf - 2 * endPlate.Bolt.EdgeDistTransvDir;
				if (beam.GageOnColumn < columnGageMin && beam.GageOnColumn > columnGageMax)
					Reporting.AddLine("Check Column Gage: Should be between " + columnGageMin + " " + ConstUnit.Length + " and " + columnGageMax + " " + ConstUnit.Length + " (NG)");

				columnGageMin = column.Shape.g1;
				columnGageMax = column.Shape.g1;
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
			{
				double C1 = Math.Max(ConstNum.ONEANDFIVE_EIGHTS_INCH * endPlate.Bolt.BoltSize, ConstNum.ONEANDFIVE_EIGHTS_INCH);
				columnGageMin = Math.Max((2 * C1 + beam.Shape.tw), 3.5);
				columnGageMin = NumberFun.Round(columnGageMin, ERoundingPrecision.Half, ERoundingStyle.RoundUp);
				if (columnGageMin > column.Shape.B - 4.5 * column.Shape.tnom - 1.25 * endPlate.Bolt.BoltSize)
					Reporting.AddLine("Bolts do not fit on column (NG)");

				if (!beam.GageOnColumn_User && !beam.GageOnFlange_User)
					beam.GageOnColumn = beam.GageOnFlange = columnGageMin;

				columnGageMax = columnGageMin;
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
			{
				columnGageMin = beam.Shape.tw + 2 * (endPlate.Bolt.BoltSize + ConstNum.HALF_INCH);
				columnGageMax = column.Shape.t - 2 * endPlate.Bolt.EdgeDistTransvDir;
			}
			else
			{
				columnGageMin = beam.Shape.tw + 2 * (endPlate.Bolt.BoltSize + ConstNum.HALF_INCH);
				columnGageMax = 100;
			}

			columnGageMin = NumberFun.Round(columnGageMin, ERoundingPrecision.Fourth, ERoundingStyle.RoundDown);
			columnGageMax = NumberFun.Round(columnGageMax, ERoundingPrecision.Fourth, ERoundingStyle.RoundDown);
			if (columnGageMax < columnGageMin)
				Reporting.AddLine("Minimum bolt gage for tightening is greater then maximum gage for fitting. (NG)");

			return columnGageMin;
		}

		private static void GetNumberOfBolts(EMemberType memberType, ref int maxBolts, double P, double V, double SupFu, ref double Fbs, double SupThickness, ref double BearingCap1, ref int n, ref int np, ref double Ball, ref double a, ref double B, ref double tt, ref double ta, ref double TensionCap )
		{
			int minBolts;
			int Nbearing;
			double accessHoleHeight;
			double maxL;
			double minL;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var endPlate = beam.WinConnect.ShearEndPlate;

			accessHoleHeight = 0.375 + Math.Max(0.75, 0.75 * beam.Shape.tf);
			maxL = (float)(beam.Shape.d - 2 * (beam.Shape.tf + accessHoleHeight));
			minL = beam.WinConnect.Beam.WebHeight / 2;

			maxBolts = (int)(endPlate.Bolt.NumberOfLines * Math.Floor((maxL - 2 * endPlate.Bolt.EdgeDistLongDir) / endPlate.Bolt.SpacingLongDir + 1));
			minBolts = (int)(endPlate.Bolt.NumberOfLines * Math.Floor((minL - 2 * endPlate.Bolt.EdgeDistLongDir) / endPlate.Bolt.SpacingLongDir + 1));
			if(!endPlate.Bolt.NumberOfBolts_User)
				endPlate.Bolt.NumberOfBolts = (int)Math.Max(minBolts, BoltsForTension.CalcBoltsForTension(memberType, endPlate.Bolt, P, V, 0, false));
			// This lines rounds the number up if we ended up with and odd value for Number of Bolts (MT 6/3/15)
			endPlate.Bolt.NumberOfRows = (int)Math.Ceiling(endPlate.Bolt.NumberOfBolts / (double)endPlate.Bolt.NumberOfLines);
			Fbs = CommonCalculations.SpacingBearing(endPlate.Bolt.SpacingLongDir, endPlate.Bolt.HoleWidth, endPlate.Bolt.BoltSize, endPlate.Bolt.HoleType, SupFu, false);
			BearingCap1 = endPlate.Bolt.NumberOfLines * (Fbs * endPlate.Bolt.NumberOfRows) * SupThickness;
			Nbearing = (int)(Math.Ceiling(endPlate.Bolt.NumberOfLines * V / (Fbs * SupThickness) / endPlate.Bolt.NumberOfLines));

			n = Math.Max(endPlate.Bolt.NumberOfBolts, Nbearing);
			if (n > maxBolts)
			{
				n = maxBolts;
				endPlate.Bolt.NumberOfBolts = n;
				Reporting.AddLine("Too many bolts. Number of bolts set to: " + n);
			}
			do
			{
				np = n;
				Ball = BoltsForTension.CalcBoltsForTension(memberType, endPlate.Bolt, P, V, n, false);
                if (!endPlate.Bolt.NumberOfRows_User) 
                    endPlate.Bolt.NumberOfRows = n / endPlate.Bolt.NumberOfLines;
				endPlate.Bolt.NumberOfBolts = n;
				a = endPlate.Bolt.EdgeDistTransvDir;
				B = (endPlate.Width - beam.Shape.tw) / 2 - a;
				tt = ConstNum.SIXTEENTH_INCH;
				do
				{
					tt += ConstNum.SIXTEENTH_INCH;
					endPlate.Bolt.NumberOfBolts = endPlate.Bolt.NumberOfBolts / endPlate.Bolt.NumberOfLines;
					ta = MiscCalculationsWithReporting.HangerStrength(beam.MemberType, endPlate.Bolt.HoleWidth, endPlate.Material.Fu, tt, a, B, Ball, 0, false);
					endPlate.Bolt.NumberOfBolts = endPlate.Bolt.NumberOfBolts * endPlate.Bolt.NumberOfLines;
					TensionCap = n * ta;
				} while (TensionCap < P && tt < ConstNum.FIVE_EIGHTS_INCH);
				if (tt > ConstNum.FIVE_EIGHTS_INCH && n < maxBolts)
					n += endPlate.Bolt.NumberOfLines;
			} while (np < n && tt < ConstNum.FIVE_EIGHTS_INCH);
			if (n <= maxBolts)
				endPlate.Bolt.NumberOfBolts = n;
			else
			{
				endPlate.Bolt.NumberOfBolts = maxBolts;
				Reporting.AddLine("Too many bolts. Number of bolts set to: " + maxBolts);
			}
		}
	}
}