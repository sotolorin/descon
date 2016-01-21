using System;
using System.Text;
using System.Threading;
using Descon.Data;

namespace Descon.UI.DataAccess
{
	public static class UnityInteraction
	{
		public static void WaitForConnectionSend(IAsyncResult iar)
		{
			CommonDataStatic.Server.EndWaitForConnection(iar);
		}

		/// <summary>
		/// Saves the current drawing data if necessary and notifies Unity of an update
		/// </summary>
		public static void SendDataToUnity(string command)
		{
			bool processWasSuspended = false;

			if (CommonDataStatic.IsProcessSuspended)
			{
				processWasSuspended = true;
				CommonDataStatic.UnityProcess.Resume();
			}

			if (CommonDataStatic.Server != null && CommonDataStatic.Server.IsConnected)
			{
				var save = new SaveDataToXML();

				save.SavePreferences();

				if (command == ConstString.UNITY_UPDATE || command == ConstString.UNITY_CREATE_IMAGE)
					save.SaveDesconDrawing(ConstString.FILE_UNITY_DRAWING, false);

				CommonDataStatic.Server.Write(Encoding.ASCII.GetBytes(command), 0, command.Length);
				CommonDataStatic.Server.WaitForPipeDrain();
			}

			if (processWasSuspended)
				CommonDataStatic.UnityProcess.Suspend();
		}

        public static void SendDataToUnityTesting(string command)
        {
            var save = new SaveDataToXML();
            ConstString.FOLDER_LOCATION = ConstString.FOLDER_LOCATION + "\\Preferences.xml";
            save.TestSavePreferences();
            ConstString.FOLDER_LOCATION = ConstString.FOLDER_LOCATION.Replace("Preferences.xml", "Drawing.dsn");

            if (command == ConstString.UNITY_UPDATE || command == ConstString.UNITY_CREATE_IMAGE)
            {
                save.SaveDesconDrawing(ConstString.FOLDER_LOCATION, false);
            }
        }

		public static void RequestDrawing()
		{
			int waitCounter = 0;
			bool processWasSuspended = false;

			CommonDataStatic.UnityDoneCreatingImage = false;

			if (CommonDataStatic.IsProcessSuspended)
			{
				processWasSuspended = true;
				CommonDataStatic.UnityProcess.Resume();
			}

			SendDataToUnity(ConstString.UNITY_CREATE_IMAGE);

			while (!CommonDataStatic.UnityDoneCreatingImage)
			{
				Thread.Sleep(10);
				if (waitCounter++ == 300) // Give up after 3 seconds
					return;
			}

			if (processWasSuspended)
				CommonDataStatic.UnityProcess.Suspend();

			CommonDataStatic.UnityDoneCreatingImage = false;
		}
	}
}