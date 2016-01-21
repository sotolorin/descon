using System;
using Descon.Data;

namespace Descon.Calculations
{
	internal class PostCalcStuff
	{
		#region After the Calcs

		/// <summary>
		/// Takes care of some final data manipulation
		/// </summary>
		internal void PostCalc()
		{
			foreach (var detailData in CommonDataStatic.DetailDataDict.Values)
			{
				CalcBraceXAndY(detailData);
			}

			BoltMethods.SetHoleTypesEnabledOrDisabled(CommonDataStatic.DetailDataDict[EMemberType.RightBeam]);
			BoltMethods.SetHoleTypesEnabledOrDisabled(CommonDataStatic.DetailDataDict[EMemberType.LeftBeam]);

			//BoltMethods.SetBoltSlotNames();
		}

		private void CalcBraceXAndY(DetailData detailData)
		{
			DetailData beam;
			DetailData column;
			double S;
			double c;
			double Th;
			double attachmentLength;

			column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			if (detailData.MemberType == EMemberType.LowerLeft || detailData.MemberType == EMemberType.UpperLeft)
				beam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			else
				beam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

			switch (detailData.GussetToColumnConnection)
			{
				case EBraceConnectionTypes.ClipAngle:
					attachmentLength = detailData.WinConnect.ShearClipAngle.Length;
					break;
				case EBraceConnectionTypes.FabricatedTee:
					attachmentLength = detailData.BraceConnect.FabricatedTee.Length;
					break;
				case EBraceConnectionTypes.DirectlyWelded:
					attachmentLength = detailData.BraceConnect.Gusset.VerticalForceColumn;
					break;
				case EBraceConnectionTypes.SinglePlate:
					attachmentLength = detailData.WinConnect.ShearWebPlate.Length;
					break;
				case EBraceConnectionTypes.EndPlate:
					attachmentLength = detailData.WinConnect.ShearEndPlate.Length;
					break;
				default:
					attachmentLength = 0;
					break;
			}

			if (column.WebOrientation == EWebOrientation.InPlane)
				Th = detailData.Shape.d / 2;
			else if (column.ShapeType == EShapeType.HollowSteelSection)
				Th = column.Shape.bf / 2;
			else
				Th = column.Shape.tw / 2;

			S = Math.Sin(detailData.Angle * ConstNum.RADIAN);
			c = Math.Cos(detailData.Angle * ConstNum.RADIAN);

			switch (CommonDataStatic.JointConfig)
			{
				case EJointConfiguration.BraceVToBeam:
					detailData.BraceX = 0;
					detailData.BraceY = Math.Sign(S) * detailData.Shape.d / 2;
					break;
				case EJointConfiguration.BraceToColumnBase:
					detailData.BraceX = Math.Sign(c) * (Th + detailData.EndSetback);
					if (detailData.GussetToBeamConnection == EBraceConnectionTypes.DirectlyWelded)
						detailData.BraceY = detailData.BraceConnect.BasePlate.Thickness / 2 + detailData.BraceConnect.BasePlate.CornerClip;
					else
						detailData.BraceY = detailData.BraceConnect.BasePlate.Thickness / 2;
					break;
				default:
					if (beam.IsActive)
					{
						detailData.BraceX = Math.Sign(c) * (Th + detailData.EndSetback);
						detailData.BraceY = Math.Sign(S) * (beam.Shape.d / 2) + beam.WorkPointX;
					}
					else
					{
						detailData.BraceX = Math.Sign(c) * (Th + detailData.EndSetback);
						if (detailData.GussetToColumnConnection == EBraceConnectionTypes.DirectlyWelded)
						{
							detailData.BraceY = (detailData.BraceX - detailData.WorkPointX) * S / c + detailData.WorkPointY;
							detailData.BraceY = 0;
						}
						else
							detailData.BraceY = (detailData.BraceX - detailData.WorkPointX) * S / c + detailData.WorkPointY - Math.Sign(S) * (attachmentLength / 2 + 1);
					}
					break;
			}
		}

		#endregion
	}
}
