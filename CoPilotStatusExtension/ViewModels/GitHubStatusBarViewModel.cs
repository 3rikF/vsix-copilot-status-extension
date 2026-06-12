
// ignore spelling: bindable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

using CoPilotStatusExtension.GitHubApiModels;
using CoPilotStatusExtension.Models;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.ViewModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
public class GitHubStatusBarViewModel : INotifyPropertyChanged
{
	//-----------------------------------------------------------------------------------------------------------------
	#region Fields

	private const string EXTENSION_NAME = "Copilot Status Extension";

	private string _statusText		= string.Empty;

	private CopilotUserInfo?		_basicCopilotUserInfo;
	private CopilotBillingUsage?	_githubBillingUsage;
	private CopilotQuotaResponse?	_personalQuota;
	private RateLimitInfo?			_apiRateLimit;

	#endregion Fields

	//-----------------------------------------------------------------------------------------------------------------
	#region UI Bindable Properties

	public bool HasValidUser
		=> !string.IsNullOrWhiteSpace(_personalQuota?.Login ?? _basicCopilotUserInfo?.Username);

	public bool HasValidToken
		=> !string.IsNullOrWhiteSpace(_personalQuota?.Login);

	/// <summary>
	/// The text shown in the status  bar item.
	/// Contains either the username + percentage or a status like "Not Signed In".
	/// </summary>
	public string StatusText
		=> _statusText;
	public string ExtensionNameAndVersion
		{ get;} = GetExtensionNameAndVersion();

	/// <summary>
	/// Premium interactions used as a fraction in the range [0, 1] (null if unavailable).
	/// </summary>
	public double? PremiumInteractionsUsedPercent
		=> _personalQuota?.QuotaSnapshots?.PremiumInteractions?.PercentUsed;

	public IEnumerable<QuotaDetail> QuotaDetails
	{
		get
		{
			if (_personalQuota?.QuotaSnapshots is null)
				yield break;

			if (_personalQuota.QuotaSnapshots.PremiumInteractions is not null)
				yield return _personalQuota.QuotaSnapshots.PremiumInteractions;

			if (_personalQuota.QuotaSnapshots.Chat is not null)
				yield return _personalQuota.QuotaSnapshots.Chat;

			if (_personalQuota.QuotaSnapshots.Completions is not null)
				yield return _personalQuota.QuotaSnapshots.Completions;
		}
	}

	public CopilotUserInfo? AccountInfo
		=> _basicCopilotUserInfo;

	public CopilotQuotaResponse? PersonalInfo
		=> _personalQuota;

	public RateLimitInfo? ApiRateLimitInfo
		=> _apiRateLimit;

	public string? UserProfileUrl
	{
		get
		{
			string? login = _personalQuota?.Login;
			return string.IsNullOrEmpty(login)
				? null
				: $"https://github.com/{login}";
		}
	}

	#endregion UI Bindable Properties

	//-----------------------------------------------------------------------------------------------------------------
	#region Methods

	public void SetData(CopilotUserInfo? copilotUserInfo, CopilotBillingUsage? billingUsage, CopilotQuotaResponse? personalQuota, RateLimitInfo? apiRateLimit)
	{
		_basicCopilotUserInfo	= copilotUserInfo;
		_githubBillingUsage		= billingUsage;
		_personalQuota			= personalQuota;
		_apiRateLimit			= apiRateLimit;

		_statusText				= GetStatusText();

		RaiseAllPropertyChanged();
	}

	private string GetStatusText()
	{
		StringBuilder sb = new();

		//-------------------------------------------------
		string? userName = _personalQuota?.Login
			?? _basicCopilotUserInfo?.Username;

		if (!string.IsNullOrEmpty(userName))
			_ = sb.Append(userName);

		//-------------------------------------------------
		else if (_basicCopilotUserInfo?.Status?.Equals("NotSignedInToGitHub", StringComparison.OrdinalIgnoreCase) == true)
			_ = sb.Append("Not signed in");
		else
			_ = sb.Append("Status not available");

		//-------------------------------------------------
		if (PremiumInteractionsUsedPercent is not null)
			_ = sb.Append($": {PremiumInteractionsUsedPercent:P1}");

		//-------------------------------------------------
		return sb.ToString();
	}

	private static string GetExtensionNameAndVersion()
	{
		try
		{
			Assembly assembly = typeof(GitHubStatusBarViewModel).Assembly;
			string resourceName = assembly.GetManifestResourceNames()
				.FirstOrDefault(r => r.EndsWith("source.extension.vsixmanifest"));

			if (resourceName is null)
				return EXTENSION_NAME;

			using Stream stream = assembly.GetManifestResourceStream(resourceName);
			if (stream is null)
				return EXTENSION_NAME;

			XDocument doc = XDocument.Load(stream);
			XNamespace ns = "http://schemas.microsoft.com/developer/vsx-schema/2011";

			string? version = doc.Root?
				.Element(ns + "Metadata")?
				.Element(ns + "Identity")?
				.Attribute("Version")?
				.Value;

			return !string.IsNullOrEmpty(version)
				? $"{EXTENSION_NAME} v{version}"
				: EXTENSION_NAME;
		}
		catch
		{
			return EXTENSION_NAME;
		}
	}

	#endregion Methods

	//-----------------------------------------------------------------------------------------------------------------
	#region INotifyPropertyChanged

	protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
	{
		if (Equals(field, newValue))
			return false;

		else
		{
			field = newValue;
			RaisePropertyChanged(propertyName);

			return true;
		}
	}

	protected bool SetProperty<T>(ref T field, T newValue, params string[] additionalDependentProperties)
	{
		if (Equals(field, newValue))
			return false;

		else
		{
			field = newValue;

			foreach (string dependentProperty in additionalDependentProperties)
				RaisePropertyChanged(dependentProperty);

			return true;
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

	protected virtual void RaiseAllPropertyChanged()
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

	#endregion
}
