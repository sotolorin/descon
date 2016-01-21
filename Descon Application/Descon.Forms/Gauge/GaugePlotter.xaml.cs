using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Descon.Forms
{
	/// <summary>
	/// Gauge made up of two parts that shows what capacity we are at.
	/// </summary>
	public partial class GaugePlotter : INotifyPropertyChanged
	{
		/// <summary>
		/// Gauge capacity value to use for colored portion. Anything over 1.0 will make the entire 225 degrees red
		/// </summary>
		public double GaugeCapacity
		{
			private get { return (double)GetValue(GaugeCapacityProperty); }
			set { SetValue(GaugeCapacityProperty, value); }
		}

		public static readonly DependencyProperty GaugeCapacityProperty =
			DependencyProperty.Register("GaugeCapacity", typeof (double), typeof (GaugePlotter), new PropertyMetadata(0.7, ConstructGaugePieces));

		/// <summary>
		/// Percentage value used to bind to whichever data is needed
		/// </summary>
		public string GaugeCapacityText
		{
			private get { return (string)GetValue(GaugeCapacityTextProperty); }
			set { SetValue(GaugeCapacityTextProperty, value); }
		}

		public static readonly DependencyProperty GaugeCapacityTextProperty =
			DependencyProperty.Register("GaugeCapacityText", typeof (string), typeof (GaugePlotter), new PropertyMetadata("Capacity", ConstructGaugePieces));

		private static void ConstructGaugePieces(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			var gaugePlotter = obj as GaugePlotter;
			if (gaugePlotter != null)
				gaugePlotter.ConstructGaugePieces();
		}

		public GaugePlotter()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Color of the first part of the gauge and the text which is why this must remain public.
		/// </summary>
		private Brush gaugeCapacityColor
		{
			get
			{
				if (GaugeCapacity >= 0 && GaugeCapacity <= 0.9)
					return new SolidColorBrush(Color.FromRgb(12, 152, 122));
				else if (GaugeCapacity > 0.9 && GaugeCapacity <= 1.0)
					return new SolidColorBrush(Color.FromRgb(251, 177, 96));
				else
					return new SolidColorBrush(Color.FromRgb(196, 71, 71));
			}
		}

		/// <summary>
		/// Constructs pie pieces and adds them to the visual tree for this control's canvas
		/// </summary>
		private void ConstructGaugePieces()
		{
			double coloredAnglePortion;	// Colored portion that represents the capacity value
			double radius = canvas.ActualWidth / 2;
			double xPosition = canvas.ActualWidth / 2;
			double yPosition = canvas.ActualHeight / 2;

			const int HALF_CIRCLE = 180;	// Degrees in half circle
			const int TOTAL_ANGLE = 198;	// Total degrees to use for the entire gauge. Based on 1.10 X 180 degrees since 1.10 is the max capacity we want to show
			const double INNER_RADIUS_FACTOR = 5.0 / 6.0;	// Size of the blank inner part of the gauge

			if (radius > canvas.ActualHeight / 2)
				radius = canvas.ActualHeight / 2;

			if (GaugeCapacity > 0 && GaugeCapacity <= 1.00)
				coloredAnglePortion = GaugeCapacity * HALF_CIRCLE;
			else if (GaugeCapacity >= 0 && GaugeCapacity <= 1.10) // Percentage past the top portion of the gauge. 1.10 is maxed out.
				coloredAnglePortion = HALF_CIRCLE + ((TOTAL_ANGLE - HALF_CIRCLE) * (GaugeCapacity - 1) * 10);
			else
				coloredAnglePortion = TOTAL_ANGLE;

			// Gray section that is always the size of the entire gauge
			var gaugeBackgroundWedge = new GaugeSection();
			gaugeBackgroundWedge.GaugeRadius = radius;
			gaugeBackgroundWedge.GaugeInnerRadius = radius * INNER_RADIUS_FACTOR;
			gaugeBackgroundWedge.GaugeCenterX = xPosition;
			gaugeBackgroundWedge.GaugeCenterY = yPosition;
			gaugeBackgroundWedge.GaugeWedgeAngle = TOTAL_ANGLE;
			gaugeBackgroundWedge.GaugePieceStartAngle = HALF_CIRCLE;
			gaugeBackgroundWedge.Fill = Brushes.LightGray;

			// Colored gauge section that represents the actual capacity value and covers the gray wedge
			var gaugeCapacityWedge = new GaugeSection();
			gaugeCapacityWedge.GaugeRadius = radius;
			gaugeCapacityWedge.GaugeInnerRadius = radius * INNER_RADIUS_FACTOR;
			gaugeCapacityWedge.GaugeCenterX = xPosition;
			gaugeCapacityWedge.GaugeCenterY = yPosition;
			gaugeCapacityWedge.GaugeWedgeAngle = coloredAnglePortion;
			gaugeCapacityWedge.GaugePieceStartAngle = HALF_CIRCLE;
			gaugeCapacityWedge.Fill = gaugeCapacityColor;

			// Text displaying the actual gauge capacity in the center of the control
			var textBlock = new Label();
			if (GaugeCapacity == double.MaxValue)
				textBlock.Content = "NG";
			else
				textBlock.Content = String.Format("{0:N2}", GaugeCapacity);
			textBlock.Width = canvas.ActualWidth;
			textBlock.Height = canvas.ActualHeight;
			textBlock.HorizontalContentAlignment = HorizontalAlignment.Center;
			textBlock.VerticalContentAlignment = VerticalAlignment.Center;
			textBlock.Foreground = gaugeCapacityColor;
			textBlock.FontWeight = FontWeights.Bold;
			textBlock.FontSize = 16;

			canvas.Children.Clear();
			canvas.Children.Insert(0, gaugeBackgroundWedge);	// First add the gray base making up the total gauge size
			canvas.Children.Insert(1, gaugeCapacityWedge);		// Then add the colored capacity section covering the gray base
			canvas.Children.Insert(2, textBlock);				// Finally add the gauge value in the middle over everything

			OnPropertyChanged("gaugeCapacityColor");
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			ConstructGaugePieces();
		}
	}
}