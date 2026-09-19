
using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Task = System.Threading.Tasks.Task;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Helper;

//-----------------------------------------------------------------------------------------------------------------------------------------
public static class ExtensionLogger
{
	[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Used like a variable")]
	private static IVsOutputWindowPane? _pane;

	public static async Task LogAsync(string message)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		_ = GetOrCreatePane()
			?.OutputStringThreadSafe($"[{DateTime.Now:HH:mm:ss.fff}] {message}\n");
	}

	private static IVsOutputWindowPane? GetOrCreatePane()
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		if (_pane is not null)
			return _pane;

		if (ServiceProvider.GlobalProvider.GetService(typeof(SVsOutputWindow)) is not IVsOutputWindow outputWindow)
			return null;

		Guid paneGuid	= new (CoPilotStatusExtensionPackage.PACKAGE_GUID_STRING);
		_				= outputWindow.GetPane(ref paneGuid, out _pane); //---- VSConstants.S_OK ----

		if (_pane is null)
		{
			_ = outputWindow.CreatePane(ref paneGuid, CoPilotStatusExtensionPackage.EXTENSION_NAME, fInitVisible: 1, fClearWithSolution: 0);
			_ = outputWindow.GetPane(ref paneGuid, out _pane);
		}

		return _pane;
	}

	//public static async Task ShowPaneAsync()
	//{
	//	await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
	//	_pane?.Activate(); // Bringt den Pane in den Vordergrund
	//}
}