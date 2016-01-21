using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using Descon.WebAccess;

namespace ServerLicenseProxy
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void CreateListener()
		{
			TcpListener tcpListener = null;
			IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];

			try
			{
				int port;

				if (int.TryParse(tbxPort.Text, out port) && port <= 65535 && port > 0)
				{
					lblMessage.Content = "Waiting for connection...";
					tcpListener = new TcpListener(ipAddress, port);
					tcpListener.Start();
				}
				else
				{
					MessageBox.Show("ERROR", "Please enter a valid port number.", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}
			catch (Exception e)
			{
				lblMessage.Content = "Error: " + e;
			}

			while (true)
			{
				// Always use a Sleep call in a while(true) loop to avoid locking up your CPU.
				Thread.Sleep(10);
				// Create a TCP socket.
				TcpClient tcpClient = tcpListener.AcceptTcpClient();
				// Read the data stream from the client.
				byte[] bytes = new byte[256];
				NetworkStream stream = tcpClient.GetStream();
				stream.Read(bytes, 0, bytes.Length);
				SocketHelper helper = new SocketHelper();
				helper.processMsg(tcpClient, stream, bytes);
			}
		}

		private class SocketHelper
		{
			string mstrMessage;
			private string result;
			byte[] bytesSent;
			readonly SQLRead _sqlRead = new SQLRead();

			public void processMsg(TcpClient client, NetworkStream stream, byte[] bytesReceived)
			{
				// Handle the message received and send a response back to the client.
				mstrMessage = Encoding.ASCII.GetString(bytesReceived, 0, bytesReceived.Length);

				var splitString = mstrMessage.Split('|');
				if (splitString.Count() == 2 && splitString[0] == "1")
					result = _sqlRead.GetLicenseType(splitString[1]);
				else if (splitString.Count() == 5 && splitString[0] == "1")
					result = _sqlRead.CheckAndUpdateComputerID(splitString[1], splitString[2]);
				else
					result = string.Empty;

				bytesSent = Encoding.ASCII.GetBytes(result);
				stream.Write(bytesSent, 0, bytesSent.Length);
			}
		}

		private void btnStart_Click(object sender, RoutedEventArgs e)
		{
			CreateListener();
		}

		private void btnExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}