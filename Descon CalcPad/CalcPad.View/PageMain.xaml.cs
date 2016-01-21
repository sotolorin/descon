using System;
using System.Windows.Documents;
using System.Windows.Input;
using CalcPad.Resources;
using CalcPad.ViewModel;

namespace CalcPad.View
{
	/// <summary>
	/// Interaction logic for PageMain.xaml
	/// </summary>
	public partial class PageMain
	{
		private readonly GetResources _getResources = new GetResources();

		public PageMain()
		{
			InitializeComponent();

			rtbEditor.Document = ((MainViewModel)DataContext).EquationDocument;

			Resources.MergedDictionaries.Add(_getResources.GetLanguageDictionary());
			Resources.MergedDictionaries.Add(_getResources.GetUnitDictionary());

			((MainViewModel)DataContext).UnitsChanged += units_Changed;
		}

		private void units_Changed(object sender, EventArgs e)
		{
			Resources.MergedDictionaries.Add(_getResources.GetUnitDictionary());
		}

		private void rtbEditor_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.OemPlus && Keyboard.Modifiers == ModifierKeys.None)
			{
				((MainViewModel)DataContext).EquationDocument = rtbEditor.Document;
				rtbEditor.Document = ((MainViewModel)DataContext).EquationDocument;
			}
			else if (e.Key == Key.Enter || e.Key == Key.Return)
				((Paragraph)rtbEditor.Document.Blocks.LastBlock).Inlines.Add(new LineBreak());

			rtbEditor.CaretPosition = rtbEditor.CaretPosition.DocumentEnd;
		}
	}
}