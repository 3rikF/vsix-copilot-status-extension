
using System;
using System.Globalization;
using System.Windows.Data;

//---------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Views.Converters;

//---------------------------------------------------------------------------------------------------------------------
public abstract class ComparisonConverterBase : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (ConverterNumbersHelper.IsNumber(value) && ConverterNumbersHelper.IsNumber(parameter))
			return Compare(value, parameter);

		else
			return false;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();

	protected abstract bool Compare(dynamic valueA, dynamic valueB);
}

//---------------------------------------------------------------------------------------------------------------------
public sealed class IsLessThenConverter : ComparisonConverterBase
{
	protected override bool Compare(dynamic valueA, dynamic valueB)
		=> valueA < valueB;
}

//---------------------------------------------------------------------------------------------------------------------
public sealed class IsGreaterThenConverter : ComparisonConverterBase
{
	protected override bool Compare(dynamic valueA, dynamic valueB)
		=> valueA > valueB;
}