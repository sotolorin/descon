//using System;
//using System.Collections.Generic;
//using Descon.Data;
//using Descon.UI.DataAccess;

//namespace Descon.Calculations
//{
//	public class BeamToColumnConnection
//	{
////		public static void DesignBeamToColumn(EMemberType memberType)
////		{
////			double t60 = 0;
////			double t50 = 0;
////			double t20 = 0;
////			double t6 = 0;
////			double t5 = 0;
////			double t2 = 0;
////			double tmax = 0;
////			double t40 = 0;
////			double t30 = 0;
////			double t10 = 0;
////			double t0 = 0;
////			double t4 = 0;
////			double t3 = 0;
////			double t1 = 0;
////			double b = 0;
////			int NforTV = 0;
////			double cap = 0;
////			int N = 0;
////			double t_Comp = 0;
////			double t_Tens = 0;
////			double supportThickness = 0;
////			double Mfws = 0;
////			double w = 0;
////			double fraverage = 0;
////			double fr = 0;
////			int Fb = 0;
////			double V = 0;
////			double H_Max = 0;
////			double H_Comp = 0;
////			double H_Tens = 0;
////			double L = 0;
////			double beamToColumnWeldSize;

////			Reporting.AddDebugLine("DesignBeamToColumn",
////				new List<string> {"memberType = " + MiscMethods.GetComponentName(memberType)});

////			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BraceVToBeam)
////				return;

////			var component = CommonDataStatic.DetailDataDict[memberType];
////			// This was missing in Descon 7, so I'm hoping it is correct. (MT 11/04/14)
////			if (component.ShapeType == EShapeType.WideFlange)
////				supportThickness = component.Shape.tw;
////			else
////				supportThickness = component.Shape.tf;

////			if (component.GussetToColumnConnection == EBraceConnectionTypes.DirectlyWelded)
////			{
////				L = component.Shape.d - (2 * component.Shape.kdes);
////				H_Tens = component.Tension;
////				H_Comp = component.Compression;
////				H_Max = Math.Max(H_Tens, H_Comp);
////				V = component.WinConnect.ShearClipAngle.ForceY;

////				Fb = 0;
////				fr = Math.Sqrt(Math.Pow(H_Max / L + Fb, 2) + Math.Pow(V / L, 2));
////				fraverage = Math.Sqrt(Math.Pow(H_Max / L + Fb / 2.0, 2) + Math.Pow(V / L, 2));
////				w = Math.Max(fr, 1.25 * fraverage) / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
////				beamToColumnWeldSize = NumberFun.ConvertFromFraction(w, 16);
////				Mfws = CommonCalculations.MinimumWeld(supportThickness, component.Shape.tw);
////				beamToColumnWeldSize = Math.Max(beamToColumnWeldSize, Mfws);
////				component.EndSetback = 0;

////				return;
////			}

////			Reporting.AddHeader(MiscMethods.GetComponentName(memberType) + " - Beam to Column Connection");
////			switch (component.ShearConnection)
////			{
////				case EShearCarriedBy.ClipAngle:
////					ClipAnglesBrace.ClipAngleForces(memberType);

////					t_Tens = component.Tension / 2;
////					t_Comp = component.Compression / 2;
////					V = component.WinConnect.ShearClipAngle.ForceY / 2;

////					Reporting.AddLine("Vertical Force on Each Clip Angle = " + V + ConstUnit.Force);
////					Reporting.AddLine("Horizontal Tension on Each Clip Angle = " + t_Tens + ConstUnit.Force);
////					Reporting.AddLine("Horizontal Compression on Each Clip Angle = " + t_Comp + ConstUnit.Force);

////					N = -component.WinConnect.ShearClipAngle.BoltOnColumn.NumberOfBolts;

