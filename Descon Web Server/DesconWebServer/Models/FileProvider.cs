using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DesconWebServer.Models
{
	public interface IFileProvider
	{
		bool Exists(string fileName);
		FileStream Open(string filePath);
		long GetLength(string name);
		string GetLatestVersionText();
		string GetFilePath(string name);
	}

	public class FileProvider : IFileProvider
	{
		private readonly string _filesDirectory;
		private const string PublicReleaseLocationKey = "FolderPublicRelease";
		private const string InternalReleaseLocationKey = "FolderInternalRelease";

		public FileProvider(bool publicRelease)
		{
			string fileLocation;

			if(publicRelease)
				fileLocation = ConfigurationManager.AppSettings[PublicReleaseLocationKey];
			else
				fileLocation = ConfigurationManager.AppSettings[InternalReleaseLocationKey];
			
			if (!String.IsNullOrWhiteSpace(fileLocation))
				_filesDirectory = fileLocation;
		}

		public bool Exists(string fileName)
		{
			Regex digitsOnly = new Regex(@"[^\d]");
			fileName = digitsOnly.Replace(fileName, string.Empty) + ".exe";

			// Make sure we dont access directories outside of our main folder for security reasons
			string file = Directory.GetFiles(_filesDirectory, fileName, SearchOption.TopDirectoryOnly).FirstOrDefault();
			return file != null;
		}

		public FileStream Open(string filePath)
		{
			return File.Open(GetFilePath(filePath), FileMode.Open, FileAccess.Read);
		}

		public long GetLength(string name)
		{
			return new FileInfo(GetFilePath(name)).Length;
		}

		public string GetLatestVersionText()
		{
			var fileNameList = new List<int>();
			var fileList = Directory.GetFiles(_filesDirectory).ToList();

			foreach (var fileName in fileList)
			{
				int fileVersionName;

				var digitsOnly = new Regex(@"[^\d]");
				string fileVersion = digitsOnly.Replace(fileName, string.Empty);

				if (int.TryParse(fileVersion, out fileVersionName))
					fileNameList.Add(fileVersionName);
			}

			fileNameList.Sort();
			return fileNameList.Last().ToString();
		}

		public string GetFilePath(string name)
		{
			return Path.Combine(_filesDirectory, name);
		}
	}
}