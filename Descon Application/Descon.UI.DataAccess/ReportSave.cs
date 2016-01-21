using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using Descon.Data;
using Color = System.Drawing.Color;
using Cursors = System.Windows.Input.Cursors;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace Descon.UI.DataAccess
{
	// Third party PDF converter: WKHTMLTOPDF - http://wkhtmltopdf.org/index.html

	/// <summary>
	/// Gives the user the option to save the report in a variety of formats
	/// </summary>
	public class ReportSave
	{
		/// <summary>
		/// Alternate report save method that takes the HTML and a filePath parameter. If the filePath is set, the report will
		/// automatically save without user interaction.
		/// If projectManagerPrint == true then we have a few special things to do. 
		/// </summary>
		public void SaveReport(string html, string filePath, CommonData data, bool projectManagerReport)
		{
			var saveDialog = new SaveFileDialog();

			if (filePath == string.Empty)
			{
				saveDialog = new SaveFileDialog();
				saveDialog.AddExtension = true;
				saveDialog.Filter = "PDF (*.pdf)|*.pdf|" +
				                    "HTML (*.html)|*.html";
				if (!projectManagerReport)
				{
					saveDialog.Filter += "|Rich Text File (*.rtf) (Some formatting and header will be lost)|*.rtf|" +
					                     "Text File (*.txt) (Formatting will be lost)|*.txt";
					switch (CommonDataStatic.Preferences.ReportSettings.DefaultSaveFileFormat)
					{
						case EReportFileTypes.PDF:
							saveDialog.FilterIndex = 1;
							break;
						case EReportFileTypes.HTML:
							saveDialog.FilterIndex = 2;
							break;
						case EReportFileTypes.RTF:
							saveDialog.FilterIndex = 3;
							break;
						case EReportFileTypes.TXT:
							saveDialog.FilterIndex = 4;
							break;
					}
				}
				else
					saveDialog.FilterIndex = 0;

				saveDialog.OverwritePrompt = true;
				saveDialog.Title = "Save Report";

				saveDialog.FileName = ConstString.FILE_DETAIL_REPORT;
				filePath = ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + ConstString.FILE_DETAIL_REPORT;

				if (saveDialog.ShowDialog() != DialogResult.OK)
					return;
			}

			if (!String.IsNullOrEmpty(saveDialog.FileName))
				filePath = saveDialog.FileName;
			if (filePath == string.Empty)
				return;

			var richTextBox = new RichTextBox();

			// For RTF and TXT files we need to put the text in a hidden RichTextBox control and save from that control
			if (filePath.EndsWith(".txt") || filePath.EndsWith(".rtf"))
			{
				foreach (var line in CommonDataStatic.DetailReportLineList)
				{
					foreach (var var in CommonDataStatic.ReportJavascriptVarList)
					{
						if (line.LineString.Contains(var.Key))
							line.LineString = line.LineString.Replace(var.Key, var.Value.ToString());
					}

					line.LineString = line.LineString.Replace("|b|", string.Empty);
					line.LineString = line.LineString.Replace("|e|", string.Empty);

					string newLine = string.Empty;
					// Add one extra new line after a header and two if there is a blank line
					if (IsReportHeaderLineType(line.LineType))
						newLine = "\r";
					// Makes the line bold or colored if necessary
					if (line.LineString.Contains(ConstString.REPORT_OK) || line.LineString.Contains(ConstString.REPORT_NOGOOD) ||
						IsReportHeaderLineType(line.LineType))
						richTextBox.SelectionFont = new Font(CommonDataStatic.Preferences.ReportSettings.FontName, 10, FontStyle.Bold);
					if (line.LineString.Contains(ConstString.REPORT_OK))
						richTextBox.SelectionColor = Color.Green;
					else if (line.LineString.Contains(ConstString.REPORT_NOGOOD))
						richTextBox.SelectionColor = Color.Red;

					richTextBox.AppendText(newLine + line.LineString + "\r");

					// Reset everything for the next line
					richTextBox.SelectionFont = new Font(CommonDataStatic.Preferences.ReportSettings.FontName, 10, FontStyle.Regular);
					richTextBox.ForeColor = Color.Black;
				}
			}

			if (filePath.EndsWith(".txt"))
				richTextBox.SaveFile(filePath, RichTextBoxStreamType.PlainText);
			else if (filePath.EndsWith(".rtf"))
				richTextBox.SaveFile(filePath, RichTextBoxStreamType.RichText);
			else if (filePath.EndsWith(".html"))
				File.WriteAllText(filePath, html);
			else if (filePath.EndsWith(".pdf")) // Uses third party command line utility
			{
				Mouse.OverrideCursor = Cursors.Wait;

				if (!projectManagerReport && data != null)
				{
					// If we are showing the header on every page, we don't want the one displayed at the top of the report
					if (CommonDataStatic.Preferences.ReportSettings.PDFShowHeaderEveryPage)
						html = new ReportBuild().BuildReport(false);
				}

				// Save out the HTML file
				File.WriteAllText(ConstString.FILE_REPORT_HTML, html);

				SaveHeaderForPDFPages();

				// Set up each command parameter for the utility
				string commandParameter = "-q"; // Quiet mode reducing the amount of text in the command window
				commandParameter += " --page-size \"Letter\""; // Sets paper size to 8.5 X 11
				if (CommonDataStatic.Preferences.ReportSettings.PDFShowHeaderEveryPage)
					commandParameter += " --header-html " + ConstString.FILE_PDFHEADER_HTML; // Header file in HTML format

				//commandParameter += " --margin-bottom 10mm";
				//commandParameter += " --margin-left 10mm";
				//commandParameter += " --margin-right 10mm";
				//commandParameter += " --margin-top 10mm";

				// Set up the file paths. First is the location of the HTML file, second is the file path the user chose to save
				string parameterFilePaths = " \"" + ConstString.FILE_REPORT_HTML + "\" \"" + filePath + "\"";

				// Creates a process and sets the command window to be hidden so it doesn't flash up while saving
				var process = new Process();
				var startInfo = new ProcessStartInfo
				{
					WindowStyle = ProcessWindowStyle.Hidden,
					FileName = ConstString.FILE_HTML_TO_PDF_UTILITY,
					Arguments = commandParameter + parameterFilePaths
				};

				process.StartInfo = startInfo;

				// Start the process and wait until it finishes before continuing
				process.Start();
				process.WaitForExit();

				Mouse.OverrideCursor = Cursors.Arrow;
			}
		}

		private void SaveHeaderForPDFPages()
		{
			var header = new StringBuilder();

			var reportSettings = CommonDataStatic.Preferences.ReportSettings;

			header.Append("<!DOCTYPE html>");
			header.Append("<html><head><script>");
			header.Append("function subst() {");
			header.Append("  var vars={};");
			header.Append("  var x=window.location.search.substring(1).split('&');");
			header.Append("  for (var i in x) {var z=x[i].split('=',2);vars[z[0]] = unescape(z[1]);}");
			header.Append("  var x=['frompage','topage','page','webpage','section','subsection','subsubsection'];");
			header.Append("  for (var i in x) {");
			header.Append("	 var y = document.getElementsByClassName(x[i]);");
			header.Append("	 for (var j=0; j<y.length; ++j) y[j].textContent = vars[x[i]];");
			header.Append("  }");
			header.Append("}");
			header.Append("</script>");
			header.Append("<STYLE type='text/css'>");
			header.Append("* {font-family: \"" + reportSettings.FontName + "\"; " + "font-size: " + reportSettings.FontSize + ";}");
			header.Append("table {border-collapse: collapse;}\r");
			header.Append("table.header {width: 95%;}\r");
			header.Append("th, td {padding-top: 2px; padding-bottom: 2px; padding-left: 10px; width: 20%;}\r");
			header.Append("th {border-right: 1px solid rgb(225, 225, 225); text-align: left;}\r");
			header.Append("</STYLE>");
			header.Append("</head><body onload=\"subst()\">");
			header.Append(new ReportBuild().BuildHeaderTable(reportSettings.SmartPageNumbering));
			header.Append("</br>");
			header.Append("</body></html>");

			File.WriteAllText(ConstString.FILE_PDFHEADER_HTML, header.ToString());
		}

		private bool IsReportHeaderLineType(EReportLineType lineType)
		{
			if (lineType == EReportLineType.Header || lineType == EReportLineType.MainHeader || lineType == EReportLineType.GoToHeader)
				return true;
			else
				return false;
		}
	}
}