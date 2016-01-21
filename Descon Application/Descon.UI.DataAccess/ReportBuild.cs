using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Descon.Data;

namespace Descon.UI.DataAccess
{
	public class ReportBuild
	{
		private const string LINE_BREAK = "</BR>";
		private const string HEADER_CLASS = " class=\"header\"";
		private const string NG_CLASS = " class=\"ng\"";
		private const string OK_CLASS = " class=\"ok\"";
		private ReportSettings _reportSettings;

		/// <summary>
		/// Formats the lines of the report into HTML
		/// </summary>
		/// <param name="addHeader">Optional parameter to skip the header.</param>
		public string BuildReport(bool addHeader = true)
		{
			_reportSettings = CommonDataStatic.Preferences.ReportSettings;

			if (CommonDataStatic.DetailDataDict.All(d => !d.Value.IsActive))
				return string.Empty;

			var body = new StringBuilder();
			string jointConfigString = CreateJointConfigText();
			int lineNumber = 0;
			string lineNumberString; // Used as the representation for line number as ID
			int totalLineCount = CommonDataStatic.DetailReportLineList.Count();

			CommonDataStatic.ReportGoToList = new Dictionary<string, string>();
			CommonDataStatic.ReportCapacityList = new List<string>();
			CommonDataStatic.ReportNoGoodList = new List<string>();

			// Determines which line subsequent columns begin at
			int newColumnLineNumber = totalLineCount / _reportSettings.NumberOfColumns;
			if (totalLineCount % _reportSettings.NumberOfColumns != 0)
				newColumnLineNumber++;

			int columnWidthPercentage = 100 / _reportSettings.NumberOfColumns;

			body.Append(BuildHead());

			if (addHeader)
				body.Append(BuildHeaderTable());

			if (_reportSettings.ShowDrawingAtTop)
			{
				body.Append(LINE_BREAK);
				body.Append(BuildDrawingTable());
			}

			if (!_reportSettings.ShowCalculations)
				return body.ToString();
			
			// \r is igrnored by the HTML, but useful when opening the HTML as text to debug
			body.Append("<h3>BASIC DETAILS OVERVIEW</h3>\r");
			body.Append("<div><b>Joint Configuration: </b>" + jointConfigString + "</div>\r");
			body.Append(LINE_BREAK);

			foreach (var member in CommonDataStatic.DetailDataDict.Where(m => m.Value.ShapeName != ConstString.NONE))
			{
				var memberData = member.Value;
				body.Append("<div><b>Member: </b>" + CommonDataStatic.CommonLists.CompleteMemberList.First(c => c.Key == memberData.MemberType).Value + "</div>\r");
				body.Append("<div><b>Section: </b>" + CommonDataStatic.AllShapes.First(c => c.Key == memberData.ShapeName).Value.Name + "</div>\r");
				body.Append("<div><b>Material: </b>" + CommonDataStatic.MaterialDict.First(c => c.Key == memberData.Material.Name).Value.Name + "</div>\r");
				body.Append(LINE_BREAK);
			}

			body.Append("<h3>DETAILED CALCULATION REPORT</h3>\r");

			// This is the table around the entire calc portion of the report. Allows the user to make the report multiple columns.
			body.Append("<table>\r");
			body.Append("<tr>\r");
			body.Append("<td class=\"calcs\" width=\"" + columnWidthPercentage + "%\">\r");

			// This logic formats all decimal numbers to two places after the decimal place. Whole numbers will be unaffected.
			foreach (var line in CommonDataStatic.DetailReportLineList) // Parse through each line output by the calculations
			{
				int decimalCounter = 0; // Used to determine how far we are into the decimal portion of number
				bool haveDecimal = false; // Determines if we have a decimal yet

				line.LineNumber = ++lineNumber; // Increments the line number and sets the value
				lineNumberString = lineNumber.ToString().PadLeft(5, '0');

				// Close the previous column and begin a new one
				if (lineNumber % newColumnLineNumber == 0 && lineNumber != totalLineCount)
				{
					body.Append("</td>");
					body.Append("<td class=\"calcs\" width=\"" + columnWidthPercentage + "%\">");
				}

				if (line.LineString.Contains('.')) // Only run logic if there is a decimal point
				{
					for (int i = 0; i < line.LineString.Length; i++) // Parse through each character in the current line
					{
						char[] character = {line.LineString[i]}; // Creates char[] for the current character. Necessary for the next line
						if (!Regex.IsMatch(new string(character), @"[0-9]")) // Checks to see if the current character is a number
						{
							haveDecimal = false; // Reset the haveDecimal flag to show we have a number
							decimalCounter = 0; // Reset the counter so we can start the process again
						}
						if (line.LineString[i] == '.') // Toggle haveDecimal if we have a decimal point
							haveDecimal = true;
						if (haveDecimal && Char.IsDigit(line.LineString[i])) // Increases the counter if we had a decimal and the current character is a number or decimal
							decimalCounter++;

						if (decimalCounter > 4) // If we have more than 4 decimal places, cut everything else off.
						{
							line.LineString = line.LineString.Remove(i, 1);
							i--;
						}
						if (haveDecimal && decimalCounter > 1 && character[0] == '0') // If we have more than 1 decimal place, and we're on a 0, see if we should remove it. (Ex: 1.0000 should be changed to 1.0, but 1.0001 shouldn't be changed to 1.01)
						{
							if (i == line.LineString.Length - 1) // If the 0 is the last char in the line, then remove it
							{
								line.LineString = line.LineString.Remove(i, 1);
								i--;
							}
							else
							{
								char nextChar = line.LineString[i + 1];
								if (!Char.IsDigit(nextChar)) // If the character after the 0 is not a numerical digit, then remove the 0.
								{
									line.LineString = line.LineString.Remove(i, 1);
									i--;
								}
							}
						}
					}
				}

				string lineString = line.LineString;

				// Replaces specific characters with their equivalent HTML character codes so they appear correctly.
				lineString = lineString.Replace("²", "&sup2;");
				lineString = lineString.Replace("³", "&sup3;");
				lineString = lineString.Replace("^0.5", "<sup>^0.5</sup>");	// Turns this into a superscript
				lineString = lineString.Replace("<=", "≤");
				lineString = lineString.Replace(">=", "≥");

				if (_reportSettings.AddLineNumbersToReport && line.LineType != EReportLineType.Header)
					lineString = lineString.Insert(0, "(" + line.LineNumber.ToString("D5") + "): ");

				switch (line.LineType)
				{
					case EReportLineType.Header:
						lineString = AddHeaderLine(lineString, lineNumberString);
						break;
					case EReportLineType.MainHeader:
						lineString = AddGoToHeaderLine(lineString, lineNumberString, true);
						break;
					case EReportLineType.GoToHeader:
						lineString = AddGoToHeaderLine(lineString, lineNumberString, false);
						break;
					case EReportLineType.NormalLine:
						lineString = AddLine(lineString, lineNumberString);
						break;
					case EReportLineType.CapacityLine:
						lineString = AddCapacityLine(lineString, lineNumberString);
						break;
					default:
						lineString = AddLine(lineString, lineNumberString);
						break;
				}

				// This replaces the special OK and NG that we didn't want to trigger the red and green lines with
				lineString = lineString.Replace(ConstString.REPORT_NOGOOD_SPECIAL, " " + ConstString.REPORT_NOGOOD);
				lineString = lineString.Replace(ConstString.REPORT_OK_SPECIAL, " " + ConstString.REPORT_OK);

				lineString = lineString.Replace("|b|", "<span class=\"");
				lineString = lineString.Replace("|e|", "\"></span>");

				if (lineNumber == 1 || lineNumber == newColumnLineNumber)
					lineString = lineString.Replace(LINE_BREAK, string.Empty);

				body.Append(lineString);
				body.Append("\r");
			}

			body.Append("</td>\r");
			body.Append("</tr>\r");
			body.Append("</table>\r");

			if (!_reportSettings.ShowDrawingAtTop)
			{
				body.Append(LINE_BREAK);
				body.Append(BuildDrawingTable());
			}

			body.Append(BuildFooter());
			body.Append(BuildButt());

			return body.ToString();
		}

