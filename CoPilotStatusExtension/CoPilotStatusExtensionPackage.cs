
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

using CoPilotStatusExtension.GitHubApiModels;
using CoPilotStatusExtension.Helper;
using CoPilotStatusExtension.Models;
using CoPilotStatusExtension.Views;

using Microsoft.VisualStudio;
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
[Guid(CoPilotStatusExtensionPackage.PACKAGE_GUID_STRING)]
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string,					PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string,				PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string,				PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(VSConstants.UICONTEXT.EmptySolution_string,				PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string,	PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string,		PackageAutoLoadFlags.BackgroundLoad)]
//--- do not auto-load when ---
//[ProvideAutoLoad(VSConstants.UICONTEXT.DesignMode_string,					PackageAutoLoadFlags.BackgroundLoad)]
[SuppressMessage("Usage", "VSTHRD101:Avoid unsupported async delegates", Justification = "<Pending>")]
public sealed class CoPilotStatusExtensionPackage : AsyncPackage
{
	//-----------------------------------------------------------------------------------------------------------------
	#region Fields

	public const string EXTENSION_NAME		= "Copilot Status Extension";
	public const string PACKAGE_GUID_STRING	= "86cc1277-85e2-4030-ba7f-ceb48452ad40";

	private Timer					_refreshTimer	= null!;
	private GitHubStatusBarControl	_statusControl	= null!;
	private GitHubApiService		_gitHubService	= null!;
	private CoPilotTokenManager?	_tokenManager	= null;

	private readonly SemaphoreSlim _semaphore		= new(1, 1);

	#endregion Fields

	//-----------------------------------------------------------------------------------------------------------------
	#region Visual Tree Helper

	/// <summary>
	/// Classic helper method to find a child of a given type in the visual tree of a WPF application.
	/// </summary>
	/// <typeparam name="T">The type of the child to find.</typeparam>
	/// <param name="parent">The parent dependency object.</param>
	/// <returns>The first child of the specified type, or null if none is found.</returns>
	private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
	{
		if (parent is null)
			return null;

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
	/// Initialization of the package; this method is called right after the package is sited,
	/// so this is the place where you can put all the initialization code that rely on services provided by VisualStudio.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
	/// <param name="progress">A provider for progress updates.</param>
	/// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		await ExtensionLogger.LogAsync($"Initializing {EXTENSION_NAME} v{VersionHelper.GetExtensionVersion("?.?.?")}");

		await base.InitializeAsync(cancellationToken, progress);
		await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

		await TryInjectingStatusBarItemAsync();
	}

	private async Task TryInjectingStatusBarItemAsync()
	{
		if (Application.Current?.MainWindow is { } mainWindow)
		{
			await ExtensionLogger.LogAsync("Main window available");

			RegisterClosedHandler(mainWindow);

			//--- Case 1: Main window already exists and is loaded ---
			if (mainWindow.IsLoaded)
			{
				await ExtensionLogger.LogAsync("Main window loaded");
				await AddStatusBarControlAsync(mainWindow);
			}

			//--- Case 2: Main window exists but is not yet loaded ---
			else
			{
				await ExtensionLogger.LogAsync("Register for [Loaded] event");
				RegisterLoadedHandler(mainWindow);
			}
		}

		//--- Case 3: Main window is still null (startup window is open) ---
		// We wait until a window becomes active in WPF:
		else
		{
			await ExtensionLogger.LogAsync("Main window not available: Register for [Activated] event");

			EventHandler tmpActivatedHandler = null!;
			tmpActivatedHandler = async (s, e) =>
			{
				try
				{
					//--- receiving activated event when the main window is still null sounds kinda impossible ... but who knows ---
					if (Application.Current?.MainWindow is { } mainWindow)
					{
						Application.Current.Activated -= tmpActivatedHandler;

						if (mainWindow.IsLoaded)
						{
							await ExtensionLogger.LogAsync("Activated: Main window loaded");
							await AddStatusBarControlAsync(mainWindow);
						}
						else
						{
							await ExtensionLogger.LogAsync("Activated: Register for [Loaded] event");
							RegisterLoadedHandler(mainWindow);
						}
					}
				}
				catch (Exception ex)
				{
					await ExtensionLogger.LogAsync(ex.ToString());
				}
			};

			Application.Current?.Activated += tmpActivatedHandler;
		}
	}

	/// <summary>Cleans up the status bar control when the main window is closed.</summary>
	/// <param name="mainWindow">The main window of the application.</param>
	private void RegisterClosedHandler(Window mainWindow)
	{
		EventHandler tmpClosedHandler	= null!;
		tmpClosedHandler				= (s, e) =>
		{
			mainWindow.Closed		-= tmpClosedHandler;
			mainWindow.Activated	-= HandleRefreshGitHubStatus;

			// Remove the control from status bar here
			if (FindChild<StatusBar>(mainWindow) is { } statusBar)
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
		};

		mainWindow.Closed += tmpClosedHandler;
	}

	/// <summary>
	/// Registers a handler for the Loaded event of the main window.
	/// When the main window is loaded, it injects the status bar control.
	/// </summary>
	/// <remarks>
	/// This method is only called when the main window exists but is not yet loaded.
	/// It ensures that the status bar control is injected once the main window is ready.
	/// </remarks>
	/// <param name="mainWindow">The main window of the application.</param>
	private void RegisterLoadedHandler(Window mainWindow)
	{
		RoutedEventHandler tmpLoadedHandler = null!;
		tmpLoadedHandler = async (s, e) =>
		{
			mainWindow.Loaded -= tmpLoadedHandler;
			await AddStatusBarControlAsync(mainWindow);
		};

		mainWindow.Loaded += tmpLoadedHandler;
	}

