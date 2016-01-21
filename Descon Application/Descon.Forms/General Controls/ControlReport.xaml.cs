using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Descon.Data;
using Descon.UI.DataAccess;
using Microsoft.Win32;
using mshtml;
using Clipboard = System.Windows.Clipboard;
using ComboBox = System.Windows.Controls.ComboBox;
using Control = System.Windows.Forms.Control;
using Point = System.Drawing.Point;
using TextDataFormat = System.Windows.TextDataFormat;

namespace Descon.Forms
{
	/// <summary>
	/// Generic report form used to display all reports. Uses a WinForms WebBrowser control because the WPF one is missing
	/// a lot of features. 
	/// </summary>
	public partial class ControlReport
	{
		private CommonData _data;
		private int _currentNoGoodBlock;
		private int _scrollBarPosition;
		private int _currentBookmark;
		private Point _cursorPosition;
		private string _currentMarkedLine;
		private bool _projectManagerReport;

		/// <summary>
		/// This event is used to trigger the Apply to Drawing button when a new shape is selected on this child control
		/// </summary>
		public static readonly RoutedEvent SettingsUpdatedEvent = EventManager.RegisterRoutedEvent("SettingsUpdated", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ControlDetailData));

		/// <summary>
		/// This event is used to trigger the Apply to Drawing button when a new shape is selected on this child control
		/// </summary>
		public event RoutedEventHandler SettingsUpdated
		{
			add { AddHandler(SettingsUpdatedEvent, value); }
			remove { RemoveHandler(SettingsUpdatedEvent, value); }
		}

		public ControlReport()
		{
			InitializeComponent();
 
			// This would not work in the xaml, so we're doing it manually
			webBrowser.ContextMenuStrip = new ContextMenuStrip();

			webBrowser.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Copy", null, menuCopy_Click, Keys.C | Keys.Control));
			webBrowser.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			webBrowser.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Add Comment", null, menuAddComment_Click));
			webBrowser.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Remove Comment", null, menuClearComment_Click));
			webBrowser.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			webBrowser.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Toggle Highlight", null, menuToggleHighlight_Click));
			webBrowser.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Clear All Highlighting", null, menuClearHighlights_Click));
			webBrowser.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			webBrowser.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Toggle Bookmark", null, menuToggleBookmark_Click));
			webBrowser.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Clear All Bookmarks", null, menuClearBookmarks_Click));
			// Commented out for public builds since the functionality is not complete
			//webBrowser.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			//webBrowser.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Toggle Highlight of Value", null, menuToggleHighlightOfVariable_Click));

			borderFlowDocReader.Visibility = Visibility.Hidden;
		}

		public void ScrollToTop()
		{
			if (webBrowser != null && webBrowser.Document != null && webBrowser.Document.Window != null)
				webBrowser.Document.Window.ScrollTo(0, 0);
		}

		private void controlReportHTML_Loaded(object sender, RoutedEventArgs e)
		{
			if (DataContext != null)
				_data = (CommonData)DataContext;
			// Add the bindings manually because this is a WinForms control. DocumentText is the WebBrowser property, ReportText
			// is the CommonData property that holds the report
			if (_data != null && webBrowser.DataBindings.Count == 0)
				webBrowser.DataBindings.Add("DocumentText", _data, "ReportText");

			// No binding property on the control so this has to be here
			if (CommonDataStatic.LicenseType == ELicenseType.Demo_1)
				((Control)webBrowser).Enabled = false;

			RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\PageSetup", true);

			if (key != null)
			{
				//key.SetValue("footer", CommonDataStatic.LicenseTypeDisplay + " Licensed to: " + CommonDataStatic.CompanyName);
				key.SetValue("footer", string.Empty);
				//key.SetValue("header", "Page &p of &P");
				//key.SetValue("Print_Background", "yes");
			}
		}

		/// <summary>
		/// This fires when we have a new document and is used to save the scroll bar position to use later in 
		/// webBrowser_DocumentCompleted
		/// </summary>
		private void webBrowser_FileDownload(object sender, EventArgs e)
		{
			if (webBrowser.Document != null && webBrowser.Document.Body != null &&
			    webBrowser.Document.Title != string.Empty && webBrowser.Document.Body.Parent != null)
				_scrollBarPosition = webBrowser.Document.Body.Parent.ScrollRectangle.Y;
		}

