using System;
using System.Linq;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class MiscCalculationsWithReporting
	{
		internal static double HSSSideWallCripling(EMemberType memberType, double R, double L)
		{
			double FiRn;
			double Qf;
			double U;
			double fc;
			double s1;
			double Be;
			double dd;

			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var beam = CommonDataStatic.DetailDataDict[memberType];

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
			{
				dd = column.Shape.B - 3 * column.Shape.tf;
				Be = beam.Shape.bf / column.Shape.bf;
			}
			else
			{
				dd = column.Shape.bf - 3 * column.Shape.tf;
				Be = beam.Shape.bf / column.Shape.B;
			}

			s1 = 1 / 12.0 * (column.Shape.bf * Math.Pow(column.Shape.B, 3) - (column.Shape.bf - 2 * column.Shape.tf) * Math.Pow(column.Shape.B - 2 * column.Shape.tf, 3)) / (column.Shape.B / 2);

			fc = CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD ? 0.6 * column.Material.Fy : column.Material.Fy;

			U = Math.Abs(column.P / (column.Shape.a * fc) + beam.Moment / (s1 * fc));
			Qf = Math.Min(1.3 - 0.4 * U / Be, 1);
			FiRn = ConstNum.FIOMEGA0_75N * 1.6 * Math.Pow(column.Shape.tw, 2) * (1 + 3 * L / dd) * Math.Pow(ConstNum.ELASTICITY * fc, 0.5) * Qf;

			Reporting.AddHeader("Local Crippling of HSS Sidewalls:");

			Reporting.AddLine("U = |Pr / (Ac * Fc) + Mr / (Sc * Fc)|");
			Reporting.AddLine("=| " + column.P + " / (" + column.Shape.a + " * " + fc + ") + " + beam.Moment + " / (" + s1 + " * " + fc + ")) |");
			Reporting.AddLine("= " + U);

			Reporting.AddLine(string.Empty);
			Reporting.AddLine("Qf = Min(1.3 - 0.4 * U / (bf / B); 1) = Min(1.3 - 0.4 * " + U + " / (" + column.Shape.bf + " / " + Be + "); 1) = " + Qf);

			Reporting.AddLine(string.Empty);
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 1.6 * t² * (1 + 3 * N / H) * (E * Fy)^ 0.5 * Qf");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 1.6 * " + column.Shape.tw + "² * (1 + 3 * " + L + " / " + dd + ") * (" + ConstNum.ELASTICITY + " * " + column.Material.Fy + ")^0.5 * " + Qf);
			if (FiRn >= R)
				Reporting.AddCapacityLine("= " + FiRn + " >= " + R + ConstUnit.Force + " (OK)", R / FiRn, "Local Crippling of HSS Sidewalls", EMemberType.PrimaryMember);
			else
				Reporting.AddCapacityLine("= " + FiRn + " << " + R + ConstUnit.Force + " (NG)", R / FiRn, "Local Crippling of HSS Sidewalls", EMemberType.PrimaryMember);

			return Qf;
		}

		internal static void HSSSideWallYielding(double h, double V, double n, double tf, double L)
		{
			double FiRn;
			double k;
			double R;

			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			if (V == 0)		// V = 0 for moment connections
				R = h;
			else
				R = Math.Pow(Math.Pow(h, 2) + Math.Pow(1.73 * V, 2), 0.5);
			
			k = 1.5 * column.Shape.tw + tf;
			FiRn = 2 * ConstNum.FIOMEGA1_0N * column.Material.Fy * column.Shape.tw * Math.Min(5 * k + n, L);

			Reporting.AddHeader("Local Yielding of HSS Sidewalls:");
			if (R != h)
			{
				Reporting.AddHeader("Resultant force:");
				Reporting.AddLine("R = (H² + (1.73 * V)²)^0.5");
				Reporting.AddLine("= ((" + h + " )² + ((1.73 * " + V + ") )²)^0.5");
				Reporting.AddLine("= " + R + ConstUnit.Force);
			}
			Reporting.AddLine("k = 1.5 * t + tf = 1.5 * " + column.Shape.tw + " + " + tf + " = " + k + ConstUnit.Length);
			Reporting.AddLine("N = " + n + ConstUnit.Length);

			Reporting.AddLine(ConstString.PHI + " Rn = 2 * " + ConstString.FIOMEGA1_0 + " * Fy * t * Min((5 * k + N), L)");
			Reporting.AddLine("= 2 * " + ConstString.FIOMEGA1_0 + " * " + column.Material.Fy + " * " + column.Shape.tw + " * Min((5 * " + k + " + " + n + "), " + L + ")");
			if (FiRn >= R)
				Reporting.AddCapacityLine("= " + FiRn + " >= " + R + ConstUnit.Force + " (OK)", R / FiRn, "Local Yielding of HSS Sidewalls", EMemberType.PrimaryMember);
			else
				Reporting.AddCapacityLine("= " + FiRn + " << " + R + ConstUnit.Force + " (NG)", R / FiRn, "Local Yielding of HSS Sidewalls", EMemberType.PrimaryMember);
		}

		internal static void HSSSideWallBuckling(double R, double qf)
		{
			double FiRn;
			double d_flat;

			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			if(CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
				d_flat = column.Shape.d - 3 * column.Shape.tw;
			else
				d_flat = column.Shape.bf - 3 * column.Shape.tw;

			FiRn = ConstNum.FIOMEGA0_9N * 48 * Math.Pow(column.Shape.tw, 3) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy, 0.5) * qf / d_flat;

			Reporting.AddHeader("Local Buckling of HSS Sidewalls:");
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * 48 * t³ * (E * Fy)^0.5 * Qf / Hf");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 48 * " + column.Shape.tw + "³ * (" + ConstNum.ELASTICITY + " * " + column.Material.Fy + ")^0.5 * " + qf + " / " + d_flat);
			if (FiRn >= R)
				Reporting.AddCapacityLine("= " + FiRn + " >= " + R + ConstUnit.Force + " (OK)", R / FiRn, "Local Buckling of HSS Sidewalls", EMemberType.PrimaryMember);
			else
				Reporting.AddCapacityLine("= " + FiRn + " << " + R + ConstUnit.Force + " (NG)", R / FiRn, "Local Buckling of HSS Sidewalls", EMemberType.PrimaryMember);
		}

		internal static void HSSColumnYieldLineCheck(EMemberType memberType)
		{

			double r;
			double WSFOriginal;
			double WSFR;
			double B;
			double yieldLineFactor;
			double WSF;

			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var beam = CommonDataStatic.DetailDataDict[memberType];

			Reporting.AddHeader("Column Check:");
			yieldLineFactor = YieldLineFactor(memberType, true);

			switch (CommonDataStatic.Preferences.CalcMode)
			{
				case ECalcMode.LRFD:
					Reporting.AddHeader("HSS Wall Strength Factor (RuW / t²)");
					break;
				case ECalcMode.ASD:
					Reporting.AddHeader("HSS Wall Strength Factor (RaW / t²)");
					break;
			}

			WSF = ConstNum.FIOMEGA0_9N * yieldLineFactor * column.Material.Fy * beam.WinConnect.ShearSeat.StiffenerLength / (0.8 * 4);
			Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * k * Fy * L / (0.8 * 4)");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + yieldLineFactor + " * " + column.Material.Fy + " * " + beam.WinConnect.ShearSeat.StiffenerLength + " / (0.8 * 4) = " + WSF + ConstUnit.Force + " / " + ConstUnit.Length);

			beam.WinConnect.Fema.L = column.WinConnect.ShearSeat.StiffenerLength;

			B = CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb ? column.Shape.d : column.Shape.bf;

			WSFR = ReducedWSF.ReducedWSFCalc(B, beam.WinConnect.Fema.L);
			if (WSFR > 0)
			{
				Reporting.AddHeader("HSS Wall Strength Factor (WSFR)");
				Reporting.AddLine("Reduced based on 2005 Specification, Section K1");
				Reporting.AddLine("WSFR = " + WSFR + ConstUnit.Force + " / " + ConstUnit.Length);

				WSFOriginal = WSF;
				WSF = Math.Min(WSF, WSFR);

				Reporting.AddLine("WSF = Min(WSF, WSFR) = Min(" + WSFOriginal + ", " + WSFR + ") = " + WSF + ConstUnit.Force + " / " + ConstUnit.Length);
			}
			else
				Reporting.AddHeader("2005 Specification, Section K1 does not control.");

			Reporting.AddHeader("Yield Line Strength:");

			if (CommonDataStatic.Preferences.CalcMode == ECalcMode.LRFD)
				Reporting.AddLine("Ru = Wall Strength Factor * t² / W");
			else if (CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD)
				Reporting.AddLine("Ra = Wall Strength Factor * t² / W");

			r = WSF * Math.Pow(column.Shape.tf, 2) / beam.WinConnect.ShearSeat.PlateWidth;
			Reporting.AddLine("= WSF * t² / W = " + WSF + " * " + column.Shape.tf + "² / " + beam.WinConnect.ShearSeat.PlateWidth);
			if (r >= beam.ShearForce)
				Reporting.AddCapacityLine("= " + r + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / r, "HSS Wall Strength Factor", memberType);
			else
				Reporting.AddCapacityLine("= " + r + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / r, "HSS Wall Strength Factor", memberType);
		}

		internal static void EffectiveSupThickness(EMemberType memberType, ref double SupThickness, ref double SupThickness1)
		{
			double shearForceLeft;
			double shearForceRight;
			double numberOfBoltsMin;
			double numberOfBoltsRight = 0;
			double numberOfBoltsLeft = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
			{
				SupThickness = column.Shape.tf;
				SupThickness1 = column.Shape.tf;
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
			{
				switch (rightBeam.ShearConnection)
				{
					case EShearCarriedBy.EndPlate:
						numberOfBoltsRight = rightBeam.WinConnect.ShearEndPlate.Bolt.NumberOfRows;
						break;
					case EShearCarriedBy.Tee:
						numberOfBoltsRight = rightBeam.WinConnect.ShearWebTee.BoltOslOnFlange.NumberOfRows;
						break;
					case EShearCarriedBy.ClipAngle:
						if (rightBeam.WinConnect.ShearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
							numberOfBoltsRight = rightBeam.WinConnect.ShearClipAngle.BoltOslOnSupport.NumberOfRows;
						break;
				}

				switch (leftBeam.ShearConnection)
				{
					case EShearCarriedBy.EndPlate:
						numberOfBoltsLeft = leftBeam.WinConnect.ShearEndPlate.Bolt.NumberOfRows;
						break;
					case EShearCarriedBy.Tee:
						numberOfBoltsLeft = leftBeam.WinConnect.ShearWebTee.BoltOslOnFlange.NumberOfRows;
						break;
					case EShearCarriedBy.ClipAngle:
						if (leftBeam.WinConnect.ShearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
							numberOfBoltsLeft = leftBeam.WinConnect.ShearClipAngle.BoltOslOnSupport.NumberOfRows;
						break;
				}

				if (numberOfBoltsLeft > 0 && numberOfBoltsRight > 0)
				{
					double numberOfBoltsAligned = DetermineBoltAlignment(memberType, false);
					if (numberOfBoltsAligned > 0)
						numberOfBoltsMin = Math.Min(numberOfBoltsAligned, Math.Min(numberOfBoltsLeft, numberOfBoltsRight));
					else
						numberOfBoltsMin = Math.Min(numberOfBoltsLeft, numberOfBoltsRight);

					shearForceRight = Math.Abs(rightBeam.ShearForce);
					shearForceLeft = Math.Abs(leftBeam.ShearForce);
					if (beam.MemberType == EMemberType.LeftBeam)
					{
						SupThickness = column.Shape.tw * (1 - numberOfBoltsMin / numberOfBoltsLeft * (shearForceRight / numberOfBoltsRight) / (shearForceRight / numberOfBoltsRight + shearForceLeft / numberOfBoltsLeft));
						SupThickness1 = column.Shape.tw;
					}
					else
					{
						SupThickness = column.Shape.tw * (1 - numberOfBoltsMin / numberOfBoltsRight * (shearForceLeft / numberOfBoltsLeft) / (shearForceRight / numberOfBoltsRight + shearForceLeft / numberOfBoltsLeft));
						SupThickness1 = column.Shape.tw;
					}
				}
				else
				{
					SupThickness = column.Shape.tw;
					SupThickness1 = column.Shape.tw;
				}
			}
		}

		internal static double DetermineBoltAlignment(EMemberType memberType, bool enableReporting)
		{
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			double rightBeamBoltOneValue = rightBeam.WinConnect.Beam.TopElValue - rightBeam.WinConnect.Beam.DistanceToFirstBoltDisplay;
			double leftBeamBoltOneValue = leftBeam.WinConnect.Beam.TopElValue - leftBeam.WinConnect.Beam.DistanceToFirstBoltDisplay;

			double[] rightBeamBoltValues;
			double[] leftBeamBoltValues;

			int rightNumberOfRows = 0;
			double rightSpacingLongDir = 0;
			double rightMinSpacing = 0;
			
			int leftNumberOfRows = 0;
			double leftSpacingLongDir = 0;
			double leftMinSpacing = 0;

			if (!rightBeam.IsActive || !leftBeam.IsActive)
				return 0;

			// First step is to get the value index of each bolt for the current shear connection
			switch (rightBeam.ShearConnection)
			{
				case EShearCarriedBy.ClipAngle:
					rightNumberOfRows = rightBeam.WinConnect.ShearClipAngle.BoltOslOnSupport.NumberOfRows;
					rightSpacingLongDir = rightBeam.WinConnect.ShearClipAngle.BoltOslOnSupport.SpacingLongDir;
					rightMinSpacing = rightBeam.WinConnect.ShearClipAngle.BoltOslOnSupport.MinSpacing;
					break;
				case EShearCarriedBy.EndPlate:
					rightNumberOfRows = rightBeam.WinConnect.ShearEndPlate.Bolt.NumberOfRows;
					rightSpacingLongDir = rightBeam.WinConnect.ShearEndPlate.Bolt.SpacingLongDir;
					rightMinSpacing = rightBeam.WinConnect.ShearEndPlate.Bolt.MinSpacing;
					break;
				case EShearCarriedBy.Tee:
					rightNumberOfRows = rightBeam.WinConnect.ShearWebTee.BoltOslOnFlange.NumberOfRows;
					rightSpacingLongDir = rightBeam.WinConnect.ShearWebTee.BoltOslOnFlange.SpacingLongDir;
					rightMinSpacing = rightBeam.WinConnect.ShearWebTee.BoltOslOnFlange.MinSpacing;
					break;
			}

			// Fill right beam values
			rightBeamBoltValues = new double[rightNumberOfRows];
			for (int i = 0; i < rightNumberOfRows; i++)
			{
				if (i == 0)
					rightBeamBoltValues[i] = rightBeamBoltOneValue;
				else
					rightBeamBoltValues[i] = rightBeamBoltValues[i - 1] - rightSpacingLongDir;
			}

			switch (leftBeam.ShearConnection)
			{
				case EShearCarriedBy.ClipAngle:
					leftNumberOfRows = leftBeam.WinConnect.ShearClipAngle.BoltOslOnSupport.NumberOfRows;
					leftSpacingLongDir = leftBeam.WinConnect.ShearClipAngle.BoltOslOnSupport.SpacingLongDir;
					leftMinSpacing = leftBeam.WinConnect.ShearClipAngle.BoltOslOnSupport.MinSpacing;
					break;
				case EShearCarriedBy.EndPlate:
					leftNumberOfRows = leftBeam.WinConnect.ShearEndPlate.Bolt.NumberOfRows;
					leftSpacingLongDir = leftBeam.WinConnect.ShearEndPlate.Bolt.SpacingLongDir;
					leftMinSpacing = leftBeam.WinConnect.ShearEndPlate.Bolt.MinSpacing;
					break;
				case EShearCarriedBy.Tee:
					leftNumberOfRows = leftBeam.WinConnect.ShearWebTee.BoltOslOnFlange.NumberOfRows;
					leftSpacingLongDir = leftBeam.WinConnect.ShearWebTee.BoltOslOnFlange.SpacingLongDir;
					leftMinSpacing = leftBeam.WinConnect.ShearWebTee.BoltOslOnFlange.MinSpacing;
					break;
			}

			// Fill left beam values
			leftBeamBoltValues = new double[leftNumberOfRows];
			for (int i = 0; i < leftNumberOfRows; i++)
			{
				if (i == 0)
					leftBeamBoltValues[i] = leftBeamBoltOneValue;
				else
					leftBeamBoltValues[i] = leftBeamBoltValues[i - 1] - leftSpacingLongDir;
			}

			if (rightBeamBoltValues.Count() < 0 || leftBeamBoltValues.Count() < 1)
				return 0;

			// If any of the values are in the range of the opposite beam, but do not match any bolts there will be an NG
			if (enableReporting)
			{
				bool notAligned = false;
				double rightMax = rightBeamBoltValues.Max();
				double rightMin = rightBeamBoltValues.Min();
				double leftMax = leftBeamBoltValues.Max();
				double leftMin = leftBeamBoltValues.Min();

				double rightSpacing = leftMin - rightMax;
				double leftSpacing = rightMin - leftMax;

				foreach (var value in rightBeamBoltValues)
				{
					if (value <= leftMax && value >= leftMin && !leftBeamBoltValues.Contains(value))
						notAligned = true;
				}

				foreach (var value in leftBeamBoltValues)
				{
					if (value <= rightMax && value >= rightMin && !rightBeamBoltValues.Contains(value))
						notAligned = true;
				}

				if (notAligned)
					Reporting.AddLine("RIGHT AND LEFT SIDE BOLTS DO NOT ALIGN (NG)");
				if (memberType == EMemberType.RightBeam && rightSpacing > 0 && rightMinSpacing > rightSpacing)
					Reporting.AddLine("Spacing = " + (leftMin - rightMax) + " << Minimum Spacing = " + rightMinSpacing + ConstUnit.Length + " (NG)");
				if (memberType == EMemberType.LeftBeam && leftSpacing > 0 && leftMinSpacing > leftSpacing)
					Reporting.AddLine("Spacing = " + (rightMin - leftMax) + "<< Minimum Spacing = " + rightMinSpacing + ConstUnit.Length + " (NG)");
			}

			// Return the number of matched bolts
			return rightBeamBoltValues.Intersect(leftBeamBoltValues).Count();
		}

		internal static double AngleGageMinimum(double d, int n)
		{
			double g;

			d = d / ConstNum.ONE_INCH;

			if (n == 2)
			{
				if (d <= 0.625)
					g = 1 + 15 / 16.0;
				else if (d <= 0.75)
					g = 1.375 + 0.75; 
				else if (d <= 0.875)
					g = 1.5 + 0.875; 
				else if (d <= 1)
					g = 1.625 + 15 / 16.0; 
				else if (d <= 1.125)
					g = 1.875 + 1.0625;
				else if (d <= 1.25)
					g = 2 + 1.125;
				else if (d <= 1.375)
					g = 2.125 + 1.25;
				else if (d <= 1.5)
					g = 2.25 + 1.3125;
				else
					g = 2.113 * d + 0.997;
			}
			else
				g = d + 0.5;

			return ConstNum.ONE_INCH * NumberFun.Round(g, 8);
		}

		internal static double BeamWebTearout(EMemberType memberType, int nv, int nh, double sv, double sh, EBeamOrAttachment BeamOrAttachment, ETearOutBlockShear TearOutOrBlockShear, bool enableReporting)
		{
			double beamWebTearout = 0;
			double Agt;
			double Agv;
			double Ant;
			double Anv;
			double Lengt = 0;
			double V = 0;
			double t = 0;
			double VertHole = 0;
			double HorizHole = 0;
			double HorizEdge = 0;
			double fy = 0;
			double fu = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];

			switch (BeamOrAttachment)
			{
				case EBeamOrAttachment.Beam:
					HorizEdge = beam.WinConnect.Beam.Lh;
					HorizHole = beam.BoltBrace.HoleLength;
					VertHole = beam.BoltBrace.HoleWidth;
					fu = beam.WinConnect.Beam.Material.Fu;
					fy = beam.WinConnect.Beam.Material.Fy;
					t = beam.Shape.tw;
					V = beam.P;
					Lengt = beam.WinConnect.Beam.WebHeight;
					break;
				case EBeamOrAttachment.Angle:
					HorizEdge = beam.WinConnect.ShearClipAngle.BoltWebOnBeam.EdgeDistTransvDir;
					HorizHole = beam.WinConnect.ShearClipAngle.BoltWebOnBeam.HoleLength;
					VertHole = beam.WinConnect.ShearClipAngle.BoltWebOnBeam.HoleWidth;
					fu = beam.WinConnect.ShearClipAngle.Material.Fu;
					fy = beam.WinConnect.ShearClipAngle.Material.Fy;
					t = beam.WinConnect.ShearClipAngle.Size.t;
					Lengt = beam.WinConnect.ShearClipAngle.Length;
					V = beam.P / beam.WinConnect.ShearClipAngle.Number;
					break;
				case EBeamOrAttachment.WebTee:
					HorizEdge = beam.WinConnect.ShearWebTee.BoltWebOnStem.EdgeDistTransvDir;
					HorizHole = beam.WinConnect.ShearWebTee.BoltWebOnStem.HoleLength;
					VertHole = beam.WinConnect.ShearWebTee.BoltWebOnStem.HoleWidth;
					fu = beam.WinConnect.ShearWebTee.Material.Fu;
					fy = beam.WinConnect.ShearWebTee.Material.Fy;
					t = beam.WinConnect.ShearWebTee.Size.t;
					Lengt = beam.WinConnect.ShearWebTee.SLength;
					V = beam.P;
					break;
				case EBeamOrAttachment.SinglePlate:
					HorizEdge = beam.WinConnect.ShearWebPlate.Bolt.EdgeDistTransvDir;
					HorizHole = beam.WinConnect.ShearWebPlate.Bolt.HoleLength;
					VertHole = beam.WinConnect.ShearWebPlate.Bolt.HoleWidth;
					fu = beam.WinConnect.ShearWebPlate.Material.Fu;
					fy = beam.WinConnect.ShearWebPlate.Material.Fy;
					t = beam.Shape.t;
					Lengt = beam.WinConnect.ShearWebPlate.Length;
					V = beam.P;
					break;
			}

			Anv = 2 * (HorizEdge + sh * (nh - 1) - (HorizHole + ConstNum.SIXTEENTH_INCH) * (nh - 0.5));
			Ant = (nv - 1) * (sv - (VertHole + ConstNum.SIXTEENTH_INCH));
			Agv = 2 * (HorizEdge + sh * (nh - 1));
			Agt = (nv - 1) * sv;

			switch (TearOutOrBlockShear)
			{
				case ETearOutBlockShear.TearOut:
					if (BeamOrAttachment != EBeamOrAttachment.Beam || (BeamOrAttachment == EBeamOrAttachment.Beam && beam.WinConnect.Beam.TopCope && beam.WinConnect.Beam.BottomCope))
					{
						beamWebTearout = ConstNum.FIOMEGA0_75N * (Lengt - nv * (VertHole + ConstNum.SIXTEENTH_INCH)) * t * fu;
						if (enableReporting)
						{
							Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * (L - nv * (dv + " + ConstNum.SIXTEENTH_INCH + ") * t * Fu");
							Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * (" + Lengt + " - " + nv + " * " + (VertHole + ConstNum.SIXTEENTH_INCH) + ") * " + t + " * " + fu);
							if (beamWebTearout >= V)
								Reporting.AddLine("= " + beamWebTearout + " >= " + V + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= " + beamWebTearout + " << " + V + ConstUnit.Force + " (NG)");
						}
					}
					break;
				case ETearOutBlockShear.BlockShear:
					beamWebTearout = BlockShearNew(fu, Anv, 1, Ant, Agv, fy, t, V, enableReporting);
					if (enableReporting)
					{
						switch (BeamOrAttachment)
						{
							case EBeamOrAttachment.Beam:
								Reporting.AddHeader("Beam Web Block Shear under Axial Load:");
								break;
							case EBeamOrAttachment.WebTee:
								Reporting.AddHeader("Tee Stem Block Shear under Beam Axial Load:");
								break;
							case EBeamOrAttachment.SinglePlate:
								Reporting.AddHeader("Plate Block Shear under Beam Axial Load:");
								break;
							default:
								Reporting.AddHeader("Angle Block Shear under Beam Axial Load:");
								break;
						}
						Reporting.AddHeader("Shear Area Length (net), Lnv = 2 * (Lh + sh * (nh - 1) - (dh + " + ConstNum.SIXTEENTH_INCH + ") * (nh - 0.5))");
						Reporting.AddLine("= 2 * (" + HorizEdge + " + " + sh + " * (" + nh + " -1) - " + (HorizHole + ConstNum.SIXTEENTH_INCH) + " * (" + nh + " - 0.5))");
						Reporting.AddLine("= " + Anv + ConstUnit.Length);

						Reporting.AddHeader("Shear Area Length (gross), Lgv = 2 * (Lh + sh * (nh - 1))");
						Reporting.AddLine("= 2 * (" + HorizEdge + " + " + sh + " * (" + nh + " - 1))");
						Reporting.AddLine("= " + Agv + ConstUnit.Length);

						Reporting.AddHeader("Tension Area Length (net), Lnt = (nv - 1) * (sv - (dv + " + ConstNum.SIXTEENTH_INCH + "))");
						Reporting.AddLine("= (" + nv + " - 1) * (" + sv + " - " + (VertHole + ConstNum.SIXTEENTH_INCH) + ")");
						Reporting.AddLine("= " + Ant + ConstUnit.Length);

						Reporting.AddHeader("Tension Area Length (gross), Lgt = (nv - 1) * sv");
						Reporting.AddLine("= (" + nv + " - 1) * " + sv);
						Reporting.AddLine("= " + Agt + ConstUnit.Length);
					}
					break;
			}

			return beamWebTearout;
		}

		internal static double BlockShearNew(double Fu, double Lnv, double Ubs, double Lnt, double Lgv, double Fy, double t, double V, bool enableReporting)
		{
			double BlockShearNew;
			double FiRn;

			if (t != 0)
			{
				FiRn = ConstNum.FIOMEGA0_75N * Math.Min(0.6 * Fu * Lnv + Ubs * Fu * Lnt, 0.6 * Fy * Lgv + Ubs * Fu * Lnt) * t;
				BlockShearNew = FiRn;
				if (enableReporting)
				{
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Min((0.6 * Fu * Lnv + Ubs * Fu * Lnt); (0.6 * Fy * Lgv + Ubs * Fu * Lnt)) * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * Min((0.6 * " + Fu + " * " + Lnv + " + " + Ubs + " * " + Fu + " * " + Lnt + "); (0.6 * " + Fy + " * " + Lgv + " + " + Ubs + " * " + Fu + " * " + Lnt + ")) * " + t);
					if (V > 0)
					{
						if (FiRn >= V)
							Reporting.AddCapacityLine("= " + FiRn + " >= " + V + ConstUnit.Force + " (OK)", V / FiRn, "Block Shear", EMemberType.PrimaryMember);
						else
							Reporting.AddCapacityLine("= " + FiRn + " << " + V + ConstUnit.Force + " (NG)", V / FiRn, "Block Shear", EMemberType.PrimaryMember);
					}
				}
			}
			else
			{
				FiRn = ConstNum.FIOMEGA0_75N * Math.Min(0.6 * Fu * Lnv + Ubs * Fu * Lnt, 0.6 * Fy * Lgv + Ubs * Fu * Lnt);
				BlockShearNew = FiRn;
				if (enableReporting)
				{
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Min((0.6 * Fu * Anv + Ubs * Fu * Ant); (0.6 * Fy * Agv + Ubs * Fu * Ant)) ");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * Min((0.6 * " + Fu + " * " + Lnv + " + " + Ubs + " * " + Fu + " * " + Lnt + "); (0.6 * " + Fy + " * " + Lgv + " + " + Ubs + " * " + Fu + " * " + Lnt + "))");
					if (V > 0)
					{
						if (FiRn >= V)
							Reporting.AddCapacityLine("= " + FiRn + " >= " + V + ConstUnit.Force + " (OK)", V / FiRn, "Block Shear", EMemberType.PrimaryMember);
						else
							Reporting.AddCapacityLine("= " + FiRn + " << " + V + ConstUnit.Force + " (NG)", V / FiRn, "Block Shear", EMemberType.PrimaryMember);
					}
				}
			}

			return BlockShearNew;
		}

		private static void BlockShearCapacity(EMemberType memberType, EShearOpt shearopt, ref double Ubs, ref double Beamlv, ref double Lnt, ref double Lnv, ref double Lgt, ref double Lgv, double webd, ref double Lnv2, ref double PhiRn, ref double PhiRn2, ref double h, ref double Aw, ref int a, ref double PhiRg, ref double BsCap)
		{
			int boltNumRows = 0;
			double boltWidth = 0;
			double Anv;
			double PhiRn_rupture;

			var beam = CommonDataStatic.DetailDataDict[memberType];

			Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Shear Strength of the Beam:");
			if (beam.WinConnect.Beam.TopCope)
			{
				Reporting.AddHeader("Block Shear:");
				switch (shearopt)
				{
					case EShearOpt.ClipAngle:
						Ubs = 1;
						Beamlv = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (beam.WinConnect.ShearClipAngle.FrontY + beam.WinConnect.ShearClipAngle.Length / 2 - beam.WinConnect.ShearClipAngle.BoltWebOnBeam.EdgeDistLongDir);
						if (Beamlv < beam.WinConnect.Beam.DistanceToFirstBolt)
							Beamlv = beam.WinConnect.Beam.DistanceToFirstBolt;
						Lnt = beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH - (beam.WinConnect.ShearClipAngle.BoltWebOnBeam.HoleLength + ConstNum.SIXTEENTH_INCH) / 2;
						Lnv = ((beam.WinConnect.ShearClipAngle.BoltWebOnBeam.NumberOfRows - 1) * (beam.WinConnect.ShearClipAngle.BoltWebOnBeam.SpacingLongDir - (beam.WinConnect.ShearClipAngle.BoltWebOnBeam.HoleWidth + ConstNum.SIXTEENTH_INCH)) + Beamlv - (beam.WinConnect.ShearClipAngle.BoltWebOnBeam.HoleWidth + ConstNum.SIXTEENTH_INCH) / 2);
						Lgt = beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH;
						Lgv = (beam.WinConnect.ShearClipAngle.BoltWebOnBeam.NumberOfRows - 1) * beam.WinConnect.ShearClipAngle.BoltWebOnBeam.SpacingLongDir + Beamlv;

                        Reporting.AddLine(String.Empty);
						Reporting.AddLine("Net Length with Tension resistance (Lnt)");
						Reporting.AddLine("= lh - " + ConstNum.QUARTER_INCH + " - (dh + " + ConstNum.SIXTEENTH_INCH + ") / 2 = " + (beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH) + " - " + beam.WinConnect.ShearClipAngle.BoltWebOnBeam.HoleLength + ConstNum.SIXTEENTH_INCH + " / 2 = " + Lnt + ConstUnit.Length);

                        Reporting.AddLine(String.Empty);
						Reporting.AddLine("Gross Length with Tension resistance (Lgt) = Lh - " + ConstNum.QUARTER_INCH + " = " + Lgt + ConstUnit.Length);

                        Reporting.AddLine(String.Empty);
						Reporting.AddLine("Net Length with Shear resistance, Lnv");
						Reporting.AddLine("= ((n - 1) * (s - (dv + " + ConstNum.SIXTEENTH_INCH + ")) + Lv - (dv + " + ConstNum.SIXTEENTH_INCH + ") / 2) ");
						Reporting.AddLine("= ((" + beam.WinConnect.ShearClipAngle.BoltWebOnBeam.NumberOfRows + " - 1) * (" + beam.WinConnect.ShearClipAngle.BoltWebOnBeam.SpacingLongDir + " - " + (beam.WinConnect.ShearClipAngle.BoltWebOnBeam.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") + " + Beamlv + " - " + (beam.WinConnect.ShearClipAngle.BoltWebOnBeam.HoleWidth + ConstNum.SIXTEENTH_INCH) + " / 2) ");
						Reporting.AddLine("= " + Lnv + ConstUnit.Length);

                        Reporting.AddLine(String.Empty);
						Reporting.AddLine("Gross Length with Shear resistance (Lgv)");
						Reporting.AddLine("= (n - 1) * s + Lv");
						Reporting.AddLine("= (" + beam.WinConnect.ShearClipAngle.BoltWebOnBeam.NumberOfRows + " - 1) * " + beam.WinConnect.ShearClipAngle.BoltWebOnBeam.SpacingLongDir + " + " + Beamlv + " = " + Lgv + ConstUnit.Length);

						if (beam.WinConnect.Beam.BottomCope)
						{
							Lnv2 = webd - beam.WinConnect.ShearClipAngle.BoltWebOnBeam.NumberOfRows * (beam.WinConnect.ShearClipAngle.BoltWebOnBeam.HoleWidth + ConstNum.SIXTEENTH_INCH);

                            Reporting.AddLine(String.Empty);
							Reporting.AddLine("Net Length with Shear resistance (no tensile area), Lnv'");
							Reporting.AddLine("= h - n * (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + webd + " - " + beam.WinConnect.ShearClipAngle.BoltWebOnBeam.NumberOfRows + " * " + (beam.WinConnect.ShearClipAngle.BoltWebOnBeam.HoleWidth + ConstNum.SIXTEENTH_INCH));
							Reporting.AddLine("= " + Lnv2 + ConstUnit.Length);
						}
						break;
					case EShearOpt.WebTee:
						Ubs = 1;
						Beamlv = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (beam.WinConnect.ShearWebTee.FrontY + beam.WinConnect.ShearWebTee.SLength / 2 - beam.WinConnect.ShearWebTee.BoltOslOnFlange.EdgeDistLongDir);
						if (Beamlv < beam.WinConnect.Beam.DistanceToFirstBolt)
							Beamlv = beam.WinConnect.Beam.DistanceToFirstBolt;
						Lnt = beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH - (beam.WinConnect.ShearWebTee.BoltOslOnFlange.HoleLength + ConstNum.SIXTEENTH_INCH) / 2;
						Lnv = ((beam.WinConnect.ShearWebTee.BoltOslOnFlange.NumberOfBolts - 1) * (beam.WinConnect.ShearWebTee.BoltOslOnFlange.SpacingLongDir - (beam.WinConnect.ShearWebTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH)) + Beamlv - (beam.WinConnect.ShearWebTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH) / 2);
						Lgt = beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH;
						Lgv = (beam.WinConnect.ShearWebTee.BoltOslOnFlange.NumberOfBolts - 1) * beam.WinConnect.ShearWebTee.BoltOslOnFlange.SpacingLongDir + Beamlv;

                        Reporting.AddLine(String.Empty);
						Reporting.AddLine("Net Length with Tension resistance (Lnt)");
						Reporting.AddLine("= lh - " + ConstNum.QUARTER_INCH + " - (dh + " + ConstNum.SIXTEENTH_INCH + ") / 2 = " + (beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH) + " - " + (beam.WinConnect.ShearWebTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH) + " / 2 = " + Lnt + ConstUnit.Length);

                        Reporting.AddLine(String.Empty);
                        Reporting.AddLine("Gross Length with Tension resistance (Lgt) = Lh - " + ConstNum.QUARTER_INCH + " = " + Lgt + ConstUnit.Length);

                        Reporting.AddLine(String.Empty);
						Reporting.AddLine("Net Length with Shear resistance (Lnv)");
						Reporting.AddLine("= ((n - 1) * (s - (dv + " + ConstNum.SIXTEENTH_INCH + ")) + Lv - (dv + " + ConstNum.SIXTEENTH_INCH + ") / 2) ");
						Reporting.AddLine("= ((" + beam.WinConnect.ShearWebTee.BoltOslOnFlange.NumberOfBolts + " - 1) * (" + beam.WinConnect.ShearWebTee.BoltOslOnFlange.SpacingLongDir + " - " + (beam.WinConnect.ShearWebTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") + " + Beamlv + " - " + (beam.WinConnect.ShearWebTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH) + " / 2) ");
						Reporting.AddLine("= " + Lnv + ConstUnit.Length);

                        Reporting.AddLine(String.Empty);
						Reporting.AddLine("Gross Length with Shear resistance (Lgv)");
						Reporting.AddLine("= (n - 1) * s + Lv");
						Reporting.AddLine("= (" + beam.WinConnect.ShearWebTee.BoltOslOnFlange.NumberOfBolts + " - 1) * " + beam.WinConnect.ShearWebTee.BoltOslOnFlange.SpacingLongDir + " + " + Beamlv + " = " + Lgv + ConstUnit.Length);
						if (beam.WinConnect.Beam.BottomCope)
						{
							Lnv2 = webd - beam.WinConnect.ShearWebTee.BoltOslOnFlange.NumberOfBolts * (beam.WinConnect.ShearWebTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH);
                            Reporting.AddLine(String.Empty);
                            Reporting.AddLine("Net Length with Shear resistance (no tensile area), Lnv'");
							Reporting.AddLine("= h - n * (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + webd + " - " + beam.WinConnect.ShearWebTee.BoltOslOnFlange.NumberOfBolts + " * " + (beam.WinConnect.ShearWebTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH));
							Reporting.AddLine("= " + Lnv2 + ConstUnit.Length);
						}
						break;
					case EShearOpt.SpliceWithMoment:
						Beamlv = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (beam.WinConnect.ShearWebPlate.FrontY + beam.WinConnect.ShearWebPlate.Length / 2 - beam.WinConnect.ShearWebPlate.Bolt.EdgeDistLongDir);
						if (Beamlv < beam.WinConnect.Beam.DistanceToFirstBolt)
							Beamlv = beam.WinConnect.Beam.DistanceToFirstBolt;
						Lgt = beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH + (beam.WinConnect.ShearWebPlate.Bolt.NumberOfLines - 1) * beam.WinConnect.ShearWebPlate.Bolt.SpacingTransvDir;
						Lnt = Lgt - (beam.WinConnect.ShearWebPlate.Bolt.NumberOfLines - 0.5) * (beam.WinConnect.ShearWebPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
						Lnv = ((beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows - 1) * (beam.WinConnect.ShearWebPlate.Bolt.SpacingLongDir - (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) + Beamlv - (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) / 2);
						Lgv = (beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows - 1) * beam.WinConnect.ShearWebPlate.Bolt.SpacingLongDir + Beamlv;
						Ubs = beam.WinConnect.ShearWebPlate.Bolt.NumberOfLines > 1 ? 0.5 : 1;
						if (beam.WinConnect.ShearWebPlate.Bolt.NumberOfLines == 1)
						{
							Ubs = 1;
                            Reporting.AddLine(String.Empty);
							Reporting.AddLine("Net Length with Tension resistance (Lnt)");
							Reporting.AddLine("= lh - " + ConstNum.QUARTER_INCH + " - (dh + " + ConstNum.SIXTEENTH_INCH + ") / 2 = " + (beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH) + " - " + (beam.WinConnect.ShearWebPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) + " / 2 = " + Lnt + ConstUnit.Length);
							Reporting.AddLine("Gross Length with Tension resistance, Lgt = lh - " + ConstNum.QUARTER_INCH + " = " + Lgt + ConstUnit.Length);
						}
						else
						{
                            Reporting.AddLine(String.Empty);
							Reporting.AddLine("Gross Length with Tension resistance (Lgt) = lh - " + ConstNum.QUARTER_INCH + " + (Nh - 1) * sh");
							Reporting.AddLine("= " + (beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH) + " + (" + beam.WinConnect.ShearWebPlate.Bolt.NumberOfLines + " - 1) * " + beam.WinConnect.ShearWebPlate.Bolt.SpacingTransvDir + " = " + Lgt + ConstUnit.Length);
                            Reporting.AddLine(String.Empty);
                            Reporting.AddLine("Net Length with Tension resistance (Lnt)");
							Reporting.AddLine("= Lgt - (Nh - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgt + " - (" + beam.WinConnect.ShearWebPlate.Bolt.NumberOfLines + "-0.5) * (" + (beam.WinConnect.ShearWebPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) + ") = " + Lnt + ConstUnit.Length);
						}
                        Reporting.AddLine(String.Empty);
						Reporting.AddLine("Net Length with Shear resistance (Lnv)");
						Reporting.AddLine("= ((n - 1) * (s - (dv + " + ConstNum.SIXTEENTH_INCH + ")) + Lv - (dv + " + ConstNum.SIXTEENTH_INCH + ") / 2) ");
						Reporting.AddLine("= ((" + beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows + " - 1) * (" + beam.WinConnect.ShearWebPlate.Bolt.SpacingLongDir + " - " + (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") + " + Beamlv + " - " + (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + " / 2) ");
						Reporting.AddLine("= " + Lnv + ConstUnit.Length);

                        Reporting.AddLine(String.Empty);
						Reporting.AddLine("Gross Length with Shear resistance (Lgv)");
						Reporting.AddLine("= (n - 1) * s + Lv");
						Reporting.AddLine("= (" + beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows + " - 1) * " + beam.WinConnect.ShearWebPlate.Bolt.SpacingLongDir + " + " + Beamlv + " = " + Lgv + ConstUnit.Length);

						if (beam.WinConnect.Beam.BottomCope)
						{
							Lnv2 = webd - beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows * (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
                            Reporting.AddLine(String.Empty);
                            Reporting.AddLine("Net Length with Shear resistance (no tensile area), Lnv'");
							Reporting.AddLine("= h - n * (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + webd + " - " + beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows + " * " + (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH));
							Reporting.AddLine("= " + Lnv2 + ConstUnit.Length);
						}
						break;
					case EShearOpt.SpliceWithoutMoment:
						Beamlv = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (beam.WinConnect.ShearWebPlate.FrontY + beam.WinConnect.ShearWebPlate.Length / 2 - beam.WinConnect.ShearWebPlate.Bolt.EdgeDistLongDir);
						if (Beamlv < beam.WinConnect.Beam.DistanceToFirstBolt)
							Beamlv = beam.WinConnect.Beam.DistanceToFirstBolt;
						Lnt = beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH - (beam.WinConnect.ShearWebPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) / 2;
						Lnv = ((beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows - 1) * (beam.WinConnect.ShearWebPlate.Bolt.SpacingLongDir - (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) + Beamlv - (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) / 2);
						Lgt = beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH;
						Lgv = (beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows - 1) * beam.WinConnect.ShearWebPlate.Bolt.SpacingLongDir + Beamlv;

                        Reporting.AddLine(String.Empty);
						Reporting.AddLine("Net Length with Tension resistance (Lnt)");
						Reporting.AddLine("= lh-" + ConstNum.QUARTER_INCH + " - (dh + " + ConstNum.SIXTEENTH_INCH + ") / 2 = " + (beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH) + " - " + (beam.WinConnect.ShearWebPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) + " / 2 = " + Lnt + ConstUnit.Length);

                        Reporting.AddLine(String.Empty);
                        Reporting.AddLine("Gross Length with Tension resistance (Lgt) = lh - " + ConstNum.QUARTER_INCH + " = " + Lgt + ConstUnit.Length);

                        Reporting.AddLine(String.Empty);
						Reporting.AddLine("Net Length with Shear resistance (Lnv)");
						Reporting.AddLine("= ((n - 1) * (s - (dv + " + ConstNum.SIXTEENTH_INCH + ")) + Lv - (dv + " + ConstNum.SIXTEENTH_INCH + ") / 2)");
						Reporting.AddLine("= ((" + beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows + " - 1) * (" + beam.WinConnect.ShearWebPlate.Bolt.SpacingLongDir + " - " + (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") + " + Beamlv + " - " + (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + " / 2) ");
						Reporting.AddLine("= " + Lnv + ConstUnit.Length);

                        Reporting.AddLine(String.Empty);
						Reporting.AddLine("Gross Length with Shear resistance (Lgv)");
						Reporting.AddLine("= (n - 1) * s + Lv");
						Reporting.AddLine("= (" + beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows + " - 1) * " + beam.WinConnect.ShearWebPlate.Bolt.SpacingLongDir + " + " + Beamlv + " = " + Lgv + ConstUnit.Length);
						if (beam.WinConnect.Beam.BottomCope)
						{
							Lnv2 = webd - beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows * (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
                            Reporting.AddLine(String.Empty);
                            Reporting.AddLine("Net Length with Shear resistance (no tensile area) (Lnv)'");
							Reporting.AddLine("= h - n * (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + webd + " - " + beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows + " * " + (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH));
							Reporting.AddLine("= " + Lnv2 + ConstUnit.Length);
						}
						break;
				}
				PhiRn = BlockShearNew(beam.WinConnect.Beam.Material.Fu, Lnv, 1, Lnt, Lgv, beam.WinConnect.Beam.Material.Fy, beam.Shape.tw, 0, true);
				Reporting.AddLine("= " + PhiRn + ConstUnit.Force);
			}
			// This code should not run when we don't have a Top Cope (MT 8/27/2015)
			//else
			//{
			//	switch (shearopt)
			//	{
			//		case EShearOpt.ClipAngle:
			//			PhiRn = (webd - beam.WinConnect.ShearClipAngle.BoltWebOnBeam.NumberOfRows * (beam.WinConnect.ShearClipAngle.BoltWebOnBeam.HoleWidth + ConstNum.SIXTEENTH_INCH)) * beam.Shape.tw * ConstNum.FIOMEGA0_75N * 0.6 * beam.WinConnect.Beam.Material.Fu;
			//			Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Shear Rupture Strength, " + ConstString.PHI + " Rn");
			//			Reporting.AddLine("= (d - n * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * tw * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu");
			//			Reporting.AddLine("= (" + webd + " - " + beam.WinConnect.ShearClipAngle.BoltWebOnBeam.NumberOfRows + " * " + (beam.WinConnect.ShearClipAngle.BoltWebOnBeam.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + beam.Shape.tw + " * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + beam.WinConnect.Beam.Material.Fu);
			//			Reporting.AddLine("= " + PhiRn + ConstUnit.Force);
			//			break;
			//		case EShearOpt.TeeShear:
			//			PhiRn = (webd - beam.WinConnect.ShearWebTee.BoltWebOnStem.NumberOfBolts * (beam.WinConnect.ShearWebTee.BoltWebOnStem.HoleWidth + ConstNum.SIXTEENTH_INCH)) * beam.Shape.tw * ConstNum.FIOMEGA0_75N * 0.6 * beam.WinConnect.Beam.Material.Fu;
			//			Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Shear Rupture Strength, " + ConstString.PHI + " Rn");
			//			Reporting.AddLine("= (d - n * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * tw * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu");
			//			Reporting.AddLine("= (" + webd + " - " + beam.WinConnect.ShearWebTee.BoltWebOnStem.NumberOfBolts + " * " + (beam.WinConnect.ShearWebTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + beam.Shape.tw + " * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + beam.WinConnect.Beam.Material.Fu);
			//			Reporting.AddLine("= " + PhiRn + ConstUnit.Force);
			//			break;
			//		case EShearOpt.SpliceWithMoment:
			//		case EShearOpt.SpliceWithoutMoment:
			//			PhiRn = (webd - beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows * (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * beam.Shape.tw * ConstNum.FIOMEGA0_75N * 0.6 * beam.WinConnect.Beam.Material.Fu;
			//			Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Shear Rupture Strength, " + ConstString.PHI + " Rn");
			//			Reporting.AddLine("= (d - n * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * tw * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu");
			//			Reporting.AddLine("= (" + webd + " - " + beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows + " * " + (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + beam.Shape.tw + " * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + beam.WinConnect.Beam.Material.Fu);
			//			Reporting.AddLine("= " + PhiRn + ConstUnit.Force);
			//			break;
			//	}
			//}

			h = beam.Shape.d - Math.Max(beam.WinConnect.Beam.TCopeD, beam.Shape.kdet) - Math.Max(beam.WinConnect.Beam.BCopeD, beam.Shape.kdet);
			h = webd;
			Aw = webd * beam.Shape.tw;
			Reporting.AddLine("A = dw * tw = " + webd + " * " + beam.Shape.tw + " = " + Aw + ConstUnit.Area);
			
			switch (shearopt)
			{
				case EShearOpt.ClipAngle:
					boltNumRows = beam.WinConnect.ShearClipAngle.BoltWebOnBeam.NumberOfRows;
					boltWidth = beam.WinConnect.ShearClipAngle.BoltWebOnBeam.HoleWidth;
					break;
				case EShearOpt.WebTee:
					boltNumRows = beam.WinConnect.ShearWebTee.BoltWebOnStem.NumberOfBolts;
					boltWidth = beam.WinConnect.ShearWebTee.BoltWebOnStem.HoleWidth;
					break;
				case EShearOpt.SpliceWithMoment:
				case EShearOpt.SpliceWithoutMoment:
					boltNumRows = beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows;
					boltWidth = beam.WinConnect.ShearWebPlate.Bolt.HoleWidth;
					break;
			}

			a = 500;     //  no stiffeners
			PhiRg = GrossShearStrength(beam.WinConnect.Beam.Material.Fy, Aw, memberType, true);
			Anv = (webd - boltNumRows * (boltWidth + ConstNum.SIXTEENTH_INCH)) * beam.Shape.tw;
			Reporting.AddLine(String.Empty);
			Reporting.AddLine("Anv = (dw - N * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * tw");
			Reporting.AddLine("= (" + webd + " - " + boltNumRows + " * (" + boltWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + beam.Shape.tw);
			Reporting.AddLine("= " + Anv + ConstUnit.Area);

			PhiRn_rupture = NetShearStrength(beam.WinConnect.Beam.Material.Fu, Anv, true);
			BsCap = Math.Min(PhiRn_rupture, Math.Min(PhiRn, PhiRg));

			string capacityString = "Beam " + ConstString.DES_OR_ALLOWABLE + " Shear Strength";
			Reporting.AddHeader(capacityString);
			Reporting.AddLine("= Min(" + ConstString.PHI + " Rn_block_shear, " + ConstString.PHI + " Rn_yielding, " + ConstString.PHI + " Rn_rupture)");
			if (BsCap >= beam.ShearForce)
				Reporting.AddCapacityLine("= " + BsCap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / BsCap, capacityString, memberType);
			else
				Reporting.AddCapacityLine("= " + BsCap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / BsCap, capacityString, memberType);
		}

		internal static void BlockShear(EMemberType memberType, EShearOpt shearopt, ref double BsCap, bool enableReport)
		{
			double PhiRg;
			int a;
			double Aw;
			double h;
			double PhiRn2 = 0;
			double PhiRn = 0;
			double Lnv2 = 0;
			double Lgv = 0;
			double Lgt = 0;
			double Lnv = 0;
			double Lnt = 0;
			double Beamlv = 0;
			double Ubs = 0;
			double webd;

			var beam = CommonDataStatic.DetailDataDict[memberType];

			webd = beam.Shape.d - beam.WinConnect.Beam.TCopeD - beam.WinConnect.Beam.BCopeD;
			if (beam.WinConnect.Beam.TopCope)
			{
				switch (shearopt)
				{
					case EShearOpt.ClipAngle:
						Beamlv = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (beam.WinConnect.ShearClipAngle.FrontY + beam.WinConnect.ShearClipAngle.Length / 2 - beam.WinConnect.ShearClipAngle.BoltOslOnSupport.EdgeDistLongDir);
						if (Beamlv < beam.WinConnect.Beam.DistanceToFirstBolt)
							Beamlv = beam.WinConnect.Beam.DistanceToFirstBolt;
						Lnt = beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH - (beam.WinConnect.ShearClipAngle.BoltOslOnSupport.HoleLength + ConstNum.SIXTEENTH_INCH) / 2;
						Lnv = ((beam.WinConnect.ShearClipAngle.BoltOslOnSupport.NumberOfRows - 1) * (beam.WinConnect.ShearClipAngle.BoltOslOnSupport.SpacingLongDir - (beam.WinConnect.ShearClipAngle.BoltOslOnSupport.HoleWidth + ConstNum.SIXTEENTH_INCH)) + Beamlv - (beam.WinConnect.ShearClipAngle.BoltOslOnSupport.HoleWidth + ConstNum.SIXTEENTH_INCH) / 2);
						Lgt = beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH;
						Lgv = (beam.WinConnect.ShearClipAngle.BoltOslOnSupport.NumberOfRows - 1) * beam.WinConnect.ShearClipAngle.BoltOslOnSupport.SpacingLongDir + Beamlv;
						if (beam.WinConnect.Beam.BottomCope)
							Lnv2 = webd - beam.WinConnect.ShearClipAngle.BoltOslOnSupport.NumberOfRows * (beam.WinConnect.ShearClipAngle.BoltOslOnSupport.HoleWidth + ConstNum.SIXTEENTH_INCH);
						break;
					case EShearOpt.WebTee:
						Beamlv = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (beam.WinConnect.ShearWebTee.FrontY + beam.WinConnect.ShearWebTee.SLength / 2 - beam.WinConnect.ShearWebTee.BoltOslOnFlange.EdgeDistLongDir);
						if (Beamlv < beam.WinConnect.Beam.DistanceToFirstBolt)
							Beamlv = beam.WinConnect.Beam.DistanceToFirstBolt;
						Lnt = beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH - (beam.WinConnect.ShearWebTee.BoltOslOnFlange.HoleLength + ConstNum.SIXTEENTH_INCH) / 2;
						Lnv = ((beam.WinConnect.ShearWebTee.BoltOslOnFlange.NumberOfBolts - 1) * (beam.WinConnect.ShearWebTee.BoltOslOnFlange.SpacingLongDir - (beam.WinConnect.ShearWebTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH)) + Beamlv - (beam.WinConnect.ShearWebTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH) / 2);
						Lgt = beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH;
						Lgv = (beam.WinConnect.ShearWebTee.BoltOslOnFlange.NumberOfBolts - 1) * beam.WinConnect.ShearWebTee.BoltOslOnFlange.SpacingLongDir + Beamlv;
						if (beam.WinConnect.Beam.BottomCope)
							Lnv2 = webd - beam.WinConnect.ShearWebTee.BoltOslOnFlange.NumberOfBolts * (beam.WinConnect.ShearWebTee.BoltOslOnFlange.HoleWidth + ConstNum.SIXTEENTH_INCH);
						break;
					case EShearOpt.SpliceWithMoment:
						Beamlv = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (beam.WinConnect.ShearWebPlate.FrontY + beam.WinConnect.ShearWebPlate.Length / 2 - beam.WinConnect.ShearWebPlate.Bolt.EdgeDistLongDir);
						if (Beamlv < beam.WinConnect.Beam.DistanceToFirstBolt)
							Beamlv = beam.WinConnect.Beam.DistanceToFirstBolt;
						Lgt = beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH + (beam.WinConnect.ShearWebPlate.Bolt.NumberOfLines - 1) * beam.WinConnect.ShearWebPlate.Bolt.SpacingTransvDir;
						Lnt = Lgt - (beam.WinConnect.ShearWebPlate.Bolt.NumberOfLines - 0.5) * (beam.WinConnect.ShearWebPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
						Lnv = ((beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows - 1) * (beam.WinConnect.ShearWebPlate.Bolt.SpacingLongDir - (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) + Beamlv - (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) / 2);
						Lgv = (beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows - 1) * beam.WinConnect.ShearWebPlate.Bolt.SpacingLongDir + Beamlv;
						Ubs = beam.WinConnect.ShearWebPlate.Bolt.NumberOfLines > 1 ? 0.5 : 1;
						if (beam.WinConnect.Beam.BottomCope)
							Lnv2 = webd - beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows * (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
						break;
					case EShearOpt.SpliceWithoutMoment:
						Beamlv = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (beam.WinConnect.ShearWebPlate.FrontY + beam.WinConnect.ShearWebPlate.Length / 2 - beam.WinConnect.ShearWebPlate.Bolt.EdgeDistLongDir);
						if (Beamlv < beam.WinConnect.Beam.DistanceToFirstBolt)
							Beamlv = beam.WinConnect.Beam.DistanceToFirstBolt;
						Lnt = beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH - (beam.WinConnect.ShearWebPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) / 2;
						Lnv = ((beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows - 1) * (beam.WinConnect.ShearWebPlate.Bolt.SpacingLongDir - (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) + Beamlv - (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) / 2);
						Lgt = beam.WinConnect.Beam.Lh - ConstNum.QUARTER_INCH;
						Lgv = (beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows - 1) * beam.WinConnect.ShearWebPlate.Bolt.SpacingLongDir + Beamlv;
						if (beam.WinConnect.Beam.BottomCope)
							Lnv2 = webd - beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows * (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
						break;
				}
				PhiRn = BlockShearNew(beam.WinConnect.Beam.Material.Fu, Lnv, 1, Lnt, Lgv, beam.WinConnect.Beam.Material.Fy, beam.Shape.tw, 0, false);
				if (beam.WinConnect.Beam.BottomCope)
				{
					PhiRn2 = ConstNum.FIOMEGA0_75N * 0.6 * beam.WinConnect.Beam.Material.Fu * Lnv2 * beam.Shape.tw;
					PhiRn = Math.Min(PhiRn, PhiRn2);
				}
			}
			else
			{
				switch (shearopt)
				{
					case EShearOpt.ClipAngle:
                        PhiRn = (webd - beam.WinConnect.ShearClipAngle.BoltWebOnBeam.NumberOfRows * (beam.WinConnect.ShearClipAngle.BoltWebOnBeam.HoleWidth + ConstNum.SIXTEENTH_INCH)) * beam.Shape.tw * ConstNum.FIOMEGA0_75N * 0.6 * beam.WinConnect.Beam.Material.Fu;
                        break;
					case EShearOpt.WebTee:
						PhiRn = (webd - beam.WinConnect.ShearWebTee.BoltWebOnStem.NumberOfBolts * (beam.WinConnect.ShearWebTee.BoltWebOnStem.HoleWidth + ConstNum.SIXTEENTH_INCH)) * beam.Shape.tw * ConstNum.FIOMEGA0_75N * 0.6 * beam.WinConnect.Beam.Material.Fu;
						break;
					case EShearOpt.SpliceWithMoment:
					case EShearOpt.SpliceWithoutMoment:
						PhiRn = (webd - beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows * (beam.WinConnect.ShearWebPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * beam.Shape.tw * ConstNum.FIOMEGA0_75N * 0.6 * beam.WinConnect.Beam.Material.Fu;
						break;
				}
			}
			h = beam.Shape.d - Math.Max(beam.WinConnect.Beam.TCopeD, beam.Shape.kdet) - Math.Max(beam.WinConnect.Beam.BCopeD, beam.Shape.kdet);
			Aw = webd * beam.Shape.tw;
			a = 500;     //  no stiffeners
			PhiRg = GrossShearStrength(beam.WinConnect.Beam.Material.Fy, Aw, memberType, false);
			BsCap = Math.Min(PhiRn, PhiRg);
			if (enableReport)
				BlockShearCapacity(memberType, shearopt, ref Ubs, ref Beamlv, ref Lnt, ref Lnv, ref Lgt, ref Lgv, webd, ref Lnv2, ref PhiRn, ref PhiRn2, ref h, ref Aw, ref a, ref PhiRg, ref BsCap);
		}

		internal static void ClipWelds(EMemberType memberType, ref double WeldCap, bool enableReporting)
		{
			double Rtw;
			double Cap1;
			string AngNum;
			double weldforCap;
			double wmax;
			double minweld;
			double FWeldsize;
			double C1;
			double R;
			double Cnst;
			double theta;
			double a;
			double k;
			double Number;
			double weldSize;
			double th;
			double hl;
			double gap;
			double fexx;
			double wmaxlimit;
			bool userOverwroteWeld = false;

			var beam = CommonDataStatic.DetailDataDict[memberType];

			gap = beam.EndSetback;

			if (beam.ShearConnection == EShearCarriedBy.Tee)
			{
				hl = beam.WinConnect.ShearWebTee.Size.d;
				beam.WinConnect.Fema.L = beam.WinConnect.ShearWebTee.SLength;
				th = beam.WinConnect.ShearWebTee.Size.tw;
				weldSize = beam.WinConnect.ShearWebTee.WeldSizeStem;
				fexx = beam.WinConnect.ShearWebTee.Weld.Fexx;
				Number = 1;
				if (beam.WinConnect.ShearWebTee.WeldSizeStem_User)
					userOverwroteWeld = true;
			}
			else
			{
				hl = beam.WinConnect.ShearClipAngle.OppositeOfOSL;
				beam.WinConnect.Fema.L = beam.WinConnect.ShearClipAngle.Length;
				th = beam.WinConnect.ShearClipAngle.Size.t;
				weldSize = beam.WinConnect.ShearClipAngle.WeldSizeBeam;
				Number = beam.WinConnect.ShearClipAngle.Number;
				fexx = beam.WinConnect.ShearClipAngle.Weld.Fexx;
				if (beam.WinConnect.ShearClipAngle.WeldSizeBeam_User)
					userOverwroteWeld = true;
			}

			k = (hl - gap) / beam.WinConnect.Fema.L;
			if (k < 0)
			{
				WeldCap = 0;
				return;
			}
			a = ((hl - beam.WinConnect.Fema.L * k * k / (1 + k + k)) / beam.WinConnect.Fema.L);
			if (beam.ShearForce == 0)
				theta = Math.PI / 2;
			else
				theta = Math.Atan(Math.Abs(beam.P / beam.ShearForce));
			if (k > 2)
				k = 2;
			if (a > 3)
				a = 3;

			Cnst = EccentricWeld.GetEccentricWeld(k, a, theta, false);
			//EccentricWeld.GetEccentricWeld(k, a, ref Cnst, theta, false);
			R = Math.Sqrt(Math.Pow(beam.P, 2) + Math.Pow(beam.ShearForce, 2)) / 2;
			C1 = CommonCalculations.WeldTypeRatio();
			if (CommonDataStatic.Units == EUnit.Metric)
				C1 *= 1000;
			FWeldsize = Math.Ceiling(R / (Cnst * beam.WinConnect.Fema.L * C1)) / 16;
			minweld = CommonCalculations.MinimumWeld(th, beam.Shape.tw);

			if (!userOverwroteWeld)
				weldSize = Math.Max(minweld, FWeldsize);
			
			if (th < ConstNum.QUARTER_INCH)
				wmax = th;
			else
				wmax = th - ConstNum.SIXTEENTH_INCH;

			if (!userOverwroteWeld)
				weldSize = Math.Min(weldSize, wmax);

			wmaxlimit = beam.Shape.tw * beam.Material.Fu / (0.707 * fexx * Number);

			if (weldSize > wmaxlimit)
				weldforCap = wmaxlimit;
			else
				weldforCap = weldSize;
			if (!beam.WinConnect.ShearWebTee.WeldSizeStem_User && beam.ShearConnection == EShearCarriedBy.Tee)
				beam.WinConnect.ShearWebTee.WeldSizeStem = weldSize;
			else if (!beam.WinConnect.ShearClipAngle.WeldSizeBeam_User)
				beam.WinConnect.ShearClipAngle.WeldSizeBeam = weldSize;

			WeldCap = Number * (Cnst * beam.WinConnect.Fema.L * C1 * 16) * weldforCap;
			if (Number == 2)
				AngNum = " 2 * ";
			else
				AngNum = String.Empty;

			if (enableReporting)
			{
				Reporting.AddLine("k = " + k);
				Reporting.AddLine("a = " + a);
				Reporting.AddLine("L = " + beam.WinConnect.Fema.L + ConstUnit.Length);
				Reporting.AddLine("Theta = " + theta * 180 / Math.PI + " Degrees ");
				Reporting.AddLine(ConstString.PHI + " C = " + Cnst);
				Reporting.AddLine("C1 = " + C1);

				Reporting.AddHeader("Weld Strength (Before Beam Web Check):");
				Reporting.AddLine("= " + AngNum + " C * L * C1 * D");
				Reporting.AddLine("= " + AngNum + Cnst + " * " + beam.WinConnect.Fema.L + " * " + C1 + " * " + (16 * weldSize));
				Cap1 = Number * (Cnst * beam.WinConnect.Fema.L * C1 * 16) * weldSize;
				Reporting.AddLine("= " + Cap1 + ConstUnit.Force);

				Reporting.AddHeader("Reduction Factor for Beam Web Thickness, Rtw:");
				if (Number == 2)
				{
					Reporting.AddLine("= 0.707 * fu * tw / ((D / 16) * Fexx)");
					Reporting.AddLine("= 0.707 * " + beam.Material.Fu + " * " + beam.Shape.tw + " / ((" + (16 * weldSize) + " / 16) * " + fexx + ")");
					Rtw = 0.707 * beam.Material.Fu * beam.Shape.tw / (weldSize * fexx);
				}
				else
				{
					Reporting.AddLine("= 1.414 * fu * tw / ((D / 16) * Fexx)");
					Reporting.AddLine("= 1.414 * " + beam.Material.Fu + " * " + beam.Shape.tw + " / ((" + (16 * weldSize) + " / 16) * " + fexx + ")");
					Rtw = 1.414 * beam.Material.Fu * beam.Shape.tw / (weldSize * fexx);
				}
				if (Rtw > 1)
				{
					WeldCap = Cap1;
					Reporting.AddLine("= " + Rtw + " >= 1 No Reduction");
					Reporting.AddLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + WeldCap + ConstUnit.Force);
				}
				else
				{
					WeldCap = Rtw * Cap1;
					Reporting.AddLine("= " + Rtw);
					Reporting.AddLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + Rtw + " * " + Cap1 + " = " + WeldCap + ConstUnit.Force);
				}
			}
		}

		internal static void DesignBeamToColumnShear(EMemberType memberType)
		{
			double V;
			double t;
			double axial = 0;
			double maxL;
			double minL;

			var beam = CommonDataStatic.DetailDataDict[memberType];

			if (beam.ShearForce == 0)
				return;

			switch (beam.MomentConnection)
			{
				case EMomentCarriedBy.NoMoment:
					axial = beam.P;
					break;
				case EMomentCarriedBy.Angles:
				case EMomentCarriedBy.EndPlate:
				case EMomentCarriedBy.Tee:
					axial = beam.P * beam.Shape.d * beam.Shape.tw / beam.Shape.a;
					break;
				case EMomentCarriedBy.DirectlyWelded:
					axial = 0;
					break;
				case EMomentCarriedBy.FlangePlate:
					axial = 0;
					break;
			}

			switch (beam.ShearConnection)
			{
				case EShearCarriedBy.ClipAngle:
					t = axial / beam.WinConnect.ShearClipAngle.Number;
					V = beam.ShearForce / beam.WinConnect.ShearClipAngle.Number;
					if (beam.WinConnect.ShearClipAngle.BeamSideConnection == EConnectionStyle.Welded)
						maxL = beam.WinConnect.Beam.WebHeight - 2 * beam.WinConnect.ShearClipAngle.WeldSizeBeam;
					else
						maxL = beam.WinConnect.Beam.WebHeight;
					if (beam.P <= 0)
						minL = beam.WinConnect.Beam.WebHeight / 2;
					else
						minL = (beam.Shape.d - 2 * beam.Shape.kdet) / 2;
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
					{
						if (maxL > CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.t)
						{
							maxL = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.t;
							minL = maxL / 2;
						}
					}
					beam.WinConnect.ShearClipAngle.V = V;
					beam.WinConnect.ShearClipAngle.h = t;
					ClipAngle.DesignClipAngles(memberType, ref maxL, ref minL);
					break;
				case EShearCarriedBy.SinglePlate:
					if (beam.MomentConnection != EMomentCarriedBy.NoMoment && beam.P != 0)
					{
						t = 0;
						V = beam.ShearForce;
					}
					else
					{
						t = axial;
						V = beam.ShearForce;
						if (t != 0)
							beam.WinConnect.ShearWebPlate.ExtendedConfiguration = true;
					}
					maxL = beam.WinConnect.Beam.WebHeight;
					if (beam.P <= 0)
						minL = beam.WinConnect.Beam.WebHeight / 2;
					else
						minL = (beam.Shape.d - 2 * beam.Shape.kdet) / 2;
					if (beam.WinConnect.ShearWebPlate.ExtendedConfiguration)
						SinglePlateMoment.DesignSinglePlateMoment(memberType, t, ref V, ref maxL, ref minL);
					else if (beam.MomentConnection == EMomentCarriedBy.NoMoment && CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice &&
						!((CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb) && beam.WinConnect.ShearWebPlate.ExtendedConfiguration))
						SinglePlateShear.DesignSinglePlateShear(memberType, V, maxL);
					else
						SinglePlateMoment.DesignSinglePlateMoment(memberType, t, ref V, ref maxL, ref minL);
					break;
				case EShearCarriedBy.EndPlate:
					t = axial;
					V = beam.ShearForce;
					maxL = beam.Shape.d - Math.Max(beam.WinConnect.Beam.TCopeD, beam.Shape.tf) - Math.Max(beam.WinConnect.Beam.BCopeD, beam.Shape.tf);     // Beam.WebHeight
					if (beam.P <= 0)
						minL = (beam.Shape.d - beam.WinConnect.Beam.TCopeD - beam.WinConnect.Beam.BCopeD) / 2;
					else
						minL = (beam.Shape.d - 2 * beam.Shape.kdet) / 2;
					EndPlateShear.DesignEndPlateShear(memberType, t, V);
					break;
				case EShearCarriedBy.Seat:
					V = beam.ShearForce;
					Seat.DesignSeat(memberType);
					break;
				case EShearCarriedBy.Tee:
					t = axial;
					V = beam.ShearForce;
					if (beam.WinConnect.ShearWebTee.BeamSideConnection == EConnectionStyle.Welded)
						maxL = beam.WinConnect.Beam.WebHeight - 2 * beam.WinConnect.ShearWebTee.WeldSizeStem;
					else
						maxL = beam.WinConnect.Beam.WebHeight;
					if (beam.P <= 0)
						minL = beam.WinConnect.Beam.WebHeight / 2;
					else
						minL = (beam.Shape.d - 2 * beam.Shape.kdet) / 2;
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
					{
						if (maxL > CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.t)
						{
							maxL = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.t;
							minL = maxL / 2;
						}
					}
					beam.WinConnect.ShearWebTee.V = V;
					beam.WinConnect.ShearWebTee.H = t;
					TeeShear.DesignTeeShear(memberType, ref maxL, ref minL);
					break;
			}
		}

		internal static void DesignBeamToColumnMoment(EMemberType memberType)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];

			if (beam.Moment == 0)
				return;

			switch (beam.MomentConnection)
			{
				case EMomentCarriedBy.Angles:
					FlangeAngles.DesignFlangeAngles(memberType);
					beam.WinConnect.Beam.BotAttachThick = beam.WinConnect.MomentFlangeAngle.Angle.t;
					beam.WinConnect.Beam.TopAttachThick = beam.WinConnect.MomentFlangeAngle.Angle.t;
					break;
				case EMomentCarriedBy.EndPlate:
					EndPlateMoment.DesignEndPlateMoment(memberType);
					beam.WinConnect.Beam.BotAttachThick = 0;
					beam.WinConnect.Beam.TopAttachThick = 0;
					break;
				case EMomentCarriedBy.Tee:
					TeeMoment.DesignTeeMoment(memberType);
					beam.WinConnect.Beam.BotAttachThick = beam.WinConnect.MomentTee.TopTeeShape.tw;
					beam.WinConnect.Beam.TopAttachThick = beam.WinConnect.MomentTee.TopTeeShape.tw;
					break;
				case EMomentCarriedBy.DirectlyWelded:
					DirectlyWelded.DesignDirectlyWelded(memberType);
					beam.WinConnect.Beam.BotAttachThick = 0;
					beam.WinConnect.Beam.TopAttachThick = 0;
					break;
				case EMomentCarriedBy.FlangePlate:
					switch (CommonDataStatic.BeamToColumnType)
					{
						case EJointConfiguration.BeamToGirder:
						case EJointConfiguration.BeamSplice:
							FlangeSplicePlate.DesignFlangeSplicePlate(memberType);
							beam.WinConnect.Beam.BotAttachThick = CommonDataStatic.ColumnSplice.FillerThicknessFlangeLower;
							beam.WinConnect.Beam.TopAttachThick = CommonDataStatic.ColumnSplice.FillerThicknessFlangeUpper;
							break;
						case EJointConfiguration.BeamToColumnWeb:
							FlangePlatesToWeb.DesignFlangePlatesToWeb(memberType);
							beam.WinConnect.Beam.BotAttachThick = beam.WinConnect.MomentDirectWeld.Bottom.FlangeThickness;
							beam.WinConnect.Beam.TopAttachThick = beam.WinConnect.MomentDirectWeld.Top.FlangeThickness;
							break;
						default:
							FlangePlates.DesignFlangePlates(memberType);
							beam.WinConnect.Beam.BotAttachThick = beam.WinConnect.MomentFlangePlate.BottomThickness;
							beam.WinConnect.Beam.TopAttachThick = beam.WinConnect.MomentFlangePlate.TopThickness;
							break;
					}
					break;
			}
		}

		internal static double YieldLineFactor(EMemberType memberType, bool enableReport)
		{
			double e;
			double yieldlinefactor;
			double d;
			double c;
			double a;
			double t;
			double f;
			double g;
			double h;
			double M;
			double n;
			double B;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			beam.WinConnect.Fema.L = CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearSeat.StiffenerLength;
			if (column.ShapeType == EShapeType.HollowSteelSection)
			{
				if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToColumnWeb)
					B = column.Shape.bf;
				else
					B = column.Shape.d;

				n = (2 * beam.WinConnect.Fema.L + 2.65 * B);
				M = (B * (B - 0.4 * beam.WinConnect.Fema.L) / (4 * beam.WinConnect.Fema.L));
				h = Math.Sqrt((B - 0.4 * beam.WinConnect.Fema.L) * (7 * B + 0.4 * beam.WinConnect.Fema.L));
				g = 1 + 0.661 * B / beam.WinConnect.Fema.L;
				f = 1 / (B - 0.2 * beam.WinConnect.Fema.L);
				yieldlinefactor = (f * (g * h + M + n));

				if (enableReport)
				{
					Reporting.AddHeader("Yield Line Factor (k)");
					Reporting.AddLine("B = " + B + ConstUnit.Length);

					Reporting.AddLine("n = 2 * L + 2.65 * B");
					Reporting.AddLine("= 2 * " + beam.WinConnect.Fema.L + " + 2.65 * " + B + " = " + n + ConstUnit.Length);

					Reporting.AddLine("M = B * (B - 0.4 * L) / (4 * L)");
					Reporting.AddLine("= " + B + " * ( " + B + " - 0.4 * " + beam.WinConnect.Fema.L + ") / (4 * " + beam.WinConnect.Fema.L + ")" + " = " + M + ConstUnit.Length);

					Reporting.AddLine("H = ((B - 0.4 * L) * (7 * B + 0.4 * L))^0.5  ");
					Reporting.AddLine("= (( " + B + " - 0.4 * " + beam.WinConnect.Fema.L + ") * (7 * " + B + " + 0.4 * " + beam.WinConnect.Fema.L + "))^0.5  " + " = " + h + ConstUnit.Length);

					Reporting.AddLine("g = 1 + 0.661 * B / L");
					Reporting.AddLine("= 1 + 0.661 * " + B + " / " + beam.WinConnect.Fema.L + " = " + g);

					Reporting.AddLine("f = 1 / (B - 0.2 * L)");
					Reporting.AddLine("f = 1 / ( " + B + " - 0.2 * " + beam.WinConnect.Fema.L + ") = " + f + ConstUnit.Length + "^-1");

					Reporting.AddLine("k = f * (g * H + M + n)");
					Reporting.AddLine("= " + f + " * ( " + g + " * " + h + " + " + M + " + " + n + ") = " + yieldlinefactor);
				}
			}
			else
			{
				t = column.Shape.t;
				B = CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearSeat.PlateLength;
				a = 2 / (2 * t - B);
				c = 2 + 0.866 * t / beam.WinConnect.Fema.L;
				if (t < B)
				{
					d = 0;
					e = 0;
				}
				else
				{
					d = Math.Sqrt((t - B) * (3 * t + B));
					e = t * (t - B) / (2 * beam.WinConnect.Fema.L);
				}
				g = 4 * beam.WinConnect.Fema.L + 3.464 * t;
				yieldlinefactor = (a * (c * d + e + g));
			}

			return yieldlinefactor;
		}

		internal static double MinimumEdgeDist(double boltSize, double P, double FuT, double ErolledOrSheared, double HoleLength, EBoltHoleType holeType)
		{
			double eincCorrected = 0;

			if (HoleLength > boltSize + ConstNum.SIXTEENTH_INCH)
			{
				switch (holeType)
				{
					case EBoltHoleType.SSLP:
						if (boltSize <= ConstNum.SEVEN_EIGHTS_INCH)
							eincCorrected = ConstNum.EIGHTH_INCH;
						else if (boltSize < ConstNum.NINE_EIGHTS)
							eincCorrected = ConstNum.EIGHTH_INCH;
						else
							eincCorrected = ConstNum.THREE_SIXTEENTHS;
						break;
					case EBoltHoleType.LSLP:
						eincCorrected = 0.75 * boltSize;
						break;
					case EBoltHoleType.OVS:
						if (boltSize <= ConstNum.SEVEN_EIGHTS_INCH)
							eincCorrected = ConstNum.SIXTEENTH_INCH;
						else
							eincCorrected = ConstNum.EIGHTH_INCH;
						break;
					case EBoltHoleType.SSLN:
					case EBoltHoleType.LSLN:
						if (boltSize <= ConstNum.SEVEN_EIGHTS_INCH)
							eincCorrected = ConstNum.EIGHTH_INCH;
						else if (boltSize <= 1)
							eincCorrected = ConstNum.EIGHTH_INCH;
						else
							eincCorrected = ConstNum.THREE_SIXTEENTHS;
						break;
				}
			}

			return ErolledOrSheared + eincCorrected;
		}

		internal static double ShearedEdgeforAngle(double d)
		{
			double shearedEdgeforAngle = 0;

			switch (CommonDataStatic.Units)
			{
				case EUnit.US:
					if (d <= 0.5)
						shearedEdgeforAngle = d + 0.375;
					else if (d <= 0.75)
						shearedEdgeforAngle = d + 0.5;
					else if (d <= 0.875)
						shearedEdgeforAngle = d + 0.375;
					else if (d <= 1)
						shearedEdgeforAngle = d + 0.25;
					else if (d <= 1.125)
						shearedEdgeforAngle = d + 0.875;
					else if (d <= 1.25)
						shearedEdgeforAngle = (d + 1);
					else
						shearedEdgeforAngle = 1.75 * d;
					break;
				case EUnit.Metric:
					if (d <= 16)
						shearedEdgeforAngle = 28;
					else if (d <= 20)
						shearedEdgeforAngle = 34;
					else if (d <= 22)
						shearedEdgeforAngle = 32;
					else if (d <= 24)
						shearedEdgeforAngle = 32;
					else if (d <= 27)
						shearedEdgeforAngle = 48;
					else if (d <= 30)
						shearedEdgeforAngle = 52;
					else if (d <= 36)
						shearedEdgeforAngle = 64;
					else
						shearedEdgeforAngle = 1.75 * d;
					break;
			}

			return shearedEdgeforAngle;
		}

		internal static void EccentricBolt(double L, double BB, double g, double Nvl, double n, ref double c)
		{
			int sum = 0;
			double SRXDS = 0;
			double SRXD = 0;
			double Pu = 0;
			double RI2 = 0;
			double RI1 = 0;
			double f2 = 0;
			double RI0 = 0;
			double FRI = 0;
			double F1 = 0;
			double RI = 0;
			double ro = 0;
			double DeltaMax = 0;
			double Rmax = 0;
			double IYC = 0;
			double Ixc = 0;
			double h = 0;
			double V = 0;
			double Li = 0;
			double LiMax = 0;
			double Horiz = 0;
			double Vert = 0;

			if (L == 0)
			{
				c = n * Nvl;
				return;
			}
			if (L == 1000)
			{
				Vert = (n - 1) * BB / 2;
				Horiz = g * (Nvl - 1) / 2;
				LiMax = Math.Pow(Math.Pow(Vert, 2) + Math.Pow(Horiz, 2), 0.5);
				c = 0;
				for (int i = 1; i <= Nvl; i++)
				{
					for (int j = 1; j <= n; j++)
					{
						Li = Math.Pow(Math.Pow((j - 1) * BB - Vert, 2) + Math.Pow((i - 1) * g - Horiz, 2), 0.5);
						c += Li * Math.Pow(1 - Math.Exp(-(10 * Li * 0.34 / LiMax)), 0.55);
					}
				}
				return;
			}
			V = (n - 1) * BB;
			h = (Nvl - 1) * g;
			sum = 0;
			for (int i = 1; i <= n; i++)
				sum += (i - 1) * (i - 1);
			Ixc = (Nvl * BB * BB * sum - n * Nvl * V * V / 4);

			sum = 0;
			for (int i = 1; i <= Nvl; i++)
				sum += (i - 1) * (i - 1);
			IYC = (n * g * g * sum - n * Nvl * h * h / 4);

			Rmax = 1;
			DeltaMax = 0.34;
			ro = (Ixc + IYC) / (L * n * Nvl);
			RI = 0;
			EccentricBolt_9052(V, RI, h, DeltaMax, ref SRXD, ref SRXDS, Nvl, g, n, BB, Rmax, L, ref FRI);
			F1 = FRI;
			RI0 = RI;
			RI = ro;
			EccentricBolt_9052(V, RI, h, DeltaMax, ref SRXD, ref SRXDS, Nvl, g, n, BB, Rmax, L, ref FRI);
			f2 = FRI;
			RI1 = RI;

			RI2 = RI1 - (RI1 - RI0) * f2 / (f2 - F1);
			while (Math.Abs(RI2 - RI1) >= 0.0001)
			{
				RI0 = RI1;
				RI1 = RI2;
				F1 = f2;
				RI = RI2;
				EccentricBolt_9052(V, RI, h, DeltaMax, ref SRXD, ref SRXDS, Nvl, g, n, BB, Rmax, L, ref FRI);
				f2 = FRI;
				RI2 = RI1 - (RI1 - RI0) * f2 / (f2 - F1);
			}

			ro = RI2;
			Pu = 2 * SRXD + SRXDS;
			c = Pu / Rmax;
		}

		private static void EccentricBolt_9052(double V, double RI, double h, double DeltaMax, ref double SRXD, ref double SRXDS, double Nvl, double g, double n, double BB, double Rmax, double L, ref double FRI)
		{
			double dmax = 0;
			double dd = 0;
			double SRD = 0;
			double SRDS = 0;
			double xx = 0;
			double yy = 0;
			double d = 0;
			double Delta = 0;
			double rr = 0;

			dmax = Math.Sqrt(V * V / 4 + (RI + h / 2) * (RI + h / 2));
			dd = -10 * DeltaMax / dmax;
			SRD = 0;
			SRXD = 0;
			SRDS = 0;
			SRXDS = 0;

			for (int i = 1; i <= Nvl; i++)
			{
				xx = RI - h / 2 + (i - 1) * g;
				for (int j = 0; j <= ((int)(n / 2 - 0.1)); j++)
				{
					yy = V / 2 - j * BB;
					d = Math.Sqrt(xx * xx + yy * yy);
					Delta = dd * d;
					rr = Rmax * Math.Pow(1 - Math.Pow(2.718, Delta), 0.55);
					if (d - Math.Abs(xx) > 0.0001)
					{
						SRD = SRD + rr * d;
						SRXD = SRXD + rr * xx / d;
					}
					else
					{
						SRDS = SRDS + rr * d;
						SRXDS = SRXDS + rr * Math.Sign(xx);
					}
				}
			}

			FRI = (L + RI) * (2 * SRXD + SRXDS) - (2 * SRD + SRDS);
		}

		internal static void BeamCheck(EMemberType memberType)
		{
			double Mn = 0;
			double Yt = 0;
			double Afn = 0;
			double Afg = 0;
			double Nt = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];

			Reporting.AddGoToHeader("Check Beam:");
			Reporting.AddHeader("Beam Flange Effective Area:");
			switch (beam.MomentConnection)
			{
				case EMomentCarriedBy.FlangePlate:
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
						Nt = beam.WinConnect.MomentDirectWeld.Bolt.NumberOfLines;
					else
						Nt = beam.WinConnect.MomentFlangePlate.Bolt.NumberOfLines;
					break;
				case EMomentCarriedBy.Angles:
					Nt = beam.WinConnect.MomentFlangeAngle.ColumnBolt.NumberOfBolts;
					break;
				case EMomentCarriedBy.Tee:
					Nt = beam.WinConnect.MomentTee.BoltBeamStem.NumberOfLines;
					break;
			}

			Afg = beam.Shape.tf * beam.Shape.bf;
			Reporting.AddLine("Afg = tf * bf = " + beam.Shape.tf + " * " + beam.Shape.bf + " = " + Afg + ConstUnit.Area);
			Afn = beam.Shape.tf * (beam.Shape.bf - Nt * (beam.WinConnect.MomentFlangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH));
			Reporting.AddLine("Afn = tf * (bf - Nt * (dh + " + ConstNum.SIXTEENTH_INCH + ")) = " + beam.Shape.tf + " * (" + beam.Shape.bf + " - (" + Nt + " * (" + beam.WinConnect.MomentFlangePlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + "))) = " + Afn + ConstUnit.Area);
			if (beam.Material.Fy / beam.Material.Fu <= 0.8)
			{
				Reporting.AddLine("Fy / Fu <= 0.8 ---- Yt = 1");
				Yt = 1;
			}
			else
			{
				Reporting.AddLine("Fy/Fu >> 0.8 ---- Yt = 1.1");
				Yt = 1.1;
			}
			Reporting.AddLine("Fu * Afn = " + beam.Material.Fu + " * " + Afn + " = " + (beam.Material.Fu * Afn) + ConstUnit.Force);
			Reporting.AddLine("Yt * Fy * Afg = " + Yt + " * " + beam.Material.Fy + " * " + Afg + " = " + (beam.Material.Fy * Afg) + ConstUnit.Force);

			if (beam.Material.Fu * Afn < Yt * beam.Material.Fy * Afg)
			{
				Mn = beam.Material.Fu * Afn * beam.Shape.sx / Afg;
				Reporting.AddLine("Mn = Fu * Afn * Sx / Afg = " + beam.Material.Fu + " * " + Afn + " * " + beam.Shape.sx + " / " + Afg);
				Reporting.AddLine("= " + Mn + ConstUnit.ForcePerUnitLength);
			}
			else
			{
				Mn = beam.Material.Fy * beam.Shape.zx;
				Reporting.AddLine("Mn = Fy * Zx = " + beam.Material.Fy + " * " + beam.Shape.zx + " = " + Mn + ConstUnit.Moment);
			}

			if (CommonDataStatic.Units == EUnit.US)
			{
				if (ConstNum.FIOMEGA0_9N * Mn / ConstNum.COEFFICIENT_ONE_THOUSAND >= beam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND)
					Reporting.AddCapacityLine(ConstString.PHI + " Mn = " + ConstString.FIOMEGA0_9 + " * Mn = " + (ConstNum.FIOMEGA0_9N * Mn / ConstNum.COEFFICIENT_ONE_THOUSAND) + " >= " + (beam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (OK)", beam.Moment / (ConstNum.FIOMEGA0_9N * Mn), "Beam Flange Effective Area", beam.MemberType);
				else
					Reporting.AddCapacityLine(ConstString.PHI + " Mn = " + ConstString.FIOMEGA0_9 + " * Mn = " + (ConstNum.FIOMEGA0_9N * Mn / ConstNum.COEFFICIENT_ONE_THOUSAND) + " << " + (beam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (NG)", beam.Moment / (ConstNum.FIOMEGA0_9N * Mn), "Beam Flange Effective Area", beam.MemberType);
			}
			else
			{
				if (ConstNum.FIOMEGA0_9N * Mn >= beam.Moment)
					Reporting.AddCapacityLine(ConstString.PHI + " Mn = " + ConstString.FIOMEGA0_9 + " * Mn = " + (ConstNum.FIOMEGA0_9N * Mn / ConstNum.COEFFICIENT_ONE_THOUSAND) + " >= " + (beam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (OK)", beam.Moment / (ConstNum.FIOMEGA0_9N * Mn), "Beam Flange Effective Area", beam.MemberType);
				else
					Reporting.AddCapacityLine(ConstString.PHI + " Mn = " + ConstString.FIOMEGA0_9 + " * Mn = " + (ConstNum.FIOMEGA0_9N * Mn / ConstNum.COEFFICIENT_ONE_THOUSAND) + " << " + (beam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (NG)", beam.Moment / (ConstNum.FIOMEGA0_9N * Mn), "Beam Flange Effective Area", beam.MemberType);
			}
			if (beam.P != 0)
			{
				Reporting.AddLine("The effect of the beam axial force is not included in the Beam Bending Strength check.");
				Reporting.AddLine("The user must check this condition outside the program.");
			}
		}

		internal static double MinimumSpacing(double d, double P, double FuT, double HoleLength, EBoltHoleType ht)
		{
			double minimumSpacing = 0;

			if (ht == EBoltHoleType.LSLN)
				minimumSpacing = P / (ConstNum.FIOMEGA0_75N * FuT) + HoleLength;
			else
				minimumSpacing = P / (ConstNum.FIOMEGA0_75N * 1.2 * FuT) + HoleLength;
			if (CommonDataStatic.IsFema)
				minimumSpacing = 3 * d;
			else
			{
				if (minimumSpacing > 3 * d)
					minimumSpacing = 3 * d;
				if (minimumSpacing < ConstNum.EIGHT_THIRDS * d)
					minimumSpacing = ConstNum.EIGHT_THIRDS * d;
			}
			if (CommonDataStatic.Units == EUnit.Metric)
				minimumSpacing = (int)Math.Ceiling(minimumSpacing);

			return minimumSpacing;
		}

		internal static double HangerStrength(EMemberType memberType, double HoleSize, double Fu, double t, double aa, double B, double FiRn, double rutApplied, bool enableReporting)
		{
			double HangerStrength;

			double rutAverage = 0;
			double rutTemp = 0;
			double alfa;
			double t_required;
			double AlfaPr;
			double Beta;
			double AlfaP;
			double Delta;
			double tc;
			double ro;
			double ap;
			double Bp;
			double P;
			double a;
			double rut;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			Bolt bolt;

			if (aa > 1.25 * B)
				a = 1.25 * B;
			else
				a = aa;

			if (beam.P != 0)
			{
				switch (beam.MomentConnection)
				{
					case EMomentCarriedBy.EndPlate:
						P = beam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir;
						bolt = beam.WinConnect.MomentEndPlate.Bolt;
						break;
					case EMomentCarriedBy.Angles:
						P = (beam.WinConnect.MomentFlangeAngle.ColumnBoltSpacingOut + beam.GageOnColumn) / 2;
						bolt = beam.WinConnect.MomentFlangeAngle.BeamBolt;
						break;
					default:
						P = beam.BoltBrace.SpacingLongDir;
						bolt = beam.BoltBrace;
						break;
				}

				Bp = (B - bolt.BoltSize / 2);
				ap = (a + bolt.BoltSize / 2);
				ro = Bp / ap;
				tc = Math.Sqrt(4 / ConstNum.FIOMEGA0_9N * FiRn * Bp / (P * Fu));
				Delta = 1 - HoleSize / P;
				AlfaP = (Math.Pow(tc / t, 2) - 1) / (Delta * (1 + ro));

				if (AlfaP > 1)
					rut = FiRn * Math.Pow(t / tc, 2) * (1 + Delta);
				else if (AlfaP < 0)
					rut = FiRn;
				else
					rut = FiRn * Math.Pow(t / tc, 2) * (1 + Delta * AlfaP);

				if (bolt.NumberOfBolts > 2 && enableReporting)
				{
					if (beam.MomentConnection == EMomentCarriedBy.EndPlate)
						Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Tension Strength per Tributary Area for Each Bolt:");
					else
						Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Tension Strength per Tributary Area for Each Interior Bolt:");
					Reporting.AddLine("a = " + a + ConstUnit.Length + "    b = " + B + ConstUnit.Length);
					Reporting.AddLine("dh = " + HoleSize + ConstUnit.Length + "    b' = " + Bp + ConstUnit.Length);
					Reporting.AddLine("a' = " + ap + ConstUnit.Length + "     p = " + P + ConstUnit.Length);

					Reporting.AddLine("tc = (4 / " + ConstString.FIOMEGA0_9 + " * " + ConstString.PHI + " Rn * b' / (p * Fu))^0.5");
					Reporting.AddLine("= (4 / " + ConstString.FIOMEGA0_9 + " * " + FiRn + " * " + Bp + "/(" + P + " * " + Fu + "))^0.5 ");
					Reporting.AddLine("= " + tc + ConstUnit.Length);

					Reporting.AddLine("delta = 1 - dh / p ");
					Reporting.AddLine("= 1 - " + HoleSize + " / " + P);
					Reporting.AddLine("= " + Delta);

					Reporting.AddLine("ro = b' / a'");
					Reporting.AddLine("= " + Bp + " / " + ap);
					Reporting.AddLine("= " + ro);

					Reporting.AddLine("Alfa' = ((tc / t) ^2 - 1) / (delta * (1 + ro))");
					Reporting.AddLine("= ((" + tc + " / " + t + ")² - 1) / (" + Delta + " * (1 + " + ro + "))");
					Reporting.AddLine("= " + AlfaP);

					if (AlfaP > 1)
					{
						Reporting.AddLine(ConstString.PHI + "Tn = " + ConstString.PHI + " Rn * (t / tc) ^ 2 * (1 + delta)");
						Reporting.AddLine("= " + FiRn + " * (" + t + " / " + tc + ")² * (1 + " + Delta + ") = " + rut + ConstUnit.Force);
					}
					else if (AlfaP < 0)
						Reporting.AddLine(ConstString.PHI + " Tn = " + ConstString.PHI + " Rn = " + FiRn + ConstUnit.Force);
					else
					{
						Reporting.AddLine(ConstString.PHI + " Tn = " + ConstString.PHI + " Rn * (t / tc) ^ 2 * (1 + delta * Alfa')");
						Reporting.AddLine("= " + FiRn + " * (" + t + " / " + tc + ")² * (1 + " + Delta + " * " + AlfaP + ") = " + rut + ConstUnit.Force);
					}

					if (beam.ShearConnection == EShearCarriedBy.Tee && beam.MomentConnection == EMomentCarriedBy.Tee)
					{
						Beta = (FiRn / rut - 1) / ro;
						if (Beta >= 1)
							AlfaPr = 1;
						else
							AlfaPr = Math.Min(1, Beta / (1 - Beta) / Delta);
						t_required = Math.Sqrt(4 / ConstNum.FIOMEGA0_9N * rut * Bp / (P * Fu * (1 + Delta * AlfaPr)));
						Reporting.AddHeader("Required Thickness:");
						Reporting.AddLine("treq = (4 / " + ConstString.FIOMEGA0_9 + " * rut * bp / (p * Fu * (1 + Delta * AlfaP)))^0.5");
						Reporting.AddLine("= (4 / " + ConstString.FIOMEGA0_9 + " * " + rut + " * " + Bp + " / (" + P + " * " + Fu + " * (1 + " + Delta + " * " + AlfaPr + ")))^0.5");
						Reporting.AddLine("= " + t_required + ConstUnit.Length);
					}
				}
			}

			switch (beam.MomentConnection)
			{
				case EMomentCarriedBy.EndPlate:
					P = beam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir;
					bolt = beam.WinConnect.MomentEndPlate.Bolt;
					break;
				case EMomentCarriedBy.Angles:
					if (beam.WinConnect.MomentFlangeAngle.ColumnBolt.NumberOfBolts > 2)
						P = beam.WinConnect.MomentFlangeAngle.ColumnBoltSpacingOut / 2 + beam.WinConnect.MomentFlangeAngle.ColumnBolt.EdgeDistLongDir;
					else
						P = beam.GageOnColumn / 2 + beam.WinConnect.MomentFlangeAngle.ColumnBolt.EdgeDistLongDir;
					bolt = beam.WinConnect.MomentFlangeAngle.BeamBolt;
					break;
				default:
					P = beam.BoltBrace.EdgeDistLongDir + beam.BoltBrace.SpacingLongDir / 2;
					bolt = beam.BoltBrace;
					break;
			}

			Bp = (Math.Abs(B) - bolt.BoltSize / 2);
			ap = (a + bolt.BoltSize / 2);
			tc = Math.Sqrt(4 / ConstNum.FIOMEGA0_9N * FiRn * Bp / (P * Fu));
			Delta = 1 - HoleSize / P;
			ro = Bp / ap;
			AlfaP = (Math.Pow(tc / t, 2) - 1) / (Delta * (1 + ro));

			if (AlfaP > 1)
				rut = FiRn * Math.Pow(t / tc, 2) * (1 + Delta);
			else if (AlfaP < 0)
				rut = FiRn;
			else
				rut = FiRn * Math.Pow(t / tc, 2) * (1 + Delta * AlfaP);

			if (rutApplied != 0)
				rutTemp = rutApplied;
			else
				rutTemp = rut;

			// prying force
			alfa = Math.Max(0, 1 / Delta * (rutTemp / FiRn * Math.Pow(tc / t, 2) - 1));
			CommonDataStatic.PryingForce = FiRn * Delta * alfa * ro * Math.Pow(t / tc, 2);

			if (enableReporting)
			{
				if (bolt.NumberOfBolts > 2)
					Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Tension Strength per Tributary Area for Each Exterior Bolt:");
				else
				{
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
						Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + "Tension Strength of Plate per Tributary Area for Each Bolt:");
					else
						Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Tension Strength per Tributary Area for Each Bolt:");
				}
				Reporting.AddLine("a = " + a + ConstUnit.Length + ",      b = " + B + ConstUnit.Length);
				Reporting.AddLine("dh = " + HoleSize + ConstUnit.Length + ",     b' = " + Bp + ConstUnit.Length);
				Reporting.AddLine("a' = " + ap + ConstUnit.Length + ",     p = " + P + ConstUnit.Length);

				Reporting.AddLine(String.Empty);
				Reporting.AddLine("tc = (4 / " + ConstString.FIOMEGA0_9 + " * " + ConstString.PHI + " Rn * b' / (p * Fu))^0.5 ");
				Reporting.AddLine("= (4 / " + ConstString.FIOMEGA0_9 + " * " + FiRn + " * " + Bp + " / (" + P + " * " + Fu + "))^0.5 ");
				Reporting.AddLine("= " + tc + ConstUnit.Length);

				Reporting.AddLine(String.Empty);
				Reporting.AddLine("delta = 1 - dh / p ");
				Reporting.AddLine("= 1 - " + HoleSize + " / " + P);
				Reporting.AddLine("= " + Delta);

				Reporting.AddLine(String.Empty);
				Reporting.AddLine("ro = b' / a' ");
				Reporting.AddLine("= " + Bp + " / " + ap);
				Reporting.AddLine("= " + ro);

				Reporting.AddLine(String.Empty);
				Reporting.AddLine("Alfa' = ((tc / t) ^2 - 1) / (delta * (1 + ro))");
				Reporting.AddLine("= ((" + tc + " / " + t + ")² - 1) / (" + Delta + " * (1 + " + ro + "))");
				Reporting.AddLine("= " + AlfaP);

				if (AlfaP > 1)
				{
					Reporting.AddLine(String.Empty);
					Reporting.AddLine(ConstString.PHI + "Tn = " + ConstString.PHI + " Rn * (t / tc) ^ 2 * (1 + delta)");
					Reporting.AddLine("= " + FiRn + " * (" + t + " / " + tc + ")² * (1 + " + Delta + ") ");
					Reporting.AddLine("= " + rut + ConstUnit.Force);
				}
				else if (AlfaP < 0)
				{
					Reporting.AddLine(String.Empty);
					Reporting.AddLine(ConstString.PHI + "Tn = " + ConstString.PHI + " Rn = " + FiRn + ConstUnit.Force);
				}
				else
				{
					Reporting.AddLine(String.Empty);
					Reporting.AddLine(ConstString.PHI + "Tn = " + ConstString.PHI + " Rn * (t / tc) ^ 2 * (1 + delta * Alfa')");
					Reporting.AddLine("= " + FiRn + " * (" + t + " / " + tc + ")² * (1 + " + Delta + " * " + AlfaP + ") ");
					Reporting.AddLine("= " + rut + ConstUnit.Force);
				}

				Reporting.AddHeader("Prying Force:");
				Reporting.AddLine("Alfa = Max[0; (1 / Delta) * (rut / " + ConstString.PHI + " Rn * (tc / t)² - 1)]");
				Reporting.AddLine("= Max(0; (1 / " + Delta + ") * (" + rutTemp + " / " + FiRn + " * (" + tc + " / " + t + ")² - 1))");
				Reporting.AddLine("= " + alfa);
				Reporting.AddLine(string.Empty);

				Reporting.AddLine("qu = " + ConstString.PHI + " Rn * Delta * alfa * ro * (t / tc)²");
				Reporting.AddLine("= " + FiRn + " * " + Delta + " * " + alfa + " * " + ro + " * (" + t + " / " + tc + ")²");
				Reporting.AddLine("= " + CommonDataStatic.PryingForce + ConstUnit.Force + " / bolt");

				if (beam.ShearConnection == EShearCarriedBy.Tee)
				{
					Beta = (FiRn / rut - 1) / ro;
					if (Beta >= 1)
						AlfaPr = 1;
					else
						AlfaPr = Math.Min(1, Beta / (1 - Beta) / Delta);
					t_required = Math.Sqrt(4 / ConstNum.FIOMEGA0_9N * rutTemp * Bp / (P * Fu * (1 + Delta * AlfaPr)));

					if (CommonDataStatic.Units == EUnit.Metric)
						t_required = Math.Round(t_required, 2);

					Reporting.AddHeader("Required Thickness:");
					Reporting.AddLine("treq = (4 / " + ConstString.FIOMEGA0_9 + " * rut * bp / (p * Fu * (1 + Delta * AlfaP)))^0.5");
					Reporting.AddLine("= (4 / " + ConstString.FIOMEGA0_9 + " * " + rutTemp + " * " + Bp + " / (" + P + " * " + Fu + " * (1 + " + Delta + " * " + AlfaPr + ")))^0.5");
					if (t_required <= t)
						Reporting.AddLine("= " + t_required + " <= " + t + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + t_required + " >> " + t + ConstUnit.Length + " (NG)");
				}
				if (bolt.NumberOfBolts > 2 && beam.P != 0)
				{
					Reporting.AddLine("Average " + ConstString.PHI + "Tn:");
					Reporting.AddLine("= (2 * " + ConstString.PHI + "Tn_Ext + (N - 2) * " + ConstString.PHI + "Tn_Int) / N");
					Reporting.AddLine("= (2 * " + rut + " + (" + bolt.NumberOfBolts + " - 2) * " + rut + ") / " + bolt.NumberOfBolts);
					Reporting.AddLine("= " + rutAverage + ConstUnit.Force);
				}
			}

			if (bolt.NumberOfBolts > 2 && beam.P != 0)
			{
				rutAverage = (2 * rut + (bolt.NumberOfBolts - 2) * rut) / bolt.NumberOfBolts;
				HangerStrength = rutAverage;
			}
			else
				HangerStrength = rut;

			return HangerStrength;
		}

		internal static void SeatedBeamNewCalc(EMemberType memberType, double angleLength, double k_angle, double taIn, ref double taOut, ref double Nbearing, ref double FiRn, bool enableReporting)
		{
			double FiRnBb = 0;
			double FiRnBs;
			double FiRnc;
			double FiRnY;
			double ta_shear = 0;
			double ta_bending = 0;
			double ecc = 0;
			double minleg;
			double NreqC2;
			double x3;
			double NreqC1;
			double x2;
			double X1;
			double NbearingPrevious;
			double NreqY;
			double NBearingTest;

			bool skipCapacityCheck = false;

			var beam = CommonDataStatic.DetailDataDict[memberType];

			// Beam Web Yielding
			NBearingTest = beam.ShearForce / (beam.Material.Fy * beam.Shape.tw) - 2.5 * beam.Shape.kdes;
			NreqY = Math.Max(NBearingTest, beam.Shape.kdes);
			// Beam web crippling
			Nbearing = NreqY;

			do
			{
				NbearingPrevious = Nbearing;
				if (Nbearing / beam.Shape.d <= 0.2)
				{
					X1 = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5);
					x2 = X1 * (3 / beam.Shape.d) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					NreqC1 = (beam.ShearForce - X1) / x2;
					Nbearing = Math.Max(NreqC1, NreqY);
				}
				else
				{
					X1 = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5);
					x2 = X1 * (4 / beam.Shape.d) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					x3 = -X1 * 0.2 * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					NreqC2 = (beam.ShearForce - X1 - x3) / x2;
					Nbearing = Math.Max(NreqC2, NreqY);
				}
			} while (NbearingPrevious != Nbearing);
			
			switch (beam.WinConnect.ShearSeat.Connection)
			{
				case ESeatConnection.Bolted:
				case ESeatConnection.Welded:
					minleg = beam.EndSetback + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + beam.WinConnect.ShearSeat.Bolt.EdgeDistLongDir;
					if (!beam.WinConnect.Beam.BoltEdgeDistanceOnFlange_User && minleg > beam.WinConnect.ShearSeat.AngleShort)
						beam.WinConnect.Beam.BoltEdgeDistanceOnFlange = beam.WinConnect.ShearSeat.AngleShort - (beam.EndSetback + beam.WinConnect.ShearSeat.Bolt.EdgeDistLongDir);
					ecc = Nbearing / 2 + beam.EndSetback - k_angle;
					if (ecc <= 0)
						ecc = 0;
					if (angleLength != 0)
					{
						ta_bending = Math.Pow(4 * beam.ShearForce * ecc / (ConstNum.FIOMEGA0_9N * beam.WinConnect.ShearSeat.Material.Fy * angleLength), 0.5);
						ta_shear = beam.ShearForce / (ConstNum.FIOMEGA1_0N * 0.6 * beam.WinConnect.ShearSeat.Material.Fy * angleLength);
					}
					taOut = Math.Min(ta_bending, ta_shear);
					break;
				default:
					skipCapacityCheck = true;
					break;
			}

			if (!skipCapacityCheck)
			{
				FiRnY = (Nbearing + 2.5 * beam.Shape.kdes) * beam.Material.Fy * beam.Shape.tw;

				if (Nbearing / beam.Shape.d <= 0.2)
				{
					X1 = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5);
					x2 = X1 * (3 / beam.Shape.d) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					FiRnc = X1 + x2 * Nbearing;
				}
				else
				{
					X1 = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5);
					x2 = X1 * (4 / beam.Shape.d) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					x3 = -X1 * 0.2 * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					FiRnc = X1 + x3 + x2 * Nbearing;
				}
				FiRnBs = ConstNum.FIOMEGA1_0N * 0.6 * beam.WinConnect.ShearSeat.Material.Fy * angleLength * taIn;
				if (ecc > 0)
				{
					if (beam.EndSetback >= k_angle)
					{
						ecc = Nbearing/2 + beam.EndSetback - k_angle;
						FiRnBb = ConstNum.FIOMEGA0_9N*beam.WinConnect.ShearSeat.Material.Fy*angleLength*Math.Pow(taIn, 2)/(4*ecc);
					}
					else
					{
						ecc = (Nbearing + beam.EndSetback - k_angle) / 2;
						FiRnBb = ConstNum.FIOMEGA0_9N * Nbearing * beam.WinConnect.ShearSeat.Material.Fy * angleLength * Math.Pow(taIn, 2) / (8 * Math.Pow(ecc, 2));	
					}

					FiRn = Math.Min(Math.Min(FiRnY, FiRnc), Math.Min(FiRnBb, FiRnBs));
				}
				else
					FiRn = Math.Min(Math.Min(FiRnY, FiRnc), FiRnBs);
			}

			if (enableReporting)
			{
				if (beam.WinConnect.ShearSeat.Connection == ESeatConnection.Bolted || beam.WinConnect.ShearSeat.Connection == ESeatConnection.Welded)
					Reporting.AddGoToHeader("Check Beam Web and Seat Angle");
				else
					Reporting.AddGoToHeader("Check Beam Web");

				//= max (V/(Fy*tw) – 2.5*kdes; kdes)
				//= max (40/(50*0.395) – 2.5*1.01 ; 1.01)
				//= max (-0.50; 1.01)
				//= 1.01

				Reporting.AddHeader("Minimum Bearing Length Required (N)");
				Reporting.AddLine("= Max(V / (Fy * tw) - 2.5 * kdes; kdes))");
				Reporting.AddLine("= Max(" + beam.ShearForce + " / (" + beam.Material.Fy + " * " + beam.Shape.tw + ") - 2.5 * " + beam.Shape.kdes + "; " + beam.Shape.kdes + "))");
				Reporting.AddLine("= Max(" + NBearingTest + "; " + beam.Shape.kdes + ")");
				Reporting.AddLine("= " + Nbearing + ConstUnit.Length);

				Reporting.AddHeader("Beam web yielding:");
				FiRnY = ConstNum.FIOMEGA1_0N * (Nbearing + 2.5 * beam.Shape.kdes) * beam.Material.Fy * beam.Shape.tw;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA1_0 + " * (N + 2.5 * k) * Fy * tw");
				Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (" + Nbearing + " + 2.5 * " + beam.Shape.kdes + ") * " + beam.Material.Fy + " * " + beam.Shape.tw);
				if (FiRnY >= beam.ShearForce)
					Reporting.AddCapacityLine("= " + FiRnY + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRnY, "Beam web yielding", memberType);
				else
					Reporting.AddCapacityLine("= " + FiRnY + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRnY, "Beam web yielding", memberType);

				Reporting.AddHeader("Beam web crippling:");
				if (Nbearing / beam.Shape.d <= 0.2)
				{
					X1 = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5);
					x2 = X1 * (3 / beam.Shape.d) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					FiRnc = X1 + x2 * Nbearing;

					Reporting.AddLine("N / d  = " + Nbearing + " / " + beam.Shape.d + " = " + (Nbearing / beam.Shape.d) + " <=  0.2 ");
					Reporting.AddLine("X1 = " + ConstString.FIOMEGA0_75 + " * 0.4 * tw² * (E * Fy * tf / tw)^0.5");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + beam.Shape.tw + "² * (" + ConstNum.ELASTICITY + " * " + beam.Material.Fy + " * " + beam.Shape.tf + " / " + beam.Shape.tw + ")^0.5");
					Reporting.AddLine("= " + X1);

					Reporting.AddLine("X2 = X1 * (3 / d) * (tw / tf)^1.5");
					Reporting.AddLine("= " + X1 + " * (3 / " + beam.Shape.d + ") * (" + beam.Shape.tw + " / " + beam.Shape.tf + ")^1.5");
					Reporting.AddLine("= " + x2);

					Reporting.AddLine(ConstString.PHI + " Rn = X1 + X2 * N");
					Reporting.AddLine("= X1 + X2 * N = " + X1 + " + " + x2 + " * " + Nbearing);
				}
				else
				{
					X1 = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5);
					x2 = X1 * (4 / beam.Shape.d) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					x3 = -X1 * 0.2 * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					FiRnc = X1 + x3 + x2 * Nbearing;
					Reporting.AddLine("N / d  = " + Nbearing + " / " + beam.Shape.d + " = " + (Nbearing / beam.Shape.d) + " >> 0.2");
					Reporting.AddLine("X1 = " + ConstString.FIOMEGA0_75 + " * 0.4 * tw² * (E * Fy * tf / tw)^0.5");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + beam.Shape.tw + "² * (" + ConstNum.ELASTICITY + " * " + beam.Material.Fy + " * " + beam.Shape.tf + " / " + beam.Shape.tw + ")^0.5");
					Reporting.AddLine("= " + X1);

					Reporting.AddLine("X2 = X1 * (4 / d) * (tw / tf)^1.5");
					Reporting.AddLine("= " + X1 + " * (4 / " + beam.Shape.d + ") * (" + beam.Shape.tw + " / " + beam.Shape.tf + ")^1.5");
					Reporting.AddLine("= " + x2);
					Reporting.AddLine("X3 = -X1 * 0.2 * (tw / tf)^1.5");
					Reporting.AddLine("= -" + X1 + " * 0.2 * (" + beam.Shape.tw + " / " + beam.Shape.tf + "^1.5");
					Reporting.AddLine("= " + x3);
					Reporting.AddLine(ConstString.PHI + " Rn = X1 + X3 + X2 * N = " + X1 + " + " + x3 + " + " + x2 + " * " + Nbearing);
				}
				if (FiRnc >= beam.ShearForce)
					Reporting.AddCapacityLine("= " + FiRnc + " >=  " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRnc, "Beam web crippling", memberType);
				else
					Reporting.AddCapacityLine("= " + FiRnc + " <<  " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRnc, "Beam web crippling", memberType);

				if (beam.WinConnect.ShearSeat.Connection != ESeatConnection.Bolted && beam.WinConnect.ShearSeat.Connection != ESeatConnection.Welded)
					return;

				Reporting.AddHeader("Seat Angle Bending:");

				if (beam.EndSetback >= k_angle)
				{
					Reporting.AddLine("e = N / 2 + C - ka = " + Nbearing + " / 2 + " + beam.EndSetback + " - " + k_angle + " = " + ecc + ConstUnit.Length);	
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * L * t² / (4 * e)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + beam.WinConnect.ShearSeat.Material.Fy + " * " + angleLength + " * " + taIn + "² / (4 * " + ecc + ")");
				}
				else
				{
					Reporting.AddLine("e = (N + C - ka) / 2 = (" + Nbearing + " + " + beam.EndSetback + " - " + k_angle + ") / 2 = " + ecc + ConstUnit.Length);
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * N * Fy * L * t² / (8 * e²)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + Nbearing + " * " + beam.WinConnect.ShearSeat.Material.Fy + " * " + angleLength + " * " + taIn + "² / (8 * " + ecc + "²)");
				}

				if (FiRnBb >= beam.ShearForce)
					Reporting.AddCapacityLine("= " + FiRnBb + " >=  " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRnBb, "Seat Angle Bending", memberType);
				else
					Reporting.AddCapacityLine("= " + FiRnBb + " <<  " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce, "Seat Angle Bending", memberType);

				Reporting.AddHeader("Seat Angle Shear:");
				Reporting.AddHeader("Yielding:");
				FiRnBs = ConstNum.FIOMEGA1_0N * 0.6 * beam.WinConnect.ShearSeat.Material.Fy * angleLength * taIn;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy * L * t =  " + ConstString.FIOMEGA1_0 + " * 0.6 * " + beam.WinConnect.ShearSeat.Material.Fy + " * " + angleLength + " * " + taIn);
				if (FiRnBs >= beam.ShearForce)
					Reporting.AddCapacityLine("= " + FiRnBs + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRnBs, "Seat Angle Shear Yielding", memberType);
				else
					Reporting.AddCapacityLine("= " + FiRnBs + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRnBs, "Seat Angle Shear Yielding", memberType);

				Reporting.AddHeader("Rupture:");
				FiRnBs = ConstNum.FIOMEGA0_75N * 0.6 * beam.WinConnect.ShearSeat.Material.Fu * angleLength * taIn;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu * L * t = " + ConstString.FIOMEGA0_75 + " * 0.6 * " + beam.WinConnect.ShearSeat.Material.Fu + " * " + angleLength + " * " + taIn);
				if (FiRnBs >= beam.ShearForce)
					Reporting.AddCapacityLine("= " + FiRnBs + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRnBs, "Seat Angle Shear Rupture", memberType);
				else
					Reporting.AddCapacityLine("= " + FiRnBs + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRnBs, "Seat Angle Shear Rupture", memberType);
			}
		}

		internal static double OslWeld(EMemberType memberType, double P, bool enableReporting)
		{
			double OslWeld = 0;
			double Cnst = 0;
			double a = 0;
			double k = 0;
			double FiRn = 0;
			double oslPlus = 0;
			double C1 = 0;
			int Number = 0;
			double SupWeld = 0;
			double osl = 0;
			double fexx;

			var beam = CommonDataStatic.DetailDataDict[memberType];

			double w = 0;
			// p=0  returns Capacity  for one line of vertical weld
			// p>0 returns Weld size (P is load on one angle)
			if (beam.ShearConnection == EShearCarriedBy.Tee)
			{
				beam.WinConnect.Fema.L = beam.WinConnect.ShearWebTee.SLength;
				osl = 0;
				SupWeld = beam.WinConnect.ShearWebTee.WeldSizeFlange;
				fexx = beam.WinConnect.ShearWebTee.Weld.Fexx;
				Number = 2;
			}
			else
			{
				beam.WinConnect.Fema.L = beam.WinConnect.ShearClipAngle.Length;
				osl = beam.WinConnect.ShearClipAngle.LengthOfOSL;
				SupWeld = beam.WinConnect.ShearClipAngle.WeldSizeSupport;
				Number = beam.WinConnect.ShearClipAngle.Number;
				fexx = beam.WinConnect.ShearClipAngle.Weld.Fexx;
			}
			C1 = CommonCalculations.WeldTypeRatio();

			switch (Number)
			{
				case 1:
					k = osl / beam.WinConnect.Fema.L;
					a = k * (2 + k) / (2 * (1 + k));
					Cnst = EccentricWeld.GetEccentricWeld(k, a, 0, enableReporting);
					//EccentricWeld.GetEccentricWeld(k, a, ref Cnst, 0, enableReporting);
					Cnst *= 10;	// I think this makes the value correct (MT 01/29/15)
					C1 = CommonCalculations.WeldTypeRatio();
					if (P > 0)
					{
						w = P / (16 * Cnst * C1 * beam.WinConnect.Fema.L);
						OslWeld = NumberFun.Round(w, 16);
						if (enableReporting)
						{
							Reporting.AddHeader("Required Weld Size (w):");
							Reporting.AddLine("= Pu / (16 * C * C1 * L)");
							Reporting.AddLine("= " + P + " / 16 * " + Cnst + " * " + C1 + " * " + beam.WinConnect.Fema.L + ") = " + w + ConstUnit.Length);
						}
					}
					else
					{
						FiRn = (16 * w * Cnst * C1 * beam.WinConnect.Fema.L);
						OslWeld = FiRn;
						if (enableReporting)
						{
							Reporting.AddLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ", " + ConstString.PHI + " Rn:");
							Reporting.AddLine("= 16 * w * C * c1 * L");
							Reporting.AddLine("= 16 * " + SupWeld + " * " + Cnst + " * " + C1 + " * " + beam.WinConnect.Fema.L + " = " + FiRn + ConstUnit.Force);
						}
					}
					break;
				case 2:
					oslPlus = osl;
					if (MiscMethods.IsFlareWeld(memberType))
					{
						if (P > 0)
						{
							w = P * Math.Sqrt(1 + 12.96 * Math.Pow(oslPlus, 2) / Math.Pow(beam.WinConnect.Fema.L, 2)) / (ConstNum.FIOMEGA0_75N * 0.6 * fexx * beam.WinConnect.Fema.L);
							OslWeld = NumberFun.Round(w, 16);
						}
						else
						{
							FiRn = (ConstNum.FIOMEGA0_75N * 0.6 * fexx * SupWeld * beam.WinConnect.Fema.L / Math.Sqrt(1 + 12.96 * Math.Pow(oslPlus, 2) / Math.Pow(beam.WinConnect.Fema.L, 2)));
							OslWeld = FiRn;
						}
					}
					else
					{
						if (P > 0)
						{
							w = P * Math.Sqrt(1 + 12.96 * Math.Pow(oslPlus, 2) / Math.Pow(beam.WinConnect.Fema.L, 2)) / (ConstNum.FIOMEGA0_75N * 0.4242 * fexx * beam.WinConnect.Fema.L);
							OslWeld = NumberFun.Round(w, 16);
						}
						else
						{
							FiRn = (ConstNum.FIOMEGA0_75N * 0.4242 * fexx * SupWeld * beam.WinConnect.Fema.L / Math.Sqrt(1 + 12.96 * Math.Pow(oslPlus, 2) / Math.Pow(beam.WinConnect.Fema.L, 2)));
							OslWeld = FiRn;
						}
					}

					if (Double.IsNaN(OslWeld))
						OslWeld = 0;
					if (Double.IsNaN(FiRn))
						FiRn = 0;

					if (MiscMethods.IsFlareWeld(memberType) && enableReporting)
					{
						if (P > 0)
						{
							Reporting.AddHeader("Required Weld Size (w):");
							Reporting.AddLine("= Pu * (1 + 12.96 * e² / L²)^0.5 / (" + ConstString.FIOMEGA0_75 + " * 0.6 * Fexx * L)");
							Reporting.AddLine("= " + P + " * (1 + 12.96 * " + oslPlus + "² / " + beam.WinConnect.Fema.L + "²)^0.5 / (2 * " + ConstString.FIOMEGA0_75 + " * 0.6 *" + fexx + " * " + beam.WinConnect.Fema.L + ")");
							Reporting.AddLine("= " + w + ConstUnit.Length);
						}
						else
						{
							Reporting.AddHeader("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " (One Line), " + ConstString.PHI + " Rn:");
							Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.6 * Fexx * w * L / (1 + 12.96 * e² / L²)^0.5");
							Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.6 * " + fexx + " * " + SupWeld + " * " + beam.WinConnect.Fema.L + " / (1 + 12.96 * " + oslPlus + "² / " + beam.WinConnect.Fema.L + "²)^0.5");
							Reporting.AddLine("= " + FiRn + ConstUnit.Force);
						}
					}
					else if(enableReporting)
					{
						if (P > 0)
						{
							Reporting.AddHeader("Required Weld Size (w):");
							Reporting.AddLine("= Pu * (1 + 12.96 * e² / L²)^0.5 / (" + ConstString.FIOMEGA0_75 + " * 0.4242 * Fexx * L)");
							Reporting.AddLine("= " + P + " * (1 + 12.96 * " + oslPlus + "² / " + beam.WinConnect.Fema.L + "²)^0.5 / (2 * " + ConstString.FIOMEGA0_75 + " * 0.4242 *" + fexx + " * " + beam.WinConnect.Fema.L + ")");
							Reporting.AddLine("= " + w + ConstUnit.Length);
						}
						else
						{
							Reporting.AddHeader("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " (One Line), " + ConstString.PHI + " Rn:");
							Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4242 * Fexx * w * L / (1 + 12.96 * e² / L²)^0.5");
							Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4242 * " + fexx + " * " + SupWeld + " * " + beam.WinConnect.Fema.L + " /(1 + 12.96 * " + oslPlus + "² / " + beam.WinConnect.Fema.L + "²)^0.5");
							Reporting.AddLine("= " + FiRn + ConstUnit.Force);
						}
					}
					break;
			}
			return OslWeld;
		}

		internal static double GrossShearStrength(double Fyw, double Aw, EMemberType memberType, bool enableReporting)
		{
			double Vn;
			double Cv;
			string fiOmegaString;
			double fiOmegaValue;
			const double k = 5.0;

			// New logic added in Descon 8 - Shear Strength of I-Shaped Members - Equations G2-2 through G2-4
			var beam = CommonDataStatic.DetailDataDict[memberType];
			if (beam.Shape.h_tw <= 2.24 * Math.Pow(ConstNum.ELASTICITY / Fyw, 0.5))
			{
				Cv = 1;
				fiOmegaString = ConstString.FIOMEGA1_0;
				fiOmegaValue = ConstNum.FIOMEGA1_0N;
			}
			else if (beam.Shape.h_tw <= 1.10 * Math.Pow(k * ConstNum.ELASTICITY / Fyw, 0.5))
			{
				Cv = 1;
				fiOmegaString = ConstString.FIOMEGA0_9;
				fiOmegaValue = ConstNum.FIOMEGA0_9N;
			}
			else if ((1.10 * Math.Pow(k * ConstNum.ELASTICITY / Fyw, 0.5) < beam.Shape.h_tw) &&
			         (beam.Shape.t / beam.Shape.tw <= 1.37 * Math.Pow(k * ConstNum.ELASTICITY / Fyw, 0.5)))
			{
				Cv = (1.10 * Math.Pow(k * ConstNum.ELASTICITY / Fyw, 0.5)) / (beam.Shape.t / beam.Shape.tw);
				fiOmegaString = ConstString.FIOMEGA0_9;
				fiOmegaValue = ConstNum.FIOMEGA0_9N;
			}
			else
			{
				Cv = 1.51 * ConstNum.ELASTICITY * k / (Fyw * Math.Pow(beam.Shape.h_tw, 2));
				fiOmegaString = ConstString.FIOMEGA0_9;
				fiOmegaValue = ConstNum.FIOMEGA0_9N;
			}

			Vn = 0.6 * Fyw * Aw * Cv;

			if (enableReporting)
			{
				Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Shear Yield Strength:");
				Vn = 0.6 * Fyw * Aw;
				Reporting.AddLine("Rn = 0.6 * Fy * A * Cv");
				Reporting.AddLine("= 0.6 * " + Fyw + " * " + Aw + " * " + Cv);
				Reporting.AddLine("= " + Vn + ConstUnit.Force);
				Reporting.AddLine(ConstString.PHI + " Rn = " + fiOmegaString + " * " + Vn + " = " + fiOmegaValue * Vn + ConstUnit.Force);
			}

			return ConstNum.FIOMEGA1_0N * Vn;
		}

		private static void BlockShearForWelded_BlockShearCapacity(EMemberType memberType, EShearCarriedBy shearopt, ref double Beamlv, ref double Lnt, ref double Lnv, ref double Lgt, ref double Lgv, double webd, ref double Lnv2, double Ubs, ref double PhiRn, ref double PhiRn2, ref double h, ref double Aw, ref double Anv, ref int a, ref double PhiRg, ref double PhiRnet, ref double BsCap)
		{
			double minPhiRn;

			var beam = CommonDataStatic.DetailDataDict[memberType];

			Reporting.AddHeader("Block Shear:");
			if (beam.WinConnect.Beam.TopCope)
			{
				switch (shearopt)
				{
					case EShearCarriedBy.ClipAngle:
						Beamlv = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (beam.WinConnect.ShearClipAngle.FrontY + beam.WinConnect.ShearClipAngle.Length / 2);
						// The following was added for Descon 8
						if (beam.WinConnect.Beam.TopCope &&
							beam.WinConnect.ShearClipAngle.BeamSideConnection == EConnectionStyle.Welded)
						{
							while (Beamlv < ConstNum.QUARTER_INCH)
								Beamlv += ConstNum.HALF_INCH;

							if (beam.WinConnect.Beam.CopeReinforcement.Type == EReinforcementType.None)
							{
								while (Beamlv < (beam.WinConnect.ShearClipAngle.WeldSizeBeam + ConstNum.SIXTEENTH_INCH))
									Beamlv += ConstNum.HALF_INCH;
							}
							else
							{
								while (Beamlv < (beam.WinConnect.ShearClipAngle.WeldSizeBeam + beam.WinConnect.Beam.CopeReinforcement.WeldSize + ConstNum.HALF_INCH))
									Beamlv += ConstNum.HALF_INCH;
							}
						}

						Lnt = beam.WinConnect.ShearClipAngle.OppositeOfOSL - beam.EndSetback;
						Lnv = beam.WinConnect.ShearClipAngle.Length + Beamlv;
						Lgt = Lnt;
						Lgv = Lnv;
						Reporting.AddHeader("Net Length with Tension resistance (Lnt)");
						Reporting.AddLine("= al - ec = " + beam.WinConnect.ShearClipAngle.OppositeOfOSL + " - " + beam.EndSetback + " = " + Lnt + ConstUnit.Length);
						Reporting.AddLine("Gross Length with Tension resistance (Lgt) = Lnt = " + Lgt + ConstUnit.Length);
						Reporting.AddLine("Net Length with Shear resistance (Lnv) = La + Lv");
						Reporting.AddLine("= " + beam.WinConnect.ShearClipAngle.Length + " + " + Beamlv + " = " + Lnv + ConstUnit.Length);
						Reporting.AddLine("Gross Length with Shear resistance (Lgv) = Lnv = " + Lgv + ConstUnit.Length);
						if (beam.WinConnect.Beam.BottomCope)
						{
							Lnv2 = webd;
							Reporting.AddHeader("Net Length with Shear resistance (no tensile area), Lnv' = " + Lnv2 + ConstUnit.Length);
						}

						if (!beam.WinConnect.DistanceToFirstBolt_User && beam.WinConnect.ShearClipAngle.Position != EPosition.MatchOtherSideBolts)
						{
							beam.WinConnect.Beam.DistanceToFirstBoltDisplay = Beamlv + beam.WinConnect.Beam.TCopeD + beam.WinConnect.ShearClipAngle.BoltOslOnSupport.EdgeDistLongDir;
							
							DetailData oppositeBeam;
							if (beam.MemberType == EMemberType.LeftBeam)
								oppositeBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
							else
								oppositeBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

							if (!oppositeBeam.WinConnect.DistanceToFirstBolt_User && oppositeBeam.WinConnect.ShearClipAngle.Position == EPosition.MatchOtherSideBolts)
								oppositeBeam.WinConnect.Beam.DistanceToFirstBoltDisplay = beam.WinConnect.Beam.DistanceToFirstBoltDisplay + -beam.WinConnect.Beam.TopElValue;
						}
						break;
					case EShearCarriedBy.Tee:
						Beamlv = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (beam.WinConnect.ShearWebTee.FrontY + beam.WinConnect.ShearWebTee.SLength / 2);
						// The following was added for Descon 8
						if (beam.WinConnect.Beam.TopCope &&
							beam.WinConnect.ShearWebTee.BeamSideConnection == EConnectionStyle.Welded)
						{
							while (Beamlv < ConstNum.QUARTER_INCH)
								Beamlv += ConstNum.HALF_INCH;

							if (beam.WinConnect.Beam.CopeReinforcement.Type == EReinforcementType.None)
							{
								while (Beamlv < (beam.WinConnect.ShearWebTee.WeldSizeStem + ConstNum.SIXTEENTH_INCH))
									Beamlv += ConstNum.HALF_INCH;
							}
							else
							{
								while (Beamlv < (beam.WinConnect.ShearWebTee.WeldSizeStem + beam.WinConnect.Beam.CopeReinforcement.WeldSize + ConstNum.SIXTEENTH_INCH))
									Beamlv += ConstNum.HALF_INCH;
							}
						}

						Lnt = beam.WinConnect.ShearWebTee.Size.d - beam.EndSetback;
						Lnv = beam.WinConnect.ShearWebTee.SLength + Beamlv;
						Lgt = Lnt;
						Lgv = Lnv;
						Reporting.AddHeader("Net Length with Tension resistance (Lnt)");
						Reporting.AddLine("= al - ec = " + beam.WinConnect.ShearWebTee.Size.d + " - " + beam.EndSetback + " = " + Lnt + ConstUnit.Length);
						Reporting.AddLine("Gross Length with Tension resistance (Lgt) = Lnt = " + Lgt + ConstUnit.Length);
						Reporting.AddLine("Net Length with Shear resistance (Lnv) = La + Lv");
						Reporting.AddLine("= " + beam.WinConnect.ShearWebTee.SLength + " + " + Beamlv + " = " + Lnv + ConstUnit.Length);
						Reporting.AddHeader("Gross Length with Shear resistance (Lgv) = Lnv = " + Lgv + ConstUnit.Length);
						if (beam.WinConnect.Beam.BottomCope)
						{
							Lnv2 = webd;
							Reporting.AddHeader("Net Length with Shear resistance (no tensile area), Lnv' = " + Lnv2 + ConstUnit.Length);
						}
						break;
				}

				PhiRn = BlockShearNew(beam.Material.Fu, Lnv, 1, Lnt, Lgv, beam.Material.Fy, beam.Shape.tw, 0, true);
				Reporting.AddLine("= " + PhiRn + ConstUnit.Force);

				if (beam.WinConnect.Beam.BottomCope)
				{
					PhiRn2 = ConstNum.FIOMEGA0_75N * 0.6 * beam.Material.Fu * Lnv2 * beam.Shape.tw;
					Reporting.AddHeader(ConstString.PHI + " Rn' = " + ConstString.FIOMEGA0_75 + " * (0.6 * Fu * Lnv') * tw");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * (0.6 * " + beam.Material.Fu + " * " + Lnv2 + ") * " + beam.Shape.tw);
					Reporting.AddLine("= " + PhiRn2 + ConstUnit.Force);
					minPhiRn = Math.Min(PhiRn, PhiRn2);
					Reporting.AddLine(ConstString.PHI + " Rn = Min(" + PhiRn + ", " + PhiRn2 + ") = " + minPhiRn + ConstUnit.Force);
					PhiRn = minPhiRn;
					Reporting.AddLine("= " + PhiRn + ConstUnit.Force);
				}
			}
			else
			{
				switch (shearopt)
				{
					case EShearCarriedBy.ClipAngle:
						PhiRn = webd * beam.Shape.tw * ConstNum.FIOMEGA0_75N * 0.6 * beam.Material.Fu;
						Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Shear Strength on Net Area, " + ConstString.PHI + " Rn");
						Reporting.AddLine("= d * tw * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu");
						Reporting.AddLine("= " + webd + " * " + beam.Shape.tw + " * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + beam.Material.Fu);
						Reporting.AddLine("= " + PhiRn + ConstUnit.Force);
						break;
					case EShearCarriedBy.Tee:
						PhiRn = webd * beam.Shape.tw * ConstNum.FIOMEGA0_75N * 0.6 * beam.Material.Fu;
						Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Shear Strength on Net Area, " + ConstString.PHI + " Rn");
						Reporting.AddLine("= d * tw * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu");
						Reporting.AddLine("= " + webd + " * " + beam.Shape.tw + " * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + beam.Material.Fu);
						Reporting.AddLine("= " + PhiRn + ConstUnit.Force);
						break;
				}
			}
			
			h = beam.Shape.d - Math.Max(beam.WinConnect.Beam.TCopeD, beam.Shape.kdes) - Math.Max(beam.WinConnect.Beam.BCopeD, beam.Shape.kdet);
			Aw = webd * beam.Shape.tw;
			Anv = webd * beam.Shape.tw;
			a = 500;     //  no stiffeners
			PhiRg = GrossShearStrength(beam.Material.Fy, Aw, memberType, true);
			PhiRnet = NetShearStrength(beam.Material.Fu, Anv, true);

			BsCap = Math.Min(PhiRnet, Math.Min(PhiRn, PhiRg));

			string capacityText = "Shear Rupture Strength";
			if (BsCap >= beam.ShearForce)
				Reporting.AddCapacityLine("Beam " + ConstString.DES_OR_ALLOWABLE + " Shear Strength = Min(" + ConstString.PHI + " Rn_rupture , " + ConstString.PHI + " Rn_yield , " + ConstString.PHI + " Rn_block_shear) = " + BsCap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / BsCap, capacityText, memberType);
			else
				Reporting.AddCapacityLine("Beam " + ConstString.DES_OR_ALLOWABLE + " Shear Strength = Min(" + ConstString.PHI + " Rn_rupture , " + ConstString.PHI + " Rn_yield , " + ConstString.PHI + " Rn_block_shear) = " + BsCap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / BsCap, capacityText, memberType);
		}

		internal static void BlockShearForWelded(EMemberType memberType, ref double BsCap, EShearCarriedBy shearopt)
		{
			double PhiRnet = 0;
			double Anv = 0;
			double PhiRg = 0;
			int a = 0;
			double Aw = 0;
			double h = 0;
			double PhiRn2 = 0;
			double PhiRn = 0;
			double Lnv2 = 0;
			double Lgv = 0;
			double Lgt = 0;
			double Lnv = 0;
			double Lnt = 0;
			double Beamlv = 0;
			double webd = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];

			webd = beam.Shape.d - beam.WinConnect.Beam.TCopeD - beam.WinConnect.Beam.BCopeD;

			if (beam.WinConnect.Beam.TopCope)
			{
				switch (shearopt)
				{
					case EShearCarriedBy.ClipAngle:
						Beamlv = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (beam.WinConnect.ShearClipAngle.FrontY + beam.WinConnect.ShearClipAngle.Length / 2);
						Lnt = beam.WinConnect.ShearClipAngle.LengthOfOSL - beam.EndSetback;
						Lnv = beam.WinConnect.ShearClipAngle.Length + Beamlv;
						Lgt = Lnt;
						Lgv = Lnt;
						if (beam.WinConnect.Beam.BottomCope)
							Lnv2 = webd;
						break;
					case EShearCarriedBy.Tee:
						Beamlv = beam.WinConnect.Beam.FrontY + beam.Shape.d / 2 - beam.WinConnect.Beam.TCopeD - (beam.WinConnect.ShearWebTee.FrontY + beam.WinConnect.ShearWebTee.SLength / 2);
						Lnt = beam.WinConnect.ShearWebTee.Size.d - beam.EndSetback;
						Lnv = beam.WinConnect.ShearWebTee.SLength + Beamlv;
						Lgt = Lnt;
						Lgv = Lnv;
						if (beam.WinConnect.Beam.BottomCope)
							Lnv2 = webd;
						break;
				}

				PhiRn = BlockShearNew(beam.Material.Fu, Lnv, 1, Lnt, Lgv, beam.Material.Fy, beam.Shape.tw, 0, false);
				if (beam.WinConnect.Beam.BottomCope)
				{
					PhiRn2 = ConstNum.FIOMEGA0_75N * 0.6 * beam.Material.Fu * Lnv2 * beam.Shape.tw;
					PhiRn = Math.Min(PhiRn, PhiRn2);
				}
			}
			else
			{
				if (shearopt == EShearCarriedBy.ClipAngle)
					PhiRn = webd * beam.Shape.tw * ConstNum.FIOMEGA0_75N * 0.6 * beam.Material.Fu;
				else if (shearopt == EShearCarriedBy.Tee)
					PhiRn = webd * beam.Shape.tw * ConstNum.FIOMEGA0_75N * 0.6 * beam.Material.Fu;
			}
			h = beam.Shape.d - Math.Max(beam.WinConnect.Beam.TCopeD, beam.Shape.kdet) - Math.Max(beam.WinConnect.Beam.BCopeD, beam.Shape.kdet);
			Aw = webd * beam.Shape.tw;
			a = 500; //  no stiffeners
			PhiRg = GrossShearStrength(beam.Material.Fy, Aw, memberType, false);

			Anv = webd * beam.Shape.tw;
			PhiRnet = NetShearStrength(beam.Material.Fu, Anv, false);
			BsCap = Math.Min(Math.Min(PhiRn, PhiRg), PhiRnet);
			BlockShearForWelded_BlockShearCapacity(memberType, shearopt, ref Beamlv, ref Lnt, ref Lnv, ref Lgt, ref Lgv, webd, ref Lnv2, 1, ref PhiRn, ref PhiRn2, ref h, ref Aw, ref Anv, ref a, ref PhiRg, ref PhiRnet, ref BsCap);
		}

		internal static double SeatWeld(EMemberType memberType, double P, double ShortLeg, double LongLeg, bool enableReport)
		{
			double seatWeld = 0;
			double FiRn = 0;
			double e = 0;
			double C1 = 0;
			double w = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];

			C1 = CommonCalculations.WeldTypeRatio();
			e = (ShortLeg - beam.EndSetback) / 2 + beam.EndSetback;
			beam.WinConnect.Fema.L = LongLeg;
			if (P > 0)
			{
				w = P * Math.Sqrt(1 + 20.25 * Math.Pow(e, 2) / Math.Pow(beam.WinConnect.Fema.L, 2)) / (2 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * C1 * beam.WinConnect.Fema.L);
				seatWeld = NumberFun.Round(w, 16);
			}
			else
			{
				FiRn = (2 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * C1 * beam.WinConnect.ShearSeat.WeldSizeSupport * beam.WinConnect.Fema.L / Math.Sqrt(1 + 20.25 * Math.Pow(e, 2) / Math.Pow(beam.WinConnect.Fema.L, 2)));
				seatWeld = FiRn;
			}

			if (enableReport)
			{
				Reporting.AddLine("e = (W - C) / 2 + C = (" + ShortLeg + " - " + beam.EndSetback + ") / 2 + " + beam.EndSetback + " = " + e + ConstUnit.Length);
				if (P > 0)
				{
					Reporting.AddHeader("Required Weld Size, w:");
					Reporting.AddLine("= Pu * (1 + 20.25 * e² / L²)^0.5 / (2 * " + ConstString.FIOMEGA0_75 + " * Fexx * c1 * L)");
					Reporting.AddLine("= " + P + " * (1 + 20.25 * " + e + "²/" + beam.WinConnect.Fema.L + "²)^0.5 / (2 * " + ConstString.FIOMEGA0_75 + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + C1 + " * " + beam.WinConnect.Fema.L + ")");
					Reporting.AddLine("= " + w + ConstUnit.Length);
				}
				else
				{
					Reporting.AddHeader("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ", " + ConstString.PHI + " Rn:");
					Reporting.AddLine("= 2 * " + ConstString.FIOMEGA0_75 + " * Fexx * c1 * w * L / (1 + 20.25 * e² / L²)^0.5");
					Reporting.AddLine("= 2 * " + ConstString.FIOMEGA0_75 + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + C1 + " * " + beam.WinConnect.ShearSeat.WeldSizeSupport + " * " + beam.WinConnect.Fema.L + " / (1 + 20.25 * " + e + "² / " + beam.WinConnect.Fema.L + "²)^0.5");
					Reporting.AddLine("= " + FiRn + ConstUnit.Force);
				}
			}

			return seatWeld;
		}

		internal static void OSLBoltsWithoutPlaneEccentricity(EMemberType memberType, double V, int h, int n, int s, double e, ref double ruv, ref double rut)
		{
			double y0 = 0;
			double Sum = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			ruv = V / (2 * n);
			beam.WinConnect.Fema.L = ((n - 1) * s);
			Sum = 0;
			y0 = beam.WinConnect.Fema.L / 2;
			do
			{
				Sum += y0;
				y0 = y0 - s;
			} while (y0 > 0);
			Sum = 2 * Sum;
			rut = V * e / (2 * Sum) + h / (2.0 * n);

			Reporting.AddLine("rut = V * e / (2 * dm) + H / N");
			Reporting.AddLine("= " + V + " * " + e + " / (2 * " + Sum + ") + " + h + " / " + (2 * n));
			Reporting.AddLine("= " + rut + ConstUnit.Force + " / bolt");
		}

		internal static double LateralPlateBuckling(double c, double ho, double Fy, bool enableReporting)
		{
			double intCaseArg;
			double tp_min = 0;
			double k = 0;

			intCaseArg = 2 * c / ho;
			if (intCaseArg <= 0.25)
				k = 16;
			else if (intCaseArg <= 0.3)
				k = 13;
			else if (intCaseArg <= 0.4)
				k = 10;
			else if (intCaseArg <= 0.5)
				k = 6;
			else if (intCaseArg <= 0.6)
				k = 4.5;
			else if (intCaseArg <= 0.75)
				k = 2.5;
			else if (intCaseArg <= 1)
				k = 1.3;
			else if (intCaseArg <= 1.5)
				k = 0.8;
			else if (intCaseArg <= 2)
				k = 0.6;
			else if (intCaseArg <= 3)
				k = 0.5;
			else
				k = 0.425;
			
			tp_min = ho * Math.Sqrt(Fy) / (1.37 * Math.Sqrt(ConstNum.ELASTICITY * k));

			if (enableReporting)
			{
				Reporting.AddHeader("Lateral Plate Buckling:");
				Reporting.AddLine("c = " + c + ConstUnit.Length + ", ho = L = " + ho + ConstUnit.Length + ", 2c / ho = " + (2 * c / ho) + ",   K = " + k);
				Reporting.AddLine("t_min = L * (Fy^0.5) / (1.37 * (" + ConstNum.ELASTICITY + " * K)^0.5) = " + ho + " * " + Math.Pow(Fy, 0.5) + " / (1.37 * " + Math.Pow(ConstNum.ELASTICITY * k, 0.5) + ") >= " + ConstNum.QUARTER_INCH);
			}

			return Math.Max(ConstNum.QUARTER_INCH, tp_min);
		}

		internal static double K_Factor(int c, double h0)
		{
			double tmpCaseArg = 0;
			double k = 0;

			tmpCaseArg = 2 * c / h0;
			if (tmpCaseArg <= 0.25)
				k = 16;
			else if (tmpCaseArg <= 0.3)
				k = 13;
			else if (tmpCaseArg <= 0.4)
				k = 10;
			else if (tmpCaseArg <= 0.5)
				k = 6;
			else if (tmpCaseArg <= 0.6)
				k = 4.5;
			else if (tmpCaseArg <= 0.75)
				k = 2.5;
			else if (tmpCaseArg <= 1)
				k = 1.3;
			else if (tmpCaseArg <= 1.5)
				k = 0.8;
			else if (tmpCaseArg <= 2)
				k = 0.6;
			else if (tmpCaseArg <= 3)
				k = 0.5;
			else
				k = 0.425;
			
			return k;
		}

		private static double NetShearStrength(double Fuw, double Anv, bool enableReporting)
		{
			double NetShearStrength = 0;
			double Rn = 0;

			Rn = 0.6 * Fuw * Anv;
			NetShearStrength = ConstNum.FIOMEGA0_75N * Rn;

		    if (enableReporting)
		    {
		        Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Shear Rupture Strength:");
		        Rn = 0.6*Fuw*Anv;
		        Reporting.AddLine(" Rn = 0.6 * Fu * Anv");
		        Reporting.AddLine("= 0.6 * " + Fuw + " * " + Anv);
		        Reporting.AddLine("= " + Rn + ConstUnit.Force);
				Reporting.AddLine(string.Empty);
		        Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * " + Rn + " = " +
		                          ConstNum.FIOMEGA0_75N*Rn + ConstUnit.Force);
		    }

		    return NetShearStrength;
		}
	}
}