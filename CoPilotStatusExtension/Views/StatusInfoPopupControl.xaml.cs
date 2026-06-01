using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Views;

//-----------------------------------------------------------------------------------------------------------------------------------------
public partial class StatusInfoPopupControl : UserControl
{
	public StatusInfoPopupControl()
		=> InitializeComponent();

	private void OnUserProfileClick(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement element && element.Tag is string url && !string.IsNullOrEmpty(url))
		{
			_ = Process.Start(new ProcessStartInfo(url)
			{
				UseShellExecute = true,
			});
		}
	}
}