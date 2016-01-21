//using System;
//using System.Collections.Generic;
//using Descon.Data;
//using Descon.UI.DataAccess;

//namespace Descon.Calculations
//{
//	public class Beam
//	{
//		public static void BeamWebYieldingAndCripling(EMemberType memberType)
//		{
//			double Rcap = 0;
//			double R = 0;
//			double Moment = 0;
//			double eLeft = 0;
//			double xLeft2 = 0;
//			double eRight = 0;
//			double xRight2 = 0;
//			double dumy = 0;
//			double L = 0;
//			double ortasi = 0;
//			double V = 0;
//			double H = 0;
//			double RcapBot = 0;
//			double RcapTop = 0;
//			int k = 0;
//			double Nbot = 0;
//			double Ntop = 0;
//			double tf = 0;
//			double tw = 0;
//			double thWebYieldingBot = 0;
//			double Rbot = 0;
//			double thWebYieldingTop = 0;
//			double Rtop = 0;
//			double Mi2Max = 0;
//			double Hi2Max = 0;
//			double Vi2Comp = 0;
//			double Vi2Max = 0;
//			double Mi1Max = 0;
//			double Hi1Max = 0;
//			double Vi1Comp = 0;
//			double Vi1Max = 0;
//			double GapB = 0;
//			double GapT = 0;

//			Reporting.AddDebugLine("BeamWebYieldingAndCripling",
//				new List<string> {"memberType = " + MiscMethods.GetComponentName(memberType)});

//			// Set the components to be used as the beam and braces for the rest of the calcs.
//			DetailData beam;
//			DetailData upperBrace;
//			DetailData lowerBrace;
//			DetailData component = CommonDataStatic.DetailDataDict[memberType];

//			if (memberType == EMemberType.LowerLeft || memberType == EMemberType.UpperLeft)
//			{
//				beam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
//				upperBrace = CommonDataStatic.DetailDataDict[EMemberType.UpperLeft];
//				lowerBrace = CommonDataStatic.DetailDataDict[EMemberType.LowerLeft];
//			}
//			else
//			{
//				beam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
//				upperBrace = CommonDataStatic.DetailDataDict[EMemberType.UpperRight];
//				lowerBrace = CommonDataStatic.DetailDataDict[EMemberType.LowerRight];
//			}

//			// Eq. K1-1 and K1-2 for Yielding and K1-4 or K1-5 for Crippling
//			if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BraceVToBeam)
//			{
//				if (upperBrace.BraceConnect.Gusset.ColumnSideSetback > beam.EndSetback + beam.WinConnect.Beam.TCopeL)
//					GapT = 0;
//				else
//					GapT = beam.EndSetback + beam.WinConnect.Beam.TCopeL - upperBrace.BraceConnect.Gusset.ColumnSideSetback;
//				if (lowerBrace.BraceConnect.Gusset.ColumnSideSetback > beam.EndSetback + beam.WinConnect.Beam.TCopeL)
//					GapB = 0;
//				else
//					GapT = beam.EndSetback + beam.WinConnect.Beam.TCopeL - lowerBrace.BraceConnect.Gusset.ColumnSideSetback;

//				Vi1Max = Math.Max(upperBrace.BraceConnect.Gusset.GussetEFTension.Vb, upperBrace.BraceConnect.Gusset.GussetEFCompression.Vb);
//				Vi1Comp = upperBrace.BraceConnect.Gusset.GussetEFCompression.Vb;
//				Hi1Max = Math.Max(upperBrace.BraceConnect.Gusset.GussetEFTension.Hb, upperBrace.BraceConnect.Gusset.GussetEFCompression.Hb);
//				Mi1Max = Math.Max(upperBrace.BraceConnect.Gusset.GussetEFTension.Mb, upperBrace.BraceConnect.Gusset.GussetEFCompression.Mb);

