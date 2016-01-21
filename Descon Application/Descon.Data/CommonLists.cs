using System.Collections;
using System.Collections.Generic;

namespace Descon.Data
{
	/// <summary>
	/// Lists used throughout the application for things like drop down combo boxes and other data selections. Most
	/// are dictionaries to make it easier to keep track of the current selection using an enum, while also having a
	/// string to display.
	/// </summary>
	public sealed class CommonLists
	{
		/// <summary>
		/// Available components based on the current Joint Configuration and displayed when selected
		/// </summary>
		public SortedList MemberList
		{
			get
			{
				var components = new SortedList();

				switch (CommonDataStatic.JointConfig)
				{
					case EJointConfiguration.BraceToColumn:
						components = new SortedList
						{
							{EMemberType.PrimaryMember, ConstString.MEMBER_COLUMN_OR_BEAM},
							{EMemberType.RightBeam, ConstString.MEMBER_RIGHT_SIDE_BEAM},
							{EMemberType.LeftBeam, ConstString.MEMBER_LEFT_SIDE_BEAM}
						};
						// Standard License can only have braces if the column is not HSS
						if ((CommonDataStatic.LicenseMinimumStandard && CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].ShapeType != EShapeType.HollowSteelSection) ||
						    CommonDataStatic.LicenseMinimumNext)
						{
							components.Add(EMemberType.UpperRight, ConstString.MEMBER_UPPER_RIGHT_BRACE);
							components.Add(EMemberType.LowerRight, ConstString.MEMBER_LOWER_RIGHT_BRACE);
							components.Add(EMemberType.UpperLeft, ConstString.MEMBER_UPPER_LEFT_BRACE);
							components.Add(EMemberType.LowerLeft, ConstString.MEMBER_LOWER_LEFT_BRACE);
						}
						break;
					case EJointConfiguration.BraceVToBeam:
						components = new SortedList
						{
							{EMemberType.PrimaryMember, ConstString.MEMBER_COLUMN_OR_BEAM},
							{EMemberType.UpperRight, ConstString.MEMBER_UPPER_RIGHT_BRACE},
							{EMemberType.LowerRight, ConstString.MEMBER_LOWER_RIGHT_BRACE},
							{EMemberType.UpperLeft, ConstString.MEMBER_UPPER_LEFT_BRACE},
							{EMemberType.LowerLeft, ConstString.MEMBER_LOWER_LEFT_BRACE}
						};
						break;
					case EJointConfiguration.BraceToColumnBase:
						components = new SortedList
						{
							{EMemberType.PrimaryMember, ConstString.MEMBER_COLUMN_OR_BEAM},
							{EMemberType.UpperRight, ConstString.MEMBER_UPPER_RIGHT_BRACE},
							{EMemberType.UpperLeft, ConstString.MEMBER_UPPER_LEFT_BRACE},
						};
						break;
					case EJointConfiguration.ColumnSplice:
						components = new SortedList
						{
							{EMemberType.LeftBeam, ConstString.MEMBER_LEFT_SIDE_BEAM},
							{EMemberType.RightBeam, ConstString.MEMBER_RIGHT_SIDE_BEAM}
						};
						break;
					case EJointConfiguration.BeamToGirder:
						components = new SortedList
						{
							{EMemberType.PrimaryMember, ConstString.MEMBER_GIRDER},
							{EMemberType.LeftBeam, ConstString.MEMBER_LEFT_SIDE_BEAM},
							{EMemberType.RightBeam, ConstString.MEMBER_RIGHT_SIDE_BEAM},
						};
						break;
					case EJointConfiguration.BeamSplice:
						components = new SortedList
						{
							{EMemberType.LeftBeam, ConstString.MEMBER_LEFT_SIDE_BEAM},
							{EMemberType.RightBeam, ConstString.MEMBER_RIGHT_SIDE_BEAM},
						};
						break;
				}

				return components;
			}
		}

		public SortedList MemberListFormControlShell
		{
			get
			{
				var memberList = new SortedList();
				if (CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive)
					memberList.Add(EMemberType.RightBeam, ConstString.MEMBER_RIGHT_SIDE_BEAM);
				if (CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive)
					memberList.Add(EMemberType.LeftBeam, ConstString.MEMBER_LEFT_SIDE_BEAM);

				return memberList;
			}
		}

