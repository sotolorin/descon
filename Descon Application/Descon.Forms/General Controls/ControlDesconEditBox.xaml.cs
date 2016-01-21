using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Descon.Data;

namespace Descon.Forms
{
	/// <summary>
	/// Custom control that contains a textbox, unit label, and checkbox. The checkbox will automatically become checked
	/// when the user types something. This will be used to determine whether or not a certain value should be calculated
	/// in code. If checked, any calculations SHOULD NOT SET the value.
	/// 
	/// Please make only one of the controls visible depending on the data type. All controls on the form must be bound to
	/// the CommonData object to work correctly.
	/// </summary>
	[Description("A custom control containing a textbox and checkbox. The checkbox automatically checks after typing in the textbox.")]
	public partial class ControlDesconEditBox
	{
		/// <summary>
		/// Can be used to notify other parent controls that the text or combobox selection has been changed
		/// </summary>
		public EventHandler DataChanged { get; set; }

		public ControlDesconEditBox()
		{
			InitializeComponent();
		}

		private void parent_Loaded(object sender, RoutedEventArgs e)
		{
			// Sets the width to Auto if 0
			lblUnit.Width = DSCUnitWidth == 0 ? double.NaN : DSCUnitWidth;

			if (DSCShowTextBox == Visibility.Visible)
			{
				var myBinding = new Binding
				{
					Path = new PropertyPath("DSCTextBoxValue"),
					Source = DependencyProperty.UnsetValue
				};

				// String format is set by the control properties so this binding must be done when the control is loaded
				switch (DSCTextBoxType)
				{
					case EDesconEditBoxTypes.Decimal_Two_Digits:
						myBinding.StringFormat = "N2";
						break;
					case EDesconEditBoxTypes.Decimal_Three_Digits:
						myBinding.StringFormat = "N3";
						break;
					case EDesconEditBoxTypes.Decimal_Four_Digits:
						myBinding.StringFormat = "N4";
						break;
					case EDesconEditBoxTypes.Fraction_Over_16:
						myBinding.StringFormat = "N0";
						myBinding.Converter = new ConvertFraction();
						break;
					case EDesconEditBoxTypes.Feet_To_Inches:
						myBinding.StringFormat = "N2";
						myBinding.Converter = new ConvertInchesToFeet();
						break;
					case EDesconEditBoxTypes.Integer:
						myBinding.StringFormat = "N0";
						break;
				}

				tbxTextBox.TabIndex = DSCTabIndex;

				BindingOperations.SetBinding(tbxTextBox, TextBox.TextProperty, myBinding);
			}
		}

		#region Various control properties

		[Description("Sets the SelectedValuePath for the ComboBox. Defaults to Key for dictionaries.")]
		public object DSCSelectedValuePath
		{
			get { return GetValue(DSCSelectedValuePathProperty); }
			set { SetValue(DSCSelectedValuePathProperty, value); }
		}

		public static readonly DependencyProperty DSCSelectedValuePathProperty =
			DependencyProperty.Register("DSCSelectedValuePath", typeof(object), typeof(ControlDesconEditBox), new PropertyMetadata("Key"));

		[Description("Sets the DisplayValuePath for the ComboBox. Defaults to Key for dictionaries. Change to Value for Enum based dictionaries.")]
		public object DSCDisplayValuePath
		{
			get { return GetValue(DSCDisplayValuePathProperty); }
			set { SetValue(DSCDisplayValuePathProperty, value); }
		}

		public static readonly DependencyProperty DSCDisplayValuePathProperty =
			DependencyProperty.Register("DSCDisplayValuePath", typeof(object), typeof(ControlDesconEditBox), new PropertyMetadata("Key"));

		[Description("Used to set the number format of the textbox.")]
		public EDesconEditBoxTypes DSCTextBoxType
		{
			get { return (EDesconEditBoxTypes)GetValue(DSCTextBoxTypeProperty); }
			set { SetValue(DSCTextBoxTypeProperty, value); }
		}

		public static readonly DependencyProperty DSCTextBoxTypeProperty =
			DependencyProperty.Register("DSCTextBoxType", typeof(EDesconEditBoxTypes), typeof(ControlDesconEditBox), new PropertyMetadata());