//				Vi2Max = Math.Max(lowerBrace.BraceConnect.Gusset.GussetEFTension.Vb, lowerBrace.BraceConnect.Gusset.GussetEFCompression.Vb);
//				Vi2Comp = lowerBrace.BraceConnect.Gusset.GussetEFCompression.Vb;
//				Hi2Max = Math.Max(lowerBrace.BraceConnect.Gusset.GussetEFTension.Hb, lowerBrace.BraceConnect.Gusset.GussetEFCompression.Hb);
//				Mi2Max = Math.Max(lowerBrace.BraceConnect.Gusset.GussetEFTension.Mb, lowerBrace.BraceConnect.Gusset.GussetEFCompression.Mb);

//				if (upperBrace.IsActive)
//				{
//					Rtop = Math.Sqrt(Math.Pow(1.73 * Hi1Max, 2) + Math.Pow(Math.Abs(Vi1Max) + 3 * Math.Abs(Mi1Max) / (upperBrace.BraceConnect.Gusset.Hb - GapT), 2));
//					thWebYieldingTop = Rtop / (ConstNum.FIOMEGA1_0N * beam.Material.Fy * (upperBrace.BraceConnect.Gusset.Hb - GapT) + 2.5 * beam.Shape.kdes);
//				}
//				else
//				{
//					Rtop = 0;
//					thWebYieldingTop = 0;
//				}
//				if (lowerBrace.IsActive)
//				{
//					Rbot = Math.Sqrt(Math.Pow(1.73 * Hi2Max, 2) + Math.Pow(Math.Abs(Vi2Max) + 3 * Math.Abs(Mi2Max) / (lowerBrace.BraceConnect.Gusset.Hb - GapB), 2));
//					thWebYieldingBot = Rbot / (ConstNum.FIOMEGA1_0N * beam.Material.Fy * (lowerBrace.BraceConnect.Gusset.Hb - GapB) + 2.5 * beam.Shape.kdes);
//				}
//				else
//				{
//					Rbot = 0;
//					thWebYieldingBot = 0;
//				}

//				if (Double.IsNaN(Rbot))
//					Rbot = 0;
//				if (Double.IsNaN(Rtop))
//					Rtop = 0;
//				if (Double.IsNaN(thWebYieldingTop))
//					thWebYieldingTop = 0;
//				if (Double.IsNaN(thWebYieldingBot))
//					thWebYieldingBot = 0;

//				if (upperBrace.IsActive)
//				{
//					Reporting.AddHeader("Beam Web Local Yielding:");
//					Reporting.AddHeader("Force from Top (Rtop) = ((1.73 * HbTop)² + (VbTop + 3 * MbTop / Ltop)²)^0.5");
//					Reporting.AddLine("= ((1.73 * " + Hi1Max + ")² + (" + Math.Abs(Vi1Max) + " + 3 * " + Math.Abs(Mi1Max) + " / " + (upperBrace.BraceConnect.Gusset.Hb - GapT) + ")²)^0.5");
//					Reporting.AddLine("= " + Rtop + ConstUnit.Force);

//					Reporting.AddHeader("Required Web Thickness = Rtop / ( Fy * (L + 2.5 * k))");
//					Reporting.AddLine("= " + Rtop + " / (" + beam.Material.Fy + " * (" + (upperBrace.BraceConnect.Gusset.Hb - GapT) + " + 2.5 * " + beam.Shape.kdes + "))");
//					Reporting.AddLine("= " + thWebYieldingTop + ConstUnit.Length);
//					if (thWebYieldingTop <= beam.Shape.tw)
//						Reporting.AddLine(thWebYieldingTop + " <= " + beam.Shape.tw + ConstUnit.Length + " (OK)");
//					else
//						Reporting.AddLine(thWebYieldingTop + " >> " + beam.Shape.tw + ConstUnit.Length + " (NG)");
//				}
//				if (lowerBrace.IsActive)
//				{
//					Reporting.AddHeader("Beam Web Local Yielding:");
//					Reporting.AddHeader("Force from Bottom, Rbot = ((1.73 * HbBot)² + (VbBot + 3 * MbBot / LBot)²)^0.5");
//					Reporting.AddLine("= ((1.73 * " + Math.Abs(Hi2Max) + ")² + (" + Math.Abs(Vi2Max) + " + 3 * " + Math.Abs(Mi2Max) + " / " + (lowerBrace.BraceConnect.Gusset.Hb - GapB) + ")²)^0.5");
//					Reporting.AddLine("= " + Rbot + ConstUnit.Force);

