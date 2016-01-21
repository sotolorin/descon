using System;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;

namespace Descon.Forms
{
	public class GaugeSection : Shape
	{
		/// <summary>
		/// Radious of the entire gauge
		/// </summary>
		public double GaugeRadius { private get; set; }
		/// <summary>
		/// How far one section is pushed out from the center
		/// </summary>
		public double GaugePushOut { private get; set; }
		/// <summary>
		/// Radius of the inner empty section of the gauge
		/// </summary>
		public double GaugeInnerRadius { private get; set; }
		/// <summary>
		/// Size of one wedge or piece. 90 degress would be 1/4 of a complete circle
		/// </summary>
		public double GaugeWedgeAngle { private get; set; }
		/// <summary>
		/// Angle of the complete circle where the piece starts. 0 degrees is top middle
		/// </summary>
		public double GaugePieceStartAngle { private get; set; }
		/// <summary>
		/// X Coordinate of the center of the gauge
		/// </summary>
		public double GaugeCenterX { private get; set; }
		/// <summary>
		/// Y Coordinate of the center of the gauge
		/// </summary>
		public double GaugeCenterY { private get; set; }

		protected override Geometry DefiningGeometry
		{
			get
			{
				StreamGeometry geometry = new StreamGeometry();
				geometry.FillRule = FillRule.EvenOdd;

				using (StreamGeometryContext context = geometry.Open())
				{
					DrawGeometry(context);
				}

				geometry.Freeze();

				return geometry;
			}
		}

		/// <summary>
		/// Draws the pie piece
		/// </summary>
		private void DrawGeometry(StreamGeometryContext context)
		{
			if (GaugeCenterX == 0 && GaugeCenterY == 0)		// Don't go through the drawing if the canvas has no size yet
				return;

			bool isLargeArc = GaugeWedgeAngle > 180;

			Point innerArcStartPoint = ComputeCartesianCoordinate(GaugePieceStartAngle, GaugeInnerRadius);
			innerArcStartPoint.Offset(GaugeCenterX, GaugeCenterY);

			Point innerArcEndPoint = ComputeCartesianCoordinate(GaugePieceStartAngle + GaugeWedgeAngle, GaugeInnerRadius);
			innerArcEndPoint.Offset(GaugeCenterX, GaugeCenterY);

			Point outerArcStartPoint = ComputeCartesianCoordinate(GaugePieceStartAngle, GaugeRadius);
			outerArcStartPoint.Offset(GaugeCenterX, GaugeCenterY);

			Point outerArcEndPoint = ComputeCartesianCoordinate(GaugePieceStartAngle + GaugeWedgeAngle, GaugeRadius);
			outerArcEndPoint.Offset(GaugeCenterX, GaugeCenterY);

			if (GaugePushOut > 0)
			{
				Point offset = ComputeCartesianCoordinate(GaugePieceStartAngle + GaugeWedgeAngle / 2, GaugePushOut);
				innerArcStartPoint.Offset(offset.X, offset.Y);
				innerArcEndPoint.Offset(offset.X, offset.Y);
				outerArcStartPoint.Offset(offset.X, offset.Y);
				outerArcEndPoint.Offset(offset.X, offset.Y);
			}

			Size outerArcSize = new Size(GaugeRadius, GaugeRadius);
			Size innerArcSize = new Size(GaugeInnerRadius, GaugeInnerRadius);

			context.BeginFigure(innerArcStartPoint, true, true);
			context.LineTo(outerArcStartPoint, true, true);
			context.ArcTo(outerArcEndPoint, outerArcSize, 0, isLargeArc, SweepDirection.Clockwise, true, true);
			context.LineTo(innerArcEndPoint, true, true);
			context.ArcTo(innerArcStartPoint, innerArcSize, 0, isLargeArc, SweepDirection.Counterclockwise, true, true);

			// Figure out where the tip of the triangle should be (2 pixels below the gauge ring)
			double triangleTip = (GaugeCenterY * 2) - GaugeRadius - GaugeInnerRadius + 2;
			Point triangleStartPoint = new Point(GaugeCenterX, triangleTip);

			// Draw the triangle in proportion to the size of the gauge
			context.BeginFigure(triangleStartPoint, true, true);
			context.LineTo(new Point(triangleStartPoint.X - GaugeRadius / 10, triangleStartPoint.Y + GaugeRadius / 5), true, true);
			context.LineTo(new Point(triangleStartPoint.X + GaugeRadius / 10, triangleStartPoint.Y + GaugeRadius / 5), true, true);
		}

		/// <summary>
		/// Converts a coordinate from the polar coordinate system to the cartesian coordinate system.
		/// </summary>
		private Point ComputeCartesianCoordinate(double angle, double radius)
		{
			// convert to radians
			double angleRad = (Math.PI / 180.0) * (angle - 90);

			double x = radius * Math.Cos(angleRad);
			double y = radius * Math.Sin(angleRad);

			return new Point(x, y);
		}
	}
}