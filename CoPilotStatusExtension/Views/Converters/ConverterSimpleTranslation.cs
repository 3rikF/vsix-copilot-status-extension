using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Views.Converters;

//-----------------------------------------------------------------------------------------------------------------------------------------
file static class SimpleStaticTranslations
{
	private readonly static Dictionary<string, string> TRANSLATIONS = new()
	{
		//--- account type ---
		{"individual",						"Individual"},
		{"business",						"Organization"},
		{"enterprise",						"Enterprise"},

		//--- CoPilot subscription status ---
		{"free_limited_quota",				"Free (limited quota)"},
		{"trial_subscriber_quota",			"Trial Subscriber"},
		{"yearly_subscriber_quota",			"Yearly Subscriber"},
		{"copilot_enterprise_seat_quota",	"Copilot Enterprise"},
		{"copilot_for_business_seat_quota", "Copilot for Business"},

		//--- quota IDs ---
		{"chat",							"Chat"},
		{"completions",						"Completions"},
		{"premium_interactions",			"AI-Credits"},
	};

	public static bool TryGetValue(string key, out string? value)
		=> TRANSLATIONS.TryGetValue(key, out value);

	public static string Translate(string key)
	{
		if (TRANSLATIONS.TryGetValue(key, out var value))
			return value;
		else
			return $"Unknown [{key}]";
	}
}

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class SimpleTranslationConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is string key && !string.IsNullOrWhiteSpace(key))
			return SimpleStaticTranslations.Translate(key);

		else
			//return DependencyProperty.UnsetValue;
			return Binding.DoNothing;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
