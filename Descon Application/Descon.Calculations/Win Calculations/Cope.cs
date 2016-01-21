using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class Cope
	{
		internal static void CopedBeam(EMemberType memberType, bool enableReport)
		{
			double SreqForRupture = 0;
			double SreqForYielding = 0;
			double SreqForBuckling = 0;
			double h_connector = 0;
			double A_req = 0;
			double SecMod = 0;
			double SecmodR = 0;
			double SecmodY = 0;
			double SecModB = 0;
			double VGross;
			double Ff;
			double kk;
			double Ixx;
			double dumy1;
			double nn;
			double dumy;
			double FiFcr;
			double Q;
			double lamda;
			double FiFbc;
			double fd;
			double h01;
			double h0;
			double dc;
			double e;
			double c;
			double MaxSectionMod;
			double ruptureStrength;
			double YieldingStrength;
			double BucklingStrength;

			var beam = CommonDataStatic.DetailDataDict[memberType];

			if (!beam.WinConnect.Beam.TopCope && !beam.WinConnect.Beam.BottomCope)
				return;

			switch (beam.ShearConnection)
			{
				case EShearCarriedBy.ClipAngle:
					if (beam.WinConnect.ShearClipAngle.BeamSideConnection == EConnectionStyle.Welded)
						h_connector = beam.WinConnect.ShearClipAngle.Length + 2 * (beam.WinConnect.ShearClipAngle.WeldSizeBeam + ConstNum.SIXTEENTH_INCH);
					else
						h_connector = beam.WinConnect.ShearClipAngle.Length;
					break;
				case EShearCarriedBy.SinglePlate:
					h_connector = beam.WinConnect.ShearWebPlate.Length;
					break;
				case EShearCarriedBy.Tee:
					if (beam.WinConnect.ShearWebTee.BeamSideConnection == EConnectionStyle.Welded)
						h_connector = beam.WinConnect.ShearWebTee.SLength + 2 * (beam.WinConnect.ShearWebTee.WeldSizeStem + ConstNum.SIXTEENTH_INCH);
					else
						h_connector = beam.WinConnect.ShearWebTee.SLength;
					break;
				case EShearCarriedBy.EndPlate:
					h_connector = beam.WinConnect.Beam.WebHeight;
					break;
			}

			//  Buckling check of coped beam
			if (beam.WinConnect.Beam.TCopeL > beam.WinConnect.Beam.BCopeL)
				c = beam.WinConnect.Beam.TCopeL;
			else
				c = beam.WinConnect.Beam.BCopeL;
			e = c + beam.EndSetback;
			dc = Math.Max(beam.WinConnect.Beam.TCopeD, beam.WinConnect.Beam.BCopeD);
			h0 = beam.Shape.d - beam.WinConnect.Beam.TCopeD - beam.WinConnect.Beam.BCopeD;
			if (beam.WinConnect.Beam.TopCope && beam.WinConnect.Beam.BottomCope)
			{
				if (beam.WinConnect.Beam.TCopeL <= 2 * beam.Shape.d && beam.WinConnect.Beam.TCopeD <= 0.2 * beam.Shape.d && (beam.WinConnect.Beam.BCopeL <= 2 * beam.Shape.d && beam.WinConnect.Beam.BCopeD <= 0.2 * beam.Shape.d))
					SecMod = beam.Shape.tw * h0 * h0 / 6;
				else if (beam.WinConnect.Beam.TCopeL <= 2 * beam.Shape.d && beam.WinConnect.Beam.BCopeL <= 2 * beam.Shape.d)
				{
					h0 = beam.Shape.d - beam.WinConnect.Beam.TCopeD - beam.WinConnect.Beam.BCopeD;
					c = Math.Max(beam.WinConnect.Beam.TCopeL, beam.WinConnect.Beam.BCopeL);
					lamda = h0 * Math.Pow(beam.Material.Fy, 0.5) / (10 * beam.Shape.tw * Math.Pow(475 + 280 * Math.Pow(h0 / c, 2), 0.5));

					if (lamda <= 0.7)
						Q = 1;
					else if (lamda <= 1.41)
						Q = 1.34 - 0.486 * lamda;
					else
						Q = 1.3 / Math.Pow(lamda, 2);

					FiFcr = ConstNum.FIOMEGA0_9N * beam.Material.Fy * Q;
					SecMod = beam.Shape.tw * h0 * h0 / 6;
					BucklingStrength = FiFcr * SecMod / e;
					if (BucklingStrength < beam.ShearForce)
						SecModB = SecMod * beam.ShearForce / BucklingStrength;

					VGross = MiscCalculationsWithReporting.GrossShearStrength(beam.Material.Fy, beam.Shape.tw * h0, memberType, false);
					if (VGross < beam.ShearForce)
						A_req = beam.Shape.tw * beam.WinConnect.Beam.WebHeight * beam.ShearForce / VGross;

					MaxSectionMod = Math.Max(SecModB, Math.Max(SecmodY, SecmodR));
					BeamCopeReinforcement(memberType, MaxSectionMod, A_req, (int)h0, h_connector, c, dc, false);
				}
			}
			else if (beam.WinConnect.Beam.TopCope)
			{
				if (beam.WinConnect.Beam.TCopeL <= 2 * beam.Shape.d && beam.WinConnect.Beam.TCopeD <= beam.Shape.d / 2)
				{
					h01 = beam.Shape.d - beam.WinConnect.Beam.TCopeD - beam.Shape.tf;
					dumy = beam.Shape.tw * 0.5 * h01 * h01 + beam.Shape.bf * (h01 + 0.5 * beam.Shape.tf) * beam.Shape.tf;
					nn = dumy / (h01 * beam.Shape.tw + beam.Shape.bf * beam.Shape.tf);
					dumy1 = (h01 + 0.5 * beam.Shape.tf - nn) * (h01 + 0.5 * beam.Shape.tf - nn);
					dumy = h01 * h01 / 12 + (0.5 * h01 - nn) * (0.5 * h01 - nn);
					Ixx = h01 * beam.Shape.tw * dumy + beam.Shape.bf * beam.Shape.tf * (beam.Shape.tf * beam.Shape.tf / 12 + dumy1);
					SecMod = Ixx / nn;
					//SecMod = beam.Shape.tw * h0 * h0 / 6;
					// Local Web Buckling
					if (c / h0 <= 1)
						kk = 2.2 * Math.Pow(h0 / c, 1.65);
					else
						kk = 2.2 * h0 / c;
					if (c / beam.Shape.d <= 1)
						Ff = 2 * c / beam.Shape.d;
					else
						Ff = 1 + c / beam.Shape.d;
					FiFbc = ConstNum.FIOMEGA0_9N * Math.Min(0.904 * ConstNum.ELASTICITY * Ff * kk * beam.Shape.tw * beam.Shape.tw / (h0 * h0), beam.Material.Fy);
					BucklingStrength = FiFbc * SecMod / e;
					if (BucklingStrength < beam.ShearForce)
						SecModB = SecMod * beam.ShearForce / BucklingStrength;
					// Flexural Yielding
					YieldingStrength = ConstNum.FIOMEGA0_9N * beam.Material.Fy * SecMod / e;
					if (YieldingStrength < beam.ShearForce)
						SecmodY = SecMod * beam.ShearForce / YieldingStrength;
					// Flexural Rupture
					ruptureStrength = ConstNum.FIOMEGA0_75N * beam.Material.Fu * SecMod / e;
					if (ruptureStrength < beam.ShearForce)
						SecmodR = SecMod * beam.ShearForce / ruptureStrength;

					VGross = MiscCalculationsWithReporting.GrossShearStrength(beam.Material.Fy, beam.Shape.tw * h0, memberType, false);
					if (VGross < beam.ShearForce)
						A_req = beam.Shape.tw * beam.WinConnect.Beam.WebHeight * beam.ShearForce / VGross;

					MaxSectionMod = Math.Max(SecModB, Math.Max(SecmodY, SecmodR));
					BeamCopeReinforcement(memberType, MaxSectionMod, A_req, (int)h0, h_connector, c, dc, false);
				}
			}
			else if (beam.WinConnect.Beam.BottomCope)
			{
				if (beam.WinConnect.Beam.BCopeL <= 2 * beam.Shape.d && beam.WinConnect.Beam.BCopeD <= beam.Shape.d / 2)
				{
					h01 = beam.Shape.d - beam.WinConnect.Beam.BCopeD - beam.Shape.tf;
					dumy = beam.Shape.tw * 0.5 * h01 * h01 + beam.Shape.bf * (h01 + 0.5 * beam.Shape.tf) * beam.Shape.tf;
					nn = dumy / (h01 * beam.Shape.tw + beam.Shape.bf * beam.Shape.tf);
					dumy = h01 * h01 / 12 + (0.5 * h01 - nn) * (0.5 * h01 - nn);
					dumy1 = (h01 + 0.5 * beam.Shape.tf - nn) * (h01 + 0.5 * beam.Shape.tf - nn);
					Ixx = h01 * beam.Shape.tw * dumy + beam.Shape.bf * beam.Shape.tf * (beam.Shape.tf * beam.Shape.tf / 12 + dumy1);
					SecMod = Ixx / nn;
					// Flexural Yielding
					YieldingStrength = ConstNum.FIOMEGA0_9N * beam.Material.Fy * SecMod / e;
					if (YieldingStrength < beam.ShearForce)
						SecmodY = SecMod * beam.ShearForce / YieldingStrength;
					// Flexural Rupture
					ruptureStrength = ConstNum.FIOMEGA0_75N * beam.Material.Fu * SecMod / e;
					if (ruptureStrength < beam.ShearForce)
						SecmodR = SecMod * beam.ShearForce / ruptureStrength;

					VGross = MiscCalculationsWithReporting.GrossShearStrength(beam.Material.Fy, beam.Shape.tw * h0, memberType, false);
					if (VGross < beam.ShearForce)
						A_req = beam.Shape.tw * beam.WinConnect.Beam.WebHeight * beam.ShearForce / VGross;

					MaxSectionMod = Math.Max(SecModB, Math.Max(SecmodY, SecmodR));
					BeamCopeReinforcement(memberType, MaxSectionMod, A_req, (int)h0, h_connector, c, dc, false);
				}
			}

			if (enableReport)
			{
				// check angle length to fit
				Reporting.AddGoToHeader("Coped Beam Web Strength:");
				if (beam.WinConnect.Beam.TCopeL > beam.WinConnect.Beam.BCopeL)
					c = beam.WinConnect.Beam.TCopeL;
				else
					c = beam.WinConnect.Beam.BCopeL;
				e = c + beam.EndSetback;
				dc = Math.Max(beam.WinConnect.Beam.TCopeD, beam.WinConnect.Beam.BCopeD);
				h0 = beam.Shape.d - beam.WinConnect.Beam.TCopeD - beam.WinConnect.Beam.BCopeD;
				Reporting.AddLine("Top Cope Length = " + beam.WinConnect.Beam.TCopeL + ConstUnit.Length);
				Reporting.AddLine("Top Cope Depth = " + beam.WinConnect.Beam.TCopeD + ConstUnit.Length);
				Reporting.AddLine("Bottom Cope Length = " + beam.WinConnect.Beam.BCopeL + ConstUnit.Length);
				Reporting.AddLine("Bottom Cope Depth = " + beam.WinConnect.Beam.BCopeD + ConstUnit.Length);
				Reporting.AddLine("c = " + c + ConstUnit.Length);
				Reporting.AddLine("e = " + e + ConstUnit.Length);
				Reporting.AddLine("h0 = " + h0 + ConstUnit.Length);
				Reporting.AddLine("d = " + beam.Shape.d + ConstUnit.Length);

				if (beam.WinConnect.Beam.TopCope && beam.WinConnect.Beam.BottomCope)
				{
					if (beam.WinConnect.Beam.TCopeL <= 2 * beam.Shape.d && beam.WinConnect.Beam.TCopeD <= 0.2 * beam.Shape.d && (beam.WinConnect.Beam.BCopeL <= 2 * beam.Shape.d && beam.WinConnect.Beam.BCopeD <= 0.2 * beam.Shape.d))
					{
						fd = 3.5 - 7.5 * (dc / beam.Shape.d);
						FiFbc = ConstNum.FIOMEGA0_9N * Math.Min(0.62 * Math.PI * ConstNum.ELASTICITY * fd * Math.Pow(beam.Shape.tw, 2) / (c * h0), beam.WinConnect.Beam.Material.Fy);
						BucklingStrength = FiFbc * SecMod / e;
						
						Reporting.AddHeader("Local Web Buckling:");
						Reporting.AddLine("fd = 3.5 - 7.5 * (dc / d) ");
						Reporting.AddLine("= 3.5 - 7.5 * (" + dc + " / " + beam.Shape.d + ") = " + fd);

						Reporting.AddLine(ConstString.PHI + " Fbc = " + ConstString.FIOMEGA0_9 + " * Min(0.62 * pi * " + ConstNum.ELASTICITY + " * fd * tw² / (c * h0), Fy)");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * Min(0.62 * pi * " + ConstNum.ELASTICITY + " * " + fd + " * " + beam.Shape.tw + " ^ 2 / (" + c + " * " + h0 + ")," + beam.Material.Fy + ")");
						Reporting.AddLine("= " + FiFbc + ConstUnit.Stress);

						Reporting.AddHeader("Buckling Strength = " + ConstString.PHI + " Fbc * Snet / e");
						Reporting.AddLine("= " + FiFbc + " * " + SecMod + " / " + e);
						
						if (BucklingStrength >= beam.ShearForce)
							Reporting.AddCapacityLine("= " + BucklingStrength + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / BucklingStrength, "Buckling Strength", memberType);
						else
						{
							Reporting.AddLine("= " + BucklingStrength + " << " + beam.ShearForce + ConstUnit.Force);
							Reporting.AddLine("Reinforcement Required");
							SreqForBuckling = SecMod * beam.ShearForce / BucklingStrength;
							Reporting.AddLine("Required Section Modulus, Sreq_Buckling = Snet * Ru / Buckling Strength");
							Reporting.AddLine("= " + SecMod + " * " + beam.ShearForce + " / " + BucklingStrength + " = " + SreqForBuckling + ConstUnit.SecMod);
						}
					}
					else if (beam.WinConnect.Beam.TCopeL <= 2 * beam.Shape.d && beam.WinConnect.Beam.BCopeL <= 2 * beam.Shape.d)
					{
						h0 = beam.Shape.d - beam.WinConnect.Beam.TCopeD - beam.WinConnect.Beam.BCopeD;
						c = Math.Max(beam.WinConnect.Beam.TCopeL, beam.WinConnect.Beam.BCopeL);
						Reporting.AddLine("h0 = " + h0 + "  c = " + c + " K = " + 0); // 0 was originally k, but k was never set
						lamda = h0 * Math.Pow(beam.Material.Fy, 0.5) / (10 * beam.Shape.tw * Math.Pow(475 + 280 * Math.Pow(h0 / c, 2), 0.5));
						Reporting.AddLine("Lambda = h0 * Fy^0.5 / (10 * tw * (475 + 280 * (h0 / c)²)^0.5)");
						Reporting.AddLine("= " + h0 + " * " + beam.Material.Fy + "^0.5 / (10 * " + beam.Shape.tw + " * (475 + 280 * (" + h0 + " / " + c + ")²)^0.5)");
						Reporting.AddLine("= " + lamda);
						if (lamda <= 0.7)
						{
							Q = 1;
							Reporting.AddLine("Q = 1");
						}
						else if (lamda <= 1.41)
						{
							Q = 1.34 - 0.486 * lamda;
							Reporting.AddLine("Q = 1.34 - 0.486 * Lambda = 1.34 - 0.486 * " + lamda + " = " + Q);
						}
						else
						{
							Q = 1.3 / Math.Pow(lamda, 2);
							Reporting.AddLine("Q = 1.3 / (Lambda²) = 1.3 / (" + lamda + "²) = " + Q);
						}
						FiFcr = ConstNum.FIOMEGA0_9N * beam.Material.Fy * Q;
						Reporting.AddLine(ConstString.PHI + " Fcr = " + ConstString.FIOMEGA0_9 + " * Fy * Q = " + ConstString.FIOMEGA0_9 + " * " + beam.Material.Fy + " * " + Q);
						BucklingStrength = FiFcr * SecMod / e;
						Reporting.AddHeader("Buckling Strength = " + ConstString.PHI + " Fcr * Snet / e");
						Reporting.AddLine("= " + FiFcr + " * " + SecMod + " / " + e);
						
						if (BucklingStrength >= beam.ShearForce)
							Reporting.AddCapacityLine("= " + BucklingStrength + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / BucklingStrength, "Buckling Strength", memberType);
						else
						{
							Reporting.AddCapacityLine("= " + BucklingStrength + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / BucklingStrength, "Buckling Strength", memberType);
							Reporting.AddLine("Reinforcement Required");
							SreqForBuckling = SecMod * beam.ShearForce / BucklingStrength;
							Reporting.AddLine("Required Section Modulus, Sreq_Buckling = Snet * Ru / Buckling Strength");
							Reporting.AddLine("= " + SecMod + " * " + beam.ShearForce + " / " + BucklingStrength + " = " + SreqForBuckling + ConstUnit.SecMod);
						}
					}
					else
						Reporting.AddLine("Beam Cope is too long. (NG)");

					Reporting.AddHeader("Local Web Flexural Yielding, " + ConstString.PHI + " Mn / e:");
					YieldingStrength = ConstNum.FIOMEGA0_9N * beam.Material.Fy * SecMod / e;
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * Fy * Snet / e = " + ConstString.FIOMEGA0_9 + " * " + beam.Material.Fy + " * " + SecMod + " / " + e);
					if (YieldingStrength >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + YieldingStrength + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / YieldingStrength, "Local Web Flexural Yielding", memberType);
					else
					{
						Reporting.AddLine("= " + YieldingStrength + " << " + beam.ShearForce + ConstUnit.Force);
						Reporting.AddLine("Reinforcement Required");
						SreqForYielding = SecMod * beam.ShearForce / YieldingStrength;
						Reporting.AddLine("Required Section Modulus, Sreq_Yielding = Snet * Ru / Yielding Strength");
						Reporting.AddLine("= " + SecMod + " * " + beam.ShearForce + " / " + YieldingStrength + " = " + SreqForYielding + ConstUnit.SecMod);
					}
					Reporting.AddHeader("Local Web Flexural Rupture, " + ConstString.PHI + " Mn / e:");
					ruptureStrength = ConstNum.FIOMEGA0_75N * beam.Material.Fu * SecMod / e;
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * Fu * Snet / e = " + ConstString.FIOMEGA0_75 + "  * " + beam.Material.Fu + " * " + SecMod + " / " + e);
					if (ruptureStrength >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + ruptureStrength + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / ruptureStrength, "Local Web Flexural Rupture", memberType);
					else
					{
						Reporting.AddLine("= " + ruptureStrength + " << " + beam.ShearForce + ConstUnit.Force);
						Reporting.AddLine("Reinforcement Required");
						SreqForRupture = SecMod * beam.ShearForce / ruptureStrength;
						Reporting.AddLine("Required Section Modulus, Sreq_Rupture= Snet * Ru/Rupture Strength");
						Reporting.AddLine("= " + SecMod + " * " + beam.ShearForce + " / " + ruptureStrength + " = " + SreqForRupture + ConstUnit.SecMod);
					}
				}
				else if (beam.WinConnect.Beam.TopCope)
				{
					if (beam.WinConnect.Beam.TCopeL <= 2 * beam.Shape.d && beam.WinConnect.Beam.TCopeD <= beam.Shape.d / 2)
					{
						h01 = beam.Shape.d - beam.WinConnect.Beam.TCopeD - beam.Shape.tf;
						dumy = beam.Shape.tw * 0.5 * h01 * h01 + beam.Shape.bf * (h01 + 0.5 * beam.Shape.tf) * beam.Shape.tf;
						nn = dumy / (h01 * beam.Shape.tw + beam.Shape.bf * beam.Shape.tf);
						dumy1 = (h01 + 0.5 * beam.Shape.tf - nn) * (h01 + 0.5 * beam.Shape.tf - nn);
						dumy = h01 * h01 / 12 + (0.5 * h01 - nn) * (0.5 * h01 - nn);
						Ixx = h01 * beam.Shape.tw * dumy + beam.Shape.bf * beam.Shape.tf * (beam.Shape.tf * beam.Shape.tf / 12 + dumy1);
						SecMod = Ixx / nn;

						Reporting.AddLine(String.Empty);

						if (c / h0 <= 1)
						{
							kk = 2.2 * Math.Pow(h0 / c, 1.65);
							Reporting.AddLine("c / h0 <= 1, k = 2.2 * (h0 / c)^1.65 ");
							Reporting.AddLine("= 2.2 * (" + h0 + " / " + c + ")^1.65 = " + kk);
						}
						else
						{
							kk = 2.2 * h0 / c;
							Reporting.AddLine("c / h0 >> 1, k = 2.2 * (h0 / c) ");
							Reporting.AddLine("= 2.2 * (" + h0 + " / " + c + ") = " + kk);
						}

						Reporting.AddLine(String.Empty);

						if (c / beam.Shape.d <= 1)
						{
							Ff = 2 * c / beam.Shape.d;
							Reporting.AddLine("c / d <= 1, f = 2 * c / d ");
							Reporting.AddLine("= 2 * " + c + " / " + beam.Shape.d + " = " + Ff);
						}
						else
						{
							Ff = 1 + c / beam.Shape.d;
							Reporting.AddLine("c / d >> 1, f = 1 + c / d ");
							Reporting.AddLine("= 1 + " + c + " / " + beam.Shape.d + " = " + Ff);
						}
						FiFbc = ConstNum.FIOMEGA0_9N * Math.Min(0.904 * ConstNum.ELASTICITY * Ff * kk * Math.Pow(beam.Shape.tw / h0, 2), beam.Material.Fy);
						Reporting.AddLine(ConstString.PHI + " Fbc = " + ConstString.FIOMEGA0_9 + " * Min(0.904 * E * f * k * (tw / h0)², Fy)");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * Min(0.904 * " + ConstNum.ELASTICITY + " * " + Ff + " * " + kk + " * (" + beam.Shape.tw + " / " + h0 + ")²)  ," + beam.Material.Fy + ")");
						Reporting.AddLine("= " + FiFbc + ConstUnit.Stress);

						BucklingStrength = FiFbc * SecMod / e;
						Reporting.AddHeader("Buckling Strength = " + ConstString.PHI + " Fbc * Snet / e");
						Reporting.AddLine("= " + FiFbc + " * " + SecMod + " / " + e);
						if (BucklingStrength >= beam.ShearForce)
							Reporting.AddCapacityLine("= " + BucklingStrength + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / BucklingStrength, "Buckling Strength", memberType);
						else
						{
							Reporting.AddLine("= " + BucklingStrength + " << " + beam.ShearForce + ConstUnit.Force);
							Reporting.AddLine("Reinforcement Required");
							SreqForBuckling = SecMod * beam.ShearForce / BucklingStrength;
							Reporting.AddLine("Required Section Modulus, Sreq_Buckling = Snet * Ru / Buckling Strength");
							Reporting.AddLine("= " + SecMod + " * " + beam.ShearForce + " / " + BucklingStrength + " = " + SreqForBuckling + ConstUnit.SecMod);
						}
						Reporting.AddLine("Local Web Flexural Yielding, " + ConstString.PHI + " Mn/e:");
						YieldingStrength = ConstNum.FIOMEGA0_9N * beam.Material.Fy * SecMod / e;
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * Fy * Snet / e = " + ConstString.FIOMEGA0_9 + " * " + beam.Material.Fy + " * " + SecMod + " / " + e);
						if (YieldingStrength >= beam.ShearForce)
							Reporting.AddCapacityLine("= " + YieldingStrength + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / YieldingStrength, "Local Web Flexural Yielding", memberType);
						else
						{
							Reporting.AddLine("= " + YieldingStrength + " << " + beam.ShearForce + ConstUnit.Force);
							Reporting.AddLine("Reinforcement Required");
							SreqForYielding = SecMod * beam.ShearForce / YieldingStrength;
							Reporting.AddLine("Required Section Modulus, Sreq_Yielding = Snet * Ru / Yielding Strength");
							Reporting.AddLine("= " + SecMod + " * " + beam.ShearForce + " / " + YieldingStrength + " = " + SreqForYielding + ConstUnit.SecMod);
						}
						Reporting.AddLine("Local Web Flexural Rupture " + ConstString.PHI + " Mn / e:");
						ruptureStrength = ConstNum.FIOMEGA0_75N * beam.Material.Fu * SecMod / e;
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * Fu * Snet / e = " + ConstString.FIOMEGA0_75 + "  * " + beam.Material.Fu + " * " + SecMod + " / " + e);
						if (ruptureStrength >= beam.ShearForce)
							Reporting.AddCapacityLine("= " + ruptureStrength + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / ruptureStrength, "Local Web Flexural Rupture", memberType);
						else
						{
							Reporting.AddLine("= " + ruptureStrength + " << " + beam.ShearForce + ConstUnit.Force);
							Reporting.AddLine("Reinforcement Required");
							SreqForRupture = SecMod * beam.ShearForce / ruptureStrength;
							Reporting.AddLine("Required Section Modulus, Sreq_Rupture= Snet * Ru / Rupture Strength");
							Reporting.AddLine("= " + SecMod + " * " + beam.ShearForce + " / " + ruptureStrength + " = " + SreqForRupture + ConstUnit.SecMod);
						}
					}
					else
					{
						Reporting.AddLine("Beam Cope is too long. (NG)");
						return;
					}
				}
				else if (beam.WinConnect.Beam.BottomCope)
				{
					if (beam.WinConnect.Beam.BCopeL <= 2 * beam.Shape.d && beam.WinConnect.Beam.BCopeD <= beam.Shape.d / 2)
					{
						h01 = beam.Shape.d - beam.WinConnect.Beam.BCopeD - beam.Shape.tf;
						dumy = beam.Shape.tw * 0.5 * h01 * h01 + beam.Shape.bf * (h01 + 0.5 * beam.Shape.tf) * beam.Shape.tf;
						nn = dumy / (h01 * beam.Shape.tw + beam.Shape.bf * beam.Shape.tf);
						dumy = h01 * h01 / 12 + (0.5 * h01 - nn) * (0.5 * h01 - nn);
						dumy1 = (h01 + 0.5 * beam.Shape.tf - nn) * (h01 + 0.5 * beam.Shape.tf - nn);
						Ixx = h01 * beam.Shape.tw * dumy + beam.Shape.bf * beam.Shape.tf * (beam.Shape.tf * beam.Shape.tf / 12 + dumy1);
						SecMod = Ixx / nn;
						YieldingStrength = ConstNum.FIOMEGA0_9N * beam.Material.Fy * SecMod / e;
						Reporting.AddHeader("Local Web Flexural Yielding, " + ConstString.PHI + " Mn / e:");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * Fy * Snet / e = " + ConstString.FIOMEGA0_9 + " * " + beam.Material.Fy + " * " + SecMod + " / " + e);

						if (YieldingStrength >= beam.ShearForce)
							Reporting.AddCapacityLine("= " + YieldingStrength + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / YieldingStrength, "Local Web Flexural Yielding", memberType);
						else
						{
							Reporting.AddLine("= " + YieldingStrength + " << " + beam.ShearForce + ConstUnit.Force);
							Reporting.AddLine("Reinforcement Required");
							SreqForYielding = SecMod * beam.ShearForce / YieldingStrength;
							Reporting.AddLine("Required Section Modulus, Sreq_Yielding = Snet * Ru / Yielding Strength");
							Reporting.AddLine("= " + SecMod + " * " + beam.ShearForce + " / " + YieldingStrength + " = " + SreqForYielding + ConstUnit.SecMod);
						}
						ruptureStrength = ConstNum.FIOMEGA0_75N * beam.Material.Fu * SecMod / e;
						Reporting.AddHeader("Local Web Flexural Rupture " + ConstString.PHI + " Mn / e:");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * Fy * Snet / e = " + ConstString.FIOMEGA0_75 + " * " + beam.Material.Fu + " * " + SecMod + " / " + e);
						if (ruptureStrength >= beam.ShearForce)
							Reporting.AddCapacityLine("= " + ruptureStrength + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / ruptureStrength, "Local Web Flexural Rupture", memberType);
						else
						{
							Reporting.AddLine("= " + ruptureStrength + " << " + beam.ShearForce + ConstUnit.Force);
							Reporting.AddLine("Reinforcement Required");
							SreqForRupture = SecMod * beam.ShearForce / ruptureStrength;
							Reporting.AddLine("Required Section Modulus, Sreq_Rupture= Snet * Ru / Rupture Strength");
							Reporting.AddLine("= " + SecMod + " * " + beam.ShearForce + " / " + ruptureStrength + " = " + SreqForRupture + ConstUnit.SecMod);
						}
					}
					else
					{
						Reporting.AddLine("Beam Cope is too long. (NG)");
						return;
					}
				}

				if (!beam.WinConnect.Beam.TopCope)
				{
					VGross = MiscCalculationsWithReporting.GrossShearStrength(beam.Material.Fy, beam.Shape.tw * h0, memberType, false);
					if (VGross >= beam.ShearForce)
						Reporting.AddCapacityLine("Gross Shear Strength = " + VGross + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / VGross, "Gross Shear Strength", memberType);
					else
					{
						Reporting.AddCapacityLine("Gross Shear Strength = " + VGross + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / VGross, "Gross Shear Strength", memberType);
						Reporting.AddLine("Reinforcement Required");
						A_req = beam.Shape.tw * beam.WinConnect.Beam.WebHeight * beam.ShearForce / VGross;
						Reporting.AddLine(String.Empty);
						Reporting.AddLine("Required Area = tw * h0 * Ru / (Gross Shear Strength) = " + beam.Shape.tw + " * " + h0 + " * " + beam.ShearForce + " / " + VGross + ConstUnit.Length);
					}
				}
				MaxSectionMod = Math.Max(SreqForBuckling, Math.Max(SreqForYielding, SreqForRupture));

				if (MaxSectionMod > SecMod)
					BeamCopeReinforcement(memberType, MaxSectionMod, A_req, (int)h0, h_connector, c, dc, enableReport);
				else
					Reporting.AddLine("Web reinforcement not required for flexural strength.");
			}
		}

		private static void BeamCopeReinforcement(EMemberType memberType, double Sreq, double A_req, int h0, double h_connector, double L_Cope, double d_Cope, bool enableReporting)
		{
			double t = 0;
			double B = 0;
			double Ls = 0;
			double s = 0;
			double slenderness = 0;
			double y2;
			double x2;
			double y1;
			double X1;
			double d1 = 0;
			double Aw = 0;
			double Af = 0;
			double minWeld;
			double maxWeld;
			double maxWeldTest1;
			double maxWeldTest2;
			double weldStrength;
			double weldForce;
			double ix = 0;
			double h;
			double ho;
			double w;
			double minThickness;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var wcBeam = beam.WinConnect.Beam;

			double sqrtFy = Math.Sqrt(beam.Material.Fy);

			// h_connector includes space for weld if any
			if (Sreq == 0)
			{
				wcBeam.CopeReinforcement.Type = EReinforcementType.None;
				wcBeam.CopeReinforcement.LengthT = 0;
				wcBeam.CopeReinforcement.LengthB = 0;
				wcBeam.CopeReinforcement.Width = 0;
				wcBeam.CopeReinforcement.t = 0;
				wcBeam.CopeReinforcement.WeldSize = 0;

				return;
			}
			if (wcBeam.TopCope)
				wcBeam.CopeReinforcement.LengthT = 0;
			if (wcBeam.BottomCope)
				wcBeam.CopeReinforcement.LengthB = 0;

			ho = beam.Shape.d - wcBeam.TCopeD - wcBeam.BCopeD;
			h = beam.Shape.d - 2 * beam.Shape.kdes;
			if (wcBeam.TopCope && wcBeam.BottomCope)
			{
				B = Math.Min(Math.Max(beam.Shape.bf / 2, 5), beam.Shape.bf);

				double sTest = 0.38 * Math.Pow(ConstNum.ELASTICITY, 0.5) / sqrtFy;
				
				do
				{
					t += ConstNum.SIXTEENTH_INCH;
					t = CommonCalculations.PlateThickness(t);
					ix = 1 / 12.0 * B * Math.Pow(ho + 2 * t, 3) - 1 / 12.0 * (B - beam.Shape.tw) * Math.Pow(ho, 3);
					s = ix / (ho / 2 + t);
					slenderness = (B - beam.Shape.tw) / (2 * t);
				} while (s < Sreq || slenderness > sTest);

				if (h / beam.Shape.tw <= 60)
				{
					wcBeam.CopeReinforcement.Type = EReinforcementType.HorizontalSiffener;
					wcBeam.CopeReinforcement.LengthT = NumberFun.Round(wcBeam.TCopeL + wcBeam.TCopeD, 4);
					wcBeam.CopeReinforcement.LengthB = NumberFun.Round(wcBeam.BCopeL + wcBeam.BCopeD, 4);
					wcBeam.CopeReinforcement.Width = B;
					wcBeam.CopeReinforcement.t = t;
				}
				else
				{
					wcBeam.CopeReinforcement.Type = EReinforcementType.HorizontalAndVerticalStiffener;
					wcBeam.CopeReinforcement.LengthT = NumberFun.Round(wcBeam.TCopeL + wcBeam.TCopeL / 3, 4);
					wcBeam.CopeReinforcement.LengthB = NumberFun.Round(wcBeam.BCopeL + wcBeam.BCopeL / 3, 4);
					wcBeam.CopeReinforcement.Width = B;
					wcBeam.CopeReinforcement.t = t;
				}
				// welds
				if (wcBeam.TopCope && wcBeam.BottomCope)
				{
					beam.WinConnect.Fema.L = Math.Min(wcBeam.TCopeL, wcBeam.BCopeL);
					Ls = Math.Min(wcBeam.CopeReinforcement.LengthT, wcBeam.CopeReinforcement.LengthB);
				}
				else if (wcBeam.TopCope)
				{
					beam.WinConnect.Fema.L = wcBeam.TCopeL;
					Ls = wcBeam.CopeReinforcement.LengthT;
				}
				else if (wcBeam.BottomCope)
				{
					beam.WinConnect.Fema.L = wcBeam.BCopeL;
					Ls = wcBeam.CopeReinforcement.LengthB;
				}

				weldForce = beam.ShearForce * (beam.EndSetback + beam.WinConnect.Fema.L) / s * B * t;
				w = NumberFun.Round(weldForce / (2 * 0.707 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * Ls), 16);
				minWeld = CommonCalculations.MinimumWeld(beam.Shape.tw, t);
				wcBeam.CopeReinforcement.WeldSize = Math.Max(w, minWeld);
			}
			else if (wcBeam.TopCope || wcBeam.BottomCope)
			{
				ho = beam.Shape.d - wcBeam.TCopeD - wcBeam.BCopeD;
				Af = beam.Shape.bf * beam.Shape.tf;
				Aw = (ho - beam.Shape.tf) * beam.Shape.tw;
				B = NumberFun.Round(Math.Min(Math.Max(beam.Shape.bf / 2, 5), beam.Shape.bf), ERoundingPrecision.Fourth, ERoundingStyle.Nearest);
				double sTest = 0.38 * Math.Pow(ConstNum.ELASTICITY, 0.5) / sqrtFy;

				do
				{
					t += ConstNum.SIXTEENTH_INCH;
					t = CommonCalculations.PlateThickness(t);

					//Ix = 1 / 12 * bf * tf³ + Af * (d1 - tf / 2)² 
					//+ 1 / 12 * tw * (ho - tf)³ + Aw * (d1 + tf - (ho - tf) / 2)²
					//+ 1 / 12 * b * t³ + b * t * (ho + tp / 2 - d1)²

					d1 = (Af * beam.Shape.tf / 2 + Aw * (beam.Shape.tf + (ho - beam.Shape.tf) / 2) + B * t * (ho + t / 2)) / (Af + Aw + B * t);
					ix = 1 / 12.0 * beam.Shape.bf * Math.Pow(beam.Shape.tf, 3) + Af * Math.Pow(d1 - beam.Shape.tf / 2, 2) +
					     1 / 12.0 * beam.Shape.tw * Math.Pow(ho - beam.Shape.tf, 3) + Aw * Math.Pow(d1 - beam.Shape.tf - (ho - beam.Shape.tf) / 2, 2) +
					     1 / 12.0 * B * Math.Pow(t, 3) + B * t * Math.Pow(ho + t / 2 - d1, 2);

					s = ix / Math.Max(d1, ho + t - d1);
					slenderness = (B - beam.Shape.tw) / (2 * t);
				} while (s < Sreq || slenderness > sTest);

				if (h / beam.Shape.tw <= 60)
				{
					wcBeam.CopeReinforcement.Type = EReinforcementType.HorizontalSiffener;
					if (wcBeam.TopCope)
						wcBeam.CopeReinforcement.LengthT = NumberFun.Round(wcBeam.TCopeL + wcBeam.TCopeD, 4);
					if (wcBeam.BottomCope)
						wcBeam.CopeReinforcement.LengthB = NumberFun.Round(wcBeam.BCopeL + wcBeam.BCopeD, 4);
					wcBeam.CopeReinforcement.Width = B;
					wcBeam.CopeReinforcement.t = t;
				}
				else
				{
					wcBeam.CopeReinforcement.Type = EReinforcementType.HorizontalAndVerticalStiffener;
					if (wcBeam.TopCope)
						wcBeam.CopeReinforcement.LengthT = NumberFun.Round(wcBeam.TCopeL + wcBeam.TCopeL / 3, 4);
					if (wcBeam.BottomCope)
						wcBeam.CopeReinforcement.LengthB = NumberFun.Round(wcBeam.BCopeL + wcBeam.BCopeL / 3, 4);
					wcBeam.CopeReinforcement.Width = B;
					wcBeam.CopeReinforcement.t = t;
				}
				// welds
				if (wcBeam.TopCope && wcBeam.BottomCope)
					beam.WinConnect.Fema.L = Math.Min(wcBeam.TCopeL, wcBeam.BCopeL);
				else if (wcBeam.TopCope)
					beam.WinConnect.Fema.L = wcBeam.TCopeL;
				else if (wcBeam.BottomCope)
					beam.WinConnect.Fema.L = wcBeam.BCopeL;
			}

			weldForce = beam.ShearForce * (beam.EndSetback + beam.WinConnect.Fema.L) / s * B * t;

			maxWeldTest1 = beam.Shape.tw * beam.Material.Fu / (CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * 2);
			maxWeldTest2 = t * beam.Material.Fu / (CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707);
			maxWeld = Math.Min(maxWeldTest1, maxWeldTest2);
			minWeld = CommonCalculations.MinimumWeld(beam.Shape.tw, t);
			if (maxWeld >= minWeld)
				wcBeam.CopeReinforcement.WeldSize = minWeld;
			else
				wcBeam.CopeReinforcement.WeldSize = maxWeld;

			do
			{
				weldStrength = ConstNum.FIOMEGA0_75N * wcBeam.CopeReinforcement.WeldSize * 2 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * beam.WinConnect.Fema.L;
				wcBeam.CopeReinforcement.WeldSize += ConstNum.SIXTEENTH_INCH;

			} while (weldStrength < weldForce && wcBeam.CopeReinforcement.WeldSize <= maxWeld);

			wcBeam.CopeReinforcement.WeldSize -= ConstNum.SIXTEENTH_INCH;

			wcBeam.CopeReinforcement.Width = wcBeam.CopeReinforcement.Width;//NumberFun.Round(wcBeam.CopeReinforcement.Width, ERoundingPrecision.Fourth, ERoundingStyle.Nearest);

			if (enableReporting)
			{
				Reporting.AddHeader("Design Stiffeners to get Snet = " + Sreq + ConstUnit.SecMod);
				if (wcBeam.CopeReinforcement.Type != EReinforcementType.DoublerPlate)
				{
					if (wcBeam.TopCope && wcBeam.BottomCope)
					{
						if (wcBeam.CopeReinforcement.Type != EReinforcementType.HorizontalAndVerticalStiffener)
						{
							if (wcBeam.CopeReinforcement.LengthB == wcBeam.CopeReinforcement.LengthT)
							{
								Reporting.AddHeader("Top & Bottom PL " + wcBeam.CopeReinforcement.LengthT + " X " + wcBeam.CopeReinforcement.Width + " X " + wcBeam.CopeReinforcement.t + ConstUnit.Length);
								if (wcBeam.CopeReinforcement.LengthT >= wcBeam.TCopeL + wcBeam.TCopeD)
									Reporting.AddLine("Plate Length = " + wcBeam.CopeReinforcement.LengthT + " >=  c + dc  = " + (wcBeam.TCopeL + wcBeam.TCopeD) + ConstUnit.Length + " (OK)");
								else
									Reporting.AddLine("Plate Length = " + wcBeam.CopeReinforcement.LengthT + " <<  c + dc  = " + (wcBeam.TCopeL + wcBeam.TCopeD) + ConstUnit.Length + " (NG)");
							}
							else
							{
								Reporting.AddHeader("Top PL " + wcBeam.CopeReinforcement.LengthT + "X" + wcBeam.CopeReinforcement.Width + "X" + wcBeam.CopeReinforcement.t + ConstUnit.Length);
								if (wcBeam.CopeReinforcement.LengthT >= wcBeam.TCopeL + wcBeam.TCopeD)
									Reporting.AddLine("Plate Length = " + wcBeam.CopeReinforcement.LengthT + " >=  c + dc  = " + (wcBeam.TCopeL + wcBeam.TCopeD) + ConstUnit.Length + " (OK)");
								else
									Reporting.AddLine("Plate Length = " + wcBeam.CopeReinforcement.LengthT + " <<  c + dc  = " + (wcBeam.TCopeL + wcBeam.TCopeD) + ConstUnit.Length + " (NG)");

								Reporting.AddHeader("Bottom PL " + wcBeam.CopeReinforcement.LengthB + "X" + wcBeam.CopeReinforcement.Width + "X" + wcBeam.CopeReinforcement.t + ConstUnit.Length);
								if (wcBeam.CopeReinforcement.LengthB >= wcBeam.BCopeL + wcBeam.BCopeD)
									Reporting.AddLine("Plate Length = " + wcBeam.CopeReinforcement.LengthB + " >=  c + dc  = " + (wcBeam.BCopeL + wcBeam.BCopeD) + ConstUnit.Length + " (OK)");
								else
									Reporting.AddLine("Plate Length = " + wcBeam.CopeReinforcement.LengthB + " <<  c + dc  = " + (wcBeam.BCopeL + wcBeam.BCopeD) + ConstUnit.Length + " (NG)");
							}
						}
						else
						{
							if (wcBeam.CopeReinforcement.LengthB == wcBeam.CopeReinforcement.LengthT)
							{
								Reporting.AddHeader("Top & Bottom PL " + wcBeam.CopeReinforcement.LengthT + "X" + wcBeam.CopeReinforcement.Width + "X" + wcBeam.CopeReinforcement.t + ConstUnit.Length);
								if (wcBeam.CopeReinforcement.LengthT >= wcBeam.TCopeL + wcBeam.TCopeL / 3)
									Reporting.AddLine("Plate Length = " + wcBeam.CopeReinforcement.LengthT + " >=  c + c / 3  = " + (wcBeam.TCopeL + wcBeam.TCopeL / 3) + ConstUnit.Length + " (OK)");
								else
									Reporting.AddLine("Plate Length = " + wcBeam.CopeReinforcement.LengthT + " <<  c + c / 3  = " + (wcBeam.TCopeL + wcBeam.TCopeL / 3) + ConstUnit.Length + " (NG)");
							}
							else
							{
								Reporting.AddHeader("Top PL " + wcBeam.CopeReinforcement.LengthT + "X" + wcBeam.CopeReinforcement.Width + "X" + wcBeam.CopeReinforcement.t + ConstUnit.Length);
								if (wcBeam.CopeReinforcement.LengthT >= wcBeam.TCopeL + wcBeam.TCopeL / 3)
									Reporting.AddLine("Plate Length = " + wcBeam.CopeReinforcement.LengthT + " >=  c + c / 3  = " + (wcBeam.TCopeL + wcBeam.TCopeL / 3) + ConstUnit.Length + " (OK)");
								else
									Reporting.AddLine("Plate Length = " + wcBeam.CopeReinforcement.LengthT + " <<  c + c / 3  = " + (wcBeam.TCopeL + wcBeam.TCopeL / 3) + ConstUnit.Length + " (NG)");

								Reporting.AddHeader("Bottom PL " + wcBeam.CopeReinforcement.LengthB + "X" + wcBeam.CopeReinforcement.Width + "X" + wcBeam.CopeReinforcement.t + ConstUnit.Length);
								if (wcBeam.CopeReinforcement.LengthB >= wcBeam.BCopeL + wcBeam.BCopeL / 3)
									Reporting.AddLine("Plate Length = " + wcBeam.CopeReinforcement.LengthB + " >=  c + c / 3  = " + (wcBeam.BCopeL + wcBeam.BCopeL / 3) + ConstUnit.Length + " (OK)");
								else
									Reporting.AddLine("Plate Length = " + wcBeam.CopeReinforcement.LengthB + " <<  c + c / 3  = " + (wcBeam.BCopeL + wcBeam.BCopeL / 3) + ConstUnit.Length + " (NG)");
							}
						}
						Reporting.AddLine("ho = d - dct - dcb = " + beam.Shape.d + " - " + wcBeam.TCopeD + " - " + wcBeam.BCopeD + " = " + ho + ConstUnit.Length);
						Reporting.AddLine("h = d - 2 * k = " + beam.Shape.d + " - 2 * " + beam.Shape.kdes + " = " + h + ConstUnit.Length);
						Reporting.AddLine("Ix = 1 / 12 * b * (ho + 2 * t)³ - 1 / 12 * (b - tw) * ho³");
						Reporting.AddLine("= 1 / 12 * " + B + " * (" + ho + " + 2 * " + t + ")³ - 1 / 12 * (" + B + " - " + beam.Shape.tw + ") * " + ho + "³");
						Reporting.AddLine("= " + ix + " " + ConstUnit.MomentInertia);
						Reporting.AddLine("Snet = Ix / (ho / 2 + t) = " + ix + "/ (" + ho + "/ 2 + " + t + ")");
						if (s >= Sreq)
							Reporting.AddLine("= " + s + " >= " + Sreq + ConstUnit.SecMod + " (OK)");
						else
							Reporting.AddLine("= " + s + " << " + Sreq + ConstUnit.SecMod + " (NG)");

						Reporting.AddHeader("Check slenderness:");
						Reporting.AddLine("Slenderness = (b - tw) / (2 * t) = (" + B + " - " + beam.Shape.tw + ") / (2 * " + t + ")");
						if (slenderness <= 0.38 * Math.Pow(ConstNum.ELASTICITY, 0.5) / sqrtFy)
							Reporting.AddLine("= " + slenderness + " <=  0.38 * (E / Fy)^0.5 = 0.38 * (" + ConstNum.ELASTICITY + " / " + wcBeam.Material.Fy + ")^0.5 = " + 0.38 * Math.Pow(ConstNum.ELASTICITY, 0.5) / Math.Pow(beam.Material.Fy, 0.5) + " (OK)");
						else
							Reporting.AddLine("= " + slenderness + " >>  0.38 * (E / Fy)^0.5 = 0.38 * (" + ConstNum.ELASTICITY + " / " + wcBeam.Material.Fy + ")^0.5 = " + 0.38 * Math.Pow(ConstNum.ELASTICITY, 0.5) / Math.Pow(beam.Material.Fy, 0.5) + " (NG)");
					}
					else if (wcBeam.TopCope || wcBeam.BottomCope)
					{
						Reporting.AddLine("Try: PL" + wcBeam.CopeReinforcement.LengthT + " X " + wcBeam.CopeReinforcement.Width + " X " + wcBeam.CopeReinforcement.t + ConstUnit.Length);
						Reporting.AddLine("ho = d - dct - dcb = " + beam.Shape.d + " - " + wcBeam.TCopeD + " - " + wcBeam.BCopeD + " = " + ho + ConstUnit.Length);
						Reporting.AddLine("h = d - 2 * k = " + beam.Shape.d + " - 2 * " + beam.Shape.kdes + " = " + h + ConstUnit.Length);
						Reporting.AddLine("Af = bf * tf = " + beam.Shape.bf + " * " + beam.Shape.tf + " = " + Af + ConstUnit.Area);
						Reporting.AddLine("Aw = (ho - tf) * tw = (" + ho + " - " + beam.Shape.tf + ") * " + beam.Shape.tw + " = " + Aw + ConstUnit.Area);
						Reporting.AddLine("d1 = (" + Af + " * " + beam.Shape.tf + " / 2 + " + Aw + " * (" + beam.Shape.tf + " + (" + ho + " - " + beam.Shape.tf + ") / 2) + " + B + " * " + t + " * (" + ho + " + " + t + " / 2)) / (" + Af + " + " + Aw + " + " + B + " * " + t + ")");
						Reporting.AddLine("= " + d1 + ConstUnit.Length);
						Reporting.AddLine("Ix = 1 / 12 * bf * tf³ + Af * (d1 - tf / 2)² ");
						Reporting.AddLine("+ 1 / 12 * tw * (ho - tf)³ + Aw * (d1 + tf - (ho - tf) / 2)²");
						Reporting.AddLine("+ 1 / 12 * b * t³ + b * t * (ho + tp / 2 - d1)²");
						Reporting.AddLine(" = 1 / 12 * " + beam.Shape.bf + " * " + beam.Shape.tf + "³ + " + Af + " * (" + d1 + " - " + beam.Shape.tf + " / 2)²");
						Reporting.AddLine("+ 1 / 12 * " + beam.Shape.tw + " * (" + ho + " - " + beam.Shape.tf + ")³ + " + Aw + " * (" + d1 + " - " + beam.Shape.tf + " - (" + ho + " - " + beam.Shape.tf + ") / 2)²");
						Reporting.AddLine("+ 1 / 12 * " + B + " * " + wcBeam.CopeReinforcement.t + "³ + " + B + " * " + t + " * (" + ho + " + " + t + " / 2 - " + d1 + ")²");
						Reporting.AddLine(" = " + ix + " " + ConstUnit.MomentInertia);

						Reporting.AddLine("Snet = Ix / Max(d1; d + t - d1) = " + ix + " / Max(" + d1 + "; " + ho + " + " + t + " - " + d1 + ")");
						if (s >= Sreq)
							Reporting.AddLine("=" + s + " >= " + Sreq + ConstUnit.SecMod + " (OK)");
						else
							Reporting.AddLine("= " + s + " << " + Sreq + ConstUnit.SecMod + " (NG)");

						Reporting.AddHeader("Check slenderness:");
						//slenderness = (B - beam.Shape.tw) / (2 * t);
						Reporting.AddLine("Slenderness = (b - tw) / (2 * t) = (" + B + " - " + beam.Shape.tw + ") / (2 * " + t + ")");
						if (slenderness <= 0.38 * Math.Pow(ConstNum.ELASTICITY, 0.5) / sqrtFy)
							Reporting.AddLine("= " + slenderness + " <= 0.38 * (E / Fy)^0.5 = 0.38 * (" + ConstNum.ELASTICITY + " / " + beam.Material.Fy + ")^0.5 = " + 0.38 * Math.Pow(ConstNum.ELASTICITY, 0.5) / Math.Pow(beam.Material.Fy, 0.5) + " (OK)");
						else
							Reporting.AddLine("= " + slenderness + " >> 0.38 * (E / Fy)^0.5 = 0.38 * (" + ConstNum.ELASTICITY + " / " + beam.Material.Fy + ")^0.5 = " + 0.38 * Math.Pow(ConstNum.ELASTICITY, 0.5) / Math.Pow(beam.Material.Fy, 0.5) + " (NG)");
					}
					Reporting.AddHeader("Check Welds:");
					Reporting.AddHeader("WeldForce = Ru * (be + c) / Snet * b * t");
					Reporting.AddLine("= " + beam.ShearForce + " * (" + beam.EndSetback + " + " + beam.WinConnect.Fema.L + ") / " + s + " * " + B + " * " + t + " = " + weldForce + ConstUnit.Force);

					Reporting.AddLine("Maximum Useful Weld Size = Min((tw * Fu / (Fexx * 0.707 * 2); (t * Fu / (Fexx * 0.707))");
					Reporting.AddLine("Min(" + beam.Shape.tw + " * " + beam.Material.Fu + " / (" + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * 0.707 * 2); (" + t + " * " + beam.Material.Fu + " / (" + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * 0.707))");
					Reporting.AddLine("Min(" + maxWeldTest1 + ", " + maxWeldTest2 + ")");
					Reporting.AddLine("= " + maxWeld);

					Reporting.AddLine("Minimum Weld Size = " + minWeld + ConstUnit.Length);
					Reporting.AddLine("Use " + wcBeam.CopeReinforcement.WeldSize + ConstUnit.Length + " weld for strength calculation");
					Reporting.AddLine("Weld Strength, " + ConstString.PHI + "Rn = " + ConstString.FIOMEGA0_75 + " w * 2 * 0.6 * Fexx * 0.707 * c");
					Reporting.AddLine(ConstString.FIOMEGA0_75 + " * " + wcBeam.CopeReinforcement.WeldSize + " * 2 * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * 0.707 * " + beam.WinConnect.Fema.L);
					if (weldStrength > weldForce)
					{
						Reporting.AddCapacityLine(" = " + weldStrength + " > " + weldForce + " (OK)", weldForce / weldStrength, "Weld Strength", memberType);
						wcBeam.CopeReinforcement.WeldSize = minWeld;
						Reporting.AddLine("Use " + wcBeam.CopeReinforcement.WeldSize + ConstUnit.Length + " welds for cope reinforcement plate.");
					}
					else
					{
						Reporting.AddCapacityLine(" = " + weldStrength + " < " + weldForce + " (NG)", weldForce / weldStrength, "Weld Strength", memberType);
						Reporting.AddLine("Sufficient weld strength not possible with current beam web thickness.");
					}
				}
			}

			ho = h_connector;
			t = Math.Max(6 * Sreq / Math.Pow(ho, 2), A_req / ho);
			t = t / 2; // use two plates
			beam.WinConnect.Fema.L = (L_Cope + d_Cope);
			minThickness = ho * Math.Pow(beam.Material.Fy, 0.5) / (2.45 * Math.Pow(ConstNum.ELASTICITY, 0.5));
			if (t < minThickness)
				t = minThickness;
			t = CommonCalculations.PlateThickness(t);

			X1 = wcBeam.FrontX;
			y1 = wcBeam.FrontY + beam.Shape.d / 2 - wcBeam.TCopeD - wcBeam.WebHeight / 2 - ho / 2;
			x2 = (X1 + Math.Sign(X1) * beam.WinConnect.Fema.L);
			y2 = y1 + ho;

			wcBeam.CopeReinforcement.XF1 = X1;
			wcBeam.CopeReinforcement.XF2 = x2;
			wcBeam.CopeReinforcement.YF1 = y1;
			wcBeam.CopeReinforcement.YF2 = y2;
			wcBeam.CopeReinforcement.t = t;
			wcBeam.CopeReinforcement.Width = ho;
			wcBeam.CopeReinforcement.LengthT = beam.WinConnect.Fema.L;
			wcBeam.CopeReinforcement.Type = EReinforcementType.DoublerPlate;
		}
	}
}