		// The following methods add HTML anchors that are used to navigate to specific lines through the UI
		/// <summary>
		/// Returns a Bold style line with a line break before it. This type of line will have an indent in the drop down Go To menu
		/// </summary>
		private string AddHeaderLine(string line, string lineNumber)
		{
			line = line.Insert(0, LINE_BREAK + "<div" + HEADER_CLASS + " id=\"" + lineNumber + ConstString.REPORT_NORMAL_LINE + "\">");
			line = line.Insert(line.Length, "</div>");
			return line;
		}

		/// <summary>
		/// Returns a Bold style line with a line break before it. Will not show up in Go To drop down
		/// </summary>
		private string AddGoToHeaderLine(string line, string lineNumber, bool isMainHeader)
		{
			if (isMainHeader)
				CommonDataStatic.ReportGoToList.Add(lineNumber + ConstString.REPORT_NORMAL_LINE, line.TrimEnd(':'));
			else
				CommonDataStatic.ReportGoToList.Add(lineNumber + ConstString.REPORT_NORMAL_LINE, "     " + line.TrimEnd(':'));

			line = line.Insert(0, LINE_BREAK + "<div" + HEADER_CLASS + " id=\"" + lineNumber + ConstString.REPORT_NORMAL_LINE + "\">");
			line = line.Insert(line.Length, "</div>");

			return line;
		}