////					Reporting.AddHeader("Angle to Column Bolts");
////					Reporting.AddLine(component.WinConnect.ShearClipAngle.BoltOnColumn.NumberOfBolts + " " + component.WinConnect.ShearClipAngle.BoltOnColumn.BoltName + " Bolts");
////					Reporting.AddHeader("Shear Strength of Bolts");
////					if (component.WinConnect.ShearClipAngle.BoltOnColumn.Connection == EBoltType.SC &&
////						!(CommonDataStatic.Preferences.Seismic == ESeismic.Seismic && CommonDataStatic.SeismicSettings.Response == EResponse.RGreaterThan3 && component.WinConnect.ShearClipAngle.BoltOnColumn.HoleType == EBoltHoleType.STD))
////					{
////						cap = 2 * component.WinConnect.ShearClipAngle.BoltOnColumn.NumberOfBolts * component.WinConnect.ShearClipAngle.BoltOnColumn.Fv * (1 - t_Tens / (1.13F * component.WinConnect.ShearClipAngle.BoltOnColumn.Pretension * component.WinConnect.ShearClipAngle.BoltOnColumn.NumberOfBolts));
////						Reporting.AddLine("= 2 * n * (" + ConstString.PHI + " Rn) * (1 - Tu / (1.13 * Tm * n))");
////						Reporting.AddLine("= 2 * " + component.WinConnect.ShearClipAngle.BoltOnColumn.NumberOfBolts + " * " + component.WinConnect.ShearClipAngle.BoltOnColumn.Fv + " *  (1 - " + t_Tens + " / (1.13 * " + component.WinConnect.ShearClipAngle.BoltOnColumn.Pretension + " * " + component.WinConnect.ShearClipAngle.BoltOnColumn.NumberOfBolts + "))");
////						if (cap >= 2 * V)
////							Reporting.AddLine(cap + " >= " + 2 * V + ConstUnit.Force + " (OK)");
////						else
////							Reporting.AddLine(cap + " << " + 2 * V + ConstUnit.Force + " (NG)");
////					}
////					else
////					{
////						cap = 2 * component.WinConnect.ShearClipAngle.BoltOnColumn.NumberOfBolts * component.WinConnect.ShearClipAngle.BoltOnColumn.Fv;
////						if (cap >= 2 * V)
////							Reporting.AddLine("2 * n * FiRn = 2 * " + component.WinConnect.ShearClipAngle.BoltOnColumn.NumberOfBolts + " * " + component.WinConnect.ShearClipAngle.BoltOnColumn.Fv + " = " + cap + " >= " + 2 * V + ConstUnit.Force + " (OK)");
////						else
////							Reporting.AddLine("2 * n * FiRn = 2 * " + component.WinConnect.ShearClipAngle.BoltOnColumn.NumberOfBolts + " * " + component.WinConnect.ShearClipAngle.BoltOnColumn.Fv + " = " + cap + " << " + 2 * V + ConstUnit.Force + " (NG)");
////					}
////					NforTV = (int)BoltsForTension.CalcBoltsForTension(memberType, component.WinConnect.ShearClipAngle.BoltOnColumn, t_Tens, V, N, true);
////					if (NforTV < 2)
////						NforTV = 2;
////					component.WinConnect.ShearClipAngle.BoltOnColumn.NumberOfBolts = NforTV;
////					b = BoltsForTension.CalcBoltsForTension(memberType, component.WinConnect.ShearClipAngle.BoltOnColumn, t_Tens, V, NforTV, true);

////					ClipAnglesBrace.DesignClipAngles(memberType, NforTV, b);

////					if (CommonDataStatic.Preferences.UseContinuousClipAngles)
////					{
////						component.BraceConnect.Beam.TCopeL = Math.Floor(component.WinConnect.ShearClipAngle.ShortLeg
////																			+ component.WinConnect.ShearClipAngle.LongLeg - component.WinConnect.ShearClipAngle.LengthOfOSL
////																			+ component.Shape.kdet - component.Shape.tf - component.EndSetback);

////						component.BraceConnect.Beam.CopeDepth = component.Shape.kdet;
////					}
////					else
////					{
////						component.BraceConnect.Beam.TCopeL = 0;
////						component.BraceConnect.Beam.CopeDepth = 0;
////					}
////					if (CommonDataStatic.Preferences.UseContinuousClipAngles)
////					{
////						DetailData upperBrace;
////						DetailData lowerBrace;

