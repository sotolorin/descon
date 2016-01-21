using System.Collections.Generic;
using Descon.Data;

namespace Descon.Calculations
{
	internal class HSSWSF
	{
		internal double B;
		internal double L;
		internal double WSF;
	}

	public static class ReducedWSF
	{
		public static double ReducedWSFCalc(double B, double L)
		{
			double ReducedWSF = 0;

			var hssWSFList = new List<HSSWSF>
			{
				new HSSWSF {B = 12, L = 6, WSF = 830},
				
				new HSSWSF {B = 14, L = 6, WSF = 843},
				
				new HSSWSF {B = 16, L = 6, WSF = 737},
				
				new HSSWSF {B = 18, L = 6, WSF = 656},
				new HSSWSF {B = 18, L = 7, WSF = 892},
				new HSSWSF {B = 18, L = 8, WSF = 1140},
				
				new HSSWSF {B = 20, L = 6, WSF = 590},
				new HSSWSF {B = 20, L = 7, WSF = 803},
				new HSSWSF {B = 20, L = 8, WSF = 1050},
				new HSSWSF {B = 20, L = 9, WSF = 1280},
				
				new HSSWSF {B = 22, L = 6, WSF = 536},
				new HSSWSF {B = 22, L = 7, WSF = 730},
				new HSSWSF {B = 22, L = 8, WSF = 953},
				new HSSWSF {B = 22, L = 9, WSF = 1210},
				new HSSWSF {B = 22, L = 10, WSF = 1420},
				
				new HSSWSF {B = 24, L = 6, WSF = 492},
				new HSSWSF {B = 24, L = 7, WSF = 669},
				new HSSWSF {B = 24, L = 8, WSF = 874},
				new HSSWSF {B = 24, L = 9, WSF = 1110},
				new HSSWSF {B = 24, L = 10, WSF = 1370},
				new HSSWSF {B = 24, L = 11, WSF = 1560},
				
				new HSSWSF {B = 26, L = 6, WSF = 454},
				new HSSWSF {B = 26, L = 7, WSF = 618},
				new HSSWSF {B = 26, L = 8, WSF = 807},
				new HSSWSF {B = 26, L = 9, WSF = 1020},
				new HSSWSF {B = 26, L = 10, WSF = 1260},
				new HSSWSF {B = 26, L = 11, WSF = 1530},
				new HSSWSF {B = 26, L = 12, WSF = 1690},
				
				new HSSWSF {B = 28, L = 6, WSF = 421},
				new HSSWSF {B = 28, L = 7, WSF = 574},
				new HSSWSF {B = 28, L = 8, WSF = 749},
				new HSSWSF {B = 28, L = 9, WSF = 948},
				new HSSWSF {B = 28, L = 10, WSF = 1170},
				new HSSWSF {B = 28, L = 11, WSF = 1420},
				new HSSWSF {B = 28, L = 12, WSF = 1690},
				new HSSWSF {B = 28, L = 13, WSF = 1830},
				
				new HSSWSF {B = 30, L = 6, WSF = 393},
				new HSSWSF {B = 30, L = 7, WSF = 535},
				new HSSWSF {B = 30, L = 8, WSF = 699},
				new HSSWSF {B = 30, L = 9, WSF = 885},
				new HSSWSF {B = 30, L = 10, WSF = 1090},
				new HSSWSF {B = 30, L = 11, WSF = 1320},
				new HSSWSF {B = 30, L = 12, WSF = 1570},
				new HSSWSF {B = 30, L = 13, WSF = 1850},
				new HSSWSF {B = 30, L = 14, WSF = 1970},
				
				new HSSWSF {B = 32, L = 6, WSF = 369},
				new HSSWSF {B = 32, L = 7, WSF = 502},
				new HSSWSF {B = 32, L = 8, WSF = 656},
				new HSSWSF {B = 32, L = 9, WSF = 830},
				new HSSWSF {B = 32, L = 10, WSF = 1020},
				new HSSWSF {B = 32, L = 11, WSF = 1240},
				new HSSWSF {B = 32, L = 12, WSF = 1470},
				new HSSWSF {B = 32, L = 13, WSF = 1730},
				new HSSWSF {B = 32, L = 14, WSF = 2010}
			};

			if (CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD)
			{
				foreach (var item in hssWSFList)
					item.WSF = item.WSF / 1.5;
			}

			for (int i = 0; i < hssWSFList.Count; i++)
			{
				if (B == hssWSFList[i].B && L == hssWSFList[i].L)
				{
					ReducedWSF = hssWSFList[i].WSF;
					break;
				}
				else if (B == hssWSFList[i].B && B == hssWSFList[i - 1].B && (L > hssWSFList[i - 1].L && L < hssWSFList[i].L))
				{
					ReducedWSF = hssWSFList[i].WSF - (hssWSFList[i].WSF - hssWSFList[i - 1].WSF) / (hssWSFList[i].L - hssWSFList[i - 1].L) * (hssWSFList[i].L - L);
					break;
				}
				else if (B == hssWSFList[i].B && B == hssWSFList[i + 1].B && (L > hssWSFList[i].L && L < hssWSFList[i + 1].L))
				{
					ReducedWSF = hssWSFList[i + 1].WSF - (hssWSFList[i + 1].WSF - hssWSFList[i].WSF) / (hssWSFList[i + 1].L - hssWSFList[i].L) * (hssWSFList[i + 1].L - L);
					break;
				}
				else
					ReducedWSF = 0;
			}

			return ReducedWSF;
		}
	}
}