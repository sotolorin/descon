using System;
using System.Linq;
using System.Xml.Serialization;

namespace Descon.Data
{
	// If properties are added to this class, they also must be added to the XML file to avoid import errors when this file is loaded.
	// You can use the [XmlIgnore] tag to add properties without requiring them to be in the XML.
	// If you need to add new fields, see documentation on the VM for steps:
	// T:\Descon 8 Files\Installer and Reference Files\Shape Database Conversion Guide.rtf
	[Serializable]
	public sealed class Shape
	{
		private double _t;
		private double _tf;
		private double _bf;
		private double _d;
		private double _tw;
		private double _g2;
		private double _bT;
		private double _ix;
		private double _rx;
		private double _iy;
		private double _zy;
		private double _sy;
		private double _ry;
		private double _j;
		private double _c;
		private double _dT;
		private double _htT;
		private double _sx;
		private double _zx;
		private double _tdes;
		private double _a;

		/// <summary>
		/// Creates a copy that can be edited without altering the original data
		/// </summary>
		public Shape ShallowCopy()
		{
			return (Shape)MemberwiseClone();
		}

		/// <summary>
		/// Sets the default Shape name to None (Name = "None")
		/// </summary>
		public Shape()
		{
			Name = ConstString.NONE;
			Code = 0;
			_t = t;
		}

		/// <summary>
		/// Bool to determine if this is a custom user shape
		/// </summary>
		public bool User { get; set; }

		/// <summary>
		/// Enum used to determine the type of the shape. Saved in the XML as a char
		/// </summary>
		public EShapeType TypeEnum { get; set; }

		/// <summary>
		/// Returns the US equivalent shape if Metric. Used for Unity so it doesn't have to convert the Metric shape values
		/// </summary>
		[XmlIgnore]
		public Shape USShape
		{
			get
			{
				if (UnitSystem == EUnit.Metric && Code != 0)
					return CommonDataStatic.AllShapes.First(s => s.Value.Code == Code && s.Value.UnitSystem == EUnit.US).Value.ShallowCopy();
				else
					return this;
			}
		}

		/// <summary>
		/// Integer code assigned to each shape. The US and it's Metric equivalent will have the same code value.
		/// </summary>
		public int Code { get; set; }

		/// <summary>
		/// Unit System - Currently only US or Metric. Reports the value saved in Unit. This makes it easier to save and import.
		/// </summary>
		[XmlIgnore]
		public EUnit UnitSystem
		{
			get { return unit == "u" ? EUnit.US : EUnit.Metric; }
		}

		/// <summary>
		/// Determines if the special HSS values for the new material should be used. True if the shape type is HSS and the material
		/// is set to ConstString.HSS_MATERIAL
		/// </summary>
		[XmlIgnore]
		public bool UseHSSMaterialValues { get; set; }

		/// <summary>
		/// Usually ColumnGage or GageOnFlange
		/// </summary>
		public double g1 { get; set; }

		// These are values calculated throughout the program and don't need to be saved
		[XmlIgnore]
		public double g0 { get; set; }

		[XmlIgnore]
		public double g2
		{
			get
			{
				if (_g2 == 0)
					_g2 = g1;
				return _g2;
			}
			set { _g2 = value; }
		}

		/// <summary>
		/// Character version of unit used to save to XML. u = US, m = Metric
		/// </summary>
		public string unit { get; set; }

		/// <summary>
		/// Shape type (string instead of enum from source spreadsheet)
		/// </summary>
		public string type { get; set; }

		/// <summary>
		/// Shape name
		/// </summary>
		public string Name { get; set; }

		public double weight { get; set; }

		public double d
		{
			get { return TypeEnum == EShapeType.HollowSteelSection ? b : _d; }
			set { _d = value; }
		}

		public double bf
		{
			// This always seems to be B. (MT 5/21/15)
			get { return TypeEnum == EShapeType.HollowSteelSection ? B : _bf; }
			set { _bf = value; }
		}

		public double b { get; set; }

