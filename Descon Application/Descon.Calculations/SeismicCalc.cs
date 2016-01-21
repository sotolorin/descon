using System;
using System.Windows;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class SeismicCalc
	{
		public static bool CheckSeismicSelections()
		{
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

			if (rightBeam.IsActive)
			{
				if (CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight <= 0 ||
					CommonDataStatic.SeismicSettings.GravityLoadRight <= 0 ||
					CommonDataStatic.SeismicSettings.DistanceToGravityLoadRight <= 0)
				{
					MessageBox.Show("All right side data must be entered.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
					return false;
				}
			}

			if (leftBeam.IsActive)
			{
				if (CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft <= 0 ||
					CommonDataStatic.SeismicSettings.GravityLoadLeft <= 0 ||
					CommonDataStatic.SeismicSettings.DistanceToGravityLoadLeft <= 0)
				{
					MessageBox.Show("All left side data must be entered.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
					return false;
				}
			}

			return true;
		}

		public static void ApplySeismicData()
		{
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

			rightBeam.WinConnect.MomentEndPlate.Material = CommonDataStatic.SeismicSettings.Material;
			rightBeam.WinConnect.ShearWebPlate.Material = CommonDataStatic.SeismicSettings.Material;

			leftBeam.WinConnect.MomentEndPlate.Material = CommonDataStatic.SeismicSettings.Material;
			leftBeam.WinConnect.ShearWebPlate.Material = CommonDataStatic.SeismicSettings.Material;

			CommonDataStatic.ColumnStiffener.Material = CommonDataStatic.SeismicSettings.Material;

			if (CommonDataStatic.SeismicSettings.DesignType == ESeismicDesignType.AISC358)
			{
				if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.RBS)
					RBS_Design_358();
				else
					SeismicEndPlateCalc.SeismicEndPlate();
			}

			if (rightBeam.MomentConnection == EMomentCarriedBy.EndPlate)
				rightBeam.WinConnect.MomentEndPlate.FlangeWeldType = EWeldType.CJP;

			if (leftBeam.MomentConnection == EMomentCarriedBy.EndPlate)
				leftBeam.WinConnect.MomentEndPlate.FlangeWeldType = EWeldType.CJP;
		}

		private static void RBS_Design_358()
		{
			double C_B_Ratio = 0;
			double L_Vu_web = 0;
			double L_Vu_beam = 0;
			string momentunit = "";
			double R_Vu_web = 0;
			double h = 0;
			double R_Vu_beam = 0;
			double WeightPerFoot = 0;
			double LimitingRatio = 0;
			double bovt = 0;
			double L_Mv = 0;
			double R_Mv = 0;
			double Pac = 0;
			double Puc = 0;
			double TwcMin = 0;
			double FiRn = 0;
			double Rn = 0;
			double tw = 0;
			double Pc = 0;
			double pr = 0;
			double PannelZoneShear = 0;
			double MinReq_tfc = 0;
			double tcfLimit1 = 0;
			double Ryc = 0;
			double RRyb = 0;
			double LRyb = 0;
			double L_CJP_WeldLength = 0;
			double L_HightOFAccessHole = 0;
			double L_Vg = 0;
			double L_aP = 0;
			double L_bP = 0;
			double L_cP = 0;
			double L_Mpe = 0;
			double L_VRBS = 0;
			double L_Mpr = 0;
			double L_Cpr = 0;
			double L_Ze = 0;
			double L_c_max = 0;
			double L_b_min = 0;
			double L_a_min = 0;
			double R_CJP_WeldLength = 0;
			double R_HightOFAccessHole = 0;
			double R_Vg = 0;
			double R_aP = 0;
			double R_bP = 0;
			double R_cP = 0;
			double R_Mpe = 0;
			double R_VRBS = 0;
			double P = 0;
			double b1 = 0;
			double a1 = 0;
			double L_prime = 0;
			double R_Mpr = 0;
			double R_Cpr = 0;
			double R_Ze = 0;
			double R_c_max = 0;
			double R_b_min = 0;
			double R_a_min = 0;
			double LambdaPW = 0;
			double ratio = 0;
			double LambdaPF = 0;
			double Pa = 0;
			double Ca = 0;
			double LMav = 0;
			double RMav = 0;
			double Pac_required = 0;
			double LMuv = 0;
			double L_zRBS = 0;
			double RMuv = 0;
			double R_ZRBS = 0;
			double Sum_Mpb = 0;
			double Sum_Mpc = 0;
			double minColumn_tw = 0;
			double Wz = 0;
			double dz = 0;
			double t = 0;
			double ColumnDepth = 0;
			double WeightRatio = 0;
			double Puc_required = 0;
			double Pu = 0;
			double Py = 0;
			double Ry_Lb = 0;
			double Ry_Rb = 0;
			double pg = 0;
			double tp_forFilletweld = 0;

			if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.OMF)
				return;

			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var stiffener = CommonDataStatic.ColumnStiffener;
			var seismic = CommonDataStatic.SeismicSettings;

			Pu = column.P;
			switch (CommonDataStatic.Units)
			{
				case EUnit.US:
					WeightRatio = 1;
					break;
				case EUnit.Metric:
					WeightRatio = 1.49;
					break;
			}

			if (seismic.FramingType == EFramingSystem.SMF)
			{
				t = column.Shape.tw;
				dz = Math.Max(rightBeam.Shape.d - rightBeam.Shape.tf, leftBeam.Shape.d - leftBeam.Shape.tf);
				Wz = column.Shape.t;
				minColumn_tw = (dz + Wz) / 90;
			}

			// Section 9.6 Equation 9-3
			if (seismic.FramingType == EFramingSystem.SMF)
			{
				if (CommonDataStatic.Preferences.CalcMode == ECalcMode.LRFD)
				{
					Sum_Mpc = 2 * column.Shape.zx * (column.Material.Fy - Puc_required / column.Shape.a);
					Sum_Mpb = 1.1 * Ry_Rb * rightBeam.Material.Fy * R_ZRBS + RMuv + (1.1 * Ry_Lb * leftBeam.Material.Fy * L_zRBS + LMuv);
				}
				else if (CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD)
				{
					Sum_Mpc = 2 * column.Shape.zx * (column.Material.Fy / 1.5 - Pac_required / column.Shape.a);
					Sum_Mpb = 1.1 / 1.5 * Ry_Rb * rightBeam.Material.Fy * R_ZRBS + RMav + (1.1 * Ry_Lb * leftBeam.Material.Fy * L_zRBS + LMav);
				}
			}

			// Table I-8-1
			if (CommonDataStatic.Preferences.CalcMode == ECalcMode.LRFD)
				Ca = Pu / (0.9 * Py);
			else if (CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD)
				Ca = 1.67 * Pa / Py;

			if (Sum_Mpc / Sum_Mpb > 2)
			{
				//  Specification Table 4-1
				LambdaPF = 0.38 * Math.Pow(ConstNum.ELASTICITY / column.Material.Fy, 0.5);
				ratio = column.Shape.bf / column.Shape.tf / 2;

				LambdaPW = 3.76 * Math.Pow(ConstNum.ELASTICITY / column.Material.Fy, 0.5); //  Table I-8-1 footnote k,j
				ratio = column.Shape.t / column.Shape.tw;

				// STEP 1: Beam Flange CutOut:Eq: 5.8-1,2,3
				if (rightBeam.IsActive)
				{
					R_a_min = 0.5 * rightBeam.Shape.bf;
					R_b_min = 0.65 * rightBeam.Shape.d;
					R_c_max = 0.25 * rightBeam.Shape.bf;

					seismic.R_a = R_a_min;
					seismic.R_b = R_b_min * 1.1F;
					seismic.R_c = R_c_max;

					// STEP 2 EQ 5.8-4
					R_Ze = rightBeam.Shape.zx - 2 * seismic.R_c * rightBeam.Shape.tf * (rightBeam.Shape.d - rightBeam.Shape.tf);
					seismic.RReducedSectionZ = R_Ze;
					seismic.RHingeDistance = seismic.R_a + seismic.R_b / 2;

					// STEP 3 EQ:5.8-5
					rightBeam.WinConnect.Fema.Ry = rightBeam.Material.Ry;
					R_Cpr = (rightBeam.Material.Fy + rightBeam.Material.Fu) / (2 * rightBeam.Material.Fy);
					R_Mpr = R_Cpr * rightBeam.WinConnect.Fema.Ry * rightBeam.Material.Fy * R_Ze;

					// STEP 4 Shear force at RBS center
					L_prime = seismic.ClearSpanOfBeamRight - 2 * (seismic.R_a + seismic.R_b / 2);
					rightBeam.WinConnect.Fema.sh = (seismic.R_a + seismic.R_b / 2);
					a1 = seismic.DistanceToGravityLoadRight - rightBeam.WinConnect.Fema.sh;
					b1 = L_prime - a1;
					P = seismic.GravityLoadRight;
					R_VRBS = Math.Max((P * b1 + 2 * R_Mpr) / L_prime, (P * a1 - 2 * R_Mpr) / L_prime);

					// STEP 5: EQ 5.8-6
					seismic.R_Mf = R_Mpr + R_VRBS * (seismic.R_a + seismic.R_b / 2);

					// STEP 6  EQ 5.8-7
					R_Mpe = rightBeam.Shape.zx * rightBeam.WinConnect.Fema.Ry * rightBeam.Material.Fy;

					// STEP 7  EQ 5.8-8
					if (seismic.R_Mf > ConstNum.FIOMEGA0_9N * R_Mpe)
					{
						R_cP = seismic.R_c;
						seismic.R_c = 1.1F * seismic.R_c;
						if (seismic.R_c >= R_c_max)
						{
							seismic.R_c = R_cP;
							R_bP = seismic.R_b;
							seismic.R_b = seismic.R_b / 1.01F;
						}
						if (seismic.R_b < R_b_min)
						{
							seismic.R_b = R_bP;
							R_aP = seismic.R_a;
							seismic.R_a = seismic.R_a / 1.01F;
						}
						if (seismic.R_a < R_a_min)
							seismic.R_a = R_aP;
					}
					// Loop While R_Mfis = "NG" And (R_c <= R_c_max / 1.1 Or R_b >= R_b_min * 1.01 Or R_a >= R_a_min * 1.01)
					seismic.R_Radius = (4 * Math.Pow(seismic.R_c, 2) + Math.Pow(seismic.R_b, 2)) / (8 * seismic.R_c);

					// STEP 8 EQ 5.8-9
					P = seismic.GravityLoadRight;
					a1 = seismic.DistanceToGravityLoadRight;
					b1 = seismic.ClearSpanOfBeamRight - a1;
					R_Vg = Math.Max(P * b1 / seismic.ClearSpanOfBeamRight, P * a1 / seismic.ClearSpanOfBeamRight);
					seismic.R_Vu = 2 * R_Mpr / (seismic.ClearSpanOfBeamRight - 2 * (seismic.R_a + seismic.R_b / 2)) + R_Vg;
					rightBeam.ShearForce = seismic.R_Vu;

					// STEP 9 BeamWeb to Column Conn.
					R_HightOFAccessHole = Math.Max(1.5 * rightBeam.Shape.tw, ConstNum.ONE_INCH);
					R_HightOFAccessHole = Math.Max(R_HightOFAccessHole, ConstNum.TWO_INCHES);
				}

				if (leftBeam.IsActive)
				{
					leftBeam.EndSetback = 0;

					// STEP 1: Beam Flange CutOut:Eq: 5.8-1,2,3
					L_a_min = 0.5 * leftBeam.Shape.bf;
					L_b_min = 0.65 * leftBeam.Shape.d;
					L_c_max = 0.25 * leftBeam.Shape.bf;

					seismic.L_a = L_a_min;
					seismic.L_b = L_b_min * 1.1F;
					seismic.L_c = L_c_max;
					seismic.L_Radius = (4 * Math.Pow(seismic.L_c, 2) + Math.Pow(seismic.L_b, 2)) / (8 * seismic.L_c);

					// STEP 2 EQ 5.8-4
					L_Ze = leftBeam.Shape.zx - 2 * seismic.L_c * leftBeam.Shape.tf * (leftBeam.Shape.d - leftBeam.Shape.tf);
					seismic.LReducedSectionZ = L_Ze;
					seismic.LHingeDistance = seismic.L_a + seismic.L_b / 2;

					// STEP 3 EQ:5.8-5
					leftBeam.WinConnect.Fema.Ry = leftBeam.Material.Ry;
					L_Cpr = (leftBeam.Material.Fy + leftBeam.Material.Fu) / (2 * leftBeam.Material.Fy);
					L_Mpr = L_Cpr * leftBeam.WinConnect.Fema.Ry * leftBeam.Material.Fy * L_Ze;

					// STEP 4 Shear force at RBS center
					L_prime = seismic.ClearSpanOfBeamLeft - 2 * (seismic.L_a + seismic.L_b / 2);
					leftBeam.WinConnect.Fema.sh = (seismic.L_a + seismic.L_b / 2);
					a1 = seismic.DistanceToGravityLoadLeft - leftBeam.WinConnect.Fema.sh;
					b1 = L_prime - a1;
					P = seismic.GravityLoadLeft;
					L_VRBS = Math.Max((P * b1 + 2 * L_Mpr) / L_prime, (P * a1 - 2 * L_Mpr) / L_prime);

					// STEP 5: EQ 5.8-6If LBeam.Shape <> "None" Then
					seismic.L_Mf = L_Mpr + L_VRBS * (seismic.L_a + seismic.L_b / 2);

					// STEP 6  EQ 5.8-7
					leftBeam.WinConnect.Fema.Ry = leftBeam.Material.Ry;
					L_Mpe = leftBeam.Shape.zx * leftBeam.WinConnect.Fema.Ry * leftBeam.Material.Fy;

					// STEP 7  EQ 5.8-8
					if (seismic.L_Mf > ConstNum.FIOMEGA0_9N * L_Mpe)
					{
						L_cP = seismic.L_c;
						seismic.L_c = 1.1F * seismic.L_c;

						seismic.L_c = 1.1F * seismic.L_c;
						if (seismic.L_c >= L_c_max)
						{
							seismic.L_c = L_cP;
							L_bP = seismic.L_b;
							seismic.L_b = seismic.L_b / 1.01F;
						}
						if (seismic.L_b < L_b_min)
						{
							seismic.L_b = L_bP;
							L_aP = seismic.L_a;
							seismic.L_a = seismic.L_a / 1.01F;
						}
						if (seismic.L_a < L_a_min)
							seismic.L_a = L_aP;
					}
					// Loop While L_Mfis = "NG" And L_c <= L_c_Max
					seismic.L_Radius = (4 * Math.Pow(seismic.L_c, 2) + Math.Pow(seismic.L_b, 2)) / (8 * seismic.L_c);

					// STEP 8 EQ 5.8-9
					P = seismic.GravityLoadLeft;
					a1 = seismic.DistanceToGravityLoadLeft;
					b1 = seismic.ClearSpanOfBeamLeft - a1;
					L_Vg = Math.Max(P * b1 / seismic.ClearSpanOfBeamLeft, P * a1 / seismic.ClearSpanOfBeamLeft);
					seismic.L_Vu = 2 * L_Mpr / (seismic.ClearSpanOfBeamLeft - 2 * (seismic.L_a + seismic.L_b / 2)) + L_Vg;
					leftBeam.ShearForce = seismic.L_Vu;

					// STEP 9 BeamWeb to Column Conn.
					L_HightOFAccessHole = Math.Max(1.5 * leftBeam.Shape.tw, ConstNum.ONE_INCH);
					L_HightOFAccessHole = Math.Min(L_HightOFAccessHole, ConstNum.TWO_INCHES);
					L_CJP_WeldLength = leftBeam.Shape.d - 2 * (leftBeam.Shape.tf + L_HightOFAccessHole);
					seismic.MaxL = L_CJP_WeldLength;
					seismic.MinL = leftBeam.Shape.d / 2;
				}

				// Step 10 continuity plates Chapter 2
				// tcfLimit= 0.4*(1.8bbf*tbf*Fyb*Ryb/(Fyc*Ryc))^0.5 EQ 2.4.4-1
				LRyb = leftBeam.Material.Ry;
				RRyb = rightBeam.Material.Ry;
				Ryc = rightBeam.Material.Ry;
				tcfLimit1 = 0.4 * Math.Pow(1.8 * Math.Max(rightBeam.Shape.bf, leftBeam.Shape.bf) * Math.Max(rightBeam.Shape.tf, leftBeam.Shape.tf) * Math.Max(rightBeam.Material.Fy * RRyb, leftBeam.Material.Fy * LRyb) / (column.Material.Fy * Ryc), 0.5);

				if (!stiffener.SThickness_User && column.Shape.tf < tcfLimit1)
				{
					MinReq_tfc = Math.Max(rightBeam.Shape.bf, leftBeam.Shape.bf) / 6; //  EQ 2.4.4.2
					if (column.Shape.tf < MinReq_tfc)
						stiffener.SThickness = Math.Max(rightBeam.Shape.tf, leftBeam.Shape.tf);
				}

				// Step 11 Panel Zone Section 5.4 of 358 and 9.3 or 10.3 of 341, also Section J10.6 of 360-05
				if (rightBeam.IsActive && leftBeam.IsActive)
					PannelZoneShear = seismic.R_Mf / (rightBeam.Shape.d - rightBeam.Shape.tf) + seismic.L_Mf / (leftBeam.Shape.d - leftBeam.Shape.tf);
				else if (rightBeam.IsActive)
					PannelZoneShear = seismic.R_Mf / (rightBeam.Shape.d - rightBeam.Shape.tf);
				else if (rightBeam.IsActive)
					PannelZoneShear = seismic.L_Mf / (leftBeam.Shape.d - leftBeam.Shape.tf);

				pr = column.P;
				if (CommonDataStatic.Preferences.CalcMode == ECalcMode.LRFD)
					Pc = column.Material.Fy * column.Shape.a;
				else if (CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD)
					Pc = 0.6 * column.Material.Fy * column.Shape.a;

				tw = column.Shape.tw - ConstNum.SIXTEENTH_INCH;
				do
				{
					tw = tw + ConstNum.SIXTEENTH_INCH;
					if (seismic.PanelZoneDeformation == EPanelZoneDeformation.IsConsidered)
					{
						if (pr <= 0.75 * Pc)
							Rn = 0.6 * column.Material.Fy * column.Shape.d * tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * tw)); // J10-11
						else
							Rn = 0.6 * column.Material.Fy * column.Shape.d * tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * tw)) * (1.9 - 1.2 * pr / Pc); // J10-12
					}
					else
					{
						if (pr <= 0.4 * Pc)
							Rn = 0.6 * column.Material.Fy * column.Shape.d * tw; // J10-9
						else
							Rn = 0.6 * column.Material.Fy * column.Shape.d * tw * (1.4 - pr / Pc); // J10-10
					}

					FiRn = ConstNum.FIOMEGA1_0N * Rn;
				} while (FiRn < PannelZoneShear);
				if (tw > column.Shape.tw)
					stiffener.DThickness = (tw - column.Shape.tw) * column.Material.Fy / stiffener.Material.Fy;

				dz = Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) - stiffener.SThickness;
				Wz = column.Shape.d - 2 * column.Shape.tf;
				TwcMin = (dz + Wz) / 90; // EQ 9-2
				if (!stiffener.DNumberOfPlates_User && !stiffener.DThickness_User)
				{
					if (stiffener.DThickness >= TwcMin)
					{
						if (stiffener.DThickness > 2 * TwcMin)
						{
							stiffener.DNumberOfPlates = 2;
							stiffener.DThickness = 2 * stiffener.DThickness / 2;
							stiffener.DThickness2 = stiffener.DThickness;
						}
						else
							stiffener.DNumberOfPlates = 1;
					}
					else
					{
						stiffener.DThickness = TwcMin;
						stiffener.DNumberOfPlates = 1;
					}
				}

				
				// Moved from SeismicDoublerPlateReport() (MT - 7/9/15)
				double fr;
				double encroach;
				double re;

				fr = column.Shape.kdet - column.Shape.tf;
				if (CommonDataStatic.Units == EUnit.US)
					encroach = Math.Pow(3.0 / 32 * fr - Math.Pow(3.0 / 64, 2), 0.5); //  fillet encroachment
				else
					encroach = Math.Pow(3.0 / 32 * 25.4 * fr - Math.Pow(3.0 / 64 * 25.4, 2), 0.5); //  fillet encroachment

				re = NumberFun.Round(encroach, 16);
				tp_forFilletweld = fr - re; // EQ 4.4-4

				if (!stiffener.DThickness_User)
				{
					while (tp_forFilletweld > stiffener.DThickness)
						stiffener.DThickness += ConstNum.SIXTEENTH_INCH;
				}

				stiffener.DThickness = NumberFun.Round(stiffener.DThickness, 16);

				// Step 12 Column-Beam moment ratio Section 5.4
				if (seismic.FramingType == EFramingSystem.SMF)
				{
					switch (CommonDataStatic.Preferences.CalcMode)
					{
						case ECalcMode.LRFD:
							Puc = column.P;
							Sum_Mpc = 2 * column.Shape.zx * (column.Material.Fy - Puc / column.Shape.a);
							break;
						case ECalcMode.ASD:
							Pac = column.P;
							Sum_Mpc = 2 * column.Shape.zx * (column.Material.Fy / 1.5 - Pac / column.Shape.a);
							break;
					}
					R_Mv = seismic.R_Mf + R_VRBS * column.Shape.d / 2;
					L_Mv = seismic.L_Mf + L_VRBS * column.Shape.d / 2;
				}

				Reporting.AddMainHeader("Beam Connection to Column Flange");
				Reporting.AddHeader("Column: " + column.Shape.Name + " - " + column.Material.Name);
				Reporting.AddLine("Prequalified Seismic Design Using ANSI/AISC 358-05");
				Reporting.AddLine("Connection Type: RBS");
				if (seismic.FramingType == EFramingSystem.SMF)
					Reporting.AddLine("Moment Frame Type: SMF");
				else if (seismic.FramingType == EFramingSystem.IMF)
					Reporting.AddLine("Moment Frame Type: IMF");

				if (leftBeam.IsActive)
				{
					Reporting.AddHeader("Left Side Beam: " + leftBeam.Shape.Name + " - " + leftBeam.Material.Name);
					Reporting.AddLine("Moment: " + (leftBeam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
					Reporting.AddLine("Shear: " + leftBeam.ShearForce + ConstUnit.Force);
					Reporting.AddLine("Axial Force: " + leftBeam.P + ConstUnit.Force);
				}
				if (rightBeam.IsActive)
				{
					Reporting.AddHeader("Right Side Beam: " + rightBeam.Shape.Name + " - " + rightBeam.Material.Name);
					Reporting.AddLine("Moment: " + (rightBeam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
					Reporting.AddLine("Shear: " + rightBeam.ShearForce + ConstUnit.Force);
					Reporting.AddLine("Axial Force: " + rightBeam.P + ConstUnit.Force);
				}

				Reporting.AddHeader("Column Limitations");
				Reporting.AddLine("Columns Shall be rolled Shapes or Built-up sections Satisfying the requirements");
				Reporting.AddLine("of Section 2.3 of ANSI/AISC 358-05");

				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange)
					Reporting.AddLine("Beam Connected to Column Flange (OK)");
				else
					Reporting.AddLine("Beam not Connected to Column Flange (NG)");

				ColumnDepth = column.Shape.d;

				if (ColumnDepth <= 36 * ConstNum.ONE_INCH)
					Reporting.AddLine("Column depth = " + ColumnDepth + " <= " + (36 * ConstNum.ONE_INCH) + ConstUnit.Length + "(OK)");
				else
					Reporting.AddLine("Column depth = " + ColumnDepth + " >> " + (36 * ConstNum.ONE_INCH) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Column Flange Width/Thickness Ratio:");
				bovt = column.Shape.bf / 2 / column.Shape.tf;
				Reporting.AddLine("b/tf = " + column.Shape.bf + " /2/" + column.Shape.tf);
				Reporting.AddLine("= " + bovt);

				LimitingRatio = 0.3 * Math.Pow(ConstNum.ELASTICITY / column.Material.Fy, 0.5);
				Reporting.AddHeader("LimitingRatio = 0.30 * (E / Fy)^0.5");
				if (LimitingRatio >= bovt)
					Reporting.AddLine("= " + LimitingRatio + " >= " + bovt + " (OK)");
				else
					Reporting.AddLine("= " + LimitingRatio + " << " + bovt + " (NG)");

				if (rightBeam.IsActive)
				{
					Reporting.AddHeader("Right Side Beam:");
					Reporting.AddLine(rightBeam.Shape.Name + " - " + rightBeam.Material.Name);
					Reporting.AddLine("Clear Length of Beam: " + seismic.ClearSpanOfBeamRight + ConstUnit.Length);
					Reporting.AddLine("Moment: " + (rightBeam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
					Reporting.AddLine("Shear: " + rightBeam.ShearForce + ConstUnit.Force);
					Reporting.AddLine("Axial Force: " + rightBeam.P + ConstUnit.Force);
					Reporting.AddLine("Gravity Load: " + seismic.GravityLoadRight + ConstUnit.Force);
				}

				if (leftBeam.IsActive)
				{
					Reporting.AddHeader("Left Side Beam:");
					Reporting.AddLine(leftBeam.Shape.Name + " - " + leftBeam.Material.Name);
					Reporting.AddLine("Clear Length of Beam: " + seismic.ClearSpanOfBeamLeft + ConstUnit.Length);
					Reporting.AddLine("Moment: " + (leftBeam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
					Reporting.AddLine("Shear: " + leftBeam.ShearForce + ConstUnit.Force);
					Reporting.AddLine("Axial Force: " + leftBeam.P + ConstUnit.Force);
					Reporting.AddLine("Gravity Load: " + seismic.GravityLoadLeft + ConstUnit.Force);
				}

				// 1)RBEAM LIMITATIONS
				if (rightBeam.IsActive)
				{
					Reporting.AddHeader("Right Side Beam Requirements:");

					if (rightBeam.Shape.d <= 37.3 * ConstNum.ONE_INCH)
						Reporting.AddLine("Beam Depth = " + rightBeam.Shape.d + " << " + 37.3 * ConstNum.ONE_INCH + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Beam Depth = " + rightBeam.Shape.d + " << " + 37.3 * ConstNum.ONE_INCH + ConstUnit.Length + " (NG)");

					WeightPerFoot = rightBeam.Shape.weight;
					if (WeightPerFoot <= 300 * WeightRatio)
						Reporting.AddLine("Beam Weight = " + WeightPerFoot + " << " + 300 * WeightRatio + ConstUnit.ForceUniform + " (OK)");
					else
						Reporting.AddLine("Beam Weight = " + WeightPerFoot + " >> " + 300 * WeightRatio + ConstUnit.ForceUniform + " (NG)");

					rightBeam.WinConnect.Fema.L = seismic.ClearSpanOfBeamRight;
					switch (seismic.FramingType)
					{
						case EFramingSystem.SMF:
							if (rightBeam.WinConnect.Fema.L / rightBeam.Shape.d >= 7)
								Reporting.AddLine("Beam Span to depth Ratio (L/d) = " + rightBeam.WinConnect.Fema.L + " / " + rightBeam.Shape.d + " = " + rightBeam.WinConnect.Fema.L / rightBeam.Shape.d + " >=  7 (OK)");
							else
								Reporting.AddLine("Beam Span to depth Ratio (L/d) = " + rightBeam.WinConnect.Fema.L + " / " + rightBeam.Shape.d + " = " + rightBeam.WinConnect.Fema.L / rightBeam.Shape.d + " <<  7 (NG)");
							break;
						case EFramingSystem.IMF:
							if (rightBeam.WinConnect.Fema.L / rightBeam.Shape.d >= 5)
								Reporting.AddLine("Beam Span to depth Ratio (L/d) = " + rightBeam.WinConnect.Fema.L + " / " + rightBeam.Shape.d + " = " + rightBeam.WinConnect.Fema.L / rightBeam.Shape.d + " >=  5 (OK)");
							else
								Reporting.AddLine("Beam Span to depth Ratio (L/d) = " + rightBeam.WinConnect.Fema.L + " / " + rightBeam.Shape.d + " = " + rightBeam.WinConnect.Fema.L / rightBeam.Shape.d + " <<  5 (NG)");
							break;
					}

					if (rightBeam.Shape.tf <= 1.75 * ConstNum.ONE_INCH)
						Reporting.AddLine("Beam Flange thickness = " + rightBeam.Shape.tf + " <= " + 1.75 * ConstNum.ONE_INCH + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Beam Flange thickness = " + rightBeam.Shape.tf + " >> " + 1.75 * ConstNum.ONE_INCH + ConstUnit.Length + " (NG)");

					LambdaPF = 0.38 * Math.Pow(ConstNum.ELASTICITY / rightBeam.Material.Fy, 0.5);
					Reporting.AddLine("Limiting b/t = 0.38 * (E / Fy)^0.5 = 0.38 * (" + ConstNum.ELASTICITY + " / " + rightBeam.Material.Fy + ")^0.5 = " + LambdaPF);
					ratio = rightBeam.Shape.bf / rightBeam.Shape.tf / 2;
					if (ratio <= LambdaPF)
						Reporting.AddLine("Beam Flange b/t = " + (rightBeam.Shape.bf / 2) + " / " + rightBeam.Shape.tf + " = " + ratio + " <= " + LambdaPF + " (OK)");
					else
						Reporting.AddLine("Beam Flange b/t = " + (rightBeam.Shape.bf / 2) + " / " + rightBeam.Shape.tf + " = " + ratio + " >> " + LambdaPF + " (NG)");

					LambdaPW = 3.76 * Math.Pow(ConstNum.ELASTICITY / rightBeam.Material.Fy, 0.5);
					Reporting.AddLine("Limiting h/tw = 3.76 * (E / Fy)^0.5 = 3.76 * (" + ConstNum.ELASTICITY + " / " + rightBeam.Material.Fy + ")^0.5 = " + LambdaPW);
					ratio = rightBeam.Shape.t / rightBeam.Shape.tw;
					if (ratio <= LambdaPW)
						Reporting.AddLine("Beam Web h/t = " + rightBeam.Shape.t + " / " + rightBeam.Shape.tw + " = " + ratio + " <= " + LambdaPW + " (OK)");
					else
						Reporting.AddLine("Beam Web h/t = " + rightBeam.Shape.t + " / " + rightBeam.Shape.tw + " = " + ratio + " >> " + LambdaPW + " (NG)");
				}

				// 3)COLUMN LIMITATIONS
				// 4)COLUMN BEAM RELATIONSHIPS
				// 5)RBEAM RBS CALCS
				if (rightBeam.IsActive)
				{
					Reporting.AddHeader("Right Side Beam RBS Calcs:");
					Reporting.AddHeader("Flange Cutout Parameters:");

					if (0.5 * rightBeam.Shape.bf <= seismic.R_a && seismic.R_a <= 0.75 * rightBeam.Shape.bf)
						Reporting.AddLine("0.5 * Bf = " + (rightBeam.Shape.bf / 2) + " <=  a = " + seismic.R_a + " <=  0.75 * Bf = " + 0.75 * rightBeam.Shape.bf + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Distance a = " + seismic.R_a + " is not within the range, 0.5Bf ~ 0.75Bf  (  NG)");

					if (0.65 * rightBeam.Shape.d <= seismic.R_b && seismic.R_b <= 0.85 * rightBeam.Shape.d)
						Reporting.AddLine("0.65 * d = " + (rightBeam.Shape.d / 2) + " <=  b = " + seismic.R_b + " <=  0.85 * d = " + 0.85 * rightBeam.Shape.d + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Distance b = " + seismic.R_b + " is not within the range, 0.65d ~ 0.85d  (  NG)");

					if (0.1 * rightBeam.Shape.bf <= seismic.R_c && seismic.R_c <= 0.25 * rightBeam.Shape.bf)
						Reporting.AddLine("0.1 * Bf = " + 0.1 * rightBeam.Shape.bf + " <=  c = " + seismic.R_c + " <=  0.25 * Bf = " + 0.25 * rightBeam.Shape.bf + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Distance c = " + seismic.R_c + " is not within the range, 0.1Bf ~ 0.25Bf  (  NG)");

					// STEP 2 EQ 5.8-4
					Reporting.AddHeader("RBS Plastic Section Modulus:");
					R_Ze = rightBeam.Shape.zx - 2 * seismic.R_c * rightBeam.Shape.tf * (rightBeam.Shape.d - rightBeam.Shape.tf);
					Reporting.AddLine("Ze = z - 2 * c * tf * (d - tf)");
					Reporting.AddLine("= " + rightBeam.Shape.zx + " - 2 * " + seismic.R_c + " * " + rightBeam.Shape.tf + " * (" + rightBeam.Shape.d + " - " + rightBeam.Shape.tf + ")");
					Reporting.AddLine("= " + R_Ze + ConstUnit.SecMod);

					// STEP 3 EQ:5.8-5
					Reporting.AddHeader("Probable Max. Moment at RBS center:");
					rightBeam.WinConnect.Fema.Ry = rightBeam.Material.Ry;
					Reporting.AddLine("Ry = " + rightBeam.WinConnect.Fema.Ry);
					R_Cpr = (rightBeam.Material.Fy + rightBeam.Material.Fu) / (2 * rightBeam.Material.Fy);
					Reporting.AddLine("Cpr = (Fy + Fu) / (2 * Fy) = (" + rightBeam.Material.Fy + " + " + rightBeam.Material.Fu + ") / (2 * " + rightBeam.Material.Fy + ")" + " = " + R_Cpr);
					Reporting.AddLine("Mpr = Cpr * Ry * Fy * Ze");
					Reporting.AddLine("= " + R_Cpr + " * " + rightBeam.WinConnect.Fema.Ry + " * " + rightBeam.Material.Fy + " * " + R_Ze + " = " + R_Mpr + ConstUnit.MomentUnitInch);

					// STEP 4 Shear force at RBS center
					Reporting.AddHeader("Shear Force at RBS center:");
					Reporting.AddLine("Length Between plastic hinges (Lph)");
					L_prime = seismic.ClearSpanOfBeamRight - 2 * (seismic.R_a + seismic.R_b / 2);
					Reporting.AddLine("= (BeamLength - 2 * (a + b / 2))= (" + seismic.ClearSpanOfBeamRight + " - 2 * (" + seismic.R_a + " + " + seismic.R_b + " / 2))");
					Reporting.AddLine("= " + L_prime + ConstUnit.Length);
					rightBeam.WinConnect.Fema.sh = (seismic.R_a + seismic.R_b / 2);
					a1 = seismic.DistanceToGravityLoadRight - rightBeam.WinConnect.Fema.sh;
					b1 = L_prime - a1;
					Reporting.AddLine("sh = (R_a + R_b / 2) = (" + seismic.R_a + " + " + seismic.R_b + " / 2) = " + rightBeam.WinConnect.Fema.sh + ConstUnit.Length);
					Reporting.AddLine("a1 = R_Vg_Dist - sh = " + seismic.DistanceToGravityLoadRight + " - " + rightBeam.WinConnect.Fema.sh + " = " + a1 + ConstUnit.Length);
					Reporting.AddLine("b1 = Lph - a1 = " + L_prime  + " - " + a1 + " = " + b1 + ConstUnit.Length);
					pg = seismic.GravityLoadRight;
					R_VRBS = Math.Max((pg * b1 + 2 * R_Mpr) / (seismic.ClearSpanOfBeamRight - 2 * (seismic.R_a + seismic.R_b / 2)), (pg * a1 - 2 * R_Mpr) / (seismic.ClearSpanOfBeamRight - 2 * (seismic.R_a + seismic.R_b / 2)));
					Reporting.AddLine("R_VRBS= Max((Pg * b1 + 2 * R_Mpr) / Lph, (Pg * a1 - 2 * R_Mpr) /Lph )");
					Reporting.AddLine("= Max((" + pg + " * " + b1 + " + 2 * " + R_Mpr + " / " + L_prime + ", (" + pg + " * " + a1 + " - 2 * " + R_Mpr + " / " + L_prime + ")");
					Reporting.AddLine("= " + R_VRBS + ConstUnit.Force);

					// STEP 5: EQ 5.8-6				
					Reporting.AddHeader("Probable Max. Moment at Column Face:");
					seismic.R_Mf = R_Mpr + R_VRBS * (seismic.R_a + seismic.R_b / 2);
					Reporting.AddLine("Mf = Mpr + VRBS * (a + b / 2)");
					Reporting.AddLine("= " + R_Mpr + " + " + R_VRBS + " * (" + seismic.R_a + " + " + (seismic.R_b / 2) + ")= " + seismic.R_Mf + ") " + ConstUnit.MomentUnitInch);

					// STEP 6  EQ 5.8-7				
					Reporting.AddHeader("Expected Plastic Moment of Beam:");
					R_Mpe = rightBeam.Shape.zx * rightBeam.WinConnect.Fema.Ry * rightBeam.Material.Fy;
					Reporting.AddLine("Mpe = Zb * Ry * Fy = " + rightBeam.Shape.zx + " * " + rightBeam.WinConnect.Fema.Ry + " * " + rightBeam.Material.Fy + " = " + R_Mpe + ConstUnit.MomentUnitInch);

					// STEP 7  EQ 5.8-8

					if (seismic.R_Mf <= ConstNum.FIOMEGA1_0N * R_Mpe)
						Reporting.AddLine("Mf = " + seismic.R_Mf + " <= " + ConstNum.FIOMEGA1_0N + " * " + R_Mpe + ConstUnit.MomentUnitInch + " (OK)");
					else
						Reporting.AddLine("Mf = " + seismic.R_Mf + " >> " + ConstNum.FIOMEGA1_0N + " * " + R_Mpe + ConstUnit.MomentUnitInch + " (NG)");

					// STEP 8 EQ 5.8-9					
					Reporting.AddHeader("Required Shear Strength of Beam & Web Connection:");
					P = seismic.GravityLoadRight;
					a1 = seismic.DistanceToGravityLoadRight;
					b1 = seismic.ClearSpanOfBeamRight - a1;
					R_Vg = Math.Max(P * b1 / seismic.ClearSpanOfBeamRight, P * a1 / seismic.ClearSpanOfBeamRight);
					seismic.R_Vu = 2 * R_Mpr / (seismic.ClearSpanOfBeamRight - 2 * (seismic.R_a + seismic.R_b / 2)) + R_Vg;

					Reporting.AddLine("Vu = 2 * Mpr / (L - 2 * (a + b / 2)) + Vgr");
					Reporting.AddLine("= 2 * " + R_Mpr + " / (" + seismic.ClearSpanOfBeamRight + " - 2 * (" + seismic.R_a + " + " + (seismic.R_b / 2) + ")) + " + R_Vg + " = " + seismic.R_Vu + ConstUnit.Force);

					Reporting.AddHeader("Available Shear Strength of Beam:");
					R_Vu_beam = BeamShearStrength(rightBeam.Material.Fy, rightBeam.Shape.t, rightBeam.Shape.tw, rightBeam.Shape.d);
					if (R_Vu_beam >= seismic.R_Vu)
						Reporting.AddLine("= " + R_Vu_beam + " >= " + seismic.R_Vu + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + R_Vu_beam + " << " + seismic.R_Vu + ConstUnit.Force + " (NG)");

					// STEP 9 BeamWeb to Column Conn.
					R_HightOFAccessHole = Math.Max(1.5 * rightBeam.Shape.tw, ConstNum.ONE_INCH);
					R_HightOFAccessHole = Math.Max(R_HightOFAccessHole, ConstNum.TWO_INCHES);
					R_CJP_WeldLength = rightBeam.Shape.d - 2 * (rightBeam.Shape.tf + R_HightOFAccessHole);

					Reporting.AddHeader("Available Shear Strength of Web at column Face:");
					h = rightBeam.Shape.d - 2 * (rightBeam.Shape.tf + R_HightOFAccessHole);
					R_Vu_web = BeamShearStrength(rightBeam.Material.Fy, h, rightBeam.Shape.tw, h);
					if (R_Vu_web >= seismic.R_Vu)
						Reporting.AddLine("= " + R_Vu_web + " >= " + seismic.R_Vu + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + R_Vu_web + " << " + seismic.R_Vu + ConstUnit.Force + " (NG)");
				}

				// 2)LBEAM LIMITATIONS
				if (leftBeam.IsActive)
				{
					Reporting.AddHeader("Left Side Beam Requirements:");

					if (leftBeam.Shape.d <= 37.3 * ConstNum.ONE_INCH)
						Reporting.AddLine("Beam Depth = " + leftBeam.Shape.d + " << " + 37.3 * ConstNum.ONE_INCH + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Beam Depth = " + leftBeam.Shape.d + " << " + 37.3 * ConstNum.ONE_INCH + ConstUnit.Length + " (NG)");

					WeightPerFoot = leftBeam.Shape.weight;
					if (WeightPerFoot <= 300 * WeightRatio)
						Reporting.AddLine("Beam Weight = " + WeightPerFoot + " << " + 300 * WeightRatio + ConstUnit.ForceUniform + " (OK)");
					else
						Reporting.AddLine("Beam Weight = " + WeightPerFoot + " >> " + 300 * 3.39 * WeightRatio + ConstUnit.ForceUniform + " (NG)");

					leftBeam.WinConnect.Fema.L = seismic.ClearSpanOfBeamLeft;
					switch (seismic.FramingType)
					{
						case EFramingSystem.SMF:
							if (leftBeam.WinConnect.Fema.L / leftBeam.Shape.d >= 7)
								Reporting.AddLine("Beam Span to depth Ratio (L / d) = " + leftBeam.WinConnect.Fema.L + " / " + leftBeam.Shape.d + " = " + leftBeam.WinConnect.Fema.L / leftBeam.Shape.d + " >=  7 (OK)");
							else
								Reporting.AddLine("Beam Span to depth Ratio (L / d) = " + leftBeam.WinConnect.Fema.L + " / " + leftBeam.Shape.d + " = " + leftBeam.WinConnect.Fema.L / leftBeam.Shape.d + " <<  7 (OK)");
							break;
						case EFramingSystem.IMF:
							if (leftBeam.WinConnect.Fema.L / leftBeam.Shape.d >= 5)
								Reporting.AddLine("Beam Span to depth Ratio (L / d) = " + leftBeam.WinConnect.Fema.L + " / " + leftBeam.Shape.d + " = " + leftBeam.WinConnect.Fema.L / leftBeam.Shape.d + " >=  5 (OK)");
							else
								Reporting.AddLine("Beam Span to depth Ratio (L / d) = " + leftBeam.WinConnect.Fema.L + " / " + leftBeam.Shape.d + " = " + leftBeam.WinConnect.Fema.L / leftBeam.Shape.d + " <<  5 (OK)");
							break;
					}
					if (leftBeam.Shape.tf <= 1.75 * ConstNum.ONE_INCH)
						Reporting.AddLine("Beam Flange thickness = " + leftBeam.Shape.tf + " <= " + 1.75 * ConstNum.ONE_INCH + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Beam Flange thickness = " + leftBeam.Shape.tf + " >> " + 1.75 * ConstNum.ONE_INCH + ConstUnit.Length + " (NG)");

					LambdaPF = 0.38 * Math.Pow(ConstNum.ELASTICITY / leftBeam.Material.Fy, 0.5);
					Reporting.AddLine("Limiting b / t = 0.38 * (E / Fy)^0.5 = 0.38 * (" + ConstNum.ELASTICITY + " / " + leftBeam.Material.Fy + ")^0.5 = " + LambdaPF);
					ratio = leftBeam.Shape.bf / leftBeam.Shape.tf / 2;
					if (ratio <= LambdaPF)
						Reporting.AddLine("Beam Flange b / t = " + (leftBeam.Shape.bf / 2) + " / " + leftBeam.Shape.tf + " = " + ratio + " <= " + LambdaPF + " (OK)");
					else
						Reporting.AddLine("Beam Flange b / t = " + (leftBeam.Shape.bf / 2) + " / " + leftBeam.Shape.tf + " = " + ratio + " >> " + LambdaPF + " (NG)");

					LambdaPW = 3.76 * Math.Pow(ConstNum.ELASTICITY / leftBeam.Material.Fy, 0.5);
					Reporting.AddLine("Limiting h / tw = 3.76 * (E / Fy)^0.5 = 3.76 * (" + ConstNum.ELASTICITY + " / " + leftBeam.Material.Fy + ")^0.5 = " + LambdaPW);
					ratio = leftBeam.Shape.t / leftBeam.Shape.tw;
					if (ratio <= LambdaPW)
						Reporting.AddLine("Beam Web h / t = " + leftBeam.Shape.t + " / " + leftBeam.Shape.tw + " = " + ratio + " <= " + LambdaPW + " (OK)");
					else
						Reporting.AddLine("Beam Web h / t = " + leftBeam.Shape.t + " / " + leftBeam.Shape.tw + " = " + ratio + " >> " + LambdaPW + " (NG)");
				}

				// 6)LBEAM RBS CALCS
				if (leftBeam.IsActive)
				{
					Reporting.AddHeader("Left Side Beam RBS Calcs:");
					Reporting.AddHeader("Flange Cutout Parameters:");

					if (0.5 * leftBeam.Shape.bf <= seismic.L_a && seismic.L_a <= 0.75 * leftBeam.Shape.bf)
						Reporting.AddLine("0.5 * Bf = " + (leftBeam.Shape.bf / 2) + " <=  a = " + seismic.L_a + " <=  0.75 * Bf = " + 0.75 * leftBeam.Shape.bf + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Distance a = " + seismic.L_a + " is not within the range, 0.5Bf ~ 0.75Bf  (  NG)");

					if (0.65 * leftBeam.Shape.d <= seismic.L_b && seismic.L_b <= 0.85 * leftBeam.Shape.d)
						Reporting.AddLine("0.65 * d = " + (leftBeam.Shape.d / 2) + " <=  b = " + seismic.L_b + " <=  0.85 * d = " + 0.85 * leftBeam.Shape.d + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Distance b = " + seismic.L_b + " is not within the range, 0.65d ~ 0.85d  (  NG)");

					if (0.1 * leftBeam.Shape.bf <= seismic.L_c && seismic.L_c <= 0.25 * leftBeam.Shape.bf)
						Reporting.AddLine("0.1 * Bf = " + 0.1 * leftBeam.Shape.bf + " <=  c = " + seismic.L_c + " <=  0.25 * Bf = " + 0.25 * leftBeam.Shape.bf + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Distance c = " + seismic.L_c + " is not within the range, 0.1Bf ~ 0.25Bf  (  NG)");

					// STEP 2 EQ 5.8-4
					Reporting.AddHeader("RBS Plastic Section Modulus:");
					L_Ze = leftBeam.Shape.zx - 2 * seismic.L_c * leftBeam.Shape.tf * (leftBeam.Shape.d - leftBeam.Shape.tf);
					Reporting.AddLine("Ze = z - 2 * c * tf * (d - tf)");
					Reporting.AddLine("= " + leftBeam.Shape.zx + " - 2 * " + seismic.L_c + " * " + leftBeam.Shape.tf + " * (" + leftBeam.Shape.d + " - " + leftBeam.Shape.tf + ")");
					Reporting.AddLine("= " + L_Ze + ConstUnit.SecMod);

					// STEP 3 EQ:5.8-5					
					Reporting.AddHeader("Probable Max. Moment at RBS center:");
					leftBeam.WinConnect.Fema.Ry = leftBeam.Material.Ry;
					Reporting.AddLine("Ry = " + leftBeam.WinConnect.Fema.Ry);

					L_Cpr = (leftBeam.Material.Fy + leftBeam.Material.Fu) / (2 * leftBeam.Material.Fy);
					Reporting.AddLine("Cpr = (Fy + Fu) / (2 * Fy) = (" + leftBeam.Material.Fy + " + " + leftBeam.Material.Fu + ") / (2 * " + leftBeam.Material.Fy + ")" + " = " + L_Cpr);
					Reporting.AddLine("Mpr = Cpr * Ry * Fy * Ze");
					Reporting.AddLine("= " + L_Cpr + " * " + leftBeam.WinConnect.Fema.Ry + " * " + leftBeam.Material.Fy + " * " + L_Ze + " = " + L_Mpr + ConstUnit.MomentUnitInch);

					// STEP 4 Shear force at RBS center
					Reporting.AddHeader("Shear Force at RBS center:");
					Reporting.AddHeader("Length Between plastic hinges (Lph)");
					L_prime = seismic.ClearSpanOfBeamLeft - 2 * (seismic.L_a + seismic.L_b / 2);
					Reporting.AddLine("= (BeamLength - 2 * (a + b / 2)) = (" + seismic.ClearSpanOfBeamLeft + " - 2 * (" + seismic.L_a + " + " + seismic.L_b + "/2))");
					Reporting.AddLine("= " + L_prime + ConstUnit.Length);
					leftBeam.WinConnect.Fema.sh = (seismic.L_a + seismic.L_b / 2);
					a1 = seismic.DistanceToGravityLoadLeft - leftBeam.WinConnect.Fema.sh;
					b1 = L_prime - a1;

					Reporting.AddLine("sh = (L_a + L_b / 2) = (" + seismic.L_a + " + " + seismic.L_b + " / 2) = " + leftBeam.WinConnect.Fema.sh + ConstUnit.Length);
					Reporting.AddLine("a1 = L_Vg_Dist - sh = " + seismic.DistanceToGravityLoadLeft + " - " + leftBeam.WinConnect.Fema.sh + " = " + a1 + ConstUnit.Length);
					Reporting.AddLine("b1 = Lph - a1 = " + L_prime + " - " + a1 + " = " + b1 + ConstUnit.Length);
					pg = seismic.GravityLoadLeft;
					L_VRBS = Math.Max((pg * b1 + 2 * L_Mpr) / (seismic.ClearSpanOfBeamLeft - 2 * (seismic.L_a + seismic.L_b / 2)), (pg * a1 - 2 * L_Mpr) / (seismic.ClearSpanOfBeamLeft - 2 * (seismic.L_a + seismic.L_b / 2)));
					Reporting.AddLine("L_VRBS = Max((Pg * b1 + 2 * L_Mpr) / Lph, (Pg * a1 - 2 * L_Mpr) /Lph )");
					Reporting.AddLine("= Max((" + pg + " * " + b1 + " + 2 * " + L_Mpr + " " + b1 + " / " + L_prime + ", (" + pg + " * " + a1 + " - 2 * " + L_Mpr + " / " + L_prime);
					Reporting.AddLine("= " + L_VRBS + ConstUnit.Force);

					// STEP 5: EQ 5.8-6
					Reporting.AddHeader("Probable Max Moment at Column Face:");
					seismic.L_Mf = L_Mpr + L_VRBS * (seismic.L_a + seismic.L_b / 2);
					Reporting.AddLine("Mf = Mpr + L_VRBS * (a + b / 2)");
					Reporting.AddLine("= " + L_Mpr + " + " + L_VRBS + " * (" + seismic.L_a + " + " + (seismic.L_b / 2) + ")= " + seismic.L_Mf + " )" + ConstUnit.MomentUnitInch);

					// STEP 6  EQ 5.8-7
					Reporting.AddHeader("Expected Plastic Moment of Beam:");
					L_Mpe = leftBeam.Shape.zx * leftBeam.WinConnect.Fema.Ry * leftBeam.Material.Fy;
					Reporting.AddLine("Mpe = Zb * Ry * Fy = " + leftBeam.Shape.zx + " * " + leftBeam.WinConnect.Fema.Ry + " * " + leftBeam.Material.Fy + " = " + L_Mpe + ConstUnit.MomentUnitInch);

					// STEP 7  EQ 5.8-8				
					if (seismic.L_Mf <= ConstNum.FIOMEGA1_0N * L_Mpe)
						Reporting.AddLine("Mf = " + seismic.L_Mf + " <= " + ConstNum.FIOMEGA1_0N + " * " + L_Mpe + " " + momentunit + " (OK)");
					else
						Reporting.AddLine("Mf = " + seismic.L_Mf + " >> " + ConstNum.FIOMEGA1_0N + " * " + L_Mpe + " " + momentunit + " (NG)");

					// STEP 8 EQ 5.8-9				
					Reporting.AddHeader("Required Shear Strength of Beam & Web Connection:");
					P = seismic.GravityLoadLeft;
					a1 = seismic.DistanceToGravityLoadLeft;
					b1 = seismic.ClearSpanOfBeamLeft - a1;
					L_Vg = Math.Max(P * b1 / seismic.ClearSpanOfBeamLeft, P * a1 / seismic.ClearSpanOfBeamLeft);

					seismic.L_Vu = 2 * L_Mpr / (seismic.ClearSpanOfBeamLeft - 2 * (seismic.L_a + seismic.L_b / 2)) + L_Vg;
					Reporting.AddLine("Vu = 2 * Mpr / (L - 2 * (a + b / 2)) + Vgr");
					Reporting.AddLine("= 2 * " + L_Mpr + " / (" + seismic.ClearSpanOfBeamLeft + " - 2 * (" + seismic.L_a + " + " + (seismic.L_b / 2) + ")) + " + L_Vg + " = " + seismic.L_Vu + ConstUnit.Force);

					Reporting.AddHeader("Available Shear Strength of Beam:");
					L_Vu_beam = BeamShearStrength(leftBeam.Material.Fy, leftBeam.Shape.t, leftBeam.Shape.tw, leftBeam.Shape.d);
					if (L_Vu_beam >= seismic.L_Vu)
						Reporting.AddLine("= " + L_Vu_beam + " >= " + seismic.L_Vu + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + L_Vu_beam + " << " + seismic.L_Vu + ConstUnit.Force + " (NG)");

					// STEP 9 BeamWeb to Column Conn.
					L_HightOFAccessHole = Math.Max(1.5 * leftBeam.Shape.tw, ConstNum.ONE_INCH);
					L_HightOFAccessHole = Math.Min(L_HightOFAccessHole, ConstNum.TWO_INCHES);

					Reporting.AddHeader("Available Shear Strength of Web at column Face:");
					h = leftBeam.Shape.d - 2 * (leftBeam.Shape.tf + L_HightOFAccessHole);
					L_Vu_web = BeamShearStrength(leftBeam.Material.Fy, h, leftBeam.Shape.tw, h);
					if (L_Vu_web >= seismic.L_Vu)
						Reporting.AddLine("= " + L_Vu_web + " >= " + seismic.L_Vu + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + L_Vu_web + " << " + seismic.L_Vu + ConstUnit.Force + " (NG)");
				}

				if (seismic.FramingType == EFramingSystem.SMF || seismic.FramingType == EFramingSystem.IMF)
				{
					Reporting.AddHeader("Column-Beam Moment Ratio:");
					switch (CommonDataStatic.Preferences.CalcMode)
					{
						case ECalcMode.LRFD:
							Puc = column.P;
							Sum_Mpc = 2 * column.Shape.zx * (column.Material.Fy - Puc / column.Shape.a);
							Reporting.AddLine("Sum_Mpc = 2 * Zc * (Fyc - Puc / Agc)");
							Reporting.AddLine("= 2 * " + column.Shape.zx + " * (" + column.Material.Fy + " - " + Puc + " / " + column.Shape.a + ")");
							Reporting.AddLine("= " + Sum_Mpc + ConstUnit.MomentUnitInch);
							break;
						case ECalcMode.ASD:
							Pac = column.P;
							Sum_Mpc = 2 * column.Shape.zx * (column.Material.Fy / 1.5 - Pac / column.Shape.a);
							Reporting.AddLine("Sum_Mpc = 2 * Zc * (Fyc / 1.5 - Puc / Agc)");
							Reporting.AddLine("= 2 * " + column.Shape.zx + " * (" + column.Material.Fy / 1.5 + " - " + Puc + " / " + column.Shape.a + ")");
							Reporting.AddLine("= " + Sum_Mpc + ConstUnit.MomentUnitInch);
							break;
					}
					R_Mv = R_VRBS * (seismic.R_a + seismic.R_b / 2 + column.Shape.d / 2);
					L_Mv = L_VRBS * (seismic.L_a + seismic.L_b / 2 + column.Shape.d / 2);
					Reporting.AddLine("R_Mv = R_VRBS * (a + b / 2 + dc / 2)");
					Reporting.AddLine("= " + R_VRBS + " * (" + seismic.R_a + " + " + seismic.R_b + " / 2 + " + column.Shape.d + "/ 2)");
					Reporting.AddLine("= " + R_Mv + ConstUnit.MomentUnitInch);

					Reporting.AddLine("L_Mv = L_VRBS * (a + b / 2 + dc / 2)");
					Reporting.AddLine("= " + L_VRBS + " * (" + seismic.L_a + " + " + seismic.L_b + " / 2 + " + column.Shape.d + "/ 2)");
					Reporting.AddLine("= " + L_Mv + ConstUnit.MomentUnitInch);

					Sum_Mpb = R_Mpr + L_Mpr + R_Mv + L_Mv;
					Reporting.AddLine("Sum_Mpb = R_Mpr + L_Mpr + R_Mv + L_Mv");
					Reporting.AddLine("= " + R_Mpr + " + " + L_Mpr + " + " + R_Mv + " + " + L_Mv);
					Reporting.AddLine("= " + Sum_Mpb + ConstUnit.MomentUnitInch);

					C_B_Ratio = Sum_Mpc / Sum_Mpb;

					Reporting.AddLine("Sum_Mpc / Sum_Mpb = " + Sum_Mpc + " / " + Sum_Mpb);

					if (Sum_Mpc / Sum_Mpb > 1)
						Reporting.AddLine("= " + C_B_Ratio + " > 1.0 (OK)");
					else
						Reporting.AddLine("= " + C_B_Ratio + " << 1.0 (NG)");
				}

				Stiff.SeismicDoublerPlateReport(seismic.R_Mf, seismic.L_Mf, tp_forFilletweld);

				if (rightBeam.IsActive)
				{
					rightBeam.ShearForce = seismic.R_Vu;
					rightBeam.Moment = seismic.R_Mf;

					rightBeam.MomentConnection = EMomentCarriedBy.DirectlyWelded;
					if (seismic.FramingType == EFramingSystem.IMF)
						rightBeam.ShearConnection = EShearCarriedBy.SinglePlate;
				}

				if (leftBeam.IsActive)
				{
					leftBeam.ShearForce = seismic.L_Vu;
					leftBeam.Moment = seismic.L_Mf;

					leftBeam.MomentConnection = EMomentCarriedBy.DirectlyWelded;
					if (seismic.FramingType == EFramingSystem.IMF)
						leftBeam.ShearConnection = EShearCarriedBy.SinglePlate;
				}
			}
		}

		private static double BeamShearStrength(double Fy, double h, double tw, double d)
		{
			double BeamShearStrength;
			double Vu;
			double limit_3;
			double limit_2;
			double kv = 0;
			double Cv;
			double FitempN = 0;
			string Fitemp = string.Empty;
			double limit_1;
			double Aw;
			double hovtw;

			hovtw = h / tw;
			Reporting.AddLine("h/tw = " + h + " / " + tw + " = " + hovtw);

			Aw = d * tw;
			Reporting.AddLine("Aw = d * tw = " + d + " * " + tw + " = " + Aw + ConstUnit.Area);
			Reporting.AddLine("kv = 5");
			if (hovtw <= 2.24 * Math.Pow(ConstNum.ELASTICITY / Fy, 0.5))
			{
				limit_1 = 2.24 * Math.Pow(ConstNum.ELASTICITY / Fy, 0.5);
				Reporting.AddLine("2.24 * (E/Fy) ^ 0.5 = 2.24 *(" + ConstNum.ELASTICITY + " / " + Fy + ")^0.5  = " + limit_1);
				Reporting.AddLine("Cv = 1");
				Cv = 1;
				Fitemp = ConstString.FIOMEGA1_0;
				FitempN = ConstNum.FIOMEGA1_0N;
			}
			else if (hovtw <= 1.1 * Math.Pow(kv * ConstNum.ELASTICITY / Fy, 0.5))
			{
				limit_2 = 1.1 * Math.Pow(kv * ConstNum.ELASTICITY / Fy, 0.5);
				Cv = 1;
				Reporting.AddLine("1.1 *(kv * E/Fy) ^ 0.5 = 1.1 * (5 * YMEn / " + Fy + ") ^ 0.5 = " + limit_2);
				Reporting.AddLine("Cv = 1");
				Fitemp = ConstString.FIOMEGA0_9;
				FitempN = ConstNum.FIOMEGA0_9N;
			}
			else if (hovtw <= 1.37 * Math.Pow(kv * ConstNum.ELASTICITY / Fy, 0.5))
			{
				limit_3 = 1.37 * Math.Pow(kv * ConstNum.ELASTICITY / Fy, 0.5);
				Reporting.AddLine("1.37 *(kv * E / Fy) ^ 0.5 = 1.37 * (5 * YME / " + Fy + ") ^ 0.5");
				Reporting.AddLine("= " + limit_3 + " >> " + hovtw);
				Cv = 1.1 * Math.Pow(kv * ConstNum.ELASTICITY / Fy, 0.5) / hovtw;
				Reporting.AddLine("Cv = 1.1 * (kv * E / Fy) ^ 0.5 / (h / tw)");
				Reporting.AddLine("= 1.1 * (" + kv + " * " + ConstNum.ELASTICITY + " / " + Fy + ") ^ 0.5 / " + hovtw);
				Reporting.AddLine("= " + Cv);
				Fitemp = ConstString.FIOMEGA0_9;
				FitempN = ConstNum.FIOMEGA0_9N;
			}
			else
			{
				Cv = 1.51 * ConstNum.ELASTICITY * kv / (Math.Pow(hovtw, 2) * Fy);
				Reporting.AddLine("Cv = 1.51 * kv * E / Fy) / (h / tw)²");
				Reporting.AddLine("= 1.1 * (" + kv + " * " + ConstNum.ELASTICITY + " / " + Fy + ") ^ 0.5 / " + hovtw);
				Reporting.AddLine("= " + Cv);
				Fitemp = ConstString.FIOMEGA0_9;
				FitempN = ConstNum.FIOMEGA0_9N;
			}

			Vu = FitempN * 0.6 * Fy * Aw * Cv;
			Reporting.AddLine("FiVn = " + Fitemp + " * 0.6 * Fy * Aw * Cv");
			Reporting.AddLine("= " + Fitemp + " * 0.6 * " + Fy + " * " + Aw + " * " + Cv);
			Reporting.AddLine("= " + Vu);

			BeamShearStrength = Vu;

			return BeamShearStrength;
		}

		public static void EndPlatePrequalifiedLimitations(DetailData beam, ESeismicConnectionType connectionType, EPrequalifiedLimitType limitType, ref double MaxValue, ref double MinValue)
		{
			double LambdaPW;
			double ratio;
			double LambdaPF;
			double SpanToDepthRatio;
			double Bbf;
			double tbf;
			double d;
			double Pb;
			double g;
			double Bp;
			double tp;
			double BeamSpan;
			string Header1;

			switch (limitType)
			{
				case EPrequalifiedLimitType.PlateThickness:
					switch (connectionType)
					{
						case ESeismicConnectionType.E4:
							MaxValue = 2.25 * ConstNum.ONE_INCH;
							MinValue = 0.5 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES4:
							MaxValue = 1.5 * ConstNum.ONE_INCH;
							MinValue = 0.5 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES8:
							MaxValue = 2.5 * ConstNum.ONE_INCH;
							MinValue = 0.75 * ConstNum.ONE_INCH;
							break;
					}
					break;
				case EPrequalifiedLimitType.EndPlateWidth:
					switch (connectionType)
					{
						case ESeismicConnectionType.E4:
							MaxValue = 10.75 * ConstNum.ONE_INCH;
							MinValue = 7 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES4:
							MaxValue = 10.75 * ConstNum.ONE_INCH;
							MinValue = 7 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES8:
							MaxValue = 15 * ConstNum.ONE_INCH;
							MinValue = 9 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
					}
					break;
				case EPrequalifiedLimitType.BoltGage:
					switch (connectionType)
					{
						case ESeismicConnectionType.E4:
							MaxValue = 6 * ConstNum.ONE_INCH;
							MinValue = 4 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES4:
							MaxValue = 6 * ConstNum.ONE_INCH;
							MinValue = 3.25 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES8:
							MaxValue = 6 * ConstNum.ONE_INCH;
							MinValue = 5 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
					}
					break;
				case EPrequalifiedLimitType.BoltDistanceFromBeamFlange:
					switch (connectionType)
					{
						case ESeismicConnectionType.E4:
							MaxValue = 4.5F * ConstNum.ONE_INCH;
							MinValue = 1.5 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES4:
							MaxValue = 5.5F * ConstNum.ONE_INCH;
							MinValue = 1.75 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES8:
							MaxValue = 2 * ConstNum.ONE_INCH;
							MinValue = 1.625 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
					}
					break;
				case EPrequalifiedLimitType.BoltVerticalSpacing:
					if (connectionType == ESeismicConnectionType.E4)
					{
						MaxValue = 0;
						MinValue = 0;
					}
					else if (connectionType == ESeismicConnectionType.ES4)
					{
						MaxValue = 0;
						MinValue = 0;
					}
					else if (connectionType == ESeismicConnectionType.ES8)
					{
						MaxValue = 3.75F * ConstNum.ONE_INCH;
						MinValue = 3.5 * ConstNum.ONE_INCH;
						if (CommonDataStatic.Units == EUnit.Metric)
						{
							MaxValue = (int) Math.Floor(MaxValue);
							MinValue = -((int) Math.Floor(-MinValue));
						}
					}
					break;
				case EPrequalifiedLimitType.BeamDepth:
					switch (connectionType)
					{
						case ESeismicConnectionType.E4:
							MaxValue = 55 * ConstNum.ONE_INCH;
							MinValue = 13.75F * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES4:
							MaxValue = 24 * ConstNum.ONE_INCH;
							MinValue = 13.75F * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES8:
							MaxValue = 36 * ConstNum.ONE_INCH;
							MinValue = 18 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
					}
					break;
				case EPrequalifiedLimitType.BeamFlangeThickness:
					switch (connectionType)
					{
						case ESeismicConnectionType.E4:
							MaxValue = 0.75 * ConstNum.ONE_INCH;
							MinValue = 0.375 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES4:
							MaxValue = 0.75 * ConstNum.ONE_INCH;
							MinValue = 0.375 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES8:
							MaxValue = 1 * ConstNum.ONE_INCH;
							MinValue = 0.5625 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
					}
					break;
				case EPrequalifiedLimitType.BeamFlangeWidth:
					switch (connectionType)
					{
						case ESeismicConnectionType.E4:
							MaxValue = 9.25 * ConstNum.ONE_INCH;
							MinValue = 6 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES4:
							MaxValue = 9 * ConstNum.ONE_INCH;
							MinValue = 6 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES8:
							MaxValue = 12.25 * ConstNum.ONE_INCH;
							MinValue = 7.5F * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
					}
					break;
			}
			if (beam.MemberType == EMemberType.RightBeam)
				Header1 = "Right Side End Plate Prequalification:";
			else
				Header1 = "Left Side End Plate Prequalification:";

			BeamSpan = beam.Length;

			if (limitType == EPrequalifiedLimitType.PlateThickness)
				Reporting.AddHeader(Header1);

			switch (limitType)
			{
				case EPrequalifiedLimitType.PlateThickness:
					switch (connectionType)
					{
						case ESeismicConnectionType.E4:
							MaxValue = 2.25 * ConstNum.ONE_INCH;
							MinValue = 0.5 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES4:
							MaxValue = 1.5 * ConstNum.ONE_INCH;
							MinValue = 0.5 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES8:
							MaxValue = 2.5 * ConstNum.ONE_INCH;
							MinValue = 0.75 * ConstNum.ONE_INCH;
							break;
					}
					Reporting.AddHeader("Plate Thickness:");
					tp = beam.WinConnect.MomentEndPlate.Thickness;

					Reporting.AddLine("Min_tp = " + MinValue + "Max_tp = " + MaxValue + ConstUnit.Length);
					if (tp <= MaxValue && tp >= MinValue)
						Reporting.AddLine(MinValue + " <<=  " + beam.WinConnect.MomentEndPlate.Thickness + " <=  " + MaxValue + ConstUnit.Length + " (OK)");
					else if (tp > MaxValue)
						Reporting.AddLine("tp = " + beam.WinConnect.MomentEndPlate.Thickness + " >> " + MaxValue + ConstUnit.Length + " (NG)");
					else if (tp < MinValue)
						Reporting.AddLine("tp = " + beam.WinConnect.MomentEndPlate.Thickness + " << " + MinValue + ConstUnit.Length + " (NG)");

					break;
				case EPrequalifiedLimitType.EndPlateWidth:
					switch (connectionType)
					{
						case ESeismicConnectionType.E4:
							MaxValue = 10.75 * ConstNum.ONE_INCH;
							MinValue = 7 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES4:
							MaxValue = 10.75 * ConstNum.ONE_INCH;
							MinValue = 7 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES8:
							MaxValue = 15 * ConstNum.ONE_INCH;
							MinValue = 9 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
					}
					Reporting.AddHeader("End Plate Width:");
					Bp = beam.WinConnect.MomentEndPlate.Width;

					Reporting.AddLine("Min_bp = " + MinValue + "Max_bp = " + MaxValue + ConstUnit.Length);
					if (Bp <= MaxValue && Bp >= MinValue)
						Reporting.AddLine(MinValue + " <<= " + beam.WinConnect.MomentEndPlate.Width + " <=  " + MaxValue + ConstUnit.Length + " (OK)");
					else if (Bp > MaxValue)
						Reporting.AddLine("bp = " + beam.WinConnect.MomentEndPlate.Width + " >> " + MaxValue + ConstUnit.Length + " (NG)");
					else if (Bp < MinValue)
						Reporting.AddLine("bp = " + beam.WinConnect.MomentEndPlate.Width + " << " + MinValue + ConstUnit.Length + " (NG)");

					break;
				case EPrequalifiedLimitType.BoltGage:
					switch (connectionType)
					{
						case ESeismicConnectionType.E4:
							MaxValue = 6 * ConstNum.ONE_INCH;
							MinValue = 4 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES4:
							MaxValue = 6 * ConstNum.ONE_INCH;
							MinValue = 3.25 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES8:
							MaxValue = 6 * ConstNum.ONE_INCH;
							MinValue = 5 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
					}
					Reporting.AddHeader("Bolt Gage on Column:");
					g = beam.GageOnFlange;

					Reporting.AddLine("Min_g = " + MinValue + "Max_g = " + MaxValue + ConstUnit.Length);
					if (g <= MaxValue && g >= MinValue)
						Reporting.AddLine(MinValue + " <<= " + beam.GageOnFlange + " <= " + MaxValue + ConstUnit.Length + " (OK)");
					else if (g > MaxValue)
						Reporting.AddLine("g = " + beam.GageOnFlange + " > " + MaxValue + ConstUnit.Length + " (NG)");
					else if (g < MinValue)
						Reporting.AddLine("g = " + beam.GageOnFlange + " < " + MinValue + ConstUnit.Length + " (NG)");
					break;
				case EPrequalifiedLimitType.BoltDistanceFromBeamFlange:
					switch (connectionType)
					{
						case ESeismicConnectionType.E4:
							MaxValue = 4.5F * ConstNum.ONE_INCH;
							MinValue = 1.5 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES4:
							MaxValue = 5.5F * ConstNum.ONE_INCH;
							MinValue = 1.75 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES8:
							MaxValue = 2 * ConstNum.ONE_INCH;
							MinValue = 1.625 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
					}
					Reporting.AddHeader("Bolt Distance from beam flange top or bottom:");
					beam.WinConnect.Fema.pf = beam.WinConnect.MomentEndPlate.DistanceToFirstBolt;

					Reporting.AddLine("Min_Pf = " + MinValue + "Max_Pf = " + MaxValue + ConstUnit.Length);
					if (beam.WinConnect.Fema.pf <= MaxValue && beam.WinConnect.Fema.pf >= MinValue)
						Reporting.AddLine(MinValue + " <<= " + beam.WinConnect.MomentEndPlate.DistanceToFirstBolt + " <= " + MaxValue + ConstUnit.Length + " (OK)");
					else if (beam.WinConnect.Fema.pf > MaxValue)
						Reporting.AddLine("Pf = " + beam.WinConnect.MomentEndPlate.DistanceToFirstBolt + " > " + MaxValue + ConstUnit.Length + " (NG)");
					else if (beam.WinConnect.Fema.pf < MinValue)
						Reporting.AddLine("Pf = " + beam.WinConnect.MomentEndPlate.DistanceToFirstBolt + " < " + MinValue + ConstUnit.Length + " (NG)");
					break;
				case EPrequalifiedLimitType.BoltVerticalSpacing:
					if (connectionType == ESeismicConnectionType.E4)
					{
						MaxValue = 0;
						MinValue = 0;
					}
					else if (connectionType == ESeismicConnectionType.ES4)
					{
						MaxValue = 0;
						MinValue = 0;
					}
					else if (connectionType == ESeismicConnectionType.ES8)
					{
						MaxValue = 3.75F * ConstNum.ONE_INCH;
						MinValue = 3.5 * ConstNum.ONE_INCH;
						if (CommonDataStatic.Units == EUnit.Metric)
						{
							MaxValue = (int) Math.Floor(MaxValue);
							MinValue = -((int) Math.Floor(-MinValue));
						}
						Reporting.AddLine("Bolt Vertical Spacing:");
						Pb = beam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir;

						Reporting.AddLine("Min_Pb = " + MinValue + "Max_Pb = " + MaxValue + ConstUnit.Length);
						if (Pb <= MaxValue && Pb >= MinValue)
							Reporting.AddLine(MinValue + " <<= " + beam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir + " <= " + MaxValue + ConstUnit.Length + " (OK)");
						else if (Pb > MaxValue)
							Reporting.AddLine("Pb = " + beam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir + " > " + MaxValue + ConstUnit.Length + " (NG)");
						else if (Pb < MinValue)
							Reporting.AddLine("Pb = " + beam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir + " < " + MinValue + ConstUnit.Length + " (NG)");
					}
					break;
				case EPrequalifiedLimitType.BeamDepth:
					switch (connectionType)
					{
						case ESeismicConnectionType.E4:
							MaxValue = 55 * ConstNum.ONE_INCH;
							MinValue = 13.75F * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES4:
							MaxValue = 24 * ConstNum.ONE_INCH;
							MinValue = 13.75F * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES8:
							MaxValue = 36 * ConstNum.ONE_INCH;
							MinValue = 18 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
					}
					Reporting.AddHeader("Beam Depth:");
					d = beam.Shape.d;
					Reporting.AddLine("Min_d = " + MinValue + "Max_d = " + MaxValue + ConstUnit.Length);
					if (d <= MaxValue && d >= MinValue)
						Reporting.AddLine(MinValue + " <=  " + beam.Shape.d + " <=  " + MaxValue + ConstUnit.Length + " (OK)");
					else if (d > MaxValue)
						Reporting.AddLine("d = " + beam.Shape.d + " > " + MaxValue + ConstUnit.Length + " (NG)");
					else if (d < MinValue)
						Reporting.AddLine("d = " + beam.Shape.d + " < " + MinValue + ConstUnit.Length + " (NG)");
					break;
				case EPrequalifiedLimitType.BeamFlangeThickness:
					switch (connectionType)
					{
						case ESeismicConnectionType.E4:
							MaxValue = 0.75 * ConstNum.ONE_INCH;
							MinValue = 0.375 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES4:
							MaxValue = 0.75 * ConstNum.ONE_INCH;
							MinValue = 0.375 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES8:
							MaxValue = 1 * ConstNum.ONE_INCH;
							MinValue = 0.5625 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
					}
					Reporting.AddHeader("Beam Flange Thickness:");
					tbf = beam.Shape.tf;

					Reporting.AddLine("Min_tbf = " + MinValue + "Max_tbf = " + MaxValue + ConstUnit.Length);
					if (tbf <= MaxValue && tbf >= MinValue)
						Reporting.AddLine(MinValue + " <<= " + beam.Shape.tf + " <= " + MaxValue + ConstUnit.Length + " (OK)");
					else if (tbf > MaxValue)
						Reporting.AddLine("tbf = " + beam.Shape.tf + " >> " + MaxValue + ConstUnit.Length + "(NG)");
					else if (tbf < MinValue)
						Reporting.AddLine("tbf = " + beam.Shape.tf + " << " + MinValue + ConstUnit.Length + " (NG)");

					break;
				case EPrequalifiedLimitType.BeamFlangeWidth:
					switch (connectionType)
					{
						case ESeismicConnectionType.E4:
							MaxValue = 9.25 * ConstNum.ONE_INCH;
							MinValue = 6 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES4:
							MaxValue = 9 * ConstNum.ONE_INCH;
							MinValue = 6 * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
						case ESeismicConnectionType.ES8:
							MaxValue = 12.25 * ConstNum.ONE_INCH;
							MinValue = 7.5F * ConstNum.ONE_INCH;
							if (CommonDataStatic.Units == EUnit.Metric)
							{
								MaxValue = (int) Math.Floor(MaxValue);
								MinValue = -((int) Math.Floor(-MinValue));
							}
							break;
					}
					Reporting.AddHeader("Beam Flange Width:");
					Bbf = beam.Shape.bf;

					Reporting.AddLine("Min_Bbf = " + MinValue + "Max_Bbf = " + MaxValue + ConstUnit.Length);
					if (Bbf <= MaxValue && Bbf >= MinValue)
						Reporting.AddLine(MinValue + " <<= " + beam.Shape.bf + " <= " + MaxValue + ConstUnit.Length + " (OK)");
					else if (Bbf > MaxValue)
						Reporting.AddLine("Bbf = " + beam.Shape.bf + " > " + MaxValue + ConstUnit.Length + " (NG)");
					else if (Bbf < MinValue)
						Reporting.AddLine("Bbf = " + beam.Shape.bf + " < " + MinValue + ConstUnit.Length + " (NG)");
					break;
			}
			if (limitType == EPrequalifiedLimitType.BeamFlangeWidth)
			{
				// (5)
				Reporting.AddHeader("Beam Span to Depth Ratio:");
				if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.SMF)
				{
					SpanToDepthRatio = BeamSpan / beam.Shape.d;
					Reporting.AddLine("L/d = " + BeamSpan + " / " + beam.Shape.d);
					if (SpanToDepthRatio >= 7)
						Reporting.AddLine("= " + SpanToDepthRatio + " >= 7" + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + SpanToDepthRatio + " << 7" + ConstUnit.Length + " (NG)");
				}
				else if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.IMF)
				{
					SpanToDepthRatio = BeamSpan / beam.Shape.d;
					Reporting.AddLine("L/d = " + BeamSpan + " / " + beam.Shape.d);
					if (SpanToDepthRatio >= 5)
						Reporting.AddLine("= " + SpanToDepthRatio + " >= 5" + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + SpanToDepthRatio + " << 5" + ConstUnit.Length + " (NG)");
				}

				// (6)slenderness check:
				Reporting.AddHeader("Slenderness Check of Beam");
				LambdaPF = 0.38 * Math.Pow(ConstNum.ELASTICITY / beam.Material.Fy, 0.5);

				Reporting.AddHeader("Beam Flange:");
				Reporting.AddLine("Limiting b/t = 0.38 * (E/Fy)^0.5 = 0.38 * (" + ConstNum.ELASTICITY + " / " + beam.Material.Fy + ")^0.5 = " + LambdaPF);
				ratio = beam.Shape.bf / beam.Shape.tf / 2;
				Reporting.AddLine("Bf/tf/2 = " + beam.Shape.bf + " / " + beam.Shape.tf + "/2");
				if (ratio <= LambdaPF)
					Reporting.AddLine("= " + (beam.Shape.bf / 2) + " / " + beam.Shape.tf + " = " + ratio + " <= " + LambdaPF + " (OK)");
				else
					Reporting.AddLine("= " + (beam.Shape.bf / 2) + " / " + beam.Shape.tf + " = " + ratio + " >> " + LambdaPF + " (NG)");

				Reporting.AddHeader("Beam Web:");
				LambdaPW = 3.76 * Math.Pow(ConstNum.ELASTICITY / beam.Material.Fy, 0.5);
				Reporting.AddLine("Limiting h/tw = 3.76 * (E/Fy)^0.5 = 3.76 * (" + ConstNum.ELASTICITY + " / " + beam.Material.Fy + ")^0.5 = " + LambdaPW);
				ratio = beam.Shape.t / beam.Shape.tw;
				Reporting.AddLine("T/tw = " + beam.Shape.t + " / " + beam.Shape.tw);
				if (ratio <= LambdaPW)
					Reporting.AddLine("= " + beam.Shape.t + " / " + beam.Shape.tw + " = " + ratio + " <= " + LambdaPW + " (OK)");
				else
					Reporting.AddLine("= " + beam.Shape.t + " / " + beam.Shape.tw + " = " + ratio + " >> " + LambdaPW + " (NG)");
			}
		}
	}
}