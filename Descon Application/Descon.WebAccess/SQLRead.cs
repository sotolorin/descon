using System;
using System.Data;
using System.Data.SqlClient;

namespace Descon.WebAccess
{
	public class SQLRead
	{
		internal string TestDatabaseConnection()
		{
			var myConnection = new DesconSQLConnection().GetConnection();

			try
			{
				myConnection.Open();
			}
			catch (SqlException e)
			{
				if (e.InnerException != null)
					return e.InnerException.Message;
				else
					return e.Message;
			}
			catch (Exception e)
			{
				if (e.InnerException != null)
					return e.InnerException.Message;
				else
					return e.Message;
			}

			myConnection.Close();

			return string.Empty;
		}

		/// <summary>
		/// Returns the license type as a string
		/// </summary>
		public string GetLicenseType(string UserEmail, string CompanyName)
		{
			var myConnection = new DesconSQLConnection().GetConnection();
			string result;

			try
			{
				myConnection.Open();

				SqlCommand myCommand = new SqlCommand(string.Empty, myConnection);

				SqlParameter myParam1 = new SqlParameter("@Param1", SqlDbType.NVarChar);
				myParam1.Value = UserEmail;

				SqlParameter myParam2 = new SqlParameter("@Param2", SqlDbType.NVarChar);
				myParam2.Value = CompanyName;

				myCommand.Parameters.Add(myParam1);
				myCommand.Parameters.Add(myParam2);

				myCommand.CommandText = "SELECT LicenseType" +
				                        " FROM CompanyLicense cl, UserLicense ul" +
				                        " WHERE ul.UserEmail = @Param1" +
				                        "  AND ul.FK_UserLicense_CompanyLicense = cl.PK_CompanyLicense" +
										"  AND cl.CompanyName = @Param2";

				result = (string)myCommand.ExecuteScalar();
			}
			catch (Exception e)
			{
				return e.ToString();
			}
			finally
			{
				myConnection.Close();
			}

			return result;
		}