		public double tw
		{
			get
			{
				if (TypeEnum == EShapeType.SingleAngle || TypeEnum == EShapeType.DoubleAngle)
					return t;
				else if (TypeEnum == EShapeType.HollowSteelSection)
					return tf;
				else
					return _tw;
			}
			set { _tw = value; }
		}

		/// <summary>
		/// Flange Thickness
		/// </summary>
		public double tf
		{
			get { return TypeEnum == EShapeType.HollowSteelSection ? tdes : _tf; }
			set { _tf = value; }
		}

		public double t
		{
			get
			{
				if (TypeEnum == EShapeType.WideFlange && !User)
					_t = d - 2 * kdet;
				return _t;
			}
			set { _t = value; }
		}

		public double kdes { get; set; }

		public double kdet { get; set; }

		public double k1 { get; set; }

		public double H { get; set; }
		public double B { get; set; }
		public double h { get; set; }
		public double sz { get; set; }

		// The following are not used in any calculations, but may be used for the graphics
		public double Ht { get; set; }
		public double id { get; set; }
		public double od { get; set; }
		public double tnom { get; set; }
		public double x { get; set; }
		public double y { get; set; }
		public double e0 { get; set; }
		public double xp { get; set; }
		public double yp { get; set; }
		public double bf_2tf { get; set; }
		public double h_tw { get; set; }
		public double cw { get; set; }
		public double wno { get; set; }
		public double sw { get; set; }
		public double qf { get; set; }
		public double qw { get; set; }
		public double ro { get; set; }
		public double rz { get; set; }

		// Some of the values only appy to when the Shape is HSS and the new material is chosen. These values like tdes have a second
		// value with _A at the end. The _A value is for the new material.
		#region Special HSS Data for new ASTM A1085 Material

		public double tdes_A { get; set; }

		public double tdes
		{
			get { return UseHSSMaterialValues ? tdes_A : _tdes; }
			set { _tdes = value; }
		}

		public double a_A { get; set; }

		public double a
		{
			get { return UseHSSMaterialValues ? a_A : _a; }
			set { _a = value; }
		}

		public double b_t_A { get; set; }

		public double b_t
		{
			get { return UseHSSMaterialValues ? b_t_A : _bT; }
			set { _bT = value; }
		}

		public double ht_t_A { get; set; }

		public double ht_t
		{
			get { return UseHSSMaterialValues ? ht_t_A : _htT; }
			set { _htT = value; }
		}

		public double d_t_A { get; set; }

		public double d_t
		{
			get { return UseHSSMaterialValues ? d_t_A : _dT; }
			set { _dT = value; }
		}

		public double ix_A { get; set; }

		public double ix
		{
			get { return UseHSSMaterialValues ? ix_A : _ix; }
			set { _ix = value; }
		}

		public double sx_A { get; set; }

		public double sx
		{
			get { return UseHSSMaterialValues ? sx_A : _sx; }
			set { _sx = value; }
		}

		public double rx_A { get; set; }

		public double rx
		{
			get { return UseHSSMaterialValues ? rx_A : _rx; }
			set { _rx = value; }
		}

		public double zx_A { get; set; }
		
		public double zx
		{
			get { return UseHSSMaterialValues ? zx_A : _zx; }
			set { _zx = value; }
		}

		public double iy_A { get; set; }

		public double iy
		{
			get { return UseHSSMaterialValues ? iy_A : _iy; }
			set { _iy = value; }
		}

		public double sy_A { get; set; }

		public double sy
		{
			get { return UseHSSMaterialValues ? sy_A : _sy; }
			set { _sy = value; }
		}

		public double zy_A { get; set; }

		public double zy
		{
			get { return UseHSSMaterialValues ? zy_A : _zy; }
			set { _zy = value; }
		}

		public double ry_A { get; set; }

		public double ry
		{
			get { return UseHSSMaterialValues ? ry_A : _ry; }
			set { _ry = value; }
		}

		public double j_A { get; set; }

		public double j
		{
			get { return UseHSSMaterialValues ? j_A : _j; }
			set { _j = value; }
		}

		public double c_A { get; set; }

		public double c
		{
			get { return UseHSSMaterialValues ? c_A : _c; }
			set { _c = value; }
		}

		#endregion
	}
}