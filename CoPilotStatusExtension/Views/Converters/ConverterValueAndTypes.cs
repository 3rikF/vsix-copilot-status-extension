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
public sealed class StatusToTextConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is string status)
		{
			if (status is null || string.IsNullOrWhiteSpace(status))
				return "Status not available";

			else if (status.Equals("NotSignedInToGitHub", StringComparison.OrdinalIgnoreCase))
				return "Not signed in";

			else if (status.Equals("OK", StringComparison.OrdinalIgnoreCase))
				return "OK";

			else
				return $"Unhandled status [{status}]";
		}
		else
			return Binding.DoNothing;
	}
	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		=> throw new NotSupportedException();
}

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class SubscriptionToTextConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is string status)
		{
			return status switch
			{
				"free_limited_quota"			=> "Free (limited quota)",
				"trial_subscriber_quota"		=> "Trial Subscriber",
				"yearly_subscriber_quota"		=> "Yearly Subscriber",
				"copilot_enterprise_seat_quota"	=> "Copilot Enterprise",
				"copilot_for_business_seat_quota" => "Copilot for Business",
				_								=> $"Unknown [{status}]"
			};
		}
		else
			return Binding.DoNothing;
	}
	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		=> throw new NotSupportedException();
}

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class CopilotPlanToTextConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is string plan)
		{
			return plan switch
			{
				"individual"			=> "Individual",
				"enterprise"			=> "Organization",
				_						=> $"Unknown [{plan}]"
			};
		}
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