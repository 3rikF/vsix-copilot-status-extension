
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
public enum EStatusEnum
{
	Unset,
	OK,
	NotSignedInToGitHub,
	Unhandled,
}

public enum ESubscriptionType
{
	Unknown,
	free_limited_quota,
	trial_subscriber_quota,
	yearly_subscriber_quota,
	copilot_enterprise_seat_quota,
	copilot_for_business_seat_quota,
}

//-----------------------------------------------------------------------------------------------------------------------------------------
public class GitHubStatusBarViewModel : INotifyPropertyChanged
{
	//-----------------------------------------------------------------------------------------------------------------
	#region Fields

	private string _statusText		= string.Empty;
	private string _statusToolTip	= string.Empty;

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

	public string StatusText
	{
		get => _statusText;
		private set => SetProperty(ref _statusText, value);
	}

	public string StatusToolTip
	{
		get => _statusToolTip;
		private set => SetProperty(ref _statusToolTip, value);
	}

	#endregion UI Bindable Properties

	//-----------------------------------------------------------------------------------------------------------------
	#region Static Methods

	private static EStatusEnum ParseStatus(string? status)
	{
		if (status is null || string.IsNullOrWhiteSpace(status))
			return EStatusEnum.Unset;

		else if (status.Equals("NotSignedInToGitHub", StringComparison.OrdinalIgnoreCase))
			return EStatusEnum.NotSignedInToGitHub;

		else if (status.Equals("OK", StringComparison.OrdinalIgnoreCase))
			return EStatusEnum.OK;

		else
			return EStatusEnum.Unhandled;
	}

	private static ESubscriptionType ParseSubscriptionType(string? subscriptionType)
	{
		return !string.IsNullOrWhiteSpace(subscriptionType) && Enum.TryParse<ESubscriptionType>(subscriptionType, ignoreCase: true, out ESubscriptionType result)
			? result
			: ESubscriptionType.Unknown;
	}

	private static StringBuilder GetQuotaDetailToolTip(string title, QuotaDetail detail)
	{
		if (detail.Unlimited)
		{
			return new StringBuilder()
				.AppendLine()
				.AppendLine($"{title}:")
				.AppendLine($"      Unlimited");
		}
		else
		{
			return new StringBuilder()
				.AppendLine()
				.AppendLine($"{title}:")
				.AppendLine($"      Used:			{100-detail.PercentRemaining:F1}%  ({detail.QuotaRemaining} / {detail.Entitlement} left)")
				.AppendLine($"      Overage:		{(detail.OveragePermitted ? $"{detail.OverageCount} extra" : "[Not Permitted]")}");
		}
	}

	private static string FormatBool(bool? value) => value switch
	{
		true  => "✔",
		false => "❌",
		null  => "❓",
	};

	#endregion Static Methods

	//-----------------------------------------------------------------------------------------------------------------
	#region Methods

	public void SetData(CopilotUserInfo? copilotUserInfo, CopilotBillingUsage? billingUsage, CopilotQuotaResponse? personalQuota, RateLimitInfo? apiRateLimit)
	{
		_basicCopilotUserInfo	= copilotUserInfo;
		_githubBillingUsage		= billingUsage;
		_personalQuota			= personalQuota;
		_apiRateLimit			= apiRateLimit;

		RaiseAllPropertyChanged();

		UpdateStatusText();
		UpdateToolTip();
	}

	private void UpdateStatusText()
	{
		StringBuilder sb = new();

		string userName = _personalQuota?.Login
			?? _basicCopilotUserInfo?.Username
			?? "n/a";

		_ = sb.Append(ParseStatus(_basicCopilotUserInfo?.Status) switch
		{
			EStatusEnum.OK					=> userName,
			EStatusEnum.Unset				=> "Status not available",
			EStatusEnum.NotSignedInToGitHub	=> "Not signed in",
			EStatusEnum.Unhandled			=> $"Unhandled status [{_basicCopilotUserInfo?.Status}]",
			_								=> "Unknown status"
		});

		if (PremiumInteractionsUsedPercent is not null)
			_ = sb.Append($": {PremiumInteractionsUsedPercent:P1}");

		StatusText = sb.ToString();
	}

