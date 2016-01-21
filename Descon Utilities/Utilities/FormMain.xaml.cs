using System;
using System.Text;
using System.Windows;
using Descon.WebAccess;
using Microsoft.Win32;

namespace Utilities
{
	/// <summary>
	/// Interaction logic for FormMain.xaml
	/// </summary>
	public partial class FormMain
	{
		private readonly SQLServerIntraction _serverInteraction = new SQLServerIntraction();

		public FormMain()
		{
			InitializeComponent();

			DataContext = new CommonData();

			try
			{
				using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Descon Plus", true))
				{
					string keyValue;

					if (key != null)
					{
						keyValue = (string)key.GetValue("WS");
						if (keyValue != Encoding.UTF8.GetString(Convert.FromBase64String("WkVCUU1M")))
						{
							MessageBox.Show("You do not have rights to run this application. Program will now close.");
							Close();
						}
					}
				}
			}
			catch
			{
				MessageBox.Show("Error reading registry. Program will now close.");
				Close();
			}
		}

		private void btnTestDatabaseConnection_Click(object sender, RoutedEventArgs e)
		{
			string error = _serverInteraction.TestDatabaseConnection();
			if (error != string.Empty)
				MessageBox.Show(error);
			else
			{
				gbxCompany.IsEnabled = true;
				gbxUser.IsEnabled = true;
				gbxUpdateUser.IsEnabled = true;
				gbxUpdateCompanyName.IsEnabled = true;
			}
		}

		#region Company Stuff

		private void btnAddCompany_Click(object sender, RoutedEventArgs e)
		{
			if (CheckForValidCompanyData())
			{
				string error = _serverInteraction.AddNewCompany(tbxCompanyName.Text, cbxLicenseType.Text, int.Parse(tbxCompanyNumberOfSeats.Text), dpLicenseEndDate.DisplayDate);
				if (error == "0")
					MessageBox.Show("Duplicate company was not added.");
				else if (error != string.Empty)
					MessageBox.Show(error);
				else
					MessageBox.Show("Company added successfully.");
			}
		}

		private void btnUpdateCompany_Click(object sender, RoutedEventArgs e)
		{
			if (CheckForValidCompanyData())
			{
				string error = _serverInteraction.UpdateCompany(tbxCompanyName.Text, cbxLicenseType.Text, int.Parse(tbxCompanyNumberOfSeats.Text), dpLicenseEndDate.DisplayDate);
				if (error != string.Empty)
					MessageBox.Show(error);
				else
					MessageBox.Show("Company updated successfully.");
			}
		}

		private bool CheckForValidCompanyData()
		{
			int numberOfSeats;

			if (tbxCompanyName.Text == string.Empty)
			{
				MessageBox.Show("Please enter a company name.");
				return false;
			}

			if (!int.TryParse(tbxCompanyNumberOfSeats.Text, out numberOfSeats))
			{
				MessageBox.Show("Please enter a valid number for the number of seats.");
				return false;
			}

			if (dpLicenseEndDate.SelectedDate == null || dpLicenseEndDate.SelectedDate <= DateTime.Today)
			{
				MessageBox.Show("Please enter a License End Date later than today.");
				return false;
			}

			return true;
		}

		private void btnUpdateCompanyName_Click(object sender, RoutedEventArgs e)
		{
			if (CheckForValidCompanyUpdateData())
			{
				string error = _serverInteraction.UpdateCompanyName(tbxCompanyNameOld.Text, tbxCompanyNameNew.Text);
				if (error != string.Empty)
					MessageBox.Show(error);
				else
					MessageBox.Show("Company name updated successfully.");
			}
		}

		#endregion

		#region User Stuff

		private void btnAddUser_Click(object sender, RoutedEventArgs e)
		{
			if (CheckForValidUserData())
			{
				string error = _serverInteraction.AddNewUser(tbxUserCompanyName.Text, tbxUserEmail.Text, "100000");
				if (error == "0")
					MessageBox.Show("Duplicate user was not added.");
				else if (error != string.Empty)
					MessageBox.Show(error);
				else
					MessageBox.Show("User added successfully.");
			}
		}

