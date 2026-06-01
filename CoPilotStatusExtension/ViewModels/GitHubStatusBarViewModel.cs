
// ignore spelling: bindable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

using CoPilotStatusExtension.GitHubApiModels;
using CoPilotStatusExtension.Models;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.ViewModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
public class GitHubStatusBarViewModel : INotifyPropertyChanged
{
	//-----------------------------------------------------------------------------------------------------------------
	#region Fields

	private string _statusText		= string.Empty;

	private CopilotUserInfo?		_basicCopilotUserInfo;
	private CopilotBillingUsage?	_githubBillingUsage;
	private CopilotQuotaResponse?	_personalQuota;
	private RateLimitInfo?			_apiRateLimit;

	#endregion Fields

	//-----------------------------------------------------------------------------------------------------------------
	#region UI Bindable Properties

	public double? PremiumInteractionsUsedPercent
	{
		get
		{
			double? remaining	= _personalQuota?.QuotaSnapshots?.PremiumInteractions?.QuotaRemaining;
			double? entitled	= _personalQuota?.QuotaSnapshots?.PremiumInteractions?.Entitlement;

			return remaining is null || entitled is null
					? null
					: 1 - (remaining / entitled);
		}
	}

	public string? CurrentStatus
		=> _basicCopilotUserInfo?.Status;

	public string StatusText
		=> _statusText;

	public string StatusToolTip
		{ get;} = GetExtensionNameAndVersion();

	public IEnumerable<QuotaDetail> QuotaDetails
	{
		get
		{
			if (_personalQuota?.QuotaSnapshots is null)
				yield break;

			if (_personalQuota.QuotaSnapshots.Chat is not null)
				yield return _personalQuota.QuotaSnapshots.Chat;

			if (_personalQuota.QuotaSnapshots.Completions is not null)
				yield return _personalQuota.QuotaSnapshots.Completions;

			if (_personalQuota.QuotaSnapshots.PremiumInteractions is not null)
				yield return _personalQuota.QuotaSnapshots.PremiumInteractions;
		}
	}

	public CopilotUserInfo AccountInfo
		=> _basicCopilotUserInfo;

	public CopilotQuotaResponse? PersonalInfo
		=> _personalQuota;

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
		Version? version = typeof(GitHubStatusBarViewModel).Assembly.GetName().Version;
		return version is not null
			? $"Copilot Status Extension v{version}"
			: "Copilot Status Extension";
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
