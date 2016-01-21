using System;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Calculations
{
	public class BraceToGussetConnection
	{
		public static void DesignBraceToGusset(EMemberType memberType)
		{
			bool NetTensionOk = false;
			bool WebTearOutOk = false;
			bool BearingonWebOK = false;
			bool FlangeTearOutOk = false;
			bool BearingonFlangeOK = false;
			bool calcBraceCheckPassed = true;
			double vf = 0;
			double vw = 0;
			double boltnwmax = 0;
			double plwidth = 0;
			double Aw = 0;
			double a = 0;
			double Af = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];

			if (!component.IsActive)
				return;

			if (component.EndSetback >= 0 && !(component.ShapeType == EShapeType.HollowSteelSection && component.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted))
			{
				Af = component.Shape.tf * component.Shape.bf;
				a = component.Shape.a;
				Aw = a - 2 * Af;
				if (!(component.BraceConnect.ClawAngles.DoNotConnectFlanges & component.BraceConnect.SplicePlates.DoNotConnectWeb))
				{
					component.BraceConnect.Brace.FlangeForceTension = Af / a * component.AxialTension;
					component.BraceConnect.Brace.FlangeForceCompression = Af / a * component.AxialCompression;
					component.BraceConnect.Brace.WebForceTension = Aw / a * component.AxialTension;
					component.BraceConnect.Brace.WebForceCompression = Aw / a * component.AxialCompression;
					component.PFlange = Math.Max(component.BraceConnect.Brace.FlangeForceTension, component.BraceConnect.Brace.FlangeForceCompression);
					component.PWeb = Math.Max(component.BraceConnect.Brace.WebForceTension, component.BraceConnect.Brace.WebForceCompression);
				}
				else if (!component.BraceConnect.ClawAngles.DoNotConnectFlanges)
				{
					component.BraceConnect.Brace.FlangeForceTension = component.AxialTension / 2;
					component.BraceConnect.Brace.FlangeForceCompression = component.AxialCompression / 2;
					component.BraceConnect.Brace.WebForceTension = 0;
					component.BraceConnect.Brace.WebForceCompression = 0;
					component.PFlange = Math.Max(component.BraceConnect.Brace.FlangeForceTension, component.BraceConnect.Brace.FlangeForceCompression);
					component.PWeb = 0;
				}
				else if (!component.BraceConnect.SplicePlates.DoNotConnectWeb)
				{
					component.BraceConnect.Brace.FlangeForceTension = 0;
					component.BraceConnect.Brace.FlangeForceCompression = 0;
					component.BraceConnect.Brace.WebForceTension = component.AxialTension;
					component.BraceConnect.Brace.WebForceCompression = component.AxialCompression;
					component.PFlange = 0;
					component.PWeb = Math.Max(component.BraceConnect.Brace.WebForceTension, component.BraceConnect.Brace.WebForceCompression);
				}

				if (!component.BraceConnect.SplicePlates.DoNotConnectWeb)
				{
					plwidth = component.Shape.t;
					boltnwmax = (int)Math.Ceiling((plwidth - 2 * component.BraceConnect.SplicePlates.Bolt.EdgeDistTransvDir) / component.BraceConnect.SplicePlates.Bolt.SpacingTransvDir) + 1;
					vw = component.BraceConnect.SplicePlates.Bolt.BoltStrength;
					if (boltnwmax <= 1)
						component.BraceConnect.SplicePlates.Bolt.NumberOfLines = 1;
					else
						component.BraceConnect.SplicePlates.Bolt.NumberOfLines = 2;
					if (component.BraceConnect.SplicePlates.Bolt.NumberOfLines == 1)
						component.BraceConnect.SplicePlates.Bolt.NumberOfBolts = 2;
					else
						component.BraceConnect.SplicePlates.Bolt.NumberOfBolts = 4;
					component.BraceConnect.SplicePlates.Bolt.NumberOfRows = component.BraceConnect.SplicePlates.Bolt.NumberOfBolts / component.BraceConnect.SplicePlates.Bolt.NumberOfLines;
				}
				if (!component.BraceConnect.ClawAngles.DoNotConnectFlanges)
				{
					vf = component.BoltGusset.BoltStrength;
					component.BoltGusset.NumberOfBolts = Math.Max(2, (int)Math.Ceiling(Math.Abs(component.PFlange) / (2 * vf)));
				}
				if (component.BraceConnect.ClawAngles.DoNotConnectFlanges)
				{
					BearingonFlangeOK = true;
					FlangeTearOutOk = true;
				}
				if (component.BraceConnect.SplicePlates.DoNotConnectWeb)
				{
					BearingonWebOK = true;
					WebTearOutOk = true;
				}

				if (component.ShapeType != EShapeType.HollowSteelSection)
				{
					do
					{
						calcBraceCheckPassed = BraceCheck.CalcBraceCheck(component.MemberType, ref BearingonFlangeOK, ref BearingonWebOK, ref FlangeTearOutOk, ref WebTearOutOk, ref NetTensionOk, false);
						if (!(BearingonFlangeOK && FlangeTearOutOk))
						{
							int numberOfBolts = ++CommonDataStatic.DetailDataDict[EMemberType.UpperRight].BoltBrace.NumberOfBolts;
							CommonDataStatic.DetailDataDict[EMemberType.UpperLeft].BoltBrace.NumberOfBolts = numberOfBolts;
							CommonDataStatic.DetailDataDict[EMemberType.LowerRight].BoltBrace.NumberOfBolts = numberOfBolts;
							CommonDataStatic.DetailDataDict[EMemberType.LowerLeft].BoltBrace.NumberOfBolts = numberOfBolts;
						}
						if (!(BearingonWebOK && WebTearOutOk))
						{
							if (component.BraceConnect.SplicePlates.Bolt.NumberOfRows < 3 || component.BraceConnect.SplicePlates.Bolt.NumberOfLines == boltnwmax)
							{
								component.BraceConnect.SplicePlates.Bolt.NumberOfRows++;
								component.BraceConnect.SplicePlates.Bolt.NumberOfBolts = component.BraceConnect.SplicePlates.Bolt.NumberOfRows * component.BraceConnect.SplicePlates.Bolt.NumberOfLines;
							}
							else
							{
								if (component.BraceConnect.SplicePlates.Bolt.NumberOfLines < boltnwmax)
								{
									component.BraceConnect.SplicePlates.Bolt.NumberOfLines++;
									component.BraceConnect.SplicePlates.Bolt.NumberOfBolts = component.BraceConnect.SplicePlates.Bolt.NumberOfRows * component.BraceConnect.SplicePlates.Bolt.NumberOfLines;
								}
								else
								{
									component.BraceConnect.SplicePlates.Bolt.NumberOfRows++;
									component.BraceConnect.SplicePlates.Bolt.NumberOfBolts = component.BraceConnect.SplicePlates.Bolt.NumberOfRows * component.BraceConnect.SplicePlates.Bolt.NumberOfLines;
								}
							}
						}
					} while (!calcBraceCheckPassed);
				}

                Reporting.AddHeader(CommonDataStatic.CommonLists.CompleteMemberList[component.MemberType] + " to Gusset Connection");

				if (component.BraceConnect.ClawAngles.SizeName != "None" && (!component.BraceConnect.ClawAngles.DoNotConnectFlanges && !component.BraceConnect.SplicePlates.DoNotConnectWeb))
				{
					Reporting.AddLine("Flange Area = Af ");
					Reporting.AddLine("= tf * bf");
					Reporting.AddLine("= " + component.Shape.tf + " * " + component.Shape.bf + " = " + Af + ConstUnit.Area);

					Reporting.AddLine("Web Area = Aw ");
					Reporting.AddLine("= A - 2 * Af");
					Reporting.AddLine("= " + a + " - 2 * " + Af + " = " + Aw + ConstUnit.Area);

					Reporting.AddLine("Flange Force (Each) Tension = Pft ");
					Reporting.AddLine("= Af * Fx_T / A");
					Reporting.AddLine("= " + Af + " * " + component.AxialTension + " / " + a + " = " + component.BraceConnect.Brace.FlangeForceTension + ConstUnit.Force);

					Reporting.AddLine("Flange Force (Each) Compression = Pfc ");
					Reporting.AddLine("= Af * Fx_C / A");
					Reporting.AddLine("= " + Af + " * " + component.AxialCompression + " / " + a + " = " + component.BraceConnect.Brace.FlangeForceCompression + ConstUnit.Force);

					Reporting.AddLine("Web Force Tension = Pwt ");
					Reporting.AddLine("= Aw * Fx_T / A");
					Reporting.AddLine("= " + Aw + " * " + component.AxialTension + " / " + a + "= " + component.BraceConnect.Brace.WebForceTension + ConstUnit.Force);

					Reporting.AddLine("Web Force Compression = Pwc ");
					Reporting.AddLine("= Aw * Fx_C / A");
					Reporting.AddLine("= " + Aw + " * " + component.AxialCompression + " / " + a + " = " + component.BraceConnect.Brace.WebForceCompression + ConstUnit.Force);

					Reporting.AddLine("Number of Bolts on Flange (on Each Angle) = Nbf");
					Reporting.AddLine("= Max(Pft, Pfc) / (2 * Fv)");
					Reporting.AddLine("= " + Math.Max(component.BraceConnect.Brace.FlangeForceTension, component.BraceConnect.Brace.FlangeForceCompression) + " /(2 * " + vf + ")");

					if (component.PFlange / 2 / vf <= component.BoltGusset.NumberOfBolts)
						Reporting.AddLine("= " + (component.PFlange / 2 / vf) + ", Use " + component.BoltGusset.NumberOfBolts + " Bolts (OK)");
					else
						Reporting.AddLine("= " + (component.PFlange / 2 / vf) + ", Use " + component.BoltGusset.NumberOfBolts + " Bolts (NG)");

					Reporting.AddHeader("Number of Bolts on Web = Nbw");
					Reporting.AddLine("= Max(Pwt, Pwc) / (2 * Fv)");
					Reporting.AddLine("= " + component.PWeb + " / (2 * " + vw + ")");

					if (component.PWeb / 2 / vw <= component.BraceConnect.SplicePlates.Bolt.NumberOfBolts)
						Reporting.AddLine("= " + (component.PWeb / 2 / vw) + ", Use " + component.BraceConnect.SplicePlates.Bolt.NumberOfBolts + " Bolts in " + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " Lines (OK)");
					else
						Reporting.AddLine("= " + (component.PWeb / 2 / vw) + ", Use " + component.BraceConnect.SplicePlates.Bolt.NumberOfBolts + " Bolts in " + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " Lines (NG)");
				}
				else if (!component.BraceConnect.ClawAngles.DoNotConnectFlanges)
				{

					Reporting.AddHeader("Force carried by each flange connection (Pf)");
					Reporting.AddLine("= Fx / 2 = " + component.BraceConnect.Brace.MaxForce + " / 2 = " + component.PFlange + ConstUnit.Force);
					Reporting.AddLine("Number of Bolts on Flange (on Each Angle) = Nbf");
					Reporting.AddLine("= Pf / (2 * Fv)" + " = " + component.PFlange + " / (2 * " + vf + ")");
					if (component.PFlange / 2 / vf <= component.BoltGusset.NumberOfBolts)
						Reporting.AddLine("= " + (component.PFlange / 2 / vf) + ", Use " + component.BoltGusset.NumberOfBolts + " Bolts (OK)");
					else
						Reporting.AddLine("= " + (component.PFlange / 2 / vf) + ", Use " + component.BoltGusset.NumberOfBolts + " Bolts (NG)");
				}
				else
				{
					Reporting.AddHeader("Force carried by web connection:");
					Reporting.AddLine("    Pw = " + component.PWeb + ConstUnit.Force);
					Reporting.AddLine("Number of Bolts on Web = Nbw");
					Reporting.AddLine("= Pw / (2 * Fv)" + " = " + component.PWeb + " / (2 * " + vw + ")");
					if (component.PWeb / 2 / vw <= component.BraceConnect.SplicePlates.Bolt.NumberOfBolts)
						Reporting.AddLine("= " + (component.PWeb / 2 / vw) + ", Use " + component.BraceConnect.SplicePlates.Bolt.NumberOfBolts + " Bolts in " + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " Lines (OK)");
					else
						Reporting.AddLine("= " + (component.PWeb / 2 / vw) + ", Use " + component.BraceConnect.SplicePlates.Bolt.NumberOfBolts + " Bolts in " + component.BraceConnect.SplicePlates.Bolt.NumberOfLines + " Lines (NG)");
				}
				if (!component.BraceConnect.ClawAngles.DoNotConnectFlanges)
					DesignClawAngles.CalcDesignClawAngles(component.MemberType);
				if (!component.BraceConnect.SplicePlates.DoNotConnectWeb)
					DesignSplicePlate.CalcDesignSplicePlate(component.MemberType);
				BraceCheck.CalcBraceCheck(component.MemberType, ref BearingonFlangeOK, ref BearingonWebOK, ref FlangeTearOutOk, ref WebTearOutOk, ref NetTensionOk, true);
			}
			else
			{
				if (!component.BraceConnect.Gusset.EdgeDistance_User)
					component.BraceConnect.Gusset.EdgeDistance = component.BoltBrace.EdgeDistGusset;
				switch (component.ShapeType)
				{
					case EShapeType.WTSection:
						DesignWTBraceToGusset.CalcDesignWTBraceToGusset(memberType);
						break;
					case EShapeType.SingleAngle:
						DesignSingleAngleToGusset.CalcDesignSingleAngletoGusset(memberType);
						break;
					case EShapeType.DoubleAngle:
						DesignDoubleAngletoGusset(memberType);
						break;
					case EShapeType.HollowSteelSection:
						DesignHSSbracetoGusset.CalcDesignHSSBraceToGusset(memberType);
						break;
					case EShapeType.SingleChannel:
					case EShapeType.DoubleChannel:
						DesignChannelToGusset.CalcDesignChannelToGusset(memberType);
						break;
				}
			}
		}

		private static void DesignDoubleAngletoGusset(EMemberType memberType)
		{
			double tmpCaseArg;
			double Bracedevelopes;
			double Gussetdevelopes;
			double weldCapacity;
			double Ant2;
			double Ant1;
			double Anv;
			double Ant;
			double Agv;
			double Agt;
			double SpacingbearingCapComp;
			double SpacingbearingCap;
			double edgebearingCap;
			double FbreStag = 0;
			double edgdist;
			double smin1;
			double sminG;
			double sminB;
			double spacing;
			double edgemin;
			double Smin;
			double SminGusset;
			double SminBrace;
			double excessforce;
			double AgStiffener = 0;
			double AgAdditionalforRupture;
			double An_effective;
			double excessForceR;
			double An2;
			double An1;
			double An = 0;
			double U = 0;
			double L = 0;
			double x_;
			double L2;
			double L1;
			double excessForceY;
			double AgAdditionalforYielding;
			double AgrossYielding;
			double FiRn;
			bool loopAgain;
			double capacity;
			double edgeWeldCapacity;
			double betaN2;
			double backWeldCapacity;
			double betaN1;
			double maxweldsize;
			double minweldsize;
			double BackWeldArea;
			double EdgeWeldArea;
			double pback;
			double Pedge;
			int N_RequiredForBlockShear;
			double N_RequiredBComp;
			double N_RequiredB;
			double Fbrs;
			double Fbre;
			double np;
			int N_RequiredS;
			double spmin;
			double g2;
			double g1;
			double g0;
			double AngB;
			double AngX;
			double thick;
			double LongLeg;
			double shortleg;
			double cx;
			double ar;
			double t;
			double b;
			double d;
			string r_y_String = "";
			string r_t_String = "";
			string Yildiz = "";
			double r_y_Strength = 0;
			double r_t_Strength = 0;
			double r_y = 0;
			double r_t = 0;

			var component = CommonDataStatic.DetailDataDict[memberType];

			if (!component.IsActive)
				return;

			SmallMethodsDesign.ExpectedStrengthCoefficient(memberType, ref r_t, ref r_y, ref r_t_Strength, ref r_y_Strength, ref Yildiz, ref r_t_String, ref r_y_String);

			if (CommonDataStatic.Preferences.Seismic == ESeismic.Seismic)
			{
				r_t = component.Material.Rt;
				r_y = component.Material.Ry;
			}
			else
			{
				r_t = 1;
				r_y = 1;
			}

			component.BraceConnect.ClawAngles.DoNotConnectFlanges = true;
			component.BraceConnect.SplicePlates.DoNotConnectWeb = true;

			d = component.Shape.d;
			b = component.Shape.bf;
			t = component.Shape.tf;
			ar = (b + d - t) * t;
			component.Shape.a = 2 * ar;
			cx = (Math.Pow(d, 2) * t / 2 + (b - t) * Math.Pow(t, 2) / 2) / ar;

			shortleg = Math.Min(d, b);
			LongLeg = Math.Max(d, b); 
			thick = t;

			if (component.BraceToGussetWeldedOrBolted == EConnectionStyle.Welded)
			{
				if (component.WebOrientation == EWebOrientation.OutOfPlane)
				{
					AngX = (Math.Pow(shortleg, 2) / 2 * thick + (LongLeg - thick) * Math.Pow(thick, 2) / 2) / ((LongLeg + shortleg - thick) * thick);
					AngB = shortleg - AngX;
				}
				else
				{
					AngX = (Math.Pow(LongLeg, 2) / 2 * thick + (shortleg - thick) * Math.Pow(thick, 2) / 2) / ((shortleg + LongLeg - thick) * thick);
					AngB = LongLeg - AngX;
				}
			}
			else
			{
				g0 = CommonCalculations.AngleStandardGage(0, d);
				g1 = CommonCalculations.AngleStandardGage(1, d);
				g2 = CommonCalculations.AngleStandardGage(2, d);

				if (component.WebOrientation == EWebOrientation.OutOfPlane)
				{
					AngX = Math.Min(ConstNum.THREE_INCHES, CommonCalculations.AngleStandardGage(component.BoltBrace.NumberOfRows - 1, shortleg)); // AngX = Math.Min(BRACE1.UcinchN, SmallMethodsBrace.AngleStandardGage(brace.BraceBolt.NumRowsOfBolts - 1, shortleg));
					AngB = shortleg - AngX;
				}
				else
				{
					AngX = Math.Min(ConstNum.THREE_INCHES, CommonCalculations.AngleStandardGage(component.BoltBrace.NumberOfRows - 1, LongLeg)); // AngX = Math.Min(BRACE1.UcinchN, SmallMethodsBrace.AngleStandardGage(brace.BraceBolt.NumRowsOfBolts - 1, LongLeg));
					AngB = LongLeg - AngX;
				}
			}
			if (component.OSLOnBeamSide)
			{
				component.AngleBeamSide = AngX; 
				component.AngleColumnSide = AngB;
			}
			else
			{
				component.AngleBeamSide = AngB;
				component.AngleColumnSide = AngX;
			}

			g0 = CommonCalculations.AngleStandardGage(0, d);
			g1 = AngX;

			if (component.BoltBrace.NumberOfRows == 1)
				g2 = 0;
			else
				g2 = CommonCalculations.AngleStandardGage(2, d);
			if (component.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
			{
				if (component.BoltBrace.NumberOfBolts != 0)
				{
					if (component.BoltBrace.NumberOfBolts > 3 && d > ConstNum.FOUR_INCHES)
					{
						if (component.BoltBrace.NumberOfRows == 0)
							component.BoltBrace.NumberOfRows = 2;
						component.BoltBrace.EdgeDistTransvDir = d - g1 - g2;
						if (g2 < 2.667 * component.BoltBrace.BoltSize && component.BoltBrace.NumberOfRows == 2)
						{
							component.BraceConnect.Brace.BoltsAreStaggered = true;
							spmin = 2 * Math.Sqrt(Math.Pow(2.667 * component.BoltBrace.BoltSize, 2) - Math.Pow(g2, 2));
							if (component.BoltBrace.SpacingLongDir < spmin)
								component.BoltBrace.SpacingLongDir = NumberFun.Round(spmin, 2);
						}
						else
							component.BraceConnect.Brace.BoltsAreStaggered = false;
						if (!component.EndSetback_User && component.BoltBrace.NumberOfRows == 1)
							component.EndSetback = -(component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir * (component.BoltBrace.NumberOfBolts - 1) + component.BraceConnect.Gusset.EdgeDistance);
						else if (!component.EndSetback_User && component.BoltBrace.NumberOfBolts % 2 == 0 && !component.BraceConnect.Brace.BoltsAreStaggered) // else if (brace.Bolt.NumberOfBolts % 2 == 0 && !brace.BraceConnect.Brace.BoltsAreStaggered)
							component.EndSetback = -(component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir * (component.BoltBrace.NumberOfBolts / 2 - 1) + component.BraceConnect.Gusset.EdgeDistance);
						else if(!component.EndSetback_User)
							component.EndSetback = -(component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir * ((component.BoltBrace.NumberOfBolts + 1) / 2 - 1) + component.BraceConnect.Gusset.EdgeDistance);
					}
					else
					{
						if (component.BoltBrace.NumberOfRows == 0)
							component.BoltBrace.NumberOfRows = 1;
						component.BraceConnect.Brace.BoltsAreStaggered = false;
						component.BoltBrace.EdgeDistTransvDir = d - g1;
						if (component.BoltBrace.NumberOfBolts < 2)
							component.BoltBrace.NumberOfBolts = 2;
						if (!component.EndSetback_User)
							component.EndSetback = -(component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir
							                         * (component.BoltBrace.NumberOfBolts - 1) + component.BraceConnect.Gusset.EdgeDistance);
					}
				}
				else
				{
					N_RequiredS = ((int) (component.BraceConnect.Brace.MaxForce / (2 * component.BoltBrace.BoltStrength)));
					if (component.AxialTension > 0)
						np = SmallMethodsDesign.NBRequiredForGussetBlockShear(memberType, 0, N_RequiredS);
					else
						np = 0;
					if (N_RequiredS < np)
						N_RequiredS = ((int) np);
					if (N_RequiredS > 3 && d >= 5 * ConstNum.ONE_INCH && (component.BoltBrace.NumberOfRows == 0 || component.BoltBrace.NumberOfRows == 2)) // if (N_RequiredS > 3 && d >= 5 * ConstNum.ONE_INCH && (!BRACE1.UserSelectedBraceLongBoltLines[m] || brace.BraceBolt.NumRowsOfBolts == 2))
					{
						if (component.BoltBrace.NumberOfRows == 0)
							component.BoltBrace.NumberOfRows = 2;
						if (Equals(g1, 0.0))
							g1 = g0;
						component.BoltBrace.EdgeDistTransvDir = d - g1 - g2;

						Fbre = CommonCalculations.EdgeBearing(component.BoltBrace.EdgeDistLongDir,
							(component.BraceConnect.Brace.WebLong), component.BoltBrace.BoltSize,
							component.Material.Fu, component.BoltBrace.HoleType, false);

						Fbrs = CommonCalculations.SpacingBearing(component.BoltBrace.SpacingLongDir,
							component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize,
							component.BoltBrace.HoleType, component.Material.Fu, false);

						N_RequiredB = ((component.AxialTension / component.Shape.tf - 2 * Fbre) / Fbrs + 2) / 2;
						N_RequiredBComp = component.AxialCompression / component.Shape.tf / Fbrs / 2;
						N_RequiredB = Math.Max(N_RequiredB, N_RequiredBComp);
						if (component.AxialTension > 0)
							N_RequiredForBlockShear = SmallMethodsDesign.NBRequiredforBraceBlockShear(memberType, 2 * t, EShapeType.DoubleAngle);
						else
							N_RequiredForBlockShear = 0;

						component.BoltBrace.NumberOfBolts = (int)Math.Ceiling(Math.Max(N_RequiredS, Math.Max(N_RequiredB, N_RequiredForBlockShear)));

						if (!component.EndSetback_User && component.BoltBrace.NumberOfBolts % 2 == 0 && !component.BraceConnect.Brace.BoltsAreStaggered)
							component.EndSetback = -(component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir * (component.BoltBrace.NumberOfBolts / 2 - 1) + component.BraceConnect.Gusset.EdgeDistance);
						else if(!component.EndSetback_User)
							component.EndSetback = -(component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir * ((component.BoltBrace.NumberOfBolts + 1) / 2 - 1) + component.BraceConnect.Gusset.EdgeDistance);
					}
					else
					{
						if (component.BoltBrace.NumberOfRows == 0 || d < 5 * ConstNum.ONE_INCH)
						{
							component.BoltBrace.NumberOfRows = 1;
						}

						component.BraceConnect.Brace.BoltsAreStaggered = false;
						component.BoltBrace.EdgeDistTransvDir = d - g1;

						Fbre = CommonCalculations.EdgeBearing(component.BoltBrace.EdgeDistLongDir,
							component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize,
							component.Material.Fu, component.BoltBrace.HoleType, false);

						Fbrs = CommonCalculations.SpacingBearing(component.BoltBrace.SpacingLongDir,
							component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize,
							component.BoltBrace.HoleType, component.Material.Fu, false);

						N_RequiredB = ((component.AxialTension / component.Shape.tf - 2 * Fbre) / Fbrs + 2) / 2;
						N_RequiredBComp = component.AxialCompression / component.Shape.tf / Fbrs / 2;
						N_RequiredB = Math.Max(N_RequiredB, N_RequiredBComp);

						if (component.AxialTension > 0)
							N_RequiredForBlockShear = SmallMethodsDesign.NBRequiredforBraceBlockShear(memberType, 2 * t, EShapeType.DoubleAngle);
						else
							N_RequiredForBlockShear = 0;
						component.BoltBrace.NumberOfBolts = (int) Math.Ceiling(Math.Max(N_RequiredS, Math.Max(N_RequiredB, N_RequiredForBlockShear)));
						if (component.BoltBrace.NumberOfBolts < 2)
							component.BoltBrace.NumberOfBolts = 2;
						if (!component.EndSetback_User)
							component.EndSetback = -(component.BoltBrace.EdgeDistLongDir + component.BoltBrace.SpacingLongDir * (component.BoltBrace.NumberOfBolts - 1) + component.BraceConnect.Gusset.EdgeDistance);
					}

				}
				component.BraceConnect.Brace.BraceWeld.Weld1L = -component.EndSetback;
				component.BraceConnect.Brace.BraceWeld.Weld1sz = 0;
			}
			else
			{
				// welded
				Pedge = cx / d * (component.BraceConnect.Brace.MaxForce / 2);
				pback = (d - cx) / d * (component.BraceConnect.Brace.MaxForce / 2);
				EdgeWeldArea = Pedge / (0.707 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				BackWeldArea = pback / (0.707 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx);
				minweldsize = CommonCalculations.MinimumWeld(component.Shape.tf, component.BraceConnect.Gusset.Thickness);
				maxweldsize = Math.Max(component.Shape.tf * ConstNum.FIOMEGA1_0N * 0.6 * component.Material.Fy / (ConstNum.FIOMEGA0_75N * 0.6 * 0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx), component.BraceConnect.Gusset.Thickness * ConstNum.FIOMEGA0_75N * 0.6 * component.BraceConnect.Gusset.Material.Fu / (ConstNum.FIOMEGA0_75N * 0.6 * 1.414 * CommonDataStatic.Preferences.DefaultElectrode.Fexx));
				tmpCaseArg = component.Shape.tf;
				if (tmpCaseArg < ConstNum.QUARTER_INCH)
				{
					if (maxweldsize > component.Shape.tf)
						maxweldsize = component.Shape.tf;
				}
				else
				{
					if (maxweldsize > component.Shape.tf - ConstNum.SIXTEENTH_INCH)
						maxweldsize = component.Shape.tf - ConstNum.SIXTEENTH_INCH;
				}
				if (minweldsize > maxweldsize)
					minweldsize = maxweldsize;

				component.BraceConnect.Brace.BraceWeld.Weld1L = BackWeldArea / component.BraceConnect.Brace.WeldSize;
				if (Math.Max(component.Shape.d, component.Shape.bf) > component.BraceConnect.Brace.BraceWeld.Weld1L)
					component.BraceConnect.Brace.BraceWeld.Weld1L = Math.Max(component.Shape.d, component.Shape.bf);

				if (!component.EndSetback_User)
					component.EndSetback = NumberFun.Round(-component.BraceConnect.Brace.BraceWeld.Weld1L, ERoundingPrecision.Fourth, ERoundingStyle.RoundDown);
				component.BraceConnect.Brace.BraceWeld.Weld1L = -component.EndSetback;
				do
				{
					betaN1 = CommonCalculations.WeldBetaFactor(component.BraceConnect.Brace.BraceWeld.Weld1L, component.BraceConnect.Brace.WeldSize);
					backWeldCapacity = betaN1 * 2 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * component.BraceConnect.Brace.WeldSize * component.BraceConnect.Brace.BraceWeld.Weld1L;
					betaN2 = CommonCalculations.WeldBetaFactor(component.BraceConnect.Brace.BraceWeld.Weld2L, component.BraceConnect.Brace.WeldSize);
					edgeWeldCapacity = betaN2 * 2 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * component.BraceConnect.Brace.BraceWeld.Weld2sz * component.BraceConnect.Brace.BraceWeld.Weld2L;
					capacity = backWeldCapacity + edgeWeldCapacity;
					if (capacity < component.BraceConnect.Brace.MaxForce)
					{
						if (component.BraceConnect.Brace.WeldSize < maxweldsize)
							component.BraceConnect.Brace.WeldSize = component.BraceConnect.Brace.WeldSize + ConstNum.SIXTEENTH_INCH;
						else
							component.BraceConnect.Brace.BraceWeld.Weld1L = component.BraceConnect.Brace.BraceWeld.Weld1L + NumberFun.ConvertFromFraction(4);
						loopAgain = true;
					}
					else
						loopAgain = false;
				} while (loopAgain);
				component.BraceConnect.Brace.BraceWeld.Weld2sz = component.BraceConnect.Brace.WeldSize;
				component.BraceConnect.Brace.BraceWeld.Weld2L = component.BraceConnect.Brace.BraceWeld.Weld1L * EdgeWeldArea / BackWeldArea;
			}

			// Tension Yielding Required Gross Area:
			FiRn = ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy * component.Shape.a;

			AgrossYielding = component.AxialTension / (ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy);
			AgAdditionalforYielding = Math.Max(0, AgrossYielding - component.Shape.a) * (r_y * component.Material.Fy / component.BraceConnect.Gusset.Material.Fy);
			excessForceY = component.AxialTension - FiRn;

			// Tension Rupture
			if (component.WebOrientation == EWebOrientation.OutOfPlane)
			{
				L1 = shortleg;
				L2 = LongLeg;
			}
			else
			{
				L2 = shortleg;
				L1 = LongLeg;
			}

			x_ = t / 2 * (t * L1 + Math.Pow(L2, 2) - Math.Pow(t, 2)) / (component.Shape.a / 2);
			switch (component.BraceToGussetWeldedOrBolted)
			{
				case EConnectionStyle.Welded:
					L = -component.BraceConnect.Gusset.ColumnSideSetback;
					U = 1 - x_ / L;
					FiRn = ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu * U * component.Shape.a;
					An = component.Shape.a;
					break;
				case EConnectionStyle.Bolted:
					if (component.BoltBrace.NumberOfRows == 1)
						An = component.Shape.a - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
					else if (component.BraceConnect.Brace.BoltsAreStaggered)
					{
						An1 = component.Shape.a - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
						An2 = component.Shape.a + (-2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) + Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2)) * t;
						An = Math.Min(An1, An2);
					}
					else
					{
						if (component.BoltBrace.NumberOfBolts % 2 == 0)
							An = component.Shape.a - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
						else
						{
							An1 = component.Shape.a - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * t;
							An2 = component.Shape.a + (Math.Pow(component.BoltBrace.SpacingLongDir, 2) / (4 * g2) - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH)) * t;
							An = Math.Min(An1, An2);
						}
					}
					L = (Math.Ceiling((double) component.BoltBrace.NumberOfBolts / component.BoltBrace.NumberOfRows) - 1) * component.BoltBrace.SpacingLongDir;
					U = 1 - x_ / L;
					break;
			}

			FiRn = ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu * U * An;

			excessForceR = component.AxialTension - FiRn;
			An_effective = component.AxialTension / (ConstNum.FIOMEGA0_75N * r_t * U * component.Material.Fu);
			AgAdditionalforRupture = Math.Max(0, An_effective / U + component.Shape.a - An - component.Shape.a) * (r_t * component.Material.Fu / component.BraceConnect.Gusset.Material.Fu);

			if (AgStiffener > 0)
			{
				component.BraceStiffener.Thickness = CommonCalculations.PlateThickness(AgStiffener / (component.Shape.d - component.Shape.kdet - ConstNum.ONEANDHALF_INCHES));
				component.BraceStiffener.WeldSize = NumberFun.Round((Math.Min(component.Material.Fu * component.Shape.tf / 2, component.BraceConnect.Gusset.Material.Fu * component.BraceStiffener.Thickness) / (0.707 * CommonDataStatic.Preferences.DefaultElectrode.Fexx)), 16);
			}

			excessforce = Math.Max(0, Math.Max(excessForceY, excessForceR));
			component.BraceStiffener.Width = component.Shape.d - component.Shape.kdet - ConstNum.ONEANDHALF_INCHES;
			component.BraceStiffener.Length = -component.BraceConnect.Gusset.ColumnSideSetback;
			component.BraceStiffener.Area = AgStiffener;
			component.BraceStiffener.Force = excessforce;

			switch (component.BraceToGussetWeldedOrBolted)
			{
				case EConnectionStyle.Bolted:
					Reporting.AddHeader("Bolted " + MiscMethods.GetComponentName(memberType) + " Brace to Gusset Connection");
					Reporting.AddLine("Brace Member: " + component.ShapeName);
					Reporting.AddLine(String.Format("Brace Force (Tension): " + component.AxialTension + ConstUnit.Force));
					Reporting.AddLine(String.Format("Brace Force (Compresssion): {0} {1}", component.AxialCompression, ConstUnit.Force));
					Reporting.AddLine(String.Format("Bolts: {0} - {1}", component.BoltBrace.NumberOfBolts, component.BoltBrace.BoltName));
					Reporting.AddLine(String.Format("Bolt Holes on Brace: {0} Transv. X, {1} Longit. X",
						component.BraceConnect.Brace.WebTrans, component.BraceConnect.Brace.WebLong));
					Reporting.AddLine(String.Format("Bolt Holes on Gusset: {0} Transv. X, {1} Longit. X",
						component.BraceConnect.Gusset.HoleTransP, component.BraceConnect.Gusset.HoleLongP));

					if (component.BoltBrace.Slot0 && component.BoltBrace.Slot1)
					{
						SminBrace = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, 2 * component.Shape.tf, component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts, component.BraceConnect.Brace.WebLong);
						SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, component.BraceConnect.Gusset.Thickness, component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts, component.BraceConnect.Gusset.HoleLongP);
					}
					else if (component.BoltBrace.Slot0)
					{
						SminBrace = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, 2 * component.Shape.tf, component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts, component.BoltBrace.BoltSize + ConstNum.SIXTEENTH_INCH);
						SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, component.BraceConnect.Gusset.Thickness, component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts, component.BraceConnect.Gusset.HoleLongP);
					}
					else if (component.BoltBrace.Slot1)
					{
						SminBrace = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, 2 * component.Shape.tf, component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts, component.BraceConnect.Brace.WebLong);
						SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, component.BraceConnect.Gusset.Thickness, component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts, component.BoltBrace.BoltSize + ConstNum.SIXTEENTH_INCH);
					}
					else
					{
						SminBrace = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, 2 * component.Shape.tf, component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts, component.BoltBrace.BoltSize + ConstNum.SIXTEENTH_INCH);
						SminGusset = SmallMethodsDesign.MinBoltSpacing(component.BoltBrace, component.Material.Fu, component.BraceConnect.Gusset.Thickness, component.BraceConnect.Brace.MaxForce / component.BoltBrace.NumberOfBolts, component.BoltBrace.BoltSize + ConstNum.SIXTEENTH_INCH);
					}

					Smin = Math.Min(SminBrace, SminGusset);
					if (component.BoltBrace.SpacingLongDir >= Smin)
						Reporting.AddLine(String.Format("Longitudinal Bolt Spacing = {0} >= Min. Spacing {1} {2} (OK)",
							component.BoltBrace.SpacingLongDir, Smin, ConstUnit.Length));
					else
						Reporting.AddLine(String.Format("Longitudinal Bolt Spacing = {0} >> Min. Spacing {1} {2} (NG)",
							component.BoltBrace.SpacingLongDir, Smin, ConstUnit.Length));

					edgemin = Math.Max(component.BoltBrace.MinEdgeSheared + component.BoltBrace.EdgeDistBrace, SmallMethodsDesign.MinClearDistForBearing(component.AxialTension / component.BoltBrace.NumberOfBolts, (2 * component.Shape.tf), component.Material.Fu, component.BoltBrace.HoleType) + component.BraceConnect.Brace.WebLong / 2);
					if (component.BoltBrace.EdgeDistBrace >= edgemin)
						Reporting.AddLine(String.Format("Longitudinal Edge Distance on Brace = {0} >= Minimum Edge Distance {1} {2} (OK)",
							component.BoltBrace.EdgeDistBrace, edgemin, ConstUnit.Length));
					else
						Reporting.AddLine(String.Format("Longitudinal Edge Distance on Brace = {0} >> Minimum Edge Distance {1} {2} (NG)",
							component.BoltBrace.EdgeDistBrace, edgemin, ConstUnit.Length));

					edgemin = Math.Max(component.BoltBrace.MinEdgeSheared + component.BoltBrace.Eincr, SmallMethodsDesign.MinClearDistForBearing(component.AxialTension / component.BoltBrace.NumberOfBolts, component.BraceConnect.Gusset.Thickness, component.BraceConnect.Gusset.Material.Fu, component.BoltBrace.HoleType) + component.BraceConnect.Gusset.HoleLongP / 2);
					if (component.BoltBrace.EdgeDistGusset >= edgemin)
						Reporting.AddLine(String.Format("Longitudinal Edge Distance on Gusset = {0} >= Minimum Edge Distance {1} {2} (OK)",
							component.BoltBrace.EdgeDistGusset, edgemin, ConstUnit.Length));
					else
						Reporting.AddLine(String.Format("Longitudinal Edge Distance on Gusset = {0} >> Minimum Edge Distance {1} {2} (NG)",
							component.BoltBrace.EdgeDistGusset, edgemin, ConstUnit.Length));

					if (component.BoltBrace.NumberOfRows == 2)
					{
						Smin = ConstNum.EIGHT_THIRDS * component.BoltBrace.BoltSize;
						spacing = CommonCalculations.AngleStandardGage(2, d);

						if (component.BraceConnect.Brace.BoltsAreStaggered)
						{
							if (component.BoltBrace.SpacingLongDir / 2 - component.BraceConnect.Brace.WebLong < component.BoltBrace.BoltSize)
							{
								sminB = component.BraceConnect.Brace.WebTrans + Math.Sqrt(Math.Pow(component.BoltBrace.BoltSize, 2) - Math.Pow(component.BoltBrace.SpacingLongDir / 2 - component.BraceConnect.Brace.WebLong, 2));
								sminB = Math.Sqrt(Math.Pow(sminB, 2) + Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2));
							}
							else
							{
								sminB = component.BraceConnect.Brace.WebTrans;
								sminB = Math.Sqrt(Math.Pow(sminB, 2) + Math.Pow(component.BoltBrace.BoltSize + component.BraceConnect.Brace.WebLong, 2));
							}
							if (component.BoltBrace.SpacingLongDir / 2 - component.BraceConnect.Gusset.HoleLongP < component.BoltBrace.BoltSize)
							{
								sminG = component.BraceConnect.Gusset.HoleTransP + Math.Sqrt(Math.Pow(component.BoltBrace.BoltSize, 2) - Math.Pow(component.BoltBrace.SpacingLongDir / 2 - component.BraceConnect.Gusset.HoleLongP, 2));
								sminG = Math.Sqrt(Math.Pow(sminG, 2) + Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2));
							}
							else
							{
								sminG = component.BraceConnect.Gusset.HoleTransP;
								sminG = Math.Sqrt(Math.Pow(sminG, 2) + Math.Pow(component.BoltBrace.BoltSize + component.BraceConnect.Gusset.HoleLongP, 2));
							}
							smin1 = Math.Max(Smin, Math.Max(sminB, sminG));
							if (Math.Sqrt(Math.Pow(Math.Max(component.BraceConnect.Gusset.HoleTransP, component.BraceConnect.Brace.WebTrans), 2) + Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2)) > smin1)
								Smin = Math.Max(component.BraceConnect.Gusset.HoleTransP, component.BraceConnect.Brace.WebTrans);
							else
								Smin = Math.Sqrt(Math.Pow(smin1, 2) - Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2));
						}
						else
						{
							sminB = component.BraceConnect.Brace.WebTrans + component.BoltBrace.BoltSize;
							sminG = component.BraceConnect.Gusset.HoleTransP + component.BoltBrace.BoltSize;
							smin1 = Math.Max(sminB, sminG);
							Smin = Math.Max(Smin, smin1);
						}

						if (spacing >= Smin)
							Reporting.AddLine(String.Format("Transverse Bolt Spacing = {0} >= Minimum Spacing {1} {2} (OK)", spacing, Smin, ConstUnit.Length));
						else
							Reporting.AddLine(String.Format("Transverse Bolt Spacing = {0} << Minimum Spacing {1} {2} (NG)", spacing, Smin, ConstUnit.Length));
					}

					edgemin = component.BoltBrace.MinEdgeRolled + component.BoltBrace.Eincr;
					if (component.BoltBrace.EdgeDistTransvDir >= edgemin)
						Reporting.AddLine(String.Format("Transverse Edge Distance = {0} >= Minimum Edge Distance {1} {2} (OK)",
							component.BoltBrace.EdgeDistTransvDir, edgemin, ConstUnit.Length));
					else
						Reporting.AddLine(String.Format("Transverse Edge Distance = {0} << Minimum Edge Distance {1} {2} (NG)",
							component.BoltBrace.EdgeDistTransvDir, edgemin, ConstUnit.Length));

					Reporting.AddHeader("Bolt " + " Shear Strength");
					FiRn = 2 * component.BoltBrace.NumberOfBolts * component.BoltBrace.BoltStrength;
					if (FiRn >= component.BraceConnect.Brace.MaxForce)
						Reporting.AddLine(String.Format("FiRn = 2 * N * Fv: 2 * {0} * {1} {2} >= {3} (OK)",
							component.BoltBrace.NumberOfBolts, component.BoltBrace.BoltStrength, ConstUnit.Force, FiRn));
					else
						Reporting.AddLine(String.Format("FiRn = 2 * N * Fv: 2 * {0} * {1} {2} << {3} (NG)",
							component.BoltBrace.NumberOfBolts, component.BoltBrace.BoltStrength, ConstUnit.Force, FiRn));

					Reporting.AddHeader("Brace Check - Bolt Bearing on Brace:");
					Fbre = CommonCalculations.EdgeBearing(component.BoltBrace.EdgeDistBrace, (component.BraceConnect.Brace.WebLong), component.BoltBrace.BoltSize, component.Material.Fu, component.BoltBrace.HoleType, false);
					if (component.BoltBrace.NumberOfRows == 2 && component.BraceConnect.Brace.BoltsAreStaggered)
					{
						edgdist = component.BoltBrace.EdgeDistBrace + component.BoltBrace.SpacingLongDir / 2;
						Reporting.AddLine("For the second bolt line:");
						FbreStag = CommonCalculations.EdgeBearing(edgdist, (component.BraceConnect.Brace.WebLong), component.BoltBrace.BoltSize, component.Material.Fu, component.BoltBrace.HoleType, false);
					}

					Fbrs = CommonCalculations.SpacingBearing(component.BoltBrace.SpacingLongDir, component.BraceConnect.Brace.WebLong, component.BoltBrace.BoltSize, component.BoltBrace.HoleType, component.Material.Fu, false);
					if (component.BoltBrace.NumberOfRows == 2 && component.BraceConnect.Brace.BoltsAreStaggered)
					{
						edgebearingCap = (Fbre + FbreStag) * 2 * component.Shape.tf;
						Reporting.AddLine("Bearing Strength of end bolts = (Fbre + Fbre1) * 2 * t");
						Reporting.AddLine(String.Format("{0} {1} = ({2} + {3}) * {4} * {5}",
							edgebearingCap, ConstUnit.Force, Fbre, FbreStag, component.Shape.tf, edgebearingCap));
					}
					else
					{
						edgebearingCap = component.BoltBrace.NumberOfRows * Fbre * 2 * component.Shape.tf;
						if (component.BoltBrace.NumberOfRows == 2)
						{
							Reporting.AddLine("Bearing Strength of end bolts = 2 * Fbre * 2 * t");
							Reporting.AddLine(String.Format("{0} {1} = 2 * {2} * 2 * {3}",
								edgebearingCap, ConstUnit.Force, Fbre, component.Shape.tf));
						}
						else
						{
							Reporting.AddLine("Bearing Strength of end bolts = * Fbre * 2 * t");
							Reporting.AddLine(String.Format("{0} {1} = {2} * 2 * {3}",
								edgebearingCap, ConstUnit.Force, Fbre, component.Shape.tf));
						}
					}

					SpacingbearingCap = (component.BoltBrace.NumberOfBolts - component.BoltBrace.NumberOfRows) * Fbrs * 2 * component.Shape.tf;
					SpacingbearingCapComp = component.BoltBrace.NumberOfBolts * Fbrs * 2 * component.Shape.tf;
					Reporting.AddLine("Bearing Strength of remaining bolts = (N - Ne) * Fbrs * 2 * t");
					Reporting.AddLine(String.Format("{0} {1} = ({2} - {3}) * {4} * 2 * {5}",
						SpacingbearingCap, ConstUnit.Force, component.BoltBrace.NumberOfBolts, component.BoltBrace.NumberOfRows, Fbrs, component.Shape.tf));

					Reporting.AddLine("Total Bearing Strength With Tensile Force:");
					capacity = edgebearingCap + SpacingbearingCap;

					if (capacity >= component.AxialTension)
						Reporting.AddLine(String.Format("FiRn = capacity = {0} + {1} = {2} >= {3} {4} (OK)",
							edgebearingCap, SpacingbearingCap, capacity, component.AxialTension, ConstUnit.Force));
					else
						Reporting.AddLine(String.Format("FiRn = capacity = {0} + {1} = {2} << {3} {4} (NG)",
							edgebearingCap, SpacingbearingCap, capacity, component.AxialTension, ConstUnit.Force));

					Reporting.AddLine("With Compressive Force:");
					Reporting.AddLine("FiRn = N * Fbrs * 2 * t");
					Reporting.AddLine(String.Format("{0} = {1} * {2} * 2 * {3}", FiRn, component.BoltBrace.NumberOfBolts, Fbrs, component.Shape.tf));
					if (SpacingbearingCapComp >= component.AxialCompression)
						Reporting.AddLine(String.Format("{0} >= {1} {2} (OK)",
							SpacingbearingCapComp, component.AxialCompression, ConstUnit.Force));
					else
						Reporting.AddLine(String.Format("{0} << {1} {2} (NG)",
							SpacingbearingCapComp, component.AxialCompression, ConstUnit.Force));

					if (component.AxialTension > 0)
					{
						Reporting.AddLine("Block Shear Rupture of the Brace:");
						g0 = CommonCalculations.AngleStandardGage(0, d);
						g1 = CommonCalculations.AngleStandardGage(1, d);
						g2 = CommonCalculations.AngleStandardGage(2, d);
						if (component.BoltBrace.NumberOfRows == 1)
						{
							Reporting.AddHeader("Bolt Gage on angle (g) = " + g0 + ConstUnit.Length);
							Agt = component.BoltBrace.EdgeDistTransvDir * 2 * t;
							Agv = (component.BoltBrace.EdgeDistBrace + (component.BoltBrace.NumberOfBolts - 1) * component.BoltBrace.SpacingLongDir) * 2 * t;
							Ant = (component.BoltBrace.EdgeDistTransvDir - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) / 2) * 2 * t;
							Anv = (component.BoltBrace.EdgeDistBrace + (component.BoltBrace.NumberOfBolts - 1) * component.BoltBrace.SpacingLongDir - (component.BoltBrace.NumberOfBolts - 0.5) * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH)) * 2 * t;
							
							Reporting.AddLine("Agt = et * 2 * t = ");
							Reporting.AddLine(String.Format("{0} {1} = {2} * 2 * {3}", Agt, ConstUnit.Length, component.BoltBrace.EdgeDistTransvDir, t));
							Reporting.AddLine(String.Format("Ant = Agt - 0.5 * (dh + {0}) * 2 * t", ConstNum.SIXTEENTH_INCH));
							Reporting.AddLine(String.Format("{0} {1} = {2} - 0.5 * ({3} + {4}) * 2 * {5}",
								Ant, ConstUnit.Area, Agt, component.BraceConnect.Brace.WebTrans, ConstNum.SIXTEENTH_INCH, t));

							Reporting.AddLine("Agv = (el + (Nl - 1) * sl) * 2 * t");
							Reporting.AddLine(String.Format("{0} {1} = ({2} + ({3} - 1) * {4}) * 2 {5}",
								Agv, ConstUnit.Area, component.BoltBrace.EdgeDistBrace, component.BoltBrace.NumberOfBolts, component.BoltBrace.SpacingLongDir, t));

							Reporting.AddLine(String.Format("Anv = Agv - (Nl - 0.5) * (dh + {0}) * 2 * t", ConstNum.SIXTEENTH_INCH));
							Reporting.AddLine(String.Format("{0} {1} = {2} - ({3} - 0.5) * ({4} + {5}) * 2 * {6}",
								Anv, ConstUnit.Area, Agv, component.BoltBrace.NumberOfBolts, component.BraceConnect.Brace.WebLong, ConstNum.SIXTEENTH_INCH, t));
						}
						else if (component.BraceConnect.Brace.BoltsAreStaggered)
						{
							Reporting.AddHeader("Bolt Gage on angle (g1) = " + g1 + ConstUnit.Length + ", " + "g2 = " + g2 + ConstUnit.Length);
							Agt = (component.BoltBrace.EdgeDistTransvDir + g2) * 2 * t;
							Ant1 = Agt - 0.5 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * 2 * t;
							Ant2 = Agt - 0.5 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * 2 * t + (Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2) - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH)) * 2 * t;
							Ant = Math.Min(Ant1, Ant2);
							Reporting.AddLine("Agt =(eat + g2) * 2 * t");
							Reporting.AddLine(String.Format("{0} {1} = ({2} + {3}) * 2 {4}",
								Agt, ConstUnit.Area, component.BoltBrace.EdgeDistTransvDir, g2, t));

							Reporting.AddLine(String.Format("Ant1 = Agt - 0.5 * (dh + {0}) * 2 * t", ConstNum.SIXTEENTH_INCH));
							Reporting.AddLine(String.Format("{0} {1} = {2} - 0.5 * ({3} + {4}) * 2 * {5}",
								Ant1, ConstUnit.Area, Agt, component.BraceConnect.Brace.WebTrans, ConstNum.SIXTEENTH_INCH, t));

							Reporting.AddLine(String.Format("Ant2 = Agt - 0.5 * (dh + {0}) * 2 * t + ((sl / 2)² / (4 * g2) - (dh + {0})) * 2 * t", ConstNum.SIXTEENTH_INCH));
							Reporting.AddLine(String.Format("{0} {1} = {2} - 0.5 * ({3} + {4}) * 2 * {5} + (({6} / 2)² / (4 * {7}) - ({8} + {4})) * 2 * {5}",
								Ant2, ConstUnit.Area, Agt, component.BraceConnect.Brace.WebTrans, ConstNum.SIXTEENTH_INCH, t, component.BoltBrace.SpacingLongDir, g2, component.BraceConnect.Brace.WebTrans));

							Reporting.AddLine(String.Format("Ant = Min(Ant1, Ant2) = {0} {1} = Min({2}, {3})", Ant, ConstUnit.Area, Ant1, Ant2));

							if (component.BoltBrace.NumberOfBolts % 2 == 0)
							{
								Agv = (component.BoltBrace.EdgeDistBrace + component.BoltBrace.SpacingLongDir / 2 + (component.BoltBrace.NumberOfBolts / 2 - 1) * component.BoltBrace.SpacingLongDir) * 2 * t;
								Anv = Agv - (component.BoltBrace.NumberOfBolts / 2.0 - 0.5) * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH) * 2 * t;
								Reporting.AddLine("Agv = (el + s / 2 + (Nl - 1) * s) * 2 * t");
								Reporting.AddLine(String.Format("{0} {1} = ({2} + {3} / 2 + ({2} - 1) * {4}) * 2 * {5}",
									Agv, ConstUnit.Area, component.BoltBrace.EdgeDistBrace, component.BoltBrace.SpacingLongDir, component.BoltBrace.NumberOfBolts, t));

								Reporting.AddLine(String.Format("Anv = Agv - (N1 - 0.5) * (dh + {0}) * 2 * t", ConstNum.SIXTEENTH_INCH));
								Reporting.AddLine(String.Format("{0} {1} = {2} - ({3} - 0.5) * * ({4} + {5}) * 2 * {6}",
									Anv, ConstUnit.Area, Agv, component.BoltBrace.NumberOfBolts, component.BraceConnect.Brace.WebLong, ConstNum.SIXTEENTH_INCH, t));
							}
							else
							{
								Agv = (component.BoltBrace.EdgeDistBrace + ((component.BoltBrace.NumberOfBolts + 1) / 2 - 1) * component.BoltBrace.SpacingLongDir) * 2 * t;
								Anv = Agv - ((component.BoltBrace.NumberOfBolts + 1) / 2.0 - 0.5) * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH) * 2 * t;
								Reporting.AddLine("Agv = (el + (N1 - 1) * sl) * 2 * t");
								Reporting.AddLine("= (" + component.BoltBrace.EdgeDistBrace + " + (" + component.BoltBrace.NumberOfBolts + " - 1)*" + component.BoltBrace.SpacingLongDir + "*2*" + t);
								Reporting.AddLine("= " + Agv + ConstUnit.Area);

								Reporting.AddLine("Anv = Agv - (Nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ")*2*t");
								Reporting.AddLine("= " + Agv + " - (" + component.BoltBrace.NumberOfBolts + " - 0.5)*(" + component.BraceConnect.Brace.WebLong + " + " + ConstNum.SIXTEENTH_INCH + ")*2* " + t);
								Reporting.AddLine("= " + Anv + ConstUnit.Area);
							}
						}
						else
						{
							Reporting.AddHeader("Bolt Gage on angle (g1) = " + g1 + ConstUnit.Length + ", " + "g2 = " + g2 + ConstUnit.Length);
							Reporting.AddLine(" ");
							if (component.BoltBrace.NumberOfBolts % 2 == 0)
							{
								Agt = (component.BoltBrace.EdgeDistTransvDir + g2) * 2 * t;
								Ant = Agt - 1.5 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * 2 * t;
								Reporting.AddLine("Agt = (et + g2) * 2 * t");
								Reporting.AddLine("= (" + component.BoltBrace.EdgeDistTransvDir + " + " + g2 + ") * 2 *" + t);
								Reporting.AddLine("= " + Agt + ConstUnit.Area);
								Reporting.AddLine("Ant = Agt - 1.5 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * 2 * t");
								Reporting.AddLine("= " + Agt + " - 1.5 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * 2 *" + t);
								Reporting.AddLine("= " + Ant + ConstUnit.Area);
								Agv = (component.BoltBrace.EdgeDistBrace + (component.BoltBrace.NumberOfBolts / 2 - 1) * component.BoltBrace.SpacingLongDir) * 2 * t;
								Anv = Agv - (component.BoltBrace.NumberOfBolts / 2.0 - 0.5) * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH) * 2 * t;
								Reporting.AddLine("Agv = (el + (Nl - 1) * sl) * 2 * t");
								Reporting.AddLine("= (" + component.BoltBrace.EdgeDistBrace + " + (" + component.BoltBrace.NumberOfBolts / 2 + " - 1) * " + component.BoltBrace.SpacingLongDir + ") * 2 * " + t);
								Reporting.AddLine("= " + Agv + ConstUnit.Area);
								Reporting.AddLine("Anv = Agv - (N1 - 0.5)*(dh + " + ConstNum.SIXTEENTH_INCH + ")*2*t");
								Reporting.AddLine("Anv = Agv - (" + component.BoltBrace.NumberOfBolts / 2 + " - 0.5) * (" + component.BraceConnect.Brace.WebLong + " + " + ConstNum.SIXTEENTH_INCH + ")*2*" + t);
								Reporting.AddLine("= " + Anv + ConstUnit.Area);
							}
							else
							{
								Agt = (component.BoltBrace.EdgeDistTransvDir + g2) * 2 * t;
								Ant1 = Agt - 0.5 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * 2 * t;
								Ant2 = Agt + (Math.Pow(component.BoltBrace.SpacingLongDir, 2) / (4 * g2) - 1.5 * component.BraceConnect.Brace.WebTrans) * 2 * t;
								Ant = Math.Min(Ant1, Ant2);
								Reporting.AddLine("Agt = (et + g2) * 2 * t");
								Reporting.AddLine("= (" + component.BoltBrace.EdgeDistTransvDir + " + " + g2 + ") * 2 * " + t);
								Reporting.AddLine("= " + Agt + ConstUnit.Area);

								Reporting.AddLine("Ant1 = Agt - 0.5 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * 2 * t");
								Reporting.AddLine("= " + Agt + " - 0.5*(" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ")*2*" + t);
								Reporting.AddLine("= " + Ant1 + ConstUnit.Area);

								Reporting.AddLine("Ant2 = Agt + (sl² /(4*g2) - 1.5*(dh + " + ConstNum.SIXTEENTH_INCH + ")) * 2 * t");
								Reporting.AddLine("= " + Agt + " + (" + component.BoltBrace.SpacingLongDir + "² /(4*" + g2 + ") - 1.5*(" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + "))*2*" + t);
								Reporting.AddLine("= " + Ant2 + ConstUnit.Area);

								Reporting.AddLine("Ant = Min(Ant1,Ant2) = " + Ant + ConstUnit.Area);

								Agv = (component.BoltBrace.EdgeDistBrace + ((component.BoltBrace.NumberOfBolts + 1) / 2 - 1) * component.BoltBrace.SpacingLongDir) * 2 * t;
								Anv = Agv - ((component.BoltBrace.NumberOfBolts + 1) / 2.0 - 0.5) * (component.BraceConnect.Brace.WebLong + ConstNum.SIXTEENTH_INCH) * 2 * t;
								Reporting.AddLine("Agv = (el + (Nl - 1) * sl) * 2 * t");
								Reporting.AddLine("= (" + component.BoltBrace.EdgeDistBrace + " + (" + component.BoltBrace.NumberOfBolts + " - 1) * " + component.BoltBrace.SpacingLongDir + ") * 2 * " + t);
								Reporting.AddLine("= " + Agv + ConstUnit.Area);

								Reporting.AddLine("Anv = Agv - (Nl - 0.5) * (dh + " + ConstNum.SIXTEENTH_INCH + ") * 2 * t");
								Reporting.AddLine("= " + Agv + " - (" + component.BoltBrace.NumberOfBolts + " - 0.5) * (" + component.BraceConnect.Brace.WebLong + " + " + ConstNum.SIXTEENTH_INCH + ") * 2 * " + t);
								Reporting.AddLine("= " + Anv + ConstUnit.Area);
							}
						}

						FiRn = SmallMethodsDesign.BlockShearPrint(Agv, Anv, Agt, Ant, component.Material.Fy, component.Material.Fu, true);
						if (FiRn >= component.AxialTension)
							Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
						else
							Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
					}

					FiRn = ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy * component.Shape.a;
					Reporting.AddHeader("Tension Yielding:");
					Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_9 + " * " + r_y_String + Yildiz + "Fy * Ag = " + ConstString.FIOMEGA0_9 + " * " + r_y_Strength + Yildiz + component.Material.Fy + " * " + component.Shape.a);
					if (FiRn >= component.AxialTension)
						Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
					{
						Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
						Reporting.AddLine("Brace Reinforcement Required.");
					}

					Reporting.AddHeader("Tension Rupture:");
					if (component.BoltBrace.NumberOfRows == 1)
					{
						An = component.Shape.a - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * 2 * t;
						Reporting.AddLine("An = Ag - (dh +" + ConstNum.SIXTEENTH_INCH + ") * 2 * t");
						Reporting.AddLine("= " + component.Shape.a + " - (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * 2*" + t + " = " + An + ConstUnit.Area);
					}
					else if (component.BraceConnect.Brace.BoltsAreStaggered)
					{
						Reporting.AddHeader("Bolt Gage on angle (g1) = " + g1 + ConstUnit.Length + ", " + "g2 = " + g2 + ConstUnit.Length);
						An1 = component.Shape.a - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * 2 * t;
						An2 = component.Shape.a + (-2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) + Math.Pow(component.BoltBrace.SpacingLongDir / 2, 2) / (4 * g2)) * 2 * t;
						An = Math.Min(An1, An2);
						Reporting.AddLine("An1 = Ag - (dh +" + ConstNum.SIXTEENTH_INCH + ") * 2 * t");
						Reporting.AddLine("= " + component.Shape.a + " - (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") *2* " + t);
						Reporting.AddLine("= " + An1 + ConstUnit.Area);

						Reporting.AddLine("An2 = Ag + (-2 * (dh + " + ConstNum.SIXTEENTH_INCH + ") + (s / 2)²/(4 * g2)) * 2 * t");
						Reporting.AddLine("= " + component.Shape.a + " + (-2 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") + (" + component.BoltBrace.SpacingLongDir + "/2)² /(4*" + g2 + "))*2*" + t);
						Reporting.AddLine("= " + An2 + ConstUnit.Area);

						Reporting.AddLine("An = Min(An1, An2) = " + An + ConstUnit.Area);
					}
					else
					{
						if (component.BoltBrace.NumberOfBolts % 2 == 0)
						{
							An = component.Shape.a - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * 2 * t;
							Reporting.AddLine("An = Ag - 2 * (dh + " + ConstNum.SIXTEENTH_INCH + ") * 2 * t");
							Reporting.AddLine("= " + component.Shape.a + " - 2 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") *2* " + t + " = " + An + ConstUnit.Area);
						}
						else
						{
							An1 = component.Shape.a - (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH) * 2 * t;
							An2 = component.Shape.a + (Math.Pow(component.BoltBrace.SpacingLongDir, 2) / (4 * g2) - 2 * (component.BraceConnect.Brace.WebTrans + ConstNum.SIXTEENTH_INCH)) * 2 * t;
							An = Math.Min(An1, An2);
							Reporting.AddLine("Bolt Gage on angle (g1) = " + g1 + ConstUnit.Length + ", " + "g2 = " + g2 + ConstUnit.Length);

							Reporting.AddLine("An1 = Ag - (dh +" + ConstNum.SIXTEENTH_INCH + ") * 2 * t");
							Reporting.AddLine("= " + component.Shape.a + " - (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") * 2* " + t + " = " + An1 + ConstUnit.Area);

							Reporting.AddLine("An2 = Ag + (-2 * (dh + " + ConstNum.SIXTEENTH_INCH + ") + s² / (4 * g2)) * 2 * t");
							Reporting.AddLine("= " + component.Shape.a + " + (-2 * (" + component.BraceConnect.Brace.WebTrans + " + " + ConstNum.SIXTEENTH_INCH + ") + " + component.BoltBrace.SpacingLongDir + "² /(4*" + g2 + "))*2*" + t);
							Reporting.AddLine("= " + An2 + ConstUnit.Area);

							Reporting.AddLine("An = Min(An1, An2) = " + An + ConstUnit.Area);
						}
					}

					Reporting.AddHeader("Shear Lag Factor (U) = 1 - x / L = 1 - " + x_ + " / " + L + " = " + U);

					FiRn = ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu * U * An;
					Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * " + r_t_String + Yildiz + "Fu * U * An");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + r_t_Strength + Yildiz + component.Material.Fu + " * " + U + " * " + An);
					if (FiRn >= component.AxialTension)
						Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
					{
						Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
						excessForceR = component.AxialTension - FiRn;
						Reporting.AddLine("Brace Reinforcement Required.");
					}
					break;
				case EConnectionStyle.Welded:
					Reporting.AddHeader("Welded Brace to Gusset Connection");
					Reporting.AddLine("Brace Force (Tension) = " + component.AxialTension + ConstUnit.Force);
					Reporting.AddLine("Brace Force (Compression) = " + component.AxialCompression + ConstUnit.Force);
					Reporting.AddLine("Brace to Gusset Weld Size = " + component.BraceConnect.Brace.WeldSize + ConstUnit.Length);
					Reporting.AddLine("Brace to Gusset Weld Length Along Heel of Each Angle = " + component.BraceConnect.Brace.BraceWeld.Weld1L + ConstUnit.Length);
					Reporting.AddLine("Brace to Gusset Weld Length Along Toe of Each Angle = " + component.BraceConnect.Brace.BraceWeld.Weld2L + ConstUnit.Length);

					minweldsize = CommonCalculations.MinimumWeld(component.Shape.tf, component.BraceConnect.Gusset.Thickness);
					tmpCaseArg = component.Shape.tf;
					if (tmpCaseArg < ConstNum.QUARTER_INCH)
						maxweldsize = component.Shape.tf;
					else
						maxweldsize = component.Shape.tf - ConstNum.SIXTEENTH_INCH;
					if (minweldsize > maxweldsize)
						minweldsize = maxweldsize;
					if (component.BraceConnect.Brace.WeldSize >= minweldsize)
						Reporting.AddLine("Weld Size = " + component.BraceConnect.Brace.WeldSize + " >= Minimum Weld Size = " + CommonCalculations.WeldSize(minweldsize) + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Weld Size = " + component.BraceConnect.Brace.WeldSize + " << Minimum Weld Size = " + CommonCalculations.WeldSize(minweldsize) + ConstUnit.Length + " (NG)");

					if (component.BraceConnect.Brace.WeldSize <= maxweldsize)
						Reporting.AddLine("Weld Size = " + component.BraceConnect.Brace.WeldSize + " <= Maximum Weld Size = " + CommonCalculations.WeldSize(maxweldsize) + ConstUnit.Length + " (OK)");
					else
						Reporting.AddLine("Weld Size = " + component.BraceConnect.Brace.WeldSize + " >> Maximum Weld Size = " + CommonCalculations.WeldSize(maxweldsize) + ConstUnit.Length + " (NG)");

					betaN1 = CommonCalculations.WeldBetaFactor(component.BraceConnect.Brace.BraceWeld.Weld1L, component.BraceConnect.Brace.WeldSize);
					backWeldCapacity = betaN1 * 2 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * component.BraceConnect.Brace.WeldSize * component.BraceConnect.Brace.BraceWeld.Weld1L;
					betaN2 = CommonCalculations.WeldBetaFactor(component.BraceConnect.Brace.BraceWeld.Weld2L, component.BraceConnect.Brace.WeldSize);
					edgeWeldCapacity = betaN2 * 2 * ConstNum.FIOMEGA0_75N * 0.6 * CommonDataStatic.Preferences.DefaultElectrode.Fexx * 0.707 * component.BraceConnect.Brace.BraceWeld.Weld2sz * component.BraceConnect.Brace.BraceWeld.Weld2L;
					Reporting.AddLine("Heel Weld Strength = " + ConstNum.BETAS + " * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fexx * 0.707 * w * L");
					Reporting.AddLine("= " + betaN1 + " * 2 * " + ConstString.FIOMEGA0_75 + "* 0.6 *" + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * 0.707 * " + component.BraceConnect.Brace.WeldSize + " * " + component.BraceConnect.Brace.BraceWeld.Weld1L);
					Reporting.AddLine("= " + backWeldCapacity + ConstUnit.Force);

					Reporting.AddLine("Toe Weld Strength = " + ConstNum.BETAS + " * 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * Fexx * 0.707 * w * L");
					Reporting.AddLine("= " + betaN2 + " * 2 * " + ConstString.FIOMEGA0_75 + " * 0.6 * " + CommonDataStatic.Preferences.DefaultElectrode.Fexx + " * 0.707 * " + component.BraceConnect.Brace.BraceWeld.Weld2sz + " * " + component.BraceConnect.Brace.BraceWeld.Weld2L);
					Reporting.AddLine("= " + edgeWeldCapacity + ConstUnit.Force);

					weldCapacity = backWeldCapacity + edgeWeldCapacity;
					if (weldCapacity >= component.BraceConnect.Brace.MaxForce)
						Reporting.AddLine("Total Weld Strength = " + weldCapacity + " >= " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("Total Weld Strength = " + weldCapacity + " << " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

					Gussetdevelopes = ConstNum.FIOMEGA0_75N * 0.6 * component.BraceConnect.Gusset.Material.Fu * component.BraceConnect.Gusset.Thickness * (component.BraceConnect.Brace.BraceWeld.Weld1L + component.BraceConnect.Brace.BraceWeld.Weld2L);
					Reporting.AddLine("Maximum Weld Force Gusset Can Develop:");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu * t * L");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.6*" + component.BraceConnect.Gusset.Material.Fu + " * " + component.BraceConnect.Gusset.Thickness + " * (" + component.BraceConnect.Brace.BraceWeld.Weld1L + " + " + component.BraceConnect.Brace.BraceWeld.Weld2L + ")");
					if (Gussetdevelopes >= component.BraceConnect.Brace.MaxForce)
						Reporting.AddLine("= " + Gussetdevelopes + " >= " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Gussetdevelopes + " << " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

					Bracedevelopes = ConstNum.FIOMEGA0_75N * 0.6 * component.Material.Fu * component.Shape.tf * 2 * (component.BraceConnect.Brace.BraceWeld.Weld1L + component.BraceConnect.Brace.BraceWeld.Weld2L);
					Reporting.AddLine("Maximum Weld Force Brace Can Develop:");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.6 * Fu * t * 2 * L");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * 0.6 * " + component.Material.Fu + " * " + component.Shape.tf + " * 2 * (" + component.BraceConnect.Brace.BraceWeld.Weld1L + " + " + component.BraceConnect.Brace.BraceWeld.Weld2L + ")");
					if (Bracedevelopes >= component.BraceConnect.Brace.MaxForce)
						Reporting.AddLine("= " + Bracedevelopes + " >= " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (OK)");
					else
						Reporting.AddLine("= " + Bracedevelopes + " << " + component.BraceConnect.Brace.MaxForce + ConstUnit.Force + " (NG)");

					Reporting.AddHeader("Brace Check");

					FiRn = ConstNum.FIOMEGA0_9N * r_y * component.Material.Fy * component.Shape.a;
					Reporting.AddLine("Tension Yielding:");
					Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_9 + " * " + r_y_String + Yildiz + "Fy * Ag = " + ConstString.FIOMEGA0_9 + " * " + r_y_Strength + Yildiz + component.Material.Fy + " * " + component.Shape.a);
					if (FiRn >= component.AxialTension)
						Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
					{
						Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
						Reporting.AddLine("Brace Reinforcement Required.");
					}

					Reporting.AddLine("Tension Rupture:");
					Reporting.AddLine("Shear Lag Factor (U) = 1 - x / L = 1 - " + x_ + " / " + L + " = " + U);
					FiRn = ConstNum.FIOMEGA0_75N * r_t * component.Material.Fu * U * component.Shape.a;
					Reporting.AddLine("FiRn = " + ConstString.FIOMEGA0_75 + " * " + r_t_String + Yildiz + "Fu * U * Ag");
					Reporting.AddLine("= " + ConstString.FIOMEGA0_75 + " * " + r_t_Strength + Yildiz + component.Material.Fu + " * " + U + " * " + component.Shape.a);
					if (FiRn >= component.AxialTension)
						Reporting.AddLine("= " + FiRn + " >= " + component.AxialTension + ConstUnit.Force + " (OK)");
					else
					{
						Reporting.AddLine("= " + FiRn + " << " + component.AxialTension + ConstUnit.Force + " (NG)");
						Reporting.AddLine("Brace Reinforcement Required.");
						excessForceR = component.AxialTension - FiRn;
					}
					break;
			}

			if (excessForceY > 0)
				Reporting.AddLine("Force in excess of Yield Strength = " + excessForceY + ConstUnit.Force);
			if (excessForceR > 0)
				Reporting.AddLine("Force in excess of Rupture Strength = " + excessForceR + ConstUnit.Force);

			AgStiffener = Math.Max(AgAdditionalforYielding, AgAdditionalforRupture);
			excessforce = Math.Max(0, Math.Max(excessForceY, excessForceR));
			component.BraceStiffener.Width = component.Shape.d - component.Shape.kdet - ConstNum.ONEANDHALF_INCHES;
			component.BraceStiffener.Length = -component.BraceConnect.Gusset.ColumnSideSetback;
			component.BraceStiffener.Area = AgStiffener;
			component.BraceStiffener.Force = excessforce;
			if (component.BraceStiffener.Area > 0)
			{
				Reporting.AddLine("WARNING:");
				Reporting.AddLine("Required Reinforcement Area Using " + component.BraceConnect.Gusset.Material.Name + " Steel:");
				Reporting.AddLine("A_gross = " + component.BraceStiffener.Area + ConstUnit.Area);
				Reporting.AddLine("Use cover plates to provide the required area.");
				Reporting.AddLine("The length of plates must be sufficient to develop the excess force for yielding and rupture");
				Reporting.AddLine("with welds or bolts considering gross and effective net areas.");
			}
		}
	}
}