using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class Stiff
	{
		public static void Stiffeners(bool enableReporting)
		{
			double lngthP = 0;
			double lngthC = 0;
			double MinWidth = 0;
			double ast = 0;
			double tCompstiff = 0;
			double tsmin = 0;
			double clip = 0;
			double tTensionStiff = 0;
			double fl = 0;
			double lngth = 0;
			double maxwidth = 0;
			double tBot = 0;
			double tTop = 0;
			double AstMinComp = 0;
			double AstMinTen = 0;
			double CompStifForce = 0;
			double TenstifForce = 0;
			double LFiRnWebBuckling = 0;
			double LFiRnWebCrippling = 0;
			double LtBot = 0;
			double LFiRnWebYielding = 0;
			double Lbsmin = 0;
			double LFiRnFlBending = 0;
			double LtTop = 0;
			double RFiRnWebBuckling = 0;
			double RFiRnWebCrippling = 0;
			double Nd = 0;
			double n = 0;
			double RtBot = 0;
			double RFiRnWebYielding = 0;
			double fyc = 0;
			double Alpham = 0;
			double bs = 0;
			double Pe = 0;
			double k1 = 0;
			double RbsMin = 0;
			double RFiRnFlBending = 0;
			double RtTop = 0;
			double ct = 0;

			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var stiffener = CommonDataStatic.ColumnStiffener;

			if (rightBeam.MomentConnection != EMomentCarriedBy.NoMoment)
			{
				// Local Flange Bending
				if (stiffener.TopOfBeamToColumn >= 10 * column.Shape.tf || !stiffener.BeamNearTopOfColumn)
					ct = 1;
				else
					ct = 0.5;
				switch (rightBeam.MomentConnection)
				{
					case EMomentCarriedBy.FlangePlate:
						RtTop = rightBeam.WinConnect.MomentFlangePlate.TopThickness;
						RFiRnFlBending = ConstNum.FIOMEGA0_9N * 6.25 * Math.Pow(column.Shape.tf, 2) * column.Material.Fy * ct;
						RbsMin = Math.Max(rightBeam.WinConnect.MomentFlangePlate.TopWidth, rightBeam.WinConnect.MomentFlangePlate.BottomWidth) / 3 - column.Shape.tw / 2;
						break;
					case EMomentCarriedBy.DirectlyWelded:
						RtTop = rightBeam.Shape.tf;
						RFiRnFlBending = ConstNum.FIOMEGA0_9N * 6.25 * Math.Pow(column.Shape.tf, 2) * column.Material.Fy * ct;
						RbsMin = (rightBeam.Shape.bf - column.Shape.tw) / 2;
						break;
					case EMomentCarriedBy.EndPlate:
						RtTop = rightBeam.Shape.tf;
						k1 = column.Shape.k1; // RBeam.k - RBeam.tf + RBeam.tw / 2
						Pe = rightBeam.GageOnFlange / 2 - rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize / 4 - k1;
						if (rightBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts == 8)
						{
							bs = 2 * rightBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt + rightBeam.Shape.tf + 3.5 * rightBeam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir;
							Alpham = 1.13 * Math.Pow(Pe / rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize, 0.25);
						}
						else
						{
							bs = 2.5 * (2 * rightBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt + rightBeam.Shape.tf);
							Alpham = 1.36 * Math.Pow(Pe / rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize, 0.25);
						}
						fyc = column.Material.Fy;
						RFiRnFlBending = ConstNum.FIOMEGA0_9N * (bs / (Alpham * Pe)) * Math.Pow(column.Shape.tf, 2) * fyc * ct;
						RbsMin = rightBeam.Shape.bf / 3 - column.Shape.tw / 2;
						break;
					case EMomentCarriedBy.Tee:
						RtTop = rightBeam.WinConnect.MomentTee.TopTeeShape.tf;
						k1 = column.Shape.k1;
						Pe = rightBeam.WinConnect.MomentTee.Column_g / 2 - rightBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize / 4 - k1;
						if (rightBeam.WinConnect.MomentTee.BoltColumnFlange.NumberOfBolts == 8)
						{
							bs = 2 * rightBeam.WinConnect.MomentTee.Column_a + rightBeam.WinConnect.MomentTee.TopTeeShape.tf + 3.5 * rightBeam.WinConnect.MomentTee.Column_s;
							Alpham = 1.13 * Math.Pow(Pe / rightBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize, 0.25);
						}
						else
						{
							bs = 2.5 * (2 * rightBeam.WinConnect.MomentTee.Column_a + rightBeam.WinConnect.MomentTee.TopTeeShape.tf);
							Alpham = 1.36 * Math.Pow(Pe / rightBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize, 0.25);
						}
						fyc = column.Material.Fy;

						RFiRnFlBending = ConstNum.FIOMEGA0_9N * (bs / (Alpham * Pe)) * Math.Pow(column.Shape.tf, 2) * fyc * ct;
						RbsMin = rightBeam.Shape.bf / 3 - column.Shape.tw / 2;
						break;
					case EMomentCarriedBy.Angles:
						RtTop = rightBeam.WinConnect.MomentFlangeAngle.Angle.t;
						k1 = column.Shape.k1;
						Pe = rightBeam.GageOnColumn / 2 - rightBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize / 4 - k1;
						if (rightBeam.WinConnect.MomentFlangeAngle.ColumnBolt.NumberOfBolts == 4)
							bs = 2.5 * (rightBeam.GageOnColumn / 2 - column.Shape.k1) + rightBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize + 2 * rightBeam.GageOnColumn;
						else
							bs = 2.5 * (rightBeam.GageOnColumn / 2 - column.Shape.k1) + rightBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize;
						Alpham = 1.36 * Math.Pow(Pe / rightBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize, 0.25);

						fyc = column.Material.Fy;
						RFiRnFlBending = ConstNum.FIOMEGA0_9N * (bs / (Alpham * Pe)) * Math.Pow(column.Shape.tf, 2) * fyc * ct;
						RbsMin = rightBeam.Shape.bf / 3 - column.Shape.tw / 2;
						break;
				}
				// Local Web Yielding
				if (stiffener.TopOfBeamToColumn >= column.Shape.d || !stiffener.BeamNearTopOfColumn)
					ct = 1;
				else
					ct = 0.5;
				switch (rightBeam.MomentConnection)
				{
					case EMomentCarriedBy.FlangePlate:
						RFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * 5 * column.Shape.kdes + rightBeam.WinConnect.MomentFlangePlate.TopThickness + 2 * rightBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize) * column.Shape.tw * column.Material.Fy;
						break;
					case EMomentCarriedBy.DirectlyWelded:
						RFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * 5 * column.Shape.kdes + rightBeam.Shape.tf) * column.Shape.tw * column.Material.Fy;
						break;
					case EMomentCarriedBy.EndPlate:
						RFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * (6 * column.Shape.kdes + 2 * rightBeam.WinConnect.MomentEndPlate.Thickness) + rightBeam.Shape.tf + 2 * rightBeam.WinConnect.MomentEndPlate.FlangeWeldSize) * column.Shape.tw * column.Material.Fy;
						break;
					case EMomentCarriedBy.Tee:
						RFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * (6 * column.Shape.kdes + 2 * rightBeam.WinConnect.MomentTee.TopTeeShape.tf) + rightBeam.WinConnect.MomentTee.TopTeeShape.tw) * column.Shape.tw * column.Material.Fy;
						break;
					case EMomentCarriedBy.Angles:
						RFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * 5 * column.Shape.kdes + rightBeam.WinConnect.MomentFlangeAngle.Angle.t) * column.Shape.tw * column.Material.Fy;
						break;
				}

				// Web Crippling
				switch (rightBeam.MomentConnection)
				{
					case EMomentCarriedBy.FlangePlate:
						RtBot = rightBeam.WinConnect.MomentFlangePlate.BottomThickness;
						n = rightBeam.WinConnect.MomentFlangePlate.BottomThickness + 2 * rightBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize;
						break;
					case EMomentCarriedBy.DirectlyWelded:
						RtBot = rightBeam.Shape.tf;
						n = rightBeam.Shape.tf + 2 * rightBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize;
						break;
					case EMomentCarriedBy.EndPlate:
						RtBot = rightBeam.Shape.tf;
						n = rightBeam.Shape.tf + 2 * rightBeam.WinConnect.MomentEndPlate.FlangeWeldSize + 2 * rightBeam.WinConnect.MomentEndPlate.Thickness;
						break;
					case EMomentCarriedBy.Tee:
						RtBot = rightBeam.WinConnect.MomentTee.TopTeeShape.tf;
						n = rightBeam.WinConnect.MomentTee.TopTeeShape.tw + 2 * (rightBeam.WinConnect.MomentTee.TopTeeShape.kdes - rightBeam.WinConnect.MomentTee.TopTeeShape.tf) + 2 * rightBeam.WinConnect.MomentTee.TopTeeShape.tf;
						break;
					case EMomentCarriedBy.Angles:
						RtBot = rightBeam.WinConnect.MomentFlangeAngle.Angle.t;
						n = rightBeam.WinConnect.MomentFlangeAngle.Angle.tf + rightBeam.WinConnect.MomentFlangeAngle.Angle.t;
						break;
				}
				if (stiffener.TopOfBeamToColumn >= column.Shape.d / 2 || !stiffener.BeamNearTopOfColumn)
				{
					ct = 1;
					if (n / column.Shape.d <= 0.2)
						Nd = 3 * n / column.Shape.d;
					else
						Nd = 4 * n / column.Shape.d - 0.2;
				}
				else
				{
					ct = 0.5;
					Nd = 3 * n / column.Shape.d;
				}
				RFiRnWebCrippling = ConstNum.FIOMEGA0_75N * 0.8 * ct * Math.Pow(column.Shape.tw, 2) * (1 + Nd * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);

				// Compression Buckling of the Web
				if (stiffener.TopOfBeamToColumn >= column.Shape.d / 2 || !stiffener.BeamNearTopOfColumn)
					ct = 1;
				else
					ct = 0.5;
				RFiRnWebBuckling = ConstNum.FIOMEGA0_9N * 24 * ct * Math.Pow(column.Shape.tw, 3) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy, 0.5) / (column.Shape.d - 2 * column.Shape.kdes);
			}

			//  **** Left Side Beam
			if (leftBeam.MomentConnection != EMomentCarriedBy.NoMoment)
			{
				// Local Flange Bending
				if (stiffener.TopOfBeamToColumn >= 10 * column.Shape.tf || !stiffener.BeamNearTopOfColumn)
				{
					ct = 1;
				}
				else
				{
					ct = 0.5;
				}
				switch (leftBeam.MomentConnection)
				{
					case EMomentCarriedBy.FlangePlate:
						LtTop = leftBeam.WinConnect.MomentFlangePlate.TopThickness;
						LFiRnFlBending = ConstNum.FIOMEGA0_9N * 6.25 * Math.Pow(column.Shape.tf, 2) * column.Material.Fy * ct;
						Lbsmin = Math.Max(leftBeam.WinConnect.MomentFlangePlate.TopWidth, leftBeam.WinConnect.MomentFlangePlate.BottomWidth) / 3 - column.Shape.tw / 2;
						break;
					case EMomentCarriedBy.DirectlyWelded:
						LtTop = leftBeam.Shape.tf;
						LFiRnFlBending = ConstNum.FIOMEGA0_9N * 6.25 * Math.Pow(column.Shape.tf, 2) * column.Material.Fy * ct;
						Lbsmin = leftBeam.Shape.bf / 3 - column.Shape.tw / 2;
						break;
					case EMomentCarriedBy.EndPlate:
						LtTop = leftBeam.Shape.tf;
						k1 = column.Shape.k1; // LBeam.k - LBeam.tf + LBeam.tw / 2
						Pe = leftBeam.GageOnFlange / 2 - leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize / 4 - k1;
						if (leftBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts == 8)
						{
							bs = 2 * leftBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt + leftBeam.Shape.tf + 3.5 * leftBeam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir;
							Alpham = 1.13 * Math.Pow(Pe / leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize, 0.25);
						}
						else
						{
							bs = 2.5 * (2 * leftBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt + leftBeam.Shape.tf);
							Alpham = 1.36 * Math.Pow(Pe / leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize, 0.25);
						}
						fyc = column.Material.Fy;

						LFiRnFlBending = ConstNum.FIOMEGA0_9N * (bs / (Alpham * Pe)) * Math.Pow(column.Shape.tf, 2) * fyc * ct;
						Lbsmin = leftBeam.Shape.bf / 3 - column.Shape.tw / 2;
						break;
					case EMomentCarriedBy.Tee:
						LtTop = leftBeam.WinConnect.MomentTee.TopTeeShape.tf;
						k1 = column.Shape.k1;
						Pe = leftBeam.WinConnect.MomentTee.Column_g / 2 - leftBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize / 4 - k1;
						if (leftBeam.WinConnect.MomentTee.BoltColumnFlange.NumberOfBolts == 8)
						{
							bs = 2 * leftBeam.WinConnect.MomentTee.Column_a + leftBeam.WinConnect.MomentTee.TopTeeShape.tf + 3.5 * leftBeam.WinConnect.MomentTee.Column_s;
							Alpham = 1.13 * Math.Pow(Pe / leftBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize, 0.25);
						}
						else
						{
							bs = 2.5 * (2 * leftBeam.WinConnect.MomentTee.Column_a + leftBeam.WinConnect.MomentTee.TopTeeShape.tf);
							Alpham = 1.36 * Math.Pow(Pe / leftBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize, 0.25);
						}
						fyc = column.Material.Fy;

						LFiRnFlBending = ConstNum.FIOMEGA0_9N * (bs / (Alpham * Pe)) * Math.Pow(column.Shape.tf, 2) * fyc * ct;
						Lbsmin = leftBeam.Shape.bf / 3 - column.Shape.tw / 2;
						break;
					case EMomentCarriedBy.Angles:
						LtTop = leftBeam.WinConnect.MomentFlangeAngle.Angle.t;
						k1 = column.Shape.k1;
						Pe = leftBeam.GageOnColumn / 2 - leftBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize / 4 - k1;
						if (leftBeam.WinConnect.MomentFlangeAngle.ColumnBolt.NumberOfBolts == 4)
							bs = 2.5 * (leftBeam.GageOnColumn / 2 - column.Shape.k1) + leftBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize + 2 * leftBeam.GageOnColumn;
						else
							bs = 2.5 * (leftBeam.GageOnColumn / 2 - column.Shape.k1) + leftBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize;
						Alpham = 1.36 * Math.Pow(Pe / leftBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize, 0.25);
						fyc = column.Material.Fy;

						LFiRnFlBending = ConstNum.FIOMEGA0_9N * (bs / (Alpham * Pe)) * Math.Pow(column.Shape.tf, 2) * fyc * ct;
						Lbsmin = (leftBeam.Shape.bf - column.Shape.tw) / 2;
						break;
				}
				// Local Web Yielding
				if (stiffener.TopOfBeamToColumn >= column.Shape.d || !stiffener.BeamNearTopOfColumn)
					ct = 1;
				else
					ct = 0.5;
				switch (leftBeam.MomentConnection)
				{
					case EMomentCarriedBy.FlangePlate:
						LFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * 5 * column.Shape.kdes + leftBeam.WinConnect.MomentFlangePlate.TopThickness + 2 * leftBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize) * column.Shape.tw * column.Material.Fy;
						break;
					case EMomentCarriedBy.DirectlyWelded:
						LFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * 5 * column.Shape.kdes + leftBeam.Shape.tf + 2 * leftBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize) * column.Shape.tw * column.Material.Fy;
						break;
					case EMomentCarriedBy.EndPlate:
						LFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * (6 * column.Shape.kdes + 2 * leftBeam.WinConnect.MomentEndPlate.Thickness) + leftBeam.Shape.tf + 2 * leftBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize) * column.Shape.tw * column.Material.Fy;
						break;
					case EMomentCarriedBy.Tee:
						LFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * (6 * column.Shape.kdes + 2 * leftBeam.WinConnect.MomentTee.TopTeeShape.tf) + leftBeam.WinConnect.MomentTee.TopTeeShape.tw) * column.Shape.tw * column.Material.Fy;
						break;
					case EMomentCarriedBy.Angles:
						LFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * 5 * column.Shape.kdes + leftBeam.WinConnect.MomentFlangeAngle.Angle.t) * column.Shape.tw * column.Material.Fy;
						break;
				}

				// Web Crippling
				switch (leftBeam.MomentConnection)
				{
					case EMomentCarriedBy.FlangePlate:
						LtBot = leftBeam.WinConnect.MomentFlangePlate.BottomThickness;
						n = leftBeam.WinConnect.MomentFlangePlate.BottomThickness + 2 * leftBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize;
						break;
					case EMomentCarriedBy.DirectlyWelded:
						LtBot = leftBeam.Shape.tf;
						n = leftBeam.Shape.tf + 2 * leftBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize;
						break;
					case EMomentCarriedBy.EndPlate:
						LtBot = leftBeam.Shape.tf;
						n = leftBeam.Shape.tf + 2 * leftBeam.WinConnect.MomentEndPlate.FlangeWeldSize + 2 * leftBeam.WinConnect.MomentEndPlate.Thickness;
						break;
					case EMomentCarriedBy.Tee:
						LtBot = leftBeam.WinConnect.MomentTee.TopTeeShape.tf;
						n = leftBeam.WinConnect.MomentTee.TopTeeShape.tw + 2 * (leftBeam.WinConnect.MomentTee.TopTeeShape.kdes - leftBeam.WinConnect.MomentTee.TopTeeShape.tf) + 2 * leftBeam.WinConnect.MomentTee.TopTeeShape.tf;
						break;
					case EMomentCarriedBy.Angles:
						LtBot = leftBeam.WinConnect.MomentFlangeAngle.Angle.t;
						n = leftBeam.WinConnect.MomentFlangeAngle.Angle.tf + leftBeam.WinConnect.MomentFlangeAngle.Angle.t;
						break;
				}
				if (stiffener.TopOfBeamToColumn >= column.Shape.d / 2 || !stiffener.BeamNearTopOfColumn)
				{
					ct = 1;
					if (n / column.Shape.d <= 0.2)
						Nd = 3 * n / column.Shape.d;
					else
						Nd = 4 * n / column.Shape.d - 0.2;
				}
				else
				{
					ct = 0.5;
					Nd = 3 * n / column.Shape.d;
				}
				LFiRnWebCrippling = ConstNum.FIOMEGA0_75N * 0.8 * ct * Math.Pow(column.Shape.tw, 2) * (1 + Nd * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);

				// Compression Buckling of the Web
				if (stiffener.TopOfBeamToColumn >= column.Shape.d / 2 || !stiffener.BeamNearTopOfColumn)
					ct = 1;
				else
					ct = 0.5;
				LFiRnWebBuckling = ConstNum.FIOMEGA0_9N * 24 * ct * Math.Pow(column.Shape.tw, 3) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy, 0.5) / (column.Shape.d - 2 * column.Shape.kdes);
			}

			stiffener.LTenstifForce = Math.Max(stiffener.LPuf - LFiRnFlBending, stiffener.LPuf - LFiRnWebYielding);
			if (stiffener.LTenstifForce < 0)
				stiffener.LTenstifForce = 0;
			stiffener.RTenstifForce = Math.Max(stiffener.RPuf - RFiRnFlBending, stiffener.RPuf - RFiRnWebYielding);
			if (stiffener.RTenstifForce < 0)
				stiffener.RTenstifForce = 0;

			if (rightBeam.MomentConnection != EMomentCarriedBy.NoMoment)
				stiffener.LCompStifForce = Math.Max(stiffener.LPuf - LFiRnWebCrippling, stiffener.LPuf - LFiRnWebBuckling);
			else
				stiffener.LCompStifForce = stiffener.LPuf - LFiRnWebCrippling;

			stiffener.LCompStifForce = Math.Max(stiffener.LCompStifForce, stiffener.LPuf - LFiRnWebYielding);

			if (leftBeam.MomentConnection != EMomentCarriedBy.NoMoment)
				stiffener.RCompStifForce = Math.Max(stiffener.RPuf - RFiRnWebCrippling, stiffener.RPuf - RFiRnWebBuckling);
			else
				stiffener.RCompStifForce = stiffener.RPuf - RFiRnWebCrippling;

			stiffener.RCompStifForce = Math.Max(stiffener.RCompStifForce, stiffener.RPuf - RFiRnWebYielding);

			if (stiffener.LCompStifForce < 0)
				stiffener.LCompStifForce = 0;
			if (stiffener.RCompStifForce < 0)
				stiffener.RCompStifForce = 0;

			TenstifForce = Math.Max(stiffener.LTenstifForce, stiffener.RTenstifForce);
			CompStifForce = Math.Max(stiffener.LCompStifForce, stiffener.RCompStifForce);
			AstMinTen = TenstifForce / (ConstNum.FIOMEGA0_9N * stiffener.Material.Fy);
			AstMinComp = CompStifForce / (ConstNum.FIOMEGA0_9N * stiffener.Material.Fy);
			tTop = Math.Max(LtTop, RtTop);
			tBot = Math.Max(LtBot, RtBot);

			if (TenstifForce > 0 || CommonDataStatic.SeismicSettings.Type == ESeismicType.High)
				stiffener.TenStiff = true;
			else
				stiffener.TenStiff = false;
			if (CompStifForce > 0 || CommonDataStatic.SeismicSettings.Type == ESeismicType.High)
				stiffener.CompStiff = true;
			else
				stiffener.CompStiff = false;

			if (stiffener.TenStiff)
			{
				if (!stiffener.SWidth_User && CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
					stiffener.SWidth = (column.Shape.bf - column.Shape.tw) / 2;
				else if (!stiffener.SWidth_User && CommonDataStatic.SeismicSettings.Type == ESeismicType.High)
					stiffener.SWidth = (column.Shape.bf - column.Shape.tw) / 2;

				if (!stiffener.SWidth_User && CommonDataStatic.Preferences.Seismic == ESeismic.Seismic)
					stiffener.SWidth = (column.Shape.bf - column.Shape.tw) / 2;

				if (!stiffener.SWidth_User && stiffener.UseExtendedDoublerPlate)
				{
					switch (stiffener.DNumberOfPlates)
					{
						case 2:
						case 1:
							maxwidth = (column.Shape.bf - column.Shape.tw) / 2 - stiffener.DThickness;
							stiffener.SWidth = Math.Min(stiffener.SWidth, maxwidth);
							break;
						case 0:
							maxwidth = (column.Shape.bf - column.Shape.tw) / 2;
							stiffener.SWidth = Math.Min(stiffener.SWidth, maxwidth);
							break;
					}
				}
				if (!stiffener.SWidth_User)
				{
					if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic && stiffener.SThickness > 0)
						stiffener.SWidth = maxwidth;
					stiffener.SWidth = NumberFun.Round(stiffener.SWidth, 4);
				}

				if (stiffener.StiffenerLength == EStiffenerLength.PartialLengthOfWeb)
				{
					lngth = TenstifForce / (ConstNum.FIOMEGA0_9N * 0.6 * column.Shape.tw * column.Material.Fy) + (column.Shape.kdet - column.Shape.tf);
					if (lngth <= column.Shape.d / 2 - column.Shape.tf)
						stiffener.SLength = NumberFun.Round(column.Shape.d / 2 - column.Shape.tf, 4);
					else if (lngth < column.Shape.d - 2 * column.Shape.kdet)
						stiffener.SLength = NumberFun.Round(lngth + column.Shape.kdet - column.Shape.tf, 8);
					else
						stiffener.SLength = ((int)Math.Floor(16 * (column.Shape.d - 2 * column.Shape.tf))) / 16.0;
					fl = column.Shape.kdet - column.Shape.tf;
					tTensionStiff = TenstifForce / (ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * (stiffener.SLength - fl) * 2);
				}
				else
				{
					stiffener.SLength = ((int)Math.Floor(16 * (column.Shape.d - 2 * column.Shape.tf))) / 16.0;
					fl = 2 * (column.Shape.kdet - column.Shape.tf);
					tTensionStiff = (stiffener.LTenstifForce + stiffener.RTenstifForce) / (ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * (stiffener.SLength - fl) * 2);
				}

				clip = column.Shape.kdet - column.Shape.tf;
				if (tTensionStiff < AstMinTen / (2 * (stiffener.SWidth - clip)))
					tTensionStiff = AstMinTen / (2 * (stiffener.SWidth - clip));

				if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
					tsmin = Math.Max(tTop / 2, stiffener.SWidth * Math.Pow(stiffener.Material.Fy / ConstNum.ELASTICITY, 0.5) / 0.56);
				else if (CommonDataStatic.SeismicSettings.Type == ESeismicType.High)
					tsmin = Math.Max(tTop, stiffener.SWidth * Math.Pow(stiffener.Material.Fy / ConstNum.ELASTICITY, 0.5) / 0.56);

				tTensionStiff = Math.Max(tsmin, tTensionStiff);
			}
			if (stiffener.CompStiff)
			{
				if (!stiffener.SWidth_User && CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
					stiffener.SWidth = (column.Shape.bf - column.Shape.tw) / 2; //Math.Max(RbsMin, Lbsmin); (2015)
				else if (!stiffener.SWidth_User && CommonDataStatic.SeismicSettings.Type == ESeismicType.High)
					stiffener.SWidth = (column.Shape.bf - column.Shape.tw) / 2;

				if (!stiffener.SWidth_User && stiffener.UseExtendedDoublerPlate)
				{
					switch (stiffener.DNumberOfPlates)
					{
						case 2:
						case 1:
							maxwidth = (column.Shape.bf - column.Shape.tw) / 2 - stiffener.DThickness;
							stiffener.SWidth = Math.Min(stiffener.SWidth, maxwidth);
							break;
						case 0:
							maxwidth = (column.Shape.bf - column.Shape.tw) / 2;
							stiffener.SWidth = Math.Min(stiffener.SWidth, maxwidth);
							break;
					}
				}

				if (!stiffener.SWidth_User)
				{
					if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic && stiffener.SThickness > 0)
						stiffener.SWidth = (column.Shape.bf - column.Shape.tw - stiffener.DNumberOfPlates * stiffener.DThickness) / 2;
					stiffener.SWidth = NumberFun.Round(stiffener.SWidth, 4);
				}

				if (stiffener.StiffenerLength == EStiffenerLength.PartialLengthOfWeb)
				{
					lngth = CompStifForce / (ConstNum.FIOMEGA0_9N * 0.6 * column.Shape.tw * column.Material.Fy) + (column.Shape.kdet - column.Shape.tf);
					if (lngth <= column.Shape.d / 2 - column.Shape.tf)
						stiffener.SLength = NumberFun.Round(column.Shape.d / 2 - column.Shape.tf, 4);
					else if (lngth < column.Shape.d - 2 * column.Shape.kdet)
						stiffener.SLength = NumberFun.Round(lngth + column.Shape.kdet - column.Shape.tf, 8);
					else
						stiffener.SLength = ((int)Math.Floor(16 * (column.Shape.d - 2 * column.Shape.tf))) / 16.0;
					fl = column.Shape.kdet - column.Shape.tf;
					tTensionStiff = CompStifForce / (ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * (stiffener.SLength - fl) * 2);
					fl = column.Shape.kdet - column.Shape.tf;
					tCompstiff = CompStifForce / (ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * (stiffener.SLength - fl) * 2);
				}
				else
				{
					stiffener.SLength = NumberFun.Round(column.Shape.d - 2 * column.Shape.tf, 16);
					fl = 2 * (column.Shape.kdet - column.Shape.tf);
					tCompstiff = (stiffener.LCompStifForce + stiffener.RCompStifForce) / (ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * (stiffener.SLength - fl) * 2);
				}

				clip = column.Shape.kdet - column.Shape.tf;
				if (tCompstiff < AstMinComp / (2 * (stiffener.SWidth - clip)))
					tCompstiff = AstMinComp / (2 * (stiffener.SWidth - clip));

				if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
					tsmin = Math.Max(tBot / 2, stiffener.SWidth * Math.Pow(stiffener.Material.Fy / ConstNum.ELASTICITY, 0.5) / 0.56);
				else if (CommonDataStatic.SeismicSettings.Type == ESeismicType.High)
					tsmin = Math.Max(tBot, stiffener.SWidth * Math.Pow(stiffener.Material.Fy / ConstNum.ELASTICITY, 0.5) / 0.56);

				tCompstiff = Math.Max(tsmin, tCompstiff);
			}

			if (!stiffener.SThickness_User)
				stiffener.SThickness = NumberFun.Round(Math.Max(tTensionStiff, tCompstiff), 16);

			if (!enableReporting)
				return;

			Reporting.AddGoToHeader("Column Stiffeners");
			if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic && CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.RBS)
			{
				Reporting.AddHeader("Minimum Thickness of Stiffener plates");
				if (rightBeam.IsActive && leftBeam.IsActive)
				{
					if (!stiffener.SThickness_User)
						stiffener.SThickness = Math.Max(rightBeam.Shape.tf, leftBeam.Shape.tf);
					Reporting.AddHeader("Minimum t_ps = max(RBeam_tf, LBeam_tf)= Max( " + rightBeam.Shape.tf + ", " + leftBeam.Shape.tf + " )");
					Reporting.AddLine(" = " + stiffener.SThickness + ConstUnit.Length);

				}
				else if (rightBeam.IsActive)
				{
					if (!stiffener.SThickness_User)
						stiffener.SThickness = rightBeam.Shape.tf / 2;
					Reporting.AddHeader("Minimum t_ps =  RBeam_tf / 2 = " + (rightBeam.Shape.tf / 2.0) + ConstUnit.Length);

				}
				else if (leftBeam.IsActive)
				{
					if (!stiffener.SThickness_User)
						stiffener.SThickness = leftBeam.Shape.tf / 2;
					Reporting.AddHeader("Minimum t_ps =  LBeam_tf / 2 = " + (leftBeam.Shape.tf / 2) + ConstUnit.Length);
				}

				Reporting.AddLine("See Below for Required thickness of Stiffeners");
			}
			if (rightBeam.IsActive && rightBeam.MomentConnection != EMomentCarriedBy.NoMoment)
			{
				Reporting.AddHeader("Right Side Beam");
				Reporting.AddLine("Local Flange Bending Strength," + ConstString.PHI + " Rn");

				if (stiffener.TopOfBeamToColumn >= 10 * column.Shape.tf || !stiffener.BeamNearTopOfColumn)
					ct = 1;
				else
					ct = 0.5;
				switch (rightBeam.MomentConnection)
				{
					case EMomentCarriedBy.FlangePlate:
						RtTop = rightBeam.WinConnect.MomentFlangePlate.TopThickness;
						RFiRnFlBending = ConstNum.FIOMEGA0_9N * 6.25 * Math.Pow(column.Shape.tf, 2) * column.Material.Fy * ct;
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 6.25 * (tf²) * Fy * ct");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 6.25 * (" + column.Shape.tf + "²) * " + column.Material.Fy + " * " + ct);
						RbsMin = Math.Max(rightBeam.WinConnect.MomentFlangePlate.TopWidth, rightBeam.WinConnect.MomentFlangePlate.BottomWidth) / 3 - column.Shape.tw / 2;
						break;
					case EMomentCarriedBy.DirectlyWelded:
						RtTop = rightBeam.Shape.tf;
						RFiRnFlBending = ConstNum.FIOMEGA0_9N * 6.25 * Math.Pow(column.Shape.tf, 2) * column.Material.Fy * ct;
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 6.25 * (tf²) * Fy * ct");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 6.25 * (" + column.Shape.tf + "²) * " + column.Material.Fy + " * " + ct);
						RbsMin = rightBeam.Shape.bf / 3 - column.Shape.tw / 2;
						break;
					case EMomentCarriedBy.EndPlate:
						RtTop = rightBeam.Shape.tf;
						k1 = column.Shape.k1;
						Pe = rightBeam.GageOnFlange / 2 - rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize / 4 - k1;
						Reporting.AddLine("Pe = g / 2 - db / 4 - k1 = " + rightBeam.GageOnFlange + " / 2 - " + rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + " / 4 - " + k1 + " = " + Pe + ConstUnit.Length);
						if (rightBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts == 8)
						{
							bs = 2 * rightBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt + rightBeam.Shape.tf + 3.5 * rightBeam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir;
							Reporting.AddLine("bs = 2 * pf + tf + 3.5 * pb = 2 * " + rightBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt + " + " + rightBeam.Shape.tf + " + 3.5 * " + rightBeam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir + " = " + bs + ConstUnit.Length);
							Alpham = 1.13 * Math.Pow(Pe / rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize, 0.25);
							Reporting.AddLine("am = 1.13 * (Pe / db)^0.25 = 1.13 * (" + Pe + " / " + rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ")^0.25 = " + Alpham);
						}
						else
						{
							bs = 2.5 * (2 * rightBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt + rightBeam.Shape.tf);
							Reporting.AddLine("bs = 2.5 * (2 * pf + tf) = 2.5 * (2 * " + rightBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt + " + " + rightBeam.Shape.tf + ") = " + bs + ConstUnit.Length);
							Alpham = 1.36 * Math.Pow(Pe / rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize, 0.25);
							Reporting.AddLine("am = 1.36 * (Pe / db)^0.25 = 1.36 * (" + Pe + " / " + rightBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ")^0.25 = " + Alpham);
						}
						fyc = column.Material.Fy;

						RFiRnFlBending = ConstNum.FIOMEGA0_9N * (bs / (Alpham * Pe)) * Math.Pow(column.Shape.tf, 2) * fyc * ct;

						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * (bs / (am * Pe)) * (tf²) * Fyc * ct");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * (" + bs + " / (" + Alpham + " * " + Pe + ")) * (" + column.Shape.tf + "²) * " + fyc + " * " + ct);
						RbsMin = rightBeam.Shape.bf / 3 - column.Shape.tw / 2;
						break;
					case EMomentCarriedBy.Tee:
						RtTop = rightBeam.WinConnect.MomentTee.TopTeeShape.tf;
						k1 = column.Shape.k1;
						Pe = rightBeam.WinConnect.MomentTee.Column_g / 2 - rightBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize / 4 - k1;
						Reporting.AddLine("Pe = g / 2 - d / 4 - k1");
						Reporting.AddLine("= " + rightBeam.WinConnect.MomentTee.Column_g + " / 2 - " + rightBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize + " / 4 - " + k1 + "");
						Reporting.AddLine("= " + Pe + ConstUnit.Length);
						if (rightBeam.WinConnect.MomentTee.BoltColumnFlange.NumberOfBolts == 8)
						{
							bs = 2 * rightBeam.WinConnect.MomentTee.Column_a + rightBeam.WinConnect.MomentTee.TopTeeShape.tf + 3.5 * rightBeam.WinConnect.MomentTee.Column_s;
							Reporting.AddLine("bs = 2 * pf + tf + 3.5 * pb");
							Reporting.AddLine("= 2 * " + rightBeam.WinConnect.MomentTee.Column_a + " + " + rightBeam.WinConnect.MomentTee.TopTeeShape.tf + " + 3.5 * " + rightBeam.WinConnect.MomentTee.Column_s);
							Reporting.AddLine("= " + bs + ConstUnit.Length);
							Alpham = 1.13 * Math.Pow(Pe / rightBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize, 0.25);
							Reporting.AddLine("am = 1.13 * (Pe / d)^0.25");
							Reporting.AddLine("= 1.13 * (" + Pe + " / " + rightBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize + ")^0.25");
							Reporting.AddLine("= " + Alpham);
						}
						else
						{
							bs = 2.5 * (2 * rightBeam.WinConnect.MomentTee.Column_a + rightBeam.WinConnect.MomentTee.TopTeeShape.tf);
							Reporting.AddLine("bs = 2.5 * (2 * pf + tf)");
							Reporting.AddLine("= 2.5 * (2 * " + rightBeam.WinConnect.MomentTee.Column_a + " + " + rightBeam.WinConnect.MomentTee.TopTeeShape.tf + ")");
							Reporting.AddLine("= " + bs + ConstUnit.Length);
							Alpham = 1.36 * Math.Pow(Pe / rightBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize, 0.25);
							Reporting.AddLine("am = 1.36 * (Pe / d)^0.25");
							Reporting.AddLine("= 1.36 * (" + Pe + " / " + rightBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize + ")^0.25");
							Reporting.AddLine("= " + Alpham);
						}
						fyc = column.Material.Fy;

						RFiRnFlBending = ConstNum.FIOMEGA0_9N * (bs / (Alpham * Pe)) * Math.Pow(column.Shape.tf, 2) * fyc * ct;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " *(bs / (am * Pe)) * (tf²) * Fyc * ct");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " *(" + bs + " / (" + Alpham + " * " + Pe + ")) * (" + column.Shape.tf + "²)*" + fyc + " * " + ct);

						RbsMin = rightBeam.Shape.bf / 3 - column.Shape.tw / 2;
						break;
					case EMomentCarriedBy.Angles:
						RtTop = rightBeam.WinConnect.MomentFlangeAngle.Angle.t;
						k1 = column.Shape.k1;
						Pe = rightBeam.GageOnColumn / 2 - rightBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize / 4 - k1;
						Reporting.AddLine("Pe = g/ 2 - d / 4 - k1");
						Reporting.AddLine("= " + rightBeam.GageOnColumn + " / 2 - " + rightBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize + " / 4 - " + k1 + " = " + Pe + ConstUnit.Length);
						if (rightBeam.WinConnect.MomentFlangeAngle.ColumnBolt.NumberOfBolts == 4)
						{
							bs = 2.5 * (rightBeam.GageOnColumn / 2 - column.Shape.k1) + rightBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize + 2 * rightBeam.GageOnColumn;
							Reporting.AddLine("bs = 2.5 * (g / 2 - k1) + d + 2 * g1");
							Reporting.AddLine("= 2.5 * (" + rightBeam.GageOnColumn + " / 2 - " + column.Shape.k1 + ") + " + rightBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize + " + 2 * " + rightBeam.GageOnColumn);
						}
						else
						{
							bs = 2.5 * (rightBeam.GageOnColumn / 2 - column.Shape.k1) + rightBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize;
							Reporting.AddLine("bs = 2.5 * (g / 2 - k1) + d");
							Reporting.AddLine("= 2.5 * (" + rightBeam.GageOnColumn + " / 2 - " + column.Shape.k1 + ") + " + rightBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize);
						}
						Reporting.AddLine("= " + bs + ConstUnit.Length);
						Alpham = 1.36 * Math.Pow(Pe / rightBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize, 0.25);
						Reporting.AddLine("am = 1.36 * (Pe / d)^0.25");
						Reporting.AddLine("= 1.36 * (" + Pe + " / " + rightBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize + ")^0.25 = " + Alpham);
						fyc = column.Material.Fy;

						RFiRnFlBending = ConstNum.FIOMEGA0_9N * (bs / (Alpham * Pe)) * Math.Pow(column.Shape.tf, 2) * fyc * ct;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * (bs / (am * Pe)) * (tf²) * Fyc * ct");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * (" + bs + " / (" + Alpham + " * " + Pe + ")) * (" + column.Shape.tf + "²)*" + fyc + " * " + ct);
						RbsMin = rightBeam.Shape.bf / 3 - column.Shape.tw / 2;
						break;
				}
				Reporting.AddLine("= " + RFiRnFlBending + ConstUnit.Force);

				Reporting.AddHeader("Local Web Yielding Strength, " + ConstString.PHI + " Rn");
				if (stiffener.TopOfBeamToColumn >= column.Shape.d || !stiffener.BeamNearTopOfColumn)
					ct = 1;
				else
					ct = 0.5;
				switch (rightBeam.MomentConnection)
				{
					case EMomentCarriedBy.FlangePlate:
						RFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * 5 * column.Shape.kdes + rightBeam.WinConnect.MomentFlangePlate.TopThickness + 2 * rightBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize) * column.Shape.tw * column.Material.Fy;
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (ct * 5 * k + t + 2 * w) * tw * Fy");
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (" + ct + " *5* " + column.Shape.kdes + " + " + rightBeam.WinConnect.MomentFlangePlate.TopThickness + " + 2 * " + rightBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize + ") * " + column.Shape.tw + " * " + column.Material.Fy);
						break;
					case EMomentCarriedBy.DirectlyWelded:
						RFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * 5 * column.Shape.kdes + rightBeam.Shape.tf) * column.Shape.tw * column.Material.Fy;
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (ct * 5 * k + t) * tw * Fy");
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (" + ct + " * 5 * " + column.Shape.kdes + " + " + rightBeam.Shape.tf + ") * " + column.Shape.tw + " * " + column.Material.Fy);
						break;
					case EMomentCarriedBy.EndPlate:
						RFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * (6 * column.Shape.kdes + 2 * rightBeam.WinConnect.MomentEndPlate.Thickness) + rightBeam.Shape.tf + 2 * rightBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize) * column.Shape.tw * column.Material.Fy;
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (ct * (6 * k + 2 * tp) + tf + 2 * w) * tw * Fy");
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (" + ct + " * (6 * " + column.Shape.kdes + " + 2 * " + rightBeam.WinConnect.MomentEndPlate.Thickness + ") + " + rightBeam.Shape.tf + " + 2 * " + rightBeam.WinConnect.MomentEndPlate.FlangeWeldSize + ") * " + column.Shape.tw + " * " + column.Material.Fy);
						break;
					case EMomentCarriedBy.Tee:
						RFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * (6 * column.Shape.kdes + 2 * rightBeam.WinConnect.MomentTee.TopTeeShape.tf) + rightBeam.WinConnect.MomentTee.TopTeeShape.tw) * column.Shape.tw * column.Material.Fy;
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (ct * (6 * k + 2 * tf) + tw) * twc * Fy");
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (" + ct + " * (6 * " + column.Shape.kdes + " + 2 * " + rightBeam.WinConnect.MomentTee.TopTeeShape.tf + ") + " + rightBeam.WinConnect.MomentTee.TopTeeShape.tw + ") * " + column.Shape.tw + " * " + column.Material.Fy);
						break;
					case EMomentCarriedBy.Angles:
						RFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * 5 * column.Shape.kdes + rightBeam.WinConnect.MomentFlangeAngle.Angle.t) * column.Shape.tw * column.Material.Fy;
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (ct * 5 * k + t) * twc * Fy");
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (" + ct + " * 5 * " + column.Shape.kdes + " + " + rightBeam.WinConnect.MomentFlangeAngle.Angle.t + ") * " + column.Shape.tw + " * " + column.Material.Fy);
						break;
				}
				Reporting.AddLine("= " + RFiRnWebYielding + ConstUnit.Force);

				// Web Crippling
				Reporting.AddHeader("Column Web Crippling:");
				switch (rightBeam.MomentConnection)
				{
					case EMomentCarriedBy.FlangePlate:
						RtBot = rightBeam.WinConnect.MomentFlangePlate.BottomThickness;
						n = rightBeam.WinConnect.MomentFlangePlate.BottomThickness + 2 * rightBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize;
						Reporting.AddLine("N = t + 2 * w = " + rightBeam.WinConnect.MomentFlangePlate.BottomThickness + " + 2 * " + rightBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize + " = " + n + ConstUnit.Length);
						break;
					case EMomentCarriedBy.DirectlyWelded:
						RtBot = rightBeam.Shape.tf;
						n = rightBeam.Shape.tf + 2 * rightBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize;
						Reporting.AddLine("N = tf = " + rightBeam.Shape.tf + ConstUnit.Length);
						break;
					case EMomentCarriedBy.EndPlate:
						RtBot = rightBeam.Shape.tf;
						n = rightBeam.Shape.tf + 2 * rightBeam.WinConnect.MomentEndPlate.FlangeWeldSize + 2 * rightBeam.WinConnect.MomentEndPlate.Thickness;
						Reporting.AddLine("N = tf + 2 * w + 2 * tp = " + rightBeam.Shape.tf + " + 2 * " + rightBeam.WinConnect.MomentEndPlate.FlangeWeldSize + " + 2 * " + rightBeam.WinConnect.MomentEndPlate.Thickness + " = " + n + ConstUnit.Length);
						break;
					case EMomentCarriedBy.Tee:
						RtBot = rightBeam.WinConnect.MomentTee.TopTeeShape.tf;
						n = rightBeam.WinConnect.MomentTee.TopTeeShape.tw + 2 * (rightBeam.WinConnect.MomentTee.TopTeeShape.kdes - rightBeam.WinConnect.MomentTee.TopTeeShape.tf) + 2 * rightBeam.WinConnect.MomentTee.TopTeeShape.tf;
						Reporting.AddLine("N = tw + 2 * f + 2 * tf");
						Reporting.AddLine("= " + rightBeam.WinConnect.MomentTee.TopTeeShape.tw + " + 2 * " + (rightBeam.WinConnect.MomentTee.TopTeeShape.kdes - rightBeam.WinConnect.MomentTee.TopTeeShape.tf) + " + 2 * " + rightBeam.WinConnect.MomentTee.TopTeeShape.tf + " = " + n + ConstUnit.Length);
						break;
					case EMomentCarriedBy.Angles:
						RtBot = rightBeam.WinConnect.MomentFlangeAngle.Angle.t;
						n = rightBeam.WinConnect.MomentFlangeAngle.Angle.kdet + rightBeam.WinConnect.MomentFlangeAngle.Angle.t;
						Reporting.AddLine("N = k + t");
						Reporting.AddLine("= " + rightBeam.WinConnect.MomentFlangeAngle.Angle.kdet + " + " + rightBeam.WinConnect.MomentFlangeAngle.Angle.t + " = " + n + ConstUnit.Length);
						break;
				}

				if (stiffener.TopOfBeamToColumn >= column.Shape.d / 2 || !stiffener.BeamNearTopOfColumn)
				{
					ct = 1;
					Reporting.AddLine("Ct = 1.0");
					if (n / column.Shape.d <= 0.2)
					{
						Nd = 3 * n / column.Shape.d;
						Reporting.AddLine("Nd = 3 * N / d = 3 * " + n + " / " + column.Shape.d + " = " + Nd);
					}
					else
					{
						Nd = 4 * n / column.Shape.d - 0.2;
						Reporting.AddLine("Nd = 4 * N / d - 0.2 = 4 * " + n + " / " + column.Shape.d + " - 0.2 = " + Nd);
					}
				}
				else
				{
					ct = 0.5;
					Reporting.AddLine("Ct = 0.5");
					Nd = 3 * n / column.Shape.d;
					Reporting.AddLine("Nd = 3 * N / d = 3 * " + n + " / " + column.Shape.d + " = " + Nd);
				}

				RFiRnWebCrippling = ConstNum.FIOMEGA0_75N * 0.8 * ct * Math.Pow(column.Shape.tw, 2) * (1 + Nd * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.8 * ct * (tw²) * [1 + Nd * (tw / tf)^1.5] * (E * Fy * tf / tw)^0.5");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.8 * " + ct + " * (" + column.Shape.tw + "²) * [1 + " + Nd + " * (" + column.Shape.tw + " / " + column.Shape.tf + ")^1.5]*(" + ConstNum.ELASTICITY + " * " + column.Material.Fy + " * " + column.Shape.tf + " / " + column.Shape.tw + ")^0.5");
				Reporting.AddLine("= " + RFiRnWebCrippling + ConstUnit.Force);

				if (rightBeam.MomentConnection != EMomentCarriedBy.NoMoment && leftBeam.MomentConnection != EMomentCarriedBy.NoMoment)
				{
					Reporting.AddHeader("Compression Buckling of the Web:");
					if (stiffener.TopOfBeamToColumn >= column.Shape.d / 2 || !stiffener.BeamNearTopOfColumn)
						ct = 1;
					else
						ct = 0.5;
					RFiRnWebBuckling = ConstNum.FIOMEGA0_9N * 24 * ct * Math.Pow(column.Shape.tw, 3) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy, 0.5) / (column.Shape.d - 2 * column.Shape.kdes);
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * 24 * ct * tw³ * (E * Fy)^0.5 / (d - 2 * k)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 24 * " + ct + " * " + column.Shape.tw + "³ * (" + ConstNum.ELASTICITY + " * " + column.Material.Fy + ")^0.5 / (" + column.Shape.d + " - 2 * " + column.Shape.kdes + ")");
					Reporting.AddLine("= " + RFiRnWebBuckling + ConstUnit.Force);
				}
			}

			if (leftBeam.MomentConnection != EMomentCarriedBy.NoMoment)
			{
				Reporting.AddHeader("Left Side Beam");
				Reporting.AddHeader("Local Flange Bending Strength, " + ConstString.PHI + " Rn");

				if (stiffener.TopOfBeamToColumn >= 10 * column.Shape.tf || !stiffener.BeamNearTopOfColumn)
					ct = 1;
				else
					ct = 0.5;
				switch (leftBeam.MomentConnection)
				{
					case EMomentCarriedBy.FlangePlate:
						LtTop = leftBeam.WinConnect.MomentFlangePlate.TopThickness;
						LFiRnFlBending = ConstNum.FIOMEGA0_9N * 6.25 * Math.Pow(column.Shape.tf, 2) * column.Material.Fy * ct;
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 6.25 * (tf²) * Fy * ct");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 6.25 * (" + column.Shape.tf + "²) * " + column.Material.Fy + " * " + ct);
						Lbsmin = Math.Max(leftBeam.WinConnect.MomentFlangePlate.TopWidth, leftBeam.WinConnect.MomentFlangePlate.BottomWidth) / 3 - column.Shape.tw / 2;
						break;
					case EMomentCarriedBy.DirectlyWelded:
						LtTop = leftBeam.Shape.tf;
						LFiRnFlBending = ConstNum.FIOMEGA0_9N * 6.25 * Math.Pow(column.Shape.tf, 2) * column.Material.Fy * ct;
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 6.25 * (tf²) * Fy * ct");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 6.25 * (" + column.Shape.tf + "²) * " + column.Material.Fy + " * " + ct);
						Lbsmin = leftBeam.Shape.bf / 3 - column.Shape.tw / 2;
						break;
					case EMomentCarriedBy.EndPlate:
						LtTop = leftBeam.Shape.tf;
						k1 = column.Shape.k1;
						Pe = leftBeam.GageOnFlange / 2 - leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize / 4 - k1;
						Reporting.AddLine("Pe = g / 2 - db / 4 - k1 = " + leftBeam.GageOnFlange + " / 2 - " + leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + "/4 - " + k1 + " = " + Pe + ConstUnit.Length);
						if (leftBeam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts == 8)
						{
							bs = 2 * leftBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt + leftBeam.Shape.tf + 3.5 * leftBeam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir;
							Reporting.AddLine("bs = 2 * pf + tf + 3.5 * pb = 2 * " + leftBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt + " + " + leftBeam.Shape.tf + " + 3.5 * " + leftBeam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir + " = " + bs + ConstUnit.Length);
							Alpham = 1.13 * Math.Pow(Pe / leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize, 0.25);
							Reporting.AddLine("am = 1.13 * (Pe / db)^0.25 = 1.13 * (" + Pe + " / " + leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ")^0.25 = " + Alpham);
						}
						else
						{
							bs = 2.5 * (2 * leftBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt + leftBeam.Shape.tf);
							Reporting.AddLine("bs = 2.5 * (2 * pf + tf) = 2.5 * (2 * " + leftBeam.WinConnect.MomentEndPlate.DistanceToFirstBolt + " + " + leftBeam.Shape.tf + ") = " + bs + ConstUnit.Length);
							Alpham = 1.36 * Math.Pow(Pe / leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize, 0.25);
							Reporting.AddLine("am = 1.36 * (Pe / db)^0.25 = 1.36 * (" + Pe + " / " + leftBeam.WinConnect.MomentEndPlate.Bolt.BoltSize + ")^0.25 = " + Alpham);
						}
						fyc = column.Material.Fy;
						LFiRnFlBending = ConstNum.FIOMEGA0_9N * (bs / (Alpham * Pe)) * Math.Pow(column.Shape.tf, 2) * fyc * ct;
						Reporting.AddLine("");
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * (bs / (am * Pe)) * (tf²) * Fyc * ct");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * (" + bs + " / (" + Alpham + " * " + Pe + ")) * (" + column.Shape.tf + "²) * " + fyc + " * " + ct);
						Lbsmin = leftBeam.Shape.bf / 3 - column.Shape.tw / 2;
						break;
					case EMomentCarriedBy.Tee:
						LtTop = leftBeam.WinConnect.MomentTee.TopTeeShape.tf;
						k1 = column.Shape.k1;
						Pe = leftBeam.WinConnect.MomentTee.Column_g / 2 - leftBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize / 4 - k1;
						Reporting.AddLine("Pe = g / 2 - d / 4 - k1");
						Reporting.AddLine("= " + leftBeam.WinConnect.MomentTee.Column_g + " / 2 - " + leftBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize + " / 4 - " + k1 + " = " + Pe + ConstUnit.Length);
						Reporting.AddLine("= " + Pe + ConstUnit.Length);
						if (leftBeam.WinConnect.MomentTee.BoltColumnFlange.NumberOfBolts == 8)
						{
							bs = 2 * leftBeam.WinConnect.MomentTee.Column_a + leftBeam.WinConnect.MomentTee.TopTeeShape.tf + 3.5 * leftBeam.WinConnect.MomentTee.Column_s;
							Reporting.AddLine("bs = 2 * pf + tf + 3.5 * pb");
							Reporting.AddLine("= 2 * " + leftBeam.WinConnect.MomentTee.Column_a + " + " + leftBeam.WinConnect.MomentTee.TopTeeShape.tf + " + 3.5 * " + leftBeam.WinConnect.MomentTee.Column_s);
							Reporting.AddLine("= " + bs + ConstUnit.Length);
							Alpham = 1.13 * Math.Pow(Pe / leftBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize, 0.25);
							Reporting.AddLine("am = 1.13 * (Pe / d)^0.25");
							Reporting.AddLine("= 1.13 * (" + Pe + " / " + leftBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize + ")^0.25");
							Reporting.AddLine("= " + Alpham);
						}
						else
						{
							bs = 2.5 * (2 * leftBeam.WinConnect.MomentTee.Column_a + leftBeam.WinConnect.MomentTee.TopTeeShape.tf);
							Reporting.AddLine("bs = 2.5 * (2 * pf + tf)");
							Reporting.AddLine("= 2.5 * (2 * " + leftBeam.WinConnect.MomentTee.Column_a + " + " + leftBeam.WinConnect.MomentTee.TopTeeShape.tf + ")");
							Reporting.AddLine("= " + bs + ConstUnit.Length);
							Alpham = 1.36 * Math.Pow(Pe / leftBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize, 0.25);
							Reporting.AddLine("am = 1.36 * (Pe / d)^0.25");
							Reporting.AddLine("= 1.36 * (" + Pe + " / " + leftBeam.WinConnect.MomentTee.BoltColumnFlange.BoltSize + ")^0.25");
							Reporting.AddLine("= " + Alpham);
						}
						fyc = column.Material.Fy;
						LFiRnFlBending = ConstNum.FIOMEGA0_9N * (bs / (Alpham * Pe)) * Math.Pow(column.Shape.tf, 2) * fyc * ct;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * (bs / (am * Pe)) * (tf²) * Fyc * ct");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * (" + bs + " / (" + Alpham + " * " + Pe + ")) * (" + column.Shape.tf + "²) * " + fyc + " * " + ct);
						Lbsmin = leftBeam.Shape.bf / 3 - column.Shape.tw / 2;
						break;
					case EMomentCarriedBy.Angles:
						LtTop = leftBeam.WinConnect.MomentFlangeAngle.Angle.t;
						k1 = column.Shape.k1;
						Pe = leftBeam.GageOnColumn / 2 - leftBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize / 4 - k1;
						Reporting.AddLine("Pe = g/ 2 - d / 4 - k1");
                        Reporting.AddLine("= " + leftBeam.GageOnColumn + " / 2 - " + leftBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize + " / 4 - " + k1 + " = " + Pe + ConstUnit.Length);
						if (leftBeam.WinConnect.MomentFlangeAngle.ColumnBolt.NumberOfBolts == 4)
						{
							bs = 2.5 * (leftBeam.WinConnect.MomentFlangeAngle.ColumnBoltSpacingOut / 2 - column.Shape.k1) + leftBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize + 2 * leftBeam.GageOnColumn;
							Reporting.AddLine("bs = 2.5 * (g / 2 - k1) + d + 2 * g1");
							Reporting.AddLine("= 2.5 * (" + leftBeam.WinConnect.MomentFlangeAngle.ColumnBoltSpacingOut + " / 2 - " + column.Shape.k1 + ") + " + leftBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize + " + 2 * " + leftBeam.GageOnColumn);
						}
						else
						{
							bs = 2.5 * (leftBeam.GageOnColumn / 2 - column.Shape.k1) + leftBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize;
							Reporting.AddLine("bs = 2.5 * (g / 2 - k1) + d");
							Reporting.AddLine("= 2.5 * (" + leftBeam.GageOnColumn + " / 2 - " + column.Shape.k1 + ") + " + leftBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize);
						}
						Reporting.AddLine("= " + bs + ConstUnit.Length);
						Alpham = 1.36 * Math.Pow(Pe / leftBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize, 0.25);
						Reporting.AddLine("am = 1.36 * (Pe / d)^0.25");
						Reporting.AddLine("= 1.36 * (" + Pe + " / " + leftBeam.WinConnect.MomentFlangeAngle.ColumnBolt.BoltSize + ")^0.25 = " + Alpham);
						fyc = column.Material.Fy;

						LFiRnFlBending = ConstNum.FIOMEGA0_9N * (bs / (Alpham * Pe)) * Math.Pow(column.Shape.tf, 2) * fyc * ct;
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * (bs / (am * Pe)) * (tf²) * Fyc * ct");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * (" + bs + " / (" + Alpham + " * " + Pe + ")) * (" + column.Shape.tf + "²) * " + fyc + " * " + ct);
						Lbsmin = leftBeam.Shape.bf / 3 - column.Shape.tw / 2;
						break;
				}
				Reporting.AddLine("= " + LFiRnFlBending + ConstUnit.Force);

				Reporting.AddHeader("Local Web Yielding Strength," + ConstString.PHI + " Rn");
				if (stiffener.TopOfBeamToColumn >= column.Shape.d || !stiffener.BeamNearTopOfColumn)
					ct = 1;
				else
					ct = 0.5;
				switch (leftBeam.MomentConnection)
				{
					case EMomentCarriedBy.FlangePlate:
						LFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * 5 * column.Shape.kdes + leftBeam.WinConnect.MomentFlangePlate.TopThickness + 2 * leftBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize) * column.Shape.tw * column.Material.Fy;
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (ct * 5 * k + t + 2 * w) * tw * Fy");
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (" + ct + " * 5 * " + column.Shape.kdes + " + " + leftBeam.WinConnect.MomentFlangePlate.TopThickness + " + 2 * " + leftBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize + ") * " + column.Shape.tw + " * " + column.Material.Fy);
						break;
					case EMomentCarriedBy.DirectlyWelded:
						LFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * 5 * column.Shape.kdes + leftBeam.Shape.tf) * column.Shape.tw * column.Material.Fy;
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (ct * 5 * k + t) * tw * Fy");
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (" + ct + " * 5 * " + column.Shape.kdes + " + " + leftBeam.Shape.tf + ") * " + column.Shape.tw + " * " + column.Material.Fy);
						break;
					case EMomentCarriedBy.EndPlate:
						LFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * (6 * column.Shape.kdes + 2 * leftBeam.WinConnect.MomentEndPlate.Thickness) + leftBeam.Shape.tf + 2 * leftBeam.WinConnect.MomentEndPlate.FlangeWeldSize) * column.Shape.tw * column.Material.Fy;
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (ct * (6 * k + 2 * tp) + tf + 2 * w) * tw * Fy");
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (" + ct + " * (6 * " + column.Shape.kdes + " + 2 * " + leftBeam.WinConnect.MomentEndPlate.Thickness + ") + " + leftBeam.Shape.tf + " + 2 * " + leftBeam.WinConnect.MomentEndPlate.FlangeWeldSize + ") * " + column.Shape.tw + " * " + column.Material.Fy);
						break;
					case EMomentCarriedBy.Tee:
						LFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * (6 * column.Shape.kdes + 2 * leftBeam.WinConnect.MomentTee.TopTeeShape.tf) + leftBeam.WinConnect.MomentTee.TopTeeShape.tw) * column.Shape.tw * column.Material.Fy;
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (ct * (6 * k + 2 * tf) + tw) * twc * Fy");
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (" + ct + " * (6 * " + column.Shape.kdes + " + 2 * " + leftBeam.WinConnect.MomentTee.TopTeeShape.tf + ") + " + leftBeam.WinConnect.MomentTee.TopTeeShape.tw + ") * " + column.Shape.tw + " * " + column.Material.Fy);
						break;
					case EMomentCarriedBy.Angles:
						LFiRnWebYielding = ConstNum.FIOMEGA1_0N * (ct * 5 * column.Shape.kdes + leftBeam.WinConnect.MomentFlangeAngle.Angle.t) * column.Shape.tw * column.Material.Fy;
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (ct * 5 * k + t) * twc * Fy");
						Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (" + ct + " * 5 * " + column.Shape.kdes + " + " + leftBeam.WinConnect.MomentFlangeAngle.Angle.t + ") * " + column.Shape.tw + " * " + column.Material.Fy);
						break;
				}
				Reporting.AddLine("= " + LFiRnWebYielding + ConstUnit.Force);

				Reporting.AddHeader("Column Web Crippling:");
				switch (leftBeam.MomentConnection)
				{
					case EMomentCarriedBy.FlangePlate:
						LtBot = leftBeam.WinConnect.MomentFlangePlate.BottomThickness;
						n = leftBeam.WinConnect.MomentFlangePlate.BottomThickness + 2 * leftBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize;
						Reporting.AddLine("N = t + 2 * minimumWeld = " + leftBeam.WinConnect.MomentFlangePlate.BottomThickness + " + 2 * " + leftBeam.WinConnect.MomentFlangePlate.TopPlateToSupportWeldSize + " = " + n + ConstUnit.Length);
						break;
					case EMomentCarriedBy.DirectlyWelded:
						LtBot = leftBeam.Shape.tf;
						n = leftBeam.Shape.tf;
						Reporting.AddLine("N = tf = " + leftBeam.Shape.tf + ConstUnit.Length);
						break;
					case EMomentCarriedBy.EndPlate:
						LtBot = leftBeam.Shape.tf;
						n = leftBeam.Shape.tf + 2 * leftBeam.WinConnect.MomentEndPlate.FlangeWeldSize + 2 * leftBeam.WinConnect.MomentEndPlate.Thickness;
						Reporting.AddLine("N = tf + 2 * minimumWeld + 2 * tp = " + leftBeam.Shape.tf + " + 2 * " + leftBeam.WinConnect.MomentEndPlate.FlangeWeldSize + " + 2 * " + leftBeam.WinConnect.MomentEndPlate.Thickness + " = " + n + ConstUnit.Length);
						break;
					case EMomentCarriedBy.Tee:
						LtBot = leftBeam.WinConnect.MomentTee.TopTeeShape.tf;
						n = leftBeam.WinConnect.MomentTee.TopTeeShape.tw + 2 * (leftBeam.WinConnect.MomentTee.TopTeeShape.kdes - leftBeam.WinConnect.MomentTee.TopTeeShape.tf) + 2 * leftBeam.WinConnect.MomentTee.TopTeeShape.tf;
						Reporting.AddLine("N = tw + 2 * f + 2 * tf");
						Reporting.AddLine("= " + leftBeam.WinConnect.MomentTee.TopTeeShape.tw + " + 2 * " + (leftBeam.WinConnect.MomentTee.TopTeeShape.kdes - leftBeam.WinConnect.MomentTee.TopTeeShape.tf) + " + 2 * " + leftBeam.WinConnect.MomentTee.TopTeeShape.tf + " = " + n + ConstUnit.Length);
						break;
					case EMomentCarriedBy.Angles:
						LtBot = leftBeam.WinConnect.MomentFlangeAngle.Angle.t;
						n = leftBeam.WinConnect.MomentFlangeAngle.Angle.kdet + leftBeam.WinConnect.MomentFlangeAngle.Angle.t;
						Reporting.AddLine("N = k + t = " + leftBeam.WinConnect.MomentFlangeAngle.Angle.kdet + " + " + leftBeam.WinConnect.MomentFlangeAngle.Angle.t + " = " + n + ConstUnit.Length);
						break;
				}
				if (stiffener.TopOfBeamToColumn >= column.Shape.d / 2 || !stiffener.BeamNearTopOfColumn)
				{
					ct = 1;
					Reporting.AddLine("Ct = 1.0");
					if (n / column.Shape.d <= 0.2)
					{
						Nd = 3 * n / column.Shape.d;
						Reporting.AddLine("Nd = 3 * N / d = 3 * " + n + " / " + column.Shape.d + " = " + Nd);
					}
					else
					{
						Nd = 4 * n / column.Shape.d - 0.2;
						Reporting.AddLine("Nd = 4 * N / d - 0.2 = 4 * " + n + " / " + column.Shape.d + " - 0.2 = " + Nd);
					}
				}
				else
				{
					ct = 0.5;
					Reporting.AddLine("Ct = 0.5");
					Nd = 3 * n / column.Shape.d;
					Reporting.AddLine("Nd = 3 * N / d = 3 * " + n + " / " + column.Shape.d + " = " + Nd);
				}
				LFiRnWebCrippling = ConstNum.FIOMEGA0_75N * 0.8 * ct * Math.Pow(column.Shape.tw, 2) * (1 + Nd * Math.Pow(column.Shape.tw / column.Shape.tf, 1.5)) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy * column.Shape.tf / column.Shape.tw, 0.5);
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.8 * ct * (tw²) * [1 + Nd * (tw / tf)^1.5] * (E * Fy * tf / tw)^0.5");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.8 * " + ct + " * (" + column.Shape.tw + "²) * [1 + " + Nd + " * (" + ConstNum.ELASTICITY + " * " + column.Shape.tw + " / " + column.Shape.tf + ")^1.5] * (" + column.Material.Fy + " * " + column.Shape.tf + " / " + column.Shape.tw + ")^0.5");
				Reporting.AddLine("= " + LFiRnWebCrippling + ConstUnit.Force);

				// Directly Welded, Flange Plate checks added in Descon 8 (4/24/15 - Mike)
				if (leftBeam.MomentConnection != EMomentCarriedBy.NoMoment && rightBeam.MomentConnection != EMomentCarriedBy.NoMoment &&
					leftBeam.MomentConnection != EMomentCarriedBy.DirectlyWelded &&
					leftBeam.MomentConnection != EMomentCarriedBy.FlangePlate)
				{
					Reporting.AddHeader("Compression Buckling of the Web:");
					if (stiffener.TopOfBeamToColumn >= column.Shape.d / 2 || !stiffener.BeamNearTopOfColumn)
						ct = 1;
					else
						ct = 0.5;
					LFiRnWebBuckling = ConstNum.FIOMEGA0_9N * 24 * ct * Math.Pow(column.Shape.tw, 3) * Math.Pow(ConstNum.ELASTICITY * column.Material.Fy, 0.5) / (column.Shape.d - 2 * column.Shape.kdes);
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * 24 * ct * tw³ * (E * Fy)^0.5 / (d - 2 * k)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 24 * " + ct + " * " + column.Shape.tw + "³ * (" + ConstNum.ELASTICITY + " * " + column.Material.Fy + ")^0.5 / (" + column.Shape.d + " - 2 * " + column.Shape.kdes + ")");
					Reporting.AddLine("= " + LFiRnWebBuckling + ConstUnit.Force);
				}
			}

			Reporting.AddHeader("Tension Flange Stiffener Force (TFrc):");
			if (leftBeam.IsActive)
			{
				stiffener.LTenstifForce = Math.Max(stiffener.LPuf - LFiRnFlBending, stiffener.LPuf - LFiRnWebYielding);
				if (stiffener.LTenstifForce < 0)
				{
					stiffener.LTenstifForce = 0;
				}
				Reporting.AddHeader("Left Side:");
				Reporting.AddLine("LTFrc = Max(LPuf - L" + ConstString.PHI + " Rn_FlBending; LPuf - L " + ConstString.PHI + " Rn_WebYielding) >= 0");
				Reporting.AddLine("= Max(" + stiffener.LPuf + " - " + LFiRnFlBending + "; " + stiffener.LPuf + " - " + LFiRnWebYielding + ") = " + stiffener.LTenstifForce + ConstUnit.Force);
			}

			if (rightBeam.IsActive)
			{
				stiffener.RTenstifForce = Math.Max(stiffener.RPuf - RFiRnFlBending, stiffener.RPuf - RFiRnWebYielding);
				if (stiffener.RTenstifForce < 0)
					stiffener.RTenstifForce = 0;

				Reporting.AddHeader("Right Side:");
				Reporting.AddLine("RTFrc = Max(RPuf - R" + ConstString.PHI + " Rn_FlBending; RPuf - R " + ConstString.PHI + " Rn_WebYielding) >= 0");
				Reporting.AddLine("= Max(" + stiffener.RPuf + " - " + RFiRnFlBending + "; " + stiffener.RPuf + " - " + RFiRnWebYielding + ") = " + stiffener.RTenstifForce + ConstUnit.Force);
			}

			Reporting.AddHeader("Compression Flange Stiffener Force (CFrc):");

			if (rightBeam.IsActive && (rightBeam.MomentConnection != EMomentCarriedBy.NoMoment))
				stiffener.RCompStifForce = Math.Max(stiffener.RCompStifForce, stiffener.RPuf - RFiRnWebBuckling);

			if (leftBeam.IsActive && (leftBeam.MomentConnection != EMomentCarriedBy.NoMoment))
				stiffener.LCompStifForce = Math.Max(stiffener.LCompStifForce, stiffener.LPuf - LFiRnWebBuckling);

			if (stiffener.LCompStifForce < 0)
				stiffener.LCompStifForce = 0;
			if (stiffener.RCompStifForce < 0)
				stiffener.RCompStifForce = 0;

			double stiffForceValueLeft = 0;
			double stiffForceValueRight = 0;

			if (leftBeam.IsActive)
			{
				Reporting.AddHeader("Left Side:");
				if (rightBeam.MomentConnection != EMomentCarriedBy.NoMoment && (leftBeam.MomentConnection != EMomentCarriedBy.NoMoment))
				{
					stiffener.LCompStifForce = Math.Max(stiffener.LPuf - LFiRnWebCrippling, Math.Max(stiffener.LPuf - LFiRnWebYielding, stiffener.LPuf - LFiRnWebBuckling));
					if (stiffener.LCompStifForce > 0)
						stiffForceValueLeft = stiffener.LCompStifForce;
					
					Reporting.AddLine("LCFrc = Max[(LPuf - L" + ConstString.PHI + " Rn_WebCrippling), (LPuf - L " + ConstString.PHI + " Rn_WebYielding); (LPuf - L" + ConstString.PHI + " Rn_WebBuckling)] >= 0");
					Reporting.AddLine("= Max[(" + stiffener.LPuf + " - " + LFiRnWebCrippling + "), (" + stiffener.LPuf + " - " + LFiRnWebYielding + "), (" + stiffener.LPuf + " - " + LFiRnWebBuckling + ")] = " + stiffForceValueLeft + ConstUnit.Force);
				}
				else
				{
					stiffener.LCompStifForce = Math.Max(stiffener.LPuf - LFiRnWebCrippling, stiffener.LPuf - LFiRnWebYielding);
					if (stiffener.LCompStifForce > 0)
						stiffForceValueLeft = stiffener.LCompStifForce;
					
					Reporting.AddLine("LCFrc = Max[(LPuf - L" + ConstString.PHI + " Rn_WebCrippling), (LPuf - L " + ConstString.PHI + " Rn_WebYielding)] >= 0");
					Reporting.AddLine("= Max[(" + stiffener.LPuf + " - " + LFiRnWebCrippling + "), (" + stiffener.LPuf + " - " + LFiRnWebYielding + ")] = " + stiffForceValueLeft + ConstUnit.Force);
				}
			}

			if (rightBeam.IsActive)
			{
				Reporting.AddHeader("Right Side:");
				if (rightBeam.MomentConnection != EMomentCarriedBy.NoMoment && (leftBeam.MomentConnection != EMomentCarriedBy.NoMoment))
				{
					stiffener.RCompStifForce = Math.Max(stiffener.RPuf - RFiRnWebCrippling, Math.Max(stiffener.RPuf - RFiRnWebYielding, stiffener.RPuf - RFiRnWebBuckling));
					if (stiffener.RCompStifForce > 0)
						stiffForceValueRight = stiffener.RCompStifForce;
					
					Reporting.AddLine("RCFrc = Max[(RPuf - R" + ConstString.PHI + " Rn_WebCrippling), (RPuf - R " + ConstString.PHI + " Rn_WebYielding); (RPuf-R" + ConstString.PHI + " Rn_WebBuckling)] >= 0");
					Reporting.AddLine("= Max[(" + stiffener.RPuf + " - " + RFiRnWebCrippling + "), (" + stiffener.RPuf + " - " + RFiRnWebYielding + ");,(" + stiffener.RPuf + " - " + RFiRnWebBuckling + ")] = " + stiffForceValueRight + ConstUnit.Force);
				}
				else
				{
					stiffener.RCompStifForce = Math.Max(stiffener.RPuf - RFiRnWebCrippling, stiffener.RPuf - RFiRnWebYielding);
					if (stiffener.RCompStifForce > 0)
						stiffForceValueRight = stiffener.RCompStifForce;
					
					Reporting.AddLine("RCFrc = Max[(RPuf - R" + ConstString.PHI + " Rn_WebCrippling), (RPuf - R " + ConstString.PHI + " Rn_WebYielding)] >= 0");
					Reporting.AddLine("= Max[(" + stiffener.RPuf + " - " + RFiRnWebCrippling + "), (" + stiffener.RPuf + " - " + RFiRnWebYielding + ")] = " + stiffForceValueRight + ConstUnit.Force);
				}
			}

			TenstifForce = Math.Max(stiffener.LTenstifForce, stiffener.RTenstifForce);
			CompStifForce = Math.Max(stiffener.LCompStifForce, stiffener.RCompStifForce);

			Reporting.AddLine(string.Empty);
			Reporting.AddLine("TFrc = Max(LTFrc, RTFrc) = Max(" + stiffener.LTenstifForce + ", " + stiffener.RTenstifForce + ") = " + TenstifForce + ConstUnit.Force);
			Reporting.AddLine("CFrc = Max(LCFrc, RCFrc) = Max(" + stiffForceValueLeft + ", " + stiffForceValueRight + ") = " + CompStifForce + ConstUnit.Force);

			Reporting.AddLine(string.Empty);
			if (TenstifForce > 0 || CommonDataStatic.SeismicSettings.Type == ESeismicType.High)
			{
				stiffener.TenStiff = true;
				Reporting.AddLine("TFrc >> 0 or High Seismic Loading");
				Reporting.AddLine("Stiffeners required opposite tension flange");
			}
			else
			{
				stiffener.TenStiff = false;
				Reporting.AddLine("Stiffeners not required opposite tension flange.");
			}
			if (CompStifForce > 0 || CommonDataStatic.SeismicSettings.Type == ESeismicType.High)
			{
				stiffener.CompStiff = true;
				Reporting.AddLine("CFrc >> 0 or High Seismic Loading");
				Reporting.AddLine("Stiffeners required opposite compression flange.");
			}
			else
			{
				stiffener.CompStiff = false;
				Reporting.AddLine("Stiffeners not required opposite compression flange.");
			}

			if (stiffener.TenStiff || stiffener.CompStiff)
			{
				Reporting.AddHeader("Required stiffener area for strength:");
				AstMinTen = TenstifForce / (ConstNum.FIOMEGA0_9N * stiffener.Material.Fy);
				AstMinComp = CompStifForce / (ConstNum.FIOMEGA0_9N * stiffener.Material.Fy);
				ast = Math.Max(AstMinTen, AstMinComp);
				Reporting.AddLine("Tension and/or compresion:");
				Reporting.AddLine("Ast = Max(TFrc, CFrc) / (" + ConstString.FIOMEGA0_9 + " * Fy)");
				Reporting.AddLine("= Max(" + TenstifForce + ", " + CompStifForce + ") / (" + ConstString.FIOMEGA0_9 + " * " + stiffener.Material.Fy + ")");
				Reporting.AddLine("= Max(" + AstMinTen + ", " + AstMinComp + ") " + ConstUnit.Area);

				tTop = Math.Max(LtTop, RtTop);
				tBot = Math.Max(LtBot, RtBot);
				// ColStiffener.Width = ast / Min(tTop, tBot)

				if (!stiffener.SWidth_User && stiffener.UseExtendedDoublerPlate)
				{
					switch (stiffener.DNumberOfPlates)
					{
						case 2:
						case 1:
							maxwidth = (column.Shape.bf - column.Shape.tw) / 2 - stiffener.DThickness;
							stiffener.SWidth = Math.Min(stiffener.SWidth, maxwidth);
							break;
						case 0:
							maxwidth = (column.Shape.bf - column.Shape.tw) / 2;
							stiffener.SWidth = Math.Min(stiffener.SWidth, maxwidth);
							break;
					}
				}

				if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
					tsmin = Math.Max(tTop / 2, stiffener.SWidth * Math.Pow(stiffener.Material.Fy / ConstNum.ELASTICITY, 0.5) / 0.56);
				else
					tsmin = Math.Max(tTop, stiffener.SWidth * Math.Pow(stiffener.Material.Fy / ConstNum.ELASTICITY, 0.5) / 0.56);

				if (!stiffener.SThickness_User)
				{
					while (stiffener.SThickness < tsmin)
						stiffener.SThickness += ConstNum.SIXTEENTH_INCH;
				}

				if (!stiffener.SWidth_User)
					stiffener.SWidth = NumberFun.Round(stiffener.SWidth, 4);

				if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
					MinWidth = Math.Max(RbsMin, Lbsmin);
				else if (CommonDataStatic.SeismicSettings.Type == ESeismicType.High)
					MinWidth = (column.Shape.bf - column.Shape.tw) / 2;

				if (stiffener.UseExtendedDoublerPlate && stiffener.DThickness2 > 0)
					MinWidth = stiffener.SWidth - stiffener.DThickness;

				if (stiffener.SWidth >= MinWidth)
					Reporting.AddLine("Stiffener Width (bs) = " + stiffener.SWidth + " >= Minimum Width = " + MinWidth + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Stiffener Width (bs) = " + stiffener.SWidth + " << Minimum Width = " + MinWidth + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Stiffener Length:");
				if (stiffener.StiffenerLength == EStiffenerLength.PartialLengthOfWeb)
				{
					Reporting.AddHeader("Using Partial Length Stiffeners");
					switch (stiffener.DNumberOfPlates)
					{
						case 0:
							Reporting.AddHeader("Minimum Length to Develop Stiffener Force in Column Web:");
							lngth = Math.Max(TenstifForce, CompStifForce) / (ConstNum.FIOMEGA0_9N * 0.6 * column.Shape.tw * column.Material.Fy) + (column.Shape.kdet - column.Shape.tf);
							Reporting.AddLine("MinLength = Max(TFrc, CFrc) / (" + ConstString.FIOMEGA0_9 + " * 0.6 * tw * Fy) + (k - tf)");
							Reporting.AddLine("= Max(" + TenstifForce + "; " + CompStifForce + ") / (" + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Shape.tw + " * " + column.Material.Fy + ") + (" + column.Shape.kdet + " - " + column.Shape.tf + ")");
							break;
						case 1:
							if (stiffener.UseExtendedDoublerPlate)
							{
								Reporting.AddHeader("Minimum Length to Develop Stiffener Force in Column Web:");
								lngthC = Math.Max(TenstifForce, CompStifForce) / (2 * ConstNum.FIOMEGA0_9N * 0.6 * column.Shape.tw * column.Material.Fy) + (column.Shape.kdet - column.Shape.tf);
								Reporting.AddLine("MinLengthC = Max(TFrc, CFrc) / (2 * " + ConstString.FIOMEGA0_9 + " * 0.6 * tw * Fy) + (k - tf)");
								Reporting.AddLine("= Max(" + TenstifForce + ", " + CompStifForce + ") / (2 * " + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Shape.tw + " * " + column.Material.Fy + ") + (" + column.Shape.kdet + " - " + column.Shape.tf + ")");
								Reporting.AddLine("= " + lngthC + ConstUnit.Length);
								Reporting.AddLine("Minimum Length to Develop Stiffener Force in Doubler Plate:");
								lngthP = Math.Max(TenstifForce, CompStifForce) / (2 * ConstNum.FIOMEGA0_9N * 0.6 * stiffener.DThickness * stiffener.Material.Fy) + (column.Shape.kdet - column.Shape.tf);

								Reporting.AddLine("MinLengthP = Max(TFrc, CFrc) / (2 * " + ConstString.FIOMEGA0_9 + " * 0.6 * tp * Fy) + (k - tf)");
								Reporting.AddLine("= Max(" + TenstifForce + ", " + CompStifForce + ") / (2 * " + ConstString.FIOMEGA0_9 + " * 0.6 * " + stiffener.DThickness + " * " + stiffener.Material.Fy + ") + (" + column.Shape.kdet + " - " + column.Shape.tf + ")");
								Reporting.AddLine("= " + lngthP + ConstUnit.Length);
								lngth = Math.Max(lngthC, lngthP);
								Reporting.AddLine("Length = Max(lngthC, lngthP) = Max(" + lngthC + ", " + lngthP + ")");
							}
							else
							{
								Reporting.AddHeader("Minimum Length to Develop Stiffener Force in Column Web:");
								lngth = Math.Max(TenstifForce, CompStifForce) / (ConstNum.FIOMEGA0_9N * 0.6 * column.Shape.tw * column.Material.Fy) + (column.Shape.kdet - column.Shape.tf);
								Reporting.AddLine("MinLength = Max(TFrc, CFrc) / (" + ConstString.FIOMEGA0_9 + " * 0.6 * tw * Fy) + (k - tf)");
								Reporting.AddLine("= Max(" + TenstifForce + "; " + CompStifForce + ") / (" + ConstString.FIOMEGA0_9 + " * 0.6 *" + column.Shape.tw + " * " + column.Material.Fy + ") + (" + column.Shape.kdet + " - " + column.Shape.tf + ")");
							}
							break;
						case 2:
							Reporting.AddHeader("Minimum Length to Develop Stiffener Force in Doubler Plates:");
							lngth = Math.Max(TenstifForce, CompStifForce) / (2 * ConstNum.FIOMEGA0_9N * 0.6 * stiffener.DThickness * stiffener.Material.Fy) + (column.Shape.kdet - column.Shape.tf);
							Reporting.AddLine("MinLength = Max(TFrc, CFrc) / (2 * " + ConstString.FIOMEGA0_9 + " * 0.6 * tp * Fy) + (k - tf)");
							Reporting.AddLine("= Max(" + TenstifForce + ", " + CompStifForce + ") / (2 * " + ConstString.FIOMEGA0_9 + " * 0.6 * " + stiffener.DThickness + " * " + stiffener.Material.Fy + ") + (" + column.Shape.kdet + " - " + column.Shape.tf + ")");
							break;
					}
					if (lngth <= stiffener.SLength)
						Reporting.AddLine("= " + lngth + " <= " + stiffener.SLength + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + lngth + " >> " + stiffener.SLength + ConstUnit.Length + " (NG)");

					if (column.Shape.d / 2 - column.Shape.tf <= stiffener.SLength)
						Reporting.AddLine("Min. Length = dc/2 - tf = " + (column.Shape.d / 2) + " - " + column.Shape.tf + " <= " + stiffener.SLength + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Min. Length = dc/2 - tf = " + (column.Shape.d / 2) + " - " + column.Shape.tf + " >> " + stiffener.SLength + ConstUnit.Length + " (NG)");

					fl = column.Shape.kdet - column.Shape.tf;
					tTensionStiff = TenstifForce / (ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * (stiffener.SLength - fl) * 2);

					Reporting.AddHeader("Stiffener thickness required for shear:");
					Reporting.AddLine("= Max(TFrc, CFrc) / (" + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * (L - clip) * 2)");
					Reporting.AddLine("= Max(" + TenstifForce + ";" + CompStifForce + ") / (" + ConstString.FIOMEGA0_9 + " * 0.6 * " + stiffener.Material.Fy + " * (" + stiffener.SLength + " - " + fl + ") * 2)");
				}
				else
				{
					stiffener.SLength = NumberFun.Round(column.Shape.d - 2 * column.Shape.tf, 16);
					Reporting.AddLine("L = d - 2 * tf = " + column.Shape.d + " - 2 * " + column.Shape.tf + " = " + stiffener.SLength + ConstUnit.Length);
					Reporting.AddLine("(Using Full Length Stiffeners)");
					fl = 2 * (column.Shape.kdet - column.Shape.tf);
					tTensionStiff = (stiffener.LTenstifForce + stiffener.RTenstifForce) / (ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * (stiffener.SLength - fl) * 2);

					Reporting.AddHeader("Stiffener thickness required for shear:");
					Reporting.AddLine("= Max([LTFrc + RCFrc], [LCFrc + RTFrc]) / (" + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * (L - 2 * clip) * 2)");
					Reporting.AddLine("= Max([" + stiffener.LTenstifForce + "+" + stiffener.RCompStifForce + "]; [" + stiffener.LCompStifForce + "+" + stiffener.RTenstifForce + "]) / (" + ConstString.FIOMEGA0_9 + " * 0.6 * " + stiffener.Material.Fy + " * (" + stiffener.SLength + " - " + fl + ") * 2)");
				}
				
				if (tTensionStiff <= stiffener.SThickness)
					Reporting.AddCapacityLine("= " + tTensionStiff + " <= " + stiffener.SThickness + ConstUnit.Length + " (OK)", tTensionStiff / stiffener.SThickness, "Stiffener thickness required for shear", EMemberType.PrimaryMember);
				else
					Reporting.AddCapacityLine("= " + tTensionStiff + " >> " + stiffener.SThickness + ConstUnit.Length + " (NG)", tTensionStiff / stiffener.SThickness, "Stiffener thickness required for shear", EMemberType.PrimaryMember);
				
				clip = column.Shape.kdet - column.Shape.tf;
				tTensionStiff = ast / (2 * (stiffener.SWidth - clip));

				Reporting.AddHeader("Stiffener thickness required for minimum area:");
				Reporting.AddLine("= Ast / (2 * (bs - clip))");
				if (tTensionStiff <= stiffener.SThickness)
					Reporting.AddCapacityLine("= " + ast + " / (2 * (" + stiffener.SWidth + " - " + clip + ")) = " + tTensionStiff + " <= " + stiffener.SThickness + ConstUnit.Length + " (OK)", tTensionStiff / stiffener.SThickness, "Stiffener thickness required for minimum area", EMemberType.PrimaryMember);
				else
					Reporting.AddCapacityLine("= " + ast + " / (2 * (" + stiffener.SWidth + " - " + clip + ")) = " + tTensionStiff + " >> " + stiffener.SThickness + ConstUnit.Length + " (NG)", tTensionStiff / stiffener.SThickness, "Stiffener thickness required for minimum area", EMemberType.PrimaryMember);

				if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
				{
					tsmin = Math.Max(tTop / 2, stiffener.SWidth * Math.Pow(stiffener.Material.Fy / ConstNum.ELASTICITY, 0.5) / 0.56);
					Reporting.AddLine(string.Empty);
					Reporting.AddLine("Minimum Thickness = Max(tm / 2, bs * (Fy / E)^0.5 / 0.56)");
					Reporting.AddLine("= Max(" + tTop + " / 2, " + stiffener.SWidth + " * (" + stiffener.Material.Fy + " / " + ConstNum.ELASTICITY + ")^0.5 / 0.56)");
				}
				else if (CommonDataStatic.SeismicSettings.Type == ESeismicType.High)
				{
					tsmin = Math.Max(tTop, stiffener.SWidth * Math.Pow(stiffener.Material.Fy / ConstNum.ELASTICITY, 0.5) / 0.56);
					Reporting.AddLine("Minimum Thickness = Max(tm; bs * (Fy / E)^0.5 / 0.56)");
					Reporting.AddLine("= Max(" + tTop + ", " + stiffener.SWidth + " * (" + stiffener.Material.Fy + " / " + ConstNum.ELASTICITY + ")^0.5 / 0.56)");
				}

				if (tsmin <= stiffener.SThickness)
					Reporting.AddLine("= " + tsmin + " <= " + stiffener.SThickness + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + tsmin + " >> " + stiffener.SThickness + ConstUnit.Length + " (NG)");
			}
		}

		private static double MinimumWeldForStiff(double t1, double t2)
		{
			double minimumWeld = 0;
			double tmpCaseArg;

			switch (CommonDataStatic.Units)
			{
				case EUnit.US:
					tmpCaseArg = Math.Min(t1, t2);
					if (tmpCaseArg <= 0.25)
						minimumWeld = 0.125;
					else if (tmpCaseArg <= 0.5)
						minimumWeld = 0.1875;
					else if (tmpCaseArg <= 0.75)
						minimumWeld = 0.25;
					else
						minimumWeld = 0.3125;
					if (minimumWeld > t1)
						minimumWeld = t1;
					if (minimumWeld > t2)
						minimumWeld = t2;
					break;
				case EUnit.Metric:
					tmpCaseArg = Math.Min(t1, t2);
					if (tmpCaseArg <= 6)
						minimumWeld = 3;
					else if (tmpCaseArg <= 13)
						minimumWeld = 5;
					else if (tmpCaseArg <= 19)
						minimumWeld = 6;
					else
						minimumWeld = 8;
					if (minimumWeld > t1)
						minimumWeld = t1;
					if (minimumWeld > t2)
						minimumWeld = t2;
					break;
			}

			return Math.Max(minimumWeld, 0.1875);
		}

		public static void ColumnLocalStress(EMemberType memberType)
		{
			//double FsAll;
			//double Ls = 0;
			//double fs;
			//double Mn = 0;
			//double SupFy = 0;
			//double th = 0;
			//double g;
			//double B;
			//double a;
			//double c;
			//double V = 0;
			//double eEquiv = 0;
			//double t;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			//var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			// T = Applied tension per bolt or per inch or welded
			// L = Bolt Spacing or 1 inch for welded
			// eEquiv = Equivalent moment arm
			// Ls = Length of Shear resistance area
			// V = Shear to be resisted by Ls*th
			if (beam.P != 0)
			{
				switch (CommonDataStatic.BeamToColumnType)
				{
					case EJointConfiguration.BeamToHSSColumn:
						Reporting.AddHeader("Column flange local bending was not checked!");
						break;
					case EJointConfiguration.BeamToColumnWeb:
						Reporting.AddHeader("Column Web local bending was not checked!");
						break;
					case EJointConfiguration.BeamToGirder:
						Reporting.AddHeader("Girder web local bending was not checked!");
						break;
				}
			}

			//return;		 This was here in Descon 7. Who knows why?
			//switch (beam.ShearConnection)
			//{
			//	case EShearCarriedBy.ClipAngle:
			//		if (beam.WinConnect.ShearClipAngle.SupportSideConnection == EConnectionStyle.Bolted)
			//		{
			//			t = beam.WinConnect.ShearClipAngle.h / (beam.WinConnect.ShearClipAngle.BoltOslOnSupport.NumberOfBolts * beam.WinConnect.ShearClipAngle.Number);
			//			beam.WinConnect.Fema.L = beam.WinConnect.ShearClipAngle.BoltOslOnSupport.SpacingLongDir;
			//			switch (CommonDataStatic.JointConfig)
			//			{
			//				case EJointConfiguration.BeamToHSSColumn:
			//					eEquiv = column.GageOnFlange / 2 - column.Shape.k1;
			//					V = t;
			//					break;
			//				case EJointConfiguration.BeamToColumnWeb:
			//					if (beam.WinConnect.ShearClipAngle.Number == 1)
			//					{
			//						c = column.Shape.t;
			//						a = c / 2 + beam.Shape.tw / 2 + beam.WinConnect.ShearClipAngle.OSLValue - beam.WinConnect.ShearClipAngle.BoltOslOnSupport.EdgeDistTransvDir;
			//						B = c - a;
			//						eEquiv = Math.Pow(a, 2) * B / Math.Pow(c, 2);
			//						V = t * Math.Pow(a, 2) * (a + 3 * B) / Math.Pow(c, 3);
			//					}
			//					else
			//					{
			//						a = column.Shape.t;
			//						g = beam.Shape.tw + 2 * (beam.WinConnect.ShearClipAngle.OSLValue - beam.WinConnect.ShearClipAngle.BoltOslOnSupport.EdgeDistTransvDir);
			//						eEquiv = (Math.Pow(a, 2) - Math.Pow(g, 2)) / (4 * a);
			//						V = t;
			//					}
			//					break;
			//			}
			//		}
			//		break;
			//	case EShearCarriedBy.EndPlate:
			//		t = beam.WinConnect.ShearClipAngle.h / (beam.WinConnect.ShearClipAngle.BoltOslOnSupport.NumberOfBolts * beam.WinConnect.ShearClipAngle.Number);
			//		beam.WinConnect.Fema.L = beam.WinConnect.ShearClipAngle.BoltOslOnSupport.SpacingLongDir;
			//		switch (CommonDataStatic.JointConfig)
			//		{
			//			case EJointConfiguration.BeamToHSSColumn:
			//				eEquiv = column.GageOnFlange - column.Shape.k1;
			//				V = t;
			//				break;
			//			case EJointConfiguration.BeamToColumnWeb:
			//				if (beam.WinConnect.ShearClipAngle.Number == 1)
			//				{
			//					c = column.Shape.t;
			//					a = c / 2 + beam.Shape.tw / 2 + beam.WinConnect.ShearClipAngle.OSLValue - beam.WinConnect.ShearClipAngle.BoltOslOnSupport.EdgeDistTransvDir;
			//					B = c - a;
			//					eEquiv = Math.Pow(a, 2) * B / Math.Pow(c, 2);
			//					V = t * Math.Pow(a, 2) * (a + 3 * B) / Math.Pow(c, 3);
			//				}
			//				else
			//				{
			//					a = column.Shape.t;
			//					g = beam.Shape.tw + 2 * (beam.WinConnect.ShearClipAngle.OSLValue - beam.WinConnect.ShearClipAngle.BoltOslOnSupport.EdgeDistTransvDir);
			//					eEquiv = (Math.Pow(a, 2) - Math.Pow(g, 2)) / (4 * a);
			//					V = t;
			//				}
			//				break;
			//		}
			//		break;
			//}

			//Reporting.AddHeader("Local Stresses in Support from Beam Axial Load");
			//switch (CommonDataStatic.JointConfig)
			//{
			//	case EJointConfiguration.BeamToHSSColumn:
			//		th = column.Shape.tf;
			//		SupFy = column.Material.Fy;
			//		Reporting.AddHeader("Column Flange Local Bending:");
			//		break;
			//	case EJointConfiguration.BeamToColumnWeb:
			//		th = column.Shape.tw;
			//		SupFy = column.Material.Fy;
			//		Reporting.AddHeader("Column Web Local Bending:");
			//		break;
			//	case EJointConfiguration.BeamToGirder:
			//		th = column.Shape.tw;
			//		SupFy = column.Material.Fy;
			//		Reporting.AddHeader("Girder Web Local Bending:");
			//		break;
			//}

			//Mn = column.Material.Fy * Math.Pow(column.Shape.tw / 2, 2);

			//switch (CommonDataStatic.JointConfig)
			//{
			//	case EJointConfiguration.BeamToHSSColumn:
			//		Reporting.AddHeader("Column Flange Out-of-Plane Shear:");
			//		break;
			//	case EJointConfiguration.BeamToColumnWeb:
			//		Reporting.AddHeader("Column Web Out-of-Plane Shear:");
			//		break;
			//	case EJointConfiguration.BeamToGirder:
			//		Reporting.AddHeader("Girder Web Out-of-Plane Shear:");
			//		break;
			//}
			//fs = V / (Ls * th);
			//if (double.IsNaN(fs))
			//	fs = 0;
			//FsAll = 0.4 * SupFy;
			//Reporting.AddHeader("Shear Stress = V / (Ls * th)");
			//Reporting.AddLine("= " + V + " / (" + Ls + " * " + th + ")");
			//if (fs <= FsAll)
			//	Reporting.AddLine("= " + fs + " <= 0.4 * Fy = " + FsAll + ConstUnit.Stress + " (OK)");
			//else
			//	Reporting.AddLine("= " + fs + " >> 0.4 * Fy = " + FsAll + ConstUnit.Stress + " (NG)");
		}

		public static void FlangeForces(bool PanelZone, bool enableReporting)
		{
			double LMoment;
			double RMoment;
			string Oniki;
			double OnikiN;

			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			CommonDataStatic.ColumnStiffener.LPuf = 0;
			CommonDataStatic.ColumnStiffener.RPuf = 0;
			CommonDataStatic.ColumnStiffener.LRy = 0;
			CommonDataStatic.ColumnStiffener.RRy = 0;
			CommonDataStatic.ColumnStiffener.LDepth = 0;
			CommonDataStatic.ColumnStiffener.RDepth = 0;

			if (CommonDataStatic.Units == EUnit.US)
			{
				Oniki = "";
				OnikiN = 1;
			}
			else
			{
				Oniki = "";
				OnikiN = 1;
			}
			RMoment = rightBeam.Moment;
			LMoment = leftBeam.Moment;

			if (PanelZone && enableReporting)
			{
				Reporting.AddHeader("Column Panel Zone Shear Controlled by Load Combination");
			}

			if (rightBeam.IsActive && rightBeam.MomentConnection != EMomentCarriedBy.NoMoment)
			{
				switch (rightBeam.MomentConnection)
				{
					case EMomentCarriedBy.EndPlate:
						CommonDataStatic.ColumnStiffener.RDepth = rightBeam.Shape.d - rightBeam.Shape.tf;
						CommonDataStatic.ColumnStiffener.RDblextension = 3 * column.Shape.kdet + rightBeam.WinConnect.MomentEndPlate.Thickness;
						break;
					case EMomentCarriedBy.Tee:
						CommonDataStatic.ColumnStiffener.RDepth = rightBeam.Shape.d + (rightBeam.WinConnect.MomentTee.TopTeeShape.tw + rightBeam.WinConnect.MomentTee.BottomTeeShape.tw) / 2;
						CommonDataStatic.ColumnStiffener.RDblextension = 3 * column.Shape.kdet + rightBeam.WinConnect.MomentTee.TopTeeShape.tf;
						break;
					case EMomentCarriedBy.Angles:
						CommonDataStatic.ColumnStiffener.RDepth = rightBeam.Shape.d + rightBeam.WinConnect.MomentFlangeAngle.Angle.t; // *2
						CommonDataStatic.ColumnStiffener.RDblextension = 3 * column.Shape.kdet + (rightBeam.WinConnect.MomentFlangeAngle.Angle.b - rightBeam.WinConnect.MomentFlangeAngle.BeamBolt.EdgeDistTransvDir);
						break;
					case EMomentCarriedBy.FlangePlate:
						CommonDataStatic.ColumnStiffener.RDepth = rightBeam.Shape.d + (rightBeam.WinConnect.MomentFlangePlate.TopThickness + rightBeam.WinConnect.MomentFlangePlate.BottomThickness) / 2;
						CommonDataStatic.ColumnStiffener.RDblextension = 2.5 * column.Shape.kdet;
						break;
					case EMomentCarriedBy.DirectlyWelded:
						CommonDataStatic.ColumnStiffener.RDepth = rightBeam.Shape.d - rightBeam.Shape.tf;
						CommonDataStatic.ColumnStiffener.RDblextension = 2.5 * column.Shape.kdet;
						break;
				}
				if (!CommonDataStatic.ColumnStiffener.UseExtendedDoublerPlate)
					CommonDataStatic.ColumnStiffener.RDblextension = 0;

				switch (CommonDataStatic.SeismicSettings.Type)
				{
					case ESeismicType.Low:
						CommonDataStatic.ColumnStiffener.RDepth = rightBeam.Shape.d;
						CommonDataStatic.ColumnStiffener.RPuf = RMoment * OnikiN / CommonDataStatic.ColumnStiffener.RDepth + rightBeam.P / 2;
						break;
					case ESeismicType.High:
						if (CommonDataStatic.Units == EUnit.US)
						{
							if (rightBeam.Material.Fy < 40)
								CommonDataStatic.ColumnStiffener.RRy = 1.5;
							else if (rightBeam.Material.Fy < 48)
								CommonDataStatic.ColumnStiffener.RRy = 1.3;
							else
								CommonDataStatic.ColumnStiffener.RRy = 1.1;
						}
						else
						{
							if (rightBeam.Material.Fy < 275)
								CommonDataStatic.ColumnStiffener.RRy = 1.5;
							else if (rightBeam.Material.Fy < 330)
								CommonDataStatic.ColumnStiffener.RRy = 1.3;
							else
								CommonDataStatic.ColumnStiffener.RRy = 1.1;
						}
						switch (CommonDataStatic.SeismicSettings.FramingType)
						{
							case EFramingSystem.OMF:
								CommonDataStatic.ColumnStiffener.RPuf = Math.Min(RMoment * OnikiN, 1.1 * CommonDataStatic.ColumnStiffener.RRy * rightBeam.Material.Fy * rightBeam.Shape.zx) / CommonDataStatic.ColumnStiffener.RDepth;
								break;
							case EFramingSystem.SMF:
							case EFramingSystem.IMF:
								CommonDataStatic.ColumnStiffener.RPuf = Math.Min(RMoment * OnikiN / CommonDataStatic.ColumnStiffener.RDepth, (1.1 * CommonDataStatic.ColumnStiffener.RRy * rightBeam.Material.Fy * CommonDataStatic.SeismicSettings.RReducedSectionZ + rightBeam.ShearForce * CommonDataStatic.SeismicSettings.RHingeDistance) / CommonDataStatic.ColumnStiffener.RDepth);
								break;
						}
						break;
				}
			}
			if (leftBeam.IsActive && leftBeam.MomentConnection != EMomentCarriedBy.NoMoment)
			{
				switch (leftBeam.MomentConnection)
				{
					case EMomentCarriedBy.EndPlate:
						CommonDataStatic.ColumnStiffener.LDepth = leftBeam.Shape.d - leftBeam.Shape.tf;
						CommonDataStatic.ColumnStiffener.LDblextension = 3 * column.Shape.kdet + leftBeam.WinConnect.MomentEndPlate.Thickness;
						break;
					case EMomentCarriedBy.Tee:
						CommonDataStatic.ColumnStiffener.LDepth = leftBeam.Shape.d + (leftBeam.WinConnect.MomentTee.TopTeeShape.tw + leftBeam.WinConnect.MomentTee.BottomTeeShape.tf) / 2;
						CommonDataStatic.ColumnStiffener.LDblextension = 3 * column.Shape.kdet + leftBeam.WinConnect.MomentTee.TopTeeShape.tf;
						break;
					case EMomentCarriedBy.Angles:
						CommonDataStatic.ColumnStiffener.LDepth = leftBeam.Shape.d + leftBeam.WinConnect.MomentFlangeAngle.Angle.t; // *2
						CommonDataStatic.ColumnStiffener.LDblextension = 3 * column.Shape.kdet + (leftBeam.WinConnect.MomentFlangeAngle.Angle.b - leftBeam.WinConnect.MomentFlangeAngle.BeamBolt.EdgeDistTransvDir);
						break;
					case EMomentCarriedBy.FlangePlate:
						CommonDataStatic.ColumnStiffener.LDepth = leftBeam.Shape.d + (leftBeam.WinConnect.MomentFlangePlate.TopThickness + leftBeam.WinConnect.MomentFlangePlate.BottomThickness) / 2;
						CommonDataStatic.ColumnStiffener.LDblextension = 2.5 * column.Shape.kdet;
						break;
					case EMomentCarriedBy.DirectlyWelded:
						CommonDataStatic.ColumnStiffener.LDepth = leftBeam.Shape.d - leftBeam.Shape.tf;
						CommonDataStatic.ColumnStiffener.LDblextension = 2.5 * column.Shape.kdet;
						break;
				}
				if (!CommonDataStatic.ColumnStiffener.UseExtendedDoublerPlate)
					CommonDataStatic.ColumnStiffener.LDblextension = 0;
				switch (CommonDataStatic.SeismicSettings.Type)
				{
					case ESeismicType.Low:
						if (leftBeam.IsActive)
							CommonDataStatic.ColumnStiffener.LPuf = LMoment * OnikiN / CommonDataStatic.ColumnStiffener.LDepth + leftBeam.P / 2;
						else
							CommonDataStatic.ColumnStiffener.LPuf = 0;
						break;
					case ESeismicType.High:
						if (CommonDataStatic.Units == EUnit.US)
						{
							if (leftBeam.Material.Fy < 40)
								CommonDataStatic.ColumnStiffener.LRy = 1.5;
							else if (leftBeam.Material.Fy < 48)
								CommonDataStatic.ColumnStiffener.LRy = 1.3;
							else
								CommonDataStatic.ColumnStiffener.LRy = 1.1;
						}
						else
						{
							if (leftBeam.Material.Fy < 275)
								CommonDataStatic.ColumnStiffener.LRy = 1.5;
							else if (leftBeam.Material.Fy < 330)
								CommonDataStatic.ColumnStiffener.LRy = 1.3;
							else
								CommonDataStatic.ColumnStiffener.LRy = 1.1;
						}
						if (leftBeam.IsActive)
						{
							switch (CommonDataStatic.SeismicSettings.FramingType)
							{
								case EFramingSystem.OMF:
									CommonDataStatic.ColumnStiffener.LPuf = Math.Min(LMoment * OnikiN, 1.1F * CommonDataStatic.ColumnStiffener.LRy * leftBeam.Material.Fy * leftBeam.Shape.zx) / CommonDataStatic.ColumnStiffener.LDepth;
									break;
								case EFramingSystem.SMF:
								case EFramingSystem.IMF:
									CommonDataStatic.ColumnStiffener.LPuf = Math.Min(LMoment * OnikiN / CommonDataStatic.ColumnStiffener.LDepth, (1.1 * CommonDataStatic.ColumnStiffener.LRy * leftBeam.Material.Fy * CommonDataStatic.SeismicSettings.LReducedSectionZ + leftBeam.ShearForce * CommonDataStatic.SeismicSettings.LHingeDistance) / CommonDataStatic.ColumnStiffener.LDepth);
									break;
							}
						}
						break;
				}
			}

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
				Reporting.AddHeader("HSS Column Panel Zone");
			else
			{
				if (PanelZone)
					Reporting.AddGoToHeader("Column Web Shear Reinforcement");
				else
					Reporting.AddGoToHeader("Column Stiffeners");
			}

			Reporting.AddHeader("Framing System: " + CommonDataStatic.SeismicSettings.FramingType);
			if (CommonDataStatic.SeismicSettings.InelasticPanelZone)
			{
				Reporting.AddLine("User specified that inelastic deformation of the column panel zone");
				Reporting.AddLine("was considered in the analysis of the frame stability.");
			}
			if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic && CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.RBS)
			{
				if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.OMF || CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.SMF)
				{
					if (rightBeam.IsActive)
					{
						Reporting.AddHeader("Right Side Beam:");
						Reporting.AddLine("Reduced Plastic Section Modulus = " + CommonDataStatic.SeismicSettings.RReducedSectionZ + ConstUnit.SecMod);
						Reporting.AddLine("Plastic Hinge Distance to Column Face = " + CommonDataStatic.SeismicSettings.RHingeDistance + ConstUnit.Length);
					}
					if (leftBeam.IsActive)
					{
						Reporting.AddHeader("Left Side Beam:");
						Reporting.AddLine("Reduced Plastic Section Modulus = " + CommonDataStatic.SeismicSettings.LReducedSectionZ + ConstUnit.SecMod);
						Reporting.AddLine("Plastic Hinge Distance to Column Face = " + CommonDataStatic.SeismicSettings.LHingeDistance + ConstUnit.Length);
					}
				}
			}

			Reporting.AddHeader("Column Axial Force (Pu) = " + column.P + ConstUnit.Force);
			if (column.P > 0.85 * column.Shape.a * column.Material.Fy)
				Reporting.AddLine("Column axial force is excessive. (NG)");
			Reporting.AddLine("Column Shear Force (Vus) = " + column.ShearForce + ConstUnit.Force);


			if (rightBeam.IsActive && rightBeam.MomentConnection != EMomentCarriedBy.NoMoment)
			{
				Reporting.AddHeader("Right Side Beam Flange Forces:");
				if (CommonDataStatic.SeismicSettings.ConnectionType == ESeismicConnectionType.RBS)
				{
					if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.IMF || CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.SMF)
					{
						Reporting.AddLine("Plastic Section Modulus at Plastic Hinge = " + CommonDataStatic.SeismicSettings.RReducedSectionZ + ConstUnit.SecMod);
						Reporting.AddLine("Plastic Hinge Distance to Column Face = " + CommonDataStatic.SeismicSettings.RHingeDistance + ConstUnit.Length);
					}
				}
				switch (rightBeam.MomentConnection)
				{
					case EMomentCarriedBy.EndPlate:
						CommonDataStatic.ColumnStiffener.RDepth = rightBeam.Shape.d - rightBeam.Shape.tf;
						CommonDataStatic.ColumnStiffener.RDblextension = 3 * column.Shape.kdet + rightBeam.WinConnect.MomentEndPlate.Thickness;
						break;
					case EMomentCarriedBy.Tee:
						CommonDataStatic.ColumnStiffener.RDepth = rightBeam.Shape.d + (rightBeam.WinConnect.MomentTee.TopTeeShape.tw + rightBeam.WinConnect.MomentTee.BottomTeeShape.tw) / 2;
						CommonDataStatic.ColumnStiffener.RDblextension = 3 * column.Shape.kdet + rightBeam.WinConnect.MomentTee.TopTeeShape.tf;
						break;
					case EMomentCarriedBy.Angles:
						CommonDataStatic.ColumnStiffener.RDepth = rightBeam.Shape.d + rightBeam.WinConnect.MomentFlangeAngle.Angle.t;
						CommonDataStatic.ColumnStiffener.RDblextension = 3 * column.Shape.kdet + (rightBeam.WinConnect.MomentFlangeAngle.Angle.b - rightBeam.WinConnect.MomentFlangeAngle.BeamBolt.EdgeDistTransvDir);
						break;
					case EMomentCarriedBy.FlangePlate:
						CommonDataStatic.ColumnStiffener.RDepth = rightBeam.Shape.d + (rightBeam.WinConnect.MomentFlangePlate.TopThickness + rightBeam.WinConnect.MomentFlangePlate.BottomThickness) / 2;
						CommonDataStatic.ColumnStiffener.RDblextension = 2.5 * column.Shape.kdet;
						break;
					case EMomentCarriedBy.DirectlyWelded:
						CommonDataStatic.ColumnStiffener.RDepth = rightBeam.Shape.d - rightBeam.Shape.tf;
						CommonDataStatic.ColumnStiffener.RDblextension = 2.5 * column.Shape.kdet;
						break;
				}
				if (!CommonDataStatic.ColumnStiffener.UseExtendedDoublerPlate)
				{
					CommonDataStatic.ColumnStiffener.RDblextension = 0;
				}
				if (rightBeam.IsActive)
				{
					switch (CommonDataStatic.SeismicSettings.Type)
					{
						case ESeismicType.Low:
							CommonDataStatic.ColumnStiffener.RPuf = RMoment * OnikiN / CommonDataStatic.ColumnStiffener.RDepth + rightBeam.P / 2;
							Reporting.AddHeader("PufRight = Mu" + Oniki + " / dm + Pu / 2");
							Reporting.AddLine("= " + RMoment + Oniki + " / " + CommonDataStatic.ColumnStiffener.RDepth + " + " + rightBeam.P + " / 2");
							Reporting.AddLine("= " + CommonDataStatic.ColumnStiffener.RPuf + ConstUnit.Force);
							break;
						case ESeismicType.High:
							if (CommonDataStatic.Units == EUnit.US)
							{
								if (rightBeam.Material.Fy < 40)
									CommonDataStatic.ColumnStiffener.RRy = 1.5;
								else if (rightBeam.Material.Fy < 48)
									CommonDataStatic.ColumnStiffener.RRy = 1.3;
								else
									CommonDataStatic.ColumnStiffener.RRy = 1.1;
							}
							else
							{
								if (rightBeam.Material.Fy < 275)
									CommonDataStatic.ColumnStiffener.RRy = 1.5;
								else if (rightBeam.Material.Fy < 330)
									CommonDataStatic.ColumnStiffener.RRy = 1.3;
								else
									CommonDataStatic.ColumnStiffener.RRy = 1.1;
							}
							switch (CommonDataStatic.SeismicSettings.FramingType)
							{
								case EFramingSystem.OMF:
									CommonDataStatic.ColumnStiffener.RPuf = Math.Min(RMoment * OnikiN, 1.1 * CommonDataStatic.ColumnStiffener.RRy * rightBeam.Material.Fy * rightBeam.Shape.zx) / CommonDataStatic.ColumnStiffener.RDepth;
									Reporting.AddLine("PufRight = Min(Mu" + Oniki + ", 1.1 * Ry * Fy * Z) / dm");
									Reporting.AddLine("= Min(" + RMoment + Oniki + ", 1.1 * " + CommonDataStatic.ColumnStiffener.RRy + " * " + rightBeam.Material.Fy + " * " + rightBeam.Shape.zx + ") / " + CommonDataStatic.ColumnStiffener.RDepth);
									break;
								case EFramingSystem.SMF:
								case EFramingSystem.IMF:
									CommonDataStatic.ColumnStiffener.RPuf = Math.Min(RMoment * OnikiN / CommonDataStatic.ColumnStiffener.RDepth, (1.1 * CommonDataStatic.ColumnStiffener.RRy * rightBeam.Material.Fy * CommonDataStatic.SeismicSettings.RReducedSectionZ + rightBeam.ShearForce * CommonDataStatic.SeismicSettings.RHingeDistance) / CommonDataStatic.ColumnStiffener.RDepth);
									Reporting.AddLine("PufRight = Min(Mu" + Oniki + " / dm, (1.1 * Ry * Fy * Z + Vus * a) / dm)");
									Reporting.AddLine("= Min(" + RMoment + Oniki + " / " + CommonDataStatic.ColumnStiffener.RDepth + ", (1.1 * " + CommonDataStatic.ColumnStiffener.RRy + " * " + rightBeam.Material.Fy + " * " + CommonDataStatic.SeismicSettings.RReducedSectionZ + " + " + rightBeam.ShearForce + " * " + CommonDataStatic.SeismicSettings.RHingeDistance + ")/" + CommonDataStatic.ColumnStiffener.RDepth);
									break;
							}
							Reporting.AddLine("= " + CommonDataStatic.ColumnStiffener.RPuf + ConstUnit.Force);
							break;
					}
				}
			}

			if (leftBeam.IsActive && leftBeam.MomentConnection != EMomentCarriedBy.NoMoment)
			{
				Reporting.AddHeader("Left Side Beam Flange Forces:");
				if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.IMF || CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.SMF)
				{
					Reporting.AddLine("Plastic Section Modulus at Plastic Hinge = " + CommonDataStatic.SeismicSettings.LReducedSectionZ + ConstUnit.SecMod);
					Reporting.AddLine("Plastic Hinge Distance to Column Face = " + CommonDataStatic.SeismicSettings.LHingeDistance + ConstUnit.Length);
				}
				switch (leftBeam.MomentConnection)
				{
					case EMomentCarriedBy.EndPlate:
						CommonDataStatic.ColumnStiffener.LDepth = leftBeam.Shape.d - leftBeam.Shape.tf;
						CommonDataStatic.ColumnStiffener.LDblextension = 3 * column.Shape.kdet + leftBeam.WinConnect.MomentEndPlate.Thickness;
						break;
					case EMomentCarriedBy.Tee:
						CommonDataStatic.ColumnStiffener.LDepth = leftBeam.Shape.d + (leftBeam.WinConnect.MomentTee.TopTeeShape.tw + leftBeam.WinConnect.MomentTee.BottomTeeShape.tf) / 2;
						CommonDataStatic.ColumnStiffener.LDblextension = 3 * column.Shape.kdet + leftBeam.WinConnect.MomentTee.TopTeeShape.tf;
						break;
					case EMomentCarriedBy.Angles:
						CommonDataStatic.ColumnStiffener.LDepth = leftBeam.Shape.d + leftBeam.WinConnect.MomentFlangeAngle.Angle.t;
						CommonDataStatic.ColumnStiffener.LDblextension = 3 * column.Shape.kdet + (leftBeam.WinConnect.MomentFlangeAngle.Angle.b - leftBeam.WinConnect.MomentFlangeAngle.BeamBolt.EdgeDistTransvDir);
						break;
					case EMomentCarriedBy.FlangePlate:
						CommonDataStatic.ColumnStiffener.LDepth = leftBeam.Shape.d + (leftBeam.WinConnect.MomentFlangePlate.TopThickness + leftBeam.WinConnect.MomentFlangePlate.BottomThickness) / 2;
						CommonDataStatic.ColumnStiffener.LDblextension = 2.5 * column.Shape.kdet;
						break;
					case EMomentCarriedBy.DirectlyWelded:
						CommonDataStatic.ColumnStiffener.LDepth = leftBeam.Shape.d - leftBeam.Shape.tf;
						CommonDataStatic.ColumnStiffener.LDblextension = 2.5 * column.Shape.kdet;
						break;
				}
				if (!CommonDataStatic.ColumnStiffener.UseExtendedDoublerPlate)
					CommonDataStatic.ColumnStiffener.LDblextension = 0;
				switch (CommonDataStatic.SeismicSettings.Type)
				{
					case ESeismicType.Low:
						CommonDataStatic.ColumnStiffener.LPuf = LMoment * OnikiN / CommonDataStatic.ColumnStiffener.LDepth + leftBeam.P / 2;
						Reporting.AddLine("PufLeft = Mu" + Oniki + " / dm + Pu / 2");
						Reporting.AddLine("= " + LMoment + Oniki + " / " + CommonDataStatic.ColumnStiffener.LDepth + " + " + leftBeam.P + " / 2");
						Reporting.AddLine("= " + CommonDataStatic.ColumnStiffener.LPuf + ConstUnit.Force);
						break;
					case ESeismicType.High:
						if (CommonDataStatic.Units == EUnit.US)
						{
							if (leftBeam.Material.Fy < 40)
								CommonDataStatic.ColumnStiffener.LRy = 1.5;
							else if (leftBeam.Material.Fy < 48)
								CommonDataStatic.ColumnStiffener.LRy = 1.3;
							else
								CommonDataStatic.ColumnStiffener.LRy = 1.1;
						}
						else
						{
							if (leftBeam.Material.Fy < 275)
								CommonDataStatic.ColumnStiffener.LRy = 1.5;
							else if (leftBeam.Material.Fy < 330)
								CommonDataStatic.ColumnStiffener.LRy = 1.3;
							else
								CommonDataStatic.ColumnStiffener.LRy = 1.1;
						}
						switch (CommonDataStatic.SeismicSettings.FramingType)
						{
							case EFramingSystem.OMF:
								CommonDataStatic.ColumnStiffener.LPuf = Math.Min(LMoment * OnikiN, 1.1 * CommonDataStatic.ColumnStiffener.LRy * leftBeam.Material.Fy * leftBeam.Shape.zx) / CommonDataStatic.ColumnStiffener.LDepth;
								Reporting.AddLine("PufLeft = Min(Mu" + Oniki + ", 1.1 * Ry * Fy * Z) / dm");
								Reporting.AddLine("= Min(" + LMoment + Oniki + ", 1.1 * " + CommonDataStatic.ColumnStiffener.LRy + " * " + leftBeam.Material.Fy + " * " + leftBeam.Shape.zx + ") / " + CommonDataStatic.ColumnStiffener.LDepth);
								break;
							case EFramingSystem.SMF:
							case EFramingSystem.IMF:
								CommonDataStatic.ColumnStiffener.LPuf = Math.Min(LMoment * OnikiN / CommonDataStatic.ColumnStiffener.LDepth, (1.1 * CommonDataStatic.ColumnStiffener.LRy * leftBeam.Material.Fy * CommonDataStatic.SeismicSettings.LReducedSectionZ + leftBeam.ShearForce * CommonDataStatic.SeismicSettings.LHingeDistance) / CommonDataStatic.ColumnStiffener.LDepth);
								Reporting.AddLine("PufLeft = Min(Mu" + Oniki + "/dm, (1.1 * Ry * Fy * Z + Vus * a) / dm)");
								Reporting.AddLine("= Min(" + LMoment + Oniki + " / " + CommonDataStatic.ColumnStiffener.LDepth + ";, (1.1 * " + CommonDataStatic.ColumnStiffener.LRy + " * " + leftBeam.Material.Fy + " * " + CommonDataStatic.SeismicSettings.LReducedSectionZ + " + " + leftBeam.ShearForce + " * " + CommonDataStatic.SeismicSettings.LHingeDistance + ")/" + CommonDataStatic.ColumnStiffener.LDepth);
								break;
						}
						Reporting.AddLine("= " + CommonDataStatic.ColumnStiffener.LPuf + ConstUnit.Force);
						break;
				}
			}
		}

		public static void DoublerPl()
		{
			double tw = 0;
			double Wz = 0;
			double dz = 0;
			double diff = 0;
			double LvLtemp = 0;
			double LvRtemp = 0;
			double oneplateminTh = 0;
			double tp_withStiffener = 0;
			double clip = 0;
			double CompStifForce = 0;
			double TenstifForce = 0;
			double tp_forFilletweld = 0;
			double re = 0;
			double fr = 0;
			double tp = 0;
			double tf = 0;
			double dc = 0;
			double ts = 0;
			double dm = 0;
			double tp_ShearBuckling = 0;
			double FIRV = 0;
			double Pc = 0;
			double Py = 0;
			double Vu = 0;
			double e = 0;

			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var columnStiffener = CommonDataStatic.ColumnStiffener;

			// Required Panel Zone Shear Strength
			switch (CommonDataStatic.SeismicSettings.Type)
			{
				case ESeismicType.Low:
					Vu = columnStiffener.LPuf + columnStiffener.RPuf - column.ShearForce;
					break;
				case ESeismicType.High:
					if (Math.Abs(columnStiffener.LPuf) > 0 && Math.Abs(columnStiffener.RPuf) > 0)
						Vu = 0.8 * (columnStiffener.LPuf + columnStiffener.RPuf) - column.ShearForce;
					else
						Vu = columnStiffener.LPuf + columnStiffener.RPuf - column.ShearForce;
					break;
			}
			if (columnStiffener.ColumnWebShear_User)
				Vu = columnStiffener.ColumnWebShearOverride;
			Vu = Math.Abs(Vu);
			columnStiffener.ColumnWebShearOverride = Vu;

			//  Panel zone shear strength
			Py = column.Shape.a * column.Material.Fy;
			if (CommonDataStatic.Preferences.CalcMode == ECalcMode.LRFD)
				Pc = Py;
			else
				Pc = 0.6 * Py;

			if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low || CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.OMF)
			{
				// a Low seismic
				if (!CommonDataStatic.SeismicSettings.InelasticPanelZone)
				{
					if (column.P <= 0.4 * Pc)
						FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw;
					else
						FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1.4F - column.P / Pc);
				}
				else
				{
					// inelastic panel zone
					if (column.P <= 0.75 * Pc)
						FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw));
					else
						FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw)) * (1.9 - 1.2 * column.P / Pc);
				}
			}
			else
			{
				// High seismic with SMF or IMF
				if (!CommonDataStatic.SeismicSettings.InelasticPanelZone)
				{
					if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic)
						Reporting.AddLine("Inelastic deformations of the panel zone must be included in the analysis of the stability of the frame.  You have not specified this to be the case. (NG)");
				}
				if (column.P <= 0.75 * Pc)
					FIRV = ConstNum.FIOMEGA0_75N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw));
				else
					FIRV = ConstNum.FIOMEGA0_75N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw)) * (1.9 - 1.2 * column.P / Pc);
			}
			if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
				tp_ShearBuckling = (column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(column.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5));
			else
			{
				dm = Math.Max(columnStiffener.RDepth, columnStiffener.LDepth);
				ts = columnStiffener.SThickness;
				dc = column.Shape.d;
				tf = column.Shape.tf;
				tp_ShearBuckling = Math.Max((column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(column.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5)), (dm - ts + dc - 2 * tf) / 90);
			}

			if (Vu <= FIRV && tp_ShearBuckling < column.Shape.tw)
			{
				// Doubler Plate not required
				tp_ShearBuckling = 0;
				tp = 0;
				columnStiffener.DThickness = 0;
				columnStiffener.DThickness2 = 0;
			}
			else
			{
				// Doubler Plate required
				columnStiffener.DColShr = Vu - FIRV;
				if (columnStiffener.DColShr < 0)
					columnStiffener.DColShr = 0;
				tp = columnStiffener.DColShr / (ConstNum.FIOMEGA0_9N * 0.6 * columnStiffener.Material.Fy * column.Shape.d);
				fr = column.Shape.kdet - column.Shape.tf;

				if (CommonDataStatic.Units == EUnit.US)
					e = Math.Pow(3.0 / 32 * fr - Math.Pow(3.0 / 64, 2), 0.5); //  fillet encroachment
				else
					e = Math.Pow(3.0 / 32 * 25.4 * fr - Math.Pow(3.0 / 64 * 25.4, 2), 0.5); //  fillet encroachment
				re = NumberFun.Round(e, 16);
				tp_forFilletweld = fr - re;
				if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
					tp_ShearBuckling = (column.Shape.d - 2 * column.Shape.kdes) * Math.Pow(columnStiffener.Material.Fy, 0.5) / (2.24F * Math.Pow(ConstNum.ELASTICITY, 0.5));
				else
				{
					dm = Math.Max(columnStiffener.RDepth, columnStiffener.LDepth);
					ts = columnStiffener.SThickness;
					dc = column.Shape.d;
					tf = column.Shape.tf;
					tp_ShearBuckling = Math.Max((column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(columnStiffener.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5)), (dm - ts + dc - 2 * tf) / 90);
				}
				if (columnStiffener.UseExtendedDoublerPlate)
				{
					TenstifForce = Math.Max(columnStiffener.LTenstifForce, columnStiffener.RTenstifForce);
					CompStifForce = Math.Max(columnStiffener.LCompStifForce, columnStiffener.RCompStifForce);
					clip = column.Shape.kdet - column.Shape.tf;
					if (columnStiffener.StiffenerLength == EStiffenerLength.PartialLengthOfWeb)
						tp_withStiffener = Math.Max(TenstifForce, CompStifForce) / (ConstNum.FIOMEGA0_9N * 0.6 * columnStiffener.Material.Fy * 2 * Math.Min(2 * (columnStiffener.SLength - clip), column.Shape.d));
					else
						tp_withStiffener = Math.Max(columnStiffener.LTenstifForce + columnStiffener.RCompStifForce, columnStiffener.RTenstifForce + columnStiffener.LCompStifForce) / (ConstNum.FIOMEGA0_9N * 0.6 * columnStiffener.Material.Fy * 2 * Math.Min(2 * (columnStiffener.SLength - 2 * clip), column.Shape.d));
				}
				else
					tp_withStiffener = 0;

				oneplateminTh = Math.Max(tp_forFilletweld, Math.Max(tp_ShearBuckling, tp_withStiffener));

				if (!columnStiffener.DThickness_User && !columnStiffener.DNumberOfPlates_User)
				{
					if (tp < oneplateminTh)
					{
						columnStiffener.DThickness = oneplateminTh;
						columnStiffener.DThickness2 = 0;
						columnStiffener.DNumberOfPlates = 1;
					}
					else if (tp <= 0.75 * ConstNum.ONE_INCH)
					{
						columnStiffener.DThickness = tp;
						columnStiffener.DThickness2 = 0;
						columnStiffener.DNumberOfPlates = 1;
					}
					else
					{
						columnStiffener.DThickness = Math.Max(tp / 2, oneplateminTh);
						columnStiffener.DThickness2 = columnStiffener.DThickness;
						columnStiffener.DNumberOfPlates = 2;
					}
					// check col.web thickness to develop stiffener when cwsr=1
					columnStiffener.DThickness = CommonCalculations.PlateThickness(NumberFun.Round(columnStiffener.DThickness, 16));
					columnStiffener.DThickness2 = NumberFun.Round(columnStiffener.DThickness2, 16);
					if (!columnStiffener.DHorizontal_User)
						columnStiffener.DHorizontal = NumberFun.Round(column.Shape.d - 2 * column.Shape.tf, 16);
				}
			}
			if (columnStiffener.DNumberOfPlates > 0)
				columnStiffener.Dpt = CommonCalculations.PlateThickness(Math.Max(Math.Max(tp_ShearBuckling, tp), tp_withStiffener) / columnStiffener.DNumberOfPlates);

			if (columnStiffener.TenStiff & columnStiffener.CompStiff)
			{
				if (columnStiffener.UseExtendedDoublerPlate)
				{
					if (columnStiffener.BeamNearTopOfColumn)
					{
						columnStiffener.DVertical = columnStiffener.RDepth + columnStiffener.RDblextension - columnStiffener.SThickness / 2;
						columnStiffener.LvL = columnStiffener.LDepth + columnStiffener.LDblextension - columnStiffener.SThickness / 2;
					}
					else
					{
						columnStiffener.DVertical = columnStiffener.RDepth + 2 * columnStiffener.RDblextension + 2 * columnStiffener.SThickness / 2;
						columnStiffener.LvL = columnStiffener.LDepth + 2 * columnStiffener.LDblextension + 2 * columnStiffener.SThickness / 2;
					}
				}
				else
				{
					columnStiffener.DVertical = columnStiffener.RDepth - 2 * columnStiffener.SThickness; // / 2
					columnStiffener.LvL = columnStiffener.LDepth - 2 * columnStiffener.SThickness; // / 2
				}
			}
			else if (columnStiffener.TenStiff)
			{
				if (columnStiffener.UseExtendedDoublerPlate)
				{
					columnStiffener.DVertical = columnStiffener.RDepth + 2 * columnStiffener.RDblextension + columnStiffener.SThickness / 2;
					columnStiffener.LvL = columnStiffener.LDepth + 2 * columnStiffener.LDblextension + columnStiffener.SThickness / 2;
				}
				else
				{
					columnStiffener.DVertical = columnStiffener.RDepth + columnStiffener.RDblextension - columnStiffener.SThickness / 2;
					columnStiffener.LvL = columnStiffener.LDepth + columnStiffener.LDblextension - columnStiffener.SThickness / 2;
				}
			}
			else if (columnStiffener.CompStiff)
			{
				if (columnStiffener.UseExtendedDoublerPlate)
				{
					columnStiffener.DVertical = columnStiffener.RDepth + 2 * columnStiffener.RDblextension + columnStiffener.SThickness / 2;
					columnStiffener.LvL = columnStiffener.LDepth + 2 * columnStiffener.LDblextension + columnStiffener.SThickness / 2;
				}
				else
				{
					columnStiffener.DVertical = columnStiffener.RDepth + columnStiffener.RDblextension - columnStiffener.SThickness / 2;
					columnStiffener.LvL = columnStiffener.LDepth + columnStiffener.LDblextension - columnStiffener.SThickness / 2;
				}
			}
			else
			{
				columnStiffener.DVertical = columnStiffener.RDepth + 2 * columnStiffener.RDblextension;
				columnStiffener.LvL = columnStiffener.LDepth + 2 * columnStiffener.LDblextension;
			}
			if (columnStiffener.LDepth > 0 && columnStiffener.RDepth > 0)
			{
				columnStiffener.LDoublerPlateBottom = leftBeam.WinConnect.Beam.FrontY - columnStiffener.LvL / 2;
				columnStiffener.RDoublerPlateBottom = rightBeam.WinConnect.Beam.FrontY - columnStiffener.DVertical / 2;
				if (columnStiffener.UseExtendedDoublerPlate)
				{
					if (columnStiffener.BeamNearTopOfColumn)
					{
						LvRtemp = columnStiffener.RDepth + 2 * columnStiffener.RDblextension + 2 * columnStiffener.SThickness / 2;
						LvLtemp = columnStiffener.LDepth + 2 * columnStiffener.LDblextension + 2 * columnStiffener.SThickness / 2;
						columnStiffener.LDoublerPlateBottom = leftBeam.WinConnect.Beam.FrontY - LvLtemp / 2;
						columnStiffener.RDoublerPlateBottom = rightBeam.WinConnect.Beam.FrontY - LvRtemp / 2;
						diff = columnStiffener.LDoublerPlateBottom - columnStiffener.RDoublerPlateBottom;
						columnStiffener.LDoublerPlateBottom = Math.Min(columnStiffener.LDoublerPlateBottom, columnStiffener.RDoublerPlateBottom);
						columnStiffener.RDoublerPlateBottom = columnStiffener.LDoublerPlateBottom;
						if (diff > 0)
							columnStiffener.LvL = columnStiffener.LvL + diff;
						else
							columnStiffener.DVertical = columnStiffener.DVertical - diff;
					}
					else
					{
						columnStiffener.LvL = Math.Max(columnStiffener.LvL, columnStiffener.DVertical);
						columnStiffener.DVertical = columnStiffener.LvL;
						columnStiffener.LDoublerPlateBottom = Math.Min(columnStiffener.LDoublerPlateBottom, columnStiffener.RDoublerPlateBottom);
						columnStiffener.RDoublerPlateBottom = columnStiffener.LDoublerPlateBottom;
					}
				}
			}
			else if (columnStiffener.RDepth > 0)
			{
				columnStiffener.LvL = columnStiffener.DVertical;
				columnStiffener.RDoublerPlateBottom = rightBeam.WinConnect.Beam.FrontY - columnStiffener.DVertical / 2;
				columnStiffener.LDoublerPlateBottom = columnStiffener.RDoublerPlateBottom;
			}
			else if (columnStiffener.LDepth > 0)
			{
				columnStiffener.DVertical = columnStiffener.LvL;
				columnStiffener.LDoublerPlateBottom = leftBeam.WinConnect.Beam.FrontY - columnStiffener.LvL / 2;
				columnStiffener.RDoublerPlateBottom = columnStiffener.LDoublerPlateBottom;
			}

			columnStiffener.DVertical = NumberFun.Round(columnStiffener.DVertical, 16);
			columnStiffener.LvL = NumberFun.Round(columnStiffener.LvL, 16);

			columnStiffener.DVertical = Math.Max(columnStiffener.DVertical, columnStiffener.LvL);
			columnStiffener.LvL = columnStiffener.DVertical;
			columnStiffener.RDoublerPlateBottom = Math.Min(columnStiffener.LDoublerPlateBottom, columnStiffener.RDoublerPlateBottom);
			columnStiffener.LDoublerPlateBottom = columnStiffener.RDoublerPlateBottom;
			columnStiffener.RDblextension = Math.Max(columnStiffener.RDblextension, columnStiffener.LDblextension);
			columnStiffener.LDblextension = columnStiffener.RDblextension;

			Reporting.AddHeader("Column Panel Zone:");
			if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic)
			{
				Reporting.AddHeader("Column Web Thickness (tw) = " + column.Shape.tw + ConstUnit.Length);

				if (rightBeam.IsActive && leftBeam.IsActive)
				{
					Reporting.AddLine("Panel Zone Depth (dz) = Max(Rbeam_d - 2 * Rbeam_tf, Lbeam_d - 2 * Lbeam_tf)");
					dz = Math.Max(rightBeam.Shape.d - 2 * rightBeam.Shape.tf, leftBeam.Shape.d - 2 * leftBeam.Shape.tf);
					Reporting.AddLine("= Max( " + rightBeam.Shape.d + " - 2 * " + rightBeam.Shape.tf + ", " + leftBeam.Shape.d + " - 2 * " + leftBeam.Shape.tf + " )");
					Reporting.AddLine("dz = " + dz + ConstUnit.Length);
				}
				else if (rightBeam.IsActive)
				{
					dz = rightBeam.Shape.d;
					Reporting.AddLine("dz = " + dz + ConstUnit.Length);

				}
				else if (leftBeam.IsActive)
				{
					dz = leftBeam.Shape.d;
					Reporting.AddLine("dz = " + dz + ConstUnit.Length);
				}

				Wz = column.Shape.d - 2 * column.Shape.tf;
				Reporting.AddLine("Panel Zone Width, Wz = Column_d - 2 * column_tf = " + column.Shape.d + " - 2 * " + column.Shape.tf);
				Reporting.AddLine("Wz = " + Wz + ConstUnit.Length);
				tw = column.Shape.tw;
				Reporting.AddLine("(dz + Wz) / 90 = " + (dz + Wz) / 90 + " " + ConstUnit.Length);
				if (tw >= (dz + Wz) / 90)
				{
					Reporting.AddLine(" tw >= (dz + Wz) / 90 :");
					Reporting.AddLine(" " + tw + " >=  (" + dz + " + " + Wz + ") / 90 (OK)");
				}
				else
				{
					Reporting.AddLine(" tw >= (dz + Wz) / 90 :");
					Reporting.AddLine(" " + tw + " >=  (" + dz + " + " + Wz + ") / 90 (NG)");
				}
			}

			Reporting.AddHeader("Required Strength (Vu)");
			switch (CommonDataStatic.SeismicSettings.Type)
			{
				case ESeismicType.Low:
					Vu = columnStiffener.LPuf + columnStiffener.RPuf - column.ShearForce;
					Reporting.AddLine("= | PufLeft + PufRight - Vus |");
					Reporting.AddLine("= | " + columnStiffener.LPuf + " + " + columnStiffener.RPuf + " - " + column.ShearForce + " |");
					break;
				case ESeismicType.High:
					if (Math.Abs(columnStiffener.LPuf) > 0 && Math.Abs(columnStiffener.RPuf) > 0)
					{
						Vu = 0.8 * (columnStiffener.LPuf + columnStiffener.RPuf) - column.ShearForce;
						Reporting.AddLine("= | 0.8 * (PufLeft + PufRight) - Vus |");
						Reporting.AddLine("= | 0.8 * (" + columnStiffener.LPuf + " + " + columnStiffener.RPuf + ") - " + column.ShearForce + " |");
					}
					else
					{
						Vu = columnStiffener.LPuf + columnStiffener.RPuf - column.ShearForce;
						Reporting.AddLine("= | Puf - Vus| ");
						Reporting.AddLine("= | " + (columnStiffener.LPuf + columnStiffener.RPuf) + " - " + column.ShearForce + " |");
					}
					break;
			}
			Reporting.AddLine("= " + Math.Abs(Vu) + ConstUnit.Force);

			if (columnStiffener.ColumnWebShear_User)
			{
				Reporting.AddHeader("Use Vu = " + Math.Abs(columnStiffener.ColumnWebShearOverride) + ConstUnit.Force + " (User Specified)");
				Vu = columnStiffener.ColumnWebShearOverride;
			}
			Vu = Math.Abs(Vu);
			columnStiffener.ColumnWebShearOverride = Vu;
			//  Panel zone shear strength
			Reporting.AddHeader("Column Web Shear Strength:");
			Py = column.Shape.a * column.Material.Fy;
			if (CommonDataStatic.Preferences.CalcMode == ECalcMode.LRFD)
			{
				Pc = Py;
				Reporting.AddLine("Pc = Py = A * Fy = " + column.Shape.a + " * " + column.Material.Fy + " = " + Py + ConstUnit.Force);
			}
			else
			{
				Pc = 0.6 * Py;
				Reporting.AddLine("Pc = 0.6 * Py = 0.6 * A * Fy = 0.6 * " + column.Shape.a + " * " + column.Material.Fy + " = " + Pc + ConstUnit.Force);
			}

			if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low || CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.OMF)
			{
				// a Low seismic
				if (!CommonDataStatic.SeismicSettings.InelasticPanelZone)
				{
					if (column.P <= 0.4 * Pc)
					{
						FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw;
						Reporting.AddLine("Pr <= 0.4 * Pc");
						Reporting.AddLine(ConstString.PHI + "Rv = " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * d * tw");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw);
					}
					else
					{
						FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1.4F - column.P / Pc);
						Reporting.AddLine("Pr >> 0.4 * Pc");
						Reporting.AddLine(ConstString.PHI + "Rv = " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * d * tw * (1.4 - Pr / Pc)");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw + " * (1.4 - " + column.P + " / " + Pc + ")");
					}
				}
				else
				{
					// inelastic panel zone
					if (column.P <= 0.75 * Pc)
					{
						FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw));
						Reporting.AddLine("Pr <= 0.75 * Pc");
						Reporting.AddLine(ConstString.PHI + "Rv = " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * dc * tw * (1 + 3 * bcf * tcf² / (db * dc * tw))");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw + " * (1 + 3 * " + column.Shape.bf + " * " + column.Shape.tf + "² / (" + Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) + " * " + column.Shape.d + " * " + column.Shape.tw + "))");
					}
					else
					{
						FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw)) * (1.9F - 1.2 * column.P / Py);
						Reporting.AddLine("Pr >> 0.75 * Pc");
						Reporting.AddLine(ConstString.PHI + "Rv = " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * dc * tw * (1 + 3 * bcf * tcf² / (db * dc * tw)) * (1.9 - 1.2 * Pr / Pc)");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw + " * (1 + 3 * " + column.Shape.bf + " * " + column.Shape.tf + "² / (" + Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) + " * " + column.Shape.d + " * " + column.Shape.tw + "))* (1.9 - 1.2 * " + column.P + " / " + Pc + ")");
					}
					if (!CommonDataStatic.SeismicSettings.InelasticPanelZone)
					{
						Reporting.AddHeader("Inelastic deformations of the panel zone was not considered in frame stability analysis.");
					}
				}
			}
			else
			{
				//  b)High seismic with SMF or IMF
				if (column.P <= 0.75 * Pc)
				{
					FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw));
					Reporting.AddLine("Pr <= 0.75 * Pc");
					Reporting.AddLine(ConstString.PHI + "Rv = " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * dc * tw * (1 + 3 * bf * tf² / (db * dc * tw))");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw + " * (1 + 3*" + column.Shape.bf + " * " + column.Shape.tf + "²/(" + Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) + " * " + column.Shape.d + " * " + column.Shape.tw + "))");
				}
				else
				{
					FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw)) * (1.9F - 1.2 * column.P / Pc);
					Reporting.AddLine("Pr >> 0.75 * Pc");
					Reporting.AddLine(ConstString.PHI + "Rv = " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * dc * tw * (1 + 3 * bf * tf² / (db * dc * tw)) * (1.9 - 1.2 * Pr / Pc)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw + " * (1 + 3 * " + column.Shape.bf + " * " + column.Shape.tf + "²/(" + Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) + " * " + column.Shape.d + " * " + column.Shape.tw + "))* (1.9 - 1.2 * " + column.P + " / " + Pc + ")");
				}
				Reporting.AddHeader("Inelastic deformations of the panel zone was considered in frame stability analysis.");

			}
			if (FIRV >= Vu)
			{
				Reporting.AddLine("= " + FIRV + " >= " + Vu + ConstUnit.Force);
				Reporting.AddLine("Doubler Plate Not Required for Strength");
			}
			else
			{
				Reporting.AddLine("= " + FIRV + " << " + Vu + ConstUnit.Force);
				Reporting.AddLine("Doubler Plate Required for Strength");
			}

			Reporting.AddHeader("Shear Buckling of Web:");
			double columnFy = CommonDataStatic.Units == EUnit.Metric ? column.Material.Fy_Metric : column.Material.Fy_US;
			if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
			{
				tp_ShearBuckling = (column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(columnFy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5));
				Reporting.AddLine("Thickness Required = h * (Fy^0.5) / (2.24 * E^0.5) = " + (column.Shape.d - 2 * column.Shape.kdes) + " * (" + columnFy + "^0.5) / (2.24 * " + ConstNum.ELASTICITY + "^0.5)");
			}
			else
			{
				dm = Math.Max(columnStiffener.RDepth, columnStiffener.LDepth);
				ts = columnStiffener.SThickness;
				dc = column.Shape.d;
				tf = column.Shape.tf;
				tp_ShearBuckling = Math.Max((column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(columnFy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5)), (dm - ts + dc - 2 * tf) / 90);
				Reporting.AddLine("Thickness Required = Max([h * (Fy^0.5) / (2.24 * E^0.5)]; [(dm - ts + dc - 2 * tf) / 90])");
				Reporting.AddLine("= Max([" + (column.Shape.d - 2 * column.Shape.kdes) + " * (" + columnFy + "^0.5) / (2.24 * E^0.5)]; [(" + dm + " - " + ts + " + " + dc + " - 2 * " + tf + ") / 90])");
			}
			if (tp_ShearBuckling <= column.Shape.tw)
			{
				Reporting.AddLine("= " + tp_ShearBuckling + " <= " + column.Shape.tw + ConstUnit.Length);
				Reporting.AddLine("Doubler Plate Not Required for Shear Buckling");
			}
			else
			{
				Reporting.AddLine("= " + tp_ShearBuckling + " >> " + column.Shape.tw + ConstUnit.Length);
				Reporting.AddLine("Doubler Plate Required for Shear Buckling");
			}
			if (Vu <= FIRV && tp_ShearBuckling <= column.Shape.tw)
			{
				// Doubler Plate not required
				columnStiffener.DThickness = 0;
				columnStiffener.DThickness2 = 0;
				columnStiffener.DNumberOfPlates = 0;
			}
			else
			{
				// Doubler Plate required
				columnStiffener.DColShr = Vu - FIRV;
				if (columnStiffener.DColShr < 0)
					columnStiffener.DColShr = 0;
				
				tp = columnStiffener.DColShr / columnStiffener.DNumberOfPlates / (ConstNum.FIOMEGA0_9N * 0.6 * columnStiffener.Material.Fy * column.Shape.d); // EQ 4.4-1
				Reporting.AddHeader("Doubler Plate Thickness = " + columnStiffener.DThickness + ConstUnit.Length);
				Reporting.AddHeader("Required for strength = Vudp / (Npl * " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * dc)");
				Reporting.AddLine("= " + columnStiffener.DColShr + " / (" + columnStiffener.DNumberOfPlates + " * " + ConstString.FIOMEGA0_9 + " * 0.6 * " + columnStiffener.Material.Fy + " * " + column.Shape.d + ")");
				if (tp <= columnStiffener.DThickness)
					Reporting.AddCapacityLine("= " + tp + " <= " + columnStiffener.DThickness + ConstUnit.Length + " (OK)", tp / columnStiffener.DThickness, "Required for strength", EMemberType.PrimaryMember);
				else
					Reporting.AddCapacityLine("= " + tp + " >> " + columnStiffener.DThickness + ConstUnit.Length + " (NG)", tp / columnStiffener.DThickness, "Required for strength", EMemberType.PrimaryMember);

				fr = column.Shape.kdet - column.Shape.tf;
				if (CommonDataStatic.Units == EUnit.US)
					e = Math.Pow(3.0 / 32 * fr - Math.Pow(3.0 / 64, 2), 0.5); //  fillet encroachment
				else
					e = Math.Pow(3.0 / 32 * 25.4 * fr - Math.Pow(3.0 / 64 * 25.4, 2), 0.5); //  fillet encroachment
				
				re = NumberFun.Round(e, 16);
				tp_forFilletweld = fr - re; // EQ 4.4-4
				Reporting.AddHeader("Required for fillet weld detail = k - tf - re");
				if (tp_forFilletweld <= columnStiffener.DThickness)
					Reporting.AddLine("= " + column.Shape.kdet + " - " + column.Shape.tf + " - " + re + " = " + tp_forFilletweld + " <= " + columnStiffener.DThickness + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + column.Shape.kdet + " - " + column.Shape.tf + " - " + re + " = " + tp_forFilletweld + " >> " + columnStiffener.DThickness + ConstUnit.Length + " (NG)");

				if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
				{
					tp_ShearBuckling = (column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(columnStiffener.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5)); // EQ 4.4-5
					Reporting.AddHeader("Required for shear buckling = (d - 2 * k) * (Fy)^0.5 / (2.24 * E^0.5)");
					Reporting.AddLine("= (" + column.Shape.d + " - 2 * " + column.Shape.kdes + ") * (" + columnStiffener.Material.Fy + ")^0.5/(2.24 * " + ConstNum.ELASTICITY + "^0.5)");
				}
				else
				{
					dm = Math.Max(columnStiffener.RDepth, columnStiffener.LDepth);
					ts = columnStiffener.SThickness;
					dc = column.Shape.d;
					tf = column.Shape.tf;
					tp_ShearBuckling = Math.Max((column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(columnStiffener.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5)), (dm - ts + dc - 2 * tf) / 90); // EQ 4.4-6
					Reporting.AddHeader("Required for shear buckling = Max([(d - 2 * k) * (Fy)^0.5 / (2.24 * E^0.5)]; [(dm - ts + dc - 2 * tf) / 90])");
					Reporting.AddLine("= Max([" + column.Shape.d + " - 2 * " + column.Shape.kdes + ") * (" + columnStiffener.Material.Fy + ")^0.5 / (2.24 * " + ConstNum.ELASTICITY + "^0.5)];[(" + dm + " - " + ts + " + " + dc + " - 2 * " + tf + ")/90])");
				}
				if (tp_ShearBuckling <= columnStiffener.DThickness)
					Reporting.AddCapacityLine("= " + tp_ShearBuckling + " <= " + columnStiffener.DThickness + ConstUnit.Length + " (OK)", tp_ShearBuckling / columnStiffener.DThickness, "Required for shear buckling", EMemberType.PrimaryMember);
				else
					Reporting.AddCapacityLine("= " + tp_ShearBuckling + " >> " + columnStiffener.DThickness + ConstUnit.Length + " (NG)", tp_ShearBuckling / columnStiffener.DThickness, "Required for shear buckling", EMemberType.PrimaryMember);

				if (columnStiffener.UseExtendedDoublerPlate)
				{
					TenstifForce = Math.Max(columnStiffener.LTenstifForce, columnStiffener.RTenstifForce);
					CompStifForce = Math.Max(columnStiffener.LCompStifForce, columnStiffener.RCompStifForce);
					clip = column.Shape.kdet - column.Shape.tf;
					Reporting.AddHeader("Required to transmit stiffener force: ");
					if (columnStiffener.StiffenerLength == EStiffenerLength.PartialLengthOfWeb)
					{
						// EQ 4.4 - 2
						tp_withStiffener = Math.Max(TenstifForce, CompStifForce) / (ConstNum.FIOMEGA0_9N * 0.6 * columnStiffener.Material.Fy * 2 * Math.Min(2 * (columnStiffener.SLength - clip), column.Shape.d));
						Reporting.AddLine("= Rust / (" + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * 2 * Min(2 * (L - clip), dc))");
						Reporting.AddLine("= " + Math.Max(TenstifForce, CompStifForce) + " / (" + ConstString.FIOMEGA0_9 + " * 0.6 * " + columnStiffener.Material.Fy + " * 2 * Min(2 * (" + columnStiffener.SLength + " - " + clip + "); " + column.Shape.d + "))");
					}
					else
					{
						// EQ 4.4-3
						tp_withStiffener = Math.Max(columnStiffener.LTenstifForce + columnStiffener.RCompStifForce, columnStiffener.RTenstifForce + columnStiffener.LCompStifForce) / (ConstNum.FIOMEGA0_9N * 0.6 * columnStiffener.Material.Fy * 2 * Math.Min(2 * (columnStiffener.SLength - 2 * clip), column.Shape.d));
						Reporting.AddLine("= (Rust1 + Rust2) / (" + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * 2 * Min(2 * (L - 2 * clip), dc))");
						if (columnStiffener.LTenstifForce + columnStiffener.RCompStifForce > columnStiffener.RTenstifForce + columnStiffener.LCompStifForce)
							Reporting.AddLine("= (" + columnStiffener.LTenstifForce + " + " + columnStiffener.RCompStifForce + ") / (" + ConstString.FIOMEGA0_9 + " * 0.6 * " + columnStiffener.Material.Fy + " * 2 * Min(2 * (" + columnStiffener.SLength + " - 2 * " + clip + "); " + column.Shape.d + "))");
						else
							Reporting.AddLine("= (" + columnStiffener.RTenstifForce + " + " + columnStiffener.LCompStifForce + ") / (" + ConstString.FIOMEGA0_9 + " * 0.6 * " + columnStiffener.Material.Fy + " * 2 * Min(2 * (" + columnStiffener.SLength + " - 2 * " + clip + "); " + column.Shape.d + "))");
					}
					if (tp_withStiffener <= columnStiffener.DThickness)
						Reporting.AddCapacityLine("= " + tp_withStiffener + " <= " + columnStiffener.DThickness + ConstUnit.Length + " (OK)", tp_withStiffener / columnStiffener.DThickness, "Required to transmit stiffener force", EMemberType.PrimaryMember);
					else
						Reporting.AddCapacityLine("= " + tp_withStiffener + " >> " + columnStiffener.DThickness + ConstUnit.Length + " (NG)", tp_withStiffener / columnStiffener.DThickness, "Required to transmit stiffener force", EMemberType.PrimaryMember);
				}
				else
					tp_withStiffener = 0;

				columnStiffener.Dpt = CommonCalculations.PlateThickness(Math.Max(Math.Max(tp_ShearBuckling, tp), tp_withStiffener));
				Reporting.AddHeader("Doubler Plate Thickness excluding Fillet Weld Detail:");
				Reporting.AddLine("= " + columnStiffener.Dpt + ConstUnit.Length);
			}
		}

		public static void StiffenerAndDoublerPlateWelds()
		{
			double MinMinWeld;
			double W_Flange1;
			double Rust16 = 0;
			double Rust16p;
			double Rust16c;
			double Rust15;
			double Rust14 = 0;
			double Rust13 = 0;
			double RustMax;
			double Rust13p;
			double Rust13c;
			double Rust12;
			double Rust11;
			double w_minFlangeCompression = 0;
			double clip = 0;
			double rustCompression = 0;
			double rustTension = 0;
			double w_minFlangeTension = 0;
			double Bevel;
			double wmin = 0;
			double w_Flange;
			double teff;
			double minweld;
			double minimumWeld = 0;

			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var stiffener = CommonDataStatic.ColumnStiffener;

			// Doubler Plate to flange weld
			minweld = MinimumWeldForStiff(stiffener.DThickness, column.Shape.tf);
			if (stiffener.DNumberOfPlates > 0)
			{
				if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low || CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.OMF)
				{
					teff = stiffener.DColShr / stiffener.DNumberOfPlates / (ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * column.Shape.d);
					w_Flange = stiffener.DColShr / stiffener.DNumberOfPlates / (ConstNum.FIOMEGA0_75N * 0.707 * 0.6 * stiffener.Weld.Fexx * column.Shape.d);
					minimumWeld = Math.Max(minweld, w_Flange);
					minimumWeld = Math.Max(minimumWeld, teff * 1.4142F);
				}
				else
				{
					teff = stiffener.DColShr / stiffener.DNumberOfPlates / (ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * column.Shape.d);
					w_Flange = 1.7 * stiffener.Material.Fy * teff / stiffener.Weld.Fexx;
					wmin = teff * 1.4142;
					Bevel = column.Shape.kdet - column.Shape.tf;
					if (Bevel < stiffener.DThickness)
						wmin = wmin - (stiffener.DThickness - Bevel);
					minimumWeld = Math.Max(minweld, Math.Max(w_Flange, wmin));
				}
				if (!stiffener.DFlangeWeldSize_User)
					stiffener.DFlangeWeldSize = NumberFun.Round(minimumWeld, 16);

				if (!stiffener.DWebWeldSize_User && stiffener.UseExtendedDoublerPlate || !(stiffener.CompStiff & stiffener.TenStiff))
					stiffener.DWebWeldSize = Math.Min(MinimumWeldForStiff(stiffener.DThickness, column.Shape.tw), stiffener.DThickness - ConstNum.SIXTEENTH_INCH);
			}

			if (!stiffener.SFlangeWeldSize_User && (stiffener.CompStiff || stiffener.TenStiff))
			{
				rustCompression = Math.Max(stiffener.LCompStifForce, stiffener.RCompStifForce);
				rustTension = Math.Max(stiffener.LTenstifForce, stiffener.RTenstifForce);
				if (stiffener.DNumberOfPlates > 0 && stiffener.UseExtendedDoublerPlate)
					clip = Math.Max(column.Shape.kdet - column.Shape.tf, stiffener.DThickness);
				else
					clip = column.Shape.kdet - column.Shape.tf;

				if (!stiffener.SWidth_User)
					stiffener.SWidth = NumberFun.Round((column.Shape.bf - column.Shape.tw - 2 * stiffener.DThickness) / 2, 4);

				// Updated for Descon 8
				w_minFlangeTension = rustTension / (ConstNum.FIOMEGA0_75N * (1.5 * 0.6 * stiffener.Weld.Fexx) * (stiffener.SWidth - clip) * 2 * 1.4142);
				w_minFlangeCompression = rustCompression / (ConstNum.FIOMEGA0_75N * (1.5 * 0.6 * stiffener.Weld.Fexx) * (stiffener.SWidth - clip) * 2 * 1.4142);

				if (!stiffener.TenStiff)
					minimumWeld = w_minFlangeCompression;
				else if (!stiffener.CompStiff)
					minimumWeld = w_minFlangeTension;
				else
					minimumWeld = Math.Max(w_minFlangeTension, w_minFlangeCompression);
				wmin = MinimumWeldForStiff(stiffener.SThickness, column.Shape.tf);
				stiffener.SFlangeWeldSize = NumberFun.Round(Math.Max(wmin, minimumWeld), 16);
			}

			if (stiffener.CompStiff || stiffener.TenStiff)
			{
				clip = column.Shape.kdet - column.Shape.tf;
				if (stiffener.StiffenerLength == EStiffenerLength.PartialLengthOfWeb)
				{
					rustCompression = Math.Max(stiffener.LCompStifForce, stiffener.RCompStifForce);
					Rust11 = ConstNum.FIOMEGA0_9N * stiffener.Material.Fy * 2 * (stiffener.SWidth - clip) * stiffener.SThickness;
					Rust12 = ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * (stiffener.SLength - clip) * 2 * stiffener.SThickness;
					Rust13c = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw;
					Rust13p = ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * column.Shape.d * stiffener.DThickness;
					RustMax = Math.Min(Rust11, Rust12);
					switch (stiffener.DNumberOfPlates)
					{
						case 0:
							Rust13 = Rust13c;
							wmin = MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw);
							break;
						case 1:
							if (stiffener.UseExtendedDoublerPlate)
							{
								Rust13 = Math.Max(Rust13p, Rust13c);
								wmin = Math.Max(MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw), MinimumWeldForStiff(stiffener.SThickness, stiffener.DThickness));
							}
							else
							{
								Rust13 = Rust13c;
								wmin = MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw);
							}
							break;
						case 2:
							if (stiffener.UseExtendedDoublerPlate)
							{
								Rust13 = Rust13p;
								wmin = MinimumWeldForStiff(stiffener.SThickness, stiffener.DThickness);
							}
							else
							{
								Rust13 = Rust13c;
								wmin = MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw);
							}
							break;
					}
					RustMax = Math.Min(RustMax, Rust13);
					rustCompression = Math.Min(rustCompression, RustMax);
					minimumWeld = rustCompression / (1.2728 * stiffener.Weld.Fexx * (stiffener.SLength - clip)); // 4.3-9
					minimumWeld = Math.Max(wmin, minimumWeld);
				}
				else
				{
					rustCompression = stiffener.LCompStifForce + stiffener.RCompStifForce;
					if (rightBeam.MomentConnection == EMomentCarriedBy.NoMoment || leftBeam.MomentConnection == EMomentCarriedBy.NoMoment)
						Rust14 = ConstNum.FIOMEGA0_9N * stiffener.Material.Fy * 2 * (stiffener.SLength - clip) * stiffener.SThickness;
					else
						Rust14 = ConstNum.FIOMEGA0_9N * stiffener.Material.Fy * 4 * (stiffener.SLength - clip) * stiffener.SThickness;

					Rust15 = ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * (stiffener.SLength - 2 * clip) * 2 * stiffener.SThickness;
					Rust16c = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw;
					Rust16p = ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * column.Shape.d * stiffener.DThickness;
					RustMax = Math.Min(Rust14, Rust15);
					switch (stiffener.DNumberOfPlates)
					{
						case 0:
							Rust16 = Rust16c;
							wmin = MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw);
							break;
						case 1:
							if (stiffener.UseExtendedDoublerPlate)
							{
								Rust16 = Math.Max(Rust16p, Rust16c);
								wmin = Math.Max(MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw), MinimumWeldForStiff(stiffener.SThickness, stiffener.DThickness));
							}
							else
							{
								Rust16 = Rust16c;
								wmin = MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw);
							}
							break;
						case 2:
							if (stiffener.UseExtendedDoublerPlate)
							{
								Rust16 = Rust16p;
								wmin = MinimumWeldForStiff(stiffener.SThickness, stiffener.DThickness);
							}
							else
							{
								Rust16 = Rust16c;
								wmin = MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw);
							}
							break;
					}
					RustMax = Math.Min(RustMax, Rust16);
					rustCompression = Math.Min(rustCompression, RustMax);
					minimumWeld = rustCompression / (1.2728F * stiffener.Weld.Fexx * (stiffener.SLength - 2 * clip));
					minimumWeld = Math.Max(wmin, minimumWeld);
				}
			}

			if (!stiffener.SWebWeldSize_User)
				stiffener.SWebWeldSize = NumberFun.Round(minimumWeld, 16);

			if (stiffener.DNumberOfPlates > 0)
			{
				Reporting.AddGoToHeader("Doubler Plate Welds:");
				Reporting.AddLine("Doubler Plate to Flange Weld = " + CommonCalculations.WeldSize(stiffener.DFlangeWeldSize) + ConstUnit.Length);

				teff = stiffener.DColShr / stiffener.DNumberOfPlates / (ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * column.Shape.d);

				minweld = MinimumWeldForStiff(stiffener.DThickness, column.Shape.tf);
				if (minweld <= stiffener.DFlangeWeldSize)
					Reporting.AddLine("Minimum weld size for material thickness = " + CommonCalculations.WeldSize(minweld) + " <= " + CommonCalculations.WeldSize(stiffener.DFlangeWeldSize) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum weld size for material thickness = " + CommonCalculations.WeldSize(minweld) + " >> " + CommonCalculations.WeldSize(stiffener.DFlangeWeldSize) + " (NG)");

				if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low || CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.OMF)
				{
					W_Flange1 = stiffener.DColShr / stiffener.DNumberOfPlates / (ConstNum.FIOMEGA0_75N * 0.707 * 0.6 * stiffener.Weld.Fexx * column.Shape.d);
					w_Flange = Math.Max(W_Flange1, teff * 1.4142);
					Reporting.AddHeader("Weld size to develop doubler plate force:");
					Reporting.AddLine("= Max(Vupl / (" + ConstString.FIOMEGA0_75 + " * 0.707 * 0.6 * Fexx * Column d), teff * 1.4142)");
					Reporting.AddLine("= Max(" + (stiffener.DColShr / stiffener.DNumberOfPlates) + " / (" + ConstString.FIOMEGA0_75 + " * 0.707" + " * 0.6 * " + stiffener.Weld.Fexx + " * " + column.Shape.d + "), " + teff + " * 1.4142)");
					Reporting.AddLine("= Max(" + W_Flange1 + ", " + teff * 1.4142 + ")" + ConstUnit.Length);
				}
				else
				{
					w_Flange = 1.7 * stiffener.Material.Fy * stiffener.DThickness / stiffener.Weld.Fexx;
					Reporting.AddHeader("Weld size to develop doubler plate strength:");
					Reporting.AddLine("= 1.7 * Fy * t / Fexx = 1.7 * " + stiffener.Material.Fy + " * " + stiffener.DThickness + " / " + stiffener.Weld.Fexx);
				}
				if (w_Flange <= stiffener.DFlangeWeldSize)
					Reporting.AddCapacityLine("= " + CommonCalculations.WeldSize(w_Flange) + " <= " + CommonCalculations.WeldSize(stiffener.DFlangeWeldSize) + ConstUnit.Length + " (OK)", w_Flange / stiffener.DFlangeWeldSize, "Weld size to develop doubler plate force", EMemberType.PrimaryMember);
				else
					Reporting.AddCapacityLine("= " + CommonCalculations.WeldSize(w_Flange) + " >> " + CommonCalculations.WeldSize(stiffener.DFlangeWeldSize) + ConstUnit.Length + " (NG)", w_Flange / stiffener.DFlangeWeldSize, "Weld size to develop doubler plate force", EMemberType.PrimaryMember);

				teff = stiffener.DColShr / stiffener.DNumberOfPlates / (ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * column.Shape.d);
				w_Flange = 1.7 * stiffener.Material.Fy * teff / stiffener.Weld.Fexx;
				wmin = teff * 1.4142;
				Bevel = column.Shape.kdet - column.Shape.tf;
				if (Bevel < stiffener.DThickness)
					wmin = wmin - (stiffener.DThickness - Bevel);

				// Doubler Plate to web weld
				if (stiffener.UseExtendedDoublerPlate || !(stiffener.CompStiff & stiffener.TenStiff))
				{
					minweld = MinimumWeldForStiff(stiffener.DThickness, column.Shape.tw);
					MinMinWeld = Math.Min(minweld, stiffener.DThickness - ConstNum.SIXTEENTH_INCH);

					Reporting.AddHeader("Doubler Plate to Web Weld:");
					Reporting.AddLine("Minimum weld size = Min(Wmin,  t - " + ConstNum.SIXTEENTH_INCH + ")");
					if (MinMinWeld <= stiffener.DWebWeldSize)
						Reporting.AddLine("= " + CommonCalculations.WeldSize(MinMinWeld) + " <= " + CommonCalculations.WeldSize(stiffener.DWebWeldSize) + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + CommonCalculations.WeldSize(MinMinWeld) + " >> " + CommonCalculations.WeldSize(stiffener.DWebWeldSize) + ConstUnit.Length + " (NG)");
				}
			}

			if (stiffener.TenStiff || stiffener.CompStiff)
			{
				Reporting.AddGoToHeader("Stiffener Welds:");
				Reporting.AddLine("Stiffener to Flange Weld:");
				minweld = MinimumWeldForStiff(stiffener.SThickness, column.Shape.tf);
				if (minweld <= stiffener.SFlangeWeldSize)
					Reporting.AddLine("Minimum Weld Size = " + minweld + " <= " + stiffener.SFlangeWeldSize + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size = " + minweld + " >> " + stiffener.SFlangeWeldSize + ConstUnit.Length + " (NG)");
			}
			if (stiffener.TenStiff)
			{
				Reporting.AddHeader("Tension Stiffener to Flange Weld:");
				Reporting.AddLine("w_Req. = Rust / (" + ConstString.PHI + " * (1.5 * 0.6 * Fexx) * bs - clip) * 2 * 1.4142)");
				Reporting.AddLine("= " + rustTension + " / (" + ConstString.FIOMEGA0_75 + " * 1.5 * 0.6 * " + stiffener.Weld.Fexx + ") * (" + stiffener.SWidth + " - " + clip + ") * 2 * 1.4142)");
				if (w_minFlangeTension <= stiffener.SFlangeWeldSize)
					Reporting.AddCapacityLine("= " + w_minFlangeTension + " <= " + stiffener.SFlangeWeldSize + ConstUnit.Length + " (OK)", w_minFlangeTension / stiffener.SFlangeWeldSize, "Tension Stiffener to Flange Weld", EMemberType.PrimaryMember);
				else
					Reporting.AddCapacityLine("= " + w_minFlangeTension + " >> " + stiffener.SFlangeWeldSize + ConstUnit.Length + " (NG)", w_minFlangeTension / stiffener.SFlangeWeldSize, "Tension Stiffener to Flange Weld", EMemberType.PrimaryMember);
			}
			if (stiffener.CompStiff)
			{
				Reporting.AddHeader("Compression Stiffener to Flange Weld:");
				Reporting.AddLine("w_Req. = Rust / (" + ConstString.PHI + " * (1.5 * 0.6 * Fexx) * bs - clip) * 2 * 1.4142)");
				Reporting.AddLine("= " + rustCompression + " / (" + ConstString.FIOMEGA0_75 + " * 1.5 * 0.6 * " + stiffener.Weld.Fexx + ") * (" + stiffener.SWidth + " - " + clip + ") * 2 * 1.4142)");
				if (w_minFlangeCompression <= stiffener.SFlangeWeldSize)
					Reporting.AddLine("= " + w_minFlangeCompression + " <= " + stiffener.SFlangeWeldSize + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + w_minFlangeCompression + " >> " + stiffener.SFlangeWeldSize + ConstUnit.Length + " (NG)");
			}

			if (stiffener.CompStiff || stiffener.TenStiff)
			{
				Reporting.AddHeader("Stiffener to Panel Zone Weld:");
				clip = column.Shape.kdet - column.Shape.tf;
				if (stiffener.StiffenerLength == EStiffenerLength.PartialLengthOfWeb)
				{
					rustCompression = Math.Max(Math.Max(stiffener.LTenstifForce, stiffener.RTenstifForce), Math.Max(stiffener.LCompStifForce, stiffener.RCompStifForce));
					Reporting.AddLine("Stiffener Force, Rust = Max(LTRust, RTRust, LCRust, RcRust)");
					Reporting.AddLine("= Max(" + stiffener.LTenstifForce + ";" + stiffener.RTenstifForce + "; " + stiffener.LCompStifForce + "; " + stiffener.RCompStifForce + ") = " + rustCompression + ConstUnit.Force);

					Reporting.AddLine("Welds need to develop only the lesser of Rust");
					Reporting.AddLine("and the minimum of the following forces:");

					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " of stiffeners to flange connection:");
					Rust11 = ConstNum.FIOMEGA0_9N * stiffener.Material.Fy * 2 * (stiffener.SWidth - clip) * stiffener.SThickness;
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * Fy * 2 * (bs - clip) * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + stiffener.Material.Fy + " * 2 * (" + stiffener.SWidth + " - " + clip + ") * " + stiffener.SThickness + " = " + Rust11 + ConstUnit.Force);

					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Shear Strength of stiffener and web interface area,");
					Rust12 = ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * (stiffener.SLength - clip) * 2 * stiffener.SThickness;
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * (bs - clip) * 2 * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + stiffener.Material.Fy + " * (" + stiffener.SLength + " - " + clip + ") * 2 * " + stiffener.SThickness + " = " + Rust12 + ConstUnit.Force);

					Reporting.AddLine("Shear yield strength of the panel zone,");
					Rust13c = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw;
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * Fyc * dc * tw (for column web, if applicable)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw + " = " + Rust13c + ConstUnit.Force);

					Rust13p = ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * column.Shape.d * stiffener.DThickness;
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * Fyp * dc * tp (for doubler plate, if applicable)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + stiffener.Material.Fy + " * " + column.Shape.d + " * " + stiffener.DThickness + " = " + Rust13p + ConstUnit.Force);

					RustMax = Math.Min(Rust11, Rust12);
					switch (stiffener.DNumberOfPlates)
					{
						case 0:
							Rust13 = Rust13c;
							wmin = MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw);
							break;
						case 1:
							if (stiffener.UseExtendedDoublerPlate)
							{
								Rust13 = Math.Max(Rust13p, Rust13c);
								wmin = Math.Max(MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw), MinimumWeldForStiff(stiffener.SThickness, stiffener.DThickness));
							}
							else
							{
								Rust13 = Rust13c;
								wmin = MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw);
							}
							break;
						case 2:
							if (stiffener.UseExtendedDoublerPlate)
							{
								Rust13 = Rust13p;
								wmin = MinimumWeldForStiff(stiffener.SThickness, stiffener.DThickness);
							}
							else
							{
								Rust13 = Rust13c;
								wmin = MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw);
							}
							break;
					}
					RustMax = Math.Min(RustMax, Rust13);
					rustCompression = Math.Min(rustCompression, RustMax);

					Reporting.AddHeader("Weld Design Force (Rust_Weld) = " + rustCompression + ConstUnit.Force);
					if (CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD)
						minimumWeld = rustCompression / (0.8485 * stiffener.Weld.Fexx * (stiffener.SLength - clip));
					else
						minimumWeld = rustCompression / (1.2728 * stiffener.Weld.Fexx * (stiffener.SLength - clip));

					if (wmin <= stiffener.SWebWeldSize)
						Reporting.AddLine("Minimum Weld Size = " + wmin + " <= " + stiffener.SWebWeldSize + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Minimum Weld Size = " + wmin + " >> " + stiffener.SWebWeldSize + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Required weld size for strength");
					if (CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD)
					{
						Reporting.AddLine("= Rust_Weld /(0.8485 * Fexx * (L - clip))");
						Reporting.AddLine("= " + rustCompression + " / (0.8485 * " + stiffener.Weld.Fexx + " * (" + stiffener.SLength + " - " + clip + "))");
					}
					else
					{
						Reporting.AddLine("= Rust_Weld /(1.2728 * Fexx * (L - clip))");
						Reporting.AddLine("= " + rustCompression + " / (1.2728 * " + stiffener.Weld.Fexx + " * (" + stiffener.SLength + " - " + clip + "))");
					}

					if (minimumWeld <= stiffener.SWebWeldSize)
						Reporting.AddLine("= " + minimumWeld + " <= " + stiffener.SWebWeldSize + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + minimumWeld + " >> " + stiffener.SWebWeldSize + ConstUnit.Length + " (NG)");
				}
				else
				{
					// full depth
					rustCompression = Math.Max(stiffener.LTenstifForce + stiffener.RCompStifForce, stiffener.RTenstifForce + stiffener.LCompStifForce);
					Reporting.AddLine("Stiffener Force, Rust = Max[(LTRust + RcRust); (RTRust + LCRust)]");
					Reporting.AddLine("= Max[(" + stiffener.LTenstifForce + " + " + stiffener.RCompStifForce + "); (" + stiffener.RTenstifForce + " + " + stiffener.LCompStifForce + ")] = " + rustCompression + ConstUnit.Force);
					Reporting.AddLine(string.Empty); // Added in Descon 8
					Reporting.AddLine("Welds need to develop only the lesser of Rust and the minimum of the following forces:");

					Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE_STRENGTH + " of stiffeners to flange connection:");
					if (rightBeam.Moment == 0 || leftBeam.Moment == 0)
					{
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * Fy * 2 * (L - clip) * t");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + stiffener.Material.Fy + " * 2 * (" + stiffener.SLength + " - " + clip + ") * " + stiffener.SThickness + " = " + Rust14 + ConstUnit.Force);
					}
					else
					{
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * Fy * 4 * (L - clip) * t");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + stiffener.Material.Fy + " * 4 * (" + stiffener.SLength + " - " + clip + ") * " + stiffener.SThickness + " = " + Rust14 + ConstUnit.Force);
					}
					Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Shear Strength of stiffener and web interface area:");
					Rust15 = ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * (stiffener.SLength - 2 * clip) * 2 * stiffener.SThickness;
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * (bs - 2 * clip) * 2 * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + stiffener.Material.Fy + " * (" + stiffener.SWidth + " - 2 * " + clip + ") * 2 * " + stiffener.SThickness + " = " + Rust15 + ConstUnit.Force);

					Reporting.AddHeader("Shear yield strength of the panel zone,");
					Rust16c = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw;
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * Fyc * dc * tw (for column web, if applicable)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw + " = " + Rust16c + ConstUnit.Force);

					Rust16p = ConstNum.FIOMEGA0_9N * 0.6 * stiffener.Material.Fy * column.Shape.d * stiffener.DThickness;
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * Fyp * dc * tp (for doubler plate, if applicable)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + stiffener.Material.Fy + " * " + column.Shape.d + " * " + stiffener.DThickness + " = " + Rust16p + ConstUnit.Force);

					RustMax = Math.Min(Rust14, Rust15);
					switch (stiffener.DNumberOfPlates)
					{
						case 0:
							Rust16 = Rust16c;
							wmin = MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw);
							break;
						case 1:
							if (stiffener.UseExtendedDoublerPlate)
							{
								Rust16 = Math.Max(Rust16p, Rust16c);
								wmin = Math.Max(MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw), MinimumWeldForStiff(stiffener.SThickness, stiffener.DThickness));
							}
							else
							{
								Rust16 = Rust16c;
								wmin = MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw);
							}
							break;
						case 2:
							if (stiffener.UseExtendedDoublerPlate)
							{
								Rust16 = Rust16p;
								wmin = MinimumWeldForStiff(stiffener.SThickness, stiffener.DThickness);
							}
							else
							{
								Rust16 = Rust16c;
								wmin = MinimumWeldForStiff(stiffener.SThickness, column.Shape.tw);
							}
							break;
					}
					RustMax = Math.Min(RustMax, Rust16);
					rustCompression = Math.Min(rustCompression, RustMax);

					Reporting.AddHeader("Weld Design Force, Rust_Weld = " + rustCompression + ConstUnit.Force);
					if (CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD)
						minimumWeld = rustCompression / (0.8485 * stiffener.Weld.Fexx * (stiffener.SLength - 2 * clip));
					else
						minimumWeld = rustCompression / (1.2728 * stiffener.Weld.Fexx * (stiffener.SLength - 2 * clip));

					if (wmin <= stiffener.SWebWeldSize)
						Reporting.AddLine("Minimum Weld Size = " + wmin + " <= " + stiffener.SWebWeldSize + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Minimum Weld Size = " + wmin + " >> " + stiffener.SWebWeldSize + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Required weld size for strength:");
					if (CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD)
					{
						Reporting.AddLine("= Rust_Weld /(0.8485 * Fexx * (L - 2 * clip))");
						Reporting.AddLine("= " + rustCompression + " / (0.8485 * " + stiffener.Weld.Fexx + " * (" + stiffener.SLength + " - 2 * " + clip + "))");
					}
					else
					{
						Reporting.AddLine("= Rust_Weld /(1.2728 * Fexx * (L - 2 * clip))");
						Reporting.AddLine("= " + rustCompression + " / (1.2728 * " + stiffener.Weld.Fexx + " * (" + stiffener.SLength + " - 2 * " + clip + "))");
					}

					if (minimumWeld <= stiffener.SWebWeldSize)
						Reporting.AddCapacityLine("= " + minimumWeld + " <= " + stiffener.SWebWeldSize + ConstUnit.Length + " (OK)", minimumWeld / stiffener.SWebWeldSize, "Required weld size for strength", EMemberType.PrimaryMember);
					else
						Reporting.AddCapacityLine("= " + minimumWeld + " >> " + stiffener.SWebWeldSize + ConstUnit.Length + " (NG)", minimumWeld / stiffener.SWebWeldSize, "Required weld size for strength", EMemberType.PrimaryMember);
				}
			}
		}

		public static void HSSSideWallShear()
		{
			double diff;
			double LvLtemp;
			double LvRtemp ;
			double oneplateminTh;
			double tp_withStiffener;
			double clip;
			double CompStifForce;
			double TenstifForce;
			double tp_forFilletweld;
			double re;
			double e;
			double fr;
			double tp;
			double tf;
			double dc;
			double ts;
			double dm;
			double tp_ShearBuckling;
			double FIRV;
			double Py;
			double Vu = 0;

			double h_tw = 0;
			double cvFactor;
			double cv = 0;

			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var columnStiffener = CommonDataStatic.ColumnStiffener;

			// Required Panel Zone Shear Strength
			switch (CommonDataStatic.SeismicSettings.Type)
			{
				case ESeismicType.Low:
					Vu = columnStiffener.LPuf + columnStiffener.RPuf - column.ShearForce;
					break;
				case ESeismicType.High:
					if (columnStiffener.LPuf > 0 && columnStiffener.RPuf > 0)
						Vu = 0.8 * (columnStiffener.LPuf + columnStiffener.RPuf) - column.ShearForce;
					else
						Vu = columnStiffener.LPuf + columnStiffener.RPuf - column.ShearForce;
					break;
			}

			if (columnStiffener.ColumnWebShear_User)
				Vu = columnStiffener.ColumnWebShearOverride;
			columnStiffener.ColumnWebShearOverride = Vu;
			if (CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD)
				Py = 0.6 * column.Shape.a * column.Material.Fy;
			else
				Py = column.Shape.a * column.Material.Fy;

			if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low || CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.OMF)
			{
				if (!CommonDataStatic.SeismicSettings.InelasticPanelZone)
				{
					h_tw = (column.Shape.Ht - 3 * column.Shape.tw) / column.Shape.tw;
					cvFactor = Math.Pow(5 * ConstNum.ELASTICITY / column.Material.Fy, 0.5);
					if (h_tw <= cvFactor)
						cv = 1;
					else if ((1.10 * cvFactor) < h_tw && h_tw <= (1.37 * cvFactor))
						cv = cvFactor * 1.10;
					else if (h_tw > (1.37 * cvFactor))
						cv = 1.51 * ConstNum.ELASTICITY * 5 / (column.Material.Fy * Math.Pow(h_tw, 2));

					if (column.P <= 0.4 * Py)
						FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * 2 * (column.Shape.Ht - 3 * column.Shape.tw) * column.Shape.tw * cv;
					else
						FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * 2 * (column.Shape.Ht - 3 * column.Shape.tw) * column.Shape.tw * (1.4 - column.P / Py) * cv;
				}
				else
				{
					if (column.P <= 0.75 * Py)
						FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw));
					else
						FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw)) * (1.9F - 1.2 * column.P / Py);
				}
			}
			else
			{
				if (!CommonDataStatic.SeismicSettings.InelasticPanelZone)
					Reporting.AddLine("Inelastic deformations of the panel zone must be included in the analysis of the stability of the frame.  You have not specified this to be the case. (NG)");
				if (column.P <= 0.75 * Py)
					FIRV = ConstNum.FIOMEGA0_75N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw));
				else
					FIRV = ConstNum.FIOMEGA0_75N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw)) * (1.9F - 1.2 * column.P / Py);
			}
			if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
				tp_ShearBuckling = (column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(column.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5));
			else
			{
				dm = Math.Max(columnStiffener.RDepth, columnStiffener.LDepth);
				ts = columnStiffener.SThickness;
				dc = column.Shape.d;
				tf = column.Shape.tf;
				tp_ShearBuckling = Math.Max((column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(column.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5)), (dm - ts + dc - 2 * tf) / 90);
			}

			if (Vu / 2 <= FIRV && tp_ShearBuckling < column.Shape.tw)
			{
				// Doubler Plate not required
				columnStiffener.DThickness = 0;
				columnStiffener.DThickness2 = 0;
			}
			else
			{
				// Doubler Plate required
				columnStiffener.DColShr = Vu / 2 - FIRV;
				if (columnStiffener.DColShr < 0)
					columnStiffener.DColShr = 0;
				tp = columnStiffener.DColShr / (ConstNum.FIOMEGA0_9N * 0.6 * columnStiffener.Material.Fy * column.Shape.d);
				fr = column.Shape.kdet - column.Shape.tf;
				e = fr * Math.Sqrt(1 - Math.Pow(1 - 1 / (16 * fr), 2));
				re = ((int)Math.Floor(16 * e - ConstNum.SIXTEENTH_INCH)) / 16.0;
				tp_forFilletweld = fr - re;
				if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
					tp_ShearBuckling = (column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(columnStiffener.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5));
				else
				{
					dm = Math.Max(columnStiffener.RDepth, columnStiffener.LDepth);
					ts = columnStiffener.SThickness;
					dc = column.Shape.d;
					tf = column.Shape.tf;
					tp_ShearBuckling = Math.Max((column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(columnStiffener.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5)), (dm - ts + dc - 2 * tf) / 90);
				}
				if (columnStiffener.UseExtendedDoublerPlate)
				{
					TenstifForce = Math.Max(columnStiffener.LTenstifForce, columnStiffener.RTenstifForce);
					CompStifForce = Math.Max(columnStiffener.LCompStifForce, columnStiffener.RCompStifForce);
					clip = column.Shape.kdet - column.Shape.tf;
					if (columnStiffener.StiffenerLength == EStiffenerLength.PartialLengthOfWeb)
						tp_withStiffener = Math.Max(TenstifForce, CompStifForce) / (ConstNum.FIOMEGA0_9N * 0.6 * columnStiffener.Material.Fy * 2 * Math.Min(2 * (columnStiffener.SLength - clip), column.Shape.d));
					else
						tp_withStiffener = Math.Max(columnStiffener.LTenstifForce + columnStiffener.RCompStifForce, columnStiffener.RTenstifForce + columnStiffener.LCompStifForce) / (ConstNum.FIOMEGA0_9N * 0.6 * columnStiffener.Material.Fy * 2 * Math.Min(2 * (columnStiffener.SLength - 2 * clip), column.Shape.d));
				}
				else
					tp_withStiffener = 0;

				oneplateminTh = Math.Max(tp_forFilletweld, Math.Max(tp_ShearBuckling, tp_withStiffener));

				if (!columnStiffener.DThickness_User && !columnStiffener.DNumberOfPlates_User)
				{
					if (tp < oneplateminTh)
					{
						columnStiffener.DThickness = oneplateminTh;
						columnStiffener.DThickness2 = 0;
						columnStiffener.DNumberOfPlates = 1;
					}
					else if (tp <= 0.75)
					{
						columnStiffener.DThickness = tp;
						columnStiffener.DThickness2 = 0;
						columnStiffener.DNumberOfPlates = 1;
					}
					else
					{
						columnStiffener.DThickness = Math.Max(tp / 2, oneplateminTh);
						columnStiffener.DThickness2 = columnStiffener.DThickness;
						columnStiffener.DNumberOfPlates = 2;
					}
					// check col.web thickness to develop stiffener when cwsr=1
					columnStiffener.DThickness = NumberFun.Round(columnStiffener.DThickness, 16);
					columnStiffener.DThickness2 = NumberFun.Round(columnStiffener.DThickness2, 16);
					columnStiffener.DHorizontal = NumberFun.Round(column.Shape.d - 2 * column.Shape.tf, 16);
				}
			}

			if (!columnStiffener.DVertical_User)
			{
				if (columnStiffener.TenStiff && columnStiffener.CompStiff)
				{
					if (columnStiffener.UseExtendedDoublerPlate)
					{
						if (columnStiffener.BeamNearTopOfColumn)
						{
							columnStiffener.DVertical = columnStiffener.RDepth + columnStiffener.RDblextension - columnStiffener.SThickness / 2;
							columnStiffener.LvL = columnStiffener.LDepth + columnStiffener.LDblextension - columnStiffener.SThickness / 2;
						}
						else
						{
							columnStiffener.DVertical = columnStiffener.RDepth + 2 * columnStiffener.RDblextension + 2 * columnStiffener.SThickness / 2;
							columnStiffener.LvL = columnStiffener.LDepth + 2 * columnStiffener.LDblextension + 2 * columnStiffener.SThickness / 2;
						}
					}
					else
					{
						columnStiffener.DVertical = columnStiffener.RDepth - 2 * columnStiffener.SThickness / 2;
						columnStiffener.LvL = columnStiffener.LDepth - 2 * columnStiffener.SThickness / 2;
					}
				}
				else if (columnStiffener.TenStiff)
				{
					if (columnStiffener.UseExtendedDoublerPlate)
					{
						columnStiffener.DVertical = columnStiffener.RDepth + 2 * columnStiffener.RDblextension + columnStiffener.SThickness / 2;
						columnStiffener.LvL = columnStiffener.LDepth + 2 * columnStiffener.LDblextension + columnStiffener.SThickness / 2;
					}
					else
					{
						columnStiffener.DVertical = columnStiffener.RDepth + columnStiffener.RDblextension - columnStiffener.SThickness / 2;
						columnStiffener.LvL = columnStiffener.LDepth + columnStiffener.LDblextension - columnStiffener.SThickness / 2;
					}
				}
				else if (columnStiffener.CompStiff)
				{
					if (columnStiffener.UseExtendedDoublerPlate)
					{
						columnStiffener.DVertical = columnStiffener.RDepth + 2 * columnStiffener.RDblextension + columnStiffener.SThickness / 2;
						columnStiffener.LvL = columnStiffener.LDepth + 2 * columnStiffener.LDblextension + columnStiffener.SThickness / 2;
					}
					else
					{
						columnStiffener.DVertical = columnStiffener.RDepth + columnStiffener.RDblextension - columnStiffener.SThickness / 2;
						columnStiffener.LvL = columnStiffener.LDepth + columnStiffener.LDblextension - columnStiffener.SThickness / 2;
					}
				}
				else
				{
					columnStiffener.DVertical = columnStiffener.RDepth + 2 * columnStiffener.RDblextension;
					columnStiffener.LvL = columnStiffener.LDepth + 2 * columnStiffener.LDblextension;
				}
				if (columnStiffener.LDepth > 0 && columnStiffener.RDepth > 0)
				{
					columnStiffener.LDoublerPlateBottom = leftBeam.WinConnect.Beam.FrontY - columnStiffener.LvL / 2;
					columnStiffener.RDoublerPlateBottom = rightBeam.WinConnect.Beam.FrontY - columnStiffener.DVertical / 2;
					if (columnStiffener.UseExtendedDoublerPlate)
					{
						if (columnStiffener.BeamNearTopOfColumn)
						{
							LvRtemp = columnStiffener.RDepth + 2 * columnStiffener.RDblextension + 2 * columnStiffener.SThickness / 2;
							LvLtemp = columnStiffener.LDepth + 2 * columnStiffener.LDblextension + 2 * columnStiffener.SThickness / 2;
							columnStiffener.LDoublerPlateBottom = leftBeam.WinConnect.Beam.FrontY - LvLtemp / 2;
							columnStiffener.RDoublerPlateBottom = rightBeam.WinConnect.Beam.FrontY - LvRtemp / 2;
							diff = columnStiffener.LDoublerPlateBottom - columnStiffener.RDoublerPlateBottom;
							columnStiffener.LDoublerPlateBottom = Math.Min(columnStiffener.LDoublerPlateBottom, columnStiffener.RDoublerPlateBottom);
							columnStiffener.RDoublerPlateBottom = columnStiffener.LDoublerPlateBottom;
							if (diff > 0)
								columnStiffener.LvL = columnStiffener.LvL + diff;
							else
								columnStiffener.DVertical = columnStiffener.DVertical - diff;
						}
						else
						{
							columnStiffener.LvL = Math.Max(columnStiffener.LvL, columnStiffener.DVertical);
							columnStiffener.DVertical = columnStiffener.LvL;
							columnStiffener.LDoublerPlateBottom = Math.Min(columnStiffener.LDoublerPlateBottom, columnStiffener.RDoublerPlateBottom);
							columnStiffener.RDoublerPlateBottom = columnStiffener.LDoublerPlateBottom;
						}
					}
				}
				else if (columnStiffener.RDepth > 0)
				{
					columnStiffener.LvL = columnStiffener.DVertical;
					columnStiffener.RDoublerPlateBottom = rightBeam.WinConnect.Beam.FrontY - columnStiffener.DVertical / 2;
					columnStiffener.LDoublerPlateBottom = columnStiffener.RDoublerPlateBottom;
				}
				else if (columnStiffener.LDepth > 0)
				{
					columnStiffener.DVertical = columnStiffener.LvL;
					columnStiffener.LDoublerPlateBottom = leftBeam.WinConnect.Beam.FrontY - columnStiffener.LvL / 2;
					columnStiffener.RDoublerPlateBottom = columnStiffener.LDoublerPlateBottom;
				}

				columnStiffener.DVertical = NumberFun.Round(columnStiffener.DVertical, 16);
				columnStiffener.LvL = NumberFun.Round(columnStiffener.LvL, 16);
			}

			// DoublerPlateCheck:
			Reporting.AddHeader("HSS Column Panel Zone Shear:");
			Reporting.AddLine("Required Strength (Vu)");
			switch (CommonDataStatic.SeismicSettings.Type)
			{
				case ESeismicType.Low:
					Vu = columnStiffener.LPuf + columnStiffener.RPuf - column.ShearForce;
					Reporting.AddLine("= PufLeft + PufRight - Vus");
					Reporting.AddLine("= " + columnStiffener.LPuf + " + " + columnStiffener.RPuf + " - " + column.ShearForce);
					break;
				case ESeismicType.High:
					if (columnStiffener.LPuf > 0 && columnStiffener.RPuf > 0)
					{
						Vu = 0.8 * (columnStiffener.LPuf + columnStiffener.RPuf) - column.ShearForce;
						Reporting.AddLine("= 0.8 * (PufLeft + PufRight) - Vus");
						Reporting.AddLine("= 0.8 * (" + columnStiffener.LPuf + " + " + columnStiffener.RPuf + ") - " + column.ShearForce);
					}
					else
					{
						Vu = columnStiffener.LPuf + columnStiffener.RPuf - column.ShearForce;
						Reporting.AddLine("= Puf - Vus");
						Reporting.AddLine("= " + (columnStiffener.LPuf + columnStiffener.RPuf) + " - " + column.ShearForce);
					}
					break;
			}
			Reporting.AddLine("= " + Vu + ConstUnit.Force);

			if (columnStiffener.ColumnWebShear_User)
			{
				Reporting.AddLine("Use Vu = " + columnStiffener.ColumnWebShearOverride + ConstUnit.Force + " (User Specified)");
				Vu = columnStiffener.ColumnWebShearOverride;
			}

			//  Panel zone shear strength
			Reporting.AddHeader("HSS Side Wall Shear Strength:");
			if (CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD)
				Reporting.AddLine("Py = 0.6 * A * Fy = 0.6 * " + column.Shape.a + " * " + column.Material.Fy + " = " + Py + ConstUnit.Force);
			else
				Reporting.AddLine("Py = A * Fy = " + column.Shape.a + " * " + column.Material.Fy + " = " + Py + ConstUnit.Force);

			if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low || CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.OMF)
			{
				// New logic for Descon 8
				if (!CommonDataStatic.SeismicSettings.InelasticPanelZone)
				{
					if (column.P <= 0.4 * Py)
					{
						Reporting.AddLine("Pu <= 0.4 * Py");
						Reporting.AddLine("h/tw = (H - 3 * t) / t = (" + column.Shape.Ht + " - 3 * " + column.Shape.tw + ") / " + column.Shape.tw + " = " + h_tw);
						Reporting.AddLine("Cv = " + cv);
						Reporting.AddLine(ConstString.PHI + " Rv = " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * 2 (H - 3 * t) * t * Cv");
						Reporting.AddLine(" = " + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Material.Fy + " * 2 * (" + column.Shape.Ht + " - 3 * " + column.Shape.tw + ") * " + column.Shape.tw + " * " + cv);
					}
					else
					{
						Reporting.AddLine("Pu >> 0.4 * Py");
						Reporting.AddLine("h/tw = (H - 3 * t) / t = (" + column.Shape.Ht + " - 3 * " + column.Shape.tw + ") / " + column.Shape.tw + " = " + h_tw);
						Reporting.AddLine("Cv = " + cv);
						Reporting.AddLine(ConstString.PHI + " Rv = " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * 2 (H - 3 * t) * t * (1.4 - Pu / Py) * Cv");
						Reporting.AddLine(" = " + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Material.Fy + " * 2 * (" + column.Shape.Ht + " - 3 * " + column.Shape.tw + ") * " + column.Shape.tw + " * (1.4 - " + column.P + " / " + Py + ") * " + cv);
					}
				}
				else
				{
					if (column.P <= 0.75 * Py)
					{
						FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw));
						Reporting.AddLine("Pu <= 0.75 * Py");
						Reporting.AddLine(ConstString.PHI + " Rv = " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * dc * tw * (1 + 3 * bf * tf² / (db * dc * tw))");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw + " * (1 + 3 *" + column.Shape.bf + " * " + column.Shape.tf + "²/(" + Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) + " * " + column.Shape.d + " * " + column.Shape.tw + "))");
					}
					else
					{
						FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw)) * (1.9 - 1.2 * column.P / Py);
						Reporting.AddLine("Pu >> 0.75 * Py");
						Reporting.AddLine(ConstString.PHI + " Rv = " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * dc * tw * (1 + 3 * bf * tf² / (db * dc * tw)) * (1.9 - 1.2 * Pu / Py)");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw + " * (1 + 3 * " + column.Shape.bf + " * " + column.Shape.tf + "² / (" + Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) + " * " + column.Shape.d + " * " + column.Shape.tw + ")) * (1.9 - 1.2 * " + column.P + " / " + Py + ")");
					}
				}
			}
			else
			{
				//  b)High seismic with SMF or IMF
				if (!CommonDataStatic.SeismicSettings.InelasticPanelZone)
				{
					Reporting.AddHeader("Inelastic deformations of the panel zone must be included in the analysis of the stability");
					Reporting.AddHeader("of the SMF and IMF with high seismic loads. User has not specified this to be the case.");
				}
				if (column.P <= 0.75 * Py)
				{
					FIRV = 0.75 * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw));
					Reporting.AddLine("Pu <= 0.75 * Py");
					Reporting.AddLine(ConstString.PHI + "Rv = 0.75 * 0.6 * Fu * dc * tw * (1 + 3 * bf * tf² / (db * dc * tw))");
					Reporting.AddLine("= 0.75 * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw + " * (1 + 3 * " + column.Shape.bf + " * " + column.Shape.tf + "²/(" + Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) + " * " + column.Shape.d + " * " + column.Shape.tw + "))");
				}
				else
				{
					FIRV = 0.75 * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw)) * (1.9 - 1.2 * column.P / Py);
					Reporting.AddLine("Pu >> 0.75 * Py");
					Reporting.AddLine(ConstString.PHI + "Rv = 0.75 * 0.6 * Fu * dc * tw * (1 + 3 * bf * tf² / (db * dc * tw)) * (1.9 - 1.2 * Pu / Py)");
					Reporting.AddLine("= 0.75 * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw + " * (1 + 3 * " + column.Shape.bf + " * " + column.Shape.tf + "²/(" + Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) + " * " + column.Shape.d + " * " + column.Shape.tw + "))* (1.9 - 1.2 * " + column.P + " / " + Py + ")");
				}
			}

			Reporting.AddLine("= " + FIRV + ConstUnit.Force);
			if (FIRV >= Vu)
				Reporting.AddCapacityLine(ConstString.PHI + " Rv = " + FIRV + " >= " + Vu + ConstUnit.Force + " (OK)", Vu / FIRV, "HSS Side Wall Shear Strength", EMemberType.PrimaryMember);
			else
			{
				Reporting.AddCapacityLine(ConstString.PHI + " Rv = " + FIRV + " << " + Vu + ConstUnit.Force + " (NG)", Vu / FIRV, "HSS Side Wall Shear Strength", EMemberType.PrimaryMember);
				Reporting.AddLine("(Reinforcement Required for Strength) (NG)");
			}

			Reporting.AddHeader("Shear Buckling of HSS Side Wall:");
			if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
			{
				tp_ShearBuckling = (column.Shape.d - 2 * column.Shape.kdet) * Math.Sqrt(column.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5));
				Reporting.AddLine("Thickness Required = Tc * (Fy^0.5) / (2.24 * E^0.5) = " + (column.Shape.d - 2 * column.Shape.kdet) + " * (" + column.Material.Fy + "^0.5) / (2.24 * (" + ConstNum.ELASTICITY + ")^0.5)");
			}
			else
			{
				dm = Math.Max(columnStiffener.RDepth, columnStiffener.LDepth);
				ts = columnStiffener.SThickness;
				dc = column.Shape.d;
				tf = column.Shape.tf;
				tp_ShearBuckling = Math.Max((column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(column.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5)), (dm - ts + dc - 2 * tf) / 90);
				Reporting.AddLine("Thickness Required = Max([Tc * (Fy^0.5) / (2.24 * E^0.5)]; [(dm - ts + dc - 2 * tf) / 90])");
				Reporting.AddLine("= Max([" + (column.Shape.d - 2 * column.Shape.kdes) + " * (" + column.Material.Fy + "^0.5) / (2.24 * (" + ConstNum.ELASTICITY + ")^0.5) ; [(" + dm + " - " + ts + " + " + dc + " - 2 * " + tf + ")/90])");
			}
			if (tp_ShearBuckling <= column.Shape.tw)
				Reporting.AddLine("= " + tp_ShearBuckling + " <= " + column.Shape.tw + ConstUnit.Length + " (OK)");
			else
			{
				Reporting.AddLine("= " + tp_ShearBuckling + " >> " + column.Shape.tw + ConstUnit.Length + " (NG)");
				Reporting.AddLine("(Reinforcement Required for Shear Buckling)");
			}
			if (Vu <= 2 * FIRV && tp_ShearBuckling <= column.Shape.tw)
			{
				// Doubler Plate not required
				tp_ShearBuckling = 0;
				tp = 0;
				columnStiffener.DThickness = 0;
				columnStiffener.DThickness2 = 0;
				columnStiffener.DNumberOfPlates = 0;
				Reporting.AddLine("HSS Side Wall Reinforcement Not Required (OK)");
			}
			else
			{
				Reporting.AddLine("HSS Side Wall Reinforcement Required (NG)");
				// Doubler Plate required
				columnStiffener.DColShr = Vu / 2 - FIRV;
				if (columnStiffener.DColShr < 0)
					columnStiffener.DColShr = 0;
				tp = columnStiffener.DColShr / columnStiffener.DNumberOfPlates / (ConstNum.FIOMEGA0_9N * 0.6 * columnStiffener.Material.Fy * column.Shape.d); // EQ 4.4-1

				Reporting.AddHeader("Doubler Plate Thickness = " + columnStiffener.DThickness + ConstUnit.Length);
				Reporting.AddLine("Required for strength = Vudp / (Npl * " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * dc)");
				Reporting.AddLine("= " + columnStiffener.DColShr + " / (" + columnStiffener.DNumberOfPlates + " * " + ConstString.FIOMEGA0_9 + " * 0.6 * " + columnStiffener.Material.Fy + " * " + column.Shape.d + ")");
				if (tp <= columnStiffener.DThickness)
					Reporting.AddLine("= " + tp + " <= " + columnStiffener.DThickness + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + tp + " >> " + columnStiffener.DThickness + ConstUnit.Length + " (NG)");

				fr = column.Shape.kdet - column.Shape.tf;
				e = fr * Math.Sqrt(1 - Math.Pow(1 - 1.0 / (16 * fr), 2));
				re = NumberFun.Round(e - ConstNum.SIXTEENTH_INCH, 16);
				tp_forFilletweld = fr - re; // EQ 4.4-4

				Reporting.AddHeader("Required for fillet weld detail = k - tf - re");
				if (tp_forFilletweld <= columnStiffener.DThickness)
					Reporting.AddLine("= " + column.Shape.kdet + " - " + column.Shape.tf + " - " + re + " = " + tp_forFilletweld + " <= " + columnStiffener.DThickness + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + column.Shape.kdet + " - " + column.Shape.tf + " - " + re + " = " + tp_forFilletweld + " >> " + columnStiffener.DThickness + ConstUnit.Length + " (NG)");

				if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
				{
					tp_ShearBuckling = (column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(columnStiffener.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5)); // EQ 4.4-5
					Reporting.AddLine("Required for shear buckling = (d - 2 * k) * (Fy)^0.5 / (2.24 * E^0.5)");
					Reporting.AddLine("= (" + column.Shape.d + " - 2 * " + column.Shape.kdes + ") * (" + columnStiffener.Material.Fy + ")^0.5/(2.24 * " + ConstNum.ELASTICITY + "^0.5)");
				}
				else
				{
					dm = Math.Max(columnStiffener.RDepth, columnStiffener.LDepth);
					ts = columnStiffener.SThickness;
					dc = column.Shape.d;
					tf = column.Shape.tf;
					tp_ShearBuckling = Math.Max((column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(columnStiffener.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5)), (dm - ts + dc - 2 * tf) / 90); // EQ 4.4-6
					Reporting.AddHeader("Required for shear buckling = Max([(d - 2 * k) * (Fy)^0.5 / (2.24 * E^0.5)], [(dm - ts + dc - 2 * tf) / 90])");
					Reporting.AddLine("= Max([" + column.Shape.d + " - 2 * " + column.Shape.kdes + ") * (" + columnStiffener.Material.Fy + ")^0.5/(2.24 * " + ConstNum.ELASTICITY + "^0.5)], [(" + dm + " - " + ts + " + " + dc + " - 2 * " + tf + ")/90])");
				}
				if (tp_ShearBuckling <= columnStiffener.DThickness)
					Reporting.AddLine("= " + tp_ShearBuckling + " <= " + columnStiffener.DThickness + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + tp_ShearBuckling + " >> " + columnStiffener.DThickness + ConstUnit.Length + " (NG)");

				if (columnStiffener.UseExtendedDoublerPlate)
				{
					TenstifForce = Math.Max(columnStiffener.LTenstifForce, columnStiffener.RTenstifForce);
					CompStifForce = Math.Max(columnStiffener.LCompStifForce, columnStiffener.RCompStifForce);
					clip = column.Shape.kdet - column.Shape.tf;
					Reporting.AddHeader("Required to transmit stiffener force:");
					if (columnStiffener.StiffenerLength == EStiffenerLength.PartialLengthOfWeb)
					{
						// EQ 4.4 - 2
						tp_withStiffener = Math.Max(TenstifForce, CompStifForce) / (ConstNum.FIOMEGA0_9N * 0.6 * columnStiffener.Material.Fy * 2 * Math.Min(2 * (columnStiffener.SLength - clip), column.Shape.d));
						Reporting.AddLine("= Rust /(" + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * 2 * Min(2 * (L - clip), dc))");
						Reporting.AddLine("= " + Math.Max(TenstifForce, CompStifForce) + " / (" + ConstString.FIOMEGA0_9 + " * 0.6 *" + columnStiffener.Material.Fy + " *2*Min(2*(" + columnStiffener.SLength + " - " + clip + "); " + column.Shape.d + "))");
					}
					else
					{
						// EQ 4.4-3
						tp_withStiffener = Math.Max(columnStiffener.LTenstifForce + columnStiffener.RCompStifForce, columnStiffener.RTenstifForce + columnStiffener.LCompStifForce) / (ConstNum.FIOMEGA0_9N * 0.6 * columnStiffener.Material.Fy * 2 * Math.Min(2 * (columnStiffener.SLength - 2 * clip), column.Shape.d));
						Reporting.AddLine("= (Rust1 + Rust2) / (" + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * 2 * Min(2 * (L - 2 * clip), dc))");
						if (columnStiffener.LTenstifForce + columnStiffener.RCompStifForce > columnStiffener.RTenstifForce + columnStiffener.LCompStifForce)
							Reporting.AddLine("= (" + columnStiffener.LTenstifForce + " + " + columnStiffener.RCompStifForce + ") / (" + ConstString.FIOMEGA0_9 + " * 0.6 *" + columnStiffener.Material.Fy + " * 2 * Min(2 * (" + columnStiffener.SLength + " - 2 * " + clip + "); " + column.Shape.d + "))");
						else
							Reporting.AddLine("= (" + columnStiffener.RTenstifForce + " + " + columnStiffener.LCompStifForce + ") / (" + ConstString.FIOMEGA0_9 + " * 0.6 *" + columnStiffener.Material.Fy + " * 2 * Min(2 * (" + columnStiffener.SLength + " - 2 * " + clip + "); " + column.Shape.d + "))");
					}
					if (tp_withStiffener <= columnStiffener.DThickness)
						Reporting.AddLine("= " + tp_withStiffener + " <= " + columnStiffener.DThickness + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + tp_withStiffener + " >> " + columnStiffener.DThickness + ConstUnit.Length + " (NG)");
				}
			}
		}

		public static void SeismicDoublerPlateReport(double R_Mf, double L_Mf, double tp_forFilletweld)
		{
			double tp_withStiffener = 0;
			double clip = 0;
			double CompStifForce = 0;
			double TenstifForce = 0;
			double re = 0;
			double fr = 0;
			double tp = 0;
			double tp_ShearBuckling = 0;
			double tf = 0;
			double dc = 0;
			double ts = 0;
			double dm = 0;
			double FIRV = 0;
			double Pc = 0;
			double Py = 0;
			double Vu = 0;
			double PannelZoneShear = 0;
			double Dbl_PL_Minimum_tp = 0;
			double Limit = 0;
			double Wz = 0;
			double dz = 0;
			double tw = 0;
			double encroach = 0;

			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			Reporting.AddHeader("Column Panel Zone:");
			if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic)
			{
				Reporting.AddHeader("Column Web Thickness (tw) = " + column.Shape.tw + ConstUnit.Length);
				tw = column.Shape.tw;
				if (rightBeam.IsActive && leftBeam.IsActive)
				{
					Reporting.AddLine("Panel Zone Depth (dz) = Max(Rbeam_d - 2 * Rbeam_tf,Lbeam_d - 2 * Lbeam_tf)");
					dz = Math.Max(rightBeam.Shape.d - 2 * rightBeam.Shape.tf, leftBeam.Shape.d - 2 * leftBeam.Shape.tf);
					Reporting.AddLine("= Max( " + rightBeam.Shape.d + " - 2 * " + rightBeam.Shape.tf + " , " + leftBeam.Shape.d + " - 2 * " + leftBeam.Shape.tf + " )");
					Reporting.AddLine("dz = " + dz + ConstUnit.Length);
				}
				else if (rightBeam.IsActive)
				{
					dz = rightBeam.Shape.d;
					Reporting.AddLine("dz = " + dz + ConstUnit.Length);

				}
				else if (leftBeam.IsActive)
				{
					dz = leftBeam.Shape.d;
					Reporting.AddLine("dz = " + dz + ConstUnit.Length);
				}

				Wz = column.Shape.d - 2 * column.Shape.tf;
				Reporting.AddLine("Panel Zone Width (Wz) = Column_d - 2 * column_tf = " + column.Shape.d + " -2 * " + column.Shape.tf);
				Reporting.AddLine("Wz = " + Wz + ConstUnit.Length);
				Limit = (dz + Wz) / 90;
				Reporting.AddLine("(dz + Wz) / 90 = (" + dz + " + " + Wz + " ) / 90 = " + Limit + ConstUnit.Length);
				if (tw >= Limit)
					Reporting.AddLine(" tw >= " + Limit + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine(" tw << " + Limit + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Doubler Plate Minimum Thickness:");
				Dbl_PL_Minimum_tp = Limit;
				Reporting.AddLine("Dbl_PL_Minimum_tp = " + Limit + ConstUnit.Length);
				Reporting.AddLine("See Below for Doubler Plate Required Thickness:");
			}
			Reporting.AddHeader("Required Strength (Vu)");

			if (rightBeam.IsActive && leftBeam.IsActive)
			{
				PannelZoneShear = R_Mf / (rightBeam.Shape.d - rightBeam.Shape.tf) + L_Mf / (leftBeam.Shape.d - leftBeam.Shape.tf);
				Reporting.AddLine("PannelZoneShear = R_Mf / (db - tf) + L_Mf / (db - tf)");
				Reporting.AddLine("= " + R_Mf + " / (" + rightBeam.Shape.d + " - " + rightBeam.Shape.tf + ") + " + L_Mf + " / (" + leftBeam.Shape.d + " - " + leftBeam.Shape.tf + ")");
				Reporting.AddLine("= " + PannelZoneShear + ConstUnit.Force);
			}
			else if (rightBeam.IsActive)
			{
				PannelZoneShear = R_Mf / (rightBeam.Shape.d - rightBeam.Shape.tf);
				Reporting.AddLine("PannelZoneShear = R_Mf / (db - tf)");
				Reporting.AddLine("= " + R_Mf + " / (" + rightBeam.Shape.d + " - " + rightBeam.Shape.tf + ")");
				Reporting.AddLine("= " + PannelZoneShear + ConstUnit.Force);
			}
			else if (leftBeam.IsActive)
			{
				PannelZoneShear = L_Mf / (leftBeam.Shape.d - leftBeam.Shape.tf);
				Reporting.AddLine("PannelZoneShear = L_Mf / (db - tf)");
				Reporting.AddLine("= " + L_Mf + " / (" + leftBeam.Shape.d + " - " + leftBeam.Shape.tf + ")");
				Reporting.AddLine("= " + PannelZoneShear + ConstUnit.Force);
			}

			CommonDataStatic.ColumnStiffener.ColumnWebShearOverride = PannelZoneShear;
			Vu = PannelZoneShear;

			//  Panel zone shear strength
			Reporting.AddHeader("Column Web Shear Strength:");
			Py = column.Shape.a * column.Material.Fy;

			if (CommonDataStatic.Preferences.CalcMode == ECalcMode.LRFD)
			{
				Pc = Py;
				Reporting.AddLine("Pc = Py = A * Fy = " + column.Shape.a + " * " + column.Material.Fy + " = " + Py + ConstUnit.Force);
			}
			else
			{
				Pc = 0.6 * Py;
				Reporting.AddLine("Pc = 0.6 * Py = 0.6 * A * Fy = 0.6 * " + column.Shape.a + " * " + column.Material.Fy + " = " + Pc + ConstUnit.Force);
			}

			if (!CommonDataStatic.SeismicSettings.InelasticPanelZone)
			{
				if (column.P <= 0.4 * Pc)
				{
					FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw;
					Reporting.AddLine("Pr <= 0.4 * Pc");
					Reporting.AddLine(ConstString.PHI + "Rv = " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * d * tw");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw);
				}
				else
				{
					FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1.4 - column.P / Pc);
					Reporting.AddLine("Pr >> 0.4 * Pc");
					Reporting.AddLine(ConstString.PHI + "Rv = " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * d * tw * (1.4 - Pr / Pc)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + "  * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw + " * (1.4 - " + column.P + " / " + Pc + ")");
				}
			}
			else
			{
				if (column.P <= 0.75 * Pc)
				{
					FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw));
					Reporting.AddLine("Pr <= 0.75 * Pc");
					Reporting.AddLine(ConstString.PHI + "Rv = " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * dc * tw * (1 + 3 * bf * tf² / (db * dc * tw))");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw + " * (1 + 3 * " + column.Shape.bf + " * " + column.Shape.tf + "² / (" + Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) + " * " + column.Shape.d + " * " + column.Shape.tw + "))");
				}
				else
				{
					FIRV = ConstNum.FIOMEGA0_9N * 0.6 * column.Material.Fy * column.Shape.d * column.Shape.tw * (1 + 3 * column.Shape.bf * Math.Pow(column.Shape.tf, 2) / (Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) * column.Shape.d * column.Shape.tw)) * (1.9F - 1.2 * column.P / Pc);
					Reporting.AddLine("Pr >> 0.75 * Pc");
					Reporting.AddLine(ConstString.PHI + "Rv = " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * dc * tw * (1 + 3 * bf * tf² / (db * dc * tw)) * (1.9 - 1.2 * Pr / Pc)");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.6 * " + column.Material.Fy + " * " + column.Shape.d + " * " + column.Shape.tw + " * (1 + 3 * " + column.Shape.bf + " * " + column.Shape.tf + "² / (" + Math.Max(rightBeam.Shape.d, leftBeam.Shape.d) + " * " + column.Shape.d + " * " + column.Shape.tw + ")) * (1.9 - 1.2 * " + column.P + " / " + Pc + ")");
				}
			}

			Reporting.AddLine(string.Empty);
			if (FIRV >= Vu)
			{
				Reporting.AddLine("= " + FIRV + " >= " + Vu + ConstUnit.Force);
				Reporting.AddLine("Doubler Plate Not Required for Strength");
			}
			else
			{
				Reporting.AddLine("= " + FIRV + " << " + Vu + ConstUnit.Force);
				Reporting.AddLine("Doubler Plate Required for Strength");
			}

			Reporting.AddHeader("Shear Buckling of Web:");
			dm = Math.Max(CommonDataStatic.ColumnStiffener.RDepth, CommonDataStatic.ColumnStiffener.LDepth);
			ts = CommonDataStatic.ColumnStiffener.SThickness;
			dc = column.Shape.d;
			tf = column.Shape.tf;
			tp_ShearBuckling = (column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(column.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5));
			Reporting.AddLine("Thickness Required = h * (Fy^0.5) / (2.24 * E^0.5)");
			Reporting.AddLine("= " + (column.Shape.d - 2 * column.Shape.kdes) + " * (" + column.Material.Fy + "^0.5) / (2.24 * E^0.5) ");

			if (tp_ShearBuckling <= column.Shape.tw)
			{
				Reporting.AddHeader("= " + tp_ShearBuckling + " <= " + column.Shape.tw + ConstUnit.Length);
				Reporting.AddHeader("Doubler Plate Not Required for Shear Buckling");
			}
			else
			{
				Reporting.AddHeader("= " + tp_ShearBuckling + " >> " + column.Shape.tw + ConstUnit.Length);
				Reporting.AddHeader("Doubler Plate Required for Shear Buckling");
			}

			if (Vu <= FIRV && tp_ShearBuckling <= column.Shape.tw)
			{
				// Doubler Plate not required
				tp_ShearBuckling = 0;
				tp = 0;
				CommonDataStatic.ColumnStiffener.DThickness = 0;
				CommonDataStatic.ColumnStiffener.DThickness2 = 0;
				CommonDataStatic.ColumnStiffener.DNumberOfPlates = 0;
			}
			else
			{
				// Doubler Plate required
				CommonDataStatic.ColumnStiffener.DColShr = Vu - FIRV;
				if (CommonDataStatic.ColumnStiffener.DColShr < 0)
					CommonDataStatic.ColumnStiffener.DColShr = 0;
				tp = CommonDataStatic.ColumnStiffener.DColShr / CommonDataStatic.ColumnStiffener.DNumberOfPlates / (ConstNum.FIOMEGA0_9N * 0.6 * CommonDataStatic.ColumnStiffener.Material.Fy * column.Shape.d); // EQ 4.4-1

				Reporting.AddHeader("Doubler Plate Thickness = " + CommonDataStatic.ColumnStiffener.DThickness + ConstUnit.Length);
				Reporting.AddLine("Required for strength = Vudp / (Npl * " + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * dc)");
				Reporting.AddLine("= " + CommonDataStatic.ColumnStiffener.DColShr + " / (" + CommonDataStatic.ColumnStiffener.DNumberOfPlates + " * " + ConstString.FIOMEGA0_9 + " * 0.6 * " + CommonDataStatic.ColumnStiffener.Material.Fy + " * " + column.Shape.d + ")");
				if (tp <= CommonDataStatic.ColumnStiffener.DThickness)
					Reporting.AddLine("= " + tp + " <= " + CommonDataStatic.ColumnStiffener.DThickness + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + tp + " >> " + CommonDataStatic.ColumnStiffener.DThickness + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Required for fillet weld detail = k - tf - re");
				if (tp_forFilletweld <= CommonDataStatic.ColumnStiffener.DThickness)
					Reporting.AddLine("= " + column.Shape.kdet + " - " + column.Shape.tf + " - " + re + " = " + tp_forFilletweld + " <= " + CommonDataStatic.ColumnStiffener.DThickness + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + column.Shape.kdet + " - " + column.Shape.tf + " - " + re + " = " + tp_forFilletweld + " >> " + CommonDataStatic.ColumnStiffener.DThickness + ConstUnit.Length + " (NG)");

				if (CommonDataStatic.SeismicSettings.Type == ESeismicType.Low)
				{
					tp_ShearBuckling = (column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(CommonDataStatic.ColumnStiffener.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5)); // EQ 4.4-5
					Reporting.AddLine("Required for shear buckling = (d - 2 * k) * (Fy)^0.5 / (2.24 * E^0.5)");
					Reporting.AddLine("= (" + column.Shape.d + " - 2 * " + column.Shape.kdes + ") * (" + CommonDataStatic.ColumnStiffener.Material.Fy + ")^0.5 / (2.24*" + ConstNum.ELASTICITY + "^0.5)");
				}
				else
				{
					dm = Math.Max(CommonDataStatic.ColumnStiffener.RDepth, CommonDataStatic.ColumnStiffener.LDepth);
					ts = CommonDataStatic.ColumnStiffener.SThickness;
					dc = column.Shape.d;
					tf = column.Shape.tf;
					tp_ShearBuckling = Math.Max((column.Shape.d - 2 * column.Shape.kdes) * Math.Sqrt(CommonDataStatic.ColumnStiffener.Material.Fy) / (2.24 * Math.Pow(ConstNum.ELASTICITY, 0.5)), (dm - ts + dc - 2 * tf) / 90); // EQ 4.4-6
					Reporting.AddHeader("Required for shear buckling = max([(d - 2 * k) * (Fy)^0.5 / (2.24 * E^0.5)];[(dm - ts + dc - 2 * tf) / 90])");
					Reporting.AddLine("= MAx([" + column.Shape.d + " - 2 * " + column.Shape.kdes + ") * (" + CommonDataStatic.ColumnStiffener.Material.Fy + ")^0.5 / (2.24 * " + ConstNum.ELASTICITY + "^0.5)], [(" + dm + " - " + ts + " + " + dc + " - 2 * " + tf + ") / 90])");
				}

				if (tp_ShearBuckling <= CommonDataStatic.ColumnStiffener.DThickness)
					Reporting.AddLine("= " + tp_ShearBuckling + " <= " + CommonDataStatic.ColumnStiffener.DThickness + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + tp_ShearBuckling + " >> " + CommonDataStatic.ColumnStiffener.DThickness + ConstUnit.Length + " (NG)");

				if (CommonDataStatic.ColumnStiffener.UseExtendedDoublerPlate)
				{
					TenstifForce = Math.Max(CommonDataStatic.ColumnStiffener.LTenstifForce, CommonDataStatic.ColumnStiffener.RTenstifForce);
					CompStifForce = Math.Max(CommonDataStatic.ColumnStiffener.LCompStifForce, CommonDataStatic.ColumnStiffener.RCompStifForce);
					clip = column.Shape.kdet - column.Shape.tf;
					Reporting.AddHeader("Required to transmit stiffener force: ");
					if (CommonDataStatic.ColumnStiffener.StiffenerLength == EStiffenerLength.PartialLengthOfWeb)
					{
						// EQ 4.4 - 2
						tp_withStiffener = Math.Max(TenstifForce, CompStifForce) / (ConstNum.FIOMEGA0_9N * 0.6 * CommonDataStatic.ColumnStiffener.Material.Fy * 2 * Math.Min(2 * (CommonDataStatic.ColumnStiffener.SLength - clip), column.Shape.d));
						Reporting.AddLine("= Rust / (" + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * 2 * Min(2 * (L - clip), dc))");
						Reporting.AddLine("= " + Math.Max(TenstifForce, CompStifForce) + " / (" + ConstString.FIOMEGA0_9 + " * 0.6 * " + CommonDataStatic.ColumnStiffener.Material.Fy + " * 2 * Min(2 * (" + CommonDataStatic.ColumnStiffener.SLength + " - " + clip + "); " + column.Shape.d + "))");
					}
					else
					{
						// EQ 4.4-3
						tp_withStiffener = Math.Max(CommonDataStatic.ColumnStiffener.LTenstifForce + CommonDataStatic.ColumnStiffener.RCompStifForce, CommonDataStatic.ColumnStiffener.RTenstifForce + CommonDataStatic.ColumnStiffener.LCompStifForce) / (ConstNum.FIOMEGA0_9N * 0.6 * CommonDataStatic.ColumnStiffener.Material.Fy * 2 * Math.Min(2 * (CommonDataStatic.ColumnStiffener.SLength - 2 * clip), column.Shape.d));
						Reporting.AddLine("= (Rust1 + Rust2) / (" + ConstString.FIOMEGA0_9 + " * 0.6 * Fy * 2 * Min(2 * (L - 2 * clip), dc))");
						if (CommonDataStatic.ColumnStiffener.LTenstifForce + CommonDataStatic.ColumnStiffener.RCompStifForce > CommonDataStatic.ColumnStiffener.RTenstifForce + CommonDataStatic.ColumnStiffener.LCompStifForce)
							Reporting.AddLine("= (" + CommonDataStatic.ColumnStiffener.LTenstifForce + " + " + CommonDataStatic.ColumnStiffener.RCompStifForce + ") / (" + ConstString.FIOMEGA0_9 + " * 0.6 *" + CommonDataStatic.ColumnStiffener.Material.Fy + " *2*Min(2*(" + CommonDataStatic.ColumnStiffener.SLength + " - 2 * " + clip + "); " + column.Shape.d + "))");
						else
							Reporting.AddLine("= (" + CommonDataStatic.ColumnStiffener.RTenstifForce + " + " + CommonDataStatic.ColumnStiffener.LCompStifForce + ") / (" + ConstString.FIOMEGA0_9 + " * 0.6 *" + CommonDataStatic.ColumnStiffener.Material.Fy + " *2*Min(2*(" + CommonDataStatic.ColumnStiffener.SLength + " - 2 * " + clip + "); " + column.Shape.d + "))");
					}
					if (tp_withStiffener <= CommonDataStatic.ColumnStiffener.DThickness)
						Reporting.AddLine("= " + tp_withStiffener + " <= " + CommonDataStatic.ColumnStiffener.DThickness + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + tp_withStiffener + " >> " + CommonDataStatic.ColumnStiffener.DThickness + ConstUnit.Length + " (NG)");
				}
				else
					tp_withStiffener = 0;

				CommonDataStatic.ColumnStiffener.Dpt = CommonCalculations.PlateThickness(Math.Max(Math.Max(tp_ShearBuckling, tp), tp_withStiffener));
				Reporting.AddHeader("Doubler Plate Thickness excluding Fillet Weld Detail:");
				Reporting.AddHeader("= " + CommonDataStatic.ColumnStiffener.Dpt + ConstUnit.Length);
			}
		}
	}
}