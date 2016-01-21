using Descon.Data;

namespace Descon.Calculations
{
	internal class PreCalcStuff
	{
		/// <summary>
		/// Sets specific values for the calcs in one place
		/// </summary>
		internal void PreCalc()
		{
			// Only do these calcs if we're in seismic.     -RM 03/16/2015
			if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic && MiscMethods.IsAnyBraceActive())
			{
				SeismicForceCalc.CalculateSeismicForces(EMemberType.UpperRight);
				SeismicForceCalc.CalculateSeismicForces(EMemberType.LowerRight);
				SeismicForceCalc.CalculateSeismicForces(EMemberType.UpperLeft);
				SeismicForceCalc.CalculateSeismicForces(EMemberType.LowerLeft);
			}

			foreach (var detailData in CommonDataStatic.DetailDataDict.Values)
			{
				// Set the default ShearForce value
				if (!detailData.IsActive)
				{
					detailData.ShearForce = 0;
					continue; // Move on if the member is not active
				}
				else if (detailData.ShearForce == 0 && detailData.MemberType != EMemberType.PrimaryMember)
					detailData.ShearForce = CommonDataStatic.Units == EUnit.US ? 15 : 66.72;

				// Set some specific Beam values
				switch (detailData.ShearConnection)
				{
					case EShearCarriedBy.ClipAngle:
						detailData.WinConnect.Beam.WebAttachBottom = detailData.WinConnect.ShearClipAngle.Length / 2;
						detailData.WinConnect.Beam.WebAttachTop = detailData.WinConnect.ShearClipAngle.Length / 2;
						if (!detailData.WinConnect.ShearClipAngle.Position_User)
						{
							if(CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && detailData.MomentConnection != EMomentCarriedBy.NoMoment)
								detailData.WinConnect.ShearClipAngle.Position = EPosition.Center;
							else if (detailData.MomentConnection == EMomentCarriedBy.Tee) 
								detailData.WinConnect.ShearClipAngle.Position = EPosition.Center;
							else 
								detailData.WinConnect.ShearClipAngle.Position = EPosition.Top;
						}
						CommonCalculations.SetBeamLv(detailData.MemberType, detailData.WinConnect.ShearClipAngle.Position, detailData.WinConnect.ShearClipAngle.BoltOslOnSupport, detailData.WinConnect.ShearClipAngle.Length);
						break;
					case EShearCarriedBy.SinglePlate:
						if (CommonDataStatic.JointConfig == EJointConfiguration.BeamToColumnWeb && detailData.MomentConnection != EMomentCarriedBy.NoMoment)
						{
							detailData.WinConnect.Beam.WebAttachBottom = detailData.WinConnect.ShearWebPlate.h1 / 2;
							detailData.WinConnect.Beam.WebAttachTop = detailData.WinConnect.ShearWebPlate.h1 / 2;
						}
						else
						{
							detailData.WinConnect.Beam.WebAttachBottom = detailData.WinConnect.ShearWebPlate.Length / 2;
							detailData.WinConnect.Beam.WebAttachTop = detailData.WinConnect.ShearWebPlate.Length / 2;
						}
						if (!detailData.WinConnect.ShearWebPlate.Position_User)
						{
							if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && detailData.MomentConnection != EMomentCarriedBy.NoMoment)
								detailData.WinConnect.ShearWebPlate.Position = EPosition.Center;
							else if (detailData.MomentConnection == EMomentCarriedBy.Tee)
								detailData.WinConnect.ShearWebPlate.Position = EPosition.Center;
							else
								detailData.WinConnect.ShearWebPlate.Position = EPosition.Top;
						}
						CommonCalculations.SetBeamLv(detailData.MemberType, detailData.WinConnect.ShearWebPlate.Position, detailData.WinConnect.ShearWebPlate.Bolt, detailData.WinConnect.ShearWebPlate.Length);
						break;
					case EShearCarriedBy.Tee:
						detailData.WinConnect.Beam.WebAttachBottom = detailData.WinConnect.ShearWebTee.SLength / 2;
						detailData.WinConnect.Beam.WebAttachTop = detailData.WinConnect.ShearWebTee.SLength / 2;
						if (!detailData.WinConnect.ShearWebTee.Position_User)
						{
							if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && detailData.MomentConnection != EMomentCarriedBy.NoMoment)
								detailData.WinConnect.ShearWebTee.Position = EPosition.Center;
							else if (detailData.MomentConnection == EMomentCarriedBy.Tee)
								detailData.WinConnect.ShearWebTee.Position = EPosition.Center;
							else
								detailData.WinConnect.ShearWebTee.Position = EPosition.Top;
						}
						CommonCalculations.SetBeamLv(detailData.MemberType, detailData.WinConnect.ShearWebTee.Position, detailData.WinConnect.ShearWebTee.BoltOslOnFlange, detailData.WinConnect.ShearWebTee.SLength);
						break;
					case EShearCarriedBy.EndPlate:
						if (!detailData.WinConnect.ShearEndPlate.Position_User)
						{
							if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnWeb && detailData.MomentConnection != EMomentCarriedBy.NoMoment)
								detailData.WinConnect.ShearEndPlate.Position = EPosition.Center;
							else if (detailData.MomentConnection == EMomentCarriedBy.Tee)
								detailData.WinConnect.ShearEndPlate.Position = EPosition.Center;
							else
								detailData.WinConnect.ShearEndPlate.Position = EPosition.Top;
						}
						CommonCalculations.SetBeamLv(detailData.MemberType, detailData.WinConnect.ShearEndPlate.Position, detailData.WinConnect.ShearEndPlate.Bolt, detailData.WinConnect.ShearEndPlate.Length);
						break;
					default:
						detailData.WinConnect.Beam.WebAttachBottom = 0;
						detailData.WinConnect.Beam.WebAttachTop = 0;
						break;
				}

				detailData.WinConnect.Beam.FrontY = detailData.WinConnect.Beam.TopElValue - detailData.Shape.d / 2;

				SetGageValues(detailData);
				SetMomentFlangeAngleValues(detailData.WinConnect.MomentFlangeAngle);
				SetBoltValues(detailData);
			}

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
				SetBeamSpliceData();

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.ColumnSplice)
				SetColumnSpliceData();

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
			{
				var girder = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
				var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
				var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

				rightBeam.WinConnect.Beam.FrontY = girder.Shape.d / 2 - (girder.WinConnect.Beam.TopElValue - rightBeam.WinConnect.Beam.TopElValue) - rightBeam.Shape.d / 2;
				leftBeam.WinConnect.Beam.FrontY = girder.Shape.d / 2 - (girder.WinConnect.Beam.TopElValue - leftBeam.WinConnect.Beam.TopElValue) - leftBeam.Shape.d / 2;
			}

			//MiscMethods.SetStiffenerValues();
		}

		/// <summary>
		/// Connections on both sides need to match so this logic sets the opposite beam to current beam's values
		/// </summary>
		private void SetBeamSpliceData()
		{
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

			CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape = new Shape();

			switch (CommonDataStatic.SelectedMember.MemberType)
			{
				case EMemberType.RightBeam:
					leftBeam.WinConnect.ShearWebPlate = rightBeam.WinConnect.ShearWebPlate.ShallowCopy();
					leftBeam.WinConnect.ShearEndPlate = rightBeam.WinConnect.ShearEndPlate.ShallowCopy();
					leftBeam.WinConnect.ShearClipAngle = rightBeam.WinConnect.ShearClipAngle.ShallowCopy();
					leftBeam.WinConnect.ShearClipAngle.SupportSideConnection = EConnectionStyle.Bolted;
					leftBeam.WinConnect.ShearWebTee = rightBeam.WinConnect.ShearWebTee.ShallowCopy();
					leftBeam.WinConnect.MomentFlangePlate = rightBeam.WinConnect.MomentFlangePlate.ShallowCopy();
					leftBeam.ShearConnection = rightBeam.ShearConnection;
					leftBeam.MomentConnection = rightBeam.MomentConnection;
					leftBeam.ShearForce = rightBeam.ShearForce;
					leftBeam.Moment = rightBeam.Moment;
					leftBeam.AxialCompression = rightBeam.AxialCompression;
					leftBeam.AxialTension = rightBeam.AxialTension;
					leftBeam.TransferCompression = rightBeam.TransferCompression;
					leftBeam.TransferTension = rightBeam.TransferTension;
					break;
				case EMemberType.LeftBeam:
					rightBeam.WinConnect.ShearWebPlate = leftBeam.WinConnect.ShearWebPlate.ShallowCopy();
					rightBeam.WinConnect.ShearEndPlate = leftBeam.WinConnect.ShearEndPlate.ShallowCopy();
					rightBeam.WinConnect.ShearClipAngle = leftBeam.WinConnect.ShearClipAngle.ShallowCopy();
					rightBeam.WinConnect.ShearClipAngle.SupportSideConnection = EConnectionStyle.Bolted;
					rightBeam.WinConnect.ShearWebTee = leftBeam.WinConnect.ShearWebTee.ShallowCopy();
					rightBeam.WinConnect.MomentFlangePlate = leftBeam.WinConnect.MomentFlangePlate.ShallowCopy();
					rightBeam.ShearConnection = leftBeam.ShearConnection;
					rightBeam.MomentConnection = leftBeam.MomentConnection;
					rightBeam.ShearForce = leftBeam.ShearForce;
					rightBeam.Moment = leftBeam.Moment;
					rightBeam.AxialCompression = leftBeam.AxialCompression;
					rightBeam.AxialTension = leftBeam.AxialTension;
					rightBeam.TransferCompression = leftBeam.TransferCompression;
					rightBeam.TransferTension = leftBeam.TransferTension;
					break;
			}
		}

		private void SetColumnSpliceData()
		{
			var colSplice = CommonDataStatic.ColumnSplice;
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			tColumn.Shape = new Shape();

			if (colSplice.Bolt.Slot0 || colSplice.Bolt.Slot1 || colSplice.Bolt.Slot3 || colSplice.Bolt.Slot3)
			{
				colSplice.HoleVertical = colSplice.Bolt.HoleDiameterSTD;
				colSplice.HoleHorizontal = colSplice.Bolt.HoleDiameterSTD;
			}
			else if (colSplice.Bolt.HoleType == EBoltHoleType.SSLP || colSplice.Bolt.HoleType == EBoltHoleType.LSLP)
			{
				colSplice.HoleVertical = colSplice.Bolt.HoleLength;
				colSplice.HoleHorizontal = colSplice.Bolt.HoleWidth;
			}
			else
			{
				colSplice.HoleVertical = colSplice.Bolt.HoleWidth;
				colSplice.HoleHorizontal = colSplice.Bolt.HoleLength;
			}

			if (tColumn.Shape.Code >= 233 && tColumn.Shape.Code <= 263)
			{
				colSplice.BoltGageWebLower = ConstNum.THREE_INCHES;
				colSplice.BoltGageWebUpper = ConstNum.THREE_INCHES;
			}
			else
			{
				colSplice.BoltGageWebLower = ConstNum.FOUR_INCHES;
				colSplice.BoltGageWebUpper = ConstNum.FOUR_INCHES;
			}
		}

		private void SetGageValues(DetailData detailData)
		{
			double gageOnColumn = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.g1;
			double gageOnFlange = CommonDataStatic.DetailDataDict[detailData.MemberType].Shape.g1;
			gageOnColumn = NumberFun.Round(gageOnColumn, ERoundingPrecision.Fourth, ERoundingStyle.RoundDown);
			gageOnFlange = NumberFun.Round(gageOnFlange, ERoundingPrecision.Fourth, ERoundingStyle.RoundDown);

			if (!detailData.GageOnColumn_User)
				detailData.GageOnColumn = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.g1;

			if (!detailData.GageOnFlange_User)
			{
				detailData.WinConnect.Beam.GageOnFlange = gageOnFlange;
				if (detailData.MomentConnection == EMomentCarriedBy.NoMoment &&
				    CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamToHSSColumn &&
				    CommonDataStatic.BeamToColumnType != EJointConfiguration.BeamSplice)
					detailData.GageOnFlange = gageOnColumn;
				else
					detailData.GageOnFlange = gageOnFlange;
			}
			else // If the user overwrote the GageOnFlange value in the Beam form
				detailData.GageOnFlange = detailData.WinConnect.Beam.GageOnFlange;

			if (!detailData.GageOnColumn_User &&
			    (CommonDataStatic.JointConfig == EJointConfiguration.BeamToGirder || CommonDataStatic.JointConfig == EJointConfiguration.BeamSplice))
				detailData.GageOnColumn = ConstNum.FIVEANDHALF_INCHES;
		}

		private void SetMomentFlangeAngleValues(WCMomentFlangeAngle momentFlangeAngle)
		{
			double gc2p;

			var columnBolt = momentFlangeAngle.ColumnBolt;
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToColumnFlange)
			{
				momentFlangeAngle.ColumnBoltSpacingOut = 2.67 * columnBolt.BoltSize;
				gc2p = ((column.Shape.bf - column.Shape.g1 - columnBolt.MinEdgeRolled) / 2);
				if (momentFlangeAngle.ColumnBoltSpacingOut > gc2p)
					momentFlangeAngle.ColumnBoltSpacingOut = 0;
			}
			else
				momentFlangeAngle.ColumnBoltSpacingOut = ConstNum.THREE_INCHES;

			momentFlangeAngle.BeamBoltSpacing3 = ConstNum.THREE_INCHES;
		}

		private void SetBoltValues(DetailData detailData)
		{
			detailData.BraceConnect.Brace.DistanceFromWP = detailData.BoltBrace.EdgeDistTransvDir;
			detailData.BraceConnect.Gusset.EdgeDistance = detailData.BoltBrace.EdgeDistTransvDir;

			if (detailData.BoltBrace.Slot0)
			{
				switch (detailData.BoltBrace.HoleDir)
				{
					case EBoltHoleDir.N:
						detailData.BraceConnect.Gusset.HoleTransP = detailData.BoltBrace.HoleLength;
						detailData.BraceConnect.Gusset.HoleLongP = detailData.BoltBrace.HoleWidth;
						detailData.BraceConnect.Gusset.EincrP = 0;
						break;
					case EBoltHoleDir.B:
						detailData.BraceConnect.Gusset.HoleTransP = detailData.BoltBrace.HoleLength;
						detailData.BraceConnect.Gusset.HoleLongP = detailData.BoltBrace.HoleWidth;
						detailData.BraceConnect.Gusset.EincrP = detailData.BoltBrace.Eincr;
						break;
					case EBoltHoleDir.T:
						detailData.BraceConnect.Gusset.HoleTransP = detailData.BoltBrace.HoleLength;
						detailData.BraceConnect.Gusset.HoleLongP = detailData.BoltBrace.HoleWidth;
						detailData.BraceConnect.Gusset.EincrP = 0;
						break;
					case EBoltHoleDir.L:
						detailData.BraceConnect.Gusset.HoleTransP = detailData.BoltBrace.HoleWidth;
						detailData.BraceConnect.Gusset.HoleLongP = detailData.BoltBrace.HoleLength;
						detailData.BraceConnect.Gusset.EincrP = detailData.BoltBrace.Eincr;
						break;
				}
			}
			else if (detailData.BoltBrace.Slot1)
			{
				switch (detailData.BoltBrace.HoleDir)
				{
					case EBoltHoleDir.B:
					case EBoltHoleDir.L:
						detailData.BraceConnect.ClawAngles.HoleLongG = detailData.BoltBrace.HoleLength;
						detailData.BraceConnect.ClawAngles.HoleTransG = detailData.BoltBrace.HoleWidth;
						detailData.BraceConnect.SplicePlates.HoleLongG = detailData.BoltBrace.HoleLength;
						detailData.BraceConnect.SplicePlates.HoleTransG = detailData.BoltBrace.HoleWidth;
						detailData.BraceConnect.Brace.WebLong = detailData.BoltBrace.HoleLength;
						detailData.BraceConnect.Brace.WebTrans = detailData.BoltBrace.HoleWidth;
						break;
					case EBoltHoleDir.T:
					case EBoltHoleDir.N:
						detailData.BraceConnect.ClawAngles.HoleLongG = detailData.BoltBrace.HoleWidth;
						detailData.BraceConnect.ClawAngles.HoleTransG = detailData.BoltBrace.HoleLength;
						detailData.BraceConnect.SplicePlates.HoleLongG = detailData.BoltBrace.HoleWidth;
						detailData.BraceConnect.SplicePlates.HoleTransG = detailData.BoltBrace.HoleLength;
						detailData.BraceConnect.Brace.WebTrans = detailData.BoltBrace.HoleLength;
						detailData.BraceConnect.Brace.WebLong = detailData.BoltBrace.HoleWidth;
						break;
				}
			}
			else if (detailData.BoltBrace.Slot2)
			{
				switch (detailData.BoltBrace.HoleDir)
				{
					case EBoltHoleDir.B:
					case EBoltHoleDir.L:
						detailData.BraceConnect.Brace.FlangeLong = detailData.BoltBrace.HoleLength;
						detailData.BraceConnect.Brace.FlangeTrans = detailData.BoltBrace.HoleWidth;
						break;
					case EBoltHoleDir.T:
					case EBoltHoleDir.N:
						detailData.BraceConnect.Brace.FlangeTrans = detailData.BoltBrace.HoleLength;
						detailData.BraceConnect.Brace.FlangeLong = detailData.BoltBrace.HoleWidth;
						break;
				}
			}
			else if (detailData.BoltBrace.Slot3)
			{
				switch (detailData.BoltBrace.HoleDir)
				{
					case EBoltHoleDir.B:
					case EBoltHoleDir.L:
						detailData.BraceConnect.ClawAngles.HoleLongB = detailData.BoltBrace.HoleLength;
						detailData.BraceConnect.ClawAngles.HoleTransB = detailData.BoltBrace.HoleWidth;
						detailData.BraceConnect.SplicePlates.HoleLongB = detailData.BoltBrace.HoleLength;
						detailData.BraceConnect.SplicePlates.HoleTransB = detailData.BoltBrace.HoleWidth;
						break;
					case EBoltHoleDir.T:
					case EBoltHoleDir.N:
						detailData.BraceConnect.ClawAngles.HoleLongB = detailData.BoltBrace.HoleWidth;
						detailData.BraceConnect.ClawAngles.HoleTransB = detailData.BoltBrace.HoleLength;
						detailData.BraceConnect.SplicePlates.HoleLongB = detailData.BoltBrace.HoleWidth;
						detailData.BraceConnect.SplicePlates.HoleTransB = detailData.BoltBrace.HoleLength;
						break;
				}
			}
			else
			{
				detailData.BraceConnect.Gusset.HoleTransP = detailData.BoltBrace.HoleDiameterSTD;
				detailData.BraceConnect.Gusset.HoleLongP = detailData.BoltBrace.HoleDiameterSTD;
				detailData.BraceConnect.Gusset.EincrP = 0;
			}
		}
	}
}