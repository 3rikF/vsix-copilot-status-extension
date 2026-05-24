
using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Reflection;

using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Threading;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension;

//-----------------------------------------------------------------------------------------------------------------------------------------
public sealed class CoPilotTokenManager : IDisposable
{
	//-----------------------------------------------------------------------------------------------------------------
	#region Fields

	private object? _cpTokenManager;
	private EventHandler? _cpIdentityChangedHandler;
	private readonly IComponentModel? _componentModel;
	private readonly JoinableTaskFactory _joinableTaskFactory;

	#endregion Fields

	//-----------------------------------------------------------------------------------------------------------------
	#region Construction

	public CoPilotTokenManager(IComponentModel? componentModel, JoinableTaskFactory joinableTaskFactor)
	{
		_componentModel			= componentModel;
		_joinableTaskFactory		= joinableTaskFactor;
	}

	#endregion Construction

	//-----------------------------------------------------------------------------------------------------------------
	#region Private Methods

	private EventInfo? GetCopilotIdentityChangedEvent()
		=> _cpTokenManager?.GetType().GetEvent("CopilotIdentityChanged", BindingFlags.Public | BindingFlags.Instance);

	private object? GetAuthInfoObject()
	{
		if (_cpTokenManager is null)
			return null;

		MethodInfo method = _cpTokenManager.GetType().GetMethod("GetAuthInfo", BindingFlags.Public | BindingFlags.Instance);
		return method?.Invoke(_cpTokenManager, /*forceRefresh*/ [false]);
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

			ExportProvider exportProvider	= _componentModel.DefaultExportProvider;

			//--- Get the ICopilotTokenManager MEF component from Copilot's Conversations.Abstractions assembly ---
			_cpTokenManager = exportProvider.GetExportedValueOrDefault<object>("Conversations.Abstractions.ICopilotTokenManager");
			if (_cpTokenManager is null)
			{
				Debug.WriteLine("ICopilotTokenManager could not be resolved from MEF.");
				return;
			}

			//--- Subscribe to CopilotIdentityChanged so the status bar updates on login/logout ---
			EventInfo? copilotIdentityChangedEventInfo = GetCopilotIdentityChangedEvent();

			if (copilotIdentityChangedEventInfo is not null)
				copilotIdentityChangedEventInfo.AddEventHandler(_cpTokenManager, updateStatsEventHandler);
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Error initializing Copilot MEF components: [{ex.Message}]");
		}
	}

	public GitHubStatusData? GetCurrentStatus()
	{
		object? authInfo = GetAuthInfoObject();

		if (authInfo is null)
			return null;

		//-------------------------------------------------------------------------------------
		GitHubStatusData status = new ()
		{
			Status							= GetProperty<object>(authInfo, "Status")?.ToString()		?? "STATUS_NA",
			IsIndividual					= GetProperty<bool?>(authInfo, "IsIndividual"),
			IsEnterprise					= GetProperty<bool?>(authInfo, "IsEnterprise"),

			ChatEnabled						= GetProperty<bool?>(authInfo, "ChatEnabled"),
			CompletionsEnabled				= GetProperty<bool?>(authInfo, "CompletionsEnabled"),
			EditorPreviewFeaturesEnabled	= GetProperty<bool?>(authInfo, "EditorPreviewFeaturesEnabled"),
			McpEnabledByToken				= GetProperty<bool?>(authInfo, "McpEnabledByToken"),

			GitHubUsername					= GetProperty<string>(authInfo, "GitHubUsername")			?? string.Empty,
			GitHubPassword					= GetProperty<string>(authInfo, "GitHubToken")				?? string.Empty,
			SubscriptionType				= GetProperty<string>(authInfo, "SubscriptionType")		?? string.Empty,

			AnnotationsEnabled				= GetProperty<bool?>(authInfo, "AnnotationsEnabled"),
			CodeQuoteEnabled				= GetProperty<bool?>(authInfo, "CodeQuoteEnabled"),
			ChatJetbrainsEnabled			= GetProperty<bool?>(authInfo, "ChatJetbrainsEnabled"),
			CopilotExclusion				= GetProperty<bool?>(authInfo, "CopilotExclusion"),
		};

		if (GetProperty<object>(authInfo, "TokenEnvelope") is object tokenEnvelope)
		{

			status.AnnotationsEnabled		= GetProperty<bool?>(tokenEnvelope, "AnnotationsEnabled");
			status.ChatJetbrainsEnabled		= GetProperty<bool?>(tokenEnvelope, "ChatJetbrainsEnabled");
			status.CodeQuoteEnabled			= GetProperty<bool?>(tokenEnvelope, "CodeQuoteEnabled");
			status.CopilotExclusionEnabled	= GetProperty<bool?>(tokenEnvelope, "CopilotExclusionEnabled");
			status.CopilotExclusion			= GetProperty<bool?>(tokenEnvelope, "CopilotExclusion");
			status.ErrorDetails				= GetProperty<object>(tokenEnvelope, "ErrorDetails");
			long expiresAt					= GetProperty<long>(tokenEnvelope, "ExpiresAt");
			status.ExpiresAt				= DateTimeOffset.FromUnixTimeSeconds(expiresAt).ToLocalTime().DateTime;
		}

		return status;
	}

	public void Test()
	{
		try
		{
			if (_componentModel is null)
				return;

			ExportProvider exportProvider	= _componentModel.DefaultExportProvider;

			//--- retrieve the private [exportProvider] field from [exportProvider] ---
			FieldInfo? privateExportProviderInfo	= exportProvider.GetType().GetField("exportProvider", BindingFlags.NonPublic | BindingFlags.Instance);
			object? privateExportProvider			= privateExportProviderInfo?.GetValue(exportProvider);

			//--- retrieve the private [composition] field from [privateExportProvider] ---
			FieldInfo? compositionFieldInfo			= privateExportProvider?.GetType().GetField("composition", BindingFlags.NonPublic | BindingFlags.Instance);
			object? composition						= compositionFieldInfo?.GetValue(privateExportProvider);


		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex.ToString());
		}
	}

	#endregion Public Methods

	//-----------------------------------------------------------------------------------------------------------------
	#region IDisposable

	public void Dispose()
	{
		if (_cpTokenManager is not null && _cpIdentityChangedHandler is not null)
		{
			//--- determines the info-object and then uses [_cpTokenManager] as parent to remove the event handler ---
			GetCopilotIdentityChangedEvent()
				?.RemoveEventHandler(_cpTokenManager, _cpIdentityChangedHandler);
		}
	}

	#endregion IDisposable
}
