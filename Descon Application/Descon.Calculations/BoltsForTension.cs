using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class BoltsForTension
	{
		internal static double CalcBoltsForTension(EMemberType memberType, Bolt bolt, double t, double V, double N, bool enableReporting)
		{
			double Rn = 0;
			double ShearStress;
			double bt3;
			double bt0;
			double fvv = 0;
			double b = 0;
			double a = 0;
			double bt = 0;
			double bt2;
			double bt1;
			double Ft = 0;
			double ClipAngleMax = 0;
			double ClipAngleMin = 0;
			double blba;
			double Ab;
			double Ta;
			double Va;

			// if n=0 returns number of bolts
			// if n>0 returns Allowable tension per bolt
			// if n<0 returns user defined number of bolts =-n

			var component = CommonDataStatic.DetailDataDict[memberType];

			Va = Math.Abs(V);
			Ta = Math.Abs(t);
			Ab = Math.Pow(bolt.BoltSize, 2) * Math.PI / 4;

			blba = CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD ? 1.5 : 1;
			
			// This section is here for Brace mode and shouldn't affect Win mode calls
			switch (memberType)
			{
				case EMemberType.RightBeam:
				case EMemberType.LeftBeam:
					ClipAngleMin = component.Shape.d / 2;
					ClipAngleMax = component.Shape.t + SmallMethodsDesign.Encroachment(memberType);
					break;
				case EMemberType.UpperRight:
				case EMemberType.LowerRight:
				case EMemberType.UpperLeft:
				case EMemberType.LowerLeft:
					ClipAngleMin = component.BraceConnect.Gusset.VerticalForceColumn / 2;
					ClipAngleMax = component.BraceConnect.Gusset.VerticalForceColumn - ConstNum.TWO_INCHES;
					break;
			}

			component.WinConnect.ShearClipAngle.MinBolts = ((int)Math.Ceiling((ClipAngleMin - 2 * bolt.EdgeDistLongDir) / bolt.SpacingLongDir)) + 1;
			if (component.WinConnect.ShearClipAngle.MinBolts < 2)
				component.WinConnect.ShearClipAngle.MinBolts = 2;
						
			switch (CommonDataStatic.Units)
			{
				case EUnit.US:
					if (bolt.ASTMType == EBoltASTM.A325)
						Ft = 90;
					else if (bolt.ASTMType == EBoltASTM.A490)
						Ft = 113;
					break;
				case EUnit.Metric:
					if (bolt.ASTMType == EBoltASTM.A325)
						Ft = 620;
					else if (bolt.ASTMType == EBoltASTM.A490)
						Ft = 780;
					else
						Ft = 0.75 * component.Material.Fu;
					break;
			}

			if (bolt.BoltType == EBoltType.SC)
			{
				if (N <= 0)
				{
					bt1 = Va / bolt.BoltStrength + Ta / (1.13 * bolt.Pretension);
					bt2 = Ta / (ConstNum.FIOMEGA0_75N * Ft * Ab);
					bt = Math.Ceiling(Math.Max(bt1, bt2));
					if (bt < 2)
						bt = 2;
					if (N < 0)
						bt = -N;
				}
				else
				{
					switch (CommonDataStatic.Units)
					{
						case EUnit.US:
							if (bolt.ASTMType == EBoltASTM.A325)
								a = ConstNum.FIOMEGA0_75N * 90 * Ab;
							else if (bolt.ASTMType == EBoltASTM.A490)
								a = ConstNum.FIOMEGA0_75N * 113 * Ab;
							break;
						case EUnit.Metric:
							if (bolt.ASTMType == EBoltASTM.A325)
								a = ConstNum.FIOMEGA0_75N * 620 * Ab;
							else if (bolt.ASTMType == EBoltASTM.A490)
								a = ConstNum.FIOMEGA0_75N * 780 * Ab;
							else
								a = ConstNum.FIOMEGA0_75N * 0.75 * component.Material.Fu * Ab;
							break;
					}
					b = 1.13 * bolt.Pretension * (1 - Va / (N * bolt.BoltStrength));
					if (b < 0)
						b = 0;
					bt = Math.Min(a, b);
				}
				if (N == 0)
				{
					bt1 = Va / bolt.BoltStrength + Ta / (1.13 * bolt.Pretension);
					bt2 = Ta / (ConstNum.FIOMEGA0_75N * Ft * Ab);
					bt = (int)Math.Ceiling(Math.Max(bt1, bt2));
					if (bt < 2)
						bt = 2;
					if (bt1 >= bt2 && enableReporting)
					{
						Reporting.AddHeader("Number of Bolts Required = " + bt);
						Reporting.AddLine("= V / FIRstr + T / (1.13 * Pretension)");
						Reporting.AddLine("= " + Va + " / " + bolt.BoltStrength + " + " + Ta + " / (1.13 * " + bolt.Pretension + ")");
						Reporting.AddLine("= " + bt1);
					}
					else if(enableReporting)
					{
						Reporting.AddHeader("Number of Bolts Required = " + bt + " (Tension without shear controls)");
						Reporting.AddLine(" = T / " + ConstString.FIOMEGA0_75 + " * Ft * Ab)");
						Reporting.AddLine(" = " + t + " / (" + ConstString.FIOMEGA0_75 + " * " + Ft + " * " + Ab);
					}
				}
				else if (N < 0)
				{
					bt = -N;
					if (enableReporting)
						Reporting.AddHeader(String.Format("Number of Bolts Provided = {0}", bt));
				}
				else
				{
					b = 1.13 * bolt.Pretension * (1 - Va / (N * bolt.BoltStrength));
					if (b < 0)
						b = 0;
					bt = Math.Min(a, b);
					a = component.Material.Fu * ConstNum.FIOMEGA0_75N * Ab;

					if (enableReporting)
					{
						Reporting.AddLine("Nominal Tension Strength per Bolt (Rn)= " + Rn);
						Reporting.AddLine("= 1.13 * Pretension * (1 - V / (n * FIRstr))");
						Reporting.AddLine("= 1.13 * " + bolt.Pretension + " * (1 - " + Va + " / (" + N + " * " + bolt.BoltStrength + "))");

						if (b <= a)
							Reporting.AddLine("= " + b + ConstUnit.Force);
						else
							Reporting.AddLine("= " + b + " >> " + a + ", Use " + a + ConstUnit.Force);
					}
				}
			}
			else
			{
				switch (CommonDataStatic.Units)
				{
					case EUnit.US:
						switch (bolt.ASTMType)
						{
							case EBoltASTM.A325:
								a = 117;
								if (bolt.BoltType == EBoltType.N)
								{
									b = 2.5 * blba;
									fvv = CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14 ? 54 : 48;
								}
								else
								{
									b = 2 * blba;
									fvv = CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14 ? 68 : 60;
								}
								break;
							case EBoltASTM.A490:
								a = 147;
								if (bolt.BoltType == EBoltType.N)
								{
									b = 2.5 * blba;
									fvv = CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14 ? 68 : 60;
								}
								else
								{
									b = 2 * blba;
									fvv = CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14 ? 84 : 75;
								}
								break;
						}
						break;
					case EUnit.Metric:
						switch (bolt.ASTMType)
						{
							case EBoltASTM.A325:
								a = 807;
								if (bolt.BoltType == EBoltType.N)
								{
									b = 2.5 * blba;
									fvv = CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14 ? 372 : 330;
								}
								else
								{
									b = 2 * blba;
									fvv = CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14 ? 457 : 414;
								}
								break;
							case EBoltASTM.A490:
								a = 1010;
								if (bolt.BoltType == EBoltType.N)
								{
									b = 2.5 * blba;
									fvv = CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14 ? 457 : 414;
								}
								else
								{
									b = 2 * blba;
									fvv = CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14 ? 579 : 520;
								}
								break;
							default:
								a = 1.25 * component.Material.Fu;
								if (bolt.BoltType == EBoltType.N)
								{
									b = 2.5 * blba;
									fvv = CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14 ? 0.45 * component.Material.Fu : 0.4 * component.Material.Fu;
								}
								else
								{
									b = 2 * blba;
									fvv = CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14 ? 0.563 * component.Material.Fu : 0.5 * component.Material.Fu;
								}
								break;
						}
						break;
				}


				Ab = Math.Pow(bolt.BoltSize, 2) * Math.PI / 4;
				if (N <= 0)
				{
					bt0 = (Ta / ConstNum.FIOMEGA0_75N + b * Va) / (a * Ab); // interaction
					bt2 = Va / (ConstNum.FIOMEGA0_75N * fvv * Ab); // Shear Only
					bt3 = Ta / (ConstNum.FIOMEGA0_75N * Ft * Ab); // Tension only
					bt1 = Math.Max(bt0, (Math.Max(bt2, bt3)));
					bt = (int) Math.Ceiling(bt1);
					if (bt < 2)
						bt = 2;
					if (N < 0)
						bt = -N;

					if (enableReporting)
					{
						Reporting.AddLine(String.Format("Number of Bolts Required (bt)= " + bt1, bt));
						if (bt1 == bt2)
						{
							Reporting.AddLine("= V / ( " + ConstString.FIOMEGA0_75 + " * Fv * Ab)");
							Reporting.AddLine("= " + V + " / (" + ConstString.FIOMEGA0_75 + " * " + fvv + " * " + Ab + ")");
						}
						else if (bt1 == bt3)
						{
							Reporting.AddLine("= T / (" + ConstString.FIOMEGA0_75 + " * Ft * Ab)");
							Reporting.AddLine("= " + t + " / (" + ConstString.FIOMEGA0_75 + " * " + Ft + " * " + Ab + ")");
						}
						else
						{
							Reporting.AddLine("= (T / " + ConstString.FIOMEGA0_75 + " + b * V) / (a * Ab)");
							Reporting.AddLine("= (" + t + " / " + ConstString.FIOMEGA0_75 + " + " + b + " * " + V + ") / (" + a + " * " + Ab + ")");
						}

						Reporting.AddLine("= " + bt1 + ", Use " + -N + " Bolts"); // User Defined
					}
				}
				else
				{
                    ShearStress = Va / (N * Ab);

                    if (ShearStress < (a - Ft) / b) Rn = Ft * Ab;
                    else if (ShearStress > ConstNum.FIOMEGA0_75N * fvv) Rn = 0;
                    else Rn = Math.Min(a - b * Va / (N * Ab), Ft) * Ab;
                    
                    bt = ConstNum.FIOMEGA0_75N * Rn;

					if (enableReporting)
					{
                        // For some reason the reporting in this section was vastly different than in v7.
                        // Fixed for Bug 163 -RM
                        Reporting.AddHeader("Nominal Strength per Bolt = rn");

                        if (ShearStress < (a - Ft) / b)
							Reporting.AddLine(" = Ft * Ab = " + Ft + " * " + Ab + " = " + Rn + ConstUnit.Force);
						else if (ShearStress > ConstNum.FIOMEGA0_75N * fvv)
							Reporting.AddLine(" = 0 (Bolts fail in shear (NG))");
						else
						{
                            Reporting.AddLine(" = (a - b * V / (N * Ab)) * Ab <= Ft * Ab");
                            Reporting.AddLine(String.Format(" = (" + a + " - " + b + " * " + Va + " / (" + N + " * " + Ab + ")) * " + Ab + " <= " + Ft + " * " + Ab, a, b, Va, N, Ab, Ft));
                            Reporting.AddLine(" = " + Rn + ConstUnit.Force);
						}
                        Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " per Bolt, " + ConstString.PHI + "rn = " + ConstString.FIOMEGA0_75 + " * rn = " + bt + ConstUnit.Force);
				    }
				}
			}

			return CommonDataStatic.Units == EUnit.Metric ? bt / 1000 : bt;
		}
	}
}