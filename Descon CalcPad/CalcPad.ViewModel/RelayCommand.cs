using System;
using System.Windows.Input;

namespace CalcPad.ViewModel
{
	/// <summary>
	/// Helps pass a control CommandParameter through to the action
	/// </summary>
	public class RelayCommand : ICommand
	{
		private readonly Predicate<object> canExecute;
		private readonly Action<object> execute;

		#region Constructors and Destructors

		public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			this.execute = execute;
			this.canExecute = canExecute;
		}

		#endregion

		#region Events

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }

			remove { CommandManager.RequerySuggested -= value; }
		}

		#endregion

		#region Implemented Interfaces


		public bool CanExecute(object parameter)
		{
			return canExecute == null || canExecute(parameter);
		}

		public void Execute(object parameter)
		{
			execute(parameter);
		}

		#endregion
	}
}