		/// <summary>
		/// Adds an (NG), (OK) or regular line
		/// </summary>
		private string AddLine(string line, string lineNumber)
		{
			if (line.Contains(ConstString.REPORT_OK))
			{
				line = line.Insert(0, "<div" + OK_CLASS + " id=\"" + lineNumber + ConstString.REPORT_NORMAL_LINE + "\">");
				line = line.Insert(line.Length, "</div>");
			}
			else if (line.Contains(ConstString.REPORT_NOGOOD))
			{
				line = line.Insert(0, "<div" + NG_CLASS + " id=\"" + lineNumber + ConstString.REPORT_NORMAL_LINE + "\">");
				line = line.Insert(line.Length, "</div>");
				CommonDataStatic.ReportNoGoodList.Add(lineNumber + ConstString.REPORT_NORMAL_LINE);
			}
			else if (line == string.Empty)
				line = "<div id=\"" + lineNumber + ConstString.REPORT_NORMAL_LINE + "\">" + LINE_BREAK + "</div>";
			else
			{
				line = line.Insert(0, "<div id=\"" + lineNumber + ConstString.REPORT_NORMAL_LINE + "\">");
				line = line.Insert(line.Length, "</div>");
			}

			return line;
		}

		/// <summary>
		/// This creates a capacity line that is linked from the Capacity Gauges. Doubles as (OK) and (NG)
		/// </summary>
		private string AddCapacityLine(string line, string lineNumber)
		{
			if (line.Contains(ConstString.REPORT_OK))
			{
				line = line.Insert(0, "<div" + OK_CLASS + " id=\"" + lineNumber + ConstString.REPORT_NORMAL_LINE + "\">");
				CommonDataStatic.ReportCapacityList.Add(lineNumber + ConstString.REPORT_NORMAL_LINE);
			}
			else if (line.Contains(ConstString.REPORT_NOGOOD))
			{
				line = line.Insert(0, "<div" + NG_CLASS + " id=\"" + lineNumber + ConstString.REPORT_NORMAL_LINE + "\">");
				CommonDataStatic.ReportCapacityList.Add(lineNumber + ConstString.REPORT_NORMAL_LINE);
				CommonDataStatic.ReportNoGoodList.Add(lineNumber + ConstString.REPORT_NORMAL_LINE);
			}
			else
			{
				line = line.Insert(0, "<div id=\"" + lineNumber + ConstString.REPORT_NORMAL_LINE + "\">");
				CommonDataStatic.ReportCapacityList.Add(lineNumber + ConstString.REPORT_NORMAL_LINE);
			}

			line = line.Insert(line.Length, "</div>");
			return line;
		}

