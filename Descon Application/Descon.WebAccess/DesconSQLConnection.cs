using System;
using System.Data.SqlClient;
using System.Text;

namespace Descon.WebAccess
{
	internal class DesconSQLConnection
	{
		internal SqlConnection GetConnection()
		{
			// Applications on IIS that may be useful to upate the account
			//SQLServerManager12.msc
			//CLICONFG.EXE

			//SQL String: "Data Source=sql1.desconplus.com\\REXSQLSVR01,8484;Password=123Descon;User ID=dbasic;Initial Catalog=DesconLicensing;"
			//return new SqlConnection(Encoding.UTF8.GetString(Convert.FromBase64String("RGF0YSBTb3VyY2U9c3FsMS5kZXNjb25wbHVzLmNvbVxcUkVYU1FMU1ZSMDEsODQ4NDtQYXNzd29yZD0xMjNEZXNjb247VXNlciBJRD1kYmFzaWM7SW5pdGlhbCBDYXRhbG9nPURlc2NvbkxpY2Vuc2luZzs=")));
			//SQL String: "Data Source=REXSQLSVR01,8484;Password=123Descon;User ID=dbasic;Initial Catalog=DesconLicensing;"
			return new SqlConnection(Encoding.UTF8.GetString(Convert.FromBase64String("RGF0YSBTb3VyY2U9UkVYU1FMU1ZSMDEsODQ4NDtQYXNzd29yZD0xMjNEZXNjb247VXNlciBJRD1kYmFzaWM7SW5pdGlhbCBDYXRhbG9nPURlc2NvbkxpY2Vuc2luZzs=")));
		}
	}
}