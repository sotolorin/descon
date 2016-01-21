using System;
using System.Data;
using System.Data.SqlClient;

namespace Descon.WebAccess
{
	public class SQLAddUpdate
	{
		internal string AddNewCompany(string CompanyName, string LicenseType, int NumberOfUsers, DateTime LicenseEndDate)
		{
			var myConnection = new DesconSQLConnection().GetConnection();
			SqlCommand myCommand;
			int count;

			try
			{
				myConnection.Open();

				SqlParameter myParam1 = new SqlParameter("@Param1", SqlDbType.NVarChar);
				myParam1.Value = CompanyName;

				SqlParameter myParam2 = new SqlParameter("@Param2", SqlDbType.NVarChar, 10);
				myParam2.Value = LicenseType;

				SqlParameter myParam3 = new SqlParameter("@Param3", SqlDbType.Int);
				myParam3.Value = NumberOfUsers;

				SqlParameter myParam4 = new SqlParameter("@Param4", SqlDbType.Date);
				myParam4.Value = LicenseEndDate;

				myCommand = new SqlCommand(string.Empty, myConnection);

				myCommand.Parameters.Add(myParam1);
				myCommand.Parameters.Add(myParam2);
				myCommand.Parameters.Add(myParam3);
				myCommand.Parameters.Add(myParam4);

				// First check for a duplicate company
				myCommand.CommandText = "SELECT COUNT(*)" +
				                        " FROM CompanyLicense" +
				                        " WHERE CompanyName = @Param1";

				count = (int)myCommand.ExecuteScalar();
				if (count > 0)	// Duplicate company, so don't add
					return "0";

				myCommand.CommandText = "INSERT INTO CompanyLicense " +
				                        "(CompanyName, LicenseType, NumberOfUsers, LicenseEndDate) " +
				                        "VALUES (@Param1, @Param2, @Param3, @Param4)";

				myCommand.ExecuteNonQuery();
			}
			catch (SqlException e)
			{
				return e.ToString();
			}
			catch (Exception e)
			{
				return e.ToString();
			}
			finally
			{
				myConnection.Close();
			}

			return string.Empty;
		}

		internal string UpdateCompany(string CompanyName, string LicenseType, int NumberOfUsers, DateTime LicenseEndDate)
		{
			var myConnection = new DesconSQLConnection().GetConnection();
			SqlCommand myCommand;

			try
			{
				myConnection.Open();

				SqlParameter myParam1 = new SqlParameter("@Param1", SqlDbType.NVarChar);
				myParam1.Value = CompanyName;

				SqlParameter myParam2 = new SqlParameter("@Param2", SqlDbType.NVarChar, 10);
				myParam2.Value = LicenseType;

				SqlParameter myParam3 = new SqlParameter("@Param3", SqlDbType.Int);
				myParam3.Value = NumberOfUsers;

				SqlParameter myParam4 = new SqlParameter("@Param4", SqlDbType.Date);
				myParam4.Value = LicenseEndDate;

				myCommand = new SqlCommand(string.Empty, myConnection);

				myCommand.Parameters.Add(myParam1);
				myCommand.Parameters.Add(myParam2);
				myCommand.Parameters.Add(myParam3);
				myCommand.Parameters.Add(myParam4);

				myCommand.CommandText = "UPDATE CompanyLicense" +
				                        " SET LicenseType = @Param2," +
				                        " NumberOfUsers = @Param3," +
				                        " LicenseEndDate = @Param4" +
				                        " WHERE CompanyName = @Param1";

				myCommand.ExecuteNonQuery();
			}
			catch (SqlException e)
			{
				return e.ToString();
			}
			catch (Exception e)
			{
				return e.ToString();
			}
			finally
			{
				myConnection.Close();
			}

			return string.Empty;
		}