//					Reporting.AddHeader("Required Web Thickness = Rbot / (" + ConstNum.FIOMEGA1_0N + " * Fy * (L + 2.5 * k))");
//					Reporting.AddLine("= " + Rbot + " / (" + ConstNum.FIOMEGA1_0N + " * " + beam.Material.Fy + " * (" + (lowerBrace.BraceConnect.Gusset.Hb - GapB) + " + 2.5 * " + beam.Shape.kdes + "))");
//					Reporting.AddLine("= " + thWebYieldingBot + ConstUnit.Length);
//					if (thWebYieldingBot <= beam.Shape.tw)
//						Reporting.AddLine(thWebYieldingBot + " <= " + beam.Shape.tw + ConstUnit.Length + " (OK)");
//					else
//						Reporting.AddLine(thWebYieldingBot + " >> " + beam.Shape.tw + ConstUnit.Length + " (NG)");
//				}

//				// web crippling
//				if (upperBrace.IsActive)
//					Rtop = Math.Abs(Vi1Comp) + 3 * Math.Abs(Mi1Max / (upperBrace.BraceConnect.Gusset.Hb - GapT));
//				else
//					Rtop = 0;
//				if (lowerBrace.IsActive)
//					Rbot = Math.Abs(Vi2Comp) + 3 * Math.Abs(Mi2Max / (lowerBrace.BraceConnect.Gusset.Hb - GapB));
//				else
//					Rbot = 0;
//				if (double.IsNaN(Rtop))
//					Rtop = 0;
//				if (double.IsNaN(Rbot))
//					Rbot = 0;

//				tw = beam.Shape.tw;
//				tf = beam.Shape.tf;
//				Ntop = (upperBrace.BraceConnect.Gusset.Hb - GapT) / beam.Shape.d;
//				Nbot = (lowerBrace.BraceConnect.Gusset.Hb - GapB) / beam.Shape.d;
//				k = 0;
//				do
//				{
//					k++;
//					if (Ntop <= 0.2)
//						RcapTop = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(ConstNum.ELASTICITY, 0.5) * Math.Pow(tw, 2) * (1 + 3 * Ntop * Math.Pow(tw / tf, 1.5)) * Math.Sqrt(beam.Material.Fy * tf / tw);
//					else
//						RcapTop = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(ConstNum.ELASTICITY, 0.5) * Math.Pow(tw, 2) * (1 + (4 * Ntop - 0.2) * Math.Pow(tw / tf, 1.5)) * Math.Sqrt(beam.Material.Fy * tf / tw);

//					if (Nbot <= 0.2)
//						RcapBot = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(ConstNum.ELASTICITY, 0.5) * Math.Pow(tw, 2) * (1 + 3 * Nbot * Math.Pow(tw / tf, 1.5)) * Math.Sqrt(beam.Material.Fy * tf / tw);
//					else
//						RcapBot = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(ConstNum.ELASTICITY, 0.5) * Math.Pow(tw, 2) * (1 + (4 * Nbot - 0.2) * Math.Pow(tw / tf, 1.5)) * Math.Sqrt(beam.Material.Fy * tf / tw);

