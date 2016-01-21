using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using CalcPad.Model;
using CalcPad.Resources;

namespace CalcPad.ViewModel
{
	public class MainViewModel : ObservableObject
	{
		private FlowDocument _equationDocument;
		private readonly EquationVariableConverter _equationVariableConverter = new EquationVariableConverter();
		private ObservableCollection<object> _controlCollectionUnitButtons;
		private ObservableCollection<object> _controlCollectionMain;

		public MainViewModel()
		{
			Settings = new Settings();
			CommonLists = new CommonLists();
			EquationsAndVariables = new EquationsAndVariables();
			_controlCollectionMain = new ObservableCollection<object>();
			_controlCollectionUnitButtons = new ObservableCollection<object>();

			Settings.UnitSystem = EUnitSystem.Imperial;
		}

		public readonly CommonLists CommonLists;
		public Settings Settings = new Settings();

		public EUnitSystem UnitSystem
		{
			get { return Settings.UnitSystem; }
			set
			{
				Settings.UnitSystem = value;
				if (UnitsChanged != null)
					UnitsChanged.Invoke(null, new EventArgs());
				OnPropertyChanged("UnitSystem");
			}
		}

		public EventHandler UnitsChanged { get; set; }

		#region Main Controls

		public ObservableCollection<Object> ControlCollectionMain
		{
			get
			{
				_controlCollectionMain.Clear();

				var comboBox = new ComboBox();
				var textBlock = new TextBlock();

				textBlock.Text = "Unit System:";
				_controlCollectionMain.Add(textBlock);

				comboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding {Source = CommonLists.UnitSystemDict});
				comboBox.DisplayMemberPath = "Value";
				comboBox.SelectedValuePath = "Key";
				comboBox.SetBinding(Selector.SelectedValueProperty, new Binding("SelectedValue") {Source = UnitSystem});
				comboBox.SelectedIndex = 0;
				_controlCollectionMain.Add(comboBox);

				return _controlCollectionMain;
			}
			set
			{
				_controlCollectionMain = value;
				OnPropertyChanged("ControlCollectionUnitButtonsMain");
			}
		}

		#endregion

		#region Unit Button Controls

		/// <summary>
		/// Collection of controls to display at the top of the screen
		/// </summary>
		public ObservableCollection<Object> ControlCollectionUnitButtons
		{
			get
			{
				_controlCollectionUnitButtons.Clear();

				foreach (var control in CommonLists.UnitTypeDict)
				{
					var button = new Button();
					button.Content = control.Value;
					button.Command = OnButtonClickCommand;
					button.CommandParameter = button;
					_controlCollectionUnitButtons.Add(button);
				}
				return _controlCollectionUnitButtons;
			}
			set
			{
				_controlCollectionUnitButtons = value;
				OnPropertyChanged("ControlCollectionUnitButtons");
			}
		}

		ICommand onButtonClickCommand;
		public ICommand OnButtonClickCommand
		{
			get
			{
				return onButtonClickCommand ?? (onButtonClickCommand = new RelayCommand(ButtonClick));
			}
		}

		private void ButtonClick(object button)
		{
			Button clickedbutton = button as Button;
			if (clickedbutton != null)
			{
				string msg = string.Format("You pressed the {0} button", clickedbutton.Content);
				MessageBox.Show(msg);
			}
		}

		#endregion

		#region Equation Report

		/// <summary>
		/// Equation FlowDocument containing all of the entered text
		/// </summary>
		public FlowDocument EquationDocument
		{
			get
			{
				var paragraph = new Paragraph();

				foreach (var equationLine in EquationsAndVariables.EquationLineList)
				{
					paragraph.Inlines.Add(equationLine.Equation);
					paragraph.Inlines.Add(new LineBreak());
				}

				_equationDocument = new FlowDocument(paragraph);

				return _equationDocument;
			}
			set
			{
				_equationDocument = value;
				EquationsAndVariables.EquationLineList.Clear();

				foreach (var block in (((Paragraph)_equationDocument.Blocks.FirstBlock).Inlines))
				{
					if (block is Run)
					{
						var equationLine = new EquationLine {Equation = ((Run)block).Text};
						EquationsAndVariables.EquationLineList.Add(equationLine);
					}
				}

				ConvertText();
			}
		}

		/// <summary>
		/// List of equation lines in the document
		/// </summary>
		public EquationsAndVariables EquationsAndVariables { get; set; }

		#endregion

		#region Equation Variables

		public string EquationVariableText { get; set; }

		public ICommand ConvertEquationVariableText
		{
			get { return new DelegateCommand(ConvertText); }
		}

		private void ConvertText()
		{
			if (!string.IsNullOrEmpty(EquationVariableText))
			{
				EquationsAndVariables.EquationVariableDict.Clear();
				var textLines = EquationVariableText.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
				foreach (var line in textLines)
				{
					var result = _equationVariableConverter.ConvertText(line);
					EquationsAndVariables.EquationVariableDict.Add(result.Key, result.Value);
				}

				EquationsAndVariables.SolveAndFormatEquations();
			}
		}

		#endregion
	}
}