using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace Descon.Data
{
	public class SaveDataToXML
	{
		private int _tryCounter;

		/// <summary>
		/// Saves all of the current data as a drawing file. Note that this only saves numerical data and not the actual drawing.
		/// </summary>
		public void SaveDesconDrawing(string path, bool compressFile)
		{
			var file = new SaveFileStructure();
			var writer = new XmlSerializer(typeof (SaveFileStructure));

			file.JointConfig = CommonDataStatic.JointConfig;
			file.SteelCode = CommonDataStatic.Preferences.SteelCode;
			file.CalcMode = CommonDataStatic.Preferences.CalcMode;
			file.BracingType = CommonDataStatic.Preferences.BracingType;
			file.UnitSystem = CommonDataStatic.Units;
			file.ColumnSplice = CommonDataStatic.ColumnSplice;
			file.ColumnStiffener = CommonDataStatic.ColumnStiffener;
			file.Seismic = CommonDataStatic.Preferences.Seismic;
			file.SeismicSettings = CommonDataStatic.SeismicSettings;
			file.ReportBookmarkList = CommonDataStatic.ReportBookmarkList;
			file.ReportHighlightList = CommonDataStatic.ReportHighlightList;
			file.LicenseType = CommonDataStatic.LicenseType;
			file.ReportCommentList = new List<ReportComment>();

			foreach (var comment in CommonDataStatic.ReportCommentList)
				file.ReportCommentList.Add(new ReportComment {LineNumber = comment.Key, Comment = comment.Value});

			file.DetailDataList = CommonDataStatic.DetailDataDict.Select(detail => detail.Value).ToList();

			try
			{
				// Unity doesn't know how to decompress properly, so we give it an uncompressed file.
				if (!compressFile)
				{
					using (var fileStream = new StreamWriter(path))
					{
						writer.Serialize(fileStream, file);
						fileStream.Close();
					}
				}
				else
				{
					// Sends the save command to Unity and then waits for it to finish
					int waitCounter = 0;
					bool unitySaveSuccess = true;
					while (!CommonDataStatic.UnityDoneSaving)
					{
						if (waitCounter++ == 300) // Give up after 3 seconds
						{
							unitySaveSuccess = false;
							break;
						}
					}

					// Loads the dimension positioning data from Unity to save in the file.
					if (unitySaveSuccess)
					{
						var saveFile = new LoadDataFromXML().LoadDesconDrawing(ConstString.FILE_UNITY_DIMENSIONS, false);
                        if (saveFile != null)
                        {
                            file.DimensionData = saveFile.DimensionData;
                            file.CameraData = saveFile.CameraData;
                        }
					}

					CompressAndSaveFile(file, path);
				}
			}
			catch (IOException)
			{
			}
			finally
			{
				CommonDataStatic.UnityDoneSaving = false;
			}
		}

		public void SaveLicenseFile(object data)
		{
			CompressAndSaveFile(data, ConstString.FILE_LICENSE);
		}

		private void CompressAndSaveFile(object data, string path)
		{
			using (var dataStream = new MemoryStream())
			{
				using (var xmlWriter = XmlWriter.Create(dataStream))
				{
					var xmlSerializer = new XmlSerializer(data.GetType());
					xmlSerializer.Serialize(xmlWriter, data);
					dataStream.Position = 0;

					// Create the compressed file
					using (var outFile = File.Create(path))
					{
						using (var compress = new DeflateStream(outFile, CompressionMode.Compress))
						{
							// Copy the source data into the compression stream and write it out
							var buffer = new byte[dataStream.Length];
							int numRead;
							while ((numRead = dataStream.Read(buffer, 0, buffer.Length)) != 0)
								compress.Write(buffer, 0, numRead);
						}
					}
				}
			}

			using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Descon Plus", true))
			{
				if (key != null)
				{
					byte[] dateBytes = MiscMethods.GetHashOfFile(ConstString.FILE_LICENSE);
					key.SetValue("PS", Convert.ToBase64String(dateBytes));
				}
			}
		}

		/// <summary>
		/// Unity uses this to save the dimensions to a seperate file
		/// </summary>
        public void SaveDimensionsForUnity(List<DimensionData> dimensionData, List<CameraData> cameraData)
		{
			var file = new SaveFileStructure {DimensionData = dimensionData, CameraData = cameraData};
			var writer = new XmlSerializer(typeof(SaveFileStructure));

			using (var fileStream = new StreamWriter(ConstString.FILE_UNITY_DIMENSIONS))
			{
				writer.Serialize(fileStream, file);
			}
		}

		/// <summary>
		/// Saves the preferences file
		/// </summary>
		public void SavePreferences()
		{
			try
			{
				var writer = new XmlSerializer(typeof (Preferences));

				using (var fileStream = new StreamWriter(ConstString.FILE_PREFERENCES))
				{
					writer.Serialize(fileStream, CommonDataStatic.Preferences);
				}
			}
			catch (IOException e)
			{
				if (_tryCounter > 10)
					return;
				Thread.Sleep(50);
				_tryCounter++;
				SavePreferences();
			}
			finally
			{
				_tryCounter = 0;
			}
		}

		/// <summary>
		/// Saves just the Report Settings to a seperate file. Currently used for the Project Manager
		/// </summary>
		public void SaveReportSettings()
		{
			try
			{
				var writer = new XmlSerializer(typeof(ReportSettings));

				using (var fileStream = new StreamWriter(ConstString.FILE_PREFERENCES_MANAGER))
				{
					writer.Serialize(fileStream, CommonDataStatic.Preferences.ReportSettings);
				}
			}
			catch (IOException)
			{
				if (_tryCounter > 20)
					return;
				Thread.Sleep(50);
				SaveReportSettings();
				_tryCounter++;
			}
			finally
			{
				_tryCounter = 0;
			}
		}

        /// <summary>
        /// Saves the preferences file Use Only for Testing
        /// </summary>
        public void TestSavePreferences()
        {
            try
            {
                var writer = new XmlSerializer(typeof(Preferences));

                using (var fileStream = new StreamWriter(ConstString.FOLDER_LOCATION))
                {
                    writer.Serialize(fileStream, CommonDataStatic.Preferences);
                }
            }
            catch (IOException)
            {
                if (_tryCounter > 20)
                    return;
                Thread.Sleep(50);
                TestSavePreferences();
                _tryCounter++;
            }
            finally
            {
                _tryCounter = 0;
            }
        }


		/// <summary>
		/// Saves the user shapes
		/// </summary>
		public void SaveUserShapes()
		{
			var writer = new XmlSerializer(typeof(List<Shape>));
			var shapeList = CommonDataStatic.AllShapes.Where(s => s.Value.User).Select(shape => shape.Value).ToList();

			using (var fileStream = new StreamWriter(ConstString.FILE_USER_SHAPES, false))
			{
				writer.Serialize(fileStream, shapeList);
			}
		}

		/// <summary>
		/// Saves the materials to a user file if edited
		/// </summary>
		public void SaveMaterials()
		{
			var writer = new XmlSerializer(typeof(List<Material>));

			using (var fileStream = new StreamWriter(ConstString.FILE_USER_MATERIALS, false))
			{
				writer.Serialize(fileStream, CommonDataStatic.MaterialDict.Select(m => m.Value).Where(m => m.UserDefined).ToList());
			}
		}

		/// <summary>
		/// Saves the Welds to a user file if edited
		/// </summary>
		public void SaveWelds()
		{
			var writer = new XmlSerializer(typeof(List<Weld>));

			using (var fileStream = new StreamWriter(ConstString.FILE_USER_WELDS, false))
			{
				writer.Serialize(fileStream, CommonDataStatic.WeldDict.Select(w => w.Value).Where(w => w.UserDefined).ToList());
			}
		}
	}
}