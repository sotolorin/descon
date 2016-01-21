using System;
using System.Collections.Generic;

namespace Descon.Data
{
	/// <summary>
	/// Save file data format
	/// </summary>
	[Serializable]
	public class SaveFileStructure
	{
		public EJointConfiguration JointConfig;
		public EUnit UnitSystem;
		public ECalcMode CalcMode;
		public ESteelCode SteelCode;
		public EBracingType BracingType;
		public ESeismic Seismic;
		public ELicenseType LicenseType;

		// These classes are global to all members and need to be saved seperately.
		public ColumnSplice ColumnSplice;
		public ColumnStiffener ColumnStiffener;
		public SeismicSettings SeismicSettings;

		public List<string> ReportHighlightList;
		public List<string> ReportBookmarkList;
		public List<ReportComment> ReportCommentList; 

		public List<DetailData> DetailDataList;
		public List<DimensionData> DimensionData;
        public List<CameraData> CameraData;
	}

	public class ReportComment
	{
		public string LineNumber { get; set; }
		public string Comment { get; set; }
	}

	/// <summary>
	/// Used by Unity to save the dimension and callout positioning
	/// </summary>
	public class DimensionData
	{
		public DimensionData() { }

		public bool IsDimension;
		public bool UseAddLeader;

		public float[] PointA;
		public float[] PointB;
		public float[] PointC;
		public float[] PointD;
		public float ALen;
		public float DLen;

		public string Text;
		public string AddText;
		public float LabelLength;

		public bool IsWeld;
		public int WeldType;
		public EOffsetType OffsetType;
		public float LeftOffset;
		public float RightOffset;

		public int ViewIndex;
		public string tagName;
        public string version;
	}

    /// <summary>
    /// Used by Unity to save the camera data
    /// </summary>
    public class CameraData
    {
        public CameraData() { }
        public int cameraIndex = 0;
        public float zoom;
        public float posX;
        public float posY;
        public float posZ;
        public bool hasMoved;
    }
}