using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace Utilities
{
	/// <summary>
	/// Interaction logic for AdditionalUtilities.xaml
	/// </summary>
	public partial class AdditionalUtilities
	{
		public AdditionalUtilities()
		{
			InitializeComponent();
		}

		private void convertCoeffsToXML_Click(object sender, RoutedEventArgs e)
		{
			string fileText;

			var coefficients = new List<double>();
			var coefficientList = new List<List<double>>();

			var openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == true)
			{
				fileText = File.ReadAllText(openFileDialog.FileName);

				for (int i = 0; i < fileText.Length; i += 4)
					coefficients.Add(double.Parse(fileText.Substring(i, 4)));
			}

			for (int i = 0; i < coefficients.Count / 23; i++)
				coefficientList.Add(new List<double>());

			int listCounter = 0;

			for (int i = 1; i <= coefficients.Count; i++)
			{
				coefficientList[listCounter].Add(coefficients[i - 1]);

				if (i % 23 == 0)
					listCounter++;
			}

			var writer = new XmlSerializer(typeof(List<List<double>>));

			using (var fileStream = new StreamWriter(@"C:\Files\Descon\Coeffs.xml"))
			{
				writer.Serialize(fileStream, coefficientList);
			}
		}
	}
}