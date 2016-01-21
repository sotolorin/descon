using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;
using Descon.Calculations;
using Descon.Data;
using DesconPM.DataAccess;
using Cursors = System.Windows.Input.Cursors;
using MessageBox = System.Windows.MessageBox;

namespace DesconPM.Main
{
	public partial class FormDesconPMMain
	{
		private void RefreshReport()
		{
			Mouse.OverrideCursor = Cursors.Wait;

			int index = 0;

			_projectData.ReportText = string.Empty;
			new LoadDataFromXML().LoadReportSettings();

			foreach (var drawing in _projectData.ProjectStructure.DrawingItems)
			{
				if (drawing.Checked) // If the user selected the drawing to be in the report, otherwise just bump the index up
				{
					_projectData.ReportText += "<div id=\"" + ConstString.REPORT_INDEX + index + "\">";
					_projectData.ReportText += "<b>Drawing " + (index + 1) + " - " + drawing.Name + " - " + drawing.Description + "</b></div>";
					_projectData.ReportText += "</br>";
					_projectData.ReportText += Calculations.RunCalculationsSilently(drawing.DetailDataList, _projectData.ThemeAccent);
					_projectData.ReportText += "</br></br>";
				}
				index++;
			}

			Mouse.OverrideCursor = Cursors.Arrow;
		}

		#region Main Project File Methods

		private void OpenProject()
		{
			string path = string.Empty;
			ProjectFileStructure saveFile;
			var reader = new XmlSerializer(typeof (ProjectFileStructure));

			var openDialog = new OpenFileDialog
			{
				AddExtension = true,
				DefaultExt = ConstString.FILE_EXTENSION_PROJECT,
				Filter = ConstString.FILE_DESCRIPTION_PROJECT + "|*" + ConstString.FILE_EXTENSION_PROJECT,
				Title = "Open Project"
			};

			if (openDialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
			{
				path = openDialog.FileName;

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

							saveFile = (ProjectFileStructure)reader.Deserialize(outFile);
						}
					}
				}

				if (saveFile != null)
					_projectData.ProjectStructure = saveFile;
			}
		}

		private void NewProject()
		{
			_projectData.ProjectStructure = new ProjectFileStructure
			{
				DateModified = DateTime.Now,
				DateCreated = DateTime.Now
			};
		}

		private void SaveProject()
		{
			_projectData.ProjectStructure.DateModified = DateTime.Now;

			if (!string.IsNullOrEmpty(_projectData.ProjectStructure.FileName))
				SaveProject(_projectData.ProjectStructure.FileName);
			else
				SaveAsProject();
		}

		private void SaveAsProject()
		{
			_projectData.ProjectStructure.DateModified = DateTime.Now;

			if (string.IsNullOrEmpty(_projectData.ProjectStructure.FileName))
				_projectData.ProjectStructure.FileName = _projectData.ProjectStructure.Name + ConstString.FILE_EXTENSION_PROJECT;

			var saveDialog = new SaveFileDialog
			{
				AddExtension = true,
				DefaultExt = ConstString.FILE_EXTENSION_PROJECT,
				Filter = ConstString.FILE_DESCRIPTION_PROJECT + "|*" + ConstString.FILE_EXTENSION_PROJECT,
				FileName = _projectData.ProjectStructure.FileName,
				OverwritePrompt = true,
				Title = "Save Project"
			};

			if (saveDialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
				SaveProject(saveDialog.FileName);
		}

		private void SaveProject(string fileName)
		{
			_projectData.ProjectStructure.FileName = fileName;

			using (var dataStream = new MemoryStream())
			{
				using (var xmlWriter = XmlWriter.Create(dataStream))
				{
					var xmlSerializer = new XmlSerializer(_projectData.ProjectStructure.GetType());
					xmlSerializer.Serialize(xmlWriter, _projectData.ProjectStructure);
					dataStream.Position = 0;

					// Create the compressed file
					using (var outFile = File.Create(fileName))
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
		}

		#endregion

		#region Drawing Collection Methods

		private void MoveDrawing(bool moveUp)
		{
			// Moves the selected item in the grid either up or down
			if (moveUp && dataGrid.SelectedIndex > 0)
				_projectData.ProjectStructure.DrawingItems.Move(dataGrid.SelectedIndex, --dataGrid.SelectedIndex);
			else if (!moveUp && dataGrid.SelectedIndex < _projectData.ProjectStructure.DrawingItems.Count - 1)
				_projectData.ProjectStructure.DrawingItems.Move(dataGrid.SelectedIndex, ++dataGrid.SelectedIndex);
			else
				return;

			IndexItemsAndAutoFitGrid();

			dataGrid.SelectedIndex = moveUp ? --dataGrid.SelectedIndex : ++dataGrid.SelectedIndex;
		}

		private void AddDrawing()
		{
			string[] fileNames;

			var openDialog = new OpenFileDialog
			{
				AddExtension = true,
				DefaultExt = ConstString.FILE_EXTENSION_DRAWING,
				Filter = ConstString.FILE_DESCRIPTION_DRAWING + "|*" + ConstString.FILE_EXTENSION_DRAWING,
				Title = "Open Drawing",
				Multiselect = true
			};

			if (openDialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
				fileNames = openDialog.FileNames;
			else
				return;

			foreach (var fileName in fileNames)
			{
				var file = new LoadDataFromXML().LoadDesconDrawing(fileName);
				if (file == null)
					MessageBox.Show("Invalid File.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
				else
				{
					var fileNameArray = fileName.Split("\\".ToCharArray());
					var name = fileNameArray[fileNameArray.Count() - 1];

					var projectItem = new DrawingItem
					{
						DetailDataList = file.DetailDataList,
						Name = name,
						Checked = true,
						Index = _projectData.ProjectStructure.DrawingItems.Count + 1
					};

					_projectData.ProjectStructure.DrawingItems.Add(projectItem);
				}
			}

			IndexItemsAndAutoFitGrid();
		}

		private void RemoveDrawing()
		{
			_projectData.ProjectStructure.DrawingItems.Remove((DrawingItem)dataGrid.SelectedItem);
			IndexItemsAndAutoFitGrid();
		}

		/// <summary>
		/// Using the current position in the grid, the index of each item is reset starting at 1
		/// </summary>
		private void IndexItemsAndAutoFitGrid()
		{
			int currentIndex = 1;

			foreach (var item in _projectData.ProjectStructure.DrawingItems)
			{
				item.Index = currentIndex;
				currentIndex++;
			}

			// Set the last column to fill the remainder of the grid
			var lastColumn = dataGrid.Columns.LastOrDefault();
			if (lastColumn != null)
				lastColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);

			dataGrid.UpdateLayout();

			// Autofit all other columns
			foreach (var column in dataGrid.Columns)
			{
				if (Equals(column, lastColumn))		// Skip the last column
					continue;

				column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
			}

			dataGrid.UpdateLayout();
		}

		#endregion
	}
}