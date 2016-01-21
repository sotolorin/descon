using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Descon.Data;
using MahApps.Metro;

namespace Descon.UI.DataAccess
{
	/// <summary>
	/// Changes the theme according to the current selected color
	/// </summary>
	public class AccentColorMenuData
	{
		public string Name { get; set; }
		public SolidColorBrush ColorBrush { get; set; }

		private ICommand changeAccentCommand;
		public ICommand ChangeAccentCommand
		{
			get
			{
				return changeAccentCommand ??
					   (changeAccentCommand = new SimpleCommand { CanExecuteDelegate = x => true, ExecuteDelegate = x => ChangeAccent() });
			}
		}

		private void ChangeAccent()
		{
			var theme = ThemeManager.DetectAppStyle(Application.Current);
			var accent = ThemeManager.Accents.First(x => x.Name == Name);
			ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
		}
	}

	public class SimpleCommand : ICommand
	{
		public Predicate<object> CanExecuteDelegate { get; set; }
		public Action<object> ExecuteDelegate { get; set; }

		public bool CanExecute(object parameter)
		{
			if (CanExecuteDelegate != null)
				return CanExecuteDelegate(parameter);
			return true;
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object parameter)
		{
			if (ExecuteDelegate != null)
				ExecuteDelegate(parameter);
		}
	}

	// The following class and flags are used to handle the Unity process and suspend it when needed.
	[Flags]
	public enum ThreadAccess
	{
		//TERMINATE = (0x0001),
		SUSPEND_RESUME = (0x0002),
		//GET_CONTEXT = (0x0008),
		//SET_CONTEXT = (0x0010),
		//SET_INFORMATION = (0x0020),
		//QUERY_INFORMATION = (0x0040),
		//SET_THREAD_TOKEN = (0x0080),
		//IMPERSONATE = (0x0100),
		//DIRECT_IMPERSONATION = (0x0200)
	}

	public static class ProcessExtension
	{
		[DllImport("kernel32.dll")]
		static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
		[DllImport("kernel32.dll")]
		static extern uint SuspendThread(IntPtr hThread);
		[DllImport("kernel32.dll")]
		static extern int ResumeThread(IntPtr hThread);

		public static void Suspend(this Process process)
		{
			CommonDataStatic.IsProcessSuspended = true;

			foreach (ProcessThread thread in process.Threads)
			{
				var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
				if (pOpenThread == IntPtr.Zero)
				{
					break;
				}
				SuspendThread(pOpenThread);
			}
		}
		public static void Resume(this Process process)
		{
			CommonDataStatic.IsProcessSuspended = false;

			foreach (ProcessThread thread in process.Threads)
			{
				var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
				if (pOpenThread == IntPtr.Zero)
				{
					break;
				}
				ResumeThread(pOpenThread);
			}
		}
	}
}