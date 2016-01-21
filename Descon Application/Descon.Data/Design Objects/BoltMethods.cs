using System;

namespace Descon.Data
{
	public static class BoltMethods
	{
		public static Bolt GetCurrentBolt(EMemberSubType subType)
		{
			switch (subType)
			{
				case EMemberSubType.BoltShearSupport:
				case EMemberSubType.BoltShearBeam:
					switch (CommonDataStatic.SelectedMember.ShearConnection)
					{
						case EShearCarriedBy.ClipAngle:
							if (subType == EMemberSubType.BoltShearBeam)
								return CommonDataStatic.SelectedMember.WinConnect.ShearClipAngle.BoltWebOnBeam;
							else
								return CommonDataStatic.SelectedMember.WinConnect.ShearClipAngle.BoltOslOnSupport;
						case EShearCarriedBy.EndPlate:
							return CommonDataStatic.SelectedMember.WinConnect.ShearEndPlate.Bolt;
						case EShearCarriedBy.Seat:
							return CommonDataStatic.SelectedMember.WinConnect.ShearSeat.Bolt;
						case EShearCarriedBy.SinglePlate:
							return CommonDataStatic.SelectedMember.WinConnect.ShearWebPlate.Bolt;
						case EShearCarriedBy.Tee:
							if (subType == EMemberSubType.BoltShearBeam)
								return CommonDataStatic.SelectedMember.WinConnect.ShearWebTee.BoltWebOnStem;
							else
								return CommonDataStatic.SelectedMember.WinConnect.ShearWebTee.BoltOslOnFlange;

					}
					break;
				case EMemberSubType.BoltMomentSupport:
				case EMemberSubType.BoltMomentBeam:
					switch (CommonDataStatic.SelectedMember.MomentConnection)
					{
						case EMomentCarriedBy.Angles:
							if (subType == EMemberSubType.BoltMomentBeam)
								return CommonDataStatic.SelectedMember.WinConnect.MomentFlangeAngle.BeamBolt;
							else
								return CommonDataStatic.SelectedMember.WinConnect.MomentFlangeAngle.ColumnBolt;
						case EMomentCarriedBy.DirectlyWelded:
							return CommonDataStatic.SelectedMember.WinConnect.MomentDirectWeld.Bolt;
						case EMomentCarriedBy.EndPlate:
							return CommonDataStatic.SelectedMember.WinConnect.MomentEndPlate.Bolt;
						case EMomentCarriedBy.FlangePlate:
							return CommonDataStatic.SelectedMember.WinConnect.MomentFlangePlate.Bolt;
						case EMomentCarriedBy.Tee:
							if (subType == EMemberSubType.BoltMomentBeam)
								return CommonDataStatic.SelectedMember.WinConnect.MomentTee.BoltBeamStem;
							else
								return CommonDataStatic.SelectedMember.WinConnect.MomentTee.BoltColumnFlange;
					}
					break;
			}

			return null;
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

		/// <summary>
		/// Sets the names of the Bolt slots. Names depend on configuration, shear, and moment type in use. There are some additional
		/// conditions set by the HoleType that are evaluated in the Bolt object itself. Spreadsheet in TFS ID 383
		/// </summary>
		public static void SetBoltSlotNames()
		{
			if (CommonDataStatic.DetailDataDict == null)
				return;

			foreach (var detailData in CommonDataStatic.DetailDataDict.Values)
			{
				if (detailData.IsActive)
				{
					var winData = detailData.WinConnect;

					if (detailData.MemberType == EMemberType.PrimaryMember || MiscMethods.IsBeam(detailData.MemberType))
					{
						switch (CommonDataStatic.BeamToColumnType)
						{
							case EJointConfiguration.BeamToColumnFlange:
							case EJointConfiguration.BeamToColumnWeb:
							case EJointConfiguration.BeamToGirder:
							case EJointConfiguration.BeamToHSSColumn:
								switch (detailData.ShearConnection)
								{
									case EShearCarriedBy.ClipAngle:
										winData.ShearClipAngle.BoltOslOnSupport.Slot0Name = string.Empty;
										winData.ShearClipAngle.BoltOslOnSupport.Slot1Name = string.Empty;
										winData.ShearClipAngle.BoltOslOnSupport.Slot2Name = "Angles on Support";
										winData.ShearClipAngle.BoltOslOnSupport.Slot3Name = "Support";
										winData.ShearClipAngle.BoltWebOnBeam.Slot0Name = "Angles on Beam";
										winData.ShearClipAngle.BoltWebOnBeam.Slot1Name = "Beam Web";
										winData.ShearClipAngle.BoltWebOnBeam.Slot2Name = string.Empty;
										winData.ShearClipAngle.BoltWebOnBeam.Slot3Name = string.Empty;
										break;
									case EShearCarriedBy.Tee:
										winData.ShearWebTee.BoltOslOnFlange.Slot0Name = string.Empty;
										winData.ShearWebTee.BoltOslOnFlange.Slot1Name = string.Empty;
										winData.ShearWebTee.BoltOslOnFlange.Slot2Name = "Tee Flange on Support";
										winData.ShearWebTee.BoltOslOnFlange.Slot3Name = "Support";
										winData.ShearWebTee.BoltWebOnStem.Slot0Name = "Tee Stem on Beam";
										winData.ShearWebTee.BoltWebOnStem.Slot1Name = "Beam Web";
										winData.ShearWebTee.BoltWebOnStem.Slot2Name = string.Empty;
										winData.ShearWebTee.BoltWebOnStem.Slot3Name = string.Empty;
										break;
									case EShearCarriedBy.EndPlate:
										winData.ShearEndPlate.Bolt.Slot0Name = string.Empty;
										winData.ShearEndPlate.Bolt.Slot1Name = string.Empty;
										winData.ShearEndPlate.Bolt.Slot2Name = "End Plate";
										winData.ShearEndPlate.Bolt.Slot3Name = "Support";
										break;
									case EShearCarriedBy.SinglePlate:
										winData.ShearWebPlate.Bolt.Slot0Name = "Single Plate";
										winData.ShearWebPlate.Bolt.Slot1Name = "Beam Web";
										winData.ShearWebPlate.Bolt.Slot2Name = string.Empty;
										winData.ShearWebPlate.Bolt.Slot3Name = string.Empty;
										break;
									case EShearCarriedBy.Seat:
										winData.ShearSeat.Bolt.Slot0Name = string.Empty;
										winData.ShearSeat.Bolt.Slot1Name = string.Empty;
										winData.ShearSeat.Bolt.Slot2Name = "Seat";
										winData.ShearSeat.Bolt.Slot3Name = "Support";
										break;
								}

								switch (detailData.MomentConnection)
								{
									case EMomentCarriedBy.FlangePlate:
										winData.MomentFlangePlate.Bolt.Slot0Name = "Plates";
										winData.MomentFlangePlate.Bolt.Slot1Name = "Beam Flange";
										winData.MomentFlangePlate.Bolt.Slot2Name = string.Empty;
										winData.MomentFlangePlate.Bolt.Slot3Name = string.Empty;
										break;
									case EMomentCarriedBy.Tee:
										winData.MomentTee.BoltColumnFlange.Slot0Name = string.Empty;
										winData.MomentTee.BoltColumnFlange.Slot1Name = string.Empty;
										winData.MomentTee.BoltColumnFlange.Slot2Name = "Tee Flange on Column";
										winData.MomentTee.BoltColumnFlange.Slot3Name = "Column";
										winData.MomentTee.BoltBeamStem.Slot0Name = "Tee Stem on Beam";
										winData.MomentTee.BoltBeamStem.Slot1Name = "Beam Flange";
										winData.MomentTee.BoltBeamStem.Slot2Name = string.Empty;
										winData.MomentTee.BoltBeamStem.Slot3Name = string.Empty;
										break;
									case EMomentCarriedBy.Angles:
										winData.MomentFlangeAngle.ColumnBolt.Slot0Name = string.Empty;
										winData.MomentFlangeAngle.ColumnBolt.Slot1Name = string.Empty;
										winData.MomentFlangeAngle.ColumnBolt.Slot2Name = "Angle on Support";
										winData.MomentFlangeAngle.ColumnBolt.Slot3Name = "Support";
										winData.MomentFlangeAngle.BeamBolt.Slot0Name = "Angle on Beam";
										winData.MomentFlangeAngle.BeamBolt.Slot1Name = "Beam Flange";
										winData.MomentFlangeAngle.BeamBolt.Slot2Name = string.Empty;
										winData.MomentFlangeAngle.BeamBolt.Slot3Name = string.Empty;
										break;
									case EMomentCarriedBy.EndPlate:
										winData.MomentEndPlate.Bolt.Slot0Name = string.Empty;
										winData.MomentEndPlate.Bolt.Slot1Name = string.Empty;
										winData.MomentEndPlate.Bolt.Slot2Name = "End Plate";
										winData.MomentEndPlate.Bolt.Slot3Name = "Support";
										break;
								}
								break;
							case EJointConfiguration.BeamSplice:
								switch (detailData.ShearConnection)
								{
									case EShearCarriedBy.ClipAngle:
										winData.ShearClipAngle.BoltOslOnSupport.Slot0Name = string.Empty;
										winData.ShearClipAngle.BoltOslOnSupport.Slot1Name = string.Empty;
										winData.ShearClipAngle.BoltOslOnSupport.Slot2Name = "Angles on Support";
										winData.ShearClipAngle.BoltOslOnSupport.Slot3Name = string.Empty;
										winData.ShearClipAngle.BoltWebOnBeam.Slot0Name = "Angles on Beam";
										winData.ShearClipAngle.BoltWebOnBeam.Slot1Name = "Beam Web";
										winData.ShearClipAngle.BoltWebOnBeam.Slot2Name = string.Empty;
										winData.ShearClipAngle.BoltWebOnBeam.Slot3Name = string.Empty;
										break;
									case EShearCarriedBy.Tee:
										winData.ShearWebTee.BoltOslOnFlange.Slot0Name = string.Empty;
										winData.ShearWebTee.BoltOslOnFlange.Slot1Name = string.Empty;
										winData.ShearWebTee.BoltOslOnFlange.Slot2Name = "Tee Flange on Support";
										winData.ShearWebTee.BoltOslOnFlange.Slot3Name = string.Empty;
										winData.ShearWebTee.BoltWebOnStem.Slot0Name = "Tee Stem on Beam";
										winData.ShearWebTee.BoltWebOnStem.Slot1Name = "Beam Web";
										winData.ShearWebTee.BoltWebOnStem.Slot2Name = string.Empty;
										winData.ShearWebTee.BoltWebOnStem.Slot3Name = string.Empty;
										break;
									case EShearCarriedBy.EndPlate:
										if (CommonDataStatic.SelectedMember.MemberType == EMemberType.LeftBeam)
										{
											winData.ShearEndPlate.Bolt.Slot0Name = string.Empty;
											winData.ShearEndPlate.Bolt.Slot1Name = string.Empty;
											winData.ShearEndPlate.Bolt.Slot2Name = "Left End Plate";
											winData.ShearEndPlate.Bolt.Slot3Name = string.Empty;
										}
										else if (CommonDataStatic.SelectedMember.MemberType == EMemberType.RightBeam)
										{
											winData.ShearEndPlate.Bolt.Slot0Name = "Right End Plate";
											winData.ShearEndPlate.Bolt.Slot1Name = string.Empty;
											winData.ShearEndPlate.Bolt.Slot2Name = string.Empty;
											winData.ShearEndPlate.Bolt.Slot3Name = string.Empty;
										}
										break;
									case EShearCarriedBy.SinglePlate:
										if (CommonDataStatic.SelectedMember.MemberType == EMemberType.LeftBeam)
										{
											winData.ShearWebPlate.Bolt.Slot0Name = string.Empty;
											winData.ShearWebPlate.Bolt.Slot1Name = string.Empty;
											winData.ShearWebPlate.Bolt.Slot2Name = "Left Plate(s)";
											winData.ShearWebPlate.Bolt.Slot3Name = "Left Beam Web";
										}
										else if (CommonDataStatic.SelectedMember.MemberType == EMemberType.RightBeam)
										{
											winData.ShearWebPlate.Bolt.Slot0Name = "Right Plate(s)";
											winData.ShearWebPlate.Bolt.Slot1Name = "Right Beam Web";
											winData.ShearWebPlate.Bolt.Slot2Name = string.Empty;
											winData.ShearWebPlate.Bolt.Slot3Name = string.Empty;
										}
										break;
								}
								break;
							case EJointConfiguration.ColumnSplice:
								switch (CommonDataStatic.ColumnSplice.ConnectionOption)
								{
									case ESpliceConnection.FlangePlate:
										if (CommonDataStatic.SelectedMember.MemberType == EMemberType.LeftBeam) // Bottom Column
										{
											CommonDataStatic.ColumnSplice.Bolt.Slot0Name = string.Empty;
											CommonDataStatic.ColumnSplice.Bolt.Slot1Name = string.Empty;
											CommonDataStatic.ColumnSplice.Bolt.Slot2Name = "Plates on Bottom Column";
											CommonDataStatic.ColumnSplice.Bolt.Slot3Name = "Bot Column Flanges";
										}
										else if (CommonDataStatic.SelectedMember.MemberType == EMemberType.RightBeam) // Top Column
										{
											CommonDataStatic.ColumnSplice.Bolt.Slot0Name = "Plates on Top Column";
											CommonDataStatic.ColumnSplice.Bolt.Slot1Name = "Top Column Flanges";
											CommonDataStatic.ColumnSplice.Bolt.Slot2Name = string.Empty;
											CommonDataStatic.ColumnSplice.Bolt.Slot3Name = string.Empty;
										}
										break;
									default:
										if (CommonDataStatic.SelectedMember.MemberType == EMemberType.LeftBeam) // Bottom Column
										{
											CommonDataStatic.ColumnSplice.Bolt.Slot0Name = string.Empty;
											CommonDataStatic.ColumnSplice.Bolt.Slot1Name = string.Empty;
											CommonDataStatic.ColumnSplice.Bolt.Slot2Name = "Plates on Bottom Column";
											CommonDataStatic.ColumnSplice.Bolt.Slot3Name = "Bottom Column Web";
										}
										else if (CommonDataStatic.SelectedMember.MemberType == EMemberType.LeftBeam) // Bottom Column
										{
											CommonDataStatic.ColumnSplice.Bolt.Slot0Name = "Plates on Top Column";
											CommonDataStatic.ColumnSplice.Bolt.Slot1Name = "Top Column Web";
											CommonDataStatic.ColumnSplice.Bolt.Slot2Name = string.Empty;
											CommonDataStatic.ColumnSplice.Bolt.Slot3Name = string.Empty;
										}
										break;
								}
								break;
						}
					}
					else // Braces
					{
						switch (detailData.ShapeType)
						{
							case EShapeType.SingleAngle:
							case EShapeType.DoubleAngle:
								detailData.BoltGusset.Slot0Name = string.Empty;
								detailData.BoltGusset.Slot1Name = string.Empty;
								detailData.BoltGusset.Slot2Name = "Angle";
								detailData.BoltGusset.Slot3Name = "Gusset";
								break;
							case EShapeType.SingleChannel:
							case EShapeType.DoubleChannel:
								detailData.BoltGusset.Slot0Name = string.Empty;
								detailData.BoltGusset.Slot1Name = string.Empty;
								detailData.BoltGusset.Slot2Name = "Channel";
								detailData.BoltGusset.Slot3Name = "Gusset";
								break;
							case EShapeType.HollowSteelSection:
								detailData.BoltGusset.Slot0Name = string.Empty;
								detailData.BoltGusset.Slot1Name = string.Empty;
								detailData.BoltGusset.Slot2Name = "Knife Plate";
								detailData.BoltGusset.Slot3Name = "Gusset";
								break;
							case EShapeType.WTSection:
								detailData.BoltGusset.Slot0Name = string.Empty;
								detailData.BoltGusset.Slot1Name = string.Empty;
								detailData.BoltGusset.Slot2Name = "WT";
								detailData.BoltGusset.Slot3Name = "Gusset";
								break;
							case EShapeType.WideFlange:
								switch (detailData.BraceMoreDataSelection)
								{
									case EBraceConnectionTypes.ClawAngle:
									case EBraceConnectionTypes.ClipAngle:
										detailData.BoltGusset.Slot0Name = string.Empty;
										detailData.BoltGusset.Slot1Name = string.Empty;
										detailData.BoltGusset.Slot2Name = "Angles on Gusset";
										detailData.BoltGusset.Slot3Name = "Gusset";
										detailData.BoltBrace.Slot0Name = "Angles on Brace";
										detailData.BoltBrace.Slot1Name = "Brace Flanges";
										detailData.BoltBrace.Slot2Name = string.Empty;
										detailData.BoltBrace.Slot3Name = string.Empty;
										break;
									default:
										detailData.BoltGusset.Slot0Name = string.Empty;
										detailData.BoltGusset.Slot1Name = string.Empty;
										detailData.BoltGusset.Slot2Name = "Plates on Gusset";
										detailData.BoltGusset.Slot3Name = "Gusset";
										detailData.BoltBrace.Slot0Name = "Plates on Brace";
										detailData.BoltBrace.Slot1Name = "Brace Flanges";
										detailData.BoltBrace.Slot2Name = string.Empty;
										detailData.BoltBrace.Slot3Name = string.Empty;
										break;
								}
								break;
						}
						break;
					}
				}
			}
		}

		internal static double ShearedEdge(double d)
		{
			double shearedEdge = 0;

			//// This If is new in Descon 8
			//if (CommonDataStatic.Preferences != null && CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
			//	shearedEdge = RolledEdge(d);
			//else
			//{
			switch (CommonDataStatic.Units)
			{
				case EUnit.US:
					if (d <= 0.5)
						shearedEdge = d + 0.375;
					else if (d <= 0.75)
						shearedEdge = d + 0.5;
					else if (d <= 0.875)
						shearedEdge = d + 0.625;
					else if (d <= 1)
						shearedEdge = d + 0.75;
					else if (d <= 1.125)
						shearedEdge = d + 0.875;
					else if (d <= 1.25)
						shearedEdge = (d + 1);
					else
						shearedEdge = 1.75 * d;
					break;
				case EUnit.Metric:
					if (d <= 16)
						shearedEdge = 28;
					else if (d <= 20)
						shearedEdge = 34;
					else if (d <= 22)
						shearedEdge = 38;
					else if (d <= 24)
						shearedEdge = 42;
					else if (d <= 27)
						shearedEdge = 48;
					else if (d <= 30)
						shearedEdge = 52;
					else if (d <= 36)
						shearedEdge = 64;
					else
						shearedEdge = 1.75 * d;
					break;
			}
			//}

			return shearedEdge;
		}

		internal static double RolledEdge(double d)
		{
			double e = 0;

			switch (CommonDataStatic.Units)
			{
				case EUnit.US:
					if (d <= 1)
						e = d + 0.25;
					else if (d <= 1.25)
						e = d + 0.375;
					else
						e = 1.25 * d;
					break;
				case EUnit.Metric:
					if (d <= 24)
						e = (d + 6);
					else if (d <= 27)
						e = (d + 7);
					else if (d <= 30)
						e = (d + 8);
					else if (d <= 36)
						e = (d + 10);
					else
						e = 1.25 * d;
					e = (int)Math.Ceiling(e);
					break;
			}
			return e;
		}

		internal static double Pretension(double boltSize, EBoltASTM astmType)
		{
			double pretension = 0;

			if (CommonDataStatic.Units == EUnit.US)
			{
				if (boltSize <= 0.51)
					pretension = astmType == EBoltASTM.A325 ? 12 : 15;
				else if (boltSize <= 0.626)
					pretension = astmType == EBoltASTM.A325 ? 19 : 24;
				else if (boltSize <= 0.76)
					pretension = astmType == EBoltASTM.A325 ? 28 : 35;
				else if (boltSize <= 0.876)
					pretension = astmType == EBoltASTM.A325 ? 39 : 49;
				else if (boltSize <= 1.01)
					pretension = astmType == EBoltASTM.A325 ? 51 : 64;
				else if (boltSize <= 1.126)
					pretension = astmType == EBoltASTM.A325 ? 56 : 80;
				else if (boltSize <= 1.26)
					pretension = astmType == EBoltASTM.A325 ? 71 : 102;
				else if (boltSize <= 1.376)
					pretension = astmType == EBoltASTM.A325 ? 85 : 121;
				else if (boltSize <= 1.51)
					pretension = astmType == EBoltASTM.A325 ? 103 : 148;

				return pretension;
			}
			else
			{
				switch (astmType)
				{
					case EBoltASTM.A325:
					case EBoltASTM.A490:
						if (boltSize <= 16)
							pretension = astmType == EBoltASTM.A325 ? 91 : 114;
						else if (boltSize <= 20)
							pretension = astmType == EBoltASTM.A325 ? 142 : 179;
						else if (boltSize <= 22)
							pretension = astmType == EBoltASTM.A325 ? 176 : 221;
						else if (boltSize <= 24)
							pretension = astmType == EBoltASTM.A325 ? 205 : 257;
						else if (boltSize <= 27)
							pretension = astmType == EBoltASTM.A325 ? 267 : 334;
						else if (boltSize <= 30)
							pretension = astmType == EBoltASTM.A325 ? 326 : 408;
						else if (boltSize <= 36)
							pretension = astmType == EBoltASTM.A325 ? 475 : 595;
						break;
					default:
						pretension = 0;
						break;
				}

				pretension *= 1000;
				return pretension;
			}
		}

		internal static void SetHoleType(Bolt bolt, EBoltHoleType holeType)
		{
			switch (holeType)
			{
				case EBoltHoleType.STD:
					bolt.HoleDir = EBoltHoleDir.N;
					bolt.Eincr = 0;
					if (!bolt.OverrideLimitState)
						bolt.LimitState = EBoltLimitState.Serviceability;
					if (CommonDataStatic.Units == EUnit.Metric)
						bolt.HoleLength = bolt.HoleWidth = bolt.HoleDiameterSTD;
					else
						bolt.HoleLength = bolt.HoleWidth = bolt.BoltSize + 0.0625;
					break;
				case EBoltHoleType.OVS:
					bolt.HoleDir = EBoltHoleDir.B;
					if (!bolt.OverrideLimitState)
						bolt.LimitState = EBoltLimitState.Strength;
					if (CommonDataStatic.Units == EUnit.Metric)
					{
						bolt.Eincr = bolt.BoltSize <= 22 ? 2 : 3;
						if (bolt.BoltSize < 22)
							bolt.HoleLength = bolt.HoleWidth = (bolt.BoltSize + 4);
						else if (bolt.BoltSize < 27)
							bolt.HoleLength = bolt.HoleWidth = (bolt.BoltSize + 6);
						else
							bolt.HoleLength = bolt.HoleWidth = (bolt.BoltSize + 8);
					}
					else
					{
						bolt.Eincr = bolt.BoltSize < 1 ? 0.0625 : 0.125;
						if (bolt.BoltSize < 0.625)
							bolt.HoleLength = bolt.HoleWidth = bolt.BoltSize + 0.125;
						else if (bolt.BoltSize < 1)
							bolt.HoleLength = bolt.HoleWidth = bolt.BoltSize + 0.1875;
						else if (bolt.BoltSize < 1.125)
							bolt.HoleLength = bolt.HoleWidth = bolt.BoltSize + 0.25;
						else
							bolt.HoleLength = bolt.HoleWidth = bolt.BoltSize + 0.3125;
					}
					break;
				case EBoltHoleType.SSLN:
					bolt.HoleDir = EBoltHoleDir.T;
					if (!bolt.OverrideLimitState)
						bolt.LimitState = EBoltLimitState.Serviceability;
					if (CommonDataStatic.Units == EUnit.Metric)
					{
						bolt.Eincr = bolt.BoltSize < 27 ? 3 : 5;
						bolt.HoleWidth = bolt.HoleDiameterSTD;
						if (bolt.BoltSize < 22)
							bolt.HoleLength = bolt.BoltSize + 6;
						else if (bolt.BoltSize < 27)
							bolt.HoleLength = bolt.BoltSize + 8;
						else
							bolt.HoleLength = bolt.BoltSize + 10;
					}
					else
					{
						bolt.Eincr = bolt.BoltSize <= 1 ? 0.125 : 0.1875;
						bolt.HoleWidth = bolt.BoltSize + 0.0625;
						if (bolt.BoltSize < 0.625)
							bolt.HoleLength = bolt.BoltSize + 0.1875;
						else if (bolt.BoltSize < 1)
							bolt.HoleLength = bolt.BoltSize + 0.25;
						else if (bolt.BoltSize < 1.125)
							bolt.HoleLength = bolt.BoltSize + 0.3125;
						else
							bolt.HoleLength = bolt.BoltSize + 0.375;
					}
					break;
				case EBoltHoleType.LSLN:
					bolt.HoleDir = EBoltHoleDir.T;
					if (!bolt.OverrideLimitState)
						bolt.LimitState = EBoltLimitState.Serviceability;
					if (CommonDataStatic.Units == EUnit.Metric)
					{
						bolt.Eincr = Math.Ceiling(0.75 * bolt.BoltSize);
						bolt.HoleWidth = bolt.HoleDiameterSTD;
						bolt.HoleLength = Math.Floor(2.5 * bolt.BoltSize);
					}
					else
					{
						bolt.Eincr = 0.75 * bolt.BoltSize;
						bolt.HoleWidth = bolt.BoltSize + 0.0625;
						bolt.HoleLength = 2.5 * bolt.BoltSize;
					}
					break;
				case EBoltHoleType.SSLP:
					bolt.HoleDir = EBoltHoleDir.L;
					if (!bolt.OverrideLimitState)
						bolt.LimitState = EBoltLimitState.Strength;
					if (CommonDataStatic.Units == EUnit.Metric)
					{
						bolt.Eincr = bolt.BoltSize < 27 ? 3 : 5;
						bolt.HoleWidth = bolt.HoleDiameterSTD;
						if (bolt.BoltSize < 22)
							bolt.HoleLength = (bolt.BoltSize + 6);
						else if (bolt.BoltSize < 27)
							bolt.HoleLength = (bolt.BoltSize + 8);
						else
							bolt.HoleLength = (bolt.BoltSize + 10);
					}
					else
					{
						bolt.Eincr = bolt.BoltSize <= 1 ? 0.125 : 0.1875;
						bolt.HoleWidth = bolt.BoltSize + 0.0625;
						if (bolt.BoltSize < 0.625)
							bolt.HoleLength = bolt.BoltSize + 0.1875;
						else if (bolt.BoltSize < 1)
							bolt.HoleLength = bolt.BoltSize + 0.25;
						else if (bolt.BoltSize < 1.125)
							bolt.HoleLength = bolt.BoltSize + 0.3125;
						else
							bolt.HoleLength = bolt.BoltSize + 0.375;
					}
					break;
				case EBoltHoleType.LSLP:
					bolt.HoleDir = EBoltHoleDir.L;
					if (!bolt.OverrideLimitState)
						bolt.LimitState = EBoltLimitState.Strength;
					if (CommonDataStatic.Units == EUnit.Metric)
					{
						bolt.Eincr = (int)Math.Ceiling(0.75 * bolt.BoltSize);
						bolt.HoleWidth = bolt.HoleDiameterSTD;
						bolt.HoleLength = (int)Math.Ceiling(2.5 * bolt.BoltSize);
					}
					else
					{
						bolt.Eincr = 0.75 * bolt.BoltSize;
						bolt.HoleWidth = bolt.BoltSize + 0.0625;
						bolt.HoleLength = 2.5 * bolt.BoltSize;
					}
					break;
			}

			bolt.OnPropertyChanged("LimitState");
		}

		/// <summary>
		/// Calculates the Bolt Strength (Fv) value and also sets Pretension
		/// </summary>
		internal static double CalculateBoltStrength(Bolt bolt)
		{
			double boltStrength = 0;

			double ft = 0;
			double hsc = 0;
			double mu = 0;
			double ab;
			double factorSC = 0;
			double factorNX;

			if (bolt.ASTMType == EBoltASTM.NonASTM)
				ft = ConstNum.FIOMEGA0_75N * 0.75 * bolt.ASTM.Fu;

			if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC13)
			{
				if (CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD)
				{
					factorNX = 0.5;
					factorSC = bolt.LimitState == EBoltLimitState.Serviceability ? 1 / 1.5 : 1 / 1.76;
				}
				else
				{
					factorNX = 0.75;
					factorSC = bolt.LimitState == EBoltLimitState.Serviceability ? 1 : 0.85;
				}
			}
			else
			{
				switch (bolt.HoleType)
				{
					case EBoltHoleType.SSLN:
					case EBoltHoleType.STD:
						factorSC = CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD ? 1 / 1.5 : 1;
						break;
					case EBoltHoleType.SSLP:
					case EBoltHoleType.OVS:
						factorSC = CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD ? 1 / 1.76 : 0.85;
						break;
					case EBoltHoleType.LSLN:
					case EBoltHoleType.LSLP:
						factorSC = CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD ? 1 / 2.14 : 0.7;
						break;
				}

				factorNX = CommonDataStatic.Preferences.CalcMode == ECalcMode.ASD ? 0.5 : 0.75;
			}

			ab = Math.Pow(bolt.BoltSize, 2) * Math.PI / 4;

			bolt.Pretension = Pretension(bolt.BoltSize, bolt.ASTMType);
			if (bolt.Pretension == 0)
				bolt.Pretension = ft * ab * 0.7 * 0.75;

			switch (bolt.BoltType)
			{
				case EBoltType.SC:
					switch (bolt.SurfaceClass)
					{
						case EBoltSurfaceClass.A:
							if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC13)
								mu = 0.35;
							else
								mu = 0.3;
							break;
						case EBoltSurfaceClass.C:
							mu = 0.35;
							break;
						case EBoltSurfaceClass.B:
							mu = 0.5;
							break;
					}
					switch (bolt.HoleType)
					{
						case EBoltHoleType.STD:
							hsc = 1;
							break;
						case EBoltHoleType.OVS:
						case EBoltHoleType.SSLN:
						case EBoltHoleType.SSLP:
							hsc = 0.85;
							break;
						case EBoltHoleType.LSLN:
						case EBoltHoleType.LSLP:
							hsc = 0.7;
							break;
					}

					if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC13)
						boltStrength = factorSC * 1.13 * mu * hsc * bolt.Pretension / ab;
					else
						boltStrength = factorSC * 1.13 * mu * bolt.FillerFactor * bolt.Pretension / ab;

