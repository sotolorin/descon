using System;
using System.Collections.Generic;
using System.Linq;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class EccentricWeld
	{
		/// <summary>
		/// Uses values from AISC lookup tables to find the coeffecient value
		/// </summary>
		internal static double GetEccentricWeld(double k, double a, double theta, bool enableReporting)
		{
			var coeffs = CommonDataStatic.EccentricWeldCoefficients;
			double angle = (theta * 180) / Math.PI;
			double c;
			double x1;
			double x2;

			const int NUMBER_OF_K_VALUES = 16;

			var aValueLookUpDict = GetAValueDictionary();
			var kValueLookUpDict = GetKValueDictionary();
			var angleValueLookUpDict = GetAngleDictionary();

			// a is the vertical column, k is the horizontal column
			// Each angle has its own a by k table

			// The coefficient list is a list of lists (I heard you like lists). First we need to find which angle section. There are 16
			// lists of a values per angle, so we multiply by 16 to get the base angle section. Then we add the k value to get the correct
			// list of the 16. Now we are pointing to a list of a values. We simply index the correct a value to get the final constant.

			// Direct match is an easy lookup
			if (angleValueLookUpDict.ContainsKey(angle) && aValueLookUpDict.ContainsKey(a) && kValueLookUpDict.ContainsKey(k))
				c = coeffs[angleValueLookUpDict[angle] * 16 + kValueLookUpDict[k]][aValueLookUpDict[a]];
			else if (angle >= 90) // For angles greater than or equal 90 degrees, we can use these equations. There are no tables above 75 degrees.
			{
				if (CommonDataStatic.LShapedWeld)
					c = (2.09 + 1.39 * k);
				else
					c = (2.09 + 2.78 * k);
			}
			else // Without a direct match or with an angle less than 90 degrees, we must interpolate the values
			{
				// These are the limit values for when a or k is not exactly one of the index keys
				double aLower = 0;
				double kLower = 0;
				double aUpper = 0;
				double kUpper = 0;

				// First find the correct angle to use. Rounds down to the nearest 15 degree increment
				if (!angleValueLookUpDict.ContainsKey(angle))
				{
					for (int i = 0; i < angleValueLookUpDict.Count; i++)
					{
						if (angle >= angleValueLookUpDict.ElementAt(i).Key && angle < angleValueLookUpDict.ElementAt(i + 1).Key)
						{
							angle = angleValueLookUpDict.ElementAt(i).Key;
							i = int.MaxValue - 1; // kills the loop
						}
					}
				}

				// Now find the upper and lower range for the a value
				if (aValueLookUpDict.ContainsKey(a))
					aLower = aUpper = a;
				else
				{
					for (int i = 0; i < aValueLookUpDict.Count; i++)
					{
						if (a >= aValueLookUpDict.ElementAt(i).Key && a < aValueLookUpDict.ElementAt(i + 1).Key)
						{
							aLower = aValueLookUpDict.ElementAt(i).Key;
							aUpper = aValueLookUpDict.ElementAt(i + 1).Key;
							i = int.MaxValue - 1; // kills the loop
						}
					}
				}

				// Now find the upper and lower range for the k value
				if (kValueLookUpDict.ContainsKey(k))
					kLower = kUpper = k;
				else
				{
					for (int i = 0; i < kValueLookUpDict.Count; i++)
					{
						if (k >= kValueLookUpDict.ElementAt(i).Key && k < kValueLookUpDict.ElementAt(i + 1).Key)
						{
							kLower = kValueLookUpDict.ElementAt(i).Key;
							kUpper = kValueLookUpDict.ElementAt(i + 1).Key;
							i = int.MaxValue - 1; // kills the loop
						}
					}
				}

				// Interpolation
				double aLower_kLower = coeffs[angleValueLookUpDict[angle] * NUMBER_OF_K_VALUES + kValueLookUpDict[kLower]][aValueLookUpDict[aLower]];
				double aUpper_kUpper = coeffs[angleValueLookUpDict[angle] * NUMBER_OF_K_VALUES + kValueLookUpDict[kUpper]][aValueLookUpDict[aUpper]];

				double aUpper_kLower = coeffs[angleValueLookUpDict[angle] * NUMBER_OF_K_VALUES + kValueLookUpDict[kLower]][aValueLookUpDict[aUpper]];
				double aLower_kUpper = coeffs[angleValueLookUpDict[angle] * NUMBER_OF_K_VALUES + kValueLookUpDict[kUpper]][aValueLookUpDict[aLower]];

				if (aUpper != aLower)
				{
					x1 = aUpper_kLower + (a - aUpper) * (aUpper_kLower - aLower_kLower) / (aUpper - aLower);
					x2 = aUpper_kUpper + (a - aUpper) * (aUpper_kUpper - aLower_kUpper) / (aUpper - aLower);
				}
				else
				{
					x1 = aUpper_kLower + (a - aUpper) * (aUpper_kLower - aLower_kLower);
					x2 = aUpper_kUpper + (a - aUpper) * (aUpper_kUpper - aLower_kUpper);
				}

				c = (x1 + 10 * (k - kLower) * (x2 - x1));
			}

			c *= ConstNum.WELD_CONVERSION;

			if (enableReporting)
			{
				Reporting.AddHeader("Eccentric Weld");
				Reporting.AddLine("k = " + k + ", a = " + a + ", Theta = " + ((theta * 180) / Math.PI));
				Reporting.AddLine(ConstString.PHI + " C = " + c);
			}

			return c;
		}

		/// <summary>
		///a index value used to look up final value in complete coefficient list. Vertical column in AISC table.
		/// </summary>
		private static Dictionary<double, int> GetAValueDictionary()
		{
			return new Dictionary<double, int>
			{
				{0.00, 0},
				{0.10, 1},
				{0.15, 2},
				{0.20, 3},
				{0.25, 4},
				{0.30, 5},
				{0.40, 6},
				{0.50, 7},
				{0.60, 8},
				{0.70, 9},
				{0.80, 10},
				{0.90, 11},
				{1.00, 12},
				{1.20, 13},
				{1.40, 14},
				{1.60, 15},
				{1.80, 16},
				{2.00, 17},
				{2.20, 18},
				{2.40, 19},
				{2.60, 20},
				{2.80, 21},
				{3.00, 22}
			};
		}

		/// <summary>
		/// k index value used to look up final value in complete coefficient list. Horizontal column in AISC table
		/// </summary>
		private static Dictionary<double, int> GetKValueDictionary()
		{
			return new Dictionary<double, int>
			{
				{0.0, 0},
				{0.1, 1},
				{0.2, 2},
				{0.3, 3},
				{0.4, 4},
				{0.5, 5},
				{0.6, 6},
				{0.7, 7},
				{0.8, 8},
				{0.9, 9},
				{1.0, 10},
				{1.2, 11},
				{1.4, 12},
				{1.6, 13},
				{1.8, 14},
				{2.0, 15}
			};
		}

		/// <summary>
		/// angle index value used to look up final value in complete coefficient list. Determines which AISC table to use.
		/// </summary>
		private static Dictionary<double, int> GetAngleDictionary()
		{
			return new Dictionary<double, int>
			{
				{0, 0},
				{15, 1},
				{30, 2},
				{45, 3},
				{60, 4},
				{75, 5}
			};
		}
	}
}