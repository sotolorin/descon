using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class FormControlShell
	{
		private readonly CommonData _data;
		// The following two properties are used to serialize and backup the data in case the user cancels.
		private readonly XmlSerializer _serializer = new XmlSerializer(typeof(DetailData));
		private readonly MemoryStream _memoryStream = new MemoryStream();

		/// <summary>
		/// Sets the window position after everything else is initialized.
		/// </summary>
		private void FormControlShell_Loaded(object sender, RoutedEventArgs e)
		{
			if (Left == 0 && Top == 0)
				WindowStartupLocation = WindowStartupLocation.CenterOwner;

			BoltMethods.SetBoltSlotNames();
		}

		/// <summary>
		/// Opens the standard shell form with one control on it.
		/// </summary>
		/// <param name="data">CommonData</param>
		/// <param name="controlTab0">Whichever Control you want on the form</param>
		/// <param name="titleTab0">Title of the control</param>
		/// <param name="hideButtons">Hides all of the things!</param>
		/// /// <param name="alwaysOnTop">Keeps the window at the front</param>
		public FormControlShell(CommonData data, UIElement controlTab0, string titleTab0, bool hideButtons = false, bool alwaysOnTop = false)
		{
			InitializeComponent();

			Owner = Application.Current.MainWindow;

			_data = data;
			Title = titleTab0;

			_serializer.Serialize(_memoryStream, _data.DetailDataDict[_data.SelectedMember.MemberType]);

			// Remove uncessary tabs if we don't display Beam and Column Stiffener. Removing changes the index, so we want to remove the
			// second tab 3 times.
			tabControl.Items.RemoveAt(1);
			tabControl.Items.RemoveAt(1);
			tabControl.Items.RemoveAt(1);

			tabItem0.Header = titleTab0;

			tabGrid0.Children.Add(controlTab0);

			if (hideButtons)
				stackpanelButtons.Visibility = Visibility.Collapsed;

			Topmost = alwaysOnTop;

			DataContext = _data;
		}
		
		/// <summary>
		///  Opens the standard shell form with up to 4 controls. After the first two controls, the Beam and Column Stiffener controls
		/// are automatically added to the form.
		/// </summary>
		/// <param name="data">CommonData</param>
		/// <param name="controlTab0">First Control to be displayed</param>
		/// <param name="controlTab1">Second Control to be displayed. Set to NULL if not needed.</param>
		/// <param name="titleTab0">Title of Control 1</param>
		/// <param name="titleTab1">Title of Control 2. Set to string.empty if not needed.</param>
		/// <param name="selectedTab">Which tab is selected at form startup.</param>
		public FormControlShell(CommonData data, UIElement controlTab0, UIElement controlTab1, string titleTab0, string titleTab1, EMemberSubType selectedTab)
		{
			InitializeComponent();

			Owner = Application.Current.MainWindow;

			_data = data;
			Title = titleTab0;

			// Saves the data so if the user cancels we can restore the information
			_serializer.Serialize(_memoryStream, _data.SelectedMember);

			if (controlTab0 == null && controlTab1 == null)
			{
				tabControl.Items.RemoveAt(0);
				tabControl.Items.RemoveAt(0);
			}
			else if (controlTab1 == null)
				tabControl.Items.RemoveAt(1);

			if (controlTab0 != null)
			{
				tabItem0.Header = titleTab0;
				tabGrid0.Children.Add(controlTab0);
			}
			if (controlTab1 != null)
			{
				tabGrid1.Children.Add(controlTab1);
				tabItem1.Header = titleTab1;
			}

			tabItem2.Header = "Beam";
			tabGrid2.Children.Add(new ControlWCBeam());

			tabItem3.Header = "Stiffener";
			tabGrid3.Children.Add(new ControlStiffeners());

			DataContext = _data;

			switch (selectedTab)
			{
				case EMemberSubType.Shear:
					((TabItem)tabControl.Items[0]).IsSelected = true;
					break;
				case EMemberSubType.Moment:
					((TabItem)tabControl.Items[1]).IsSelected = true;
					break;
				case EMemberSubType.Beam:
					if(controlTab0 == null && controlTab1 == null)
						((TabItem)tabControl.Items[0]).IsSelected = true;
					else if (controlTab1 == null || controlTab0 == null)
						((TabItem)tabControl.Items[1]).IsSelected = true;
					else
						((TabItem)tabControl.Items[2]).IsSelected = true;
					break;
				case EMemberSubType.Stiffener:
					if (controlTab0 == null && controlTab1 == null)
						((TabItem)tabControl.Items[1]).IsSelected = true;
					else if (controlTab1 == null || controlTab0 == null)
						((TabItem)tabControl.Items[2]).IsSelected = true;
					else
						((TabItem)tabControl.Items[3]).IsSelected = true;
					break;
			}
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			if (CheckBoltData())
			{
				DialogResult = true;
				Close();
			}
		}

		private void btnApply_Click(object sender, RoutedEventArgs e)
		{
			if (CheckBoltData())
			{
				new SaveDataToXML().SavePreferences();
				UnityInteraction.SendDataToUnity(ConstString.UNITY_PREFERENCES_UPDATE);

				new MiscFormMethods().ApplyChangesToDrawing(_data);
				((CommonData)DataContext).OnPropertyChanged(null);
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			CleanUpAndClose();
			Close();
		}

		private bool CheckBoltData()
		{
			if (Title.Contains("Bolt"))
			{
				var bolt = _data.CurrentBolt;
				// Check to see if none of the check boxes are selected and at least one is visible.
				if (!bolt.Slot0 && !bolt.Slot1 && !bolt.Slot2 && !bolt.Slot3 &&
				    (bolt.Slot0Visibility || bolt.Slot1Visibility || bolt.Slot2Visibility || bolt.Slot3Visibility))
				{
					MessageBox.Show("Specify Element(s) with Slotted or Oversize Holes to Proceed.");
					return false;
				}
				else
					return true;
			}
			else
				return true;
		}

		private void MetroWindow_Closing(object sender, CancelEventArgs e)
		{
			if (DialogResult != true)
				CleanUpAndClose();
		}

		private void CleanUpAndClose()
		{
			CommonDataStatic.LoadingFileInProgress = true;

			_memoryStream.Position = 0;
			var data = (DetailData)_serializer.Deserialize(_memoryStream);
			_data.DetailDataDict[_data.SelectedMember.MemberType] = data;
			_data.MemberType = _data.SelectedMember.MemberType;

			CommonDataStatic.LoadingFileInProgress = false;
		}
	}
}