using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	/// <summary>
	/// This control is where all of the data is entered on the main form. It contains multiple canvases that are hidden and shown
	/// depending on the current Joint Config and Component Type
	/// </summary>
	public partial class ControlDetailData
	{
		private CommonData _data;
		private readonly MiscFormMethods miscFormMethods = new MiscFormMethods();

		public ControlDetailData()
		{
			InitializeComponent();

			imageWebSplice.ToolTip = ConstString.HELP_DESIGN_WEB_SPLICE;
		}

		/// <summary>
		/// This event is used to trigger the Apply to Drawing button when a new shape is selected on this child control
		/// </summary>
		public static readonly RoutedEvent ApplyChangesEvent = EventManager.RegisterRoutedEvent("ApplyChanges", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (ControlDetailData));

		/// <summary>
		/// This event is used to trigger the Apply to Drawing button when a new shape is selected on this child control
		/// </summary>
		public event RoutedEventHandler ApplyChanges
		{
			add { AddHandler(ApplyChangesEvent, value); }
			remove { RemoveHandler(ApplyChangesEvent, value); }
		}

		private void btnArrowPrevious_Click(object sender, RoutedEventArgs e)
		{
			ArrowClick(false);
		}

		private void btnArrowNext_Click(object sender, RoutedEventArgs e)
		{
			ArrowClick(true);
		}

		/// <summary>
		/// Moves to next or previous member. Applies changes before switching members.
		/// </summary>
		private void ArrowClick(bool next)
		{
			RaiseEvent(new RoutedEventArgs(ApplyChangesEvent));

			int index = _data.CommonLists.MemberList.IndexOfKey(_data.MemberType);
			if (next)
				index++;
			else
				index--;

			if (next && index >= _data.CommonLists.MemberList.Count)
				_data.MemberType = (EMemberType)_data.CommonLists.MemberList.GetKey(0);
			else if (!next && index < 0)
				_data.MemberType = (EMemberType)_data.CommonLists.MemberList.GetKey(_data.CommonLists.MemberList.Count - 1);
			else
				_data.MemberType = (EMemberType)_data.CommonLists.MemberList.GetKey(index);

			SelectShapeField();
		}

		private void btnShearConnection_Click(object sender, RoutedEventArgs e)
		{
			OpenConnectionForm(EMemberSubType.Shear);
		}

		private void btnMomentConnection_Click(object sender, RoutedEventArgs e)
		{
			OpenConnectionForm(EMemberSubType.Moment);
		}

		/// <summary>
		/// Opens the generic Control Shell form after assigning the tab controls to the appropiate forms.
		/// </summary>
		/// <param name="selectedTab">Which tab to default to. 0 is the first tab. Moment forms on the second tab would be 1.</param>
		public void OpenConnectionForm(EMemberSubType selectedTab)
		{
			miscFormMethods.OpenConnectionForm(selectedTab, _data);

			var newEventArgs = new RoutedEventArgs(ApplyChangesEvent);
			RaiseEvent(newEventArgs);
		}

		public void OpenCurrentBoltForm(EMemberSubType subType)
		{
			new FormControlShell(_data, new ControlBoltSelection(ref _data, BoltMethods.GetCurrentBolt(subType)), "Bolt Selection").ShowDialog();

			RaiseEvent(new RoutedEventArgs(ApplyChangesEvent));
		}

		public void SelectShapeField()
		{
			cbxSection.Focus();
		}

		/// <summary>
		/// In Brace mode this will bring up the appropiate form for the current connection type
		/// </summary>
		private void btnMoreData_Click(object sender, RoutedEventArgs e)
		{
			miscFormMethods.OpenMoreData(_data);
			RaiseEvent(new RoutedEventArgs(ApplyChangesEvent));
		}

		private void btnSpliceDetails_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlWCSplice(ref _data), "Column Splice");
			form.ShowDialog();

			RaiseEvent(new RoutedEventArgs(ApplyChangesEvent));
		}

		private void btnColumnStiffeners_Click(object sender, RoutedEventArgs e)
		{
			OpenConnectionForm(EMemberSubType.Stiffener);

			RaiseEvent(new RoutedEventArgs(ApplyChangesEvent));
		}

		private void btnBeam_Click(object sender, RoutedEventArgs e)
		{
			OpenConnectionForm(EMemberSubType.Beam);

			RaiseEvent(new RoutedEventArgs(ApplyChangesEvent));
		}

		private void desconControl_DataChanged(object sender, EventArgs e)
		{
			if (_data != null)
			{
				RaiseEvent(new RoutedEventArgs(ApplyChangesEvent));
			}
		}

		private void btnShapeSearch_Click(object sender, RoutedEventArgs e)
		{
			var formSelection = new FormSelection(_data, EFormSelectionDataType.Shapes);
			if (formSelection.ShowDialog() == true)
				_data.ShapeName = formSelection.SelectedValue;
		}

		private void btnWinMaterialSearch_Click(object sender, RoutedEventArgs e)
		{
			var formSelection = new FormSelection(_data, EFormSelectionDataType.Materials);
			if (formSelection.ShowDialog() == true)
				_data.SelectedMember.Material.Name = formSelection.SelectedValue;
		}

		private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			_data = (CommonData)DataContext;
		}

		private void comboBox_DropDownClosed(object sender, EventArgs e)
		{
			RaiseEvent(new RoutedEventArgs(ApplyChangesEvent));
		}

		private void cbxSection_LostFocus(object sender, RoutedEventArgs e)
		{
			if (cbxSection.Text != _data.ShapeName)
				cbxSection.Text = _data.ShapeName;
		}

		// Calls up the search form if the user hits the ?/ key on the keyboard
		private void cbxSection_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (e.Text == "?")
			{
				e.Handled = true;
				btnShapeSearch_Click(sender, e);
			}
		}

		// Switches the shape type depending on what the user typed.
		private void cbxSection_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Tab)
			{
				RaiseEvent(new RoutedEventArgs(ApplyChangesEvent));
				return;
			}

			string typedText = ((ComboBox)sender).Text.ToLower();
			var textBox = (TextBox)cbxSection.Template.FindName("PART_EditableTextBox", cbxSection);

			if (_data.ShapeType != EShapeType.HollowSteelSection &&
			    _data.CommonLists.ShapeTypeList.ContainsKey(EShapeType.HollowSteelSection) &&
			    (typedText == "hss" || typedText == "pipe"))
			{
				_data.ShapeType = EShapeType.HollowSteelSection;
				cbxSection.Text = typedText.ToUpper();
				textBox.SelectionStart = cbxSection.Text.Length;
			}
			else if (_data.ShapeType != EShapeType.WideFlange &&
			         _data.CommonLists.ShapeTypeList.ContainsKey(EShapeType.WideFlange) &&
			         (typedText == "w" || typedText == "s" || typedText == "hp"))
			{
				_data.ShapeType = EShapeType.WideFlange;
				cbxSection.Text = typedText.ToUpper();
				textBox.SelectionStart = cbxSection.Text.Length;
			}

			RaiseEvent(new RoutedEventArgs(ApplyChangesEvent));
		}
	}
}