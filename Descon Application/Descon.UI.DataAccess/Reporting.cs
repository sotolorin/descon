using Descon.Data;

namespace Descon.UI.DataAccess
{
	/// <summary>
	/// This class is used to set up paragraphs and styles for the report. Each one of the Add methods below adds a line with a
	/// particular style to CommonDataStatic.DetailReportLineList. These styles are then converted to HTML when the report is
	/// generated. These are basically shortcuts to avoid extra long method calls in the calculations.
	/// </summary>
	public static class Reporting
	{
		/// <summary>
		/// Adds a main header type line to CommonDataStatic.DetailReportLineList. Bold with line spacing. Added to Go To menu.
		/// </summary>
		public static void AddMainHeader(string header)
		{
			AddLineToReport(EReportLineType.MainHeader, header);
		}

		/// <summary>
		/// Adds a header type line to CommonDataStatic.DetailReportLineList. Bold with line spacing. NOT added to Go To menu.
		/// </summary>
		public static void AddHeader(string header)
		{
			AddLineToReport(EReportLineType.Header, header);
		}

		/// <summary>
		/// Adds a header type line to CommonDataStatic.DetailReportLineList. Bolt with line spacing. Added to Go To menu.
		/// </summary>
		public static void AddGoToHeader(string header)
		{
			AddLineToReport(EReportLineType.GoToHeader, header);
		}

		/// <summary>
		/// Adds a normal line to CommonDataStatic.DetailReportLineList
		/// </summary>
		public static void AddLine(string line)
		{
			AddLineToReport(EReportLineType.NormalLine, line);
		}

		/// <summary>
		/// Adds a line that is good capacity check (not an error) in bold and green
		/// </summary>
		public static void AddCapacityLine(string line, double capacityValue, string capacityText, EMemberType memberType)
		{
			AddLineToReport(EReportLineType.CapacityLine, line);
			CommonDataStatic.GaugeData.Add(new GaugeData
			{
				CapacityDescription = MiscMethods.GetComponentName(memberType) + " - " + capacityText,
				CapacityValue = capacityValue,
			});
		}

		public static void AddFormulaLine()
		{
			AddLine("The following formulae have been derived using an interaction equation of the form:");
			AddLine("ft / Ft + (fv / Fv)² = 1");
			AddLine("Ref: \"Combined Shear and Tension Stress\", Subhash - C. Goel, Engineering Journal, 3rd Q 1986, AISC");
			AddLine(string.Empty);
		}

		/// <summary>
		/// Adds the actual line to the report file
		/// </summary>
		private static void AddLineToReport(EReportLineType lineType, string text)
		{
			var reportLine = new ReportLine
			{
				LineType = lineType,
				LineString = text
			};

			CommonDataStatic.DetailReportLineList.Add(reportLine);
		}
	}
}