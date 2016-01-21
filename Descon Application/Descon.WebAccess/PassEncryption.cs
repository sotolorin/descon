using System;
using System.Security.Cryptography;
using System.Text;

namespace Descon.WebAccess
{
	internal class PassEncryption
	{
		internal string EncryptPass(string pass)
		{
			// Random byte array to use as salt
			byte[] shearConnection = {0x20, 0x80, 0x80, 0x90, 0x30, 0x50, 0x60, 0x30, 0x10, 0x90};
			string momentConnection = BitConverter.ToString(shearConnection);

			string salt = pass + momentConnection;

			using (SHA512Managed HashTool = new SHA512Managed())
			{
				var bytes = Encoding.UTF8.GetBytes(pass + salt);

				salt = string.Empty;
				pass = string.Empty;

				var bytes2 = HashTool.ComputeHash(bytes);
				HashTool.Clear();

				return BitConverter.ToString(bytes2).Replace("-", string.Empty);
			}
		}
	}
}