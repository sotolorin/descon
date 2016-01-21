using System;
using System.Threading;
using System.Windows;

namespace CalcPad.Resources
{
	public class GetResources
	{
		public ResourceDictionary GetLanguageDictionary()
		{
			ResourceDictionary dict = new ResourceDictionary();

			switch (Thread.CurrentThread.CurrentCulture.ToString())
			{
				case "en-US":
					dict.Source = new Uri("pack://application:,,,/CalcPad.Resources;component/Languages/StringResources.en-US.xaml");
					break;
				case "en-MX":
					dict.Source = new Uri("pack://application:,,,/CalcPad.Resources;component/Languages/StringResources.es-MX.xaml");
					break;
				default:
					dict.Source = new Uri("pack://application:,,,/CalcPad.Resources;component/Languages/StringResources.en-US.xaml");
					break;
			}
			return dict;
		}

		public ResourceDictionary GetUnitDictionary()
		{
			ResourceDictionary dict = new ResourceDictionary();
			EUnitSystem unitSystem = EUnitSystem.Imperial;

			switch (unitSystem)
			{
				case EUnitSystem.Imperial:
					dict.Source = new Uri("pack://application:,,,/CalcPad.Resources;component/UnitStrings/Units.Imperial.xaml");
					break;
				case EUnitSystem.Metric:
					dict.Source = new Uri("pack://application:,,,/CalcPad.Resources;component/UnitStrings/Units.Metric.xaml");
					break;
				default:
					dict.Source = new Uri("pack://application:,,,/CalcPad.Resources;component/UnitStrings/Units.Imperial.xaml");
					break;
			}
			return dict;
		}
	}
}