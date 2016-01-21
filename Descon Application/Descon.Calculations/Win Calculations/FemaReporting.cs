using System;
using System.Linq;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class FemaReporting
	{
		public static void DesignFemaReporting(EMemberType memberType)
		{
			double tfc_req = 0;
			double Mfail_BeamFlange = 0;
			double Mfail_bolttension = 0;
			double Mfail_tflange = 0;
			double ap = 0;
			double g1 = 0;
			double Mfail_stemfracture = 0;
			double w2 = 0;
			double w1 = 0;
			double Thetaeff = 0;
			double L_tee = 0;
			double tpMin2 = 0;
			double tpMin1 = 0;
			double Mf_Max = 0;
			double Tub = 0;
			double Tub_min = 0;
			double Tb = 0;
			double ts = 0;
			double dbt = 0;
			double Ffu = 0;
			double Pb = 0;
			double wmax_beamweb = 0;
			double minweld = 0;
			double w_web = 0;
			double tpS = 0;
			double tpM = 0;
			double pt = 0;
			double db = 0;
			double Fyp = 0;
			double s = 0;
			double g = 0;
			double BoltDiam = 0;
			double Ab = 0;
			double AboltShear = 0;
			double Abolt = 0;
			double d1 = 0;
			double d0 = 0;
			double Plt6 = 0;
			double Maxweld2 = 0;
			double MaxWeld1 = 0;
			double Coeff = 0;
			double ex = 0;
			double k2 = 0;
			double k1 = 0;
			double k = 0;
			double Plt5 = 0;
			double Vmax = 0;
			double Plt4 = 0;
			double Teq = 0;
			double Plt3 = 0;
			double Plt2 = 0;
			double Plt1 = 0;
			double w_Column_req = 0;
			double C1 = 0;
			double Cnst = 0;
			double theta = 0;
			double wL_eff = 0;
			double Tst = 0;
			double Vst = 0;
			double Lff = 0;
			int columnd = 0;
			bool BeamFlangeSlenderness = false;
			double tpz = 0;
			double wmax = 0;
			double LW = 0;
			double tp = 0;
			double Vf = 0;
			double Rmp = 0;
			double tf = 0;
			double Srbs = 0;
			double Zrbs = 0;
			double radius = 0;
			double B = 0;
			double a = 0;
			double Mfail = 0;
			double Tn = 0;
			double Mfail_bm = 0;
			double Ltf3 = 0;
			double Mfail_fp = 0;
			double Ltf2 = 0;
			double Mfail_Bolts = 0;
			double Ltf1 = 0;
			double S3 = 0;
			double S4 = 0;
			double s2 = 0;
			double s1 = 0;
			double n = 0;
			double tpl = 0;
			double Vweb = 0;
			double shf = 0;
			double c = 0;
			double Ae = 0;
			double An = 0;
			double Ag = 0;
			double Bp = 0;
			double Ryc = 0;
			double h = 0;
			double w = 0;
			double Length;
			double Ze;
			double Se;
			double sh;
			double L1;
			double Vg;
			double Vp;
			double Lp_h;
			double Lpl;
			double pf;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var femaData = beam.WinConnect.Fema;

			// Section 3.7.4
			if (CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.OMF || CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.SMF)
				return;

			PrintFema1(memberType);

			switch (femaData.Connection)
			{
				case EFemaConnectionType.BFP:
					Reporting.AddHeader("Flange Plates and Bolts:");
					Reporting.AddLine("Flange Plates: Two PL " + beam.WinConnect.MomentFlangePlate.TopLength + " X " + beam.WinConnect.MomentFlangePlate.TopWidth + " X " + beam.WinConnect.MomentFlangePlate.TopThickness + ConstUnit.Length);
					Reporting.AddLine("Bolts:  (" + (beam.WinConnect.MomentFlangePlate.Bolt.NumberOfRows * 4) + " Total) " + beam.WinConnect.MomentFlangePlate.Bolt.BoltName);
					if (beam.WinConnect.MomentFlangePlate.Bolt.BoltSize <= ConstNum.NINE_EIGHTS)
						Reporting.AddLine("Bolt diameter <= " + ConstNum.NINE_EIGHTS + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Bolt diameter >> " + ConstNum.NINE_EIGHTS + ConstUnit.Length + " (NG)");
					if (beam.WinConnect.MomentFlangePlate.Bolt.BoltType == EBoltType.X)
						Reporting.AddLine("Bolt threads excluded from shear plane (OK)");
					else
						Reporting.AddLine("Bolt threads included in shear plane (NG)");

					Length = beam.WinConnect.MomentFlangePlate.TopLength;
					Bp = beam.WinConnect.MomentFlangePlate.TopWidth;

					Reporting.AddHeader("FEMA 350 Design Calculations:");
					Ag = beam.Shape.tf * beam.Shape.bf;

					Reporting.AddHeader("Effective Plastic Modulus of Beam (Ze)");
					Reporting.AddLine("Ag = tf * bf = " + beam.Shape.tf + " * " + beam.Shape.bf + " = " + Ag + ConstUnit.Area);
					An = beam.Shape.tf * (beam.Shape.bf - beam.WinConnect.MomentFlangePlate.Bolt.NumberOfLines * (beam.WinConnect.MomentFlangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH));
					Reporting.AddLine("An = tf * (bf - Nt * (dh + " + ConstNum.SIXTEENTH_INCH + ")) = " + beam.Shape.tf + " *(" + beam.Shape.bf + " -(" + beam.WinConnect.MomentFlangePlate.Bolt.NumberOfLines + " * (" + beam.WinConnect.MomentFlangePlate.Bolt.HoleWidth + " + .0625)) = " + An + ConstUnit.Area);
					Reporting.AddLine("Ae = 0.75 * Fu * An / (0.9 * Fy) <= Ag");
					Ae = 0.75 * beam.Material.Fu * An / (0.9 * beam.Material.Fy);
					Reporting.AddLine("= 0.75 * " + beam.Material.Fu + " * " + An + " / (0.9 * " + beam.Material.Fy + ")");
					if (Ae >= Ag)
					{
						Ae = Ag;
						Reporting.AddLine("= " + Ae + "		 " + Ag + "  Use:  Ae = " + Ag + ConstUnit.Area);
					}
					else
						Reporting.AddLine("= " + Ae + ConstUnit.Area);

					Ze = beam.Shape.zx - (Ag - Ae) * (beam.Shape.d - beam.Shape.tf);
					Reporting.AddHeader("Ze = Zb -  (Ag - Ae) * (db - tf)");
					Reporting.AddLine("= " + beam.Shape.zx + " - (" + Ag + " - " + Ae + ") * (" + beam.Shape.d + " - " + beam.Shape.tf + ")");
					Reporting.AddLine("= " + Ze + ConstUnit.SecMod);
					Reporting.AddHeader("Elastic Section Modulus of Beam, Sb:");
					c = beam.WinConnect.MomentFlangePlate.Bolt.NumberOfLines * (beam.WinConnect.MomentFlangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
					Reporting.AddLine("c = Nl*(dh+" + ConstNum.SIXTEENTH_INCH + ") = " + c + ConstUnit.Length);
					Se = beam.Shape.sx - c * (beam.Shape.tf * beam.Shape.d - 2 * Math.Pow(beam.Shape.tf, 2));
					Reporting.AddHeader("Sb = S - c * (tf * d - 2 * tf²)");
					Reporting.AddLine("= " + beam.Shape.sx + " - " + c + " * (" + beam.Shape.tf + " * " + beam.Shape.d + " - 2 * " + beam.Shape.tf + "²)");
					Reporting.AddLine("= " + Se + ConstUnit.SecMod);

					femaData.Cpr = Fema.FCpr(memberType); //  (beam.Material.Fy + Beam.Fu) / (2 * beam.Material.Fy) 'eq. 3.2"
					femaData.Mpr = Fema.FMpr(memberType); //  Cpr * Ry * Ze * beam.Material.Fy 'eq. 3.1
					femaData.Cy = Fema.FCy(memberType); // Beam.s / (Cpr * Ze) ' eq.3.4
					sh = (column.Shape.d / 2 + Length);
					Reporting.AddLine("Sh = dc/ 2 + Lpl = " + column.Shape.d + " / 2 + " + Length + " = " + sh + ConstUnit.Length);
					if (femaData.shf == -100)
						shf = sh;
					else
						shf = femaData.shf;

					Reporting.AddLine("Shf = " + shf + ConstUnit.Length);
					L1 = (femaData.L - sh - shf); //    =  L'
					Reporting.AddLine("L1 = L- sh - shf = " + femaData.L + " - " + sh + " - " + shf + " = " + L1 + ConstUnit.Length);

					femaData.Pg = femaData.Pg;
					Lp_h = (femaData.Lp - Length);
					Reporting.AddHeader("Dist. from Load P to Hinge = " + Lp_h + ConstUnit.Length);

					femaData.Wg = femaData.Wg;
					Vp = Fema.fVp(memberType); // (2 * Mpr + femaData.Pg* Lp + Wg * L1 ^ 2 / 2) / L1 'fig. 3-3

					femaData.Mc = Fema.fMc(memberType); // Mpr + Vp * sh 'fig. 3-4

					Lpl = Length;
					femaData.Mf = Fema.fMf(memberType); // Mpr + Vp * Length

					femaData.Myf = Fema.fMyf(memberType); //  Cy * Mf 'eq.3.3

					Vg = Fema.fVg(memberType); // Wg * (femaData.L - Column.d) / 2 + femaData.Pg* (femaData.L - Column.d - Lp) / (femaData.L - Column.d)
					Vweb = Fema.fVweb(memberType); // 2 * Mf / (femaData.L - Column.d) + Vg 'Eq. 3-52

					Reporting.AddHeader("Plate Thickness:");
					tpl = (-beam.Shape.d + Math.Pow(Math.Pow(beam.Shape.d, 2) + 4.4F * femaData.Myf / (beam.WinConnect.MomentFlangePlate.Material.Fy * Bp), 0.5)) / 2;
					Reporting.AddLine("tpl = (-db + (db² + 4.4 * Myf / (Fy * bp))^0.5) / 2");
					Reporting.AddLine("= (-" + beam.Shape.d + " + (" + beam.Shape.d + "² + 4.4 * " + femaData.Myf + " / (" + beam.WinConnect.MomentFlangePlate.Material.Fy + " * " + Bp + "))^0.5) / 2 = " + tpl + ConstUnit.Length);
					if (tpl <= beam.WinConnect.MomentFlangePlate.TopThickness)
						Reporting.AddLine("= " + tpl + " <= " + beam.WinConnect.MomentFlangePlate.TopThickness + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + tpl + " >> " + beam.WinConnect.MomentFlangePlate.TopThickness + ConstUnit.Length + " (NG)");

					// Step 6
					n = beam.WinConnect.MomentFlangePlate.Bolt.NumberOfRows;
					s1 = beam.EndSetback + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange;
					s2 = beam.WinConnect.MomentFlangePlate.Bolt.SpacingLongDir;
					S4 = beam.WinConnect.MomentFlangePlate.Bolt.EdgeDistLongDir;
					S3 = (n - 1) * s2;
					Ltf1 = (femaData.L - column.Shape.d) / (femaData.L - column.Shape.d - (2 * s1 + S3)); // eq 3-44
					Reporting.AddLine("L_tf1 = (L - dc) / (femaData.L - dc - (2 * S1 + S3)) = (" + femaData.L + " - " + column.Shape.d + ") / (" + femaData.L + " - " + column.Shape.d + " - (2 * " + s1 + " + " + S3 + ")) = " + Ltf1);
					// step 7
					Mfail_Bolts = 2 * n * beam.WinConnect.MomentFlangePlate.Bolt.BoltStrength * Math.Pow(beam.WinConnect.MomentFlangePlate.Bolt.BoltSize, 2) * Math.PI / 4 * beam.Shape.d * Ltf1; // eq 3-43
					Reporting.AddLine("M_fail_bolts = 2 * N * (Ab * Fv_bolt) * db * Ltf1"); // eq 3-43"
					Reporting.AddLine("= 2 * " + n + " * (" + beam.WinConnect.MomentFlangePlate.Bolt.BoltStrength * Math.Pow(beam.WinConnect.MomentFlangePlate.Bolt.BoltSize, 2) * Math.PI / 4 + ") * " + beam.Shape.d + " * " + Ltf1); // eq 3-43
					if (Mfail_Bolts > 1.2 * femaData.Myf)
						Reporting.AddLine("= " + Mfail_Bolts + " >> 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (OK)");
					else
						Reporting.AddLine("= " + Mfail_Bolts + " <= 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (NG)");

					// Step 8
					Ltf2 = (femaData.L - column.Shape.d) / (femaData.L - column.Shape.d - 2 * s1); // eq 3-46
					Reporting.AddLine("L_tf2 = (L - dc) / (femaData.L - dc - 2 * S1) = (" + femaData.L + " - " + column.Shape.d + ") / (" + femaData.L + " - " + column.Shape.d + " - 2 * " + s1 + ") = " + Ltf2);

					Mfail_fp = 0.85 * beam.WinConnect.MomentFlangePlate.Material.Fu * (Bp - 2 * (beam.WinConnect.MomentFlangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * beam.WinConnect.MomentFlangePlate.TopThickness * (beam.Shape.d + beam.WinConnect.MomentFlangePlate.TopThickness) * Ltf2; // eq 3-45
					Reporting.AddLine("M_fail_fp = 0.85 * Fu * (bp - 2 * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * tp * (db + tp) * Ltf2"); // eq 3-45
					Reporting.AddLine("= 0.85 * " + beam.WinConnect.MomentFlangePlate.Material.Fu + " * (" + Bp + " - 2 * (" + beam.WinConnect.MomentFlangePlate.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + beam.WinConnect.MomentFlangePlate.TopThickness + " * (" + beam.Shape.d + " + " + beam.WinConnect.MomentFlangePlate.TopThickness + ") * " + Ltf2); // eq 3-45
					if (Mfail_fp > 1.2 * femaData.Myf)
						Reporting.AddLine("= " + Mfail_fp + " >> 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (OK)");
					else
						Reporting.AddLine("= " + Mfail_fp + " <= 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (NG)");

					// Step 9
					Ltf3 = (femaData.L - column.Shape.d) / (femaData.L - column.Shape.d - 2 * (s1 + S3)); // eq 3-48
					Reporting.AddLine("L_tf3 = (L - dc) / (femaData.L - dc - 2 * (S1 + S3)) = (" + femaData.L + " - " + column.Shape.d + ") / (" + femaData.L + " - " + column.Shape.d + " - 2 * (" + s1 + " + " + S3 + ")) = " + Ltf3);

					Mfail_bm = beam.Material.Fu * (beam.Shape.zx - 2 * (beam.WinConnect.MomentFlangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * beam.Shape.tf * (beam.Shape.d - beam.Shape.tf)) * Ltf3; // eq 3-47
					Reporting.AddLine("M_fail_bm = Fu * (Zb - 2 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * tf * (db - tf)) * Ltf3"); // eq 3-47
					Reporting.AddLine("= " + beam.Material.Fu + " * (" + beam.Shape.zx + " - 2 * (" + beam.WinConnect.MomentFlangePlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ") * " + beam.Shape.tf + " * (" + beam.Shape.d + " - " + beam.Shape.tf + ")) * " + Ltf3); // eq 3-47
					if (Mfail_bm > 1.2 * femaData.Myf)
						Reporting.AddLine("= " + Mfail_bm + " >> 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (OK)");
					else
						Reporting.AddLine("= " + Mfail_bm + " <= 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (NG)");

					// Step 10
					Tn = Math.Min(2.4F * beam.Material.Fu * (S3 + s1 - c) * beam.Shape.tf, 2.4F * beam.WinConnect.MomentFlangePlate.Material.Fu * (S3 + S4) * beam.WinConnect.MomentFlangePlate.TopThickness); // eq.3-50 3-51
					Reporting.AddLine("Tn = Min(2.4 * Fu * (S3 + S1 - C) * tf; 2.4 * Fu_pl * (S3 + S4) * tp)"); // eq.3-50 3-51
					Reporting.AddLine("= Min(2.4 * " + beam.Material.Fu + " * (" + S3 + " + " + s1 + " - " + c + ") * " + beam.Shape.tf + "; 2.4 * " + beam.WinConnect.MomentFlangePlate.Material.Fu + " * (" + S3 + " + " + S4 + ") * " + beam.WinConnect.MomentFlangePlate.TopThickness + ")"); // eq.3-50 3-51
					Reporting.AddLine("= " + Tn + ConstUnit.Force);

					Mfail = Tn * (beam.Shape.d + beam.WinConnect.MomentFlangePlate.TopThickness) * Ltf1; // eq 3-49
					Reporting.AddLine("Mfail_bolt_hole = Tn * (db + tp) * Ltf1");
					Reporting.AddLine("= " + Tn + " * (" + beam.Shape.d + " + " + beam.WinConnect.MomentFlangePlate.TopThickness + ") * " + Ltf1); // eq 3-49"
					if (Mfail > 1.2 * femaData.Myf)
						Reporting.AddLine("= " + Mfail + " >> 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (OK)");
					else
						Reporting.AddLine("= " + Mfail + " <= 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (NG)");

					Reporting.AddLine("Plate Force at Column Face (Ff) = 1.2 * Myf / (d + tp)");
					Reporting.AddLine("= 1.2 * " + femaData.Myf + "/(" + beam.Shape.d + "+" + beam.WinConnect.MomentFlangePlate.TopThickness + ") = " + 1.2 * femaData.Myf / (beam.Shape.d + beam.WinConnect.MomentFlangePlate.TopThickness) + ConstUnit.Force);
					break;
				case EFemaConnectionType.RBS:
					Reporting.AddHeader("Beam Flange:");
					Reporting.AddLine("CJP groove weld top and bottom,");
					Reporting.AddLine("with " + NumberFun.ConvertFromFraction(5) + ConstUnit.Length + " fillet welds as shown.");

					Reporting.AddHeader("Beam web:");
					Reporting.AddLine("Shear tab bolted to web, welded to column");
					Reporting.AddLine("Bolts: (" + beam.WinConnect.ShearWebPlate.Bolt.NumberOfBolts + ") " + beam.WinConnect.ShearWebPlate.Bolt.BoltName);

					Reporting.AddHeader("Beam Flange Reduction:");
					a = 0.625 * beam.Shape.bf / 8; // Int(-8 * 0.625 * Beam.BF) / 8
					B = 0.75 * beam.Shape.d / 8; // Int(-8 * 0.75 * Beam.d) / 8
					c = femaData.c;
					Reporting.AddLine("Dist. from col. face to mid-pnt. of reduced section = " + a + B / 2 + ConstUnit.Length);
					radius = (Math.Pow(B, 2) / 4 + Math.Pow(c, 2)) / (2 * c);
					Reporting.AddLine("Dist. from uncut edge of beam flange to center of cut-out circle = " + (radius - c) + ConstUnit.Length);
					Reporting.AddLine("Chord Length of cut-out = " + B + ConstUnit.Length);
					Reporting.AddLine("Radius of cut-out = " + radius + ConstUnit.Length);
					Reporting.AddLine("Depth of cut-out = " + c + ConstUnit.Length);

					Reporting.AddHeader("FEMA 350 Design Calculations:");
					femaData.Cpr = 1.15; // (beam.Material.Fy + Beam.Fu) / (2 * beam.Material.Fy) 'eq. 3.2
					// Ze = Beam.z
					Zrbs = beam.Shape.zx - 2 * c * beam.Shape.tf * (beam.Shape.d - beam.Shape.tf);
					Srbs = beam.Shape.sx - c * (2 * beam.Shape.tf * beam.Shape.d - 4 * Math.Pow(beam.Shape.tf, 2));
					Ze = Zrbs;
					Se = Srbs;
					Reporting.AddLine("Ze = Zb - 2 * C * tf * (db - tf)");
					Reporting.AddLine("  = " + beam.Shape.zx + " - 2 * " + c + " * " + tf + " * (" + beam.Shape.d + " - " + beam.Shape.tf);
					Reporting.AddLine("  = " + Zrbs + ConstUnit.SecMod);

					Reporting.AddLine("Srbs = S -  C * (2 * tf * d- 4 * tf² )");
					Reporting.AddLine("  = " + beam.Shape.sx + " - * " + c + " * (2 * " + beam.Shape.tf + " * " + beam.Shape.d + " - 4 * " + beam.Shape.tf + "²)");
					Reporting.AddLine("  = " + Srbs + ConstUnit.SecMod);
					Reporting.AddLine("Cpr = " + femaData.Cpr);
					Ze = Zrbs;
					femaData.Mpr = Fema.FMpr(memberType); // Cpr * Ry * Zrbs * beam.Material.Fy 'eq. 3.1

					sh = (column.Shape.d / 2 + 0.625 * beam.Shape.bf + 0.75 * beam.Shape.d / 2); // eq. 3-15 and 3-16
					shf = femaData.shf == -100 ? sh : femaData.shf;

					Reporting.AddLine("sh = dc / 2 + 0.625 * bf + 0.75 * db/2"); // eq. 3-15 and 3-16
					Reporting.AddLine("  = " + column.Shape.d + " / 2 + 0.625 * " + beam.Shape.bf + " + 0.75 * " + beam.Shape.d + " /2"); // eq. 3-15 and 3-16
					Reporting.AddLine("  = " + sh + ConstUnit.Length + "      shf = " + shf + ConstUnit.Length);
					L1 = (femaData.L - sh - shf); //    =  L'
					Vp = Fema.fVp(memberType); // (2 * Mpr + femaData.Pg* Lp + Wg * L1 ^ 2 / 2) / L1 'fig. 3-3
					femaData.Mc = Fema.fMc(memberType); // Mpr + Vp * sh 'fig. 3-4
					Lpl = (sh - column.Shape.d / 2);
					femaData.Mf = Fema.fMf(memberType); // Mpr + Vp * (sh - Column.d / 2)
					Rmp = femaData.Ry * beam.Shape.zx * beam.Material.Fy;
					if (femaData.Mf < Rmp)
						Reporting.AddLine("Mf << (Ry * Z * Fy) = " + femaData.Ry + " * " + beam.Shape.zx + " * " + beam.Material.Fy + " = " + Rmp + ConstUnit.Moment + " (OK)");
					else
						Reporting.AddLine("Mf >=  (Ry * Z * Fy) = " + femaData.Ry + " * " + beam.Shape.zx + " * " + beam.Material.Fy + " = " + Rmp + ConstUnit.Moment + " (NG)");

					femaData.Cy = Srbs / (femaData.Cpr * Zrbs); //  eq.3.4
					Reporting.AddLine("Cy = Se / (Cpr * Ze) = " + beam.Shape.sx + " / (" + femaData.Cpr + " * " + Ze + ") = " + femaData.Cy); //  eq.3.4
					Vg = Fema.fVg(memberType); // Wg * (femaData.L - Column.d) / 2 + femaData.Pg* (femaData.L - Column.d - Lp) / (femaData.L - Column.d)
					Vf = 2 * femaData.Mf / (femaData.L - column.Shape.d) + Vg; // Eq. 3-52
					Reporting.AddLine("Vf = 2 * Mf / (L - dc) + Vg = 2 * " + femaData.Mf + " / (" + femaData.L + " - " + column.Shape.d + ") + " + Vg + " = " + Vf + ConstUnit.Force); // Eq. 3-52
					break;
				case EFemaConnectionType.WFP:
					Reporting.AddHeader("Flange Plate: PL " + beam.WinConnect.MomentFlangePlate.TopLength + " X " + beam.WinConnect.MomentFlangePlate.TopWidth + " X " + beam.WinConnect.MomentFlangePlate.TopThickness + ConstUnit.Length);
					Length = beam.WinConnect.MomentFlangePlate.TopLength;
					Bp = beam.WinConnect.MomentFlangePlate.TopWidth;
					// Step 3
					Ze = beam.Shape.zx;
					Se = beam.Shape.sx;
					Reporting.AddLine("Ze = Zb = " + Ze + ConstUnit.SecMod);
					femaData.Cpr = Fema.FCpr(memberType); // (beam.Material.Fy + Beam.Fu) / (2 * beam.Material.Fy) 'eq. 3.2"
					femaData.Mpr = Fema.FMpr(memberType); // Cpr * Ry * Ze * beam.Material.Fy 'eq. 3.1
					femaData.Cy = Fema.FCy(memberType); // Beam.s / (Cpr * Ze) ' eq.3.4
					sh = (column.Shape.d / 2 + Length);
					Reporting.AddLine("Sh = dc / 2 + Lpl = " + column.Shape.d + " / 2 + " + Length + " = " + sh + ConstUnit.Length);
					shf = femaData.shf == -100 ? sh : femaData.shf;

					Reporting.AddLine("Shf = " + shf + ConstUnit.Length);
					L1 = (femaData.L - sh - shf); //    =  L'
					Reporting.AddLine("L1 = L - sh - shf = " + femaData.L + " - " + sh + " - " + shf + " = " + L1 + ConstUnit.Length);
					femaData.Pg = femaData.Pg;
					Lp_h = (femaData.Lp - Length);
					Reporting.AddLine("Dist. from Load P to Hinge = " + Lp_h + ConstUnit.Length);
					femaData.Wg = femaData.Wg;
					Vp = Fema.fVp(memberType); // (2 * Mpr + femaData.Pg* Lp + Wg * L1 ^ 2 / 2) / L1 'fig. 3-3

					Lpl = Length;
					femaData.Mf = Fema.fMf(memberType); // Mpr + Vp * Length
					femaData.Mc = Fema.fMc(memberType); //  Mpr + Vp * sh 'fig. 3-4
					femaData.Myf = Fema.fMyf(memberType); // Cy * Mf 'eq.3.3

					// Step 4
					tp = (-beam.Shape.d + Math.Pow(Math.Pow(beam.Shape.d, 2) + 4 * femaData.Myf / (beam.WinConnect.MomentFlangePlate.Material.Fy * beam.WinConnect.MomentFlangePlate.TopWidth), 0.5)) / 2;
					Reporting.AddLine("tp = (-db + (db² + 4 * Myf / (Fyp *  bp))^0.5) / 2");
					Reporting.AddLine(" = (-" + beam.Shape.d + " + (" + beam.Shape.d + "² + 4 * " + femaData.Myf + " / (" + beam.WinConnect.MomentFlangePlate.Material.Fy + " * " + beam.WinConnect.MomentFlangePlate.TopWidth + "))^0.5) / 2");
					if (tp <= beam.WinConnect.MomentFlangePlate.TopThickness)
						Reporting.AddLine("= " + tp + " <= " + beam.WinConnect.MomentFlangePlate.TopThickness + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + tp + " >> " + beam.WinConnect.MomentFlangePlate.TopThickness + ConstUnit.Length + " (NG)");
					//  Step 5
					Reporting.AddHeader("Required Weld Size:");
					LW = 2 * (Length - beam.EndSetback) + (beam.Shape.bf - 2); // Notes 3 and 4
					Reporting.AddLine("Lw = 2 * (Lpl - c) + (bf - 2)");
					Reporting.AddLine(" = 2 * (" + Length + " - " + beam.EndSetback + ") + (" + beam.Shape.bf + " - 2) = " + LW + ConstUnit.Length);
					w = femaData.Mf / ((beam.Shape.d + beam.WinConnect.MomentFlangePlate.TopThickness) * 0.707 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * LW);
					Reporting.AddLine("w = Mf / ((db + tp) * 0.707 * 0.6 * Fexx * Lw) = " + femaData.Mf + " / ((" + beam.Shape.d + " + " + beam.WinConnect.MomentFlangePlate.TopThickness + ") * 0.707 * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + LW + ")");
					if (w <= beam.WinConnect.MomentFlangePlate.TopPlateToBeamWeldSize)
						Reporting.AddLine(" = " + w + " <= " + beam.WinConnect.MomentFlangePlate.TopPlateToBeamWeldSize + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine(" = " + w + " >> " + beam.WinConnect.MomentFlangePlate.TopPlateToBeamWeldSize + ConstUnit.Length + " (NG)");

					wmax = beam.WinConnect.MomentFlangePlate.TopThickness - ConstNum.SIXTEENTH_INCH; // Min(tp - FractionofinchN(1), Beam.tf - FractionofinchN(1))
					Reporting.AddHeader("w_max = Min(tp; tf) - " + ConstNum.SIXTEENTH_INCH + " = Min(" + beam.WinConnect.MomentFlangePlate.TopThickness + ";" + beam.Shape.tf + " ) - " + ConstNum.SIXTEENTH_INCH + "");
					if (wmax <= w)
						Reporting.AddLine(" = " + wmax + " <= " + w + ConstUnit.Length + " (NG)");
					else
						Reporting.AddLine(" = " + wmax + " >> " + w + ConstUnit.Length + " (OK)");
					break;
				case EFemaConnectionType.WUFB:
					Reporting.AddHeader("Beam Flange:");
					Reporting.AddLine("CJP groove weld top and bottom,");
					Reporting.AddLine("with " + NumberFun.ConvertFromFraction(5) + ConstUnit.Length + " fillet welds as shown.");

					Reporting.AddHeader("Beam web:");
					Reporting.AddLine("Shear tab bolted to web, welded to column");
					Reporting.AddLine("Bolts: (" + beam.WinConnect.ShearWebPlate.Bolt.NumberOfBolts + ") " + beam.WinConnect.ShearWebPlate.Bolt.BoltName);

					Reporting.AddHeader("FEMA 350 Design Calculations:");
					Ze = beam.Shape.zx;
					Se = beam.Shape.sx;
					Reporting.AddLine("Ze = Zb = " + Ze + ConstUnit.SecMod);
					femaData.Cpr = Fema.FCpr(memberType); //  (beam.Material.Fy + Beam.Fu) / (2 * beam.Material.Fy) 'eq. 3.2
					femaData.Mpr = Fema.FMpr(memberType); // Cpr * Ry * Ze * beam.Material.Fy 'eq. 3.1
					sh = ((column.Shape.d + beam.Shape.d) / 2);
					Reporting.AddHeader("Sh = (dc + db)/2 = (" + column.Shape.d + " + " + beam.Shape.d + ") / 2 = " + sh + ConstUnit.Length);
					shf = femaData.shf == -100 ? sh : femaData.shf;

					Reporting.AddHeader("Shf = " + shf + ConstUnit.Length + " (= User-defined Sh at far end of beam)");
					L1 = (femaData.L - sh - shf); //    =  L'
					femaData.Wg = femaData.Wg; // GravityLoad 'k/ft
					femaData.Pg = femaData.Pg;
					Lp_h = (femaData.Lp - (sh - column.Shape.d / 2));
					Vp = Fema.fVp(memberType); // (2 * Mpr + femaData.Pg* Lp + Wg * L1 ^ 2 / 2) / L1 'fig. 3-3
					femaData.Mc = Fema.fMc(memberType); // Mpr + Vp * sh 'fig. 3-4
					Lpl = (sh - column.Shape.d / 2);
					femaData.Mf = Fema.fMf(memberType); // Mpr + Vp * (sh - Column.d / 2) 'fig. 3-4
					femaData.Cy = Fema.FCy(memberType); // Beam.s / (Cpr * Ze) ' eq.3.4
					femaData.Myf = Fema.fMyf(memberType); //  Cy * Mf 'eq.3.3

					Vg = Fema.fVg(memberType); //  Wg * (femaData.L - Column.d) / 2 + femaData.Pg* (femaData.L - Column.d - Lp) / (femaData.L - Column.d)
					Vf = 2 * femaData.Mf / (femaData.L - column.Shape.d) + Vg; // Eq. 3-52
					Reporting.AddLine("Vf = 2 * Mf / (femaData.L - dc) + Vg = 2 * " + femaData.Mf + " / (" + femaData.L + " - " + column.Shape.d + ") + " + Vg + " = " + Vf + ConstUnit.Force); // Eq. 3-52
					femaData.Vf = Vf;

					// Panel zone thickness for simultaneous yielding of column web and beam
					// H = average of story heights above and below the joint
					tpz = femaData.Cy * femaData.Mc * (h - beam.Shape.d) / (h * 0.9 * 0.6 * column.Material.Fy * Ryc * column.Shape.d * (beam.Shape.d - beam.Shape.tf));
					// Vg=Shear at col. face from factored gravity load
					break;
				case EFemaConnectionType.WUFW:
					Reporting.AddHeader("Beam Flange:");
					Reporting.AddLine("CJP groove weld top and bottom,");
					Reporting.AddLine("with " + NumberFun.ConvertFromFraction(5) + ConstUnit.Length + " fillet welds as shown.");

					Reporting.AddHeader("Beam web:");
					Reporting.AddLine("Beam web welded (CJP) to column as shown");
					Reporting.AddLine("Shear tab welded to beam web and column as shown.");

					Reporting.AddHeader("FEMA 350 Design Calculations:");
					Ze = beam.Shape.zx;
					Se = beam.Shape.sx;
					Reporting.AddLine("Ze = Zb = " + Ze + ConstUnit.SecMod);
					femaData.Cpr = Fema.FCpr(memberType); // (beam.Material.Fy + Beam.Fu) / (2 * beam.Material.Fy) 'eq. 3.2

					femaData.Mpr = Fema.FMpr(memberType); // Cpr * Ry * Ze * beam.Material.Fy 'eq. 3.1

					sh = ((column.Shape.d + beam.Shape.d) / 2);
					Reporting.AddLine("Sh = (dc + db) / 2 = (" + column.Shape.d + " + " + beam.Shape.d + ") / 2 = " + sh + ConstUnit.Length);
					shf = femaData.shf == -100 ? sh : femaData.shf;

					Reporting.AddLine("Shf = " + shf + ConstUnit.Length);
					L1 = (femaData.L - sh - shf); //    =  L'
					femaData.Wg = femaData.Wg; // GravityLoad 'k/ft
					femaData.Pg = femaData.Pg;
					Lp_h = (femaData.Lp - (sh - column.Shape.d / 2));
					Vp = Fema.fVp(memberType); //  (2 * Mpr + femaData.Pg* Lp + Wg * L1 ^ 2 / 2) / L1 'fig. 3-3
					femaData.Mc = Fema.fMc(memberType); // Mpr + Vp * sh 'fig. 3-4
					femaData.Cy = Fema.FCy(memberType); // Beam.s / (Cpr * Ze) ' eq.3.4
					if (!beam.WinConnect.ShearWebPlate.Thickness_User)
						beam.WinConnect.ShearWebPlate.Thickness = beam.Shape.tw;
					beam.WinConnect.ShearWebPlate.Length = beam.Shape.d - 2 * (beam.Shape.tf + Math.Max(0.75 * beam.Shape.tf, 0.75) + ConstNum.THREE_EIGHTS_INCH) + 2 * ConstNum.EIGHTH_INCH;
                    beam.WinConnect.ShearWebPlate.Width = beam.EndSetback + 3 * beam.Shape.tf + ConstNum.THREE_EIGHTS_INCH + ConstNum.TWO_INCHES;
					beam.WinConnect.ShearWebPlate.BeamWeldSize = beam.WinConnect.ShearWebPlate.Thickness - ConstNum.SIXTEENTH_INCH;
					beam.WinConnect.ShearWebPlate.SupportWeldType = EWeldType.CJP;

					// Panel zone thickness for simultaneous yielding of column web and beam
					// H = average of story heights above and below the joint
					Reporting.AddHeader("Shear Connection:");
					Reporting.AddHeader("Plate Dimensions:");
					Reporting.AddLine("Thickness = " + beam.WinConnect.ShearWebPlate.Thickness + ConstUnit.Length + " (same as beam web thikness)  (OK)");
					Reporting.AddLine("Length = db - 2 * (tf + max(0.75*tf; 0.75) + " + ConstNum.THREE_EIGHTS_INCH + ") + 2 * " + ConstNum.EIGHTH_INCH + "");
					Reporting.AddLine(" = " + beam.Shape.d + " - 2 * (" + beam.Shape.tf + " + max(0.75 * " + beam.Shape.tf + "; 0.75) + " + ConstNum.THREE_EIGHTS_INCH + ") + 2 * " + ConstNum.EIGHTH_INCH + "");
					Reporting.AddLine(" = " + beam.WinConnect.ShearWebPlate.Length + ConstUnit.Length + " (overlaps weld access hole by " + ConstNum.EIGHTH_INCH + ConstUnit.Length + " )  (OK)");
					Reporting.AddLine("Width = (Beam end offset) + 3 * tf + " + ConstNum.THREE_EIGHTS_INCH + " + " + ConstNum.TWO_INCHES);
					Reporting.AddLine(" = " + beam.EndSetback + " + 3 * " + beam.Shape.tf + " + " + ConstNum.THREE_EIGHTS_INCH + " + " + ConstNum.TWO_INCHES);
					Reporting.AddLine(" = " + beam.WinConnect.ShearWebPlate.Width + ConstUnit.Length + " (extends " + ConstNum.TWO_INCHES + ConstUnit.Length + " beyond weld access hole)  (OK)");

					Reporting.AddLine("Use: PL" + beam.WinConnect.ShearWebPlate.Thickness + ConstUnit.Length + " X" + beam.WinConnect.ShearWebPlate.Width + ConstUnit.Length + " X" + beam.WinConnect.ShearWebPlate.Length + ConstUnit.Length);
					Reporting.AddLine("Taper top and bottom starting at " + ConstNum.ONE_INCH + ConstUnit.Length + " from column to ");
					Reporting.AddLine(" " + (beam.WinConnect.ShearWebPlate.Length - ConstNum.TWO_INCHES) + ConstUnit.Length + "  PL length at end of plate as shown.");

					Reporting.AddHeader("Welds:");
					Reporting.AddLine("Beam web-to-column:  CJP");
					Reporting.AddLine("Plate-to-column:  PJP with reinforcement equal to plate thickness  (OK)");
					Reporting.AddLine("Plate-to-column:  PJP with reinforcement equal to plate thickness  (OK)");
					beam.WinConnect.ShearWebPlate.BeamWeldSize = beam.WinConnect.ShearWebPlate.Thickness - ConstNum.SIXTEENTH_INCH;
					Reporting.AddLine("Plate-to-beam web: " + beam.WinConnect.ShearWebPlate.BeamWeldSize + ConstUnit.Length + " (" + ConstNum.SIXTEENTH_INCH + ConstUnit.Length + " less then plate thickness)  (OK)");
					Reporting.AddLine("Weld tapered edges plus 1/3 of plate length top and bottom.");
					break;
				case EFemaConnectionType.FF:
					Reporting.AddHeader("Beam Flange:");
					Reporting.AddLine("CJP groove weld top and bottom,");
					Reporting.AddLine("with " + NumberFun.ConvertFromFraction(5) + ConstUnit.Length + " fillet welds as shown.");

					Reporting.AddHeader("Beam web:");
					Reporting.AddLine("Beam web cut back " + 5.5 * beam.Shape.tf + ConstUnit.Length + "  as shown.");
					Reporting.AddLine("Shear tab welded to beam web and column as shown.");

					Reporting.AddHeader("FEMA 350 Design Calculations:");
					Ze = beam.Shape.zx;
					Se = beam.Shape.sx;
					Reporting.AddLine("Ze = Zb = " + Ze + ConstUnit.SecMod);
					if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.SMF)
					{
						femaData.Cpr = 1.2;
						Reporting.AddLine("femaData.Cpr = 1.2");
					}
					else
						femaData.Cpr = Fema.FCpr(memberType); //  (beam.Material.Fy + Beam.Fu) / (2 * beam.Material.Fy) 'eq. 3.2

					femaData.Mpr = Fema.FMpr(memberType); // Cpr * Ry * Ze * beam.Material.Fy 'eq. 3.1
					sh = ((column.Shape.d + beam.Shape.d) / 2);
					Reporting.AddHeader("Sh = (dc + db) / 2 = (" + column.Shape.d + " + " + beam.Shape.d + ") / 2 = " + sh + ConstUnit.Length);
					shf = femaData.shf == -100 ? sh : femaData.shf;

					Reporting.AddLine("Shf = " + shf + ConstUnit.Length);
					BeamFlangeSlenderness = beam.Shape.bf / (2 * beam.Shape.tf) <= 0.305F * Math.Pow(ConstNum.ELASTICITY / beam.Material.Fy, 0.5);
					L1 = (femaData.L - sh - shf); //    =  L'
					Reporting.AddLine("L' = L- sh - shf = " + femaData.L + " - " + sh + " - " + shf + " = " + L1 + ConstUnit.Length);
					femaData.Wg = femaData.Wg; // k/ft
					Lp_h = (femaData.Lp - (sh - column.Shape.d / 2));
					femaData.Pg = femaData.Pg;

					Vp = Fema.fVp(memberType); // (2 * Mpr + femaData.Pg* Lp + Wg * L1 ^ 2 / 2) / L1 'fig. 3-3

					femaData.Mc = Fema.fMc(memberType); //  Mpr + Vp * sh 'fig. 3-4

					femaData.Cy = Fema.FCy(memberType); // Beam.s / (Cpr * Ze) ' eq.3.4
					Lpl = sh - columnd / 2.0;
					femaData.Mf = Fema.fMf(memberType); // Mpr + Vp * (sh - Column.d / 2) 'fig. 3-4

					// Length of Free Flange
					Lff = 5.5 * beam.Shape.tf; // Eq. 3-10
					Reporting.AddLine("Lff = 5.5 * tf = 5.5 * " + beam.Shape.tf + " = " + Lff + ConstUnit.Length);
					Vg = Fema.fVg(memberType); // Wg * (femaData.L - Column.d) / 2 + femaData.Pg* (femaData.L - Column.d - Lp) / (femaData.L - Column.d)

					Reporting.AddHeader("Web Connection:");
					Reporting.AddLine("Shear Force on the shear tab, Vst:");
					Vst = 2 * femaData.Mf / (femaData.L - column.Shape.d) + Vg; // Eq. 3-10
					Reporting.AddLine("Vst = 2 * Mf / (femaData.L - dc) + Vg = 2 * " + femaData.Mf + " / (" + femaData.L + " - " + column.Shape.d + ") + " + Vg + " = " + Vst + ConstUnit.Force);
					// Step 6
					Reporting.AddHeader("Tension Force on the shear tab (Tst):");
					Tst = femaData.Mf / (beam.Shape.d - beam.Shape.tf) - femaData.Ry * beam.Material.Fy * beam.Shape.bf * beam.Shape.tf; // Eq. 3-11
					Reporting.AddLine("Tst = Mf / (d - tf) - Ry * Fy * bf * tf "); // Eq. 3-11
					Reporting.AddLine("= " + femaData.Mf + "/ (" + beam.Shape.d + " - " + beam.Shape.tf + ") - " + femaData.Ry + " * " + beam.Material.Fy + " * " + beam.Shape.bf + " * " + beam.Shape.tf); // Eq. 3-11
					Reporting.AddLine("= " + Tst + ConstUnit.Force);
					B = 2;
					beam.WinConnect.ShearWebPlate.Length = beam.Shape.d - 2 * beam.Shape.tf - 2 * B; // Eq. 3-12
					Reporting.AddLine("Plate length, Hst = db - 2 * tf - 2 * b = " + beam.Shape.d + " - 2 * " + beam.Shape.tf + " - 2 * " + B + " = " + beam.WinConnect.ShearWebPlate.Length + ConstUnit.Length);
					beam.WinConnect.ShearWebPlate.Width = 2 * Lff;
					Reporting.AddLine("Plate width, W = 2 * Lff = " + beam.WinConnect.ShearWebPlate.Width + ConstUnit.Length);
					// Step 8
					// effective weld length:
					Reporting.AddHeader("Effective weld length:");
					wL_eff = beam.WinConnect.ShearWebPlate.Length / 2;
					Reporting.AddLine("wL_eff = Hst / 2 = " + wL_eff + ConstUnit.Length);
					Reporting.AddLine("Angle of resultant force from vertical:");
					theta = Math.Atan(Tst / (0.5 * Vst)) * 57.296;
					Reporting.AddLine("Theta = Atn(Tst / (0.5 * Vst)) * 57.296 = Atn(" + Tst + " / (0.5 * " + Vst + ")) * 57.296 = " + theta + " Degree");

					Reporting.AddHeader("Required plate to column weld size:");
					Cnst = EccentricWeld.GetEccentricWeld(0, 0, theta / 57.296, true);
					//EccentricWeld.GetEccentricWeld(0, 0, ref Cnst, theta / 57.296, true);
					Cnst = 2 * Cnst;
					C1 = CommonCalculations.WeldTypeRatio();
					w_Column_req = Math.Pow(4 * Math.Pow(Tst, 2) + Math.Pow(Vst, 2), 0.5) / (16 * Cnst * C1 * wL_eff);
					Reporting.AddLine("w_Column_req = (4 * Tst² + Vst²)^0.5 / (16 * C * C1 * wL_eff)");
					Reporting.AddLine(" = (4 * " + Tst + "² + " + Vst + "²)^0.5 / (16*" + c + " * " + C1 + "* " + wL_eff + ")");
					Reporting.AddLine(" = " + w_Column_req + ConstUnit.Length);

					Reporting.AddHeader("Plate Thickness:");
					Reporting.AddHeader("Required thickness for shear on CJP weld:");
					Plt1 = 0.5 * Vst / (0.8 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * beam.Shape.d / 4); // Shear Only weld
					Reporting.AddLine("Plt1 = 0.5 * Vst / (0.8 * 0.6 * Fexx * db / 4)");
					Reporting.AddLine(" = 0.5 * " + Vst + " / (0.8 * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + beam.Shape.d + " / 4) = " + Plt1 + ConstUnit.Length);
					Reporting.AddLine("Required thickness for shear on plate:");

					Plt2 = 0.5 * Vst / (0.9 * 0.6 * Math.Min(beam.WinConnect.ShearWebPlate.Material.Fy, column.Material.Fy) * beam.Shape.d / 4); // Shear Only plate
					Reporting.AddLine("Plt2 = 0.5 * Vst / (0.9 * 0.6 * Min(Fyp, Fyc) * db / 4)");
					Reporting.AddLine(" = 0.5 * " + Vst + " / (0.9 * 0.6 * Min(" + beam.WinConnect.ShearWebPlate.Material.Fy + "; " + column.Material.Fy + ") * " + beam.Shape.d + " / 4) = " + Plt2 + ConstUnit.Length);

					Reporting.AddHeader("Required thickness for tension only:");
					Plt3 = Tst / (0.9 * Math.Min(beam.WinConnect.ShearWebPlate.Material.Fy, column.Material.Fy) * beam.Shape.d / 4); //  Tension only
					Reporting.AddLine("Plt3= Tst / (0.9 * Min(Fyp; Fyc) * db / 4)");
					Reporting.AddLine(" = " + Tst + " / (0.9* Min(" + beam.WinConnect.ShearWebPlate.Material.Fy + "; " + column.Material.Fy + ") * " + beam.Shape.d + " / 4) = " + Plt3 + ConstUnit.Length);

					// Equivalent Tension
					Reporting.AddHeader("Combined tension and shear (equivalent tension):");
					Teq = Tst / 2 + Math.Pow(Math.Pow(Tst / 2, 2) + Math.Pow(Vst / 2, 2), 0.5);
					Reporting.AddLine("Teq = Tst / 2 + ((Tst / 2)² + (Vst / 2)²)^0.5");
					Reporting.AddLine(" = " + Tst + " / 2 + ((" + Tst + " / 2)² + (" + Vst + " / 2)²)^0.5 = " + Teq + ConstUnit.Force);

					Reporting.AddHeader("Required thickness for equivalent tension:");
					Plt4 = Teq / (0.9 * Math.Min(beam.WinConnect.ShearWebPlate.Material.Fy, column.Material.Fy) * beam.Shape.d / 4); //  Combined Tension and Shear
					Reporting.AddLine("Plt4= Teq/ (0.9 * Min(Fyp; Fyc) * db / 4)");
					Reporting.AddLine(" = " + Teq + " / (0.9* Min(" + beam.WinConnect.ShearWebPlate.Material.Fy + "; " + column.Material.Fy + ") * " + beam.Shape.d + " / 4) = " + Plt4 + ConstUnit.Length);
					// Maximum Shear
					Reporting.AddHeader("Maximum shear:");
					Vmax = Math.Pow(Math.Pow(Tst / 2, 2) + Math.Pow(Vst / 2, 2), 0.5);
					Reporting.AddLine("Vmax = (( Tst / 2)² + (Vst / 2)²)^0.5 = ((" + Tst + " / 2)² + (" + Vst + " / 2)²)^0.5 = " + Vmax + ConstUnit.Force);

					Reporting.AddHeader("Required thickness for maximum shear:");
					Plt5 = Vmax / (0.9 * 0.6 * Math.Min(beam.WinConnect.ShearWebPlate.Material.Fy, column.Material.Fy) * beam.Shape.d / 4); // Maximum Shear Stress
					Reporting.AddLine("Plt5= Vmax/ (0.9 * 0.6 * Min(Fyp; Fyc) * db / 4)");
					Reporting.AddLine(" = " + Vmax + " / (0.9 * 0.6 * Min(" + beam.WinConnect.ShearWebPlate.Material.Fy + "; " + column.Material.Fy + ") * " + beam.Shape.d + " / 4) = " + Plt5 + ConstUnit.Length);

					Reporting.AddHeader("Maximum of Plt1 ~ Plt5:");
					if (!beam.WinConnect.ShearWebPlate.Thickness_User)
						beam.WinConnect.ShearWebPlate.Thickness = Math.Max(Math.Max(Plt1, Plt2), Math.Max(Math.Max(Plt3, Plt4), Plt5));
					Reporting.AddLine("tp = max(max(Plt1; Plt2); max(max(Plt3; Plt4); Plt5))");
					Reporting.AddLine(" = max(max(" + Plt1 + "; " + Plt2 + "); max(max(" + Plt3 + "; " + Plt4 + "); " + Plt5 + "))");
					Reporting.AddLine(" = " + beam.WinConnect.ShearWebPlate.Thickness + ConstUnit.Length);

					Reporting.AddHeader("Plate to Beam Weld:");
					k = (beam.WinConnect.ShearWebPlate.Width - Lff - ConstNum.ONE_INCH) / beam.WinConnect.ShearWebPlate.Length;
					Reporting.AddLine("Horiz. projection of inclined edge of plate = k*Hst:");
					Reporting.AddLine("k = (W - Lff - BirInchN) / Hst = (" + beam.WinConnect.ShearWebPlate.Width + " - " + Lff + " - " + ConstNum.ONE_INCH + ") / " + beam.WinConnect.ShearWebPlate.Length + " = " + k);
					k1 = k;
					Reporting.AddHeader("Vert. projection of inclined edge of plate = k1*Hst:   k1 = k = " + k);
					k2 = (beam.WinConnect.ShearWebPlate.Width - Lff) / beam.WinConnect.ShearWebPlate.Length;

					Reporting.AddHeader("Dist. between two vertical welds = k2 * Hst:");
					Reporting.AddLine("k2 = (W - Lff) / Hst = (" + beam.WinConnect.ShearWebPlate.Width + " - " + Lff + ") / " + beam.WinConnect.ShearWebPlate.Length + " = " + k2);
					ex = Math.Abs((beam.WinConnect.ShearWebPlate.Width * (2 - 0.5858F * k) - beam.WinConnect.ShearWebPlate.Length * (k + k2 + 0.707 * Math.Pow(k, 2))) / (2 + 0.8284 * k) - Tst * (beam.WinConnect.ShearWebPlate.Length - beam.Shape.d / 4) / Vst);
					Reporting.AddLine("Load Eccentricity on weld group (includes moment effect):");
					Reporting.AddLine("ex = | W * (2 - 0.5858 * k) - Hst * (k + k2 + 0.707 * k²)) / (2 + 0.8284 * k) - Tst * (Hst - d / 4) / Vst |");
					Reporting.AddLine(" = | (" + beam.WinConnect.ShearWebPlate.Width + "* (2 - 0.5858 * " + k + ") - " + beam.WinConnect.ShearWebPlate.Length + "* (" + k + " + " + k2 + " + 0.707 * " + k + "²)) / (2 + 0.8284 * " + k + ")");
					Reporting.AddLine("    - " + Tst + " * (" + beam.WinConnect.ShearWebPlate.Length + " - " + beam.Shape.d + " / 4) / " + Vst + " | = " + ex + ConstUnit.Length);

					Fema.TrapezoidalWeld(memberType, k, k1, k2, ex, 0, beam.WinConnect.ShearWebPlate.Length, ref Coeff);
					Reporting.AddLine("Weld strength coefficient (C) = " + Coeff);
					w = Vst / (16 * Coeff * C1 * beam.WinConnect.ShearWebPlate.Length);
					Reporting.AddLine("Required weld size:");
					Reporting.AddLine("w = Vst / (16 * C * C1 * Lp) = " + Vst + " / (16 * " + Coeff + " * " + C1 + " * " + beam.WinConnect.ShearWebPlate.Length + ") = " + w + " " + ConstUnit.Length + "");
					// Ceck Bam Web:
					MaxWeld1 = beam.Shape.tw - ConstNum.SIXTEENTH_INCH;
					Reporting.AddLine("MaxWeld1 = tw - " + ConstNum.SIXTEENTH_INCH + " = " + beam.Shape.tw + " - " + ConstNum.SIXTEENTH_INCH + "  = " + MaxWeld1 + ConstUnit.Length);
					Maxweld2 = 1.4141 * beam.Shape.tw * beam.Material.Fu / CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine("Maxweld2 = 1.4141 * tw * Fu / Fexx = 1.4141 * " + beam.Shape.tw + " * " + beam.Material.Fu + " / " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " = " + Maxweld2 + ConstUnit.Length);
					Plt6 = CommonDataStatic.Preferences.DefaultElectrode.Fexx * w / (1.4141F * beam.WinConnect.ShearWebPlate.Material.Fu);
					Reporting.AddLine("Plt6 = Fexx * w / (1.4141 * Fu) = " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + w + " / (1.4141 * " + beam.WinConnect.ShearWebPlate.Material.Fu + ") = " + Plt6 + ConstUnit.Length);
					if (!beam.WinConnect.ShearWebPlate.Thickness_User)
						beam.WinConnect.ShearWebPlate.Thickness = Math.Max(beam.WinConnect.ShearWebPlate.Thickness, Plt6);
					Reporting.AddLine("tp = max(tp; Plt6) = max(" + beam.WinConnect.ShearWebPlate.Thickness + "; " + Plt6 + ") = " + beam.WinConnect.ShearWebPlate.Thickness + ConstUnit.Length);
					if (!beam.WinConnect.ShearWebPlate.Thickness_User)
						beam.WinConnect.ShearWebPlate.Thickness = CommonCalculations.PlateThickness(beam.WinConnect.ShearWebPlate.Thickness);
					Reporting.AddLine("Use tp = " + beam.WinConnect.ShearWebPlate.Thickness + ConstUnit.Length);
					break;
				case EFemaConnectionType.BUEP:
					Reporting.AddHeader("FEMA 350 Design Calculations:");
					femaData.Cpr = Fema.FCpr(memberType); // (beam.Material.Fy + Beam.Fu) / (2 * beam.Material.Fy) 'eq. 3.2
					Ze = beam.Shape.zx;
					Se = beam.Shape.sx;
					Reporting.AddLine("Ze = Zb = " + Ze + ConstUnit.SecMod);
					femaData.Mpr = Fema.FMpr(memberType); // Cpr * Ry * Ze * beam.Material.Fy 'eq. 3.1
					femaData.Wg = femaData.Wg; // k/ft
					femaData.Pg = femaData.Pg;
					sh = (column.Shape.d / 2 + beam.Shape.d / 3 + beam.WinConnect.MomentEndPlate.Thickness);
					Reporting.AddLine("Sh = dc / 2 + db / 3 + tp = " + column.Shape.d + " / 2 + " + beam.Shape.d + " / 3 + " + beam.WinConnect.MomentEndPlate.Thickness + " = " + sh + ConstUnit.Length);
					shf = femaData.shf == -100 ? sh : femaData.shf;

					Reporting.AddLine("Shf = " + shf + ConstUnit.Length);
					L1 = (femaData.L - sh - shf); //    =  L'
					Reporting.AddLine("L' = L- sh - shf = " + femaData.L + " - " + sh + " - " + shf + " = " + L1 + ConstUnit.Length);
					Lp_h = (femaData.Lp - (sh - column.Shape.d / 2));
					Vp = Fema.fVp(memberType); // (2 * Mpr + femaData.Pg* Lp + Wg * L1 ^ 2 / 2) / L1 'fig. 3-3
					Lpl = (sh - column.Shape.d / 2);
					femaData.Mf = Fema.fMf(memberType); //  Mpr + Vp * (sh - Column.d / 2)
					femaData.Mc = Fema.fMc(memberType); //  Mpr + Vp * sh 'fig. 3-4
					femaData.Cy = Fema.FCy(memberType);
					Vg = Fema.fVg(memberType); // Wg * (femaData.L - Column.d) / 2 + femaData.Pg* (femaData.L - Column.d - Lp) / (femaData.L - Column.d)
					Vf = 2 * femaData.Mf / (femaData.L - column.Shape.d) + Vg; // Eq. 3-52
					Reporting.AddLine("Vf = 2 * Mf / (femaData.L - dc) + Vg = 2 * " + femaData.Mf + " / (" + femaData.L + " - " + column.Shape.d + ") + " + Vg + " = " + Vf + ConstUnit.Force); // Eq. 3-52
					// Step 2, 3
					Reporting.AddHeader("End Plate");
					pf = beam.WinConnect.MomentEndPlate.DistanceToFirstBolt;
					Reporting.AddLine("pf = " + pf + ConstUnit.Length + "");
					d0 = (beam.Shape.d + pf - beam.Shape.tf / 2);
					Reporting.AddLine("d0 = d +pf - tf / 2 = " + beam.Shape.d + " + " + pf + " - " + beam.Shape.tf + " / 2 " + ConstUnit.Length + "");
					d1 = beam.Shape.d - 1.5 * beam.Shape.tf - pf;
					Reporting.AddLine("d1 = d - 1.5 * tf - pf = " + beam.Shape.d + " - 1.5 * " + beam.Shape.tf + " - " + pf + ConstUnit.Length);

					Reporting.AddHeader("Bolt Area Required for Tension:");
					switch (beam.WinConnect.MomentEndPlate.Bolt.ASTMType)
					{
						case EBoltASTM.A325:
							Abolt = femaData.Mf / (2 * ConstNum.COEFFICIENT_90 * (d0 + d1)); // eq 3-18
							Reporting.AddLine("A_bolt = Mf / (2 * " + ConstNum.COEFFICIENT_90 + " * (d0 + d1)) = " + femaData.Mf + " / (2 * " + ConstNum.COEFFICIENT_90 + " * (" + d0 + " + " + d1 + ")) = " + Abolt + ConstUnit.Area);
							break;
						case EBoltASTM.A490:
							Abolt = femaData.Mf / (2 * ConstNum.COEFFICIENT_113 * (d0 + d1));
							Reporting.AddLine("A_bolt = Mf / (2 * " + ConstNum.COEFFICIENT_113 + " * (d0 + d1)) = " + femaData.Mf + " / (2 * " + ConstNum.COEFFICIENT_113 + " * (" + d0 + " + " + d1 + ")) = " + Abolt + ConstUnit.Area);
							break;
					}

					Reporting.AddHeader("Bolt Area Required for Shear:");
					AboltShear = Vf / (3 * beam.WinConnect.MomentEndPlate.Bolt.BoltStrength); // eq 3-19
					Reporting.AddLine("A_bolt_shear = Vf / (3 * Fv) = " + Vf + " / (3 * " + beam.WinConnect.MomentEndPlate.Bolt.BoltStrength + ") = " + AboltShear + ConstUnit.Area);
					Ab = Math.Max(Abolt, AboltShear);
					Reporting.AddLine("Ab = max(A_bolt, A_bolt_shear) = " + Ab + ConstUnit.Area);

					Reporting.AddHeader("Bolt Diameter:");
					BoltDiam = Math.Pow(Ab * 4 / Math.PI, 0.5) / 8;
					Reporting.AddLine("d_bolt = (4 * Ab / 3.1416)^0.5 = (0.4 * " + Ab + " / PI)^0.5 = " + Math.Pow(Ab * 4 / Math.PI, 0.5) + ConstUnit.Length + " Use " + BoltDiam + ConstUnit.Length);
					if (BoltDiam > ConstNum.ONEANDHALF_INCHES)
						Reporting.AddLine("Bolt diameter exceeds " + ConstNum.ONEANDHALF_INCHES + ConstUnit.Length + " try BSEP connection. (NG)");

					beam.WinConnect.MomentEndPlate.Bolt.BoltSize = BoltDiam;
					// Step 4, 5
					g = column.GageOnFlange;
					Reporting.AddLine("Bolt Gage on Column, g = " + g + ConstUnit.Length);
					Bp = beam.Shape.bf + ConstNum.ONE_INCH / 4; // Int(-4 * (Beam.BF + birinchn)) / 4
					Reporting.AddLine("End Plate Width, bp = " + Bp + ConstUnit.Length);

					Reporting.AddHeader("End Plate Thickness:");
					s = Math.Pow(Bp * g, 0.5); // eq 3-21
					Reporting.AddLine("s = (bp * g)^0.5 = " + s + ConstUnit.Length);
					Fyp = beam.WinConnect.MomentEndPlate.Material.Fy;
					db = beam.Shape.d;
					pt = beam.Shape.tf + pf;
					Reporting.AddLine("pt = Beam.tf + pf  = " + beam.Shape.tf + " + " + pf + " = " + pt + ConstUnit.Length);

					Reporting.AddHeader("Required for bending:");
					tpM = Math.Pow(femaData.Mf / (0.8 * Fyp * ((db - pt) * (Bp / 2 * (1 / pf + 1 / s) + (pf + s) * 2 / g) + Bp / 2 * (db / pf + 1 / 2.0))), 0.5); // eq 3-20
					Reporting.AddLine("tpM = (Mf / (0.8 * Fyp * ((db - pt) *(bp / 2 * (1 / pf + 1 / s) + (pf + s) * 2 / g) + bp / 2 * (db / pf + 1/2))))^0.5"); // eq 3-20
					Reporting.AddLine(" = (" + femaData.Mf + " / (0.8 * " + Fyp + " * ((" + db + " - " + pt + ") * (" + Bp + " / 2 * (1 / " + pf + " + 1 / " + s + ") + (" + pf + " + " + s + ") * 2 / " + g + ") + " + Bp + " / 2 * (" + db + " / " + pf + " + 1 / 2))))^0.5"); // eq 3-20
					Reporting.AddLine("= " + tpM + ConstUnit.Length);

					Reporting.AddHeader("Required for Shear:");
					tpS = femaData.Mf / (1.1 * Fyp * Bp * (db - beam.Shape.tf)); // eq 3-22
					Reporting.AddLine("tpS = Mf / (1.1 * Fyp * bp * (db - tf))"); // eq 3-22
					Reporting.AddLine(" = " + femaData.Mf + " / (1.1 * " + Fyp + " * " + Bp + " * (" + db + " - " + beam.Shape.tf + "))"); // eq 3-22
					Reporting.AddLine("= " + tpS + ConstUnit.Length);

                    if (!beam.WinConnect.MomentEndPlate.Thickness_User) 
                        beam.WinConnect.MomentEndPlate.Thickness = CommonCalculations.PlateThickness(Math.Max(tpM, tpS));
					Reporting.AddLine("Use " + beam.WinConnect.MomentEndPlate.Thickness + ConstUnit.Length + " Plate");
                    if (!beam.WinConnect.MomentEndPlate.Width_User) 
                        beam.WinConnect.MomentEndPlate.Width = Bp;
                    if (!beam.WinConnect.MomentEndPlate.Length_User) 
                        beam.WinConnect.MomentEndPlate.Length = beam.Shape.d + 2 * (pf + beam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir) / 4;
                    if (!beam.WinConnect.MomentEndPlate.DistanceToFirstBolt_User) 
                        beam.WinConnect.MomentEndPlate.DistanceToFirstBolt = pf;
                    if (!beam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts_User) 
                        beam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts = 4;

					beam.WinConnect.MomentEndPlate.FlangeWeldType = EWeldType.CJP;
					if (!beam.EndSetback_User)
						beam.EndSetback = beam.WinConnect.MomentEndPlate.Thickness;

					Reporting.AddGoToHeader("Beam Web to Plate Weld:");
					w_web = Vf / (2 * (beam.Shape.d - 2 * beam.Shape.kdes) * 0.707 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddLine("Web_weld = Vf / (2 * (db - 2 * k) * 0.707 * 0.6 * Fexx)");
					Reporting.AddLine(" = " + Vf + "/ (2 * (" + beam.Shape.d + " - 2 * " + beam.Shape.kdes + ") * 0.707 * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " = " + w_web + ConstUnit.Length);

					minweld = CommonCalculations.MinimumWeld(beam.Shape.tw, beam.WinConnect.MomentEndPlate.Thickness);
					Reporting.AddLine("Minimum Weld = " + minweld + ConstUnit.Length);
					wmax_beamweb = beam.Material.Fu * beam.Shape.tw / (2 * 0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddLine("wmax_beamweb = Fu * tw / (2 * 0.707 * Fexx)");
					Reporting.AddLine("  = " + beam.Material.Fu + " * " + beam.Shape.tw + " / (2 * 0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddLine(" = " + wmax_beamweb + ConstUnit.Length);
					w = Math.Max(w_web, minweld);
					if (wmax_beamweb > w)
						w = wmax_beamweb;
                    if (!beam.WinConnect.MomentEndPlate.WebWeldRestSize_User) 
                        beam.WinConnect.MomentEndPlate.WebWeldRestSize = w / 16;

					Reporting.AddLine(" Use " + beam.WinConnect.MomentEndPlate.WebWeldRestSize + ConstUnit.Length + "  double-fillet or CJP weld.");
					break;
				case EFemaConnectionType.BSEP:
					Reporting.AddHeader("FEMA 350 Design Calculations:");
					// Step 1
					femaData.Cpr = Fema.FCpr(memberType); //  (beam.Material.Fy + Beam.Fu) / (2 * beam.Material.Fy) 'eq. 3.2
					Ze = beam.Shape.zx;
					Se = beam.Shape.sx;
					Reporting.AddLine("Ze = Zb = " + Ze + ConstUnit.SecMod);
					femaData.Mpr = Fema.FMpr(memberType); // Cpr * Ry * Ze * beam.Material.Fy 'eq. 3.1
					femaData.Wg = femaData.Wg; // k/ft
					femaData.Pg = femaData.Pg;
					sh = (column.Shape.d / 2 + beam.WinConnect.MomentEndPlate.Thickness + beam.WinConnect.MomentEndPlate.BraceStiffener.Length);
					Reporting.AddLine("Sh = dc / 2 + tp + L_stiff = " + column.Shape.d + "/2 + " + beam.WinConnect.MomentEndPlate.Thickness + " + " + beam.WinConnect.MomentEndPlate.BraceStiffener.Length + " = " + sh + ConstUnit.Length);
					shf = femaData.shf == -100 ? sh : femaData.shf;

					Reporting.AddLine("Shf = " + shf + ConstUnit.Length);
					L1 = (femaData.L - sh - shf); //    =  L'
					Reporting.AddLine("L' = L- sh - shf = " + femaData.L + " - " + sh + " - " + shf + " = " + L1 + ConstUnit.Length);
					Lp_h = (femaData.Lp - (sh - column.Shape.d / 2));
					Vp = Fema.fVp(memberType); //  (2 * Mpr + femaData.Pg* Lp + Wg * L1 ^ 2 / 2) / L1 'fig. 3-3
					Lpl = (sh - column.Shape.d / 2);
					femaData.Mf = Fema.fMf(memberType); // Mpr + Vp * (sh - Column.d / 2)
					femaData.Mc = Fema.fMc(memberType); // Mpr + Vp * sh 'fig. 3-4
					Vg = Fema.fVg(memberType); // Wg * (femaData.L - Column.d) / 2 + femaData.Pg* (femaData.L - Column.d - Lp) / (femaData.L - Column.d)
					femaData.Cy = Fema.FCy(memberType);
					Vf = 2 * femaData.Mf / (femaData.L - column.Shape.d) + Vg; // Eq. 3-52
					Reporting.AddLine("Vf = 2 * Mf / (femaData.L - dc) + Vg = 2 * " + femaData.Mf + " / (" + femaData.L + " - " + column.Shape.d + ") + " + Vg + " = " + Vf + ConstUnit.Force); // Eq. 3-52
					// Step 2, 3
					Reporting.AddHeader("End Plate");
					Reporting.AddLine("Plate Dimensions:");
					Reporting.AddLine("Thickness = " + beam.WinConnect.MomentEndPlate.Thickness + ConstUnit.Length);
					Reporting.AddLine("Width = " + beam.WinConnect.MomentEndPlate.Width + ConstUnit.Length);
					Reporting.AddLine("Length = " + beam.WinConnect.MomentEndPlate.Length + ConstUnit.Length);
					Reporting.AddLine("Bolts:  (16) " + beam.WinConnect.MomentEndPlate.Bolt.BoltName);

					pf = ConstNum.TWO_INCHES;
					Reporting.AddLine("pf = " + ConstNum.TWO_INCHES + ConstUnit.Length);
					Pb = beam.WinConnect.MomentEndPlate.Bolt.SpacingLongDir;
					Reporting.AddLine("pb = " + Pb + ConstUnit.Length);
					d0 = (beam.Shape.d + pf - beam.Shape.tf / 2); // Eq.3-36
					Reporting.AddLine("d0 = d + pf - tf / 2 = " + beam.Shape.d + " + " + pf + " - " + beam.Shape.tf + " / 2 " + ConstUnit.Length + "");
					d1 = beam.Shape.d - 1.5 * beam.Shape.tf - pf - Pb;
					Reporting.AddLine("d1 = d - 1.5 * tf - pf = " + beam.Shape.d + " - 1.5 * " + beam.Shape.tf + " - " + pf + " - " + Pb + ConstUnit.Length);

					Reporting.AddHeader("Bolt Tension");
					Reporting.AddLine("Flange Force:");
					Ffu = femaData.Mf / (beam.Shape.d - beam.Shape.tf); // eq 3-36
					Reporting.AddLine("Ffu = Mf / (d - tf) = " + femaData.Mf + " / (" + beam.Shape.d + " - " + beam.Shape.tf + ") = " + Ffu + ConstUnit.Force); // eq 3-36

					tp = beam.WinConnect.MomentEndPlate.Thickness;
					dbt = beam.WinConnect.MomentEndPlate.Bolt.BoltSize;
					ts = CommonCalculations.PlateThickness(beam.Shape.tf);
					Bp = beam.WinConnect.MomentEndPlate.Width;
					Tb = beam.WinConnect.MomentEndPlate.Bolt.Pretension;
					Reporting.AddLine("Minimum Bolt Tensile Strength Required:");

					Tub_min = 0.00002305 * Math.Pow(pf, 0.591) * Math.Pow(Ffu, 2.583) / (Math.Pow(tp, 0.895) * Math.Pow(dbt, 1.909) * Math.Pow(ts, 0.327) * Math.Pow(Bp, 0.965)) + Tb;
					Reporting.AddLine("Tub_min = 0.00002305 * pf^0.591 * Ffu².583 / (tp^0.895 * dbt^1.909 * ts^0.327 * bp^0.965) + Tb");
					Reporting.AddLine(" = 0.00002305 * " + pf + "^0.591 * " + Ffu + "².583 / (" + tp + "^0.895 * " + dbt + "^1.909 * " + ts + "^0.327 * " + Bp + "^0.965) + " + Tb);
					Reporting.AddLine(" = " + Tub_min + ConstUnit.Force);

					Abolt = Math.PI * Math.Pow(dbt, 2) / 4;
					Reporting.AddLine("Bolt Area, A_bolt = pi * dbt ^ 2 / 4 = " + Math.PI + " * " + dbt + " ^ 2 / 4 = " + Abolt + ConstUnit.Area);
					Reporting.AddLine("");
					switch (beam.WinConnect.MomentEndPlate.Bolt.ASTMType)
					{
						case EBoltASTM.A325:
							Tub = ConstNum.COEFFICIENT_90 * Abolt;
							if (Tub >= Tub_min)
								Reporting.AddLine("Tub = " + ConstNum.COEFFICIENT_90 + "*A_bolt = " + ConstNum.COEFFICIENT_90 + " * " + Abolt + " = " + Tub + " >= " + Tub_min + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("Tub = " + ConstNum.COEFFICIENT_90 + "*A_bolt = " + ConstNum.COEFFICIENT_90 + " * " + Abolt + " = " + Tub + " << " + Tub_min + ConstUnit.Force + " (NG)");
							break;
						case EBoltASTM.A490:
							Tub = ConstNum.COEFFICIENT_113 * Abolt;
							if (Tub >= Tub_min)
								Reporting.AddLine("Tub = " + ConstNum.COEFFICIENT_113 + "*A_bolt = " + ConstNum.COEFFICIENT_113 + " * " + Abolt + " = " + Tub + " >= " + Tub_min + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("Tub = " + ConstNum.COEFFICIENT_113 + "*A_bolt = " + ConstNum.COEFFICIENT_113 + " * " + Abolt + " = " + Tub + " << " + Tub_min + ConstUnit.Force + " (NG)");
							break;
					}

					Reporting.AddHeader("Max. Moment Bolts Can Develop:");
					Mf_Max = 3.4 * Tub * (d0 + d1);
					if (Mf_Max > femaData.Mf)
						Reporting.AddLine("Mf_Max = 3.4 * Tub * (d0 + d1) = 3.4 * " + Tub + " * (" + d0 + " + " + d1 + ") = " + Mf_Max + "  >> " + femaData.Mf + ConstUnit.Moment + " (OK)");
					else
						Reporting.AddLine("Mf_Max = 3.4 * Tub * (d0 + d1) = 3.4 * " + Tub + " * (" + d0 + " + " + d1 + ") = " + Mf_Max + "  <= " + femaData.Mf + ConstUnit.Moment + " (NG)");

					Reporting.AddHeader("Required Bolt Area for Shear Resistance:");
					AboltShear = Vf / (6 * beam.WinConnect.MomentEndPlate.Bolt.BoltStrength); // eq 3-33
					Reporting.AddLine("A_bolt_Shear = Vf / (6 * Fv)"); // eq 3-33
					if (AboltShear <= Abolt)
						Reporting.AddLine(" = " + Vf + " / (6 * " + beam.WinConnect.MomentEndPlate.Bolt.BoltStrength + ") = " + AboltShear + "  <= " + Abolt + ConstUnit.Area + " (OK)"); // eq 3-33
					else
						Reporting.AddLine(" = " + Vf + " / (6 * " + beam.WinConnect.MomentEndPlate.Bolt.BoltStrength + ") = " + AboltShear + "  >> " + Abolt + ConstUnit.Area + " (NG)"); // eq 3-33

					// Column Check:
					g = column.GageOnFlange;
					Reporting.AddLine("Bolt Gage on Column, g = " + g + ConstUnit.Length);
					Bp = beam.Shape.bf + 1;
					ts = beam.Shape.tw;
					dbt = beam.WinConnect.MomentEndPlate.Bolt.BoltSize;

					Reporting.AddHeader("End Plate Thickness");
					tpMin1 = 0.00609 * Math.Pow(pf, 0.9) * Math.Pow(g, 0.6) * Math.Pow(Ffu, 0.9) / (Math.Pow(dbt, 0.9) * Math.Pow(ts, 0.1) * Math.Pow(Bp, 0.7)); // eq 3-34
					Reporting.AddLine("tpMin1 = 0.00609 * pf^0.9 * g^0.6 * Ffu^0.9 / (dbt^0.9 * ts^0.1 * bp^0.7)");
					Reporting.AddLine(" = 0.00609 * " + pf + "^0.9 * " + g + "^0.6 * " + Ffu + "^0.9 / (" + dbt + "^0.9 * " + ts + "^0.1 * " + Bp + "^0.7)");
					if (tpMin1 <= beam.WinConnect.MomentEndPlate.Thickness)
						Reporting.AddLine(" = " + tpMin1 + " <= " + beam.WinConnect.MomentEndPlate.Thickness + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine(" = " + tpMin1 + " >> " + beam.WinConnect.MomentEndPlate.Thickness + ConstUnit.Length + " (NG)");

					tpMin2 = 0.00413 * Math.Pow(pf, 0.25) * Math.Pow(g, 0.15) * Ffu / (Math.Pow(dbt, 0.7) * Math.Pow(ts, 0.15) * Math.Pow(Bp, 0.3)); // eq 3-35"
					Reporting.AddLine("tpMin2 = 0.00413 * pf^0.25 * g^0.15 * Ffu / (dbt^0.7 * ts^0.15 * bp^0.3)"); // eq 3-35"
					Reporting.AddLine(" = 0.00413 * " + pf + "^0.25 * " + g + "^0.15 * " + Ffu + " / (" + dbt + "^0.7 * " + ts + "^0.15 * " + Bp + "^0.3) 'eq 3-35");
					if (tpMin2 <= beam.WinConnect.MomentEndPlate.Thickness)
						Reporting.AddLine(" = " + tpMin2 + " <= " + beam.WinConnect.MomentEndPlate.Thickness + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine(" = " + tpMin2 + " >> " + beam.WinConnect.MomentEndPlate.Thickness + ConstUnit.Length + " (NG)");

					Reporting.AddGoToHeader("Beam Web to Plate Weld:");
					w_web = Vf / (2 * (beam.Shape.d - 2 * beam.Shape.kdes) * 0.707 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddLine("Web_weld = Vf / (2 * (db - 2 * k) * 0.707 * 0.6 * Fexx)");
					Reporting.AddLine(" = " + Vf + "/ (2 * (" + beam.Shape.d + " - 2 * " + beam.Shape.kdes + ") * 0.707 * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " = " + w_web + ConstUnit.Length);

					minweld = CommonCalculations.MinimumWeld(beam.Shape.tw, beam.WinConnect.MomentEndPlate.Thickness);
					Reporting.AddLine("Minimum Weld = " + minweld + ConstUnit.Length);
					wmax_beamweb = beam.Material.Fu * beam.Shape.tw / (2 * 0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddLine("wmax_beamweb = Fu * tw / (2 * 0.707 * Fexx)");
					Reporting.AddLine("  = " + beam.Material.Fu + " * " + beam.Shape.tw + " / (2 * 0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddLine(" = " + wmax_beamweb + ConstUnit.Length);
					w = Math.Max(w_web, minweld);
					if (wmax_beamweb > w)
						w = wmax_beamweb;

					Reporting.AddHeader(" Use " + beam.WinConnect.MomentEndPlate.WebWeldRestSize + ConstUnit.Length + "  double-fillet or CJP weld.");
					break;
				case EFemaConnectionType.DST:
					Reporting.AddLine("Tee Stubs and Bolts:");
					Reporting.AddLine("Selected Tee: " + beam.WinConnect.MomentTee.TopTeeShape.Name + " - " + beam.WinConnect.MomentTee.TopMaterial.Name);
					Reporting.AddLine("Length of Tee at Flange: " + beam.WinConnect.MomentTee.TopLengthAtFlange + ConstUnit.Length);
					Reporting.AddLine("Length of Tee at End of Stem: " + beam.WinConnect.MomentTee.TopLengthAtStem + ConstUnit.Length);

					Reporting.AddHeader("Bolts on each Tee Stub:");
					Reporting.AddLine(beam.WinConnect.MomentTee.BoltBeamStem.BoltName + " (4) on Flange + (" + (beam.WinConnect.MomentTee.BoltBeamStem.NumberOfLines * beam.WinConnect.MomentTee.BoltBeamStem.NumberOfLines) + ") on Stem ");
					if (beam.WinConnect.MomentTee.BoltBeamStem.BoltSize >= NumberFun.ConvertFromFraction(14) && beam.WinConnect.MomentTee.BoltBeamStem.BoltSize <= ConstNum.ONE_INCH)
						Reporting.AddLine("Bolt diameter must be between " + NumberFun.ConvertFromFraction(14) + " and " + ConstNum.ONE_INCH + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Bolt diameter must be between " + NumberFun.ConvertFromFraction(14) + " and " + ConstNum.ONE_INCH + ConstUnit.Length + " (NG)");

					if (beam.WinConnect.MomentFlangePlate.Bolt.BoltType == EBoltType.X)
						Reporting.AddLine("Bolt threads excluded from shear plane (OK)");
					else
						Reporting.AddLine("Bolt threads included in shear plane (NG)");

					Reporting.AddHeader("Beam web connection:");
					Reporting.AddLine("Shear tab bolted to web, welded to column");
					Reporting.AddLine("Plate Length = " + beam.WinConnect.ShearWebPlate.Length + ConstUnit.Length);
					Reporting.AddLine("Plate Width = " + beam.WinConnect.ShearWebPlate.Width + ConstUnit.Length);
					Reporting.AddLine("Plate thickness = " + beam.WinConnect.ShearWebPlate.Thickness + ConstUnit.Length);
					if (beam.WinConnect.ShearWebPlate.Thickness < NumberFun.ConvertFromFraction(5) || beam.WinConnect.ShearWebPlate.Thickness > ConstNum.HALF_INCH)
						Reporting.AddLine("Plate thickness must be between " + NumberFun.ConvertFromFraction(5) + " and " + ConstNum.HALF_INCH + ConstUnit.Length + " (NG)");
					else
						Reporting.AddLine("Plate thickness must be between " + NumberFun.ConvertFromFraction(5) + " and " + ConstNum.HALF_INCH + ConstUnit.Length + " (OK)");

					Reporting.AddHeader("Bolts: (" + beam.WinConnect.ShearWebPlate.Bolt.NumberOfBolts + ") " + beam.WinConnect.ShearWebPlate.Bolt.BoltName);

					Reporting.AddHeader("FEMA 350 Design Calculations:");
					Length = beam.WinConnect.MomentTee.TopTeeShape.d;
					L_tee = beam.WinConnect.MomentTee.TopLengthAtFlange; //   Min(Column.BF, Beam.BF)
					Ag = beam.Shape.tf * beam.Shape.bf;

					Reporting.AddHeader("Effective Plastic Modulus of Beam (Ze):");
					Reporting.AddLine("Ag = tf * bf = " + beam.Shape.tf + " * " + beam.Shape.bf + " = " + Ag + ConstUnit.Area);
					An = beam.Shape.tf * (beam.Shape.bf - beam.WinConnect.MomentTee.BoltBeamStem.NumberOfLines * (beam.WinConnect.MomentFlangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH));
					Reporting.AddLine("An = tf * (bf - Nt * (dh + " + ConstNum.SIXTEENTH_INCH + ")) = " + beam.Shape.tf + " *(" + beam.Shape.bf + " -(" + beam.WinConnect.MomentFlangePlate.Bolt.NumberOfLines + " * (" + beam.WinConnect.MomentFlangePlate.Bolt.HoleWidth + " + .0625)) = " + An + ConstUnit.Area);
					Reporting.AddLine("Ae = 0.75 * Fu * An/(0.9 * Fy) <= Ag");
					Ae = 0.75 * beam.Material.Fu * An / (0.9 * beam.Material.Fy);
					Reporting.AddLine("= 0.75 * " + beam.Material.Fu + " * " + An + " / (0.9 * " + beam.Material.Fy + ")");
					if (Ae >= Ag)
					{
						Ae = Ag;
						Reporting.AddLine("= " + Ae + " >= " + Ag + "  Use:  Ae = " + Ag + ConstUnit.Area);
					}
					else
						Reporting.AddLine("= " + Ae + ConstUnit.Area);

					Ze = beam.Shape.zx - (Ag - Ae) * (beam.Shape.d - beam.Shape.tf);
					Reporting.AddLine("Ze = Zb -  (Ag - Ae) * (db - tf)");
					Reporting.AddLine("= " + beam.Shape.zx + " - (" + Ag + " - " + Ae + ") * (" + beam.Shape.d + " - " + beam.Shape.tf + ")");
					Reporting.AddLine("= " + Ze + ConstUnit.SecMod);

					Reporting.AddHeader("Elastic Section Modulus of Beam, Sb:");
					c = beam.WinConnect.MomentTee.BoltBeamStem.NumberOfLines * (beam.WinConnect.MomentFlangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
					Reporting.AddLine("c = Nl * (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + c + ConstUnit.Length);
					Se = beam.Shape.sx - c * (beam.Shape.tf * beam.Shape.d - 2 * Math.Pow(beam.Shape.tf, 2));
					Reporting.AddLine("Sb = S - c * (tf * d - 2 * tf²)");
					Reporting.AddLine("= " + beam.Shape.sx + " - " + c + " * (" + beam.Shape.tf + " * " + beam.Shape.d + " - 2 * " + beam.Shape.tf + "²)");
					Reporting.AddLine("= " + Se + ConstUnit.SecMod);
					femaData.Cpr = Fema.FCpr(memberType); //  (beam.Material.Fy + Beam.Fu) / (2 * beam.Material.Fy) 'eq. 3.2
					femaData.Mpr = Fema.FMpr(memberType); // Cpr * Ry * Ze * beam.Material.Fy 'eq. 3.1
					femaData.Cy = Fema.FCy(memberType); // Beam.s / (Cpr * Ze) ' eq.3.4
					sh = (column.Shape.d / 2 + Length);
					Reporting.AddLine("Sh = dc/ 2 + d_tee = " + column.Shape.d + " / 2 + " + Length + " = " + sh + ConstUnit.Length);
					shf = femaData.shf == -100 ? sh : femaData.shf;

					Reporting.AddLine("Shf = " + shf + ConstUnit.Length);
					L1 = (femaData.L - sh - shf); //    =  L'
					Reporting.AddLine("L' = L- sh - shf = " + femaData.L + " - " + sh + " - " + shf + " = " + L1 + ConstUnit.Length);
					femaData.Pg = femaData.Pg;
					Lp_h = (femaData.Lp - Length);
					Reporting.AddLine("Dist. from Load P to Hinge = " + Lp_h + ConstUnit.Length);
					femaData.Wg = femaData.Wg;
					Vp = Fema.fVp(memberType); // (2 * Mpr + femaData.Pg* Lp + Wg * L1 ^ 2 / 2) / L1 'fig. 3-3
					Vg = Fema.fVg(memberType); // Wg * (femaData.L - Column.d) / 2 + femaData.Pg* (femaData.L - Column.d - Lp) / (femaData.L - Column.d)
					femaData.Mc = Fema.fMc(memberType); // Mpr + Vp * sh 'fig. 3-4
					Lpl = Length;
					femaData.Mf = Fema.fMf(memberType); // Mpr + Vp * Length
					femaData.Myf = Fema.fMyf(memberType); // Cy * Mf 'eq.3.3
					Vst = Fema.fVweb(memberType);
					// Step 3 : Check panel zone
					// Step 4
					n = -((int) Math.Floor(-0.5 * 1.2 * femaData.Myf / (2 * beam.Shape.d * beam.WinConnect.MomentTee.BoltBeamStem.BoltStrength * Math.Pow(beam.WinConnect.MomentTee.BoltBeamStem.BoltSize, 2) * Math.PI / 4))) / 0.5;
					s1 = beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + beam.EndSetback;
					s2 = beam.WinConnect.MomentFlangePlate.Bolt.SpacingLongDir;
					S3 = (n - 1) * s2;
					S4 = beam.WinConnect.MomentTee.TopTeeShape.d - (s1 + S3);
					// step 5
					Reporting.AddHeader("Bolt Check:");
					Ltf1 = (femaData.L - column.Shape.d) / (femaData.L - column.Shape.d - (2 * s1 + S3)); // eq 3-56
					Reporting.AddLine("L_tf1 = (L - dc) / (femaData.L - dc - (2 * S1 + S3)) = (" + femaData.L + " - " + column.Shape.d + ") / (" + femaData.L + " - " + column.Shape.d + " - (2 * " + s1 + " + " + S3 + ")) = " + Ltf1);

					Mfail_Bolts = 2 * n * beam.WinConnect.MomentTee.BoltBeamStem.BoltStrength * Math.Pow(beam.WinConnect.MomentTee.BoltBeamStem.BoltSize, 2) * Math.PI / 4 * beam.Shape.d * Ltf1; // eq 3-55
					Mfail = Mfail_Bolts;
					Reporting.AddHeader("M_fail_bolts = 2 * N * Ab * Fv_bolt * db * Ltf1 'eq 3-43");
					Reporting.AddLine("= 2 * " + n + " * " + Math.Pow(beam.WinConnect.MomentTee.BoltBeamStem.BoltSize, 2) * Math.PI / 4 + " * " + beam.WinConnect.MomentTee.BoltBeamStem.BoltStrength + " * " + beam.Shape.d + " * " + Ltf1); // eq 3-43
					if (Mfail_Bolts > 1.2 * femaData.Myf)
						Reporting.AddLine("= " + Mfail_Bolts + " >> 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (OK)");
					else
						Reporting.AddLine("= " + Mfail_Bolts + " <= 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (NG)");
					// Step 6
					Reporting.AddHeader("Tee stem Fracture:");
					Ltf2 = (femaData.L - column.Shape.d) / (femaData.L - column.Shape.d - 2 * s1); // eq 3-60
					Reporting.AddLine("L_tf2 = (L - dc) / (L - dc - 2 * S1) = (" + femaData.L + " - " + column.Shape.d + ") / (" + femaData.L + " - " + column.Shape.d + " - 2 * " + s1 + ") = " + Ltf2);

					Thetaeff = Math.Max(15, Math.Min(60 * beam.WinConnect.MomentTee.TopTeeShape.tw, 30));
					Reporting.AddHeader("Theta_eff = Max(15; Min(60 * tw_tee; 30)) = max(15; Min(60 * " + beam.WinConnect.MomentTee.TopTeeShape.tw + "; 30)) = " + Thetaeff + " deg.");

					w1 = (beam.WinConnect.MomentTee.TopLengthAtFlange - beam.WinConnect.MomentTee.TopLengthAtStem) / (beam.WinConnect.MomentTee.TopTeeShape.d - beam.WinConnect.MomentTee.TopTeeShape.tf) * (S4 + S3) + beam.WinConnect.MomentTee.TopLengthAtStem;
					Reporting.AddHeader("W1 = (L_flange - L_stem) / (d_tee - tf_tee) * (S4 + S3) + L_stem");
					Reporting.AddLine(" = (" + beam.WinConnect.MomentTee.TopLengthAtFlange + " - " + beam.WinConnect.MomentTee.TopLengthAtStem + ") / (" + beam.WinConnect.MomentTee.TopTeeShape.d + " - " + beam.WinConnect.MomentTee.TopTeeShape.tf + ") * (" + S4 + " + " + S3 + ") + " + beam.WinConnect.MomentTee.TopLengthAtStem);
					Reporting.AddLine(" = " + w1 + ConstUnit.Length);
					w2 = beam.GageOnFlange + S3 * Math.Tan(Thetaeff * Math.PI / 180);
					Reporting.AddLine("W2 = g + S3 * Tan(Theta_eff) = " + beam.GageOnFlange + " + " + S3 + " * Tan(" + Thetaeff * Math.PI / 180 + ") = " + w2 + ConstUnit.Length);
					w = Math.Min(beam.WinConnect.MomentTee.TopLengthAtFlange, Math.Min(w1, w2)); // eq. 3-58
					Reporting.AddLine("W = Min(L_flange; Min(w1, w2)) = Min(" + beam.WinConnect.MomentTee.TopLengthAtFlange + "; Min(" + w1 + ", " + w2 + ")) = " + w + ConstUnit.Length);

					Mfail_stemfracture = beam.WinConnect.MomentTee.TopMaterial.Fu * (w - 2 * (beam.WinConnect.MomentTee.BoltBeamStem.BoltSize + ConstNum.EIGHTH_INCH)) * beam.WinConnect.MomentTee.TopTeeShape.tw * (beam.Shape.d + beam.WinConnect.MomentTee.TopTeeShape.tw) * Ltf2; // eq 3-57
					if (Mfail > Mfail_stemfracture)
						Mfail = Mfail_stemfracture;

					Reporting.AddHeader("Mfail_stemfracture = Fu * (W - 2 * (d_bt + " + ConstNum.EIGHTH_INCH + ")) * tw * (d_bm + tw) * Ltf2"); // eq 3-57
					Reporting.AddLine(" = " + beam.WinConnect.MomentTee.TopMaterial.Fu + " * (" + w + " - 2 * (" + beam.WinConnect.MomentTee.BoltBeamStem.BoltSize + " + " + ConstNum.EIGHTH_INCH + ")) * " + beam.WinConnect.MomentTee.TopTeeShape.tw + " * (" + beam.Shape.d + " + " + beam.WinConnect.MomentTee.TopTeeShape.tw + ") * " + Ltf2); // eq 3-57
					if (Mfail_stemfracture > 1.2 * femaData.Myf)
						Reporting.AddLine("= " + Mfail_stemfracture + " >> 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (OK)");
					else
						Reporting.AddLine("= " + Mfail_stemfracture + " <= 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (NG)");

					Reporting.AddHeader("Plastic Bending of Tee Flange:");
					// Step 7
					g1 = 2 * beam.WinConnect.MomentTee.Column_a + beam.WinConnect.MomentTee.TopTeeShape.tw;
					ap = (beam.WinConnect.MomentTee.TopTeeShape.bf - g1) / 2 + beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize / 2; // eq 3-62
					Reporting.AddLine("a' = (bf_tee - g) / 2 + d_bolt / 2 = (" + beam.WinConnect.MomentTee.TopTeeShape.bf + " - " + g1 + ") / 2 + " + beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize + " / 2  = " + ap); // eq 3-62
					Bp = (g1 - beam.WinConnect.MomentTee.TopTeeShape.tw) / 2 - (beam.WinConnect.MomentTee.TopTeeShape.k1 - beam.WinConnect.MomentTee.TopTeeShape.tw / 2) / 2 - beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize / 2; // eq 3-63
					Reporting.AddLine("b' = (g  - Tee.tw) / 2 - (k1 / 2 - tw / 4) - d_bolt / 2 = (" + g1 + " - " + beam.WinConnect.MomentTee.TopTeeShape.tw + ") / 2 - (" + beam.WinConnect.MomentTee.TopTeeShape.k1 + " / 2 - " + beam.WinConnect.MomentTee.TopTeeShape.tw + " / 4) - " + beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize + " / 2 = " + Bp + ConstUnit.Length); // eq 3-63
					Reporting.AddLine("");
					Mfail_tflange = beam.WinConnect.MomentTee.TopMaterial.Fy * w * Math.Pow(beam.WinConnect.MomentTee.TopTeeShape.tf, 2) * (2 * ap - beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize / 4) * (beam.Shape.d - beam.WinConnect.MomentTee.TopTeeShape.tw) / (4 * ap * Bp - beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize * (ap + Bp)); // eq 3-61
					if (Mfail > Mfail_tflange)
						Mfail = Mfail_tflange;
					Reporting.AddLine("Mfail_tflange = Fy * W * tf ² * (2 * a' - d_bolt / 4) * (d_beam - tw) / (4 * a' * b' - d_bolt * (a' + b'))"); // eq 3-61
					Reporting.AddLine(" = " + beam.WinConnect.MomentTee.TopMaterial.Fy + " * " + w + " * " + beam.WinConnect.MomentTee.TopTeeShape.tf + " ^ 2 * (2 * " + ap + " - " + beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize + " / 4) * (" + beam.Shape.d + " - " + beam.WinConnect.MomentTee.TopTeeShape.tw + ") / (4 * " + ap + " * " + Bp + " - " + beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize + " * (" + ap + " + " + Bp + "))"); // eq 3-61
					if (Mfail_tflange > 1.2 * femaData.Myf)
						Reporting.AddLine("= " + Mfail_tflange + " >> 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (OK)");
					else
						Reporting.AddLine("= " + Mfail_tflange + " <= 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (NG)");
					// Step 8
					Reporting.AddHeader("Tension Bolts:");
					switch (beam.WinConnect.MomentTee.BoltColumnFlange.ASTMType)
					{
						case EBoltASTM.A325:
							Tub = ConstNum.COEFFICIENT_90 * Math.PI * Math.Pow(beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize, 2) / 4;
							Reporting.AddLine("Tub = " + ConstNum.COEFFICIENT_90 + " * Ab = " + ConstNum.COEFFICIENT_90 + " * " + Math.PI * Math.Pow(beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize, 2) / 4 + " = " + Tub + ConstUnit.Force);
							break;
						case EBoltASTM.A490:
							Tub = ConstNum.COEFFICIENT_113 * Math.PI * Math.Pow(beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize, 2) / 4;
							Reporting.AddLine("Tub = " + ConstNum.COEFFICIENT_113 + " * Ab = " + ConstNum.COEFFICIENT_113 + " * " + Math.PI * Math.Pow(beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize, 2) / 4 + " = " + Tub + ConstUnit.Force);
							break;
					}

					Mfail_bolttension = beam.WinConnect.MomentTee.BoltColumnFlange.NumberOfBolts * (beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize + beam.WinConnect.MomentTee.TopTeeShape.tw) * (Tub + w * beam.WinConnect.MomentTee.TopMaterial.Fy * Math.Pow(beam.WinConnect.MomentTee.TopTeeShape.tf, 2) / (16 * ap)) * ap / (ap + Bp); // eq 3-64"
					if (Mfail > Mfail_bolttension)
						Mfail = Mfail_bolttension;
					Reporting.AddHeader("Mfail_bolttension = N * (d_bolt + tw) * (Tub + W * Fy * tf ² / (16 * a')) * a' / (a' + b')"); // eq 3-64
					Reporting.AddLine(" = " + beam.WinConnect.MomentTee.BoltColumnFlange.NumberOfBolts + " * (" + beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize + " + " + beam.WinConnect.MomentTee.TopTeeShape.tw + ") * (" + Tub + " + " + w + " * " + beam.WinConnect.MomentTee.TopMaterial.Fy + " * " + beam.WinConnect.MomentTee.TopTeeShape.tf + " ^ 2 / (16 * " + ap + ")) * " + ap + " / (" + ap + " + " + Bp + ")"); // eq 3-64
					if (Mfail_bolttension > 1.2 * femaData.Myf)
						Reporting.AddLine("= " + Mfail_bolttension + " >> 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (OK)");
					else
						Reporting.AddLine("= " + Mfail_bolttension + " <= 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (NG)");
					// Step 9
					Reporting.AddHeader("Beam FlangeNet section Fracture:");
					Ltf3 = (femaData.L - column.Shape.d) / (femaData.L - column.Shape.d - 2 * (s1 + S3)); // eq 3-66
					Reporting.AddLine("Ltf3 = (L - dc) / (L - cd - 2 * (S1 + S3))"); // eq 3-66
					Reporting.AddLine(" = (" + femaData.L + " - " + column.Shape.d + ") / (" + femaData.L + " - " + column.Shape.d + " - 2 * (" + s1 + " + " + S3 + ")) = " + Ltf3); // eq 3-66
					Mfail_BeamFlange = beam.Material.Fu * (beam.Shape.zx - 2 * (beam.WinConnect.MomentTee.BoltBeamStem.BoltSize + ConstNum.EIGHTH_INCH) * beam.Shape.tf * (beam.Shape.d - beam.Shape.tf)) * Ltf3; // eq 3-65
					if (Mfail > Mfail_BeamFlange)
						Mfail = Mfail_BeamFlange;
					Reporting.AddHeader("Mfail_BeamFlange = (Fu * (Z - 2 * (d_bolt + " + ConstNum.EIGHTH_INCH + ") * tf * (d - tf))) * Ltf3"); // eq 3-65
					Reporting.AddLine(" = (" + beam.Material.Fu + " * (" + beam.Shape.zx + " - 2 * (" + beam.WinConnect.MomentTee.BoltBeamStem.BoltSize + " + " + ConstNum.EIGHTH_INCH + ") * " + beam.Shape.tf + " * (" + beam.Shape.d + "- " + beam.Shape.tf + "))) * " + Ltf3); // eq 3-65
					if (Mfail_BeamFlange > 1.2 * femaData.Myf)
						Reporting.AddLine("= " + Mfail_BeamFlange + " >> 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (OK)");
					else
						Reporting.AddLine("= " + Mfail_BeamFlange + " <= 1.2 * Myf = " + 1.2 * femaData.Myf + ConstUnit.Moment + " (NG)");

					Reporting.AddHeader("Connection Stiffness:");
					Reporting.AddLine("ks = db * Mfail / " + ConstNum.THREE_EIGHTS_INCH + "");
					// Step 10 - Block shear of Tee Stem
					// Step 11
					// Step 12
					tfc_req = femaData.Mf / ((beam.Shape.d - beam.WinConnect.MomentTee.TopTeeShape.tw) * (6 * column.Shape.kdes) * column.Material.Fy); // eq 3-68
					break;
			}
		}

		private static void PrintFema1(EMemberType memberType)
		{
			int s11;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var femaData = beam.WinConnect.Fema;

			Reporting.AddHeader("Connection to Column Flange Using FEMA-350");
			Reporting.AddLine("FEMA-350 Connection Type:");
			Reporting.AddLine(femaData.Connection + " - " + CommonDataStatic.CommonLists.FEMAConnections.First(f => f.Key == femaData.Connection).Value);
			switch (femaData.Connection)
			{
				case EFemaConnectionType.DST:
				case EFemaConnectionType.WUFB:
					if (CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.OMF)
						Reporting.AddLine("Framing Type, " + CommonDataStatic.SeismicSettings.FramingType + " is not Qualified. (NG)");
					else
						Reporting.AddLine("Framing Type, " + CommonDataStatic.SeismicSettings.FramingType + " is Qualified. (OK)");
					break;
				default:
					if (CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.OMF || CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.SMF)
						Reporting.AddLine("Framing Type, " + CommonDataStatic.SeismicSettings.FramingType + " is not Qualified. (NG)");
					else
						Reporting.AddLine("Framing Type, " + CommonDataStatic.SeismicSettings.FramingType + " is Qualified. (OK)");
					break;
			}

			Reporting.AddHeader("Beam and Column Requirements:");
			Reporting.AddLine("Column: " + column.ShapeName + " - " + column.Material.Name);
			s11 = column.ShapeName.Count(x => x == 'X') - 1;
			if (s11 < 0)
				s11 = 0;
			if (s11 > 0 && femaData.ColumnDepth == "ALL")
				Reporting.AddLine("Column Depth is Qualified. (OK)");
			else
				Reporting.AddLine("Column Depth is not Qualified. (NG)");

			if (femaData.ColumnMaterialSpec.Contains(column.Material.Name))
				Reporting.AddLine("Column Material is Qualified. (OK)");
			else
				Reporting.AddLine("Column Material is not Qualified. (NG)");

			if (beam.MemberType == EMemberType.LeftBeam)
				Reporting.AddLine("Left Side Beam: " + beam.ShapeName + " - " + beam.Material.Name);
			else
				Reporting.AddLine("Right Side Beam: " + beam.ShapeName + " - " + beam.Material.Name);

			if (CommonDataStatic.Units == EUnit.US)
			{
				Reporting.AddHeader("Beam Span Length CL-to-CL of Columns,femaData.L= " + (femaData.L / 12) + " ft");
				Reporting.AddHeader("Average of Story Heights Above and Below the Joint, H = " + (femaData.H / 12) + " ft");
			}
			else
			{
				Reporting.AddHeader("Beam Span Length CL-to-CL of Columns,femaData.L= " + (femaData.L / 1000) + " m");
				Reporting.AddHeader("Average of Story Heights Above and Below the Joint, H = " + (femaData.H / 1000) + " m");
			}

			if (femaData.BeamMaterialSpec.Contains(leftBeam.Material.Name))
				Reporting.AddLine("Beam Material is Qualified. (OK)");
			else
				Reporting.AddLine("Beam Material is not Qualified. (NG)");

			if (femaData.MaximumBeamFlangeThickness >= beam.Shape.tf)
				Reporting.AddLine("Beam Flange Thickness = " + beam.Shape.tf + " <= Max. thickness = " + femaData.MaximumBeamFlangeThickness + ConstUnit.Length + "Qualified. (OK)");
			else
				Reporting.AddLine("Beam Flange Thickness = " + beam.Shape.tf + " >> Max. thickness = " + femaData.MaximumBeamFlangeThickness + ConstUnit.Length + "not Qualified. (NG)");

			if (femaData.MinimumSpanToDepthRatio <= femaData.L / beam.Shape.d)
				Reporting.AddLine("Beam Span/Depth Ratio is Qualified. (OK)");
			else
				Reporting.AddLine("Beam Span/Depth Ratio is not Qualified. (NG)");

			Reporting.AddLine("Expected Yield strength Ratio:");
			Reporting.AddLine("Ry = " + femaData.Ry + " Ryc = " + femaData.Ryc);

			Reporting.AddLine("Factored Gravity Loads on Beam:");
			if (CommonDataStatic.Units == EUnit.US)
			{
				Reporting.AddHeader("Uniform Load, femaData.Wg = " + (12 * femaData.Wg) + " k/ft");
				Reporting.AddHeader("Distance from load P to Col. Face, Lp = " + (femaData.Lp / 12) + " ft");
			}
			else
			{
				Reporting.AddHeader("Uniform Load, femaData.Wg = " + femaData.Wg + " kN/m");
				Reporting.AddHeader("Distance from load P to Col. Face, Lp = " + (femaData.Lp / 1000) + " m");
				Reporting.AddHeader("Resultant of Concentrated Loads, P = " + (femaData.Pg / 1000) + " kN");
			}
		}
	}
}