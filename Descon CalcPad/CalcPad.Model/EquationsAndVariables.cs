using System.Collections.Generic;

namespace CalcPad.Model
{
	public sealed class EquationsAndVariables
	{
		private readonly EquationConverter _equationConverter = new EquationConverter();

		public EquationsAndVariables()
		{
			EquationLineList = new List<EquationLine>();
			EquationVariableDict = new Dictionary<string, EquationVariable>();
		}

		public List<EquationLine> EquationLineList { get; set; }
		public Dictionary<string, EquationVariable> EquationVariableDict { get; set; }

		public void SolveAndFormatEquations()
		{
			foreach (var equation in EquationLineList)
			{
				EquationLine equationLine;

				if (equation.Equation.EndsWith("="))
				{
					equationLine = _equationConverter.ConvertEquation(equation.Equation, EquationVariableDict);
					equation.Equation = equationLine.Equation;
					equation.Result = equationLine.Result;
				}
			}
		}
	}
}