////						if (memberType == EMemberType.LowerLeft || memberType == EMemberType.UpperLeft)
////							upperBrace = CommonDataStatic.DetailDataDict[EMemberType.UpperLeft];
////						else
////							upperBrace = CommonDataStatic.DetailDataDict[EMemberType.UpperRight];

////						if (memberType == EMemberType.LowerLeft || memberType == EMemberType.UpperLeft)
////							lowerBrace = CommonDataStatic.DetailDataDict[EMemberType.LowerLeft];
////						else
////							lowerBrace = CommonDataStatic.DetailDataDict[EMemberType.LowerRight];

////						switch (memberType)
////						{
////							case EMemberType.LowerRight:
////							case EMemberType.UpperRight:
////								var rBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

////								rBeam.BraceConnect.Beam.FillerRequired = false;
////								t1 = rBeam.Shape.tw;
////								if (upperBrace.IsActive && lowerBrace.IsActive)
////								{
////									t3 = upperBrace.BraceConnect.Gusset.Thickness;
////									t4 = lowerBrace.BraceConnect.Gusset.Thickness;
////									t0 = Math.Max(t1, Math.Max(t3, t4));
////									t10 = (t0 - t1) / 2;
////									t30 = (t0 - t3) / 2;
////									t40 = (t0 - t4) / 2;
////									tmax = Math.Max(t10, Math.Max(t30, t40));
////								}
////								else if (upperBrace.IsActive)
////								{
////									t3 = upperBrace.BraceConnect.Gusset.Thickness;
////									t0 = Math.Max(t1, t3);
////									t10 = (t0 - t1) / 2;
////									t30 = (t0 - t3) / 2;
////									tmax = Math.Max(t10, t30);
////								}
////								else if (lowerBrace.IsActive)
////								{
////									t4 = lowerBrace.BraceConnect.Gusset.Thickness;
////									t0 = Math.Max(t1, t4);
////									t10 = (t0 - t1) / 2;
////									t40 = (t0 - t4) / 2;
////									tmax = Math.Max(t10, t40);
////								}
////								if (tmax >= (CommonDataStatic.Units == EUnit.US ? (ConstNum.SIXTEENTH_INCH) : (25.4 * ConstNum.SIXTEENTH_INCH)))
////									rBeam.BraceConnect.Beam.FillerRequired = true;
////								break;
////							case EMemberType.LowerLeft:
////							case EMemberType.UpperLeft:
////								var lBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
////								lBeam.BraceConnect.Beam.FillerRequired = false;
////								t2 = lBeam.Shape.tw;
////								if (upperBrace.IsActive && lowerBrace.IsActive)
////								{
////									t5 = upperBrace.BraceConnect.Gusset.Thickness;
////									t6 = lowerBrace.BraceConnect.Gusset.Thickness;
////									t0 = Math.Max(t2, Math.Max(t5, t6));
////									t20 = (t0 - t2) / 2;
////									t50 = (t0 - t5) / 2;
////									t60 = (t0 - t6) / 2;
////									tmax = Math.Max(t20, Math.Max(t50, t60));
////								}
////								else if (upperBrace.IsActive)
////								{
////									t5 = upperBrace.BraceConnect.Gusset.Thickness;
////									t0 = Math.Max(t2, t5);
////									t20 = (t0 - t2) / 2;
////									t50 = (t0 - t5) / 2;
////									tmax = Math.Max(t20, t50);
////								}
////								else if (lowerBrace.IsActive)
////								{
////									t6 = lowerBrace.BraceConnect.Gusset.Thickness;
////									t0 = Math.Max(t2, t6);
////									t20 = (t0 - t2) / 2;
////									t60 = (t0 - t6) / 2;
////									tmax = Math.Max(t20, t60);
////								}
////								if (tmax >= (CommonDataStatic.Units == EUnit.US ? (ConstNum.SIXTEENTH_INCH) : (25.4 * ConstNum.SIXTEENTH_INCH)))
////									lBeam.BraceConnect.Beam.FillerRequired = true;
////								break;
////						}
////					}
////					if (CommonDataStatic.Preferences.UseContinuousClipAngles)
////					{
////						DetailData upperBrace;
////						if (memberType == EMemberType.LowerLeft || memberType == EMemberType.UpperLeft)
////							upperBrace = CommonDataStatic.DetailDataDict[EMemberType.UpperLeft];
////						else
////							upperBrace = CommonDataStatic.DetailDataDict[EMemberType.UpperRight];

