using System.Collections.Generic;

namespace CalcPad.Resources
{
	public class CommonLists
	{
		public Dictionary<EUnitSystem, string> UnitSystemDict
		{
			get
			{
				var getResources = new GetResources();
				var stringResource = getResources.GetLanguageDictionary();

				return new Dictionary<EUnitSystem, string>
				{
					{EUnitSystem.Imperial, stringResource["UnitSystemImperial"].ToString()},
					{EUnitSystem.Metric, stringResource["UnitSystemMetric"].ToString()}
				};
			}
		}

		public Dictionary<EUnitType, string> UnitTypeDict
		{
			get
			{
				var getResources = new GetResources();
				var unitResource = getResources.GetUnitDictionary();

				return new Dictionary<EUnitType, string>
				{
					{EUnitType.Length, unitResource["Length"].ToString()},
					{EUnitType.LengthLarge, unitResource["LengthLarge"].ToString()},
					{EUnitType.LengthSmall, unitResource["LengthSmall"].ToString()},
					{EUnitType.Area, unitResource["Area"].ToString()},
					{EUnitType.Force, unitResource["Force"].ToString()},
					{EUnitType.ForcePerUnitLength, unitResource["ForcePerUnitLength"].ToString()},
					{EUnitType.Moment, unitResource["Moment"].ToString()},
					{EUnitType.MomentPerUnitLengthLarge, unitResource["MomentPerUnitLengthLarge"].ToString()},
					{EUnitType.MomentPerUnitLengthSmall, unitResource["MomentPerUnitLengthSmall"].ToString()}
				};
			}
		}
	}
}