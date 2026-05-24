using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSIXTestProject
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
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
	[Guid(VSIXTestProjectPackage.PACKAGE_GUID_STRING)]
	public sealed class VSIXTestProjectPackage : AsyncPackage
	{
		//-----------------------------------------------------------------------------------------------------------------
		#region Fields

		/// <summary>
		/// VSIXTestProjectPackage GUID string.
		/// </summary>
		public const string PACKAGE_GUID_STRING = "5986d7de-476a-450c-a8cb-078e728df1fc";

		private Timer					_refreshTimer				= null!;
		private GitHubStatusBarControl	_statusControl				= null!;
		private GitHubApiService		_gitHubService				= null!;
		private object?					_cpTokenManager;
		private EventHandler?			_cpIdentityChangedHandler;

		#endregion Fields

		//-----------------------------------------------------------------------------------------------------------------
		#region Visual Tree Helper

		private static StatusBar? FindStatusBar()
		{
			if (Application.Current?.MainWindow is not Window mainWindow)
				return null;

			return FindChild<StatusBar>(mainWindow);
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
			// When initialized asynchronously, the current thread may be a background thread at this point.
			// Do any initialization that requires the UI thread after switching to the UI thread.
			await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

			if (FindStatusBar() is not StatusBar statusBar)
				return;

			_gitHubService	= new GitHubApiService();
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

			//--- trigger-based: refresh when the VS window regains focus ---
			Application.Current.MainWindow.Activated += OnMainWindowActivated;

			//--- 60s fallback timer ---
			_refreshTimer = new Timer(
				_ => {_ = JoinableTaskFactory.RunAsync(RefreshGitHubStatusAsync);}
				, null
				, TimeSpan.Zero
				, TimeSpan.FromSeconds(60));

			InitializeCopilotMef();
		}

		private void InitializeCopilotMef()
		{
			try
			{
				if (GetGlobalService(typeof(SComponentModel)) is not IComponentModel componentModel)
					return;

				var exportProvider = componentModel.DefaultExportProvider;

				_cpTokenManager = exportProvider.GetExportedValueOrDefault<object>("Conversations.Abstractions.ICopilotTokenManager");

				if (_cpTokenManager is null)
				{
					Debug.WriteLine("ICopilotTokenManager could not be resolved from MEF.");
					return;
				}

				// Subscribe to CopilotIdentityChanged so the status bar updates on login/logout
				var changedEvent = _cpTokenManager.GetType().GetEvent("CopilotIdentityChanged",
					BindingFlags.Public | BindingFlags.Instance);

				if (changedEvent is not null)
				{
					_cpIdentityChangedHandler = (s, e) => _ = JoinableTaskFactory.RunAsync(RefreshGitHubStatusAsync);

					changedEvent.AddEventHandler(_cpTokenManager, _cpIdentityChangedHandler);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error initializing Copilot MEF components: {ex.Message}");
			}
		}

		private object? GetCopilotAuthInfo()
		{
			if (_cpTokenManager is null)
				return null;

			var method = _cpTokenManager.GetType()
				.GetMethod("GetAuthInfo", BindingFlags.Public | BindingFlags.Instance);

			return method?.Invoke(_cpTokenManager, new object[] { false });
		}

		private static T? ReadProperty<T>(object? obj, string propertyName)
		{
			if (obj is null)
				return default;

			var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
			return (T?)prop?.GetValue(obj);
		}

		private void OnMainWindowActivated(object sender, EventArgs e)
			=> _ = JoinableTaskFactory.RunAsync(RefreshGitHubStatusAsync);

		private GitHubStatusData? GetCurrentStatus()
		{
			object? authInfo = GetCopilotAuthInfo();

			if (authInfo is null)
				return null;

			//-------------------------------------------------------------------------------------
			GitHubStatusData status = new ()
			{
				Status							= ReadProperty<object>(authInfo, "Status")?.ToString()		?? "STATUS_NA",
				IsIndividual					= ReadProperty<bool?>(authInfo, "IsIndividual"),
				IsEnterprise					= ReadProperty<bool?>(authInfo, "IsEnterprise"),

				ChatEnabled						= ReadProperty<bool?>(authInfo, "ChatEnabled"),
				CompletionsEnabled				= ReadProperty<bool?>(authInfo, "CompletionsEnabled"),
				EditorPreviewFeaturesEnabled	= ReadProperty<bool?>(authInfo, "EditorPreviewFeaturesEnabled"),
				McpEnabledByToken				= ReadProperty<bool?>(authInfo, "McpEnabledByToken"),

				GitHubUsername					= ReadProperty<string>(authInfo, "GitHubUsername")			?? string.Empty,
				GitHubPassword					= ReadProperty<string>(authInfo, "GitHubToken")				?? string.Empty,
				SubscriptionType				= ReadProperty<string>(authInfo, "SubscriptionType")		?? string.Empty,

				AnnotationsEnabled				= ReadProperty<bool?>(authInfo, "AnnotationsEnabled"),
				CodeQuoteEnabled				= ReadProperty<bool?>(authInfo, "CodeQuoteEnabled"),
				ChatJetbrainsEnabled			= ReadProperty<bool?>(authInfo, "ChatJetbrainsEnabled"),
				CopilotExclusion				= ReadProperty<bool?>(authInfo, "CopilotExclusion"),
			};

			if (ReadProperty<object>(authInfo, "TokenEnvelope") is object tokenEnvelope)
			{

				status.AnnotationsEnabled		= ReadProperty<bool?>(tokenEnvelope, "AnnotationsEnabled");
				status.ChatJetbrainsEnabled		= ReadProperty<bool?>(tokenEnvelope, "ChatJetbrainsEnabled");
				status.CodeQuoteEnabled			= ReadProperty<bool?>(tokenEnvelope, "CodeQuoteEnabled");
				status.CopilotExclusionEnabled	= ReadProperty<bool?>(tokenEnvelope, "CopilotExclusionEnabled");
				status.CopilotExclusion			= ReadProperty<bool?>(tokenEnvelope, "CopilotExclusion");
				status.ErrorDetails				= ReadProperty<object>(tokenEnvelope, "ErrorDetails");
				long expiresAt					= ReadProperty<long>(tokenEnvelope, "ExpiresAt");
				status.ExpiresAt				= DateTimeOffset.FromUnixTimeSeconds(expiresAt).ToLocalTime().DateTime;
			}

			return status;
		}

		private async Task RefreshGitHubStatusAsync()
		{
			//if (!cpSignedIn)
			//{
			//	await JoinableTaskFactory.SwitchToMainThreadAsync();
			//	_statusControl.StatusText = "⚠ Not Signed In";
			//	return;
			//}

			// --- GitHub API rate limits ---
			//(string userName, string token) = await _gitHubService.GetTokenAsync();
			//
			//if (string.IsNullOrEmpty(token))
			//{
			//	await JoinableTaskFactory.SwitchToMainThreadAsync();
			//	string sub = string.IsNullOrEmpty(cpSubType) ? "" : $" ({cpSubType})";
			//	_statusControl.StatusText = $"🤖 {cpUsername}{sub}";
			//	return;
			//}

			//GitHubStatusData data = await _gitHubService.FetchStatusAsync(userName, token);

			await JoinableTaskFactory.SwitchToMainThreadAsync();

			//if (data.ErrorMessage != null)
			//{
			//	_statusControl.StatusText = $"⚠ GitHub: {data.ErrorMessage}";
			//	return;
			//}

			//string rateInfo = $"⚡ {data.CoreRemaining}/{data.CoreLimit} | 🔍 {data.SearchRemaining}/{data.SearchLimit}";
			//string copilotLabel = cpChatEnabled
			//	? $"🤖 {cpUsername} ({cpSubType})"
			//	: $"🤖 {cpUsername}";

			//-------------------------------------------------------------------------------------
			await JoinableTaskFactory.SwitchToMainThreadAsync();
			_statusControl.StatusData = GetCurrentStatus();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_refreshTimer?.Dispose();

				if (Application.Current?.MainWindow != null)
					Application.Current.MainWindow.Activated -= OnMainWindowActivated;

				// Unsubscribe Copilot identity change event
				if (_cpTokenManager is not null && _cpIdentityChangedHandler is not null)
				{
					var changedEvent = _cpTokenManager.GetType().GetEvent("CopilotIdentityChanged",
						BindingFlags.Public | BindingFlags.Instance);
					changedEvent?.RemoveEventHandler(_cpTokenManager, _cpIdentityChangedHandler);
				}

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
}
