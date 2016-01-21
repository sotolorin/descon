using System;
using System.Linq;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class Seat
	{
		internal static void DesignSeat(EMemberType memberType)
		{
			double Ws;
			double FiRnRupture;
			double FiRnYielding;
			int Fbe;
			double RF;
			double Ls;
			double BearingCap;
			double Cap;
			double usefulw;
			double Ru;
			double WSFR;
			double k;
			double column_T;
			double wp;
			double StiffenerLength;
			double c;
			double a;
			double usefulweldsize;
			double e;
			double minweld;
			double maximumStiffenerLength;
			double BF;
			double maximumStiffenerLength2;
			double maximumStiffenerLength1;
			double tReq;
			double ratio;
			double tfReq;
			double Lnet;
			double WidthThickness = 0;
			double Teetw;
			double g;
			double g1;
			int FbeA;
			double anl2;
			double n;
			double NforYielding;
			double x5;
			double X4;
			double NforCrippling;
			double x3;
			double x2;
			double X1;
			double wUseful;
			double B;
			double Gage = 0;
			double FiRn = 0;
			double L2 = 0;
			double Fbs = 0;
			double MinThicknessSupport;
			double FbsS;
			double MinThicknessAngle;
			double FbsA;
			double CG;
			double BB;
			int NB;
			double ercl;
			double SupFu = 0;
			double SupThickness1 = 0;
			double SupThickness;
			double WSF;
			double wmin;
			double weldMax;
			double w = 0;
			double tss;
			double th;
			double tForWeld;
			double tShear;
			double t = 0;
			double ta = 0;
			double DesignSeat_Nbearing = 0;

			bool shart5 = false;
			bool Shart3 = false;
			bool shart2 = false;
			bool shart1 = false;
			bool angleLengthValidType = false;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			var shearSeat = beam.WinConnect.ShearSeat;
			if (!shearSeat.Bolt.EdgeDistLongDir_User)
				shearSeat.Bolt.EdgeDistLongDir = shearSeat.Bolt.MinEdgeSheared;

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BeamToColumnFlange:
				case EJointConfiguration.BeamToHSSColumn:
					SupThickness = column.Shape.tf;
					SupThickness1 = column.Shape.tf;
					SupFu = column.Material.Fu;
					break;
				case EJointConfiguration.BeamToColumnWeb:
					if (beam.MemberType == EMemberType.LeftBeam)
					{
						SupThickness = column.Shape.tw * Math.Abs(leftBeam.ShearForce) / (Math.Abs(leftBeam.ShearForce) + Math.Abs(rightBeam.ShearForce));
						SupThickness1 = column.Shape.tw;
					}
					else
					{
						SupThickness = column.Shape.tw * Math.Abs(rightBeam.ShearForce) / (Math.Abs(leftBeam.ShearForce) + Math.Abs(rightBeam.ShearForce));
						SupThickness1 = column.Shape.tw;
					}
					SupFu = column.Material.Fu;
					break;
				case EJointConfiguration.BeamToGirder:
					if (beam.MemberType == EMemberType.LeftBeam)
					{
						SupThickness = column.Shape.tw * Math.Abs(leftBeam.ShearForce) / (Math.Abs(leftBeam.ShearForce) + Math.Abs(rightBeam.ShearForce));
						SupThickness1 = column.Shape.tw;
					}
					else
					{
						SupThickness = column.Shape.tw * Math.Abs(rightBeam.ShearForce) / (Math.Abs(leftBeam.ShearForce) + Math.Abs(rightBeam.ShearForce));
						SupThickness1 = column.Shape.tw;
					}
					SupFu = column.Material.Fu;
					break;
				default:
					SupThickness = 0;
					break;
			}

			// Result will be 0 if there is only one beam (Mike - 5/1/15)
			if (SupThickness == 0)
				SupThickness = SupThickness1;

			ercl = beam.EndSetback + ConstNum.QUARTER_INCH;
			shearSeat.Bolt.EdgeDistTransvDir = shearSeat.Bolt.MinEdgeRolled;
			switch (shearSeat.Connection)
			{
				case ESeatConnection.Bolted:
					NB = (int)(beam.ShearForce / shearSeat.Bolt.BoltStrength);
					BB = column.Shape.t;
					CG = beam.GageOnColumn;
					beam.GageOnFlange = beam.WinConnect.Beam.GageOnFlange;
					switch (CommonDataStatic.BeamToColumnType)
					{
						case EJointConfiguration.BeamToHSSColumn:
						case EJointConfiguration.BeamToColumnFlange:
							NB = (int)Math.Ceiling(beam.ShearForce / (2 * shearSeat.Bolt.BoltStrength)) * 2;
							FbsA = CommonCalculations.SpacingBearing(shearSeat.Bolt.SpacingTransvDir, shearSeat.Bolt.HoleWidth, shearSeat.Bolt.BoltSize, shearSeat.Bolt.HoleType, shearSeat.Material.Fu, false);
							MinThicknessAngle = beam.ShearForce / (NB * FbsA);
							FbsS = CommonCalculations.SpacingBearing(shearSeat.Bolt.SpacingTransvDir, shearSeat.Bolt.HoleWidth, shearSeat.Bolt.BoltSize, shearSeat.Bolt.HoleType, SupFu, false);
							MinThicknessSupport = beam.ShearForce / (NB * FbsS);
							if (SupThickness < MinThicknessSupport && SupThickness != 0)
							{
								NB = (int)Math.Ceiling(NB / 2.0 * MinThicknessSupport / SupThickness) * 2;
								MinThicknessAngle = beam.ShearForce / (NB * Fbs);
							}
							shearSeat.Bolt.NumberOfBolts = NB;
							switch (NB)
							{
								case 2:
									L2 = shearSeat.Bolt.EdgeDistTransvDir;
									shearSeat.Bolt.NumberOfLines = 2;
									shearSeat.Bolt.NumberOfRows = 1;
									break;
								case 4:
									L2 = shearSeat.Bolt.EdgeDistTransvDir + shearSeat.Bolt.SpacingTransvDir;
									shearSeat.Bolt.NumberOfLines = 2;
									shearSeat.Bolt.NumberOfRows = 2;
									break;
								case 6:
									L2 = shearSeat.Bolt.EdgeDistTransvDir + 2 * shearSeat.Bolt.SpacingTransvDir;
									shearSeat.Bolt.NumberOfLines = 2;
									shearSeat.Bolt.NumberOfRows = 3;
									break;
							}
							break;
						case EJointConfiguration.BeamToColumnWeb:
							if (shearSeat.Bolt.NumberOfRows == 0)
								shearSeat.Bolt.NumberOfRows = 1;
							NB = (int)Math.Ceiling(beam.ShearForce / (shearSeat.Bolt.BoltStrength * shearSeat.Bolt.NumberOfRows)) * shearSeat.Bolt.NumberOfRows;
							FbsA = CommonCalculations.SpacingBearing(shearSeat.Bolt.SpacingTransvDir, shearSeat.Bolt.HoleWidth, shearSeat.Bolt.BoltSize, shearSeat.Bolt.HoleType, shearSeat.Material.Fu, false);
							MinThicknessAngle = beam.ShearForce / (NB * FbsA);
							FbsS = CommonCalculations.SpacingBearing(shearSeat.Bolt.SpacingTransvDir, shearSeat.Bolt.HoleWidth, shearSeat.Bolt.BoltSize, shearSeat.Bolt.HoleType, SupFu, false);
							MinThicknessSupport = beam.ShearForce / (NB * FbsS);
							if (SupThickness < MinThicknessSupport)
							{
								NB = (int)Math.Ceiling(NB * MinThicknessSupport / SupThickness);
								MinThicknessAngle = beam.ShearForce / (NB * FbsA);
							}
							if (NB == 1)
								NB = 2;
							if (NB == 5)
								NB = 6;
							if (NB == 7 || NB == 8)
								NB = 9;

							shearSeat.Bolt.NumberOfBolts = NB;
							switch (NB)
							{
								case 2:
									shearSeat.Bolt.NumberOfLines = 2;
									shearSeat.Bolt.NumberOfRows = 1;
									L2 = shearSeat.Bolt.EdgeDistTransvDir;
									break;
								case 3:
									if (BB >= 8 * ConstNum.ONE_INCH)
									{
										shearSeat.Bolt.NumberOfLines = 3;
										shearSeat.Bolt.NumberOfRows = 1;
										L2 = shearSeat.Bolt.EdgeDistTransvDir;
									}
									else
									{
										NB = 4;
										shearSeat.Bolt.NumberOfLines = 2;
										shearSeat.Bolt.NumberOfRows = 2;
										L2 = shearSeat.Bolt.EdgeDistTransvDir + shearSeat.Bolt.SpacingTransvDir;
									}
									break;
								case 4:
									shearSeat.Bolt.NumberOfLines = 2;
									shearSeat.Bolt.NumberOfRows = 2;
									L2 = shearSeat.Bolt.EdgeDistTransvDir + shearSeat.Bolt.SpacingTransvDir;
									break;
								case 6:
									if (BB >= 8 * ConstNum.ONE_INCH)
									{
										angleLengthValidType = true;
										shearSeat.Bolt.NumberOfLines = 3;
										shearSeat.Bolt.NumberOfRows = 2;
										L2 = shearSeat.Bolt.EdgeDistTransvDir + shearSeat.Bolt.SpacingTransvDir;
									}
									else
									{
										shearSeat.Bolt.NumberOfLines = 2;
										shearSeat.Bolt.NumberOfRows = 3;
										L2 = shearSeat.Bolt.EdgeDistTransvDir + 2 * shearSeat.Bolt.SpacingTransvDir;
									}
									break;
								case 9:
									angleLengthValidType = true;
									shearSeat.Bolt.NumberOfLines = 3;
									shearSeat.Bolt.NumberOfRows = 3;
									L2 = shearSeat.Bolt.EdgeDistTransvDir + 2 * shearSeat.Bolt.SpacingTransvDir;
									break;
							}
							break;
					}
					if (!shearSeat.AngleLength_User)
					{
						if (beam.WinConnect.Beam.GageOnFlange > ConstNum.THREEANDHALF_INCHES || CG > ConstNum.THREEANDHALF_INCHES || angleLengthValidType)
							shearSeat.AngleLength = 8 * ConstNum.ONE_INCH;
						else
							shearSeat.AngleLength = 6 * ConstNum.ONE_INCH;
						if (shearSeat.AngleLength > column.Shape.bf)
							shearSeat.AngleLength = column.Shape.bf;
					}
					switch (shearSeat.Bolt.NumberOfLines)
					{
						case 2:
							shearSeat.TrSpacingOut = 0;
							break;
						case 3:
							shearSeat.TrSpacingOut = shearSeat.Bolt.SpacingLongDir;
							break;
						case 4:
							shearSeat.TrSpacingOut = (shearSeat.AngleLength - shearSeat.Bolt.SpacingLongDir - 2 * shearSeat.Bolt.EdgeDistLongDir) / 2;
							break;
					}

					if (!shearSeat.Angle_User)
					{
						foreach (var shape in CommonDataStatic.ShapesSingleAngle)
						{
							MiscCalculationsWithReporting.SeatedBeamNewCalc(beam.MemberType, shearSeat.AngleLength, shape.Value.kdet, shape.Value.t, ref ta, ref DesignSeat_Nbearing, ref FiRn, false);
							Gage = Math.Max(shape.Value.kdet + shearSeat.Bolt.BoltSize, 0);
							if (FiRn >= beam.ShearForce && shape.Value.d >= DesignSeat_Nbearing + beam.EndSetback && shape.Value.b >= L2 + Gage && shape.Value.t >= ta && shape.Value.d - (beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + beam.EndSetback) >= shearSeat.Bolt.EdgeDistTransvDir)
							{
								if (FiRn >= beam.ShearForce)
								{
									shearSeat.Angle = shape.Value;
									break;
								}
							}
						}
					}

					shearSeat.Bolt.SpacingTransvDir = shearSeat.Bolt.SpacingTransvDir;
					shearSeat.Angle.g0 = ercl + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange;
					shearSeat.Angle.g1 = Gage;
					if (shearSeat.TopAngle.t < ConstNum.QUARTER_INCH && !shearSeat.TopAngle_User)
					{
						// Ugly, but 483 is: L4X4X1/4 US, L102X102X6.4 Metric which is what the program wants
						shearSeat.TopAngle = CommonDataStatic.AllShapes.First(s => s.Value.Code == 483).Value;
						shearSeat.TopAngle.g1 = 2.5 * ConstNum.ONE_INCH;
					}

					if (!shearSeat.AngleLength_User)
						shearSeat.AngleLength = Math.Max(beam.WinConnect.Beam.GageOnFlange, beam.GageOnColumn) + 2 * shearSeat.Bolt.EdgeDistLongDir;
					shearSeat.TopAngle.g0 = ercl + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange;
					shearSeat.Angle.g2 = shearSeat.Bolt.SpacingTransvDir;
					break;
				case ESeatConnection.Welded:
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
					{
						if (column.WebOrientation != EWebOrientation.OutOfPlane)
							BB = column.Shape.d - 3 * column.Shape.tf - ConstNum.ONEANDHALF_INCHES;
						else
							BB = column.Shape.bf - 3 * column.Shape.tf - ConstNum.ONEANDHALF_INCHES;
					}
					else
					{
						if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange)
							BB = column.Shape.bf - ConstNum.ONEANDHALF_INCHES;
						else
							BB = column.Shape.t - 0.75 * ConstNum.ONE_INCH;
					}

					B = beam.GageOnFlange + 2 * shearSeat.Bolt.MinEdgeSheared;
					if (!shearSeat.AngleLength_User)
						shearSeat.AngleLength = NumberFun.Round(Math.Min(BB, B), 8);

					foreach (var shape in CommonDataStatic.ShapesSingleAngle)
					{
						Shape tempShape;
						double shapeLong;
						double shapeShort;

						if (shearSeat.Angle_User)
							tempShape = shearSeat.Angle.ShallowCopy();
						else
							tempShape = shape.Value.ShallowCopy();

						shapeLong = Math.Max(tempShape.b, tempShape.d);
						shapeShort = Math.Min(tempShape.b, tempShape.d);

						MiscCalculationsWithReporting.SeatedBeamNewCalc(beam.MemberType, shearSeat.AngleLength, tempShape.kdet, tempShape.t, ref ta, ref DesignSeat_Nbearing, ref FiRn, false);
						if (FiRn >= beam.ShearForce && shapeLong >= ConstNum.FOUR_INCHES && tempShape.t >= ta && (shapeShort >= DesignSeat_Nbearing + beam.EndSetback && shapeShort <= 1.05 * ConstNum.FOUR_INCHES && shapeShort >= ConstNum.THREEANDHALF_INCHES))
						{
							wmin = CommonCalculations.MinimumWeld(SupThickness1, tempShape.tw);
							wUseful = SupThickness * SupFu / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
							if (tempShape.t >= ConstNum.QUARTER_INCH)
								weldMax = tempShape.t - ConstNum.SIXTEENTH_INCH;
							else
								weldMax = tempShape.t;
							w = MiscCalculationsWithReporting.SeatWeld(beam.MemberType, beam.ShearForce, shapeShort, shapeLong, false);
							w = Math.Max(wmin, w);

							if (!shearSeat.Angle_User && (w > weldMax || w > NumberFun.ConvertFromFraction(5) && shapeLong < 8 * ConstNum.ONE_INCH))
								continue;

							if (FiRn >= beam.ShearForce)
								shearSeat.Angle = tempShape.ShallowCopy();
							else if (!shearSeat.Angle_User)
								continue;
						}
						else
							continue;

						break;
					}

					if (!shearSeat.WeldSizeSupport_User)
						shearSeat.WeldSizeSupport = w;
					if (!shearSeat.WeldSizeBeam_User)
						shearSeat.WeldSizeBeam = CommonCalculations.MinimumWeld(shearSeat.Angle.t, beam.Shape.tf);
					if (beam.Shape.tf >= ConstNum.QUARTER_INCH)
						weldMax = beam.Shape.tf - ConstNum.SIXTEENTH_INCH;
					else
						weldMax = beam.Shape.tf;
					if (shearSeat.WeldSizeBeam > weldMax && !shearSeat.WeldSizeBeam_User)
						shearSeat.WeldSizeBeam = weldMax;
					shearSeat.TopAngleLength = NumberFun.Round(beam.Shape.bf - ConstNum.ONEANDHALF_INCHES, 8);
					if (shearSeat.TopAngle.t < ConstNum.QUARTER_INCH && !shearSeat.TopAngle_User)
					{
						shearSeat.TopAngle = CommonDataStatic.AllShapes.First(s => s.Value.Name == "L4X4X1/4").Value;
						shearSeat.TopAngle.t = 0.25 * ConstNum.ONE_INCH;
					}

					if (!shearSeat.WeldSizeBeam_User)
						shearSeat.WeldSizeBeam = CommonCalculations.MinimumWeld(shearSeat.TopAngle.t, beam.Shape.tf);
					if (beam.Shape.tf >= ConstNum.QUARTER_INCH)
						weldMax = shearSeat.TopAngle.t - ConstNum.SIXTEENTH_INCH;
					else
						weldMax = shearSeat.TopAngle.t;
					if (shearSeat.WeldSizeBeam > weldMax && !shearSeat.WeldSizeBeam_User)
						shearSeat.WeldSizeBeam = weldMax;

					// This seems to be corrupting the correct weld size (MT 6/29/15)
					//shearSeat.WeldSizeSupport = CommonCalculations.MinimumWeld(shearSeat.TopAngle.t, SupThickness1);
					
					//if (shearSeat.TopAngle.t >= ConstNum.QUARTER_INCH)
					//	weldMax = shearSeat.TopAngle.t - ConstNum.SIXTEENTH_INCH;
					//else
					//	weldMax = shearSeat.TopAngle.t;
					//if (shearSeat.WeldSizeSupport > weldMax && !shearSeat.WeldSizeSupport_User)
					//	shearSeat.WeldSizeSupport = weldMax;
					break;
				case ESeatConnection.BoltedStiffenedPlate:
					NB = -((int)Math.Floor(-beam.ShearForce / shearSeat.Bolt.BoltStrength / 2)) * 2;
					shearSeat.Bolt.NumberOfLines = 2;
					shearSeat.Bolt.NumberOfRows = (NB / 2);

					X1 = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5);
					x2 = X1 * (4 / beam.Shape.d) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					x3 = -X1 * 0.2 * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					NforCrippling = (beam.ShearForce - (X1 + x3)) / x2;
					X4 = 2.5 * beam.Shape.kdes * beam.Material.Fy * beam.Shape.tw;
					x5 = beam.Material.Fy * beam.Shape.tw;
					NforYielding = (beam.ShearForce - X4) / x5;
					n = Math.Max(beam.Shape.kdet, Math.Max(NforCrippling, NforYielding));
					if (n / beam.Shape.d <= 0.2)
					{
						X1 = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5);
						x2 = X1 * (3 / beam.Shape.d) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
						NforCrippling = (beam.ShearForce - X1) / x2;
						n = Math.Max(beam.Shape.kdet, Math.Max(NforCrippling, NforYielding));
					}
					if ((n + ercl) <= ConstNum.THREEANDHALF_INCHES)
						anl2 = ConstNum.THREEANDHALF_INCHES;
					else if ((n + ercl) <= ConstNum.FOUR_INCHES)
						anl2 = ConstNum.FOUR_INCHES;
					else if ((n + ercl) <= ConstNum.FIVE_INCHES)
						anl2 = ConstNum.FIVE_INCHES;
					else
					{
						Reporting.AddLine("Seat width too large. (NG)");
						anl2 = ConstNum.FIVE_INCHES;
					}
					tss = beam.ShearForce / (ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(beam.Material.Fy, shearSeat.Material.Fy) * (anl2 - ercl));
					tss = Math.Max(tss, beam.Shape.tw * beam.Material.Fy / shearSeat.Material.Fy);
					FbsA = CommonCalculations.SpacingBearing(shearSeat.Bolt.SpacingTransvDir, shearSeat.Bolt.HoleWidth, shearSeat.Bolt.BoltSize, shearSeat.Bolt.HoleType, shearSeat.Material.Fu, true);
					FbeA = (int)CommonCalculations.EdgeBearing(shearSeat.Bolt.EdgeDistLongDir, shearSeat.Bolt.HoleWidth, shearSeat.Bolt.BoltSize, shearSeat.Material.Fu, shearSeat.Bolt.HoleType, true);
					MinThicknessAngle = beam.ShearForce / ((NB - 2) * FbsA + 2 * FbeA);
					FbsS = CommonCalculations.SpacingBearing(shearSeat.Bolt.SpacingTransvDir, shearSeat.Bolt.HoleWidth, shearSeat.Bolt.BoltSize, shearSeat.Bolt.HoleType, SupFu, true);
					MinThicknessSupport = beam.ShearForce / (NB * FbsS);
					if (SupThickness < MinThicknessSupport)
					{
						NB = (int)Math.Ceiling(NB * MinThicknessSupport / SupThickness);
						shearSeat.Bolt.NumberOfRows = (NB / 2);
						MinThicknessAngle = beam.ShearForce / ((NB - 2) * FbsA + 2 * FbeA);
					}
					g1 = Math.Max(shearSeat.Bolt.EdgeDistLongDir, shearSeat.Bolt.BoltSize + ConstNum.HALF_INCH);
					g = Math.Max(NumberFun.ConvertFromFraction(26), g1);
					if (!shearSeat.StiffenerLength_User)
						shearSeat.StiffenerLength = (shearSeat.Bolt.NumberOfRows - 1) * shearSeat.Bolt.SpacingTransvDir + shearSeat.Bolt.EdgeDistLongDir + g;

					switch (shearSeat.Stiffener)
					{
						case ESeatStiffener.Tee:
							Teetw = MinThicknessAngle;
							WidthThickness = 0.75 * Math.Pow(ConstNum.ELASTICITY, 0.5) / Math.Sqrt(shearSeat.Material.Fy);
							Lnet = shearSeat.StiffenerLength - shearSeat.Bolt.NumberOfRows * (shearSeat.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH); // Tee flange
							tfReq = Math.Max(beam.ShearForce / (2 * ConstNum.FIOMEGA1_0N * 0.6 * shearSeat.Material.Fy * shearSeat.StiffenerLength), beam.ShearForce / (2 * ConstNum.FIOMEGA0_75N * 0.6 * shearSeat.Material.Fu * Lnet)); // Tee flange
							tss = Math.Max(tfReq, Teetw);
							foreach (var shape in CommonDataStatic.AllShapes.Where(s => s.Value.TypeEnum == EShapeType.WideFlange))
							{
								ratio = (shape.Value.d - shape.Value.tf) / shape.Value.tw;
								if (shape.Value.tw >= tss && shape.Value.tf >= tfReq && shape.Value.d >= anl2 && ratio <= WidthThickness)
								{
									if ((CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn) && shape.Value.bf >= beam.GageOnColumn + 2 * shearSeat.Bolt.EdgeDistTransvDir)
									{
										shearSeat.StiffenerTee = shape.Value;
										break;
									}
									if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && shape.Value.bf <= column.Shape.t && shape.Value.bf >= shearSeat.Bolt.SpacingLongDir + 2 * shearSeat.Bolt.EdgeDistTransvDir)
									{
										shearSeat.StiffenerTee = shape.Value;
										break;
									}
								}
							}

							if (!shearSeat.StiffenerWidth_User)
								shearSeat.StiffenerWidth = shearSeat.StiffenerTee.d;
							if (!shearSeat.StiffenerThickness_User)
								shearSeat.StiffenerThickness = shearSeat.StiffenerTee.tw;
							shearSeat.StiffenerFlangeTh = shearSeat.StiffenerTee.tf;
							shearSeat.StiffenerFlangeWidth = shearSeat.StiffenerTee.bf;
							if (!shearSeat.PlateLength_User)
								shearSeat.PlateLength = NumberFun.Round(shearSeat.StiffenerFlangeWidth + ConstNum.ONE_INCH, 8);
							if (!shearSeat.PlateWidth_User)
								shearSeat.PlateWidth = NumberFun.Round(shearSeat.StiffenerWidth + ConstNum.HALF_INCH, 8);
							if (!shearSeat.PlateThickness_User)
								shearSeat.PlateThickness = ConstNum.THREE_EIGHTS_INCH;
							break;
						case ESeatStiffener.L2:
							Lnet = shearSeat.StiffenerLength - shearSeat.Bolt.NumberOfRows * (shearSeat.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
							tReq = Math.Max(beam.ShearForce / (ConstNum.FIOMEGA1_0N * 0.6 * shearSeat.Material.Fy * shearSeat.StiffenerLength), beam.ShearForce / (ConstNum.FIOMEGA0_75N * 0.6 * shearSeat.Material.Fu * Lnet));
							tss = Math.Max(Math.Max(tss, tReq), MinThicknessAngle);
							WidthThickness = 0.56 * Math.Pow(ConstNum.ELASTICITY / shearSeat.Material.Fy, 0.5);
							tss = tss / 2;
							foreach (var shape in CommonDataStatic.AllShapes.Where(s => s.Value.TypeEnum == EShapeType.DoubleAngle))
							{
								ratio = (shape.Value.d - shape.Value.t) / shape.Value.t;
								if (shape.Value.t >= tss && shape.Value.d >= anl2 && ratio <= WidthThickness)
								{
									if ((CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn) && shape.Value.b >= shearSeat.Bolt.SpacingLongDir / 2 + shearSeat.Bolt.EdgeDistTransvDir)
									{
										shearSeat.StiffenerAngle = shape.Value;
										break;
									}
									if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && 2 * shape.Value.b <= column.Shape.t && shape.Value.b >= shearSeat.Bolt.SpacingLongDir / 2 + shearSeat.Bolt.EdgeDistTransvDir)
									{
										shearSeat.StiffenerAngle = shape.Value;
										break;
									}
								}
							}

							if (!shearSeat.StiffenerWidth_User)
								shearSeat.StiffenerWidth = shearSeat.StiffenerAngle.d;
							if (!shearSeat.StiffenerThickness_User)
								shearSeat.StiffenerThickness = 2 * shearSeat.StiffenerAngle.t;
							shearSeat.StiffenerFlangeTh = shearSeat.StiffenerAngle.t;
							shearSeat.StiffenerFlangeWidth = 2 * shearSeat.StiffenerAngle.b;
							if (!shearSeat.PlateLength_User)
								shearSeat.PlateLength = NumberFun.Round(shearSeat.StiffenerFlangeWidth + ConstNum.ONE_INCH, 8);
							if (!shearSeat.PlateWidth_User)
								shearSeat.PlateWidth = NumberFun.Round(shearSeat.StiffenerWidth + ConstNum.HALF_INCH, 8);
							if (!shearSeat.PlateThickness_User)
								shearSeat.PlateThickness = ConstNum.THREE_EIGHTS_INCH;
							break;
					}
					if (shearSeat.TopAngle.t < ConstNum.QUARTER_INCH && !shearSeat.TopAngle_User)
					{
						shearSeat.TopAngle.t = 0.25 * ConstNum.ONE_INCH;
						if (CommonDataStatic.Units == EUnit.US)
							shearSeat.TopAngle = CommonDataStatic.AllShapes.First(s => s.Value.Name == "L4X4X1/4").Value;
						else
							shearSeat.TopAngle = CommonDataStatic.AllShapes.First(s => s.Value.Name == "L102X102X6.4").Value;
					}
					shearSeat.TopAngleLength = Math.Max(beam.GageOnFlange + 2 * shearSeat.Bolt.EdgeDistLongDir, beam.GageOnColumn + 2 * shearSeat.Bolt.EdgeDistLongDir);
					shearSeat.TopAngle.g0 = ercl + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange;
					break;
					// This was never implemented fully in Descon 7, but we could add it back later (MT - 1/20/15)
					//case ESeatConnection.BoltedStiffenedAngle:
					//	NB = -((int) Math.Floor(-beam.ShearForce / shearSeat.Bolt.Fv / 2)) * 2;
					//	shearSeat.Bolt.NumberOfLines = 2;
					//	shearSeat.Bolt.NumberOfRows = (NB / 2);

					//	X1 = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5);
					//	x2 = X1 * (4 / beam.Shape.d) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					//	x3 = -X1 * 0.2 * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					//	NforCrippling = (beam.ShearForce - (X1 + x3)) / x2;
					//	X4 = 2.5 * beam.Shape.kdes * beam.Material.Fy * beam.Shape.tw;
					//	x5 = beam.Material.Fy * beam.Shape.tw;
					//	NforYielding = (beam.ShearForce - X4) / x5;
					//	n = Math.Max(beam.Shape.kdet, Math.Max(NforCrippling, NforYielding));
					//	if (n / beam.Shape.d <= 0.2)
					//	{
					//		X1 = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5);
					//		x2 = X1 * (3 / beam.Shape.d) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					//		NforCrippling = (beam.ShearForce - X1) / x2;
					//		n = Math.Max(beam.Shape.kdet, Math.Max(NforCrippling, NforYielding));
					//	}
					//	if ((n + ercl) <= ConstNum.THREEANDHALF_INCHES)
					//		anl2 = ConstNum.THREEANDHALF_INCHES;
					//	else if ((n + ercl) <= ConstNum.FOUR_INCHES)
					//		anl2 = ConstNum.FOUR_INCHES;
					//	else if ((n + ercl) <= ConstNum.FIVE_INCHES)
					//		anl2 = ConstNum.FIVE_INCHES;
					//	else if ((n + ercl) <= 2 * ConstNum.THREE_INCHES)
					//		anl2 = 2 * ConstNum.THREE_INCHES;
					//	else if ((n + ercl) <= ConstNum.THREE_INCHES + ConstNum.FOUR_INCHES)
					//		anl2 = ConstNum.THREE_INCHES + ConstNum.FOUR_INCHES;
					//	else if ((n + ercl) <= 2 * ConstNum.FOUR_INCHES)
					//		anl2 = 2 * ConstNum.FOUR_INCHES;
					//	else
					//	{
					//		Reporting.AddLine("Seat width too large. (NG)");
					//		return;
					//	}
					//	if (!shearSeat.Angle_User)
					//	{
					//		foreach (var shape in CommonDataStatic.AllShapes.Where(s => s.Value.TypeEnum == EShapeType.SingleAngle))
					//		{
					//			if (shape.Value.d >= ConstNum.FOUR_INCHES && shape.Value.b >= anl2 && shape.Value.t >= ConstNum.THREE_EIGHTS_INCH)
					//			{
					//				shearSeat.Angle = shape.Value;
					//				break;
					//			}
					//		}
					//	}
					//	shearSeat.Bolt.SpacingTransvDir = shearSeat.Bolt.SpacingTransvDir;
					//	shearSeat.Angle.g0 = ercl + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange;
					//	shearSeat.Angle.g1 = Gage;
					//	shearSeat.Angle.g2 = shearSeat.Bolt.SpacingTransvDir;

					//	tss = beam.ShearForce / (ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(beam.Material.Fy, shearSeat.Material.Fy) * (anl2 - ercl));
					//	tss = Math.Max(tss, beam.Shape.tw * beam.Material.Fy / shearSeat.Material.Fy);
					//	switch (shearSeat.Stiffener)
					//	{
					//		case ESeatStiffener.Tee:
					//			WidthThickness = 0.75 * Math.Pow(ConstNum.ELASTICITY, 0.5) / Math.Sqrt(shearSeat.Material.Fy);
					//			foreach (var shape in CommonDataStatic.AllShapes.Where(s => s.Value.TypeEnum == EShapeType.WideFlange))
					//			{
					//				if (shape.Value.TypeEnum == EShapeType.WideFlange)
					//				{
					//					ratio = (shape.Value.d - shape.Value.tf) / shape.Value.tw;
					//					if (shape.Value.tw >= tss && shape.Value.d >= anl2 && ratio <= WidthThickness)
					//					{
					//						if ((CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn) && shape.Value.bf >= beam.GageOnColumn + 2 * shearSeat.Bolt.EdgeDistTransvDir)
					//						{
					//							shearSeat.StiffenerTee = shape.Value;
					//							break;
					//						}
					//						if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && shape.Value.bf <= column.Shape.t && shape.Value.bf >= shearSeat.Bolt.SpacingLongDir + 2 * shearSeat.Bolt.EdgeDistTransvDir)
					//						{
					//							shearSeat.StiffenerTee = shape.Value;
					//							break;
					//						}
					//					}
					//				}
					//			}

					//			shearSeat.StiffenerWidth = shearSeat.StiffenerTee.d;
					//			shearSeat.StiffenerThickness = shearSeat.StiffenerTee.tw;
					//			shearSeat.StiffenerFlangeTh = shearSeat.StiffenerTee.tf;
					//			shearSeat.StiffenerFlangeWidth = shearSeat.StiffenerTee.bf;

					//			g1 = Math.Max(shearSeat.Bolt.EdgeDistLongDir, shearSeat.Bolt.Diameter + ConstNum.HALF_INCH);
					//			g = Math.Max(NumberFun.ConvertFromFraction(26), g1);
					//			shearSeat.AngleLength = NumberFun.Round(shearSeat.StiffenerFlangeWidth, 8);
					//			shearSeat.StiffenerLength = (shearSeat.Bolt.NumberOfRows - 1) * shearSeat.Bolt.SpacingTransvDir + shearSeat.Bolt.EdgeDistLongDir + g;
					//			break;
					//		case ESeatStiffener.L2:
					//			tss = tss / 2;
					//			foreach (var shape in CommonDataStatic.ShapesDoubleAngle)
					//			{
					//				if (shape.Value.t >= tss && shape.Value.d >= anl2)
					//				{
					//					if ((CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn) && shape.Value.b >= shearSeat.Bolt.SpacingLongDir / 2 + shearSeat.Bolt.EdgeDistTransvDir)
					//					{
					//						shearSeat.StiffenerAngle = shape.Value.ShallowCopy();
					//						break;
					//					}
					//					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && 2 * shape.Value.b <= column.Shape.t && shape.Value.b >= shearSeat.Bolt.SpacingLongDir / 2 + shearSeat.Bolt.EdgeDistTransvDir)
					//					{
					//						shearSeat.StiffenerAngle = shape.Value.ShallowCopy();
					//						break;
					//					}
					//				}
					//			}

					//			shearSeat.StiffenerWidth = shearSeat.StiffenerAngle.d;
					//			shearSeat.StiffenerThickness = 2 * shearSeat.StiffenerAngle.t;
					//			shearSeat.StiffenerFlangeTh = shearSeat.StiffenerAngle.t;
					//			shearSeat.StiffenerFlangeWidth = 2 * shearSeat.StiffenerAngle.b;

					//			g1 = Math.Max(shearSeat.Bolt.EdgeDistLongDir, shearSeat.Bolt.Diameter + ConstNum.HALF_INCH);
					//			g = Math.Max(NumberFun.ConvertFromFraction(26), g1);
					//			shearSeat.AngleLength = NumberFun.Round(shearSeat.StiffenerFlangeWidth, 8);
					//			shearSeat.StiffenerLength = (shearSeat.Bolt.NumberOfRows - 1) * shearSeat.Bolt.SpacingTransvDir + shearSeat.Bolt.EdgeDistLongDir + g;
					//			shearSeat.Angle.g0 = ercl + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange;
					//			break;
					//	}
					//	if (shearSeat.TopAngle.t < ConstNum.QUARTER_INCH && !shearSeat.TopAngle_User)
					//	{
					//		shearSeat.TopAngle.t = ConstNum.QUARTER_INCH;
					//		shearSeat.TopAngle = CommonDataStatic.AllShapes.First(s => s.Value.Name == "L4X4X1/4").Value;
					//	}
					//	shearSeat.AngleLength = Math.Max(beam.GageOnFlange + 2 * shearSeat.Bolt.EdgeDistLongDir, beam.GageOnColumn + 2 * shearSeat.Bolt.EdgeDistLongDir);
					//	shearSeat.TopAngle.g0 = ercl + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange;
					//	shearSeat.Angle.g1 = 2.5 * ConstNum.ONE_INCH;
					//	break;
				case ESeatConnection.WeldedStiffened:
					MiscCalculationsWithReporting.SeatedBeamNewCalc(beam.MemberType, t, t, t, ref t, ref DesignSeat_Nbearing, ref FiRn, false);
					X1 = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5);
					x2 = X1 * (4 / beam.Shape.d) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					x3 = -X1 * 0.2 * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
					NforCrippling = (beam.ShearForce - (X1 + x3)) / x2;
					X4 = 2.5 * beam.Shape.kdes * beam.Material.Fy * beam.Shape.tw;
					x5 = beam.Material.Fy * beam.Shape.tw;
					NforYielding = (beam.ShearForce - X4) / x5;
					n = Math.Max(beam.Shape.kdet, Math.Max(NforCrippling, NforYielding));
					if (n / beam.Shape.d <= 0.2)
					{
						X1 = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5);
						x2 = X1 * (3 / beam.Shape.d) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5);
						NforCrippling = (beam.ShearForce - X1) / x2;
						n = Math.Max(beam.Shape.kdet, Math.Max(NforCrippling, NforYielding));
					}
					if (n < beam.Shape.kdet)
						n = beam.Shape.kdet;
					if (n + beam.EndSetback > 9 * ConstNum.ONE_INCH)
					{
						Reporting.AddLine("Beam bearing length (" + n + ConstUnit.Length + " ) required for web yielding or crippling is too long. (NG)");
						return;
					}
					if (!shearSeat.PlateWidth_User)
						shearSeat.PlateWidth = NumberFun.Round(n + ercl, 8);
					if (shearSeat.PlateWidth < ConstNum.FOUR_INCHES)
						shearSeat.PlateWidth = ConstNum.FOUR_INCHES;

					if (!shearSeat.StiffenerWidth_User)
						shearSeat.StiffenerWidth = shearSeat.PlateWidth;
					n = shearSeat.StiffenerWidth - ercl;
					tss = beam.ShearForce / (ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(beam.Material.Fy, shearSeat.Material.Fy) * n);
					tss = Math.Max(tss, beam.Shape.tw * beam.Material.Fy / shearSeat.Material.Fy);
					if (!shearSeat.StiffenerThickness_User)
						shearSeat.StiffenerThickness = NumberFun.Round(tss, 8);
					WidthThickness = 0.56 * Math.Pow(ConstNum.ELASTICITY / shearSeat.Material.Fy, 0.5);
					th = shearSeat.StiffenerWidth / WidthThickness;
					if (shearSeat.StiffenerThickness < th)
						shearSeat.StiffenerThickness = NumberFun.Round(th, 8);

					maximumStiffenerLength1 = shearSeat.PlateWidth / 0.2;
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange)
						maximumStiffenerLength2 = (column.Shape.bf - shearSeat.StiffenerThickness) / 0.4;
					else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
					{
						if (column.WebOrientation == EWebOrientation.OutOfPlane)
							BF = column.Shape.d - 3 * column.Shape.tf;
						else
							BF = column.Shape.bf - 3 * column.Shape.tf;

						maximumStiffenerLength2 = (BF - shearSeat.StiffenerThickness) / 0.4;
					}
					else
						maximumStiffenerLength2 = (column.Shape.t - shearSeat.StiffenerThickness) / 0.4;

					maximumStiffenerLength = Math.Min(maximumStiffenerLength1, maximumStiffenerLength2);
					minweld = CommonCalculations.MinimumWeld(SupThickness1, shearSeat.StiffenerThickness);
					w = minweld;
					if (shearSeat.PlateWidth >= 6 * ConstNum.ONE_INCH && w < NumberFun.ConvertFromFraction(5))
						w = NumberFun.ConvertFromFraction(5);
					tForWeld = 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * w / shearSeat.Material.Fu;

					if (tForWeld > shearSeat.StiffenerThickness)
						shearSeat.StiffenerThickness = NumberFun.Round(tForWeld, 8);
					e = 0.8 * shearSeat.StiffenerWidth;

					usefulweldsize = SupFu * SupThickness / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					w = Math.Min(w, usefulweldsize);
					do
					{
						a = Math.Pow(2.4 * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * w / beam.ShearForce, 2);
						B = -1;
						c = -16 * Math.Pow(e, 2);
						X1 = (-B + Math.Sqrt(Math.Pow(B, 2) - 4 * a * c)) / (2 * a);
						StiffenerLength = NumberFun.Round(Math.Sqrt(X1), 4);
						wp = w;
						if (maximumStiffenerLength < StiffenerLength)
							w += ConstNum.SIXTEENTH_INCH;
					} while (w != wp);

					if (w > usefulweldsize)
						Reporting.AddLine("Effective support thickness cannot develop the required weld size. (NG)");

					w = Math.Max(w, minweld);
					if (!shearSeat.WeldSizeSupport_User)
						shearSeat.WeldSizeSupport = w;
					if (!shearSeat.StiffenerLength_User)
						shearSeat.StiffenerLength = StiffenerLength;
					tShear = beam.ShearForce / (ConstNum.FIOMEGA1_0N * 0.6 * shearSeat.Material.Fy * shearSeat.StiffenerLength);
					if (shearSeat.StiffenerThickness < tShear)
						shearSeat.StiffenerThickness = NumberFun.Round(tShear, 8);
					t = 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * shearSeat.WeldSizeSupport / shearSeat.Material.Fu;
					if (shearSeat.StiffenerThickness < t)
						shearSeat.StiffenerThickness = NumberFun.Round(t, 8);
					if (!shearSeat.PlateThickness_User)
						shearSeat.PlateThickness = Math.Max(ConstNum.THREE_EIGHTS_INCH, NumberFun.Round(shearSeat.StiffenerThickness / 2, 8));
					if (!shearSeat.PlateLength_User)
						shearSeat.PlateLength = NumberFun.Round(Math.Max(0.4 * StiffenerLength + shearSeat.StiffenerThickness, beam.Shape.bf), 4);
					if (column.ShapeType == EShapeType.HollowSteelSection)
					{
						if (column.WebOrientation == EWebOrientation.OutOfPlane)
							column_T = column.Shape.d - 3 * column.Shape.tf;
						else
							column_T = column.Shape.bf - 3 * column.Shape.tf;
					}
					else
						column_T = column.Shape.t;

					if (!shearSeat.PlateLength_User && (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn) && shearSeat.PlateLength > column_T)
						shearSeat.PlateLength = Math.Max(0.4 * StiffenerLength + shearSeat.StiffenerThickness, column_T);
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && !rightBeam.IsActive && !leftBeam.IsActive)
					{
						if (!shearSeat.StiffenerLength_User)
							shearSeat.StiffenerLength = StiffenerLength - ConstNum.QUARTER_INCH;
						if (column.ShapeType == EShapeType.HollowSteelSection)
						{
							if (column.WebOrientation == EWebOrientation.OutOfPlane)
								BF = column.Shape.d - 3 * column.Shape.tf;
							else
								BF = column.Shape.bf - 3 * column.Shape.tf;
						}
						else
							BF = column.Shape.t;
						if (!shearSeat.StiffenerLength_User)
						{
							do
							{
								shearSeat.StiffenerLength = shearSeat.StiffenerLength + ConstNum.QUARTER_INCH;
								k = MiscCalculationsWithReporting.YieldLineFactor(memberType, false);
								if (column.ShapeType == EShapeType.HollowSteelSection)
								{
									e = 0.8 * shearSeat.StiffenerWidth;
									WSF = ConstNum.FIOMEGA0_9N * k * column.Material.Fy * shearSeat.StiffenerLength / (0.8 * 4);
									FiRn = WSF * Math.Pow(column.Shape.tf, 2) / shearSeat.StiffenerWidth;
									WSFR = ReducedWSF.ReducedWSFCalc(shearSeat.Angle.bf, shearSeat.StiffenerLength);
									Ru = WSFR * Math.Pow(column.Shape.tf, 2) / shearSeat.StiffenerWidth;
									if (Ru > 0)
										FiRn = Math.Min(FiRn, Ru);
								}
								else
								{
									e = 0.8 * shearSeat.StiffenerWidth;
									FiRn = ConstNum.FIOMEGA0_9N * k * shearSeat.StiffenerLength * Math.Pow(column.Shape.tw, 2) * column.Material.Fy / (4 * e);
								}
							} while (FiRn < beam.ShearForce && 0.4 * shearSeat.StiffenerLength + shearSeat.StiffenerThickness < BF);
						}
					}
					if (!shearSeat.PlateLength_User)
						shearSeat.PlateLength = NumberFun.Round(Math.Max(0.4 * shearSeat.StiffenerLength + shearSeat.StiffenerThickness, beam.Shape.bf), 4); // Int(-4 * Math.Max(0.4 * Seat.StiffenerLength + Seat.StiffenerThickness, Beam.BF)) / 4
					if (!shearSeat.PlateLength_User && CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && shearSeat.PlateLength > column_T)
					{
						shearSeat.PlateLength = Math.Max(0.4 * StiffenerLength + shearSeat.StiffenerThickness, column_T);
						if (shearSeat.PlateLength < column_T)
							Reporting.AddLine("Seat plate does not fit in the column web. (NG)");
					}

					if (shearSeat.Stiffener == ESeatStiffener.Tee)
					{
						WidthThickness = 0.75 * Math.Pow(ConstNum.ELASTICITY, 0.5) / Math.Sqrt(shearSeat.Material.Fy);
						if (!shearSeat.StiffenerWidth_User)
							shearSeat.StiffenerWidth = shearSeat.PlateWidth;
						foreach (var shape in CommonDataStatic.ShapesTee)
						{
							Shape tempShape = shape.Value.ShallowCopy();

							w = Math.Max(CommonCalculations.MinimumWeld(SupThickness1, tempShape.tw), CommonCalculations.MinimumWeld(SupThickness1, tempShape.tf));
							if (shearSeat.PlateWidth >= 6 * ConstNum.ONE_INCH && w < NumberFun.ConvertFromFraction(5))
								w = NumberFun.ConvertFromFraction(5);
							usefulw = Math.Min(SupThickness * column.Material.Fu / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx), tempShape.tw * shearSeat.Material.Fu / (1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx));
							e = 0.8 * shearSeat.StiffenerWidth;

							ratio = shearSeat.StiffenerWidth / tempShape.tw;
							shart1 = tempShape.tw >= shearSeat.StiffenerThickness && tempShape.d >= shearSeat.StiffenerLength + tempShape.tf;
							shart2 = tempShape.tf >= ConstNum.THREE_EIGHTS_INCH && ratio <= WidthThickness;
							Shart3 = tempShape.bf >= 0.4 * (tempShape.d - tempShape.tf) + tempShape.tw && tempShape.bf >= beam.Shape.bf + 2 * beam.Shape.tf;
							shart5 = shearSeat.WeldSizeSupport <= usefulw;

							if (shart1 && shart2 && Shart3 && shart5)
							{
								if ((CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn) && tempShape.bf <= column.Shape.bf + 2 * shearSeat.WeldSizeSupport || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && tempShape.bf <= column.Shape.d - 2 * column.Shape.kdes)
								{
									shearSeat.StiffenerTee = tempShape.ShallowCopy();
									break;
								}
								if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && tempShape.bf - 0.25 <= column.Shape.d - 2 * column.Shape.kdes)
								{
									shearSeat.StiffenerTee = tempShape.ShallowCopy();
									break;
								}
							}
						}

						if (!shearSeat.StiffenerThickness_User)
							shearSeat.StiffenerThickness = shearSeat.StiffenerTee.tw;
						shearSeat.StiffenerFlangeTh = shearSeat.StiffenerTee.tf;
						shearSeat.StiffenerFlangeWidth = shearSeat.StiffenerTee.bf;
						if (!shearSeat.StiffenerLength_User)
							shearSeat.StiffenerLength = shearSeat.StiffenerTee.d - shearSeat.StiffenerTee.tf;
						if (!shearSeat.PlateWidth_User)
							shearSeat.PlateWidth = shearSeat.StiffenerWidth;
						if (!shearSeat.PlateThickness_User)
							shearSeat.PlateThickness = shearSeat.StiffenerTee.tf;
						if (!shearSeat.PlateLength_User)
							shearSeat.PlateLength = shearSeat.StiffenerFlangeWidth;
					}

					if (!shearSeat.WeldSizeBeam_User)
						shearSeat.WeldSizeBeam = CommonCalculations.MinimumWeld(beam.Shape.tf, shearSeat.PlateThickness);
					if (beam.Shape.tf >= ConstNum.QUARTER_INCH)
						weldMax = beam.Shape.tf - ConstNum.SIXTEENTH_INCH;
					else
						weldMax = beam.Shape.tf;

					if (shearSeat.WeldSizeBeam > weldMax && !shearSeat.WeldSizeBeam_User)
						shearSeat.WeldSizeBeam = weldMax;
					if (shearSeat.TopAngle.t < ConstNum.QUARTER_INCH && !shearSeat.TopAngle_User)
					{
						if (CommonDataStatic.Units == EUnit.US)
							shearSeat.TopAngle = CommonDataStatic.AllShapes.First(s => s.Value.Name == "L4X4X1/4").Value;
						else
							shearSeat.TopAngle = CommonDataStatic.AllShapes.First(s => s.Value.Name == "L102X102X6.4").Value;
					}
					shearSeat.TopAngleLength = NumberFun.Round(beam.Shape.bf - ConstNum.ONEANDHALF_INCHES, 8);
					shearSeat.WeldSizeBeam = CommonCalculations.MinimumWeld(shearSeat.TopAngle.t, beam.Shape.tf);
					if (beam.Shape.tf >= ConstNum.QUARTER_INCH)
						weldMax = shearSeat.TopAngle.t - ConstNum.SIXTEENTH_INCH;
					else
						weldMax = shearSeat.TopAngle.t;

					if (shearSeat.WeldSizeBeam > weldMax && !shearSeat.WeldSizeBeam_User)
						shearSeat.WeldSizeBeam = weldMax;

					if (!shearSeat.WeldSizeSupport_User)
						shearSeat.WeldSizeSupport = CommonCalculations.MinimumWeld(shearSeat.StiffenerTee.tw, SupThickness1);

					if (shearSeat.TopAngle.t > ConstNum.QUARTER_INCH)
						weldMax = shearSeat.TopAngle.t - ConstNum.SIXTEENTH_INCH;
					else
						weldMax = shearSeat.TopAngle.t;

					if ( !shearSeat.WeldSizeSupport_User && shearSeat.WeldSizeSupport > weldMax)
						shearSeat.WeldSizeSupport = weldMax;
					break;
			}

			shearSeat.TopAngle.g0 = ercl + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange;
			shearSeat.TopAngle.g1 = ConstNum.TWOANDHALF_INCHES;
			shearSeat.Angle.g0 = ercl + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange;
			shearSeat.Angle.g1 = Math.Max(shearSeat.Angle.kdet + shearSeat.Bolt.BoltSize, ConstNum.TWOANDHALF_INCHES);
			shearSeat.Angle.g2 = shearSeat.Bolt.SpacingTransvDir;
			if (shearSeat.Stiffener == ESeatStiffener.Tee && shearSeat.Connection == ESeatConnection.WeldedStiffened)
			{
				if (!shearSeat.StiffenerThickness_User)
					shearSeat.StiffenerThickness = shearSeat.StiffenerTee.tw;
				shearSeat.StiffenerFlangeTh = shearSeat.StiffenerTee.tf;
				shearSeat.StiffenerFlangeWidth = shearSeat.StiffenerTee.bf;
				if (!shearSeat.StiffenerLength_User)
					shearSeat.StiffenerLength = shearSeat.StiffenerTee.d - shearSeat.StiffenerTee.tf;
			}
			if (shearSeat.Stiffener == ESeatStiffener.Tee && shearSeat.Connection == ESeatConnection.BoltedStiffenedPlate)
			{
				if (!shearSeat.StiffenerWidth_User)
					shearSeat.StiffenerWidth = shearSeat.StiffenerTee.d;
				if (!shearSeat.StiffenerThickness_User)
					shearSeat.StiffenerThickness = shearSeat.StiffenerTee.tw;
				shearSeat.StiffenerFlangeTh = shearSeat.StiffenerTee.tf;
				shearSeat.StiffenerFlangeWidth = shearSeat.StiffenerTee.bf;
				if (!shearSeat.PlateLength_User)
					shearSeat.PlateLength = NumberFun.Round(shearSeat.StiffenerFlangeWidth + ConstNum.ONE_INCH, 8);
				if (!shearSeat.PlateWidth_User)
					shearSeat.PlateWidth = NumberFun.Round(shearSeat.StiffenerWidth + ConstNum.HALF_INCH, 8);
				if (!shearSeat.PlateThickness_User)
					shearSeat.PlateThickness = ConstNum.THREE_EIGHTS_INCH;
				g1 = Math.Max(shearSeat.Bolt.EdgeDistLongDir, shearSeat.Bolt.BoltSize + ConstNum.HALF_INCH);
				g = Math.Max(NumberFun.ConvertFromFraction(26), g1);
				if (!shearSeat.StiffenerLength_User)
					shearSeat.StiffenerLength = (shearSeat.Bolt.NumberOfRows - 1) * shearSeat.Bolt.SpacingTransvDir + shearSeat.Bolt.EdgeDistLongDir + g;
			}

			if (shearSeat.Stiffener == ESeatStiffener.L2 && shearSeat.Connection == ESeatConnection.BoltedStiffenedPlate)
			{
				if (!shearSeat.StiffenerWidth_User)
					shearSeat.StiffenerWidth = shearSeat.StiffenerAngle.d;
				if (!shearSeat.StiffenerLength_User)
					shearSeat.StiffenerLength = 2 * shearSeat.StiffenerAngle.t;
				shearSeat.StiffenerFlangeTh = shearSeat.StiffenerAngle.t;
				shearSeat.StiffenerFlangeWidth = 2 * shearSeat.StiffenerAngle.b;

				g1 = Math.Max(shearSeat.Bolt.EdgeDistLongDir, shearSeat.Bolt.BoltSize + ConstNum.HALF_INCH);
				g = Math.Max(NumberFun.ConvertFromFraction(26), g1);
				if (!shearSeat.AngleLength_User)
					shearSeat.AngleLength = NumberFun.Round(shearSeat.StiffenerFlangeWidth, 8);
				if (!shearSeat.StiffenerLength_User)
					shearSeat.StiffenerLength = (shearSeat.Bolt.NumberOfRows - 1) * shearSeat.Bolt.SpacingTransvDir + shearSeat.Bolt.EdgeDistLongDir + g;
				shearSeat.Angle.g0 = ercl + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange;
			}

			//minweld = CommonCalculations.MinimumWeld(SupThickness1, shearSeat.StiffenerThickness);
			//while (w < minweld)
			//	w += ConstNum.SIXTEENTH_INCH;


			ercl = beam.EndSetback + ConstNum.QUARTER_INCH;
			Reporting.AddMainHeader(beam.ComponentName + " - " + beam.ShapeName + " Seated Shear Connection");
			Reporting.AddLine("Attachment Material: " + shearSeat.Material.Name);

			switch (shearSeat.Connection)
			{
				case ESeatConnection.Bolted:
					Reporting.AddHeader("Connection Type: Bolted Seat");
					n = shearSeat.Bolt.NumberOfRows * shearSeat.Bolt.NumberOfLines;
					Reporting.AddLine("Seat Angle: " + shearSeat.Angle.Name + " with " + n + " bolts " + shearSeat.Bolt.BoltName);
					Reporting.AddLine("Top Angle: " + shearSeat.TopAngle.Name + " (Bolts per Shop Practice)");
					if (shearSeat.TopAngle.t < ConstNum.QUARTER_INCH)
						Reporting.AddLine("Top Angle thickness must be at least " + ConstNum.QUARTER_INCH + ConstUnit.Length + " (NG)");

					Cap = n * shearSeat.Bolt.BoltStrength;

					Reporting.AddHeader("Bolts:");
					if (Cap >= beam.ShearForce)
						Reporting.AddCapacityLine(ConstString.DES_OR_ALLOWABLE + " Shear Strength = n * Fv = " + n + " * " + shearSeat.Bolt.BoltStrength + " = " + Cap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / Cap, "Bolts", memberType);
					else
						Reporting.AddCapacityLine(ConstString.DES_OR_ALLOWABLE + " Shear Strength = n * Fv = " + n + " * " + shearSeat.Bolt.BoltStrength + " = " + Cap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / Cap, "Bolts", memberType);

					Reporting.AddHeader("Bolt Bearing on Angle:");
					Fbs = CommonCalculations.SpacingBearing(shearSeat.Bolt.SpacingTransvDir, shearSeat.Bolt.HoleWidth, shearSeat.Bolt.BoltSize, shearSeat.Bolt.HoleType, shearSeat.Material.Fu, true);
					BearingCap = n * Fbs * shearSeat.Angle.t;
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength on Angle:");
					Reporting.AddLine("= n * Fbs * t");
					Reporting.AddLine("= " + n + " * " + Fbs + " * " + shearSeat.Angle.t);
					if (BearingCap >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + BearingCap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / BearingCap, "Bolt Bearing on Angle", memberType);
					else
						Reporting.AddCapacityLine("= " + BearingCap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / BearingCap, "Bolt Bearing on Angle", memberType);

					Reporting.AddHeader("Bolt Bearing on Support:");
					Fbs = CommonCalculations.SpacingBearing(shearSeat.Bolt.SpacingTransvDir, shearSeat.Bolt.HoleWidth, shearSeat.Bolt.BoltSize, shearSeat.Bolt.HoleType, SupFu, true);
					BearingCap = n * Fbs * SupThickness;
					Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Bearing Strength on Support");
					Reporting.AddLine("= n * Fbs * t");
					Reporting.AddLine("= " + n + " * " + Fbs + " * " + SupThickness);
					if (BearingCap >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + BearingCap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / BearingCap, "Bolt Bearing on Angle", memberType);
					else
						Reporting.AddCapacityLine("= " + BearingCap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / BearingCap, "Bolt Bearing on Angle", memberType);

					a = 1 / (2 * beam.Material.Fy * beam.Shape.tw);
					B = -5 / 4.0 * beam.Shape.kdes - shearSeat.Angle.kdet + ercl;
					c = -0.225 * shearSeat.Material.Fy * shearSeat.AngleLength * Math.Pow(shearSeat.Angle.t, 2);
					FiRn = (-B + Math.Sqrt(Math.Pow(B, 2) - 4 * a * c)) / (2 * a);
					n = FiRn / (beam.Material.Fy * beam.Shape.tw) - 2.5 * beam.Shape.kdes;
					if (n < 2.5 * beam.Shape.kdes)
					{
						a = 1 / (4 * beam.Material.Fy * beam.Shape.tw);
						B = -shearSeat.Angle.kdet + ercl;
						c = -0.225 * shearSeat.Material.Fy * shearSeat.AngleLength * Math.Pow(shearSeat.Angle.t, 2);
						FiRn = (-B + Math.Sqrt(Math.Pow(B, 2) - 4 * a * c)) / (2 * a);
						n = FiRn / (beam.Material.Fy * beam.Shape.tw) - 2.5 * beam.Shape.kdes;
						if (n < 0)
							n = 0;
					}
					if (n > shearSeat.Angle.b - ercl)
					{
						n = shearSeat.Angle.b - ercl;
						FiRn = (n + 2.5 * beam.Shape.kdes) * beam.Shape.tw * beam.Material.Fy;
					}

					Reporting.AddHeader("Beam Web and Angle");
					MiscCalculationsWithReporting.SeatedBeamNewCalc(beam.MemberType, shearSeat.AngleLength, shearSeat.Angle.kdet, shearSeat.Angle.t, ref ta, ref DesignSeat_Nbearing, ref FiRn, true);
					break;
				case ESeatConnection.Welded:
					Reporting.AddHeader("Connection Type: Welded");
					Reporting.AddLine("Seat Angle: " + shearSeat.Angle.Name);
					Reporting.AddLine("Top Angle: " + shearSeat.TopAngle.Name);
					if (shearSeat.TopAngle.t < ConstNum.QUARTER_INCH)
						Reporting.AddLine("Top Angle thickness must be at least " + ConstNum.QUARTER_INCH + ConstUnit.Length + " (NG)");
					Ls = beam.GageOnFlange + 2 * shearSeat.Bolt.MinEdgeSheared;
					if (shearSeat.AngleLength >= Ls)
						Reporting.AddLine("Seat Angle Length = " + shearSeat.AngleLength + " >=  g + 2 * e = " + Ls + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Seat Angle Length = " + shearSeat.AngleLength + " <<  g + 2 * e = " + Ls + ConstUnit.Length + " (NG)");

					string seatAngleText;

					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange)
					{
						Ls = column.Shape.bf - 2 * shearSeat.WeldSizeSupport;
						seatAngleText = " Column bf - 2 * w";
					}
					else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
					{
						if (column.WebOrientation == EWebOrientation.OutOfPlane)
							BF = column.Shape.d;
						else
							BF = column.Shape.bf;
						Ls = BF - 2 * (shearSeat.WeldSizeSupport + 1.5 * column.Shape.tf);
						seatAngleText = "(HSS Flat Width) - 2 * w";
					}
					else
					{
						Ls = column.Shape.t - 2 * shearSeat.WeldSizeSupport;
						seatAngleText = " (Column T - dist) - 2 * w";
					}
					if (shearSeat.AngleLength <= Ls)
						Reporting.AddLine("Seat Angle Length = " + shearSeat.AngleLength + " <= " + seatAngleText + " = " + Ls + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Seat Angle Length = " + shearSeat.AngleLength + " >> " + seatAngleText + " = " + Ls + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Check Beam Web and Angle");
					MiscCalculationsWithReporting.SeatedBeamNewCalc(beam.MemberType, shearSeat.AngleLength, shearSeat.Angle.kdet, shearSeat.Angle.t, ref ta, ref DesignSeat_Nbearing, ref FiRn, true);

					wmin = CommonCalculations.MinimumWeld(SupThickness1, shearSeat.Angle.t);
					Reporting.AddHeader("Angle to Support Weld:");
					if (wmin <= shearSeat.WeldSizeSupport)
						Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(wmin) + " <= " + CommonCalculations.WeldSize(shearSeat.WeldSizeSupport) + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(wmin) + " >> " + CommonCalculations.WeldSize(shearSeat.WeldSizeSupport) + ConstUnit.Length + " (NG)");
					if (shearSeat.Angle.t >= ConstNum.QUARTER_INCH)
						weldMax = shearSeat.Angle.t - ConstNum.SIXTEENTH_INCH;
					else
						weldMax = shearSeat.Angle.t;

					if (weldMax >= shearSeat.WeldSizeSupport)
						Reporting.AddLine("Maximum Weld Size = " + CommonCalculations.WeldSize(weldMax) + " >= " + CommonCalculations.WeldSize(shearSeat.WeldSizeSupport) + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Maximum Weld Size = " + CommonCalculations.WeldSize(weldMax) + " << " + CommonCalculations.WeldSize(shearSeat.WeldSizeSupport) + ConstUnit.Length + " (NG)");

					FiRn = MiscCalculationsWithReporting.SeatWeld(beam.MemberType, 0, Math.Min(shearSeat.Angle.b, shearSeat.Angle.d), Math.Max(shearSeat.Angle.b, shearSeat.Angle.d), true);
					wUseful = SupThickness * SupFu / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					RF = wUseful / shearSeat.WeldSizeSupport;

					string capacityText = "Strength Reduction Factor for Support Thickness (Rf)";
					Reporting.AddHeader(capacityText);
					Reporting.AddLine("= 1.414 * t * Fu / (Fexx * w)");
					Reporting.AddLine("= 1.414 * " + SupThickness + " * " + SupFu + " / (" + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + shearSeat.WeldSizeSupport + ")");
					if (RF >= 1)
					{
						Reporting.AddLine("= " + RF + ", No Reduction");
						if (FiRn >= beam.ShearForce)
							Reporting.AddCapacityLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + FiRn + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRn, capacityText, memberType);
						else
							Reporting.AddCapacityLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + FiRn + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRn, capacityText, memberType);
					}
					else
					{
						Reporting.AddLine("= " + RF);
						if (RF * FiRn >= beam.ShearForce)
							Reporting.AddCapacityLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + RF + " * " + FiRn + " = " + RF * FiRn + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRn, capacityText, memberType);
						else
							Reporting.AddCapacityLine("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + RF + " * " + FiRn + " = " + RF * FiRn + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRn, capacityText, memberType);
					}
					break;
				case ESeatConnection.BoltedStiffenedPlate:
					Reporting.AddHeader("Connection Type: Bolted Stiffened (Plate)");
					n = shearSeat.Bolt.NumberOfLines * shearSeat.Bolt.NumberOfRows;
					if (shearSeat.Connection == ESeatConnection.BoltedStiffenedPlate)
						Reporting.AddLine("Seat Plate: PL " + shearSeat.PlateLength + " X " + shearSeat.PlateWidth + " X " + shearSeat.PlateThickness + ConstUnit.Length);
					else
						Reporting.AddLine("Seat Angle: " + shearSeat.Angle.Name + " Length = " + shearSeat.AngleLength + ConstUnit.Length);

					if (shearSeat.Stiffener == ESeatStiffener.Tee)
						Reporting.AddLine("Stiffener: 2" + shearSeat.StiffenerTee.Name + " X " + shearSeat.StiffenerLength + ConstUnit.Length + " with (" + n + ") " + shearSeat.Bolt.BoltName + " Bolts");
					else
						Reporting.AddLine("Stiffener: " + shearSeat.StiffenerAngle.Name + " X " + shearSeat.StiffenerLength + ConstUnit.Length + " with (" + n + ") " + shearSeat.Bolt.BoltName + " Bolts");

					Reporting.AddLine("Top Angle: " + shearSeat.TopAngle.Name + " X " + shearSeat.TopAngleLength + ConstUnit.Length + " (Bolts per Shop Practice)");
					if (shearSeat.TopAngle.t < ConstNum.QUARTER_INCH)
						Reporting.AddLine("Top Angle thickness must be at least " + ConstNum.QUARTER_INCH + ConstUnit.Length + " (NG)");
					if (shearSeat.StiffenerWidth > ConstNum.FIVE_INCHES)
					{
						Reporting.AddLine("Stiffener width is greater than five inches.");
						Reporting.AddLine("Stiffener and Bolts may have to be designed as a bracket. (NG)");
					}

					Reporting.AddHeader("Bolt Shear:");
					Cap = n * shearSeat.Bolt.BoltStrength;
					if (Cap >= beam.ShearForce)
						Reporting.AddCapacityLine("Capacity = n * Fv = " + n + " * " + shearSeat.Bolt.BoltStrength + " = " + Cap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRn, "Bolt Shear", memberType);
					else
						Reporting.AddCapacityLine("Capacity = n * Fv = " + n + " * " + shearSeat.Bolt.BoltStrength + " = " + Cap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRn, "Bolt Shear", memberType);

					Reporting.AddHeader("Bolt Bearing on Stiffener:");
					Fbe = (int)CommonCalculations.EdgeBearing(shearSeat.Bolt.EdgeDistLongDir, (shearSeat.Bolt.HoleWidth), shearSeat.Bolt.BoltSize, shearSeat.Material.Fu, shearSeat.Bolt.HoleType, true);
					Fbs = CommonCalculations.SpacingBearing(shearSeat.Bolt.SpacingLongDir, shearSeat.Bolt.HoleWidth, shearSeat.Bolt.BoltSize, shearSeat.Bolt.HoleType, shearSeat.Material.Fu, true);
					BearingCap = (2 * Fbe + (n - 2) * Fbs) * shearSeat.StiffenerFlangeTh;

					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = (2 * Fbe + (n - 2) * Fbs) * t");
					Reporting.AddLine("= (2 * " + Fbe + " + (" + n + " - 2) * " + Fbs + ") * " + shearSeat.StiffenerFlangeTh);
					if (BearingCap >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + BearingCap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / BearingCap, "Bolt Bearing on Stiffener", memberType);
					else
						Reporting.AddCapacityLine("= " + BearingCap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / BearingCap, "Bolt Bearing on Stiffener", memberType);

					Reporting.AddHeader("Bolt Bearing on Support:");
					Fbs = CommonCalculations.SpacingBearing(shearSeat.Bolt.SpacingLongDir, shearSeat.Bolt.HoleWidth, shearSeat.Bolt.BoltSize, shearSeat.Bolt.HoleType, SupFu, true);
					BearingCap = n * Fbs * SupThickness;

					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = n * Fbs * t");
					Reporting.AddLine("= " + n + " * " + Fbs + " * " + SupThickness);
					if (BearingCap >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + BearingCap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / BearingCap, "Bolt Bearing on Support", memberType);
					else
						Reporting.AddCapacityLine("= " + BearingCap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / BearingCap, "Bolt Bearing on Support", memberType);

					Reporting.AddGoToHeader("Beam Check:");
					n = shearSeat.StiffenerWidth - ercl;
					Reporting.AddHeader("Beam Web Yielding:");
					Cap = ConstNum.FIOMEGA1_0N * (n + 2.5 * beam.Shape.kdes) * beam.Shape.tw * beam.Material.Fy;
					Reporting.AddHeader("Capacity = " + ConstString.FIOMEGA1_0 + " * (N + 2.5 * k) * tw * Fy");
					Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (" + n + " + 2.5 * " + beam.Shape.kdes + ") * " + beam.Shape.tw + " * " + beam.Material.Fy);
					if (Cap >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + Cap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / Cap, "Beam Web Yielding", memberType);
					else
						Reporting.AddCapacityLine("= " + Cap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / Cap, "Beam Web Yielding", memberType);

					Reporting.AddHeader("Beam Web Crippling:");
					if (n / beam.Shape.d <= 0.2)
					{
						Cap = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5) * (1 + n * (3 / beam.Shape.d) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5));
						Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Strength = " + ConstString.FIOMEGA0_75 + " * 0.4 * tw² * (E * Fy * tf / tw)^0.5 * (1 + (3 * N / d) * (tw / tf)^1.5)");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + beam.Shape.tw + "² * (" + ConstNum.ELASTICITY + " * " + beam.Material.Fy + " * " + beam.Shape.tf + " / " + beam.Shape.tw + ")^0.5 ");
						Reporting.AddLine("* (1 + (3 * " + n + "/ " + beam.Shape.d + ") * (" + beam.Shape.tw + " / " + beam.Shape.tf + ")^1.5)");
					}
					else
					{
						Cap = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5) * (1 + (4 * n / beam.Shape.d - 0.2) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5));
						Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Strength = " + ConstString.FIOMEGA0_75 + " * 0.4 * tw² * (E * Fy * tf / tw)^0.5 * (1 + (4 * N / d - 0.2) * (tw / tf)^1.5)");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + beam.Shape.tw + "² * (" + ConstNum.ELASTICITY + " * " + beam.Material.Fy + " * " + beam.Shape.tf + " / " + beam.Shape.tw + ")^0.5");
						Reporting.AddLine("* (1 + (4 *" + n + " / " + beam.Shape.d + " - 0.2) * (" + beam.Shape.tw + " / " + beam.Shape.tf + ")^1.5)");
					}
					if (Cap >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + Cap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / Cap, "Beam Web Crippling", memberType);
					else
						Reporting.AddCapacityLine("= " + Cap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / Cap, "Beam Web Crippling", memberType);

					Reporting.AddGoToHeader("Bearing Stress on Stiffener:");
					if (shearSeat.StiffenerThickness >= beam.Shape.tw * beam.Material.Fy / shearSeat.Material.Fy)
						Reporting.AddLine("Thickness = " + shearSeat.StiffenerThickness + " >= (tw * Fyb / Fys) = " + (beam.Shape.tw * beam.Material.Fy / shearSeat.Material.Fy) + ConstUnit.Length + ConstString.REPORT_OK_SPECIAL);
					else
						Reporting.AddLine("Thickness = " + shearSeat.StiffenerThickness + " << (tw * Fyb / Fys) = " + (beam.Shape.tw * beam.Material.Fy / shearSeat.Material.Fy) + ConstUnit.Length + ConstString.REPORT_NOGOOD_SPECIAL);

					Cap = ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(beam.Material.Fy, shearSeat.Material.Fy) * n * shearSeat.StiffenerThickness;
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " =  " + ConstString.FIOMEGA0_75 + " * 1.8 * Min(Fyb, Fys) * N * t");
					Reporting.AddLine("=  " + ConstString.FIOMEGA0_75 + " * 1.8 * Min(" + beam.Material.Fy + ", " + shearSeat.Material.Fy + ") * " + n + " * " + shearSeat.StiffenerThickness);
					if (Cap >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + Cap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / Cap, "Bearing Stress on Stiffener", memberType);
					else
						Reporting.AddCapacityLine("= " + Cap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / Cap, "Bearing Stress on Stiffener", memberType);

					Reporting.AddHeader("Stiffener Shear Strength:");
					switch (shearSeat.Stiffener)
					{
						case ESeatStiffener.Tee:
							Reporting.AddHeader("Yielding of Tee Flange:");
							FiRnYielding = 2 * ConstNum.FIOMEGA1_0N * 0.6 * shearSeat.StiffenerLength * shearSeat.StiffenerFlangeTh * shearSeat.Material.Fy;
							Reporting.AddLine(ConstString.PHI + " Rn = 2 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * L * tf * Fy");
							Reporting.AddLine("= 2 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + shearSeat.StiffenerLength + " * " + shearSeat.StiffenerFlangeTh + " * " + shearSeat.Material.Fy);
							if (FiRnYielding >= beam.ShearForce)
								Reporting.AddCapacityLine("= " + FiRnYielding + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRnYielding, "Yielding of Tee Flange", memberType);
							else
								Reporting.AddCapacityLine("= " + FiRnYielding + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRnYielding, "Yielding of Tee Flange", memberType);

							Reporting.AddHeader("Rupture of Tee Flange:");
							FiRnRupture = 2 * ConstNum.FIOMEGA0_75N * 0.6 * (shearSeat.StiffenerLength - shearSeat.Bolt.NumberOfRows * (shearSeat.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * shearSeat.StiffenerFlangeTh * shearSeat.Material.Fu;
							Reporting.AddLine(ConstString.PHI + " Rn = 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * (L - Nr * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * tf * Fu");
							Reporting.AddLine("= 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * (" + shearSeat.StiffenerLength + " - " + shearSeat.Bolt.NumberOfRows + "*(" + shearSeat.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + "))*" + shearSeat.StiffenerFlangeTh + " * " + shearSeat.Material.Fu);
							if (FiRnRupture >= beam.ShearForce)
								Reporting.AddCapacityLine("= " + FiRnRupture + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRnRupture, "Rupture of Tee Flange", memberType);
							else
								Reporting.AddCapacityLine("= " + FiRnRupture + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRnRupture, "Rupture of Tee Flange", memberType);

							Reporting.AddHeader("Yielding of Tee Stem:");
							FiRnYielding = ConstNum.FIOMEGA1_0N * 0.6 * shearSeat.StiffenerLength * shearSeat.StiffenerThickness * shearSeat.Material.Fy;
							Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.FIOMEGA1_0N + " * 0.6 * L * ts * Fy");
							Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + " * 0.6 * " + shearSeat.StiffenerLength + " * " + shearSeat.StiffenerThickness + " * " + shearSeat.Material.Fy);
							if (FiRnYielding >= beam.ShearForce)
								Reporting.AddCapacityLine("= " + FiRnYielding + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRnYielding, "Yielding of Tee Stem", memberType);
							else
								Reporting.AddCapacityLine("= " + FiRnYielding + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRnYielding, "Yielding of Tee Stem", memberType);
							break;
						case ESeatStiffener.L2:
							Reporting.AddHeader("Yielding:");
							FiRnYielding = 2 * ConstNum.FIOMEGA1_0N * 0.6 * shearSeat.StiffenerLength * shearSeat.StiffenerFlangeTh * shearSeat.Material.Fy;
							Reporting.AddLine(ConstString.PHI + " Rn = 2 * " + ConstNum.FIOMEGA1_0N + " * 0.6 * L * t * Fy");
							Reporting.AddLine("= 2 * " + ConstString.FIOMEGA1_0 + " * 0.6 *" + shearSeat.StiffenerLength + " * " + shearSeat.StiffenerFlangeTh + " * " + shearSeat.Material.Fy);
							if (FiRnYielding >= beam.ShearForce)
								Reporting.AddCapacityLine("= " + FiRnYielding + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRnYielding, "Yielding", memberType);
							else
								Reporting.AddCapacityLine("= " + FiRnYielding + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRnYielding, "Yielding", memberType);

							Reporting.AddHeader("Rupture:");
							FiRnRupture = 2 * ConstNum.FIOMEGA0_75N * 0.6 * (shearSeat.StiffenerLength - shearSeat.Bolt.NumberOfRows * (shearSeat.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH)) * shearSeat.StiffenerFlangeTh * shearSeat.Material.Fu;
							Reporting.AddLine(ConstString.PHI + " Rn = 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * (L - Nr * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t * Fu");
							Reporting.AddLine("= 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * (" + shearSeat.StiffenerLength + " - " + shearSeat.Bolt.NumberOfRows + "*(" + shearSeat.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + "))*" + shearSeat.StiffenerFlangeTh + " * " + shearSeat.Material.Fu);
							if (FiRnRupture >= beam.ShearForce)
								Reporting.AddCapacityLine("= " + FiRnRupture + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRnRupture, "Rupture", memberType);
							else
								Reporting.AddCapacityLine("= " + FiRnRupture + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRnRupture, "Rupture", memberType);
							break;
					}

					Reporting.AddHeader("Stiffener Width/Thickness Ratio:");
					switch (shearSeat.Stiffener)
					{
						case ESeatStiffener.Tee:
							WidthThickness = 0.75 * Math.Pow(ConstNum.ELASTICITY, 0.5) / Math.Sqrt(shearSeat.Material.Fy);
							Reporting.AddHeader("Maximum Allowed = 0.75 * " + ConstNum.ELASTICITY + "^0.5 / (Fy)^0.5 = " + WidthThickness);
							ratio = (shearSeat.StiffenerWidth - shearSeat.StiffenerFlangeTh) / shearSeat.StiffenerThickness;
							if (ratio <= WidthThickness)
								Reporting.AddLine("Width/Thickness = " + (shearSeat.StiffenerWidth - shearSeat.StiffenerFlangeTh) + " / " + shearSeat.StiffenerThickness + " = " + ratio + " <= " + WidthThickness + " (OK)");
							else
								Reporting.AddLine("Width/Thickness = " + (shearSeat.StiffenerWidth - shearSeat.StiffenerFlangeTh) + " / " + shearSeat.StiffenerThickness + " = " + ratio + " >> " + WidthThickness + " (NG)");
							break;
						case ESeatStiffener.L2:
							WidthThickness = 0.56 * Math.Pow(ConstNum.ELASTICITY / shearSeat.Material.Fy, 0.5);
							Reporting.AddHeader("Maximum Allowed = 0.56 * (E / Fy) ^ 0.5 = " + WidthThickness);
							ratio = (shearSeat.StiffenerWidth - shearSeat.StiffenerFlangeTh) / (shearSeat.StiffenerThickness / 2);
							if (ratio <= WidthThickness)
								Reporting.AddLine("Width/Thickness = " + (shearSeat.StiffenerWidth - shearSeat.StiffenerFlangeTh) + " / " + (shearSeat.StiffenerThickness / 2) + " = " + ratio + " <= " + WidthThickness + " (OK)");
							else
								Reporting.AddLine("Width/Thickness = " + (shearSeat.StiffenerWidth - shearSeat.StiffenerFlangeTh) + " / " + (shearSeat.StiffenerThickness / 2) + " = " + ratio + " >> " + WidthThickness + " (NG)");
							break;
					}
					break;
				case ESeatConnection.WeldedStiffened:
					Reporting.AddHeader("Connection Type: Welded Stiffened");

					if (shearSeat.Stiffener == ESeatStiffener.Plate)
					{
						Reporting.AddLine("Seat Plate: PL " + shearSeat.PlateLength + " X " + shearSeat.PlateWidth + " X " + shearSeat.PlateThickness + ConstUnit.Length);
						Reporting.AddLine("Stiffener: PL " + shearSeat.StiffenerLength + " X " + shearSeat.StiffenerWidth + " X " + shearSeat.StiffenerThickness + ConstUnit.Length);
						Reporting.AddLine("Top Angle: " + shearSeat.TopAngle.Name + " X " + shearSeat.TopAngleLength + ConstUnit.Length);
						if (shearSeat.TopAngle.t < ConstNum.QUARTER_INCH)
							Reporting.AddLine("Top Angle thickness must be at least " + ConstNum.QUARTER_INCH + ConstUnit.Length + " (NG)");
					}
					else
					{
						Reporting.AddLine("Seat/Stiffener: " + shearSeat.StiffenerTee.Name + " X " + shearSeat.StiffenerWidth + ConstUnit.Length);
						Reporting.AddLine("Top Angle: " + shearSeat.TopAngle.Name + " X " + shearSeat.TopAngleLength + ConstUnit.Length);
						if (shearSeat.TopAngle.t < ConstNum.QUARTER_INCH)
							Reporting.AddLine("Top Angle thickness must be at least " + ConstNum.QUARTER_INCH + ConstUnit.Length + " (NG)");
					}

					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
						MiscCalculationsWithReporting.HSSColumnYieldLineCheck(beam.MemberType);

					Reporting.AddGoToHeader("Beam Check:");
					n = shearSeat.StiffenerWidth - ercl;
					Reporting.AddHeader("Beam Web Yielding:");
					Cap = ConstNum.FIOMEGA1_0N * (n + 2.5 * beam.Shape.kdes) * beam.Shape.tw * beam.Material.Fy;
					Reporting.AddLine("Capacity = " + ConstString.FIOMEGA1_0 + " * (N + 2.5 * k) * tw * Fy ");
					Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * (" + n + " + 2.5 * " + beam.Shape.kdes + ") * " + beam.Shape.tw + " * " + beam.Material.Fy);
					if (Cap >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + Cap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / Cap, "Beam Web Yielding", memberType);
					else
						Reporting.AddCapacityLine("= " + Cap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / Cap, "Beam Web Yielding", memberType);

					if (n / beam.Shape.d <= 0.2)
					{
						Cap = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5) * (1 + n * (3 / beam.Shape.d) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5));
						Reporting.AddHeader("Beam Web Crippling:");
						Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Strength = " + ConstString.FIOMEGA0_75 + " * 0.4 * tw² * (E * Fy * tf / tw)^0.5 *(1 + (3 * N / d) * (tw / tf)^1.5)");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + beam.Shape.tw + "² * (" + ConstNum.ELASTICITY + " * " + beam.Material.Fy + " * " + beam.Shape.tf + " / " + beam.Shape.tw + ")^0.5 ");
						Reporting.AddLine("* (1 + (3 * " + n + "/ " + beam.Shape.d + ") * (" + beam.Shape.tw + " / " + beam.Shape.tf + ")^1.5)");
					}
					else
					{
						Cap = ConstNum.FIOMEGA0_75N * 0.4 * Math.Pow(beam.Shape.tw, 2) * Math.Pow(ConstNum.ELASTICITY * beam.Material.Fy * beam.Shape.tf / beam.Shape.tw, 0.5) * (1 + (4 * n / beam.Shape.d - 0.2) * Math.Pow(beam.Shape.tw / beam.Shape.tf, 1.5));
						Reporting.AddHeader("Beam Web Crippling:");
						Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Strength = " + ConstString.FIOMEGA0_75 + " * 0.4 * tw² * (E * Fy * tf / tw)^0.5 * (1 + (4 * N / d - 0.2) * (tw / tf)^1.5)");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4 * " + beam.Shape.tw + "² * (" + ConstNum.ELASTICITY + " * " + beam.Material.Fy + " * " + beam.Shape.tf + " / " + beam.Shape.tw + ")^0.5 ");
						Reporting.AddLine(" * (1 + (4 * " + n + " / " + beam.Shape.d + " - 0.2) * (" + beam.Shape.tw + " / " + beam.Shape.tf + ")^1.5)");
					}
					if (Cap >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + Cap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / Cap, "Beam Web Crippling", memberType);
					else
						Reporting.AddCapacityLine("= " + Cap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / Cap, "Beam Web Crippling", memberType);

					switch (shearSeat.Stiffener)
					{
						case ESeatStiffener.Plate:
							capacityText = "Bearing Stress on Stiffener";
							break;
						default:
							capacityText = "Bearing Stress on Tee Web";
							break;
					}

					Reporting.AddGoToHeader(capacityText + ":");

					if (shearSeat.StiffenerThickness >= beam.Shape.tw * beam.Material.Fy / shearSeat.Material.Fy)
						Reporting.AddLine("Thickness = " + shearSeat.StiffenerThickness + " >= (tw * Fyb / Fys) = " + (beam.Shape.tw * beam.Material.Fy / shearSeat.Material.Fy) + ConstUnit.Length + ConstString.REPORT_OK_SPECIAL);
					else
						Reporting.AddLine("Thickness = " + shearSeat.StiffenerThickness + " << (tw * Fyb / Fys) = " + (beam.Shape.tw * beam.Material.Fy / shearSeat.Material.Fy) + ConstUnit.Length + ConstString.REPORT_NOGOOD_SPECIAL);

					Reporting.AddLine(string.Empty);
					Cap = ConstNum.FIOMEGA0_75N * 1.8 * Math.Min(beam.Material.Fy, shearSeat.Material.Fy) * n * shearSeat.StiffenerThickness;
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + ConstString.FIOMEGA0_75 + " * 1.8 * Min(Fyb, Fys) * N * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 1.8 * Min(" + beam.Material.Fy + ", " + shearSeat.Material.Fy + ") * " + n + " * " + shearSeat.StiffenerThickness);
					if (Cap >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + Cap + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / Cap, capacityText, memberType);
					else
						Reporting.AddCapacityLine("= " + Cap + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / Cap, capacityText, memberType);

					switch (shearSeat.Stiffener)
					{
						case ESeatStiffener.Plate:
							capacityText = "Seat/Stiffener to Support Weld:";
							Reporting.AddGoToHeader(capacityText);
							break;
						default:
							capacityText = "Tee Stem to Support Weld:";
							Reporting.AddGoToHeader(capacityText);
							break;
					}

					w = CommonCalculations.MinimumWeld(SupThickness1, shearSeat.StiffenerThickness);
					if (w <= shearSeat.WeldSizeSupport)
						Reporting.AddLine("Minimum Size = " + CommonCalculations.WeldSize(w) + " <= " + CommonCalculations.WeldSize(shearSeat.WeldSizeSupport) + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Minimum Size = " + CommonCalculations.WeldSize(w) + " >> " + CommonCalculations.WeldSize(shearSeat.WeldSizeSupport) + ConstUnit.Length + " (NG)");
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && shearSeat.StiffenerLength >= 9 && shearSeat.PlateWidth <= 4)
					{
						if ((column.Shape.Code == 225 || column.Shape.Code == 198) && shearSeat.WeldSizeSupport > ConstNum.QUARTER_INCH)
						{
							Reporting.AddHeader("For a " + column.ShapeName + " column and the calculated or specified");
							Reporting.AddHeader("stiffener dimensions, the weld size is limited to " + ConstNum.QUARTER_INCH + ConstUnit.Length);
							Reporting.AddHeader("which is less then the calculated or specified weld size. (NG)");
						}
					}

					e = 0.8 * shearSeat.PlateWidth;
					Reporting.AddLine(string.Empty);
					Reporting.AddLine("Eccentricity (e) = 0.8 * W = " + e + ConstUnit.Length);
					Reporting.AddLine(string.Empty);

					FiRn = 2.4 * Math.Pow(shearSeat.StiffenerLength, 2) * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * shearSeat.WeldSizeSupport / Math.Sqrt(16 * Math.Pow(e, 2) + Math.Pow(shearSeat.StiffenerLength, 2));
					Reporting.AddLine(ConstString.PHI + " Rn = 2.4 * (L²) * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx * w / (16 * e² + L²)^0.5");
					Reporting.AddLine("= 2.4 * (" + shearSeat.StiffenerLength + "²) * " + ConstString.FIOMEGA0_75 + " * 0.4242 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + shearSeat.WeldSizeSupport + " / (16 * " + e + "² + " + shearSeat.StiffenerLength + "²)^0.5");
					if (FiRn >= beam.ShearForce)
						Reporting.AddCapacityLine("= " + FiRn + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRn, capacityText, beam.MemberType);
					else
						Reporting.AddCapacityLine("= " + FiRn + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRn, capacityText, beam.MemberType);

					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && (rightBeam.IsActive || leftBeam.IsActive))
					{
						Reporting.AddHeader("Column Web Strength:");
						k = MiscCalculationsWithReporting.YieldLineFactor(memberType, true);
						FiRn = ConstNum.FIOMEGA0_9N * k * shearSeat.StiffenerLength * Math.Pow(column.Shape.tw, 2) * column.Material.Fy / (4 * e);
						Reporting.AddLine("Yield line factor (k) = " + k);
						Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * k * L * tw² * Fy / (4 * e)");
						Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + k + " * " + shearSeat.StiffenerLength + " * (" + column.Shape.tw + "²) * " + column.Material.Fy + " / (4 * " + e + ")");
						if (FiRn >= beam.ShearForce)
							Reporting.AddCapacityLine("= " + FiRn + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRn, "Column Web Strength", beam.MemberType);
						else
							Reporting.AddCapacityLine("= " + FiRn + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRn, "Column Web Strength", beam.MemberType);
					}

					Ls = Math.Max(0.4 * shearSeat.StiffenerLength + shearSeat.StiffenerThickness, beam.Shape.bf);
					switch (shearSeat.Stiffener)
					{
						case ESeatStiffener.Plate:
							Reporting.AddGoToHeader("Check Seat and Stiffener Sizes:");
							if (shearSeat.PlateLength >= Ls)
								Reporting.AddLine("Seat Plate Length = " + shearSeat.PlateLength + " >= Max(0.4 * L + t; bf) = " + Ls + ConstUnit.Length + " (OK)");
							else
								Reporting.AddLine("Seat Plate Length = " + shearSeat.PlateLength + " << Max(0.4 * L + t; bf) = " + Ls + ConstUnit.Length + " (NG)");

							if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
							{
								if (column.Shape.bf < 0.4 * shearSeat.StiffenerLength + shearSeat.StiffenerThickness)
									Reporting.AddLine("Column bf = " + column.Shape.bf + " << 0.4 * L + t = " + (0.4 * shearSeat.StiffenerLength + shearSeat.StiffenerThickness) + ConstUnit.Length + " (NG)");
								else
									Reporting.AddLine("Column bf = " + column.Shape.bf + " >= 0.4 * L + t = " + (0.4 * shearSeat.StiffenerLength + shearSeat.StiffenerThickness) + ConstUnit.Length + " (OK)");
							}
							else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
							{
								if (column.Shape.t >= 0.4 * shearSeat.StiffenerLength + shearSeat.StiffenerThickness)
									Reporting.AddLine("Column_T = " + column.Shape.t + " >= 0.4 * L + t = " + 0.4 * shearSeat.StiffenerLength + " " + shearSeat.StiffenerThickness + ConstUnit.Length + " (OK)");
								else
									Reporting.AddLine("Column_T = " + column.Shape.t + " << 0.4 * L + t = " + 0.4 * shearSeat.StiffenerLength + " " + shearSeat.StiffenerThickness + ConstUnit.Length + " (NG)");
							}

							Ws = 0.2 * shearSeat.StiffenerLength;
							if (shearSeat.PlateLength >= Ws)
								Reporting.AddLine("Seat Plate Width = " + shearSeat.PlateWidth + " >= 0.2 * L = " + Ws + ConstUnit.Length + " (OK)");
							else
								Reporting.AddLine("Seat Plate Width = " + shearSeat.PlateWidth + " << 0.2 * L = " + Ws + ConstUnit.Length + " (NG)");

							Reporting.AddHeader("Shear Yielding of Stiffener:");
							FiRnYielding = ConstNum.FIOMEGA1_0N * 0.6 * shearSeat.StiffenerLength * shearSeat.StiffenerThickness * shearSeat.Material.Fy;
							Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA1_0 + " * 0.6 * L * ts * Fy");
							Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * 0.6 * " + shearSeat.StiffenerLength + " * " + shearSeat.StiffenerThickness + " * " + shearSeat.Material.Fy);
							if (FiRnYielding >= beam.ShearForce)
								Reporting.AddCapacityLine("= " + FiRnYielding + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRnYielding, "Shear Yielding of Stiffener", memberType);
							else
								Reporting.AddCapacityLine("= " + FiRnYielding + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRnYielding, "Shear Yielding of Stiffener", memberType);

							Reporting.AddHeader("Stiffener Width / Thickness Ratio:");
							WidthThickness = 0.56 * Math.Pow(ConstNum.ELASTICITY / shearSeat.Material.Fy, 0.5);
							Reporting.AddHeader("Maximum Allowed = 0.56 * (E / Fy) ^ 0.5 = " + WidthThickness);
							break;
						case ESeatStiffener.Tee:
							Reporting.AddGoToHeader("Check Tee Size:");
							if (shearSeat.PlateLength >= Ls)
								Reporting.AddLine("Flange Width = " + shearSeat.PlateLength + " >= Max(0.4 * L + t, bf) = " + Ls + ConstUnit.Length + " (OK)");
							else
								Reporting.AddLine("Flange Width = " + shearSeat.PlateLength + " << Max(0.4 * L + t, bf) = " + Ls + ConstUnit.Length + " (NG)");

							if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange || CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
							{
								if (column.Shape.bf < 0.4 * shearSeat.StiffenerLength + shearSeat.StiffenerThickness)
									Reporting.AddLine("Column_bf = " + column.Shape.bf + " << 0.4 * L + t = " + (0.4 * shearSeat.StiffenerLength + shearSeat.StiffenerThickness) + ConstUnit.Length + " (NG)");
								else
									Reporting.AddLine("Column_bf = " + column.Shape.bf + " >= 0.4 * L + t = " + (0.4 * shearSeat.StiffenerLength + shearSeat.StiffenerThickness) + ConstUnit.Length + " (OK)");
							}
							else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb)
							{
								if (column.Shape.t < 0.4 * shearSeat.StiffenerLength + shearSeat.StiffenerThickness)
									Reporting.AddLine("Column_T = " + column.Shape.t + " << 0.4 * L + t = " + (0.4 * shearSeat.StiffenerLength + shearSeat.StiffenerThickness) + ConstUnit.Length + " (NG)");
								else
									Reporting.AddLine("Column_T = " + column.Shape.t + " >= 0.4 * L + t = " + (0.4 * shearSeat.StiffenerLength + shearSeat.StiffenerThickness) + ConstUnit.Length + " (OK)");
							}

							Reporting.AddHeader("Stem Width/Thickness Ratio:");
							WidthThickness = 0.75 * Math.Pow(ConstNum.ELASTICITY, 0.5) / Math.Sqrt(shearSeat.Material.Fy);
							Reporting.AddLine("Maximum Allowed = 0.75 * " + ConstNum.ELASTICITY + "^0.5 / (Fy)^0.5 = " + WidthThickness);
							break;
					}

					ratio = shearSeat.StiffenerWidth / shearSeat.StiffenerThickness;
					if (ratio <= WidthThickness)
						Reporting.AddLine("Width / Thickness = " + shearSeat.StiffenerWidth + " / " + shearSeat.StiffenerThickness + " = " + ratio + " <= " + WidthThickness + " (OK)");
					else
						Reporting.AddLine("Width / Thickness = " + shearSeat.StiffenerWidth + " / " + shearSeat.StiffenerThickness + " = " + ratio + " >> " + WidthThickness + " (NG)");

					Reporting.AddHeader("Stiffener Thickness to Develop Weld Strength:");
					t = 1.414 * shearSeat.WeldSizeSupport * CommonDataStatic.Preferences.DefaultElectrode.Fexx / shearSeat.Material.Fu;
					Reporting.AddLine("tReq = 1.414 * w * Fexx / Fu");
					Reporting.AddLine("= 1.414 * " + shearSeat.WeldSizeSupport + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " / " + shearSeat.Material.Fu);
					if (t <= shearSeat.StiffenerThickness)
						Reporting.AddLine("= " + t + " <= " + shearSeat.StiffenerThickness + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + t + " >> " + shearSeat.StiffenerThickness + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Stiffener Thickness v.s. Beam Web Thickness:");
					t = beam.Material.Fy * beam.Shape.tw / shearSeat.Material.Fy;
					Reporting.AddLine("tReq = Fyw * tw / Fys");
					Reporting.AddLine("= " + beam.Material.Fy + " * " + beam.Shape.tw + " / " + shearSeat.Material.Fy);
					if (t <= shearSeat.StiffenerThickness)
						Reporting.AddLine("= " + t + " <= " + shearSeat.StiffenerThickness + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + t + " >> " + shearSeat.StiffenerThickness + ConstUnit.Length + " (NG)");

					Reporting.AddHeader("Seat Pl./Tee Fl. Thickness to Develop Weld Strength:");
					t = 0.707 * shearSeat.WeldSizeSupport * CommonDataStatic.Preferences.DefaultElectrode.Fexx / shearSeat.Material.Fu;
					Reporting.AddLine("tReq = 0.707 * w * Fexx / Fu");
					Reporting.AddLine("= 0.707 * " + shearSeat.WeldSizeSupport + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + "/ " + shearSeat.Material.Fu);
					if (t <= shearSeat.PlateThickness)
						Reporting.AddLine("= " + t + " <= " + shearSeat.PlateThickness + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + t + " >> " + shearSeat.PlateThickness + ConstUnit.Length + " (NG)");

					usefulweldsize = SupFu * SupThickness / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
					Reporting.AddHeader("Useful Weld Size Based on Effective Support Thickness:");
					Reporting.AddLine("wu = Fu * t_eff / (0.707" + " * Fexx)");
					Reporting.AddLine("= " + SupFu + " * " + SupThickness + " / (0.707" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
					if (usefulweldsize >= shearSeat.WeldSizeSupport)
						Reporting.AddLine("= " + usefulweldsize + " >= " + shearSeat.WeldSizeSupport + ConstUnit.Length + " (OK)");
					else
					{
						Reporting.AddHeader("= " + usefulweldsize + " << " + shearSeat.WeldSizeSupport + ConstUnit.Length);
						Reporting.AddHeader("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + " Using wu  ");
						e = 0.8 * shearSeat.PlateWidth;
						Reporting.AddHeader("Eccentricity, e = 0.8 * W = " + e + ConstUnit.Length);
						FiRn = 2.4 * Math.Pow(shearSeat.StiffenerLength, 2) * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * usefulweldsize / Math.Sqrt(16 * Math.Pow(e, 2) + Math.Pow(shearSeat.StiffenerLength, 2));
						Reporting.AddLine(ConstString.PHI + " Rn = 2.4 * (L²) * " + ConstString.FIOMEGA0_75 + " * 0.4242 * Fexx * wu / (16 * e² + L²)^0.5");
						Reporting.AddLine("= 2.4 * (" + shearSeat.StiffenerLength + "²) * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + usefulweldsize + "/(16*" + e + "² + " + shearSeat.StiffenerLength + "²)^0.5");
						if (FiRn >= beam.ShearForce)
							Reporting.AddCapacityLine("= " + FiRn + " >= " + beam.ShearForce + ConstUnit.Force + " (OK)", beam.ShearForce / FiRn, "Useful Weld Size", memberType);
						else
							Reporting.AddCapacityLine("= " + FiRn + " << " + beam.ShearForce + ConstUnit.Force + " (NG)", beam.ShearForce / FiRn, "Useful Weld Size", memberType);
					}
					break;
			}
		}
	}
}