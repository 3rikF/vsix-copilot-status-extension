
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

//---------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Views.Converters;

/// <summary>
/// Converts a percentage value (0–100) into a LinearGradientBrush that fills
/// three colored segments up to the current value:
///   0– 50 % → Green
///  50– 75 % → Orange
///  75–100 % → Red
/// The portion beyond the current value is transparent (no fill).
/// </summary>
[ValueConversion(typeof(double), typeof(LinearGradientBrush))]
internal class ProgressSegmentBrushConverter : IValueConverter
{
	// Segment boundaries (0–1 scale)
	private const double SEG1_END = 0.50;   // 0–50 %
	private const double SEG2_END = 0.75;   // 50–75 %
	// SEG3_END = 1.0                        // 75–100 %

	private static readonly Color COLOR_SEG1  = Colors.ForestGreen;
	private static readonly Color COLOR_SEG2  = Colors.Orange;
	private static readonly Color COLOR_SEG3  = Colors.OrangeRed;
	private static readonly Color COLOR_EMPTY = Color.FromArgb(0, 0, 0, 0); // transparent

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (!ConverterNumbersHelper.TryGetNumber(value, out dynamic number))
			return Binding.DoNothing;


		double percent = System.Convert.ToDouble(number);
		percent = Math.Max(0.0, Math.Min(1.0, percent));
		double opacity = 1;

		if (ConverterNumbersHelper.TryGetNumber(parameter, out dynamic opacityParameter) && opacityParameter is >=0.0 and <=1.0)
			opacity = (double)opacityParameter;

		var brush = new LinearGradientBrush
		{
			StartPoint	= new System.Windows.Point(0, 0.5),
			EndPoint	= new System.Windows.Point(1, 0.5),
			Opacity		= opacity,
		};

		if (percent <= 0.0)
		{
			// Completely empty – single transparent stop
			brush.GradientStops.Add(new GradientStop(COLOR_EMPTY, 0.0));
			brush.GradientStops.Add(new GradientStop(COLOR_EMPTY, 1.0));
			return brush;
		}

		// --- Segment 1: 0 – 50 % (green) ---
		double seg1Fill = Math.Min(percent, SEG1_END);
		brush.GradientStops.Add(new GradientStop(COLOR_SEG1, 0.0));
		brush.GradientStops.Add(new GradientStop(COLOR_SEG1, seg1Fill));

		if (percent <= SEG1_END)
		{
			// Fill ends inside segment 1
			brush.GradientStops.Add(new GradientStop(COLOR_EMPTY, seg1Fill));
			brush.GradientStops.Add(new GradientStop(COLOR_EMPTY, 1.0));
			return brush;
		}

		// --- Segment 2: 50 – 75 % (orange) ---
		double seg2Fill = Math.Min(percent, SEG2_END);
		brush.GradientStops.Add(new GradientStop(COLOR_SEG2, SEG1_END));
		brush.GradientStops.Add(new GradientStop(COLOR_SEG2, seg2Fill));

		if (percent <= SEG2_END)
		{
			brush.GradientStops.Add(new GradientStop(COLOR_EMPTY, seg2Fill));
			brush.GradientStops.Add(new GradientStop(COLOR_EMPTY, 1.0));
			return brush;
		}

		// --- Segment 3: 75 – 100 % (red) ---
		brush.GradientStops.Add(new GradientStop(COLOR_SEG3, SEG2_END));
		brush.GradientStops.Add(new GradientStop(COLOR_SEG3, percent));

		if (percent < 1.0)
		{
			brush.GradientStops.Add(new GradientStop(COLOR_EMPTY, percent));
			brush.GradientStops.Add(new GradientStop(COLOR_EMPTY, 1.0));
		}

		return brush;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