////						DetailData lowerBrace;
////						if (memberType == EMemberType.LowerLeft || memberType == EMemberType.UpperLeft)
////							lowerBrace = CommonDataStatic.DetailDataDict[EMemberType.LowerLeft];
////						else
////							lowerBrace = CommonDataStatic.DetailDataDict[EMemberType.LowerRight];

////						switch (memberType)
////						{
////							case EMemberType.LowerRight:
////							case EMemberType.UpperRight:
////								var rBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

////								if (upperBrace.IsActive || lowerBrace.IsActive)
////								{
////									t1 = rBeam.Shape.tw;
////									t3 = upperBrace.BraceConnect.Gusset.Thickness;
////									t4 = lowerBrace.BraceConnect.Gusset.Thickness;
////									if (upperBrace.IsActive && lowerBrace.IsActive)
////									{
////										t0 = Math.Max(t1, Math.Max(t3, t4));
////										t10 = (t0 - t1) / 2;
////										t30 = (t0 - t3) / 2;
////										t40 = (t0 - t4) / 2;
////										if (t10 >= ConstNum.SIXTEENTH_INCH && t10 < ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine("Provide " + t10 + ConstUnit.Length + " filler and increase weld size");
////											Reporting.AddLine("accordingly for the right hand side beam, both sides of the web.");
////										}
////										else if (t10 >= ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine(t10 + " " + ConstUnit.Length + " fillers extending beyond the edges of the angles");
////											Reporting.AddLine("are required for the right hand side beam.");
////											Reporting.AddLine(" (See LRFD Specification Section J6.)");
////										}
////										if (t30 >= ConstNum.SIXTEENTH_INCH && t30 < ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine("Provide " + t30 + ConstUnit.Length + " filler and increase weld size");
////											Reporting.AddLine("accordingly for the upper right hand side gusset, both sides of the plate.");
////										}
////										else if (t30 >= ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine(t30 + " " + ConstUnit.Length + " fillers extending beyond the edges of the angles");
////											Reporting.AddLine("are required for the upper right hand side gusset.");
////											Reporting.AddLine("(See LRFD Specification Section J6.)");
////										}
////										if (t40 >= ConstNum.SIXTEENTH_INCH && t40 < ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine("Provide " + t40 + ConstUnit.Length + " filler and increase weld size accordingly");
////											Reporting.AddLine("for the lower right hand side gusset, both sides of the plate.");
////										}
////										else if (t40 >= ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine(t40 + " " + ConstUnit.Length + " fillers extending beyond the edges of the angles");
////											Reporting.AddLine("are required for the lower right hand side gusset.");
////											Reporting.AddLine("(See LRFD Specification Section J6.)");
////										}
////									}
////									else if (upperBrace.IsActive)
////									{
////										t0 = Math.Max(t1, t3);
////										t10 = (t0 - t1) / 2;
////										t30 = (t0 - t3) / 2;
////										if (t10 >= ConstNum.SIXTEENTH_INCH && t10 < ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine("Provide " + t10 + ConstUnit.Length + " filler and increase weld size");
////											Reporting.AddLine("accordingly for the right hand side beam, both sides of the web.");
////										}
////										else if (t10 >= ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine(t10 + " " + ConstUnit.Length + " fillers extending beyond the edges of the angles");
////											Reporting.AddLine("are required for the right hand side beam.");
////											Reporting.AddLine("(See LRFD Specification Section J6.)");
////										}
////										if (t30 >= ConstNum.SIXTEENTH_INCH && t30 < ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine("Provide " + t30 + ConstUnit.Length + " filler and increase weld size accordingly");
////											Reporting.AddLine("for the upper right hand side gusset, both sides of the plate.");
////										}
////										else if (t30 >= ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine(t30 + " " + ConstUnit.Length + " fillers extending beyond the edges of the angles");
////											Reporting.AddLine("are required for the upper right hand side gusset.");
////											Reporting.AddLine("(See LRFD Specification Section J6.)");
////										}
////									}
////									else if (lowerBrace.IsActive)
////									{
////										t0 = Math.Max(t1, t4);
////										t10 = (t0 - t1) / 2;
////										t40 = (t0 - t4) / 2;
////										if (t10 >= ConstNum.SIXTEENTH_INCH && t10 < ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine("Provide " + t10 + ConstUnit.Length + " filler and increase weld size");
////											Reporting.AddLine("accordingly for the right hand side beam, both sides of the web.");
////										}
////										else if (t10 >= ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine(t10 + " " + ConstUnit.Length + " fillers extending beyond the edges of the angles");
////											Reporting.AddLine("are required for the right hand side beam.");
////											Reporting.AddLine("(See LRFD Specification Section J6.)");
////										}
////										if (t40 >= ConstNum.SIXTEENTH_INCH && t40 < ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine("Provide " + t40 + ConstUnit.Length + " filler and increase weld size accordingly");
////											Reporting.AddLine("for the upper right hand side gusset, both sides of the plate.");
////										}
////										else if (t40 >= ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine(t40 + " " + ConstUnit.Length + " fillers extending beyond the edges of the angles");
////											Reporting.AddLine("are required for the upper right hand side gusset.");
////											Reporting.AddLine(" (See LRFD Specification Section J6.)");
////										}
////									}
////									SmallMethodsDesign.BeamShear(memberType);
////								}
////								break;
////							case EMemberType.LowerLeft:
////							case EMemberType.UpperLeft:
////								var lBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

