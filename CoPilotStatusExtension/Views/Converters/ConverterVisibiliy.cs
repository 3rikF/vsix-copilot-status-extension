
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Views.Converters;

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class BoolToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool boolValue && targetType == typeof(Visibility))
		{
			Visibility visibilityMode = Visibility.Collapsed;

			if (parameter is string p)
			{
				if (p?.StartsWith("!") ?? false)
					boolValue = !boolValue;

				if (p?.StartsWith("H") ?? false)
					visibilityMode = Visibility.Hidden;
			}

			return boolValue ? Visibility.Visible : visibilityMode;
		}

		return Binding.DoNothing;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}

public sealed class NullToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		//--- start values ---
		Visibility hideMode	= Visibility.Collapsed;
		bool isVisible		= (value is string strValue)
			? (!string.IsNullOrWhiteSpace(strValue))
			: (value is not null);

		//--- parameters ---
		if (parameter is string p)
		{
			if (p?.Contains("!") ?? false)
				isVisible = !isVisible;

			if (p?.Contains("H") ?? false)
				hideMode = Visibility.Hidden;
		}

		//--- final result ---
		return isVisible ? Visibility.Visible : hideMode;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}

public sealed class EqualityToVisibilityConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (targetType == typeof(Visibility))
		{
			return (value?.Equals(parameter) == true)
				? Visibility.Visible
				: Visibility.Collapsed;
		}

		else
			return Binding.DoNothing;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}