//					if (k == 1)
//					{
//						Reporting.AddHeader("Beam Web Crippling:");
//						if (beam.IsActive)
//						{
//							Reporting.AddHeader("Force from Top, Rtop = VbTop + 3 * MbTop / Ltop");
//							Reporting.AddLine("= " + Math.Abs(Vi1Max) + " + 3 * " + Math.Abs(Mi1Max) + " / " + (upperBrace.BraceConnect.Gusset.Hb - GapT));
//							Reporting.AddLine("= " + Rtop + ConstUnit.Force);
//							Reporting.AddLine("Top Loading, FiRn:");
//							if (Ntop <= 0.2)
//							{
//								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstNum.ELASTICITY + "^0.5 * tw² * (1 + 3 * (Ntop / d) * (tw / tf)^1.5) * (Fy * tf / tw)^0.5");
//								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + Math.Pow(ConstNum.ELASTICITY, 0.5) + " * " + tw + "² * (1 + 3 * (" + upperBrace.BraceConnect.Gusset.Hb + " / " + beam.Shape.d + ")");
//								Reporting.AddLine("* (" + tw + " / " + tf + ")^1.5) * (" + beam.Material.Fy + " * " + tf + " / " + tw + ")^0.5");
//							}
//							else
//							{
//								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstNum.ELASTICITY + "^0.5 * tw² * (1 + (4 * (Ntop / d)-0.2) * (tw / tf)^1.5) * (Fy * tf / tw)^0.5");
//								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + Math.Pow(ConstNum.ELASTICITY, 0.5) + " * " + tw + "² * (1 + (4 * (" + upperBrace.BraceConnect.Gusset.Hb + " / " + beam.Shape.d + ")-0.2)");
//								Reporting.AddLine("* (" + tw + " / " + tf + ")^1.5) * (" + beam.Material.Fy + " * " + tf + " / " + tw + ")^0.5");
//							}
//							if (RcapTop >= Rtop)
//								Reporting.AddLine(RcapTop + " >= " + Rtop + ConstUnit.Force + " (OK)");
//							else
//								Reporting.AddLine(RcapTop + " << " + Rtop + ConstUnit.Force + " (NG)");
//						}
//						if (lowerBrace.IsActive)
//						{
//							Reporting.AddHeader("Force from Bottom, Rbot = VbBot + 3 * MbBot / LBot");
//							Reporting.AddLine("= " + Math.Abs(Vi2Max) + " + 3 * " + Math.Abs(Mi2Max) + " / " + (lowerBrace.BraceConnect.Gusset.Hb - GapB));
//							Reporting.AddLine("= " + Rbot + ConstUnit.Force);
//							Reporting.AddHeader("Bottom Loading, FiRn:");
//							if (Nbot <= 0.2)
//							{
//								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstNum.ELASTICITY + "^0.5 * tw² * (1 + 3 * (Nbot / d) * (tw / tf)^1.5) * (Fy * tf / tw)^0.5");
//								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + Math.Pow(ConstNum.ELASTICITY, 0.5) + " * " + tw + "² * (1 + 3 * (" + (lowerBrace.BraceConnect.Gusset.Hb - GapB) + " / " + beam.Shape.d + ")");
//								Reporting.AddLine("* (" + tw + " / " + tf + ")^1.5) * (" + beam.Material.Fy + " * " + tf + " / " + tw + ")^0.5");
//							}
//							else
//							{
//								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + ConstNum.ELASTICITY + "^0.5 * tw² * (1 + (4 * (Nbot / d) - 0.2) * (tw / tf)^1.5) * (Fy * tf / tw)^0.5");
//								Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + Math.Pow(ConstNum.ELASTICITY, 0.5) + " * " + tw + "² * (1 + (4 * (" + (lowerBrace.BraceConnect.Gusset.Hb - GapB) + " / " + beam.Shape.d + ") - 0.2)");
//								Reporting.AddLine("* (" + tw + " / " + tf + ")^1.5) * (" + beam.Material.Fy + " * " + tf + " / " + tw + ")^0.5");
//							}
//							if (RcapBot >= Rbot)
//								Reporting.AddLine(RcapBot + " >= " + Rbot + ConstUnit.Force + " (OK)");
//							else
//								Reporting.AddLine(RcapBot + " << " + Rbot + ConstUnit.Force + " (NG)");
//						}
//					}

//					if (RcapTop < Rtop || RcapBot < Rbot)
//						tw += ConstNum.SIXTEENTH_INCH;
//					if (k > 1000)
//						break;
//				} while (RcapTop < Rtop && upperBrace.IsActive || RcapBot < Rbot && lowerBrace.IsActive);

//				if (tw > beam.Shape.tw)
//					Reporting.AddHeader("Required Thickness (Web Crippling) = " + tw + ConstUnit.Length);
//			}
//			else // V Brace
//			{
//				H = Math.Abs(-beam.BraceConnect.Gusset.Hb + upperBrace.BraceConnect.Gusset.Hb);
//				V = Math.Abs(beam.BraceConnect.Gusset.VerticalForceBeam + upperBrace.BraceConnect.Gusset.VerticalForceBeam);

