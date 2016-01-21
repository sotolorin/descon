using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace Descon.Data
{
	/// <summary>
	/// Useful and simple methods used throughout the application
	/// </summary>
	public static class MiscMethods
	{
		// Used to get the path string from a registry entry
		[DllImport("shell32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SHGetPathFromIDListW(IntPtr pidl, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszPath);

		/// <summary>
		/// Returng the version number from the assembly itself and pads out single digits at the end
		/// </summary>
		/// <returns></returns>
		public static string GetVersionString()
		{
			try
			{
				string version = FileVersionInfo.GetVersionInfo(AppDomain.CurrentDomain.BaseDirectory + ConstString.DESCON_EXE).FileVersion;
				var versionParts = version.Split('.');

				version = versionParts[0] + '.' + versionParts[1] + '.' + versionParts[2] + '.' + versionParts[3].PadLeft(2, '0');

				return version;
			}
			catch
			{
				return String.Empty;
			}
		}

		public static string GetShortComponentName(EMemberType memberType)
		{
			switch (memberType)
			{
				case EMemberType.PrimaryMember:
					return "|b|c";
				case EMemberType.LeftBeam:
					return "|b|l";
				case EMemberType.RightBeam:
					return "|b|r";
				case EMemberType.LowerLeft:
					return "|b|ll";
				case EMemberType.LowerRight:
					return "|b|lr";
				case EMemberType.UpperLeft:
					return "|b|ul";
				case EMemberType.UpperRight:
					return "|b|ur";
				default:
					return string.Empty;
			}
		}

		public static void ResetReportLists()
		{
			CommonDataStatic.ReportBookmarkList = new List<string>();
			CommonDataStatic.ReportHighlightList = new List<string>();
			CommonDataStatic.ReportCommentList = new Dictionary<string, string>();
			CommonDataStatic.ReportNoGoodList = new List<string>();
			CommonDataStatic.ReportCapacityList = new List<string>();
			CommonDataStatic.ReportGoToList = new Dictionary<string, string>();
		}

		/// <summary>
		/// Sets the default UI data
		/// </summary>
		public static void SetDefaultData()
		{
			CommonDataStatic.DetailDataDict = new Dictionary<EMemberType, DetailData>
			{
				{EMemberType.PrimaryMember, new DetailData()},
				{EMemberType.UpperLeft, new DetailData()},
				{EMemberType.UpperRight, new DetailData()},
				{EMemberType.LowerLeft, new DetailData()},
				{EMemberType.LowerRight, new DetailData()},
				{EMemberType.LeftBeam, new DetailData()},
				{EMemberType.RightBeam, new DetailData()}
			};

			CommonDataStatic.ColumnSplice = new ColumnSplice();
			CommonDataStatic.ColumnStiffener = new ColumnStiffener();
			CommonDataStatic.SeismicSettings = new SeismicSettings();

			CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember] = new DetailData
			{
				Shape = new Shape(),
				MemberType = EMemberType.PrimaryMember,
				ShapeType = EShapeType.WideFlange
			};
			CommonDataStatic.DetailDataDict[EMemberType.UpperRight] = new DetailData
			{
				Shape = new Shape(),
				GussetToColumnConnection = EBraceConnectionTypes.DirectlyWelded,
				GussetToBeamConnection = EBraceConnectionTypes.DirectlyWelded,
				MemberType = EMemberType.UpperRight,
				ShapeType = EShapeType.WideFlange
			};
			CommonDataStatic.DetailDataDict[EMemberType.UpperLeft] = new DetailData
			{
				Shape = new Shape(),
				GussetToColumnConnection = EBraceConnectionTypes.DirectlyWelded,
				GussetToBeamConnection = EBraceConnectionTypes.DirectlyWelded,
				MemberType = EMemberType.UpperLeft,
				ShapeType = EShapeType.WideFlange
			};
			CommonDataStatic.DetailDataDict[EMemberType.LowerRight] = new DetailData
			{
				Shape = new Shape(),
				GussetToColumnConnection = EBraceConnectionTypes.DirectlyWelded,
				GussetToBeamConnection = EBraceConnectionTypes.DirectlyWelded,
				MemberType = EMemberType.LowerRight,
				ShapeType = EShapeType.WideFlange
			};
			CommonDataStatic.DetailDataDict[EMemberType.LowerLeft] = new DetailData
			{
				Shape = new Shape(),
				GussetToColumnConnection = EBraceConnectionTypes.DirectlyWelded,
				GussetToBeamConnection = EBraceConnectionTypes.DirectlyWelded,
				MemberType = EMemberType.LowerLeft,
				ShapeType = EShapeType.WideFlange
			};
			CommonDataStatic.DetailDataDict[EMemberType.RightBeam] = new DetailData
			{
				Shape = new Shape(),
				GussetToBeamConnection = EBraceConnectionTypes.DirectlyWelded,
				MemberType = EMemberType.RightBeam,
				ShapeType = EShapeType.WideFlange
			};
			CommonDataStatic.DetailDataDict[EMemberType.LeftBeam] = new DetailData
			{
				Shape = new Shape(),
				GussetToBeamConnection = EBraceConnectionTypes.DirectlyWelded,
				MemberType = EMemberType.LeftBeam,
				ShapeType = EShapeType.WideFlange
			};
		}

		internal static void SetDefaultWelds()
		{
			if (CommonDataStatic.DetailDataDict == null)
				return;
			foreach (var detailData in CommonDataStatic.DetailDataDict.Values)
			{
				detailData.WinConnect.ShearClipAngle.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
				detailData.WinConnect.ShearEndPlate.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
				detailData.WinConnect.ShearSeat.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
				detailData.WinConnect.ShearWebPlate.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
				detailData.WinConnect.ShearWebTee.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
				detailData.WinConnect.MomentDirectWeld.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
				detailData.WinConnect.MomentEndPlate.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
				detailData.WinConnect.MomentFlangeAngle.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
				detailData.WinConnect.MomentFlangePlate.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
				detailData.WinConnect.MomentTee.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();
			}
		}

		/// <summary>
		/// Sets the Bolt Hole Type radio buttons to be either disabled or enabled depending on various conditions.
		/// Must call for both right and left beams.
		/// </summary>
		public static void SetHoleTypesEnabledOrDisabled(DetailData detailData)
		{
			var shearCarriedBy = detailData.ShearConnection;
			bool hasAxialLoad = detailData.AxialCompression > 0;
			Bolt bolt;
			Bolt boltSupport;
			Bolt boltBeam;

			switch (shearCarriedBy)
			{
				case EShearCarriedBy.SinglePlate:
					bolt = detailData.WinConnect.ShearWebPlate.Bolt;
					// OVS, SSLP, LSLP
					if (bolt.BoltType != EBoltType.SC)
						bolt.isOVSEnabled = bolt.isSSLPEnabled = bolt.isLSLPEnabled = false;
					else
						bolt.isOVSEnabled = bolt.isSSLPEnabled = bolt.isLSLPEnabled = true;
					// SSLN, LSLN
					if (hasAxialLoad && bolt.BoltType != EBoltType.SC)
						bolt.isSSLNEnabled = bolt.isLSLNEnabled = false;
					else
						bolt.isSSLNEnabled = bolt.isLSLNEnabled = true;
					// Special case for Single Plate that requirs LSLN to be disabled
					if (bolt.disableLSLNWithBoltGroupEccentricity)
						bolt.isLSLNEnabled = false;
					break;
				case EShearCarriedBy.EndPlate:
					bolt = detailData.WinConnect.ShearEndPlate.Bolt;
					// OVS, SSLP, LSLP
					if (bolt.BoltType != EBoltType.SC)
						bolt.isOVSEnabled = bolt.isSSLPEnabled = bolt.isLSLPEnabled = false;
					else
						bolt.isOVSEnabled = bolt.isSSLPEnabled = bolt.isLSLPEnabled = true;
					// SSLN, LSLN
					bolt.isSSLNEnabled = bolt.isLSLNEnabled = true;
					break;
				case EShearCarriedBy.ClipAngle:
				case EShearCarriedBy.Tee:
					if (shearCarriedBy == EShearCarriedBy.ClipAngle)
					{
						boltSupport = detailData.WinConnect.ShearClipAngle.BoltOslOnSupport;
						boltBeam = detailData.WinConnect.ShearClipAngle.BoltWebOnBeam;
					}
					else
					{
						boltSupport = detailData.WinConnect.ShearWebTee.BoltOslOnFlange;
						boltBeam = detailData.WinConnect.ShearWebTee.BoltWebOnStem;				
					}

					// OVS, SSLP, LSLP
					if (boltSupport.BoltType != EBoltType.SC)
						boltSupport.isOVSEnabled = boltSupport.isSSLPEnabled = boltSupport.isLSLPEnabled = false;
					else
						boltSupport.isOVSEnabled = boltSupport.isSSLPEnabled = boltSupport.isLSLPEnabled = true;
					// SSLN, LSLN
					boltSupport.isSSLNEnabled = boltSupport.isLSLNEnabled = true;

					// OVS, SSLP, LSLP
					if (boltBeam.BoltType != EBoltType.SC)
						boltBeam.isOVSEnabled = boltBeam.isSSLPEnabled = boltBeam.isLSLPEnabled = false;
					else
						boltBeam.isOVSEnabled = boltBeam.isSSLPEnabled = boltBeam.isLSLPEnabled = true;
					// SSLN, LSLN
					if (hasAxialLoad && boltBeam.BoltType != EBoltType.SC)
						boltBeam.isSSLNEnabled = boltBeam.isLSLNEnabled = false;
					else
						boltBeam.isSSLNEnabled = boltBeam.isLSLNEnabled = true;
					break;
			}
		}

		public static byte[] GetHashOfFile(string path)
		{
			using (var md5 = MD5.Create())
			{
				using (var stream = File.OpenRead(path))
				{
					return md5.ComputeHash(stream);
				}
			}
		}

		public static string ConvertHTMLReponseToString(byte[] downloadedString)
		{
			string result;

			result = Encoding.ASCII.GetString(downloadedString);
			result = result.Replace("\\", string.Empty).Replace("\"", string.Empty);

			return result;
		}

		public static string ConvertHTMLReponseToString(string downloadedString)
		{
			downloadedString = downloadedString.Replace("\\", string.Empty).Replace("\"", string.Empty);

			return downloadedString;
		}

		/// <summary>
		/// Converts string received from database
		/// </summary>
		public static ELicenseType ConvertFromStringToLicenseType(string licenseType)
		{
			switch (licenseType)
			{
				case "FREE":
					return ELicenseType.Open_2;
				case "BASIC":
					return ELicenseType.Basic_3;
				case "STANDARD":
					return ELicenseType.Standard_4;
				case "NEXT":
					return ELicenseType.Next_5;
				default:
					return ELicenseType.NoMatch;
			}
		}

		public static void DeactivateBraces()
		{
			CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].Shape = new Shape();
			CommonDataStatic.DetailDataDict[EMemberType.LowerRight].Shape = new Shape();
			CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].Shape = new Shape();
			CommonDataStatic.DetailDataDict[EMemberType.UpperRight].Shape = new Shape();
		}

		public static Dictionary<string, string> GetRecentlyOpenedFileList()
		{
			var recentFileList = new Dictionary<string, string>();

			try
			{
				if (CommonDataStatic.Preferences.NumberOfRecentlyOpenFiles == 0)
					recentFileList.Add("File history disabled in Settings", string.Empty);
				else
				{
					recentFileList.Add("RECENT FILES", string.Empty);
					using (var regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\ComDlg32\\OpenSavePidlMRU"))
					{
						if (regKey != null)
						{
							RegistryKey myKey = regKey.OpenSubKey("dsn");

							if (myKey != null)
							{
								byte[] recentFilesCompleteIndexList = (byte[])myKey.GetValue("MRUListEx");
								var recentFiles = new List<string>();

								if (recentFilesCompleteIndexList != null)
								{
									for (int i = 0; i < recentFilesCompleteIndexList.Length; i += 4)
									{
										if (i % 4 == 0)
											recentFiles.Add(recentFilesCompleteIndexList[i].ToString());
									}

									for (int i = 0; recentFileList.Count <= CommonDataStatic.Preferences.NumberOfRecentlyOpenFiles && i < recentFiles.Count(); i++)
									{
										string longPath = GetPathFromPIDL((byte[])myKey.GetValue(recentFiles[i]));
										string[] pathArray = longPath.Split("\\".ToCharArray());
										string shortPath;
										if (pathArray.Count() <= 3)
											shortPath = longPath;
										else
											shortPath = pathArray[0] + "\\...\\" + pathArray[pathArray.Length - 2] + "\\" + pathArray[pathArray.Length - 1];

										if (!recentFileList.ContainsKey(shortPath) && File.Exists(longPath))
											recentFileList.Add(shortPath, longPath);
									}
								}
							}
						}
					}
				}

				if (recentFileList.Count == 1)
				{
					recentFileList.Clear();
					recentFileList.Add("No recent files available.", string.Empty);
				}
			}
			catch (Exception e)
			{
				// Something failed retrieving the file names, so just continue on. Uncomment Exception to see what it is.
			}

			return recentFileList;
		}

		private static string GetPathFromPIDL(byte[] byteCode)
		{
			StringBuilder builder = new StringBuilder(500);

			IntPtr ptr;
			GCHandle h0 = GCHandle.Alloc(byteCode, GCHandleType.Pinned);
			try
			{
				ptr = h0.AddrOfPinnedObject();
			}
			finally
			{
				h0.Free();
			}

			SHGetPathFromIDListW(ptr, builder);

			return builder.ToString();
		}

		#region Shortcuts to check for specific conditions

		public static bool IsFlareWeld(EMemberType memberType)
		{
			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
				return true;
			else if (CommonDataStatic.DetailDataDict[memberType].ShearConnection == EShearCarriedBy.ClipAngle &&
				CommonDataStatic.DetailDataDict[memberType].WinConnect.ShearClipAngle.LengthOfOSL + CommonDataStatic.DetailDataDict[memberType].Shape.tw / 2 >=
				CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.bf / 2)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Returns true if any brace is active
		/// </summary>
		public static bool IsAnyBraceActive()
		{
			if (CommonDataStatic.DetailDataDict == null)
				return false;

			if (CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].IsActive ||
			    CommonDataStatic.DetailDataDict[EMemberType.UpperRight].IsActive ||
			    CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].IsActive ||
			    CommonDataStatic.DetailDataDict[EMemberType.LowerRight].IsActive)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Returns true if the component type is column, beam, right beam or left beam and false otherwise.
		/// Index m &lt;= 2 would return true, m > 2 would return false
		/// </summary>
		public static bool IsBeamOrColumn(EMemberType memberType)
		{
			if (memberType == EMemberType.PrimaryMember || 
				memberType == EMemberType.LeftBeam || memberType == EMemberType.RightBeam)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Returns true if the component type is right beam or left beam
		/// </summary>
		public static bool IsBeam(EMemberType memberType)
		{
			if (memberType == EMemberType.LeftBeam || memberType == EMemberType.RightBeam)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Returns true if the component type is a Upper, Lower, Right, or Left brace
		/// </summary>
		public static bool IsBrace(EMemberType memberType)
		{
			if (memberType == EMemberType.UpperLeft || memberType == EMemberType.UpperRight ||
				memberType == EMemberType.LowerLeft || memberType == EMemberType.LowerRight)
				return true;
			else
				return false;
		}

		/// <summary>
		///  Returns the name of the component as a string
		/// </summary>
		public static string GetComponentName(EMemberType memberType)
		{
			return CommonDataStatic.CommonLists.CompleteMemberList[memberType];
		}

		/// <summary>
		/// Returns the equivalent of m - 1 which is the component one index lower. Some of the return values may never be used,
		/// but they are all here.
		/// </summary>
		public static EMemberType ComponentMinusOne(EMemberType memberType)
		{
			switch (memberType)
			{
				case EMemberType.RightBeam:
					return EMemberType.PrimaryMember;
				case EMemberType.LeftBeam:
					return EMemberType.RightBeam;
				case EMemberType.UpperRight:
					return EMemberType.LeftBeam;
				case EMemberType.LowerRight:
					return EMemberType.UpperRight;
				case EMemberType.UpperLeft:
					return EMemberType.LowerRight;
				default: //EMemberType.LowerLeft:
					return EMemberType.UpperLeft;
			}
		}

		/// <summary>
		/// Returns the correct beam depending on which Brace we are working with (right or left)
		/// </summary>
		public static EMemberType BeamComponentFromBrace(EMemberType memberType)
		{
			if (memberType == EMemberType.LowerRight || memberType == EMemberType.UpperRight)
				return EMemberType.RightBeam;
			else
				return EMemberType.LeftBeam;
		}

		#endregion
	}
}