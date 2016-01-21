using System.Collections.Generic;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
    public static class Calculations
    {
	    /// <summary>
	    /// Can be used by external modules to run the calculations and return an HTML report
	    /// </summary>
	    public static string RunCalculationsSilently(List<DetailData> dataList)
	    {
		    CommonDataStatic.DetailDataDict.Clear();

		    foreach (var detailData in dataList)
			    CommonDataStatic.DetailDataDict.Add(detailData.MemberType, detailData);

		    RunCalculations();
		    RunCalculations();

		    return new ReportBuild().BuildReport();
	    }

	    /// <summary>
		/// Runs the calculations for the UI. The UI handles some other tasks that must be done
		/// </summary>
	    public static void RunCalculations()
	    {
			CommonDataStatic.DetailReportLineList.Clear();
			CommonDataStatic.ReportJavascriptVarList = new Dictionary<string, double>();

		    var initializeData = new PreCalcStuff();

			var upperRight = CommonDataStatic.DetailDataDict[EMemberType.UpperRight];
			var lowerRight = CommonDataStatic.DetailDataDict[EMemberType.LowerRight];
			var upperLeft = CommonDataStatic.DetailDataDict[EMemberType.UpperLeft];
			var lowerLeft = CommonDataStatic.DetailDataDict[EMemberType.LowerLeft];

			initializeData.PreCalc();

		    DesignWin.Design();
			// These are the old calls to the Beam code no longer in use
			//if (CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive)
			//	BeamToColumnConnection.DesignBeamToColumn(EMemberType.LeftBeam);
			//if (CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive)
			//	BeamToColumnConnection.DesignBeamToColumn(EMemberType.RightBeam);

			// These are the brace related calculations
			if (upperRight.IsActive)
			{
                InitialCalcBrace.TubularSetup(EMemberType.UpperRight);
				InitialCalcBrace.InitialCalc(EMemberType.UpperRight);
				GussetSize.CalcGussetSize(EMemberType.UpperRight);
			}
			if (lowerRight.IsActive)
			{
                InitialCalcBrace.TubularSetup(EMemberType.LowerRight);
				InitialCalcBrace.InitialCalc(EMemberType.LowerRight);
				GussetSize.CalcGussetSize(EMemberType.LowerRight);
			}
			if (upperLeft.IsActive)
			{
                InitialCalcBrace.TubularSetup(EMemberType.UpperLeft);
				InitialCalcBrace.InitialCalc(EMemberType.UpperLeft);
				GussetSize.CalcGussetSize(EMemberType.UpperLeft);

			}
			if (lowerLeft.IsActive)
			{
                InitialCalcBrace.TubularSetup(EMemberType.LowerLeft);
				InitialCalcBrace.InitialCalc(EMemberType.LowerLeft);
				GussetSize.CalcGussetSize(EMemberType.LowerLeft);
			}

			new PostCalcStuff().PostCalc();
	    }
    }
}