		/// <summary>
		/// This fires when the document is loaded and displayed.
		/// </summary>
		private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			_currentNoGoodBlock = 0;
			if (webBrowser.Document != null && webBrowser.Document.Window != null && webBrowser.Document.Body != null)
			{
				webBrowser.Document.Window.ScrollTo(0, _scrollBarPosition);

				webBrowser.Document.MouseDown -= WebBrowserDocument_Click;
				webBrowser.Document.MouseDown += WebBrowserDocument_Click;

				webBrowser.Document.Body.DetachEventHandler("ondblclick", webBrowser_DoubleClick);
				webBrowser.Document.Body.AttachEventHandler("ondblclick", webBrowser_DoubleClick);

				webBrowser.Document.MouseDown -= webBrowser_Click;
				webBrowser.Document.MouseDown += webBrowser_Click;
			}

			ResetBookmarks();
			ResetHighlights();
			ResetComments();

			CommonDataStatic.ReportNoGoodList = CommonDataStatic.ReportNoGoodList.OrderBy(l => l).ToList();
			CommonDataStatic.ReportBookmarkList = CommonDataStatic.ReportBookmarkList.OrderBy(l => l).ToList();

			_data.OnPropertyChanged("ReportGoToList");
		}

		/// <summary>
		/// Finds the element located at the current cursor position
		/// </summary>
		/// <returns></returns>
		private HtmlElement FindValidDocElement()
		{
			if (webBrowser.Document == null)
				return null;

			var docElement = webBrowser.Document.GetElementFromPoint(_cursorPosition);
			if (docElement == null)
				return null;

			if (docElement.TagName.ToLower() == "span" && docElement.GetAttribute("className").Contains("_"))
				return docElement;

			if (docElement.TagName.ToLower() != "div" && docElement.Parent != null)
				docElement = docElement.Parent;

			if (docElement.TagName.ToLower() == "div" || docElement.TagName.ToLower() == "b")
				return docElement;
			else
				return null;
		}

		/// <summary>
		/// This is used by the Project Manager solution when it loads this control. Hides extra controls and marks a bool
		/// that controls some specific functionality.
		/// </summary>
		public void ProjectManagerReportInit()
		{
			cbxGoTo.Visibility = Visibility.Collapsed;

			_projectManagerReport = true;

			new LoadDataFromXML().LoadReportSettings();
		}

		/// <summary>
		/// Finds a specific block in the HTML and scrolls there. Needs to be public for the Project Manager to use it
		/// </summary>
		public void FindBlockInReport(string lineNumber)
		{
			RemoveMarkedLine();

			if (webBrowser.Document != null)
			{
				var originalElement = webBrowser.Document.GetElementById(lineNumber);
				if (originalElement == null)
					return;

				_currentMarkedLine = lineNumber;

				// Finds the line three lines above the one we are looking for
				var digitsOnly = new Regex(@"[^\d]");
				int lineNumberNumericalPortion = int.Parse(digitsOnly.Replace(lineNumber, string.Empty));
				if (lineNumberNumericalPortion > 3)
					lineNumber = lineNumber.Replace(lineNumberNumericalPortion.ToString().PadLeft(5, '0'), (lineNumberNumericalPortion - 3).ToString().PadLeft(5, '0'));

				var newElement = webBrowser.Document.GetElementById(lineNumber);
				if (newElement != null)
					newElement.ScrollIntoView(true);

				// Marks the scrolled to line temporarily
				originalElement.SetAttribute("classname", originalElement.GetAttribute("classname") + " navigation_target");
			}
		}

		// Unmarks the line that was scrolled to
		private void webBrowser_Click(object sender, EventArgs e)
		{
			RemoveMarkedLine();
		}

		private void RemoveMarkedLine()
		{
			if (webBrowser.Document != null && _currentMarkedLine != null)
			{
				var element = webBrowser.Document.GetElementById(_currentMarkedLine);
				if (element != null)
					element.SetAttribute("classname", element.GetAttribute("classname").Replace(" navigation_target", string.Empty));
			}

			_currentMarkedLine = null;
		}

		#region Buttons

		private void btnSaveReport_Click(object sender, RoutedEventArgs e)
		{
			if (webBrowser.Document != null)
				new ReportSave().SaveReport(webBrowser.Document.GetElementsByTagName("HTML")[0].OuterHtml, string.Empty, _data, _projectManagerReport);
		}

		private void btnEditReportSettings_Click(object sender, RoutedEventArgs e)
		{
			if (_projectManagerReport)
				new LoadDataFromXML().LoadReportSettings();

			var form = new FormControlShell(_data, new ControlReportSettings(_projectManagerReport), "Report Settings");
			var result = form.ShowDialog();
			if (result == true && _projectManagerReport)
			{
				new SaveDataToXML().SaveReportSettings();
				new LoadDataFromXML().LoadReportSettings();
			}
			else if (result == true & !_projectManagerReport)
				new SaveDataToXML().SavePreferences();
			else
				_data.Preferences = new LoadDataFromXML().LoadPreferences();

			UnityInteraction.SendDataToUnity(ConstString.UNITY_PREFERENCES_UPDATE);

			if (!_projectManagerReport)
				new MiscFormMethods().ApplyChangesToDrawing(_data);

			RaiseEvent(new RoutedEventArgs(SettingsUpdatedEvent));
		}

		private void btnSearch_Click(object sender, RoutedEventArgs e)
		{
			CallFindDialog();
		}

		private void btnColumns_Click(object sender, RoutedEventArgs e)
		{
			if(borderWebBrowser.Visibility == Visibility.Visible)
				borderWebBrowser.Visibility = Visibility.Hidden;
			else
				borderWebBrowser.Visibility = Visibility.Visible;

			if (borderFlowDocReader.Visibility == Visibility.Visible)
				borderFlowDocReader.Visibility = Visibility.Hidden;
			else
				borderFlowDocReader.Visibility = Visibility.Visible;

			if (stackPanelHTMLControls.Visibility == Visibility.Visible)
				stackPanelHTMLControls.Visibility = Visibility.Hidden;
			else
				stackPanelHTMLControls.Visibility = Visibility.Visible;
		}

		private void btnHome_Click(object sender, RoutedEventArgs e)
		{
			if (webBrowser.Document != null && webBrowser.Document.Window != null)
				webBrowser.Document.Window.ScrollTo(0, 0);
		}

		private void btnPrint_Click(object sender, RoutedEventArgs e)
		{
			webBrowser.ShowPrintDialog();
		}

		private void btnPrintPreview_Click(object sender, RoutedEventArgs e)
		{
			webBrowser.ShowPrintPreviewDialog();
		}

		private void btnPageSetup_Click(object sender, RoutedEventArgs e)
		{
			webBrowser.ShowPageSetupDialog();
		}

		private void btnNextNoGood_Click(object sender, RoutedEventArgs e)
		{
			if (CommonDataStatic.ReportNoGoodList.Count < 1)
				return;

			if (_currentNoGoodBlock == _data.NumberOfNoGoods - 1)
				_currentNoGoodBlock = 0;
			else
				_currentNoGoodBlock++;

			FindBlockInReport(CommonDataStatic.ReportNoGoodList[_currentNoGoodBlock]);
		}

		private void btnPreviousNoGood_Click(object sender, RoutedEventArgs e)
		{
			if (CommonDataStatic.ReportNoGoodList.Count < 1)
				return;

			if (_currentNoGoodBlock == 0)
				_currentNoGoodBlock = _data.NumberOfNoGoods - 1;
			else
				_currentNoGoodBlock--;

			FindBlockInReport(CommonDataStatic.ReportNoGoodList[_currentNoGoodBlock]);
		}

		private void btnBookmarkNext_Click(object sender, RoutedEventArgs e)
		{
			NavigateBookmarks(true);
		}

		private void btnBookmarkPrevious_Click(object sender, RoutedEventArgs e)
		{
			NavigateBookmarks(false);
		}

		private void cbxGoTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (((ComboBox)sender).SelectedValue != null)
				FindBlockInReport(((ComboBox)sender).SelectedValue.ToString());
		}

		#endregion

		#region Right Click Menu

		private void menuToggleBookmark_Click(object sender, EventArgs e)
		{
			ToggleBookMark(FindValidDocElement());
		}

		private void menuClearBookmarks_Click(object sender, EventArgs e)
		{
			CommonDataStatic.ReportBookmarkList.Clear();

			if (webBrowser.Document != null)
			{
				foreach (HtmlElement element in webBrowser.Document.GetElementsByTagName("div"))
					ClearBookmark(element);
			}
		}

		private void menuCopy_Click(object sender, EventArgs e)
		{
			if (webBrowser.Document != null)
			{
				var htmlDocument = webBrowser.Document.DomDocument as IHTMLDocument2;

				if (htmlDocument != null && htmlDocument.selection != null)
				{
					var range = htmlDocument.selection.createRange() as IHTMLTxtRange;
					if (range != null && range.text != null)
						Clipboard.SetText(range.text, TextDataFormat.UnicodeText);
				}
			}
		}

		private void menuAddComment_Click(object sender, EventArgs e)
		{
			AddComment(FindValidDocElement());
		}

		private void menuClearComment_Click(object sender, EventArgs e)
		{
			ClearComment();
		}

		// Adds a span tag surrounding the user highlighted text
		private void menuToggleHighlight_Click(object sender, EventArgs e)
		{
			ToggleHighlight(FindValidDocElement());
		}

		// Removes the span tag completely and replaces it with just the text
		private void menuClearHighlights_Click(object sender, EventArgs e)
		{
			ClearHighlights();
		}

		// Removes the span tag completely and replaces it with just the text
		private void menuToggleHighlightOfVariable_Click(object sender, EventArgs e)
		{
			ToggleHighlightOfVariable(FindValidDocElement());
		}

		#endregion

		#region Bookmarking

		private void NavigateBookmarks(bool forward)
		{
			if (CommonDataStatic.ReportBookmarkList.Count < 1)
				return;

			if (forward)
			{
				_currentBookmark++;
				if (_currentBookmark >= CommonDataStatic.ReportBookmarkList.Count)
					_currentBookmark = 0;
			}
			else
			{
				_currentBookmark--;

				if (_currentBookmark < 0)
					_currentBookmark = CommonDataStatic.ReportBookmarkList.Count - 1;
			}

			FindBlockInReport(CommonDataStatic.ReportBookmarkList[_currentBookmark]);
		}


		private void ToggleBookMark(HtmlElement docElement)
		{
			HtmlElement element;

			if (webBrowser.Document == null)
				return;

			if (docElement == null || docElement.Id == null)
				return;

			if (CommonDataStatic.ReportBookmarkList.Contains(docElement.Id) && docElement.GetElementsByTagName("img").Count > 0)
			{
				CommonDataStatic.ReportBookmarkList.Remove(docElement.Id);
				ClearBookmark(docElement);
			}
			else
			{
				if (!CommonDataStatic.ReportBookmarkList.Contains(docElement.Id))
					CommonDataStatic.ReportBookmarkList.Add(docElement.Id);

				element = webBrowser.Document.CreateElement("img");
				if (element != null) 
				{
					element.SetAttribute("classname", "bookmark");
					element.SetAttribute("src", ConstString.FILE_BOOKMARK_PNG);
					docElement.InsertAdjacentElement(HtmlElementInsertionOrientation.BeforeEnd, element);
				}

				CommonDataStatic.ReportBookmarkList = CommonDataStatic.ReportBookmarkList.OrderBy(l => l).ToList();
			}
		}

		private void ClearBookmark(HtmlElement docElement)
		{
			if (docElement.Id == null)
				return;

			foreach (HtmlElement element in docElement.GetElementsByTagName("img"))
				element.OuterHtml = element.InnerText;
		}

		private void ResetBookmarks()
		{
			if (webBrowser.Document == null)
				return;

			foreach (HtmlElement docElement in webBrowser.Document.GetElementsByTagName("div"))
			{
				if (docElement.Id != null && CommonDataStatic.ReportBookmarkList.Contains(docElement.Id))
					ToggleBookMark(docElement);
			}
		}

		#endregion

		#region Highlighting

		private void ToggleHighlightOfVariable(HtmlElement element)
		{
			if (webBrowser.Document == null || element == null)
				return;

			string classname = element.GetAttribute("classname");

			foreach (HtmlElement docElement in webBrowser.Document.GetElementsByTagName("span"))
			{
				if (docElement.GetAttribute("classname") == classname && string.IsNullOrEmpty(docElement.Style))
					docElement.Style = "background-color: lightblue;";
				else
					docElement.Style = null;
			}
		}

		private void ToggleHighlight(HtmlElement docElement)
		{
			if (docElement == null || docElement.Id == null)
				return;

			string classname = docElement.GetAttribute("classname");

			if (!classname.Contains("highlight"))
			{
				docElement.SetAttribute("classname", classname + " highlight");
				if (docElement.Children.Count > 0)
					docElement.Children[0].Style = "background-color: yellow;";

				if(!CommonDataStatic.ReportHighlightList.Contains(docElement.Id))
					CommonDataStatic.ReportHighlightList.Add(docElement.Id);
			}
			else
			{
				docElement.SetAttribute("classname", classname.Replace(" highlight", string.Empty));

				CommonDataStatic.ReportHighlightList.Remove(docElement.Id);
			}

			// Only toggle the bookmark if there isn't one already
			if (CommonDataStatic.Preferences.ReportSettings.AutoToggleBookmarks && docElement.GetElementsByTagName("img").Count == 0)
				ToggleBookMark(FindValidDocElement());
		}

		private void ClearHighlights()
		{
			if (webBrowser.Document == null)
				return;

			foreach (HtmlElement element in webBrowser.Document.GetElementsByTagName("div"))
			{
				if(element.GetAttribute("classname").Contains("highlight"))
					ToggleHighlight(element);
			}
		}

		private void ResetHighlights()
		{
			if (webBrowser.Document == null)
				return;

			foreach (HtmlElement docElement in webBrowser.Document.GetElementsByTagName("div"))
			{
				if (CommonDataStatic.ReportHighlightList.Contains(docElement.Id))
					ToggleHighlight(docElement);
			}
		}

		#endregion

		#region Commenting

		/// <summary>
		/// Saves the comments to CommonDataStatic so they can be put in the save file
		/// </summary>
		public void SaveComments()
		{

			if (webBrowser.Document == null)
				return;

			foreach (HtmlElement docElement in webBrowser.Document.GetElementsByTagName("span"))
			{
				if (docElement.GetAttribute("classname") == "comment")
					CommonDataStatic.ReportCommentList[docElement.Id] = docElement.InnerText;
			}
		}

		private void AddComment(HtmlElement docElement, string comment = "&nbsp;&nbsp;") // Adds two spaces by default
		{
			if (webBrowser.Document == null)
				return;

			if (docElement == null || docElement.Id == null)
				return;

			if (docElement.GetElementsByTagName("span").Count > 0)
				return;

			docElement.Style = "margin-bottom: 0.25em";

			var element = webBrowser.Document.CreateElement("span");
			if (element != null)
			{
				element.SetAttribute("contenteditable", "true");
				element.SetAttribute("classname", "comment");
				element.SetAttribute("id", docElement.Id);
				element.InnerHtml = comment;

				// Only toggle the bookmark if there is none already
				if (CommonDataStatic.Preferences.ReportSettings.AutoToggleBookmarks && docElement.GetElementsByTagName("img").Count == 0)
					ToggleBookMark(docElement);

				docElement.InsertAdjacentElement(HtmlElementInsertionOrientation.BeforeEnd, element);

				if (!CommonDataStatic.ReportCommentList.ContainsKey(docElement.Id))
					CommonDataStatic.ReportCommentList.Add(docElement.Id, string.Empty);

				element.Focus();
			}
		}

		private void ClearComment()
		{
			var docElement = FindValidDocElement();
			if (docElement == null)
				return;

			docElement.Style = "margin-bottom:initial";

			foreach (HtmlElement element in docElement.GetElementsByTagName("span"))
			{
				if (element.GetAttribute("classname") == "comment")
					element.OuterHtml = string.Empty;
			}

			CommonDataStatic.ReportCommentList.Remove(docElement.Id);
		}

		private void ResetComments()
		{
			if (webBrowser.Document == null)
				return;

			foreach (HtmlElement docElement in webBrowser.Document.GetElementsByTagName("div"))
			{
				if (docElement.Id != null && CommonDataStatic.ReportCommentList.ContainsKey(docElement.Id))
					AddComment(docElement, CommonDataStatic.ReportCommentList[docElement.Id]);
			}
		}

		#endregion

		#region Key and Mouse events

		/// <summary>
		/// Hot keys are disabled in the browser to avoid users executing unwanted commands. This handles specific hot keys that we still need
		/// </summary>
		private void webBrowser_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.C)
				menuCopy_Click(null, new EventArgs());
			if (e.Control && e.KeyCode == Keys.F)
				CallFindDialog();
		}

		private void WebBrowserDocument_Click(object sender, HtmlElementEventArgs e)
		{
			_cursorPosition = e.MousePosition;
		}

		private void webBrowser_DoubleClick(object sender, EventArgs e)
		{
			ToggleBookMark(FindValidDocElement());
		}

		#endregion

		#region Crazy COM stuff to call Find dialog because there is no easy way

		private Guid cmdGuid = new Guid("ED016940-BD5B-11CF-BA4E-00C04FD70816");
		private enum MiscCommandTarget
		{
			Find = 1
		}
		private HTMLDocument GetDocument()
		{
			if (webBrowser.Document != null)
			{
				HTMLDocument htm = (HTMLDocument)webBrowser.Document.DomDocument;
				return htm;
			}
			else
				return null;
		}

		private void CallFindDialog()
		{
			// Used to call the Find dialog control
			IOleCommandTarget cmdt;
			Object o = new object();
			cmdt = (IOleCommandTarget)GetDocument();
			cmdt.Exec(ref cmdGuid, (uint)MiscCommandTarget.Find, (uint)SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, ref o, ref o);
		}

		#endregion
	}
}