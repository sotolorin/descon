using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Descon.Data;

namespace Descon.UI.DataAccess
{
	public static class SoftwareUpdate
	{
		private static CommonData _data;

		/// <summary>
		/// Checks for a new version of the software, optionally downloads it, then optionally installs it.
		/// </summary>
		public static void CheckForUpdate(CommonData data, bool displayNoUpdateMessage)
		{
			string result;
			int version;

			_data = data;

			try
			{
				Mouse.OverrideCursor = Cursors.Wait;
				//Setup the webclient and get the current local version number
				var myWebClient = new WebClient();
				myWebClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
				version = int.Parse(MiscMethods.GetVersionString().Replace(".", string.Empty));

				// Retrieve the latest version number from the server and remove any non-numerical digits
				result = myWebClient.DownloadString(new Uri("http://access.desconplus.com/api/CheckForUpdate/1"));
				var digitsOnly = new Regex(@"[^\d]");
				result = digitsOnly.Replace(result, string.Empty);
				Mouse.OverrideCursor = Cursors.Arrow;

				// If the server version is newer than the local version, we want to ask about an update
				if (!string.IsNullOrEmpty(result) && int.Parse(result) > version)
				{
					var messageBoxResult = MessageBox.Show("New version available. See Change Log after updating for change list. Download and notify once complete?",
						"UPDATE AVAILABLE", MessageBoxButton.OKCancel, MessageBoxImage.Information);
					if (messageBoxResult == MessageBoxResult.OK)
					{						
						// Attaches events used to update the UI for the user to show download progress
						myWebClient.DownloadFileCompleted += DownloadFileDownloadFileCompletedCallback;
						myWebClient.DownloadProgressChanged += DownloadProgressChangedCallback;
						myWebClient.DownloadFileAsync(new Uri("http://access.desconplus.com/api/CheckForUpdate/2"), ConstString.FILE_INSTALLER);
					}
				}
				else if (displayNoUpdateMessage)
					MessageBox.Show("No updates available.", "NO UPDATE", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (WebException)
			{
				MessageBox.Show("Update Server temporarily unavailable. Please try again in a few minutes.",
					"Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				Mouse.OverrideCursor = Cursors.Arrow;
			}
		}

		/// <summary>
		/// Handles installing the downloaded file by displaying the location and killing the current application after
		/// launching the installer
		/// </summary>
		private static void DownloadFileDownloadFileCompletedCallback(object sender, AsyncCompletedEventArgs e)
		{
			string messageBoxText = "New version downloaded to: " + ConstString.FILE_INSTALLER +
			                        "\r\rWould you like to install now? Descon will close and your work will be saved automatically.";

			if (CommonDataStatic.CurrentFilePath == ConstString.FILE_DEFAULT_NAME)
				messageBoxText += "\r\rYour work will be saved to: " + ConstString.FOLDER_MYDOCUMENTS_DESCON + ConstString.FILE_DEFAULT_NAME;

			var result = MessageBox.Show(messageBoxText, "INSTALL NEW VERSION?", MessageBoxButton.OKCancel, MessageBoxImage.Information);
			if (result == MessageBoxResult.OK)
			{
				if (CommonDataStatic.CurrentFilePath == ConstString.FILE_DEFAULT_NAME)
					CommonDataStatic.CurrentFilePath = ConstString.FOLDER_MYDOCUMENTS_DESCON + ConstString.FILE_DEFAULT_NAME;

				new SaveDataToXML().SaveDesconDrawing(CommonDataStatic.CurrentFilePath, true);

				if (File.Exists(ConstString.FILE_INSTALLER))
				{
					var procInfo = new ProcessStartInfo(ConstString.FILE_INSTALLER);
					Process.Start(procInfo);
					Application.Current.Shutdown();
				}
				else
					MessageBox.Show("Installer could not be found. Please check for updates again", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			_data.UpdateDownloadStatus = string.Empty;
		}

		/// <summary>
		/// Updates the percentage progress text display in the UI.
		/// </summary>
		private static void DownloadProgressChangedCallback(object sender, DownloadProgressChangedEventArgs e)
		{
			if (e.ProgressPercentage % 10 == 0)
				_data.UpdateDownloadStatus = "Download Percentage Complete: " + e.ProgressPercentage + "%";
		}
	}
}