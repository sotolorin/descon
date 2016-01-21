using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Descon.Forms;
using Microsoft.Shell;

namespace Descon.Main
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : ISingleInstanceApp
	{
		private const string _unique = "Descon Main App";

		private void Application_Startup(object sender, StartupEventArgs args)
		{
			string _filePath = string.Empty;

			if (args.Args.Length > 0)
				_filePath = args.Args[0];

			// If Descon is already open we kill the extra instance
			if (SingleInstance<App>.InitializeAsFirstInstance(_unique))
				new FormStartup(_filePath);
			else
				Current.Shutdown();
		}

		#region ISingleInstanceApp Members

		/// <summary>
		/// This method is triggered when Descon is already open and the user tries to open it again or double clicks a save file.
		/// </summary>
		[STAThread]
		public bool SignalExternalCommandLineArgs(IList<string> args)
		{
			if (MainWindow.WindowState == WindowState.Minimized)
				MainWindow.WindowState = WindowState.Normal;
			// The first arg [0] is the application path and the second [1] is the file path that was double clicked
			if (args.Count > 1)
				((FormDesconMain)MainWindow).OpenFileFromArgs(args);

			MainWindow.Activate();

			return true;
		}

		#endregion

		// These methods override some default WPF behavior to allow for some special functionality. Related to selecting the text
		// in a TextBox automatically.
		protected override void OnStartup(StartupEventArgs e)
		{
			// Selects all text in a text box when it receives focus
			EventManager.RegisterClassHandler(typeof(TextBox), UIElement.PreviewMouseLeftButtonDownEvent,
				new MouseButtonEventHandler(SelectivelyIgnoreMouseButton));
			//EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotKeyboardFocusEvent,
			//	new RoutedEventHandler(SelectAllText));
			EventManager.RegisterClassHandler(typeof (TextBox), Control.MouseDoubleClickEvent,
				new RoutedEventHandler(SelectAllText));
			base.OnStartup(e);
		}

		private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
		{
			// Verifies the current object is a TextBox
			DependencyObject parent = e.OriginalSource as UIElement;
			while (parent != null && !(parent is TextBox))
				parent = VisualTreeHelper.GetParent(parent);

			if (parent != null)
			{
				var textBox = (TextBox) parent;
				if (!textBox.IsKeyboardFocusWithin)
				{
					// If the text box is not yet focused, give it the focus and stop further processing of this click event.
					textBox.Focus();
					e.Handled = true;
				}
			}
		}

		private void SelectAllText(object sender, RoutedEventArgs e)
		{
			var textBox = e.OriginalSource as TextBox;
			if (textBox != null)
				textBox.SelectAll();
		}
	}
}