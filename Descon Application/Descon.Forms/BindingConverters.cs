using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Descon.Data;

namespace Descon.Forms
{
	/// <summary>
	/// Inverts bool values to avoid having to use an additional parameter
	/// </summary>
	public sealed class InvertBool : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}
	}

	/// <summary>
	/// Inverts Visbility values to avoid having to use an additional parameter
	/// </summary>
	public sealed class InvertVisibility : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (Visibility)value == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (Visibility)value == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
		}
	}

	/// <summary>
	/// Converts fractional numbers entered as X / 16 to decimals and then back again.
	/// </summary>
	public sealed class ConvertFraction : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || value.ToString() == string.Empty)
				return 0;
			else if (CommonDataStatic.Units == EUnit.US)
				return Math.Round(double.Parse(value.ToString()) * 16.0, MidpointRounding.AwayFromZero);
			else
				return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || value.ToString() == string.Empty)
				return 0;
			else if (CommonDataStatic.Units == EUnit.US)
				return double.Parse(value.ToString()) / 16.0;
			else
				return value;
		}
	}

	/// <summary>
	/// Converts fractional numbers entered as X / 16 to decimals and then back again.
	/// </summary>
	public sealed class ConvertInchesToFeet : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || value.ToString() == string.Empty)
				return 0;
			else if (CommonDataStatic.Units == EUnit.US)
				return NumberFun.Round(double.Parse(value.ToString()) / 12.0, ERoundingPrecision.WholeNumber, ERoundingStyle.Nearest);
			else
				return NumberFun.Round(double.Parse(value.ToString()) / 1000.0, ERoundingPrecision.Half, ERoundingStyle.Nearest);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || value.ToString() == string.Empty)
				return 0;
			else if (CommonDataStatic.Units == EUnit.US)
				return double.Parse(value.ToString()) * 12.0;
			else
				return double.Parse(value.ToString()) * 1000.0;
		}
	}

	/// <summary>
	/// Converts enum values to bools so we can determine if a particular radio button is selected for WPF bindings
	/// </summary>
	public sealed class EnumToBool : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.Equals(parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.Equals(true) ? parameter : Binding.DoNothing;
		}
	}

	/// <summary>
	/// Converts bools to Visibility to use in the UI. Returns Collapsed or Visible
	/// </summary>
	public sealed class BoolToVisibilityCollapsed : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.Equals(true) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.Equals(Visibility.Visible);
		}
	}

	/// <summary>
	/// Converts bools to Visibility to use in the UI. Returns Hidden or Visible
	/// </summary>
	public sealed class BoolToVisibilityHidden : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.Equals(true) ? Visibility.Visible : Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.Equals(Visibility.Visible);
		}
	}

	/// <summary>
	/// Converts bools to Colors that are used for the backgrounds of buttons. True = selected color
	/// </summary>
	public sealed class BoolToColor : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.Equals(true) ? new SolidColorBrush(Color.FromRgb(225, 225, 225)) : new SolidColorBrush(Colors.White);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.Equals(new SolidColorBrush(Color.FromRgb(225, 225, 225)));
		}
	}
}