		#region Lists used for combo boxes on main form

		/// <summary>
		/// Available Joint Configurations based on the current license available in the Joint Configuration menu
		/// </summary>
		public Dictionary<EJointConfiguration, string> JointConfigListForMenu
		{
			get
			{
				var jointConfigs = new Dictionary<EJointConfiguration, string>();
				jointConfigs.Add(EJointConfiguration.BraceToColumn, ConstString.JOINT_CONFIG_BRACE_TO_COLUMN);

				if (CommonDataStatic.LicenseMinimumBasic)
				{
					jointConfigs.Add(EJointConfiguration.BeamToGirder, ConstString.JOINT_CONFIG_BEAM_TO_GIRDER);
					jointConfigs.Add(EJointConfiguration.BeamSplice, ConstString.JOINT_CONFIG_BEAM_SPLICE);
				}

				if (CommonDataStatic.LicenseMinimumStandard)
				{
					jointConfigs.Add(EJointConfiguration.ColumnSplice, ConstString.JOINT_CONFIG_COLUMN_SPLICE);
					jointConfigs.Add(EJointConfiguration.BraceVToBeam, ConstString.JOINT_CONFIG_V_BRACE_TO_BEAM);
					jointConfigs.Add(EJointConfiguration.BraceToColumnBase, ConstString.JOINT_CONFIG_BRACE_TO_COLUMN_BASE);
				}

				return jointConfigs;
			}
		}

		/// <summary>
		/// Shape types based on the current license and selected component
		/// </summary>
		public Dictionary<EShapeType, string> ShapeTypeList
		{
			get
			{
				var shapeTypes = new Dictionary<EShapeType, string>();
				shapeTypes.Add(EShapeType.WideFlange, "Wide Flange Section");

				switch (CommonDataStatic.SelectedMember.MemberType)
				{
					case EMemberType.PrimaryMember:
						if (CommonDataStatic.LicenseMinimumBasic && CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToGirder)
							shapeTypes.Add(EShapeType.HollowSteelSection, "Hollow Steel Section");
						break;
					case EMemberType.LowerLeft:
					case EMemberType.LowerRight:
					case EMemberType.UpperLeft:
					case EMemberType.UpperRight:
						if (CommonDataStatic.LicenseMinimumStandard)
						{
							shapeTypes.Add(EShapeType.HollowSteelSection, "Hollow Steel Section");
							shapeTypes.Add(EShapeType.WTSection, "WT Section");
							shapeTypes.Add(EShapeType.SingleAngle, "Single Angle");
							shapeTypes.Add(EShapeType.DoubleAngle, "Double Angle");
							shapeTypes.Add(EShapeType.SingleChannel, "Single Channel");
							shapeTypes.Add(EShapeType.DoubleChannel, "Double Channel");
						}
						break;
				}

				return shapeTypes;
			}
		}

		public Dictionary<EBraceConnectionTypes, string> GussetToBeamConnectionList
		{
			get
			{
				Dictionary<EBraceConnectionTypes, string> connectionList;

				connectionList = new Dictionary<EBraceConnectionTypes, string>();
				connectionList.Add(EBraceConnectionTypes.DirectlyWelded, ConstString.CON_DIRECTLY_WELDED);

				return connectionList;
			}
		}

