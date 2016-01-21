using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class DesignEndPlate
	{
		internal static void CalcDesignEndPlate(EMemberType memberType, double H_Tens, double H_Comp, double V, int Mom)
		{
			double weldforcalc = 0;
			double Ta = 0;
			double Ta_Web = 0;
			double edg = 0;
			double Ant = 0;
			double Agt = 0;
			double Agv = 0;
			double Anv = 0;
			double Ag = 0;
			double BearingCap = 0;
			double Ball_Web = 0;
			double cap = 0;
			bool loopAgain = false;
			double wReq = 0;
			double minweld = 0;
			double c1 = 0;
			double Cnst = 0;
			int k = 0;
			double Theta = 0;
			double usefulweldsize = 0;
			double Th = 0;
			double tTemp = 0;
			double t_reqForBearingCap = 0;
			double t_reqForBlockShearStrength = 0;
			double Lnt = 0;
			double Lgt = 0;
			double t_reqForGrossShearCap = 0;
			double Lgv = 0;
			double t_reqForNetShearCap = 0;
			double Lnv = 0;
			double Fbs = 0;
			double Fbe = 0;
			double columngagemax = 0;
			double columngagemin = 0;
			double GageCol2 = 0;
			double GageCol1 = 0;
			double re = 0;
			double tmax = 0;
			int SumY = 0;
			double Sum = 0;
			double Reduction = 0;
			double EndPlateMax = 0;
			double EndPlateMin = 0;
			double t_RequiredForTension = 0;
			double FiRn = 0;
			double FiRnPerBolt = 0;
			double b = 0;
			double a = 0;
			double Ball = 0;
			double t = 0;
			int NforTV = 0;
			int N = 0;
			double width1 = 0;
			double RequiredMinLength = 0;
			double L = 0;
			double H = 0;
			double Ru = 0;
			double BF = 0;
			double maxbolts = 0;
			double thick = 0;
			double boltDistToHSSside = 0;

			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var component = CommonDataStatic.DetailDataDict[memberType];
			var componentIsBeam = memberType == EMemberType.LeftBeam || memberType == EMemberType.RightBeam;
			var componentIsBrace = (memberType == EMemberType.UpperRight || memberType == EMemberType.LowerRight ||
			                        memberType == EMemberType.UpperLeft || memberType == EMemberType.LowerLeft);

			H = component.BraceConnect.Gusset.Hc;

			if (component.IsActive && !component.GussetWeldedToColumn)
			{
				if (component.BraceConnect.Gusset.Thickness == 0)
					component.WinConnect.ShearEndPlate.Thickness = ConstNum.THREE_EIGHTS_INCH;
				
				if (component.BraceConnect.Gusset.ColumnSideSetback == 0)
					component.BraceConnect.Gusset.ColumnSideSetback = ConstNum.HALF_INCH;
				else if (!component.BraceConnect.Gusset.ColumnSideSetback_User)
					component.BraceConnect.Gusset.ColumnSideSetback = component.WinConnect.ShearEndPlate.Thickness + component.WinConnect.ShearEndPlate.ConnectionPlateThickness;

				if (componentIsBeam)
				{
					thick = component.Shape.tw;
					maxbolts = (int)Math.Ceiling((component.Shape.t - 2 * component.WinConnect.ShearEndPlate.Bolt.EdgeDistTransvDir) / component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir) + 1;
				}
				else
					thick = component.BraceConnect.Gusset.Thickness;

				if (column.ShapeType == EShapeType.HollowSteelSection)
				{
					if (column.WebOrientation == EWebOrientation.InPlane)
						BF = column.Shape.bf;
					else
						BF = column.Shape.d;
					if (boltDistToHSSside == 0.0)
						boltDistToHSSside = Math.Max(2, component.WinConnect.ShearEndPlate.Bolt.BoltSize + ConstNum.HALF_INCH);
					Ru = Math.Pow(Math.Pow(V, 2) + Math.Pow(H, 2), 0.5);
					L = Ru / (2 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * (5 / 8.0) * column.Shape.tf);
					if (double.IsNaN(L))
						L = 0;
					else
					{
						do
						{
							if (L != 0)
								Ru = Math.Pow(Math.Pow(V, 2) + Math.Pow(H + 3 * Mom / L, 2), 0.5);
							RequiredMinLength = Ru / (2 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * (5 / 8.0) * column.Shape.tf);
							if (RequiredMinLength == 0 || Math.Abs(L - RequiredMinLength) / RequiredMinLength < 0.01)
								break;
							else
								L = RequiredMinLength;
						} while (true);
					}
				}

				if (column.ShapeType == EShapeType.HollowSteelSection)
					width1 = BF + 2 * component.WinConnect.ShearEndPlate.Bolt.EdgeDistTransvDir + 2 * boltDistToHSSside;
				else if (column.WebOrientation == EWebOrientation.InPlane)
					width1 = column.GageOnFlange + 2 * component.WinConnect.ShearEndPlate.Bolt.EdgeDistTransvDir;
				else
				{
					width1 = Math.Min(column.Shape.g2 + 2 * component.WinConnect.ShearEndPlate.Bolt.EdgeDistTransvDir, column.Shape.t);
					if (!component.WinConnect.ShearEndPlate.Bolt.EdgeDistTransvDir_User)
						component.WinConnect.ShearEndPlate.Bolt.EdgeDistTransvDir = (width1 - column.Shape.g2) / 2;
				}

				component.WinConnect.ShearEndPlate.Width = width1;

				if (component.WinConnect.ShearEndPlate.Bolt.NumberOfRows != 0.0)
					N = -component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts;
				else
					N = 0;
				NforTV = (int) Math.Max(2, BoltsForTension.CalcBoltsForTension(memberType, component.WinConnect.ShearEndPlate.Bolt, H / 2, (V / 2), N, true));
                if (!component.WinConnect.ShearEndPlate.Bolt.NumberOfRows_User)
				{
					component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts = NforTV;
					component.WinConnect.ShearEndPlate.Bolt.NumberOfRows = component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts;
				}

				t = ConstNum.THREE_EIGHTS_INCH;
				N = component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts;
				Ball = BoltsForTension.CalcBoltsForTension(memberType, component.WinConnect.ShearEndPlate.Bolt, H / 2, (V / 2), N, true);
				component.WinConnect.ShearEndPlate.Length = (N - 1) * component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir + 2 * component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir;
				L = component.WinConnect.ShearEndPlate.Length;
				if (column.ShapeType == EShapeType.HollowSteelSection)
				{
					a = (component.WinConnect.ShearEndPlate.Width - BF) / 2 - boltDistToHSSside;
					b = (BF + 2 * boltDistToHSSside - thick) / 2;
				}
				else
				{
					a = (Math.Min(component.WinConnect.ShearEndPlate.Width, column.Shape.bf) - column.GageOnFlange) / 2;
					b = (column.GageOnFlange - thick) / 2;
				}
				do
				{
					FiRnPerBolt = SmallMethodsDesign.HangerAllowable(memberType, t, ref a, b, Ball, false);
					FiRn = 2 * FiRnPerBolt * N;
					if (FiRn < H)
					{
						if (t < ConstNum.ONE_INCH)
							t = t + ConstNum.SIXTEENTH_INCH;
						else if (N < maxbolts && component.WinConnect.ShearEndPlate.Bolt.NumberOfRows == 0.0)
						{
							N = N + 1;
							L = L + component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir;
							Ball = BoltsForTension.CalcBoltsForTension(memberType, component.WinConnect.ShearEndPlate.Bolt, H / 2, (V / 2), N, false);
						}
						else
							break;
					}
				} while (FiRn < H);
				t_RequiredForTension = t;
                if (!component.WinConnect.ShearEndPlate.Bolt.NumberOfRows_User)
				{
					component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts = N;
					component.WinConnect.ShearEndPlate.Bolt.NumberOfRows = N;
				}
				component.WinConnect.ShearEndPlate.Length = (component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts - 1) * component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir + 2 * component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir;

				if (component.WinConnect.ShearEndPlate.Position == EPosition.NoConnection)
					return;

				switch (memberType)
				{
					case EMemberType.RightBeam:
					case EMemberType.LeftBeam:
						EndPlateMin = component.Shape.d / 2;
						EndPlateMax = component.Shape.t;
						Reduction = 0;
						component.WinConnect.ShearEndPlate.Length = (component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts - 1) * component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir + 2 * component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir;
						break;
					default:
						component.WinConnect.ShearEndPlate.Length = (component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts - 1) * component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir + component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir + component.WinConnect.ShearEndPlate.TOBtoFirstBolt;
						if (!component.BraceConnect.Gusset.DontConnectBeam)
						{
							EndPlateMin = component.BraceConnect.Gusset.ColumnSide + 2 - component.WinConnect.ShearEndPlate.TOBtoFirstBolt + component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir;
							EndPlateMax = EndPlateMin;
						}
						else
						{
							EndPlateMin = 6;
							EndPlateMax = 100;
						}
						N = component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts;
						Sum = 0;
						SumY = 0;
						for (int i = 1; i <= N / 2; i += 1)
						{
							Sum = (Sum + Math.Pow((N - 1) / 2 - (i - 1), 2));
							SumY = SumY + ((N - 1) / 2 - (i - 1));
						}
						tmax = (N - 1) * Math.Abs(component.BraceConnect.Gusset.Mc) / (4 * component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir * Sum);
						Reduction = tmax * SumY * 2 / ((N - 1) * component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir);
						break;
				}

				maxbolts = (int) Math.Floor((EndPlateMax - 2 * component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir) / component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir) + 1;

				re = component.WinConnect.ShearEndPlate.Bolt.MinEdgeSheared;
				if (column.WebOrientation == EWebOrientation.InPlane)
				{
					GageCol1 = 2 * (column.Shape.k1 + component.WinConnect.ShearEndPlate.Bolt.BoltSize);
					GageCol2 = column.Shape.tw + 2 * (component.WinConnect.ShearEndPlate.Bolt.BoltSize + ConstNum.HALF_INCH);
					if (GageCol1 < GageCol2)
						columngagemin = GageCol2;
					else
						columngagemin = GageCol1;
					columngagemax = column.Shape.bf - 2 * re;
					if (columngagemin <= column.GageOnFlange && columngagemax >= column.GageOnFlange)
					{
						columngagemin = column.GageOnFlange;
						columngagemax = column.GageOnFlange;
					}
					else
					{
						Reporting.AddLine("Check Column Gage: Should be between " + columngagemin + ConstUnit.Length + " and " + columngagemax + ConstUnit.Length);
						columngagemin = column.GageOnFlange;
						columngagemax = column.GageOnFlange;
					}
				}
				else
				{

					columngagemin = 0;
					columngagemax = column.Shape.t - 2 * re;
					if (column.Shape.g2 > 0)
					{
						if (columngagemin <= column.Shape.g2 && columngagemax >= column.Shape.g2)
						{
							columngagemin = column.Shape.g2;
							columngagemax = column.Shape.g2;
						}
						else
						{
							Reporting.AddLine("Check Column Gage: Should be less then " + columngagemax + ConstUnit.Length);
							columngagemin = column.Shape.g2;
							columngagemax = column.Shape.g2;
						}
					}
				}
				if (componentIsBrace)
					component.WinConnect.ShearEndPlate.Length = (component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts - 1) * component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir + 2 * component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir;

				Fbe = CommonCalculations.EdgeBearing(component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir, component.WinConnect.ShearEndPlate.Bolt.HoleLength, component.WinConnect.ShearEndPlate.Bolt.BoltSize, component.WinConnect.ShearEndPlate.Material.Fu, component.WinConnect.ShearEndPlate.Bolt.HoleType, false);
				Fbs = CommonCalculations.SpacingBearing(component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir, component.WinConnect.ShearEndPlate.Bolt.HoleLength, component.WinConnect.ShearEndPlate.Bolt.BoltSize, component.WinConnect.ShearEndPlate.Bolt.HoleType, component.WinConnect.ShearEndPlate.Material.Fu, false);

				// shear rupture
				Lnv = 2 * (component.WinConnect.ShearEndPlate.Length - component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts * (component.WinConnect.ShearEndPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH));
				t_reqForNetShearCap = V / (ConstNum.FIOMEGA0_75N * Lnv * 0.6 * component.WinConnect.ShearEndPlate.Material.Fu);
				// shear yielding
				Lgv = 2 * component.WinConnect.ShearEndPlate.Length;
				t_reqForGrossShearCap = V / (ConstNum.FIOMEGA1_0N * Lgv * 0.6 * component.WinConnect.ShearEndPlate.Material.Fy);
				// block shear rupture
				Lgv = 2 * (component.WinConnect.ShearEndPlate.Length - component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir);
				Lnv = Lgv - 2 * (component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts - 0.5) * (component.WinConnect.ShearEndPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
				Lgt = 2 * component.WinConnect.ShearEndPlate.Bolt.EdgeDistTransvDir;
				Lnt = Lgt - 2 * 0.5 * (component.WinConnect.ShearEndPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);

				t_reqForBlockShearStrength = V / (ConstNum.FIOMEGA0_75N * (0.6 * Math.Min(component.WinConnect.ShearEndPlate.Material.Fy * Lgv, component.WinConnect.ShearEndPlate.Material.Fu * Lnv) + 1 * component.WinConnect.ShearEndPlate.Material.Fu * Lnt));
				t_reqForBearingCap = V / (2 * (Fbe + Fbs * (component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts - 1)));
				t = Math.Max(Math.Max(t_RequiredForTension, t_reqForBearingCap), Math.Max(t_reqForBlockShearStrength, Math.Max(t_reqForGrossShearCap, t_reqForNetShearCap)));

				if (!component.WinConnect.ShearEndPlate.Thickness_User)
					component.WinConnect.ShearEndPlate.Thickness = t;

				// Connection Plate Begin
				if (column.ShapeType == EShapeType.HollowSteelSection)
				{
					a = (component.WinConnect.ShearEndPlate.Width - BF) / 2 - boltDistToHSSside;
					b = boltDistToHSSside;

					do
					{
						FiRnPerBolt = SmallMethodsDesign.HangerAllowable(memberType, t, ref a, b, Ball, false);
						FiRn = 2 * FiRnPerBolt * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts;
						if (FiRn < H)
							t = t + ConstNum.SIXTEENTH_INCH;
					} while (FiRn < H && t < 1);
					if (!component.WinConnect.ShearEndPlate.ConnectionPlateThickness_User)
						component.WinConnect.ShearEndPlate.ConnectionPlateThickness = Math.Max(CommonCalculations.PlateThickness(t), ConstNum.THREE_EIGHTS_INCH);
				}

				// Connection Plate End 
				if (column.ShapeType == EShapeType.HollowSteelSection)
				{
					a = (component.WinConnect.ShearEndPlate.Width - BF) / 2 - boltDistToHSSside;
					b = (BF + 2 * boltDistToHSSside - thick) / 2;
				}
				else
				{
					a = (Math.Min(component.WinConnect.ShearEndPlate.Width, column.Shape.bf) - column.GageOnFlange) / 2;
					b = (column.GageOnFlange - thick) / 2;
				}
				Ball = BoltsForTension.CalcBoltsForTension(memberType, component.WinConnect.ShearEndPlate.Bolt, H / 2, (V / 2), component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts, true);
				tTemp = t;
				do
				{
					FiRnPerBolt = SmallMethodsDesign.HangerAllowable(memberType, t, ref a, b, Ball, false);
					FiRn = 2 * FiRnPerBolt * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts;

					if (FiRn < H && FiRn > 0 || t > 1 && component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts < maxbolts && component.WinConnect.ShearEndPlate.Bolt.NumberOfRows == 0.0)
					{
						if (t > 1 && component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts < maxbolts)
						{
							component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts = component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts + 1;
							t = tTemp;
							component.WinConnect.ShearEndPlate.Length = component.WinConnect.ShearEndPlate.Length + component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir;
							Ball = BoltsForTension.CalcBoltsForTension(memberType, component.WinConnect.ShearEndPlate.Bolt, H / 2, (V / 2), component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts, true);
						}
						else
						{
							if (Ball <= FiRnPerBolt)
								break;
							else
								t += ConstNum.SIXTEENTH_INCH;
						}
					}
					else
						break;
				} while (true);
                if (!component.WinConnect.ShearEndPlate.Thickness_User)
					component.WinConnect.ShearEndPlate.Thickness = Math.Max(CommonCalculations.PlateThickness(t), ConstNum.THREE_EIGHTS_INCH);

				// If ContinuousClipAngles Then
				switch (memberType)
				{
					case EMemberType.RightBeam:
					case EMemberType.UpperRight:
					case EMemberType.LowerRight:
						if (CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive &&
						    CommonDataStatic.DetailDataDict[EMemberType.UpperRight].IsActive &&
						    CommonDataStatic.DetailDataDict[EMemberType.LowerRight].IsActive)
						{
                            if (!component.WinConnect.ShearEndPlate.Thickness_User)
							{
								component.WinConnect.ShearEndPlate.Thickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.RightBeam].WinConnect.ShearEndPlate.Thickness,
										Math.Max(
											CommonDataStatic.DetailDataDict[EMemberType.UpperRight].WinConnect.ShearEndPlate.Thickness,
											CommonDataStatic.DetailDataDict[EMemberType.LowerRight].WinConnect.ShearEndPlate.Thickness));
							}
                            if (!component.WinConnect.ShearEndPlate.ConnectionPlateThickness_User)
							{
								component.WinConnect.ShearEndPlate.ConnectionPlateThickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.RightBeam].WinConnect.ShearEndPlate.ConnectionPlateThickness,
										Math.Max(
											CommonDataStatic.DetailDataDict[EMemberType.UpperRight].WinConnect.ShearEndPlate.ConnectionPlateThickness,
											CommonDataStatic.DetailDataDict[EMemberType.LowerRight].WinConnect.ShearEndPlate.ConnectionPlateThickness));
							}
							// EndPlate(m).Width = Max(EndPlate(1).Width, Max(EndPlate(3).Width, EndPlate(4).Width))
						}
						else if (CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive &&
						         CommonDataStatic.DetailDataDict[EMemberType.UpperRight].IsActive)
						{
                            if (!component.WinConnect.ShearEndPlate.Thickness_User)
							{
								component.WinConnect.ShearEndPlate.Thickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.RightBeam].WinConnect.ShearEndPlate.Thickness,
										CommonDataStatic.DetailDataDict[EMemberType.UpperRight].WinConnect.ShearEndPlate.Thickness);
							}
                            if (!component.WinConnect.ShearEndPlate.ConnectionPlateThickness_User)
							{
								component.WinConnect.ShearEndPlate.ConnectionPlateThickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.RightBeam].WinConnect.ShearEndPlate
											.ConnectionPlateThickness,
										CommonDataStatic.DetailDataDict[EMemberType.UpperRight].WinConnect.ShearEndPlate
											.ConnectionPlateThickness);
							}
							// EndPlate(m).Width = Max(EndPlate(1).Width, EndPlate(3).Width)
						}
						else if (CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive &&
						         CommonDataStatic.DetailDataDict[EMemberType.LowerRight].IsActive)
						{
                            if (!component.WinConnect.ShearEndPlate.Thickness_User)
							{
								component.WinConnect.ShearEndPlate.Thickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.RightBeam].WinConnect.ShearEndPlate.Thickness,
										CommonDataStatic.DetailDataDict[EMemberType.LowerRight].WinConnect.ShearEndPlate.Thickness);
							}
                            if (!component.WinConnect.ShearEndPlate.ConnectionPlateThickness_User)
							{
								component.WinConnect.ShearEndPlate.ConnectionPlateThickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.RightBeam].WinConnect.ShearEndPlate.ConnectionPlateThickness,
										CommonDataStatic.DetailDataDict[EMemberType.LowerRight].WinConnect.ShearEndPlate.ConnectionPlateThickness);
							}
							// EndPlate(m).Width = Max(EndPlate(1).Width, EndPlate(4).Width)
						}
						else if (CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].IsActive &&
						         CommonDataStatic.DetailDataDict[EMemberType.UpperRight].IsActive)
						{
                            if (!component.WinConnect.ShearEndPlate.Thickness_User)
							{
								component.WinConnect.ShearEndPlate.Thickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.LowerRight].WinConnect.ShearEndPlate.Thickness,
										CommonDataStatic.DetailDataDict[EMemberType.UpperRight].WinConnect.ShearEndPlate.Thickness);
							}
                            if (!component.WinConnect.ShearEndPlate.ConnectionPlateThickness_User)
							{
								component.WinConnect.ShearEndPlate.ConnectionPlateThickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.LowerRight].WinConnect.ShearEndPlate.ConnectionPlateThickness,
										CommonDataStatic.DetailDataDict[EMemberType.UpperRight].WinConnect.ShearEndPlate.ConnectionPlateThickness);
							}
							// EndPlate(m).Width = Max(EndPlate(4).Width, EndPlate(3).Width)
						}
						break;
					case EMemberType.LeftBeam:
					case EMemberType.UpperLeft:
					case EMemberType.LowerLeft:
						if (CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive &&
						    CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].IsActive &&
						    CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].IsActive)
						{
							if (!component.WinConnect.ShearEndPlate.Thickness_User)
							{
								component.WinConnect.ShearEndPlate.Thickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].WinConnect.ShearEndPlate.Thickness,
										Math.Max(
											CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].WinConnect.ShearEndPlate.Thickness,
											CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].WinConnect.ShearEndPlate.Thickness));
							}
                            if (!component.WinConnect.ShearEndPlate.ConnectionPlateThickness_User)
							{
								component.WinConnect.ShearEndPlate.ConnectionPlateThickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].WinConnect.ShearEndPlate.ConnectionPlateThickness,
										Math.Max(
											CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].WinConnect.ShearEndPlate.ConnectionPlateThickness,
											CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].WinConnect.ShearEndPlate.ConnectionPlateThickness));
							}
						}
						else if (CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive &&
						         CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].IsActive)
						{
                            if (!component.WinConnect.ShearEndPlate.Thickness_User)
							{
								component.WinConnect.ShearEndPlate.Thickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].WinConnect.ShearEndPlate.Thickness,
										CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].WinConnect.ShearEndPlate.Thickness);
							}
                            if (!component.WinConnect.ShearEndPlate.ConnectionPlateThickness_User)
							{
								component.WinConnect.ShearEndPlate.ConnectionPlateThickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].WinConnect.ShearEndPlate.ConnectionPlateThickness,
										CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].WinConnect.ShearEndPlate.ConnectionPlateThickness);
							}
							// EndPlate(m).Width = Max(EndPlate(2).Width, EndPlate(5).Width)
						}
						else if (CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive &&
						         CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].IsActive)
						{
                            if (!component.WinConnect.ShearEndPlate.Thickness_User)
							{
								component.WinConnect.ShearEndPlate.Thickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].WinConnect.ShearEndPlate.Thickness,
										CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].WinConnect.ShearEndPlate.Thickness);
							}
                            if (!component.WinConnect.ShearEndPlate.ConnectionPlateThickness_User)
							{
								component.WinConnect.ShearEndPlate.ConnectionPlateThickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].WinConnect.ShearEndPlate.ConnectionPlateThickness,
										CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].WinConnect.ShearEndPlate.ConnectionPlateThickness);
							}
						}
						else if (CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].IsActive &&
						         CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].IsActive)
						{
                            if (!component.WinConnect.ShearEndPlate.Thickness_User)
							{
								component.WinConnect.ShearEndPlate.Thickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].WinConnect.ShearEndPlate.Thickness,
										CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].WinConnect.ShearEndPlate.Thickness);
							}
                            if (!component.WinConnect.ShearEndPlate.ConnectionPlateThickness_User)
							{
								component.WinConnect.ShearEndPlate.ConnectionPlateThickness =
									Math.Max(
										CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].WinConnect.ShearEndPlate.ConnectionPlateThickness,
										CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].WinConnect.ShearEndPlate.ConnectionPlateThickness);
							}
						}
						break;
				}

				// Required Weld Size
				switch (memberType)
				{
					case EMemberType.RightBeam:
					case EMemberType.LeftBeam:
						if (!component.EndSetback_User)
							component.EndSetback = component.WinConnect.ShearEndPlate.Thickness + component.WinConnect.ShearEndPlate.ConnectionPlateThickness;
						Th = component.Shape.tw;
						L = Math.Min(component.WinConnect.ShearEndPlate.Length, component.Shape.t);
						break;
					default:
						if (!component.BraceConnect.Gusset.ColumnSideSetback_User)
							component.BraceConnect.Gusset.ColumnSideSetback = component.WinConnect.ShearEndPlate.Thickness + component.WinConnect.ShearEndPlate.ConnectionPlateThickness;
						Th = component.BraceConnect.Gusset.Thickness;
						L = component.WinConnect.ShearEndPlate.Length;
						break;
				}

				usefulweldsize = ConstNum.FIOMEGA0_75N * 0.6 *
				                 Math.Min(component.Material.Fu * Th, 2 * component.WinConnect.ShearEndPlate.Material.Fu * component.WinConnect.ShearEndPlate.Thickness) /
								 (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				do
				{
					CommonDataStatic.LShapedWeld = true;
					if (V != 0)
						Theta = Math.Atan(Math.Abs(H / V));

					k = 0;
					if (Mom == 0)
						a = 0;
					else if (H == 0)
						a = 3;
					else
						a = Mom / H / L;

					Cnst = EccentricWeld.GetEccentricWeld(k, a, Theta, false);
					//EccentricWeld.GetEccentricWeld(k, a, ref Cnst, Theta, false);
					c1 = CommonCalculations.WeldTypeRatio();
					minweld = CommonCalculations.MinimumWeld(Th, component.WinConnect.ShearEndPlate.Thickness);
					Ru = Math.Pow(Math.Pow(H, 2) + Math.Pow(V, 2), 0.5);
					wReq = Ru / (2 * 16 * c1 * Cnst * L);
					if (wReq > usefulweldsize)
					{
						if (memberType == EMemberType.LeftBeam || componentIsBrace)
						{
							if (L + component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir <= component.Shape.t)
							{
								L += component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir;
								loopAgain = true;
							}
							else
								loopAgain = false;
						}
						else
						{
							L += component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir;
							loopAgain = true;
						}
					}
					else
						loopAgain = false;
				} while (loopAgain);

				component.WinConnect.ShearEndPlate.Length = L;
                if (!component.WinConnect.ShearEndPlate.Bolt.NumberOfRows_User)
                {
	                component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts = (int)Math.Floor((component.WinConnect.ShearEndPlate.Length - 2 * component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir) / component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir) + 1;
					component.WinConnect.ShearEndPlate.Bolt.NumberOfRows = component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts;
				}
				component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir = (component.WinConnect.ShearEndPlate.Length - (component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts - 1) * component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir) / 2;
                if (!component.WinConnect.ShearEndPlate.WeldSize_User)
					component.WinConnect.ShearEndPlate.WeldSize = NumberFun.Round(Math.Max(wReq, minweld), 16);
				if (column.ShapeType == EShapeType.HollowSteelSection)
				{
					// Connection Plate to HSS flare weld
					if (V != 0)
						Theta = Math.Atan(Math.Abs(H / V));

					k = 0;
					if (Mom == 0)
						a = 0;
					else if (H == 0)
						a = 3;
					else
						a = Mom / H / L;
					Cnst = EccentricWeld.GetEccentricWeld(k, a, Theta, true);
					//EccentricWeld.GetEccentricWeld(k, a, ref Cnst, Theta, true);
					CommonDataStatic.LShapedWeld = false;
					c1 = CommonCalculations.WeldTypeRatio();
					FiRn = 2 * 16 * c1 * Cnst * L * (5 / 8.0 * column.Shape.tf);
				}

				if (column.ShapeType == EShapeType.HollowSteelSection)
					Reporting.AddHeader("End Plate - to - Connection Plate Bolts:");
				else
					Reporting.AddHeader("End Plate to Column Bolts:");

				Reporting.AddLine("(" + (2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts) + ") " + component.WinConnect.ShearEndPlate.Bolt.BoltName + " Bolts");
				Reporting.AddGoToHeader(ConstString.DES_OR_ALLOWABLE + " Shear Strength of Bolts:");

				switch (memberType)
				{
					case EMemberType.RightBeam:
					case EMemberType.LeftBeam:
						if (component.WinConnect.ShearEndPlate.Bolt.BoltType == EBoltType.SC && !(CommonDataStatic.Preferences.Seismic == ESeismic.Seismic && CommonDataStatic.SeismicSettings.Response == EResponse.RGreaterThan3 && component.WinConnect.ShearEndPlate.Bolt.HoleType == EBoltHoleType.STD))
						{
							cap = 2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts * component.WinConnect.ShearEndPlate.Bolt.BoltStrength * (1 - H / (1.13 * component.WinConnect.ShearEndPlate.Bolt.Pretension * 2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts));
							Reporting.AddLine("= n * (" + ConstString.PHI + " Rn) * (1 - Tu / (1.13 * Tm * n))");
							Reporting.AddLine("= " + (2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts) + " * " + component.WinConnect.ShearEndPlate.Bolt.BoltStrength + " * (1 - " + H + " / (1.13 * " + component.WinConnect.ShearEndPlate.Bolt.Pretension + " * " + (2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts) + "))");
							if (cap >= V)
								Reporting.AddLine("= " + cap + " >= " + V + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= " + cap + " << " + V + ConstUnit.Force + " (NG)");
						}
						else
						{
							cap = 2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts * component.WinConnect.ShearEndPlate.Bolt.BoltStrength;
							if (cap >= V)
								Reporting.AddLine("= n * (" + ConstString.PHI + " Rn) = " + (2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts) + " * " + component.WinConnect.ShearEndPlate.Bolt.BoltStrength + " = " + cap + " >= " + V + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= n * (" + ConstString.PHI + " Rn) = " + (2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts) + " * " + component.WinConnect.ShearEndPlate.Bolt.BoltStrength + " = " + cap + " << " + V + ConstUnit.Force + " (NG)");
						}
						Ball_Web = BoltsForTension.CalcBoltsForTension(memberType, component.WinConnect.ShearEndPlate.Bolt, H / 2, (V / 2), component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts, true);
						break;
					default:
						if (component.WinConnect.ShearEndPlate.Bolt.BoltType == EBoltType.SC && !(CommonDataStatic.Preferences.Seismic == ESeismic.Seismic && CommonDataStatic.SeismicSettings.Response == EResponse.RGreaterThan3 && component.WinConnect.ShearEndPlate.Bolt.HoleType == EBoltHoleType.STD))
						{
							cap = 2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts * component.WinConnect.ShearEndPlate.Bolt.BoltStrength * (1 - H / (1.13 * component.WinConnect.ShearEndPlate.Bolt.Pretension * 2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts));
							Reporting.AddLine("= n * (" + ConstString.PHI + " Rn) * (1 - Tu / (1.13 * Tm * n))");
							Reporting.AddLine("= " + (2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts) + " * " + component.WinConnect.ShearEndPlate.Bolt.BoltStrength + " *  (1 - " + H + " / (1.13 * " + component.WinConnect.ShearEndPlate.Bolt.Pretension + " * " + (2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts) + "))");
							if (cap >= V)
								Reporting.AddLine("= " + cap + " >= " + V + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= " + cap + " << " + V + ConstUnit.Force + " (NG)");
						}
						else
						{
							cap = 2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts * component.WinConnect.ShearEndPlate.Bolt.BoltStrength;
							if (cap >= V)
								Reporting.AddLine("= 2 * n * (" + ConstString.PHI + " Rn) = 2 * " + component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts + " * " + component.WinConnect.ShearEndPlate.Bolt.BoltStrength + " = " + cap + " >= " + V + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= 2 * n * (" + ConstString.PHI + " Rn) = 2 * " + component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts + " * " + component.WinConnect.ShearEndPlate.Bolt.BoltStrength + " = " + cap + " << " + V + ConstUnit.Force + " (NG)");
						}
						break;
				}

				Reporting.AddHeader("End Plate");
				Reporting.AddLine("Design " + CommonDataStatic.CommonLists.CompleteMemberList[MiscMethods.ComponentMinusOne(memberType)] + " End Plate");
				
				Reporting.AddHeader("Bolt Bearing On Plate:");
				Fbe = CommonCalculations.EdgeBearing(component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir, component.WinConnect.ShearEndPlate.Bolt.HoleLength, component.WinConnect.ShearEndPlate.Bolt.BoltSize, component.WinConnect.ShearEndPlate.Material.Fu, component.WinConnect.ShearEndPlate.Bolt.HoleType, false);
				Fbs = CommonCalculations.SpacingBearing(component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir, component.WinConnect.ShearEndPlate.Bolt.HoleLength, component.WinConnect.ShearEndPlate.Bolt.BoltSize, component.WinConnect.ShearEndPlate.Bolt.HoleType, component.WinConnect.ShearEndPlate.Material.Fu, false);
				BearingCap = 2 * (Fbe + Fbs * (component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts - 1)) * component.WinConnect.ShearEndPlate.Thickness;

				Reporting.AddLine("Bearing Capacity = 2 * (Fbe + Fbs * (n - 1)) * t");
				Reporting.AddLine("= 2 * (" + Fbe + " + " + Fbs + " * (" + component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts + " - 1)) * " + component.WinConnect.ShearEndPlate.Thickness);
				if (BearingCap >= Math.Abs(V))
					Reporting.AddLine("= " + BearingCap + " >= " + Math.Abs(V) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + BearingCap + " << " + Math.Abs(V) + ConstUnit.Force + " (NG)");

				Reporting.AddHeader("End Plate Length = Lp = " + component.WinConnect.ShearEndPlate.Length + ConstUnit.Length);
				Ag = 2 * component.WinConnect.ShearEndPlate.Length * component.WinConnect.ShearEndPlate.Thickness;
				FiRn = ConstNum.FIOMEGA1_0N * Ag * 0.6 * component.WinConnect.ShearEndPlate.Material.Fy;
				Reporting.AddLine("Shear Yielding of Plate:");

				Reporting.AddLine("Ag = 2 * Lp * t = 2 * " + component.WinConnect.ShearEndPlate.Length + " * " + component.WinConnect.ShearEndPlate.Thickness + " = " + Ag + ConstUnit.Area);
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA1_0 + " * Ag * 0.6 * Fy");
				Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * " + Ag + " * 0.6 * " + component.WinConnect.ShearEndPlate.Material.Fy);
				if (FiRn >= V)
					Reporting.AddLine("= " + FiRn + " >= " + V + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + FiRn + " << " + V + ConstUnit.Force + " (NG)");

				Reporting.AddHeader("Shear Rupture:");
				Anv = 2 * (component.WinConnect.ShearEndPlate.Length - component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts * (component.WinConnect.ShearEndPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * component.WinConnect.ShearEndPlate.Thickness;
				FiRn = ConstNum.FIOMEGA0_75N * Anv * 0.6 * component.WinConnect.ShearEndPlate.Material.Fu;

				Reporting.AddLine("Anv = 2 * (Lp - n*(dh+" + ConstNum.SIXTEENTH_INCH + ")) * t");
				Reporting.AddLine("= 2 * (" + component.WinConnect.ShearEndPlate.Length + " - " + component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts + " *(" + component.WinConnect.ShearEndPlate.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + component.WinConnect.ShearEndPlate.Thickness);
				Reporting.AddLine("= " + Anv + ConstUnit.Area);

				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Anv * 0.6 * Fu");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + Anv + " * 0.6 * " + component.WinConnect.ShearEndPlate.Material.Fu);
				if (FiRn >= V)
					Reporting.AddLine("= " + FiRn + " >= " + V + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + FiRn + " << " + V + ConstUnit.Force + " (NG)");

				Reporting.AddHeader("Block Shear Rupture:");
				Agv = 2 * (component.WinConnect.ShearEndPlate.Length - component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir) * component.WinConnect.ShearEndPlate.Thickness;
				Reporting.AddLine("Agv = 2 * (Lp - el) * t");
				Reporting.AddLine("= 2 * (" + component.WinConnect.ShearEndPlate.Length + " - " + component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir + ") * " + component.WinConnect.ShearEndPlate.Thickness);
				Reporting.AddLine("= " + Agv + ConstUnit.Area);

				Anv = Agv - 2 * (component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts - 0.5) * (component.WinConnect.ShearEndPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.WinConnect.ShearEndPlate.Thickness;
				Reporting.AddLine("Anv = Agv - 2 * (N - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("= " + Agv + " - 2 * (" + component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts + " - 0.5) * (" + component.WinConnect.ShearEndPlate.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.WinConnect.ShearEndPlate.Thickness);
				Reporting.AddLine("= " + Anv + ConstUnit.Area);

				Agt = 2 * component.WinConnect.ShearEndPlate.Bolt.EdgeDistTransvDir * component.WinConnect.ShearEndPlate.Thickness;
				Reporting.AddLine("Agt = 2 * et * t =  2 * " + component.WinConnect.ShearEndPlate.Bolt.EdgeDistTransvDir + " * " + component.WinConnect.ShearEndPlate.Thickness + " = " + Agt + ConstUnit.Area);

				Ant = Agt - 2 * 0.5 * (component.WinConnect.ShearEndPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.WinConnect.ShearEndPlate.Thickness;
				Reporting.AddLine("Ant = Agt - 2 * 0.5 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("= " + Agt + " - 2 * 0.5 * (" + component.WinConnect.ShearEndPlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.WinConnect.ShearEndPlate.Thickness);
				Reporting.AddLine("= " + Ant + ConstUnit.Area);

				FiRn = SmallMethodsDesign.BlockShearPrint(Agv, Anv, Agt, Ant, component.WinConnect.ShearEndPlate.Material.Fy, component.WinConnect.ShearEndPlate.Material.Fu, true);

				if (FiRn >= V)
					Reporting.AddLine("= " + FiRn + " >= " + V + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + FiRn + " << " + V + ConstUnit.Force + " (NG)");

				if (componentIsBeam)
					thick = component.Shape.tw;
				else
					thick = component.BraceConnect.Gusset.Thickness;

				if (column.ShapeType == EShapeType.HollowSteelSection)
				{
					b = (BF + 2 * boltDistToHSSside - thick) / 2;
					edg = (component.WinConnect.ShearEndPlate.Width - BF - 2 * boltDistToHSSside) / 2;
				}
				else
				{
					b = (column.GageOnFlange - thick) / 2;
					if (column.WebOrientation == EWebOrientation.InPlane)
						edg = Math.Min((component.WinConnect.ShearEndPlate.Width - column.GageOnFlange) / 2, (column.Shape.bf - column.GageOnFlange) / 2);
					else
						edg = (component.WinConnect.ShearEndPlate.Width - column.GageOnFlange) / 2;
				}
				switch (memberType)
				{
					case EMemberType.RightBeam:
					case EMemberType.LeftBeam:
						//EndPlateBoltsForFlange = false;
						Ta_Web = SmallMethodsDesign.HangerAllowable(memberType, component.WinConnect.ShearEndPlate.Thickness, ref edg, b, Ball_Web, true);

						Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Tension Strength:");
						FiRn = 2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts * Ta_Web;
						Reporting.AddLine(ConstString.PHI + " Rn = n * Ta");
						Reporting.AddLine("= " + (2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts) + " * " + Ta_Web);
						if (FiRn >= H)
							Reporting.AddLine("= " + FiRn + " >= " + H + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + FiRn + " << " + H + ConstUnit.Force + " (NG)");
						break;
					default:
						Ta = SmallMethodsDesign.HangerAllowable(memberType, component.WinConnect.ShearEndPlate.Thickness, ref edg, b, Ball, true);
						Reporting.AddHeader("Reduction in Tension Strength due to Moment = Tm");
						Reporting.AddLine("= " + Reduction + ConstUnit.Force); // & " (see 'Help' for computation)"

						Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Tension Strength:");
						FiRn = 2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts * Ta - Reduction;
						Reporting.AddLine(ConstString.PHI + " Rn = 2 * n * Ta - Tm");
						Reporting.AddLine("= 2 * " + component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts + " * " + Ta + " - " + Reduction);
						if (FiRn >= H)
							Reporting.AddLine("= " + FiRn + " >= " + H + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + FiRn + " << " + H + ConstUnit.Force + " (NG)");
						break;
				}

				switch (memberType)
				{
					case EMemberType.RightBeam:
					case EMemberType.LeftBeam:
						Reporting.AddHeader("End Plate to Beam Weld");
						Th = component.Shape.tw;
						L = Math.Min(component.WinConnect.ShearEndPlate.Length, component.Shape.t);
						break;
					default:
						Reporting.AddHeader("End Plate to Gusset Plate Weld");
						Th = component.BraceConnect.Gusset.Thickness;
						L = component.WinConnect.ShearEndPlate.Length;
						break;
				}
				minweld = CommonCalculations.MinimumWeld(Th, component.WinConnect.ShearEndPlate.Thickness);
				if (component.WinConnect.ShearEndPlate.WeldSize >= minweld)
					Reporting.AddLine("Weld  Size = " + CommonCalculations.WeldSize(component.WinConnect.ShearEndPlate.WeldSize) + " >=  Min. Weld = " + CommonCalculations.WeldSize(((int) minweld)) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld  Size = " + CommonCalculations.WeldSize(component.WinConnect.ShearEndPlate.WeldSize) + " << " + CommonCalculations.WeldSize(minweld) + ConstUnit.Length + " (NG)");

				usefulweldsize = ConstNum.FIOMEGA0_75N * 0.6 * Math.Min(component.Material.Fu * Th, 2 * component.WinConnect.ShearEndPlate.Material.Fu * component.WinConnect.ShearEndPlate.Thickness) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddHeader("Useful Weld Size = " + ConstString.FIOMEGA0_75 + " * 0.6 * Min(Fu_g * t_g, 2 * Fu_ep * t_ep) / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * Fexx)");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.6 * Min(" + component.Material.Fu + " * " + Th + ", 2 * " + component.WinConnect.ShearEndPlate.Material.Fu + " * " + component.WinConnect.ShearEndPlate.Thickness + ") / (" + ConstString.FIOMEGA0_75 + " * 0.6 * 1.414 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldsize >= component.WinConnect.ShearEndPlate.WeldSize)
				{
					Reporting.AddLine("= " + usefulweldsize + " >= " + component.WinConnect.ShearEndPlate.WeldSize + ConstUnit.Length + " (OK)");
					weldforcalc = component.WinConnect.ShearEndPlate.WeldSize;
				}
				else
				{
					Reporting.AddLine("= " + usefulweldsize + " << " + component.WinConnect.ShearEndPlate.WeldSize + ConstUnit.Length);
					Reporting.AddLine("Use " + usefulweldsize + ConstUnit.Length + " for strength calculation.");
					weldforcalc = usefulweldsize;
				}
				Ru = Math.Pow(Math.Pow(H, 2) + Math.Pow(V, 2), 0.5);
				Reporting.AddHeader("Resultant Force:");
				Reporting.AddLine("Ru = (H² + V²)^0.5 = (" + H + " ^ 2 + " + V + " ^ 2) ^ 0.5 = " + Ru + ConstUnit.Force);
				CommonDataStatic.LShapedWeld = true;

				if (V != 0)
					Theta = Math.Atan(Math.Abs(H / V));

				k = 0;
				if (Mom == 0)
					a = 0;
				else if (H == 0)
					a = 3;
				else
					a = Mom / H / L;
				Cnst = EccentricWeld.GetEccentricWeld(k, a, Theta, true);
				//EccentricWeld.GetEccentricWeld(k, a, ref Cnst, Theta, true);
				CommonDataStatic.LShapedWeld = false;
				c1 = CommonCalculations.WeldTypeRatio();
				FiRn = 2 * 16 * c1 * Cnst * L * weldforcalc;

				Reporting.AddHeader("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");

				Reporting.AddLine(ConstString.PHI + " Rn = 2 * 16 * C1 * C * L * w");
				Reporting.AddLine("= 2 * 16 *  " + c1 + " * " + Cnst + " * " + L + " * " + weldforcalc);
				if (FiRn >= Ru)
					Reporting.AddLine("= " + FiRn + " >= " + Ru + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + FiRn + " << " + Ru + ConstUnit.Force + " (NG)");

				if (column.ShapeType == EShapeType.HollowSteelSection)
				{
					Reporting.AddHeader("Connection Plate");
					Reporting.AddLine("Design " + CommonDataStatic.CommonLists.CompleteMemberList[MiscMethods.ComponentMinusOne(memberType)] + " Conn. Plate");
					Reporting.AddLine("Bolt Bearing On Plate:");

					Fbe = CommonCalculations.EdgeBearing(component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir, component.WinConnect.ShearEndPlate.Bolt.HoleLength, component.WinConnect.ShearEndPlate.Bolt.BoltSize, component.WinConnect.ShearEndPlate.Material.Fu, component.WinConnect.ShearEndPlate.Bolt.HoleType, false);
					Fbs = CommonCalculations.SpacingBearing(component.WinConnect.ShearEndPlate.Bolt.SpacingLongDir, component.WinConnect.ShearEndPlate.Bolt.HoleLength, component.WinConnect.ShearEndPlate.Bolt.BoltSize, component.WinConnect.ShearEndPlate.Bolt.HoleType, component.WinConnect.ShearEndPlate.Material.Fu, false);
					BearingCap = 2 * (Fbe + Fbs * (component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts - 1)) * component.WinConnect.ShearEndPlate.ConnectionPlateThickness;

					Reporting.AddHeader("Bearing Capacity = 2 * (Fbe + Fbs * (n - 1)) * t");
					Reporting.AddLine("= 2 * (" + Fbe + " + " + Fbs + " * (" + component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts + " - 1)) * " + component.WinConnect.ShearEndPlate.ConnectionPlateThickness);
					if (BearingCap >= Math.Abs(V))
						Reporting.AddLine("= " + BearingCap + " >= " + Math.Abs(V) + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + BearingCap + " << " + Math.Abs(V) + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Connection Plate Length = Lp = " + component.WinConnect.ShearEndPlate.Length + ConstUnit.Length);
					Ag = 2 * component.WinConnect.ShearEndPlate.Length * component.WinConnect.ShearEndPlate.ConnectionPlateThickness;
					FiRn = ConstNum.FIOMEGA1_0N * Ag * 0.6 * component.WinConnect.ShearEndPlate.Material.Fy;
					Reporting.AddLine("Shear Yielding of Plate:");
					Reporting.AddLine("Ag = 2 * Lp * t = 2 * " + component.WinConnect.ShearEndPlate.Length + " * " + component.WinConnect.ShearEndPlate.ConnectionPlateThickness + " = " + Ag + ConstUnit.Area);

					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA1_0 + " * Ag * 0.6 * Fy");
					Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * " + Ag + " * 0.6 * " + component.WinConnect.ShearEndPlate.Material.Fy);
					if (FiRn >= V)
						Reporting.AddLine("= " + FiRn + " >= " + V + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + FiRn + " << " + V + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Shear Rupture:");
					Anv = 2 * (component.WinConnect.ShearEndPlate.Length - component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts * (component.WinConnect.ShearEndPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * component.WinConnect.ShearEndPlate.ConnectionPlateThickness;
					FiRn = ConstNum.FIOMEGA0_75N * Anv * 0.6 * component.WinConnect.ShearEndPlate.Material.Fu;

					Reporting.AddLine("Anv = 2 * (Lp - n * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
					Reporting.AddLine("= 2 * (" + component.WinConnect.ShearEndPlate.Length + " - " + component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts + " *(" + component.WinConnect.ShearEndPlate.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + component.WinConnect.ShearEndPlate.ConnectionPlateThickness);
					Reporting.AddLine("= " + Anv + ConstUnit.Area);

					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Anv * 0.6 * Fu");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + Anv + " * 0.6 * " + component.WinConnect.ShearEndPlate.Material.Fu);
					if (FiRn >= V)
						Reporting.AddLine("= " + FiRn + " >= " + V + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + FiRn + " << " + V + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Block Shear Rupture:");
					Agv = 2 * (component.WinConnect.ShearEndPlate.Length - component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir) * component.WinConnect.ShearEndPlate.ConnectionPlateThickness;
					Reporting.AddLine("Agv = 2 * (Lp - el) * t");
					Reporting.AddLine("= 2 * (" + component.WinConnect.ShearEndPlate.Length + " - " + component.WinConnect.ShearEndPlate.Bolt.EdgeDistLongDir + ") * " + component.WinConnect.ShearEndPlate.ConnectionPlateThickness);
					Reporting.AddLine("= " + Agv + ConstUnit.Area);

					Anv = Agv - 2 * (component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts - 0.5) * (component.WinConnect.ShearEndPlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH) * component.WinConnect.ShearEndPlate.ConnectionPlateThickness;
					Reporting.AddLine("Anv = Agv - 2 * (N - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("= " + Agv + " - 2 * (" + component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts + " - 0.5) * (" + component.WinConnect.ShearEndPlate.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.WinConnect.ShearEndPlate.ConnectionPlateThickness);
					Reporting.AddLine("= " + Anv + ConstUnit.Area);

					Agt = 2 * component.WinConnect.ShearEndPlate.Bolt.EdgeDistTransvDir * component.WinConnect.ShearEndPlate.ConnectionPlateThickness;
					Reporting.AddLine("Agt = 2 * et * t =  2 * " + component.WinConnect.ShearEndPlate.Bolt.EdgeDistTransvDir + " * " + component.WinConnect.ShearEndPlate.ConnectionPlateThickness + " = " + Agt + ConstUnit.Area);

					Ant = Agt - 2 * 0.5 * (component.WinConnect.ShearEndPlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * component.WinConnect.ShearEndPlate.ConnectionPlateThickness;
					Reporting.AddLine("Ant = Agt - 2 * 0.5 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("= " + Agt + " - 2 * 0.5 * (" + component.WinConnect.ShearEndPlate.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ") * " + component.WinConnect.ShearEndPlate.ConnectionPlateThickness);
					Reporting.AddLine("= " + Ant + ConstUnit.Area);

					FiRn = SmallMethodsDesign.BlockShearPrint(Agv, Anv, Agt, Ant, component.WinConnect.ShearEndPlate.Material.Fy, component.WinConnect.ShearEndPlate.Material.Fu, true);

					if (FiRn >= V)
						Reporting.AddLine("= " + FiRn + " >= " + V + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + FiRn + " << " + V + ConstUnit.Force + " (NG)");

					if (componentIsBeam)
						thick = component.Shape.tw;
					else
						thick = component.BraceConnect.Gusset.Thickness;

					edg = (component.WinConnect.ShearEndPlate.Width - BF - 2 * boltDistToHSSside) / 2;
					a = (component.WinConnect.ShearEndPlate.Width - BF) / 2 - boltDistToHSSside;
					b = boltDistToHSSside;

					switch (memberType)
					{
						case EMemberType.RightBeam:
						case EMemberType.LeftBeam:
							//EndPlateBoltsForFlange = false;
							Ta_Web = SmallMethodsDesign.HangerAllowable(memberType, component.WinConnect.ShearEndPlate.ConnectionPlateThickness, ref edg, b, Ball_Web, true);
							Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Tension Strength:");
							FiRn = 2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts * Ta_Web;
							Reporting.AddLine(ConstString.PHI + " Rn = n * Ta");
							Reporting.AddLine("= " + (2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts) + " * " + Ta_Web);
							if (FiRn >= H)
								Reporting.AddLine("= " + FiRn + " >= " + H + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= " + FiRn + " << " + H + ConstUnit.Force + " (NG)");
							break;
						default:
							Ta = SmallMethodsDesign.HangerAllowable(memberType, component.WinConnect.ShearEndPlate.ConnectionPlateThickness, ref edg, b, Ball, true);
							Reporting.AddHeader("Reduction in Tension Strength due to Moment = Tm");
							Reporting.AddLine("= " + Reduction + ConstUnit.Force);

							Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Tension Strength:");
							FiRn = 2 * component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts * Ta - Reduction;
							Reporting.AddLine(ConstString.PHI + " Rn = 2 * n * Ta - Tm");
							Reporting.AddLine("= 2 * " + component.WinConnect.ShearEndPlate.Bolt.NumberOfBolts + " * " + Ta + " - " + Reduction);
							if (FiRn >= H)
								Reporting.AddLine("= " + FiRn + " >= " + H + ConstUnit.Force + " (OK)");
							else
								Reporting.AddLine("= " + FiRn + " << " + H + ConstUnit.Force + " (NG)");
							break;
					}

					Reporting.AddHeader("Connection Plate to HSS Weld");
					Th = component.WinConnect.ShearEndPlate.ConnectionPlateThickness;
					L = component.WinConnect.ShearEndPlate.Length;
					Ru = Math.Pow(Math.Pow(H, 2) + Math.Pow(V, 2), 0.5);

					Reporting.AddHeader("Resultant Force:");
					Reporting.AddLine("Ru = (H² + V²)^0.5 = (" + H + " ^ 2 + " + V + " ^ 2) ^ 0.5 = " + Ru + ConstUnit.Force);
					CommonDataStatic.LShapedWeld = true;
					if (V != 0)
						Theta = Math.Atan(Math.Abs(H / V));

					k = 0;
					if (Mom == 0)
						a = 0;
					else if (H == 0)
						a = 3;
					else
						a = Mom / H / L;
					Cnst = EccentricWeld.GetEccentricWeld(k, a, Theta, true);
					//EccentricWeld.GetEccentricWeld(k, a, ref Cnst, Theta, true);

					CommonDataStatic.LShapedWeld = false;
					c1 = CommonCalculations.WeldTypeRatio();
					FiRn = 2 * 16 * c1 * Cnst * L * (5 / 8.0 * column.Shape.tf);

					Reporting.AddHeader("Weld " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * 16 * C1 * C * L * (5 / 8) * t");
					Reporting.AddLine("= 2 * 16 * " + c1 + " * " + Cnst + " * " + L + " * (5 / 8) * " + column.Shape.tf);
					if (FiRn >= Ru)
						Reporting.AddLine("= " + FiRn + " >= " + Ru + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + FiRn + " << " + Ru + ConstUnit.Force + " (NG)");
				}
			}
		}
	}
}