//				//TODO: Figure out what 26, 36, 25 are
//				//ortasi = (BRACE1.x[26 + i1] + BRACE1.x[36 + i1]) / 2;
//				//L = BRACE1.x[26 + i1] - BRACE1.x[36 + i1];
//				//SmallMethodsDesign.Intersect(0, BRACE1.x[25 + i1].ToDbl(), BRACE1.y[25 + i1].ToDbl(), Math.Tan(beam.Angle * BRACE1.radian), beam.WorkPointX, beam.WorkPointY, ref xRight2, ref dumy);
//				//eRight = xRight2 - ortasi;
//				//SmallMethodsDesign.Intersect(0, BRACE1.x[25 + i1].ToDbl(), BRACE1.y[25 + i1].ToDbl(), Math.Tan(component.Angle * BRACE1.radian), component.WorkPointX, component.WorkPointY, ref xLeft2, ref dumy);
//				eRight = xRight2 - ortasi;
//				eLeft = xLeft2 - ortasi;
//				Moment = Math.Abs(eLeft * beam.BraceConnect.Gusset.VerticalForceBeam + eRight * upperBrace.BraceConnect.Gusset.VerticalForceBeam);

//				R = Math.Sqrt(Math.Pow(1.73 * H, 2) + Math.Pow(V + 3 * Moment / L, 2));
//				thWebYieldingTop = R / (ConstNum.FIOMEGA1_0N * beam.Material.Fy * L);

//				Reporting.AddHeader("Beam Local Stresses");
//				Reporting.AddHeader("Beam Web Local Yielding:");
//				Reporting.AddLine("Force from Gusset (R) = ((1.73 * H)² + (V + 3 * M / L)²)^0.5");
//				Reporting.AddLine("= ((1.73 * " + H + ")² + (" + V + " + 3 * " + Moment + " / " + L + ")²)^0.5");
//				Reporting.AddLine("= " + R + ConstUnit.Force);

//				Reporting.AddHeader("Required Web Thickness = R / (" + ConstNum.FIOMEGA1_0N + " * Fy * L)");
//				Reporting.AddLine("= " + R + " / (" + ConstNum.FIOMEGA1_0N + " * " + beam.Material.Fy + " * " + L + ")");
//				if (thWebYieldingTop >= beam.Shape.tw)
//					Reporting.AddLine("= " + thWebYieldingTop + " >= " + beam.Shape.tw + ConstUnit.Length + " (NG)");
//				else
//					Reporting.AddLine("= " + thWebYieldingTop + " << " + beam.Shape.tw + ConstUnit.Length + " (OK)");

//				R = V + 3 * Moment / L;
//				tw = beam.Shape.tw;
//				tf = beam.Shape.tf;
//				Rcap = ConstNum.FIOMEGA0_75N * 0.8 * Math.Pow(ConstNum.ELASTICITY, 0.5) * Math.Pow(tw, 2) * (1 + 3 * (L / beam.Shape.d) * Math.Pow(tw / tf, 1.5)) * Math.Sqrt(beam.Material.Fy * tf / tw);

//				Reporting.AddHeader("Beam Web Crippling:");
//				Reporting.AddLine("Force from Gusset (R) = V + 3 * M / L");
//				Reporting.AddLine("= " + V + " + 3 * " + Moment + " / " + L);
//				Reporting.AddLine("= " + R + ConstUnit.Force);
//				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.8 * " + ConstNum.ELASTICITY + "^0.5 * tw² * (1 + 3 * (L / d) * (tw / tf)^1.5) * (Fy * tf / tw)^0.5");
//				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.8 * " + Math.Pow(ConstNum.ELASTICITY, 0.5) + " * " + tw + "² * (1 + 3 * (" + L + " / " + beam.Shape.d + ") * (" + tw + " / " + tf + ")^1.5) * (" + beam.Material.Fy + " * " + tf + " / " + tw + ")^0.5");
//				if (Rcap >= R)
//					Reporting.AddLine("= " + Rcap + " >= " + R + ConstUnit.Force + " (OK)");
//				else
//					Reporting.AddLine("= " + Rcap + " << " + R + ConstUnit.Force + " (NG)");
//			}
//		}
//	}
//}