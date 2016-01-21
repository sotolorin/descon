using System;

namespace Descon.Data
{
	public static class MiscCalculationDataMethods
	{
		public static void CalculateCopeStuff(EMemberType memberType)
		{
			double elDiff;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var girder = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			if (!beam.WinConnect.Beam.TCope_User)
			{
				beam.WinConnect.Beam.TCopeD = 0;
				beam.WinConnect.Beam.TCopeL = 0;
			}
			if (!beam.WinConnect.Beam.BCope_User)
			{
				beam.WinConnect.Beam.BCopeD = 0;
				beam.WinConnect.Beam.BCopeL = 0;
			}

			if (!beam.WinConnect.Beam.TCope_User && CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
			{
				elDiff = girder.WinConnect.Beam.TopElValue - beam.WinConnect.Beam.TopElValue;
				if (elDiff < girder.Shape.kdet && beam.EndSetback < (girder.Shape.bf - girder.Shape.tw) / 2)
				{
					if (!beam.WinConnect.Beam.TCope_User)
					{
						beam.WinConnect.Beam.TCopeD = NumberFun.Round(Math.Max(girder.Shape.kdet - elDiff, beam.Shape.kdet), 8);
						beam.WinConnect.Beam.TCopeL = NumberFun.Round(Math.Max(ConstNum.QUARTER_INCH, (girder.Shape.bf - girder.Shape.tw) / 2 - beam.EndSetback + ConstNum.HALF_INCH), 8);
					}
				}

				elDiff = -(girder.WinConnect.Beam.TopElValue - girder.Shape.d) + (beam.WinConnect.Beam.TopElValue - beam.Shape.d);
				if (elDiff < girder.Shape.kdet && beam.EndSetback < (girder.Shape.bf - girder.Shape.tw) / 2)
				{
					if (!beam.WinConnect.Beam.BCope_User)
					{
						beam.WinConnect.Beam.BCopeD = NumberFun.Round(Math.Max(girder.Shape.kdet - elDiff, girder.Shape.kdet), 8);
						beam.WinConnect.Beam.BCopeL = NumberFun.Round(Math.Max(ConstNum.QUARTER_INCH, (girder.Shape.bf - girder.Shape.tw) / 2 - beam.EndSetback + ConstNum.HALF_INCH), 8);
					}
				}
			}
		}

		internal static void SetTeeLengthData(WCShearWebTee webTee)
		{
			if (CommonDataStatic.LoadingFileInProgress)
				return;

			double lengthWeb, lengthSupport;

			if (webTee.OSLConnection == EConnectionStyle.Welded && webTee.BeamSideConnection == EConnectionStyle.Bolted)
				webTee.SLength = (webTee.BoltWebOnStem.NumberOfRows - 1) * webTee.BoltWebOnStem.MinSpacing + 2 * webTee.BoltWebOnStem.EdgeDistLongDir;
			else if (webTee.OSLConnection == EConnectionStyle.Bolted && webTee.BeamSideConnection == EConnectionStyle.Welded)
				webTee.SLength = (webTee.BoltOslOnFlange.NumberOfRows - 1) * webTee.BoltOslOnFlange.MinSpacing + 2 * webTee.BoltOslOnFlange.EdgeDistLongDir;
			else if (webTee.OSLConnection == EConnectionStyle.Bolted && webTee.BeamSideConnection == EConnectionStyle.Bolted)
			{
				if (webTee.BoltStagger == EBoltStagger.OneLessRow)
					webTee.BoltStagger = EBoltStagger.None;	// Resets the value so we force the re-evaluation in case the NumberOfRows values aren't synced yet
				if (webTee.BoltStagger == EBoltStagger.None)
				{
					if (webTee.BoltOslOnFlange.NumberOfRows + 1 == webTee.BoltWebOnStem.NumberOfRows ||
						webTee.BoltWebOnStem.NumberOfRows + 1 == webTee.BoltOslOnFlange.NumberOfRows)
						webTee.BoltStagger = EBoltStagger.OneLessRow;
					else
					{
						lengthSupport = (webTee.BoltOslOnFlange.NumberOfRows - 1) * webTee.BoltOslOnFlange.MinSpacing + 2 * webTee.BoltOslOnFlange.EdgeDistLongDir;
						lengthWeb = (webTee.BoltWebOnStem.NumberOfRows - 1) * webTee.BoltWebOnStem.MinSpacing + 2 * webTee.BoltWebOnStem.EdgeDistLongDir;
						webTee.SLength = Math.Max(lengthSupport, lengthWeb);
					}
				}
				else
				{
					if (webTee.BoltOslOnFlange.NumberOfRows == webTee.BoltWebOnStem.NumberOfRows)
					{
						lengthSupport = (webTee.BoltOslOnFlange.NumberOfRows - 0.5) * webTee.BoltOslOnFlange.MinSpacing + 2 * webTee.BoltOslOnFlange.EdgeDistLongDir;
						lengthWeb = (webTee.BoltWebOnStem.NumberOfRows - 0.5) * webTee.BoltWebOnStem.MinSpacing + 2 * webTee.BoltWebOnStem.EdgeDistLongDir;
						webTee.SLength = Math.Max(lengthSupport, lengthWeb);
					}
					else if (webTee.BoltOslOnFlange.NumberOfRows + 1 == webTee.BoltWebOnStem.NumberOfRows)
					{
						webTee.BoltStagger = EBoltStagger.OneLessRow;
						webTee.SLength = ((webTee.BoltWebOnStem.NumberOfRows - 1) * webTee.BoltWebOnStem.MinSpacing + 2 * webTee.BoltWebOnStem.EdgeDistLongDir);
					}
					else if (webTee.BoltOslOnFlange.NumberOfRows - 1 == webTee.BoltWebOnStem.NumberOfRows)
					{
						webTee.BoltStagger = EBoltStagger.OneLessRow;
						webTee.SLength = ((webTee.BoltOslOnFlange.NumberOfRows - 1) * webTee.BoltOslOnFlange.MinSpacing + 2 * webTee.BoltOslOnFlange.EdgeDistLongDir);
					}
				}
			}
		}

		public static void SetKBraceValues()
		{
			// Left Side
			if (!CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].IsActive &&
				!CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].IsActive &&
				CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive)
			{
				CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].KBrace = true;
				CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].KBrace = true;
				CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].KneeBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].KneeBrace = false;
			}
			else if (CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].IsActive &&
					 !CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].IsActive &&
					 CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive)
			{
				CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].KBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].KBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].KneeBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].KneeBrace = true;
			}
			else if (!CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].IsActive &&
					 CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].IsActive &&
					 CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive)
			{
				CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].KBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].KBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].KneeBrace = true;
				CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].KneeBrace = false;
			}
			else
			{
				CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].KBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].KBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].KneeBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].KneeBrace = false;
			}

			// Right Side
			if (!CommonDataStatic.DetailDataDict[EMemberType.LowerRight].IsActive &&
				!CommonDataStatic.DetailDataDict[EMemberType.UpperRight].IsActive &&
				CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive)
			{
				CommonDataStatic.DetailDataDict[EMemberType.LowerRight].KBrace = true;
				CommonDataStatic.DetailDataDict[EMemberType.UpperRight].KBrace = true;
				CommonDataStatic.DetailDataDict[EMemberType.LowerRight].KneeBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.UpperRight].KneeBrace = false;
			}
			else if (CommonDataStatic.DetailDataDict[EMemberType.LowerRight].IsActive &&
					 !CommonDataStatic.DetailDataDict[EMemberType.UpperRight].IsActive &&
					 CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive)
			{
				CommonDataStatic.DetailDataDict[EMemberType.LowerRight].KBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.UpperRight].KBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.LowerRight].KneeBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.UpperRight].KneeBrace = true;
			}
			else if (!CommonDataStatic.DetailDataDict[EMemberType.LowerRight].IsActive &&
					 CommonDataStatic.DetailDataDict[EMemberType.UpperRight].IsActive &&
					 CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive)
			{
				CommonDataStatic.DetailDataDict[EMemberType.LowerRight].KBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.UpperRight].KBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.LowerRight].KneeBrace = true;
				CommonDataStatic.DetailDataDict[EMemberType.UpperRight].KneeBrace = false;
			}
			else
			{
				CommonDataStatic.DetailDataDict[EMemberType.LowerRight].KBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].KBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.LowerRight].KneeBrace = false;
				CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].KneeBrace = false;
			}
		}

		public static double ExpYieldStr(double yield)
		{
			double ry = 0;

			switch (CommonDataStatic.Units)
			{
				case EUnit.US:
					if (yield < 36)
						ry = 1.4;
					else if (yield < 40)
						ry = 1.5;
					else if (yield < 48)
						ry = 1.3;
					else
						ry = 1.1;
					break;
				case EUnit.Metric:
					if (yield < 250)
						ry = 1.4;
					else if (yield < 280)
						ry = 1.5;
					else if (yield < 330)
						ry = 1.3;
					else
						ry = 1.1;
					break;
			}

			return ry;
		}

		public static double StaggeredBoltGage(double d, double P, double t)
		{
			double StaggeredBoltGage = 0;
			double BoltGage = 0;
			double pp = 0;
			double dd = 0;

			if (CommonDataStatic.Units == EUnit.Metric)
			{
				dd = d / 25.4;
				pp = P / 25.4;
			}
			else
			{
				dd = d;
				pp = P;
			}

			if (dd <= 0.625)
			{
				if (pp >= 1.625)
					BoltGage = 1;
				else if (pp >= 1.5)
					BoltGage = 1.125;
				else if (pp >= 1.5)
					BoltGage = 1.25;
				else if (pp >= 1.4375)
					BoltGage = 1.375;
				else if (pp >= 1.25)
					BoltGage = 1.5;
				else if (pp >= 1.25)
					BoltGage = 1.625;
				else if (pp >= 1.1875)
					BoltGage = 1.75;
				else if (pp >= 1.125)
					BoltGage = 1.875;
				else if (pp >= 1)
					BoltGage = 2;
				else if (pp >= 0.8125)
					BoltGage = 2.125;
			}
			else if (dd <= 0.75)
			{
				if (pp >= 1.9375)
					BoltGage = 1.25;
				else if (pp >= 1.875)
					BoltGage = 1.375;
				else if (pp >= 1.8125)
					BoltGage = 1.5;
				else if (pp >= 1.75)
					BoltGage = 1.625;
				else if (pp >= 1.6875)
					BoltGage = 1.75;
				else if (pp >= 1.5625)
					BoltGage = 1.875;
				else if (pp >= 1.5)
					BoltGage = 2;
				else if (pp >= 1.375)
					BoltGage = 2.125;
				else if (pp >= 1.25)
					BoltGage = 2.25;
				else if (pp >= 1.125)
					BoltGage = 2.375;
				else if (pp >= 0.875)
					BoltGage = 2.5;
			}
			else if (dd <= 0.875)
			{
				if (pp >= 2.1875)
					BoltGage = 1.375;
				else if (pp >= 2.125)
					BoltGage = 1.5;
				else if (pp >= 2.0625)
					BoltGage = 1.625;
				else if (pp >= 2)
					BoltGage = 1.75;
				else if (pp >= 1.9375)
					BoltGage = 1.875;
				else if (pp >= 1.8125)
					BoltGage = 2;
				else if (pp >= 1.6875)
					BoltGage = 2.125;
				else if (pp >= 1.5625)
					BoltGage = 2.25;
				else if (pp >= 1.5)
					BoltGage = 2.375;
				else if (pp >= 1.375)
					BoltGage = 2.5;
				else if (pp >= 1.1875)
					BoltGage = 2.625;
				else if (pp >= 1.9375)
					BoltGage = 2.75;
			}
			else if (dd <= 1)
			{
				if (pp >= 2.3125)
					BoltGage = 1.5;
				else if (pp >= 2.3125)
					BoltGage = 1.625;
				else if (pp >= 2.25)
					BoltGage = 1.75;
				else if (pp >= 2.1875)
					BoltGage = 1.875;
				else if (pp >= 2.125)
					BoltGage = 2;
				else if (pp >= 2)
					BoltGage = 2.125;
				else if (pp >= 1.875)
					BoltGage = 2.25;
				else if (pp >= 1.75)
					BoltGage = 2.375;
				else if (pp >= 1.625)
					BoltGage = 2.5;
				else if (pp >= 1.5)
					BoltGage = 2.625;
				else if (pp >= 1.375)
					BoltGage = 2.75;
				else if (pp >= 1.1875)
					BoltGage = 2.875;
				else if (pp >= 0.875)
					BoltGage = 3;
			}
			else if (dd <= 1.125)
			{
				if (pp >= 2.5625)
					BoltGage = 1.625;
				else if (pp >= 2.5625)
					BoltGage = 1.75;
				else if (pp >= 2.5)
					BoltGage = 1.875;
				else if (pp >= 2.4375)
					BoltGage = 2;
				else if (pp >= 2.375)
					BoltGage = 2.125;
				else if (pp >= 2.25)
					BoltGage = 2.25;
				else if (pp >= 2.125)
					BoltGage = 2.375;
				else if (pp >= 2)
					BoltGage = 2.5;
				else if (pp >= 1.9375)
					BoltGage = 2.625;
				else if (pp >= 1.875)
					BoltGage = 2.75;
				else if (pp >= 1.625)
					BoltGage = 2.875;
				else if (pp >= 1.5)
					BoltGage = 3;
				else if (pp >= 1.25)
					BoltGage = 3.125;
				else if (pp >= 0.9375)
					BoltGage = 3.25;
			}
			else if (dd <= 1.25)
			{
				if (pp >= 2.8125)
					BoltGage = 1.75;
				else if (pp >= 2.75)
					BoltGage = 1.875;
				else if (pp >= 2.75)
					BoltGage = 2;
				else if (pp >= 2.6875)
					BoltGage = 2.125;
				else if (pp >= 2.625)
					BoltGage = 2.25;
				else if (pp >= 2.5)
					BoltGage = 2.375;
				else if (pp >= 2.4375)
					BoltGage = 2.5;
				else if (pp >= 2.3125)
					BoltGage = 2.625;
				else if (pp >= 2.125)
					BoltGage = 2.75;
				else if (pp >= 2.0625)
					BoltGage = 2.875;
				else if (pp >= 2)
					BoltGage = 3;
				else if (pp >= 1.875)
					BoltGage = 3.125;
				else if (pp >= 1.75)
					BoltGage = 3.25;
				else if (pp >= 1.625)
					BoltGage = 3.375;
				else if (pp >= 1.375)
					BoltGage = 3.5;
				else if (pp >= 1.0625)
					BoltGage = 3.625;
			}
			else if (dd <= 1.375)
			{
				if (pp >= 3)
					BoltGage = 1.75;
				else if (pp >= 3)
					BoltGage = 1.875;
				else if (pp >= 2.9375)
					BoltGage = 2;
				else if (pp >= 2.9375)
					BoltGage = 2.125;
				else if (pp >= 2.875)
					BoltGage = 2.25;
				else if (pp >= 2.8125)
					BoltGage = 2.375;
				else if (pp >= 2.75)
					BoltGage = 2.5;
				else if (pp >= 2.625)
					BoltGage = 2.625;
				else if (pp >= 2.5)
					BoltGage = 2.75;
				else if (pp >= 2.375)
					BoltGage = 2.875;
				else if (pp >= 2.25)
					BoltGage = 3;
				else if (pp >= 2.125)
					BoltGage = 3.125;
				else if (pp >= 2)
					BoltGage = 3.25;
				else if (pp >= 1.9375)
					BoltGage = 3.375;
				else if (pp >= 1.75)
					BoltGage = 3.5;
				else if (pp >= 1.5625)
					BoltGage = 3.625;
				else if (pp >= 1.3125)
					BoltGage = 3.75;
			}
			else if (dd <= 1.5)
			{
				if (pp >= 3.25)
					BoltGage = 1.875;
				else if (pp >= 3.25)
					BoltGage = 2;
				else if (pp >= 3.1875)
					BoltGage = 2.125;
				else if (pp >= 3.1875)
					BoltGage = 2.25;
				else if (pp >= 3.125)
					BoltGage = 2.375;
				else if (pp >= 3.0625)
					BoltGage = 2.5;
				else if (pp >= 3)
					BoltGage = 2.625;
				else if (pp >= 2.875)
					BoltGage = 2.75;
				else if (pp >= 2.8125)
					BoltGage = 2.875;
				else if (pp >= 2.6875)
					BoltGage = 3;
				else if (pp >= 2.5)
					BoltGage = 3.125;
				else if (pp >= 2.375)
					BoltGage = 3.25;
				else if (pp >= 2.25)
					BoltGage = 3.375;
				else if (pp >= 2.125)
					BoltGage = 3.5;
				else if (pp >= 2)
					BoltGage = 3.625;
				else if (pp >= 1.875)
					BoltGage = 3.75;
				else if (pp >= 1.6875)
					BoltGage = 3.875;
				else if (pp >= 1.375)
					BoltGage = 4;
			}

			if (CommonDataStatic.Units == EUnit.Metric)
				BoltGage = Math.Ceiling(BoltGage * 25.4);
			StaggeredBoltGage = BoltGage + t;

			return StaggeredBoltGage;
		}

		public static void WebTeeSupportWeldType(EMemberType memberType, ref double BF, Shape shearWebTeeSize)
		{
			double Bfr = 0;
			double Bfl = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
				BF = column.Shape.d;
			else
				BF = column.Shape.bf;

			Bfl = BF / 2 - beam.Shape.tw / 2 - shearWebTeeSize.tw / 2;
			Bfr = BF / 2 + beam.Shape.tw / 2 + shearWebTeeSize.tw / 2;
			beam.WinConnect.ShearWebTee.FarSideWeldIsFlare = false;
			beam.WinConnect.ShearWebTee.FarSideWeldIsFillet = false;
			beam.WinConnect.ShearWebTee.NearSideWeldIsFlare = false;
			beam.WinConnect.ShearWebTee.NearSideWeldIsFillet = false;
			if (Bfr <= shearWebTeeSize.bf / 2)
				beam.WinConnect.ShearWebTee.FarSideWeldIsFlare = true;
			else if (Bfr >= shearWebTeeSize.bf / 2 + beam.WinConnect.ShearWebTee.WeldSizeFlange + ConstNum.SIXTEENTH_INCH + 2 * column.Shape.tf)
				beam.WinConnect.ShearWebTee.FarSideWeldIsFillet = true;
			if (Bfl <= shearWebTeeSize.bf / 2)
			{
				beam.WinConnect.ShearWebTee.NearSideWeldIsFlare = true;
				if (!beam.WinConnect.ShearWebTee.WeldSizeFlange_User && beam.WinConnect.ShearWebTee.WeldSizeFlange < 5 / 8.0 * column.Shape.tf / 0.707)
					beam.WinConnect.ShearWebTee.WeldSizeFlange = NumberFun.Round(5 / 8.0 * column.Shape.tf / 0.707, 16);
			}
			else if (Bfl >= shearWebTeeSize.bf / 2 - beam.WinConnect.ShearWebTee.WeldSizeFlange + ConstNum.SIXTEENTH_INCH - 2 * column.Shape.tf)
				beam.WinConnect.ShearWebTee.NearSideWeldIsFillet = true;
		}
	}
}
