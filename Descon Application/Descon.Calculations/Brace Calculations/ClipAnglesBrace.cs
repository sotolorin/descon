using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
    public class ClipAnglesBrace
    {
	    public static void DesignClipAngles(EMemberType memberType, int numberOfBolts, double ball)
	    {
		    double R = 0;
		    double V = 0;
		    double H_Comp = 0;
		    double H_Ten = 0;
		    double FiRn = 0;
		    double Ag = 0;
		    double maxweld = 0;
		    double minweld = 0;
		    double TensionCap = 0;
		    double Ta = 0;
		    double weldcap = 0;
		    double CALengthWeld = 0;
		    double CALengthBolt = 0;
		    int previousnumberofbolts = 0;
		    double edg = 0;
		    double b = 0;
		    double Gage = 0;
		    double AnglGage2 = 0;
		    double AnglGage1 = 0;
		    double ColumnGage = 0;
		    double thick = 0;
		    double osl = 0;
		    double fsz = 0;
		    double BearingCap = 0;
		    double BlockShearStrength = 0;
		    double Ant = 0;
		    double Agt = 0;
		    double GrossShearCap = 0;
		    double Agv = 0;
		    double NetShearCap = 0;
		    double Anv = 0;
		    double t = 0;
		    double Fbs = 0;
		    double Fbe = 0;
		    bool AllisOkey = false;
		    string gagecheck = string.Empty;
		    double columngagemax = 0;
		    double columngagemin = 0;
		    double GageCol2 = 0;
		    double GageCol1 = 0;
		    double re = 0;
		    double maxbolts = 0;
		    double tmax = 0;
		    double SumY = 0;
		    double Sum = 0;
		    int N = 0;
		    int minbolt = 0;
		    double Reduction = 0;
		    double ClipAngleMax = 0;
		    double ClipAngleMin = 0;
		    double twORtg = 0;
		    double MaxH = 0;

		    var component = CommonDataStatic.DetailDataDict[memberType];
		    var clipAngle = component.WinConnect.ShearClipAngle;
		    var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

		    DetailData otherSide;

		    if (memberType == EMemberType.RightBeam) 
				otherSide = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
		    else if (memberType == EMemberType.LeftBeam) 
				otherSide = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
		    else if (memberType == EMemberType.UpperRight) 
				otherSide = CommonDataStatic.DetailDataDict[EMemberType.UpperLeft];
		    else if (memberType == EMemberType.UpperLeft) 
				otherSide = CommonDataStatic.DetailDataDict[EMemberType.UpperRight];
		    else if (memberType == EMemberType.LowerRight) 
				otherSide = CommonDataStatic.DetailDataDict[EMemberType.LowerLeft];
		    else if (memberType == EMemberType.LowerLeft)
				otherSide = CommonDataStatic.DetailDataDict[EMemberType.LowerRight];
		    else 
				otherSide = column;

		    MaxH = Math.Max(component.AxialTension, component.AxialCompression);
		    component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts = numberOfBolts;
		    if (clipAngle.Position == EPosition.NoConnection)
			    return;

		    Reporting.AddHeader(String.Format("Clip Angles - Component: {0}", CommonDataStatic.CommonLists.CompleteMemberList[memberType]));

		    switch (memberType)
		    {
			    case EMemberType.RightBeam:
			    case EMemberType.LeftBeam:
				    twORtg = component.Shape.tw;
				    break;
			    case EMemberType.UpperRight:
			    case EMemberType.LowerRight:
			    case EMemberType.UpperLeft:
			    case EMemberType.LowerLeft:
				    twORtg = component.BraceConnect.Gusset.Thickness;
				    break;
		    }

		    switch (memberType)
		    {
			    case EMemberType.RightBeam:
			    case EMemberType.LeftBeam:
				    ClipAngleMin = component.Shape.d / 2;
				    ClipAngleMax = component.Shape.t + SmallMethodsDesign.Encroachment(memberType);
				    Reduction = 0;
				    break;
			    default:
				    if (!component.BraceConnect.Gusset.DontConnectBeam)
				    {
					    ClipAngleMin = component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir + 2 * component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistLongDir;
					    ClipAngleMax = component.BraceConnect.Gusset.VerticalForceColumn - 2;
				    }
				    else
				    {
					    ClipAngleMin = 6 * ConstNum.ONE_INCH;
					    ClipAngleMax = 100 * ConstNum.ONE_INCH;
				    }
					minbolt = Math.Max(2, ((int)Math.Ceiling((ClipAngleMin - 2 * component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistLongDir) / component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir)) + 1);
				    if (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts < minbolt)
				    {
					    component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts = minbolt;
					    numberOfBolts = minbolt;
				    }
				    N = component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts;
				    Sum = 0;
				    SumY = 0;
				    for (int i = 1; i <= N / 2; i++)
				    {
					    Sum = (Sum + Math.Pow((N - 1.0) / 2.0 - (i - 1.0), 2));
					    SumY = SumY + ((N - 1.0) / 2.0 - (i - 1.0));
				    }
				    tmax = (N - 1) * Math.Abs(component.BraceConnect.Gusset.Mc) / (4 * component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir * Sum);
				    Reduction = tmax * SumY * 2 / ((N - 1) * component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir);
				    break;
		    }
		    maxbolts = Math.Floor((ClipAngleMax - 2 * component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistLongDir) / component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir) + 1;

		    re = component.WinConnect.ShearClipAngle.BoltOnGusset.MinEdgeRolled;
		    if (column.WebOrientation == EWebOrientation.InPlane)
		    {
			    GageCol1 = 2 * (column.Shape.k1 + component.WinConnect.ShearClipAngle.BoltOnGusset.BoltSize);
			    GageCol2 = column.Shape.tw + 2 * (component.WinConnect.ShearClipAngle.BoltOnGusset.BoltSize + ConstNum.HALF_INCH);

			    if (GageCol1 < GageCol2)
				    columngagemin = GageCol2;
			    else
				    columngagemin = GageCol1;
			    columngagemax = column.Shape.bf - 2 * re;

			    if (columngagemin <= column.GageOnFlange && columngagemax >= column.GageOnFlange)
			    {
				    columngagemin = column.GageOnFlange;
				    columngagemax = column.GageOnFlange;
				    gagecheck = "";
			    }
			    else
			    {
				    columngagemin = column.GageOnFlange;
				    columngagemax = column.GageOnFlange;
			    }
		    }
		    else
		    {
			    if (clipAngle.GussetSideGage != 0.0)
			    {
				    columngagemin = clipAngle.GussetSideGage;
				    columngagemax = clipAngle.GussetSideGage;
				    column.Shape.g2 = columngagemin;
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
						    gagecheck = "";
					    }
					    else
					    {
						    gagecheck = "Check Column Gage: Should be less then " + columngagemax + ConstUnit.Length;
						    columngagemin = column.Shape.g2;
						    columngagemax = column.Shape.g2;
					    }
				    }
				    clipAngle.GussetSideGage = column.Shape.g2;
			    }
		    }
		    AllisOkey = false;
		    clipAngle.Length = (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 1) * component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir + 2 * component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistLongDir;
		    Fbe = CommonCalculations.EdgeBearing(component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistLongDir,
			    ((int) clipAngle.BoltOnColumn.HoleLength), component.WinConnect.ShearClipAngle.BoltOnGusset.BoltSize,
				clipAngle.Material.Fu, component.WinConnect.ShearClipAngle.BoltOnGusset.HoleType, false);

			Fbs = CommonCalculations.SpacingBearing(component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir,
			    clipAngle.BoltOnColumn.HoleLength, component.WinConnect.ShearClipAngle.BoltOnGusset.BoltSize, component.WinConnect.ShearClipAngle.BoltOnGusset.HoleType,
				clipAngle.Material.Fu, false);

		    do
		    {
				foreach (var shape in CommonDataStatic.ShapesSingleAngle)
			    {
				    if (!clipAngle.SizeName_User) clipAngle.Size = shape.Value;

				    t = clipAngle.Thickness;

				    if (clipAngle.AnglesBoltedToGusset)
					    SmallMethodsDesign.ClipAnglesGussetSide(memberType, false);

				    // shear rupture
				    Anv = (clipAngle.Length -
				           component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts * (clipAngle.BoltOnColumn.HoleLength + ConstNum.SIXTEENTH_INCH)) *
				          t;
					NetShearCap = 2 * ConstNum.FIOMEGA0_75N * Anv * 0.6 * clipAngle.Material.Fu;

				    // shear yielding
				    Agv = clipAngle.Length * t;
				    GrossShearCap = ConstNum.FIOMEGA1_0N * 2 * Agv * 0.6 * clipAngle.Material.Fy;

				    // block shear rupture
				    Agv = (clipAngle.Length - component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistLongDir) * t;
				    Anv = Agv -
				          (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 0.5) *
				          (clipAngle.BoltOnColumn.HoleLength + ConstNum.SIXTEENTH_INCH) * t;
				    Agt = component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistTransvDir * t;
				    Ant = Agt - 0.5 * (clipAngle.BoltOnColumn.HoleWidth + ConstNum.SIXTEENTH_INCH) * t;

					BlockShearStrength = ConstNum.FIOMEGA0_75N * 2 * (0.6 * Math.Min(clipAngle.Material.Fu * Anv, clipAngle.Material.Fy * Agv) + 1 * clipAngle.Material.Fu * Ant);

				    BearingCap = 2 * (Fbe + Fbs * (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 1)) * t;

				    AllisOkey = ConstNum.TOLERANCE * NetShearCap >= Math.Abs(clipAngle.ForceY);
				    AllisOkey = AllisOkey &&
				                (ConstNum.TOLERANCE * GrossShearCap >= Math.Abs(clipAngle.ForceY));
				    AllisOkey = AllisOkey &&
				                (ConstNum.TOLERANCE * BlockShearStrength >= Math.Abs(clipAngle.ForceY));
				    AllisOkey = AllisOkey &&
				                (ConstNum.TOLERANCE * BearingCap >= Math.Abs(clipAngle.ForceY));

				    fsz = clipAngle.Fillet;
				    osl = clipAngle.LengthOfOSL;

				    thick = MiscMethods.IsBrace(memberType) ? component.BraceConnect.Gusset.Thickness : component.Shape.tw;

				    if (column.WebOrientation == EWebOrientation.InPlane)
				    {
					    if (clipAngle.AnglesBoltedToGusset)
						    SmallMethodsDesign.AngleBeamSideGage(memberType, EBoltedLeg.Both, component.WinConnect.ShearClipAngle.BoltOnGusset.BoltSize, thick, ref ColumnGage);
					    else
						    SmallMethodsDesign.AngleBeamSideGage(memberType, EBoltedLeg.Support, component.WinConnect.ShearClipAngle.BoltOnGusset.BoltSize, thick, ref ColumnGage);

					    if (column.GageOnFlange != 0)
						    Gage = column.GageOnFlange;
					    else
						    Gage = ColumnGage;

					    b = (Gage - thick) / 2 - t / 2;
					    edg = Math.Min(osl - t / 2 - b, (column.Shape.bf - Gage) / 2);
					    if(!clipAngle.GussetSideGage_User) clipAngle.GussetSideGage = NumberFun.Round(Gage, 4);
				    }
				    else
				    {
					    //  web is out of plane
					    AnglGage1 = thick + 2 * (t + component.WinConnect.ShearClipAngle.BoltOnGusset.BoltSize + 0.5);
					    AnglGage2 = thick + 2 * (component.WinConnect.ShearClipAngle.BoltOnGusset.BoltSize + fsz);
					    Gage = Math.Max(AnglGage1, AnglGage2);

					    switch (memberType)
					    {
						    case EMemberType.RightBeam:
						    case EMemberType.UpperRight:
						    case EMemberType.LowerRight:
                                if (!clipAngle.GussetSideGage_User) clipAngle.GussetSideGage = NumberFun.Round(Math.Max(columngagemin, Gage), 16);
							    break;
						    case EMemberType.LeftBeam:
						    case EMemberType.UpperLeft:
						    case EMemberType.LowerLeft:
                                if (!clipAngle.GussetSideGage_User) clipAngle.GussetSideGage = NumberFun.Round(Math.Max(columngagemin, Math.Max(Gage, otherSide.WinConnect.ShearClipAngle.GussetSideGage)), 16);
							    break;
					    }

					    b = (clipAngle.GussetSideGage - thick) / 2 - t / 2;
					    edg = osl - t / 2 - b;
					    if (edg >= re)
					    {
						    gagecheck = string.Empty;
						    column.Shape.g2 = clipAngle.GussetSideGage;
					    }
					    else
						    gagecheck = "Check bolt gage in column web. (NG)";
				    }

				    ClipAngles(memberType, ref CALengthBolt, ref CALengthWeld, ref weldcap);

				    Ta = SmallMethodsDesign.HangerAllowable(memberType, t, ref edg, b, ball, false);
				    TensionCap = 2 * component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts * Ta - Reduction;
				    AllisOkey = AllisOkey && ConstNum.TOLERANCE * TensionCap >= component.AxialTension;

				    //  ClipWelds m, ClipForceX(m), ClipForceY(m), CALengthWeld, ClipAngleMax, WeldCap
				    if (!clipAngle.AnglesBoltedToGusset)
				    {
					    minweld = CommonCalculations.MinimumWeld(clipAngle.Thickness, twORtg);
					    if (clipAngle.Thickness < ConstNum.QUARTER_INCH)
						    maxweld = clipAngle.Thickness;
					    else
						    maxweld = clipAngle.Thickness - ConstNum.SIXTEENTH_INCH;
					    AllisOkey = AllisOkey && minweld <= maxweld &&
					                (weldcap >=
					                 Math.Sqrt(Math.Pow(MaxH, 2) + Math.Pow(clipAngle.ForceY, 2)));
				    }
				    if (clipAngle.AnglesBoltedToGusset)
				    {
					    clipAngle.BoltOnGusset.EdgeDistTransvDir =
						    clipAngle.ShortLeg +
						    clipAngle.LongLeg -
						    clipAngle.LengthOfOSL -
						    clipAngle.GussetSideGage;
				    }

					if (AllisOkey)
						break;
			    }
			    if (!AllisOkey && component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts < maxbolts)
			    {
				    component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts = component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts + 1;
				    numberOfBolts = component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts;
				    clipAngle.Length = (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 1) * component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir + 2 * component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistLongDir;
			    }
			    previousnumberofbolts = component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts;
		    } while (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts > previousnumberofbolts);

		    if (NetShearCap >= Math.Abs(clipAngle.ForceY))
				Reporting.AddLine("NetShearCap " + NetShearCap + " " + ConstUnit.Force + " >= " + Math.Abs(clipAngle.ForceY) + ConstUnit.Force + " (OK)");
		    else
				Reporting.AddLine("NetShearCap " + NetShearCap + " " + ConstUnit.Force + " << " + Math.Abs(clipAngle.ForceY) + ConstUnit.Force + " (NG)");

		    if (GrossShearCap >= Math.Abs(clipAngle.ForceY))
				Reporting.AddLine("GrossShearCap " + GrossShearCap + " " + ConstUnit.Force + " >= " + Math.Abs(clipAngle.ForceY) + ConstUnit.Force + " (OK)");
		    else
				Reporting.AddLine("GrossShearCap " + GrossShearCap + " " + ConstUnit.Force + " << " + Math.Abs(clipAngle.ForceY) + ConstUnit.Force + " (NG)");

		    if (BearingCap >= Math.Abs(clipAngle.ForceY))
				Reporting.AddLine("BearingCap " + BearingCap + " " + ConstUnit.Force + " >= " + Math.Abs(clipAngle.ForceY) + ConstUnit.Force + " (OK)");
		    else
				Reporting.AddLine("BearingCap " + BearingCap + " " + ConstUnit.Force + " << " + Math.Abs(clipAngle.ForceY) + ConstUnit.Force + " (NG)");

		    if (TensionCap >= component.AxialTension)
				Reporting.AddLine("TensionCap " + TensionCap + " " + ConstUnit.Force + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
		    else if (TensionCap >= component.AxialTension)
				Reporting.AddLine("TensionCap " + TensionCap + " " + ConstUnit.Force + " <= " + component.AxialTension + ConstUnit.Force);
		    else
				Reporting.AddLine("TensionCap " + TensionCap + " " + ConstUnit.Force + " << " + component.AxialTension + ConstUnit.Force + " (NG)");

		    if (weldcap >= Math.Sqrt(Math.Pow(MaxH, 2) + Math.Pow(clipAngle.ForceY, 2)))
				Reporting.AddLine("weldcap " + weldcap + " " + ConstUnit.Force + " >= " + Math.Sqrt(Math.Pow(MaxH, 2) + Math.Pow(clipAngle.ForceY, 2)) + ConstUnit.Force + " (OK)");
		    else if (ConstNum.TOLERANCE * weldcap >= Math.Sqrt(Math.Pow(MaxH, 2) + Math.Pow(clipAngle.ForceY, 2)))
				Reporting.AddLine("weldcap " + weldcap + " " + ConstUnit.Force + " << " + Math.Sqrt(Math.Pow(MaxH, 2) + Math.Pow(clipAngle.ForceY, 2)) + ConstUnit.Force + " (NG)");
		    else
				Reporting.AddLine("weldcap " + weldcap + " " + ConstUnit.Force + " << " + Math.Sqrt(Math.Pow(MaxH, 2) + Math.Pow(clipAngle.ForceY, 2)) + ConstUnit.Force + " (NG)");

		    if (ClipAngleMin <= clipAngle.Length)
			    Reporting.AddLine("Minimum Length of Clip Angle = " + ClipAngleMin + " <= " + clipAngle.Length + ConstUnit.Length + " (OK)");
		    else
			    Reporting.AddLine("Minimum Length of Clip Angle = " + ClipAngleMin + " >> " + clipAngle.Length + ConstUnit.Length + " (NG)");

		    if (ClipAngleMax >= clipAngle.Length)
			    Reporting.AddLine("Maximum Length of Clip Angle = " + ClipAngleMax + " >= " + clipAngle.Length + ConstUnit.Length + " (OK)");
		    else
			    Reporting.AddLine("Maximum Length of Clip Angle = " + ClipAngleMax + " << " + clipAngle.Length + ConstUnit.Length + " (NG)");

		    Reporting.AddLine("Try: 2" + clipAngle.SizeName);

		    if (column.WebOrientation == EWebOrientation.OutOfPlane && 2 * osl + thick > column.Shape.t)
			    Reporting.AddLine("Angles will not fit in the column web. (NG)");

		    Reporting.AddHeader("Bolt Bearing on Angle OSL:");
			Fbe = CommonCalculations.EdgeBearing(component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistLongDir, clipAngle.BoltOnColumn.HoleLength, component.WinConnect.ShearClipAngle.BoltOnGusset.BoltSize, clipAngle.Material.Fu, component.WinConnect.ShearClipAngle.BoltOnGusset.HoleType, false);
			Fbs = CommonCalculations.SpacingBearing(component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir, clipAngle.BoltOnColumn.HoleLength, component.WinConnect.ShearClipAngle.BoltOnGusset.BoltSize, component.WinConnect.ShearClipAngle.BoltOnGusset.HoleType, clipAngle.Material.Fu, false);

		    Reporting.AddHeader("Bearing Capacity = BrCap:");
		    Reporting.AddLine("= 2 * (Fbe + Fbs * (n - 1)) * t");
		    Reporting.AddLine("= 2 * (" + Fbe + " + " + Fbs + " * (" + component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts + " - 1)) * " + clipAngle.Thickness);

		    Reporting.AddHeader("Shear Yielding of Angles:");
		    Reporting.AddLine("Clip Angle Length = La ");
		    Reporting.AddLine("= (n - 1) * s + 2 * e");
		    Reporting.AddLine("= (" + component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts + " - 1) * " + component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir + " + 2 * " + component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistLongDir);
		    Reporting.AddLine("= " + clipAngle.Length + ConstUnit.Length);

		    Ag = clipAngle.Length * clipAngle.Thickness;
		    FiRn = ConstNum.FIOMEGA1_0N * 2 * Ag * 0.6 * clipAngle.Material.Fy;
		    Reporting.AddLine("Ag = La * t = " + clipAngle.Length + " * " + clipAngle.Thickness + " = " + Ag + ConstUnit.Area);
		    Reporting.AddLine("FiRn = " + ConstNum.FIOMEGA1_0N + " * 2 * Ag * 0.6 * Fy");
		    Reporting.AddLine("= " + ConstNum.FIOMEGA1_0N + " * 2 * " + Ag + " * 0.6 * " + clipAngle.Material.Fy);
		    if (FiRn >= clipAngle.ForceY)
			    Reporting.AddLine("= " + FiRn + " >= " + clipAngle.ForceY + ConstUnit.Force + " (OK)");
		    else
			    Reporting.AddLine("= " + FiRn + " << " + clipAngle.ForceY + ConstUnit.Force + " (NG)");

		    Reporting.AddHeader("Shear Rupture of Angles:");
		    Anv = (clipAngle.Length - component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts * (clipAngle.BoltOnColumn.HoleLength + ConstNum.SIXTEENTH_INCH)) * clipAngle.Thickness;
			FiRn = ConstNum.FIOMEGA0_75N * 2 * Anv * 0.6 * clipAngle.Material.Fu;
		    Reporting.AddLine("Anv = (La - n * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
		    Reporting.AddLine("= (" + clipAngle.Length + " - " + component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts + " * (" + clipAngle.BoltOnColumn.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + clipAngle.Thickness);
		    Reporting.AddLine("= " + Anv + ConstUnit.Area);
		    Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * 2 * Anv * 0.6 * Fu");
		    Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 2 * " + Anv + " * 0.6 * " + clipAngle.Material.Fu);
		    if (FiRn >= clipAngle.ForceY)
			    Reporting.AddLine("= " + FiRn + " >= " + clipAngle.ForceY + ConstUnit.Force + " (OK)");
		    else
			    Reporting.AddLine("= " + FiRn + " << " + clipAngle.ForceY + ConstUnit.Force + " (NG)");

		    Reporting.AddHeader("Block Shear Rupture of Each Angle:");
		    Agv = (clipAngle.Length - component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistLongDir) * clipAngle.Thickness;
		    Reporting.AddLine("Agv = (La - el) * t");
		    Reporting.AddLine("= (" + clipAngle.Length + " - " + component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistLongDir + ") * " + clipAngle.Thickness);
		    Reporting.AddLine("= " + Agv + ConstUnit.Area);

		    Anv = Agv - (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 0.5) * (clipAngle.BoltOnColumn.HoleLength + ConstNum.SIXTEENTH_INCH) * clipAngle.Thickness;
		    Reporting.AddLine("Anv = Agv - (N - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
		    Reporting.AddLine("= " + Agv + " - (" + component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts + " - 0.5) * (" + clipAngle.BoltOnColumn.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ")*" + clipAngle.Thickness);
		    Reporting.AddLine("= " + Anv + ConstUnit.Area);

		    Agt = component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistTransvDir * clipAngle.Thickness;
		    Reporting.AddLine("Agt = et * t =  " + component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistTransvDir + " * " + clipAngle.Thickness + " = " + Agt + ConstUnit.Area);

		    Ant = Agt - 0.5 * (clipAngle.BoltOnColumn.HoleWidth + ConstNum.SIXTEENTH_INCH) * clipAngle.Thickness;
		    Reporting.AddLine("Ant = Agt - 0.5 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
		    Reporting.AddLine("= " + Agt + " - 0.5 * (" + clipAngle.BoltOnColumn.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ") * " + clipAngle.Thickness);
		    Reporting.AddLine("= " + Ant + ConstUnit.Area);
			FiRn = SmallMethodsDesign.BlockShearPrint(Agv, Anv, Agt, Ant, clipAngle.Material.Fy, clipAngle.Material.Fu, true);

		    if (FiRn >= clipAngle.ForceY / 2)
			    Reporting.AddLine("= " + FiRn + " >= " + clipAngle.ForceY + ConstUnit.Force + " (OK)");
		    else
			    Reporting.AddLine("= " + FiRn + " << " + clipAngle.ForceY + ConstUnit.Force + " (NG)");

		    b = (clipAngle.GussetSideGage - thick) / 2 - clipAngle.Thickness / 2;
		    if (column.WebOrientation == EWebOrientation.InPlane)
			    edg = Math.Min(osl - clipAngle.Thickness / 2 - b, (column.Shape.bf - clipAngle.GussetSideGage) / 2);
		    else
			    edg = osl - clipAngle.Thickness / 2 - b;

			Reporting.AddGoToHeader("Angle OSL Tension Strength:");
		    if (String.IsNullOrEmpty(gagecheck))
			    Reporting.AddLine("Gage, (g) = " + clipAngle.GussetSideGage);
		    else
		    {
			    Reporting.AddLine("Gage (g) = " + clipAngle.GussetSideGage + " (NG)");
				Reporting.AddLine(gagecheck);
		    }

			Reporting.AddLine("Bolt Distance to Back of the Angle Minus Half Thickness = b");
		    Reporting.AddLine("= (g - th) / 2 - t / 2");
		    Reporting.AddLine("= (" + clipAngle.GussetSideGage + " - " + thick + ") / 2 - " + (clipAngle.Thickness / 2));
		    Reporting.AddLine("= " + b + ConstUnit.Length);

			Reporting.AddLine("Bolt Distance to Edge = e ");
		    Reporting.AddLine("= osl - t / 2 - b");
		    Reporting.AddLine("= " + osl + " - " + clipAngle.Thickness + " / 2 - " + b);
		    Reporting.AddLine("= " + edg + ConstUnit.Length);

		    Ta = SmallMethodsDesign.HangerAllowable(memberType, clipAngle.Thickness, ref edg, b, ball, true);

			Reporting.AddLine("Reduction in Tension Strength due to Moment = Tm");
		    Reporting.AddLine("= " + Reduction + ConstUnit.Force); // & " (see 'Help' for computation)"
		    
			Reporting.AddHeader("Tension Strength:");
		    FiRn = 2 * numberOfBolts * Ta - Reduction;
		    Reporting.AddLine("FiRn = 2 * n * Ta - Tm");
		    Reporting.AddLine("= 2 * " + numberOfBolts + " * " + Ta + " - " + Reduction);
		    if (FiRn >= component.AxialTension)
			    Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
		    else
			    Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");

		    if (clipAngle.AnglesBoltedToGusset)
		    {
			    clipAngle.WeldSize = 0;
			    SmallMethodsDesign.ClipAnglesGussetSide(memberType, true);
		    }
		    else
		    {
			    SmallMethodsDesign.ClipWelds(memberType, MaxH, clipAngle.ForceY, ref CALengthWeld, ClipAngleMax, ref weldcap, true);
			    H_Ten = component.AxialTension;
			    H_Comp = component.AxialCompression;
			    V = clipAngle.ForceY;
			    R = Math.Sqrt(Math.Pow(V, 2) + Math.Pow(H_Ten, 2));
			    SmallMethodsDesign.GussetandBeamCheckwithWeldedClipAngles(memberType, V, H_Ten, H_Comp, R);
		    }
		    if (!AllisOkey)
			    Reporting.AddLine("Selected clip angles, 2" + clipAngle.SizeName + " will not work.");
		    else
			    Reporting.AddLine("Use 2 " + clipAngle.SizeName);
	    }

	    public static void ClipAngleForces(EMemberType memberType)
		{
			double Hb6C;
			double Hb6T;
			double Hb5C;
			double Hb5T;
			double Tension;
			double Tens3;
			double tens1;
			double Compression;
			double Comp3;
			double comp1;
			double Hb4C;
			double Hb4T;
			double Hb3C;
			double Hb3T;
			double YForce2;
			double YForce1;
		    string connector = MiscMethods.GetComponentName(memberType);

			SeismicForceCalc.TransferForceAndBeamFx(memberType);

			Reporting.AddHeader("Clip Angle Forces");

			DetailData beam;
			DetailData upperBrace;
			DetailData lowerBrace;

			switch (memberType)
			{
				case EMemberType.RightBeam:
					beam = CommonDataStatic.DetailDataDict[memberType];
					upperBrace = CommonDataStatic.DetailDataDict[EMemberType.UpperRight];
					lowerBrace = CommonDataStatic.DetailDataDict[EMemberType.LowerRight];

					YForce1 = Math.Abs(-upperBrace.BraceConnect.Gusset.GussetEFTension.Vb - lowerBrace.BraceConnect.Gusset.GussetEFCompression.Vb + Math.Abs(beam.ShearForce));
					YForce2 = Math.Abs(upperBrace.BraceConnect.Gusset.GussetEFCompression.Vb + lowerBrace.BraceConnect.Gusset.GussetEFTension.Vb + Math.Abs(beam.ShearForce));
					beam.WinConnect.ShearClipAngle.ForceY = Math.Max(YForce1, YForce2);

					Hb3T = upperBrace.BraceConnect.Gusset.GussetEFTension.Hc;
					Hb3C = upperBrace.BraceConnect.Gusset.GussetEFCompression.Hc;
					Hb4T = lowerBrace.BraceConnect.Gusset.GussetEFTension.Hc;
					Hb4C = lowerBrace.BraceConnect.Gusset.GussetEFCompression.Hc;

					comp1 = -Hb4C + Hb3T + beam.AxialCompression;
					Comp3 = -Hb3C + Hb4T + beam.AxialCompression;

					Compression = Math.Max(0, Math.Max(comp1, Comp3));

					tens1 = -Hb3T + Hb4C + beam.AxialTension;
					Tens3 = -Hb4T + Hb3C + beam.AxialTension;

					Tension = Math.Max(0, Math.Max(tens1, Tens3));

					//beam.Compression = Compression;
					//beam.Tension = Tension;
					beam.WinConnect.ShearClipAngle.ForceX = Math.Max(Tension, Compression);

					Reporting.AddLine(String.Format("Vertical Force on {0} = V = {1} {2}", connector, beam.WinConnect.ShearClipAngle.ForceX, ConstUnit.Force));
					Reporting.AddLine(String.Format("Horizontal Force on {0} = H", connector));
					Reporting.AddLine(String.Format("H (Tension) = {0} {1}", Tension, ConstUnit.Force));
					Reporting.AddLine(String.Format("H (Compression) = {0} {1}", Compression, ConstUnit.Force));

					CommonDataStatic.DetailDataDict[memberType] = beam;
					CommonDataStatic.DetailDataDict[EMemberType.UpperRight] = upperBrace;
					CommonDataStatic.DetailDataDict[EMemberType.LowerRight] = lowerBrace;

					break;
				case EMemberType.LeftBeam:
					beam = CommonDataStatic.DetailDataDict[memberType];
					upperBrace = CommonDataStatic.DetailDataDict[EMemberType.UpperLeft];
					lowerBrace = CommonDataStatic.DetailDataDict[EMemberType.LowerLeft];

					YForce1 = Math.Abs(-upperBrace.BraceConnect.Gusset.GussetEFTension.Vb - lowerBrace.BraceConnect.Gusset.GussetEFCompression.Vb + Math.Abs(beam.ShearForce));     // 3T,4C
					YForce2 = Math.Abs(upperBrace.BraceConnect.Gusset.GussetEFCompression.Vb + lowerBrace.BraceConnect.Gusset.GussetEFTension.Vb + Math.Abs(beam.ShearForce));     // 3C,4T

					beam.WinConnect.ShearClipAngle.ForceY = Math.Max(YForce1, YForce2);

					Hb5T = upperBrace.BraceConnect.Gusset.GussetEFTension.Hc;
					Hb5C = upperBrace.BraceConnect.Gusset.GussetEFCompression.Hc;
					Hb6T = lowerBrace.BraceConnect.Gusset.GussetEFTension.Hc;
					Hb6C = lowerBrace.BraceConnect.Gusset.GussetEFCompression.Hc;

					comp1 = -Hb6C + Hb5T + beam.AxialCompression;
					Comp3 = -Hb5C + Hb6T + beam.AxialCompression;

					Compression = Math.Max(0, Math.Max(comp1, Comp3));

					tens1 = -Hb5T + Hb6C + beam.AxialTension;
					Tens3 = -Hb6T + Hb5C + beam.AxialTension;

					Tension = Math.Max(0, Math.Max(tens1, Tens3));

					//beam.Compression = Compression;
					//beam.Tension = Tension;
					beam.WinConnect.ShearClipAngle.ForceX = Math.Max(Tension, Compression);

					Reporting.AddLine(String.Format("Vertical Force on {0} = V = {1} {2}",connector, beam.WinConnect.ShearClipAngle.ForceX, ConstUnit.Force));
					Reporting.AddLine(String.Format("Horizontal Force on {0} = H", connector));
					Reporting.AddLine(String.Format("H (Tension) = {0} {1}", Tension, ConstUnit.Force));
					Reporting.AddLine(String.Format("H (Compression) = {0} {1}", Compression, ConstUnit.Force));

					CommonDataStatic.DetailDataDict[memberType] = beam;
					CommonDataStatic.DetailDataDict[EMemberType.UpperLeft] = upperBrace;
					CommonDataStatic.DetailDataDict[EMemberType.LowerLeft] = lowerBrace;

					break;
				default:
					CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearClipAngle.ForceY = Math.Max(CommonDataStatic.DetailDataDict[memberType].BraceConnect.Gusset.GussetEFTension.Vc, CommonDataStatic.DetailDataDict[memberType].BraceConnect.Gusset.GussetEFCompression.Vc);
					//CommonDataStatic.DetailDataDict[memberType].AxialTension = CommonDataStatic.DetailDataDict[memberType].BraceConnect.Gusset.GussetEFTension.Hc;
					//CommonDataStatic.DetailDataDict[memberType].AxialCompression = CommonDataStatic.DetailDataDict[memberType].BraceConnect.Gusset.GussetEFCompression.Hc;

					Reporting.AddLine("Direct Welding");
					Reporting.AddLine(string.Empty);
					Reporting.AddLine(String.Format("Vertical Force on {0} = V", connector));
					Reporting.AddLine(String.Format("Vertical Force on {0} = V = {1} {2}",
						connector, CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearClipAngle.ForceY, ConstUnit.Force));
					Reporting.AddLine(string.Empty);
					Reporting.AddLine(String.Format("Horizontal Tension = {0} {1}", CommonDataStatic.DetailDataDict[memberType].AxialTension, ConstUnit.Force));
					Reporting.AddLine(String.Format("Horizontal Compression = {0} {1}", CommonDataStatic.DetailDataDict[memberType].AxialCompression, ConstUnit.Force));

					break;
			}
		}

	    private static void ClipAngles(EMemberType memberType, ref double CALengthBolt, ref double CALengthWeld, ref double weldcap)
		{
			double bvn = 0;
			int N = 0;
			double ClipAngleMax = 0;
			double ClipAngleMin = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];
			var clipAngle = component.WinConnect.ShearClipAngle;

			if (component.ShapeName == ConstString.NONE)
				return;

			if (memberType == EMemberType.LeftBeam || memberType == EMemberType.RightBeam)
			{
				ClipAngleMin = component.Shape.d / 2;
				ClipAngleMax = component.Shape.t;
				if (CommonDataStatic.Preferences.UseContinuousClipAngles &&
					CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].ShapeName != ConstString.NONE &&
					CommonDataStatic.DetailDataDict[EMemberType.UpperRight].ShapeName != ConstString.NONE)
					ClipAngleMin = ClipAngleMax / 2;
				clipAngle.MinBolts = Math.Ceiling((ClipAngleMin - 2 * component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistLongDir) / component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir) + 1;
			}
			else
			{
				if (component.GussetToBeamConnection == EBraceConnectionTypes.ClipAngle)
				{
					ClipAngleMin = component.BraceConnect.Gusset.VerticalForceColumn / 2;
					clipAngle.MinBolts = Math.Ceiling((ClipAngleMin - 2 * component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistLongDir) / component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir) + 1;
					if (CommonDataStatic.Preferences.UseContinuousClipAngles)
						ClipAngleMax = component.BraceConnect.Gusset.VerticalForceColumn - ConstNum.TWO_INCHES;
					else
						ClipAngleMax = component.BraceConnect.Gusset.VerticalForceColumn - ConstNum.TWO_INCHES;
				}
				else
				{
					ClipAngleMin = 6 * ConstNum.ONE_INCH;
					clipAngle.MinBolts = 2;
					ClipAngleMax = 100 * ConstNum.ONE_INCH;
				}
			}

			if (clipAngle.MinBolts < 2)
				clipAngle.MinBolts = 2;

			N = -component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts;
			BoltsForTension.CalcBoltsForTension(memberType, component.WinConnect.ShearClipAngle.BoltOnGusset, (component.AxialTension / 2), (clipAngle.ForceY / 2), N, false);
			bvn = clipAngle.MinBolts;
			component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts = (int)bvn;

			if (clipAngle.AnglesBoltedToGusset)
			{
				if (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts < clipAngle.BoltOnGusset.NumberOfBolts)
					component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts = clipAngle.BoltOnGusset.NumberOfBolts;
				else
					component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts = clipAngle.BoltOnColumn.NumberOfBolts;
			}
			CALengthBolt = (component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfBolts - 1) * component.WinConnect.ShearClipAngle.BoltOnGusset.SpacingLongDir + 2 * component.WinConnect.ShearClipAngle.BoltOnGusset.EdgeDistLongDir;

			//  If Not editmode Then
			clipAngle.Length = CALengthBolt;
			if (clipAngle.AnglesBoltedToGusset)
			{
				if (clipAngle.BoltOnColumn.BoltSize == 0)
				{
                    if (clipAngle.BoltOnColumn.NumberOfRows_User) component.WinConnect.ShearClipAngle.BoltOnGusset.NumberOfRows = clipAngle.BoltOnColumn.NumberOfRows;
                    clipAngle.BoltOnColumn = component.WinConnect.ShearClipAngle.BoltOnColumn;
				    clipAngle.BoltOnGusset = component.WinConnect.ShearClipAngle.BoltOnGusset;
				}
				clipAngle.WeldSize = 0;
			}
			else
				SmallMethodsDesign.ClipWelds(memberType, Math.Max(component.AxialTension, component.AxialCompression), clipAngle.ForceY, ref CALengthWeld, ClipAngleMax, ref weldcap, false);
		}
    }
}