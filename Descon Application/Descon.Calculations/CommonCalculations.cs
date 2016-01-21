using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	/// <summary>
	/// These calculation methods were used in both Brace and Win modes
	/// </summary>
	public static class CommonCalculations
	{
		public static double AngleStandardGage(int N, double leg0)
		{
			double angleStandardGage = 0;

			double leg = leg0 / ConstNum.ONE_INCH;
			double g1;
			double g2;
			double g;

			switch (N)
			{
				case 0:
					if (leg < 2.5)
						g = 0;
					else if (leg < 3)
						g = 1.375;
					else if (leg < 3.5)
						g = 1.75;
					else if (leg < 4)
						g = 2;
					else if (leg < 4)
						g = 2;
					else if (leg < 5)
						g = 2.5;
					else if (leg < 6)
						g = 3;
					else if (leg < 7)
						g = 3.5;
					else if (leg < 8)
						g = 4;
					else
						g = (0.5 * (1 + leg));
					g *= ConstNum.ONE_INCH;
					angleStandardGage = g;
					break;
				case 1:
					if (leg < 5)
						g1 = 0;
					else if (leg < 6)
						g1 = 2;
					else if (leg < 7)
						g1 = 2.25;
					else if (leg < 8)
						g1 = 2.5;
					else if (leg < 9)
						g1 = 3;
					else
						g1 = 3.5;
					g1 = g1 * ConstNum.ONE_INCH;
					angleStandardGage = g1;
					break;
				case 2:
					if (leg < 5)
						g2 = 0;
					else if (leg < 6)
						g2 = 1.75;
					else if (leg < 7)
						g2 = 2.5;
					else
						g2 = 3;
					g2 *= ConstNum.ONE_INCH;
					angleStandardGage = g2;
					break;
			}
			return angleStandardGage;
		}

		public static void SetBeamLv(EMemberType memberType, EPosition position, Bolt bolt, double length)
		{
			var member = CommonDataStatic.DetailDataDict[memberType];

			switch (position)
			{
				case EPosition.Top:
					member.WinConnect.Beam.DistanceToFirstBolt = ConstNum.THREE_INCHES;
					if (!member.WinConnect.DistanceToFirstBolt_User)
						member.WinConnect.Beam.DistanceToFirstBoltDisplay = ConstNum.THREE_INCHES;
					break;
				case EPosition.Center:
					member.WinConnect.Beam.DistanceToFirstBolt = (member.Shape.d / 2) - (length / 2) + bolt.EdgeDistLongDir;
					if (!member.WinConnect.DistanceToFirstBolt_User)
						member.WinConnect.Beam.DistanceToFirstBoltDisplay = (member.Shape.d / 2) - (length / 2) + bolt.EdgeDistLongDir;
					break;
				case EPosition.Bottom:
					member.WinConnect.Beam.DistanceToFirstBolt = member.Shape.d - 3 + (2 * bolt.EdgeDistLongDir) - length;
					if (!member.WinConnect.DistanceToFirstBolt_User)
						member.WinConnect.Beam.DistanceToFirstBoltDisplay = member.Shape.d - 3 + (2 * bolt.EdgeDistLongDir) - length;
					break;
				case EPosition.MatchOtherSideBolts:
					var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
					var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

					if (memberType == EMemberType.RightBeam)
					{
						member.WinConnect.Beam.DistanceToFirstBolt = leftBeam.WinConnect.Beam.DistanceToFirstBolt;
						if (!member.WinConnect.DistanceToFirstBolt_User)
						{
							member.WinConnect.Beam.DistanceToFirstBoltDisplay = leftBeam.WinConnect.Beam.DistanceToFirstBoltDisplay;
							if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToGirder)
								member.WinConnect.Beam.DistanceToFirstBoltDisplay += (-leftBeam.WinConnect.Beam.TopElValue + rightBeam.WinConnect.Beam.TopElValue);
						}
					}
					else if (memberType == EMemberType.LeftBeam)
					{
						member.WinConnect.Beam.DistanceToFirstBolt = rightBeam.WinConnect.Beam.DistanceToFirstBolt;
						if (!member.WinConnect.DistanceToFirstBolt_User)
						{
							member.WinConnect.Beam.DistanceToFirstBoltDisplay = rightBeam.WinConnect.Beam.DistanceToFirstBoltDisplay;
							if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToGirder)
								member.WinConnect.Beam.DistanceToFirstBoltDisplay += (leftBeam.WinConnect.Beam.TopElValue + -rightBeam.WinConnect.Beam.TopElValue);
						}
					}
					break;

			}

			if (!member.WinConnect.DistanceToFirstBolt_User)
				member.WinConnect.Beam.DistanceToFirstBoltDisplay = NumberFun.Round(member.WinConnect.Beam.DistanceToFirstBoltDisplay, ERoundingPrecision.Half, ERoundingStyle.RoundUp);

			if (member.ShearConnection == EShearCarriedBy.ClipAngle || member.ShearConnection == EShearCarriedBy.Tee)
			{
				if (member.WinConnect.Beam.TopCope && !member.WinConnect.DistanceToFirstBolt_User)
				{
					member.WinConnect.Beam.DistanceToFirstBolt -= member.WinConnect.Beam.TCopeD;
					if (!member.WinConnect.DistanceToFirstBolt_User)
						member.WinConnect.Beam.DistanceToFirstBoltDisplay += member.WinConnect.Beam.TopElValue;
				}
				else if (member.WinConnect.Beam.TopCope && member.WinConnect.DistanceToFirstBolt_User)
				{
					member.WinConnect.Beam.DistanceToFirstBolt = member.WinConnect.Beam.DistanceToFirstBoltDisplay - member.WinConnect.Beam.TCopeD;
					if (!member.WinConnect.DistanceToFirstBolt_User)
						member.WinConnect.Beam.DistanceToFirstBoltDisplay += member.WinConnect.Beam.TopElValue;
				}
			}
		}

		internal static double WeldBetaFactor(double wLength, double wSize)
		{
			if (wSize > 0)
				return Math.Min(1, 1.2 - 0.002 * wLength / wSize);
			else
				return 1;
		}

		internal static double BucklingStress(double kL_r, double YieldStress, bool enableReporting)
		{
			double BucklingStress;

			double Lc;
			double Fe;
			double Fcr;

			if (kL_r <= 25)
			{
				Fcr = YieldStress;
				if (enableReporting)
				{
					Reporting.AddLine("kL / r <= 25");
					Reporting.AddLine("Fcr = Fy = " + YieldStress + ConstUnit.Stress);
				}
			}
			else
			{
				Fe = Math.Pow(Math.PI, 2) * ConstNum.ELASTICITY / Math.Pow(kL_r, 2);
				if (Fe >= 0.44 * YieldStress && enableReporting)
				{
					Reporting.AddLine("Fe = pi² * E / (kL / r)² = 3.14² * " + ConstNum.ELASTICITY + " / " + kL_r + "²");
					Reporting.AddLine("= " + Fe + " >= 0.44 * Fy = 0.44 * " + YieldStress + " = " + 0.44 * YieldStress + ConstUnit.Stress);
				}
				else if(enableReporting)
				{
					Reporting.AddLine("Fe = pi² * E / (kL / r)² = 3.14² * " + ConstNum.ELASTICITY + " / " + kL_r + "²");
					Reporting.AddLine("= " + Fe + " << 0.44 * Fy = 0.44 * " + YieldStress + " = " + 0.44 * YieldStress + ConstUnit.Stress);
				}
				if (Fe >= 0.44 * YieldStress)
				{
					Lc = YieldStress / Fe;
					Fcr = Math.Pow(0.658, Lc) * YieldStress;

					if (enableReporting)
					{
						Reporting.AddLine("Fy / Fe = " + YieldStress + " / " + Fe + " = " + Lc);
						Reporting.AddLine("Fcr = 0.658^" + Lc + " * Fy = 0.658^" + Lc + " * " + YieldStress + " = " + Fcr + ConstUnit.Stress);
					}
				}
				else
				{
					Fcr = 0.877 * Fe;
					if (enableReporting)
						Reporting.AddLine("Fcr =  0.877 * Fe = 0.877 * " + Fe + " = " + Fcr + ConstUnit.Stress);
				}
			}

			BucklingStress = Fcr;
			return BucklingStress;
		}

		internal static double SpacingBearing(double spacing, double holeSize, double diameter, EBoltHoleType holeType, double fu, bool enableReport)
		{
			double spacingBearing;
			double Lc;
			double c;
			double C1;
			double Limit;
			double FiFactor;

			FiFactor = CommonDataStatic.IsFema ? 1 : ConstNum.FIOMEGA0_75N;

			if (holeType == EBoltHoleType.LSLN)
			{
				Limit = FiFactor * 2 * diameter * fu;
				C1 = 2;
				c = 1;
			}
			else
			{
				Limit = FiFactor * 2.4 * diameter * fu;
				C1 = 2.4;
				c = 1.2;
			}
			Lc = spacing - holeSize;
			spacingBearing = FiFactor * c * Lc * fu;

			if (enableReport)
			{
				Reporting.AddLine(String.Empty);
				Reporting.AddLine("Bearing Strength / Bolt / Thickness Using Bolt Spacing = Fbs");
				Reporting.AddLine("Bolt Spacing = " + spacing + ConstUnit.Length + ", Hole Size = " + holeSize + ConstUnit.Length);
				Reporting.AddLine("= " + (CommonDataStatic.IsFema ? "1" : ConstString.FIOMEGA0_75) + " * " + c + " * Lc * Fu <= " + (CommonDataStatic.IsFema ? "1" : ConstString.FIOMEGA0_75) + " * " + C1 + " * d * Fu = " + Limit + ConstUnit.ForcePerUnitLength);
				Reporting.AddLine("= " + (CommonDataStatic.IsFema ? "1" : ConstString.FIOMEGA0_75) + " * " + c + " * " + Lc + " * " + fu + " = " + spacingBearing + ConstUnit.ForcePerUnitLength);
			}
			if (spacingBearing > Limit)
			{
				spacingBearing = Limit;
				if (enableReport)
					Reporting.AddLine("Use: Fbs = " + spacingBearing + ConstUnit.ForcePerUnitLength);
			}

			return spacingBearing;
		}

		internal static double EdgeBearing(double E, double holeSize, double d, double Fu, EBoltHoleType holeType, bool enableReport)
		{
			double EdgeBearing;
			double Lc;
			double c;
			double C1;
			double limit;
			double fiFactor;
		    string fiFactorString;

			fiFactor = CommonDataStatic.IsFema ? 1 : ConstNum.FIOMEGA0_75N;
		    fiFactorString = CommonDataStatic.IsFema ? "1" : ConstString.FIOMEGA0_75;

			if (holeType == EBoltHoleType.LSLN)
			{
				limit = fiFactor * 2 * d * Fu;
				C1 = 2;
				c = 1;
			}
			else
			{
				limit = fiFactor * 2.4 * d * Fu;
				C1 = 2.4;
				c = 1.2;
			}
			Lc = (E - holeSize / 2);
			EdgeBearing = fiFactor * c * Lc * Fu;

			if (enableReport)
			{
				Reporting.AddLine("Bearing Strength / Bolt / Thickness Using Bolt Edge Distance = Fbe");
				Reporting.AddLine("Edge Dist. = " + E + ConstUnit.Length + ", Hole Size = " + holeSize + ConstUnit.Length);
				Reporting.AddLine("= " + fiFactorString + " * " + c + " * Lc * Fu  <= " + fiFactorString + " * " + C1 + " * d * Fu = " + limit + ConstUnit.ForcePerUnitLength);
				Reporting.AddLine("= " + fiFactorString + " * " + c + " * " + Lc + " * " + Fu + " = " + EdgeBearing + ConstUnit.ForcePerUnitLength);
				if (EdgeBearing > limit)
				{
					EdgeBearing = limit;
					Reporting.AddLine("Use: Fbe = " + EdgeBearing + ConstUnit.ForcePerUnitLength);
				}
			}

			return EdgeBearing;
		}

		public static double PlateThickness(double t)
		{
			double plateThickness;

			if (t <= 0.375)
				plateThickness = NumberFun.Round(t, 16);
			else if (t <= 1)
				plateThickness = NumberFun.Round(t, 8);
			else
				plateThickness = NumberFun.Round(t, 4);
			return plateThickness;
		}

		public static double WeldSize(double ws)
		{
			double WeldSize;

			if (CommonDataStatic.Units == EUnit.Metric)
			{
				WeldSize = Math.Floor(ws);
				return WeldSize;
			}

			return NumberFun.Round(ws, ERoundingPrecision.Sixteenth, ERoundingStyle.Nearest);
		}

		internal static double MinimumWeld(double t1, double t2)
		{
			double minimumWeld;

			double tempWeld;
			double weld;

			tempWeld = Math.Min(t1, t2);

			if (tempWeld <= ConstNum.QUARTER_INCH)
				weld = ConstNum.EIGHTH_INCH;
			else if (tempWeld <= ConstNum.HALF_INCH)
				weld = ConstNum.THREE_SIXTEENTHS;
			else if (tempWeld <= NumberFun.ConvertFromFraction(12))
				weld = ConstNum.QUARTER_INCH;
			else
				weld = NumberFun.ConvertFromFraction(5);

			minimumWeld = Math.Max(weld, ConstNum.THREE_SIXTEENTHS);
			return minimumWeld;
		}

		internal static double WeldTypeRatio()
		{
			switch (CommonDataStatic.Units)
			{
				case EUnit.US:
					if (CommonDataStatic.Preferences.DefaultElectrode.Fexx <= 70)
						return CommonDataStatic.Preferences.DefaultElectrode.Fexx / 70;
					else if (CommonDataStatic.Preferences.DefaultElectrode.Fexx <= 90)
						return 0.9 * CommonDataStatic.Preferences.DefaultElectrode.Fexx / 70;
					else
						return 0.85 * CommonDataStatic.Preferences.DefaultElectrode.Fexx / 70;
				default: // Metric
					if (CommonDataStatic.Preferences.DefaultElectrode.Fexx <= 480)
						return CommonDataStatic.Preferences.DefaultElectrode.Fexx / 480;
					else if (CommonDataStatic.Preferences.DefaultElectrode.Fexx <= 620)
						return 0.9 * CommonDataStatic.Preferences.DefaultElectrode.Fexx / 480;
					else
						return 0.85 * CommonDataStatic.Preferences.DefaultElectrode.Fexx / 480;
			}
		}
	}
}