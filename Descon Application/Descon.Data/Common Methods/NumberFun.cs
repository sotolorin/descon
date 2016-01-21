using System;

namespace Descon.Data
{
	/// <summary>
	/// Miscellaneous number conversion methods used throughout the application
	/// </summary>
	public static class NumberFun
	{
		/// <summary>
		/// Calculates the angle value from the Rise and Run values
		/// </summary>
		public static double AngleFromRiseAndRun(EMemberType memberType)
		{
			double angle = 0;
			double rise = CommonDataStatic.DetailDataDict[memberType].SlopeRise;
			double run = CommonDataStatic.DetailDataDict[memberType].SlopeRun;

			switch (memberType)
			{
				case EMemberType.PrimaryMember:
					angle = 90;
					break;
				case EMemberType.RightBeam:
					angle = 0;
					break;
				case EMemberType.LeftBeam:
					angle = 180;
					break;
				case EMemberType.UpperRight:
					angle = Math.Atan(Math.Abs(rise / run)) / ConstNum.RADIAN;
					break;
				case EMemberType.LowerRight:
					angle = 360 - Math.Atan(Math.Abs(rise / run)) / ConstNum.RADIAN;
					break;
				case EMemberType.UpperLeft:
					angle = 180 - Math.Atan(Math.Abs(rise / run)) / ConstNum.RADIAN;
					break;
				case EMemberType.LowerLeft:
					angle = 180 + Math.Atan(Math.Abs(rise / run)) / ConstNum.RADIAN;
					break;
			}

			if (angle == 0 && MiscMethods.IsBrace(memberType))
				angle = 45;

			return angle;
		}

		/// <summary>
		/// Rounds a number with custom precision
		/// </summary>
		/// <param name="number">Number to round</param>
		/// <param name="roundingPrecision">Ignored for Metric. Precision to round to. For example: 16 - upper 1/16, 2 - upper .5</param>
		public static double Round(double number, double roundingPrecision)
		{
			if (CommonDataStatic.Units == EUnit.Metric)
				return Math.Ceiling(number);
			else
				return Math.Ceiling(roundingPrecision * number) / roundingPrecision;
		}

		/// <summary>
		/// Rounds a number to a specific precision and with a specific style, such as round up or round down
		/// </summary>
	    public static double Round(double number, ERoundingPrecision precision, ERoundingStyle style)
	    {
	        double roundedNumber;
	        int precisionReciprocal;
	        double precisionDecimal;

	        switch (precision)
	        {
	            case ERoundingPrecision.WholeNumber:
	                precisionReciprocal = 1;
	                break;
                case ERoundingPrecision.Half:
                    precisionReciprocal = 2;
                    break;
                case ERoundingPrecision.Fourth:
                    precisionReciprocal = 4;
                    break;
                case ERoundingPrecision.Eighth:
                    precisionReciprocal = 8;
                    break;
                case ERoundingPrecision.Tenth:
                    precisionReciprocal = 10;
                    break;
                case ERoundingPrecision.Sixteenth:
                    precisionReciprocal = 16;
                    break;
                case ERoundingPrecision.Hundredth:
                    precisionReciprocal = 100;
                    break;
                default:
	                precisionReciprocal = 1;
	                break;
	        }

	        precisionDecimal = 1.0 / precisionReciprocal;

            switch (style)
	        {
	            case ERoundingStyle.Nearest:
	                double y, decimalPortionY;
	                int q;

	                y = number/precisionDecimal;
	                decimalPortionY = y >= 0.0 ? (y - Math.Floor(y)) : (y - Math.Ceiling(y));
	                q = Math.Abs(decimalPortionY) >= 0.5 
                        ? (int) Round(y, ERoundingPrecision.WholeNumber, ERoundingStyle.TowardsInfinity) 
                        : (int) Round(y, ERoundingPrecision.WholeNumber, ERoundingStyle.TowardsZero);

	                roundedNumber = q * precisionDecimal;
                    
	                break;
                case ERoundingStyle.RoundUp:
	                roundedNumber = Math.Ceiling(precisionReciprocal * number) / precisionReciprocal;
	                break;
                case ERoundingStyle.RoundDown:
                    roundedNumber = Math.Floor(precisionReciprocal * number) / precisionReciprocal;
	                break;
                case ERoundingStyle.TowardsZero:
	                if (number > 0.0)
                        roundedNumber = Math.Floor(precisionReciprocal * number) / precisionReciprocal;
                    else if (number < 0.0)
                        roundedNumber = Math.Ceiling(precisionReciprocal * number) / precisionReciprocal;
                    else roundedNumber = number;
	                break;
                case ERoundingStyle.TowardsInfinity:
                    if (number > 0.0)
                        roundedNumber = Math.Ceiling(precisionReciprocal * number) / precisionReciprocal;
                    else if (number < 0.0)
                        roundedNumber = Math.Floor(precisionReciprocal * number) / precisionReciprocal;
                    else roundedNumber = number;
	                break;
                default:
	                roundedNumber = 1;
	                break;
	        }
	        return roundedNumber;
	    }

		/// <summary>
		/// Takes an int and returns the value over 16. Automatic conversion if metric.
		/// </summary>
		public static double ConvertFromFraction(double number)
		{
			if (CommonDataStatic.Units == EUnit.Metric)
				return Round(25.4 * number / 16, ERoundingPrecision.WholeNumber, ERoundingStyle.Nearest);
			else
				return number / 16;
		}

		/// <summary>
		/// Returns the double equivalent to the fraction passed in
		/// </summary>
		/// <param name="number">Numerator</param>
		/// <param name="denominator">Denominator to divide by</param>
		public static double ConvertFromFraction(double number, int denominator)
		{
			if (CommonDataStatic.Units == EUnit.Metric)
				return Round(25.4 * number / denominator, 2);
			else
				return number / denominator;
		}

		/// <summary>
		/// Converts the moment value between US and Metric. Requires some special rounding.
		/// </summary>
		public static double ConvertMoment(double moment)
		{
			if (CommonDataStatic.Units == EUnit.Metric)
			{
				moment *= ConstNum.MOMENT_CONVERSION;
				moment = Round(moment, ERoundingPrecision.WholeNumber, ERoundingStyle.Nearest);
				moment = moment - (moment % 100);
				return moment;
			}
			else
			{
				moment *= ConstNum.MOMENT_CONVERSION;
				moment = Round(moment, ERoundingPrecision.WholeNumber, ERoundingStyle.Nearest);
				return moment;
			}
		}
	}
}