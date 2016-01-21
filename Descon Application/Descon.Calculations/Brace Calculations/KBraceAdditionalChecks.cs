using Descon.Data;

namespace Descon.Calculations
{
	internal static class KBraceAdditionalChecks
	{
		internal static void CalcKBraceAdditionalChecks(EMemberType memberType , ref double t, bool printout)
		{
			//double HorizLength0;
			//double x2R;
			//double x1R;
			//double x2L;
			//double x1L;
			//double yr;
			//double yL;
			//double L1;
			//double xLeft3;
			//double xLeft1;
			//double xLeft0;
			//double xRight3;
			//double xRight1;
			//double xRight0;
			//double slop2728;
			//double slop2627;
			//double eLeft;
			//double xLeft2;
			//double eRight;
			//double xRight2;
			//double ev;
			//double yRight;
			//double xRight;
			//double yLeft;
			//double xLeft;
			//double tOther;
			//double treq;
			//double sqr12;
			//double hh;
			//object sigma = null;
			//double sigmax;
			//double BucklingCap;
			//double KLoR;
			//double Lcr;
			//double xfg;
			//double yfg;
			//double y2B;
			//double y1B;
			//double y2T;
			//double y1T;
			//double Mi;
			//double Mom;
			//double Vi;
			//double Hi;
			//double ecc;
			//double Xb;
			//double xt;
			//double HorizLength;
			//double VertLength;
			//double tNormal;
			//double t0;
			//double VgCap;
			//double Vn;
			//double Aw;
			//double Fb;
			//double fvv;
			//double fh;
			//double Moment;
			//double eBottom;
			//double yBottom;
			//double eTop;
			//double yTop1;
			//double dumy;
			//double L;
			//double ortasi;
			//double eh;
			//double yBot;
			//double xBot;
			//double yTop;
			//double xTop;
			//int i1 = 0;
			//double V;
			//double H;
			//double L_withStiffener;
			//double LFreeMax;
			//double Lfg_Right;
			//double Lfg_Left;
			//double L_Available;
			//double Lfg;

			//if (BRACE1.Seismic || BRACE1.CheckEdgeBuckling)
			//{
			//	switch (BRACE1.JointType)
			//	{
			//		case 1:
			//			switch (m)
			//			{
			//				case 5:
			//					Lfg = BRACE1.x[28] - BRACE1.x[38];
			//					L_Available = BRACE1.StiffX[3] - BRACE1.StiffX[5];
			//					Lfg_Left = BRACE1.StiffX[5] - BRACE1.x[38];
			//					Lfg_Right = BRACE1.x[28] - BRACE1.StiffX[3];
			//					break;
			//				case 6:
			//					Lfg = BRACE1.x[33] - BRACE1.x[43];
			//					L_Available = BRACE1.StiffX[4] - BRACE1.StiffX[6];
			//					Lfg_Left = BRACE1.StiffX[6] - BRACE1.x[43];
			//					Lfg_Right = BRACE1.x[33] - BRACE1.StiffX[4];
			//					break;
			//			}
			//			LFreeMax = (Math.Max(Lfg_Left, Lfg_Right));
			//			t = (Math.Max(t, LFreeMax/(0.75*Math.Sqrt(BRACE1.YMEn/BRACE1.Gusset[m].Fy)));
			//			L_withStiffener = t*(0.75*Math.Sqrt(BRACE1.YMEn/BRACE1.Gusset[m].Fy));
			//			if (L_withStiffener < L_Available)
			//			{
			//				BRACE1.NumberOfStiffeners[m] = (-((int) Math.Floor(-L_Available/L_withStiffener)) + 1);
			//			}
			//			else
			//			{
			//				BRACE1.NumberOfStiffeners[m] = 0;
			//			}
			//			break;
			//		case 0:
			//			if (BRACE1.CheckEdgeBuckling)
			//			{
			//				switch (m)
			//				{
			//					case 4:
			//						Lfg = BRACE1.y[27] - BRACE1.y[32];
			//						L_Available = BRACE1.KStiffY[3] - BRACE1.KStiffY[4];
			//						break;
			//					case 6:
			//						Lfg = BRACE1.y[37] - BRACE1.y[42];
			//						L_Available = BRACE1.KStiffY[5] - BRACE1.KStiffY[6];
			//						break;
			//				}
			//				L_withStiffener = t*(ConstString.FIOMEGA0_75N*Math.Sqrt(BRACE1.YMEn/BRACE1.Gusset[m].Fy));
			//				if (L_withStiffener < Lfg)
			//				{
			//					BRACE1.NumberOfStiffeners[m] = (-((int) Math.Floor(-Lfg/L_withStiffener)) - 1);
			//				}
			//				else
			//				{
			//					BRACE1.NumberOfStiffeners[m] = 0;
			//				}
			//			}
			//			else
			//			{
			//				BRACE1.NumberOfStiffeners[m] = 0;
			//			}
			//			break;
			//	}
			//	if (BRACE1.NumberOfStiffeners[m] == 1)
			//	{
			//		BRACE1.NumberOfStiffeners[m] = 2;
			//	}


			//}
			//else
			//{
			//	BRACE1.NumberOfStiffeners[m] = 0;
			//}

			//switch (BRACE1.JointType)
			//{
			//	case 0:
			//		H = Math.Abs(BRACE1.Gusset[m].Hc + BRACE1.Gusset[m - 1].Hc);
			//		V = -BRACE1.Gusset[m].Vc + BRACE1.Gusset[m - 1].Vc;
			//		i1 = 5*(m - 4);
			//		switch (m)
			//		{
			//			case 4:
			//				if (BRACE1.FlangeConnection[3] != 0)
			//				{
			//					// claw angle
			//					xTop = BRACE1.BoltX[11, 1];
			//					yTop = BRACE1.BoltY[9, 1];
			//				}
			//				else if (BRACE1.SectionTypeIndex[3] == 0)
			//				{
			//					// web plate
			//					xTop = BRACE1.BoltX[7, BRACE1.Bolts[7].N];
			//					yTop = BRACE1.BoltY[7, BRACE1.Bolts[7].N + 1 - BRACE1.BoltNw[7, 1]];
			//				}
			//				else if (BRACE1.BraceToGussetConType[3] == "Bolted")
			//				{
			//					// non W brace
			//					xTop = Math.Min(BRACE1.x[454], BRACE1.x[455]);
			//					if (BRACE1.SectionTypeIndex[3] == 1)
			//					{
			//						yTop = Math.Min(BRACE1.y[454], BRACE1.y[455]) - (BRACE1.ElProp[3].BF - 2*BRACE1.BraceBolts[3].eat - (BRACE1.NumBoltLinesForBrace[3] - 1)*BRACE1.BraceBolts[3].st)*Math.Cos(BRACE1.member[3].Angle*BRACE1.radian);
			//					}
			//					else
			//					{
			//						yTop = Math.Min(BRACE1.y[454], BRACE1.y[455]));
			//					}
			//				}
			//				else
			//				{
			//					// brace welded 'non W brace
			//					xTop = Math.Min(BRACE1.x[301], BRACE1.x[303]));
			//					yTop = Math.Min(BRACE1.y[301], BRACE1.y[303]));
			//				}
			//				if (BRACE1.FlangeConnection[4] != 0)
			//				{
			//					// claw angle
			//					xBot = BRACE1.BoltX[17, 1];
			//					yBot = BRACE1.BoltY[15, 1];
			//				}
			//				else if (BRACE1.SectionTypeIndex[4] == 0)
			//				{
			//					// web plate
			//					xBot = BRACE1.BoltX[13, BRACE1.Bolts[13].N];
			//					yBot = BRACE1.BoltY[13, BRACE1.BoltNw[13, 1]];
			//				}
			//				else if (BRACE1.BraceToGussetConType[4] == "Bolted")
			//				{
			//					// non W brace
			//					xBot = Math.Min(BRACE1.x[460], BRACE1.x[461]));
			//					if (BRACE1.SectionTypeIndex[4] == 1)
			//					{
			//						yBot = (Math.Max(BRACE1.y[460], BRACE1.y[461]) + (BRACE1.ElProp[4].BF - 2*BRACE1.BraceBolts[4].eat - (BRACE1.NumBoltLinesForBrace[4] - 1)*BRACE1.BraceBolts[3].st)*Math.Cos(BRACE1.member[4].Angle*BRACE1.radian);
			//					}
			//					else
			//					{
			//						yBot = (Math.Max(BRACE1.y[460], BRACE1.y[461]));
			//					}
			//				}
			//				else
			//				{
			//					// brace welded 'non W brace
			//					xBot = Math.Min(BRACE1.x[309], BRACE1.x[311]));
			//					yBot = (Math.Max(BRACE1.y[309], BRACE1.y[311]));
			//				}
			//				break;
			//			case 6:
			//				if (BRACE1.FlangeConnection[5] != 0)
			//				{
			//					// claw angle
			//					xTop = BRACE1.BoltX[23, 1];
			//					yTop = BRACE1.BoltY[21, 1];
			//				}
			//				else if (BRACE1.SectionTypeIndex[5] == 0)
			//				{
			//					// web plate
			//					xTop = BRACE1.BoltX[19, BRACE1.Bolts[19].N];
			//					yTop = BRACE1.BoltY[19, BRACE1.BoltNw[19, 1]];
			//				}
			//				else if (BRACE1.BraceToGussetConType[5] == "Bolted")
			//				{
			//					// non W brace
			//					xTop = (Math.Max(BRACE1.x[466], BRACE1.x[467]));
			//					if (BRACE1.SectionTypeIndex[5] == 1)
			//					{
			//						yTop = Math.Min(BRACE1.y[466], BRACE1.y[467]) + (BRACE1.ElProp[5].BF - 2*BRACE1.BraceBolts[5].eat - (BRACE1.NumBoltLinesForBrace[5] - 1)*BRACE1.BraceBolts[5].st)*Math.Cos(BRACE1.member[5].Angle*BRACE1.radian);
			//					}
			//					else
			//					{
			//						yTop = Math.Min(BRACE1.y[466], BRACE1.y[467]));
			//					}
			//				}
			//				else
			//				{
			//					// brace welded 'non W brace
			//					xTop = (Math.Max(BRACE1.x[317], BRACE1.x[319]));
			//					yTop = Math.Min(BRACE1.y[317], BRACE1.y[319]));
			//				}
			//				if (BRACE1.FlangeConnection[6] != 0)
			//				{
			//					// claw angle
			//					xBot = BRACE1.BoltX[29, 1];
			//					yBot = BRACE1.BoltY[27, 1];
			//				}
			//				else if (BRACE1.SectionTypeIndex[6] == 0)
			//				{
			//					// web plate
			//					xBot = BRACE1.BoltX[25, BRACE1.Bolts[25].N];
			//					yBot = BRACE1.BoltY[25, BRACE1.BoltNw[25, 1]];
			//				}
			//				else if (BRACE1.BraceToGussetConType[6] == "Bolted")
			//				{
			//					// non W brace
			//					xBot = (Math.Max(BRACE1.x[472], BRACE1.x[473]));
			//					if (BRACE1.SectionTypeIndex[6] == 1)
			//					{
			//						yBot = (Math.Max(BRACE1.y[472], BRACE1.y[473]) - (BRACE1.ElProp[6].BF - 2*BRACE1.BraceBolts[6].eat - (BRACE1.NumBoltLinesForBrace[6] - 1)*BRACE1.BraceBolts[6].st)*Math.Cos(BRACE1.member[6].Angle*BRACE1.radian);
			//					}
			//					else
			//					{
			//						yBot = (Math.Max(BRACE1.y[472], BRACE1.y[473]));
			//					}
			//				}
			//				else
			//				{
			//					// brace welded 'non W brace
			//					xBot = (Math.Max(BRACE1.x[325], BRACE1.x[327]));
			//					yBot = (Math.Max(BRACE1.y[325], BRACE1.y[327]));
			//				}
			//				break;
			//		}
			//		eh = Math.Min(Math.Abs(xTop), Math.Abs(xBot)) - Math.Abs(BRACE1.x[25 + 5*(m - 3)]);
			//		ortasi = (BRACE1.y[29 + i1] + BRACE1.y[34 + i1])/2;
			//		L = BRACE1.y[29 + i1] - BRACE1.y[34 + i1];
			//		SmallMethodsBrace.Intersect(100, BRACE1.x[25 + i1], BRACE1.y[25 + i1], Math.Tan(BRACE1.member[m - 1].Angle*BRACE1.radian), BRACE1.member[m - 1].WorkX, BRACE1.member[m - 1].WorkY, ref dumy, ref yTop1);
			//		eTop = yTop1 - ortasi;
			//		SmallMethodsBrace.Intersect(100, BRACE1.x[25 + i1], BRACE1.y[25 + i1], Math.Tan(BRACE1.member[m].Angle*BRACE1.radian), BRACE1.member[m].WorkX, BRACE1.member[m].WorkY, ref dumy, ref yBottom);
			//		eBottom = ortasi - yBottom;
			//		Moment = Math.Abs(eTop*BRACE1.Gusset[m - 1].Hc - eBottom*BRACE1.Gusset[m].Hc + V*eh);
			//		fh = H/L;
			//		fvv = Math.Abs(V/L);
			//		Fb = 6*Moment/(L*L);
			//		do
			//		{
			//			//     hovtw = L / t
			//			Aw = L*t;
			//			//     kv = 5
			//			//  If hovtw <= 187 * Sqr(kv / Gusset(m).Fy) Then
			//			Vn = 0.6*BRACE1.Gusset[m].Fy*Aw;
			//			//  ElseIf hovtw <= 234 * Sqr(kv / Gusset(m).Fy) Then
			//			//    Vn = 0.6 * Gusset(m).Fy * Aw * (187 * Sqr(kv / Gusset(m).Fy)) / hovtw
			//			//  Else
			//			//    Vn = Aw * 26400 * kv / (hovtw ^ 2)
			//			//  End If
			//			VgCap = BRACE1.FiOmega1_0N*Vn;
			//			t0 = t;
			//			if (!(BRACE1.DetReport || BRACE1.SumReport || BRACE1.UserChoseGussetThickness[m]))
			//			{
			//				if (VgCap < fvv*L)
			//				{
			//					t = t + BRACE1.FractionofinchN(1);
			//				}
			//			}
			//		} while (t0 < t);
			//		tNormal = (fh + Fb)/(BRACE1.FiOmega0_9N*BRACE1.Gusset[m].Fy);
			//		if (!(BRACE1.DetReport || BRACE1.SumReport || BRACE1.UserChoseGussetThickness[m]))
			//		{
			//			if (t < tNormal)
			//			{
			//				t = tNormal;
			//			}
			//		}
			//		H = (Math.Max(BRACE1.GussetEFTension[m].Hc + BRACE1.GussetEFCompression[m - 1].Hc, BRACE1.GussetEFTension[m - 1].Hc + BRACE1.GussetEFCompression[m].Hc));
			//		V = (Math.Max(BRACE1.GussetEFTension[m].Vc + BRACE1.GussetEFCompression[m - 1].Vc, BRACE1.GussetEFTension[m - 1].Vc + BRACE1.GussetEFCompression[m].Vc));
			//		VertLength = yTop - yBot;
			//		switch (m)
			//		{
			//			case 4:
			//				HorizLength = Math.Abs((BRACE1.x[27] - BRACE1.x[32])/(BRACE1.y[27] - BRACE1.y[32])*(ortasi - BRACE1.y[32]) + BRACE1.x[32] - BRACE1.x[25 + 5*(m - 4)]);
			//				xt = Math.Abs((BRACE1.x[27] - BRACE1.x[28])/(BRACE1.y[27] - BRACE1.y[28])*(ortasi - BRACE1.y[28]) + BRACE1.x[28] - BRACE1.x[25 + 5*(m - 4)]);
			//				Xb = Math.Abs((BRACE1.x[32] - BRACE1.x[33])/(BRACE1.y[32] - BRACE1.y[33])*(ortasi - BRACE1.y[33]) + BRACE1.x[33] - BRACE1.x[25 + 5*(m - 4)]);
			//				break;
			//			case 6:
			//				HorizLength = Math.Abs((BRACE1.x[37] - BRACE1.x[42])/(BRACE1.y[37] - BRACE1.y[42])*(ortasi - BRACE1.y[42]) + BRACE1.x[42] - BRACE1.x[35 + 5*(m - 4)]);
			//				xt = Math.Abs((BRACE1.x[37] - BRACE1.x[38])/(BRACE1.y[37] - BRACE1.y[38])*(ortasi - BRACE1.y[38]) + BRACE1.x[38] - BRACE1.x[35 + 5*(m - 4)]);
			//				Xb = Math.Abs((BRACE1.x[42] - BRACE1.x[43])/(BRACE1.y[42] - BRACE1.y[43])*(ortasi - BRACE1.y[43]) + BRACE1.x[43] - BRACE1.x[35 + 5*(m - 4)]);
			//				break;
			//		}
			//		HorizLength = Math.Min(HorizLength, Math.Min(xt, Xb)));
			//		ecc = HorizLength/2;
			//		Hi = BRACE1.Gusset[m - 1].Hc - H/2;
			//		Vi = BRACE1.Gusset[m - 1].Vc - V/2;
			//		Mom = eTop*BRACE1.Gusset[m - 1].Hc - eBottom*BRACE1.Gusset[m].Hc;
			//		Mi = Math.Abs(BRACE1.Gusset[m - 1].Hc*eTop + Vi*ecc - Mom/2 - H/8*L);
			//		y1T = BRACE1.y[431 + 4*(m - 4)];
			//		y2T = BRACE1.y[433 + 4*(m - 4)];
			//		y1B = BRACE1.y[431 + 4*(m - 3)];
			//		y2B = BRACE1.y[433 + 4*(m - 3)];
			//		if (y1T < y2T && y1B > y2B)
			//		{
			//			yfg = y1T - y1B;
			//			xfg = BRACE1.x[431 + 4*(m - 4)] - BRACE1.x[431 + 4*(m - 3)];
			//		}
			//		else if (y1T < y2T)
			//		{
			//			yfg = y1T - y2B;
			//			xfg = BRACE1.x[431 + 4*(m - 4)] - BRACE1.x[433 + 4*(m - 3)];
			//		}
			//		else if (y1B > y2B)
			//		{
			//			yfg = y2T - y1B;
			//			xfg = BRACE1.x[431 + 4*(m - 4)] - BRACE1.x[433 + 4*(m - 3)];
			//		}
			//		else
			//		{
			//			yfg = y2T - y2B;
			//			xfg = BRACE1.x[433 + 4*(m - 4)] - BRACE1.x[431 + 4*(m - 3)];
			//		}
			//		Lfg = Math.Sqrt(Math.Pow(xfg, 2) + Math.Pow(yfg, 2));
			//		VertLength = 0.5*(Lfg + VertLength);
			//		if (BRACE1.CheckGussetStressesBetweenBraces)
			//		{
			//			fh = Math.Abs(Vi/HorizLength);
			//			fvv = Math.Abs(Hi/HorizLength);
			//			Fb = 6*Mi/(HorizLength*HorizLength);
			//			do
			//			{
			//				//     hovtw = HorizLength / t
			//				// Aw = HorizLength * t
			//				//     kv = 5
			//				//  If hovtw <= 187 * Sqr(kv / Gusset(m).Fy) Then
			//				// Vn = 0.6 * Gusset(m).Fy * Aw
			//				//  ElseIf hovtw <= 234 * Sqr(kv / Gusset(m).Fy) Then
			//				//    Vn = 0.6 * Gusset(m).Fy * Aw * (187 * Sqr(kv / Gusset(m).Fy)) / hovtw
			//				//  Else
			//				//    Vn = Aw * 26400 * kv / (hovtw ^ 2)
			//				//  End If
			//				VgCap = BRACE1.FiOmega1_0N*0.6*BRACE1.Gusset[m].Fy*HorizLength*t;
			//				t0 = t;
			//				if (!(BRACE1.DetReport || BRACE1.SumReport || BRACE1.UserChoseGussetThickness[m]))
			//				{
			//					if (VgCap < fvv*HorizLength)
			//					{
			//						t = t + BRACE1.FractionofinchN(1);
			//					}
			//				}
			//			} while (t0 < t);
			//			tNormal = BRACE1.ConvertInteger((fh + Fb)/(BRACE1.FiOmega0_9N*BRACE1.Gusset[m].Fy), 16);
			//			if (!(BRACE1.DetReport || BRACE1.SumReport || BRACE1.UserChoseGussetThickness[m]))
			//			{
			//				if (t < tNormal)
			//				{
			//					t = tNormal;
			//				}
			//			}
			//			//  L_withStiffener = t * (FiOmega0_75N * Sqr(YMEn / Gusset(m).Fy))
			//			//  If L_withStiffener < Lfg Then
			//			//    NumberOfStiffeners(m) = -Int(-Lfg / L_withStiffener) - 1
			//			//  Else
			//			//  End If
			//			// VertLength = L_withStiffener
			//			do
			//			{
			//				Lcr = 1.2*VertLength;
			//				KLoR = 3.464*Lcr/t;
			//				//    LamdaC = KLoR * Sqr(Gusset(m).Fy / YMEn) / pi
			//				//     If LamdaC <= 1.5 Then
			//				//       Fcr = 0.658 ^ (LamdaC ^ 2) * Gusset(m).Fy
			//				//     Else
			//				//       Fcr = 0.877 * Gusset(m).Fy / (LamdaC ^ 2)
			//				//     End If
			//				BucklingCap = BRACE1.FiOmega0_9N*BucklingStress(KLoR, BRACE1.Gusset[m].Fy);
			//				t0 = t;
			//				// VertLength0 = VertLength
			//				if (!(BRACE1.DetReport || BRACE1.SumReport || BRACE1.UserChoseGussetThickness[m]))
			//				{
			//					if (BucklingCap < (fh + Fb)/t)
			//					{
			//						t = t + BRACE1.FractionofinchN(1);
			//						// VertLength = VertLength0 - BirInchN / 4
			//					}
			//				}
			//			} while (t0 < t); // VertLength < VertLength0 '
			//		}
			//		// check edge buckling
			//		//  GoTo donotcheckedgebuckling
			//		if (BRACE1.CheckEdgeBuckling)
			//		{
			//			// t0 = Lfg / (FiOmega0_75N * Sqr(YMEn / Gusset(m).Fy))
			//			switch (m)
			//			{
			//				case 5:
			//					L_Available = BRACE1.StiffX[3] - BRACE1.StiffX[5];
			//					break;
			//				case 6:
			//					L_Available = BRACE1.StiffX[4] - BRACE1.StiffX[6];
			//					break;
			//			}
			//			L_withStiffener = t*(ConstString.FIOMEGA0_75N*Math.Sqrt(BRACE1.YMEn/BRACE1.Gusset[m].Fy));
			//			if (L_withStiffener < Lfg)
			//			{
			//				BRACE1.NumberOfStiffeners[m] = (-((int) Math.Floor(-Lfg/L_withStiffener)) - 1);
			//			}
			//			else
			//			{
			//			}
			//		}
			//		else
			//		{
			//			t0 = 0;
			//		}
			//		if (!(BRACE1.DetReport || BRACE1.SumReport || BRACE1.UserChoseGussetThickness[m]))
			//		{
			//			if (t < t0)
			//			{
			//				t = t0;
			//			}
			//		}
			//		//if (BRACE1.DetReport && printout)
			//		//{
			//		//	BRACE1.bookmark = BRACE1.bookmark + 1;
			//		//	Report.DefInstance.HeadingList.Items.Add("   Additional Checks for K-brace Gusset:");
			//		//	BRACE1.HeadingIndex[BRACE1.bookmark] = BRACE1.LineIndex;
			//		//	Reporting.AddLine(" ");
			//		//	Reporting.AddLine("1Additional Checks for K-brace Gusset:");
			//		//	Reporting.AddLine(" ");
			//		//	Reporting.AddLine("1Gusset Stresses at Vertical Section at End of Brace:");
			//		//	Reporting.AddLine(" (Section is at " + BRACE1.f(eh, 2, 2) + BRACE1.DistanceUnit + " from column face.)");
			//		//	Reporting.AddLine(" ");
			//		//	Reporting.AddLine("Normal Stress:");
			//		//	Reporting.AddLine(" ");
			//		//	Moment = Math.Abs(eTop*BRACE1.Gusset[m - 1].Hc - eBottom*BRACE1.Gusset[m].Hc + V*eh);
			//		//	Reporting.AddLine("Moment, M = | eTop*HcTop - eBot*HcBot + V*eh |");
			//		//	Reporting.AddLine("  = " + BRACE1.f(eTop, 2, 2) + " * " + BRACE1.f(BRACE1.Gusset[m - 1].Hc, 3, 1) + " - " + BRACE1.f(eBottom, 2, 2) + " * " + BRACE1.f(BRACE1.Gusset[m].Hc, 3, 1) + " + " + BRACE1.f(V, 3, 1) + " * " + BRACE1.f(eh, 2, 2));
			//		//	Reporting.AddLine("  = " + BRACE1.f(Moment, 4, 1) + BRACE1.MomentUnitInch);
			//		//	Reporting.AddLine(" ");
			//		//	Reporting.AddLine("Stress = H/A + 6*M/(L²*t)");
			//		//	sigmax = Math.Abs(H)/(L*t) + 6*Moment/(Math.Pow(L, 2)*t);
			//		//	Reporting.AddLine("  = " + BRACE1.f(Math.Abs(H), 3, 1) + "/(" + BRACE1.f(L, 2, 2) + " * " + BRACE1.f(t, 1, 3) + ") + 6*" + BRACE1.f(Moment, 4, 1) + "/(" + BRACE1.f(L, 2, 2) + "² * " + BRACE1.f(t, 1, 3) + ")");
			//		//	if (sigma <= BRACE1.FiOmega0_9N*BRACE1.Gusset[m].Fy)
			//		//	{
			//		//		Reporting.AddLine("  = " + BRACE1.f(sigmax, 2, 2) + " <=  " + BRACE1.FiOmega0_9 + "Fy = " + BRACE1.f((BRACE1.FiOmega0_9N*BRACE1.Gusset[m].Fy), 2, 2) + " ksi OK");
			//		//	}
			//		//	else
			//		//	{
			//		//		Reporting.AddLine("  = " + BRACE1.f(sigmax, 2, 2) + " >>  " + BRACE1.FiOmega0_9 + "Fy = " + BRACE1.f((BRACE1.FiOmega0_9N*BRACE1.Gusset[m].Fy), 2, 2) + " ksi NG");
			//		//	}
			//		//	Reporting.AddLine(" ");
			//		//	Reporting.AddLine("Shear Yielding:");
			//		//	Reporting.AddLine(" ");
			//		//	//    hL = L
			//		//	//    hovtw = hL / t
			//		//	//    kv = 5
			//		//	Aw = (double) (L*t);
			//		//	//    OKPrintToFile "Use kv = 5 (Conservative)"
			//		//	Reporting.AddLine("Ag = h*t = " + BRACE1.f(L, 2, 2) + " * " + BRACE1.f(t, 1, 4) + " = " + BRACE1.f(Aw, 2, 2) + BRACE1.AreaUnit);
			//		//	Reporting.AddLine("");
			//		//	// If hovtw <= 187 * Sqr(kv / Gusset(m).Fy) Then
			//		//	//   OKPrintToFile "h/t = " & f$(hovtw, 3, 3) & " <= 187/(kv/Fy)^0.5"
			//		//	//   OKPrintToFile ""
			//		//	Vn = 0.6*BRACE1.Gusset[m].Fy*Aw;
			//		//	Reporting.AddLine("Vn = 0.6 * Fy * Ag");
			//		//	Reporting.AddLine("= 0.6 * " + BRACE1.f(BRACE1.Gusset[m].Fy, 2, 1) + " * " + BRACE1.f(Aw, 2, 2) + " = " + BRACE1.f(Vn, 3, 1) + ConstUnit.Force);
			//		//	// ElseIf hovtw <= 234 * Sqr(kv / Gusset(m).Fy) Then
			//		//	//   OKPrintToFile "187/(kv/Fy)^0.5 <= h/t = " & f$(hovtw, 2, 2) & " <= 234/(kv/Fy)^0.5"
			//		//	//   Vn = 0.6 * Gusset(m).Fy * Aw * (187 * Sqr(kv / Gusset(m).Fy)) / hovtw
			//		//	//   OKPrintToFile ""
			//		//	//     OKPrintToFile "Vn = 0.6*Fy*Aw*(187*(kv/Fy)^0.5)/(h/t)"
			//		//	//     OKPrintToFile "= 0.6*" & f$(Gusset(m).Fy, 2, 1) & " * " & f$(Aw, 2, 3) & "*(187*(" & f$(kv, 1, 0) & " / " & f$(Gusset(m).Fy, 2, 1) & ")^0.5) / " & f$(hovtw, 3, 3)
			//		//	//     OKPrintToFile "= " & f$(Vn, 3, 1) & " " & ForceUnit
			//		//	//   Else
			//		//	//     OKPrintToFile "h/t = " & f$(hovtw, 3, 3) & " >> 234/(kv/Fy)^0.5"
			//		//	//     OKPrintToFile ""
			//		//	//     Vn = Aw * 26400 * kv / (hovtw ^ 2)
			//		//	//     OKPrintToFile "Vn = Aw*26400*kv/(h/t)²"
			//		//	//     OKPrintToFile "Vn = " & f$(Aw, 2, 2) & " * 26400 * " & f$(kv, 1, 0) & "/(" & f$(hovtw, 3, 3) & "²)"
			//		//	//     OKPrintToFile "= " & f$(Vn, 3, 1) & " " & ForceUnit
			//		//	//   End If
			//		//	VgCap = BRACE1.FiOmega1_0N*Vn;
			//		//	Reporting.AddLine(" ");
			//		//	if (VgCap >= Math.Abs(V))
			//		//	{
			//		//		Reporting.AddLine(BRACE1.PHI + " Rn = " + BRACE1.FiOmega1_0 + "*Vn = " + BRACE1.f(VgCap, 3, 1) + " >= " + BRACE1.f(Math.Abs(V), 3, 1) + ConstUnit.Force + " (OK)");
			//		//	}
			//		//	else
			//		//	{
			//		//		Reporting.AddLine(BRACE1.PHI + " Rn = " + BRACE1.FiOmega1_0 + "*Vn = " + BRACE1.f(VgCap, 3, 1) + " << " + BRACE1.f(Math.Abs(V), 3, 1) + ConstUnit.Force + " (NG)");
			//		//	}
			//		//	Reporting.AddLine(" ");
			//		//	Hi = BRACE1.Gusset[m - 1].Hc - H/2;
			//		//	Vi = BRACE1.Gusset[m - 1].Vc - V/2;
			//		//	Mom = eTop*BRACE1.Gusset[m - 1].Hc - eBottom*BRACE1.Gusset[m].Hc;
			//		//	Mi = Math.Abs(BRACE1.Gusset[m - 1].Hc*eTop + Vi*ecc - Mom/2 - H/8*L);
			//		//	if (BRACE1.CheckGussetStressesBetweenBraces)
			//		//	{
			//		//		Reporting.AddLine("1Gusset Stresses at Horizontal Mid-Section:");
			//		//		Reporting.AddLine(" (Section width, Lh = " + BRACE1.f(HorizLength, 2, 2) + BRACE1.DistanceUnit + ")");
			//		//		Reporting.AddLine(" ");
			//		//		Reporting.AddLine("Horizontal Shear, Hi = HcTop - H/2 = " + BRACE1.f(Hi, 3, 1) + ConstUnit.Force);
			//		//		Reporting.AddLine("Vertical Force, Vi = VcTop - V/2 = " + BRACE1.f(Vi, 3, 1) + ConstUnit.Force);
			//		//		Reporting.AddLine("Moment, Mi = HcTop*eTop + Vi*(Lh/2) - M/2 - h*L/8) = " + BRACE1.f(Mi, 4, 1) + BRACE1.MomentUnitInch);
			//		//		Reporting.AddLine("");
			//		//		Reporting.AddLine("Normal Stress:");
			//		//		Reporting.AddLine("  = Vi/A + 6*Mi/(Lh²*t)");
			//		//		sigmax = Math.Abs(Vi)/(HorizLength*t) + 6*Math.Abs(Mi)/(Math.Pow(HorizLength, 2)*t);
			//		//		Reporting.AddLine("  = " + BRACE1.f(Math.Abs(Vi), 3, 1) + "/(" + BRACE1.f(HorizLength, 2, 2) + " * " + BRACE1.f(t, 1, 3) + ") + 6*" + BRACE1.f(Math.Abs(Mi), 4, 1) + "/(" + BRACE1.f(HorizLength, 2, 2) + "² * " + BRACE1.f(t, 1, 3) + ")");
			//		//		if (sigmax <= BRACE1.FiOmega0_9N*BRACE1.Gusset[m].Fy)
			//		//		{
			//		//			Reporting.AddLine("  = " + BRACE1.f(sigmax, 2, 2) + " <= 0.9Fy = " + BRACE1.f((BRACE1.FiOmega0_9N*BRACE1.Gusset[m].Fy), 2, 2) + " ksi OK");
			//		//		}
			//		//		else
			//		//		{
			//		//			Reporting.AddLine("  = " + BRACE1.f(sigmax, 2, 2) + " >> 0.9Fy = " + BRACE1.f((BRACE1.FiOmega0_9N*BRACE1.Gusset[m].Fy), 2, 2) + " ksi NG");
			//		//		}
			//		//		Reporting.AddLine(" ");
			//		//		Reporting.AddLine("Shear Yielding:");
			//		//		Reporting.AddLine(" ");
			//		//		hh = HorizLength;
			//		//		//     hovtw = hh / t
			//		//		//     kv = 5
			//		//		Aw = (double) (hh*t);
			//		//		//     OKPrintToFile "Use kv = 5 (Conservative)"
			//		//		Reporting.AddLine("Ag = h*t = " + BRACE1.f(hh, 2, 2) + " * " + BRACE1.f(t, 1, 4) + " = " + BRACE1.f(Aw, 2, 2) + BRACE1.AreaUnit);
			//		//		Reporting.AddLine("");
			//		//		//   If hovtw <= 187 * Sqr(kv / Gusset(m).Fy) Then
			//		//		//     OKPrintToFile "h/t = " & f$(hovtw, 3, 3) & " <= 187/(kv/Fy)^0.5"
			//		//		//     OKPrintToFile ""
			//		//		Vn = 0.6*BRACE1.Gusset[m].Fy*Aw;
			//		//		Reporting.AddLine("Vn = 0.6 * Fy * Ag");
			//		//		Reporting.AddLine("= 0.6 * " + BRACE1.f(BRACE1.Gusset[m].Fy, 2, 1) + " * " + BRACE1.f(Aw, 2, 2) + " = " + BRACE1.f(Vn, 3, 1) + ConstUnit.Force);
			//		//		//   ElseIf hovtw <= 234 * Sqr(kv / Gusset(m).Fy) Then
			//		//		//     OKPrintToFile "187/(kv/Fy)^0.5 <= h/t = " & f$(hovtw, 2, 2) & " <= 234/(kv/Fy)^0.5"
			//		//		//     Vn = 0.6 * Gusset(m).Fy * Aw * (187 * Sqr(kv / Gusset(m).Fy)) / hovtw
			//		//		//     OKPrintToFile ""
			//		//		//     OKPrintToFile "Vn = 0.6*Fy*Aw*(187*(kv/Fy)^0.5)/(h/t)"
			//		//		//     OKPrintToFile "= 0.6*" & f$(Gusset(m).Fy, 2, 1) & " * " & f$(Aw, 2, 3) & "*(187*(" & f$(kv, 1, 0) & " / " & f$(Gusset(m).Fy, 2, 1) & ")^0.5) / " & f$(hovtw, 3, 3)
			//		//		//     OKPrintToFile "= " & f$(Vn, 3, 1) & " " & ForceUnit
			//		//		//   Else
			//		//		//     OKPrintToFile "h/t = " & f$(hovtw, 3, 3) & " >> 234/(kv/Fy)^0.5"
			//		//		//     OKPrintToFile ""
			//		//		//     Vn = Aw * 26400 * kv / (hovtw ^ 2)
			//		//		//     OKPrintToFile "Vn = Aw*26400*kv/(h/t)²"
			//		//		//     OKPrintToFile "Vn = " & f$(Aw, 2, 2) & " * 26400 * " & f$(kv, 1, 0) & "/(" & f$(hovtw, 3, 3) & "²)"
			//		//		//     OKPrintToFile "= " & f$(Vn, 3, 1) & " " & ForceUnit
			//		//		//   End If
			//		//		VgCap = BRACE1.FiOmega1_0N*Vn;
			//		//		Reporting.AddLine(" ");
			//		//		if (VgCap >= Math.Abs(Hi))
			//		//		{
			//		//			Reporting.AddLine(BRACE1.PHI + " Rn = " + BRACE1.FiOmega1_0 + "*Vn = " + BRACE1.f(VgCap, 3, 1) + " >= " + BRACE1.f(Math.Abs(Hi), 3, 1) + ConstUnit.Force + " (OK)");
			//		//		}
			//		//		else
			//		//		{
			//		//			Reporting.AddLine(BRACE1.PHI + " Rn = " + BRACE1.FiOmega1_0 + "*Vn = " + BRACE1.f(VgCap, 3, 1) + " << " + BRACE1.f(Math.Abs(Hi), 3, 1) + ConstUnit.Force + " (NG)");
			//		//		}
			//		//		Reporting.AddLine(" ");
			//		//		Reporting.AddLine("1Gusset Buckling Between Braces:");
			//		//		Reporting.AddLine("");
			//		//		Reporting.AddLine("Length of Column Strip, Lv = " + BRACE1.f(VertLength, 2, 2) + BRACE1.DistanceUnit);
			//		//		Reporting.AddLine("Use K=1.2");
			//		//		Reporting.AddLine(" ");
			//		//		Lcr = 1.2*VertLength;
			//		//		// cc = 756.6 / Sqr(Gusset(m).Fy)
			//		//		KLoR = (double) (3.464*Lcr/t);
			//		//		Reporting.AddLine("KL/r = 1.2*Lv*(3.464/t) = " + BRACE1.f(KLoR, 3, 2));
			//		//		sqr12 = Math.Sqrt(12);
			//		//		KLoR = 3.464*Lcr/BRACE1.Gusset[m].Th;
			//		//		//   LamdaC = KLoR * Sqr(Gusset(m).Fy / YMEn) / pi
			//		//		//    If LamdaC <= 1.5 Then
			//		//		//      Fcr = 0.658 ^ (LamdaC ^ 2) * Gusset(m).Fy
			//		//		//    Else
			//		//		//      Fcr = 0.877 * Gusset(m).Fy / (LamdaC ^ 2)
			//		//		//    End If
			//		//		//  OKPrintToFile " "
			//		//		//  OKPrintToFile "LambdaC = KL/r * (Fy/E)^0.5/PI"
			//		//		//  OKPrintToFile "= " & f$(KLoR, 3, 1) & " * (" & f$(Gusset(m).Fy, 2, 1) & " / " & f$(YMEn, 6, 0) & ")^0.5/" & f$(pi, 1, 5)
			//		//		// OKPrintToFile "= " & f$(LamdaC, 2, 3)
			//		//		Reporting.AddLine(" ");
			//		//		BucklingCap = BRACE1.FiOmega0_9N*BucklingStress(KLoR, BRACE1.Gusset[m].Fy);
			//		//		//     If LamdaC <= 1.5 Then
			//		//		//       Fcr = 0.658 ^ (LamdaC ^ 2) * Gusset(m).Fy
			//		//		//    OKPrintToFile "Fcr = 0.658^(LamdaC²)*Fy"
			//		//		//    OKPrintToFile "= 0.658^(" & f$(LamdaC, 2, 1) & "²) * " & f$(Gusset(m).Fy, 2, 1) & " = " & f$(Fcr, 2, 2) & StressUnit
			//		//		//     Else
			//		//		//       Fcr = 0.877 * Gusset(m).Fy / (LamdaC ^ 2)
			//		//		//    OKPrintToFile "Fcr = 0.877*Fy/(LambdaC²)"
			//		//		//    OKPrintToFile "= 0.877*" & f$(Gusset(m).Fy, 2, 1) & "/(" & f$(LamdaC, 2, 3) & "²) = " & f$(Fcr, 2, 2) & StressUnit
			//		//		//     End If
			//		//		//    OKPrintToFile " "
			//		//		//    BucklingCap = FiOmega0_85N * Fcr
			//		//		if (BucklingCap >= sigmax)
			//		//		{
			//		//			Reporting.AddLine(BRACE1.PHI + "Fcr = " + BRACE1.FiOmega0_9 + " * " + BRACE1.f(BucklingCap/BRACE1.FiOmega0_9N, 2, 2) + " = " + BRACE1.f(BucklingCap, 3, 3) + " >= " + BRACE1.f(sigmax, 3, 3) + " ksi OK");
			//		//		}
			//		//		else
			//		//		{
			//		//			Reporting.AddLine(BRACE1.PHI + "Fcr = " + BRACE1.FiOmega0_9 + " * " + BRACE1.f(BucklingCap/BRACE1.FiOmega0_9N, 2, 2) + " = " + BRACE1.f(BucklingCap, 3, 3) + " << " + BRACE1.f(sigmax, 3, 3) + " ksi NG");
			//		//			Reporting.AddLine(" (Gusset plate must be stiffened between braces.)");
			//		//		}
			//		//		Reporting.AddLine("");
			//		//	}
			//		//	else
			//		//	{
			//		//		Reporting.AddLine("User turned off gusset stress check between braces.");
			//		//		Reporting.AddLine("");
			//		//	}
			//		//	goto neednotcheckedgebuckling;
			//		//	if (BRACE1.CheckEdgeBuckling)
			//		//	{
			//		//		Reporting.AddLine("1Gusset Free Edge Buckling:");
			//		//		Reporting.AddLine("");
			//		//		Reporting.AddLine("Length of Free Edge, Lf = " + BRACE1.f(Lfg, 2, 2) + BRACE1.DistanceUnit);
			//		//		Reporting.AddLine("Required gusset thickness to prevent");
			//		//		Reporting.AddLine("buckling before yielding= Lf/(0.75*(E/Fy)^0.5)");
			//		//		Reporting.AddLine("    = Lf/(0.75*(E/Fy)^0.5)");
			//		//		treq = Lfg/(0.75*Math.Pow(BRACE1.YMEn/BRACE1.Gusset[m].Fy, 0.5));
			//		//		Reporting.AddLine("    = " + BRACE1.f(Lfg, 2, 2) + "/(0.75*(" + BRACE1.f(BRACE1.YMEn, 6, 0) + " / " + BRACE1.f(BRACE1.Gusset[m].Fy, 2, 1) + ")^0.5)");
			//		//		if (treq <= t)
			//		//		{
			//		//			Reporting.AddLine("    = " + BRACE1.f(treq, 3, 3) + " <= " + BRACE1.f(t, 3, 3) + BRACE1.DistanceUnit + " (OK)");
			//		//		}
			//		//		else
			//		//		{
			//		//			Reporting.AddLine("    = " + BRACE1.f(treq, 3, 3) + " >> " + BRACE1.f(t, 3, 3) + BRACE1.DistanceUnit + " (NG)");
			//		//			Reporting.AddLine(" (Gusset plate edge may be stiffened to prevent edge buckling.)");
			//		//		}
			//		//		Reporting.AddLine("");
			//		//	}
			//		//	else
			//		//	{
			//		//		Reporting.AddLine("User turned off gusset edge buckling criteria.");
			//		//		Reporting.AddLine("");
			//		//	}
			//		//	neednotcheckedgebuckling:
			//		//	;
			//		//}
			//		// *********
			//		// *********
			//		break;
			//	case 1:
			//		V = BRACE1.Gusset[m].Vbm + BRACE1.Gusset[m - 2].Vbm;
			//		H = -BRACE1.Gusset[m].Hb + BRACE1.Gusset[m - 2].Hb;
			//		i1 = 5*(m - 5);
			//		tOther = BRACE1.Gusset[m - 2].Th;
			//		t = (Math.Max(t, tOther);
			//		switch (m)
			//		{
			//			case 5:
			//				if (BRACE1.FlangeConnection[5] != 0)
			//				{
			//					// claw angle
			//					xLeft = BRACE1.BoltX[23, 1];
			//					yLeft = BRACE1.BoltY[21, 1];
			//				}
			//				else if (BRACE1.SectionTypeIndex[5] == 0)
			//				{
			//					// web plate
			//					xLeft = BRACE1.BoltX[19, BRACE1.Bolts[19].N];
			//					yLeft = BRACE1.BoltY[19, BRACE1.Bolts[19].N + 1 - BRACE1.BoltNw[19, 1]];
			//				}
			//				else if (BRACE1.BraceToGussetConType[5] == "Bolted")
			//				{
			//					// non W brace
			//					yLeft = Math.Min(BRACE1.y[466], BRACE1.y[467]));
			//					if (BRACE1.SectionTypeIndex[5] == 1)
			//					{
			//						xLeft = (Math.Max(BRACE1.x[466], BRACE1.x[467]) - (BRACE1.ElProp[5].BF - 2*BRACE1.BraceBolts[5].eat - (BRACE1.NumBoltLinesForBrace[5] - 1)*BRACE1.BraceBolts[5].st)*Math.Sin(BRACE1.member[5].Angle*BRACE1.radian);
			//					}
			//					else
			//					{
			//						xLeft = (Math.Max(BRACE1.x[466], BRACE1.x[467]));
			//					}
			//				}
			//				else
			//				{
			//					// brace welded 'non W brace
			//					xLeft = (Math.Max(BRACE1.x[317], BRACE1.x[319]));
			//					yLeft = Math.Min(BRACE1.y[317], BRACE1.y[319]));
			//				}
			//				if (BRACE1.FlangeConnection[3] != 0)
			//				{
			//					// claw angle
			//					xRight = BRACE1.BoltX[11, 1];
			//					yRight = BRACE1.BoltY[9, 1];
			//				}
			//				else if (BRACE1.SectionTypeIndex[3] == 0)
			//				{
			//					// web plate
			//					xRight = BRACE1.BoltX[7, BRACE1.Bolts[7].N];
			//					yRight = BRACE1.BoltY[7, BRACE1.BoltNw[7, 1]];
			//				}
			//				else if (BRACE1.BraceToGussetConType[3] == "Bolted")
			//				{
			//					// non W brace
			//					yRight = Math.Min(BRACE1.y[454], BRACE1.y[455]));
			//					if (BRACE1.SectionTypeIndex[3] == 1)
			//					{
			//						xRight = Math.Min(BRACE1.x[454], BRACE1.x[455]) + (BRACE1.ElProp[3].BF - 2*BRACE1.BraceBolts[3].eat - -(BRACE1.NumBoltLinesForBrace[3] - 1)*BRACE1.BraceBolts[3].st)*Math.Sin(BRACE1.member[3].Angle*BRACE1.radian);
			//					}
			//					else
			//					{
			//						xRight = Math.Min(BRACE1.x[454], BRACE1.x[455]));
			//					}
			//				}
			//				else
			//				{
			//					// brace welded 'non W brace
			//					xRight = Math.Min(BRACE1.x[301], BRACE1.x[303]));
			//					yRight = Math.Min(BRACE1.y[301], BRACE1.y[303]));
			//				}
			//				break;
			//			case 6:
			//				if (BRACE1.FlangeConnection[6] != 0)
			//				{
			//					// claw angle
			//					xLeft = BRACE1.BoltX[29, 1];
			//					yLeft = BRACE1.BoltY[27, 1];
			//				}
			//				else if (BRACE1.SectionTypeIndex[6] == 0)
			//				{
			//					// web plate
			//					xLeft = BRACE1.BoltX[25, BRACE1.Bolts[25].N];
			//					yLeft = BRACE1.BoltY[25, BRACE1.BoltNw[25, 1]];
			//				}
			//				else if (BRACE1.BraceToGussetConType[6] == "Bolted")
			//				{
			//					// non W brace
			//					yLeft = (Math.Max(BRACE1.x[472], BRACE1.x[473]));
			//					if (BRACE1.SectionTypeIndex[6] == 1)
			//					{
			//						xLeft = (Math.Max(BRACE1.x[472], BRACE1.x[473]) + (BRACE1.ElProp[6].BF - 2*BRACE1.BraceBolts[6].eat - (BRACE1.NumBoltLinesForBrace[6] - 1)*BRACE1.BraceBolts[6].st)*Math.Sin(BRACE1.member[6].Angle*BRACE1.radian);
			//					}
			//					else
			//					{
			//						xLeft = (Math.Max(BRACE1.x[472], BRACE1.x[473]));
			//					}
			//				}
			//				else
			//				{
			//					// brace welded 'non W brace
			//					xLeft = (Math.Max(BRACE1.x[325], BRACE1.x[327]));
			//					yLeft = (Math.Max(BRACE1.y[325], BRACE1.y[327]));
			//				}
			//				if (BRACE1.FlangeConnection[4] != 0)
			//				{
			//					// claw angle
			//					xRight = BRACE1.BoltX[17, 1];
			//					yRight = BRACE1.BoltY[15, 1];
			//				}
			//				else if (BRACE1.SectionTypeIndex[4] == 0)
			//				{
			//					// web plate
			//					xRight = BRACE1.BoltX[13, BRACE1.Bolts[13].N];
			//					yRight = BRACE1.BoltY[13, BRACE1.BoltNw[13, 1]];
			//				}
			//				else if (BRACE1.BraceToGussetConType[4] == "Bolted")
			//				{
			//					// non W brace
			//					yRight = (Math.Max(BRACE1.y[460], BRACE1.y[461]));
			//					if (BRACE1.SectionTypeIndex[4] == 1)
			//					{
			//						xRight = Math.Min(BRACE1.x[460], BRACE1.x[461]) - (BRACE1.ElProp[4].BF - 2*BRACE1.BraceBolts[4].eat - -(BRACE1.NumBoltLinesForBrace[4] - 1)*BRACE1.BraceBolts[4].st)*Math.Sin(BRACE1.member[4].Angle*BRACE1.radian);
			//					}
			//					else
			//					{
			//						xRight = Math.Min(BRACE1.x[460], BRACE1.x[461]));
			//					}
			//				}
			//				else
			//				{
			//					// brace welded 'non W brace
			//					xRight = Math.Min(BRACE1.x[311], BRACE1.x[309]));
			//					yRight = (Math.Max(BRACE1.y[311], BRACE1.y[309]));
			//				}
			//				break;
			//		}
			//		ev = Math.Min(Math.Abs(yLeft), Math.Abs(yRight)) - Math.Abs(BRACE1.y[25 + 5*(m - 3)]);
			//		ortasi = (BRACE1.x[36 + i1] + BRACE1.x[26 + i1])/2;
			//		L = BRACE1.x[26 + i1] - BRACE1.x[36 + i1];
			//		SmallMethodsBrace.Intersect(0, BRACE1.x[25 + i1], BRACE1.y[25 + i1], Math.Tan(BRACE1.member[m - 2].Angle*BRACE1.radian), BRACE1.member[m - 2].WorkX, BRACE1.member[m - 2].WorkY, ref xRight2, ref dumy);
			//		eRight = xRight2 - ortasi;
			//		SmallMethodsBrace.Intersect(0, BRACE1.x[25 + i1], BRACE1.y[25 + i1], Math.Tan(BRACE1.member[m].Angle*BRACE1.radian), BRACE1.member[m].WorkX, BRACE1.member[m].WorkY, ref xLeft2, ref dumy);
			//		eLeft = ortasi - xLeft2;
			//		Moment = Math.Abs(eLeft*BRACE1.Gusset[m].Vbm - eRight*BRACE1.Gusset[m - 2].Vbm + H*ev);
			//		if (BRACE1.x[26 + i1] - BRACE1.x[27 + i1] != 0)
			//		{
			//			slop2627 = (BRACE1.y[26 + i1] - BRACE1.y[27 + i1])/(BRACE1.x[26 + i1] - BRACE1.x[27 + i1]);
			//			slop2728 = (BRACE1.y[27 + i1] - BRACE1.y[28 + i1])/(BRACE1.x[27 + i1] - BRACE1.x[28 + i1]);
			//			SmallMethodsBrace.Intersect(0, BRACE1.x[25 + i1], BRACE1.y[25 + i1] + Math.Sign(BRACE1.y[25 + i1])*ev, slop2627, BRACE1.x[27 + i1], BRACE1.y[27 + i1], ref xRight0, ref dumy);
			//			SmallMethodsBrace.Intersect(0, BRACE1.x[25 + i1], BRACE1.y[25 + i1] + Math.Sign(BRACE1.y[25 + i1])*ev, slop2728, BRACE1.x[27 + i1], BRACE1.y[27 + i1], ref xRight1, ref dumy);
			//			xRight3 = Math.Min(xRight0, xRight1));
			//		}
			//		else
			//		{
			//			slop2728 = (BRACE1.y[27 + i1] - BRACE1.y[28 + i1])/(BRACE1.x[27 + i1] - BRACE1.x[28 + i1]);
			//			SmallMethodsBrace.Intersect(0, BRACE1.x[25 + i1], BRACE1.y[25 + i1] + Math.Sign(BRACE1.y[25 + i1])*ev, 100, BRACE1.x[27 + i1], BRACE1.y[27 + i1], ref xRight0, ref dumy);
			//			SmallMethodsBrace.Intersect(0, BRACE1.x[25 + i1], BRACE1.y[25 + i1] + Math.Sign(BRACE1.y[25 + i1])*ev, slop2728, BRACE1.x[27 + i1], BRACE1.y[27 + i1], ref xRight1, ref dumy);
			//			xRight3 = Math.Min(xRight0, xRight1));
			//		}
			//		if (BRACE1.x[36 + i1] - BRACE1.x[37 + i1] != 0)
			//		{
			//			slop2627 = (BRACE1.y[35 + i1] - BRACE1.y[37 + i1])/(BRACE1.x[36 + i1] - BRACE1.x[37 + i1]);
			//			slop2728 = (BRACE1.y[37 + i1] - BRACE1.y[38 + i1])/(BRACE1.x[37 + i1] - BRACE1.x[38 + i1]);
			//			SmallMethodsBrace.Intersect(0, BRACE1.x[25 + i1], BRACE1.y[25 + i1] + Math.Sign(BRACE1.y[25 + i1])*ev, slop2627, BRACE1.x[37 + i1], BRACE1.y[37 + i1], ref xLeft0, ref dumy);
			//			SmallMethodsBrace.Intersect(0, BRACE1.x[25 + i1], BRACE1.y[25 + i1] + Math.Sign(BRACE1.y[25 + i1])*ev, slop2728, BRACE1.x[37 + i1], BRACE1.y[37 + i1], ref xLeft1, ref dumy);
			//			xLeft3 = (Math.Max(xLeft0, xLeft1));
			//		}
			//		else
			//		{
			//			slop2728 = (BRACE1.y[37 + i1] - BRACE1.y[38 + i1])/(BRACE1.x[37 + i1] - BRACE1.x[38 + i1]);
			//			SmallMethodsBrace.Intersect(0, BRACE1.x[25 + i1], BRACE1.y[25 + i1] + Math.Sign(BRACE1.y[25 + i1])*ev, 100, BRACE1.x[37 + i1], BRACE1.y[37 + i1], ref xLeft0, ref dumy);
			//			SmallMethodsBrace.Intersect(0, BRACE1.x[25 + i1], BRACE1.y[25 + i1] + Math.Sign(BRACE1.y[25 + i1])*ev, slop2728, BRACE1.x[37 + i1], BRACE1.y[37 + i1], ref xLeft1, ref dumy);
			//			xLeft3 = (Math.Max(xLeft0, xLeft1));
			//		}
			//		L1 = xRight3 - xLeft3;
			//		fh = Math.Abs(H/L1);
			//		fvv = Math.Abs(V/L1);
			//		Fb = 6*Moment/(L1*L1);
			//		do
			//		{
			//			//      hovtw = L1 / t
			//			Aw = L1*t;
			//			//      kv = 5
			//			//   If hovtw <= 187 * Sqr(kv / Gusset(m).Fy) Then
			//			Vn = 0.6*BRACE1.Gusset[m].Fy*Aw;
			//			//   ElseIf hovtw <= 234 * Sqr(kv / Gusset(m).Fy) Then
			//			//     Vn = 0.6 * Gusset(m).Fy * Aw * (187 * Sqr(kv / Gusset(m).Fy)) / hovtw
			//			//   Else
			//			//     Vn = Aw * 26400 * kv / (hovtw ^ 2)
			//			//   End If
			//			VgCap = BRACE1.FiOmega1_0N*Vn;
			//			t0 = t;
			//			if (!(BRACE1.DetReport || BRACE1.SumReport || BRACE1.UserChoseGussetThickness[m]))
			//			{
			//				if (VgCap < fh*L1)
			//				{
			//					t = t + BRACE1.FractionofinchN(1);
			//				}
			//			}
			//		} while (t0 < t);
			//		tNormal = (fvv + Fb)/(0.6*BRACE1.Gusset[m].Fy);
			//		if (!(BRACE1.DetReport || BRACE1.SumReport || BRACE1.UserChoseGussetThickness[m]))
			//		{
			//			if (t < tNormal)
			//			{
			//				t = tNormal;
			//			}
			//		}
			//		V = BRACE1.Gusset[m].Vbm + BRACE1.Gusset[m - 2].Vbm;
			//		H = BRACE1.Gusset[m].Hb - BRACE1.Gusset[m - 2].Hb;
			//		HorizLength = xRight - xLeft;
			//		switch (m)
			//		{
			//			case 5:
			//				VertLength = Math.Abs((BRACE1.y[28] - BRACE1.y[38])/(BRACE1.x[28] - BRACE1.x[38])*(ortasi - BRACE1.x[38]) + BRACE1.y[38] - BRACE1.y[25]);
			//				yL = Math.Abs((BRACE1.y[38] - BRACE1.y[37])/(BRACE1.x[38] - BRACE1.x[37])*(ortasi - BRACE1.x[37]) + BRACE1.y[37] - BRACE1.y[35]);
			//				yr = Math.Abs((BRACE1.y[28] - BRACE1.y[27])/(BRACE1.x[28] - BRACE1.x[27])*(ortasi - BRACE1.x[27]) + BRACE1.y[27] - BRACE1.y[25]);
			//				break;
			//			case 6:
			//				VertLength = Math.Abs((BRACE1.y[33] - BRACE1.y[43])/(BRACE1.x[33] - BRACE1.x[43])*(ortasi - BRACE1.x[43]) + BRACE1.y[43] - BRACE1.y[30]);
			//				yL = Math.Abs((BRACE1.y[43] - BRACE1.y[42])/(BRACE1.x[43] - BRACE1.x[42])*(ortasi - BRACE1.x[43]) + BRACE1.y[43] - BRACE1.y[40]);
			//				yr = Math.Abs((BRACE1.y[33] - BRACE1.y[32])/(BRACE1.x[33] - BRACE1.x[32])*(ortasi - BRACE1.x[32]) + BRACE1.y[32] - BRACE1.y[30]);
			//				break;
			//		}
			//		VertLength = Math.Min(VertLength, Math.Min(yL, yr)));
			//		ecc = VertLength/2;
			//		Vi = BRACE1.Gusset[m].Vbm - V/2;
			//		Hi = BRACE1.Gusset[m].Hb - H/2;
			//		Mom = eLeft*BRACE1.Gusset[m].Vbm - eRight*BRACE1.Gusset[m - 2].Vbm;
			//		Mi = Math.Abs(BRACE1.Gusset[m].Vbm*eLeft + Hi*ecc - Mom/2 - V/8*L);
			//		x1L = BRACE1.x[441 + 4*(m - 5)];
			//		x2L = BRACE1.x[439 + 4*(m - 5)];
			//		x1R = BRACE1.x[433 + 4*(m - 5)];
			//		x2R = BRACE1.x[431 + 4*(m - 5)];
			//		if (x1L > x2L && x1R < x2R)
			//		{
			//			xfg = x1R - x1L;
			//			yfg = BRACE1.y[433 + 4*(m - 5)] - BRACE1.y[441 + 4*(m - 5)];
			//		}
			//		else if (x1L > x2L)
			//		{
			//			xfg = x2R - x1L;
			//			yfg = BRACE1.y[431 + 4*(m - 5)] - BRACE1.y[441 + 4*(m - 5)];
			//		}
			//		else if (x1R < x2R)
			//		{
			//			xfg = x1R - x2L;
			//			yfg = BRACE1.y[439 + 4*(m - 5)] - BRACE1.y[433 + 4*(m - 5)];
			//		}
			//		else
			//		{
			//			xfg = x2R - x2L;
			//			yfg = BRACE1.y[439 + 4*(m - 5)] - BRACE1.y[431 + 4*(m - 5)];
			//		}
			//		Lfg = Math.Sqrt(Math.Pow(xfg, 2) + Math.Pow(yfg, 2));
			//		HorizLength = 0.5*(Lfg + HorizLength);
			//		if (BRACE1.CheckGussetStressesBetweenBraces)
			//		{
			//			fvv = Math.Abs(Vi/VertLength);
			//			fh = Math.Abs(Hi/VertLength);
			//			Fb = 6*Mi/(VertLength*VertLength);
			//			do
			//			{
			//				//     hovtw = VertLength / t
			//				Aw = VertLength*t;
			//				//     kv = 5
			//				//  If hovtw <= 187 * Sqr(kv / Gusset(m).Fy) Then
			//				Vn = 0.6*BRACE1.Gusset[m].Fy*Aw;
			//				//  ElseIf hovtw <= 234 * Sqr(kv / Gusset(m).Fy) Then
			//				//    Vn = 0.6 * Gusset(m).Fy * Aw * (187 * Sqr(kv / Gusset(m).Fy)) / hovtw
			//				//  Else
			//				//    Vn = Aw * 26400 * kv / (hovtw ^ 2)
			//				//  End If
			//				VgCap = BRACE1.FiOmega1_0N*Vn;
			//				t0 = t;
			//				if (!(BRACE1.DetReport || BRACE1.SumReport || BRACE1.UserChoseGussetThickness[m]))
			//				{
			//					if (VgCap < fvv*VertLength)
			//					{
			//						t = t + BRACE1.FractionofinchN(1);
			//					}
			//				}
			//			} while (t0 < t);
			//			tNormal = BRACE1.ConvertInteger((fh + Fb)/(BRACE1.FiOmega0_9N*BRACE1.Gusset[m].Fy), 16);
			//			if (!(BRACE1.DetReport || BRACE1.SumReport || BRACE1.UserChoseGussetThickness[m]))
			//			{
			//				if (t < tNormal)
			//				{
			//					t = tNormal;
			//				}
			//			}
			//			L_withStiffener = t*(ConstString.FIOMEGA0_75N*Math.Sqrt(BRACE1.YMEn/BRACE1.Gusset[m].Fy));
			//			if (HorizLength > L_withStiffener)
			//			{
			//				HorizLength = L_withStiffener;
			//			}
			//			do
			//			{
			//				Lcr = 1.2*HorizLength;
			//				// cc = 756.6 / Sqr(Gusset(m).Fy)
			//				KLoR = 3.464*Lcr/t;
			//				//   LamdaC = KLoR * Sqr(Gusset(m).Fy / YMEn) / pi
			//				//    If LamdaC <= 1.5 Then
			//				//      Fcr = 0.658 ^ (LamdaC ^ 2) * Gusset(m).Fy
			//				//    Else
			//				//      Fcr = 0.877 * Gusset(m).Fy / (LamdaC ^ 2)
			//				//    End If
			//				BucklingCap = BRACE1.FiOmega0_9N*BucklingStress(KLoR, BRACE1.Gusset[m].Fy);
			//				//  t0 = t
			//				HorizLength0 = HorizLength;
			//				if (!(BRACE1.DetReport || BRACE1.SumReport || BRACE1.UserChoseGussetThickness[m]))
			//				{
			//					if (BucklingCap < (fh + Fb)/t)
			//					{
			//						// t = t + FractionofinchN(1)
			//						HorizLength = (HorizLength - ConstNum.ONE_INCH/4);
			//					}
			//				}
			//			} while (HorizLength0 > HorizLength); // t0 < t
			//		}
			//		// check edge buckling
			//		if (BRACE1.CheckEdgeBuckling)
			//		{
			//			// t0 = Lfg / (0.75 * Sqr(YMEn / Gusset(m).Fy))
			//			switch (m)
			//			{
			//				case 5:
			//					L_Available = BRACE1.StiffX[3] - BRACE1.StiffX[5];
			//					break;
			//				case 6:
			//					L_Available = BRACE1.StiffX[4] - BRACE1.StiffX[6];
			//					break;
			//			}
			//			L_withStiffener = t*(ConstString.FIOMEGA0_75N*Math.Sqrt(BRACE1.YMEn/BRACE1.Gusset[m].Fy));
			//			if (L_withStiffener < Lfg)
			//			{
			//				BRACE1.NumberOfStiffeners[m] = (-((int) Math.Floor(-Lfg/L_withStiffener)) + 1);
			//				BRACE1.NumberOfStiffeners[m] = (-((int) Math.Floor(-L_Available/L_withStiffener)) + 1);
			//			}
			//			else
			//			{
			//			}
			//		}
			//		else
			//		{
			//			t0 = 0;
			//		}
			//		if (!(BRACE1.DetReport || BRACE1.SumReport || BRACE1.UserChoseGussetThickness[m]))
			//		{
			//			if (t < t0)
			//			{
			//				t = t0;
			//			}
			//		}
			//		//if (BRACE1.DetReport && printout)
			//		//{
			//		//	BRACE1.bookmark = BRACE1.bookmark + 1;
			//		//	Report.DefInstance.HeadingList.Items.Add("   Additional Checks for V-brace Gusset:");
			//		//	BRACE1.HeadingIndex[BRACE1.bookmark] = BRACE1.LineIndex;
			//		//	Reporting.AddLine(" ");
			//		//	Reporting.AddLine("1Additional Checks for V-Brace Gusset:");
			//		//	Reporting.AddLine(" ");
			//		//	Reporting.AddLine("1Gusset Stresses at Horizontal Section at End of Brace:");
			//		//	Reporting.AddLine(" (Section is at " + BRACE1.f(ev, 2, 2) + BRACE1.DistanceUnit + " from beam face.)");
			//		//	Reporting.AddLine(" ");
			//		//	Reporting.AddLine("Normal Stress:");
			//		//	Reporting.AddLine(" ");
			//		//	Moment = Math.Abs(eLeft*BRACE1.Gusset[m].Vbm - eRight*BRACE1.Gusset[m - 2].Vbm + H*ev);
			//		//	Reporting.AddLine("Moment, M = | eLeft*VbLeft - eRight*VbRight + H*ev |");
			//		//	Reporting.AddLine("  = |" + BRACE1.f(eLeft, 2, 2) + " * " + BRACE1.f(BRACE1.Gusset[m].Vbm, 3, 1) + " - " + BRACE1.f(eRight, 2, 2) + " * " + BRACE1.f(BRACE1.Gusset[m - 2].Vbm, 3, 1) + " + " + BRACE1.f(H, 3, 1) + " * " + BRACE1.f(ev, 2, 2) + " |");
			//		//	Reporting.AddLine("  = " + BRACE1.f(Moment, 4, 1) + BRACE1.MomentUnitInch);
			//		//	Reporting.AddLine(" ");
			//		//	Reporting.AddLine("Stress = V/A + 6*M/(L1²*t)");
			//		//	sigmax = Math.Abs(V)/(L1*t) + 6*Moment/(Math.Pow(L1, 2)*t);
			//		//	Reporting.AddLine("  = " + BRACE1.f(Math.Abs(V), 3, 1) + "/(" + BRACE1.f(L1, 2, 2) + " * " + BRACE1.f(t, 1, 3) + ") + 6*" + BRACE1.f(Moment, 4, 1) + "/(" + BRACE1.f(L1, 2, 2) + "² * " + BRACE1.f(t, 1, 3) + ")");
			//		//	if (sigmax <= BRACE1.FiOmega0_9N*BRACE1.Gusset[m].Fy)
			//		//	{
			//		//		Reporting.AddLine("  = " + BRACE1.f(sigmax, 2, 2) + " <= " + BRACE1.FiOmega0_9 + "Fy = " + BRACE1.f((BRACE1.FiOmega0_9N*BRACE1.Gusset[m].Fy), 2, 2) + " ksi OK");
			//		//	}
			//		//	else
			//		//	{
			//		//		Reporting.AddLine("  = " + BRACE1.f(sigmax, 2, 2) + " >> " + BRACE1.FiOmega0_9 + "Fy = " + BRACE1.f((BRACE1.FiOmega0_9N*BRACE1.Gusset[m].Fy), 2, 2) + " ksi NG");
			//		//	}
			//		//	Reporting.AddLine(" ");
			//		//	Reporting.AddLine("Shear Yielding:");
			//		//	Reporting.AddLine(" ");
			//		//	//    hovtw = L1 / t
			//		//	Aw = (double) (L1*t);
			//		//	//    kv = 5
			//		//	//   OKPrintToFile "Use kv = 5"
			//		//	Reporting.AddLine("Ag = h*t = " + BRACE1.f(L1, 2, 2) + " * " + BRACE1.f(t, 1, 4) + " = " + BRACE1.f(Aw, 2, 2) + BRACE1.AreaUnit);
			//		//	Reporting.AddLine("");
			//		//	//   If hovtw <= 187 * Sqr(kv / Gusset(m).Fy) Then
			//		//	//     OKPrintToFile "h/t = " & f$(hovtw, 3, 3) & " <= 187/(kv/Fy)^0.5"
			//		//	//     OKPrintToFile ""
			//		//	Vn = 0.6*BRACE1.Gusset[m].Fy*Aw;
			//		//	Reporting.AddLine("Vn = 0.6 * Fy * Ag");
			//		//	Reporting.AddLine("= 0.6 * " + BRACE1.f(BRACE1.Gusset[m].Fy, 2, 1) + " * " + BRACE1.f(Aw, 2, 2) + " = " + BRACE1.f(Vn, 3, 1) + ConstUnit.Force);
			//		//	//   ElseIf hovtw <= 234 * Sqr(kv / Gusset(m).Fy) Then
			//		//	//     OKPrintToFile "187/(kv/Fy)^0.5 <= h/t = " & f$(hovtw, 2, 2) & " <= 234/(kv/Fy)^0.5"
			//		//	//     Vn = 0.6 * Gusset(m).Fy * Aw * (187 * Sqr(kv / Gusset(m).Fy)) / hovtw
			//		//	//     OKPrintToFile ""
			//		//	//     OKPrintToFile "Vn = 0.6*Fy*Aw*(187*(kv/Fy)^0.5)/(h/t)"
			//		//	//     OKPrintToFile "= 0.6*" & f$(Gusset(m).Fy, 2, 1) & " * " & f$(Aw, 2, 3) & "*(187*(" & f$(kv, 1, 0) & " / " & f$(Gusset(m).Fy, 2, 1) & ")^0.5) / " & f$(hovtw, 3, 3)
			//		//	//     OKPrintToFile "= " & f$(Vn, 3, 1) & " " & ForceUnit
			//		//	//   Else
			//		//	//     OKPrintToFile "h/t = " & f$(hovtw, 3, 3) & " >> 234/(kv/Fy)^0.5"
			//		//	//     OKPrintToFile ""
			//		//	//     Vn = Aw * 26400 * kv / (hovtw ^ 2)
			//		//	//     OKPrintToFile "Vn = Aw*26400*kv/(h/t)²"
			//		//	//     OKPrintToFile "Vn = " & f$(Aw, 2, 2) & " * 26400 * " & f$(kv, 1, 0) & "/(" & f$(hovtw, 3, 3) & "²)"
			//		//	//     OKPrintToFile "= " & f$(Vn, 3, 1) & " " & ForceUnit
			//		//	//   End If
			//		//	VgCap = BRACE1.FiOmega1_0N*Vn;
			//		//	Reporting.AddLine(" ");
			//		//	if (VgCap >= Math.Abs(H))
			//		//	{
			//		//		Reporting.AddLine(BRACE1.PHI + " Rn = " + BRACE1.FiOmega1_0 + "*Vn = " + BRACE1.f(VgCap, 3, 1) + " >= " + BRACE1.f(Math.Abs(H), 3, 1) + ConstUnit.Force + " (OK)");
			//		//	}
			//		//	else
			//		//	{
			//		//		Reporting.AddLine(BRACE1.PHI + " Rn = " + BRACE1.FiOmega1_0 + "*Vn = " + BRACE1.f(VgCap, 3, 1) + " << " + BRACE1.f(Math.Abs(H), 3, 1) + ConstUnit.Force + " (NG)");
			//		//	}
			//		//	Reporting.AddLine(" ");
			//		//	Vi = BRACE1.Gusset[m].Vbm - V/2;
			//		//	Hi = BRACE1.Gusset[m].Hb - H/2;
			//		//	Mom = eLeft*BRACE1.Gusset[m].Vbm - eRight*BRACE1.Gusset[m - 2].Vbm;
			//		//	Mi = BRACE1.Gusset[m].Vbm*eLeft + Hi*ecc - Mom/2 - V/8*L;
			//		//	if (BRACE1.CheckGussetStressesBetweenBraces)
			//		//	{
			//		//		Reporting.AddLine("1Gusset Stresses at Vertical Mid-Section:");
			//		//		Reporting.AddLine(" (Section width, Lv = " + BRACE1.f(VertLength, 2, 2) + BRACE1.DistanceUnit + ")");
			//		//		Reporting.AddLine(" ");
			//		//		Reporting.AddLine("Vertical Shear, Vi = VbLeft - V/2 = " + BRACE1.f(Vi, 3, 1) + ConstUnit.Force);
			//		//		Reporting.AddLine("Horizontal Force, Hi = HbLeft - H/2 = " + BRACE1.f(Hi, 3, 1) + ConstUnit.Force);
			//		//		Reporting.AddLine("Moment, Mi = VbLeft*eLeft + Hi*(Lv/2) - M/2- V*L / 8) = " + BRACE1.f(Mi, 4, 1) + BRACE1.MomentUnitInch);
			//		//		Reporting.AddLine("");
			//		//		Reporting.AddLine("Normal Stress:");
			//		//		Reporting.AddLine("  = Hi/A + 6*Mi/(Lv²*t)");
			//		//		sigmax = Math.Abs(Hi)/(VertLength*t) + 6*Math.Abs(Mi)/(Math.Pow(VertLength, 2)*t);
			//		//		Reporting.AddLine("  = " + BRACE1.f(Math.Abs(Hi), 3, 1) + "/(" + BRACE1.f(VertLength, 2, 2) + " * " + BRACE1.f(t, 1, 3) + ") + 6*" + BRACE1.f(Math.Abs(Mi), 4, 1) + "/(" + BRACE1.f(VertLength, 2, 2) + "² * " + BRACE1.f(t, 1, 3) + ")");
			//		//		if (sigmax <= BRACE1.FiOmega0_9N*BRACE1.Gusset[m].Fy)
			//		//		{
			//		//			Reporting.AddLine("  = " + BRACE1.f(sigmax, 2, 2) + " <= 0.6y = " + BRACE1.f((BRACE1.FiOmega0_9N*BRACE1.Gusset[m].Fy), 2, 2) + " ksi OK");
			//		//		}
			//		//		else
			//		//		{
			//		//			Reporting.AddLine("  = " + BRACE1.f(sigmax, 2, 2) + " >> 0.6y = " + BRACE1.f((BRACE1.FiOmega0_9N*BRACE1.Gusset[m].Fy), 2, 2) + " ksi NG");
			//		//		}
			//		//		Reporting.AddLine(" ");
			//		//		Reporting.AddLine("Shear Yielding:");
			//		//		Reporting.AddLine(" ");
			//		//		//      hovtw = VertLength / t
			//		//		//     kv = 5
			//		//		Aw = (double) (VertLength*t);
			//		//		//     OKPrintToFile "Use kv = 5 (Conservative)"
			//		//		Reporting.AddLine("Ag = h*t = " + BRACE1.f(VertLength, 2, 2) + " * " + BRACE1.f(t, 1, 4) + " = " + BRACE1.f(Aw, 2, 2) + BRACE1.AreaUnit);
			//		//		Reporting.AddLine("");
			//		//		//   If hovtw <= 187 * Sqr(kv / Gusset(m).Fy) Then
			//		//		//     OKPrintToFile "h/t = " & f$(hovtw, 3, 3) & " <= 187/(kv/Fy)^0.5"
			//		//		//     OKPrintToFile ""
			//		//		Vn = 0.6*BRACE1.Gusset[m].Fy*Aw;
			//		//		Reporting.AddLine("Vn = 0.6 * Fy * Ag");
			//		//		Reporting.AddLine("= 0.6 * " + BRACE1.f(BRACE1.Gusset[m].Fy, 2, 1) + " * " + BRACE1.f(Aw, 2, 2) + " = " + BRACE1.f(Vn, 3, 1) + ConstUnit.Force);
			//		//		//   ElseIf hovtw <= 234 * Sqr(kv / Gusset(m).Fy) Then
			//		//		//     OKPrintToFile "187/(kv/Fy)^0.5 <= h/t = " & f$(hovtw, 2, 2) & " <= 234/(kv/Fy)^0.5"
			//		//		//     Vn = 0.6 * Gusset(m).Fy * Aw * (187 * Sqr(kv / Gusset(m).Fy)) / hovtw
			//		//		//     OKPrintToFile ""
			//		//		//     OKPrintToFile "Vn = 0.6*Fy*Aw*(187*(kv/Fy)^0.5)/(h/t)"
			//		//		//     OKPrintToFile "= 0.6*" & f$(Gusset(m).Fy, 2, 1) & " * " & f$(Aw, 2, 3) & "*(187*(" & f$(kv, 1, 0) & " / " & f$(Gusset(m).Fy, 2, 1) & ")^0.5) / " & f$(hovtw, 3, 3)
			//		//		//     OKPrintToFile "= " & f$(Vn, 3, 1) & " " & ForceUnit
			//		//		//   Else
			//		//		//     OKPrintToFile "h/t = " & f$(hovtw, 3, 3) & " >> 234/(kv/Fy)^0.5"
			//		//		//     OKPrintToFile ""
			//		//		//     Vn = Aw * 26400 * kv / (hovtw ^ 2)
			//		//		//     OKPrintToFile "Vn = Aw*26400*kv/(h/t)²"
			//		//		//     OKPrintToFile "Vn = " & f$(Aw, 2, 2) & " * 26400 * " & f$(kv, 1, 0) & "/(" & f$(hovtw, 3, 3) & "²)"
			//		//		//     OKPrintToFile "= " & f$(Vn, 3, 1) & " " & ForceUnit
			//		//		//   End If
			//		//		VgCap = BRACE1.FiOmega1_0N*Vn;
			//		//		Reporting.AddLine(" ");
			//		//		if (VgCap >= Math.Abs(Vi))
			//		//		{
			//		//			Reporting.AddLine(BRACE1.PHI + " Rn = " + BRACE1.FiOmega1_0 + "*Vn = " + BRACE1.f(VgCap, 3, 1) + " >= " + BRACE1.f(Math.Abs(Vi), 3, 1) + ConstUnit.Force + " (OK)");
			//		//		}
			//		//		else
			//		//		{
			//		//			Reporting.AddLine(BRACE1.PHI + " Rn = " + BRACE1.FiOmega1_0 + "*Vn = " + BRACE1.f(VgCap, 3, 1) + " << " + BRACE1.f(Math.Abs(Vi), 3, 1) + ConstUnit.Force + " (NG)");
			//		//		}
			//		//		Reporting.AddLine(" ");
			//		//		Reporting.AddLine("1Gusset Buckling Between Braces:");
			//		//		Reporting.AddLine("");
			//		//		Reporting.AddLine("Length of Column Strip, Lh = " + BRACE1.f(HorizLength, 2, 2) + BRACE1.DistanceUnit);
			//		//		Reporting.AddLine("Use K=1.2");
			//		//		Lcr = 1.2*HorizLength;
			//		//		// cc = 756.6 / Sqr(Gusset(m).Fy)
			//		//		KLoR = (double) (3.464*Lcr/t);
			//		//		//   LamdaC = KLoR * Sqr(Gusset(m).Fy / YMEn) / pi
			//		//		//    If LamdaC <= 1.5 Then
			//		//		//      Fcr = 0.658 ^ (LamdaC ^ 2) * Gusset(m).Fy
			//		//		//    Else
			//		//		//      Fcr = 0.877 * Gusset(m).Fy / (LamdaC ^ 2)
			//		//		//    End If
			//		//		// OKPrintToFile " "
			//		//		// OKPrintToFile "LambdaC = KL/r * (Fy/E)^0.5/PI"
			//		//		// OKPrintToFile "= " & f$(KLoR, 3, 1) & " * (" & f$(Gusset(m).Fy, 2, 1) & " / " & f$(YMEn, 6, 0) & ")^0.5/" & f$(pi, 1, 5)
			//		//		// OKPrintToFile "= " & f$(LamdaC, 2, 3)
			//		//		// OKPrintToFile " "
			//		//		BucklingCap = BRACE1.FiOmega0_9N*BucklingStress(KLoR, BRACE1.Gusset[m].Fy);
			//		//		//   If LamdaC <= 1.5 Then
			//		//		//     Fcr = 0.658 ^ (LamdaC ^ 2) * Gusset(m).Fy
			//		//		//  OKPrintToFile "Fcr = 0.658^(LamdaC²)*Fy"
			//		//		//  OKPrintToFile "= 0.658^(" & f$(LamdaC, 2, 1) & "²) * " & f$(Gusset(m).Fy, 2, 1) & " = " & f$(Fcr, 2, 2) & StressUnit
			//		//		//   Else
			//		//		//     Fcr = 0.877 * Gusset(m).Fy / (LamdaC ^ 2)
			//		//		//  OKPrintToFile "Fcr = 0.877*Fy/(LambdaC²)"
			//		//		//  OKPrintToFile "= 0.877*" & f$(Gusset(m).Fy, 2, 1) & "/(" & f$(LamdaC, 2, 3) & "²) = " & f$(Fcr, 2, 2) & StressUnit
			//		//		//   End If
			//		//		Reporting.AddLine(" ");
			//		//		//  BucklingCap = FiOmega0_85N * Fcr
			//		//		if (BucklingCap >= sigmax)
			//		//		{
			//		//			Reporting.AddLine(BRACE1.PHI + "Fcr = " + BRACE1.FiOmega0_9 + " * " + BRACE1.f(BucklingCap/BRACE1.FiOmega0_9N, 2, 2) + " = " + BRACE1.f(BucklingCap, 3, 3) + " >= " + BRACE1.f(sigmax, 3, 3) + " ksi OK");
			//		//		}
			//		//		else
			//		//		{
			//		//			Reporting.AddLine(BRACE1.PHI + "Fcr = " + BRACE1.FiOmega0_9 + " * " + BRACE1.f(BucklingCap/BRACE1.FiOmega0_9N, 2, 2) + " = " + BRACE1.f(BucklingCap, 3, 3) + " << " + BRACE1.f(sigmax, 3, 3) + " ksi NG");
			//		//			Reporting.AddLine(" (Gusset plate must be stiffened between braces.)");
			//		//		}
			//		//		Reporting.AddLine("");
			//		//	}
			//		//	else
			//		//	{
			//		//		Reporting.AddLine("User turned off gusset stress check between braces.");
			//		//		Reporting.AddLine("");
			//		//	}
			//		//	if (BRACE1.CheckEdgeBuckling)
			//		//	{
			//		//		Reporting.AddLine("1Gusset Free Edge Buckling:");
			//		//		Reporting.AddLine("");
			//		//		Reporting.AddLine("Length of Free Edge, Lf = " + BRACE1.f(Lfg, 2, 2) + BRACE1.DistanceUnit);
			//		//		Reporting.AddLine("Required gusset thickness to prevent");
			//		//		Reporting.AddLine("buckling before yielding= Lf/(0.75*(E/Fy)^0.5)");
			//		//		Reporting.AddLine("    = Lf/(0.75*(E/Fy)^0.5)");
			//		//		treq = Lfg/(0.75*Math.Pow(BRACE1.YMEn/BRACE1.Gusset[m].Fy, 0.5));
			//		//		Reporting.AddLine("    = " + BRACE1.f(Lfg, 2, 2) + "/(0.75*(" + BRACE1.f(BRACE1.YMEn, 6, 0) + " / " + BRACE1.f(BRACE1.Gusset[m].Fy, 2, 1) + ")^0.5)");
			//		//		if (treq <= t)
			//		//		{
			//		//			Reporting.AddLine("    = " + BRACE1.f(treq, 3, 3) + " <= " + BRACE1.f(t, 3, 3) + BRACE1.DistanceUnit + " (OK)");
			//		//		}
			//		//		else
			//		//		{
			//		//			Reporting.AddLine("    = " + BRACE1.f(treq, 3, 3) + " >> " + BRACE1.f(t, 3, 3) + BRACE1.DistanceUnit + " (NG)");
			//		//			Reporting.AddLine(" (Gusset plate edge may be stiffened to prevent edge buckling.)");
			//		//		}
			//		//		Reporting.AddLine("");
			//		//	}
			//		//	else
			//		//	{
			//		//		Reporting.AddLine("User turned off gusset edge buckling criteria.");
			//		//		Reporting.AddLine("");
			//		//	}
			//		//}
			//		break;
			//}
		}
	}
}