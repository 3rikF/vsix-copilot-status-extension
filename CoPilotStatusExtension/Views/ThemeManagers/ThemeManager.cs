
using System.Windows;
using System.Windows.Media;

using Microsoft.VisualStudio.PlatformUI;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Views.ThemeManagers;

//-----------------------------------------------------------------------------------------------------------------------------------------
/// <summary>
/// Keeps theme-dependent resources in sync with the active Visual Studio theme.
/// Call <see cref="Initialize"/> once after the WPF application resources are available.
/// </summary>
internal sealed class ThemeManager : IThemeManager
{
	private const string MOUSE_OVER_BACKGROUND_KEY = "MouseOverBackground";

	//---------------------------------------------------------------------
	public void Initialize()
	{
		ApplyTheme();
		VSColorTheme.ThemeChanged += OnThemeChanged;
	}

	//---------------------------------------------------------------------
	public void Cleanup()
	{
		VSColorTheme.ThemeChanged -= OnThemeChanged;
	}

	//---------------------------------------------------------------------
	private void OnThemeChanged(ThemeChangedEventArgs e)
		=> ApplyTheme();

	//---------------------------------------------------------------------
	private static void ApplyTheme()
	{
		// Detect light vs. dark by sampling the VS environment background color.
		// EnvironmentBackground is light in Light/Blue themes, dark in Dark theme.
		System.Drawing.Color bg = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
		bool isDark = (bg.R * 0.299 + bg.G * 0.587 + bg.B * 0.114) < 128;

		SolidColorBrush brush = isDark
			? new SolidColorBrush(Color.FromArgb(31, 255, 255, 255))	// white @ 12% opacity for dark themes
			: new SolidColorBrush(Color.FromArgb(51, 0, 0, 0));			// black @ 20% opacity for light themes

		brush.Freeze();

		Application.Current.Resources[MOUSE_OVER_BACKGROUND_KEY] = brush;
	}
}