////								if (upperBrace.IsActive || lowerBrace.IsActive)
////								{
////									t2 = lBeam.Shape.tw;
////									t5 = upperBrace.BraceConnect.Gusset.Thickness;
////									t6 = lowerBrace.BraceConnect.Gusset.Thickness;
////									if (upperBrace.IsActive && lowerBrace.IsActive)
////									{
////										t0 = Math.Max(t2, Math.Max(t5, t6));
////										t20 = (t0 - t2) / 2;
////										t50 = (t0 - t5) / 2;
////										t60 = (t0 - t6) / 2;
////										if (t20 >= ConstNum.SIXTEENTH_INCH && t20 < ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine("Provide " + t20 + ConstUnit.Length + " filler and increase weld size");
////											Reporting.AddLine("accordingly for the left hand side beam, both sides of the web.");
////										}
////										else if (t20 >= ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine(t20 + " " + ConstUnit.Length + " fillers extending beyond the edges of the angles");
////											Reporting.AddLine("are required for the left hand side beam.");
////											Reporting.AddLine("(See LRFD Specification Section J6.)");
////										}
////										if (t50 >= ConstNum.SIXTEENTH_INCH && t50 < ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine("Provide " + t50 + ConstUnit.Length + " filler and increase weld size accordingly");
////											Reporting.AddLine("accordingly for the upper left hand side gusset, both sides of the plate.");
////										}
////										else if (t50 >= ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine(t50 + " " + ConstUnit.Length + " fillers extending beyond the edges of the angles");
////											Reporting.AddLine("are required for the upper left hand side gusset.");
////											Reporting.AddLine("(See LRFD Specification Section J6.)");
////										}
////										if (t60 >= ConstNum.SIXTEENTH_INCH && t60 < ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine("Provide " + t60 + ConstUnit.Length + " filler and increase weld size");
////											Reporting.AddLine("accordingly for the lower left hand side gusset, both sides of the plate.");
////										}
////										else if (t60 >= ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine(t60 + ConstUnit.Length + " fillers extending beyond the edges of the angles");
////											Reporting.AddLine("are required for the lower left hand side gusset.");
////											Reporting.AddLine("(See LRFD Specification Section J6.)");
////										}
////									}
////									else if (upperBrace.IsActive)
////									{
////										t0 = Math.Max(t2, t5);
////										t20 = (t0 - t2) / 2;
////										t50 = (t0 - t5) / 2;
////										if (t20 >= ConstNum.SIXTEENTH_INCH && t20 < ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine("Provide " + t20 + ConstUnit.Length + " filler and increase weld size");
////											Reporting.AddLine("accordingly for the left hand side beam, both sides of the web.");
////										}
////										else if (t20 >= ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine(t20 + " " + ConstUnit.Length + " fillers extending beyond the edges of the angles");
////											Reporting.AddLine("are required for the left hand side beam.");
////											Reporting.AddLine("(See LRFD Specification Section J6.)");
////										}
////										if (t50 >= ConstNum.SIXTEENTH_INCH && t50 < ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine("Provide " + t50 + ConstUnit.Length + " filler and increase weld size");
////											Reporting.AddLine("accordingly for the upper left hand side gusset, both sides of the plate.");
////										}
////										else if (t50 >= ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine(t50 + " " + ConstUnit.Length + " fillers extending beyond the edges of the angles");
////											Reporting.AddLine("are required for the upper left hand side gusset.");
////											Reporting.AddLine("(See LRFD Specification Section J6.)");
////										}
////									}
////									else if (lowerBrace.IsActive)
////									{
////										t0 = Math.Max(t2, t6);
////										t20 = (t0 - t2) / 2;
////										t60 = (t0 - t6) / 2;
////										if (t20 >= ConstNum.SIXTEENTH_INCH && t20 < ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine("Provide " + t20 + ConstUnit.Length + " filler and increase weld size");
////											Reporting.AddLine("accordingly for the left hand side beam, both sides of the web.");
////										}
////										else if (t20 >= ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine(t20 + " " + ConstUnit.Length + " fillers extending beyond the edges of the angles");
////											Reporting.AddLine("are required for the left hand side beam.");
////											Reporting.AddLine("(See LRFD Specification Section J6.)");
////										}
////										if (t60 >= ConstNum.SIXTEENTH_INCH && t60 < ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine("Provide " + t60 + ConstUnit.Length + " filler and increase weld size");
////											Reporting.AddLine("accordingly for the upper left hand side gusset, both sides of the plate.");
////										}
////										else if (t60 >= ConstNum.QUARTER_INCH)
////										{
////											Reporting.AddLine(t60 + " " + ConstUnit.Length + " fillers extending beyond the edges of the angles");
////											Reporting.AddLine("are required for the upper left hand side gusset.");
////											Reporting.AddLine("(See LRFD Specification Section J6.)");
////										}
////									}
////									SmallMethodsDesign.BeamShear(memberType);
////								}
////								break;
////						}
////					}
////					break;
////				case EShearCarriedBy.DirectlyWelded:
////					break;
////				case EShearCarriedBy.SinglePlate:
////					DesignSinglePlate.CalcDesignSinglePlate(memberType, component.Tension, component.Compression, component.WinConnect.ShearClipAngle.ForceY, 0);
////					break;
////				case EShearCarriedBy.Tee:
////					DesignFabricatedTee.CalcDesignFabricatedTee(memberType, component.Tension, component.Compression, component.WinConnect.ShearClipAngle.ForceY, 0);
////					break;
////				case EShearCarriedBy.EndPlate:
////					DesignEndPlate.CalcDesignEndPlate(memberType, component.Tension, component.Compression, component.WinConnect.ShearClipAngle.ForceY, 0);
////					break;
////			}

////			ColumnAndBeamCheck(memberType);
////		}
//	}
//}