	/// <summary>
	/// Creates and adds a <see cref="StatusBarItem"/> showing all the
	/// cool GitHub Copilot information to the status bar of the main window.
	/// </summary>
	/// <param name="mainWindow">The main window of the application.</param>
	private async Task AddStatusBarControlAsync(Window mainWindow)
	{
		if (FindChild<StatusBar>(mainWindow) is not { } statusBar)
		{
			await ExtensionLogger.LogAsync("Could not find StatusBar in the visual tree of the main window: This is the end of the world as you know it");
			return;
		}

		await ExtensionLogger.LogAsync("StatusBar found: Adding status bar item");

		_statusControl		= new GitHubStatusBarControl();
		StatusBarItem item	= new ()
		{
			Content						= _statusControl,
			HorizontalAlignment			= HorizontalAlignment.Right,
			Padding						= new Thickness(0),
			VerticalContentAlignment	= VerticalAlignment.Stretch,
			HorizontalContentAlignment	= HorizontalAlignment.Stretch,
		};

		DockPanel.SetDock(item, Dock.Right);
		_ = statusBar.Items.Add(item);

		//---- Initialize GitHub MEF --------------------------------------------------------------
		_gitHubService	= new GitHubApiService();

		if (GetGlobalService(typeof(SComponentModel)) is IComponentModel componentModel)
		{
			_tokenManager = new CoPilotTokenManager(componentModel);

			//--- initialize MEF end register [CopilotIdentityChanged] event-handler ---
			_tokenManager.InitializeCopilotMef(HandleRefreshGitHubStatus);

			//--- refresh when the VS window regains focus ---
			Application.Current.MainWindow.Activated += HandleRefreshGitHubStatus;

			//--- refresh after 60s fallback timer ---
			_refreshTimer = new Timer( _ => _ = JoinableTaskFactory.RunAsync(RefreshGitHubStatusAsync), null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
		}
	}

	private void HandleRefreshGitHubStatus(object? sender, EventArgs? e)
	{
		if (_tokenManager is null)
			return;

		_ = JoinableTaskFactory.RunAsync(RefreshGitHubStatusAsync);
	}

	private async Task RefreshGitHubStatusAsync()
	{
		if (_tokenManager is null)
			return;

		if (!await _semaphore.WaitAsync(0))  // 0ms = non-blocking TryEnter-Äquivalent
			return;

		try
		{
			//---- determine CoPilot username and access-token ------------------------------------
			CopilotUserInfo? copilotInfo	= _tokenManager.GetCopilotUserInfo();

			if (copilotInfo is null || string.IsNullOrEmpty(copilotInfo.Username) || string.IsNullOrEmpty(copilotInfo.AccessToken))
			{
				await JoinableTaskFactory.SwitchToMainThreadAsync();
				_statusControl.SetData(copilotInfo, null, null, null);

				return;
			}

			//---- fetch GitHub API data ----------------------------------------------------------

			//---- Billing ----------------------------------------------------
			(int billingStatusCode, string billingReasonPhrase, CopilotBillingUsage? billingUsage) = (400, string.Empty, null);
			//(int billingStatusCode, string billingReasonPhrase, CopilotBillingUsage? billingUsage)
			//	= await _gitHubService
			//		.FetchUserBillingUsageAsync(copilotInfo.Username, copilotInfo.AccessToken)
			//		.ConfigureAwait(false);

			//--- reset all double percentage values from [0-100] to [0-1] for easier binding in the UI ---
			if (billingUsage is not null)
			{
				billingUsage.TotalNetAmount			/= 100;
				billingUsage.TotalQuantity			/= 100;
				billingUsage.TotalIncludedQuantity	/= 100;
				billingUsage.TotalOverageQuantity	/= 100;
			}

			//---- Personal Metrics -------------------------------------------
			(int chatStatusCode, string chatReasonPhrase, CopilotQuotaResponse? personalQuota, RateLimitInfo? apiRateLimit) = await _gitHubService
				.FetchUserChatUsageAsync(copilotInfo.AccessToken)
				.ConfigureAwait(false);

			//--- reset all double percentage values from [0-100] to [0-1] for easier binding in the UI ---
			if (personalQuota?.QuotaSnapshots is not null)
			{
				personalQuota.QuotaSnapshots.Chat.PercentRemaining					/= 100;
				personalQuota.QuotaSnapshots.Completions.PercentRemaining			/= 100;
				personalQuota.QuotaSnapshots.PremiumInteractions.PercentRemaining	/= 100;
			}

			//---- Update Status UI -------------------------------------------
			await JoinableTaskFactory.SwitchToMainThreadAsync();
			_statusControl.SetData(copilotInfo, billingUsage, personalQuota, apiRateLimit);
		}
		catch (Exception ex)
		{
			await ExtensionLogger.LogAsync($"{ex.GetType().Name} in [{nameof(RefreshGitHubStatusAsync)}]");
			await ExtensionLogger.LogAsync(ex.ToString());
		}
		finally
		{
			_ = _semaphore.Release();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_refreshTimer?.Dispose();
			_tokenManager?.Dispose();
		}

		base.Dispose(disposing);
	}

	#endregion
}