		internal string AddNewUser(string companyName, string userEmail, string userSupportID)
		{
			var myConnection = new DesconSQLConnection().GetConnection();
			int count;

			try
			{
				myConnection.Open();

				// Now we set up the parameters and add the data
				SqlParameter myParam1 = new SqlParameter("@Param1", SqlDbType.NVarChar);
				myParam1.Value = companyName;

				SqlParameter myParam2 = new SqlParameter("@Param2", SqlDbType.NVarChar);
				myParam2.Value = userEmail;

				SqlParameter myParam3 = new SqlParameter("@Param3", SqlDbType.NVarChar);
				myParam3.Value = userSupportID;

				SqlCommand myCommand = new SqlCommand(string.Empty, myConnection);

				myCommand.Parameters.Add(myParam1);
				myCommand.Parameters.Add(myParam2);
				myCommand.Parameters.Add(myParam3);

				// First check for a duplicate user
				myCommand.CommandText = "SELECT COUNT(*)" +
				                        " FROM UserLicense ul, CompanyLicense cl" +
				                        " WHERE ul.FK_UserLicense_CompanyLicense = cl.PK_CompanyLicense" +
				                        "  AND cl.CompanyName = @Param1" +
				                        "  AND UserEmail = @Param2";

				count = (int)myCommand.ExecuteScalar();
				if (count > 0)	// Duplicate user, so don't add
					return "0";

				myCommand.CommandText = "INSERT INTO UserLicense (FK_UserLicense_CompanyLicense, UserEmail, UserSupportID) " +
										"VALUES((SELECT PK_CompanyLicense FROM CompanyLicense WHERE CompanyName = @Param1), @Param2, @Param3)";

				myCommand.ExecuteNonQuery();
			}
			catch (SqlException e)
			{
				return e.ToString();
			}
			catch (Exception e)
			{
				return e.ToString();
			}
			finally
			{
				myConnection.Close();
			}

			return string.Empty;
		}

		internal string UpdateCompanyName(string oldCompanyName, string newCompanyName)
		{
			var myConnection = new DesconSQLConnection().GetConnection();

			try
			{
				myConnection.Open();

				// Now we set up the parameters and add the data
				SqlParameter myParam1 = new SqlParameter("@Param1", SqlDbType.NVarChar);
				myParam1.Value = oldCompanyName;

				SqlParameter myParam2 = new SqlParameter("@Param2", SqlDbType.NVarChar);
				myParam2.Value = newCompanyName;

				SqlCommand myCommand = new SqlCommand(string.Empty, myConnection);

				myCommand.Parameters.Add(myParam1);
				myCommand.Parameters.Add(myParam2);

				myCommand.CommandText = "UPDATE CompanyLicense SET CompanyName = @Param2 WHERE CompanyName = @Param1";

				myCommand.ExecuteNonQuery();
			}
			catch (SqlException e)
			{
				return e.ToString();
			}
			catch (Exception e)
			{
				return e.ToString();
			}
			finally
			{
				myConnection.Close();
			}

			return string.Empty;
		}

		internal string UpdateUserEmail(string oldEmail, string newEmail)
		{
			var myConnection = new DesconSQLConnection().GetConnection();

			try
			{
				myConnection.Open();

				// Now we set up the parameters and add the data
				SqlParameter myParam1 = new SqlParameter("@Param1", SqlDbType.NVarChar);
				myParam1.Value = oldEmail;

				SqlParameter myParam2 = new SqlParameter("@Param2", SqlDbType.NVarChar);
				myParam2.Value = newEmail;

				SqlCommand myCommand = new SqlCommand(string.Empty, myConnection);

				myCommand.Parameters.Add(myParam1);
				myCommand.Parameters.Add(myParam2);

				myCommand.CommandText = "UPDATE UserLicense SET UserEmail = @Param2 WHERE UserEmail = @Param1";

				myCommand.ExecuteNonQuery();
			}
			catch (SqlException e)
			{
				return e.ToString();
			}
			catch (Exception e)
			{
				return e.ToString();
			}
			finally
			{
				myConnection.Close();
			}

			return string.Empty;
		}

