using System.Linq;

namespace Descon.Data
{
	public static class ConvertUnits
	{
		/// <summary>
		/// This method converts shapes and data between US and Metric when the user switches systems. The main changes are finding
		/// the equivalent shapes and reinitializing the bolts.
		/// </summary>
		public static void UnitsChanged()
		{
			ReloadAngleShapeLists();

			if (CommonDataStatic.Preferences.Units == EUnit.US)
				CommonDataStatic.Preferences.DefaultElectrode = CommonDataStatic.WeldDict["E70XX"].ShallowCopy();
			else
				CommonDataStatic.Preferences.DefaultElectrode = CommonDataStatic.WeldDict["E48XX"].ShallowCopy();

			foreach (var detailData in CommonDataStatic.DetailDataDict)
			{
				var data = detailData.Value;
				data.Shape = !data.IsActive ? new Shape() : GetEquivalentShape(data.Shape.Code);

				CommonDataStatic.Preferences.DefaultMinimumEdgeDistances.SelectedBoltSize = CommonDataStatic.CommonLists.BoltSizes[0];

				data.EndSetback = NumberFun.Round(data.EndSetback * ConstNum.LENGTH_CONVERSION, 1);
				data.WinConnect.Beam.Lh = NumberFun.Round(data.WinConnect.Beam.Lh * ConstNum.LENGTH_CONVERSION, 1);
				data.WinConnect.Beam.DistanceToFirstBolt = NumberFun.Round(data.WinConnect.Beam.DistanceToFirstBolt * ConstNum.LENGTH_CONVERSION, 1);

				data.WinConnect.ShearClipAngle.BoltOnColumn.SetBaseValues();
				data.WinConnect.ShearClipAngle.BoltOnGusset.SetBaseValues();
				data.WinConnect.ShearClipAngle.BoltOslOnSupport.SetBaseValues();
				data.WinConnect.ShearClipAngle.BoltWebOnBeam.SetBaseValues();
				data.WinConnect.ShearClipAngle.Size = GetEquivalentShape(data.WinConnect.ShearClipAngle.Size.Code);
				data.WinConnect.ShearClipAngle.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();

				data.WinConnect.ShearEndPlate.Bolt.SetBaseValues();
				data.WinConnect.ShearEndPlate.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();

				data.WinConnect.ShearSeat.Bolt.SetBaseValues();
				data.WinConnect.ShearSeat.Angle = GetEquivalentShape(data.WinConnect.ShearSeat.Angle.Code);
				data.WinConnect.ShearSeat.TopAngle = GetEquivalentShape(data.WinConnect.ShearSeat.TopAngle.Code);
				data.WinConnect.ShearSeat.StiffenerTee = GetEquivalentShape(data.WinConnect.ShearSeat.StiffenerTee.Code);
				data.WinConnect.ShearSeat.StiffenerAngle = GetEquivalentShape(data.WinConnect.ShearSeat.StiffenerAngle.Code);
				data.WinConnect.ShearSeat.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();

				data.WinConnect.ShearWebPlate.Bolt.SetBaseValues();
				data.WinConnect.ShearWebPlate.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();

				data.WinConnect.ShearWebTee.BoltOslOnFlange.SetBaseValues();
				data.WinConnect.ShearWebTee.BoltWebOnStem.SetBaseValues();
				data.WinConnect.ShearWebTee.Size = GetEquivalentShape(data.WinConnect.ShearWebTee.Size.Code);
				data.WinConnect.ShearWebTee.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();

				data.WinConnect.MomentDirectWeld.Bolt.SetBaseValues();
				data.WinConnect.MomentDirectWeld.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();

				data.WinConnect.MomentEndPlate.Bolt.SetBaseValues();
				data.WinConnect.MomentEndPlate.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();

				data.WinConnect.MomentFlangeAngle.BeamBolt.SetBaseValues();
				data.WinConnect.MomentFlangeAngle.ColumnBolt.SetBaseValues();
				data.WinConnect.MomentFlangeAngle.Angle = GetEquivalentShape(data.WinConnect.MomentFlangeAngle.Angle.Code);
				data.WinConnect.MomentFlangeAngle.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();

				data.WinConnect.MomentFlangePlate.Bolt.SetBaseValues();
				data.WinConnect.MomentFlangePlate.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();

				data.WinConnect.MomentTee.BoltColumnFlange.SetBaseValues();
				data.WinConnect.MomentTee.BoltBeamStem.SetBaseValues();
				data.WinConnect.MomentTee.TopTeeShape = GetEquivalentShape(data.WinConnect.MomentTee.TopTeeShape.Code);
				data.WinConnect.MomentTee.BottomTeeShape = GetEquivalentShape(data.WinConnect.MomentTee.BottomTeeShape.Code);
				data.WinConnect.MomentTee.Weld = CommonDataStatic.Preferences.DefaultElectrode.ShallowCopy();

				data.BraceConnect.FabricatedTee.Bolt.SetBaseValues();

				data.BraceConnect.SplicePlates.Bolt.SetBaseValues();

				// Converts a few values manually and rounds them to avoid the values changing too much when swapping Units
				data.ShearForce = NumberFun.Round(data.ShearForce * ConstNum.KIP_KN_CONVERSION, 1);
				data.Moment = NumberFun.ConvertMoment(data.Moment);
				data.AxialTension = NumberFun.Round(data.AxialTension * ConstNum.KFT_KNM_CONVERSION, 1);
				data.AxialCompression = NumberFun.Round(data.AxialCompression * ConstNum.KFT_KNM_CONVERSION, 1);
				data.AxialTension = NumberFun.Round(data.AxialTension * ConstNum.KFT_KNM_CONVERSION, 1);
				data.AxialCompression = NumberFun.Round(data.AxialCompression * ConstNum.KFT_KNM_CONVERSION, 1);
			}
		}

