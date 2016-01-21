using System;

namespace Descon.Data
{
	/// <summary>
	/// Number constants that are commonly used mostly in the calculations. Some change according to the unit system
	/// </summary>
	public static class ConstNum
	{
		// Declare some bools to make ASD/LRFD and US/Metric checks quick and consise.
		private static bool IsMetric
		{
			get { return CommonDataStatic.Units == EUnit.Metric; }
		}

		private static bool IsASD
		{
			get { return CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD; }
		}

		private static bool IsFema350
		{
			get { return CommonDataStatic.IsFema; }
		}

		#region Conversion Factors

		/// <summary>
		/// Multiplier to convert inch values to metric mainly used for Unity. In Metric mode, Unity will actually be using the
		/// US shape and then converting the values.
		/// </summary>
		public static double METRIC_MULTIPLIER
		{
			get { return IsMetric ? .03937 : 1; }
		}

		/// <summary>
		/// Multiplier that converts between between inches and mm
		/// </summary>
		public static double LENGTH_CONVERSION
		{
			get { return IsMetric ? 25.4 : .03937; }
		}
	
		/// <summary>
		/// Multiplier that converts between kips and kN's. Conversion value is determined by the current Unit system.
		/// </summary>
		public static double KIP_KN_CONVERSION
		{
			get { return IsMetric ? 4.448 : 0.225; }
		}

		/// <summary>
		/// Multiplier that converts between k-ft and kN-m. Conversion value is determined by the current Unit system.
		/// </summary>
		public static double KFT_KNM_CONVERSION
		{
			get { return IsMetric ? 1.356 : 0.738; }
		}

		/// <summary>
		/// Multiplier that converts between US and Metric moment values.
		/// </summary>
		public static double MOMENT_CONVERSION
		{
			get { return IsMetric ? 113.09523 : 0.00884; }
		}

		#endregion

		#region Common Numerical Values

		/// <summary>
		/// Returns 8 / 3 in double form (8.0 / 3.0)
		/// </summary>
		public const double EIGHT_THIRDS = 8.0 / 3.0;

		/// <summary>
		/// I don't know what this is, but it was used in the old Descon. = 223
		/// </summary>
		public const double BETAS = 223;

		/// <summary>
		/// This is used as a conversion from Degrees to Radians. It does not equal 1 radian, but 1 degree in radians.
		/// </summary>
		public const double RADIAN = Math.PI / 180;

		/// <summary>
		/// This is a tolerance factor used in some engineering checks. Adds %2 to original value.
		/// </summary>
		public const double TOLERANCE = 1.02;

		#endregion

		#region Engineering Constants

		public static double WELD_CONVERSION
		{
			get
			{
				if (IsMetric)
					return IsASD ? 1 / 0.75 * (1 / 2.0) * 109.7 / 16 : 109.7 / 16;
				else
					return IsASD ? 1 / 0.75 * (1 / 2.0) * 1 : 1;
			}
		}

		public static double FIOMEGA0_9N
		{
			get { return (IsASD && !IsFema350) ? (1 / 1.67) : 0.9; }
		}

		public static double FIOMEGA0_75N
		{
			get { return (IsASD && !IsFema350) ? (1 / 2.0) : 0.75; }
		}

		public static double FIOMEGA1_0N
		{
			get { return (IsASD && !IsFema350) ? (1 / 1.5) : 1.0; }
		}

		public static double FIOMEGA0_95N
		{
			get { return (IsASD && !IsFema350) ? (1 / 1.58) : 0.95; }
		}

		public static double ELASTICITY		// DESGEN.YMEn
		{
			get { return IsMetric ? 200000 : 29000; }
		}

		#endregion

		#region Preset Large Values

		public static double COEFFICIENT_ONE_THOUSAND	//Katsayi1000000N
		{
			get { return IsMetric ? 1000 : 12; }
		}

        public static double COEFFICIENT_90				//Katsayi90N
        {
            get { return IsMetric ? 621 / 1000.0 : 90; }
        }

        public static double COEFFICIENT_113			//Katsayi113N
        {
            get { return IsMetric ? 779 / 1000.0 : 113; }
        }

	    public static double COEFFICIENT_117			//Katsayi117N
	    {
            get { return IsMetric ? 807 : 117; }
	    }

        public static double COEFFICIENT_147			//Katsayi147N
        {
            get { return IsMetric ? 1010 : 147; }
        }

		#endregion

		#region Preset Inch Values

		public static double SIXTEENTH_INCH
		{
			get { return IsMetric ? 2 : 0.0625; }
		}

		public static double EIGHTH_INCH
		{
			get { return IsMetric ? 3 : 0.125; }
		}

		public static double THREE_SIXTEENTHS
		{
			get { return IsMetric ? 5 : 0.1875; }
		}

		public static double QUARTER_INCH							// CeyrekInch
		{
			get { return IsMetric ? 6 : 0.25; }
		}

		public static double FIVE_SIXTEENTHS
		{
			get { return IsMetric ? 8 : 0.3125; }
		}

		public static double THREE_EIGHTS_INCH
		{
			get { return IsMetric ? 10 : 0.375; }
		}

		public static double HALF_INCH								// YarimInch
		{
			get { return IsMetric ? 13 : 0.5; }
		}

		public static double FIVE_EIGHTS_INCH
		{
			get { return IsMetric ? 16 : 0.625; }
		}

		public static double SEVEN_EIGHTS_INCH
		{
			get { return IsMetric ? 22 : 0.875; }
		}

		public static double ONE_INCH								// BirInch
		{
			get { return IsMetric ? 25 : 1.0; }
		}

		public static double NINE_EIGHTS
		{
			get { return IsMetric ? 29 : 1.125; }
		}

		public static double ONEANDHALF_INCHES						// BirBucukInch
		{
			get { return IsMetric ? 38 : 1.5; }
		}

		public static double ONEANDFIVE_EIGHTS_INCH
		{
			get { return IsMetric ? 42 : 1.625; }
		}

		public static double TWO_INCHES								// IkiInch
		{
			get { return IsMetric ? 51 : 2.0; }
		}

		public static double TWOANDHALF_INCHES						// IkiBucukInch
		{
			get { return IsMetric ? 64 : 2.5; }
		}

		public static double THREE_INCHES							// UcInch
		{
			get { return IsMetric ? 75 : 3.0; }
		}

		public static double THREEANDHALF_INCHES					// UcBucukInch
		{
			get { return IsMetric ? 89 : 3.5; }
		}

		public static double FOUR_INCHES							// DortInch
		{
			get { return IsMetric ? 102 : 4.0; }
		}

		public static double FIVE_INCHES							// BesInch
		{
			get { return IsMetric ? 127 : 5.0; }
		}

		public static double FIVEANDHALF_INCHES						// BesBucukInch
		{
			get { return IsMetric ? 140 : 5.5; }
		}

		public static double SEVENANDHALF_INCHES					// YediBucukInch
		{
			get { return IsMetric ? 191 : 7.5; }
		}

		#endregion
	}
}