		private void btnResetUserPassword_Click(object sender, RoutedEventArgs e)
		{
			if (CheckForValidUserData())
			{
				string error = _serverInteraction.ResetUserPassword(tbxUserCompanyName.Text, tbxUserEmail.Text);
				if (error != string.Empty)
					MessageBox.Show(error);
				else
					MessageBox.Show("Password reset successfully.");
			}
		}

		private void btnResetUserComputers_Click(object sender, RoutedEventArgs e)
		{
			if (CheckForValidUserData())
			{
				string error = _serverInteraction.ResetUserComputers(tbxUserCompanyName.Text, tbxUserEmail.Text);
				if (error != string.Empty)
					MessageBox.Show(error);
				else
					MessageBox.Show("User computers reset successfully.");
			}
		}

		private bool CheckForValidUserData()
		{
			if (tbxUserCompanyName.Text == string.Empty)
			{
				MessageBox.Show("Please enter a company name.");
				return false;
			}

			if (tbxUserEmail.Text == string.Empty)
			{
				MessageBox.Show("Please enter a user e-mail.");
				return false;
			}

			return true;
		}

		private void btnUpdateUserEmail_Click(object sender, RoutedEventArgs e)
		{
			if (CheckForValidUserUpdateData())
			{
				string error = _serverInteraction.UpdateUserEmail(tbxEmailOld.Text, tbxEmailNew.Text);
				if (error != string.Empty)
					MessageBox.Show(error);
				else
					MessageBox.Show("User email updated successfully.");
			}
		}

		#endregion

		private bool CheckForValidUserUpdateData()
		{
			if (tbxEmailOld.Text == string.Empty)
			{
				MessageBox.Show("Please enter the original e-mail.");
				return false;
			}

			if (tbxEmailNew.Text == string.Empty)
			{
				MessageBox.Show("Please enter the new e-mail.");
				return false;
			}

			return true;
		}


		private bool CheckForValidCompanyUpdateData()
		{
			if (tbxCompanyNameOld.Text == string.Empty)
			{
				MessageBox.Show("Please enter the original company name.");
				return false;
			}

			if (tbxCompanyNameNew.Text == string.Empty)
			{
				MessageBox.Show("Please enter the new company name.");
				return false;
			}

			return true;
		}

		private void btnAdditionalUtilities_Click(object sender, RoutedEventArgs e)
		{
			new AdditionalUtilities().ShowDialog();
		}

		// The following features may be added later

		//private void btnTestProxy_Click(object sender, RoutedEventArgs e)
		//{
		//	var proxy = new LicenseProxyClient();
		//	string result = proxy.Connect("localHost", 43000, "Hello");
		//	MessageBox.Show(result);
		//}

		//private void btnAddCompaniesFromFile_Click(object sender, RoutedEventArgs e)
		//{
		//	string fileName;

		//	var openDialog = new OpenFileDialog();
		//	if (openDialog.ShowDialog() != false)
		//		fileName = openDialog.FileName;
		//	else
		//		return;

		//	Mouse.OverrideCursor = Cursors.Wait;

		//	var lines = File.ReadAllLines(fileName, Encoding.UTF7);

		//	foreach (var line in lines)
		//	{
		//		var lineArray = line.Split('|');

		//		string error = _serverInteraction.AddNewCompany(
		//			lineArray[0],
		//			lineArray[1],
		//			int.Parse(lineArray[2]),
		//			DateTime.Parse(lineArray[3]));

		//		if (error != string.Empty)
		//			MessageBox.Show(error);
		//	}

		//	Mouse.OverrideCursor = Cursors.Arrow;
		//}

		//private void btnAddUsersFromFile_Click(object sender, RoutedEventArgs e)
		//{
		//	string fileName;

		//	var openDialog = new OpenFileDialog();
		//	if (openDialog.ShowDialog() != false)
		//		fileName = openDialog.FileName;
		//	else
		//		return;

		//	Mouse.OverrideCursor = Cursors.Wait;

		//	var lines = File.ReadAllLines(fileName, Encoding.UTF7);

		//	foreach (var line in lines)
		//	{
		//		var lineArray = line.Split('|');

		//		string error = _serverInteraction.AddNewUser(
		//			lineArray[0],
		//			lineArray[1],
		//			"");

		//		if (error != string.Empty)
		//			MessageBox.Show(error);
		//	}

		//	Mouse.OverrideCursor = Cursors.Arrow;
		//}
	}
}