		/// <summary>
		/// Adds the HTML for the HEAD portion of the report. Sets the styles and main font.
		/// </summary>
		private string BuildHead()
		{
			var body = new StringBuilder();

			body.Append("<!DOCTYPE html>\r");
			body.Append("<HTML>\r");
			body.Append("<HEAD>\r");
			body.Append("<META http-equiv='Content-Type' content='text/html; charset=unicode'/>\r");
			body.Append("<META http-equiv='X-UA-Compatible' content='IE=9'/>\r"); // Enables HTML5 content
			body.Append("<title>DETAIL CALCULATION REPORT</title>\r");

			// CSS Styles for the entire document
			body.Append("<STYLE type='text/css'>\r");
			body.Append("* {font-family: \"" + _reportSettings.FontName + "\"; " + "font-size: " + _reportSettings.FontSize + ";}\r");

			// This only affects PDF saving and is required to make the images and headers format correctly
			body.Append("table {border-collapse: collapse;}\r");
			body.Append("table.image {width: 90%; page-break-after: always;  page-break-inside: avoid;}\r");
			body.Append("table.header {width: 95%;}\r");
			body.Append("th, td {padding-top: 2px; padding-bottom: 2px; padding-left: 10px; width: 20%;}\r");
			body.Append("th {border-right: 1px solid rgb(225, 225, 225); text-align: left;}\r");
			body.Append("td.calcs {border: 1px solid rgb(225, 225, 225); text-align: left;}\r");
			
			// Images
			body.Append("img.large {width: 95%; height: auto;}\r");
			body.Append("img.bookmark {height: 16; width: 16; float: right; vertical-align: middle; margin-right: 0.25em}\r");
			
			// Comments
			body.Append("span[contenteditable=\"true\"].comment {outline: 1px solid rgb(225, 225, 225); background-color: #ffffcc; " +
						"font-weight: normal; color: black;}\r");
			body.Append("span.comment {margin-bottom: 0.25em; margin-right: 0.25em; float: right;}\r");
			body.Append("p { margin: 0; }\r"); // Hitting ENTER in a comment creates a <p> tag. This eliminates the extra space around each line.

			// Divs for text lines
			body.Append("div.header {font-weight: bold;}\r");
			body.Append("div.ok {font-weight: bold; color: green;}\r");
			body.Append("div.ng {font-weight: bold; color: red;}\r");
			body.Append("div.highlight {background-color: yellow;}\r");
			body.Append("div.navigation_target {background-color: rgb(136, 195, 225);}\r");

			body.Append("</STYLE>\r");

			body.Append("</HEAD>\r");
			body.Append("<BODY>\r");

			return body.ToString();
		}

		/// <summary>
		/// Adds the bottom of the HTML file to close any of the major tags
		/// </summary>
		private string BuildButt()
		{
			var body = new StringBuilder();

			body.Append("<SCRIPT>\r");
			foreach (var var in CommonDataStatic.ReportJavascriptVarList)
			{
				body.Append("var " + var.Key + " = " + var.Value + ";\r");
				body.Append("var myElements = document.querySelectorAll(\"." + var.Key + "\");\r");
				body.Append("for (var i = 0; i < myElements.length; i++) {\r");
				body.Append("    myElements[i].innerHTML = " + var.Key + ";\r");
				body.Append("}\r");
			}

			// Commented out for public builds since the functionality is not complete
			//body.Append("var spanElements = document.getElementsByTagName(\"span\");\r");
			//body.Append("for (var i = 0; i < spanElements.length; i++) {\r");
			//body.Append("    spanElements[i].onmouseover = function() {mouseOver(this.className)};\r");
			//body.Append("    spanElements[i].onmouseout = function() {mouseOut(this.className)};\r");
			//body.Append("}\r");

			//body.Append("function mouseOver(className) {\r");
			//body.Append("    var myElements = document.querySelectorAll(\".\" + className);\r");
			//body.Append("    for (var i = 0; i < myElements.length; i++)\r");
			//body.Append("        myElements[i].style.backgroundColor = \"lightblue\";\r");
			//body.Append("}\r");

			//body.Append("function mouseOut(className) {\r");
			//body.Append("var myElements = document.querySelectorAll(\".\" + className);\r");
			//body.Append("    for (var i = 0; i < myElements.length; i++) {\r");
			//body.Append("        myElements[i].removeAttribute(\"style\")\r");
			//body.Append("    }\r");
			//body.Append("}\r");

			body.Append("</SCRIPT>\r");

			body.Append("</BODY>\r");
			body.Append("</HTML>");

			return body.ToString();
		}