		[Description("TextBox value binding with special handling for 1/16 style TextBoxes.")]
		public object DSCTextBoxValue
		{
			get { return GetValue(DSCTextBoxValueProperty); }
			set { SetValue(DSCTextBoxValueProperty, value); }
		}

		public static readonly DependencyProperty DSCTextBoxValueProperty =
			DependencyProperty.Register("DSCTextBoxValue", typeof(object), typeof(ControlDesconEditBox),
				new FrameworkPropertyMetadata(999, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		[Description("Text Label that is displayed at the front of the control")]
		public object DSCLabel
		{
			get { return GetValue(DSCLabelProperty); }
			set { SetValue(DSCLabelProperty, value); }
		}

		public static readonly DependencyProperty DSCLabelProperty =
			DependencyProperty.Register("DSCLabel", typeof(object), typeof(ControlDesconEditBox), new PropertyMetadata());

		[Description("Unit type binding for Unit label")]
		public object DSCUnit
		{
			get { return GetValue(DSCUnitProperty); }
			set { SetValue(DSCUnitProperty, value); }
		}

		public static readonly DependencyProperty DSCUnitProperty =
			DependencyProperty.Register("DSCUnit", typeof(object), typeof(ControlDesconEditBox), new PropertyMetadata());

		[Description("Sets the width of the Unit label on the control. Used for lining up control vertically. Set " +
		             "the Horizontal Alignment to Right. 0 = Auto")]
		
		public int DSCUnitWidth
		{
			get { return (int)GetValue(DSCUnitWidthProperty); }
			set { SetValue(DSCUnitWidthProperty, value); }
		}

		public static readonly DependencyProperty DSCUnitWidthProperty =
			DependencyProperty.Register("DSCUnitWidth", typeof(int), typeof(ControlDesconEditBox), new PropertyMetadata());

		[Description("CheckBox value binding")]
		public bool DSCCheckBoxValue
		{
			get { return (bool)GetValue(DSCCheckBoxValueProperty); }
			set { SetValue(DSCCheckBoxValueProperty, value); }
		}

		public static readonly DependencyProperty DSCCheckBoxValueProperty =
			DependencyProperty.Register("DSCCheckBoxValue", typeof(bool), typeof(ControlDesconEditBox),
				new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		[Description("ComboBox list items value binding")]
		public object DSCComboBoxSource
		{
			get { return GetValue(DSCComboBoxSourceProperty); }
			set { SetValue(DSCComboBoxSourceProperty, value); }
		}

		public static readonly DependencyProperty DSCComboBoxSourceProperty =
			DependencyProperty.Register("DSCComboBoxSource", typeof(object), typeof(ControlDesconEditBox), new PropertyMetadata());

		[Description("ComboBox selected value binding")]
		public object DSCComboBoxValue
		{
			get { return GetValue(DSCComboBoxValueProperty); }
			set { SetValue(DSCComboBoxValueProperty, value); }
		}

		public static readonly DependencyProperty DSCComboBoxValueProperty =
			DependencyProperty.Register("DSCComboBoxValue", typeof(object), typeof(ControlDesconEditBox), 
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		[Description("Allows the user to type a negative sign for negative values.")]
		public bool DSCAllowNegatives
		{
			get { return (bool)GetValue(DSCAllowNegativesProperty); }
			set { SetValue(DSCAllowNegativesProperty, value); }
		}

		public static readonly DependencyProperty DSCAllowNegativesProperty =
			DependencyProperty.Register("DSCAllowNegatives", typeof(bool), typeof(ControlDesconEditBox),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		[Description("Tab Index of Text Box portion must be set manually.")]
		public int DSCTabIndex
		{
			get { return (int)GetValue(DSCTabIndexProperty); }
			set { SetValue(DSCTabIndexProperty, value); }
		}

		public static readonly DependencyProperty DSCTabIndexProperty =
			DependencyProperty.Register("DSCTabIndex", typeof(int), typeof(ControlDesconEditBox),
			new FrameworkPropertyMetadata(2147483647, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		#endregion

		#region Component Visibility

		[Description("Can toggle the label in case you want it hidden in certain cases.")]
		public Visibility DSCShowLabel
		{
			get { return (Visibility)GetValue(DSCShowLabelProperty); }
			set { SetValue(DSCShowLabelProperty, value); }
		}

		public static readonly DependencyProperty DSCShowLabelProperty =
			DependencyProperty.Register("DSCShowLabel", typeof(Visibility), typeof(ControlDesconEditBox), new PropertyMetadata());

		[Description("Determines whether or not the TextBox is shown. This should be exclusive.")]
		public Visibility DSCShowTextBox
		{
			get { return (Visibility)GetValue(DSCShowTextBoxProperty); }
			set { SetValue(DSCShowTextBoxProperty, value); }
		}

		public static readonly DependencyProperty DSCShowTextBoxProperty =
			DependencyProperty.Register("DSCShowTextBox", typeof(Visibility), typeof(ControlDesconEditBox), new PropertyMetadata());

		[Description("Determines whether or not the ComboBox is shown. This should be exclusive.")]
		public Visibility DSCShowComboBox
		{
			get { return (Visibility)GetValue(DSCShowComboBoxProperty); }
			set { SetValue(DSCShowComboBoxProperty, value);}
		}

		public static readonly DependencyProperty DSCShowComboBoxProperty =
			DependencyProperty.Register("DSCShowComboBox", typeof(Visibility), typeof(ControlDesconEditBox), new PropertyMetadata());

		[Description("Determines whether or not the CheckBox is shown.")]
		public Visibility DSCShowCheckBox
		{
			get { return (Visibility)GetValue(DSCShowCheckBoxProperty); }
			set { SetValue(DSCShowCheckBoxProperty, value); }
		}

		public static readonly DependencyProperty DSCShowCheckBoxProperty =
			DependencyProperty.Register("DSCShowCheckBox", typeof(Visibility), typeof(ControlDesconEditBox), new PropertyMetadata());

		[Description("Determines whether the Unit label is shown in the control.")]
		public Visibility DSCShowUnit
		{
			get { return (Visibility)GetValue(DSCShowUnitProperty); }
			set { SetValue(DSCShowUnitProperty, value); }
		}

		public static readonly DependencyProperty DSCShowUnitProperty =
			DependencyProperty.Register("DSCShowUnit", typeof(Visibility), typeof(ControlDesconEditBox), new PropertyMetadata());

		#endregion

		#region TextBox keypress and ComboBox events

		private void tbxValue_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Tab)
				CommitChanges(sender);

			e.Handled = e.Key == Key.Space;
		}

		private void tbxValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (chxCheckBox.IsChecked != true) // This seems odd, but we only want the checkbox checked one time to avoid triggering updates
				chxCheckBox.IsChecked = true;

			switch (DSCTextBoxType)
			{
				case EDesconEditBoxTypes.Integer:
				case EDesconEditBoxTypes.Fraction_Over_16:
					if (DSCAllowNegatives && (e.Text == "-"))
						e.Handled = false;
					else if (new Regex(@"\d").IsMatch(e.Text))
						e.Handled = false;
					else
						e.Handled = true;
					break;
				case EDesconEditBoxTypes.Decimal_Two_Digits:
				case EDesconEditBoxTypes.Decimal_Three_Digits:
				case EDesconEditBoxTypes.Decimal_Four_Digits:
				case EDesconEditBoxTypes.Feet_To_Inches:
					if (DSCAllowNegatives && (e.Text == "-"))
						e.Handled = false;
					else if (e.Text == "." || new Regex(@"\d").IsMatch(e.Text))
						e.Handled = false;
					else
						e.Handled = true;
					break;
			}

			// If the user presses ENTER we want to commit immediately
			if (e.Text == "\r")
				CommitChanges(sender);
		}

		private void CommitChanges(object sender)
		{
			var tBox = (TextBox)sender;
			var prop = TextBox.TextProperty;
			var binding = BindingOperations.GetBindingExpression(tBox, prop);
			if (binding != null)
				binding.UpdateSource();

			if (DataChanged != null)
				DataChanged.Invoke(null, new EventArgs());

			tbxTextBox.SelectAll();
		}

		private void ComboBox_DropDownClosed(object sender, EventArgs e)
		{
			chxCheckBox.IsChecked = true;
			if (DataChanged != null)
				DataChanged.Invoke(null, new EventArgs());
		}

		private void tbxTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (DataChanged != null)
				DataChanged.Invoke(null, new EventArgs());
		}

		#endregion

		private void tbxTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			tbxTextBox.SelectAll();
		}
	}
}