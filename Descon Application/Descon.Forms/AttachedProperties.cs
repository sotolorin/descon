using System.Windows;
using System.Windows.Controls;

namespace Descon.Forms
{
	/// <summary>
	/// Automatic margin setter that can be used in a specific control type such as panels. Each item's margin would not
	/// have to be set manually.
	/// </summary>
	public class MarginSetterPanel
	{
		public static Thickness GetMargin(DependencyObject obj)
		{
			return (Thickness) obj.GetValue(MarginProperty);
		}

		public static void SetMargin(DependencyObject obj, Thickness value)
		{
			obj.SetValue(MarginProperty, value);
		}

		public static readonly DependencyProperty MarginProperty =
			DependencyProperty.RegisterAttached("Margin",
				typeof (Thickness),
				typeof (MarginSetterPanel),
				new UIPropertyMetadata(new Thickness(),
				MarginChangedCallback));

		private static void MarginChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
		{
			var panel = sender as Panel;

			if (panel == null) return;
			panel.Loaded += panel_Loaded;
		}

		private static void panel_Loaded(object sender, RoutedEventArgs e)
		{
			var panel = sender as Panel;

			if (panel != null)
			{
				foreach (var child in panel.Children)
				{
					var fe = child as FrameworkElement;

					if (fe != null)
						fe.Margin = GetMargin(panel);
				}
			}
		}
	}
}