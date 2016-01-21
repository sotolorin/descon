namespace Descon.Data
{
	/// <summary>
	/// Seismic settings - note that the Seismic mode itself is in Preferences
	/// </summary>
	/// [Serializable]
	public class SeismicSettings : IConnectDescon
	{
		private ESeismicDesignType _designType;

		public SeismicSettings()
		{
			EndPlateBolt = CommonDataStatic.Preferences.DefaultBolt;
			BeamWebBolt = CommonDataStatic.Preferences.DefaultBolt;
			Type = ESeismicType.Low;
			ConnectionType = ESeismicConnectionType.RBS;
			FramingType = EFramingSystem.OMF;
			DesignType = ESeismicDesignType.AISC341;
			PanelZoneDeformation = EPanelZoneDeformation.IsNotConsidered;
			DistanceFromBeamToColumn = ESeismicDistance.GreaterThanColumnDepth;
			Material = CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate;
		}

		public ESeismicType Type { get; set; }
		public ESeismicConnectionType ConnectionType { get; set; }
		public EFramingSystem FramingType { get; set; }

		public ESeismicDesignType DesignType
		{
			get { return _designType; }
			set
			{
				_designType = value;
				OnPropertyChanged("DesignType");
			}
		}

		public EPanelZoneDeformation PanelZoneDeformation { get; set; }		// DESGEN.InelasticPanelZone = true if PanelZoneDeformation = IsConsidered
		public ESeismicDistance DistanceFromBeamToColumn { get; set; }
		public EResponse Response { get; set; }

		public Bolt EndPlateBolt { get; set; }
		public Bolt BeamWebBolt { get; set; }

		public double ClearSpanOfBeamRight { get; set; }			// DESGEN.RBeamLength
		public double ClearSpanOfBeamLeft { get; set; }				// DESGEN.LBeamLength
		public double GravityLoadRight { get; set; }				// DESGEN.RBeamGload / R_Vgr
		public double GravityLoadLeft { get; set; }					// DESGEN.LBeamGload / L_Vgr
		public double DistanceToGravityLoadRight { get; set; }		// R_Vg_Dist
		public double DistanceToGravityLoadLeft { get; set; }		// L_Vg_Dist

		public bool InelasticPanelZone
		{
			get { return PanelZoneDeformation == EPanelZoneDeformation.IsConsidered; }
		}

		public bool StiffenerToEndPlateWeldIsCJP;
		public double RReducedSectionZ;
		public double LReducedSectionZ;
		public double RHingeDistance;
		public double LHingeDistance;

		public double MaxL;
		public double MinL;

		public double R_a;
		public double L_a;
		public double R_b;
		public double L_b;
		public double R_c;
		public double L_c;
		public double R_Mf;
		public double L_Mf;
		public double R_Vu;
		public double L_Vu;
		public double R_hst;
		public double L_hst;
		public double R_lst;
		public double L_lst;
		public double R_sh;
		public double L_sh;
		public double R_ts;
		public double L_ts;

		public double L_Radius;
		public double R_Radius;
	}
}