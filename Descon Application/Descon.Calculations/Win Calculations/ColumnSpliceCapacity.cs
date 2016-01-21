using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public static class ColumnSpliceCapacity
	{
		public static void DesignColumnSpliceCapacity( double dmax, double Acontact, double S_Contact, double AcontactwithFiller, double S_ContactwithFiller, double Fbr_Column, double Fbr_Filler, double Fbr_C, double d, double Fbr_F, bool Type1 )
		{
			double U = 0;
			double CapacityN = 0;
			double Ae = 0;
			double An = 0;
			double CapacityBS = 0;
			double Lnv = 0;
			double Lgv = 0;
			double Lnt = 0;
			double Lgt = 0;
			double CapacityBr = 0;
			double Fbrs = 0;
			double Fbre = 0;
			double usefulweldonPlate = 0;
			double FfSeismic = 0;
			double FWeldSizeFillerToCol = 0;
			double FWeldSizePlateToFiller = 0;
			double FWeldSizePlateToCol = 0;
			double capacity = 0;
			double eh = 0;
			double dumy = 0;
			double minedge = 0;
			double minsp = 0;
			double FuT = 0;
			double ce = 0;
			double s = 0;
			double e = 0;
			double FfillerUF = 0;
			double FfillerLF = 0;
			double Fsplice = 0;
			double Ffiller = 0;
			double FspliceT = 0;
			double FfillerT = 0;
			double Fsplice0t = 0;
			double Ffiller0T = 0;
			double FspliceC = 0;
			double FfillerC = 0;
			double Fsplice0c = 0;
			double Ffiller0c = 0;

			var colSplice = CommonDataStatic.ColumnSplice;

			ColumnSpliceCapacityPart1(dmax, Acontact, S_Contact, AcontactwithFiller, S_ContactwithFiller, Fbr_Column, Fbr_Filler, Fbr_C, d, Fbr_F, Type1, ref Ffiller0c, ref Fsplice0c, ref FfillerC, ref FspliceC, ref Ffiller0T, ref Fsplice0t, ref FfillerT, ref FspliceT, ref Ffiller, ref Fsplice, ref FfillerLF, ref FfillerUF);
			if (colSplice.ConnectionOption == ESpliceConnection.FlangePlate || colSplice.ConnectionOption == ESpliceConnection.FlangeAndWebPlate)
			{
				Reporting.AddHeader("Connection to Upper Column Flanges:");
				if (colSplice.ConnectionUpper == EConnectionStyle.Bolted)
				{
					BoltsUpperColumn(ref e, ref s, ref ce, ref FuT, Fsplice, ref minsp, dumy, ref minedge, ref eh, ref capacity, FfillerUF);
					if (FspliceT > 0)
					{
						BoltBearingUpperColumnTension(ref Fbre, ref Fbrs, ref CapacityBr, FspliceT);
						BlockShearUpperColumn(ref Lgt, ref Lnt, ref Lgv, ref Lnv, ref CapacityBS, FspliceT);
						PlateTensionGross(FspliceT);
						PlateTensionBoltedNetUpperColumn(ref An, ref Ae, ref CapacityN, FspliceT);
					}
					if (FspliceC > 0)
					{
						BoltBearingUpperColumnCompression(ref Fbrs, ref CapacityBr, FspliceC);
						Reporting.AddHeader("See Conn. to Lower Col. for Buckling Check.");
					}
				}
				else
				{
					WeldsUpperColumn(ref FWeldSizePlateToCol, ref FWeldSizePlateToFiller, ref FWeldSizeFillerToCol, FspliceT, ref FfSeismic, Fsplice, ref usefulweldonPlate, ref capacity, FspliceC, FfillerC);
					if (FspliceT > 0)
					{
						PlateTensionGross(FspliceT);
						PlateTensionWeldedNetUpperColumn(ref U, ref CapacityN, FspliceT);
					}
					if (FspliceC > 0)
						Reporting.AddHeader("See Conn. to Lower Col. for Buckling Check.");
				}

				Reporting.AddHeader("Connection to Lower Column Flanges:");
				if (colSplice.ConnectionLower == EConnectionStyle.Bolted)
				{
					BoltsLowerColumn(ref e, ref s, ref ce, ref FuT, Fsplice, ref minsp, dumy, ref minedge, ref eh, ref capacity, FfillerLF, Type1);
					if (FspliceT > 0)
					{
						BoltBearingLowerColumnTension(ref Fbre, ref Fbrs, ref CapacityBr, FspliceT);
						BlockShearLowerColumn(ref Lgt, ref Lnt, ref Lgv, ref Lnv, ref CapacityBS, FspliceT);
						PlateTensionGross(FspliceT);
						PlateTensionBoltedNetLowerColumn(ref An, ref Ae, ref CapacityN, FspliceT);
					}
					if (FspliceC > 0)
					{
						BoltBearingLowerColumnCompression(ref Fbrs, ref CapacityBr, FspliceC);
						PlateBuckling(FspliceC);
					}
				}
				else
				{
					WeldsLowerColumn(ref FWeldSizePlateToCol, ref FWeldSizePlateToFiller, ref FWeldSizeFillerToCol, FspliceT, ref FfSeismic, Fsplice, ref usefulweldonPlate, ref capacity, FspliceC, FfillerC);
					if (FspliceT > 0)
					{
						PlateTensionGross(FspliceT);
						PlateTensionWeldedNetLowerColumn(ref U, ref CapacityN, FspliceT);
					}
					if (FspliceC > 0)
						PlateBuckling(FspliceC);
				}
			}

			if (colSplice.ConnectionOption == ESpliceConnection.FlangeAndWebPlate)
				ColumnSpliceWebPlate.WebPlateCapacity(EColumnSplice.Both, 0);
			if (colSplice.ConnectionOption == ESpliceConnection.ButtPlate)
				ColumnSpliceMisc.ButtPlateCapacity();
			if (colSplice.ConnectionOption == ESpliceConnection.DirectlyWelded)
				ColumnSpliceDirectWeld.DesignColumnSpliceDirectWeld(ref Acontact);
			
			ColumnSpliceMisc.ColumnStressCheck();
			ColumnSpliceMisc.SpliceMomentOfInertia();
		}

		private static void ColumnSpliceCapacityPart1( double dmax, double Acontact, double S_Contact, double AcontactwithFiller, double S_ContactwithFiller, double Fbr_Column, double Fbr_Filler, double Fbr_C, double d, double Fbr_F, bool Type1, ref double Ffiller0c, ref double Fsplice0c, ref double FfillerC, ref double FspliceC, ref double Ffiller0T, ref double Fsplice0t, ref double FfillerT, ref double FspliceT, ref double Ffiller, ref double Fsplice, ref double FfillerLF, ref double FfillerUF )
		{
			double Pexcess;
			double Fb;
			double Fa;

			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			if (colSplice.ConnectionOption != ESpliceConnection.ButtPlate)
			{
				if (colSplice.ConnectionLower == EConnectionStyle.Bolted || colSplice.ConnectionUpper == EConnectionStyle.Bolted)
				{
					Reporting.AddHeader("Bolts: " + colSplice.Bolt.BoltName);
					Reporting.AddLine("Bolt Holes (Vert X Horiz.):");
					if (colSplice.ConnectionUpper == EConnectionStyle.Bolted)
						Reporting.AddLine("Upper Column: " + colSplice.Bolt.HoleLength + " X " + colSplice.Bolt.HoleWidth);
					if (colSplice.ConnectionLower == EConnectionStyle.Bolted)
						Reporting.AddLine("Lower Column: " + colSplice.Bolt.HoleLength + " X " + colSplice.Bolt.HoleWidth);
					if (colSplice.ConnectionUpper == EConnectionStyle.Bolted)
						Reporting.AddLine("Plates on Upper Column: " + colSplice.Bolt.HoleLength + " X " + colSplice.Bolt.HoleWidth);
					if (colSplice.ConnectionLower == EConnectionStyle.Bolted)
						Reporting.AddLine("Plates on Lower Column: " + colSplice.Bolt.HoleLength + " X " + colSplice.Bolt.HoleWidth);
				}

				if (colSplice.FillerThicknessFlangeUpper + colSplice.FillerThicknessFlangeLower + colSplice.FillerThicknessWebUpper + colSplice.FillerThicknessWebLower > 0)
				{
					Reporting.AddHeader("Fillers:");
					if (colSplice.FillerThicknessFlangeUpper > 0)
						Reporting.AddLine("Upper Column Flange: " + colSplice.FillerLengthFlangeUpper + " X " + colSplice.FillerWidthFlangeUpper + " X " + colSplice.FillerThicknessFlangeUpper + ConstUnit.Length);
					if (colSplice.FillerThicknessFlangeLower > 0)
						Reporting.AddLine("Lower Column Flange: " + colSplice.FillerLengthFlangeLower + " X " + colSplice.FillerWidthFlangeLower + " X " + colSplice.FillerThicknessFlangeLower + ConstUnit.Length);
					if (colSplice.FillerThicknessWebUpper > 0)
						Reporting.AddLine("Upper Column Web: " + colSplice.FillerLengthWebUpper + " X " + colSplice.FillerWidthWebUpper + " X " + colSplice.FillerThicknessWebUpper + ConstUnit.Length);
					if (colSplice.FillerThicknessWebLower > 0)
						Reporting.AddLine("Lower Column Web: " + colSplice.FillerLengthWebLower + " X " + colSplice.FillerWidthWebLower + " X " + colSplice.FillerThicknessWebLower + ConstUnit.Length);
				}
			}
			Reporting.AddHeader("Splice Plates and Stabilizers:");
			switch (colSplice.ConnectionOption)
			{
				case ESpliceConnection.ButtPlate:
					Reporting.AddLine("Butt Plate: " + colSplice.ButtLength + " X " + colSplice.ButtWidth + " X " + colSplice.ButtThickness + ConstUnit.Length);
					Reporting.AddLine("Stabilizers per usual practice");
					break;
				case ESpliceConnection.DirectlyWelded:
					Reporting.AddLine("Web Splice/Stabilizer Channel(s): " + colSplice.NumberOfChannels + colSplice.Channel.Name + " (" + colSplice.ChannelType + ")");
					break;
				case ESpliceConnection.FlangePlate:
					Reporting.AddLine("Flange Splice: 2PL - " + colSplice.FlangePLLength + " X " + colSplice.SpliceWidthFlange + " X " + colSplice.SpliceThicknessFlange + ConstUnit.Length);
					break;
				case ESpliceConnection.FlangeAndWebPlate:
					Reporting.AddLine("Flange Splice: 2PL - " + colSplice.FlangePLLength + " X " + colSplice.SpliceWidthFlange + " X " + colSplice.SpliceThicknessFlange + ConstUnit.Length);
					Reporting.AddLine("Web Splice: 2PL - " + colSplice.WebPLLength + " X " + colSplice.SpliceWidthWeb + " X " + colSplice.SpliceThicknessWeb + ConstUnit.Length);
					break;
			}

			if (colSplice.ConnectionOption == ESpliceConnection.FlangePlate || colSplice.ConnectionOption == ESpliceConnection.FlangeAndWebPlate)
			{
				Reporting.AddHeader("Contact Surface:");

				Reporting.AddHeader("(Fillets Neglected)");
				Reporting.AddLine("Area w/o Fillers (Ac) = " + Acontact + ConstUnit.Area);
				Reporting.AddLine("Section Modulus (Sc) = " + S_Contact + ConstUnit.SecMod);
				Reporting.AddLine("Area with Fillers (Acf) = " + AcontactwithFiller + ConstUnit.Area);
				Reporting.AddLine("Section Modulus (Scf) = " + S_ContactwithFiller + ConstUnit.SecMod);

				Reporting.AddHeader("Bearing Strength at Contact Surface:");
				Reporting.AddLine("Without Fillers (Fbrc) = " + ConstString.FIOMEGA0_75 + " * 1.8 * Fy * Ac =  " + ConstString.FIOMEGA0_75 + " * 1.8 * " + Math.Min(tColumn.Material.Fy, bColumn.Material.Fy) + " * " + Acontact + " = " + Fbr_Column + ConstUnit.Force);
				Reporting.AddLine("With Fillers (Fbrf) = Fbrc +  " + ConstString.FIOMEGA0_75 + " * 1.8 * Fy' * (Acf - Ac)");
				Reporting.AddLine("=  " + Fbr_Column + " + " + ConstString.FIOMEGA0_75 + " * 1.8 * " + Math.Min(colSplice.Material.Fy, bColumn.Material.Fy) + " * (" + AcontactwithFiller + " - " + Acontact + ") =  " + Fbr_Filler + ConstUnit.Force);

				Reporting.AddHeader("Design Forces");
				if (colSplice.Moment == 0 && colSplice.Compression > 0)
				{
					Reporting.AddHeader("Concentric Compression:");
					if (colSplice.Compression <= Fbr_Column)
					{
						Reporting.AddLine("Axial force (C) = " + colSplice.Compression + " <= Fbrc = " + Fbr_Column + ConstUnit.Force);
						FfillerC = 0;
						Reporting.AddLine("FfillerC = 0");
						FspliceC = 0;
						Reporting.AddLine("FSpliceC = 0");
					}
					else if (colSplice.Compression <= Fbr_Filler)
					{
						Reporting.AddLine("Axial force (C) = " + colSplice.Compression + " >> Fbrc = " + Fbr_Column + ConstUnit.Force);
						Reporting.AddLine("C <= Fbrf = " + Fbr_Filler + ConstUnit.Force);
						FfillerC = colSplice.Compression / AcontactwithFiller * (AcontactwithFiller - Acontact) / 2;
						Reporting.AddLine("FfillerC = C / Acf * (Acf - Ac) / 2");
						Reporting.AddLine("= " + colSplice.Compression + " / " + AcontactwithFiller + " * (" + AcontactwithFiller + " - " + Acontact + ") / 2 ");
						Reporting.AddLine("= " + FfillerC + ConstUnit.Force + " (each filler)");
						FspliceC = 0;
						Reporting.AddLine("FSpliceC = 0");
					}
					else
					{
						Reporting.AddLine("Axial force (C) = " + colSplice.Compression + " >> Fbrf = " + Fbr_Filler + ConstUnit.Force);
						FfillerC = (Fbr_Filler - Fbr_Column) / 2; // each filler
						Reporting.AddLine("FfillerC = (Fbrf - Fbrc) / 2 = " + FfillerC + " = (" + Fbr_Filler + " - " + Fbr_Column + ") / 2");
						Reporting.AddLine("=" + FfillerC + ConstUnit.Force + " (each filler)");

						FspliceC = (colSplice.Compression - Fbr_Filler) / 2; // each splice
						Reporting.AddLine("FSpliceC = (C - Fbrf) / 2 = " + colSplice.Compression + " = (" + Fbr_Filler + ") / 2");
						Reporting.AddLine("=" + FspliceC + ConstUnit.Force + " (each plate)");
					}
				}
				Ffiller0c = Math.Max(Ffiller0c, FfillerC);
				Fsplice0c = Math.Max(Fsplice0c, FspliceC);

				if (colSplice.Moment == 0 && colSplice.Tension > 0)
				{
					Reporting.AddHeader("Concentric Tension:");
					FfillerT = 0;
					Reporting.AddLine("FFillerT = 0");
					FspliceT = colSplice.Tension / 2; // each splice
					Reporting.AddLine("FSpliceT = T / 2 = " + colSplice.Tension + ") / 2 = " + FspliceT + ConstUnit.Force + " (each plate)");
				}
				Ffiller0T = Math.Max(Ffiller0T, FfillerT);
				Fsplice0t = Math.Max(Fsplice0t, FspliceT);

				if (colSplice.Moment > 0 && colSplice.Tension >= 0)
				{
					Reporting.AddHeader("Tension plus Moment:");
					FfillerT = 0;
					Reporting.AddLine("FFillerT = 0");

					if (colSplice.Tension > 0)
					{
						FspliceT = colSplice.Tension / 2 + colSplice.Moment / dmax;
						Reporting.AddLine("FSpliceT = T / 2 + Ms / d = " + colSplice.Tension + " / 2 + " + colSplice.Moment + " / " + dmax + "  = " + FspliceT + " ' " + ConstUnit.Force + " (each plate)");
					}
					else
					{
						FspliceT = -colSplice.Tension / 2 + colSplice.Moment / dmax;
						Reporting.AddLine("FSpliceT = -Cmin / 2 + Ms / d = " + (-colSplice.Cmin) + " / 2 + " + colSplice.Moment + " / " + dmax + "  = " + FspliceT + " ' " + ConstUnit.Force + " (each plate)");
					}
				}
				Ffiller0T = Math.Max(Ffiller0T, FfillerT);
				Fsplice0t = Math.Max(Fsplice0t, FspliceT);

				if (colSplice.Moment > 0 && colSplice.Compression >= 0)
				{
					Reporting.AddHeader("Compression plus Moment:");
					Reporting.AddHeader("Compression Side");

					Fa = colSplice.Compression / Acontact;
					Reporting.AddLine("fa = C / Ac = " + colSplice.Compression + " / " + Acontact + " = " + Fa + ConstUnit.Stress);
					Fb = colSplice.Moment / S_Contact;
					Reporting.AddLine("fb = Ms / Scf = " + colSplice.Moment + " / " + S_Contact + " = " + Fb + ConstUnit.Stress);
					if (Fa <= Fbr_C)
					{
						Reporting.AddLine("fa <= FbrAllw = " + ConstString.FIOMEGA0_75 + " * 1.8 * Fy = " + Fbr_C + ConstUnit.Stress);
						if (Fa + Fb <= Fbr_C)
						{
							Reporting.AddLine("fa+fb <= " + ConstString.FIOMEGA0_75 + " * 1.8 * Fy = " + Fbr_C + ConstUnit.Stress);
							FfillerC = 0;
							FspliceC = 0;
							Reporting.AddLine("FfillerC = 0");
							Reporting.AddLine("FSpliceC = 0");
						}
						else
						{
							Reporting.AddLine("fa + fb >> FbrAllw = " + ConstString.FIOMEGA0_75 + " * 1.8 * Fy = " + Fbr_C + ConstUnit.Stress);
							Pexcess = (Fa + Fb - Fbr_C) * S_Contact / dmax;
							Reporting.AddLine("Excess Force (Pex):");
							Reporting.AddLine("= (fa + fb - FbrAllw) * Scf / Dmax");
							Reporting.AddLine("= (" + Fa + " + " + Fb + " - " + Fbr_C + ") * " + S_Contact + " / " + dmax);
							Reporting.AddLine("= " + Pexcess + ConstUnit.Force);
							if (AcontactwithFiller - Acontact > 0)
							{
								Fa = Pexcess / ((AcontactwithFiller - Acontact) / 2);
								Reporting.AddLine("Fa_Filler = Pex / ((Acf - Ac) / 2)  = " + Pexcess + " / ((" + AcontactwithFiller + " - " + Acontact + ") / 2) = " + Fa + ConstUnit.Stress);
								if (Fa <= Fbr_F)
								{
									Reporting.AddLine("fa <= FbrAllwf = " + Fbr_F + ConstUnit.Stress);
									FfillerC = Pexcess;
									Reporting.AddLine("FfillerC = Pex = " + Pexcess + ConstUnit.Force);
									FspliceC = 0;
									Reporting.AddLine("FSpliceC = 0");
								}
								else
								{
									Reporting.AddLine("fa >> FbrAllwf = " + Fbr_F + ConstUnit.Stress);
									FfillerC = Fbr_F * (AcontactwithFiller - Acontact) / 2;
									Reporting.AddLine("FfillerC = FbrAllwf * (Acf - Ac) / 2 = " + Fbr_F + " * (" + AcontactwithFiller + " - " + Acontact + ") / 2 = " + FfillerC + ConstUnit.Force);
									FspliceC = Pexcess - FfillerC;
									Reporting.AddLine("FSpliceC = Pex - FFillerC = " + Pexcess + " - " + FfillerC + " = " + FspliceC + ConstUnit.Force);
								}
							}
							else
							{
								FfillerC = 0;
								FspliceC = Pexcess;
								Reporting.AddLine("FSpliceC = Pex = " + Pexcess + ConstUnit.Force);
							}
						}
					}
					else
					{
						Reporting.AddLine("fa >> FbrAllw = " + ConstString.FIOMEGA0_75 + " * 1.8 * Fy = " + Fbr_C + ConstUnit.Stress);
						Reporting.AddHeader("Compute stresses using section including fillers");
						Fa = colSplice.Compression / AcontactwithFiller;
						Reporting.AddLine("fa = C/Acf = " + colSplice.Compression + " / " + AcontactwithFiller + " = " + Fa + ConstUnit.Stress);
						Fb = colSplice.Moment / S_ContactwithFiller;
						Reporting.AddLine("fb = Ms/Scf = " + colSplice.Moment + " / " + S_ContactwithFiller + " = " + Fb + ConstUnit.Stress);
						if (Fa + Fb <= Fbr_F)
						{
							Reporting.AddLine("fa + fb <=  FbrAllwf = " + ConstString.FIOMEGA0_75 + " * 1.8 * Fy = " + Fbr_F + ConstUnit.Stress);
							FfillerC = (Fa + Fb) * (AcontactwithFiller - Acontact) / 2;
							Reporting.AddLine("FfillerC = (fa + fb) * (Acf - Ac) / 2 = (" + Fa + "+" + Fb + ") * (" + AcontactwithFiller + " - " + Acontact + ") / 2 = " + FfillerC + ConstUnit.Force);
							FspliceC = 0;
							Reporting.AddLine("FSpliceC = 0");
						}
						else
						{
							Reporting.AddLine("fa + fb >> FbrAllwf = " + ConstString.FIOMEGA0_75 + " * 1.8 * Fy = " + Fbr_F + ConstUnit.Stress);
							Pexcess = (Fa + Fb - Fbr_F) * S_ContactwithFiller / dmax;
							Reporting.AddLine("Excess Force, Pex:");
							Reporting.AddLine("= (fa + fb - FbrAllwf) * Scf / Dmax");
							Reporting.AddLine("= (" + Fa + " + " + Fb + " - " + Fbr_F + ") * " + S_ContactwithFiller + " / " + dmax);
							Reporting.AddLine("= " + Pexcess + ConstUnit.Force);
							FfillerC = Fbr_F * (AcontactwithFiller - Acontact) / 2;
							Reporting.AddLine("FFillerC = FbrAllwf * (Acf - Ac) / 2 = " + Fbr_F + " * (" + AcontactwithFiller + " - " + Acontact + ") / 2 = " + FfillerC + ConstUnit.Force);
							FspliceC = Pexcess;
							Reporting.AddLine("FSpliceC = Pex = " + Pexcess + ConstUnit.Force);

						}
					}
					Ffiller0c = Math.Max(Ffiller0c, FfillerC);
					Fsplice0c = Math.Max(Fsplice0c, FspliceC);

					Reporting.AddHeader("Tension Side");
					Reporting.AddLine("Without Fillers:");
					Fa = 0.75 * colSplice.Compression / Acontact - colSplice.Moment / S_Contact;
					Reporting.AddLine("fa = 0.75 * C / Ac -  Ms / S_Contact");
					Reporting.AddLine("= 0.75 * " + colSplice.Compression + " / " + Acontact + " - " + colSplice.Moment + " / " + S_Contact);
					Reporting.AddLine("= " + Fa + ConstUnit.Stress);
					if (Fa >= 0)
					{
						FfillerT = 0;
						FspliceT = 0;
						Reporting.AddLine("FFillerT = 0; FSpliceT = 0");
					}
					else
					{
						FfillerT = 0;
						Reporting.AddLine("FFillerT = 0");
						FspliceT = (colSplice.Moment - 0.75 * colSplice.Compression / Acontact * S_Contact) / dmax;
						Reporting.AddLine("FSpliceT = (Ms - (0.75 * C / Ac) * Sc) / d");
						Reporting.AddLine("= (" + colSplice.Moment + " - (0.75 * " + colSplice.Compression + " / " + Acontact + ") * " + S_Contact + ") / " + dmax);
						Reporting.AddLine("= " + FspliceT + ConstUnit.Force);
					}
					Ffiller0T = Math.Max(Ffiller0T, FfillerT);
					Fsplice0t = Math.Max(Fsplice0t, FspliceT);

					Reporting.AddHeader("With Fillers:");
					// With Fillers
					Fa = 0.75 * colSplice.Compression / AcontactwithFiller - colSplice.Moment / S_ContactwithFiller;
					Reporting.AddLine("fa = 0.75 * C / Ac - Ms / S_Contact");
					Reporting.AddLine("= 0.75 * " + colSplice.Compression + " / " + AcontactwithFiller + " - " + colSplice.Moment + " / " + S_ContactwithFiller);
					Reporting.AddLine("= " + Fa + ConstUnit.Stress);
					if (Fa >= 0)
					{
						FfillerT = 0;
						FspliceT = 0;
						Reporting.AddLine("FFillerT = 0; FSpliceT = 0");
					}
					else
					{
						FfillerT = 0;
						Reporting.AddLine("FFillerT = 0");
						FspliceT = (colSplice.Moment - 0.75 * colSplice.Compression / AcontactwithFiller * S_ContactwithFiller) / dmax;
						Reporting.AddLine("FSpliceT = (Ms - (0.75 * C / Ac) * Sc) / d");
						Reporting.AddLine("= (" + colSplice.Moment + " - (0.75 * " + colSplice.Compression + " / " + AcontactwithFiller + ") * " + S_ContactwithFiller + ") / " + dmax);
						Reporting.AddLine("= " + FspliceT + ConstUnit.Force);
					}
					Ffiller0T = Math.Max(Ffiller0T, FfillerT);
					Fsplice0t = Math.Max(Fsplice0t, FspliceT);
				}
				Ffiller0c = Math.Max(Ffiller0c, FfillerC);
				Fsplice0c = Math.Max(Fsplice0c, FspliceC);
				FfillerC = Ffiller0c;
				FspliceC = Fsplice0c;

				Ffiller0T = Math.Max(Ffiller0T, FfillerT);
				Fsplice0t = Math.Max(Fsplice0t, FspliceT);
				FfillerT = Ffiller0T;
				FspliceT = Fsplice0t;
				if (colSplice.UseSeismic && colSplice.SMF)
				{
					FspliceT = Math.Max(FspliceT, ColumnSplice.FfToDevMoment);
				}

				Ffiller = Math.Max(FfillerC, FfillerT);
				Fsplice = Math.Max(FspliceC, FspliceT);

				Reporting.AddHeader("Maximum Tensile forces: ");
				Reporting.AddLine("Filler: " + FfillerT + ConstUnit.Force);
				Reporting.AddLine("Splice Plate: " + FspliceT + ConstUnit.Force);
				Reporting.AddLine("Maximum Compressive forces: ");
				Reporting.AddLine("Filler: " + FfillerC + ConstUnit.Force);
				Reporting.AddLine("Splice Plate: " + FspliceC + ConstUnit.Force);
				if (colSplice.FillerThicknessFlangeLower == 0)
					FfillerLF = 0;
				else
					FfillerLF = Ffiller;

				if (colSplice.FillerThicknessFlangeUpper == 0)
					FfillerUF = 0;
				else
					FfillerUF = Ffiller;
			}
		}

		private static void BoltsUpperColumn( ref double e, ref double s, ref double ce, ref double FuT, double Fsplice, ref double minsp, double dumy, ref double minedge, ref double eh, ref double capacity, double FfillerUF )
		{
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			e = colSplice.BoltVertEdgeDistancePlate;
			s = colSplice.BoltVertSpacing;
			ce = colSplice.BoltVertEdgeDistanceColumn;
			if (e == 0)
				e = Math.Max(colSplice.Bolt.MinEdgeSheared, ConstNum.ONEANDHALF_INCHES);
			if (ce == 0)
				ce = Math.Max(colSplice.Bolt.MinEdgeSheared, ConstNum.ONEANDHALF_INCHES);
			if (s == 0)
				s = Math.Max(2.66F * colSplice.Bolt.BoltSize, ConstNum.THREE_INCHES);
			
			colSplice.BoltVertEdgeDistancePlate = e;
			colSplice.BoltVertSpacing = s;
			colSplice.BoltVertEdgeDistanceColumn = ce;
			Reporting.AddHeader("Bolt Spacing:");
			FuT = Math.Min(colSplice.SpliceThicknessFlange * colSplice.Material.Fu, tColumn.Shape.tf * tColumn.Material.Fu);
			minsp = MiscCalculationsWithReporting.MinimumSpacing((colSplice.Bolt.BoltSize), (Fsplice / (2 * colSplice.BoltRowsFlangeUpper)), FuT, colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleType);
			if (double.IsNaN(minsp))
				minsp = 0;

			if (s >= minsp)
				Reporting.AddLine("s = " + s + " >= " + minsp + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("s = " + s + " << " + minsp + ConstUnit.Length + " (NG)");

			Reporting.AddLine("Bolt Vertical Edge Dist. on Plate:");
			FuT = colSplice.SpliceThicknessFlange * colSplice.Material.Fu;
			minedge = MiscCalculationsWithReporting.MinimumEdgeDist(colSplice.Bolt.BoltSize, Fsplice / (2 * colSplice.BoltRowsFlangeUpper), FuT, colSplice.Bolt.MinEdgeSheared, (int)colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleType);
			if (e >= minedge)
				Reporting.AddLine("E = " + e + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("E = " + e + " << " + minedge + ConstUnit.Length + " (NG)");

			Reporting.AddLine("Bolt Vertical Edge Dist. on Column:");
			FuT = tColumn.Shape.tf * tColumn.Material.Fu;
			minedge = MiscCalculationsWithReporting.MinimumEdgeDist(colSplice.Bolt.BoltSize, Fsplice / (2 * colSplice.BoltRowsFlangeUpper), FuT, colSplice.Bolt.MinEdgeSheared, (int)colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleType);
			if (ce >= minedge)
				Reporting.AddLine("ce = " + ce + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("ce = " + ce + " << " + minedge + ConstUnit.Length + " (NG)");

			Reporting.AddLine("Bolt Horiz. Edge Dist. on Plate:");
			minedge = colSplice.Bolt.MinEdgeSheared + colSplice.Bolt.Eincr;
			eh = (colSplice.SpliceWidthFlange - colSplice.BoltGageFlangeUpper) / 2;
			if (eh >= minedge)
				Reporting.AddLine("Eh = " + eh + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Eh = " + eh + " << " + minedge + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Bolt Horiz. Edge Dist. on Column:");
			minedge = colSplice.Bolt.MinEdgeRolled + colSplice.Bolt.Eincr;
			eh = (tColumn.Shape.bf - colSplice.BoltGageFlangeUpper) / 2;
			if (eh >= minedge)
				Reporting.AddLine("Eh = " + eh + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Eh = " + eh + " << " + minedge + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Bolt Shear:");
			if (colSplice.Bolt.BoltType == EBoltType.SC || colSplice.FillerThicknessFlangeUpper <= ConstNum.QUARTER_INCH)
			{
				capacity = 2 * (colSplice.BoltRowsFlangeUpper + colSplice.FillerNumBoltRowsUF) * colSplice.Bolt.BoltStrength;
				Reporting.AddLine(ConstString.PHI + " Rn = 2 * N * Fv = 2 * " + (colSplice.BoltRowsFlangeUpper + colSplice.FillerNumBoltRowsUF) + " * " + colSplice.Bolt.BoltStrength);
				if (capacity >= FfillerUF + Fsplice)
					Reporting.AddLine("= " + capacity + " >= " + (FfillerUF + Fsplice) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + (FfillerUF + Fsplice) + ConstUnit.Force + " (NG)");
			}
			else if (colSplice.FillerThicknessFlangeUpper <= 0.75 * ConstNum.ONE_INCH)
			{
				capacity = 2 * (colSplice.BoltRowsFlangeUpper + colSplice.FillerNumBoltRowsUF) * colSplice.Bolt.BoltStrength * (1 - 0.4 * (colSplice.FillerThicknessFlangeUpper - ConstNum.QUARTER_INCH));
				Reporting.AddLine(ConstString.PHI + " Rn = 2 * N * Fv * (1 - 0.4 * (t - " + ConstNum.QUARTER_INCH + "))");
				Reporting.AddLine("= 2 * " + (colSplice.BoltRowsFlangeUpper + colSplice.FillerNumBoltRowsUF) + " * " + colSplice.Bolt.BoltStrength + "*(1 - 0.4*(" + colSplice.FillerThicknessFlangeUpper + " - " + ConstNum.QUARTER_INCH + "))");
				if (capacity >= FfillerUF + Fsplice)
					Reporting.AddLine("= " + capacity + " >= " + (FfillerUF + Fsplice) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + (FfillerUF + Fsplice) + ConstUnit.Force + " (NG)");
			}
			else
			{
				capacity = 2 * (colSplice.BoltRowsFlangeUpper + colSplice.FillerNumBoltRowsUF) * colSplice.Bolt.BoltStrength;
				Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ns + Nf)*Fv = 2 * (" + colSplice.BoltRowsFlangeUpper + " + " + colSplice.FillerNumBoltRowsUF + ") * " + colSplice.Bolt.BoltStrength);
				if (capacity >= FfillerUF + Fsplice)
					Reporting.AddLine("= " + capacity + " >= " + (FfillerUF + Fsplice) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + (FfillerUF + Fsplice) + ConstUnit.Force + " (NG)");
			}
		}

		private static void BoltBearingUpperColumnTension( ref double Fbre, ref double Fbrs, ref double CapacityBr, double FspliceT )
		{
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Bolt Bearing Under Tensile Force:");
			Reporting.AddHeader("Column Flange:");
			Fbre = CommonCalculations.EdgeBearing(colSplice.BoltVertEdgeDistanceColumn, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, tColumn.Material.Fu, colSplice.Bolt.HoleType, true);
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, tColumn.Material.Fu, true);
			CapacityBr = 2 * (Fbre + (colSplice.BoltRowsFlangeUpper - 1) * Fbrs) * tColumn.Shape.tf;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Fbre + (N - 1) * Fbrs) * tf");
			Reporting.AddLine("= 2 * (" + Fbre + " + (" + colSplice.BoltRowsFlangeUpper + " - 1) * " + Fbrs + ") * " + tColumn.Shape.tf);
			if (CapacityBr >= FspliceT)
				Reporting.AddLine("= " + CapacityBr + " >= " + FspliceT + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + FspliceT + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Splice Plate:");
			Fbre = CommonCalculations.EdgeBearing(colSplice.BoltVertEdgeDistancePlate, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Material.Fu, colSplice.Bolt.HoleType, true);
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, true);
			CapacityBr = 2 * (Fbre + (colSplice.BoltRowsFlangeUpper - 1) * Fbrs) * colSplice.SpliceThicknessFlange;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Fbre + (N - 1) * Fbrs) * t");
			Reporting.AddLine("= 2 * (" + Fbre + " + (" + colSplice.BoltRowsFlangeUpper + " - 1) * " + Fbrs + ") * " + colSplice.SpliceThicknessFlange);
			if (CapacityBr >= FspliceT)
				Reporting.AddLine("= " + CapacityBr + " >= " + FspliceT + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + FspliceT + ConstUnit.Force + " (NG)");
		}

		private static void BlockShearUpperColumn( ref double Lgt, ref double Lnt, ref double Lgv, ref double Lnv, ref double CapacityBS, double FspliceT )
		{
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Block Shear:");
			Reporting.AddHeader("Column Flange:");
			Lgt = tColumn.Shape.bf - colSplice.BoltGageFlangeUpper;
			Lnt = Lgt - (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
			Lgv = 2 * (colSplice.BoltVertEdgeDistanceColumn + (colSplice.BoltRowsFlangeUpper - 1) * colSplice.BoltVertSpacing);
			Lnv = Lgv - 2 * (colSplice.BoltRowsFlangeUpper - 0.5) * (colSplice.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
			Reporting.AddLine("Lgt = bf - g = " + tColumn.Shape.bf + " - " + colSplice.BoltGageFlangeUpper + " = " + Lgt + ConstUnit.Length);
			Reporting.AddLine("Lnt = Lgt - (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgt + " - (" + colSplice.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnt + ConstUnit.Length);
			Reporting.AddLine("Lgv = 2 * (e + (N - 1) * s) = 2 * (" + colSplice.BoltVertEdgeDistanceColumn + " + (" + colSplice.BoltRowsFlangeUpper + " - 1) * " + colSplice.BoltVertSpacing + ") = " + Lgv + ConstUnit.Length);
			Reporting.AddLine("Lnv = Lgv - 2 * (N - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgv + " - 2 * (" + colSplice.BoltRowsFlangeUpper + " - 0.5) * (" + colSplice.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnv + ConstUnit.Length);
			CapacityBS = MiscCalculationsWithReporting.BlockShearNew(tColumn.Material.Fu, Lnv, 1, Lnt, Lgv, tColumn.Material.Fy, tColumn.Shape.tf, FspliceT, true);

			Reporting.AddHeader("Splice Plate:");
			Lgt = Math.Min(colSplice.BoltGageFlangeUpper, colSplice.SpliceWidthFlange - colSplice.BoltGageFlangeUpper);
			Lnt = Lgt - (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
			Lgv = 2 * (colSplice.BoltVertEdgeDistancePlate + (colSplice.BoltRowsFlangeUpper - 1) * colSplice.BoltVertSpacing);
			Lnv = Lgv - 2 * (colSplice.BoltRowsFlangeUpper - 0.5) * (colSplice.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
			Reporting.AddLine("Lgt = Min(g ; W - g) = Min(" + colSplice.BoltGageFlangeUpper + ";" + colSplice.SpliceWidthFlange + " - " + colSplice.BoltGageFlangeUpper + ") = " + Lgt + ConstUnit.Length);
			Reporting.AddLine("Lnt = Lgt - (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgt + " - (" + colSplice.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnt + ConstUnit.Length);
			Reporting.AddLine("Lgv = 2 * (e + (N - 1) * s) = 2 * (" + colSplice.BoltVertEdgeDistancePlate + " + (" + colSplice.BoltRowsFlangeUpper + " - 1) * " + colSplice.BoltVertSpacing + ") = " + Lgv + ConstUnit.Length);
			Reporting.AddLine("Lnv = Lgv - 2 * (N-0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgv + " - 2 * (" + colSplice.BoltRowsFlangeUpper + " - 0.5) * (" + colSplice.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnv + ConstUnit.Length);

			CapacityBS = MiscCalculationsWithReporting.BlockShearNew(colSplice.Material.Fu, Lnv, 1, Lnt, Lgv, colSplice.Material.Fy, colSplice.SpliceThicknessFlange, FspliceT, true);
		}

		private static void PlateTensionGross( double FspliceT )
		{
			double CapacityG;

			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Splice Plate Tension:");
			Reporting.AddHeader("On Gross Area:");
			CapacityG = ConstNum.FIOMEGA0_9N * colSplice.Material.Fy * colSplice.SpliceWidthFlange * colSplice.SpliceThicknessFlange;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + "  * Fy * Ag = " + ConstString.FIOMEGA0_9 + "  * " + colSplice.Material.Fy + " * " + (colSplice.SpliceWidthFlange * colSplice.SpliceThicknessFlange));
			if (CapacityG >= FspliceT)
				Reporting.AddLine("= " + CapacityG + " >= " + FspliceT + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityG + " << " + FspliceT + ConstUnit.Force + " (NG)");
		}

		private static void PlateTensionBoltedNetUpperColumn( ref double An, ref double Ae, ref double CapacityN, double FspliceT )
		{
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("On Net Area of Splice Plate:");
			An = (colSplice.SpliceWidthFlange - 2 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * colSplice.SpliceThicknessFlange;
			Ae = Math.Min(An, 0.85F * colSplice.SpliceWidthFlange * colSplice.SpliceThicknessFlange);
			CapacityN = ConstNum.FIOMEGA0_75N * colSplice.Material.Fu * Ae;
			Reporting.AddHeader("An = (W - 2 * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + colSplice.SpliceWidthFlange + " - 2 * (" + colSplice.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + colSplice.SpliceThicknessFlange);
			Reporting.AddLine("= " + An + ConstUnit.Area);

			Reporting.AddHeader("Ae = Min(An; 0.85*Ag) = " + Ae + ConstUnit.Area);
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + "* Fu * Ae = " + ConstString.FIOMEGA0_75 + " * " + colSplice.Material.Fu + " * " + Ae);
			if (CapacityN >= FspliceT)
				Reporting.AddLine("= " + CapacityN + " >= " + FspliceT + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityN + " << " + FspliceT + ConstUnit.Force + " (NG)");
		}

		private static void BoltBearingUpperColumnCompression( ref double Fbrs, ref double CapacityBr, double FspliceC )
		{
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Bolt Bearing Under Compressive Force:");
			Reporting.AddHeader("Column Flange:");
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, tColumn.Material.Fu, true);
			CapacityBr = 2 * colSplice.BoltRowsFlangeUpper * Fbrs * tColumn.Shape.tf;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * N * Fbrs * tf");
			Reporting.AddLine("= 2 * " + colSplice.BoltRowsFlangeUpper + " * " + Fbrs + " * " + tColumn.Shape.tf);
			if (CapacityBr >= FspliceC)
				Reporting.AddLine("= " + CapacityBr + " >= " + FspliceC + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + FspliceC + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Splice Plate:");
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, true);
			CapacityBr = 2 * colSplice.BoltRowsFlangeUpper * Fbrs * colSplice.SpliceThicknessFlange;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * N * Fbrs * t");
			Reporting.AddLine("= 2 * " + colSplice.BoltRowsFlangeUpper + " * " + Fbrs + " * " + colSplice.SpliceThicknessFlange);
			if (CapacityBr >= FspliceC)
				Reporting.AddLine("= " + CapacityBr + " >= " + FspliceC + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + FspliceC + ConstUnit.Force + " (NG)");
		}

		private static void WeldsUpperColumn( ref double FWeldSizePlateToCol, ref double FWeldSizePlateToFiller, ref double FWeldSizeFillerToCol, double FspliceT, ref double FfSeismic, double Fsplice, ref double usefulweldonPlate, ref double capacity, double FspliceC, double FfillerC )
		{
			var tColumn = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			FWeldSizePlateToCol = CommonCalculations.MinimumWeld(tColumn.Shape.tf, colSplice.SpliceThicknessFlange);
			FWeldSizePlateToFiller = CommonCalculations.MinimumWeld(colSplice.FillerThicknessFlangeUpper, colSplice.SpliceThicknessFlange);
			FWeldSizeFillerToCol = CommonCalculations.MinimumWeld(tColumn.Shape.tf, colSplice.FillerThicknessFlangeUpper);

			Reporting.AddHeader("Welds:");
			if (colSplice.UseSeismic && FspliceT > 0)
				FfSeismic = Math.Max(FspliceT, ColumnSplice.FlangeTensionforSeismic);
			else
				FfSeismic = Fsplice;
			if (colSplice.FillerThicknessFlangeUpper < ConstNum.QUARTER_INCH)
			{
				if (FWeldSizePlateToCol <= colSplice.FilletWeldSizeFlangeUpper)
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(FWeldSizePlateToCol) + " <= " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeFlangeUpper) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(FWeldSizePlateToCol) + " >> " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeFlangeUpper) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Splice Plate:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * colSplice.SpliceThicknessFlange, tColumn.Material.Fu * tColumn.Shape.tf) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t, Fuc * tf) / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + colSplice.SpliceThicknessFlange + ", " + tColumn.Material.Fu + " * " + tColumn.Shape.tf + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeFlangeUpper - colSplice.FillerThicknessFlangeUpper)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w - t_fill = " + (colSplice.FilletWeldSizeFlangeUpper - colSplice.FillerThicknessFlangeUpper) + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYUF + colSplice.WeldLengthXUF) * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * wu * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYUF + " + " + colSplice.WeldLengthXUF + ") * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w - t_fill = " + (colSplice.FilletWeldSizeFlangeUpper - colSplice.FillerThicknessFlangeUpper) + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYUF + colSplice.WeldLengthXUF) * (colSplice.FilletWeldSizeFlangeUpper - colSplice.FillerThicknessFlangeUpper) * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * (w - t_fill) * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYUF + " + " + colSplice.WeldLengthXUF + ") * (" + colSplice.FilletWeldSizeFlangeUpper + " - " + colSplice.FillerThicknessFlangeUpper + ") * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= FfSeismic)
					Reporting.AddLine("= " + capacity + " >= Fsplice = " + FfSeismic + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << Fsplice = " + FfSeismic + ConstUnit.Force + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Splice Plate & Filler:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * (colSplice.SpliceThicknessFlange + colSplice.FillerThicknessFlangeUpper), tColumn.Material.Fu * tColumn.Shape.tf) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * (t+t_fill); Fuc * tf) / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + (colSplice.SpliceThicknessFlange + colSplice.FillerThicknessFlangeUpper) + "; " + tColumn.Material.Fu + " * " + tColumn.Shape.tf + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeFlangeUpper)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FilletWeldSizeFlangeUpper + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYUF + colSplice.WeldLengthXUF) * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * wu * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYUF + " + " + colSplice.WeldLengthXUF + ") * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FilletWeldSizeFlangeUpper + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYUF + colSplice.WeldLengthXUF) * colSplice.FilletWeldSizeFlangeUpper * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * w) * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYUF + " + " + colSplice.WeldLengthXUF + ") * " + colSplice.FilletWeldSizeFlangeUpper + ") * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= Math.Max(FspliceC + FfillerC, FfSeismic))
					Reporting.AddLine("= " + capacity + " >= Fsplice + Ffiller = " + Math.Max(FspliceC + FfillerC, FfSeismic) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << Fsplice + Ffiller = " + Math.Max(FspliceC + FfillerC, FfSeismic) + ConstUnit.Force + " (NG)");
			}
			else
			{
				if (FWeldSizePlateToFiller <= colSplice.FilletWeldSizeFlangeUpper)
					Reporting.AddLine("Min. Weld Size (Plate/Filler) = " + CommonCalculations.WeldSize(FWeldSizePlateToCol) + " <= " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeFlangeUpper) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size(Plate/Filler) = " + CommonCalculations.WeldSize(FWeldSizePlateToCol) + " >> " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeFlangeUpper) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Splice Plate/Filler:");
				usefulweldonPlate = colSplice.Material.Fu * Math.Min(colSplice.SpliceThicknessFlange, colSplice.FillerThicknessFlangeUpper) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Fu * Min(t; t_fill) / (0.707 * Fexx)");
				Reporting.AddLine("= " + colSplice.Material.Fu + " * Min(" + colSplice.SpliceThicknessFlange + "; " + colSplice.FillerThicknessFlangeUpper + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeFlangeUpper)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FilletWeldSizeFlangeUpper + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYUF + colSplice.WeldLengthXUF) * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * wu * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYUF + " + " + colSplice.WeldLengthXUF + ") * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FilletWeldSizeFlangeUpper + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYUF + colSplice.WeldLengthXUF) * colSplice.FilletWeldSizeFlangeUpper * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * w * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYUF + " + " + colSplice.WeldLengthXUF + ") * " + colSplice.FilletWeldSizeFlangeUpper + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= FfSeismic)
					Reporting.AddLine("= " + capacity + " >= " + FfSeismic + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + FfSeismic + ConstUnit.Force + " (NG)");

				if (FWeldSizeFillerToCol <= colSplice.FillerWeldSizeUF)
					Reporting.AddLine("Min. Weld Size (Filler/Flange) = " + CommonCalculations.WeldSize(FWeldSizeFillerToCol) + " <= " + CommonCalculations.WeldSize(colSplice.FillerWeldSizeUF) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size (Filler/Flange) = " + CommonCalculations.WeldSize(FWeldSizeFillerToCol) + " >> " + CommonCalculations.WeldSize(colSplice.FillerWeldSizeUF) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Filler/Flange:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * colSplice.FillerThicknessFlangeUpper, tColumn.Material.Fu * tColumn.Shape.tf) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t_fill, Fuc * tf) / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + colSplice.FillerThicknessFlangeUpper + ", " + tColumn.Material.Fu + " * " + tColumn.Shape.tf + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FillerWeldSizeUF)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FillerWeldSizeUF + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYUF + colSplice.WeldLengthXUF) * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * wu * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYUF + " + " + colSplice.WeldLengthXUF + ") * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FillerWeldSizeUF + ConstUnit.Length);
					capacity = 2 * colSplice.FillerWeldLengthUF * colSplice.FillerWeldSizeUF * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine("");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * Lb * w * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * " + colSplice.FillerWeldLengthUF + " * " + colSplice.FillerWeldSizeUF + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= Math.Max(FspliceC + FfillerC, FfSeismic))
					Reporting.AddLine("= " + capacity + " >= Fsplice + Ffiller = " + Math.Max(FspliceC + FfillerC, FfSeismic) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << Fsplice + Ffiller = " + Math.Max(FspliceC + FfillerC, FfSeismic) + ConstUnit.Force + " (NG)");
			}
		}

		private static void PlateTensionWeldedNetUpperColumn( ref double U, ref double CapacityN, double FspliceT )
		{
			var colSplice = CommonDataStatic.ColumnSplice;

			if (colSplice.WeldLengthYUF >= 2 * colSplice.SpliceWidthFlange)
				U = 1;
			else if (colSplice.WeldLengthYUF >= 1.5 * colSplice.SpliceWidthFlange)
				U = 0.87;
			else if (colSplice.WeldLengthYUF >= colSplice.SpliceWidthFlange)
				U = 0.75;
			else
				U = 0.75;

			Reporting.AddHeader("On Effective Net Area:");
			CapacityN = ConstNum.FIOMEGA0_75N * colSplice.Material.Fu * U * colSplice.SpliceWidthFlange * colSplice.SpliceThicknessFlange;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + "* Fu * u * W * t");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + "* " + colSplice.Material.Fu + " * " + U + " * " + colSplice.SpliceWidthFlange + " * " + colSplice.SpliceThicknessFlange);
			if (CapacityN >= FspliceT)
				Reporting.AddLine("= " + CapacityN + " >= " + FspliceT + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityN + " << " + FspliceT + ConstUnit.Force + " (NG)");
		}

		private static void BoltsLowerColumn( ref double e, ref double s, ref double ce, ref double FuT, double Fsplice, ref double minsp, double dumy, ref double minedge, ref double eh, ref double capacity, double FfillerLF, bool Type1 )
		{
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			e = colSplice.BoltVertEdgeDistancePlate;
			s = colSplice.BoltVertSpacing;
			ce = colSplice.BoltVertEdgeDistanceColumn;
			if (e == 0)
				e = Math.Max(colSplice.Bolt.MinEdgeSheared, ConstNum.ONEANDHALF_INCHES);
			if (ce == 0)
				ce = Math.Max(colSplice.Bolt.MinEdgeSheared, ConstNum.ONEANDHALF_INCHES);
			if (s == 0)
				s = Math.Max(2.66F * colSplice.Bolt.BoltSize, ConstNum.THREE_INCHES);
			
			colSplice.BoltVertEdgeDistancePlate = e;
			colSplice.BoltVertSpacing = s;
			colSplice.BoltVertEdgeDistanceColumn = ce;
			
			Reporting.AddHeader("Bolt Spacing:");
			FuT = Math.Min(colSplice.SpliceThicknessFlange * colSplice.Material.Fu, bColumn.Shape.tf * bColumn.Material.Fu);
			minsp = MiscCalculationsWithReporting.MinimumSpacing((colSplice.Bolt.BoltSize), (Fsplice / (2 * colSplice.BoltRowsFlangeLower)), FuT, colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleType);
			if (double.IsNaN(minsp))
				minsp = 0;

			if (s >= minsp)
				Reporting.AddLine("s = " + s + " >= " + minsp + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("s = " + s + " << " + minsp + ConstUnit.Length + " (NG)");

			Reporting.AddLine("Bolt Vertical Edge Dist. on Plate:");
			FuT = colSplice.SpliceThicknessFlange * colSplice.Material.Fu;
			minedge = MiscCalculationsWithReporting.MinimumEdgeDist(colSplice.Bolt.BoltSize, Fsplice / (2 * colSplice.BoltRowsFlangeLower), FuT, colSplice.Bolt.MinEdgeSheared, (int)colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleType);
			if (e >= minedge)
				Reporting.AddLine("E = " + e + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("E = " + e + " << " + minedge + ConstUnit.Length + " (NG)");

			Reporting.AddLine("Bolt Vertical Edge Dist. on Column:");
			FuT = bColumn.Shape.tf * bColumn.Material.Fu;
			minedge = MiscCalculationsWithReporting.MinimumEdgeDist(colSplice.Bolt.BoltSize, Fsplice / (2 * colSplice.BoltRowsFlangeLower), FuT, colSplice.Bolt.MinEdgeSheared, (int)colSplice.Bolt.HoleWidth, colSplice.Bolt.HoleType);
			if (ce >= minedge)
				Reporting.AddLine("ce = " + ce + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("ce = " + ce + " << " + minedge + ConstUnit.Length + " (NG)");

			Reporting.AddLine("Bolt Horiz. Edge Dist. on Plate:");
			minedge = colSplice.Bolt.MinEdgeSheared + colSplice.Bolt.Eincr;
			eh = (colSplice.SpliceWidthFlange - colSplice.BoltGageFlangeLower) / 2;
			if (eh >= minedge)
				Reporting.AddLine("Eh = " + eh + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Eh = " + eh + " << " + minedge + ConstUnit.Length + " (NG)");

			Reporting.AddLine("Bolt Horiz. Edge Dist. on Column:");
			minedge = colSplice.Bolt.MinEdgeRolled + colSplice.Bolt.Eincr;
			eh = (bColumn.Shape.bf - colSplice.BoltGageFlangeLower) / 2;
			if (eh >= minedge)
				Reporting.AddLine("Eh = " + eh + " >= " + minedge + ConstUnit.Length + " (OK)");
			else
				Reporting.AddLine("Eh = " + eh + " << " + minedge + ConstUnit.Length + " (NG)");

			Reporting.AddHeader("Bolt Shear:");
			if (colSplice.Bolt.BoltType == EBoltType.SC || colSplice.FillerThicknessFlangeLower <= ConstNum.QUARTER_INCH)
			{
				capacity = 2 * (colSplice.BoltRowsFlangeLower + colSplice.FillerNumBoltRowsLF) * colSplice.Bolt.BoltStrength;
				Reporting.AddLine(ConstString.PHI + " Rn = 2 * N * Fv = 2 * " + (colSplice.BoltRowsFlangeLower + colSplice.FillerNumBoltRowsLF) + " * " + colSplice.Bolt.BoltStrength);
				if (capacity >= FfillerLF + Fsplice)
					Reporting.AddLine("= " + capacity + " >= " + (FfillerLF + Fsplice) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + (FfillerLF + Fsplice) + ConstUnit.Force + " (NG)");
			}
			else if (colSplice.FillerThicknessFlangeLower <= 0.75 * ConstNum.ONE_INCH)
			{
				capacity = 2 * (colSplice.BoltRowsFlangeLower + colSplice.FillerNumBoltRowsLF) * colSplice.Bolt.BoltStrength * (1 - 0.4 * (colSplice.FillerThicknessFlangeLower - ConstNum.QUARTER_INCH));
				Reporting.AddLine(ConstString.PHI + " Rn = 2 * N*Fv*(1 - 0.4*(t - ceyrekinchn))");
				Reporting.AddLine("= 2 * " + (colSplice.BoltRowsFlangeLower + colSplice.FillerNumBoltRowsLF) + " * " + colSplice.Bolt.BoltStrength + "*(1 - 0.4*(" + colSplice.FillerThicknessFlangeLower + " - " + ConstNum.QUARTER_INCH + "))");
				if (capacity >= FfillerLF + Fsplice)
					Reporting.AddLine("= " + capacity + " >= " + (FfillerLF + Fsplice) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + (FfillerLF + Fsplice) + ConstUnit.Force + " (NG)");
			}
			else
			{
				capacity = 2 * (colSplice.BoltRowsFlangeLower + colSplice.FillerNumBoltRowsLF) * colSplice.Bolt.BoltStrength;
				Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ns + Nf)*Fv = 2 * (" + colSplice.BoltRowsFlangeLower + " + " + colSplice.FillerNumBoltRowsLF + ") * " + colSplice.Bolt.BoltStrength);
				if (capacity >= FfillerLF + Fsplice)
					Reporting.AddLine("= " + capacity + " >= " + (FfillerLF + Fsplice) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + (FfillerLF + Fsplice) + ConstUnit.Force + " (NG)");
			}

			if (Type1)
			{
				if (colSplice.BoltRowsFlangeLower < 3)
					Reporting.AddLine("Number of Bolt Rows << 3 (NG)");
			}
			else
			{
				if (colSplice.BoltRowsFlangeLower < 2)
					Reporting.AddLine("Number of Bolt Rows << 2 (NG)");
			}
		}

		private static void BoltBearingLowerColumnTension( ref double Fbre, ref double Fbrs, ref double CapacityBr, double FspliceT )
		{
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Bolt Bearing Under Tensile Force:");
			Reporting.AddHeader("Column Flange:");
			Fbre = CommonCalculations.EdgeBearing(colSplice.BoltVertEdgeDistanceColumn, (colSplice.Bolt.HoleWidth), colSplice.Bolt.BoltSize, bColumn.Material.Fu, colSplice.Bolt.HoleType, true);
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, bColumn.Material.Fu, true);
			CapacityBr = 2 * (Fbre + (colSplice.BoltRowsFlangeLower - 1) * Fbrs) * bColumn.Shape.tf;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Fbre + (N - 1) * Fbrs) * tf");
			Reporting.AddLine("= 2 * (" + Fbre + " + (" + colSplice.BoltRowsFlangeLower + " - 1) * " + Fbrs + ") * " + bColumn.Shape.tf);
			if (CapacityBr >= FspliceT)
				Reporting.AddLine("= " + CapacityBr + " >= " + FspliceT + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + FspliceT + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Splice Plate:");
			Fbre = CommonCalculations.EdgeBearing(colSplice.BoltVertEdgeDistancePlate, (colSplice.Bolt.HoleWidth), colSplice.Bolt.BoltSize, colSplice.Material.Fu, colSplice.Bolt.HoleType, true);
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, true);
			CapacityBr = 2 * (Fbre + (colSplice.BoltRowsFlangeLower - 1) * Fbrs) * colSplice.SpliceThicknessFlange;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Fbre + (N - 1) * Fbrs) * t");
			Reporting.AddLine("= 2 * (" + Fbre + " + (" + colSplice.BoltRowsFlangeLower + " - 1) * " + Fbrs + ") * " + colSplice.SpliceThicknessFlange);
			if (CapacityBr >= FspliceT)
				Reporting.AddLine("= " + CapacityBr + " >= " + FspliceT + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + FspliceT + ConstUnit.Force + " (NG)");
		}

		private static void BlockShearLowerColumn( ref double Lgt, ref double Lnt, ref double Lgv, ref double Lnv, ref double CapacityBS, double FspliceT )
		{
			double dh = 0;

			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Block Shear:");
			Reporting.AddHeader("Column Flange:");

			Lgt = bColumn.Shape.bf - colSplice.BoltGageFlangeLower;
			Lnt = Lgt - (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
			Lgv = 2 * (colSplice.BoltVertEdgeDistanceColumn + (colSplice.BoltRowsFlangeLower - 1) * colSplice.BoltVertSpacing);
			Lnv = Lgv - 2 * (colSplice.BoltRowsFlangeLower - 0.5) * (colSplice.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
			Reporting.AddLine("Lgt = bf - g = " + bColumn.Shape.bf + " - " + colSplice.BoltGageFlangeLower + " = " + Lgt + ConstUnit.Length);
			Reporting.AddLine("Lnt = Lgt - (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgt + " - (" + colSplice.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnt + ConstUnit.Length);
			Reporting.AddLine("Lgv = 2 * (e + (N - 1) * s) = 2 * (" + colSplice.BoltVertEdgeDistanceColumn + " + (" + colSplice.BoltRowsFlangeLower + " - 1) * " + colSplice.BoltVertSpacing + ") = " + Lgv + ConstUnit.Length);
			Reporting.AddLine("Lnv = Lgv - 2 * (N-0.5)* (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgv + " - 2 * (" + colSplice.BoltRowsFlangeLower + " - 0.5) * (" + colSplice.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnv + ConstUnit.Length);
			CapacityBS = MiscCalculationsWithReporting.BlockShearNew(bColumn.Material.Fu, Lnv, 1, Lnt, Lgv, bColumn.Material.Fy, bColumn.Shape.tf, FspliceT, true);

			Reporting.AddHeader("Splice Plate:");
			Lgt = Math.Min(colSplice.BoltGageFlangeLower, colSplice.SpliceWidthFlange - colSplice.BoltGageFlangeLower);
			Lnt = Lgt - (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH);
			Lgv = 2 * (colSplice.BoltVertEdgeDistancePlate + (colSplice.BoltRowsFlangeLower - 1) * colSplice.BoltVertSpacing);
			Lnv = Lgv - 2 * (colSplice.BoltRowsFlangeLower - 0.5) * (colSplice.Bolt.HoleWidth + ConstNum.SIXTEENTH_INCH);
			Reporting.AddLine("Lgt = Min(g ; W - g) = Min(" + colSplice.BoltGageFlangeLower + "; " + colSplice.SpliceWidthFlange + " - " + colSplice.BoltGageFlangeLower + ") = " + Lgt + ConstUnit.Length);
			Reporting.AddLine("Lnt = Lgt - (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgt + " - (" + dh + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnt + ConstUnit.Length);
			Reporting.AddLine("Lgv = 2 * (e + (N - 1) * s) = 2 * (" + colSplice.BoltVertEdgeDistancePlate + " + (" + colSplice.BoltRowsFlangeLower + " - 1) * " + colSplice.BoltVertSpacing + ") = " + Lgv + ConstUnit.Length);
			Reporting.AddLine("Lnv = Lgv - 2 * (N-0.5)* (dh + " + ConstNum.SIXTEENTH_INCH + ") = " + Lgv + " - 2 * (" + colSplice.BoltRowsFlangeLower + " - 0.5) * (" + colSplice.Bolt.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ") = " + Lnv + ConstUnit.Length);
			CapacityBS = MiscCalculationsWithReporting.BlockShearNew(colSplice.Material.Fu, Lnv, 1, Lnt, Lgv, colSplice.Material.Fy, colSplice.SpliceThicknessFlange, FspliceT, true);
		}

		private static void PlateTensionBoltedNetLowerColumn( ref double An, ref double Ae, ref double CapacityN, double FspliceT )
		{
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("On Net Area:");
			An = (colSplice.SpliceWidthFlange - 2 * (colSplice.Bolt.HoleLength + ConstNum.SIXTEENTH_INCH)) * colSplice.SpliceThicknessFlange;
			Ae = Math.Min(An, 0.85F * colSplice.SpliceWidthFlange * colSplice.SpliceThicknessFlange);
			CapacityN = ConstNum.FIOMEGA0_75N * colSplice.Material.Fu * Ae;
			Reporting.AddHeader("An = (W - 2 * (dh + " + ConstNum.SIXTEENTH_INCH + ")) * t");
			Reporting.AddLine("= (" + colSplice.SpliceWidthFlange + " - 2 * (" + colSplice.Bolt.HoleLength + " + " + ConstNum.SIXTEENTH_INCH + ")) * " + colSplice.SpliceThicknessFlange);
			Reporting.AddLine("= " + An + ConstUnit.Area);

			Reporting.AddHeader("Ae = Min(An, 0.85 * Ag) = " + Ae + ConstUnit.Area);
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + "* Fu * Ae = " + ConstString.FIOMEGA0_75 + " * " + colSplice.Material.Fu + " * " + Ae);
			if (CapacityN >= FspliceT)
				Reporting.AddLine("= " + CapacityN + " >= " + FspliceT + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityN + " << " + FspliceT + ConstUnit.Force + " (NG)");
		}

		private static void BoltBearingLowerColumnCompression( ref double Fbrs, ref double CapacityBr, double FspliceC )
		{
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Bolt Bearing Under Compressive Force:");
			Reporting.AddHeader("Column Flange:");
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, bColumn.Material.Fu, true);
			CapacityBr = 2 * colSplice.BoltRowsFlangeLower * Fbrs * bColumn.Shape.tf;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * N * Fbrs * tf");
			Reporting.AddLine("= 2 * " + colSplice.BoltRowsFlangeLower + " * " + Fbrs + " * " + bColumn.Shape.tf);
			if (CapacityBr >= FspliceC)
				Reporting.AddLine("= " + CapacityBr + " >= " + FspliceC + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + FspliceC + ConstUnit.Force + " (NG)");

			Reporting.AddHeader("Splice Plate:");
			Fbrs = CommonCalculations.SpacingBearing(colSplice.BoltVertSpacing, colSplice.Bolt.HoleWidth, colSplice.Bolt.BoltSize, colSplice.Bolt.HoleType, colSplice.Material.Fu, true);
			CapacityBr = 2 * colSplice.BoltRowsFlangeLower * Fbrs * colSplice.SpliceThicknessFlange;
			Reporting.AddLine(ConstString.PHI + " Rn = 2 * N * Fbrs * t");
			Reporting.AddLine("= 2 * " + colSplice.BoltRowsFlangeLower + " * " + Fbrs + " * " + colSplice.SpliceThicknessFlange);
			if (CapacityBr >= FspliceC)
				Reporting.AddLine("= " + CapacityBr + " >= " + FspliceC + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityBr + " << " + FspliceC + ConstUnit.Force + " (NG)");
		}

		private static void PlateBuckling( double FspliceC )
		{
			double BuckLength;
			double sqr12;
			double k;
			double kL_r;
			double Fcr;
			double FiRn;

			var colSplice = CommonDataStatic.ColumnSplice;

			Reporting.AddHeader("Splice Plate Buckling:");

			if (colSplice.ConnectionUpper == EConnectionStyle.Bolted)
				BuckLength = colSplice.BoltVertEdgeDistanceColumn;
			else
				BuckLength = colSplice.SpliceLengthUpperFlange - colSplice.WeldLengthYUF;
			if (colSplice.ConnectionLower == EConnectionStyle.Bolted)
				BuckLength = BuckLength + colSplice.BoltVertEdgeDistanceColumn;
			else
				BuckLength = BuckLength + colSplice.SpliceLengthLowerFlange - colSplice.WeldLengthYLF;
			
			sqr12 = Math.Sqrt(12);
			k = 0.65;
			Reporting.AddLine("Unbraced Length (L) = " + BuckLength + ConstUnit.Length);
			Reporting.AddLine("Effective Length Factor (K) = 0.65");
			kL_r = BuckLength * k * sqr12 / colSplice.SpliceThicknessFlange;

			Reporting.AddLine("KL / r = k * L / (t / 3.464) = " + k + " * " + BuckLength + " / (" + colSplice.SpliceThicknessFlange + " / 3.464) = " + kL_r);

			Fcr = CommonCalculations.BucklingStress(kL_r, colSplice.Material.Fy, true);

			FiRn = ConstNum.FIOMEGA0_9N * Fcr * colSplice.SpliceWidthFlange * colSplice.SpliceThicknessFlange;
			if (FiRn >= FspliceC)
				Reporting.AddLine(ConstString.PHI + " cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + colSplice.SpliceWidthFlange + " * " + colSplice.SpliceThicknessFlange + " = " + FiRn + " >= " + FspliceC + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine(ConstString.PHI + " cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + colSplice.SpliceWidthFlange + " * " + colSplice.SpliceThicknessFlange + " = " + FiRn + " << " + FspliceC + ConstUnit.Force + " (NG)");
		}

		private static void WeldsLowerColumn( ref double FWeldSizePlateToCol, ref double FWeldSizePlateToFiller, ref double FWeldSizeFillerToCol, double FspliceT, ref double FfSeismic, double Fsplice, ref double usefulweldonPlate, ref double capacity, double FspliceC, double FfillerC )
		{
			var bColumn = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
			var colSplice = CommonDataStatic.ColumnSplice;

			FWeldSizePlateToCol = CommonCalculations.MinimumWeld(bColumn.Shape.tf, colSplice.SpliceThicknessFlange);
			FWeldSizePlateToFiller = CommonCalculations.MinimumWeld(colSplice.FillerThicknessFlangeLower, colSplice.SpliceThicknessFlange);
			FWeldSizeFillerToCol = CommonCalculations.MinimumWeld(bColumn.Shape.tf, colSplice.FillerThicknessFlangeLower);

			Reporting.AddHeader("Welds:");
			if (colSplice.UseSeismic && FspliceT > 0)
				FfSeismic = Math.Max(FspliceT, ColumnSplice.FlangeTensionforSeismic);
			else
				FfSeismic = Fsplice;
			
			if (colSplice.FillerThicknessFlangeLower < ConstNum.QUARTER_INCH)
			{
				if (FWeldSizePlateToCol <= colSplice.FilletWeldSizeFlangeLower)
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(FWeldSizePlateToCol) + " <= " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeFlangeLower) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size = " + CommonCalculations.WeldSize(FWeldSizePlateToCol) + " >> " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeFlangeLower) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Splice Plate:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * colSplice.SpliceThicknessFlange, bColumn.Material.Fu * bColumn.Shape.tf) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t; Fuc * tf) / (0.707" + " * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + colSplice.SpliceThicknessFlange + "; " + bColumn.Material.Fu + " * " + bColumn.Shape.tf + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeFlangeLower - colSplice.FillerThicknessFlangeLower)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w - t_fill = " + (colSplice.FilletWeldSizeFlangeLower - colSplice.FillerThicknessFlangeLower) + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYLF + colSplice.WeldLengthXLF) * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine("");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * wu * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYLF + " + " + colSplice.WeldLengthXLF + ") * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w - t_fill = " + (colSplice.FilletWeldSizeFlangeLower - colSplice.FillerThicknessFlangeLower) + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYLF + colSplice.WeldLengthXLF) * (colSplice.FilletWeldSizeFlangeLower - colSplice.FillerThicknessFlangeLower) * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine("");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * (w - t_fill) * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYLF + " + " + colSplice.WeldLengthXLF + ") * (" + colSplice.FilletWeldSizeFlangeLower + " - " + colSplice.FillerThicknessFlangeLower + ") * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= FfSeismic)
					Reporting.AddLine("= " + capacity + " >= Fsplice = " + FfSeismic + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << Fsplice = " + FfSeismic + ConstUnit.Force + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Splice Plate & Filler:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * (colSplice.SpliceThicknessFlange + colSplice.FillerThicknessFlangeLower), bColumn.Material.Fu * bColumn.Shape.tf) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * (t+t_fill); Fuc * tf) / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + (colSplice.SpliceThicknessFlange + colSplice.FillerThicknessFlangeLower) + "; " + bColumn.Material.Fu + " * " + bColumn.Shape.tf + ") / (0.707 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeFlangeLower)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FilletWeldSizeFlangeLower + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYLF + colSplice.WeldLengthXLF) * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * wu * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYLF + " + " + colSplice.WeldLengthXLF + ") * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FilletWeldSizeFlangeLower + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYLF + colSplice.WeldLengthXLF) * colSplice.FilletWeldSizeFlangeLower * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * w) * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYLF + " + " + colSplice.WeldLengthXLF + ") * " + colSplice.FilletWeldSizeFlangeLower + ") * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= Math.Max(FspliceC + FfillerC, FfSeismic))
					Reporting.AddLine("= " + capacity + " >= Fsplice + Ffiller = " + Math.Max(FspliceC + FfillerC, FfSeismic) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << Fsplice + Ffiller = " + Math.Max(FspliceC + FfillerC, FfSeismic) + ConstUnit.Force + " (NG)");
			}
			else
			{
				if (FWeldSizePlateToFiller <= colSplice.FilletWeldSizeFlangeLower)
					Reporting.AddLine("Min. Weld Size (Plate/Filler) = " + CommonCalculations.WeldSize(FWeldSizePlateToCol) + " <= " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeFlangeLower) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size(Plate/Filler) = " + CommonCalculations.WeldSize(FWeldSizePlateToCol) + " >> " + CommonCalculations.WeldSize(colSplice.FilletWeldSizeFlangeLower) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Splice Plate/Filler:");
				usefulweldonPlate = colSplice.Material.Fu * Math.Min(colSplice.SpliceThicknessFlange, colSplice.FillerThicknessFlangeLower) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Fu * Min(t, t_fill) / (0.707 * Fexx)");
				Reporting.AddLine("= " + colSplice.Material.Fu + " * Min(" + colSplice.SpliceThicknessFlange + "; " + colSplice.FillerThicknessFlangeLower + ") / (0.707* " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FilletWeldSizeFlangeLower)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FilletWeldSizeFlangeLower + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYLF + colSplice.WeldLengthXLF) * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine("");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * wu * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYLF + " + " + colSplice.WeldLengthXLF + ") * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FilletWeldSizeFlangeLower + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYLF + colSplice.WeldLengthXLF) * colSplice.FilletWeldSizeFlangeLower * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine("");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * w * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYLF + " + " + colSplice.WeldLengthXLF + ") * " + colSplice.FilletWeldSizeFlangeLower + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= FfSeismic)
					Reporting.AddLine("= " + capacity + " >= " + FfSeismic + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << " + FfSeismic + ConstUnit.Force + " (NG)");

				if (FWeldSizeFillerToCol <= colSplice.FillerWeldSizeLF)
					Reporting.AddLine("Min. Weld Size (Filler/Flange) = " + CommonCalculations.WeldSize(FWeldSizeFillerToCol) + " <= " + CommonCalculations.WeldSize(colSplice.FillerWeldSizeLF) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Minimum Weld Size (Filler/Flange) = " + CommonCalculations.WeldSize(FWeldSizeFillerToCol) + " >> " + CommonCalculations.WeldSize(colSplice.FillerWeldSizeLF) + ConstUnit.Length + " (NG)");

				Reporting.AddHeader("Useful Weld Size on Filler/Flange:");
				usefulweldonPlate = Math.Min(colSplice.Material.Fu * colSplice.FillerThicknessFlangeLower, bColumn.Material.Fu * bColumn.Shape.tf) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				Reporting.AddLine("wu = Min(Fup * t_fill; Fuc * tf) / (0.707 * Fexx)");
				Reporting.AddLine("= Min(" + colSplice.Material.Fu + " * " + colSplice.FillerThicknessFlangeLower + "; " + bColumn.Material.Fu + " * " + bColumn.Shape.tf + ") / (0.707* " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + ")");
				if (usefulweldonPlate < colSplice.FillerWeldSizeLF)
				{
					Reporting.AddLine("= " + usefulweldonPlate + " << w = " + colSplice.FillerWeldSizeLF + ConstUnit.Length);
					capacity = 2 * (colSplice.WeldLengthYLF + colSplice.WeldLengthXLF) * usefulweldonPlate * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine("");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * (Ly + Lx) * wu * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * (" + colSplice.WeldLengthYLF + " + " + colSplice.WeldLengthXLF + ") * " + usefulweldonPlate + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				else
				{
					Reporting.AddLine("= " + usefulweldonPlate + " >= w = " + colSplice.FillerWeldSizeLF + ConstUnit.Length);
					capacity = 2 * colSplice.FillerWeldLengthLF * colSplice.FillerWeldSizeLF * ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx;
					Reporting.AddLine("");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * Lb * w * " + ConstString.FIOMEGA0_75 + " * 0.4242" + " * Fexx");
					Reporting.AddLine("= 2 * " + colSplice.FillerWeldLengthLF + " * " + colSplice.FillerWeldSizeLF + " * " + ConstString.FIOMEGA0_75 + " *0.4242" + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				}
				if (capacity >= Math.Max(FspliceC + FfillerC, FfSeismic))
					Reporting.AddLine("= " + capacity + " >= Fsplice + Ffiller = " + Math.Max(FspliceC + FfillerC, FfSeismic) + ConstUnit.Force + " (OK)");
				else
					Reporting.AddLine("= " + capacity + " << Fsplice + Ffiller = " + Math.Max(FspliceC + FfillerC, FfSeismic) + ConstUnit.Force + " (NG)");
			}
		}

		private static void PlateTensionWeldedNetLowerColumn( ref double U, ref double CapacityN, double FspliceT )
		{
			var colSplice = CommonDataStatic.ColumnSplice;

			if (colSplice.WeldLengthYLF >= 2 * colSplice.SpliceWidthFlange)
				U = 1;
			else if (colSplice.WeldLengthYLF >= 1.5 * colSplice.SpliceWidthFlange)
				U = 0.87;
			else if (colSplice.WeldLengthYLF >= colSplice.SpliceWidthFlange)
				U = 0.75;
			else
				U = 0.75;

			Reporting.AddHeader("On Effective Net Area:");
			CapacityN = ConstNum.FIOMEGA0_75N * colSplice.Material.Fu * U * colSplice.SpliceWidthFlange * colSplice.SpliceThicknessFlange;
			Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + "* Fu * u * W * t");
			Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + "* " + colSplice.Material.Fu + " * " + U + " * " + colSplice.SpliceWidthFlange + " * " + colSplice.SpliceThicknessFlange);
			if (CapacityN >= FspliceT)
				Reporting.AddLine("= " + CapacityN + " >= " + FspliceT + ConstUnit.Force + " (OK)");
			else
				Reporting.AddLine("= " + CapacityN + " << " + FspliceT + ConstUnit.Force + " (NG)");
		}
	}
}