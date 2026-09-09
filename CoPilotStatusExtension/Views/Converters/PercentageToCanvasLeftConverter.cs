using System;
using System.Globalization;
using System.Windows.Data;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Views.Converters;

//-----------------------------------------------------------------------------------------------------------------------------------------
/// <summary>
/// Converts a percentage in the range [0, 1] into a horizontal Canvas position.
/// The first value is the available width and the second value is the percentage.
/// </summary>
public sealed class PercentageToCanvasLeftConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length < 2
			|| values[0] is not double width
			|| values[1] is not double percentage
			|| double.IsNaN(width)
			|| double.IsInfinity(width)
			|| width <= 0)
		{
			return 0d;
		}

		double markerWidth = 3d;
		if (parameter is not null
			&& double.TryParse(parameter.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double configuredMarkerWidth)
			&& configuredMarkerWidth > 0)
		{
			markerWidth = configuredMarkerWidth;
		}

		percentage = Math.Max(0d, Math.Min(1d, percentage));
		return Math.Max(0d, Math.Min(width - markerWidth, width * percentage - markerWidth / 2d));
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