		public static void ReloadAngleShapeLists()
		{
			CommonDataStatic.ShapesSingleAngle =
				CommonDataStatic.AllShapes.Where(s => s.Value.Name == ConstString.NONE ||
				                                      (s.Value.UnitSystem == CommonDataStatic.Units && s.Value.TypeEnum == EShapeType.SingleAngle))
					.OrderBy(s => s.Value.b).ThenBy(s => s.Value.d).ThenBy(s => s.Value.t)
					.ToDictionary(s => s.Key, s => s.Value);

			CommonDataStatic.ShapesSingleAngleNoNone =
				CommonDataStatic.AllShapes.Where(s => s.Value.Name == ConstString.NONE ||
				                                      (s.Value.Name != ConstString.NONE && s.Value.UnitSystem == CommonDataStatic.Units && s.Value.TypeEnum == EShapeType.SingleAngle))
					.OrderBy(s => s.Value.b).ThenBy(s => s.Value.d).ThenBy(s => s.Value.t)
					.ToDictionary(s => s.Key, s => s.Value);

			CommonDataStatic.ShapesDoubleAngle =
				CommonDataStatic.AllShapes.Where(s => s.Value.Name == ConstString.NONE ||
				                                      (s.Value.UnitSystem == CommonDataStatic.Units && s.Value.TypeEnum == EShapeType.DoubleAngle))
					.OrderBy(s => s.Value.b).ThenBy(s => s.Value.d).ThenBy(s => s.Value.t)
					.ToDictionary(s => s.Key, s => s.Value);

			CommonDataStatic.ShapesTee = CommonDataStatic.AllShapes.Where(s => s.Value.Name == ConstString.NONE ||
			                                                                   (s.Value.UnitSystem == CommonDataStatic.Units && s.Value.TypeEnum == EShapeType.WTSection))
				.OrderBy(s => NumberFun.Round(s.Value.d, ERoundingPrecision.Half, ERoundingStyle.Nearest)).ThenBy(s => s.Value.weight)
				.ToDictionary(s => s.Key, s => s.Value);

			CommonDataStatic.ShapesSingleChannel = CommonDataStatic.AllShapes.Where(s => s.Value.Name == ConstString.NONE ||
			                                                                             (s.Value.UnitSystem == CommonDataStatic.Units && s.Value.TypeEnum == EShapeType.SingleChannel))
				.OrderBy(s => NumberFun.Round(s.Value.d, ERoundingPrecision.Half, ERoundingStyle.Nearest)).ThenBy(s => s.Value.weight)
				.ToDictionary(s => s.Key, s => s.Value);
		}

		/// <summary>
		/// Gets the Metric equivalent of the current US shape and vice versa. Code 0 means we have a blank or invalid shape
		/// </summary>
		private static Shape GetEquivalentShape(int code)
		{
			if (code == 0)
				return new Shape();
			else
				return CommonDataStatic.AllShapes.First(s => s.Value.Code == code && s.Value.UnitSystem == CommonDataStatic.Units).Value.ShallowCopy();
		}
	}
}