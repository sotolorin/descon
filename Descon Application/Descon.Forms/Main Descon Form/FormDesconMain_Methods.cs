using System;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Descon.Data;
using Descon.UI.DataAccess;
using MahApps.Metro;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Descon.Forms
{
	/// <summary>
	/// This partial class is intended to hold various methods used on the main form
	/// </summary>
	public partial class FormDesconMain
	{
		/// <summary>
		/// Alias so ControlDetailData can tie to this method as an event.
		/// </summary>
		private void ApplyChangesToDrawing(object sender, RoutedEventArgs e)
		{
			if (!_dontApplyChanges)
			{
				miscFormMethods.ApplyChangesToDrawing(_data);

				// Sets up the gauges and adds them to side panel
				wrapPanelGauges.Children.Clear();

				for (int i = 0; i < _data.GaugeData.Count; i++)
				{
					var gaugeData = _data.GaugeData[i];
					var gauge = new GaugePlotter();
					gauge.Width = 70;
					gauge.Height = 70;
					gauge.GaugeCapacityText = gaugeData.CapacityDescription;
					gauge.GaugeCapacity = gaugeData.CapacityValue;
					gauge.MouseUp += gauge_MouseUp;

					wrapPanelGauges.Children.Insert(i, gauge);
				}
			}
		}

		private string gaugeIndex;

		private void gauge_MouseUp(object sender, MouseButtonEventArgs e)
		{
			// This line must be at the beginning because the gauges are removed if the report wasn't open
			int gaugeNumber = wrapPanelGauges.Children.IndexOf(sender as UIElement);

			if (gaugeNumber < 0 || CommonDataStatic.LicenseType == ELicenseType.Demo_1)
				return;

			gaugeIndex = CommonDataStatic.ReportCapacityList[gaugeNumber].PadLeft(5, '0');

			// If the report isn't already open, we need to open it and wait for the document to refresh before navigating to the block
			if (!CommonDataStatic.IsReportOpen)
			{
				btnReport_Click(sender, e);

				controlReport.webBrowser.DocumentCompleted += webBrowser_DocumentCompleted;
			}
			else
				controlReport.FindBlockInReport(gaugeIndex);
		}

		private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			controlReport.FindBlockInReport(gaugeIndex);
			controlReport.webBrowser.DocumentCompleted -= webBrowser_DocumentCompleted;
		}

		/// <summary>
		/// Sets a few properties related to the theme that must be changed manually
		/// </summary>
		private void ThemeManager_IsThemeChanged(object sender, EventArgs e)
		{
			var theme = ThemeManager.DetectAppStyle(Application.Current);
			_data.Preferences.ApplicationThemeName = theme.Item2.Name;
			_data.ThemeAccent = _data.AccentColors.First(a => a.Name == theme.Item2.Name).ColorBrush;
		}

		#region File Manipulation

		private void ResizeGraphicsPanel()
		{
			if (CommonDataStatic.UnityProcess == null)
				return;

			const int SWP_ASYNCWINDOWPOS = 0x4000; // Helps the Descon window resize smoothly without being interrupted

			SetWindowPos(CommonDataStatic.UnityProcess.MainWindowHandle, IntPtr.Zero, 0, 0, (int)windowsFormHostUnity.ActualWidth, (int)windowsFormHostUnity.ActualHeight, SWP_ASYNCWINDOWPOS);
		}

		/// <summary>
		/// Waits for the click commands from Unity. Strings sent from Unity include which component was clicked.
		/// </summary>
		private void WaitForConnectionReceive(IAsyncResult iar)
		{
			var receivePipe = (NamedPipeServerStream)iar.AsyncState;

			try
			{
				const int BUFFSIZE = 255;
				byte[] buffer = new byte[BUFFSIZE];

				receivePipe.EndWaitForConnection(iar);

				int count = receivePipe.Read(buffer, 0, BUFFSIZE);

				if (count > 0 && count <= BUFFSIZE)
				{
					// Format is: EMemberType|EMemberSubType|EClickType
					var receivedString = Encoding.ASCII.GetString(buffer, 0, count).Split('|');

					if (receivedString.Count() == 1 && receivedString[0] == ConstString.UNITY_UPDATE_DONE)
					{
						CommonDataStatic.UnityDoneUpdating = true;
						return;
					}

					if (receivedString.Count() == 1 && receivedString[0] == ConstString.UNITY_DONE_SAVING)
					{
						CommonDataStatic.UnityDoneSaving = true;
						return;
					}

					if (receivedString.Count() == 1 && receivedString[0] == ConstString.UNITY_CREATE_IMAGE_DONE)
					{
						CommonDataStatic.UnityDoneCreatingImage = true;
						return;
					}

					var commonList = new CommonLists();

					var memberType = commonList.CompleteMemberList.First(c => c.Value == receivedString[0]).Key;
					var subType = commonList.MemberSubType.First(c => c.Value == receivedString[1]).Key;
					var clickType = commonList.ClickType.First(c => c.Value == receivedString[2]).Key;

					_data.MemberType = memberType;

					// Application Dispatcher is necessary to avoid opening the form up on a new thread which doesn't work
					switch (clickType)
					{
						case EClickType.Double:
							switch (subType)
							{
								case EMemberSubType.Main:
									if (memberType != EMemberType.PrimaryMember)
									{
										Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
											new Action(() => controlDetailData.OpenConnectionForm(EMemberSubType.Beam)));
									}
									break;
								case EMemberSubType.BoltShearBeam:
								case EMemberSubType.BoltShearSupport:
								case EMemberSubType.BoltMomentBeam:
								case EMemberSubType.BoltMomentSupport:
									Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
										new Action(() => controlDetailData.OpenCurrentBoltForm(subType)));
									break;
								case EMemberSubType.Shear:
									Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
										new Action(() => controlDetailData.OpenConnectionForm(EMemberSubType.Shear)));
									break;
								case EMemberSubType.Moment:
									Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
										new Action(() => controlDetailData.OpenConnectionForm(EMemberSubType.Moment)));
									break;
								case EMemberSubType.Stiffener:
									Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
										new Action(() => controlDetailData.OpenConnectionForm(EMemberSubType.Stiffener)));
									break;
							}
							break;
						case EClickType.Right:
							break;
					}
				}
			}
			catch
			{
				// Do nothing
			}
			finally
			{
				// This resets the methods and waits for the next string sent from Unity
				receivePipe.Close();
				receivePipe = null;
				receivePipe = new NamedPipeServerStream(ConstString.UNITY_PIPE_NAME_RECEIVE, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
				receivePipe.BeginWaitForConnection(WaitForConnectionReceive, receivePipe);
			}
		}

		#endregion

		private void SavePreferencesAndTellUnity()
		{
			_saveDataToXml.SavePreferences();
			UnityInteraction.SendDataToUnity(ConstString.UNITY_PREFERENCES_UPDATE);
		}

		private void DesconMainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			ResizeGraphicsPanel();
		}

		private EJointConfiguration _tempConfiguration;

		private void ComboBox_DropDownOpened(object sender, EventArgs e)
		{
			_tempConfiguration = CommonDataStatic.JointConfig;
		}

		private void ComboBox_DropDownClosed(object sender, EventArgs e)
		{
			if (_tempConfiguration != CommonDataStatic.JointConfig)
			{
				if (!NewFile())
					_data.JointConfig = _tempConfiguration;
			}
		}

		private void btnDrawing_Click(object sender, RoutedEventArgs e)
		{
			CommonDataStatic.UnityProcess.Resume();

			if (!CommonDataStatic.IsReportOpen) // Already open so ignore the click
			{
				btnReport_Click(null, new RoutedEventArgs());
				return;
			}

			stackPanelDrawingTab.Margin = new Thickness(0, 7, 1, 0);
			stackPanelReportTab.Margin = new Thickness(0, 7, 1, 1);
			stackPanelDesignButtons.Visibility = Visibility.Visible;
			stackPanelReportButtons.Visibility = Visibility.Collapsed;

			CommonDataStatic.IsReportOpen = false;
			tabItemDrawing.IsSelected = true;
		}

		private void btnReport_Click(object sender, RoutedEventArgs e)
		{
			if (CommonDataStatic.IsReportOpen) // Already open so ignore the click
			{
				btnDrawing_Click(null, new RoutedEventArgs());
				return;
			}

			stackPanelDrawingTab.Margin = new Thickness(0, 7, 1, 1);
			stackPanelReportTab.Margin = new Thickness(0, 7, 1, 0);
			stackPanelDesignButtons.Visibility = Visibility.Collapsed;
			stackPanelReportButtons.Visibility = Visibility.Visible;

			CommonDataStatic.IsReportOpen = true;
			tabItemReport.IsSelected = true;

			ApplyChangesToDrawing(null, new RoutedEventArgs());

			CommonDataStatic.UnityProcess.Suspend();
		}

		private void btnGauges_Click(object sender, RoutedEventArgs e)
		{
			if (wrapPanelGauges.Visibility == Visibility.Visible)
			{
				wrapPanelGauges.Visibility = Visibility.Collapsed;
				btnArrowGauges.Source = new BitmapImage(new Uri("pack://application:,,,/Descon.Resources;component/Images/Icons/Descon_UI_Icons_MainTab_ArrowLeft.png", UriKind.Absolute));
			}
			else
			{
				wrapPanelGauges.Visibility = Visibility.Visible;
				btnArrowGauges.Source = new BitmapImage(new Uri("pack://application:,,,/Descon.Resources;component/Images/Icons/Descon_UI_Icons_MainTab_ArrowRight.png", UriKind.Absolute));
			}
		}

		private void menuExit_Click(object sender, RoutedEventArgs e)
		{
			new SaveDataToXML().SavePreferences();
			MessageBoxResult result;

			result = MessageBox.Show("Save current file?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
			if (result == MessageBoxResult.Yes)
			{
				if (new MiscFormMethods().SaveFile(_data))
					Close();
			}
			else if (result == MessageBoxResult.No)
				Close();
		}
	}
}