		/// <summary>
		/// Checks the ComputerID against the current record and activates or adds if necessary. Returns 0, 1, 2, 3, or error
		/// </summary>
		/// <returns>
		/// 0: Success
		/// 1: User Logged into to another computer
		/// 2: No seats available
		/// 3: Bad UserEmail/Password
		/// 4: No matching company
		/// 5: License Expired
		/// 6: All computer slots in use
		/// Other Error</returns>
		public string CheckAndUpdateComputerID(string companyName, string userEmail, string userPassword, string computerID)
		{
			var myConnection = new DesconSQLConnection().GetConnection();
			int count;

			try
			{
				myConnection.Open();

				SqlCommand myCommand = new SqlCommand(string.Empty, myConnection);

				SqlParameter myParam1 = new SqlParameter("@Param1", SqlDbType.NVarChar);
				myParam1.Value = userEmail;

				SqlParameter myParam2 = new SqlParameter("@Param2", SqlDbType.NVarChar);
				myParam2.Value = new PassEncryption().EncryptPass(userPassword);

				SqlParameter myParam3 = new SqlParameter("@Param3", SqlDbType.NVarChar);
				myParam3.Value = computerID;

				SqlParameter myParam4 = new SqlParameter("@Param4", SqlDbType.NVarChar);
				myParam4.Value = companyName;

				myCommand.Parameters.Add(myParam1);
				myCommand.Parameters.Add(myParam2);
				myCommand.Parameters.Add(myParam3);
				myCommand.Parameters.Add(myParam4);

				// ****STEP 1**** Check for matching company name
				myCommand.CommandText = "SELECT COUNT(*)" +
				                        " FROM CompanyLicense" +
				                        " WHERE CompanyName = @Param4";

				count = (int)myCommand.ExecuteScalar();
				if (count < 1)
					return "4";
				// ****END OF STEP 1

				// ****STEP 2**** Check for non-expired license
				myCommand.CommandText = "SELECT COUNT(*)" +
				                        " FROM CompanyLicense" +
				                        " WHERE CompanyName = @Param4" +
				                        "  AND GETDATE() <= LicenseEndDate";

				count = (int)myCommand.ExecuteScalar();
				if (count < 1)
					return "5";
				// ****END OF STEP 2****

				// ****STEP 3**** Check for blank password and add it to the database if blank
				myCommand.CommandText = "SELECT COUNT(*)" +
				                        " FROM UserLicense ul, CompanyLicense cl" +
				                        " WHERE ul.FK_UserLicense_CompanyLicense = cl.PK_CompanyLicense" +
				                        "  AND cl.CompanyName = @Param4" +
				                        "  AND UserEmail = @Param1" +
				                        "  AND UserPassword IS NULL";

				count = (int)myCommand.ExecuteScalar();
				if (count > 0) // We need to write the new pass to the database
				{
					//Check just check e-mail and pass
					myCommand.CommandText = "UPDATE UserLicense" +
					                        " SET UserPassword = @Param2" +
					                        " FROM UserLicense ul, CompanyLicense cl" +
					                        " WHERE ul.FK_UserLicense_CompanyLicense = cl.PK_CompanyLicense" +
					                        "  AND cl.CompanyName = @Param4" +
					                        "  AND UserEmail = @Param1" +
					                        "  AND UserPassword IS NULL";

					myCommand.ExecuteNonQuery();
				}
				// ****END OF STEP 3****

				// ****STEP 4**** Check e-mail and password combination. Drop out with error if doesn't match
				myCommand.CommandText = "SELECT COUNT(*)" +
				                        " FROM UserLicense" +
				                        " WHERE UserEmail = @Param1" +
				                        "  AND UserPassword = @Param2";

				count = (int)myCommand.ExecuteScalar();
				if (count < 1) // Bad pass or e-mail
					return "3";
				// ****END OF STEP 4****

				// ****STEP 5**** Checked if user is logged in and verify we have open computer slots
				myCommand.CommandText = "SELECT ul.Activated, ul.UserComputer, ul.Activated_2, ul.UserComputer_2" +
				                        " FROM UserLicense ul, CompanyLicense cl" +
				                        " WHERE ul.UserEmail = @Param1" +
				                        "  AND ul.FK_UserLicense_CompanyLicense = cl.PK_CompanyLicense" +
				                        "  AND cl.CompanyName = @Param4";

				var reader = myCommand.ExecuteReader();
				reader.Read();

				int activated = reader.GetInt16(0);
				var computer = reader.GetSqlString(1);
				int activated_2 = reader.GetInt16(2);
				var computer_2 = reader.GetSqlString(3);

				reader.Close();
				
				if ((activated == 1 && computer == computerID) ||
					(activated_2 == 1 && computer_2 == computerID)) // Logged in already, good to go
					return "0";
				else if ((activated == 1 && computer != computerID) ||
					(activated_2 == 1 && computer_2 != computerID)) // Logged in at a third computer
					return "1";
				else if (computer != computerID && computer_2 != computerID) // Both computers in use already
					return "6";
				// ****END OF STEP 5****

				// ****STEP 6**** Check if we have seats available
				myCommand.CommandText = "SELECT cl.NumberOfUsers - ISNULL(SUM(ul.Activated + ul.Activated_2), 0)" +
				                        " FROM CompanyLicense cl, UserLicense ul" +
				                        " WHERE ul.FK_UserLicense_CompanyLicense = cl.PK_CompanyLicense" +
				                        "  AND cl.CompanyName = @Param4" +
				                        " GROUP BY cl.NumberOfUsers";

				int numberOfFreeSeats = (int)myCommand.ExecuteScalar();
				if (numberOfFreeSeats < 1)
					return "2";
				// ****END OF STEP 6****

				// **** STEP 7**** Activate the empty computer slot
				return new SQLAddUpdate().ActivateComputerSlot(userEmail, companyName, computerID);
				// ****END OF STEP 7****
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
	}
}