		public string ReactivateLicense(string userEmail, string companyName, string userComputer)
		{
			var myConnection = new DesconSQLConnection().GetConnection();
			int count;

			try
			{
				myConnection.Open();

				// Now we set up the parameters and add the data
				SqlParameter myParam1 = new SqlParameter("@Param1", SqlDbType.NVarChar);
				myParam1.Value = userEmail;

				SqlParameter myParam2 = new SqlParameter("@Param2", SqlDbType.NVarChar);
				myParam2.Value = companyName;

				SqlParameter myParam3 = new SqlParameter("@Param3", SqlDbType.NVarChar);
				myParam3.Value = userComputer;

				SqlCommand myCommand = new SqlCommand(string.Empty, myConnection);

				myCommand.Parameters.Add(myParam1);
				myCommand.Parameters.Add(myParam2);
				myCommand.Parameters.Add(myParam3);

				// Check and see if someone else logged in with the same user during the deactivated window
				myCommand.CommandText = "SELECT COUNT(*)" +
										" FROM UserLicense ul, CompanyLicense cl" +
										" WHERE ul.UserEmail = @Param1" +
										"  AND ul.FK_UserLicense_CompanyLicense = cl.PK_CompanyLicense" +
										"  AND cl.CompanyName = @Param2" +
										"  AND ((ul.Activated = 1 AND ul.UserComputer = @Param3)" +
										"   OR (ul.Activated_2 = 1 AND ul.UserComputer_2 = @Param3))";

				count = (int)myCommand.ExecuteScalar();

				if (count > 0)
					return new SQLAddUpdate().ActivateComputerSlot(userEmail, companyName, userComputer);
				else
					return "0";	// Kill the app if someone else logged in
			}
			catch (SqlException e)
			{
				return e.ToString();
			}
			catch (Exception e)
			{
				return e.ToString();
			}
			finally
			{
				myConnection.Close();
			}
		}

		internal string ActivateComputerSlot(string userEmail, string companyName, string userComputer)
		{
			var myConnection = new DesconSQLConnection().GetConnection();

			try
			{
				myConnection.Open();

				SqlParameter myParam1 = new SqlParameter("@Param1", SqlDbType.NVarChar);
				myParam1.Value = userEmail;

				SqlParameter myParam2 = new SqlParameter("@Param2", SqlDbType.NVarChar);
				myParam2.Value = companyName;

				SqlParameter myParam3 = new SqlParameter("@Param3", SqlDbType.NVarChar);
				myParam3.Value = userComputer;

				SqlCommand myCommand = new SqlCommand(string.Empty, myConnection);

				myCommand.Parameters.Add(myParam1);
				myCommand.Parameters.Add(myParam2);
				myCommand.Parameters.Add(myParam3);

				myCommand.CommandText = "SELECT UserComputer" +
				                        " FROM UserLicense ul, CompanyLicense cl" +
				                        " WHERE ul.UserEmail = @Param1" +
				                        "  AND ul.FK_UserLicense_CompanyLicense = cl.PK_CompanyLicense" +
				                        "  AND cl.CompanyName = @Param2";

				string userComputerColumnName;
				string activatedColumnName;
				// Test to see which of the UserComputers is null and fill that one in. 
				var value = myCommand.ExecuteScalar();
				if (value == DBNull.Value || (string)value == userComputer)
				{
					userComputerColumnName = "UserComputer";
					activatedColumnName = "Activated";
				}
				else
				{
					userComputerColumnName = "UserComputer_2";
					activatedColumnName = "Activated_2";
				}

				myCommand.CommandText = "UPDATE UserLicense" +
				                        " SET UserLicense." + userComputerColumnName + " = @Param3, UserLicense." + activatedColumnName + " = 1" +
				                        " FROM UserLicense ul, CompanyLicense cl" +
				                        " WHERE ul.FK_UserLicense_CompanyLicense = cl.PK_CompanyLicense" +
				                        "  AND cl.CompanyName = @Param2" +
				                        "  AND ul.UserEmail = @Param1";
				myCommand.ExecuteNonQuery();
			}
			catch (SqlException e)
			{
				return e.ToString();
			}
			catch (Exception e)
			{
				return e.ToString();
			}
			finally
			{
				myConnection.Close();
			}

			return "0";
		}

