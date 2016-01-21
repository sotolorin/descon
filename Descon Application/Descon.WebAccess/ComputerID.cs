using System;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Windows;

namespace Descon.WebAccess
{
	public static class ComputerID
	{
		/// <summary>
		/// Returns the MAC Address of the current system used for licensing
		/// </summary>
		/// <returns></returns>
		public static string GetComputerID()
		{
			string id = String.Empty;

			try
			{
				var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");

				foreach (var queryObj in searcher.Get())
				{
					id = queryObj["Architecture"] + "|";
					id += queryObj["Caption"] + "|";
					id += queryObj["Family"] + "|";
					id += queryObj["ProcessorId"] + "|";
				}

				id += NetworkInterface.GetAllNetworkInterfaces()
					.Where(i => i.NetworkInterfaceType != NetworkInterfaceType.Loopback && i.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
					.OrderBy(i => i.Name)
					.First()
					.GetPhysicalAddress()
					.ToString();
			}
			catch (ManagementException e)
			{
				MessageBox.Show("An error occurred while querying machine ID: " + e.Message);
			}

			return id;
		}
	}
}