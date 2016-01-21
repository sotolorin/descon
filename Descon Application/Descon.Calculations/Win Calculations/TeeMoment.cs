using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	internal static class TeeMoment
	{
		internal static void DesignTeeMoment(EMemberType memberType)
		{
			double Tmaxw;
			double TMinW;
			double Tweldlength;
			double AnShear;
			double AgShear;
			double AnTension;
			double AgTension;
			double Tn;
			double bn;
			double Tg;
			double Mp;
			double Ae;
			double Ae1;
			double An;
			double Ag;
			double Cap;
			double a1;
			double Neff;
			double etmin;
			double elmin;
			double BearingCap;
			double Fbs;
			double Fbe;
			double newweld;
			double minw;
			double tfReq;
			double AlfaPr;
			double Delta;
			double Beta;
			double ro;
			double P;
			double Apr;
			double Dpr;
			double a;
			double Bpr;
			double Anreq;
			double AgReq;
			double weldlength;
			double w1;
			double ef;
			double Fcr;
			double kL_r;
			double rg;
			double k;
			double tReq_BlockShear;
			double LnShear;
			double LgShear;
			double LnTension;
			double LgTension;
			double tnReq;
			double HoleSize;
			double tgReq;
			double AnReq1;
			double AgReq1;
			double Tsl;
			double FiRn;
			double tfr;
			double Tfl;
			double MaxTeeFlange;
			double Ff;
			double Rload;
			double w;

			double tempBeamEndSetback;
			double tempShape_tw = 0;
			double tempShape_bf = 0;
			double tempShape_tf;
			double tempShape_d;

			// These need to be initialized
			bool cutTee = false;

			double EffectiveBolts = 0;
			double B = 0;
			double t = 0;
			double fta = 0;
			double BF = 0;
			double tbReq;
			double tf_reqforFlangeBending = 0;
			double NBFlangeTotal = 0;
			double tw_reqforWebYielding = 0;
			double bf_reqforWeldStrength = 0;
			double ermin = 0;
			double esmin = 0;
			double MinTeeFlange = 0;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
			var momentTee = beam.WinConnect.MomentTee;

			if (!momentTee.BoltBeamStem.NumberOfLines_User)
				momentTee.BoltBeamStem.NumberOfLines = momentTee.BoltColumnFlange.Slot3 ? 4 : 2;
			//if (!momentTee.BoltBeamStem.EdgeDistLongDir_User)
			momentTee.BoltBeamStem.EdgeDistLongDir = CommonDataStatic.Units == EUnit.US ? 1.5 : 38;

			if (!momentTee.Column_a_User)
				momentTee.Column_a = momentTee.BoltBeamStem.BoltSize + ConstNum.HALF_INCH + beam.Shape.tf;

			if (!momentTee.Column_e_User)
				momentTee.Column_e = ConstNum.ONEANDHALF_INCHES;

			if (!momentTee.TopLengthAtStem_User)
				momentTee.TopLengthAtStem = 0;

			if (!CommonDataStatic.IsFema)
			{
				switch (CommonDataStatic.BeamToColumnType)
				{
					case EJointConfiguration.BeamToColumnFlange:
					case EJointConfiguration.BeamToHSSColumn:
						if (CommonDataStatic.IsFema)
							Rload = Math.Abs(beam.Moment / (beam.Shape.d + beam.WinConnect.MomentTee.TopTeeShape.tf));
						else
							Rload = Math.Abs(beam.P / 2) + Math.Abs(beam.Moment / beam.Shape.d);
						Ff = Rload;
						MaxTeeFlange = (2 * Math.Min(beam.WinConnect.Beam.TopElValue - beam.WinConnect.Beam.WebAttachTop, beam.WinConnect.Beam.WebAttachBottom - beam.WinConnect.Beam.TopElValue + beam.Shape.d) - 1);
						MaxTeeFlange = Math.Abs(MaxTeeFlange);
						break;
					default:
						return;
				}
				// Flange Connection
				switch (CommonDataStatic.BeamToColumnType)
				{
					case EJointConfiguration.BeamToColumnFlange:
						switch (momentTee.BoltColumnFlange.ASTMType)
						{
							case EBoltASTM.A325:
								fta = ConstNum.FIOMEGA0_75N * ConstNum.COEFFICIENT_90 * Math.Pow(momentTee.BoltColumnFlange.BoltSize, 2) * Math.PI / 4;
								break;
							case EBoltASTM.A490:
								fta = ConstNum.FIOMEGA0_75N * ConstNum.COEFFICIENT_113 * Math.Pow(momentTee.BoltColumnFlange.BoltSize, 2) * Math.PI / 4;
								break;
							default:
								fta = ConstNum.FIOMEGA0_75N * 0.75 * momentTee.BoltColumnFlange.ASTM.Fu * Math.Pow(momentTee.BoltColumnFlange.BoltSize, 2) * Math.PI / 4;
								break;
						}

						double numberOfBolts = Ff / fta;
						if (!momentTee.Column_a_User)
							momentTee.Column_a = momentTee.BoltBeamStem.BoltSize + ConstNum.HALF_INCH + beam.Shape.tf;

						if (numberOfBolts <= 4)
						{
							momentTee.BoltColumnFlangeNumberOfBolts = 4;
							MinTeeFlange = 2 * (momentTee.Column_a + momentTee.Column_e);
						}
						else if (numberOfBolts <= 6)
						{
							momentTee.BoltColumnFlangeNumberOfBolts = 8;
							MinTeeFlange = 2 * (momentTee.Column_a + momentTee.Column_s + momentTee.Column_e);
						}
						else if (numberOfBolts <= 12)
						{
							momentTee.BoltColumnFlangeNumberOfBolts = 16;
							MinTeeFlange = 2 * (momentTee.Column_a + momentTee.Column_s + momentTee.Column_e);
						}

						GetColumnGageforTee(memberType);
						if (momentTee.BoltColumnFlangeNumberOfBolts == 16)
							Tfl = momentTee.Column_g + 2 * (momentTee.Column_g1 + momentTee.BoltColumnFlange.EdgeDistLongDir);
						else
							Tfl = momentTee.Column_g + 2 * momentTee.BoltColumnFlange.EdgeDistLongDir;

						momentTee.TopLengthAtFlange = Tfl;

						break;
					case EJointConfiguration.BeamToHSSColumn:
						if (CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].WebOrientation == EWebOrientation.OutOfPlane)
							Tfl = column.Shape.d + 2;
						else
							Tfl = column.Shape.bf + 2;

						momentTee.TopLengthAtFlange = Tfl;
						momentTee.TopLengthAtStem = momentTee.TopLengthAtFlange;
						tf_reqforFlangeBending = Math.Sqrt(Ff / (ConstNum.FIOMEGA0_9N * 6.25 * momentTee.TopMaterial.Fy));
						tfr = tf_reqforFlangeBending - ConstNum.SIXTEENTH_INCH;

						do
						{
							tfr += ConstNum.SIXTEENTH_INCH;
							FiRn = ConstNum.FIOMEGA0_9N * 6.25 * Math.Pow(tfr, 2) * momentTee.TopMaterial.Fy * Math.Min(momentTee.TopLengthAtFlange / (20 * tfr), 1);
						} while (!(FiRn >= Ff));

						tf_reqforFlangeBending = tfr;
						tw_reqforWebYielding = Ff / (2 * 1 * Math.Min(2.5 * momentTee.TopTeeShape.kdet + 2.5 * column.Shape.tf, momentTee.TopLengthAtFlange / 2) * momentTee.TopMaterial.Fy); //  "2.5 * Column.tf" is "N"
						bf_reqforWeldStrength = Ff / (2 * 0.8 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 5 / 8 * column.Shape.tf);
						momentTee.CWeld = 5 / 8.0 * column.Shape.tf;
						GetColumnGageforTee(memberType);
						break;
				}

				if (momentTee.TeeConnectionStyle == EConnectionStyle.Bolted)
				{
					momentTee.BoltBeamStem.NumberOfRows = (int) Math.Ceiling(Ff / momentTee.BoltBeamStem.BoltStrength / momentTee.BoltBeamStem.NumberOfLines);
					NBFlangeTotal = momentTee.BoltBeamStem.NumberOfRows * momentTee.BoltBeamStem.NumberOfLines;
					if (momentTee.BoltBeamStem.EdgeDistTransvDir == 0)
						momentTee.BoltBeamStem.EdgeDistTransvDir = momentTee.BoltBeamStem.MinEdgeSheared;

					GetBeamGageforTee(memberType);
					if (momentTee.BoltBeamStem.NumberOfLines == 4)
						Tsl = momentTee.Beam_g + 2 * (momentTee.Beam_g1 + momentTee.BoltBeamStem.EdgeDistTransvDir);
					else
						Tsl = momentTee.Beam_g + 2 * momentTee.BoltBeamStem.EdgeDistTransvDir;

					if (Tsl > momentTee.TopLengthAtStem)
						momentTee.TopLengthAtStem = Tsl;

					if (Math.Abs(momentTee.TopLengthAtFlange - momentTee.TopLengthAtStem) <= 2)
					{
						momentTee.TopLengthAtFlange = Math.Max(momentTee.TopLengthAtFlange, momentTee.TopLengthAtStem);
						momentTee.TopLengthAtStem = momentTee.TopLengthAtFlange;
					}

					momentTee.BoltBeamStem.EdgeDistTransvDir = (momentTee.TopLengthAtStem - momentTee.Beam_g) / 2;

					AgReq1 = Ff / (ConstNum.FIOMEGA0_9N * momentTee.TopMaterial.Fy); // yielding of web
					AnReq1 = Ff / (ConstNum.FIOMEGA0_75N * momentTee.TopMaterial.Fu); // fracture of web

					tgReq = AgReq1 / momentTee.TopLengthAtStem;
					HoleSize = momentTee.BoltBeamStem.HoleWidth;
					tnReq = AnReq1 / (momentTee.TopLengthAtStem - Math.Max(0.15 * momentTee.TopLengthAtStem, momentTee.BoltBeamStem.NumberOfLines * (HoleSize + ConstNum.SIXTEENTH_INCH)));

					if (momentTee.BoltBeamStem.NumberOfLines == 4)
					{
						LgTension = Math.Min(momentTee.Beam_g, 2 * momentTee.BoltBeamStem.EdgeDistTransvDir) + 2 * momentTee.Beam_g1;
						LnTension = LgTension - 3 * (momentTee.BoltBeamStem.HoleWidth + ConstNum.SIXTEENTH_INCH);
					}
					else
					{
						LgTension = Math.Min(momentTee.Beam_g, 2 * momentTee.BoltBeamStem.EdgeDistTransvDir);
						LnTension = LgTension - (momentTee.BoltBeamStem.HoleWidth + ConstNum.SIXTEENTH_INCH);
					}
					LgShear = 2 * ((momentTee.BoltBeamStem.NumberOfLines - 1) * momentTee.BoltBeamStem.SpacingLongDir + momentTee.BoltBeamStem.EdgeDistLongDir);
					LnShear = LgShear - 2 * (momentTee.BoltBeamStem.NumberOfRows - 0.5) * (momentTee.BoltBeamStem.HoleLength + ConstNum.SIXTEENTH_INCH);

					tReq_BlockShear = Rload / MiscCalculationsWithReporting.BlockShearNew(momentTee.TopMaterial.Fu, LnShear, 1, LnTension, LgShear, momentTee.TopMaterial.Fy, 1, 0, false);

					Fbe = CommonCalculations.EdgeBearing(momentTee.BoltBeamStem.EdgeDistTransvDir, momentTee.BoltBeamStem.HoleLength, momentTee.BoltBeamStem.BoltSize, momentTee.TopMaterial.Fu, momentTee.BoltBeamStem.HoleType, false);
					Fbs = CommonCalculations.SpacingBearing(momentTee.BoltBeamStem.SpacingLongDir, momentTee.BoltBeamStem.HoleLength, momentTee.BoltBeamStem.BoltSize, momentTee.BoltBeamStem.HoleType, momentTee.TopMaterial.Fu, false);
					BearingCap = momentTee.BoltBeamStem.NumberOfLines * (Fbe + Fbs * (momentTee.BoltBeamStem.NumberOfRows - 1));
					tbReq = Ff / BearingCap;

					t = NumberFun.Round(Math.Max(Math.Max(tgReq, tnReq), Math.Max(tReq_BlockShear, tbReq)), 16);

					do
					{
						k = 0.65;
						beam.WinConnect.Fema.L = (beam.EndSetback + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange - momentTee.TopTeeShape.tf);
						rg = t / Math.Sqrt(12);
						kL_r = k * beam.WinConnect.Fema.L / rg;

						Fcr = CommonCalculations.BucklingStress(kL_r, momentTee.TopMaterial.Fy, false);

						FiRn = ConstNum.FIOMEGA0_9N * Fcr * momentTee.TopLengthAtFlange * t;

						t += ConstNum.SIXTEENTH_INCH;
					} while (FiRn < Ff);
				}

				if (momentTee.TeeConnectionStyle == EConnectionStyle.Bolted)
				{
					ermin = momentTee.BoltColumnFlange.MinEdgeRolled;
					esmin = momentTee.BoltColumnFlange.MinEdgeSheared;
					ef = MiscCalculationsWithReporting.MinimumEdgeDist(momentTee.BoltColumnFlange.BoltSize, Ff / NBFlangeTotal, beam.Material.Fu * beam.Shape.tf, momentTee.BoltColumnFlange.MinEdgeSheared, momentTee.BoltColumnFlange.HoleLength, momentTee.BoltColumnFlange.HoleType);
					beam.WinConnect.Beam.BoltEdgeDistanceOnFlange = Math.Max(ef, beam.WinConnect.Beam.BoltEdgeDistanceOnFlange);
				}

				// First find the Top Tee Shape (i = 0) then find the Bottom Tee Shape (i = 1)
				for (int i = 0; i < 2; i++)
				{
					bool shart = false;
					bool shart1;
					bool shart2;
					bool shart3;

					foreach (var tee in CommonDataStatic.ShapesTee)
					{
						if (tee.Value.Name == ConstString.NONE)
							continue;

						Shape tempTee;
						tempBeamEndSetback = beam.EndSetback;

						// If the user chose Tee's or we don't need them then skip trying to find them
						if (i == 0 && momentTee.TopTeeShape_User)
							continue;
						else if (i == 1 && (momentTee.BottomTeeShape_User || momentTee.TeeConnectionStyle == EConnectionStyle.Bolted))
							continue;
						else
							tempTee = tee.Value;

						if (tempBeamEndSetback < tempTee.kdet)
							tempBeamEndSetback = tempTee.kdet;

						tempShape_tf = tempTee.tf;

						if (momentTee.TeeConnectionStyle == EConnectionStyle.Bolted)
							tempShape_d = momentTee.BoltBeamStem.SpacingLongDir * (momentTee.BoltBeamStem.NumberOfRows - 1) + momentTee.BoltBeamStem.EdgeDistLongDir + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + tempBeamEndSetback;
						else
						{
							w = CommonCalculations.MinimumWeld(tempTee.tw, beam.Shape.tf);
							if (tempTee.tw >= ConstNum.THREE_EIGHTS_INCH)
								w = NumberFun.ConvertFromFraction(5);
							if (tempTee.tw <= ConstNum.QUARTER_INCH)
								w1 = tempTee.tw;
							else
								w1 = tempTee.tw - ConstNum.SIXTEENTH_INCH;

							momentTee.BWeld = Math.Min(w, w1);
							weldlength = Ff / (ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentTee.BWeld);

							momentTee.TopLengthAtStem = Math.Min(momentTee.TopLengthAtFlange, NumberFun.Round(beam.Shape.bf - 2 * momentTee.BWeld - ConstNum.QUARTER_INCH, 8));
							tempShape_d = (weldlength - momentTee.TopLengthAtStem) / 2 + Math.Max(tempBeamEndSetback, 1.732 * Math.Abs(momentTee.TopLengthAtFlange - momentTee.TopLengthAtStem) / 2);
							AgReq = Ff / (ConstNum.FIOMEGA0_9N * momentTee.TopMaterial.Fy);
							Anreq = Ff / (ConstNum.FIOMEGA0_75N * momentTee.TopMaterial.Fu);
							tgReq = AgReq / momentTee.TopLengthAtStem;
							tnReq = Anreq / momentTee.TopLengthAtStem;
							t = Math.Max(tgReq, tnReq) - ConstNum.SIXTEENTH_INCH;

							do
							{
								t += ConstNum.SIXTEENTH_INCH;
								k = 0.65;
								beam.WinConnect.Fema.L = (tempBeamEndSetback - tempShape_tf);
								rg = t / Math.Sqrt(12);
								kL_r = (k * beam.WinConnect.Fema.L / rg);

								Fcr = CommonCalculations.BucklingStress(kL_r, momentTee.TopMaterial.Fy, false);

								FiRn = ConstNum.FIOMEGA0_9N * Fcr * momentTee.TopLengthAtStem * t;
							} while (FiRn < Ff);

							if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
								tempShape_tw = Math.Max(t, tw_reqforWebYielding);
							else
								tempShape_tw = t;
						}

						if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
						{
							tempShape_tf = tf_reqforFlangeBending;
							tempShape_bf = bf_reqforWeldStrength;
							shart1 = tempTee.d >= tempShape_d && tempTee.tw >= tempShape_tw && tempTee.tf >= tempShape_tf && tempTee.bf >= tempShape_bf;
							if (shart1)
							{
								if (momentTee.TeeConnectionStyle == EConnectionStyle.Bolted)
								{
									momentTee.TopTeeShape = tempTee.ShallowCopy();
									momentTee.BottomTeeShape = tempTee.ShallowCopy();
								}
								else
								{
									switch (i)
									{
										case 0:
											momentTee.TopTeeShape = tempTee.ShallowCopy();
											break;
										case 1:
											momentTee.BottomTeeShape = tempTee.ShallowCopy();
											break;
									}
								}
								break;
							}
						}
						else
						{
							shart1 = tempTee.d >= tempShape_d && tempTee.tw >= tempShape_tw;
							shart2 = MaxTeeFlange + tempTee.tw >= tempTee.bf;
							shart3 = MinTeeFlange + tempTee.tw <= tempTee.bf;

							if (i == 0)
							{
								shart = shart1 && shart2 && shart3;
								cutTee = false;
								if (momentTee.Column_e < ermin)
								{
									momentTee.Column_e = ermin;
									momentTee.BoltColumnFlange.EdgeDistTransvDir = momentTee.Column_e;
								}
								BF = tempTee.bf;
							}
							else if (shart1 && MaxTeeFlange >= MinTeeFlange)
							{
								shart = shart1;
								cutTee = true;
								if (momentTee.Column_e < esmin)
								{
									momentTee.Column_e = esmin;
									momentTee.BoltColumnFlange.EdgeDistTransvDir = momentTee.Column_e;
								}
								BF = MaxTeeFlange + tempTee.tw;
							}
							if (shart)
							{
								if (momentTee.BoltColumnFlangeNumberOfBolts <= 4)
								{
									momentTee.BoltColumnFlangeNumberOfBolts = 4;
									EffectiveBolts = 4;
									t = Ff / EffectiveBolts;
								}
								else if (momentTee.BoltColumnFlangeNumberOfBolts <= 8)
								{
									momentTee.BoltColumnFlangeNumberOfBolts = 8;
									EffectiveBolts = 6;
									t = Ff / EffectiveBolts;
								}
								else if (momentTee.BoltColumnFlangeNumberOfBolts <= 16)
								{
									momentTee.BoltColumnFlangeNumberOfBolts = 16;
									EffectiveBolts = 12;
									t = Ff / EffectiveBolts;
								}

								B = momentTee.Column_a;
								Bpr = momentTee.Column_a - 0.5 * momentTee.BoltColumnFlange.BoltSize;
								a = (BF - tempTee.tw) / 2 - momentTee.Column_a;

								if (a > 1.25 * momentTee.Column_a)
									a = 1.25 * momentTee.Column_a;

								if (a < momentTee.Column_e)
									continue;

								Dpr = momentTee.BoltColumnFlange.HoleLength;
								Apr = a + 0.5 * momentTee.BoltColumnFlange.BoltSize;

								if (momentTee.BoltColumnFlangeNumberOfBolts == 16)
									P = momentTee.TopLengthAtFlange / 4;
								else
									P = momentTee.TopLengthAtFlange / 2;

								ro = Bpr / Apr;
								Beta = (fta / t - 1) / ro;
								Delta = 1 - Dpr / P;
								if (Beta >= 1)
									AlfaPr = 1;
								else
									AlfaPr = Math.Min(1, Beta / (1 - Beta) / Delta);

								tfReq = Math.Sqrt(4 / ConstNum.FIOMEGA0_9N * t * Bpr / (P * momentTee.TopMaterial.Fy * (1 + Delta * AlfaPr)));
								if (tempTee.tf >= tfReq)
								{
									if (momentTee.TeeConnectionStyle == EConnectionStyle.Bolted)
									{
										momentTee.TopTeeShape = tempTee.ShallowCopy();
										momentTee.BottomTeeShape = tempTee.ShallowCopy();
									}
									else
									{
										switch (i)
										{
											case 0:
												momentTee.TopTeeShape = tempTee.ShallowCopy();
												break;
											case 1:
												momentTee.BottomTeeShape = tempTee.ShallowCopy();
												break;
										}
									}
									break;
								}
							}
						}
					}

					if (cutTee)
						tempShape_bf = MaxTeeFlange + momentTee.TopTeeShape.tw;
					else
						tempShape_bf = momentTee.TopTeeShape.bf;

					if (tempShape_tw == 0)
						tempShape_tw = momentTee.TopTeeShape.tw;

					if (momentTee.TeeConnectionStyle == EConnectionStyle.Welded)
					{
						minw = CommonCalculations.MinimumWeld(tempShape_tw, beam.Shape.tf);
						if (tempShape_tw <= ConstNum.QUARTER_INCH)
							w1 = tempShape_tw;
						else
							w1 = tempShape_tw - ConstNum.SIXTEENTH_INCH;

						newweld = Math.Min(minw, w1);
						momentTee.BWeld = NumberFun.Round(Math.Max(momentTee.BWeld, newweld), 16);
					}
				}
			}

			if (CommonDataStatic.IsFema)
			{
				Rload = Math.Abs(beam.Moment / (beam.Shape.d + momentTee.TopTeeShape.tf));
				Ff = Rload;
			}
			else
			{
				//if (!momentTee.BoltBeamStem.EdgeDistLongDir_User)
				momentTee.BoltBeamStem.EdgeDistLongDir = momentTee.TopTeeShape.d - beam.EndSetback - beam.WinConnect.Beam.BoltEdgeDistanceOnFlange - (momentTee.BoltBeamStem.NumberOfRows - 1) * momentTee.BoltBeamStem.SpacingLongDir;
				if (!momentTee.BoltBeamStem.EdgeDistTransvDir_User)
					momentTee.BoltBeamStem.EdgeDistTransvDir = (momentTee.TopLengthAtStem - momentTee.Beam_g - 2 * momentTee.Beam_g1) / 2;
				Ff = Math.Abs(beam.P / 2) + Math.Abs(beam.Moment / (beam.Shape.d + momentTee.TopTeeShape.tw / 2 + momentTee.BottomTeeShape.tw / 2));
			}

			// Tees are the same when bolted, so match the data
			if (momentTee.TeeConnectionStyle == EConnectionStyle.Bolted)
			{
				momentTee.BottomMaterial = momentTee.TopMaterial.ShallowCopy();
				momentTee.BottomLengthAtFlange = momentTee.TopLengthAtFlange;
				momentTee.BottomLengthAtStem = momentTee.TopLengthAtStem;
				momentTee.BottomWeldSize = momentTee.TopWeldSize;
			}

			if (momentTee.TopTeeShape.Name != ConstString.NONE)
			{
				momentTee.Column_a = (momentTee.TopTeeShape.bf - momentTee.TopTeeShape.tw - 2 * momentTee.Column_e) / 2;
				if (!beam.EndSetback_User && beam.EndSetback < momentTee.TopTeeShape.kdet)
					beam.EndSetback = momentTee.TopTeeShape.kdet;
				beam.WinConnect.Beam.BotAttachThick = tempShape_tw;
				beam.WinConnect.Beam.TopAttachThick = tempShape_tw;
				beam.WinConnect.Beam.WebHeight = beam.Shape.d - (tempShape_bf - tempShape_tw);
			}

			Reporting.AddMainHeader("Moment Connection Using Tee Stubs:");
			Reporting.AddLine("Tee on Top Flange: " + momentTee.TopTeeShape.Name);
			Reporting.AddLine("Tee on Bottom Flange: " + momentTee.BottomTeeShape.Name);
			Reporting.AddLine("Tee Material: " + momentTee.TopMaterial.Name);
			Reporting.AddLine("Tee Flange: Bolted");
			Reporting.AddLine("Tee Stem: " + momentTee.TeeConnectionStyle); // DESGEN.TopTee.BeamSide
			Reporting.AddLine(string.Empty);
			Reporting.AddLine("Beam Axial Load (P) = " + beam.P + ConstUnit.Force);
			Reporting.AddLine("Beam Moment (M) = " + (beam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot);
			if (CommonDataStatic.Units == EUnit.US)
			{
				Reporting.AddLine("Flange Force (Ff) = P / 2 + M / (d + t)");
				Reporting.AddLine("= " + Math.Abs(beam.P) + " / 2 + " + Math.Abs(beam.Moment) + " / (" + beam.Shape.d + " + " + (momentTee.TopTeeShape.tw / 2 + momentTee.BottomTeeShape.tw / 2) + ")");
			}
			else
			{
				Reporting.AddLine("Flange Force (Ff) = P / 2 + M / (d + t)");
				Reporting.AddLine("= " + Math.Abs(beam.P) + " / 2 + " + Math.Abs(beam.Moment) + " / (" + beam.Shape.d + " + " + (momentTee.TopTeeShape.tw / 2 + momentTee.BottomTeeShape.tw / 2) + ")");
			}
			Reporting.AddLine("= " + Ff + ConstUnit.Force);

			int numberOfRowsOfBolts = 0;

			switch (beam.ShearConnection)
			{
				case EShearCarriedBy.SinglePlate:
					numberOfRowsOfBolts = beam.WinConnect.ShearWebPlate.Bolt.NumberOfRows;
					break;
				case EShearCarriedBy.EndPlate:
					numberOfRowsOfBolts = beam.WinConnect.ShearEndPlate.Bolt.NumberOfRows;
					break;
				case EShearCarriedBy.ClipAngle:
					numberOfRowsOfBolts = beam.WinConnect.ShearClipAngle.BoltOslOnSupport.NumberOfRows;
					break;
				case EShearCarriedBy.Tee:
					numberOfRowsOfBolts = beam.WinConnect.ShearWebTee.StemNumberOfRows;
					break;
			}

			double upperTeeDistance = beam.WinConnect.Beam.DistanceToFirstBolt - momentTee.BoltColumnFlange.EdgeDistLongDir;
			double upperTeeCheck = (momentTee.TopTeeShape.bf - momentTee.TopTeeShape.t) / 2 + ConstNum.HALF_INCH;
			double lowerTeeDistance = beam.WinConnect.Beam.DistanceToFirstBolt + (numberOfRowsOfBolts - 1) * momentTee.BoltColumnFlange.SpacingLongDir + momentTee.BoltColumnFlange.EdgeDistLongDir;
			double lowerTeeCheck = beam.Shape.d - (momentTee.TopTeeShape.bf - momentTee.TopTeeShape.tw) / 2 - ConstNum.HALF_INCH;

			Reporting.AddHeader("Dimensional Clearances");
			Reporting.AddLine("Lv - BoltDistToHorizEdge << (Tee bf - Tee tw) / 2 + " + ConstNum.HALF_INCH);
			Reporting.AddLine(beam.WinConnect.Beam.DistanceToFirstBolt + " - " + momentTee.BoltColumnFlange.EdgeDistLongDir + " << " + "(" + momentTee.TopTeeShape.bf + " - " + momentTee.TopTeeShape.tw + ") / 2 + " + ConstNum.HALF_INCH);
			if (upperTeeDistance < upperTeeCheck)
			{
				Reporting.AddLine(upperTeeDistance + " << " + upperTeeCheck);
				Reporting.AddLine("Shear connection clashes with upper tee flange (NG)");
			}
			else
				Reporting.AddLine(upperTeeDistance + " >= " + upperTeeCheck + "(OK)");

			Reporting.AddLine(string.Empty);
			Reporting.AddLine("Lv + (n - 1) * s + BoltDistanceToHorizEdge >> beam d - (Tee bf - Tee tw) / 2 - " + ConstNum.HALF_INCH);
			Reporting.AddLine(beam.WinConnect.Beam.DistanceToFirstBolt + " + (" + numberOfRowsOfBolts + " - 1) * " + momentTee.BoltColumnFlange.SpacingLongDir + " + " + momentTee.BoltColumnFlange.EdgeDistLongDir + " >> " + beam.Shape.d + " - " + "(" + momentTee.TopTeeShape.bf + " - " + momentTee.TopTeeShape.tw + ") / 2 - " + ConstNum.HALF_INCH);
			if (lowerTeeDistance > lowerTeeCheck)
			{
				Reporting.AddLine(lowerTeeDistance + " >> " + lowerTeeCheck);
				Reporting.AddLine("Shear connection clashes with lower tee flange (NG)");
			}
			else
				Reporting.AddLine(lowerTeeDistance + " <= " + lowerTeeCheck + "(OK)");

			switch (CommonDataStatic.BeamToColumnType)
			{
				case EJointConfiguration.BeamToHSSColumn:
					FiRn = 2 * 0.8 * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 5 / 8.0 * column.Shape.tf * momentTee.TopTeeShape.bf;

					Reporting.AddGoToHeader("Tee Flange to HSS Welds (Flare Bevel):");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * 0.8 * 0.6 * Fexx * (5 / 8 * t) * bf");
					Reporting.AddLine("= 2 * 0.8 * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * (" + (5 / 8.0 * column.Shape.tf) + ") * " + momentTee.TopTeeShape.bf);
					if (FiRn >= Ff)
						Reporting.AddCapacityLine("= " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / FiRn, "Tee Flange to HSS Welds (Flare Bevel)", memberType);
					else
						Reporting.AddCapacityLine("= " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / FiRn, "Tee Flange to HSS Welds (Flare Bevel)", memberType);

					FiRn = ConstNum.FIOMEGA0_9N * 6.25 * Math.Pow(momentTee.TopTeeShape.tf, 2) * momentTee.TopMaterial.Fy * Math.Min(momentTee.TopLengthAtFlange / (20 * momentTee.TopTeeShape.tf), 1);
					Reporting.AddHeader("Tee Flange Bending:");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * 0.5 * " + ConstString.FIOMEGA0_9 + " * 6.25 * tf² * Fy * Min[L / (20 * tf), 1]"); // 50% reduction because load is less then 10tf from end of tee
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * 6.25 * " + momentTee.TopTeeShape.tf + "² * " + momentTee.TopMaterial.Fy + " * Min[" + momentTee.TopLengthAtFlange + " / (20 * " + momentTee.TopTeeShape.tf + "), 1]");
					if (FiRn >= Ff)
						Reporting.AddCapacityLine("= " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / FiRn, "Tee Flange Bending", memberType);
					else
						Reporting.AddCapacityLine("= " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / FiRn, "Tee Flange Bending", memberType);
					break;
				default:
					Reporting.AddGoToHeader("Tee Flange Bolts:");
					Reporting.AddLine(momentTee.BoltColumnFlangeNumberOfBolts + " Bolts " + momentTee.BoltColumnFlange.BoltName);
					Reporting.AddLine("Bolt Holes on Tee: " + momentTee.BoltColumnFlange.HoleWidth + ConstUnit.Length + " Vert. X " + momentTee.BoltColumnFlange.HoleLength + ConstUnit.Length + " Horiz.");
					Reporting.AddLine("Bolt Holes on Column: " + momentTee.BoltColumnFlange.HoleWidth + ConstUnit.Length + " Vert. X " + momentTee.BoltColumnFlange.HoleLength + ConstUnit.Length + " Horiz.");
					elmin = momentTee.BoltColumnFlange.MinEdgeSheared;

					Reporting.AddHeader("Distance to End (el):");
					if (momentTee.BoltColumnFlange.EdgeDistLongDir >= elmin)
						Reporting.AddLine("= " + momentTee.BoltColumnFlange.EdgeDistLongDir + " >= " + elmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + momentTee.BoltColumnFlange.EdgeDistLongDir + " << " + elmin + ConstUnit.Length + " (NG)");
					if (cutTee)
						etmin = elmin;
					else
						etmin = momentTee.BoltColumnFlange.MinEdgeRolled;

					Reporting.AddLine("Distance to Horizontal Edge (et):");
					if (momentTee.BoltColumnFlange.EdgeDistTransvDir >= etmin)
						Reporting.AddLine("= " + momentTee.BoltColumnFlange.EdgeDistTransvDir + " >= " + etmin + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + momentTee.BoltColumnFlange.EdgeDistTransvDir + " << " + etmin + ConstUnit.Length + " (NG)");

					MiscCalculationsWithReporting.BeamCheck(beam.MemberType);

					Reporting.AddGoToHeader("Tee Flange Pullout:");
					if (momentTee.BoltColumnFlangeNumberOfBolts <= 4)
					{
						momentTee.BoltColumnFlangeNumberOfBolts = 4;
						EffectiveBolts = 4;
						t = Ff / EffectiveBolts;
					}
					else if (momentTee.BoltColumnFlangeNumberOfBolts <= 8)
					{
						momentTee.BoltColumnFlangeNumberOfBolts = 8;
						EffectiveBolts = 6;
						t = Ff / EffectiveBolts;
					}
					else if (momentTee.BoltColumnFlangeNumberOfBolts <= 16)
					{
						momentTee.BoltColumnFlangeNumberOfBolts = 16;
						EffectiveBolts = 12;
						t = Ff / EffectiveBolts;
					}

					Neff = EffectiveBolts;
					Reporting.AddLine("Effective Number of Bolts = " + EffectiveBolts);
					Reporting.AddLine("Flange Force per Effective Bolt = Ff / Neff = " + Ff + " / " + EffectiveBolts + " = " + t + ConstUnit.Force);
					Reporting.AddLine(ConstString.DES_OR_ALLOWABLE_STRENGTH + " of One Bolt (rn) = " + ConstString.PHI + " Ft * Ab");
					switch (momentTee.BoltColumnFlange.ASTMType)
					{
						case EBoltASTM.A325:
							fta = ConstNum.FIOMEGA0_75N * ConstNum.COEFFICIENT_90 * Math.Pow(momentTee.BoltColumnFlange.BoltSize, 2) * Math.PI / 4;
							Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + ConstNum.COEFFICIENT_90 + " * " + momentTee.BoltColumnFlange.BoltSize + "² * " + Math.PI + " / 4");
							break;
						case EBoltASTM.A490:
							fta = ConstNum.FIOMEGA0_75N * ConstNum.COEFFICIENT_113 * Math.Pow(momentTee.BoltColumnFlange.BoltSize, 2) * Math.PI / 4;
							Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + ConstNum.COEFFICIENT_113 + " * " + momentTee.BoltColumnFlange.BoltSize + "² * " + Math.PI + " / 4");
							break;
					}
					if (fta >= Ff / Neff)
						Reporting.AddCapacityLine("= " + fta + " >= " + Ff / Neff + ConstUnit.Force + " (OK)", Ff / Neff / fta, "Effective Number of Bolts", memberType);
					else
						Reporting.AddCapacityLine("= " + fta + " << " + Ff / Neff + ConstUnit.Force + " (NG)", Ff / Neff / fta, "Effective Number of Bolts", memberType);

					Reporting.AddHeader("Check Flange Thickness of Tee:");
					Bpr = momentTee.Column_a - 0.5 * momentTee.BoltColumnFlange.BoltSize;
					a = (momentTee.TopTeeShape.bf - momentTee.TopTeeShape.tw) / 2 - momentTee.Column_a;
					Reporting.AddLine("b = " + momentTee.Column_a + ConstUnit.Length);
					Reporting.AddLine("b'= b - d / 2 = " + momentTee.Column_a + " - " + momentTee.BoltColumnFlange.BoltSize + " / 2 = " + Bpr + ConstUnit.Length);
					Reporting.AddLine("a = (bf - tw) / 2 - b");
					Reporting.AddLine("= (" + momentTee.TopTeeShape.bf + " - " + momentTee.TopTeeShape.tw + ") / 2 - " + momentTee.Column_a);
					a1 = 1.25 * momentTee.Column_a;
					if (a > a1)
					{
						Reporting.AddLine("= " + a + " >> 1.25 * b,  use a = 1.25 * " + B + " = " + a1 + ConstUnit.Length);
						a = a1;
					}
					else
						Reporting.AddLine("= " + a + " <= 1.25 * b = 1.25 * " + momentTee.Column_a + " = " + a1 + ConstUnit.Length + " (OK)");

					Dpr = momentTee.BoltColumnFlange.HoleLength;
					Apr = a + 0.5 * momentTee.BoltColumnFlange.BoltSize;
					Reporting.AddLine("d'= " + momentTee.BoltColumnFlange.HoleLength + ConstUnit.Length);
					Reporting.AddLine("a'= a + d / 2 = " + a + " + " + momentTee.BoltColumnFlange.BoltSize + " / 2 = " + Apr + ConstUnit.Length);
					ro = Bpr / Apr;
					if (momentTee.BoltColumnFlangeNumberOfBolts == 16)
					{
						P = momentTee.TopLengthAtFlange / 4;
						Reporting.AddLine("p = Length of Tee (L_Tee) / 4 = " + momentTee.TopLengthAtFlange + " / 4 = " + P + ConstUnit.Length);
					}
					else
					{
						P = momentTee.TopLengthAtFlange / 2;
						Reporting.AddLine("p = Length of Tee (L_Tee) / 2 = " + momentTee.TopLengthAtFlange + " / 2 = " + P + ConstUnit.Length);
					}
					Reporting.AddLine("ro = b' / a' = " + Bpr + " / " + Apr + " = " + ro);
					Beta = (fta / t - 1) / ro;
					Reporting.AddLine("Beta = (B / T - 1) / ro = (" + fta + " / " + t + " - 1) / " + ro + " = " + Beta);
					Delta = 1 - Dpr / P;
					Reporting.AddLine("Delta = 1 - d'/p = 1 - " + Dpr + " / " + P + " = " + Delta);
					if (Beta >= 1)
					{
						Reporting.AddLine("Beta >= 1,  Alfa'= 1");
						AlfaPr = 1;
					}
					else
					{
						AlfaPr = Math.Min(1, Beta / (1 - Beta) / Delta);
						Reporting.AddLine("Beta << 1");
						Reporting.AddLine("Alfa'= Min(1, (Beta / (1 - Beta)) / Delta)");
						Reporting.AddLine("= Min(1, (" + Beta + " / (1 - " + Beta + ")) / " + Delta + ") = " + AlfaPr);
					}
					tfReq = Math.Sqrt(4 / ConstNum.FIOMEGA0_9N * t * Bpr / (P * momentTee.TopMaterial.Fy * (1 + Delta * AlfaPr)));

					Reporting.AddLine("Required flange thickness (treq)");
					Reporting.AddLine("= (4 / " + ConstString.FIOMEGA0_9 + " * T * b' / (p * Fy * (1 + Delta * Alfa')))^0.5");
					Reporting.AddLine("= (4 / " + ConstString.FIOMEGA0_9 + " * " + t + " * " + Bpr + " / (" + P + " * " + momentTee.TopMaterial.Fy + " * (1 + " + Delta + " * " + AlfaPr + ")))^0.5");
					if (tfReq <= momentTee.TopTeeShape.tf)
						Reporting.AddLine("= " + tfReq + " <= " + momentTee.TopTeeShape.tf + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("= " + tfReq + " >> " + momentTee.TopTeeShape.tf + ConstUnit.Length + " (NG)");
					break;
			}

			if (momentTee.TeeConnectionStyle == EConnectionStyle.Bolted)
			{
				Reporting.AddGoToHeader("Bolts on Tee Stem:");
				Reporting.AddHeader(momentTee.BoltBeamStem.NumberOfBolts + " Bolts " + momentTee.BoltBeamStem.BoltName);
				Reporting.AddLine("Bolt Holes on Tee: " + momentTee.BoltBeamStem.HoleWidth + ConstUnit.Length + " Trans. X " + momentTee.BoltBeamStem.HoleLength + ConstUnit.Length + "  Long.");
				Reporting.AddLine("Bolt Holes on Beam: " + momentTee.BoltBeamStem.HoleWidth + ConstUnit.Length + " Trans. X " + momentTee.BoltBeamStem.HoleLength + ConstUnit.Length + " Long.");
				elmin = momentTee.BoltBeamStem.MinEdgeSheared;

				Reporting.AddHeader("Distance to End (el):");
				if (momentTee.BoltBeamStem.EdgeDistLongDir >= elmin)
					Reporting.AddLine("= " + momentTee.BoltBeamStem.EdgeDistLongDir + " >= " + elmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + momentTee.BoltBeamStem.EdgeDistLongDir + " << " + elmin + ConstUnit.Length + " (NG)");

				elmin = momentTee.BoltBeamStem.MinEdgeSheared + momentTee.BoltBeamStem.Eincr;

				Reporting.AddLine("Distance to End (et):");
				if (momentTee.BoltBeamStem.EdgeDistTransvDir >= elmin)
					Reporting.AddLine("= " + momentTee.BoltBeamStem.EdgeDistTransvDir + " >= " + elmin + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("= " + momentTee.BoltBeamStem.EdgeDistTransvDir + " << " + elmin + ConstUnit.Length + " (NG)");

				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Shear Strength = n * Fv");
				Cap = momentTee.BoltBeamStem.NumberOfBolts * momentTee.BoltBeamStem.BoltStrength;
				if (Cap >= Ff)
					Reporting.AddCapacityLine("= " + momentTee.BoltBeamStem.NumberOfBolts + " * " + momentTee.BoltBeamStem.BoltStrength + " = " + Cap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Cap, "Shear Strength", memberType);
				else
					Reporting.AddCapacityLine("= " + momentTee.BoltBeamStem.NumberOfBolts + " * " + momentTee.BoltBeamStem.BoltStrength + " = " + Cap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Cap, "Shear Strength", memberType);

				Reporting.AddGoToHeader("Beam Reduced Section:");
				Reporting.AddHeader("Beam Flange Effective Area:");
				Ag = beam.Shape.tf * beam.Shape.bf;

				Reporting.AddLine("Ag = tf * bf = " + beam.Shape.tf + " * " + beam.Shape.bf + " = " + Ag + ConstUnit.Area);
				An = beam.Shape.tf * (beam.Shape.bf - momentTee.BoltBeamStem.NumberOfLines * (momentTee.BoltBeamStem.HoleWidth + ConstNum.SIXTEENTH_INCH));
				Reporting.AddLine("An = tf * (bf - Nt * (dh + " + ConstNum.SIXTEENTH_INCH + ")) = " + beam.Shape.tf + " * (" + beam.Shape.bf + " - (" + momentTee.BoltBeamStem.NumberOfLines + " * (" + momentTee.BoltBeamStem.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + ")) = " + An + ConstUnit.Area);
				Reporting.AddLine("Ae = 0.75 * Fu * An / (" + ConstString.FIOMEGA0_9 + " * Fy) <= Ag");
				Ae1 = ConstNum.FIOMEGA0_75N * beam.Material.Fu * An / (ConstNum.FIOMEGA0_9N * beam.Material.Fy);
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + beam.Material.Fu + " * " + An + " / (" + ConstString.FIOMEGA0_9 + " * " + beam.Material.Fy + ")");
				if (Ae1 >= Ag)
				{
					Ae = Ag;
					Reporting.AddLine("= " + Ae1 + " >= " + Ag + ";  Ae = " + Ag + ConstUnit.Area);
				}
				else
				{
					Ae = Ae1;
					Reporting.AddLine("= " + Ae1 + ConstUnit.Area);
				}

				Reporting.AddHeader("Effective Plastic Section Modulus (Ze):");
				Reporting.AddLine("= Z - 2 * (Ag - Ae) * d / 2");
				beam.WinConnect.Fema.Ze = beam.Shape.zx - 2 * (Ag - Ae) * beam.Shape.d / 2;
				Reporting.AddLine("= " + beam.Shape.zx + " - 2 * (" + Ag + " - " + Ae + ") * " + beam.Shape.d + " / 2");
				Reporting.AddLine("= " + beam.WinConnect.Fema.Ze + ConstUnit.SecMod);
				Mp = beam.WinConnect.Fema.Ze * beam.Material.Fy;

				Reporting.AddLine(string.Empty);
				Reporting.AddLine("Plastic Moment (Mp) = Ze * Fy = " + beam.WinConnect.Fema.Ze + " * " + beam.Material.Fy + " = " + Mp + ConstUnit.Moment + "");
				if (CommonDataStatic.Units == EUnit.US)
				{
					if (ConstNum.FIOMEGA0_9N * Mp / ConstNum.COEFFICIENT_ONE_THOUSAND >= beam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND)
						Reporting.AddCapacityLine(ConstString.PHI + " Mn = " + ConstString.FIOMEGA0_9 + " * Mp= " + ConstNum.FIOMEGA0_9N * Mp / ConstNum.COEFFICIENT_ONE_THOUSAND + " >= " + (beam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (OK)", beam.Moment / (ConstNum.FIOMEGA0_9N * Mp), "Effective Plastic Section Modulus (Ze)", beam.MemberType);
					else
						Reporting.AddCapacityLine(ConstString.PHI + " Mn = " + ConstString.FIOMEGA0_9 + " * Mp = " + ConstNum.FIOMEGA0_9N * Mp / ConstNum.COEFFICIENT_ONE_THOUSAND + " << " + (beam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (NG)", beam.Moment / (ConstNum.FIOMEGA0_9N * Mp), "Effective Plastic Section Modulus (Ze)", beam.MemberType);
				}
				else
				{
					if (ConstNum.FIOMEGA0_9N * Mp >= beam.Moment)
						Reporting.AddCapacityLine(ConstString.PHI + " Mn = " + ConstString.FIOMEGA0_9 + " * Mp = " + ConstNum.FIOMEGA0_9N * Mp / ConstNum.COEFFICIENT_ONE_THOUSAND + " >= " + (beam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (OK)", beam.Moment / (ConstNum.FIOMEGA0_9N * Mp), "Effective Plastic Section Modulus (Ze)", beam.MemberType);
					else
						Reporting.AddCapacityLine(ConstString.PHI + " Mn = " + ConstString.FIOMEGA0_9 + " * Mp = " + ConstNum.FIOMEGA0_9N * Mp / ConstNum.COEFFICIENT_ONE_THOUSAND + " << " + (beam.Moment / ConstNum.COEFFICIENT_ONE_THOUSAND) + ConstUnit.MomentUnitFoot + " (NG)", beam.Moment / (ConstNum.FIOMEGA0_9N * Mp), "Effective Plastic Section Modulus (Ze)", beam.MemberType);
				}

				if (beam.P != 0)
				{
					Reporting.AddHeader("The effect of the beam axial force is not included in the Beam Bending Strength check.");
					Reporting.AddHeader("The user must check this condition outside the program.");
				}

				Reporting.AddGoToHeader("Bolt Bearing");
				Reporting.AddHeader("Bolt Bearing on Tee Stem:");
				Fbe = CommonCalculations.EdgeBearing(momentTee.BoltBeamStem.EdgeDistLongDir, momentTee.BoltBeamStem.HoleLength, momentTee.BoltBeamStem.BoltSize, momentTee.TopMaterial.Fu, momentTee.BoltBeamStem.HoleType, true);
				Fbs = CommonCalculations.SpacingBearing(momentTee.BoltBeamStem.SpacingLongDir, momentTee.BoltBeamStem.HoleLength, momentTee.BoltBeamStem.BoltSize, momentTee.BoltBeamStem.HoleType, momentTee.TopMaterial.Fu, true);

				BearingCap = momentTee.BoltBeamStem.NumberOfLines * (Fbe + Fbs * (momentTee.BoltBeamStem.NumberOfRows - 1)) * momentTee.TopTeeShape.tw;

				Reporting.AddLine(string.Empty);
				Reporting.AddLine("Bearing Capacity = nT * (Fbe + Fbs * (nL - 1)) * t");
				Reporting.AddLine("= " + momentTee.BoltBeamStem.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + momentTee.BoltBeamStem.NumberOfRows + " - 1)) * " + momentTee.TopTeeShape.tw);
				if (BearingCap >= Ff)
					Reporting.AddCapacityLine("= " + BearingCap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / BearingCap, "Bolts", memberType);
				else
					Reporting.AddCapacityLine("= " + BearingCap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / BearingCap, "Bolts", memberType);

				Reporting.AddHeader("Bolt Bearing on Beam Flange:");
				Fbe = CommonCalculations.EdgeBearing(beam.WinConnect.Beam.BoltEdgeDistanceOnFlange, momentTee.BoltBeamStem.HoleLength, momentTee.BoltBeamStem.BoltSize, beam.Material.Fu, momentTee.BoltBeamStem.HoleType, true);

				Fbs = CommonCalculations.SpacingBearing(momentTee.BoltBeamStem.SpacingLongDir, momentTee.BoltBeamStem.HoleLength, momentTee.BoltBeamStem.BoltSize, momentTee.BoltBeamStem.HoleType, beam.Material.Fu, true);
				BearingCap = momentTee.BoltBeamStem.NumberOfLines * (Fbe + Fbs * (momentTee.BoltBeamStemNumberOfBolts - 1)) * beam.Shape.tf;

				Reporting.AddLine(string.Empty);
				Reporting.AddLine("Bearing Capacity = nT * (Fbe + Fbs * (nL - 1)) * t");
				Reporting.AddLine("= " + momentTee.BoltBeamStem.NumberOfLines + " * (" + Fbe + " + " + Fbs + " * (" + momentTee.BoltBeamStemNumberOfBolts + " - 1)) * " + beam.Shape.tf);
				if (BearingCap >= Ff)
					Reporting.AddCapacityLine("= " + BearingCap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / BearingCap, "Bearing Capacity", memberType);
				else
					Reporting.AddCapacityLine("= " + BearingCap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / BearingCap, "Bearing Capacity", memberType);
				if (!CommonDataStatic.IsFema)
				{
					// Local Yielding of Tee Stem
					FiRn = 2 * 1 * Math.Min(2.5 * momentTee.TopTeeShape.kdes + 2.5 * column.Shape.tf, momentTee.TopLengthAtFlange / 2) * momentTee.TopTeeShape.tw * momentTee.TopMaterial.Fy; //  "2.5 * Column.tf" is "N"

					Reporting.AddGoToHeader("Local Web Yielding of Tee:");
					Reporting.AddLine(ConstString.PHI + " Rn = 2 * 1.0 * Min[(2.5 * k + 2.5 * t), L / 2] * tw * Fy");
					Reporting.AddLine("= 2 * 1.0 * Min[(2.5 * " + momentTee.TopTeeShape.kdes + " + 2.5 * " + column.Shape.tf + "), " + momentTee.TopLengthAtFlange + "/2] * " + momentTee.TopTeeShape.tw + " * " + momentTee.TopMaterial.Fy);
					if (FiRn >= Ff)
						Reporting.AddCapacityLine("= " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / FiRn, "Local Web Yielding of Tee", memberType);
					else
						Reporting.AddCapacityLine("= " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / FiRn, "Local Web Yielding of Tee", memberType);

					Reporting.AddGoToHeader(ConstString.DES_OR_ALLOWABLE + " Tension Strength of Tee Stem");
					Tg = momentTee.TopLengthAtStem * momentTee.TopTeeShape.tw * ConstNum.FIOMEGA0_9N * momentTee.TopMaterial.Fy;

					Reporting.AddHeader("Tension Yielding:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * b * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " *" + momentTee.TopMaterial.Fy + " * " + momentTee.TopLengthAtStem + " * " + momentTee.TopTeeShape.tw);
					if (Tg >= Ff)
						Reporting.AddCapacityLine("= " + Tg + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tg, "Tension Yielding", memberType);
					else
						Reporting.AddCapacityLine("= " + Tg + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tg, "Tension Yielding", memberType);

					bn = momentTee.TopLengthAtStem - Math.Max(0.15 * momentTee.TopLengthAtStem, momentTee.BoltBeamStem.NumberOfLines * (momentTee.BoltBeamStem.HoleWidth + ConstNum.SIXTEENTH_INCH));
					Tn = bn * momentTee.TopTeeShape.tw * ConstNum.FIOMEGA0_75N * momentTee.TopMaterial.Fu;

					Reporting.AddHeader("Tension Rupture:");
					Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * Fu * (b - Max(0.15 * b; Nt * (dh + " + ConstNum.SIXTEENTH_INCH + "))) * t");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + momentTee.TopMaterial.Fu + " * " + "(" + momentTee.TopLengthAtStem + " - Max(0.15 * " + momentTee.TopLengthAtStem + " ; " + momentTee.BoltBeamStem.NumberOfLines + "  * (" + momentTee.BoltBeamStem.HoleWidth + " + " + ConstNum.SIXTEENTH_INCH + "))) * " + momentTee.TopTeeShape.tw);
					if (Tn >= Ff)
						Reporting.AddCapacityLine("= " + Tn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tn, "Tension Rupture", memberType);
					else
						Reporting.AddCapacityLine("= " + Tn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tn, "Tension Rupture", memberType);
				}

				Reporting.AddHeader("Block shear rupture of the Stem:");
				if (momentTee.BoltBeamStemNumberOfBolts == 4)
				{
					AgTension = (Math.Min(momentTee.Beam_g, 2 * momentTee.BoltBeamStem.EdgeDistTransvDir) + 2 * momentTee.Beam_g1) * momentTee.TopTeeShape.tw;
					Reporting.AddLine("Agt = (Min(g1, 2 * e) + 2 * g2) * t");
					Reporting.AddLine("= (" + Math.Min(momentTee.Beam_g, 2 * momentTee.BoltBeamStem.EdgeDistTransvDir) + " + 2 * " + momentTee.Beam_g1 + ") * " + momentTee.TopTeeShape.tw);
					Reporting.AddLine("= " + AgTension + ConstUnit.Area);

					Reporting.AddLine(string.Empty);
					AnTension = AgTension - 3 * (momentTee.BoltBeamStem.HoleWidth + ConstNum.SIXTEENTH_INCH) * momentTee.TopTeeShape.tw;
					Reporting.AddHeader("Ant = Agt - (n - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("= " + AgTension + " - 3 * (" + (momentTee.BoltBeamStem.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + momentTee.TopTeeShape.tw);
					Reporting.AddLine("= " + AnTension + ConstUnit.Area);
				}
				else
				{
					AgTension = Math.Min(momentTee.Beam_g, momentTee.TopLengthAtStem - momentTee.Beam_g) * momentTee.TopTeeShape.tw;
					Reporting.AddLine("Agt = Min(g, 2 * e) * t = " + Math.Min(momentTee.Beam_g, momentTee.TopLengthAtStem - momentTee.Beam_g) + " * " + momentTee.TopTeeShape.tw);
					Reporting.AddLine("= " + AgTension + ConstUnit.Area);

					Reporting.AddLine(string.Empty);
					AnTension = AgTension - (momentTee.BoltBeamStem.HoleWidth + ConstNum.SIXTEENTH_INCH) * momentTee.TopTeeShape.tw;
					Reporting.AddLine("Ant = Agt - (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
					Reporting.AddLine("= " + AgTension + " - (" + (momentTee.BoltBeamStem.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + momentTee.TopTeeShape.tw);
					Reporting.AddLine("= " + AnTension + ConstUnit.Area);
				}
				Reporting.AddLine(string.Empty);
				AgShear = 2 * ((momentTee.BoltBeamStem.NumberOfRows - 1) * momentTee.BoltBeamStem.SpacingLongDir + momentTee.BoltBeamStem.EdgeDistLongDir) * momentTee.TopTeeShape.tw;
				Reporting.AddLine("Agv = 2 * ((nl - 1) * s + Le) * t");
				Reporting.AddLine("= 2 * ((" + momentTee.BoltBeamStem.NumberOfRows + " - 1) * " + momentTee.BoltBeamStem.SpacingLongDir + " + " + momentTee.BoltBeamStem.EdgeDistLongDir + ") * " + momentTee.TopTeeShape.tw);
				Reporting.AddLine("= " + AgShear + ConstUnit.Area);

				Reporting.AddLine(string.Empty);
				AnShear = AgShear - 2 * (momentTee.BoltBeamStem.NumberOfRows - 0.5) * (momentTee.BoltBeamStem.HoleLength + ConstNum.SIXTEENTH_INCH) * momentTee.TopTeeShape.tw;
				Reporting.AddLine("Anv = Agv - 2 * (nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("=" + AgShear + " - 2 * (" + momentTee.BoltBeamStem.NumberOfRows + " - 0.5) * (" + (momentTee.BoltBeamStem.HoleLength + ConstNum.SIXTEENTH_INCH) + ")*" + momentTee.TopTeeShape.tw);
				Reporting.AddLine("= " + AnShear + ConstUnit.Area);

				Reporting.AddLine(string.Empty);
				MiscCalculationsWithReporting.BlockShearNew(momentTee.TopMaterial.Fu, AnShear, 1, AnTension, AgShear, momentTee.TopMaterial.Fy, 0, Ff, true);

				Reporting.AddHeader("Block shear rupture of the Beam Flange:");

				AgTension = (beam.Shape.bf - momentTee.Beam_g) * beam.Shape.tf;
				Reporting.AddLine("Agt = (bf - g) * t = (" + beam.Shape.bf + " - " + momentTee.Beam_g + ") *  " + beam.Shape.tf);
				Reporting.AddLine("= " + AgTension + ConstUnit.Area);

				Reporting.AddLine(string.Empty);
				AnTension = AgTension - (momentTee.BoltBeamStem.NumberOfLines - 1) * (momentTee.BoltBeamStem.HoleWidth + ConstNum.SIXTEENTH_INCH) * beam.Shape.tf;
				Reporting.AddLine("Ant = Agt - (nt - 1) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("= " + AgTension + " - (" + momentTee.BoltBeamStem.NumberOfLines + " - 1) * (" + (momentTee.BoltBeamStem.HoleWidth + ConstNum.SIXTEENTH_INCH) + ") * " + beam.Shape.tf);
				Reporting.AddLine("= " + AnTension + ConstUnit.Area);

				Reporting.AddLine(string.Empty);
				AgShear = 2 * ((momentTee.BoltBeamStem.NumberOfRows - 1) * momentTee.BoltBeamStem.SpacingLongDir + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange) * beam.Shape.tf;
				Reporting.AddLine("Agv = 2 * ((nl - 1) * s + ef) * t");
				Reporting.AddLine("= 2 * ((" + momentTee.BoltBeamStem.NumberOfRows + " - 1) * " + momentTee.BoltBeamStem.SpacingLongDir + " + " + momentTee.BoltBeamStem.EdgeDistTransvDir + ") * " + beam.Shape.tf);
				Reporting.AddLine("= " + AgShear + ConstUnit.Area);

				Reporting.AddLine(string.Empty);
				AnShear = AgShear - 2 * (momentTee.BoltBeamStem.NumberOfRows - 0.5) * (momentTee.BoltBeamStem.HoleLength + ConstNum.SIXTEENTH_INCH) * beam.Shape.tf;
				Reporting.AddLine("Anv = Agv - 2 * (nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * t");
				Reporting.AddLine("=" + AgShear + " - 2 * (" + momentTee.BoltBeamStem.NumberOfRows + " - 0.5) * (" + (momentTee.BoltBeamStem.HoleLength + ConstNum.SIXTEENTH_INCH) + ")*" + beam.Shape.tf);
				Reporting.AddLine("= " + AnShear + ConstUnit.Area);

				Reporting.AddLine(string.Empty);
				MiscCalculationsWithReporting.BlockShearNew(beam.Material.Fu, AnShear, 1, AnTension, AgShear, beam.Material.Fy, 0, Ff, true);
				if (CommonDataStatic.IsFema)
					return;

				Reporting.AddHeader("Bottom Tee Stem " + ConstString.DES_OR_ALLOWABLE + " Compressive Strength:");

				k = 0.65;
				beam.WinConnect.Fema.L = (beam.EndSetback + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange - momentTee.TopTeeShape.tf);
				t = momentTee.BottomTeeShape.tw;
				rg = t / Math.Sqrt(12);
				kL_r = (k * beam.WinConnect.Fema.L / rg);

				Fcr = CommonCalculations.BucklingStress(kL_r, momentTee.TopMaterial.Fy, true);

				Reporting.AddLine("Unbraced Length (L) = c + ef - tf = " + beam.EndSetback + " + " + beam.WinConnect.Beam.BoltEdgeDistanceOnFlange + " - " + momentTee.TopTeeShape.tf + " = " + beam.WinConnect.Fema.L + ConstUnit.Length);
				Reporting.AddLine("Effective Length Factor (K) = 0.65");
				kL_r = (beam.WinConnect.Fema.L * k / (momentTee.TopTeeShape.tw / Math.Sqrt(12)));

				Reporting.AddLine(string.Empty);
				Reporting.AddLine("KL / r = k * L / (t / 3.464) = " + k + " * " + beam.WinConnect.Fema.L + " / (" + momentTee.TopTeeShape.tw + " / 3.464) = " + kL_r);

				Fcr = CommonCalculations.BucklingStress(kL_r, momentTee.TopMaterial.Fy, true);

				FiRn = ConstNum.FIOMEGA0_9N * Fcr * momentTee.TopLengthAtFlange * momentTee.TopTeeShape.tw;
				if (FiRn >= Ff)
					Reporting.AddCapacityLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + momentTee.TopLengthAtFlange + " * " + momentTee.TopTeeShape.tw + " = " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / FiRn, "Bolts", memberType);
				else
					Reporting.AddCapacityLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + momentTee.TopLengthAtFlange + " * " + momentTee.TopTeeShape.tw + " = " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / FiRn, "Bolts", memberType);
			}
			else
			{
				Tweldlength = 2 * momentTee.TopTeeShape.d + momentTee.TopLengthAtStem - Math.Max(2 * beam.EndSetback, 1.732 * Math.Abs(momentTee.TopLengthAtFlange - momentTee.TopLengthAtStem));
				// Bottom Flange Weld
				TMinW = CommonCalculations.MinimumWeld(momentTee.TopTeeShape.tw, beam.Shape.tf);
				if (momentTee.TopTeeShape.tw >= ConstNum.QUARTER_INCH)
					Tmaxw = momentTee.TopTeeShape.tw - ConstNum.SIXTEENTH_INCH;
				else
					Tmaxw = momentTee.TopTeeShape.tw;

				Reporting.AddHeader("Tee Stem to Beam Weld");
				Reporting.AddHeader("Tee Stem to Beam Weld (Top and Bottom):");
				if (momentTee.BottomWeldSize >= TMinW)
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentTee.BottomWeldSize) + " >= Minimum Weld Size = " + CommonCalculations.WeldSize(TMinW) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentTee.BottomWeldSize) + " << Minimum Weld Size = " + CommonCalculations.WeldSize(TMinW) + ConstUnit.Length + " (NG)");

				if (momentTee.BottomWeldSize <= Tmaxw)
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentTee.BottomWeldSize) + " <= Maximum Weld Size = " + CommonCalculations.WeldSize(Tmaxw) + ConstUnit.Length + " (OK)");
				else
					Reporting.AddLine("Weld Size = " + CommonCalculations.WeldSize(momentTee.BottomWeldSize) + " >> Maximum Weld Size = " + CommonCalculations.WeldSize(Tmaxw) + ConstUnit.Length + " (NG)");

				Cap = ConstNum.FIOMEGA0_75N * 0.4242 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * momentTee.BottomWeldSize * Tweldlength;
				Reporting.AddLine("Strength = " + ConstString.FIOMEGA0_75 + " * 0.4242 " + " * Fexx * w * L");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.4242 " + " * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * " + momentTee.BottomWeldSize + " * " + Tweldlength);
				if (Cap >= Ff)
					Reporting.AddCapacityLine("= " + Cap + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Cap, "Tee Stem to Beam Weld", memberType);
				else
					Reporting.AddCapacityLine("= " + Cap + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Cap, "Tee Stem to Beam Weld", memberType);

				Reporting.AddHeader("Tee Stem Tension " + ConstString.DES_OR_ALLOWABLE_STRENGTH + ":");
				Tg = momentTee.TopLengthAtStem * momentTee.TopTeeShape.tw * ConstNum.FIOMEGA0_9N * momentTee.TopMaterial.Fy;
				Reporting.AddHeader("Tension Yielding:");
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_9 + " * Fy * b * t");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_9 + " * " + momentTee.TopMaterial.Fy + " * " + momentTee.TopLengthAtStem + " * " + momentTee.TopTeeShape.tw);
				if (Tg >= Ff)
					Reporting.AddCapacityLine("= " + Tg + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tg, "Tension Yielding", memberType);
				else
					Reporting.AddCapacityLine("= " + Tg + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tg, "Tension Yielding", memberType);

				bn = momentTee.TopLengthAtStem;
				Tn = 0.85 * bn * momentTee.TopTeeShape.tw * ConstNum.FIOMEGA0_75N * momentTee.TopMaterial.Fu;
				Reporting.AddHeader("Tension Rupture:");
				Reporting.AddLine(ConstString.PHI + " Rn = " + ConstString.FIOMEGA0_75 + " * 0.85 * Fu * b * t");
				Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + momentTee.TopMaterial.Fu + " * " + momentTee.TopLengthAtStem + " * " + momentTee.TopTeeShape.tw);
				if (Tn >= Ff)
					Reporting.AddCapacityLine("= " + Tn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / Tn, "Tension Rupture", memberType);
				else
					Reporting.AddCapacityLine("= " + Tn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / Tn, "Tension Rupture", memberType);

				Reporting.AddLine(ConstString.DES_OR_ALLOWABLE + " Compressive Strength of Tee Stem:");

				k = 0.65;
				beam.WinConnect.Fema.L = (Math.Max(beam.EndSetback, 1.732F * Math.Abs(momentTee.TopLengthAtFlange - momentTee.TopLengthAtStem) / 2) - momentTee.TopTeeShape.tf);
				rg = t / Math.Sqrt(12);
				kL_r = k * beam.WinConnect.Fema.L / rg;
				Reporting.AddLine("Unbraced Length (L) = " + beam.WinConnect.Fema.L + ConstUnit.Length + " , Effective Length Factor, K = 0.65");

				kL_r = (beam.WinConnect.Fema.L * k / (momentTee.TopTeeShape.tw / Math.Sqrt(12)));

				Reporting.AddLine("KL / r = k * L / (t / 3.464) = " + k + " * " + beam.WinConnect.Fema.L + " / (" + momentTee.TopTeeShape.tw + "/3.464) = " + kL_r);

				Fcr = CommonCalculations.BucklingStress(kL_r, momentTee.TopMaterial.Fy, true);

				FiRn = ConstNum.FIOMEGA0_9N * Fcr * momentTee.TopLengthAtStem * momentTee.TopTeeShape.tw;
				if (FiRn >= Ff)
					Reporting.AddCapacityLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + momentTee.TopLengthAtStem + " * " + momentTee.TopTeeShape.tw + " = " + FiRn + " >= " + Ff + ConstUnit.Force + " (OK)", Ff / FiRn, "Unbraced Length", memberType);
				else
					Reporting.AddCapacityLine(ConstString.PHI + "cPn = " + ConstString.FIOMEGA0_9 + " * Fcr * Ag = " + ConstString.FIOMEGA0_9 + " * " + Fcr + " * " + momentTee.TopLengthAtStem + " * " + momentTee.TopTeeShape.tw + " = " + FiRn + " << " + Ff + ConstUnit.Force + " (NG)", Ff / FiRn, "Unbraced Length", memberType);
			}

			if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToHSSColumn)
			{
				double qf;

				Reporting.AddHeader("Concentrated Forces on HSS");

				qf = MiscCalculationsWithReporting.HSSSideWallCripling(beam.MemberType, Ff, 2 * momentTee.TopTeeShape.kdes);
				MiscCalculationsWithReporting.HSSSideWallYielding(Ff, 0, momentTee.TopTeeShape.tw, momentTee.TopTeeShape.tf, momentTee.TopTeeShape.bf);
				MiscCalculationsWithReporting.HSSSideWallBuckling(Ff, qf);
			}
		}

		private static void GetColumnGageforTee(EMemberType memberType)
		{
			double gage1;
			double gage2;
			double gageMin;
			double gageMax;
			double rolledEdge;
			double outGage;

			var momentTee = CommonDataStatic.DetailDataDict[memberType].WinConnect.MomentTee;
			var column = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

			if (!momentTee.Column_g_User)
				momentTee.Column_g = column.GageOnFlange;

			gage1 = 2 * (column.Shape.k1 + momentTee.BoltColumnFlange.BoltSize);
			gage2 = column.Shape.tw + 2 * (momentTee.BoltColumnFlange.BoltSize + ConstNum.HALF_INCH);
			gageMin = Math.Max(gage2, gage1);
			rolledEdge = momentTee.BoltColumnFlange.MinEdgeRolled;
			gageMax = column.Shape.bf - 2 * rolledEdge;

			if (momentTee.Column_g < gageMin)
				momentTee.Column_g = NumberFun.Round(gageMin, 2);

			outGage = (gageMax - momentTee.Column_g) / 2;

			if (outGage >= 2.67 * momentTee.BoltColumnFlange.BoltSize)
				momentTee.Column_g1 = outGage;
			else
			{
				momentTee.Column_g1 = 0;
				momentTee.BoltColumnFlangeNumberOfBolts = 4;
			}
		}

		private static void GetBeamGageforTee(EMemberType memberType)
		{
			double gage1;
			double gage2;
			double gageMin;
			double gageMax;
			double rolledEdge;
			double outGage;

			var beam = CommonDataStatic.DetailDataDict[memberType];
			var momentTee = CommonDataStatic.DetailDataDict[memberType].WinConnect.MomentTee;

			if (!momentTee.Beam_g_User)
				momentTee.Beam_g = beam.GageOnFlange;

			gage1 = 2 * (beam.Shape.k1 + momentTee.BoltBeamStem.BoltSize);
			gage2 = beam.Shape.tw + 2 * (momentTee.BoltBeamStem.BoltSize + ConstNum.HALF_INCH);
			gageMin = Math.Max(gage2, gage1);
			rolledEdge = momentTee.BoltBeamStem.MinEdgeRolled;
			gageMax = beam.Shape.bf - 2 * rolledEdge;

			if (momentTee.Beam_g < gageMin)
				momentTee.Beam_g = gageMin;

			outGage = (gageMax - momentTee.Beam_g) / 2;

			if (outGage >= 2.67 * momentTee.BoltBeamStem.BoltSize)
				momentTee.Beam_g1 = outGage;
			else
			{
				momentTee.Beam_g1 = 0;
				momentTee.BoltBeamStem.NumberOfLines = 2;
			}
		}
	}
}