	private void UpdateToolTip()
	{
		StringBuilder sb = new();

		//--- helper ----------------------------------------------------------
		IEnumerable<string> EnumerateAccountTypes()
		{
			if (_basicCopilotUserInfo?.IsEnterprise == true)
				yield return "Enterprise";
			if (_basicCopilotUserInfo?.IsIndividual == true)
				yield return "Individual";
		}

		string subscriptionType = ParseSubscriptionType(_basicCopilotUserInfo?.SubscriptionType) switch
		{
			ESubscriptionType.free_limited_quota			=> "Free (limited quota)",
			ESubscriptionType.trial_subscriber_quota		=> "Trial Subscriber",
			ESubscriptionType.yearly_subscriber_quota		=> "Yearly Subscriber",
			ESubscriptionType.copilot_enterprise_seat_quota	=> "Copilot Enterprise",
			ESubscriptionType.copilot_for_business_seat_quota => "Copilot for Business",
			ESubscriptionType.Unknown						=> "Unknown",
			_ => $"[{_basicCopilotUserInfo?.SubscriptionType}]"
		};

		//--- basic info ------------------------------------------------------
		_ = _basicCopilotUserInfo is null
			? sb.AppendLine("Basic user information is not available.")
			: sb
				.AppendLine("𝗕𝗮𝘀𝗶𝗰 𝗜𝗻𝗳𝗼𝗿𝗺𝗮𝘁𝗶𝗼𝗻")
				.AppendLine($"   Status:			{_basicCopilotUserInfo.Status}")
				.AppendLine($"   User:			{_basicCopilotUserInfo.Username}")
				.AppendLine($"   Subscription:		{subscriptionType}")
				.AppendLine($"   Account type:		{string.Join(", ", EnumerateAccountTypes())}");

		//--- Quota details ---------------------------------------------------
		if (_personalQuota is not null)
		{
			_ = sb
				.AppendLine()
				.AppendLine("𝗙𝗲𝗮𝘁𝘂𝗿𝗲𝘀 𝗘𝗻𝗮𝗯𝗹𝗲𝗱")
				.AppendLine($"   Chat:			{FormatBool(_personalQuota.ChatEnabled)}")
				.AppendLine($"   Editor preview:		{FormatBool(_personalQuota.EditorPreviewFeaturesEnabled)}")
				.AppendLine($"   CLI:			{FormatBool(_personalQuota.CliEnabled)}")
				.AppendLine($"   CLI Remote Control:	{FormatBool(_personalQuota.CliRemoteControlEnabled)}")
				.AppendLine($"   Cloud Session Storage:	{FormatBool(_personalQuota.CloudSessionStorageEnabled)}")
				.AppendLine($"   Copilot Ignore:		{FormatBool(_personalQuota.CopilotIgnoreEnabled)}")
				.AppendLine($"   MCP:			{FormatBool(_personalQuota.IsMcpEnabled)}");

			_ = sb
				.AppendLine()
				.Append("𝗣𝗲𝗿𝘀𝗼𝗻𝗮𝗹 𝗠𝗲𝘁𝗿𝗶𝗰𝘀");

			//--- premium quota ---------------------------
			_ = _personalQuota.QuotaSnapshots?.PremiumInteractions is not null
				? sb.Append(GetQuotaDetailToolTip("   Premium Interactions", _personalQuota.QuotaSnapshots.PremiumInteractions))
				: sb.AppendLine($"   Premium Interactions: [No data]");

			//--- Chat Interactions -----------------------
			_ = _personalQuota.QuotaSnapshots?.Chat is not null
				? sb.Append(GetQuotaDetailToolTip("   Chat Interactions", _personalQuota.QuotaSnapshots.Chat))
				: sb.AppendLine($"   Chat Interactions: [No data]");

			//--- Completions -----------------------------
			_ = _personalQuota.QuotaSnapshots?.Completions is not null
				? sb.Append(GetQuotaDetailToolTip("   Completions", _personalQuota.QuotaSnapshots.Completions))
				: sb.AppendLine($"   Completions: [No data]");
		}
		else
			_ = sb.AppendLine("Personal quota and usage information is not available.");

		//--- API rate limit ---------------------------------------------------
		_ = _apiRateLimit is null
			? sb.AppendLine("GitHub API rate limit information is not available.")
			 : sb
				.AppendLine()
				.AppendLine("𝗚𝗶𝘁𝗛𝘂𝗯 𝗔𝗣𝗜 𝗥𝗮𝘁𝗲 𝗟𝗶𝗺𝗶𝘁")
				.AppendLine($"   Limit:			{_apiRateLimit.Limit}")
				.AppendLine($"   Remaining:		{_apiRateLimit.Remaining}")
				.AppendLine($"   Reset At:		{_apiRateLimit.Reset}");

		//--- return result ---------------------------------------------------
		StatusToolTip = sb.ToString();
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
