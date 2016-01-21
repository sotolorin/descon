using System;
using System.Linq;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class Fema
	{
		private static double[] Weldy = new double[302];
		private static double[] WeldX = new double[302];
		private static double[] WeldElementsJ1 = new double[302];
		private static double[] WeldElementsJ2 = new double[302];

		public static void SetFemaVariables(EMemberType memberType)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];
			var femaData = beam.WinConnect.Fema;

			switch (femaData.Connection)
			{
				case EFemaConnectionType.WUFB:
					femaData.WeldAccessHole = true;
					beam.MomentConnection = EMomentCarriedBy.DirectlyWelded;
					WeldedUnreinforcedFlangeBoltedWeb(memberType);
					break;
				case EFemaConnectionType.WUFW:
					femaData.WeldAccessHole = true;
					beam.MomentConnection = EMomentCarriedBy.DirectlyWelded;
					WeldedUnreinforcedFlangeWeldedWeb(memberType);
					break;
				case EFemaConnectionType.FF:
					femaData.WeldAccessHole = false;
					beam.MomentConnection = EMomentCarriedBy.DirectlyWelded;
					FreeFlangeConnection(memberType);
					break;
				case EFemaConnectionType.RBS:
					femaData.WeldAccessHole = true;
					beam.MomentConnection = EMomentCarriedBy.DirectlyWelded;
					ReducedBeamSectionConnection(memberType);
					break;
				case EFemaConnectionType.WFP:
					femaData.WeldAccessHole = false;
					beam.MomentConnection = EMomentCarriedBy.FlangePlate;
					beam.WinConnect.MomentFlangePlate.Connection = EConnectionStyle.Welded;
					beam.WinConnect.MomentFlangePlate.Material = CommonDataStatic.MaterialDict["A572-50/345"];
					beam.WinConnect.MomentFlangePlate.Bolt = femaData.FlangeBolt.ShallowCopy();
					WeldedFlangePlateConnection(memberType);
					break;
				case EFemaConnectionType.BUEP:
				case EFemaConnectionType.BSEP:
					femaData.WeldAccessHole = false;
					beam.MomentConnection = EMomentCarriedBy.EndPlate;
					beam.WinConnect.MomentEndPlate.Material = CommonDataStatic.MaterialDict["A36"];
					beam.WinConnect.MomentEndPlate.Bolt = femaData.FlangeBolt;
					if (femaData.Connection == EFemaConnectionType.BUEP)
						BoltedUnstiffenedEndPlateConnection(memberType);
					else // BSEP
						BoltedStiffenedEndPlateConnection(memberType);
					break;
				case EFemaConnectionType.BFP:
					femaData.WeldAccessHole = false;
					beam.MomentConnection = EMomentCarriedBy.FlangePlate;
					beam.WinConnect.MomentFlangePlate.Connection = EConnectionStyle.Bolted;
					beam.WinConnect.MomentFlangePlate.Bolt.NumberOfLines = 2;
					beam.WinConnect.MomentFlangePlate.Material = femaData.Material;
					beam.WinConnect.MomentFlangePlate.Bolt = femaData.FlangeBolt.ShallowCopy();
					BoltedFlangePlateConnection(memberType);
					break;
				case EFemaConnectionType.DST:
					femaData.WeldAccessHole = false;
					beam.MomentConnection = EMomentCarriedBy.Tee;
					beam.WinConnect.MomentTee.TeeConnectionStyle = EConnectionStyle.Bolted;
					beam.WinConnect.ShearWebPlate.Bolt = femaData.FlangeBolt;
					DoubleSplitTeeConnection(memberType);
					beam.WinConnect.MomentTee.BoltColumnFlange = beam.WinConnect.MomentTee.BoltBeamStem = femaData.FlangeBolt.ShallowCopy();
					break;
			}
			switch (femaData.Connection)
			{
				case EFemaConnectionType.DST:
				case EFemaConnectionType.BFP:
					beam.Moment = 1.2 * femaData.Myf;
					break;
				case EFemaConnectionType.BSEP:
				case EFemaConnectionType.BUEP:
				case EFemaConnectionType.FF:
				case EFemaConnectionType.RBS:
					beam.Moment = femaData.Mf;
					break;
				default:
					beam.Moment = femaData.Myf;
					break;
			}
			if (beam.Moment == 0)
				beam.Moment = femaData.Mc;
			beam.ShearForce = femaData.Vf;
		}

		private static void WeldedUnreinforcedFlangeBoltedWeb(EMemberType memberType)
		{
			double Tcp = 0;
			double Tcf_min = 0;
			double tpz = 0;
			double Vf = 0;
			double L2 = 0;
			double Myf = 0;
			int Mc = 0;
			double Ryc = 0;
			string ColumnMaterialSpec = "";
			string ColumnDepth = "";
			string BeamMaterialSpec = "";
			double MaximumFlangeThickness = 0;
			int MinimumSpanToDepthRatio = 0;
			string MaximumBeamDepth = "";
			double shf = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var femaData = beam.WinConnect.Fema;
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

			// WUF-B
			if (CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.OMF)
				return;

			// L= center-center length of the beam
			femaData.sh = ((column.Shape.d + beam.Shape.d) / 2);
			if (femaData.shf == -100)
				shf = femaData.sh;
			else
				shf = femaData.shf;

			MaximumBeamDepth = "W36 W920";
			MinimumSpanToDepthRatio = 7;
			MaximumFlangeThickness = ConstNum.ONE_INCH;
			BeamMaterialSpec = "A572-50 A992 A913-50/S75 A572M-50 A992M A913M-50/S75";
			ColumnDepth = "W8 W10 W12 W14 W200 W250 W310 W360";
			ColumnMaterialSpec = "A572-50 A992 A913-50 A913-65 A572M-50 A992M A913M-50 A913M-65";
			femaData.Ze = beam.Shape.zx;
			femaData.Cpr = (beam.Material.Fy + beam.Material.Fu) / (2 * beam.Material.Fy); // eq. 3.2
			femaData.Ry = MiscCalculationDataMethods.ExpYieldStr(beam.Material.Fy);
			Ryc = MiscCalculationDataMethods.ExpYieldStr(column.Material.Fy);
			femaData.Mpr = femaData.Cpr * femaData.Ry * femaData.Ze * beam.Material.Fy; // eq. 3.1
			femaData.L1 = (femaData.L - femaData.sh - shf); //    =  L'
			femaData.Wg = femaData.Wg; // GravityLoad 'k/ft
			femaData.Pg = femaData.Pg;
			femaData.Lp_h = (femaData.Lp - (femaData.sh - column.Shape.d / 2));
			femaData.Vp = ((2 * femaData.Mpr + femaData.Pg * (femaData.L1 - femaData.Lp_h) + femaData.Wg * Math.Pow(femaData.L1, 2) / 2) / femaData.L1); // fig. 3-3
			Mc = ((int)(femaData.Mpr + femaData.Vp * femaData.sh)); // fig. 3-4
			femaData.Mf = ((int)(femaData.Mpr + femaData.Vp * (femaData.sh - column.Shape.d / 2))); // fig. 3-4

			femaData.Cy = beam.Shape.sx / (femaData.Cpr * femaData.Ze); //  eq.3.4
			Myf = femaData.Cy * femaData.Mf; // eq.3.3
			// femaData.Vg=Shear at col. face from factored gravity load
			L2 = femaData.L - column.Shape.d;
			femaData.Vg = femaData.Pg * (L2 - femaData.Lp) / L2 + femaData.Wg * L2 / 2;
			Vf = (2 * femaData.Mf / (femaData.L - column.Shape.d) + femaData.Vg);
			femaData.Vf = Vf;

			// Panel zone thickness for simultaneous yielding of column web and beam
			// H = average of story heights above and below the joint
			tpz = femaData.Cy * Mc * (femaData.H - beam.Shape.d) / (femaData.H * 0.9 * 0.6 * column.Material.Fy * Ryc * column.Shape.d * (beam.Shape.d - beam.Shape.tf));

			// Design shear tab for V=Vf
			// Design bolts for bearing with Fi=1
			// Continuity Plates:
			Tcf_min = Math.Max(0.4 * Math.Pow(1.8 * beam.Shape.bf * beam.Shape.tf * beam.Material.Fy * femaData.Ry / (column.Material.Fy * Ryc), 0.5), beam.Shape.bf / 6);
			if (Tcf_min > column.Shape.tf)
			{
				// continuity plate thickness NOTE: Continuity plates to be welded to column flange by CJP
				// and welded to web to develop the shear strength of net length of plate
				if (beam.MemberType == EMemberType.RightBeam && leftBeam.MomentConnection != EMomentCarriedBy.NoMoment ||
				    beam.MemberType == EMemberType.LeftBeam && rightBeam.MomentConnection != EMomentCarriedBy.NoMoment)
					Tcp = Math.Max(rightBeam.Shape.tf, leftBeam.Shape.tf);
				else if (beam.MemberType == EMemberType.RightBeam)
					Tcp = rightBeam.Shape.tf / 2;
				else if (beam.MemberType == EMemberType.LeftBeam)
					Tcp = leftBeam.Shape.tf / 2;
				Tcp = Math.Max(Tcp, CommonDataStatic.ColumnStiffener.SThickness);
			}

			femaData.shf = shf;
			femaData.Ryc = Ryc;
			femaData.Mc = Mc;
			femaData.Myf = Myf;
			femaData.Vf = Vf;
			femaData.BeamMaterialSpec = BeamMaterialSpec;
			femaData.ColumnMaterialSpec = ColumnMaterialSpec;
			femaData.ColumnDepth = ColumnDepth;
			femaData.MaximumBeamDepth = MaximumBeamDepth;
			femaData.MaximumBeamFlangeThickness = MaximumFlangeThickness;
			femaData.MinimumSpanToDepthRatio = MinimumSpanToDepthRatio;
		}

		private static void WeldedUnreinforcedFlangeWeldedWeb(EMemberType memberType)
		{
			double Vf = 0;
			double Myf = 0;
			double Tcp = 0;
			double Tcf_min = 0;
			double tpz = 0;
			double Mc = 0;
			double Ryc = 0;
			string ColumnMaterialSpec = "";
			string BeamMaterialSpec = "";
			string ColumnDepth = "";
			double MaximumFlangeThickness = 0;
			int MinimumSpanToDepthRatio = 0;
			string MaximumBeamDepth = "";
			double shf = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var femaData = beam.WinConnect.Fema;

			if (CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.OMF || CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.SMF)
				return;

			// L= center-center length of the beam
			femaData.sh = ((column.Shape.d + beam.Shape.d) / 2);
			if (femaData.shf == -100)
				shf = femaData.sh;
			else
				shf = femaData.shf;

			MaximumBeamDepth = "W36 W920";
			switch (CommonDataStatic.SeismicSettings.FramingType)
			{
				case EFramingSystem.OMF:
					MinimumSpanToDepthRatio = 5;
					MaximumFlangeThickness = ConstNum.ONEANDHALF_INCHES;
					ColumnDepth = "ALL"; // all
					break;
				case EFramingSystem.SMF:
					MinimumSpanToDepthRatio = 7;
					MaximumFlangeThickness = ConstNum.ONE_INCH;
					ColumnDepth = "W12 W14 W310 W360";
					break;
			}
			BeamMaterialSpec = "A572-50 A992 A913-50/S75 A572M-50 A992M A913M-50/S75";
			ColumnMaterialSpec = "A572-50 A992 A913-50 A913-65 A572M-50 A992M A913M-50 A913M-65";
			femaData.Ze = beam.Shape.zx;
			femaData.Cpr = (beam.Material.Fy + beam.Material.Fu) / (2 * beam.Material.Fy); // eq. 3.2
			femaData.Ry = MiscCalculationDataMethods.ExpYieldStr(beam.Material.Fy);
			Ryc = MiscCalculationDataMethods.ExpYieldStr(column.Material.Fy);
			femaData.Mpr = femaData.Cpr * femaData.Ry * femaData.Ze * beam.Material.Fy; // eq. 3.1
			femaData.L1 = (femaData.L - femaData.sh - shf); //    =  L'
			femaData.Wg = femaData.Wg; // GravityLoad 'k/ft
			femaData.Pg = femaData.Pg;
			femaData.Lp_h = (femaData.Lp - (femaData.sh - column.Shape.d / 2));
			femaData.Vp = ((2 * femaData.Mpr + femaData.Pg * (femaData.L1 - femaData.Lp_h) + femaData.Wg * Math.Pow(femaData.L1, 2) / 2) / femaData.L1); // fig. 3-3

			Mc = femaData.Mpr + femaData.Vp * femaData.sh; // fig. 3-4
			femaData.Cy = beam.Shape.sx / (femaData.Cpr * femaData.Ze); //  eq.3.4
			if (!beam.WinConnect.ShearWebPlate.Thickness_User)
				beam.WinConnect.ShearWebPlate.Thickness = CommonCalculations.PlateThickness(beam.Shape.tw);
			beam.WinConnect.ShearWebPlate.Length = beam.Shape.d - 2 * (beam.Shape.tf + Math.Max(0.75 * beam.Shape.tf, NumberFun.ConvertFromFraction(12)) + ConstNum.THREE_EIGHTS_INCH) + 2 * ConstNum.EIGHTH_INCH;
			beam.WinConnect.ShearWebPlate.BeamWeldSize = beam.WinConnect.ShearWebPlate.Thickness - ConstNum.SIXTEENTH_INCH;
			if (!beam.WinConnect.ShearWebPlate.SupportWeldSize_User)
				beam.WinConnect.ShearWebPlate.SupportWeldSize = 0; // 0.3125 'CJP Figure 3-8
			// Panel zone thickness for simultaneous yielding of column web and beam
			// H = average of story heights above and below the joint
			tpz = femaData.Cy * Mc * (femaData.H - beam.Shape.d) / (femaData.H * 0.9 * 0.6 * column.Material.Fy * Ryc * column.Shape.d * (beam.Shape.d - beam.Shape.tf));

			// Continuity Plates:
			Tcf_min = Math.Max(0.4 * Math.Pow(1.8 * beam.Shape.bf * beam.Shape.tf * beam.Material.Fy * femaData.Ry / (column.Material.Fy * Ryc), 0.5), beam.Shape.bf / 6);
			if (Tcf_min > column.Shape.tf)
			{
				// continuity plate thickness NOTE: Continuity plates to be welded to column flange by CJP
				// and welded to web to develop the shear strength of net length of plate
				if (beam.MemberType == EMemberType.RightBeam && leftBeam.MomentConnection != EMomentCarriedBy.NoMoment ||
				    beam.MemberType == EMemberType.LeftBeam && rightBeam.MomentConnection != EMomentCarriedBy.NoMoment)
					Tcp = Math.Max(rightBeam.Shape.tf, leftBeam.Shape.tf);
				else if (beam.MemberType == EMemberType.RightBeam)
					Tcp = rightBeam.Shape.tf / 2;
				else if (beam.MemberType == EMemberType.LeftBeam)
					Tcp = leftBeam.Shape.tf / 2;
				Tcp = Math.Max(Tcp, CommonDataStatic.ColumnStiffener.SThickness);
			}

			femaData.shf = shf;
			femaData.Ryc = Ryc;
			femaData.Mc = Mc;
			femaData.Myf = Myf;
			femaData.Vf = Vf;
			femaData.BeamMaterialSpec = BeamMaterialSpec;
			femaData.ColumnMaterialSpec = ColumnMaterialSpec;
			femaData.ColumnDepth = ColumnDepth;
			femaData.MaximumBeamDepth = MaximumBeamDepth;
			femaData.MaximumBeamFlangeThickness = MaximumFlangeThickness;
			femaData.MinimumSpanToDepthRatio = MinimumSpanToDepthRatio;
		}

		private static void FreeFlangeConnection(EMemberType memberType)
		{
			double Myf = 0;
			double Tcp = 0;
			double Tcf_min = 0;
			double tpz = 0;
			double Plt6 = 0;
			double Maxweld2 = 0;
			double MaxWeld1 = 0;
			double w = 0;
			double Coeff = 0;
			double ex = 0;
			double k2 = 0;
			double k1 = 0;
			double k = 0;
			double Plt5 = 0;
			double Vmax = 0;
			double Plt4 = 0;
			double Teq = 0;
			double Plt3 = 0;
			double Plt2 = 0;
			double Plt1 = 0;
			double w_Column_req = 0;
			double C1 = 0;
			double Cnst = 0;
			double theta = 0;
			double wL_eff = 0;
			double Tst = 0;
			double Vst = 0;
			double Lff = 0;
			double Mc = 0;
			double Vf = 0;
			double L2 = 0;
			double Ryc = 0;
			string ColumnMaterialSpec = "";
			string BeamMaterialSpec = "";
			string ColumnDepth = "";
			double MaximumFlangeThickness = 0;
			int MinimumSpanToDepthRatio = 0;
			string MaximumBeamDepth = "";
			double shf = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var femaData = beam.WinConnect.Fema;

			// Section 3.5.3
			bool BeamFlangeSlenderness = false;
			if (CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.OMF || CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.SMF)
				return;

			// L= center-center length of the beam
			femaData.sh = ((column.Shape.d + beam.Shape.d) / 2);
			if (femaData.shf == -100)
				shf = femaData.sh;
			else
				shf = femaData.shf;

			switch (CommonDataStatic.SeismicSettings.FramingType)
			{
				case EFramingSystem.OMF:
					MaximumBeamDepth = "W36 W920";
					MinimumSpanToDepthRatio = 5;
					MaximumFlangeThickness = 1.25 * ConstNum.ONE_INCH;
					ColumnDepth = "ALL"; // all
					break;
				case EFramingSystem.SMF:
					MaximumBeamDepth = "W30 W760";
					MinimumSpanToDepthRatio = 7;
					MaximumFlangeThickness = 0.75 * ConstNum.ONE_INCH;
					ColumnDepth = "W12 W14 W310 W360";
					break;
			}
			BeamFlangeSlenderness = beam.Shape.bf / (2 * beam.Shape.tf) <= 0.305 * Math.Pow(ConstNum.ELASTICITY / beam.Material.Fy, 0.5);

			BeamMaterialSpec = "A572-50 A992 A913-50/S75 A572M-50 A992M A913M-50/S75";
			ColumnMaterialSpec = "A572-50 A992 A913-50 A913-65 A572M-50 A992M A913M-50 A913M-65";
			femaData.Ze = beam.Shape.zx;
			if (CommonDataStatic.SeismicSettings.FramingType == EFramingSystem.SMF)
				femaData.Cpr = 1.2;
			else
				femaData.Cpr = (beam.Material.Fy + beam.Material.Fu) / (2 * beam.Material.Fy); // eq. 3.2

			femaData.Ry = MiscCalculationDataMethods.ExpYieldStr(beam.Material.Fy);
			Ryc = MiscCalculationDataMethods.ExpYieldStr(column.Material.Fy);
			femaData.Mpr = femaData.Cpr * femaData.Ry * femaData.Ze * beam.Material.Fy; // eq. 3.1
			femaData.L1 = (femaData.L - femaData.sh - shf); //    =  L'
			femaData.Wg = femaData.Wg; // k/ft
			femaData.Lp_h = (femaData.Lp - (femaData.sh - column.Shape.d / 2));
			femaData.Pg = femaData.Pg;
			femaData.Vp = ((2 * femaData.Mpr + femaData.Pg * (femaData.L1 - femaData.Lp_h) + femaData.Wg * Math.Pow(femaData.L1, 2) / 2) / femaData.L1); // fig. 3-3
			L2 = femaData.L - column.Shape.d;
			femaData.Vg = femaData.Pg * (L2 - femaData.Lp) / L2 + femaData.Wg * L2 / 2;
			Vf = (2 * femaData.Mf / (femaData.L - column.Shape.d) + femaData.Vg);

			Mc = femaData.Mpr + femaData.Vp * femaData.sh; // fig. 3-4
			femaData.Cy = beam.Shape.sx / (femaData.Cpr * femaData.Ze); //  eq.3.4
			femaData.Mf = ((int)(femaData.Mpr + femaData.Vp * (femaData.sh - column.Shape.d / 2))); // fig. 3-4

			// Length of Free Flange
			Lff = 5.5 * beam.Shape.tf; // Eq. 3-10
			Vst = 2 * femaData.Mf / (femaData.L - column.Shape.d) + femaData.Vg; // Eq. 3-10
			beam.ShearForce = Vst;
			// Step 6
			Tst = femaData.Mf / (beam.Shape.d - beam.Shape.tf) - femaData.Ry * beam.Material.Fy * beam.Shape.bf * beam.Shape.tf; // Eq. 3-11

			beam.WinConnect.ShearWebPlate.Length = beam.Shape.d - 2 * beam.Shape.tf - ConstNum.TWO_INCHES * 2; // Eq. 3-12
			beam.WinConnect.ShearWebPlate.Width = 2 * Lff;
			// Step 8
			// effective weld length:
			wL_eff = beam.WinConnect.ShearWebPlate.Length / 2;
			theta = Math.Atan(Tst / (0.5 * Vst)); // * 57.296
			Cnst = EccentricWeld.GetEccentricWeld(0, 0, theta, true);
			//EccentricWeld.GetEccentricWeld(0, 0, ref Cnst, theta, true);
			Cnst = 2 * Cnst;
			C1 = CommonCalculations.WeldTypeRatio();
			w_Column_req = Math.Pow(4 * Math.Pow(Tst, 2) + Math.Pow(Vst, 2), 0.5) / (Cnst * C1 * wL_eff);
			Plt1 = 0.5 * Vst / (0.8 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * beam.Shape.d / 4); // Shear Only weld
			Plt2 = 0.5 * Vst / (0.9 * 0.6 * Math.Min(beam.WinConnect.ShearWebPlate.Material.Fy, column.Material.Fy) * beam.Shape.d / 4); // Shear Only plate
			Plt3 = Tst / (0.9 * Math.Min(beam.WinConnect.ShearWebPlate.Material.Fy, column.Material.Fy) * beam.Shape.d / 4); // Tension only
			// Equivalent Tension
			Teq = Tst / 2 + Math.Pow(Math.Pow(Tst / 2, 2) + Math.Pow(Vst / 2, 2), 0.5);
			Plt4 = Teq / (0.9 * Math.Min(beam.WinConnect.ShearWebPlate.Material.Fy, column.Material.Fy) * beam.Shape.d / 4); // Combined Tension and Shear
			// Maximum Shear
			Vmax = Math.Pow(Math.Pow(Tst / 2, 2) + Math.Pow(Vst / 2, 2), 0.5);
			Plt5 = Vmax / (0.9 * 0.6 * Math.Min(beam.WinConnect.ShearWebPlate.Material.Fy, column.Material.Fy) * beam.Shape.d / 4); // Maximum Shear Stress
			if (!beam.WinConnect.ShearWebPlate.Thickness_User)
				beam.WinConnect.ShearWebPlate.Thickness = Math.Max(Math.Max(Plt1, Plt2), Math.Max(Math.Max(Plt3, Plt4), Plt5));

			k = (beam.WinConnect.ShearWebPlate.Width - Lff - ConstNum.ONE_INCH) / beam.WinConnect.ShearWebPlate.Length;
			k1 = k;
			k2 = (beam.WinConnect.ShearWebPlate.Width - Lff) / beam.WinConnect.ShearWebPlate.Length;
			ex = Math.Abs((beam.WinConnect.ShearWebPlate.Width * (2 - 0.5858F * k) - beam.WinConnect.ShearWebPlate.Length * (k + k2 + 0.707 * Math.Pow(k, 2))) / (2 + 0.8284 * k) - Tst * (beam.WinConnect.ShearWebPlate.Length - beam.Shape.d / 4) / Vst);
			TrapezoidalWeld(memberType, k, k1, k2, ex, 0, beam.WinConnect.ShearWebPlate.Length, ref Coeff);
			w = Vst / (Coeff * C1 * beam.WinConnect.ShearWebPlate.Length);
			// Ceck Bam Web:
			MaxWeld1 = beam.Shape.tw - ConstNum.SIXTEENTH_INCH;
			Maxweld2 = 1.4141 * beam.Shape.tw * beam.Material.Fu / CommonDataStatic.Preferences.DefaultElectrode.Fexx;
			Plt6 = CommonDataStatic.Preferences.DefaultElectrode.Fexx * w / (1.4141F * beam.WinConnect.ShearWebPlate.Material.Fu);
			if (!beam.WinConnect.ShearWebPlate.Thickness_User)
				beam.WinConnect.ShearWebPlate.Thickness = CommonCalculations.PlateThickness(Math.Max(beam.WinConnect.ShearWebPlate.Thickness, Plt6));

			beam.WinConnect.ShearWebPlate.BeamWeldSize = w;
			beam.WinConnect.ShearWebPlate.SupportWeldType = EWeldType.CJP; // CJP Figure 3-9
			if (!beam.WinConnect.ShearWebPlate.SupportWeldSize_User)
				beam.WinConnect.ShearWebPlate.SupportWeldSize = 0;
			// Panel zone thickness for simultaneous yielding of column web and beam
			// H = average of story heights above and below the joint
			tpz = femaData.Cy * Mc * (femaData.H - beam.Shape.d) / (femaData.H * 0.9 * 0.6 * column.Material.Fy * Ryc * column.Shape.d * (beam.Shape.d - beam.Shape.tf)); //  eq.3-7

			// Continuity Plates:
			Tcf_min = Math.Max(0.4 * Math.Pow(1.8 * beam.Shape.bf * beam.Shape.tf * beam.Material.Fy * femaData.Ry / (column.Material.Fy * Ryc), 0.5), beam.Shape.bf / 6); // eq. 3-5, 3-6
			if (Tcf_min > column.Shape.tf)
			{
				// continuity plate thickness NOTE: Continuity plates to be welded to column flange by CJP
				//                                                             and welded to web to develop the shear strength of net length of plate
				if (beam.MemberType == EMemberType.RightBeam && leftBeam.MomentConnection != EMomentCarriedBy.NoMoment || beam.MemberType == EMemberType.LeftBeam && rightBeam.MomentConnection != EMomentCarriedBy.NoMoment)
					Tcp = Math.Max(rightBeam.Shape.tf, leftBeam.Shape.tf);
				else if (beam.MemberType == EMemberType.RightBeam)
					Tcp = rightBeam.Shape.tf / 2;
				else if (beam.MemberType == EMemberType.LeftBeam)
					Tcp = leftBeam.Shape.tf / 2;
				Tcp = Math.Max(Tcp, CommonDataStatic.ColumnStiffener.SThickness);
			}

			femaData.shf = shf;
			femaData.Ryc = Ryc;
			femaData.Mc = Mc;
			femaData.Myf = Myf;
			femaData.Vf = Vf;
			femaData.BeamMaterialSpec = BeamMaterialSpec;
			femaData.ColumnMaterialSpec = ColumnMaterialSpec;
			femaData.ColumnDepth = ColumnDepth;
			femaData.MaximumBeamDepth = MaximumBeamDepth;
			femaData.MaximumBeamFlangeThickness = MaximumFlangeThickness;
			femaData.MinimumSpanToDepthRatio = MinimumSpanToDepthRatio;
		}

		private static void WeldedFlangePlateConnection(EMemberType memberType)
		{
			double Vweb = 0;
			double Tcp = 0;
			double Tcf_min = 0;
			double tpz = 0;
			double wmax = 0;
			double LW = 0;
			double Myf = 0;
			double Mc = 0;
			double Ryc = 0;
			double shf = 0;
			double tp = 0;
			string ColumnMaterialSpec = "";
			string BeamMaterialSpec = "";
			string ColumnDepth = "";
			double MaximumFlangeThickness = 0;
			int MinimumSpanToDepthRatio = 0;
			string MaximumBeamDepth = "";
			double w = 0;
			double Length;
			double PlWidth;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var femaData = beam.WinConnect.Fema;

			if (CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.OMF || CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.SMF)
				return;

			// L= center-center length of the beam
			MaximumBeamDepth = "W36 W920";
			switch (CommonDataStatic.SeismicSettings.FramingType)
			{
				case EFramingSystem.OMF:
					MinimumSpanToDepthRatio = 5;
					MaximumFlangeThickness = ConstNum.ONEANDHALF_INCHES;
					ColumnDepth = "ALL"; // all
					break;
				case EFramingSystem.SMF:
					MinimumSpanToDepthRatio = 7;
					MaximumFlangeThickness = ConstNum.ONE_INCH;
					ColumnDepth = "W12 W14 W310 W360";
					break;
			}
			BeamMaterialSpec = "A572-50 A992 A913-50/S75 A572M-50 A992M A913M-50/S75";
			ColumnMaterialSpec = "A572-50 A992 A913-50 A913-65 A572M-50 A992M A913M-50 A913M-65";
			// Flange Plate Size:
			// PlateMaterial=Grade 50
			// Step 1,2
			Length = beam.Shape.d / 2 / 4; // Int(-4 * Beam.d / 2) / 4 ' first trial
			tp = beam.Shape.tf;
			do
			{
				PlWidth = beam.Shape.bf + 2 * (tp + ConstNum.QUARTER_INCH);
				// Step 3
				femaData.Ze = beam.Shape.zx;
				femaData.sh = (column.Shape.d / 2 + Length);
				if (femaData.shf == -100)
					shf = femaData.sh;
				else
					shf = femaData.shf;

				femaData.Cpr = (beam.Material.Fy + beam.Material.Fu) / (2 * beam.Material.Fy); // eq. 3.2
				femaData.Ry = MiscCalculationDataMethods.ExpYieldStr(beam.Material.Fy);
				Ryc = MiscCalculationDataMethods.ExpYieldStr(column.Material.Fy);
				femaData.Mpr = femaData.Cpr * femaData.Ry * femaData.Ze * beam.Material.Fy; // eq. 3.1
				femaData.Cy = beam.Shape.sx / (femaData.Cpr * femaData.Ze); //  eq.3.4
				femaData.L1 = (femaData.L - femaData.sh - shf); //    =  L'
				femaData.Pg = femaData.Pg;
				femaData.Lp_h = (femaData.Lp - Length);
				femaData.Wg = femaData.Wg;
				femaData.Vp = ((2 * femaData.Mpr + femaData.Pg * (femaData.L1 - femaData.Lp_h) + femaData.Wg * Math.Pow(femaData.L1, 2) / 2) / femaData.L1); // fig. 3-3
				femaData.Mf = ((int)(femaData.Mpr + femaData.Vp * Length));
				Mc = femaData.Mpr + femaData.Vp * femaData.sh; // fig. 3-4
				Myf = femaData.Cy * femaData.Mf; // eq.3.3
				// Step 4
				tp = (-beam.Shape.d + Math.Pow(Math.Pow(beam.Shape.d, 2) + 4 * Myf / (beam.WinConnect.ShearWebPlate.Material.Fy * PlWidth), 0.5)) / 2;

				//  Step 5
				LW = 2 * (Length - beam.EndSetback) + (beam.Shape.bf - ConstNum.TWO_INCHES); // Notes 3 and 4
				w = femaData.Mf / ((beam.Shape.d + tp) * 0.707 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * LW);
				wmax = Math.Min(tp - ConstNum.SIXTEENTH_INCH, beam.Shape.tf - ConstNum.SIXTEENTH_INCH);
				if (w > wmax && Length < femaData.L / 4)
					Length = Length + ConstNum.QUARTER_INCH;
				else
					break;
			} while (true);

			beam.WinConnect.MomentFlangePlate.TopLength = Length / 4;
			beam.WinConnect.MomentFlangePlate.TopWidth = PlWidth / 4;
			beam.WinConnect.MomentFlangePlate.TopThickness = CommonCalculations.PlateThickness(tp);
			beam.WinConnect.MomentFlangePlate.TopPlateToBeamWeldSize = w / 16;
			beam.WinConnect.MomentFlangePlate.PlateToSupportWeldType = EWeldType.CJP;
			beam.WinConnect.Beam.TopAttachThick = beam.WinConnect.MomentFlangePlate.TopThickness;
			beam.WinConnect.Beam.BotAttachThick = beam.WinConnect.MomentFlangePlate.TopThickness;

			beam.WinConnect.ShearWebPlate.Length = (beam.Shape.d - 2 * beam.Shape.kdes - ConstNum.TWO_INCHES) / 2;
			beam.WinConnect.ShearWebPlate.Width = beam.EndSetback + ConstNum.FOUR_INCHES / 2;
			beam.WinConnect.ShearWebPlate.Thickness = CommonCalculations.PlateThickness(beam.Shape.tw);
			if (!beam.WinConnect.ShearWebPlate.SupportWeldSize_User)
				beam.WinConnect.ShearWebPlate.SupportWeldSize = 0;
			beam.WinConnect.ShearWebPlate.SupportWeldType = EWeldType.CJP;
			beam.WinConnect.ShearWebPlate.BeamWeldSize = beam.WinConnect.ShearWebPlate.Thickness - ConstNum.SIXTEENTH_INCH;

			// Panel zone thickness for simultaneous yielding of column web and beam
			// H = average of story heights above and below the joint
			tpz = femaData.Cy * Mc * (femaData.H - beam.Shape.d) / (femaData.H * 0.9 * 0.6 * column.Material.Fy * Ryc * column.Shape.d * (beam.Shape.d - beam.Shape.tf)); //  eq.3-7

			// Continuity Plates:
			Tcf_min = Math.Max(0.4 * Math.Pow(1.8 * beam.WinConnect.MomentFlangePlate.TopWidth * beam.Shape.tf * beam.Material.Fy * femaData.Ry / (column.Material.Fy * Ryc), 0.5), beam.Shape.bf / 6); // eq. 3-5, 3-6
			if (Tcf_min > column.Shape.tf)
			{
				// continuity plate thickness NOTE: Continuity plates to be welded to column flange by CJP
				//                                                             and welded to web to develop the shear strength of net length of plate
				if (beam.MemberType == EMemberType.RightBeam && leftBeam.MomentConnection != EMomentCarriedBy.NoMoment || beam.MemberType == EMemberType.LeftBeam && rightBeam.MomentConnection != EMomentCarriedBy.NoMoment)
					Tcp = Math.Max(rightBeam.Shape.tf, leftBeam.Shape.tf);
				else if (beam.MemberType == EMemberType.RightBeam)
					Tcp = rightBeam.Shape.tf / 2;
				else if (beam.MemberType == EMemberType.LeftBeam)
					Tcp = leftBeam.Shape.tf / 2;
				Tcp = Math.Max(Tcp, CommonDataStatic.ColumnStiffener.SThickness);
			}

			femaData.shf = shf;
			femaData.Ryc = Ryc;
			femaData.Myf = Myf;
			femaData.Vf = Vweb;
			femaData.Mc = Mc;
			femaData.BeamMaterialSpec = BeamMaterialSpec;
			femaData.ColumnMaterialSpec = ColumnMaterialSpec;
			femaData.ColumnDepth = ColumnDepth;
			femaData.MaximumBeamDepth = MaximumBeamDepth;
			femaData.MaximumBeamFlangeThickness = MaximumFlangeThickness;
			femaData.MinimumSpanToDepthRatio = MinimumSpanToDepthRatio;
		}

		private static void ReducedBeamSectionConnection(EMemberType memberType)
		{
			double Myf = 0;
			double Tcp = 0;
			double Tcf_min = 0;
			double tpz = 0;
			double Vf = 0;
			double Mc = 0;
			double BeamslendernessMax = 0;
			double Beamslenderness = 0;
			double BF = 0;
			double yarichap = 0;
			double Ryc = 0;
			double Srbs = 0;
			double Zrbs = 0;
			double c = 0;
			string ColumnMaterialSpec = "";
			string BeamMaterialSpec = "";
			string ColumnDepth = "";
			double MaximumFlangeThickness = 0;
			int MinimumSpanToDepthRatio = 0;
			double B = 0;
			double a = 0;
			string MaximumBeamDepth = "";
			double shf = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var femaData = beam.WinConnect.Fema;

			// Section 3.5.5
			if (CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.OMF || CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.SMF)
				return;

			// L= center-center length of the beam
			femaData.sh = (column.Shape.d / 2 + 0.625 * beam.Shape.bf + 0.75 * beam.Shape.d / 2); // eq. 3-15 and 3-16
			shf = femaData.shf == -100 ? femaData.sh : femaData.shf;

			MaximumBeamDepth = "W36 W920"; // max weight 300 lb/ft, 446 kg/m

			a = 0.625 * beam.Shape.bf / 8; // Int(-8 * 0.625 * RBeam.BF) / 8
			B = 0.75 * beam.Shape.d / 8; // Int(-8 * 0.75 * Beam.d) / 8
			switch (CommonDataStatic.SeismicSettings.FramingType)
			{
				case EFramingSystem.OMF:
					MinimumSpanToDepthRatio = 5;
					MaximumFlangeThickness = 1.75 * ConstNum.ONE_INCH;
					ColumnDepth = "ALL"; // all
					break;
				case EFramingSystem.SMF:
					MinimumSpanToDepthRatio = 7;
					MaximumFlangeThickness = 1.75 * ConstNum.ONE_INCH;
					ColumnDepth = "W12 W14 W310 W360";
					break;
			}
			BeamMaterialSpec = "A572-50 A992 A913-50/S75 A572M-50 A992M A913M-50/S75";
			ColumnMaterialSpec = "A572-50 A992 A913-50 A913-65 A572M-50 A992M A913M-50 A913M-65";

			// Step 2
			femaData.Cpr = 1.15; // (beam.Material.Fy + Beam.Fu) / (2 * beam.Material.Fy) 'eq. 3.2
			c = 0.2 * beam.Shape.bf / 16;
			do
			{
				Zrbs = beam.Shape.zx - 2 * c * beam.Shape.tf * (beam.Shape.d - beam.Shape.tf);
				Srbs = beam.Shape.sx - c * (2 * beam.Shape.tf * beam.Shape.d - 4 * Math.Pow(beam.Shape.tf, 2));
				femaData.Ze = Zrbs;
				femaData.Se = Srbs;
				femaData.Ry = MiscCalculationDataMethods.ExpYieldStr(beam.Material.Fy);
				Ryc = MiscCalculationDataMethods.ExpYieldStr(column.Material.Fy);
				femaData.Mpr = femaData.Cpr * femaData.Ry * Zrbs * beam.Material.Fy; // eq. 3.1
				femaData.L1 = (femaData.L - femaData.sh - shf); //    =  L'
				femaData.Lp_h = (femaData.Lp - (femaData.sh - column.Shape.d / 2));
				femaData.Wg = femaData.Wg; // k/ft
				femaData.Pg = femaData.Pg;
				femaData.Vp = ((2 * femaData.Mpr + femaData.Pg * (femaData.L1 - femaData.Lp_h) + femaData.Wg * Math.Pow(femaData.L1, 2) / 2) / femaData.L1); // fig. 3-3
				femaData.Mf = ((int)(femaData.Mpr + femaData.Vp * (femaData.sh - column.Shape.d / 2)));
				if (femaData.Mf < femaData.Ry * beam.Shape.zx * beam.Material.Fy)
					break;
				else if (c < 0.25 * beam.Shape.bf)
					c = c + 0.01F * beam.Shape.bf / 16;
				else
					break;
			} while (true);
			Zrbs = beam.Shape.zx - 2 * c * beam.Shape.tf * (beam.Shape.d - beam.Shape.tf);
			Srbs = beam.Shape.sx - c * (2 * beam.Shape.tf * beam.Shape.d - 4 * Math.Pow(beam.Shape.tf, 2));
			femaData.Ze = Zrbs;
			femaData.Se = Srbs;

			yarichap = (4 * Math.Pow(c, 2) + Math.Pow(B, 2)) / (8 * c);
			BF = beam.Shape.bf - 2 * (c - yarichap + Math.Pow(Math.Pow(yarichap, 2) - Math.Pow(B / 3, 2), 0.5));
			Beamslenderness = BF / (2 * beam.Shape.tf);
			BeamslendernessMax = 0.305 * Math.Pow(ConstNum.ELASTICITY / beam.Material.Fy, 0.5);

			Mc = femaData.Mpr + femaData.Vp * femaData.sh; // fig. 3-4
			femaData.Cy = Srbs / (femaData.Cpr * Zrbs); //  eq.3.4
			femaData.Vg = femaData.Wg * (femaData.L - column.Shape.d) / 2 + femaData.Pg * (femaData.L - column.Shape.d - femaData.Lp) / (femaData.L - column.Shape.d);
			Vf = (2 * femaData.Mf / (femaData.L - column.Shape.d) + femaData.Vg);
			tpz = femaData.Cy * Mc * (femaData.H - beam.Shape.d) / (femaData.H * 0.9 * 0.6 * column.Material.Fy * Ryc * column.Shape.d * (beam.Shape.d - beam.Shape.tf));

			// Continuity Plates:
			Tcf_min = Math.Max(0.4 * Math.Pow(1.8 * beam.Shape.bf * beam.Shape.tf * beam.Material.Fy * femaData.Ry / (column.Material.Fy * Ryc), 0.5), beam.Shape.bf / 6);
			if (Tcf_min > column.Shape.tf)
			{
				// continuity plate thickness NOTE: Continuity plates to be welded to column flange by CJP
				// and welded to web to develop the shear strength of net length of plate
				if (beam.MemberType == EMemberType.RightBeam && leftBeam.MomentConnection != EMomentCarriedBy.NoMoment || beam.MemberType == EMemberType.LeftBeam && rightBeam.MomentConnection != EMomentCarriedBy.NoMoment)
					Tcp = Math.Max(rightBeam.Shape.tf, leftBeam.Shape.tf);
				else if (beam.MemberType == EMemberType.RightBeam)
					Tcp = rightBeam.Shape.tf / 2;
				else if (beam.MemberType == EMemberType.LeftBeam)
					Tcp = leftBeam.Shape.tf / 2;
				Tcp = Math.Max(Tcp, CommonDataStatic.ColumnStiffener.SThickness);
			}

			femaData.c = c;
			femaData.shf = shf;
			femaData.Ryc = Ryc;
			femaData.Myf = Myf;
			femaData.Mc = Mc;
			femaData.Vf = Vf;
			femaData.BeamMaterialSpec = BeamMaterialSpec;
			femaData.ColumnMaterialSpec = ColumnMaterialSpec;
			femaData.ColumnDepth = ColumnDepth;
			femaData.MaximumBeamDepth = MaximumBeamDepth;
			femaData.MaximumBeamFlangeThickness = MaximumFlangeThickness;
			femaData.MinimumSpanToDepthRatio = MinimumSpanToDepthRatio;
		}

		private static void BoltedUnstiffenedEndPlateConnection(EMemberType memberType)
		{
			double Myf = 0;
			double tfc_Limit = 0;
			double yc = 0;
			double C2 = 0;
			double tfc_req = 0;
			double c = 0;
			double C1 = 0;
			double wmax_beamweb = 0;
			double minweld = 0;
			double w_web = 0;
			double prevThickness = 0;
			double tpS = 0;
			double tpM = 0;
			double pt = 0;
			double db = 0;
			double Fyp = 0;
			double s = 0;
			double g = 0;
			double Bolt_d = 0;
			double BoltDiam = 0;
			double Ab = 0;
			double AboltShear = 0;
			double Abolt = 0;
			double d1 = 0;
			double d0 = 0;
			double Vf = 0;
			double Mc = 0;
			double shf = 0;
			double Ryc = 0;
			string ColumnMaterialSpec = "";
			string BeamMaterialSpec = "";
			string ColumnDepth = "";
			double MaximumFlangeThickness = 0;
			int MinimumSpanToDepthRatio = 0;
			string MaximumBeamDepth = "";
			double Bp = 0;
			double w = 0;
			double pf;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var femaData = beam.WinConnect.Fema;

			// Section 3.6.1
			if (CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.OMF || CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.SMF)
				return;

			// L= center-center length of the beam
			switch (CommonDataStatic.SeismicSettings.FramingType)
			{
				case EFramingSystem.OMF:
					MaximumBeamDepth = "W30 W760";
					MinimumSpanToDepthRatio = 5;
					MaximumFlangeThickness = 0.75 * ConstNum.ONE_INCH;
					ColumnDepth = "ALL"; // all
					break;
				case EFramingSystem.SMF:
					MaximumBeamDepth = "W24 W610";
					MinimumSpanToDepthRatio = 7;
					MaximumFlangeThickness = 0.75 * ConstNum.ONE_INCH;
					ColumnDepth = "W8 W10 W12 W14 W200 W250 W310 W360";
					break;
			}
			BeamMaterialSpec = "A572-50 A992 A913-50/S75 A572M-50 A992M A913M-50/S75";
			ColumnMaterialSpec = "A572-50 A992 A913-50 A913-65 A572M-50 A992M A913M-50 A913M-65";
			beam.WinConnect.MomentEndPlate.Material = CommonDataStatic.MaterialDict["A36"];

			// Step 1
			femaData.Cpr = (beam.Material.Fy + beam.Material.Fu) / (2 * beam.Material.Fy); // eq. 3.2
			femaData.Ze = beam.Shape.zx;

			femaData.Ry = MiscCalculationDataMethods.ExpYieldStr(beam.Material.Fy);
			Ryc = MiscCalculationDataMethods.ExpYieldStr(column.Material.Fy);
			femaData.Mpr = femaData.Cpr * femaData.Ry * femaData.Ze * beam.Material.Fy; // eq. 3.1
			femaData.Wg = femaData.Wg; // k/ft
			femaData.Pg = femaData.Pg;
			do
			{
				femaData.sh = (column.Shape.d / 2 + beam.WinConnect.MomentEndPlate.Thickness + beam.Shape.d / 3);
				if (femaData.shf == -100)
					shf = femaData.sh;
				else
					shf = femaData.shf;
				femaData.Cy = beam.Shape.sx / (femaData.Cpr * femaData.Ze); //  eq.3.4

				femaData.L1 = (femaData.L - femaData.sh - shf); //    =  L'
				femaData.Lp_h = (femaData.Lp - (femaData.sh - column.Shape.d / 2));
				femaData.Vp = ((2 * femaData.Mpr + femaData.Pg * (femaData.L1 - femaData.Lp_h) + femaData.Wg * Math.Pow(femaData.L1, 2) / 2) / femaData.L1); // fig. 3-3
				femaData.Mf = ((int)(femaData.Mpr + femaData.Vp * (femaData.sh - column.Shape.d / 2)));
				Mc = (femaData.Mpr + femaData.Vp * femaData.sh); // fig. 3-4
				femaData.Vg = femaData.Wg * (femaData.L - column.Shape.d) / 2 + femaData.Pg * (femaData.L - column.Shape.d - femaData.Lp) / (femaData.L - column.Shape.d);
				Vf = 2 * femaData.Mf / (femaData.L - column.Shape.d) + femaData.Vg; // Eq. 3-52

				// Step 2, 3
				pf = ConstNum.TWO_INCHES;
				d0 = (beam.Shape.d + pf - beam.Shape.tf / 2);
				d1 = beam.Shape.d - 1.5 * beam.Shape.tf - pf;
				switch (beam.WinConnect.MomentEndPlate.Bolt.ASTMType)
				{
					case EBoltASTM.A325:
						Abolt = femaData.Mf / (2 * ConstNum.COEFFICIENT_90 * (d0 + d1)); // eq 3-18
						break;
					case EBoltASTM.A490:
						Abolt = femaData.Mf / (2 * ConstNum.COEFFICIENT_113 * (d0 + d1));
						break;
				}
				AboltShear = ((2 * femaData.Mf / (femaData.L - column.Shape.d) + femaData.Pg) / (3 * beam.WinConnect.MomentEndPlate.Bolt.BoltStrength)); // eq 3-19
				Ab = Math.Max(Abolt, AboltShear);
				BoltDiam = Math.Pow(Ab * 4 / Math.PI, 0.5) / 8;

				BoltDiam = Math.Pow(Ab * 4 / Math.PI, 0.5) / 8;
				if (BoltDiam > beam.WinConnect.MomentEndPlate.Bolt.BoltSize)
					beam.WinConnect.MomentEndPlate.Bolt.BoltSize = Bolt_d;
				else
					BoltDiam = beam.WinConnect.MomentEndPlate.Bolt.BoltSize;

				beam.WinConnect.MomentEndPlate.Bolt.BoltSize = BoltDiam;

				// Step 4, 5
				g = column.GageOnFlange;
				Bp = beam.Shape.bf + ConstNum.ONE_INCH;
				s = Math.Pow(Bp * g, 0.5); // eq 3-21
				Fyp = beam.WinConnect.MomentEndPlate.Material.Fy;
				db = beam.Shape.d;
				pt = beam.Shape.tf + pf;
				tpM = Math.Pow(femaData.Mf / (0.8 * Fyp * ((db - pt) * (Bp / 2 * (1 / pf + 1 / s) + (pf + s) * 2 / g) + Bp / 2 * (db / pf + 1 / 2.0))), 0.5); // eq 3-20
				tpS = femaData.Mf / (1.1 * Fyp * Bp * (db - beam.Shape.tf)); // eq 3-22
				prevThickness = beam.WinConnect.MomentEndPlate.Thickness;
				if (!beam.WinConnect.MomentEndPlate.Thickness_User)
					beam.WinConnect.MomentEndPlate.Thickness = CommonCalculations.PlateThickness(Math.Max(tpM, tpS));
			} while (prevThickness != beam.WinConnect.MomentEndPlate.Thickness);

			if (!beam.WinConnect.MomentEndPlate.Width_User)
				beam.WinConnect.MomentEndPlate.Width = Bp / 4;
			if (!beam.WinConnect.MomentEndPlate.Length_User)
				beam.WinConnect.MomentEndPlate.Length = beam.Shape.d + 2 * (pf + beam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir) / 4;
			if (!beam.WinConnect.MomentEndPlate.DistanceToFirstBolt_User)
				beam.WinConnect.MomentEndPlate.DistanceToFirstBolt = pf;
			if (!beam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts_User)
				beam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts = 4;

			beam.WinConnect.MomentEndPlate.FlangeWeldType = EWeldType.CJP;
			if (!beam.EndSetback_User)
				beam.EndSetback = beam.WinConnect.MomentEndPlate.Thickness;
			w_web = Vf / (2 * (beam.Shape.d - 2 * beam.Shape.kdes) * 0.707 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
			minweld = CommonCalculations.MinimumWeld(beam.Shape.tw, beam.WinConnect.MomentEndPlate.Thickness);
			wmax_beamweb = beam.Material.Fu * beam.Shape.tw / (2 * 0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
			w = Math.Max(w_web, minweld);
			if (wmax_beamweb > w)
				w = wmax_beamweb;
			if (!beam.WinConnect.MomentEndPlate.WebWeldRestSize_User)
				beam.WinConnect.MomentEndPlate.WebWeldRestSize = w / 16;

			// Step 6 --Tension
			C1 = column.GageOnFlange / 2 - column.Shape.k1; // eq 3-24
			c = beam.Shape.tf + 2 * pf;
			tfc_req = Math.Pow(femaData.Mf * C1 / (2 * column.Material.Fy * c * (beam.Shape.d - beam.Shape.tf)), 0.5); // eq 3-23
			if (tfc_req > column.Shape.tf)
			{
				// Continuity plates are required
				// also check the following
				C2 = (column.Shape.bf - column.GageOnFlange) / 2;
				s = Math.Pow(C1 * C2 * (2 * column.Shape.bf - 4 * column.Shape.k1) / (C2 + 2 * C1), 0.5);
				yc = (c / 2 + s) * (1 / C2 + 2 / C1) + (C2 + C1) * (4 / c + 2 / s);
				tfc_Limit = Math.Pow(femaData.Mf / (2 * (db - beam.Shape.tf) * (0.8 * column.Material.Fy * yc)), 0.5);
			}

			femaData.shf = shf;
			femaData.Ryc = Ryc;
			femaData.Mc = Mc;
			femaData.c = c;
			femaData.Myf = Myf;
			femaData.Vf = Vf;
			femaData.BeamMaterialSpec = BeamMaterialSpec;
			femaData.ColumnMaterialSpec = ColumnMaterialSpec;
			femaData.ColumnDepth = ColumnDepth;
			femaData.MaximumBeamDepth = MaximumBeamDepth;
			femaData.MaximumBeamFlangeThickness = MaximumFlangeThickness;
			femaData.MinimumSpanToDepthRatio = MinimumSpanToDepthRatio;
		}

		private static void BoltedStiffenedEndPlateConnection(EMemberType memberType)
		{
			double Myf = 0;
			double c = 0;
			double StiffenerHeight = 0;
			double wmax_beamweb = 0;
			double minweld = 0;
			double w_web = 0;
			double TubMin = 0;
			double Tub = 0;
			double tp = 0;
			double tpMin2 = 0;
			double tpMin1 = 0;
			double dbt = 0;
			double ts = 0;
			double g = 0;
			double Bolt_d = 0;
			double Ab = 0;
			double dc = 0;
			double AboltShear = 0;
			double Abolt = 0;
			double Ffu = 0;
			double d1 = 0;
			double d0 = 0;
			double Pb = 0;
			double Vf = 0;
			double Mc = 0;
			double Ryc = 0;
			string ColumnMaterialSpec = "";
			string BeamMaterialSpec = "";
			string ColumnDepth = "";
			double MaximumFlangeThickness = 0;
			int MinimumSpanToDepthRatio = 0;
			string MaximumBeamDepth = "";
			double shf = 0;
			double w = 0;
			double Bp = 0;
			double pf;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var femaData = beam.WinConnect.Fema;

			// Section 3.6.2
			if (CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.OMF || CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.SMF)
				return;

			// L= center-center length of the beam
			femaData.sh = (column.Shape.d / 2 + beam.WinConnect.MomentEndPlate.Thickness + beam.WinConnect.MomentEndPlate.BraceStiffener.Length);
			if (femaData.shf == -100)
				shf = femaData.sh;
			else
				shf = femaData.shf;

			switch (CommonDataStatic.SeismicSettings.FramingType)
			{
				case EFramingSystem.OMF:
					MaximumBeamDepth = "W36 W920";
					MinimumSpanToDepthRatio = 5;
					MaximumFlangeThickness = ConstNum.ONE_INCH;
					ColumnDepth = "ALL"; // all
					break;
				case EFramingSystem.SMF:
					MaximumBeamDepth = "W36 W920";
					MinimumSpanToDepthRatio = 7;
					MaximumFlangeThickness = ConstNum.ONE_INCH;
					ColumnDepth = "W12 W14 W310 W360";
					break;
			}
			BeamMaterialSpec = "A572-50 A992 A913-50/S75 A572M-50 A992M A913M-50/S75";
			ColumnMaterialSpec = "A572-50 A992 A913-50 A913-65 A572M-50 A992M A913M-50 A913M-65";
			beam.WinConnect.MomentEndPlate.Material = CommonDataStatic.MaterialDict["A36"];

			// Step 1
			femaData.Cpr = (beam.Material.Fy + beam.Material.Fu) / (2 * beam.Material.Fy); // eq. 3.2
			femaData.Ze = beam.Shape.zx;

			femaData.Cy = beam.Shape.sx / (femaData.Cpr * femaData.Ze); //  eq.3.4
			femaData.Ry = MiscCalculationDataMethods.ExpYieldStr(beam.Material.Fy);
			Ryc = MiscCalculationDataMethods.ExpYieldStr(column.Material.Fy);
			femaData.Mpr = femaData.Cpr * femaData.Ry * femaData.Ze * beam.Material.Fy; // eq. 3.1
			femaData.L1 = (femaData.L - femaData.sh - shf); //    =  L'
			femaData.Wg = femaData.Wg; // GravityLoad 'k/ft
			femaData.Pg = femaData.Pg;
			femaData.Vp = ((2 * femaData.Mpr + femaData.Pg * femaData.Lp + femaData.Wg * Math.Pow(femaData.L1, 2) / 2) / femaData.L1); // fig. 3-3
			femaData.Mf = ((int)(femaData.Mpr + femaData.Vp * (femaData.sh - column.Shape.d / 2)));
			Mc = (femaData.Mpr + femaData.Vp * femaData.sh); // fig. 3-4
			femaData.Vg = femaData.Wg * (femaData.L - column.Shape.d) / 2 + femaData.Pg * (femaData.L - column.Shape.d - femaData.Lp) / (femaData.L - column.Shape.d);
			Vf = 2 * femaData.Mf / (femaData.L - column.Shape.d) + femaData.Vg; // Eq. 3-52
			// Step 2, 3
			pf = ConstNum.TWO_INCHES;
			Pb = beam.WinConnect.MomentEndPlate.Bolt.SpacingLongDir;
			d0 = (beam.Shape.d + pf - beam.Shape.tf / 2); // Eq.3-36
			d1 = beam.Shape.d - 1.5 * beam.Shape.tf - pf - Pb;
			Ffu = femaData.Mf / (beam.Shape.d - beam.Shape.tf); // eq 3-36
			switch (beam.WinConnect.MomentEndPlate.Bolt.ASTMType)
			{
				case EBoltASTM.A325:
					Abolt = femaData.Mf / (3.4 * ConstNum.COEFFICIENT_90 * (d0 + d1)); // eq 3-31
					// AboltMin = TubMin / (0.75 * katsayi90n)
					break;
				case EBoltASTM.A490:
					// AboltMin = TubMin / (0.75 * katsayi113n)
					Abolt = femaData.Mf / (3.4 * ConstNum.COEFFICIENT_113 * (d0 + d1));
					break;
			}
			AboltShear = (2 * femaData.Mf / (femaData.L - dc) + femaData.Pg) / (6 * beam.WinConnect.MomentEndPlate.Bolt.BoltStrength); // eq 3-33
			Ab = Math.Max(Abolt, AboltShear);
			Bolt_d = Math.Pow(4 * Ab / Math.PI, 0.5) / 8; // Int(-8 * (4 * Ab / pi) ^ 0.5) / 8
			if (Bolt_d > beam.WinConnect.MomentEndPlate.Bolt.BoltSize)
				beam.WinConnect.MomentEndPlate.Bolt.BoltSize = Bolt_d;
			else
				Bolt_d = beam.WinConnect.MomentEndPlate.Bolt.BoltSize;

			// MomentEndPlateBolts.Fv = MomentEndPlateBolts.Fv * (Bolt_d / MomentEndPlateBolts.d) ^ 2
			beam.WinConnect.MomentEndPlate.Bolt.Pretension = beam.WinConnect.MomentEndPlate.Bolt.Pretension * Math.Pow(Bolt_d / beam.WinConnect.MomentEndPlate.Bolt.BoltSize, 2);
			beam.WinConnect.MomentEndPlate.Bolt.BoltSize = Bolt_d;

			// redo hole size etc....
			g = column.GageOnFlange;
			Bp = beam.Shape.bf + ConstNum.ONE_INCH;
			ts = beam.Shape.tw;
			dbt = beam.WinConnect.MomentEndPlate.Bolt.BoltSize;
			if (CommonDataStatic.Units == EUnit.US)
			{
				tpMin1 = 0.00609 * Math.Pow(pf, 0.9) * Math.Pow(g, 0.6) * Math.Pow(Ffu, 0.9) / (Math.Pow(dbt, 0.9) * Math.Pow(ts, 0.1) * Math.Pow(Bp, 0.7)); // eq 3-34
				tpMin2 = 0.00413 * Math.Pow(pf, 0.25) * Math.Pow(g, 0.15) * Ffu / (Math.Pow(dbt, 0.7) * Math.Pow(ts, 0.15) * Math.Pow(Bp, 0.3)); // eq 3-35
			}
			else
			{
				tpMin1 = 0.010905 * 0.00609 * Math.Pow(pf, 0.9) * Math.Pow(g, 0.6) * Math.Pow(Ffu, 0.9) / (Math.Pow(dbt, 0.9) * Math.Pow(ts, 0.1) * Math.Pow(Bp, 0.7)); // eq 3-34
				tpMin2 = 0.064609 * 0.00413 * Math.Pow(pf, 0.25) * Math.Pow(g, 0.15) * Ffu / (Math.Pow(dbt, 0.7) * Math.Pow(ts, 0.15) * Math.Pow(Bp, 0.3)); // eq 3-35
			}
			tp = CommonCalculations.PlateThickness(Math.Max(tpMin1, tpMin2));
			Ab = Math.PI * Math.Pow(dbt, 2) / 4;
			switch (beam.WinConnect.MomentEndPlate.Bolt.ASTMType)
			{
				case EBoltASTM.A325:
					Tub = ConstNum.COEFFICIENT_90 * Ab;
					break;
				case EBoltASTM.A490:
					Tub = ConstNum.COEFFICIENT_113 * Ab;
					break;
			}
			do
			{
				if (CommonDataStatic.Units == EUnit.US)
					TubMin = 0.00002305 * Math.Pow(pf, 0.591) * Math.Pow(Ffu, 2.583) / (Math.Pow(tp, 0.895) * Math.Pow(dbt, 1.909) * Math.Pow(ts, 0.327) * Math.Pow(Bp, 0.965)) + beam.WinConnect.MomentEndPlate.Bolt.Pretension; // eq 3-32
				else
					TubMin = 0.1409 * 0.00002305 * Math.Pow(pf, 0.591) * Math.Pow(Ffu, 2.583) / (Math.Pow(tp, 0.895) * Math.Pow(dbt, 1.909) * Math.Pow(ts, 0.327) * Math.Pow(Bp, 0.965)) + beam.WinConnect.MomentEndPlate.Bolt.Pretension; // eq 3-32
				if (Tub < TubMin)
					tp = CommonCalculations.PlateThickness(tp + ConstNum.SIXTEENTH_INCH);
				else
					break;
			} while (true);

			if (!beam.WinConnect.MomentEndPlate.Thickness_User)
				beam.WinConnect.MomentEndPlate.Thickness = CommonCalculations.PlateThickness(tp);

			w_web = Vf / (2 * (beam.Shape.d - 2 * beam.Shape.kdes) * 0.707 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
			minweld = CommonCalculations.MinimumWeld(beam.Shape.tw, beam.WinConnect.MomentEndPlate.Thickness);
			wmax_beamweb = beam.Material.Fu * beam.Shape.tw / (2 * 0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
			w = Math.Max(w_web, minweld);
			if (wmax_beamweb > w)
				w = wmax_beamweb;

			if (!beam.WinConnect.MomentEndPlate.WebWeldRestSize_User)
				beam.WinConnect.MomentEndPlate.WebWeldRestSize = w / 16;
			if (!beam.WinConnect.MomentEndPlate.Width_User)
				beam.WinConnect.MomentEndPlate.Width = Bp / 4;
			if (!beam.WinConnect.MomentEndPlate.Length_User)
				beam.WinConnect.MomentEndPlate.Length = beam.Shape.d + 2 * (pf + Pb + beam.WinConnect.MomentEndPlate.Bolt.EdgeDistLongDir) / 4;
			if (!beam.WinConnect.MomentEndPlate.DistanceToFirstBolt_User)
				beam.WinConnect.MomentEndPlate.DistanceToFirstBolt = pf;
			if (!beam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts_User)
				beam.WinConnect.MomentEndPlate.Bolt.NumberOfBolts = 8;

			beam.WinConnect.MomentEndPlate.FlangeWeldType = EWeldType.CJP;
			if (!beam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir_User)
				beam.WinConnect.MomentEndPlate.Bolt.SpacingTransvDir = Pb;
			if (!beam.EndSetback_User)
				beam.EndSetback = beam.WinConnect.MomentEndPlate.Thickness;

			// Stiffeners:
			StiffenerHeight = (beam.WinConnect.MomentEndPlate.Length - beam.Shape.d) / 2.0 / 8.0;
			beam.WinConnect.MomentEndPlate.BraceStiffener.Length = (StiffenerHeight - ConstNum.ONE_INCH) / 0.57735 + ConstNum.ONE_INCH / 8;
			beam.WinConnect.MomentEndPlate.BraceStiffener.Thickness = CommonCalculations.PlateThickness(beam.Shape.tw);

			femaData.shf = shf;
			femaData.Ryc = Ryc;
			femaData.Mc = Mc;
			femaData.c = c;
			femaData.Myf = Myf;
			femaData.Vf = Vf;
			femaData.BeamMaterialSpec = BeamMaterialSpec;
			femaData.ColumnMaterialSpec = ColumnMaterialSpec;
			femaData.ColumnDepth = ColumnDepth;
			femaData.MaximumBeamDepth = MaximumBeamDepth;
			femaData.MaximumBeamFlangeThickness = MaximumFlangeThickness;
			femaData.MinimumSpanToDepthRatio = MinimumSpanToDepthRatio;
		}

		private static void BoltedFlangePlateConnection(EMemberType memberType)
		{
			double Tcp = 0;
			double Tcf_min = 0;
			double tpz = 0;
			double Vweb = 0;
			double Mfail = 0;
			double Tn = 0;
			double Mfail_bm = 0;
			double Ltf3 = 0;
			double Mfail_fp = 0;
			double Ltf2 = 0;
			double Mfail_Bolts = 0;
			double LengthPrev = 0;
			double S4 = 0;
			double s2 = 0;
			double s1Min = 0;
			double n = 0;
			double S3 = 0;
			double s1 = 0;
			double Ltf1 = 0;
			double tpl = 0;
			double Vf = 0;
			double shf = 0;
			double c = 0;
			double Ae = 0;
			double An = 0;
			double Ag = 0;
			double Bp = 0;
			double db = 0;
			string ColumnDepth = "";
			double MaximumFlangeThickness = 0;
			int MinimumSpanToDepthRatio = 0;
			double Length;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var femaData = beam.WinConnect.Fema;

			// Section 3.6.3
			if (CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.OMF || CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.SMF)
				return;

			// L= center-center length of the beam
			switch (CommonDataStatic.SeismicSettings.FramingType)
			{
				case EFramingSystem.OMF:
					MinimumSpanToDepthRatio = 5;
					MaximumFlangeThickness = 1.25 * ConstNum.ONE_INCH;
					ColumnDepth = "ALL";
					break;
				case EFramingSystem.SMF:
					MinimumSpanToDepthRatio = 8;
					MaximumFlangeThickness = 0.75 * ConstNum.ONE_INCH;
					ColumnDepth = "W12 W14 W310 W360";
					break;
			}

			db = beam.Shape.d;
			// Step 1,2
			Length = beam.Shape.d / 2 / 4;
			Bp = Math.Min((int)Math.Floor(4 * column.Shape.bf), beam.Shape.bf / 4);

			Ag = beam.Shape.tf * beam.Shape.bf;
			An = beam.Shape.tf * (beam.Shape.bf - beam.WinConnect.MomentFlangePlate.Bolt.NumberOfLines * (beam.WinConnect.MomentFlangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH));
			Ae = 0.75 * beam.Material.Fu * An / (0.9 * beam.Material.Fy);
			if (Ae >= Ag)
				Ae = Ag;

			c = beam.WinConnect.MomentFlangePlate.Bolt.NumberOfLines * (beam.WinConnect.MomentFlangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
			femaData.Ze = beam.Shape.zx - (Ag - Ae) * (beam.Shape.d - beam.Shape.tf);
			femaData.Se = beam.Shape.sx - c * (beam.Shape.tf * beam.Shape.d - 2 * Math.Pow(beam.Shape.tf, 2));

			femaData.Cpr = (beam.Material.Fy + beam.Material.Fu) / (2 * beam.Material.Fy); // eq. 3.2
			femaData.Ry = MiscCalculationDataMethods.ExpYieldStr(beam.Material.Fy);
			femaData.Ryc = MiscCalculationDataMethods.ExpYieldStr(column.Material.Fy);
			femaData.Mpr = femaData.Cpr * femaData.Ry * femaData.Ze * beam.Material.Fy; // eq. 3.1
			femaData.Cy = femaData.Se / (femaData.Cpr * femaData.Ze); // eq.3.4
			do
			{
				femaData.sh = (column.Shape.d / 2 + Length);
				femaData.Lp_h = (femaData.Lp - Length);
				shf = femaData.shf == -100 ? femaData.sh : femaData.shf;

				femaData.Mc = femaData.Mpr + femaData.Vp * femaData.sh; // fig. 3-4
				femaData.L1 = (femaData.L - femaData.sh - shf); // =  L'
				femaData.Vp = ((2 * femaData.Mpr + femaData.Pg * (femaData.L1 - femaData.Lp_h) + femaData.Wg * Math.Pow(femaData.L1, 2) / 2) / femaData.L1); // fig. 3-3
				femaData.Mf = ((int)(femaData.Mpr + femaData.Vp * Length));
				femaData.Vg = femaData.Wg * (femaData.L - column.Shape.d) / 2 + femaData.Pg * (femaData.L - column.Shape.d - femaData.Lp) / (femaData.L - column.Shape.d);
				Vf = (2 * femaData.Mf / (femaData.L - column.Shape.d) + femaData.Vg); // Eq. 3-52

				femaData.Myf = femaData.Cy * femaData.Mf; // eq.3.3
				// Step 5
				tpl = (-db + Math.Pow(Math.Pow(db, 2) + 4.4 * femaData.Myf / (beam.WinConnect.MomentFlangePlate.Material.Fy * Bp), 0.5)) / 2;
				// Step 6
				Ltf1 = (femaData.L - column.Shape.d) / (femaData.L - column.Shape.d - (2 * s1 + S3)); // eq 3-44
				n = -((int)Math.Floor(-1.2 * femaData.Myf / (2 * beam.Shape.d * beam.WinConnect.MomentFlangePlate.Bolt.BoltStrength * Math.Pow(beam.WinConnect.MomentFlangePlate.Bolt.BoltSize, 2) * Math.PI / 4 * Ltf1)));
				s1 = beam.EndSetback + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange;
				s1Min = tpl + beam.WinConnect.MomentFlangePlate.Bolt.BoltSize + ConstNum.HALF_INCH;
				if (!beam.WinConnect.Beam.BoltEdgeDistanceOnFlange_User && s1 < s1Min)
				{
					s1 = s1Min;
					beam.WinConnect.Beam.BoltEdgeDistanceOnFlange = s1 - beam.EndSetback;
				}
				s2 = beam.WinConnect.MomentFlangePlate.Bolt.SpacingLongDir;
				S3 = (n - 1) * s2;
				S4 = beam.WinConnect.MomentFlangePlate.Bolt.EdgeDistLongDir;
				LengthPrev = Length;
				Length = s1 + S3 + S4;
			} while (LengthPrev != Length);

			beam.WinConnect.MomentFlangePlate.Bolt.NumberOfRows = (int)n;

			// step 7
			Mfail_Bolts = 2 * n * beam.WinConnect.MomentFlangePlate.Bolt.BoltStrength * Math.Pow(beam.WinConnect.MomentFlangePlate.Bolt.BoltSize, 2) * Math.PI / 4 * beam.Shape.d * Ltf1; // eq 3-43

			// Step 8
			Ltf2 = ((femaData.L - column.Shape.d) / (femaData.L - column.Shape.d - 2 * s1)); // eq 3-46
			do
			{
				Mfail_fp = (0.85 * beam.WinConnect.MomentFlangePlate.Material.Fu * (Bp - 2 * (beam.WinConnect.MomentFlangePlate.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * tpl * (beam.Shape.d + tpl) * Ltf2); // eq 3-45

				if (Mfail_fp > 1.2 * femaData.Myf)
					break; // Plate net section fracture OK
				else
					tpl = tpl + ConstNum.SIXTEENTH_INCH;
			} while (true);

			// Step 9
			Ltf3 = (femaData.L - column.Shape.d) / (femaData.L - column.Shape.d - 2 * (s1 + S3)); // eq 3-48
			Mfail_bm = beam.Material.Fu * (beam.Shape.zx - 2 * (beam.WinConnect.MomentFlangePlate.Bolt.BoltSize + 2 * ConstNum.SIXTEENTH_INCH) * beam.Shape.tf * (beam.Shape.d - beam.Shape.tf)) * Ltf3; // eq 3-47

			// Step 10
			Tn = Math.Min(2.4F * beam.Material.Fu * (S3 + s1 - c) * beam.Shape.tf, 2.4F * beam.WinConnect.MomentFlangePlate.Material.Fu * (S3 + S4) * tpl); // eq.3-50 3-51
			Mfail = Tn * (beam.Shape.d + tpl) * Ltf1; // eq 3-49

			// Step 11
			// check block shear and pull-through, fig 3-19
			// Step 12 - Single plate design, bolted shear tab
			femaData.Vg = femaData.Wg * (femaData.L - column.Shape.d) / 2 + femaData.Pg * (femaData.L - column.Shape.d - femaData.Lp) / (femaData.L - column.Shape.d);

			Vweb = (2 * femaData.Mf / (femaData.L - column.Shape.d) + femaData.Vg); // Eq. 3-52

			beam.WinConnect.MomentFlangePlate.TopLength = Length;
			beam.WinConnect.MomentFlangePlate.TopWidth = Bp;
			beam.WinConnect.MomentFlangePlate.TopThickness = CommonCalculations.PlateThickness(tpl);
			beam.WinConnect.MomentFlangePlate.PlateToSupportWeldType = EWeldType.CJP;
			beam.WinConnect.MomentFlangePlate.Bolt.NumberOfRows = (int)n;
			beam.WinConnect.MomentFlangePlate.Bolt.NumberOfBolts = beam.WinConnect.MomentFlangePlate.Bolt.NumberOfRows * beam.WinConnect.MomentFlangePlate.Bolt.NumberOfLines;
			beam.WinConnect.Beam.TopAttachThick = beam.WinConnect.MomentFlangePlate.TopThickness;
			beam.WinConnect.Beam.BotAttachThick = beam.WinConnect.MomentFlangePlate.TopThickness;
			if (!beam.EndSetback_User && beam.EndSetback < beam.WinConnect.MomentFlangePlate.TopThickness / 2)
				beam.EndSetback = beam.WinConnect.MomentFlangePlate.TopThickness / 2;
			if (!beam.WinConnect.Beam.BoltEdgeDistanceOnFlange_User && beam.WinConnect.Beam.BoltEdgeDistanceOnFlange < beam.EndSetback + beam.WinConnect.MomentFlangePlate.TopThickness)
				beam.WinConnect.Beam.BoltEdgeDistanceOnFlange = beam.EndSetback + beam.WinConnect.MomentFlangePlate.TopThickness;

			// Panel zone thickness for simultaneous yielding of column web and beam
			// H = average of story heights above and below the joint
			tpz = femaData.Cy * femaData.Mc * (femaData.H - beam.Shape.d) / (femaData.H * 0.9 * 0.6 * column.Material.Fy * femaData.Ryc * column.Shape.d * (beam.Shape.d - beam.Shape.tf)); //  eq.3-7

			// Continuity Plates:
			Tcf_min = Math.Max(0.4 * Math.Pow(1.8 * beam.WinConnect.MomentFlangePlate.TopWidth * beam.Shape.tf * beam.Material.Fy * femaData.Ry / (column.Material.Fy * femaData.Ryc), 0.5), beam.Shape.bf / 6); // eq. 3-5, 3-6
			if (Tcf_min > column.Shape.tf)
			{
				// continuity plate thickness NOTE: Continuity plates to be welded to column flange by CJP
				//                                                             and welded to web to develop the shear strength of net length of plate
				if (beam.MemberType == EMemberType.RightBeam && leftBeam.MomentConnection != EMomentCarriedBy.NoMoment || beam.MemberType == EMemberType.LeftBeam && rightBeam.MomentConnection != EMomentCarriedBy.NoMoment)
					Tcp = Math.Max(rightBeam.Shape.tf, leftBeam.Shape.tf);
				else if (beam.MemberType == EMemberType.RightBeam)
					Tcp = rightBeam.Shape.tf / 2;
				else if (beam.MemberType == EMemberType.LeftBeam)
					Tcp = leftBeam.Shape.tf / 2;
				Tcp = Math.Max(Tcp, CommonDataStatic.ColumnStiffener.SThickness);
			}

			femaData.shf = shf;
			femaData.c = c;
			femaData.Vf = Vweb;
			femaData.ColumnDepth = ColumnDepth;
			femaData.MaximumBeamFlangeThickness = MaximumFlangeThickness;
			femaData.MinimumSpanToDepthRatio = MinimumSpanToDepthRatio;
		}

		private static void DoubleSplitTeeConnection(EMemberType memberType)
		{
			double Tcp = 0;
			double Tcf_min = 0;
			double tpz = 0;
			double tfc_req = 0;
			double Mfail_BeamFlange = 0;
			double Ltf3 = 0;
			bool loopAgain = false;
			double Mfail_bolttension = 0;
			double Tub = 0;
			double Mfail_tflange = 0;
			double Bp = 0;
			double ap = 0;
			double g1 = 0;
			double Mfail_stemfracture = 0;
			double w = 0;
			double w2 = 0;
			double w1 = 0;
			double Thetaeff = 0;
			double Ltf2 = 0;
			double Mfail_Bolts = 0;
			double Ltf1 = 0;
			double S4 = 0;
			double S3 = 0;
			double s2 = 0;
			double s1 = 0;
			double n = 0;
			double Vst = 0;
			double shf = 0;
			double MaxTeeFlangeWidth = 0;
			double EstimatedLength = 0;
			double Ae = 0;
			double An = 0;
			double Ag = 0;
			double MinStemLength = 0;
			double MaxStemLength = 0;
			double MinFlangeLength = 0;
			double MaxFlangeLength = 0;
			string ColumnDepth = "";
			double MaximumFlangeThickness = 0;
			int MinimumSpanToDepthRatio = 0;
			string MaximumBeamDepth = "";
			double Length;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var femaData = beam.WinConnect.Fema;

			// Section 3.7.4
			if (CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.OMF || CommonDataStatic.SeismicSettings.FramingType != EFramingSystem.SMF)
				return;

			// L= center-center length of the beam
			switch (CommonDataStatic.SeismicSettings.FramingType)
			{
				case EFramingSystem.OMF:
					MaximumBeamDepth = "W36 W920";
					MinimumSpanToDepthRatio = 5;
					MaximumFlangeThickness = 1.25 * ConstNum.ONE_INCH;
					ColumnDepth = "ALL"; // all
					break;
				case EFramingSystem.SMF:
					MaximumBeamDepth = "W24 W610";
					MinimumSpanToDepthRatio = 8;
					MaximumFlangeThickness = 0.75 * ConstNum.ONE_INCH;
					ColumnDepth = "W12 W14 W310 W360";
					break;
			}

			MaxFlangeLength = Math.Max(beam.Shape.bf, column.Shape.bf);
			MinFlangeLength = column.GageOnFlange + 2 * beam.WinConnect.MomentTee.BoltColumnFlange.EdgeDistLongDir;
			MaxStemLength = beam.Shape.bf;
			MinStemLength = beam.GageOnFlange + 2 * beam.WinConnect.MomentTee.BoltBeamStem.EdgeDistLongDir;

			Ag = beam.Shape.tf * beam.Shape.bf;
			An = beam.Shape.tf * (beam.Shape.bf - beam.WinConnect.MomentTee.BoltBeamStem.NumberOfLines * (beam.WinConnect.MomentFlangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH));
			Ae = 0.75 * beam.Material.Fu * An / (0.9 * beam.Material.Fy);
			if (Ae >= Ag)
				Ae = Ag;

			femaData.Ze = beam.Shape.zx - (Ag - Ae) * (beam.Shape.d - beam.Shape.tf);
			femaData.c = beam.WinConnect.MomentTee.BoltBeamStem.NumberOfLines * (beam.WinConnect.MomentFlangePlate.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
			femaData.Se = beam.Shape.sx - femaData.c * (beam.Shape.tf * beam.Shape.d - 2 * Math.Pow(beam.Shape.tf, 2));

			femaData.Cpr = (beam.Material.Fy + beam.Material.Fu) / (2 * beam.Material.Fy); // eq. 3.2
			femaData.Ry = MiscCalculationDataMethods.ExpYieldStr(beam.Material.Fy);
			femaData.Ryc = MiscCalculationDataMethods.ExpYieldStr(column.Material.Fy);
			femaData.Mpr = femaData.Cpr * femaData.Ry * femaData.Ze * beam.Material.Fy; // eq. 3.1
			femaData.Mc = femaData.Mpr + femaData.Vp * femaData.sh; // fig. 3-4
			femaData.Cy = femaData.Se / (femaData.Cpr * femaData.Ze); //  eq.3.4
			EstimatedLength = 7.5 * ConstNum.ONE_INCH + beam.EndSetback + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange;
			MaxTeeFlangeWidth = 2 * (beam.WinConnect.MomentTee.Column_a + beam.WinConnect.MomentTee.BoltColumnFlange.EdgeDistTransvDir); //  plus Tee.tw
			beam.WinConnect.MomentTee.TopLengthAtStem = (MaxStemLength + MinStemLength) / 2;
			beam.WinConnect.MomentTee.TopLengthAtFlange = (MaxFlangeLength + MinFlangeLength) / 2;
			if (!beam.WinConnect.MomentTee.TopTeeShape_User)
			{
				do
				{
					Shape tee;
					//   will repeat if tee.flength changed
					foreach (var shape in CommonDataStatic.AllShapes.Where(s => s.Value.TypeEnum == EShapeType.WTSection))
					{
						tee = shape.Value;

						Length = tee.d;

						femaData.sh = (column.Shape.d / 2 + Length);
						if (femaData.shf == -100)
							shf = femaData.sh;
						else
							shf = femaData.shf;
						femaData.L1 = (femaData.L - femaData.sh - shf); // =  L'
						femaData.Pg = femaData.Pg;
						femaData.Lp_h = (femaData.Lp - Length);
						femaData.Wg = femaData.Wg;
						femaData.Vp = ((2 * femaData.Mpr + femaData.Pg * (femaData.L1 - femaData.Lp_h) + femaData.Wg * Math.Pow(femaData.L1, 2) / 2) / femaData.L1); // fig. 3-3
						femaData.Vg = femaData.Wg * (femaData.L - column.Shape.d) / 2 + femaData.Pg * (femaData.L - column.Shape.d - femaData.Lp) / (femaData.L - column.Shape.d);
						femaData.Mf = ((int)(femaData.Mpr + femaData.Vp * Length));
						femaData.Vg = femaData.Wg * (femaData.L - column.Shape.d) / 2 + femaData.Pg * (femaData.L - column.Shape.d - femaData.Lp) / (femaData.L - column.Shape.d);
						Vst = (2 * femaData.Mf / (femaData.L - column.Shape.d) + femaData.Vg); // eq 3-69

						femaData.Myf = femaData.Cy * femaData.Mf; // eq.3.3
						// Step 3 : Check panel zone
						// Step 4
						n = ((int)Math.Ceiling(0.5 * 1.2 * femaData.Myf / (2 * beam.Shape.d * beam.WinConnect.MomentTee.BoltBeamStem.BoltStrength * Math.Pow(beam.WinConnect.MomentTee.BoltBeamStem.BoltSize, 2) * Math.PI / 4))) / 0.5;

						if (!beam.EndSetback_User)
							beam.EndSetback = tee.tf;
						s1 = beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + beam.EndSetback;
						s2 = beam.WinConnect.MomentTee.BoltBeamStem.SpacingLongDir;
						S3 = (n - 1) * s2;
						S4 = tee.d - s1 - S3;
						if (S4 < beam.WinConnect.MomentTee.BoltBeamStem.EdgeDistLongDir)
							continue;
						EstimatedLength = s1 + S3 + S4;
						if (EstimatedLength > tee.d)
							continue;
						// step 5
						Ltf1 = (femaData.L - column.Shape.d) / (femaData.L - column.Shape.d - (2 * s1 + S3)); // eq 3-56
						Mfail_Bolts = 2 * n * beam.WinConnect.MomentTee.BoltBeamStem.BoltStrength * Math.Pow(beam.WinConnect.MomentTee.BoltBeamStem.BoltSize, 2) * Math.PI / 4 * beam.Shape.d * Ltf1; // eq 3-55
						if (Mfail_Bolts <= 1.2 * femaData.Myf)
							continue;
						// Step 6
						Ltf2 = ((femaData.L - column.Shape.d) / (femaData.L - column.Shape.d - 2 * s1)); // eq 3-60
						Thetaeff = Math.Min(60 * tee.tw / ConstNum.ONE_INCH, 30);
						Thetaeff = Math.Max(Thetaeff, 15);
						w1 = (beam.WinConnect.MomentTee.TopLengthAtFlange - beam.WinConnect.MomentTee.TopLengthAtStem) / (tee.d - tee.tf) * (S4 + S3) + beam.WinConnect.MomentTee.TopLengthAtStem;
						w2 = beam.GageOnFlange + S3 * Math.Tan(Thetaeff * Math.PI / 180);
						w = Math.Min(beam.WinConnect.MomentTee.TopLengthAtFlange, Math.Min(w1, w2)); // eq. 3-58
						Mfail_stemfracture = (beam.WinConnect.MomentTee.TopMaterial.Fu * (w - 2 * (beam.WinConnect.MomentTee.BoltBeamStem.BoltSize + ConstNum.EIGHTH_INCH)) * tee.tw * (beam.Shape.d + tee.tw) * Ltf2); // eq 3-57
						if (Mfail_stemfracture <= 1.2 * femaData.Myf)
							continue;

						// Step 7
						beam.WinConnect.MomentTee.Column_a = (beam.GageOnFlange - tee.tw) / 2;

						g1 = 2 * beam.WinConnect.MomentTee.Column_a + tee.tw;
						ap = (tee.bf - g1) / 2 + beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize / 2; // eq 3-62
						Bp = (g1 - tee.tw) / 2 - (tee.k1 - tee.tw / 2) / 2 - beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize / 2; // eq 3-63
						Mfail_tflange = beam.WinConnect.MomentTee.TopMaterial.Fy * w * Math.Pow(tee.tf, 2) * (2 * ap - beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize / 4) * (beam.Shape.d - beam.WinConnect.MomentTee.TopTeeShape.tw) / (4 * ap * Bp - beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize * (ap + Bp)); // eq 3-61
						if (Mfail_tflange <= 1.2 * femaData.Myf)
							continue;

						beam.WinConnect.MomentTee.TopTeeShape = tee;
						break;
					}

					if (beam.WinConnect.MomentTee.TopTeeShape.bf > MaxTeeFlangeWidth + beam.WinConnect.MomentTee.TopTeeShape.tw)
						beam.WinConnect.MomentTee.TopTeeShape.bf = MaxTeeFlangeWidth + beam.WinConnect.MomentTee.TopTeeShape.tw;

					beam.WinConnect.MomentTee.BoltBeamStem.NumberOfLines = 2;
					beam.WinConnect.MomentTee.BoltColumnFlange.NumberOfBolts = 4;
					beam.WinConnect.MomentTee.Beam_g = beam.GageOnFlange;
					beam.WinConnect.MomentTee.Beam_g1 = beam.WinConnect.MomentTee.BoltBeamStem.SpacingTransvDir;
					beam.WinConnect.MomentTee.Column_g = column.GageOnFlange;
					beam.WinConnect.MomentTee.Column_g1 = beam.WinConnect.MomentTee.BoltColumnFlange.SpacingTransvDir;
					beam.WinConnect.MomentTee.BoltBeamStem.EdgeDistTransvDir = (beam.Shape.bf - beam.GageOnFlange) / 2;
					beam.WinConnect.MomentTee.Column_a = (beam.WinConnect.MomentTee.TopTeeShape.g1 - beam.WinConnect.MomentTee.TopTeeShape.tf) / 2;
					beam.WinConnect.MomentTee.Column_e = (beam.WinConnect.MomentTee.TopTeeShape.bf - beam.WinConnect.MomentTee.TopTeeShape.g1) / 2;

					// Step 8
					switch (beam.WinConnect.MomentTee.BoltColumnFlange.ASTMType)
					{
						case EBoltASTM.A325:
							Tub = ConstNum.COEFFICIENT_90 * Math.PI * Math.Pow(beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize, 2) / 4;
							break;
						case EBoltASTM.A490:
							Tub = ConstNum.COEFFICIENT_113 * Math.PI * Math.Pow(beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize, 2) / 4;
							break;
					}
					Mfail_bolttension = beam.WinConnect.MomentTee.BoltColumnFlange.NumberOfBolts * (beam.WinConnect.MomentTee.BoltColumnFlange.BoltSize + beam.WinConnect.MomentTee.TopTeeShape.tw) * (Tub + w * beam.WinConnect.MomentTee.TopMaterial.Fy * Math.Pow(beam.WinConnect.MomentTee.TopTeeShape.tf, 2) / (16 * ap)) * ap / (ap + Bp); // eq 3-64
					loopAgain = false;
					if (Mfail_bolttension <= 1.2 * femaData.Myf)
					{
						if (beam.WinConnect.MomentTee.TopLengthAtFlange >= beam.WinConnect.MomentTee.Column_g + 2 * (beam.WinConnect.MomentTee.Column_g1 + beam.WinConnect.MomentTee.BoltColumnFlange.EdgeDistTransvDir))
							beam.WinConnect.MomentTee.BoltColumnFlange.NumberOfBolts = 8;
						else if (column.Shape.bf >= beam.WinConnect.MomentTee.Column_g + 2 * (beam.WinConnect.MomentTee.Column_g1 + beam.WinConnect.MomentTee.BoltColumnFlange.EdgeDistTransvDir))
						{
							beam.WinConnect.MomentTee.BoltColumnFlange.NumberOfBolts = 8;
							beam.WinConnect.MomentTee.TopLengthAtFlange = column.Shape.bf;
							loopAgain = true;
						}
					}
				} while (loopAgain);
			}

			beam.WinConnect.Beam.TopAttachThick = beam.WinConnect.MomentTee.TopTeeShape.tw;
			beam.WinConnect.Beam.BotAttachThick = beam.WinConnect.MomentTee.TopTeeShape.tw;

			// Step 9
			Ltf3 = (femaData.L - column.Shape.d) / (femaData.L - column.Shape.d - 2 * (s1 + S3)); // eq 3-66
			Mfail_BeamFlange = beam.Material.Fu * (beam.Shape.zx - 2 * (beam.WinConnect.MomentTee.BoltBeamStem.BoltSize + ConstNum.EIGHTH_INCH) * beam.Shape.tf * (beam.Shape.d - beam.Shape.tf)) * Ltf3; // eq 3-65

			// Step 10 - Block shear of Tee Stem
			// Step 11

			// Step 12
			tfc_req = femaData.Mf / ((beam.Shape.d - beam.WinConnect.MomentTee.TopTeeShape.tw) * (6 * column.Shape.kdes) * column.Material.Fy); // eq 3-68

			// Step 13
			// If continuity plates required then
			// Column.tf>=Tee.tf
			// Step 14

			// Panel zone thickness for simultaneous yielding of column web and beam
			// H = average of story heights above and below the joint
			tpz = femaData.Cy * femaData.Mc * (femaData.H - beam.Shape.d) / (femaData.H * 0.9 * 0.6 * column.Material.Fy * femaData.Ryc * column.Shape.d * (beam.Shape.d - beam.Shape.tf)); //  eq.3-7

			// Continuity Plates:
			Tcf_min = Math.Max(0.4 * Math.Pow(1.8 * beam.WinConnect.MomentFlangePlate.TopWidth * beam.Shape.tf * beam.Material.Fy * femaData.Ry / (column.Material.Fy * femaData.Ryc), 0.5), beam.Shape.bf / 6); // eq. 3-5, 3-6
			if (Tcf_min > column.Shape.tf)
			{
				// continuity plate thickness NOTE: Continuity plates to be welded to column flange by CJP
				//                                                             and welded to web to develop the shear strength of net length of plate
				if (beam.MemberType == EMemberType.RightBeam && leftBeam.MomentConnection != EMomentCarriedBy.NoMoment || beam.MemberType == EMemberType.LeftBeam && rightBeam.MomentConnection != EMomentCarriedBy.NoMoment)
					Tcp = Math.Max(rightBeam.Shape.tf, leftBeam.Shape.tf);
				else if (beam.MemberType == EMemberType.RightBeam)
					Tcp = rightBeam.Shape.tf / 2;
				else if (beam.MemberType == EMemberType.LeftBeam)
					Tcp = leftBeam.Shape.tf / 2;
				Tcp = Math.Max(Tcp, CommonDataStatic.ColumnStiffener.SThickness);
			}

			femaData.shf = shf;
			femaData.Vf = Vst;
			femaData.ColumnDepth = ColumnDepth;
			femaData.MaximumBeamDepth = MaximumBeamDepth;
			femaData.MaximumBeamFlangeThickness = MaximumFlangeThickness;
			femaData.MinimumSpanToDepthRatio = MinimumSpanToDepthRatio;
		}

		public static void TrapezoidalWeld(EMemberType memberType, double k, double k1, double k2, double ex, int theta, double L, ref double Coeff)
		{
			double NumberOfElementsVL = 0;
			double nl = 0;
			double nkL = 0;
			double inc2 = 0;
			double inc1 = 0;
			double NumberOfElements = 0;
			double y4 = 0;
			double X4 = 0;
			double y3 = 0;
			double x3 = 0;
			double y2 = 0;
			double x2 = 0;
			double y1 = 0;
			double X1 = 0;
			double XLL = 0;
			double s = 0;
			double c = 0;
			double k2L = 0;
			double kk1L = 0;
			double kk1 = 0;
			double k1L = 0;
			double kL = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var femaData = beam.WinConnect.Fema;

			kL = k * L;
			k1L = k1 * L;
			kk1 = Math.Sqrt(Math.Pow(k, 2) + Math.Pow(k1, 2));
			kk1L = kk1 * L;
			k2L = k2 * L;
			if (k == 0)
			{
				c = 0;
				s = 1;
			}
			else
			{
				c = kL / kk1L;
				s = k1L / kk1L;
			}

			XLL = (k * kk1 + k2) / (2 * (kk1 + 1 - k1)) * L;
			femaData.L1 = ((1 - 2 * k1) * L);
			X1 = kL - XLL;
			y1 = -L / 2;
			x2 = -XLL;
			y2 = ((-femaData.L1) / 2);
			x3 = -XLL;
			y3 = (femaData.L1 / 2);
			X4 = kL - XLL;
			y4 = L / 2;
			// weldelement coordinates
			if (k > 0)
			{
				NumberOfElements = 199;
				inc1 = (femaData.L1 + 2 * kk1L) / NumberOfElements;
				inc2 = inc1;
				nkL = ((int)Math.Floor(kk1L / inc1)) + 1;
				nl = ((int)Math.Floor(femaData.L1 / inc2));
				NumberOfElements = (nkL - 1 + nl + nkL - 1);
				inc1 = kk1L / (nkL - 1);
				if (femaData.L1 > 0)
					inc2 = femaData.L1 / nl;
				else
					inc2 = 0;
				for (int i = 1; i <= nkL; i++)
				{
					WeldX[i] = X1 - (i - 1) * inc1 * c;
					Weldy[i] = y1 + (i - 1) * inc1 * s;
				}
				for (int i = (int)(nkL + 1); i <= (nkL + nl); i++)
				{
					WeldX[i] = x2;
					Weldy[i] = y2 + (i - nkL) * inc2;
				}
				for (int i = (int)(nkL + nl + 1); i <= (nkL + nl + nkL - 1); i++)
				{
					WeldX[i] = x3 + (i - nkL - nl) * inc1 * c;
					Weldy[i] = y3 + (i - nkL - nl) * inc1 * s;
				}
			}
			else
			{
				NumberOfElements = 100;
				inc2 = L / NumberOfElements;
				nl = NumberOfElements;
				for (int i = 1; i <= (nl + 1); i++)
				{
					WeldX[i] = 0;
					Weldy[i] = -L / 2 + (i - 1) * inc2;
				}
			}
			for (int j = 1; j <= NumberOfElements; j++)
			{
				WeldElementsJ1[j] = j;
				WeldElementsJ2[j] = j + 1;
			}

			// weldelement coordinates-vertical line
			NumberOfElementsVL = 100;
			inc2 = L / NumberOfElementsVL;
			nl = NumberOfElementsVL;

			for (int i = 200; i <= (199 + nl + 1); i++)
			{
				Weldy[i] = -L / 2 + (i - 199) * inc2;
				WeldX[i] = k2L - XLL;
				WeldElementsJ1[i] = i;
				WeldElementsJ2[i] = i + 1;
			}
			NumberOfElements = 300;
			Coeff = EccIncLoadOnWG(((int)k), k1, k2, ex, theta, L, NumberOfElements);
		}

		private static double EccIncLoadOnWG(int k, double k1, double k2, double ex, int theta, double L, double NumberOfElements)
		{
			double EccIncLoadOnWG = 0;
			double ro3 = 0;
			double ro2 = 0;
			double ro1 = 0;
			double SumRf = 0;
			double sumfy = 0;
			double sumfx = 0;
			double P = 0;
			double Deltai = 0;
			double deltaM = 0;
			int kcritical = 0;
			double du = 0;
			int k_forLrMax = 0;
			double ro = 0;
			int it = 0;
			double q2 = 0;
			double q1 = 0;
			int J2 = 0;
			int J1 = 0;
			double Cb = 0;
			double MinRot = 0;
			double LrMax = 0;
			double yG = 0;
			double y0 = 0;
			double xG = 0;
			double x0 = 0;
			double e = 0;
			double Moment = 0;
			double Moment1 = 0;
			double Moment2 = 0;
			double Moment3 = 0;
			double SumNormal = 0;
			double SumNormal1 = 0;
			double SumNormal2 = 0;
			double SumNormal3 = 0;
			double SumParallel = 0;
			double c = 0;
			double s = 0;
			double[] Xb = new double[302];
			double[] Yb = new double[302];
			double[] Lb = new double[302];
			double[] Rb = new double[302];
			double[] aci = new double[302];
			double[] Fb = new double[302];
			double[] Fxb = new double[302];
			double[] Fyb = new double[302];
			double[] DeltaU = new double[302];

			c = Math.Cos(theta);
			if (c < 0.00001)
				c = 0;
			if (c > 0.99999)
				c = 1;
			s = Math.Sin(theta);
			if (s < 0.00001)
				s = 0;
			if (s > 0.99999)
				s = 1;
			e = ex * c;
			if (e == 0)
			{
				x0 = -10000000 * c + xG;
				y0 = 10000000 * s + yG;
				LrMax = 0;
				MinRot = 100000;
				Cb = 0;
				for (k = 1; k <= NumberOfElements; k += 1)
				{
					J1 = (int)WeldElementsJ1[k];
					J2 = (int)WeldElementsJ2[k];

					Xb[k] = (WeldX[J1] + WeldX[J2]) / 2;
					Yb[k] = (Weldy[J1] + Weldy[J2]) / 2;
					Lb[k] = Math.Sqrt(Math.Pow(WeldX[J1] - WeldX[J2], 2) + Math.Pow(Weldy[J1] - Weldy[J2], 2));
					Rb[k] = Math.Sqrt(Math.Pow(Xb[k] - x0, 2) + Math.Pow(Yb[k] - y0, 2));


					q1 = (Weldy[J2] - Weldy[J1]) * (Yb[k] - y0) - (Xb[k] - x0) * (WeldX[J2] - WeldX[J1]);
					q2 = (WeldX[J2] - WeldX[J1]) * (Yb[k] - y0) - (Weldy[J2] - Weldy[J1]) * (Xb[k] - x0);
					if (q2 == 0)
						aci[k] = 90;
					else
						aci[k] = Math.Atan(Math.Abs(q1 / q2)) * 180 / Math.PI;
					if (180 - aci[k] < aci[k])
						aci[k] = 180 - aci[k];

					Fb[k] = 1.3921F * (1 + 0.5 * Math.Pow(Math.Sin(aci[k] * Math.PI / 180), 1.5)) * Lb[k];
					if (Rb[k] == 0)
					{
						Fxb[k] = 0;
						Fyb[k] = 0;
					}
					else
					{
						Fxb[k] = Fb[k] / Rb[k] * Math.Abs(Yb[k] - y0) * Math.Sign((y0 - Yb[k]));
						Fyb[k] = Fb[k] / Rb[k] * Math.Abs(Xb[k] - x0) * Math.Sign((Xb[k] - x0));
					}
					Cb = Cb + Fb[k];
				}
			}
			else
			{
				it = 0;
				ro = 0.000001;
				do
				{
					it = it + 1;
					x0 = -ro * c + xG;
					y0 = ro * s + yG;
					LrMax = 0;
					MinRot = 100000;
					for (k = 1; k <= NumberOfElements; k += 1)
					{
						J1 = (int)WeldElementsJ1[k];
						J2 = (int)WeldElementsJ2[k];
						Xb[k] = (WeldX[J1] + WeldX[J2]) / 2;
						Yb[k] = (Weldy[J1] + Weldy[J2]) / 2;
						Lb[k] = Math.Sqrt(Math.Pow(WeldX[J1] - WeldX[J2], 2) + Math.Pow(Weldy[J1] - Weldy[J2], 2));
						Rb[k] = Math.Sqrt(Math.Pow(Xb[k] - x0, 2) + Math.Pow(Yb[k] - y0, 2));
						Fb[k] = 1;
						q1 = (Weldy[J2] - Weldy[J1]) * (Yb[k] - y0) - (Xb[k] - x0) * (WeldX[J2] - WeldX[J1]);
						q2 = (WeldX[J2] - WeldX[J1]) * (Yb[k] - y0) - (Weldy[J2] - Weldy[J1]) * (Xb[k] - x0);
						if (q2 == 0)
							aci[k] = 90;
						else
							aci[k] = Math.Atan(Math.Abs(q1 / q2)) * 180 / Math.PI;
						if (180 - aci[k] < aci[k])
							aci[k] = 180 - aci[k];

						DeltaU[k] = 1.087F * Math.Pow(aci[k] + 6, -0.65F);
						if (DeltaU[k] > 0.17)
							DeltaU[k] = 0.17F;
						if (LrMax < Rb[k])
						{
							LrMax = Rb[k];
							k_forLrMax = k;
							du = DeltaU[k];
						}
					}
					for (k = 1; k <= NumberOfElements; k += 1)
					{
						if (Rb[k] != 0)
						{
							if (MinRot > DeltaU[k] / Rb[k])
							{
								MinRot = DeltaU[k] / Rb[k];
								du = DeltaU[k];
								kcritical = k;
							}
						}

					}
					for (k = 1; k <= NumberOfElements; k += 1)
					{
						deltaM = 0.209 * Math.Pow(aci[k] + 2, -0.32);

						Deltai = Rb[k] * du / Rb[kcritical];
						P = Deltai / deltaM;
						if (1.9 - 0.9 * P <= 0)
							Fb[k] = 1.3921F * (1 + 0.5 * Math.Pow(Math.Sin(aci[k] * Math.PI / 180), 1.5)) * Math.Pow(P, 0.3F) * Lb[k];
						else
							Fb[k] = 1.3921F * (1 + 0.5 * Math.Pow(Math.Sin(aci[k] * Math.PI / 180), 1.5)) * Math.Pow(P * (1.9F - 0.9F * P), 0.3F) * Lb[k];
					}

					sumfx = 0;
					sumfy = 0;
					SumRf = 0;
					SumParallel = 0; //  Parallel to Load
					SumNormal = 0; //  Normal to Load
					for (k = 1; k <= NumberOfElements; k += 1)
					{
						if (Rb[k] == 0)
						{
							Fxb[k] = 0;
							Fyb[k] = 0;
						}
						else
						{
							Fxb[k] = Fb[k] / Rb[k] * Math.Abs(Yb[k] - y0) * Math.Sign((y0 - Yb[k]));
							Fyb[k] = Fb[k] / Rb[k] * Math.Abs(Xb[k] - x0) * Math.Sign((Xb[k] - x0));
						}
						sumfx = sumfx + Fxb[k];
						sumfy = sumfy + Fyb[k];
						SumRf = SumRf + Rb[k] * Fb[k];
						SumParallel = SumParallel + Fxb[k] * s + Fyb[k] * c;
						SumNormal = SumNormal + Fxb[k] * c - Fyb[k] * s;
					}
					Cb = Math.Sqrt(Math.Pow(sumfx, 2) + Math.Pow(sumfy, 2));
					Cb = SumParallel;
					Moment = SumRf - Cb * (e + ro);
					switch (it)
					{
						case 1:
							SumNormal1 = SumNormal;
							Moment1 = Moment;
							ro1 = ro;
							ro = 10000000;
							break;
						case 2:
							SumNormal2 = SumNormal;
							Moment2 = Moment;
							ro2 = ro;
							ro = 10000000 / 2;
							break;
						default:
							ro3 = ro;
							SumNormal3 = SumNormal;
							Moment3 = Moment;
							if (Moment3 * Moment1 < 0)
							{
								ro = (ro1 + ro3) / 2;
								Moment2 = Moment3;
								ro2 = ro3;
							}
							else
							{
								ro = (ro2 + ro3) / 2;
								Moment1 = Moment3;
								ro1 = ro3;
							}
							break;
					}
				} while ((Math.Abs(SumNormal) > 0.00001 || Math.Abs(Moment) > 0.00001) && ro != ro3);

			}
			EccIncLoadOnWG = Cb / L * ConstNum.WELD_CONVERSION;
			return EccIncLoadOnWG;
		}

		public static void ContinuityAndDoublerPlates(EMemberType memberType)
		{
			double tpl = 0;
			double MaxBf = 0;
			double MaxBtf = 0;
			double tf_L = 0;
			double Bf_L = 0;
			double tf_R = 0;
			double Bf_R = 0;
			double tpz = 0;
			double Ltpz = 0;
			double Rtpz = 0;
			double Tcp = 0;
			double tfc_minL = 0;
			double tfc_minR = 0;
			double Pb = 0;
			double Ffu = 0;
			double AlfaM = 0;
			double Aw = 0;
			double Af = 0;
			double C3 = 0;
			double dbt = 0;
			double Ca = 0;
			double twc_min = 0;
			double tfc_Limit = 0;
			double yc = 0;
			double s = 0;
			double C2 = 0;
			double tfc_min = 0;
			double C1 = 0;
			double LMc = 0;
			double RMc = 0;
			double LCy = 0;
			double RCy = 0;
			double Depth = 0;
			double Ryb = 0;
			double Fyb = 0;
			double Rtf = 0;
			double ltf = 0;
			double tf = 0;
			double BF = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var rightBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var leftBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var femaData = beam.WinConnect.Fema;
			var columnStiffener = CommonDataStatic.ColumnStiffener;

			if (femaData.H == 0)
				return;

			columnStiffener.Material = femaData.Material;

			switch (femaData.Connection)
			{
				case EFemaConnectionType.BFP:
				case EFemaConnectionType.WFP:
					BF = beam.WinConnect.MomentFlangePlate.TopWidth;
					tf = beam.WinConnect.MomentFlangePlate.TopThickness;
					ltf = leftBeam.WinConnect.MomentFlangePlate.TopThickness;
					Rtf = rightBeam.WinConnect.MomentFlangePlate.TopThickness;
					Fyb = beam.WinConnect.MomentFlangePlate.Material.Fy;
					Ryb = MiscCalculationDataMethods.ExpYieldStr(beam.WinConnect.MomentFlangePlate.Material.Fy);
					Depth = beam.Shape.d + 2 * tf;
					beam.BraceStiffener.RDepth = rightBeam.Shape.d + 2 * rightBeam.WinConnect.MomentFlangePlate.TopThickness;
					beam.BraceStiffener.LDepth = leftBeam.Shape.d + 2 * leftBeam.WinConnect.MomentFlangePlate.TopThickness;
					break;
				case EFemaConnectionType.BUEP:
					BF = beam.Shape.bf;
					tf = beam.Shape.tf;
					ltf = leftBeam.Shape.tf * leftBeam.Material.Fy / columnStiffener.Material.Fy;
					Rtf = rightBeam.Shape.tf * rightBeam.Material.Fy / columnStiffener.Material.Fy;
					Fyb = beam.Material.Fy;
					Ryb = femaData.Ry;
					Depth = beam.WinConnect.MomentEndPlate.Length / 2 + (beam.Shape.d - beam.Shape.tf) / 2;
					if (rightBeam.Shape.d > 0)
						beam.BraceStiffener.RDepth = rightBeam.WinConnect.MomentEndPlate.Length / 2 + (rightBeam.Shape.d - rightBeam.Shape.tf) / 2;
					else
						beam.BraceStiffener.RDepth = -1;
					if (leftBeam.Shape.d > 0)
						beam.BraceStiffener.LDepth = leftBeam.WinConnect.MomentEndPlate.Length / 2 + (leftBeam.Shape.d - leftBeam.Shape.tf) / 2;
					else
						beam.BraceStiffener.LDepth = -1;
					break;
				case EFemaConnectionType.BSEP:
					BF = beam.Shape.bf;
					tf = beam.Shape.tf;
					ltf = leftBeam.Shape.tf * leftBeam.Material.Fy / columnStiffener.Material.Fy;
					Rtf = rightBeam.Shape.tf * rightBeam.Material.Fy / columnStiffener.Material.Fy;
					Fyb = beam.Material.Fy;
					Ryb = femaData.Ry;
					Depth = beam.WinConnect.MomentEndPlate.Length / 2 + (beam.Shape.d - beam.Shape.tf) / 2;
					if (rightBeam.Shape.d > 0)
						beam.BraceStiffener.RDepth = rightBeam.WinConnect.MomentEndPlate.Length / 2 + (rightBeam.Shape.d - rightBeam.Shape.tf) / 2;
					else
						beam.BraceStiffener.RDepth = -1;
					if (leftBeam.Shape.d > 0)
						beam.BraceStiffener.LDepth = leftBeam.WinConnect.MomentEndPlate.Length / 2 + (leftBeam.Shape.d - leftBeam.Shape.tf) / 2;
					else
						beam.BraceStiffener.LDepth = -1;
					break;
				case EFemaConnectionType.DST:
					BF = beam.WinConnect.MomentTee.TopLengthAtFlange;
					tf = beam.WinConnect.MomentTee.TopTeeShape.tw;
					ltf = leftBeam.WinConnect.MomentTee.TopTeeShape.tw;
					Rtf = rightBeam.WinConnect.MomentTee.TopTeeShape.tw;
					Fyb = beam.WinConnect.MomentTee.TopMaterial.Fy;
					Ryb = MiscCalculationDataMethods.ExpYieldStr(beam.WinConnect.MomentTee.TopMaterial.Fy);
					Depth = beam.Shape.d + 2 * tf;
					beam.BraceStiffener.RDepth = rightBeam.Shape.d + rightBeam.WinConnect.MomentTee.TopTeeShape.tw;
					beam.BraceStiffener.LDepth = leftBeam.Shape.d + leftBeam.WinConnect.MomentTee.TopTeeShape.tw;
					break;
				default:
					BF = beam.Shape.bf;
					tf = beam.Shape.tf;
					ltf = leftBeam.Shape.tf * leftBeam.Material.Fy / columnStiffener.Material.Fy;
					Rtf = rightBeam.Shape.tf * rightBeam.Material.Fy / columnStiffener.Material.Fy;
					Fyb = beam.Material.Fy;
					Ryb = femaData.Ry;
					Depth = beam.Shape.d;
					beam.BraceStiffener.RDepth = rightBeam.Shape.d;
					beam.BraceStiffener.LDepth = leftBeam.Shape.d;
					break;
			}

			if (rightBeam.Shape.d > 0 && leftBeam.Shape.d > 0)
			{
				RCy = rightBeam.WinConnect.Fema.Cy;
				LCy = leftBeam.WinConnect.Fema.Cy;
				RMc = rightBeam.WinConnect.Fema.Mc;
				LMc = leftBeam.WinConnect.Fema.Mc;
			}
			else if (rightBeam.Shape.d > 0)
			{
				RCy = rightBeam.WinConnect.Fema.Cy;
				RMc = rightBeam.WinConnect.Fema.Mc;
			}
			else if (leftBeam.Shape.d > 0)
			{
				LCy = leftBeam.WinConnect.Fema.Cy;
				LMc = leftBeam.WinConnect.Fema.Mc;
			}

			// Continuity Plates:
			switch (femaData.Connection)
			{
				case EFemaConnectionType.BUEP:
					// Step 6 --Tension
					C1 = column.GageOnFlange / 2 - column.Shape.k1; // eq 3-24
					femaData.c = beam.Shape.tf + 2 * femaData.pf;
					tfc_min = Math.Pow(femaData.Mf * C1 / (2 * column.Material.Fy * femaData.c * (beam.Shape.d - beam.Shape.tf)), 0.5); // eq 3-23
					if (tfc_min > column.Shape.tf)
					{
						// Continuity plates are required
						// also check the following
						C2 = (column.Shape.bf - column.GageOnFlange) / 2;
						s = Math.Pow(C1 * C2 * (2 * column.Shape.bf - 4 * column.Shape.k1) / (C2 + 2 * C1), 0.5);
						yc = (femaData.c / 2 + s) * (1 / C2 + 2 / C1) + (C2 + C1) * (4 / femaData.c + 2 / s);
						tfc_Limit = Math.Pow(femaData.Mf / (2 * (beam.Shape.d - beam.Shape.tf) * (0.8 * column.Material.Fy * yc)), 0.5);
						if (column.Shape.tf < tfc_Limit)
						{
							// Use a column with a thicker flange
						}
					}
					// Step 8 -- Compression
					twc_min = femaData.Mf / ((beam.Shape.d - beam.Shape.tf) * (6 * column.Shape.kdes + 2 * beam.WinConnect.MomentEndPlate.Thickness + beam.Shape.tf) * column.Material.Fy);
					break;
				case EFemaConnectionType.BSEP:
					switch (beam.WinConnect.MomentEndPlate.Bolt.ASTMType)
					{
						case EBoltASTM.A325:
							Ca = 1.45;
							break;
						case EBoltASTM.A490:
							Ca = 1.48;
							break;
					}
					dbt = beam.WinConnect.MomentEndPlate.Bolt.BoltSize;
					C3 = column.GageOnFlange / 2 - dbt / 4 - column.Shape.k1; // eq 3-39
					femaData.c = beam.Shape.tf + 2 * femaData.pf;
					Af = beam.Shape.tf * beam.Shape.bf;
					Aw = beam.Shape.tw * (beam.Shape.d - 2 * beam.Shape.tf);
					AlfaM = Ca * Math.Pow(Af / Aw, 1 / 3.0) * Math.Pow(C3 / dbt, 0.25);
					Ffu = femaData.Mf / (beam.Shape.d - beam.Shape.tf);
					Pb = Depth;
					tfc_min = Math.Pow(AlfaM * Ffu * C3 / (0.9 * column.Material.Fy * (3.5 * Pb + femaData.c)), 0.5); // eq 3-37
					// Step 6 -- Compression
					twc_min = femaData.Mf / ((beam.Shape.d - beam.Shape.tf) * (6 * column.Shape.kdes + 2 * beam.WinConnect.MomentEndPlate.Thickness + beam.Shape.tf) * column.Material.Fy);
					// Step 7
					// If Continuity plates are required then Column.tf>= tp
					// Step 8
					break;
				case EFemaConnectionType.DST:
					tfc_min = 1.5 * beam.WinConnect.MomentTee.TopTeeShape.tf;
					twc_min = femaData.Mf / ((beam.Shape.d + beam.WinConnect.MomentTee.TopTeeShape.tw) * (6 * column.Shape.kdes + 2 * beam.WinConnect.MomentTee.TopTeeShape.tf + beam.WinConnect.MomentTee.TopTeeShape.tw) * column.Material.Fy);
					break;
				default:
					tfc_minR = Math.Max(0.4 * Math.Pow(1.8 * rightBeam.Shape.bf * rightBeam.Shape.tf * Fyb * Ryb / (column.Material.Fy * femaData.Ryc), 0.5), rightBeam.Shape.bf / 6); // eq. 3-5, 3-6
					tfc_minL = Math.Max(0.4 * Math.Pow(1.8 * leftBeam.Shape.bf * leftBeam.Shape.tf * Fyb * Ryb / (column.Material.Fy * femaData.Ryc), 0.5), leftBeam.Shape.bf / 6); // eq. 3-5, 3-6
					tfc_min = Math.Max(tfc_minR, tfc_minL);
					twc_min = 0;
					break;
			}
			if (tfc_min > column.Shape.tf || twc_min > column.Shape.tw)
			{
				if (beam.MemberType == EMemberType.RightBeam && (leftBeam.MomentConnection != EMomentCarriedBy.NoMoment && leftBeam.MomentConnection != EMomentCarriedBy.NoMoment) ||
				    beam.MemberType == EMemberType.LeftBeam && (rightBeam.MomentConnection != EMomentCarriedBy.NoMoment && rightBeam.MomentConnection != EMomentCarriedBy.NoMoment))
					Tcp = Math.Max(Math.Max(Rtf, ltf), columnStiffener.SWidth * 1.79F * Math.Pow(columnStiffener.Material.Fy / ConstNum.ELASTICITY, 0.5));
				else if (beam.MemberType == EMemberType.RightBeam)
					Tcp = Math.Max(Rtf / 2, columnStiffener.SWidth * 1.79F * Math.Pow(columnStiffener.Material.Fy / ConstNum.ELASTICITY, 0.5));
				else if (beam.MemberType == EMemberType.LeftBeam)
					Tcp = Math.Max(ltf / 2, columnStiffener.SWidth * 1.79F * Math.Pow(columnStiffener.Material.Fy / ConstNum.ELASTICITY, 0.5));

				if (!columnStiffener.SThickness_User)
					columnStiffener.SThickness = CommonCalculations.PlateThickness(Tcp);
				columnStiffener.CompStiff = true;
				columnStiffener.TenStiff = true;
				if (!columnStiffener.SWidth_User)
					columnStiffener.SWidth = (column.Shape.bf - column.Shape.tw) / 2;
				columnStiffener.SLength = column.Shape.d - 2 * column.Shape.tf;
			}
			else
			{
				columnStiffener.SThickness = 0;
				columnStiffener.CompStiff = false;
				columnStiffener.TenStiff = false;
				columnStiffener.SWidth = 0;
				columnStiffener.SLength = 0;
			}
			// Doubler Plate
			if (rightBeam.Shape.d > 0)
				Rtpz = RCy * RMc * (femaData.H - beam.BraceStiffener.RDepth) / (femaData.H * 0.9 * 0.6 * column.Material.Fy * femaData.Ryc * column.Shape.d * (beam.BraceStiffener.RDepth - rightBeam.Shape.tf)); //  eq.3-7
			else
				Rtpz = 0;

			if (leftBeam.Shape.d > 0)
				Ltpz = LCy * LMc * (femaData.H - beam.BraceStiffener.LDepth) / (femaData.H * 0.9 * 0.6 * column.Material.Fy * femaData.Ryc * column.Shape.d * (beam.BraceStiffener.LDepth - leftBeam.Shape.tf)); //  eq.3-7
			else
				Ltpz = 0;
			tpz = Rtpz + Ltpz;

			if (tpz > column.Shape.tw)
			{
				if (!CommonDataStatic.ColumnStiffener.DNumberOfPlates_User)
					columnStiffener.DNumberOfPlates = 2;
				if (!CommonDataStatic.ColumnStiffener.DThickness_User)
					columnStiffener.DThickness = CommonCalculations.PlateThickness((tpz - column.Shape.tw) * column.Material.Fy / (2 * columnStiffener.Material.Fy));
				columnStiffener.DThickness2 = columnStiffener.DThickness;
				if (!CommonDataStatic.ColumnStiffener.DHorizontal_User)
					columnStiffener.DHorizontal = column.Shape.d - 2 * column.Shape.tf;
				if (!CommonDataStatic.ColumnStiffener.DFlangeWeldSize_User)
					columnStiffener.DFlangeWeldSize = ConstNum.FIOMEGA0_9N * 0.6 * columnStiffener.DThickness * columnStiffener.Material.Fy / (ConstNum.FIOMEGA0_75N * 0.707 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				if (!CommonDataStatic.ColumnStiffener.DWebWeldSize_User)
					columnStiffener.DWebWeldSize = columnStiffener.DFlangeWeldSize;

				if (leftBeam.IsActive && rightBeam.IsActive)
				{
					columnStiffener.LDoublerPlateBottom = leftBeam.WinConnect.Beam.TopElValue - leftBeam.Shape.d - leftBeam.WinConnect.Beam.BotAttachThick + columnStiffener.SThickness;
					columnStiffener.LvL = leftBeam.Shape.d + leftBeam.WinConnect.Beam.TopAttachThick + leftBeam.WinConnect.Beam.BotAttachThick - 2 * columnStiffener.SThickness;
					columnStiffener.RDoublerPlateBottom = rightBeam.WinConnect.Beam.TopElValue - rightBeam.Shape.d - rightBeam.WinConnect.Beam.BotAttachThick + columnStiffener.SThickness;
					if (!CommonDataStatic.ColumnStiffener.DVertical_User)
						columnStiffener.DVertical = rightBeam.Shape.d + rightBeam.WinConnect.Beam.TopAttachThick + rightBeam.WinConnect.Beam.BotAttachThick - 2 * columnStiffener.SThickness;
				}
				else if (leftBeam.IsActive)
				{
					columnStiffener.LDoublerPlateBottom = leftBeam.WinConnect.Beam.TopElValue - leftBeam.Shape.d - (leftBeam.WinConnect.Beam.BotAttachThick - columnStiffener.SThickness) / 2;
					columnStiffener.LvL = leftBeam.Shape.d + (leftBeam.WinConnect.Beam.TopAttachThick + leftBeam.WinConnect.Beam.BotAttachThick) / 2 - columnStiffener.SThickness;
					columnStiffener.RDoublerPlateBottom = columnStiffener.LDoublerPlateBottom;
					if (!CommonDataStatic.ColumnStiffener.DVertical_User)
						columnStiffener.DVertical = columnStiffener.LvL;
				}
				else if (rightBeam.IsActive)
				{
					columnStiffener.RDoublerPlateBottom = rightBeam.WinConnect.Beam.TopElValue - rightBeam.Shape.d - (rightBeam.WinConnect.Beam.BotAttachThick - columnStiffener.SThickness) / 2;
					if (!CommonDataStatic.ColumnStiffener.DVertical_User)
						columnStiffener.DVertical = rightBeam.Shape.d + (rightBeam.WinConnect.Beam.TopAttachThick + rightBeam.WinConnect.Beam.BotAttachThick) / 2 - columnStiffener.SThickness;
					columnStiffener.LDoublerPlateBottom = columnStiffener.RDoublerPlateBottom;
					columnStiffener.LvL = columnStiffener.DVertical;
				}
			}
			else
			{
				columnStiffener.DNumberOfPlates = 0;
				columnStiffener.DThickness = 0;
				columnStiffener.DThickness2 = 0;
				columnStiffener.LvL = 0;
				columnStiffener.DVertical = 0;
				columnStiffener.DHorizontal = 0;
				columnStiffener.LDoublerPlateBottom = 0;
				columnStiffener.RDoublerPlateBottom = 0;
			}

			// Continuity Plates:
			// continuity plate thickness NOTE: Continuity plates to be welded to column flange by CJP
			// and welded to web to develop the shear strength of net length of plate
			Reporting.AddHeader("Column Panel Zone");
			Reporting.AddHeader("Continuity Plates");

			switch (femaData.Connection)
			{
				case EFemaConnectionType.BUEP:
					// Step 6 --Tension
					Reporting.AddLine("Minimum required column flange thickness:");
					C1 = column.GageOnFlange / 2 - column.Shape.k1; // eq 3-24
					Reporting.AddLine("C1 = g / 2 - k1= " + column.GageOnFlange + " / 2 - " + column.Shape.k1 + "= " + C1 + " " + ConstUnit.Length);

					femaData.c = beam.Shape.tf + 2 * femaData.pf;
					Reporting.AddLine("C = tf + 2 * pf  = " + beam.Shape.tf + " + 2 * " + femaData.pf + " = " + femaData.c + " " + ConstUnit.Length);

					tfc_min = Math.Pow(femaData.Mf * C1 / (2 * column.Material.Fy * femaData.c * (beam.Shape.d - beam.Shape.tf)), 0.5); // eq 3-23
					Reporting.AddLine("tfc_min = (Mf * C1 / (2 * Fy * C * (d - tf)))^0.5"); // eq 3-23
					Reporting.AddLine(" = (" + femaData.Mf + " * " + C1 + " / (2 * " + column.Material.Fy + " * " + femaData.c + " * (" + beam.Shape.d + " - " + beam.Shape.tf + "))) ^0.5 'eq 3-23")
						;
					if (tfc_min > column.Shape.tf)
					{
						Reporting.AddLine(" = " + tfc_min + " >> " + column.Shape.tf + " " + ConstUnit.Length);
						Reporting.AddHeader("Continuity plates are required to resist tension.");
						Reporting.AddLine("C2 = (bf - g) / 2 = (" + column.Shape.bf + " - " + column.GageOnFlange + ") / 2 = " + C2 + " " + ConstUnit.Length);

						s = Math.Pow(C1 * C2 * (2 * column.Shape.bf - 4 * column.Shape.k1) / (C2 + 2 * C1), 0.5);
						Reporting.AddLine("s = (C1 * C2 * (2 * bf - 4 * k1) / (C2 + 2 * C1)) ^0.5");
						Reporting.AddLine(" = (" + C1 + " * " + C2 + " * (2 * " + column.Shape.bf + " - 4 * " + column.Shape.k1 + ") / (" + C2 + " + 2 * " + C1 + ")) ^0.5 = " + s + " " + ConstUnit.Length);

						yc = (femaData.c / 2 + s) * (1 / C2 + 2 / C1) + (C2 + C1) * (4 / femaData.c + 2 / s);
						Reporting.AddLine("Yc = (C / 2 + s) * (1 / C2 + 2 / C1) + (C2 + C1) * (4 / C + 2 / s)");
						Reporting.AddLine(" = (" + femaData.c + " / 2 + " + s + ") * (1 / " + C2 + " + 2 / " + C1 + ") + (" + C2 + " + " + C1 + ") * (4 / " + femaData.c + " + 2 / " + s + ") = " + yc);

						tfc_Limit = Math.Pow(femaData.Mf / (2 * (beam.Shape.d - beam.Shape.tf) * (0.8 * column.Material.Fy * yc)), 0.5);
						Reporting.AddLine("tfc_Limit = (Mf / (2 * (db - tfb) * (0.8 * Fyc * Yc))) ^0.5");
						Reporting.AddLine(" = (" + femaData.Mf + "/ (2 * (" + beam.Shape.d + " - " + beam.Shape.tf + ") * (0.8 * " + column.Material.Fy + " * " + yc + ")))^0.5");
						if (tfc_Limit <= column.Shape.tf)
							Reporting.AddLine(" = " + tfc_Limit + " <= " + column.Shape.tf + ConstUnit.Length + " (OK)");
						else
						{
							Reporting.AddLine(" = " + tfc_Limit + " >> " + column.Shape.tf + ConstUnit.Length + " (NG)");
							Reporting.AddHeader("Use a column with a thicker flange.");
						}
					}
					else
					{
						Reporting.AddLine(" = " + tfc_min + " >> " + column.Shape.tf + " " + ConstUnit.Length);
						Reporting.AddHeader(" Continuity plates are not required to resist tension.");
					}
					// Step 8 -- Compression
					Reporting.AddHeader("Minimum required column web thickness:");
					twc_min = femaData.Mf / ((beam.Shape.d - beam.Shape.tf) * (6 * column.Shape.kdes + 2 * beam.WinConnect.MomentEndPlate.Thickness + beam.Shape.tf) * column.Material.Fy);
					Reporting.AddLine("twc_min = Mf / ((db - tfb) * (6 * k + 2 * tp + tfb) * Fy)");
					Reporting.AddLine(" = " + femaData.Mf + " / ((" + beam.Shape.d + " - " + beam.Shape.tf + ") * (6 * " + column.Shape.kdes + " + 2 * " + beam.WinConnect.MomentEndPlate.Thickness + " + " + beam.Shape.tf + ") * " + column.Material.Fy + ")");
					if (twc_min > column.Shape.tw)
					{
						Reporting.AddLine(" = " + twc_min + " >> " + column.Shape.tw + " " + ConstUnit.Length);
						Reporting.AddLine(" Continuity plates are required to resist compression.");
					}
					else
					{
						Reporting.AddLine(" = " + twc_min + " <= " + column.Shape.tw + " " + ConstUnit.Length);
						Reporting.AddHeader(" Continuity plates are not required to resist compression.");
					}
					break;
				case EFemaConnectionType.BSEP:
					if (beam.WinConnect.MomentEndPlate.Bolt.ASTMType == EBoltASTM.A325)
						Ca = 1.45;
					else if (beam.WinConnect.MomentEndPlate.Bolt.ASTMType == EBoltASTM.A490)
						Ca = 1.48;

					Reporting.AddHeader("Minimum required column flange thickness:");
					Reporting.AddLine("Ca = " + Ca);

					dbt = beam.WinConnect.MomentEndPlate.Bolt.BoltSize;
					C3 = column.GageOnFlange / 2 - dbt / 4 - column.Shape.k1; // eq 3-39
					Reporting.AddLine("C3 = g / 2 - dbt / 4 - k1 = " + column.GageOnFlange + " / 2 - " + dbt + " / 4 - " + column.Shape.k1 + " = " + C3 + " " + ConstUnit.Length);

					femaData.c = beam.Shape.tf + 2 * femaData.pf;
					Reporting.AddLine("C = tfb + 2 * pf = " + beam.Shape.tf + " + 2 * " + femaData.pf + " = " + femaData.c + " " + ConstUnit.Length);

					Af = beam.Shape.tf * beam.Shape.bf;
					Reporting.AddLine("Af = tfb * bfb = " + beam.Shape.tf + " * " + beam.Shape.bf + " = " + Af + " " + ConstUnit.Area);

					Aw = beam.Shape.tw * (beam.Shape.d - 2 * beam.Shape.tf);
					Reporting.AddLine("Aw = twb * (d - 2 * tfb)= " + beam.Shape.tw + " * (" + beam.Shape.d + " - 2 * " + beam.Shape.tf + ")= " + Aw + " " + ConstUnit.Area);

					AlfaM = Ca * Math.Pow(Af / Aw, 1 / 3.0) * Math.Pow(C3 / dbt, 0.25);
					Reporting.AddLine("AlphaM = Ca * (Af / Aw)^(1 / 3) * (C3 / dbt)^0.25 = " + Ca + " * (" + Af + "/ " + Aw + ") ^(1 / 3) * (" + C3 + " / " + dbt + ") ^0.25= " + AlfaM);

					Ffu = femaData.Mf / (beam.Shape.d - beam.Shape.tf);
					Reporting.AddLine("Ffu = Mf / (d - tfb) = " + femaData.Mf + " / (" + beam.Shape.d + " - " + beam.Shape.tf + ") = " + Ffu + " " + ConstUnit.Force);

					Pb = Depth;
					tfc_min = Math.Pow(AlfaM * Ffu * C3 / (0.9 * column.Material.Fy * (3.5 * Pb + femaData.c)), 0.5); // eq 3-37
					Reporting.AddLine("tfc_min = (AlphaM * Ffu * C3 / (0.9 * Fy * (3.5 * pb + C)))^0.5"); // eq 3-37
					Reporting.AddLine(" = (" + AlfaM + " * " + Ffu + " * " + C3 + " / (0.9 * " + column.Material.Fy + " * (3.5 * " + Pb + " + " + femaData.c + "))) ^0.5"); // eq 3-37
					if (tfc_min > column.Shape.tf)
					{
						Reporting.AddLine(" = " + tfc_min + " >> tfc = " + column.Shape.tf + " " + ConstUnit.Length);
						Reporting.AddHeader(" Continuity plates are required to resist tension.");
					}
					else
					{
						Reporting.AddLine(" = " + tfc_min + " <=  tfc = " + column.Shape.tf + " " + ConstUnit.Length);
						Reporting.AddHeader(" Continuity plates are not required to resist tension.");
					}

					// Step 6 -- Compression
					Reporting.AddHeader("Minimum required column web thickness:");
					twc_min = femaData.Mf / ((beam.Shape.d - beam.Shape.tf) * (6 * column.Shape.kdes + 2 * beam.WinConnect.MomentEndPlate.Thickness + beam.Shape.tf) * column.Material.Fy);
					Reporting.AddLine("twc_min = Mf / ((db - tfb) * (6 * k + 2 * tp + tfb) * Fy)");
					Reporting.AddLine(" = " + femaData.Mf + " / ((" + beam.Shape.d + " - " + beam.Shape.tf + ") * (6 * " + column.Shape.kdes + " + 2 * " + beam.WinConnect.MomentEndPlate.Thickness + " + " + beam.Shape.tf + ") * " + column.Material.Fy + ")");
					if (twc_min > column.Shape.tw)
					{
						Reporting.AddLine(" = " + twc_min + " >> " + column.Shape.tw + " " + ConstUnit.Length);
						Reporting.AddHeader(" Continuity plates are required to resist compression.");
					}
					else
					{
						Reporting.AddLine(" = " + twc_min + " <= " + column.Shape.tw + " " + ConstUnit.Length);
						Reporting.AddHeader(" Continuity plates are not required to resist compression.");
					}
					if (twc_min > column.Shape.tw || tfc_min > column.Shape.tf)
					{
						columnStiffener.CompStiff = true;
						columnStiffener.TenStiff = true;
					}
					else
					{
						columnStiffener.CompStiff = false;
						columnStiffener.TenStiff = false;
					}

					// Step 7
					// If Continuity plates are required then Column.tf>= tp
					if (columnStiffener.CompStiff || columnStiffener.TenStiff)
					{
						if (column.Shape.tf <= beam.WinConnect.MomentEndPlate.Thickness)
							Reporting.AddLine("Column flange thickness = " + column.Shape.tf + " <= End plate thickness = " + beam.WinConnect.MomentEndPlate.Thickness + ConstUnit.Length + " (NG)");
						else
							Reporting.AddLine("Column flange thickness = " + column.Shape.tf + " >> End plate thickness = " + beam.WinConnect.MomentEndPlate.Thickness + ConstUnit.Length + " (OK)");
					}
					// Step 8
					break;
				case EFemaConnectionType.DST:
					tfc_min = 1.5 * beam.WinConnect.MomentTee.TopTeeShape.tf;
					Reporting.AddLine("Minimum required column flange thickness:");
					Reporting.AddLine("Tfc_min =  1.5 * tf_tee=  1.5 * " + beam.WinConnect.MomentTee.TopTeeShape.tf); // eq. 3-67
					if (tfc_min > column.Shape.tf)
					{
						Reporting.AddLine(" = " + tfc_min + " >> tfc = " + column.Shape.tf + " " + ConstUnit.Length);
						Reporting.AddHeader(" Continuity plates are required to resist tension.");
					}
					else
					{
						Reporting.AddLine(" = " + tfc_min + " <=  tfc = " + column.Shape.tf + " " + ConstUnit.Length);
						Reporting.AddHeader(" Continuity plates are not required to resist tension.");
					}

					twc_min = femaData.Mf / ((beam.Shape.d + beam.WinConnect.MomentTee.TopTeeShape.tw) * (6 * column.Shape.kdes + 2 * beam.WinConnect.MomentTee.TopTeeShape.tf + beam.WinConnect.MomentTee.TopTeeShape.tw) * column.Material.Fy);
					Reporting.AddHeader("Minimum required column web thickness:");
					Reporting.AddLine("twc_min = Mf / ((db + tw_tee) * (6 * k + 2 * tf_tee + tw_tee) * Fy)");
					Reporting.AddLine(" = " + femaData.Mf + " / ((" + beam.Shape.d + " + " + beam.WinConnect.MomentTee.TopTeeShape.tw + ") * (6 * " + column.Shape.kdes + " + 2 * " + beam.WinConnect.MomentTee.TopTeeShape.tf + " + " + beam.WinConnect.MomentTee.TopTeeShape.tw + ") * " + column.Material.Fy + ")");
					if (twc_min > column.Shape.tw)
					{
						Reporting.AddLine(" = " + twc_min + " >> " + column.Shape.tw + " " + ConstUnit.Length);
						Reporting.AddLine(" Continuity plates are required to resist compression.");
					}
					else
					{
						Reporting.AddLine(" = " + twc_min + " <= " + column.Shape.tw + " " + ConstUnit.Length);
						Reporting.AddLine(" Continuity plates are not required to resist compression.");
					}
					if (twc_min > column.Shape.tw || tfc_min > column.Shape.tf)
					{
						columnStiffener.CompStiff = true;
						columnStiffener.TenStiff = true;
					}
					else
					{
						columnStiffener.CompStiff = false;
						columnStiffener.TenStiff = false;
					}
					Reporting.AddLine("");
					// Step 7
					// If Continuity plates are required then Column.tf>= tp
					if (columnStiffener.CompStiff | columnStiffener.TenStiff)
					{
						if (column.Shape.tf < beam.WinConnect.MomentTee.TopTeeShape.tf)
							Reporting.AddLine("Column flange thickness = " + column.Shape.tf + " << tf_tee = " + beam.WinConnect.MomentTee.TopTeeShape.tf + ConstUnit.Length + " (NG)");
						else
							Reporting.AddLine("Column flange thickness = " + column.Shape.tf + " >= tf_tee = " + beam.WinConnect.MomentTee.TopTeeShape.tf + ConstUnit.Length + " (OK)");
					}
					break;
				default:
					Bf_R = rightBeam.Shape.bf;
					tf_R = rightBeam.Shape.tf;
					Bf_L = leftBeam.Shape.bf;
					tf_L = leftBeam.Shape.tf;
					MaxBtf = Math.Max(Bf_R * tf_R, Bf_L * tf_L);
					MaxBf = Math.Max(Bf_R, Bf_L);
					tfc_min = Math.Max(0.4 * Math.Pow(1.8 * Math.Max(Bf_R * tf_R, Bf_L * tf_L) * Fyb * Ryb / (column.Material.Fy * femaData.Ryc), 0.5), Math.Max(Bf_R, Bf_L) / 6); // eq. 3-5, 3-6
					Reporting.AddHeader("Minimum required column flange thickness:");
					Reporting.AddLine("Tfc_min = Max(0.4 * (1.8 * Max(Bf_R * tf_R, Bf_L * tf_L)* Fy * Ry / (Fyc * Ryc))^0.5; Max(Bf_R, Bf_L) / 6)"); // eq. 3-5, 3-6
					Reporting.AddLine(" = Max(0.4 * (1.8 * " + MaxBtf + " * " + Fyb + " * " + Ryb + " / (" + column.Material.Fy + " * " + femaData.Ryc + ")) ^0.5; " + MaxBf + " / 6)"); // eq. 3-5, 3-6
					if (tfc_min > column.Shape.tf)
					{
						Reporting.AddLine(" = " + tfc_min + " >> tfc = " + column.Shape.tf + " " + ConstUnit.Length);
						Reporting.AddHeader("Continuity plates are required.");
					}
					else
					{
						Reporting.AddLine(" = " + tfc_min + " <= tfc = " + column.Shape.tf + " " + ConstUnit.Length);
						Reporting.AddHeader("Continuity plates are not required.");
					}
					break;
			}

			if (columnStiffener.CompStiff | columnStiffener.TenStiff)
			{
				Reporting.AddHeader("Continuity Plate Thickness:");
				Reporting.AddLine("tf_R and tf_L are tf values adjusted by the");
				Reporting.AddLine("ratio of Fy of element delivering the flange");
				Reporting.AddLine("force to the Fy of stiffener plate.");

				if (beam.MemberType == EMemberType.RightBeam && leftBeam.MomentConnection != EMomentCarriedBy.NoMoment || beam.MemberType == EMemberType.LeftBeam && rightBeam.MomentConnection != EMomentCarriedBy.NoMoment)
				{
					Tcp = Math.Max(Math.Max(Rtf, ltf), columnStiffener.SWidth * 1.79F * Math.Pow(columnStiffener.Material.Fy / ConstNum.ELASTICITY, 0.5));
					Reporting.AddLine("Tcp = Max(Max(tf_R; tf_L); W * 1.79 * (Fyp / " + ConstNum.ELASTICITY + ")^0.5)");
					Reporting.AddLine(" = Max(Max(" + Rtf + "; " + ltf + "); " + columnStiffener.SWidth + " * 1.79 * (" + columnStiffener.Material.Fy + " / " + ConstNum.ELASTICITY + ") ^0.5)");

				}
				else if (beam.MemberType == EMemberType.RightBeam)
				{
					Tcp = Math.Max(Rtf / 2, columnStiffener.SWidth * 1.79F * Math.Pow(columnStiffener.Material.Fy / ConstNum.ELASTICITY, 0.5));
					Reporting.AddLine("Tcp = max(tf_R/2; W * 1.79 * (Fyp / " + ConstNum.ELASTICITY + ")^0.5)");
					Reporting.AddLine(" = max(" + Rtf + "/2; " + columnStiffener.SWidth + " * 1.79 * (" + columnStiffener.Material.Fy + " / " + ConstNum.ELASTICITY + ") ^0.5)");

				}
				else if (beam.MemberType == EMemberType.LeftBeam)
				{
					Tcp = Math.Max(ltf / 2, columnStiffener.SWidth * 1.79F * Math.Pow(columnStiffener.Material.Fy / ConstNum.ELASTICITY, 0.5));
					Reporting.AddLine("Tcp = Max(tf_L/2; W * 1.79 * (Fyp / " + ConstNum.ELASTICITY + ")^0.5)");
					Reporting.AddLine(" = Max(" + ltf + "/2; " + columnStiffener.SWidth + " * 1.79 * (" + columnStiffener.Material.Fy + " / " + ConstNum.ELASTICITY + ") ^0.5)");

				}
				Reporting.AddLine(" = " + Tcp + " " + ConstUnit.Length);
				if (!columnStiffener.SThickness_User)
					columnStiffener.SThickness = CommonCalculations.PlateThickness(Tcp);
				columnStiffener.CompStiff = true;
				columnStiffener.TenStiff = true;
				if (!columnStiffener.SWidth_User)
					columnStiffener.SWidth = (column.Shape.bf - column.Shape.tw) / 2;
				columnStiffener.SLength = column.Shape.d - 2 * column.Shape.tf;

				Reporting.AddHeader("Use Four " + columnStiffener.Material.Name + " Plates:");
				Reporting.AddLine("Thickness = " + columnStiffener.SThickness + " " + ConstUnit.Length);
				Reporting.AddLine("Width (each) = (bfc - twc) / 2 = (" + column.Shape.bf + " - " + column.Shape.tw + ") / 2 = " + columnStiffener.SWidth + " " + ConstUnit.Length);
				Reporting.AddLine("Length = (dc- 2 * tfc) = (" + column.Shape.d + " - 2 * " + column.Shape.tf + ") = " + columnStiffener.SLength + " " + ConstUnit.Length);
			}
			else
			{
				columnStiffener.SThickness = 0;
				columnStiffener.CompStiff = false;
				columnStiffener.TenStiff = false;
				columnStiffener.SWidth = 0;
				columnStiffener.SLength = 0;
			}

			Reporting.AddHeader("Doubler Plate");
			if (rightBeam.Shape.d > 0)
			{
				Reporting.AddHeader("3Panel zone thick.req'd. for right side conn.:");
				Rtpz = RCy * RMc * (femaData.H - beam.BraceStiffener.RDepth) / (femaData.H * 0.9 * 0.6 * column.Material.Fy * femaData.Ryc * column.Shape.d * (beam.BraceStiffener.RDepth - rightBeam.Shape.tf)); //  eq.3-7
				Reporting.AddLine("tpz = Cy * Mc * (H - db) / (H * 0.9 * 0.6 * Fyc * Ryc * dc * (db - tf))"); //  eq.3-7
				Reporting.AddLine(" = " + RCy + " * " + RMc + " * (" + femaData.H + " - " + beam.BraceStiffener.RDepth + ") / (" + femaData.H + " * 0.9 * 0.6 * " + column.Material.Fy + " * " + femaData.Ryc + " * " + column.Shape.d + " * (" + beam.BraceStiffener.RDepth + " - " + rightBeam.Shape.tf + ")) ' eq.3-7");

				if (leftBeam.Shape.d > 0)
				{
					Reporting.AddLine(" = " + Rtpz + ConstUnit.Length);
				}
			}
			if (leftBeam.Shape.d > 0)
			{
				Reporting.AddHeader("Panel zone thick.req'd. for left side conn.:");
				Ltpz = LCy * LMc * (femaData.H - beam.BraceStiffener.LDepth) / (femaData.H * 0.9 * 0.6 * column.Material.Fy * femaData.Ryc * column.Shape.d * (beam.BraceStiffener.LDepth - leftBeam.Shape.tf)); //  eq.3-7
				Reporting.AddLine("tpz = Cy * Mc * (H - db) / (H * 0.9 * 0.6 * Fyc * Ryc * dc * (db - tf))"); //  eq.3-7
				Reporting.AddLine(" = " + LCy + " * " + LMc + " * (" + femaData.H + " - " + beam.BraceStiffener.LDepth + ") / (" + femaData.H + " * 0.9 * 0.6 * " + column.Material.Fy + " * " + femaData.Ryc + " * " + column.Shape.d + " * (" + beam.BraceStiffener.LDepth + " - " + leftBeam.Shape.tf + ")) ' eq.3-7");
				if (rightBeam.Shape.d > 0)
					Reporting.AddLine(" = " + Ltpz + ConstUnit.Length);
			}
			if (rightBeam.Shape.d > 0 && leftBeam.Shape.d > 0)
			{
				tpz = Rtpz + Ltpz;
				Reporting.AddHeader("Left and Right side required thicknesses combined:");
			}
			else if (rightBeam.Shape.d > 0)
				tpz = Rtpz;
			else if (leftBeam.Shape.d > 0)
				tpz = Ltpz;

			if (tpz > column.Shape.tw)
			{
				Reporting.AddLine(" = " + tpz + " >> twc = " + column.Shape.tw + ConstUnit.Length + " Doubler plate required.");
				columnStiffener.DNumberOfPlates = 2;
				Reporting.AddLine("");
				Reporting.AddHeader("Plate Thickness:");
				tpl = (tpz - column.Shape.tw) * column.Material.Fy / (2 * columnStiffener.Material.Fy);
				if (column.Material.Fy != columnStiffener.Material.Fy)
				{
					Reporting.AddLine("tPL = (tpz - tw) * Fy_c / (2 * Fy_p)");
					Reporting.AddLine(" =(" + tpz + " - " + column.Shape.tw + ") * " + column.Material.Fy + " / (2 * " + columnStiffener.Material.Fy + ")");
					Reporting.AddLine(" = " + tpl + " " + ConstUnit.Length);
				}
				else
				{
					Reporting.AddLine("tPL = (tpz - Column.tw) / 2");
					Reporting.AddLine(" =(" + tpz + " - " + column.Shape.tw + ") / 2 ");
					Reporting.AddLine(" = " + tpl + " " + ConstUnit.Length);
				}
				if (!columnStiffener.DThickness_User)
					columnStiffener.DThickness = CommonCalculations.PlateThickness(tpl);
				columnStiffener.DThickness2 = columnStiffener.DThickness;
				if (!columnStiffener.DHorizontal_User)
					columnStiffener.DHorizontal = column.Shape.d - 2 * column.Shape.tf;
				if (!columnStiffener.DFlangeWeldSize_User)
					columnStiffener.DFlangeWeldSize = ConstNum.FIOMEGA0_9N * 0.6 * columnStiffener.DThickness * columnStiffener.Material.Fy / (ConstNum.FIOMEGA0_75N * 0.707 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				if (!columnStiffener.DWebWeldSize_User)
					columnStiffener.DWebWeldSize = columnStiffener.DFlangeWeldSize;

				Reporting.AddHeader("Use Two " + columnStiffener.Material.Name + " Plates:");
				Reporting.AddLine("Thickness = " + columnStiffener.DThickness + " " + ConstUnit.Length);
				Reporting.AddLine("Width = dc - 2 * tfc = " + column.Shape.d + " - 2 * " + column.Shape.tf + " = " + columnStiffener.DHorizontal + " " + ConstUnit.Length);
				if (columnStiffener.DVertical != columnStiffener.LvL)
					Reporting.AddLine("Length = " + columnStiffener.DVertical + " ~ " + columnStiffener.LvL + " " + ConstUnit.Length);
				else
					Reporting.AddLine("Length = " + columnStiffener.DVertical + " " + ConstUnit.Length);

				Reporting.AddHeader("Doubler Plate Welds:");
				Reporting.AddHeader("Required fillet weld size:");

				Reporting.AddLine("w_req.= " + ConstString.FIOMEGA0_9 + " * 0.6 * tp * Fy / (" + ConstString.FIOMEGA0_75 + " * 0.707 * 0.6 * Fexx)"); // Int(-16 * 0.6 * DoublerPlate.t1 * DoublerPlate.Fy / (0.707 * 0.6 * Fexx)"
				Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 0.6 * " + columnStiffener.DThickness + " * " + columnStiffener.Material.Fy + " / (" + ConstString.FIOMEGA0_75 + " * 0.707 * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				Reporting.AddLine("= " + columnStiffener.DWebWeldSize + " " + ConstUnit.Length);
			}
			else
			{
				columnStiffener.DNumberOfPlates = 0;
				columnStiffener.DThickness = 0;
				columnStiffener.DThickness2 = 0;
				columnStiffener.LvL = 0;
				columnStiffener.DVertical = 0;
				columnStiffener.DHorizontal = 0;
				columnStiffener.LDoublerPlateBottom = 0;
				columnStiffener.RDoublerPlateBottom = 0;
				Reporting.AddLine(" = " + tpz + " <= twc = " + column.Shape.tw + ConstUnit.Length + " Doubler plate not required.");
			}
		}

		internal static double FCpr(EMemberType memberType)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];

			beam.WinConnect.Fema.Cpr = (beam.Material.Fy + beam.Material.Fu) / (2 * beam.Material.Fy); // eq. 3.2"
			Reporting.AddHeader("Cpr = (Fyb + Fub) / (2 * Fyb) = (" + beam.Material.Fy + " + " + beam.Material.Fu + ") / (2 * " + beam.Material.Fy + ") = " + beam.WinConnect.Fema.Cpr); // eq. 3.2"
			return beam.WinConnect.Fema.Cpr;
		}

		internal static double FMpr(EMemberType memberType)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];

			beam.WinConnect.Fema.Mpr = beam.WinConnect.Fema.Cpr * beam.WinConnect.Fema.Ry * beam.WinConnect.Fema.Ze * beam.Material.Fy; // eq. 3.1
			Reporting.AddHeader("Mpr = Cpr * Ry * Ze * Fyb = " + beam.WinConnect.Fema.Cpr + " * " + beam.WinConnect.Fema.Ry + " * " + beam.WinConnect.Fema.Ze + " * " + beam.Material.Fy);
			if (CommonDataStatic.Units == EUnit.US)
				Reporting.AddLine(" = " + beam.WinConnect.Fema.Mpr + " " + ConstUnit.Moment + " (" + beam.WinConnect.Fema.Mpr / 12 + " " + ConstUnit.MomentUnitFoot + ")"); // eq. 3.1
			else
				Reporting.AddLine(" = " + beam.WinConnect.Fema.Mpr + " " + ConstUnit.Moment + " (" + beam.WinConnect.Fema.Mpr / 1000000 + " " + ConstUnit.MomentUnitFoot + ")"); // eq. 3.1
			return beam.WinConnect.Fema.Mpr;
		}

		internal static double FCy(EMemberType memberType)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];

			beam.WinConnect.Fema.Cy = beam.WinConnect.Fema.Se / (beam.WinConnect.Fema.Cpr * beam.WinConnect.Fema.Ze); //  eq.3.4
			Reporting.AddHeader("Cy = Sb / (Cpr * Ze) = " + beam.WinConnect.Fema.Se + " / (" + beam.WinConnect.Fema.Cpr + " * " + beam.WinConnect.Fema.Ze + ") = " + beam.WinConnect.Fema.Cy); //  eq.3.4
			return beam.WinConnect.Fema.Cy;
		}

		internal static double fVp(EMemberType memberType)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];

			beam.WinConnect.Fema.Vp = ((2 * beam.WinConnect.Fema.Mpr + beam.WinConnect.Fema.Pg * (beam.WinConnect.Fema.L1 - beam.WinConnect.Fema.Lp_h) + beam.WinConnect.Fema.Wg * Math.Pow(beam.WinConnect.Fema.L1, 2) / 2) / beam.WinConnect.Fema.L1); // fig. 3-3
			Reporting.AddHeader("Vp = (2 * Mpr + Pg * (L1-Lp_h) + Wg * L1² / 2) / femaData.L1 'fig. 3-3");
			Reporting.AddLine(" (Lp_h = Distance from Pg to Plastic Hinge)");
			Reporting.AddLine("= (2 * " + beam.WinConnect.Fema.Mpr + " + " + beam.WinConnect.Fema.Pg + " * (" + beam.WinConnect.Fema.L1 + " - " + beam.WinConnect.Fema.Lp_h + ") + " + beam.WinConnect.Fema.Wg + " * " + beam.WinConnect.Fema.L1 + "² / 2) / " + beam.WinConnect.Fema.L1 + " = " + beam.WinConnect.Fema.Vp + " " + ConstUnit.Force); // fig. 3-3
			return beam.WinConnect.Fema.Vp;
		}

		internal static int fMc(EMemberType memberType)
		{
			int Mc = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];

			Mc = ((int)(beam.WinConnect.Fema.Mpr + beam.WinConnect.Fema.Vp * beam.WinConnect.Fema.sh)); // fig. 3-4
			Reporting.AddHeader("Mc = Mpr + Vp * sh = " + beam.WinConnect.Fema.Mpr + " + " + beam.WinConnect.Fema.Vp + " * " + beam.WinConnect.Fema.sh);
			if (CommonDataStatic.Units == EUnit.US)
				Reporting.AddLine(" = " + Mc + " " + ConstUnit.Moment + " (" + (Mc / 12) + " " + ConstUnit.MomentUnitFoot + ")");
			else
				Reporting.AddLine(" = " + Mc + " " + ConstUnit.Moment + " (" + (Mc / 1000000) + " " + ConstUnit.MomentUnitFoot + ")");
			return Mc;
		}

		internal static int fMf(EMemberType memberType)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];

			beam.WinConnect.Fema.Mf = ((int)(beam.WinConnect.Fema.Mpr + beam.WinConnect.Fema.Vp * beam.WinConnect.Fema.Lpl));
			Reporting.AddHeader("Mf = Mpr + Vp * Lpl = " + beam.WinConnect.Fema.Mpr + " + " + beam.WinConnect.Fema.Vp + " * " + beam.WinConnect.Fema.Lpl);
			if (CommonDataStatic.Units == EUnit.US)
				Reporting.AddLine(" = " + beam.WinConnect.Fema.Mf + " " + ConstUnit.Moment + "(" + (beam.WinConnect.Fema.Mf / 12) + " " + ConstUnit.MomentUnitFoot + ")");
			else
				Reporting.AddLine(" = " + beam.WinConnect.Fema.Mf + " " + ConstUnit.Moment + "(" + (beam.WinConnect.Fema.Mf / 1000000) + " " + ConstUnit.MomentUnitFoot + ")");
			return (int)beam.WinConnect.Fema.Mf;
		}

		internal static double fMyf(EMemberType memberType)
		{
			double Myf = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];

			Myf = beam.WinConnect.Fema.Cy * beam.WinConnect.Fema.Mf; // eq.3.3
			if (CommonDataStatic.Units == EUnit.US)
				Reporting.AddLine("Myf = Cy * Mf = " + beam.WinConnect.Fema.Cy + " * " + beam.WinConnect.Fema.Mf + " = " + Myf + " " + ConstUnit.Moment + " (" + Myf / 12 + " " + ConstUnit.MomentUnitFoot + ")");
			else
				Reporting.AddLine("Myf = Cy * Mf = " + beam.WinConnect.Fema.Cy + " * " + beam.WinConnect.Fema.Mf + " = " + Myf + " " + ConstUnit.Moment + " (" + Myf / 1000000 + " " + ConstUnit.MomentUnitFoot + ")");
			return Myf;
		}

		internal static double fVg(EMemberType memberType)
		{
			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			beam.WinConnect.Fema.Vg = beam.WinConnect.Fema.Wg * (beam.WinConnect.Fema.L - column.Shape.d) / 2 + beam.WinConnect.Fema.Pg * (beam.WinConnect.Fema.L - column.Shape.d - beam.WinConnect.Fema.Lp) / (beam.WinConnect.Fema.L - column.Shape.d);
			Reporting.AddHeader("Vg = Wg * (L - dc) / 2 + Pg * (L - dc - Lp) / (L - dc)");
			Reporting.AddLine("(Lp = Distance from Pg to col. face.)");
			Reporting.AddLine("= " + beam.WinConnect.Fema.Wg + " * (" + beam.WinConnect.Fema.L + " - " + column.Shape.d + ") / 2 + " + beam.WinConnect.Fema.Pg + " * (" + beam.WinConnect.Fema.L + " - " + column.Shape.d + " - " + beam.WinConnect.Fema.Lp + ") / (" + beam.WinConnect.Fema.L + " - " + column.Shape.d + ") = " + beam.WinConnect.Fema.Vg + " " + ConstUnit.Force);
			return beam.WinConnect.Fema.Vg;
		}

		internal static double fVweb(EMemberType memberType)
		{
			double Vweb = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			Vweb = (2 * beam.WinConnect.Fema.Mf / (beam.WinConnect.Fema.L - column.Shape.d) + beam.WinConnect.Fema.Vg); // Eq. 3-52
			Reporting.AddHeader("Vweb = 2 * Mf / (L - dc) + Vg = 2 * " + beam.WinConnect.Fema.Mf + " / (" + beam.WinConnect.Fema.L + " - " + column.Shape.d + ") + " + beam.WinConnect.Fema.Vg + " = " + Vweb + " " + ConstUnit.Force); // Eq. 3-52
			return Vweb;
		}
	}
}