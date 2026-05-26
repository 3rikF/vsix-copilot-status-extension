
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

using CoPilotStatusExtension.GitHubApiModels;
using CoPilotStatusExtension.Models;
using CoPilotStatusExtension.Views;

using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension;

//-----------------------------------------------------------------------------------------------------------------------------------------
/// <summary>
/// This is the class that implements the package exposed by this assembly.
/// </summary>
/// <remarks>
/// <para>
/// The minimum requirement for a class to be considered a valid package for Visual Studio
/// is to implement the <see cref="IVsPackage"/> interface (done by inheriting from <see cref="AsyncPackage"/>)
/// and register itself with the shell.
/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
/// to do it: it derives from the Package class that provides the implementation of the
/// IVsPackage interface and uses the registration attributes defined in the framework to
/// register itself and its components with the shell. These attributes tell the pkgdef creation
/// utility what data to put into .pkgdef file.
/// </para>
/// <para>
/// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
/// </para>
/// </remarks>
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
[Guid(CoPilotStatusExtensionPackage.PACKAGE_GUID_STRING)]
public sealed class CoPilotStatusExtensionPackage : AsyncPackage
{
	//-----------------------------------------------------------------------------------------------------------------
	#region Fields

	/// <summary>
	/// CoPilotStatusExtensionPackage GUID string.
	/// </summary>
	public const string PACKAGE_GUID_STRING = "86cc1277-85e2-4030-ba7f-ceb48452ad40";

	private Timer					_refreshTimer	= null!;
	private GitHubStatusBarControl	_statusControl	= null!;
	private GitHubApiService		_gitHubService	= null!;
	private CoPilotTokenManager?	_tokenManager	= null;

	private readonly object _syncRoot = new();

	#endregion Fields

	//-----------------------------------------------------------------------------------------------------------------
	#region Visual Tree Helper

	private static StatusBar? FindStatusBar()
	{
		return Application.Current?.MainWindow is not Window mainWindow
			? null
			: FindChild<StatusBar>(mainWindow);
	}

	private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
	{
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(parent, i);

			if (child is T match)
				return match;

			else if (FindChild<T>(child) is T result)
				return result;
		}

		return null;
	}

	#endregion Visual Tree Helper

	//-----------------------------------------------------------------------------------------------------------------
	#region Package Members

	/// <summary>
	/// Initialization of the package; this method is called right after the package is sited, so this is the place
	/// where you can put all the initialization code that rely on services provided by VisualStudio.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
	/// <param name="progress">A provider for progress updates.</param>
	/// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		//--- Initialize Controls -----------------------------------------------------------------
		// When initialized asynchronously, the current thread may be a background thread at this point.
		// Do any initialization that requires the UI thread after switching to the UI thread.
		await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

		if (FindStatusBar() is not StatusBar statusBar)
			return;

		_statusControl	= new GitHubStatusBarControl();

		StatusBarItem item = new ()
		{
			Content						= _statusControl,
			HorizontalAlignment			= HorizontalAlignment.Right,
			Padding						= new Thickness(0),
			VerticalContentAlignment	= VerticalAlignment.Stretch,
			HorizontalContentAlignment	= HorizontalAlignment.Stretch,
		};

		DockPanel.SetDock(item, Dock.Right);
		_ = statusBar.Items.Add(item);

		//--- Initialize GitHub MEF ---------------------------------------------------------------
		_gitHubService	= new GitHubApiService();

		if (GetGlobalService(typeof(SComponentModel)) is  IComponentModel componentModel)
		{
			_tokenManager = new CoPilotTokenManager(componentModel);

			//--- initialize MEF end register [CopilotIdentityChanged] event-handler ---
			_tokenManager.InitializeCopilotMef(OnMainWindowActivated);

			//--- refresh when the VS window regains focus ---
			Application.Current.MainWindow.Activated += OnMainWindowActivated;

			//--- refresh after 60s fallback timer ---
			_refreshTimer = new Timer(_ => OnMainWindowActivated(null, null), null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
		}
	}

	private void OnMainWindowActivated(object? sender, EventArgs? e)
	{
		if (_tokenManager is null)
			return;

		_ = JoinableTaskFactory.RunAsync(RefreshGitHubStatusAsync);
	}

	private async Task RefreshGitHubStatusAsync()
	{
		if (_tokenManager is null)
			return;

		if (!Monitor.TryEnter(_syncRoot))
			return;

		try
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync();

			//--- determine CoPilot username and access-token -------------------------------------
			CopilotUserInfo? copilotInfo	= _tokenManager.GetCopilotUserInfo();

			if (copilotInfo is null || string.IsNullOrEmpty(copilotInfo.Username) || string.IsNullOrEmpty(copilotInfo.AccessToken))
			{
				await JoinableTaskFactory.SwitchToMainThreadAsync();
				_statusControl.SetData(null, null, null, null);

				return;
			}


			//--- fetch GitHub API data -----------------------------------------------------------

			//--- Billing -----------------------------------------------------
			(int billingStatusCode, string billingReasonPhrase, CopilotBillingUsage? billingUsage)
				= await _gitHubService
					.FetchUserBillingUsageAsync(copilotInfo.Username, copilotInfo.AccessToken)
					.ConfigureAwait(false);


			//--- Personal Metrics --------------------------------------------
			(int chatStatusCode, string chatReasonPhrase, CopilotQuotaResponse? personalQuota, RateLimitInfo? apiRateLimit) = await _gitHubService
				.FetchUserChatUsageAsync(copilotInfo.AccessToken)
				.ConfigureAwait(false);


			//--- Update Status UI --------------------------------------------
			await JoinableTaskFactory.SwitchToMainThreadAsync();
			_statusControl.SetData(copilotInfo, billingUsage, personalQuota, apiRateLimit);
		}
		catch (Exception ex)
		{
			await Console.Error.WriteLineAsync(ex.ToString());
		}
		finally
		{
			Monitor.Exit(_syncRoot);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_refreshTimer?.Dispose();

			if (Application.Current?.MainWindow is not  null)
				Application.Current.MainWindow.Activated -= OnMainWindowActivated;

			_tokenManager?.Dispose();

			//--- remove from status bar ---
			JoinableTaskFactory.Run(async () =>
			{
				await JoinableTaskFactory.SwitchToMainThreadAsync();

				if (FindStatusBar() is StatusBar statusBar)
				{
					for (int i = statusBar.Items.Count - 1; i >= 0; i--)
					{
						if (statusBar.Items[i] is StatusBarItem item && item.Content == _statusControl)
						{
							statusBar.Items.RemoveAt(i);
							break;
						}
					}
				}
			});
		}

		base.Dispose(disposing);
	}

	#endregion
}
