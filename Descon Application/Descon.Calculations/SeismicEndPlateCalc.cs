using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class SeismicEndPlateCalc
	{
		public static void SeismicEndPlate()
		{
			double Sum_Mpb = 0;
			double L_Mv;
			double R_Mv = 0;
			double Pac = 0;
			double Sum_Mpc = 0;
			double Puc = 0;
			double C_B_Ratio = 0;
			double LMomentEndPlateStiffenerHeight = 0;
			double L_Fsu = 0;
			double Lno = 0;
			double Lni = 0;
			double L_Ffu = 0;
			double tprequired = 0;
			double L_Mpe = 0;
			double L_Cpr = 0;
			double L_Vg = 0;
			double LSpanToDepthRatio = 0;
			double LPreviousThickness = 0;
			double R_Fsu = 0;
			double FidRn18 = 0;
			double n = 0;
			double FidRn17 = 0;
			double FidRn16 = 0;
			double Rn = 0;
			double ct = 0;
			double FidRn15 = 0;
			double FidRn = 0;
			double fiOmega0_90n = 0;
			double FidMcf = 0;
			double extension = 0;
			double yc = 0;
			double c = 0;
			double bcf = 0;
			double Rno = 0;
			double Rni = 0;
			double No = 0;
			double Lc = 0;
			double Ni = 0;
			int NB = 0;
			double StiffenerToEndPlateWeld = 0;
			double w_req = 0;
			double BeamFlangetoStiffenerWeld = 0;
			double Limit = 0;
			double ts = 0;
			double ts_min = 0;
			double An = 0;
			bool loopAgain = false;
			double FiRn = 0;
			double R_Ffu = 0;
			double tp_Req = 0;
			double tpr = 0;
			double yp = 0;
			double s = 0;
			double de = 0;
			double h4 = 0;
			double h3 = 0;
			double h2 = 0;
			double dboltRequired = 0;
			double h1 = 0;
			double h0 = 0;
			double Fnt = 0;
			double g = 0;
			double Gage = 0;
			double Bp = 0;
			double R_Mpe = 0;
			double R_Cpr = 0;
			double R_Vg = 0;
			double b1 = 0;
			double a1 = 0;
			double P = 0;
			double Pb = 0;
			double pfo = 0;
			double pfi = 0;
			double LambdaPW = 0;
			double ratio = 0;
			double LambdaPF = 0;
			double RSpanToDepthRatio = 0;
			double MinValue = 0;
			double MaxValue = 0;
			double RPreviousThickness = 0;
			double LimitingRatio = 0;
			double bovt = 0;
			double MaxBeamDepth = 0;
			double ColumnDepth = 0;
			bool devamet;

			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var columnStiffener = CommonDataStatic.ColumnStiffener;

			Reporting.AddLine("Beam Connection to Column Flange");
			Reporting.AddLine("Prequalified Seismic Design Using ANSI/AISC 358-05");
			Reporting.AddLine("Column: " + column.Shape.Name + " - " + column.Material.Name);

			if (leftBeam.IsActive)
			{
				Reporting.AddLine("Left Side Beam: " + leftBeam.Shape.Name + " - " + leftBeam.Material.Name);
				Reporting.AddLine("Moment: " + (leftBeam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
				Reporting.AddLine("Shear: " + leftBeam.ShearForce + ConstUnit.Force);
				Reporting.AddLine("Axial Force: " + leftBeam.P + ConstUnit.Force);
			}
			if (rightBeam.IsActive)
			{
				Reporting.AddLine("Right Side Beam: " + rightBeam.Shape.Name + " - " + rightBeam.Material);
				Reporting.AddLine("Moment: " + (rightBeam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
				Reporting.AddLine("Shear: " + rightBeam.ShearForce + ConstUnit.Force);
				Reporting.AddLine("Axial Force: " + rightBeam.P + ConstUnit.Force);
			}

			if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
				Reporting.AddLine("Connection Type: 8-Bolt_Stiffened End Plate");
			else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
				Reporting.AddLine("Connection Type: 4-Bolt_Stiffened End Plate");
			else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4)
				Reporting.AddLine("Connection Type: 4-Bolt_Unstiffened End Plate");

			if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.SMF)
				Reporting.AddLine("Moment Frame Type: SMF");
			if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.IMF)
				Reporting.AddLine("Moment Frame Type: IMF");

			Reporting.AddLine("Column Limitations");
			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange)
				Reporting.AddLine("  End Plate Connected to Column Flange (OK)");
			else
				Reporting.AddLine("  End Plate not Connected to Column Flange (NG)");

			ColumnDepth = column.Shape.d;
			MaxBeamDepth = Math.Max(leftBeam.Shape.d, rightBeam.Shape.d);
			Reporting.AddLine("Maximum Beam depth = max(Lbeam_d, Rbeam_d)=  " + MaxBeamDepth + "  " + ConstUnit.Length);

			if (ColumnDepth <= MaxBeamDepth)
				Reporting.AddLine(" Column depth = " + column.Shape.d + "  <=  " + MaxBeamDepth + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine(" Column depth = " + column.Shape.d + "  >>  " + MaxBeamDepth + ConstUnit.Length + " (NG)");

			Reporting.AddLine("Column Flange Width/Thickness Ratio:");
			bovt = column.Shape.bf / 2 / column.Shape.tf;
			Reporting.AddLine("b/tf = " + column.Shape.bf + " /2/" + column.Shape.tf);

			Reporting.AddLine("= " + bovt);
			LimitingRatio = 0.3 * Math.Pow(ConstNum.ELASTICITY / column.Material.Fy, 0.5);
			Reporting.AddLine("LimitingRatio = 0.30 * (E/Fy)^0.5");
			if (LimitingRatio >= bovt)
				Reporting.AddLine("= " + LimitingRatio + "  >= " + bovt + " (OK)");
			else
				Reporting.AddLine("= " + LimitingRatio + "  << " + bovt + " (NG)");

			if (rightBeam.IsActive)
			{
				Reporting.AddHeader("Right Side Beam:");
				Reporting.AddLine(" " + rightBeam.Shape.Name + " - " + rightBeam.Material);
				Reporting.AddLine("Clear Length of Beam:" + CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight + ConstUnit.Length);
				Reporting.AddLine("Moment: " + (rightBeam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
				Reporting.AddLine("Shear: " + rightBeam.ShearForce + ConstUnit.Force);
				Reporting.AddLine("Axial Force: " + rightBeam.P + ConstUnit.Force);
				Reporting.AddLine("Gravity Load: " + CommonDataStatic.SeismicSettings.GravityLoadRight + "  " + ConstUnit.Force);
			}

			if (leftBeam.IsActive)
			{
				Reporting.AddHeader("Left Side Beam:");
				Reporting.AddLine(" " + leftBeam.Shape.Name + " - " + leftBeam.Material.Name);
				Reporting.AddLine("Clear Length of Beam:" + CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft + ConstUnit.Length);
				Reporting.AddLine("Moment: " + (leftBeam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
				Reporting.AddLine("Shear: " + leftBeam.ShearForce + ConstUnit.Force);
				Reporting.AddLine("Axial Force: " + leftBeam.P + ConstUnit.Force);
				Reporting.AddLine("Gravity Load: " + CommonDataStatic.SeismicSettings.GravityLoadLeft + "  " + ConstUnit.Force);
			}

			do
			{
				RPreviousThickness = rightBeam.WinConnect.MomentEndPlate.Thickness;

				if (rightBeam.IsActive)
				{
					SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BeamDepth, ref MaxValue, ref MinValue);

					// (3)none
					// (4) Flange Thickness
					SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BeamFlangeThickness, ref MaxValue, ref MinValue);

					// (5)
					if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.SMF)
						RSpanToDepthRatio = CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight / rightBeam.Shape.d;
					else if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.IMF)
						RSpanToDepthRatio = CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight / rightBeam.Shape.d;

					// (6)slenderness check:
					LambdaPF = 0.38 * Math.Pow(ConstNum.ELASTICITY / rightBeam.Material.Fy, 0.5);
					ratio = rightBeam.Shape.bf / rightBeam.Shape.tf / 2;
					LambdaPW = 3.76 * Math.Pow(ConstNum.ELASTICITY / rightBeam.Material.Fy, 0.5);
					ratio = rightBeam.Shape.t / rightBeam.Shape.tw;
					SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltDistanceFromBeamFlange, ref MaxValue, ref MinValue);

					pfi = 2;
					if (pfi > MaxValue)
						pfi = MaxValue;
					if (pfi < MinValue)
						pfi = MinValue;
					pfo = pfi;

					// 6.10 Design Procedure
					// Step 1
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
					{
						SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltVerticalSpacing, ref MaxValue, ref MinValue);

						Pb = -((int) Math.Floor((double) -4 * (2 + 2 / 3) * rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize)) / 4.0;
						if (Pb > MaxValue)
							Pb = MaxValue;
						if (Pb < MinValue)
							Pb = MinValue;
					}

					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4)
						CommonDataStatic.SeismicSettings.R_sh = Math.Min(rightBeam.Shape.d / 2, 3 * rightBeam.Shape.bf);
					else
					{
						if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
							CommonDataStatic.SeismicSettings.R_hst = pfo + rightBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir;
						else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
							CommonDataStatic.SeismicSettings.R_hst = pfo + rightBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir + rightBeam.WinConnect.MomentEndPlate.Bolt.SpacingLongDir;
						CommonDataStatic.SeismicSettings.R_lst = -((int) Math.Floor(-4 * ((CommonDataStatic.SeismicSettings.R_hst - ConstNum.ONE_INCH) / 0.57735 + ConstNum.ONE_INCH))) / 4.0;
						CommonDataStatic.SeismicSettings.R_sh = rightBeam.WinConnect.MomentEndPlate.Thickness + CommonDataStatic.SeismicSettings.R_lst;
					}

					CommonDataStatic.SeismicSettings.GravityLoadRight = CommonDataStatic.SeismicSettings.GravityLoadRight;
					P = CommonDataStatic.SeismicSettings.GravityLoadRight;
					a1 = CommonDataStatic.SeismicSettings.DistanceToGravityLoadRight;
					b1 = CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight - a1;
					R_Vg = Math.Max(P * b1 / CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight, P * a1 / CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight);

					rightBeam.WinConnect.Fema.Ry = rightBeam.Material.Ry;
					R_Cpr = (rightBeam.Material.Fy + rightBeam.Material.Fu) / (2 * rightBeam.Material.Fy);
					R_Mpe = R_Cpr * rightBeam.WinConnect.Fema.Ry * rightBeam.Material.Fy * rightBeam.Shape.zx; // probable moment at hinge
					CommonDataStatic.SeismicSettings.R_Vu = 2 * R_Mpe / (CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight - 2 * CommonDataStatic.SeismicSettings.R_sh) + R_Vg;
					CommonDataStatic.SeismicSettings.R_Mf = R_Mpe + CommonDataStatic.SeismicSettings.R_Vu * CommonDataStatic.SeismicSettings.R_sh;
					SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.EndPlateWidth, ref MaxValue, ref MinValue);

					Bp = rightBeam.Shape.bf + ConstNum.ONE_INCH;
					if (Bp > MaxValue)
						Bp = MaxValue;
					if (Bp < MinValue)
						Bp = MinValue;

                    if (!rightBeam.WinConnect.MomentEndPlate.Width_User) 
                        rightBeam.WinConnect.MomentEndPlate.Width = Bp;
					// step 2
					SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltGage, ref MaxValue, ref MinValue);

					Gage = column.GageOnFlange;
					if (Gage > MaxValue)
						Gage = MaxValue;
					if (Gage < MinValue)
						Gage = MinValue;
					g = Gage;

					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
					{
						SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltVerticalSpacing, ref MaxValue, ref MinValue);

						Pb = -((int) Math.Floor((double) -4 * (2 + 2 / 3) * rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize)) / 4.0;
						if (Pb > MaxValue)
							Pb = MaxValue;
						if (Pb < MinValue)
							Pb = MinValue;
					}

					// Step 3 ' Bolt diameter
					switch (rightBeam.WinConnect.MomentEndPlate.Bolt.ASTMType)
					{
						case EBoltASTM.A325:
							Fnt = 90; // ksi
							break;
						case EBoltASTM.A490:
							Fnt = 113; // ksi
							break;
					}

					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4)
					{
						h0 = rightBeam.Shape.d - rightBeam.Shape.tf / 2 + pfo;
						h1 = h0 - rightBeam.Shape.tf - pfi - pfo;
						dboltRequired = Math.Pow(2 * rightBeam.WinConnect.Fema.Mf / (Math.PI * ConstNum.FIOMEGA0_75N * Fnt * (h0 + h1)), 0.5);
					}
					else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
					{
						h1 = rightBeam.Shape.d - rightBeam.Shape.tf / 2 + pfo + Pb;
						h2 = h1 - Pb;
						h3 = h2 - pfo - rightBeam.Shape.tf - pfi;
						h4 = h3 - Pb;
						dboltRequired = Math.Pow(2 * CommonDataStatic.SeismicSettings.R_Mf / (Math.PI * ConstNum.FIOMEGA0_75N * Fnt * (h1 + h2 + h3 + h4)), 0.5);
					}

					dboltRequired = -((int) Math.Floor(-8 * dboltRequired)) / 8.0;
					de = rightBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir;

					// Step 4  ???? What purpose
					// Step 5 End Plate thickness
					s = 0.5 * Math.Pow(Bp * g, 0.5);
					if (pfi > s)
						pfi = s;
					pfo = pfi;
					
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4)
						yp = Bp / 2 * (h1 * (1 / pfi + 1 / s) + h0 * (1 / pfo) - 0.5) + 2 / g * (h1 * (pfi + s));
					else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
					{
						if (de <= s)
							yp = Bp / 2 * (h1 * (1 / pfi + 1 / s) + h0 * (1 / pfo + 1 / (2 * s))) + 2 / g * (h1 * (pfi + s) + h0 * (de + pfo));
						else
							yp = Bp / 2 * (h1 * (1 / pfi + 1 / s) + h0 * (1 / pfo + 1 / s)) + 2 / g * (h1 * (pfi + s) + h0 * (s + pfo));
					}
					else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
					{
						if (de <= s)
							yp = Bp / 2 * (h1 * (1 / (2 * de)) + h2 * (1 / pfo) + h3 * (1 / pfi) + h4 / s) + 2 / g * (h1 * (de + Pb / 4) + h2 * (pfo + 3 * Pb / 4) + h3 * (pfi + Pb / 4) + h4 * (s + 3 * Pb / 4) + Math.Pow(Pb, 2)) + g;
						else
							yp = Bp / 2 * (h1 * (1 / s) + h2 * (1 / pfo) + h3 * (1 / pfi) + h4 / s) + 2 / g * (h1 * (de + Pb / 4) + h2 * (pfo + 3 * Pb / 4) + h3 * (pfi + Pb / 4) + h4 * (s + 3 * Pb / 4) + Math.Pow(Pb, 2)) + g;
					}

					tpr = 0.75;
					do
					{
						CommonDataStatic.SeismicSettings.R_sh = CommonDataStatic.SeismicSettings.R_lst + tpr;
						CommonDataStatic.SeismicSettings.R_Mf = R_Mpe + CommonDataStatic.SeismicSettings.R_Vu * CommonDataStatic.SeismicSettings.R_sh;
						tp_Req = Math.Pow(1.11 * CommonDataStatic.SeismicSettings.R_Mf / (ConstNum.FIOMEGA0_9N * rightBeam.WinConnect.MomentEndPlate.Material.Fy * yp), 0.5);
                        if (!rightBeam.WinConnect.MomentEndPlate.Thickness_User) 
                            rightBeam.WinConnect.MomentEndPlate.Thickness = CommonCalculations.PlateThickness(Math.Max(tp_Req, tpr));

						if (tp_Req > tpr)
						{
							tpr = rightBeam.WinConnect.MomentEndPlate.Thickness;
							devamet = true;
						}
						else
							devamet = false;
					} while (devamet);

                    if (!rightBeam.WinConnect.MomentEndPlate.Thickness_User) 
                        rightBeam.WinConnect.MomentEndPlate.Thickness = CommonCalculations.PlateThickness(Math.Max(tp_Req, tpr));

					// Step 6
					// step 7 Beam Flange Force
					R_Ffu = CommonDataStatic.SeismicSettings.R_Mf / (rightBeam.Shape.d - rightBeam.Shape.tf);

					// step 8 End Plate Shear Yielding (Out of Plane)
					do
					{
						FiRn = ConstNum.FIOMEGA1_0N * 0.6 * rightBeam.WinConnect.MomentEndPlate.Material.Fy * rightBeam.WinConnect.MomentEndPlate.Width * rightBeam.WinConnect.MomentEndPlate.Thickness;
						if (FiRn <= R_Ffu / 2)
						{
							loopAgain = true;
                            if (!rightBeam.WinConnect.MomentEndPlate.Thickness_User) 
                                rightBeam.WinConnect.MomentEndPlate.Thickness = CommonCalculations.PlateThickness(rightBeam.WinConnect.MomentEndPlate.Thickness + ConstNum.SIXTEENTH_INCH);
						}
						else
							loopAgain = false;
					} while (loopAgain);
                    if (!rightBeam.WinConnect.MomentEndPlate.Thickness_User) 
                        rightBeam.WinConnect.MomentEndPlate.Thickness = CommonCalculations.PlateThickness(rightBeam.WinConnect.MomentEndPlate.Thickness);

					// step 9 Shear Rupture (Out of Plane)
					do
					{
						An = (rightBeam.WinConnect.MomentEndPlate.Width - 2 * (rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ConstNum.EIGHTH_INCH * ConstNum.ONE_INCH)) * rightBeam.WinConnect.MomentEndPlate.Thickness;
						FiRn = ConstNum.FIOMEGA0_75N * 0.6 * rightBeam.WinConnect.MomentEndPlate.Material.Fu * An;
						if (FiRn <= R_Ffu / 2)
						{
							loopAgain = true;
                            if (!rightBeam.WinConnect.MomentEndPlate.Thickness_User) 
                                rightBeam.WinConnect.MomentEndPlate.Thickness = rightBeam.WinConnect.MomentEndPlate.Thickness + ConstNum.SIXTEENTH_INCH;
						}
						else
							loopAgain = false;
					} while (loopAgain);
                    if (!rightBeam.WinConnect.MomentEndPlate.Thickness_User) 
                        rightBeam.WinConnect.MomentEndPlate.Thickness = CommonCalculations.PlateThickness(rightBeam.WinConnect.MomentEndPlate.Thickness);

					// Step 10 Stiffener thickness
					ts_min = rightBeam.Shape.tw * rightBeam.Material.Fy / rightBeam.WinConnect.MomentEndPlate.Material.Fy;
					ts = ts_min;
					// stiffener width thickness ratio:
					Limit = 0.56 * Math.Pow(ConstNum.ELASTICITY / rightBeam.WinConnect.MomentEndPlate.Material.Fy, 0.5);
					do
					{
						ratio = CommonDataStatic.SeismicSettings.R_hst / ts;
						if (Limit < ratio)
						{
							loopAgain = true;
							ts = ts + ConstNum.SIXTEENTH_INCH;
						}
						else
							loopAgain = false;
					} while (loopAgain);
					
					CommonDataStatic.SeismicSettings.R_ts = CommonCalculations.PlateThickness(ts);

					// Stiffener welds:
					BeamFlangetoStiffenerWeld = rightBeam.WinConnect.MomentEndPlate.Material.Fu * ts / (1.41F * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					if (CommonDataStatic.SeismicSettings.R_ts > 0.375 * ConstNum.ONE_INCH)
						CommonDataStatic.SeismicSettings.StiffenerToEndPlateWeldIsCJP = true;
					else
					{
						CommonDataStatic.SeismicSettings.StiffenerToEndPlateWeldIsCJP = false;
						w_req = rightBeam.WinConnect.MomentEndPlate.Material.Fu * CommonDataStatic.SeismicSettings.R_ts / (1.273F * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						StiffenerToEndPlateWeld = w_req;
					}

					// Step 11 Bolt Shear Rupture Strength
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
						NB = 4;
					else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
						NB = 8;

					FiRn = NB * rightBeam.WinConnect.MomentEndPlate.Bolt.BoltStrength;

					// Step 12 Bolt Bearing
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
					{
						Ni = 2;
						Lc = pfi + pfo - (rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH) + rightBeam.Shape.tf;
					}
					else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
					{
						Ni = 4;
						Lc = Pb - (rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH);
					}
					No = Ni;
					Rni = Math.Min(1.2 * Lc * rightBeam.WinConnect.MomentEndPlate.Thickness * rightBeam.WinConnect.MomentEndPlate.Material.Fu, 2.4F * rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize * rightBeam.WinConnect.MomentEndPlate.Thickness * rightBeam.WinConnect.MomentEndPlate.Material.Fu);
					Rno = Rni;
					// On End Plate
					FiRn = ConstNum.FIOMEGA0_75N * Ni * Rni + ConstNum.FIOMEGA0_75N * No * Rno;

					// Step 13 Beam to EndPlate Welds
					// Step 14 Column Flange Flexural Yielding
					bcf = column.Shape.bf;
					s = 0.5 * Math.Pow(bcf * g, 0.5);
					c = pfi + pfo + rightBeam.Shape.tf;
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
						yc = (bcf / 2 * (h1 / s + h0 / s) + 2 / g * (h1 * (s + 3 * c / 4) + h0 * (s + c / 4) + Math.Pow(c, 2) / 2) + g / 2);
					else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
						yc = (bcf / 2 * (h1 / s + h4 / s) + 2 / g * (h1 * (Pb + c / 2 + s) + h2 * (Pb / 2 + c / 4) + h3 * (Pb / 2 + c / 2) + h4 * s) + g / 2);

					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
						extension = pfo + de;
					else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
						extension = pfo + Pb + de;

                    if (!rightBeam.WinConnect.MomentEndPlate.Length_User) 
                        rightBeam.WinConnect.MomentEndPlate.Length = rightBeam.Shape.d + 2 * extension;

					// Step 15 Required Stiffener Force
					// ColumnFlangeFlexuralDesignStrength:
					FidMcf = (fiOmega0_90n * column.Material.Fy * yc * Math.Pow(column.Shape.tf, 2));
					FidRn = FidMcf / (rightBeam.Shape.d - rightBeam.Shape.tf);
					FidRn15 = FidRn;

					// Step 16 Column Web Yielding
					ct = columnStiffener.BeamNearTopOfColumn ? 1 : 0.5;

					Rn = ct * (6 * column.Shape.kdes + rightBeam.Shape.tf + 2 * rightBeam.WinConnect.MomentEndPlate.Thickness) * column.Material.Fy * column.Shape.tw;

					FidRn = ConstNum.FIOMEGA1_0N * Rn;
					FidRn16 = FidRn;
					// Step 17 ColumnWebBuckling
					if (columnStiffener.TopOfBeamToColumn >= column.Shape.d / 2)
						FiRn = ConstNum.FIOMEGA0_75N * 24 * Math.Pow(column.Shape.tw, 3) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy, 0.5) / column.Shape.t;
					else
						FiRn = ConstNum.FIOMEGA0_75N * 12 * Math.Pow(column.Shape.tw, 3) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy, 0.5) / column.Shape.t;

					FidRn17 = FiRn;

					// Step 18 Column Web Crippling
					n = rightBeam.Shape.tf;
					if (columnStiffener.TopOfBeamToColumn >= column.Shape.d / 2)
						FiRn = ConstNum.FIOMEGA0_75N * 0.8 * Math.Pow(column.Shape.tw, 2) * (1 + 3 * (n / column.Shape.d) * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);
					else
					{
						if (n / column.Shape.d < 0.2)
							FiRn = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(column.Shape.tw, 2) * (1 + 3 * (n / column.Shape.d) * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);
						else
							FiRn = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(column.Shape.tw, 2) * (1 + (4 * n / column.Shape.d - 0.2) * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);
					}
					FidRn18 = FiRn;

					// Step 19 Stiffener Required Strength
					R_Fsu = R_Ffu - Math.Min(Math.Min(FidRn15, FidRn16), Math.Min(FidRn17, FidRn18));

					// Step 10 Panel Zone per Provisions Section 9.3 for SMF and 10.3 for IMF
					rightBeam.MomentConnection = EMomentCarriedBy.EndPlate;
					if (!rightBeam.EndSetback_User)
						rightBeam.EndSetback = rightBeam.WinConnect.MomentEndPlate.Thickness;
                    if (!rightBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt_User) 
                        rightBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt = pfi;
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
                        if (!rightBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts_User) 
                            rightBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts = 8;
					else
                        if (!rightBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts_User) 
                            rightBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts = 4;

                    if (!rightBeam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir_User) 
                        rightBeam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir = Pb;
					rightBeam.WinConnect.MomentEndPlate.BraceStiffener.Length = CommonDataStatic.SeismicSettings.R_lst;

					rightBeam.WinConnect.MomentEndPlate.BraceStiffener.Thickness = ts;
					rightBeam.Moment = CommonDataStatic.SeismicSettings.R_Mf;
					rightBeam.ShearForce = CommonDataStatic.SeismicSettings.R_Vu;
				}

			} while (rightBeam.WinConnect.MomentEndPlate.Thickness != RPreviousThickness);

			do
			{
				LPreviousThickness = leftBeam.WinConnect.MomentEndPlate.Thickness;

				if (leftBeam.IsActive)
				{
					SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BeamDepth, ref MaxValue, ref MinValue);

					SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BeamFlangeThickness, ref MaxValue, ref MinValue);

					if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.SMF)
						LSpanToDepthRatio = CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft / leftBeam.Shape.d;
					else if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.IMF)
						LSpanToDepthRatio = CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft / leftBeam.Shape.d;

					// (6) slenderness check:
					LambdaPF = 0.38 * Math.Pow(ConstNum.ELASTICITY / leftBeam.Material.Fy, 0.5);
					ratio = leftBeam.Shape.bf / leftBeam.Shape.tf / 2;

					LambdaPW = 3.76 * Math.Pow(ConstNum.ELASTICITY / leftBeam.Material.Fy, 0.5);
					ratio = leftBeam.Shape.t / leftBeam.Shape.tw;

					SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltDistanceFromBeamFlange, ref MaxValue, ref MinValue);

					pfi = 2;

					if (pfi > MaxValue)
						pfi = MaxValue;
					if (pfi < MinValue)
						pfi = MinValue;
					pfo = pfi;

					// 6.10 Design Procedure
					//  Step 1
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
					{
						SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltVerticalSpacing, ref MaxValue, ref MinValue);

						Pb = -((int) Math.Floor((double) -4 * (2 + 2 / 3) * leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize)) / 4.0;
						if (Pb > MaxValue)
							Pb = MaxValue;
						if (Pb < MinValue)
							Pb = MinValue;
					}

					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4)
						CommonDataStatic.SeismicSettings.L_sh = Math.Min(leftBeam.Shape.d / 2, 3 * leftBeam.Shape.bf);
					else
					{
						// EPS4, EPS8
						if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
							CommonDataStatic.SeismicSettings.L_hst = pfo + leftBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir;
						else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
							CommonDataStatic.SeismicSettings.L_hst = pfo + leftBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir + leftBeam.WinConnect.MomentEndPlate.Bolt.SpacingLongDir;
						CommonDataStatic.SeismicSettings.L_lst = -((int) Math.Floor(-4 * ((CommonDataStatic.SeismicSettings.L_hst - ConstNum.ONE_INCH) / 0.57735 + ConstNum.ONE_INCH))) / 4.0;

						CommonDataStatic.SeismicSettings.L_sh = leftBeam.WinConnect.MomentEndPlate.Thickness + CommonDataStatic.SeismicSettings.L_lst; // Calculate tp and Lst before here
					}
					CommonDataStatic.SeismicSettings.GravityLoadLeft = CommonDataStatic.SeismicSettings.GravityLoadLeft;
					P = CommonDataStatic.SeismicSettings.GravityLoadLeft;
					a1 = CommonDataStatic.SeismicSettings.DistanceToGravityLoadLeft;
					b1 = CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft - a1;
					L_Vg = Math.Max(P * b1 / CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft, P * a1 / CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft);

					leftBeam.WinConnect.Fema.Ry = leftBeam.Material.Ry;
					L_Cpr = (leftBeam.Material.Fy + leftBeam.Material.Fu) / (2 * leftBeam.Material.Fy);
					L_Mpe = L_Cpr * leftBeam.WinConnect.Fema.Ry * leftBeam.Material.Fy * leftBeam.Shape.zx; // probable moment at hinge
					CommonDataStatic.SeismicSettings.L_Vu = 2 * L_Mpe / (CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft - 2 * CommonDataStatic.SeismicSettings.L_sh) + L_Vg;
					CommonDataStatic.SeismicSettings.L_Mf = L_Mpe + CommonDataStatic.SeismicSettings.L_Vu * CommonDataStatic.SeismicSettings.L_sh;
					SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.EndPlateWidth, ref MaxValue, ref MinValue);

					Bp = leftBeam.Shape.bf + ConstNum.ONE_INCH;
					if (Bp > MaxValue)
						Bp = MaxValue;
					if (Bp < MinValue)
						Bp = MinValue;
                    if (!leftBeam.WinConnect.MomentEndPlate.Width_User) 
                        leftBeam.WinConnect.MomentEndPlate.Width = Bp;
					
					// step 2
					SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltGage, ref MaxValue, ref MinValue);

					Gage = column.GageOnFlange;
					if (Gage > MaxValue)
						Gage = MaxValue;
					if (Gage < MinValue)
						Gage = MinValue;
					g = Gage;

					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
					{
						SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltVerticalSpacing, ref MaxValue, ref MinValue);

						Pb = -((int) Math.Floor((double) -4 * (2 + 2 / 3) * leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize)) / 4.0;
						if (Pb > MaxValue)
							Pb = MaxValue;
						if (Pb < MinValue)
							Pb = MinValue;
					}

					// Step 3 ' Bolt diameter
					switch (leftBeam.WinConnect.MomentEndPlate.Bolt.ASTMType)
					{
						case EBoltASTM.A325:
							Fnt = 90; // ksi
							break;
						case EBoltASTM.A490:
							Fnt = 113; // ksi
							break;
					}
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4)
					{
						h0 = leftBeam.Shape.d - leftBeam.Shape.tf / 2 + pfo;
						h1 = h0 - leftBeam.Shape.tf - pfi - pfo;
						dboltRequired = Math.Pow(2 * leftBeam.WinConnect.Fema.Mf / (Math.PI * ConstNum.FIOMEGA0_75N * Fnt * (h0 + h1)), 0.5);
					}
					else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
					{
						h1 = leftBeam.Shape.d - leftBeam.Shape.tf / 2 + pfo + Pb;
						h2 = h1 - Pb;
						h3 = h2 - pfo - leftBeam.Shape.tf - pfi;
						h4 = h3 - Pb;
						dboltRequired = Math.Pow(2 * leftBeam.WinConnect.Fema.Mf / (Math.PI * ConstNum.FIOMEGA0_75N * Fnt * (h1 + h2 + h3 + h4)), 0.5);
					}
					dboltRequired = -((int) Math.Floor(-8 * dboltRequired)) / 8.0;
                    de = leftBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir;

					// Step 4  ???? What purpose
					// Step 5 End Plate thickness
					s = 0.5 * Math.Pow(Bp * g, 0.5);
					if (pfi > s)
						pfi = s;
					pfo = pfi;
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4)
						yp = Bp / 2 * (h1 * (1 / pfi + 1 / s) + h0 * (1 / pfo) - 0.5) + 2 / g * (h1 * (pfi + s));
					else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
					{
						if (de <= s)
							yp = Bp / 2 * (h1 * (1 / pfi + 1 / s) + h0 * (1 / pfo + 1 / (2 * s))) + 2 / g * (h1 * (pfi + s) + h0 * (de + pfo));
						else
							yp = Bp / 2 * (h1 * (1 / pfi + 1 / s) + h0 * (1 / pfo + 1 / s)) + 2 / g * (h1 * (pfi + s) + h0 * (s + pfo));

					}
					else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
					{
						if (de <= s)
							yp = Bp / 2 * (h1 * (1 / (2 * de)) + h2 * (1 / pfo) + h3 * (1 / pfi) + h4 / s) + 2 / g * (h1 * (de + Pb / 4) + h2 * (pfo + 3 * Pb / 4) + h3 * (pfi + Pb / 4) + h4 * (s + 3 * Pb / 4) + Math.Pow(Pb, 2)) + g;
						else
							yp = Bp / 2 * (h1 * (1 / s) + h2 * (1 / pfo) + h3 * (1 / pfi) + h4 / s) + 2 / g * (h1 * (de + Pb / 4) + h2 * (pfo + 3 * Pb / 4) + h3 * (pfi + Pb / 4) + h4 * (s + 3 * Pb / 4) + Math.Pow(Pb, 2)) + g;
					}

					tprequired = 0.75;
					do
					{
						CommonDataStatic.SeismicSettings.L_sh = CommonDataStatic.SeismicSettings.L_lst + tprequired;
						CommonDataStatic.SeismicSettings.L_Mf = L_Mpe + CommonDataStatic.SeismicSettings.L_Vu * CommonDataStatic.SeismicSettings.L_sh;
						tp_Req = Math.Pow(1.11 * CommonDataStatic.SeismicSettings.L_Mf / (ConstNum.FIOMEGA0_9N * leftBeam.WinConnect.MomentEndPlate.Material.Fy * yp), 0.5);
                        if (!leftBeam.WinConnect.MomentEndPlate.Thickness_User) 
                            leftBeam.WinConnect.MomentEndPlate.Thickness = CommonCalculations.PlateThickness(Math.Max(tp_Req, tprequired));

						if (tp_Req > tprequired)
						{
							tprequired = leftBeam.WinConnect.MomentEndPlate.Thickness;
							devamet = true;
						}
						else
							devamet = false;
					} while (devamet);

					// Step 6
					// step 7 Beam Flange Force
					L_Ffu = CommonDataStatic.SeismicSettings.L_Mf / (leftBeam.Shape.d - leftBeam.Shape.tf);

					// step 8 End Plate Shear Yielding (Out of Plane)
					do
					{
						FiRn = ConstNum.FIOMEGA1_0N * 0.6 * leftBeam.WinConnect.MomentEndPlate.Material.Fy * leftBeam.WinConnect.MomentEndPlate.Width * leftBeam.WinConnect.MomentEndPlate.Thickness;
						if (FiRn <= L_Ffu / 2)
						{
							loopAgain = true;
                            if (!leftBeam.WinConnect.MomentEndPlate.Thickness_User) 
                                leftBeam.WinConnect.MomentEndPlate.Thickness = CommonCalculations.PlateThickness(leftBeam.WinConnect.MomentEndPlate.Thickness + ConstNum.SIXTEENTH_INCH);
						}
						else
							loopAgain = false;
					} while (loopAgain);

                    if (!leftBeam.WinConnect.MomentEndPlate.Thickness_User) 
                        leftBeam.WinConnect.MomentEndPlate.Thickness = CommonCalculations.PlateThickness(leftBeam.WinConnect.MomentEndPlate.Thickness);

					// step 9 Shear Rupture (Out of Plane)
					do
					{
						An = (leftBeam.WinConnect.MomentEndPlate.Width - 2 * (leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ConstNum.EIGHTH_INCH * ConstNum.ONE_INCH)) * leftBeam.WinConnect.MomentEndPlate.Thickness;
						FiRn = ConstNum.FIOMEGA0_75N * 0.6 * leftBeam.WinConnect.MomentEndPlate.Material.Fu * An;
						if (FiRn <= L_Ffu / 2)
						{
							loopAgain = true;
                            if (!leftBeam.WinConnect.MomentEndPlate.Thickness_User) 
                                leftBeam.WinConnect.MomentEndPlate.Thickness = leftBeam.WinConnect.MomentEndPlate.Thickness + ConstNum.SIXTEENTH_INCH;
						}
						else
							loopAgain = false;
					} while (loopAgain);
                    if (!leftBeam.WinConnect.MomentEndPlate.Thickness_User) 
                        leftBeam.WinConnect.MomentEndPlate.Thickness = CommonCalculations.PlateThickness(leftBeam.WinConnect.MomentEndPlate.Thickness);

					// Step 10 Stiffener thickness
					ts_min = leftBeam.Shape.tw * leftBeam.Material.Fy / leftBeam.WinConnect.MomentEndPlate.Material.Fy;
					ts = ts_min;
					// stiffener width thickness ratio:
					Limit = 0.56 * Math.Pow(ConstNum.ELASTICITY / leftBeam.WinConnect.MomentEndPlate.Material.Fy, 0.5);
					do
					{
						ratio = CommonDataStatic.SeismicSettings.L_hst / ts;

						if (Limit < ratio)
						{
							loopAgain = true;
							ts += ConstNum.SIXTEENTH_INCH;
						}
						else
							loopAgain = false;
					} while (loopAgain);
					CommonDataStatic.SeismicSettings.L_ts = CommonCalculations.PlateThickness(ts);

					if (CommonDataStatic.SeismicSettings.L_ts > 0.375 * ConstNum.ONE_INCH)
						CommonDataStatic.SeismicSettings.StiffenerToEndPlateWeldIsCJP = true;
					else
					{
						CommonDataStatic.SeismicSettings.StiffenerToEndPlateWeldIsCJP = false;
						w_req = leftBeam.WinConnect.MomentEndPlate.Material.Fu * CommonDataStatic.SeismicSettings.L_ts / (1.273F * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					}

					// Step 11 Bolt Shear Rupture Strength
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
						NB = 4;
					else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
						NB = 8;

					// Step 12 Bolt Bearing
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
					{
						Ni = 2;
						Lc = pfi + pfo - (leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH) + leftBeam.Shape.tf;
					}
					else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
					{
						Ni = 4;
						Lc = Pb - (leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH);
					}

					No = Ni;
					Lni = Math.Min(1.2 * Lc * leftBeam.WinConnect.MomentEndPlate.Thickness * leftBeam.WinConnect.MomentEndPlate.Material.Fu, 2.4F * leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize * leftBeam.WinConnect.MomentEndPlate.Thickness * leftBeam.WinConnect.MomentEndPlate.Material.Fu);
					Lno = Lni;
					// On End Plate
					FiRn = ConstNum.FIOMEGA0_75N * Ni * Lni + ConstNum.FIOMEGA0_75N * No * Lno;

					// Step 14 Column Flange Flexural Yielding
					bcf = column.Shape.bf;
					s = 0.5 * Math.Pow(bcf * g, 0.5);
					c = pfi + pfo + leftBeam.Shape.tf;
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
						yc = (bcf / 2 * (h1 / s + h0 / s) + 2 / g * (h1 * (s + 3 * c / 4) + h0 * (s + c / 4) + Math.Pow(c, 2) / 2) + g / 2);
					else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
						yc = (bcf / 2 * (h1 / s + h4 / s) + 2 / g * (h1 * (Pb + c / 2 + s) + h2 * (Pb / 2 + c / 4) + h3 * (Pb / 2 + c / 2) + h4 * s) + g / 2);

					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
						extension = pfo + de;
					else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
						extension = pfo + Pb + de;

                    if (!leftBeam.WinConnect.MomentEndPlate.Length_User) 
                        leftBeam.WinConnect.MomentEndPlate.Length = leftBeam.Shape.d + 2 * extension;

					// Step 15 Required Stiffener Force
					// ColumnFlangeFlexuralDesignStrength:
					FidMcf = (fiOmega0_90n * column.Material.Fy * yc * Math.Pow(column.Shape.tf, 2));
					FidRn = FidMcf / (leftBeam.Shape.d - leftBeam.Shape.tf);
					FidRn15 = FidRn;

					// Step 16 Column Web Yielding
					ct = columnStiffener.BeamNearTopOfColumn ? 1 : 0.5;

					Rn = ct * (6 * column.Shape.kdes + leftBeam.Shape.tf + 2 * leftBeam.WinConnect.MomentEndPlate.Thickness) * column.Material.Fy * column.Shape.tw;
					FidRn = ConstNum.FIOMEGA1_0N * Rn;

					FidRn16 = FidRn;
					// Step 17 ColumnWebBuckling
					if (columnStiffener.TopOfBeamToColumn >= column.Shape.d / 2)
						FiRn = ConstNum.FIOMEGA0_75N * 24 * Math.Pow(column.Shape.tw, 3) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy, 0.5) / column.Shape.t;
					else
						FiRn = ConstNum.FIOMEGA0_75N * 12 * Math.Pow(column.Shape.tw, 3) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy, 0.5) / column.Shape.t;
					FidRn17 = FiRn;

					// Step 18 Column Web Crippling
					n = leftBeam.Shape.tf;
					if (columnStiffener.TopOfBeamToColumn >= column.Shape.d / 2)
						FiRn = ConstNum.FIOMEGA0_75N * 0.8 * Math.Pow(column.Shape.tw, 2) * (1 + 3 * (n / column.Shape.d) * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);
					else
					{
						if (n / column.Shape.d < 0.2)
							FiRn = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(column.Shape.tw, 2) * (1 + 3 * (n / column.Shape.d) * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);
						else
							FiRn = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(column.Shape.tw, 2) * (1 + (4 * n / column.Shape.d - 0.2) * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);
					}

					FidRn18 = FiRn;

					// Step 19 Stiffener Required Strength
					L_Fsu = L_Ffu - Math.Min(Math.Min(FidRn15, FidRn16), Math.Min(FidRn17, FidRn18));

					if (!columnStiffener.SWidth_User)
						columnStiffener.SWidth = (column.Shape.bf - 2 * column.Shape.k1) / 2;
					if (!columnStiffener.SThickness_User)
						columnStiffener.SThickness = Math.Max(ConstNum.FIOMEGA0_9N * (L_Fsu + R_Fsu) / (leftBeam.WinConnect.MomentEndPlate.Material.Fy * columnStiffener.SWidth), ConstNum.FIOMEGA0_75N * (L_Ffu + R_Ffu) / (leftBeam.WinConnect.MomentEndPlate.Material.Fy * columnStiffener.SWidth)) / 2; 
					columnStiffener.SLength = column.Shape.d - 2 * column.Shape.tf;

					// Step 10 Panel Zone per Provisions Section 9.3 for SMF and 10.3 for IMF
					leftBeam.MomentConnection = EMomentCarriedBy.EndPlate;
					if (!leftBeam.EndSetback_User)
						leftBeam.EndSetback = leftBeam.WinConnect.MomentEndPlate.Thickness;
                    if (!leftBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt_User) 
                        leftBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt = pfi;
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
                        if (!leftBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts_User) 
                            leftBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts = 8;
					else
                        if (!leftBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts_User) 
                            leftBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts = 4;
                    if (!leftBeam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir_User) 
                        leftBeam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir = Pb;
					leftBeam.WinConnect.MomentEndPlate.BraceStiffener.Length = CommonDataStatic.SeismicSettings.L_lst;
					LMomentEndPlateStiffenerHeight = CommonDataStatic.SeismicSettings.L_hst;

					leftBeam.WinConnect.MomentEndPlate.BraceStiffener.Thickness = ts;
					leftBeam.Moment = CommonDataStatic.SeismicSettings.L_Mf;
					leftBeam.ShearForce = CommonDataStatic.SeismicSettings.L_Vu;
				}
			} while (leftBeam.WinConnect.MomentEndPlate.Thickness != LPreviousThickness);

			for (int i = 0; i < 2; i++)
			{
				SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.PlateThickness, ref MaxValue, ref MinValue);
				SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.EndPlateWidth, ref MaxValue, ref MinValue);
				SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltGage, ref MaxValue, ref MinValue);
				SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltDistanceFromBeamFlange, ref MaxValue, ref MinValue);
				SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltVerticalSpacing, ref MaxValue, ref MinValue);
				SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BeamDepth, ref MaxValue, ref MinValue);
				SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BeamFlangeThickness, ref MaxValue, ref MinValue);
				SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BeamFlangeWidth, ref MaxValue, ref MinValue);

				if (rightBeam.IsActive)
					SeismicEndPlateReport_Right();

				if (leftBeam.IsActive)
					SeismicEndPlateReport_Left();

				if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.SMF || CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.IMF)
				{
					if (C_B_Ratio <= 0)
					{
						Reporting.AddHeader("Column-Beam Moment Ratio");
						switch (CommonDataStatic.Preferences.CalcMode)
						{
							case ECalcMode.LRFD:
								Puc = column.P;
								Sum_Mpc = 2 * column.Shape.zx * (column.Material.Fy - Puc / column.Shape.a);
								Reporting.AddHeader("Sum_Mpc = 2 * Zc * (Fyc - Puc / Agc)");
								Reporting.AddLine("= 2 * " + column.Shape.zx + " * (" + column.Material.Fy + " - " + Puc + " / " + column.Shape.a + ")");
								Reporting.AddLine("= " + Sum_Mpc + ConstUnit.MomentUnitInch);
								break;
							case ECalcMode.ASD:
								Pac = column.P;
								Sum_Mpc = 2 * column.Shape.zx * (column.Material.Fy / 1.5 - Pac / column.Shape.a);
								Reporting.AddHeader("Sum_Mpc = 2 * Zc * (Fyc/1.5 - Puc / Agc)");
								Reporting.AddLine("= 2 * " + " * (" + column.Material.Fy / 1.5 + " - " + Puc + " / " + column.Shape.a + ")");
								Reporting.AddLine("= " + Sum_Mpc + ConstUnit.MomentUnitInch);
								break;
						}
						R_Mv = CommonDataStatic.SeismicSettings.R_Vu * (CommonDataStatic.SeismicSettings.R_sh + column.Shape.d / 2);
						L_Mv = CommonDataStatic.SeismicSettings.L_Vu * (CommonDataStatic.SeismicSettings.L_sh + column.Shape.d / 2);
						Reporting.AddLine("R_Mv = R_Vu * (R_sh + dc / 2)");
						Reporting.AddLine(" = " + CommonDataStatic.SeismicSettings.R_Vu + " * (" + CommonDataStatic.SeismicSettings.R_sh + " + " + (column.Shape.d / 2) + " )");
						Reporting.AddLine("= " + R_Mv + ConstUnit.MomentUnitInch);

						Reporting.AddLine("L_Mv = L_Vu * (L_sh+ dc / 2)");
						Reporting.AddLine(" = " + CommonDataStatic.SeismicSettings.L_Vu + " * (" + CommonDataStatic.SeismicSettings.L_sh + " + " + (column.Shape.d / 2) + " )");
						Reporting.AddLine("= " + L_Mv + ConstUnit.MomentUnitInch);

						Sum_Mpb = CommonDataStatic.SeismicSettings.R_Mf + CommonDataStatic.SeismicSettings.L_Mf + R_Mv + L_Mv;
						Reporting.AddLine("Sum_Mpb = R_Mf + L_Mf + R_Mv + L_Mv");
						Reporting.AddLine("= " + CommonDataStatic.SeismicSettings.R_Mf + " + " + CommonDataStatic.SeismicSettings.L_Mf + " + " + R_Mv + " + " + L_Mv);
						Reporting.AddLine("= " + Sum_Mpb + ConstUnit.MomentUnitInch);

						C_B_Ratio = Sum_Mpc / Sum_Mpb;

						Reporting.AddLine("Sum_Mpc / Sum_Mpb = " + Sum_Mpc + " / " + Sum_Mpb);

						if (Sum_Mpc / Sum_Mpb > 1)
							Reporting.AddLine("= " + C_B_Ratio + " > 1.0 (OK)");
						else
							Reporting.AddLine("= " + C_B_Ratio + " << 1.0 (NG)");
					}
				}
			}
		}

		public static void SeismicEndPlateReport_Right()
		{
			double R_Fsu = 0;
			double minFsu = 0;
			double FinRn18 = 0;
			double n = 0;
			double FinRn17 = 0;
			double FidRn16 = 0;
			double Rn = 0;
			double ct = 0;
			double FidRn15 = 0;
			double FidRn = 0;
			double FidMcf = 0;
			double extension = 0;
			double tcf_req = 0;
			int pso = 0;
			int psi = 0;
			double Yc_Unstiffened = 0;
			double yc = 0;
			double c = 0;
			double bcf = 0;
			double Rno = 0;
			double Rni = 0;
			double No = 0;
			double db = 0;
			double tf = 0;
			double Lc = 0;
			double Ni = 0;
			int NB = 0;
			double EndPlatetoStiffenerWeld = 0;
			double BeamFlangetoStiffenerWeld = 0;
			double w_req = 0;
			double Limit = 0;
			double ts = 0;
			double ts_min = 0;
			bool loopAgain = false;
			double FiRn = 0;
			double FiRnR = 0;
			double An = 0;
			double FiRnY = 0;
			double R_Ffu = 0;
			double tp_Req = 0;
			double yp = 0;
			double s = 0;
			double de = 0;
			double h4 = 0;
			double h3 = 0;
			double h2 = 0;
			double dboltRequired = 0;
			double h1 = 0;
			double h0 = 0;
			double Fnt = 0;
			double g = 0;
			double Gage = 0;
			double Bp = 0;
			double R_Mpe = 0;
			double R_Cpr = 0;
			double R_Vg = 0;
			double b1 = 0;
			double a1 = 0;
			double P = 0;
			double Pb = 0;
			double pfo = 0;
			double pfi = 0;
			double LambdaPW = 0;
			double ratio = 0;
			double LambdaPF = 0;
			double RSpanToDepthRatio = 0;
			double MinValue = 0;
			double MaxValue = 0;

			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			if (!rightBeam.IsActive)
				return;

			Reporting.AddHeader("Right Side Beam Limitations:");
			SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BeamDepth, ref MaxValue, ref MinValue);

			// (3)none
			// (4) Flange Thickness
			SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BeamFlangeThickness, ref MaxValue, ref MinValue);

			// (5)
			Reporting.AddHeader("Beam Span-to-Depth Ratio:");
			if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.SMF)
			{
				RSpanToDepthRatio = CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight / rightBeam.Shape.d;
				Reporting.AddLine("L/d = " + CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight + " / " + rightBeam.Shape.d);

				if (RSpanToDepthRatio >= 7)
					Reporting.AddLine("= " + RSpanToDepthRatio + " >= 7 (OK)");
				else
					Reporting.AddLine("= " + RSpanToDepthRatio + " << 7 (NG)");
			}
			else if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.IMF)
			{
				RSpanToDepthRatio = CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight / rightBeam.Shape.d;
				Reporting.AddLine("L/d = " + CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight + " / " + rightBeam.Shape.d);

				if (RSpanToDepthRatio >= 5)
					Reporting.AddLine("= " + RSpanToDepthRatio + " >= 5 (OK)");
				else
					Reporting.AddLine("= " + RSpanToDepthRatio + " << 5 (NG)");
			}

			// (6) slenderness check:
			Reporting.AddHeader("Beam Flange Slenderness:");
			LambdaPF = 0.38 * Math.Pow(ConstNum.ELASTICITY / rightBeam.Material.Fy, 0.5);
			Reporting.AddLine("Limiting b/t = 0.38 * (E/Fy)^0.5 = 0.38 * (" + ConstNum.ELASTICITY + " / " + rightBeam.Material.Fy + ")^0.5 = " + LambdaPF);
			ratio = rightBeam.Shape.bf / rightBeam.Shape.tf / 2;
			if (ratio <= LambdaPF)
				Reporting.AddLine("Beam Flange b/t = " + (rightBeam.Shape.bf / 2) + " / " + rightBeam.Shape.tf + " = " + ratio + " <= " + LambdaPF + " (OK)");
			else
				Reporting.AddLine("Beam Flange b/t = " + (rightBeam.Shape.bf / 2) + " / " + rightBeam.Shape.tf + " = " + ratio + " >> " + LambdaPF + " (NG)");

			Reporting.AddHeader("Beam Web Slenderness:");
			LambdaPW = 3.76 * Math.Pow(ConstNum.ELASTICITY / rightBeam.Material.Fy, 0.5);
			Reporting.AddLine("Limiting h/tw = 3.76 * (E/Fy)^0.5 = 3.76 * (" + ConstNum.ELASTICITY + " / " + rightBeam.Material.Fy + ")^0.5 = " + LambdaPW);
			ratio = rightBeam.Shape.t / rightBeam.Shape.tw;
			if (ratio <= LambdaPW)
				Reporting.AddLine("Beam Web h/t = " + rightBeam.Shape.t + " / " + rightBeam.Shape.tw + " = " + ratio + " <= " + LambdaPW + " (OK)");
			else
				Reporting.AddLine("Beam Web h/t = " + rightBeam.Shape.t + " / " + rightBeam.Shape.tw + " = " + ratio + " >> " + LambdaPW + " (NG)");

			SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltDistanceFromBeamFlange, ref MaxValue, ref MinValue);

			pfi = ConstNum.TWO_INCHES;
			if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
			{
				if (pfi > MaxValue)
					pfi = MaxValue;
				if (pfi < MinValue)
					pfi = MinValue;
				pfo = pfi;
			}

			// 6.10 Design Procedure
			//  Step 1
			if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
			{
				SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltVerticalSpacing, ref MaxValue, ref MinValue);

				Pb = -((int) Math.Floor((double) -4 * (2 + 2 / 3) * rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize)) / 4.0;
				if (Pb > MaxValue)
					Pb = MaxValue;
				if (Pb < MinValue)
					Pb = MinValue;
			}
			rightBeam.WinConnect.MomentEndPlate.Bolt.SpacingLongDir = Pb;
			Reporting.AddHeader("Right Side Beam End Plate Design:");
			if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4)
				Reporting.AddHeader("Distance from column face to Plastic Hinge (Sh):");
			else
			{
				if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
				{

					Reporting.AddLine("End Plate Stiffener Dimensions, Hst and Lst:");
					CommonDataStatic.SeismicSettings.R_hst = pfo + rightBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir;
					Reporting.AddLine("Hst = pfo + e = " + pfo + " + " + rightBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir + " = " + CommonDataStatic.SeismicSettings.R_hst + ConstUnit.Length);
				}
				else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
				{
					CommonDataStatic.SeismicSettings.R_hst = pfo + rightBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir + rightBeam.WinConnect.MomentEndPlate.Bolt.SpacingLongDir;
					Reporting.AddLine("End Plate Stiffener Dimensions, Hst and Lst:");
					Reporting.AddLine("Hst = pfo + pb + e = " + pfo + " + " + rightBeam.WinConnect.MomentEndPlate.Bolt.SpacingLongDir + " + " + rightBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir + " = " + CommonDataStatic.SeismicSettings.R_hst + ConstUnit.Length);
				}
				if (CommonDataStatic.SeismicSettings.ConnectionType != ESeismicConnectionType.E4)
				{

					CommonDataStatic.SeismicSettings.R_lst = -((int) Math.Floor(-4 * ((CommonDataStatic.SeismicSettings.R_hst - ConstNum.ONE_INCH) / 0.57735 + ConstNum.ONE_INCH))) / 4.0;
					Reporting.AddLine("Lst = (Hst- " + ConstNum.ONE_INCH + ") / 0.57735 +" + ConstNum.ONE_INCH + " = " + (CommonDataStatic.SeismicSettings.R_hst - ConstNum.ONE_INCH) + " / 0.57735 + " + ConstNum.ONE_INCH + " = " + CommonDataStatic.SeismicSettings.R_lst + ConstUnit.Length);
					Reporting.AddLine("Distance from column face to Plastic Hinge (Sh):");
					CommonDataStatic.SeismicSettings.R_sh = rightBeam.WinConnect.MomentEndPlate.Thickness + CommonDataStatic.SeismicSettings.R_lst;
					Reporting.AddLine("sh = tp + Lst = " + rightBeam.WinConnect.MomentEndPlate.Thickness + " + " + CommonDataStatic.SeismicSettings.R_lst + " = " + CommonDataStatic.SeismicSettings.R_sh + ConstUnit.Length);
				}
			}

			Reporting.AddHeader("Resultant Gravity Load:");
			CommonDataStatic.SeismicSettings.GravityLoadRight = CommonDataStatic.SeismicSettings.GravityLoadRight;
			P = CommonDataStatic.SeismicSettings.GravityLoadRight;
			a1 = CommonDataStatic.SeismicSettings.DistanceToGravityLoadRight;
			Reporting.AddLine("Vgr = " + CommonDataStatic.SeismicSettings.GravityLoadRight + ConstUnit.Force);
			Reporting.AddLine("Distance from Column Face to Vgr, a1");
			Reporting.AddLine("a1 = " + a1 + ConstUnit.Length);
			b1 = CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight - a1;
			Reporting.AddLine("Distance from Far_End_Column Face to Vgr, b1");
			Reporting.AddLine("b1 = " + b1 + ConstUnit.Length);
			
			R_Vg = Math.Max(P * b1 / CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight, P * a1 / CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight);	
			Reporting.AddHeader("Beam Gravity End_Force");
			Reporting.AddLine("Vg = max((Vgr * b1) / L, (Vgr * a1) / L)");
			Reporting.AddLine("= max((" + CommonDataStatic.SeismicSettings.GravityLoadRight + " * " + b1 + ") /" + CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight + " , (" + CommonDataStatic.SeismicSettings.GravityLoadRight + " * " + a1 + ") /" + CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight + ")");
			Reporting.AddLine("= " + R_Vg + ConstUnit.Force);

			rightBeam.WinConnect.Fema.Ry = rightBeam.Material.Ry;
			R_Cpr = (rightBeam.Material.Fy + rightBeam.Material.Fu) / (2 * rightBeam.Material.Fy);
			Reporting.AddLine("Cpr = (Fy + Fu) / (2 * Fy)");
			Reporting.AddLine("= (" + rightBeam.Material.Fy + " + " + rightBeam.Material.Fu + ") / (2 * " + rightBeam.Material.Fy + ")");
			Reporting.AddLine("= " + R_Cpr);

			Reporting.AddHeader("Probable maximum Moment at hinge:");
			R_Mpe = R_Cpr * rightBeam.WinConnect.Fema.Ry * rightBeam.Material.Fy * rightBeam.Shape.zx; // probable moment at hinge
			Reporting.AddLine("Mpe = Cpr * Ry * Fy * Z"); // probable moment at hinge"
			Reporting.AddLine("= " + R_Cpr + " * " + rightBeam.WinConnect.Fema.Ry + " * " + rightBeam.Material.Fy + " * " + rightBeam.Shape.zx);
			Reporting.AddLine("= " + R_Mpe + ConstUnit.MomentUnitInch);

			Reporting.AddHeader("Shear Force at Beam end:");
			Reporting.AddLine("Vu = 2 * Mpe / (L - 2 * sh) + Vg");
			Reporting.AddLine("= 2 * " + R_Mpe + "/ (" + CommonDataStatic.SeismicSettings.ClearSpanOfBeamRight + " - 2 * " + CommonDataStatic.SeismicSettings.R_sh + ") + " + R_Vg);
			Reporting.AddLine("= " + CommonDataStatic.SeismicSettings.R_Vu + ConstUnit.Force);

			Reporting.AddHeader("Moment at column face (Mf):");
			CommonDataStatic.SeismicSettings.R_Mf = R_Mpe + CommonDataStatic.SeismicSettings.R_Vu * CommonDataStatic.SeismicSettings.R_sh;
			Reporting.AddLine("Mf = Mpe + Vu * sh");
			Reporting.AddLine("= " + R_Mpe + " + " + CommonDataStatic.SeismicSettings.R_Vu + " * " + CommonDataStatic.SeismicSettings.R_sh);
			Reporting.AddLine("= " + CommonDataStatic.SeismicSettings.R_Mf + ConstUnit.MomentUnitInch);

			Bp = rightBeam.Shape.bf + ConstNum.ONE_INCH;
            if (!rightBeam.WinConnect.MomentEndPlate.Width_User) 
                rightBeam.WinConnect.MomentEndPlate.Width = Bp;

			SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.EndPlateWidth, ref MaxValue, ref MinValue);

			if (Bp > MaxValue)
				Bp = MaxValue;
			if (Bp < MinValue)
				Bp = MinValue;

            if (!rightBeam.WinConnect.MomentEndPlate.Width_User) 
                rightBeam.WinConnect.MomentEndPlate.Width = Bp;
			// step 2
			SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltGage, ref MaxValue, ref MinValue);

			Gage = column.GageOnFlange;

			if (Gage > MaxValue)
				Gage = MaxValue;
			if (Gage < MinValue)
				Gage = MinValue;
			g = Gage;

			if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
			{
				SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltVerticalSpacing, ref MaxValue, ref MinValue);

				Pb = -((int) Math.Floor((double) -4 * (2 + 2 / 3) * rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize)) / 4.0;
				if (Pb > MaxValue)
					Pb = MaxValue;
				if (Pb < MinValue)
					Pb = MinValue;
			}
			rightBeam.WinConnect.MomentEndPlate.Bolt.SpacingLongDir = Pb;

			// Step 3 ' Bolt diameter
			switch (rightBeam.WinConnect.MomentEndPlate.Bolt.ASTMType)
			{
				case EBoltASTM.A325:
					Fnt = 620; // N/mm²
					break;
				case EBoltASTM.A490:
					Fnt = 780; // N/mm²
					break;
			}

			Reporting.AddHeader("Required Bolt Diameter, db_req:");
			Reporting.AddLine("Fnt = " + Fnt + ConstUnit.Stress);

			if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4)
			{
				h0 = rightBeam.Shape.d - rightBeam.Shape.tf / 2 + pfo;
				h1 = h0 - rightBeam.Shape.tf - pfi - pfo;
				dboltRequired = Math.Pow(2 * CommonDataStatic.SeismicSettings.R_Mf / (Math.PI * ConstNum.FIOMEGA0_75N * Fnt * (h0 + h1)), 0.5);
				Reporting.AddLine("h0= " + rightBeam.Shape.d + " - " + rightBeam.Shape.tf + " / 2 + " + pfo + " = " + h0 + ConstUnit.Length);
				Reporting.AddLine("h1= " + h0 + " - " + rightBeam.Shape.tf + " - " + pfi + " = " + h1 + ConstUnit.Length);
				Reporting.AddLine("db_req = (2 * Mf / (pi * " + ConstString.FIOMEGA0_75 + " * Fnt * (h0 + h1))) ^ 0.5");
				Reporting.AddLine("=(2 * " + CommonDataStatic.SeismicSettings.R_Mf + " / (" + Math.PI + " * " + ConstString.FIOMEGA0_75 + " * " + Fnt + " * (" + h0 + " + " + h1 + "))) ^ 0.5");
				Reporting.AddLine("= " + dboltRequired + ConstUnit.Length);

			}
			else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
			{
				h1 = rightBeam.Shape.d - rightBeam.Shape.tf / 2 + pfo + Pb;
				h2 = h1 - Pb;
				h3 = h2 - pfo - rightBeam.Shape.tf - pfi;
				h4 = h3 - Pb;
				dboltRequired = Math.Pow(2 * CommonDataStatic.SeismicSettings.R_Mf / (Math.PI * ConstNum.FIOMEGA0_75N * Fnt * (h1 + h2 + h3 + h4)), 0.5);

				Reporting.AddLine("h1 = " + rightBeam.Shape.d + " - " + rightBeam.Shape.tf + " / 2 + " + pfo + " + " + Pb + " = " + h1 + ConstUnit.Length);
				Reporting.AddLine("h2 = " + h1 + " - " + Pb + " = " + h2 + ConstUnit.Length);
				Reporting.AddLine("h3 = " + h2 + " - " + pfo + " - " + rightBeam.Shape.tf + " - " + pfi + " = " + h3 + ConstUnit.Length);
				Reporting.AddLine("h4 = " + h3 + " - " + Pb + " = " + h4 + ConstUnit.Length);
				Reporting.AddLine("db_req = (2 * Mf / (pi * " + ConstString.FIOMEGA0_75 + " * Fnt * (h1 + h2 + h3 + h4))) ^ 0.5");
				Reporting.AddLine("= (2 * " + CommonDataStatic.SeismicSettings.R_Mf + " / (" + Math.PI + " * " + ConstString.FIOMEGA0_75 + " * " + Fnt + " * (" + h1 + " + " + h2 + " + " + h3 + " + " + h4 + "))) ^ 0.5");
				Reporting.AddLine("= " + dboltRequired + ConstUnit.Length);
			}
			dboltRequired = -((int) Math.Floor(-8 * dboltRequired)) / 8.0;
			Reporting.AddLine("Use " + dboltRequired + ConstUnit.Length + "Bolts");

			rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize = dboltRequired;
			de = rightBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir;
			
			// Step 4  ???? What purpose
			// Step 5 End Plate thickness
			Reporting.AddHeader("End Plate Thickness:");
			s = 0.5 * Math.Pow(Bp * g, 0.5);
			Reporting.AddLine("s = 0.5 * (bp * g) ^ 0.5");
			Reporting.AddLine("= 0.5 * (" + Bp + " * " + g + ") ^ 0.5 = " + s + ConstUnit.Length);

			if (pfi > s)
			{
				pfi = s;
				Reporting.AddLine("pfi = " + s + ConstUnit.Length);
				Reporting.AddLine("pfo = " + s + ConstUnit.Length);
			}
			pfo = pfi;

			if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4)
			{
				yp = Bp / 2 * (h1 * (1 / pfi + 1 / s) + h0 * (1 / pfo) - 0.5) + 2 / g * (h1 * (pfi + s));
				Reporting.AddLine("yp= Bp / 2 * (h1 * (1 / pfi + 1 / s) + h0 * (1 / Pfo) - 0.5) + 2 / g * (h1 * (pfi + s))");
				Reporting.AddLine("= " + Bp + " / 2 * (" + h1 + " * (1 / " + pfi + " + 1 / " + s + ") + " + h0 + " * (1 / " + pfo + ") - 0.5) + 2 / " + g + " * (" + h1 + " * (" + pfi + " + " + s + "))");
				Reporting.AddLine("= " + yp + ConstUnit.Length);
			}
			else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
			{
				if (de <= s)
				{
					yp = Bp / 2 * (h1 * (1 / pfi + 1 / s) + h0 * (1 / pfo + 1 / (2 * s))) + 2 / g * (h1 * (pfi + s) + h0 * (de + pfo));
					Reporting.AddLine("yp = " + Bp + " / 2 * (" + h1 + " * (1 / " + pfi + " + 1 / " + s + ") + " + h0 + " * (1 / " + pfo + " + 1 / (2 * " + s + ")) + 2 / " + g + " * (" + h1 + " * (" + pfi + " + " + s + ") + " + h0 + " * (" + de + " + " + pfo + "))");
					Reporting.AddLine("= " + yp + ConstUnit.Length);

				}
				else
				{
					yp = Bp / 2 * (h1 * (1 / pfi + 1 / s) + h0 * (1 / pfo + 1 / s)) + 2 / g * (h1 * (pfi + s) + h0 * (s + pfo));
					Reporting.AddLine("yp = " + Bp + " / 2 * (" + h1 + " * (1 / " + pfi + " + 1 / " + s + ") + " + h0 + " * (1 / " + pfo + " + 1 / " + s + ")) + 2 / " + g + " * (" + h1 + " * (" + pfi + " + " + s + ") + " + h0 + " * (" + s + " + " + pfo + "))");
					Reporting.AddLine("= " + yp + ConstUnit.Length);
				}
			}
			else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
			{

				if (de <= s)
				{
					yp = Bp / 2 * (h1 * (1 / (2 * de)) + h2 * (1 / pfo) + h3 * (1 / pfi) + h4 / s) + 2 / g * (h1 * (de + Pb / 4) + h2 * (pfo + 3 * Pb / 4) + h3 * (pfi + Pb / 4) + h4 * (s + 3 * Pb / 4) + Math.Pow(Pb, 2)) + g;
					Reporting.AddLine("yp = Bp / 2 * (h1 * (1 / (2 * de)) + h2 * (1 / Pfo) + h3 * (1 / pfi) + h4 / s)");
					Reporting.AddLine("+ 2 / g * (h1 * (de + pb / 4) + h2 * (Pfo + 3 * pb / 4) + h3 * (pfi + pb / 4) + h4 * (s + 3 * pb / 4) + pb ^ 2) + g");

					Reporting.AddLine("yp = " + Bp + " / 2 * (" + h1 + " * (1 / (2 * " + de + ")) + " + h2 + " * (1 / " + pfo + ") + " + h3 + " * (1 / " + pfi + ") + " + h4 + " / " + s + ") _");
					Reporting.AddLine("+ 2 / " + g + " * (" + h1 + " * (" + de + " + " + Pb + " / 4) + " + h2 + " * (" + pfo + " + 3 * " + Pb + " / 4) + " + h3 + " * (" + pfi + " + " + Pb + " / 4) + " + h4 + " * (" + s + " + 3 * " + Pb + " / 4) + " + Pb + " ^ 2) + " + g + "");
					Reporting.AddLine("= " + yp + ConstUnit.Length);
				}
				else
				{
					yp = Bp / 2 * (h1 * (1 / s) + h2 * (1 / pfo) + h3 * (1 / pfi) + h4 / s) + 2 / g * (h1 * (de + Pb / 4) + h2 * (pfo + 3 * Pb / 4) + h3 * (pfi + Pb / 4) + h4 * (s + 3 * Pb / 4) + Math.Pow(Pb, 2)) + g;

					Reporting.AddLine("yp = Bp / 2 * (h1 * (1 / s) + h2 * (1 / Pfo) + h3 * (1 / pfi) + h4 / s)");
					Reporting.AddLine("+ 2 / g * (h1 * (de + pb / 4) + h2 * (Pfo + 3 * pb / 4) + h3 * (pfi + pb / 4) + h4 * (s + 3 * pb / 4) + pb ^ 2) + g");

					Reporting.AddLine("yp = " + Bp + " / 2 * (" + h1 + " * (1 / " + s + ") + " + h2 + " * (1 / " + pfo + ") + " + h3 + " * (1 / " + pfi + ") + " + h4 + " / " + s + ")");
					Reporting.AddLine("+ 2 / " + g + " * (" + h1 + " * (" + de + " + " + Pb + " / 4) + " + h2 + " * (" + pfo + " + 3 * " + Pb + " / 4) + " + h3 + " * (" + pfi + " + " + Pb + " / 4) + " + h4 + " * (" + s + " + 3 * " + Pb + " / 4) + " + Pb + " ^ 2) + " + g);
					Reporting.AddLine("= " + yp + ConstUnit.Length);
				}
			}

			tp_Req = Math.Pow(1.11 * CommonDataStatic.SeismicSettings.R_Mf / (ConstNum.FIOMEGA0_9N * rightBeam.WinConnect.MomentEndPlate.Material.Fy * yp), 0.5);
			Reporting.AddLine("tp_Req = (1.11 * Mf / (" + ConstString.FIOMEGA0_9 + "* Fy * yp)) ^ 0.5");
			Reporting.AddLine("= (1.11 * " + CommonDataStatic.SeismicSettings.R_Mf + " / (" + ConstString.FIOMEGA0_9 + " * " + rightBeam.WinConnect.MomentEndPlate.Material.Fy + " * " + yp + ")) ^ 0.5");
			Reporting.AddLine("= " + tp_Req + ConstUnit.Length + " (Minimum)");

			// Step 6
			// step 7 Beam Flange Force			
			Reporting.AddHeader("Factored Beam Flange Force (Ffu):");
			R_Ffu = CommonDataStatic.SeismicSettings.R_Mf / (rightBeam.Shape.d - rightBeam.Shape.tf);
			Reporting.AddLine("Ffu = Mf / (d - tbf)");
			Reporting.AddLine("= " + CommonDataStatic.SeismicSettings.R_Mf + " / (" + rightBeam.Shape.d + " - " + rightBeam.Shape.tf + ") = " + R_Ffu + ConstUnit.Force);

			if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4)
			{
				// shear yielding
				FiRnY = ConstNum.FIOMEGA1_0N * 0.6 * rightBeam.WinConnect.MomentEndPlate.Material.Fy * rightBeam.WinConnect.MomentEndPlate.Width * rightBeam.WinConnect.MomentEndPlate.Thickness;
				// Shear Rupture
				An = (rightBeam.WinConnect.MomentEndPlate.Width - 2 * (rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ConstNum.EIGHTH_INCH * ConstNum.ONE_INCH)) * rightBeam.WinConnect.MomentEndPlate.Thickness;
				FiRnR = ConstNum.FIOMEGA0_75N * 0.6 * rightBeam.WinConnect.MomentEndPlate.Material.Fu * An;
				FiRn = Math.Min(FiRnY, FiRnR);

				// step 8 End Plate Shear Yielding (Out of Plane)			
				Reporting.AddHeader("End Plate Shear Yielding (Out of Plane):");
				FiRn = ConstNum.FIOMEGA1_0N * 0.6 * rightBeam.WinConnect.MomentEndPlate.Material.Fy * rightBeam.WinConnect.MomentEndPlate.Width * rightBeam.WinConnect.MomentEndPlate.Thickness;
				Reporting.AddLine("FiRn = " + ConstNum.FIOMEGA1_0N + " * 0.6 * Fy * bp * tp");
				Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + rightBeam.WinConnect.MomentEndPlate.Material.Fy + " * " + rightBeam.WinConnect.MomentEndPlate.Width + " * " + rightBeam.WinConnect.MomentEndPlate.Thickness);
				if (FiRn < R_Ffu / 2)
				{
					Reporting.AddLine("= " + FiRn + " << " + R_Ffu + " / 2 = R_Ffu / 2" + ConstUnit.Force);
					Reporting.AddLine("Plate Yield Strength not satisfied, Increase thickness");
				}
				else
					Reporting.AddLine("= " + FiRn + " >= " + R_Ffu + " / 2 = " + (R_Ffu / 2) + ConstUnit.Force + " (OK)");

				Reporting.AddHeader("End Plate Shear Rupture (Out of Plane):");
				An = (rightBeam.WinConnect.MomentEndPlate.Width - 2 * (rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ConstNum.EIGHTH_INCH * ConstNum.ONE_INCH)) * rightBeam.WinConnect.MomentEndPlate.Thickness;
				Reporting.AddLine("An = (bp - 2 * (db + " + ConstNum.EIGHTH_INCH + ")) * tp");
				Reporting.AddLine("= (" + rightBeam.WinConnect.MomentEndPlate.Width + " - 2 * (" + rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + " + " + ConstNum.EIGHTH_INCH + ")) * " + rightBeam.WinConnect.MomentEndPlate.Thickness + "= " + An + ConstUnit.Area);
				FiRn = ConstNum.FIOMEGA0_75N * 0.6 * rightBeam.WinConnect.MomentEndPlate.Material.Fu * An;
				Reporting.AddLine("FiRn= " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu  * An");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.6 * " + rightBeam.WinConnect.MomentEndPlate.Material.Fu + " * " + An);

				if (FiRn < R_Ffu / 2)
				{
					Reporting.AddLine("= " + FiRn + " << " + R_Ffu + " / 2 = " + (R_Ffu / 2) + ConstUnit.Force);
					Reporting.AddLine("Plate Rupture Strength not satisfied");
				}
				else
					Reporting.AddLine("= " + FiRn + " >= " + R_Ffu + " / 2 = " + (R_Ffu / 2) + ConstUnit.Force + " (OK)");

				// Step 10 Stiffener thickness
				Reporting.AddHeader("End Plate Stiffener Minimum Thickness:");
				ts_min = rightBeam.Shape.tw * rightBeam.Material.Fy / rightBeam.WinConnect.MomentEndPlate.Material.Fy;
				ts = ts_min;
				Reporting.AddLine("ts_min = tbw * Fyb / Fys");
				Reporting.AddLine("= " + rightBeam.Shape.tw + " * " + rightBeam.Material.Fy + " / " + rightBeam.WinConnect.MomentEndPlate.Material.Fy);
				Reporting.AddLine("= " + ts_min + ConstUnit.Length);

				// stiffener width thickness ratio:
				Reporting.AddLine("stiffener width/thickness ratio:");
				Limit = 0.56 * Math.Pow(ConstNum.ELASTICITY / rightBeam.WinConnect.MomentEndPlate.Material.Fy, 0.5);

				do
				{
					ratio = CommonDataStatic.SeismicSettings.R_hst / ts;

					if (Limit < ratio)
					{
						loopAgain = true;
						ts = ts + ConstNum.SIXTEENTH_INCH;
					}
					else
						loopAgain = false;
				} while (loopAgain);

				CommonDataStatic.SeismicSettings.R_ts = CommonCalculations.PlateThickness(ts);
				Reporting.AddLine("ts = " + ts + "  >= " + ts_min + ConstUnit.Length + " (OK)");
				
				Reporting.AddHeader("Limiting width/Thickness ratio:");
				Reporting.AddLine("=0.56 * (" + ConstNum.ELASTICITY + " / " + leftBeam.WinConnect.MomentEndPlate.Material.Fy + ") ^ 0.5 = " + Limit);
				Reporting.AddLine("hst/ts = " + CommonDataStatic.SeismicSettings.R_hst + " / " + CommonDataStatic.SeismicSettings.R_ts + " = " + ratio + "  <=  " + Limit + " (OK)");

				// Stiffener welds:
				Reporting.AddHeader("End Plate Stiffener Welds:");
				if (CommonDataStatic.SeismicSettings.R_ts > 0.375 * ConstNum.ONE_INCH)
				{
					Reporting.AddHeader("Stiffener thicknes (ts) = " + ts + "  >>  " + 0.375 * ConstNum.ONE_INCH + ConstUnit.Length);
					Reporting.AddLine("Use CJP Groove welds");

					CommonDataStatic.SeismicSettings.StiffenerToEndPlateWeldIsCJP = true;
				}
				else
				{
					CommonDataStatic.SeismicSettings.StiffenerToEndPlateWeldIsCJP = false;
					Reporting.AddHeader("Stiffener thicknes (ts) = " + CommonDataStatic.SeismicSettings.R_ts + " <= " + 0.375 * ConstNum.ONE_INCH + ConstUnit.Length);
					Reporting.AddLine("Use fillet welds");

					Reporting.AddHeader("Beam Flange to Stiffener Weld:");
					w_req = rightBeam.WinConnect.MomentEndPlate.Material.Fu * CommonDataStatic.SeismicSettings.R_ts / (1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddLine("w_Req = Fu * ts / (1.414 * Fexx)");
					Reporting.AddLine("= " + rightBeam.WinConnect.MomentEndPlate.Material.Fu + " * " + CommonDataStatic.SeismicSettings.R_ts + " /(1.414 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
					Reporting.AddLine("= " + w_req + ConstUnit.Length);
					BeamFlangetoStiffenerWeld = -((int) Math.Floor(-16 * w_req)) / 16.0;

					Reporting.AddHeader("End Plate to Stiffener Weld:");
					w_req = rightBeam.WinConnect.MomentEndPlate.Material.Fu * CommonDataStatic.SeismicSettings.R_ts / (1.273F * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddLine("w_Req = Fu * ts / (1.273 * Fexx)");
					Reporting.AddLine("= " + rightBeam.WinConnect.MomentEndPlate.Material.Fu + " * " + CommonDataStatic.SeismicSettings.R_ts + " /(1.273 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
					Reporting.AddLine("= " + w_req + ConstUnit.Length);
					EndPlatetoStiffenerWeld = -((int) Math.Floor(-16 * w_req)) / 16.0;
				}

				// Step 11 Bolt Shear Rupture Strength
				if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
					NB = 4;
				else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
					NB = 8;

				Reporting.AddHeader("Bolt Shear Rupture Strength:");
				FiRn = NB * rightBeam.WinConnect.MomentEndPlate.Bolt.BoltStrength;
				Reporting.AddLine("= NB * Fv = " + NB + " * " + rightBeam.WinConnect.MomentEndPlate.Bolt.BoltStrength);

				if (FiRn >= CommonDataStatic.SeismicSettings.R_Vu)
					Reporting.AddLine("= " + FiRn + " >= " + CommonDataStatic.SeismicSettings.R_Vu + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + FiRn + " << " + CommonDataStatic.SeismicSettings.R_Vu + ConstUnit.Force + " (NG)");

				// Step 12 Bolt Bearing
				Reporting.AddHeader("Bolt Bearing:");
				if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
				{
					Ni = 2;
					Lc = pfi + rightBeam.Shape.tf + pfo - (rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH);
					Reporting.AddLine("Lc = pfi + tf + Pfo - (db + " + ConstNum.SIXTEENTH_INCH + ")");
					Reporting.AddLine("= " + pfi + " + " + tf + " + " + pfo + " - (" + db + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lc + ConstUnit.Length);

				}
				else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
				{
					Ni = 4;
					Lc = Pb - (rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH);
					Reporting.AddLine("Lc = pb - (db + " + ConstNum.SIXTEENTH_INCH + ")");
					Reporting.AddLine("= " + Pb + " - (" + rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + " + " + ConstNum.SIXTEENTH_INCH + ")= " + Lc + ConstUnit.Length);
				}
				No = Ni;
				Rni = Math.Min(1.2 * Lc * rightBeam.WinConnect.MomentEndPlate.Thickness * rightBeam.WinConnect.MomentEndPlate.Material.Fu, 2.4F * rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize * rightBeam.WinConnect.MomentEndPlate.Thickness * rightBeam.WinConnect.MomentEndPlate.Material.Fu);
				Reporting.AddLine(" Rni = Min(1.2 * Lc * t * Fu, 2.4 * db * t * Fu)");
				Reporting.AddLine("= Min(1.2 * " + Lc + " * " + rightBeam.WinConnect.MomentEndPlate.Thickness + " * " + rightBeam.WinConnect.MomentEndPlate.Material.Fu + " , 2.4 * " + rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + " * " + rightBeam.WinConnect.MomentEndPlate.Thickness + " * " + rightBeam.WinConnect.MomentEndPlate.Material.Fu + ")");
				Reporting.AddLine("= " + Rni + ConstUnit.Force);

				Rno = Math.Min(1.2 * Lc * rightBeam.WinConnect.MomentEndPlate.Thickness * rightBeam.WinConnect.MomentEndPlate.Material.Fu, 2.4F * rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize * rightBeam.WinConnect.MomentEndPlate.Thickness * rightBeam.WinConnect.MomentEndPlate.Material.Fu);
				Reporting.AddLine(" Rno = Min(1.2 * Lc * t * Fu, 2.4 * db * t * Fu)");
				Reporting.AddLine("= Min(1.2 * " + Lc + " * " + rightBeam.WinConnect.MomentEndPlate.Thickness + " * " + rightBeam.WinConnect.MomentEndPlate.Material.Fu + " , 2.4 * " + rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + " * " + rightBeam.WinConnect.MomentEndPlate.Thickness + " * " + rightBeam.WinConnect.MomentEndPlate.Material.Fu + ")");
				Reporting.AddLine("= " + Rno + ConstUnit.Force);

				Reporting.AddHeader("Bearing On End Plate:");
				FiRn = ConstNum.FIOMEGA0_75N * Ni * Rni + ConstNum.FIOMEGA0_75N * No * Rno;
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * Ni * Rni + " + ConstString.FIOMEGA0_75 + " * No * Rno");
				Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * " + Ni + " * " + Rni + " + " + ConstString.FIOMEGA0_75 + " * " + No + " * " + Rno);
				if (FiRn >= CommonDataStatic.SeismicSettings.R_Vu)
					Reporting.AddLine("= " + FiRn + " >= " + CommonDataStatic.SeismicSettings.R_Vu + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + FiRn + " << " + CommonDataStatic.SeismicSettings.R_Vu + ConstUnit.Force + " (OK)");

				Reporting.AddHeader("Bearing On Column Flange");
				FiRn = ConstNum.FIOMEGA0_75N * Ni * Rni + ConstNum.FIOMEGA0_75N * No * Rno;
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * Ni * Rni + " + ConstString.FIOMEGA0_75 + " * No * Rno");
				Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * " + Ni + " * " + Rni + " + " + ConstString.FIOMEGA0_75 + " * " + No + " * " + Rno);
				if (FiRn >= CommonDataStatic.SeismicSettings.R_Vu)
					Reporting.AddLine("= " + FiRn + " >= " + CommonDataStatic.SeismicSettings.R_Vu + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + FiRn + " << " + CommonDataStatic.SeismicSettings.R_Vu + ConstUnit.Force + " (OK)");

				// Step 13 Beam to EndPlate Welds
				Reporting.AddHeader("Beam-to-End Plate Welds");
				Reporting.AddLine("Beam Flange to End Plate Weld is CJP");
				if (CommonDataStatic.Units == EUnit.US)
					Reporting.AddLine("with 5/16 inch fillet weld on inside face of flange.");
				else if (CommonDataStatic.Units == EUnit.Metric)
					Reporting.AddLine("with 8 mm fillet weld on inside face of flange.");

				Reporting.AddHeader("Beam Web-to-End Plate Weld:");
				Reporting.AddLine("Use CJP Weld.");

				SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.PlateThickness, ref MaxValue, ref MinValue);
				SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.EndPlateWidth, ref MaxValue, ref MinValue);

				if (Bp > MaxValue)
					Bp = MaxValue;
				if (Bp < MinValue)
					Bp = MinValue;
                if (!rightBeam.WinConnect.MomentEndPlate.Width_User) 
                    rightBeam.WinConnect.MomentEndPlate.Width = Bp;

				// step 2
				SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltGage, ref MaxValue, ref MinValue);
				Gage = column.GageOnFlange;
				if (Gage > MaxValue)
					Gage = MaxValue;
				if (Gage < MinValue)
					Gage = MinValue;
				g = Gage;

				SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltDistanceFromBeamFlange, ref MaxValue, ref MinValue);

				if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
				{
					SeismicCalc.EndPlatePrequalifiedLimitations(rightBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltVerticalSpacing, ref MaxValue, ref MinValue);
					Pb = -((int) Math.Floor((double) -4 * (2 + 2 / 3) * rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize)) / 4.0;
					if (Pb > MaxValue)
						Pb = MaxValue;
					if (Pb < MinValue)
						Pb = MinValue;
				}
				rightBeam.WinConnect.MomentEndPlate.Bolt.SpacingLongDir = Pb;

				Reporting.AddHeader("Column Side Design for Right Side Beam:");
				// Step 14 Column Flange Flexural Yielding		
				Reporting.AddLine("Column Flange Flexural Yielding");
				bcf = column.Shape.bf;
				Reporting.AddLine("bcf = " + column.Shape.bf + ConstUnit.Length);
				s = 0.5 * Math.Pow(bcf * g, 0.5);
				Reporting.AddLine("s = 0.5 * (bcf * g) ^ 0.5 = 0.5 * (" + bcf + " * " + g + ") ^ 0.5 = " + s + ConstUnit.Length);

				if (CommonDataStatic.ColumnStiffener.SThickness == 0)
				{
					// unstiffened column

					c = pfi + pfo + rightBeam.Shape.tf;
					Reporting.AddLine("c = pfi + Pfo + RBeam.tf = " + pfi + " + " + pfo + " + " + rightBeam.Shape.tf + " = " + c + ConstUnit.Length);
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
					{
						yc = (bcf / 2 * (h1 / s + h0 / s) + 2 / g * (h1 * (s + 3 * c / 4) + h0 * (s + c / 4) + Math.Pow(c, 2) / 2) + g / 2);
						Reporting.AddLine("yc = bcf/2 * (h1/s + h0/s) + 2/g * (h1 * (s + 3 * c/4) + h0 * (s + c/4) + c² /2) + g/2");
						Reporting.AddLine("=  " + bcf + "/2 * ( " + h1 + " / " + s + "  +  " + h0 + "  /  " + s + " ) + 2 /  " + g + "  * ( " + h1 + "  * ( " + s + " + 3 *  " + c + "/4) +  " + h0 + "  * ( " + s + " + " + c + "/4) +  " + c + "^ 2 / 2) +  " + g + "/2");
						Reporting.AddLine("= " + yc + ConstUnit.Length);
					}
					else
					{
						// EPS8
						yc = (bcf / 2 * (h1 / s + h4 / s) + 2 / g * (h1 * (Pb + c / 2 + s) + h2 * (Pb / 2 + c / 4) + h3 * (Pb / 2 + c / 2) + h4 * s) + g / 2);
						Reporting.AddLine("yc = bcf/2 * (h1/s + h4/s) + 2/g * (h1 * (Pb + c/2 + s) + h2 * (Pb/2 + c/4) + h3 * (Pb/2 + c/2) + h4 * s) + g/2");
						Reporting.AddLine("= " + bcf + "/2 * (" + h1 + " / " + s + " + " + h4 + " / " + s + ") + 2 / " + g + " * (" + h1 + " * (" + Pb + " + " + c + "/2 + " + s + ") + " + h2 + " * (" + Pb + "/2 + " + c + "/4) + " + h3 + " * (" + Pb + "/2 +  " + c + "/2) + " + h4 + " * " + s + ") +  " + g + "/2");
						Reporting.AddLine("= " + yc + ConstUnit.Length);
					}
					Yc_Unstiffened = yc;
				}
				else
				{
					// stiffened column
					c = pfi + pfo + CommonDataStatic.ColumnStiffener.SThickness;
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
					{
						c = pfi + pfo + CommonDataStatic.ColumnStiffener.SThickness;
						Reporting.AddLine("c = pfi + Pfo + Col_Stiff_t = " + pfi + " + " + pfo + " + " + CommonDataStatic.ColumnStiffener.SThickness + " = " + c + ConstUnit.Length);
						yc = bcf / 2 * (h1 * (1 / s + 1.0 / psi) + h0 * (1 / s + 1.0 / pso)) + 2 / g * (h1 * (s + psi) + h0 * (s + pso));
						Reporting.AddLine("yc = bcf/2 * (h1 *(1/s + 1/psi) + h0 * (1/s + 1/pso)) + 2/g * (h1 * (s + psi) + h0 * (s + pso))");
						Reporting.AddLine("= " + bcf + "/2 * (" + h1 + " * (1/" + s + " + 1/" + psi + ") + " + h0 + " * (1/" + s + " + 1/" + pso + ")) + 2/" + g + " * (" + h1 + " * (" + s + " + " + psi + ") + " + h0 + " * (" + s + " + " + pso + "))");
						Reporting.AddLine("= " + yc + ConstUnit.Length);
					}
					else
					{
						// EPS8
						yc = bcf / 2 * (h1 / s + h2 / pso + h3 / psi + h4 / s) + 2 / g * (h1 * (s + Pb / 4) + h2 * (pso + 3 * Pb / 4) + h3 * (psi + Pb / 4) + h4 * (s + 3 * Pb / 4) + Math.Pow(Pb, 2)) + g;
						Reporting.AddLine("yc = bcf/2 * ((h1/s) + h2/pso + h3/psi + h4/s) + 2/g * (h1 * (s + Pb/4) + h2 * (pso + 3 * Pb/4) + h3 * (psi + Pb/4) + h4 * (s + 3 * Pb/4) + Pb²) + g");
						Reporting.AddLine("= " + bcf + "/2 * ((" + h1 + " / " + s + ") + " + h2 + " / " + pso + " + " + h3 + " / " + psi + " + " + h4 + " / " + s + ") + 2/" + g + " * (" + h1 + " * (" + s + " + " + Pb + "/4) + " + h2 + " * (" + pso + " + 3 * " + Pb + "/4) + " + h3 + " * (" + psi + " + " + Pb + "/4) + " + h4 + " * (" + s + " + 3 * " + Pb + "/4) + " + Pb + " ^ 2) + " + g);
						Reporting.AddLine("= " + yc + ConstUnit.Length);
					}
				}

				Reporting.AddLine("Required Column tf");
				tcf_req = Math.Pow(1.11 * CommonDataStatic.SeismicSettings.R_Mf / (ConstNum.FIOMEGA0_9N * column.Material.Fy * yc), 0.5);
				Reporting.AddLine("tcf_req = (1.11 * Mf / (" + ConstString.FIOMEGA0_9 + " * Fy * yc)) ^ 0.5");
				Reporting.AddLine("= (1.11 * " + CommonDataStatic.SeismicSettings.R_Mf + " / (" + ConstString.FIOMEGA0_9 + " * " + column.Material.Fy + " * " + yc + ")) ^ 0.5");

				if (tcf_req <= column.Shape.tf)
				{
					Reporting.AddLine("= " + tcf_req + " <= Column tf = " + column.Shape.tf + ConstUnit.Length + " (OK)");
					Reporting.AddLine("Stiffeners are not required for Col. Flange Flextural Yielding.");
				}
				else
				{
					Reporting.AddLine("= " + tcf_req + " >> Column tf = " + column.Shape.tf + ConstUnit.Length);
					Reporting.AddLine("Stiffeners are required for Col. Flange Flextural Yielding.");
				}

				if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
					extension = pfo + de;
				else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
					extension = pfo + Pb + de;
                if (!rightBeam.WinConnect.MomentEndPlate.Length_User) 
                    rightBeam.WinConnect.MomentEndPlate.Length = rightBeam.Shape.d + 2 * extension;

				// Step 15 Required Stiffener Force
				Reporting.AddHeader("Required Stiffener Force:");
				Reporting.AddLine("Column Flange Flexural Design Strength:");
				FidMcf = (ConstNum.FIOMEGA0_9N * column.Material.Fy * Yc_Unstiffened * Math.Pow(column.Shape.tf, 2));
				Reporting.AddLine("FidMcf = " + ConstString.FIOMEGA0_9 + " * Fy * Yc * Column.tf²");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + column.Material.Fy + " * " + Yc_Unstiffened + " * " + column.Shape.tf + "² = " + FidMcf + ConstUnit.MomentUnitInch);

				Reporting.AddHeader("Column Flange Design Force (FidRn)");
				FidRn = FidMcf / (rightBeam.Shape.d - rightBeam.Shape.tf);
				Reporting.AddLine("FidRn = FidMcf / (d - tf)= " + FidMcf + " / (" + rightBeam.Shape.d + " - " + rightBeam.Shape.tf + ") = " + FidRn + ConstUnit.Force);
				FidRn15 = FidRn;

				// Step 16 Column Web Yielding
				Reporting.AddHeader("Column Web Local Yielding Strength:");
				ct = CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn ? 1 : 0.5;

				Reporting.AddLine("Ct = " + ct);
				Rn = ct * (6 * column.Shape.kdes + rightBeam.Shape.tf + 2 * rightBeam.WinConnect.MomentEndPlate.Thickness) * column.Material.Fy * column.Shape.tw;
				Reporting.AddLine(" Rn = ct * (6 * kc + tbf + 2 * tp) * Fyc * tcw");
				Reporting.AddLine("= ct * (6 * " + column.Shape.kdes + " + " + rightBeam.Shape.tf + " + 2 * " + rightBeam.WinConnect.MomentEndPlate.Thickness + ") * " + column.Material.Fy + " * " + column.Shape.tw);
				Reporting.AddLine("= " + Rn + ConstUnit.Force);
				FidRn = ConstNum.FIOMEGA1_0N * Rn;
				Reporting.AddLine("FidRn = " + ConstNum.FIOMEGA1_0N + " * Rn = " + ConstNum.FIOMEGA1_0N + " * " + Rn + " = " + FidRn + ConstUnit.Force);

				if (FidRn >= R_Ffu)
				{
					Reporting.AddLine(FidRn + " >= " + R_Ffu + ConstUnit.Force);
					Reporting.AddLine("ContinuityPlates Not Required for Column Web Yielding");
				}
				else
				{
					Reporting.AddLine(FidRn + " << " + R_Ffu + ConstUnit.Force);
					Reporting.AddLine("ContinuityPlates are Required for Column Web Yielding");
				}
				FidRn16 = FidRn;

				// Step 17 ColumnWebBuckling
				Reporting.AddHeader("Column Web Buckling");
				if (CommonDataStatic.ColumnStiffener.TopOfBeamToColumn >= column.Shape.d / 2)
				{
					FiRn = ConstNum.FIOMEGA0_75N * (24 * Math.Pow(column.Shape.tw, 3) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy, 0.5)) / column.Shape.t;
					Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * (24 * tcw ^ 3 * (E * Fyc) ^ 0.5) / h");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * (24 * " + column.Shape.tw + " ^ 3 * (" + ConstNum.ELASTICITY + " * " + column.Material.Fy + ") ^ 0.5) / " + column.Shape.t);
					Reporting.AddLine("= " + FiRn + ConstUnit.Force);
				}
				else
				{
					FiRn = ConstNum.FIOMEGA0_75N * 12 * Math.Pow(column.Shape.tw, 3) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy, 0.5) / column.Shape.t;
					Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * (12 * tcw ^ 3 * (E * Fyc) ^ 0.5) / h");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * (12 * " + column.Shape.tw + " ^ 3 * (" + ConstNum.ELASTICITY + " * " + column.Material.Fy + ") ^ 0.5) / " + column.Shape.t);
					Reporting.AddLine("= " + FiRn + ConstUnit.Force);
				}

				if (FiRn >= R_Ffu)
				{
					Reporting.AddLine(" " + FiRn + " >= " + R_Ffu + ConstUnit.Force);
					Reporting.AddLine("Continuity Plates Not Required for Column Web Buckling.");
				}
				else
				{
					Reporting.AddLine(" " + FiRn + " << " + R_Ffu + ConstUnit.Force);
					Reporting.AddLine("Continuity Plates Required for Column Web Buckling.");
				}
				FinRn17 = FiRn;

				// Step 18 Column Web Crippling
				Reporting.AddHeader("Column Web Crippling");
				n = rightBeam.Shape.tf;
				if (CommonDataStatic.ColumnStiffener.TopOfBeamToColumn >= column.Shape.d / 2)
				{
					FiRn = ConstNum.FIOMEGA0_75N * 0.8 * Math.Pow(column.Shape.tw, 2) * (1 + 3 * (n / column.Shape.d) * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);
					Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * 0.8 * tcw² * (1 + 3 * (N/dc) * (tcw/tcf)^1.5) * (E * Fyc * tcf/tcw)^0.5");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.8 * " + column.Shape.tw + "² * (1 + 3 * (" + n + " / " + column.Shape.d + " ) * (" + column.Shape.tw + " / " + column.Shape.tf + ")^1.5) * (" + ConstNum.ELASTICITY + " * " + column.Material.Fy + " * " + column.Shape.tf + " / " + column.Shape.tw + ")^0.5");
				}
				else
				{
					if (n / column.Shape.d < 0.2)
					{
						FiRn = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(column.Shape.tw, 2) * (1 + 3 * (n / column.Shape.d) * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);
						Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * 0.4 * tcw² * (1 + 3 * (N/dc) * (tcw/tcf)^1.5) * (E * Fyc * tcf/tcw)^0.5");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.8 * " + column.Shape.tw + "² * (1 + 3 * (" + n + " / " + column.Shape.d + " ) * (" + column.Shape.tw + " / " + column.Shape.tf + ")^1.5) * (" + ConstNum.ELASTICITY + " * " + column.Material.Fy + " * " + column.Shape.tf + " / " + column.Shape.tw + ")^0.5");
					}
					else
					{
						FiRn = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(column.Shape.tw, 2) * (1 + (4 * n / column.Shape.d - 0.2) * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);
						Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * 0.4 * tcw² * (1 +(4*N/dc-0.2) * (tcw/tcf)^1.5) * (E * Fyc * tcf/tcw)^0.5");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + column.Shape.tw + "² * (1 + (4*" + n + " / " + column.Shape.d + "-0.2) * (" + column.Shape.tw + " / " + column.Shape.tf + ")^1.5) * (" + ConstNum.ELASTICITY + " * " + column.Material.Fy + " * " + column.Shape.tf + " / " + column.Shape.tw + ")^0.5");
					}
				}

				if (FiRn >= R_Ffu)
				{
					Reporting.AddLine("= " + FiRn + "  >= " + R_Ffu + "  " + ConstUnit.Force + " (OK)");
					Reporting.AddLine("Continuity Plates Not Required for column web crippling");
				}
				else
				{
					Reporting.AddLine("= " + FiRn + "  << " + R_Ffu + "  " + ConstUnit.Force + " (NG)");
					Reporting.AddLine("ContinuityPlates Required for column web crippling");
				}
				FinRn18 = FiRn;

				// Step 19 Stiffener Required Strength
				Reporting.AddHeader("Stiffener Required Strength");
				minFsu = Math.Min(Math.Min(FidRn15, FidRn16), Math.Min(FinRn17, FinRn18));
				R_Fsu = R_Ffu - Math.Min(Math.Min(FidRn15, FidRn16), Math.Min(FinRn17, FinRn18));
				Reporting.AddLine("Fsu = Ffu-Min(" + FidRn15 + ", " + FidRn16 + ", " + FinRn17 + ", " + FinRn18 + ")");
				Reporting.AddLine("= " + R_Ffu + "- " + minFsu + " = " + R_Fsu + ConstUnit.Force);

				// Step 20 Panel Zone per Provisions Section 9.3 for SMF and 10.3 for IMF
				rightBeam.MomentConnection = EMomentCarriedBy.EndPlate;
				if (!rightBeam.EndSetback_User)
					rightBeam.EndSetback = rightBeam.WinConnect.MomentEndPlate.Thickness;
                if (!rightBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt_User) 
                    rightBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt = pfi;
				if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
                    if (!rightBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts_User) 
                        rightBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts = 8;
				else
                    if (!rightBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts_User) 
                        rightBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts = 4;
                if (!rightBeam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir_User) 
                    rightBeam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir = Pb;
				rightBeam.WinConnect.MomentEndPlate.BraceStiffener.Length = CommonDataStatic.SeismicSettings.R_lst;

				rightBeam.WinConnect.MomentEndPlate.BraceStiffener.Thickness = CommonDataStatic.SeismicSettings.R_ts;
				rightBeam.Moment = CommonDataStatic.SeismicSettings.R_Mf;
				rightBeam.ShearForce = CommonDataStatic.SeismicSettings.R_Vu;
			}
		}

		public static void SeismicEndPlateReport_Left()
		{
			double L_Fsu = 0;
			double FinRn18 = 0;
			double minFsu = 0;
			double n = 0;
			double FinRn17 = 0;
			double FidRn16 = 0;
			double Rn = 0;
			double ct = 0;
			double FidRn15 = 0;
			double FidRn = 0;
			double FidMcf = 0;
			double extension = 0;
			double tcf_req = 0;
			int pso = 0;
			int psi = 0;
			double Yc_Unstiffened = 0;
			double yc = 0;
			double c = 0;
			double bcf = 0;
			double Rno = 0;
			double Rni = 0;
			double No = 0;
			double db = 0;
			double Lc = 0;
			double Ni = 0;
			int NB = 0;
			double w_req = 0;
			double Limit = 0;
			double ts = 0;
			double ts_min = 0;
			bool loopAgain = false;
			double FiRn = 0;
			double FiRnR = 0;
			double An = 0;
			double FiRnY = 0;
			double L_Ffu = 0;
			double tp_Req = 0;
			double yp = 0;
			double s = 0;
			double de = 0;
			double h4 = 0;
			double h3 = 0;
			double h2 = 0;
			double dboltRequired = 0;
			double h1 = 0;
			double h0 = 0;
			double Fnt = 0;
			double g = 0;
			double Gage = 0;
			double Bp = 0;
			double L_Mpe = 0;
			double L_Cpr = 0;
			double L_Vg = 0;
			double b1 = 0;
			double a1 = 0;
			double P = 0;
			double Pb = 0;
			double pfo = 0;
			double pfi = 0;
			double LambdaPW = 0;
			double ratio = 0;
			double LambdaPF = 0;
			double LSpanToDepthRatio = 0;
			double MinValue = 0;
			double MaxValue = 0;

			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

			if (!leftBeam.IsActive)
				return;

			Reporting.AddHeader("Left Side Beam Limitations:");
			SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BeamDepth, ref MaxValue, ref MinValue);

			// (3)none
			// (4) Flange Thickness
			SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BeamFlangeThickness, ref MaxValue, ref MinValue);

			// (5)
			Reporting.AddLine("Beam Span-to-Depth Ratio:");
			if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.SMF)
			{
				LSpanToDepthRatio = CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft / leftBeam.Shape.d;
				Reporting.AddLine("L/d = " + CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft + " / " + leftBeam.Shape.d);

				if (LSpanToDepthRatio >= 7)
					Reporting.AddLine("= " + LSpanToDepthRatio + " >= 7 (OK)");
				else
					Reporting.AddLine("= " + LSpanToDepthRatio + " << 7 (NG)");
			}
			else if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.IMF)
			{
				LSpanToDepthRatio = CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft / leftBeam.Shape.d;
				Reporting.AddLine("L/d = " + CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft + " / " + leftBeam.Shape.d);

				if (LSpanToDepthRatio >= 5)
					Reporting.AddLine("= " + LSpanToDepthRatio + " >= 5 (OK)");
				else
					Reporting.AddLine("= " + LSpanToDepthRatio + " << 5 (NG)");
			}

			// (6)slenderness check:
			Reporting.AddHeader("Beam Flange Slenderness:");
			LambdaPF = 0.38 * Math.Pow(ConstNum.ELASTICITY / leftBeam.Material.Fy, 0.5);
			Reporting.AddLine("Limiting b/t = 0.38 * (E/Fy)^0.5 = 0.38 * (" + ConstNum.ELASTICITY + " / " + leftBeam.Material.Fy + ")^0.5 = " + LambdaPF);
			ratio = leftBeam.Shape.bf / leftBeam.Shape.tf / 2;
			if (ratio <= LambdaPF)
				Reporting.AddLine("Beam Flange b/t = " + (leftBeam.Shape.bf / 2) + " / " + leftBeam.Shape.tf + " = " + ratio + " <= " + LambdaPF + " (OK)");
			else
				Reporting.AddLine("Beam Flange b/t = " + (leftBeam.Shape.bf / 2) + " / " + leftBeam.Shape.tf + " = " + ratio + " >> " + LambdaPF + " (NG)");

			Reporting.AddHeader("Beam Web Slenderness:");
			LambdaPW = 3.76 * Math.Pow(ConstNum.ELASTICITY / leftBeam.Material.Fy, 0.5);
			Reporting.AddLine("Limiting h/tw = 3.76 * (E/Fy)^0.5 = 3.76 * (" + ConstNum.ELASTICITY + " / " + leftBeam.Material.Fy + ")^0.5 = " + LambdaPW);
			ratio = leftBeam.Shape.t / leftBeam.Shape.tw;
			if (ratio <= LambdaPW)
				Reporting.AddLine("Beam Web h/t = " + leftBeam.Shape.t + " / " + leftBeam.Shape.tw + " = " + ratio + " <= " + LambdaPW + " (OK)");
			else
				Reporting.AddLine("Beam Web h/t = " + leftBeam.Shape.t + " / " + leftBeam.Shape.tw + " = " + ratio + " >> " + LambdaPW + " (NG)");

			SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltDistanceFromBeamFlange, ref MaxValue, ref MinValue);

			pfi = ConstNum.TWO_INCHES;
			if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
			{
				if (pfi > MaxValue)
					pfi = MaxValue;
				if (pfi < MinValue)
					pfi = MinValue;
			}
			pfo = pfi;

			// 6.10 Design Procedure
			//  Step 1
			if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
			{
				SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltVerticalSpacing, ref MaxValue, ref MinValue);
				Pb = -((int) Math.Floor((double) -4 * (2 + 2 / 3) * leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize)) / 4.00;
				if (Pb > MaxValue)
					Pb = MaxValue;
				if (Pb < MinValue)
					Pb = MinValue;
			}
			leftBeam.WinConnect.MomentEndPlate.Bolt.SpacingLongDir = Pb;

			Reporting.AddHeader("Left Side Beam End Plate Design:");
			if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4)
			{
				Reporting.AddHeader("Distance from column face to Plastic Hinge (Sh):");
				Reporting.AddLine("sh = Min(d / 2, 3 * BF)= Min(" + leftBeam.Shape.d + "/2,3*" + leftBeam.Shape.bf + ")= " + CommonDataStatic.SeismicSettings.L_sh + ConstUnit.Length);
			}
			else
			{
				// EPS4, EPS8
				if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
				{
					Reporting.AddHeader("End Plate Stiffener Dimensions, Hst and Lst:");
					CommonDataStatic.SeismicSettings.L_hst = pfo + leftBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir;
					Reporting.AddLine("Hst = pfo + e = " + pfo + " + " + leftBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir + " = " + CommonDataStatic.SeismicSettings.L_hst + ConstUnit.Length);
				}
				else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
				{
					CommonDataStatic.SeismicSettings.L_hst = pfo + leftBeam.WinConnect.MomentEndPlate.Bolt.SpacingLongDir + leftBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir;
					Reporting.AddHeader("End Plate Stiffener Dimensions, Hst and Lst:");
					Reporting.AddLine("Hst = pfo + pb + e = " + pfo + " + " + leftBeam.WinConnect.MomentEndPlate.Bolt.SpacingLongDir + " + " + leftBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir + " = " + CommonDataStatic.SeismicSettings.L_hst + ConstUnit.Length);
				}
				if (CommonDataStatic.SeismicSettings.ConnectionType != ESeismicConnectionType.E4)
				{
					Reporting.AddHeader("Lst = (Hst- " + ConstNum.ONE_INCH + ") / 0.57735 +" + ConstNum.ONE_INCH + " = " + (CommonDataStatic.SeismicSettings.L_hst - ConstNum.ONE_INCH) + " / 0.57735 + " + ConstNum.ONE_INCH + " = " + CommonDataStatic.SeismicSettings.L_lst + ConstUnit.Length);
					CommonDataStatic.SeismicSettings.L_sh = leftBeam.WinConnect.MomentEndPlate.Thickness + CommonDataStatic.SeismicSettings.L_lst;
					Reporting.AddLine("sh = tp + Lst = " + leftBeam.WinConnect.MomentEndPlate.Thickness + " + " + CommonDataStatic.SeismicSettings.L_lst + " = " + CommonDataStatic.SeismicSettings.L_sh + ConstUnit.Length);
				}
			}

			Reporting.AddHeader("Resultant Gravity Load:");
			CommonDataStatic.SeismicSettings.GravityLoadLeft = CommonDataStatic.SeismicSettings.GravityLoadLeft;
			P = CommonDataStatic.SeismicSettings.GravityLoadLeft;
			a1 = CommonDataStatic.SeismicSettings.DistanceToGravityLoadLeft;
			Reporting.AddLine("Vgr = " + CommonDataStatic.SeismicSettings.GravityLoadLeft + ConstUnit.Force);
			Reporting.AddLine("Distance from Column Face to Vgr, a1");
			Reporting.AddLine("a1 = " + a1 + ConstUnit.Length);
			b1 = CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft - a1;
			Reporting.AddLine("Distance from Far_End_Column Face to Vgr, b1");
			Reporting.AddLine("b1 = " + b1 + ConstUnit.Length);

			L_Vg = Math.Max(P * b1 / CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft, P * a1 / CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft);
			Reporting.AddHeader("Beam Gravity End_Force");
			Reporting.AddLine("Vg = max((Vgr * b1) / L, (Vgr * a1) / L)");
			Reporting.AddLine("= max((" + CommonDataStatic.SeismicSettings.GravityLoadLeft + " * " + b1 + ") /" + CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft + " , (" + CommonDataStatic.SeismicSettings.GravityLoadLeft + " * " + a1 + ") /" + CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft + ")");
			Reporting.AddLine("= " + L_Vg + ConstUnit.Force);

			leftBeam.WinConnect.Fema.Ry = leftBeam.Material.Ry;
			L_Cpr = (leftBeam.Material.Fy + leftBeam.Material.Fu) / (2 * leftBeam.Material.Fy);
			Reporting.AddLine("Cpr = (Fy + Fu) / (2 * Fy)");
			Reporting.AddLine("= (" + leftBeam.Material.Fy + " + " + leftBeam.Material.Fu + ") / (2 * " + leftBeam.Material.Fy + ")");
			Reporting.AddLine("= " + L_Cpr);
			
			Reporting.AddHeader("Probable maximum Moment at hinge:");
			L_Mpe = L_Cpr * leftBeam.WinConnect.Fema.Ry * leftBeam.Material.Fy * leftBeam.Shape.zx; // probable moment at hinge
			Reporting.AddLine("Mpe = Cpr * Ry * Fy * Z"); // probable moment at hinge"
			Reporting.AddLine("= " + L_Cpr + " * " + leftBeam.WinConnect.Fema.Ry + " * " + leftBeam.Material.Fy + " * " + leftBeam.Shape.zx);
			Reporting.AddLine("= " + L_Mpe + ConstUnit.MomentUnitInch);

			Reporting.AddHeader("Shear Force at Beam end:");
			CommonDataStatic.SeismicSettings.L_Vu = 2 * L_Mpe / (CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft - 2 * CommonDataStatic.SeismicSettings.L_sh) + L_Vg;
			Reporting.AddLine("Vu = 2 * Mpe / (L - 2 * sh) + Vg");
			Reporting.AddLine("= 2 * " + L_Mpe + "/ (" + CommonDataStatic.SeismicSettings.ClearSpanOfBeamLeft + " - 2 * " + CommonDataStatic.SeismicSettings.L_sh + ") + " + L_Vg);
			Reporting.AddLine("= " + CommonDataStatic.SeismicSettings.L_Vu + ConstUnit.Force);

			Reporting.AddHeader("Moment at column face (Mf):");
			CommonDataStatic.SeismicSettings.L_Mf = L_Mpe + CommonDataStatic.SeismicSettings.L_Vu * CommonDataStatic.SeismicSettings.L_sh;
			Reporting.AddLine("Mf = Mpe + Vu * sh");
			Reporting.AddLine("= " + L_Mpe + " + " + CommonDataStatic.SeismicSettings.L_Vu + " * " + CommonDataStatic.SeismicSettings.L_sh);
			Reporting.AddLine("= " + CommonDataStatic.SeismicSettings.L_Mf + ConstUnit.MomentUnitInch);

			Bp = leftBeam.Shape.bf + ConstNum.ONE_INCH;
            if (!leftBeam.WinConnect.MomentEndPlate.Width_User) 
                leftBeam.WinConnect.MomentEndPlate.Width = Bp;

			SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.EndPlateWidth, ref MaxValue, ref MinValue);

			if (Bp > MaxValue)
				Bp = MaxValue;
			if (Bp < MinValue)
				Bp = MinValue;
            if (!leftBeam.WinConnect.MomentEndPlate.Width_User) 
                leftBeam.WinConnect.MomentEndPlate.Width = Bp;

			// step 2
			SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltGage, ref MaxValue, ref MinValue);

			Gage = column.GageOnFlange;

			if (Gage > MaxValue)
				Gage = MaxValue;
			if (Gage < MinValue)
				Gage = MinValue;
			g = Gage;

			if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
			{
				SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltVerticalSpacing, ref MaxValue, ref MinValue);
				Pb = -((int) Math.Floor((double) -4 * (2 + 2 / 3) * leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize)) / 4.0;
				if (Pb > MaxValue)
					Pb = MaxValue;
				if (Pb < MinValue)
					Pb = MinValue;
			}
			leftBeam.WinConnect.MomentEndPlate.Bolt.SpacingLongDir = Pb;

			// Step 3 ' Bolt diameter
			if (CommonDataStatic.Units == EUnit.US)
			{
				switch (leftBeam.WinConnect.MomentEndPlate.Bolt.ASTMType)
				{
					case EBoltASTM.A325:
						Fnt = 90; // ksi
						break;
					case EBoltASTM.A490:
						Fnt = 113; // ksi
						break;
				}
			}
			else if (CommonDataStatic.Units == EUnit.Metric)
			{
				switch (leftBeam.WinConnect.MomentEndPlate.Bolt.ASTMType)
				{
					case EBoltASTM.A325:
						Fnt = 620; // N/mm²
						break;
					case EBoltASTM.A490:
						Fnt = 780; // N/mm²
						break;
				}
			}

			Reporting.AddHeader("Required Bolt Diameter, db_req:");
			Reporting.AddLine("Fnt = " + Fnt + ConstUnit.Stress);
			if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4)
			{
				h0 = leftBeam.Shape.d - leftBeam.Shape.tf / 2 + pfo;
				h1 = h0 - leftBeam.Shape.tf - pfi - pfo;
				dboltRequired = Math.Pow(2 * CommonDataStatic.SeismicSettings.L_Mf / (Math.PI * ConstNum.FIOMEGA0_75N * Fnt * (h0 + h1)), 0.5);
				Reporting.AddLine("h0= " + leftBeam.Shape.d + " - " + leftBeam.Shape.tf + " / 2 + " + pfo + " = " + h0 + ConstUnit.Length);
				Reporting.AddLine("h1= " + h0 + " - " + leftBeam.Shape.tf + " - " + pfi + " = " + h1 + ConstUnit.Length);
				Reporting.AddLine("db_req = (2 * Mf / (pi * " + ConstString.FIOMEGA0_75 + " * Fnt * (h0 + h1))) ^ 0.5");
				Reporting.AddLine("=(2 * " + CommonDataStatic.SeismicSettings.L_Mf + " / (" + Math.PI + " * " + ConstString.FIOMEGA0_75 + " * " + Fnt + " * (" + h0 + " + " + h1 + "))) ^ 0.5");
				Reporting.AddLine("= " + dboltRequired + ConstUnit.Length);

			}
			else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
			{
				h1 = leftBeam.Shape.d - leftBeam.Shape.tf / 2 + pfo + Pb;
				h2 = h1 - Pb;
				h3 = h2 - pfo - leftBeam.Shape.tf - pfi;
				h4 = h3 - Pb;
				dboltRequired = Math.Pow(2 * CommonDataStatic.SeismicSettings.L_Mf / (Math.PI * ConstNum.FIOMEGA0_75N * Fnt * (h1 + h2 + h3 + h4)), 0.5);

				Reporting.AddLine("h1 = " + leftBeam.Shape.d + " - " + leftBeam.Shape.tf + " / 2 + " + pfo + " + " + Pb + " = " + h1 + ConstUnit.Length);
				Reporting.AddLine("h2 = " + h1 + " - " + Pb + " = " + h2 + ConstUnit.Length);
				Reporting.AddLine("h3 = " + h2 + " - " + pfo + " - " + leftBeam.Shape.tf + " - " + pfi + " = " + h3 + ConstUnit.Length);
				Reporting.AddLine("h4 = " + h3 + " - " + Pb + " = " + h4 + ConstUnit.Length);
				Reporting.AddLine("db_req = (2 * Mf / (pi * " + ConstString.FIOMEGA0_75 + " * Fnt * (h1 + h2 + h3 + h4))) ^ 0.5");
				Reporting.AddLine("= (2 * " + CommonDataStatic.SeismicSettings.L_Mf + " / (" + Math.PI + " * " + ConstString.FIOMEGA0_75 + " * " + Fnt + " * (" + h1 + " + " + h2 + " + " + h3 + " + " + h4 + "))) ^ 0.5");
				Reporting.AddLine("= " + dboltRequired + ConstUnit.Length);
			}
			dboltRequired = -((int) Math.Floor(-8 * dboltRequired)) / 8.0;
			Reporting.AddLine("Use " + dboltRequired + ConstUnit.Length + "Bolts");
			leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize = dboltRequired;
			de = leftBeam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir;

			// Step 4  ???? What purpose
			// Step 5 End Plate thickness		
			Reporting.AddLine("End Plate Thickness:");
			s = 0.5 * Math.Pow(Bp * g, 0.5);
			Reporting.AddLine("s = 0.5 * (bp * g) ^ 0.5");
			Reporting.AddLine("= 0.5 * (" + Bp + " * " + g + ") ^ 0.5 = " + s + ConstUnit.Length);

			if (pfi > s)
			{
				pfi = s;
				Reporting.AddLine("pfi = " + s + ConstUnit.Length);
				Reporting.AddLine("pfo = " + s + ConstUnit.Length);
			}
			pfo = pfi;
			if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4)
			{
				yp = Bp / 2 * (h1 * (1 / pfi + 1 / s) + h0 * (1 / pfo) - 0.5) + 2 / g * (h1 * (pfi + s));
				Reporting.AddLine("yp= Bp / 2 * (h1 * (1 / pfi + 1 / s) + h0 * (1 / Pfo) - 0.5) + 2 / g * (h1 * (pfi + s))");
				Reporting.AddLine("= " + Bp + " / 2 * (" + h1 + " * (1 / " + pfi + " + 1 / " + s + ") + " + h0 + " * (1 / " + pfo + ") - 0.5) + 2 / " + g + " * (" + h1 + " * (" + pfi + " + " + s + "))");
				Reporting.AddLine("= " + yp + ConstUnit.Length);
			}
			else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
			{
				if (de <= s)
				{
					yp = Bp / 2 * (h1 * (1 / pfi + 1 / s) + h0 * (1 / pfo + 1 / (2 * s))) + 2 / g * (h1 * (pfi + s) + h0 * (de + pfo));
					Reporting.AddLine("yp = " + Bp + " / 2 * (" + h1 + " * (1 / " + pfi + " + 1 / " + s + ") + " + h0 + " * (1 / " + pfo + " + 1 / (2 * " + s + ")) + 2 / " + g + " * (" + h1 + " * (" + pfi + " + " + s + ") + " + h0 + " * (" + de + " + " + pfo + "))");
					Reporting.AddLine("= " + yp + ConstUnit.Length);

				}
				else
				{
					yp = Bp / 2 * (h1 * (1 / pfi + 1 / s) + h0 * (1 / pfo + 1 / s)) + 2 / g * (h1 * (pfi + s) + h0 * (s + pfo));
					Reporting.AddLine("yp = " + Bp + " / 2 * (" + h1 + " * (1 / " + pfi + " + 1 / " + s + ") + " + h0 + " * (1 / " + pfo + " + 1 / " + s + ")) + 2 / " + g + " * (" + h1 + " * (" + pfi + " + " + s + ") + " + h0 + " * (" + s + " + " + pfo + "))");
					Reporting.AddLine("= " + yp + ConstUnit.Length);
				}

			}
			else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
			{
				if (de <= s)
				{
					yp = Bp / 2 * (h1 * (1 / (2 * de)) + h2 * (1 / pfo) + h3 * (1 / pfi) + h4 / s) + 2 / g * (h1 * (de + Pb / 4) + h2 * (pfo + 3 * Pb / 4) + h3 * (pfi + Pb / 4) + h4 * (s + 3 * Pb / 4) + Math.Pow(Pb, 2)) + g;
					Reporting.AddLine("yp = Bp / 2 * (h1 * (1 / (2 * de)) + h2 * (1 / Pfo) + h3 * (1 / pfi) + h4 / s)");
					Reporting.AddLine("+ 2 / g * (h1 * (de + pb / 4) + h2 * (Pfo + 3 * pb / 4) + h3 * (pfi + pb / 4) + h4 * (s + 3 * pb / 4) + pb ^ 2) + g");

					Reporting.AddLine("yp = " + Bp + " / 2 * (" + h1 + " * (1 / (2 * " + de + ")) + " + h2 + " * (1 / " + pfo + ") + " + h3 + " * (1 / " + pfi + ") + " + h4 + " / " + s + ") _");
					Reporting.AddLine("+ 2 / " + g + " * (" + h1 + " * (" + de + " + " + Pb + " / 4) + " + h2 + " * (" + pfo + " + 3 * " + Pb + " / 4) + " + h3 + " * (" + pfi + " + " + Pb + " / 4) + " + h4 + " * (" + s + " + 3 * " + Pb + " / 4) + " + Pb + " ^ 2) + " + g + "");
					Reporting.AddLine("= " + yp + ConstUnit.Length);
				}
				else
				{
					yp = Bp / 2 * (h1 * (1 / s) + h2 * (1 / pfo) + h3 * (1 / pfi) + h4 / s) + 2 / g * (h1 * (de + Pb / 4) + h2 * (pfo + 3 * Pb / 4) + h3 * (pfi + Pb / 4) + h4 * (s + 3 * Pb / 4) + Math.Pow(Pb, 2)) + g;

					Reporting.AddLine("yp = Bp / 2 * (h1 * (1 / s) + h2 * (1 / Pfo) + h3 * (1 / pfi) + h4 / s)");
					Reporting.AddLine("+ 2 / g * (h1 * (de + pb / 4) + h2 * (Pfo + 3 * pb / 4) + h3 * (pfi + pb / 4) + h4 * (s + 3 * pb / 4) + pb ^ 2) + g");

					Reporting.AddLine("yp = " + Bp + " / 2 * (" + h1 + " * (1 / " + s + ") + " + h2 + " * (1 / " + pfo + ") + " + h3 + " * (1 / " + pfi + ") + " + h4 + " / " + s + ")");
					Reporting.AddLine("+ 2 / " + g + " * (" + h1 + " * (" + de + " + " + Pb + " / 4) + " + h2 + " * (" + pfo + " + 3 * " + Pb + " / 4) + " + h3 + " * (" + pfi + " + " + Pb + " / 4) + " + h4 + " * (" + s + " + 3 * " + Pb + " / 4) + " + Pb + " ^ 2) + " + g);
					Reporting.AddLine("= " + yp + ConstUnit.Length);


				}
			}

			tp_Req = Math.Pow(1.11 * CommonDataStatic.SeismicSettings.L_Mf / (ConstNum.FIOMEGA0_9N * leftBeam.WinConnect.MomentEndPlate.Material.Fy * yp), 0.5);
			Reporting.AddLine("tp_Req = (1.11 * L_Mf / (" + ConstString.FIOMEGA0_9 + "* Fy * yp)) ^ 0.5");
			Reporting.AddLine("= (1.11 * " + CommonDataStatic.SeismicSettings.L_Mf + " / (" + ConstString.FIOMEGA0_9 + " * " + leftBeam.WinConnect.MomentEndPlate.Material.Fy + " * " + yp + ")) ^ 0.5");
			Reporting.AddLine("= " + tp_Req + ConstUnit.Length + " (Minimum)");

			// Step 6
			// step 7 Beam Flange Force			
			Reporting.AddHeader("Factored Beam Flange Force (Ffu):");
			L_Ffu = CommonDataStatic.SeismicSettings.L_Mf / (leftBeam.Shape.d - leftBeam.Shape.tf);
			Reporting.AddLine("Ffu = Mf / (d - tbf)");
			Reporting.AddLine("= " + CommonDataStatic.SeismicSettings.L_Mf + " / (" + leftBeam.Shape.d + " - " + leftBeam.Shape.tf + ") = " + L_Ffu + ConstUnit.Force);

			if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4)
			{
				// shear yielding
				FiRnY = ConstNum.FIOMEGA1_0N * 0.6 * leftBeam.WinConnect.MomentEndPlate.Material.Fy * leftBeam.WinConnect.MomentEndPlate.Width * leftBeam.WinConnect.MomentEndPlate.Thickness;
				// Shear Rupture
				An = (leftBeam.WinConnect.MomentEndPlate.Width - 2 * (leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ConstNum.EIGHTH_INCH * ConstNum.ONE_INCH)) * leftBeam.WinConnect.MomentEndPlate.Thickness;
				FiRnR = ConstNum.FIOMEGA0_75N * 0.6 * leftBeam.WinConnect.MomentEndPlate.Material.Fu * An;
				FiRn = Math.Min(FiRnY, FiRnR);

				// step 8 End Plate Shear Yielding (Out of Plane)
				Reporting.AddHeader("End Plate Shear Yielding:");
				FiRn = ConstNum.FIOMEGA1_0N * 0.6 * leftBeam.WinConnect.MomentEndPlate.Material.Fy * leftBeam.WinConnect.MomentEndPlate.Width * leftBeam.WinConnect.MomentEndPlate.Thickness;
				Reporting.AddLine("FiRn = " + ConstNum.FIOMEGA1_0N + " * 0.6 * Fy * bp * tp");
				Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + leftBeam.WinConnect.MomentEndPlate.Material.Fy + " * " + leftBeam.WinConnect.MomentEndPlate.Width + " * " + leftBeam.WinConnect.MomentEndPlate.Thickness);
				if (FiRn < L_Ffu / 2)
				{
					Reporting.AddLine("= " + FiRn + " << " + L_Ffu + " / 2 = L_Ffu / 2" + ConstUnit.Force);
					Reporting.AddLine("Plate Yield Strength not satisfied, Increase thickness");
				}
				else
					Reporting.AddLine("= " + FiRn + " >= " + L_Ffu + " / 2 = " + (L_Ffu / 2) + ConstUnit.Force + " (OK)");

				Reporting.AddHeader("End Plate Shear Rupture (Out of Plane):");
				An = (leftBeam.WinConnect.MomentEndPlate.Width - 2 * (leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ConstNum.EIGHTH_INCH * ConstNum.ONE_INCH)) * leftBeam.WinConnect.MomentEndPlate.Thickness;
				Reporting.AddLine("An = (bp - 2 * (db + " + ConstNum.EIGHTH_INCH + ")) * tp");
				Reporting.AddLine("= (" + leftBeam.WinConnect.MomentEndPlate.Width + " - 2 * (" + leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + " + " + ConstNum.EIGHTH_INCH + ")) * " + leftBeam.WinConnect.MomentEndPlate.Thickness + "= " + An + ConstUnit.Area);
				FiRn = ConstNum.FIOMEGA0_75N * 0.6 * leftBeam.WinConnect.MomentEndPlate.Material.Fu * An;
				Reporting.AddLine("FiRn= " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu  * An");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.6 * " + leftBeam.WinConnect.MomentEndPlate.Material.Fu + " * " + An);

				if (FiRn < L_Ffu / 2)
				{
					Reporting.AddLine("= " + FiRn + " << " + L_Ffu + " / 2 = " + (L_Ffu / 2) + ConstUnit.Force);
					Reporting.AddLine("Plate Rupture Strength not satisfied");
				}
				else
					Reporting.AddLine("= " + FiRn + " >= " + L_Ffu + " / 2 = " + (L_Ffu / 2) + ConstUnit.Force + " (OK)");

				// Step 10 Stiffener thickness
				Reporting.AddHeader("End PlateStiffener Thickness:");
				ts_min = leftBeam.Shape.tw * leftBeam.Material.Fy / leftBeam.WinConnect.MomentEndPlate.Material.Fy;
				ts = ts_min;
				Reporting.AddLine("ts_min = tbw * Fyb / Fys");
				Reporting.AddLine("= " + leftBeam.Shape.tw + " * " + leftBeam.Material.Fy + " / " + leftBeam.WinConnect.MomentEndPlate.Material.Fy);
				Reporting.AddLine("= " + ts_min + ConstUnit.Length);

				// stiffener width thickness ratio:
				Reporting.AddHeader("Stiffener width/thickness ratio:");
				Limit = 0.56 * Math.Pow(ConstNum.ELASTICITY / leftBeam.WinConnect.MomentEndPlate.Material.Fy, 0.5);

				do
				{
					ratio = CommonDataStatic.SeismicSettings.L_hst / ts;

					if (Limit < ratio)
					{
						loopAgain = true;
						ts = ts + ConstNum.SIXTEENTH_INCH;
					}
					else
						loopAgain = false;
				} while (loopAgain);

				CommonDataStatic.SeismicSettings.L_ts = CommonCalculations.PlateThickness(ts);
				Reporting.AddLine("ts = " + ts + "  >= " + ts_min + ConstUnit.Length + " (OK)");

				Reporting.AddHeader("Limiting width/Thickness ratio:");
				Reporting.AddLine("= 0.56 * (" + ConstNum.ELASTICITY + " / " + leftBeam.WinConnect.MomentEndPlate.Material.Fy + ") ^ 0.5 = " + Limit);
				Reporting.AddLine("hst / ts = " + CommonDataStatic.SeismicSettings.L_hst + " / " + CommonDataStatic.SeismicSettings.L_ts + " = " + ratio + "  <=  " + Limit + " (OK)");

				// Stiffener welds:			
				Reporting.AddLine("End Plate Stiffener Welds:");
				if (CommonDataStatic.SeismicSettings.L_ts > 0.375 * ConstNum.ONE_INCH)
				{
					Reporting.AddHeader("Stiffener thicknes (ts) = " + CommonDataStatic.SeismicSettings.L_ts + " > " + 0.375 * ConstNum.ONE_INCH + ConstUnit.Length);
					Reporting.AddLine("Use CJP Groove welds");

					CommonDataStatic.SeismicSettings.StiffenerToEndPlateWeldIsCJP = true;
					CommonDataStatic.SeismicSettings.StiffenerToEndPlateWeldIsCJP = true;
				}
				else
				{
					CommonDataStatic.SeismicSettings.StiffenerToEndPlateWeldIsCJP = false;
					CommonDataStatic.SeismicSettings.StiffenerToEndPlateWeldIsCJP = false;
					Reporting.AddHeader("Stiffener thicknes (ts) = " + CommonDataStatic.SeismicSettings.L_ts + " <= " + 0.375 * ConstNum.ONE_INCH + ConstUnit.Length);
					Reporting.AddLine("Use fillet welds");

					Reporting.AddHeader("Beam Flange to Stiffener Weld:");
					w_req = leftBeam.WinConnect.MomentEndPlate.Material.Fu * CommonDataStatic.SeismicSettings.L_ts / (1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddLine("w_Req = Fu * ts / (1.414 * Fexx)");
					Reporting.AddLine("= " + leftBeam.WinConnect.MomentEndPlate.Material.Fu + " * " + CommonDataStatic.SeismicSettings.L_ts + " /(1.414 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
					Reporting.AddLine("= " + w_req + ConstUnit.Length);

					Reporting.AddHeader("End Plate to Stiffener Weld:");
					w_req = leftBeam.WinConnect.MomentEndPlate.Material.Fu * CommonDataStatic.SeismicSettings.L_ts / (1.273F * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddLine("w_Req = Fu * L_ts / (1.273 * Fexx)");
					Reporting.AddLine("= " + leftBeam.WinConnect.MomentEndPlate.Material.Fu + " * " + ts + " /(1.273 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
					Reporting.AddLine("= " + w_req + ConstUnit.Length);
				}

				// Step 11 Bolt Shear Rupture Strength
				if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
					NB = 4;
				else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
					NB = 8;

				Reporting.AddHeader("Bolt Shear Rupture Strength:");
				FiRn = NB * leftBeam.WinConnect.MomentEndPlate.Bolt.BoltStrength;
				Reporting.AddLine("= NB * Fv = " + NB + " * " + leftBeam.WinConnect.MomentEndPlate.Bolt.BoltStrength);

				if (FiRn >= CommonDataStatic.SeismicSettings.L_Vu)
					Reporting.AddLine("= " + FiRn + " >= " + CommonDataStatic.SeismicSettings.L_Vu + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + FiRn + " << " + CommonDataStatic.SeismicSettings.L_Vu + ConstUnit.Force + " (NG)");

				// Step 12 Bolt Bearing			
				Reporting.AddHeader("Bolt Bearing:");
				if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
				{
					Ni = 2;
					Lc = pfi + leftBeam.Shape.tf + pfo - (leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH);
					Reporting.AddLine("Lc = pfi + tf + Pfo - (db + " + ConstNum.SIXTEENTH_INCH + ")");
					Reporting.AddLine("= " + pfi + " + " + leftBeam.Shape.tf + " + " + pfo + " - (" + db + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lc + ConstUnit.Length);

				}
				else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
				{
					Ni = 4;
					Lc = Pb - (leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ConstNum.SIXTEENTH_INCH);
					Reporting.AddLine("Lc = pb - (db + " + ConstNum.SIXTEENTH_INCH + ")");
					Reporting.AddLine("= " + Pb + " - (" + leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + " + " + ConstNum.SIXTEENTH_INCH + ")= " + Lc + ConstUnit.Length);
				}
				No = Ni;
				Rni = Math.Min(1.2 * Lc * leftBeam.WinConnect.MomentEndPlate.Thickness * leftBeam.WinConnect.MomentEndPlate.Material.Fu, 2.4F * leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize * leftBeam.WinConnect.MomentEndPlate.Thickness * leftBeam.WinConnect.MomentEndPlate.Material.Fu);
				Reporting.AddLine(" Rni = Min(1.2 * Lc * t * Fu, 2.4 * db * t * Fu)");
				Reporting.AddLine("= Min(1.2 * " + Lc + " * " + leftBeam.WinConnect.MomentEndPlate.Thickness + " * " + leftBeam.WinConnect.MomentEndPlate.Material.Fu + " , 2.4 * " + leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + " * " + leftBeam.WinConnect.MomentEndPlate.Thickness + " * " + leftBeam.WinConnect.MomentEndPlate.Material.Fu + ")");
				Reporting.AddLine("= " + Rni + ConstUnit.Force);

				Rno = Math.Min(1.2 * Lc * leftBeam.WinConnect.MomentEndPlate.Thickness * leftBeam.WinConnect.MomentEndPlate.Material.Fu, 2.4F * leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize * leftBeam.WinConnect.MomentEndPlate.Thickness * leftBeam.WinConnect.MomentEndPlate.Material.Fu);
				Reporting.AddLine(" Rno = Min(1.2 * Lc * t * Fu, 2.4 * db * t * Fu)");
				Reporting.AddLine("= Min(1.2 * " + Lc + " * " + leftBeam.WinConnect.MomentEndPlate.Thickness + " * " + leftBeam.WinConnect.MomentEndPlate.Material.Fu + " , 2.4 * " + leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + " * " + leftBeam.WinConnect.MomentEndPlate.Thickness + " * " + leftBeam.WinConnect.MomentEndPlate.Material.Fu + ")");
				Reporting.AddLine("= " + Rno + ConstUnit.Force);

				Reporting.AddLine("Bearing On End Plate:");
				FiRn = ConstNum.FIOMEGA0_75N * Ni * Rni + ConstNum.FIOMEGA0_75N * No * Rno;
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * Ni * Rni + " + ConstString.FIOMEGA0_75 + " * No * Rno");
				Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * " + Ni + " * " + Rni + " + " + ConstString.FIOMEGA0_75 + " * " + No + " * " + Rno);
				if (FiRn >= CommonDataStatic.SeismicSettings.L_Vu)
					Reporting.AddLine("= " + FiRn + " >= " + CommonDataStatic.SeismicSettings.L_Vu + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + FiRn + " << " + CommonDataStatic.SeismicSettings.L_Vu + ConstUnit.Force + " (OK)");

				Reporting.AddHeader("Bearing On Column Flange");
				// FiRn_Column = FiRn * Column.tf * column.Material.Fu / RMomentEndPlate.t * RMomentEndPlate.Fu
				FiRn = ConstNum.FIOMEGA0_75N * Ni * Rni + ConstNum.FIOMEGA0_75N * No * Rno;
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * Ni * Rni + " + ConstString.FIOMEGA0_75 + " * No * Rno");
				Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * " + Ni + " * " + Rni + " + " + ConstString.FIOMEGA0_75 + " * " + No + " * " + Rno);
				if (FiRn >= CommonDataStatic.SeismicSettings.L_Vu)
					Reporting.AddLine("= " + FiRn + " >= " + CommonDataStatic.SeismicSettings.L_Vu + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + FiRn + " << " + CommonDataStatic.SeismicSettings.L_Vu + ConstUnit.Force + " (OK)");

				// Step 13 Beam to EndPlate Welds
				Reporting.AddHeader("Beam-to-End Plate Welds:");
				Reporting.AddLine("Beam Flange to End Plate Weld is CJP");
				if (CommonDataStatic.Units == EUnit.US)
					Reporting.AddLine("with 5/16 inch fillet weld on inside face of flange.");
				else if (CommonDataStatic.Units == EUnit.Metric)
					Reporting.AddLine("with 8 mm fillet weld on inside face of flange.");

				Reporting.AddLine("Beam Web-to-End Plate Weld:");
				Reporting.AddLine("Use CJP Weld.");

				SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.PlateThickness, ref MaxValue, ref MinValue);
				SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.EndPlateWidth, ref MaxValue, ref MinValue);

				if (Bp > MaxValue)
					Bp = MaxValue;
				if (Bp < MinValue)
					Bp = MinValue;

                if (!leftBeam.WinConnect.MomentEndPlate.Width_User) 
                    leftBeam.WinConnect.MomentEndPlate.Width = Bp;
				
				// step 2
				SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltGage, ref MaxValue, ref MinValue);
				Gage = column.GageOnFlange;
				if (Gage > MaxValue)
					Gage = MaxValue;
				if (Gage < MinValue)
					Gage = MinValue;
				g = Gage;

				SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltDistanceFromBeamFlange, ref MaxValue, ref MinValue);

				if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
				{
					SeismicCalc.EndPlatePrequalifiedLimitations(leftBeam, CommonDataStatic.SeismicSettings.ConnectionType, EPrequalifiedLimitType.BoltVerticalSpacing, ref MaxValue, ref MinValue);
					Pb = -((int) Math.Floor((double) -4 * (2 + 2 / 3) * rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize)) / 4.0;
					if (Pb > MaxValue)
						Pb = MaxValue;
					if (Pb < MinValue)
						Pb = MinValue;
				}
				rightBeam.WinConnect.MomentEndPlate.Bolt.SpacingLongDir = Pb;

				// Step 14 Column Flange Flexural Yielding
				Reporting.AddHeader("Column Side Design for Left Side Beam:");
				Reporting.AddLine("Column Flange Flexural Yielding");

				bcf = column.Shape.bf;
				Reporting.AddLine("bcf = " + column.Shape.bf + ConstUnit.Length);
				s = 0.5 * Math.Pow(bcf * g, 0.5);
				Reporting.AddLine("s = 0.5 * (bcf * g) ^ 0.5 = 0.5 * (" + bcf + " * " + g + ") ^ 0.5 = " + s + ConstUnit.Length);

				if (CommonDataStatic.ColumnStiffener.SThickness == 0)
				{
					// unstiffened column

					c = pfi + pfo + leftBeam.Shape.tf;
					Reporting.AddLine("c = pfi + Pfo + LBeam.tf = " + pfi + " + " + pfo + " + " + leftBeam.Shape.tf + " = " + c + ConstUnit.Length);
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
					{
						yc = (bcf / 2 * (h1 / s + h0 / s) + 2 / g * (h1 * (s + 3 * c / 4) + h0 * (s + c / 4) + Math.Pow(c, 2) / 2) + g / 2);
						Reporting.AddLine("yc = bcf/2 * (h1/s + h0/s) + 2/g * (h1 * (s + 3 * c/4) + h0 * (s + c/4) + c² /2) + g/2");
						Reporting.AddLine("=  " + bcf + "/2 * ( " + h1 + " / " + s + "  +  " + h0 + "  /  " + s + " ) + 2 /  " + g + "  * ( " + h1 + "  * ( " + s + " + 3 *  " + c + "/4) +  " + h0 + "  * ( " + s + " + " + c + "/4) +  " + c + "^ 2 / 2) +  " + g + "/2");
						Reporting.AddLine("= " + yc + ConstUnit.Length);
					}
					else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
					{
						yc = (bcf / 2 * (h1 / s + h4 / s) + 2 / g * (h1 * (Pb + c / 2 + s) + h2 * (Pb / 2 + c / 4) + h3 * (Pb / 2 + c / 2) + h4 * s) + g / 2);
						Reporting.AddLine("yc = bcf/2 * (h1/s + h4/s) + 2/g * (h1 * (Pb + c/2 + s) + h2 * (Pb/2 + c/4) + h3 * (Pb/2 + c/2) + h4 * s) + g/2");
						Reporting.AddLine("= " + bcf + "/2 * (" + h1 + " / " + s + " + " + h4 + " / " + s + ") + 2 / " + g + " * (" + h1 + " * (" + Pb + " + " + c + "/2 + " + s + ") + " + h2 + " * (" + Pb + "/2 + " + c + "/4) + " + h3 + " * (" + Pb + "/2 +  " + c + "/2) + " + h4 + " * " + s + ") +  " + g + "/2");
						Reporting.AddLine("= " + yc + ConstUnit.Length);
					}
					Yc_Unstiffened = yc;
				}
				else
				{
					// stiffened column
					c = pfi + pfo + CommonDataStatic.ColumnStiffener.SThickness;
					if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
					{
						c = pfi + pfo + CommonDataStatic.ColumnStiffener.SThickness;
						Reporting.AddLine("c = pfi + Pfo + Col_Stiff_t = " + pfi + " + " + pfo + " + " + CommonDataStatic.ColumnStiffener.SThickness + " = " + c + ConstUnit.Length);
						yc = bcf / 2 * (h1 * (1 / s + 1.0 / psi) + h0 * (1 / s + 1.0 / pso)) + 2 / g * (h1 * (s + psi) + h0 * (s + pso));
						Reporting.AddLine("yc = bcf/2 * (h1 *(1/s + 1/psi) + h0 * (1/s + 1/pso)) + 2/g * (h1 * (s + psi) + h0 * (s + pso))");
						Reporting.AddLine("= " + bcf + "/2 * (" + h1 + " * (1/" + s + " + 1/" + psi + ") + " + h0 + " * (1/" + s + " + 1/" + pso + ")) + 2/" + g + " * (" + h1 + " * (" + s + " + " + psi + ") + " + h0 + " * (" + s + " + " + pso + "))");
						Reporting.AddLine("= " + yc + ConstUnit.Length);
					}
					else
					{
						// EPS8
						yc = bcf / 2 * (h1 / s + h2 / pso + h3 / psi + h4 / s) + 2 / g * (h1 * (s + Pb / 4) + h2 * (pso + 3 * Pb / 4) + h3 * (psi + Pb / 4) + h4 * (s + 3 * Pb / 4) + Math.Pow(Pb, 2)) + g;
						Reporting.AddLine("yc = bcf/2 * ((h1/s) + h2/pso + h3/psi + h4/s) + 2/g * (h1 * (s + Pb/4) + h2 * (pso + 3 * Pb/4) + h3 * (psi + Pb/4) + h4 * (s + 3 * Pb/4) + Pb²) + g");
						Reporting.AddLine("= " + bcf + "/2 * ((" + h1 + " / " + s + ") + " + h2 + " / " + pso + " + " + h3 + " / " + psi + " + " + h4 + " / " + s + ") + 2/" + g + " * (" + h1 + " * (" + s + " + " + Pb + "/4) + " + h2 + " * (" + pso + " + 3 * " + Pb + "/4) + " + h3 + " * (" + psi + " + " + Pb + "/4) + " + h4 + " * (" + s + " + 3 * " + Pb + "/4) + " + Pb + " ^ 2) + " + g);
						Reporting.AddLine("= " + yc + ConstUnit.Length);
					}
				}

				Reporting.AddLine("Required Column tf");
				tcf_req = Math.Pow(1.11 * CommonDataStatic.SeismicSettings.L_Mf / (ConstNum.FIOMEGA0_9N * column.Material.Fy * yc), 0.5);
				Reporting.AddLine("tcf_req = (1.11 * Mf / (" + ConstString.FIOMEGA0_9 + " * Fy * yc)) ^ 0.5");
				Reporting.AddLine("= (1.11 * " + CommonDataStatic.SeismicSettings.L_Mf + " / (" + ConstString.FIOMEGA0_9 + " * " + column.Material.Fy + " * " + yc + ")) ^ 0.5");

				if (tcf_req <= column.Shape.tf)
				{
					Reporting.AddLine("= " + tcf_req + " <=  tcf = " + column.Shape.tf + ConstUnit.Length + " (OK)");
					Reporting.AddLine("Stiffeners are not required for Col. Flange Flextural Yielding.");
				}
				else
				{
					Reporting.AddLine("= " + tcf_req + " >>  tcf = " + column.Shape.tf + ConstUnit.Length);
					Reporting.AddLine("Stiffeners are required for Col. Flange Flextural Yielding.");
				}

				if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.E4 || CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES4)
					extension = pfo + de;
				else if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
					extension = pfo + Pb + de;

                if (!leftBeam.WinConnect.MomentEndPlate.Length_User) 
                    leftBeam.WinConnect.MomentEndPlate.Length = leftBeam.Shape.d + 2 * extension;

				// Step 15 Required Stiffener Force			
				Reporting.AddHeader("Required Stiffener Force:");
				Reporting.AddLine("Column Flange Flexural Design Strength:");
				FidMcf = (ConstNum.FIOMEGA0_9N * column.Material.Fy * Yc_Unstiffened * Math.Pow(column.Shape.tf, 2));
				Reporting.AddLine("FidMcf = " + ConstString.FIOMEGA0_9 + " * Fy * Yc * Column.tf²");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + column.Material.Fy + " * " + Yc_Unstiffened + " * " + column.Shape.tf + "² = " + FidMcf + ConstUnit.MomentUnitInch);

				Reporting.AddHeader("Column Flange Design Force, FidRn");
				FidRn = FidMcf / (leftBeam.Shape.d - leftBeam.Shape.tf);
				Reporting.AddLine("FidRn = FidMcf / (d - tf)= " + FidMcf + " / (" + leftBeam.Shape.d + " - " + leftBeam.Shape.tf + ") = " + FidRn + ConstUnit.Force);
				FidRn15 = FidRn;

				// Step 16 Column Web Yielding
				Reporting.AddLine("Column Web Local Yielding Strength:");

				// If LDistanceToColumnEnd < Column.d Then
				if (CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn)
					ct = 1;
				else
					ct = 0.5;

				Reporting.AddLine("Ct = " + ct);
				Rn = ct * (6 * column.Shape.kdes + leftBeam.Shape.tf + 2 * leftBeam.WinConnect.MomentEndPlate.Thickness) * column.Material.Fy * column.Shape.tw;
				Reporting.AddLine(" Rn = ct * (6 * kc + tbf + 2 * tp) * Fyc * tcw");
				Reporting.AddLine("= ct * (6 * " + column.Shape.kdes + " + " + leftBeam.Shape.tf + " + 2 * " + leftBeam.WinConnect.MomentEndPlate.Thickness + ") * " + column.Material.Fy + " * " + column.Shape.tw);
				Reporting.AddLine("= " + Rn + ConstUnit.Force);
				FidRn = ConstNum.FIOMEGA1_0N * Rn;
				Reporting.AddLine("FidRn = " + ConstNum.FIOMEGA1_0N + " * Rn = " + ConstNum.FIOMEGA1_0N + " * " + Rn + " = " + FidRn + ConstUnit.Force);

				if (FidRn >= L_Ffu)
				{
					Reporting.AddLine(FidRn + " >= " + L_Ffu + ConstUnit.Force);
					Reporting.AddLine("ContinuityPlates Not Required for Column Web Yielding");
				}
				else
				{
					Reporting.AddLine(FidRn + " << " + L_Ffu + ConstUnit.Force);
					Reporting.AddLine("ContinuityPlates are Required for Column Web Yielding");
				}
				FidRn16 = FidRn;

				// Step 17 ColumnWebBuckling		
				Reporting.AddHeader("Column Web Buckling");
				if (CommonDataStatic.ColumnStiffener.TopOfBeamToColumn >= column.Shape.d / 2)
				{
					FiRn = ConstNum.FIOMEGA0_75N * (24 * Math.Pow(column.Shape.tw, 3) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy, 0.5)) / column.Shape.t;
					Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * (24 * tcw ^ 3 * (E * Fyc) ^ 0.5) / h");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * (24 * " + column.Shape.tw + " ^ 3 * (" + ConstNum.ELASTICITY + " * " + column.Material.Fy + ") ^ 0.5) / " + column.Shape.t);
					Reporting.AddLine("= " + FiRn + ConstUnit.Force);
				}
				else
				{
					FiRn = ConstNum.FIOMEGA0_75N * 12 * Math.Pow(column.Shape.tw, 3) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy, 0.5) / column.Shape.t;
					Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * (12 * tcw ^ 3 * (E * Fyc) ^ 0.5) / h");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * (12 * " + column.Shape.tw + " ^ 3 * (" + ConstNum.ELASTICITY + " * " + column.Material.Fy + ") ^ 0.5) / " + column.Shape.t);
					Reporting.AddLine("= " + FiRn + ConstUnit.Force);
				}

				if (FiRn >= L_Ffu)
				{
					Reporting.AddLine(" " + FiRn + " >= " + L_Ffu + ConstUnit.Force);
					Reporting.AddLine("Continuity Plates Not Required for Column Web Buckling.");
				}
				else
				{
					Reporting.AddLine(" " + FiRn + " << " + L_Ffu + ConstUnit.Force);
					Reporting.AddLine("ContinuityPlates Required for Column Web Buckling.");
				}
				FinRn17 = FiRn;

				// Step 18 Column Web Crippling
				Reporting.AddHeader("Column Web Crippling");
				n = leftBeam.Shape.tf;
				if (CommonDataStatic.ColumnStiffener.TopOfBeamToColumn >= column.Shape.d / 2)
				{
					FiRn = ConstNum.FIOMEGA0_75N * 0.8 * Math.Pow(column.Shape.tw, 2) * (1 + 3 * (n / column.Shape.d) * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);
					Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * 0.8 * tcw² * (1 + 3 * (N/dc) * (tcw/tcf)^1.5) * (E * Fyc * tcf/tcw)^0.5");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.8 * " + column.Shape.tw + "² * (1 + 3 * (" + n + " / " + column.Shape.d + " ) * (" + column.Shape.tw + " / " + column.Shape.tf + ")^1.5) * (" + ConstNum.ELASTICITY + " * " + column.Material.Fy + " * " + column.Shape.tf + " / " + column.Shape.tw + ")^0.5");
				}
				else
				{
					if (n / column.Shape.d < 0.2)
					{
						FiRn = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(column.Shape.tw, 2) * (1 + 3 * (n / column.Shape.d) * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);
						Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * 0.4 * tcw² * (1 + 3 * (N/dc) * (tcw/tcf)^1.5) * (E * Fyc * tcf/tcw)^0.5");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.8 * " + column.Shape.tw + "² * (1 + 3 * (" + n + " / " + column.Shape.d + " ) * (" + column.Shape.tw + " / " + column.Shape.tf + ")^1.5) * (" + ConstNum.ELASTICITY + " * " + column.Material.Fy + " * " + column.Shape.tf + " / " + column.Shape.tw + ")^0.5");
					}
					else
					{
						FiRn = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(column.Shape.tw, 2) * (1 + (4 * n / column.Shape.d - 0.2) * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);
						Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * 0.4 * tcw² * (1 +(4*N/dc-0.2) * (tcw/tcf)^1.5) * (E * Fyc * tcf/tcw)^0.5");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + column.Shape.tw + "² * (1 + (4*" + n + " / " + column.Shape.d + "-0.2) * (" + column.Shape.tw + " / " + column.Shape.tf + ")^1.5) * (" + ConstNum.ELASTICITY + " * " + column.Material.Fy + " * " + column.Shape.tf + " / " + column.Shape.tw + ")^0.5");
					}
				}
				if (FiRn >= L_Ffu)
				{
					Reporting.AddLine("= " + FiRn + "  >= " + L_Ffu + "  " + ConstUnit.Force + " (OK)");
					Reporting.AddLine("Continuity Plates Not Required for column web crippling");
				}
				else
				{
					Reporting.AddLine("= " + FiRn + "  << " + L_Ffu + "  " + ConstUnit.Force + " (NG)");
					Reporting.AddLine("Continuity Plates Required for column web crippling");
				}

				// Step 19 Stiffener Required Strength
				Reporting.AddHeader("Stiffener Required Strength");
				minFsu = Math.Min(Math.Min(FidRn15, FidRn16), Math.Min(FinRn17, FinRn18));
				L_Fsu = L_Ffu - Math.Min(Math.Min(FidRn15, FidRn16), Math.Min(FinRn17, FinRn18));
				Reporting.AddLine("Fsu = Ffu-Min(" + FidRn15 + ", " + FidRn16 + ", " + FinRn17 + ", " + FinRn18 + ")");
				Reporting.AddLine("= " + L_Ffu + "- " + minFsu + " = " + L_Fsu + ConstUnit.Force);

				// Step 20 Panel Zone per Provisions Section 9.3 for SMF and 10.3 for IMF
				leftBeam.MomentConnection = EMomentCarriedBy.EndPlate;
				if (!leftBeam.EndSetback_User)
					leftBeam.EndSetback = leftBeam.WinConnect.MomentEndPlate.Thickness;
                if (!leftBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt_User) 
                    leftBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt = pfi;
				if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.ES8)
                    if (!leftBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts_User) 
                        leftBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts = 8;
				else
                    if (!leftBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts_User) 
                        leftBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts = 4;

                if (!leftBeam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir_User) 
                    leftBeam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir = Pb;
				leftBeam.WinConnect.MomentEndPlate.BraceStiffener.Length = CommonDataStatic.SeismicSettings.L_lst;

				leftBeam.WinConnect.MomentEndPlate.BraceStiffener.Thickness = CommonDataStatic.SeismicSettings.L_ts;
				leftBeam.Moment = CommonDataStatic.SeismicSettings.L_Mf;
				leftBeam.ShearForce = CommonDataStatic.SeismicSettings.L_Vu;
			}
		}
	}
}