using System;
using System.Net.Sockets;
using System.Text;

namespace Descon.WebAccess
{
	public class LicenseProxyClient
	{
		public string Connect(string serverIP, int serverPort, string message)
		{
			string output;

			try
			{
				TcpClient client = new TcpClient(serverIP, serverPort);

				// Translate the passed message into ASCII and store it as a byte array.
				var data = Encoding.ASCII.GetBytes(message);

				// Get a client stream for reading and writing.
				// Stream stream = client.GetStream();
				var stream = client.GetStream();

				// Send the message to the connected TcpServer. 
				stream.Write(data, 0, data.Length);

				output = "Sent: " + message;

				// Buffer to store the response bytes.
				data = new Byte[256];

				// String to store the response ASCII representation.
				String responseData;

				// Read the first batch of the TcpServer response bytes.
				Int32 bytes = stream.Read(data, 0, data.Length);
				responseData = Encoding.ASCII.GetString(data, 0, bytes);
				output += " - Received: " + responseData;

				// Close everything.
				stream.Close();
				client.Close();

				return output;
			}
			catch (ArgumentNullException e)
			{
				output = "ArgumentNullException: " + e;
				return output;
			}
			catch (SocketException e)
			{
				output = "SocketException: " + e;
				return output;
			}
		}
	}
}
