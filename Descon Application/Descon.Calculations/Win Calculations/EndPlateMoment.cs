using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class EndPlateMoment
	{
		internal static void DesignEndPlateMoment(EMemberType memberType)
		{
			int k;
			double wreq;
			double WebWeld;
			double capacity;
			double Fym;
			double wldsize;
			double fweldReq;
			double Meu;
			double Ru;
			double d;
			double transvedgedistance;
			double minedge;
			double WebWeldL;
			double wweldReq;
			double FiMn;
			double tReq;
			double MinimumEffectiveFlangeTipLength2;
			double w;
			double MinimumEffectiveFlangeTipLength1;
			double minweld;
			double RequiredEndPlateThickness;
			double Mu_tip;
			double Mu2;
			double qu;
			double ta = 0;
			double ta_prev;
			double MinThicknessforShearRupture;
			double Lnv;
			double FiRn_perBolt;
			double t_Min;
			double maxrowofshearbolts;
			double BB;
			double MinThicknessforShearYielding;
			double FiRn;
			double AvailableLength;
			double WeldLengthRequiredForBeamShear;
			double L_reqForFlareWeld;
			double BF;
			double Mfws;
			double ShearWeldL;
			double wws1;
			double fws1;
			double Ma = 0;
			double Peff;
			double t = 0;
			double treqForShear;
			double tpa;
			double Meff;
			double AlfaM;
			double Cb;
			double Pe;
			double wtfactor;
			double Ca;
			double Fbt = 0;
			double Fbu = 0;
			double Fb;
			double Favg;
			double Aw;
			double Af;
			double shearbytensionbolts;
			double aa;
			double FiFt;
			double ft;
			double fvv;
			double TensionLimit = 0;
			double a = 0;
			double ShearCap;
			double TenCap;
			double fta;
			double V;
			double NBTOTAL;
			double NsMax;
			double C0;
			int TRYALS;
			int NB;
			string Twice1 = "";
			string EightBolt = "";
			double Abolt;
			double effwidth;
			double B = 0;
			double Ff;
			double shedge;
			double onbesbucuk;
			double blba;
			int twice;

			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var beam = CommonDataStatic.DetailDataDict[memberType];
			var momentEndPlate = beam.WinConnect.MomentEndPlate;

			double minfweld;
			if (CommonDataStatic.IsFema)
				return;

			blba = CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD ? 1.5 : 1;
			onbesbucuk = CommonDataStatic.Units == EUnit.US ? 15.5 : 390;

			if (!CommonDataStatic.IsFema)
			{
				shedge = momentEndPlate.Bolt.MinEdgeSheared;
				if (momentEndPlate.Bolt.EdgeDistLongDir < shedge)
					momentEndPlate.Bolt.EdgeDistLongDir = shedge;
				if (momentEndPlate.Bolt.EdgeDistTransvDir < shedge)
					momentEndPlate.Bolt.EdgeDistTransvDir = shedge;

				Ff = beam.Moment / (beam.Shape.d - beam.Shape.tf) + Math.Abs(beam.P) / 2; // Axial Load included on 8/7/98
				switch (CommonDataStatic.BeamToColumnType)
				{
					case EJointConfiguration.BeamToColumnFlange:
						B = beam.GageOnFlange + 2 * momentEndPlate.Bolt.EdgeDistTransvDir;
						if (!momentEndPlate.Width_User)
							momentEndPlate.Width = NumberFun.Round(Math.Max(beam.Shape.bf + ConstNum.ONE_INCH, B), 16); // Int(-16 * Math.Max((Beam.BF + BirInchN), B)) / 16
						if (momentEndPlate.Width > beam.Shape.bf + ConstNum.ONE_INCH)
							effwidth = beam.Shape.bf + ConstNum.ONE_INCH;
						else
							effwidth = momentEndPlate.Width;

						if (!momentEndPlate.FlangeWeldSize_User)
							momentEndPlate.FlangeWeldSize = NumberFun.Round(Ff / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * (2 * (beam.Shape.bf + beam.Shape.tf) - beam.Shape.tw)), 16); // Int(-16 * Ff / (FiOmega0_75N *0.4242 * Fexx * (2 * (Beam.BF + Beam.tf) - Beam.tw))) / 16
						if (momentEndPlate.FlangeWeldSize > ConstNum.HALF_INCH || momentEndPlate.FlangeWeldType == EWeldType.CJP)
						{
							momentEndPlate.FlangeWeldType = EWeldType.CJP;
							if (!momentEndPlate.FlangeWeldSize_User)
								momentEndPlate.FlangeWeldSize = Math.Min(ConstNum.QUARTER_INCH * beam.Shape.tf, ConstNum.THREE_EIGHTS_INCH);
						}
						Abolt = 0.785398 * momentEndPlate.Bolt.BoltSize * momentEndPlate.Bolt.BoltSize;
						if (!momentEndPlate.DistanceToFirstBolt_User && momentEndPlate.DistanceToFirstBolt < momentEndPlate.Bolt.BoltSize + ConstNum.HALF_INCH)
							momentEndPlate.DistanceToFirstBolt = momentEndPlate.Bolt.BoltSize + ConstNum.HALF_INCH;
						if ((beam.Shape.tf + momentEndPlate.DistanceToFirstBolt + momentEndPlate.Bolt.SpacingTransvDir) * 2 + momentEndPlate.Bolt.SpacingTransvDir <= beam.Shape.d && momentEndPlate.Bolt.ASTMType == EBoltASTM.A325)
							EightBolt = "Y";
						else
							EightBolt = "N";
						if (column.Shape.bf >= onbesbucuk && momentEndPlate.Width >= onbesbucuk)
							Twice1 = "Y";
						else
							Twice1 = "N";
						Twice1 = "N"; // Temporarily set
						NB = 2;
						TRYALS = 0;
						momentEndPlate.BraceStiffener.Thickness = 0;
						momentEndPlate.BraceStiffener.Length = 0;

						twice = 1;
						TWICE4:
						if (!momentEndPlate.Bolt.NumberOfBolts_User)
							momentEndPlate.Bolt.NumberOfBolts = 4 * twice;
						C0 = 1;
						NsMax = 2 * (((int)Math.Floor((beam.Shape.d - 2 * (beam.Shape.tf + momentEndPlate.DistanceToFirstBolt)) / momentEndPlate.Bolt.SpacingTransvDir)) - 1);
						goto TRY4FIRST;
						TRY8:
						TWICE8:
						if (!momentEndPlate.Bolt.NumberOfBolts_User)
							momentEndPlate.Bolt.NumberOfBolts = 8 * twice;
						C0 = 0.75;
						NsMax = 2 * (((int)Math.Floor((beam.Shape.d - 2 * (beam.Shape.tf + momentEndPlate.DistanceToFirstBolt + momentEndPlate.Bolt.SpacingTransvDir)) / momentEndPlate.Bolt.SpacingTransvDir)) - 1);
						TRY4FIRST:
						if (NsMax < 0)
							NsMax = 0;

						k = -2;
						bool NumberOfBoltsForMomentEndPlateOK;

						do
						{
							k += 2;
							// For k = NS To NsMax Step 2
							Abolt = 0.785398 * momentEndPlate.Bolt.BoltSize * momentEndPlate.Bolt.BoltSize;
							NBTOTAL = 2 * momentEndPlate.Bolt.NumberOfBolts + k;
							V = beam.ShearForce / NBTOTAL * momentEndPlate.Bolt.NumberOfBolts;
							switch (momentEndPlate.Bolt.BoltType)
							{
								case EBoltType.SC:
									if (momentEndPlate.Bolt.ASTMType == EBoltASTM.A325)
										fta = ConstNum.FIOMEGA0_75N * ConstNum.COEFFICIENT_90 * Abolt;
									else if (momentEndPlate.Bolt.ASTMType == EBoltASTM.A490)
										fta = ConstNum.FIOMEGA0_75N * ConstNum.COEFFICIENT_113 * Abolt;
									else
										fta = ConstNum.FIOMEGA0_75N * 0.75 * momentEndPlate.Bolt.ASTM.Fu * Abolt;
									TenCap = momentEndPlate.Bolt.NumberOfBolts * C0 * fta;
									ShearCap = NBTOTAL * momentEndPlate.Bolt.BoltStrength;
									break;
								default:
									blba = CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD ? 1.5 : 1;
									fta = BoltsForTension.CalcBoltsForTension(memberType, momentEndPlate.Bolt, Ff, V, momentEndPlate.Bolt.NumberOfBolts, false);
									TenCap = momentEndPlate.Bolt.NumberOfBolts * C0 * fta;
									if (momentEndPlate.Bolt.ASTMType == EBoltASTM.A325)
									{
										a = ConstNum.COEFFICIENT_117;
										TensionLimit = ConstNum.COEFFICIENT_90;
										if (momentEndPlate.Bolt.BoltType == EBoltType.N)
											B = 2.5 * blba;
										else
											B = 2 * blba;
									}
									else if (momentEndPlate.Bolt.ASTMType == EBoltASTM.A490)
									{
										a = ConstNum.COEFFICIENT_147;
										TensionLimit = ConstNum.COEFFICIENT_113;
										if (momentEndPlate.Bolt.BoltType == EBoltType.N)
											B = 2.5 * blba;
										else
											B = 2 * blba;
									}
									else if (CommonDataStatic.Units == EUnit.Metric)
									{
										a = 1.25 * column.Material.Fu;
										if (momentEndPlate.Bolt.BoltType == EBoltType.N)
										{
											B = 2.5 * blba;
											fvv = 0.4 * column.Material.Fu;
											if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
												fvv = 0.45 * column.Material.Fu;
										}
										else
										{
											B = 2 * blba;
											fvv = 0.5 * column.Material.Fu;
											if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
												fvv = 0.563 * column.Material.Fu;
										}
									}
									ft = a - B * V / (momentEndPlate.Bolt.NumberOfBolts * Abolt);
									if (ft > TensionLimit)
										ft = TensionLimit;
									FiFt = ConstNum.FIOMEGA0_75N * ft;
									aa = FiFt * Abolt * momentEndPlate.Bolt.NumberOfBolts;
									if (aa >= Ff)
									{
										fvv = Math.Min((a - Math.Min(TensionLimit, Ff / (Abolt * momentEndPlate.Bolt.NumberOfBolts))) / B, momentEndPlate.Bolt.BoltStrength / (ConstNum.FIOMEGA0_75N * Abolt));
										shearbytensionbolts = ConstNum.FIOMEGA0_75N * Abolt * momentEndPlate.Bolt.NumberOfBolts * fvv;
									}
									else
										shearbytensionbolts = 0;
									ShearCap = (NBTOTAL - momentEndPlate.Bolt.NumberOfBolts) * momentEndPlate.Bolt.BoltStrength + shearbytensionbolts;
									break;
							}
							if (TenCap >= Ff && ShearCap >= beam.ShearForce)
							{
								NumberOfBoltsForMomentEndPlateOK = true;
								goto cik1;
							}
							else
								NumberOfBoltsForMomentEndPlateOK = false;
						} while (k < NsMax);
						cik1:
						if (NumberOfBoltsForMomentEndPlateOK && !momentEndPlate.AdditionalBoltsForShear_User)
							momentEndPlate.AdditionalBoltsForShear = k;
						TRYALS = TRYALS + 1;
						if (!NumberOfBoltsForMomentEndPlateOK)
						{
							if (TRYALS == 1 && EightBolt == "Y")
							{
								if (CommonDataStatic.Units == EUnit.Metric)
								{
									if (momentEndPlate.Bolt.BoltSize < 20)
									{
										momentEndPlate.Bolt.BoltSize = 20;
										Abolt = 0.785398 * momentEndPlate.Bolt.BoltSize * momentEndPlate.Bolt.BoltSize;
									}
								}
								else
								{
									if (momentEndPlate.Bolt.BoltSize < 0.75)
									{
										momentEndPlate.Bolt.BoltSize = 0.75;
										Abolt = 0.785398 * momentEndPlate.Bolt.BoltSize * momentEndPlate.Bolt.BoltSize;
									}
								}
								if (momentEndPlate.Bolt.BoltSize > ConstNum.ONEANDHALF_INCHES)
								{
									momentEndPlate.Bolt.BoltSize = ConstNum.ONEANDHALF_INCHES;
									Abolt = 0.785398 * momentEndPlate.Bolt.BoltSize * momentEndPlate.Bolt.BoltSize;
								}
								goto TRY8;
							}
							if (TRYALS <= 2 && Twice1 == "Y")
							{
								twice = 2;
								goto TWICE4;
							}
							if (TRYALS <= 3 && Twice1 == "Y" && EightBolt == "Y")
							{
								twice = 2;
								goto TWICE8;
							}
						}

						if (momentEndPlate.Bolt.NumberOfBolts / twice < 5)
						{
							Af = beam.Shape.tf * beam.Shape.bf;
							Aw = (beam.Shape.d - 2 * beam.Shape.tf) * beam.Shape.tw;
							Favg = 0.5 * (beam.Material.Fy + momentEndPlate.Material.Fy);
							Fb = ConstNum.FIOMEGA0_75N * momentEndPlate.Material.Fy;

							switch (momentEndPlate.Bolt.ASTMType)
							{
								case EBoltASTM.A325:
									if (CommonDataStatic.Units == EUnit.US)
									{
										Fbu = 93;
										Fbt = 44;
									}
									else
									{
										Fbu = 620;
										Fbt = 310;
									}
									break;
								case EBoltASTM.A490:
									if (CommonDataStatic.Units == EUnit.US)
									{
										Fbu = 115;
										Fbt = 54;
									}
									else
									{
										Fbu = 780;
										Fbt = 390;
									}
									break;
							}

							Ca = 1.2 * (1.29 * Math.Pow(Favg / Fbu, 0.4) * Math.Pow(Fbt / Fb, 0.5));
							if (momentEndPlate.FlangeWeldType == EWeldType.CJP)
								wtfactor = 1;
							else
								wtfactor = 0.707;
							Pe = momentEndPlate.DistanceToFirstBolt - momentEndPlate.Bolt.BoltSize / 4 - wtfactor * momentEndPlate.FlangeWeldSize;
							Cb = Math.Pow(beam.Shape.bf / effwidth, 0.5);
							AlfaM = Ca * Cb * Math.Pow(Af / Aw, 1 / 3) * Math.Pow(Pe / momentEndPlate.Bolt.BoltSize, 0.25);
							Meff = AlfaM * Ff * Pe / 4;
							tpa = Math.Sqrt(4 * Meff / (ConstNum.FIOMEGA0_9N * effwidth * momentEndPlate.Material.Fy));
							treqForShear = Ff / (2 * ConstNum.FIOMEGA1_0N * 0.6 * momentEndPlate.Material.Fy * effwidth);
							if (!momentEndPlate.Thickness_User)
								momentEndPlate.Thickness = NumberFun.Round(Math.Max(tpa, treqForShear), 16); // Int(-16 * Math.Max(tpa, treqForShear)) / 16
							if (!momentEndPlate.Length_User)
								momentEndPlate.Length = beam.Shape.d + (momentEndPlate.DistanceToFirstBolt + momentEndPlate.Bolt.EdgeDistLongDir) * 2;
							if (!momentEndPlate.WebWeldNearLength_User)
								momentEndPlate.WebWeldNearLength = NumberFun.Round(2 * momentEndPlate.Bolt.BoltSize + momentEndPlate.DistanceToFirstBolt, 8);
						}
						else
						{
							t = Ff / (6 * twice);
							if (!momentEndPlate.DistanceToFirstBolt_User && momentEndPlate.DistanceToFirstBolt > ConstNum.TWOANDHALF_INCHES)
								momentEndPlate.DistanceToFirstBolt = ConstNum.TWOANDHALF_INCHES;
							if (!momentEndPlate.Bolt.SpacingTransvDir_User && momentEndPlate.Bolt.SpacingTransvDir > 3 * momentEndPlate.Bolt.BoltSize)
								momentEndPlate.Bolt.SpacingTransvDir = 3 * momentEndPlate.Bolt.BoltSize;

							Peff = momentEndPlate.DistanceToFirstBolt * Math.Sqrt(Math.Pow(beam.GageOnFlange, 2) + Math.Pow(momentEndPlate.DistanceToFirstBolt, 2)) / (4.17 * ConstNum.ONE_INCH);
							Ma = (t * Peff * twice);
							tpa = Math.Sqrt(4 * Ma / (ConstNum.FIOMEGA0_9N * momentEndPlate.Material.Fy * effwidth));
							treqForShear = Ff / (2 * ConstNum.FIOMEGA1_0N * 0.6 * momentEndPlate.Material.Fy * effwidth);

							if (!momentEndPlate.Thickness_User)
								momentEndPlate.Thickness = NumberFun.Round(Math.Max(tpa, treqForShear), 16);

							if (!momentEndPlate.Length_User)
								momentEndPlate.Length = beam.Shape.d + (momentEndPlate.DistanceToFirstBolt + momentEndPlate.Bolt.SpacingTransvDir + momentEndPlate.Bolt.EdgeDistLongDir) * 2;
							momentEndPlate.BraceStiffener.Thickness = NumberFun.Round(beam.Shape.tw * beam.Material.Fy / momentEndPlate.Material.Fy, 8); // Int(-Beam.tw * 8) / 8
							momentEndPlate.BraceStiffener.Length = momentEndPlate.DistanceToFirstBolt + momentEndPlate.Bolt.SpacingTransvDir + momentEndPlate.Bolt.EdgeDistLongDir - ConstNum.QUARTER_INCH;

							momentEndPlate.BraceStiffener.WeldSize = 0;
							if (!momentEndPlate.WebWeldNearLength_User)
								momentEndPlate.WebWeldNearLength = NumberFun.Round(2 * momentEndPlate.Bolt.BoltSize + momentEndPlate.DistanceToFirstBolt + 0.5 * momentEndPlate.Bolt.SpacingTransvDir, 8);
						}
						if (momentEndPlate.FlangeWeldType == EWeldType.Fillet)
						{
							fws1 = CommonCalculations.MinimumWeld(momentEndPlate.Thickness, beam.Shape.tf);
							if (momentEndPlate.FlangeWeldSize < fws1)
								if (!momentEndPlate.FlangeWeldSize_User)
									momentEndPlate.FlangeWeldSize = fws1;
						}
						//  ***CHECK WEB WELD FOR END REACTION
						wws1 = CommonCalculations.MinimumWeld(beam.Shape.tw, momentEndPlate.Thickness);
						if (!momentEndPlate.WebWeldNearSize_User)
							momentEndPlate.WebWeldNearSize = ConstNum.FIOMEGA0_9N * beam.Material.Fy * beam.Shape.tw / (2 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						if (momentEndPlate.WebWeldNearSize < wws1)
						{
							if (!momentEndPlate.WebWeldNearSize_User)
								momentEndPlate.WebWeldNearSize = wws1;
						}
						if (!momentEndPlate.WebWeldNearSize_User)
							momentEndPlate.WebWeldNearSize = NumberFun.Round(momentEndPlate.WebWeldNearSize, 16);
						ShearWeldL = Math.Min(beam.Shape.d / 2 - beam.Shape.tf, beam.Shape.d - 2 * beam.Shape.tf - momentEndPlate.DistanceToFirstBolt - 2 * momentEndPlate.Bolt.BoltSize - (momentEndPlate.Bolt.NumberOfBolts - 4) / 4.0 * momentEndPlate.Bolt.SpacingTransvDir);
						if (!momentEndPlate.WebWeldRestSize_User)
							momentEndPlate.WebWeldRestSize = beam.ShearForce / (2 * ShearWeldL * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						Mfws = CommonCalculations.MinimumWeld(momentEndPlate.Thickness, beam.Shape.tw);
						if (momentEndPlate.WebWeldRestSize < Mfws)
							if (!momentEndPlate.WebWeldRestSize_User)
								momentEndPlate.WebWeldRestSize = Mfws;
						if (!momentEndPlate.WebWeldRestSize_User)
							momentEndPlate.WebWeldRestSize = NumberFun.Round(momentEndPlate.WebWeldRestSize, 16);

						if (!beam.EndSetback_User)
							beam.EndSetback = momentEndPlate.Thickness;
						break;
					case EJointConfiguration.BeamToHSSColumn:
						if (!momentEndPlate.DistanceToFirstBolt_User)
							momentEndPlate.DistanceToFirstBolt = (momentEndPlate.Bolt.SpacingTransvDir - beam.Shape.tf) / 2;

						momentEndPlate.Bolt.NumberOfBolts = 0;
						if (!momentEndPlate.Bolt.NumberOfBolts_User)
							momentEndPlate.Bolt.NumberOfBolts = 4;

						if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
							BF = column.Shape.d;
						else
							BF = column.Shape.bf;

						beam.GageOnFlange = BF + 2 * (momentEndPlate.Bolt.BoltSize + ConstNum.HALF_INCH);
						B = beam.GageOnFlange + 2 * momentEndPlate.Bolt.EdgeDistTransvDir;
						if (!momentEndPlate.Width_User)
							momentEndPlate.Width = NumberFun.Round(Math.Max(beam.Shape.bf + ConstNum.ONE_INCH, B), 16);

						// Connection Plate
						L_reqForFlareWeld = NumberFun.Round(Math.Max(3, Ff / (2 * 0.8 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * (5 / 8.0) * column.Shape.tf)), 2);
						if (momentEndPlate.Bolt.SpacingTransvDir < L_reqForFlareWeld)
						{
							if (!momentEndPlate.Bolt.SpacingTransvDir_User)
								momentEndPlate.Bolt.SpacingTransvDir = L_reqForFlareWeld;
							Reporting.AddLine("Effective weld length = Tension bolts vertical spacing is too small. (NG)");
						}
						if (!momentEndPlate.Length_User)
							momentEndPlate.Length = beam.Shape.d - beam.Shape.tf + momentEndPlate.Bolt.SpacingTransvDir + 2 * momentEndPlate.Bolt.EdgeDistLongDir;
						WeldLengthRequiredForBeamShear = beam.ShearForce / (2 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * (5 / 8.0) * column.Shape.tf);
						AvailableLength = Math.Min(beam.Shape.d / 2 - beam.Shape.tf, beam.Shape.d - 1.5 * beam.Shape.tf - momentEndPlate.Bolt.SpacingTransvDir / 2 - 2 * momentEndPlate.Bolt.BoltSize);
						if (AvailableLength < WeldLengthRequiredForBeamShear)
						{
							FiRn = 2 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * (5 / 8.0) * column.Shape.tf * AvailableLength;
							Reporting.AddLine("Beam end reaction is too high!" + "\r" + "Flare bevel welds cannot develop more than " + FiRn + ConstUnit.Force + ". (NG)");
						}
						MinThicknessforShearYielding = beam.ShearForce / (2 * ConstNum.FIOMEGA1_0N * 0.6 * momentEndPlate.Length * momentEndPlate.Material.Fy);
						BB = (beam.GageOnFlange - BF) / 2 + column.Shape.tf;
						aa = (momentEndPlate.Width - beam.GageOnFlange) / 2;
						maxrowofshearbolts = 2 * (((int)Math.Floor((beam.Shape.d - 2 * (beam.Shape.tf + momentEndPlate.DistanceToFirstBolt)) / momentEndPlate.Bolt.SpacingTransvDir)) - 1);
						t_Min = Math.Max(0.1875, MinThicknessforShearYielding);
						for (k = 0; k <= maxrowofshearbolts; k++)
						{
							V = beam.ShearForce * (1 - 2 * k / (2 * k + 8)) / 2;
							FiRn_perBolt = BoltsForTension.CalcBoltsForTension(memberType, momentEndPlate.Bolt, Ff / 2, V / 2, 2, false);
							if (FiRn_perBolt != 0)
							{
								Lnv = momentEndPlate.Length - (k + 4) * (momentEndPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
								MinThicknessforShearRupture = beam.ShearForce / (2 * ConstNum.FIOMEGA0_75N * 0.6 * momentEndPlate.Material.Fu * Lnv);
								t = NumberFun.Round(Math.Max(t_Min, MinThicknessforShearRupture), 16) - ConstNum.SIXTEENTH_INCH;
								do
								{
									t += ConstNum.SIXTEENTH_INCH;
									t = CommonCalculations.PlateThickness(t);
									ta_prev = ta;
									ta = MiscCalculationsWithReporting.HangerStrength(beam.MemberType, momentEndPlate.Bolt.HoleWidth, momentEndPlate.Material.Fu, t, aa, BB, FiRn_perBolt, 0, false);
								} while (Ff > 4 * ta && ta_prev != ta);
								if (Ff <= 4 * ta || k == maxrowofshearbolts)
								{
									if (!momentEndPlate.AdditionalBoltsForShear_User)
										momentEndPlate.AdditionalBoltsForShear = 2 * k;
									break;
								}
							}
						}
						if (k > maxrowofshearbolts)
							return;

						momentEndPlate.ConnectionPlateThickness = t;

						// End Plate
						qu = CommonDataStatic.PryingForce;
						Mu2 = 2 * qu * aa;
						Mu_tip = (beam.GageOnFlange - beam.Shape.bf) / 2 * Ff / 2 - Mu2;
						RequiredEndPlateThickness = Math.Max(momentEndPlate.ConnectionPlateThickness, Math.Pow(Mu_tip * 4 / (ConstNum.FIOMEGA0_9N * momentEndPlate.Material.Fy * (2 * aa + beam.Shape.tf)), 0.5));
						if (!momentEndPlate.Thickness_User)
							momentEndPlate.Thickness = CommonCalculations.PlateThickness(RequiredEndPlateThickness);
						minweld = CommonCalculations.MinimumWeld(beam.Shape.tf, momentEndPlate.Thickness);
						MinimumEffectiveFlangeTipLength1 = Ff / 2 / (ConstNum.FIOMEGA0_9N * beam.Material.Fy * beam.Shape.tf); //  for flange area to resist concentrated tip force
						w = minweld - ConstNum.SIXTEENTH_INCH;
						do
						{
							w += ConstNum.SIXTEENTH_INCH;
							MinimumEffectiveFlangeTipLength2 = Ff / 2 / (ConstNum.FIOMEGA0_75N * 0.6 * 2 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * w) - beam.Shape.tf; //  for weld length needed using minimum size
						} while (MinimumEffectiveFlangeTipLength2 > beam.Shape.bf / 2);
						beam.WinConnect.Fema.L = Math.Max(MinimumEffectiveFlangeTipLength1, MinimumEffectiveFlangeTipLength2);
						if (!momentEndPlate.FlangeWeldSize_User)
							momentEndPlate.FlangeWeldSize = w;
						//  Web Weld for shear
						AvailableLength = Math.Min(beam.Shape.d / 2 - beam.Shape.tf, beam.Shape.d - 1.5 * beam.Shape.tf - momentEndPlate.Bolt.SpacingTransvDir / 2 - 2 * momentEndPlate.Bolt.BoltSize);
						if (!momentEndPlate.WebWeldRestSize_User)
							momentEndPlate.WebWeldRestSize = beam.ShearForce / (2 * AvailableLength * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
						Mfws = CommonCalculations.MinimumWeld(momentEndPlate.Thickness, beam.Shape.tw);
						if (momentEndPlate.WebWeldRestSize < Mfws)
							if (!momentEndPlate.WebWeldRestSize_User)
								momentEndPlate.WebWeldRestSize = Mfws;
						if (!momentEndPlate.WebWeldRestSize_User)
							momentEndPlate.WebWeldRestSize = NumberFun.Round(momentEndPlate.WebWeldRestSize, 16); // Int(-MomentEndPlate.WebWeld * 16) / 16
						if (!momentEndPlate.WebWeldNearLength_User)
							momentEndPlate.WebWeldNearLength = NumberFun.Round(2 * momentEndPlate.Bolt.BoltSize + momentEndPlate.DistanceToFirstBolt, 2); // Int(-(2 * MomentEndPlateBolts.d + MomentEndPlate.DistanceToFirstBolt) * 8) / 8
						if (!momentEndPlate.WebWeldNearSize_User)
							momentEndPlate.WebWeldNearSize = NumberFun.Round(ConstNum.FIOMEGA0_9N * beam.Material.Fy * beam.Shape.tw / (2 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx), 16); // Int(-16 * FiOmega0_9N  * beam.Material.Fy * Beam.tw / (2 * FiOmega0_75N *0.4242 * Fexx)) / 16
						if (momentEndPlate.WebWeldNearSize < Mfws)
							if (!momentEndPlate.WebWeldNearSize_User)
								momentEndPlate.WebWeldNearSize = Mfws;
						if (!beam.EndSetback_User)
							beam.EndSetback = momentEndPlate.Thickness + momentEndPlate.ConnectionPlateThickness;
						break;
				}
			}

			if (momentEndPlate.Bolt.NumberOfBolts > 4)
			{
				momentEndPlate.BraceStiffener.Thickness = NumberFun.Round(beam.Shape.tw, 8); // Int(-Beam.tw * 8) / 8
				if (!momentEndPlate.Length_User)
					momentEndPlate.Length = beam.Shape.d + (momentEndPlate.DistanceToFirstBolt + momentEndPlate.Bolt.SpacingTransvDir + momentEndPlate.Bolt.EdgeDistLongDir) * 2;
			}
			//else if (!momentEndPlate.Length_User) // Uncessary length calc (MT 7/16/15)
			//	momentEndPlate.Length = beam.Shape.d + (momentEndPlate.DistanceToFirstBolt + momentEndPlate.Bolt.EdgeDistLongDir) * 2;

			Reporting.AddHeader(beam.ComponentName + " - " + beam.ShapeName + " Moment Connection");
			if (CommonDataStatic.IsFema)
			{
				FemaReporting.DesignFemaReporting(memberType);
				return;
			}
			Reporting.AddMainHeader("Moment Connection Using End Plate:");
			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
			{
				Reporting.AddLine("End Plate: " + momentEndPlate.Width + ConstUnit.Length + " X " + momentEndPlate.Length + ConstUnit.Length + " X " + momentEndPlate.Thickness + ConstUnit.Length);
				Reporting.AddLine("Connection Plate: " + momentEndPlate.Width + ConstUnit.Length + " X " + momentEndPlate.Length + ConstUnit.Length + " X " + momentEndPlate.ConnectionPlateThickness + ConstUnit.Length);
				Reporting.AddLine("Plate Material: " + momentEndPlate.Material.Name);
				Reporting.AddLine("Bolts: (" + (2 * momentEndPlate.Bolt.NumberOfBolts) + ") " + momentEndPlate.Bolt.BoltName + " Top and Bottom Flange, Total");
				Reporting.AddLine("Bolt Holes on Conn. Plate: " + momentEndPlate.Bolt.HoleWidthSupport + ConstUnit.Length + "  Vert. X " + momentEndPlate.Bolt.HoleLengthSupport + ConstUnit.Length + "  Horiz."); //DESGEN.SupportHole.HoleHoriz
				Reporting.AddLine("Bolt Holes on End Plate: " + momentEndPlate.Bolt.HoleWidth + ConstUnit.Length + "  Vert. X " + momentEndPlate.Bolt.HoleLength + ConstUnit.Length + "  Horiz.");
			}
			else
			{
				Reporting.AddHeader("Plate (W x L x T): " + momentEndPlate.Width + ConstUnit.Length + " X " + momentEndPlate.Length + ConstUnit.Length + " X " + momentEndPlate.Thickness + ConstUnit.Length);
				Reporting.AddLine("Plate Material: " + momentEndPlate.Material.Name);
				Reporting.AddLine("Bolts: (" + (2 * momentEndPlate.Bolt.NumberOfBolts) + ") " + momentEndPlate.Bolt.BoltName + " Top and Bottom Flange, Total");

				Reporting.AddLine("Bolt Holes on Support: " + momentEndPlate.Bolt.HoleWidthSupport + ConstUnit.Length + "  Vert. X " + momentEndPlate.Bolt.HoleLengthSupport + ConstUnit.Length + "  Horiz.");
				Reporting.AddLine("Bolt Holes on Plate: " + momentEndPlate.Bolt.HoleWidth + ConstUnit.Length + "  Vert. X " + momentEndPlate.Bolt.HoleLength + ConstUnit.Length + "  Horiz.");
			}
			if (momentEndPlate.AdditionalBoltsForShear > 0)
			{
				Reporting.AddLine("Bolts: (" + momentEndPlate.AdditionalBoltsForShear + ") " + momentEndPlate.Bolt.BoltName + " Additional Bolts for End Shear");
			}
			Reporting.AddLine("Weld: " + CommonDataStatic.Preferences.DefaultElectrode.Name);

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BeamToHSSColumn:
					Ff = beam.Moment / (beam.Shape.d - beam.Shape.tf) + Math.Abs(beam.P) / 2; // Axial Load included on 8/7/98
					Reporting.AddLine("");
					if (CommonDataStatic.Units == EUnit.US)
					{
						if (beam.P != 0)
						{
							Reporting.AddLine("Flange Force, Ff = M / (d - tf) + P / 2");
							Reporting.AddLine("= " + beam.Moment + " / (" + beam.Shape.d + " - " + beam.Shape.tf + ")+ " + Math.Abs(beam.P) + "/2 = " + Ff + ConstUnit.Force);
						}
						else
						{
							Reporting.AddLine("Flange Force, Ff = M / (d - tf)");
							Reporting.AddLine("= " + beam.Moment + " / (" + beam.Shape.d + " - " + beam.Shape.tf + ") = " + Ff + ConstUnit.Force);
						}
					}
					else
					{
						if (beam.P != 0)
						{
							Reporting.AddLine("Flange Force, Ff = M / (d - tf) + P / 2");
							Reporting.AddLine("= " + beam.Moment + "/ (" + beam.Shape.d + " - " + beam.Shape.tf + ")+ " + Math.Abs(beam.P) + "/2 = " + Ff + ConstUnit.Force);
						}
						else
						{
							Reporting.AddLine("Flange Force, Ff = M / (d - tf)");
							Reporting.AddLine("= " + beam.Moment + " / (" + beam.Shape.d + " - " + beam.Shape.tf + ") = " + Ff + ConstUnit.Force);
						}
					}
					momentEndPlate.Bolt.NumberOfBolts = 0;
					if (!momentEndPlate.Bolt.NumberOfBolts_User)
						momentEndPlate.Bolt.NumberOfBolts = 4;
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
						BF = column.Shape.d;
					else
						BF = column.Shape.bf;

					B = beam.GageOnFlange + 2 * momentEndPlate.Bolt.EdgeDistTransvDir;
					if (!momentEndPlate.Width_User)
						momentEndPlate.Width = NumberFun.Round(Math.Max(beam.Shape.bf + ConstNum.ONE_INCH, B), 16);

					Reporting.AddHeader("Connection Plate");

					// MomentEndPlate.BoltVerticalSpacing = L_reqForFlareWeld
					if (!momentEndPlate.Length_User)
						momentEndPlate.Length = beam.Shape.d - beam.Shape.tf + momentEndPlate.Bolt.SpacingTransvDir + 2 * momentEndPlate.Bolt.EdgeDistLongDir;
					BB = (beam.GageOnFlange - BF) / 2 + column.Shape.tf;
					aa = (momentEndPlate.Width - beam.GageOnFlange) / 2;

					Reporting.AddHeader("Bolt Shear:");
					FiRn = (8 + momentEndPlate.AdditionalBoltsForShear) * momentEndPlate.Bolt.BoltStrength;
					Reporting.AddLine(ConstString.PHI + " Rn = N_total * " + ConstString.PHI + " Rn = " + (8 + momentEndPlate.AdditionalBoltsForShear) + " * " + momentEndPlate.Bolt.BoltStrength);
					if (FiRn >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + FiRn + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRn, "Bolt Shear", memberType);
					else
						Reporting.AddCapacityLine("= " + FiRn + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRn, "Bolt Shear", memberType);

					Reporting.AddHeader("Plate-to Column Welds");
					WeldLengthRequiredForBeamShear = beam.ShearForce / (2 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * (5 / 8.0) * column.Shape.tf);
					AvailableLength = Math.Min(beam.Shape.d / 2 - beam.Shape.tf, beam.Shape.d - 1.5 * beam.Shape.tf - momentEndPlate.Bolt.SpacingTransvDir / 2 - 2 * momentEndPlate.Bolt.BoltSize);
					FiRn = 2 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * (5 / 8.0) * column.Shape.tf * AvailableLength;
					if (AvailableLength < WeldLengthRequiredForBeamShear)
						Reporting.AddLine("Beam end reaction is too high. Flare bevel welds cannot develop more than " + FiRn + ConstUnit.Force + ". (NG)");

					Reporting.AddHeader("Shear Strength of Plate-to-Column Weld:");
					Reporting.AddHeader("Effective Length of Weld:");
					Reporting.AddLine("Le = Min[d / 2 - tf, d - 1.5 * tf - sv / 2 - 2 * d]");
					Reporting.AddLine("= Min[" + beam.Shape.d + "/2 - " + beam.Shape.tf + ", " + beam.Shape.d + " - 1.5 * " + beam.Shape.tf + " - " + momentEndPlate.Bolt.SpacingTransvDir + " / 2 - 2 * " + momentEndPlate.Bolt.BoltSize + "]");
					Reporting.AddLine("= " + AvailableLength + ConstUnit.Length);
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fexx * (5 / 8) * t * Le");
					Reporting.AddLine("= 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * (5 / 8) * " + column.Shape.tf + " * " + AvailableLength);
					if (FiRn >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + FiRn + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRn, "Shear Strength of Plate-to-Column Weld", memberType);
					else
						Reporting.AddCapacityLine("= " + FiRn + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRn, "Shear Strength of Plate-to-Column Weld", memberType);

					FiRn = 2 * 0.8 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * (5 / 8.0) * column.Shape.tf * momentEndPlate.Bolt.SpacingTransvDir;
					Reporting.AddHeader("Tensile Strength of Plate-to-Column Weld:");
					Reporting.AddHeader("Effective Length of Weld, Le = Tension bolt pitch, sv = " + momentEndPlate.Bolt.SpacingTransvDir + ConstUnit.Length);
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * 0.8 * 0.6 * Fexx * (5 / 8) * t * Le");
					Reporting.AddLine("= 2 * 0.8 * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * (5 / 8) * " + column.Shape.tf + " * " + momentEndPlate.Bolt.SpacingTransvDir);
					if (FiRn >= Ff)
						Reporting.AddCapacityLine("= " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / FiRn, "Tensile Strength of Plate-to-Column Weld", memberType);
					else
						Reporting.AddCapacityLine("= " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / FiRn, "Tensile Strength of Plate-to-Column Weld", memberType);

					Reporting.AddHeader("Plate Bending with Prying Action:");
					V = beam.ShearForce * (1 - momentEndPlate.AdditionalBoltsForShear / (momentEndPlate.AdditionalBoltsForShear + 8)) / 2;
					FiRn_perBolt = BoltsForTension.CalcBoltsForTension(memberType, momentEndPlate.Bolt, Ff / 2, V / 2, 2, true);
					ta = MiscCalculationsWithReporting.HangerStrength(beam.MemberType, momentEndPlate.Bolt.HoleWidth, momentEndPlate.Material.Fu, momentEndPlate.ConnectionPlateThickness, aa, BB, FiRn_perBolt, 0, true);
					Reporting.AddLine("Total " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");

					if (4 * ta >= Ff)
						Reporting.AddCapacityLine(ConstString.PHI + " Rn = 4 * " + ConstString.PHI + "Tn = " + 4 * ta + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / (4 * ta), "Plate Bending with Prying Action", memberType);
					else
						Reporting.AddCapacityLine(ConstString.PHI + " Rn = 4 * " + ConstString.PHI + "Tn = " + 4 * ta + " << " + Ff + ConstUnit.Force + " (NG)", Ff / (4 * ta), "Plate Bending with Prying Action", memberType);

					Reporting.AddHeader("Plate Shear");
					Reporting.AddHeader("Shear Yielding of Plate:");
					FiRn = 2 * ConstNum.FIOMEGA1_0N * 0.6 * momentEndPlate.Length * momentEndPlate.ConnectionPlateThickness * momentEndPlate.Material.Fy;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * " + ConstString.FIOMEGA1_0 + " * 0.6 * L * t * Fy");
					Reporting.AddLine("= 2 * " + ConstString.FIOMEGA1_0 + " * 0.6 * " + momentEndPlate.Length + " * " + momentEndPlate.ConnectionPlateThickness + " * " + momentEndPlate.Material.Fy);
					if (FiRn >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + FiRn + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRn, "Shear Yielding of Plate", memberType);
					else
						Reporting.AddCapacityLine("= " + FiRn + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRn, "Shear Yielding of Plate", memberType);

					Reporting.AddHeader("Shear Rupture of Plate:");
					FiRn = 2 * ConstNum.FIOMEGA0_75N * 0.6 * momentEndPlate.Material.Fu * (momentEndPlate.Length - (4 + momentEndPlate.AdditionalBoltsForShear / 2) * (momentEndPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * momentEndPlate.ConnectionPlateThickness;
					Reporting.AddLine(ConstString.PHI + " Rn = (2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu * (L - (4 + Ns / 2) * (dh + " + ConstNum.SIXTEENTH_INCH + "))) * t");
					Reporting.AddLine("= (2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + momentEndPlate.Material.Fu + " * (" + momentEndPlate.Length + " - (4 + " + momentEndPlate.AdditionalBoltsForShear + " / 2) * (" + momentEndPlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + "))) * " + momentEndPlate.ConnectionPlateThickness);
					if (FiRn >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + FiRn + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRn, "Shear Rupture of Plate", memberType);
					else
						Reporting.AddCapacityLine("= " + FiRn + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRn, "Shear Rupture of Plate", memberType);

					Reporting.AddHeader("End Plate");
					Reporting.AddHeader("Plate Bending");
					if (momentEndPlate.Thickness >= momentEndPlate.ConnectionPlateThickness)
						Reporting.AddLine("End PL Thickness = " + momentEndPlate.Thickness + " >= Conn. PL Thickness = " + momentEndPlate.ConnectionPlateThickness + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("End PL Thickness = " + momentEndPlate.Thickness + " << Conn. PL Thickness = " + momentEndPlate.ConnectionPlateThickness + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Bending of Plate at Beam Flange Tip:");
					qu = CommonDataStatic.PryingForce;
					Mu2 = 2 * qu * aa;
					Mu_tip = (beam.GageOnFlange - beam.Shape.bf) / 2 * Ff / 2 - Mu2;
					Reporting.AddLine("Mu_tip = (g - bf) / 2 * Ff / 2 - 2 * qu * a");
					Reporting.AddLine("= (" + beam.GageOnFlange + " - " + beam.Shape.bf + ") / 2 * " + Ff + " / 2 - 2 * " + qu + " * " + aa);
					Reporting.AddLine("= " + Mu_tip + ConstUnit.Moment + "");
					FiMn = ConstNum.FIOMEGA0_9N * momentEndPlate.Material.Fy * (2 * aa + beam.Shape.tf) * Math.Pow(momentEndPlate.Thickness, 2) / 4;
					Reporting.AddLine(ConstString.PHI + "Mn = " + ConstString.FIOMEGA0_9 + " * Fy * (2 * a + tf) * tp² / 4");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + "   * " + momentEndPlate.Material.Fy + " * (2 * " + aa + " + " + beam.Shape.tf + ") * " + momentEndPlate.Thickness + "² / 4");
					if (FiMn >= Mu_tip)
						Reporting.AddCapacityLine("= " + FiMn + " >= " + Mu_tip + ConstUnit.Moment + " (OK)", Mu_tip / FiMn, "Bending of Plate at Beam Flange Tip", memberType);
					else
						Reporting.AddCapacityLine("= " + FiMn + " << " + Mu_tip + ConstUnit.Moment + " (NG)", Mu_tip / FiMn, "Bending of Plate at Beam Flange Tip", memberType);

					Reporting.AddHeader("Plate Shear");
					Reporting.AddHeader("Shear Yielding of Plate:");
					FiRn = 2 * ConstNum.FIOMEGA1_0N * 0.6 * momentEndPlate.Length * momentEndPlate.Thickness * momentEndPlate.Material.Fy;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * " + ConstString.FIOMEGA1_0 + " * 0.6 * L * t * Fy");
					Reporting.AddLine("= 2 * " + ConstString.FIOMEGA1_0 + " * 0.6 * " + momentEndPlate.Length + " * " + momentEndPlate.Thickness + " * " + momentEndPlate.Material.Fy);
					if (FiRn >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + FiRn + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRn, "Shear Yielding of Plate", memberType);
					else
						Reporting.AddCapacityLine("= " + FiRn + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRn, "Shear Yielding of Plate", memberType);

					Reporting.AddHeader("Shear Rupture of Plate:");
					FiRn = 2 * ConstNum.FIOMEGA0_75N * 0.6 * momentEndPlate.Material.Fu * (momentEndPlate.Length - (4 + momentEndPlate.AdditionalBoltsForShear / 2) * (momentEndPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * momentEndPlate.Thickness;
					Reporting.AddLine(ConstString.PHI + " Rn = (2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu * (L - (4 + Ns / 2) * (dh +  " + ConstNum.SIXTEENTH_INCH + "))) * tp");
					Reporting.AddLine("= (2 * " + ConstString.FIOMEGA0_75 + " *0.6 * " + momentEndPlate.Material.Fu + " * (" + momentEndPlate.Length + " - (4 + " + momentEndPlate.AdditionalBoltsForShear + " / 2) * (" + momentEndPlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + "))) * " + momentEndPlate.Thickness);
					if (FiRn >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + FiRn + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRn, "Shear Rupture of Plate", memberType);
					else
						Reporting.AddCapacityLine("= " + FiRn + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRn, "Shear Rupture of Plate", memberType);

					Reporting.AddHeader("Beam-to-End Plate Welds");
					MinimumEffectiveFlangeTipLength1 = Ff / 2 / (ConstNum.FIOMEGA0_9N * beam.Material.Fy * beam.Shape.tf); //  for flange area to resist concentrated tip force
					MinimumEffectiveFlangeTipLength2 = Ff / 2 / (ConstNum.FIOMEGA0_75N * 0.6 * 2 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * momentEndPlate.FlangeWeldSize); //  for weld length needed using minimum size

					Reporting.AddHeader("Flange Weld:");
					Mfws = CommonCalculations.MinimumWeld(momentEndPlate.Thickness, beam.Shape.tf);
					if (Mfws <= momentEndPlate.FlangeWeldSize)
						Reporting.AddLine("Minimum Weld = " + Mfws + " <= " + momentEndPlate.FlangeWeldSize + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Minimum Weld = " + Mfws + " >> " + momentEndPlate.FlangeWeldSize + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Minimum Length of Flange Resisting the Tip Force:");
					Reporting.AddLine("Lf = (Ff / 2) / (" + ConstString.FIOMEGA0_9 + " * Fy * tf)");
					Reporting.AddLine("= (" + Ff + " / 2) / (" + ConstString.FIOMEGA0_9 + " * " + beam.Material.Fy + " * " + beam.Shape.tf + ")");
					if (MinimumEffectiveFlangeTipLength1 <= beam.Shape.bf / 2)
						Reporting.AddCapacityLine("= " + MinimumEffectiveFlangeTipLength1 + " <= bf / 2 = " + (beam.Shape.bf / 2) + ConstUnit.Length + " (OK)", MinimumEffectiveFlangeTipLength1 / (beam.Shape.bf / 2), "Minimum Length of Flange Resisting the Tip Force", memberType);
					else
						Reporting.AddCapacityLine("= " + MinimumEffectiveFlangeTipLength1 + " >> bf / 2 = " + (beam.Shape.bf / 2) + ConstUnit.Length + " (NG)", MinimumEffectiveFlangeTipLength1 / (beam.Shape.bf / 2), "Minimum Length of Flange Resisting the Tip Force", memberType);

					Reporting.AddHeader("Minimum Flange Weld Length to Resist the Tip Force:");
					Reporting.AddLine("Lf = (Ff / 2) / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 2 * Fexx * 0.707 * w)");
					Reporting.AddLine("= (" + Ff + " / 2) / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 2 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * 0.707 * " + momentEndPlate.FlangeWeldSize + ")");
					if (MinimumEffectiveFlangeTipLength2 <= beam.Shape.bf / 2 + beam.Shape.tf)
						Reporting.AddCapacityLine("= " + MinimumEffectiveFlangeTipLength2 + " <= bf / 2 + tf = " + (beam.Shape.bf / 2 + beam.Shape.tf) + ConstUnit.Length + " (OK)", MinimumEffectiveFlangeTipLength2 / (beam.Shape.bf / 2 + beam.Shape.tf), "Minimum Flange Weld Length to Resist the Tip Force", memberType);
					else
						Reporting.AddCapacityLine("= " + MinimumEffectiveFlangeTipLength2 + " >> bf / 2 + tf = " + (beam.Shape.bf / 2 + beam.Shape.tf) + ConstUnit.Length + " (NG)", MinimumEffectiveFlangeTipLength2 / (beam.Shape.bf / 2 + beam.Shape.tf), "Minimum Flange Weld Length to Resist the Tip Force", memberType);

					Reporting.AddHeader("Web Weld for Shear:");
					Mfws = CommonCalculations.MinimumWeld(momentEndPlate.Thickness, beam.Shape.tw);
					if (Mfws <= momentEndPlate.WebWeldRestSize)
						Reporting.AddLine("Minimum Weld = " + Mfws + " <= " + momentEndPlate.WebWeldRestSize + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Minimum Weld = " + Mfws + " >> " + momentEndPlate.WebWeldRestSize + ConstUnit.Length + " (NG)");

					AvailableLength = Math.Min(beam.Shape.d / 2 - beam.Shape.tf, beam.Shape.d - 1.5 * beam.Shape.tf - momentEndPlate.Bolt.SpacingTransvDir / 2 - 2 * momentEndPlate.Bolt.BoltSize);
					Reporting.AddHeader("Effective Length, Le = " + AvailableLength + ConstUnit.Length);
					Reporting.AddHeader("(See Above for Calculation)");
					FiRn = 2 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentEndPlate.WebWeldRestSize * AvailableLength;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx * w * Le");
					Reporting.AddLine("= 2 * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + momentEndPlate.WebWeldRestSize + " * " + AvailableLength);
					if (FiRn >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + FiRn + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRn, "Web Weld for Shear", memberType);
					else
						Reporting.AddCapacityLine("= " + FiRn + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRn, "Web Weld for Shear", memberType);

					wweldReq = ConstNum.FIOMEGA0_9N * beam.Material.Fy * beam.Shape.tw / (2 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddHeader("Required Weld Size to Develop Web Tensile Strength:");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " *Fy * tw / (2 * " + ConstString.FIOMEGA0_75 + " * 0.4242 * Fexx)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + beam.Material.Fy + " * " + beam.Shape.tw + " / (2 * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ") ");
					if (wweldReq <= momentEndPlate.WebWeldNearSize)
						Reporting.AddLine("= " + wweldReq + " <= " + momentEndPlate.WebWeldNearSize + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + wweldReq + " >> " + momentEndPlate.WebWeldNearSize + ConstUnit.Length + " (NG)");
					if (momentEndPlate.WebWeldNearSize > momentEndPlate.WebWeldRestSize)
					{
						Reporting.AddHeader("Required length of this weld:");
						WebWeldL = momentEndPlate.DistanceToFirstBolt + 2 * momentEndPlate.Bolt.BoltSize;
						if (WebWeldL <= momentEndPlate.WebWeldNearLength)
							Reporting.AddLine("= Pf + 2 * db = " + momentEndPlate.DistanceToFirstBolt + " + 2 * " + momentEndPlate.Bolt.BoltSize + " = " + WebWeldL + " <= " + momentEndPlate.WebWeldNearLength + ConstUnit.Length + " (OK)");
						else
							Reporting.AddLine("= Pf + 2 * db = " + momentEndPlate.DistanceToFirstBolt + " + 2 * " + momentEndPlate.Bolt.BoltSize + " = " + WebWeldL + " >> " + momentEndPlate.WebWeldNearLength + ConstUnit.Length + " (NG)");
					}
					if (!beam.EndSetback_User)
						beam.EndSetback = momentEndPlate.Thickness + momentEndPlate.ConnectionPlateThickness;
					break;
				default:
					if (momentEndPlate.Width < beam.Shape.bf + ConstNum.ONE_INCH)
						Reporting.AddHeader("End plate width should be " + ConstNum.ONE_INCH + ConstUnit.Length + " greater than beam flange width.");
					if (momentEndPlate.Width > beam.Shape.bf + ConstNum.ONE_INCH)
					{
						effwidth = beam.Shape.bf + ConstNum.ONE_INCH;
						Reporting.AddHeader("Effective width of end plate = " + effwidth + ConstUnit.Length);
					}
					else
						effwidth = momentEndPlate.Width;
					if (momentEndPlate.FlangeWeldType == EWeldType.Fillet)
						Reporting.AddHeader("Beam Flange Weld: " + CommonCalculations.WeldSize(momentEndPlate.FlangeWeldSize) + ConstUnit.Length);
					else
						Reporting.AddHeader("Beam Flange Weld: Full Pen. with " + CommonCalculations.WeldSize(momentEndPlate.FlangeWeldSize) + ConstUnit.Length + "  Reinforcement");
					Reporting.AddLine("Beam Web Weld: " + CommonCalculations.WeldSize(momentEndPlate.WebWeldRestSize) + ConstUnit.Length);
					if (momentEndPlate.WebWeldRestSize < momentEndPlate.WebWeldNearSize)
						Reporting.AddHeader("Beam Web Weld Near Tension Flange: " + CommonCalculations.WeldSize(momentEndPlate.WebWeldNearSize) + ConstUnit.Length + " X " + momentEndPlate.WebWeldNearLength + ConstUnit.Length);
					else
					{
						Reporting.AddHeader("Beam Web Weld Develops Tension Strength of Beam Web.");
					}
					Ff = beam.Moment / (beam.Shape.d - beam.Shape.tf) + Math.Abs(beam.P) / 2; // Axial Load included on 8/7/98
					Reporting.AddLine("");
					if (CommonDataStatic.Units == EUnit.US)
					{
						if (beam.P != 0)
						{
							Reporting.AddLine("Flange Force (Ff) = M / (d - tf) + P / 2");
							Reporting.AddLine("= " + beam.Moment + " / (" + beam.Shape.d + " - " + beam.Shape.tf + ") + " + Math.Abs(beam.P) + " / 2 = " + Ff + ConstUnit.Force);
						}
						else
						{
							Reporting.AddLine("Flange Force (Ff) = M / (d - tf)");
							Reporting.AddLine("= " + beam.Moment + " / (" + beam.Shape.d + " - " + beam.Shape.tf + ") = " + Ff + ConstUnit.Force);
						}
					}
					else
					{
						if (beam.P != 0)
						{
							Reporting.AddLine("Flange Force (Ff) = M / (d - tf) + P / 2");
							Reporting.AddLine("= " + beam.Moment + " / (" + beam.Shape.d + " - " + beam.Shape.tf + ") + " + Math.Abs(beam.P) + " / 2 = " + Ff + ConstUnit.Force);
						}
						else
						{
							Reporting.AddLine("Flange Force (Ff) = M / (d - tf)");
							Reporting.AddLine("= " + beam.Moment + " / (" + beam.Shape.d + " - " + beam.Shape.tf + ") = " + Ff + ConstUnit.Force);
						}
					}

					Reporting.AddGoToHeader("Check Bolts:");
					Abolt = 0.785398 * momentEndPlate.Bolt.BoltSize * momentEndPlate.Bolt.BoltSize;
					NBTOTAL = 2 * momentEndPlate.Bolt.NumberOfBolts + momentEndPlate.AdditionalBoltsForShear;
					V = beam.ShearForce / NBTOTAL * momentEndPlate.Bolt.NumberOfBolts;

					minedge = momentEndPlate.Bolt.MinEdgeSheared;
					if (momentEndPlate.Bolt.EdgeDistLongDir >= minedge)
						Reporting.AddLine("Bolt dist. to horiz. edge of plate = " + momentEndPlate.Bolt.EdgeDistLongDir + " >= " + minedge + " (OK)");
					else
					{
						Reporting.AddLine("Bolt dist. to horiz. edge of plate = " + momentEndPlate.Bolt.EdgeDistLongDir + " << " + minedge + " (NG)");
						Reporting.AddLine("(Edge distance may be OK if plate edge is gas cut or saw cut. Check!)");
					}
					transvedgedistance = (momentEndPlate.Width - beam.GageOnFlange) / 2;
					if (transvedgedistance >= minedge)
						Reporting.AddLine("Bolt dist. to vert. edge of plate = " + transvedgedistance + " >= " + minedge + " (OK)");
					else
					{
						Reporting.AddLine("Bolt dist. to vert. edge of plate = " + transvedgedistance + " << " + minedge + " (NG)");
						Reporting.AddLine("(Edge distance may be OK if plate edge is gas cut or saw cut. Check!)");
					}

					Reporting.AddHeader("Shear Force Carried by Tension Bolts for Moment (V): ");
					Reporting.AddLine("= (Vtotal / Ntotal) * NumberOfBoltsForMoment = (" + beam.ShearForce + " / " + NBTOTAL + ") * " + momentEndPlate.Bolt.NumberOfBolts);
					Reporting.AddLine("= " + V + ConstUnit.Force);
					if (momentEndPlate.Bolt.ASTMType != EBoltASTM.A325 && momentEndPlate.Bolt.NumberOfBolts > 4)
					{
						Reporting.AddLine("Four tension bolts are not adequate, and only");
						Reporting.AddLine("A325 Bolts are allowed with 8 tension bolt design. (NG)");
					}
					else
					{
						if (momentEndPlate.Bolt.NumberOfBolts == 4)
							C0 = 1;
						else
							C0 = 0.75;
						switch (momentEndPlate.Bolt.BoltType)
						{
							case EBoltType.SC:
								if (momentEndPlate.Bolt.ASTMType == EBoltASTM.A325)
									fta = ConstNum.FIOMEGA0_75N * ConstNum.COEFFICIENT_90 * Abolt;
								else if (momentEndPlate.Bolt.ASTMType == EBoltASTM.A490)
									fta = ConstNum.FIOMEGA0_75N * ConstNum.COEFFICIENT_113 * Abolt;
								else
									fta = ConstNum.FIOMEGA0_75N * 0.75 * momentEndPlate.Material.Fu * Abolt;

								TenCap = momentEndPlate.Bolt.NumberOfBolts * C0 * fta;
								Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Tension Strength = NumberOfBoltsForMoment * C0 * " + ConstString.PHI + " Rn = " + momentEndPlate.Bolt.NumberOfBolts + " * " + C0 + " * " + fta);
								if (TenCap >= Ff)
									Reporting.AddCapacityLine("= " + TenCap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / TenCap, "Tension Strength", memberType);
								else
									Reporting.AddCapacityLine("= " + TenCap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / TenCap, "Tension Strength", memberType);

								ShearCap = NBTOTAL * momentEndPlate.Bolt.BoltStrength;
								if (ShearCap >= beam.ShearForce)
									Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Shear Strength = Ntotal * Fv = " + NBTOTAL + " * " + momentEndPlate.Bolt.BoltStrength + " = " + ShearCap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)");
								else
									Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Shear Strength = Ntotal * Fv = " + NBTOTAL + " * " + momentEndPlate.Bolt.BoltStrength + " = " + ShearCap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)");
								break;
							default:
								fta = BoltsForTension.CalcBoltsForTension(memberType, momentEndPlate.Bolt, Ff, V, momentEndPlate.Bolt.NumberOfBolts, true);
								TenCap = momentEndPlate.Bolt.NumberOfBolts * C0 * fta;
								Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Tension Strength = NumberOfBoltsForMoment * C0 * " + ConstString.PHI + " Rn = " + momentEndPlate.Bolt.NumberOfBolts + " * " + C0 + " * " + fta);
								if (TenCap >= Ff)
									Reporting.AddCapacityLine("= " + TenCap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / TenCap, "Tension Strength", memberType);
								else
									Reporting.AddCapacityLine("= " + TenCap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / TenCap, "Tension Strength", memberType);

								if (momentEndPlate.Bolt.ASTMType == EBoltASTM.A325)
								{
									a = ConstNum.COEFFICIENT_117;
									TensionLimit = ConstNum.COEFFICIENT_90;
									if (momentEndPlate.Bolt.BoltType == EBoltType.N)
										B = 2.5 * blba;
									else
										B = 2 * blba;
								}
								else if (momentEndPlate.Bolt.ASTMType == EBoltASTM.A490)
								{
									a = ConstNum.COEFFICIENT_147;
									TensionLimit = ConstNum.COEFFICIENT_113;
									if (momentEndPlate.Bolt.BoltType == EBoltType.N)
										B = 2.5 * blba;
									else
										B = 2 * blba;
								}
								if (TenCap >= Ff)
								{
									Reporting.AddHeader("End Reaction Strength of Tension Bolts, Rt:");
									fvv = Math.Min((a - Math.Min(TensionLimit, Ff / (Abolt * momentEndPlate.Bolt.NumberOfBolts))) / B, momentEndPlate.Bolt.BoltStrength / (ConstNum.FIOMEGA0_75N * Abolt));

									Reporting.AddHeader("Nominal Strength, Fv = Min [(a - Min [TensionLimit, Ff / (Ab * Number Of Bolts For Moment)])/b, Fv0]");
									Reporting.AddLine("= Min [(" + a + " - Min [" + TensionLimit + ", " + Ff + " / (" + Abolt + " * " + momentEndPlate.Bolt.NumberOfBolts + ")]) / " + B + ", " + momentEndPlate.Bolt.BoltStrength + " / (" + ConstString.FIOMEGA0_75 + " * " + Abolt + ")]");
									Reporting.AddLine("= " + fvv + ConstUnit.Stress);
									shearbytensionbolts = ConstNum.FIOMEGA0_75N * Abolt * momentEndPlate.Bolt.NumberOfBolts * fvv;

									Reporting.AddLine("Rt = " + ConstString.FIOMEGA0_75 + "  * Ab * NumberOfBoltsForMoment * Fv");
									Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + "  * " + Abolt + " * " + momentEndPlate.Bolt.NumberOfBolts + " * " + fvv);
									Reporting.AddLine("= " + shearbytensionbolts + ConstUnit.Force);
								}
								else
								{
									Reporting.AddHeader("End Reaction Resisted by Tension Bolts (Rt) = 0");
									shearbytensionbolts = 0;
								}
								Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Shear Strength = (Ntotal - NumberOfBoltsForMoment) * Fv + Rt");
								ShearCap = (NBTOTAL - momentEndPlate.Bolt.NumberOfBolts) * momentEndPlate.Bolt.BoltStrength + shearbytensionbolts;
								Reporting.AddLine("= (" + NBTOTAL + " - " + momentEndPlate.Bolt.NumberOfBolts + ") * " + momentEndPlate.Bolt.BoltStrength + " + " + shearbytensionbolts);
								if (ShearCap >= beam.ShearForce)
									Reporting.AddCapacityLine("= " + ShearCap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / ShearCap, "Shear Strength", memberType);
								else
									Reporting.AddCapacityLine("= " + ShearCap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / ShearCap, "Shear Strength", memberType);
								break;
						}
					}

					Reporting.AddHeader("Plate Thickness:");
					switch (momentEndPlate.Bolt.NumberOfBolts)
					{
						case 4:
							Af = beam.Shape.tf * beam.Shape.bf;
							Reporting.AddLine("Beam Flange Area (Af) = tf * bf = " + beam.Shape.tf + " * " + beam.Shape.bf + " = " + Af + ConstUnit.Area);
							Aw = (beam.Shape.d - 2 * beam.Shape.tf) * beam.Shape.tw;
							Reporting.AddLine("Beam Web Area (Aw) = (d - 2 * tf) * tw = " + beam.Shape.d + " - 2 * " + beam.Shape.tf + ") * " + beam.Shape.tw + " = " + Aw + ConstUnit.Area);
							Favg = 0.5 * (beam.Material.Fy + momentEndPlate.Material.Fy);
							Fb = ConstNum.FIOMEGA0_75N * momentEndPlate.Material.Fy;

							switch (momentEndPlate.Bolt.ASTMType)
							{
								case EBoltASTM.A325:
									if (CommonDataStatic.Units == EUnit.US)
									{
										Fbu = 93;
										Fbt = 44;
									}
									else
									{
										Fbu = 620;
										Fbt = 310;
									}
									break;
								case EBoltASTM.A490:
									if (CommonDataStatic.Units == EUnit.US)
									{
										Fbu = 115;
										Fbt = 54;
									}
									else
									{
										Fbu = 780;
										Fbt = 390;
									}
									break;
							}

							Ca = 1.2 * (1.29 * Math.Pow(Favg / Fbu, 0.4) * Math.Pow(Fbt / Fb, 0.5));
							Reporting.AddLine("Ca = 1.2 * (1.29 * (Favg / Fbu)^0.4 * (Fbt / Fb)^0.5)");
							Reporting.AddLine("= 1.2 * (1.29 * (" + Favg + " / " + Fbu + ")^0.4 * (" + Fbt + " / " + Fb + ")^0.5)");
							Reporting.AddLine("= " + Ca);
							if (momentEndPlate.FlangeWeldType == EWeldType.CJP)
								wtfactor = 1;
							else
								wtfactor = 0.707;

							Pe = momentEndPlate.DistanceToFirstBolt - momentEndPlate.Bolt.BoltSize / 4 - wtfactor * momentEndPlate.FlangeWeldSize;
							Reporting.AddLine("Pe = Pf - db/4 - wt");
							Reporting.AddLine("= " + momentEndPlate.DistanceToFirstBolt + " - " + momentEndPlate.Bolt.BoltSize + "/4 - " + wtfactor * momentEndPlate.FlangeWeldSize);
							Reporting.AddLine("= " + Pe + ConstUnit.Length);

							AlfaM = Ca * Math.Sqrt(beam.Shape.bf / effwidth * Math.Sqrt(Pe / momentEndPlate.Bolt.BoltSize)) * Math.Pow(Af / Aw, 0.3333333);
							Reporting.AddHeader("Alfa = Ca * ((bf / bp) * (Pe / d)^0.5)^0.5 * (Af / Aw)^0.333");
							Reporting.AddLine("= " + Ca + " * ((" + beam.Shape.bf + " / " + effwidth + ") * (" + Pe + " / " + momentEndPlate.Bolt.BoltSize + ")^0.5)^0.5 * (" + Af + " / " + Aw + ")^0.333");
							Reporting.AddLine("= " + AlfaM);

							Meff = AlfaM * Ff * Pe / 4;
							Reporting.AddLine("Effective Plate Moment, Me:");
							Reporting.AddLine("= Alfa * Ff * Pe / 4 ");
							Reporting.AddLine("= " + AlfaM + " * " + Ff + " * " + Pe + " / 4 ");
							Reporting.AddLine("= " + Meff + ConstUnit.Moment);

							Reporting.AddLine("Required Plate Thickness for Bending:");
							tReq = Math.Sqrt(4 * Meff / (effwidth * ConstNum.FIOMEGA0_9N * momentEndPlate.Material.Fy));
							Reporting.AddLine("tb = (4 * Me / (bp * " + ConstString.FIOMEGA0_9 + "   * Fy))^0.5 ");
							Reporting.AddLine("= (4 * " + Meff + " / (" + effwidth + " * " + ConstString.FIOMEGA0_9 + " * " + momentEndPlate.Material.Fy + "))^0.5");
							if (tReq <= momentEndPlate.Thickness)
								Reporting.AddLine("= " + tReq + " <= " + momentEndPlate.Thickness + ConstUnit.Length + " (OK)");
							else
								Reporting.AddLine("= " + tReq + " >> " + momentEndPlate.Thickness + ConstUnit.Length + " (NG)");

							break;
						case 8:
							if (momentEndPlate.DistanceToFirstBolt <= ConstNum.TWOANDHALF_INCHES)
								Reporting.AddLine("Pf = " + momentEndPlate.DistanceToFirstBolt + " <= " + ConstNum.TWOANDHALF_INCHES + ConstUnit.Length + " (OK)");
							else
								Reporting.AddLine("Pf = " + momentEndPlate.DistanceToFirstBolt + " >> " + ConstNum.TWOANDHALF_INCHES + ConstUnit.Length + " (NG)");
							d = momentEndPlate.Bolt.BoltSize;
							if (momentEndPlate.Bolt.SpacingTransvDir >= ConstNum.EIGHT_THIRDS * d)
								Reporting.AddLine("Vertical Bolt Spacing = " + momentEndPlate.Bolt.SpacingTransvDir + "   >=  " + (ConstNum.EIGHT_THIRDS) + " * " + d + " = " + (ConstNum.EIGHT_THIRDS * d) + ConstUnit.Length + " (OK)");
							else
								Reporting.AddLine("Vertical Bolt Spacing = " + momentEndPlate.Bolt.SpacingTransvDir + "   <<  " + (ConstNum.EIGHT_THIRDS) + " * " + d + " = " + (ConstNum.EIGHT_THIRDS * d) + ConstUnit.Length + " (NG)");

							if (beam.GageOnFlange >= ConstNum.FIVEANDHALF_INCHES)
								Reporting.AddLine("Bolt Gage = " + beam.GageOnFlange + " >= " + ConstNum.FIVEANDHALF_INCHES + ConstUnit.Length + " (OK)");
							else
								Reporting.AddLine("Bolt Gage = " + beam.GageOnFlange + " << " + ConstNum.FIVEANDHALF_INCHES + ConstUnit.Length + " (NG)");

							if (beam.GageOnFlange <= ConstNum.SEVENANDHALF_INCHES)
								Reporting.AddLine("Bolt Gage = " + beam.GageOnFlange + " <= " + ConstNum.SEVENANDHALF_INCHES + ConstUnit.Length + " (OK)");
							else
								Reporting.AddLine("Bolt Gage = " + beam.GageOnFlange + " >> " + ConstNum.SEVENANDHALF_INCHES + ConstUnit.Length + " (NG)");

							Ru = Ff / 6;
							Reporting.AddHeader("Bolt Tension (ru) = Ff / 6 = " + Ff + " / 6 = " + t + ConstUnit.Force);
							Peff = momentEndPlate.DistanceToFirstBolt * Math.Sqrt(Math.Pow(beam.GageOnFlange, 2) + Math.Pow(momentEndPlate.DistanceToFirstBolt, 2)) / (4.17 * ConstNum.ONE_INCH);
							Reporting.AddLine("Peff = pf*(g² + pf²)^0.5/(4.17 * " + ConstNum.ONE_INCH + ")");
							Reporting.AddLine("= " + momentEndPlate.DistanceToFirstBolt + " * (" + beam.GageOnFlange + "² + " + momentEndPlate.DistanceToFirstBolt + "²)^0.5/" + 4.17 * ConstNum.ONE_INCH);
							Reporting.AddLine("= " + Peff + ConstUnit.Length);

							Meu = Ru * Peff;
							Reporting.AddHeader("Plate Moment (Meu) = ru * Peff ");
							Reporting.AddLine("= " + Ru + " * " + Peff + " = " + Meu + ConstUnit.Moment + "");
							tpa = Math.Sqrt(4 * Meu / (ConstNum.FIOMEGA0_9N * momentEndPlate.Material.Fy * effwidth));

							Reporting.AddHeader("Required Plate Thickness for Bending");
							Reporting.AddLine("tb = (4 * Ma / (" + ConstString.FIOMEGA0_9 + " * Fy * bp))^0.5");
							Reporting.AddLine("= (4 * " + Ma + " / (" + ConstString.FIOMEGA0_9 + " * " + momentEndPlate.Material.Fy + " * " + effwidth + "))^0.5 ");
							if (tpa <= momentEndPlate.Thickness)
								Reporting.AddLine("= " + tpa + " <= " + momentEndPlate.Thickness + ConstUnit.Length + " (OK)");
							else
								Reporting.AddLine("= " + tpa + " >> " + momentEndPlate.Thickness + ConstUnit.Length + " (NG)");
							break;
						case 16:
							break;
					}
					Reporting.AddHeader("Required Plate Thickness for Shear:");
					treqForShear = Ff / (2 * ConstNum.FIOMEGA1_0N * 0.6 * momentEndPlate.Material.Fy * effwidth);
					Reporting.AddLine("ts = Ff / (2 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * bp * Fy)");
					Reporting.AddLine("= " + Ff + " / (2*" + ConstNum.FIOMEGA1_0N + " * 0.6 * " + effwidth + " * " + momentEndPlate.Material.Fy + ")");
					if (treqForShear <= momentEndPlate.Thickness)
						Reporting.AddLine("= " + treqForShear + " <= " + momentEndPlate.Thickness + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + treqForShear + " >> " + momentEndPlate.Thickness + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Beam Flange to Plate Weld: ");
					if (momentEndPlate.FlangeWeldType == EWeldType.Fillet)
					{
						// MomentEndPlate.FlangeWeldSize = -Int(-Ff / (0.01326 * Fexx * (2 * (Beam.BF + Beam.tf) - Beam.tw))) / 16
						minfweld = CommonCalculations.MinimumWeld(momentEndPlate.Thickness, beam.Shape.tf);
						if (minfweld <= momentEndPlate.FlangeWeldSize)
							Reporting.AddLine("Minimum Fillet Weld Size = " + CommonCalculations.WeldSize(minfweld) + " <= " + CommonCalculations.WeldSize(momentEndPlate.FlangeWeldSize) + ConstUnit.Length + " (OK)");
						else
							Reporting.AddLine("Minimum Fillet Weld Size = " + CommonCalculations.WeldSize(minfweld) + " >> " + CommonCalculations.WeldSize(momentEndPlate.FlangeWeldSize) + ConstUnit.Length + " (NG)");

						fweldReq = Ff / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * (2 * (beam.Shape.bf + beam.Shape.tf) - beam.Shape.tw));
						Reporting.AddLine("Required Fillet Weld Size = Ff / (" + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx * (2 * (bf + tf)-tw))");
						Reporting.AddLine("= " + Ff + " / (" + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * (2 * (" + beam.Shape.bf + " + " + beam.Shape.tf + ") - " + beam.Shape.tw + ")) ");
						if (fweldReq <= momentEndPlate.FlangeWeldSize)
							Reporting.AddLine("= " + fweldReq + " <= " + momentEndPlate.FlangeWeldSize + ConstUnit.Length + " (OK)");
						else
							Reporting.AddLine("= " + fweldReq + " >> " + momentEndPlate.FlangeWeldSize + ConstUnit.Length + " (NG)");
					}
					else
					{
						fweldReq = Ff / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * (2 * (beam.Shape.bf + beam.Shape.tf) - beam.Shape.tw));
						Reporting.AddHeader("Required Fillet Weld Size = Ff  /(" + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx * (2 * (bf + tf) - tw))");
						Reporting.AddLine("= " + Ff + " / (" + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * (2 * (" + beam.Shape.bf + " + " + beam.Shape.tf + ") - " + beam.Shape.tw + ")) ");
						if (fweldReq > ConstNum.HALF_INCH)
							Reporting.AddLine("= " + fweldReq + "  >  " + ConstNum.HALF_INCH + ConstUnit.Length + " Use CJP weld with reinforcement.");

						Reporting.AddHeader("CJP Weld w/Reinforcement");
						wldsize = Math.Min(0.25 * beam.Shape.tf, ConstNum.THREE_EIGHTS_INCH);
						// reinfsize = weldsize(wldsize)
						if (wldsize <= momentEndPlate.FlangeWeldSize)
							Reporting.AddLine("Minimum Reinforcement Size = " + CommonCalculations.WeldSize(wldsize) + " <= " + CommonCalculations.WeldSize(momentEndPlate.FlangeWeldSize) + ConstUnit.Length + " (OK)");
						else
							Reporting.AddLine("Minimum Reinforcement Size = " + CommonCalculations.WeldSize(wldsize) + " >> " + CommonCalculations.WeldSize(momentEndPlate.FlangeWeldSize) + ConstUnit.Length + " (NG)");

						Fym = Math.Min(momentEndPlate.Material.Fy, beam.Material.Fy);
						B = Math.Min(beam.Shape.bf, momentEndPlate.Width);
						capacity = ConstNum.FIOMEGA0_9N * Fym * B * beam.Shape.tf;
						Reporting.AddLine("CJP Weld Strength = " + ConstString.FIOMEGA0_9 + " * Fy * b * t");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + "   * " + Fym + " * " + B + " * " + beam.Shape.tf);
						if (capacity >= Ff)
							Reporting.AddLine("= " + capacity + " >= " + Ff + ConstUnit.Force + " (OK)");
						else
						{
							Reporting.AddLine("= " + capacity + " << " + Ff + ConstUnit.Force);
							Reporting.AddLine("CJP weld is assumed to develop beam moment strength. (OK)");
						}
					}

					Reporting.AddGoToHeader("Beam Web to Plate Weld: ");
					minfweld = CommonCalculations.MinimumWeld(momentEndPlate.Thickness, beam.Shape.tw);

					if (minfweld <= momentEndPlate.WebWeldRestSize)
						Reporting.AddLine("Minimum Fillet Weld Size = " + CommonCalculations.WeldSize(minfweld) + " <= " + CommonCalculations.WeldSize(momentEndPlate.WebWeldRestSize) + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Minimum Fillet Weld Size = " + CommonCalculations.WeldSize(minfweld) + " >> " + CommonCalculations.WeldSize(momentEndPlate.WebWeldRestSize) + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Required Weld Size for End Shear:");
					ShearWeldL = Math.Min(beam.Shape.d / 2 - beam.Shape.tf, beam.Shape.d - 2 * beam.Shape.tf - momentEndPlate.DistanceToFirstBolt - 2 * momentEndPlate.Bolt.BoltSize - (momentEndPlate.Bolt.NumberOfBolts - 4) / 4.0 * momentEndPlate.Bolt.SpacingTransvDir);

					Reporting.AddHeader("Effective Length of Weld (L)");
					if (momentEndPlate.Bolt.NumberOfBolts == 4)
					{
						Reporting.AddLine("= Min((d / 2 - tf); (d - 2 * tf - 2 * db - pf))");
						Reporting.AddLine("= Min((" + beam.Shape.d + " / 2 - " + beam.Shape.tf + "), ( " + beam.Shape.d + " - 2 * " + beam.Shape.tf + " - 2 * " + momentEndPlate.Bolt.BoltSize + " - " + momentEndPlate.DistanceToFirstBolt + "))");
					}
					else
					{
						Reporting.AddLine("= Min((d / 2 - tf); (d - 2 * tf - 2 * db - pf - sv))");
						Reporting.AddLine("= Min((" + beam.Shape.d + " / 2 - " + beam.Shape.tf + "), ( " + beam.Shape.d + " - 2 * " + beam.Shape.tf + " - 2 * " + momentEndPlate.Bolt.BoltSize + " - " + momentEndPlate.DistanceToFirstBolt + " - " + momentEndPlate.Bolt.SpacingTransvDir + "))");
					}
					Reporting.AddLine("= " + ShearWeldL + ConstUnit.Length);
					WebWeld = beam.ShearForce / (2 * ShearWeldL * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddLine("Weld Size = V / (2 * L * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx)");
					Reporting.AddLine("= " + beam.ShearForce + " / (2 * " + ShearWeldL + " * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
					if (WebWeld <= momentEndPlate.WebWeldRestSize)
						Reporting.AddLine("= " + WebWeld + " <= " + momentEndPlate.WebWeldRestSize + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + WebWeld + " >> " + momentEndPlate.WebWeldRestSize + ConstUnit.Length + " (NG)");
					wweldReq = ConstNum.FIOMEGA0_9N * beam.Material.Fy * beam.Shape.tw / (2 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);

					Reporting.AddHeader("Required Weld Size to Develop Web Tensile Strength:");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * Fy * tw / (2 * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + beam.Material.Fy + " * " + beam.Shape.tw + " / (2 * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ") ");
					if (wweldReq <= momentEndPlate.WebWeldNearSize)
						Reporting.AddLine("= " + wweldReq + " <= " + momentEndPlate.WebWeldNearSize + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + wweldReq + " >> " + momentEndPlate.WebWeldNearSize + ConstUnit.Length + " (NG)");
					if (momentEndPlate.WebWeldNearSize > momentEndPlate.WebWeldRestSize)
					{
						Reporting.AddHeader("Required length of this weld:");
						if (momentEndPlate.Bolt.NumberOfBolts == 4)
						{
							WebWeldL = momentEndPlate.DistanceToFirstBolt + 2 * momentEndPlate.Bolt.BoltSize;
							if (WebWeldL <= momentEndPlate.WebWeldNearLength)
								Reporting.AddLine("= Pf + 2 * db = " + momentEndPlate.DistanceToFirstBolt + " + 2 * " + momentEndPlate.Bolt.BoltSize + " = " + WebWeldL + " <= " + momentEndPlate.WebWeldNearLength + ConstUnit.Length + " (OK)");
							else
								Reporting.AddLine("= Pf + 2 * db = " + momentEndPlate.DistanceToFirstBolt + " + 2 * " + momentEndPlate.Bolt.BoltSize + " = " + WebWeldL + " >> " + momentEndPlate.WebWeldNearLength + ConstUnit.Length + " (NG)");
						}
						else
						{
							WebWeldL = momentEndPlate.DistanceToFirstBolt + momentEndPlate.Bolt.SpacingTransvDir / 2 + 2 * momentEndPlate.Bolt.BoltSize;
							if (WebWeldL <= momentEndPlate.WebWeldNearLength)
								Reporting.AddLine("= Pf + 2 * db + sv / 2 = " + momentEndPlate.DistanceToFirstBolt + " + 2 * " + momentEndPlate.Bolt.BoltSize + " + " + momentEndPlate.Bolt.SpacingTransvDir + "/2 = " + WebWeldL + " <= " + momentEndPlate.WebWeldNearLength + ConstUnit.Length + " (OK)");
							else
								Reporting.AddLine("= Pf + 2 * db + sv / 2 = " + momentEndPlate.DistanceToFirstBolt + " + 2 * " + momentEndPlate.Bolt.BoltSize + " + " + momentEndPlate.Bolt.SpacingTransvDir + "/2 = " + WebWeldL + " >> " + momentEndPlate.WebWeldNearLength + ConstUnit.Length + " (NG)");
						}
					}
					if (momentEndPlate.Bolt.NumberOfBolts > 4)
					{
						momentEndPlate.BraceStiffener.Thickness = NumberFun.Round(beam.Shape.tw, 8);
						if (!momentEndPlate.Length_User)
							momentEndPlate.Length = beam.Shape.d + (momentEndPlate.DistanceToFirstBolt + momentEndPlate.Bolt.SpacingTransvDir + momentEndPlate.Bolt.EdgeDistLongDir) * 2;
						Reporting.AddHeader("Stiffeners required between beam flanges and end plate.");
						Reporting.AddLine("Stiffener Material: " + momentEndPlate.Material.Name);
						Reporting.AddLine("Stiffener Thickness = " + momentEndPlate.BraceStiffener.Thickness + ConstUnit.Length);
						Reporting.AddLine("Stiffener Length = " + momentEndPlate.BraceStiffener.Length + " X " + (2 * momentEndPlate.BraceStiffener.Length) + ConstUnit.Length);
						Reporting.AddLine("Stiffener Weld = CJP double-bevel groove");
						wreq = NumberFun.Round(momentEndPlate.Material.Fu * momentEndPlate.BraceStiffener.Thickness / (1.41 * CommonDataStatic.Preferences.DefaultElectrode.Fexx), 16);
						Reporting.AddLine("or " + CommonCalculations.WeldSize(wreq) + ConstUnit.Length + " Double fillet welds.");
					}
					break;
			}
		}
	}
}