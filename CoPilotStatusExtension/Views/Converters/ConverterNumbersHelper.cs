
using System.Globalization;

//---------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Views.Converters;

//---------------------------------------------------------------------------------------------------------------------
internal static class ConverterNumbersHelper
{
	public static bool IsInteger(object value)
		=> value is byte or short or ushort or int or uint or long or ulong;

	public static bool IsReal(object value)
		=> value is float or double or decimal;

	public static bool IsNumber(object value)
		=> IsInteger(value) || IsReal(value);

	public static bool TryGetInteger(object? value, out dynamic number)
	{
		if (value is null)
		{
			number = 0;
			return false;
		}
		else if (IsInteger(value))
		{
			number = value;
			return true;
		}

		else if (long.TryParse(value?.ToString() ?? string.Empty, NumberStyles.Any, CultureInfo.InvariantCulture, out long pno1))
		{
			number = pno1;
			return true;
		}

		else
		{
			number = 0;
			return false;
		}
	}

	public static bool TryGetNumber(object? value, out dynamic number)
	{
		if (value is null)
		{
			number = 0D;
			return false;
		}
		else if (IsNumber(value))
		{
			number = value;
			return true;
		}

		else if (double.TryParse(value?.ToString() ?? string.Empty, NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
		{
			number = d;
			return true;
		}

		else
		{
			number = 0D;
			return false;
		}
	}
}
