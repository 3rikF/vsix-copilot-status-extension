
// ignore spelling: mef

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Composition;
using Microsoft.VisualStudio.Threading;

using ExportProvider = System.ComponentModel.Composition.Hosting.ExportProvider;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Models;

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class CoPilotTokenManager(IComponentModel? componentModel) : IDisposable
{
	//-----------------------------------------------------------------------------------------------------------------
	#region Fields

	private readonly IComponentModel? _componentModel	= componentModel;

	private object? _copilotTokenManager				= null;
	private EventHandler? _updateStatsEventHandler		= null;

	#endregion Fields
	#region Construction

	#endregion Construction

	//-----------------------------------------------------------------------------------------------------------------
	#region Private Methods

	private EventInfo? GetCopilotIdentityChangedEvent()
		=> _copilotTokenManager?.GetType().GetEvent("CopilotIdentityChanged", BindingFlags.Public | BindingFlags.Instance);

	private object? GetAuthInfoObject()
	{
		if (_copilotTokenManager is null)
			return null;

		MethodInfo method = _copilotTokenManager.GetType().GetMethod("GetAuthInfo", BindingFlags.Public | BindingFlags.Instance);
		return method?.Invoke(_copilotTokenManager, /*forceRefresh*/ [false]);
	}

	private static T? GetProperty<T>(object? obj, string propertyName)
	{
		PropertyInfo? prop = obj?.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
		return prop?.GetValue(obj) is T value ? value : default;
	}

	#endregion Private Methods

	//-----------------------------------------------------------------------------------------------------------------
	#region Public Methods

	public void InitializeCopilotMef(EventHandler updateStatsEventHandler)
	{
		try
		{
			if (_componentModel is null)
				return;

			ExportProvider exportProvider = _componentModel.DefaultExportProvider;

			//--- Get the ICopilotTokenManager MEF component from Copilot's Conversations.Abstractions assembly ---
			_copilotTokenManager = exportProvider.GetExportedValueOrDefault<object>("Conversations.Abstractions.ICopilotTokenManager");
			if (_copilotTokenManager is null)
			{
				Debug.WriteLine("ICopilotTokenManager could not be resolved from MEF.");
				return;
			}

			//--- Subscribe to CopilotIdentityChanged so the status bar updates on login/logout ---
			_updateStatsEventHandler = updateStatsEventHandler;

			GetCopilotIdentityChangedEvent()
				?.AddEventHandler(_copilotTokenManager, _updateStatsEventHandler);
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Error initializing Copilot MEF components: [{ex.Message}]");
		}
	}

	public CopilotUserInfo? GetCopilotUserInfo()
	{
		object? authInfo = GetAuthInfoObject();

		if (authInfo is null)
			return null;

		//-------------------------------------------------------------------------------------
		CopilotUserInfo status = new ()
		{
			Status							= GetProperty<object>(authInfo, "Status")?.ToString()	?? "STATUS_NA",

			Username						= GetProperty<string>(authInfo, "GitHubUsername")		?? string.Empty,
			AccessToken						= GetProperty<string>(authInfo, "GitHubToken")			?? string.Empty,
			SubscriptionType				= GetProperty<string>(authInfo, "SubscriptionType")		?? string.Empty,

			IsIndividual					= GetProperty<bool?>(authInfo, "IsIndividual"),
			IsEnterprise					= GetProperty<bool?>(authInfo, "IsEnterprise"),

			ChatEnabled						= GetProperty<bool?>(authInfo, "ChatEnabled"),
			CompletionsEnabled				= GetProperty<bool?>(authInfo, "CompletionsEnabled"),
			McpEnabledByToken				= GetProperty<bool?>(authInfo, "McpEnabledByToken"),

			EditorPreviewFeaturesEnabled	= GetProperty<bool?>(authInfo, "EditorPreviewFeaturesEnabled"),
			CopilotExclusion				= GetProperty<bool?>(authInfo, "CopilotExclusion"),
		};

		return status;
	}

	internal string[] DEBUG_GetAllCopilotMefInstances()
	{
		try
		{
			if (_componentModel is null)
				return [];

			ExportProvider exportProvider	= _componentModel.DefaultExportProvider;

			//--- retrieve the private [exportProvider] field from [exportProvider] ---
			FieldInfo? privateExportProviderInfo	= exportProvider.GetType().GetField("exportProvider", BindingFlags.NonPublic | BindingFlags.Instance);
			object? privateExportProvider			= privateExportProviderInfo?.GetValue(exportProvider);

			//--- retrieve the private [composition] field from [privateExportProvider] ---
			FieldInfo? compositionFieldInfo			= privateExportProvider?.GetType().GetField("composition", BindingFlags.NonPublic | BindingFlags.Instance);
			object? composition						= compositionFieldInfo?.GetValue(privateExportProvider);

			//--- retrieve the private [resolver] field from [composition] ---
			PropertyInfo? resolverPropertyInfo		= composition?.GetType().GetProperty("Resolver", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			Resolver? resolver						= resolverPropertyInfo?.GetValue(composition) as Resolver;

			//--- [composition] is a RuntimeComposition – enumerate all exported contract names containing "copilot" ---
			if (composition is RuntimeComposition runtimeComposition)
			{
				return runtimeComposition
					.Parts
					.SelectMany(p => p.Exports)
					.Where(e => e.ContractName.Contains("copilot", StringComparison.OrdinalIgnoreCase))
					.Select(e => e.ContractName)
					.Distinct()
					.OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
					.ToArray();
			}
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex.ToString());
		}

		return [];
	}

	internal void DEBUG_TestInitializes()
	{
		//ExportProvider exportProvider = _componentModel!.DefaultExportProvider;
		//object? blah1 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.Common.AuthManager");
		//object? blah2 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.Common.AzureDevOpsTokenManager");
		//object? blah3 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.Common.CopilotTokenManagerImpl");
		//object? blah4 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.Common.FreeCopilotManager");
		//object? blah5 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.Common.GitHubTokenManager");
		//object? blah6 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.CopilotSessionProvider");
		//object? blah7 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.WebTools.Scaffolding.Core.Copilot.ICopilotChatService");
		////object? blah8 = exportProvider.GetExportedValueOrDefault<object>("Conversations.Shared.Options.IMarshaledCopilotOptions");
		//try
		//{
		//	object? blah9 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.DiagnosticsHub.VisualStudio.DataWarehouse.Copilot.DiagnosticsHubFunctionProvider");
		//	object? blah10 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.DiagnosticsHub.VisualStudio.DataWarehouse.Copilot.PerformanceProfilerActivationTools");
		//}
		//catch (Exception ex)
		//{ }
		//
		//// !
		//object? blah11 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.CopilotInteractionManager");
		//// !
		//object? blah12 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.Core.Agents.PromptDateBuilder");
		//
		//
		//object? blah13 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.Core.GitHubTelemetry.IGitHubTelemetryClient");
		//object? blah14 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.Core.GitHubTelemetry.ITelemetryConfigurationProvider");
		//object? blah15 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.Core.HostEnvironment.ICopilotHostEnvironment");
		//object? blah16 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.Core.HttpClientConfiguration.IMachineIdProvider");
		//object? blah17 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.Core.ICopilotAgentsActivationMonitor");
		//object? blah18 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.ICopilotTokenCounter");
		//object? blah19 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.UI.ChatMefPartAccessor");
		//object? blah20 = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.UI.IViewModelServices");
		//object? blah21oteote = exportProvider.GetExportedValueOrDefault<object>("Microsoft.VisualStudio.Copilot.UI.Mcp.Authentication.IMcpAuthViewModelProvider");
	}

	#endregion Public Methods

	//-----------------------------------------------------------------------------------------------------------------
	#region IDisposable

	public void Dispose()
	{
		if (_copilotTokenManager is not null && _updateStatsEventHandler is not null)
		{
			//--- determines the info-object and then uses [_cpTokenManager] as parent to remove the event handler ---
			GetCopilotIdentityChangedEvent()
				?.RemoveEventHandler(_copilotTokenManager, _updateStatsEventHandler);
		}
	}

	#endregion IDisposable
}
