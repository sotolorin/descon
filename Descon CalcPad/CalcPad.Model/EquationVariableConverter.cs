using System.Collections.Generic;
using System.Linq;
using CalcPad.Resources;

namespace CalcPad.Model
{
	public class EquationVariableConverter
	{
		public KeyValuePair<string, EquationVariable> ConvertText(string text)
		{
			string variableName = string.Empty;
			double value = 0;
			EUnitType unitType = EUnitType.Length;

			string[] textArray = text.Split('=');
			if (textArray.Count() > 1)
			{
				variableName = textArray[0].Trim();
				value = double.Parse(textArray[1]);
			}

			return new KeyValuePair<string, EquationVariable>(variableName, new EquationVariable {UnitType = unitType, Value = value});
		}
	}
}
