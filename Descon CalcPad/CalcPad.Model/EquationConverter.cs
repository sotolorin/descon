using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace CalcPad.Model
{
	/// <summary>
	/// Formats the entered equation and solves to get the result
	/// </summary>
	internal class EquationConverter
	{
		internal EquationLine ConvertEquation(string equation, Dictionary<string, EquationVariable> equationVariableList)
		{
			equation = equation.TrimEnd('=');

			var equationLine = new EquationLine();
			string formattedEquation = equation; // Modified and trimmed version of the original equation

			// Counts the number of ^ in the equation
			var charArray = formattedEquation.ToCharArray();
			int carrotCount = Array.FindAll(charArray, c => c == '^').Count();

			// Replace each instance of entered variables in the formatted equation
			equationVariableList = equationVariableList.OrderByDescending(e => e.Key.Length).ToDictionary(e => e.Key, e => e.Value);
			foreach (var equationVariable in equationVariableList)
				formattedEquation = formattedEquation.Replace(equationVariable.Key, equationVariable.Value.Value.ToString());

			// For each ^ in the equation, that math segment is calculated. Each pass eliminates the previous ^
			for(int i = 0; i < carrotCount; i++)
				formattedEquation = ConvertExponentToValue(formattedEquation);

			// Replace max and min temporarily so the trimming section doesn't remove them
			formattedEquation = Regex.Replace(formattedEquation, "x", "*", RegexOptions.IgnoreCase);

			// Removes all characters except for the ones in the list. These are the only ones supported by Compute()
			charArray = formattedEquation.ToCharArray();
			charArray = Array.FindAll(charArray, (c => (char.IsNumber(c)
			                                || c == '.'
			                                || c == '('
			                                || c == ')'
			                                || c == '+'
			                                || c == '-'
			                                || c == '/'
			                                || c == '*')));

			formattedEquation = new string(charArray);

			if (formattedEquation.Length > 0)
			{
				// Remove any excess characters after the last number
				while (formattedEquation[formattedEquation.Length - 1] != ')' && !char.IsNumber(formattedEquation[formattedEquation.Length - 1]))
					formattedEquation = formattedEquation.Remove(formattedEquation.Length - 1);

				equationLine.Result = new DataTable().Compute(formattedEquation, null);
				equationLine.Equation = equation + " = " + formattedEquation + " = " + equationLine.Result;
			}
			else
				equationLine.Equation = equation;

			return equationLine;
		}

		private string ConvertExponentToValue(string equation)
		{
			equation = equation.Replace(" ", string.Empty);

			bool moreDigitsToFind = true;
			string exponent = string.Empty;
			string root = string.Empty;
			int carrotIndex = equation.IndexOf('^');
			int counter = carrotIndex + 1;

			// Find the digits right of the ^ that make up the exponent
			while (moreDigitsToFind)
			{
				if (counter < equation.Length && char.IsNumber(equation[counter]))
				{
					exponent += equation[counter];
					counter++;
				}
				else
					moreDigitsToFind = false;
			}

			counter = carrotIndex - 1;
			moreDigitsToFind = true;

			// Find the digits left of the ^ that make up the root. These are found in reverse order.
			while (moreDigitsToFind)
			{
				if (counter >= 0 && char.IsNumber(equation[counter]))
				{
					root += equation[counter];
					counter--;
				}
				else
					moreDigitsToFind = false;
			}

			// Reverses the root because we read the values in reverse order
			char[] rootArray = root.ToCharArray();
			root = new string(rootArray.Reverse().ToArray());

			return equation.Replace(root + "^" + exponent, Math.Pow(int.Parse(root), int.Parse(exponent)).ToString());
		}
	}
}