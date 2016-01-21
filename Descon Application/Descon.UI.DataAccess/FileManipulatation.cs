using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Descon.Data;

namespace Descon.UI.DataAccess
{
	/// <summary>
	/// Methods for saving and loading all the various files used throughout Descon
	/// </summary>
    public class FileManipulatation
	{
		/// <summary>
		/// Checks for existing files necessary for Descon and replaces them with files built into the DLL if needed.
		/// </summary>
		public void ReplaceMissingFiles()
		{
			try
			{
				DirectoryInfo downloadedMessageInfo = new DirectoryInfo(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA);
				foreach (FileInfo file in downloadedMessageInfo.GetFiles())
				{
					if (file.Extension == ConstString.FILE_EXTENSION_DRAWING && file.Name != ConstString.FILE_DEFAULT_NAME)
						file.Delete();
				}

				if (File.Exists(ConstString.FILE_INSTALLER))
					File.Delete(ConstString.FILE_INSTALLER);
				if (File.Exists(ConstString.FILE_UNITY_DRAWING))
					File.Delete(ConstString.FILE_UNITY_DRAWING);

				// First create any missing directories
				if (!Directory.Exists(ConstString.FOLDER_MYDOCUMENTS_DESCON))
					Directory.CreateDirectory(ConstString.FOLDER_MYDOCUMENTS_DESCON);
				if (!Directory.Exists(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA))
					Directory.CreateDirectory(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA);
				if (!Directory.Exists(ConstString.FOLDER_DESCONDATA_BUG_REPORT))
					Directory.CreateDirectory(ConstString.FOLDER_DESCONDATA_BUG_REPORT);
				if (!Directory.Exists(ConstString.FOLDER_MYDOCUMENTS_SCREENSHOTS))
					Directory.CreateDirectory(ConstString.FOLDER_MYDOCUMENTS_SCREENSHOTS);
				if (!Directory.Exists(ConstString.FOLDER_DESCONDATA_BUG_REPORT))
					Directory.CreateDirectory(ConstString.FOLDER_DESCONDATA_BUG_REPORT);
				if (!Directory.Exists(ConstString.FOLDER_DESCONDATA_THEMES))
					Directory.CreateDirectory(ConstString.FOLDER_DESCONDATA_THEMES);

				// Check for and replace the data file version file if missing. This will replace the shape files if needed.
				if (!File.Exists(ConstString.FILE_DATA_FILE_VERSION))
					WriteResourceToFile(ConstString.FILE_DATA_FILE_VERSION);
				if (!File.Exists(ConstString.FILE_PREFERENCES_VERSION))
					WriteResourceToFile(ConstString.FILE_PREFERENCES_VERSION);

				// Temporary for a few builds since these files aren't needed any more (first build 8.0.0.30)
				if (File.Exists(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "Welds.xml"))
					File.Delete(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "Welds.xml");
				if (File.Exists(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "Materials.xml"))
					File.Delete(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "Materials.xml");
				if (File.Exists(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "Shapes.xml"))
					File.Delete(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "Shapes.xml");
				if (File.Exists(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "DataFileVersion.dat"))
					File.Delete(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "DataFileVersion.dat");
				if (File.Exists(ConstString.FOLDER_MYDOCUMENTS_DESCON + "License - MahApps Metro.rtf"))
					File.Delete(ConstString.FOLDER_MYDOCUMENTS_DESCON + "License - MahApps Metro.rtf");
				if (File.Exists(ConstString.FOLDER_MYDOCUMENTS_DESCON + "License - wkhtmltopdf.rtf"))
					File.Delete(ConstString.FOLDER_MYDOCUMENTS_DESCON + "License - wkhtmltopdf.rtf");

				if (!File.Exists(ConstString.FILE_LOGO))
					WriteResourceToFile(ConstString.FILE_LOGO);

				// Other necessary files to copy
				WriteResourceToFile(ConstString.FILE_BOOKMARK_PNG);
				WriteResourceToFile(ConstString.FILE_EDIT_TEXT_PNG);
				WriteResourceToFile(ConstString.FILE_CHANGELOG);
				WriteResourceToFile(ConstString.FILE_EULA);
			}
			catch (Exception)
			{
				// Do nothing and fail silently
			}
		}

		/// <summary>
		/// Writes resource files internal to the DLL to the hard drive. Parameters come from ConstString
		/// </summary>
		/// <param name="savePath">Save path on hard drive</param>
		public void WriteResourceToFile(string savePath)
		{
			try
			{
				int count = savePath.Split('\\').Count();
				var resourceName = savePath.Split('\\')[count - 1];
				string directoryName = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
				var assembly = Assembly.LoadFrom(directoryName + "\\Descon.Resources.dll");
				using (var resource = assembly.GetManifestResourceStream("Descon.Resources.Documents." + resourceName))
				{
					using (var file = new FileStream(savePath, FileMode.Create, FileAccess.Write))
					{
						if (resource != null)
							resource.CopyTo(file);
					}
				}
			}
			catch (Exception )
			{
				FileAttributes attributes = File.GetAttributes(savePath);

				if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				{
					attributes = RemoveAttribute(attributes, FileAttributes.ReadOnly);
					File.SetAttributes(savePath, attributes);
					WriteResourceToFile(savePath);
				}
			}
		}

		private FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
		{
			return attributes & ~attributesToRemove;
		}
	}
}