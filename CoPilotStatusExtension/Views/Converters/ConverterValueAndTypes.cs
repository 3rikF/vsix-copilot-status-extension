using System;
using System.Globalization;
using System.Windows.Data;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Views.Converters;

//-----------------------------------------------------------------------------------------------------------------------------------------
//public sealed class UnixMillisecondsToDateTimeConverter : IValueConverter
//{
//	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
//	{
//		if (ConverterNumbersHelper.TryGetNumber(value, out dynamic number))
//		{
//			long milliseconds				= System.Convert.ToInt64(number);
//			DateTimeOffset dateTimeOffset	= DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
//
//			return dateTimeOffset.LocalDateTime;
//		}
//		else
//			return Binding.DoNothing;
//	}
//
//	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
//		=> throw new NotSupportedException();
//}

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class UtcToLocalConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is DateTime dateTime)
			return dateTime.ToLocalTime();

		else if (value is DateTimeOffset dateTimeOffset)
			return dateTimeOffset.ToLocalTime();

		else
			return Binding.DoNothing;
	}
	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		=> throw new NotSupportedException();
}

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class BoolToTextConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is not bool boolean)
			return Binding.DoNothing;

		if (parameter is string paramString)
		{
			string[] parts = paramString.Split(';');

			if (parts.Length == 2)
				return boolean ? parts[0] : parts[1];
		}

		return boolean ? "Yes" : "No";
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}