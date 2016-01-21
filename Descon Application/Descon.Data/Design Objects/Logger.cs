using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Descon.Data
{
	/// <summary>
	/// Logger methods to keep track of user data and how they use the application
	/// </summary>
	[Serializable]
	public class Logger
	{
		#region Values to save in file

		public Logger()
		{
			MinutesPerSession = new List<int>();
		}

		/// <summary>
		/// This needs to be called before the Log data is saved. This adds the new values to the file according to how the user
		/// used the application in the latest session.
		/// </summary>
		public void SetupDataForSaving()
		{
			MinutesPerSession.Add((int) (DateTime.Now - TimeLaunched).TotalMinutes);
			AverageMinutes = (int)MinutesPerSession.Average();
		}

		public List<int> MinutesPerSession;
		public int AverageMinutes;

		#endregion

		#region Placeholders and other properties to make things simpler

		[XmlIgnore] public DateTime TimeLaunched;

		#endregion
	}
}