		public string DeactivateComputerSlot(string userEmail, string companyName)
		{
			var myConnection = new DesconSQLConnection().GetConnection();

			try
			{
				myConnection.Open();

				SqlParameter myParam1 = new SqlParameter("@Param1", SqlDbType.NVarChar);
				myParam1.Value = userEmail;

				SqlParameter myParam2 = new SqlParameter("@Param2", SqlDbType.NVarChar);
				myParam2.Value = companyName;

				SqlCommand myCommand = new SqlCommand(string.Empty, myConnection);

				myCommand.Parameters.Add(myParam1);
				myCommand.Parameters.Add(myParam2);

				myCommand.CommandText = "UPDATE UserLicense" +
										" SET UserLicense.Activated = 0, UserLicense.Activated_2 = 0" +
										" FROM UserLicense ul, CompanyLicense cl" +
										" WHERE ul.FK_UserLicense_CompanyLicense = cl.PK_CompanyLicense" +
										"  AND cl.CompanyName = @Param2" +
										"  AND ul.UserEmail = @Param1";
				myCommand.ExecuteNonQuery();
			}
			catch (SqlException e)
			{
				return e.ToString();
			}
			catch (Exception e)
			{
				return e.ToString();
			}
			finally
			{
				myConnection.Close();
			}

			return "0";
		}

		internal string ResetUserComputers(string companyName, string userEmail)
		{
			var myConnection = new DesconSQLConnection().GetConnection();

			try
			{
				myConnection.Open();

				// Now we set up the parameters and add the data
				SqlParameter myParam1 = new SqlParameter("@Param1", SqlDbType.NVarChar);
				myParam1.Value = companyName;

				SqlParameter myParam2 = new SqlParameter("@Param2", SqlDbType.NVarChar);
				myParam2.Value = userEmail;

				SqlCommand myCommand = new SqlCommand(string.Empty, myConnection);

				myCommand.Parameters.Add(myParam1);
				myCommand.Parameters.Add(myParam2);

				myCommand.CommandText = "UPDATE UserLicense" +
										" SET UserComputer = NULL, UserComputer_2 = NULL" +
										" FROM CompanyLicense cl, UserLicense ul" +
										" WHERE ul.UserEmail = @Param2" +
										"  AND ul.FK_UserLicense_CompanyLicense = cl.PK_CompanyLicense" +
										"  AND cl.CompanyName = @Param1";

				myCommand.ExecuteNonQuery();
			}
			catch (SqlException e)
			{
				return e.ToString();
			}
			catch (Exception e)
			{
				return e.ToString();
			}
			finally
			{
				myConnection.Close();
			}

			return string.Empty;
		}

		internal string ResetUserPassword(string companyName, string userEmail)
		{
			var myConnection = new DesconSQLConnection().GetConnection();

			try
			{
				myConnection.Open();

				// Now we set up the parameters and add the data
				SqlParameter myParam1 = new SqlParameter("@Param1", SqlDbType.NVarChar);
				myParam1.Value = companyName;

				SqlParameter myParam2 = new SqlParameter("@Param2", SqlDbType.NVarChar);
				myParam2.Value = userEmail;

				SqlCommand myCommand = new SqlCommand(string.Empty, myConnection);

				myCommand.Parameters.Add(myParam1);
				myCommand.Parameters.Add(myParam2);

				myCommand.CommandText = "UPDATE UserLicense" +
										" SET UserPassword = null" +
										" FROM CompanyLicense cl, UserLicense ul" +
										" WHERE ul.UserEmail = @Param2" +
										"  AND ul.FK_UserLicense_CompanyLicense = cl.PK_CompanyLicense" +
										"  AND cl.CompanyName = @Param1";

				myCommand.ExecuteNonQuery();
			}
			catch (SqlException e)
			{
				return e.ToString();
			}
			catch (Exception e)
			{
				return e.ToString();
			}
			finally
			{
				myConnection.Close();
			}

			return string.Empty;
		}
	}
}