		/// <summary>
		/// Used for the Beam to Gusset list.
		/// </summary>
		public Dictionary<EBraceConnectionTypes, string> GussetToColumnConnectionList
		{
			get
			{
				var connectionList = new Dictionary<EBraceConnectionTypes, string>();

				connectionList.Add(EBraceConnectionTypes.DirectlyWelded, ConstString.CON_DIRECTLY_WELDED);

				if (CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive || CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive)
				{
					if (CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].ShapeType == EShapeType.WideFlange)
						connectionList.Add(EBraceConnectionTypes.ClipAngle, ConstString.CON_CLIP_ANGLE);
					else
						connectionList.Add(EBraceConnectionTypes.FabricatedTee, ConstString.CON_FABRICATED_TEE);
					connectionList.Add(EBraceConnectionTypes.SinglePlate, ConstString.CON_SINGLE_PLATE);
					connectionList.Add(EBraceConnectionTypes.EndPlate, ConstString.CON_END_PLATE);
				}

				return connectionList;
			}
		}

		/// <summary>
		/// Determines what is available in the More Data configuration menu. Lots of things affect this.
		/// </summary>
		public Dictionary<EBraceConnectionTypes, string> BraceMoreDataComponentList
		{
			get
			{
				var connectionList = new Dictionary<EBraceConnectionTypes, string>();
				var member = CommonDataStatic.SelectedMember;

				if (member == null)
					return connectionList;
				// Add these if any beam is active. Doesn't matter if we're editing a beam or not
				if (CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive || CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive)
				{
					switch (member.GussetToColumnConnection)
					{
						case EBraceConnectionTypes.ClipAngle:
							connectionList.Add(EBraceConnectionTypes.ClipAngle, ConstString.CON_CLIP_ANGLE);
							break;
						case EBraceConnectionTypes.FabricatedTee:
							connectionList.Add(EBraceConnectionTypes.FabricatedTee, ConstString.CON_FABRICATED_TEE);
							break;
						case EBraceConnectionTypes.SinglePlate:
							connectionList.Add(EBraceConnectionTypes.SinglePlate, ConstString.CON_SINGLE_PLATE);
							break;
						case EBraceConnectionTypes.EndPlate:
							connectionList.Add(EBraceConnectionTypes.EndPlate, ConstString.CON_END_PLATE);
							break;
					}
				}
				// Special cases based on the Joint Config when editing a beam
				if (MiscMethods.IsBeam(CommonDataStatic.SelectedMember.MemberType) && CommonDataStatic.JointConfig == EJointConfiguration.BraceToColumnBase)
					connectionList.Add(EBraceConnectionTypes.BasePlate, ConstString.CON_BASE_PLATE);
				
				// Add these if editing a brace
				if (!MiscMethods.IsBeam(CommonDataStatic.SelectedMember.MemberType))
				{
					connectionList.Add(EBraceConnectionTypes.Brace, ConstString.CON_BRACE);
					connectionList.Add(EBraceConnectionTypes.GussetPlate, ConstString.CON_GUSSET_PLATE);

					if (member.ShapeType == EShapeType.WideFlange)
					{
						connectionList.Add(EBraceConnectionTypes.ClawAngle, ConstString.CON_CLAW_ANGLE);
						connectionList.Add(EBraceConnectionTypes.SplicePlate, ConstString.CON_SPLICE_PLATE);
					}
					if (member.ShapeType == EShapeType.HollowSteelSection && member.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
						connectionList.Add(EBraceConnectionTypes.SplicePlate, ConstString.CON_SPLICE_PLATE);
				}

				return connectionList;
			}
		}

		public Dictionary<EShearCarriedBy, string> ShearCarriedByList
		{
			get
			{
				var shearList = new Dictionary<EShearCarriedBy, string>();

				shearList.Add(EShearCarriedBy.SinglePlate, ConstString.CON_SINGLE_PLATE);
				shearList.Add(EShearCarriedBy.ClipAngle, ConstString.CON_CLIP_ANGLE);

				if (CommonDataStatic.LicenseMinimumBasic)
					shearList.Add(EShearCarriedBy.EndPlate, ConstString.CON_END_PLATE);

				if (CommonDataStatic.LicenseMinimumStandard)
				{
					shearList.Add(EShearCarriedBy.Tee, ConstString.CON_TEE);

					if (CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToGirder && CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice)
						shearList.Add(EShearCarriedBy.Seat, ConstString.CON_SEAT);
				}

				return shearList;
			}
		}

		public Dictionary<EConnectionStyle, string> ConnectionStyle
		{
			get
			{
				return new Dictionary<EConnectionStyle, string>
				{
					{EConnectionStyle.Welded, "Welded"},
					{EConnectionStyle.Bolted, "Bolted"},
				};
			}
		}

		public Dictionary<EWebOrientation, string> WebOrientation
		{
			get
			{
				return new Dictionary<EWebOrientation, string>
				{
					{EWebOrientation.InPlane, "In Plane"},
					{EWebOrientation.OutOfPlane, "Out of Plane"},
				};
			}
		}

		/// <summary>
		/// Allows the user to choose how the moment is carried by. Changes according to Joint Configuration
		/// </summary>
		public Dictionary<EMomentCarriedBy, string> MomentCarriedByList()
		{
			var components = new Dictionary<EMomentCarriedBy, string>();
			components.Add(EMomentCarriedBy.NoMoment, ConstString.CON_NO_MOMENT);

			if (CommonDataStatic.SelectedMember != null)
			{
				switch (CommonDataStatic.BeamToColumnType)
				{
					case EJointConfiguration.BeamToColumnFlange:
						switch (CommonDataStatic.SelectedMember.ShearConnection)
						{
							case EShearCarriedBy.ClipAngle:
							case EShearCarriedBy.SinglePlate:
							case EShearCarriedBy.Tee:
								components.Add(EMomentCarriedBy.DirectlyWelded, ConstString.CON_DIRECTLY_WELDED);
								if (CommonDataStatic.LicenseMinimumStandard)
								{
									components.Add(EMomentCarriedBy.FlangePlate, ConstString.CON_FLANGE_PLATE);
									components.Add(EMomentCarriedBy.Tee, ConstString.CON_TEE);
									components.Add(EMomentCarriedBy.Angles, ConstString.CON_ANGLES);
								}
								break;
							case EShearCarriedBy.EndPlate:
								if (CommonDataStatic.LicenseMinimumNext)
									components.Add(EMomentCarriedBy.EndPlate, ConstString.CON_END_PLATE);
								break;
						}
						break;
					case EJointConfiguration.BeamToColumnWeb:
						switch (CommonDataStatic.SelectedMember.ShearConnection)
						{
							case EShearCarriedBy.SinglePlate:
								components.Add(EMomentCarriedBy.DirectlyWelded, ConstString.CON_DIRECTLY_WELDED);
								if (CommonDataStatic.LicenseMinimumStandard)
									components.Add(EMomentCarriedBy.FlangePlate, ConstString.CON_FLANGE_PLATE);
								break;
							case EShearCarriedBy.EndPlate:
								components.Add(EMomentCarriedBy.EndPlate, ConstString.CON_END_PLATE);
								break;
						}
						break;
					case EJointConfiguration.BeamToHSSColumn:
						switch (CommonDataStatic.SelectedMember.ShearConnection)
						{
							case EShearCarriedBy.SinglePlate:
							case EShearCarriedBy.ClipAngle:
							case EShearCarriedBy.Tee:
								components.Add(EMomentCarriedBy.DirectlyWelded, ConstString.CON_DIRECTLY_WELDED);
								if (CommonDataStatic.LicenseMinimumStandard)
								{
									components.Add(EMomentCarriedBy.FlangePlate, ConstString.CON_FLANGE_PLATE);
									components.Add(EMomentCarriedBy.Tee, ConstString.CON_TEE);
								}
								break;
							case EShearCarriedBy.EndPlate:
								components.Add(EMomentCarriedBy.EndPlate, ConstString.CON_END_PLATE);
								break;
						}
						break;
					case EJointConfiguration.BeamToGirder:
					case EJointConfiguration.BeamSplice:
						if (CommonDataStatic.LicenseMinimumStandard)
							components.Add(EMomentCarriedBy.FlangePlate, ConstString.CON_FLANGE_PLATE);
						break;
				}
			}

			return components;
		}

		public Dictionary<ELicenseType, string> LicenseTypes
		{
			get
			{
				return new Dictionary<ELicenseType, string>
				{
					{ELicenseType.Developer_0, "DEVELOPER"},
					{ELicenseType.Demo_1, "DEMO"},
					{ELicenseType.Open_2, "OPEN"},
					{ELicenseType.Basic_3, "BASIC"},
					{ELicenseType.Standard_4, "STANDARD"},
					{ELicenseType.Next_5, "NEXT"}
				};
			}
		}

		#endregion

		#region Miscellaneous Lists

		// Every theme in the system. This is used to create the actual theme xaml files
		public Dictionary<string, byte[]> ThemeDict
		{
			get
			{
				return new Dictionary<string, byte[]>
				{
					{"Theme 01", new byte[] {12, 129, 166}},
					{"Theme 02", new byte[] {44, 53, 136}},
					{"Theme 03", new byte[] {64, 61, 122}},
					{"Theme 04", new byte[] {96, 83, 149}},
					{"Theme 05", new byte[] {81, 117, 186}},
					{"Theme 06", new byte[] {5, 69, 89}},
					{"Theme 07", new byte[] {85, 146, 179}},
					{"Theme 08", new byte[] {97, 110, 147}},
					{"Theme 09", new byte[] {58, 65, 107}},
					{"Theme 10", new byte[] {50, 50, 50}}
				};
			}
		}

		public List<int> Numbers1To4
		{
			get
			{
				return new List<int>
				{
					1,
					2,
					3,
					4
				};
			}
		}

		public List<int> Numbers0To2
		{
			get
			{
				return new List<int>
				{
					0,
					1,
					2
				};
			}
		}

		public List<int> NumberOfRecentFiles
		{
			get
			{
				return new List<int>
				{
					0,
					5,
					10,
					15,
					20
				};
			}
		}

		public Dictionary<EMemberType, string> CompleteMemberList
		{
			get
			{
				var completeMemberList = new Dictionary<EMemberType, string>();

				completeMemberList.Add(EMemberType.PrimaryMember, ConstString.MEMBER_COLUMN_OR_BEAM);
				completeMemberList.Add(EMemberType.RightBeam, ConstString.MEMBER_RIGHT_SIDE_BEAM);
				completeMemberList.Add(EMemberType.LeftBeam, ConstString.MEMBER_LEFT_SIDE_BEAM);

				if (CommonDataStatic.LicenseMinimumStandard)
				{
					completeMemberList.Add(EMemberType.UpperRight, ConstString.MEMBER_UPPER_RIGHT_BRACE);
					completeMemberList.Add(EMemberType.LowerRight, ConstString.MEMBER_LOWER_RIGHT_BRACE);
					completeMemberList.Add(EMemberType.UpperLeft, ConstString.MEMBER_UPPER_LEFT_BRACE);
					completeMemberList.Add(EMemberType.LowerLeft, ConstString.MEMBER_LOWER_LEFT_BRACE);
				}

				return completeMemberList;
			}
		}

		public Dictionary<EMemberSubType, string> MemberSubType
		{
			get
			{
				return new Dictionary<EMemberSubType, string>
				{
					{EMemberSubType.Main, "Main"},
					{EMemberSubType.Shear, "Shear"},
					{EMemberSubType.Moment, "Moment"},
					{EMemberSubType.BoltShearBeam, "BoltShearBeam"},
					{EMemberSubType.BoltShearSupport, "BoltShearSupport"},
					{EMemberSubType.BoltMomentBeam, "BoltMomentBeam"},
					{EMemberSubType.BoltMomentSupport, "BoltMomentSupport"},
					{EMemberSubType.Beam, "Beam"},
					{EMemberSubType.Stiffener, "Stiffener"}
				};
			}
		}

		public Dictionary<EClickType, string> ClickType
		{
			get
			{
				return new Dictionary<EClickType, string>
				{
					{EClickType.Single, "Single"},
					{EClickType.Double, "Double"},
					{EClickType.Right, "Right"}
				};
			}
		}

		public Dictionary<EFemaConnectionType, string> FEMAConnections
		{
			get
			{
				return new Dictionary<EFemaConnectionType, string>
				{
					{EFemaConnectionType.None, "None"},
					{EFemaConnectionType.WUFB, "WUF-B: Welded Unreinforced Flanges - Bolted Web"},
					{EFemaConnectionType.WUFW, "WUF-W: Welded Unreinforced Flanges - Welded Web"},
					{EFemaConnectionType.FF, "FF: Free Flange"},
					{EFemaConnectionType.RBS, "RBS: Reduced Beam Section"},
					{EFemaConnectionType.WFP, "WFP: Welded Flange Plate"},
					{EFemaConnectionType.BUEP, "BUEP: Bolted Unstiffened End Plate"},
					{EFemaConnectionType.BSEP, "BSEP: Bolted Stiffened End Plate"},
					{EFemaConnectionType.BFP, "BFP: Bolted Flange Plate"},
					{EFemaConnectionType.DST, "DST: Double Split Tee"}
				};
			}
		}

		public Dictionary<EPrefsMaterialDefaults, string> PrefsMaterialDefaultList
		{
			get
			{
				var completeMaterialList = new Dictionary<EPrefsMaterialDefaults, string>
				{
					{EPrefsMaterialDefaults.WShapes, "W Shapes"},
					{EPrefsMaterialDefaults.WTShapes, "WT Shapes"},
					{EPrefsMaterialDefaults.HSSShapes, "HSS Shapes"},
					{EPrefsMaterialDefaults.Angles, "Angles"},
					{EPrefsMaterialDefaults.Channels, "Channels"},
					{EPrefsMaterialDefaults.ConnectionPlate,"Connection Plates"},
					{EPrefsMaterialDefaults.StiffenerPlate,"Stiffener Plates"}
				};

				if (CommonDataStatic.LicenseMinimumNext)
					completeMaterialList.Add(EPrefsMaterialDefaults.GussetPlate, "Gusset Plate");

				return completeMaterialList;
			}
		}

		public Dictionary<EPrefsDistanceToBraceAxisSelection, string> PrefsDistanceToBraceAxis
		{
			get
			{
				return new Dictionary<EPrefsDistanceToBraceAxisSelection, string>
				{
					{EPrefsDistanceToBraceAxisSelection.t2, "2t"},
					{EPrefsDistanceToBraceAxisSelection.t3, "3t"},
					{EPrefsDistanceToBraceAxisSelection.t4, "4t"}
				};
			}
		}

		public Dictionary<EDrawingTheme, string> DrawingThemeList
		{
			get
			{
				return new Dictionary<EDrawingTheme, string>
				{
					{EDrawingTheme.Default, "Default"},
					{EDrawingTheme.Muted, "Muted"},
					{EDrawingTheme.Bright, "Bright"}
				};
			}
		}

		#endregion

		#region Bolt related Lists

		public Dictionary<int, int> NumberOfBolts2_4
		{
			get
			{
				return new Dictionary<int, int>
				{
					{2, 2},
					{4, 4},
				};
			}
		}

		public Dictionary<int, int> NumberOfBolts4_8_16
		{
			get
			{
				return new Dictionary<int, int>
				{
					{4, 4},
					{8, 8},
					{16, 16}
				};
			}
		}

		public Dictionary<EBoltMinSpacing, string> BoltMinSpacing
		{
			get
			{
				if (CommonDataStatic.Units == EUnit.US)
				{
					return new Dictionary<EBoltMinSpacing, string>
					{
						{EBoltMinSpacing.DiameterX267, "2.67 X Diameter"},
						{EBoltMinSpacing.DiameterX3, "3 X Diameter"},
						{EBoltMinSpacing.Three, "3"},
						{EBoltMinSpacing.Custom, "Custom"}
					};
				}
				else
				{
					return new Dictionary<EBoltMinSpacing, string>
					{
						{EBoltMinSpacing.DiameterX267, "67 X Diameter"},
						{EBoltMinSpacing.DiameterX3, "75 X Diameter"},
						{EBoltMinSpacing.Three, "75"},
						{EBoltMinSpacing.Custom, "Custom"}
					};
				}
			}
		}

		public List<double> BoltSizes
		{
			get
			{
				if (CommonDataStatic.Units == EUnit.Metric)
				{
					return new List<double>
					{
						16,
						20,
						22,
						24,
						27,
						30,
						36
					};
				}
				else
				{
					return new List<double>
					{
						0.625,
						0.75,
						0.875,
						1,
						1.125,
						1.25,
						1.375,
						1.5
					};
				}
			}
		}

		#endregion

		#region Report Lists

		/// <summary>
		/// List of fonts available for HTML report. Must be initialized elsewhere due to limited .Net functionality in this DLL
		/// </summary>
		public List<string> ReportFontList { get; set; }

		/// <summary>
		/// List of available font sizes for HTML report. HTML supports these strings directly
		/// </summary>
		public List<string> ReportFontSizeList
		{
			get
			{
				return new List<string>
				{
					"X-small",
					"Small",
					"Medium",
					"Large"
				};
			}
		}

		public Dictionary<EReportFileTypes, string> ReportSaveFormatList
		{
			get
			{
				return new Dictionary<EReportFileTypes, string>
				{
					{EReportFileTypes.PDF, "PDF (.pdf)"},
					{EReportFileTypes.HTML, "HTML (.html)"},
					{EReportFileTypes.RTF, "Rich Text File (.rtf)"},
					{EReportFileTypes.TXT, "Text File (.txt)"}
				};
			}
		}

		#endregion
	}
}