using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class FlangeAngles
	{
		internal static void DesignFlangeAngles(EMemberType memberType)
		{
			double AnShear = 0;
			double AgShear = 0;
			double AnTension = 0;
			double AgTension = 0;
			double Lg = 0;
			double bsideedge = 0;
			double Cap = 0;
			double s = 0;
			double AnglGage2 = 0;
			double AnglGage1 = 0;
			double etmin = 0;
			double elmin = 0;
			double BearingCap = 0;
			double Fbs = 0;
			double si = 0;
			double Fbe = 0;
			double wreq = 0;
			double capacity = 0;
			double plasticmoment = 0;
			double ShearStrength = 0;
			double ShearRupture = 0;
			double ShearYielding = 0;
			double TensionCap = 0;
			double Ball = 0;
			double ta = 0;
			double B = 0;
			double AngleGage = 0;
			double FiRn = 0;
			double Fcr = 0;
			double kL_r = 0;
			double rg = 0;
			double k = 0;
			double An = 0;
			double Ag = 0;
			double thicknessMin = 0;
			double tnReqSup = 0;
			double weldlength = 0;
			double ea = 0;
			double FuT = 0;
			double ef = 0;
			double tbReq = 0;
			double tReq = 0;
			double t = 0;
			double tnReq = 0;
			double HoleSize = 0;
			double tgReq = 0;
			double L2 = 0;
			double tReq_BlockShear = 0;
			double Rn = 0;
			double rn2 = 0;
			double rn1 = 0;
			double LnTension = 0;
			double LgTension = 0;
			double LnShear = 0;
			double LgShear = 0;
			double AnShearReq = 0;
			double AgShearReq = 0;
			double Anreq = 0;
			double AgReq = 0;
			double reOsl = 0;
			double ref_migname = 0;
			double Ff = 0;
			double w = 0;
			double Mw = 0;
			double maxw = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			var momentFlangeAngle = beam.WinConnect.MomentFlangeAngle;

			Ff = Math.Abs(beam.P / 2) + Math.Abs(beam.Moment / beam.Shape.d);
			ref_migname = momentFlangeAngle.BeamBolt.MinEdgeSheared;
			if (momentFlangeAngle.BeamBolt.EdgeDistLongDir < ref_migname)
				momentFlangeAngle.BeamBolt.EdgeDistLongDir = ref_migname;
			reOsl = momentFlangeAngle.ColumnBolt.MinEdgeSheared;
			if (momentFlangeAngle.ColumnBolt.EdgeDistTransvDir < reOsl)
				momentFlangeAngle.ColumnBolt.EdgeDistTransvDir = reOsl;

			switch (momentFlangeAngle.BeamConnection)
			{
				case EConnectionStyle.Bolted:
					momentFlangeAngle.BeamBolt.NumberOfRows = (((int)Math.Ceiling(Ff / momentFlangeAngle.BeamBolt.BoltStrength / momentFlangeAngle.ColumnBolt.NumberOfBolts)));
					momentFlangeAngle.BeamBolt.NumberOfBolts = momentFlangeAngle.BeamBolt.NumberOfRows * momentFlangeAngle.ColumnBolt.NumberOfBolts;
					GetGageOnBeam(memberType, ref_migname);
					if (momentFlangeAngle.ColumnBolt.NumberOfBolts == 2)
						beam.WinConnect.Fema.L1 = (beam.GageOnFlange + 2 * momentFlangeAngle.BeamBolt.EdgeDistLongDir);
					else
						beam.WinConnect.Fema.L1 = (beam.GageOnFlange + 2 * (beam.GageOnFlange + momentFlangeAngle.BeamBolt.EdgeDistLongDir));

					AgReq = Ff / (ConstNum.FIOMEGA0_9N * momentFlangeAngle.Material.Fy); // tension yielding
					Anreq = Ff / (ConstNum.FIOMEGA0_75N * momentFlangeAngle.Material.Fu); // tension rupture
					AgShearReq = Ff / (ConstNum.FIOMEGA1_0N * 0.6 * momentFlangeAngle.Material.Fy); // Shear Yielding
					AnShearReq = Ff / (ConstNum.FIOMEGA0_75N * 0.6 * momentFlangeAngle.Material.Fu); // Shear rupure

					LgShear = 2 * ((momentFlangeAngle.ColumnBolt.NumberOfBolts - 1) * momentFlangeAngle.BeamBolt.SpacingTransvDir + momentFlangeAngle.BeamBolt.EdgeDistTransvDir);
					LnShear = LgShear - 2 * (momentFlangeAngle.ColumnBolt.NumberOfBolts - 0.5) * (momentFlangeAngle.BeamBolt.HoleLength + ConstNum.SIXTEENTH_INCH);
					if (momentFlangeAngle.BeamBolt.NumberOfRows == 4)
					{
						LgTension = beam.GageOnFlange + 2 * beam.GageOnFlange; // case 2
						LnTension = LgTension - 3 * (momentFlangeAngle.BeamBolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
						rn1 = MiscCalculationsWithReporting.BlockShearNew(momentFlangeAngle.Material.Fu, LnShear, 1, LnTension, LgShear, momentFlangeAngle.Material.Fy, 1, 0, false);
						LgTension = momentFlangeAngle.Length - beam.GageOnFlange; // case 6
						LnTension = LgTension - 3 * (momentFlangeAngle.BeamBolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
						rn2 = MiscCalculationsWithReporting.BlockShearNew(momentFlangeAngle.Material.Fu, LnShear, 1, LnTension, LgShear, momentFlangeAngle.Material.Fy, 1, 0, false);
						Rn = Math.Min(rn1, rn2);
					}
					else
					{
						LgTension = beam.GageOnFlange; // case 4
						LnTension = LgTension - (momentFlangeAngle.BeamBolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
						rn1 = MiscCalculationsWithReporting.BlockShearNew(momentFlangeAngle.Material.Fu, LnShear, 1, LnTension, LgShear, momentFlangeAngle.Material.Fy, 1, 0, false);
						LgTension = momentFlangeAngle.Length - beam.GageOnFlange; // case 3
						LnTension = LgTension - (momentFlangeAngle.BeamBolt.HoleWidth + ConstNum.SIXTEENTH_INCH);

						rn2 = MiscCalculationsWithReporting.BlockShearNew(momentFlangeAngle.Material.Fu, LnShear, 1, LnTension, LgShear, momentFlangeAngle.Material.Fy, 1, 0, false);
						Rn = Math.Min(rn1, rn2);
					}
					tReq_BlockShear = Ff / Rn;
					break;
				case EConnectionStyle.Welded:
					beam.WinConnect.Fema.L1 = beam.Shape.bf;
					AgReq = Ff / (ConstNum.FIOMEGA0_9N * momentFlangeAngle.Material.Fy);
					Anreq = Ff / (ConstNum.FIOMEGA0_75N * momentFlangeAngle.Material.Fu);
					AgShearReq = Ff / (ConstNum.FIOMEGA1_0N * 0.6 * momentFlangeAngle.Material.Fy); // Shear Yielding
					AnShearReq = Ff / (ConstNum.FIOMEGA0_75N * 0.6 * momentFlangeAngle.Material.Fu); // Shear rupure
					break;
			}

			GetNumberofColumnBolts(memberType, Ff, ref Ball);
			if (momentFlangeAngle.ColumnBolt.NumberOfBolts == 2)
				L2 = beam.GageOnColumn + 2 * momentFlangeAngle.ColumnBolt.EdgeDistTransvDir;
			else
				L2 = beam.GageOnColumn + 2 * (beam.GageOnColumn + momentFlangeAngle.ColumnBolt.EdgeDistTransvDir);

			momentFlangeAngle.Length = Math.Max(beam.WinConnect.Fema.L1, L2);

			if (momentFlangeAngle.ColumnBolt.NumberOfBolts == 2 && !momentFlangeAngle.ColumnBolt.EdgeDistLongDir_User)
				momentFlangeAngle.ColumnBolt.EdgeDistLongDir = (momentFlangeAngle.Length - beam.GageOnColumn) / 2;
			else if(!momentFlangeAngle.ColumnBolt.EdgeDistLongDir_User)
				momentFlangeAngle.ColumnBolt.EdgeDistLongDir = (momentFlangeAngle.Length - beam.GageOnColumn - 2 * momentFlangeAngle.ColumnBoltSpacingOut) / 2;
			
			if (momentFlangeAngle.BeamBolt.NumberOfBolts == 2 && !momentFlangeAngle.BeamBolt.EdgeDistLongDir_User)
				momentFlangeAngle.BeamBolt.EdgeDistLongDir = (momentFlangeAngle.Length - beam.GageOnFlange) / 2;
			else if(!momentFlangeAngle.BeamBolt.EdgeDistLongDir_User)
				momentFlangeAngle.BeamBolt.EdgeDistLongDir = (momentFlangeAngle.Length - beam.GageOnFlange - 2 * beam.GageOnFlange) / 2;

			switch (momentFlangeAngle.BeamConnection)
			{
				case EConnectionStyle.Bolted:
					tgReq = Math.Max(AgReq, AgShearReq) / momentFlangeAngle.Length;
					HoleSize = momentFlangeAngle.BeamBolt.HoleWidth;
					tnReq = Anreq / (momentFlangeAngle.Length - Math.Max(0.15 * momentFlangeAngle.Length, momentFlangeAngle.ColumnBolt.NumberOfBolts * (HoleSize + ConstNum.SIXTEENTH_INCH)));
					GetRequiredThicknessForBearing(memberType, ref Fbe, ref si, ref Fbs, ref BearingCap, Ff, ref tbReq);
					t = Math.Max(tgReq, tnReq);
					tReq = NumberFun.Round(Math.Max(tbReq, t), 16);
					ef = MiscCalculationsWithReporting.MinimumEdgeDist(momentFlangeAngle.BeamBolt.BoltSize, Ff / momentFlangeAngle.BeamBolt.NumberOfBolts, beam.Material.Fu * beam.Shape.tf, momentFlangeAngle.BeamBolt.MinEdgeSheared, momentFlangeAngle.BeamBolt.HoleLength, momentFlangeAngle.BeamBolt.HoleType);
					FuT = momentFlangeAngle.Material.Fu * ConstNum.QUARTER_INCH;

					ea = MiscCalculationsWithReporting.MinimumEdgeDist(momentFlangeAngle.BeamBolt.BoltSize, Ff / momentFlangeAngle.BeamBolt.NumberOfBolts, FuT, momentFlangeAngle.BeamBolt.MinEdgeRolled, momentFlangeAngle.BeamBolt.HoleLength, momentFlangeAngle.BeamBolt.HoleType);

					if (!beam.WinConnect.Beam.BoltEdgeDistanceOnFlange_User)
						beam.WinConnect.Beam.BoltEdgeDistanceOnFlange = Math.Max(ef, beam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
					momentFlangeAngle.AngleLong = momentFlangeAngle.BeamBolt.SpacingLongDir * (momentFlangeAngle.BeamBolt.NumberOfRows - 1) + Math.Max(ea, momentFlangeAngle.BeamBolt.EdgeDistTransvDir) + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + beam.EndSetback;
					break;
				case EConnectionStyle.Welded:
					weldlength = Math.Min(beam.Shape.bf, momentFlangeAngle.Length);
					tgReq = AgReq / weldlength;
					w = NumberFun.Round(Ff / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * weldlength), 16);
					if (w >= ConstNum.QUARTER_INCH)
						tReq = w + ConstNum.SIXTEENTH_INCH;
					else
						tReq = w;
					if (!momentFlangeAngle.BeamWeldSize_User)
						momentFlangeAngle.BeamWeldSize = w;
					momentFlangeAngle.AngleLong = 4;
					break;
			}

			tnReqSup = AnShearReq / (momentFlangeAngle.Length - momentFlangeAngle.ColumnBolt.NumberOfBolts * (momentFlangeAngle.ColumnBolt.HoleLength + ConstNum.SIXTEENTH_INCH));
			thicknessMin = Math.Max(tReq_BlockShear, Math.Max(tReq, tnReqSup));

			if (momentFlangeAngle.BeamBolt.NumberOfRows == 4)
				momentFlangeAngle.BeamBolt.EdgeDistLongDir = (momentFlangeAngle.Length - beam.GageOnFlange) - 2 * column.GageOnFlange;
			else
				momentFlangeAngle.BeamBolt.EdgeDistLongDir = (momentFlangeAngle.Length - beam.GageOnFlange) / 2;

			foreach (var angle in CommonDataStatic.ShapesSingleAngle)
			{
				Shape tempAngle;
				if (momentFlangeAngle.Angle_User)
					tempAngle = momentFlangeAngle.Angle.ShallowCopy();
				else
					tempAngle = angle.Value.ShallowCopy();

				if (tempAngle.b < momentFlangeAngle.AngleLong && !momentFlangeAngle.Angle_User)
					continue;

				if (momentFlangeAngle.BeamConnection == EConnectionStyle.Bolted)
				{
					Ag = momentFlangeAngle.Length * tempAngle.t;
					An = (momentFlangeAngle.Length - momentFlangeAngle.ColumnBolt.NumberOfBolts * (HoleSize + ConstNum.SIXTEENTH_INCH)) * tempAngle.t;
				}
				else
				{
					Ag = momentFlangeAngle.Length * tempAngle.t;
					An = weldlength * tempAngle.t;
				}
				if ((tempAngle.t < thicknessMin || An < Anreq || Ag < AgReq) && !momentFlangeAngle.Angle_User)
					continue;

				if (momentFlangeAngle.BeamConnection == EConnectionStyle.Welded)
				{
					k = 0.65;
					beam.WinConnect.Fema.L = tempAngle.d;
					rg = tempAngle.t / Math.Sqrt(12);
					kL_r = (k * beam.WinConnect.Fema.L / rg);
					Fcr = CommonCalculations.BucklingStress(kL_r, momentFlangeAngle.Material.Fy, false);

					FiRn = ConstNum.FIOMEGA0_9N * Fcr * Ag;
				}
				else
				{
					k = 0.65;
					beam.WinConnect.Fema.L = (tempAngle.d - momentFlangeAngle.BeamBolt.EdgeDistTransvDir - (momentFlangeAngle.ColumnBolt.NumberOfBolts - 1) * momentFlangeAngle.BeamBolt.SpacingLongDir);
					rg = tempAngle.t / Math.Sqrt(12);
					kL_r = (k * beam.WinConnect.Fema.L / rg);

					Fcr = CommonCalculations.BucklingStress(kL_r, momentFlangeAngle.Material.Fy, false);

					FiRn = ConstNum.FIOMEGA0_9N * Fcr * Ag;
				}

				switch (momentFlangeAngle.ColumnConnection)
				{
					case EConnectionStyle.Bolted:
						AngleGage = CommonCalculations.AngleStandardGage(0, tempAngle.d);
						if (!momentFlangeAngle.ColumnBolt.EdgeDistTransvDir_User)
							momentFlangeAngle.ColumnBolt.EdgeDistTransvDir = tempAngle.d - AngleGage;
						B = (AngleGage - tempAngle.t / 2);
						if (!momentFlangeAngle.ColumnBolt.SpacingLongDir_User)
							momentFlangeAngle.ColumnBolt.SpacingLongDir = momentFlangeAngle.Length / momentFlangeAngle.ColumnBolt.NumberOfBolts;
						momentFlangeAngle.ColumnBolt.NumberOfBolts = momentFlangeAngle.ColumnBolt.NumberOfBolts;
						ta = MiscCalculationsWithReporting.HangerStrength(beam.MemberType, momentFlangeAngle.ColumnBolt.HoleWidth, momentFlangeAngle.Material.Fu, tempAngle.t, momentFlangeAngle.ColumnBolt.EdgeDistTransvDir, B, Ball, 0, false);
						TensionCap = momentFlangeAngle.ColumnBolt.NumberOfBolts * ta;
						ShearYielding = ConstNum.FIOMEGA1_0N * 0.6 * momentFlangeAngle.Material.Fy * momentFlangeAngle.Length * tempAngle.t;
						ShearRupture = ConstNum.FIOMEGA0_75N * 0.6 * momentFlangeAngle.Material.Fu * tempAngle.t * (momentFlangeAngle.Length - momentFlangeAngle.ColumnBolt.NumberOfBolts * (momentFlangeAngle.ColumnBolt.HoleLength + ConstNum.SIXTEENTH_INCH));
						ShearStrength = Math.Min(ShearYielding, ShearRupture);
						break;
					case EConnectionStyle.Welded:
						plasticmoment = Math.Pow(tempAngle.t, 2) * momentFlangeAngle.Length * momentFlangeAngle.Material.Fy / 4;
						TensionCap = plasticmoment / (tempAngle.b - tempAngle.kdet);
						ShearYielding = ConstNum.FIOMEGA1_0N * 0.6 * momentFlangeAngle.Material.Fy * momentFlangeAngle.Length * tempAngle.t;
						ShearRupture = ConstNum.FIOMEGA0_75N * 0.6 * momentFlangeAngle.Material.Fu * tempAngle.t * momentFlangeAngle.Length;
						ShearStrength = Math.Min(ShearYielding, ShearRupture);
						break;
				}
				if ((TensionCap < Ff || ShearStrength < Ff) && !momentFlangeAngle.Angle_User)
					continue;

				if (momentFlangeAngle.BeamConnection == EConnectionStyle.Welded)
				{
					Mw = CommonCalculations.MinimumWeld(tempAngle.t, beam.Shape.tf);
					if (tempAngle.t < ConstNum.QUARTER_INCH)
						maxw = tempAngle.t;
					else
						maxw = tempAngle.t - ConstNum.SIXTEENTH_INCH;

					if (!momentFlangeAngle.BeamWeldSize_User)
						momentFlangeAngle.BeamWeldSize = Math.Max(momentFlangeAngle.BeamWeldSize, Mw);
					if (!momentFlangeAngle.BeamWeldSize_User)
						momentFlangeAngle.BeamWeldSize = Math.Min(momentFlangeAngle.BeamWeldSize, maxw);
					capacity = ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentFlangeAngle.BeamWeldSize * momentFlangeAngle.Length;
					if (capacity < Ff && !momentFlangeAngle.Angle_User)
						continue;
				}
				if (momentFlangeAngle.ColumnConnection == EConnectionStyle.Welded)
				{
					weldlength = Math.Min(momentFlangeAngle.Length, column.Shape.bf);
					wreq = Ff / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * weldlength);
					Mw = CommonCalculations.MinimumWeld(tempAngle.t, column.Shape.tf);
					if (tempAngle.t < ConstNum.QUARTER_INCH)
						maxw = tempAngle.t;
					else
						maxw = tempAngle.t - ConstNum.SIXTEENTH_INCH;

					w = Math.Max(wreq, Mw);
					if (!momentFlangeAngle.ColumnWeldSize_User)
						momentFlangeAngle.ColumnWeldSize = NumberFun.Round(Math.Min(maxw, w), 16);
					capacity = ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentFlangeAngle.ColumnWeldSize * weldlength;
					if (capacity < Ff && !momentFlangeAngle.Angle_User)
						continue;
				}

				momentFlangeAngle.BeamBolt.EdgeDistTransvDir = momentFlangeAngle.AngleLong - (beam.EndSetback + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + (momentFlangeAngle.BeamBolt.NumberOfRows - 1) * momentFlangeAngle.BeamBolt.SpacingLongDir);

				// We reached the end, which means we have a good angle
				momentFlangeAngle.Angle = tempAngle.ShallowCopy();
				break;
			}

			momentFlangeAngle.BeamBolt.EdgeDistTransvDir = momentFlangeAngle.AngleLong - (beam.EndSetback + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + (momentFlangeAngle.BeamBolt.NumberOfRows - 1) * momentFlangeAngle.BeamBolt.SpacingLongDir);

			Ff = Math.Abs(beam.P / 2) + Math.Abs(beam.Moment / beam.Shape.d);

			Reporting.AddMainHeader("Moment Connection Using Angles:");
			Reporting.AddLine("Top and Bottom Angles: " + momentFlangeAngle.Angle.Name);
			Reporting.AddLine("Angle Material: " + momentFlangeAngle.Material.Name);
			Reporting.AddLine("Support Side: " + momentFlangeAngle.ColumnConnection);
			Reporting.AddLine("Beam Side: " + momentFlangeAngle.BeamConnection);
			Reporting.AddLine(string.Empty);

			Reporting.AddLine("Beam Axial Load (P) = " + beam.P + ConstUnit.Force);
            Reporting.AddLine("Beam Moment (M) = " + (beam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);      // Changed COEFFICIENT_ONE_THOUSAND to COEFFICIENT_ONE_THOUSAND   -RM
			Reporting.AddLine("Flange Force (F) = P / 2 + M / d");
			Reporting.AddLine("= " + (Math.Abs(beam.P) + " / 2 + " + Math.Abs(beam.Moment) + " / " + beam.Shape.d));
			Reporting.AddLine("= " + Ff + ConstUnit.Force);
			Reporting.AddLine(string.Empty);

			if (momentFlangeAngle.ColumnConnection == EConnectionStyle.Bolted)
			{
				Reporting.AddGoToHeader("Support Side Bolts: " + momentFlangeAngle.ColumnBolt.NumberOfBolts + " Bolts " + momentFlangeAngle.ColumnBolt.BoltName);
                Reporting.AddLine(string.Empty);

				Reporting.AddLine("Bolt Holes on Angle: " + momentFlangeAngle.BeamBolt.HoleWidth + ConstUnit.Length + "  Vert. X " + momentFlangeAngle.BeamBolt.HoleLength + ConstUnit.Length + "  Horiz.");
				Reporting.AddLine("Bolt Holes on Support: " + momentFlangeAngle.ColumnBolt.HoleWidth + ConstUnit.Length + "  Vert. X " + momentFlangeAngle.ColumnBolt.HoleLength + ConstUnit.Length + "  Horiz.");
                Reporting.AddLine(string.Empty);

				elmin = momentFlangeAngle.ColumnBolt.MinEdgeSheared;

				Reporting.AddLine("Bolt Distance to End (el):");
				if (momentFlangeAngle.ColumnBolt.EdgeDistLongDir >= elmin)
					Reporting.AddLine("= " + momentFlangeAngle.ColumnBolt.EdgeDistLongDir + " >= " + elmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + momentFlangeAngle.ColumnBolt.EdgeDistLongDir + " << " + elmin + ConstUnit.Length + " (NG)");

                Reporting.AddLine(string.Empty);

				etmin = momentFlangeAngle.ColumnBolt.MinEdgeRolled;

				Reporting.AddLine("Distance to Horizontal Edge (et):");
				if (momentFlangeAngle.ColumnBolt.EdgeDistTransvDir >= etmin)
					Reporting.AddLine("= " + momentFlangeAngle.ColumnBolt.EdgeDistTransvDir + " >= " + etmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + momentFlangeAngle.ColumnBolt.EdgeDistTransvDir + " << " + etmin + ConstUnit.Length + " (NG)");

				AnglGage1 = momentFlangeAngle.Angle.t + momentFlangeAngle.ColumnBolt.BoltSize + ConstNum.HALF_INCH;
				AnglGage2 = momentFlangeAngle.ColumnBolt.BoltSize + momentFlangeAngle.Angle.kdet;
				AngleGage = Math.Max(AnglGage1, AnglGage2);
				if (AngleGage > momentFlangeAngle.Angle.d - momentFlangeAngle.ColumnBolt.EdgeDistTransvDir)
					Reporting.AddLine("Check bolt gage on angle. (NG)");

				Reporting.AddGoToHeader("Angle OSL Tension Strength:");

                // For some reason, the following 3 lines were removed on 5/27. They are necessary, so I added them back in.    06/09/2015  -RM
                Ball = BoltsForTension.CalcBoltsForTension(memberType, beam.WinConnect.MomentFlangeAngle.ColumnBolt, Ff, 0, beam.WinConnect.MomentFlangeAngle.ColumnBolt.NumberOfBolts, true);
                B = beam.WinConnect.MomentFlangeAngle.ShortLeg - beam.WinConnect.MomentFlangeAngle.Angle.t / 2 - beam.WinConnect.MomentFlangeAngle.ColumnBolt.EdgeDistTransvDir;
                double a = Math.Min(beam.WinConnect.MomentFlangeAngle.ColumnBolt.EdgeDistTransvDir, 1.25 * B); 
				s = momentFlangeAngle.ColumnBolt.SpacingLongDir;
				momentFlangeAngle.ColumnBolt.SpacingLongDir = momentFlangeAngle.Length / momentFlangeAngle.ColumnBolt.NumberOfBolts;
				momentFlangeAngle.ColumnBolt.NumberOfBolts = momentFlangeAngle.ColumnBolt.NumberOfBolts;
				ta = MiscCalculationsWithReporting.HangerStrength(beam.MemberType, momentFlangeAngle.ColumnBolt.HoleWidth, momentFlangeAngle.Material.Fu, momentFlangeAngle.Angle.t, a, B, Ball, 0, true);
				TensionCap = momentFlangeAngle.ColumnBolt.NumberOfBolts * ta;
				momentFlangeAngle.ColumnBolt.SpacingLongDir = s;

				Reporting.AddHeader(ConstString.DES_OR_ALLOWABLE + " Tension Strength:");
				if (TensionCap >= Ff)
					Reporting.AddCapacityLine("= n * " + ConstString.PHI + "Tn = " + momentFlangeAngle.ColumnBolt.NumberOfBolts + " * " + ta + " = " + TensionCap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / TensionCap, "Tension Strength", memberType);
				else
					Reporting.AddCapacityLine("= n * " + ConstString.PHI + "Tn = " + momentFlangeAngle.ColumnBolt.NumberOfBolts + " * " + ta + " = " + TensionCap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / TensionCap, "Tension Strength", memberType);

				Reporting.AddHeader("Angle Shear Strength:");
				ShearYielding = ConstNum.FIOMEGA1_0N * 0.6 * momentFlangeAngle.Material.Fy * momentFlangeAngle.Length * momentFlangeAngle.Angle.t;
                Reporting.AddLine(string.Empty);
				Reporting.AddLine("Shear Yielding " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ": ");
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstNum.FIOMEGA1_0N + " * 0.6 * Fy * L * t");
				Reporting.AddLine("= " + ConstString.FIOMEGA1_0 + " * 0.6 * " + momentFlangeAngle.Material.Fy + " * " + momentFlangeAngle.Length + " * " + momentFlangeAngle.Angle.t);
				if (ShearYielding >= Ff)
					Reporting.AddCapacityLine("= " + ShearYielding + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / ShearYielding, "Angle Shear Yielding Strength", memberType);
				else
					Reporting.AddCapacityLine("= " + ShearYielding + " << " + Ff + ConstUnit.Force + " (NG)", Ff / ShearYielding, "Angle Shear Yielding Strength", memberType);

                Reporting.AddLine(string.Empty);
				Reporting.AddLine("Shear Rupture " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ": ");
                ShearRupture = ConstNum.FIOMEGA0_75N * 0.6 * momentFlangeAngle.Material.Fu * momentFlangeAngle.Angle.t * (momentFlangeAngle.Length - momentFlangeAngle.ColumnBolt.NumberOfBolts * (momentFlangeAngle.ColumnBolt.HoleLength + ConstNum.SIXTEENTH_INCH));
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu * t * (L - n * (dh + " + ConstNum.SIXTEENTH_INCH + "))");
                Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.6 * " + momentFlangeAngle.Material.Fu + " * " + momentFlangeAngle.Angle.t + " * (" + momentFlangeAngle.Length + " - " + momentFlangeAngle.ColumnBolt.NumberOfBolts + " * (" + (momentFlangeAngle.ColumnBolt.HoleLength + ConstNum.SIXTEENTH_INCH) + "))");
				if (ShearRupture >= Ff)
					Reporting.AddCapacityLine("= " + ShearRupture + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / ShearRupture, "Angle Shear Rupture Strength", memberType);
				else
					Reporting.AddCapacityLine("= " + ShearRupture + " << " + Ff + ConstUnit.Force + " (NG)", Ff / ShearRupture, "Angle Shear Rupture Strength", memberType);
			}
			else
			{
				Mw = CommonCalculations.MinimumWeld(momentFlangeAngle.Angle.t, column.Shape.tf);
				if (momentFlangeAngle.Angle.t >= ConstNum.QUARTER_INCH)
					maxw = momentFlangeAngle.Angle.t - ConstNum.SIXTEENTH_INCH;
				else
					maxw = momentFlangeAngle.Angle.t;

				Reporting.AddHeader("Angle OSL Weld:");
				if (momentFlangeAngle.ColumnWeldSize >= Mw)
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentFlangeAngle.ColumnWeldSize) + " >= Minimum Weld Size = " + CommonCalculations.WeldSize(Mw) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentFlangeAngle.ColumnWeldSize) + " << Minimum Weld Size = " + CommonCalculations.WeldSize(Mw) + ConstUnit.Length + " (NG)");

				if (momentFlangeAngle.ColumnWeldSize <= maxw)
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentFlangeAngle.ColumnWeldSize) + " <= Maximum Weld Size = " + CommonCalculations.WeldSize(maxw) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentFlangeAngle.ColumnWeldSize) + " >> Maximum Weld Size = " + CommonCalculations.WeldSize(maxw) + ConstUnit.Length + " (NG)");

				weldlength = Math.Min(column.Shape.bf, momentFlangeAngle.Length);
				Cap = 0.2121 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentFlangeAngle.ColumnWeldSize * weldlength;

				Reporting.AddLine("Capacity = 0.2121 * Fexx * w * L");
				Reporting.AddLine("= 0.2121 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + momentFlangeAngle.ColumnWeldSize + " * " + weldlength);
				if (Cap >= Ff)
					Reporting.AddLine("= " + Cap + " >= " + Ff + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + Cap + " << " + Ff + ConstUnit.Force + " (NG)");

				Reporting.AddHeader("Angle OSL Tension Capacity:");
				Cap = 0.125 * Math.Pow(momentFlangeAngle.Angle.t, 2) * momentFlangeAngle.Length * momentFlangeAngle.Material.Fy / (momentFlangeAngle.Angle.b - momentFlangeAngle.Angle.t);
				Reporting.AddHeader("Tension Capacity of Angle Leg:");
				Reporting.AddLine("= 0.125 * t² * L * Fy / (l - t)");
				Reporting.AddLine("= 0.125 * " + momentFlangeAngle.Angle.t + "² * " + momentFlangeAngle.Length + " * " + momentFlangeAngle.Material.Fy + " / (" + momentFlangeAngle.Angle.b + " - " + momentFlangeAngle.Angle.t + ")");
				if (Cap >= Ff)
					Reporting.AddLine("= " + Cap + " >= " + Ff + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + Cap + " << " + Ff + ConstUnit.Force + " (NG)");
			}
			if (momentFlangeAngle.BeamConnection == EConnectionStyle.Bolted)
			{
				Reporting.AddGoToHeader("Beam Side Bolts:");
				Reporting.AddLine(momentFlangeAngle.BeamBolt.NumberOfBolts + " Bolts " + momentFlangeAngle.BeamBolt.BoltName);
				Reporting.AddLine("Bolt Holes on Angle: " + momentFlangeAngle.BeamBolt.HoleWidth + ConstUnit.Length + "  Trans. X " + momentFlangeAngle.BeamBolt.HoleLength + ConstUnit.Length + " Long.");
				Reporting.AddLine("Bolt Holes on Beam: " + momentFlangeAngle.BeamBolt.HoleWidth + ConstUnit.Length + "  Trans. X " + momentFlangeAngle.BeamBolt.HoleLength + ConstUnit.Length + "  Long.");
				
				elmin = momentFlangeAngle.BeamBolt.MinEdgeSheared;

                Reporting.AddLine(string.Empty);
				Reporting.AddLine("Distance to End (el):");
				if (momentFlangeAngle.BeamBolt.EdgeDistLongDir >= elmin)
					Reporting.AddLine("= " + momentFlangeAngle.BeamBolt.EdgeDistLongDir + " >= " + elmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + momentFlangeAngle.BeamBolt.EdgeDistLongDir + " << " + elmin + ConstUnit.Length + " (NG)");

				elmin = momentFlangeAngle.BeamBolt.MinEdgeRolled;

                Reporting.AddLine(string.Empty);
                Reporting.AddLine("Distance to Rolled Edge of Angle (et):");

				bsideedge = momentFlangeAngle.LongLeg - ((momentFlangeAngle.BeamBolt.NumberOfRows - 1) * momentFlangeAngle.BeamBolt.SpacingLongDir + beam.EndSetback + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange);

				if (bsideedge >= elmin)
					Reporting.AddLine("= " + bsideedge + " >= " + elmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + bsideedge + " << " + elmin + ConstUnit.Length + " (NG)");

				AnglGage1 = momentFlangeAngle.Angle.t+ momentFlangeAngle.BeamBolt.BoltSize + ConstNum.HALF_INCH;
				AnglGage2 = momentFlangeAngle.BeamBolt.BoltSize + momentFlangeAngle.Angle.kdet;
				AngleGage = Math.Max(AnglGage1, AnglGage2);
				if (AngleGage > beam.EndSetback + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange)
					Reporting.AddLine("Check bolt gage on angle. (NG)");

				MiscCalculationsWithReporting.BeamCheck(beam.MemberType);

				Reporting.AddHeader("Bolt Shear Strength:");
				Cap = momentFlangeAngle.BeamBolt.NumberOfBolts * momentFlangeAngle.BeamBolt.BoltStrength;
				if (Cap >= Ff)
					Reporting.AddCapacityLine("= n * Fv = " + momentFlangeAngle.BeamBolt.NumberOfBolts + " * " + momentFlangeAngle.BeamBolt.BoltStrength + " = " + Cap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Cap, "Bolt Shear Strength", memberType);
				else
					Reporting.AddCapacityLine("= n * Fv = " + momentFlangeAngle.BeamBolt.NumberOfBolts + " * " + momentFlangeAngle.BeamBolt.BoltStrength + " = " + Cap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Cap, "Bolt Shear Strength", memberType);

				Reporting.AddHeader("Bolt Bearing:");

                Reporting.AddLine(string.Empty);
				Reporting.AddLine("Bolt Bearing on Angle:");
                Reporting.AddLine(string.Empty);

				Fbe = CommonCalculations.EdgeBearing(momentFlangeAngle.BeamBolt.EdgeDistTransvDir, (momentFlangeAngle.BeamBolt.HoleLength), momentFlangeAngle.BeamBolt.BoltSize, momentFlangeAngle.Material.Fu, momentFlangeAngle.BeamBolt.HoleType, true);
				si = momentFlangeAngle.BeamBolt.SpacingTransvDir;
				Fbs = CommonCalculations.SpacingBearing(momentFlangeAngle.BeamBolt.SpacingTransvDir, momentFlangeAngle.BeamBolt.HoleLength, momentFlangeAngle.BeamBolt.BoltSize, momentFlangeAngle.BeamBolt.HoleType, momentFlangeAngle.Material.Fu, true);
				BearingCap = momentFlangeAngle.ColumnBolt.NumberOfBolts * (Fbe + Fbs * (momentFlangeAngle.BeamBolt.NumberOfRows - 1)) * momentFlangeAngle.Angle.t;

                Reporting.AddLine(string.Empty);
				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = nT * (Fbe + Fbs * (nL - 1)) * t");
				Reporting.AddLine("= " + momentFlangeAngle.ColumnBolt.NumberOfBolts + " * (" + Fbe + " + " + Fbs + " * (" + momentFlangeAngle.BeamBolt.NumberOfRows + " - 1)) * " + momentFlangeAngle.Angle.t);
				if (BearingCap >= Ff)
					Reporting.AddCapacityLine("= " + BearingCap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / BearingCap, "Bearing Strength", memberType);
				else
					Reporting.AddCapacityLine("= " + BearingCap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / BearingCap, "Bearing Strength", memberType);

                Reporting.AddLine(string.Empty);
				Reporting.AddLine("Bolt Bearing on Beam Flange:");
                Reporting.AddLine(string.Empty);

				Fbe = CommonCalculations.EdgeBearing(beam.WinConnect.Beam.BoltEdgeDistanceOnFlange, (momentFlangeAngle.BeamBolt.HoleLength), momentFlangeAngle.BeamBolt.BoltSize, beam.Material.Fu, momentFlangeAngle.BeamBolt.HoleType, true);
				si = momentFlangeAngle.BeamBolt.SpacingTransvDir;
				Fbs = CommonCalculations.SpacingBearing(momentFlangeAngle.BeamBolt.SpacingTransvDir, momentFlangeAngle.BeamBolt.HoleLength, momentFlangeAngle.BeamBolt.BoltSize, momentFlangeAngle.BeamBolt.HoleType, beam.Material.Fu, true);
				BearingCap = momentFlangeAngle.ColumnBolt.NumberOfBolts * (Fbe + Fbs * (momentFlangeAngle.BeamBolt.NumberOfRows - 1)) * beam.Shape.tf;

                Reporting.AddLine(string.Empty);
				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Bearing Strength = nT * (Fbe + Fbs * (nL - 1)) * t");
				Reporting.AddLine("= " + momentFlangeAngle.ColumnBolt.NumberOfBolts + " * (" + Fbe + " + " + Fbs + " * (" + momentFlangeAngle.BeamBolt.NumberOfRows + " - 1)) * " + beam.Shape.tf);
				if (BearingCap >= Ff)
					Reporting.AddCapacityLine("= " + BearingCap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / BearingCap, "Bolt Bearing on Beam Flange", memberType);
				else
					Reporting.AddCapacityLine("= " + BearingCap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / BearingCap, "Bolt Bearing on Beam Flange", memberType);

				Reporting.AddHeader("Angle Tension Strength");

                Reporting.AddLine(string.Empty);
				Reporting.AddLine("Tension yielding of the angle:");
                Reporting.AddLine(string.Empty);

				FiRn = ConstNum.FIOMEGA0_9N * momentFlangeAngle.Material.Fy * momentFlangeAngle.Angle.t * momentFlangeAngle.Length;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * t * L");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + momentFlangeAngle.Material.Fy + " * " + momentFlangeAngle.Angle.t + " * " + momentFlangeAngle.Length);
				if (FiRn >= Ff)
					Reporting.AddCapacityLine("= " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / FiRn, "Tension yielding of the angle", memberType);
				else
					Reporting.AddCapacityLine("= " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / FiRn, "Tension yielding of the angle", memberType);

                Reporting.AddLine(string.Empty);
				Reporting.AddLine("Tension rupture of the angle:");
                Reporting.AddLine(string.Empty);

				Lg = momentFlangeAngle.Length;
				FiRn = ConstNum.FIOMEGA0_75N * momentFlangeAngle.Material.Fu * momentFlangeAngle.Angle.t * (momentFlangeAngle.Length - Math.Max(0.15F * Lg, momentFlangeAngle.BeamBolt.NumberOfRows * (momentFlangeAngle.BeamBolt.HoleWidth + ConstNum.SIXTEENTH_INCH)));
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Fu * t * (L - Max(0.15 * Ag, n * (dh + " + ConstNum.SIXTEENTH_INCH + ")))");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + momentFlangeAngle.Material.Fu + " * " + momentFlangeAngle.Angle.t + " * (" + momentFlangeAngle.Length + " - Max(0.15 * " + Ag + ", " + momentFlangeAngle.BeamBolt.NumberOfRows + " * (" + momentFlangeAngle.BeamBolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")))");
				if (FiRn >= Ff)
					Reporting.AddCapacityLine("= " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / FiRn, "Tension rupture of the angle", memberType);
				else
					Reporting.AddCapacityLine("= " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / FiRn, "Tension rupture of the angle", memberType);

                Reporting.AddLine(string.Empty);
				Reporting.AddLine("Block shear rupture of the angle:");
                Reporting.AddLine(string.Empty);
				Reporting.AddLine("Inside Block:");
                Reporting.AddLine(string.Empty);

				if (momentFlangeAngle.BeamBolt.NumberOfRows == 4)
				{
                    AgTension = (beam.GageOnFlange + 2 * beam.GageOnFlange) * momentFlangeAngle.Angle.t; // case 2
					Reporting.AddLine("Agt = (g1 + 2 * g2) * t");
                    Reporting.AddLine("= (" + beam.GageOnFlange + " + 2 * " + beam.GageOnFlange + ") * " + momentFlangeAngle.Angle.t);
					Reporting.AddLine("= " + AgTension + ConstUnit.Area);
                    Reporting.AddLine(string.Empty);

                    AnTension = AgTension - 3 * (momentFlangeAngle.BeamBolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * momentFlangeAngle.Angle.t;
					Reporting.AddLine("Ant = Agt - (n - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
                    Reporting.AddLine("= " + AgTension + " - 3 * (" + (momentFlangeAngle.BeamBolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + momentFlangeAngle.Angle.t);
					Reporting.AddLine("= " + AnTension + ConstUnit.Area);
                    Reporting.AddLine(string.Empty);
				}
				else
				{
                    AgTension = beam.GageOnFlange * momentFlangeAngle.Angle.t;
                    Reporting.AddLine("Agt = g * t = " + beam.GageOnFlange + " * " + momentFlangeAngle.Angle.t);
					Reporting.AddLine("= " + AgTension + ConstUnit.Area);
                    Reporting.AddLine(string.Empty);

                    AnTension = AgTension - (momentFlangeAngle.BeamBolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * momentFlangeAngle.Angle.t;
					Reporting.AddLine("Ant = Agt - (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
                    Reporting.AddLine("= " + AgTension + " - (" + (momentFlangeAngle.BeamBolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + momentFlangeAngle.Angle.t);
					Reporting.AddLine("= " + AnTension + ConstUnit.Area);
                    Reporting.AddLine(string.Empty);
				}
                AgShear = 2 * ((momentFlangeAngle.ColumnBolt.NumberOfBolts - 1) * momentFlangeAngle.BeamBolt.SpacingTransvDir + momentFlangeAngle.BeamBolt.EdgeDistTransvDir) * momentFlangeAngle.Angle.t;
				Reporting.AddLine("Agv = 2 * ((nl - 1) * s + Le) * t");
				Reporting.AddLine("= 2 * ((" + momentFlangeAngle.ColumnBolt.NumberOfBolts + " - 1) * " + momentFlangeAngle.BeamBolt.SpacingTransvDir + " + " + momentFlangeAngle.BeamBolt.EdgeDistTransvDir + ") * " + momentFlangeAngle.Angle.t);
				Reporting.AddLine("= " + AgShear + ConstUnit.Area);
                Reporting.AddLine(string.Empty);

				AnShear = AgShear - 2 * (momentFlangeAngle.ColumnBolt.NumberOfBolts - 0.5) * (momentFlangeAngle.BeamBolt.HoleLength + ConstNum.SIXTEENTH_INCH) * momentFlangeAngle.Angle.t;
				Reporting.AddLine("Anv = Agv - 2 * (nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("=" + AgShear + " - 2 * (" + momentFlangeAngle.ColumnBolt.NumberOfBolts + " - 0.5) * (" + (momentFlangeAngle.BeamBolt.HoleLength + ConstNum.SIXTEENTH_INCH) + ") * " + momentFlangeAngle.Angle.t);
				Reporting.AddLine("= " + AnShear + ConstUnit.Area);
                Reporting.AddLine(string.Empty);

				FiRn = MiscCalculationsWithReporting.BlockShearNew(momentFlangeAngle.Material.Fu, AnShear, 1, AnTension, AgShear, momentFlangeAngle.Material.Fy, 0, Ff, true);

                Reporting.AddLine(string.Empty);
				Reporting.AddLine("Outside Blocks:");
                Reporting.AddLine(string.Empty);

				if (momentFlangeAngle.BeamBolt.NumberOfRows == 4)
				{
					AgTension = -beam.GageOnFlange * momentFlangeAngle.Angle.t; // case 6

					Reporting.AddLine("Agt = (L - g1) * t");
					Reporting.AddLine("= (" + momentFlangeAngle.Length + " - " + beam.GageOnFlange + ") * " + momentFlangeAngle.Angle.t);
					Reporting.AddLine("= " + AgTension + ConstUnit.Area);
                    Reporting.AddLine(string.Empty);

					AnTension = AgTension - 3 * (momentFlangeAngle.BeamBolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * momentFlangeAngle.Angle.t;
					Reporting.AddLine("Ant = Agt - (n - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("= " + AgTension + " - 3 * (" + (momentFlangeAngle.BeamBolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + momentFlangeAngle.Angle.t);
					Reporting.AddLine("= " + AnTension + ConstUnit.Area);
                    Reporting.AddLine(string.Empty);
				}
				else
				{
					AgTension = (momentFlangeAngle.Length - beam.GageOnFlange) * momentFlangeAngle.Angle.t;
                    Reporting.AddLine("Agt = (L - g1) * t");
                    Reporting.AddLine("= (" + momentFlangeAngle.Length + " - " + beam.GageOnFlange + ") * " + momentFlangeAngle.Angle.t);
					Reporting.AddLine("= " + AgTension + ConstUnit.Area);
                    Reporting.AddLine(string.Empty);

					AnTension = AgTension - (momentFlangeAngle.BeamBolt.HoleWidth + ConstNum.SIXTEENTH_INCH) * momentFlangeAngle.Angle.t;
					Reporting.AddLine("Ant = Agt - (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("= " + AgTension + " - (" + (momentFlangeAngle.BeamBolt.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + momentFlangeAngle.Angle.t);
					Reporting.AddLine("= " + AnTension + ConstUnit.Area);
                    Reporting.AddLine(string.Empty);
				}

				AgShear = 2 * ((momentFlangeAngle.ColumnBolt.NumberOfBolts - 1) * momentFlangeAngle.BeamBolt.SpacingTransvDir + momentFlangeAngle.BeamBolt.EdgeDistTransvDir) * momentFlangeAngle.Angle.t;
				Reporting.AddLine("Agv = 2 * ((nl - 1) * s + Le) * t");
				Reporting.AddLine("= 2 * ((" + momentFlangeAngle.ColumnBolt.NumberOfBolts + " - 1) * " + momentFlangeAngle.BeamBolt.SpacingTransvDir + " + " + momentFlangeAngle.BeamBolt.EdgeDistTransvDir + ") * " + momentFlangeAngle.Angle.t);
				Reporting.AddLine("= " + AgShear + ConstUnit.Area);
                Reporting.AddLine(string.Empty);
				
				AnShear = AgShear - 2 * (momentFlangeAngle.ColumnBolt.NumberOfBolts - 0.5) * (momentFlangeAngle.BeamBolt.HoleLength + ConstNum.SIXTEENTH_INCH) * momentFlangeAngle.Angle.t;
				Reporting.AddLine("Anv = Agv - 2 * (nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("=" + AgShear + " - 2 * (" + momentFlangeAngle.ColumnBolt.NumberOfBolts + " - 0.5) * (" + (momentFlangeAngle.BeamBolt.HoleLength + ConstNum.SIXTEENTH_INCH) + ") * " + momentFlangeAngle.Angle.t);
				Reporting.AddLine("= " + AnShear + ConstUnit.Area);
                Reporting.AddLine(string.Empty);

				FiRn = MiscCalculationsWithReporting.BlockShearNew(momentFlangeAngle.Material.Fu, AnShear, 1, AnTension, AgShear, momentFlangeAngle.Material.Fy, 0, Ff, true);
			}
			else
			{
				weldlength = Math.Min(beam.Shape.bf, momentFlangeAngle.Length);
				w = NumberFun.Round(Ff / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * weldlength), 16); // Int(-16 * Ff / (FiOmega0_75N *0.4242 * Fexx * weldlength)) / 16
				if (w >= ConstNum.QUARTER_INCH)
					tReq = w - ConstNum.SIXTEENTH_INCH;
				else
					tReq = w;
				Mw = CommonCalculations.MinimumWeld(momentFlangeAngle.Angle.t, beam.Shape.tf);
				if (momentFlangeAngle.Angle.t >= ConstNum.QUARTER_INCH)
					maxw = momentFlangeAngle.Angle.t - ConstNum.SIXTEENTH_INCH;
				else
					maxw = momentFlangeAngle.Angle.t;

				if (!momentFlangeAngle.BeamWeldSize_User)
					momentFlangeAngle.BeamWeldSize = Math.Max(w, Mw);

				Reporting.AddGoToHeader("Angle Beam Side Weld:");
				if (momentFlangeAngle.BeamWeldSize >= Mw)
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentFlangeAngle.BeamWeldSize) + " >= Minimum Weld Size = " + CommonCalculations.WeldSize(Mw) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentFlangeAngle.BeamWeldSize) + " << Minimum Weld Size = " + CommonCalculations.WeldSize(Mw) + ConstUnit.Length + " (NG)");

				if (momentFlangeAngle.BeamWeldSize <= maxw)
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentFlangeAngle.BeamWeldSize) + " <= Maximum Weld Size = " + CommonCalculations.WeldSize(maxw) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentFlangeAngle.BeamWeldSize) + " >> Maximum Weld Size = " + CommonCalculations.WeldSize(maxw) + ConstUnit.Length + " (NG)");

				Cap = ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentFlangeAngle.BeamWeldSize * weldlength;
				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " = " + ConstString.FIOMEGA0_75 + " * 0.4242 * Fexx * w * L");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4242 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + momentFlangeAngle.BeamWeldSize + " * " + weldlength);
				if (Cap >= Ff)
					Reporting.AddCapacityLine("= " + Cap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Cap, "Angle Beam Side Weld", memberType);
				else
					Reporting.AddCapacityLine("= " + Cap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Cap, "Angle Beam Side Weld", memberType);

				Reporting.AddHeader("Angle Beam Side Leg Tension:");
				Reporting.AddHeader("Tension yielding of the angle:");

				FiRn = ConstNum.FIOMEGA0_9N * momentFlangeAngle.Material.Fy * momentFlangeAngle.Angle.t * momentFlangeAngle.Length;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * t * L");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + momentFlangeAngle.Material.Fy + " * " + momentFlangeAngle.Angle.t + " * " + momentFlangeAngle.Length);
				if (FiRn >= Ff)
					Reporting.AddCapacityLine("= " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / FiRn, "Tension yielding of the angle", memberType);
				else
					Reporting.AddCapacityLine("= " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / FiRn, "Tension yielding of the angle", memberType);

				Reporting.AddHeader("Tension rupture of the angle:");
				FiRn = ConstNum.FIOMEGA0_75N * momentFlangeAngle.Material.Fu * momentFlangeAngle.Angle.t * weldlength;
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Fu * t * Lw");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + momentFlangeAngle.Material.Fu + " * " + momentFlangeAngle.Angle.t + " * " + weldlength);
				if (FiRn >= Ff)
					Reporting.AddCapacityLine("= " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / FiRn, "Tension rupture of the angle", memberType);
				else
					Reporting.AddCapacityLine("= " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / FiRn, "Tension rupture of the angle", memberType);
			}

			Reporting.AddHeader("Buckling of the Angle:");
			if (momentFlangeAngle.BeamConnection == EConnectionStyle.Welded)
				beam.WinConnect.Fema.L = momentFlangeAngle.AngleLong;
			else
				beam.WinConnect.Fema.L = (beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + beam.EndSetback);

			k = 0.65;
			rg = momentFlangeAngle.Angle.t / Math.Sqrt(12);
			kL_r = (k * beam.WinConnect.Fema.L / rg);
			Ag = momentFlangeAngle.Angle.t * momentFlangeAngle.Length;

            Reporting.AddLine(string.Empty);
			Reporting.AddLine("L = " + beam.WinConnect.Fema.L + ConstUnit.Length + ", K = 0.65, r = " + rg + ConstUnit.Length + ", KL/r = " + kL_r);
            Reporting.AddLine(string.Empty);

			Fcr = CommonCalculations.BucklingStress(kL_r, momentFlangeAngle.Material.Fy, true);

			FiRn = ConstNum.FIOMEGA0_9N * Fcr * Ag;
			if (FiRn >= Ff)
				Reporting.AddLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + Ag + " = " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + Ag + " = " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)");

            Reporting.AddLine(string.Empty);
			Reporting.AddLine("Note: Descon does not check the moment versus rotation behaviour");
			Reporting.AddLine("of the connection. If your particular application requires this check,");
			Reporting.AddLine("you must do it outside the program.");
		}

		private static void GetGageOnBeam(EMemberType memberType, double ref_migname)
		{
			double Gage1 = 0;
			double Gage2 = 0;
			double gagemin = 0;
			double gagemax = 0;
			double outgage = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var momentFlangeAngle = beam.WinConnect.MomentFlangeAngle;

			Gage1 = 2 * (beam.Shape.k1 + momentFlangeAngle.BeamBolt.BoltSize);
			Gage2 = beam.Shape.tw + 2 * (momentFlangeAngle.BeamBolt.BoltSize + ConstNum.HALF_INCH);
			gagemin = Math.Max(Gage2, Gage1);
			gagemax = beam.Shape.bf - 2 * ref_migname;
			if (beam.GageOnFlange < gagemin)
				beam.GageOnFlange = gagemin;
			if (beam.GageOnFlange > gagemax)
				beam.GageOnFlange = gagemax;

			outgage = (gagemax - beam.GageOnFlange) / 2;
			if (outgage >= 2.67 * momentFlangeAngle.BeamBolt.BoltSize)
				beam.GageOnFlange = outgage;
		}

		private static void GetNumberofColumnBolts(EMemberType memberType, double Ff, ref double Ball)
		{
			int n;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var momentFlangeAngle = beam.WinConnect.MomentFlangeAngle;

			n = (int)Math.Max(2, BoltsForTension.CalcBoltsForTension(memberType, momentFlangeAngle.ColumnBolt, Ff, 0, 0, false));

			if (n > 2 && beam.GageOnColumn == 0 || n > 4)
				Reporting.AddLine("Too many bolts required on column side. (NG)");
			else
			{
				if (n > 2 || momentFlangeAngle.ColumnBolt.NumberOfBolts == 4)
					n = 4;
			}
			momentFlangeAngle.ColumnBolt.NumberOfBolts = n;
			Ball = BoltsForTension.CalcBoltsForTension(memberType, momentFlangeAngle.ColumnBolt, Ff, 0, momentFlangeAngle.ColumnBolt.NumberOfBolts, false);
		}

		private static void GetRequiredThicknessForBearing(EMemberType memberType, ref double Fbe, ref double si, ref double Fbs, ref double BearingCap, double Ff, ref double tbReq)
		{
			var momentFlangeAngle = CommonDataStatic.DetailDataDict[memberType].WinConnect.MomentFlangeAngle;

			Fbe = CommonCalculations.EdgeBearing(momentFlangeAngle.BeamBolt.EdgeDistTransvDir, momentFlangeAngle.BeamBolt.HoleLength,
				momentFlangeAngle.BeamBolt.BoltSize, momentFlangeAngle.Material.Fu, momentFlangeAngle.BeamBolt.HoleType, false);
			si = momentFlangeAngle.BeamBolt.SpacingTransvDir;
			Fbs = CommonCalculations.SpacingBearing(momentFlangeAngle.BeamBolt.SpacingTransvDir, momentFlangeAngle.BeamBolt.HoleLength,
				momentFlangeAngle.BeamBolt.BoltSize, momentFlangeAngle.BeamBolt.HoleType, momentFlangeAngle.Material.Fu, false);
			BearingCap = momentFlangeAngle.ColumnBolt.NumberOfBolts * (Fbe + Fbs * (momentFlangeAngle.BeamBolt.NumberOfRows - 1));
			tbReq = Ff / BearingCap;
		}
	}
}