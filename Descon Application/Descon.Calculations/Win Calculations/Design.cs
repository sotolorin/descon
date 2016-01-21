using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class DesignWin
	{
		internal static void Design()
		{
			double MnBot;
			double MnTop;
			string forceType;

			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var columnSplice = CommonDataStatic.ColumnSplice;

			if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic)
				SeismicCalc.ApplySeismicData();

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BeamToHSSColumn:
					Reporting.AddMainHeader("Beam Connection to HSS Column");
					Reporting.AddLine("Column: " + column.ShapeName + " - " + column.Material.Name);
					if (leftBeam.IsActive)
						Reporting.AddLine("Left Side Beam: " + leftBeam.ShapeName + " - " + leftBeam.Material.Name);
					if (rightBeam.IsActive)
						Reporting.AddLine("Right Side Beam: " + rightBeam.ShapeName + " - " + rightBeam.Material.Name);
					if (column.P > 0)
						forceType = "Compression";
					else if (column.P < 0)
						forceType = "Tension";
					else
						forceType = string.Empty;

					Reporting.AddLine("Axial Force: " + column.P + ConstUnit.Force + " " + forceType);
					break;
				case EJointConfiguration.BeamToColumnFlange:
					Reporting.AddMainHeader("Beam Connection to Column Flange");
					Reporting.AddLine("Column: " + column.ShapeName + " - " + column.Material.Name);
					if (leftBeam.IsActive)
					{
						Reporting.AddLine("Left Side Beam: " + leftBeam.ShapeName + " - " + leftBeam.Material.Name);
						Reporting.AddLine("Moment: " + (leftBeam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
						Reporting.AddLine("Shear: " + leftBeam.ShearForce + ConstUnit.Force);
						Reporting.AddLine("Axial Force: " + leftBeam.P + ConstUnit.Force);
					}
					if (rightBeam.IsActive)
					{
						Reporting.AddLine("Right Side Beam: " + rightBeam.ShapeName + " - " + rightBeam.Material.Name);
						Reporting.AddLine("Moment: " + (rightBeam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
						Reporting.AddLine("Shear: " + rightBeam.ShearForce + ConstUnit.Force);
						Reporting.AddLine("Axial Force: " + rightBeam.P + ConstUnit.Force);
					}
					break;
				case EJointConfiguration.BeamToColumnWeb:
					Reporting.AddMainHeader("Beam Connection to Column Web");
					Reporting.AddLine("Column: " + column.ShapeName + " - " + column.Material.Name);
					if (leftBeam.IsActive)
					{
						Reporting.AddLine("Left Side Beam: " + leftBeam.ShapeName + " - " + leftBeam.Material.Name);
						Reporting.AddLine("Moment: " + (leftBeam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
						Reporting.AddLine("Shear: " + leftBeam.ShearForce + ConstUnit.Force);
						Reporting.AddLine("Axial Force: " + leftBeam.P + ConstUnit.Force);
					}
					if (rightBeam.IsActive)
					{
						Reporting.AddLine("Right Side Beam: " + rightBeam.ShapeName + " - " + rightBeam.Material.Name);
						Reporting.AddLine("Moment: " + (rightBeam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
						Reporting.AddLine("Shear: " + rightBeam.ShearForce + ConstUnit.Force);
						Reporting.AddLine("Axial Force: " + rightBeam.P + ConstUnit.Force);
					}
					break;
				case EJointConfiguration.ColumnSplice:
					Reporting.AddMainHeader("Column Splice");
					switch (columnSplice.ConnectionOption)
					{
						case ESpliceConnection.DirectlyWelded:
							Reporting.AddLine("Directly Welded Column Splice");
							break;
						case ESpliceConnection.ButtPlate:
							Reporting.AddLine("Column Splice with Butt Plate");
							break;
						case ESpliceConnection.FlangeAndWebPlate:
							Reporting.AddLine("Column Splice with Flange and Web Plate");
							break;
						case ESpliceConnection.FlangePlate:
							Reporting.AddLine("Column Splice with Flange Plate");
							break;
					}

					Reporting.AddLine(string.Empty);
					Reporting.AddLine("Top Column: " + tColumn.ShapeName + " - " + tColumn.Material.Name);
					Reporting.AddLine("Bottom Column: " + bColumn.ShapeName + " - " + bColumn.Material.Name);

					Reporting.AddLine(string.Empty);
					Reporting.AddLine("Axial Force (Compression), C = " + columnSplice.Compression + ConstUnit.Force);
					Reporting.AddLine("Axial Force (Tension), T = " + columnSplice.Tension + ConstUnit.Force);
					Reporting.AddLine("Moment (Strong Axis), Ms = " + (columnSplice.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
					Reporting.AddLine("Shear Force (Strong Axis), Vs = " + columnSplice.Shear + ConstUnit.Force);
					Reporting.AddLine("Minimum Compression Axial Force (Cmin) = " + columnSplice.Cmin + ConstUnit.Force);
					if (columnSplice.DesignWebSpliceFor == EDesignWebSpliceFor.Vs)
						columnSplice.WebShear = columnSplice.Shear;
					else
						columnSplice.WebShear = columnSplice.Shear - 0.33 * columnSplice.Cmin;

					Reporting.AddLine("Shear Force for Design, Vs = " + columnSplice.WebShear + ConstUnit.Force);
					if (columnSplice.UseSeismic)
					{
						Reporting.AddHeader("Forces per Seismic Provisions:");
						ColumnSplice.FlangeTensionforSeismic = 0.5 * Math.Min(MiscCalculationDataMethods.ExpYieldStr(bColumn.Material.Fy) * bColumn.Material.Fy * bColumn.Shape.tf * bColumn.Shape.bf, MiscCalculationDataMethods.ExpYieldStr(tColumn.Material.Fy) * tColumn.Material.Fy * tColumn.Shape.tf * tColumn.Shape.bf);
						Reporting.AddLine("Ry * Fy * Af / 2 = " + ColumnSplice.FlangeTensionforSeismic + ConstUnit.Force);
						if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.SMF)
						{
							MnTop = Math.Min(tColumn.Material.Fy * tColumn.Shape.zx, 1.5 * tColumn.Material.Fy * tColumn.Shape.sx);
							MnBot = Math.Min(bColumn.Material.Fy * bColumn.Shape.zx, 1.5 * bColumn.Material.Fy * bColumn.Shape.sx);

							ColumnSplice.FfToDevMoment = Math.Min(MnTop / (tColumn.Shape.d - tColumn.Shape.tf), MnBot / (bColumn.Shape.d - bColumn.Shape.tf));
							Reporting.AddLine("Flange Force to Develop Mn of Smaller Column = " + ColumnSplice.FfToDevMoment + ConstUnit.Force);
						}
						else
							ColumnSplice.FfToDevMoment = 0;
					}
					if (columnSplice.UseSeismic && CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.SMF)
					{
						columnSplice.WebShear = column.ShearForce;
						Reporting.AddLine("Required Shear Strength Based on Column End Expected Yield Moment = " + column.ShearForce + ConstUnit.Force);
					}

					Reporting.AddLine(string.Empty);
					Reporting.AddLine("All Attachments Are: " + columnSplice.Material.Name);
					break;
				case EJointConfiguration.BeamToGirder:
					Reporting.AddMainHeader("Beam Connection to Girder");
					Reporting.AddLine("Girder: " + column.ShapeName + " - " + column.Material.Name);
					if (leftBeam.IsActive)
					{
						Reporting.AddLine("Left Side Beam: " + leftBeam.ShapeName + " - " + leftBeam.Material.Name);
						Reporting.AddLine("Moment: " + (leftBeam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
						Reporting.AddLine("Shear: " + leftBeam.ShearForce + ConstUnit.Force);
						Reporting.AddLine("Axial Force: " + leftBeam.P + ConstUnit.Force);
					}
					if (rightBeam.IsActive)
					{
						if (leftBeam.IsActive)
							Reporting.AddLine(string.Empty);
						Reporting.AddLine("Right Side Beam: " + rightBeam.ShapeName + " - " + rightBeam.Material.Name);
						Reporting.AddLine("Moment: " + (rightBeam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
						Reporting.AddLine("Shear: " + rightBeam.ShearForce + ConstUnit.Force);
						Reporting.AddLine("Axial Force: " + rightBeam.P + ConstUnit.Force);
					}
					break;
				case EJointConfiguration.BeamSplice:
					Reporting.AddMainHeader("Beam Splice Connection");
					if (leftBeam.IsActive)
						Reporting.AddLine("Left Side Beam: " + leftBeam.ShapeName + " - " + leftBeam.Material.Name);
					if (rightBeam.IsActive)
					{
						Reporting.AddLine("Right Side Beam: " + rightBeam.ShapeName + " - " + rightBeam.Material.Name);
						Reporting.AddLine("Moment: " + (rightBeam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
						Reporting.AddLine("Shear: " + rightBeam.ShearForce + ConstUnit.Force);
						Reporting.AddLine("Axial Force: " + rightBeam.P + ConstUnit.Force);
					}
					break;
			}

			if (CommonDataStatic.IsFema)
			{
				Reporting.AddHeader("Reference:");
				Reporting.AddLine("Recommended Seismic Design Criteria For New Steel Moment-Frame Buildings");
				Reporting.AddLine("FEMA 350, July 2000");
				Reporting.AddLine("Federal Emergency Management Agency.");
				Reporting.AddLine(string.Empty);
				Reporting.AddLine("The above reference includes the design procedure,");
				Reporting.AddLine("equations, and notations used in the calculations below.");
				Reporting.AddLine("The user is advised to review this calculation report in");
				Reporting.AddLine("view of the information provided in the above reference,");
				Reporting.AddLine("where additional detailing and quality assurance");
				Reporting.AddLine("requirements are also available.");
				Reporting.AddLine("See, also, -- Errata FEMA 350, April 22, 2003 --");
				Reporting.AddLine("which is available for download at AISC.org");
				Reporting.AddLine(string.Empty);
				Reporting.AddLine("These seismic connections have been prequalified only");
				Reporting.AddLine("for applications using rolled structural steel shapes,");
				Reporting.AddLine("with certain size and material limitations, and conforming");
				Reporting.AddLine("to ASTM A6/A6M-04a Standard Specification.");
			}

			Reporting.AddLine(string.Empty);
			Reporting.AddLine("All Welds Are " + CommonDataStatic.Preferences.DefaultElectrode.Name);

			if (CommonDataStatic.BeamToColumnType != EJointConfiguration.ColumnSplice && rightBeam.MomentConnection != EMomentCarriedBy.Tee)
				rightBeam.WinConnect.Beam.WebHeight = rightBeam.Shape.t;
			if (CommonDataStatic.BeamToColumnType != EJointConfiguration.ColumnSplice && leftBeam.MomentConnection != EMomentCarriedBy.Tee)
				leftBeam.WinConnect.Beam.WebHeight = leftBeam.Shape.t;
			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice && leftBeam.IsActive && rightBeam.IsActive)
			{
				rightBeam.WinConnect.Beam.WebHeight = rightBeam.Shape.t;
				leftBeam.WinConnect.Beam.WebHeight = leftBeam.Shape.t;
				rightBeam.WinConnect.Beam.WebHeight = Math.Min(rightBeam.WinConnect.Beam.WebHeight, leftBeam.WinConnect.Beam.WebHeight);
				leftBeam.WinConnect.Beam.WebHeight = rightBeam.WinConnect.Beam.WebHeight;
			}
			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.ColumnSplice && bColumn.IsActive && tColumn.IsActive)
				ColumnSplice.DesignColumnSplice();
			else
				DesignCalcs();
		}

        /// <summary>
        /// All of this is a rough translation from the last secion of the v7 DDESIGN.design() method, excluding all global variable assignments.
        /// It kick-starts all the Win calculations.
        /// </summary>
        private static void DesignCalcs()
        {
            var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
            var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
            var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			if (CommonDataStatic.BeamToColumnType != EJointConfiguration.ColumnSplice &&
				CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToGirder &&
				CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice)
            {
				if (column.IsActive && rightBeam.IsActive)
				{
					MiscCalculationsWithReporting.DesignBeamToColumnMoment(EMemberType.RightBeam);
					if (rightBeam.MomentConnection != EMomentCarriedBy.EndPlate)	// Don't run shear when Moment is End Plate
						MiscCalculationsWithReporting.DesignBeamToColumnShear(EMemberType.RightBeam);
					if (rightBeam.WinConnect.Beam.TopCope | rightBeam.WinConnect.Beam.BottomCope)
						Cope.CopedBeam(EMemberType.RightBeam, false);
				}
                if (column.IsActive && leftBeam.IsActive)
                {
                    MiscCalculationsWithReporting.DesignBeamToColumnMoment(EMemberType.LeftBeam);
					if (leftBeam.MomentConnection != EMomentCarriedBy.EndPlate)	// Don't run shear when Moment is End Plate
		                MiscCalculationsWithReporting.DesignBeamToColumnShear(EMemberType.LeftBeam);
                    if (leftBeam.WinConnect.Beam.TopCope | leftBeam.WinConnect.Beam.BottomCope)
						Cope.CopedBeam(EMemberType.LeftBeam, false);
                }
            }
            else // Beam Splice or Beam to Girder
            {
				if (rightBeam.IsActive)
					MiscCalculationDataMethods.CalculateCopeStuff(EMemberType.RightBeam);
				if (leftBeam.IsActive)
					MiscCalculationDataMethods.CalculateCopeStuff(EMemberType.LeftBeam);

                if (rightBeam.IsActive && leftBeam.IsActive)
                {
                    // V7 defaults to flange plates in this situation. Not sure if this should be more flexible in V8.
	                if (leftBeam.Moment != 0)
		                leftBeam.MomentConnection = EMomentCarriedBy.FlangePlate;
	                if (rightBeam.Moment != 0)
		                rightBeam.MomentConnection = EMomentCarriedBy.FlangePlate;

                    MiscCalculationsWithReporting.DesignBeamToColumnMoment(EMemberType.RightBeam);
                    MiscCalculationsWithReporting.DesignBeamToColumnMoment(EMemberType.LeftBeam);
                }
				if (rightBeam.IsActive)
					MiscCalculationsWithReporting.DesignBeamToColumnShear(EMemberType.RightBeam);
				if (leftBeam.IsActive)
					MiscCalculationsWithReporting.DesignBeamToColumnShear(EMemberType.LeftBeam);
            }

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange && 
                (rightBeam.Moment > 0 || leftBeam.Moment > 0) &&
                (rightBeam.MomentConnection != EMomentCarriedBy.NoMoment || leftBeam.MomentConnection != EMomentCarriedBy.NoMoment) &&
                (rightBeam.IsActive || leftBeam.IsActive))
            {
                Stiff.FlangeForces(true, false);
				Stiff.Stiffeners(false);
                Stiff.DoublerPl();

                Stiff.FlangeForces(false, false);
                Stiff.Stiffeners(true);
                Stiff.StiffenerAndDoublerPlateWelds();
            }
            else
            {
                if (!CommonDataStatic.IsFema)
                {
					if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn &&
                        (rightBeam.MomentConnection != EMomentCarriedBy.NoMoment || leftBeam.MomentConnection != EMomentCarriedBy.NoMoment) &&
                        (rightBeam.IsActive || leftBeam.IsActive))
                    {
                        Stiff.FlangeForces(CommonDataStatic.SeismicSettings.InelasticPanelZone, true);
                        Stiff.HSSSideWallShear();
                    }
                }
                else
                {
                    if (column.IsActive)
                    {
                        if(rightBeam.IsActive) Fema.ContinuityAndDoublerPlates(EMemberType.RightBeam);
                        if(leftBeam.IsActive) Fema.ContinuityAndDoublerPlates(EMemberType.LeftBeam);
                    }
                }
            }
	    }
	}
}