					if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic &&
						CommonDataStatic.SeismicSettings.Response == EResponse.RGreaterThan3 &&
						bolt.HoleType == EBoltHoleType.STD)
					{
						switch (bolt.ASTMType)
						{
							case EBoltASTM.A325:
								boltStrength = CommonDataStatic.Units == EUnit.US ? 60 : 414;
								if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
									boltStrength = CommonDataStatic.Units == EUnit.US ? 68 : 457;
								break;
							case EBoltASTM.A490:
								boltStrength = CommonDataStatic.Units == EUnit.US ? 75 : 520;
								if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
									boltStrength = CommonDataStatic.Units == EUnit.US ? 84 : 579;
								break;
						}

						boltStrength *= factorNX;
					}
					break;
				case EBoltType.N:
					switch (bolt.ASTMType)
					{
						case EBoltASTM.A325:
							boltStrength = CommonDataStatic.Units == EUnit.US ? 48 : 330;
							if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
								boltStrength = CommonDataStatic.Units == EUnit.US ? 54 : 372;
							break;
						case EBoltASTM.A490:
							boltStrength = CommonDataStatic.Units == EUnit.US ? 60 : 414;
							if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
								boltStrength = CommonDataStatic.Units == EUnit.US ? 68 : 457;
							break;
					}

					boltStrength *= factorNX;
					break;
				case EBoltType.X:
					switch (bolt.ASTMType)
					{
						case EBoltASTM.A325:
							boltStrength = CommonDataStatic.Units == EUnit.US ? 60 : 414;
							if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
								boltStrength = CommonDataStatic.Units == EUnit.US ? 68 : 457;
							break;
						case EBoltASTM.A490:
							boltStrength = CommonDataStatic.Units == EUnit.US ? 75 : 520;
							if (CommonDataStatic.Preferences.SteelCode == ESteelCode.AISC14)
								boltStrength = CommonDataStatic.Units == EUnit.US ? 84 : 579;
							break;
					}

					boltStrength *= factorNX;
					break;
			}

			boltStrength *= Math.PI * Math.Pow(bolt.BoltSize, 2) / 4;
			return CommonDataStatic.Units == EUnit.Metric ? boltStrength / 1000 : boltStrength;
		}
	}
}