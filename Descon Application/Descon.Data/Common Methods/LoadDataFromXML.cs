using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace Descon.Data
{
	public class LoadDataFromXML
	{
		/// <summary>
		/// Special loading method just for Unity. It doesn't need any of the special checking.
		/// </summary>
		public Preferences LoadPreferencesForUnity()
		{
			CommonDataStatic.MaterialDict = LoadMaterials();
			CommonDataStatic.WeldDict = LoadWelds();

			var xmlSerializer = new XmlSerializer(typeof (Preferences));
			using (var fileStream = File.OpenRead(ConstString.FILE_PREFERENCES))
			{
				var prefs = (Preferences)xmlSerializer.Deserialize(fileStream);
				fileStream.Close();
				return prefs;
			}
		}

		public LicensingData LoadLicenseFile()
		{
			LicensingData licenseFileStructure;

			if (!File.Exists(ConstString.FILE_LICENSE))
				return null;

			using (var inFile = File.OpenRead(ConstString.FILE_LICENSE))
			{
				//Create the decompressed stream
				using (var outFile = new MemoryStream())
				{
					using (var decompress = new DeflateStream(inFile, CompressionMode.Decompress))
					{
						//Copy the decompression stream into the output file. Buffer is arbitrary because we don't know how big the file will be. 
						var buffer = new byte[100000];
						int numRead;
						while ((numRead = decompress.Read(buffer, 0, buffer.Length)) != 0)
							outFile.Write(buffer, 0, numRead);

						outFile.Position = 0;

						var reader = new XmlSerializer(typeof (LicensingData));
						licenseFileStructure = (LicensingData)reader.Deserialize(outFile);
					}
				}
			}

			// Check to make sure the hash of the file matches the registry
			if (CommonDataStatic.LicenseType != ELicenseType.Developer_0)
			{
				using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Descon Plus", true))
				{
					if (key != null)
					{
						byte[] hashBytes = null;
						var hash = (string)key.GetValue("PS");
						if (hash != null)
							hashBytes = Convert.FromBase64String(hash);

						// Hashes don't match meaning the user replaced their file. So we replace theirs. Hah!
						if (hashBytes != null && Encoding.UTF8.GetString(hashBytes) != Encoding.UTF8.GetString(MiscMethods.GetHashOfFile(ConstString.FILE_LICENSE)))
						{
							new SaveDataToXML().SaveLicenseFile(new LicensingData());
							return new LicensingData();
						}
					}
				}
			}

			return licenseFileStructure;
		}

		/// <summary>
		/// Loads the Preferences file and and creates one if the current one is corrupt or missing
		/// </summary>
		public Preferences LoadPreferences()
		{
			var xmlSerializer = new XmlSerializer(typeof (Preferences));

			try
			{
				// Check to make sure the preferences haven't been updated and need to be replaced
				if (File.ReadAllText(ConstString.FILE_PREFERENCES_VERSION) != ConstString.PREFERENCES_VERSION)
				{
					File.WriteAllText(ConstString.FILE_PREFERENCES_VERSION, ConstString.PREFERENCES_VERSION);
					CommonDataStatic.Preferences = null;
				}
				else
				{
					using (var fileStream = File.OpenRead(ConstString.FILE_PREFERENCES))
					{
						CommonDataStatic.Preferences = (Preferences)xmlSerializer.Deserialize(fileStream);
						fileStream.Close();
					}

					if (CommonDataStatic.Units == EUnit.US && CommonDataStatic.Preferences.DefaultElectrode.Metric)
						CommonDataStatic.Preferences.DefaultElectrode = CommonDataStatic.WeldDict["E70XX"];
				}
			}
			catch
			{
				CommonDataStatic.Preferences = null;
			}
			finally
			{
				// This should only hit when something is wrong with the preferences file or it cannot be read
				if (CommonDataStatic.Preferences == null)
				{
					CommonDataStatic.Preferences = new Preferences();
					CommonDataStatic.Preferences.DefaultBoltUS = new Bolt(); // This has to be here because it relies on other data being set
					// This creates a new Metric bolt. The initial values of a bolt are set by the Unit system, 
					// so we have to swap it temporarily for the metric bolt to initialize properly
					CommonDataStatic.Preferences.Units = EUnit.Metric;
					CommonDataStatic.Preferences.DefaultBoltMetric = new Bolt();
					CommonDataStatic.Preferences.Units = EUnit.US;

					new SaveDataToXML().SavePreferences();
				}
			}

			return CommonDataStatic.Preferences;
		}


		/// <summary>
		/// Loads the  report settings file for the project manager
		/// </summary>
		public void LoadReportSettings()
		{
			var xmlSerializer = new XmlSerializer(typeof (ReportSettings));

			if (!File.Exists(ConstString.FILE_PREFERENCES_MANAGER))
				new SaveDataToXML().SaveReportSettings();

			using (var fileStream = File.OpenRead(ConstString.FILE_PREFERENCES_MANAGER))
			{
				CommonDataStatic.Preferences.ReportSettings = (ReportSettings)xmlSerializer.Deserialize(fileStream);
			}
		}

		/// <summary>
		/// Loads the materials file
		/// </summary>
		public List<List<double>> LoadEccentricWeldCoefficients()
		{
			List<List<double>> coefficients;
			var reader = new XmlSerializer(typeof(List<List<double>>));
			string directoryName = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
			var assem = Assembly.LoadFrom(directoryName + "\\Descon.Resources.dll");

			using (Stream stream = assem.GetManifestResourceStream("Descon.Resources.Documents.EccentricWeldCoefficients.xml"))
			{
				coefficients = (List<List<double>>)reader.Deserialize(stream);
			}

			return coefficients;
		}

		/// <summary>
		/// Loads the materials file
		/// </summary>
		public Dictionary<string, Material> LoadMaterials()
		{
			var materials = new Dictionary<string, Material>();
			var reader = new XmlSerializer(typeof (List<Material>));
			string directoryName = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
			var assem = Assembly.LoadFrom(directoryName + "\\Descon.Resources.dll");

			using (Stream stream = assem.GetManifestResourceStream("Descon.Resources.Documents.Materials.xml"))
			{
				var materialList = (List<Material>)reader.Deserialize(stream);
				foreach (var material in materialList)
					materials.Add(material.Name, material);
			}

			if (File.Exists(ConstString.FILE_USER_MATERIALS))
			{
				using (var fileStream = File.OpenRead(ConstString.FILE_USER_MATERIALS))
				{
					var materialList = (List<Material>)reader.Deserialize(fileStream);
					foreach (var material in materialList)
					{
						material.UserDefined = true;
						materials.Add(material.Name, material);
					}
				}
			}

			return materials;
		}

		/// <summary>
		/// Loads the Welds file
		/// </summary>
		public Dictionary<string, Weld> LoadWelds()
		{
			var welds = new Dictionary<string, Weld>();
			var reader = new XmlSerializer(typeof(List<Weld>));
			string directoryName = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
			var assem = Assembly.LoadFrom(directoryName + "\\Descon.Resources.dll");

			using (Stream stream = assem.GetManifestResourceStream("Descon.Resources.Documents.Welds.xml"))
			{
				var weldList = (List<Weld>)reader.Deserialize(stream);
				foreach (var weld in weldList)
					welds.Add(weld.Name, weld);
			}

			if (File.Exists(ConstString.FILE_USER_WELDS))
			{
				using (var fileStream = File.OpenRead(ConstString.FILE_USER_WELDS))
				{
					var weldList = (List<Weld>)reader.Deserialize(fileStream);
					foreach (var weld in weldList)
					{
						weld.UserDefined = true;
						welds.Add(weld.Name, weld);
					}
				}
			}

			return welds;
		}

		/// <summary>
		/// Load a saved file. The dicionary itself cannot be saved normally, so a SaveFileStructure class has been created to load the data
		/// and then put it into a dictionary the application can use. If dataLoadOnly is set to true, then the data in CommonDataStatic
		/// is not set. Used to test the file for specific values or conditions.
		/// </summary>
		public SaveFileStructure LoadDesconDrawing(string path, bool dataLoadOnly = false)
		{
			//string test = CommonDataStatic.BeamToColumnType.ToString(); //put in code to see where we are failing

			CommonDataStatic.LoadingFileInProgress = true;

			var reader = new XmlSerializer(typeof (SaveFileStructure));
			SaveFileStructure saveFileStructure;
			bool oldFileType = false;
			bool isFileMetric;

			if (!File.Exists(path))
				return null;

			try
			{
				// Checks to see if we have an uncompressed file. Backwards compatibility and also for Unity
				using (var readFirstLine = new StreamReader(path))
				{
					var firstLine = readFirstLine.ReadLine();
					if (firstLine != null && firstLine.Contains("xml version"))
						oldFileType = true;
					readFirstLine.Close();
				}
				if (oldFileType)
				{
					if (path != ConstString.FILE_UNITY_DIMENSIONS)
					{
						using (var text = File.OpenText(path))
						{
							isFileMetric = text.ReadToEnd().Contains("<UnitSystem>Metric</UnitSystem>");

							if (isFileMetric && CommonDataStatic.Units == EUnit.US)
							{
								CommonDataStatic.Preferences.Units = EUnit.Metric;
								ConvertUnits.UnitsChanged();
							}
							else if (!isFileMetric && CommonDataStatic.Units == EUnit.Metric)
							{
								CommonDataStatic.Preferences.Units = EUnit.US;
								ConvertUnits.UnitsChanged();
							}
						}
					}
					using (var fileStream = File.OpenRead(path))
					{
						saveFileStructure = (SaveFileStructure)reader.Deserialize(fileStream);
						fileStream.Close();
					}
				}
				else
				{
					using (var inFile = File.OpenRead(path))
					{
						//Create the decompressed stream
						using (var outFile = new MemoryStream())
						{
							using (var decompress = new DeflateStream(inFile, CompressionMode.Decompress))
							{
								//Copy the decompression stream into the output file. Buffer is arbitrary because we don't know how big the file will be. 
								var buffer = new byte[2000000];
								int numRead;
								while ((numRead = decompress.Read(buffer, 0, buffer.Length)) != 0)
									outFile.Write(buffer, 0, numRead);

								outFile.Position = 0;

								// Check the unit system of the file and set the current unit system to whatever it is
								string completeFile = Encoding.ASCII.GetString(outFile.ToArray());
								isFileMetric = completeFile.Contains("<UnitSystem>Metric</UnitSystem>");
								if (isFileMetric && CommonDataStatic.Units == EUnit.US)
								{
									CommonDataStatic.Preferences.Units = EUnit.Metric;
									ConvertUnits.UnitsChanged();
								}
								else if (!isFileMetric && CommonDataStatic.Units == EUnit.Metric)
								{
									CommonDataStatic.Preferences.Units = EUnit.US;
									ConvertUnits.UnitsChanged();
								}

								outFile.Position = 0;

								saveFileStructure = (SaveFileStructure)reader.Deserialize(outFile);
								decompress.Close();
							}

							outFile.Close();
						}

						inFile.Close();
					}
				}

				if (dataLoadOnly)
					return saveFileStructure;

				if (saveFileStructure != null && path != ConstString.FILE_UNITY_DIMENSIONS)
				{
					CommonDataStatic.JointConfig = saveFileStructure.JointConfig;
					CommonDataStatic.Preferences.CalcMode = saveFileStructure.CalcMode;
					CommonDataStatic.Preferences.SteelCode = saveFileStructure.SteelCode;
					CommonDataStatic.Preferences.BracingType = saveFileStructure.BracingType;
					CommonDataStatic.ColumnSplice = saveFileStructure.ColumnSplice;
					CommonDataStatic.ColumnStiffener = saveFileStructure.ColumnStiffener;
					CommonDataStatic.Preferences.Seismic = saveFileStructure.Seismic;
					CommonDataStatic.SeismicSettings = saveFileStructure.SeismicSettings;
					CommonDataStatic.ReportHighlightList = saveFileStructure.ReportHighlightList;
					CommonDataStatic.ReportBookmarkList = saveFileStructure.ReportBookmarkList;
					CommonDataStatic.ReportCommentList = new Dictionary<string, string>();

					foreach (var comment in saveFileStructure.ReportCommentList)
						CommonDataStatic.ReportCommentList.Add(comment.LineNumber, comment.Comment);

					if (CommonDataStatic.DetailDataDict != null)
						CommonDataStatic.DetailDataDict.Clear();
					else
						CommonDataStatic.DetailDataDict = new Dictionary<EMemberType, DetailData>();
					foreach (var detail in saveFileStructure.DetailDataList)
						CommonDataStatic.DetailDataDict.Add(detail.MemberType, detail);
					if (!CommonDataStatic.DetailDataDict.Any())
						return null;
				}

				CommonDataStatic.LoadingFileInProgress = false;

				return saveFileStructure;
			}
			catch (IOException e)
			{
				//File.WriteAllText(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "error io" + DateTime.Now.Ticks + ".txt", e.Message);
				return null;
			}
			catch (Exception e)
			{
				//File.WriteAllText(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "error" + DateTime.Now.Ticks + ".txt", e.Message + test);
				//if (e.InnerException != null)
				//	File.WriteAllText(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA + "error inner" + DateTime.Now.TimeOfDay + ".txt", e.InnerException.Message);
				return null;
			}
		}

		public void LoadShapes()
		{
			List<Shape> shapes;
			CommonDataStatic.AllShapes = new Dictionary<string, Shape> {{ConstString.NONE, new Shape()}};
			string directoryName = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
			var assem = Assembly.LoadFrom(directoryName + "\\Descon.Resources.dll");

			using (Stream stream = assem.GetManifestResourceStream("Descon.Resources.Documents.Shapes.xml"))
			{
				var reader = new XmlSerializer(typeof (List<Shape>));
				shapes = (List<Shape>)reader.Deserialize(stream);
			}

			foreach (var shape in shapes)
			{
				switch (shape.type)
				{
					case "":
						shape.TypeEnum = EShapeType.None;
						break;
					case "W":
					case "S":
					case "HP":
						shape.TypeEnum = EShapeType.WideFlange;
						break;
					case "WT":
					case "MT":
					case "ST":
						shape.TypeEnum = EShapeType.WTSection;
						break;
					case "L":
						shape.TypeEnum = EShapeType.SingleAngle;
						break;
					case "2L":
						shape.TypeEnum = EShapeType.DoubleAngle;
						break;
					case "HSS":
					case "PIPE":
						shape.TypeEnum = EShapeType.HollowSteelSection;
						break;
					case "C":
					case "MC":
						shape.TypeEnum = EShapeType.SingleChannel;
						break;
					case "2C":
					case "2MC":
						shape.TypeEnum = EShapeType.DoubleChannel;
						break;
				}

				// Some metric shape data is saved as mm^3/10^3 which is equivalent to 1000 * value
				if (shape.UnitSystem == EUnit.Metric)
				{
					shape.sx *= 1000;
					shape.sx_A *= 1000;
					shape.sy *= 1000;
					shape.sy_A *= 1000;
					shape.zx *= 1000;
					shape.zx_A *= 1000;
					shape.zy *= 1000;
					shape.zy_A *= 1000;
					shape.sz *= 1000;
					shape.qf *= 1000;
				}

				if (shape.TypeEnum != EShapeType.None)
				{
					shape.User = false;
					// Don't add shapes with more than 3 parts (1X1X1X1 wouldn't show up)
					if (shape.Name.Split('X').Length <= 3)
						CommonDataStatic.AllShapes.Add(shape.Name, shape);
				}
			}

			ConvertUnits.ReloadAngleShapeLists();
		}
	}
}