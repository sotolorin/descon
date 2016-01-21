using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class InitialCalcBrace
	{
		public static void InitialCalc(EMemberType memberType)
		{
			double eLeft = 0;
			double eRight = 0;
			double xRight2 = 0;
			double xLeft2 = 0;
			double dumy = 0;
			double Moment;
			double L;
			double eBottom = 0;
			double eTop = 0;
			double V;
			double H;
			double MomThresh = 0;
			int cs = 0;
			int bs = 0;
			string cms = "";
			string bms = "";
			double ecc;
			double ortasi = 0;
			double yi = 0;
			double AbsCos = 0;
			double AbsSin = 0;
			double Mb;
			double Mc;
			double Frc;
			string Contr;
			double R;
			double RComp = 0;
			double RTen = 0;
			double d;
			double Kp;
			string MCarriedby = "";
			double k;
			double Betabar;
			double Alfabar;
			double FreeGussetEdge;
			double alfa;
			double Beta = 0;
			double tt;
			double ec = 0;
			double eb = 0;
			double Theta = 0;
			double Gap;

			bool tempSpecialcase = false;
			double beamMoment;
			double columnMoment;

			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var brace = CommonDataStatic.DetailDataDict[memberType];
			DetailData beam;

			if (memberType == EMemberType.LowerLeft || memberType == EMemberType.UpperLeft) 
				beam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			else 
				beam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			BraceToGussetConnection.DesignBraceToGusset(memberType);

			if (!brace.BraceConnect.Gusset.BeamSide_User && brace.BraceConnect.Gusset.BeamSide == 0 && brace.BraceConnect.Gusset.DontConnectBeam)
				brace.BraceConnect.Gusset.BeamSide = beam.Shape.d;
			if (!brace.BraceConnect.Gusset.ColumnSide_User && brace.BraceConnect.Gusset.ColumnSide == 0 && brace.BraceConnect.Gusset.DontConnectBeam)
				brace.BraceConnect.Gusset.ColumnSide = beam.Shape.d;

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase)
				Gap = brace.BraceConnect.BasePlate.CornerClip + brace.BraceConnect.Gusset.ColumnSideSetback;
			else
			{
				if (brace.BraceConnect.Gusset.ColumnSideSetback > beam.EndSetback + beam.WinConnect.Beam.TCopeL)
					Gap = brace.BraceConnect.Gusset.ColumnSideSetback;
				else
					Gap = beam.EndSetback + beam.WinConnect.Beam.TCopeL;
			}

			switch (memberType)
			{
				case EMemberType.UpperRight:
					Theta = (90 - brace.Angle) * ConstNum.RADIAN;
					eb = (beam.Shape.d / 2) - brace.WorkPointY;

					if (column.WebOrientation == EWebOrientation.InPlane)
						ec = (column.Shape.d / 2) - brace.WorkPointX;
					else
						ec = 0;
					break;
				case EMemberType.LowerRight:
					Theta = (brace.Angle - 270) * ConstNum.RADIAN;
					eb = (beam.Shape.d / 2) + brace.WorkPointY;

					if (column.WebOrientation == EWebOrientation.InPlane)
						ec = (column.Shape.d / 2) - brace.WorkPointX;
					else
						ec = 0;
					break;
				case EMemberType.UpperLeft:
					Theta = (brace.Angle - 90) * ConstNum.RADIAN;
					eb = (beam.Shape.d / 2) - brace.WorkPointY + beam.WorkPointY;

					if (column.WebOrientation == EWebOrientation.InPlane)
						ec = (column.Shape.d / 2) - brace.WorkPointX;
					else
						ec = 0;
					break;
				case EMemberType.LowerLeft:
					Theta = (270 - brace.Angle) * ConstNum.RADIAN;
					eb = (beam.Shape.d / 2) + brace.WorkPointY - beam.WorkPointY;


					if (column.WebOrientation == EWebOrientation.InPlane)
						ec = (column.Shape.d / 2) - brace.WorkPointX;
					else
						ec = 0;
					break;
			}
			if (column.ShapeType == EShapeType.HollowSteelSection && column.WebOrientation == EWebOrientation.OutOfPlane)
				ec = column.Shape.bf / 2;
			if (Equals(beam.Shape.d, 0.0))
				eb = 0;

			tt = Math.Tan(Theta);

			if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BraceToColumnBase && brace.GussetToColumnConnection != EBraceConnectionTypes.DirectlyWelded)
			{
				if (Beta == 0.0)
					Beta = brace.BraceConnect.Gusset.ColumnSide / 2;
			}

			if (Math.Abs(ec) < 0.05 * column.Shape.d && Math.Abs(eb) < 0.05 * Math.Abs(brace.WorkPointY))
			{
				tempSpecialcase = true;
				ec = 0;
				eb = 0;
			}

			if (tempSpecialcase)
			{
				eb = 0;
				ec = 0;
			}
			else if (brace.BraceConnect.Gusset.DontConnectColumn)
				Beta = 0;

			alfa = (eb + Beta) * Math.Tan(Theta) - ec;

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase)
			{
				if (memberType == EMemberType.UpperLeft)
					FreeGussetEdge = brace.BraceConnect.Gusset.BeamSide + brace.BraceConnect.Gusset.ColumnSideSetback + column.Shape.d / 2 - brace.BraceConnect.BasePlate.LeftEdgeToColumn;
				else
					FreeGussetEdge = brace.BraceConnect.Gusset.BeamSide + brace.BraceConnect.Gusset.ColumnSideSetback + column.Shape.d / 2 - brace.BraceConnect.BasePlate.RightEdgeToColumn;
				if (FreeGussetEdge < 0)
					FreeGussetEdge = 0;
				Alfabar = (brace.BraceConnect.Gusset.BeamSide + brace.BraceConnect.Gusset.ColumnSideSetback - FreeGussetEdge + Gap) / 2;
			}
			else
				Alfabar = (brace.BraceConnect.Gusset.BeamSide + brace.BraceConnect.Gusset.ColumnSideSetback - Gap) / 2 + Gap;
			Betabar = Beta;

			if (brace.BraceConnect.Gusset.DontConnectBeam)
			{
				alfa = 0;
				Alfabar = 0;
				if (eb == 0.0)
					Betabar = 0;
			}
			else
			{
				k = eb * tt - ec;

				switch (brace.BraceConnect.Gusset.Moment)
				{
					case EMomentForEquilibrium.GussetToBeam:
						MCarriedby = "Beam interface";
						Beta = Betabar;
						alfa = k + Betabar * tt;
						break;
					case EMomentForEquilibrium.GussetToColumn:
						MCarriedby = "Column interface";
						alfa = Alfabar;
						Beta = -(k - Alfabar) / tt;
						break;
					case EMomentForEquilibrium.Both:
						if (alfa != Alfabar || Beta != Betabar)
						{
							if (Betabar == 0.0)
							{
								MCarriedby = CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase
									? "Base Plate interface"
									: "Beam interface";
								Beta = Betabar;
								alfa = k + Betabar * tt;
							}
							else
							{
								MCarriedby = CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase
									? "Base Plate and Column interfaces"
									: "Beam and Column interfaces";
								Kp = Alfabar * (tt + Alfabar / Betabar);
								d = (tt * tt + Math.Pow(Alfabar / Betabar, 2));
								alfa = (Kp * tt + k * Math.Pow(Alfabar / Betabar, 2)) / d;
								Beta = (Kp - k * tt) / d;
							}
						}
						break;
				}
			}

			if (brace.AxialTension != 0)
				RTen = brace.AxialTension / Math.Sqrt(Math.Pow(alfa + ec, 2) + Math.Pow(Beta + eb, 2));
			if (brace.AxialCompression != 0)
				RComp = brace.AxialCompression / Math.Sqrt(Math.Pow(alfa + ec, 2) + Math.Pow(Beta + eb, 2));

			if (double.IsInfinity(RTen))
				RTen = 0;
			if (double.IsInfinity(RComp))
				RComp = 0;

			if (RTen >= RComp)
			{
				R = RTen;
				Contr = "Tension";
				Frc = brace.AxialTension;
			}
			else
			{
				R = RComp;
				Contr = "Compression";
				Frc = brace.AxialCompression;
			}

			if (!brace.BraceConnect.Gusset.BeamSide_User)
				brace.BraceConnect.Gusset.BeamSide = alfa * R;
			brace.BraceConnect.Gusset.Hc = ec * R;
			Mc = Math.Abs(brace.BraceConnect.Gusset.Hc * (Beta - Betabar));

			if (brace.BraceConnect.Gusset.VerticalForce_User)
			{
				Mb = Math.Abs(alfa * (eb * R - brace.BraceConnect.Gusset.VerticalForceBeam));

				if(alfa != Alfabar)
					Mb = Mb + Math.Abs(brace.BraceConnect.Gusset.VerticalForceBeam * (alfa - Alfabar));
			}
			else if (brace.BraceConnect.Gusset.DontConnectColumn)
			{
				AbsSin = Math.Abs(Math.Sin(Theta));
				AbsCos = Math.Abs(Math.Cos(Theta));
				if (!brace.BraceConnect.Gusset.BeamSide_User)
					brace.BraceConnect.Gusset.BeamSide = Frc * AbsSin;
				brace.BraceConnect.Gusset.VerticalForceBeam = Frc * AbsCos;
				Mb = Frc * ((Alfabar + ec) * AbsCos - eb * AbsSin);
				brace.BraceConnect.Gusset.Hc = 0;
				Mc = 0;
				brace.BraceConnect.Gusset.ColumnSide = 0;
			}
			else
			{
				brace.BraceConnect.Gusset.VerticalForceBeam = eb * R;
				if (!brace.BraceConnect.Gusset.ColumnSide_User)
					brace.BraceConnect.Gusset.ColumnSide = Beta * R;
				Mb = Math.Abs(brace.BraceConnect.Gusset.VerticalForceBeam * (alfa - Alfabar));
			}

			if (brace.BraceConnect.Gusset.DontConnectBeam)
			{
				AbsSin = Math.Abs(Math.Sin(Theta));
				AbsCos = Math.Abs(Math.Cos(Theta));
				brace.BraceConnect.Gusset.Hc = Frc * AbsSin;
				if (!brace.BraceConnect.Gusset.ColumnSide_User)
					brace.BraceConnect.Gusset.ColumnSide = Frc * AbsCos;

				ecc = yi - ortasi;
				Mc = brace.BraceConnect.Gusset.Hc * ecc;
				brace.BraceConnect.Gusset.BeamSide = 0;
				Mb = 0;
				brace.BraceConnect.Gusset.VerticalForceBeam = 0;
			}
			if (tempSpecialcase)
			{
				switch (memberType)
				{
					case EMemberType.UpperRight:
						bs = 1;
						cs = 1;
						break;
					case EMemberType.LowerRight:
						bs = -1;
						cs = -1;
						break;
					case EMemberType.UpperLeft:
						bs = 1;
						cs = -1;
						break;
					case EMemberType.LowerLeft:
						bs = -1;
						cs = 1;
						break;
				}
				beamMoment = bs * brace.BraceConnect.Gusset.BeamSide * beam.Shape.d / 2;
				columnMoment = cs * brace.BraceConnect.Gusset.ColumnSide * column.Shape.d / 4;
			}
			else
			{
				beamMoment = 0;
				columnMoment = 0;
			}

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
			{
				if (!brace.BraceConnect.Gusset.BeamSide_User)
					brace.BraceConnect.Gusset.BeamSide = Frc * Math.Abs(Math.Cos(brace.Angle * ConstNum.RADIAN));
				brace.BraceConnect.Gusset.VerticalForceBeam = Frc * Math.Abs(Math.Sin(brace.Angle * ConstNum.RADIAN));
				brace.BraceConnect.Gusset.ColumnSide = 0;
				brace.BraceConnect.Gusset.Hc = 0;
			}
			else if (brace.BraceConnect.Gusset.DontConnectBeam)
			{
				AbsSin = Math.Abs(Math.Sin(Theta));
				AbsCos = Math.Abs(Math.Cos(Theta));
				brace.BraceConnect.Gusset.Hc = Frc * AbsSin;
				if (!brace.BraceConnect.Gusset.ColumnSide_User)
					brace.BraceConnect.Gusset.ColumnSide = Frc * AbsCos;

				ecc = yi - ortasi;
				Mc = brace.BraceConnect.Gusset.Hc * ecc;
				brace.BraceConnect.Gusset.BeamSide = 0;
				Mb = 0;
				brace.BraceConnect.Gusset.VerticalForceBeam = 0;
			}

			brace.BraceConnect.Gusset.Mb = Math.Abs(Mb);
			brace.BraceConnect.Gusset.Mc = Math.Abs(Mc);

			if (Contr == "Tension")
			{
				brace.BraceConnect.Gusset.GussetEFTension.Vc = brace.BraceConnect.Gusset.ColumnSide;
				brace.BraceConnect.Gusset.GussetEFTension.Vb = brace.BraceConnect.Gusset.VerticalForceBeam;
				brace.BraceConnect.Gusset.GussetEFTension.Hc = brace.BraceConnect.Gusset.Hc;
				brace.BraceConnect.Gusset.GussetEFTension.Hb = brace.BraceConnect.Gusset.BeamSide;
				brace.BraceConnect.Gusset.GussetEFTension.Mc = brace.BraceConnect.Gusset.Mc;
				brace.BraceConnect.Gusset.GussetEFTension.Mb = brace.BraceConnect.Gusset.Mb;

				if (brace.AxialTension != 0 && brace.AxialCompression != 0)
				{
					brace.BraceConnect.Gusset.GussetEFTension.Vc = brace.BraceConnect.Gusset.ColumnSide * brace.AxialCompression / brace.AxialTension;
					brace.BraceConnect.Gusset.GussetEFCompression.Vb = brace.BraceConnect.Gusset.VerticalForceBeam * brace.AxialCompression / brace.AxialTension;
					brace.BraceConnect.Gusset.GussetEFTension.Hc = brace.BraceConnect.Gusset.Hc * brace.AxialCompression / brace.AxialTension;
					brace.BraceConnect.Gusset.GussetEFTension.Hb = brace.BraceConnect.Gusset.BeamSide * brace.AxialCompression / brace.AxialTension;
					brace.BraceConnect.Gusset.GussetEFCompression.Mc = brace.BraceConnect.Gusset.Mc * brace.AxialCompression / brace.AxialTension;
					brace.BraceConnect.Gusset.GussetEFCompression.Mb = brace.BraceConnect.Gusset.Mb * brace.AxialCompression / brace.AxialTension;
				}
			}
			else
			{
				brace.BraceConnect.Gusset.GussetEFTension.Vc = brace.BraceConnect.Gusset.ColumnSide;
				brace.BraceConnect.Gusset.GussetEFCompression.Vb = brace.BraceConnect.Gusset.VerticalForceBeam;
				brace.BraceConnect.Gusset.GussetEFTension.Hc = brace.BraceConnect.Gusset.Hc;
				brace.BraceConnect.Gusset.GussetEFTension.Hb = brace.BraceConnect.Gusset.BeamSide;
				brace.BraceConnect.Gusset.GussetEFCompression.Mc = brace.BraceConnect.Gusset.Mc;
				brace.BraceConnect.Gusset.GussetEFCompression.Mb = brace.BraceConnect.Gusset.Mb;

				if (brace.AxialCompression != 0 && brace.AxialCompression != 0)
				{
					brace.BraceConnect.Gusset.GussetEFTension.Vc = brace.BraceConnect.Gusset.ColumnSide * brace.AxialTension / brace.AxialCompression;
					brace.BraceConnect.Gusset.GussetEFTension.Vb = brace.BraceConnect.Gusset.VerticalForceBeam * brace.AxialTension / brace.AxialCompression;
					brace.BraceConnect.Gusset.GussetEFTension.Hc = brace.BraceConnect.Gusset.Hc * brace.AxialTension / brace.AxialCompression;
					brace.BraceConnect.Gusset.GussetEFTension.Hb = brace.BraceConnect.Gusset.BeamSide * brace.AxialTension / brace.AxialCompression;
					brace.BraceConnect.Gusset.GussetEFTension.Mc = brace.BraceConnect.Gusset.Mc * brace.AxialTension / brace.AxialCompression;
					brace.BraceConnect.Gusset.GussetEFTension.Mb = brace.BraceConnect.Gusset.Mb * brace.AxialTension / brace.AxialCompression;
				}
			}

			Reporting.AddHeader("Gusset Edge Forces");

			if (!((brace.KneeBrace | brace.KBrace) || CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam))
			{
				if (brace.BraceConnect.Gusset.DontConnectBeam)
					Reporting.AddLine("Theta = " + Theta * 180 / Math.PI + " Degrees (eb) = " + eb + ConstUnit.Length + "  ec = " + ec + ConstUnit.Length);
				else
				{
					if ((brace.BraceConnect.Gusset.Mb > 0 || brace.BraceConnect.Gusset.Mc > 0) && !String.IsNullOrEmpty(MCarriedby))
						Reporting.AddHeader("Gusset edge moments carried by: " + MCarriedby);
					Reporting.AddLine("Theta = " + Theta * 180 / Math.PI);
					Reporting.AddLine("Degrees (eb) = " + eb + ConstUnit.Length);
					Reporting.AddLine("ec = " + ec + ConstUnit.Length);
					Reporting.AddLine("Beta = " + Beta + ConstUnit.Length);
					Reporting.AddLine("BetaBar = " + Betabar + ConstUnit.Length);
					Reporting.AddLine("AlphaBar = " + Alfabar + ConstUnit.Length);
					Reporting.AddLine("Alpha = (Beta + eb) * Tan(Theta) - ec");
					Reporting.AddLine("= (" + Beta + " + " + eb + ") * Tan(" + Theta * 180 / Math.PI + ") - " + ec);
					Reporting.AddLine("= " + alfa + ConstUnit.Length);
				}
			}

			for (int i = 0; i < 2; i++) // Loop through two times
			{
				switch (i)
				{
					case 0:
						Reporting.AddHeader("With Tensile Brace Force:");
						Frc = brace.AxialTension;
						if (!brace.BraceConnect.Gusset.ColumnSide_User)
							brace.BraceConnect.Gusset.ColumnSide = brace.BraceConnect.Gusset.GussetEFTension.Vc;
						brace.BraceConnect.Gusset.VerticalForceBeam = brace.BraceConnect.Gusset.GussetEFTension.Vb;
						brace.BraceConnect.Gusset.Hc = brace.BraceConnect.Gusset.GussetEFTension.Hc;
						if (!brace.BraceConnect.Gusset.BeamSide_User)
							brace.BraceConnect.Gusset.BeamSide = brace.BraceConnect.Gusset.GussetEFTension.Hb;
						brace.BraceConnect.Gusset.Mc = brace.BraceConnect.Gusset.GussetEFTension.Mc;
						brace.BraceConnect.Gusset.Mb = brace.BraceConnect.Gusset.GussetEFTension.Mb;
						Mc = brace.BraceConnect.Gusset.Mc;
						Mb = brace.BraceConnect.Gusset.Mb;
						R = RTen;
						break;
					case 1:
						Reporting.AddHeader("With Compressive Brace Force:");
						Frc = brace.AxialCompression;
						if (!brace.BraceConnect.Gusset.ColumnSide_User)
							brace.BraceConnect.Gusset.ColumnSide = brace.BraceConnect.Gusset.GussetEFTension.Vc;
						brace.BraceConnect.Gusset.VerticalForceBeam = brace.BraceConnect.Gusset.GussetEFCompression.Vb;
						brace.BraceConnect.Gusset.Hc = brace.BraceConnect.Gusset.GussetEFTension.Hc;
						if (!brace.BraceConnect.Gusset.BeamSide_User)
							brace.BraceConnect.Gusset.BeamSide = brace.BraceConnect.Gusset.GussetEFTension.Hb;
						brace.BraceConnect.Gusset.Mc = brace.BraceConnect.Gusset.GussetEFCompression.Mc;
						brace.BraceConnect.Gusset.Mb = brace.BraceConnect.Gusset.GussetEFCompression.Mb;
						Mc = brace.BraceConnect.Gusset.Mc;
						Mb = brace.BraceConnect.Gusset.Mb;
						R = RComp;
						break;
				}
				if (brace.BraceConnect.Gusset.DontConnectColumn && ec > 0)
				{
					AbsSin = Math.Abs(Math.Sin(Theta));
					AbsCos = Math.Abs(Math.Cos(Theta));
					if (!brace.BraceConnect.Gusset.BeamSide_User)
						brace.BraceConnect.Gusset.BeamSide = Frc * AbsSin;
					brace.BraceConnect.Gusset.VerticalForceBeam = Frc * AbsCos;
					Mb = Frc * ((Alfabar + ec) * AbsCos - eb * AbsSin);
					brace.BraceConnect.Gusset.Hc = 0;
					Mc = 0;
					brace.BraceConnect.Gusset.ColumnSide = 0;
					Reporting.AddLine("Hb = P * Sin(Theta) = " + Frc + " * " + AbsSin + ") = " + brace.BraceConnect.Gusset.BeamSide + ConstUnit.Force);
					Reporting.AddLine("Vb = P * Cos(Theta) = " + Frc + " * " + AbsCos + ") = " + brace.BraceConnect.Gusset.VerticalForceBeam + ConstUnit.Force);
					Reporting.AddLine("Mb = P * ((AlphaBar + ec) * Cos(Theta) - eb * Sin(Theta))");
					Reporting.AddLine("= " + Frc + " * ((" + Alfabar + " + " + ec + ") * " + AbsCos + " - " + eb + " * " + AbsSin + ")");
					Reporting.AddLine("= " + Mb + ConstUnit.Moment);
					Reporting.AddLine("Hc = 0");
					Reporting.AddLine("Vc = 0");
					Reporting.AddLine("Mc = 0");
				}
				else if (brace.BraceConnect.Gusset.DontConnectBeam)
				{
					AbsSin = Math.Abs(Math.Sin(Theta));
					AbsCos = Math.Abs(Math.Cos(Theta));
					brace.BraceConnect.Gusset.Hc = Frc * AbsSin;
					if (!brace.BraceConnect.Gusset.ColumnSide_User)
						brace.BraceConnect.Gusset.ColumnSide = Frc * AbsCos;
					ecc = yi - ortasi;

					Mc = brace.BraceConnect.Gusset.Hc * ecc;
					brace.BraceConnect.Gusset.BeamSide = 0;
					Mb = 0;
					brace.BraceConnect.Gusset.VerticalForceBeam = 0;
					Reporting.AddLine("Hc = P * Sin(Theta) = " + Frc + " * " + AbsSin + ") = " + brace.BraceConnect.Gusset.Hc + ConstUnit.Force);
					Reporting.AddLine("Vc = P * Cos(Theta) = " + Frc + " * " + AbsCos + ") = " + brace.BraceConnect.Gusset.ColumnSide + ConstUnit.Force);
					Reporting.AddLine("Eccentricity = " + ecc + ConstUnit.Length);
					Reporting.AddLine("Mc = Hc * ecc = " + brace.BraceConnect.Gusset.Hc + " * " + ecc + " = " + Mc + ConstUnit.Moment);
					Reporting.AddLine("Hb = 0");
					Reporting.AddLine("Vb = 0");
					Reporting.AddLine("Mb = 0");
				}
				else
				{
					Reporting.AddLine("r = Fx / ((Alpha + ec)² + (beta + eb)²)^0.5");
					Reporting.AddLine("= " + Frc + " / ((" + alfa + " + " + ec + ")² + (" + Beta + " + " + eb + ")²)^0.5");
					Reporting.AddLine("= " + R + ConstUnit.ForceUniform);
					Reporting.AddLine("Hb = Alpha * r = " + alfa + " * " + R);
					Reporting.AddLine("= " + alfa * R + ConstUnit.Force);
					Reporting.AddLine("Hc = ec * r = " + ec + " * " + R);
					Reporting.AddLine("= " + ec * R + ConstUnit.Force);

					if (brace.BraceConnect.Gusset.VerticalForce_User)
					{
						Reporting.AddLine("Vb = " + brace.BraceConnect.Gusset.VerticalForceBeam + ConstUnit.Force + " (Adjusted by user)");
						Reporting.AddLine("Vc = " + brace.BraceConnect.Gusset.ColumnSide + ConstUnit.Force + " (Adjusted by user)");
					}
					else
					{
						Reporting.AddLine("Vb = eb * r = " + eb + " * " + R);
						Reporting.AddLine("= " + brace.BraceConnect.Gusset.VerticalForceBeam + ConstUnit.Force);
						Reporting.AddLine("Vc = beta * r = " + Beta + " * " + R);
					}

					switch (CommonDataStatic.Units)
					{
						case EUnit.US:
							MomThresh = 0.5;
							break;
						case EUnit.Metric:
							MomThresh = 56500;
							break;
					}
					if (Mb <= MomThresh)
						Reporting.AddLine("Mb = 0");
					else
					{
						if (brace.BraceConnect.Gusset.VerticalForce_User)
						{
							Reporting.AddLine("Mb = |Alpha * (eb * r - Vb) + Alpha * (eb * r - Vb)| ");
							Reporting.AddLine("= |" + alfa + " * (" + eb + " * " + R + " - " + brace.BraceConnect.Gusset.VerticalForceBeam + " + " + alfa + " * (" + eb + " * " + R + " - " + brace.BraceConnect.Gusset.VerticalForceBeam + "|");
						}
						else
						{
							Reporting.AddLine("Mb = |Vb * (Alpha - AlphaBar)|");
							Reporting.AddLine("= |" + brace.BraceConnect.Gusset.VerticalForceBeam + " * (" + alfa + " - " + Alfabar + ")|");
						}
						Reporting.AddLine("= " + Mb + ConstUnit.Moment);
					}

					if (Mc <= MomThresh)
						Reporting.AddLine("Mc = 0");
					else
					{
						Reporting.AddHeader("Mc = |brace.BraceConnect.Gusset.Hc * (Beta - BetaBar)|");
						Reporting.AddLine("= |" + brace.BraceConnect.Gusset.Hc + " * (" + Beta + " - " + Betabar + ")|");
						Reporting.AddLine("= " + Mc + ConstUnit.Moment);
					}
					if (tempSpecialcase && CommonDataStatic.BeamToColumnType != EJointConfiguration.BraceToColumnBase)
					{
						Reporting.AddHeader("Additional Moment in Beam = brace.BraceConnect.Gusset.BeamSide * db / 2");
						Reporting.AddLine("= " + bms + brace.BraceConnect.Gusset.BeamSide + " * " + brace.Shape.d + "/ 2 = " + beamMoment + ConstUnit.Moment);
						Reporting.AddHeader("Additional Moment in Column = brace.BraceConnect.Gusset.ColumnSide * dc / 4");
						Reporting.AddLine("= " + cms + brace.BraceConnect.Gusset.ColumnSide + " * " + brace.Shape.d + "/ 4 = " + columnMoment + ConstUnit.Moment);
					}
				}
				if ((brace.KneeBrace | brace.KBrace) || CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
				{
					Reporting.AddHeader("Theta = " + Theta / ConstNum.RADIAN + " Degrees");
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
					{
						Reporting.AddLine("Vb = P * Cos(Theta) = " + Frc + " * " + Math.Cos(Theta) + " = " + brace.BraceConnect.Gusset.VerticalForceBeam + ConstUnit.Force);
						Reporting.AddLine("Hb = P * Sin(Theta) = " + Frc + " * " + Math.Sin(Theta) + " = " + brace.BraceConnect.Gusset.BeamSide + ConstUnit.Force);
					}
					else
					{
						Reporting.AddLine("Hc = P * Sin(Theta) = " + Frc + " * " + AbsSin + " = " + brace.BraceConnect.Gusset.Hc + ConstUnit.Force);
						Reporting.AddLine("Vc = P * Cos(Theta) = " + Frc + " * " + AbsCos + " = " + brace.BraceConnect.Gusset.ColumnSide + ConstUnit.Force);
					}

					if (brace.KBrace)
					{
						if (memberType == EMemberType.UpperLeft || memberType == EMemberType.UpperRight)
							Reporting.AddHeader("See below for combination of upper and lower brace forces.");
						if (memberType == EMemberType.LowerLeft || memberType == EMemberType.LowerRight)
						{
							var upperBrace = memberType == EMemberType.LowerLeft
								? CommonDataStatic.DetailDataDict[EMemberType.UpperLeft]
								: CommonDataStatic.DetailDataDict[EMemberType.UpperRight];

							Reporting.AddHeader("Combined Forces:");
							H = upperBrace.BraceConnect.Gusset.Hc + brace.BraceConnect.Gusset.Hc;
							Reporting.AddLine("Hc = HcTop + HcBot = " + brace.BraceConnect.Gusset.Hc + " + " + brace.BraceConnect.Gusset.Hc + " = " + H + ConstUnit.Force);
							V = upperBrace.BraceConnect.Gusset.ColumnSide - brace.BraceConnect.Gusset.ColumnSide;
							Reporting.AddLine("Vc = VcTop - VcBot = " + brace.BraceConnect.Gusset.ColumnSide + " - " + brace.BraceConnect.Gusset.ColumnSide + " = " + V + ConstUnit.Force);
							Moment = eTop * upperBrace.BraceConnect.Gusset.Hc - eBottom * brace.BraceConnect.Gusset.Hc;
							Reporting.AddLine("Mc = eTop * HcTop - eBot * HcBot");
							Reporting.AddLine("= " + eTop + " * " + brace.BraceConnect.Gusset.Hc + " - " + eBottom + " * " + brace.BraceConnect.Gusset.Hc);
							Reporting.AddLine("= " + Moment + ConstUnit.Moment);

							if (memberType == EMemberType.LowerLeft)
								CommonDataStatic.DetailDataDict[EMemberType.UpperLeft] = upperBrace;
							else
								CommonDataStatic.DetailDataDict[EMemberType.UpperRight] = upperBrace;
						}
					}
					else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
					{
						if (memberType == EMemberType.LowerRight || memberType == EMemberType.UpperRight)
							Reporting.AddHeader("See below for combination of left and right side brace forces.");
						if (memberType == EMemberType.UpperLeft || memberType == EMemberType.LowerLeft)
						{
							var otherSide = memberType == EMemberType.LowerLeft
								? CommonDataStatic.DetailDataDict[EMemberType.LowerRight]
								: CommonDataStatic.DetailDataDict[EMemberType.UpperRight];

							Reporting.AddHeader("Combined Forces:");
							V = Math.Abs(brace.BraceConnect.Gusset.VerticalForceBeam + otherSide.BraceConnect.Gusset.VerticalForceBeam);
							H = -brace.BraceConnect.Gusset.BeamSide + otherSide.BraceConnect.Gusset.BeamSide;
							Reporting.AddLine("H = HbRight - HbLeft   = " + brace.BraceConnect.Gusset.BeamSide + " - " + otherSide.BraceConnect.Gusset.BeamSide + " = " + H + ConstUnit.Force);
							Reporting.AddLine("V = VbRight + VbLeft = " + brace.BraceConnect.Gusset.VerticalForceBeam + " + " + otherSide.BraceConnect.Gusset.VerticalForceBeam + " = " + V + ConstUnit.Force);
							//TODO - Figure out what, 26, 36, 25 are
							//ortasi = (BRACE1.x[(26 + i1).ToInt()] + BRACE1.x[(36 + i1).ToInt()]) / 2;
							//L = BRACE1.x[(26 + i1).ToInt()] - BRACE1.x[(36 + i1).ToInt()];
							//SmallMethodsDesign.Intersect(0, BRACE1.x[(25 + i1).ToInt()].ToDbl(), BRACE1.y[(25 + i1).ToInt()].ToDbl(), Math.Tan(beam.Angle * ConstNum.RADIAN), beam.WorkPointX, beam.WorkPointY, ref xRight2, ref dumy);
							//eRight = xRight2 - ortasi;
							//SmallMethodsDesign.Intersect(0, BRACE1.x[(25 + i1).ToInt()].ToDbl(), BRACE1.y[(25 + i1).ToInt()].ToDbl(), Math.Tan(brace.Angle * ConstNum.RADIAN), brace.WorkPointX, brace.WorkPointY, ref xLeft2, ref dumy);
							//eLeft = xLeft2 - ortasi;
							
							Moment = eLeft * brace.BraceConnect.Gusset.VerticalForceBeam + eRight * otherSide.BraceConnect.Gusset.VerticalForceBeam;
							Reporting.AddLine("M = eLeft * VbLeft + eRight * VbRight");
							Reporting.AddLine("=  " + eLeft + " * " + brace.BraceConnect.Gusset.VerticalForceBeam + " + " + eRight + " * " + otherSide.BraceConnect.Gusset.VerticalForceBeam);
							Reporting.AddLine("= " + Moment + ConstUnit.Moment);

							if (memberType == EMemberType.LowerLeft)
								CommonDataStatic.DetailDataDict[EMemberType.LowerRight] = otherSide;
							else
								CommonDataStatic.DetailDataDict[EMemberType.UpperRight] = otherSide;
						}
					}
				}
			}

			CommonDataStatic.DetailDataDict[memberType].BraceConnect.Gusset.Thickness = GussetThickness.CalcGussetThickness(memberType);
            GussetToColumn.CalcGussetToColumn(memberType);
		}

        public static void TubularSetup(EMemberType memberType)
        {
            var brace = CommonDataStatic.DetailDataDict[memberType];

	        if (!brace.EndSetback_User)
	        {
		        if (brace.ShapeType == EShapeType.HollowSteelSection && brace.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
			        brace.EndSetback = ConstNum.HALF_INCH;
		        else
			        brace.EndSetback = -ConstNum.ONE_INCH;
	        }
        }
	}
}