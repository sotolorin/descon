using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class GussetSize
	{
		public static void CalcGussetSize(EMemberType memberType)
		{
			double yy = 0;
			double xx = 0;
			double slp2 = 0;
			double slp1 = 0;
			double ydumy = 0;
			double yy1 = 0;
			double xx1 = 0;
			double sc = 0;
			double ym = 0;
			double xm = 0;
			double sb = 0;
			double AxB = 0;
			double AxK = 0;
			double AxF = 0;
			double AxG = 0;
			double yN = 0;
			double xN = 0;
			double yH = 0;
			double xH = 0;
			double yG = 0;
			double xG = 0;
			double H = 0;
			double gbg = 0;
			double alfa = 0;
			double Beta = 0;
			double xeb = 0;
			double yechange = 0;
			double yep = 0;
			double la = 0;
			double edg = 0;
			double yBottom = 0;
			double yTop = 0;
			int j2 = 0;
			int j1 = 0;
			double yL = 0;
			double xL = 0;
			double xK = 0;
			double yK = 0;
			double yf = 0;
			double xf = 0;
			double ye = 0;
			double xe = 0;
			double Yb = 0;
			double ya = 0;
			double yD = 0;
			double xD = 0;
			double Xb = 0;
			double xat = 0;
			double xaTest = 0;
			double xa = 0;
			double PullBack = 0;
			double ec = 0;
			double eb = 0;
			double cv = 0;
			double ch = 0;
			double y1 = 0;
			double x1 = 0;
			double g = 0;
			double V = 0;
			double lpv = 0;
			double x_A_to_CL_BraceEnd = 0;
			double lph = 0;
			double lp = 0;
			double lg = 0;
			double d2 = 0;
			double d1 = 0;
			double d = 0;
			double dbeam = 0;
			double dcol = 0;
			double boltRange = 0;
			EPosition position;
			double osl = 0;
			double attachmentlength = 0;
			double y0 = 0;
			double x0 = 0;
			double absCos = 0;
			double absSin = 0;
			double cos = 0;
			double sin = 0;
			double Th = 0;
			double xGmaxForSeismic = 0;
			// These are used to hold the values of BRACE1.y[i1], braceYi1Plus1, etc.
			double braceYi1, braceXi1;
			double braceYi1Plus1, braceXi1Plus1;
			double braceYi1Plus2, braceXi1Plus2;
			double braceYi1Plus3, braceXi1Plus3;
			double braceYi1Plus4, braceXi1Plus4;

			string beamType = string.Empty;

			DetailData beam;
			var component = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			if (memberType == EMemberType.UpperLeft || memberType == EMemberType.LowerLeft)
				beam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			else
				beam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			if ((component.KBrace | component.KneeBrace))
				CommonDataStatic.Preferences.Seismic = ESeismic.NonSeismic;

			if (column.WebOrientation == EWebOrientation.InPlane)
				Th = column.Shape.d / 2;
			else if (column.ShapeType == EShapeType.HollowSteelSection)
				Th = column.Shape.bf / 2;
			else
				Th = component.Shape.tw / 2;

			sin = Math.Sin(component.Angle * ConstNum.RADIAN);
			cos = Math.Cos(component.Angle * ConstNum.RADIAN);
			absSin = Math.Abs(sin);
			absCos = Math.Abs(cos);
			x0 = component.WorkPointX;
			y0 = component.WorkPointY;

			switch (component.GussetToColumnConnection)
			{
				case EBraceConnectionTypes.ClipAngle:
					attachmentlength = component.WinConnect.ShearClipAngle.Length;
					osl = Math.Sign(cos) * (component.WinConnect.ShearClipAngle.ShortLeg + component.WinConnect.ShearClipAngle.LongLeg - component.WinConnect.ShearClipAngle.LengthOfOSL + ConstNum.ONE_INCH);
					position = component.WinConnect.ShearClipAngle.Position;
					boltRange = component.WinConnect.ShearClipAngle.TopOfBeamToBolt;
					break;
				case EBraceConnectionTypes.FabricatedTee:
					attachmentlength = component.BraceConnect.FabricatedTee.Length;
					osl = Math.Sign(cos) * component.BraceConnect.FabricatedTee.D;
					if (memberType == EMemberType.UpperLeft || memberType == EMemberType.UpperRight)
						position = EPosition.Bottom;
					else
						position = EPosition.Top;
					boltRange = component.BraceConnect.FabricatedTee.FirstBoltDistance;
					break;
				case EBraceConnectionTypes.DirectlyWelded:
					if ((component.KBrace | component.KneeBrace) == false)
						attachmentlength = component.BraceConnect.Gusset.VerticalForceColumn;
					osl = 0;
					position = EPosition.Top;
					boltRange = 0;
					break;
				case EBraceConnectionTypes.SinglePlate:
					attachmentlength = component.WinConnect.ShearWebPlate.Length;
					osl = Math.Sign(cos) * (component.WinConnect.ShearWebPlate.Width + ConstNum.ONE_INCH);
					if (memberType == EMemberType.UpperLeft || memberType == EMemberType.UpperRight)
						position = EPosition.Bottom;
					else
						position = EPosition.Top;
					boltRange = component.WinConnect.ShearWebPlate.BraceDistanceToFirstBolt;
					break;
				case EBraceConnectionTypes.EndPlate:
					attachmentlength = component.WinConnect.ShearEndPlate.Length;
					osl = Math.Sign(cos) * component.WinConnect.ShearEndPlate.Thickness;
					if (memberType == EMemberType.UpperLeft || memberType == EMemberType.UpperRight)
						position = EPosition.Bottom;
					else
						position = EPosition.Top;
					boltRange = component.WinConnect.ShearEndPlate.TOBtoFirstBolt;
					break;
				default:
					attachmentlength = 0;
					osl = 0;
					position = EPosition.NoConnection;
					boltRange = 0;
					break;
			}

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BraceVToBeam:
					braceXi1 = 0;
					braceYi1 = Math.Sign(sin) * CommonDataStatic.DetailDataDict[EMemberType.RightBeam].Shape.d / 2;
					break;
				case EJointConfiguration.BraceToColumnBase:
					braceXi1 = Math.Sign(cos) * (Th + component.BraceConnect.Gusset.ColumnSideSetback);
					if (component.BraceConnect.Gusset.DontConnectBeam)
						braceYi1 = component.BraceConnect.BasePlate.Thickness / 2 + component.BraceConnect.BasePlate.CornerClip;
					else
						braceYi1 = component.BraceConnect.BasePlate.Thickness / 2;
					break;
				default: // EJointConfiguration.BraceToColumn
					if (beam.IsActive)
					{
						braceXi1 = Math.Sign(cos) * (Th + component.BraceConnect.Gusset.ColumnSideSetback);
						braceYi1 = Math.Sign(sin) * (beam.Shape.d / 2) + beam.WorkPointY;
					}
					else
					{
						braceXi1 = Math.Sign(cos) * (Th + component.BraceConnect.Gusset.ColumnSideSetback);
						if (component.GussetToColumnConnection == EBraceConnectionTypes.DirectlyWelded)
						{
							braceYi1 = (braceXi1 - component.WorkPointX) * sin / cos + component.WorkPointY;
							braceYi1 = 0;
						}
						else
							braceYi1 = (braceXi1 - component.WorkPointX) * sin / cos + component.WorkPointY - Math.Sign(sin) * (attachmentlength / 2 + 1);
					}
					break;
			}

			if (component.ShapeType == EShapeType.SingleAngle || component.ShapeType == EShapeType.DoubleAngle)
			{
				dcol = component.AngleColumnSide + component.BraceConnect.Brace.BraceWeld.Weld1sz;
				dbeam = component.AngleBeamSide + component.BraceConnect.Brace.BraceWeld.Weld1sz;
				d = component.Shape.d / 2 + component.BraceConnect.Brace.BraceWeld.Weld1sz;
			}
			else if (component.ShapeType == EShapeType.WTSection)
			{
				d = component.Shape.bf / 2;
				dcol = d;
				dbeam = d;
			}
			else
			{
				if (!component.BraceConnect.ClawAngles.DoNotConnectFlanges)
					d = component.Shape.d / 2 + component.BraceConnect.Brace.BraceWeld.Weld1sz + (component.BraceConnect.ClawAngles.ShortLeg + component.BraceConnect.ClawAngles.LongLeg - component.BraceConnect.ClawAngles.LengthOfOSL);
				else
				{
					d1 = component.Shape.d / 2 + component.BraceConnect.Brace.BraceWeld.Weld1sz;
					d2 = component.BraceConnect.SplicePlates.Width / 2;
					d = Math.Max(d1, d2);
				}
				dcol = d;
				dbeam = d;
			}

			if (component.EndSetback < 0 && !(component.ShapeType == EShapeType.HollowSteelSection && component.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted))
			{
				lg = -component.EndSetback;
				lp = 0;
			}
			else if (component.ShapeType == EShapeType.HollowSteelSection && component.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
			{
				lp = component.BraceConnect.SplicePlates.LengthG;
				lg = 0;
			}
			else
			{
				lg = component.BraceConnect.ClawAngles.LengthG;
				lp = component.BraceConnect.SplicePlates.LengthG;
				if (lg == 0)
					lg = lp;
				if (lp == 0)
					lp = lg;
			}
			d1 = lg * absCos + dcol * absSin;
			d2 = lp * absCos + component.BraceConnect.SplicePlates.Width / 2 * absSin;
			lph = d2 - d1;
			if (lph < 0)
				lph = 0;
			else
				lph = lph * Math.Sign(cos);
			x_A_to_CL_BraceEnd = d1 * Math.Sign(cos) + lph;
			d1 = lg * absSin + dbeam * absCos;
			d2 = lp * absSin + component.BraceConnect.SplicePlates.Width / 2 * absCos;
			lpv = d2 - d1;
			if (lpv < 0)
				lpv = 0;
			else
				lpv = lpv * Math.Sign(sin);

			if (component.GussetToColumnConnection == EBraceConnectionTypes.DirectlyWelded)
			{
				V = 2 * (d / cos - component.EndSetback * absSin);
				braceYi1 = y0 + (braceXi1 - x0) * sin / cos - V / 2;
			}
			g = Math.Sign(cos) * component.BraceConnect.Gusset.ColumnSideSetback;
			x1 = braceXi1;
			y1 = braceYi1;
			if (!component.BraceConnect.ClawAngles.DoNotConnectFlanges)
			{
				if (beam.IsActive && position != EPosition.NoConnection)
					ch = Math.Sign(cos) * ConstNum.ONE_INCH;
				else
					ch = Math.Sign(cos) * (ConstNum.ONE_INCH + (attachmentlength / 2 - Math.Abs(osl) * Math.Abs(sin / cos)) * Math.Abs(sin)) * Math.Abs(cos) - Math.Sign(cos) * (component.Shape.d / 2 + component.BraceConnect.ClawAngles.LongLeg + component.BraceConnect.ClawAngles.ShortLeg - component.BraceConnect.ClawAngles.LengthOfOSL) * Math.Abs(sin);
			}
			else
			{
				if (beam.IsActive && position != EPosition.NoConnection)
					ch = Math.Sign(cos) * ConstNum.ONE_INCH;
				else
					ch = Math.Sign(cos) * (ConstNum.ONE_INCH + (attachmentlength / 2 - Math.Abs(osl) * Math.Abs(sin / cos)) * Math.Abs(sin)) * Math.Abs(cos) - Math.Sign(cos) * dcol * Math.Abs(sin); //  ElProp(m).d / 2 * Abs(S)
			}

			if (Math.Abs(ch) > Math.Abs(osl) - component.BraceConnect.Gusset.ColumnSideSetback)
				ch = Math.Sign(ch) * (Math.Abs(osl) - component.BraceConnect.Gusset.ColumnSideSetback);

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase && component.BraceConnect.Gusset.DontConnectBeam)
				cv = Math.Sign(sin) * ConstNum.ONE_INCH + component.BraceConnect.BasePlate.CornerClip;
			else
				cv = Math.Sign(sin) * ConstNum.ONE_INCH;
			eb = Math.Abs(y1 - y0);
			ec = Math.Abs(x1 - x0 - g);
			if (!component.BraceConnect.Gusset.DontConnectBeam)
			{
				if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic)
				{
					PullBack = CommonDataStatic.Preferences.TransferForce;
					if (CommonDataStatic.Preferences.InputForceType == EPrefsInputForce.TransferForce)
						PullBack = PullBack * component.BraceConnect.Gusset.Thickness;

					if (PullBack <= 0)
						PullBack = 2 * component.BraceConnect.Gusset.Thickness;
				}
				else
					PullBack = 0;
				if (position == EPosition.NoConnection)
					xa = x1 + lph + cos * PullBack;
				else
					xa = x1 - g + osl + ch + lph + cos * PullBack;
			}
			else if (component.GussetToColumnConnection == EBraceConnectionTypes.DirectlyWelded && component.KBrace)
				xa = x0 + (component.BraceConnect.Brace.DistanceFromWP - component.EndSetback) * cos - x_A_to_CL_BraceEnd;
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase)
				xa = x1 - g + osl + ch + lph; // + c * PullBack

			if (component.ShapeType == EShapeType.WideFlange)
			{
				if (!component.BraceConnect.ClawAngles.DoNotConnectFlanges)
				{
					if (CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].ShapeType == EShapeType.WideFlange &&
					    CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].WebOrientation == EWebOrientation.InPlane)
						xa = Math.Sign(cos) * (CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.d / 2 + osl + ConstNum.ONE_INCH);
					else if (CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].ShapeType == EShapeType.WideFlange &&
					         CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].WebOrientation == EWebOrientation.OutOfPlane)
						xa = Math.Sign(cos) * (Math.Max(CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.bf / 2, osl) + ConstNum.ONE_INCH);
				}
				else
				{
					if (CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].ShapeType == EShapeType.WideFlange &&
					    CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].WebOrientation == EWebOrientation.InPlane)
						xa = Math.Sign(cos) * (CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.d / 2 + osl + ConstNum.ONE_INCH);
					else if (CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].ShapeType == EShapeType.WideFlange &&
					         CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].WebOrientation == EWebOrientation.OutOfPlane)
						xa = Math.Sign(cos) * (Math.Max(CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.bf / 2, osl) + ConstNum.ONE_INCH);
				}
			}
			else if (component.ShapeType == EShapeType.WTSection)
			{
				//xa = BRACE1.x[301 + 8 * (m - 3)];
			}
			else if (component.ShapeType == EShapeType.SingleAngle || component.ShapeType == EShapeType.DoubleAngle)
			{
				//	if (BRACE1.BraceOslOnBeamSide[m] != 0)
				//		xa = BRACE1.x[301 + 8 * (m - 3)] - component.Shape.d * absSin * Math.Sign(cos); // + BraceWeld(m).Weld1sz
				//	else
				//		xa = BRACE1.x[301 + 8 * (m - 3)];
			}
			else if (component.ShapeType == EShapeType.HollowSteelSection && component.BraceToGussetWeldedOrBolted != EConnectionStyle.Bolted)
			{
				//xa = BRACE1.x[301 + 8 * (m - 3)];
			}
			else if (component.ShapeType == EShapeType.HollowSteelSection && component.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
			{
				//xa = BRACE1.x[301 + 8 * (m - 3)] + component.BraceConnect.SplicePlates.LengthG * cos;
			}
			else if (component.ShapeType == EShapeType.SingleChannel || component.ShapeType == EShapeType.DoubleChannel)
			{
				//xa = BRACE1.x[301 + 8 * (m - 3)];
			}

			if (component.ShapeType == EShapeType.WideFlange)
			{
				xaTest = component.BraceConnect.Brace.DistanceFromWP - component.EndSetback - Math.Max(component.BraceConnect.ClawAngles.LengthG, component.BraceConnect.SplicePlates.LengthG);
				xat = Math.Sign(cos) * (xaTest * cos * Math.Sign(cos) - dcol * sin * Math.Sign(sin));
				if (Math.Abs(xat) < Math.Abs(xa))
				{
					// BraceSetBack(m) = Gusset(m).BraceGap + Max(ClawAngle(m).LengthG, WebPlate(m).LengthG) + Abs(xa / c)
				}
				xa = xat;
				component.BraceConnect.Gusset.Length = component.BraceConnect.Brace.DistanceFromWP - component.EndSetback;
			}

			if (CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].WebOrientation == EWebOrientation.OutOfPlane)
			{
				if (Math.Abs(xa) < CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.bf / 2)
				{
					// xa = Sgn(xa) * (ElProp(0).BF / 2 + BirInchN)
				}
			}
			Xb = xa + (dcol + dbeam) * absSin * Math.Sign(cos);
			xD = xa + dcol * absSin * Math.Sign(cos);
			yD = sin / cos * (xD - x0) + y0;
			ya = yD + dcol * absCos * Math.Sign(sin);
			Yb = ya - (dcol + dbeam) * absCos * Math.Sign(sin);
			if (Math.Abs(Yb) < Math.Abs(y1 + cv + lpv) && (!component.BraceConnect.Gusset.DontConnectBeam || CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase))
			{
				Yb = y1 + cv + lpv;
				ya = Yb + (dcol + dbeam) * absCos * Math.Sign(sin);
				yD = ya - dcol * absCos * Math.Sign(sin);
				xD = cos / sin * (yD - y0) + x0;
				xa = xD - dcol * absSin * Math.Sign(cos);
				Xb = xa + (dcol + dbeam) * absSin * Math.Sign(cos);
			}
			if (!component.BraceConnect.ClawAngles.DoNotConnectFlanges)
			{
				xe = xa + lg * cos;
				ye = ya + lg * sin;
				xf = Xb + lg * cos;
				yf = Yb + lg * sin;
			}
			else
			{
				switch (memberType)
				{
					case EMemberType.UpperRight:
					case EMemberType.LowerLeft:
						xe = xa + lg * cos - ConstNum.ONEANDHALF_INCHES * sin;
						ye = ya + lg * sin + ConstNum.ONEANDHALF_INCHES * cos;
						xf = Xb + lg * cos + ConstNum.ONEANDHALF_INCHES * sin;
						yf = Yb + lg * sin - ConstNum.ONEANDHALF_INCHES * cos;
						break;
					case EMemberType.LowerRight:
					case EMemberType.UpperLeft:
						xe = xa + lg * cos + ConstNum.ONEANDHALF_INCHES * sin;
						ye = ya + lg * sin - ConstNum.ONEANDHALF_INCHES * cos;
						xf = Xb + lg * cos - ConstNum.ONEANDHALF_INCHES * sin;
						yf = Yb + lg * sin + ConstNum.ONEANDHALF_INCHES * cos;
						break;
				}
			}

			yK = y1;
			xK = -sin / cos * (y1 - ye) + xe;
			xL = x1 - g;
			yL = -cos / sin * (xL - xe) + ye;

			//j1 = 81 + (m - 1) * 8;
			j2 = j1 + 3;
			if (memberType == EMemberType.UpperRight || memberType == EMemberType.UpperLeft)
			{
				//yTop = BRACE1.y[j2];
				//yBottom = BRACE1.y[j1];
			}
			else
			{
				//yTop = BRACE1.y[j1];
				//yBottom = BRACE1.y[j2];
			}
			V = Math.Abs(ye - y1);
			edg = component.BoltBrace.EdgeDistLongDir;
			la = attachmentlength;
			switch (component.GussetToColumnConnection)
			{
				case EBraceConnectionTypes.ClipAngle:
					if (V < la + 2 * edg)
						V = la + 2 * edg;
					break;
				case EBraceConnectionTypes.SinglePlate:
					if (V < la - edg + component.WinConnect.ShearWebPlate.BraceDistanceToFirstBolt)
					{
						V = la - edg + component.WinConnect.ShearWebPlate.BraceDistanceToFirstBolt;
						yep = ye;
						ye = y1 + Math.Sign(sin) * V;
						yechange = ye - yep;
						xe = xe - yechange * (sin / cos);
						if (Math.Abs(xe) < Th + component.WinConnect.ShearWebPlate.Width)
						{
							int sign = 1;
							if (memberType == EMemberType.UpperLeft || memberType == EMemberType.LowerLeft)
								sign = -1;

							xeb = xe;
							xe = sign * (Th + component.WinConnect.ShearWebPlate.Width);
							xf = xf + Math.Sign(xf) * Math.Abs(xe - xeb);
							yf = yf + Math.Sign(yf) * Math.Abs(xe - xeb) * cos / sin;
							yK = y1;
							SmallMethodsDesign.Intersect(-cos / sin, xf, yf, 0, 0, y1, ref xK, ref yK);
							if (!component.BraceConnect.Brace.DistanceFromWP_User)
								component.BraceConnect.Brace.DistanceFromWP = component.BraceConnect.Brace.DistanceFromWP + Math.Sign(component.BraceConnect.Brace.DistanceFromWP) * Math.Abs((xe - xeb) / cos);
						}
					}
					break;
				case EBraceConnectionTypes.FabricatedTee:
					if (V < la - edg + component.BraceConnect.FabricatedTee.FirstBoltDistance)
					{
						V = la - edg + component.BraceConnect.FabricatedTee.FirstBoltDistance;
						yep = ye;
						ye = y1 + Math.Sign(sin) * V;
						yechange = ye - yep;
						xe = xe - yechange * (sin / cos);
						if (Math.Abs(xe) < Th + component.BraceConnect.FabricatedTee.D)
						{
							int sign = 1;
							if (memberType == EMemberType.UpperLeft || memberType == EMemberType.LowerLeft)
								sign = -1;

							xeb = xe;
							xe = sign * (Th + component.BraceConnect.FabricatedTee.D);
							xf = xf + Math.Sign(xf) * Math.Abs(xe - xeb);
							yf = yf + Math.Sign(yf) * Math.Abs(xe - xeb) * cos / sin;
							yK = y1;
							SmallMethodsDesign.Intersect(-cos / sin, xf, yf, 0, 0, y1, ref xK, ref yK);
							if (!component.BraceConnect.Brace.DistanceFromWP_User)
								component.BraceConnect.Brace.DistanceFromWP = component.BraceConnect.Brace.DistanceFromWP + Math.Sign(component.BraceConnect.Brace.DistanceFromWP) * Math.Abs((xe - xeb) / cos);
						}
					}
					break;
				case EBraceConnectionTypes.EndPlate:
					if (V < la - edg + component.WinConnect.ShearEndPlate.TOBtoFirstBolt)
						V = la - edg + component.WinConnect.ShearEndPlate.TOBtoFirstBolt;
					break;
			}

			switch (position)
			{
				case EPosition.Center:
					Beta = V / 2;
					break;
				case EPosition.Top:
				case EPosition.Bottom:
					Beta = V - la / 2 - edg;
					break;
				case EPosition.MatchOtherSideBolts:
					Beta = Math.Abs((yTop + yBottom) * 0.5 - y1);
					break;
				case EPosition.NoConnection:
					Beta = 0;
					V = Math.Max(0, Math.Sign(sin) * (Math.Tan((component.Angle - 10 * Math.Sign(sin) * Math.Sign(cos)) * ConstNum.RADIAN) * (x1 - xa) + ya - y1)); // (((yE - yA) / (xE - xA) * (x1 - xA) + yA) - y1))
					break;
			}

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase && component.GussetToColumnConnection == EBraceConnectionTypes.DirectlyWelded)
				Beta = component.BraceConnect.BasePlate.CornerClip + V / 2;

			if (!component.BraceConnect.Gusset.DontConnectBeam)
			{
				alfa = (Beta + eb) * absCos / absSin - ec;
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase)
				{
					gbg = component.BraceConnect.BasePlate.CornerClip;
					H = Math.Sign(cos) * (2 * alfa - gbg);
					if (Math.Abs(H) < 2 * gbg)
						H = Math.Sign(H) * 2 * gbg;
				}
				else
				{
					gbg = Math.Max(component.BraceConnect.Gusset.ColumnSideSetback, beam.EndSetback + beam.WinConnect.Beam.TCopeL);
					H = Math.Sign(cos) * (2 * (alfa - gbg) + gbg - component.BraceConnect.Gusset.ColumnSideSetback);
					if (Math.Abs(H) < 2 * gbg)
						H = Math.Sign(H) * 2 * gbg;
				}
			}
			else
			{
				alfa = 0;
				H = Math.Abs(Xb - x1);
				if (V < attachmentlength + boltRange && component.GussetToColumnConnection != EBraceConnectionTypes.DirectlyWelded)
					V = attachmentlength + boltRange;
			}
			xG = x1 + Math.Sign(x1) * Math.Abs(H);
			if (Math.Abs(xG) < Math.Abs(Xb))
				xG = Xb;

			yG = y1;
			xH = x1;
			yH = y1 + Math.Sign(sin) * V;

			xN = xe;
			yN = ye;
			if (component.KBrace)
			{
				yH = yN;
				V = Math.Abs(yH - y1);
				xG = xf;
				H = Math.Abs(xG - x1);
				switch (memberType)
				{
					case EMemberType.UpperRight:
					case EMemberType.UpperLeft:
						xG = xf;
						yf = -cos / sin * (xf - xe) + ye;
						break;
					case EMemberType.LowerRight:
					case EMemberType.LowerLeft:
						xG = xf;
						yf = -cos / sin * (xf - xe) + ye;
						break;
				}
			}
			else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
			{
				yH = yN;
				V = Math.Abs(yH - y1);
				xG = xf;
				H = Math.Abs(xG - x1);
				switch (memberType)
				{
					case EMemberType.UpperRight:
					case EMemberType.UpperLeft:
						xG = xf;
						yf = -cos / sin * (xf - xe) + ye;
						break;
					case EMemberType.LowerRight:
					case EMemberType.LowerLeft:
						xG = xf;
						yf = -cos / sin * (xf - xe) + ye;
						break;
				}

			}

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase && component.BraceConnect.Gusset.DontConnectBeam)
				xG = x1 + Math.Sign(cos) * 2;
			if (component.KneeBrace)
			{
				yH = yN;
				V = 2 * Math.Abs(yH - ((x1 - x0) * sin / cos + y0));
			}

			AxG = Math.Abs(xG);
			AxF = Math.Abs(xf);
			AxK = Math.Abs(xK);
			AxB = Math.Abs(Xb);

			if (!component.BraceConnect.Gusset.DontConnectBeam)
			{
				PullBack = CommonDataStatic.Preferences.TransferForce;
				if (CommonDataStatic.Preferences.InputForceType == EPrefsInputForce.TransferForce)
					PullBack = PullBack * component.BraceConnect.Gusset.Thickness;
				if (component.ShapeType != EShapeType.WideFlange)
				{
					if (component.ShapeType == EShapeType.HollowSteelSection && component.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
					{
						sb = component.BraceConnect.Brace.DistanceFromWP - (component.BraceConnect.SplicePlates.LengthG - component.BraceConnect.SplicePlates.Bolt.EdgeDistLongDir) - PullBack - component.EndSetback;
						xGmaxForSeismic = Math.Abs(sin / cos * (y0 + sb * sin - y1) + x0 + sb * cos);
					}
					else
					{
						sb = component.BraceConnect.Brace.DistanceFromWP - PullBack;
						xGmaxForSeismic = Math.Abs(sin / cos * (y0 + sb * sin - y1) + x0 + sb * cos);
					}
				}
				else
				{
					sb = component.BraceConnect.Brace.DistanceFromWP - Math.Max(lg, lp) - PullBack - component.EndSetback;
					xGmaxForSeismic = Math.Abs(sin / cos * (y0 + sb * sin - y1) + x0 + sb * cos);
				}

				if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic)
				{
					if (AxG > xGmaxForSeismic)
					{
						AxG = xGmaxForSeismic;
						xG = AxG * Math.Sign(cos);
					}
				}

				if (AxG >= AxF && AxG <= AxK)
				{
					xm = xG;
					ym = -cos / sin * (xG - xe) + ye;
				}
				else if (AxG < AxF && AxG >= AxB)
				{
					xm = xf;
					ym = yf;
				}
				else if (AxG < AxB)
				{
					xm = xf;
					ym = yf;
				}
				else if (AxG > AxK)
				{
					xG = xK;
					xm = xK;
					ym = yK;
				}
				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam && CommonDataStatic.Preferences.Seismic != ESeismic.Seismic)
					xG = xm;
			}
			else
			{
				xm = xf;
				ym = yf;
			}
			if (!component.BraceConnect.Gusset.DontConnectBeam || beam.IsActive || component.KBrace || CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase)
			{
				sc = Math.Sign(sin) * Math.Sign(cos);
				braceXi1Plus1 = xG;
				braceYi1Plus1 = yG;

				braceXi1Plus2 = component.BraceConnect.Gusset.Length * cos + dbeam * sin * sc + x0; // xm
				braceYi1Plus2 = component.BraceConnect.Gusset.Length * sin - dbeam * cos * sc + y0; // ym

				braceXi1Plus4 = xH;
				braceYi1Plus4 = yH;

				braceXi1Plus3 = component.BraceConnect.Gusset.Length * cos - dcol * sin * sc + x0; // xn
				braceYi1Plus3 = component.BraceConnect.Gusset.Length * sin + dcol * cos * sc + y0; // yn

				if (component.WinConnect.ShearClipAngle.Position == EPosition.NoConnection)
					SmallMethodsDesign.Intersect(0, braceXi1Plus4, braceYi1Plus4, -cos / sin, braceXi1Plus3, braceYi1Plus3, ref xx1, ref yy1);

				if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceToColumnBase && component.BraceConnect.Gusset.DontConnectBeam)
				{
					braceYi1Plus1 = braceYi1;
					SmallMethodsDesign.Intersect(0, braceXi1, braceYi1, (yG - ym) / (xG - xm), xG, yG, ref braceXi1Plus1, ref ydumy);
				}
			}
			else
			{
				braceYi1 = (braceXi1 - x0) * Math.Tan(component.Angle * ConstNum.RADIAN) - Math.Sign(sin) * V / 2 + y0;
				braceXi1Plus1 = xG;
				braceYi1Plus1 = braceYi1;
				y1 = braceYi1;
				if ((V - attachmentlength) / 2 / (Math.Abs(osl) + 1 - component.BraceConnect.Gusset.ColumnSideSetback) < Math.Abs(Math.Sign(cos) * (yf - y1) / (xf - x1)))
				{
					slp1 = Math.Sign((sin / cos)) * ((V - attachmentlength) / 2) / (Math.Abs(osl) + 1 - component.BraceConnect.Gusset.ColumnSideSetback);
					slp2 = (yf - ye) / (xf - xe);
					SmallMethodsDesign.Intersect(slp1, braceXi1, braceYi1, slp2, xf, yf, ref xx, ref yy);
					xm = xx;
					ym = yy;
				}
				braceXi1Plus2 = xm;
				braceYi1Plus2 = ym;

				braceXi1Plus4 = xH;
				yH = braceYi1 + Math.Sign(sin) * V;
				braceYi1Plus4 = yH;

				braceXi1Plus3 = xN;
				braceYi1Plus3 = yN;
			}

			yG = braceYi1;

			if (!component.BraceConnect.Gusset.BeamSide_User)
				component.BraceConnect.Gusset.BeamSide = Math.Abs(xG - x1);
			if (!component.BraceConnect.Gusset.ColumnSide_User)
				component.BraceConnect.Gusset.ColumnSide = V;
			if (!component.BraceConnect.Gusset.ColumnSideFreeEdgeY_User)
				component.BraceConnect.Gusset.ColumnSideFreeEdgeY = Math.Abs(ym - yG);
			if (!component.BraceConnect.Gusset.ColumnSideFreeEdgeX_User)
				component.BraceConnect.Gusset.ColumnSideFreeEdgeX = Math.Abs(xm - xG);
			if (!component.BraceConnect.Gusset.BeamSideFreeEdgeY_User)
				component.BraceConnect.Gusset.BeamSideFreeEdgeY = Math.Abs(yN - yH);
			if (!component.BraceConnect.Gusset.BeamSideFreeEdgeX_User)
				component.BraceConnect.Gusset.BeamSideFreeEdgeX = Math.Abs(xN - xH);

			if (!component.BraceConnect.Brace.DistanceFromWP_User)
			{
				if (component.ShapeType == EShapeType.WideFlange)
					component.BraceConnect.Brace.DistanceFromWP = lg + component.EndSetback + Math.Sqrt(Math.Pow(xD - x0, 2) + Math.Pow(yD - y0, 2));
				else if (component.ShapeType == EShapeType.HollowSteelSection && component.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
					component.BraceConnect.Brace.DistanceFromWP = component.EndSetback + Math.Sqrt(Math.Pow(xD - x0, 2) + Math.Pow(yD - y0, 2));
			}

			component.BraceConnect.Gusset.Length = component.BraceConnect.Brace.DistanceFromWP - component.EndSetback;
			component.BraceConnect.Gusset.BraceSide = Math.Sqrt(Math.Pow(xm - xN, 2) + Math.Pow(ym - yN, 2));

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BraceToColumn:
					if (!component.KBrace || !component.KneeBrace)
						beamType = "Beam";
					else
						beamType = string.Empty;
					break;
				case EJointConfiguration.BraceVToBeam:
					beamType = "Beam";
					break;
				case EJointConfiguration.BraceToColumnBase:
					beamType = "Base Plate";
					break;
			}
			
			Reporting.AddHeader(CommonDataStatic.CommonLists.CompleteMemberList[MiscMethods.BeamComponentFromBrace(memberType)] + " Gusset Dimensions:");
			Reporting.AddLine("Column Side (Lgc) = " + component.BraceConnect.Gusset.ColumnSide + " " + ConstUnit.Length);
			Reporting.AddLine(beamType + " Side (Lgb) = " + component.BraceConnect.Gusset.BeamSide + " " + ConstUnit.Length);
			Reporting.AddLine(beamType + " Side Free Edge (Lvfx) = " + component.BraceConnect.Gusset.BeamSideFreeEdgeX + ConstUnit.Length);
			Reporting.AddLine(beamType + " Side Free Edge (Lvfy) = " + component.BraceConnect.Gusset.BeamSideFreeEdgeY + ConstUnit.Length);
			Reporting.AddLine("Column Side Free Edge (Lhfx) = " + component.BraceConnect.Gusset.ColumnSideFreeEdgeX + ConstUnit.Length);
			Reporting.AddLine("Column Side Free Edge (Lhfy) = " + component.BraceConnect.Gusset.ColumnSideFreeEdgeY + ConstUnit.Length);
		}
	}
}