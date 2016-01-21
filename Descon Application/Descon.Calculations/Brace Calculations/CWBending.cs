using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class CWBending
	{
		internal static void CalcCWBending(EMemberType memberType)
		{
			double H_Max;
			double Mom;
			double ecc;
			double ShStress = 0;
			double eBottom;
			double yBottom = 0;
			double eTop = 0;
			double L = 0;
			double ortasi = 0;
			double H;
			double Wc;
			double conLength = 0;
			double Moment2;
			double V;
			double Moment1;
			double FhorizMotherside;
			double FhorizM;
			double ShearStress;
			double FiMn;
			double Moment = 0;
			double Fhoriz = 0;
			double conLengthOS = 0;
			double spanL;

			DetailData otherSide = null;
			var component = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var shearClipAngle = component.WinConnect.ShearClipAngle;

			bool componentIsBrace = (memberType == EMemberType.UpperRight ||
			                         memberType == EMemberType.LowerRight ||
			                         memberType == EMemberType.UpperLeft ||
			                         memberType == EMemberType.LowerLeft);

			switch (memberType)
			{
				case EMemberType.RightBeam:
					if (CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive)
						otherSide = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
					break;
				case EMemberType.UpperRight:
					if (CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].IsActive)
						otherSide = CommonDataStatic.DetailDataDict[EMemberType.UpperLeft];
					break;
				case EMemberType.LowerRight:
					if (CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].IsActive)
						otherSide = CommonDataStatic.DetailDataDict[EMemberType.LowerLeft];
					break;
				case EMemberType.LeftBeam:
					if (CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive)
						otherSide = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
					break;
				case EMemberType.UpperLeft:
					if (CommonDataStatic.DetailDataDict[EMemberType.UpperRight].IsActive)
						otherSide = CommonDataStatic.DetailDataDict[EMemberType.UpperRight];
					break;
				case EMemberType.LowerLeft:
					if (CommonDataStatic.DetailDataDict[EMemberType.LowerRight].IsActive)
						otherSide = CommonDataStatic.DetailDataDict[EMemberType.LowerRight];
					break;
			}
			if (!componentIsBrace)
			{
				Reporting.AddHeader("Column Web Bending and out of Plane Shear");

				if (MiscMethods.IsBeam(memberType))
				{
					Reporting.AddLine("WARNING");
					Reporting.AddLine("When the left and right side connectors are offset");
					Reporting.AddLine("from one another, or when their lengths are different,");
					Reporting.AddLine("the following calculations may have to be revised");
					Reporting.AddLine("by the user outside the program.");
				}
				else
				{
					switch (memberType)
					{
						case EMemberType.RightBeam:
							if (CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].KBrace |
							    CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].KneeBrace |
							    CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].KneeBrace)
							{
								Reporting.AddLine("See above warning");
							}
							break;
						case EMemberType.LeftBeam:
							if (CommonDataStatic.DetailDataDict[EMemberType.UpperRight].KBrace |
							    CommonDataStatic.DetailDataDict[EMemberType.UpperRight].KneeBrace |
							    CommonDataStatic.DetailDataDict[EMemberType.LowerRight].KneeBrace)
							{
								Reporting.AddLine("See above warning");
							}
							break;
					}
				}

				switch (component.GussetToColumnConnection)
				{
					case EBraceConnectionTypes.ClipAngle:
					case EBraceConnectionTypes.EndPlate:
						spanL = column.Shape.d - 2 * column.Shape.kdes;
						if (otherSide != null)
						{
							if (otherSide.GussetToColumnConnection == EBraceConnectionTypes.SinglePlate)
								conLengthOS = otherSide.WinConnect.ShearWebPlate.Length;
							else if (otherSide.GussetToColumnConnection == EBraceConnectionTypes.DirectlyWelded)
								conLengthOS = otherSide.BraceConnect.Gusset.VerticalForceColumn;
						}
						if (otherSide == null)
						{
							Fhoriz = shearClipAngle.ForceX / shearClipAngle.BoltOnGusset.NumberOfBolts;

							Reporting.AddLine("H = Fx / n");
							Reporting.AddLine(String.Format("{0} = {1} / {2}", Fhoriz, shearClipAngle.ForceX, shearClipAngle.BoltOnGusset.NumberOfBolts));

							Reporting.AddLine("Moment (M) = H * (Wc - g) / 4");
							Reporting.AddLine(String.Format("{0} = {1} * ({2} - {3}) / 4", Moment, Fhoriz, spanL, column.Shape.g2));

							FiMn = ConstNum.FIOMEGA0_9N * column.Material.Fy * Math.Pow(column.Shape.tw, 2) * (shearClipAngle.BoltOnGusset.SpacingLongDir - column.BoltBrace.HoleDiameterSTD) / 4;

							Reporting.AddHeader("Bending Strength");
							Reporting.AddLine("FiMn = " + ConstString.FIOMEGA0_9 + " * Fy * tw² * (S - dh) / 4");
							Reporting.AddLine(String.Format("{0} = {1} * {2} * {3}² * ({4} - {5}) / 4",
								FiMn, ConstString.FIOMEGA0_9, column.Material.Fy, column.Shape.tw, shearClipAngle.BoltOnGusset.SpacingLongDir, column.BoltBrace.HoleDiameterSTD));

							//TODO: There is currently no replacement for Shape.g2
							Moment = Fhoriz * (spanL - column.Shape.g2) / 4;

							if (FiMn >= Moment)
								Reporting.AddLine(String.Format("{0} >= {1} {2} (OK)", FiMn, Moment, ConstUnit.Length));
							else
								Reporting.AddLine(String.Format("{0} << {1} {2} (NG)", FiMn, Moment, ConstUnit.Length));

							ShearStress = Fhoriz / (2 * column.Shape.tw * (shearClipAngle.BoltOnGusset.SpacingLongDir - column.BoltBrace.HoleDiameterSTD));

							Reporting.AddHeader("Shear Stress");
							Reporting.AddLine("fv = H / (2 * tw * (s - dh))");
							Reporting.AddLine(String.Format("{0} = {1} / (2 * {2} * ({3} - {4}))",
								ShearStress, Fhoriz, column.Shape.tw, shearClipAngle.BoltOnGusset.SpacingLongDir, column.BoltBrace.HoleDiameterSTD));

							if (ShearStress <= ConstNum.FIOMEGA1_0N * 0.6 * column.Material.Fy)
								Reporting.AddLine(String.Format("ShearStress {0} <= " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy {1} (OK)", ShearStress, column.Material.Fy));
							else
								Reporting.AddLine(String.Format("ShearStress {0} >> " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy {1} (NG)", ShearStress, column.Material.Fy));
						}
						else
						{
							if (otherSide.GussetToColumnConnection == EBraceConnectionTypes.ClipAngle || otherSide.GussetToColumnConnection == EBraceConnectionTypes.EndPlate)
							{
								Fhoriz = Math.Abs(shearClipAngle.ForceX / shearClipAngle.BoltOnGusset.NumberOfBolts - otherSide.WinConnect.ShearClipAngle.ForceX / shearClipAngle.BoltOnGusset.NumberOfBolts);

								Reporting.AddLine("H = Abs(Fx / n - Fx' / n')");
								Reporting.AddLine(String.Format("{0} = Abs({1} / {2} - {3} / {4})",
									Fhoriz, shearClipAngle.ForceX, shearClipAngle.BoltOnGusset.NumberOfBolts, otherSide.WinConnect.ShearClipAngle.ForceX, shearClipAngle.BoltOnGusset.NumberOfBolts));

								Moment = Fhoriz * (spanL - column.Shape.g2) / 4;

								Reporting.AddLine("Moment (M) = H * (Wc - g) / 4");
								Reporting.AddLine(String.Format("{0} = {1} * ({2} - {3}) / 4", Moment, Fhoriz, spanL, column.Shape.g2));

								FiMn = ConstNum.FIOMEGA0_9N * column.Material.Fy * Math.Pow(column.Shape.tw, 2) * (shearClipAngle.BoltOnGusset.SpacingLongDir - column.BoltBrace.HoleDiameterSTD) / 4;

								Reporting.AddHeader("Bending Strength");
								Reporting.AddLine("FiMn = " + ConstString.FIOMEGA0_9 + " * Fy * tw² * (S - dh) / 4");
								Reporting.AddLine(String.Format("{0} = {1} * {2} * {3}² * ({4} - {5}) / 4",
									FiMn, ConstString.FIOMEGA0_9, column.Material.Fy, column.Shape.tw, shearClipAngle.BoltOnGusset.SpacingLongDir, column.BoltBrace.HoleDiameterSTD));

								if (FiMn >= Moment)
									Reporting.AddLine(String.Format("{0} >= {1} {2} (OK)", FiMn, Moment, ConstUnit.Length));
								else
									Reporting.AddLine(String.Format("{0} << {1} {2} (NG)", FiMn, Moment, ConstUnit.Length));

								ShearStress = Fhoriz / (2 * column.Shape.tw * (shearClipAngle.BoltOnGusset.SpacingLongDir - column.BoltBrace.HoleDiameterSTD));

								Reporting.AddHeader("Shear Stress");
								Reporting.AddLine("fv = H / (2 * tw * (s - dh))");
								Reporting.AddLine(String.Format("{0} = {1} / (2 * {2} * ({3} - {4}))",
									ShearStress, Fhoriz, column.Shape.tw, shearClipAngle.BoltOnGusset.SpacingLongDir, column.BoltBrace.HoleDiameterSTD));

								if (ShearStress <= ConstNum.FIOMEGA1_0N * 0.6 * column.Material.Fy)
									Reporting.AddLine(String.Format("Shear Stress {0} <= " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy {1} (OK)", ShearStress, column.Material.Fy));
								else
									Reporting.AddLine(String.Format("Shear Stress {0} >> " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy {1} (NG)", ShearStress, column.Material.Fy));
							}
							else if (otherSide.GussetToColumnConnection == EBraceConnectionTypes.SinglePlate || otherSide.GussetToColumnConnection == EBraceConnectionTypes.DirectlyWelded)
							{
								FhorizM = shearClipAngle.ForceX / shearClipAngle.BoltOnGusset.NumberOfBolts;

								Reporting.AddLine("H_Angles = Fx / n");
								Reporting.AddLine(String.Format("{0} = {1} / {2}", Fhoriz, shearClipAngle.ForceX, shearClipAngle.BoltOnGusset.NumberOfBolts));

								FhorizMotherside = otherSide.WinConnect.ShearClipAngle.ForceX / conLengthOS * shearClipAngle.BoltOnGusset.SpacingLongDir;

								Reporting.AddLine("H_Plate = Fx' / Lp * s");
								Reporting.AddLine(String.Format("{0} = {1} / {2} * {3}", FhorizMotherside, otherSide.WinConnect.ShearClipAngle.ForceX, conLengthOS, shearClipAngle.BoltOnGusset.SpacingLongDir));

								Moment1 = Math.Abs((spanL - column.Shape.g2) * (FhorizM - FhorizMotherside) / 4);

								Reporting.AddHeader("Bending and shear at reduced section");
								Reporting.AddLine("M = Abs((Wc - g) * (H_Angles - H_Plate)) / 4");
								Reporting.AddLine(String.Format("{0} = Abs(({1} - {2}) * ({3} - {4})) / 4",
									Moment1, spanL, column.Shape.g2, FhorizM, FhorizMotherside));

								FiMn = ConstNum.FIOMEGA0_9N * column.Material.Fy * Math.Pow(column.Shape.tw, 2) * (shearClipAngle.BoltOnGusset.SpacingLongDir - column.BoltBrace.HoleDiameterSTD) / 4;

								Reporting.AddHeader("Bending Strength");
								Reporting.AddLine("FiMn = " + ConstString.FIOMEGA0_9 + " * Fy * tw² * (S - dh) / 4");
								Reporting.AddLine(String.Format("{0} = {1} * {2} * {3}² * ({4} - {5}) / 4",
									FiMn, ConstString.FIOMEGA0_9, column.Material.Fy, column.Shape.tw, shearClipAngle.BoltOnGusset.SpacingLongDir, column.BoltBrace.HoleDiameterSTD));

								if (FiMn >= Moment1)
									Reporting.AddLine(String.Format("{0} >= {1} {2} (OK)", FiMn, Moment1, ConstUnit.Length));
								else
									Reporting.AddLine(String.Format("{0} << {1} {2} (NG)", FiMn, Moment1, ConstUnit.Length));

								V = Math.Max(Math.Abs(FhorizMotherside - FhorizM), Math.Abs(FhorizMotherside)) / 2;

								Reporting.AddLine("V = Max(Abs((H_Plate - H_Angles)), Abs((H_Plate)) ) / 2");
								Reporting.AddLine(String.Format("{0} = Max(Abs(({1} - {2})), Abs(({1}))) / 2", V, FhorizMotherside, FhorizM));

								ShearStress = V / (column.Shape.tw * (shearClipAngle.BoltOnGusset.SpacingLongDir - column.BoltBrace.HoleDiameterSTD));

								Reporting.AddLine("fv = V / (tw * (s - dh))");
								Reporting.AddLine(String.Format("{0} = {1} / ({2} * ({3} - {4}))",
									ShearStress, V, column.Shape.tw, shearClipAngle.BoltOnGusset.SpacingLongDir, column.BoltBrace.HoleDiameterSTD));

								if (ShearStress <= ConstNum.FIOMEGA1_0N * 0.6 * column.Material.Fy)
									Reporting.AddLine(String.Format("Shear Stress {0} <= " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy {1} (OK)", ShearStress, column.Material.Fy));
								else
									Reporting.AddLine(String.Format("Shear Stress {0} >> " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy {1} (NG)", ShearStress, column.Material.Fy));

								Moment2 = Math.Abs((spanL * FhorizMotherside + (column.Shape.g2 - spanL) * FhorizM) / 4);

								Reporting.AddHeader("Bending at mid-section");
								Reporting.AddLine("M = Abs((Wc * H_Plate + (g - Wc) * H_Angles)) / 4");
								Reporting.AddLine(String.Format("{0} = Abs(({1} * {2} + ({3} - {2}) * {4})) / 4",
									Moment2, spanL, FhorizMotherside, column.Shape.g2, FhorizM));

								FiMn = ConstNum.FIOMEGA0_9N * column.Material.Fy * Math.Pow(column.Shape.tw, 2) * shearClipAngle.BoltOnGusset.SpacingLongDir / 4;

								Reporting.AddHeader("Bending Strength");
								Reporting.AddLine("FiMn = " + ConstString.FIOMEGA0_9 + " * Fy * tw² * S / 4");
								Reporting.AddLine(String.Format("{0} = {1} * {2} * {3}² * {4} / 4",
									FiMn, ConstString.FIOMEGA0_9, column.Material.Fy, column.Shape.tw, shearClipAngle.BoltOnGusset.SpacingLongDir));

								if (FiMn >= Moment2)
									Reporting.AddLine(String.Format("{0} >= {1} {2} (OK)", FiMn, Moment2, ConstUnit.Length));
								else
									Reporting.AddLine(String.Format("{0} << {1} {2} (NG)", FiMn, Moment2, ConstUnit.Length));
							}
						}
						break;
					case EBraceConnectionTypes.SinglePlate:
						spanL = column.Shape.d - 2 * column.Shape.kdes;

						if (component.GussetToColumnConnection == EBraceConnectionTypes.SinglePlate)
							conLength = component.WinConnect.ShearWebPlate.Length;
						else if (component.GussetToColumnConnection == EBraceConnectionTypes.DirectlyWelded)
							conLength = component.BraceConnect.Gusset.VerticalForceColumn;

						if (otherSide != null)
						{
							if (otherSide.GussetToColumnConnection == EBraceConnectionTypes.SinglePlate)
								conLengthOS = otherSide.WinConnect.ShearWebPlate.Length;
							else if (otherSide.GussetToColumnConnection == EBraceConnectionTypes.DirectlyWelded)
								conLengthOS = otherSide.BraceConnect.Gusset.VerticalForceColumn;
						}

						if (otherSide == null)
						{
							Fhoriz = shearClipAngle.ForceX / conLength;

							Reporting.AddLine("H = Fx / L");
							Reporting.AddLine(String.Format("{0} = {1} / {2}", Fhoriz, shearClipAngle.ForceX, conLength));

							Moment = Fhoriz * spanL / 4;

							Reporting.AddLine("Moment (M) = H * Wc / 4");
							Reporting.AddLine(String.Format("{0} = {1} * {2} / 4", Moment, Fhoriz, spanL));

							FiMn = ConstNum.FIOMEGA0_9N * column.Material.Fy * Math.Pow(column.Shape.tw, 2) / 4;

							Reporting.AddHeader("Bending Strength:");
							Reporting.AddLine("FiMn = " + ConstString.FIOMEGA0_9 + " * Fy * tw² / 4");
							Reporting.AddLine(String.Format("{0} = {1} * {2} * {3}² / 4", FiMn, ConstString.FIOMEGA0_9, column.Material.Fy, column.Shape.tw));

							if (FiMn >= Moment)
								Reporting.AddLine(String.Format("{0} >= {1} {2} (OK)", FiMn, Moment, ConstUnit.Length));
							else
								Reporting.AddLine(String.Format("{0} << {1} {2} (NG)", FiMn, Moment, ConstUnit.Length));

							ShearStress = Fhoriz / (2 * column.Shape.tw);

							Reporting.AddHeader("Shear Stress");
							Reporting.AddLine("fv = H / (2 * tw)");
							Reporting.AddLine(String.Format("{0} = {1} / (2 * {2})", ShearStress, Fhoriz, column.Shape.tw));

							if (ShearStress <= ConstNum.FIOMEGA1_0N * 0.6 * column.Material.Fy)
								Reporting.AddLine(String.Format("Shear Stress {0} <= " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy {1} (OK)", ShearStress, column.Material.Fy));
							else
								Reporting.AddLine(String.Format("Shear Stress {0} >> " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy {1} (NG)", ShearStress, column.Material.Fy));
						}
						else
						{
							if (otherSide.GussetToColumnConnection == EBraceConnectionTypes.SinglePlate || otherSide.GussetToColumnConnection == EBraceConnectionTypes.DirectlyWelded)
							{
								Fhoriz = Math.Abs(shearClipAngle.ForceX / conLength - otherSide.WinConnect.ShearClipAngle.ForceX / conLengthOS);

								Reporting.AddLine("H = Abs(Fx / L - Fx' / L')");
								Reporting.AddLine(String.Format("{0} = {1} / {2} = {3} / {4}",
									Fhoriz, shearClipAngle.ForceX, conLength, otherSide.WinConnect.ShearClipAngle.ForceX, conLengthOS));

								Moment = Fhoriz * spanL / 4;

								Reporting.AddLine("H = Fx / L");
								Reporting.AddLine(String.Format("{0} = {1} / {2}", Fhoriz, shearClipAngle.ForceX, conLength));

								FiMn = ConstNum.FIOMEGA0_9N * column.Material.Fy * Math.Pow(column.Shape.tw, 2) / 4;

								Reporting.AddHeader("Bending Strength:");
								Reporting.AddLine("FiMn = FiOmega0_9 * Fy * tw² / 4");
								Reporting.AddLine(String.Format("{0} = {1} * {2} * {3}² / 4", FiMn, ConstString.FIOMEGA0_9, column.Material.Fy, column.Shape.tw));

								if (FiMn >= Moment)
									Reporting.AddLine(String.Format("{0} >= {1} {2} (OK)", FiMn, Moment, ConstUnit.Length));
								else
									Reporting.AddLine(String.Format("{0} << {1} {2} (NG)", FiMn, Moment, ConstUnit.Length));

								Reporting.AddHeader("Shear Stress:");
								ShearStress = Fhoriz / (2 * column.Shape.tw);
								Reporting.AddLine("fv = H / (2 * tw)");
								Reporting.AddLine(String.Format("{0} = {1} / (2 * {2})", ShearStress, Fhoriz, column.Shape.tw));

								if (ShearStress <= ConstNum.FIOMEGA1_0N * 0.6 * column.Material.Fy)
									Reporting.AddLine(String.Format("Shear Stress {0} <= " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy {1} (OK)", ShearStress, column.Material.Fy));
								else
									Reporting.AddLine(String.Format("Shear Stress {0} >> " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy {1} (NG)", ShearStress, column.Material.Fy));
							}
							else if (otherSide.GussetToColumnConnection == EBraceConnectionTypes.ClipAngle || otherSide.GussetToColumnConnection == EBraceConnectionTypes.EndPlate)
							{
								FhorizMotherside = otherSide.WinConnect.ShearClipAngle.ForceX / shearClipAngle.BoltOnGusset.NumberOfBolts;

								Reporting.AddLine("H_Angles = Fx / n");
								Reporting.AddLine(String.Format("{0} = {1} / {2}", FhorizMotherside, otherSide.WinConnect.ShearClipAngle.ForceX, shearClipAngle.BoltOnGusset.NumberOfBolts));

								FhorizM = shearClipAngle.ForceX / conLength * shearClipAngle.BoltOnGusset.SpacingLongDir;

								Reporting.AddLine("H_Plate = Fx' / L * s");
								Reporting.AddLine(String.Format("{0} = {1} / {2} * {3}",
									FhorizM, shearClipAngle.ForceX, conLength, shearClipAngle.BoltOnGusset.SpacingLongDir));

								Moment1 = Math.Abs((spanL - column.Shape.g2) * (FhorizM - FhorizMotherside) / 4);

								Reporting.AddHeader("Bending and shear at reduced section");
								Reporting.AddLine("M = Abs(Wc - g) * (H_Angles - H_Plate)) / 4");
								Reporting.AddLine(String.Format("{0} = Abs({1} - {2}) * {3} - {4})) / 4",
									Moment1, spanL, column.Shape.g2, FhorizM, FhorizMotherside));

								FiMn = ConstNum.FIOMEGA0_9N * column.Material.Fy * Math.Pow(column.Shape.tw, 2) * (shearClipAngle.BoltOnGusset.SpacingLongDir - column.BoltBrace.HoleDiameterSTD) / 4;

								Reporting.AddHeader("Bending Strength:");
								Reporting.AddLine("FiMn = " + ConstString.FIOMEGA0_9 + " * Fy * tw² * (S - dh) / 4");
								Reporting.AddLine(String.Format("{0} = {1} * {2} * {3}² * ({4} - {5}) / 4",
									FiMn, ConstString.FIOMEGA0_9, column.Material.Fy, column.Shape.tw, shearClipAngle.BoltOnGusset.SpacingLongDir, column.BoltBrace.HoleDiameterSTD));

								if (FiMn >= Moment1)
									Reporting.AddLine(String.Format("{0} >= {1} {2} (OK)", FiMn, Moment1, ConstUnit.Length));
								else
									Reporting.AddLine(String.Format("{0} << {1} {2} (NG)", FiMn, Moment1, ConstUnit.Length));


								V = Math.Max(Math.Abs(FhorizMotherside - FhorizM), Math.Abs(FhorizM)) / 2;

								Reporting.AddLine("V = Max(Abs(H_Plate - H_Angles), Abs(H_Plate)) / 2");
								Reporting.AddLine(String.Format("{0} = Max(Abs({1} - {2}), Abs({2})) / 2", V, FhorizMotherside, FhorizM));

								ShearStress = V / (column.Shape.tw * (shearClipAngle.BoltOnGusset.SpacingLongDir - column.BoltBrace.HoleDiameterSTD));

								Reporting.AddLine("fv = V / (tw * (s - dh))");
								Reporting.AddLine(String.Format("{0} = {1} / ({2} * ({3} - {4}))",
									ShearStress, V, column.Shape.tw, shearClipAngle.BoltOnGusset.SpacingLongDir, column.BoltBrace.HoleDiameterSTD));

								if (ShearStress <= ConstNum.FIOMEGA1_0N * 0.6 * column.Material.Fy)
									Reporting.AddLine(String.Format("Shear Stress {0} <= " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy {1} (OK)", ShearStress, column.Material.Fy));
								else
									Reporting.AddLine(String.Format("Shear Stress {0} >> " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy {1} (NG)", ShearStress, column.Material.Fy));

						        Moment2 = Math.Abs((spanL * FhorizM + (otherSide.WinConnect.ShearClipAngle.GussetSideGage - spanL) * FhorizMotherside) / 4);

								Reporting.AddHeader("Bending at mid-section");
								Reporting.AddLine("M = Abs(Wc * H_Plate + (g - Wc) * H_Angles) / 4");
								Reporting.AddLine(String.Format("{0} = Abs({1} * {2} + ({3} - {1}) * {4}) / 4",
							        Moment2, spanL, FhorizM, otherSide.WinConnect.ShearClipAngle.GussetSideGage, FhorizMotherside));

								FiMn = ConstNum.FIOMEGA0_9N * column.Material.Fy * Math.Pow(column.Shape.tw, 2) * shearClipAngle.BoltOnGusset.SpacingLongDir / 4;

								Reporting.AddHeader("Bending Strength");
								Reporting.AddLine("FiMn = " + ConstString.FIOMEGA0_9 + " * Fy * tw² * S / 4");
								Reporting.AddLine(String.Format("{0} = {1} * {2} * {3}² * {4} / 4",
									FiMn, ConstString.FIOMEGA0_9, column.Material.Fy, column.Shape.tw, shearClipAngle.BoltOnGusset.SpacingLongDir));

								if (FiMn >= Moment2)
									Reporting.AddLine(String.Format("{0} >= {1} {2} (OK)", FiMn, Moment2, ConstUnit.Length));
								else
									Reporting.AddLine(String.Format("{0} << {1} {2} (NG)", FiMn, Moment2, ConstUnit.Length));
							}
						}
						break;
				}
			}
			else if ((component.KBrace | component.KneeBrace) || CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase)
			{
				Reporting.AddHeader("Column Web Bending and out of Plane Shear");
				switch (memberType)
				{
					case EMemberType.UpperRight:
					case EMemberType.LowerRight:
						if (CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].ShapeName != ConstString.NONE ||
						    CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].ShapeName != ConstString.NONE ||
						    CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].ShapeName != ConstString.NONE)
						{
							otherSide = new DetailData();
						}
						break;
					case EMemberType.UpperLeft:
					case EMemberType.LowerLeft:
						if (CommonDataStatic.DetailDataDict[EMemberType.RightBeam].ShapeName != ConstString.NONE ||
						    CommonDataStatic.DetailDataDict[EMemberType.UpperRight].ShapeName != ConstString.NONE ||
						    CommonDataStatic.DetailDataDict[EMemberType.LowerRight].ShapeName != ConstString.NONE)
						{
							otherSide = new DetailData();
						}
						break;
				}
				if (MiscMethods.IsBeam(memberType))
				{
					Reporting.AddLine("WARNING");
					Reporting.AddLine("In the following calculations the left and right side");
					Reporting.AddLine("effects are considered independantly.  These calculations");
					Reporting.AddLine("are provided for information only.  Using the parameters");
					Reporting.AddLine("in these calculations, the user should evaluate the");
					Reporting.AddLine("combination of the left and right side effects.");
				}
				Wc = column.Shape.d - 2 * column.Shape.kdes;
				if (component.KBrace && otherSide != null)
				{
					H = Math.Abs(component.BraceConnect.Gusset.Hc + otherSide.BraceConnect.Gusset.Hc);
					eBottom = ortasi - yBottom;
					Moment = Math.Abs(eTop * otherSide.BraceConnect.Gusset.Hc - eBottom * component.BraceConnect.Gusset.Hc);
					Fhoriz = (H + 3 * Moment / L) / L;
					ShStress = Fhoriz / (2 * column.Shape.tw);
					Reporting.AddHeader("Column Web Bending:");
					Reporting.AddLine("Horizontal Force (H) = FxTop + FxBot");
					Reporting.AddLine("= | " + otherSide.BraceConnect.Gusset.Hc + " + " + component.BraceConnect.Gusset.Hc + " |");
					Reporting.AddLine("= " + H + ConstUnit.Force);
					Reporting.AddLine("Gusset Edge Moment (M) = |(eTop * FxTop - eBot * FxBot)|");
					Reporting.AddLine("= | " + eTop + " * " + otherSide.BraceConnect.Gusset.Hc + " - " + eBottom + " * " + component.BraceConnect.Gusset.Hc + " |");
					Reporting.AddLine("= " + Moment + ConstUnit.MomentUnitFoot);

					Moment = Fhoriz * Wc / 4;
					Reporting.AddLine("Moment (M) = H * Wc / 4 = " + Fhoriz + " * " + Wc + " / 4 = " + Moment + ConstUnit.MomentUnitFoot);

					FiMn = ConstNum.FIOMEGA0_9N * column.Material.Fy * Math.Pow(column.Shape.tw, 2) / 4;
					Reporting.AddHeader("Bending Strength:");
					Reporting.AddLine(ConstString.PHI + "Mn = " + ConstString.FIOMEGA0_9 + " * Fy * tw² / 4");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + component.Material.Fy + " * " + column.Shape.tw + "² / 4");

					if (FiMn >= Moment)
						Reporting.AddLine(FiMn + " >= " + Moment + " " + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine(FiMn + " << " + Moment + " " + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Column Web Transverse Shear:");
					Reporting.AddLine("fv = (H / L + 3 * M / L²) / (2 * tw )");
					Reporting.AddLine("  = (" + H + " / " + L + " + 3 * " + Moment + " / " + L + "²) / (2 * " + column.Shape.tw + ")");

					if (ShStress <= ConstNum.FIOMEGA1_0N * 0.6 * column.Material.Fy)
						Reporting.AddLine("Shear Stress " + ShStress + " <= FIOMEGA1_0N * 0.6 * Fy " + column.Material.Fy + " (OK)");
					else
						Reporting.AddLine("Shear Stress " + ShStress + " >> FIOMEGA1_0N * 0.6 * Fy " + column.Material.Fy + " (NG)");

					if (MiscMethods.IsBeam(memberType) && ShStress > 0)
					{
						Reporting.AddLine("Warning:");
						Reporting.AddLine("The column web bending and shear stresses above may be partially ");
						Reporting.AddLine("or fully additive to the stresses due to the forces and moments ");
						Reporting.AddLine("from the members connected to the opposite side of the column web.");
						Reporting.AddLine("Checking this condition is left to the user.");
					}
				}
				else if (component.KneeBrace || CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase) //else if (BRACE1.KneeBrace[m] || BRACE1.JointType == 2)
				{
					switch (component.GussetToColumnConnection)
					{
						case EBraceConnectionTypes.ClipAngle:
						case EBraceConnectionTypes.EndPlate:
							L = shearClipAngle.Length - 2 * shearClipAngle.BoltOnColumn.MinEdgeSheared;
							break;
						case EBraceConnectionTypes.SinglePlate:
							L = component.WinConnect.ShearWebPlate.Length;
							break;
						case EBraceConnectionTypes.FabricatedTee:
							L = component.BraceConnect.FabricatedTee.Length;
							break;
						case EBraceConnectionTypes.DirectlyWelded:
							if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase)
							{
								//L = BRACE1.y[29 + 5 * (m - 3)] - component.BraceConnect.BasePlate.CornerClip - component.BraceConnect.BasePlate.Thickness / 2;
							}
							else
								L = component.BraceConnect.Gusset.VerticalForceColumn;
							break;
					}
					//i1 = 5 * (m - 3);
					//SmallMethodsBrace.Intersect(100, BRACE1.x[25 + i1], BRACE1.y[25 + i1], Math.Tan(component.Angle * ConstNum.RADIAN), component.WorkPointX, component.WorkPointY, ref x0, ref y0);

					//ecc = Math.Abs((BRACE1.y[25 + i1] + BRACE1.y[29 + i1]) / 2 - y0);
					H = Math.Max(Math.Abs(component.BraceConnect.Gusset.Hc), 0.01);

					ecc = component.BraceConnect.Gusset.Mc / H;
					Mom = Math.Abs(H * ecc);

					H_Max = H / L + 6 * Mom / Math.Pow(L, 2);
					Reporting.AddLine(String.Format("Gusset edge (L) = {0} {1}", L, ConstUnit.Length));
					Reporting.AddLine(String.Format("Horizontal Force (H) = {0} {1}", H, ConstUnit.Force));
					Reporting.AddLine(String.Format("Eccentricity (ec) = {0} {1}", ecc, ConstUnit.Force));
					Reporting.AddLine(String.Format("Gusset Edge Moment (M) = H*ec = {0} {1}", Mom, ConstUnit.Length));

					Reporting.AddHeader("Max. Horizontal Force Intensity");
					Reporting.AddLine("H_max = H / L + 6 * M / L²");
					Reporting.AddLine(String.Format("{0} = {1} / {2} + 6 * {3} / {2}²", H_Max, H, L, Mom));

					Moment = H_Max * Wc / 4;

					ShStress = H_Max / (2 * column.Shape.tw);

					Reporting.AddHeader("Column Web Bending");
					Reporting.AddLine("Moment (M) = H_max * Wc / 4");
					Reporting.AddLine(String.Format("{0} = {1} * {2} / 4", Moment, H_Max, Wc));

					FiMn = ConstNum.FIOMEGA0_9N * column.Material.Fy * Math.Pow(column.Shape.tw, 2) / 4;

					Reporting.AddHeader("Bending Strength");
					Reporting.AddLine("FiMn = " + ConstString.FIOMEGA0_9 + " * Fy * tw² / 4");
					Reporting.AddLine(String.Format("{0} = {1} * {2} * {3}² / 4",
						FiMn, ConstString.FIOMEGA0_9, column.Material.Fy, column.Shape.tw));

					if (FiMn >= Moment)
						Reporting.AddLine(String.Format("{0} >= {1} {2} (OK)", FiMn, Moment, ConstUnit.Length));
					else
						Reporting.AddLine(String.Format("{0} << {1} {2} (NG)", FiMn, Moment, ConstUnit.Length));

					Reporting.AddHeader("Shear Stress");
					Reporting.AddLine("fv = H_max / (2 * tw)");
					Reporting.AddLine(String.Format("{0} = {1} / (2 * {2})", column.Material.Fy, H_Max, column.Shape.tw));

					if (ShStress <= ConstNum.FIOMEGA1_0N * 0.6 * column.Material.Fy)
						Reporting.AddLine(String.Format("Shear Stress {0} <= " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy {1} (OK)", ShStress, column.Material.Fy));
					else
						Reporting.AddLine(String.Format("Shear Stress {0} >> " + ConstString.FIOMEGA1_0 + " * 0.6 * Fy {1} (NG)", ShStress, column.Material.Fy));
				}
			}
		}
	}
}