namespace Descon.Data
{
	/// <summary>
	/// Displays units according the Unit System we are currently using. Most units have spaces before the first character
	/// so they display correctly in reporting.
	/// </summary>
	public class ConstUnit
	{
		public static string Length
		{
			get { return CommonDataStatic.Units == EUnit.Metric ? " mm" : " in."; }
		}

		public static string SmallLength
		{
			get { return CommonDataStatic.Units == EUnit.Metric ? " mm" : " 1/16 in."; }
		}

		public static string LargeLength
		{
			get { return CommonDataStatic.Units == EUnit.Metric ? " m" : " ft."; }
		}

		public static string SecMod
		{
			get { return CommonDataStatic.Units == EUnit.Metric ? " mm³" : " in³"; }
		}

		public static string Area
		{
			get { return CommonDataStatic.Units == EUnit.Metric ? " cm²" : " in²"; }
		}

		public static string Force
		{
			get { return CommonDataStatic.Units == EUnit.Metric ? " kN" : " kips"; }
		}

		public static string ForceUniform
		{
			get { return CommonDataStatic.Units == EUnit.Metric ? " kN/m" : " k/ft."; }
		}

		public static string ForcePerUnitLength
		{
			get { return CommonDataStatic.Units == EUnit.Metric ? " kN-mm" : " kips/in."; }
		}

		public static string Moment
		{
			get { return CommonDataStatic.Units == EUnit.Metric ? " N-mm/mm" : " kip-in./in."; }
		}

		public static string MomentInertia
		{
			get { return CommonDataStatic.Units == EUnit.Metric ? " mm^4" : " in^4"; }
		}

		public static string MomentUnitInch
		{
			get { return CommonDataStatic.Units == EUnit.Metric ? " N-mm" : " k-in."; }
		}

		public static string MomentUnitFoot
		{
			get { return CommonDataStatic.Units == EUnit.Metric ? " kN-m" : " k-ft."; }
		}

		public static string Stress
		{
			get { return CommonDataStatic.Units == EUnit.Metric ? " kN/mm²" : " ksi"; }
		}

		public static string StressMaterials
		{
			get { return CommonDataStatic.Units == EUnit.Metric ? " MPa" : " ksi"; }
		}
	}
}