		private string BuildDrawingTable()
		{
			var table = new StringBuilder();

			if (!_reportSettings.ShowDrawing || _reportSettings.NoViewsSelected)
				return string.Empty;

			if (_reportSettings.CombineDrawingViews && File.Exists(ConstString.FILE_UNITY_IMAGE_ALL))
			{
				table.Append("<table class=\"image\">\r");
				table.Append("<tr>\r");
				table.Append("<td><img class=\"large\" src=\"" + ConstString.FILE_UNITY_IMAGE_ALL + "\"></td>\r");
				table.Append("</tr>\r");
				table.Append("</table>\r");
			}

			if (!_reportSettings.CombineDrawingViews)
			{
				if (_reportSettings.ShowLeftSideView && File.Exists(ConstString.FILE_UNITY_IMAGE_LEFT))
				{
					table.Append("<table class=\"image\">\r");
					table.Append("<tr>\r");
					table.Append("<td><img class=\"large\" src=\"" + ConstString.FILE_UNITY_IMAGE_LEFT + "\"></td>\r");
					table.Append("</tr>\r");
					table.Append("</table>\r");
				}
				if (_reportSettings.ShowRightSideView && File.Exists(ConstString.FILE_UNITY_IMAGE_RIGHT))
				{
					table.Append("<table class=\"image\">\r");
					table.Append("<tr>\r");
					table.Append("<td><img class=\"large\" src=\"" + ConstString.FILE_UNITY_IMAGE_RIGHT + "\"></td>\r");
					table.Append("</tr>\r");
					table.Append("</table>\r");
				}
				if (_reportSettings.ShowTopView && File.Exists(ConstString.FILE_UNITY_IMAGE_TOP))
				{
					table.Append("<table class=\"image\">\r");
					table.Append("<tr>\r");
					table.Append("<td><img class=\"large\" src=\"" + ConstString.FILE_UNITY_IMAGE_TOP + "\"></td>\r");
					table.Append("</tr>\r");
					table.Append("</table>\r");
				}

				if (_reportSettings.ShowFrontView && File.Exists(ConstString.FILE_UNITY_IMAGE_FRONT))
				{
					table.Append("<table class=\"image\">\r");
					table.Append("<tr>\r");
					table.Append("<td><img class=\"large\" src=\"" + ConstString.FILE_UNITY_IMAGE_FRONT + "\"></td>\r");
					table.Append("</tr>\r");
					table.Append("</table>\r");
				}
				if (_reportSettings.Show3DView && File.Exists(ConstString.FILE_UNITY_IMAGE_3D))
				{
					table.Append("<table class=\"image\">\r");
					table.Append("<tr>\r");
					table.Append("<td><img class=\"large\" src=\"" + ConstString.FILE_UNITY_IMAGE_3D + "\"></td>\r");
					table.Append("</tr>\r");
					table.Append("</table>\r");
				}
			}

			return table.ToString();
		}

