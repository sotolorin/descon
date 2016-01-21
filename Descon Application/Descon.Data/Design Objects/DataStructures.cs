using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Descon.Data
{
	/// <summary>
	/// This is a shortcut so the event handler and method don't have to be implemented whenever INotifyPropertyChanged is needed.
	/// </summary>
	[Serializable]
	public abstract class INotifyPropertyChangedDescon : INotifyPropertyChanged
	{
		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public abstract class IConnectDescon : INotifyPropertyChangedDescon
	{
		public Material Material { get; set; }

		public string MaterialName
		{
			get { return Material != null ? Material.Name : string.Empty; }
			set
			{
				if (!String.IsNullOrEmpty(value))
				{
					if (CommonDataStatic.MaterialDict.ContainsKey(value))
						Material = CommonDataStatic.MaterialDict[value];
					else
					{
						Material.UserDefined = true;
						CommonDataStatic.MaterialDict.Add(Material.Name, Material);
						CommonDataStatic.NeedToSaveMaterialsOrWelds = true;
					}
				}
			}
		}

		public Weld Weld { get; set; }

		public string WeldName
		{
			get { return Weld != null ? Weld.Name : null; }
			set
			{
				if (!String.IsNullOrEmpty(value))
				{
					if (CommonDataStatic.WeldDict.ContainsKey(value))
						Weld = CommonDataStatic.WeldDict[value];
					else
					{
						Weld.UserDefined = true;
						CommonDataStatic.WeldDict.Add(Weld.Name, Weld);
						CommonDataStatic.NeedToSaveMaterialsOrWelds = true;
					}
				}
			}
		}

		// These are used for the Unity drawing code only
		[XmlIgnore] public double FrontX;
		[XmlIgnore] public double FrontY;
	}

	/// <summary>
	/// This is the definition of a line in the report. LineType is initialized to NormalLine
	/// </summary>
	public sealed class ReportLine
	{
		public ReportLine()
		{
			LineType = EReportLineType.NormalLine;
		}

		public EReportLineType LineType;
		public string LineString;
		public int LineNumber;
	}

	/// <summary>
	/// This simple class is used to hold the data for each gauge. The gauge Description that will show as text
	/// and the actual capacity value.
	/// </summary>
	public sealed class GaugeData
	{
		public string CapacityDescription;
		public double CapacityValue;
		public double GaugeNumber;
	}

	[Serializable]
	public sealed class Material
	{
		/// <summary>
		/// Creates a copy that can be edited without altering the original data
		/// </summary>
		public Material ShallowCopy()
		{
			return (Material)MemberwiseClone();
		}

		// These need to remain public to avoid data errors
	    public double Fu_US { get; set; }
		public double Fu_Metric { get; set; }
		public double Fy_US { get; set; }
		public double Fy_Metric { get; set; }

		public string Name { get; set; }

		[XmlIgnore]
	    public double Fu
	    {
		    get
		    {
				if (CommonDataStatic.Units == EUnit.US)
					return Fu_US;
			    else
					return Fu_Metric / 1000;
		    }
		    set
		    {
				if (CommonDataStatic.Units == EUnit.US)
					Fu_US = value;
			    else
					Fu_Metric = value;
		    }
	    }

		[XmlIgnore]
		public double Fy
		{
			get
			{
				if (CommonDataStatic.Units == EUnit.US)
					return Fy_US;
				else
					return Fy_Metric / 1000;
			}
			set
			{
				if (CommonDataStatic.Units == EUnit.US)
					Fy_US = value;
				else
					Fy_Metric = value;
			}
		}

        public double Ry { get; set; }
        public double Rt { get; set; }
		
		[XmlIgnore]
		public bool UserDefined { get; set; }
	}

    [Serializable]
	public sealed class Weld
	{
		private double _fexx;

		/// <summary>
		/// Creates a copy that can be edited without altering the original data
		/// </summary>
		public Weld ShallowCopy()
		{
			return (Weld)MemberwiseClone();
		}

		public string Name { get; set; }

	    public double Fexx
	    {
		    get { return CommonDataStatic.Units == EUnit.Metric ? _fexx / 1000 : _fexx; }
		    set { _fexx = value; }
	    }

	    public bool Metric;
		[XmlIgnore]
	    public bool UserDefined { get; set; }
	}

    [Serializable]
    public sealed class EdgeForce
    {
        public double Vc;
        public double Vb;
        public double Hc;
        public double Hb;
        public double Mc;
        public double Mb;
    }

    [Serializable]
	public sealed class BeamCopeReinforcement
	{
		public double XF1;
		public double XF2;
		public double YF1;
		public double YF2;
		public double XT1;
		public double XT2;
		public double YT1;
		public double YT2;
		public double XT3;
		public double XT4;
		public double YT3;
		public double YT4;
		public double XS1;
		public double XS2;
		public double YS1;
		public double YS2;
		public double XS3;
		public double XS4;
		public double YS3;
		public double YS4;
		public double t;
		public double Width;
		public double LengthT;
		public double LengthB;
		public double WeldSize;
		public EReinforcementType Type;
	}

    [Serializable]
    public sealed class BraceWelds
    {
        public double Weld1L;
        public double Weld1sz;
        public double Weld2L;
        public double Weld2sz;
    }

    [Serializable]
    public sealed class BraceStiffener
    {
        public double Length;
        public double Width;
        public double Thickness;
        public double Area;
        public double Force;
        public double WeldSize;
	    public double RDepth;
	    public double LDepth;
    }

	[Serializable]
	public sealed class BoltUserASTM
	{
		public string Name { get; set; }
		public int Fu { get; set; }
	}
}