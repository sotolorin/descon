using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class SeismicForceCalc
	{
		internal static void TransferForceAndBeamFx(EMemberType memberType)
		{
			double Hb4C;
			double Hb4T;
			double Hb3C;
			double Hb3T;

			var beam = new DetailData();
			var upperBrace = new DetailData();
			var lowerBrace = new DetailData();

			switch (memberType)
			{
				case EMemberType.RightBeam:
				case EMemberType.UpperRight:
				case EMemberType.LowerRight:
					beam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
					upperBrace = CommonDataStatic.DetailDataDict[EMemberType.UpperRight];
					lowerBrace = CommonDataStatic.DetailDataDict[EMemberType.LowerRight];
					break;
				case EMemberType.LeftBeam:
				case EMemberType.UpperLeft:
				case EMemberType.LowerLeft:
					beam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
					upperBrace = CommonDataStatic.DetailDataDict[EMemberType.UpperLeft];
					lowerBrace = CommonDataStatic.DetailDataDict[EMemberType.LowerLeft];
					break;
			}

			if (upperBrace.IsActive)
			{
				Hb3T = upperBrace.AxialTension * Math.Cos(upperBrace.Angle * ConstNum.RADIAN);
				Hb3C = -upperBrace.AxialCompression * Math.Cos(upperBrace.Angle * ConstNum.RADIAN);
			}
			else
			{
				Hb3T = 0;
				Hb3C = 0;
			}
			if (lowerBrace.IsActive)
			{
				Hb4T = lowerBrace.AxialTension * Math.Cos(lowerBrace.Angle * ConstNum.RADIAN);
				Hb4C = -lowerBrace.AxialCompression * Math.Cos(lowerBrace.Angle * ConstNum.RADIAN);
			}
			else
			{
				Hb4T = 0;
				Hb4C = 0;
			}

			if (CommonDataStatic.Preferences.InputForceType == EPrefsInputForce.TransferForce)
			{
				beam.AxialTension = beam.AxialTension = Math.Max(Math.Abs(Hb3T), Math.Abs(Hb4T)) + beam.TransferTension;
				beam.AxialCompression = beam.AxialCompression = Math.Max(Math.Abs(Hb3C), Math.Abs(Hb4C)) + beam.TransferCompression;
			}
			else
			{
				beam.AxialTension = beam.AxialTension;
				beam.AxialCompression = beam.AxialCompression;
				beam.TransferTension = Math.Max(Math.Abs(Hb3T), Math.Abs(Hb4T)) - beam.AxialTension;
				beam.TransferCompression = Math.Max(Math.Abs(Hb3C), Math.Abs(Hb4C)) - beam.AxialCompression;
			}

			Reporting.AddHeader("Transfer Force and Beam Fx");
			Reporting.AddLine(String.Format("Beam Compression: {0} {1}", beam.AxialCompression, ConstUnit.Force));
			Reporting.AddLine(String.Format("Beam Tension: {0} {1}", beam.AxialTension, ConstUnit.Force));
			Reporting.AddLine(String.Format("Upper Brace Compression: {0} {1}", upperBrace.AxialTension, ConstUnit.Force));
			Reporting.AddLine(String.Format("Upper Brace Tension: {0} {1}", upperBrace.AxialTension, ConstUnit.Force));
			Reporting.AddLine(String.Format("Lower Brace Compression: {0} {1}", lowerBrace.AxialTension, ConstUnit.Force));
			Reporting.AddLine(String.Format("Lower Brace Tension: {0} {1}", lowerBrace.AxialTension, ConstUnit.Force));
		}

		public static void CalculateSeismicForces(EMemberType memberType)
		{
			double Fcr = 0;
			double Fe = 0;
			double KLovR = 0;
			double R = 0;
			double L = 0;
			double MomInert = 0;
			double rysA = 0;
			double rxsA = 0;
			double EnKucukMomInertia = 0;
			double rw = 0;
			double rz = 0;
			double ry = 0;
			double rx = 0;
			double Iw = 0;
			double Ip = 0;
			double Iz = 0;
			double Iy = 0;
			double Ix = 0;
			double Mp = 0;
			double Moment14 = 0;
			double Force11 = 0;
			double Force10 = 0;
			double Force12 = 0;
			double Pn = 0;
			double ForceB = 0;
			double BraceForce = 0;
			double ForceU = 0;
			double Force4 = 0;
			double Force3 = 0;
			double Force2 = 0;
			double Force1 = 0;
			double BoltSlipFrc = 0;
			double r_y = 0;
			double Area;

			var component = CommonDataStatic.DetailDataDict[memberType];
			if (!component.IsActive || (MiscMethods.IsBrace(memberType) && component.Length == 0))
			{
				component.AxialCompression = 0;
				component.AxialTension = 0;
				return;
			}

			Area = component.Shape.a;

			if (component.MemberType == EMemberType.LowerLeft)
				Area *= 2;

			r_y = component.Material.Ry;

			if (CommonDataStatic.SeismicSettings.Response == EResponse.RGreaterThan3)
			{
				// This logic uses information in a Seismic form that is not currently in Descon 8 (SeismicForceCalculation.xaml). We will add it if needed.
				switch (CommonDataStatic.Preferences.BracingType)
				{
					case EBracingType.OCBF:
						Force1 = r_y * component.Material.Fy * Area; // LRFD
						Force2 = Force1 / 1.5; // ASD
						switch (CommonDataStatic.Preferences.CalcMode)
						{
							case ECalcMode.LRFD:
								if (Force3 == 0 && Force4 == 0)
									ForceU = Force1;
								else if (Force3 == 0)
									ForceU = Math.Min(Force1, Force4);
								else if (Force4 == 0)
									ForceU = Math.Min(Force1, Force3);
								if (BoltSlipFrc == 0)
									BoltSlipFrc = ForceU;
								break;
							case ECalcMode.ASD:
								if (Force3 == 0 && Force4 == 0)
									ForceU = Force2;
								else if (Force3 == 0)
									ForceU = Math.Min(Force2, Force4);
								else if (Force4 == 0)
									ForceU = Math.Min(Force2, Force3);
								if (BoltSlipFrc == 0)
									BoltSlipFrc = ForceU;
								break;
						}
						BraceForce = ForceU;
						BoltSlipFrc = ForceB;
						Pn = BraceBucklingStrength(memberType) * Area;
						switch (CommonDataStatic.Preferences.CalcMode)
						{
							case ECalcMode.LRFD:
								component.AxialCompression = ForceU;
								component.AxialTension = ForceU;
								break;
							case ECalcMode.ASD:
								component.AxialCompression = ForceU;
								component.AxialTension = ForceU;
								break;
						}
						break;
					case EBracingType.SCBF:
						switch (CommonDataStatic.Preferences.CalcMode)
						{
							case ECalcMode.LRFD:
								Force10 = r_y * component.Material.Fy * Area;
								if (Force12 > 0)
									ForceU = Math.Min(Force10, Force12);
								else
									ForceU = Force10;
								break;
							case ECalcMode.ASD:
								Force11 = r_y * component.Material.Fy * Area / 1.5;
								if (Force12 > 0)
									ForceU = Math.Min(Force11, Force12);
								else
									ForceU = Force11;
								break;
						}
						component.AxialTension = ForceU;
						Moment14 = 1.1 * r_y * Mp;
						MomentOfInertia(memberType, ref Ix, ref Iy, ref Iz, ref Ip, ref Iw, ref rx, ref ry, ref rz, ref rw, ref EnKucukMomInertia, ref rxsA, ref rysA);
						MomInert = EnKucukMomInertia;
						L = component.Length * 12;
						R = Math.Pow(MomInert / component.Shape.a, 0.5);
						KLovR = L / R; // k=1
						Fe = Math.Pow(Math.PI, 2) * ConstNum.ELASTICITY / Math.Pow(KLovR, 2);
						if (Fe >= 0.44 * component.Material.Fy)
							Fcr = Math.Pow(0.658, component.Material.Fy / Fe) * component.Material.Fy;
						else
							Fcr = 0.877 * Fe;

						Pn = Fcr * Area;
						Pn = BraceBucklingStrength(memberType) * Area;
						switch (CommonDataStatic.Preferences.CalcMode)
						{
							case ECalcMode.LRFD:
								component.AxialCompression = 1.1 * r_y * Pn;
								break;
							case ECalcMode.ASD:
								component.AxialCompression = 1.1 / 1.5 * r_y * Pn;
								break;
						}
						break;
				}
			}
			else
			{
				Pn = BraceBucklingStrength(memberType) * Area;
				if (!component.AxialTension_User)
					component.AxialTension = ConstNum.FIOMEGA0_9N * component.Material.Fy * Area;
				if (!component.AxialCompression_User)
					component.AxialCompression = ConstNum.FIOMEGA0_9N * Pn;
			}
		}

		private static double BraceBucklingStrength(EMemberType memberType)
		{
			double BraceBucklingStrength = 0;

			double dw = 0;
			double Aditional = 0;
			int Kz = 0;
			double Ag = 0;
			double r0b2 = 0;
			double Ixp = 0;
			double y0 = 0;
			int x0 = 0;
			double Pn = 0;
			double be2 = 0;
			double be1 = 0;
			double b = 0;
			double FpPrev = 0;
			double fp = 0;
			double Fe_flx = 0;
			double KLovRm = 0;
			double H1 = 0;
			double x_ = 0;
			double tg = 0;
			double rib = 0;
			double ri = 0;
			double a = 0;
			double Fcr_ft = 0;
			double Fcrz = 0;
			double Fcry = 0;
			double Fcr = 0;
			double Fe = 0;
			double Fez = 0;
			double Fey = 0;
			double H = 0;
			double ro = 0;
			double yo = 0;
			int xo = 0;
			double Q = 0;
			double Qa = 0;
			double Aeff = 0;
			double be = 0;
			double Qs = 0;
			double tw = 0;
			double d = 0;
			double tf = 0;
			double BF = 0;
			double yy = 0;
			double KLOvRx = 0;
			double KLovRy = 0;
			double KLovR = 0;
			double L = 0;
			double R = 0;
			double r2 = 0;
			double r1 = 0;
			double Area = 0;
			double G_ = 0;
			double Qw = 0;
			double Qf = 0;
			double Sw = 0;
			double alpha = 0;
			double Cw = 0;
			double j = 0;
			double rysA = 0;
			double rxsA = 0;
			double EnKucukMomInertia = 0;
			double rw = 0;
			double rz = 0;
			double ry = 0;
			double rx = 0;
			double Iw = 0;
			double Ip = 0;
			double Iz = 0;
			double Iy = 0;
			double Ix = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];

			if (component.KBrace || component.KneeBrace)
				CommonDataStatic.Preferences.Seismic = ESeismic.NonSeismic;

			MomentOfInertia(memberType, ref Ix, ref Iy, ref Iz, ref Ip, ref Iw, ref rx, ref ry, ref rz, ref rw, ref EnKucukMomInertia, ref rxsA, ref rysA);
			TorsionProperties(memberType, Ix, Iy, ref j, ref Cw, ref alpha, ref component.Wno, ref Sw, ref Qf, ref Qw);

			G_ = CommonDataStatic.Units == EUnit.US ? 11200 : 77200;

			Area = component.Shape.a;
			if (rz > 0 && rw > 0)
				r1 = Math.Min(rz, rw);
			else
				r1 = 0;
			r2 = Math.Min(rx, ry);

			if (r1 == 0)
				R = r2;
			else
				R = Math.Min(r1, r2);
			if (CommonDataStatic.Units == EUnit.US)
				L = component.Length * 12;
			else
				L = component.Length * 1000;

			KLovR = 1 * L / R;
			KLovRy = 1 * L / ry;
			KLOvRx = 1 * L / rx;
			component.Material.Fy = component.Material.Fy;
			yy = Math.Pow(ConstNum.ELASTICITY / component.Material.Fy, 0.5);
			// Slender Elements
			BF = component.Shape.bf;
			tf = component.Shape.tf;
			d = component.Shape.d;
			tw = component.Shape.tw;
			component.Slender = false;
			switch (component.ShapeType)
			{
				case EShapeType.WideFlange:
					if (BF / (2 * tf) > 0.56 * yy || (d - 2 * component.Shape.kdes) / tw > 1.49 * yy)
					{
						component.Slender = true;
						if (BF / (2 * tf) <= 0.56 * yy)
							Qs = 1;
						else if (BF / (2 * tf) <= 1.03 * yy)
							Qs = 1.415 - 0.74 * (BF / (2 * tf)) / yy;
						else
							Qs = 0.69 * ConstNum.ELASTICITY / (component.Material.Fy * Math.Pow(BF / (2 * tf), 2));

						if ((d - 2 * component.Shape.kdes) / tw > 1.4 * Math.Pow(ConstNum.ELASTICITY / component.Material.Fy, 0.5))
						{
							be = 1.92 * tw * Math.Pow(ConstNum.ELASTICITY / component.Material.Fy, 0.5) * (1 - 0.38 / ((d - 2 * component.Shape.kdes) / tw) * Math.Pow(ConstNum.ELASTICITY / component.Material.Fy, 0.5));
							be = Math.Min(be, d - 2 * component.Shape.kdes);
							Aeff = Area - (d - 2 * component.Shape.kdes - be) * tw;
							Qa = Aeff / Area;
						}
						Q = Qs * Qa;
					}
					break;
				case EShapeType.WTSection:
					xo = 0;
					yo = (BF * Math.Pow(tf, 2) / 2 + (d - tf) * tw * ((d - tf) / 2 + tf) - tf / 2) / Area;
					ro = Math.Pow(Math.Pow(xo, 2) + Math.Pow(yo, 2) + (Ix + Iy) / Area, 0.5);
					H = 1 - Math.Pow(yo / ro, 2);
					if (d / tw > 0.75 * yy || BF / (2 * tf) > 1 * yy)
					{
						component.Slender = true;
						Fey = Math.Pow(Math.PI, 2) * ConstNum.ELASTICITY / Math.Pow(KLovRy, 2); // Eq E4-10
						Fez = G_ * j / (Area * Math.Pow(ro, 2)); // Eq E4-11
						Fe = (Fey + Fez) / (2 * H) * (1 - Math.Pow(1 - 4 * Fey * Fez * H / Math.Pow(Fey + Fez, 2), 0.5)); // Eq E4-5
						// Fe = Min(Fe, pi ^ 2 * YMEn / (KLOvRx) ^ 2) 'Eq E3.4
						if (d / tw <= 0.75 * yy)
							Qs = 1;
						else if (d / tw <= 1.03 * yy)
							Qs = 1.908 - 1.22 * (d / tw) / yy;
						else if (d / tw > 1.03 * yy)
							Qs = 0.69 * ConstNum.ELASTICITY / (component.Material.Fy * Math.Pow(d / tw, 2));
						Q = Qs;
						if (Fe >= 0.44 * Q * component.Material.Fy)
							Fcr = Q * Math.Pow(0.658, Q * component.Material.Fy / Fe) * component.Material.Fy;
						else
							Fcr = 0.877 * Fe;
					}
					else
					{
						component.Slender = false;
						// Flexure
						Fe = Math.Pow(Math.PI, 2) * ConstNum.ELASTICITY / Math.Pow(KLOvRx, 2); // EqE 3.4
						if (KLOvRx <= 4.71 * Math.Pow(ConstNum.ELASTICITY / component.Material.Fy, 0.5))
							Fcr = Math.Pow(0.658, component.Material.Fy / Fe) * component.Material.Fy; // Eq E3-2
						else
							Fcr = 0.877 * Fe; // Eq E3-3
						// Torsion or Flexure and Torsion
						Fe = Math.Pow(Math.PI, 2) * ConstNum.ELASTICITY / Math.Pow(KLovRy, 2); // EqE 3.4
						if (KLovRy <= 4.71 * Math.Pow(ConstNum.ELASTICITY / component.Material.Fy, 0.5))
							Fcry = Math.Pow(0.658, component.Material.Fy / Fe) * component.Material.Fy; // Eq E3-2
						else
							Fcry = 0.877 * Fe; // Eq E3-3
						Fcrz = G_ * j / (Area * Math.Pow(ro, 2)); // Eq E4-3
						Fcr_ft = (Fcry + Fcrz) / (2 * H) * (1 - Math.Pow(1 - 4 * Fcry * Fcrz * H / Math.Pow(Fcry + Fcrz, 2), 0.5)); // Eq E4-2
						Fcr = Math.Min(Fcr, Fcr_ft);
					}
					BraceBucklingStrength = Fcr;
					return BraceBucklingStrength;
				case EShapeType.SingleAngle:
					if (Math.Max(BF, d) / tf > 0.45 * yy)
					{
						component.Slender = true;
						if (Math.Max(BF, d) / tf <= 0.91 * yy)
							Qs = 1.34 - 0.76 * Math.Max(BF, d) / tf / yy;
						else if ((Math.Max(BF, d) / tf) > 0.91 * yy)
							Qs = 0.53 * ConstNum.ELASTICITY / (component.Material.Fy * Math.Pow(Math.Max(BF, d) / tf, 2));
					}
					else
						Qs = 1;
					break;
				case EShapeType.DoubleAngle:
					a = 3 * rz * KLOvRx / 4;
					ri = Math.Min(Math.Min(rz, rw), Math.Min(rxsA, rysA));
					rib = rysA;
					tg = 0.75;
					x_ = (d * Math.Pow(tf, 2) / 2 + (BF - tf) * tf * ((BF - tf) / 2 + tf)) / (component.Shape.a / 2);
					H1 = 2 * x_ + tg;
					alpha = H1 / (2 * rib);
					KLovRm = Math.Pow(Math.Pow(KLovRy, 2) + 0.82 * Math.Pow(alpha / (1 + alpha), 2) * Math.Pow(a / rib, 2), 0.5);
					xo = 0;
					yo = (BF * Math.Pow(tf, 2) / 2 + (d - tf) * tf * ((d - tf) / 2 + tf)) / Area; // - tf / 2
					ro = Math.Pow(Math.Pow(xo, 2) + Math.Pow(yo, 2) + (Ix + Iy) / Area, 0.5);
					H = 1 - Math.Pow(yo / ro, 2);
					if (Math.Max(BF, d) / tf > 0.45 * yy)
					{
						component.Slender = true;
						if (Math.Max(BF, d) / tf <= 0.91 * yy)
							Qs = 1.34 - 0.76 * Math.Max(BF, d) / tf / yy; // E7-11
						else if ((Math.Max(BF, d) / tf) > 0.91 * yy)
							Qs = 0.53 * ConstNum.ELASTICITY / (component.Material.Fy * Math.Pow(Math.Max(BF, d) / tf, 2)); // E7-12

						Q = Qs;
						// flexure
						Fe_flx = Math.Pow(Math.PI, 2) * ConstNum.ELASTICITY / Math.Pow(KLOvRx, 2); // Eq E3.4
						// Torsion + Flexure and Torsion
						Fey = Math.Pow(Math.PI, 2) * ConstNum.ELASTICITY / Math.Pow(KLovRm, 2); // Eq E4-10
						j = 2 * (BF + d - tf) * Math.Pow(tf, 3) / 3;
						Fez = G_ * j / (Area * Math.Pow(ro, 2)); // Eq E4-11***** ????
						Fcr_ft = (Fey + Fez) / (2 * H) * (1 - Math.Pow(1 - 4 * Fey * Fez * H / Math.Pow(Fey + Fez, 2), 0.5)); // Eq E4-5
						Fe = Math.Min(Fe_flx, Fcr_ft);
						if (Fe >= 0.44 * Q * component.Material.Fy)
							Fcr = Q * Math.Pow(0.658, Q * component.Material.Fy / Fe) * component.Material.Fy;
						else
							Fcr = 0.877 * Fe;
					}
					else
					{
						component.Slender = false;
						// Flexure
						Fe = Math.Pow(Math.PI, 2) * ConstNum.ELASTICITY / Math.Pow(KLOvRx, 2); // EqE 3.4
						if (KLOvRx <= 4.71 * Math.Pow(ConstNum.ELASTICITY / component.Material.Fy, 0.5))
							Fcr = Math.Pow(0.658, component.Material.Fy / Fe) * component.Material.Fy; // Eq E3-2
						else
							Fcr = 0.877 * Fe; // Eq E3-3

						// Torsion or Flexure and Torsion
						Fe = Math.Pow(Math.PI, 2) * ConstNum.ELASTICITY / Math.Pow(KLovRm, 2); // EqE 3.4
						if (KLovRy <= 4.71 * Math.Pow(ConstNum.ELASTICITY / component.Material.Fy, 0.5))
							Fcry = Math.Pow(0.658, component.Material.Fy / Fe) * component.Material.Fy; // Eq E3-2
						else
							Fcry = 0.877 * Fe; // Eq E3-3
						j = 2 * (BF + d - tf) * Math.Pow(tf, 3) / 3;
						Fcrz = G_ * j / (Area * Math.Pow(ro, 2)); // Eq E4-3
						Fcr_ft = (Fcry + Fcrz) / (2 * H) * (1 - Math.Pow(1 - 4 * Fcry * Fcrz * H / Math.Pow(Fcry + Fcrz, 2), 0.5)); // Eq E4-2
						Fcr = Math.Min(Fcr, Fcr_ft);
					}
					BraceBucklingStrength = Fcr;
					return BraceBucklingStrength;
				case EShapeType.HollowSteelSection:
					Aeff = component.Shape.a;
					Q = 1;
					fp = component.Material.Fy;
					do
					{
						FpPrev = fp;
						Fe = Math.Pow(Math.PI, 2) * ConstNum.ELASTICITY / Math.Pow(KLovR, 2);
						KLovR = 1 * L / R;
						if (KLovR <= 4.71 * Math.Pow(ConstNum.ELASTICITY / (Q * component.Material.Fy), 0.5))
							Fcr = Q * Math.Pow(0.658, Q * component.Material.Fy / Fe) * component.Material.Fy;
						else
							Fcr = 0.877 * Fe;
						b = BF - 2 * (1.5 * tf);
						H = d - 2 * (1.5 * tf);
						if (b / tf >= 1.4 * Math.Pow(ConstNum.ELASTICITY / fp, 0.5) || H / tf >= 1.4 * Math.Pow(ConstNum.ELASTICITY / fp, 0.5))
						{
							component.Slender = true;
							if (b / tf >= 1.4 * Math.Pow(ConstNum.ELASTICITY / fp, 0.5))
								be1 = Math.Min(1.92 * tf * Math.Pow(ConstNum.ELASTICITY / fp, 0.5) * (1 - 0.38 / (b / tf) * Math.Pow(ConstNum.ELASTICITY / fp, 0.5)), b);
							else
								be1 = b;
							if (H / tf > 1.4 * Math.Pow(ConstNum.ELASTICITY / fp, 0.5))
								be2 = Math.Min(1.92 * tf * Math.Pow(ConstNum.ELASTICITY / fp, 0.5) * (1 - 0.38 / (H / tf) * Math.Pow(ConstNum.ELASTICITY / fp, 0.5)), H);
							else
								be2 = H;
							Aeff = 2 * (be1 + be2) * tf;
							Qa = Aeff / component.Shape.a;
						}
						else
							Qa = 1;
						Q = Qa;
						Pn = Fcr * Aeff;
						fp = Pn / Aeff;
					} while (fp != FpPrev);
					if (component.Shape.TypeEnum == EShapeType.HollowSteelSection)
					{
						if (d / tf > 0.11 * ConstNum.ELASTICITY / component.Material.Fy && d / tf < 0.45 * ConstNum.ELASTICITY / component.Material.Fy)
							Qa = Math.Min(1, 0.038 * ConstNum.ELASTICITY / (component.Material.Fy * (d / tf)) + 2 / 3);
					}
					break;
				case EShapeType.SingleChannel:
					if (BF / tf > 0.56 * yy || (d - 2 * tf) / tw > 1.49 * yy)
						component.Slender = true;
					break;
				case EShapeType.DoubleChannel:
					if (BF / tf > 0.56 * yy)
						component.Slender = true;
					break;
			}
			if (Qa > 0 && Qs > 0)
				Q = Qa * Qs;
			else if (Qa > 0)
				Q = Qa;
			else if (Qs > 0)
				Q = Qs;
			else
				Q = 1;

			Fe = Math.Pow(Math.PI, 2) * ConstNum.ELASTICITY / Math.Pow(KLovR, 2); // EqE 3.4

			if (KLovR <= 4.71 * Math.Pow(ConstNum.ELASTICITY / (Q * component.Material.Fy), 0.5))
				Fcr = Math.Pow(0.658, Q * component.Material.Fy / Fe) * component.Material.Fy;
			else
				Fcr = 0.877 * Fe;

			if (!component.Slender)
				goto NotSlender;

			Fe = Math.Pow(Math.PI, 2) * ConstNum.ELASTICITY / Math.Pow(KLovR, 2);
			// E3
			if (KLovR <= 4.71 * Math.Pow(ConstNum.ELASTICITY / (Q * component.Material.Fy), 0.5))
				Fcr = Q * Math.Pow(0.658, Q * component.Material.Fy / Fe) * component.Material.Fy;
			else
				Fcr = 0.877 * Fe;

			// E4
			switch (component.ShapeType)
			{
				case EShapeType.WTSection:
				case EShapeType.DoubleAngle:
				case EShapeType.SingleChannel:
					// E.4 a
					KLovR = 1 * L / ry;
					Fe = Math.Pow(Math.PI, 2) * ConstNum.ELASTICITY / Math.Pow(KLovR, 2);
					if (KLovR <= 4.71 * Math.Pow(ConstNum.ELASTICITY / component.Material.Fy, 0.5))
						Fcry = Math.Pow(0.658, component.Material.Fy / Fe) * component.Material.Fy;
					else
						Fcry = 0.877 * Fe;
					if (memberType == EMemberType.RightBeam)
						j = (component.Shape.bf * Math.Pow(component.Shape.tf, 3) + (component.Shape.d - component.Shape.tf) * Math.Pow(component.Shape.tw, 3)) / 3; // Tee
					else
						j = 2 * (component.Shape.bf + component.Shape.d - component.Shape.tf) * Math.Pow(component.Shape.tf, 3) / 3; // 2L

					ro = Math.Pow(Ip / component.Shape.a, 0.5);
					ro = 2.45;
					x0 = 0;
					y0 = Math.Pow((Ix - Ixp) / component.Shape.a, 0.5);
					r0b2 = Math.Pow(x0, 2) + Math.Pow(y0, 2) + (Ix + Iy) / component.Shape.a;
					H = 1 - (Math.Pow(x0, 2) + Math.Pow(y0, 2)) / Math.Pow(r0b2, 2);
					Ag = component.Shape.a;
					Fcrz = G_ * j / (Ag * r0b2);
					Fcr = (Fcry + Fcrz) / (2 * H) * (1 - Math.Pow(1 - 4 * Fcry * Fcrz * H / Math.Pow(Fcry + Fcrz, 2), 0.5));
					break;
				case EShapeType.HollowSteelSection:
				case EShapeType.WideFlange:
				case EShapeType.DoubleChannel:
					// E.4 b, i
					Kz = 1;
					Cw = Iy * Math.Pow(Math.Max(BF, d) - tf, 2) / 4;
					Fe = Math.Pow(Math.PI, 2) * ConstNum.ELASTICITY / Math.Pow(KLovR, 2);
					KLovR = 1 * L / R;
					if (KLovR <= 4.71 * Math.Pow(ConstNum.ELASTICITY / (Q * component.Material.Fy), 0.5))
						Fcr = Q * Math.Pow(0.658, Q * component.Material.Fy / Fe) * component.Material.Fy;
					else
						Fcr = 0.877 * Fe;
					Qa = 1;
					break;
			}
			// E5 Single Angles
			// (a)
			if (d == BF || d > BF)
			{
				if (L / R <= 80)
					KLovR = 72 + 0.75 * L / R;
				else
					KLovR = Math.Min(32 + 1.25 * L / R, 200);
			}
			else
			{
				if (BF / d < 1.7)
				{
					Aditional = 4 * (Math.Pow(BF / d, 2) - 1);
					if (L / R <= 80)
						KLovR = 72 + 0.75 * L / R + Aditional;
					else
						KLovR = Math.Min(32 + 1.25 * L / R, 200) + Aditional;
				}
				switch (component.ShapeType)
				{
					case EShapeType.WideFlange:
						if (BF / (2 * tf) >= 1.49 * yy)
							be1 = Math.Min(1.92 * tf * Math.Pow(ConstNum.ELASTICITY / Fcr, 0.5) * (1 - 0.34 / (BF / (2 * tf)) * yy), BF / 2);
						else
							be1 = BF / 2;
						dw = d - 2 * component.Shape.kdes;
						if (dw / (2 * tw) >= 1.49 * yy)
							be2 = Math.Min(1.92 * tw * Math.Pow(ConstNum.ELASTICITY / Fcr, 0.5) * (1 - 0.34 / (dw / (2 * tw)) * yy), dw / 2);
						else
							be2 = dw;
						Aeff = 4 * be1 * tf + be2 * tw;
						Qa = Aeff / component.Shape.a;
						Q = Qa;
						break;
					case EShapeType.WTSection:
						if (BF / (2 * tf) >= 1.49 * yy)
							be1 = Math.Min(1.92 * tf * Math.Pow(ConstNum.ELASTICITY / Fcr, 0.5) * (1 - 0.34 / (BF / (2 * tf)) * yy), BF / 2);
						else
							be1 = BF / 2;
						dw = d - 2 * component.Shape.kdes;
						if (dw / (2 * tw) >= 1.49 * yy)
							be2 = Math.Min(1.92 * tw * Math.Pow(ConstNum.ELASTICITY / Fcr, 0.5) * (1 - 0.34 / (dw / (2 * tw)) * yy), dw / 2);
						else
							be2 = dw;
						Aeff = be1 * tf + be2 * tw;
						Qa = Aeff / component.Shape.a;
						break;
					case EShapeType.SingleAngle:
						// Commented out in Descon 7:
						//if (BF / tf >= 1.49 * yy)
						//	be1 = Math.Min(1.92 * tf * Math.Pow(ConstNum.ELASTICITY / Fcr, 0.5) * (1 - 0.34 / (BF / tf) * yy), BF);
						//else
						//	be1 = BF;
						//if (d / tf >= 1.49 * yy)
						//	be2 = Math.Min(1.92 * tf * Math.Pow(ConstNum.ELASTICITY / Fcr, 0.5) * (1 - 0.34 / (d / (2 * tf)) * yy), d / 2);
						//else
						//	be2 = d;
						//Aeff = (be1 + be2) * tf;
						//Qa = Aeff / component.Shape.a;
						break;
					case EShapeType.DoubleAngle:
						if (BF / tf >= 1.49 * yy)
							be1 = Math.Min(1.92 * tf * Math.Pow(ConstNum.ELASTICITY / Fcr, 0.5) * (1 - 0.34 / (BF / tf) * yy), BF);
						else
							be1 = BF;
						if (d / tf >= 1.49 * yy)
							be2 = Math.Min(1.92 * tf * Math.Pow(ConstNum.ELASTICITY / Fcr, 0.5) * (1 - 0.34 / (d / (2 * tf)) * yy), d / 2);
						else
							be2 = d;
						Aeff = 2 * (be1 + be2) * tf;
						Qa = Aeff / component.Shape.a;
						break;
					case EShapeType.SingleChannel:
						if (BF / (2 * tf) >= 1.49 * yy)
							be1 = Math.Min(1.92 * tf * Math.Pow(ConstNum.ELASTICITY / Fcr, 0.5) * (1 - 0.34 / (BF / (2 * tf)) * yy), BF / 2);
						else
							be1 = BF / 2;
						dw = d - 2 * component.Shape.kdes;
						if (dw / (2 * tw) >= 1.49 * yy)
							be2 = Math.Min(1.92 * tw * Math.Pow(ConstNum.ELASTICITY / Fcr, 0.5) * (1 - 0.34 / (dw / (2 * tw)) * yy), dw / 2);
						else
							be2 = dw;
						Aeff = 2 * be1 * tf + be2 * tw;
						Qa = Aeff / component.Shape.a;
						break;
					case EShapeType.DoubleChannel:
						if (BF / (2 * tf) >= 1.49 * yy)
							be1 = Math.Min(1.92 * tf * Math.Pow(ConstNum.ELASTICITY / Fcr, 0.5) * (1 - 0.34 / (BF / (2 * tf)) * yy), BF / 2);
						else
							be1 = BF / 2;
						dw = d - 2 * component.Shape.kdes;
						if (dw / (2 * tw) >= 1.49 * yy)
							be2 = Math.Min(1.92 * tw * Math.Pow(ConstNum.ELASTICITY / Fcr, 0.5) * (1 - 0.34 / (dw / (2 * tw)) * yy), dw / 2);
						else
							be2 = dw;
						Aeff = 4 * be1 * tf + be2 * tw;
						Qa = Aeff / component.Shape.a;
						break;
				}
			}
			if (Qa == 0)
				Qa = 1;
			if (Qs == 0)
				Qs = 1;
			BraceBucklingStrength = Qs * Qa * Math.Min(Fcr, component.Material.Fy);

			return BraceBucklingStrength;
			NotSlender:
			return Math.Min(Fcr, component.Material.Fy);
		}

		private static void MomentOfInertia(EMemberType memberType, ref double Ix, ref double Iy, ref double Iz, ref double Ip, ref double Iw, ref double rx, ref double ry, ref double rz, ref double rw, ref double EnKucukMomInertia, ref double rxsA, ref double rysA)
		{
			double xc = 0;
			double H = 0;
			double Ixp = 0;
			double tg = 0;
			double Theta = 0;
			double y0 = 0;
			double x0 = 0;
			double k = 0;
			double c = 0;
			double a = 0;
			double t = 0;
			double b = 0;
			double ar = 0;
			double Yc = 0;
			double d = 0;
			double BF = 0;
			double tw = 0;
			double tf = 0;
			double Area = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];

			Area = component.Shape.a;
			if (memberType == EMemberType.LowerLeft)
				Area *= 2;

			switch (component.ShapeType)
			{
				case EShapeType.WideFlange:
					tf = component.Shape.tf;
					tw = component.Shape.tw;
					BF = component.Shape.bf;
					d = component.Shape.d;
					Ix = (BF * Math.Pow(d, 3) - (BF - tw) * Math.Pow(d - 2 * tf, 3)) / 12;
					Iy = (2 * tf * Math.Pow(BF, 3) + (d - 2 * tf) * Math.Pow(tw, 3)) / 12;
					EnKucukMomInertia = Iy;
					break;
				case EShapeType.WTSection:
					tf = component.Shape.tf;
					tw = component.Shape.tw;
					BF = component.Shape.bf;
					d = component.Shape.d;
					Yc = (BF * tf * (d - tf / 2) + tw * Math.Pow(d - tf, 2) / 2) / component.Shape.a;
					Ix = Math.Pow(d - tf, 3) * tw / 12 + (d - tf) * tw * Math.Pow(Yc - (d - tf) / 2, 2) + BF * Math.Pow(tf, 3) / 12 + BF * tf * Math.Pow(d - tf / 2 - Yc, 2);
					Iy = (Math.Pow(BF, 3) * tf + (d - tf) * Math.Pow(tw, 3)) / 12;
					EnKucukMomInertia = Math.Min(Ix, Iy);
					break;
				case EShapeType.SingleAngle:
				case EShapeType.DoubleAngle:
					if (memberType == EMemberType.UpperRight)
						ar = component.Shape.a / 2;
					else
						ar = component.Shape.a;

					d = component.Shape.d; // Achi(k).Long
					b = component.Shape.bf; // Achi(k).Short
					t = component.Shape.tf; // Achi(k).Th
					a = b - t;
					c = d - t;
					k = -a * b * c * d * t / (4 * (b + c));
					x0 = (Math.Pow(b, 2) + c * t) / (2 * (b + c));
					y0 = (Math.Pow(d, 2) + a * t) / (2 * (b + c));
					Ix = (t * Math.Pow(d - y0, 3) + b * Math.Pow(y0, 3) - a * Math.Pow(y0 - t, 3)) / 3;
					Iy = (t * Math.Pow(b - x0, 3) + d * Math.Pow(x0, 3) - c * Math.Pow(x0 - t, 3)) / 3;
					if (Iy - Ix == 0)
						Theta = Math.PI / 4;
					else
						Theta = Math.Atan(2 * k / (Iy - Ix)) / 2;

					Iz = Ix * Math.Pow(Math.Sin(Theta), 2) + Iy * Math.Pow(Math.Cos(Theta), 2) + k * Math.Sin(2 * Theta);
					Iw = Iy * Math.Pow(Math.Sin(Theta), 2) + Ix * Math.Pow(Math.Cos(Theta), 2) - k * Math.Sin(2 * Theta);
					rz = Math.Pow(Iz / ar, 0.5F);
					rw = Math.Pow(Iw / ar, 0.5);
					rx = Math.Pow(Ix / ar, 0.5F);
					ry = Math.Pow(Iy / ar, 0.5F);
					rxsA = rx;
					rysA = ry;
					EnKucukMomInertia = Math.Min(Math.Min(Ix, Iy), Math.Min(Iz, Iw));
					if (memberType == EMemberType.UpperRight)
					{
						// Case 3 '2L
						tg = 0.375; // 0.75 'Gusset(m).Th
						tf = component.Shape.tf;
						tw = 2 * component.Shape.tf;
						if (component.WebOrientation == EWebOrientation.InPlane)
						{
							BF = 2 * Math.Min(component.Shape.d, component.Shape.bf);
							d = Math.Max(component.Shape.bf, component.Shape.d);
						}
						else
						{
							BF = 2 * Math.Max(component.Shape.bf, component.Shape.d);
							d = Math.Min(component.Shape.d, component.Shape.bf);
						}
						Yc = (BF * tf * (d - tf / 2) + tw * Math.Pow(d - tf, 2) / 2) / component.Shape.a;
						Ix = Math.Pow(d - tf, 3) * tw / 12 + (d - tf) * tw * Math.Pow(Yc - (d - tf) / 2, 2) + BF * Math.Pow(tf, 3) / 12 + BF * tf * Math.Pow(d - tf / 2 - Yc, 2);
						Iy = (Math.Pow(BF + tg, 3) * tf + (d - tf) * Math.Pow(tw + tg, 3)) / 12 - d * Math.Pow(tg, 3) / 12;
						Ixp = Ix + component.Shape.a * Math.Pow(Yc - tf / 2, 2);
						Ip = Ixp + Iy;
						rx = Math.Pow(Ix / component.Shape.a, 0.5F);
						ry = Math.Pow(Iy / component.Shape.a, 0.5F);
						EnKucukMomInertia = Math.Min(Ix, Iy);
					}
					break;
				case EShapeType.HollowSteelSection:
					t = component.Shape.tf;
					b = component.Shape.bf;
					H = component.Shape.d;
					Ix = (b * Math.Pow(H, 3) - (b - 2 * t) * Math.Pow(H - 2 * t, 3)) / 12 - 4 * (4 - Math.PI) * Math.Pow(t, 2) * Math.Pow(H / 2 - t / 2, 2) + 4 * (1 - Math.PI / 4) * Math.Pow(t, 2) * Math.Pow(H / 2 - t, 2);
					Iy = (H * Math.Pow(b, 3) - (H - 2 * t) * Math.Pow(b - 2 * t, 3)) / 12 - 4 * (4 - Math.PI) * Math.Pow(t, 2) * Math.Pow(b / 2 - t / 2, 2) + 4 * (1 - Math.PI / 4) * Math.Pow(t, 2) * Math.Pow(b / 2 - t, 2);
					EnKucukMomInertia = Math.Min(Ix, Iy);
					break;
				case EShapeType.SingleChannel:
				case EShapeType.DoubleChannel:
					tw = component.Shape.tw;
					BF = component.Shape.bf;
					tf = component.Shape.tf * 1.02;
					d = component.Shape.d;
					xc = (Math.Pow(BF, 2) * tf + d * Math.Pow(tw, 2) / 2 - tf * Math.Pow(tw, 2)) / component.Shape.a;
					Ix = (Math.Pow(d, 3) * BF - (BF - tw) * Math.Pow(d - 2 * tf, 3)) / 12;
					Iy = Math.Pow(BF, 3) * tf / 6 + (d - 2 * tf) * Math.Pow(tw, 3) / 12 + BF * tf * Math.Pow(BF / 2 - xc, 2) + (d - 2 * tf) * tw * Math.Pow(xc - tw / 2, 2);
					if (memberType == EMemberType.LowerLeft)
					{
						Ix = 2 * Ix;
						tg = component.BraceConnect.Gusset.Thickness;
						Iy = 2 * (Iy + component.Shape.a * Math.Pow(tg / 2 + xc, 2));
					}
					EnKucukMomInertia = Math.Min(Ix, Iy);
					break;
			}
			rx = Math.Pow(Ix / Area, 0.5);
			ry = Math.Pow(Iy / Area, 0.5);
		}

		public static void TorsionProperties(EMemberType memberType, double Ix, double Iy, ref double j, ref double Cw, ref double alpha, ref double Wno, ref double Sw, ref double Qf, ref double Qw)
		{
			double Sw3 = 0;
			double Sw2 = 0;
			double Sw1 = 0;
			double Wn2 = 0;
			double U = 0;
			double Eo = 0;
			double bp = 0;
			double taverage = 0;
			double tfp = 0;
			int t = 0;
			double H2 = 0;
			double H1 = 0;
			double df = 0;
			double H = 0;
			double G_ = 0;
			double tw = 0;
			double tf = 0;
			double BF = 0;
			double d = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];

			d = component.Shape.d;
			BF = component.Shape.bf;
			tf = component.Shape.tf;
			tw = component.Shape.tw;
			G_ = ConstNum.ELASTICITY / 2.6;

			switch (component.ShapeType)
			{
				case EShapeType.WideFlange:
					H = d - tf;
					j = (2 * BF * Math.Pow(tf, 3) + (d - 2 * tf) * Math.Pow(tw, 3)) / 3;
					Cw = Iy * Math.Pow(H, 2) / 4;
					alpha = Math.Pow(ConstNum.ELASTICITY * Cw / (G_ * j), 0.5);
					Wno = H * BF / 4;
					Sw = H * Math.Pow(BF, 2) * tf / 16;
					Qf = H * tf * (df - tw) / 4;
					Qw = H * BF * tf / 2 + Math.Pow(H - tf, 2) * tw / 8;
					break;
				case EShapeType.WTSection:
					j = (BF * Math.Pow(tf, 3) + (d - tf) * Math.Pow(tw, 3)) / 3;
					H = (d - tf / 2);
					Cw = Math.Pow(tf, 3) * Math.Pow(BF, 3) / 144 + Math.Pow(tw, 3) * Math.Pow(H, 3) / 36;
					break;
				case EShapeType.SingleAngle:
					j = (BF * Math.Pow(tf, 3) + (d - tf) * Math.Pow(tf, 3)) / 3;
					H1 = (d - tf / 2);
					H2 = (BF - tf / 2);
					Cw = Math.Pow(tf, 3) * (Math.Pow(H1, 3) + Math.Pow(H2, 3)) / 36;
					break;
				case EShapeType.DoubleAngle:
					// d is back to back
					j = (2 * BF * Math.Pow(tf, 3) + (d - tf) * Math.Pow(2 * tf + ConstNum.THREE_EIGHTS_INCH, 3)) / 3;
					break;
				case EShapeType.HollowSteelSection:
					j = 2 * tf * Math.Pow(d - tf, 2) * Math.Pow(BF - t, 2) / (d + BF - 2 * tf);
					break;
				case EShapeType.SingleChannel:
					tfp = (tf + (BF - tw) / 6);
					taverage = (tf + tfp) / 2;
					j = (2 * (BF - tw) * Math.Pow(tf, 3) + d * Math.Pow(tw, 3)) / 3 + 2 * (BF - tw) * Math.Pow(2 * (tfp - tf) / 3, 3);
					H = d - taverage;
					bp = (BF - tw / 2);
					Eo = tf * Math.Pow(bp, 2) / (2 * bp * tf + H * tw / 3);
					U = bp - Eo;
					Cw = Math.Pow(H, 2) * Math.Pow(bp, 2) * taverage * (bp - 3 * Eo) / 6 + Math.Pow(Eo, 2) * Ix;
					alpha = Math.Pow(ConstNum.ELASTICITY * Cw / (G_ * j), 0.5);
					Wno = U * H / 2;
					Wn2 = Eo * H / 2;
					Sw1 = Math.Pow(U, 2) * H * tf / 4;
					Sw2 = H * bp * tf * (bp - 2 * Eo) / 4;
					Sw3 = (Sw2 - Eo * Math.Pow(H, 2) * tw / 8);
					break;
				case EShapeType.DoubleChannel:
					j = (2 * BF * Math.Pow(taverage, 3) + (d - 2 * taverage) * Math.Pow(2 * tw + ConstNum.THREE_EIGHTS_INCH, 3)) / 3;
					break;
			}
		}
	}
}