		/// <summary>
		/// Constructs the header table portion of the report which contains information set by the user. Also used for PDF.
		/// </summary>
		internal string BuildHeaderTable(bool addPDFPageNumber = false)
		{
			if (_reportSettings == null)
				_reportSettings = CommonDataStatic.Preferences.ReportSettings;

			string seismicText = CommonDataStatic.Preferences.Seismic == ESeismic.Seismic ? "Yes" : "No"; 

			var table = new StringBuilder();

			// Table has 5 columns. First column is an image that takes up all rows.
			// Row 1: Title 1, Value 1, Title 6, Value 6
			// Row 2: Title 2, Value 2, Title 7, Value 7
			// Row 3: Title 3, Value 3, Title 8, Value 8
			// Row 4: Title 4, Value 4, Title 9, Value 9
			// Row 5: Units, [Unit System], Method [Steel Code]
			// Row 6: Code, [Calc Mode], Seismic, [Seismic Mode]
			// Row 7: Title 5, Value 5 (Value 5 takes up the remaining columns)
			// Row 8: File Name, [File Name] (File Name takes up the remaining columns)
			table.Append("<table class=\"header\">\r");
			table.Append("<tr>");
			table.Append("<td rowspan=\"8\"><img class=\"large\" src=\"" + ConstString.FILE_LOGO + "\"></td>\r");
			table.Append("<th>" + _reportSettings.Title1 + "</th>");
			table.Append("<td>" + _reportSettings.Value1 + "</td>");
			table.Append("<th>" + _reportSettings.Title6 + "</th>");
			table.Append("<td>" + _reportSettings.Value6 + "</td>");
			table.Append("</tr>\r");
			if (addPDFPageNumber)
			{
				table.Append("<tr>");
				table.Append("<th>" + _reportSettings.Title2 + "</th>");
				table.Append("<td><span class=\"page\"></span></td>");
				table.Append("<th>" + _reportSettings.Title7 + "</th>");
				table.Append("<td><span class=\"topage\"></span></td>");
				table.Append("</tr>\r");
			}
			else
			{
				table.Append("<tr>");
				table.Append("<th>" + _reportSettings.Title2 + "</th>");
				table.Append("<td>" + _reportSettings.Value2 + "</td>");
				table.Append("<th>" + _reportSettings.Title7 + "</th>");
				table.Append("<td>" + _reportSettings.Value7 + "</td>");
				table.Append("</tr>\r");
			}
			table.Append("<tr>");
			table.Append("<th>" + _reportSettings.Title3 + "</th>");
			table.Append("<td>" + _reportSettings.Value3 + "</td>");
			table.Append("<th>" + _reportSettings.Title8 + "</th>");
			table.Append("<td>" + _reportSettings.Value8 + "</td>");
			table.Append("</tr>\r");
			table.Append("<tr>");
			table.Append("<th>" + _reportSettings.Title4 + "</th>");
			table.Append("<td>" + _reportSettings.Value4 + "</td>");
			table.Append("<th>" + _reportSettings.Title9 + "</th>");
			table.Append("<td>" + _reportSettings.Value9 + "</td>");
			table.Append("</tr>\r");
			table.Append("<tr>");
			table.Append("<th>UNITS</th>");
			table.Append("<td>" + CommonDataStatic.Units + "</td>");
			table.Append("<th>CODE</th>");
			table.Append("<td>" + CommonDataStatic.Preferences.SteelCode + "</td>");
			table.Append("</tr>\r");
			table.Append("<tr>");
			table.Append("<th>METHOD</th>");
			table.Append("<td>" + CommonDataStatic.Preferences.CalcMode + "</td>");
			table.Append("<th>SEISMIC</th>");
			table.Append("<td>" + seismicText + "</td>");
			table.Append("</tr>\r");
			table.Append("<tr>");
			table.Append("<th>" + _reportSettings.Title5 + "</th>");
			table.Append("<td colspan=\"3\">" + _reportSettings.Value5 + "</td>");
			table.Append("</tr>\r");
			table.Append("<tr>");
			table.Append("<th>FILE NAME</th>");
			if (_reportSettings.ShowFullFilePathInHeader)
				table.Append("<td colspan=\"4\">" + CommonDataStatic.CurrentFilePath + "</td>");
			else
				table.Append("<td colspan=\"4\">" + CommonDataStatic.CurrentFileName + "</td>");
			table.Append("</tr>\r");
			table.Append("</table>\r");

			return table.ToString();
		}

		private string BuildFooter()
		{
			var footer = new StringBuilder();

			footer.Append(LINE_BREAK);
			footer.Append(CommonDataStatic.LicenseTypeDisplay + " <B>Licensed to:</B> " + CommonDataStatic.CompanyName);
			footer.Append(LINE_BREAK + "\r");

			return footer.ToString();
		}

		/// <summary>
		/// Sets the text for the display of the Joint Config
		/// </summary>
		private string CreateJointConfigText()
		{
			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BraceToColumn:
					return ConstString.JOINT_CONFIG_BRACE_TO_COLUMN;
				case EJointConfiguration.BraceVToBeam:
					return ConstString.JOINT_CONFIG_V_BRACE_TO_BEAM;
				case EJointConfiguration.BraceToColumnBase:
					return ConstString.JOINT_CONFIG_BRACE_TO_COLUMN_BASE;
				case EJointConfiguration.BeamToColumnFlange:
					return ConstString.JOINT_CONFIG_BEAM_TO_COLUMN_FLANGE;
				case EJointConfiguration.BeamToColumnWeb:
					return ConstString.JOINT_CONFIG_BEAM_TO_COLUMN_WEB;
				case EJointConfiguration.BeamToHSSColumn:
					return ConstString.JOINT_CONFIG_BEAM_TO_HSS_COLUMN;
				case EJointConfiguration.ColumnSplice:
					return ConstString.JOINT_CONFIG_COLUMN_SPLICE;
				case EJointConfiguration.BeamToGirder:
					return ConstString.JOINT_CONFIG_BEAM_TO_GIRDER;
				default:
					return ConstString.JOINT_CONFIG_BEAM_SPLICE;
			}
		}
	}
}